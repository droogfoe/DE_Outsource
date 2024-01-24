using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utage {
    public class AdvUguiMessageWindow_Bubble : AdvUguiMessageWindow {
        [Space(10)]
        [Header("[ Bubble Setting ]")]
        [SerializeField] Transform playerTrans;
        [SerializeField] Transform followTrans;
        [SerializeField] Vector3 offset;
        [SerializeField] bool ignoreEndSignal;
        private Camera registCam;
        private RectTransform rect;
        private void Start()
        {
            rect = GetComponent<RectTransform>();
            LocateHoverBubble();
        }

        public void SetTarget(Transform _target)
        {
            followTrans = _target;
        }
        [Sirenix.OdinInspector.Button("OnTapCloseWindow")]
        public override void OnTapCloseWindow()
        {
            base.OnTapCloseWindow();
        }

        protected override void LateUpdate()
        {
            if (Camera.main == null)
                return;
            
            registCam = Camera.main;
            playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

            var faceDot = Vector3.Dot(registCam.transform.forward, Vector3.Normalize(followTrans.position - playerTrans.position));
            bool faceViewFlag = (faceDot > 0) ? true : false;

            if (Engine.UiManager.Status == AdvUiManager.UiStatus.Default)
            {
                rootChildren.SetActive(Engine.UiManager.IsShowingMessageWindow & faceViewFlag);
                if (Engine.UiManager.IsShowingMessageWindow)
                {
                    if (translateMessageWindowRoot != null)
                    {
                        translateMessageWindowRoot.alpha = Engine.Config.MessageWindowAlpha;
                    }
                }
            }

            LocateHoverBubble();
            UpdateCurrent();
        }
        private void LocateHoverBubble()
        {
            if (registCam == null)
                return;
            
            var characterPosition = registCam.WorldToScreenPoint(followTrans.position);
            characterPosition += offset;
            rect.position = characterPosition;
        }
        public override void AdvEngineClose()
        {
            if (ignoreEndSignal)
                return;

            base.AdvEngineClose();
        }
    }
}