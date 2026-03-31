using UnityEngine;

namespace CNRMods;

public class MapNavButton : MonoBehaviour
{
	public bool isNext;

	public CustomMapsHook hook;

	private void OnClick()
	{
		if ((Object)(object)hook != (Object)null)
		{
			if (isNext)
			{
				hook.OnNextMap();
			}
			else
			{
				hook.OnPreMap();
			}
		}
	}
}
