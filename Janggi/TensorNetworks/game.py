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

STONE_SCORE = [0, 2, 3, 5, 7, 13, 3, 10000, -2, -3, -5, -7, -13, -3, -10000]

_stone_letters = [
    "＋", "졸", "상","마", "포", "차", "사","초", "兵", "象", "馬", "包", "車", "士", "楚"
	]

def is_my(stone):
    return 1 <= stone <= 7
def is_yo(stone):
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


# 장기판위의 위치, position - pos 는 (y, x)의 튜플로 나타냄 - 기보 노테이션과 비교해서 
# x y 순서는 같은데 0부터 시작하는 점에 유의
# move는 (pos, pos)로 나타냄. 어떤 말이 움직여는지에 대한 정보는 포함하지 않음.

# 널 포지션... 쓸데없는 것 같지만 필요함
POS_EMPTY = (12, 27)

# 널 무브.. 마찬가지로.
MOVE_EMPTY = (POS_EMPTY, POS_EMPTY)


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

def get_score(board):
    '''
    장기 점수를 계산
    value network 대신 쓸 수 있다.
    '''
    score = 0
    for y in range(10):
        for x in range(9):
            score += STONE_SCORE[board[y, x]]
    return score

def get_next_board(board, move):
    '''
    move를 적용한 다음 보드를 리턴
    '''
    pos_from = move[0]
    pos_to = move[1]
    nu_board = np.copy(board)
    stone_from = nu_board[pos_from]
    nu_board[pos_from] = Stone.EMPTY
    nu_board[pos_to] = stone_from

    return nu_board




# get_possible_to에서 쓰는 상수
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

# 특정한 위치(pos_from)에서 움직일 수 있는 리스트를 출력한다.
# 항상 초(하단)의 입장에서 본다.
def get_possible_pos_to(board, pos_from):

    if board[pos_from] == 0:
        raise Exception()
    elif is_yo(board[pos_from]):
        raise Exception()

    py = pos_from[0]
    px = pos_from[1]

    stone_from = board[pos_from]

    #가능한 도착 지점
    pos_to_all = []
    #가능한 도착 지점의 부분집합. 특별히 상대 기물을 취할 수 있는 지점
    pos_to_take = []

    def add_pos_to(pos_to):
        stone_to = board[pos_to]
        if stone_to == Stone.EMPTY:
            pos_to_all.append(pos_to)
        elif is_yo(stone_to):
            pos_to_all.append(pos_to)
            pos_to_take.append(pos_to)

    if stone_from == Stone.MY_CHA :
        def visit_cha(pos_to):
            stone_to = board[pos_to]
            #비어있으면 계속 진행가능
            if stone_to == Stone.EMPTY:
                pos_to_all.append(pos_to)
                return True 
            #상대기물이면 여기까지 진행가능
            elif is_yo(stone_to):
                pos_to_all.append(pos_to)
                pos_to_take.append(pos_to)
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
            if not visit_cha((py, x)):
                break

        #궁 귀에서 대각선 움직임
        if px == 3 and (py == 0 or py == 7):
            if visit_cha((py + 1, px + 1)):
                visit_cha((py + 2, px + 2))
        elif px == 3 and (py == 2 or py == 9):
            if visit_cha((py - 1, px + 1)):
                visit_cha((py - 2, px + 2))
        elif px == 5 and (py == 0 or py == 7):
            if visit_cha((py + 1, px - 1)):
                visit_cha((py + 2, px - 2))
        elif px == 5 and (py == 2 or py == 9):
            if visit_cha((py - 1, px - 1)):
                visit_cha((py - 2, px - 2))
        elif px == 4 and (py == 1 or py == 8):
            add_pos_to((py + 1, px + 1))
            add_pos_to((py + 1, px - 1))
            add_pos_to((py - 1, px + 1))
            add_pos_to((py - 1, px - 1))

        #궁중에서 대각선


    elif stone_from == Stone.MY_PO or stone_from == Stone.YO_PO:
        dari = [False] #다리를 못 밟았으면 False, 다리를 밟았으면 True
        def visit_po(pos_to):
            stone_to = board[pos_to]
            #포를 만나면 다리고 뭐고 그냥 멈춤
            if stone_to == Stone.MY_PO or stone_to == Stone.YO_PO:
                return False
            if not dari[0]:
                if stone_to == Stone.EMPTY:
                    return True
                else:
                    dari[0] = True
                    return True
            else:
                if stone_to == Stone.EMPTY:
                    pos_to_all.append(pos_to)
                    return True
                elif is_yo(stone_to):
                    pos_to_all.append(pos_to)
                    pos_to_take.append(pos_to)
                    return False
                else:
                    return False

        #상하좌우 방향으로 살핀다.
        dari[0] = False
        for y in range(py - 1, -1, -1):
            if not visit_po((y, px)):
                break

        dari[0] = False
        for y in range(py + 1, 10):
            if not visit_po((y, px)):
                break

        dari[0] = False
        for x in range(px - 1, -1, -1):
            if not visit_po((py, x)):
                break

        dari[0] = False
        for x in range(px + 1, 9):
            if not visit_po((py, x)):
                break

        dari[0] = False
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

            add_pos_to(pos_to)
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

            add_pos_to(pos_to)

    elif stone_from == Stone.MY_SA  or stone_from == Stone.MY_KING:        
        relative_y = pos_from[0] - 7
        relative_x = pos_from[1] - 3        

        ways = _way_goong[relative_y][relative_x]
        for way in ways:
            pos_to = (py + way[0], px + way[1])
            stone_to = board[pos_to]
            if stone_to == Stone.EMPTY:
                pos_to_all.append(pos_to)
            elif is_yo(stone_to):
                pos_to_all.append(pos_to)
                pos_to_take.append(pos_to)

    elif stone_from == Stone.MY_JOL:
        for way in _way_my_jol:
            pos_to = (py + way[0], px + way[1])
            if not in_board(pos_to):
                continue
            add_pos_to(pos_to)
        
        #궁 안에서 움직임 추가
        if pos_from == (2, 3) or pos_from == (2, 5):
            add_pos_to((1, 4))
        elif pos_from == (1, 4):
            add_pos_to(0, 3)
            add_pos_to(0, 5)
    else:
        raise Exception('unexpected kind of stone')

    return pos_to_all, pos_to_take

def get_all_moves(board):
    moves = []
    takes = []
    for y in range(10):
        for x in range(9):
            stone_from = board[y, x]
            if is_my(stone_from):
                pos_to_all, pos_to_take = get_possible_pos_to(board, (y, x))
                moves += [((y, x), pt) for pt in pos_to_all]
                takes += [((y, x), pt) for pt in pos_to_take]
    return moves, takes




# 판 위 아래 뒤집기
# AI는 무조건 아래쪽 입장에서 생각하기 때문에 위쪽 진영을 위한 전략을 AI에게 물어볼 때, 뒤집어서 물어본다.
# value network는 판을 뒤집지 않고 +-를 반대로 생각해도 되지만 policy network는 무조건 뒤집을 수 밖에 없다.
def rot_board(board):
    nu_board = np.zeros([10, 9], np.uint8)
    #위치도 바꿔주고 세력도 바꿔준다. (한->초, 초->한)
    for y in range(10):
        for x in range(9):
            nu_board[y, x] = get_opposite_stone(board[9 - y, 8 - x])

    return nu_board

#move를 판과 같이 뒤집어준다ㅏ.
def rot_move(move):
    if move == MOVE_EMPTY:
        return move
    pos_from = move[0]
    pos_to = move[1]
    return (9 - pos_from[0], 8 - pos_from[1]), (9 - pos_to[0], 8 - pos_to[1])

def rot_moves(moves):
    nu_moves = []
    for move in moves:
        nu_moves.append(rot_move(move))
    return nu_moves

# 장기 특성상 좌우를 바꿔도 동일하다.
# 학습 샘플의 개수를 두 배로 늘릴 수 있다.
def flip_lr(board):
    nu_board = np.zeros([10, 9], np.uint8)
    for y in range(10):
        for x in range(9):
            nu_board[y, x] = board[y, 8 - x]
    return nu_board

_star_mark_pos = [
    (2, 1), (2, 7), (3, 0), (3, 2), (3, 4), (3, 6), (3, 8),
    (7, 1), (7, 7), (6, 0), (6, 2), (6, 4), (6, 6), (6, 8)
    ]
def print_board(board, yellow_back = [], green_back = [], magenta_back = [], cyan_back = []):
    for y in range(0, 10):
        line = ''
        for x in range(0, 9):
            stone = board[y, x];

            if (y, x) in yellow_back:
                back_color = Back.YELLOW
            elif (y, x) in green_back:
                back_color = Back.GREEN
            elif (y, x) in magenta_back:
                back_color = Back.MAGENTA
            elif (y, x) in cyan_back:
                back_color = Back.CYAN
            else:
                back_color = Back.BLACK

            if stone == Stone.EMPTY:
                if (y, x) in _star_mark_pos:
                    fore_color = Fore.WHITE
                else:
                    fore_color = Fore.LIGHTBLACK_EX
            elif is_my(stone):
                fore_color = Fore.LIGHTBLUE_EX
            else:
                fore_color = Fore.LIGHTRED_EX

            if stone == Stone.EMPTY and (y, x) in _star_mark_pos:
                stone_letter = '＊'
            else:
                stone_letter = _stone_letters[stone]

            line += back_color + fore_color + stone_letter
            line += Back.BLACK + ' '

        print(line)

##-------------------

#학습을 위한 게임
#수를 놓을 때마다 판을 돌리기 때문에
#게임 플레이에는 부적합..
class Replay:
    def __init__(self, init_board, moves):
        self.board = init_board
        self.moves = moves

        #첫 번째 시작이 한이면 초로 바꿈
        first_move = self.moves[0]
        from_pos = first_move[0]

        if is_yo(self.board[from_pos]):
            self.board = rot_board(self.board)
            self.moves = rot_moves(self.moves)

    def iterator(self):
        '''
        현재 보드, 다음 착수를 리턴
        '''

        cur_board = self.board.copy()
        for turn, moves in enumerate(self.moves):
            move = self.moves[turn]
            #상대방 차례라면
            if turn % 2 == 1:
                move = rot_move(move)
        
            yield cur_board, move

            # 다음 보드를 만든다.
            if move != MOVE_EMPTY:
                pos_from = move[0]
                pos_to = move[1]

                next_board = cur_board.copy()
                next_board[pos_to] = next_board[pos_from]
                next_board[pos_from] = 0

            next_board = rot_board(next_board)
            cur_board = next_board

def test_print_board():
    import random
    board = init_board(random.randint(0, 3), random.randint(0, 3))
    print_board(board)

if __name__ == '__main__':
    test_print_board()


#r
#외통장군을 불러서 100% 이길 수 있는 형태를 학습하는 것이
#도움이 될까?


#게임 클래스가 있긴 있어야겠지
#기보로 게임 돌아가는 거 보면 학습 시작
#reignforcement 학습도 해야겠지?
#기본 전략이랑 싸우는 거.
#랜덤이랑 싸우는 거 하나.
#

