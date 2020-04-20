﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UniRx;

[RequireComponent(typeof(VideoPlayer),typeof(RawImage))]
public class UVideoPlayer : MonoBehaviour
{
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
    private void Awake() {
        rectTransform = this.transform as RectTransform;
        RenderTexture _videoRT = new RenderTexture((int)rectTransform.rect.width,(int)rectTransform.rect.height,0,RenderTextureFormat.ARGB32);
        
        videoPlayer = this.GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = _videoRT;

        videoImage = this.GetComponent<RawImage>();
        videoImage.texture = _videoRT;
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
        }

        if(vpLoopTimer!=null){
            vpLoopTimer.Dispose();
            vpLoopTimer = null;
        }

        vpReachEnd = _=>{
                Debug.Log("tigger video end point:" + videoPlayer.time);
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
                OnReachEndHandler(videoPlayer);
        };

        vpLoopTimer = Observable.EveryUpdate().Where(_=>_bSeekCompleted).Subscribe(_1=>{
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


        this.SeekTo(_startTime,_=>{
            _bSeekCompleted = true;
            videoPlayer.loopPointReached += vpReachEnd;
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
        }

        if(vpSeekTimer!=null){
            vpSeekTimer.Dispose();
        }

        vpSeekCompleted = _=>{
            videoPlayer.seekCompleted -= vpSeekCompleted;
            vpSeekCompleted = null;
            Func<long,bool> _condition = _2=>{
                return Mathf.Abs((float)videoPlayer.time-(float)InTime)<0.2f;
            };
            vpSeekTimer = Observable.EveryUpdate().Where(_condition).Subscribe(_2=>{
                Debug.Log("seek completed "+ videoPlayer.time);
                videoPlayer.SetDirectAudioMute(0, false);
                videoPlayer.Pause();
                InSeekedHandler(videoPlayer);
                vpSeekTimer.Dispose();
                vpSeekTimer = null;    
            });
        };

        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.seekCompleted += vpSeekCompleted;
        videoPlayer.time = InTime;
        if(!videoPlayer.isPlaying){
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

    private void Reset() {
        RectTransform _transform = this.transform as RectTransform;
        _transform.anchorMin = Vector2.zero;
        _transform.anchorMax = Vector2.one;
        _transform.offsetMin = Vector2.zero;
        _transform.offsetMax = Vector2.zero;
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