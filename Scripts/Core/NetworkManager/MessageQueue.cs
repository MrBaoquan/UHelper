using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Google.Protobuf;

namespace UHelper
{


public class UMessage : UEvent
{
    public string IP = string.Empty;
    public int Port = -1;
    public byte[] RawData = null;

    public string ConnectionKey{
        get{
            return string.Format("{0}_{1}", IP, Port);
        }
    }
}

public enum NetProtocol
{
    Unknown = -1,
    Tcp = 6,
    Udp = 17
}

public class NetMessage : UEvent
{
    public UMessage Message;
    public NetProtocol Protocol = NetProtocol.Tcp;
}

public class MessageQueeue
{

    private Queue<UMessage> messages = new Queue<UMessage>();

    public void PushMessage(UMessage InMessage)
    {
        lock (this)
        {
            messages.Enqueue(InMessage);
        }
    }

    public UMessage PopMessage()
    {
        lock(this){
            if(messages.Count<=0){
                return null;
            }
            return messages.Dequeue();
        }
    }
}


}
