﻿using System.Linq;
using System.Net.Mime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UHelper
{

public abstract class UEvent{}

public interface IEventHandler{
    void SubscribeEvents();
    void UnsubscribeEvents();
}


public class UEventManager : Singleton<UEventManager>,Manageable
{
    private Dictionary<Type,Action<UEvent>> delegates = new Dictionary<Type, Action<UEvent>>();
    private Dictionary<Delegate,Action<UEvent>> lookup = new Dictionary<Delegate, Action<UEvent>>();
    public void Register<T>(Action<T> InDelegate) where T : UEvent
    {
        if(lookup.ContainsKey(InDelegate)){
            Debug.LogWarning("has been registered already");
            return;
        }
        Action<UEvent> _newDelegate = _event=>InDelegate(_event as T);
        lookup.Add(InDelegate,_newDelegate);

        Type _actionKey = typeof(T);
        Action<UEvent> _delegate;
        if(delegates.TryGetValue(_actionKey,out _delegate)){
            _delegate += _newDelegate;
            delegates[_actionKey] = _delegate;
        }else{
            delegates.Add(_actionKey,_newDelegate);
        }
    }

    public void Unregister<T>(Action<T> InDelegate) where T : UEvent
    {
        Action<UEvent> _internal_action;
        if(lookup.TryGetValue(InDelegate,out _internal_action)){
            Action<UEvent> _delegate;
            Type _actionKey = typeof(T);
            if(delegates.TryGetValue(_actionKey,out _delegate)){
                _delegate -= _internal_action;
                if(_delegate==null){
                    delegates.Remove(_actionKey);
                }else{
                    delegates[_actionKey] = _delegate;
                }
            }
            lookup.Remove(InDelegate);
        }   
    }

    public void Unregister<T>() where T : UEvent
    {
        Action<UEvent> _delegate;
        Type _actionKey = typeof(T);
        if(delegates.TryGetValue(_actionKey,out _delegate)){
            delegates.Remove(_actionKey);
            var _delegates = _delegate.GetInvocationList();
            _delegates.ToList().ForEach(_=>{
                lookup.Remove(_);
            });
        }
    }

    public void Fire(UEvent InEvent)
    {
        Action<UEvent> _internal_action;
        if(delegates.TryGetValue(InEvent.GetType(),out _internal_action)){
            _internal_action.Invoke(InEvent);
        }
    }
    
    public void Initialize()
    {
     
        
    }

    public void Uninitialize()
    {

    }


}




}
