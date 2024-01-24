using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opsive.Shared.Events;


public class demo_EventHandlerListenerMono : MonoBehaviour
{
    public string eventName;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"RegisterEvent {eventName}");
        EventHandler.RegisterEvent<string>(eventName, CallBack);
    }

    private void CallBack(string str)
    {
        Debug.Log(GetType() + " get callback info: " + str);
    }
}
