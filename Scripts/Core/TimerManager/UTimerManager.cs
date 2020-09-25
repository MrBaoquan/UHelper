﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UHelper;
using UniRx;

public class UTimerManager : Singleton<UTimerManager>,Manageable
{
    public void Initialize(){}

    public void UnInitialize(){}

    public void SetTimeout(float InDuration, Action OnCompleted = null, Action<float> OnUpdate = null, float InInterval=0.05f)
    {
        float _startTime = Time.time;
        IDisposable _timerHandler = null;
        _timerHandler = Observable.Interval(TimeSpan.FromSeconds(InInterval)).Where((_1,_2)=>{
            float _delta = Time.time - _startTime;
            bool _condition = _delta <= InDuration;
            if(_condition){
                float _progress = Mathf.Clamp(_delta / InDuration, 0 ,1);
               if(OnUpdate!=null) OnUpdate(_progress);
            }else{
                if(OnUpdate!=null) OnUpdate(1.0f);
                _timerHandler.Dispose();
                if(OnCompleted!=null) OnCompleted();
            }
            return _condition;
        }).Subscribe(_=>{});
    }

}