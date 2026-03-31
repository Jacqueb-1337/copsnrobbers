using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace CNRMods;

public class ContentManager : MonoBehaviour
{
	private const string ContentUrl = "https://play.jacqueb.me/economy/content.php";

	public const string MapCacheDir = "/storage/emulated/0/CNRMods/content_cache/maps/";

	private const string TexCacheDir = "/storage/emulated/0/CNRMods/content_cache/textures/";

	private const string ThumbCacheDir = "/storage/emulated/0/CNRMods/content_cache/thumbs/";

	private const string DataCacheDir = "/storage/emulated/0/CNRMods/content_cache/data/";

	private const string ManifestCache = "/storage/emulated/0/CNRMods/content_cache/manifest.json";

	private const string VersionPref = "CNRMod_ContentVersion";

	public static OfficialMapEntry[] OfficialMaps = new OfficialMapEntry[0];

	public static OfficialTextureEntry[] OfficialTextures = new OfficialTextureEntry[0];

	public static OfficialDataEntry[] OfficialData = new OfficialDataEntry[0];

	public static bool Ready = false;

	private static Dictionary<string, Texture2D> _texCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	private static Dictionary<string, Texture2D> _thumbCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	public static Texture2D GetMapThumbnail(string id)
	{
		Texture2D value;
		return _thumbCache.TryGetValue(id, out value) ? value : null;
	}

	private void Start()
	{
		EnsureDirs();
		LoadCachedManifest();
		((MonoBehaviour)this).StartCoroutine(FetchAndSync());
	}

	private void OnLevelWasLoaded(int level)
	{
		if (_texCache.Count > 0)
		{
			((MonoBehaviour)this).StartCoroutine(ApplyTextureSwaps());
		}
	}

	private static void EnsureDirs()
	{
		try
		{
			string[] array = new string[5] { "/storage/emulated/0/CNRMods/content_cache/", "/storage/emulated/0/CNRMods/content_cache/maps/", "/storage/emulated/0/CNRMods/content_cache/textures/", "/storage/emulated/0/CNRMods/content_cache/thumbs/", "/storage/emulated/0/CNRMods/content_cache/data/" };
			string[] array2 = array;
			foreach (string path in array2)
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("ContentManager EnsureDirs: " + ex.Message);
		}
	}

	private static void LoadCachedManifest()
	{
		if (!File.Exists("/storage/emulated/0/CNRMods/content_cache/manifest.json"))
		{
			Ready = true;
			return;
		}
		try
		{
			ParseManifest(File.ReadAllText("/storage/emulated/0/CNRMods/content_cache/manifest.json"));
			OfficialTextureEntry[] officialTextures = OfficialTextures;
			foreach (OfficialTextureEntry officialTextureEntry in officialTextures)
			{
				string path = "/storage/emulated/0/CNRMods/content_cache/textures/" + officialTextureEntry.Id + ".png";
				if (File.Exists(path))
				{
					LoadTexFile(officialTextureEntry.Id, path);
				}
			}
			OfficialMapEntry[] officialMaps = OfficialMaps;
			foreach (OfficialMapEntry officialMapEntry in officialMaps)
			{
				string[] array = new string[2] { "jpg", "png" };
				foreach (string text in array)
				{
					string path = "/storage/emulated/0/CNRMods/content_cache/thumbs/" + officialMapEntry.Id + "." + text;
					if (File.Exists(path))
					{
						LoadThumbFile(officialMapEntry.Id, path);
						break;
					}
				}
			}
			if (!VerifyAndClean())
			{
				ModEntry.Log("ContentManager: hash mismatch(es) found — clearing version to force re-download");
				PlayerPrefs.SetString("CNRMod_ContentVersion", "");
				PlayerPrefs.Save();
			}
			ModEntry.Log("ContentManager: cache loaded — maps=" + OfficialMaps.Length + " tex=" + OfficialTextures.Length + " data=" + OfficialData.Length);
		}
		catch (Exception ex)
		{
			ModEntry.Log("ContentManager: LoadCachedManifest error: " + ex.Message);
		}
		Ready = true;
	}

	private static string ComputeMD5(string path)
	{
		try
		{
			using MD5 mD = MD5.Create();
			using FileStream inputStream = File.OpenRead(path);
			byte[] array = mD.ComputeHash(inputStream);
			StringBuilder stringBuilder = new StringBuilder(32);
			byte[] array2 = array;
			foreach (byte b in array2)
			{
				stringBuilder.Append(b.ToString("x2"));
			}
			return stringBuilder.ToString();
		}
		catch
		{
			return null;
		}
	}

	private static bool VerifyAndClean()
	{
		bool result = true;
		OfficialMapEntry[] officialMaps = OfficialMaps;
		foreach (OfficialMapEntry officialMapEntry in officialMaps)
		{
			if (!string.IsNullOrEmpty(officialMapEntry.Hash))
			{
				string path = "/storage/emulated/0/CNRMods/content_cache/maps/" + officialMapEntry.Id + ".json";
				if (File.Exists(path))
				{
					string text = ComputeMD5(path);
					if (text != officialMapEntry.Hash.ToLower())
					{
						ModEntry.Log("ContentManager: map hash mismatch [" + officialMapEntry.Id + "] server=" + officialMapEntry.Hash + " local=" + text + " — deleting");
						try
						{
							File.Delete(path);
						}
						catch
						{
						}
						result = false;
					}
				}
			}
			if (string.IsNullOrEmpty(officialMapEntry.ThumbnailHash))
			{
				continue;
			}
			string[] array = new string[4] { "jpg", "png", "gif", "webp" };
			foreach (string text2 in array)
			{
				string path2 = "/storage/emulated/0/CNRMods/content_cache/thumbs/" + officialMapEntry.Id + "." + text2;
				if (!File.Exists(path2))
				{
					continue;
				}
				string text = ComputeMD5(path2);
				if (text != officialMapEntry.ThumbnailHash.ToLower())
				{
					ModEntry.Log("ContentManager: thumb hash mismatch [" + officialMapEntry.Id + "] — deleting");
					try
					{
						File.Delete(path2);
					}
					catch
					{
					}
					result = false;
				}
				break;
			}
		}
		OfficialTextureEntry[] officialTextures = OfficialTextures;
		foreach (OfficialTextureEntry officialTextureEntry in officialTextures)
		{
			if (string.IsNullOrEmpty(officialTextureEntry.Hash))
			{
				continue;
			}
			string path = "/storage/emulated/0/CNRMods/content_cache/textures/" + officialTextureEntry.Id + ".png";
			if (!File.Exists(path))
			{
				continue;
			}
			string text = ComputeMD5(path);
			if (text != officialTextureEntry.Hash.ToLower())
			{
				ModEntry.Log("ContentManager: tex hash mismatch [" + officialTextureEntry.Id + "] — deleting");
				try
				{
					File.Delete(path);
				}
				catch
				{
				}
				result = false;
			}
		}
		OfficialDataEntry[] officialData = OfficialData;
		foreach (OfficialDataEntry officialDataEntry in officialData)
		{
			if (string.IsNullOrEmpty(officialDataEntry.Hash))
			{
				continue;
			}
			string path = "/storage/emulated/0/CNRMods/content_cache/data/" + officialDataEntry.Id + ".json";
			if (!File.Exists(path))
			{
				continue;
			}
			string text = ComputeMD5(path);
			if (text != officialDataEntry.Hash.ToLower())
			{
				ModEntry.Log("ContentManager: data hash mismatch [" + officialDataEntry.Id + "] — deleting");
				try
				{
					File.Delete(path);
				}
				catch
				{
				}
				result = false;
			}
		}
		return result;
	}

	private IEnumerator FetchAndSync()
	{
		WWW www = new WWW("https://play.jacqueb.me/economy/content.php");
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			ModEntry.Log("ContentManager: fetch error: " + www.error);
			Ready = true;
			yield break;
		}
		string json = www.text;
		string newVer = ModEntry.ParseJsonValue(json, "manifest_version") ?? "";
		string oldVer = PlayerPrefs.GetString("CNRMod_ContentVersion", "");
		ParseManifest(json);
		try
		{
			File.WriteAllText("/storage/emulated/0/CNRMods/content_cache/manifest.json", json);
		}
		catch
		{
		}
		if (newVer != oldVer || newVer == "")
		{
			ModEntry.Log("ContentManager: new version (" + oldVer + " -> " + newVer + "), downloading items");
			yield return ((MonoBehaviour)this).StartCoroutine(DownloadItems());
			PlayerPrefs.SetString("CNRMod_ContentVersion", newVer);
			PlayerPrefs.Save();
		}
		else
		{
			ModEntry.Log("ContentManager: up to date (" + newVer + ")");
		}
		Ready = true;
		ModEntry.Log("ContentManager ready — maps=" + OfficialMaps.Length + " tex=" + OfficialTextures.Length + " data=" + OfficialData.Length);
	}

	private static void ParseManifest(string json)
	{
		try
		{
			string text = ExtractArray(json, "maps");
			string text2 = ExtractArray(json, "textures");
			string text3 = ExtractArray(json, "data");
			CManifestMap[] array = (string.IsNullOrEmpty(text) ? new CManifestMap[0] : (JsonReader.Deserialize<CManifestMap[]>(text) ?? new CManifestMap[0]));
			CManifestTexture[] array2 = (string.IsNullOrEmpty(text2) ? new CManifestTexture[0] : (JsonReader.Deserialize<CManifestTexture[]>(text2) ?? new CManifestTexture[0]));
			CManifestData[] array3 = (string.IsNullOrEmpty(text3) ? new CManifestData[0] : (JsonReader.Deserialize<CManifestData[]>(text3) ?? new CManifestData[0]));
			List<OfficialMapEntry> list = new List<OfficialMapEntry>();
			CManifestMap[] array4 = array;
			foreach (CManifestMap cManifestMap in array4)
			{
				if (!string.IsNullOrEmpty(cManifestMap.id) && !string.IsNullOrEmpty(cManifestMap.url))
				{
					list.Add(new OfficialMapEntry
					{
						Id = cManifestMap.id,
						Name = cManifestMap.name,
						Url = cManifestMap.url,
						ThumbnailUrl = (cManifestMap.thumbnail_url ?? ""),
						Hash = (cManifestMap.hash ?? ""),
						ThumbnailHash = (cManifestMap.thumbnail_hash ?? "")
					});
				}
			}
			OfficialMaps = list.ToArray();
			List<OfficialTextureEntry> list2 = new List<OfficialTextureEntry>();
			CManifestTexture[] array5 = array2;
			foreach (CManifestTexture cManifestTexture in array5)
			{
				if (!string.IsNullOrEmpty(cManifestTexture.id) && !string.IsNullOrEmpty(cManifestTexture.url))
				{
					list2.Add(new OfficialTextureEntry
					{
						Id = cManifestTexture.id,
						MaterialName = cManifestTexture.material_name,
						Url = cManifestTexture.url,
						Hash = (cManifestTexture.hash ?? "")
					});
				}
			}
			OfficialTextures = list2.ToArray();
			List<OfficialDataEntry> list3 = new List<OfficialDataEntry>();
			CManifestData[] array6 = array3;
			foreach (CManifestData cManifestData in array6)
			{
				if (!string.IsNullOrEmpty(cManifestData.id) && !string.IsNullOrEmpty(cManifestData.url))
				{
					list3.Add(new OfficialDataEntry
					{
						Id = cManifestData.id,
						Key = cManifestData.key,
						Url = cManifestData.url,
						Hash = (cManifestData.hash ?? "")
					});
				}
			}
			OfficialData = list3.ToArray();
		}
		catch (Exception ex)
		{
			ModEntry.Log("ContentManager: ParseManifest error: " + ex.Message);
		}
	}

	private static string ExtractArray(string json, string key)
	{
		try
		{
			string text = "\"" + key + "\":";
			int num = json.IndexOf(text);
			if (num < 0)
			{
				return null;
			}
			int num2 = json.IndexOf('[', num + text.Length);
			if (num2 < 0)
			{
				return null;
			}
			int num3 = 0;
			int num4 = num2;
			for (int i = num2; i < json.Length; i++)
			{
				if (json[i] == '[')
				{
					num3++;
				}
				else if (json[i] == ']')
				{
					num3--;
					if (num3 == 0)
					{
						num4 = i;
						break;
					}
				}
			}
			return json.Substring(num2, num4 - num2 + 1);
		}
		catch
		{
			return null;
		}
	}

	private IEnumerator DownloadItems()
	{
		try
		{
			OfficialMapEntry[] officialMaps = OfficialMaps;
			foreach (OfficialMapEntry m in officialMaps)
			{
				string path = "/storage/emulated/0/CNRMods/content_cache/maps/" + m.Id + ".json";
				yield return ((MonoBehaviour)this).StartCoroutine(DownloadFile(m.Url, path, "map:" + m.Id));
				if (File.Exists(path) && !string.IsNullOrEmpty(m.Hash))
				{
					string text = ComputeMD5(path);
					if (text != m.Hash.ToLower())
					{
						ModEntry.Log("ContentManager: map download hash mismatch [" + m.Id + "] expected=" + m.Hash + " got=" + text + " — deleting");
						try
						{
							File.Delete(path);
						}
						catch
						{
						}
					}
				}
				if (string.IsNullOrEmpty(m.ThumbnailUrl))
				{
					continue;
				}
				string thumbPath = string.Concat(str3: m.ThumbnailUrl.EndsWith(".png") ? "png" : "jpg", str0: "/storage/emulated/0/CNRMods/content_cache/thumbs/", str1: m.Id, str2: ".");
				yield return ((MonoBehaviour)this).StartCoroutine(DownloadFile(m.ThumbnailUrl, thumbPath, "thumb:" + m.Id));
				if (!File.Exists(thumbPath))
				{
					continue;
				}
				if (!string.IsNullOrEmpty(m.ThumbnailHash) && ComputeMD5(thumbPath) != m.ThumbnailHash.ToLower())
				{
					ModEntry.Log("ContentManager: thumb download hash mismatch [" + m.Id + "] — deleting");
					try
					{
						File.Delete(thumbPath);
					}
					catch
					{
					}
				}
				else
				{
					LoadThumbFile(m.Id, thumbPath);
				}
			}
		}
		finally
		{
		}
		try
		{
			OfficialTextureEntry[] officialTextures = OfficialTextures;
			foreach (OfficialTextureEntry te in officialTextures)
			{
				string path2 = "/storage/emulated/0/CNRMods/content_cache/textures/" + te.Id + ".png";
				yield return ((MonoBehaviour)this).StartCoroutine(DownloadFile(te.Url, path2, "tex:" + te.Id));
				if (!File.Exists(path2))
				{
					continue;
				}
				if (!string.IsNullOrEmpty(te.Hash) && ComputeMD5(path2) != te.Hash.ToLower())
				{
					ModEntry.Log("ContentManager: tex download hash mismatch [" + te.Id + "] — deleting");
					try
					{
						File.Delete(path2);
					}
					catch
					{
					}
				}
				else
				{
					LoadTexFile(te.Id, path2);
				}
			}
		}
		finally
		{
		}
		try
		{
			OfficialDataEntry[] officialData = OfficialData;
			foreach (OfficialDataEntry d in officialData)
			{
				string path3 = "/storage/emulated/0/CNRMods/content_cache/data/" + d.Id + ".json";
				yield return ((MonoBehaviour)this).StartCoroutine(DownloadFile(d.Url, path3, "data:" + d.Id));
				if (!File.Exists(path3) || string.IsNullOrEmpty(d.Hash))
				{
					continue;
				}
				string text = ComputeMD5(path3);
				if (text != d.Hash.ToLower())
				{
					ModEntry.Log("ContentManager: data download hash mismatch [" + d.Id + "] — deleting");
					try
					{
						File.Delete(path3);
					}
					catch
					{
					}
				}
			}
		}
		finally
		{
		}
	}

	private IEnumerator DownloadFile(string url, string dest, string label)
	{
		WWW www = new WWW(url);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			ModEntry.Log("ContentManager: download error [" + label + "]: " + www.error);
			yield break;
		}
		try
		{
			File.WriteAllBytes(dest, www.bytes);
			ModEntry.Log("ContentManager: saved " + label);
		}
		catch (Exception ex)
		{
			ModEntry.Log("ContentManager: write error [" + label + "]: " + ex.Message);
		}
	}

	private static void LoadTexFile(string id, string path)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		try
		{
			byte[] array = File.ReadAllBytes(path);
			Texture2D val = new Texture2D(2, 2, (TextureFormat)5, false);
			if (val.LoadImage(array))
			{
				((Object)val).name = id;
				_texCache[id] = val;
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("ContentManager: LoadTexFile error [" + id + "]: " + ex.Message);
		}
	}

	private static void LoadThumbFile(string id, string path)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		try
		{
			byte[] array = File.ReadAllBytes(path);
			Texture2D val = new Texture2D(2, 2, (TextureFormat)5, false);
			if (val.LoadImage(array))
			{
				((Object)val).name = "thumb_" + id;
				_thumbCache[id] = val;
				ModEntry.Log("ContentManager: loaded thumbnail for " + id);
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log("ContentManager: LoadThumbFile error [" + id + "]: " + ex.Message);
		}
	}

	private IEnumerator ApplyTextureSwaps()
	{
		yield return (object)new WaitForSeconds(0.3f);
		Dictionary<string, Texture2D> matToTex = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
		OfficialTextureEntry[] officialTextures = OfficialTextures;
		foreach (OfficialTextureEntry officialTextureEntry in officialTextures)
		{
			if (_texCache.ContainsKey(officialTextureEntry.Id))
			{
				matToTex[officialTextureEntry.MaterialName] = _texCache[officialTextureEntry.Id];
			}
		}
		if (matToTex.Count == 0)
		{
			yield break;
		}
		int swapped = 0;
		Renderer[] renderers = (Renderer[])(object)Object.FindObjectsOfType(typeof(Renderer));
		HashSet<int> seen = new HashSet<int>();
		Renderer[] array = renderers;
		foreach (Renderer val in array)
		{
			if ((Object)(object)val == (Object)null || (Object)(object)val.sharedMaterial == (Object)null)
			{
				continue;
			}
			int instanceID = ((Object)val.sharedMaterial).GetInstanceID();
			if (!seen.Contains(instanceID))
			{
				seen.Add(instanceID);
				string text = ((Object)val.sharedMaterial).name;
				int num = text.IndexOf(" (Instance)", StringComparison.OrdinalIgnoreCase);
				if (num >= 0)
				{
					text = text.Substring(0, num).Trim();
				}
				if (matToTex.ContainsKey(text))
				{
					val.material.mainTexture = (Texture)(object)matToTex[text];
					swapped++;
				}
			}
		}
		if (swapped > 0)
		{
			ModEntry.Log("ContentManager: swapped " + swapped + " material texture(s)");
		}
	}

	public static string GetData(string key)
	{
		OfficialDataEntry[] officialData = OfficialData;
		foreach (OfficialDataEntry officialDataEntry in officialData)
		{
			if (!string.Equals(officialDataEntry.Key, key, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			string path = "/storage/emulated/0/CNRMods/content_cache/data/" + officialDataEntry.Id + ".json";
			if (File.Exists(path))
			{
				try
				{
					return File.ReadAllText(path);
				}
				catch
				{
					return null;
				}
			}
		}
		return null;
	}
}
