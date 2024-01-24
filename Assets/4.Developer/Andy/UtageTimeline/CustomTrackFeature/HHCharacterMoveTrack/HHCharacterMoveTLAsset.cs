using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;
using MoveType = UtageCharacter.MoveType;

[Serializable]
public class HHCharacterMoveTLAsset : PlayableAsset
{
    public enum ActionType
    {
        Move,
        Turn
    }
    public ActionType actionType;
    [SerializeField] bool relocateStart = true, relocateEnd = true;
    [SerializeField] int assignIndex;
    [SerializeField] float angle;
    [SerializeField] MoveType moveType;
    public bool RelocateStart => relocateStart;
    public bool RelocateEnd => relocateEnd;
    public int Index => assignIndex;
    public float Angle => angle;
    public MoveType MvType => moveType;
    public HHCharacterMoveTLBehaviour behaviour;
    public TimelineClip clip;
    public PlayableDirector director;
    public string DisplayName 
    {
        get
        {
            if (actionType == ActionType.Move)
            {
                return actionType.ToString() + "_" + assignIndex;
            }
            else
            {
                return actionType.ToString() + "_" + angle;
            }
        }
    }
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<HHCharacterMoveTLBehaviour>.Create(graph);
        behaviour = playable.GetBehaviour();
        behaviour.SetAsset(director, this);

        return playable;
    }
}
