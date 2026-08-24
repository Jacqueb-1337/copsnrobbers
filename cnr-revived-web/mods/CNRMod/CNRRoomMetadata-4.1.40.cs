using System;
using System.Collections;
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
            try
            {
                activeUrl = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "") ?? "";
                customName = PlayerPrefs.GetString("CNRMod_CustomMapName", "") ?? "";
            }
            catch { }

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
                string key = gameMode + "|" + mapId + "|" + mapName + "|" + thumbUrl;

                if (key != _lastKey)
                {
                    _lastKey = key;
                    ApplyMapMetadata(mapId, mapName, thumbUrl);
                    ApplyModeMetadata(gameMode);
                }

                // Keep these authoritative even if another legacy component refreshes the card.
                if (!string.IsNullOrEmpty(mapName) && Original.mapNameLabel != null && Original.mapNameLabel.text != mapName)
                    Original.mapNameLabel.text = mapName;
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

        private void ApplyMapMetadata(string mapId, string mapName, string thumbUrl)
        {
            if (!string.IsNullOrEmpty(mapName) && Original.mapNameLabel != null)
                Original.mapNameLabel.text = mapName;

            if (string.IsNullOrEmpty(mapId) && string.IsNullOrEmpty(thumbUrl))
            {
                SetCustomThumbnail(null);
                return;
            }

            OfficialMapEntry localEntry = CNRMatchMetadata.FindOfficialMap(mapId);
            Texture2D localThumb = !string.IsNullOrEmpty(mapId) ? ContentManager.GetMapThumbnail(mapId) : null;
            if (localThumb != null)
            {
                SetCustomThumbnail(localThumb);
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
            SetCustomThumbnail(tex);
        }

        private void SetCustomThumbnail(Texture2D tex)
        {
            if (Original == null || Original.mapLogoSprite == null) return;
            if (tex == null)
            {
                Original.mapLogoSprite.enabled = true;
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
