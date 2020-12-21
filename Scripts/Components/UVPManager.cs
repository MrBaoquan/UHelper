using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


namespace UHelper
{

public class UVPManager : MonoBehaviour
{
    public RenderTexture renderTexture;
    Dictionary<VideoClip,UVideoPlayer> clipPlayers = new Dictionary<VideoClip, UVideoPlayer>();
    Dictionary<string,UVideoPlayer> urlPlayers = new Dictionary<string, UVideoPlayer>();
    UVideoPlayer currentPlayer = null;
    public void PreparePlayers(string[] InUrls)
    {
        urlPlayers.Values.ToList().ForEach(_videoPlayer=>{
            Destroy(_videoPlayer.gameObject);
        });

        InUrls.ToList().ForEach(_url=>{
            var _videoName = Path.GetFileNameWithoutExtension(_url);
            var _newPlayer = new GameObject(_videoName);
            _newPlayer.transform.parent = this.transform;
            var _urlPlayer = _newPlayer.AddComponent<UVideoPlayer>();
            _urlPlayer.BuildRenderTexture(Screen.width,Screen.height);
            _urlPlayer.Render2Texture();
            urlPlayers.Add(_videoName, buildUrlPlayer(_urlPlayer,_url));
        });
    }

    private UVideoPlayer buildUrlPlayer(UVideoPlayer InPlayer, string InUrl)
    {
        InPlayer.DisablePlayOnAwake()
            .DisableLoop()
            .Prepare(InUrl);
        return InPlayer;
    }

    public void PlayByUrl(string InUrl, VideoPlayer.EventHandler OnReachEndHandler=null, int loop=-1, float StartTime=0, float InEndTime=0){
        var _videoName = Path.GetFileNameWithoutExtension(InUrl);
        Debug.LogFormat("UVP:{0}",_videoName);
        if(!urlPlayers.ContainsKey(_videoName)){
            Debug.LogWarning("{0} not exists");
            return;
        }

        if(currentPlayer!=null){
            currentPlayer.Stop();
        }
        currentPlayer = urlPlayers[_videoName];
        currentPlayer.Play(OnReachEndHandler,loop, StartTime, InEndTime);
        renderTexture = currentPlayer.RenderTexture;
        this.BroadcastMessage("OnPlayByUrl",currentPlayer,SendMessageOptions.DontRequireReceiver);
    }

    public void Pause(bool Reset=false)
    {
        if(currentPlayer==null) return;
        if(currentPlayer.isPlaying)
            currentPlayer.Pause();
    }

    public void Stop(){
        if(currentPlayer==null) return;
        currentPlayer.Stop();
    }
    //public string[] urls = null;
    // Start is called before the first frame update
    void Start()
    {
        // Application.targetFrameRate = -1;
        // string _videoPath = Path.Combine(Application.streamingAssetsPath,"Assets/Videos");
        // urls = (new DirectoryInfo(_videoPath)).GetFiles("*.mp4").Select(_=>_.FullName).ToArray();
        // PreparePlayers(urls);
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.T)){
        //     PlayByUrl(urls[3],_=>{
        //         Debug.LogWarning("=============== Play completed ==============");
        //     },1,12,15);
        // }
    }
}



}

