using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;

public class ListToPopupAttribute : PropertyAttribute
{
    public Type myType;
    public string propertyName;

    public ListToPopupAttribute(Type _myType, string _propertyName)
    {
        myType = _myType;
        propertyName = _propertyName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ListToPopupAttribute))]
public class ListToPopupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ListToPopupAttribute atb = attribute as ListToPopupAttribute;
        List<string> stringList = null;

        if(atb.myType.GetField(atb.propertyName) != null)
        {
            var values = atb.myType.GetField(atb.propertyName).GetValue(atb.myType);
            stringList = values as List<string>;

        }

        if(stringList != null && stringList.Count != 0)
        {
            if (stringList.Contains(property.stringValue))
            {
                int selectIndex = Mathf.Max(stringList.IndexOf(property.stringValue), 0);
                selectIndex = EditorGUI.Popup(position, property.name, selectIndex, stringList.ToArray());
                //Debug.Log(property.displayName);
                property.stringValue = stringList[selectIndex];
            }
            else
            {
                if(property.stringValue == string.Empty || property.stringValue == "")
                {
                    property.stringValue = stringList[0];
                    return;
                }
                EditorGUI.PropertyField(position, property, label);
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif
