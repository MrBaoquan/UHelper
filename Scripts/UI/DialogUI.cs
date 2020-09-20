using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UHelper;
using UniRx;
public class DialogUI : UIBase
{

    Action<DialogUI> onConfirm = null;

    public void SetCallback(Action<DialogUI> InCallback){
        onConfirm = InCallback;
    }

    public void SetContent(string InContent){
        this.Get<Text>("text_content").text = InContent;
    }


    // Start is called before the first frame update
    private void Start()
    {
        this.Get<Button>("btn_confirm").OnClickAsObservable().Subscribe(_=>{
            Managements.UI.HideUI("DialogUI");
            if(onConfirm!=null) onConfirm.Invoke(this);
        });
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    // Called when this ui is showing
    protected override void OnShow()
    {

    }

    // Called when this ui is hidden
    protected override void OnHidden()
    {

    }
}
