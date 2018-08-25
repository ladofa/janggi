import os
import game
import tensorflow as tf

# 하나의 게임에 대한 기보를 dict로 저장함
# dict.keys() = {'out_info', 'out_moves'}
# out_info = 대회 정보에 대한 dict
# out_moves = move의 리스트


def prepare_gibo(path):
    gibo_set = []
    for info in os.walk(path):
       root = info[0]
       files = info[2]
       for file in files:
           gibo = read_gibo(root + '\\' + files)
           #2차원 구조로 놔둔다. 효율성을 위해서
           gibo_set.append(gibo)
    return gibo_set

class GiboGenerator:
    def __init__(self, path):
        self.gibo_set = prepare_gibo(path)

    def gen_policy(self):
        while True:
            for gibos in self.gibo_set:
                for gibo in gibos:
                    info = gibo['info']
                    my_first = info['my_first']

                    







def get_dataset(path, kind, batchsize=8):
    ds = tf.data.Dataset.from_sparse_tensor_slices
    ds  = tf.data.Dataset.batch(batchsize)

    


        