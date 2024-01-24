using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ListPopupAttribute : PropertyAttribute {
    public Type myType;
    private string propertyName = "POPUPLIST";
    public string PropertyName { get { return propertyName; } }
    public ListPopupAttribute(Type _type)
    {
        myType = _type;
    }
}
