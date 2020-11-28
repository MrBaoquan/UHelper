using System.Collections.Generic;
using System.Threading.Tasks;
namespace UHelper{
    

abstract class USerialPortMessageReceiver
{

    protected USerialPort serialPort = null;
    private Queue<SPMessage> messages = null;

    private bool alive = true;

    public void Prepare(USerialPort InSerialPort, Queue<SPMessage> InMessages){
        serialPort = InSerialPort;
        messages = InMessages;
        OnConnected();
    }


    Task flushTask = null;
    public void OnConnected(){
        flushTask = Task.Factory.StartNew(()=>{
            while(alive){
                OnFlushMessage();
            }
        },TaskCreationOptions.LongRunning);
    }

    public virtual void OnFlushMessage(){}

    protected void PushMessage(SPMessage InMessage)
    {
        messages.Enqueue(InMessage);
        UnityEngine.Debug.Log("push message");
    }

    public void Stop(){
        alive = false;
        flushTask.Dispose();
    }

}


}