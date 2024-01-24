using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

[System.Serializable]
public class BubbleMessageCommand {
    public Transform target;
    public string scenario;
}

public class UtageBubblesFactory : MonoBehaviour
{
    private static UtageBubblesFactory instance;
    public static UtageBubblesFactory Instance 
    {
        get
        {
            return instance;
        }
    }
    [SerializeField] BubbleAdvPropertyRegister bubbleSetPrefab;
    [SerializeField] SoundManager advSoundManager;
    [SerializeField] Camera eventCamera;
    [SerializeField]
    private List<BubbleMessageCommand> messageCommands;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public BubbleAdvPropertyRegister SpawnBubble(Transform _target, string _senario)
    {
        var bubble = Instantiate(bubbleSetPrefab, transform);
        bubble.SetData(advSoundManager, eventCamera, _target, _senario);
        
        if (messageCommands == null)
        {
            messageCommands = new List<BubbleMessageCommand>();
        }
        var bubbleMessageCommand= new BubbleMessageCommand();
        bubbleMessageCommand.target = _target;
        bubbleMessageCommand.scenario = _senario;
        messageCommands.Add(bubbleMessageCommand);
        
        return bubble;
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
