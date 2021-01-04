﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UHelper
{
public static class MonobehaviourExtension
{

    public static T Get<T>(this MonoBehaviour _behaviour) where T : Component
    {
        return _behaviour.GetComponent<T>();
    }

    public static T Get<T>(this MonoBehaviour _behaviour, string InPath) where T : Component
    {
        Transform _target = Get(_behaviour,InPath);
        return _target.GetComponent<T>();
    }

    public static Transform Get(this MonoBehaviour _behaviour, string InPath){
        Transform _target = _behaviour.transform.Find(InPath);
        if(_target==null){
            //Debug.LogWarningFormat("Can not find gameobjet with path: {0}",InPath);
            return null;
        }
        return _target;
    }

    public static bool Contain<T>(this MonoBehaviour _behaviour, T _component) where T : Component
    {
        Component _out_component;
        return _behaviour.TryGetComponent(_component.GetType(),out _out_component);
    }

    public static bool Contains<T>(this MonoBehaviour _behaviour) where T : Component
    {
        Component _out_component;
        return _behaviour.TryGetComponent(typeof(T), out _out_component);
    }

    public static bool Contains<T>(this Transform _transform) where T :Component
    {
        T _component;
        return _transform.gameObject.TryGetComponent<T>(out _component);
    }

    public static bool Contains(this Transform _transform, Type _type)
    {
        Component _out_component;
        return _transform.TryGetComponent(_type, out _out_component);
    }

    public static T AddComponent<T>(this MonoBehaviour _behaviour) where T : Component
    {
        if(!_behaviour.Contains<T>()){
            return _behaviour.gameObject.AddComponent<T>();
        }
        return _behaviour.GetComponent<T>();
    }

    public static T AddComponent<T>(this Transform _transform) where T : Component
    {
        if(_transform.Contains<T>()){
            return _transform.GetComponent<T>();
        }
        return _transform.gameObject.AddComponent<T>();
    }

    public static void SetActive(this Transform _transform, bool Value){
        _transform.gameObject.SetActive(Value);
    }

    public static void SetChildrenActive(this Transform _transform, bool bActive){
        _transform.gameObject.SetChildrenActive(bActive);
    }

    // 设置指定子元素激活状态
    public static void SetChildrenActive(this GameObject _self, bool bActive){
     
        for(int _index=0; _index<_self.transform.childCount;++_index){
            var _go = _self.transform.GetChild(_index).gameObject;
            if(_go.activeInHierarchy!=bActive){
                _go.SetActive(bActive);
            }
        }
    }

    public static GameObject Child(this GameObject _self, string InName){
        var _child = _self.transform.Find(InName);
        return _child?_child.gameObject:null;
    }

    public static void SetChildrenActive(this GameObject _self, bool bActive, int StartIndex, int EndIndex=0, bool bRevertOther=false){
     
        int _endIndex = EndIndex==0?_self.transform.childCount:EndIndex<0?_self.transform.childCount+EndIndex:EndIndex;
        for(int _index=StartIndex; _index< _endIndex; ++_index){
            var _go = _self.transform.GetChild(_index).gameObject;
            if(_go.activeInHierarchy!=bActive){
                _go.SetActive(bActive);
            }
        }

        for(int _index=0;_index<StartIndex;++_index){
            var _go = _self.transform.GetChild(_index).gameObject;
            if(_go.activeInHierarchy==bActive){
                _go.SetActive(!bActive);
            }
        }

        if(!bRevertOther) return;
        
        for(int _index=_endIndex;_index<_self.transform.childCount;++_index){
            var _go = _self.transform.GetChild(_index).gameObject;
            if(_go.activeInHierarchy==bActive){
                _go.SetActive(!bActive);
            }
        }
    }


    // 获取指定子元素激活状态
    public static bool IsChildrenActive(this GameObject _self, int Index=-1){
        if(Index==-1){
            for(int _index=0; _index<_self.transform.childCount;++_index){
                if(!_self.transform.GetChild(_index).gameObject.activeInHierarchy){
                    return false;
                }
            }
        }
        if(Index >= _self.transform.childCount){
            return false;
        }
        return _self.transform.GetChild(Index).gameObject.activeInHierarchy;
    }



    // 获取子元素
    public static List<Transform> Children(this GameObject _self, bool bOnlyEnabled=true){
        List<Transform> _children = new List<Transform>();
        for(int _index=0; _index<_self.transform.childCount;++_index){
            if(bOnlyEnabled){
                if(_self.transform.GetChild(_index).gameObject.activeInHierarchy){
                    _children.Add(_self.transform.GetChild(_index));
                }
            }else{
                _children.Add(_self.transform.GetChild(_index));
            }   
        }
        return _children;
    }

    public static List<Transform> Children(this Transform _self, bool bOnlyEnabled=true){
        return _self.gameObject.Children(bOnlyEnabled);
    }

    public static T GetComponent<T>(this Transform _self) where T : Component
    {
        return _self.gameObject.GetComponent<T>();
    }


        static Vector3[] corners = new Vector3[4];
    
    /// <summary>
    /// Transform the bounds of the current rect transform to the space of another transform.
    /// </summary>
    /// <param name="source">The rect to transform</param>
    /// <param name="target">The target space to transform to</param>
    /// <returns>The transformed bounds</returns>
    public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
    {
        // Based on code in ScrollRect's internal GetBounds and InternalGetBounds methods
        var bounds = new Bounds();
        if (source != null) {
            source.GetWorldCorners(corners);

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var matrix = target.worldToLocalMatrix;
            for (int j = 0; j < 4; j++) {
                Vector3 v = matrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
        }
        return bounds;
    }

    /// <summary>
    /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
    /// </summary>
    /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
    /// <param name="distance">The distance in the scroll rect's view's coordiante space</param>
    /// <returns>The normalized scoll distance</returns>
    public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
    {
        // Based on code in ScrollRect's internal SetNormalizedPosition method
        var viewport = scrollRect.viewport;
        var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
        var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

        var content = scrollRect.content;
        var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

        var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
        return distance / hiddenLength;
    }

    /// <summary>
    /// Scroll the target element to the vertical center of the scroll rect's viewport.
    /// Assumes the target element is part of the scroll rect's contents.
    /// </summary>
    /// <param name="scrollRect">Scroll rect to scroll</param>
    /// <param name="target">Element of the scroll rect's content to center vertically</param>
    public static void ScrollToCenter(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis = RectTransform.Axis.Horizontal)
    {
        float _centerPosition = scrollRect.GetItemNormallizedPosition(target, axis);
        if (axis == RectTransform.Axis.Vertical) {
            scrollRect.verticalNormalizedPosition = _centerPosition;
        } else {
            scrollRect.horizontalNormalizedPosition = _centerPosition;
        }
    }

    public static float GetItemNormallizedPosition(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis = RectTransform.Axis.Horizontal)
    {
        // The scroll rect's view's space is used to calculate scroll position
        var view = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();

        // Calcualte the scroll offset in the view's space
        var viewRect = view.rect;
        var elementBounds = target.TransformBoundsTo(view);

        float _centerPosition = 0;
        // Normalize and apply the calculated offset
        if (axis == RectTransform.Axis.Vertical) {
            var offset = viewRect.center.y - elementBounds.center.y;
            var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
            _centerPosition =  Mathf.Clamp(scrollPos, 0, 1);
            //scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollPos, 0, 1);
        } else {
            var offset = viewRect.center.x - elementBounds.center.x;
            var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
            _centerPosition = Mathf.Clamp(scrollPos, 0, 1);
            //scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollPos, 0, 1);
        }
        Debug.Log(_centerPosition);
        return _centerPosition;
    }




}

}