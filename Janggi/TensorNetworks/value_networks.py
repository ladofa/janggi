import tensorflow as tf

def get_network(name, input_board):
	if name == 'simple':
		return simple_network(input_board)


def simple_network(input_board):
	channels = 56
	x = tf.cast(input_board, tf.float32)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)

	x = tf.layers.conv2d(x, 512, 1, padding='same', activation=tf.nn.relu)
	x = tf.reshape(x, [-1, 512 * 90])
	x = tf.layers.dense(x, 2)

	return x
