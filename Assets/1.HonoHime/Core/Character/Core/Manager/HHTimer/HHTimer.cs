using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class HHTimer : MonoBehaviour
{

    public enum BulletTimeScale
    {
        Scale,
        UnScale,
    }
    [SerializeField]
     BulletTimeScale bulletTimeScale;
    public static BulletTimeScale SetBulletTimeScale
    {
        get
        {
            return ins.bulletTimeScale;
        }
        set
        {
            ins.bulletTimeScale = value;
        }
    }
    public static bool PauseTime
    {
        set
        {
            if (ins != null)
            {
                ins.pauseTime = value;
            }
        }
        get
        {
            return ins.pauseTime;
        }
    }
    public bool pauseTime;
    public  float timeScale = 1;
    public static float deltaTime
    {
        get
        {
         
            if (ins == null)
            {
                return 0;
            }
            else
            {
                if (ins.pauseTime) return 0;
                return Time.deltaTime* ins.timeScale;
            }
        }
    }
    public static float UnScaleDeltaTime
    {
        get
        {
            if (ins == null)
            {
                return 0;
            }
            else
            {
                return Time.unscaledDeltaTime;
            }
        }
    }
    public static float TimeSacle
    {
        set
        {
            if (ins == null)
            {
                return;
            }
            ins.timeScale = value;
        }
        get
        {
            if (ins == null)
            {
                return 1;
            }
            if (ins.pauseTime) return 0;

            return ins.timeScale;
        }
    }



    static HHTimer ins
    {
        get
        {
            return _ins;
        }
    }
    static HHTimer _ins;
  
    private void Awake()
    {
        if (_ins == null)
        {
            _ins = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {

        DOTween.ManualUpdate(deltaTime, UnScaleDeltaTime);
        if (bulletTimeScale == BulletTimeScale.Scale)
        {
            //UbhTimer.instance.bulletSpeedAmp = timeScale;
        }
        else
        {
            //UbhTimer.instance.bulletSpeedAmp = 1;
        }
    }
}
