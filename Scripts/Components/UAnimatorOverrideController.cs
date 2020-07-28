using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UAnimatorOverrideController : MonoBehaviour
{
    Animator animator;

    /**
     *    规则  必须 要和animationcontroller中要替换的动画名称匹配,且不能重名
     */
    public List<AnimationClip> animationClips = new List<AnimationClip>();

    void buildRefs(){
        if(!animator) animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        buildRefs();
        AnimatorOverrideController _animController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        var _animClips = new List<KeyValuePair<AnimationClip,AnimationClip>>();
        foreach(var _animClip in _animController.animationClips){
            var _newAnimClip = getAnimationClip(_animClip.name);
            if(_newAnimClip!=null){
                _animClips.Add(new KeyValuePair<AnimationClip, AnimationClip>(_animClip,_newAnimClip));
            }
        }
        _animController.ApplyOverrides(_animClips);
        animator.runtimeAnimatorController = _animController;

    }

    AnimationClip getAnimationClip(string InName){
        return animationClips.Where(_animationClip=>_animationClip&&_animationClip.name == InName).FirstOrDefault();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
