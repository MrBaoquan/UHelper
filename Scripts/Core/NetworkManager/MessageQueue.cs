using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Google.Protobuf;

namespace UHelper
{

public class NetMessage :UEvent
{
    public MessageQueeue.Message Message;
}

public class MessageQueeue
{
    public class Message
    {
        public string From=string.Empty;
        public IMessage Data;
    }

    private Queue<Message> messages = new Queue<Message>();

    public void PushMessage(Message InMessage)
    {
        lock (this)
        {
            messages.Enqueue(InMessage);
        }
    }

    public Message PopMessage()
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
