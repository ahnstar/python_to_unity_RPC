import keyboard #키보드 입력 라이브러리
import socket #TCP 소켓 사용 라이브러리
import time

print("Python's working")
host, port = "127.0.0.1", 19999
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM) # AF_INET : IPv4
sock.connect((host, port))
print('Connection is success')

test_bit = 100
cmd = [0,0,0,0]; #[fun_code, pos X, pos Y, pos Z]

#무한루프
while True:
    #key_input = keyboard.read_key();

    # 0.5초 마다 (sleep 0.5 sec)
    time.sleep(0.5)

    # 1번 펑션 : TCP 통신 done
    # 2번 펑션 : 통신 data ECHO
    # 3번 펑션 : 특정 위치 이동
    # 4번 펑션 : Reset function
    cmd[0] = int(input("1~4 Function Code? : "))
    #print(type(cmd[0]))
    #print(cmd[0]);

    #1번 펑션 : TCP 통신 done
    if cmd[0] == 1:
        for i in range(1,4):
            cmd[i] = 0 #나머지 0으로 초기화
        print(cmd)

    # 2번 펑션 : 통신 data ECHO
    if cmd[0] == 2:
        cmd[1] = int(input("Function 2 Parameter Input : "))
        #print("echo data : ")
        #receivedData = sock.recv(1024).decode("UTF-8")

    # 3번 펑션 : 특정 위치 이동
    if cmd[0] == 3:
        for i in range(1,4): # 원소의 개수가 n개일 때 
            cmd[i] = int(input("Function 3 Parameter Input (enter) : "))
        print(cmd)

    # 4번 펑션 : Reset function
    if cmd[0] == 4:
        print(cmd, "function 4 call")

    #Test Function
    #if key_input == 'left':
    if cmd[0] == 5:
        test_bit += 10;    # int형


    #socket의 send()와 recv()를 사용할 때 무조건 바이트 형식으로 전송
    #int에는 encode 함수가 없다 -> 즉 str만 소켓에 보낼수 있다.
    sock.sendall(str(cmd).encode("UTF-8")) #Converting string to Byte, and sending it to C#
    print("Send Data : ",cmd)


    # C#에서 데이터를 다시 받는 부분
    receivedData = sock.recv(1024).decode("UTF-8") #receiveing data in Byte fron C#, and converting it to String
    print("Get Data : ", receivedData)