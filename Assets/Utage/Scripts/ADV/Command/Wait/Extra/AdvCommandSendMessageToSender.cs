using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtageExtensions;


namespace Utage
{
    public class AdvCommandSendMessageToSender : AdvCommand {
		public AdvCommandSendMessageToSender(StringGridRow row)
			: base(row)
		{
			this.methodName = ParseCell<string>(AdvColumnName.Arg1);
			//this.arg2 = ParseCellOptional<int>(AdvColumnName.Arg2, 0);
		}

	
		
		public override void DoCommand(AdvEngine engine)
		{
			engine.ScenarioPlayer.SendMessageTarget.SafeSendMessage("OnDoCommand", this);
		}


		/// <summary>
		/// コマンドの待機処理をするか
		/// </summary>
		public bool IsWait { get { return isWait; } set { isWait = value; } }
		bool isWait = false;

		/// <summary>
		/// 名前
		/// </summary>
		public string MethodName { get { return methodName; } }
		string methodName;

		/// <summary>
		/// 引数2
		/// </summary>
		/// 
		//public int Arg2 { get { return arg2; } }
		//int arg2;
		public T Arg2<T>()
		{
			T value = ParseCellOptional<T>(AdvColumnName.Arg2,default(T));
			return value;
		}
		/// <summary>
		/// 引数3
		/// </summary>
		//public string Arg3 { get { return arg3; } }
		//string arg3;
		public T Arg3<T>()
		{
			T value = ParseCellOptional<T>(AdvColumnName.Arg3, default(T));
			return value;
		}

		/// <summary>
		/// 引数4
		/// </summary>
		//public string Arg4 { get { return arg4; } }
		//string arg4;
		public T Arg4<T>()
		{
			T value = ParseCellOptional<T>(AdvColumnName.Arg4, default(T));
			return value;
		}
		/// <summary>
		/// 引数5
		/// </summary>
		//public string Arg5 { get { return arg5; } }
		//string arg5;
		public T Arg5<T>()
		{
			T value = ParseCellOptional<T>(AdvColumnName.Arg5, default(T));
			return value;
		}
		/// <summary>
		/// 引数6
		/// </summary>
		//public string Arg6 { get { return arg6; } }
		//string arg6;
		public T Arg6<T>()
		{
			T value = ParseCellOptional<T>(AdvColumnName.Arg6, default(T));
			return value;
		}
		/// <summary>
		/// テキスト
		/// </summary>
		public string Text { get { return text; } }
		string text;
	}
}

