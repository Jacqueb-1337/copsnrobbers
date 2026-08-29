using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CNRMods
{
    // Immutable room metadata advertised through Photon lobby properties.
    // Runtime events remain responsible only for mutable match state.
    internal static class CNRMatchMetadata
    {
        internal const string PropGameMode = "cnrg";
        internal const string PropMapId = "cnmi";
        internal const string PropMapName = "cnmn";
        internal const string PropMapThumb = "cnmt";
        internal const string PropMapType = "cnmty";

        internal static string CanonicalFromVanillaMode(string raw)
        {
            if (raw == "0") return "tdm";
            if (raw == "1") return CtfMode.PendingCtf || CtfMode.IsCtfRoom ? "ctf" : "stronghold";
            if (raw == "2") return ZombieMode.PendingZombie || ZombieMode.IsZombieRoom ? "zombies" : "kc";
            return "";
        }

        internal static OfficialMapEntry FindOfficialMapByUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || ContentManager.OfficialMaps == null) return null;
            for (int i = 0; i < ContentManager.OfficialMaps.Length; i++)
            {
                OfficialMapEntry map = ContentManager.OfficialMaps[i];
                if (map != null && !string.IsNullOrEmpty(map.Url) &&
                    string.Equals(map.Url, url, StringComparison.OrdinalIgnoreCase)) return map;
            }
            return null;
        }

        internal static bool ValidateInitialAdvertisement(System.Collections.Hashtable props, out string reason)
        {
            reason = "";
            if (props == null) { reason = "Room advertisement payload is missing."; return false; }

            string packedSettings = props.ContainsKey(CNRMatchSettings.PropSettings) ? props[CNRMatchSettings.PropSettings] as string : "";
            CNRMatchSettingsData settings;
            if (!CNRMatchSettings.TryUnpack(packedSettings, out settings, out reason))
            {
                reason = "Room advertisement has invalid match settings: " + reason;
                return false;
            }

            string mode = props.ContainsKey(PropGameMode) ? Convert.ToString(props[PropGameMode]) : "";
            mode = CNRMatchSettings.NormalizeMode(mode);
            if (string.IsNullOrEmpty(mode) || settings.Mode != mode)
            {
                reason = "Room advertisement mode does not match its match settings.";
                return false;
            }

            string mapType = props.ContainsKey(PropMapType) ? Convert.ToString(props[PropMapType]).Trim().ToLowerInvariant() : "";
            string mapId = props.ContainsKey(PropMapId) ? Convert.ToString(props[PropMapId]) : "";
            string mapName = props.ContainsKey(PropMapName) ? Convert.ToString(props[PropMapName]) : "";
            if (mapType != "vanilla" && mapType != "legacy" && mapType != "dlc")
            {
                reason = "Room advertisement has an invalid map type.";
                return false;
            }
            if (mapType == "vanilla")
            {
                if (!string.IsNullOrEmpty(mapId) || !string.IsNullOrEmpty(mapName))
                {
                    reason = "Vanilla room advertisement contains custom-map identity.";
                    return false;
                }
                return true;
            }
            if (mapType == "dlc" && string.IsNullOrEmpty(mapId))
            {
                reason = "DLC room advertisement is missing its map ID.";
                return false;
            }
            if (string.IsNullOrEmpty(mapId) && string.IsNullOrEmpty(mapName))
            {
                reason = "Custom room advertisement is missing its map identity.";
                return false;
            }

            string packedResources = props.ContainsKey(CNRRoomResources.PropInlineResources)
                ? Convert.ToString(props[CNRRoomResources.PropInlineResources]) : "";
            List<CNRRoomResource> resources;
            string resourceReason;
            if (!CNRRoomResources.TryUnpackInline(packedResources, out resources, out resourceReason))
            {
                reason = "Room advertisement has invalid resource metadata: " + resourceReason;
                return false;
            }
            bool foundMap = false;
            for (int i = 0; resources != null && i < resources.Count; i++)
            {
                CNRRoomResource r = resources[i];
                if (r == null) continue;
                bool kindMatches = mapType == "dlc"
                    ? string.Equals(r.kind, "dlcmap", StringComparison.OrdinalIgnoreCase)
                    : string.Equals(r.kind, "map", StringComparison.OrdinalIgnoreCase);
                bool idMatches = string.IsNullOrEmpty(mapId) || string.Equals(r.id, mapId, StringComparison.OrdinalIgnoreCase);
                if (kindMatches && idMatches && !string.IsNullOrEmpty(r.url)) { foundMap = true; break; }
            }
            if (!foundMap)
            {
                reason = "Custom room advertisement does not include its required map resource.";
                return false;
            }
            return true;
        }

        internal static string GetSelectedGameMode(MultiplayerSelectDirector msd)
        {
            if (CtfMode.PendingCtf || CtfMode.IsCtfRoom) return "ctf";
            if (ZombieMode.PendingZombie || ZombieMode.IsZombieRoom) return "zombies";
            if (msd == null) return "";
            if (msd.curModeSet == GrowthGameModeTag.tTeamDeathMatch) return "tdm";
            if (msd.curModeSet == GrowthGameModeTag.tStronghold) return "stronghold";
            if (msd.curModeSet == GrowthGameModeTag.tKillingCompetition) return "kc";
            return "";
        }

        internal static string GetDisplayMode(string id)
        {
            if (id == "ctf") return "CAPTURE THE FLAG";
            if (id == "zombies") return "ZOMBIES";
            if (id == "stronghold") return "STRONGHOLD";
            if (id == "kc") return "KILLING COMPETITION";
            if (id == "tdm") return "TEAM DEATHMATCH";
            return "";
        }

        internal static int ExpectedVanillaMode(string id)
        {
            if (id == "tdm") return 0;
            if (id == "stronghold" || id == "ctf") return 1;
            if (id == "kc" || id == "zombies") return 2;
            return -1;
        }

        internal static bool IsCustomMode(string id)
        {
            return id == "ctf" || id == "zombies";
        }

        internal static bool TryGetSelectedCustomMap(out string id, out string displayName, out string thumbUrl)
        {
            id = "";
            displayName = "";
            thumbUrl = "";

            string activeUrl = "";
            string customName = "";
            string dlcId = "";
            try
            {
                activeUrl = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "") ?? "";
                customName = PlayerPrefs.GetString("CNRMod_CustomMapName", "") ?? "";
                if (CNRDLCMapLoader.IsActive)
                    dlcId = PlayerPrefs.GetString(CNRDLCMapLoader.PrefId, "") ?? "";
            }
            catch { }

            // The dedicated DLC id is the authoritative identity. Generic name/URL prefs can
            // be cleared by selector/scene transitions even while the DLC loader remains active.
            if (!string.IsNullOrEmpty(dlcId))
            {
                OfficialMapEntry activeDlc = FindOfficialMap(dlcId);
                id = Limit(dlcId, 64);
                if (activeDlc != null)
                {
                    displayName = Limit(activeDlc.Name, 80);
                    thumbUrl = Limit(activeDlc.ThumbnailUrl, 240);
                }
                else
                    displayName = Limit(!string.IsNullOrEmpty(customName) ? customName : dlcId, 80);
                return true;
            }

            if (string.IsNullOrEmpty(activeUrl) && string.IsNullOrEmpty(customName)) return false;

            OfficialMapEntry[] maps = ContentManager.OfficialMaps;
            if (maps != null)
            {
                for (int i = 0; i < maps.Length; i++)
                {
                    OfficialMapEntry map = maps[i];
                    if (map == null) continue;
                    bool urlMatch = !string.IsNullOrEmpty(activeUrl) &&
                        string.Equals(map.Url, activeUrl, StringComparison.OrdinalIgnoreCase);
                    bool nameMatch = !string.IsNullOrEmpty(customName) &&
                        string.Equals(map.Name, customName, StringComparison.OrdinalIgnoreCase);
                    if (!urlMatch && !nameMatch) continue;

                    id = Limit(map.Id, 64);
                    displayName = Limit(map.Name, 80);
                    thumbUrl = Limit(map.ThumbnailUrl, 240);
                    return true;
                }
            }

            // User URL/custom slots do not necessarily have a manifest thumbnail,
            // but still advertise the real map name so the donor scene is not shown.
            displayName = Limit(customName, 80);
            return !string.IsNullOrEmpty(displayName);
        }

        internal static OfficialMapEntry FindOfficialMap(string id)
        {
            if (string.IsNullOrEmpty(id) || ContentManager.OfficialMaps == null) return null;
            for (int i = 0; i < ContentManager.OfficialMaps.Length; i++)
            {
                OfficialMapEntry map = ContentManager.OfficialMaps[i];
                if (map != null && string.Equals(map.Id, id, StringComparison.OrdinalIgnoreCase)) return map;
            }
            return null;
        }

        internal static bool IsSafeThumbnailUrl(string advertised, OfficialMapEntry localEntry)
        {
            if (string.IsNullOrEmpty(advertised)) return false;
            // Prefer our own signed/content-manifest value. A room host may not turn
            // the browser into an arbitrary URL fetcher.
            if (localEntry != null && !string.IsNullOrEmpty(localEntry.ThumbnailUrl))
                return string.Equals(advertised, localEntry.ThumbnailUrl, StringComparison.OrdinalIgnoreCase);

            // Official first-party thumbnails are also safe before ContentManager has
            // finished loading its local manifest.
            return advertised.StartsWith("https://play.jacqueb.me/", StringComparison.OrdinalIgnoreCase);
        }

        private static string Limit(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return "";
            value = value.Trim();
            return value.Length <= max ? value : value.Substring(0, max);
        }
    }

    // Runs after vanilla CNRRoomInfo.Update so the stock donor map/mode artwork cannot
    // overwrite the real CNR room identity advertised before joining.
    public class CNRRoomCardMetadata : MonoBehaviour
    {
        public CNRRoomInfo Original;

        private string _lastKey = "";
        private UITexture _thumbWidget;
        private UILabel _modeLabel;
        private Texture2D _downloadedThumb;
        private string _thumbRequest = "";

        void LateUpdate()
        {
            try
            {
                if (Original == null || Original.mRoomInfo == null) return;
                RoomInfo room = Original.mRoomInfo;
                string gameMode = CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropGameMode) ?? "";
                string mapId = CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropMapId) ?? "";
                string mapName = CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropMapName) ?? "";
                string thumbUrl = CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropMapThumb) ?? "";
                string mapType = CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropMapType) ?? "";
                bool customMap = string.Equals(mapType, "dlc", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(mapType, "legacy", StringComparison.OrdinalIgnoreCase) ||
                                 !string.IsNullOrEmpty(mapId) || !string.IsNullOrEmpty(mapName) || !string.IsNullOrEmpty(thumbUrl);

                // cnrx is also a lobby property. Recover the map id from its required map
                // descriptor if an older/racy host omitted cnmi/cnmn but did advertise the
                // resource correctly. Never fall back to the donor map for a known custom room.
                if (customMap && string.IsNullOrEmpty(mapId))
                {
                    List<CNRRoomResource> resources;
                    string unpackReason;
                    string packed = CNRCompatibility.GetRoomProp(room, CNRRoomResources.PropInlineResources) ?? "";
                    if (CNRRoomResources.TryUnpackInline(packed, out resources, out unpackReason))
                    {
                        for (int i = 0; i < resources.Count; i++)
                        {
                            CNRRoomResource r = resources[i];
                            if (r == null) continue;
                            if (!string.Equals(r.kind, "map", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(r.kind, "dlcmap", StringComparison.OrdinalIgnoreCase)) continue;
                            mapId = r.id ?? "";
                            break;
                        }
                    }
                }

                OfficialMapEntry localMap = !string.IsNullOrEmpty(mapId) ? CNRMatchMetadata.FindOfficialMap(mapId) : null;
                if (customMap && localMap != null)
                {
                    if (string.IsNullOrEmpty(mapName)) mapName = localMap.Name ?? "";
                    if (string.IsNullOrEmpty(thumbUrl)) thumbUrl = localMap.ThumbnailUrl ?? "";
                }
                if (customMap && string.IsNullOrEmpty(mapName))
                    mapName = !string.IsNullOrEmpty(mapId) ? mapId : (string.Equals(mapType, "dlc", StringComparison.OrdinalIgnoreCase) ? "DLC MAP" : "CUSTOM MAP");

                string key = gameMode + "|" + mapType + "|" + mapId + "|" + mapName + "|" + thumbUrl;

                if (key != _lastKey)
                {
                    _lastKey = key;
                    ApplyMapMetadata(mapId, mapName, thumbUrl, customMap);
                    ApplyModeMetadata(gameMode);
                }

                // Keep these authoritative even if another legacy component refreshes the card.
                if (!string.IsNullOrEmpty(mapName) && Original.mapNameLabel != null && Original.mapNameLabel.text != mapName)
                    Original.mapNameLabel.text = mapName;
                if (customMap && Original.mapLogoSprite != null)
                    Original.mapLogoSprite.enabled = false;
                if (CNRMatchMetadata.IsCustomMode(gameMode))
                {
                    if (Original.modeLogoSprite != null) Original.modeLogoSprite.enabled = false;
                    if (_modeLabel != null) _modeLabel.text = CNRMatchMetadata.GetDisplayMode(gameMode);
                }
            }
            catch (Exception ex)
            {
                ModEntry.Log("RoomCard metadata error: " + ex.Message);
            }
        }

        private void ApplyMapMetadata(string mapId, string mapName, string thumbUrl, bool customMap)
        {
            if (!string.IsNullOrEmpty(mapName) && Original.mapNameLabel != null)
                Original.mapNameLabel.text = mapName;

            if (!customMap)
            {
                SetCustomThumbnail(null, true);
                return;
            }

            // A custom/DLC room must never visually fall back to the donor vanilla map.
            // Hide the donor sprite immediately, then fill this area from local cache or
            // the trusted first-party thumbnail URL when it becomes available.
            SetCustomThumbnail(null, false);

            OfficialMapEntry localEntry = CNRMatchMetadata.FindOfficialMap(mapId);
            Texture2D localThumb = !string.IsNullOrEmpty(mapId) ? ContentManager.GetMapThumbnail(mapId) : null;
            if (localThumb != null)
            {
                SetCustomThumbnail(localThumb, false);
                return;
            }

            string safeUrl = "";
            if (localEntry != null && !string.IsNullOrEmpty(localEntry.ThumbnailUrl)) safeUrl = localEntry.ThumbnailUrl;
            else if (CNRMatchMetadata.IsSafeThumbnailUrl(thumbUrl, localEntry)) safeUrl = thumbUrl;

            if (!string.IsNullOrEmpty(safeUrl) && _thumbRequest != safeUrl)
            {
                _thumbRequest = safeUrl;
                StartCoroutine(FetchThumbnail(safeUrl));
            }
        }

        private IEnumerator FetchThumbnail(string url)
        {
            WWW www = new WWW(url);
            yield return www;
            if (this == null || string.IsNullOrEmpty(url) || _thumbRequest != url) yield break;
            if (!string.IsNullOrEmpty(www.error))
            {
                ModEntry.Log("RoomCard thumbnail fetch failed: " + www.error);
                yield break;
            }
            Texture2D tex = www.texture;
            if (tex == null) yield break;
            if (_downloadedThumb != null && _downloadedThumb != tex) Destroy(_downloadedThumb);
            _downloadedThumb = tex;
            SetCustomThumbnail(tex, false);
        }

        private void SetCustomThumbnail(Texture2D tex, bool showVanillaWhenNull)
        {
            if (Original == null || Original.mapLogoSprite == null) return;
            if (tex == null)
            {
                Original.mapLogoSprite.enabled = showVanillaWhenNull;
                if (_thumbWidget != null) _thumbWidget.enabled = false;
                return;
            }

            if (_thumbWidget == null)
            {
                GameObject go = new GameObject("CNRRoomThumbnail");
                go.transform.parent = Original.mapLogoSprite.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.layer = Original.mapLogoSprite.gameObject.layer;
                _thumbWidget = go.AddComponent<UITexture>();
                _thumbWidget.depth = Original.mapLogoSprite.depth + 1;
                _thumbWidget.pivot = Original.mapLogoSprite.pivot;
            }
            _thumbWidget.mainTexture = tex;
            _thumbWidget.enabled = true;
            Original.mapLogoSprite.enabled = false;
        }

        private void ApplyModeMetadata(string gameMode)
        {
            if (!CNRMatchMetadata.IsCustomMode(gameMode))
            {
                if (Original.modeLogoSprite != null) Original.modeLogoSprite.enabled = true;
                if (_modeLabel != null) _modeLabel.gameObject.SetActive(false);
                return;
            }

            if (Original.modeLogoSprite != null) Original.modeLogoSprite.enabled = false;
            if (_modeLabel == null && Original.mapNameLabel != null && Original.modeLogoSprite != null)
            {
                GameObject go = (GameObject)Instantiate(Original.mapNameLabel.gameObject);
                go.name = "CNRRoomModeLabel";
                go.transform.parent = Original.modeLogoSprite.transform.parent;
                go.transform.localPosition = Original.modeLogoSprite.transform.localPosition;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Original.mapNameLabel.transform.localScale * 0.78f;
                go.layer = Original.modeLogoSprite.gameObject.layer;
                _modeLabel = go.GetComponent<UILabel>();
                if (_modeLabel != null)
                    _modeLabel.depth = Original.modeLogoSprite.depth + 1;
            }
            if (_modeLabel != null)
            {
                _modeLabel.gameObject.SetActive(true);
                _modeLabel.text = CNRMatchMetadata.GetDisplayMode(gameMode);
            }
        }

        void OnDestroy()
        {
            if (_downloadedThumb != null) Destroy(_downloadedThumb);
            _downloadedThumb = null;
        }
    }
}
