using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;

public class demo_UtageDialogBGMaskSwitch : MonoBehaviour
{
    [SerializeField] GameObject dialogueBGMask;

    // Start is called before the first frame update
    void Start()
    {
        EventHandler.RegisterEvent(UtageTLManager.EVENT_PERFORM_LINE_ENTER, TurnOffDialogueMask);
        EventHandler.RegisterEvent(UtageTLManager.EVENT_PERFORM_LINE_EXIT, TurnOnDialogueMask);
    }
    private void OnDestroy()
    {
        EventHandler.UnregisterEvent(UtageTLManager.EVENT_PERFORM_LINE_ENTER, TurnOffDialogueMask);
        EventHandler.UnregisterEvent(UtageTLManager.EVENT_PERFORM_LINE_EXIT, TurnOnDialogueMask);
    }
    private void TurnOnDialogueMask()
    {
        if (dialogueBGMask != null)
            dialogueBGMask.SetActive(true);
    }
    private void TurnOffDialogueMask()
    {
        if (dialogueBGMask != null)
            dialogueBGMask.SetActive(false);
    }
}
