using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UHelper
{

public class UInputField : MonoBehaviour
{
    public string title = "标题";
    public float minValue = 0;
    public float maxValue = 100f;
    public float defaultValue = 50f;

    public float Value{
        set{
            syncValue(value);
        }
    }

    private Text textTitle;
    private InputField inputValue;
    private Slider sliderValue;

    public Action<float> onValueChanged;

    private void OnValidate() {
        BuildRefs();
        textTitle.text = title;
        inputValue.text = defaultValue.ToString();
        sliderValue.minValue = minValue;
        sliderValue.maxValue = maxValue;
        sliderValue.value = defaultValue;
    }

    void BuildRefs(){
        textTitle = this.Get<Text>("text_title");
        inputValue = this.Get<InputField>("input_value");
        sliderValue = this.Get<Slider>("slider_value");
    }

    // Start is called before the first frame update
    void Start()
    {
        BuildRefs();
        inputValue.onValueChanged.AddListener(_=>{
            float _value = _.Parse2Float();
            sliderValue.value = _value;
            onValueChange();
        });
        sliderValue.onValueChanged.AddListener(_=>{
            inputValue.text = _.ToString("0.00");
            sliderValue.value = Mathf.Clamp(_,minValue,maxValue);
            onValueChange();
        });
    }

    void syncValue(float InValue){
        sliderValue.value = InValue;
    }

    void onValueChange(){
        if(onValueChanged!=null){
            onValueChanged(sliderValue.value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}




}

