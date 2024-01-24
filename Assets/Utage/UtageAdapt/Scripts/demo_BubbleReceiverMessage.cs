using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;


public class demo_BubbleReceiverMessage : MonoBehaviour
{
    private void OnDoCommand(AdvCommandSendMessageToSender command)
    {
        Debug.Log($"Bubble from {this.gameObject.name}");
        switch (command.MethodName)
        {
            case "PlaymakerSetInt":
                PlaymakerSetInt(command);
                break;
            default:
                break;
        }
    }
    private void PlaymakerSetInt(AdvCommandSendMessageToSender command)
    {
        Debug.Log("PlaymakerSetInt :" + command.Arg2<string>());
    }
}
