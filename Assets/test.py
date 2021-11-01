import sys 
 
x = int(sys.argv[1])  
y = int(sys.argv[2]) 

f = open('myfile.txt', 'w', encoding='UTF-8')
f.write('x + y = '+str(x+y))
f.close()

print(x + y)