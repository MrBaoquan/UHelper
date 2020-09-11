using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

using UHelper;
public class SphereLayout : MonoBehaviour
{
    public float distance = 1000f;
    public float angle = 60f;
    public float rotation = 0f;

    [SerializeField]
    bool loop = false;

    private int currentIndex = 0;
    private int NextIndex{
        get {
            return currentIndex+1>=transform.childCount?loop?0:transform.childCount-1:currentIndex+1;
        }
    }

    private int PrevIndex{
        get{
            return currentIndex-1<0?loop?transform.childCount:0:currentIndex-1;
        }
    }

    private void OnEnable() {
        Debug.Log("on enable");
    }

    private void OnValidate() {
        syncLayout();
    }

    public int SelectNext()
    {
        currentIndex = NextIndex;
        syncRotation();
        return currentIndex;
    }

    public int SelectPrev(){
        currentIndex = PrevIndex;
        syncRotation();
        return currentIndex;
    }

    public void Select(int ChildIndex)
    {
        currentIndex = ChildIndex;
        syncRotation();
    }

    void syncLayout(){
        Vector3 _startPoint = transform.position + transform.forward * -distance;
        transform.localEulerAngles = Vector3.right * rotation;
        int _index = 0;
        var _children = gameObject.Children();
        //_children.Reverse();
        _children.ForEach(_transform=>{
            _transform.position = _startPoint.Rotate(transform.right * _index* -angle, transform.position);
            _transform.rotation = Quaternion.Euler(Vector3.forward*-1);

            float _delta = (transform.childCount - Mathf.Abs(currentIndex - _index)) * 10.0f;
            _transform.DOScale(Vector3.one *_delta,0.3f);
            ++_index;
        });
    }

    void syncRotation(){
        DOTween.To(()=>rotation,_=>{
            rotation=_;
            transform.rotation = Quaternion.Euler(Vector3.right*_);
            syncLayout();
        },(currentIndex*angle),0.3f);
    }

    private void Start() {
        syncLayout();
    }

    void Update()
    {
    }
}