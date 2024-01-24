using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Opsive.Shared.Events;


public class EventHandlerActivator : MonoBehaviour
{
    public string eventName;
    public string arg1;

    [Button]
    public void Test()
    {
        EventHandler.ExecuteEvent(eventName, arg1);
    }
}
