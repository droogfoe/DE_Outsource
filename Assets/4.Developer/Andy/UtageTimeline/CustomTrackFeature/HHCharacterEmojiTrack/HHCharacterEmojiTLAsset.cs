using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Emoji = UtageExtensions.UtageCharacterUtility.Emoji;

public class HHCharacterEmojiTLAsset : PlayableAsset
{
    public Emoji emoji;
    public float emoji_LifeTime = 2.0f;
    public TimelineClip clip;
    public bool playing;
    public HHCharacterEmojiTLBehaviour behaviour;
    public string DisplayName
    {
        get
        {
            return emoji.ToString();
        }
    }
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<HHCharacterEmojiTLBehaviour>.Create(graph);
        behaviour = playable.GetBehaviour();
        behaviour.SetAsset(this);
        return playable;
    }
}
