using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UHelper
{
public abstract class UIBase : MonoBehaviour
{
    private UIType uiType = UIType.Normal;
    public UIType  Type
    {
        get{
            return uiType;
        }

        set{
            uiType = value;
        }
    }

    protected bool bShow = false;
    public bool isShowing{
        get {return bShow;}
    }
    
    public virtual void OnLoad()
    {
     
    }

    public virtual void Show()
    {
        if(!this.gameObject.activeInHierarchy){
            this.gameObject.SetActive(true);
        }
        bShow = true;
        this.OnShow();
        handleShowAction();
    }

    protected virtual void handleShowAction()
    {
        this.gameObject.SetActive(true);
    }

    protected virtual void handleHideAction()
    {
        this.gameObject.SetActive(false);
    }

    public virtual void Hide()
    {
        bShow = false;
        this.OnHidden();
        handleHideAction();
    }

    protected virtual void OnShow()
    {

    }

    protected virtual void OnHidden()
    {

    }
}

}