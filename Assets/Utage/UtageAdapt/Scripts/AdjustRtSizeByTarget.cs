using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

[ExecuteAlways]
public class AdjustRtSizeByTarget : MonoBehaviour
{
    public RectTransform targetRt;
    public Vector2 mergin;
    public bool followH, followW;
    [SerializeField] RectTransform rt;
    public bool update = true;

    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!update)
            return;

        Resize();
    }
    public void Resize()
    {
        Vector2 vector2 = rt.sizeDelta;

        if (followW)
        {
            vector2.x = targetRt.sizeDelta.x + mergin.x;
        }
        if (followH)
        {
            vector2.y = targetRt.sizeDelta.y + mergin.y;
        }
        rt.sizeDelta = vector2;
    }
}
