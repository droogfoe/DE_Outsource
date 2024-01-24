using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utage;

public class UtageMessageReceiver : MonoBehaviour, IAdvSaveData {
    public string SaveKey { get { return "UtageMessageReceiver"; } }
    public bool isAdOpen = false;

    public void OnClear()
    {
        this.isAdOpen = false;
    }

    public void OnRead(BinaryReader reader)
    {
        //バージョンチェック
        int version = reader.ReadInt32();
        if (version == Version)
        {
            this.isAdOpen = reader.ReadBoolean();
        }
        else
        {
            Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
        }
    }

    //バージョンチェックしたほうが安全
    const int Version = 0;
    public void OnWrite(BinaryWriter writer)
    {
        writer.Write(Version);
    }
}
