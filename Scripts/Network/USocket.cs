
using System;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using UnityEngine;

using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace  UHelper
{

class USocket
{
    private MessageQueeue recvMessageQueue = new MessageQueeue();
    public MessageQueeue Messages{
        get {return recvMessageQueue;}
    }

    private bool available = true;
    Thread recvThread;

    private Socket tcpClient;
    public bool Connect(string InIP,int InPort)
    {
        tcpClient = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        IPAddress _ip = IPAddress.Parse(InIP);
        try
        {
            tcpClient.Connect(new IPEndPoint(_ip, InPort));
            recvThread = new Thread(() => {
                byte[] _dataSizeBuffer = new byte[32];
                byte[] _typeSizeBuffer = new byte[32];
                string _from = InIP + "_" + InPort;
                while (available)
                {
                    // ----  ---- ----**----
                    // 类型大小
                    tcpClient.Receive(_typeSizeBuffer, 0, 32, 0);
                    tcpClient.Receive(_dataSizeBuffer, 0, 32, 0);

                    int _typeSize = BitConverter.ToInt32(_typeSizeBuffer,0);
                    int _dataSize = BitConverter.ToInt32(_dataSizeBuffer, 0);

                    byte[] _type = null;
                    string _typeName = string.Empty;
                    if (_typeSize > 0)
                    {
                        _type = new byte[_typeSize];
                        tcpClient.Receive(_type, 0, _typeSize, 0);
                        _typeName = System.Text.Encoding.Default.GetString(_type).TrimEnd('\0');
                    }

                    if (_typeName == string.Empty)
                    {
                        continue;
                    }

                    byte[] _data = null;
                    if (_dataSize > 0)
                    {
                        _data = new byte[_dataSize];
                        tcpClient.Receive(_data, 0, _dataSize, 0);
                    }

                    
                    IMessage _message = null;
                    if (_data != null)
                    {
                        _message = _data.DeserializeFromTypeString(_typeName);
                    }
                    else
                    {
                        _message = ProtoMessage.CreateMessage(_typeName);
                    }
                    if (_message != null)
                    {
                        MessageQueeue.Message _recvMessage = new MessageQueeue.Message();
                        _recvMessage.From = _from;
                        _recvMessage.Data = _message;
                        recvMessageQueue.PushMessage(_recvMessage);
                    }
                }
            });
            recvThread.Start();
            }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            return false;

        }
        return true;
    }
   
    public int Send(byte[] InData)
    {
        if (tcpClient == null)
        {
            return 0;
        }
        return tcpClient.Send(InData);
    }

    public void Dispose()
    {
        available = false;
        recvThread.Abort();
    }

}
    
}
