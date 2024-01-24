using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(UtageTLLineAsset))]
public class UtageTLLineAssetEditor : ClipEditor
{
    public override void OnClipChanged(TimelineClip clip)
    {
        var asset = clip.asset as UtageTLLineAsset;
        asset.OnClipChange(clip);
        base.OnClipChanged(clip);
    }
}
