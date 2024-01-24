using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;

[Serializable]
public class HHCharacterAnimationTLAsset : PlayableAsset
{
    public enum CommandType
    {
        Index,
        StateName
    }
    public enum Type
    {
        AutoRelease,
        ManualRelease,
        Reset
    }
    public CommandType commandType;
    [OnValueChanged("HandleTypeChange")]
    public Type handleType;
    public CommandType Command_Type => commandType;
    public Type Handle_Type => handleType;
    [HideIf("handleType", Type.Reset)]
    [SerializeField] int layer = 0;
    [ShowIf("commandType", CommandType.Index)]
    [SerializeField] int index = 0;
    [SerializeField] float transition = 0.1f;
    [ShowIf("commandType", CommandType.StateName)]
    [SerializeField] string stateName;
    public string State => stateName;
    [SerializeField] float animDuration = 1.0f;
    [SerializeField] bool overrideFace = false;
    public bool OverrideFace => overrideFace;
    public float AnimDuration => animDuration;
    private bool showFlag => handleType == Type.ManualRelease & commandType == CommandType.StateName;
    [ShowIf("showFlag")]
    [SerializeField] bool flag;
    public bool Flag => flag;
    public float Transition => transition;
    public int Index => index;
    public int Layer => layer;
    [ShowIf("handleType", Type.AutoRelease)]
    [SerializeField] float fadeOutT = 0.1f;
    public float FadeOutT => fadeOutT;
    public string DisplayName
    {
        get
        {
            if (commandType == CommandType.Index)
            {
                switch (handleType)
                {
                    case Type.AutoRelease:
                        return $"Layer{layer} / Action{index}_Auto";
                    case Type.ManualRelease:
                        return $"Layer{layer} / Action{index}_Manual";
                    case Type.Reset:
                        return $"Reset";
                    default:
                        return $"default";
                }
            }
            else
            {
                switch (handleType)
                {
                    case Type.AutoRelease:
                        return $"Layer{layer} / {State}_Auto";
                    case Type.ManualRelease:
                        return $"Layer{layer} / {State}_Manual";
                    case Type.Reset:
                        return $"Reset";
                    default:
                        return $"default";
                }
            }
        }
    }
    public HHCharacterAnimationTLBehaviour behaviour;
    public TimelineClip clip;
    public PlayableDirector director;
    private void HandleTypeChange()
    {
        if (handleType == Type.ManualRelease)
        {
            animDuration = -1.0f;
        }
        else
        {
            animDuration = 1.0f;
        }
    }
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<HHCharacterAnimationTLBehaviour>.Create(graph);
        behaviour = playable.GetBehaviour();
        behaviour.SetAsset(director, this);
        return playable;
    }
}
