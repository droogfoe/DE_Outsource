using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utage;

[RequireComponent(typeof(UtageDialogCommander))]
[RequireComponent(typeof(UtageCharaterCommandHandler))]
public class UtageCharacterSpeakingHandler : MonoBehaviour
{
    [SerializeField] UtageCharacterRegisterBoard characterBoard;
    [SerializeField] UtageCharaterCommandHandler utageCharaterCommand;
    [SerializeField] AdvPage advPager;
    private void Reset()
    {
        utageCharaterCommand = GetComponent<UtageCharaterCommandHandler>();
    }
    private void Start()
    {
        advPager.OnBeginText.AddListener(StartSpeak);
        advPager.OnEndText.AddListener(StopSpeak);
        SceneManager.sceneLoaded += SceneChangeClearCharacterData;
    }

    private void SceneChangeClearCharacterData(Scene arg0, LoadSceneMode arg1)
    {
        speakingCharacter = new List<UtageCharacter>();
    }

    private List<UtageCharacter> speakingCharacter;

    private void StartSpeak(AdvPage _page)
    {
        if (_page.CharacterInfo == null)
            return;
        string characterName = advPager.CharacterInfo.NameText;
        if (!characterBoard.CharacterDic.HasCharacter(characterName))
            return;

        string genericID = characterBoard.CharacterDic.GetGenericIDFromName(characterName);
        if (speakingCharacter != null && speakingCharacter.Count > 0)
        {
            for (int i = 0; i < speakingCharacter.Count; i++)
            {
                speakingCharacter[i].SetSpeak(false);
            }
        }
        speakingCharacter = new List<UtageCharacter>();
        StartCoroutine(SpeakingListening(genericID));
    }
    IEnumerator SpeakingListening(string _id)
    {
        var characters = utageCharaterCommand.GetCharactors(_id);
        while (true)
        {
            characters = utageCharaterCommand.GetCharactors(_id);
            if (characters != null && characters.Count > 0)
                break;
            yield return null;
        }
        speakingCharacter = new List<UtageCharacter>();
        speakingCharacter.AddRange(characters);
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].SetSpeak(true);
        }
    }
    private void StopSpeak(AdvPage _page)
    {
        if (_page.CharacterInfo == null)
            return;
        string characterName = advPager.CharacterInfo.NameText;
        if (!characterBoard.CharacterDic.HasCharacter(characterName))
            return;
        string genericID = characterBoard.CharacterDic.GetGenericIDFromName(characterName);
        StartCoroutine(StopSpeakistening(genericID));
    }
    IEnumerator StopSpeakistening(string _id)
    {
        var characters = utageCharaterCommand.GetCharactors(_id);
        while (true)
        {
            characters = utageCharaterCommand.GetCharactors(_id);
            if (characters != null && characters.Count > 0)
                break;
            yield return null;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].SetSpeak(false);
        }
        speakingCharacter = new List<UtageCharacter>();
    }
}
