using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UHelper
{
public static class MonobehaviourExtension
{
    public static T Get<T>(this MonoBehaviour _behaviour,string InPath) where T : Behaviour
    {
        Transform _target = _behaviour.transform.Find(InPath);
        if(_target==null){
            Debug.LogWarningFormat("Can not find gameobjet with path: {0}",InPath);
            return null;
        }

        return _target.GetComponent<T>();
    }

    public static bool Contain<T>(this MonoBehaviour _behaviour, T _component) where T :Behaviour
    {
        Component _out_component;
        return _behaviour.TryGetComponent(_component.GetType(),out _out_component);
    }

    public static bool Contain(this Transform _transform, Type _type)
    {
        Component _out_component;
        return _transform.TryGetComponent(_type, out _out_component);
    }
}

}