using System.Net;
using System.Net.Sockets;

namespace UHelper
{

public class UMessageReceiver
{
    protected Socket socket;
    protected MessageQueeue messageQueeue;

    protected string RemoteIP = string.Empty;
    protected int RemotePort = -1;
    
    protected string ConnectionKey = string.Empty;

    public void Prepare(Socket InSocket, MessageQueeue InQueue){
        socket = InSocket;
        var _ipEndPoint = socket.RemoteEndPoint as IPEndPoint;
        RemoteIP = _ipEndPoint.Address.ToString();
        RemotePort = _ipEndPoint.Port;
        messageQueeue = InQueue;
        OnConnected();
    }

    public virtual void OnConnected(){}

    protected void pushMessage(UMessage InMessage){
        InMessage.IP = RemoteIP;
        InMessage.Port = RemotePort;
        messageQueeue.PushMessage(InMessage);
    }

    public void Dispose(){
        if(socket!=null){
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();    
            }
            catch (System.Exception){}
            
        }
    }

}

}
