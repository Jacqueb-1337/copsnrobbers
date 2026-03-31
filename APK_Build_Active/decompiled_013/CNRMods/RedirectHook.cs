using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CNRMods;

public class RedirectHook : MonoBehaviour
{
	private const float PollInterval = 1f;

	private const float KickGraceSecs = 5f;

	private string _overlayMsg = null;

	private float _overlayAlpha = 0f;

	private bool _inRoom = false;

	private bool _isMaster = false;

	private float _pollTimer = 0f;

	private readonly Dictionary<int, float> _pendingVerify = new Dictionary<int, float>();

	private static Type _pnt = null;

	private static readonly string[] ConnectScenes = new string[2] { "MultiplayerSelect", "CNRConnectMenu" };

	private static readonly string[] GameScenes = new string[14]
	{
		"FreeRun3_1", "FreeRun4_1", "FreeRun5_1", "FreeRun6_1", "FreeRun7_1", "FreeRun8_1", "FreeRun9_1", "FreeRun10_1", "FreeRun11_1", "FreeRun12_1",
		"FreeRun13_1", "FreeRun14_1", "FreeRun15_1", "CRScene1"
	};

	private int _pollDebugCount = 0;

	private void Awake()
	{
		Application.runInBackground = true;
	}

	private void OnLevelWasLoaded(int level)
	{
		string loadedLevelName = Application.loadedLevelName;
		ModEntry.Log("Scene: " + loadedLevelName);
		_pollDebugCount = 0;
		if (_inRoom && Array.IndexOf(GameScenes, loadedLevelName) < 0)
		{
			ModEntry.Log("Scene change away from game while in room — flushing map state early");
			_inRoom = false;
			ModEntry.IsMaster = false;
			_pendingVerify.Clear();
			PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
			PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
			PlayerPrefs.Save();
		}
		if (Array.IndexOf(ConnectScenes, loadedLevelName) >= 0 && ModEntry.ServerIp != "")
		{
			ModEntry.Log("Connect scene — starting LAN redirect");
			((MonoBehaviour)this).StartCoroutine(RedirectCoroutine());
		}
	}

	private void Update()
	{
		_pollTimer -= Time.deltaTime;
		if (!(_pollTimer > 0f))
		{
			_pollTimer = 1f;
			PollRoomState();
		}
	}

	private void PollRoomState()
	{
		try
		{
			Type photonNetType = GetPhotonNetType();
			if ((object)photonNetType == null)
			{
				if (_pollDebugCount++ < 3)
				{
					ModEntry.Log("PollRoomState: PhotonNetwork type not found");
				}
				return;
			}
			bool staticBool = GetStaticBool(photonNetType, "inRoom");
			bool flag = staticBool && GetStaticBool(photonNetType, "isMasterClient");
			if (_pollDebugCount < 15)
			{
				_pollDebugCount++;
				ModEntry.Log("Poll[" + _pollDebugCount + "] inRoom=" + staticBool + " isMaster=" + flag + " scene=" + Application.loadedLevelName);
			}
			if (!_inRoom && staticBool)
			{
				OnEnteredRoom(photonNetType, flag);
			}
			else if (_inRoom && !staticBool)
			{
				OnLeftRoom();
			}
			_inRoom = staticBool;
			_isMaster = flag;
			if (_inRoom && _isMaster && ModEntry.KickNoMod)
			{
				CheckKickPlayers(photonNetType);
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("PollRoomState error: " + ex.Message);
		}
	}

	private void OnEnteredRoom(Type pnt, bool asMaster)
	{
		_pollDebugCount = 999;
		ModEntry.IsMaster = asMaster;
		ModEntry.Log("Entered room (asMaster=" + asMaster + ")");
		_pendingVerify.Clear();
		string roomName = GetRoomName(pnt);
		ModEntry.Log("Room: " + (roomName ?? "(unknown)"));
		SetRoomProp(pnt, "CNR_MOD_VERSION", ModEntry.ModVersion);
		if (asMaster)
		{
			string text = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "");
			if (!string.IsNullOrEmpty(text))
			{
				SetRoomProp(pnt, "CNR_MAP_URL", text);
				if (!string.IsNullOrEmpty(ModEntry.WebUrl) && !string.IsNullOrEmpty(roomName))
				{
					((MonoBehaviour)this).StartCoroutine(PostRoomToServer(roomName, text));
				}
				((MonoBehaviour)this).StartCoroutine(DownloadMap(text));
				ModEntry.Log("Master: registered map " + text);
			}
		}
		else if (!string.IsNullOrEmpty(ModEntry.WebUrl) && !string.IsNullOrEmpty(roomName))
		{
			((MonoBehaviour)this).StartCoroutine(FetchAndCacheMap(roomName));
		}
		else
		{
			string roomPropStr = GetRoomPropStr(pnt, "CNR_MAP_URL");
			if (!string.IsNullOrEmpty(roomPropStr))
			{
				((MonoBehaviour)this).StartCoroutine(DownloadMap(roomPropStr));
			}
		}
	}

	private void OnLeftRoom()
	{
		ModEntry.Log("Left room");
		_pendingVerify.Clear();
		_pollDebugCount = 0;
		PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
		PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
		PlayerPrefs.Save();
	}

	private static string GetRoomName(Type pnt)
	{
		try
		{
			PropertyInfo property = pnt.GetProperty("room", BindingFlags.Static | BindingFlags.Public);
			if ((object)property == null)
			{
				return null;
			}
			object value = property.GetValue(null, null);
			if (value == null)
			{
				return null;
			}
			PropertyInfo property2 = value.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
			if ((object)property2 == null)
			{
				property2 = value.GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public);
			}
			if ((object)property2 != null)
			{
				return property2.GetValue(value, null) as string;
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("GetRoomName error: " + ex.Message);
		}
		return null;
	}

	private IEnumerator PostRoomToServer(string roomName, string url)
	{
		string body = "{\"room\":\"" + EscapeJson(roomName) + "\",\"mapUrl\":\"" + EscapeJson(url) + "\"}";
		byte[] data = Encoding.UTF8.GetBytes(body);
		Hashtable h = new Hashtable { ["Content-Type"] = "application/json" };
		ModEntry.Log("PostRoom -> " + ModEntry.WebUrl + "/rooms");
		WWW www = new WWW(ModEntry.WebUrl + "/rooms", data, h);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			ModEntry.Log("PostRoom error: " + www.error);
		}
		else
		{
			ModEntry.Log("PostRoom OK: " + www.text);
		}
	}

	private IEnumerator FetchAndCacheMap(string roomName)
	{
		string fetchUrl = ModEntry.SanitizeUrl(ModEntry.WebUrl + "/rooms/" + Uri.EscapeDataString(roomName));
		ModEntry.Log("Client: GET " + fetchUrl);
		WWW www = new WWW(fetchUrl);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			ModEntry.Log("FetchRoom error: " + www.error);
			yield break;
		}
		string mapUrl = ModEntry.ParseJsonStringValue(www.text, "mapUrl");
		if (string.IsNullOrEmpty(mapUrl))
		{
			ModEntry.Log("FetchRoom: no mapUrl in: " + www.text);
			yield break;
		}
		ModEntry.Log("Client: got mapUrl=" + mapUrl);
		PlayerPrefs.SetString("CNRMod_ActiveMapURL", mapUrl);
		PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
		PlayerPrefs.Save();
		((MonoBehaviour)this).StartCoroutine(DownloadMap(mapUrl));
	}

	private static string EscapeJson(string s)
	{
		if (s == null)
		{
			return "";
		}
		return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n")
			.Replace("\r", "");
	}

	private void CheckKickPlayers(Type pnt)
	{
		try
		{
			PropertyInfo property = pnt.GetProperty("otherPlayers", BindingFlags.Static | BindingFlags.Public);
			if ((object)property == null || !(property.GetValue(null, null) is Array array))
			{
				return;
			}
			HashSet<int> hashSet = new HashSet<int>();
			foreach (object item in array)
			{
				if (item == null)
				{
					continue;
				}
				int intProp = GetIntProp(item, "ID");
				string playerCustomProp = GetPlayerCustomProp(item, "CNR_MOD_VERSION");
				hashSet.Add(intProp);
				if (!_pendingVerify.ContainsKey(intProp))
				{
					if (string.IsNullOrEmpty(playerCustomProp))
					{
						_pendingVerify[intProp] = Time.time;
						ModEntry.Log("Player " + intProp + " grace window started");
					}
				}
				else if (!string.IsNullOrEmpty(playerCustomProp))
				{
					_pendingVerify.Remove(intProp);
					if (!VersionOk(playerCustomProp))
					{
						ModEntry.Log("Kicking player " + intProp + ": version mismatch '" + playerCustomProp + "'");
						KickPlayer(pnt, item);
					}
				}
				else if (Time.time - _pendingVerify[intProp] > 5f)
				{
					ModEntry.Log("Kicking player " + intProp + ": no mod version after " + 5f + "s");
					_pendingVerify.Remove(intProp);
					KickPlayer(pnt, item);
				}
			}
			List<int> list = new List<int>();
			foreach (int key in _pendingVerify.Keys)
			{
				if (!hashSet.Contains(key))
				{
					list.Add(key);
				}
			}
			foreach (int item2 in list)
			{
				_pendingVerify.Remove(item2);
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("CheckKickPlayers error: " + ex.Message);
		}
	}

	private void KickPlayer(Type pnt, object player)
	{
		try
		{
			pnt.GetMethod("CloseConnection", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { player.GetType() }, null)?.Invoke(null, new object[1] { player });
		}
		catch (Exception ex)
		{
			ModEntry.Log("KickPlayer error: " + ex.Message);
		}
	}

	private bool VersionOk(string other)
	{
		try
		{
			int num = int.Parse(ModEntry.ModVersion.Trim().Split('.')[0]);
			int num2 = int.Parse(other.Trim().Split('.')[0]);
			return num == num2;
		}
		catch
		{
			return true;
		}
	}

	private IEnumerator DownloadMap(string url)
	{
		url = ModEntry.SanitizeUrl(url);
		ModEntry.Log("DownloadMap: " + url);
		WWW www = new WWW(url);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			ModEntry.Log("DownloadMap error: " + www.error);
			yield break;
		}
		string json = www.text;
		if (string.IsNullOrEmpty(json))
		{
			ModEntry.Log("DownloadMap: empty response");
			yield break;
		}
		try
		{
			if (!Directory.Exists("/storage/emulated/0/CNRMods/"))
			{
				Directory.CreateDirectory("/storage/emulated/0/CNRMods/");
			}
			File.WriteAllText("/storage/emulated/0/CNRMods/custom_map_cache.json", json);
			PlayerPrefs.SetInt("CNRMod_MapCacheReady", 1);
			PlayerPrefs.Save();
			ModEntry.Log("Map cached (" + json.Length + " bytes)");
		}
		catch (Exception ex)
		{
			ModEntry.Log("DownloadMap save error: " + ex.Message);
		}
	}

	private void SetRoomProp(Type pnt, string key, string value)
	{
		try
		{
			PropertyInfo property = pnt.GetProperty("room", BindingFlags.Static | BindingFlags.Public);
			if ((object)property != null)
			{
				object value2 = property.GetValue(null, null);
				if (value2 != null)
				{
					Hashtable hashtable = new Hashtable();
					hashtable[key] = value;
					value2.GetType().GetMethod("SetCustomProperties", new Type[1] { typeof(Hashtable) })?.Invoke(value2, new object[1] { hashtable });
				}
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("SetRoomProp error: " + ex.Message);
		}
	}

	private void SetPlayerProp(Type pnt, string key, string value)
	{
		try
		{
			PropertyInfo property = pnt.GetProperty("player", BindingFlags.Static | BindingFlags.Public);
			if ((object)property != null)
			{
				object value2 = property.GetValue(null, null);
				if (value2 != null)
				{
					Hashtable hashtable = new Hashtable();
					hashtable[key] = value;
					value2.GetType().GetMethod("SetCustomProperties", new Type[1] { typeof(Hashtable) })?.Invoke(value2, new object[1] { hashtable });
				}
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("SetPlayerProp error: " + ex.Message);
		}
	}

	private string GetRoomPropStr(Type pnt, string key)
	{
		try
		{
			PropertyInfo property = pnt.GetProperty("room", BindingFlags.Static | BindingFlags.Public);
			if ((object)property == null)
			{
				return null;
			}
			object value = property.GetValue(null, null);
			if (value == null)
			{
				return null;
			}
			PropertyInfo property2 = value.GetType().GetProperty("customProperties", BindingFlags.Instance | BindingFlags.Public);
			if ((object)property2 == null)
			{
				return null;
			}
			return (property2.GetValue(value, null) is Hashtable hashtable && hashtable.ContainsKey(key)) ? (hashtable[key] as string) : null;
		}
		catch
		{
			return null;
		}
	}

	private string GetPlayerCustomProp(object player, string key)
	{
		try
		{
			PropertyInfo property = player.GetType().GetProperty("customProperties", BindingFlags.Instance | BindingFlags.Public);
			if ((object)property == null)
			{
				return null;
			}
			return (property.GetValue(player, null) is Hashtable hashtable && hashtable.ContainsKey(key)) ? (hashtable[key] as string) : null;
		}
		catch
		{
			return null;
		}
	}

	private int GetIntProp(object obj, string name)
	{
		try
		{
			PropertyInfo property = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
			if ((object)property != null)
			{
				return (int)property.GetValue(obj, null);
			}
		}
		catch
		{
		}
		return -1;
	}

	private IEnumerator RedirectCoroutine()
	{
		object settings = null;
		while (settings == null)
		{
			settings = GetPhotonServerSettings();
			if (settings == null)
			{
				yield return null;
			}
		}
		Type t = settings.GetType();
		SetMember(t, settings, "ServerAddress", ModEntry.ServerIp);
		SetMember(t, settings, "ServerPort", ModEntry.ServerPort);
		SetMember(t, settings, "AppID", ModEntry.AppId);
		SetMember(t, settings, "HostType", 2);
		ModEntry.Log("Override -> " + ModEntry.ServerIp + ":" + ModEntry.ServerPort);
		CallStaticVoid("PhotonNetwork", "Disconnect");
		float timeout = 8f;
		while (timeout > 0f && GetConnectionState() != 0)
		{
			timeout -= Time.unscaledDeltaTime;
			yield return null;
		}
		SwapToTcp();
		DisableEncryption();
		ModEntry.Log("Calling ConnectUsingSettings...");
		try
		{
			CallStaticWithArg("PhotonNetwork", "ConnectUsingSettings", "v2.4");
		}
		catch (Exception ex)
		{
			ModEntry.Log("ConnectUsingSettings error: " + ex.Message);
			yield break;
		}
		float connectTimeout = 30f;
		int lastState = -999;
		while (connectTimeout > 0f)
		{
			int state = GetDetailedState();
			if (state != lastState)
			{
				ModEntry.Log("detailState=" + state + " (" + (30f - connectTimeout).ToString("F1") + "s)");
				lastState = state;
			}
			if (state == 0)
			{
				ShowOverlay("LAN server unreachable.\n" + ModEntry.ServerIp);
				yield break;
			}
			if (state >= 6)
			{
				ModEntry.Log("Lobby joined!");
				yield break;
			}
			connectTimeout -= Time.unscaledDeltaTime;
			yield return null;
		}
		ModEntry.Log("Connection timed out");
		CallStaticVoid("PhotonNetwork", "Disconnect");
		ShowOverlay("LAN connection timed out.\n" + ModEntry.ServerIp);
	}

	private void ShowOverlay(string msg)
	{
		ModEntry.Log("OVERLAY: " + msg);
		_overlayMsg = msg;
		_overlayAlpha = 1f;
		((MonoBehaviour)this).StartCoroutine(FadeOverlay());
	}

	private IEnumerator FadeOverlay()
	{
		yield return (object)new WaitForSeconds(6f);
		float ft = 4f;
		while (ft > 0f)
		{
			_overlayAlpha = ft / 4f;
			ft -= Time.deltaTime;
			yield return null;
		}
		_overlayMsg = null;
		_overlayAlpha = 0f;
	}

	private void OnGUI()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		if (_overlayMsg != null && !(_overlayAlpha <= 0f))
		{
			GUI.color = new Color(0f, 0f, 0f, 0.75f * _overlayAlpha);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, 140f), (Texture)(object)Texture2D.whiteTexture);
			GUI.color = new Color(1f, 0.35f, 0.35f, _overlayAlpha);
			GUIStyle val = new GUIStyle(GUI.skin.label);
			val.fontSize = Mathf.Max(22, Screen.width / 22);
			val.alignment = (TextAnchor)4;
			val.wordWrap = true;
			GUI.Label(new Rect(20f, 8f, (float)Screen.width - 40f, 124f), "[CNR-Mod] " + _overlayMsg, val);
			GUI.color = Color.white;
		}
	}

	private static Type GetPhotonNetType()
	{
		if ((object)_pnt != null)
		{
			return _pnt;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("PhotonNetwork");
			if ((object)type != null)
			{
				_pnt = type;
				return type;
			}
		}
		return null;
	}

	private static bool GetStaticBool(Type t, string name)
	{
		try
		{
			PropertyInfo property = t.GetProperty(name, BindingFlags.Static | BindingFlags.Public);
			if ((object)property != null)
			{
				return (bool)property.GetValue(null, null);
			}
			FieldInfo field = t.GetField(name, BindingFlags.Static | BindingFlags.Public);
			if ((object)field != null)
			{
				return (bool)field.GetValue(null);
			}
		}
		catch
		{
		}
		return false;
	}

	private static object GetPhotonServerSettings()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("PhotonNetwork");
			if ((object)type == null)
			{
				continue;
			}
			PropertyInfo property = type.GetProperty("PhotonServerSettings", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)property != null)
			{
				object value = property.GetValue(null, null);
				if (value != null)
				{
					return value;
				}
			}
			FieldInfo field = type.GetField("PhotonServerSettings", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)field != null)
			{
				object value = field.GetValue(null);
				if (value != null)
				{
					return value;
				}
			}
		}
		return null;
	}

	private static int GetConnectionState()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("PhotonNetwork");
			if ((object)type != null)
			{
				PropertyInfo property = type.GetProperty("connectionState", BindingFlags.Static | BindingFlags.Public);
				if ((object)property != null)
				{
					return Convert.ToInt32(property.GetValue(null, null));
				}
			}
		}
		return -1;
	}

	private static int GetDetailedState()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("PhotonNetwork");
			if ((object)type != null)
			{
				PropertyInfo property = type.GetProperty("connectionStateDetailed", BindingFlags.Static | BindingFlags.Public);
				if ((object)property != null)
				{
					return Convert.ToInt32(property.GetValue(null, null));
				}
			}
		}
		return -1;
	}

	private static void CallStaticVoid(string typeName, string method)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType(typeName);
			if ((object)type != null)
			{
				MethodInfo method2 = type.GetMethod(method, BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
				if ((object)method2 != null)
				{
					method2.Invoke(null, null);
					break;
				}
			}
		}
	}

	private static void CallStaticWithArg(string typeName, string method, object arg)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType(typeName);
			if ((object)type == null)
			{
				continue;
			}
			MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo methodInfo in methods)
			{
				if (!(methodInfo.Name != method))
				{
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(arg.GetType()))
					{
						methodInfo.Invoke(null, new object[1] { arg });
						return;
					}
				}
			}
		}
	}

	private static void DisableEncryption()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("PhotonNetwork");
			if ((object)type == null)
			{
				continue;
			}
			FieldInfo field = type.GetField("networkingPeer", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)field == null)
			{
				continue;
			}
			object value = field.GetValue(null);
			if (value != null)
			{
				FieldInfo field2 = value.GetType().GetField("requestSecurity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)field2 != null)
				{
					field2.SetValue(value, false);
					ModEntry.Log("requestSecurity=false");
					break;
				}
			}
		}
	}

	private static void SwapToTcp()
	{
		try
		{
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				type = assembly.GetType("PhotonNetwork");
				if ((object)type != null)
				{
					break;
				}
			}
			if ((object)type == null)
			{
				return;
			}
			FieldInfo field = type.GetField("networkingPeer", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)field == null)
			{
				return;
			}
			object value = field.GetValue(null);
			if (value == null)
			{
				return;
			}
			FieldInfo fieldInfo = null;
			Type type2 = value.GetType();
			while ((object)type2 != null)
			{
				fieldInfo = type2.GetField("peerBase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)fieldInfo != null)
				{
					break;
				}
				type2 = type2.BaseType;
			}
			if ((object)fieldInfo == null)
			{
				return;
			}
			Type type3 = null;
			assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				type3 = assembly.GetType("ExitGames.Client.Photon.TPeer");
				if ((object)type3 == null)
				{
					type3 = assembly.GetType("TPeer");
				}
				if ((object)type3 != null)
				{
					break;
				}
			}
			if ((object)type3 == null)
			{
				ModEntry.Log("SwapToTcp: TPeer not found");
				return;
			}
			object obj = Activator.CreateInstance(type3, nonPublic: true);
			Type type4 = type3;
			while ((object)type4 != null)
			{
				FieldInfo field2 = type4.GetField("usedProtocol", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)field2 != null)
				{
					field2.SetValue(obj, Enum.ToObject(field2.FieldType, (byte)1));
					break;
				}
				type4 = type4.BaseType;
			}
			bool flag = false;
			Type type5 = type3;
			while ((object)type5 != null && !flag)
			{
				PropertyInfo property = type5.GetProperty("Listener", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)property != null)
				{
					MethodInfo setMethod = property.GetSetMethod(nonPublic: true);
					if ((object)setMethod != null)
					{
						setMethod.Invoke(obj, new object[1] { value });
						flag = true;
					}
					break;
				}
				type5 = type5.BaseType;
			}
			if (!flag)
			{
				string[] array = new string[4] { "<Listener>k__BackingField", "listener", "_listener", "Listener" };
				Type type6 = type3;
				while ((object)type6 != null && !flag)
				{
					string[] array2 = array;
					foreach (string name in array2)
					{
						FieldInfo field3 = type6.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if ((object)field3 != null)
						{
							field3.SetValue(obj, value);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						type6 = type6.BaseType;
					}
				}
			}
			fieldInfo.SetValue(value, obj);
			ModEntry.Log("SwapToTcp: done");
		}
		catch (Exception ex)
		{
			ModEntry.Log("SwapToTcp error: " + ex.Message);
		}
	}

	private static void SetMember(Type t, object inst, string name, object val)
	{
		FieldInfo field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if ((object)field != null)
		{
			object obj = (field.FieldType.IsEnum ? Enum.ToObject(field.FieldType, Convert.ChangeType(val, Enum.GetUnderlyingType(field.FieldType))) : Convert.ChangeType(val, field.FieldType));
			field.SetValue(inst, obj);
			ModEntry.Log("  " + name + "=" + obj);
			return;
		}
		PropertyInfo property = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if ((object)property != null && property.CanWrite)
		{
			property.SetValue(inst, Convert.ChangeType(val, property.PropertyType), null);
			ModEntry.Log("  " + name + "=" + val);
		}
	}
}
