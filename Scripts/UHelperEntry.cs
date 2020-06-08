using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * File: UHelperEntry.cs
 * File Created: 2019-10-11 08:50:03
 * Author: MrBaoquan (mrma617@gmail.com)
 * -----
 * Last Modified: 2019-10-25 10:06:12 am
 * Modified By: MrBaoquan (mrma617@gmail.com>)
 * -----
 * Copyright 2019 - 2019 mrma617@gmail.com
 */
 
namespace UHelper
{
public class UHelperEntry:SingletonBehaviour<UHelperEntry>
{
    [HideInInspector]
    public UHelperConfig config;
    private void Awake() 
    {
        Debug.Log("UHelper.Awake");
        if(UHelperEntry.Instance!=this){
            GameObject.Destroy(this);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        
        GameObject _utilGO = new GameObject("UHelperUtils");
        _utilGO.transform.parent = this.transform;
        _utilGO.AddComponent(typeof(MonobehaviourUtil));
        

        // 配置文件
        Managements.Config.Initialize();

        // 1. 初始化配置项
        config = Resources.Load<UHelperConfig>("UHelperConfig");
        
        // 2. 初始化资源管理类
        ResourceManager.Instance.Initialize();

        // 3. 初始化 UI管理类
        UIManager.Instance.Initialize();

        // 4. 初始化场景管理类
        USceneManager.Instance.Initialize();

        this.Initialize();
    }

    private void Initialize()
    {
        AppConfig _config = Managements.Config.Get<AppConfig>();

        if(_config.Screen.Mode==UFullScreenMode.MinimizedWindow){
            WinAPI.ShowWindow(WindowType.SW_SHOWMINIMIZED);
        }else{
            Screen.SetResolution(_config.Screen.Width,_config.Screen.Height,(FullScreenMode)_config.Screen.Mode);
        }
        
    }

    private void OnDestroy() {
        Debug.Log("UHelper.OnDestroy");
    }
    
}  

};
