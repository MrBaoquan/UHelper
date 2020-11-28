using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Google.Protobuf;

namespace UHelper
{


public class UMessage : UEvent
{
    public byte[] RawData = null;
}

public class NetMessage : UEvent
{
    public UMessage Message;
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
