﻿using System.Collections;
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
    public virtual void OnSpawned()
    {
     
    }

    public virtual void Show()
    {
        this.gameObject.SetActive(true);
        this.OnShow();
    }

    public virtual void Hidden()
    {
        this.gameObject.SetActive(false);
        this.OnHidden();
    }

    protected virtual void OnShow()
    {

    }

    protected virtual void OnHidden()
    {

    }
}

}