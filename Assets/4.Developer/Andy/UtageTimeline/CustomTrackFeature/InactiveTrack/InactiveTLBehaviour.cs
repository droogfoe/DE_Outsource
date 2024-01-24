using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class InactiveTLBehaviour : PlayableBehaviour
{
    GameObject target = null;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        target = playerData as GameObject;
        if (target.activeSelf)
            target.SetActive(false);
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (target == null)
            return;

        target.SetActive(true);
    }
}
