using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Ports;
namespace UHelper
{

public class SPLineMessage : SPMessage
{
    public string Content = string.Empty;
}

public class SPStringLineReceiver : USPMsgReceiver
{
    public int ReadBufferSize = 4096;
    public override void OnFlushMessage()
    {
        // serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
        // byte[] _buffer = new byte[4096];
        // byte[] _result = new byte[0];

        if(ReadBufferSize<=0) return;
        try
        {
            string _result =serialPort.ReadLine();
            UnityEngine.Debug.LogFormat("receive {0}",_result.Length);
            PushMessage(new SPLineMessage{RawData=Encoding.UTF8.GetBytes(_result), Content=_result});
        }
        catch (System.Exception e)
        {
            //UnityEngine.Debug.Log(_result.Length);
            UnityEngine.Debug.Log(e.Message);
        }
    }


}



}