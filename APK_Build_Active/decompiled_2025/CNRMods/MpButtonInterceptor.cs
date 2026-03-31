using System.IO;
using UnityEngine;

namespace CNRMods;

public class MpButtonInterceptor : MonoBehaviour
{
	public EconomyHook hook;

	private void OnClick()
	{
		bool flag = File.Exists("/sdcard/CNRMods/CNRMod.dll");
		bool flag2 = File.Exists("/sdcard/CNRMods/CNRSettingsMod.dll");
		bool flag3 = File.Exists("/sdcard/CNRMods/CNRModManager.dll");
		if (flag && flag2 && flag3)
		{
			Application.LoadLevel("MultiPlayerSelect");
		}
		else if ((Object)(object)hook != (Object)null)
		{
			hook.ShowMpMissingDialog(flag, flag2, flag3);
		}
	}
}
