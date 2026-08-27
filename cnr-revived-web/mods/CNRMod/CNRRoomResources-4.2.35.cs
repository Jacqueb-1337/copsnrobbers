using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace CNRMods
{
    [Serializable]
    internal class CNRRoomResourceManifest
    {
        public int schema = 1;
        public CNRRoomResource[] resources = new CNRRoomResource[0];
    }

    [Serializable]
    internal class CNRRoomResource
    {
        public string kind = "";
        public string id = "";
        public string url = "";
        public string hash = "";
        public bool required = true;
    }

    internal static class CNRRoomResources
    {
        internal const string PropManifestUrl = "cnru";
        internal const string PropManifestHash = "cnrh";
        internal const string PropInlineResources = "cnrx";
        internal const string GenericCacheDir = "/storage/emulated/0/CNRMods/content_cache/resources/";
        internal const string CustomMapCachePath = "/storage/emulated/0/CNRMods/custom_map_cache.json";

        internal static string HostManifestUrl = "";
        internal static string HostManifestHash = "";
        internal static string MissingRequiredDlcMapId = "";

        internal static string TakeMissingRequiredDlcMapId()
        {
            string id = MissingRequiredDlcMapId;
            MissingRequiredDlcMapId = "";
            return id;
        }

        internal static string BuildInlineHostResources()
        {
            List<CNRRoomResource> list = new List<CNRRoomResource>();
            string id, displayName, thumb;
            if (CNRMatchMetadata.TryGetSelectedCustomMap(out id, out displayName, out thumb))
            {
                OfficialMapEntry official = CNRMatchMetadata.FindOfficialMap(id);
                if (official != null && !string.IsNullOrEmpty(official.Url))
                {
                    list.Add(new CNRRoomResource {
                        kind = CNRDLCMapLoader.IsActive ? "dlcmap" : "map",
                        id = string.IsNullOrEmpty(official.Id) ? SafeId(displayName) : official.Id,
                        url = official.Url,
                        hash = official.Hash ?? "",
                        required = true
                    });
                }
                else
                {
                    string activeUrl = "";
                    try { activeUrl = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "") ?? ""; } catch { }
                    if (!string.IsNullOrEmpty(activeUrl))
                    {
                        string fallbackId = !string.IsNullOrEmpty(id) ? id : "custom_" + HashText(activeUrl).Substring(0, 12);
                        list.Add(new CNRRoomResource {
                            kind = CNRDLCMapLoader.IsActive ? "dlcmap" : "map",
                            id = SafeId(fallbackId),
                            url = activeUrl,
                            hash = "",
                            required = true
                        });
                    }
                }
            }
            return PackInline(list);
        }

        internal static string PackInline(List<CNRRoomResource> resources)
        {
            if (resources == null || resources.Count == 0) return "";
            List<string> rows = new List<string>();
            for (int i = 0; i < resources.Count; i++)
            {
                CNRRoomResource r = resources[i];
                if (r == null || string.IsNullOrEmpty(r.kind) || string.IsNullOrEmpty(r.id)) continue;
                rows.Add(B64(r.kind) + "," + B64(r.id) + "," + B64(r.url ?? "") + "," + B64(r.hash ?? "") + "," + (r.required ? "1" : "0"));
            }
            return string.Join(";", rows.ToArray());
        }

        internal static bool TryUnpackInline(string packed, out List<CNRRoomResource> resources, out string reason)
        {
            resources = new List<CNRRoomResource>();
            reason = "";
            if (string.IsNullOrEmpty(packed)) return true;
            try
            {
                string[] rows = packed.Split(';');
                for (int i = 0; i < rows.Length; i++)
                {
                    if (string.IsNullOrEmpty(rows[i])) continue;
                    string[] p = rows[i].Split(',');
                    if (p.Length < 5) { reason = "Room resource metadata is malformed."; return false; }
                    CNRRoomResource r = new CNRRoomResource();
                    r.kind = UnB64(p[0]);
                    r.id = UnB64(p[1]);
                    r.url = UnB64(p[2]);
                    r.hash = UnB64(p[3]);
                    r.required = p[4] != "0";
                    if (!ValidateDescriptor(r, out reason)) return false;
                    resources.Add(r);
                }
                return true;
            }
            catch (Exception ex)
            {
                reason = "Could not parse room resources: " + ex.Message;
                return false;
            }
        }

        internal static IEnumerator PrepareRoom(RoomInfo room, Action<string> progress, Action<bool, string> completed)
        {
            MissingRequiredDlcMapId = "";
            if (completed == null) yield break;
            if (room == null) { completed(false, "Room information is unavailable."); yield break; }

            List<CNRRoomResource> resources;
            string reason;
            string inline = CNRCompatibility.GetRoomProp(room, PropInlineResources);
            if (!TryUnpackInline(inline, out resources, out reason))
            {
                completed(false, reason);
                yield break;
            }

            string manifestUrl = CNRCompatibility.GetRoomProp(room, PropManifestUrl) ?? "";
            string manifestHash = CNRCompatibility.GetRoomProp(room, PropManifestHash) ?? "";
            if (!string.IsNullOrEmpty(manifestUrl))
            {
                if (!IsSafeHttpUrl(manifestUrl))
                {
                    completed(false, "Room resource manifest URL is not valid HTTP/HTTPS.");
                    yield break;
                }
                if (progress != null) progress("Fetching required resource manifest...");
                WWW manifestRequest = new WWW(manifestUrl);
                yield return manifestRequest;
                if (!string.IsNullOrEmpty(manifestRequest.error) || string.IsNullOrEmpty(manifestRequest.text))
                {
                    completed(false, "Could not fetch required room resources: " + (manifestRequest.error ?? "empty manifest"));
                    yield break;
                }
                byte[] manifestBytes = Encoding.UTF8.GetBytes(manifestRequest.text);
                if (!string.IsNullOrEmpty(manifestHash) && !VerifyHash(manifestBytes, manifestHash))
                {
                    completed(false, "Required resource manifest failed checksum verification.");
                    yield break;
                }
                CNRRoomResourceManifest remote;
                try { remote = JsonReader.Deserialize<CNRRoomResourceManifest>(manifestRequest.text); }
                catch (Exception ex) { completed(false, "Required resource manifest is invalid: " + ex.Message); yield break; }
                if (remote == null || remote.schema != 1 || remote.resources == null)
                {
                    completed(false, "Required resource manifest has an unsupported schema.");
                    yield break;
                }
                for (int i = 0; i < remote.resources.Length; i++)
                {
                    CNRRoomResource r = remote.resources[i];
                    if (!ValidateDescriptor(r, out reason)) { completed(false, reason); yield break; }
                    resources.Add(r);
                }
            }

            // A room advertising a custom-map identity must provide enough information
            // to acquire it, unless our trusted content manifest already knows that map.
            string mapId = CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropMapId) ?? "";
            string mapName = CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropMapName) ?? "";
            if (!string.IsNullOrEmpty(mapId) || !string.IsNullOrEmpty(mapName))
            {
                bool hasMapDescriptor = false;
                for (int i = 0; i < resources.Count; i++)
                    if (resources[i] != null && (string.Equals(resources[i].kind, "map", StringComparison.OrdinalIgnoreCase) || string.Equals(resources[i].kind, "dlcmap", StringComparison.OrdinalIgnoreCase))) { hasMapDescriptor = true; break; }
                if (!hasMapDescriptor)
                {
                    OfficialMapEntry known = CNRMatchMetadata.FindOfficialMap(mapId);
                    if (known != null && !string.IsNullOrEmpty(known.Url))
                    {
                        string advertisedType = CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropMapType) ?? "";
                        resources.Add(new CNRRoomResource { kind = string.Equals(advertisedType, "dlc", StringComparison.OrdinalIgnoreCase) ? "dlcmap" : "map", id = known.Id, url = known.Url, hash = known.Hash ?? "", required = true });
                    }
                    else
                    {
                        completed(false, "Cannot join: required custom map metadata is unavailable.");
                        yield break;
                    }
                }
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < resources.Count; i++)
            {
                CNRRoomResource r = resources[i];
                if (r == null) continue;
                string dedupe = (r.kind ?? "") + ":" + (r.id ?? "");
                if (!seen.Add(dedupe)) continue;

                string path = ResolvePath(r);
                if (string.IsNullOrEmpty(path))
                {
                    if (r.required) { completed(false, "Unsupported required resource type: " + r.kind); yield break; }
                    continue;
                }

                bool ready = File.Exists(path);
                if (ready && !string.IsNullOrEmpty(r.hash))
                {
                    if (progress != null) progress("Verifying " + FriendlyName(r) + "...");
                    ready = VerifyFileHash(path, r.hash);
                    if (!ready)
                    {
                        try { File.Delete(path); } catch { }
                        ModEntry.Log("PreJoin resource checksum mismatch, redownload required: " + path);
                    }
                }

                if (!ready)
                {
                    if (string.Equals(r.kind, "dlcmap", StringComparison.OrdinalIgnoreCase))
                    {
                        if (r.required)
                        {
                            MissingRequiredDlcMapId = r.id ?? "";
                            completed(false, "Required DLC map '" + r.id + "' is not downloaded.");
                            yield break;
                        }
                        continue;
                    }
                    if (string.IsNullOrEmpty(r.url) || !IsSafeHttpUrl(r.url))
                    {
                        if (r.required) { completed(false, "Cannot join: " + FriendlyName(r) + " is missing and no download URL was supplied."); yield break; }
                        continue;
                    }
                    if (progress != null) progress("Downloading " + FriendlyName(r) + "...");
                    WWW req = new WWW(r.url);
                    yield return req;
                    if (!string.IsNullOrEmpty(req.error) || req.bytes == null || req.bytes.Length == 0)
                    {
                        if (r.required) { completed(false, "Could not download " + FriendlyName(r) + ": " + (req.error ?? "empty file")); yield break; }
                        continue;
                    }
                    int maxBytes = string.Equals(r.kind, "dlcmap", StringComparison.OrdinalIgnoreCase) ? 128 * 1024 * 1024 : 64 * 1024 * 1024;
                    if (req.bytes.Length > maxBytes)
                    {
                        completed(false, "Required resource is too large to download safely.");
                        yield break;
                    }
                    if (!string.IsNullOrEmpty(r.hash) && !VerifyHash(req.bytes, r.hash))
                    {
                        completed(false, FriendlyName(r) + " failed checksum verification.");
                        yield break;
                    }
                    try
                    {
                        string dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        File.WriteAllBytes(path, req.bytes);
                    }
                    catch (Exception ex)
                    {
                        completed(false, "Could not save " + FriendlyName(r) + ": " + ex.Message);
                        yield break;
                    }
                }

                if (string.Equals(r.kind, "dlcmap", StringComparison.OrdinalIgnoreCase))
                {
                    if (!CNRDLCMapLoader.ActivateFile(path, r.url, r.id, out reason)) { completed(false, reason); yield break; }
                }
                else if (string.Equals(r.kind, "map", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ActivateMap(path, r, out reason)) { completed(false, reason); yield break; }
                }
            }

            completed(true, "");
        }

        private static bool ActivateMap(string path, CNRRoomResource r, out string reason)
        {
            reason = "";
            CNRDLCMapLoader.ClearActive();
            try
            {
                if (!File.Exists(path)) { reason = "Required custom map disappeared during preflight."; return false; }
                File.Copy(path, CustomMapCachePath, true);
                PlayerPrefs.SetInt("CNRMod_MapCacheReady", 1);
                PlayerPrefs.SetString("CNRMod_ActiveMapURL", r.url ?? "");
                PlayerPrefs.SetString("CNRMod_CustomMapName", r.id ?? "Custom Map");
                PlayerPrefs.Save();
                ModEntry.Log("PreJoin: activated map resource id=" + r.id + " cache=" + path);
                return true;
            }
            catch (Exception ex)
            {
                reason = "Could not activate required custom map: " + ex.Message;
                return false;
            }
        }

        internal static string ResolvePath(CNRRoomResource r)
        {
            if (r == null) return null;
            string id = SafeId(r.id);
            if (string.IsNullOrEmpty(id)) return null;
            string kind = (r.kind ?? "").Trim().ToLowerInvariant();
            if (kind == "map" || kind == "dlcmap") return ContentManager.MapCacheDir + id + ".json";
            if (kind == "data") return "/storage/emulated/0/CNRMods/content_cache/data/" + id + ".json";
            if (kind == "texture") return "/storage/emulated/0/CNRMods/content_cache/textures/" + id + ".png";
            if (kind == "asset" || kind == "assetbundle") return GenericCacheDir + id + ".bin";
            return null;
        }

        private static bool ValidateDescriptor(CNRRoomResource r, out string reason)
        {
            reason = "";
            if (r == null) { reason = "Room resource manifest contains an empty resource."; return false; }
            string kind = (r.kind ?? "").Trim().ToLowerInvariant();
            if (kind != "map" && kind != "dlcmap" && kind != "data" && kind != "texture" && kind != "asset" && kind != "assetbundle")
            {
                reason = "Room requires an unsupported resource type: " + (r.kind ?? "unknown");
                return false;
            }
            if (string.IsNullOrEmpty(SafeId(r.id))) { reason = "Room resource has an invalid id."; return false; }
            if (!string.IsNullOrEmpty(r.url) && !IsSafeHttpUrl(r.url)) { reason = "Room resource has an invalid download URL."; return false; }
            if (!string.IsNullOrEmpty(r.hash) && !IsSupportedHash(r.hash)) { reason = "Room resource uses an unsupported checksum."; return false; }
            return true;
        }

        private static string FriendlyName(CNRRoomResource r)
        {
            string kind = string.IsNullOrEmpty(r.kind) ? "resource" : r.kind;
            return kind + " " + r.id;
        }

        private static bool IsSafeHttpUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || url.Length > 1024) return false;
            return url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupportedHash(string raw)
        {
            string h = NormalizeHash(raw);
            return h.Length == 32 || h.Length == 64;
        }

        internal static bool VerifyFileHash(string path, string expected)
        {
            try { return VerifyHash(File.ReadAllBytes(path), expected); }
            catch { return false; }
        }

        internal static bool VerifyHash(byte[] bytes, string expected)
        {
            if (bytes == null || string.IsNullOrEmpty(expected)) return false;
            string clean = NormalizeHash(expected);
            byte[] digest;
            if (clean.Length == 32)
            {
                using (MD5 md5 = MD5.Create()) digest = md5.ComputeHash(bytes);
            }
            else if (clean.Length == 64)
            {
                using (SHA256 sha = SHA256.Create()) digest = sha.ComputeHash(bytes);
            }
            else return false;
            return string.Equals(ToHex(digest), clean, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeHash(string raw)
        {
            string h = (raw ?? "").Trim().ToLowerInvariant();
            if (h.StartsWith("md5:")) h = h.Substring(4);
            else if (h.StartsWith("sha256:")) h = h.Substring(7);
            return h;
        }

        private static string ToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }

        private static string SafeId(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < raw.Length && sb.Length < 80; i++)
            {
                char c = raw[i];
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_') sb.Append(c);
            }
            return sb.ToString();
        }

        private static string HashText(string text)
        {
            using (MD5 md5 = MD5.Create()) return ToHex(md5.ComputeHash(Encoding.UTF8.GetBytes(text ?? "")));
        }

        private static string B64(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? ""));
        }

        private static string UnB64(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value ?? ""));
        }
    }
}
