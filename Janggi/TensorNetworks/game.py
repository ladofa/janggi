from enum import IntEnum
import numpy as np
import os
from colorama import Fore, Back, Style
import colorama
colorama.init(autoreset=True)

import params
args = params.args


# 네트워크나 인공지는과는 상관없이 장기 본연에 대한 내용이 담김

# 게임 말은 0-14로 표현
# 아래쪽이 My, 위쪽이 Yours이다. (초 한은 관계없음, 한이 아래쪽에 위치했으다면 한 = my)
# 모든 인공지능은 my의 입장에서 생각한다.
class Stone(IntEnum):
    EMPTY = 0,

    MY_JOL = 1,
    MY_SANG = 2,
    MY_MA = 3,
    MY_PO = 4,
    MY_CHA = 5,
    MY_SA = 6
    MY_KING = 7

    YO_JOL = 8
    YO_SANG = 9
    YO_MA = 10,
    YO_PO = 11,
    YO_CHA = 12,
    YO_SA = 13,
    YO_KING = 14

_stone_letters = [
    "＋", "졸", "상","마", "포", "차", "사","초", "兵", "象", "馬", "包", "車", "士", "楚"
	]

def is_my_stone(stone):
    return 1 <= stone <= 7
def is_yo_stone(stone):
    return 8 <= stone <= 14
def get_opposite_stone(stone):
    if stone == Stone.EMPTY:
        return Stone.EMPTY
    elif stone <= 7:
        return stone + 7
    else:
        return stone - 7


def is_opposite(s1, s2):
    if s1 == 0 or s2 == 0:
        return False
    if s1 <= 7:
        return s2 >=8
    else:
        return s2 <= 7


# 장기판은 board로 칭함
# board는 10 x 9개의 Stone이라고 할 수 있다. 10 x 9 의 numpy array로 나타냄
# 장기판의 크기 10, 9는 절대불변이므로 그냥 상수를 썼다.

#초기 차림은 네 가지가 있다.
class Setting(IntEnum):
    MSSM = 0, #마상상마
    SMMS = 1, #상마마상
    SMSM = 2, #상마상마
    MSMS = 3  #마상마상

def get_setting_from_korean(korean):
    if korean == '마상상마':
        return Setting.MSSM
    elif korean == '상마마상':
        return Setting.SMMS
    elif korean == '상마상마':
        return Setting.SMSM
    elif korean == '마상마상':
        return Setting.MSMS
    else:
       raise Exception('unknown setting : ' + korean)

# 장기판위의 위치, position - pos 는 (y, x)의 튜플로 나타냄 - 기보 노테이션과 비교해서 
# x y 순서는 같은데 0부터 시작하는 점에 유의
# move는 [pos, pos]로 나타냄. 어떤 말이 움직여는지에 대한 정보는 포함하지 않음.

# 널 포지션... 쓸데없는 것 같지만 필요함
pos_empty = (12, 27)

# 널 무브.. 마찬가지로.
move_empty = [pos_empty, pos_empty]


# pos를 1 byte로 나타낸 것을 point라고 칭함. point = pos.y * 9 + pos.x
def get_point(pos):
    return pos(0) * 9 + pos(1)

def get_pos(point):
    return (point // 9, point % 9)

def in_board(pos):
    return pos[0] >= 0 and pos[1] >= 0 and pos[0] < 10 and pos[1] < 9

#게임판 초기셋팅
def init_board(my_setting, yo_setting):
    board = np.zeros([10, 9], dtype = np.uint8)
    #공통 차림
    board[0, 0] = Stone.YO_CHA
    board[0, 3] = Stone.YO_SA
    board[0, 5] = Stone.YO_SA
    board[0, 8] = Stone.YO_CHA
    board[1, 4] = Stone.YO_KING
    board[2, 1] = Stone.YO_PO
    board[2, 7] = Stone.YO_PO
    board[3, 0] = Stone.YO_JOL
    board[3, 2] = Stone.YO_JOL
    board[3, 4] = Stone.YO_JOL
    board[3, 6] = Stone.YO_JOL
    board[3, 8] = Stone.YO_JOL

    board[9, 0] = Stone.MY_CHA
    board[9, 3] = Stone.MY_SA
    board[9, 5] = Stone.MY_SA
    board[9, 8] = Stone.MY_CHA
    board[8, 4] = Stone.MY_KING
    board[7, 1] = Stone.MY_PO
    board[7, 7] = Stone.MY_PO
    board[6, 0] = Stone.MY_JOL
    board[6, 2] = Stone.MY_JOL
    board[6, 4] = Stone.MY_JOL
    board[6, 6] = Stone.MY_JOL
    board[6, 8] = Stone.MY_JOL

    #마상 위치
    if my_setting == Setting.MSSM:
        board[9, 1] = Stone.MY_MA
        board[9, 2] = Stone.MY_SANG
        board[9, 6] = Stone.MY_SANG
        board[9, 7] = Stone.MY_MA
    elif my_setting == Setting.SMMS:
        board[9, 1] = Stone.MY_SANG
        board[9, 2] = Stone.MY_MA
        board[9, 6] = Stone.MY_MA
        board[9, 7] = Stone.MY_SANG
    elif my_setting == Setting.SMSM:
        board[9, 1] = Stone.MY_SANG
        board[9, 2] = Stone.MY_MA
        board[9, 6] = Stone.MY_SANG
        board[9, 7] = Stone.MY_MA
    else:
        board[9, 1] = Stone.MY_MA
        board[9, 2] = Stone.MY_SANG
        board[9, 6] = Stone.MY_MA
        board[9, 7] = Stone.MY_SANG

    if yo_setting == Setting.MSSM:
        board[0, 1] = Stone.YO_MA
        board[0, 2] = Stone.YO_SANG
        board[0, 6] = Stone.YO_SANG
        board[0, 7] = Stone.YO_MA
    elif yo_setting == Setting.SMMS:
        board[0, 1] = Stone.YO_SANG
        board[0, 2] = Stone.YO_MA
        board[0, 6] = Stone.YO_MA
        board[0, 7] = Stone.YO_SANG

        #위쪽의 경우에도 아래쪽에서 보는 관점으로.
    elif yo_setting == Setting.SMSM:
        board[0, 1] = Stone.YO_SANG
        board[0, 2] = Stone.YO_MA
        board[0, 6] = Stone.YO_SANG
        board[0, 7] = Stone.YO_MA
    else:
        board[0, 1] = Stone.YO_MA
        board[0, 2] = Stone.YO_SANG
        board[0, 6] = Stone.YO_MA
        board[0, 7] = Stone.YO_SANG
        

    return board


# get_possible_move에서 쓰는 상수
# 마길
_way_ma = [
    [(-1, 0), (-2, -1)],
    [(-1, 0), (-2, 1)],
    [(1, 0), (2, -1)],
    [(1, 0), (2, 1)],
    [(0, 1), (1, 2)],
    [(0, 1), (-1, 2)],
    [(0, -1), (1, -2)],
    [(0, -1), (-1, -2)]
    ]
# 상길
_way_sang = [
    [(0, 1), (1, 2), (2, 3)],
    [(0, -1), (1, -2), (2, -3)],
    [(0, 1), (-1, 2), (-2, 3)],
    [(0, -1), (-1, -2), (-2, -3)],
    [(1, 0), (2, 1), (3, 2)],
    [(1, 0), (2, -1), (3, -2)],
    [(-1, 0), (-2, 1), (-3, 2)],
    [(-1, 0), (-2, -1), (-3, -2)],
    ]

_way_goong = [
        [[(0, 1), (1, 0), (1, 1)], [(0, -1), (1, 0), (0, 1)], [(0, -1), (-1, -1), (1, 0)]],
        [[(-1, 0), (0, 1), (1, 0)], [(-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1)], [(-1, 0), (0, -1), (1, 0)]],
        [[(-1, 0), (-1, 1), (0, 1)], [(0, -1), (-1, 0), (0, 1)], [(0, -1), (-1, -1), (-1, 0)]]
    ]

_way_my_jol = [(0, -1), (0, 1), (-1, 0)]
_way_yo_jol = [(0, -1), (0, 1), (1, 0)]

# 특정한 위치에서 움직일 수 있는 리스트를 출력한다.
# 항상 초(하단)의 입장에서 보기 때문에
# 한에 대해서는 판을 돌려서 생각하고 리턴할 때 다시 돌린다.
def get_possible_move(board, pos_from):

    if board[pos_from] == pos_empty:
        raise Exception()
    elif is_yo_stone(board[pos_from]):
        board = rot_board(board)
        is_rot = True
    else:
        is_rot = False

    py = pos_from[0]
    px = pos_from[1]

    stone_from = board[pos_from]

    def add_to_move(pos_to):
        stone_to = board[pos_to]
        if stone_to == Stone.EMPTY:
            moves.append(pos_to)
        elif is_yo_stone(stone_to):
            moves.append(pos_to)
            takes.append(pos_to)

    if stone_from == Stone.MY_CHA :
        def visit_cha(pos_to):
            stone_to = board[pos_to]
            #비어있으면 계속 진행가능
            if stone_to == Stone.EMPTY:
                moves.append(pos_to)
                return True 
            #상대기물이면 여기까지 진행가능
            elif is_yo_stone(stone_to):
                moves.append(pos_to)
                takes.append(pos_to)
                return False
            #내 기물이면 멱이므로 진행 불가능
            else:
                return False

        #상하좌우 방향으로 살핀다.
        for y in range(py - 1, -1, -1):
            if not visit_cha((y, px)):
                break
       
        for y in range(py + 1, 10):
            if not visit_cha((y, px)):
                break

        for x in range(px - 1, -1, -1):
            if not visit_cha((py, x)):
                break
       
        for x in range(px + 1, 9):
            if not visit_cha(board(py, x)):
                break

        #궁 귀에서 대각선 움직임
        if px == 3 and (py == 0 or py == 7):
            if visit_char((py + 1, px + 1)):
                visit_char((py + 2, px + 2))
        elif px == 3 and (py == 2 or py == 9):
            if visit_char((py - 1, px + 1)):
                visit_char((py - 2, px + 2))
        elif px == 5 and (py == 0 or py == 7):
            if visit_char((py + 1, px - 1)):
                visit_char((py + 2, px - 2))
        elif px == 5 and (py == 2 or py == 9):
            if visit_char((py - 1, px - 1)):
                visit_char((py - 2, px - 2))
        elif px == 4 and (py == 1 or py == 8):
            add_to_move((py + 1, px + 1))
            add_to_move((py + 1, px - 1))
            add_to_move((py - 1, px + 1))
            add_to_move((py - 1, px - 1))

        #궁중에서 대각선


    elif stone_from == Stone.MY_PO or stone_from == Stone.YO_PO:
        dari = False #다리를 못 밟았으면 False, 다리를 밟았으면 True
        def visit_po(pos_to):
            stone_to = board[pos_to]
            #포를 만나면 다리고 뭐고 그냥 멈춤
            if stone_to == Stone.MY_PO or stone_to == Stone.YO_PO:
                return False
            if not dari:
                if stone_to == Stone.EMPTY:
                    return True
                else:
                    dari = True
                    return True
            else:
                if stone_to == Stone.EMPTY:
                    moves.append(pos_to)
                    return True
                elif is_yo_stone(stone_to):
                    moves.append(pos_to)
                    takes.append(pos_to)
                    return False
                else:
                    return False

        #상하좌우 방향으로 살핀다.
        dari = False
        for y in range(py - 1, -1, -1):
            if not visit_cha((y, px)):
                break

        dari = False
        for y in range(py + 1, 10):
            if not visit_cha((y, px)):
                break

        dari = False
        for x in range(px - 1, -1, -1):
            if not visit_cha((py, x)):
                break

        dari = False
        for x in range(px + 1, 9):
            if not visit_cha(board(py, x)):
                break

        dari = False
        #대각선으로 점프
        if px == 3 and (py == 0 or py == 7):
            if visit_po((py + 1, px + 1)):
                visit_po((py + 2, px + 2))
        elif px == 3 and (py == 2 or py == 9):
            if visit_po((py - 1, px + 1)):
                visit_po((py - 2, px + 2))
        elif px == 5 and (py == 0 or py == 7):
            if visit_po((py + 1, px - 1)):
                visit_po((py + 2, px - 2))
        elif px == 5 and (py == 2 or py == 9):
            if visit_po((py - 1, px - 1)):
                visit_po((py - 2, px - 2))

    elif stone_from == Stone.MY_MA or stone_from == Stone.YO_MA:
        for way in _way_ma:
            pos_to = (py + way[1][0], px + way[1][0])
            pos_myuk = (py + way[0][0], px + way[0][0])

            # 보드 범위를 벗어나는 경우 X
            if not in_board(pos_to):
                continue

            # 멱에 막히는 경우
            if board[pos_myuk] != Stone.EMPTY:
                continue

            add_to_move(pos_to)
            #같은 편은 못 먹..
            #else:
            #    continue

    elif stone_from == Stone.MY_SANG or stone_from == Stone.YO_SANG:
        for way in _way_sang:
            pos_to = (py + way[2][0], px + way[2][0])
            pos_myuk1 = (py + way[0][0], px + way[0][0])
            pos_myuk2 = (py + way[1][0], px + way[1][0])

            # 보드 범위를 벗어나는 경우 X
            if not in_board(pos_to):
                continue

            # 멱에 막히는 경우
            if board[pos_myuk1] != Stone.EMPTY or board[pos_myuk2] != Stone.EMPTY:
                continue

            add_to_move(pos_to)

    elif stone_from == Stone.MY_SA or stone_from == Stone.YO_SA or stone_from == Stone.MY_KING or stone_from == Stone.YO_KING:
        relative_x = pos_from[1] - 3
        if pos_from[0] <= 3:
            relative_y = pos_from[0]
        else:
            relative_y = pos_from[0] - 7

        ways = _way_goong[relative_y][relative_x]
        for way in ways:
            pos_to = (py + way[0], px + way[1])
            stone_to = boaard[pos_to]
            if stone_to == Stone.EMPTY:
                moves.append(pos_to)
            elif is_yo_stone(stone_to):
                moves.append(pos_to)
                takes.append(pos_to)

    elif stone_from == Stone.MY_JOL:
        for way in _way_my_jol:
            pos_to = (py + way[0], px + way[1])
            if not in_board(pos_to):
                continue
            add_to_move(pos_to)
        
        #궁 안에서 움직임 추가
        if pos_from == (2, 3) or pos_from == (2, 5):
            add_to_move((1, 4))
        elif pos_from == (1, 4):
            add_to_move(0, 3)
            add_to_move(0, 5)
    else:
        raise Exception('unexpected kind of stone')

    if is_rot:
        return rot_moves(moves, takes)
    else:
        return moves, takes       



# 판 위 아래 뒤집기
# AI는 무조건 아래쪽 입장에서 생각하기 때문에 위쪽 진영을 위한 전략을 AI에게 물어볼 때, 뒤집어서 물어본다.
# value network는 판을 뒤집지 않고 +-를 반대로 생각해도 되지만 policy network는 무조건 뒤집을 수 밖에 없다.
# 한편, 학습할 때 뒤집어서 넣으면 샘플 개수를 두 배로 늘릴 수 있다.
def rot_board(board):
    nu_board = np.zeros([10, 9], np.uint8)
    for y in range(10):
        for x in range(9):
            nu_board[y, x] = get_opposite_stone(board[9 - y, 8 - x])
    return nu_board

def rot_moves(moves, takes = None):
    nu_moves = []

    for move in moves:
        nu_moves.append((9 - move[0], 8 - move[1]))

    if takes == None:
        return nu_moves
    else:
        nu_takes = []
        for take in takes:
            nu_takes.append((9 - take[0], 8 - take[1]))
        return nu_moves

# 장기 특성상 좌우를 바꿔도 동일하다.
# 학습 샘플의 개수를 두 배로 늘릴 수 있다.
def flip_lr(board):
    nu_board = np.zeros([10, 9], np.uint8)
    for y in range(10):
        for x in range(9):
            nu_board[y, x] = board[y, 8 - x]
    return nu_board

    

def read_gibo(path):
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

    out_info = {}
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
                out_info = {}
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
                    move = move_empty
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

    return out_gibo

#기보 정보를 이용하여 board와 기타 다른 정보를 만든다.
def init_board_from_gibo(gibo):
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
        info['board'] = board
        info['my_first'] = False # 상대(위쪽)부터 시작

    #판이 아니면 무조건 한차림 초차림이 있어야 한다.
    #근데 한포진 초보진도 있다..
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
        board = init_board(my_setting, yo_setting)

        info['board'] = board
        info['my_first'] = True # 나(아래쪽)부터 시작

    return board

def print_board(board):
    for y in range(0, 10):
        line = ''
        for x in range(0, 9):
            stone = board[y, x];

            back_color = Back.BLACK

            if stone == Stone.EMPTY:
                fore_color = Fore.LIGHTBLACK_EX
            elif is_my_stone(stone):
                fore_color = Fore.CYAN
            else:
                fore_color = Fore.MAGENTA

            line += back_color + fore_color + _stone_letters[stone] + ' '

        print(line)

##-------------------------------------------------------------------------------------------

def test_print_board():
    import random
    board = init_board(random.randint(0, 3), random.randint(0, 3))
    print_board(board)

def test_gibo():
    for info in os.walk(args.gibo_path):
        path = info[0]
        files = info[2]

        for file in files:
            file_path = path + '/' + file
            gibos = read_gibo(file_path)
            for gibo in gibos:
                board = init_board_from_gibo(gibo)
                print_board(board)

if __name__ == '__main__':
    test_gibo()

#r
#외통장군을 불러서 100% 이길 수 있는 형태를 학습하는 것이
#도움이 될까?


#게임 클래스가 있긴 있어야겠지
#기보로 게임 돌아가는 거 보면 학습 시작
#reignforcement 학습도 해야겠지?
#기본 전략이랑 싸우는 거.
#랜덤이랑 싸우는 거 하나.
#

