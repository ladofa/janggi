
import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
import tensorflow as tf

#from tensorflow.examples.tutorials.mnist import input_data
#mnist = input_data.read_data_sets('MNIST_data', one_hot=True)

class Network():
	def __init__(self):
		self.graph = tf.Graph()
		with self.graph.as_default():
			self.sess = tf.Session()

	def save(self, new_name):
		with self.graph.as_default():
			saver = tf.train.Saver()
			saver.save(self.sess, "./training_data/" + new_name)

	def load(self, new_name):
		with self.graph.as_default():
			saver = tf.train.Saver()
			saver.restore(self.sess, "./training_data/" + new_name)

class PolicyNetwork(Network):
	def __init__(self):
		Network.__init__(self)
		with self.graph.as_default():
			x = tf.placeholder(tf.float32, shape=[None, 9, 10, 14])
			conv1 = conv_net(x, 56)
			conv2 = conv_net(conv1, 56)
			conv3 = conv_net(conv2, 56)
			conv4 = conv_net(conv3, 56)
			conv5 = conv_net(conv4, 56)
			self.model = fc_net(conv5, 9 * 10 * 9 * 10)
			
	def train(self, x, y_):
		with self.graph.as_default():
			cross_entropy = tf.reduce_mean(
				tf.nn.softmax_cross_entropy_with_logits(labels = y_, logits = model))
			self.train_step = tf.train.AdamOptimizer(1e-4).minimize(cross_entropy)
			sess.run(self.train_step, feed_dict={x: x, y_: y_})

	def predict(self, x):
		with self.graph.as_default():
			return sess.run(self.model, {x: x})

	

def weight_variable(shape):
	return tf.Variable(tf.truncated_normal(shape, stddev=0.1))
  
def bias_variable(shape):
	return tf.Variable(tf.constant(0.1, shape=shape))

def conv2d(x, W):
	return tf.nn.conv2d(x, W, strides=[1, 1, 1, 1], padding='SAME')

def max_pool_2x2(x):
	return tf.nn.max_pool(x, ksize=[1, 2, 2, 1],
                        strides=[1, 2, 2, 1], padding='SAME')

def conv_net(x, f):
	d1 = x.shape[3].value
	w = weight_variable([5, 5, d1, f])
	b = bias_variable([f])
	conv = tf.nn.relu(conv2d(x, w) + b)
	return conv

def fc_net(x, out_dim):
	in_dim = (x.shape[1] * x.shape[2] * x.shape[3]).value
	print(x)
	print(in_dim)
	in_net = tf.reshape(x, [-1, in_dim])
	w = weight_variable([in_dim, out_dim])
	b = bias_variable([out_dim])
	return tf.nn.relu(tf.matmul(in_net, w) + b)



######################################################################################


def create_policy_net(kind):
	x = tf.placeholder(tf.float32, shape=[None, 9, 10, 14])
	conv1 = conv_net(x, 56)
	conv2 = conv_net(conv1, 56)
	conv3 = conv_net(conv2, 56)
	conv4 = conv_net(conv3, 56)
	conv5 = conv_net(conv4, 56)
	fc = fc_net(conv5, 9 * 10 * 9 * 10)
		
	cross_entropy = tf.reduce_mean(
		tf.nn.softmax_cross_entropy_with_logits(labels = y_, logits = fc))
	train_step = tf.train.AdamOptimizer(1e-4).minimize(cross_entropy)
	correct_prediction = tf.equal(tf.argmax(y_conv, 1), tf.argmax(y_, 1))
	accuracy = tf.reduce_mean(tf.cast(correct_prediction, tf.float32))
	
	return accurary

def create_value_net(kind):
	x = tf.placeholder(tf.float32, shape=[None, 9, 10, 14])
	conv1 = conv_net(x, 56)
	conv2 = conv_net(conv1, 56)
	conv3 = conv_net(conv2, 56)
	conv4 = conv_net(conv3, 56)
	conv5 = conv_net(conv4, 56)
	fc1 = fc_net(conv5, 1024)
	fc2 = fc_net(fc1, 1)

	cross_entropy = tf.reduce_mean(
		tf.nn.softmax_cross_entropy_with_logits(labels=y_, logits=fc2))
	train_step = tf.train.AdamOptimizer(1e-4).minimize(cross_entropy)
	correct_prediction = tf.equal(tf.argmax(y_conv, 1), tf.argmax(y_, 1))
	accuracy = tf.reduce_mean(tf.cast(correct_prediction, tf.float32))
	return accurary


def train_policy(net, x, y_):

	train_accuracy = net.eval(feed_dict={
	x: x, y_: y_})


	#with tf.Session() as sess:
	#  sess.run(tf.global_variables_initializer())
	#  for i in range(20000):
	#    batch = mnist.train.next_batch(50)
	#    if i % 100 == 0:
	#      train_accuracy = accuracy.eval(feed_dict={
	#          x: batch[0], y_: batch[1], keep_prob: 1.0})
	#      print('step %d, training accuracy %g' % (i, train_accuracy))
	#    train_step.run(feed_dict={x: batch[0], y_: batch[1], keep_prob: 0.5})

	#  print('test accuracy %g' % accuracy.eval(feed_dict={
	#      x: mnist.test.images, y_: mnist.test.labels, keep_prob: 1.0}))





def test_tensorflow():
	
	x = tf.placeholder(tf.float32, shape=[None, 28, 28, 14])

	W_conv1 = weight_variable([5, 5, 14, numFilter])
	b_conv1 = bias_variable([numFilter])

	x_image = tf.reshape(x, [-1, 28, 28, 3])#-1, width, height, depth

	temp = conv2d(x, W_conv1)
	temp2 = temp + b_conv1;#

	h_conv1 = tf.nn.relu(conv2d(x, W_conv1 + bconv1))
	h_pool1 = max_pool_2x2(h_conv1)

	W_conv2 = weight_variable([5, 5, 32, 64])
	b_conv2 = bias_variable([64])

	h_conv2 = tf.nn.relu(conv2d(h_pool1, W_conv2) + b_conv2)
	h_pool2 = max_pool_2x2(h_conv2)
	
	W_fc1 = weight_variable([7 * 7 * 64, 1024])
	b_fc1 = bias_variable([1024])

	h_pool2_flat = tf.reshape(h_pool2, [-1, 7*7*64])
	h_fc1 = tf.nn.relu(tf.matmul(h_pool2_flat, W_fc1) + b_fc1)

	keep_prob = tf.placeholder(tf.float32)
	h_fc1_drop = tf.nn.dropout(h_fc1, keep_prob)

	W_fc2 = weight_variable([1024, 10])
	b_fc2 = bias_variable([10])

	y_conv = tf.matmul(h_fc1_drop, W_fc2) + b_fc2

	cross_entropy = tf.reduce_mean(
		tf.nn.softmax_cross_entropy_with_logits(labels=y_, logits=y_conv))
	train_step = tf.train.AdamOptimizer(1e-4).minimize(cross_entropy)
	correct_prediction = tf.equal(tf.argmax(y_conv, 1), tf.argmax(y_, 1))
	accuracy = tf.reduce_mean(tf.cast(correct_prediction, tf.float32))

	#with tf.Session() as sess:
	#  sess.run(tf.global_variables_initializer())
	#  for i in range(20000):
	#    batch = mnist.train.next_batch(50)
	#    if i % 100 == 0:
	#      train_accuracy = accuracy.eval(feed_dict={
	#          x: batch[0], y_: batch[1], keep_prob: 1.0})
	#      print('step %d, training accuracy %g' % (i, train_accuracy))
	#    train_step.run(feed_dict={x: batch[0], y_: batch[1], keep_prob: 0.5})

	#  print('test accuracy %g' % accuracy.eval(feed_dict={
	#      x: mnist.test.images, y_: mnist.test.labels, keep_prob: 1.0}))
