
import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
import tensorflow as tf
import move_transfer


#from tensorflow.examples.tutorials.mnist import input_data
#mnist = input_data.read_data_sets('MNIST_data', one_hot=True)




class Network():
	def __init__(self):
		self.graph = tf.Graph()
		with self.graph.as_default():
			self.sess = tf.Session()

	def save(self, new_name):
		with self.graph.as_default():
			try:
				saver = tf.train.Saver()
				saver.save(self.sess, "./training_data/" + new_name)
				return True
			except:
				return False


	def load(self, new_name):
		with self.graph.as_default():
			try:
				saver = tf.train.Saver()
				saver.restore(self.sess, "./training_data/" + new_name)
				return True
			except:
				return False

class PolicyNetwork(Network):
	def __init__(self):
		Network.__init__(self)
		with self.graph.as_default():
			x = tf.placeholder(tf.float32, shape=[None, 10, 9, 118], name="x")
			y_ = tf.placeholder(tf.float32, shape=[None, 2451], name="y_")
			keep_prob = tf.placeholder(tf.float32, name="keep_prob")

			f = 192
			conv1 = conv_net(x, 5, f, 'conv1')
			conv2 = conv_net(conv1, 3, f, 'conv2')
			conv3 = conv_net(conv2, 3, f, 'conv3')
			conv4 = conv_net(conv3, 3, f, 'conv4')
			conv5 = conv_net(conv4, 3, f, 'conv5')
			conv6 = conv_net(conv5, 3, f, 'conv6')
			
			conv12 = conv_net(conv6, 3, f, 'conv12')

			dim =  (conv12.shape[1] * conv12.shape[2] * conv12.shape[3]).value
			with tf.name_scope('fc'):
				fc0 = tf.reshape(conv12, [-1, dim])

				#fc1 = fc_net(fc0, 4096, 'fc1', 'relu')
				#fc1_drop = tf.nn.dropout(fc1, keep_prob)

				self.model = fc_net(fc0, 2451, 'fc2', 'none')

			with tf.name_scope('output'):
				self.prom = tf.nn.softmax(self.model)

			with tf.name_scope('loss'):
				self.loss = tf.reduce_mean(tf.nn.softmax_cross_entropy_with_logits(labels = y_, logits = self.model))

			with tf.name_scope('train'):
				self.train_step = tf.train.AdamOptimizer().minimize(self.loss)
			
			#self.writer = tf.summary.FileWriter("d:/temp/1")
			#self.writer.add_graph(self.graph)
			#self.writer.close()

			self.sess.run(tf.global_variables_initializer())
			
			
	def train(self, data):
		with self.graph.as_default():
			self.sess.run(self.train_step, feed_dict={"x:0": data[0], "y_:0": data[1], "keep_prob:0":0.5})


	def evaluate(self, data):
		with self.graph.as_default():
			return self.sess.run(self.prom, {"x:0": data, "keep_prob:0":1})

	def get_loss(self, data):
		with self.graph.as_default():
			return self.sess.run(self.loss, feed_dict={"x:0": data[0], "y_:0": data[1], "keep_prob:0":1})

class ValueNetwork(Network):
	def __init__(self):
		Network.__init__(self)
		with self.graph.as_default():
			x = tf.placeholder(tf.float32, shape=[None, 10, 9, 118], name="x")
			y_ = tf.placeholder(tf.float32, shape=[None, 1], name="y_")
			keep_prob = tf.placeholder(tf.float32, name="keep_prob")

			f = 192
			conv1 = conv_net(x, 5, f, 'conv1')
			conv2 = conv_net(conv1, 3, f, 'conv2')
			conv3 = conv_net(conv2, 3, f, 'conv3')
			conv4 = conv_net(conv3, 3, f, 'conv4')
			conv5 = conv_net(conv4, 3, f, 'conv5')
			conv6 = conv_net(conv5, 3, f, 'conv6')
			
			conv12 = conv_net(conv6, 3, f, 'conv12')

			dim =  (conv12.shape[1] * conv12.shape[2] * conv12.shape[3]).value
			with tf.name_scope('fc'):
				fc0 = tf.reshape(conv12, [-1, dim])

				#fc1 = fc_net(fc0, 4096, 'fc1', 'relu')
				#fc1_drop = tf.nn.dropout(fc1, keep_prob)

				self.model = fc_net(fc0, 1, 'fc2', 'none')

			with tf.name_scope('output'):
				self.prom = tf.nn.sigmoid(self.model)

			with tf.name_scope('loss'):
				self.loss = tf.reduce_mean(tf.nn.sigmoid_cross_entropy_with_logits(labels = y_, logits = self.model))

			with tf.name_scope('train'):
				self.train_step = tf.train.AdamOptimizer().minimize(self.loss)
			
			#self.writer = tf.summary.FileWriter("d:/temp/1")
			#self.writer.add_graph(self.graph)
			#self.writer.close()

			self.sess.run(tf.global_variables_initializer())
			
			
	def train(self, data):
		with self.graph.as_default():
			self.sess.run(self.train_step, feed_dict={"x:0": data[0], "y_:0": data[1], "keep_prob:0":0.5})


	def evaluate(self, data):
		with self.graph.as_default():
			return self.sess.run(self.prom, {"x:0": data, "keep_prob:0":1})

	def get_loss(self, data):
		with self.graph.as_default():
			return self.sess.run(self.loss, feed_dict={"x:0": data[0], "y_:0": data[1], "keep_prob:0":1})
	

def weight_variable(shape):
	return tf.Variable(tf.truncated_normal(shape, stddev=0.1))

def bias_variable(shape):
	return tf.Variable(tf.constant(0.1, shape=shape))

def conv2d(x, W):
	return tf.nn.conv2d(x, W, strides=[1, 1, 1, 1], padding='SAME')

def max_pool_2x2(x):
	return tf.nn.max_pool(x, ksize=[1, 2, 2, 1],
                        strides=[1, 2, 2, 1], padding='SAME')

def conv_net(x, s, f, name):
	with tf.name_scope(name):
		d1 = x.shape[3].value
		w = weight_variable([s, s, d1, f])
		b = bias_variable([f])
		conv = tf.nn.relu(conv2d(x, w) + b)
		return conv

def fc_net(x, out_dim, name, func = 'relu'):
	with tf.name_scope(name):
		in_dim = x.shape[1].value;
		w = weight_variable([in_dim, out_dim])
		b = bias_variable([out_dim])
		if func == 'relu':
			return tf.nn.relu(tf.matmul(x, w) + b)
		elif func == 'sigmoid':
			return tf.nn.sigmoid(tf.matmul(x, w) + b)
		elif func == 'softmax':
			return tf.nn.softmax(tf.matmul(x, w) + b)
		elif func == 'none' :
			return (tf.matmul(x, w) + b)




