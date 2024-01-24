using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

public class demo_BubblesManager : MonoBehaviour
{
    [SerializeField] BubbleAdvPropertyRegister bubbleSetPrefab;
    [SerializeField] SoundManager advSoundManager;
    [SerializeField] Camera eventCamera;
    [SerializeField]
    List<BubbleMessageCommand> messageCommands;

    [System.Serializable]
    public struct BubbleMessageCommand {
        public Transform target;
        public string scenario;
    }

    [Sirenix.OdinInspector.Button("¥Í¦¨´ú¸ÕBubble")]
    public void TestSpawnBubbleDialog()
    {
        for (int i = 0; i < messageCommands.Count; i++)
        {
            var register = Instantiate(bubbleSetPrefab, transform);
            register.SetData(advSoundManager, eventCamera, messageCommands[i].target, messageCommands[i].scenario);
        }
    }
}
