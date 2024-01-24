using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class LightControlMixerBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Light trackBinding = playerData as Light;
        float finalIntensity = 0f;
        Color finalColor = Color.black;
        if (!trackBinding)
            return;

        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<LightControlBehaviour> inputPlayable = (ScriptPlayable<LightControlBehaviour>) playable.GetInput(i);
            LightControlBehaviour input = inputPlayable.GetBehaviour();

            finalIntensity += input.intensity * inputWeight;
            finalColor += input.color * inputWeight;
        }
        trackBinding.intensity = finalIntensity;
        trackBinding.color = finalColor;
    }
}
