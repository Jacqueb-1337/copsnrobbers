using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExitGames.Client.Photon;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace CNRMods
{
    [Serializable]
    internal class CNRMultiplayerManifest
    {
        public int schema;
        public string protocol;
        public CNRRequiredMod[] requiredMods;
    }

    [Serializable]
    internal class CNRRequiredMod
    {
        public string id;
        public string minVersion;
        public string exactVersion;
    }

    internal static class CNRCompatibility
    {
        internal const string ManifestUrl = "https://play.jacqueb.me/mods/multiplayer-manifest.json";
        internal const string CachePath = "/storage/emulated/0/CNRMods/multiplayer-manifest.json";
        internal const string DefaultProtocol = "1";

        private static CNRMultiplayerManifest _manifest = MakeDefaultManifest();
        private static string _manifestRequiredProtocol = DefaultProtocol;

        internal static string Protocol
        {
            get { return DefaultProtocol; }
        }

        internal static CNRMultiplayerManifest Manifest { get { return _manifest; } }

        internal static void ApplyManifest(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return;
            try
            {
                CNRMultiplayerManifest parsed = JsonReader.Deserialize<CNRMultiplayerManifest>(raw);
                if (parsed == null || parsed.schema <= 0) return;
                if (string.IsNullOrEmpty(parsed.protocol)) parsed.protocol = DefaultProtocol;
                if (parsed.requiredMods == null) parsed.requiredMods = new CNRRequiredMod[0];
                _manifestRequiredProtocol = parsed.protocol;
                _manifest = parsed;
                ModEntry.Log("Compatibility manifest applied: requiredProtocol=" + _manifestRequiredProtocol + " binaryProtocol=" + Protocol + " requiredMods=" + parsed.requiredMods.Length);
            }
            catch (Exception ex) { ModEntry.Log("Compatibility manifest parse error: " + ex.Message); }
        }

        internal static CNRMultiplayerManifest MakeDefaultManifest()
        {
            CNRMultiplayerManifest m = new CNRMultiplayerManifest();
            m.schema = 1;
            m.protocol = DefaultProtocol;
            m.requiredMods = new CNRRequiredMod[]
            {
                new CNRRequiredMod { id = "CNRModManager", minVersion = "1.6.1" },
                new CNRRequiredMod { id = "CNRSettingsMod", minVersion = "3.1.103" }
            };
            return m;
        }

        internal static string PackRequirements()
        {
            CNRRequiredMod[] reqs = _manifest.requiredMods ?? new CNRRequiredMod[0];
            List<string> parts = new List<string>();
            for (int i = 0; i < reqs.Length; i++)
            {
                CNRRequiredMod r = reqs[i];
                if (r == null || string.IsNullOrEmpty(r.id)) continue;
                if (!string.IsNullOrEmpty(r.exactVersion)) parts.Add(r.id + "=" + r.exactVersion);
                else if (!string.IsNullOrEmpty(r.minVersion)) parts.Add(r.id + ">=" + r.minVersion);
            }
            return string.Join("|", parts.ToArray());
        }

        internal static bool ValidateLocalEnvironment(out string reason)
        {
            reason = "";
            if (!string.Equals(_manifestRequiredProtocol, Protocol, StringComparison.Ordinal))
            {
                reason = "CNRMod update required. Multiplayer manifest requires protocol " + _manifestRequiredProtocol + " but this build supports " + Protocol + ".";
                return false;
            }
            if (!ValidateRequirements(PackRequirements(), out reason)) return false;
            return true;
        }

        internal static bool ValidateRoom(RoomInfo room, out string reason, out bool modManagerHelpful)
        {
            reason = "";
            modManagerHelpful = false;
            if (!ValidateLocalEnvironment(out reason)) { modManagerHelpful = true; return false; }
            if (room == null) { reason = "Room information is unavailable."; return false; }

            string gameVersion = GetRoomProp(room, "version");
            string localGameVersion = SafeGameVersion();
            if (string.IsNullOrEmpty(gameVersion) || !string.Equals(gameVersion, localGameVersion, StringComparison.Ordinal))
            {
                reason = "Game version mismatch. Host: " + Safe(gameVersion) + "  You: " + Safe(localGameVersion);
                return false;
            }

            string apk = GetRoomProp(room, "cnra");
            string localApk = GetLocalAppVersion();
            if (string.IsNullOrEmpty(apk))
            {
                reason = "Host is using an older CNR build that does not advertise APK compatibility.";
                modManagerHelpful = true;
                return false;
            }
            if (!string.Equals(apk, localApk, StringComparison.Ordinal))
            {
                reason = "APK version mismatch. Host: " + apk + "  You: " + localApk;
                return false;
            }

            string protocol = GetRoomProp(room, "cnrp");
            if (string.IsNullOrEmpty(protocol) || !string.Equals(protocol, Protocol, StringComparison.Ordinal))
            {
                reason = "Multiplayer protocol mismatch. Host: " + Safe(protocol) + "  You: " + Protocol;
                modManagerHelpful = true;
                return false;
            }

            string cnrmod = GetRoomProp(room, "cnrm");
            if (string.IsNullOrEmpty(cnrmod) || !string.Equals(cnrmod, ModEntry.Version, StringComparison.Ordinal))
            {
                reason = "CNRMod version mismatch. Host: " + Safe(cnrmod) + "  You: " + ModEntry.Version;
                modManagerHelpful = true;
                return false;
            }

            string req = GetRoomProp(room, "cnrr");
            if (!ValidateRequirements(req, out reason))
            {
                modManagerHelpful = true;
                return false;
            }
            return true;
        }

        internal static bool ValidateRequirements(string packed, out string reason)
        {
            reason = "";
            if (string.IsNullOrEmpty(packed)) return true;
            string[] reqs = packed.Split('|');
            for (int i = 0; i < reqs.Length; i++)
            {
                string token = reqs[i].Trim();
                if (token.Length == 0) continue;
                int p = token.IndexOf(">=");
                bool exact = false;
                if (p < 0) { p = token.IndexOf('='); exact = true; }
                if (p <= 0) continue;
                string id = token.Substring(0, p).Trim();
                string required = token.Substring(p + (exact ? 1 : 2)).Trim();
                string installed = ModEntry.GetModVersion(id);
                if (string.IsNullOrEmpty(installed))
                {
                    reason = "Required mod missing: " + id + " " + (exact ? required : ">= " + required);
                    return false;
                }
                if (exact)
                {
                    if (!string.Equals(installed, required, StringComparison.Ordinal))
                    {
                        reason = id + " must be exactly " + required + " (installed " + installed + ").";
                        return false;
                    }
                }
                else if (CompareVersions(installed, required) < 0)
                {
                    reason = id + " " + required + "+ required (installed " + installed + ").";
                    return false;
                }
            }
            return true;
        }

        internal static string GetRoomProp(RoomInfo room, string key)
        {
            try
            {
                if (room == null || room.customProperties == null || !room.customProperties.ContainsKey(key)) return null;
                object v = room.customProperties[key];
                return v != null ? Convert.ToString(v) : null;
            }
            catch { return null; }
        }

        internal static string SafeGameVersion()
        {
            try { return UserDataController.GetStrVersion() ?? ""; }
            catch { return ""; }
        }

        internal static string GetLocalAppVersion()
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (AndroidJavaObject activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaObject pm = activity.Call<AndroidJavaObject>("getPackageManager"))
                    {
                        string pkg = activity.Call<string>("getPackageName");
                        using (AndroidJavaObject pi = pm.Call<AndroidJavaObject>("getPackageInfo", pkg, 0))
                        {
                            string v = pi.Get<string>("versionName");
                            if (!string.IsNullOrEmpty(v)) return v;
                        }
                    }
                }
            }
            catch (Exception ex) { ModEntry.Log("GetLocalAppVersion error: " + ex.Message); }
            return SafeGameVersion();
        }

        internal static int CompareVersions(string a, string b)
        {
            string[] aa = (a ?? "0").Split('.');
            string[] bb = (b ?? "0").Split('.');
            int count = Math.Max(aa.Length, bb.Length);
            for (int i = 0; i < count; i++)
            {
                int av = ParseVersionPart(i < aa.Length ? aa[i] : "0");
                int bv = ParseVersionPart(i < bb.Length ? bb[i] : "0");
                if (av < bv) return -1;
                if (av > bv) return 1;
            }
            return 0;
        }

        private static int ParseVersionPart(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return 0;
            int end = 0;
            while (end < raw.Length && char.IsDigit(raw[end])) end++;
            int n;
            return int.TryParse(end > 0 ? raw.Substring(0, end) : "0", out n) ? n : 0;
        }

        private static string Safe(string s) { return string.IsNullOrEmpty(s) ? "unknown" : s; }
    }

    public class CNRPreJoinGate : MonoBehaviour
    {
        private float _scanAt;
        private string _blockReason = "";
        private bool _blockCanOpenManager;
        private GUIStyle _box;
        private GUIStyle _button;

        void Start()
        {
            LoadCachedManifest();
            StartCoroutine(FetchManifest());
        }

        void OnLevelWasLoaded(int level)
        {
            _scanAt = 0f;
            _blockReason = "";
        }

        void Update()
        {
            if (Application.loadedLevelName != "MultiplayerSelect") return;
            if (Time.realtimeSinceStartup < _scanAt) return;
            _scanAt = Time.realtimeSinceStartup + 0.25f;
            HookRoomButtons();
            HookMapSelectButtons();
        }

        private void HookRoomButtons()
        {
            CNRRoomInfo[] infos = (CNRRoomInfo[])FindObjectsOfType(typeof(CNRRoomInfo));
            for (int i = 0; i < infos.Length; i++)
            {
                CNRRoomInfo info = infos[i];
                if (info == null || info.gameObject.GetComponent<CNRRoomJoinGuard>() != null) continue;
                CNRRoomJoinGuard guard = info.gameObject.AddComponent<CNRRoomJoinGuard>();
                guard.Gate = this;
                guard.Original = info;
            }
        }

        private void HookMapSelectButtons()
        {
            MapSelectButtonEvent[] buttons = (MapSelectButtonEvent[])FindObjectsOfType(typeof(MapSelectButtonEvent));
            for (int i = 0; i < buttons.Length; i++)
            {
                MapSelectButtonEvent b = buttons[i];
                if (b == null) continue;
                string n = b.buttonName.ToString();
                if (n != "WWStart" && n != "WWQuickStart") continue;
                if (b.gameObject.GetComponent<CNRMapSelectJoinGuard>() != null) continue;
                CNRMapSelectJoinGuard guard = b.gameObject.AddComponent<CNRMapSelectJoinGuard>();
                guard.Gate = this;
                guard.Original = b;
                guard.Action = n;
            }
        }

        internal void JoinRoomPreflight(RoomInfo room)
        {
            bool managerHelpful;
            string reason;
            if (!CNRCompatibility.ValidateRoom(room, out reason, out managerHelpful))
            {
                ShowBlocked(reason, managerHelpful);
                return;
            }
            BeginJoin(room);
        }

        internal void QuickStartPreflight()
        {
            string localReason;
            if (!CNRCompatibility.ValidateLocalEnvironment(out localReason)) { ShowBlocked(localReason, true); return; }
            RoomInfo[] rooms = PhotonNetwork.GetRoomList();
            List<RoomInfo> compatible = new List<RoomInfo>();
            for (int i = 0; rooms != null && i < rooms.Length; i++)
            {
                RoomInfo r = rooms[i];
                string reason;
                bool managerHelpful;
                if (r != null && r.maxPlayers != r.playerCount && CNRCompatibility.ValidateRoom(r, out reason, out managerHelpful))
                    compatible.Add(r);
            }
            if (compatible.Count == 0)
            {
                ShowBlocked("No compatible rooms are currently available.", false);
                return;
            }
            BeginJoin(compatible[UnityEngine.Random.Range(0, compatible.Count)]);
        }

        internal void CreateRoomWithCompatibility()
        {
            string localReason;
            if (!CNRCompatibility.ValidateLocalEnvironment(out localReason)) { ShowBlocked(localReason, true); return; }
            MultiplayerSelectDirector msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;
            try
            {
                msd.ToSubScene(MSD_SubScene.Loading);
                Hashtable props = new Hashtable();
                props["map"] = msd.mCurWWMapSelect;
                props["version"] = CNRCompatibility.SafeGameVersion();
                props["mode"] = ((int)msd.curModeSet).ToString();
                props["cnrp"] = CNRCompatibility.Protocol;
                props["cnrm"] = ModEntry.Version;
                props["cnra"] = CNRCompatibility.GetLocalAppVersion();
                props["cnrr"] = CNRCompatibility.PackRequirements();
                string[] lobbyProps = new string[] { "map", "version", "mode", "cnrp", "cnrm", "cnra", "cnrr" };

                int maxPlayers = 8;
                FieldInfo maxFi = typeof(MultiplayerSelectDirector).GetField("mWWMaxPlayersNum", BindingFlags.Instance | BindingFlags.NonPublic);
                if (maxFi != null) maxPlayers = Convert.ToInt32(maxFi.GetValue(msd));

                PhotonNetwork.CreateRoom(UserDataController.GetMyRoomName(), true, true, maxPlayers, props, lobbyProps);
                PhotonNetwork.playerName = GrowthManagerKit.GetMyNickName();
                msd.StopAllCoroutines();
                msd.StartCoroutine(msd.WWRoomCreateTimeOutChk());
                ModEntry.Log("PreJoin: created compatible room protocol=" + CNRCompatibility.Protocol + " cnr=" + ModEntry.Version + " apk=" + CNRCompatibility.GetLocalAppVersion() + " req=" + CNRCompatibility.PackRequirements());
            }
            catch (Exception ex)
            {
                ModEntry.Log("PreJoin create room error: " + ex.Message);
                ShowBlocked("Could not create room: " + ex.Message, false);
            }
        }

        private void BeginJoin(RoomInfo room)
        {
            MultiplayerSelectDirector msd = MultiplayerSelectDirector.mInstance;
            if (msd == null || room == null) return;
            try
            {
                if (room.maxPlayers == room.playerCount)
                {
                    ShowBlocked("Selected room is full.", false);
                    return;
                }
                msd.ToSubScene(MSD_SubScene.Loading);
                PhotonNetwork.playerName = GrowthManagerKit.GetMyNickName();
                PhotonNetwork.JoinRoom(room.name);
                msd.mCurWWMapSelect = CNRCompatibility.GetRoomProp(room, "map") ?? msd.mCurWWMapSelect;
                msd.StopAllCoroutines();
                msd.StartCoroutine(msd.WWRoomJoinTimeOutChk());
                ModEntry.Log("PreJoin: accepted room " + room.name + " protocol=" + CNRCompatibility.GetRoomProp(room, "cnrp") + " cnr=" + CNRCompatibility.GetRoomProp(room, "cnrm"));
            }
            catch (Exception ex)
            {
                ModEntry.Log("PreJoin room join error: " + ex.Message);
                ShowBlocked("Could not join room: " + ex.Message, false);
            }
        }

        internal void ShowBlocked(string reason, bool managerHelpful)
        {
            _blockReason = string.IsNullOrEmpty(reason) ? "This room is not compatible with your installation." : reason;
            _blockCanOpenManager = managerHelpful;
            ModEntry.Log("PreJoin BLOCKED: " + _blockReason);
        }

        private void LoadCachedManifest()
        {
            try
            {
                if (File.Exists(CNRCompatibility.CachePath)) CNRCompatibility.ApplyManifest(File.ReadAllText(CNRCompatibility.CachePath));
            }
            catch (Exception ex) { ModEntry.Log("Compatibility cache read error: " + ex.Message); }
        }

        private IEnumerator FetchManifest()
        {
            WWW www = new WWW(CNRCompatibility.ManifestUrl);
            yield return www;
            if (!string.IsNullOrEmpty(www.error) || string.IsNullOrEmpty(www.text))
            {
                ModEntry.Log("Compatibility manifest fetch failed; using cached/built-in rules: " + (www.error ?? "empty"));
                yield break;
            }
            CNRCompatibility.ApplyManifest(www.text);
            try
            {
                string dir = Path.GetDirectoryName(CNRCompatibility.CachePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(CNRCompatibility.CachePath, www.text);
            }
            catch (Exception ex) { ModEntry.Log("Compatibility cache write error: " + ex.Message); }
        }

        void OnGUI()
        {
            if (string.IsNullOrEmpty(_blockReason)) return;
            GUI.depth = -200;
            if (_box == null)
            {
                _box = new GUIStyle(GUI.skin.box);
                _box.alignment = TextAnchor.MiddleCenter;
                _box.wordWrap = true;
                _box.fontSize = Mathf.Max(15, Screen.height / 48);
                _button = new GUIStyle(GUI.skin.button);
                _button.fontSize = Mathf.Max(14, Screen.height / 52);
            }
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            float w = Mathf.Min(620f, Screen.width * 0.82f);
            float h = Mathf.Min(250f, Screen.height * 0.42f);
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            GUI.Box(new Rect(x, y, w, h), "MULTIPLAYER COMPATIBILITY\n\n" + _blockReason, _box);
            float bw = _blockCanOpenManager ? (w - 30f) * 0.5f : (w - 20f);
            if (_blockCanOpenManager && GUI.Button(new Rect(x + 10f, y + h - 50f, bw, 38f), "Open Mod Manager", _button)) OpenModManager();
            float closeX = _blockCanOpenManager ? x + 20f + bw : x + 10f;
            if (GUI.Button(new Rect(closeX, y + h - 50f, bw, 38f), "Close", _button)) _blockReason = "";
        }

        private void OpenModManager()
        {
            try
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch (ReflectionTypeLoadException rtle) { types = rtle.Types; }
                    if (types == null) continue;
                    for (int i = 0; i < types.Length; i++)
                    {
                        Type t = types[i];
                        if (t == null) continue;
                        MethodInfo m = t.GetMethod("OpenWindow", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                        if (m != null && t.FullName != null && t.FullName.IndexOf("ModManager", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _blockReason = "";
                            m.Invoke(null, null);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { ModEntry.Log("OpenModManager error: " + ex.Message); }
        }
    }

    public class CNRRoomJoinGuard : MonoBehaviour
    {
        public CNRPreJoinGate Gate;
        public CNRRoomInfo Original;

        void OnPress(bool pressed)
        {
            if (pressed && Original != null) Original.enabled = false;
        }

        void OnClick()
        {
            RoomInfo room = Original != null ? Original.mRoomInfo : null;
            if (Gate != null) Gate.JoinRoomPreflight(room);
            StartCoroutine(Reenable());
        }

        private IEnumerator Reenable()
        {
            yield return null;
            if (Original != null) Original.enabled = true;
        }
    }

    public class CNRMapSelectJoinGuard : MonoBehaviour
    {
        public CNRPreJoinGate Gate;
        public MapSelectButtonEvent Original;
        public string Action;

        void OnPress(bool pressed)
        {
            if (pressed && Original != null) Original.enabled = false;
        }

        void OnClick()
        {
            if (Gate != null)
            {
                if (Action == "WWStart") Gate.CreateRoomWithCompatibility();
                else if (Action == "WWQuickStart") Gate.QuickStartPreflight();
            }
            StartCoroutine(Reenable());
        }

        private IEnumerator Reenable()
        {
            yield return null;
            if (Original != null) Original.enabled = true;
        }
    }
}
