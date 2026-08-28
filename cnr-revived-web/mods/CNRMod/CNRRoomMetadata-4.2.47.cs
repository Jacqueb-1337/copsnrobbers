using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

        // Fallback for old/vanilla WWCreateRoomNStart winning the NGUI OnClick race.
        // Re-advertise all immutable CNR properties immediately after the host enters the
        // room, and update Photon property 250 so the same keys are visible in the lobby.
        internal static void RepairHostRoomMetadata()
        {
            try
            {
                Room room = PhotonNetwork.room;
                if (room == null || !PhotonNetwork.isMasterClient) return;

                MultiplayerSelectDirector msd = MultiplayerSelectDirector.mInstance;
                string canonicalMode = GetSelectedGameMode(msd);
                if (string.IsNullOrEmpty(canonicalMode))
                    canonicalMode = CanonicalFromVanillaMode(CNRCompatibility.GetRoomProp(room, "mode"));

                string mapId, mapName, mapThumb;
                bool hasCustomMap = TryGetSelectedCustomMap(out mapId, out mapName, out mapThumb);
                OfficialMapEntry official = hasCustomMap ? FindOfficialMap(mapId) : null;
                bool isDlc = hasCustomMap && ((official != null && official.IsDlcMap) || CNRDLCMapLoader.IsActive);

                System.Collections.Hashtable props = new System.Collections.Hashtable();
                props["cnrp"] = CNRCompatibility.Protocol;
                props["cnrm"] = ModEntry.Version;
                props["cnra"] = CNRCompatibility.GetLocalAppVersion();
                props["cnrr"] = CNRCompatibility.PackRequirements();
                props[CNRMatchSettings.PropSettings] = CNRMatchSettings.PackHost(msd);
                props[CNRRoomResources.PropManifestUrl] = CNRRoomResources.HostManifestUrl ?? "";
                props[CNRRoomResources.PropManifestHash] = CNRRoomResources.HostManifestHash ?? "";
                props[CNRRoomResources.PropInlineResources] = CNRRoomResources.BuildInlineHostResources();
                props[PropGameMode] = canonicalMode ?? "";
                props[PropMapId] = hasCustomMap ? (mapId ?? "") : "";
                props[PropMapName] = hasCustomMap ? (mapName ?? "") : "";
                props[PropMapThumb] = hasCustomMap ? (mapThumb ?? "") : "";
                props[PropMapType] = hasCustomMap ? (isDlc ? "dlc" : "legacy") : "vanilla";

                string[] lobbyProps = new string[] {
                    "map", "version", "mode", "cnrp", "cnrm", "cnra", "cnrr",
                    PropGameMode, PropMapId, PropMapName, PropMapThumb, PropMapType,
                    CNRMatchSettings.PropSettings, CNRRoomResources.PropManifestUrl,
                    CNRRoomResources.PropManifestHash, CNRRoomResources.PropInlineResources
                };

                // Keep the local RoomInfo coherent immediately so code running before the
                // server echo sees the same metadata.
                foreach (DictionaryEntry entry in props)
                    room.customProperties[entry.Key] = entry.Value;

                BindingFlags f = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                FieldInfo peerField = typeof(PhotonNetwork).GetField("networkingPeer", f);
                object peer = peerField != null ? peerField.GetValue(null) : null;
                if (peer == null) throw new InvalidOperationException("Photon networkingPeer is unavailable.");

                System.Collections.Hashtable serverProps = new System.Collections.Hashtable();
                foreach (DictionaryEntry entry in props)
                    serverProps[entry.Key] = entry.Value;
                serverProps[(byte)250] = lobbyProps; // GamePropertyKey.PropsListedInLobby

                MethodInfo setRoom = peer.GetType().GetMethod("OpSetPropertiesOfRoom", f, null,
                    new Type[] { typeof(System.Collections.Hashtable), typeof(bool), typeof(byte) }, null);
                if (setRoom == null) throw new MissingMethodException("OpSetPropertiesOfRoom");
                object result = setRoom.Invoke(peer, new object[] { serverProps, true, (byte)0 });

                try
                {
                    PropertyInfo listed = room.GetType().GetProperty("propertiesListedInLobby", f);
                    MethodInfo setter = listed != null ? listed.GetSetMethod(true) : null;
                    if (setter != null) setter.Invoke(room, new object[] { lobbyProps });
                }
                catch { }

                ModEntry.Log("HostRoomMetadata: advertised room=" + room.name +
                    " mode=" + (canonicalMode ?? "") + " mapId=" + (mapId ?? "") +
                    " mapType=" + (hasCustomMap ? (isDlc ? "dlc" : "legacy") : "vanilla") +
                    " op=" + (result == null ? "null" : result.ToString()));
            }
            catch (Exception ex)
            {
                ModEntry.Log("HostRoomMetadata repair error: " + ex.Message);
            }
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
