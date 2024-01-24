using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TestSubtitleEventBug : MonoBehaviour
{
    [Button]
    private void Test()
    {
        UtageDialogCommander.Inst.SayDialogByType(DialogType.Subtitle, "MinTest2", this.gameObject, () => 
        {
            CallbackAction();
        });
    }
    private void CallbackAction()
    {
        Debug.Log(this.gameObject.name + " CallbackAction");
    }
}
