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
        //���� �ν��Ͻ��� ���¸� Running���� ����
        mThread.Start();
    }

    void Update()
    {
        //transform.position += Vector3.forward * Time.deltaTime;
        //transform.position = receivedPos; //(IPC �ڵ�) assigning receivedPos in SendAndReceiveData()
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
        listener = new TcpListener(IPAddress.Any, connectionPort); //TCP ������ TcpListener Ŭ������ ���� ��Ʈ�� ����
        listener.Start();
        client = listener.AcceptTcpClient(); // AcceptTcpClient()�޼��带 ���� Ŭ���̾�Ʈ ������ ���
        Debug.Log("Welcom Client!!!");
        
        running = true;
        while (running)
        {
            string tmp_store = null;
            int fun_cmd = 0;

            tmp_store = Recieve_data();
            //print(tmp_store.GetType());
            fun_cmd = cut_fun_cmd(tmp_store); //��� �ڵ� Ȯ��
            
            if (tmp_store != null)
            {            
                if (fun_cmd == 1) //1�� Network Check Function
                {
                   func1("I can hear you.");
                }

                if (fun_cmd == 2) //2�� Echo Function 
                {
                    func2(tmp_store);
                }

                if (fun_cmd == 3) //3�� Object Move Function
                {
                    func3(tmp_store);
                }

                if (fun_cmd == 4) //4�� Reset Function
                {
                    Send_data("[0, 0, 0, 0]");
                }
            }
            else //��� �ڵ� �߸� �Է�
            {
                Send_TCP_Done();
            }
        }
    }

    // string ������ ������ �Լ�
    void Send_data(string str_data)
    {
        NetworkStream nwStream = client.GetStream();
        byte[] myWriteBuffer = Encoding.ASCII.GetBytes(str_data); //Converting string to byte data
        nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
    }

    string Recieve_data()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] myReadBuffer = new byte[client.ReceiveBufferSize]; //������ ���� ����

        int bytesRead = nwStream.Read(myReadBuffer, 0, client.ReceiveBufferSize); //�����͸� buffer�� ����
        //print(bytesRead);
        string dataReceived = Encoding.UTF8.GetString(myReadBuffer, 0, bytesRead); //buffer�� ��Ʈ������ ��ȯ
        print(dataReceived);

        return dataReceived;
    }

    //TCP ��� �Ϸ� signal
    void Send_TCP_Done()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] myWriteBuffer = Encoding.ASCII.GetBytes("TCP Done But Function Code Wrong"); //Converting string to byte data
        nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
    }

    void SendAndReceiveData()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] myReadBuffer = new byte[client.ReceiveBufferSize]; //������ ���� ����

        //Ŭ���̾�Ʈ�� ���� ���� ������
        int bytesRead = nwStream.Read(myReadBuffer, 0, client.ReceiveBufferSize); //�����͸� buffer�� ����
        print(bytesRead);
        string dataReceived = Encoding.UTF8.GetString(myReadBuffer, 0, bytesRead); //buffer�� ��Ʈ������ ��ȯ
        print(dataReceived);

        if (dataReceived != null)
        {
            //������ �ޱ�
            //receivedPos = StringToVector3(dataReceived); //<-- assigning receivedPos value from Python
            //print(dataReceived);

            //������ ������
            //byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Hey I got your message Python! Do You see this massage?"); //Converting string to byte data
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("TCP Done"); //Converting string to byte data
            nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
        }
    }

    //����ڵ� ����
    int cut_fun_cmd(string py_data)
    {
        //print("cut_fun_cmd ���� ��!!!");
        int func_code_result = 0;

        py_data = string.Join("", py_data.Split(',')); // ,(�ĸ�) ����
        if (py_data.StartsWith("[") && py_data.EndsWith("]"))
        {
            py_data = py_data.Substring(1, 2);
            func_code_result = int.Parse(py_data); // String to int ����ȯ
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

        para_data = string.Join("", para_data.Split(',', '[', ']')); //, [ ] ����
        echo_data = para_data.Split(' '); //�������� ����
        /*
        print(echo_data[0]); //��� �ڵ�
        print(echo_data[1]); //���� ������
        print(echo_data[2]); // ������ , 0
        print(echo_data[3]); // ������ , 0
        */
        Send_data(echo_data[1]);
    }

    void func3(string pos_data)
    {
        string[] obj_pos_data = new string[4];

        pos_data = string.Join("", pos_data.Split(',', '[', ']')); //, [ ] ����

        obj_pos_data = pos_data.Split(' '); //�������� ����

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

    // ��Ʈ���� ���ͷ� ��ȯ �ϴ� �Լ�
    public static Vector3 StringToVector3(string sVector)
    {
        // �Ķ���� ����
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