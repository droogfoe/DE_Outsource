using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackClipType(typeof(LightControlAsset))]
[TrackBindingType(typeof(Light))]
public class LightControlTrack : TrackAsset 
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var clips = GetClips();
        foreach (var clip in clips)
        {
            var clipAsset = clip.asset as LightControlAsset;
            clipAsset.clipPassThrough = clip;
        }
        return ScriptPlayable<LightControlMixerBehaviour>.Create(graph, inputCount);
    }
    protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
    {
        var playable = ScriptPlayable<LightControlMixerBehaviour>.Create(graph);

        var clips = GetClips();
        foreach (var _clip in clips)
        {
            var clipAsset = clip.asset as LightControlAsset;
            clipAsset.clipPassThrough = _clip;
        }
        return base.CreatePlayable(graph, gameObject, clip);
    }
}