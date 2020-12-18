using System.Linq;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

using UniRx;

namespace UHelper
{


public class SPMessage : UMessage
{
    public string PortName = string.Empty;
}


public class SerialPortManager : Singleton<SerialPortManager>, Manageable
{
    private Dictionary<string,USerialPort> serialPorts = new Dictionary<string, USerialPort>();
    private Queue<SPMessage> messages = new Queue<SPMessage>();

    bool alive = true;

    private List<USerialPortMessageReceiver> receivers = new List<USerialPortMessageReceiver>();

    public USerialPort CreateConnect(string InPortName, int InBaudRate=9600, USerialPortMessageReceiver InReceiver=null)
    {
        if(serialPorts.ContainsKey(InPortName)){
            return serialPorts[InPortName];
        }
        var _newSerialPort = new USerialPort(InPortName, InBaudRate);
        serialPorts.Add(InPortName, _newSerialPort);
        if(InReceiver!=null){
            InReceiver.Prepare(_newSerialPort,messages);
        }
        return _newSerialPort;
    }


    public void Initialize()
    {
        alive = true;
        Observable.EveryUpdate().Subscribe(_=>{
            while(messages.Count>0){
                var _message = messages.Dequeue();
                Managements.Event.Fire(_message);
            }
        });
    }

    public void UnInitialize(){
        alive = false;
        receivers.ForEach(_=>{
            _.Stop();
        });

        serialPorts.Values.ToList().ForEach(_=>{
            _.Close();
        });
    }

}


}