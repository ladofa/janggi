import tensorflow as tf

def get_network(name, input_board):
	if name == 'simple':
		return simple_network(input_board)


def simple_network(input_board):
	x = input_board
	x = tf.layers.conv2d(x, 56, 5, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, 56, 5, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, 56, 5, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, 56, 5, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, 56, 5, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, 2, 3, padding='same')

	return x
