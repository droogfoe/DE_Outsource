using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;

public class demo_GraphicManagerRegistHandler : MonoBehaviour
{
    [SerializeField] AdvGraphicManager graphicManager;
    [SerializeField] Image img;

    public void DrawGraphic()
    {
        var graphics = graphicManager.CharacterManager.AllGraphics();
        for (int i = 0; i < graphics.Count; i++)
        {
            Debug.Log(graphics[i].gameObject.name);
            var new_img = graphics[i].gameObject.AddComponent<Image>(img);
            new_img.enabled = true;
            var mask= graphics[i].gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
        }
    }
}
