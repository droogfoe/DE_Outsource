using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Sirenix.OdinInspector;

[Serializable]
public class TLBindingContrlAsset : PlayableAsset
{
    public TLBindingBehaviour template = new TLBindingBehaviour();
    public TimelineClip clip;
    public Action<TLBindingContrlAsset> onDestroy;

    [Space(10)]

    [ReadOnly][SerializeField] bool isMute = false;
    [FoldoutGroup("Binding Setting")]
    [HideInInspector]public PlayableAsset playableAsset;

    [FoldoutGroup("Binding Setting")][HideLabel]
    [ReadOnly][SerializeField] BindingData bindingData;
    [FoldoutGroup("Binding Setting")][HideIf("bindingDicEmpty")]
    [HideInInspector][SerializeField] BindingDictionary bindingDic;

    public enum ChangeTypeEnum
    {
        Empty,
        Scale,
        Shift
    }
    ChangeTypeEnum changeType = ChangeTypeEnum.Empty;
    public ChangeTypeEnum ChangeType
    {
        get { return changeType; }
        set 
        {
            if (value != changeType) ChildGroupBind();
            changeType = value;
        }
    }
    private bool hasBounded = false;

    private bool bindingAssigned { get => bindingData.Asset != null;}
    private bool bindingDicEmpty{ get { return (bindingDic == null || bindingDic.Count == 0); }}
    [HideInInspector][SerializeField] private double startOffset, scalerBaseValue, oriBindDuration;
    [HideInInspector] public double Duration, StartPoint, BindDuration, BindStart;
    private bool selecting = false;
    public bool Selecting { get => selecting; }

    [OnInspectorInit]
    private void OnInspectorInit()
    {
        selecting = true;
        Import();
        BindableLines();
        BoundEvent();
        ChildGroupBind();
    }
    [OnInspectorDispose]
    private void OnInspectorDispose()
    {
        selecting = false;
        ChildGroupBind();
    }
    private void BindableLines()
    {
        if (playableAsset == null)
            return;
        
        var tlAsset = playableAsset as TimelineAsset;
        var tracks = tlAsset.GetOutputTracks().ToArray();
        UtageTLLineTrack lineTrack = null;
        for (int i = 0; i < tracks.Length; i++)
        {
            lineTrack = tracks[i] as UtageTLLineTrack;
            if (lineTrack != null)
                break;
        }

        if (lineTrack == null || !lineTrack.hasClips)
            return;

        var clips = lineTrack.GetClips().ToArray();
        BINDABLELINES = new List<string>();
        for (int i = 0; i < clips.Length; i++)
        {
            var line = clips[i].asset as UtageTLLineAsset;
            BINDABLELINES.Add(line.template.Line);
        }
    }
    public void SaveBindOffset(double _bindDur, double _bindStart)
    {
        if (_bindDur != BindDuration || _bindStart != BindStart)
        {
            clip.start = _bindStart + startOffset;
        }
        BindDuration = _bindDur;
        BindStart = _bindStart;
        startOffset = clip.start - BindStart;
        oriBindDuration = BindDuration;
        scalerBaseValue = clip.duration / BindDuration;

        ChildGroupBind();
    }
    //public void RegionScalerUpdate(TimelineClip _clip)
    //{
    //    var _bindDuration = _clip.duration;
    //    clip.duration = _bindDuration * scalerBaseValue;
    //    clip.start = _clip.start + startOffset * _bindDuration / oriBindDuration;
    //}
    //public void RegionClapUpdate() 
    //{
    //    if (!selecting || IsBindingClipEmpty)
    //        return;

    //    if (clip.start < BindStart)
    //    {
    //        clip.start = BindStart;
    //    }
    //    if (clip.end > BindStart + BindDuration)
    //    {
    //        double newDuration = BindStart + BindDuration - clip.start;
    //        clip.duration = newDuration;
    //    }
    //}
    private bool IsBindingClipEmpty
    {
        get
        {
            return (bindingData == null || bindingData.Clip == null || bindingData.Clip.asset == null);
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var bindAsset = clip.asset as TLBindingContrlAsset;
        var playable = ScriptPlayable<TLBindingBehaviour>.Create(graph, template);
        return playable;
    }
    private void OnDestroy()
    {
        onDestroy?.Invoke(this);

        if (!IsBindingClipEmpty)
        {
            var asset = bindingData.Clip.asset;
            var _interface = (ITLBindingCallback)asset;
            if (_interface.Bindings != null && _interface.Bindings.Contains(this))
            {
                _interface.Bindings.Remove(this);
            }
        }
    }
    public void BoundEvent(bool _force = false) 
    {
        if (hasBounded || !_force)
            return;

        if (!IsBindingClipEmpty)
        {
            var asset = bindingData.Clip.asset;
            var tlAsset = asset as UtageTLLineAsset;
            tlAsset.BindingRegist(this);

            Debug.Log("BoundEvent");
            hasBounded = true;
        }
    }
    //[Button][FoldoutGroup("Binding Setting")]
    private void Import()
    {
        var tlAsset = playableAsset as TimelineAsset;
        var blockContrl = BindTrackByIndex(0, tlAsset);
        bindingDic = new BindingDictionary();
        var clips = blockContrl.GetClips().ToArray();

        for (int i = 0; i < clips.Length; i++)
        {
            var tLAssewt = clips[i].asset as PlayableAsset;
            string _name = clips[i].displayName;
            _name = _name.Replace("\n", " -> ");
            bindingDic.Add(i, new BindingData(tLAssewt, clips[i])) ;
        }
    }

    public static List<string> BINDABLELINES = new List<string>();
    [HideIf("bindingAssigned")]
    [FoldoutGroup("Binding Setting")]
    [ListToPopup(typeof(TLBindingContrlAsset), "BINDABLELINES")]
    public string SelectLine;

    [Button][HideIf("bindingAssigned")]
    [FoldoutGroup("Binding Setting")]
    private void AssignBinding()
    {
        hasBounded = false;

        if (bindingDic == null || bindingDic.Count == 0)
            Import();

        int _index = BINDABLELINES.IndexOf(SelectLine);

        if (bindingData.Asset != null)
        {
            var bindAsset = bindingData.Clip.asset;
            var bindInterface = (ITLBindingCallback)bindAsset;
            if (bindInterface.Bindings.Contains(this))
                bindInterface.Bindings.Remove(this);
        }

        if (bindingDicEmpty || bindingDic.Count <= _index)
            return;

        bindingData = new BindingData(bindingDic.ElementAtOrDefault(_index).Value);
        var asset = bindingDic.ElementAtOrDefault(_index).Value.Clip.asset;
        var tlAsset = asset as UtageTLLineAsset;
        tlAsset.BindingRegist(this);
        tlAsset.SaveBindOffset();

        SetDisplayNameCreateBindingGroup();

        bindingDic = new BindingDictionary();
    }
    public bool IsTrackInBinding(TrackAsset _track)
    {
        if (dutyGroupTrack == null) 
        {
            Debug.LogError(dutyGroupTrack.name);
            Debug.LogError(bindingDic == null);
            Debug.LogError(bindingDic.Count);
            Debug.LogError("Got return");
            return false;
        }

        bool result = false;
        var parentTrackAsset = _track.parent as TrackAsset;

        if (_track.parent == dutyGroupTrack)
        {
            result = true;
        }
        if(parentTrackAsset != null 
            && parentTrackAsset.parent == dutyGroupTrack)
        {
            result = true;
        }
        #region MyRegion
        //var childTracks = dutyGroupTrack.GetChildTracks().ToArray();
        //for (int i = 0; i < childTracks.Length; i++)
        //{
        //    var childTrack = childTracks[i];
        //    if (childTrack.GetType() == typeof(GroupTrack))
        //    {
        //        var childGroupTrack = childTrack as GroupTrack;
        //        var grandChildTracks = childGroupTrack.GetChildTracks().ToArray();
        //        for (int j = 0; j < grandChildTracks.Length; j++)
        //        {
        //            if (grandChildTracks[j] == _track
        //                || grandChildTracks[j].GetInstanceID() == _track.GetInstanceID())
        //            {
        //                result = true;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (childTrack == _track || childTrack.GetInstanceID() == _track.GetInstanceID())
        //        {
        //            result = true;
        //        }
        //    }
        //}
        #endregion

        return result;
    }
    private void SetDisplayNameCreateBindingGroup()
    {
        if (dutyGroupTrack != null)
        {
            ChildGroupBind();
            return;
        }

        var tlAsset= playableAsset as TimelineAsset;
        var lineAsset = bindingData.Asset as UtageTLLineAsset;
        clip.displayName = "<Binding>" + "\n" + lineAsset.template.Line;

        var searchGroupTrack = FindTrackByName(clip.displayName, tlAsset);
        if (searchGroupTrack != null)
        {
            dutyGroupTrack = searchGroupTrack as GroupTrack;
            return;
        }

        dutyGroupTrack = tlAsset.CreateTrack(typeof(GroupTrack), null, "<Binding> " + lineAsset.template.Line) as GroupTrack;
        ChildGroupBind();
    }
    [FoldoutGroup("ChildBinding")]
    [ReadOnly] [SerializeField] GroupTrack dutyGroupTrack;
    [FoldoutGroup("ChildBinding")]
    [ReadOnly] [SerializeField] List<EditableClipInfo> childClips;
    private void ChildGroupBind()
    {
        if (dutyGroupTrack == null)
            return;

        var childTracks = dutyGroupTrack.GetChildTracks().ToArray();
        if (childTracks == null || childTracks.Length == 0)
            return;

        childClips = new List<EditableClipInfo>();
        for (int i = 0; i < childTracks.Length; i++)
        {
            var childTrack = childTracks[i];
            if (childTrack.GetType() == typeof(GroupTrack))
            {
                var nestChildTracks = childTrack.GetChildTracks();
                foreach (var nestTrack in nestChildTracks)
                {
                    if (nestTrack.hasClips)
                    {
                        var nestClips = nestTrack.GetClips();
                        foreach (var nestClip in nestClips)
                        {
                            EditableClipInfo nestClipInfo = new EditableClipInfo(nestClip, clip);
                            childClips.Add(nestClipInfo);
                        }
                    }
                }
            }
            else
            {
                if (!childTrack.hasClips)
                    continue;
                var clips = childTrack.GetClips();
                foreach (var _clip in clips)
                {
                    EditableClipInfo clipInfo = new EditableClipInfo(_clip, clip);
                    childClips.Add(clipInfo);
                }
            }
        }
    }
    [FoldoutGroup("ChildBinding")]
    //[Button("UnlockBelong")]
    public void UnlockBelong()
    {
        if (dutyGroupTrack == null)
            return;

        var childTracks = dutyGroupTrack.GetChildTracks().ToArray();
        if (childTracks == null || childTracks.Length == 0)
            return;

        for (int i = 0; i < childTracks.Length; i++)
        {
            childTracks[i].locked = true;
        }
    }
    public void SetMuteBelong(bool _flag)
    {
        isMute = _flag;
        if (dutyGroupTrack == null)
            return;

        var childTracks = dutyGroupTrack.GetChildTracks().ToArray();
        if (childTracks == null || childTracks.Length == 0)
            return;

        dutyGroupTrack.muted = _flag;
        for (int i = 0; i < childTracks.Length; i++)
        {
            childTracks[i].muted = _flag;
        }
    }
    public void ChildClipsInfoChange(TimelineClip _targetClip, bool _force = false)
    {
        if (!selecting && !_force)
            return;
        if (childClips == null || childClips.Count == 0)
            return;

        StartPoint = _targetClip.start;
        for (int i = 0; i < childClips.Count; i++)
        {
            childClips[i].Shifting(playableAsset, _targetClip);
        }
    }
    IEnumerator EndFrameAction(Action _action)
    {
        yield return null;
        _action.Invoke();
    }
    public void ClipInfoChange(TimelineClip _targetClip)
    {
        if (clip == null)
            return;

        clip.start = _targetClip.start + startOffset;
        ChildClipsInfoChange(clip, true);
    }
    private TrackAsset BindTrackByIndex(int _index, TimelineAsset _tlAsset)
    {
        var tracks = _tlAsset.GetOutputTracks().ToArray();
        if (tracks.Length > _index)
        {
            return tracks[_index];
        }
        return null;
    }
    private TrackAsset FindTrackByName(string _name, TimelineAsset _tlAsset)
    {
        if (_name == "" || _tlAsset == null)
            return null;

        var tracks = _tlAsset.GetRootTracks().ToArray();
        for (int i = 0; i < tracks.Length; i++)
        {
            if (_name == tracks[i].name)
            {
                return tracks[i];
            }
        }
        return null;
    }

    [Serializable]
    public class BindingDictionary : UnitySerializedDictionary<int, BindingData> {}
    [Serializable]
    public class BindingData
    {
        [HideInInspector] [SerializeField] TimelineClip m_clip;
        [SerializeField] PlayableAsset m_asset;
        public TimelineClip Clip { get { return m_clip; } }
        public PlayableAsset Asset { get { return m_asset; } }
        public BindingData(BindingData _data)
        {
            m_clip = _data.m_clip;
            m_asset = _data.m_asset;
        }
        public BindingData(PlayableAsset _asset, TimelineClip _clip)
        {
            m_clip = _clip;
            m_asset = _asset;
        }
    }
    [Serializable]
    public class EditableClipInfo
    {
        public TimelineClip clip;
        public double startOffset;
        public double scalerBaseValue;
        public double oriBindDuration;

        public EditableClipInfo(TimelineClip _clip, TimelineClip _bind) 
        {
            SetInfo(_clip, _bind);
        }

        public void SetInfo(TimelineClip _clip, TimelineClip _bind)
        {
            this.clip = _clip;
            this.startOffset = _clip.start - _bind.start;
            this.oriBindDuration = _bind.duration;
            this.scalerBaseValue = _clip.duration / _bind.duration;
        }
        public void Shifting(PlayableAsset _pAsset, TimelineClip _clip)
        {
            var tlAsset = _pAsset as TimelineAsset;
            var tracks = tlAsset.GetOutputTracks().ToArray();
            for (int i = 0; i < tracks.Length; i++)
            {
                if (!tracks[i].hasClips)
                    continue;

                var trackClips = tracks[i].GetClips();
                foreach (var trackClip in trackClips)
                {
                    if (trackClip.asset == this.clip.asset){
                        trackClip.start = _clip.start + this.startOffset;
                    }
                }
            }
            //this.clip.start = _clip.start + this.startOffset;
        }
    }
}
