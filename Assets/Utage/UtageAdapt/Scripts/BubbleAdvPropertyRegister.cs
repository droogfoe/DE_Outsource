using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;

public class BubbleAdvPropertyRegister : MonoBehaviour
{
    [Header("[ 需要儲存的Property ]")]
    //需要儲存的Property
    [SerializeField] SoundManager soundManager;
    [SerializeField] Utage.CameraManager centerCamManager;
    [SerializeField] Camera eventCamera;
    [SerializeField] Transform target;
    [SerializeField] string scenario;

    [Space(10)]
    [Header("[ 需要注入Property的對象 ]")]
    //需要注入Property的對象
    [SerializeField] AdvEngine advEngine;
    [SerializeField] Canvas UI;
    [SerializeField] Canvas Canvas_AdvUI_Bubble;
    [SerializeField] AdvUguiMessageWindow_Bubble window_Bubble;
    [SerializeField] DialogCommandReceiver dialogCommandReceiver;
    public DialogCommandReceiver DialogCommandReceiver 
    {
        get
        {
            return dialogCommandReceiver;
        }
    }

    private void Start()
    {
        if (soundManager == null)
            soundManager = SoundManager.GetInstance();
        if (eventCamera == null)
            eventCamera = GameObject.FindGameObjectWithTag("DialogCamera").GetComponent<Camera>();
        if (centerCamManager == null)
            centerCamManager = eventCamera.GetComponentInParent<Utage.CameraManager>();

        if (target == null)
        {
            if (transform.parent != null)
            {
                target = transform.parent;
            }
            else
            {
                target = transform;
            }
        }

        SendData();
    }

    public void SetData(SoundManager _manager, Camera _camera, Transform _target, string _scenario)
    {
        this.soundManager = _manager;
        this.eventCamera = _camera;
        this.target = _target;
        this.scenario = _scenario;

        SendData();
    }
    private void SendData()
    {
        //advEngine.SoundManager = soundManager;
        advEngine.SetCameraManager(centerCamManager);
        UI.worldCamera = eventCamera;
        Canvas_AdvUI_Bubble.worldCamera = eventCamera;
        window_Bubble.SetTarget(target);
        dialogCommandReceiver.targetLabel = scenario;
    }
}
