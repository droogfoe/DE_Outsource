using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Sirenix.OdinInspector;

public class CurrentClipChecker : MonoBehaviour
{
    public PlayableDirector director;
    public Animator animator;
    public AnimationClip animationClip;
    public float fadeTime = 2;

    protected AnimatorOverrideController overrideController;
    private RuntimeAnimatorController oriRuntimeController;
    private AnimationPlayableOutput output;

    private void OnEnable()
    {
        FadeOutInit();
    }
    [Button]
    private void Test()
    {
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        oriRuntimeController = animator.runtimeAnimatorController;
        animator.runtimeAnimatorController = overrideController;
        overrideController["CursedDoll_Idle(2)"] = animationClip;
    }
    [Button]
    private void Return()
    {
        if (oriRuntimeController == null)
            return;

        animator.runtimeAnimatorController = oriRuntimeController;
    }
    [Button]
    private void DirectorFadeOut()
    {
        if (output.IsOutputValid())
        {
            StartCoroutine(FadeCoroutine());
        }
    }
    private IEnumerator FadeCoroutine()
    {
        float t = 0;
        while (t < fadeTime)
        {
            float weight = 1 - Mathf.Clamp01(t / fadeTime);
            output.SetWeight(weight);
            yield return null;
            t += Time.deltaTime;
        }
    }

    private void FadeOutInit()
    {
        if (!director.playableGraph.IsValid())
            return;
        var animationOutputs = director.playableGraph.GetOutputCountByType<AnimationPlayableOutput>();
        for (int i = 0; i < animationOutputs; i++)
        {
            var oldOutput = (AnimationPlayableOutput)director.playableGraph.GetOutputByType<AnimationPlayableOutput>(i);
            if (oldOutput.IsOutputValid() && oldOutput.GetTarget() != null)
            {
                // create a new output to replace the existing
                output = AnimationPlayableOutput.Create(director.playableGraph, "fake", oldOutput.GetTarget());
                output.SetSourcePlayable(oldOutput.GetSourcePlayable());
                output.SetSourceInputPort(oldOutput.GetSourceOutputPort());
                output.SetWeight(1.0f);
                oldOutput.SetTarget(null);
            }
        }
    }
}
