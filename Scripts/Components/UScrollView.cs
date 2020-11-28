﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PathologicalGames;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

namespace UHelper
{

public enum ScrollLayout{
    Horizontal_Normal,
    Horizontal_Loop
}

[RequireComponent(typeof(ScrollRect))]
public class UScrollView : MonoBehaviour
{
    public int MaxShowCount = 5;
    public ScrollLayout scrollLayout = ScrollLayout.Horizontal_Normal;
    ScrollRect scrollRect;
    RectTransform viewport = null;
    RectTransform content = null;

    SpawnPool menuPool = null;
    GameObject menuPoolRoot = null;

    public float _max = 200;
    public int cacheItemsCount = 15;

    [HideInInspector]
    RectTransform rectTransform = null;
    
    public List<Transform> Prefabs = new List<Transform>();
    private void Awake() {
        rectTransform = this.GetComponent<RectTransform>();

        menuPoolRoot = new GameObject("scroll item pool");
        menuPool = PoolManager.Pools.Create("MenuItemPool",menuPoolRoot);
        scrollRect = this.GetComponent<ScrollRect>();
        viewport = scrollRect.viewport;
        content = scrollRect.content;   
    }

    public void AddItem(Transform InItem)
    {
        InItem.name = string.Format("item_{0}",Prefabs.Count);
        menuPool.CreatePrefabPool(new PrefabPool(InItem));
        Prefabs.Add(InItem);
    }

    public void InitItems(){
        if(scrollLayout==ScrollLayout.Horizontal_Normal){
            spawnNormalLayout();
            Managements.Timer.NextFrame(()=>{
                Debug.LogFormat("max show count:{0}", MaxShowCount);
                ScrollTo(MaxShowCount -1 );
            });
        }else if(scrollLayout==ScrollLayout.Horizontal_Loop){
            spawnLoopLayout();
            Managements.Timer.NextFrame(()=>{
                scrollRect.ScrollToCenter(content.GetChild(cacheItemsCount) as RectTransform);
            });
        }
    }

    public void ScrollToPrev()
    {
        int _prevID = ClosetCenterItemIndex() - 1;
        if(_prevID < HalfShowCount){
            _prevID = HalfShowCount;
        }
        ScrollTo(_prevID);
    }

    public void ScrollToNext()
    {
        int _nextID = ClosetCenterItemIndex() + 1;
        if(_nextID>(content.childCount - HalfShowCount - 1)){
            _nextID = content.childCount - HalfShowCount - 1;
        }
        ScrollTo(_nextID);
    }

    // public int MaxShowCount()
    // {
    //     if(content.childCount<=0) return 0;
    //     Bounds _bounds = (content.GetChild(0) as RectTransform).TransformBoundsTo(viewport);
    //     return Mathf.CeilToInt(viewport.rect.width / _bounds.size.x);
    // }

    public int HalfShowCount{
        get{
            return Mathf.FloorToInt(MaxShowCount/2);
        }
    }

    public void ScrollTo(int InIndex)
    {
        Debug.LogFormat("scroll to {0}",InIndex);
        float _position = scrollRect.GetItemNormallizedPosition(content.GetChild(InIndex) as RectTransform);
        DOTween.To(()=>scrollRect.horizontalNormalizedPosition,_=>{
            scrollRect.horizontalNormalizedPosition = _;
        },_position,0.15f);
    }

    private void spawnNormalLayout(){
        Prefabs.ForEach(_=>{
            spawnNext();
        });
        var _halfCount = HalfShowCount;
        for(int _index=0; _index<_halfCount; ++_index){
            var _newPrev = spawnPrev();
            var _newNext =spawnNext();
            _newPrev.SendMessage("DoClear",SendMessageOptions.DontRequireReceiver);
            _newNext.SendMessage("DoClear",SendMessageOptions.DontRequireReceiver);
        }
    }

    private void spawnLoopLayout(){
        spawnItem(0);
        for(int _index=0; _index<cacheItemsCount; ++_index){
            spawnPrev();
            spawnNext();
        }
    }

    private int getItemID(Transform InItem){
        if(InItem==null){
            return 0;
        }
        return InItem.name.Replace("item_",string.Empty).Parse2Int();
    }

    private int nextID(int InID){
        int _next = InID + 1;
        if(_next>=Prefabs.Count){
            _next = 0;
        }
        return _next;
    }

    private int prevID(int InID){
        int _prev = InID -1;
        if(_prev<0){
            _prev = Prefabs.Count-1;
        }
        return _prev;
    }

    private Transform spawnNext(){
        int _currentID = content.childCount<=0?-1:getItemID(content.GetChild(content.childCount-1));
        var _newItem = spawnItem(nextID(_currentID));
        _newItem.SetAsLastSibling();
        return _newItem;
    }

    private Transform spawnPrev(){
        int _currentID = content.childCount<=0?0:getItemID(content.GetChild(0));
        var _newItem = spawnItem(prevID(_currentID));
        _newItem.SetAsFirstSibling();
        return _newItem;
    }

    private Transform spawnItem(int InID){
        var _item =menuPool.Spawn(Prefabs[InID],content);
        _item.name = string.Format("item_{0}",InID);
        return _item;
    }


    public float NormalizedUnitPosition(float InOffset){
        var _bounds = (content.GetChild(0) as RectTransform).TransformBoundsTo(viewport);
        Bounds _contentBounds = content.TransformBoundsTo(viewport);
        Bounds _viewBounds = new Bounds(viewport.rect.center,viewport.rect.size);
        var _hiddenLength =  _contentBounds.size[0] - _viewBounds.size[0];// contentBounds.size[axis] - viewBounds.size[axis];
        return InOffset/_hiddenLength;
    }

    public Transform ClosetCenterItem(){
        return content.gameObject.Children().OrderBy(_=>{
            var _bounds = (_ as RectTransform).TransformBoundsTo(viewport);
            return Mathf.Abs(_bounds.center.x - viewport.rect.center.x);
        }).Take(1).First();
    }

    public int ClosetCenterItemIndex(){
        return ClosetCenterItem().GetSiblingIndex();
    }

    void Start()
    {
        scrollRect.OnEndDragAsObservable().Subscribe(_=>{
            var _rectTransform = ClosetCenterItem() as RectTransform;
             float _normalizedPosition = scrollRect.GetItemNormallizedPosition(_rectTransform);

            DOTween.To(()=>scrollRect.horizontalNormalizedPosition,_2=>{
                scrollRect.horizontalNormalizedPosition = _2;
            },_normalizedPosition,0.15f).OnComplete(()=>{
                
            });

            //refreshItems();
            // var _unit = NormalizedUnitPosition(354);
            // Managements.Timer.NextFrame(()=>{
            //     float _normalizedPosition = scrollRect.GetItemNormallizedPosition(_rectTransform);
            //     Debug.Log(scrollRect.horizontalNormalizedPosition);
            //     Debug.Log(_normalizedPosition);
            //     var _mode = UMath.FMode(scrollRect.horizontalNormalizedPosition, _unit);
            //     scrollRect.horizontalNormalizedPosition = (float)(_normalizedPosition + _mode);
            //     DOTween.To(()=>scrollRect.horizontalNormalizedPosition,_2=>{
            //         scrollRect.horizontalNormalizedPosition = _2;
            //     },_normalizedPosition,0.15f).OnComplete(()=>{
                    
            //     });
            // });

        });   
    }

    private void refreshItems(){
        var _center = ClosetCenterItem();
        syncLeft(_center);
        syncRight(_center);
    }

    private int syncLeft(Transform InCenter){
        int InDelta = cacheItemsCount - InCenter.GetSiblingIndex();
        var _absDetla = Mathf.Abs(InDelta);
        if(InDelta>0){
            Debug.LogFormat("左边补充{0}个",_absDetla);
            for(int _index=0; _index<_absDetla;++_index){
                spawnPrev();
            }
        }else if(InDelta<0){
            Debug.LogFormat("左边移出{0}个",_absDetla);
            for(int _index=0; _index<_absDetla;++_index){
                menuPool.Despawn(content.GetChild(0),menuPoolRoot.transform);
            }
        }
        return _absDetla;
    }

    private void syncRight(Transform InCenter){
        int InDelta = (content.childCount - 1 - InCenter.GetSiblingIndex()) - cacheItemsCount;
        var _absDetla = Mathf.Abs(InDelta);
        if(InDelta>0){
            Debug.LogFormat("右边移出{0}个",_absDetla);
            for(int _index=0; _index<_absDetla; ++_index){
                menuPool.Despawn(content.GetChild(content.childCount-1),menuPoolRoot.transform);
            }
        }else if(InDelta<0){
            Debug.LogFormat("右边补充{0}个", _absDetla);
            for(int _index=0; _index<_absDetla; ++_index){
                spawnNext();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        content.gameObject.Children().ForEach(rectTransform=>{
            var _bounds = (rectTransform as RectTransform).TransformBoundsTo(viewport);
            float _distance = Mathf.Abs(_bounds.center.x - viewport.rect.center.x);
            float _basicScale = 0.75f;
            float _deltaScale = 0f;
            if(_distance<=_max){
                float _percent = Mathf.Clamp(_distance / _max, 0, 1);
                _deltaScale = (1 - _percent) * 0.25f;
                // isCenter = true;
            }else{
                // isCenter = false;
            }
            rectTransform.localScale = Vector3.one * (_basicScale+_deltaScale);
        });
    }
}


}

