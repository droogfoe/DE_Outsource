using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif
using Sirenix.OdinInspector;

public class UtageCinemaBlock : MonoBehaviour
{
    public UtageTLManager utageTL;
    public PlayableAsset BlockClipAsset;
    [SerializeField]
    private PlayableDirector director;
    public PlayableDirector Director { get => director; }
    /*[ReadOnly] */[SerializeField]
    private string senario;
    public string Senario { get => senario; set => senario = value; }
    public enum RunningMode
    {
        Editor,
        Runtime
    }
    //[OnValueChanged("")]
    public RunningMode previewMode = RunningMode.Editor;
    public enum MergeTrackMode
    {
        Auto,
        Manual
    }
    public enum MergeStateEnum
    {
        Mergeing,
        DisMerge
    }
    public MergeTrackMode mergeMode = MergeTrackMode.Auto;
    [SerializeField] MergeStateEnum mergeState = MergeStateEnum.DisMerge;
    public MergeStateEnum MergeState { get =>  mergeState; }
    [SerializeField] List<TLCharacterLine> charactorLines;
    [SerializeField] List<LineCallbackEvent> linesCallBackList = new List<LineCallbackEvent>();
    [SerializeField] MergeTrackDic bindTrackDic = new MergeTrackDic();
    [System.Serializable]
    public class MergeTrackDic : UnitySerializedDictionary<int, TrackPairList> {}


    [System.Serializable]
    public class LineCallbackEvent
    {
        public string line;
        [HideInInspector]
        public string guid;
        [HideInInspector]
        public int order;
        [FoldoutGroup("UnityEvent", false)]
        public UnityEvent OnPlayHandler;
        [FoldoutGroup("UnityEvent", false)]
        public UnityEvent OnEndHandler;
        public enum Type 
        {
            OnPlay,
            OnEnd
        }
    }

    [Serializable]
    public class TrackPairList
    {
        public List<TrackPair> PairList;
        public TrackPairList()
        {
            PairList = new List<TrackPair>();
        }
    }
    [Serializable]
    public class TrackPair
    {
        public TimelineClip clip;
        public TrackAsset newTrack;
        public TrackAsset originTrack;
        public TrackPair(TrackAsset _ori, TrackAsset _new, TimelineClip _clip)
        {
            originTrack = _ori;
            newTrack = _new;
            clip = _clip;
        }
    }
    [ShowIf("mergeMode", MergeTrackMode.Manual)]

    [Button("MergeTrack")]
    public void RuntimeBuildMergeTrack()
    {
        if (mergeState == MergeStateEnum.Mergeing)
            return;

        var camPitcher = GetComponent<CinemachineTrackGetCamera>();

        camPitcher.Pitch();
        previewMode = RunningMode.Runtime;

        bindTrackDic = new MergeTrackDic();
        var tlAsset = (TimelineAsset)director.playableAsset;
        foreach (var track in tlAsset.GetOutputTracks())
        {

            bool inBinding = false;
            for (int i = 0; i < charactorLines.Count; i++)
            {
                if (charactorLines[i].lineAsset.IsInBinding(track))
                    inBinding = true;
            }

            if (inBinding)
            {
                Debug.Log("inBinding/ " + track.name);
                CreateMergeTrack<CinemachineTrack>(track);
                CreateMergeTrack<AnimationTrack>(track);
                CreateMergeTrack<ActivationTrack>(track);
                CreateMergeTrack<AudioTrack>(track);
                CreateMergeTrack<HHActivationTrack>(track);
            }
        }
        camPitcher.Pitch();
        mergeState = MergeStateEnum.Mergeing;
    }
    [ShowIf("mergeMode", MergeTrackMode.Manual)]
    [Button("DisMerge")]
    public void DisbatchMergeTrack()
    {
        if (mergeState == MergeStateEnum.DisMerge)
            return;

        previewMode = RunningMode.Editor;
        foreach (var binds in bindTrackDic)
        {
            var clips = binds.Value.PairList.FirstOrDefault().newTrack.GetClips().ToArray();
            for (int i = 0; i < clips.Length; i++)
            {
                var pairData = binds.Value.PairList
                    .Where(x => x.clip == clips[i] || x.clip.asset.GetInstanceID() == clips[i].asset.GetInstanceID())
                    .Select(z => z).FirstOrDefault();
                TimelineClipExtensions.MoveToTrack(clips[i], pairData.originTrack);
            }
        }
        foreach (var binds in bindTrackDic)
        {
            var track = binds.Value.PairList.FirstOrDefault().newTrack;
            var tlAsset = (TimelineAsset)director.playableAsset;
            tlAsset.DeleteTrack(track);
        }
        mergeState = MergeStateEnum.DisMerge;
    }
    private void CreateMergeTrack<T>(TrackAsset _track) where T: TrackAsset, new()
    {
        if (_track.GetType() != typeof(T))
            return;

        var aniTrack = _track as T;
        var target = director.GetGenericBinding(aniTrack);
        var tlAsset = (TimelineAsset)director.playableAsset;
        if (!aniTrack.hasClips)
            return;

        TrackAsset newTrack = null;
        if (!bindTrackDic.ContainsKey(target.GetInstanceID()))
        {
            newTrack = tlAsset.CreateTrack<T> ();
            director.SetGenericBinding(newTrack, target);
            var newList = new TrackPairList();
            bindTrackDic.Add(target.GetInstanceID(), newList);
        }
        else
        {
            newTrack = bindTrackDic[target.GetInstanceID()].PairList.FirstOrDefault().newTrack;
        }

        var oriClips = aniTrack.GetClips().ToArray();
        for (int i = 0; i < oriClips.Length; i++)
        {
            bindTrackDic[target.GetInstanceID()].PairList.Add(new TrackPair(_track, newTrack, oriClips[i]));
            TimelineClipExtensions.MoveToTrack(oriClips[i], newTrack);
        }
    }
    //[Button("ActorLoop")]
    //public void SetActorLoopAnim()
    //{
    //    var tlAsset = (TimelineAsset)director.playableAsset;
    //    var tracks = tlAsset.GetOutputTracks();
    //    foreach (var track in tracks)
    //    {
    //        if (!track.hasClips || track.GetType() != typeof(AnimationTrack))
    //            continue;

    //        var clips = track.GetClips().ToArray();
    //        for (int i = 0; i < clips.Length; i++)
    //        {
    //            var t = director.time;
    //            if (t >= clips[i].start && t < clips[i].end)
    //            {
    //                var target = director.GetGenericBinding(track);
    //                var aniClip = clips[i].asset as AnimationPlayableAsset;
    //                Debug.Log(target.name + " / " + aniClip.clip.name);
    //            }
    //        }
    //    }
    //}
    [OnInspectorInit]
    private void ReOrderAtInspectorInit()
    {
        ReOrderListData();
    }
    [Button]
    private void LinesCallbackSetting()
    {
        if (charactorLines == null && charactorLines.Count == 0)
            return;

        if (linesCallBackList == null)
            linesCallBackList = new List<LineCallbackEvent>();

        foreach (var item in charactorLines
            .Where(x => !linesCallBackList.Any(z => z.guid == x.guid)))
        {
            var @new = new LineCallbackEvent();
            @new.order = item.index;
            @new.guid = item.guid;
            @new.line = item.line;
            linesCallBackList.Add(@new);
        }
        foreach (var item in 
            linesCallBackList.Where(x => !charactorLines.Any(z => z.guid == x.guid)).ToList())
        {
            linesCallBackList.Remove(item);
        }
    }
    public void RebuildAtEditorFrame()
    {
        if (director == null || Application.isPlaying)
            return;

#if UNITY_EDITOR
        EditorCoroutineUtility.StartCoroutine(RebuildCoroutine(), this);
#endif
    }
    IEnumerator RebuildCoroutine()
    {
        yield return null;
        var t = director.time;
        director.RebuildGraph();
        director.time = t;
        //director.Play();
    }

    public void ReOrderListData()
    {
        if (charactorLines == null || charactorLines.Count == 0)
            return;

        var redorderList = charactorLines.OrderBy(x => x.index).ToList();
        charactorLines = redorderList;

        //if (linesCallBackList == null || linesCallBackList.Count == 0)
        //    return;

        //foreach (var item in linesCallBackList)
        //{
        //    var select = charactorLines.Where(x => x.guid == item.guid).FirstOrDefault();
        //    if (select != null)
        //        item.order = select.index;
        //}
        //var reorderEventList = linesCallBackList.OrderBy(x => x.order).ToList();
        //linesCallBackList = reorderEventList;
    }

    //[Button("SetBelongBlock")]
    public void RegistBelongBlockToLine()
    {
        if (charactorLines != null && charactorLines.Count != 0)
        {
            for (int i = 0; i < charactorLines.Count; i++)
            {
                var tlClip = GetClip(charactorLines[i].guid);
                var asset = tlClip.asset as UtageTLLineAsset;
                asset.template.lineData.BelongBlock = this;
            }
        }
    }
    public void Regist(string _charactorId, string _line, Guid _guid, int _index)
    {
        if (charactorLines == null)
            charactorLines = new List<TLCharacterLine>();

        var lineInfo = new TLCharacterLine(this, _charactorId, _line, _guid, _index);
        charactorLines.Add(lineInfo);
    }
    public void Sort()
    {
        if (charactorLines == null || charactorLines.Count == 0)
            return;

        var redorderList = charactorLines.OrderBy(x => x.index).ToList();
        charactorLines = redorderList;

        if (charactorLines.Any(x => x.lineAsset.type == UtageTLLineAsset.TLLineType.Perform))
        {
            for (int i = 0; i < charactorLines.Count; i++)
            {
                if (charactorLines[i].lineAsset.type != UtageTLLineAsset.TLLineType.Perform)
                    continue;

               
                if (charactorLines[i].lineAsset.prePivotLine != null)
                {
                    var preLine = charactorLines[i].lineAsset.prePivotLine;
                    var index = charactorLines.IndexOf(preLine.template.lineData);
                    var pLine = charactorLines[i];
                    charactorLines.RemoveAt(i);
                    charactorLines.Insert(index + 1, pLine);
                }
                else if (charactorLines[i].lineAsset.posPivotLine != null)
                {
                    var posLine = charactorLines[i].lineAsset.posPivotLine;
                    var index = charactorLines.IndexOf(posLine.template.lineData);
                    var pLine = charactorLines[i];
                    charactorLines.RemoveAt(i);
                    charactorLines.Insert(index, pLine);
                }
                else
                {
                    var pLine = charactorLines[i];
                    charactorLines.RemoveAt(i);
                    charactorLines.Insert(charactorLines.Count - 1, pLine);
                }
            }
            for (int i = 0; i < charactorLines.Count; i++)
            {
                charactorLines[i].index = i;
            }

            redorderList = charactorLines.OrderBy(x => x.index).ToList();
            charactorLines = redorderList;
        }

        for (int i = 0; i < charactorLines.Count; i++)
        {
            var line = charactorLines[i];
            Debug.Log(i + " " + line.line);
            var currentClip = GetClip(line.guid);
            if (i == 0)
            {
                currentClip.start = 0;
            }
            else
            {
                var preLine = charactorLines[i - 1];
                var preClip = GetClip(preLine.guid);
                currentClip.start = preClip.end;
            }
            var tlAsset = currentClip.asset as UtageTLLineAsset;
            tlAsset.OnClipChange(currentClip, true);
        }
        LinesCallbackSetting();
    }
    public void RemoveUnUseByExistDatas(string[] _guids)
    {
        var tlAsset = (TimelineAsset)director.playableAsset;
        var tracks = tlAsset.GetRootTracks();
        var guidList = _guids.ToList();
        var performLines = charactorLines.Where(x => x.lineAsset.type == UtageTLLineAsset.TLLineType.Perform).Select(z => z.guid);
        guidList.AddRange(performLines);

#if UNITY_EDITOR
        EditorCoroutineUtility.StartCoroutine(RemoveUnUseByExistDatas_Coroutine(tracks, guidList.ToArray()), this);
#endif
    }
    private IEnumerator RemoveUnUseByExistDatas_Coroutine(IEnumerable<TrackAsset> _tracks, string[] _guids)
    {
        List<int> removeIndexs = new List<int>();
        for (int i = 0; i < charactorLines.Count; i++)
        {
            bool found = false;
            for (int j = 0; j < _guids.Length; j++)
            {
                if (charactorLines[i].guid == _guids[j])
                    found = true;
            }
            if (found == false)
            {
                var line = charactorLines[i];
                removeIndexs.Add(i);
                foreach (var track in _tracks)
                {
                    if ((track is UtageTLLineTrack))
                    {
                        var tLtrack = track as UtageTLLineTrack;
                        var clips = tLtrack.GetClips().ToArray();
                        TimelineClip destroyTarget = null;
                        for (int j = 0; j < clips.Length; j++)
                        {
                            var lineAsset = clips[j].asset as UtageTLLineAsset;
                            if (lineAsset.template.GUID == line.guid)
                            {
                                destroyTarget = clips[j];
                                break;
                            }
                        }
                        if (destroyTarget != null)
                        {
                            tLtrack.DeleteClip(destroyTarget);
                            yield return null;
                            break;
                        }
                    }
                }
                charactorLines.Remove(line);
                
            }
        }
        yield return new WaitUntil(() => charactorLines.Count == _guids.Length);

        Sort();
    }
    public TLCharacterLine GetLine(Guid _guid)
    {
        return charactorLines.Where(x => x.guid == _guid.ToString()).FirstOrDefault();
    }
    public void SetLine(TLCharacterLine _line)
    {
        if (charactorLines.Contains(_line))
            return;

        if (Guid.TryParse(_line.guid, out Guid _guid))
        {
            var line = GetLine(_guid);
            var index = charactorLines.IndexOf(line);
            charactorLines[index] = _line;
        }
    }
    public TLCharacterLine GetLine(double _time)
    {
        try
        {
            var asset = GetClip(_time).asset as UtageTLLineAsset;
            Guid guid = Guid.Parse(asset.template.lineData.guid);
            return GetLine(guid);
        }
        catch (Exception)
        {
            Debug.LogWarning($"{GetType()} get line empty");
            return null;
        }

    }
    public TimelineClip GetClip(double _time)
    {
        var tlAsset = (TimelineAsset)director.playableAsset;
        var tracks = tlAsset.GetRootTracks();
        UtageTLLineTrack dutyTrack = null;
        foreach (var track in tracks)
        {
            if (track.GetType() == typeof(UtageTLLineTrack))
                dutyTrack = track as UtageTLLineTrack;
        }
        if (!dutyTrack.hasClips)
            return null;

        TimelineClip[] clips = dutyTrack.GetClips().ToArray();
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].start <= _time && clips[i].end > _time)
            {
                return clips[i];
            }
        }
        return null;
    }
    public TimelineClip GetNextClip(double _time, out int _index)
    {
        var tlAsset = (TimelineAsset)director.playableAsset;
        var tracks = tlAsset.GetRootTracks();
        UtageTLLineTrack dutyTrack = null;
        foreach (var track in tracks)
        {
            if (track.GetType() == typeof(UtageTLLineTrack))
                dutyTrack = track as UtageTLLineTrack;
        }
        if (!dutyTrack.hasClips)
        {
            _index = -1;
            return null;
        }

        TimelineClip[] clips = dutyTrack.GetClips().ToArray();
        for (int i = 0; i < clips.Length; i++)
        {
            if (i < clips.Length - 1
                && clips[i].start <= _time && clips[i].end > _time)
            {
                _index = i + 1;
                return clips[i + 1];
            }
        }
        _index = -1;
        return null;
    }
    public TLCharacterLine GetNextLine(double _time)
    {
        var clip = GetNextClip(_time, out int index);
        if (clip == null)
        return null;

        var lineAsset = clip.asset as UtageTLLineAsset;
        return lineAsset.template.lineData;
    }
    public TLCharacterLine GetNextLine(TLCharacterLine _line)
    {
        try
        {
            var lines = charactorLines;
            ReOrderListData();
            Guid _guid = Guid.Parse(_line.guid);
            var curLine = GetLine(_guid);
            var curIndex = lines.IndexOf(curLine);
            if (curLine != charactorLines.LastOrDefault())
                return charactorLines[curIndex + 1];
            else
                return null;
        }
        catch (Exception)
        {
            Debug.LogWarning("Line still not have guid.");
            return null;
        }
       
    }
    public TLCharacterLine GetPreLine(TLCharacterLine _line)
    {
        try
        {
            var lines = charactorLines;
            ReOrderListData();
            Guid _guid = Guid.Parse(_line.guid);
            var curLine = GetLine(_guid);
            var curIndex = lines.IndexOf(curLine);
            if (curLine != charactorLines.FirstOrDefault())
                return charactorLines[curIndex - 1];
            else
                return null;
        }
        catch (Exception)
        {
            Debug.LogWarning("Line still not have guid.");
            return null;
        }
       
    }
    public TLCharacterLine[] GetLines()
    {
        return charactorLines.ToArray();
    }
    public int[] GetPerformIndexs()
    {
        var picks = charactorLines.Where(x => x.lineAsset.type == UtageTLLineAsset.TLLineType.Perform).Select(z => z.index).ToArray();
        int[] answer = new int[picks.Length];
        for (int i = 0; i < answer.Length; i++)
        {
            answer[i] = picks[i];
        }
        return answer;
    }
    public TLCharacterLine InsertNewLine(int _index, string _character, string _line, Guid _guid)
    {
        var _tlLine = new TLCharacterLine(this, _character, _line, _guid, _index);
        charactorLines.Insert(_index, _tlLine);
        LinesCallbackSetting();
        return _tlLine;
    }

    public void InsertNewClip(int _index, TLCharacterLine _tlLine)
    {
        var tlAsset = (TimelineAsset)director.playableAsset;
        var tracks = tlAsset.GetRootTracks();

        foreach (var track in tracks)
        {
            if (!(track is UtageTLLineTrack))
                continue;

            var newClip = track.CreateClip<UtageTLLineAsset>();
            var clips = track.GetClips().ToArray();
            double startTLPoint = 0, tempDuration = 0;

            if (_index <= clips.Length - 1)
            {
                startTLPoint = clips[_index].start;
                tempDuration = clips[_index].duration;
                for (int i = _index; i < charactorLines.Count; i++)
                {
                    clips[i].start += clips[i].duration;
                }
            }
            else
            {
                startTLPoint = clips[clips.Length - 1].start;
                tempDuration = clips[clips.Length - 1].duration;
            }

            newClip.start = startTLPoint;
            newClip.duration = tempDuration;
            var lineAsset = newClip.asset as UtageTLLineAsset;
            lineAsset.template.lineData = _tlLine;
            newClip.displayName = lineAsset.template.CharactorID + "\n" + lineAsset.template.Line;
            _tlLine.lineAsset = lineAsset;
        }
    }

    public void CreateData()
    {
        var tlAsset = (TimelineAsset)director.playableAsset;
        var tracks = tlAsset.GetRootTracks();

        foreach (var track in tracks){
            if (!(track is UtageTLLineTrack))
                continue;
            
            for (int i = 0; i < charactorLines.Count; i++)
            {
                var clip = track.CreateDefaultClip();
                var lineAsset = clip.asset as UtageTLLineAsset;
                lineAsset.template.lineData = charactorLines[i];
                clip.displayName = lineAsset.template.CharactorID + "\n" + lineAsset.template.Line;
                charactorLines[i].lineAsset = lineAsset;
            }
        }
    }
    public void BindingLocker(bool _flag)
    {
        var tlAsset = (TimelineAsset)director.playableAsset;
        var tracks = tlAsset.GetRootTracks();
        foreach (var track in tracks)
        {
            if (track.GetType() != typeof(UtageTLLineTrack))
                track.locked = _flag;
        }
    }
    private TimelineClip GetClip(string _guid)
    {
        var tlAsset = (TimelineAsset)director.playableAsset;
        var tracks = tlAsset.GetRootTracks();
        UtageTLLineTrack targetTrack = null;
        foreach (var track in tracks)
        {
            if (!(track is UtageTLLineTrack))
                continue;

            targetTrack = track as UtageTLLineTrack;
            break;
        }
        if (!targetTrack.hasClips)
            return null;

        var clips = targetTrack.GetClips();
        foreach (var clip in clips)
        {
            var _asset = clip.asset as UtageTLLineAsset;

            if (_asset.template.GUID == _guid)
               return clip;
        }
        return null;
    }
    public void TLLineBeenDestroyed(Guid _guid)
    {
        var line = GetLine(_guid);
        if (line == null)
            return;

        for (int i = 0; i < charactorLines.Count; i++)
        {
            if (charactorLines[i].guid == line.guid)
            {
                Debug.Log("TLLineBeenDestroyed: " + line.lineAsset.clip.displayName + " / " + line.guid);
                charactorLines.RemoveAt(i);
            }
        }
        LinesCallbackSetting();
    }
    public void SoloFromLineGroup(bool _flag, string _guid)
    {
        if (charactorLines == null || charactorLines.Count == 0)
            return;

        var guidLine = charactorLines.Where(x => x.guid == _guid).Select(z => z).FirstOrDefault();

        if (guidLine == null || !guidLine.lineAsset.HasBindings)
            return;

        for (int i = 0; i < charactorLines.Count; i++)
        {
            var line = charactorLines[i];
            if (line.guid != _guid)
            {
                line.lineAsset.SetMuteBinding(_flag);
            }
            else
            {
                if (previewMode == RunningMode.Runtime)
                    line.lineAsset.SetMuteBinding(true);
                else
                    line.lineAsset.SetMuteBinding(false);
            }
        }
    }
    public void TriggerLineEvent(LineCallbackEvent.Type _type, string _guid)
    {
        var eventPack = linesCallBackList.Where(x => x.guid == _guid).FirstOrDefault();
        
        if (_type == LineCallbackEvent.Type.OnPlay)
        {
            if (eventPack != null)
                eventPack.OnPlayHandler?.Invoke();
        }
        else if (_type == LineCallbackEvent.Type.OnEnd)
        {
            if (eventPack != null)
                eventPack.OnEndHandler?.Invoke();
        }
    }
    public void DebugTest(string _debug)
    {
        Debug.Log("debug test: " + _debug);
    }

    [System.Serializable]
    public class LineGUIDDic : UnitySerializedDictionary<string, string> { }
}

[System.Serializable]
public class TLCharacterLine
{
    public string characterId;
    public string line;
    public string guid;
    public int index;
    public string blockName;
    [SerializeField] UtageCinemaBlock belongBlock;
    public UtageCinemaBlock BelongBlock 
    {
        get
        {
            if (belongBlock == null)
                FindBlockFromScene();
            return belongBlock;
        }
        set
        {
            belongBlock = value;
        }
    }
    [ReadOnly] public UtageTLLineAsset lineAsset;

    public TLCharacterLine() { }
    public TLCharacterLine(UtageCinemaBlock _block, string _characterId, string _line, Guid _guid, int _index)
    {
        this.belongBlock = _block;
        this.blockName = _block.gameObject.name;
        this.characterId = _characterId;
        this.line = _line;
        this.guid = _guid.ToString();
        this.index = _index;
    }
    public void Update(string _line)
    {
        this.line = _line;
        lineAsset.UpdateContent(this);
        if (Guid.TryParse(guid, out Guid _guid))
        {
            var line = BelongBlock.GetLine(_guid);
            line.line = this.line;
            line = this;
        }
    }
    public void Update(string _characterId, string _line, int _index)
    {
        this.characterId = _characterId;
        this.line = _line;
        this.index = _index;
        lineAsset.UpdateContent(this);
        if (Guid.TryParse(guid, out Guid _guid))
        {
           var line = BelongBlock.GetLine(_guid);
            line.index = this.index;
            line = this;
        }
    }
    public void SelfDestroy()
    {
        try
        {
            if (Guid.TryParse(guid, out Guid _guid))
                belongBlock.TLLineBeenDestroyed(_guid);
        }
        catch (Exception)
        {
            Debug.Log("belongBlock is empty");
        }
    }
    //public void PauseTL()
    //{
    //    UtageTLManager.Inst.Pause();
    //    FindBlockFromScene();
    //    if (belongBlock != null)
    //        belongBlock.Pause();
    //}
    [Button]
    public void SoloBind(bool _flag)
    {
        FindBlockFromScene();
        if (belongBlock != null)
            belongBlock.SoloFromLineGroup(_flag, guid);
    }
    private void FindBlockFromScene()
    {
        if (belongBlock != null)
            return;

        Scene scene = SceneManager.GetActiveScene();
        var gos = scene.GetRootGameObjects();
        Transform foundGoj = null;
        for (int i = 0; i < gos.Length; i++)
        {
            foundGoj = gos[i].transform.FindChildInChildrens(blockName);
            if (foundGoj != null)
                break;
        }
        if (foundGoj != null)
            belongBlock = foundGoj.GetComponent<UtageCinemaBlock>();
    }
}

