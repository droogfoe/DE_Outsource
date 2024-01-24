using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;
using Cinemachine;

public class CinemachineTrackGetCamera : MonoBehaviour
{
    PlayableDirector playableDirector;
    // Start is called before the first frame update
    TimelineAsset timelineAsset;

    void Start() //原先寫在Awake，但不知為什麼 Get 不到了
    {
        Pitch();
    }
    [Button]
    public void Pitch()
    {
        playableDirector = GetComponent<PlayableDirector>();

        timelineAsset = (TimelineAsset)playableDirector.playableAsset;
        if (timelineAsset == null) return;

        foreach (var temp in timelineAsset.outputs)
        {
            if (temp.streamName == "Cinemachine Track" || 
                temp.outputTargetType == typeof(CinemachineBrain))
            {
                playableDirector.SetGenericBinding(temp.sourceObject, Camera.main.GetComponent<Cinemachine.CinemachineBrain>());
            }
        }
    }
}
