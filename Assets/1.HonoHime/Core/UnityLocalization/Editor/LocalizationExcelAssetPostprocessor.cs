using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class LocalizationExcelAssetPostprocessor : AssetPostprocessor
{
    public static bool handledThisFrame;

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if(CustomLocalizationTableIOEditor.ins == null)
        {
            CustomLocalizationTableIOEditor.ins = new CustomLocalizationTableIOEditor();
            CustomLocalizationTableIOEditor.ins.Init();
        }

        foreach (string assetPath in importedAssets)
        {
            if (assetPath.Equals(CustomLocalizationTableIOEditor.ins.filePath))
            {
                HandleTargetExcelImported();
            }

            break;
        }
    }

    private static void HandleTargetExcelImported()
    {
        if (handledThisFrame) { 
            Debug.Log("handledThisFrame return");
            return;
        }

        handledThisFrame = true;

        if (CustomLocalizationTableIOEditor.ins == null)
        {
            CustomLocalizationTableIOEditor.ins = new CustomLocalizationTableIOEditor();
            CustomLocalizationTableIOEditor.ins.Init();
        }

        Debug.Log("HandleTargetExcelImported : Import");

        CustomLocalizationTableIOEditor.ins.Import();
        handledThisFrame = false;
    }
}
