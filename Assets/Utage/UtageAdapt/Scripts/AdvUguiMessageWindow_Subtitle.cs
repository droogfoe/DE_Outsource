using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Utage {
    public class AdvUguiMessageWindow_Subtitle : AdvUguiMessageWindow {
        [SerializeField] Image bg;
        [SerializeField] SubtitleTextLine[] m_LineText;
        [SerializeField] AnimationCurve fadeCurve;
        [SerializeField] float textLifeTime = 4.25f;
        [SerializeField] AdvPage advPage;

        Queue<SubtitleMesWindow> textQueue;
        public class SubtitleMesWindow {
            public float LifeTime { get; private set; }
            public string OriginalText { get; private set; }
            public string NameText { get; private set; }
            public int LengthOfView { get; private set; }
            public float Alpha { get; set; }
            public AnimationCurve fadeCurve;
            public bool VisibleFlag = false;

            private float m_Duration;
            private Action m_finishAction;

            public SubtitleMesWindow(AnimationCurve _curve, float _lifeTime, string _name, string _text, int _lengthOfView, Action _finishAction = null)
            {
                this.LifeTime = _lifeTime;
                this.NameText = _name;
                this.OriginalText = _text;
                this.LengthOfView = _lengthOfView;
                Alpha = 1.0f;
                m_finishAction = _finishAction;
                fadeCurve = _curve;
                VisibleFlag = false;
            }
            public void LifeTimeUpdate()
            {
                if (!VisibleFlag)
                {
                    Alpha = 0;
                    return;
                }
                m_Duration += Time.deltaTime;
                Alpha = fadeCurve.Evaluate(m_Duration / LifeTime);
                if (m_Duration >= LifeTime)
                {
                    m_finishAction?.Invoke();
                }
            }
        }
        [System.Serializable]
        public struct SubtitleTextLine {
            public CanvasGroup canvasGroup;
            public UguiNovelText text;
            public UguiNovelText nameTxt;
            public HorizontalLayoutGroup horizontal;
        }
        public override void OnInit(AdvMessageWindowManager windowManager)
        {
            base.OnInit(windowManager);
            textQueue = new Queue<SubtitleMesWindow>();
        }
        protected override void LateUpdate()
        {
            SubTitleCycleUpdate();
            if (Engine.UiManager.Status == AdvUiManager.UiStatus.Default)
            {
                //rootChildren.SetActive(Engine.UiManager.IsShowingMessageWindow);
                if (Engine.UiManager.IsShowingMessageWindow)
                {
                    //ウィンドのアルファ値反映
                    if (translateMessageWindowRoot != null)
                    {
                        translateMessageWindowRoot.alpha = Engine.Config.MessageWindowAlpha;
                    }
                }
                else
                {
                    if (translateMessageWindowRoot != null)
                    {
                        translateMessageWindowRoot.alpha = 0;
                    }
                }
            }
            else
            {

            }
        }

        protected override void Clear()
        {
            if (iconWaitInput) iconWaitInput.SetActive(false);
            if (iconBrPage) iconBrPage.SetActive(false);

            if (m_LineText == null || m_LineText.Length <= 1)
                return;
        }
        [Sirenix.OdinInspector.Button]
        public void ShutDownVisible()
        {
            foreach (var t in textQueue)
            {
                t.VisibleFlag = false;
                t.Alpha = 0;
            }

            if (translateMessageWindowRoot != null)
                translateMessageWindowRoot.alpha = 0;

            SetBG(false);
        }
        protected override void UpdateCurrent()
        {
            if (!IsCurrent) return;

            if (Engine.UiManager.Status == AdvUiManager.UiStatus.Default)
            {
                if (Engine.UiManager.IsShowingMessageWindow)
                {
                    text.LengthOfView = (typingEffect) ? Engine.Page.CurrentTextLength : -1;

                    for (int i = 1; i < m_LineText.Length; i++)
                    {
                        m_LineText[i].text.LengthOfView = -1;
                    }
                }
                //LinkIcon();
            }
        }
        public void SetBG(bool _flag)
        {
            if (_flag)
            {
                bg.DOColor(new Color(bg.color.r, bg.color.g, bg.color.b, 1), 0.3f);
            }
            else
            {
                bg.DOColor(new Color(bg.color.r, bg.color.g, bg.color.b, 0), 0.3f);
            }
        }
        public void TurnOffBGWithDelay(float _delay)
        {
            bg.DOColor(new Color(bg.color.r, bg.color.g, bg.color.b, 0), 0.3f)
                .SetDelay(_delay);

        }
        private void SubTitleCycleUpdate()
        {
            if (textQueue.Count == 0)
                return;

            for (int i = 0; i < textQueue.Count; i++)
            {
                var textQueueElement = textQueue.ElementAtOrDefault(textQueue.Count - 1 - i);
                textQueueElement.LifeTimeUpdate();
                m_LineText[i].canvasGroup.alpha = textQueueElement.Alpha;
            }
        }
        public override void OnTextChanged(AdvMessageWindow window)
        {
            if (DialogWindowsPool.Instance.SingleDialogOnly)
            {
                AdvEngineClose();
                return;
            }

            if (textQueue.Count >= m_LineText.Length)
            {
                textQueue.Dequeue();
            }

            var page = GetComponentInParent<AdvPage>();
            float _tDuration = textLifeTime;
            if (advPage != null && advPage.CurrentTextDuration > 0)
                _tDuration = advPage.CurrentTextDuration;

            //Debug.LogError("_tDuration: " + _tDuration);
            textQueue.Enqueue(new SubtitleMesWindow(fadeCurve, _tDuration, window.NameText, window.Text.OriginalText, window.TextLength, ()=> 
            {
                textQueue.Dequeue();
            }));

            UpdateTextGroupContent();
            OnPostChangeText.Invoke(window);
            Invoke("UpdateLayoutGroup", 0.1f);
        }
        private void UpdateLayoutGroup()
        {
            for (int i = 0; i < m_LineText.Length; i++)
            {
                m_LineText[i].horizontal.CalculateLayoutInputHorizontal();
                m_LineText[i].horizontal.SetLayoutHorizontal();
            }
            for (int i = 0; i < textQueue.Count; i++)
            {
                textQueue.ElementAtOrDefault(i).VisibleFlag = true;
            }
        }
        //public void TextInQueueStartFade(AdvCommand _command)
        //{
        //    //if (!_command)
        //    //    return;

        //    for (int i = 0; i < textQueue.Count; i++)
        //    {
        //        var current = textQueue.ElementAtOrDefault(i);
        //        if (current.FadingFlag == false)
        //        {
        //            current.FadingFlag = true;
        //            break;
        //        }
        //    }
        //}
        private void UpdateTextGroupContent()
        {
            for (int i = 0; i < textQueue.Count; i++)
            {
                if (m_LineText[i].text)
                {
                    textQueue.ElementAtOrDefault(i).VisibleFlag = false;

                    var indexWindow = textQueue.ElementAtOrDefault(textQueue.Count - 1 - i);
                    m_LineText[i].text.text = "";
                    m_LineText[i].text.text = indexWindow.OriginalText;
                    m_LineText[i].text.LengthOfView = (typingEffect) ? indexWindow.LengthOfView : -1;
                    m_LineText[i].nameTxt.text = "";
                    m_LineText[i].nameTxt.text = indexWindow.NameText;
                    m_LineText[i].nameTxt.LengthOfView = -1;

                    switch (readColorMode)
                    {
                        case ReadColorMode.Change:
                            m_LineText[i].text.color = Engine.Page.CheckReadPage() ? readColor : defaultTextColor;
                            m_LineText[i].nameTxt.color = Engine.Page.CheckReadPage() ? readColor : defaultTextColor;
                            nameText.color = Engine.Page.CheckReadPage() ? readColor : defaultNameTextColor;
                            break;
                        case ReadColorMode.ChangeIgnoreNameText:
                            m_LineText[i].text.color = Engine.Page.CheckReadPage() ? readColor : defaultTextColor;
                            m_LineText[i].nameTxt.color = Engine.Page.CheckReadPage() ? readColor : defaultTextColor;
                            break;
                        case ReadColorMode.None:
                        default:
                            break;
                    }
                }
            }
        }
        public override string StartCheckCaracterCount()
        {
            return base.StartCheckCaracterCount();
        }
        public override bool TryCheckCaracterCount(string text, out int count, out string errorString)
        {
            return base.TryCheckCaracterCount(text, out count, out errorString);
        }
        public override void EndCheckCaracterCount(string text)
        {
            base.EndCheckCaracterCount(text);
        }
    }
}