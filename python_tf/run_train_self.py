'''
MCTS를 이용한 자가 학습
멀티프로세싱 기반
'''

import tensorflow as tf
from tensorflow import keras
import keras_network
import game
from params import args
import os
import time
import numpy as np

import time
import random

import mcts
from tensorflow.python.distribute import values as value_lib

import multiprocessing

###
#공유 오브젝트

manager = multiprocessing.Manager()
class Shared():
    pass
shared = Shared()


#에피소드 플레이어마다 MCTS를 진행
#진행 중 p, v 값이 필요하면 states_list, states_owners에 요청사항을 등록하고 기다림
#states_list = 요청하는 입력 states=list of (10, 9, 16)
#states_owners = 요청한 플레이어의 번호
#episode_ans = 요청 결과를 기다리기 위한 이벤트
#episode_result = 요청 결과가 들어있는 리스트 (결과 리턴)
#lock_play = 이상 내용의 critical section 접근 통제
shared.states_list = manager.list()
shared.states_owners = manager.list()
shared.episode_ans = manager.list([manager.Event() for _ in range(args.self_play_episodes)])
shared.episode_result = manager.list([None for _ in range(args.self_play_episodes)])
shared.lock_play = manager.Lock()

#플레이어가 하나의 에피소드를 끝내면 플레이 결과를 학습해야 한다.
#학습 내용을 train_sample에 담는다.
shared.lock_train = manager.Lock() #접근 통제
shared.train_samples = manager.list()

###
# 에피소드 플레이어

def task_play_episode(sh, episode_index):
    def model_fn(states, run_now = False):
        sh.episode_ans[episode_index].clear()
        with sh.lock_play:
            sh.states_list.append(states)
            sh.states_owners.append(episode_index)
        sh.episode_ans[episode_index].wait()

    def acq_fn():
        return sh.episode_result[episode_index]

    my = random.randint(0, 3)
    yo = random.randint(0, 3)
    board = game.get_init_board(my, yo)
    episode = mcts.Mcts(board, model_fn, acq_fn, [my, yo])
    episode.init()

    while True:
        while True:
            episode.travel_once()
            if episode.travel_count >= args.mcts_max_travel_count:
                episode.move()
                if episode.root.finish != None:
                    break

        epi_samples = episode.get_samples()
        with sh.lock_train:
            sh.train_samples.extend(epi_samples)

        my = random.randint(0, 3)
        yo = random.randint(0, 3)
        board = game.get_init_board(my, yo)
        episode = mcts.Mcts(board, model_fn, acq_fn, [my, yo])
        episode.init()

child_procs = []
for i in range(args.self_play_episodes):
    p = multiprocessing.Process(target=task_play_episode, args=(shared, i))
    child_procs.append(p)

for p in child_procs:
    p.start()


###
# 네트워크 실행 및 학습

model_name = 'dualres%d_%d' % (args.filters, args.n_blocks)

checkpoint = keras.callbacks.ModelCheckpoint('cp/' + model_name + '/cp', save_best_only=False, save_weights_only=True)
tensorboard = keras.callbacks.TensorBoard('logs/' + model_name)

mirrored_strategy = tf.distribute.MirroredStrategy()
with mirrored_strategy.scope():
    p_model, v_model, full_model = keras_network.gen_network(args.filters, args.n_blocks)
    latest = tf.train.latest_checkpoint('cp/' + model_name + '/')
    if latest != None:
        print('load latest model : ', latest)
        full_model.load_weights(latest)
        print('load completed.')
    full_model.compile(loss=('sparse_categorical_crossentropy', 'mse'), optimizer='adam')
    print('compile completed.')

    optimizer = tf.keras.optimizers.Nadam()

@tf.function(experimental_relax_shapes=True)
def run_full_model(x):
    a = mirrored_strategy.run(full_model, args=([x],))
    return a

@tf.function(experimental_relax_shapes=True)
def train_step(dist_inputs, global_batch_size):
    def step_fn(inputs, bs):
        states, (true_pos, true_value) = inputs
        with tf.GradientTape() as tape:
            pred_logits, pred_value = full_model(states)
            cross_entropy = tf.losses.sparse_categorical_crossentropy(y_true=true_pos, y_pred=pred_logits)
            loss_pos = tf.reduce_sum(cross_entropy) * (1.0 / bs)
            mse = tf.losses.mse(y_true=true_value, y_pred=pred_value)
            loss_value = tf.reduce_sum(mse) * (1.0 / bs)
            loss = loss_pos + loss_value
            
        grads = tape.gradient(loss, full_model.trainable_variables)
        optimizer.apply_gradients(list(zip(grads, full_model.trainable_variables)))
        return (loss_pos, loss_value)

    loss_pos, loss_value = mirrored_strategy.run(step_fn, args=(dist_inputs, global_batch_size))
    # loss_pos = mirrored_strategy.reduce(tf.distribute.ReduceOp.MEAN, loss_pos, axis=0)
    # loss_value = mirrored_strategy.reduce(tf.distribute.ReduceOp.MEAN, loss_value, axis=0)
    return loss_pos, loss_value

def split_run(states):
    if len(states) == 1:
        reps = run_full_model(states)
        return tuple([rep.values[0].numpy() for rep in reps])
    else:
        full = len(states)
        half = full // 2
        x = value_lib.PerReplica((states[:half], states[half:]))
        reps = run_full_model(x)
        return tuple([np.concatenate([rep.values[0].numpy(), rep.values[1].numpy()], axis=0) for rep in reps])


def split_train(states, pos, value):
    full = len(states)
    half = full // 2
    states = value_lib.PerReplica((states[:half], states[half:]))
    pos = value_lib.PerReplica((pos[:half], pos[half:]))
    value = value_lib.PerReplica((value[:half], value[half:]))
    loss = train_step((states, (pos, value)), full)
    return tuple([rep.values[0].numpy() + rep.values[1].numpy() for rep in loss])



#for records
winning_sheeet = np.zeros((4, 4))


def task_model():
    last_play_time = time.time()
    while True:
        do_something = False
        with shared.lock_play:
            total_states = sum([len(states) for states in shared.states_list])
        # print('[TASK model] total_states', total_states)
        if total_states > args.self_play_min_batch or (time.time() - last_play_time > 0.5  and total_states > 0):
            print('[TASK model] play start', len(shared.states_list), total_states)
            do_something = True
            with shared.lock_play:
                merged = []
                for states in shared.states_list:
                    merged.extend(states)
                merged = np.array(merged)
                merged_pos, merged_value = split_run(merged)
                cum_index = 0
                for i in range(len(shared.states_list)):
                    count = len(shared.states_list[i])
                    pos = merged_pos[cum_index:cum_index+count]
                    value = merged_value[cum_index:cum_index+count]
                    epi_index = shared.states_owners[i]
                    shared.episode_result[epi_index] = pos, value
                    shared.episode_ans[epi_index].set()
                shared.states_list[:] = [] # clear()
                shared.states_owners[:] = [] #clear()
            last_play_time = time.time()
            # print('[TASK model] predict... completed.')

        with shared.lock_train:
            total_samples = len(shared.train_samples)
        if len(shared.train_samples) > args.batch_size:
            print('[TASK model] total_samples', total_samples)
            do_something = True
            with shared.lock_train:
                merged_states = []
                merged_pos = []
                merged_value = []
                for state, pos, value in shared.train_samples:
                    merged_states.append(state)
                    merged_pos.append(pos)
                    merged_value.append(value)
                shared.train_samples[:] = []
            states = np.array(merged_states, dtype=np.float32).reshape(-1, 10, 9, 16)
            pos = np.array(merged_pos, dtype=np.int32).reshape(-1, 1)
            value = np.array(merged_value, dtype=np.float32).reshape(-1, 1)
            loss = split_train(states, pos, value)
            print('                                            [LOSS]', loss)
        
        if not do_something:
            print('???')
            time.sleep(0.1)
            print('why ???')
        
#메인 프로세스에서 그냥 실행
task_model()






    # async def task_solver():
#     for lock in req_locks:
#         await lock.wait()
#     # print('all data received...')

#     r = value_lib.PerReplica([req[:args.batch_size], req[args.batch_size:]])
#     a = fn(r)
#     ans_policy = np.concatenate([a[0].values[0].numpy(), a[0].values[1].numpy()])
#     ans_value = np.concatenate([a[1].values[0].numpy(), a[1].values[1].numpy()])

#     # print('result is now published!')
#     ans_lock.set()

# async def task_request(n, episode):
#     episode.travel_once()
#     if eipsode.travel_count > 300:
#         episode.move()
#         if episode.root.finish != None:
            
    

# def gen():
#     req = []
#     def model_fn(state):
#         req.append(state)
#         if len(req):
#             pass
    
#     for episode in episodes:
#         episode.travel_once()