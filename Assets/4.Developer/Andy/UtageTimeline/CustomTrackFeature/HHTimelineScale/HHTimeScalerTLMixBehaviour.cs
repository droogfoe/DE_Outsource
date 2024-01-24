using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class HHTimeScalerTLMixBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;

        float finalScaleValue = 0f;
        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<HHTimeScalerTLBehaviour> inputPlayable = (ScriptPlayable<HHTimeScalerTLBehaviour>)playable.GetInput(i);
            HHTimeScalerTLBehaviour input = inputPlayable.GetBehaviour();
            finalScaleValue += input.ScaleValue * inputWeight;

            Time.timeScale = finalScaleValue;
            HHTimer.TimeSacle = finalScaleValue;
        }

    }
}
