using System;

using UnityEngine;
#if UNITY_EDITOR
#endif
public static class HHUtageTLStatic
{
    public static bool UtageTLInputOverride
    {
        get
        {
            bool isUtageTLRunning = (UtageTLManager.Inst != null) && (UtageTLManager.Inst.IsPerforming);
            return isUtageTLRunning;
        }
    }
    public static Func<bool> InputHandler;
    public static Action<TLCharacterLine> PauseHandler;
    public static Action<TLCharacterLine> CloseSkipHandler;
    public static Action<TLCharacterLine> TLLineOnStartHandler;
    public static Action HideWindowHandler;
    public static Action ShowWindowHandler;
    public static Action CloseTLManagerHandler;
    public static Action<UtageCinemaBlock> RebuildHandler;
    private static string currentGraphGuid = "";

    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        Application.quitting += Quit;
    }
    static void Quit() 
    {
        InputHandler = null;
        PauseHandler = null;
        CloseSkipHandler = null;
        TLLineOnStartHandler = null;
        HideWindowHandler = null;
        RebuildHandler = null;
        currentGraphGuid = "";
    }

    public static void RebuildGraph(UtageCinemaBlock _block, string _guid)
    {
        if (currentGraphGuid != _guid)
        {
            currentGraphGuid = _guid;
            _block.RebuildAtEditorFrame();
        }
    }
    public static void SetLoopAnimation(UtageCinemaBlock _block)
    {
        //_block.SetActorLoopAnim();
    }
    public static void HideWindow()
    {
        HideWindowHandler?.Invoke();
    }
    public static void ShowWindow()
    {
        ShowWindowHandler?.Invoke();
    }
    public static void Pause(TLCharacterLine _line)
    {
        PauseHandler?.Invoke(_line);
    }
    public static void TLLineOnStart(TLCharacterLine _line)
    {
        TLLineOnStartHandler?.Invoke(_line);
    }
    public static void CloseSkipper(TLCharacterLine _line)
    {
        CloseSkipHandler?.Invoke(_line);
    }
    public static void CloseTLManager(UtageCinemaBlock _block) 
    {
        //CloseTLManagerHandler?.Invoke();
        UtageTLManager.Inst.EndTLFadeOut(_block);
    }
}
