using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;
using ChangeTypeEnum = TLBindingContrlAsset.ChangeTypeEnum;

[Serializable]
public class UtageTLLineAsset : PlayableAsset, ITLBindingCallback
{
    public enum TLLineType
    {
        Line,
        Perform
    }
    [OnValueChanged("LineTypeValueChange")]
    [SerializeField] public TLLineType type;
    public enum EndEventType
    {
        Next,
        Selection,
        Jump,
        Block,
        EndTL
    }
    public EndEventType endEventType = EndEventType.Next;
    [ShowIf("endEventType", EndEventType.Jump)]
    [SerializeField] string jumpToSenario = "";
    public string JumpLabel  { get => jumpToSenario; }
    [ShowIf("type", TLLineType.Perform)]
    [SerializeField] public UtageTLLineAsset prePivotLine;
    [ShowIf("type", TLLineType.Perform)]
    [SerializeField] public UtageTLLineAsset posPivotLine;
    [ShowIf("type", TLLineType.Perform)]
    [SerializeField] public string pre;
    [ShowIf("type", TLLineType.Perform)]
    [SerializeField] public string pos;

    private void LineTypeValueChange()
    {
        if (type == TLLineType.Perform && needRegist)
        {
            clip.displayName = "NewPerformCam";
        }
    }
    private bool needRegist
    {
        get
        {
            if (type == TLLineType.Line)
                return false;

            if (template.lineData.BelongBlock == null
                || template.lineData.guid == "")
            {
                return true;
            }
            else
            {
                if (Guid.TryParse(template.lineData.guid, out Guid guid))
                {
                    return template.lineData.BelongBlock.GetLine(guid) == null;
                }
                else
                {
                    return true;
                }
            }
        }
    }
    public TimelineClip clip;
    public UtageTLLineBehaviour template = new UtageTLLineBehaviour();

    [HideInInspector] [SerializeField] TLClipUnityEvent onClipChangeEvent;
    private bool selecting = false;
    public bool Selecting { get => selecting; }

    [ReadOnly] public double Duration, StartPoint;

    public TLClipUnityEvent OnChangeEvent
    {
        get
        {
            if (onClipChangeEvent == null)
            {
                onClipChangeEvent = new TLClipUnityEvent();
            }
            return onClipChangeEvent;
        }
    }

    [SerializeField] List<TLBindingContrlAsset> bindings;
    public bool HasBindings 
    {
        get 
        {
            return (bindings != null && bindings.Count > 0);
        }
    }
    public bool IsInBinding(TrackAsset _track)
    {
        if (bindings == null && bindings.Count <= 0)
            return false;

        bool result = false;
        for (int i = 0; i < bindings.Count; i++)
        {
            if (bindings[i].IsTrackInBinding(_track))
                result = true;
        }
        return result;
    }

    List<TLBindingContrlAsset> ITLBindingCallback.Bindings => bindings;

    [ReadOnly][SerializeField]private ChangeTypeEnum changeType = ChangeTypeEnum.Empty;
    public ChangeTypeEnum ChangeType
    {
        get{ return changeType;}
        set
        {
            if (value != changeType) SaveBindOffset();
            changeType = value;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<UtageTLLineBehaviour>.Create(graph, template);
        if (template.lineData == null)
            Debug.Log("Create empty tl asset");
        else {
            if (type == TLLineType.Line)
                clip.displayName = template.CharactorID + " \n " + template.Line;
        }

        return playable;
    }
    private void OnDestroy()
    {
        //Debug.Log($"{GetType()} OnDestroy");
        template.lineData.SelfDestroy();
    }
    public string GetNameCombine()
    {
        return template.CharactorID + " \n " + template.Line;
    }
    [Button("Regist")][ShowIf("needRegist")]
    private void RegistToBlock()
    {
        var block = template.lineData.BelongBlock;
        if (block == null)
            return;

        Guid guid = Guid.NewGuid();
        template.lineData.guid = guid.ToString();
        template.lineData = block.InsertNewLine(template.lineData.index, clip.displayName, clip.displayName, guid);
        template.lineData.lineAsset = this;

        RegistPivotLine();
    }
    public void UpdateContent(TLCharacterLine _tlLine)
    {
        if (type != TLLineType.Line)
            return;

        template.lineData = _tlLine;
        template.lineData.characterId = _tlLine.characterId.ToString();
        template.lineData.line = _tlLine.line.ToString();

        clip.displayName = template.CharactorID + " \n " + template.Line;
    }

    public void OnClipChange(TimelineClip _clip, bool force = false)
    {
        StartPoint = _clip.start;
        if (Duration != _clip.duration)
        {
            ChangeType = ChangeTypeEnum.Scale;
            Duration = _clip.duration;
        }
        else
        {
            ChangeType = ChangeTypeEnum.Shift;
        }
        if (type == TLLineType.Perform)
            RegistPivotLine();

        if (!force)
        {
            if (!selecting || bindings == null || bindings.Count == 0)
                return;
        }

        if (bindings == null || bindings.Count == 0)
            return;

        for (int i = 0; i < bindings.Count; i++)
        {
            bindings[i].ClipInfoChange(_clip);
        }
    }
    [Button]
    private void RegistPivotLine()
    {
        if (template.lineData.BelongBlock == null)
            return;

        prePivotLine = template.lineData.BelongBlock.GetPreLine(template.lineData)?.lineAsset;
        pre = (prePivotLine != null) ? prePivotLine.template.lineData.line : "";

        posPivotLine = template.lineData.BelongBlock.GetNextLine(template.lineData)?.lineAsset;
        pos = (posPivotLine != null) ? posPivotLine.template.lineData.line : "";
    }

    public void SetMuteBinding(bool _flag)
    {
        if (bindings == null || bindings.Count == 0)
            return;

        for (int i = 0; i < bindings.Count; i++)
        {
            bindings[i].SetMuteBelong(_flag);
        }
    }

    [OnInspectorInit]
    public void OnInspectorInit()
    {
        selecting = true;
        BindingsBound();
        SaveBindOffset();
    }
    [OnInspectorDispose]

    private void OnInspectorDispose()
    {
        selecting = false;
        SaveBindOffset();
    }
    private void BindingsBound()
    {
        if (bindings == null || bindings.Count == 0)
            return;

        for (int i = 0; i < bindings.Count; i++){
            bindings[i].BoundEvent(true);
        }
    }
    public void BindingRegist(TLBindingContrlAsset _target)
    {
        if (bindings == null)
            bindings = new List<TLBindingContrlAsset>();

        if (bindings.Contains(_target))
            return;

        bindings.Add(_target);
        _target.SaveBindOffset(Duration, StartPoint);
    }
    public void SaveBindOffset()
    {
        if (bindings == null || bindings.Count == 0)
            return;

        for (int i = 0; i < bindings.Count; i++){
            bindings[i].SaveBindOffset(Duration, StartPoint);
        }
    }
}
