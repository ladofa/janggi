import tensorflow as tf

def get(ph_board, kind):
    if kind == 'classic':
        return value_network_classic(ph_board)
    else:
        raise Exception('unknown network : ' + kind)

#가치 네트워크
def value_network_classic(ph_board):
    filter_size = 192
    with tf.variable_scope('value_network'):
        with tf.variable_scope('stacked_conv2d'):
            x = tf.layers.conv2d(ph_board, filter_size, 5, padding='same', activation=tf.nn.relu)#처음만 5개
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
        with tf.variabel_scope('fc'):
            dim =  (conv12.shape[1] * conv12.shape[2] * conv12.shape[3]).value
            fc0 = tf.reshape(conv12, [-1, dim])
            #시그모이드함수의 결과값의 범위는 (-1, 1)
            #1이면 반드시 이김, 0이면 비김, -1이면 반드시 짐.
            logits = tf.layers.dense(fc0, 1, activation=tf.nn.sigmoid)

    return logits
