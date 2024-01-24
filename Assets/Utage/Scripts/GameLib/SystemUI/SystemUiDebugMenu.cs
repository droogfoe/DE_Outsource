using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UtageExtensions;
using UnityEditor;
using Opsive.Shared.Events;

namespace Utage
{

	/// <summary>
	/// デバッグメニュー表示
	/// </summary>
	[AddComponentMenu("Utage/Lib/System UI/SystemUiDebugMenu")]
	public class SystemUiDebugMenu : MonoBehaviour
	{
		[SerializeField]
		GameObject buttonRoot = null;

		[SerializeField]
		GameObject buttonViewRoot = null;
		
		[SerializeField]
		UguiLocalize buttonText = null;

		[SerializeField]
		GameObject debugInfo = null;

		[SerializeField]
		Text debugInfoText = null;

		[SerializeField]
		GameObject debugLog = null;

		[SerializeField]
		Text debugLogText=null;

		[SerializeField]
		bool autoUpdateLogText = true;
		//	public UILabel debugLogLabel;

		[SerializeField]
		GameObject rootDebugMenu = null;

		[SerializeField]
		GameObject targetDeleteAllSaveData = null;

		[SerializeField]
		bool enabeReleaseBuild = false;

		public static bool JapaneseForceInit = false;


		bool Ignore
		{
			get
			{
				return !enabeReleaseBuild && !UnityEngine.Debug.isDebugBuild;
			}
		}
		void Start()
		{
			if (Ignore)
			{
				buttonRoot.SetActive(false);
			}

            if (!JapaneseForceInit)
            {
				//EventHandler.RegisterEvent<string>(UGUILocalizationEvent.GLOBAL_SETLANGUAGE_EVENT, SwitchLanguage);
				JapaneseForceInit = true;
				SwitchLanguage("Japanese");
			}
			ClearAll();
			ChangeMode(currentMode);
		}
		private void SwitchLanguage(string _curL)
        {
			LanguageManagerBase langManager = LanguageManagerBase.Instance;
			if (langManager == null) return;
			if (langManager.Languages.Count < 1) return;

            if (_curL == "繁體中文")
            {
				_curL = "ChineseTraditional";
            }
            else if (_curL == "日本語")
            {
				_curL = "Japanese";
			}

			if (langManager.Languages.Contains(_curL))
            {
				ChangeLanguage(_curL);
            }

		}
    //    private void Update()
    //    {
    //        if (Input.GetKeyDown(KeyCode.K))
    //        {
				//Debug.Log("OnClickChangeLanguage");
				//OnClickChangeLanguage();
    //        }
    //    }

        enum Mode
		{
			Hide,
			Info,
			Log,
			Memu,
			Max,
		};

		Mode currentMode = Mode.Hide;
		public void OnClickSwitchButton()
		{
			if (Ignore) return;

			ChangeMode(currentMode + 1);
		}

		void ChangeMode(Mode mode)
		{
			if (currentMode == mode) return;
			if (mode >= Mode.Max) mode = 0;

			currentMode = mode;
			ClearAll();
			StopAllCoroutines();
			switch (currentMode)
			{
				case Mode.Info:
					StartCoroutine(CoUpdateInfo());
					break;
				case Mode.Log:
					StartCoroutine(CoUpdateLog());
					break;
				case Mode.Memu:
					StartCoroutine(CoUpdateMenu());
					break;
				case Mode.Hide:
					break;
			};
		}

		void ClearAll()
		{
			buttonViewRoot.SetActive(false);

			debugInfo.SetActive(false);
			debugLog.SetActive(false);

			rootDebugMenu.SetActive(false);
		}

		IEnumerator CoUpdateInfo()
		{
			buttonViewRoot.SetActive(true);
			buttonText.Key = SystemText.DebugInfo.ToString();

			debugInfo.SetActive(true);
			while (true)
			{
				debugInfoText.text = DebugPrint.GetDebugString();
				yield return null;
			}
		}

		IEnumerator CoUpdateLog()
		{
			buttonViewRoot.SetActive(true);
			buttonText.Key = SystemText.DebugLog.ToString();

			debugLog.SetActive(true);
			if (autoUpdateLogText)
			{
				debugLogText.text += DebugPrint.GetLogString();
			}

			yield break;
		}

		IEnumerator CoUpdateMenu()
		{
			buttonViewRoot.SetActive(true);
			buttonText.Key = SystemText.DebugMenu.ToString();

			rootDebugMenu.SetActive(true);

			yield break;
		}

		//セーブデータを消去して終了
		public void OnClickDeleteAllSaveDataAndQuit()
		{
			targetDeleteAllSaveData.SafeSendMessage("OnDeleteAllSaveDataAndQuit");
			PlayerPrefs.DeleteAll();
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
		
		//キャッシュファイルを全て削除
		public void OnClickDeleteAllCacheFiles()
		{
			AssetFileManager.GetInstance().AssetBundleInfoManager.DeleteAllCache();
		}

		//言語切り替え
		public void OnClickChangeLanguage()
		{
			LanguageManagerBase langManager = LanguageManagerBase.Instance;
			if (langManager == null) return;
			if (langManager.Languages.Count < 1) return;

			//言語をシフトループ
			int index = langManager.Languages.IndexOf(langManager.CurrentLanguage);
			langManager.CurrentLanguage = langManager.Languages[(index+1) % langManager.Languages.Count];
#if UNITY_EDITOR
			Debug.Log(AssetDatabase.GetAssetPath(langManager));
#endif
			Debug.Log("CurrentLanguage: " + langManager.CurrentLanguage);
		}

		//言語切り替え
		public void ChangeLanguage(string language)
		{
			LanguageManagerBase langManager = LanguageManagerBase.Instance;
			if (langManager == null) return;
			if (langManager.Languages.Count < 1) return;

			langManager.CurrentLanguage = language;
		}
		
		//ボイスのみ言語切り替え
		public void ChangeVoiceLanguage(string language)
		{
			LanguageManagerBase langManager = LanguageManagerBase.Instance;
			if (langManager == null) return;

			langManager.VoiceLanguage = language;
		}

		//ボイスのみの言語切り替えを元に戻す
		public void ResetVoiceLanguage()
		{
			LanguageManagerBase langManager = LanguageManagerBase.Instance;
			if (langManager == null) return;

			langManager.VoiceLanguage = "";
		}
	}
}