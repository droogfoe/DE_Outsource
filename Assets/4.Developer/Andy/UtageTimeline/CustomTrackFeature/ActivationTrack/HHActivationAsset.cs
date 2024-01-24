using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;

[Serializable]
public class HHActivationAsset : PlayableAsset
{
    [Serializable]
    public enum PostPlayBackState
    {
        LeaveAsIs,
        Inactive,
        Active,
        Revert
    }
    [SerializeField]
    public PostPlayBackState PostState;
    public TimelineClip clip;
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<HHActivationBehaviour>.Create(graph);
        playable.GetBehaviour().SetAsset(this);
        return playable;
    }
}
