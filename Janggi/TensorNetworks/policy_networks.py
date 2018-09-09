import tensorflow as tf
import game

def get(features):
    if kind == 'classic':
        return _classic(ph_board)
    else:
        raise Exception('unknown network : ' + kind)

def move2byte(move):
    return move[0] * 9 + move[1] # y * width + x



#정책 네트워크
def _classic(features):
    board = features['board']
    move = features['move']
    filter_size = 192
    
    with tf.variable_scope('stacked_conv2d'):
        x = tf.layers.conv2d(board, filter_size, 5, padding='same', activation=tf.nn.relu)#처음만 5개
        x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
        x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
        x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
        x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
        x = tf.layers.conv2d(x, filter_size, 3, padding='same', activation=tf.nn.relu)
    with tf.variable_scope('fc'):
        x = tf.layers.conv2d(x, 2451, 1, padding='same', activation=tf.nn.relu)
        dim =  (conv12.shape[1] * conv12.shape[2] * conv12.shape[3]).value
        fc0 = tf.reshape(conv12, [-1, dim])
        #2451개의 가능한 움직임 중 하나
        logits = tf.layers.dense(fc0, 2451, activation=tf.nn.softmax)
    
    return logits

