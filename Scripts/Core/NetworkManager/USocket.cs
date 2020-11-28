using System.Collections.Generic;
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

    private Socket tcpClient;
    public bool Connect(string InIP,int InPort)
    {
        IPAddress _ip = IPAddress.Parse(InIP);
        IPEndPoint _endPoint = new IPEndPoint(_ip, InPort);
        bool _result = Connect(_endPoint);
        bool _reconnecting = false;
        Observable.Interval(TimeSpan.FromMilliseconds(1000)).Where((_1,_2)=>!Connected&&!_reconnecting).Subscribe(_3=>{
            Debug.Log("重连中...");
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


    private Type MsgReceiver = null;
    public void SetMessageReceiver(Type T)
    {
        if(!T.IsSubclassOf(typeof(UMessageReceiver))){
            Debug.LogWarning("not a valid class");
            return;
        }
        MsgReceiver = T;
    }

    private bool Connect(IPEndPoint InEndPoint)
    {
        tcpClient = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        

            Observable.Start(()=>{
                try
                {
                    tcpClient.Connect(InEndPoint);
                    return true;
                }
                catch(Exception e)
                {
                    Debug.Log(e.Message);
                    return false;
                }
            }).ObserveOnMainThread().Subscribe(_=>{
                Debug.LogFormat("connect result: {0}",_);
                if(!_) return;
                Managements.Event.Fire(new UNetConnectedEvent{});
                startNewReceiver(tcpClient);
            });

            // tcpClient.BeginConnect(InEndPoint,_result=>{
            //     tcpClient.EndConnect(_result);
            // },null);

            // 3秒后还没连上  要准备重连
            Managements.Timer.SetTimeout(3.0f,()=>{
                if(!tcpClient.Connected){
                    DisConnect(tcpClient);
                }
            });
        return true;
    }

    private bool DisConnect(Socket InSocket){
        if(InSocket==null) return false;
        try
        {
            Managements.Event.Fire(new UNetDisconnectedEvent());
            tcpClient.Shutdown(SocketShutdown.Both);
            tcpClient.Disconnect(false);
            tcpClient.Close();
            tcpClient = null;
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
        if(!this.allClients.ContainsKey(clientKey)){
            this.allClients.Add(clientKey,handler);
        }

        startNewReceiver(handler);
    }  


    public List<UMessageReceiver> messageReceivers = new List<UMessageReceiver>();
    private void startNewReceiver(Socket InSocket){
        UMessageReceiver _receiver = Activator.CreateInstance(MsgReceiver) as UMessageReceiver;
        messageReceivers.Add(_receiver);
        _receiver.Prepare(InSocket,messageDispatcher);
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
        try
        {
            return tcpClient.Send(InData);    
        }
        catch (System.Exception e)
        {
            //Debug.LogError(e.Message);
            DisConnect(tcpClient);
        }
        return 0;
    }

    public void Dispose()
    {

        messageReceivers.ForEach(_receiver=>{
            _receiver.Dispose();
        });
        available = false;
        tcpServerAllDone.Set();

        if(listenThread!=null){
            listenThread.Abort();
        }
    }

}
    
}
