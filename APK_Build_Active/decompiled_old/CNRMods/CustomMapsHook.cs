using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CNRMods;

public class CustomMapsHook : MonoBehaviour
{
	private static readonly string[] STANDARD_MAPS = new string[10] { "FreeRun3_1", "FreeRun4_1", "FreeRun5_1", "FreeRun6_1", "FreeRun7_1", "FreeRun8_1", "FreeRun9_1", "FreeRun10_1", "FreeRun11_1", "FreeRun12_1" };

	private static readonly string[] CUSTOM_MAPS = new string[3] { "FreeRun13_1", "FreeRun14_1", "FreeRun15_1" };

	private static readonly Dictionary<string, string> CUSTOM_NAMES = new Dictionary<string, string>
	{
		{ "FreeRun13_1", "[MOD] Map 11" },
		{ "FreeRun14_1", "[MOD] Map 12" },
		{ "FreeRun15_1", "[MOD] Map 13" }
	};

	private static readonly Dictionary<string, string> CUSTOM_SCENE_LOAD = new Dictionary<string, string>
	{
		{ "FreeRun13_1", "FreeRun3_1" },
		{ "FreeRun14_1", "FreeRun5_1" },
		{ "FreeRun15_1", "FreeRun8_1" }
	};

	private string[] _allMaps;

	private bool _hooked = false;

	private MSD_SubSceneInWorldWide _lastSubScene = (MSD_SubSceneInWorldWide)(-1);

	private int _virtualIdx = 0;

	private void Awake()
	{
		List<string> list = new List<string>(STANDARD_MAPS);
		list.AddRange(CUSTOM_MAPS);
		_allMaps = list.ToArray();
	}

	private void OnLevelWasLoaded(int level)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		_hooked = false;
		_lastSubScene = (MSD_SubSceneInWorldWide)(-1);
	}

	private void Update()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Invalid comparison between Unknown and I4
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (_hooked || Application.loadedLevelName != "MultiplayerSelect")
		{
			return;
		}
		MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
		if (!((Object)(object)mInstance == (Object)null))
		{
			if (mInstance.mCurWWSubScene != _lastSubScene)
			{
				ModEntry.Log("CustomMaps Update: mCurWWSubScene=" + mInstance.mCurWWSubScene);
				_lastSubScene = mInstance.mCurWWSubScene;
			}
			if ((int)mInstance.mCurWWSubScene == 1)
			{
				((MonoBehaviour)this).StartCoroutine(HookButtons());
				_hooked = true;
			}
		}
	}

	private IEnumerator HookButtons()
	{
		yield return (object)new WaitForSeconds(0.1f);
		MonoBehaviour[] allBehaviours = (MonoBehaviour[])(object)Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
		ModEntry.Log("HookButtons: scanning " + allBehaviours.Length + " behaviours (incl. inactive)");
		int hooked = 0;
		MonoBehaviour[] array = allBehaviours;
		foreach (MonoBehaviour val in array)
		{
			if (((object)val).GetType().Name != "MapSelectButtonEvent")
			{
				continue;
			}
			FieldInfo field = ((object)val).GetType().GetField("buttonName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				string text = field.GetValue(val).ToString();
				ModEntry.Log("  MapSelectButtonEvent: " + text + " on " + ((Object)((Component)val).gameObject).name);
				if (text == "WWMapNext" || text == "WWMapPre")
				{
					object value = Enum.Parse(field.FieldType, "Nil");
					field.SetValue(val, value);
					MapNavButton mapNavButton = ((Component)val).gameObject.AddComponent<MapNavButton>();
					mapNavButton.isNext = text == "WWMapNext";
					mapNavButton.hook = this;
					hooked++;
					ModEntry.Log("  -> HOOKED as " + (mapNavButton.isNext ? "Next" : "Pre"));
				}
			}
		}
		_hooked = hooked > 0;
		if (_hooked)
		{
			MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
			if ((Object)(object)mInstance != (Object)null)
			{
				int num = Array.IndexOf(STANDARD_MAPS, mInstance.mCurWWMapSelect);
				if (num >= 0)
				{
					_virtualIdx = num;
				}
			}
		}
		ModEntry.Log("Button hook complete — hooked " + hooked + " button(s)");
	}

	private void OnJoinedRoom()
	{
		MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
		if (!((Object)(object)mInstance == (Object)null))
		{
			string mCurWWMapSelect = mInstance.mCurWWMapSelect;
			ModEntry.Log("OnJoinedRoom: mCurWWMapSelect=" + mCurWWMapSelect);
			if (CUSTOM_NAMES.ContainsKey(mCurWWMapSelect))
			{
				ModEntry.Log("Custom map room joined — LoadLevel(" + mCurWWMapSelect + ") about to fire");
				((MonoBehaviour)this).StartCoroutine(LoadLevelWatchdog(mCurWWMapSelect));
			}
		}
	}

	private IEnumerator LoadLevelWatchdog(string expectedScene)
	{
		yield return (object)new WaitForSeconds(5f);
		if (Application.loadedLevelName == "MultiplayerSelect")
		{
			ModEntry.Log("WATCHDOG: still on MultiplayerSelect after 5s — '" + expectedScene + "' may not exist in this APK build. Redirecting to FreeRun3_1");
			MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
			if ((Object)(object)mInstance != (Object)null)
			{
				mInstance.mCurWWMapSelect = "FreeRun3_1";
				Application.LoadLevel("FreeRun3_1");
			}
		}
	}

	public void OnNextMap()
	{
		MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
		if (!((Object)(object)mInstance == (Object)null))
		{
			_virtualIdx = ((_virtualIdx < _allMaps.Length - 1) ? (_virtualIdx + 1) : 0);
			ApplyMap(mInstance, _virtualIdx);
		}
	}

	public void OnPreMap()
	{
		MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
		if (!((Object)(object)mInstance == (Object)null))
		{
			_virtualIdx = ((_virtualIdx <= 0) ? (_allMaps.Length - 1) : (_virtualIdx - 1));
			ApplyMap(mInstance, _virtualIdx);
		}
	}

	private void ApplyMap(MultiplayerSelectDirector msd, int idx)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		string text = _allMaps[idx];
		int num = Array.IndexOf(STANDARD_MAPS, text);
		if (num >= 0)
		{
			PlayerPrefs.SetString("CNRMod_CustomMapName", "");
			msd.mCurWWMapSelect = text;
			((UIWidget)msd.mWWMapUITexture).mainTexture = (Texture)msd.mWWMapTexture[num];
			((UIWidget)msd.mWWMapUITexture).MarkAsChanged();
			msd.WWResetModeCheckBox();
		}
		else
		{
			string mCurWWMapSelect = (CUSTOM_SCENE_LOAD.ContainsKey(text) ? CUSTOM_SCENE_LOAD[text] : "FreeRun3_1");
			msd.mCurWWMapSelect = mCurWWMapSelect;
			((UIWidget)msd.mWWMapUITexture).mainTexture = (Texture)msd.mWWMapTexture[STANDARD_MAPS.Length - 1];
			((UIWidget)msd.mWWMapUITexture).MarkAsChanged();
			msd.mModeCheckBoxSH.SetActive(false);
			msd.mModeCheckBoxTDM.SetActive(true);
			msd.SwitchToMode((GrowthGameModeTag)0);
			PlayerPrefs.SetString("CNRMod_CustomMapName", CUSTOM_NAMES[text]);
		}
		ModEntry.Log("Map changed to: " + text + " (loads: " + msd.mCurWWMapSelect + ")");
	}

	private void OnGUI()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
		if (!((Object)(object)mInstance == (Object)null) && (int)mInstance.mCurWWSubScene == 1)
		{
			string text = PlayerPrefs.GetString("CNRMod_CustomMapName", "");
			if (!string.IsNullOrEmpty(text))
			{
				GUIStyle val = new GUIStyle(GUI.skin.label);
				val.fontSize = 20;
				val.fontStyle = (FontStyle)1;
				val.alignment = (TextAnchor)4;
				GUIStyle val2 = val;
				float num = Screen.width;
				float num2 = Screen.height;
				float num3 = 280f;
				float num4 = 40f;
				float num5 = num * 0.5f - num3 * 0.5f;
				float num6 = num2 * 0.34f;
				GUI.color = new Color(1f, 0.55f, 0.05f, 0.95f);
				GUI.Label(new Rect(num5, num6, num3, num4), text, val2);
				GUIStyle val3 = new GUIStyle(GUI.skin.label);
				val3.fontSize = 12;
				val3.fontStyle = (FontStyle)2;
				val3.alignment = (TextAnchor)4;
				GUIStyle val4 = val3;
				GUI.color = new Color(1f, 1f, 0.3f, 0.85f);
				GUI.Label(new Rect(num5, num6 + num4, num3, 28f), "Requires mod on all clients", val4);
				GUI.color = Color.white;
			}
		}
	}
}
