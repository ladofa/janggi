#%%
import socket
import json
import io

def init(host="127.0.0.1", port=4050):
    global server_socket
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)  # 주소 재사용 활성화
    server_socket.bind((host, port))

def wait():
    global server_sock
    server_socket.listen(1)  # 하나의 클라이언트만 처리
    client_socket, client_address = server_socket.accept()
    return client_socket

#%%
def get_line(client_socket):
    lines = []
    while True:
        buf = client_socket.recv(1000000).decode('utf-8')
        lines.append(buf)
        if buf[-1] == '\n':
            break
    line = ''.join(lines)    
    return line

def send_line(client_socket, line):
    client_socket.send((line + '\r\n').encode('utf-8'))


