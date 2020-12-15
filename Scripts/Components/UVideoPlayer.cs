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

    private void Awake() {
        buildRefs();
        syncRenderMode();

        videoPlayer.errorReceived+=(_vp,_error)=>{
            Debug.LogError(_error);
        };

        videoPlayer.started+=(_)=>{
            Debug.LogFormat("UVP_{0} ============== Started: {1}", UnityEngine.Time.time, videoPlayer.time);
        };

        // videoPlayer.frameReady+= (_vp,_frame)=>{
        //     Debug.LogFormat("frame {0} ready...",_frame);
        // };
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
        Debug.LogFormat("UVP_{0} ========== request Play by url: {1}", UnityEngine.Time.time, videoPlayer.time);
        videoPlayer.url = InUrl;
        this.Play(OnReachEndHandler,loop,StartTime,InEndTime);   
    }

    public void Play(VideoClip InClip, VideoPlayer.EventHandler OnReachEndHandler=null, int loop=-1, float StartTime=0, float InEndTime=0){
        Debug.LogFormat("UVP_{0} ========== request Play by clip: {1}", UnityEngine.Time.time, videoPlayer.time);
        videoPlayer.clip = InClip;
        Play(OnReachEndHandler,loop,StartTime,InEndTime);
    }

    public void Prepare(VideoClip InClip, VideoPlayer.EventHandler OnPrepared=null){
        Debug.LogFormat("UVP_{0} ========== request prepare clip: {1}", UnityEngine.Time.time, videoPlayer.time);
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
            _endTime = Mathf.Min(InEndTime,(float)videoPlayer.length) - 0.05f;
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
                Debug.LogFormat("到达视频结尾:{0}",Time);
                _bSeekCompleted = false;
                if(_looping){
                    this.SeekTo(_startTime,_3=>{
                        if(!videoPlayer.isPlaying){
                            //Debug.Log("UVP: Play video by Play 1");
                            videoPlayer.Play();
                        }
                        _bSeekCompleted = true;
                    });
                }else{
                    //Debug.LogFormat("UVP: from start {0} reach end point:{1}, stopped", _startTime, _endTime);
                    if(videoPlayer.isPlaying){
                        //Debug.Log("UVP: Pause video P1");
                        videoPlayer.Pause();    
                    }
                    vpLoopTimer.Dispose();
                    videoPlayer.loopPointReached -= vpReachEnd;
                    vpLoopTimer = null;
                }
                if(OnReachEndHandler!=null)
                    OnReachEndHandler(videoPlayer);
        };

        vpLoopTimer = Observable.EveryUpdate().Where(_=>_bSeekCompleted).Subscribe(_1=>{
            if(videoPlayer.time<_startTime){
                _bSeekCompleted = false;
                //Debug.Log("UVP: Seek to startTime P2");
                this.SeekTo(_startTime,_4=>{
                    _bSeekCompleted = true;
                    if(!videoPlayer.isPlaying){
                        //Debug.Log("UVP: Play video by Play 4");
                        videoPlayer.Play();
                    }
                });
                return;
            }
            
            if(videoPlayer.time>=_endTime){ // 视频时间到达真正的视频结尾时, 该条件并不满足
                vpReachEnd(videoPlayer);
            }
        });
        //Debug.LogFormat("Prepared:{0}, Paused:{1}, Playing:{2} ",videoPlayer.isPrepared,videoPlayer.isPaused,videoPlayer.isPlaying);

        //Debug.Log("UVP: Seek to startTime P1");
        this.SeekTo(_startTime,_=>{
            _bSeekCompleted = true;
            videoPlayer.loopPointReached += vpReachEnd;        
            Debug.Log("UVP: Play video by seek completed");
            videoPlayer.Play();
        });
    }
    
    private VideoPlayer.EventHandler vpPreapared = null;
    public void Prepare(VideoPlayer.EventHandler OnPrepared=null)
    {
        Debug.LogFormat("UVP_{0} ==========  request prepare", UnityEngine.Time.time, videoPlayer.time);
        if(videoPlayer==null){
            Debug.LogWarning("UVP: null reference of videoPlayer");
        }
        
        if(vpPreapared!=null){
            videoPlayer.prepareCompleted -= vpPreapared;
            vpPreapared = null;
        }

        vpPreapared = _=>{
            Debug.LogFormat("UVP_{0} ==========  prepare  completed: {1}", UnityEngine.Time.time, videoPlayer.time);
            if(OnPrepared!=null)
                OnPrepared(videoPlayer);
            videoPlayer.prepareCompleted-=vpPreapared;
        };
        videoPlayer.prepareCompleted += vpPreapared;
        videoPlayer.Prepare();
    }
    private VideoPlayer.EventHandler vpSeekCompleted = null;
    private IDisposable vpSeekTimer = null;
    public void SeekTo(double InTime, VideoPlayer.EventHandler InSeekedHandler=null)
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
            videoPlayer.seekCompleted -= vpSeekCompleted;
            vpSeekCompleted = null;
            vpSeekTimer = Observable.EveryUpdate().Where(_condition).Subscribe(_2=>{
                videoPlayer.SetDirectAudioMute(0, false);
                if(videoPlayer.isPlaying){
                    //Debug.Log("UVP: Pause video by Seek()");
                    videoPlayer.Pause();
                }
                vpSeekTimer.Dispose();
                vpSeekTimer = null;   
                Debug.LogFormat("UVP_{0}:=========Seek complted delta 2: {1}",UnityEngine.Time.time, UnityEngine.Time.time - _originGameTime);
                if(InSeekedHandler != null){
                   // Debug.Log("UVP: callback seek completed.");
                    InSeekedHandler(videoPlayer);
                }
            });
        };

        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.seekCompleted += vpSeekCompleted;
        videoPlayer.time = InTime;

        if(!videoPlayer.isPlaying){
            //Debug.Log("UVP: Play video by Play 3");
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
