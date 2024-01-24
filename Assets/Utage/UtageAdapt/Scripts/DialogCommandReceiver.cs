using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utage;
using UnityEditor;

public class DialogCommandReceiver : MonoBehaviour
{
    [SerializeField] DialogType dialogType;
    [SerializeField] UtageUguiTitle title;
    [SerializeField] UtageUguiMainGame mainGame;
    [SerializeField] AdvEngine advEngine;
    public AdvEngine Engine => advEngine;
    public UnityEvent OnSayEvent, OnStopEvent;

    public AdvSelectionManager SelectionManager 
    {
        get 
        {
            if (advEngine == null)
            {
                return null;
            }
            return advEngine.gameObject.GetComponent<AdvSelectionManager>();
        }
    }
    [SerializeField] AdvScenarioPlayer advScenarioPlayer;

    private GameObject registerSender; 
    private Action eTActionTemp;

    public string targetLabel;

    [Sirenix.OdinInspector.Button]
    public void StartDialog()
    {
        advEngine.StartScenarioLabel = targetLabel;
        title.OnTapStart();
    }
    public void StartDialog(string _label, GameObject _sender = null, Action _endDialogAction = null)
    {
        targetLabel = _label;
        advEngine.StartScenarioLabel = _label;
        registerSender = _sender;

        if (advEngine != null && advEngine.UiManager != null && advEngine.UiManager.GetComponent<Canvas>() != null)
        {
            advEngine.UiManager.GetComponent<Canvas>().enabled = true;
        }
        
        title.OnTapStart();
        CheckAndAddActionInDialog(_endDialogAction);
        OnSayEvent?.Invoke(); 
    }
    public void StopDialog()
    {
        advEngine.ScenarioPlayer.EndScenario();
        mainGame.Close();
        OnStopEvent?.Invoke();

        //Debug.Log("mainGame.Close()");
        //mainGame.Close();
    }
    private UnityAction<AdvScenarioPlayer> currentEndAction;
    private UnityAction<AdvScenarioPlayer> removeCurrentSubscriber;
    private void CheckAndAddActionInDialog(Action endTxtAction = null) 
    {
        if (endTxtAction != null)
        {
            eTActionTemp = endTxtAction;
            if (currentEndAction != null)
            {
                currentEndAction.Invoke(advScenarioPlayer);
                currentEndAction = null;
            }
            if (removeCurrentSubscriber != null)
            {
                advScenarioPlayer.OnEndScenario.RemoveListener(removeCurrentSubscriber);
                removeCurrentSubscriber = null;
            }
            removeCurrentSubscriber = (advScenarioPlayer) => { RemoveRegistListener(); };
            
            currentEndAction = (advScenarioPlayer) => { endTxtAction.Invoke(); };
            advScenarioPlayer.OnEndScenario.AddListener(currentEndAction);
            advScenarioPlayer.OnEndScenario.AddListener(removeCurrentSubscriber);
        }
    }
    [Sirenix.OdinInspector.Button]
    private void RemoveRegistListener()
    {
        if (currentEndAction != null)
        {
            advScenarioPlayer.OnEndScenario.RemoveListener(currentEndAction);
            currentEndAction = null;
        }
        if (removeCurrentSubscriber != null)
        {
            advScenarioPlayer.OnEndScenario.RemoveListener(removeCurrentSubscriber);
            removeCurrentSubscriber = null;
        }
    }
    private void Start()
    {
        UtageDialogCommander.Inst.RegistDialog(dialogType, this);
    }
    private void OnDestroy()
    {
        UtageDialogCommander.Inst.RemoveDialog(dialogType, this);
    }
    private void OnDoCommand(AdvCommandSendMessageToSender command)
    {
        //¹ïµù¥UGameObject send message
        if (registerSender != null)
        {
            registerSender.SendMessage("OnDoCommand", command);
        }
    }
}
