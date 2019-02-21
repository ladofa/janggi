import tensorflow as tf

def get_network(name, input_board):
	if name == 'simple':
		return simple_network(input_board)
	elif name == 'resnet':
		return res_network(input_board)


def simple_network(input_board):
	channels = 36
	x = tf.cast(input_board, tf.float32)

	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)
	x = tf.layers.conv2d(x, channels, 3, padding='same', activation=tf.nn.relu)

	x = tf.layers.conv2d(x, 2, 3, padding='same')

	return x


def res_network(input_board):
	channels = 96
	x = tf.cast(input_board, tf.float32)
	x = tf.layers.conv2d(x, channels, 5, padding='same')

	x = res_se_layer(x, channels)
	x = res_se_layer(x, channels)
	x = res_se_layer(x, channels)
	x = res_se_layer(x, channels)
	x = res_se_layer(x, channels)
	x = res_se_layer(x, channels)

	x = tf.layers.conv2d(x, 2, 3, padding='same')

	return x


def res_layer(x, ch, kernel_size = 3):
	res = x
	x = tf.layers.batch_normalization(x)
	x = tf.nn.relu6(x)
	x = tf.layers.conv2d(x, ch, kernel_size, padding='same')

	x = tf.layers.batch_normalization(x)
	x = tf.nn.relu6(x)
	x = tf.layers.conv2d(x, ch, kernel_size, padding='same')

	if res.shape[3] != x.shape[3]:
		res = tf.layers.conv2d(res, x.shape[3], 1)
	x = res + x

	return x

def se_layer(x, ratio = 16):
	res = x
	ch = x.shape[3]
	r_ch = max(ch // 16, 4)
	x = tf.reduce_mean(x, [1, 2])
	x = tf.layers.dense(x, r_ch, activation = tf.nn.relu)
	x = tf.layers.dense(x, ch, activation = tf.nn.sigmoid)
	x = tf.reshape(x, [-1, 1, 1, ch])
	x = res * x

	return x


def res_se_layer(x, ch, kernel_size = 3):
	res = x
	x = tf.layers.batch_normalization(x)
	x = tf.nn.relu6(x)
	x = tf.layers.conv2d(x, ch, kernel_size, padding='same')

	x = tf.layers.batch_normalization(x)
	x = tf.nn.relu6(x)
	x = tf.layers.conv2d(x, ch, kernel_size, padding='same')

	if res.shape[3] != x.shape[3]:
		res = tf.layers.conv2d(res, x.shape[3], 1)
	x = se_layer(x)
	x = res + x

	return x