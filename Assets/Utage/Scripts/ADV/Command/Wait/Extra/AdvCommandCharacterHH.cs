using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtageExtensions;
using System;
using AdvCommandType = UtageExtensions.UtageCharacterUtility.AdvCommandType;

namespace Utage {
    public class AdvCommandCharacterHH : AdvCommand {
		public AdvCommandCharacterHH(StringGridRow row)
			: base(row)
		{
			this.methodName = ParseCell<string>(AdvColumnName.Arg1);
			this.characterName = ParseCell<string>(AdvColumnName.Arg2);
			this.context = ParseContextProcesser();
		}

		public override void DoCommand(AdvEngine engine)
		{
			UtageCharaterCommandHandler.Inst.OnDoCommand(this.context);
		}
		private CharacterCommandContext ParseContextProcesser()
        {
			AdvCommandType type;
			if (Enum.TryParse(methodName, out type))
			{
                switch (type)
                {
                    case AdvCommandType.Perform:
						string actName = ParseCell<string>(AdvColumnName.Arg3);
						bool actFlag = ParseCell<bool>(AdvColumnName.Arg4);
						return new PerformContext(characterName, actName, actFlag);
                    case AdvCommandType.Face:
						string faceName = ParseCell<string>(AdvColumnName.Arg3);
						bool faceFlag = ParseCell<bool>(AdvColumnName.Arg4);
						return new FaceContext(characterName, faceName, faceFlag);
					case AdvCommandType.Emoji:
						string emojiName = ParseCell<string>(AdvColumnName.Arg3);
						float emojiLT = 0.0f;
                        if (!TryParseCell<float>(AdvColumnName.Arg4, out emojiLT))
							emojiLT = 5;
						return new EmojiContext(characterName, emojiName, emojiLT);
					case AdvCommandType.Anim:
						string actionName = ParseCell<string>(AdvColumnName.Arg3);
						bool actionFlag = ParseCell<bool>(AdvColumnName.Arg4);
						float actionLT = 0.0f;
						TryParseCell<float>(AdvColumnName.Arg5, out actionLT);
						return new AnimContext(characterName, actionName, actionFlag, actionLT);
					case AdvCommandType.Move:
						string moveType = ParseCell<string>(AdvColumnName.Arg3);
						int index = ParseCell<int>(AdvColumnName.Arg4);
						return new MoveContext(characterName, moveType, index);
					default:
						return null;
                }
            }
            else
            {
				Debug.LogError(GetType() + " parse context fail.");
				return null;
            }
		}

		public string MethodName { get { return methodName; } }
		string methodName;
		public string CharacterName { get { return characterName; } }
		string characterName;

		public CharacterCommandContext Context { get { return context; } }
		private CharacterCommandContext context;
	}
	public class CharacterCommandContext {
		public string CharacterName;
		public string ActionName;
		public bool Flag;
	}
	public class PerformContext : CharacterCommandContext {
		public PerformContext(string _name, string _act, bool _flag)
        {
			CharacterName = _name;
			ActionName = _act;
			Flag = _flag;
		}
	}
	public class FaceContext : CharacterCommandContext {

		public FaceContext(string _name, string _face, bool _flag)
		{
			CharacterName = _name;
			ActionName = _face;
			Flag = _flag;
		}
	}
	public class PPEffectContext : CharacterCommandContext
	{
		public PPEffectContext(string _name, string _effect, bool _flag)
		{
			//CharacterName = _name;
			//ActionName = _emoji;
			//Flag = _flag;
		}
	}
	public class EmojiContext : CharacterCommandContext
	{
		public float LifeTime;
		public EmojiContext(string _name, string _emoji, float _lifeTime)
		{
            CharacterName = _name;
            ActionName = _emoji;
			LifeTime = _lifeTime;
        }
	}
	public class AnimContext : CharacterCommandContext
    {
		public float LifeTime;
		public AnimContext(string _name, string _action, bool _flag, float _lifeTime)
        {
			CharacterName = _name;
			ActionName = _action;
			Flag = _flag;
			LifeTime = _lifeTime;
        }
	}
	public class MoveContext : CharacterCommandContext
    {
		public int Index;
		public MoveContext(string _name, string _moveType, int _index)
        {
			CharacterName = _name;
			ActionName = _moveType;
			Index = _index;
		}
	}
}