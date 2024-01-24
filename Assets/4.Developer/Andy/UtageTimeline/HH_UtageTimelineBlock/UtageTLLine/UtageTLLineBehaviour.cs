using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;
using LineCallbackEvent = UtageCinemaBlock.LineCallbackEvent;

[Serializable]
public class UtageTLLineBehaviour : PlayableBehaviour
{
    [HideLabel][SerializeField] 
    public TLCharacterLine lineData;
    [HideInInspector] public string Line { get => lineData.line; set => lineData.line = value; }
    [HideInInspector] public string CharactorID { get => lineData.characterId; set => lineData.characterId = value; }
    [HideInInspector] public string GUID { get => lineData.guid; set => lineData.guid = value; }
    [SerializeField] bool speakAni = true;
    bool isPause, isStart;
    double clipDuration = -1;
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (lineData.BelongBlock == null)
            return;

        if (lineData != null)
            HHUtageTLStatic.TLLineOnStart(lineData);

        lineData.BelongBlock.TriggerLineEvent(LineCallbackEvent.Type.OnPlay, lineData.guid);

#if UNITY_EDITOR
        if (!isStart && !Application.isPlaying)
        {
            isStart = true;
            lineData.SoloBind(true);
            HHUtageTLStatic.RebuildGraph(lineData.BelongBlock, lineData.guid);
        }
#endif

        base.OnBehaviourPlay(playable, info);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!isPause)
        {
            var block = lineData.BelongBlock;
            if (clipDuration < 0 && block != null)
            {
                try
                {
                    var curT = block.Director.time;
                    clipDuration = block.GetClip(curT).duration;
                }
                catch (Exception)
                {
                    Debug.LogWarning("Block still not catch");
                }
            }

            var clipT = playable.GetTime();
            var interval = clipDuration - clipT;
            if (Math.Abs(interval) < 0.2d)
            {
                isPause = true;
                isStart = false;
                LineEndActionProcess();
            }
        }
        
        base.ProcessFrame(playable, info, playerData);
    }

    private void LineEndActionProcess()
    {
        if (lineData.lineAsset.type == UtageTLLineAsset.TLLineType.Line)
        {
            HHUtageTLStatic.Pause(lineData);
        }
        else if (lineData.lineAsset.type == UtageTLLineAsset.TLLineType.Perform)
        {
            lineData.BelongBlock.TriggerLineEvent(LineCallbackEvent.Type.OnEnd, lineData.guid);
            HHUtageTLStatic.CloseSkipper(lineData);
            if (lineData.lineAsset.endEventType == UtageTLLineAsset.EndEventType.EndTL)
            {
                HHUtageTLStatic.Pause(lineData);
                HHUtageTLStatic.CloseTLManager(lineData.BelongBlock);
            }
            else if (lineData.lineAsset.endEventType == UtageTLLineAsset.EndEventType.Block)
            {
                HHUtageTLStatic.Pause(lineData);
            }
        }
    }
}
