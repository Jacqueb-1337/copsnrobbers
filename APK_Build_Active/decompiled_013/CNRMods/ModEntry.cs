using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRMods;

public class ModEntry
{
	private const string LogPath = "/storage/emulated/0/CNRMods/cnrmod.log";

	private const string ConfigPath = "/storage/emulated/0/CNRMods/server.cfg";

	public const string Version = "2.0.13";

	public static string ServerIp = "";

	public static int ServerPort = 5055;

	public static string AppId = "CNRLan";

	public static string MapUrl = "";

	public static string ModVersion = "2.0.7";

	public static bool KickNoMod = true;

	public static string WebUrl = "";

	public static string EconomyUrl = "";

	public static bool IsMaster = false;

	public static Dictionary<string, string> RegisteredMods = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static bool _loaded = false;

	public static void RegisterMod(string name, string version)
	{
		if (!string.IsNullOrEmpty(name))
		{
			RegisteredMods[name] = version ?? "?";
			Log("Mod registered: " + name + " v" + (version ?? "?"));
		}
	}

	public static string GetModVersion(string name)
	{
		string value;
		return RegisteredMods.TryGetValue(name, out value) ? value : null;
	}

	public static void Load()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		if (_loaded)
		{
			Log("CNRMod: already loaded, skipping");
			return;
		}
		_loaded = true;
		RegisterMod("CNRMod", "2.0.13");
		Log("=== CNRMod Load() v2.0.13 ===");
		try
		{
			ReadConfig();
			GameObject val = new GameObject("CNRMod_Root");
			val.AddComponent<RedirectHook>();
			val.AddComponent<CustomMapsHook>();
			val.AddComponent<MapLoader>();
			val.AddComponent<ContentManager>();
			val.AddComponent<EconomyHook>();
			Object.DontDestroyOnLoad((Object)(object)val);
			Log("Mod root created.  IP=" + ((ServerIp != "") ? ServerIp : "(none)") + "  MOD_VERSION=" + ModVersion + "  KICK_NO_MOD=" + KickNoMod);
			PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
			PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
			PlayerPrefs.Save();
			Log("Startup: cleared stale map prefs");
		}
		catch (Exception ex)
		{
			Log("Load() error: " + ex);
		}
		LoadExternalMods();
	}

	private static void LoadExternalMods()
	{
		try
		{
			string[] files = Directory.GetFiles("/storage/emulated/0/CNRMods", "*.dll");
			string[] array = files;
			foreach (string path in array)
			{
				string fileName = Path.GetFileName(path);
				if (fileName.Equals("CNRMod.dll", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				try
				{
					Log("LoadExternalMods: loading " + fileName);
					byte[] rawAssembly = File.ReadAllBytes(path);
					Assembly assembly = Assembly.Load(rawAssembly);
					bool flag = false;
					Type[] types = assembly.GetTypes();
					foreach (Type type in types)
					{
						MethodInfo method = type.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
						if ((object)method != null)
						{
							method.Invoke(null, null);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Log("LoadExternalMods: no Load() in " + fileName);
					}
				}
				catch (Exception ex)
				{
					Log("LoadExternalMods: error in " + fileName + ": " + ex.Message);
				}
			}
		}
		catch (Exception ex2)
		{
			Log("LoadExternalMods: " + ex2.Message);
		}
	}

	private static void ReadConfig()
	{
		if (!File.Exists("/storage/emulated/0/CNRMods/server.cfg"))
		{
			Log("No server.cfg found");
			return;
		}
		string[] array = File.ReadAllLines("/storage/emulated/0/CNRMods/server.cfg");
		foreach (string text in array)
		{
			string text2 = text.Trim();
			if (string.IsNullOrEmpty(text2) || text2.StartsWith("#"))
			{
				continue;
			}
			int num = text2.IndexOf('=');
			if (num < 0)
			{
				continue;
			}
			string text3 = text2.Substring(0, num).Trim().ToUpperInvariant();
			string text4 = text2.Substring(num + 1).Trim();
			switch (text3)
			{
			case "SERVER_IP":
				ServerIp = text4;
				break;
			case "SERVER_PORT":
			{
				if (int.TryParse(text4, out var result))
				{
					ServerPort = result;
				}
				break;
			}
			case "APP_ID":
				AppId = text4;
				break;
			case "MAP_URL":
				MapUrl = text4;
				break;
			case "MOD_VERSION":
				ModVersion = text4;
				break;
			case "KICK_NO_MOD":
				KickNoMod = text4.ToLower() != "false" && text4 != "0";
				break;
			case "WEB_URL":
				WebUrl = text4;
				break;
			case "ECONOMY_URL":
				EconomyUrl = text4;
				break;
			}
		}
		Log("Config: IP=" + ServerIp + "  PORT=" + ServerPort + "  MAP_URL=" + MapUrl + "  VERSION=" + ModVersion + "  KICK=" + KickNoMod);
		if (string.IsNullOrEmpty(WebUrl) && !string.IsNullOrEmpty(ServerIp))
		{
			WebUrl = "http://" + ServerIp + ":1337";
		}
		Log("WebUrl=" + ((WebUrl != "") ? WebUrl : "(not set)"));
		if (string.IsNullOrEmpty(EconomyUrl))
		{
			EconomyUrl = "https://play.jacqueb.me/economy";
		}
		Log("EconomyUrl=" + EconomyUrl);
	}

	public static void Log(string msg)
	{
		try
		{
			File.AppendAllText("/storage/emulated/0/CNRMods/cnrmod.log", "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n");
		}
		catch
		{
		}
		try
		{
			Debug.Log((object)("[CNRMod] " + msg));
		}
		catch
		{
		}
	}

	public static string SanitizeUrl(string url)
	{
		if (url != null && url.StartsWith("https://") && url.Contains(":1337"))
		{
			url = "http://" + url.Substring(8);
		}
		return url;
	}

	public static string ParseJsonStringValue(string json, string key)
	{
		try
		{
			string text = "\"" + key + "\":";
			int num = json.IndexOf(text);
			if (num < 0)
			{
				return null;
			}
			int num2 = json.IndexOf('"', num + text.Length);
			if (num2 < 0)
			{
				return null;
			}
			int num3 = json.IndexOf('"', num2 + 1);
			if (num3 < 0)
			{
				return null;
			}
			return json.Substring(num2 + 1, num3 - num2 - 1).Replace("\\n", "").Replace("\\/", "/");
		}
		catch
		{
			return null;
		}
	}

	public static string ParseJsonValue(string json, string key)
	{
		try
		{
			string text = "\"" + key + "\":";
			int num = json.IndexOf(text);
			if (num < 0)
			{
				return null;
			}
			int i;
			for (i = num + text.Length; i < json.Length && json[i] == ' '; i++)
			{
			}
			if (i >= json.Length)
			{
				return null;
			}
			if (json[i] == '"')
			{
				int num2 = json.IndexOf('"', i + 1);
				if (num2 < 0)
				{
					return null;
				}
				return json.Substring(i + 1, num2 - i - 1).Replace("\\/", "/");
			}
			int j;
			for (j = i; j < json.Length && json[j] != ',' && json[j] != '}' && json[j] != ']'; j++)
			{
			}
			return json.Substring(i, j - i).Trim();
		}
		catch
		{
			return null;
		}
	}
}
