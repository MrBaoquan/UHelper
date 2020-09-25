using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UHelper
{
public struct ResourceItem
{
    public string type;
    public string path;
}

public class ResourceManager : Singleton<ResourceManager>,Manageable
{
    private Dictionary<string,List<ResourceItem>> customConfigData;
    private List<ResourceItem> persistConfigData;
    private Dictionary<string,Dictionary<string,UnityEngine.Object>> resources;
    public void Initialize()
    {
        this.ReadConfigData();
        resources = new Dictionary<string, Dictionary<string, UnityEngine.Object>>();
        foreach(var _resource in customConfigData)
        {
            resources.Add(_resource.Key,new Dictionary<string, UnityEngine.Object>());
        }
        LoadAssets(persistConfigData,"Persistence");
        this.LoadAssetByKey("Persistence");
        this.LoadSceneResources();
    }

    public void UnInitialize()
    {

    }


    // 加载当前场景资源
    public void LoadSceneResources()
    {
        string _sceneName = getCurrrentSceneName();
        this.LoadAssetByKey(_sceneName);
    }

    public void LoadSceneResources(string InSceneName)
    {
        Debug.Log("Load scene assets " + InSceneName);
        this.LoadAssetByKey(InSceneName);
    }

    public void UnloadSceneResources(string InSceneName)
    {
        this.UnLoadAssetByKey(InSceneName);
    }

    public T GetRes<T>(string InResName) where T : UnityEngine.Object
    {
        T _resource = GetPersistRes<T>(InResName);
        if(_resource==null)
        {
            _resource = GetSceneRes<T>(InResName);
        }
        
        if(_resource==null){
            Debug.LogWarningFormat("Can not find asset with name: {0}",InResName);
            return null;
        }
        return _resource;
    }

    public T GetSceneRes<T>(string InName) where T:UnityEngine.Object
    { 
        string _sceneName = getCurrrentSceneName();
        Dictionary<string,UnityEngine.Object> _resources;
        if(!resources.TryGetValue(_sceneName,out _resources))
        {
            return null;
        }
        UnityEngine.Object _resource;
        if(!_resources.TryGetValue(InName, out _resource))
        {
            return null;
        }

        return _resource as T;
    }

    public T GetPersistRes<T>(string InName) where T : UnityEngine.Object
    {
        Dictionary<string,UnityEngine.Object> _resources;
        if(!resources.TryGetValue("Persistence",out _resources))
        {
            Debug.LogWarning("There is no Persistence assets.");
            return null;
        }

        UnityEngine.Object _resource = null;
        if(!_resources.TryGetValue(InName, out _resource))
        {
            return null;
        }

        return _resource as T;
    }


    /// <summary>
    /// Private Methods Below
    /// </summary>

    private void ReadConfigData()
    {
        string _resPath = UHelperEntry.Instance.config.resPath;
        TextAsset _resAsset = Resources.Load<TextAsset>(_resPath);
        customConfigData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,List<ResourceItem>>>(_resAsset.text);
        var _persistAsset = Resources.Load<TextAsset>("Configs/Persistence/res");
        persistConfigData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ResourceItem>>(_persistAsset.text);
    }

    private void LoadAssetByKey(string InKey)
    {
        List<ResourceItem> _resItems;
        if(!customConfigData.TryGetValue(InKey,out _resItems))
        {
            return;
        }
        LoadAssets(_resItems,InKey);
    }

    private void LoadAssets(List<ResourceItem> InItems, string InResID){
        foreach(var _item in InItems)
        {
            var _T = Type.GetType("UnityEngine."+_item.type+",UnityEngine");
            UnityEngine.Object[] _resources = Resources.LoadAll(_item.path,_T);
            foreach(var _resource in _resources)
            {
                if(resources[InResID].ContainsKey(_resource.name)){
                    Debug.LogErrorFormat("resource key can not duplicate, error key: {0}",_resource.name);
                    continue;
                }
                Debug.LogFormat("{0} Add resource {1}", InResID, _resource.name);
                resources[InResID].Add(_resource.name,_resource);
            }
        }
    }

    private void UnLoadAssetByKey(string InKey)
    {
        resources[InKey].Clear();
    }

    private string getCurrrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}

}
