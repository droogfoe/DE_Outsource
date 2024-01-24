using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class ManualSetSelectEventSystem : MonoBehaviour
{
    public EventSystem m_EventSystem;
    public Button targetButton;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetButton.Select();
        }
    }
}
