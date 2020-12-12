using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using UniRx;
namespace  UHelper
{

public class UNetConnectedEvent:UEvent{
    public USocket Socket = null;
    public string IP = string.Empty;
    public int Port = -1;
    public string Key{
        get{
            return string.Format("{0}_{1}", IP, Port);
        }
    }
}
public class UNetDisconnectedEvent:UEvent{
    public USocket Socket = null;
    public string IP = string.Empty;
    public int Port = -1;
    public string Key{
        get{
            return string.Format("{0}_{1}", IP, Port);
        }
    }
}

public class USocket
{
    private MessageQueeue recvMessageQueue = new MessageQueeue();
    private MessageQueeue tcpMessageDispatcher = new MessageQueeue();
    private MessageQueeue udpMessageDispatcher = new MessageQueeue();
    public MessageQueeue Messages{
        get {return recvMessageQueue;}
    }

    private bool connected = false;
    public bool Connected{
        get{
            if(tcpClient==null) return false;
            return connected;
            // var _condition1 = !tcpClient.Poll(0,SelectMode.SelectRead);
            // var _condition2 = tcpClient.Available == 0;
            // var _condition3 = tcpClient.Connected;
        }
    }

    private bool available = true;
    private Socket tcpClient;
    private IPEndPoint endPoint;
    public string Key{
        get{
            return string.Format("{0}_{1}",endPoint.Address.ToString(),endPoint.Port);
        }
    }
    public bool Connect(string InIP,int InPort)
    {
        IPAddress _ip = IPAddress.Parse(InIP);
        this.endPoint = new IPEndPoint(_ip, InPort);
        bool _result = Connect(endPoint);

        Observable.Interval(TimeSpan.FromMilliseconds(3000)).Where((_1,_2)=>tcpClientReuse&&!Connected).Subscribe(_3=>{
            Debug.Log("Reconnecting...");
            destroySocket(tcpClient);
            Connect(endPoint);
        });
        dispatchTcpMessages();
        return _result;
    }

    UdpClient udpClient;
    CancellationTokenSource udpClientTokenSource = new CancellationTokenSource();
    public void ListenUDP(string InIP, int InPort, bool bEnableBroadcast=true)
    {
        if(udpClient!=null) return;
        udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(InIP), InPort));
        var _ip = (udpClient.Client.LocalEndPoint as IPEndPoint);
        Debug.LogFormat("Udp server listen at: {0}:{1}", _ip.Address.ToString(), _ip.Port);
        udpClient.EnableBroadcast = bEnableBroadcast;
        var _task = Task.Factory.StartNew(()=>{
            while (!udpClientTokenSource.IsCancellationRequested)
            {
                IPEndPoint _endPoint = null;
                try
                {
                    byte[] _buffer = udpClient.Receive(ref _endPoint);
                    udpMessageDispatcher.PushMessage(new UMessage{RawData=_buffer,IP=_endPoint.Address.ToString(),Port=_endPoint.Port});
                }catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
            }
        },TaskCreationOptions.LongRunning);

        dispatchUdpMessages();
    }

    public void Broadcast(byte[] InData, int InPort)
    {
        if(udpClient==null) return;
        udpClient.Send(InData, InData.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), InPort));
    }


    private Type MsgReceiver = null;
    public void SetMessageReceiver(Type T)
    {
        if(T==null) return;
        if(!T.IsSubclassOf(typeof(UMessageReceiver))){
            Debug.LogWarning("not a valid class");
            return;
        }
        MsgReceiver = T;
    }

    // Tcp Client 发送消息
    public int Send(byte[] InData)
    {
        if (tcpClient == null || !Connected)
        {
            return 0;
        }
        try
        {
            return tcpClient.Send(InData);    
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
            DisConnect(true);
            //DisConnect(tcpClient);
        }
        return 0;
    }



    /// <summary>
    /// Private Methods
    /// </summary>

    private void dispatchTcpMessages(){
        Observable.EveryUpdate().Subscribe(_=>{
            var _message = tcpMessageDispatcher.PopMessage();
            while(_message!=null){
                Managements.Event.Fire(new NetMessage{Message = _message, Protocol=NetProtocol.Tcp});
                _message = tcpMessageDispatcher.PopMessage();
            }
        });
    }

    private void dispatchUdpMessages(){
        Observable.EveryUpdate().Subscribe(_=>{
            var _message = udpMessageDispatcher.PopMessage();
            while(_message!=null){
                Managements.Event.Fire(new NetMessage{Message = _message, Protocol=NetProtocol.Udp});
                _message = udpMessageDispatcher.PopMessage();
            }
        });
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
                Debug.Log(e.StackTrace);
                return false;
            }
        }).ObserveOnMainThread().Subscribe(_=>{
            Debug.LogFormat("connect result: {0}",_);
            if(!_) return;
            connected = true;
            var _endPoint = tcpClient.RemoteEndPoint as IPEndPoint;
            Managements.Event.Fire(new UNetConnectedEvent{IP=_endPoint.Address.ToString(), Port = _endPoint.Port, Socket=this});
            startNewReceiver(tcpClient);
        });

        return true;
    }


    private void destroySocket(Socket InSocket)
    {
        if(InSocket==null) return;
        try
        {
            if(InSocket.Connected){
                InSocket.Shutdown(SocketShutdown.Both);
                InSocket.Disconnect(false);
            }
            
            InSocket.Close();
            InSocket.Dispose();     
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        InSocket = null;
    }

    private bool tcpClientReuse = true;
    public bool DisConnect(bool bReuse=false){
        tcpClientReuse = false;
        if(connected){
            connected = false;
            Managements.Event.Fire(new UNetDisconnectedEvent{IP=endPoint.Address.ToString(), Port = endPoint.Port, Socket=this});
        }
        if(!bReuse){
            destroySocket(tcpClient);
            Dispose();
        }
        tcpClientReuse = bReuse;
        return true;
    }

    public static string data = null;  
    private Socket tcpServer = null;
    private Thread listenThread = null;
    private ManualResetEvent tcpServerAllDone = new ManualResetEvent(false); 
    private Dictionary<string,Socket> allClients  = new Dictionary<string, Socket>();
    public bool Listen(string InIP="127.0.0.1",int InPort=6666) 
    {  
        IPAddress ipAddress = IPAddress.Parse(InIP);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, InPort);

        // Create a TCP/IP socket.  
        tcpServer = new Socket(ipAddress.AddressFamily,  
            SocketType.Stream, ProtocolType.Tcp );  

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try {  
            tcpServer.Bind(localEndPoint);  
            tcpServer.Listen(100);  
            listenThread = new Thread(()=>{
                while (available) {  
                    // Set the event to nonsignaled state.  
                    tcpServerAllDone.Reset();  
                    // Start an asynchronous socket to listen for connections.  
                    Debug.Log("Waiting for a connection...");  
                    tcpServer.BeginAccept(
                        new AsyncCallback(AcceptCallback),  
                        tcpServer );  

                    tcpServerAllDone.WaitOne();  
                }  
            });
            listenThread.Start();
            dispatchTcpMessages();
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
        if(MsgReceiver==null) return;
        UMessageReceiver _receiver = Activator.CreateInstance(MsgReceiver) as UMessageReceiver;
        messageReceivers.Add(_receiver);
        _receiver.Prepare(InSocket, tcpMessageDispatcher);
    }
  
    private void SendAsync(Socket handler, byte[] InData) {    
        handler.BeginSend(InData, 0, InData.Length, 0,  
            new AsyncCallback(SendCallback), handler);  
    }

    private void SendCallback(IAsyncResult ar) {  
        try {
            Socket handler = (Socket) ar.AsyncState;  
            int bytesSent = handler.EndSend(ar);
  
        } catch (Exception e) {  
            Debug.Log(e.ToString());  
        }  
    }  

    public void SendAll(byte[] InDatas)
    {
        Dictionary<string,Socket> _disconnectedClients = new Dictionary<string, Socket>();    
        foreach (var _client in allClients)
        {
            if(!_client.Value.Connected){
                _disconnectedClients.Add(_client.Key,_client.Value);
                continue;
            }
            this.SendAsync(_client.Value,InDatas);
        }

        foreach(var _client in _disconnectedClients){
            _client.Value.Dispose();
            this.allClients.Remove(_client.Key);
        }
        _disconnectedClients.Clear();
    }

    public void Dispose()
    {
        udpClientTokenSource.Cancel();
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
