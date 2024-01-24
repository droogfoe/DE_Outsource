using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

public interface ITLBindingCallback
{
    public TLClipUnityEvent OnChangeEvent { get; }
    public List<TLBindingContrlAsset> Bindings { get; }
    public void OnClipChange(TimelineClip _clip, bool _force = false);
    public void OnInspectorInit();
}

[Serializable]
public class TLClipUnityEvent : UnityEvent<TimelineClip>
{
    private List<UnityAction<TimelineClip>> actionList;
    public int ListenerCount { get => (actionList == null) ? 0 : actionList.Count; }
    new public void RemoveAllListeners()
    {
        actionList = null;
        base.RemoveAllListeners();
    }
    new public void AddListener(UnityAction<TimelineClip> call)
    {
        if (actionList == null)
            actionList = new List<UnityAction<TimelineClip>>();
        if (!actionList.Contains(call))
           actionList.Add(call);
        
        base.AddListener(call);
    }
    new public void RemoveListener(UnityAction<TimelineClip> call)
    {
        base.RemoveListener(call);

        if (actionList != null && actionList.Contains(call))
        {
            actionList.Remove(call);
            if (actionList.Count == 0)
                actionList = null;
        }
    }
}
