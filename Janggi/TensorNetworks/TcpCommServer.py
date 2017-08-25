# TCP server example
import socket

def proc_check(self, header, socket):
	socket.send('')

def proc_create(self, header, socket):


def proc_load(self, header, socket):


def proc_save(self, header, socket):


def proc_evaluate(self, header, socket):


def proc_train(self, header, socket):



server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(("", 9999))
server_socket.listen(5)

print ("TCPServer Waiting for client on port 5000")

while 1:
	#받아서
	client_socket, address = server_socket.accept()
	print ("I got a connection from ", address)

	while 1:
		received = client_socket.recv(2)
		if not received : break
		print(type(received))
		#보여주고
		print ("RECEIVED:" , received.decode())
		#에코
		client_socket.send(data);
	
	print("received all!")
	for data in dataList:
		print(data)
	
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