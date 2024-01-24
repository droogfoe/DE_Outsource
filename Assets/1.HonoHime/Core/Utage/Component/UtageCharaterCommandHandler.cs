using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utage;
using Sirenix.OdinInspector;
using static UtageExtensions.UtageCharacterUtility;

public class UtageCharaterCommandHandler : MonoBehaviour {
    private static UtageCharaterCommandHandler inst;
    public static UtageCharaterCommandHandler Inst{ get { return inst; } }

    [ReadOnly] [SerializeField] private UtageCharacterDictionary characters;

    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        // static instance 
        if (inst == null)
            inst = this;
        else
            Destroy(this);

        // dictionary
        characters = new UtageCharacterDictionary();
    }
    public void RegistCharacter(UtageCharacter _character)
    {
        if (!characters.ContainsKey(_character.CharacterName))
        {
            List<UtageCharacter> newList = new List<UtageCharacter>() { _character};
            characters.Add(_character.CharacterName, newList);
        }
        else
        {
            List<UtageCharacter> list = characters[_character.CharacterName];
            if (!list.Contains(_character)) list.Add(_character);
        }
    }
    public void UnregisterCharacter(UtageCharacter _character)
    {
        if (characters.ContainsKey(_character.CharacterName) 
            && characters[_character.CharacterName].Contains(_character))
        {
            characters[_character.CharacterName].Remove(_character);
            if (characters[_character.CharacterName].Count == 0)
                characters.Remove(_character.CharacterName);
        }
        else
        {
            Debug.LogError($"Don't contain {_character.CharacterName}");
        }
    }
    public List<UtageCharacter> GetAllCharacters()
    {
        List<UtageCharacter> list = new List<UtageCharacter>(); 
        for (int i = 0; i < characters.Count; i++)
        {
            list.AddRange(characters.ElementAtOrDefault(i).Value);
        }
        return list;
    }
    public List<UtageCharacter> GetCharactors(string _charactorID)
    {
        if (!characters.ContainsKey(_charactorID))
            return null;

        return characters[_charactorID];
    }
    public void OnDoCommand(CharacterCommandContext _context)
    {
        AdvCommandType type;
        CharacterCommandContext context;
        if (TryParseContextCommandType(_context, out type, out context)) 
        {
            SendCommandToCharacter(type, context);
        }
        else
        {
            return;
        }
    }

    private void SendCommandToCharacter(AdvCommandType type, CharacterCommandContext context)
    {
        if (characters == null || !characters.ContainsKey(context.CharacterName) || characters[context.CharacterName].Count == 0)
        {
            return;
        }

        List<UtageCharacter> list = characters[context.CharacterName];
        for (int i = 0; i < list.Count; i++)
        {
            switch (type)
            {
                case AdvCommandType.Perform:
                    list[i].SetPerformAction(context.ActionName, context.Flag);
                    break;
                case AdvCommandType.Face:
                    list[i].SetFaceAction(context.ActionName, context.Flag);
                    break;
                case AdvCommandType.Emoji:
                    EmojiContext emojiContext = (EmojiContext)context;
                    list[i].SetEmojiAction(context.ActionName, emojiContext.LifeTime);
                    break;
                case AdvCommandType.PPEffect:
                    break;
                case AdvCommandType.Anim:
                    AnimContext animContext = (AnimContext)context;
                    list[i].SetAnimAction(context.ActionName, animContext.Flag, animContext.LifeTime);
                    break;
                case AdvCommandType.Move:
                    MoveContext moveContext = (MoveContext)context;
                    list[i].SetMove(context.ActionName, moveContext.Index);
                    break;
                default:
                    break;
            }
        }
    }

    private bool TryParseContextCommandType(CharacterCommandContext _incontext, out AdvCommandType _type, out CharacterCommandContext _outcontext)
    {
        bool success = true;
        _type = AdvCommandType.None;
        _outcontext = null;

        if (_incontext.GetType() == typeof(PerformContext))
        {
            _type = AdvCommandType.Perform;
            _outcontext = (PerformContext)_incontext;
        }
        else if (_incontext.GetType() == typeof(FaceContext))
        {
            _type = AdvCommandType.Face;
            _outcontext = (FaceContext)_incontext;
        }
        else if (_incontext.GetType() == typeof(EmojiContext))
        {
            _type = AdvCommandType.Emoji;
            _outcontext = (EmojiContext)_incontext;
        }
        else if (_incontext.GetType() == typeof(PPEffectContext))
        {
            _type = AdvCommandType.PPEffect;
            _outcontext = (PPEffectContext)_incontext;
        }
        else if (_incontext.GetType() == typeof(AnimContext))
        {
            _type = AdvCommandType.Anim;
            _outcontext = (AnimContext)_incontext;
        }
        else if (_incontext.GetType() == typeof(MoveContext))
        {
            _type = AdvCommandType.Move;
            _outcontext = (MoveContext)_incontext;
        }
        else
        {
            success = false;
        }
        return success;
    }

    [System.Serializable]
    public class UtageCharacterDictionary : UnitySerializedDictionary<string, List<UtageCharacter>> { }
}
