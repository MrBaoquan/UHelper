using System.Reflection;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using Google.Protobuf;
using Google.Protobuf.Reflection;
class ProtoMessage
{

    public static IMessage  CreateMessage(string InType)
    {
        Type _T = Type.GetType(InType);
        if (_T == null)
        {
            return null;
        }
        return Activator.CreateInstance(_T) as IMessage;
    }

    public static byte[] PackageMessage(IMessage InMessage)
    {
        byte[] _data = InMessage.SerializeToByteArray();
        byte[] _type = System.Text.Encoding.Default.GetBytes(InMessage.GetType().FullName);

        byte[] _result = new byte[32 + 32 + _type.Length + _data.Length];
        
        byte[] _dataSize = BitConverter.GetBytes(_data.Length);
        byte[] _typeSize = BitConverter.GetBytes(_type.Length);

        _typeSize.CopyTo(_result, 0);
        _dataSize.CopyTo(_result, 32);
        _type.CopyTo(_result,64);
        _data.CopyTo(_result,64+_type.Length);
        return _result;
    }
}

public static class ProbufExtension
{
    public static string SerializeToString<T>(this T obj) where T : IMessage
    {
        using(MemoryStream _ms = new MemoryStream()){
            
            obj.WriteTo(_ms);
            return System.Text.Encoding.Default.GetString(_ms.GetBuffer(),0,(int)_ms.Length);
        }
    }

    public static byte[] SerializeToByteArray<T>(this T obj) where T : IMessage
    {
        return System.Text.Encoding.Default.GetBytes(obj.SerializeToString());
        // using(MemoryStream _ms = new MemoryStream()){
        //     obj.WriteTo(_ms);
        //     return _ms.ToArray();
        // }
    }

    public static T DeserializeFromString<T>(this string InData) where T : class, IMessage<T>, new()
    {
        
        byte[] arr = Convert.FromBase64String(InData);
        using (MemoryStream ms = new MemoryStream(arr))
        {
            MessageParser<T> parser = new MessageParser<T>(() => new T());
            return parser.ParseFrom(ms) as T;
        }
    }

    public static T DeserializeFromByteArray<T>(this byte[] InData) where T : class, IMessage<T>, new()
    {
        using (MemoryStream ms = new MemoryStream(InData))
        {
            MessageParser<T> parser = new MessageParser<T>(() => new T());
            return parser.ParseFrom(ms) as T;
        }
    }

    public static IMessage DeserializeFromTypeString(this byte[] InData,string InType)
    {
        using (MemoryStream ms = new MemoryStream(InData))
        {
            Type _type = Type.GetType(InType);
            var _descriptor =(MessageDescriptor)_type.GetProperty("Descriptor",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);
            return _descriptor.Parser.ParseFrom(InData);
        }
    }


    
}
