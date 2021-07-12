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

@tf.function
def fn(x):
    a = mirrored_strategy.run(full_model, args=([x],))
    return a

state1 = np.zeros((1, 10, 9, 16))
state2 = np.ones((2, 10, 9, 16))
state = value_lib.PerReplica((state1, state2))
pos1 = np.array([[1]])
pos2 = np.array([[2, 3]])
pos = value_lib.PerReplica((pos1, pos2))
value1 = np.array([[1.]], dtype=np.float32)
value2 = np.array([[1.], [1.]], dtype=np.float32)
value = value_lib.PerReplica((value1, value2))

np_state = np.zeros((128, 10, 9, 16), dtype=np.float32)
np_pos = np.zeros((128, 1), dtype=np.int32)
np_value = np.zeros((128, 1), dtype=np.float32)
train_dataset = tf.data.Dataset.from_tensor_slices((np_state, (np_pos, np_value))).batch(64) 
train_dist_dataset = mirrored_strategy.experimental_distribute_dataset(train_dataset)

#012345678#012345678#012345678#012345678#012345678#012345678#012345678#012345678#012345678#012345678#012345678#012345678

@tf.function
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

total_loss = 0
for x in train_dist_dataset:
    # print(x)
    res = train_step(x, 64)
    print(res)




a = train_step((state, (pos, value)), 3)

x = fn(state)

dataset = tf.data.Dataset.from_tensor_slices(state).batch(16)
dist_ds = mirrored_strategy.experimental_distribute_dataset(dataset)
x = [dist_ds.take(1)]


half = 16
# rep = value_lib.PerReplica(state[:half], state[half:])
result = fn(state)
a = result[0].values[0]
b = result[0].values[1]
# pos, value = full_model.predict(rep)
# print(pos, value)
print(result)