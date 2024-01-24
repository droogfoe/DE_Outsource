using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContentLimitFilter : MonoBehaviour, IPointerClickHandler
{
    public bool update = true;
    public Vector2 Limit;
    public Vector2 Mergin = new Vector2(0, 25);
    public bool Set_HLimit, Set_VLimit;
    public bool MessageMode;
    public Action textUpdateAction;

    private bool expand;
    [SerializeField] private RectTransform rt;
    [SerializeField] private Text text;

    public void OnPointerClick(PointerEventData eventData)
    {
        expand = true;
    }
    private void Awake()
    {
        if (text == null)
            text = GetComponent<Text>();
        if (rt == null)
            rt = GetComponent<RectTransform>();
        //DebugGetPreferSize();
        //SizeAdjust();
    }

    // Update is called once per frame
    void Update()
    {
        if (!update)
            return;

        SizeAdjust();
    }
    public void SizeAdjust()
    {
        rt.sizeDelta = GetAdjustSize();
    }
    public Vector2 GetAdjustSize()
    {
        Vector2 resizeValue = new Vector2(rt.sizeDelta.x, text.preferredHeight + Mergin.y);
        if (MessageMode)
        {
            resizeValue = new Vector2(((text.preferredWidth < Limit.x) ? text.preferredWidth : Limit.x), text.preferredHeight + Mergin.y);
            //resizeValue = new Vector2( text.preferredWidth, text.preferredHeight + Mergin.y);
        }

        if (!expand)
        {
            if (Set_HLimit && text.preferredWidth > Limit.x)
            {
                resizeValue.x = Limit.x;
            }
            if (Set_VLimit && text.preferredHeight > Limit.y)
            {
                resizeValue.y = Limit.y;
            }
        }

        return resizeValue;
    }
    public void DebugGetPreferSize()
    {
        Debug.Log("GetAdjustSize: " + GetAdjustSize());
    }
}
