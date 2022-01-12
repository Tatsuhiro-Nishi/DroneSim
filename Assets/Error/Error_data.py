# -*- coding: utf-8 -*-

import glob

paths = glob.glob("./*csv")

recordFile = open('result/Error_data.csv', 'w')
for path in paths:
    print(path)
    f = open(path, 'r')
    datas = f.readlines()
    f.close()
    
    labelQ = True
    error_mean, error_sum = 0, 0
    for data in datas:       
        if labelQ != True:
            txt_arr = data.split(", ")
            error_sum += float(txt_arr[0])
        labelQ = False
    error_mean = error_sum / len(datas)
    recordFile.write(path +", "+str(error_mean)+"\n")
    
recordFile.close()

