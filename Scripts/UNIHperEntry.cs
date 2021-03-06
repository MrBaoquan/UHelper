﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

/*
 * File: UNIHperEntry.cs
 * File Created: 2019-10-11 08:50:03
 * Author: MrBaoquan (mrma617@gmail.com)
 * -----
 * Last Modified: 2019-10-25 10:06:12 am
 * Modified By: MrBaoquan (mrma617@gmail.com>)
 * -----
 * Copyright 2019 - 2019 mrma617@gmail.com
 */

namespace UNIHper {
    public class UNIHperEntry : SingletonBehaviour<UNIHperEntry> {
        private void Awake () {
            Debug.Log ("UNIHper.Awake");
            if (UNIHperEntry.Instance != this) {
                GameObject.Destroy (this);
                return;
            }
            DontDestroyOnLoad (this.gameObject);

            ULog.Initialize ();

            GameObject _utilGO = new GameObject ("UNIHperUtils");
            _utilGO.transform.parent = this.transform;
            _utilGO.AddComponent (typeof (MonobehaviourUtil));

            AssemblyConfig.Refresh ();

            // 1. 配置文件
            Managements.Config.Initialize ();

            // 2. 初始化资源管理类
            ResourceManager.Instance.Initialize ();

            // 3. 初始化 UI管理类
            UIManager.Instance.Initialize ();

            // 4. 初始化场景管理类
            USceneManager.Instance.Initialize ();

            // 5. 初始化Timer管理类
            UTimerManager.Instance.Initialize ();

            // 6. 初始化网络模块
            UNetManager.Instance.Initialize ();
            this.Initialize ();
        }

        private void Initialize () {
            var appConfig = Managements.Config.Get<AppConfig> ();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            bool _fullScreen = (
                    appConfig.PrimaryScreen.Mode == FullScreenMode.ExclusiveFullScreen ||
                    appConfig.PrimaryScreen.Mode == FullScreenMode.FullScreenWindow) ?
                true : false;
            Screen.SetResolution (appConfig.PrimaryScreen.Width, appConfig.PrimaryScreen.Height, _fullScreen);
            Debug.LogFormat ("set fullScreen:{0}, width:{1}, height:{2}",
                _fullScreen, appConfig.PrimaryScreen.Width, appConfig.PrimaryScreen.Height);

            if (_fullScreen) {
                activeAllDisplays ();
            }

            KeepWindowTop ();
#endif
        }

        private void activeAllDisplays () {
            var _config = Managements.Config.Get<AppConfig> ();
            if (_config.Displays.Count <= 0) {
                _config.Displays.Clear ();
                _config.Displays = Display.displays.Select (_ => new ScreenConfig ()).ToList ();
                Managements.Config.Serialize<AppConfig> ();
            }

            int _index = 0;
            _config.Displays.ForEach (_display => {
                if (_index >= Display.displays.Length) return;
                Display.displays[_index].Activate ();
                Display.displays[_index].SetRenderingResolution (_config.Displays[_index].Width, _config.Displays[_index].Height);
                _index++;
            });
        }

        private void Update () {
            if (Input.GetKey (KeyCode.LeftShift) && Input.GetKeyDown (KeyCode.S)) {
                Managements.Config.SerializeAll ();
                Debug.Log ("Save config successfully.");
            }
        }

        private void KeepWindowTop () {
            var _config = Managements.Config.Get<AppConfig> ();
            if (_config.KeepWindowTop.Interval <= 0) return;

            var _window = WinAPI.CurrentWindow ();
            Observable.Interval (TimeSpan.FromSeconds (_config.KeepWindowTop.Interval))
                .Subscribe (_ => {
                    if (_config.KeepWindowTop.SetWindowPos)
                        WinAPI.SetWindowPos (_window,
                            (int) _config.KeepWindowTop.SetWindowPosFunction.HWndInsertAfter,
                            (int) _config.KeepWindowTop.SetWindowPosFunction.SWP_Rect.x,
                            (int) _config.KeepWindowTop.SetWindowPosFunction.SWP_Rect.y,
                            (int) _config.KeepWindowTop.SetWindowPosFunction.SWP_Rect.z,
                            (int) _config.KeepWindowTop.SetWindowPosFunction.SWP_Rect.w,
                            _config.KeepWindowTop.SetWindowPosFunction.SWPFlags.Aggregate ((_flags, _current) => _flags | _current)
                        );
                    if (_config.KeepWindowTop.ShowWindow)
                        WinAPI.ShowWindow (_window, 3);
                    if (_config.KeepWindowTop.SetForegroundWindow)
                        WinAPI.SetForegroundWindow (_window);
                });
        }

        private void OnDestroy () {
            Debug.Log ("UNIHper.OnDestroy");
        }

        private void OnApplicationQuit () {
            Debug.Log ("application quit");
            ULog.Uninitialize ();
        }

    }

};