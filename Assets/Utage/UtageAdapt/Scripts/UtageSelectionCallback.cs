using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utage;
using Sirenix.OdinInspector;

public class UtageSelectionCallback : MonoBehaviour
{
    public UtageSelectionCallbackDic CallbackEvents;
    private bool beenRegist = false;

    private void OnEnable()
    {
        if (!beenRegist)
        {
            beenRegist = true;
            StartCoroutine(WaitUtageInit());
        }
    }
    private void OnDisable()
    {
        if (beenRegist && UtageDialogCommander.Inst != null)
        {
            beenRegist = false;
            UtageDialogCommander.Inst.MainSelectionManager.OnSelected.RemoveListener(OnSelectedCallback);
        }
    }
    IEnumerator WaitUtageInit()
    {
        yield return new WaitUntil(() => UtageDialogCommander.Inst != null );
        UtageDialogCommander.Inst.MainSelectionManager
            .OnSelected.AddListener(OnSelectedCallback);
    }
    private void OnSelectedCallback(AdvSelectionManager _arg)
    {
        if (!CallbackEvents.ContainsKey(_arg.Selected.JumpLabel))
            return;

        CallbackEvents[_arg.Selected.JumpLabel]?.Invoke(_arg.Selected.Text);
    }

    [System.Serializable]
    public class UtageSelectionCallbackDic : UnitySerializedDictionary<string, UnityEvent<string>> { }
}
