#네트워크 구조 설계, 생성
#입력 = (10, 9, 16) game.py의 get_state 참고
#policy출력 = (90 * 90 + 1) game.py의 get_move_index 참고
#value출력 = () -1부터 1까지, 1은 승리, -1은 패배, 0은 비김

import tensorflow as tf
from tensorflow import keras
from functools import partial

class SqueezeExcitation(keras.layers.Layer):
    def __init__(self, rate=16, **kwargs):
        super().__init__(**kwargs)
        self.rate = 16

    def build(self, input_shape):
        in_ch = input_shape[-1]
        sq_ch = tf.cast(in_ch / self.rate, tf.int32)
        sq_ch = tf.maximum(sq_ch, tf.constant(4))
        self.dense1 = keras.layers.Dense(sq_ch, activation='relu')
        self.dense2 = keras.layers.Dense(in_ch, activation='sigmoid')
        self.reshape = keras.layers.Reshape([1, 1, in_ch])

    def call(self, input):
        x = input
        x = keras.layers.GlobalMaxPool2D()(x)
        x = self.dense1(x)
        x = self.dense2(x)
        x = self.reshape(x)
        # x = tf.reshape(x, tf.TensorShape([None, 1, 1, in_ch]))
        return input * x

conv2d = partial(keras.layers.Conv2D, padding='same', use_bias=False)

def residual_block(input, filters):
    z = conv2d(filters, 3)(input)
    z = keras.layers.BatchNormalization()(z)
    z = keras.activations.elu(z)
    z = SqueezeExcitation()(z)
    z = conv2d(filters, 3)(z)
    z = keras.layers.BatchNormalization()(z)
    z = z + input
    z = keras.activations.elu(z)
    return z


def gen_network(filters, n_blocks):
    input = keras.layers.Input(shape=[10, 9, 16])
    z = conv2d(filters, kernel_size=3)(input)
    z = keras.activations.elu(z)

    #residual tower
    for _ in range(n_blocks):
        z = residual_block(z, filters)
    
    #policy
    p = conv2d(filters=4, kernel_size=1)(z)
    p = keras.layers.BatchNormalization()(p)
    p = keras.activations.elu(p)
    p = keras.layers.Flatten()(p)
    p = keras.layers.Dense(90 * 90 + 1)(p)
    p = keras.activations.softmax(p, name='policy')

    #value
    v = conv2d(filters=1, kernel_size=1)(z)
    v = keras.layers.BatchNormalization()(v)
    v = keras.activations.elu(v)
    v = keras.layers.Flatten()(v)
    v = keras.layers.Dense(256)(v)
    v = keras.activations.elu(v)
    v = keras.layers.Dense(1)(v)
    v = keras.activations.tanh(v, name='value')
    
    p_model = keras.models.Model(inputs=[input], outputs=[p])
    v_model = keras.models.Model(inputs=[input], outputs=[v])
    full_model = keras.models.Model(inputs=[input], outputs=[p, v])

    return p_model, v_model, full_model


