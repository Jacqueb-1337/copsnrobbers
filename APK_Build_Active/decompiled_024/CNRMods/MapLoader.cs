using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace CNRMods;

public class MapLoader : MonoBehaviour
{
	private class RespawnWatcher : MonoBehaviour
	{
		public Vector3 EscapeSpawn;

		public Vector3 EnemySpawn;

		public bool HasEscape;

		public bool HasEnemy;

		public Vector3 MapCentroid;

		private GameObject _isDiedObj;

		private bool _wasDeadLastFrame;

		private Vector3 GetMySpawn()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			if (ModEntry.IsMaster && HasEscape)
			{
				return EscapeSpawn;
			}
			if (!ModEntry.IsMaster && HasEnemy)
			{
				return EnemySpawn;
			}
			if (HasEscape)
			{
				return EscapeSpawn;
			}
			if (HasEnemy)
			{
				return EnemySpawn;
			}
			return Vector3.zero;
		}

		private void DoTeleport(Vector3 pos)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = GameObject.Find("ExampleCharacter");
			if (!((Object)(object)val == (Object)null))
			{
				CharacterController component = val.GetComponent<CharacterController>();
				if ((Object)(object)component != (Object)null)
				{
					((Collider)component).enabled = false;
				}
				val.transform.position = pos;
				Vector3 val2 = default(Vector3);
				((Vector3)(ref val2))._002Ector(MapCentroid.x - pos.x, 0f, MapCentroid.z - pos.z);
				if (((Vector3)(ref val2)).sqrMagnitude > 1f)
				{
					val.transform.rotation = Quaternion.LookRotation(((Vector3)(ref val2)).normalized, Vector3.up);
				}
				if ((Object)(object)component != (Object)null)
				{
					((Collider)component).enabled = true;
				}
			}
		}

		private void Update()
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_isDiedObj == (Object)null)
			{
				_isDiedObj = GameObject.Find("IsDied");
			}
			bool flag = (Object)(object)_isDiedObj != (Object)null && _isDiedObj.activeSelf;
			if (_wasDeadLastFrame && !flag)
			{
				Vector3 mySpawn = GetMySpawn();
				if (mySpawn != Vector3.zero)
				{
					DoTeleport(mySpawn);
					ModEntry.Log("RespawnWatcher: respawned, teleported to " + mySpawn);
				}
			}
			_wasDeadLastFrame = flag;
		}
	}

	private const string CachePath = "/storage/emulated/0/CNRMods/custom_map_cache.json";

	private static readonly string[] BASE_SCENES = new string[3] { "FreeRun3_1", "FreeRun5_1", "FreeRun8_1" };

	private static readonly string[] SKIP_CONTAINS = new string[5] { "_UIDrawCall", "ExampleCharacter", "IsDied", "IsPause", "IsFireOnline" };

	private static readonly string[] SKIP_EXACT = new string[5] { "Cube", "Sphere", "Plane", "Cylinder", "Capsule" };

	private GameObject _mapRoot = null;

	private bool _spawnRunning = false;

	private bool _holdingPlayer = false;

	private bool _clientFetchStarted = false;

	private static readonly Vector3 LOADING_POS = new Vector3(0f, 4800f, 0f);

	private GameObject _loadingRoom = null;

	public void EnsureHolding()
	{
		if (Array.IndexOf(BASE_SCENES, Application.loadedLevelName) >= 0 && !_holdingPlayer)
		{
			_spawnRunning = false;
			_holdingPlayer = true;
			((MonoBehaviour)this).StartCoroutine(HoldAtLoadingPos());
			ModEntry.Log("MapLoader: EnsureHolding — late OnEnteredRoom, restarting hold");
			((MonoBehaviour)this).StartCoroutine(WaitAndSpawn());
		}
	}

	public void AbortHold()
	{
		if (_holdingPlayer || _spawnRunning)
		{
			ModEntry.Log("MapLoader: aborting hold (left room)");
			((MonoBehaviour)this).StopAllCoroutines();
			_holdingPlayer = false;
			_spawnRunning = false;
			if ((Object)(object)_loadingRoom != (Object)null)
			{
				Object.Destroy((Object)(object)_loadingRoom);
				_loadingRoom = null;
			}
		}
	}

	private void BuildLoadingRoom()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_loadingRoom != (Object)null)
		{
			Object.Destroy((Object)(object)_loadingRoom);
			_loadingRoom = null;
		}
		_loadingRoom = new GameObject("[CNRMod_Loading]");
		float x = LOADING_POS.x;
		float y = LOADING_POS.y;
		float z = LOADING_POS.z;
		AddFaceSlab(_loadingRoom, new Vector3(x, y - 2f, z), new Vector3(8f, 0.3f, 8f));
		AddFaceSlab(_loadingRoom, new Vector3(x, y + 2f, z), new Vector3(8f, 0.3f, 8f));
		AddFaceSlab(_loadingRoom, new Vector3(x + 4f, y, z), new Vector3(0.3f, 4f, 8f));
		AddFaceSlab(_loadingRoom, new Vector3(x - 4f, y, z), new Vector3(0.3f, 4f, 8f));
		AddFaceSlab(_loadingRoom, new Vector3(x, y, z + 4f), new Vector3(8f, 4f, 0.3f));
		AddFaceSlab(_loadingRoom, new Vector3(x, y, z - 4f), new Vector3(8f, 4f, 0.3f));
	}

	private static void TeleportPlayer(Vector3 pos)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GameObject.Find("ExampleCharacter");
		if (!((Object)(object)val == (Object)null))
		{
			CharacterController component = val.GetComponent<CharacterController>();
			if ((Object)(object)component != (Object)null)
			{
				((Collider)component).enabled = false;
			}
			val.transform.position = pos;
			if ((Object)(object)component != (Object)null)
			{
				((Collider)component).enabled = true;
			}
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		if (Array.IndexOf(BASE_SCENES, Application.loadedLevelName) < 0)
		{
			return;
		}
		if ((Object)(object)_mapRoot != (Object)null)
		{
			Object.Destroy((Object)(object)_mapRoot);
			_mapRoot = null;
		}
		_spawnRunning = false;
		_clientFetchStarted = false;
		string text = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "");
		bool flag = PlayerPrefs.GetInt("CNRMod_MapCacheReady", 0) == 1 && File.Exists("/storage/emulated/0/CNRMods/custom_map_cache.json");
		if (string.IsNullOrEmpty(text) && !flag && !ModEntry.FetchingRoomMap)
		{
			if (string.IsNullOrEmpty(ModEntry.WebUrl))
			{
				ModEntry.Log("MapLoader: vanilla map load, no custom map pending — skipping hold");
				return;
			}
			ModEntry.FetchingRoomMap = true;
			ModEntry.Log("MapLoader: WebUrl configured — pre-holding for potential custom map join");
		}
		if (!string.IsNullOrEmpty(text))
		{
			RedirectHook component = ((Component)this).GetComponent<RedirectHook>();
			if ((Object)(object)component != (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(component.PublishMapUrlRetry(text));
			}
		}
		_holdingPlayer = true;
		((MonoBehaviour)this).StartCoroutine(HoldAtLoadingPos());
		ModEntry.Log("MapLoader: entered base scene, waiting for map data...");
		((MonoBehaviour)this).StartCoroutine(WaitAndSpawn());
	}

	private IEnumerator HoldAtLoadingPos()
	{
		Vector3 holdPos = new Vector3(LOADING_POS.x, LOADING_POS.y, LOADING_POS.z);
		while (_holdingPlayer)
		{
			TeleportPlayer(holdPos);
			yield return null;
		}
	}

	private IEnumerator WaitAndSpawn()
	{
		if (_spawnRunning)
		{
			yield break;
		}
		_spawnRunning = true;
		if (PlayerPrefs.GetInt("CNRMod_MapCacheReady", 0) == 1 && File.Exists("/storage/emulated/0/CNRMods/custom_map_cache.json"))
		{
			ModEntry.Log("MapLoader: cache ready immediately");
			((MonoBehaviour)this).StartCoroutine(SpawnAfterDelay());
			yield break;
		}
		float waited = 0f;
		while (waited < 30f)
		{
			yield return (object)new WaitForSeconds(0.5f);
			waited += 0.5f;
			if (PlayerPrefs.GetInt("CNRMod_MapCacheReady", 0) == 1 && File.Exists("/storage/emulated/0/CNRMods/custom_map_cache.json"))
			{
				ModEntry.Log("MapLoader: cache ready after " + waited.ToString("F1") + "s");
				((MonoBehaviour)this).StartCoroutine(SpawnAfterDelay());
				yield break;
			}
			if (!(waited >= 3f))
			{
				continue;
			}
			string text = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "");
			if (!string.IsNullOrEmpty(text))
			{
				ModEntry.Log("MapLoader: 3s timeout, direct download from " + text);
				((MonoBehaviour)this).StartCoroutine(DownloadAndSpawn(text));
				yield break;
			}
			if (!ModEntry.FetchingRoomMap)
			{
				ModEntry.Log("MapLoader: room fetch completed with no custom map -- releasing hold");
				_holdingPlayer = false;
				_spawnRunning = false;
				yield break;
			}
			if (!_clientFetchStarted)
			{
				_clientFetchStarted = true;
				RedirectHook component = ((Component)this).GetComponent<RedirectHook>();
				if (!((Object)(object)component != (Object)null))
				{
					ModEntry.Log("MapLoader: no RedirectHook — releasing hold");
					ModEntry.FetchingRoomMap = false;
					_holdingPlayer = false;
					_spawnRunning = false;
					yield break;
				}
				ModEntry.Log("MapLoader: 3s — proactively calling TriggerClientFetch");
				component.TriggerClientFetch();
			}
		}
		ModEntry.Log("MapLoader: timed out 30s with no map data");
		_holdingPlayer = false;
		_spawnRunning = false;
	}

	private IEnumerator DownloadAndSpawn(string url)
	{
		url = ModEntry.SanitizeUrl(url);
		WWW www = new WWW(url);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			ModEntry.Log("MapLoader download error: " + www.error);
			_spawnRunning = false;
			yield break;
		}
		string json = www.text;
		if (string.IsNullOrEmpty(json))
		{
			ModEntry.Log("MapLoader: empty response");
			_spawnRunning = false;
			yield break;
		}
		try
		{
			File.WriteAllText("/storage/emulated/0/CNRMods/custom_map_cache.json", json);
			PlayerPrefs.SetInt("CNRMod_MapCacheReady", 1);
			PlayerPrefs.Save();
			ModEntry.Log("MapLoader: cached (" + json.Length + " bytes)");
		}
		catch (Exception ex)
		{
			ModEntry.Log("MapLoader cache error: " + ex.Message);
			_spawnRunning = false;
			yield break;
		}
		((MonoBehaviour)this).StartCoroutine(SpawnAfterDelay());
	}

	private IEnumerator SpawnAfterDelay()
	{
		yield return (object)new WaitForSeconds(0.5f);
		ModEntry.Log("MapLoader: building map (player held at loading pos)");
		try
		{
			string text = File.ReadAllText("/storage/emulated/0/CNRMods/custom_map_cache.json");
			ModEntry.Log("MapLoader: parsing " + text.Length + " bytes");
			string text2 = text.Trim();
			MapObjData[] array;
			if (text2.StartsWith("{"))
			{
				string text3 = ModEntry.ParseJsonStringValue(text2, "donor");
				if (!string.IsNullOrEmpty(text3))
				{
					ModEntry.Log("MapLoader: donor=" + text3);
				}
				int num = text2.IndexOf("\"objects\"");
				num = ((num >= 0) ? text2.IndexOf('[', num) : (-1));
				if (num < 0)
				{
					ModEntry.Log("MapLoader: no objects array in wrapper");
					yield break;
				}
				int num2 = 0;
				int num3 = num;
				for (int i = num; i < text2.Length; i++)
				{
					if (text2[i] == '[')
					{
						num2++;
					}
					else if (text2[i] == ']')
					{
						num2--;
						if (num2 == 0)
						{
							num3 = i;
							break;
						}
					}
				}
				array = JsonReader.Deserialize<MapObjData[]>(text2.Substring(num, num3 - num + 1));
			}
			else
			{
				array = JsonReader.Deserialize<MapObjData[]>(text2);
			}
			if (array == null || array.Length == 0)
			{
				ModEntry.Log("MapLoader: JSON parse failed or empty");
				yield break;
			}
			Dictionary<string, Material> dictionary = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
			Renderer[] array2 = (Renderer[])(object)Object.FindObjectsOfType(typeof(Renderer));
			foreach (Renderer val in array2)
			{
				if ((Object)(object)val == (Object)null || (Object)(object)val.sharedMaterial == (Object)null)
				{
					continue;
				}
				string text4 = ((Object)val.sharedMaterial).name;
				if (!string.IsNullOrEmpty(text4))
				{
					int num4 = text4.IndexOf(" (Instance)", StringComparison.OrdinalIgnoreCase);
					if (num4 >= 0)
					{
						text4 = text4.Substring(0, num4);
					}
					text4 = text4.Trim();
					if (!string.IsNullOrEmpty(text4) && !dictionary.ContainsKey(text4))
					{
						dictionary[text4] = val.sharedMaterial;
					}
				}
			}
			ModEntry.Log("MapLoader: " + dictionary.Count + " scene materials: " + string.Join(", ", new List<string>(dictionary.Keys).ToArray()));
			_mapRoot = new GameObject("[CustomMap]");
			int num5 = 0;
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			MapObjData[] array3 = array;
			foreach (MapObjData mapObjData in array3)
			{
				if (ShouldSkip(mapObjData.path) || mapObjData.path.Contains("EscapePosition") || mapObjData.path.Contains("EnemyPosition") || mapObjData.path.Contains("PlayerPosition") || (!string.IsNullOrEmpty(mapObjData.mesh) && mapObjData.mesh.IndexOf("Combined", StringComparison.OrdinalIgnoreCase) >= 0))
				{
					continue;
				}
				GameObject val2 = GameObject.Find(mapObjData.path);
				if ((Object)(object)val2 == (Object)null)
				{
					string b = (mapObjData.path.Contains("/") ? mapObjData.path.Substring(mapObjData.path.LastIndexOf('/') + 1) : mapObjData.path);
					GameObject[] array4 = (GameObject[])(object)Object.FindObjectsOfType(typeof(GameObject));
					GameObject[] array5 = array4;
					foreach (GameObject val3 in array5)
					{
						if (string.Equals(((Object)val3).name, b, StringComparison.OrdinalIgnoreCase))
						{
							val2 = val3;
							break;
						}
					}
				}
				if ((Object)(object)val2 == (Object)null)
				{
					ModEntry.Log("MapLoader: not found in scene: " + mapObjData.path);
					continue;
				}
				try
				{
					GameObject val4 = (GameObject)Object.Instantiate((Object)(object)val2);
					((Object)val4).name = mapObjData.path.Replace("/", "_");
					val4.transform.parent = _mapRoot.transform;
					val4.SetActive(true);
					array2 = val4.GetComponentsInChildren<Renderer>(true);
					foreach (Renderer val5 in array2)
					{
						val5.enabled = true;
					}
					Collider[] componentsInChildren = val4.GetComponentsInChildren<Collider>(true);
					foreach (Collider val6 in componentsInChildren)
					{
						Object.Destroy((Object)(object)val6);
					}
					if (mapObjData.pos != null && mapObjData.pos.Length == 3)
					{
						val4.transform.position = new Vector3(mapObjData.pos[0], mapObjData.pos[1], mapObjData.pos[2]);
					}
					if (mapObjData.rot != null && mapObjData.rot.Length >= 3)
					{
						val4.transform.rotation = Quaternion.Euler(mapObjData.rot[0], mapObjData.rot[1], mapObjData.rot[2]);
					}
					Material value = null;
					if (!string.IsNullOrEmpty(mapObjData.mat))
					{
						dictionary.TryGetValue(mapObjData.mat, out value);
					}
					array2 = val4.GetComponentsInChildren<Renderer>(true);
					foreach (Renderer val7 in array2)
					{
						if ((Object)(object)value != (Object)null)
						{
							val7.material = value;
						}
						else if (mapObjData.color != null && mapObjData.color.Length >= 3)
						{
							val7.material.color = new Color(mapObjData.color[0] / 255f, mapObjData.color[1] / 255f, mapObjData.color[2] / 255f, (mapObjData.color.Length >= 4) ? (mapObjData.color[3] / 255f) : 1f);
						}
					}
					bool flag = false;
					if (mapObjData.collidable)
					{
						MeshFilter[] componentsInChildren2 = val4.GetComponentsInChildren<MeshFilter>(true);
						foreach (MeshFilter val8 in componentsInChildren2)
						{
							if ((Object)(object)val8.sharedMesh == (Object)null || val8.sharedMesh.vertexCount < 4)
							{
								continue;
							}
							string text4 = ((Object)val8.sharedMesh).name ?? "";
							if (text4.IndexOf("Combined", StringComparison.OrdinalIgnoreCase) >= 0 || val8.sharedMesh.vertexCount > 8000)
							{
								continue;
							}
							try
							{
								MeshCollider val9 = ((Component)val8).gameObject.AddComponent<MeshCollider>();
								val9.sharedMesh = val8.sharedMesh;
								val9.convex = true;
								flag = true;
							}
							catch (Exception ex)
							{
								ModEntry.Log("MeshCollider failed for " + mapObjData.path + ": " + ex.Message);
								MeshCollider component = ((Component)val8).gameObject.GetComponent<MeshCollider>();
								if ((Object)(object)component != (Object)null)
								{
									Object.Destroy((Object)(object)component);
								}
							}
						}
						if (!flag && mapObjData.size != null && mapObjData.size.Length == 3 && mapObjData.pos != null && mapObjData.pos.Length == 3)
						{
							float num6 = mapObjData.size[0];
							float num7 = mapObjData.size[1];
							float num8 = mapObjData.size[2];
							float num9 = mapObjData.pos[0];
							float num10 = mapObjData.pos[1];
							float num11 = mapObjData.pos[2];
							AddFaceSlab(_mapRoot, new Vector3(num9, num10 + num7 * 0.5f, num11), new Vector3(num6, 0.15f, num8));
							AddFaceSlab(_mapRoot, new Vector3(num9, num10 - num7 * 0.5f, num11), new Vector3(num6, 0.15f, num8));
							AddFaceSlab(_mapRoot, new Vector3(num9 + num6 * 0.5f, num10, num11), new Vector3(0.15f, num7, num8));
							AddFaceSlab(_mapRoot, new Vector3(num9 - num6 * 0.5f, num10, num11), new Vector3(0.15f, num7, num8));
							AddFaceSlab(_mapRoot, new Vector3(num9, num10, num11 + num8 * 0.5f), new Vector3(num6, num7, 0.15f));
							AddFaceSlab(_mapRoot, new Vector3(num9, num10, num11 - num8 * 0.5f), new Vector3(num6, num7, 0.15f));
						}
					}
					hashSet.Add(mapObjData.path);
					num5++;
				}
				catch (Exception ex2)
				{
					ModEntry.Log("Clone failed: " + mapObjData.path + " err: " + ex2.Message);
				}
			}
			ModEntry.Log("MapLoader: cloned " + hashSet.Count + " donor objects");
			ClearBaseScene();
			array3 = array;
			foreach (MapObjData mapObjData in array3)
			{
				if (ShouldSkip(mapObjData.path) || hashSet.Contains(mapObjData.path))
				{
					continue;
				}
				PrimitiveType val10 = MeshToPrimitive(mapObjData.mesh);
				GameObject val3 = GameObject.CreatePrimitive(val10);
				((Object)val3).name = mapObjData.path.Replace("/", "_");
				val3.transform.parent = _mapRoot.transform;
				if ((int)val10 == 3 && mapObjData.size != null && mapObjData.size.Length == 3)
				{
					MeshFilter val8 = val3.GetComponent<MeshFilter>();
					if ((Object)(object)val8 != (Object)null)
					{
						ApplyBoxUVs(val8, Mathf.Max(0.01f, mapObjData.size[0]), Mathf.Max(0.01f, mapObjData.size[1]), Mathf.Max(0.01f, mapObjData.size[2]));
					}
				}
				if (mapObjData.pos != null && mapObjData.pos.Length == 3)
				{
					val3.transform.position = new Vector3(mapObjData.pos[0], mapObjData.pos[1], mapObjData.pos[2]);
				}
				if (mapObjData.size != null && mapObjData.size.Length == 3)
				{
					val3.transform.localScale = new Vector3(Mathf.Max(0.01f, mapObjData.size[0]), Mathf.Max(0.01f, mapObjData.size[1]), Mathf.Max(0.01f, mapObjData.size[2]));
				}
				if (mapObjData.rot != null && mapObjData.rot.Length >= 3)
				{
					val3.transform.rotation = Quaternion.Euler(mapObjData.rot[0], mapObjData.rot[1], mapObjData.rot[2]);
				}
				Renderer component2 = val3.GetComponent<Renderer>();
				if ((Object)(object)component2 != (Object)null)
				{
					Material value2 = null;
					if (!string.IsNullOrEmpty(mapObjData.mat))
					{
						dictionary.TryGetValue(mapObjData.mat, out value2);
					}
					if ((Object)(object)value2 != (Object)null)
					{
						component2.material = value2;
						component2.material.mainTextureScale = Vector2.one;
					}
					else if (mapObjData.color != null && mapObjData.color.Length >= 3)
					{
						component2.material.color = new Color(mapObjData.color[0] / 255f, mapObjData.color[1] / 255f, mapObjData.color[2] / 255f, (mapObjData.color.Length >= 4) ? (mapObjData.color[3] / 255f) : 1f);
					}
				}
				if (mapObjData.path.Contains("EscapePosition") || mapObjData.path.Contains("EnemyPosition") || mapObjData.path.Contains("PlayerPosition"))
				{
					if ((Object)(object)component2 != (Object)null)
					{
						component2.enabled = false;
					}
					Collider component3 = val3.GetComponent<Collider>();
					if ((Object)(object)component3 != (Object)null)
					{
						component3.enabled = false;
					}
				}
				else if (mapObjData.isColBox)
				{
					if ((Object)(object)component2 != (Object)null)
					{
						component2.enabled = false;
					}
				}
				else if (!mapObjData.collidable)
				{
					Collider component3 = val3.GetComponent<Collider>();
					if ((Object)(object)component3 != (Object)null)
					{
						component3.enabled = false;
					}
				}
				num5++;
			}
			ModEntry.Log("MapLoader: spawned " + num5 + " (" + hashSet.Count + " cloned, " + (num5 - hashSet.Count) + " primitives)");
			Vector3? val11 = null;
			Vector3? val12 = null;
			array3 = array;
			Vector3 value3 = default(Vector3);
			foreach (MapObjData mapObjData in array3)
			{
				if (mapObjData.path != null && mapObjData.pos != null && mapObjData.pos.Length >= 3)
				{
					((Vector3)(ref value3))._002Ector(mapObjData.pos[0], mapObjData.pos[1] + 1f, mapObjData.pos[2]);
					if (!val11.HasValue && mapObjData.path.Contains("EscapePosition"))
					{
						val11 = value3;
					}
					if (!val12.HasValue && mapObjData.path.Contains("EnemyPosition"))
					{
						val12 = value3;
					}
				}
			}
			float num12 = 0f;
			float num13 = 0f;
			int num14 = 0;
			float num15 = float.MaxValue;
			array3 = array;
			foreach (MapObjData mapObjData2 in array3)
			{
				if (mapObjData2.pos != null && mapObjData2.pos.Length >= 3 && !ShouldSkip(mapObjData2.path) && (mapObjData2.path == null || !mapObjData2.path.Contains("Position")))
				{
					num12 += mapObjData2.pos[0];
					num13 += mapObjData2.pos[2];
					num14++;
					float num16 = ((mapObjData2.size != null && mapObjData2.size.Length >= 2) ? (Mathf.Abs(mapObjData2.size[1]) * 0.5f) : 0.5f);
					float num17 = mapObjData2.pos[1] + num16;
					if (num17 < num15)
					{
						num15 = num17;
					}
				}
			}
			float num18 = ((num15 < float.MaxValue) ? num15 : 0f) + 1.8f;
			Vector3 mapCentroid = (Vector3)((num14 > 0) ? new Vector3(num12 / (float)num14, num18, num13 / (float)num14) : Vector3.zero);
			RespawnWatcher respawnWatcher = _mapRoot.AddComponent<RespawnWatcher>();
			if (val11.HasValue)
			{
				respawnWatcher.EscapeSpawn = val11.Value;
				respawnWatcher.HasEscape = true;
			}
			if (val12.HasValue)
			{
				respawnWatcher.EnemySpawn = val12.Value;
				respawnWatcher.HasEnemy = true;
			}
			respawnWatcher.MapCentroid = mapCentroid;
			_holdingPlayer = false;
			TeleportToSpawn(array);
		}
		catch (Exception ex3)
		{
			ModEntry.Log("MapLoader error: " + ex3.Message);
			_holdingPlayer = false;
		}
	}

	private static void UpdateRespawnPoints(MapObjData[] items)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			Vector3 item = default(Vector3);
			foreach (MapObjData mapObjData in items)
			{
				if (mapObjData.path != null && mapObjData.pos != null && mapObjData.pos.Length >= 3)
				{
					((Vector3)(ref item))._002Ector(mapObjData.pos[0], mapObjData.pos[1] + 1f, mapObjData.pos[2]);
					if (mapObjData.path.Contains("EscapePosition"))
					{
						list.Add(item);
					}
					else if (mapObjData.path.Contains("EnemyPosition"))
					{
						list2.Add(item);
					}
				}
			}
			if (list.Count == 0 && list2.Count == 0)
			{
				ModEntry.Log("UpdateRespawnPoints: no team markers in JSON, skipping");
				return;
			}
			int num = 0;
			int num2 = 0;
			GameObject[] array = (GameObject[])(object)Object.FindObjectsOfType(typeof(GameObject));
			GameObject[] array2 = array;
			foreach (GameObject val in array2)
			{
				if (!((Object)(object)val == (Object)null))
				{
					string text = ((Object)val).name ?? "";
					if (text.Contains("EscapePosition") && list.Count > 0)
					{
						val.transform.position = list[num % list.Count];
						num++;
					}
					else if (text.Contains("EnemyPosition") && list2.Count > 0)
					{
						val.transform.position = list2[num2 % list2.Count];
						num2++;
					}
					else if ((text.Contains("SpawnPoint") || text.Contains("Spawn_Point")) && (list.Count > 0 || list2.Count > 0))
					{
						List<Vector3> list3 = ((list.Count > 0) ? list : list2);
						int num3 = num + num2;
						val.transform.position = list3[num3 % list3.Count];
						num++;
					}
				}
			}
			ModEntry.Log("UpdateRespawnPoints: escape=" + list.Count + " enemy=" + list2.Count + " moved esc=" + num + " enmIdx=" + num2);
		}
		catch (Exception ex)
		{
			ModEntry.Log("UpdateRespawnPoints error: " + ex.Message);
		}
	}

	private static void TeleportToSpawn(MapObjData[] items)
	{
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string text = (ModEntry.IsMaster ? "EscapePosition" : "EnemyPosition");
			string text2 = "PlayerPosition";
			Vector3? val = null;
			Vector3? val2 = null;
			MapObjData[] array = items;
			Vector3 value = default(Vector3);
			foreach (MapObjData mapObjData in array)
			{
				if (mapObjData.path != null && mapObjData.pos != null && mapObjData.pos.Length >= 3)
				{
					((Vector3)(ref value))._002Ector(mapObjData.pos[0], mapObjData.pos[1] + 1f, mapObjData.pos[2]);
					if (!val.HasValue && mapObjData.path.Contains(text))
					{
						val = value;
					}
					if (!val2.HasValue && mapObjData.path.Contains(text2))
					{
						val2 = value;
					}
				}
			}
			float num = 0f;
			float num2 = 0f;
			int num3 = 0;
			float num4 = float.MaxValue;
			array = items;
			foreach (MapObjData mapObjData in array)
			{
				if (mapObjData.pos != null && mapObjData.pos.Length >= 3 && !ShouldSkip(mapObjData.path) && (mapObjData.path == null || !mapObjData.path.Contains("Position")))
				{
					num += mapObjData.pos[0];
					num2 += mapObjData.pos[2];
					num3++;
					float num5 = ((mapObjData.size != null && mapObjData.size.Length >= 2) ? (Mathf.Abs(mapObjData.size[1]) * 0.5f) : 0.5f);
					float num6 = mapObjData.pos[1] + num5;
					if (num6 < num4)
					{
						num4 = num6;
					}
				}
			}
			float num7 = ((num4 < float.MaxValue) ? num4 : 0f) + 1.8f;
			Vector3 val3 = ((num3 > 0) ? new Vector3(num / (float)num3, num7, num2 / (float)num3) : new Vector3(0f, num7, 0f));
			Vector3 val4 = (Vector3)(((_003F?)val) ?? ((_003F?)val2) ?? val3);
			string text3 = (val.HasValue ? text : (val2.HasValue ? text2 : "centroid"));
			ModEntry.Log(string.Concat("TeleportToSpawn: ", text3, "=", val4, " centroid=", val3, " (isMaster=", ModEntry.IsMaster, ")"));
			GameObject val5 = GameObject.Find("ExampleCharacter");
			if ((Object)(object)val5 == (Object)null)
			{
				ModEntry.Log("TeleportToSpawn: ExampleCharacter not found");
				return;
			}
			CharacterController component = val5.GetComponent<CharacterController>();
			if ((Object)(object)component != (Object)null)
			{
				((Collider)component).enabled = false;
			}
			val5.transform.position = val4;
			Vector3 val6 = default(Vector3);
			((Vector3)(ref val6))._002Ector(val3.x - val4.x, 0f, val3.z - val4.z);
			if (((Vector3)(ref val6)).sqrMagnitude > 1f)
			{
				val5.transform.rotation = Quaternion.LookRotation(((Vector3)(ref val6)).normalized, Vector3.up);
			}
			if ((Object)(object)component != (Object)null)
			{
				((Collider)component).enabled = true;
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("TeleportToSpawn error: " + ex.Message);
		}
	}

	private static void ClearBaseScene()
	{
		string[] keywords = new string[45]
		{
			"Camera", "camera", "Light", "light", "Sun", "Sky", "Fog", "Director", "Manager", "Controller",
			"Audio", "Sound", "Player", "Character", "Spawn", "SpawnPoint", "Canvas", "EventSystem", "UI", "UIRoot",
			"NGUI", "_UIDrawCall", "UIPanel", "UICamera", "UISprite", "UILabel", "Photon", "CNRMod", "[CustomMap]", "ExampleCharacter",
			"IsDied", "IsPause", "InGameMenu", "VCAnalog", "Joystick", "HUD", "Hud", "MainScene", "KamcordPrefab", "CNRSettings",
			"Environment", "Ambient", "Render", "Skybox", "Directional"
		};
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		GameObject[] array = (GameObject[])(object)Object.FindObjectsOfType(typeof(GameObject));
		GameObject[] array2 = array;
		foreach (GameObject val in array2)
		{
			if ((Object)(object)val.transform.parent != (Object)null)
			{
				continue;
			}
			if (ShouldPreserveRoot(((Object)val).name, keywords))
			{
				if (stringBuilder2.Length < 300)
				{
					stringBuilder2.Append(((Object)val).name).Append("|");
				}
				continue;
			}
			if (stringBuilder.Length < 300)
			{
				stringBuilder.Append(((Object)val).name).Append("|");
			}
			Renderer[] componentsInChildren = val.GetComponentsInChildren<Renderer>(true);
			foreach (Renderer val2 in componentsInChildren)
			{
				val2.enabled = false;
			}
			Collider[] componentsInChildren2 = val.GetComponentsInChildren<Collider>(true);
			foreach (Collider val3 in componentsInChildren2)
			{
				val3.enabled = false;
			}
			num++;
		}
		ModEntry.Log("ClearBaseScene: cleared " + num + " | CLEARED: " + stringBuilder);
		ModEntry.Log("ClearBaseScene: PRESERVED: " + stringBuilder2);
	}

	private static bool ShouldPreserveRoot(string name, string[] keywords)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		foreach (string value in keywords)
		{
			if (name.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private static void AddFaceSlab(GameObject parent, Vector3 worldCenter, Vector3 worldSize)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("_col");
		val.transform.parent = parent.transform;
		val.transform.position = worldCenter;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		BoxCollider val2 = val.AddComponent<BoxCollider>();
		val2.center = Vector3.zero;
		val2.size = worldSize;
	}

	private static bool ShouldSkip(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return true;
		}
		string[] sKIP_EXACT = SKIP_EXACT;
		foreach (string text in sKIP_EXACT)
		{
			if (path == text)
			{
				return true;
			}
		}
		sKIP_EXACT = SKIP_CONTAINS;
		foreach (string text in sKIP_EXACT)
		{
			if (path.Contains(text))
			{
				return true;
			}
		}
		return false;
	}

	private static PrimitiveType MeshToPrimitive(string mesh)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(mesh))
		{
			return (PrimitiveType)3;
		}
		string text = mesh.ToLower();
		if (text.Contains("sphere"))
		{
			return (PrimitiveType)0;
		}
		if (text.Contains("capsule"))
		{
			return (PrimitiveType)1;
		}
		if (text.Contains("cylinder"))
		{
			return (PrimitiveType)2;
		}
		return (PrimitiveType)3;
	}

	private static void ApplyBoxUVs(MeshFilter mf, float sx, float sy, float sz)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		Mesh mesh = mf.mesh;
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Vector2[] array = (Vector2[])(object)new Vector2[vertices.Length];
		float num = sx * 0.5f;
		float num2 = sz * 0.5f;
		float num3 = sx * 0.5f;
		float num4 = sy * 0.5f;
		float num5 = sz * 0.5f;
		float num6 = sy * 0.5f;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 val = normals[i];
			Vector3 val2 = vertices[i];
			if (Mathf.Abs(val.y) >= 0.5f)
			{
				ref Vector2 reference = ref array[i];
				reference = new Vector2((val2.x + 0.5f) * num, (val2.z + 0.5f) * num2);
			}
			else if (Mathf.Abs(val.z) >= 0.5f)
			{
				ref Vector2 reference2 = ref array[i];
				reference2 = new Vector2((val2.x + 0.5f) * num3, (val2.y + 0.5f) * num4);
			}
			else
			{
				ref Vector2 reference3 = ref array[i];
				reference3 = new Vector2((val2.z + 0.5f) * num5, (val2.y + 0.5f) * num6);
			}
		}
		mesh.uv = array;
	}
}
