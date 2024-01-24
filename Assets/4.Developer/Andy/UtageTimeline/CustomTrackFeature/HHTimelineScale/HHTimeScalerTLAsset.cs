using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class HHTimeScalerTLAsset : PlayableAsset
{
    public HHTimeScalerTLBehaviour template = new HHTimeScalerTLBehaviour();
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<HHTimeScalerTLBehaviour>.Create(graph, template);
        owner.GetComponent<PlayableDirector>().stopped += ResetTimeScale;

        return playable;
    }
    private void ResetTimeScale(PlayableDirector _director)
    {
        Time.timeScale = 1;
        HHTimer.TimeSacle = 1;
    }
}
