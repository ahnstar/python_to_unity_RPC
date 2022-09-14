using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class Unity_RPC05 : MonoBehaviour
{
    Thread mThread;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 19999;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    Vector3 receivedPos = Vector3.zero;
    bool running;

    private void Start()
    {
        ThreadStart ts = new ThreadStart(Init_Server);
        mThread = new Thread(ts);
        //현재 인스턴스의 상태를 Running으로 변경
        mThread.Start();
    }

    void Update()
    {
        //transform.position += Vector3.forward * Time.deltaTime;
        //transform.position = receivedPos; //(IPC 코드) assigning receivedPos in SendAndReceiveData()
        //transform.position = Vector3.MoveTowards(transform.position, new Vector3(pos[0], pos[1], pos[2]), Time.deltaTime * speed);

        transform.position = Vector3.MoveTowards(transform.position, receivedPos, Time.deltaTime);

    }

    void OnApplicationQuit()
    {
        listener.Stop();
    }

    void Init_Server()
    {
        localAdd = IPAddress.Parse(connectionIP); // 127.0.0.1
        listener = new TcpListener(IPAddress.Any, connectionPort); //TCP 서버는 TcpListener 클래스를 통해 포트를 열고
        listener.Start();
        client = listener.AcceptTcpClient(); // AcceptTcpClient()메서드를 통해 클라이언트 접속을 대기
        Debug.Log("Welcom Client!!!");
        
        running = true;
        while (running)
        {
            string tmp_store = null;
            int fun_cmd = 0;

            tmp_store = Recieve_data();
            //print(tmp_store.GetType());
            fun_cmd = cut_fun_cmd(tmp_store); //펑션 코드 확인
            
            if (tmp_store != null)
            {            
                if (fun_cmd == 1) //1번 Network Check Function
                {
                   func1("I can hear you.");
                }

                if (fun_cmd == 2) //2번 Echo Function 
                {
                    func2(tmp_store);
                }

                if (fun_cmd == 3) //3번 Object Move Function
                {
                    func3(tmp_store);
                }

                if (fun_cmd == 4) //4번 Reset Function
                {
                    Send_data("[0, 0, 0, 0]");
                }
            }
            else //펑션 코드 잘못 입력
            {
                Send_TCP_Done();
            }
        }
    }

    // string 데이터 보내는 함수
    void Send_data(string str_data)
    {
        NetworkStream nwStream = client.GetStream();
        byte[] myWriteBuffer = Encoding.ASCII.GetBytes(str_data); //Converting string to byte data
        nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
    }

    string Recieve_data()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] myReadBuffer = new byte[client.ReceiveBufferSize]; //데이터 저장 버퍼

        int bytesRead = nwStream.Read(myReadBuffer, 0, client.ReceiveBufferSize); //데이터를 buffer에 저장
        //print(bytesRead);
        string dataReceived = Encoding.UTF8.GetString(myReadBuffer, 0, bytesRead); //buffer를 스트링으로 전환
        print(dataReceived);

        return dataReceived;
    }

    //TCP 통신 완료 signal
    void Send_TCP_Done()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] myWriteBuffer = Encoding.ASCII.GetBytes("TCP Done But Function Code Wrong"); //Converting string to byte data
        nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
    }

    void SendAndReceiveData()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] myReadBuffer = new byte[client.ReceiveBufferSize]; //데이터 저장 버퍼

        //클라이언트로 부터 받은 데이터
        int bytesRead = nwStream.Read(myReadBuffer, 0, client.ReceiveBufferSize); //데이터를 buffer에 저장
        print(bytesRead);
        string dataReceived = Encoding.UTF8.GetString(myReadBuffer, 0, bytesRead); //buffer를 스트링으로 전환
        print(dataReceived);

        if (dataReceived != null)
        {
            //데이터 받기
            //receivedPos = StringToVector3(dataReceived); //<-- assigning receivedPos value from Python
            //print(dataReceived);

            //데이터 보내기
            //byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Hey I got your message Python! Do You see this massage?"); //Converting string to byte data
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("TCP Done"); //Converting string to byte data
            nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
        }
    }

    //펑션코드 추출
    int cut_fun_cmd(string py_data)
    {
        //print("cut_fun_cmd 동작 중!!!");
        int func_code_result = 0;

        py_data = string.Join("", py_data.Split(',')); // ,(컴마) 삭제
        if (py_data.StartsWith("[") && py_data.EndsWith("]"))
        {
            py_data = py_data.Substring(1, 2);
            func_code_result = int.Parse(py_data); // String to int 형변환
        }
        return func_code_result;
    }

    void func1(string send_text)
    {
        Send_data(send_text);
    }

    void func2(string para_data) //data echo function
    {
        string[] echo_data = new string[4];

        para_data = string.Join("", para_data.Split(',', '[', ']')); //, [ ] 제거
        echo_data = para_data.Split(' '); //공백으로 구분
        /*
        print(echo_data[0]); //펑션 코드
        print(echo_data[1]); //받은 데이터
        print(echo_data[2]); // 빈데이터 , 0
        print(echo_data[3]); // 빈데이터 , 0
        */
        Send_data(echo_data[1]);
    }

    void func3(string pos_data)
    {
        string[] obj_pos_data = new string[4];

        pos_data = string.Join("", pos_data.Split(',', '[', ']')); //, [ ] 제거

        obj_pos_data = pos_data.Split(' '); //공백으로 구분

        //obj_pos_data[0]; // Func_Code
        //obj_pos_data[1]; // pos X
        //obj_pos_data[2]; // pos Y
        //obj_pos_data[3]; // pos Z

        Vector3 pos = new Vector3(float.Parse(obj_pos_data[1]), float.Parse(obj_pos_data[2]), float.Parse(obj_pos_data[3]));
        //print(pos[0]); // pos X
        //print(pos[1]); // pos Y
        //print(pos[2]); // pos Z
        //print(pos.GetType());

        receivedPos = pos;

        Send_data("Moved");
    }

    // 스트링을 벡터로 변환 하는 함수
    public static Vector3 StringToVector3(string sVector)
    {
        // 파라메터 제거
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
}