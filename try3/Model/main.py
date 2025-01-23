#%%
import model
import server
import json
import time
import torch

server.init()
#%%


#%%
while True:
    begin = time.time()
    sock = server.wait()
    
    line = server.get_line(sock)
    data = json.loads(line)
    states = data['States']
    p, v = model.eval(states)
    ret = {
        'MessageType':'evaluate_return', 
        'PolicyIndices':[], 
        'States':[[]], 
        'Policies':p.flatten().tolist(),
        'Values':v.flatten().tolist()
        }
    s = json.dumps(ret)
    server.send_line(sock, s)
    sock.close()
    end = time.time()
    print(end - begin)
        