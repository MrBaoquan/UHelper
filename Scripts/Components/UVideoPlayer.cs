using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UniRx;


namespace UHelper
{


[RequireComponent(typeof(VideoPlayer))]
public class UVideoPlayer : MonoBehaviour
{
    public VideoRenderMode renderMode = VideoRenderMode.RenderTexture;
    public bool looping = false;
    public bool Looping
    {
        set{
            looping = value;
        }
        get {
            return looping;
        }
    }

    public bool isPlaying
    {
        get {
            if(videoPlayer!=null){
                return videoPlayer.isPlaying;
            }
            return false;
        }
    }

    public string Url
    {
        set {
            if(videoPlayer!=null){
                videoPlayer.url = value;
            }
        }

        get{
            if(videoPlayer!=null){
                return videoPlayer.url;
            }
            return string.Empty;
        }
    }

    public double Time{
        get {return videoPlayer.time;}
    }
    private RectTransform rectTransform = null;
    private VideoPlayer videoPlayer = null;
    private RawImage videoImage = null;

    private RenderTexture renderTexture = null;
    public RenderTexture RenderTexture{
        get{
            return renderTexture;
        }
    }

    public RenderTexture BuildRenderTexture(int InWidth, int InHeight, int InDepth=0, RenderTextureFormat InFormat=RenderTextureFormat.ARGB32){
        renderTexture = new RenderTexture(InWidth, InHeight, InDepth, InFormat);
        return renderTexture;
    }

    public void Render2Texture()
    {
        if(renderTexture==null) return;
        Render2Texture(renderTexture);
    }

    public void Render2Texture(RenderTexture InTexture = null)
    {
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        if(InTexture == null){
            var _renderer = this.GetComponent<RawImage>();
            if(_renderer==null){
                Debug.Log("UVP:There is no renderer component. from Render2Texture");
                return;
            }

            rectTransform = this.transform as RectTransform;
            if(rectTransform!=null){
                RenderTexture _videoRT = new RenderTexture((int)rectTransform.rect.width,(int)rectTransform.rect.height,0,RenderTextureFormat.ARGB32);
                videoPlayer.targetTexture = _videoRT;
                _renderer.texture = _videoRT;
                return;
            }
        }
        videoPlayer.targetTexture = InTexture;
    }

    public void Render2Texture(Material InMaterial){
        InMaterial.SetTexture("_MainTex",videoPlayer.targetTexture);
    }

    public void Render2Material(Renderer InRenderer=null){
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        if(InRenderer==null){
            InRenderer = this.GetComponent<Renderer>();
            if(InRenderer==null){
                Debug.LogWarning("UVP: There is no renderer component. from Render2Material");
                return;
            }
        }
        videoPlayer.targetMaterialRenderer = InRenderer;
    }

    public UVideoPlayer DisablePlayOnAwake(){
        videoPlayer.playOnAwake = false;
        return this;
    }

    public UVideoPlayer DisableLoop(){
        videoPlayer.isLooping = false;
        return this;
    }

    private void Awake() {
        buildRefs();
        videoPlayer.isLooping = false;
        //syncRenderMode();

        videoPlayer.errorReceived+=(_vp,_error)=>{
            Debug.LogError(_error);
        };

        videoPlayer.started+=(_)=>{
            //Debug.LogFormat("UVP_{0} ============== Started: {1}", UnityEngine.Time.time, videoPlayer.time);
        };
    }

    void buildRefs(){
        if(videoPlayer==null)
            videoPlayer = this.GetComponent<VideoPlayer>();
    }

    private VideoPlayer.EventHandler vpReachEnd = null;
    private IDisposable vpLoopTimer = null;

    /**
     *  loop -1 根据Looping属性决定 0 不循环   1循环
     */
    public void PlayByUrl(string InUrl, VideoPlayer.EventHandler OnReachEndHandler=null, int loop=-1, float StartTime=0, float InEndTime=0)
    {
        videoPlayer.url = InUrl;
        this.Play(OnReachEndHandler,loop,StartTime,InEndTime);   
    }

    public void Play(VideoClip InClip, VideoPlayer.EventHandler OnReachEndHandler=null, int loop=-1, float StartTime=0, float InEndTime=0){
        videoPlayer.clip = InClip;
        Play(OnReachEndHandler,loop,StartTime,InEndTime);
    }

    public void Play(VideoPlayer.EventHandler OnReachEndHandler=null, int loop=-1, float StartTime=0, float InEndTime=0)
    {
        if(videoPlayer.isPlaying){
            videoPlayer.Pause();    
            this.realPlay(OnReachEndHandler,loop,StartTime,InEndTime);
        }else if(!videoPlayer.isPrepared){
            this.Prepare(_=>{
                this.realPlay(OnReachEndHandler,loop,StartTime,InEndTime);
            });
        }else{
            this.realPlay(OnReachEndHandler,loop,StartTime,InEndTime);
        }
    }

    private bool isFullVPLength(double InLength){
        return videoPlayer.length == InLength;
    }


    /// <summary>
    /// 视频播放控制核心逻辑
    /// </summary>
    /// <param name="OnReachEndHandler"></param>
    /// <param name="loop"></param>
    /// <param name="StartTime"></param>
    /// <param name="InEndTime"></param>
    private void realPlay(VideoPlayer.EventHandler OnReachEndHandler=null, int loop=-1, float StartTime=0, float InEndTime=0)
    {
        Debug.LogFormat("UVP_{0} ========== request real play", UnityEngine.Time.time);
        double _originTime = UnityEngine.Time.time;
        if(!videoPlayer.isPrepared){
            Debug.LogWarning("UVP: video source is not prepared.");
            OnReachEndHandler(videoPlayer);
            return;
        }

        bool _looping = loop==-1?this.Looping:loop==1;
        Looping = _looping;

        double _startTime = Mathf.Max(StartTime,0);
        double _endTime = 0.0f;
        bool _bSeekCompleted = false;

        if(InEndTime>0){
            _endTime = Mathf.Min(InEndTime,(float)videoPlayer.length);
        }else{
            _endTime = videoPlayer.length;
        }
        //Debug.LogFormat("UVP:Play [{0} - {1}]",_startTime, _endTime);
        if(vpReachEnd!=null){
            videoPlayer.loopPointReached -= vpReachEnd;
            vpReachEnd = null;
        }

        if(vpLoopTimer!=null){
            vpLoopTimer.Dispose();
            vpLoopTimer = null;
        }

        vpReachEnd = _=>{
                _bSeekCompleted = false;
                if(OnReachEndHandler!=null)
                    OnReachEndHandler(videoPlayer);

                if(looping){
                    this.SeekTo(_startTime,_7=>{},_8=>{
                        _bSeekCompleted = true;
                    },true);
                }else{
                    if(videoPlayer.isPlaying){
                        videoPlayer.Pause();    
                    }
                    videoPlayer.loopPointReached -= vpReachEnd;
                    if(vpLoopTimer!=null){
                        vpLoopTimer.Dispose();
                        vpLoopTimer = null;
                    }
                }
        };

        vpLoopTimer = Observable.EveryUpdate().Where(_=>_bSeekCompleted).Subscribe(_1=>{
            if(videoPlayer.time>=_endTime){ // 经测试  触发onReachEndpoint时  videoPlayer.time 是小于 videoPlayer.length的
                vpReachEnd(videoPlayer);
            }
        });

        this.SeekTo(_startTime,_=>{},
            _2=>{
                _bSeekCompleted = true;
                videoPlayer.loopPointReached += vpReachEnd;
            },true);
    }
    
    private VideoPlayer.EventHandler vpPreapared = null;
    public void Prepare(VideoClip InClip, VideoPlayer.EventHandler OnPrepared=null){
        videoPlayer.clip = InClip;
        this.Prepare(_=>{
            this.SeekTo(0f,OnPrepared);
        });
    }

    public void Prepare(string InUrl, VideoPlayer.EventHandler OnPrepared=null){
        videoPlayer.url = InUrl;
        this.Prepare(_=>{
            this.SeekTo(0f, OnPrepared);
        });
    }
    public void Prepare(VideoPlayer.EventHandler OnPrepared=null)
    {
        if(videoPlayer==null){
            Debug.LogWarning("UVP: null reference of videoPlayer");
        }
        
        if(vpPreapared!=null){
            videoPlayer.prepareCompleted -= vpPreapared;
            vpPreapared = null;
        }

        vpPreapared = _=>{
            if(OnPrepared!=null)
                OnPrepared(videoPlayer);
            videoPlayer.prepareCompleted-=vpPreapared;
        };
        videoPlayer.prepareCompleted += vpPreapared;
        videoPlayer.Prepare();
    }
    private VideoPlayer.EventHandler vpSeekCompleted = null;
    private IDisposable vpSeekTimer = null;
    private bool timeGreaterThan(double InTime){
        return videoPlayer.time>=InTime;
    }
    public void SeekTo(double InTime, VideoPlayer.EventHandler InSeekedHandler=null, VideoPlayer.EventHandler InTimeReadyHandler=null, bool AutoPlay=false)
    {
        if(vpSeekCompleted!=null){
            videoPlayer.seekCompleted -= vpSeekCompleted;
            vpSeekCompleted = null;
        }

        if(vpSeekTimer!=null){
            vpSeekTimer.Dispose();
        }

        double _originDelta = videoPlayer.time - InTime;
        Func<long,bool> _condition = _2=>{
            var _curDelta = videoPlayer.time - InTime;
            bool _reachEnd = _curDelta>=0;
            return _reachEnd;
        };
        if(videoPlayer.time>InTime){
            _condition = _2=>{
                var _curDelta = videoPlayer.time - InTime;
                bool _reachEnd = _curDelta>=0&&_curDelta<_originDelta;
                return _reachEnd;
            };
        }

        double _originGameTime = UnityEngine.Time.time;

        Debug.LogFormat("UVP_{0}:========= start seek ",UnityEngine.Time.time, UnityEngine.Time.time - _originGameTime);
        vpSeekCompleted = _=>{
            Debug.LogFormat("UVP_{0}:=========Seek complted delta 1: {1}",UnityEngine.Time.time, UnityEngine.Time.time - _originGameTime);
            Debug.LogFormat("UVP_Current:{0}", videoPlayer.time);
            videoPlayer.seekCompleted -= vpSeekCompleted;
            vpSeekCompleted = null;
            
            videoPlayer.SetDirectAudioMute(0, false);
            if(InSeekedHandler != null){
                InSeekedHandler(videoPlayer);
            }

            Observable.EveryUpdate().Where(_condition).First().Subscribe(_11=>{
                Debug.LogFormat("UVP_{0}:=========Seek complted delta 2: {1}",UnityEngine.Time.time, UnityEngine.Time.time - _originGameTime);
                Debug.LogFormat("UVP_Current:{0}", videoPlayer.time);

                if(videoPlayer.isPlaying&&!AutoPlay){
                    videoPlayer.Pause();
                }
                if(InTimeReadyHandler!=null){
                    InTimeReadyHandler(videoPlayer);
                }
            });

        };

        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.seekCompleted += vpSeekCompleted;

        if(!videoPlayer.isPlaying){
            //Debug.Log("UVP: Play video by Play 3");
            videoPlayer.Play();
        }
        videoPlayer.time = InTime;
    }

    public void Play()
    {
        if(videoPlayer!=null){
            videoPlayer.Play();
        }
    }

    public void Stop(bool bFullyStop=false)
    {
        if(videoPlayer == null) return;

        if(bFullyStop){
            videoPlayer.Stop();
            videoPlayer.time = 0f;
        }else{
            SeekTo(0,_=>{});
        }
    }

    public void Pause()
    {
        if(videoPlayer!=null){
            videoPlayer.Pause();
        }
    }

    public void SetSpeed(float InSpeed)
    {
        videoPlayer.playbackSpeed = InSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void Reset() {
        if(videoImage is null) return;
        RectTransform _transform = this.transform as RectTransform;
        _transform.anchorMin = Vector2.zero;
        _transform.anchorMax = Vector2.one;
        _transform.offsetMin = Vector2.zero;
        _transform.offsetMax = Vector2.zero;
    }

    private void OnValidate() {
        //buildRefs();
        //syncRenderMode();
    }

    private void syncRenderMode(){
        if(renderMode == VideoRenderMode.MaterialOverride){
            Render2Material();
        }else if(renderMode == VideoRenderMode.RenderTexture){
            Render2Texture();
        }
    }


}



}

