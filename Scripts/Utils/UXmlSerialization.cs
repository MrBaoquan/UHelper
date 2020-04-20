using System.IO;
using System.Xml.Serialization;

namespace UHelper
{
public static class UXmlSerialization
{
    public static void Serialize(object item, string path)
    {
        XmlSerializer serializer = new XmlSerializer(item.GetType());
        StreamWriter writer = new StreamWriter(path);
        serializer.Serialize(writer.BaseStream, item);
        writer.Close();
    }
    public static T Deserialize<T>(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        StreamReader reader = new StreamReader(path);
        try
        {
            T deserialized = (T)serializer.Deserialize(reader.BaseStream);    
            reader.Close();
            return deserialized;
        }
        catch (System.Exception)
        {
            reader.Close();
            throw;
        }
    }
}

}