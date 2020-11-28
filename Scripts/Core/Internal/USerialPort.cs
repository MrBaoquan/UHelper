using UnityEngine;
using System.Threading;
using System.IO.Ports;

using UHelper;

namespace UHelper
{

class USerialPort
{
    SerialPort serialPort = new SerialPort();

    public int BytesToRead {
        get{
            return serialPort.BytesToRead;
        }
    }
    public string PortName{
        get{return serialPort.PortName;}
    }
    Thread readThread;
    bool bExit = false;
    public USerialPort(string InPortName, int InBaudRate){
        serialPort.PortName = InPortName;
        serialPort.BaudRate = InBaudRate;

        serialPort.Parity = Parity.None;
        serialPort.ReadTimeout = 500;
        serialPort.WriteTimeout = 500;
        serialPort.DataBits = 8;
        serialPort.StopBits = StopBits.One;
        serialPort.Open();
    }

    private byte[] tempBuffer = new byte[4096];
    public byte[] Read(int count, int offset=0){
        int _readed = serialPort.Read(tempBuffer,offset,count);
        var _result = tempBuffer.Slice(offset,offset + _readed);
        return _result;
    }

    public void Write(byte[] InData){
        if(!serialPort.IsOpen){
            Debug.LogWarning("serial port has not opened yet.");
            return;
        }
        serialPort.Write(InData,0, InData.Length);
    }

    public void Close(){
        serialPort.Close();
    }

}


}
