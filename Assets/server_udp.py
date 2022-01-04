import socketserver  
import cv2
import math
import numpy as np
from numpy import linalg as LA
import socket  
import sys  
import time
import base64
import json


class NpEncoder(json.JSONEncoder):
    """ Numpyオブジェクトを含むオブジェクトのJsonエンコーダ """
    def default(self, obj):
        if isinstance(obj, np.integer):
            return int(obj)
        elif isinstance(obj, np.floating):
            return float(obj)
        elif isinstance(obj, np.ndarray):
            return obj.tolist()
        else:
            return super(NpEncoder, self).default(obj)

def from_base64(b64msg):
   #Base64文字列をcv2イメージにデコード
   img = base64.b64decode(b64msg.replace("data:image/png;base64,",""))
   npimg = np.fromstring(img, dtype=np.uint8)
   image_data = cv2.imdecode(npimg, 1)
   return image_data

class Timer(object):
    """処理時間計測ユーティリティクラス"""
    def __init__(self, verbose=False):
        self.verbose = verbose
  
    def __enter__(self):
        self.start = time.time()
        return self
  
    def __exit__(self, *args):
        self.end = time.time()
        self.msecs = self.end - self.start * 1000
        #print('elapsed time: %f msecs' % self.msecs)

start=0
end = 0 

class UDPHandler(socketserver.BaseRequestHandler):  
        
    def handle(self):
        print("-------------------------------------")
        print("start listening")
        start = time.time()
        
        self.message = self.request[0].strip()
        print(self.message)
        end = time.time() - start
        print("receive time : "+str(end))
        information = rov_info.calc_inf(self.message.decode())

        call = str(information)
        send_time = str(time.time())
        text = call + "," + send_time
        socket = self.request[1]
    
        socket.sendto(text.encode("utf-8"), (self.client_address[0], self.client_address[1]+1))
        print("{} wrote:".format(self.client_address))
        
        #socket.settimeout(1)

class ROV_info: 
    route_num = 0
        
    def calc_inf(self, message): 
        strlen = message.split(",")
        ROV_p = [float(strlen[0]), float(strlen[2])]
        ROV_p = np.array(ROV_p)
        rad = math.radians(float(strlen[3]))
    
        ROV_dir_s = float(strlen[0]) + math.sin(rad)
        ROV_dir_c = float(strlen[2]) + math.cos(rad)
        #print("ROV ::: "+ str(math.sin(rad)) + " , "+ str(math.cos(rad)))
        ROV_dir = np.array([ROV_dir_s, ROV_dir_c])
        ROV_v = ROV_dir - ROV_p
    
        Q_Next_target, target_p = self.next_target(ROV_p)
    
        if Q_Next_target == False : return -999
    
        target_v = target_p - ROV_p
        cross = np.cross(ROV_v, target_v)
        inner = np.inner(ROV_v, target_v)
        drone_l = np.linalg.norm(ROV_v, ord=2)
        target_l = np.linalg.norm(target_v, ord=2)
        vector_cos = inner / (drone_l * target_l)
        theta = math.degrees(np.arccos(vector_cos))
        if cross < 0:
            theta *= -1 
        print("theta : "+ str(theta))
        return theta
    
    def next_target(self, ROV_p):
        p1,p2,p3 = [-10,10],[0,10],[10,10]
        p4,p5,p6 = [-10,0],[0,0],[10,0]
        p7,p8,p9 = [-10,-10],[0,-10],[10,-10]
        
        target_route = np.array([p1,p3,p6,p4,p7,p9,p6,p4,p1])
        
        target_p = target_route[self.route_num]
        print("route_num : "+ str(self.route_num))
        print("sqrt : "+ str(math.sqrt((target_route[self.route_num][0] - ROV_p[0])**2 + (target_route[self.route_num][1] - ROV_p[1])**2)))
        if math.sqrt((target_route[self.route_num][0] - ROV_p[0])**2 
                     + (target_route[self.route_num][1] - ROV_p[1])**2) < 0.5:
            self.route_num += 1
            print("route_num2 : "+ str(self.route_num))
            try :
                target_p = target_route[self.route_num]
            except IndexError:
                return (False, [-999,-999])
        return (True, target_p)

def main():
    #hostとportを設定
    HOST, PORT = "127.0.0.1", 8080
    global rov_info
    rov_info = ROV_info()
    #lsof -i :5900
    # kill {ID}
    server = socketserver.UDPServer((HOST, PORT), UDPHandler)
    server.allow_reuse_address = True

    try:
        server.serve_forever()  
    except KeyboardInterrupt:
        pass
    server.shutdown()
    sys.exit()

if __name__ == "__main__":
    main()
