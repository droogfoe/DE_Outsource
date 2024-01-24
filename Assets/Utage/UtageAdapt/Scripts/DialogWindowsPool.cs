using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;
using Sirenix.OdinInspector;

[System.Serializable]
public class DialogCommand 
{
    [EnumPaging]
    public DialogType dialogType = DialogType.Subtitle;
    public string senario = "Test1";
    public GameObject sender;
}
public enum DialogType
{
    Dialog = 1,
    Radio = 2,
    Bubble = 3,
    Subtitle = 4
}
public enum NLEventType
{
    Start,
    End,
    InputTri
}
[System.Serializable]
public class AdvWindowList
{
    public List<AdvUguiMessageWindow> List;
    public AdvWindowList()
    {
        List = new List<AdvUguiMessageWindow>();
    }
}
[System.Serializable]
public class DialogWindowDic : UnitySerializedDictionary<DialogType, AdvWindowList> { }
public class DialogWindowsPool : MonoBehaviour {
    private static DialogWindowsPool insntance;
    [SerializeField] DialogWindowDic dialogDic;
    public bool SingleDialogOnly;
    public static DialogWindowsPool Instance
    {
        get
        {
            return insntance;
        }
    }
    private void Awake()
    {
        if (insntance == null)
        {
            insntance = this;
        }
        else
        {
            Destroy(this);
        }
        dialogDic = new DialogWindowDic();
    }
    public void RegistDialog(AdvUguiMessageWindow messageWindow)
    {
        if (!dialogDic.ContainsKey(messageWindow.DialogType))
        {
            dialogDic.Add(messageWindow.DialogType, new AdvWindowList());
            dialogDic[messageWindow.DialogType].List.Add(messageWindow);
            //Debug.Log($"Regist dialog => {messageWindow.gameObject.name}/{messageWindow.GetInstanceID()}");
        }
        else
        {
            if (dialogDic[messageWindow.DialogType].List.Contains(messageWindow))
            {
                Debug.LogWarning($"Already regist {messageWindow.gameObject.name}/{messageWindow.GetInstanceID()}");
                return;
            }
            //Debug.Log($"Regist dialog => {messageWindow.gameObject.name}/{messageWindow.GetInstanceID()}"); 
            dialogDic[messageWindow.DialogType].List.Add(messageWindow);
        }
    }
    [Sirenix.OdinInspector.Button]
    public AdvEngine GetFirstAdvEngine(DialogType _type)
    {
        if (dialogDic.ContainsKey(_type))
        {
            var dialogs = dialogDic[_type];
            return dialogs.List.FirstOrDefault().Engine;
        }
        return null;
    }
    public void RemoveDialog(AdvUguiMessageWindow messageWindow)
    {
        if (!dialogDic.ContainsKey(messageWindow.DialogType))
        {
            Debug.LogWarning($"Don't have instance of '{messageWindow.DialogType}'.");
        }
        else
        {
            var list = dialogDic[messageWindow.DialogType];
            if (!list.List.Contains(messageWindow)) 
            {
                Debug.LogWarning($"Don;t have {messageWindow.gameObject.name}/{messageWindow.GetInstanceID()} in registed list.");
                return;
            }
            //Debug.Log($"Remove dialog => {messageWindow.gameObject.name}/{messageWindow.GetInstanceID()}");
            list.List.Remove(messageWindow);
        }
    }
    public void StopDialogsByType(DialogType _type)
    {
        if (!dialogDic.ContainsKey(_type))
        {
            Debug.LogWarning($"Don't have any dialogs is type of '{_type}'");
            return;
        }

        var list = dialogDic[_type].List;
        for (int i = list.Count-1 ; i >= 0; i--)
        {
            list[i].AdvEngineClose();
        }
    }
    public void FocusTypeCloseOther(DialogType _type)
    {
        for (int i = 0; i < dialogDic.Count; i++)
        {
            var type = dialogDic.ElementAtOrDefault(i).Key;
            if (type != _type)
            {
                var list = dialogDic.ElementAtOrDefault(i).Value.List;
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    list[j].AdvEngineClose();
                }
            }
        }
    } 
}
