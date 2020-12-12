using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System;
using UniRx;
using Google.Protobuf;
using UHelper;

namespace UHelper
{

public class UNetManager : Singleton<UNetManager>,Manageable
{
    private USocket tcpSocket = new USocket();

    private Dictionary<string, USocket> allClients = new Dictionary<string, USocket>();
    private Dictionary<string, USocket> allServers = new Dictionary<string, USocket>();
    private Dictionary<string, USocket> allUDPServers = new Dictionary<string, USocket>();

    public bool IsConnected(int Index){
        if(allClients.Count<=0) return false;
        return allClients.Values.ToList()[Index].Connected;
    }

    public bool AllClientConnected{
        get{
            if(allClients.Count<=0) return false;
            return allClients.Values.ToList().TrueForAll(_socket=>_socket.Connected);
        }
    }

    public Dictionary<string,USocket> AllClients{
        get{return allClients;}
    }

    public int ClientCount{
        get{
            return allClients.Count;
        }
    }

    public List<IPAddress> LocalAddressList{
        get{
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.ToList();
        }
    }

    public void Initialize(){
        // Managements.Event.Register<UNetConnectedEvent>(_=>{
        //     if(!allClients.ContainsKey(_.Key)){
        //         allClients.Add(_.Key,_.Socket);
        //         UnityEngine.Debug.LogFormat("添加socket {0}",_.Key);
        //     }
        // });

        // Managements.Event.Register<UNetDisconnectedEvent>(_=>{
        //     if(allClients.ContainsKey(_.Key)){
        //         allClients.Remove(_.Key);
        //         UnityEngine.Debug.LogFormat("移除socket {0}",_.Key);
        //     }
        // });
    }
    public void UnInitialize(){

    }

    public void SetReceiverHandler(Type T){
        tcpSocket.SetMessageReceiver(T);
    }

    public bool Connect(string InIP,int InPort, Type MessageReceiver=null)
    {
        string _key = string.Format("{0}_{1}", InIP, InPort);
        if(allClients.ContainsKey(_key)) return false;
        var _socket = new USocket();
        allClients.Add(_key, _socket);
        _socket.SetMessageReceiver(MessageReceiver);
        return _socket.Connect(InIP,InPort);
    }

    public void Disconnect(string InKey){
        if(!allClients.ContainsKey(InKey)) return;
        allClients[InKey].DisConnect();
        allClients.Remove(InKey);
    }

    public bool Listen(string InIP="127.0.0.1",int InPort=6666, Type MessageReceiver=null)
    {
        string _key = string.Format("{0}_{1}", InIP, InPort);
        if(allServers.ContainsKey(_key)) return false;

        var _socket = new USocket();
        allServers.Add(_key, _socket);
        _socket.SetMessageReceiver(MessageReceiver);
        return _socket.Listen(InIP,InPort);
    }

    public void ListenUDP(string InIP, int InPort){
        string _key = string.Format("{0}_{1}", InIP, InPort);
        if(allUDPServers.ContainsKey(_key)) return;
        var _udpServer = new USocket();
        _udpServer.ListenUDP(InIP, InPort);
        allUDPServers.Add(_key,_udpServer);
    }

    public void Broadcast(byte[] InData, int InPort, string InKey="")
    {
        if(InKey==""){
            if(allUDPServers.Count<=0){
                UnityEngine.Debug.LogFormat("No udp connection exists.");
                return;
            }
            allUDPServers.Values.ToList().ForEach(_udpClient=>{
                _udpClient.Broadcast(InData,InPort);
            });
            return;
        }
        if(!allUDPServers.ContainsKey(InKey)){
            UnityEngine.Debug.LogWarningFormat("Connection key {0} not exists", InKey);
            return;
        }
        allUDPServers[InKey].Broadcast(InData,InPort);


    }

    public void Send(byte[] InData, string InKey="")
    {
        if(InKey==""){
            if(allClients.Count<=0){
                UnityEngine.Debug.LogFormat("No connection exists.");
                return;
            }
            allClients.Values.ToList().ForEach(_socket=>{
                UnityEngine.Debug.LogFormat("send to {0}", InKey);
                _socket.Send(InData);
            });
            return;
        }
        if(!allClients.ContainsKey(InKey)){
            UnityEngine.Debug.LogWarningFormat("Connection key {0} not exists", InKey);
            return;
        }; 
        allClients[InKey].Send(InData);
    }

    public void Send(IMessage InMessage, string InKey="")
    {
        byte[] _data = ProtoMessage.PackageMessage(InMessage);
        Send(_data,InKey);
    }

    public void SendAll(IMessage InMessage)
    {
        // byte[] _data = ProtoMessage.PackageMessage(InMessage);
        // this.tcpSocket.SendAll(_data);
    }

    public void SendAll(byte[] InData)
    {
        //this.tcpSocket.SendAll(InData);
    }

    public UMessage GetMessage(){
        return this.tcpSocket.Messages.PopMessage();
    }

    public void DisposeAllTCPClients()
    {
        allClients.Values.ToList().ForEach(_socket=>{
            _socket.Dispose();
        });
    }

    public void DisposeAllUDPServers()
    {
        allUDPServers.Values.ToList().ForEach(_socket=>{
            _socket.Dispose();
        });
    }

    public void Dispose()
    {
        DisposeAllUDPServers();
        DisposeAllTCPClients();
    }

}

}
