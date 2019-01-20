import os
import tensorflow as tf

import policy_networks
import value_networks

input_channels = 31

sess = tf.Session()
policy_tensors = {}
value_tensors = {}

def load_policy(name):
	with tf.name_scope('policy'):
		input_board = tf.placeholder(tf.uint8, [None, 10, 9, 31], 'input_board')
		input_move = tf.placeholder(tf.uint8, [None, 10, 9, 2], 'input_move')
				
		with tf.name_scope('net'):
			logits = policy_networks.get_network(name, input_board)

		#calc loss
		input_move_flat = tf.reshape(input_move, [-1, 90, 2])
		input_move_float = tf.cast(input_move_float, dtype=tf.float32)
		logits_float = tf.reshape(logits, [-1, 90, 2])
		loss = tf.losses.softmax_cross_entropy(input_move_flat[0], logits_float[0])

		#optimize
		gs = tf.Variable(0, trainable=False)
		opt = tf.train.AdamOptimizer(learning_rate=0.001)
		train_op = opt.minimize(loss, global_step=gs)
	
	all_vars = tsf.global_variables('policy')
	save_vars = tf.trainable_variables('policy/net') + [gs]
	init_vars = [var for var in all_var if var not in save_vars]

	latest_checkpoint = tf.train.latest_checkpoint('training/' + name)
	if latest_checkpoint:
		saver = tf.train.Saver(save_vars)
		saver.restore(sess, latest_checkpoint)
		init_op = tf.initialize_variables(init_vars)
	else:
		#init all
		init_op = tf.initialize_variables(all_vars)

	sess.run(init_op)


	_sum_loss = tf.placeholder(tf.float32, [])
	sum_loss = tf.summary.scalar('loss', _sum_loss)
	file_writer = tf.summary.FileWriter('logs/' + name)

	policy_tensors['input_board'] = input_board
	policy_tensors['input_move'] = input_move
	policy_tensors['train_op'] = train_op
	policy_tensors['save_vars'] = save_vars
	policy_tensors['loss'] = loss
	policy_tensors['file_writer'] = file_writer
	policy_tensors['_sum_loss'] = _sum_loss
	policy_tensors['sum_loss'] = sum_loss
	policy_tensors['gs'] = gs

def train_policy(name, data):
	data_board = data['board']
	data_move = data['move']
	data_size = len(data_board)
	batch_size = 4

	input_board = policy_tensors['input_board']
	input_move = policy_tensors['input_move']
	train_op = policy_tensors['train_op']
	gs = policy_tensors['gs']
	loss = policy_tensors['loss']
	sum_loss = policy_tensors['sum_loss']
	file_writer = policy_tensors['file_writer']

	file_writer = policy_networks['file_writer']
	sum_loss = policy_networks['sum_loss']
	_sum_loss = policy_networks['_sum_loss']

	losses = []

	for i in range(0, data_size, batch_size):
		chunk_board = data_board[i : i + batch_size]
		chunk_move = data_move[i : i + batch_size]

		_, ev_gs, ev_loss = sess.run([train_op, gs, loss], feed_dict={input_board:chunk_board, input_move:chunk_move})

		losses.append(ev_loss)
	loss_avr = sum(losses) / len(losses)

	summary = sess.run(sum_loss, feed_dict={_sum_loss:loss_avr})
	file_writer.add_summary(summary, ev_gs)

	return loss_avr

def save_policy(name):
	save_vars = policy_tensors['save_vars']
	gs = policy_tensors['gs']
	ev_gs = sess.run(gs)
	saver = tf.train.Saver(save_vars)
	saver.save(sess, 'training/name/saved', ev_gs)

def load_value(name):


	   	  
#import os
#os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
#import tensorflow as tf
#import move_transfer




#class Network():
#	def __init__(self):
#		self.graph = tf.Graph()
#		with self.graph.as_default():
#			self.sess = tf.Session()

#	def save(self, new_name):
#		with self.graph.as_default():
#			try:
#				saver = tf.train.Saver()
#				saver.save(self.sess, "./training_data/" + new_name)
#				return True
#			except:
#				return False


#	def load(self, new_name):
#		with self.graph.as_default():
#			try:
#				saver = tf.train.Saver()
#				saver.restore(self.sess, "./training_data/" + new_name)
#				return True
#			except:
#				return False

#class PolicyNetwork(Network):
#	def __init__(self):
#		Network.__init__(self)
#		with self.graph.as_default():
#			x = tf.placeholder(tf.float32, shape=[None, 10, 9, 118], name="x")
#			y_ = tf.placeholder(tf.float32, shape=[None, 2451], name="y_")
#			keep_prob = tf.placeholder(tf.float32, name="keep_prob")

#			f = 192
#			conv1 = conv_net(x, 5, f, 'conv1')
#			conv2 = conv_net(conv1, 3, f, 'conv2')
#			conv3 = conv_net(conv2, 3, f, 'conv3')
#			conv4 = conv_net(conv3, 3, f, 'conv4')
#			conv5 = conv_net(conv4, 3, f, 'conv5')
#			conv6 = conv_net(conv5, 3, f, 'conv6')
			
#			conv12 = conv_net(conv6, 3, f, 'conv12')

#			dim =  (conv12.shape[1] * conv12.shape[2] * conv12.shape[3]).value
#			with tf.name_scope('fc'):
#				fc0 = tf.reshape(conv12, [-1, dim])

#				#fc1 = fc_net(fc0, 4096, 'fc1', 'relu')
#				#fc1_drop = tf.nn.dropout(fc1, keep_prob)

#				self.model = fc_net(fc0, 2451, 'fc2', 'none')

#			with tf.name_scope('output'):
#				self.prom = tf.nn.softmax(self.model)

#			with tf.name_scope('loss'):
#				self.loss = tf.reduce_mean(tf.nn.softmax_cross_entropy_with_logits(labels = y_, logits = self.model))

#			with tf.name_scope('train'):
#				self.train_step = tf.train.AdamOptimizer().minimize(self.loss)
			
#			#self.writer = tf.summary.FileWriter("d:/temp/1")
#			#self.writer.add_graph(self.graph)
#			#self.writer.close()

#			self.sess.run(tf.global_variables_initializer())
			
			
#	def train(self, data):
#		with self.graph.as_default():
#			self.sess.run(self.train_step, feed_dict={"x:0": data[0], "y_:0": data[1], "keep_prob:0":0.5})


#	def evaluate(self, data):
#		with self.graph.as_default():
#			return self.sess.run(self.prom, {"x:0": data, "keep_prob:0":1})

#	def get_loss(self, data):
#		with self.graph.as_default():
#			return self.sess.run(self.loss, feed_dict={"x:0": data[0], "y_:0": data[1], "keep_prob:0":1})

#class ValueNetwork(Network):
#	def __init__(self):
#		Network.__init__(self)
#		with self.graph.as_default():
#			x = tf.placeholder(tf.float32, shape=[None, 10, 9, 118], name="x")
#			y_ = tf.placeholder(tf.float32, shape=[None, 1], name="y_")
#			keep_prob = tf.placeholder(tf.float32, name="keep_prob")

#			f = 192
#			conv1 = conv_net(x, 5, f, 'conv1')
#			conv2 = conv_net(conv1, 3, f, 'conv2')
#			conv3 = conv_net(conv2, 3, f, 'conv3')
#			conv4 = conv_net(conv3, 3, f, 'conv4')
#			conv5 = conv_net(conv4, 3, f, 'conv5')
#			conv6 = conv_net(conv5, 3, f, 'conv6')
			
#			conv12 = conv_net(conv6, 3, f, 'conv12')

#			dim =  (conv12.shape[1] * conv12.shape[2] * conv12.shape[3]).value
#			with tf.name_scope('fc'):
#				fc0 = tf.reshape(conv12, [-1, dim])

#				#fc1 = fc_net(fc0, 4096, 'fc1', 'relu')
#				#fc1_drop = tf.nn.dropout(fc1, keep_prob)

#				self.model = fc_net(fc0, 1, 'fc2', 'none')

#			with tf.name_scope('output'):
#				self.prom = tf.nn.sigmoid(self.model)

#			with tf.name_scope('loss'):
#				self.loss = tf.reduce_mean(tf.nn.sigmoid_cross_entropy_with_logits(labels = y_, logits = self.model))

#			with tf.name_scope('train'):
#				self.train_step = tf.train.AdamOptimizer().minimize(self.loss)
			
#			#self.writer = tf.summary.FileWriter("d:/temp/1")
#			#self.writer.add_graph(self.graph)
#			#self.writer.close()

#			self.sess.run(tf.global_variables_initializer())
			
			
#	def train(self, data):
#		with self.graph.as_default():
#			self.sess.run(self.train_step, feed_dict={"x:0": data[0], "y_:0": data[1], "keep_prob:0":0.5})


#	def evaluate(self, data):
#		with self.graph.as_default():
#			return self.sess.run(self.prom, {"x:0": data, "keep_prob:0":1})

#	def get_loss(self, data):
#		with self.graph.as_default():
#			return self.sess.run(self.loss, feed_dict={"x:0": data[0], "y_:0": data[1], "keep_prob:0":1})
	

#def weight_variable(shape):
#	return tf.Variable(tf.truncated_normal(shape, stddev=0.1))

#def bias_variable(shape):
#	return tf.Variable(tf.constant(0.1, shape=shape))

#def conv2d(x, W):
#	return tf.nn.conv2d(x, W, strides=[1, 1, 1, 1], padding='SAME')

#def max_pool_2x2(x):
#	return tf.nn.max_pool(x, ksize=[1, 2, 2, 1],
#                        strides=[1, 2, 2, 1], padding='SAME')

#def conv_net(x, s, f, name):
#	with tf.name_scope(name):
#		d1 = x.shape[3].value
#		w = weight_variable([s, s, d1, f])
#		b = bias_variable([f])
#		conv = tf.nn.relu(conv2d(x, w) + b)
#		return conv

#def fc_net(x, out_dim, name, func = 'relu'):
#	with tf.name_scope(name):
#		in_dim = x.shape[1].value;
#		w = weight_variable([in_dim, out_dim])
#		b = bias_variable([out_dim])
#		if func == 'relu':
#			return tf.nn.relu(tf.matmul(x, w) + b)
#		elif func == 'sigmoid':
#			return tf.nn.sigmoid(tf.matmul(x, w) + b)
#		elif func == 'softmax':
#			return tf.nn.softmax(tf.matmul(x, w) + b)
#		elif func == 'none' :
#			return (tf.matmul(x, w) + b)




