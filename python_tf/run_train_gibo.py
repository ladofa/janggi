import tensorflow as tf
from tensorflow import keras
import keras_network
import game
import gibo
from params import args
import os
import numpy as np

import time
import random

model_name = 'dualres%d_%d' % (args.filters, args.n_blocks)

dataset_train = tf.data.TFRecordDataset(args.record_train_path, num_parallel_reads=tf.data.experimental.AUTOTUNE).cache().repeat()
dataset_val = tf.data.TFRecordDataset(args.record_val_path, num_parallel_reads=tf.data.experimental.AUTOTUNE).cache()
dataset_train = dataset_train.map(gibo.read_tfrecord, num_parallel_calls=tf.data.experimental.AUTOTUNE)
dataset_val = dataset_val.map(gibo.read_tfrecord, num_parallel_calls=tf.data.experimental.AUTOTUNE)

dataset_train = dataset_train.shuffle(300000).batch(args.batch_size, drop_remainder=True).prefetch(2)
dataset_val = dataset_val.batch(args.batch_size, drop_remainder=True).prefetch(2)

checkpoint = keras.callbacks.ModelCheckpoint('cp/' + model_name + '/cp', save_best_only=False, save_weights_only=True)
tensorboard = keras.callbacks.TensorBoard('logs_gibo/' + model_name)

mirrored_strategy = tf.distribute.MirroredStrategy()
with mirrored_strategy.scope():
    p_model, v_model, full_model = keras_network.gen_network(args.filters, args.n_blocks)
    optimizer = tf.optimizers.Nadam(0.01)
    full_model.compile(loss=('sparse_categorical_crossentropy', 'mse'), optimizer=optimizer)

    latest = tf.train.latest_checkpoint('cp/' + model_name + '/')
    if latest != None:
        full_model.load_weights(latest)

    dataset_train = mirrored_strategy.experimental_distribute_dataset(dataset_train)
    # dataset_val = mirrored_strategy.experimental_distribute_dataset(dataset_val)

    full_model.fit(dataset_train, epochs=100, callbacks=[checkpoint, tensorboard], validation_data=dataset_val, steps_per_epoch=4600)
    full_model.save_weights('saved/' + model_name + '/gibo')