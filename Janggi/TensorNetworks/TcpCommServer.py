# TCP server example
import socket
import TensorNetworks as tn

policyNetworks = {}
valueNetworks = {}

def send_ok(socket):
	socket.send(bytes([101, 0]))

def send_failed(socket):
	socket.send(bytes([102, 0]))

def recv_string(socket):
	size = socket.recv(1)
	str = socket.recv(size)
	return str.decode()

def proc_check(header, socket):
	send_ok(socket)

def proc_create(header, socket):
	name = recv_string(socket)
	if header[1] == 1:
		if name in policyNetworks:
			send_failed(socket)
		else:
			policyNetworks[name] = tn.create_policy_net()
			send_ok(socket)
	else:
		if name in valueNetworks:
			send_failed(socket)
		else:
			valueNetworks[name] = tn.create_value_net()
			send_ok(socket)
	

def proc_load(header, socket):
	callname = recv_string(socket)
	filename = recv_string(socket)
	if header[1] == 1:
		if not callname in policyNetworks:
			policyNetworks[callname] = tn.PolicyNetwork()
		policyNetworks[callname].load(filename)
	else:
		if not callname in valueNetworks:
			valueNetworks[callname] = tn.ValueNetwork()
		valueNetworks[callname].load(filename)
	send_ok(socket)

def proc_save(header, socket):
	callname = recv_string(socket)
	filename = recv_string(socket)
	if header[1] == 1:
		if not callname in policyNetworks:
			send_failed()
			return
		policyNetworks[callname].save(filename)
	else:
		if not callname in valueNetworks:
			send_failed()
			return
		valueNetworks[callname].save(filename)
	send_ok(socket)

def proc_evaluate(header, socket):
	send_ok(socket)

def proc_train(header, socket):
	send_ok(socket)



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