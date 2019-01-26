import os
import tensorflow as tf
import numpy as np

import policy_networks
import value_networks

from _params import args

input_channels = 31

sess = tf.Session()
policy_tensors = {}
value_tensors = {}

def load_policy():
	with tf.variable_scope('policy'):
		input_board = tf.placeholder(tf.uint8, [None, 10, 9, 31], 'input_board')
		input_move = tf.placeholder(tf.uint8, [None, 10, 9, 2], 'input_move')
				
		with tf.variable_scope('net'):
			move = policy_networks.get_network(args.model_policy, input_board)

		#calc loss
		input_move_flat = tf.reshape(input_move, [-1, 90, 2])
		input_move_float = tf.cast(input_move_flat, dtype=tf.float32)
		move_float = tf.reshape(move, [-1, 90, 2])


		input_from = input_move_float[:, :, 0]
		input_to = input_move_float[:, :, 0]

		move_from = move_float[:, :, 0]
		move_to = move_float[:, :, 0]

		lossFrom = tf.losses.softmax_cross_entropy(input_from, move_from)
		lossTo = tf.losses.softmax_cross_entropy(input_to, move_to)
		loss = (lossFrom + lossTo) / 2

		#optimize
		gs = tf.Variable(0, trainable=False)
		opt = tf.train.AdamOptimizer(learning_rate=0.001)
		train_op = opt.minimize(loss, global_step=gs)

		softmax_from = tf.nn.softmax(move_from)
		softmax_to = tf.nn.softmax(move_to)

		softmax_from = tf.reshape(softmax_from, [-1, 10, 9])
		softmax_to = tf.reshape(softmax_to, [-1, 10, 9])
	
	all_vars = tf.global_variables('policy')
	save_vars = tf.trainable_variables('policy/net') + [gs]
	init_vars = [var for var in all_vars if var not in save_vars]

	latest_checkpoint = tf.train.latest_checkpoint('training/' + args.model_policy + '/policy')
	if latest_checkpoint:
		saver = tf.train.Saver(save_vars)
		saver.restore(sess, latest_checkpoint)
		init_op = tf.initializers.variables(init_vars)
	else:
		#init all
		init_op = tf.initializers.variables(all_vars)

	sess.run(init_op)


	_sum_loss = tf.placeholder(tf.float32, [])
	sum_loss = tf.summary.scalar('loss', _sum_loss)
	file_writer = tf.summary.FileWriter('logs/policy/' + args.model_policy)

	policy_tensors['input_board'] = input_board
	policy_tensors['input_move'] = input_move
	policy_tensors['move_from'] = softmax_from
	policy_tensors['move_to'] = softmax_to
	policy_tensors['train_op'] = train_op
	policy_tensors['save_vars'] = save_vars
	policy_tensors['loss'] = loss
	policy_tensors['file_writer'] = file_writer
	policy_tensors['_sum_loss'] = _sum_loss
	policy_tensors['sum_loss'] = sum_loss
	policy_tensors['gs'] = gs

def train_policy(data):
	if len(policy_tensors.keys()) == 0:
		load_policy()

	data_board = data['board']
	data_move = data['move']
	data_size = len(data_board)

	if (data_size == 0): return

	batch_size = 8

	input_board = policy_tensors['input_board']
	input_move = policy_tensors['input_move']
	move_from = policy_tensors['move_from']
	move_to = policy_tensors['move_to']
	train_op = policy_tensors['train_op']

	gs = policy_tensors['gs']
	loss = policy_tensors['loss']
	sum_loss = policy_tensors['sum_loss']
	file_writer = policy_tensors['file_writer']

	file_writer = policy_tensors['file_writer']
	sum_loss = policy_tensors['sum_loss']
	_sum_loss = policy_tensors['_sum_loss']

	losses = []

	for i in range(0, data_size, batch_size):
		chunk_board = data_board[i : i + batch_size]
		chunk_move = data_move[i : i + batch_size]

		_, ev_move_from, ev_move_to, ev_gs, ev_loss = sess.run([train_op, move_from, move_to, gs, loss], feed_dict={input_board:chunk_board, input_move:chunk_move})

		losses.append(ev_loss)
	loss_avr = sum(losses) / len(losses)

	summary = sess.run(sum_loss, feed_dict={_sum_loss:loss_avr})
	file_writer.add_summary(summary, ev_gs)

	return loss_avr, ev_gs, ev_move_from, ev_move_to

def eval_policy():
	pass

def save_policy():
	save_vars = policy_tensors['save_vars']
	gs = policy_tensors['gs']
	ev_gs = sess.run(gs)
	saver = tf.train.Saver(save_vars)
	saver.save(sess,'training/' + args.model_policy + '/policy', ev_gs)


def load_value():
	with tf.variable_scope('value'):
		input_board = tf.placeholder(tf.uint8, [None, 10, 9, 31], 'input_board')
		input_judge = tf.placeholder(tf.float32, [None, 1], 'input_judge')
				
		with tf.variable_scope('net'):
			judge = value_networks.get_network(args.model_policy, input_board)

		#calc loss
		input_judge_ops = 1 - input_judge
		input_judge2 = tf.concat([input_judge, input_judge_ops], 1)
		loss = tf.losses.softmax_cross_entropy(input_judge2, judge)

		#optimize
		gs = tf.Variable(0, trainable=False)
		opt = tf.train.AdamOptimizer(learning_rate=0.001)
		train_op = opt.minimize(loss, global_step=gs)

		softmax = tf.nn.softmax(judge)
	
	all_vars = tf.global_variables('value')
	save_vars = tf.trainable_variables('value/net') + [gs]
	init_vars = [var for var in all_vars if var not in save_vars]

	latest_checkpoint = tf.train.latest_checkpoint('training/' + args.model_value + '/value')
	if latest_checkpoint:
		saver = tf.train.Saver(save_vars)
		saver.restore(sess, latest_checkpoint)
		init_op = tf.initializers.variables(init_vars)
	else:
		#init all
		init_op = tf.initializers.variables(all_vars)

	sess.run(init_op)


	_sum_loss = tf.placeholder(tf.float32, [])
	sum_loss = tf.summary.scalar('loss', _sum_loss)
	file_writer = tf.summary.FileWriter('logs/value/' + args.model_value)

	value_tensors['input_board'] = input_board
	value_tensors['input_judge'] = input_judge
	value_tensors['judge'] = softmax[:, 0]
	value_tensors['train_op'] = train_op
	value_tensors['save_vars'] = save_vars
	value_tensors['loss'] = loss
	value_tensors['file_writer'] = file_writer
	value_tensors['_sum_loss'] = _sum_loss
	value_tensors['sum_loss'] = sum_loss
	value_tensors['gs'] = gs

def train_value(data):
	if len(value_tensors.keys()) == 0:
		load_value()

	data_board = data['board']
	data_judge = data['judge']
	data_size = len(data_board)
	if data_size == 0:
		return
	batch_size = 8

	input_board = value_tensors['input_board']
	input_judge = value_tensors['input_judge']
	judge = value_tensors['judge']
	train_op = value_tensors['train_op']

	gs = value_tensors['gs']
	loss = value_tensors['loss']
	sum_loss = value_tensors['sum_loss']
	file_writer = value_tensors['file_writer']

	file_writer = value_tensors['file_writer']
	sum_loss = value_tensors['sum_loss']
	_sum_loss = value_tensors['_sum_loss']

	losses = []

	for i in range(0, data_size, batch_size):
		chunk_board = data_board[i : i + batch_size]
		chunk_judge = data_judge[i : i + batch_size]

		_, ev_judge, ev_gs, ev_loss = sess.run([train_op, judge, gs, loss], feed_dict={input_board:chunk_board, input_judge:chunk_judge})

		losses.append(ev_loss)
	loss_avr = sum(losses) / len(losses)

	summary = sess.run(sum_loss, feed_dict={_sum_loss:loss_avr})
	file_writer.add_summary(summary, ev_gs)

	return loss_avr, ev_gs, ev_judge

def eval_value():
	pass

def save_value():
	save_vars = value_tensors['save_vars']
	gs = value_tensors['gs']
	ev_gs = sess.run(gs)
	saver = tf.train.Saver(save_vars)
	saver.save(sess, 'training/' + args.model_value + '/value', ev_gs)
