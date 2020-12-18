using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;

namespace UHelper
{

public class ConfigManager : Singleton<ConfigManager>
{
    private Dictionary<string,UConfig> configs = new Dictionary<string, UConfig>();
    public void Initialize()
    {
        Type[] _configClasses =  AssemblyConfig.GetSubClasses(typeof(UConfig)).ToArray();// UReflection.SubClasses(typeof(UConfig));

        foreach (var _configClass in _configClasses)
        {
            UConfig _configInstance =  Activator.CreateInstance(_configClass) as UConfig;

            string _configDir = Path.Combine(Application.streamingAssetsPath,"Configs");
            if(!Directory.Exists(_configDir)){
                Directory.CreateDirectory(_configDir);
            }
            string _path = Path.Combine(_configDir,_configClass.Name+".xml");


            if(!File.Exists(_path)){
                UXmlSerialization.Serialize(_configInstance,_path);
            }else{
                MethodInfo _method = typeof(UXmlSerialization).GetMethod("Deserialize").MakeGenericMethod(new Type[]{_configClass});
                _configInstance = _method.Invoke(null,new object[]{_path}) as UConfig;
            }

            UReflection.SetFieldValue(_configInstance,"__path",_path);
            this.configs.Add(_configClass.Name, _configInstance);
        }
    }

    public void SerializeAll()
    {
        this.configs.Values.ToList().ForEach(_config=>{
            UXmlSerialization.Serialize(_config,_config.__path);
        });
    }

    public T Get<T>() where T:class
    {
        string _configName = typeof(T).Name;
        UConfig _config;
        if(this.configs.TryGetValue(_configName,out _config)){
            return _config as T;
        }
        return null;
    }

    public bool Serialize<T>()
    {
        string _configName = typeof(T).Name;
        UConfig _config;
        if(this.configs.TryGetValue(_configName,out _config)){
            UXmlSerialization.Serialize(_config,_config.__path);
            return true;
        }
        return false;
    }
}

}
