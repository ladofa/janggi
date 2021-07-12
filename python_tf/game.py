from enum import IntEnum
import numpy as np
import random
import os

from colorama import Fore, Back, Style
import colorama
colorama.init(autoreset=True)

from params import args


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

    #8bit unsigned로 cast하면
    YO_JOL = -1,#255
    YO_SANG = -2,#254
    YO_MA = -3,#253
    YO_PO = -4,#252
    YO_CHA = -5,#251
    YO_SA = -6,#250
    YO_KING = -7,#249

STONE_SCORE = np.zeros((256), dtype=np.float32)
STONE_SCORE[0:8] = [0, 2, 3, 5, 7, 13, 3, 10000]
#249 ~ 255까지
STONE_SCORE[-7:] = [-10000, -3, -13, -7, -5, -3, -2]

_stone_letters = [
    "＋", "졸", "상","마", "포", "차", "사","초", "兵", "象", "馬", "包", "車", "士", "楚"
	]

#안 쓰는 함수들.
#최적화를 위해 inline으로 직접 사용

#def is_my(stone):
#    return stone > 0

#def is_yo(stone):
#    return stone < 0

#def get_opposite_stone(stone):
#    return -stone

#def is_opposite(s1, s2):
#    return s1 * s2 < 0
    


# 장기판은 board로 칭함
# board는 10 x 9개의 Stone이라고 할 수 있다. 10 x 9 의 numpy array로 나타냄
# 장기판의 크기 10, 9는 절대불변이므로 그냥 매직 넘버로 사용

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
POS_EMPTY = (10, 0)

# 널 무브.. 마찬가지로.
MOVE_EMPTY = (POS_EMPTY, (0, 0))


# pos를 1 byte로 나타낸 것을 point라고 칭함. point = pos.y * 9 + pos.x
def get_point(pos):
    return pos(0) * 9 + pos(1)

def get_pos(point):
    return (point // 9, point % 9)

def in_board(pos):
    return pos[0] >= 0 and pos[1] >= 0 and pos[0] < 10 and pos[1] < 9



#게임판 초기셋팅
def get_init_board(my_setting, yo_setting):
    board = np.zeros([10, 9], dtype = np.int8)
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

    #위쪽의 경우에도 아래쪽에서 보는 관점으로.
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
    리턴값 : 내 기물 점수 - 상대 기물 점수
    '''
    return np.sum(STONE_SCORE[board.astype(np.uint8)])

#게임이 끝난 상태인지 확인
def get_result(board):
    my_area = board[7:]
    yo_area = board[:3]
    if not np.any(my_area == Stone.MY_KING):
        #아래쪽에 초는 없고 한만 있으면 빅장을 당한 것이다.
        if np.any(my_area == Stone.YO_KING):
            return 0
        #나(초)의 패배
        else:
            return -1
    elif not np.any(yo_area == Stone.YO_KING):
        #빅장
        if np.any(yo_area == Stone.MY_KING):
            return 0
        #나의승리
        else:
            return 1
    #게임이 끝나지 않음
    else:
        return None


def get_next_board(board, move):
    nu_board = np.copy(board)
    if move != MOVE_EMPTY:
        pos_from, pos_to = move
        stone_from = nu_board[pos_from]
        nu_board[pos_from] = Stone.EMPTY
        nu_board[pos_to] = stone_from
    return nu_board

#move를 적용한 다음 보드를 리턴, rotation 포함 
def get_next_board_rot(board, move):
    
    nu_board = -np.flip(board)
    move = rot_move(move)
    if move != MOVE_EMPTY:
        pos_from, pos_to = move
        stone_from = nu_board[pos_from]
        nu_board[pos_from] = 0
        nu_board[pos_to] = stone_from

    return nu_board




# get_possible_to에서 쓰는 상수
# 마길
_way_ma = [
    #현재 위치를 (0, 0)으로 볼떄
    #위로, 왼쪽 위로
    [(-1, 0), (-2, -1)],
    #위로, 오른쪽 위로
    [(-1, 0), (-2, 1)],
    #기타 등등..
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
        [[(0, 1), (1, 0), (1, 1)], [(0, -1), (1, 0), (0, 1)], [(0, -1), (1, -1), (1, 0)]],
        [[(-1, 0), (0, 1), (1, 0)], [(-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1)], [(-1, 0), (0, -1), (1, 0)]],
        [[(-1, 0), (-1, 1), (0, 1)], [(0, -1), (-1, 0), (0, 1)], [(0, -1), (-1, -1), (-1, 0)]]
    ]

_way_jol = [(0, -1), (0, 1), (-1, 0)]


def get_possible_pos_to(board, pos_from):
    '''
    특정한 위치(pos_from)에서 움직일 수 있는 리스트를 출력한다.
    항상 초(하단)의 입장에서 본다.
    '''

    if board[pos_from] <= 0:
        raise Exception()

    py = pos_from[0]
    px = pos_from[1]

    stone_from = board[pos_from]

    #가능한 도착 지점
    pos_to_all = []

    #가능한 도착 지점의 부분집합. 특별히 상대 기물을 취할 수 있는 지점
    pos_to_take = []

    #아군을 타깃으로 하는 움직임. 아군을 지킬 수 있음
    #사실 당장 possible은 아님.
    pos_to_protect = []

    #멱 잡힌 부분
    #pos_to_throat = []

    def add_pos_to(pos_to):
        stone_to = board[pos_to]
        if stone_to == 0:
            pos_to_all.append(pos_to)
        elif stone_to < 0:
            pos_to_all.append(pos_to)
            pos_to_take.append(pos_to)
        else:
            pos_to_protect.append(pos_to)

    if stone_from == Stone.MY_CHA :
        def visit_cha(pos_to):
            stone_to = board[pos_to]
            #비어있으면 계속 진행가능
            if stone_to == 0:
                pos_to_all.append(pos_to)
                return True 
            #상대기물이면 여기까지 진행가능
            elif stone_to < 0:
                pos_to_all.append(pos_to)
                pos_to_take.append(pos_to)
                return False
            #내 기물이면 멱이므로 진행 불가능
            else:
                pos_to_protect.append(pos_to)
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
        #궁중에서 움직임
        elif px == 4 and (py == 1 or py == 8):
            add_pos_to((py + 1, px + 1))
            add_pos_to((py + 1, px - 1))
            add_pos_to((py - 1, px + 1))
            add_pos_to((py - 1, px - 1))


    elif stone_from == Stone.MY_PO:
        dari = [False] #다리를 못 밟았으면 False, 다리를 밟았으면 True
        def visit_po(pos_to):
            stone_to = board[pos_to]
            if not dari[0]:
                if stone_to == 0:
                    return True
                #포만 아니면 다리로 삼을 수 있다.
                elif stone_to != Stone.MY_PO and stone_to != Stone.YO_PO:
                    dari[0] = True
                    return True
                else:
                    return False
            else:
                #빈칸은 전진 가능
                if stone_to == 0:
                    pos_to_all.append(pos_to)
                    return True
                #상대 포는 아무것도 안 됨
                elif stone_to == Stone.YO_PO:
                    return False
                #상대 기물이면서 포가 아니면 먹을 수 있다.
                elif stone_to < 0:
                    pos_to_all.append(pos_to)
                    pos_to_take.append(pos_to)
                    return False
                #내 기물이면
                else:
                    pos_to_protect.append(pos_to)
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

    elif stone_from == Stone.MY_MA:
        for way in _way_ma:
            pos_to = (py + way[1][0], px + way[1][1])
            pos_myuk = (py + way[0][0], px + way[0][1])

            # 보드 범위를 벗어나는 경우 X
            if not in_board(pos_to):
                continue

            # 멱에 막히는 경우
            if board[pos_myuk] != 0:
                continue

            add_pos_to(pos_to)
            
    elif stone_from == Stone.MY_SANG:
        for way in _way_sang:
            pos_to = (py + way[2][0], px + way[2][1])
            pos_myuk1 = (py + way[0][0], px + way[0][1])
            pos_myuk2 = (py + way[1][0], px + way[1][1])

            # 보드 범위를 벗어나는 경우 X
            if not in_board(pos_to):
                continue

            # 멱에 막히는 경우
            if board[pos_myuk1] != 0 or board[pos_myuk2] != 0:
                continue

            add_pos_to(pos_to)

    elif stone_from == Stone.MY_SA  or stone_from == Stone.MY_KING:        
        relative_y = pos_from[0] - 7
        relative_x = pos_from[1] - 3        

        ways = _way_goong[relative_y][relative_x]
        for way in ways:
            pos_to = (py + way[0], px + way[1])
            add_pos_to(pos_to)

    elif stone_from == Stone.MY_JOL:
        for way in _way_jol:
            pos_to = (py + way[0], px + way[1])
            if not in_board(pos_to):
                continue
            add_pos_to(pos_to)
        
        #궁 안에서 움직임 추가
        #귀에 있을 때
        if pos_from == (2, 3) or pos_from == (2, 5):
            add_pos_to((1, 4))
        #중앙에 있을 때
        elif pos_from == (1, 4):
            add_pos_to((0, 3))
            add_pos_to((0, 5))
    else:
        raise Exception('unexpected kind of stone')

    return pos_to_all, pos_to_take, pos_to_protect

def get_all_moves(board):
    moves = []
    takes = []
    prots = []
    for y in range(10):
        for x in range(9):
            stone_from = board[y, x]
            if stone_from > 0:
                pos_to_all, pos_to_take, pos_to_protect = get_possible_pos_to(board, (y, x))
                moves += [((y, x), pt) for pt in pos_to_all]
                takes += [((y, x), pt) for pt in pos_to_take]
                prots+= [((y, x), pt) for pt in pos_to_protect]
    moves.append(MOVE_EMPTY)
    return moves, takes, prots

#가능한 움직임인지 확인한다.
# def is_it_possible(board, pos_from, pos_to):
#     stone_from = board[pos_from]

#     if stone_from == Stone.MY_CHA:
#         if pos_from[0] == pos_to[0]:
#             for x in range(pos_from[x])
# 일단 보류하고    


# 판 위 아래 뒤집기
# AI는 무조건 아래쪽 입장에서 생각하기 때문에 위쪽 진영을 위한 전략을 AI에게 물어볼 때, 뒤집어서 물어본다.
# value network는 판을 뒤집지 않고 결과의 +-를 반대로 생각하면 된다.

# inline 사용
# def rot_board(board):
#     nu_board = -np.flip(board)
#     return nu_board

#move를 판과 같이 뒤집어준다ㅏ.
def rot_move(move):
    if move == MOVE_EMPTY:
        return move
    pos_from, pos_to = move
    return (9 - pos_from[0], 8 - pos_from[1]), (9 - pos_to[0], 8 - pos_to[1])

#좌우반전
def get_flip_move(move):
    if move == MOVE_EMPTY:
        return move
    p0, p1 = move
    return (p0[0], 8 - p0[1]), (p1[0], 8 - p1[1])

def rot_moves(moves):
    nu_moves = [rot_move(move) for move in moves]
    return nu_moves

# 장기 특성상 좌우를 바꿔도 동일하다.
# 학습 샘플의 개수를 두 배로 늘릴 수 있다.

# inline 사용
# def flip_lr(board):
#     return np.fliplr(board)

stones = [Stone.EMPTY, Stone.MY_JOL, Stone.MY_SANG, Stone.MY_MA, Stone.MY_PO, Stone.MY_CHA,
 Stone.MY_SA, Stone.MY_KING, Stone.YO_JOL , Stone.YO_SANG , Stone.YO_MA , Stone.YO_PO,
 Stone.YO_CHA , Stone.YO_SA , Stone.YO_KING]
# stones = np.array(stones, dtype=int8)

def get_state(board, dum, dtype=np.float32):
    state = np.full((10, 9, 16), -1, dtype=dtype)
    for i in range(15):
        state[..., i][board == stones[i]] = 1
    
    if dum > 0:
        state[-1].fill(1)
    return state

def get_move_index(move):
    p0, p1 = move
    #index = np.unravel_index((p0[0], p0[1], p1[0], p1[1]), (10, 9, 10, 9))
    index = p0[0] * 810 + p0[1] * 90 + p1[0] * 9 + p1[1]
    return index

    

_star_mark_pos = [
    (2, 1), (2, 7), (3, 0), (3, 2), (3, 4), (3, 6), (3, 8),
    (7, 1), (7, 7), (6, 0), (6, 2), (6, 4), (6, 6), (6, 8)
    ]
def print_board(board, yellow_back = [], green_back = [], magenta_back = [], cyan_back = []):
    for y in range(0, 10):
        line = ''
        for x in range(0, 9):
            stone = board[y, x]

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
            elif stone > 0:
                fore_color = Fore.LIGHTBLUE_EX
            else:
                fore_color = Fore.LIGHTRED_EX

            if stone == 0 and (y, x) in _star_mark_pos:
                stone_letter = '＊'
            elif stone >= 0:
                stone_letter = _stone_letters[stone]
            else:
                stone_letter = _stone_letters[abs(stone) + 7]

            line += back_color + fore_color + stone_letter
            line += Back.BLACK + ' '

        print(line)

##-------------------

# reward_lookup = [-1]
# last = 1
# for _ in range(200):
#     last = last * 0.95
#     reward_lookup.append(last)


class Replay:
    '''
    학습을 위한 게임 플레이 데이터를 만든다.
    한 입장의 착수는 판을 돌려서 초 입장으로 바꾼다.
    '''
    def __init__(self, init_board, moves, win=1, check_legal=False):
        self.board = init_board
        self.moves = moves
        self.win = win

        #첫 번째 시작이 한이면 초로 바꿈
        first_move = self.moves[0]
        from_pos = first_move[0]

        if from_pos != POS_EMPTY:
            if self.board[from_pos] < 0:
                self.board = -np.flip(self.board)
                self.moves = rot_moves(self.moves)
                self.win = -win
        
        self.check_legal = check_legal

        

    def iterator(self):
        '''
        현재 보드, 다음 착수를 리턴
        '''

        cur_board = self.board.copy()
        win = self.win
        dum = -1.5
        turn2 = len(self.moves) / 2
        for turn, move in enumerate(self.moves):
            #상대방 차례라면
            if turn % 2 == 1:
                move = rot_move(move)

            if self.check_legal:
                legal_moves = get_all_moves(cur_board)[0]
                if move not in legal_moves:
                    print_board(cur_board)
                    print(move)
                    raise Exception(cur_board, move)
                    # break
        
            yield cur_board, dum, move, (win * turn / turn2 if turn < turn2 else win)

            # 다음 보드를 만든다.
            cur_board = get_next_board_rot(cur_board, move)
            win = -win
            dum = -dum
            # next_board = cur_board.copy()
            # if move != MOVE_EMPTY:
            #     pos_from = move[0]
            #     pos_to = move[1]

            #     next_board[pos_to] = next_board[pos_from]
            #     next_board[pos_from] = 0

            # # next_board = rot_board(next_board)
            # cur_board = next_board
            

def test_print_board():
    import random
    board = get_init_board(random.randint(0, 3), random.randint(0, 3))
    print_board(board)

if __name__ == '__main__':
    #test_print_board()
    x = np.arange(9)
    y = list(map(lambda k: k + 1, x))
    print(y)


#r
#외통장군을 불러서 100% 이길 수 있는 형태를 학습하는 것이
#도움이 될까?


#게임 클래스가 있긴 있어야겠지
#기보로 게임 돌아가는 거 보면 학습 시작
#reignforcement 학습도 해야겠지?
#기본 전략이랑 싸우는 거.
#랜덤이랑 싸우는 거 하나.
#

