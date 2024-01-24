using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class demo_CubeRotater : MonoBehaviour
{
    [SerializeField] float rotSpd = 10;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Vector3.up, Time.deltaTime * rotSpd);
    }
}
