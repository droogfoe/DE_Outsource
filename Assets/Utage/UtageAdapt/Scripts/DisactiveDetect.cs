using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisactiveDetect : MonoBehaviour
{
    private void OnDisable()
    {
        Debug.Log(gameObject.name + " on disable");
    }
}
