using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class LightControlBehaviour : PlayableBehaviour
{
    public Color color = Color.white;
    public float intensity = 1f;
    [NonSerialized] public TimelineClip clipData;
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
    }
}
