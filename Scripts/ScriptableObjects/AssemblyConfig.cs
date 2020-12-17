using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


namespace UHelper
{


[System.Serializable]

public class UType
{
    public string Name;
    public string FullName;
}


[CreateAssetMenu(fileName = "Assemblies", menuName = "UHelper/Assets/AssemblyConfig", order = 2)]
public class AssemblyConfig : ScriptableObject
{
    private static AssemblyConfig instance = null;
    private static AssemblyConfig Self(){
        if(instance==null)
            instance = Resources.Load<AssemblyConfig>("Assemblies");
        return instance;
    }

    public static void Refresh()
    {
        Self().refresh();
    }

    public static Type GetUType(string InTypeName){
        return Self().getUType(InTypeName);
    }

    public List<string> Assemblies;
    public List<UType> CachedTypes;
    private Dictionary<string,string> allTypes{
        get{
            return CachedTypes.GroupBy(_type=>_type.Name)
                .Select(_group=>_group.First())
                .ToDictionary(_1=>_1.Name,_2=>_2.FullName);
        }
    }

    public List<string> filterBaseTypes = new List<string>{
        typeof(UIBase).AssemblyQualifiedName,
        typeof(SceneScriptBase).AssemblyQualifiedName,
    };

    private Type getUType(string InTypeName)
    {
        if(!allTypes.ContainsKey(InTypeName)){
            return null;
        }
        return Type.GetType(allTypes[InTypeName]);
    }


    public void refresh()
    {
        Assemblies.Clear();
        var _internalAssemblies = getAssemblies("Configs/assemblies");
        var _customAssemblies = getAssemblies(UHelperConfig.AssemblyConfigPath);

        _internalAssemblies.Concat(_customAssemblies).ToList().ForEach(_assemblyName=>{
            LoadNewAssembly(_assemblyName);
        });
    }



    private List<string> getAssemblies(string InResPath){
        var _asset = Resources.Load<TextAsset>(InResPath);
        if(_asset==null){
            Debug.LogWarningFormat("can not load {0}", InResPath);
            return new List<string>();
        }
        return JsonConvert.DeserializeObject<List<string>>(_asset.text);
    }

    public void Awake()
    {
        Debug.Log("Awake");
    }

    public void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }

    public void LoadNewAssembly(string InAssemblyName)
    {
        var _allTypes = allTypes;
        var _assembly = Assembly.Load(new AssemblyName(InAssemblyName));
        if(_assembly==null){
            UnityEngine.Debug.LogWarningFormat("can not load {0}", InAssemblyName);
            return;
        }

        if(!Assemblies.Contains(InAssemblyName)){
            Assemblies.Add(InAssemblyName);
        }

        filterBaseTypes.Select(_filterTypeString=>Type.GetType(_filterTypeString)).ToList()
            .ForEach(_filterType=>{
                Debug.Log(_filterType.FullName);
                _assembly.SubClasses(_filterType).ToList()
                    .ForEach(_type=>{
                        if(!_allTypes.ContainsKey(_type.Name)){
                            CachedTypes.Add(new UType{Name=_type.Name, FullName=_type.AssemblyQualifiedName});
                        }
                    });
            });

        
    }


}


}


