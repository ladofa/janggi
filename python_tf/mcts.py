import game
import numpy as np
# import asyncio
from params import args

#하나의 에피소드는 하나의 mcts 트리를 사용한다.

# get_pos_value = None
# def set_model(model_fn):
#     global get_pos_value
#     def func(board, moves, dum):
#         state = game.get_state(board, dum)
#         print('> model_fn')
#         pos_row, value_row = model_fn(state[None, ...])
#         print('< model_fn')
#         pos = pos_row[[game.get_index(m) for m in moves]]
#         pos = pos / np.sum(pos)
#         value = value_row[0]
#         return pos, value
#     get_pos_value = func

#mcts 트리의 각 노드
class Node:
    puct = 1.0
    temp = 1.0
    #mcts에서 사용할 모델 설정
    #모든 노드가 공통으로 하나의 모델을 사용하므로
    #static 함수로 (self 없음)
   

    def __init__(self, board, turn, parent=None, dum=-1, prev_move=game.MOVE_EMPTY):
        # print('node__init')
        #현재 판의 상태
        self.board = board
        #현재 턴 수
        self.turn = turn
        #부모 노드
        self.parent = parent
        #부모 노드가 존재하면
        if parent:
            #덤은 반대로 승계한다. (상대 덤이 -1.5면 내 덤은 1.5)
            self.dum = -parent.dum
        #부모 노드가 존재하지 않으면 에피소드의 시작 노드(루트)이므로 덤은 그냥 입력으로 받음
        else:
            self.dum = dum
        #현재 상태로 오게 만든 상대방의 움직임
        self.prev_move = prev_move

    def get_state(self):
        state = game.get_state(self.board, self.dum)
        return state

    def init(self, pos, value):
        #게임이 끝났는지 여부
        #1:승리, -1:패배, 0:비김, None:아직 결정되지 않음
        self.finish = game.get_result(self.board)

        #최대 턴에 도달햇다면 점수로 승패를 가른다.
        if self.turn >= args.mcts_max_turn:
            score = game.get_score(self.board) + self.dum
            if score > 0:
                self.finish = 1
            else:
                self.finish = -1

        #게임이 끝난 상태라면 대부분의 값들을 빈칸으로
        self.total_visited = 0
        if self.finish != None:
            #가치값은 무조건 1, 0, -1로 정해짐
            self.value = self.finish
            self.moves = []
            self.pos = []
            self.children = []
            self.total_q = []
            self.avr_q = []
            self.visited = []
            self.phi = []
        #게임이 끝나지 않은 일반적인 경우
        else:
            #현재 상태에서 가능한 움직임
            self.moves = game.get_all_moves(self.board)[0]
            #pos, value 적용
            pos = pos[[game.get_move_index(m) for m in self.moves]]
            self.pos = pos / np.sum(pos)
            self.value = value
            #자식 노드들, 일단 방문을 하지 않은 상태에서는 None으로 하고 방문할 때 생성
            self.children = [None for _ in range(len(self.moves))]

            #자식 노드 방문횟수, 점수 등 ntcs작동에 필요한 값들을 부모 노드에 저장
            self.total_q = np.zeros(len(self.moves), dtype=np.float32) # Q(s, a)
            self.avr_q = np.zeros(len(self.moves), dtype=np.float32) #W(s, a)
            self.visited = np.zeros(len(self.moves), dtype=np.int32) #N(s, a)
            self.phi = np.zeros(len(self.moves), dtype=np.float32) #q + u
            self.recalc_phi = False
            #다른 코루틴이 점령하지 않은 상태
            self.available = np.ones(len(self.moves), dtype=np.float32) #


        #q, w, n 등 주요 변수 접근에 대한 동기 락
        # self.lock = asyncio.Lock()
        self.lock = None
        #available이 없을 때 멈춤
        # self.cond = asyncio.Condition()
        
        # print('exit node__init')
    def deliver(self, i):
        next_board = game.get_next_board_rot(self.board, self.moves[i])
        child = Node(next_board, self.turn + 1, self, -self.dum, self.moves[i])
        self.children[i] = child
        return child

    def deliver_all_1(self):
        states = []
        for i in range(len(self.moves)):
            next_board = game.get_next_board_rot(self.board, self.moves[i])
            child = Node(next_board, self.turn + 1, self, -self.dum, self.moves[i])
            self.children[i] = child
            states.append(child.get_state())
        states = np.array(states)
        return states

    def deliver_all_2(self, pos, value):
        for i in range(len(self.moves)):
            self.children[i].init(pos[i], value[i])

    #down - 방문할 자식 노드
    def get_next_node_index(self):
        self.update_phi()
        return np.argmax(self.phi)

    #phi값을 새로 계산.
    def update_phi(self):
        if self.recalc_phi:
            q = self.avr_q
            u = Node.puct * self.pos * np.sqrt(np.sum(self.visited)) / (1 + self.visited)
            self.phi = q + u
            self.recalc_phi = False
    
    # async def async_get_next_node_index(self):
    #     while True:
    #         async with self.lock:
    #             self.update_phi()
    #             if sum(self.available) != 0:
    #                 i = np.argmax(self.phi * self.available)
    #                 if self.children[i] == None:
    #                     self.available[i] = 0
    #                 return i
    #         await asyncio.sleep(0.03)

    #up - 자식노드로부터 계산된 value값을 업데이트
    def add_value(self, i, value):
        self.total_visited += 1
        self.visited[i] += 1
        self.total_q[i] += value
        self.avr_q[i] = self.total_q[i] / self.visited[i]
        self.recalc_phi = True
        

    # async def async_add_value(self, i, value):
    #     async with self.lock:
    #         self.visited[i] += 1
    #         self.total_q[i] += value
    #         self.avr_q[i] = self.total_q[i] / self.visited[i]
    #         self.available[i] = 1
    #         self.recalc_phi = True

    #최종 결정
    def get_choice(self):
        if Node.temp == 0:
            return np.argmax(self.visited)
        else:
            v = self.visited ** (1 / Node.temp)
            v = v / np.sum(v)
            return np.random.choice(range(len(v)), p=v)

    #최종 결정 - temp가 1일 경우
    def get_choice1(self):
        v = self.visited / np.sum(self.visited)
        return np.random.choice(range(len(v)), p=v)

    #최종 결정 - temp가 0일 경우
    def get_choice0(self):
        return np.argmax(self.visited)


class Mcts:
    def __init__(self, board, model_fn=None, acq_fn=None, info = None):
        self.root = Node(board, 0, parent=None, dum=-1)
        self.history = []
        self.travel_count = 0
        self.model_fn = model_fn
        self.acq_fn = acq_fn
        self.info = info
    
    def init(self):
        state = self.root.get_state()
        self.model_fn([state], run_now=True)
        pos, value = self.acq_fn()
        self.root.init(pos[0], value[0])

    #하나의 노드를 방문한다.
    def travel_once(self):
        node = self.root
        visited_nodes = []
        selections = []
        while True:
            # print('travel_once')
            if node.finish != None:
                leaf_value = -node.finish        
                node.total_visited += 1
                break
            visited_nodes.insert(0, node)
            #하나의 노드 방문
            if False:
                i = node.get_next_node_index()
                selections.insert(0, i)

                # 자식 노드 방문
                if node.children[i] != None:
                    node = node.children[i]
                # leaf node - 출산
                else:
                    node = node.deliver(i)
                    leaf_value = -node.value
                    break
            #한꺼번에 모든 노드 개방
            else:
                if node.total_visited == 0:
                    states = node.deliver_all_1()
                    self.model_fn(states)
                    result = self.acq_fn()
                    node.deliver_all_2(*result)
                    i = node.get_next_node_index()
                    selections.insert(0, i)
                    node = node.children[i]
                    leaf_value = -node.value
                    break
                else:
                    i = node.get_next_node_index()
                    selections.insert(0, i)
                    node = node.children[i]
                    leaf_value = -node.value

        #부모 노드 방향으로 방문 기록 업데이트
        for node, i in zip(visited_nodes, selections):
            node.add_value(i, leaf_value)
            leaf_value = -leaf_value

        self.travel_count += 1

    # def async_travel_once(self):
    #     node = self.root
    #     visited_nodes = []
    #     selections = []
    #     while True:
    #         if node.finish != None:
    #             leaf_value = -node.value
    #             break
    #         visited_nodes.insert(0, node)
    #         i = node.async_get_next_node_index()
    #         selections.insert(0, i)

    #         # 자식 노드 방문
    #         if node.children[i] != None:
    #             node = node.children[i]
    #         # leaf node - 출산
    #         else:
    #             node = node.async_deliver(i)
    #             leaf_value = -node.value
    #             break

    #     for node, i in zip(visited_nodes, selections):
    #         node.async_add_value(i, leaf_value)
    #         leaf_value = -leaf_value

    #    self.travel_count += 1
    
    def travel(self, n):
        for _ in range(n):
            self.travel_once()

    def move(self, i=-1):
        node = self.root
        
        if node.finish != None:
            return node.finish

        if i == -1:
            i = node.get_choice1()
        if node.children[i] == None:
            child = node.deliver(i)
        else:
            child = node.children[i]
        
        self.history.append((node.board, node.moves[i], node.dum))
        self.root = child
        self.travel_count = 0
        child.parent = None #garbage collecting

        return child.finish

    def get_samples(self):
        samples = []
        if self.root.finish == None:
            return None

        if self.root.turn % 2 == 0: #마지막 진 쪽이 선수라면
            my_win = self.root.finish
        else:
            my_win = -self.root.finish

        for board, move, dum in self.history:
            p = game.get_move_index(move)
            state = game.get_state(board, dum)
            v = my_win
            samples.append((state, p, v))

            flip_state = np.flip(state, axis=1)
            flip_move = game.get_flip_move(move)
            flip_p = game.get_move_index(flip_move)
            samples.append((flip_state, flip_p, v))

            my_win = -my_win
        return samples
    

def explore(mcts):
    node = mcts.root
    history = [node]
    move = []
    while True:
        if node != None:
            # print(node.board)            
            if node.turn % 2 == 0:
                board = node.board
                prev_move = game.rot_move(node.prev_move)
                moves = node.moves
            else:
                board = -np.flip(node.board)
                prev_move = node.prev_move
                moves = game.rot_moves(node.moves)
            if prev_move == game.MOVE_EMPTY:
                prev_move = []

            for i, m in enumerate(moves):
                print('[%.2d] %.2f %4.2f/%.4d' % (i, node.pos[i], node.total_q[i], node.visited[i]), ' ', m)

            game.print_board(board, prev_move, move)

            print('value : ', node.value, 'turn : ', node.turn)

        else:
            print('NONE')
        key = input('?').lower()
        if key == '':
            pass
        elif key.isdigit():
            num = int(key)
            move = moves[num]
            
        elif key[0] == 'g':
            num = int(key[1:])
            node = node.children[num]
            history.append((node))
            move = []
        elif key == 'b':
            if len(history) > 1:
                del history[-1]
                node = history[-1]
        elif key[0] == 'x' or key[0] == 'q':
            if len(key) > 1:
                return int(key[1:])                
            else:
                return -1
        


if __name__ == '__main__':
    board = game.get_init_board(0, 0)
    mcts = Mcts(board)
    while True:
        mcts.travel(100)
        n = explore(mcts)
        if n == -2:
            break
        mcts.move(n)

