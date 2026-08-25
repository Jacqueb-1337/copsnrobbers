using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            return IsBussinOwned() && PlayerPrefs.GetString(EquippedKey, "") == BussinId && CountDonorSlots() > 0;
        }

        // Bussin is represented in the persisted vanilla loadout by an extra M87T
        // entry, always the last M87T occurrence. That keeps every vanilla loadout
        // reader happy while still allowing a real M87T and Bussin to coexist.
        public static bool IsDonorEquipped()
        {
            int count = CountDonorSlots();
            return IsBussinEquipped() ? count > 1 : count > 0;
        }

        public static int GetBussinLogicalIndex()
        {
            if (!IsBussinEquipped()) return -1;
            string[] names = GetCompactLoadout();
            for (int i = names.Length - 1; i >= 0; i--)
                if (names[i] == BussinDonor) return i;
            return -1;
        }

        private static int CountDonorSlots()
        {
            string[] names = GetCompactLoadout();
            int count = 0;
            for (int i = 0; i < names.Length; i++) if (names[i] == BussinDonor) count++;
            return count;
        }

        private static string[] GetCompactLoadout()
        {
            try { return GrowthManagerKit.GetCurEquippedWeaponNameList() ?? new string[0]; }
            catch { return new string[0]; }
        }

        private static void WriteCompactLoadout(List<string> names)
        {
            int limit = GrowthManagerKit.GetCurWeaponEquipLimitedNum();
            for (int i = 0; i < limit; i++)
                PlayerPrefs.SetString("CurWeaponEquiped_" + (i + 1), i < names.Count ? names[i] : string.Empty);
            PlayerPrefs.Save();
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

            List<string> names = new List<string>(GetCompactLoadout());
            if (IsBussinEquipped())
            {
                // Bussin is always the last M87T occurrence. Remove only that
                // occurrence so a separately-equipped real M87T is preserved.
                for (int i = names.Count - 1; i >= 0; i--)
                {
                    if (names[i] != BussinDonor) continue;
                    names.RemoveAt(i);
                    break;
                }
                PlayerPrefs.DeleteKey(EquippedKey);
                WriteCompactLoadout(names);
                return;
            }

            int limit = GrowthManagerKit.GetCurWeaponEquipLimitedNum();
            if (names.Count >= limit && names.Count > 0)
                names.RemoveAt(0); // exactly vanilla's full-loadout replacement rule
            names.Add(BussinDonor);
            WriteCompactLoadout(names);
            PlayerPrefs.SetString(EquippedKey, BussinId);
            PlayerPrefs.Save();
        }

        // Called after vanilla's M87T card click. If Bussin was the only M87T
        // placeholder before the click, vanilla interprets that click as an
        // unequip. Rebuild the result as though M87T were a distinct weapon while
        // keeping Bussin as the final M87T occurrence for unambiguous slot identity.
        public static void PreserveBussinWhenEquippingRealDonor(string[] before)
        {
            if (before == null || before.Length == 0) return;
            int limit = GrowthManagerKit.GetCurWeaponEquipLimitedNum();
            List<string> names = new List<string>(before);
            int bussinIndex = -1;
            for (int i = names.Count - 1; i >= 0; i--)
                if (names[i] == BussinDonor) { bussinIndex = i; break; }
            if (bussinIndex < 0) return;

            if (names.Count >= limit)
            {
                // Vanilla replaces the oldest slot when equipping a new distinct gun.
                if (bussinIndex == 0)
                {
                    names.RemoveAt(0);       // Bussin was the oldest and is replaced.
                    names.Add(BussinDonor); // New entry is the real M87T.
                    PlayerPrefs.DeleteKey(EquippedKey);
                    WriteCompactLoadout(names);
                    return;
                }
                names.RemoveAt(0);
                bussinIndex--;
            }

            // Add the real M87T immediately before Bussin. Relative ordering of all
            // other vanilla weapons is unchanged and Bussin remains identifiable.
            names.Insert(bussinIndex, BussinDonor);
            while (names.Count > limit) names.RemoveAt(0);
            WriteCompactLoadout(names);
            PlayerPrefs.SetString(EquippedKey, BussinId);
            PlayerPrefs.Save();
        }

        public static void ClearMarkerIfDonorMissing()
        {
            if (PlayerPrefs.GetString(EquippedKey, "") == BussinId && CountDonorSlots() == 0)
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
            CNRDLCWeaponThumbnail.ApplyCard(_card.logoSprite);
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
            if (_card != null) _card.RefreshVisual();
            RefreshDonorEquippedMarker();
        }

        public void RefreshAfterDonorClick()
        {
            if (_profile != null) _profile.UpdateWeaponData();
            RefreshEquippedWindows();
            RefreshCard();
            _lastEquipSignature = BuildEquipSignature();
        }

        private void RefreshDonorEquippedMarker()
        {
            if (_profile == null || _profile.weaponObjParent == null) return;
            bool donorEquipped = CNRDLCWeaponSystem.IsDonorEquipped();
            UIProfileWeaponPrefab[] cards = _profile.weaponObjParent.GetComponentsInChildren<UIProfileWeaponPrefab>(true);
            for (int i = 0; i < cards.Length; i++)
            {
                UIProfileWeaponPrefab card = cards[i];
                if (card == null || card.gItemInfo == null || card.gItemInfo.mName != CNRDLCWeaponSystem.BussinDonor) continue;
                if (card.equipmentedSprite != null) card.equipmentedSprite.SetActive(donorEquipped);
                CNRDLCM87TProfileGuard guard = card.GetComponent<CNRDLCM87TProfileGuard>();
                if (guard == null) guard = card.gameObject.AddComponent<CNRDLCM87TProfileGuard>();
                guard.Hook = this;
            }
        }

        private void RefreshEquippedWindows()
        {
            if (_profile == null || _profile.equipedWindow == null) return;
            int bussinIndex = CNRDLCWeaponSystem.GetBussinLogicalIndex();

            for (int i = 0; i < _profile.equipedWindow.Length; i++)
            {
                GameObject window = _profile.equipedWindow[i];
                if (window == null) continue;
                UIProfileEquipedDisplay display = window.GetComponent<UIProfileEquipedDisplay>();
                if (display == null || display.LogoSprite == null) continue;

                if (i == bussinIndex)
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
            CNRDLCWeaponThumbnail.ApplyCard(LogoObject);
        }
    }

    public class CNRDLCM87TProfileGuard : MonoBehaviour
    {
        public CNRDLCWeaponProfileHook Hook;
        private string[] _before;
        private bool _bussinBefore;
        private bool _realBefore;

        private void OnPress(bool pressed)
        {
            if (!pressed) return;
            try { _before = GrowthManagerKit.GetCurEquippedWeaponNameList(); }
            catch { _before = null; }
            _bussinBefore = CNRDLCWeaponSystem.IsBussinEquipped();
            _realBefore = CNRDLCWeaponSystem.IsDonorEquipped();
        }

        private void OnClick()
        {
            if (_bussinBefore && !_realBefore) StartCoroutine(FixAfterVanillaClick());
        }

        private IEnumerator FixAfterVanillaClick()
        {
            yield return null;
            CNRDLCWeaponSystem.PreserveBussinWhenEquippingRealDonor(_before);
            if (Hook != null) Hook.RefreshAfterDonorClick();
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
        public string cardPngBase64;
        public string hudPngBase64;
        public string cardUrl;
        public string hudUrl;
    }

    public static class CNRDLCWeaponThumbnail
    {
        private const string ThumbnailFile = "bussin_thumb_v4.png";
        private const string ThumbnailHashPref = "CNR_BussinThumbHashV4";
        private const int PreviewLayer = 30;
        private static Texture2D _thumbnail;
        private static Texture2D _hudIcon;

        private static Texture2D LoadBundledArt(bool hud)
        {
            try
            {
                string path = ContentManager.GunCacheDir + CNRDLCWeaponSystem.BussinId + ".json";
                if (!File.Exists(path)) return null;
                CNRDLCWeaponModelBundle data = JsonReader.Deserialize<CNRDLCWeaponModelBundle>(File.ReadAllText(path));
                if (data == null) return null;
                string encoded = hud ? data.hudPngBase64 : data.cardPngBase64;
                if (string.IsNullOrEmpty(encoded)) return null;
                byte[] png = Convert.FromBase64String(encoded);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.name = hud ? "BussinHudIcon" : "BussinCardIcon";
                if (!tex.LoadImage(png)) { UnityEngine.Object.Destroy(tex); return null; }
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.anisoLevel = 0;
                return tex;
            }
            catch (Exception ex)
            {
                ModEntry.Log("CNR DLC weapons: bundled art load failed: " + ex.Message);
                return null;
            }
        }

        public static Texture2D GetOrCreate()
        {
            if (_thumbnail != null) return _thumbnail;
            if (!CNRDLCWeaponSystem.HasDownloadedModel()) return null;

            _thumbnail = LoadBundledArt(false);
            if (_thumbnail != null) return _thumbnail;

            string modelHash = "";
            OfficialGunEntry entry = CNRDLCWeaponSystem.FindBussinManifestEntry();
            if (entry != null) modelHash = entry.Hash ?? "";
            string path = ContentManager.GunCacheDir + ThumbnailFile;
            _thumbnail = RenderThumbnail(path, modelHash);
            return _thumbnail;
        }

        public static Texture2D GetHudOrCreate()
        {
            if (_hudIcon != null) return _hudIcon;
            if (!CNRDLCWeaponSystem.HasDownloadedModel()) return null;
            _hudIcon = LoadBundledArt(true);
            if (_hudIcon == null) _hudIcon = GetOrCreate();
            return _hudIcon;
        }

        private const string CardAtlasSpriteName = "CNR_Bussin_Card";
        private static UIAtlas _cardAtlas;
        private static Material _cardAtlasMaterial;

        private static UIAtlas GetOrCreateCardAtlas(UISprite donor, Texture2D tex)
        {
            if (_cardAtlas != null)
            {
                if (_cardAtlas.spriteMaterial != null && _cardAtlas.spriteMaterial.mainTexture != tex)
                    _cardAtlas.spriteMaterial.mainTexture = tex;
                return _cardAtlas;
            }

            Shader shader = null;
            Material donorMaterial = donor != null && donor.atlas != null ? donor.atlas.spriteMaterial : null;
            if (donorMaterial != null) shader = donorMaterial.shader;
            if (shader == null) shader = Shader.Find("Unlit/Transparent Colored");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) return null;

            GameObject atlasObject = new GameObject("CNRBussinCardAtlas");
            UnityEngine.Object.DontDestroyOnLoad(atlasObject);
            UIAtlas atlas = atlasObject.AddComponent<UIAtlas>();

            Material material = donorMaterial != null ? new Material(donorMaterial) : new Material(shader);
            material.name = "CNR Bussin Card Material";
            material.shader = shader;
            material.mainTexture = tex;
            _cardAtlasMaterial = material;
            atlas.spriteMaterial = material;
            atlas.pixelSize = 1f;

            UIAtlas.Sprite atlasSprite = new UIAtlas.Sprite();
            atlasSprite.name = CardAtlasSpriteName;
            atlasSprite.outer = new Rect(0f, 0f, tex.width, tex.height);
            atlasSprite.inner = atlasSprite.outer;
            atlas.spriteList = new List<UIAtlas.Sprite> { atlasSprite };
            _cardAtlas = atlas;
            return atlas;
        }

        // Store and Profile use dedicated cloned cards. Keep their original UISprite
        // in the vanilla NGUI draw path and swap only its atlas/material. A newly
        // created UITexture sibling is not reliably registered by this old NGUI build.
        public static void ApplyCard(GameObject logoObject)
        {
            if (logoObject == null) return;
            Texture2D tex = GetOrCreate();
            if (tex == null) return;

            UISprite sprite = logoObject.GetComponent<UISprite>();
            if (sprite == null) { Apply(logoObject); return; }
            UIAtlas atlas = GetOrCreateCardAtlas(sprite, tex);
            if (atlas == null) { Apply(logoObject); return; }

            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform oldOverlay = holder.Find("CNRDLCWeaponThumbnail");
            if (oldOverlay != null)
            {
                UITexture oldWidget = oldOverlay.GetComponent<UITexture>();
                if (oldWidget != null) oldWidget.enabled = false;
            }

            sprite.atlas = atlas;
            sprite.spriteName = CardAtlasSpriteName;
            sprite.type = UISprite.Type.Simple;
            sprite.color = Color.white;
            sprite.enabled = true;
            sprite.MarkAsChanged();
        }

        public static void Apply(GameObject logoObject)
        {
            if (logoObject == null) return;
            Texture2D tex = GetOrCreate();
            if (tex == null) return;

            UISprite sprite = logoObject.GetComponent<UISprite>();
            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform child = holder.Find("CNRDLCWeaponThumbnail");
            GameObject go;
            if (child != null) go = child.gameObject;
            else
            {
                go = new GameObject("CNRDLCWeaponThumbnail");
                go.transform.parent = holder;
                if (holder == logoObject.transform)
                {
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                }
                else
                {
                    // Render as a sibling of the vanilla UISprite. Old NGUI can collapse
                    // a UITexture nested under a scaled UISprite to an effectively blank
                    // quad, so mirror the sprite transform directly instead.
                    go.transform.localPosition = logoObject.transform.localPosition;
                    go.transform.localRotation = logoObject.transform.localRotation;
                    go.transform.localScale = logoObject.transform.localScale;
                }
                go.layer = logoObject.layer;
            }

            UITexture widget = go.GetComponent<UITexture>();
            if (widget == null) widget = go.AddComponent<UITexture>();
            widget.mainTexture = tex;
            widget.color = Color.white;
            if (sprite != null)
            {
                widget.depth = sprite.depth + 1;
                widget.pivot = sprite.pivot;
                sprite.enabled = false;
            }
            widget.enabled = true;
            widget.MarkAsChanged();
        }

        public static void ApplyHud(GameObject logoObject)
        {
            if (logoObject == null) return;
            Texture2D tex = GetHudOrCreate();
            if (tex == null) return;

            UISprite sprite = logoObject.GetComponent<UISprite>();
            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform child = holder.Find("CNRDLCWeaponHudIcon");
            GameObject go;
            if (child != null) go = child.gameObject;
            else
            {
                go = new GameObject("CNRDLCWeaponHudIcon");
                go.transform.parent = holder;
                if (holder == logoObject.transform)
                {
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                }
                else
                {
                    go.transform.localPosition = logoObject.transform.localPosition;
                    go.transform.localRotation = logoObject.transform.localRotation;
                    go.transform.localScale = logoObject.transform.localScale;
                }
                go.layer = logoObject.layer;
            }

            UITexture widget = go.GetComponent<UITexture>();
            if (widget == null) widget = go.AddComponent<UITexture>();
            widget.mainTexture = tex;
            widget.color = Color.white;
            if (sprite != null)
            {
                widget.depth = sprite.depth + 1;
                widget.pivot = sprite.pivot;
                sprite.enabled = false;
            }
            widget.enabled = true;
            widget.MarkAsChanged();
        }

        public static void ClearHud(GameObject logoObject)
        {
            if (logoObject == null) return;
            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform child = holder.Find("CNRDLCWeaponHudIcon");
            if (child != null)
            {
                UITexture widget = child.GetComponent<UITexture>();
                if (widget != null) widget.enabled = false;
            }
            UISprite sprite = logoObject.GetComponent<UISprite>();
            if (sprite != null) sprite.enabled = true;
        }

        public static void Clear(GameObject logoObject)
        {
            if (logoObject == null) return;
            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform child = holder.Find("CNRDLCWeaponThumbnail");
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
                preview.transform.localRotation = Quaternion.Euler(5f, 270f, -3f);
                SetLayerRecursive(preview.transform, PreviewLayer);

                Renderer renderer = preview.GetComponent<Renderer>();
                if (renderer == null) renderer = preview.GetComponentInChildren<Renderer>();
                if (renderer == null) return null;
                Bounds bounds = renderer.bounds;

                camGo = new GameObject("CNR_Bussin_ThumbnailCamera");
                Camera cam = camGo.AddComponent<Camera>();
                cam.enabled = false;
                cam.clearFlags = CameraClearFlags.SolidColor;
                // Unity 4 can return an all-zero alpha channel from a transparent
                // RenderTexture. Render against a keyed opaque color and remove it
                // after ReadPixels so the resulting PNG is reliably non-blank.
                cam.backgroundColor = new Color(1f, 0f, 1f, 1f);
                cam.cullingMask = 1 << PreviewLayer;
                cam.orthographic = true;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 20f;

                const int width = 256;
                const int height = 128;
                cam.aspect = (float)width / (float)height;
                float verticalNeed = bounds.extents.y * 1.40f;
                float horizontalNeed = Mathf.Max(bounds.extents.x, bounds.extents.z) / cam.aspect * 1.40f;
                cam.orthographicSize = Mathf.Max(0.18f, Mathf.Max(verticalNeed, horizontalNeed));
                float distance = Mathf.Max(2f, bounds.extents.magnitude * 4f);
                Vector3 viewDir = new Vector3(1f, 0.20f, -0.20f).normalized;
                cam.transform.position = bounds.center + viewDir * distance;
                cam.transform.rotation = Quaternion.LookRotation(bounds.center - cam.transform.position, Vector3.up);

                rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                rt.name = "BussinThumbnailRT";
                rt.antiAliasing = 1;
                rt.Create();
                cam.targetTexture = rt;
                RenderTexture.active = rt;
                GL.Clear(true, true, cam.backgroundColor);
                cam.Render();

                Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
                result.name = "BussinThumbnail";
                result.filterMode = FilterMode.Point;
                result.wrapMode = TextureWrapMode.Clamp;
                result.anisoLevel = 0;
                result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                result.Apply();

                Color32[] pixels = result.GetPixels32();
                int visiblePixels = 0;
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color32 c = pixels[i];
                    bool keyed = c.r > 220 && c.g < 65 && c.b > 220;
                    if (keyed) c.a = 0;
                    else { c.a = 255; visiblePixels++; }
                    pixels[i] = c;
                }
                result.SetPixels32(pixels);
                result.Apply();

                if (visiblePixels < 32)
                {
                    ModEntry.Log("CNR DLC weapons: thumbnail render produced too few visible pixels (" + visiblePixels + ")");
                    UnityEngine.Object.Destroy(result);
                    return null;
                }

                try
                {
                    File.WriteAllBytes(path, result.EncodeToPNG());
                    PlayerPrefs.SetString(ThumbnailHashPref, modelHash);
                    PlayerPrefs.Save();
                }
                catch (Exception ex) { ModEntry.Log("CNR DLC weapons: thumbnail cache write failed: " + ex.Message); }

                ModEntry.Log("CNR DLC weapons: rendered Bussin' thumbnail from DLC model pixels=" + visiblePixels);
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

            int sourceCount = data.vertices.Length / 3;
            Vector3[] sourceVertices = new Vector3[sourceCount];
            for (int i = 0; i < sourceCount; i++)
                sourceVertices[i] = new Vector3(data.vertices[i * 3], data.vertices[i * 3 + 1], data.vertices[i * 3 + 2]);

            Vector2[] sourceUv = new Vector2[sourceCount];
            if (data.uv != null)
            {
                int uvCount = Mathf.Min(sourceCount, data.uv.Length / 2);
                for (int i = 0; i < uvCount; i++) sourceUv[i] = new Vector2(data.uv[i * 2], 1f - data.uv[i * 2 + 1]);
            }

            // Preview only: duplicate the mesh with reversed winding so the icon is
            // robust to the imported model's original face orientation/backface culling.
            Vector3[] vertices = new Vector3[sourceCount * 2];
            Vector2[] uv = new Vector2[sourceCount * 2];
            for (int i = 0; i < sourceCount; i++)
            {
                vertices[i] = sourceVertices[i];
                vertices[i + sourceCount] = sourceVertices[i];
                uv[i] = sourceUv[i];
                uv[i + sourceCount] = sourceUv[i];
            }
            int[] triangles = new int[data.triangles.Length * 2];
            for (int i = 0; i < data.triangles.Length; i++) triangles[i] = data.triangles[i];
            for (int i = 0; i + 2 < data.triangles.Length; i += 3)
            {
                int o = data.triangles.Length + i;
                triangles[o] = data.triangles[i] + sourceCount;
                triangles[o + 1] = data.triangles[i + 2] + sourceCount;
                triangles[o + 2] = data.triangles[i + 1] + sourceCount;
            }

            Mesh mesh = new Mesh();
            mesh.name = "BussinPreviewMesh";
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Shader shader = Shader.Find("Unlit/Transparent Cutout");
            if (shader == null) shader = Shader.Find("Transparent/Cutout/Diffuse");
            if (shader == null) shader = Shader.Find("Transparent/Cutout/VertexLit");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            Material mat = new Material(shader);
            mat.name = "BussinPreviewMaterial";
            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.5f);
            if (!string.IsNullOrEmpty(data.texturePngBase64))
            {
                byte[] png = Convert.FromBase64String(data.texturePngBase64);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.name = "BussinPreviewTexture";
                tex.LoadImage(png);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.anisoLevel = 0;
                mat.mainTexture = tex;
                mat.color = Color.white;
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

    public class CNRBussinActivationRelay : MonoBehaviour
    {
        private void OnEnable() { CNRDLCWeaponSystem.BeginRifleOverride(); }
        private void OnDisable() { CNRDLCWeaponSystem.EndRifleOverride(); }
        private void OnDestroy() { CNRDLCWeaponSystem.EndRifleOverride(); }
    }

    public class CNRDLCWeaponRuntime : MonoBehaviour
    {
        private const float BussinRecoilVelocity = 12.0f;
        private const float BussinHorizontalDrag = 30.0f;
        private const float BussinFallbackVerticalDrag = 24.0f;
        private const float BussinRisingVerticalScale = 0.22f;
        private const float BussinFallCompensation = 0.85f;
        private const float BussinFallCompensationCap = 24.0f;
        private const float BussinFallFullCompImpulse = 6.0f;
        private const float KnifeBackhopGain = 0.45f;
        private const float KnifeBackhopMaxBonus = 3.0f;
        private const float KnifeBackhopReleaseDrag = 12.0f;

        private WeaponManager _weaponManager;
        private WeaponScript _bussinWeapon;
        private WeaponScript _placeholderWeapon;
        private int _bussinSlotIndex = -1;
        private GameObject _customModel;
        private readonly Dictionary<Renderer, bool> _hidden = new Dictionary<Renderer, bool>();
        private bool _rifleOverrideActive;
        private bool _wasGrounded;
        private float _nextWeaponScan;
        private Vector3 _bussinHorizontalVelocity = Vector3.zero;
        private float _bussinFallbackVerticalVelocity;
        private Vector3 _knifeBackhopBonusVelocity = Vector3.zero;
        private Component _settingsJumpHook;
        private FieldInfo _settingsOwnJumpVelY;
        private FieldInfo _settingsOwnJumpActive;
        private GameObject _hudIconObject;

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
                if (Time.time >= _nextWeaponScan || _bussinWeapon == null)
                {
                    _nextWeaponScan = Time.time + 0.20f;
                    BindActiveBussinWeapon();
                }

                bool bussinActive = _bussinWeapon != null && _bussinWeapon.gameObject.activeInHierarchy;
                if (bussinActive && !_rifleOverrideActive)
                {
                    CNRDLCWeaponSystem.BeginRifleOverride();
                    _rifleOverrideActive = true;
                }
                else if (!bussinActive && _rifleOverrideActive)
                {
                    CNRDLCWeaponSystem.EndRifleOverride();
                    _rifleOverrideActive = false;
                }

                SuppressDonorGunVisuals();
                UpdateBussinHudIcon(bussinActive);
                DriveBussinReloadInput();
                DriveBussinFireInput();
            }
            else
            {
                UpdateBussinHudIcon(false);
                UnbindBussinWeapon();
                if (_rifleOverrideActive)
                {
                    CNRDLCWeaponSystem.EndRifleOverride();
                    _rifleOverrideActive = false;
                }
            }

            UpdateKnifeMovement();
        }

        private void UpdateBussinHudIcon(bool active)
        {
            try
            {
                if (_hudIconObject == null || !_hudIconObject.activeInHierarchy)
                    _hudIconObject = GameObject.Find("SwitchButtonBackground");
                if (_hudIconObject == null) return;
                if (active) CNRDLCWeaponThumbnail.ApplyHud(_hudIconObject);
                else CNRDLCWeaponThumbnail.ClearHud(_hudIconObject);
            }
            catch { }
        }

        private void LateUpdate()
        {
            CharacterController cc = FindLocalCharacterController();
            if (cc == null) return;

            Vector3 extra = _bussinHorizontalVelocity + _knifeBackhopBonusVelocity;
            if (Mathf.Abs(_bussinFallbackVerticalVelocity) > 0.01f)
                extra.y += _bussinFallbackVerticalVelocity;

            if (extra.sqrMagnitude > 0.000001f)
                cc.Move(extra * Time.deltaTime);

            _bussinHorizontalVelocity = Vector3.MoveTowards(
                _bussinHorizontalVelocity, Vector3.zero, BussinHorizontalDrag * Time.deltaTime);

            if (_bussinFallbackVerticalVelocity > 0f)
                _bussinFallbackVerticalVelocity = Mathf.Max(0f,
                    _bussinFallbackVerticalVelocity - BussinFallbackVerticalDrag * Time.deltaTime);
            else if (_bussinFallbackVerticalVelocity < 0f)
                _bussinFallbackVerticalVelocity = Mathf.Min(0f,
                    _bussinFallbackVerticalVelocity + BussinFallbackVerticalDrag * Time.deltaTime);
        }

        private void BindActiveBussinWeapon()
        {
            int logicalIndex = CNRDLCWeaponSystem.GetBussinLogicalIndex();
            if (logicalIndex < 0) return;

            WeaponManager manager = FindLocalWeaponManager();
            if (manager == null || manager.allWeapons == null || manager.allWeaponsTotal == null) return;
            if (_bussinWeapon != null && _weaponManager == manager && _bussinSlotIndex == logicalIndex &&
                logicalIndex < manager.allWeapons.Count && manager.allWeapons[logicalIndex] == _bussinWeapon)
                return;

            UnbindBussinWeapon();

            WeaponScript donor = null;
            for (int i = 0; i < manager.allWeaponsTotal.Count; i++)
            {
                WeaponScript candidate = manager.allWeaponsTotal[i];
                if (candidate != null && candidate.weaponName == CNRDLCWeaponSystem.BussinDonor)
                {
                    donor = candidate;
                    break;
                }
            }
            if (donor == null || logicalIndex >= manager.allWeapons.Count) return;

            _weaponManager = manager;
            _bussinSlotIndex = logicalIndex;
            _placeholderWeapon = manager.allWeapons[logicalIndex];

            GameObject clone = Instantiate(donor.gameObject) as GameObject;
            if (clone == null) { _weaponManager = null; _bussinSlotIndex = -1; return; }
            clone.name = "CNR_Bussin_Runtime";
            clone.transform.parent = donor.transform.parent;
            clone.transform.localPosition = donor.transform.localPosition;
            clone.transform.localRotation = donor.transform.localRotation;
            clone.transform.localScale = donor.transform.localScale;
            clone.SetActiveRecursively(false);

            _bussinWeapon = clone.GetComponent<WeaponScript>();
            clone.AddComponent<CNRBussinActivationRelay>();
            if (_bussinWeapon == null)
            {
                Destroy(clone);
                _weaponManager = null;
                _placeholderWeapon = null;
                _bussinSlotIndex = -1;
                return;
            }

            // Preserve the donor's complete shotgun setup, but make the handful of
            // input-facing flags explicit so a runtime clone cannot inherit a stale
            // disabled state from the prefab instance that happened to be cloned.
            _bussinWeapon.GunType = WeaponScript.gunType.SHOTGUN;
            _bussinWeapon.bPlayer = true;
            _bussinWeapon.singleFire = true;

            // Force the cloned UnityScript weapon through Awake once with Bussin's
            // level, then keep it inactive until its actual loadout slot is selected.
            CNRDLCWeaponSystem.BeginRifleOverride();
            clone.SetActiveRecursively(true);
            clone.SetActiveRecursively(false);
            CNRDLCWeaponSystem.EndRifleOverride();
            ApplyBussinAmmoLevel(_bussinWeapon);

            manager.allWeapons[logicalIndex] = _bussinWeapon;
            AttachBussinModel(_bussinWeapon);

            if (manager.index == logicalIndex)
            {
                if (_placeholderWeapon != null) _placeholderWeapon.gameObject.SetActiveRecursively(false);
                CNRDLCWeaponSystem.BeginRifleOverride();
                _rifleOverrideActive = true;
                clone.SetActiveRecursively(true);
                clone.SendMessage("selectWeapon", SendMessageOptions.DontRequireReceiver);
            }

            ModEntry.Log("CNR DLC weapons: inserted independent Bussin' runtime weapon at loadout index " + logicalIndex);
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

        private void DriveBussinReloadInput()
        {
            if (_bussinWeapon == null || !_bussinWeapon.gameObject.activeInHierarchy) return;
            if (Time.timeScale < 0.01f) return;
            if (PlayerPrefs.GetInt("FpsReload", 0) != 1) return;

            // Consume the same one-shot HUD flag vanilla uses, but route it directly to
            // the active cloned shotgun. Otherwise another WeaponScript.LateUpdate can
            // clear FpsReload before this runtime-created weapon ever sees it.
            PlayerPrefs.SetInt("FpsReload", 0);
            if (_bussinWeapon.ShotGun == null || _bussinWeapon.isReload) return;
            if (_bussinWeapon.ShotGun.bulletsLeft >= _bussinWeapon.ShotGun.bulletsPerClip) return;
            if (_bussinWeapon.ShotGun.clips <= 0) return;

            try
            {
                if (_bussinWeapon.audio != null && _bussinWeapon.ShotGun.reloadSound != null)
                {
                    _bussinWeapon.audio.clip = _bussinWeapon.ShotGun.reloadSound;
                    _bussinWeapon.audio.Play();
                }
                _bussinWeapon.StartCoroutine(_bussinWeapon.shotGunReload());
            }
            catch (Exception ex) { ModEntry.Log("CNR DLC weapons: Bussin' reload failed: " + ex.Message); }
        }

        private void DriveBussinFireInput()
        {
            if (_bussinWeapon == null || !_bussinWeapon.gameObject.activeInHierarchy) return;
            if (Time.timeScale < 0.01f) return;
            if (PlayerPrefs.GetInt("FpsOnFire", 0) != 1) return;
            if (!_bussinWeapon.canFire || _bussinWeapon.isReload || !_bussinWeapon.singleFire) return;
            if (_bussinWeapon.ShotGun == null) return;

            // The stock HUD communicates fire through FpsOnFire. A normally-authored
            // WeaponScript consumes that in LateUpdate, but this cloned legacy
            // UnityScript component can miss that input path. Calling the donor's own
            // rate-limited shotgun method here preserves vanilla projectile/network code.
            // Confirm the custom shot from the actual shell decrement, then add recoil
            // immediately. This avoids relying on the private UnityScript lastShot field.
            int before = _bussinWeapon.ShotGun.bulletsLeft;
            _bussinWeapon.shotGunFire();
            int after = _bussinWeapon.ShotGun.bulletsLeft;
            int fired = Mathf.Max(0, before - after);
            for (int i = 0; i < fired; i++) ApplyBussinRecoil();
        }

        private void ApplyBussinRecoil()
        {
            FPScontroller fps = FindBoundLocalController();
            Vector3 aimForward = FindLocalAimForward(fps);
            if (aimForward.sqrMagnitude <= 0.0001f) return;

            Vector3 impulse = -aimForward.normalized * BussinRecoilVelocity;
            _bussinHorizontalVelocity += new Vector3(impulse.x, 0f, impulse.z);

            // CNRSettingsMod owns the mobile jump arc when present. Feed the vertical
            // component into that same velocity, but scale the impulse from the player's
            // current vertical motion: damp it while already rising and compensate hard
            // falls so a downward blast still produces an actual upward kick.
            float appliedVertical = 0f;
            if (!TryInjectSettingsJumpVelocity(impulse.y, out appliedVertical))
            {
                CharacterController cc = FindLocalCharacterController();
                float currentVertical = cc != null ? cc.velocity.y : 0f;
                appliedVertical = ComputeBussinVerticalImpulse(impulse.y, currentVertical);
                _bussinFallbackVerticalVelocity += appliedVertical;
            }

            ModEntry.Log("CNR DLC weapons: Bussin' recoil impulse=" + impulse +
                " appliedY=" + appliedVertical +
                " horizontal=" + _bussinHorizontalVelocity +
                " fallbackY=" + _bussinFallbackVerticalVelocity);
        }

        private Vector3 FindLocalAimForward(FPScontroller fps)
        {
            try
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Transform direction = player.transform.Find("LookObject/Main Camera/Weapon Camera/Direction");
                    if (direction != null) return direction.TransformDirection(Vector3.forward).normalized;
                    Transform main = player.transform.Find("LookObject/Main Camera");
                    if (main != null) return main.forward.normalized;
                }
            }
            catch { }

            Camera cam = FindLocalCamera(fps);
            return cam != null ? cam.transform.forward.normalized : Vector3.zero;
        }

        private static float ComputeBussinVerticalImpulse(float impulseY, float effectiveVertical)
        {
            // Downward recoil (aiming up) stays literal. The adaptive behavior only
            // applies to upward blast-jump recoil.
            if (impulseY <= 0f) return impulseY;

            // If we're already rising quickly, stacking the full impulse makes jump
            // height explode. Keep the boost noticeable but restrained.
            if (effectiveVertical > 2f)
                return impulseY * BussinRisingVerticalScale;

            // During a fall, a fixed +12 m/s can be swallowed entirely by the game's
            // aggressive descent speed. Add compensation proportional to current fall
            // speed so a meaningful downward shot can reverse the fall instead of only
            // slowing it. Very shallow downward aim receives proportionally less help.
            if (effectiveVertical < -2f)
            {
                float aimWeight = Mathf.Clamp01(impulseY / BussinFallFullCompImpulse);
                float compensation = Mathf.Min(BussinFallCompensationCap,
                    (-effectiveVertical) * BussinFallCompensation * aimWeight);
                return impulseY + compensation;
            }

            // Grounded / near apex: preserve the full camera-derived component.
            return impulseY;
        }

        private bool TryInjectSettingsJumpVelocity(float impulseY, out float appliedImpulseY)
        {
            appliedImpulseY = 0f;
            if (Mathf.Abs(impulseY) <= 0.0001f) return true;
            try
            {
                if (_settingsJumpHook == null || _settingsOwnJumpVelY == null || _settingsOwnJumpActive == null)
                {
                    _settingsJumpHook = null;
                    _settingsOwnJumpVelY = null;
                    _settingsOwnJumpActive = null;

                    GameObject go = GameObject.Find("CNRSettingsMod");
                    if (go == null) return false;
                    Component[] components = go.GetComponents<Component>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        Component c = components[i];
                        if (c == null || c.GetType().FullName != "CNRSettingsMod.SettingsModHook") continue;
                        _settingsJumpHook = c;
                        Type t = c.GetType();
                        _settingsOwnJumpVelY = t.GetField("_ownJumpVelY", BindingFlags.Instance | BindingFlags.NonPublic);
                        _settingsOwnJumpActive = t.GetField("_ownJumpActive", BindingFlags.Instance | BindingFlags.NonPublic);
                        break;
                    }
                }

                if (_settingsJumpHook == null || _settingsOwnJumpVelY == null || _settingsOwnJumpActive == null) return false;

                bool active = Convert.ToBoolean(_settingsOwnJumpActive.GetValue(_settingsJumpHook));
                float current = Convert.ToSingle(_settingsOwnJumpVelY.GetValue(_settingsJumpHook));
                CharacterController cc = FindLocalCharacterController();
                float effectiveVertical = active ? current + Physics.gravity.y : (cc != null ? cc.velocity.y : 0f);
                appliedImpulseY = ComputeBussinVerticalImpulse(impulseY, effectiveVertical);

                if (active)
                    current += appliedImpulseY;
                else
                {
                    // JoyStickController adds Physics.gravity each frame before
                    // SettingsMod's own Y move. Compensate that baseline so the shot's
                    // vertical component becomes the desired net launch velocity.
                    current = -Physics.gravity.y + appliedImpulseY;
                    active = true;
                }

                _settingsOwnJumpVelY.SetValue(_settingsJumpHook, current);
                _settingsOwnJumpActive.SetValue(_settingsJumpHook, active);
                ModEntry.Log("CNR DLC weapons: injected Bussin vertical recoil raw=" + impulseY +
                    " effectiveY=" + effectiveVertical + " applied=" + appliedImpulseY +
                    " jumpVel=" + current);
                return true;
            }
            catch (Exception ex)
            {
                ModEntry.Log("CNR DLC weapons: SettingsMod recoil injection failed: " + ex.Message);
                return false;
            }
        }

        private static CharacterController FindLocalCharacterController()
        {
            try
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null) return null;
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc == null) cc = player.GetComponentInChildren<CharacterController>();
                return cc;
            }
            catch { return null; }
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
                    for (int i = 0; i < uvCount; i++) uv[i] = new Vector2(data.uv[i * 2], 1f - data.uv[i * 2 + 1]);
                }

                Mesh mesh = new Mesh();
                mesh.name = "BussinMesh";
                mesh.vertices = vertices;
                mesh.uv = uv;
                mesh.triangles = data.triangles;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                // Attach to the actual animated M87T body, not the WeaponScript root.
                // This inherits the donor's pump/recoil animation while replacing only
                // the gun geometry; the hands stay vanilla.
                Transform visualRoot = ws.transform.Find("Hands+M87T/fps_hand_M87T/handright/M87T_1");
                if (visualRoot == null) visualRoot = ws.transform.Find("Hands+M87T/M87T");
                if (visualRoot == null) visualRoot = ws.transform;

                List<Renderer> gunRenderers = new List<Renderer>();
                Transform z = visualRoot.Find("Z");
                Transform z1 = visualRoot.Find("Z1");
                AddRenderers(z, gunRenderers);
                AddRenderers(z1, gunRenderers);
                if (gunRenderers.Count == 0)
                {
                    Renderer fallback = visualRoot.GetComponent<Renderer>();
                    if (fallback != null) gunRenderers.Add(fallback);
                }

                Bounds donorBounds;
                bool haveDonorBounds = TryGetCombinedBounds(gunRenderers, out donorBounds);
                // Preserve the source texture's binary alpha while still receiving the
                // scene/weapon lighting. The old opaque Diffuse path treated transparent
                // texels as black, which filled in the blunderbuss fin/curl geometry.
                Shader shader = Shader.Find("Transparent/Cutout/Diffuse");
                if (shader == null) shader = Shader.Find("Transparent/Cutout/VertexLit");
                if (shader == null) shader = Shader.Find("Unlit/Transparent Cutout");
                if (shader == null) shader = Shader.Find("Transparent/Diffuse");
                if (shader == null) shader = Shader.Find("Diffuse");
                for (int i = 0; i < gunRenderers.Count; i++)
                {
                    Renderer r = gunRenderers[i];
                    if (r == null) continue;
                    if (!_hidden.ContainsKey(r)) _hidden[r] = r.enabled;
                    r.enabled = false;
                }

                Material mat = new Material(shader);
                mat.name = "BussinMaterial";
                if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.5f);
                if (!string.IsNullOrEmpty(data.texturePngBase64))
                {
                    byte[] png = Convert.FromBase64String(data.texturePngBase64);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    tex.name = "BussinTexture";
                    tex.LoadImage(png);
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.anisoLevel = 0;
                    mat.mainTexture = tex;
                    mat.color = Color.white;
                }

                _customModel = new GameObject("CNR_Bussin_Model");
                _customModel.layer = visualRoot.gameObject.layer;
                _customModel.transform.parent = visualRoot;
                _customModel.transform.localPosition = Vector3.zero;
                // Runtime feedback showed the 180-degree correction still left the
                // barrel pointing left. Rotate another 90 degrees clockwise in the
                // donor's local Y plane.
                _customModel.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
                _customModel.transform.localScale = Vector3.one;
                MeshFilter mf = _customModel.AddComponent<MeshFilter>();
                mf.mesh = mesh;
                MeshRenderer mr = _customModel.AddComponent<MeshRenderer>();
                mr.material = mat;

                if (haveDonorBounds && mr.bounds.size.sqrMagnitude > 0.000001f)
                {
                    float donorSize = donorBounds.size.magnitude;
                    float customSize = mr.bounds.size.magnitude;
                    float scale = customSize > 0.0001f ? donorSize / customSize : 1f;
                    scale = Mathf.Clamp(scale, 0.05f, 20f);
                    _customModel.transform.localScale = Vector3.one * scale;
                    Bounds fitted = mr.bounds;
                    _customModel.transform.position += donorBounds.center - fitted.center;
                }

                SuppressDonorGunVisuals();
                ModEntry.Log("CNR DLC weapons: attached animated Bussin' model (" + vertices.Length + " vertices)");
            }
            catch (Exception ex)
            {
                ModEntry.Log("CNR DLC weapons: model load failed: " + ex.Message);
            }
        }

        private static void AddRenderers(Transform root, List<Renderer> list)
        {
            if (root == null || list == null) return;
            Renderer own = root.GetComponent<Renderer>();
            if (own != null) list.Add(own);
            Renderer[] children = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < children.Length; i++)
                if (children[i] != null && !list.Contains(children[i])) list.Add(children[i]);
        }

        private static bool TryGetCombinedBounds(List<Renderer> renderers, out Bounds bounds)
        {
            bounds = new Bounds();
            bool found = false;
            for (int i = 0; i < renderers.Count; i++)
            {
                Renderer r = renderers[i];
                if (r == null) continue;
                if (!found) { bounds = r.bounds; found = true; }
                else bounds.Encapsulate(r.bounds);
            }
            return found;
        }

        private void SuppressDonorGunVisuals()
        {
            foreach (KeyValuePair<Renderer, bool> kv in _hidden)
                if (kv.Key != null && kv.Key.enabled) kv.Key.enabled = false;
        }

        private void UnbindBussinWeapon()
        {
            if (_rifleOverrideActive)
            {
                CNRDLCWeaponSystem.EndRifleOverride();
                _rifleOverrideActive = false;
            }

            if (_weaponManager != null && _bussinWeapon != null && _placeholderWeapon != null &&
                _bussinSlotIndex >= 0 && _bussinSlotIndex < _weaponManager.allWeapons.Count &&
                _weaponManager.allWeapons[_bussinSlotIndex] == _bussinWeapon)
            {
                _weaponManager.allWeapons[_bussinSlotIndex] = _placeholderWeapon;
            }

            if (_customModel != null) Destroy(_customModel);
            _customModel = null;
            foreach (KeyValuePair<Renderer, bool> kv in _hidden)
                if (kv.Key != null) kv.Key.enabled = kv.Value;
            _hidden.Clear();

            if (_bussinWeapon != null) Destroy(_bussinWeapon.gameObject);
            _bussinWeapon = null;
            _placeholderWeapon = null;
            _weaponManager = null;
            _bussinSlotIndex = -1;
        }

        private void UpdateKnifeMovement()
        {
            CharacterController cc = FindLocalCharacterController();
            if (cc == null) return;

            VCAnalogJoystickBase stick = VCAnalogJoystickBase.GetInstance("stick");
            bool knife = IsActiveLocalKnife();
            bool grounded = cc.isGrounded;
            bool backing = false;
            Vector3 backwardInputWorld = Vector3.zero;

            if (stick != null)
            {
                Vector3 localInput = new Vector3(stick.AxisX, 0f, stick.AxisY);
                backing = stick.AxisY < -0.25f && localInput.sqrMagnitude > 0.05f;
                if (backing)
                {
                    backwardInputWorld = cc.transform.TransformDirection(localInput);
                    backwardInputWorld.y = 0f;
                    if (backwardInputWorld.sqrMagnitude > 0.001f) backwardInputWorld.Normalize();
                }
            }

            // SettingsMod's held-jump logic launches in LateUpdate, so CNR sees the
            // CharacterController transition from grounded -> airborne on the following
            // Update. Each genuine backward knife hop adds a small uncapped-by-vanilla
            // bonus, reproducing the intended Portal-style backward bhop quirk.
            if (_wasGrounded && !grounded && knife && backing && backwardInputWorld.sqrMagnitude > 0.001f)
            {
                float nextBonus = Mathf.Min(KnifeBackhopMaxBonus,
                    _knifeBackhopBonusVelocity.magnitude + KnifeBackhopGain);
                _knifeBackhopBonusVelocity = backwardInputWorld * nextBonus;
                ModEntry.Log("CNR knife backhop bonus=" + nextBonus);
            }

            if (!knife || !backing)
                _knifeBackhopBonusVelocity = Vector3.MoveTowards(
                    _knifeBackhopBonusVelocity, Vector3.zero, KnifeBackhopReleaseDrag * Time.deltaTime);
            else if (_knifeBackhopBonusVelocity.sqrMagnitude > 0.001f && backwardInputWorld.sqrMagnitude > 0.001f)
                _knifeBackhopBonusVelocity = backwardInputWorld * _knifeBackhopBonusVelocity.magnitude;

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

        private static WeaponManager FindLocalWeaponManager()
        {
            WeaponManager[] managers = (WeaponManager[])Resources.FindObjectsOfTypeAll(typeof(WeaponManager));
            for (int i = 0; i < managers.Length; i++)
            {
                WeaponManager wm = managers[i];
                if (wm == null || !wm.bPlayer) continue;
                if (wm.transform.root != null && wm.transform.root.tag == "Player") return wm;
            }
            return null;
        }

        private static Camera FindLocalCamera(FPScontroller fps)
        {
            if (fps != null)
            {
                Transform main = fps.transform.Find("LookObject/Main Camera");
                if (main != null)
                {
                    Camera c = main.GetComponent<Camera>();
                    if (c != null) return c;
                }
                Camera[] cameras = fps.GetComponentsInChildren<Camera>(true);
                for (int i = 0; i < cameras.Length; i++)
                    if (cameras[i] != null && cameras[i].gameObject.activeInHierarchy) return cameras[i];
            }
            return Camera.main;
        }

        private FPScontroller FindBoundLocalController()
        {
            try
            {
                if (_weaponManager != null && _weaponManager.transform != null)
                {
                    Transform root = _weaponManager.transform.root;
                    if (root != null)
                    {
                        FPScontroller bound = root.GetComponent<FPScontroller>();
                        if (bound == null) bound = root.GetComponentInChildren<FPScontroller>();
                        if (bound != null) return bound;
                    }
                }
            }
            catch { }
            return FindLocalController();
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
