using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.ComponentModel;

[TrackColor(0.81f, 0.42f, 0.45f)]
[DisplayName("UtageTL/UtageTLBlockControlTrack")]
public class UtageTLBlockControlTrack : ControlTrack
{
    protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
    {
        return base.CreatePlayable(graph, gameObject, clip);
    }
}
