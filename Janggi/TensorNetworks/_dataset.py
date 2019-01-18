'''
학습을 위한 데이터 공급 목적
'''



import os
import tensorflow as tf
import numpy as np
import multiprocessing as mp


import _game
from _gibo import read_all_gibos
from _gibo import init_board_from_gibo

from _params import args


# 하나의 게임에 대한 기보를 dict로 저장함
# dict.keys() = {'out_info', 'out_moves'}
# out_info = 대회 정보에 대한 dict
# out_moves = move의 리스트


#class GiboGenerator:
#    def __init__(self, path):
#        self.gibo_set = prepare_gibo(path)

#    def gen_policy(self):
#        while True:
#            for gibos in self.gibo_set:
#                for gibo in gibos:
#                    info = gibo['info']
#                    my_first = info['my_first']

                    

def board2net(board):
    '''
    게임의 보드를 학습에 맞는 형태로 수정
    kernel 0-14:돌의 위치
    kernel 14-28: 움직임 가능한 
    kernel 28-42: 따먹기 가능한
    '''

    #내 움직임
    my_moves, my_takes, my_prots = _game.get_all_moves(board)

    #상대방 움직임
    yo_board = _game.rot_board(board)
    yo_moves, yo_takes, yo_prots = _game.get_all_moves(board)
    yo_moves = _game.rot_moves(yo_moves)
    yo_takes = _game.rot_moves(yo_takes)
    yo_prots = _game.rot_moves(yo_prots)

    moves = my_moves + yo_moves
    takes = my_takes + yo_takes
    prots = my_prots + yo_prots

    
    # 보드 구성
    # 0~14 : 돌 위치, 종류별로.
    # 15~28 : 해당 돌이 갈 수 있는 곳 표시
    # 29~42 : 위협을 받는 곳 표시
    # 43~56 : 보호(라기보다 복수..)해주는 곳 표시
    net_board = np.zeros([10, 9, 57], np.uint8)

    #위치 표시
    for y in range(10):
        for x in range(9):
             net_board[y, x, board[y, x]] = 1

    #내가(channel) 갈 수 있는 곳 표시
    for move in moves:
        if move == _game.MOVE_EMPTY:
            continue
        pos_from = move[0]
        pos_to = move[1]
        channel = 14 + board[pos_from]
        net_board[pos_to][channel] = 1

    #나(channel)를 위협을 하는 기물의 위치 표시
    for move in takes:
        if move == _game.MOVE_EMPTY:
            continue
        pos_from = move[0]
        pos_to = move[1]
        channel = 28 + board[pos_to] 
        net_board[pos_from][channel] = 1

    #내가(channel) 보호해 주는 기물의 위치 표시
    for move in prots:
        if move == _game.MOVE_EMPTY:
            continue
        pos_from = move[0]
        pos_to = move[1]
        channel = 42 + board[pos_to] 
        net_board[pos_from][channel] = 1

    #최대 아군 무브 개수, len(my_moves)의 최대값.. 아 대충 세려서. 현실적으로 보통은 30-40개 정도던데.
    #차 (9 + 8) * 2, 포(8 + 7) * 2, 마(8) *2, 상(8) * 2, 졸(3) * 5, 궁사(6 + 2 + 2)
    #차(34) 포(30), 마(16), 상(16), 졸(15), 궁사(10) = 121, 120이라 하자
    #이거.. 왜 계산했지?

    return net_board

def move2net(move):
    '''
    움직임을 네트워크로 표시
    '''
    net_move = np.zeros([10, 9, 2], np.uint8)
    if move != _game.MOVE_EMPTY:
        pos_from = move[0]
        pos_to = move[1]
        net_move[pos_from][0] = 1
        net_move[pos_to][1] = 1

    return net_move

  

#class GiboGenerator:
#    def __init__(self, path:str):
#        self.path = path

#    def _supplier(self):
#        for info in os.walk(self.path):
#            root = info[0]
#            files = info[2]

#            for file in files:
#                file_path = root + '/' + file
#                yield file_path
                

#    def generator(self):
#        for samples in mp.Pool(2).imap_unordered(_read_gibo, self._supplier()):
#            for sample in samples:
#                yield sample



#def get_dataset(gibo_generator:GiboGenerator, batchsize=8):
#    ds = tf.data.Dataset.from_generator(
#        gibo_generator.generator(),
#        {
#            'net_board':tf.float32,
#            'net_move':tf.float32
#        },
#        {
#            [10, 9, 42],
#            [10, 9, 2]
#        }
#    )
#    ds  = tf.data.Dataset.batch(batchsize)
#    return ds

def test_read():
    gibos = read_all_gibos(args.gibo_path)
    samples = []
    for gibo in gibos:
        # print(gibo['info'])
        board = init_board_from_gibo(gibo)
        moves = gibo['moves']
        replay = _game.ReplayForTraining(board, moves)

        for cur_board, move in replay.iterator():
            net_board = board2net(cur_board)
            net_move = move2net(move)
            samples.append({'net_board':net_board, 'net_move':net_move})
    return samples




def test_generator():
    generator = GiboGenerator(args.gibo_path)
    for sample in generator.generator():
        print(sample)

if __name__ == '__main__':
    test_read()