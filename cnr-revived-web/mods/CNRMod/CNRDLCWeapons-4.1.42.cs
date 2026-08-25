using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace CNRMods
{
    // DLC weapons intentionally use a vanilla weapon as their gameplay donor.
    // This keeps old WeaponType/network code untouched while CNR supplies the
    // ownership, UI identity, model and custom behavior.
    public static class CNRDLCWeaponSystem
    {
        public const string BussinId = "bussin";
        public const string BussinName = "Bussin'";
        public const string BussinDonor = "M87T";
        public const string BussinLevelKey = "Bussin";
        public const string EquippedKey = "CNR_EquippedDLCWeapon";

        private const string RifleBackupKey = "CNR_BussinRifleBackup";
        private const string RifleOverrideKey = "CNR_BussinRifleOverride";

        public static int GetBussinLevel()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(BussinLevelKey, 0), 0, 3);
        }

        public static bool IsBussinOwned()
        {
            return GetBussinLevel() > 0;
        }

        public static bool IsBussinEquipped()
        {
            return IsBussinOwned() && PlayerPrefs.GetString(EquippedKey, "") == BussinId && IsDonorEquipped();
        }

        public static bool IsDonorEquipped()
        {
            try
            {
                GItemInfo[] equipped = GrowthManagerKit.GetCurEquippedWeaponItemInfoList();
                if (equipped == null) return false;
                for (int i = 0; i < equipped.Length; i++)
                    if (equipped[i] != null && equipped[i].mName == BussinDonor)
                        return true;
            }
            catch { }
            return false;
        }

        public static int GetActionPrice()
        {
            int level = GetBussinLevel();
            if (level <= 1) return 1000;
            if (level == 2) return 5000;
            return 0;
        }

        public static bool PurchaseOrUpgrade()
        {
            int level = GetBussinLevel();
            if (level >= 3) return false;

            int price = GetActionPrice();
            int coins = GrowthManagerKit.GetCoins();
            if (coins < price)
            {
                ModEntry.Log("CNR DLC weapons: not enough coins for Bussin' (need " + price + ", have " + coins + ")");
                return false;
            }

            UserDataController.AddCoins(-price);
            PlayerPrefs.SetInt(BussinLevelKey, level + 1);
            PlayerPrefs.Save();
            ModEntry.Log("CNR DLC weapons: Bussin' level is now " + (level + 1));
            return true;
        }

        public static void ToggleBussinEquip()
        {
            if (!IsBussinOwned()) return;

            bool donorEquipped = IsDonorEquipped();
            bool markedBussin = PlayerPrefs.GetString(EquippedKey, "") == BussinId;

            if (markedBussin && donorEquipped)
            {
                GrowthManagerKit.ProcessOneWeaponEquipTap(BussinDonor);
                PlayerPrefs.DeleteKey(EquippedKey);
            }
            else if (donorEquipped)
            {
                // M87T is already occupying a loadout slot. Turn that slot into
                // the DLC variant without needlessly toggling it out and back in.
                PlayerPrefs.SetString(EquippedKey, BussinId);
            }
            else
            {
                GrowthManagerKit.ProcessOneWeaponEquipTap(BussinDonor);
                if (IsDonorEquipped()) PlayerPrefs.SetString(EquippedKey, BussinId);
            }
            PlayerPrefs.Save();
        }

        public static void ClearMarkerIfDonorMissing()
        {
            if (PlayerPrefs.GetString(EquippedKey, "") == BussinId && !IsDonorEquipped())
            {
                PlayerPrefs.DeleteKey(EquippedKey);
                PlayerPrefs.Save();
            }
        }

        public static GItemInfo BuildBussinInfo(bool upgradePreview)
        {
            int ownedLevel = GetBussinLevel();
            int displayLevel = ownedLevel <= 0 ? 1 : ownedLevel;
            if (upgradePreview && ownedLevel > 0 && ownedLevel < 3) displayLevel++;
            displayLevel = Mathf.Clamp(displayLevel, 1, 3);

            GItemInfo donor = null;
            try { donor = GrowthManagerKit.GetItemInfoByName(BussinDonor); } catch { }

            GItemInfo info = new GItemInfo();
            info.mName = "CNR_Bussin";
            info.mPurchasedType = GItemPurchaseType.CoinsPurchase;
            info.mUnlockCLevel = 1;
            info.mCurWeaponLevel = displayLevel;
            info.mMaxWeaponLevel = 3;
            info.mPrice = upgradePreview ? PriceForLevel(displayLevel) : GetActionPrice();
            info.mNameDisplay = BussinName;
            info.mLogoSpriteName = donor != null ? donor.mLogoSpriteName : "";
            info.mPowerSpriteName = "Rating_" + (displayLevel == 1 ? 5 : displayLevel == 2 ? 6 : 7);
            info.mClipSpriteName = "Rating_" + (displayLevel == 3 ? 2 : 1);
            info.mFireRateSpriteName = "Rating_3";
            info.mWeaponLevelSpriteName = "WeaponLevel_" + displayLevel;
            info.mCanUpgrade = ownedLevel < 3;
            info.mIsEnabled = ownedLevel > 0;
            info.mIsEquipped = IsBussinEquipped();
            info.mCanBeAutoUnlocked = false;
            info.mAutoUnlockLevel = 1;
            info.offRate = 100;
            info.offRateDescription = string.Empty;
            return info;
        }

        private static int PriceForLevel(int level)
        {
            if (level <= 1) return 1000;
            if (level == 2) return 5000;
            return 0;
        }

        public static OfficialGunEntry FindBussinManifestEntry()
        {
            for (int i = 0; i < ContentManager.OfficialGuns.Length; i++)
            {
                OfficialGunEntry g = ContentManager.OfficialGuns[i];
                if (g != null && (string.Equals(g.Id, BussinId, StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(g.GunName, BussinName, StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(g.GunName, "Bussin", StringComparison.OrdinalIgnoreCase)))
                    return g;
            }
            return null;
        }

        public static bool HasDownloadedModel()
        {
            return File.Exists(ContentManager.GunCacheDir + BussinId + ".json");
        }

        // A temporary Rifle PlayerPrefs override is required because vanilla M87T
        // reads that key at pellet-damage time. A persistent backup makes this safe
        // across an abnormal exit; normal scene teardown restores immediately.
        public static void RecoverStaleRifleOverride()
        {
            if (PlayerPrefs.GetInt(RifleOverrideKey, 0) == 0) return;
            int backup = PlayerPrefs.GetInt(RifleBackupKey, 1);
            PlayerPrefs.SetInt("Rifle", Mathf.Clamp(backup, 1, 3));
            PlayerPrefs.DeleteKey(RifleBackupKey);
            PlayerPrefs.DeleteKey(RifleOverrideKey);
            PlayerPrefs.Save();
            ModEntry.Log("CNR DLC weapons: recovered stale M87T level override");
        }

        public static void BeginRifleOverride()
        {
            int level = GetBussinLevel();
            if (level <= 0) return;
            if (PlayerPrefs.GetInt(RifleOverrideKey, 0) == 0)
            {
                PlayerPrefs.SetInt(RifleBackupKey, PlayerPrefs.GetInt("Rifle", 1));
                PlayerPrefs.SetInt(RifleOverrideKey, 1);
                PlayerPrefs.Save();
            }
            PlayerPrefs.SetInt("Rifle", level);
        }

        public static void EndRifleOverride()
        {
            if (PlayerPrefs.GetInt(RifleOverrideKey, 0) == 0) return;
            int backup = PlayerPrefs.GetInt(RifleBackupKey, 1);
            PlayerPrefs.SetInt("Rifle", Mathf.Clamp(backup, 1, 3));
            PlayerPrefs.DeleteKey(RifleBackupKey);
            PlayerPrefs.DeleteKey(RifleOverrideKey);
            PlayerPrefs.Save();
        }

        public static int GetProgressionWeaponLevel(string key, int fallback)
        {
            if (key == "Rifle" && PlayerPrefs.GetInt(RifleOverrideKey, 0) != 0)
                return PlayerPrefs.GetInt(RifleBackupKey, fallback);
            return PlayerPrefs.GetInt(key, fallback);
        }

        public static void ApplyProgressionWeaponLevel(string key, int value)
        {
            if (key == "Rifle" && PlayerPrefs.GetInt(RifleOverrideKey, 0) != 0)
            {
                int cur = PlayerPrefs.GetInt(RifleBackupKey, 1);
                if (value > cur) PlayerPrefs.SetInt(RifleBackupKey, value);
                return;
            }
            int local = PlayerPrefs.GetInt(key, 1);
            if (value > local) PlayerPrefs.SetInt(key, value);
        }
    }

    public class CNRDLCWeaponStoreHook : MonoBehaviour
    {
        private UIStoreDirector _store;
        private UIStoreWeaponPrefab _card;

        private void Start()
        {
            StartCoroutine(Setup());
        }

        private IEnumerator Setup()
        {
            yield return new WaitForSeconds(0.7f);
            _store = UIStoreDirector.mInstance;
            if (_store == null || _store.weaponPrefab == null || _store.weaponObjParent == null) yield break;
            for (int i = 0; i < 20 && (CNRDLCWeaponSystem.FindBussinManifestEntry() == null || !CNRDLCWeaponSystem.HasDownloadedModel()); i++)
                yield return new WaitForSeconds(0.25f);
            if (CNRDLCWeaponSystem.FindBussinManifestEntry() == null || !CNRDLCWeaponSystem.HasDownloadedModel())
            {
                ModEntry.Log("CNR DLC weapons: Bussin' not injected into Store (manifest/model missing)");
                yield break;
            }

            GameObject go = Instantiate(_store.weaponPrefab, _store.weaponPrefab.transform.position,
                _store.weaponPrefab.transform.rotation) as GameObject;
            if (go == null) yield break;
            go.name = "CNR DLC Weapon - Bussin";
            go.transform.parent = _store.weaponObjParent;
            go.transform.localScale = _store.weaponPrefab.transform.localScale;
            go.transform.position = _store.weaponPrefab.transform.position;
            go.transform.rotation = _store.weaponPrefab.transform.rotation;

            _card = go.GetComponent<UIStoreWeaponPrefab>();
            if (_card == null) { Destroy(go); yield break; }
            _card.weaponName = "CNR_Bussin";
            RefreshCard();

            UIStoreBtnEvent[] events = go.GetComponentsInChildren<UIStoreBtnEvent>(true);
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].buttonName != UIStoreBtnEvent.ButtonName.Upgrade) continue;
                events[i].buttonName = UIStoreBtnEvent.ButtonName.Nil;
                CNRDLCWeaponStoreButton btn = events[i].gameObject.GetComponent<CNRDLCWeaponStoreButton>();
                if (btn == null) btn = events[i].gameObject.AddComponent<CNRDLCWeaponStoreButton>();
                btn.Hook = this;
            }

            int count = GrowthManagerKit.GetAllWeaponNameList().Length + 1;
            UITable table = _store.weaponObjParent.GetComponent<UITable>();
            if (table != null)
            {
                table.columns = (count + 1) / 2;
                table.Reposition();
            }
            ModEntry.Log("CNR DLC weapons: injected Bussin' into vanilla Store weapon table");
        }

        public void PurchaseOrUpgrade()
        {
            if (!CNRDLCWeaponSystem.PurchaseOrUpgrade()) return;
            RefreshCard();
            StartCoroutine(RefreshCardNextFrame());
        }

        private IEnumerator RefreshCardNextFrame()
        {
            yield return null;
            RefreshCard();
        }

        public void RefreshCard()
        {
            if (_card == null) return;
            GItemInfo info = CNRDLCWeaponSystem.BuildBussinInfo(false);
            _card.gItemInfo = info;
            _card.upgradeGItemInfo = CNRDLCWeaponSystem.BuildBussinInfo(true);

            // UIStoreWeaponPrefab.readData() is mostly one-way: it hides controls
            // when a weapon maxes out, but it does not fully restore/change them
            // after an unlock/upgrade. Reset the mutable state first so the card
            // reflects the purchase immediately without leaving the Store scene.
            if (_card.upgradeBtn != null)
            {
                _card.upgradeBtn.SetActive(info.mCanUpgrade);
                UILabel label = _card.upgradeBtn.GetComponentInChildren<UILabel>();
                if (label != null) label.text = info.mIsEnabled ? "Upgrade" : "Unlock";
            }
            if (_card.priceLabel != null) _card.priceLabel.SetActive(info.mCanUpgrade);
            if (_card.coinSprite != null) _card.coinSprite.SetActive(info.mCanUpgrade);
            if (_card.lockSprite != null) _card.lockSprite.SetActive(!info.mIsEnabled);

            _card.readData(info);
            CNRDLCWeaponThumbnail.Apply(_card.logoSprite);
        }
    }

    public class CNRDLCWeaponStoreButton : MonoBehaviour
    {
        public CNRDLCWeaponStoreHook Hook;
        private void OnClick()
        {
            if (Hook != null) Hook.PurchaseOrUpgrade();
        }
    }

    public class CNRDLCWeaponProfileHook : MonoBehaviour
    {
        private UIProfileDirector _profile;
        private CNRDLCWeaponProfileCard _card;
        private float _nextEquipRefresh;
        private string _lastEquipSignature = "";

        private void Start()
        {
            StartCoroutine(Setup());
        }

        private void Update()
        {
            if (_profile == null || Time.time < _nextEquipRefresh) return;
            _nextEquipRefresh = Time.time + 0.15f;
            string signature = BuildEquipSignature();
            if (signature == _lastEquipSignature) return;
            _lastEquipSignature = signature;
            CNRDLCWeaponSystem.ClearMarkerIfDonorMissing();
            RefreshEquippedWindows();
            RefreshCard();
        }

        private IEnumerator Setup()
        {
            yield return new WaitForSeconds(0.7f);
            _profile = UIProfileDirector.mInstance;
            if (_profile == null || _profile.weaponPrefab == null || _profile.weaponObjParent == null) yield break;
            if (!CNRDLCWeaponSystem.IsBussinOwned()) yield break;
            for (int i = 0; i < 20 && (CNRDLCWeaponSystem.FindBussinManifestEntry() == null || !CNRDLCWeaponSystem.HasDownloadedModel()); i++)
                yield return new WaitForSeconds(0.25f);
            if (CNRDLCWeaponSystem.FindBussinManifestEntry() == null || !CNRDLCWeaponSystem.HasDownloadedModel())
                yield break;

            GameObject go = Instantiate(_profile.weaponPrefab, _profile.weaponPrefab.transform.position,
                _profile.weaponPrefab.transform.rotation) as GameObject;
            if (go == null) yield break;
            go.name = "CNR DLC Weapon - Bussin";
            go.transform.parent = _profile.weaponObjParent;
            go.transform.localScale = _profile.weaponPrefab.transform.localScale;
            go.transform.position = _profile.weaponPrefab.transform.position;
            go.transform.rotation = _profile.weaponPrefab.transform.rotation;

            UIProfileWeaponPrefab vanilla = go.GetComponent<UIProfileWeaponPrefab>();
            if (vanilla == null) { Destroy(go); yield break; }
            GItemInfo info = CNRDLCWeaponSystem.BuildBussinInfo(false);
            vanilla.gItemInfo = info;
            vanilla.ReadData(info);

            GameObject logoObject = vanilla.logoSprite;
            DestroyImmediate(vanilla);

            _card = go.AddComponent<CNRDLCWeaponProfileCard>();
            _card.Hook = this;
            _card.LogoObject = logoObject;
            _card.RefreshVisual();

            int count = GrowthManagerKit.GetUserAllEnabledWeaponItemInfo().Length + 1;
            UITable table = _profile.weaponObjParent.GetComponent<UITable>();
            if (table != null)
            {
                table.columns = count;
                table.Reposition();
            }
            RefreshEquippedWindows();
            RefreshCard();
            _lastEquipSignature = BuildEquipSignature();
            ModEntry.Log("CNR DLC weapons: injected owned Bussin' into vanilla Profile weapon table");
        }

        private string BuildEquipSignature()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            GItemInfo[] equipped = GrowthManagerKit.GetCurEquippedWeaponItemInfoList();
            if (equipped != null)
            {
                for (int i = 0; i < equipped.Length; i++)
                {
                    if (i > 0) sb.Append('|');
                    sb.Append(equipped[i] != null ? equipped[i].mName : "");
                }
            }
            sb.Append("#").Append(PlayerPrefs.GetString(CNRDLCWeaponSystem.EquippedKey, ""));
            return sb.ToString();
        }

        public void ToggleEquip()
        {
            CNRDLCWeaponSystem.ToggleBussinEquip();
            if (_profile != null) _profile.UpdateWeaponData();
            RefreshEquippedWindows();
            RefreshCard();
        }

        public void RefreshCard()
        {
            if (_card == null) return;
            _card.RefreshVisual();
        }

        private void RefreshEquippedWindows()
        {
            if (_profile == null || _profile.equipedWindow == null) return;
            GItemInfo[] equipped = GrowthManagerKit.GetCurEquippedWeaponItemInfoList();
            if (equipped == null) equipped = new GItemInfo[0];
            bool bussinEquipped = CNRDLCWeaponSystem.IsBussinEquipped();

            for (int i = 0; i < _profile.equipedWindow.Length; i++)
            {
                GameObject window = _profile.equipedWindow[i];
                if (window == null) continue;
                UIProfileEquipedDisplay display = window.GetComponent<UIProfileEquipedDisplay>();
                if (display == null || display.LogoSprite == null) continue;

                if (i < equipped.Length && bussinEquipped && equipped[i] != null && equipped[i].mName == CNRDLCWeaponSystem.BussinDonor)
                {
                    display.ReadData(CNRDLCWeaponSystem.BuildBussinInfo(false), false);
                    CNRDLCWeaponThumbnail.Apply(display.LogoSprite.gameObject);
                }
                else
                {
                    CNRDLCWeaponThumbnail.Clear(display.LogoSprite.gameObject);
                }
            }
        }

    }

    public class CNRDLCWeaponProfileCard : MonoBehaviour
    {
        public CNRDLCWeaponProfileHook Hook;
        public GameObject LogoObject;

        private void OnClick()
        {
            if (Hook != null) Hook.ToggleEquip();
        }

        public void RefreshVisual()
        {
            GItemInfo info = CNRDLCWeaponSystem.BuildBussinInfo(false);
            UILabel[] labels = GetComponentsInChildren<UILabel>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                string n = labels[i].gameObject.name.ToLowerInvariant();
                if (n.Contains("name")) labels[i].text = CNRDLCWeaponSystem.BussinName;
            }

            UISprite[] sprites = GetComponentsInChildren<UISprite>(true);
            for (int i = 0; i < sprites.Length; i++)
            {
                string n = sprites[i].gameObject.name.ToLowerInvariant();
                if (n.Contains("equipment") || n.Contains("equiped"))
                    sprites[i].gameObject.SetActive(info.mIsEquipped);
            }
            CNRDLCWeaponThumbnail.Apply(LogoObject);
        }
    }

    [Serializable]
    public class CNRDLCWeaponModelBundle
    {
        public int version;
        public string id;
        public string name;
        public string donor;
        public string credit;
        public string source;
        public string license;
        public float[] vertices;
        public float[] uv;
        public int[] triangles;
        public string texturePngBase64;
    }

    public static class CNRDLCWeaponThumbnail
    {
        private const string ThumbnailFile = "bussin_thumb.png";
        private const string ThumbnailHashPref = "CNR_BussinThumbHash";
        private const int PreviewLayer = 31;
        private static Texture2D _thumbnail;

        public static Texture2D GetOrCreate()
        {
            if (_thumbnail != null) return _thumbnail;
            if (!CNRDLCWeaponSystem.HasDownloadedModel()) return null;

            string modelHash = "";
            OfficialGunEntry entry = CNRDLCWeaponSystem.FindBussinManifestEntry();
            if (entry != null) modelHash = entry.Hash ?? "";
            string path = ContentManager.GunCacheDir + ThumbnailFile;

            try
            {
                if (File.Exists(path) && PlayerPrefs.GetString(ThumbnailHashPref, "") == modelHash)
                {
                    byte[] cached = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    if (tex.LoadImage(cached))
                    {
                        tex.name = "BussinThumbnail";
                        _thumbnail = tex;
                        return _thumbnail;
                    }
                    UnityEngine.Object.Destroy(tex);
                }
            }
            catch { }

            _thumbnail = RenderThumbnail(path, modelHash);
            return _thumbnail;
        }

        public static void Apply(GameObject logoObject)
        {
            if (logoObject == null) return;
            Texture2D tex = GetOrCreate();
            if (tex == null) return;

            UISprite sprite = logoObject.GetComponent<UISprite>();
            Transform child = logoObject.transform.Find("CNRDLCWeaponThumbnail");
            GameObject go;
            if (child != null) go = child.gameObject;
            else
            {
                go = new GameObject("CNRDLCWeaponThumbnail");
                go.transform.parent = logoObject.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.layer = logoObject.layer;
            }

            UITexture widget = go.GetComponent<UITexture>();
            if (widget == null) widget = go.AddComponent<UITexture>();
            widget.mainTexture = tex;
            if (sprite != null)
            {
                widget.depth = sprite.depth + 1;
                widget.pivot = sprite.pivot;
                sprite.enabled = false;
            }
            widget.enabled = true;
            widget.MarkAsChanged();
        }

        public static void Clear(GameObject logoObject)
        {
            if (logoObject == null) return;
            Transform child = logoObject.transform.Find("CNRDLCWeaponThumbnail");
            if (child != null)
            {
                UITexture widget = child.GetComponent<UITexture>();
                if (widget != null) widget.enabled = false;
            }
            UISprite sprite = logoObject.GetComponent<UISprite>();
            if (sprite != null) sprite.enabled = true;
        }

        private static Texture2D RenderThumbnail(string path, string modelHash)
        {
            GameObject preview = null;
            GameObject camGo = null;
            RenderTexture rt = null;
            RenderTexture oldActive = RenderTexture.active;
            try
            {
                preview = BuildPreviewModel();
                if (preview == null) return null;

                Vector3 previewBase = new Vector3(12000f, 12000f, 12000f);
                preview.transform.position = previewBase;
                SetLayerRecursive(preview.transform, PreviewLayer);

                Renderer renderer = preview.GetComponent<Renderer>();
                if (renderer == null) renderer = preview.GetComponentInChildren<Renderer>();
                if (renderer == null) return null;
                Bounds bounds = renderer.bounds;

                camGo = new GameObject("CNR_Bussin_ThumbnailCamera");
                Camera cam = camGo.AddComponent<Camera>();
                cam.enabled = false;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
                cam.cullingMask = 1 << PreviewLayer;
                cam.orthographic = true;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 10f;

                const int width = 256;
                const int height = 128;
                cam.aspect = (float)width / (float)height;
                float verticalNeed = bounds.extents.y * 1.35f;
                float horizontalNeed = bounds.extents.z / cam.aspect * 1.25f;
                cam.orthographicSize = Mathf.Max(0.18f, Mathf.Max(verticalNeed, horizontalNeed));
                cam.transform.position = bounds.center + new Vector3(2.5f, 0.30f, -0.12f);
                cam.transform.rotation = Quaternion.LookRotation(bounds.center - cam.transform.position, Vector3.up);

                rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                rt.name = "BussinThumbnailRT";
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
                result.name = "BussinThumbnail";
                result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                result.Apply();

                try
                {
                    File.WriteAllBytes(path, result.EncodeToPNG());
                    PlayerPrefs.SetString(ThumbnailHashPref, modelHash);
                    PlayerPrefs.Save();
                }
                catch (Exception ex) { ModEntry.Log("CNR DLC weapons: thumbnail cache write failed: " + ex.Message); }

                ModEntry.Log("CNR DLC weapons: rendered Bussin' thumbnail from DLC model");
                return result;
            }
            catch (Exception ex)
            {
                ModEntry.Log("CNR DLC weapons: thumbnail render failed: " + ex.Message);
                return null;
            }
            finally
            {
                RenderTexture.active = oldActive;
                if (rt != null)
                {
                    rt.Release();
                    UnityEngine.Object.Destroy(rt);
                }
                if (camGo != null) UnityEngine.Object.Destroy(camGo);
                if (preview != null) UnityEngine.Object.Destroy(preview);
            }
        }

        private static GameObject BuildPreviewModel()
        {
            string json = File.ReadAllText(ContentManager.GunCacheDir + CNRDLCWeaponSystem.BussinId + ".json");
            CNRDLCWeaponModelBundle data = JsonReader.Deserialize<CNRDLCWeaponModelBundle>(json);
            if (data == null || data.vertices == null || data.triangles == null || data.vertices.Length < 9) return null;

            Vector3[] vertices = new Vector3[data.vertices.Length / 3];
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(data.vertices[i * 3], data.vertices[i * 3 + 1], data.vertices[i * 3 + 2]);

            Vector2[] uv = new Vector2[vertices.Length];
            if (data.uv != null)
            {
                int uvCount = Mathf.Min(vertices.Length, data.uv.Length / 2);
                for (int i = 0; i < uvCount; i++) uv[i] = new Vector2(data.uv[i * 2], data.uv[i * 2 + 1]);
            }

            Mesh mesh = new Mesh();
            mesh.name = "BussinPreviewMesh";
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = data.triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Shader shader = Shader.Find("Unlit/Texture");
            if (shader == null) shader = Shader.Find("Diffuse");
            Material mat = new Material(shader);
            mat.name = "BussinPreviewMaterial";
            if (!string.IsNullOrEmpty(data.texturePngBase64))
            {
                byte[] png = Convert.FromBase64String(data.texturePngBase64);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.name = "BussinPreviewTexture";
                tex.LoadImage(png);
                mat.mainTexture = tex;
            }

            GameObject go = new GameObject("CNR_Bussin_ThumbnailModel");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
            return go;
        }

        private static void SetLayerRecursive(Transform t, int layer)
        {
            if (t == null) return;
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++) SetLayerRecursive(t.GetChild(i), layer);
        }
    }

    public class CNRDLCWeaponRuntime : MonoBehaviour
    {
        private const float BussinRecoilVelocity = 3.8f;
        private const float KnifeBackhopGain = 0.45f;
        private const float KnifeBackhopMaxHorizontal = 8.75f;

        private WeaponScript _bussinWeapon;
        private int _lastShells = -1;
        private GameObject _customModel;
        private readonly Dictionary<Renderer, bool> _hidden = new Dictionary<Renderer, bool>();
        private bool _rifleOverrideActive;
        private bool _wasGrounded;
        private float _nextWeaponScan;

        private void Start()
        {
            CNRDLCWeaponSystem.ClearMarkerIfDonorMissing();
            _wasGrounded = true;
        }

        private void Update()
        {
            CNRDLCWeaponSystem.ClearMarkerIfDonorMissing();

            bool bussin = CNRDLCWeaponSystem.IsBussinEquipped();
            if (bussin)
            {
                if (!_rifleOverrideActive)
                {
                    CNRDLCWeaponSystem.BeginRifleOverride();
                    _rifleOverrideActive = true;
                }
                if (Time.time >= _nextWeaponScan || _bussinWeapon == null || !_bussinWeapon.gameObject.activeInHierarchy)
                {
                    _nextWeaponScan = Time.time + 0.20f;
                    BindActiveBussinWeapon();
                }
                WatchBussinShot();
            }
            else
            {
                UnbindBussinWeapon();
                if (_rifleOverrideActive)
                {
                    CNRDLCWeaponSystem.EndRifleOverride();
                    _rifleOverrideActive = false;
                }
            }

            UpdateKnifeMovement();
        }

        private void BindActiveBussinWeapon()
        {
            WeaponScript found = null;
            WeaponScript[] weapons = (WeaponScript[])Resources.FindObjectsOfTypeAll(typeof(WeaponScript));
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponScript ws = weapons[i];
                if (ws == null || !ws.gameObject.activeInHierarchy) continue;
                if (ws.transform.root == null || ws.transform.root.tag != "Player") continue;
                if (ws.weaponName == CNRDLCWeaponSystem.BussinDonor)
                {
                    found = ws;
                    break;
                }
            }

            if (found == _bussinWeapon) return;
            UnbindBussinWeapon();
            _bussinWeapon = found;
            if (_bussinWeapon == null) return;

            ApplyBussinAmmoLevel(_bussinWeapon);
            _lastShells = _bussinWeapon.ShotGun != null ? _bussinWeapon.ShotGun.bulletsLeft : -1;
            AttachBussinModel(_bussinWeapon);
        }

        private void ApplyBussinAmmoLevel(WeaponScript ws)
        {
            if (ws == null || ws.ShotGun == null) return;
            int level = CNRDLCWeaponSystem.GetBussinLevel();
            int clip = level == 1 ? 10 : level == 2 ? 12 : 15;
            int reserve = level == 1 ? 80 : level == 2 ? 120 : 150;
            ws.ShotGun.bulletsPerClip = clip;
            ws.ShotGun.bulletsLeft = clip;
            ws.ShotGun.clips = reserve;
        }

        private void WatchBussinShot()
        {
            if (_bussinWeapon == null || _bussinWeapon.ShotGun == null || !_bussinWeapon.gameObject.activeInHierarchy) return;
            int shells = _bussinWeapon.ShotGun.bulletsLeft;
            if (_lastShells >= 0 && shells < _lastShells) ApplyBussinRecoil();
            _lastShells = shells;
        }

        private void ApplyBussinRecoil()
        {
            FPScontroller fps = FindLocalController();
            Camera cam = Camera.main;
            if (fps == null || fps.movement == null || cam == null) return;

            Vector3 velocity = fps.movement.velocity;
            velocity += -cam.transform.forward * BussinRecoilVelocity;
            fps.SetVelocity(velocity);
        }

        private void AttachBussinModel(WeaponScript ws)
        {
            if (ws == null || !CNRDLCWeaponSystem.HasDownloadedModel()) return;
            try
            {
                string json = File.ReadAllText(ContentManager.GunCacheDir + CNRDLCWeaponSystem.BussinId + ".json");
                CNRDLCWeaponModelBundle data = JsonReader.Deserialize<CNRDLCWeaponModelBundle>(json);
                if (data == null || data.vertices == null || data.triangles == null || data.vertices.Length < 9) return;

                Vector3[] vertices = new Vector3[data.vertices.Length / 3];
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = new Vector3(data.vertices[i * 3], data.vertices[i * 3 + 1], data.vertices[i * 3 + 2]);

                Vector2[] uv = new Vector2[vertices.Length];
                if (data.uv != null)
                {
                    int uvCount = Mathf.Min(vertices.Length, data.uv.Length / 2);
                    for (int i = 0; i < uvCount; i++) uv[i] = new Vector2(data.uv[i * 2], data.uv[i * 2 + 1]);
                }

                Mesh mesh = new Mesh();
                mesh.name = "BussinMesh";
                mesh.vertices = vertices;
                mesh.uv = uv;
                mesh.triangles = data.triangles;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                Shader shader = Shader.Find("Diffuse");
                Renderer[] donorRenderers = ws.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < donorRenderers.Length; i++)
                {
                    Renderer r = donorRenderers[i];
                    if (r == null) continue;
                    if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
                    if (!_hidden.ContainsKey(r)) _hidden[r] = r.enabled;
                    if (r.sharedMaterial != null && r.sharedMaterial.shader != null) shader = r.sharedMaterial.shader;
                    r.enabled = false;
                }

                Material mat = new Material(shader);
                mat.name = "BussinMaterial";
                if (!string.IsNullOrEmpty(data.texturePngBase64))
                {
                    byte[] png = Convert.FromBase64String(data.texturePngBase64);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    tex.name = "BussinTexture";
                    tex.LoadImage(png);
                    mat.mainTexture = tex;
                }

                _customModel = new GameObject("CNR_Bussin_Model");
                _customModel.transform.parent = ws.transform;
                _customModel.transform.localPosition = Vector3.zero;
                _customModel.transform.localRotation = Quaternion.identity;
                _customModel.transform.localScale = Vector3.one;
                MeshFilter mf = _customModel.AddComponent<MeshFilter>();
                mf.mesh = mesh;
                MeshRenderer mr = _customModel.AddComponent<MeshRenderer>();
                mr.material = mat;
                ModEntry.Log("CNR DLC weapons: attached Bussin' model (" + vertices.Length + " vertices)");
            }
            catch (Exception ex)
            {
                ModEntry.Log("CNR DLC weapons: model load failed: " + ex.Message);
            }
        }

        private void UnbindBussinWeapon()
        {
            if (_customModel != null) Destroy(_customModel);
            _customModel = null;
            foreach (KeyValuePair<Renderer, bool> kv in _hidden)
                if (kv.Key != null) kv.Key.enabled = kv.Value;
            _hidden.Clear();
            _bussinWeapon = null;
            _lastShells = -1;
        }

        private void UpdateKnifeMovement()
        {
            FPScontroller fps = FindLocalController();
            if (fps == null || fps.movement == null) return;

            bool grounded = fps.grounded;
            if (_wasGrounded && !grounded && fps.movement.velocity.y > 0.05f &&
                !fps.onLadder && !fps.crouch && !fps.prone && IsActiveLocalKnife())
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 forward = cam.transform.forward;
                    forward.y = 0f;
                    if (forward.sqrMagnitude > 0.001f) forward.Normalize();

                    Vector3 input = fps.inputMoveDirection;
                    input.y = 0f;
                    bool backing = input.sqrMagnitude > 0.05f && Vector3.Dot(input.normalized, forward) < -0.35f;
                    if (backing)
                    {
                        Vector3 velocity = fps.movement.velocity;
                        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
                        Vector3 boosted = horizontal + (-forward * KnifeBackhopGain);
                        if (boosted.magnitude > KnifeBackhopMaxHorizontal)
                            boosted = boosted.normalized * KnifeBackhopMaxHorizontal;
                        velocity.x = boosted.x;
                        velocity.z = boosted.z;
                        fps.SetVelocity(velocity);
                    }
                }
            }
            _wasGrounded = grounded;
        }

        private static bool IsActiveLocalKnife()
        {
            WeaponScript[] weapons = (WeaponScript[])Resources.FindObjectsOfTypeAll(typeof(WeaponScript));
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponScript ws = weapons[i];
                if (ws == null || !ws.gameObject.activeInHierarchy) continue;
                if (ws.transform.root == null || ws.transform.root.tag != "Player") continue;
                if (ws.GunType == WeaponScript.gunType.KNIFE) return true;
            }
            return false;
        }

        private static FPScontroller FindLocalController()
        {
            GameObject local = GameObject.Find("ExampleCharacter");
            if (local != null)
            {
                FPScontroller c = local.GetComponent<FPScontroller>();
                if (c == null) c = local.GetComponentInChildren<FPScontroller>();
                if (c != null) return c;
            }
            return UnityEngine.Object.FindObjectOfType(typeof(FPScontroller)) as FPScontroller;
        }

        private void OnDestroy()
        {
            UnbindBussinWeapon();
            if (_rifleOverrideActive)
            {
                CNRDLCWeaponSystem.EndRifleOverride();
                _rifleOverrideActive = false;
            }
        }
    }
}
