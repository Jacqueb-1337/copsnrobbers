using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRMods;

public class ModEntry
{
	private const string LogPath = "/storage/emulated/0/CNRMods/redir.log";

	private const string ConfigPath = "/storage/emulated/0/CNRMods/server.cfg";

	public static void Load()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		Log("=== IPRedirectMod Load() ===");
		try
		{
			string text = ReadServerIp();
			if (string.IsNullOrEmpty(text))
			{
				Log("No SERVER_IP in server.cfg — aborting");
				return;
			}
			Log("Creating redirect MonoBehaviour for IP: " + text);
			GameObject val = new GameObject("CNRIPRedirect");
			RedirectHook redirectHook = val.AddComponent<RedirectHook>();
			redirectHook.ServerIp = text;
			Object.DontDestroyOnLoad((Object)(object)val);
		}
		catch (Exception ex)
		{
			Log("Load() exception: " + ex);
		}
		CustomMapsEntry.Initialize();
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
				if (fileName.Equals("IPRedirectMod.dll", StringComparison.OrdinalIgnoreCase))
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
						if (method != null)
						{
							method.Invoke(null, null);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Log("LoadExternalMods: no Load() entry point in " + fileName);
					}
				}
				catch (Exception ex)
				{
					Log("LoadExternalMods: error loading " + fileName + ": " + ex.Message);
				}
			}
		}
		catch (Exception ex)
		{
			Log("LoadExternalMods: " + ex.Message);
		}
	}

	private static string ReadServerIp()
	{
		if (!File.Exists("/storage/emulated/0/CNRMods/server.cfg"))
		{
			Log("server.cfg not found at /storage/emulated/0/CNRMods/server.cfg");
			return null;
		}
		string[] array = File.ReadAllLines("/storage/emulated/0/CNRMods/server.cfg");
		foreach (string text in array)
		{
			string text2 = text.Trim();
			if (text2.StartsWith("SERVER_IP="))
			{
				return text2.Substring("SERVER_IP=".Length).Trim();
			}
		}
		return null;
	}

	public static void Log(string msg)
	{
		try
		{
			File.AppendAllText("/storage/emulated/0/CNRMods/redir.log", "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n");
		}
		catch
		{
		}
		try
		{
			Debug.Log((object)("[IPRedirect] " + msg));
		}
		catch
		{
		}
	}
}
