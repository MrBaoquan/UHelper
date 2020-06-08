using System.Xml.Linq;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UHelper
{

public enum UIType
{
    Normal,
    Standalone,
    Popup
}

public class UIManager : Singleton<UIManager>,Manageable
{
    private Transform UIRoot = null;

    private Transform NormalUIRoot = null;
    private Transform StandaloneUIRoot = null;
    private Transform PopupUIRoot = null;

    private class UIConfig
    {
        [JsonProperty("asset")]
        public string Asset = string.Empty;
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UIType Type = UIType.Normal;

        [JsonProperty("script")]
        public string script = string.Empty;

        public string GetScript(string InDefault)
        {
            if(script==string.Empty){
                return InDefault;
            }
            return script;
        }

        public string GetAssetName(string InDefault)
        {
            if(Asset==string.Empty){
                return InDefault;
            }
            return Asset;
        }
    }
    
    private Dictionary<string,Dictionary<string,UIConfig>> rawUIConfigData = null;
    
    private Dictionary<string,UIBase> allSpawnedUICaches = new Dictionary<string, UIBase>();
    private Dictionary<string,UIBase> normalUIs = new Dictionary<string, UIBase>();

    private Dictionary<string,UIBase> standaloneUIs = new Dictionary<string, UIBase>();
    private List<UIBase> popupUIs = new List<UIBase>();
    public void Initialize()
    {
        TargetUIRoot();
        ReadConfigData();
    }

    public void OnEnterScene(string InSceneName)
    {
        foreach(var _uiComponent in allSpawnedUICaches)
        {
            GameObject.Destroy(_uiComponent.Value.gameObject);
        }
        allSpawnedUICaches.Clear();
        popupUIs.Clear();
        normalUIs.Clear();
        Dictionary<string,UIConfig> _uis = null;
        if(!rawUIConfigData.TryGetValue(InSceneName, out _uis)){
            Debug.LogWarningFormat("Find nothing ui in scene {0}",InSceneName);
            return;
        }
        SpawnUIS(_uis);
    }

    public void ShowUI(string InKey, Action<UIBase> InHandler=null)
    {
        UIBase _uiComponent = null;
        if(!allSpawnedUICaches.TryGetValue(InKey, out _uiComponent)){
            Debug.LogWarningFormat("Show ui {0} failed. UI {0} not exits.",InKey);
            return;
        }
        ShowUI(InKey,_uiComponent);
        if(InHandler!=null){
            InHandler(_uiComponent);
        }
    }

    public T ShowUI<T>(Action<T> InHandler=null) where T : UIBase
    {
        string _uiKey = typeof(T).Name;
        UIBase _uiComponent = null;
        if(!allSpawnedUICaches.TryGetValue(_uiKey, out _uiComponent)){
            Debug.LogWarningFormat("Show ui {0} failed. UI {0} not exits.",_uiKey);
            return null;
        }
        ShowUI(_uiKey,_uiComponent);
        if(InHandler!=null){
            InHandler(_uiComponent as T);
        }
        return _uiComponent as T;
    }

    public T ShowUI<T>(string InKey, Action<T> InHandler=null) where T : UIBase
    {
        UIBase _uiComponent = null;
        if(!allSpawnedUICaches.TryGetValue(InKey, out _uiComponent)){
            Debug.LogWarningFormat("Show ui {0} failed. UI {0} not exits.",InKey);
            return null;
        }
        ShowUI(InKey,_uiComponent);
        if(InHandler!=null){
            InHandler(_uiComponent as T);
        }
        return _uiComponent as T;
    }

    public T ShowUI<T>(string InKey) where T : UIBase
    {
        UIBase _uiComponent = null;
        if(!allSpawnedUICaches.TryGetValue(InKey, out _uiComponent)){
            Debug.LogWarningFormat("Show ui {0} failed. UI {0} not exits.",InKey);
            return null;
        }
        ShowUI(InKey,_uiComponent);
        return _uiComponent as T;
    }

    public T GetUI<T>(string InKey="") where T : UIBase
    {
        if(InKey == ""){
            InKey = typeof(T).Name;
        }
        UIBase _uiComponent = null;
        if(!allSpawnedUICaches.TryGetValue(InKey,out _uiComponent)){
            return null;
        }
        return _uiComponent as T;
    }

    public void HideUI(string InKey)
    {
        UIBase _uiComponent;
        if(!allSpawnedUICaches.TryGetValue(InKey, out _uiComponent))
        {
            Debug.LogWarningFormat("Hide ui {0} failed. UI {0} not exits.",InKey);
            return;
        }
        UIType _uiType = _uiComponent.Type;
        switch(_uiType)
        {
            case UIType.Normal:
                hideNormalUI(InKey);
                break;
            case UIType.Standalone:
                hideStandaloneUI(InKey);
                break;
            case UIType.Popup:
                hidePopupUI(InKey);
                break;
        }

    }

    public void UnInitialize()
    {
        
    }



    /// <summary>
    /// Private Methods
    /// </summary>
    private void TargetUIRoot()
    {
        GameObject _uiRoot = GameObject.Find("UIRoot");
        if(_uiRoot){
            UIRoot = _uiRoot.transform;
        }else{
            Debug.LogWarning("Cannot find UIRoot node, it will caused ui no effect.");
            return;
        }

        NormalUIRoot = UIRoot.Find("NormalUIRoot");
        StandaloneUIRoot = UIRoot.Find("StandaloneUIRoot");
        PopupUIRoot = UIRoot.Find("PopupUIRoot");
    }

    private void ReadConfigData()
    {
        string _uiPath = UHelperEntry.Instance.config.uiPath;
        TextAsset _uiAsset = Resources.Load<TextAsset>(_uiPath);
        rawUIConfigData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,Dictionary<string,UIConfig>>>(_uiAsset.text);
    }

    private void SpawnUIS(Dictionary<string,UIConfig> InUIConfigs)
    {
        foreach(var _uiConfig in InUIConfigs)
        {
            SpawnUI(_uiConfig.Key,_uiConfig.Value);
        }
    }

    private void SpawnUI(string InUIKey,UIConfig InUIConfig)
    {
        string _scriptName = InUIConfig.GetScript(InUIKey);
        Type _T = Type.GetType(_scriptName);
        if(_T==null)
        {
            Debug.LogWarningFormat("no class name match: {0}, spawn ui {0} failed", _scriptName);
            return;
        }

        GameObject _uiPrefab = ResourceManager.Instance.GetRes<GameObject>(InUIConfig.GetAssetName(InUIKey));
        _uiPrefab.SetActive(false);
        GameObject _newUI = GameObject.Instantiate(_uiPrefab,NormalUIRoot);

        UIBase _uiComponent = _newUI.GetComponent(_T) as UIBase;
        if(!_uiComponent){
            _uiComponent = _newUI.AddComponent(_T) as UIBase;
        }
        _uiComponent.Type = InUIConfig.Type;
        _uiComponent.OnSpawned();
        _newUI.transform.SetParent(getParentUIAttachTo(_uiComponent.Type));
        allSpawnedUICaches.Add(InUIKey,_uiComponent);
    }

    private Transform getParentUIAttachTo(UIType InUIType)
    {
        switch(InUIType)
        {
            case UIType.Normal:
                return NormalUIRoot;
            case UIType.Standalone:
                return StandaloneUIRoot;
            case UIType.Popup:
                return PopupUIRoot;
        }
        return null;
    }


    private void ShowUI(string InKey, UIBase InUIComponent)
    {
        UIType _uiType = InUIComponent.Type;
        switch(_uiType)
        {
            case UIType.Normal:
                showNormalUI(InKey);
                break;
            case UIType.Standalone:
                showStandaloneUI(InKey);
                break;
            case UIType.Popup:
                showPopupUI(InKey);
                break;
        }
    }

    private void showNormalUI(string InKey)
    {
        UIBase _uiComponent = allSpawnedUICaches[InKey];
        _uiComponent.Show();
        normalUIs.Add(InKey,_uiComponent);
    }

    private void hideNormalUI(string InKey)
    {
        UIBase _uiComponent;
        if(!normalUIs.TryGetValue(InKey, out _uiComponent)){
            return;
        }
        _uiComponent.Hidden();
        normalUIs.Remove(InKey);
    }

    private void showStandaloneUI(string InKey)
    {
        UIBase _uiComponent = allSpawnedUICaches[InKey];
        foreach(var _uiItem in standaloneUIs){
            _uiItem.Value.Hidden();
        }
        _uiComponent.Show();
        if (!standaloneUIs.Keys.Contains(InKey))
        {
            standaloneUIs.Add(InKey, _uiComponent);
        }
        
    }

    private void hideStandaloneUI(string InKey)
    {
        UIBase _uiComponent;
        if(!standaloneUIs.TryGetValue(InKey,out _uiComponent)){
            return;
        }
        _uiComponent.Hidden();
        standaloneUIs.Remove(InKey);

        var _last = standaloneUIs.LastOrDefault();
        if(!_last.Equals(default)){
            _last.Value.Show();
        }
    }

    private void showPopupUI(string InKey)
    {
        UIBase _uiComponent = allSpawnedUICaches[InKey];
        if(_uiComponent==null){return;}

        if(!popupUIs.Contains(_uiComponent)){
            popupUIs.Add(_uiComponent);
        }else{
            if(popupUIs.Remove(_uiComponent)){
                popupUIs.Add(_uiComponent);
            }
        }
        _uiComponent.Show();
        _uiComponent.transform.SetAsLastSibling();
    }

    private void hidePopupUI(string InKey="")
    {
        if(popupUIs.Count<=0){
            return;
        }
        UIBase _uiComponent = null;
        if(InKey==""){
            _uiComponent = popupUIs.FirstOrDefault();
        }else{
            _uiComponent = allSpawnedUICaches[InKey];
        }
        
        if(!popupUIs.Contains(_uiComponent)){
            return;
        }
        _uiComponent.Hidden();
        popupUIs.Remove(_uiComponent);
    }

}

}
