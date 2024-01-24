using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Localization.Settings;
using Opsive.Shared.Events;


public class LanguageManagerEvent
{
    public static string GLOBAL_SETLANGUAGE_EVENT = "LanguageManager_SetLanguage";
}

public class LanguageManager : MonoBehaviour
{
    public enum Language 
    {
        CH,
        EN,
        JP,
    }

    public static Language CurrentLanguage
    {
        get
        {
            if (ins == null)
                ins = (LanguageManager)FindObjectOfType(typeof(LanguageManager));

            return ins.currentLanguage;
        }
        set
        {
            ins.currentLanguage = value;
        }
    }

    public static LanguageManager ins;

    [SerializeField]
    [OnValueChanged("EditorSetLanguage")]
    public Language currentLanguage;

    
    private void Start()
    {
        if (ins == null)
        {
            ins = this;
            transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetLanague(currentLanguage);
    }


    void EditorSetLanguage()
    {
        SetLanague(currentLanguage);
    }


    public static void SetLanague(Language language) 
    {
        if(ins == null)
            ins = (LanguageManager)FindObjectOfType(typeof(LanguageManager));

        ins.currentLanguage = language;

        CurrentLanguage = language;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(language.ToString());
        EventHandler.ExecuteEvent<string>(LanguageManagerEvent.GLOBAL_SETLANGUAGE_EVENT, language.ToString());

        Debug.Log("LanguageManager SetLanague : " + language);
    }
   
}
