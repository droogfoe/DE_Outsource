using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;
using System.IO;

[CreateAssetMenu(fileName = "CharactersBoard", menuName = "Utage/Custom/CharactersBoard", order = 1)]
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class UtageCharacterRegisterBoard : SerializedScriptableObject {
    private static UtageCharacterRegisterBoard inst;
    public static UtageCharacterRegisterBoard Inst => inst;

    public static List<object> POPUPLIST = new List<object>();

    [ListPopup(typeof(UtageCharacterRegisterBoard))]
    public string Popup;
    public TextAsset json;
    [ReadOnly]
    public string path;
    private static string PATH;

//#if UNITY_EDITOR
    [SerializeField]
    public CharactersDictionaryClass CharacterDic;
//#endif
    public static string CHARACTER_DATA;


#if UNITY_EDITOR
    static UtageCharacterRegisterBoard()
    {
        EditorApplication.update += OnInit;
        EditorApplication.wantsToQuit += SaveOnQuit;
    }
    static void OnInit()
    {
        EditorApplication.update -= OnInit;
        string[] guids = AssetDatabase.FindAssets("CharactersBoard");
        string assetGUID = guids.FirstOrDefault();
        if (assetGUID != null)
        {
            string p = AssetDatabase.GUIDToAssetPath(assetGUID);
            inst = AssetDatabase.LoadAssetAtPath<UtageCharacterRegisterBoard>(p);
            inst.InitLoad();
        }
        else
        {
            Debug.Log("assetGUID is null");
        }
    }

    static bool SaveOnQuit()
    {
        if (UtageCharacterRegisterBoard.CHARACTER_DATA == null || UtageCharacterRegisterBoard.CHARACTER_DATA == "")
        {
            Debug.Log("CHARACTERDIC null");
            return true;
        }
        else
        {
            Debug.Log("CHARACTERDIC: " + UtageCharacterRegisterBoard.CHARACTER_DATA);
            File.SetAttributes(PATH, FileAttributes.Normal);
            File.WriteAllText(PATH, CHARACTER_DATA);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
    }
    private void OnValidate()
    {
        if (CharacterDic != null && CharacterDic.Characters.Count > 0)
        {
            CharacterDic.BuildPopupList();
            POPUPLIST = CharacterDic.Characters.Keys.Cast<object>().ToList();
            CHARACTER_DATA = JsonUtility.ToJson(this.CharacterDic, true);
        }
        else
        {
            POPUPLIST = new List<object> { "(Empty)" };
            CHARACTER_DATA = "";
        }
    }
    protected override void OnAfterDeserialize()
    {
        
    }
    protected override void OnBeforeSerialize()
    {

    }
    [Button]
    private bool Save()
    {
        GetInstallTextAsset();

        string str = JsonUtility.ToJson(this.CharacterDic, true);
        CHARACTER_DATA = JsonUtility.ToJson(this.CharacterDic, true);
        File.SetAttributes(path, FileAttributes.Normal);
        File.WriteAllText(path, str);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return true;
    }
    private void InitLoad()
    {
        Load();
    }
    [Button]
    private void Load()
    {
        GetInstallTextAsset();

        StreamReader reader = new StreamReader(path);
        string str = reader.ReadToEnd();

        if (this.CharacterDic == null)
        {
            this.CharacterDic = new CharactersDictionaryClass();
            this.CharacterDic.Characters = new UtageCustomCharacters();
        }
        JsonUtility.FromJsonOverwrite(str, this.CharacterDic);
        UtageCharacterRegisterBoard.CHARACTER_DATA = str;
        reader.Close();

        this.CharacterDic.BuildPopupList();
        POPUPLIST = this.CharacterDic.Characters.Keys.Cast<object>().ToList();
    }
    //[Button]
    private void GetInstallTextAsset()
    {
        path = this.SaveDataPath;
        PATH = this.SaveDataPath;

        if (File.Exists(path))
        {
            //Debug.Log("File exist");
            this.json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
        else
        {
            //Debug.Log($"Create new json file at {path}");
            string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(path);
            string str = "";
            if (this.CharacterDic != null && this.CharacterDic.Characters != null && this.CharacterDic.Characters.Count > 0)
            {
                str = JsonUtility.ToJson(this.CharacterDic, true);
            }
            File.WriteAllText(uniqueFileName, str);
            AssetDatabase.ImportAsset(uniqueFileName);
            AssetDatabase.Refresh();
            this.json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }

        AssetDatabase.SaveAssets();
    }
    private string SaveDataPath
    {
        get
        {
            string pathTmp = AssetDatabase.GetAssetPath(this);
            if (pathTmp == "")
            {
                string[] guids = AssetDatabase.FindAssets("CharactersBoard");
                string assetGUID = guids.FirstOrDefault();
                pathTmp = AssetDatabase.GUIDToAssetPath(assetGUID);
            }
            string scoNameWithExt = this.name + Path.GetExtension(pathTmp);
            //Debug.Log(pathTmp.Replace(scoNameWithExt, "") + this.name + ".txt");

            return pathTmp.Replace(scoNameWithExt, "") + this.name + ".txt";
        }
    }
#endif
    public string[] IDs
    {
        get
        {
            if (this == null || this.CharacterDic == null || this.CharacterDic.Characters == null || this.CharacterDic.Characters.Count == 0)
            {
                return null;
            }
            else
            {
                return this.CharacterDic.Characters.Select(x => x.Key).ToArray();
            }
        }
    }
}

//#if UNITY_EDITOR

[System.Serializable]
public class UtageCustomCharacters : UnitySerializedDictionary<string, CharacterSetting> { }
[System.Serializable]
public class CharacterSetting
{
    public LocalizeName[] localizes;

    [System.Serializable]
    public struct LocalizeName
    {
        public LanguageManager.Language language;
        public string name;
    }
}
[System.Serializable]
public class CharactersDictionaryClass {
    [SerializeField]
    public UtageCustomCharacters Characters;
#if UNITY_EDITOR
    [OnCollectionChanged("Before", "After")]
    public void Before(CollectionChangeInfo info, object value)
    {
        //Debug.Log("Received callback BEFORE CHANGE with the following info: " + info + ", and the following collection instance: " + value);
    }

    public void After(CollectionChangeInfo info, object value)
    {
        BuildPopupListInternal();
    }
#endif
    private void BuildPopupListInternal()
    {
        if (Characters != null && Characters.Count != 0)
        {
            var result = Characters.OrderBy(x => x.Key);
            Characters = new UtageCustomCharacters();
            foreach (var item in result)
            {
                Characters.Add(item.Key, item.Value);
            }
        }
    }
    public void BuildPopupList()
    {
        BuildPopupListInternal();
    }
    public bool HasCharacter(string _name)
    {
        if (Characters == null || Characters.Count == 0)
            return false;

        return Characters.Any(c => c.Value.localizes.Any(v => v.name == _name));
    }
    public string GetName(string _name)
    {
        string result = "";
        if (Characters == null || Characters.Count == 0)
            return result;
        if (!Characters.Any(c => c.Value.localizes.Any(v => v.name == _name)))
            return result;


        var character = Characters.Where(c => c.Value.localizes.Any(v => v.name== _name)).FirstOrDefault();
        var curLanguage = LanguageManager.CurrentLanguage;
        switch (curLanguage)
        {
            case LanguageManager.Language.CH:
                result = character.Value.localizes.Where(x => x.language == LanguageManager.Language.CH).FirstOrDefault().name;
                break;
            case LanguageManager.Language.EN:
                result = character.Value.localizes.Where(x => x.language == LanguageManager.Language.EN).FirstOrDefault().name;
                break;
            case LanguageManager.Language.JP:
                result = character.Value.localizes.Where(x => x.language == LanguageManager.Language.JP).FirstOrDefault().name;
                break;
            default:
                result = character.Value.localizes.Where(x => x.language == LanguageManager.Language.CH).FirstOrDefault().name;
                break;
        }
        return result;
    }
    public string GetGenericIDFromName(string _name)
    {
        if (!HasCharacter(_name))
            return "";
        return Characters.Where(c => c.Value.localizes.Any(v => v.name == _name)).FirstOrDefault().Key;
    }
}
//#endif
