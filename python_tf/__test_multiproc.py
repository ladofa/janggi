import multiprocessing
import threading
import time
import random

import os


manager = multiprocessing.Manager()


class Controls:
    pass
controls = Controls()
controls.lock = manager.Lock()
controls.waitings = manager.list()
controls.events = manager.list([manager.Event() for _ in range(5)])




def child_proc(c, i):
    print('child', i, id(c))
    #ask
    while True:
        time.sleep(random.randint(1, 3))

        with c.lock:
            time.sleep(1)
            print('request', i)
            c.waitings.append(i)
            print(c.waitings)
            print(id(c.waitings))
            c.events[i].clear()
            
        c.events[i].wait()
        # print('ACQUIRE', i)

def parent_proc(c):
    print('parent', id(c))
    while True:
        with c.lock:
            time.sleep(2)
            for i in c.waitings:
                print('release', i)
                c.events[i].set()
            c.waitings[:] = []
        # c.lock.release()



    

procs = []
for i in range(5):
    p = multiprocessing.Process(target=child_proc, args=(controls, i))
    procs.append(p)

# p = multiprocessing.Process(target=parent_proc, args=(controls,))
# procs.append(p)

for p in procs:
    p.start()

parent_proc(controls)

for index, p in enumerate(procs):
    p.join()
    print(index)
     