using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class InactiveTLAsset : PlayableAsset
{

    public InactiveTLBehaviour template = new InactiveTLBehaviour();
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<InactiveTLBehaviour>.Create(graph, template);

        return playable;
    }
}
