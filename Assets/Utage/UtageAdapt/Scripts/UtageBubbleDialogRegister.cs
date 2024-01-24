using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtageBubbleDialogRegister : MonoBehaviour
{
    [SerializeField] Transform targetTrans;
    [SerializeField] string launchSenario = "Test2";

    private BubbleAdvPropertyRegister bubble;
    private void Start()
    {
        if (targetTrans == null)
            targetTrans = transform.parent;

        if (UtageBubblesFactory.Instance)
            bubble = UtageBubblesFactory.Instance.SpawnBubble(targetTrans, launchSenario);
        else
            Debug.LogError("Don't have Utage dialog system in scene!");
    }
    [Sirenix.OdinInspector.Button]
    public void StartDialog(string targetSenario, Action endDialogAction = null)
    {
        if (bubble == null)
            return;

        bubble.DialogCommandReceiver.StartDialog(targetSenario, targetTrans.gameObject, endDialogAction);
    }
    public void StartDialog(Action endDialogAction = null)
    {
        if (bubble == null)
            return;

        bubble.DialogCommandReceiver.StartDialog(launchSenario, targetTrans.gameObject, endDialogAction);
    }
}
