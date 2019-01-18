'''
기보 파일을 읽는 데까지.
'''


import os
import numpy as np
import _game
from _game import Stone

from _params import args


def get_setting_from_korean(korean):
    if korean == '마상상마':
        return _game.Setting.MSSM
    elif korean == '상마마상':
        return _game.Setting.SMMS
    elif korean == '상마상마':
        return _game.Setting.SMSM
    elif korean == '마상마상':
        return _game.Setting.MSMS
    else:
       raise Exception('unknown setting : ' + korean)


def read_gibo(path):
    '''
    기보 파일을 파싱
    '''
    ###########################################
    #1차 파싱


    #평범하게 텍스트 모드로 읽고 싶은데 0xff같은 것이 껴 있어서 cp949로 못 읽을 수 있다.
    #해서 바이트로 읽은 후 0xff를 없애준다.
    file_bytes = open(path, 'rb').read()
    #0xff가 껴있는지 찾아본다
    ff_indices = [i for i, val in enumerate(file_bytes) if val == 0xff]
    #없음 말고
    if len(ff_indices) == 0:
        fixed_bytes = file_bytes
    #있으면 뺀다
    else:
        fixed_bytes = b''
        ff_indices += [len(file_bytes)]#마지막 추가
        i_start = 0
        for i in ff_indices:
            fixed_bytes += file_bytes[i_start:i]
            #다음 스타트, +1 로 인해 0xff는 제외된다.
            i_start = i + 1

    lines = fixed_bytes.decode('cp949').splitlines()

    out_info = {'path' : path}
    out_moves = []
    out_gibo = []

    comment_found = False

    for line in lines:
        #주석..
        if '{' in line:
           comment_found = True
           
        if comment_found:
            if '}' in line:
                comment_found = False
            continue

        if line == '':
            #모든 정보를 다 읽었으므로 저장한다.
            if len(out_info) != 0 and len(out_moves) != 0:
                out_gibo.append({'info':out_info, 'moves':out_moves})
                out_info = {'path' : path}
                out_moves = []
        #대회 정보
        elif line[0] == '[':
            key_start = 1
            key_end = line.index(' ')
            value_start = line.index('"') + 1
            value_end = -line[::-1].index('"') - 1
            key = line[key_start:key_end]
            value = line[value_start:value_end]
            out_info[key] = value
        #수순(moves)
        else:
            words_pre = line.split(' ')
            #<0> 과 같은 이상한 문자가 껴 있다.. ㅜㅜ
            words = [word for word in words_pre if word != '<0>' and word != '\x1a']

            for i in range(0, len(words), 2):
                word_num = words[i]
                word_move = words[i + 1]
                num = int(word_num[:-1]) #마지막 점을 뺀다
                #한수쉼
                if word_move[0] == '한':
                    move = _game.MOVE_EMPTY
                else:
                    fy = int(word_move[0]) - 1
                    fx = int(word_move[1]) - 1

                    #다음 숫자를 찾는다
                    number_pos = 2
                    while not word_move[number_pos].isdigit() : number_pos += 1

                    ty = int(word_move[number_pos]) - 1
                    tx = int(word_move[number_pos + 1]) - 1

                    if fy == -1: fy = 9
                    if fx == -1: fx = 8
                    if ty == -1: ty = 9
                    if tx == -1: tx = 8

                    move_from = (fy, fx)
                    move_to = (ty, tx)
                    
                    move = [move_from, move_to]
                out_moves.append(move)

    ####################################
    #2차 파싱
    #학습에 맞도록 재구성
    #... 은 다른 함수에서~

    #board, move, winner   

    return out_gibo

def read_all_gibos(path):
    gibo_list = []
    for root, dirs, files in os.walk(path):
       for file in files:
           print(file)
           #기보 파일이 아니면 버리고
           first, last = os.path.splitext(file)
           if last != '.gib':
               continue
           #기보를 읽어서 저장
           gibos = read_gibo(root + '\\' + file)
           gibo_list += gibos
    return gibo_list

def init_board_from_gibo(gibo):
    '''
    파싱된 기보 정보를 이용하여 board와 기타 다른 정보를 만든다.
    '''
    info = gibo['info']
    board = np.zeros([10, 9], np.uint8)
    #기물의 위치를 전부 지정해주는 경우
    if '판' in info.keys():
        value = info['판']
        lines = value.split('/')
        x = 0
        y = 0
        for line in lines:

            for letter in line:
                try:
                    #숫자만큼 건너뛰거나
                    space = int(letter)
                    x += space
                    #숫자가 아니라면
                except ValueError:
                    #유닛을 넣는다.
                    if letter == '차':
                        board[y, x] = Stone.MY_CHA
                    elif letter == '상':
                        board[y, x] = Stone.MY_SANG
                    elif letter == '마':
                        board[y, x] = Stone.MY_MA
                    elif letter == '포':
                        board[y, x] = Stone.MY_PO
                    elif letter == '사':
                        board[y, x] = Stone.MY_SA
                    elif letter == '장':
                        board[y, x] = Stone.MY_KING
                    elif letter == '졸':
                        board[y, x] = Stone.MY_JOL
                    elif letter == '車':
                        board[y, x] = Stone.YO_CHA
                    elif letter == '象':    
                        board[y, x] = Stone.YO_SANG
                    elif letter == '馬':    
                        board[y, x] = Stone.YO_MA
                    elif letter == '包':    
                        board[y, x] = Stone.YO_PO
                    elif letter == '士':    
                        board[y, x] = Stone.YO_SA
                    elif letter == '將':    
                        board[y, x] = Stone.YO_KING
                    elif letter == '兵':    
                        board[y, x] = Stone.YO_JOL
                    elif letter == ' ': #그리고 아마도 한 접장기
                        break
                    else:
                        raise Exception('unknown letter! : ' + letter)
                    x += 1
            y += 1
            x = 0
            #'한 판정승' 은 건너뛴다.
            if y > 9:
                break

    #판이 아니면 무조건 한차림 초차림(혹은 한포진 초포진)이 있어야 한다.
    else:
        if '초차림' in info:
            mine = info['초차림']
        elif '초포진' in info:
            mine = info['초포진']
        my_setting = get_setting_from_korean(mine)
        if '한차림' in info:
            yors = info['한차림']
        elif '한포진' in info:
            yors = info['한포진']
        yo_setting = get_setting_from_korean(yors)
        board = _game.init_board(my_setting, yo_setting)

    return board



def test_gibo():
    for info in os.walk(args.gibo_path):
        path = info[0]
        files = info[2]

        for file in files:
            file_path = path + '/' + file
            gibos = read_gibo(file_path)
            for gibo in gibos:
                print(gibo['info'])
                board = init_board_from_gibo(gibo)
                moves = gibo['moves']
                replay = _game.Replay(board, moves)

                prev_move = []
                for cur_board, move in replay.iterator():
                    _game.print_board(cur_board, yellow_back=move, green_back=prev_move)
                    print(move)
                    prev_move = _game.rot_move(move)
                    input('')
                

if __name__ == '__main__':
    test_gibo()