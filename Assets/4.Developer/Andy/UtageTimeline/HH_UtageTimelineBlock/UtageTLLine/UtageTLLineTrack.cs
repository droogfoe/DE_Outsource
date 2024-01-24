using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.ComponentModel;

#if UNITY_EDITOR
using UnityEditor;
#endif

[TrackColor(0.92f, 0.78f, 0.0f)]
[TrackClipType(typeof(UtageTLLineAsset))]
[DisplayName("UtageTL/UtageTLLineTrack")]
public class UtageTLLineTrack : TrackAsset 
{
    protected override void OnCreateClip(TimelineClip clip)
    {
        UtageTLLineAsset lineAsset = clip.asset as UtageTLLineAsset;
        lineAsset.clip = clip;
        lineAsset.StartPoint = clip.start;
        lineAsset.Duration = clip.duration;

        base.OnCreateClip(clip);
    }
    protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
    {
        var lineAsset = clip.asset as UtageTLLineAsset;
        lineAsset.clip = clip;
        var block = gameObject.GetComponent<UtageCinemaBlock>();
        lineAsset.template.lineData.BelongBlock = block;
        lineAsset.template.lineData.blockName = gameObject.name;
        lineAsset.template.lineData.lineAsset = lineAsset;
        return base.CreatePlayable(graph, gameObject, clip);
    }
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        var clips = GetClips().ToArray();
        for (int i = 0; i < clips.Length; i++)
        {
            var lineAsset = clips[i].asset as UtageTLLineAsset;
            if (lineAsset.type == UtageTLLineAsset.TLLineType.Line)
            {
                clips[i].displayName = lineAsset.GetNameCombine();
            }
            //else
            //{
            //    if (i > 0)
            //    {
            //        lineAsset.prePivotLine = clips[i - 1].asset as UtageTLLineAsset;
            //        lineAsset.pre = lineAsset.prePivotLine.template.lineData.line;
            //    }
            //    if(i < clips.Length - 1)
            //    {
            //        lineAsset.posPivotLine = clips[i + 1].asset as UtageTLLineAsset;
            //        lineAsset.pos = lineAsset.posPivotLine.template.lineData.line;
            //    }
            //}

            if (lineAsset.template.lineData.BelongBlock != null)
                lineAsset.template.lineData.BelongBlock.SetLine(lineAsset.template.lineData);

            lineAsset.template.lineData.index = i;
            if (lineAsset.template.lineData.BelongBlock != null)
                lineAsset.template.lineData.BelongBlock.ReOrderListData();
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
        base.GatherProperties(director, driver);
    }
}
