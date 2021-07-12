'''
네트워크에 대한 학습이 다 되고 나서 mcts를 이용하여 결과가 잘 나오는지 살핌
'''

import tensorflow as tf
from tensorflow import keras
import keras_network
import game
from params import args
import os
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


samples = []

lock_sample = asyncio.Lock()
lock_model = asyncio.Lock()
lock_req = asyncio.Lock()

play_batch = args.self_play_batch
play_chunk_count = args.self_play_chunk_count
buf_size = play_batch * play_chunk_count

event_req = [asyncio.Event() for _ in range(play_chunk_count)]
event_ans = [asyncio.Event() for _ in range(play_chunk_count)]

mcts_search_max = args.mcts_search_max
mcts_parallel = args.mcts_parallel
mcts_simul = args.mcts_simul


req = [None for _ in range(buf_size)]

ans_policy = np.zeros([buf_size, 8101], dtype=np.float32)
ans_value = np.zeros([buf_size, 1], dtype=np.float32)
req_count = 0




@tf.function
def fn(x):
    a = mirrored_strategy.run(full_model, args=([x],))
    return a

async def async_model_fn(state):
    global req_count
    async with lock_req:
        req[req_count] = state
        req_index = req_count
        req_count += 1
        if req_count >= buf_size:
            req_count = 0

    chunk_index = 0
    for i in range(play_chunk_count):
        upper = (i + 1) * play_batch - 1
        if req_index <= upper:
            chunk_index = i
            if req_index == (i + 1) * play_batch - 1:
                event_req[i].set()
            break
        
    return ans_policy[req_index], ans_value[req_index]

def model_fn(state):
    asyncio.run(async_model_fn(state))

mcts.Node.set_model(model_fn)

async def task_play_model(chunk_index):
    while True:
        await event_req[chunk_index].wait()
        i0 = chunk_index * play_batch
        i1 = chunk_index * (play_batch + 1)
        half = play_batch // 2

        data = req[i0 : i1]
        rep = value_lib.PerReplica(data[:half], data[half:])

        async with lock_model:
            a = fn(rep)

        ans_policy[i0 : i0+half] = a[0].values[0].numpy()
        ans_policy[i0+half : i1] = a[0].values[1].numpy()
        ans_value[i0 : i0+half] = a[0].values[0].numpy()
        ans_value[i0+half : i1] = a[0].values[1].numpy()
        event_ans[chunk_index].set()
        event_ans[chunk_index].clear()


async def task_play_episode_search(episode):
    while True:
        print('episode_search')
        episode.async_travel_once()
        if episode.travel_count > mcts_search_max:
            break
        
async def task_play_episode(episode_index):
    board = game.get_init_board(random.randint(0, 3), random.randint(0, 3))
    episode = mcts.Mcts(board)

    while True:
        print('task_play_episode')
        tasks = []
        for _ in range(mcts_simul):
            task = asyncio.create_task(task_play_episode_search(episode))
            tasks.append(task)
        
        for task in tasks:
            await task
        episode.move()
        if episode.root.finish != None:
            epi_samples = episode.get_samples()
            async with lock_sample:
                samples.extend(epi_samples)

            episode = mcts.Mcts(get_random_board())


chunk = 10

async def task_train_model():
    print('task_train_model')
    while True:
        db_slice = None
        async with lock_sample:
            if len(samples) > args.batch_size * chunk:
                db_slice = samples[:args.batch_size * chunk]
                samples = samples[args.batch_size * chunk]
        if db_slice:
            for i in range(chunk):
                batch = db_slice[i * chunk : (i + 1) * chunk]
                async with lock_model:
                    res = full_model.train_on_batch(batch)
                    print(res)
        
async def main():
    global lock_sample
    global lock_model
    global lock_req
    global event_req
    global event_ans
    lock_sample = asyncio.Lock()
    lock_model = asyncio.Lock()
    lock_req = asyncio.Lock()
    event_req = [asyncio.Event() for _ in range(play_chunk_count)]
    event_ans = [asyncio.Event() for _ in range(play_chunk_count)]

    tasks = []
    for i in range(play_chunk_count):
        task = asyncio.create_task(task_play_model(i))
        tasks.append(task)

    for i in range(mcts_parallel):
        task = asyncio.create_task(task_play_episode(i))
        tasks.append(task)
    
    task = asyncio.create_task(task_train_model())
    tasks.append(task)

    for task in tasks:
        await task


asyncio.run(main())

def gen_model_fn(i):
    def model_fn():
        reuturn i#??
    return model_fn

async def task_episode():
    

samples = []
episodes = []
for i in range(32):
    my = random.randint(0, 3)
    yo = random.randint(0, 3)
    board = game.get_init_board(my, yo)
    episode = mcts.Mcts(board, gen_model_fn(), [my, yo])
    episodes.append(episode)
    

    
while True:
    #샘플을 생성한다.
    



    


    #생성된 샘플을 학습하여 소비한다.





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