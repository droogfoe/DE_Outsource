using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;

[Serializable]
public class HHCharacterMoveTLBehaviour : PlayableBehaviour
{
    [ReadOnly] [SerializeField] UtageCharactorMovementHandler moveHandler;

    bool trigger;
    [SerializeField] PlayableDirector director;
    [SerializeField] HHCharacterMoveTLAsset asset;
    Vector3 oriRot;
    public void SetAsset(PlayableDirector _director, HHCharacterMoveTLAsset _asset)
    {
        director = _director;
        asset = _asset;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying)
            return;

        if (director == null)
            director = playable.GetGraph<Playable>().GetResolver() as PlayableDirector;

        moveHandler = playerData as UtageCharactorMovementHandler;
        if (!trigger && director.state == PlayState.Playing)
        {
            trigger = true;
            if (asset.actionType == HHCharacterMoveTLAsset.ActionType.Move)
            {
                moveHandler.SetMoveResetPos(asset.Index, asset.MvType, asset.RelocateStart);
            }
            else if (asset.actionType == HHCharacterMoveTLAsset.ActionType.Turn)
            {
                moveHandler.TurnRotate(asset.Angle);
            }
        }
    }
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!Application.isPlaying)
            return;

        base.OnBehaviourPlay(playable, info);
        if (moveHandler == null)
            return;

        if (trigger && director.state == PlayState.Playing)
        {
            moveHandler.SetMove(asset.Index, asset.MvType, asset.RelocateStart);
        }
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying)
            return;

        if (trigger)
        {
            if (director.time >= asset.clip.end || director.time < asset.clip.start)
            {
                trigger = false;
                if (moveHandler != null)
                    moveHandler.StopMove(asset.RelocateEnd);
            }
            else if(director.time < asset.clip.end && director.time > asset.clip.start)
            {
                if (moveHandler != null)
                    moveHandler.StopMove(asset.RelocateEnd);
            }
        }
    }
}
