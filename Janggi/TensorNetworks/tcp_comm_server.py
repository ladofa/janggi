# TCP server example
import socket
import tensor_networks as tn
import move_transfer as mt

#네트워크를 string라벨 붙여서 저장
policy_networks = {}
value_networks = {}

############################################################################3
#데이터 송수신 및 변환
def send_ok(socket):
	socket.send(bytes([101, 0]))

def send_failed(socket):
	socket.send(bytes([102, 0]))

def recv_string(socket):
	size = socket.recv(1)
	str = socket.recv(size[0])
	return str.decode()

def recv_board(socket):
	str = socket.recv(33)
	return mt.board_str2layers(str)

def recv_move(socket):
	str = socket.recv(2)
	return mt.move_str2layers(str)

def recv_moves(socket):
	moves = []
	size = socket.recv(1)
	for i in range(size[0]):
		moves.append(recv_move(socket))
	return moves

def send_proms(socket, proms):
	msg = bytes([len(proms)]) + bytes([int(p * 255) for p in proms])
	socket.send(msg)

def recv_judge(socket):
	msg = socket.recv(1)
	judge = msg[0] / 255.0
	return judge

def send_judge(socket, judge):
	msg = bytes([int(judge * 255)])
	socket.send(msg)

def recv_policy_train_data(socket):
	#255개 단위로 들어옴.
	size_255 = socket.recv(1)
	size = size_255[0] * 255
	data_x = []
	data_y = []
	for i in range(size):
		data_x.append(recv_board(socket))
		data_y.append(recv_move(socket))
	return [data_x, data_y]

def recv_value_train_data(socket):
	size_255 = socket.recv(1)
	size = size_255[0] * 255
	data_x = []
	data_y = []
	for i in range(size):
		data_x.append(recv_board(socket))
		data_y.append(recv_judge(socket))
	return [data_x, data_y]

###########################################################################
#각 명령에 대한 처리 함수
def proc_check(header, socket):
	send_ok(socket)

def proc_create(header, socket):
	name = recv_string(socket)
	if header[1] == 1:
		if name in policy_networks:
			send_failed(socket)
		else:
			policy_networks[name] = tn.create_policy_net()
			send_ok(socket)
	else:
		if name in value_networks:
			send_failed(socket)
		else:
			value_networks[name] = tn.create_value_net()
			send_ok(socket)
	

def proc_load(header, socket):
	callname = recv_string(socket)
	filename = recv_string(socket)
	if header[1] == 1:
		if not callname in policy_networks:
			policy_networks[callname] = tn.PolicyNetwork()
		result = policy_networks[callname].load(filename)
	else:
		if not callname in value_networks:
			value_networks[callname] = tn.ValueNetwork()
		result = value_networks[callname].load(filename)
	if result:
		send_ok(socket)
	else:
		send_failed(socket)

def proc_save(header, socket):
	callname = recv_string(socket)
	filename = recv_string(socket)
	if header[1] == 1:
		if not callname in policy_networks:
			send_failed(socket)
			return
		policy_networks[callname].save(filename)
	else:
		if not callname in value_networks:
			send_failed(socket)
			return
		value_networks[callname].save(filename)
	send_ok(socket)

def proc_evaluate(header, socket):
	callname = recv_string(socket)
	if header[1] == 1:
		board = recv_board(socket)
		proms = policy_networks[callname].evaluate(board)
		send_ok(socket)
		send_proms(socket, proms)
	else:
		board = recv_board(socket)
		judge = value_networks[callname].evaluate(board)
		send_ok(socket)
		send_judge(socket, judge)

def proc_train(header, socket):
	callname = recv_string(socket)
	if header[1] == 1:
		policy_train_data = recv_policy_train_data(socket)
		policy_networks[callname].train(policy_train_data)
	else:
		value_train_data = recv_value_train_data(socket)
		value_networks[callname].train(value_train_data)
	send_ok(socket)

############################################################
##########################################################
# 메인

print("start to setting server up...")
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(("", 9999))
server_socket.listen(5)

print ("TCPServer Waiting for client on port 5000")

while 1:
	#받아서
	client_socket, address = server_socket.accept()
	print ("I got a connection from ", address)

	while 1:
		

		header = client_socket.recv(2)
		if not header : break
		code = header[0]

		if code == 1:
			proc_check(header, client_socket)
		elif code == 2:
			proc_create(header, client_socket)
		elif code == 3:
			proc_load(header, client_socket)
		elif code == 4:
			proc_save(header, client_socket)
		elif code == 5:
			proc_evaluate(header, client_socket)
		elif code == 6:
			proc_train(header, client_socket)

	
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