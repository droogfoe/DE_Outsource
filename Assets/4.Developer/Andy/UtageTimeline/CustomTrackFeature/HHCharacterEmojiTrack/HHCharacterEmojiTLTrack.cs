using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.ComponentModel;

[TrackClipType(typeof(HHCharacterEmojiTLAsset))]
[TrackBindingType(typeof(UtageCharacter))]
[DisplayName("UtageTL/HHCharacterEmojiTLTrack")]
[Serializable]
public class HHCharacterEmojiTLTrack : TrackAsset
{
    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        var asset = clip.asset as HHCharacterEmojiTLAsset;
        asset.clip = clip;
    }
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        base.GatherProperties(director, driver);
        var clips = GetClips();
        foreach (var clip in clips)
        {
            var clipAsset = clip.asset as HHCharacterEmojiTLAsset;
            clipAsset.clip = clip;
            clip.displayName = clipAsset.DisplayName;
        }
    }
}
