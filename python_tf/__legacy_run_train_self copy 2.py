'''
네트워크에 대한 학습이 다 되고 나서 mcts를 이용하여 결과가 잘 나오는지 살핌
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

import asyncio

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
    states = value_lib.PerReplica(states[:half], states[half:])
    pos = value_lib.PerReplica(pos[:half], pos[half:])
    value = value_lib.PerReplica(value[:half], value[half:])
    loss = train_step((states, (pos, value)), full)
    return tuple([rep.values[0].numpy() + rep.values[1].numpy() for rep in loss])



lock_play = asyncio.Lock()
states_list = []
states_owners = []
episode_ans = [asyncio.Event() for _ in range(args.self_play_episodes)]
episode_result = []

lock_train = asyncio.Lock()
train_samples = []


#for records
winning_sheeet = np.zeros((4, 4))



async def task_play_episode(episode_index):
    async def model_fn(states, run_now = False):
        episode_ans[episode_index].clear()
        async with lock_play:
            states_list.append(states)
            states_owners.append(episode_index)
        await episode_ans[episode_index].wait()

    def acq_fn():
        return episode_result[episode_index]

    my = random.randint(0, 3)
    yo = random.randint(0, 3)
    board = game.get_init_board(my, yo)
    episode = mcts.Mcts(board, model_fn, acq_fn, [my, yo])
    await episode.async_init()

    while True:
        while True:
            await episode.async_travel_once()
            if episode.travel_count > args.mcts_max_travel_count:
                episode.move()
                if episode.root.finish != None:
                    break

        epi_samples = episode.get_samples()
        async with lock_train:
            train_samples.extend(epi_samples)

        my = random.randint(0, 3)
        yo = random.randint(0, 3)
        board = game.get_init_board(my, yo)
        episode = mcts.Mcts(board, model_fn, acq_fn, [my, yo])
        await episode.async_init()

async def task_model():
    last_play_time = time.time()
    while True:
        do_something = False
        async with lock_play:
            total_states = sum([len(states) for states in states_list])
        print('[TASK model] total_states', total_states)
        if total_states > args.self_play_min_batch or (time.time() - last_play_time > 0.5  and total_states > 0):
            print('[TASK model] play start')
            do_something = True
            async with lock_play:
                merged = []
                for states in states_list:
                    merged.extend(states)
                merged = np.array(merged)
                merged_pos, merged_value = split_run(merged)
                cum_index = 0
                for i in range(len(states_list)):
                    count = len(states_list[i])
                    pos = merged_pos[cum_index:cum_index+count]
                    value = merged_value[cum_index:cum_index+count]
                    epi_index = states_owners[i]
                    episode_result[epi_index] = pos, value
                    episode_ans[epi_index].set()
                states_list.clear()
                states_owners.clear()
            last_play_time = time.time()
            print('[TASK model] predict... completed.')

        async with lock_train:
            total_samples = len(train_samples)
        print('[TASK model] total_samples', total_samples)
        if len(train_samples) > args.batch_size:
            do_something = True
            async with lock_train:
                merged_states = []
                merged_pos = []
                merged_value = []
                for state, pos, value in train_samples:
                    merged_states.append(state)
                    merged_pos.append(pos)
                    merged_value.append(value)
                train_samples.clear()
            states = np.array(merged_states, dtype=np.float32).reshape(-1, 10, 9, 16)
            pos = np.array(merged_pos, dtype=np.int32).reshape(-1, 1)
            value = np.array(merged_value, dtype=np.float32).reshape(-1, 1)
            loss = split_train(states, pos, value)
            print('                                            [LOSS]', loss)
        
        if not do_something:
            print('???')
            await asyncio.sleep(0.001)
            print('why ???')
        

    


async def main():
    #초기값 설정
    global lock_play
    global states_list
    global states_owners
    global episode_ans
    global episode_result
    lock_play = asyncio.Lock()
    states_list = []
    states_owners = []
    episode_ans = [asyncio.Event() for _ in range(args.self_play_episodes)]
    episode_result = [None for _ in range(args.self_play_episodes)]

    lock_train = asyncio.Lock()
    train_samples = []

    loop = asyncio.get_event_loop()
    

    ###
    #테스크 생성
    tasks = []
    for i in range(args.self_play_episodes):
        # task = asyncio.create_task(task_play_episode(i))
        # tasks.append(task)
        future = asyncio.run_coroutine_threadsafe(task_play_episode(i), loop)
        tasks.append(future)
        
    future = asyncio.run_coroutine_threadsafe(task_model(), loop)
    tasks.append(future)
    # task = asyncio.create_task(task_moodel())
    # tasks.append(task)

    # for task in tasks:
    #     await task

    for task in tasks:
        result = task.result()


asyncio.run(main())



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