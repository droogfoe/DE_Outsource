using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.ComponentModel;

[TrackColor(0.0f, 0.8f, 0.0f)]
[TrackClipType(typeof(HHActivationAsset))]
[TrackBindingType(typeof(GameObject))]
[DisplayName("UtageTL/HHActivationTrack")]
public class HHActivationTrack : TrackAsset
{
    protected override void OnCreateClip(TimelineClip clip)
    {
        var asset = clip.asset as HHActivationAsset;
        asset.clip = clip;
        base.OnCreateClip(clip);
    }
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        var clips = GetClips();
        foreach (var clip in clips)
        {
            var clipAsset = clip.asset as HHActivationAsset;
            clipAsset.clip = clip;
            clip.displayName = $"<{clipAsset.PostState.ToString()}>";
        }
        base.GatherProperties(director, driver);
    }
}
