using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class Extention 
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    public static Transform FindChildInChildrens(this Transform self, string name)
    {
        int count = self.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = self.GetChild(i);
            if (child.name == name) return child;
            Transform subChild = child.FindChildInChildrens(name);
            if (subChild != null) return subChild;
        }
        return null;
    }
    public static  void GetChildRecursive(this Transform self,ref List<Transform> listOfChildren)
    {
        if (null == self)
            return ;

       

        foreach (Transform child in self)
        {
            if (null == child)
                continue;
            //child.gameobject contains the current child you can do whatever you want like add it to an array
            listOfChildren.Add(child.transform);
            GetChildRecursive(child.transform, ref listOfChildren);
        }
       
    }
    public static void RemoveNull<T>(this List<T> list)
    {
        // Find Fist Null Element in O(n)
        var count = list.Count;
        for (var i = 0; i < count; i++)
        {
            if (list[i] == null)
            {
                // Current Position
                int newCount = i++;
                // Copy non-empty elements to current position in O(n)
                for (; i < count; i++)
                {
                    if (list[i] != null)
                    {
                        list[newCount++] = list[i];
                    }
                }
                // Remove Extra Positions O(n)
                list.RemoveRange(newCount, count - newCount);
                break;
            }
        }
    }


    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

   
   
}
