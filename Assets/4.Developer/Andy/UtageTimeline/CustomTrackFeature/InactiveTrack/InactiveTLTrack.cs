using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.ComponentModel;

[TrackColor(0.2f, 0.265f, 0.98f)]
[TrackClipType(typeof(InactiveTLAsset))]
[TrackBindingType(typeof(GameObject))]
[DisplayName("UtageTL/InactiveTLTrack")]
public class InactiveTLTrack : TrackAsset
{
    protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
    {
        var playable = ScriptPlayable<InactiveTLBehaviour>.Create(graph);
        clip.displayName = "Inactive";
        return playable;
    }
}
