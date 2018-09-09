import tensorflow as tf

def get(features, labels, ):
    if kind == 'classic':
        return _classic(ph_board)
    else:
        raise Exception('unknown network : ' + kind)

#정책 네트워크
def _classic(ph_board):
    filter_size = 192
    with tf.variable_scope('policy_network'):
        with tf.variable_scope('stacked_conv2d'):
            x = tf.layers.conv2d(ph_board, filter_size, 5, padding='same', activation=tf.nn.relu)#처음만 5개
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
            x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
        with tf.variable_scope('fc'):
            dim =  (conv12.shape[1] * conv12.shape[2] * conv12.shape[3]).value
            fc0 = tf.reshape(conv12, [-1, dim])
            #2451개의 가능한 움직임 중 하나
            logits = tf.layers.dense(fc0, 2451, activation=tf.nn.softmax)
    
    return logits

