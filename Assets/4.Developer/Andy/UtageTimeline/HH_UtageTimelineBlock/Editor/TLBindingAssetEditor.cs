using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[CustomTimelineEditor(typeof(TLBindingContrlAsset))]
public class TLBindingAssetEditor : ClipEditor
{
    public override void OnClipChanged(TimelineClip clip)
    {
        base.OnClipChanged(clip);
        var asset = clip.asset as TLBindingContrlAsset;
        //asset.RegionClapUpdate();
        asset.ChildClipsInfoChange(clip);
    }
}
