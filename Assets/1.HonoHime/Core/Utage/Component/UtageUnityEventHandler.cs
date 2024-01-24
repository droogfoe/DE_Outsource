using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Utage;

public class UtageUnityEventHandler : MonoBehaviour
{
    [System.Serializable]
    public class EventDict : UnitySerializedDictionary<string, UnityEvent> { }
    public EventDict unityEventDict;

    private void OnDoCommand(AdvCommandSendMessageToSender command)
    {
        //Debug.Log($"Bubble from {this.gameObject.name}");
        //Debug.Log("OnDoCommand");
        switch (command.MethodName)
        {
            case "InvokeUnityEvent":
                InvokeUnityEvent(command);
                break;

            default:
                break;
        }
    }

    private void InvokeUnityEvent(AdvCommandSendMessageToSender command)
    {
        string key = command.Arg2<string>();

        if (unityEventDict.ContainsKey(key))
        {
            unityEventDict[key].Invoke();
        }
    }
}
