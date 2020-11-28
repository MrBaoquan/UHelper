using System.Net.Sockets;

namespace UHelper
{

public class UMessageReceiver
{
    protected Socket socket;
    protected MessageQueeue messageQueeue;

    public void Prepare(Socket InSocket, MessageQueeue InQueue){
        socket = InSocket;
        messageQueeue = InQueue;
        OnConnected();
    }

    public virtual void OnConnected(){}

    protected void pushMessage(UMessage InMessage){
        messageQueeue.PushMessage(InMessage);
    }

    public void Dispose(){
        if(socket!=null){
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            socket.Close();
        }
    }

}

}
