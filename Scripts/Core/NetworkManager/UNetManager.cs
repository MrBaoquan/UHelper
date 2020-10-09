using UniRx;
using Google.Protobuf;
using UHelper;

namespace UHelper
{

public class UNetManager : Singleton<UNetManager>,Manageable
{
    private USocket tcpSocket = new USocket();
    private bool bConnected = false;

    public void Initialize(){}
    public void UnInitialize(){}
    public bool Connect(string InIP,int InPort)
    {
        return tcpSocket.Connect(InIP,InPort);
    }

    public bool Listen(string InIP="127.0.0.1",int InPort=6666)
    {
        return tcpSocket.Listen(InIP,InPort);
    }

    public void Send(byte[] InData)
    {
        this.tcpSocket.Send(InData);
    }

    public void Send(IMessage InMessage)
    {
        byte[] _data = ProtoMessage.PackageMessage(InMessage);
        this.tcpSocket.Send(_data);
    }

    public void SendAll(IMessage InMessage)
    {
        byte[] _data = ProtoMessage.PackageMessage(InMessage);
        this.tcpSocket.SendAll(_data);
    }

    public void SendAll(byte[] InData)
    {
        this.tcpSocket.SendAll(InData);
    }

    public MessageQueeue.Message GetMessage(){
        return this.tcpSocket.Messages.PopMessage();
    }

    public void Dispose()
    {
        this.tcpSocket.Dispose();
    }

    

}

}
