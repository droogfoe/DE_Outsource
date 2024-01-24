using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.ComponentModel;

[TrackColor(0.4f, 0.265f, 0.6f)]
[TrackClipType(typeof(HHTimeScalerTLAsset))]
[DisplayName("UtageTL/HHTimeScalerTLTrack")]
public class HHTimeScalerTLTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<HHTimeScalerTLMixBehaviour>.Create(graph, inputCount);
    }
}
