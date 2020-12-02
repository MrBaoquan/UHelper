using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniRx;
namespace UHelper
{


public class UAudioManager : SingletonBehaviour<UAudioManager>
{

    public void PlayMusic(AudioClip InMusic,float InVolume=1.0f)
    {
        musicAudioSource.clip = InMusic;
        musicAudioSource.volume = InVolume;
        musicAudioSource.Play();
    }

    public void PlayMusic(string InMusic, float InVolume=1.0f)
    {
        AudioClip _clip = Managements.Resource.GetRes<AudioClip>(InMusic);
        PlayMusic(_clip,InVolume);
    }

    public void PlayEffect(AudioClip InEffect, Action<AudioClip> InCallback=null)
    {
        effectAudioSource.PlayOneShot(InEffect);
        Observable.Interval(TimeSpan.FromSeconds(InEffect.length+0.1f))
                .First()
                .Subscribe(_1=>{
                    if(InCallback!=null) InCallback(InEffect);
                });
    }

    public void PlayEffect(string InEffect, Action<AudioClip> InCallback=null)
    {
        AudioClip _clip = Managements.Resource.GetRes<AudioClip>(InEffect);
        PlayEffect(_clip,InCallback);
    }


    private AudioSource musicAudioSource;
    private AudioSource effectAudioSource;

    private void Awake() 
    {
        AudioSource[] _audioSources = this.GetComponents<AudioSource>();
        musicAudioSource = _audioSources[0];
        effectAudioSource = _audioSources[1];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


}