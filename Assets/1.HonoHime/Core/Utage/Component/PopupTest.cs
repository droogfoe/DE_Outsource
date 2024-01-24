using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PopupTest : MonoBehaviour
{
    public static List<object> POPUPLIST;
    [ListPopup(typeof(PopupTest))]
    public string Popup;
    public List<string> stringList;
    public List<float> floatList;

    [ContextMenu("Create String List")]
    public void CreateStringList()
    {
        POPUPLIST = stringList.Cast<object>().ToList();
    }

    [ContextMenu("Create Float List")]
    public void CreateFloatList()
    {
        POPUPLIST = floatList.Cast<object>().ToList();
    }
}
