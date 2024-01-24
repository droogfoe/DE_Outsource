using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class UtageTLCutSceneSkipper : MonoBehaviour
{
    [SerializeField] GameObject skipBtn;
    [SerializeField] Image skipPressBar;
    [Range(0, 1)] [SerializeField] float riseSpd = 0.75f;
    [OnValueChanged("UpdateBarValue")]
    private float barValue = 0;
    [SerializeField] float fadeTimeLimit = 4.5f;
    public UnityEvent OnCompleted;

    [HideInInspector][SerializeField] bool isRunning = false;
    private void Start()
    {
        Close();
    }
    [Button]
    public void Open(bool force = false) 
    {
        if (isRunning && !force)
            return;

        barValue = 0;
        isRunning = true;
        skipBtn.SetActive(true);
        StartCoroutine(RunningProcess());
    }
    [Button]
    public void Restart()
    {
        Close();
        Open();
    }
    public void Close(bool force = false)
    {
        if (!isRunning && !force)
            return;

        barValue = 0;
        isRunning = false;
        skipBtn.SetActive(false);
        StopAllCoroutines();
    }
    private void Complete()
    {
        OnCompleted?.Invoke();
        Close();
    }
    private void UpdateBarValue()
    {
        barValue = Mathf.Clamp01(barValue);
        skipPressBar.fillAmount = barValue;
    }
    IEnumerator RunningProcess()
    {
        float coolDownT = 0;
        while (coolDownT < fadeTimeLimit)
        {
            if (Input.GetKey(KeyCode.E))
            {
                coolDownT = 0;
                barValue += Time.deltaTime * riseSpd;
                barValue = Mathf.Clamp01(barValue);
                if (barValue >= 1)Complete();
            }
            else
            {
                coolDownT += Time.deltaTime;
                barValue -= Time.deltaTime * riseSpd * 0.5f;
                barValue = Mathf.Clamp01(barValue);
            }

            skipPressBar.fillAmount = barValue;
            yield return null;
        }

        Close();
    }
}
