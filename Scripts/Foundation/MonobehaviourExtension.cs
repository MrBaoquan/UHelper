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
}

}