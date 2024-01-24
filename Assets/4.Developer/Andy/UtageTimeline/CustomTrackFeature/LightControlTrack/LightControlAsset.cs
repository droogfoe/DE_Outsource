using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class LightControlAsset : PlayableAsset
{
    [NonSerialized] public TimelineClip clipPassThrough = null;
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }
    public LightControlBehaviour template = new LightControlBehaviour();

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        template.clipData = clipPassThrough;
        var playable = ScriptPlayable<LightControlBehaviour>.Create(graph, template);
        return playable;
    }
}
