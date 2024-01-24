using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;

[Serializable]
public class HHCharacterAnimationTLBehaviour : PlayableBehaviour
{
    PlayableDirector director;
    HHCharacterAnimationTLAsset asset;
    [ReadOnly] [SerializeField] UtageCharacter character;
    bool trigger = false;
    public void SetAsset(PlayableDirector _director, HHCharacterAnimationTLAsset _asset)
    {
        director = _director;
        asset = _asset;
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);
        
        if (!Application.isPlaying) return;

        character = playerData as UtageCharacter;

        if (director == null)
            director = playable.GetGraph().GetResolver() as PlayableDirector;
        if (director.time < asset.clip.end && director.time > asset.clip.start)
        {
            if (!trigger)
            {
                trigger = true;
                if (asset.Handle_Type == HHCharacterAnimationTLAsset.Type.Reset)
                {
                    character.OverrideFace(false, asset.Transition);
                    character.ResetAnim(asset.Transition);
                }
                else
                {
                    if (asset.Command_Type == HHCharacterAnimationTLAsset.CommandType.Index)
                    {
                        character.OverrideFace(asset.OverrideFace, asset.Transition);
                        character.CrossIndexAnim(asset.Index, asset.AnimDuration, asset.FadeOutT, asset.Transition, asset.Layer);
                    }
                    else if (asset.Command_Type == HHCharacterAnimationTLAsset.CommandType.StateName)
                    {
                        character.OverrideFace(asset.OverrideFace, asset.Transition);
                        character.SetAnimAction(asset.State, asset.Flag, asset.AnimDuration, asset.FadeOutT, asset.Transition, asset.Layer);
                    }
                }
            }
        }
        else
        {
            if (trigger) 
            {
                trigger = false;
                if (asset.Handle_Type == HHCharacterAnimationTLAsset.Type.AutoRelease)
                {
                    character.OverrideFace(false, asset.Transition);
                    character.ResetAnim(asset.Transition);
                }
            } 
        }
    }
}
