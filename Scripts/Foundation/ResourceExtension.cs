using System.Threading;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
namespace UHelper
{
    
public static class ResourceExtension{
    public static IObservable<AudioClip> LoadAudioClip(this ResourceManager resourceManager, string InPath,AudioType InAudioType){
        return Observable.FromCoroutine<AudioClip>((_observer,_cancellationToken)=>LoadAudioClip(InPath,InAudioType,_observer,_cancellationToken));
    }

    private static IEnumerator LoadAudioClip(string InPath,AudioType InAudioType,IObserver<AudioClip> observer, CancellationToken cancellationToken){
        using(UnityWebRequest _www = UnityWebRequestMultimedia.GetAudioClip(InPath,InAudioType)){
            yield return _www.SendWebRequest();
            if(_www.isNetworkError){
                Debug.LogError(_www.error);
                observer.OnError(new Exception(_www.error));
            }else{
                var _audioClip = DownloadHandlerAudioClip.GetContent(_www);
                observer.OnNext(_audioClip);
                observer.OnCompleted();
            }
        }
    }
}


}
