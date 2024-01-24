using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtageExtensions;


public class TextInfoObserver : MonoBehaviour
{
    [SerializeField]
    protected Text text;
    public float preferredHeight, preferredWidth;

    // Update is called once per frame
    void Update()
    {
        preferredWidth = text.preferredHeight;
    }
}
