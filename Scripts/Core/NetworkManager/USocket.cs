﻿using System.Collections.Generic;
using System.Text;

using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using UniRx;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace  UHelper
{


public class StateObject {  
    // Client  socket.  
    public Socket workSocket = null;  
    // Size of receive buffer.  
    public const int BufferSize = 1024;  
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];  
// Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class UNetConnectedEvent:UEvent{}
public class UNetDisconnectedEvent:UEvent{}

class USocket
{
    private MessageQueeue recvMessageQueue = new MessageQueeue();
    private MessageQueeue messageDispatcher = new MessageQueeue();
    public MessageQueeue Messages{
        get {return recvMessageQueue;}
    }

    public bool Connected{
        get{
            if(tcpClient==null) return false;
            return  !tcpClient.Poll(0,SelectMode.SelectRead)&&tcpClient.Available==0;
        }
    }

    private bool available = true;
    Thread recvThread;

    private Socket tcpClient;
    public bool Connect(string InIP,int InPort)
    {
        IPAddress _ip = IPAddress.Parse(InIP);
        IPEndPoint _endPoint = new IPEndPoint(_ip, InPort);
        bool _result = Connect(_endPoint);
        bool _reconnecting = false;
        Observable.Interval(TimeSpan.FromMilliseconds(1000)).Where((_1,_2)=>!Connected&&!_reconnecting).Subscribe(_3=>{
            Debug.Log("断开连接了...");
            _reconnecting = true;
            Observable.Timer(TimeSpan.FromSeconds(3.0f)).Subscribe(_4=>_reconnecting = false);
            DisConnect(tcpClient);
            Connect(_endPoint);
        });
        dispatchMessage();
        return _result;
    }

    private void dispatchMessage(){
        Observable.EveryUpdate().Subscribe(_=>{
            var _message = messageDispatcher.PopMessage();
            while(_message!=null){
                Managements.Event.Fire(new NetMessage{Message = _message});
                _message = messageDispatcher.PopMessage();

            }
        });
    }

    private bool Connect(IPEndPoint InEndPoint)
    {
        Debug.Log("重新连接了");
        tcpClient = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        try
        {
     
            //tcpClient.Connect(InEndPoint);
            recvThread = new Thread(() => {
                byte[] _dataSizeBuffer = new byte[32];
                byte[] _typeSizeBuffer = new byte[32];
                string _from = InEndPoint.Address + "_" + InEndPoint.Port;
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
                        messageDispatcher.PushMessage(_recvMessage);
                    }
                }
            });

            tcpClient.BeginConnect(InEndPoint,_result=>{
                tcpClient.EndConnect(_result);
                Managements.Event.Fire(new UNetConnectedEvent{});
                recvThread.Start();
            },null);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            return false;

        }
        return true;
    }

    private bool DisConnect(Socket InSocket){
        if(InSocket==null) return false;
        try
        {
            tcpClient.Shutdown(SocketShutdown.Both);
            tcpClient.Disconnect(false);
            tcpClient.Close();
            tcpClient = null;
            recvThread.Abort();
            Managements.Event.Fire(new UNetDisconnectedEvent{});
        }
        catch (System.Exception)
        {
        }
        return true;
    }

    public static string data = null;  
    private Socket tcpServer = null;
    private Thread listenThread = null;

    // State object for reading client data asynchronously  
    public class StateObject {  
        // Client  socket.  
        public Socket workSocket = null;  
        // Size of receive buffer.  
        public const int BufferSize = 1024;  
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];  
    // Received data string.  
        public StringBuilder sb = new StringBuilder();

        public byte[] RawTypeSize = new byte[32];
        public byte[] RawDataSize = new byte[32];

        public byte[] RawTypeName = null;
        public string TypeName;
        public byte[] RawData = null;

        // 当前读取到第几步 
        public int Step = -1;
    }  

    private ManualResetEvent tcpServerAllDone = new ManualResetEvent(false); 
    private Dictionary<string,Socket> allClients  = new Dictionary<string, Socket>();
    public bool Listen(string InIP="127.0.0.1",int InPort=6666) 
    {  
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".
        IPAddress ipAddress = IPAddress.Parse(InIP);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, InPort);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,  
            SocketType.Stream, ProtocolType.Tcp );  

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try {  
            listener.Bind(localEndPoint);  
            listener.Listen(100);  
            listenThread = new Thread(()=>{
                while (available) {  
                    // Set the event to nonsignaled state.  
                    tcpServerAllDone.Reset();  
                    // Start an asynchronous socket to listen for connections.  
                    Debug.Log("Waiting for a connection...");  
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),  
                        listener );  

                    // Wait until a connection is made before continuing.  
                    tcpServerAllDone.WaitOne();  
                }  
            });
            listenThread.Start();
            dispatchMessage();
            return true;
        } catch (Exception e) {  
            Debug.Log(e.ToString());  
            return false;
        } 

    }  

    private string getClientKey(Socket InClient)
    {
        IPAddress clientIP = (InClient.RemoteEndPoint as IPEndPoint).Address;
        int clientPort = (InClient.RemoteEndPoint as IPEndPoint).Port;
        return String.Format("{0}_{1}",clientIP.ToString(),clientPort);
    }

    public void AcceptCallback(IAsyncResult ar) {  
        // Signal the main thread to continue.  
        tcpServerAllDone.Set();  
  
        // Get the socket that handles the client request.  
        Socket listener = (Socket) ar.AsyncState;  
        Socket handler = listener.EndAccept(ar);
        string clientKey = this.getClientKey(handler);
        Debug.LogWarningFormat("add connected key {0}",clientKey);
        if(!this.allClients.ContainsKey(clientKey)){
            this.allClients.Add(clientKey,handler);
        }
        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive( state.RawTypeSize, 0, 32, 0,  
            new AsyncCallback(ReadCallback), state);  
    }  
  
    public void ReadCallback(IAsyncResult ar) {  
        String content = String.Empty;  
  
        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject) ar.AsyncState;  
        Socket handler = state.workSocket;
        
  
        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);
        if(state.Step==-1&&bytesRead==32){
            state.Step = 0;
            var _typeSize = BitConverter.ToInt32(state.RawTypeSize, 0);
            state.RawTypeName = new byte[_typeSize];
            handler.BeginReceive(state.RawDataSize,0,32,0, new AsyncCallback(ReadCallback),state);
            return;
        }

        if(state.Step==0&&bytesRead==32){
            state.Step = 1;
            int _dataSize = BitConverter.ToInt32(state.RawDataSize,0);
            state.RawData = new byte[_dataSize];
            handler.BeginReceive(state.RawTypeName,0,state.RawTypeName.Length,0, new AsyncCallback(ReadCallback), state);
            return;
        }

        if(state.Step==1){
            state.Step = 2;
            var _typeName = System.Text.Encoding.Default.GetString(state.RawTypeName).TrimEnd('\0');
            state.TypeName = _typeName;
            handler.BeginReceive(state.RawData,0,state.RawData.Length,0,new AsyncCallback(ReadCallback), state);
            return;
        }

        if(state.Step == 2){
            state.Step = -1;
            IMessage _message = state.RawData.DeserializeFromTypeString(state.TypeName);
            MessageQueeue.Message _recvMessage = new MessageQueeue.Message();
            _recvMessage.From = string.Empty;
            _recvMessage.Data = _message;
            recvMessageQueue.PushMessage(_recvMessage);
            messageDispatcher.PushMessage(_recvMessage);
            handler.BeginReceive(state.RawTypeSize,0,32,0,new AsyncCallback(ReadCallback),state);
        }
        
        return ;
        if (bytesRead > 0) {  
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(  
                state.buffer, 0, bytesRead));  
  
            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();  
            if (content.IndexOf("<EOF>") > -1) {  
                // All the data has been read from the
                // client. Display it on the console.  
                Debug.LogFormat("Read {0} bytes from socket. \n Data : {1}",  
                    content.Length, content );  
                // Echo the data back to the client.  
                //Send(handler, content);  
            } else {  
                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
                new AsyncCallback(ReadCallback), state);  
            }  
        }  
    }  
  
    private void Send(Socket handler, byte[] InData) {    
        handler.BeginSend(InData, 0, InData.Length, 0,  
            new AsyncCallback(SendCallback), handler);  
    }

    private void SendCallback(IAsyncResult ar) {  
        try {  
            // Retrieve the socket from the state object.  
            Socket handler = (Socket) ar.AsyncState;  
            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);  
            //handler.Shutdown(SocketShutdown.Both);  
            //handler.Close();  
  
        } catch (Exception e) {  
            Debug.Log(e.ToString());  
        }  
    }  

    private Dictionary<string,Socket> _disconnectedClients = new Dictionary<string, Socket>();
    public void SendAll(byte[] InDatas)
    {
        
        foreach (var _client in allClients)
        {
            if(!_client.Value.Connected){
                _disconnectedClients.Add(_client.Key,_client.Value);
                continue;
            }
            this.Send(_client.Value,InDatas);
        }

        foreach(var _client in _disconnectedClients){
            _client.Value.Dispose();
            this.allClients.Remove(_client.Key);
        }
        _disconnectedClients.Clear();
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
        tcpServerAllDone.Set();

        if(recvThread!=null){
            recvThread.Abort();
        }
        
        if(listenThread!=null){
            listenThread.Abort();
        }
    }

}
    
}
