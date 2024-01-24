using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.ComponentModel;

[TrackColor(0.67f, 0.97f, 1)]
[ExcludeFromPreset]
[TrackClipType(typeof(TLBindingContrlAsset))]
[DisplayName("UtageTL/TLBindingTrack")]
public class TLBindingTrack : TrackAsset
{
    protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
    {
        var _playableAsset = gameObject.GetComponent<PlayableDirector>()?.playableAsset;

        var tlAsset = clip.asset as TLBindingContrlAsset;
        tlAsset.clip = clip;
        if (_playableAsset != null)
            tlAsset.playableAsset = _playableAsset;

        return base.CreatePlayable(graph, gameObject, clip);
    }
}
