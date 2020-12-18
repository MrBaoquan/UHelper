using System.Threading.Tasks;
namespace UHelper
{


public class SPFixedReceiver : USerialPortMessageReceiver
{
    public int FixedLength = 0;
    public override void OnFlushMessage()
    {
        if(FixedLength<=0) return;
        if(serialPort.BytesToRead>=FixedLength){
            try
            {
                var _rawData = serialPort.Read(FixedLength);
                UnityEngine.Debug.LogFormat("receive {0}",_rawData.Length);
                PushMessage(new SPMessage{RawData=_rawData, PortName=serialPort.PortName});
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
            
        }
    }


}



}