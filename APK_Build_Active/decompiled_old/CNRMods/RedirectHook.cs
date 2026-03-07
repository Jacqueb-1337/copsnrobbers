using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace CNRMods;

public class RedirectHook : MonoBehaviour
{
	public string ServerIp = "";

	private string _overlayMessage = null;

	private float _overlayOpacity = 0f;

	private DateTime _lastUpdateLog = DateTime.MinValue;

	private int _updateLogCount = 0;

	private static readonly string[] ConnectScenes = new string[2] { "MultiplayerSelect", "CNRConnectMenu" };

	private bool _connected = false;

	private void Awake()
	{
		Application.runInBackground = true;
	}

	private void Start()
	{
		Application.runInBackground = true;
		_lastUpdateLog = DateTime.Now;
		ModEntry.Log("Start() — scene=" + Application.loadedLevelName);
	}

	private void OnLevelWasLoaded(int level)
	{
		string loadedLevelName = Application.loadedLevelName;
		ModEntry.Log("OnLevelWasLoaded: scene=" + loadedLevelName + " level=" + level);
		if (Array.IndexOf(ConnectScenes, loadedLevelName) >= 0)
		{
			ModEntry.Log("Entered connect scene — starting LAN connection");
			_connected = false;
			((MonoBehaviour)this).StartCoroutine(RedirectCoroutine());
		}
		else if (loadedLevelName != "MainMenu")
		{
			ModEntry.Log("Scene '" + loadedLevelName + "' is not a connect scene and not MainMenu — leaving connection untouched");
		}
	}

	private void Update()
	{
		if (_updateLogCount < 5 && (DateTime.Now - _lastUpdateLog).TotalSeconds >= 3.0)
		{
			_updateLogCount++;
			ModEntry.Log("[Update tick " + _updateLogCount + "] deltaTime=" + Time.deltaTime.ToString("F4") + " unscaled=" + Time.unscaledDeltaTime.ToString("F4") + " realTime=" + Time.realtimeSinceStartup.ToString("F1") + " timescale=" + Time.timeScale.ToString("F2"));
			_lastUpdateLog = DateTime.Now;
		}
	}

	private void OnApplicationPause(bool paused)
	{
		ModEntry.Log("OnApplicationPause: " + paused);
	}

	private void OnDestroy()
	{
		ModEntry.Log("OnDestroy — MonoBehaviour destroyed");
	}

	private void ShowOverlay(string message)
	{
		ModEntry.Log("OVERLAY: " + message.Replace("\n", " | "));
		_overlayMessage = message;
		_overlayOpacity = 1f;
		((MonoBehaviour)this).StartCoroutine(FadeOverlay());
	}

	private IEnumerator FadeOverlay()
	{
		yield return (object)new WaitForSeconds(6f);
		float t = 4f;
		while (t > 0f)
		{
			_overlayOpacity = t / 4f;
			t -= Time.deltaTime;
			yield return null;
		}
		_overlayMessage = null;
		_overlayOpacity = 0f;
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
		if (_overlayMessage != null && !(_overlayOpacity <= 0f))
		{
			GUI.color = new Color(0f, 0f, 0f, 0.75f * _overlayOpacity);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, 140f), (Texture)(object)Texture2D.whiteTexture);
			GUI.color = new Color(1f, 0.35f, 0.35f, _overlayOpacity);
			GUIStyle val = new GUIStyle(GUI.skin.label);
			val.fontSize = Mathf.Max(22, Screen.width / 22);
			val.alignment = (TextAnchor)4;
			val.wordWrap = true;
			GUI.Label(new Rect(20f, 8f, (float)Screen.width - 40f, 124f), "[CNR-Mod] " + _overlayMessage, val);
			GUI.color = Color.white;
		}
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
		SetMember(t, settings, "ServerAddress", ServerIp);
		SetMember(t, settings, "ServerPort", 5055);
		SetMember(t, settings, "AppID", "CNRLan");
		SetMember(t, settings, "HostType", 2);
		ModEntry.Log("Override applied → " + ServerIp + ":5055  AppID=CNRLan");
		CallStaticVoid("PhotonNetwork", "Disconnect");
		ModEntry.Log("Disconnect() called");
		float timeout = 8f;
		while (timeout > 0f && GetConnectionState() != 0)
		{
			timeout -= Time.unscaledDeltaTime;
			yield return null;
		}
		ModEntry.Log("Photon disconnected, swapping to TCP then reconnecting");
		SwapToTcp();
		DisableEncryption();
		ModEntry.Log("About to call ConnectUsingSettings...");
		bool connectCallSucceeded = false;
		try
		{
			CallStaticWithArg("PhotonNetwork", "ConnectUsingSettings", "v2.4");
			connectCallSucceeded = true;
			ModEntry.Log("ConnectUsingSettings() returned OK → connecting to " + ServerIp + ":5055");
		}
		catch (Exception ex)
		{
			ModEntry.Log("ConnectUsingSettings() THREW: " + ex.Message + "\n" + ex.StackTrace);
		}
		if (!connectCallSucceeded)
		{
			ModEntry.Log("ERROR: ConnectUsingSettings failed — giving up");
			ShowOverlay("LAN connect call FAILED.\n" + ServerIp + ":5055\nSee /CNRMods/redir.log");
			yield break;
		}
		ModEntry.Log("Monitoring connection state (30s timeout)...");
		float connectTimeout = 30f;
		float heartbeatTimer = 0f;
		int lastDetailed = -999;
		while (connectTimeout > 0f)
		{
			int connState = -1;
			int detailState = -1;
			try
			{
				connState = GetConnectionState();
			}
			catch (Exception ex)
			{
				ModEntry.Log("GetConnectionState threw: " + ex.Message);
			}
			try
			{
				detailState = GetDetailedState();
			}
			catch (Exception ex)
			{
				ModEntry.Log("GetDetailedState threw: " + ex.Message);
			}
			if (detailState != lastDetailed)
			{
				ModEntry.Log("connState=" + connState + "  detailState=" + detailState + "  (" + (30f - connectTimeout).ToString("F1") + "s elapsed)  runInBg=" + Application.runInBackground);
				lastDetailed = detailState;
			}
			heartbeatTimer -= Time.unscaledDeltaTime;
			if (heartbeatTimer <= 0f)
			{
				ModEntry.Log("[heartbeat] connState=" + connState + "  detailState=" + detailState + "  (" + (30f - connectTimeout).ToString("F1") + "s elapsed)  runInBg=" + Application.runInBackground + "  dt=" + Time.unscaledDeltaTime.ToString("F4"));
				heartbeatTimer = 3f;
			}
			if (detailState == 0 || connState == 0)
			{
				ModEntry.Log("Connection FAILED — Disconnected after " + (30f - connectTimeout).ToString("F1") + "s");
				ShowOverlay("LAN server unreachable.\n" + ServerIp + ":5055\nCheck server is running.");
				yield break;
			}
			if (detailState >= 6)
			{
				ModEntry.Log("Connected to lobby! detailState=" + detailState + " after " + (30f - connectTimeout).ToString("F1") + "s");
				yield break;
			}
			connectTimeout -= Time.unscaledDeltaTime;
			yield return null;
		}
		ModEntry.Log("Connection TIMED OUT after 30s (last detailState=" + lastDetailed + ") — forcing Disconnect()");
		CallStaticVoid("PhotonNetwork", "Disconnect");
		ShowOverlay("LAN connection timed out.\n" + ServerIp + ":5055\ndetailState was " + lastDetailed + " — is server running?");
	}

	private static void DisableEncryption()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("PhotonNetwork");
			if (type == null)
			{
				continue;
			}
			FieldInfo field = type.GetField("networkingPeer", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				continue;
			}
			object value = field.GetValue(null);
			if (value != null)
			{
				FieldInfo field2 = value.GetType().GetField("requestSecurity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field2 != null)
				{
					field2.SetValue(value, false);
					ModEntry.Log("requestSecurity = false → will use direct OpAuthenticate (no DH)");
					return;
				}
			}
		}
		ModEntry.Log("WARNING: requestSecurity field not found — encryption may block auth");
	}

	private static object GetPhotonServerSettings()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("PhotonNetwork");
			if (type == null)
			{
				continue;
			}
			PropertyInfo property = type.GetProperty("PhotonServerSettings", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				object value = property.GetValue(null, null);
				if (value != null)
				{
					return value;
				}
			}
			FieldInfo field = type.GetField("PhotonServerSettings", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
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
			if (type != null)
			{
				PropertyInfo property = type.GetProperty("connectionState", BindingFlags.Static | BindingFlags.Public);
				if (property != null)
				{
					object value = property.GetValue(null, null);
					return Convert.ToInt32(value);
				}
				FieldInfo field = type.GetField("connectionState", BindingFlags.Static | BindingFlags.Public);
				if (field != null)
				{
					return Convert.ToInt32(field.GetValue(null));
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
			if (type != null)
			{
				PropertyInfo property = type.GetProperty("connectionStateDetailed", BindingFlags.Static | BindingFlags.Public);
				if (property != null)
				{
					object value = property.GetValue(null, null);
					return Convert.ToInt32(value);
				}
			}
		}
		return -1;
	}

	private static void CallStaticVoid(string typeName, string methodName)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType(typeName);
			if (type != null)
			{
				MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
				if (method != null)
				{
					method.Invoke(null, null);
					return;
				}
			}
		}
		ModEntry.Log("CallStaticVoid: " + typeName + "." + methodName + " not found");
	}

	private static void CallStaticWithArg(string typeName, string methodName, object arg)
	{
		try
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type type = assembly.GetType(typeName);
				if (type == null)
				{
					continue;
				}
				MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
				foreach (MethodInfo methodInfo in methods)
				{
					if (!(methodInfo.Name != methodName))
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
			ModEntry.Log("CallStaticWithArg: " + typeName + "." + methodName + " not found");
		}
		catch (Exception ex)
		{
			ModEntry.Log("CallStaticWithArg error: " + ex.Message);
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
				if (type != null)
				{
					break;
				}
			}
			if (type == null)
			{
				ModEntry.Log("SwapToTcp: PhotonNetwork not found");
				return;
			}
			FieldInfo field = type.GetField("networkingPeer", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				ModEntry.Log("SwapToTcp: networkingPeer field not found");
				return;
			}
			object value = field.GetValue(null);
			if (value == null)
			{
				ModEntry.Log("SwapToTcp: networkingPeer is null");
				return;
			}
			FieldInfo fieldInfo = null;
			for (Type type2 = value.GetType(); type2 != null; type2 = type2.BaseType)
			{
				fieldInfo = type2.GetField("peerBase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (fieldInfo != null)
				{
					break;
				}
			}
			if (fieldInfo == null)
			{
				ModEntry.Log("SwapToTcp: peerBase field not found");
				return;
			}
			object value2 = fieldInfo.GetValue(value);
			if (value2 != null)
			{
				ModEntry.Log("SwapToTcp: current peerBase is " + value2.GetType().Name);
			}
			Type type3 = null;
			assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				type3 = assembly.GetType("ExitGames.Client.Photon.TPeer");
				if (type3 == null)
				{
					type3 = assembly.GetType("TPeer");
				}
				if (type3 != null)
				{
					break;
				}
			}
			if (type3 == null)
			{
				ModEntry.Log("SwapToTcp: TPeer type not found");
				return;
			}
			object obj = Activator.CreateInstance(type3, nonPublic: true);
			for (Type type4 = type3; type4 != null; type4 = type4.BaseType)
			{
				FieldInfo field2 = type4.GetField("usedProtocol", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field2 != null)
				{
					field2.SetValue(obj, Enum.ToObject(field2.FieldType, (byte)1));
					ModEntry.Log("SwapToTcp: usedProtocol = Tcp");
					break;
				}
			}
			bool flag = false;
			for (Type type5 = type3; type5 != null; type5 = type5.BaseType)
			{
				PropertyInfo property = type5.GetProperty("Listener", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null)
				{
					MethodInfo setMethod = property.GetSetMethod(nonPublic: true);
					if (setMethod != null)
					{
						setMethod.Invoke(obj, new object[1] { value });
						ModEntry.Log("SwapToTcp: Listener set via property setter = " + value.GetType().Name);
						flag = true;
					}
					break;
				}
			}
			if (!flag)
			{
				string[] array = new string[4] { "<Listener>k__BackingField", "listener", "_listener", "Listener" };
				Type type6 = type3;
				while (type6 != null && !flag)
				{
					string[] array2 = array;
					foreach (string text in array2)
					{
						FieldInfo field3 = type6.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (field3 != null)
						{
							field3.SetValue(obj, value);
							ModEntry.Log("SwapToTcp: Listener set via field '" + text + "' = " + value.GetType().Name);
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
			if (!flag)
			{
				ModEntry.Log("SwapToTcp: WARNING — could not find Listener property/field on TPeer hierarchy!");
			}
			fieldInfo.SetValue(value, obj);
			ModEntry.Log("SwapToTcp: peerBase swapped to TPeer - client will connect via TCP");
		}
		catch (Exception ex)
		{
			ModEntry.Log("SwapToTcp error: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private static void SetMember(Type t, object inst, string name, object val)
	{
		FieldInfo field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null)
		{
			object obj = (field.FieldType.IsEnum ? Enum.ToObject(field.FieldType, Convert.ChangeType(val, Enum.GetUnderlyingType(field.FieldType))) : Convert.ChangeType(val, field.FieldType));
			field.SetValue(inst, obj);
			ModEntry.Log("  " + name + " = " + obj);
			return;
		}
		PropertyInfo property = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && property.CanWrite)
		{
			property.SetValue(inst, Convert.ChangeType(val, property.PropertyType), null);
			ModEntry.Log("  " + name + " = " + val);
		}
	}
}
