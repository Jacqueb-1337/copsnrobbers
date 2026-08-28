using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

        public const float BussinHeadshotMultiplier = 1.5f;

        public static int GetBussinClipSize(int level)
        {
            level = Mathf.Clamp(level, 1, 3);
            return level >= 3 ? 2 : 1;
        }

        public static int GetBussinReserveAmmo(int level)
        {
            level = Mathf.Clamp(level, 1, 3);
            return level >= 3 ? 30 : 18;
        }

        public static void EnforceBussinRuntimeAmmo(WeaponScript ws)
        {
            if (ws == null || ws.ShotGun == null) return;

            int level = Mathf.Clamp(GetBussinLevel(), 1, 3);
            int clip = GetBussinClipSize(level);
            int reserve = GetBussinReserveAmmo(level);

            ws.ShotGun.bulletsPerClip = clip;
            if (ws.ShotGun.bulletsLeft > clip) ws.ShotGun.bulletsLeft = clip;
            if (ws.ShotGun.clips > reserve) ws.ShotGun.clips = reserve;
            ws.ShotGun.fractions = 5;
        }

        public static float RollBussinPelletDamage(int level)
        {
            level = Mathf.Clamp(level, 1, 3);
            if (level == 1) return UnityEngine.Random.Range(20f, 24f);
            if (level == 2) return UnityEngine.Random.Range(24f, 28f);
            return UnityEngine.Random.Range(28f, 32f);
        }

        public static float GetBussinDamageScale(int level, float distance)
        {
            level = Mathf.Clamp(level, 1, 3);
            float fullRange = level >= 3 ? 3.0f : 2.5f;
            float kneeRange = level >= 3 ? 9.0f : 7.0f;
            float kneeScale = level == 1 ? 0.61f : level == 2 ? 0.56f : 0.50f;

            if (distance <= fullRange) return 1f;
            if (distance <= kneeRange)
            {
                float t = Mathf.Clamp01((distance - fullRange) / (kneeRange - fullRange));
                float smooth = t * t * (3f - 2f * t);
                return Mathf.Lerp(1f, kneeScale, smooth);
            }

            // Bussin' is deliberately a brutal close-range weapon. Once a pellet
            // travels past the level's falloff knee its damage roughly halves every
            // two blocks, unlike the donor M87T which has no distance falloff at all.
            float beyond = distance - kneeRange;
            return Mathf.Max(0.05f, kneeScale * Mathf.Pow(0.42f, beyond / 2f));
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
            // The vanilla card only exposes Power / Clip / Fire Rate bars. Use the
            // fire-rate bar as sustained cadence here so the very slow one-shot reload
            // is visible even though the donor's between-shot delay is unchanged.
            info.mPowerSpriteName = "Rating_" + (displayLevel == 1 ? 8 : displayLevel == 2 ? 9 : 10);
            info.mClipSpriteName = "Rating_" + (displayLevel == 3 ? 2 : 1);
            info.mFireRateSpriteName = "Rating_" + (displayLevel == 3 ? 2 : 1);
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
        private bool _setupRunning;
        private float _nextEnsure;

        private void Start()
        {
            RequestSetup();
        }

        private void Update()
        {
            if (Time.time < _nextEnsure) return;
            _nextEnsure = Time.time + 0.50f;
            if (_card == null || _store == null)
                RequestSetup();
            else
                RefreshCard();
        }

        private void RequestSetup()
        {
            if (_setupRunning) return;
            _setupRunning = true;
            StartCoroutine(Setup());
        }

        private IEnumerator Setup()
        {
            yield return new WaitForSeconds(0.7f);
            _store = UIStoreDirector.mInstance;
            if (_store == null || _store.weaponPrefab == null || _store.weaponObjParent == null)
            {
                _setupRunning = false;
                yield break;
            }
            for (int i = 0; i < 20 && (CNRDLCWeaponSystem.FindBussinManifestEntry() == null || !CNRDLCWeaponSystem.HasDownloadedModel()); i++)
                yield return new WaitForSeconds(0.25f);
            if (CNRDLCWeaponSystem.FindBussinManifestEntry() == null || !CNRDLCWeaponSystem.HasDownloadedModel())
            {
                ModEntry.Log("CNR DLC weapons: Bussin' not injected into Store yet (manifest/model missing); will retry");
                _setupRunning = false;
                yield break;
            }

            GameObject go = Instantiate(_store.weaponPrefab, _store.weaponPrefab.transform.position,
                _store.weaponPrefab.transform.rotation) as GameObject;
            if (go == null)
            {
                _setupRunning = false;
                yield break;
            }
            go.name = "CNR DLC Weapon - Bussin";
            go.transform.parent = _store.weaponObjParent;
            go.transform.localScale = _store.weaponPrefab.transform.localScale;
            go.transform.position = _store.weaponPrefab.transform.position;
            go.transform.rotation = _store.weaponPrefab.transform.rotation;

            _card = go.GetComponent<UIStoreWeaponPrefab>();
            if (_card == null)
            {
                Destroy(go);
                _setupRunning = false;
                yield break;
            }
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
            _setupRunning = false;
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
        private float _nextEnsure;
        private bool _setupRunning;
        private string _lastEquipSignature = "";

        private void Start()
        {
            RequestSetup();
        }

        private void RequestSetup()
        {
            if (_setupRunning) return;
            _setupRunning = true;
            StartCoroutine(Setup());
        }

        private void Update()
        {
            if (Time.time >= _nextEnsure)
            {
                _nextEnsure = Time.time + 0.50f;
                if (CNRDLCWeaponSystem.IsBussinOwned() && (_profile == null || _card == null))
                    RequestSetup();
            }
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
            if (_profile == null || _profile.weaponPrefab == null || _profile.weaponObjParent == null)
            {
                _setupRunning = false;
                yield break;
            }
            if (!CNRDLCWeaponSystem.IsBussinOwned())
            {
                _setupRunning = false;
                yield break;
            }
            for (int i = 0; i < 20 && (CNRDLCWeaponSystem.FindBussinManifestEntry() == null || !CNRDLCWeaponSystem.HasDownloadedModel()); i++)
                yield return new WaitForSeconds(0.25f);
            if (CNRDLCWeaponSystem.FindBussinManifestEntry() == null || !CNRDLCWeaponSystem.HasDownloadedModel())
            {
                _setupRunning = false;
                yield break;
            }

            GameObject go = Instantiate(_profile.weaponPrefab, _profile.weaponPrefab.transform.position,
                _profile.weaponPrefab.transform.rotation) as GameObject;
            if (go == null)
            {
                _setupRunning = false;
                yield break;
            }
            go.name = "CNR DLC Weapon - Bussin";
            go.transform.parent = _profile.weaponObjParent;
            go.transform.localScale = _profile.weaponPrefab.transform.localScale;
            go.transform.position = _profile.weaponPrefab.transform.position;
            go.transform.rotation = _profile.weaponPrefab.transform.rotation;

            UIProfileWeaponPrefab vanilla = go.GetComponent<UIProfileWeaponPrefab>();
            if (vanilla == null)
            {
                Destroy(go);
                _setupRunning = false;
                yield break;
            }
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
            _setupRunning = false;
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
        private const string HudAtlasSpriteName = "CNR_Bussin_Hud";
        private static string _hudFallbackSpriteName = "";

        // This NGUI build batches by material, so a separate DLC material can render
        // behind the opaque Store card even at a higher widget depth. Append the DLC
        // pixels to the existing vanilla atlas instead. Existing sprite rectangles are
        // stored in pixels and remain unchanged, so every vanilla sprite keeps its art.
        private static bool EnsureSpriteInVanillaAtlas(UISprite donor, Texture2D art, string spriteName)
        {
            if (donor == null || donor.atlas == null || donor.atlas.spriteMaterial == null || art == null)
                return false;

            UIAtlas atlas = donor.atlas;
            if (atlas.GetSprite(spriteName) != null) return true;

            Texture oldTexture = atlas.spriteMaterial.mainTexture;
            if (oldTexture == null) return false;

            RenderTexture previous = RenderTexture.active;
            RenderTexture rt = null;
            Texture2D oldCopy = null;
            try
            {
                // Normalize the atlas metadata to pixel rectangles before resizing.
                if (atlas.coordinates != UIAtlas.Coordinates.Pixels)
                    atlas.coordinates = UIAtlas.Coordinates.Pixels;

                int oldWidth = oldTexture.width;
                int oldHeight = oldTexture.height;
                int newWidth = Mathf.Max(oldWidth, art.width);
                int newHeight = oldHeight + art.height;

                rt = RenderTexture.GetTemporary(oldWidth, oldHeight, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(oldTexture, rt);
                RenderTexture.active = rt;
                oldCopy = new Texture2D(oldWidth, oldHeight, TextureFormat.ARGB32, false);
                oldCopy.ReadPixels(new Rect(0f, 0f, oldWidth, oldHeight), 0, 0);
                oldCopy.Apply(false);

                Texture2D expanded = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
                expanded.name = oldTexture.name + "_CNRExpanded";
                expanded.filterMode = oldTexture.filterMode;
                expanded.wrapMode = oldTexture.wrapMode;
                expanded.anisoLevel = oldTexture.anisoLevel;

                Color32[] clear = new Color32[newWidth * newHeight];
                expanded.SetPixels32(clear);
                expanded.SetPixels(0, art.height, oldWidth, oldHeight, oldCopy.GetPixels());
                expanded.SetPixels(0, 0, art.width, art.height, art.GetPixels());
                expanded.Apply(false, false);

                atlas.spriteMaterial.mainTexture = expanded;
                UIAtlas.Sprite entry = new UIAtlas.Sprite();
                entry.name = spriteName;
                entry.outer = new Rect(0f, oldHeight, art.width, art.height);
                entry.inner = entry.outer;
                atlas.spriteList.Add(entry);
                atlas.MarkAsDirty();
                ModEntry.Log("CNR DLC art: appended " + spriteName + " to vanilla atlas " + atlas.name
                    + " (" + oldWidth + "x" + oldHeight + " -> " + newWidth + "x" + newHeight + ")");
                return true;
            }
            catch (Exception ex)
            {
                ModEntry.Log("CNR DLC art: vanilla atlas append failed: " + ex.Message);
                return false;
            }
            finally
            {
                RenderTexture.active = previous;
                if (rt != null) RenderTexture.ReleaseTemporary(rt);
                if (oldCopy != null) UnityEngine.Object.Destroy(oldCopy);
            }
        }

        private static bool ApplyVanillaAtlasSprite(GameObject logoObject, Texture2D art, string spriteName)
        {
            if (logoObject == null || art == null) return false;
            UISprite sprite = logoObject.GetComponent<UISprite>();
            if (sprite == null || !EnsureSpriteInVanillaAtlas(sprite, art, spriteName)) return false;

            sprite.enabled = true;
            sprite.spriteName = spriteName;
            sprite.type = UISprite.Type.Simple;
            sprite.color = Color.white;
            sprite.UpdateUVs(true);
            sprite.MarkAsChanged();
            return true;
        }

        public static void PrepareHud(GameObject logoObject)
        {
            if (logoObject == null) return;
            UISprite sprite = logoObject.GetComponent<UISprite>();
            Texture2D art = GetHudOrCreate();
            if (sprite == null || art == null) return;
            if (string.IsNullOrEmpty(_hudFallbackSpriteName)) _hudFallbackSpriteName = sprite.spriteName ?? "";
            EnsureSpriteInVanillaAtlas(sprite, art, HudAtlasSpriteName);
        }

        // Store and Profile already have a registered UISprite at this transform.
        // Reuse that widget and only swap its atlas so UIPanel never loses its node.
        public static void ApplyCard(GameObject logoObject)
        {
            if (logoObject == null) return;
            Texture2D tex = GetOrCreate();
            if (tex == null) return;

            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform oldOverlay = holder.Find("CNRDLCWeaponThumbnail");
            if (oldOverlay != null)
            {
                UITexture oldWidget = oldOverlay.GetComponent<UITexture>();
                if (oldWidget != null) oldWidget.enabled = false;
            }

            ApplyVanillaAtlasSprite(logoObject, tex, CardAtlasSpriteName);
        }

        public static void Apply(GameObject logoObject)
        {
            if (logoObject == null) return;
            Texture2D tex = GetOrCreate();
            if (tex == null) return;

            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform child = holder.Find("CNRDLCWeaponThumbnail");
            if (child != null)
            {
                UITexture oldWidget = child.GetComponent<UITexture>();
                if (oldWidget != null) oldWidget.enabled = false;
            }

            ApplyVanillaAtlasSprite(logoObject, tex, CardAtlasSpriteName);
        }

        public static void ApplyHud(GameObject logoObject)
        {
            if (logoObject == null) return;
            Texture2D tex = GetHudOrCreate();
            if (tex == null) return;

            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform child = holder.Find("CNRDLCWeaponHudIcon");
            if (child != null)
            {
                UITexture oldWidget = child.GetComponent<UITexture>();
                if (oldWidget != null) oldWidget.enabled = false;
            }

            PrepareHud(logoObject);
            ApplyVanillaAtlasSprite(logoObject, tex, HudAtlasSpriteName);
        }

        public static void ClearHud(GameObject logoObject)
        {
            if (logoObject == null) return;

            UITexture inPlace = logoObject.GetComponent<UITexture>();
            if (inPlace != null) inPlace.enabled = false;

            Transform holder = logoObject.transform.parent != null ? logoObject.transform.parent : logoObject.transform;
            Transform child = holder.Find("CNRDLCWeaponHudIcon");
            if (child != null)
            {
                UITexture oldWidget = child.GetComponent<UITexture>();
                if (oldWidget != null) oldWidget.enabled = false;
            }

            UISprite sprite = logoObject.GetComponent<UISprite>();
            if (sprite != null)
            {
                sprite.enabled = true;
                if (sprite.spriteName == HudAtlasSpriteName && !string.IsNullOrEmpty(_hudFallbackSpriteName))
                {
                    sprite.spriteName = _hudFallbackSpriteName;
                    sprite.UpdateUVs(true);
                    sprite.MarkAsChanged();
                }
            }
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

    // Bussin' keeps the donor projectile's movement, hit effects and multiplayer
    // SendMessage routing, but replaces the M87T's flat damage and hard-coded 1000
    // headshot with weapon-specific distance falloff.
    public class CNRBussinBullet : Bullet
    {
        public int BussinLevel = 1;

        private Vector3 _velocity;
        private Vector3 _newPos;
        private Vector3 _oldPos;
        private Vector3 _spawnPos;
        private bool _hasHit;
        private bool _armed;
        private float _baseDamage;

        public override void Start()
        {
            // The persistent template itself has no shooter. WeaponScript assigns
            // "player" to each instantiated pellet before Unity calls Start(), so
            // only real fired pellets run this behavior.
            _armed = shooter == "player";
            if (!_armed) return;

            BussinLevel = Mathf.Clamp(BussinLevel, 1, 3);
            _baseDamage = CNRDLCWeaponSystem.RollBussinPelletDamage(BussinLevel);
            bulletDamage = _baseDamage;
            _spawnPos = transform.position;
            _newPos = transform.position;
            _oldPos = _newPos;
            _velocity = speed * transform.forward;
            UnityEngine.Object.Destroy(gameObject, life);
        }

        public override void Update()
        {
            if (!_armed || _hasHit) return;

            _newPos += _velocity * Time.deltaTime * 10f;
            Vector3 direction = _newPos - _oldPos;
            float magnitude = direction.magnitude;
            if (magnitude > 0f)
            {
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(_oldPos, direction, out hitInfo, magnitude, 19))
                {
                    _newPos = hitInfo.point;
                    _hasHit = true;
                    Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                    if (hitInfo.rigidbody != null)
                        hitInfo.rigidbody.AddForce(transform.forward * impactForce, ForceMode.Impulse);

                    if (PlayerPrefs.GetInt("GameQualityLevel", 3) == 3 && impactHoles && impactObjects != null)
                    {
                        if (hitInfo.transform.tag == "City" && impactObjects.Count > 0)
                            UnityEngine.Object.Instantiate(impactObjects[0], hitInfo.point, hitRotation);
                        else if (IsEnemyHitTag(hitInfo.transform.tag) && impactObjects.Count > 1)
                            UnityEngine.Object.Instantiate(impactObjects[1], hitInfo.point, hitRotation);
                    }
                    if (knifeHoles && impactObjects != null)
                    {
                        if (hitInfo.transform.tag == "City" && impactObjects.Count > 0)
                            UnityEngine.Object.Instantiate(impactObjects[0], hitInfo.point,
                                hitRotation * Quaternion.Euler(0f, 90f, 0f));
                        else if (IsEnemyHitTag(hitInfo.transform.tag) && impactObjects.Count > 1)
                            UnityEngine.Object.Instantiate(impactObjects[1], hitInfo.point, hitRotation);
                    }

                    float distance = Vector3.Distance(_spawnPos, hitInfo.point);
                    float scaledDamage = _baseDamage * CNRDLCWeaponSystem.GetBussinDamageScale(BussinLevel, distance);
                    int bodyDamage = Mathf.Max(1, Mathf.RoundToInt(scaledDamage));
                    int headDamage = Mathf.Max(1, Mathf.RoundToInt(scaledDamage * CNRDLCWeaponSystem.BussinHeadshotMultiplier));
                    string tag = hitInfo.transform.tag;

                    if (tag == "EnemyTag")
                    {
                        if (shooter == "player")
                            hitInfo.transform.SendMessageUpwards("decreaseBlood", bodyDamage, SendMessageOptions.DontRequireReceiver);
                        hitInfo.transform.SendMessageUpwards("setTargetIsPlayer", true, SendMessageOptions.DontRequireReceiver);
                        if (onlinePlayerTag == string.Empty)
                            hitInfo.transform.SendMessage("OnDamaged", bodyDamage, SendMessageOptions.DontRequireReceiver);
                    }
                    else if (tag == "EnemyHeadTag")
                    {
                        if (onlinePlayerTag == string.Empty)
                            hitInfo.transform.SendMessageUpwards("OnDamaged", headDamage, SendMessageOptions.DontRequireReceiver);
                    }
                    else if (tag == "EnemyBodyTag")
                    {
                        if (onlinePlayerTag == string.Empty)
                            hitInfo.transform.SendMessageUpwards("OnDamaged", bodyDamage, SendMessageOptions.DontRequireReceiver);
                    }
                    else if (tag == "EnemyFootTag")
                    {
                        if (onlinePlayerTag == string.Empty)
                            hitInfo.transform.SendMessageUpwards("OnDamaged", Mathf.Max(1, Mathf.RoundToInt(scaledDamage * 0.7f)),
                                SendMessageOptions.DontRequireReceiver);
                    }
                    else if (tag == "Player")
                    {
                        hitInfo.transform.SendMessage("PlayerDamage", bodyDamage, SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        hitInfo.transform.SendMessageUpwards("setTargetIsPlayer", false, SendMessageOptions.DontRequireReceiver);
                    }

                    UnityEngine.Object.Destroy(gameObject, 1f);
                }
            }

            _oldPos = transform.position;
            transform.position = _newPos;
        }

        private static bool IsEnemyHitTag(string tag)
        {
            return tag == "EnemyTag" || tag == "EnemyHeadTag" || tag == "EnemyBodyTag" || tag == "EnemyFootTag";
        }
    }

    public static class CNRBussinNetworkState
    {
        public static int LocalVisualLevel = 0;
        private static readonly Dictionary<int, int> _remoteVisualLevels = new Dictionary<int, int>();

        public static void Receive(int actorId, int level)
        {
            if (actorId <= 0) return;
            level = Mathf.Clamp(level, 0, 3);
            if (level <= 0) _remoteVisualLevels.Remove(actorId);
            else _remoteVisualLevels[actorId] = level;
        }

        public static bool TryGetRemoteLevel(int actorId, out int level)
        {
            return _remoteVisualLevels.TryGetValue(actorId, out level) && level > 0;
        }

        public static void RemoveActor(int actorId)
        {
            _remoteVisualLevels.Remove(actorId);
        }

        public static void ClearRemote()
        {
            _remoteVisualLevels.Clear();
        }

        public static void AppendLocalState(System.Collections.Hashtable ht)
        {
            if (ht == null) return;
            ht["bussin_v"] = LocalVisualLevel.ToString(CultureInfo.InvariantCulture);
        }

        public static void BroadcastLocalState(int level)
        {
            try
            {
                if (PhotonNetwork.room == null) return;
                System.Collections.Hashtable ht = new System.Collections.Hashtable();
                ht["bussin_v"] = Mathf.Clamp(level, 0, 3).ToString(CultureInfo.InvariantCulture);
                ModEntry.RaiseCnrEvent(ht);
            }
            catch { }
        }
    }

    // Vanilla still advertises M87T through its original weapon/animation sync so old
    // clients remain compatible. New CNRMod clients receive Bussin's selected level in
    // the existing fast visual packet and replace only the remote M87T geometry.
    public class CNRBussinRemoteWeaponRenderer : MonoBehaviour
    {
        private class RemoteVisual
        {
            public GameObject ActorObject;
            public GameObject CustomModel;
            public readonly Dictionary<Renderer, bool> Hidden = new Dictionary<Renderer, bool>();
        }

        private readonly Dictionary<int, RemoteVisual> _visuals = new Dictionary<int, RemoteVisual>();
        private float _nextPoll;
        private static Type _mgrType;
        private static FieldInfo _mgrInstanceFld;
        private static FieldInfo _mgrInfoListFld;
        private static FieldInfo _mgrObjListFld;
        private static FieldInfo _playerInfoMId;
        private static Mesh _sharedMesh;
        private static Material _sharedMaterial;

        private void Update()
        {
            if (Time.time < _nextPoll) return;
            _nextPoll = Time.time + 0.10f;
            Poll();
        }

        private void Poll()
        {
            object[] others = ModEntry.GetPhotonOtherPlayers();
            HashSet<int> activeIds = new HashSet<int>();
            for (int i = 0; i < others.Length; i++)
            {
                object player = others[i];
                if (player == null) continue;
                int actorId = ModEntry.GetPhotonPlayerId(player);
                if (actorId <= 0) continue;
                activeIds.Add(actorId);

                int level;
                if (CNRBussinNetworkState.TryGetRemoteLevel(actorId, out level))
                    EnsureVisual(actorId);
                else
                    RemoveVisual(actorId);
            }

            List<int> gone = new List<int>();
            foreach (KeyValuePair<int, RemoteVisual> kv in _visuals)
                if (!activeIds.Contains(kv.Key)) gone.Add(kv.Key);
            for (int i = 0; i < gone.Count; i++)
            {
                int actorId = gone[i];
                RemoveVisual(actorId);
                CNRBussinNetworkState.RemoveActor(actorId);
            }
        }

        private static bool EnsureManagerReflection()
        {
            if (_mgrType != null) return _mgrInstanceFld != null && _mgrInfoListFld != null && _mgrObjListFld != null;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("CNRMultiplayerManager");
                if (t == null) continue;
                _mgrType = t;
                _mgrInstanceFld = t.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                _mgrInfoListFld = t.GetField("otherPlayersInfoList", BindingFlags.Public | BindingFlags.Instance);
                _mgrObjListFld = t.GetField("otherPlayerObject", BindingFlags.NonPublic | BindingFlags.Instance);
                break;
            }
            return _mgrType != null && _mgrInstanceFld != null && _mgrInfoListFld != null && _mgrObjListFld != null;
        }

        private static GameObject FindActorObject(int actorId)
        {
            try
            {
                if (!EnsureManagerReflection()) return null;
                object mgr = _mgrInstanceFld.GetValue(null);
                if (mgr == null) return null;
                Array infos = _mgrInfoListFld.GetValue(mgr) as Array;
                Array objects = _mgrObjListFld.GetValue(mgr) as Array;
                if (infos == null || objects == null) return null;
                string wanted = actorId.ToString();
                int count = Mathf.Min(infos.Length, objects.Length);
                for (int i = 0; i < count; i++)
                {
                    object info = infos.GetValue(i);
                    if (info == null) continue;
                    if (_playerInfoMId == null)
                        _playerInfoMId = info.GetType().GetField("mId", BindingFlags.Public | BindingFlags.Instance);
                    if (_playerInfoMId == null) return null;
                    string id = _playerInfoMId.GetValue(info) as string;
                    if (id == wanted) return objects.GetValue(i) as GameObject;
                }
            }
            catch { }
            return null;
        }

        private void EnsureVisual(int actorId)
        {
            GameObject actor = FindActorObject(actorId);
            if (actor == null) return;

            RemoteVisual existing;
            if (_visuals.TryGetValue(actorId, out existing))
            {
                if (existing.ActorObject == actor && existing.CustomModel != null)
                {
                    // Vanilla AnimationSync can re-enable the donor M87T renderer after
                    // weapon/status updates. Reassert suppression for as long as this
                    // actor is advertised as holding Bussin'.
                    foreach (KeyValuePair<Renderer, bool> kv in existing.Hidden)
                        if (kv.Key != null) kv.Key.enabled = false;
                    if (!existing.CustomModel.activeSelf) existing.CustomModel.SetActive(true);
                    return;
                }
                RemoveVisual(actorId);
            }

            if (!EnsureSharedVisualAssets()) return;
            AnimationSync sync = actor.GetComponentInChildren<AnimationSync>();
            if (sync == null || sync.thirdPersonWeapons == null) return;

            Transform donor = sync.thirdPersonWeapons.Find(CNRDLCWeaponSystem.BussinDonor);
            if (donor == null)
            {
                for (int i = 0; i < sync.thirdPersonWeapons.childCount; i++)
                {
                    Transform child = sync.thirdPersonWeapons.GetChild(i);
                    if (child != null && child.name == CNRDLCWeaponSystem.BussinDonor) { donor = child; break; }
                }
            }
            if (donor == null) return;

            RemoteVisual visual = new RemoteVisual();
            visual.ActorObject = actor;
            Renderer[] renderers = donor.GetComponentsInChildren<Renderer>(true);
            Bounds donorBounds = new Bounds();
            bool haveBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null) continue;
                visual.Hidden[r] = r.enabled;
                if (!haveBounds) { donorBounds = r.bounds; haveBounds = true; }
                else donorBounds.Encapsulate(r.bounds);
                r.enabled = false;
            }

            GameObject model = new GameObject("CNR_Bussin_Remote_Model");
            model.layer = donor.gameObject.layer;
            model.transform.parent = donor;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
            model.transform.localScale = Vector3.one;
            MeshFilter mf = model.AddComponent<MeshFilter>();
            mf.sharedMesh = _sharedMesh;
            MeshRenderer mr = model.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _sharedMaterial;

            if (haveBounds && mr.bounds.size.sqrMagnitude > 0.000001f)
            {
                float customSize = mr.bounds.size.magnitude;
                float scale = customSize > 0.0001f ? donorBounds.size.magnitude / customSize : 1f;
                model.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.05f, 20f);
                Bounds fitted = mr.bounds;
                model.transform.position += donorBounds.center - fitted.center;
            }

            visual.CustomModel = model;
            _visuals[actorId] = visual;
            ModEntry.Log("CNR DLC weapons: remote actor " + actorId + " is holding Bussin'");
        }

        private static bool EnsureSharedVisualAssets()
        {
            if (_sharedMesh != null && _sharedMaterial != null) return true;
            if (!CNRDLCWeaponSystem.HasDownloadedModel()) return false;
            try
            {
                string json = File.ReadAllText(ContentManager.GunCacheDir + CNRDLCWeaponSystem.BussinId + ".json");
                CNRDLCWeaponModelBundle data = JsonReader.Deserialize<CNRDLCWeaponModelBundle>(json);
                if (data == null || data.vertices == null || data.triangles == null || data.vertices.Length < 9) return false;

                Vector3[] vertices = new Vector3[data.vertices.Length / 3];
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = new Vector3(data.vertices[i * 3], data.vertices[i * 3 + 1], data.vertices[i * 3 + 2]);
                Vector2[] uv = new Vector2[vertices.Length];
                if (data.uv != null)
                {
                    int uvCount = Mathf.Min(vertices.Length, data.uv.Length / 2);
                    for (int i = 0; i < uvCount; i++) uv[i] = new Vector2(data.uv[i * 2], 1f - data.uv[i * 2 + 1]);
                }

                _sharedMesh = new Mesh();
                _sharedMesh.name = "BussinRemoteMesh";
                _sharedMesh.vertices = vertices;
                _sharedMesh.uv = uv;
                _sharedMesh.triangles = data.triangles;
                _sharedMesh.RecalculateNormals();
                _sharedMesh.RecalculateBounds();

                Shader shader = Shader.Find("Transparent/Cutout/Diffuse");
                if (shader == null) shader = Shader.Find("Transparent/Cutout/VertexLit");
                if (shader == null) shader = Shader.Find("Unlit/Transparent Cutout");
                if (shader == null) shader = Shader.Find("Transparent/Diffuse");
                if (shader == null) shader = Shader.Find("Diffuse");
                _sharedMaterial = new Material(shader);
                _sharedMaterial.name = "BussinRemoteMaterial";
                if (_sharedMaterial.HasProperty("_Cutoff")) _sharedMaterial.SetFloat("_Cutoff", 0.5f);
                if (!string.IsNullOrEmpty(data.texturePngBase64))
                {
                    byte[] png = Convert.FromBase64String(data.texturePngBase64);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    tex.name = "BussinRemoteTexture";
                    tex.LoadImage(png);
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.anisoLevel = 0;
                    _sharedMaterial.mainTexture = tex;
                    _sharedMaterial.color = Color.white;
                }
                return true;
            }
            catch (Exception ex)
            {
                ModEntry.Log("CNR DLC weapons: remote Bussin visual load failed: " + ex.Message);
                return false;
            }
        }

        private void RemoveVisual(int actorId)
        {
            RemoteVisual visual;
            if (!_visuals.TryGetValue(actorId, out visual)) return;
            if (visual.CustomModel != null) Destroy(visual.CustomModel);
            foreach (KeyValuePair<Renderer, bool> kv in visual.Hidden)
                if (kv.Key != null) kv.Key.enabled = kv.Value;
            _visuals.Remove(actorId);
        }

        private void OnDestroy()
        {
            List<int> ids = new List<int>(_visuals.Keys);
            for (int i = 0; i < ids.Count; i++) RemoveVisual(ids[i]);
        }
    }

    public class CNRBussinActivationRelay : MonoBehaviour
    {
        private void OnEnable()
        {
            CNRDLCWeaponSystem.BeginRifleOverride();
            StartCoroutine(EnforceAmmoAfterVanillaStart());
        }

        private IEnumerator EnforceAmmoAfterVanillaStart()
        {
            // WeaponScript.Start() runs on the first real activation and re-applies the
            // donor M87T's 10/12/15-round ammo values. Wait one frame so vanilla finishes,
            // then clamp the runtime clone back to Bussin's 1/1/2-shot capacities.
            yield return null;
            CNRDLCWeaponSystem.EnforceBussinRuntimeAmmo(GetComponent<WeaponScript>());
        }

        private void OnDisable() { CNRDLCWeaponSystem.EndRifleOverride(); }
        private void OnDestroy() { CNRDLCWeaponSystem.EndRifleOverride(); }
    }

    public class CNRDLCWeaponRuntime : MonoBehaviour
    {
        private const float BussinRecoilVelocity = 12.0f;
        private const float BussinHorizontalVelocityScale = 1.43f;
        private const float BussinVerticalVelocityScale = 1.30f;
        private const float BussinHorizontalDrag = 30.0f;
        private const float BussinFallbackVerticalDrag = 24.0f;
        private const float BussinRisingVerticalScale = 0.22f;
        private const float BussinFallCompensation = 0.85f;
        private const float BussinFallCompensationCap = 24.0f;
        private const float BussinFallFullCompImpulse = 6.0f;
        private const float BussinReloadAudioGuard = 0.15f;

        private WeaponManager _weaponManager;
        private WeaponScript _bussinWeapon;
        private AudioSource _bussinFireAudio;
        private float _bussinLastShotAt = -10f;
        private bool _bussinReloadQueued;
        private float _bussinReloadAt;
        private WeaponScript _placeholderWeapon;
        private int _bussinSlotIndex = -1;
        private GameObject _customModel;
        private GameObject _bussinBulletTemplate;
        private readonly Dictionary<Renderer, bool> _hidden = new Dictionary<Renderer, bool>();
        private bool _rifleOverrideActive;
        private float _nextWeaponScan;
        private int _lastAdvertisedVisualLevel = -1;
        private Vector3 _bussinHorizontalVelocity = Vector3.zero;
        private float _bussinFallbackVerticalVelocity;
        private Component _settingsJumpHook;
        private FieldInfo _settingsOwnJumpVelY;
        private FieldInfo _settingsOwnJumpActive;
        private GameObject _hudIconObject;

        private void Start()
        {
            CNRDLCWeaponSystem.ClearMarkerIfDonorMissing();
            // Decode the HUD PNG before the player starts switching weapons so the
            // first Bussin frame never waits on disk/base64/Texture2D creation.
            CNRDLCWeaponThumbnail.GetHudOrCreate();
            if (GetComponent<CNRBussinRemoteWeaponRenderer>() == null)
                gameObject.AddComponent<CNRBussinRemoteWeaponRenderer>();
        }

        private void Update()
        {
            CNRDLCWeaponSystem.ClearMarkerIfDonorMissing();

            bool bussin = CNRDLCWeaponSystem.IsBussinEquipped();
            bool bussinSelected = bussin && IsBussinSelectedSlot();
            int visualLevel = bussinSelected
                ? Mathf.Clamp(CNRDLCWeaponSystem.GetBussinLevel(), 1, 3) : 0;
            CNRBussinNetworkState.LocalVisualLevel = visualLevel;
            if (visualLevel != _lastAdvertisedVisualLevel)
            {
                _lastAdvertisedVisualLevel = visualLevel;
                CNRBussinNetworkState.BroadcastLocalState(visualLevel);
            }
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
                UpdateBussinHudIcon(bussinSelected);
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

        }

        private bool IsBussinSelectedSlot()
        {
            int logicalIndex = CNRDLCWeaponSystem.GetBussinLogicalIndex();
            if (logicalIndex < 0) return false;

            WeaponManager manager = _weaponManager;
            if (manager == null) manager = FindLocalWeaponManager();
            return manager != null && manager.index == logicalIndex;
        }

        private void UpdateBussinHudIcon(bool active)
        {
            try
            {
                if (_hudIconObject == null || !_hudIconObject.activeInHierarchy)
                    _hudIconObject = GameObject.Find("SwitchButtonBackground");
                if (_hudIconObject == null) return;
                if (CNRDLCWeaponSystem.IsBussinEquipped()) CNRDLCWeaponThumbnail.PrepareHud(_hudIconObject);
                if (active) CNRDLCWeaponThumbnail.ApplyHud(_hudIconObject);
                else CNRDLCWeaponThumbnail.ClearHud(_hudIconObject);
            }
            catch { }
        }

        private void LateUpdate()
        {
            // Vanilla refreshes the donor icon while switching. Reassert Bussin in
            // LateUpdate from the selected logical slot so M87T never reaches render.
            UpdateBussinHudIcon(CNRDLCWeaponSystem.IsBussinEquipped() && IsBussinSelectedSlot());

            CharacterController cc = FindLocalCharacterController();
            if (cc == null) return;

            Vector3 extra = _bussinHorizontalVelocity;
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
            PrepareBussinBulletTemplate(_bussinWeapon);

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
            int level = Mathf.Clamp(CNRDLCWeaponSystem.GetBussinLevel(), 1, 3);
            int clip = CNRDLCWeaponSystem.GetBussinClipSize(level);
            int reserve = CNRDLCWeaponSystem.GetBussinReserveAmmo(level);
            ws.ShotGun.bulletsPerClip = clip;
            ws.ShotGun.bulletsLeft = clip;
            ws.ShotGun.clips = reserve;
            ws.ShotGun.fractions = 5;
            // Bussin' reload time is half of the previous 1.75x tuning, so it reloads
            // twice as fast as before: 0.875x the donor shotgun's reload duration.
            ws.ShotGun.reloadTime = Mathf.Max(0.1f, ws.ShotGun.reloadTime * 0.875f);
            // Bridge the runtime clone's real reload duration to CNRSettingsMod. The settings
            // DLL is intentionally independent from CNRMod, so PlayerPrefs avoids a hard DLL ref.
            PlayerPrefs.SetFloat("CNR_BussinReloadTime", ws.ShotGun.reloadTime);
            ModEntry.Log("CNR DLC weapons: Bussin' L" + level + " stats clip=" + clip +
                " reserve=" + reserve + " reload=" + ws.ShotGun.reloadTime + "s pellets=" + ws.ShotGun.fractions);
        }

        private void PrepareBussinBulletTemplate(WeaponScript ws)
        {
            if (ws == null || ws.ShotGun == null || ws.ShotGun.bullet == null) return;
            try
            {
                GameObject template = Instantiate(ws.ShotGun.bullet.gameObject) as GameObject;
                if (template == null) return;
                template.name = "CNR_Bussin_BulletTemplate";
                template.transform.position = new Vector3(0f, -10000f, 0f);

                Bullet source = template.GetComponent<Bullet>();
                if (source == null)
                {
                    Destroy(template);
                    return;
                }

                int speed = source.speed;
                float life = source.life;
                int damage = source.damage;
                int impactForce = source.impactForce;
                bool impactHoles = source.impactHoles;
                bool knifeHoles = source.knifeHoles;
                bool doDamage = source.doDamage;
                List<GameObject> impactObjects = source.impactObjects;
                Transform bloodParticleEffect = source.bloodParticleEffect;
                string onlinePlayerTag = source.onlinePlayerTag;
                float bulletDamage = source.bulletDamage;
                string shooter = source.shooter;

                DestroyImmediate(source);
                CNRBussinBullet custom = template.AddComponent<CNRBussinBullet>();
                custom.speed = speed;
                custom.life = life;
                custom.damage = damage;
                custom.impactForce = impactForce;
                custom.impactHoles = impactHoles;
                custom.knifeHoles = knifeHoles;
                custom.doDamage = doDamage;
                custom.impactObjects = impactObjects;
                custom.bloodParticleEffect = bloodParticleEffect;
                custom.onlinePlayerTag = onlinePlayerTag;
                custom.bulletDamage = bulletDamage;
                custom.shooter = shooter;
                custom.BussinLevel = Mathf.Clamp(CNRDLCWeaponSystem.GetBussinLevel(), 1, 3);

                _bussinBulletTemplate = template;
                ws.ShotGun.bullet = template.transform;
                ModEntry.Log("CNR DLC weapons: Bussin' custom falloff projectile armed for level " + custom.BussinLevel);
            }
            catch (Exception ex)
            {
                ModEntry.Log("CNR DLC weapons: custom projectile setup failed: " + ex.Message);
            }
        }

        private void DriveBussinReloadInput()
        {
            if (_bussinWeapon == null || !_bussinWeapon.gameObject.activeInHierarchy)
            {
                _bussinReloadQueued = false;
                return;
            }
            if (Time.timeScale < 0.01f) return;

            if (_bussinReloadQueued)
            {
                if (Time.time < _bussinReloadAt) return;
                _bussinReloadQueued = false;
                BeginBussinReload();
                return;
            }

            if (PlayerPrefs.GetInt("FpsReload", 0) != 1) return;

            // Consume the same one-shot HUD flag vanilla uses, but route it directly to
            // the active cloned shotgun. Otherwise another WeaponScript.LateUpdate can
            // clear FpsReload before this runtime-created weapon ever sees it.
            PlayerPrefs.SetInt("FpsReload", 0);
            if (_bussinWeapon.ShotGun == null || _bussinWeapon.isReload) return;
            if (_bussinWeapon.ShotGun.bulletsLeft >= _bussinWeapon.ShotGun.bulletsPerClip) return;
            if (_bussinWeapon.ShotGun.clips <= 0) return;

            // A one-shell Bussin can request reload essentially on top of the shot. Give
            // the blast a tiny lead-in so reload audio/animation does not begin on the
            // exact same frame as the muzzle report.
            float earliestReload = _bussinLastShotAt + BussinReloadAudioGuard;
            if (Time.time < earliestReload)
            {
                _bussinReloadQueued = true;
                _bussinReloadAt = earliestReload;
                return;
            }

            BeginBussinReload();
        }

        private void BeginBussinReload()
        {
            if (_bussinWeapon == null || _bussinWeapon.ShotGun == null || _bussinWeapon.isReload) return;
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
            if (fired > 0)
            {
                _bussinLastShotAt = Time.time;
                PlayBussinFireSound();
            }
            for (int i = 0; i < fired; i++) ApplyBussinRecoil();
        }

        private void PlayBussinFireSound()
        {
            if (_bussinWeapon == null || _bussinWeapon.ShotGun == null || _bussinWeapon.ShotGun.fireSound == null) return;
            AudioSource donorAudio = _bussinWeapon.audio;
            if (_bussinFireAudio == null)
            {
                _bussinFireAudio = _bussinWeapon.gameObject.AddComponent<AudioSource>();
                _bussinFireAudio.playOnAwake = false;
                _bussinFireAudio.loop = false;
            }
            if (donorAudio != null)
            {
                // Vanilla already started this clip on the shared fire/reload source.
                // Stop that copy and replay it on our dedicated source so reload audio
                // cannot cut the blast off a frame later.
                donorAudio.Stop();
                _bussinFireAudio.volume = donorAudio.volume;
                _bussinFireAudio.pitch = donorAudio.pitch;
            }
            _bussinFireAudio.clip = _bussinWeapon.ShotGun.fireSound;
            _bussinFireAudio.Play();
        }

        private void ApplyBussinRecoil()
        {
            FPScontroller fps = FindBoundLocalController();
            Vector3 aimForward = FindLocalAimForward(fps);
            if (aimForward.sqrMagnitude <= 0.0001f) return;

            float velocityMultiplier = Mathf.Clamp(CNRMatchSettings.Active.BussinVelocityTenths, 0, 50) / 10f;
            if (velocityMultiplier <= 0f) return;

            Vector3 baseImpulse = -aimForward.normalized * BussinRecoilVelocity * velocityMultiplier;
            Vector3 horizontalImpulse = new Vector3(baseImpulse.x, 0f, baseImpulse.z) * BussinHorizontalVelocityScale;
            float verticalImpulse = baseImpulse.y * BussinVerticalVelocityScale;
            _bussinHorizontalVelocity += horizontalImpulse;

            // CNRSettingsMod owns the mobile jump arc when present. Feed the vertical
            // component into that same velocity, but scale the impulse from the player's
            // current vertical motion: damp it while already rising and compensate hard
            // falls so a downward blast still produces an actual upward kick.
            float appliedVertical = 0f;
            if (!TryInjectSettingsJumpVelocity(verticalImpulse, out appliedVertical))
            {
                CharacterController cc = FindLocalCharacterController();
                float currentVertical = cc != null ? cc.velocity.y : 0f;
                appliedVertical = ComputeBussinVerticalImpulse(verticalImpulse, currentVertical);
                _bussinFallbackVerticalVelocity += appliedVertical;
            }

            ModEntry.Log("CNR DLC weapons: Bussin' recoil base=" + baseImpulse +
                " horizontalImpulse=" + horizontalImpulse +
                " verticalImpulse=" + verticalImpulse +
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
            if (_bussinBulletTemplate != null) Destroy(_bussinBulletTemplate);
            _bussinBulletTemplate = null;
            foreach (KeyValuePair<Renderer, bool> kv in _hidden)
                if (kv.Key != null) kv.Key.enabled = kv.Value;
            _hidden.Clear();

            if (_bussinWeapon != null) Destroy(_bussinWeapon.gameObject);
            _bussinWeapon = null;
            _bussinFireAudio = null;
            _bussinReloadQueued = false;
            _bussinLastShotAt = -10f;
            _placeholderWeapon = null;
            _weaponManager = null;
            _bussinSlotIndex = -1;
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
            CNRBussinNetworkState.LocalVisualLevel = 0;
            UnbindBussinWeapon();
            if (_rifleOverrideActive)
            {
                CNRDLCWeaponSystem.EndRifleOverride();
                _rifleOverrideActive = false;
            }
        }
    }
}
