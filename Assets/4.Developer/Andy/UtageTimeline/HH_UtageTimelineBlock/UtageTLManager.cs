using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Timeline;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using GUID = UnityEditor.GUID;
#endif
using Sirenix.OdinInspector;

using NPOI.SS.UserModel;
using System.Collections;

using AdvUguiManager_Subtitle = Utage.AdvUguiManager_Subtitle;
using MergeTrackMode = UtageCinemaBlock.MergeTrackMode;
using Debug = UnityEngine.Debug;
using EventHandler = Opsive.Shared.Events.EventHandler;

public class UtageTLManager : MonoBehaviour
{
    private static UtageTLManager inst;
    public static UtageTLManager Inst
    {
        get
        {
            return inst;
        }
    }
    public PlayableDirector director;
    public const string GUIDLABEL = "GUID";
    public const string ARG1LABEL = "Arg1";
    public const string EVENT_PERFORM_LINE_ENTER = "EVENT_PERFORM_LINE_ENTER";
    public const string EVENT_PERFORM_LINE_EXIT = "EVENT_PERFORM_LINE_EXIT";

    [SerializeField] string sheetName = "";
    [SerializeField] string startSenario = "";
    [SerializeField] UnityEngine.Object excelDoc;
    [SerializeField] UnityEngine.Object blockFolder;
    [SerializeField] UtageCinemaBlock blockPrefab;
    [SerializeField] bool needHidePlayer = true;
    [SerializeField] bool introFade = true, outroFade = true;
    [SerializeField] bool bgmMute = false;
    [SerializeField] SenarioBlockDic blockDic;

    [FoldoutGroup("ReadOnly", false, 0)]
    [ReadOnly] [SerializeField] string sheetPath;
    [FoldoutGroup("ReadOnly", false)]
    [ReadOnly] [SerializeField] string blockFolderPath;
    [FoldoutGroup("ReadOnly", false)]
    [ReadOnly] [SerializeField] string textLabel = "Text";
    [FoldoutGroup("ReadOnly", false)]
    [ReadOnly] [SerializeField] int CharactorColumn = -1;
    [FoldoutGroup("ReadOnly", false)]
    [ReadOnly] [SerializeField] int TextColumn = -1;
    [FoldoutGroup("ReadOnly", false)]
    [ReadOnly] [SerializeField] int GUIDColumn = -1;
    [FoldoutGroup("CallbackEvent", 3)]
    public UnityEvent OnStart;
    [FoldoutGroup("CallbackEvent")]
    public UnityEvent OnFinish;

    [HideLabel]
    [FoldoutGroup("ForceLookResetPos")]

    //[OnInspectorInit("@forceLookResetPos.RegistGoj = this.gameObject")]
    //[SerializeField] ForceLookResetPos forceLookResetPos;

    private bool AppIsPlaying { get => Application.isPlaying; }
    [FoldoutGroup("ReadOnly", false)]
    [ReadOnly] [SerializeField] private bool forceNL = false;
    private bool lineFinish = false;
    private bool isPerforming = false;
    private Coroutine waitShowWindowCoroutine;

    public bool IsPerforming
    {
        get
        {
            return isPerforming;
        }
    }
    [ReadOnly] [ShowInInspector]
    [FoldoutGroup("ReadOnly", false)]
    static private bool tlPlayBlocker = true;
    [FoldoutGroup("ReadOnly", false)]
    [ReadOnly] [ShowInInspector] static private bool isPlaying = false;
    private bool playTLOnly = false, windowShowing = false, jumpProcessingBlock = false, finishEndTrigger = false, nextLineCallbackInit = false;

#if UNITY_EDITOR
    #region Editor Method

    [Button("Init")] [HideIf("AppIsPlaying")] [FoldoutGroup("Method", 4)]
    private void InfoColumnInit()
    {
        if (excelDoc == null)
            return;
        IWorkbook book = HHUtageUtility.ReadBook(sheetPath);
        ISheet targetSht = book.GetSheet(sheetName);
        if (targetSht == null)
        {
            Debug.LogError(sheetName + " is not found.");
            return;
        }
        else
        {
            IRow firstRow = targetSht.GetRow(0);
            for (int i = 0; i < firstRow.LastCellNum; i++)
            {
                ICell cell = firstRow.GetCell(i);
                string str = cell.StringCellValue.Replace(" ", "");
                if (str == ARG1LABEL)
                    CharactorColumn = i;
                else if (str == textLabel)
                    TextColumn = i;
                else if (str == GUIDLABEL)
                    GUIDColumn = i;
            }
            if (GUIDColumn == -1)
            {
                ICell guidLabelCell = firstRow.CreateCell(firstRow.LastCellNum);
                guidLabelCell.SetCellValue(GUIDLABEL);
                GUIDColumn = firstRow.LastCellNum;

                using (FileStream fs = new FileStream(sheetPath, FileMode.Create, FileAccess.Write))
                {
                    book.Write(fs);
                }
            }
        }
    }
    [Button("Clear")] [HideIf("AppIsPlaying")] [FoldoutGroup("Method")]
    private void ClearNullBlock()
    {
        List<int> emptyIndexs = new List<int>();
        for (int i = 0; i < blockDic.Count; i++)
        {
            var keyPairValue = blockDic.ElementAtOrDefault(i);
            if (keyPairValue.Value == null)
                emptyIndexs.Add(i);
        }
        var elements = blockDic.Where(x => x.Value != null).ToArray();
        blockDic = new SenarioBlockDic();
        for (int i = 0; i < elements.Length; i++) {
            blockDic.Add(elements[i].Key, elements[i].Value);
        }

        var tlAsset = (TimelineAsset)director.playableAsset;
        var blocksCtrlTrack = GetOrNewBlockContrlTrack(tlAsset);
        if (blocksCtrlTrack == null)
            return;

        var clips = blocksCtrlTrack.GetClips().ToArray();
        for (int i = 0; i < emptyIndexs.Count; i++)
        {
            if (clips.Length > emptyIndexs[i])
            {
                tlAsset.DeleteClip(clips[emptyIndexs[i]]);
            }
        }
    }
    [Button("UnLock")] [HideIf("AppIsPlaying")] [FoldoutGroup("Method")]
    private void UnLockAllBlocks()
    {
        SetAllBlockLocker(false);
    }
    [Button] [HideIf("AppIsPlaying")] [FoldoutGroup("Method")]
    private void Install()
    {
        ClearDic();
        CreateBlockProccessor();
        CreateBlockTLAsset();
        ImportMasterTLClipAsset();
    }
    [Button("Update")] [HideIf("AppIsPlaying")] [FoldoutGroup("Method")]
    private void UpdateContent()
    {
        if (excelDoc == null || GUIDColumn == -1)
            return;

        if (blockDic.Any(x => x.Value.MergeState == UtageCinemaBlock.MergeStateEnum.Mergeing))
        {
            Debug.LogError("Some block still in 'Mergeing' state. DisMerge it before update.");
            return;
        }

        ISheet targetSht = HHUtageUtility.GetSheet(sheetName, sheetPath);
        if (targetSht == null)
        {
            Debug.LogError(sheetName + " is not found.");
            return;
        }
        // Do it twice because it alway get lose to shifting in the end block 
        EditorCoroutineUtility.StartCoroutine(UpdateContent_Coroutine(targetSht), this);
    }
    private void SetAllBlockLocker(bool _flag)
    {
        if (blockDic == null || blockDic.Count == 0)
            return;

        for (int i = 0; i < blockDic.Count; i++)
        {
            var block = blockDic.ElementAtOrDefault(i).Value;
            block.BindingLocker(_flag);
        }
    }
    private IEnumerator UpdateContent_Coroutine(ISheet _sheet)
    {
        string currentSenario = "";
        List<string> existLines = new List<string>();
        int indexInBlock = 0;
        int[] performIndexs = null;
        UtageCinemaBlock block = null;

        for (int rowIndex = /*_sheet.FirstRowNum + */1; rowIndex <= _sheet.LastRowNum; ++rowIndex)
        {
            IRow row = _sheet.GetRow(rowIndex);
            if (row == null)
                continue;
            int firstContentCellNum = -99;
            ICell cell = null;
            string currentStr = "";
            for (int i = 0; i < row.LastCellNum; i++)
            {
                cell = row.GetCell(i);
                if (cell == null)
                    continue;
                if (!string.IsNullOrEmpty(cell.StringCellValue))
                {
                    firstContentCellNum = i;
                    currentStr = cell.StringCellValue;
                    break;
                }
            }

            if (currentStr == "")
                continue;

            if (currentStr[0] == '*')
            {
                if (currentSenario != "" && currentSenario != currentStr)
                {
                    var nextBlock = blockDic[currentSenario];
                    nextBlock.RemoveUnUseByExistDatas(existLines.ToArray());
                    indexInBlock = 0;
                    yield return new WaitForSeconds(0.45f);
                }
                currentSenario = currentStr;
                existLines = new List<string>();
                block = blockDic[currentSenario];
                performIndexs = block.GetPerformIndexs();
            }
            if (!(currentStr[0] == '*'))
            {
                string text = "", character = "", guidStr = "";
                if (row.GetCell(GUIDColumn) != null && row.GetCell(GUIDColumn).ToString() != "")
                {
                    guidStr = row.GetCell(GUIDColumn).ToString();
                    existLines.Add(guidStr);

                    if (row.GetCell(TextColumn) != null && row.GetCell(TextColumn).ToString() != "")
                        text = row.GetCell(TextColumn).ToString();
                    if (row.GetCell(CharactorColumn) != null && row.GetCell(CharactorColumn).ToString() != "")
                        character = row.GetCell(CharactorColumn).ToString();

                    var guid = Guid.Parse(guidStr);
                    var line = GetLineFromDic(guid);
                    if (line == null)
                    {
                        line = block.InsertNewLine(indexInBlock, character, text, guid);
                        block.InsertNewClip(indexInBlock, line);
                        yield return new WaitForSeconds(0.45f);
                    }

                    if (line != null)
                    {
                        line.Update(character, text, indexInBlock);
                    }

                    indexInBlock++;
                }
            }
            if (currentStr == "EndScenario")
            {
                if (block.MergeState == UtageCinemaBlock.MergeStateEnum.Mergeing)
                    continue;

                block.RemoveUnUseByExistDatas(existLines.ToArray());
                indexInBlock = 0;
                yield return null;
            }
        }
        yield return null;

        SetAllBlockLocker(true);
    }

    private TLCharacterLine GetLineFromDic(Guid _guid)
    {
        for (int i = 0; i < blockDic.Count; i++)
        {
            var block = blockDic.ElementAtOrDefault(i).Value;
            var line = block.GetLine(_guid);
            if (line != null)
            {
                return line;
            }
        }
        return null;
    }
    private void ImportMasterTLClipAsset()
    {
        var tlAsset = (TimelineAsset)director.playableAsset;
        UtageTLBlockControlTrack contrlTrack = GetOrNewBlockContrlTrack(tlAsset);
        var clips = contrlTrack.GetClips();
        if (clips.Count() == 0)
        {
            for (int i = 0; i < blockDic.Count; i++)
            {
                var block = blockDic.ElementAtOrDefault(i).Value;
                var clip = contrlTrack.CreateDefaultClip();
                block.BlockClipAsset = clip.asset as PlayableAsset;
                clip.displayName = blockDic.ElementAtOrDefault(i).Key;
                clip.displayName = clip.displayName.Replace("*", "");
                var clipAsset = clip.asset as ControlPlayableAsset;

                clipAsset.sourceGameObject.exposedName = GUID.Generate().ToString();
                director.SetReferenceValue(clipAsset.sourceGameObject.exposedName, block.gameObject);
            }
        }
    }
    private void CreateBlockTLAsset()
    {
        if (blockFolder == null)
            return;
        blockFolderPath = AssetDatabase.GetAssetPath(blockFolder);

        if (blockDic == null || blockDic.Count == 0)
            return;

        var filePaths = Directory.GetFiles(blockFolderPath, "*.playable");
        List<string> fileNameList = new List<string>();
        for (int i = 0; i < filePaths.Length; i++)
        {
            var tl = AssetDatabase.LoadAssetAtPath(filePaths[i], typeof(TimelineAsset));
            fileNameList.Add(tl.name);
        }

        for (int i = 0; i < blockDic.Count; i++)
        {
            var block = blockDic.ElementAtOrDefault(i).Value;
            if (fileNameList.Count == 0 || !fileNameList.Contains(block.Senario))
            {
                TimelineAsset timeline = new TimelineAsset();
                AssetDatabase.CreateAsset(timeline, blockFolderPath + "/" + block.Senario.Replace("*", "") + ".playable");
                timeline.CreateTrack<UtageTLLineTrack>(null, "LineTrack");
                block.Director.playableAsset = timeline;
                block.CreateData();
            }
        }
    }

    private void CreateBlockProccessor()
    {
        if (excelDoc == null)
            return;
        sheetPath = AssetDatabase.GetAssetPath(excelDoc);

        ISheet targetSht = HHUtageUtility.GetSheet(sheetName, sheetPath);
        if (targetSht == null)
        {
            Debug.LogError(sheetName + " is not found.");
            return;
        }
        else
        {
            UtageCinemaBlock _block = null;
            int indexInBlock = 0;
            string currentSenario = "";
            for (int rowIndex = /*targetSht.FirstRowNum + */ 1; rowIndex <= targetSht.LastRowNum; ++rowIndex)
            {
                IRow row = targetSht.GetRow(rowIndex);
                if (row == null)
                    continue;
                int firstContentCellNum = -99;
                ICell cell = null;
                string currentStr = "";
                for (int i = 0; i < row.LastCellNum; i++)
                {
                    cell = row.GetCell(i);
                    if (cell == null)
                        continue;
                    if (!string.IsNullOrEmpty(cell.StringCellValue))
                    {
                        firstContentCellNum = i;
                        currentStr = cell.StringCellValue;
                        break;
                    }
                }

                if (currentStr == "")
                    continue;

                if (currentStr[0] == '*')
                {
                    if (currentSenario != "" && currentSenario != currentStr)
                    {
                        currentSenario = currentStr;
                        indexInBlock = 0;
                        _block = null;
                    }

                    _block = Instantiate(blockPrefab, transform);
                    _block.name = currentStr;
                    _block.Senario = currentStr;
                    _block.utageTL = this;
                    blockDic.Add(currentStr, _block);
                }
                else
                {
                    bool isCharactorLine = UtageCharacterRegisterBoard.Inst.CharacterDic.HasCharacter(currentStr);
                    if (firstContentCellNum == 1 && isCharactorLine)
                    {
                        string characterId = row.GetCell(firstContentCellNum).StringCellValue;
                        string line = row.GetCell(7).StringCellValue;
                        string guidStr = row.GetCell(row.LastCellNum - 1).StringCellValue;

                        if (Guid.TryParse(guidStr, out Guid guid))
                        {
                            if (_block != null && line != "" && characterId != "")
                            {
                                _block.Regist(characterId, line, guid, indexInBlock);
                                indexInBlock++;
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
#endif

    #region GamePlay Method
    private void OnEnable()
    {
        InitRegistProcess();
    }
    private void InitRegistProcess()
    {
        if (inst == this)
            return;

        HHUtageTLStatic.PauseHandler = null;
        HHUtageTLStatic.CloseSkipHandler = null;
        HHUtageTLStatic.InputHandler = null;
        HHUtageTLStatic.TLLineOnStartHandler = null;

        HHUtageTLStatic.HideWindowHandler = null;
        HHUtageTLStatic.ShowWindowHandler = null;

        tlPlayBlocker = true;
        director.played += OnDirectorPlayed;
        inst = this;
        HHUtageTLStatic.PauseHandler += Pause;
        HHUtageTLStatic.InputHandler += WaitInputInLineBlock;
        HHUtageTLStatic.TLLineOnStartHandler += LineOnStartCheck;
        HHUtageTLStatic.TLLineOnStartHandler += SelectedLineCheck;

        StartCoroutine(RegistDialogNLCallbackCoroutine());
        StartCoroutine(BuildBlockMergeAfterFrame());
    }
    IEnumerator RegistDialogNLCallbackCoroutine()
    {
        yield return new WaitUntil(() => UtageDialogCommander.Inst != null);

        UtageDialogCommander.Inst.RegistNLCallback(DialogType.Dialog, NLEventType.Start, LineTxtStart);
        UtageDialogCommander.Inst.RegistNLCallback(DialogType.Dialog, NLEventType.End, LineTxtEnd);
        UtageDialogCommander.Inst.RegistNLCallback(DialogType.Dialog, NLEventType.InputTri, WaitForceInput);
        nextLineCallbackInit = true;
    }
    private void Update()
    {
        if (!IsPerforming)
            return;

        //if (InputManager.GetButtonPerformed(InputManager.Button.Skip))
        //{
        //    SkipCanvasHUD.ins.HoldOn();
        //}
        //else
        //{
        //    SkipCanvasHUD.ins.UnHold();
        //}
    }
    private void OnDisable()
    {
        //if (mergeMode == MergeTrackMode.Auto)
        DisbatchMergeTracks();
        StopAllCoroutines();
        director.played -= OnDirectorPlayed;
        inst = null;

        HHUtageTLStatic.PauseHandler -= Pause;
        HHUtageTLStatic.InputHandler -= WaitInputInLineBlock;
        HHUtageTLStatic.TLLineOnStartHandler -= LineOnStartCheck;
        HHUtageTLStatic.TLLineOnStartHandler -= SelectedLineCheck;

        HHUtageTLStatic.PauseHandler = null;
        HHUtageTLStatic.CloseSkipHandler = null;
        HHUtageTLStatic.InputHandler = null;
        HHUtageTLStatic.TLLineOnStartHandler = null;

        HHUtageTLStatic.HideWindowHandler = null;
        HHUtageTLStatic.ShowWindowHandler = null;

        UtageDialogCommander.Inst.RemoveNLCallback(DialogType.Dialog, NLEventType.Start, LineTxtStart);
        UtageDialogCommander.Inst.RemoveNLCallback(DialogType.Dialog, NLEventType.End, LineTxtEnd);
        UtageDialogCommander.Inst.RemoveNLCallback(DialogType.Dialog, NLEventType.InputTri, WaitForceInput);
    }

    

    TLCharacterLine currentLine = null;
    bool doubleInputLock = false;
    private bool WaitInputInLineBlock()
    {
        //if (doubleInputLock)
        //    return false;
        if (playTLOnly || jumpProcessingBlock)
            return false;

        var currentBlock = GetCurrentBlock(out int index);
        var directorT = GetLocalTimeInBlock(index);

        if (currentLine == null || currentBlock == null)
            return false;

        var lastLineInBlock = currentBlock.GetLines()
            .OrderBy(x => x.lineAsset.clip.end)
            .LastOrDefault();

        // EndTL
        if (currentLine.guid == lastLineInBlock.guid
            && currentLine.lineAsset.endEventType == UtageTLLineAsset.EndEventType.EndTL
            && lineFinish)
        {
            EndTLFadeOut(currentBlock);
            return false;
        }
        else
        {
            var ntLine = NextLine();
            if (currentLine.lineAsset.type == UtageTLLineAsset.TLLineType.Line && currentLine.lineAsset.endEventType == UtageTLLineAsset.EndEventType.Next)
            {
                if (lineFinish)
                {
                    director.enabled = true;
                    director.time = GetNextLineTime();
                    ResetTLSpeed();
                }
            }
            else if (ntLine != null && currentLine.lineAsset.endEventType == UtageTLLineAsset.EndEventType.Next && ntLine.lineAsset.type == UtageTLLineAsset.TLLineType.Perform)
            {
                //HideMainMessageWindow();
                director.time = GetNextLineTime();
                director.enabled = true;
                ResetTLSpeed();
            }
            else if (currentLine.lineAsset.endEventType == UtageTLLineAsset.EndEventType.Jump)
            {
                if (lineFinish)
                {
                    JumpToBlock(currentLine.lineAsset.JumpLabel);
                    ResetTLSpeed();
                }
            }
            else if (currentLine.lineAsset.endEventType == UtageTLLineAsset.EndEventType.Selection)
            {

            }
            else if (currentLine.lineAsset.endEventType == UtageTLLineAsset.EndEventType.Block)
            {
                if (lineFinish)
                {
                    currentBlock.TriggerLineEvent(UtageCinemaBlock.LineCallbackEvent.Type.OnEnd, currentLine.guid);

                    return false;
                }
            }
            else if (currentLine.lineAsset.endEventType == UtageTLLineAsset.EndEventType.EndTL)
            {
                if (lineFinish)
                {
                    EndTLFadeOut(currentBlock);
                    return false;
                }
            }

            if (lineFinish)
            {
                currentBlock.TriggerLineEvent(UtageCinemaBlock.LineCallbackEvent.Type.OnEnd, currentLine.guid);
            }
        }
        if (currentLine.lineAsset.type == UtageTLLineAsset.TLLineType.Perform)
        {
            return false;
        }
        else
        {
            //if (!doubleInputLock)StartCoroutine(InputLockCoroutine());
            return true;
        }
    }
    IEnumerator InputLockCoroutine()
    {
        doubleInputLock = true;
        yield return new WaitForSeconds(0.6f);
        doubleInputLock = false;
    }
    public void EndTLFadeOut(UtageCinemaBlock currentBlock)
    {
        if (finishEndTrigger)
            return;

        finishEndTrigger = true;
        currentBlock.TriggerLineEvent(UtageCinemaBlock.LineCallbackEvent.Type.OnEnd, currentLine.guid);
        StopPerformance();

        //if (!outroFade)
        //{
        //    StopPerformance();
        //}
        //else
        //{
        //    FadeManager.ins.FadeIn(2, () =>
        //    {
        //        StopPerformance();
        //    }, true);
        //}
    }

    private TLCharacterLine NextLine()
    {
        var runningBlock = GetCurrentBlock(out int index);
        double directorT = GetLocalTimeInBlock(index);

        TLCharacterLine tlLine = null;
        if (currentLine != null && currentLine.guid == runningBlock.GetLines().LastOrDefault().guid)
        {
            if (runningBlock == blockDic.LastOrDefault().Value)
            {
                return null;
            }
            else
            {
                runningBlock = blockDic.ElementAtOrDefault(index + 1).Value;
                tlLine = runningBlock.GetLines().ElementAtOrDefault(0);
                return tlLine;
            }
        }

        var blockTrack = GetTLBlockTrack();
        double nlT = director.time;
        if (index > 0)
        {
            var clips = blockTrack.GetClips().ToArray();
            nlT -= clips[index - 1].end;
        }

        tlLine = runningBlock.GetNextLine(nlT);
        //if (tlLine == null)
        //    Debug.LogError("Empty next line");

        return tlLine;
    }

    IEnumerator BuildBlockMergeAfterFrame()
    {
        yield return new WaitForSeconds(0.5f);
        BuildBlockMergeTracks();
    }
    //[Button("MergeTrack")][FoldoutGroup("Method")]
    private void BuildBlockMergeTracks()
    {
        if (blockDic == null || blockDic.Count == 0)
            return;

        for (int i = 0; i < blockDic.Count; i++)
        {
            var block = blockDic.ElementAtOrDefault(i).Value;
            if (block.mergeMode == MergeTrackMode.Auto)
                block.RuntimeBuildMergeTrack();
        }
    }
    //[Button("DisMerge")] [FoldoutGroup("Method")]
    private void DisbatchMergeTracks()
    {
        if (blockDic == null || blockDic.Count == 0)
            return;
        for (int i = 0; i < blockDic.Count; i++)
        {
            var block = blockDic.ElementAtOrDefault(i).Value;
            if (block.mergeMode == MergeTrackMode.Auto)
            {
                block.DisbatchMergeTrack();
            }
        }
    }
    private void HideMainMessageWindow()
    {
        var engine = DialogWindowsPool.Instance.GetFirstAdvEngine(DialogType.Dialog);
        if (engine != null && !engine.UiManager.IsHide)
        {
            windowShowing = false;
            engine.UiManager.HideMessageWindow();
        }
    }
    private void ShowMainMessageWindow()
    {
        var engine = DialogWindowsPool.Instance.GetFirstAdvEngine(DialogType.Dialog);
        if (engine != null && engine.UiManager.IsHide)
        {
            windowShowing = true;
            engine.UiManager.ShowMessageWindow();
        }
    }
    [Button] [FoldoutGroup("Method")]
    public void CloseDialog()
    {
        UtageDialogCommander.Inst.StopDialogByType(DialogType.Dialog);
    }
    private void OnDirectorPlayed(PlayableDirector _director)
    {
        StartCoroutine(FrameBlockWhenTLPlay());
    }
    private UtageCinemaBlock GetCurrentBlock(out int _index)
    {
        if (blockDic == null || blockDic.Count == 0)
        {
            _index = -1;
            return null;
        }
        var blockTrack = GetTLBlockTrack();
        if (!blockTrack.hasClips)
        {
            _index = -1;
            return null;
        }

        var blockClips = blockTrack.GetClips().ToArray();
        int currentBlockIndex = 0;
        for (int i = 0; i < blockClips.Length; i++)
        {
            var pAsset = blockClips[i].asset as ControlPlayableAsset;
            if (blockClips[i].start <= director.time
                && blockClips[i].end > director.time)
            {
                currentBlockIndex = i;
                break;
            }
        }
        UtageCinemaBlock currentLineBlock = null;
        string currentBlockClipName = blockClips[currentBlockIndex].displayName;
        if (currentBlockClipName.Contains("*"))
            currentBlockClipName = currentBlockClipName.Replace("*", "");

        for (int i = 0; i < blockDic.Count; i++)
        {
            string key = blockDic.ElementAtOrDefault(i).Key.Replace("*", "");
            if (key == currentBlockClipName)
            {
                currentLineBlock = blockDic.ElementAtOrDefault(i).Value;
                break;
            }
        }

        _index = currentBlockIndex;
        return currentLineBlock;
    }

    private UtageTLBlockControlTrack GetTLBlockTrack()
    {
        UtageTLBlockControlTrack blockTrack = null;
        var tlAsset = (TimelineAsset)director.playableAsset;
        foreach (var track in tlAsset.GetOutputTracks())
        {
            if (track.GetType() == typeof(UtageTLBlockControlTrack))
            {
                blockTrack = track as UtageTLBlockControlTrack;
                break;
            }
        }

        return blockTrack;
    }

    [Button] [FoldoutGroup("Method")]
    private void NextLineTime()
    {
        var t = GetNextLineTime();
        director.time = t;
    }
    private double GetNextLineTime()
    {
        var currentLineBlock = GetCurrentBlock(out int currentBlockIndex);

        var blockTrack = GetTLBlockTrack();
        var blockClips = blockTrack.GetClips().ToArray();

        double timeInblock = 0d, answer = 0d;
        int clipIndex = 1;
        TimelineClip nextClip = null;
        if (currentLineBlock != null)
        {
            if (currentBlockIndex > 0)
                timeInblock = director.time - blockClips[currentBlockIndex - 1].end;
            else
                timeInblock = director.time;
            nextClip = currentLineBlock.GetNextClip(timeInblock, out clipIndex);
        }

        if (nextClip != null)
        {
            if (currentBlockIndex > 0)
                answer = nextClip.start + blockClips[currentBlockIndex - 1].end;
            else
                answer = nextClip.start;
        }
        else
        {
            if (blockClips.Length - 1 > currentBlockIndex)
                answer = blockClips[currentBlockIndex + 1].start;
            else
                answer = blockClips[currentBlockIndex].end;
        }
        return answer;
    }
    private void NextLineProcess()
    {
        if (tlPlayBlocker)
            return;

        if (!isPlaying)
        {
            ResetTLSpeed();
            return;
        }
        NextLineTime();
        StartCoroutine(FrameBlockWhenTLPlay());
    }
    private void LineTxtEnd()
    {
        lineFinish = true;
    }
    private void LineTxtStart()
    {
        lineFinish = false;
        jumpProcessingBlock = false;
    }
    [Button] [FoldoutGroup("Method")]
    [ShowIf("AppIsPlaying")]
    private void PlayTLOnly()
    {
        if (startSenario != "")
            JumpToBlock(startSenario);

        director.Play();
        director.RebuildGraph();
        playTLOnly = true;
    }
    [Button] [ShowIf("AppIsPlaying")]
    [FoldoutGroup("Method")]
    public void Perform()
    {
        gameObject.SetActive(true);
        InitRegistProcess();

        //GameManager.ins.LockPlayer(true);
        //EventHandler.ExecuteEvent(UILayerEventHandler.EVENT_UTAGETL_START);
        //if (introFade)
        //{
        //    //FadeManager.ins.FadeIn(0.5f, StartDE);
        //    Func<bool> condition = () => { return nextLineCallbackInit; };
        //    FadeManager.ins.FadeIn(0.5f, () => StartCoroutine(WaitCoditionRunAction(condition, StartDE)));
        //}
        //else
        //{
            //StartDE();
            Func<bool> condition = () => { return nextLineCallbackInit; };
            StartCoroutine(WaitCoditionRunAction(condition, StartDE));
        //}
    }
    private IEnumerator WaitCoditionRunAction(Func<bool> _func, Action _action)
    {
        yield return new WaitUntil(_func);
        _action.Invoke();
    }
    private void StartDE()
    {
        //if (needHidePlayer)
        //    Player.ins.SetHide(true);
        //GameManager.HideHUD();
        //if (introFade) FadeManager.ins.FadeOut();

        UnityAction action = () =>
        {
            StopPerformance();
            //if (!outroFade)
            //{
            //    StopPerformance();
            //    SkipCanvasHUD.ins.Release();
            //}
            //else
            //{
            //    FadeManager.ins.FadeIn(1, () =>
            //    {
            //        StopPerformance();
            //        SkipCanvasHUD.ins.Release();
            //    }, true);
            //}
        };
        //SkipCanvasHUD.ins.Use();
        //SkipCanvasHUD.ins.AddFinishCallBack(action);

        if (startSenario != "")
            JumpToBlock(startSenario);

        var currentBlock = GetCurrentBlock(out int index);
        var firstLine = currentBlock.GetLines()
            .OrderBy(x => x.lineAsset.clip.end)
            .FirstOrDefault();

        if (firstLine.lineAsset.type == UtageTLLineAsset.TLLineType.Perform)
        {
            //StartCoroutine(WaitIntroPerform());
        }
        else
        {
            string s = (startSenario == "") ? sheetName : startSenario;
            if (!s.Contains("*"))
                s = '*' + s;
            UtageDialogCommander.Inst.SayDialogByType(DialogType.Dialog, s, gameObject);
        }

        OnStart?.Invoke();
        director.Play();
        isPerforming = true;
        isPlaying = true;
        //if (bgmMute) FMODManager.ins.BGM_FadeOutToNone();
        //EventHandler.ExecuteEvent(UILayerEventHandler.EVENT_UTAGETL_START);
    }
    [Button] [FoldoutGroup("Method")]

    private void JumpToBlock(string _senario)
    {
        string s = _senario;
        if (_senario.Contains('*'))
            s = s.Replace("*", "");

        var tlAsset = (TimelineAsset)director.playableAsset;
        var tracks = tlAsset.GetRootTracks();
        var blockTrack = tracks
            .Where(x => x.GetType() == typeof(UtageTLBlockControlTrack))
            .Select(z => z as UtageTLBlockControlTrack).FirstOrDefault();

        if (blockTrack == null || !blockTrack.hasClips)
            return;

        var targetClip = blockTrack.GetClips()
            .Where(x => x.displayName.Contains(s))
            .Select(z => z).FirstOrDefault();

        if (targetClip == null)
            return;

        jumpProcessingBlock = true;
        director.time = targetClip.start;
    }
    IEnumerator WaitIntroPerform()
    {
        var currentBlock = GetCurrentBlock(out int index);
        while (true)
        {
            var t = GetLocalTimeInBlock(index);
            var line = currentBlock.GetLine(t);
            if (line.lineAsset.type == UtageTLLineAsset.TLLineType.Line)
                break;

            yield return null;
        }
        //UtageDialogCommander.Instance.SayDialogByType(DialogType.Dialog, sheetName, gameObject);
    }
    IEnumerator FrameBlockWhenTLPlay()
    {
        tlPlayBlocker = true;
        yield return new WaitForSeconds(0.25f);
        tlPlayBlocker = false;
    }
    #endregion
    [Button("Continue")] [ShowIf("AppIsPlaying")]
    [FoldoutGroup("Method")]

    private void ResetTLSpeed()
    {
        director.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        director.Play();
        tlPlayBlocker = true;
        isPlaying = true;
        StartCoroutine(FrameBlockWhenTLPlay());
    }
    private void SelectedLineCheck(TLCharacterLine _line)
    {
        if (_line.lineAsset.endEventType != UtageTLLineAsset.EndEventType.Selection)
            return;
        var engine = DialogWindowsPool.Instance.GetFirstAdvEngine(DialogType.Dialog);
        var selectManager = engine.gameObject.GetComponent<Utage.AdvSelectionManager>();
        selectManager.OnSelected.AddListener(WaitSelectFinish);
    }

    private void WaitSelectFinish(Utage.AdvSelectionManager arg0)
    {
        JumpToBlock(arg0.Selected.JumpLabel);
        ResetTLSpeed();
        arg0.OnSelected.RemoveListener(WaitSelectFinish);
    }
    
    public void LineOnStartCheck(TLCharacterLine _line)
    {
        if (playTLOnly || !isPerforming)
            return;

        var currentBlock = GetCurrentBlock(out int index);
        currentLine = _line;
        inst = this;
        var engine = DialogWindowsPool.Instance.GetFirstAdvEngine(DialogType.Dialog);

        var firstLine = currentBlock.GetLines().Where(x => x.lineAsset.clip.start <= 0).Select(z => z).FirstOrDefault();

        if (_line.lineAsset.type == UtageTLLineAsset.TLLineType.Perform)
            EventHandler.ExecuteEvent(EVENT_PERFORM_LINE_ENTER);
        if (_line.lineAsset.type == UtageTLLineAsset.TLLineType.Line)
            EventHandler.ExecuteEvent(EVENT_PERFORM_LINE_EXIT);

        if (firstLine.lineAsset.type == UtageTLLineAsset.TLLineType.Perform && _line.guid == firstLine.guid/* && currentBlock == blockDic.FirstOrDefault().Value*/)
        {
            //Debug.Log("First line return");
            HideMainMessageWindow();

            return;
        }

        string s = (startSenario == "") ? sheetName : startSenario;
        if (!s.Contains("*"))
            s = '*' + s;

        if (engine == null 
            && _line.lineAsset.type == UtageTLLineAsset.TLLineType.Line)
        {
            UtageDialogCommander.Inst.SayDialogByType(DialogType.Dialog, s, gameObject);
            engine = DialogWindowsPool.Instance.GetFirstAdvEngine(DialogType.Dialog);
        }
        else
        {
            if (_line.lineAsset.type == UtageTLLineAsset.TLLineType.Line)
            {
                if (engine.ScenarioPlayer.IsEndScenario)
                {
                    UtageDialogCommander.Inst.SayDialogByType(DialogType.Dialog, s, gameObject);
                }
                ShowMainMessageWindow();
            }
            else if (_line.lineAsset.type == UtageTLLineAsset.TLLineType.Perform)
            {
                UtageDialogCommander.Inst.CharacterOff();
                HideMainMessageWindow();
                waitShowWindowCoroutine = 
                    StartCoroutine(WaitForWindowShow(_line));
            }
        }
    }
    public void StopPerformance()
    {
        OnFinish?.Invoke();
        director.Stop();

        CloseDialog();
        //GameManager.ins.LockPlayer(false);
        //GameManager.ShowHUD();

        currentLine = null;
        finishEndTrigger = false;
        isPerforming = false;

        //if (needHidePlayer)
        //    Player.ins.SetHide(false);
        if (waitShowWindowCoroutine != null)
            StopCoroutine(waitShowWindowCoroutine);

        //if (bgmMute) FMODManager.ins.BGM_FadeInToDefault();
        //EventHandler.ExecuteEvent(UILayerEventHandler.EVENT_UTAGETL_END);
        //UnityAction event_UIUtageEnd = () => EventHandler.ExecuteEvent(UILayerEventHandler.EVENT_UTAGETL_END);
        //if (forceLookResetPos == null)
        //{
        //    //if (outroFade) FadeManager.ins.FadeOut(4.5f/*, event_UIUtageEnd*/);
        //}
        //else
        //{
        //    //Action action = () => { if (outroFade)FadeManager.ins.FadeOut(4.5f/*, event_UIUtageEnd*/); };
        //    //forceLookResetPos.ForceLookPosReset(action);
        //}
        
        OnDisable();
        gameObject.SetActive(false);
    }
    private void WaitForceInput()
    {
        if (forceNL)
        {
            Debug.Log("forceNL turn off");
            
            forceNL = false;
        }
    }

    private double GetLocalTimeInBlock(int index)
    {
        double directorT = director.time;
        if (index > 0)
        {
            var blockTrack = GetTLBlockTrack();
            var clips = blockTrack.GetClips().ToArray();
            directorT -= clips[index - 1].end;
        }

        return directorT;
    }

    IEnumerator WaitForWindowShow(TLCharacterLine _line)
    {
        var engine = DialogWindowsPool.Instance.GetFirstAdvEngine(DialogType.Dialog);
        if (engine != null)
        {
            engine.UiManager.GetComponent<Canvas>().enabled = false;
            while (engine.UiManager.IsHide || !windowShowing)
            {
                yield return null;
            }
            //forceNL = true;
            yield return new WaitForSeconds(0.175f);
            engine.UiManager.GetComponent<Canvas>().enabled = true;
        }
    }
    private bool IsCurrentLineIsLineType()
    {
        var currentBlock = GetCurrentBlock(out int index);
        var currentLine = currentBlock.GetLine(director.time);
        return currentLine.lineAsset.type == UtageTLLineAsset.TLLineType.Line;
    }

    public void Pause(TLCharacterLine _line)
    {
        if (tlPlayBlocker || playTLOnly)
            return;

        if (!Application.isPlaying || director == null || director.playableAsset == null)
            return;

        try
        {
            Debug.Log("Pause");
            isPlaying = false;
            this.director.playableGraph.GetRootPlayable(0).SetSpeed(0d);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private UtageTLBlockControlTrack GetOrNewBlockContrlTrack(TimelineAsset tlAsset)
    {
        var tracks = tlAsset.GetRootTracks();
        UtageTLBlockControlTrack contrlTrack = null;

        foreach (var track in tracks)
        {
            if (!(track is UtageTLBlockControlTrack))
                continue;
            contrlTrack = track as UtageTLBlockControlTrack;
        }

        if (contrlTrack == null)
            contrlTrack = tlAsset.CreateTrack<UtageTLBlockControlTrack>(null, "BlocksContrl");

        return contrlTrack;
    }


    private void ClearDic()
    {
        if (blockDic != null && blockDic.Count != 0)
        {
            for (int i = 0; i < blockDic.Count; i++)
            {
                var _block = blockDic.ElementAtOrDefault(i).Value;
                if (_block == null)
                    continue;

                DestroyImmediate(_block.gameObject);
            }
        }
        blockDic = new SenarioBlockDic();
    }
    

    [Serializable]
    public class SenarioBlockDic : UnitySerializedDictionary<string, UtageCinemaBlock> { }
}
