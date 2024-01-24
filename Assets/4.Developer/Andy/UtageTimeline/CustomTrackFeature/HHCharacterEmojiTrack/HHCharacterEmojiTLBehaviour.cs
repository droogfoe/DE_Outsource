using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Emoji = UtageExtensions.UtageCharacterUtility.Emoji;

public class HHCharacterEmojiTLBehaviour : PlayableBehaviour
{
    HHCharacterEmojiTLAsset asset;
    [ReadOnly] [SerializeField] UtageCharacter character;
    public void SetAsset(HHCharacterEmojiTLAsset _asset)
    {
        asset = _asset;
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //base.ProcessFrame(playable, info, playerData);
        if (!Application.isPlaying) return;
        character = playerData as UtageCharacter;
        if (!asset.playing)
        {
            asset.playing = true;
            character.SetEmojiAction(asset.emoji.ToString(), asset.emoji_LifeTime);
        }
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        //base.OnBehaviourPause(playable, info);
        if (asset.playing && character != null)
        {
            asset.playing = false;
            character.HideEmoji();
        }  
    }
}
