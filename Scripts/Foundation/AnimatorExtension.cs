using System;
using UnityEngine;

namespace UHelper
{

public static class AnimatorExtension
{
    public static void PlayAnimation(this Animator animator, string InState){
        animator.Play(InState);
        syncPlayState(animator);
    }

    public static void StopAnimation(this Animator animator){
        syncStopState(animator);
    }

    // private methods
    private static void syncPlayState(Animator animator){
        animator.SetBool("Stop",false);
    }

    private static void syncStopState(Animator animator){
        animator.SetBool("Stop",true);
    }

}

}