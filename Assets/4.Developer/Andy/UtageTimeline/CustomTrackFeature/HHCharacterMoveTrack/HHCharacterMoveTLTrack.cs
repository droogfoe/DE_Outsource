using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.ComponentModel;

[TrackColor(0.7f, 0.55f, 0.3f)]
[TrackClipType(typeof(HHCharacterMoveTLAsset))]
[TrackBindingType(typeof(UtageCharactorMovementHandler))]
[DisplayName("UtageTL/HHCharacterMoveTrack")]
public class HHCharacterMoveTLTrack : TrackAsset
{
    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        var asset = clip.asset as HHCharacterMoveTLAsset;
        asset.clip = clip;
    }
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        base.GatherProperties(director, driver);
        var clips = GetClips();
        foreach (var clip in clips)
        {
            var clipAsset = clip.asset as HHCharacterMoveTLAsset;
            clipAsset.clip = clip;
            clip.displayName = clipAsset.DisplayName;
            clipAsset.director = director;
        }
    }
}
