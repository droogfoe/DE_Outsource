using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.ComponentModel;

[TrackColor(0.3f, 0.5f, 0.7f)]
[TrackClipType(typeof(HHCharacterAnimationTLAsset))]
[TrackBindingType(typeof(UtageCharacter))]
[DisplayName("UtageTL/HHCharacterAnimtionTLTrack")]
[Serializable]
public class HHCharacterAnimtionTLTrack : TrackAsset
{
    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        var asset = clip.asset as HHCharacterAnimationTLAsset;
        asset.clip = clip;
    }
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        base.GatherProperties(director, driver);
        var clips = GetClips();
        foreach (var clip in clips)
        {
            var clipAsset = clip.asset as HHCharacterAnimationTLAsset;
            clipAsset.clip = clip;
            clip.displayName = clipAsset.DisplayName;
            clipAsset.director = director;
        }
    }
}
