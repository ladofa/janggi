# TCP server example
import socket
import numpy as np
import time
import struct

import tensor_networks as tn
import move_transfer as mt




#네트워크를 string라벨 붙여서 저장
time_recv = 0
time_train = 0


############################################################################3
#데이터 송수신 및 변환

def recv_bytes(socket, size_):
	received = socket.recv(size_)
	while True:
		size = size_ - len(received)
		if size == 0:
			return received
		else:
			received = received + socket.recv(size)

def send_ok(socket):
	socket.send(bytes([101, 0]))

def send_failed(socket):
	socket.send(bytes([102, 0]))

def recv_string(socket):
	size = socket.recv(1)
	str = socket.recv(size[0])
	return str.decode()

def recv_board(socket):
	received = recv_bytes(socket, 10 * 9 * tn.input_channels)
	raw = np.frombuffer(received, dtype=np.uint8)
	ret = np.reshape(raw, [10, 9, tn.input_channels])
	return ret

def recv_move(socket):
	received = socket.recv(2)
	move = np.zeros([10, 9, 2], dtype=np.uint8)

	#case of empty
	if received[0] == 255:
		return move

	fy = received[0] // 9
	fx = received[0] % 9
	ty = received[1] // 9
	tx = received[1] % 9

	move[fy, fx, 0] = 1
	move[ty, tx, 1] = 1

	return move

def recv_moves(socket):
	moves = []
	size = socket.recv(1)
	for i in range(size[0]):
		moves.append(recv_move(socket))
	return moves

def send_proms(socket, proms):
	msg2 = bytes([int(p * 255) for p in proms])
	msg = msg2
	socket.send(msg)

def recv_judge(socket):
	msg = socket.recv(4)
	judge = struct.unpack('f', msg)
	return judge

def send_judge(socket, judge):
	msg = struct.pack('f', judge)
	socket.send(msg)

def recv_policy_train_data(socket):
	#255개 단위로 들어옴.
	size_255 = socket.recv(1)
	size = size_255[0]
	data_board = [None] * size
	data_move = [None] * size
	for i in range(size):
		data_board[i] = (recv_board(socket))
		data_move[i] = (recv_move(socket))
	return {'board':data_board, 'move':data_move}

def recv_value_train_data(socket):
	size = socket.recv(1)[0]
	data_board = [None] * size
	data_judge = [None] * size
	for i in range(size):
		data_board[i] = (recv_board(socket))
		data_judge[i] = (recv_judge(socket))
	return {'board':data_board, 'judge':data_judge}

###########################################################################
#각 명령에 대한 처리 함수
def proc_check(kind, socket):
	send_ok(socket)

def proc_save(kind, socket):
	print("save...")
	if kind == 1:
		tn.save_policy()
	else:
		tn.save_value()
	send_ok(socket)
	print("save OK.")

def proc_evaluate(kind, socket):
	if kind == 1:
		board = recv_board(socket)
		proms = tn.eval_policy(board)
		send_ok(socket)
		send_proms(socket, proms[0])
	else:
		board = recv_board(socket)
		judge = proms = tn.eval_value(board)
		send_ok(socket)
		send_judge(socket, judge[0])

time_recv = 0
time_train = 0

def proc_train(kind, socket):
	if kind == 1:
		policy_train_data = recv_policy_train_data(socket)		
		loss, gs, move_from, move_to = tn.train_policy(policy_train_data)
		print('%d train : %.3lf' % (gs, loss))
	else:
		value_train_data = recv_value_train_data(socket)
		loss, gs, judge = tn.train_value(value_train_data)
		print('%d \t\t\t\t\t\tvalid : %.3lf' % (gs, loss))

	send_ok(socket)

############################################################
##########################################################
# 메인


print("start to setting server up...")
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(("", 9999))
server_socket.listen(10)

print ("TCPServer Waiting for client on port 5000")

while 1:
	#받아서
	client_socket, address = server_socket.accept()
	print ("I got a connection from ", address)

	while 1:
		header = client_socket.recv(2)
		if not header : break
		code = header[0]
		kind = header[1]

		if code == 1:
			proc_check(kind, client_socket)
		elif code == 4:
			proc_save(kind, client_socket)
		elif code == 5:
			proc_evaluate(kind, client_socket)
		elif code == 6:
			proc_train(kind, client_socket)

	
	#data = input('SEND( TYPE q or Q to Quit):')
	#if(data == 'Q' or data == 'q'):
	#	client_socket.send (data.encode())
	#	client_socket.close()
	#	break;
	#else:
	#	client_socket.send(data.encode())
		
		
	#if(data == 'q' or data == 'Q'):
	#	client_socket.close()
	#	break;
	#else:
server_socket.close()
print("SOCKET closed... END")