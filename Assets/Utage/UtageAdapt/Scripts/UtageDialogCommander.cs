using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utage;
using UnityEngine.SceneManagement;
using EventHandler = Opsive.Shared.Events.EventHandler;

public class UtageDialogCommander : MonoBehaviour {
    private static UtageDialogCommander inst;
    public static UtageDialogCommander Inst 
    {
        get 
        {
            return inst;
        }
    }
    
    [SerializeField] Utage.CameraManager cameraManager;
    [SerializeField] DialogTypeReceiverDic receivers;
    [System.Serializable]
    private class DialogTypeReceiverDic : UnitySerializedDictionary<DialogType, List<DialogCommandReceiver>> { }

    public Action NormalNLStartCallback, SubtitleNLStartCallback, RadioNLStartCallback;
    public Action NormalNLEndCallback, SubtitleNLEndCallback, RadioNLEndCallback;
    public Action NormalNLInputTriCallback, SubtitleNLInputTriCallback, RadioNLInputTriCallback;

    private void Awake()
    {
        if (inst == null)
        {
            inst = this;
            SceneManager.sceneLoaded += this.CheckMainCameraAndInstallCamStack_SceneLoaded;
            SceneManager.sceneUnloaded += this.CloseAllWhenUnloadScene;

            this.transform.parent = null;
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this);
            Destroy(this.gameObject);
        }
        NormalNLStartCallback = null;
        SubtitleNLStartCallback = null;
        RadioNLStartCallback = null;

        NormalNLEndCallback = null;
        SubtitleNLEndCallback = null;
        RadioNLEndCallback = null;

        NormalNLInputTriCallback = null;
        SubtitleNLInputTriCallback = null;
        RadioNLInputTriCallback = null;
    }
    private void Start()
    {
        CheckMainCameraAndInstallCamStack();
    }
    private void CloseAllWhenUnloadScene(Scene _scene)
    {
        CloseAllDialog();
    }
    public void CloseAllDialog()
    {
        if (receivers == null || receivers.Count == 0)
            return;

        for (int i = 0; i < receivers.Count; i++)
        {
            var list = receivers.ElementAtOrDefault(i).Value;
            for (int j = 0; j < list.Count; j++)
            {
                list[j].StopDialog();
            }
        }
    }
    [Sirenix.OdinInspector.Button]
    public void SayDialogByType(DialogType _dialogType, string _senario,  GameObject _sender = null,  Action endDialogAction = null, bool _solo = false)
    {
        if (!receivers.ContainsKey(_dialogType))
            return;
        Action action = null;
        if (_solo || _dialogType == DialogType.Dialog)
        {
            foreach (var item in receivers.Where(x => x.Key != _dialogType).ToList())
            {
                var muteList = item.Value;
                for (int i = 0; i < muteList.Count; i++)
                {
                    muteList[i].StopDialog();
                }
            }
        }
        if (action == null)
            action = endDialogAction;
        var list = receivers[_dialogType];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].StartDialog(_senario, _sender, action);
        }
    }
    public void RegistNLCallback(DialogType _type, NLEventType _eventType, Action _action)
    {
        switch (_eventType)
        {
            // NLEventType.Start
            case NLEventType.Start:
                if (_type == DialogType.Dialog)
                    NormalNLStartCallback += _action;
                else if (_type == DialogType.Subtitle)
                    SubtitleNLStartCallback += _action;
                else if (_type == DialogType.Radio)
                    RadioNLStartCallback += _action;
                break;

            // NLEventType.End
            case NLEventType.End:
                if (_type == DialogType.Dialog)
                    NormalNLEndCallback += _action;
                else if (_type == DialogType.Subtitle)
                    SubtitleNLEndCallback += _action;
                else if (_type == DialogType.Radio)
                    RadioNLEndCallback += _action;
                break;

            // NLEventType.InputTri
            case NLEventType.InputTri:
                if (_type == DialogType.Dialog)
                    NormalNLInputTriCallback += _action;
                else if (_type == DialogType.Subtitle)
                    SubtitleNLInputTriCallback += _action;
                else if (_type == DialogType.Radio)
                    RadioNLInputTriCallback += _action;
                break;


            default:
                break;
        }
    }
    public void RemoveNLCallback(DialogType _type, NLEventType _eventType, Action _action)
    {
        switch (_eventType)
        {
            // NLEventType.Start
            case NLEventType.Start:
                if (_type == DialogType.Dialog)
                    NormalNLStartCallback -= _action;
                else if (_type == DialogType.Subtitle)
                    SubtitleNLStartCallback -= _action;
                else if (_type == DialogType.Radio)
                    RadioNLStartCallback -= _action;
                break;

            // NLEventType.End
            case NLEventType.End:
                if (_type == DialogType.Dialog)
                    NormalNLEndCallback -= _action;
                else if (_type == DialogType.Subtitle)
                    SubtitleNLEndCallback -= _action;
                else if (_type == DialogType.Radio)
                    RadioNLEndCallback -= _action;
                break;

            // NLEventType.InputTri
            case NLEventType.InputTri:
                if (_type == DialogType.Dialog)
                    NormalNLInputTriCallback -= _action;
                else if (_type == DialogType.Subtitle)
                    SubtitleNLInputTriCallback -= _action;
                else if (_type == DialogType.Radio)
                    RadioNLInputTriCallback -= _action;
                break;


            default:
                break;
        }
    }
    public void TriggerNLStartCallback(int _typeStr)
    {
       var type = (DialogType)_typeStr;
        switch (type)
        {
            case DialogType.Dialog:
                NormalNLStartCallback?.Invoke();
                break;
            case DialogType.Radio:
                RadioNLStartCallback?.Invoke();
                break;
            case DialogType.Bubble:
                break;
            case DialogType.Subtitle:
                SubtitleNLStartCallback?.Invoke();
                break;
            default:
                break;
        }
    }
    public void TriggerNLInputTriCallback(int _typeStr)
    {
        var type = (DialogType)_typeStr;
        switch (type)
        {
            case DialogType.Dialog:
                NormalNLInputTriCallback?.Invoke();
                break;
            case DialogType.Radio:
                RadioNLInputTriCallback?.Invoke();
                break;
            case DialogType.Bubble:
                break;
            case DialogType.Subtitle:
                SubtitleNLInputTriCallback?.Invoke();
                break;
            default:
                break;
        }
    }
    public void TriggerNLEndCallback(int _typeStr)
    {
        var type = (DialogType)_typeStr;
        switch (type)
        {
            case DialogType.Dialog:
                NormalNLEndCallback?.Invoke();
                break;
            case DialogType.Radio:
                RadioNLEndCallback?.Invoke();
                break;
            case DialogType.Bubble:
                break;
            case DialogType.Subtitle:
                SubtitleNLEndCallback?.Invoke();
                break;

            default:
                break;
        }
    }
    public void StartSenarioCallback(int _typeStr)
    {
        var type = (DialogType)_typeStr;
        switch (type)
        {
            case DialogType.Dialog:
                //EventHandler.ExecuteEvent(UILayerEventHandler.EVENT_DIALOGUE_NORMAL_START);
                break;
            case DialogType.Radio:
                break;
            case DialogType.Bubble:
                break;
            case DialogType.Subtitle:
                break;
            default:
                break;
        }
    }
    public void EndSenarioCallback(int _typeStr)
    {
        var type = (DialogType)_typeStr;
        switch (type)
        {
            case DialogType.Dialog:
                //EventHandler.ExecuteEvent(UILayerEventHandler.EVENT_DIALOGUE_NORMAL_END);
                break;
            case DialogType.Radio:
                break;
            case DialogType.Bubble:
                break;
            case DialogType.Subtitle:
                break;
            default:
                break;
        }
    }
    [Sirenix.OdinInspector.Button]
    public void StopDialogByType(DialogType _dialogType)
    {
        if (DialogWindowsPool.Instance == null || !receivers.ContainsKey(_dialogType))
        {
            Debug.LogWarning("Don't have active dialog.");
            return;
        }
        var list = receivers[_dialogType];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].StopDialog();
        }
        //DialogWindowsPool.Instance.StopDialogsByType(_dialogType);
    }
    public void RegistDialog(DialogType _dialogType, DialogCommandReceiver _receiver)
    {
        if (receivers == null)
        {
            receivers = new DialogTypeReceiverDic();
            //receivers = new Dictionary<DialogType, List<DialogCommandReceiver>>();
        }

        if (!receivers.ContainsKey(_dialogType))
        {
            receivers.Add(_dialogType, new List<DialogCommandReceiver>());
        }
        receivers[_dialogType].Add(_receiver);

    }
    public AdvSelectionManager MainSelectionManager
    {
        get 
        {
            if (!receivers.ContainsKey(DialogType.Dialog))
                return null;

            return receivers[DialogType.Dialog].FirstOrDefault().SelectionManager;
        }
    }
    public void RemoveDialog(DialogType _dialogType, DialogCommandReceiver _receiver)
    {
        if (receivers == null)
        {
            //Debug.LogError($"List are empty");
            return;
        }

        var list = receivers[_dialogType];
        var toRemove = from item in list
                       where item == _receiver
                       select item;

        receivers[_dialogType].Remove((DialogCommandReceiver)toRemove.FirstOrDefault());
        if (receivers[_dialogType].Count == 0)
        {
            receivers.Remove(_dialogType);
        }
    }
    public void CharacterOff()
    {
        var dialogReceiver = receivers[DialogType.Dialog];
        var mainEngine = dialogReceiver.FirstOrDefault().Engine;
        float fadeTime = mainEngine.Page.ToSkippedTime(0.2f);
        AdvGraphicGroup characterManager = mainEngine.GraphicManager.CharacterManager;
        characterManager.FadeOutAll(fadeTime);
    }

    [Sirenix.OdinInspector.Button]
    public void CheckMainCameraAndInstallCamStack()
    {
        var mainCam = Camera.main;
        //Debug.Log("CheckMainCameraAndInstallCamStack: " + mainCam);
        if (mainCam.transform.root != this.transform)
        {
            var cam = cameraManager.FindCameraRoot("3DCamera");
            cam.gameObject.SetActive(false);
        }
        else
        {
            var cam = cameraManager.FindCameraRoot("3DCamera");
            cam.gameObject.SetActive(true);
        }

        //InstallCamStacksInMainCamera();
    }
    
    private void CheckMainCameraAndInstallCamStack_SceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        //StartCoroutine(InstallCameraStackAfterMainCamInit());
    }
    IEnumerator InstallCameraStackAfterMainCamInit()
    {
        while (Camera.main == null)
        {
            yield return null;
        }
        CheckMainCameraAndInstallCamStack();
    }

    private void InstallCamStacksInMainCamera()
    {
        var urpCamData = Camera.main.transform.GetComponent<UniversalAdditionalCameraData>();
        for (int i = 0; i < cameraManager.CameraList.Count; i++)
        {
            if (!cameraManager.CameraList[i].gameObject.activeSelf) continue;
            var cam = cameraManager.CameraList[i].transform.GetChild(0).GetComponent<Camera>();

            if (!urpCamData.cameraStack.Contains(cam))
                urpCamData.cameraStack.Add(cam);
        }
    }

    public void TestEndTxtStr(string _str)
    {
        Debug.Log("Test: " + _str);
    }
    public bool TryGetParameter<T>(string _key, out T _output)
    {
        _output = default(T);
        if (!receivers.ContainsKey(DialogType.Dialog) && receivers[DialogType.Dialog].Count < 1 )
            return false;
        var param = receivers[DialogType.Dialog].FirstOrDefault().Engine.Param;
        if (param.TryGetParameter(_key, out object p))
        {
            _output = param.GetParameter<T>(_key);
            return true;
        }

        return false;
    }
    public bool TrySetParameter<T>(string _key, T _value)
    {
        if (!receivers.ContainsKey(DialogType.Dialog) && receivers[DialogType.Dialog].Count < 1)
            return false;
        var param = receivers[DialogType.Dialog].FirstOrDefault().Engine.Param;
        if (param.TryGetParameter(_key, out object p))
        {
            param.SetParameter<T>(_key, _value);
            return true;
        }
        return false;
    }
}
