using System;
using UnityEngine;

namespace CNRMods;

public static class CustomMapsEntry
{
	public static void Initialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		try
		{
			GameObject val = new GameObject("CNRCustomMaps");
			val.AddComponent<CustomMapsHook>();
			Object.DontDestroyOnLoad((Object)(object)val);
			ModEntry.Log("CustomMaps mod initialized");
		}
		catch (Exception ex)
		{
			ModEntry.Log("CustomMaps init failed: " + ex);
		}
	}
}
