using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using PostPlayBackState = HHActivationAsset.PostPlayBackState;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

[Serializable]
public class HHActivationBehaviour : PlayableBehaviour
{
    [SerializeField] HHActivationAsset asset;
    [SerializeField] bool isStateDetecting = false;
    GameObject target = null;

    private double clipStartTime;
#if UNITY_EDITOR
    EditorCoroutine coroutine;
#endif
    Playable detectPlayable;
    PlayableDirector director;
    
    public void SetAsset(HHActivationAsset _asset)
    {
        asset = _asset;
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        #region MyRegion
#if UNITY_EDITOR
        if (!isStateDetecting)
        {
            isStateDetecting = true;
            detectPlayable = playable;
            director = detectPlayable.GetGraph().GetResolver() as PlayableDirector;
            coroutine = EditorCoroutineUtility.StartCoroutine(StateDetectCoroutine(), this);
        }
#endif
        #endregion
        if (playerData != null)
        {
            target = playerData as GameObject;
            if (!target.activeSelf || !target.activeInHierarchy)
                target.SetActive(true);
        }
    }
#if UNITY_EDITOR
    IEnumerator StateDetectCoroutine()
    {
        while (isStateDetecting)
        {
            var playState = detectPlayable.GetPlayState();
            if (playState == PlayState.Paused)
            {
                if (director.time < asset.clip.start)
                {
                    isStateDetecting = false;
                    if (target != null)
                    {
                        target.SetActive(false);
                        break;
                    }
                }
                else if (director.time > asset.clip.end)
                {
                    isStateDetecting = false;
                    break;
                }
            }
            yield return null;
        }
        EditorCoroutineUtility.StopCoroutine(coroutine);
    }
#endif


    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (director == null)
            return;
        if (director.time < asset.clip.end && director.time > asset.clip.start)
            return;

        if (target != null)
        {
            switch (asset.PostState)
            {
                case PostPlayBackState.LeaveAsIs:
                    break;
                case PostPlayBackState.Inactive:
                    target.SetActive(false);
                    break;
                case PostPlayBackState.Active:
                    target.SetActive(true);
                    break;
                case PostPlayBackState.Revert:
                    target.SetActive(!target.activeSelf);
                    break;
                default:
                    break;
            }
        }
    }
}
