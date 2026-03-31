using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

	private bool _hookAttempted = false;

	private MSD_SubSceneInWorldWide _lastSubScene = (MSD_SubSceneInWorldWide)(-1);

	private int _virtualIdx = 0;

	private string _activeSlot = "";

	private string _urlInput = "";

	private bool _activeIsOfficial = false;

	private Font _gameFont = null;

	private Font GetGameFont()
	{
		if ((Object)(object)_gameFont != (Object)null)
		{
			return _gameFont;
		}
		UILabel[] array = (UILabel[])(object)Object.FindObjectsOfType(typeof(UILabel));
		UILabel[] array2 = array;
		foreach (UILabel val in array2)
		{
			if ((Object)(object)val != (Object)null && (Object)(object)val.font != (Object)null && (Object)(object)val.font.dynamicFont != (Object)null)
			{
				_gameFont = val.font.dynamicFont;
				break;
			}
		}
		return _gameFont;
	}

	private void Awake()
	{
		List<string> list = new List<string>(STANDARD_MAPS);
		list.AddRange(CUSTOM_MAPS);
		_allMaps = list.ToArray();
	}

	private void RebuildMapList()
	{
		List<string> list = new List<string>(STANDARD_MAPS);
		OfficialMapEntry[] officialMaps = ContentManager.OfficialMaps;
		foreach (OfficialMapEntry officialMapEntry in officialMaps)
		{
			list.Add("OFFICIAL_" + officialMapEntry.Id);
		}
		list.AddRange(CUSTOM_MAPS);
		_allMaps = list.ToArray();
		ModEntry.Log("CustomMaps: map list rebuilt — " + STANDARD_MAPS.Length + " std + " + ContentManager.OfficialMaps.Length + " official + " + CUSTOM_MAPS.Length + " custom = " + _allMaps.Length);
	}

	private void OnLevelWasLoaded(int level)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_hooked = false;
		_hookAttempted = false;
		_lastSubScene = (MSD_SubSceneInWorldWide)(-1);
		_activeSlot = "";
		_urlInput = "";
		_activeIsOfficial = false;
		PlayerPrefs.SetString("CNRMod_CustomMapName", "");
		PlayerPrefs.Save();
	}

	private void Update()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (_hooked || Application.loadedLevelName != "MultiplayerSelect")
		{
			return;
		}
		MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
		if (!((Object)(object)mInstance == (Object)null))
		{
			if (mInstance.mCurWWSubScene != _lastSubScene)
			{
				_lastSubScene = mInstance.mCurWWSubScene;
				ModEntry.Log("CustomMaps subScene=" + mInstance.mCurWWSubScene);
			}
			if ((int)mInstance.mCurWWSubScene == 1 && !_hookAttempted)
			{
				_hookAttempted = true;
				((MonoBehaviour)this).StartCoroutine(HookButtons());
				_hooked = true;
			}
		}
	}

	private IEnumerator HookButtons()
	{
		yield return (object)new WaitForSeconds(0.1f);
		float waited = 0f;
		while (!ContentManager.Ready && waited < 5f)
		{
			yield return (object)new WaitForSeconds(0.25f);
			waited += 0.25f;
		}
		RebuildMapList();
		MonoBehaviour[] all = (MonoBehaviour[])(object)Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
		int hooked = 0;
		MonoBehaviour[] array = all;
		foreach (MonoBehaviour val in array)
		{
			if (((object)val).GetType().Name != "MapSelectButtonEvent")
			{
				continue;
			}
			FieldInfo field = ((object)val).GetType().GetField("buttonName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)field != null)
			{
				string text = field.GetValue(val).ToString();
				if (text == "WWMapNext" || text == "WWMapPre")
				{
					object value = Enum.Parse(field.FieldType, "Nil");
					field.SetValue(val, value);
					MapNavButton mapNavButton = ((Component)val).gameObject.AddComponent<MapNavButton>();
					mapNavButton.isNext = text == "WWMapNext";
					mapNavButton.hook = this;
					hooked++;
					ModEntry.Log("Hooked button: " + text);
				}
			}
		}
		if (hooked > 0)
		{
			_hooked = true;
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
		ModEntry.Log("HookButtons done: " + hooked + " hooked");
	}

	private void OnJoinedRoom()
	{
		MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
		if (!((Object)(object)mInstance == (Object)null))
		{
			string mCurWWMapSelect = mInstance.mCurWWMapSelect;
			if (CUSTOM_NAMES.ContainsKey(mCurWWMapSelect))
			{
				((MonoBehaviour)this).StartCoroutine(LoadLevelWatchdog(mCurWWMapSelect));
			}
		}
	}

	private IEnumerator LoadLevelWatchdog(string scene)
	{
		yield return (object)new WaitForSeconds(5f);
		if (Application.loadedLevelName == "MultiplayerSelect")
		{
			ModEntry.Log("Watchdog: redirecting from " + scene + " to FreeRun3_1");
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
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Expected O, but got Unknown
		//IL_0227: Expected O, but got Unknown
		string text = _allMaps[idx];
		int num = Array.IndexOf(STANDARD_MAPS, text);
		if (num >= 0)
		{
			PlayerPrefs.SetString("CNRMod_CustomMapName", "");
			PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
			PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
			PlayerPrefs.DeleteKey("CNRMod_DonorScene");
			_activeSlot = "";
			_urlInput = "";
			_activeIsOfficial = false;
			msd.mCurWWMapSelect = text;
			((UIWidget)msd.mWWMapUITexture).mainTexture = (Texture)msd.mWWMapTexture[num];
			((UIWidget)msd.mWWMapUITexture).MarkAsChanged();
			msd.WWResetModeCheckBox();
		}
		else if (text.StartsWith("OFFICIAL_"))
		{
			string text2 = text.Substring("OFFICIAL_".Length);
			OfficialMapEntry officialMapEntry = null;
			OfficialMapEntry[] officialMaps = ContentManager.OfficialMaps;
			foreach (OfficialMapEntry officialMapEntry2 in officialMaps)
			{
				if (officialMapEntry2.Id == text2)
				{
					officialMapEntry = officialMapEntry2;
					break;
				}
			}
			if (officialMapEntry == null)
			{
				ModEntry.Log("ApplyMap: official map not found: " + text2);
				return;
			}
			string[] array = new string[3] { "FreeRun3_1", "FreeRun5_1", "FreeRun8_1" };
			string text3 = "FreeRun3_1";
			string text4 = "/storage/emulated/0/CNRMods/content_cache/maps/" + officialMapEntry.Id + ".json";
			if (File.Exists(text4))
			{
				try
				{
					string json = File.ReadAllText(text4);
					string text5 = ModEntry.ParseJsonStringValue(json, "donor");
					if (!string.IsNullOrEmpty(text5) && Array.IndexOf(array, text5) >= 0)
					{
						text3 = text5;
					}
				}
				catch
				{
				}
			}
			msd.mCurWWMapSelect = text3;
			Texture2D mapThumbnail = ContentManager.GetMapThumbnail(officialMapEntry.Id);
			((UIWidget)msd.mWWMapUITexture).mainTexture = (((Object)(object)mapThumbnail != (Object)null) ? ((Texture)mapThumbnail) : ((Texture)msd.mWWMapTexture[STANDARD_MAPS.Length - 1]));
			((UIWidget)msd.mWWMapUITexture).MarkAsChanged();
			msd.WWResetModeCheckBox();
			PlayerPrefs.SetString("CNRMod_CustomMapName", officialMapEntry.Name);
			_activeSlot = "";
			_urlInput = "";
			_activeIsOfficial = true;
			string destFileName = "/storage/emulated/0/CNRMods/custom_map_cache.json";
			if (File.Exists(text4))
			{
				try
				{
					File.Copy(text4, destFileName, overwrite: true);
					PlayerPrefs.SetInt("CNRMod_MapCacheReady", 1);
					PlayerPrefs.SetString("CNRMod_DonorScene", text3);
					PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
					ModEntry.Log("Official map '" + officialMapEntry.Name + "': used pre-cached JSON (donor=" + text3 + ")");
				}
				catch (Exception ex)
				{
					ModEntry.Log("Official map copy error: " + ex.Message);
					PlayerPrefs.SetString("CNRMod_ActiveMapURL", officialMapEntry.Url);
					PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
				}
			}
			else
			{
				PlayerPrefs.SetString("CNRMod_ActiveMapURL", officialMapEntry.Url);
				PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
				PlayerPrefs.DeleteKey("CNRMod_DonorScene");
				((MonoBehaviour)this).StartCoroutine(FetchDonor(officialMapEntry.Url));
				ModEntry.Log("Official map '" + officialMapEntry.Name + "': URL queued for download (no cache yet)");
			}
			PlayerPrefs.Save();
		}
		else
		{
			string[] array = new string[3] { "FreeRun3_1", "FreeRun5_1", "FreeRun8_1" };
			string text6 = PlayerPrefs.GetString("CNRMod_DonorScene", "");
			string text3 = ((Array.IndexOf(array, text6) >= 0) ? text6 : (CUSTOM_SCENE_LOAD.ContainsKey(text) ? CUSTOM_SCENE_LOAD[text] : "FreeRun3_1"));
			msd.mCurWWMapSelect = text3;
			((UIWidget)msd.mWWMapUITexture).mainTexture = (Texture)msd.mWWMapTexture[STANDARD_MAPS.Length - 1];
			((UIWidget)msd.mWWMapUITexture).MarkAsChanged();
			msd.WWResetModeCheckBox();
			PlayerPrefs.SetString("CNRMod_CustomMapName", CUSTOM_NAMES[text]);
			_activeSlot = text;
			_activeIsOfficial = false;
			_urlInput = PlayerPrefs.GetString("CNRMod_MapURL_" + text, "");
			PlayerPrefs.SetString("CNRMod_ActiveMapURL", _urlInput);
			PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
			PlayerPrefs.Save();
			if (!string.IsNullOrEmpty(_urlInput))
			{
				((MonoBehaviour)this).StartCoroutine(FetchDonor(_urlInput));
			}
		}
		ModEntry.Log("Map -> " + text + " (loads: " + msd.mCurWWMapSelect + ")");
	}

	private IEnumerator FetchDonor(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			yield break;
		}
		url = ModEntry.SanitizeUrl(url);
		WWW www = new WWW(url);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			ModEntry.Log("FetchDonor error: " + www.error);
			yield break;
		}
		string donor = ModEntry.ParseJsonStringValue(www.text, "donor");
		string[] validDonors = new string[3] { "FreeRun3_1", "FreeRun5_1", "FreeRun8_1" };
		if (!string.IsNullOrEmpty(donor) && Array.IndexOf(validDonors, donor) >= 0)
		{
			PlayerPrefs.SetString("CNRMod_DonorScene", donor);
			PlayerPrefs.Save();
			MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
			if ((Object)(object)mInstance != (Object)null)
			{
				mInstance.mCurWWMapSelect = donor;
			}
			ModEntry.Log("FetchDonor: donor=" + donor + " applied");
		}
		else
		{
			ModEntry.Log("FetchDonor: no valid donor field in response");
		}
	}

	private void OnGUI()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Expected O, but got Unknown
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Expected O, but got Unknown
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerSelectDirector mInstance = MultiplayerSelectDirector.mInstance;
		if ((Object)(object)mInstance == (Object)null || (int)mInstance.mCurWWSubScene != 1)
		{
			return;
		}
		string text = PlayerPrefs.GetString("CNRMod_CustomMapName", "");
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		float num = Screen.width;
		float num2 = Screen.height;
		float num3 = num * 0.36f;
		float num4 = num * 0.62f;
		float num5 = num2 * 0.34f;
		int num6 = Mathf.Max(20, Mathf.RoundToInt(num2 / 27f));
		int num7 = Mathf.Max(17, Mathf.RoundToInt(num2 / 34f));
		Font gameFont = GetGameFont();
		GUIStyle val = new GUIStyle(GUI.skin.label);
		val.fontSize = num6;
		val.fontStyle = (FontStyle)1;
		val.alignment = (TextAnchor)3;
		if ((Object)(object)gameFont != (Object)null)
		{
			val.font = gameFont;
		}
		GUI.color = new Color(1f, 0.55f, 0.05f, 0.95f);
		float num8 = num4;
		float num9 = (float)num6 + 6f;
		for (int i = 0; i < text.Length; i++)
		{
			string text2 = text[i].ToString();
			Vector2 val2 = val.CalcSize(new GUIContent(text2));
			GUI.Label(new Rect(num8, num5, val2.x + 2f, num9), text2, val);
			num8 += val2.x + 2f;
		}
		GUIStyle val3 = new GUIStyle(GUI.skin.label);
		val3.fontSize = num7;
		val3.fontStyle = (FontStyle)0;
		val3.alignment = (TextAnchor)3;
		if ((Object)(object)gameFont != (Object)null)
		{
			val3.font = gameFont;
		}
		string text3 = (_activeIsOfficial ? "Official server map" : "Requires mod on all clients");
		GUI.color = (_activeIsOfficial ? new Color(0.4f, 0.9f, 0.4f, 0.85f) : new Color(1f, 1f, 0.3f, 0.85f));
		GUI.Label(new Rect(num4, num5 + (float)num6 + 6f, num3, (float)num7 + 4f), text3, val3);
		if (!string.IsNullOrEmpty(_activeSlot))
		{
			float num10 = num5 + (float)num6 + 6f + (float)num7 + 4f + 6f;
			GUIStyle val4 = new GUIStyle(GUI.skin.label);
			val4.fontSize = num7;
			val4.alignment = (TextAnchor)3;
			if ((Object)(object)gameFont != (Object)null)
			{
				val4.font = gameFont;
			}
			GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
			GUI.Label(new Rect(num4, num10, num3, (float)num7 + 4f), "Map JSON URL:", val4);
			num10 += (float)num7 + 4f;
			GUI.color = Color.white;
			GUIStyle val5 = new GUIStyle(GUI.skin.textField);
			val5.fontSize = num7;
			if ((Object)(object)gameFont != (Object)null)
			{
				val5.font = gameFont;
			}
			float num11 = (float)num7 + 14f;
			string text4 = GUI.TextField(new Rect(num4, num10, num3, num11), _urlInput, 512, val5);
			if (text4 != _urlInput)
			{
				_urlInput = text4;
				PlayerPrefs.SetString("CNRMod_MapURL_" + _activeSlot, text4);
				PlayerPrefs.SetString("CNRMod_ActiveMapURL", text4);
				PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
				PlayerPrefs.Save();
				((MonoBehaviour)this).StartCoroutine(FetchDonor(text4));
			}
		}
		GUI.color = Color.white;
	}
}
