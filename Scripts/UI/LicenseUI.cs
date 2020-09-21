using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UHelper;
using UniRx;

public class LicenseUI : UIBase
{
    const string encrypter_key = "license_key";
    public bool IsValid(){
        try
        {
            var _local_license_key = PlayerPrefs.GetString(encrypter_key,"unset");
            var _cpuid_a = Machine.CPUID;
            var _cpuid_b = Encrypter.Decrypt(_local_license_key);
            return _cpuid_b==_cpuid_a;
        }
        catch (System.Exception)
        {
            return false;
            throw;
        }
    }

    public void Check(){
        if(!IsValid()){
            Managements.UI.ShowUI<LicenseUI>();
        }
    }

    private ReactiveProperty<string> licenseContent = new ReactiveProperty<string>(string.Empty);
    // Start is called before the first frame update
    private void Start()
    {
        this.Get<InputField>("input_license").OnValueChangedAsObservable().Subscribe(_=>{
            licenseContent.Value = _;
        });

        licenseContent.Subscribe(_=>{
            this.Get<Button>("btn_active").gameObject.SetActive(_.Length>=128);
        });

        this.Get<Button>("btn_active").OnClickAsObservable().Subscribe(_1=>{
            try
            {
                var _cpuid = Machine.CPUID;
                string _input_key = Encrypter.Decrypt(licenseContent.Value);
                if(_input_key==_cpuid){
                    Managements.UI.ShowDialog("序列号有效, 软件已成功激活!",_=>{
                        PlayerPrefs.SetString(encrypter_key,licenseContent.Value);
                        Managements.UI.HideUI("LicenseUI");
                    });
                }else{
                    Managements.UI.ShowDialog("请输入正确的激活码!",_=>{});
                }
            }
            catch (System.Exception)
            {
                    Managements.UI.ShowDialog("请输入正确的激活码!",_=>{});
                throw;
            }
          
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
