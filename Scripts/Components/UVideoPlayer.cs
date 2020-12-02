using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UniRx;


[RequireComponent(typeof(VideoPlayer))]
public class UVideoPlayer : MonoBehaviour
{
    public VideoRenderMode renderMode = VideoRenderMode.RenderTexture;
    public bool Looping
    {
        set{
            if(videoPlayer!=null){
                videoPlayer.isLooping = value;
            }
        }
        get {
            if(videoPlayer!=null){
                return videoPlayer.isLooping;
            }
            return false;
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

    public void Render2Texture(RenderTexture InTexture = null)
    {
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        if(InTexture == null){
            var _renderer = this.GetComponent<RawImage>();
            if(_renderer==null){
                Debug.Log("There is no renderer component. from Render2Texture");
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

    public void Render2Material(Renderer InRenderer=null){
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        if(InRenderer==null){
            InRenderer = this.GetComponent<Renderer>();
            if(InRenderer==null){
                Debug.LogWarning("There is no renderer component. from Render2Material");
                return;
            }
        }
        videoPlayer.targetMaterialRenderer = InRenderer;
    }

    private void Awake() {
        buildRefs();
        syncRenderMode();

        videoPlayer.errorReceived+=(_vp,_error)=>{
            Debug.LogError(_error);
        };

        videoPlayer.started += _=>{
            Debug.Log("UVP: Video started");
        };

        videoPlayer.frameReady+= (_vp,_frame)=>{
            Debug.LogFormat("frame {0} ready...",_frame);
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
        videoPlayer.Pause();
        this.Play(OnReachEndHandler,loop,StartTime,InEndTime);   
    }

    public void Play(VideoClip InClip, VideoPlayer.EventHandler OnReachEndHandler=null, int loop=-1, float StartTime=0, float InEndTime=0){
        videoPlayer.clip = InClip;
        Play(OnReachEndHandler,loop,StartTime,InEndTime);
    }

    public void Prepare(VideoClip InClip, VideoPlayer.EventHandler OnPrepared=null){
        videoPlayer.clip = InClip;
        this.Prepare(_=>{
            this.SeekTo(0f,OnPrepared);
        });
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

    private void realPlay(VideoPlayer.EventHandler OnReachEndHandler=null, int loop=-1, float StartTime=0, float InEndTime=0)
    {
        if(!videoPlayer.isPrepared){
            Debug.LogWarning("video source is not prepared.");
            OnReachEndHandler(videoPlayer);
            return;
        }

        bool _looping = loop==-1?this.Looping:loop==1;
        Looping = _looping;

        double _startTime = Mathf.Max(StartTime,0);
        double _endTime = 0.0f;

        bool _bSeekCompleted = false;

        if(InEndTime>0){
            _endTime = Mathf.Min(InEndTime,(float)videoPlayer.length) - 0.2f;
        }else{
            _endTime = videoPlayer.length;
        }
        
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
                if(_looping){
                    this.SeekTo(_startTime,_3=>{
                        videoPlayer.Play();
                        _bSeekCompleted = true;
                    });
                }else{
                    videoPlayer.Pause();
                    vpLoopTimer.Dispose();
                    videoPlayer.loopPointReached -= vpReachEnd;
                    vpLoopTimer = null;
                }
                if(OnReachEndHandler!=null)
                    OnReachEndHandler(videoPlayer);
        };

        vpLoopTimer = Observable.EveryFixedUpdate().Where(_=>_bSeekCompleted).Subscribe(_1=>{
            if(videoPlayer.time<_startTime){
                _bSeekCompleted = false;
                this.SeekTo(_startTime,_4=>{
                    _bSeekCompleted = true;
                });
                return;
            }
            
            if(videoPlayer.time>=_endTime){ // 视频时间到达真正的视频结尾时, 该条件并不满足
                vpReachEnd(videoPlayer);
            }
        });
        //Debug.LogFormat("Prepared:{0}, Paused:{1}, Playing:{2} ",videoPlayer.isPrepared,videoPlayer.isPaused,videoPlayer.isPlaying);

        this.SeekTo(_startTime,_=>{
            _bSeekCompleted = true;
            videoPlayer.loopPointReached += vpReachEnd;
            Debug.Log("seek completed. begin play.");
            videoPlayer.Play();
        });
    }
    
    private VideoPlayer.EventHandler vpPreapared = null;
    public void Prepare(VideoPlayer.EventHandler OnPrepared=null)
    {
        if(videoPlayer==null){
            Debug.LogWarning("null reference of videoPlayer");
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
    public void SeekTo(double InTime,VideoPlayer.EventHandler InSeekedHandler=null)
    {
        if(vpSeekCompleted!=null){
            videoPlayer.seekCompleted -= vpSeekCompleted;
            vpSeekCompleted = null;
        }

        if(vpSeekTimer!=null){
            vpSeekTimer.Dispose();
        }

        vpSeekCompleted = _=>{
            videoPlayer.seekCompleted -= vpSeekCompleted;
            vpSeekCompleted = null;
            Func<long,bool> _condition = _2=>{
                return Mathf.Abs((float)videoPlayer.time-(float)InTime)<0.02f;
            };
            vpSeekTimer = Observable.EveryFixedUpdate().Where(_condition).Subscribe(_2=>{
                videoPlayer.SetDirectAudioMute(0, false);
                videoPlayer.Pause();
                vpSeekTimer.Dispose();
                vpSeekTimer = null;   
                Debug.LogFormat("seek to {0} completed", InTime) ;
                if(InSeekedHandler != null)
                    InSeekedHandler(videoPlayer);
            });
        };

        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.seekCompleted += vpSeekCompleted;
        videoPlayer.time = InTime;

        if(!videoPlayer.isPlaying){
            videoPlayer.Play();
        }
        
    }

    public void Play()
    {
        if(videoPlayer!=null){
            videoPlayer.Play();
        }
    }

    public void Stop()
    {
        if(videoPlayer!=null){
            videoPlayer.Stop();
            videoPlayer.time = 0f;
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
        buildRefs();
        syncRenderMode();
    }

    private void syncRenderMode(){
        if(renderMode == VideoRenderMode.MaterialOverride){
            Render2Material();
        }else if(renderMode == VideoRenderMode.RenderTexture){
            Render2Texture();
        }
    }


}
