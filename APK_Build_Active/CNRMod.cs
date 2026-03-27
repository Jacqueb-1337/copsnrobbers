using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;

namespace CNRMods
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ENTRY POINT — MainMenuDirector.LoadMods() looks for CNRMods.ModEntry.Load()
    // ══════════════════════════════════════════════════════════════════════════
    public class ModEntry
    {
        private const string LogPath    = "/storage/emulated/0/CNRMods/cnrmod.log";
        private const string ConfigPath = "/storage/emulated/0/CNRMods/server.cfg";

        // Config values (read once in Load())
        public static string ServerIp      = "";
        public static int    ServerPort    = 5055;
        public static string AppId         = "CNRLan";
        public static string MapUrl        = "";
        public static string ModVersion    = "2.0.7";
        public static bool   KickNoMod     = true;
        public static string WebUrl        = "";    // http://<host>:1337 for node server; derived from SERVER_IP if not set
        public static string EconomyUrl    = "";    // https://<host>/economy  for PHP economy API
        public static bool   IsMaster      = false;  // set by RedirectHook.OnEnteredRoom so MapLoader can pick team spawn

        // ── CNRMod binary version (hardcoded; separate from the kick-threshold in server.cfg) ─────
        public const  string Version = "2.0.7";

        // ── Mod version registry — every loaded DLL registers itself here ──────────────────────────
        // External mods call RegisterMod(name, version) via reflection on ModEntry.
        public static Dictionary<string, string> RegisteredMods = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterMod(string name, string version)
        {
            if (string.IsNullOrEmpty(name)) return;
            RegisteredMods[name] = version ?? "?";
            Log("Mod registered: " + name + " v" + (version ?? "?"));
        }

        // Returns the registered version string for the named mod, or null if not registered.
        public static string GetModVersion(string name)
        {
            string v;
            return RegisteredMods.TryGetValue(name, out v) ? v : null;
        }

        private static bool _loaded = false;

        public static void Load()
        {
            if (_loaded) { Log("CNRMod: already loaded, skipping"); return; }
            _loaded = true;
            RegisterMod("CNRMod", Version);
            Log("=== CNRMod Load() v" + Version + " ===");
            try
            {
                ReadConfig();

                var go = new GameObject("CNRMod_Root");
                go.AddComponent<RedirectHook>();
                go.AddComponent<CustomMapsHook>();
                go.AddComponent<MapLoader>();
                go.AddComponent<ContentManager>();
                go.AddComponent<EconomyHook>();
                GameObject.DontDestroyOnLoad(go);

                Log("Mod root created.  IP=" + (ServerIp != "" ? ServerIp : "(none)") +
                    "  MOD_VERSION=" + ModVersion + "  KICK_NO_MOD=" + KickNoMod);

            // Clear any stale custom-map prefs from a previous session that ended
            // without a proper OnLeftRoom (crash, force-close, etc.).
            PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
            PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
            PlayerPrefs.Save();
            Log("Startup: cleared stale map prefs");
            }
            catch (Exception ex) { Log("Load() error: " + ex); }

            LoadExternalMods();
        }

        // Scan /sdcard/CNRMods/ for any .dll other than CNRMod.dll and call its public
        // static Load() — this is how CNRSettingsMod.dll and others get initialized.
        private static void LoadExternalMods()
        {
            const string dir = "/storage/emulated/0/CNRMods";
            try
            {
                string[] files = System.IO.Directory.GetFiles(dir, "*.dll");
                foreach (string path in files)
                {
                    string name = System.IO.Path.GetFileName(path);
                    if (name.Equals("CNRMod.dll", StringComparison.OrdinalIgnoreCase)) continue;
                    try
                    {
                        Log("LoadExternalMods: loading " + name);
                        byte[] data = System.IO.File.ReadAllBytes(path);
                        System.Reflection.Assembly asm = System.Reflection.Assembly.Load(data);
                        bool found = false;
                        foreach (Type t in asm.GetTypes())
                        {
                            System.Reflection.MethodInfo m = t.GetMethod("Load",
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.Static,
                                null, Type.EmptyTypes, null);
                            if (m != null) { m.Invoke(null, null); found = true; break; }
                        }
                        if (!found) Log("LoadExternalMods: no Load() in " + name);
                    }
                    catch (Exception ex2) { Log("LoadExternalMods: error in " + name + ": " + ex2.Message); }
                }
            }
            catch (Exception ex) { Log("LoadExternalMods: " + ex.Message); }
        }

        private static void ReadConfig()
        {
            if (!File.Exists(ConfigPath)) { Log("No server.cfg found"); return; }
            foreach (string raw in File.ReadAllLines(ConfigPath))
            {
                string line = raw.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string key = line.Substring(0, eq).Trim().ToUpperInvariant();
                string val = line.Substring(eq + 1).Trim();
                switch (key)
                {
                    case "SERVER_IP":   ServerIp   = val;   break;
                    case "SERVER_PORT": int pp; if (int.TryParse(val, out pp)) ServerPort = pp; break;
                    case "APP_ID":      AppId      = val;   break;
                    case "MAP_URL":     MapUrl     = val;   break;
                    case "MOD_VERSION": ModVersion = val;   break;
                    case "KICK_NO_MOD":   KickNoMod   = val.ToLower() != "false" && val != "0"; break;
                    case "WEB_URL":       WebUrl      = val; break;
                    case "ECONOMY_URL":   EconomyUrl  = val; break;
                }
            }
            Log("Config: IP=" + ServerIp + "  PORT=" + ServerPort + "  MAP_URL=" + MapUrl +
                "  VERSION=" + ModVersion + "  KICK=" + KickNoMod);
            if (string.IsNullOrEmpty(WebUrl) && !string.IsNullOrEmpty(ServerIp))
                WebUrl = "http://" + ServerIp + ":1337";
            Log("WebUrl=" + (WebUrl != "" ? WebUrl : "(not set)"));
            // Economy URL: hardcoded host unless overridden by ECONOMY_URL in server.cfg
            if (string.IsNullOrEmpty(EconomyUrl))
                EconomyUrl = "https://play.jacqueb.me/economy";
            Log("EconomyUrl=" + EconomyUrl);
        }

        public static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); }
            catch { }
            try { Debug.Log("[CNRMod] " + msg); } catch { }
        }

        // port 1337 is plain HTTP — strip accidental https://
        public static string SanitizeUrl(string url)
        {
            if (url != null && url.StartsWith("https://") && url.Contains(":1337"))
                url = "http://" + url.Substring(8);
            return url;
        }

        public static string ParseJsonStringValue(string json, string key)
        {
            try
            {
                string k = "\"" + key + "\":";
                int ki = json.IndexOf(k);
                if (ki < 0) return null;
                int vi = json.IndexOf('"', ki + k.Length);
                if (vi < 0) return null;
                int ei = json.IndexOf('"', vi + 1);
                if (ei < 0) return null;
                return json.Substring(vi + 1, ei - vi - 1).Replace("\\n", "").Replace("\\/", "/");
            }
            catch { return null; }
        }

        // Parses both "key":123 and "key":"123"
        public static string ParseJsonValue(string json, string key)
        {
            try
            {
                string k = "\"" + key + "\":";
                int ki = json.IndexOf(k);
                if (ki < 0) return null;
                int vi = ki + k.Length;
                // skip whitespace
                while (vi < json.Length && json[vi] == ' ') vi++;
                if (vi >= json.Length) return null;
                if (json[vi] == '"')
                {
                    int ei = json.IndexOf('"', vi + 1);
                    if (ei < 0) return null;
                    return json.Substring(vi + 1, ei - vi - 1).Replace("\\/", "/");
                }
                // unquoted value (number, bool, null)
                int end = vi;
                while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ']') end++;
                return json.Substring(vi, end - vi).Trim();
            }
            catch { return null; }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  REDIRECT HOOK — Photon TCP redirect + room poll + map broadcast + kick
    // ══════════════════════════════════════════════════════════════════════════
    public class RedirectHook : MonoBehaviour
    {
        // Overlay
        private string _overlayMsg   = null;
        private float  _overlayAlpha = 0f;

        // Room state
        private bool  _inRoom   = false;
        private bool  _isMaster = false;
        private float _pollTimer = 0f;
        private const float PollInterval  = 1.0f;
        private const float KickGraceSecs = 5.0f;
        private readonly Dictionary<int, float> _pendingVerify = new Dictionary<int, float>();

        // Cached Photon type
        private static Type _pnt = null;

        private static readonly string[] ConnectScenes = { "MultiplayerSelect", "CNRConnectMenu" };

        private void Awake()  { Application.runInBackground = true; }

        private static readonly string[] GameScenes =
            { "FreeRun3_1", "FreeRun4_1", "FreeRun5_1", "FreeRun6_1", "FreeRun7_1",
              "FreeRun8_1", "FreeRun9_1", "FreeRun10_1", "FreeRun11_1", "FreeRun12_1",
              "FreeRun13_1", "FreeRun14_1", "FreeRun15_1", "CRScene1" };

        private void OnLevelWasLoaded(int level)
        {
            string scene = Application.loadedLevelName;
            ModEntry.Log("Scene: " + scene);
            _pollDebugCount = 0;  // fresh diagnostics each scene

            // Proactively flush map state when navigating away from a game scene while
            // still marked as in-room. This avoids the up-to-1 s polling race where
            // the user could create a new vanilla room before PollRoomState fires.
            if (_inRoom && Array.IndexOf(GameScenes, scene) < 0)
            {
                ModEntry.Log("Scene change away from game while in room — flushing map state early");
                _inRoom = false;
                ModEntry.IsMaster = false;
                _pendingVerify.Clear();
                PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
                PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
                PlayerPrefs.Save();
            }

            if (Array.IndexOf(ConnectScenes, scene) >= 0 && ModEntry.ServerIp != "")
            {
                ModEntry.Log("Connect scene — starting LAN redirect");
                StartCoroutine(RedirectCoroutine());
            }
        }

        private void Update()
        {
            _pollTimer -= Time.deltaTime;
            if (_pollTimer > 0f) return;
            _pollTimer = PollInterval;
            PollRoomState();
        }

        // ── Room state machine ────────────────────────────────────────────────
        private int _pollDebugCount = 0;  // limit verbose poll logging
        private void PollRoomState()
        {
            try
            {
                Type pnt = GetPhotonNetType();
                if (pnt == null) { if (_pollDebugCount++ < 3) ModEntry.Log("PollRoomState: PhotonNetwork type not found"); return; }

                bool nowInRoom = GetStaticBool(pnt, "inRoom");
                bool nowMaster = nowInRoom && GetStaticBool(pnt, "isMasterClient");

                // Log first 15 polls so we can see what Photon is returning
                if (_pollDebugCount < 15)
                {
                    _pollDebugCount++;
                    ModEntry.Log("Poll[" + _pollDebugCount + "] inRoom=" + nowInRoom + " isMaster=" + nowMaster + " scene=" + Application.loadedLevelName);
                }

                if (!_inRoom && nowInRoom)       OnEnteredRoom(pnt, nowMaster);
                else if (_inRoom && !nowInRoom)  OnLeftRoom();

                _inRoom   = nowInRoom;
                _isMaster = nowMaster;

                if (_inRoom && _isMaster && ModEntry.KickNoMod)
                    CheckKickPlayers(pnt);
            }
            catch (Exception ex) { ModEntry.Log("PollRoomState error: " + ex.Message); }
        }

        private void OnEnteredRoom(Type pnt, bool asMaster)
        {
            _pollDebugCount = 999; // stop verbose poll logging once we're in a room
            ModEntry.IsMaster = asMaster;
            ModEntry.Log("Entered room (asMaster=" + asMaster + ")");
            _pendingVerify.Clear();

            string roomName = GetRoomName(pnt);
            ModEntry.Log("Room: " + (roomName ?? "(unknown)"));

            SetRoomProp(pnt, "CNR_MOD_VERSION", ModEntry.ModVersion);

            if (asMaster)
            {
                string url = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "");
                if (!string.IsNullOrEmpty(url))
                {
                    SetRoomProp(pnt, "CNR_MAP_URL", url);
                    // Tell node server — clients will query it on join
                    if (!string.IsNullOrEmpty(ModEntry.WebUrl) && !string.IsNullOrEmpty(roomName))
                        StartCoroutine(PostRoomToServer(roomName, url));
                    // Download for local spawn
                    StartCoroutine(DownloadMap(url));
                    ModEntry.Log("Master: registered map " + url);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(ModEntry.WebUrl) && !string.IsNullOrEmpty(roomName))
                {
                    // Fetch URL from node server, then cache + MapLoader polling will spawn it
                    StartCoroutine(FetchAndCacheMap(roomName));
                }
                else
                {
                    // Fallback: read Photon room prop directly
                    string roomUrl = GetRoomPropStr(pnt, "CNR_MAP_URL");
                    if (!string.IsNullOrEmpty(roomUrl)) StartCoroutine(DownloadMap(roomUrl));
                }
            }
        }

        private void OnLeftRoom()
        {
            ModEntry.Log("Left room");
            _pendingVerify.Clear();
            _pollDebugCount = 0; // re-enable verbose logging for next room
            // Clear custom map state so a subsequent vanilla room doesn't load stale map data.
            PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
            PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
            PlayerPrefs.Save();
        }

        // ── Room name / node server helpers ────────────────────────────
        private static string GetRoomName(Type pnt)
        {
            try
            {
                PropertyInfo rp = pnt.GetProperty("room", BindingFlags.Static | BindingFlags.Public);
                if (rp == null) return null;
                object room = rp.GetValue(null, null);
                if (room == null) return null;
                // Room.Name / room.name
                PropertyInfo np = room.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
                if (np == null) np = room.GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public);
                if (np != null) return np.GetValue(room, null) as string;
            }
            catch (Exception ex) { ModEntry.Log("GetRoomName error: " + ex.Message); }
            return null;
        }

        // POST {room, mapUrl} to node server so clients can query it
        private IEnumerator PostRoomToServer(string roomName, string url)
        {
            string body = "{\"room\":\"" + EscapeJson(roomName) + "\",\"mapUrl\":\"" + EscapeJson(url) + "\"}";            byte[] data = System.Text.Encoding.UTF8.GetBytes(body);
            var h = new System.Collections.Hashtable();
            h["Content-Type"] = "application/json";
            ModEntry.Log("PostRoom -> " + ModEntry.WebUrl + "/rooms");
            var www = new WWW(ModEntry.WebUrl + "/rooms", data, h);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) ModEntry.Log("PostRoom error: " + www.error);
            else ModEntry.Log("PostRoom OK: " + www.text);
        }

        // GET /rooms/<roomName> from node server, cache the map URL, start download
        private IEnumerator FetchAndCacheMap(string roomName)
        {
            string fetchUrl = ModEntry.SanitizeUrl(ModEntry.WebUrl + "/rooms/" + Uri.EscapeDataString(roomName));
            ModEntry.Log("Client: GET " + fetchUrl);
            var www = new WWW(fetchUrl);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) { ModEntry.Log("FetchRoom error: " + www.error); yield break; }
            string mapUrl = ModEntry.ParseJsonStringValue(www.text, "mapUrl");
            if (string.IsNullOrEmpty(mapUrl)) { ModEntry.Log("FetchRoom: no mapUrl in: " + www.text); yield break; }
            ModEntry.Log("Client: got mapUrl=" + mapUrl);
            PlayerPrefs.SetString("CNRMod_ActiveMapURL", mapUrl);
            PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
            PlayerPrefs.Save();
            StartCoroutine(DownloadMap(mapUrl));
        }

        private static string EscapeJson(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }

        // ── Kick ──────────────────────────────────────────────────────────────
        private void CheckKickPlayers(Type pnt)
        {
            try
            {
                PropertyInfo op = pnt.GetProperty("otherPlayers", BindingFlags.Static | BindingFlags.Public);
                if (op == null) return;
                var others = op.GetValue(null, null) as System.Array;
                if (others == null) return;

                var current = new HashSet<int>();
                foreach (object player in others)
                {
                    if (player == null) continue;
                    int    pid = GetIntProp(player, "ID");
                    string ver = GetPlayerCustomProp(player, "CNR_MOD_VERSION");
                    current.Add(pid);

                    if (!_pendingVerify.ContainsKey(pid))
                    {
                        if (string.IsNullOrEmpty(ver))
                        {
                            _pendingVerify[pid] = Time.time;
                            ModEntry.Log("Player " + pid + " grace window started");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ver))
                        {
                            _pendingVerify.Remove(pid);
                            if (!VersionOk(ver))
                            {
                                ModEntry.Log("Kicking player " + pid + ": version mismatch '" + ver + "'");
                                KickPlayer(pnt, player);
                            }
                        }
                        else if (Time.time - _pendingVerify[pid] > KickGraceSecs)
                        {
                            ModEntry.Log("Kicking player " + pid + ": no mod version after " + KickGraceSecs + "s");
                            _pendingVerify.Remove(pid);
                            KickPlayer(pnt, player);
                        }
                    }
                }

                var gone = new List<int>();
                foreach (int pid in _pendingVerify.Keys)
                    if (!current.Contains(pid)) gone.Add(pid);
                foreach (int pid in gone) _pendingVerify.Remove(pid);
            }
            catch (Exception ex) { ModEntry.Log("CheckKickPlayers error: " + ex.Message); }
        }

        private void KickPlayer(Type pnt, object player)
        {
            try
            {
                MethodInfo m = pnt.GetMethod("CloseConnection",
                    BindingFlags.Static | BindingFlags.Public,
                    null, new Type[] { player.GetType() }, null);
                if (m != null) m.Invoke(null, new object[] { player });
            }
            catch (Exception ex) { ModEntry.Log("KickPlayer error: " + ex.Message); }
        }

        private bool VersionOk(string other)
        {
            try
            {
                int a = int.Parse(ModEntry.ModVersion.Trim().Split('.')[0]);
                int b = int.Parse(other.Trim().Split('.')[0]);
                return a == b;
            }
            catch { return true; }
        }

        // ── Map download ──────────────────────────────────────────────────────
        private IEnumerator DownloadMap(string url)
        {
            url = ModEntry.SanitizeUrl(url);
            ModEntry.Log("DownloadMap: " + url);
            var www = new WWW(url);
            yield return www;

            if (!string.IsNullOrEmpty(www.error)) { ModEntry.Log("DownloadMap error: " + www.error); yield break; }
            string json = www.text;
            if (string.IsNullOrEmpty(json)) { ModEntry.Log("DownloadMap: empty response"); yield break; }

            try
            {
                const string dir = "/storage/emulated/0/CNRMods/";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(dir + "custom_map_cache.json", json);
                PlayerPrefs.SetInt("CNRMod_MapCacheReady", 1);
                PlayerPrefs.Save();
                ModEntry.Log("Map cached (" + json.Length + " bytes)");
            }
            catch (Exception ex) { ModEntry.Log("DownloadMap save error: " + ex.Message); }
        }

        // ── Photon prop helpers ───────────────────────────────────────────────
        private void SetRoomProp(Type pnt, string key, string value)
        {
            try
            {
                PropertyInfo rp = pnt.GetProperty("room", BindingFlags.Static | BindingFlags.Public);
                if (rp == null) return;
                object room = rp.GetValue(null, null);
                if (room == null) return;
                var ht = new System.Collections.Hashtable();
                ht[key] = value;
                MethodInfo m = room.GetType().GetMethod("SetCustomProperties",
                    new Type[] { typeof(System.Collections.Hashtable) });
                if (m != null) m.Invoke(room, new object[] { ht });
            }
            catch (Exception ex) { ModEntry.Log("SetRoomProp error: " + ex.Message); }
        }

        private void SetPlayerProp(Type pnt, string key, string value)
        {
            try
            {
                PropertyInfo pp = pnt.GetProperty("player", BindingFlags.Static | BindingFlags.Public);
                if (pp == null) return;
                object player = pp.GetValue(null, null);
                if (player == null) return;
                var ht = new System.Collections.Hashtable();
                ht[key] = value;
                MethodInfo m = player.GetType().GetMethod("SetCustomProperties",
                    new Type[] { typeof(System.Collections.Hashtable) });
                if (m != null) m.Invoke(player, new object[] { ht });
            }
            catch (Exception ex) { ModEntry.Log("SetPlayerProp error: " + ex.Message); }
        }

        private string GetRoomPropStr(Type pnt, string key)
        {
            try
            {
                PropertyInfo rp = pnt.GetProperty("room", BindingFlags.Static | BindingFlags.Public);
                if (rp == null) return null;
                object room = rp.GetValue(null, null);
                if (room == null) return null;
                PropertyInfo cp = room.GetType().GetProperty("customProperties",
                    BindingFlags.Instance | BindingFlags.Public);
                if (cp == null) return null;
                var ht = cp.GetValue(room, null) as System.Collections.Hashtable;
                return (ht != null && ht.ContainsKey(key)) ? ht[key] as string : null;
            }
            catch { return null; }
        }

        private string GetPlayerCustomProp(object player, string key)
        {
            try
            {
                PropertyInfo cp = player.GetType().GetProperty("customProperties",
                    BindingFlags.Instance | BindingFlags.Public);
                if (cp == null) return null;
                var ht = cp.GetValue(player, null) as System.Collections.Hashtable;
                return (ht != null && ht.ContainsKey(key)) ? ht[key] as string : null;
            }
            catch { return null; }
        }

        private int GetIntProp(object obj, string name)
        {
            try
            {
                PropertyInfo p = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                if (p != null) return (int)p.GetValue(obj, null);
            }
            catch { }
            return -1;
        }

        // ── Redirect coroutine ────────────────────────────────────────────────
        private IEnumerator RedirectCoroutine()
        {
            object settings = null;
            while (settings == null) { settings = GetPhotonServerSettings(); if (settings == null) yield return null; }

            Type t = settings.GetType();
            SetMember(t, settings, "ServerAddress", ModEntry.ServerIp);
            SetMember(t, settings, "ServerPort",    ModEntry.ServerPort);
            SetMember(t, settings, "AppID",         ModEntry.AppId);
            SetMember(t, settings, "HostType",      2);
            ModEntry.Log("Override -> " + ModEntry.ServerIp + ":" + ModEntry.ServerPort);

            CallStaticVoid("PhotonNetwork", "Disconnect");
            float timeout = 8f;
            while (timeout > 0f) { if (GetConnectionState() == 0) break; timeout -= Time.unscaledDeltaTime; yield return null; }

            SwapToTcp();
            DisableEncryption();

            ModEntry.Log("Calling ConnectUsingSettings...");
            try { CallStaticWithArg("PhotonNetwork", "ConnectUsingSettings", "v2.4"); }
            catch (Exception ex) { ModEntry.Log("ConnectUsingSettings error: " + ex.Message); yield break; }

            float connectTimeout = 30f;
            int lastState = -999;
            while (connectTimeout > 0f)
            {
                int state = GetDetailedState();
                if (state != lastState)
                {
                    ModEntry.Log("detailState=" + state + " (" + (30f - connectTimeout).ToString("F1") + "s)");
                    lastState = state;
                }
                if (state == 0)  { ShowOverlay("LAN server unreachable.\n" + ModEntry.ServerIp); yield break; }
                if (state >= 6)  { ModEntry.Log("Lobby joined!"); yield break; }
                connectTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }
            ModEntry.Log("Connection timed out");
            CallStaticVoid("PhotonNetwork", "Disconnect");
            ShowOverlay("LAN connection timed out.\n" + ModEntry.ServerIp);
        }

        // ── Overlay ───────────────────────────────────────────────────────────
        private void ShowOverlay(string msg)
        {
            ModEntry.Log("OVERLAY: " + msg);
            _overlayMsg   = msg;
            _overlayAlpha = 1f;
            StartCoroutine(FadeOverlay());
        }

        private IEnumerator FadeOverlay()
        {
            yield return new WaitForSeconds(6f);
            float ft = 4f;
            while (ft > 0f) { _overlayAlpha = ft / 4f; ft -= Time.deltaTime; yield return null; }
            _overlayMsg   = null;
            _overlayAlpha = 0f;
        }

        private void OnGUI()
        {
            if (_overlayMsg == null || _overlayAlpha <= 0f) return;
            GUI.color = new Color(0f, 0f, 0f, 0.75f * _overlayAlpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, 140f), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.35f, 0.35f, _overlayAlpha);
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize  = Mathf.Max(22, Screen.width / 22);
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap  = true;
            GUI.Label(new Rect(20f, 8f, Screen.width - 40f, 124f), "[CNR-Mod] " + _overlayMsg, style);
            GUI.color = Color.white;
        }

        // ── Photon reflection helpers ─────────────────────────────────────────
        private static Type GetPhotonNetType()
        {
            if (_pnt != null) return _pnt;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            { Type t = asm.GetType("PhotonNetwork"); if (t != null) { _pnt = t; return t; } }
            return null;
        }

        private static bool GetStaticBool(Type t, string name)
        {
            try
            {
                PropertyInfo p = t.GetProperty(name, BindingFlags.Static | BindingFlags.Public);
                if (p != null) return (bool)p.GetValue(null, null);
                FieldInfo f = t.GetField(name, BindingFlags.Static | BindingFlags.Public);
                if (f != null) return (bool)f.GetValue(null);
            }
            catch { }
            return false;
        }

        private static object GetPhotonServerSettings()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type pn = asm.GetType("PhotonNetwork");
                if (pn == null) continue;
                PropertyInfo p = pn.GetProperty("PhotonServerSettings",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null) { object r = p.GetValue(null, null); if (r != null) return r; }
                FieldInfo f = pn.GetField("PhotonServerSettings",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null) { object r = f.GetValue(null); if (r != null) return r; }
            }
            return null;
        }

        private static int GetConnectionState()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type pn = asm.GetType("PhotonNetwork");
                if (pn == null) continue;
                PropertyInfo p = pn.GetProperty("connectionState", BindingFlags.Static | BindingFlags.Public);
                if (p != null) return Convert.ToInt32(p.GetValue(null, null));
            }
            return -1;
        }

        private static int GetDetailedState()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type pn = asm.GetType("PhotonNetwork");
                if (pn == null) continue;
                PropertyInfo p = pn.GetProperty("connectionStateDetailed", BindingFlags.Static | BindingFlags.Public);
                if (p != null) return Convert.ToInt32(p.GetValue(null, null));
            }
            return -1;
        }

        private static void CallStaticVoid(string typeName, string method)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(typeName);
                if (t == null) continue;
                MethodInfo m = t.GetMethod(method, BindingFlags.Static | BindingFlags.Public,
                    null, Type.EmptyTypes, null);
                if (m != null) { m.Invoke(null, null); return; }
            }
        }

        private static void CallStaticWithArg(string typeName, string method, object arg)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(typeName);
                if (t == null) continue;
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    if (m.Name != method) continue;
                    ParameterInfo[] prms = m.GetParameters();
                    if (prms.Length != 1) continue;
                    if (!prms[0].ParameterType.IsAssignableFrom(arg.GetType())) continue;
                    m.Invoke(null, new object[] { arg });
                    return;
                }
            }
        }

        private static void DisableEncryption()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type pn = asm.GetType("PhotonNetwork");
                if (pn == null) continue;
                FieldInfo fp = pn.GetField("networkingPeer",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (fp == null) continue;
                object peer = fp.GetValue(null);
                if (peer == null) continue;
                FieldInfo fs = peer.GetType().GetField("requestSecurity",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fs != null) { fs.SetValue(peer, false); ModEntry.Log("requestSecurity=false"); return; }
            }
        }

        private static void SwapToTcp()
        {
            try
            {
                Type pnType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                { pnType = asm.GetType("PhotonNetwork"); if (pnType != null) break; }
                if (pnType == null) return;

                FieldInfo peerFieldPN = pnType.GetField("networkingPeer",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (peerFieldPN == null) return;
                object peer = peerFieldPN.GetValue(null);
                if (peer == null) return;

                FieldInfo peerBaseField = null;
                Type search = peer.GetType();
                while (search != null)
                {
                    peerBaseField = search.GetField("peerBase",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (peerBaseField != null) break;
                    search = search.BaseType;
                }
                if (peerBaseField == null) return;

                Type tpeerType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    tpeerType = asm.GetType("ExitGames.Client.Photon.TPeer");
                    if (tpeerType == null) tpeerType = asm.GetType("TPeer");
                    if (tpeerType != null) break;
                }
                if (tpeerType == null) { ModEntry.Log("SwapToTcp: TPeer not found"); return; }

                object newTPeer = Activator.CreateInstance(tpeerType, true);

                Type bs = tpeerType;
                while (bs != null)
                {
                    FieldInfo upf = bs.GetField("usedProtocol",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (upf != null) { upf.SetValue(newTPeer, Enum.ToObject(upf.FieldType, (byte)1)); break; }
                    bs = bs.BaseType;
                }

                bool listenerSet = false;
                Type ls = tpeerType;
                while (ls != null && !listenerSet)
                {
                    PropertyInfo lp = ls.GetProperty("Listener",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (lp != null)
                    {
                        MethodInfo setter = lp.GetSetMethod(true);
                        if (setter != null) { setter.Invoke(newTPeer, new object[] { peer }); listenerSet = true; }
                        break;
                    }
                    ls = ls.BaseType;
                }
                if (!listenerSet)
                {
                    string[] cands = new string[] { "<Listener>k__BackingField", "listener", "_listener", "Listener" };
                    Type bfs = tpeerType;
                    while (bfs != null && !listenerSet)
                    {
                        foreach (string fn in cands)
                        {
                            FieldInfo bf = bfs.GetField(fn,
                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                            if (bf != null) { bf.SetValue(newTPeer, peer); listenerSet = true; break; }
                        }
                        if (!listenerSet) bfs = bfs.BaseType;
                    }
                }

                peerBaseField.SetValue(peer, newTPeer);
                ModEntry.Log("SwapToTcp: done");
            }
            catch (Exception ex) { ModEntry.Log("SwapToTcp error: " + ex.Message); }
        }

        private static void SetMember(Type t, object inst, string name, object val)
        {
            FieldInfo f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
            {
                object v = f.FieldType.IsEnum
                    ? Enum.ToObject(f.FieldType, Convert.ChangeType(val, Enum.GetUnderlyingType(f.FieldType)))
                    : Convert.ChangeType(val, f.FieldType);
                f.SetValue(inst, v);
                ModEntry.Log("  " + name + "=" + v);
                return;
            }
            PropertyInfo p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite) { p.SetValue(inst, Convert.ChangeType(val, p.PropertyType), null); ModEntry.Log("  " + name + "=" + val); }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CUSTOM MAPS HOOK — extended map selector with per-slot URL input box
    // ══════════════════════════════════════════════════════════════════════════
    public class CustomMapsHook : MonoBehaviour
    {
        static readonly string[] STANDARD_MAPS =
        {
            "FreeRun3_1",  "FreeRun4_1",  "FreeRun5_1",  "FreeRun6_1",  "FreeRun7_1",
            "FreeRun8_1",  "FreeRun9_1",  "FreeRun10_1", "FreeRun11_1", "FreeRun12_1",
        };

        static readonly string[] CUSTOM_MAPS = { "FreeRun13_1", "FreeRun14_1", "FreeRun15_1" };

        static readonly Dictionary<string, string> CUSTOM_NAMES = new Dictionary<string, string>
        {
            { "FreeRun13_1", "[MOD] Map 11" },
            { "FreeRun14_1", "[MOD] Map 12" },
            { "FreeRun15_1", "[MOD] Map 13" },
        };

        static readonly Dictionary<string, string> CUSTOM_SCENE_LOAD = new Dictionary<string, string>
        {
            { "FreeRun13_1", "FreeRun3_1" },
            { "FreeRun14_1", "FreeRun5_1" },
            { "FreeRun15_1", "FreeRun8_1" },
        };

        string[] _allMaps;
        bool     _hooked      = false;
        bool     _hookAttempted = false;  // true after first attempt — prevents spam on 0-hook result
        MSD_SubSceneInWorldWide _lastSubScene = (MSD_SubSceneInWorldWide)(-1);
        int      _virtualIdx  = 0;
        string   _activeSlot  = "";   // non-empty → user custom slot, empty → standard/official
        string   _urlInput    = "";
        bool     _activeIsOfficial = false;  // true when selected map is an official server map
        Font     _gameFont    = null;

        // Find the game's NGUI dynamic font via any live UILabel.
        Font GetGameFont()
        {
            if (_gameFont != null) return _gameFont;
            UILabel[] lbls = (UILabel[])FindObjectsOfType(typeof(UILabel));
            foreach (UILabel lbl in lbls)
                if (lbl != null && lbl.font != null && lbl.font.dynamicFont != null)
                { _gameFont = lbl.font.dynamicFont; break; }
            return _gameFont;
        }

        void Awake()
        {
            // Initial map list (no official maps yet — rebuilt in HookButtons after ContentManager loads)
            var list = new List<string>(STANDARD_MAPS);
            list.AddRange(CUSTOM_MAPS);
            _allMaps = list.ToArray();
        }

        // Rebuild _allMaps with current official maps from ContentManager injected between
        // standard maps and the user's three custom URL slots.
        void RebuildMapList()
        {
            var list = new List<string>(STANDARD_MAPS);
            foreach (var om in ContentManager.OfficialMaps)
                list.Add("OFFICIAL_" + om.Id);
            list.AddRange(CUSTOM_MAPS);
            _allMaps = list.ToArray();
            ModEntry.Log("CustomMaps: map list rebuilt — " + STANDARD_MAPS.Length + " std + "
                + ContentManager.OfficialMaps.Length + " official + " + CUSTOM_MAPS.Length + " custom = " + _allMaps.Length);
        }

        void OnLevelWasLoaded(int level)
        {
            _hooked           = false;
            _hookAttempted    = false;
            _lastSubScene     = (MSD_SubSceneInWorldWide)(-1);
            _activeSlot       = "";
            _urlInput         = "";
            _activeIsOfficial = false;
            // Clear the persisted label so it doesn't reappear on the next room-create visit
            PlayerPrefs.SetString("CNRMod_CustomMapName", "");
            PlayerPrefs.Save();
        }

        void Update()
        {
            if (_hooked) return;
            if (Application.loadedLevelName != "MultiplayerSelect") return;
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;
            if (msd.mCurWWSubScene != _lastSubScene)
            {
                _lastSubScene = msd.mCurWWSubScene;
                ModEntry.Log("CustomMaps subScene=" + msd.mCurWWSubScene);
            }
            if (msd.mCurWWSubScene != MSD_SubSceneInWorldWide.RoomCreate) return;
            if (_hookAttempted) return;  // already tried this sub-scene entry, don't spam
            _hookAttempted = true;
            StartCoroutine(HookButtons());
            _hooked = true;
        }

        IEnumerator HookButtons()
        {
            yield return new WaitForSeconds(0.1f);

            // Wait up to 5s for ContentManager to finish its manifest fetch
            float waited = 0f;
            while (!ContentManager.Ready && waited < 5f)
            { yield return new WaitForSeconds(0.25f); waited += 0.25f; }

            RebuildMapList();

            var all = (MonoBehaviour[])Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
            int hooked = 0;
            foreach (var comp in all)
            {
                if (comp.GetType().Name != "MapSelectButtonEvent") continue;
                FieldInfo btnField = comp.GetType().GetField("buttonName",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (btnField == null) continue;
                string btnName = btnField.GetValue(comp).ToString();
                if (btnName == "WWMapNext" || btnName == "WWMapPre")
                {
                    object nilVal = Enum.Parse(btnField.FieldType, "Nil");
                    btnField.SetValue(comp, nilVal);
                    var nav = comp.gameObject.AddComponent<MapNavButton>();
                    nav.isNext = (btnName == "WWMapNext");
                    nav.hook   = this;
                    hooked++;
                    ModEntry.Log("Hooked button: " + btnName);
                }
            }
            // Don't reset _hooked to false on 0 — that causes infinite spam.
            if (hooked > 0)
            {
                _hooked = true;
                var msd = MultiplayerSelectDirector.mInstance;
                if (msd != null)
                {
                    int idx = Array.IndexOf(STANDARD_MAPS, msd.mCurWWMapSelect);
                    if (idx >= 0) _virtualIdx = idx;
                }
            }
            ModEntry.Log("HookButtons done: " + hooked + " hooked");
        }

        void OnJoinedRoom()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;
            string scene = msd.mCurWWMapSelect;
            if (CUSTOM_NAMES.ContainsKey(scene))
                StartCoroutine(LoadLevelWatchdog(scene));
        }

        IEnumerator LoadLevelWatchdog(string scene)
        {
            yield return new WaitForSeconds(5f);
            if (Application.loadedLevelName == "MultiplayerSelect")
            {
                ModEntry.Log("Watchdog: redirecting from " + scene + " to FreeRun3_1");
                var msd = MultiplayerSelectDirector.mInstance;
                if (msd != null) { msd.mCurWWMapSelect = "FreeRun3_1"; Application.LoadLevel("FreeRun3_1"); }
            }
        }

        public void OnNextMap()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;
            _virtualIdx = (_virtualIdx >= _allMaps.Length - 1) ? 0 : _virtualIdx + 1;
            ApplyMap(msd, _virtualIdx);
        }

        public void OnPreMap()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;
            _virtualIdx = (_virtualIdx <= 0) ? _allMaps.Length - 1 : _virtualIdx - 1;
            ApplyMap(msd, _virtualIdx);
        }

        void ApplyMap(MultiplayerSelectDirector msd, int idx)
        {
            string scene  = _allMaps[idx];
            int    stdIdx = Array.IndexOf(STANDARD_MAPS, scene);

            if (stdIdx >= 0)
            {
                // ─ Standard map ─
                PlayerPrefs.SetString("CNRMod_CustomMapName", "");
                PlayerPrefs.SetString("CNRMod_ActiveMapURL",  "");
                PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
                PlayerPrefs.DeleteKey("CNRMod_DonorScene");
                _activeSlot       = "";
                _urlInput         = "";
                _activeIsOfficial = false;
                msd.mCurWWMapSelect = scene;
                msd.mWWMapUITexture.mainTexture = (Texture)(object)msd.mWWMapTexture[stdIdx];
                msd.mWWMapUITexture.MarkAsChanged();
                msd.WWResetModeCheckBox();
            }
            else if (scene.StartsWith("OFFICIAL_"))
            {
                // ─ Official server-provided map ─
                string omId = scene.Substring("OFFICIAL_".Length);
                OfficialMapEntry om = null;
                foreach (var m in ContentManager.OfficialMaps)
                    if (m.Id == omId) { om = m; break; }
                if (om == null) { ModEntry.Log("ApplyMap: official map not found: " + omId); return; }

                // Determine donor scene: read from pre-cached JSON's "donor" field if available.
                // Admin panel no longer stores base_scene; the JSON itself is authoritative.
                string[] validDonors = new string[]{"FreeRun3_1","FreeRun5_1","FreeRun8_1"};
                string   loadScene   = "FreeRun3_1";   // safe default
                string   cachePath   = ContentManager.MapCacheDir + om.Id + ".json";
                if (File.Exists(cachePath))
                {
                    try
                    {
                        string cachedJson = File.ReadAllText(cachePath);
                        string donor = ModEntry.ParseJsonStringValue(cachedJson, "donor");
                        if (!string.IsNullOrEmpty(donor) && Array.IndexOf(validDonors, donor) >= 0)
                            loadScene = donor;
                    }
                    catch { }
                }

                msd.mCurWWMapSelect = loadScene;

                // Use downloaded thumbnail if available; fall back to last standard map texture
                Texture2D thumb = ContentManager.GetMapThumbnail(om.Id);
                msd.mWWMapUITexture.mainTexture = thumb != null
                    ? (Texture)(object)thumb
                    : (Texture)(object)msd.mWWMapTexture[STANDARD_MAPS.Length - 1];
                msd.mWWMapUITexture.MarkAsChanged();
                msd.WWResetModeCheckBox();

                PlayerPrefs.SetString("CNRMod_CustomMapName", om.Name);
                _activeSlot       = "";   // no URL input for official maps
                _urlInput         = "";
                _activeIsOfficial = true;

                // Use pre-cached JSON if available; otherwise MapLoader will download via ActiveMapURL
                string stdCachePath = "/storage/emulated/0/CNRMods/custom_map_cache.json";
                if (File.Exists(cachePath))
                {
                    try
                    {
                        File.Copy(cachePath, stdCachePath, true);
                        PlayerPrefs.SetInt("CNRMod_MapCacheReady", 1);
                        PlayerPrefs.SetString("CNRMod_DonorScene", loadScene);
                        PlayerPrefs.SetString("CNRMod_ActiveMapURL", "");
                        ModEntry.Log("Official map '" + om.Name + "': used pre-cached JSON (donor=" + loadScene + ")");
                    }
                    catch (Exception copyEx)
                    {
                        ModEntry.Log("Official map copy error: " + copyEx.Message);
                        PlayerPrefs.SetString("CNRMod_ActiveMapURL", om.Url);
                        PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
                    }
                }
                else
                {
                    PlayerPrefs.SetString("CNRMod_ActiveMapURL", om.Url);
                    PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
                    PlayerPrefs.DeleteKey("CNRMod_DonorScene");
                    StartCoroutine(FetchDonor(om.Url));
                    ModEntry.Log("Official map '" + om.Name + "': URL queued for download (no cache yet)");
                }
                PlayerPrefs.Save();
            }
            else
            {
                // ─ User custom map slot ─
                string[] validDonors = new string[]{"FreeRun3_1","FreeRun5_1","FreeRun8_1"};
                string donorPref = PlayerPrefs.GetString("CNRMod_DonorScene", "");
                string loadScene = (Array.IndexOf(validDonors, donorPref) >= 0) ? donorPref
                    : (CUSTOM_SCENE_LOAD.ContainsKey(scene) ? CUSTOM_SCENE_LOAD[scene] : "FreeRun3_1");
                msd.mCurWWMapSelect = loadScene;
                msd.mWWMapUITexture.mainTexture = (Texture)(object)msd.mWWMapTexture[STANDARD_MAPS.Length - 1];
                msd.mWWMapUITexture.MarkAsChanged();
                msd.WWResetModeCheckBox();

                PlayerPrefs.SetString("CNRMod_CustomMapName", CUSTOM_NAMES[scene]);
                _activeSlot       = scene;
                _activeIsOfficial = false;
                _urlInput         = PlayerPrefs.GetString("CNRMod_MapURL_" + scene, "");
                PlayerPrefs.SetString("CNRMod_ActiveMapURL", _urlInput);
                PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
                PlayerPrefs.Save();
                if (!string.IsNullOrEmpty(_urlInput))
                    StartCoroutine(FetchDonor(_urlInput));
            }
            ModEntry.Log("Map -> " + scene + " (loads: " + msd.mCurWWMapSelect + ")");
        }

        IEnumerator FetchDonor(string url)
        {
            if (string.IsNullOrEmpty(url)) yield break;
            url = ModEntry.SanitizeUrl(url);
            var www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) { ModEntry.Log("FetchDonor error: " + www.error); yield break; }
            string donor = ModEntry.ParseJsonStringValue(www.text, "donor");
            string[] validDonors = new string[]{"FreeRun3_1","FreeRun5_1","FreeRun8_1"};
            if (!string.IsNullOrEmpty(donor) && Array.IndexOf(validDonors, donor) >= 0)
            {
                PlayerPrefs.SetString("CNRMod_DonorScene", donor);
                PlayerPrefs.Save();
                // Also apply directly to the map selector so the scene is correct immediately
                var msd2 = MultiplayerSelectDirector.mInstance;
                if (msd2 != null) msd2.mCurWWMapSelect = donor;
                ModEntry.Log("FetchDonor: donor=" + donor + " applied");
            }
            else
            {
                ModEntry.Log("FetchDonor: no valid donor field in response");
            }
        }

        void OnGUI()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;
            if (msd.mCurWWSubScene != MSD_SubSceneInWorldWide.RoomCreate) return;

            string displayName = PlayerPrefs.GetString("CNRMod_CustomMapName", "");
            if (string.IsNullOrEmpty(displayName)) return;

            float sw = Screen.width;
            float sh = Screen.height;
            // Right-side panel: starts at 60% of screen width
            float bw = sw * 0.36f;
            float bx = sw * 0.62f;
            float by = sh * 0.34f;

            int bigFont = Mathf.Max(20, Mathf.RoundToInt(sh / 27f));
            int subFont = Mathf.Max(17, Mathf.RoundToInt(sh / 34f));
            Font gf = GetGameFont();

            var nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize  = bigFont;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleLeft;
            if (gf != null) nameStyle.font = gf;
            GUI.color = new Color(1f, 0.55f, 0.05f, 0.95f);
            // Draw gold name with extra letter spacing
            {
                float cx = bx; float ch = bigFont + 6f;
                for (int ci = 0; ci < displayName.Length; ci++)
                {
                    string gc = displayName[ci].ToString();
                    Vector2 csz = nameStyle.CalcSize(new GUIContent(gc));
                    GUI.Label(new Rect(cx, by, csz.x + 2f, ch), gc, nameStyle);
                    cx += csz.x + 2f; // 2px extra per character
                }
            }

            var noteStyle = new GUIStyle(GUI.skin.label);
            noteStyle.fontSize  = subFont;
            noteStyle.fontStyle = FontStyle.Normal;
            noteStyle.alignment = TextAnchor.MiddleLeft;
            if (gf != null) noteStyle.font = gf;
            string subNote = _activeIsOfficial ? "Official server map" : "Requires mod on all clients";
            GUI.color = _activeIsOfficial ? new Color(0.4f, 0.9f, 0.4f, 0.85f) : new Color(1f, 1f, 0.3f, 0.85f);
            GUI.Label(new Rect(bx, by + bigFont + 6f, bw, subFont + 4f), subNote, noteStyle);

            if (!string.IsNullOrEmpty(_activeSlot))
            {
                float uy = by + bigFont + 6f + subFont + 4f + 6f;
                var lblStyle = new GUIStyle(GUI.skin.label);
                lblStyle.fontSize  = subFont;
                lblStyle.alignment = TextAnchor.MiddleLeft;
                if (gf != null) lblStyle.font = gf;
                GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
                GUI.Label(new Rect(bx, uy, bw, subFont + 4f), "Map JSON URL:", lblStyle);
                uy += subFont + 4f;

                GUI.color = Color.white;
                var tfStyle = new GUIStyle(GUI.skin.textField);
                tfStyle.fontSize = subFont;
                if (gf != null) tfStyle.font = gf;
                float tfH = subFont + 14f;
                string newUrl = GUI.TextField(new Rect(bx, uy, bw, tfH), _urlInput, 512, tfStyle);
                if (newUrl != _urlInput)
                {
                    _urlInput = newUrl;
                    PlayerPrefs.SetString("CNRMod_MapURL_" + _activeSlot, newUrl);
                    PlayerPrefs.SetString("CNRMod_ActiveMapURL", newUrl);
                    PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
                    PlayerPrefs.Save();
                    StartCoroutine(FetchDonor(newUrl));
                }
            }

            GUI.color = Color.white;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MAP NAV BUTTON — attached to NGUI arrow buttons to intercept clicks
    // ══════════════════════════════════════════════════════════════════════════
    public class MapNavButton : MonoBehaviour
    {
        public bool isNext;
        public CustomMapsHook hook;
        void OnClick() { if (hook != null) { if (isNext) hook.OnNextMap(); else hook.OnPreMap(); } }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MAP LOADER — spawns cached JSON objects when a custom map scene loads
    // ══════════════════════════════════════════════════════════════════════════
    class MapObjData
    {
        public string  path;
        public string  mesh;
        public string  mat;
        public float[] color;
        public float[] pos;
        public float[] rot;
        public float[] size;
        public bool    collidable = true;
        public bool    isColBox   = false;
    }

    public class MapLoader : MonoBehaviour
    {
        private const string CachePath = "/storage/emulated/0/CNRMods/custom_map_cache.json";

        // Base scenes used for custom map slots
        private static readonly string[] BASE_SCENES = { "FreeRun3_1", "FreeRun5_1", "FreeRun8_1" };

        // Skip these — UI draw calls, player character, logic markers
        private static readonly string[] SKIP_CONTAINS = new string[]
        {
            "_UIDrawCall", "ExampleCharacter", "IsDied", "IsPause", "IsFireOnline",
        };

        // Skip objects whose full path is exactly one of these (invisible boundary volumes)
        private static readonly string[] SKIP_EXACT = new string[]
        {
            "Cube", "Sphere", "Plane", "Cylinder", "Capsule",
        };

        private GameObject _mapRoot = null;
        private bool _spawnRunning = false;
        private bool _holdingPlayer = false;

        // Loading room — sealed collision cage far outside all donor maps.
        // Player is moved here before map geometry is built so they never see
        // the donor scene or the in-progress custom-map clone pass.
        private static readonly Vector3 LOADING_POS = new Vector3(0f, 4800f, 0f);
        private GameObject _loadingRoom = null;

        private void BuildLoadingRoom()
        {
            if (_loadingRoom != null) { UnityEngine.Object.Destroy(_loadingRoom); _loadingRoom = null; }
            _loadingRoom = new GameObject("[CNRMod_Loading]");
            // Small sealed box: 8×4×8, floor at LOADING_POS.y - 2
            const float W = 8f, H = 4f, T = 0.3f;
            float bx = LOADING_POS.x, by = LOADING_POS.y, bz = LOADING_POS.z;
            AddFaceSlab(_loadingRoom, new Vector3(bx, by - H*0.5f, bz), new Vector3(W, T, W));  // floor
            AddFaceSlab(_loadingRoom, new Vector3(bx, by + H*0.5f, bz), new Vector3(W, T, W));  // ceiling
            AddFaceSlab(_loadingRoom, new Vector3(bx + W*0.5f, by, bz), new Vector3(T, H, W));  // +X
            AddFaceSlab(_loadingRoom, new Vector3(bx - W*0.5f, by, bz), new Vector3(T, H, W));  // -X
            AddFaceSlab(_loadingRoom, new Vector3(bx, by, bz + W*0.5f), new Vector3(W, H, T));  // +Z
            AddFaceSlab(_loadingRoom, new Vector3(bx, by, bz - W*0.5f), new Vector3(W, H, T));  // -Z
        }

        private static void TeleportPlayer(Vector3 pos)
        {
            GameObject player = GameObject.Find("ExampleCharacter");
            if (player == null) return;
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = pos;
            if (cc != null) cc.enabled = true;
        }

        void OnLevelWasLoaded(int level)
        {
            if (Array.IndexOf(BASE_SCENES, Application.loadedLevelName) < 0) return;
            if (_mapRoot != null) { Destroy(_mapRoot); _mapRoot = null; }
            _spawnRunning = false;
            // Only hold the player if a custom map is actually pending.
            // On vanilla map loads (no active URL), let the game run normally.
            string activeUrl  = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "");
            bool   cacheReady = PlayerPrefs.GetInt("CNRMod_MapCacheReady", 0) == 1 && File.Exists(CachePath);
            if (string.IsNullOrEmpty(activeUrl) && !cacheReady)
            {
                ModEntry.Log("MapLoader: vanilla map load, no custom map pending — skipping hold");
                return;
            }
            // Start holding the player at the loading position immediately so they
            // never see or interact with the donor scene while the map builds.
            _holdingPlayer = true;
            StartCoroutine(HoldAtLoadingPos());
            ModEntry.Log("MapLoader: entered base scene, waiting for map data...");
            StartCoroutine(WaitAndSpawn());
        }

        // Teleports the player to LOADING_POS every frame until _holdingPlayer is cleared.
        IEnumerator HoldAtLoadingPos()
        {
            Vector3 holdPos = new Vector3(LOADING_POS.x, LOADING_POS.y, LOADING_POS.z);
            while (_holdingPlayer)
            {
                TeleportPlayer(holdPos);
                yield return null;
            }
        }

        // Polls until cache is ready (written by RedirectHook.DownloadMap).
        // Falls back to direct download after 3s if ActiveMapURL is already known.
        IEnumerator WaitAndSpawn()
        {
            if (_spawnRunning) yield break;
            _spawnRunning = true;

            // Immediate: cache already written from a previous room session
            if (PlayerPrefs.GetInt("CNRMod_MapCacheReady", 0) == 1 && File.Exists(CachePath))
            {
                ModEntry.Log("MapLoader: cache ready immediately");
                StartCoroutine(SpawnAfterDelay());
                yield break;
            }

            float waited = 0f;
            while (waited < 30f)
            {
                yield return new WaitForSeconds(0.5f);
                waited += 0.5f;

                if (PlayerPrefs.GetInt("CNRMod_MapCacheReady", 0) == 1 && File.Exists(CachePath))
                {
                    ModEntry.Log("MapLoader: cache ready after " + waited.ToString("F1") + "s");
                    StartCoroutine(SpawnAfterDelay());
                    yield break;
                }

                // After 3s, if RedirectHook hasn’t kicked off a download yet,
                // try downloading directly from the URL the map picker saved
                if (waited >= 3f)
                {
                    string url = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "");
                    if (!string.IsNullOrEmpty(url))
                    {
                        ModEntry.Log("MapLoader: 3s timeout, direct download from " + url);
                        StartCoroutine(DownloadAndSpawn(url));
                        yield break;
                    }
                }
            }
            ModEntry.Log("MapLoader: timed out 30s with no map data");
            _spawnRunning = false;
        }

        IEnumerator DownloadAndSpawn(string url)
        {
            url = ModEntry.SanitizeUrl(url);
            var www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) { ModEntry.Log("MapLoader download error: " + www.error); _spawnRunning = false; yield break; }
            string json = www.text;
            if (string.IsNullOrEmpty(json)) { ModEntry.Log("MapLoader: empty response"); _spawnRunning = false; yield break; }
            try
            {
                File.WriteAllText(CachePath, json);
                PlayerPrefs.SetInt("CNRMod_MapCacheReady", 1);
                PlayerPrefs.Save();
                ModEntry.Log("MapLoader: cached (" + json.Length + " bytes)");
            }
            catch (Exception ex) { ModEntry.Log("MapLoader cache error: " + ex.Message); _spawnRunning = false; yield break; }
            StartCoroutine(SpawnAfterDelay());
        }

        IEnumerator SpawnAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);

            ModEntry.Log("MapLoader: building map (player held at loading pos)");

            try
            {
                string json = File.ReadAllText(CachePath);
                ModEntry.Log("MapLoader: parsing " + json.Length + " bytes");
                string trimmedJson = json.Trim();
                MapObjData[] items;
                if (trimmedJson.StartsWith("{"))
                {
                    // Wrapper format: {"donor":"FreeRun8_1","objects":[...]}
                    string donor = ModEntry.ParseJsonStringValue(trimmedJson, "donor");
                    if (!string.IsNullOrEmpty(donor)) ModEntry.Log("MapLoader: donor=" + donor);
                    int arrStart = trimmedJson.IndexOf("\"objects\"");
                    arrStart = arrStart >= 0 ? trimmedJson.IndexOf('[', arrStart) : -1;
                    if (arrStart < 0) { ModEntry.Log("MapLoader: no objects array in wrapper"); yield break; }
                    int depth = 0, arrEnd = arrStart;
                    for (int ci = arrStart; ci < trimmedJson.Length; ci++)
                    {
                        if (trimmedJson[ci] == '[') depth++;
                        else if (trimmedJson[ci] == ']') { depth--; if (depth == 0) { arrEnd = ci; break; } }
                    }
                    items = JsonReader.Deserialize<MapObjData[]>(trimmedJson.Substring(arrStart, arrEnd - arrStart + 1));
                }
                else
                {
                    items = JsonReader.Deserialize<MapObjData[]>(trimmedJson);
                }
                if (items == null || items.Length == 0)
                {
                    ModEntry.Log("MapLoader: JSON parse failed or empty");
                    yield break;
                }

                // Collect material cache (fallback for objects not found in donor)
                var sceneMatCache = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
                foreach (Renderer sr in (Renderer[])FindObjectsOfType(typeof(Renderer)))
                {
                    if (sr == null || sr.sharedMaterial == null) continue;
                    string mn = sr.sharedMaterial.name;
                    if (string.IsNullOrEmpty(mn)) continue;
                    // Strip Unity's " (Instance)" suffix so keys match map builder texture names
                    int parenIdx = mn.IndexOf(" (Instance)", StringComparison.OrdinalIgnoreCase);
                    if (parenIdx >= 0) mn = mn.Substring(0, parenIdx);
                    mn = mn.Trim();
                    if (!string.IsNullOrEmpty(mn) && !sceneMatCache.ContainsKey(mn))
                        sceneMatCache[mn] = sr.sharedMaterial;
                }
                ModEntry.Log("MapLoader: " + sceneMatCache.Count + " scene materials: " + string.Join(", ", new List<string>(sceneMatCache.Keys).ToArray()));

                // Create mapRoot NOW (before ClearBaseScene) so clones parented here are preserved
                _mapRoot = new GameObject("[CustomMap]");
                int spawned = 0;
                var clonedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Pass 1 — clone actual donor GameObjects while scene is still intact
                foreach (MapObjData obj in items)
                {
                    if (ShouldSkip(obj.path)) continue;
                    // Markers are tiny — handle them in pass 2 as primitives
                    bool isMarker = obj.path.Contains("EscapePosition") ||
                                    obj.path.Contains("EnemyPosition")  ||
                                    obj.path.Contains("PlayerPosition");
                    if (isMarker) continue;

                    // Skip "Combined Mesh (root: scene)" objects in Pass 1.
                    // Statically batched meshes have their geometry moved into a batch buffer;
                    // Instantiate() produces a clone with no visible geometry.
                    // Let them fall through to Pass 2 where CreatePrimitive gives a real mesh.
                    if (!string.IsNullOrEmpty(obj.mesh) &&
                        obj.mesh.IndexOf("Combined", StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;

                    // GameObject.Find requires exact full hierarchy path.
                    // Fall back to searching by leaf name alone so objects nested deeper
                    // than expected (e.g. SnowGround/Terrain/Box021) are still found.
                    GameObject donor = GameObject.Find(obj.path);
                    if (donor == null)
                    {
                        string leafName = obj.path.Contains("/")
                            ? obj.path.Substring(obj.path.LastIndexOf('/') + 1)
                            : obj.path;
                        GameObject[] all = (GameObject[])FindObjectsOfType(typeof(GameObject));
                        foreach (GameObject go in all)
                        {
                            if (string.Equals(go.name, leafName, StringComparison.OrdinalIgnoreCase))
                            { donor = go; break; }
                        }
                    }
                    if (donor == null) { ModEntry.Log("MapLoader: not found in scene: " + obj.path); continue; }
                    try
                    {
                        GameObject clone = (GameObject)UnityEngine.Object.Instantiate(donor);
                        clone.name = obj.path.Replace("/", "_");
                        clone.transform.parent = _mapRoot.transform;
                        clone.SetActive(true);
                        // Ensure all renderers visible
                        foreach (Renderer r in clone.GetComponentsInChildren<Renderer>(true))
                            r.enabled = true;
                        // Destroy ALL colliders — they either reference the static batch mesh
                        // (MeshColliders) or have a pivot-relative center that becomes wrong
                        // once we reposition the clone to the JSON bounding-box center.
                        foreach (Collider c in clone.GetComponentsInChildren<Collider>(true))
                            UnityEngine.Object.Destroy(c);
                        if (obj.pos != null && obj.pos.Length == 3)
                            clone.transform.position = new Vector3(obj.pos[0], obj.pos[1], obj.pos[2]);
                        if (obj.rot != null && obj.rot.Length >= 3)
                            clone.transform.rotation = Quaternion.Euler(obj.rot[0], obj.rot[1], obj.rot[2]);

                        // Apply JSON material / colour to every renderer on the clone.
                        // This ensures the user-chosen texture is always visible even when the
                        // donor material name doesn't match or the mesh was statically batched.
                        {
                            Material matOverride = null;
                            if (!string.IsNullOrEmpty(obj.mat))
                                sceneMatCache.TryGetValue(obj.mat, out matOverride);
                            foreach (Renderer cr in clone.GetComponentsInChildren<Renderer>(true))
                            {
                                if (matOverride != null)
                                    cr.material = matOverride;
                                else if (obj.color != null && obj.color.Length >= 3)
                                    cr.material.color = new Color(
                                        obj.color[0] / 255f, obj.color[1] / 255f, obj.color[2] / 255f,
                                        obj.color.Length >= 4 ? obj.color[3] / 255f : 1f);
                            }
                        }

                        // Try MeshCollider.convex for objects that are NOT part of Unity's
                        // static batch (individually authored meshes, e.g. props/stairs).
                        // Statically batched objects have sharedMesh == null or named "Combined Mesh…"
                        // and will fall through to the 6-face hollow-shell below.
                        bool colliderAdded = false;
                        if (obj.collidable)
                        {
                        foreach (MeshFilter mf in clone.GetComponentsInChildren<MeshFilter>(true))
                        {
                            if (mf.sharedMesh == null || mf.sharedMesh.vertexCount < 4) continue;
                            string mn = mf.sharedMesh.name ?? "";
                            if (mn.IndexOf("Combined", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                            if (mf.sharedMesh.vertexCount > 8000) continue;
                            try
                            {
                                var mc = mf.gameObject.AddComponent<MeshCollider>();
                                mc.sharedMesh = mf.sharedMesh;
                                mc.convex     = true;
                                colliderAdded = true;
                            }
                            catch (Exception mcEx)
                            {
                                ModEntry.Log("MeshCollider failed for " + obj.path + ": " + mcEx.Message);
                                var bad = mf.gameObject.GetComponent<MeshCollider>();
                                if (bad != null) UnityEngine.Object.Destroy(bad);
                            }
                        }

                        // 6-face hollow-shell fallback — thin BoxCollider slabs parented to
                        // _mapRoot (always scale 1,1,1) so bc.size == world size exactly,
                        // regardless of the clone's pivot offset or donor scale.
                        if (!colliderAdded && obj.size != null && obj.size.Length == 3
                                          && obj.pos  != null && obj.pos.Length  == 3)
                        {
                            float wx = obj.size[0], wy = obj.size[1], wz = obj.size[2];
                            float cx = obj.pos[0],  cy = obj.pos[1],  cz = obj.pos[2];
                            const float T = 0.15f;
                            AddFaceSlab(_mapRoot, new Vector3(cx,           cy + wy*0.5f, cz          ), new Vector3(wx, T,  wz));
                            AddFaceSlab(_mapRoot, new Vector3(cx,           cy - wy*0.5f, cz          ), new Vector3(wx, T,  wz));
                            AddFaceSlab(_mapRoot, new Vector3(cx + wx*0.5f, cy,           cz          ), new Vector3(T,  wy, wz));
                            AddFaceSlab(_mapRoot, new Vector3(cx - wx*0.5f, cy,           cz          ), new Vector3(T,  wy, wz));
                            AddFaceSlab(_mapRoot, new Vector3(cx,           cy,           cz + wz*0.5f), new Vector3(wx, wy, T ));
                            AddFaceSlab(_mapRoot, new Vector3(cx,           cy,           cz - wz*0.5f), new Vector3(wx, wy, T ));
                        }
                        } // end if (obj.collidable)
                        clonedPaths.Add(obj.path);
                        spawned++;
                    }
                    catch (Exception cloneEx) { ModEntry.Log("Clone failed: " + obj.path + " err: " + cloneEx.Message); }
                }
                ModEntry.Log("MapLoader: cloned " + clonedPaths.Count + " donor objects");

                // Now hide original scene geometry (clones under [CustomMap] are safe)
                ClearBaseScene();

                // Pass 2 — primitive fallback for anything not found in the donor
                foreach (MapObjData obj in items)
                {
                    if (ShouldSkip(obj.path)) continue;
                    if (clonedPaths.Contains(obj.path)) continue;  // already cloned

                    PrimitiveType ptype = MeshToPrimitive(obj.mesh);
                    var go = GameObject.CreatePrimitive(ptype);
                    go.name = obj.path.Replace("/", "_");
                    go.transform.parent = _mapRoot.transform;

                    // Bake per-face UVs BEFORE we set localScale, while the mesh is still unit-sized.
                    if (ptype == PrimitiveType.Cube && obj.size != null && obj.size.Length == 3)
                    {
                        var mf = go.GetComponent<MeshFilter>();
                        if (mf != null)
                            ApplyBoxUVs(mf,
                                Mathf.Max(0.01f, obj.size[0]),
                                Mathf.Max(0.01f, obj.size[1]),
                                Mathf.Max(0.01f, obj.size[2]));
                    }

                    if (obj.pos != null && obj.pos.Length == 3)
                        go.transform.position = new Vector3(obj.pos[0], obj.pos[1], obj.pos[2]);
                    if (obj.size != null && obj.size.Length == 3)
                        go.transform.localScale = new Vector3(
                            Mathf.Max(0.01f, obj.size[0]),
                            Mathf.Max(0.01f, obj.size[1]),
                            Mathf.Max(0.01f, obj.size[2]));
                    if (obj.rot != null && obj.rot.Length >= 3)
                        go.transform.rotation = Quaternion.Euler(obj.rot[0], obj.rot[1], obj.rot[2]);

                    var renderer = go.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material mat = null;
                        if (!string.IsNullOrEmpty(obj.mat))
                            sceneMatCache.TryGetValue(obj.mat, out mat);
                        if (mat != null)
                        {
                            renderer.material = mat;
                            // UVs are baked per-face by ApplyBoxUVs above, so reset any
                            // tiling the source material might carry (creates a per-instance copy).
                            renderer.material.mainTextureScale = Vector2.one;
                        }
                        else if (obj.color != null && obj.color.Length >= 3)
                            renderer.material.color = new Color(
                                obj.color[0] / 255f, obj.color[1] / 255f, obj.color[2] / 255f,
                                obj.color.Length >= 4 ? obj.color[3] / 255f : 1f);
                    }

                    // Markers: invisible, no collision
                    bool isMarker2 = obj.path.Contains("EscapePosition") ||
                                     obj.path.Contains("EnemyPosition")  ||
                                     obj.path.Contains("PlayerPosition");
                    if (isMarker2)
                    {
                        if (renderer != null) renderer.enabled = false;
                        var col = go.GetComponent<Collider>();
                        if (col != null) col.enabled = false;
                    }
                    else if (obj.isColBox)
                    {
                        // Collision box: invisible but physically solid
                        if (renderer != null) renderer.enabled = false;
                        // BoxCollider from CreatePrimitive is already solid — leave it enabled
                    }
                    else if (!obj.collidable)
                    {
                        var col = go.GetComponent<Collider>();
                        if (col != null) col.enabled = false;
                    }
                    spawned++;
                }

                ModEntry.Log("MapLoader: spawned " + spawned + " (" + clonedPaths.Count + " cloned, " + (spawned - clonedPaths.Count) + " primitives)");

                // ── Step 4: collect spawn positions and hand off to RespawnWatcher ──
                Vector3? escSpawn = null, enmSpawn = null;
                foreach (MapObjData obj in items)
                {
                    if (obj.path == null || obj.pos == null || obj.pos.Length < 3) continue;
                    Vector3 p = new Vector3(obj.pos[0], obj.pos[1] + 1f, obj.pos[2]);
                    if (escSpawn == null && obj.path.Contains("EscapePosition")) escSpawn = p;
                    if (enmSpawn == null && obj.path.Contains("EnemyPosition"))  enmSpawn = p;
                }

                // Compute centroid for facing direction
                float _cx = 0, _cz = 0; int _cn = 0; float _floorY = float.MaxValue;
                foreach (MapObjData _o in items)
                {
                    if (_o.pos == null || _o.pos.Length < 3) continue;
                    if (ShouldSkip(_o.path)) continue;
                    if (_o.path != null && _o.path.Contains("Position")) continue;
                    _cx += _o.pos[0]; _cz += _o.pos[2]; _cn++;
                    float _halfH = (_o.size != null && _o.size.Length >= 2) ? Mathf.Abs(_o.size[1]) * 0.5f : 0.5f;
                    float _top = _o.pos[1] + _halfH;
                    if (_top < _floorY) _floorY = _top;
                }
                float _spawnY = (_floorY < float.MaxValue ? _floorY : 0f) + 1.8f;
                Vector3 watcherCentroid = _cn > 0 ? new Vector3(_cx / _cn, _spawnY, _cz / _cn) : Vector3.zero;

                // Attach watcher to _mapRoot so it dies if the map is reloaded
                var watcher = _mapRoot.AddComponent<RespawnWatcher>();
                if (escSpawn.HasValue) { watcher.EscapeSpawn = escSpawn.Value; watcher.HasEscape = true; }
                if (enmSpawn.HasValue) { watcher.EnemySpawn  = enmSpawn.Value;  watcher.HasEnemy  = true; }
                watcher.MapCentroid = watcherCentroid;

                // ── Step 5: release hold and teleport to spawn ───────────────
                _holdingPlayer = false;
                TeleportToSpawn(items);
            }
            catch (Exception ex) { ModEntry.Log("MapLoader error: " + ex.Message); _holdingPlayer = false; }
        }

        // Move all in-scene EscapePosition / EnemyPosition / SpawnPoint GameObjects
        // to the positions defined in the custom-map JSON, so the game's built-in
        // after-death respawn system uses the correct locations.
        private static void UpdateRespawnPoints(MapObjData[] items)
        {
            try
            {
                var escapePositions = new System.Collections.Generic.List<Vector3>();
                var enemyPositions  = new System.Collections.Generic.List<Vector3>();

                foreach (MapObjData obj in items)
                {
                    if (obj.path == null || obj.pos == null || obj.pos.Length < 3) continue;
                    Vector3 p = new Vector3(obj.pos[0], obj.pos[1] + 1f, obj.pos[2]);
                    if (obj.path.Contains("EscapePosition")) escapePositions.Add(p);
                    else if (obj.path.Contains("EnemyPosition")) enemyPositions.Add(p);
                }

                if (escapePositions.Count == 0 && enemyPositions.Count == 0)
                {
                    ModEntry.Log("UpdateRespawnPoints: no team markers in JSON, skipping");
                    return;
                }

                int escIdx = 0, enmIdx = 0;
                GameObject[] all = (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
                foreach (GameObject go in all)
                {
                    if (go == null) continue;
                    string name = go.name ?? "";
                    if (name.Contains("EscapePosition") && escapePositions.Count > 0)
                    {
                        go.transform.position = escapePositions[escIdx % escapePositions.Count];
                        escIdx++;
                    }
                    else if (name.Contains("EnemyPosition") && enemyPositions.Count > 0)
                    {
                        go.transform.position = enemyPositions[enmIdx % enemyPositions.Count];
                        enmIdx++;
                    }
                    else if ((name.Contains("SpawnPoint") || name.Contains("Spawn_Point"))
                             && (escapePositions.Count > 0 || enemyPositions.Count > 0))
                    {
                        // Generic spawn point — assign escape or enemy pool round-robin
                        var pool = escapePositions.Count > 0 ? escapePositions : enemyPositions;
                        int idx  = escIdx + enmIdx;
                        go.transform.position = pool[idx % pool.Count];
                        escIdx++;
                    }
                }
                ModEntry.Log("UpdateRespawnPoints: escape=" + escapePositions.Count + " enemy=" + enemyPositions.Count
                             + " moved esc=" + escIdx + " enmIdx=" + enmIdx);
            }
            catch (Exception ex) { ModEntry.Log("UpdateRespawnPoints error: " + ex.Message); }
        }

        // Teleport the local player to the most appropriate spawn in the map data.
        // Prefers team-specific markers (EscapePosition for master, EnemyPosition for others).
        // Falls back to any PlayerPosition marker, then to geometry centroid.
        private static void TeleportToSpawn(MapObjData[] items)
        {
            try
            {
                // Collect all spawn candidates for this player's team
                string prefer  = ModEntry.IsMaster ? "EscapePosition" : "EnemyPosition";
                string fallback = "PlayerPosition";

                Vector3? teamSpawn    = null;
                Vector3? genericSpawn = null;

                foreach (MapObjData obj in items)
                {
                    if (obj.path == null || obj.pos == null || obj.pos.Length < 3) continue;
                    Vector3 p = new Vector3(obj.pos[0], obj.pos[1] + 1f, obj.pos[2]);
                    if (teamSpawn    == null && obj.path.Contains(prefer))   teamSpawn    = p;
                    if (genericSpawn == null && obj.path.Contains(fallback)) genericSpawn = p;
                }

                // Compute geometry centroid as last-resort fallback
                float sx = 0, sz = 0; int cnt = 0;
                float floorY = float.MaxValue;
                foreach (MapObjData obj in items)
                {
                    if (obj.pos == null || obj.pos.Length < 3) continue;
                    if (ShouldSkip(obj.path)) continue;
                    if (obj.path != null && obj.path.Contains("Position")) continue;
                    sx += obj.pos[0]; sz += obj.pos[2]; cnt++;
                    float halfH = (obj.size != null && obj.size.Length >= 2) ? Mathf.Abs(obj.size[1]) * 0.5f : 0.5f;
                    float topSurface = obj.pos[1] + halfH;
                    if (topSurface < floorY) floorY = topSurface;
                }
                float spawnY = (floorY < float.MaxValue ? floorY : 0f) + 1.8f;
                Vector3 centroid = cnt > 0 ? new Vector3(sx / cnt, spawnY, sz / cnt) : new Vector3(0f, spawnY, 0f);

                Vector3 pos = teamSpawn ?? genericSpawn ?? centroid;
                string src  = teamSpawn != null ? prefer : (genericSpawn != null ? fallback : "centroid");

                ModEntry.Log("TeleportToSpawn: " + src + "=" + pos + " centroid=" + centroid + " (isMaster=" + ModEntry.IsMaster + ")");

                GameObject player = GameObject.Find("ExampleCharacter");
                if (player == null) { ModEntry.Log("TeleportToSpawn: ExampleCharacter not found"); return; }
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = pos;
                // Face toward the map's geometry centre so the view matches the editor's camera.
                Vector3 lookDir = new Vector3(centroid.x - pos.x, 0f, centroid.z - pos.z);
                if (lookDir.sqrMagnitude > 1f)
                    player.transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                if (cc != null) cc.enabled = true;
            }
            catch (Exception ex) { ModEntry.Log("TeleportToSpawn error: " + ex.Message); }
        }

        // Disable all renderers + colliders on base scene geometry, leaving
        // cameras, lights, directors, audio, players, and UI intact.
        private static void ClearBaseScene()
        {
            // Name fragments that identify non-geometry roots to preserve
            string[] preserve = new string[]
            {
                "Camera", "camera", "Light", "light", "Sun", "Sky", "Fog",
                "Director", "Manager", "Controller", "Audio", "Sound",
                "Player", "Character", "Spawn", "SpawnPoint",
                "Canvas", "EventSystem", "UI", "UIRoot", "NGUI",
                "_UIDrawCall", "UIPanel", "UICamera", "UISprite", "UILabel",
                "Photon", "CNRMod", "[CustomMap]",
                "ExampleCharacter", "IsDied", "IsPause",
                // In-game HUD, controls, settings, skybox — must not lose colliders
                "InGameMenu", "VCAnalog", "Joystick", "HUD", "Hud",
                "MainScene", "KamcordPrefab", "CNRSettings",
                // Environment / lighting roots
                "Environment", "Ambient", "Render", "Skybox", "Directional",
            };

            int cleared = 0;
            System.Text.StringBuilder clearedNames = new System.Text.StringBuilder();
            System.Text.StringBuilder preservedNames = new System.Text.StringBuilder();
            GameObject[] roots = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
            foreach (GameObject go in roots)
            {
                if (go.transform.parent != null) continue;  // only root objects
                if (ShouldPreserveRoot(go.name, preserve))
                {
                    if (preservedNames.Length < 300) preservedNames.Append(go.name).Append("|");
                    continue;
                }

                if (clearedNames.Length < 300) clearedNames.Append(go.name).Append("|");
                // Disable Renderers and Colliders. NGUI button colliders are safe
                // because UIRoot/_UIDrawCall/UIPanel etc. are all in the preserve list.
                foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;
                foreach (Collider c in go.GetComponentsInChildren<Collider>(true))
                    c.enabled = false;
                cleared++;
            }
            ModEntry.Log("ClearBaseScene: cleared " + cleared + " | CLEARED: " + clearedNames);
            ModEntry.Log("ClearBaseScene: PRESERVED: " + preservedNames);
        }

        private static bool ShouldPreserveRoot(string name, string[] keywords)
        {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (string k in keywords)
                if (name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        // Creates a thin BoxCollider slab as a child of 'parent', positioned at an
        // exact world-space centre with world-space extents.  Because the child starts
        // with scale (1,1,1), local units == world units and the math is trivial.
        private static void AddFaceSlab(GameObject parent, Vector3 worldCenter, Vector3 worldSize)
        {
            GameObject slab = new GameObject("_col");
            slab.transform.parent   = parent.transform;
            slab.transform.position = worldCenter;           // Unity auto-converts to localPosition
            slab.transform.localRotation = Quaternion.identity;
            slab.transform.localScale    = Vector3.one;
            BoxCollider bc = slab.AddComponent<BoxCollider>();
            bc.center = Vector3.zero;
            bc.size   = worldSize;  // scale is (1,1,1) so local == world
        }

        private static bool ShouldSkip(string path)
        {
            if (string.IsNullOrEmpty(path)) return true;
            foreach (string s in SKIP_EXACT)
                if (path == s) return true;
            foreach (string s in SKIP_CONTAINS)
                if (path.Contains(s)) return true;
            return false;
        }

        // Watches for death→alive transitions each frame and re-teleports the player
        // to the correct team spawn.  Attached to _mapRoot so it is destroyed on reload.
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
                if (ModEntry.IsMaster && HasEscape) return EscapeSpawn;
                if (!ModEntry.IsMaster && HasEnemy) return EnemySpawn;
                if (HasEscape) return EscapeSpawn;
                if (HasEnemy)  return EnemySpawn;
                return Vector3.zero;
            }

            private void DoTeleport(Vector3 pos)
            {
                GameObject player = GameObject.Find("ExampleCharacter");
                if (player == null) return;
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = pos;
                Vector3 lookDir = new Vector3(MapCentroid.x - pos.x, 0f, MapCentroid.z - pos.z);
                if (lookDir.sqrMagnitude > 1f)
                    player.transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                if (cc != null) cc.enabled = true;
            }

            private void Update()
            {
                // Cache IsDied lookup (search once, keep reference)
                if (_isDiedObj == null)
                    _isDiedObj = GameObject.Find("IsDied");

                bool isDead = _isDiedObj != null && _isDiedObj.activeSelf;

                // Transition: was dead last frame, now alive → player just respawned
                if (_wasDeadLastFrame && !isDead)
                {
                    Vector3 spawn = GetMySpawn();
                    if (spawn != Vector3.zero)
                    {
                        DoTeleport(spawn);
                        ModEntry.Log("RespawnWatcher: respawned, teleported to " + spawn);
                    }
                }
                _wasDeadLastFrame = isDead;
            }
        }

        private static PrimitiveType MeshToPrimitive(string mesh)
        {
            if (string.IsNullOrEmpty(mesh)) return PrimitiveType.Cube;
            string m = mesh.ToLower();
            if (m.Contains("sphere"))   return PrimitiveType.Sphere;
            if (m.Contains("capsule"))  return PrimitiveType.Capsule;
            if (m.Contains("cylinder")) return PrimitiveType.Cylinder;
            return PrimitiveType.Cube;
        }

        // Rewrite the UV coordinates on Unity's default cube mesh so that each face tiles
        // at exactly dim/2 repeats — one texture repeat per 2 world units — matching the
        // map builder's per-face tiling formula.  We read the existing vertex normals to
        // identify which face each vertex belongs to, then map the two in-plane local
        // coordinates to the correct UV range.
        //
        //  ±Y (top/bot): U along X (range sx/2),  V along Z (range sz/2)
        //  ±Z (frt/bck): U along X (range sx/2),  V along Y (range sy/2)
        //  ±X (lft/rgt): U along Z (range sz/2),  V along Y (range sy/2)
        //
        // localScale must be set to (sx, sy, sz) AFTER this call so the UVs correlate
        // with the final world-space face sizes.
        private static void ApplyBoxUVs(MeshFilter mf, float sx, float sy, float sz)
        {
            Mesh mesh = mf.mesh;  // creates a per-instance copy if the mesh is shared
            Vector3[] verts   = mesh.vertices;   // local space, each coord ∈ [−0.5, 0.5]
            Vector3[] normals = mesh.normals;
            var uvs = new Vector2[verts.Length];

            float uY = sx * 0.5f, vY = sz * 0.5f;   // ±Y faces
            float uZ = sx * 0.5f, vZ = sy * 0.5f;   // ±Z faces
            float uX = sz * 0.5f, vX = sy * 0.5f;   // ±X faces

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 n = normals[i];
                Vector3 v = verts[i];
                // (v.axis + 0.5) maps the local [−0.5,0.5] range to [0,1],
                // then multiply by the desired tile count for that dimension.
                if (Mathf.Abs(n.y) >= 0.5f)
                    uvs[i] = new Vector2((v.x + 0.5f) * uY, (v.z + 0.5f) * vY);
                else if (Mathf.Abs(n.z) >= 0.5f)
                    uvs[i] = new Vector2((v.x + 0.5f) * uZ, (v.y + 0.5f) * vZ);
                else
                    uvs[i] = new Vector2((v.z + 0.5f) * uX, (v.y + 0.5f) * vX);
            }
            mesh.uv = uvs;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CONTENT MANAGER — downloads and caches official maps, textures, data files
    // ══════════════════════════════════════════════════════════════════════════
    public class OfficialMapEntry
    {
        public string Id            = "";
        public string Name          = "";
        public string Url           = "";
        public string ThumbnailUrl  = "";   // optional image shown in map picker
        public string Hash          = "";   // MD5 of the map JSON file (empty = skip verify)
        public string ThumbnailHash = "";   // MD5 of the thumbnail image (empty = skip verify)
    }

    public class OfficialTextureEntry
    {
        public string Id           = "";
        public string MaterialName = "";
        public string Url          = "";
        public string Hash         = "";   // MD5 of the texture file
    }

    public class OfficialDataEntry
    {
        public string Id   = "";
        public string Key  = "";
        public string Url  = "";
        public string Hash = "";   // MD5 of the data file
    }

    // Raw JSON deserialization targets (field names match server JSON keys)
    class CManifestMap     { public string id = ""; public string name = ""; public string url = ""; public string thumbnail_url = ""; public string hash = ""; public string thumbnail_hash = ""; }
    class CManifestTexture { public string id = ""; public string material_name = ""; public string url = ""; public string hash = ""; }
    class CManifestData    { public string id = ""; public string key = ""; public string url = ""; public string hash = ""; }
    class CManifest
    {
        public string           manifest_version = "";
        public CManifestMap[]     maps     = new CManifestMap[0];
        public CManifestTexture[] textures = new CManifestTexture[0];
        public CManifestData[]    data     = new CManifestData[0];
    }

    public class ContentManager : MonoBehaviour
    {
        private const string ContentUrl    = "https://play.jacqueb.me/economy/content.php";
        public  const string MapCacheDir   = "/storage/emulated/0/CNRMods/content_cache/maps/";
        private const string TexCacheDir   = "/storage/emulated/0/CNRMods/content_cache/textures/";
        private const string ThumbCacheDir = "/storage/emulated/0/CNRMods/content_cache/thumbs/";
        private const string DataCacheDir  = "/storage/emulated/0/CNRMods/content_cache/data/";
        private const string ManifestCache = "/storage/emulated/0/CNRMods/content_cache/manifest.json";
        private const string VersionPref   = "CNRMod_ContentVersion";

        public static OfficialMapEntry[]     OfficialMaps     = new OfficialMapEntry[0];
        public static OfficialTextureEntry[] OfficialTextures = new OfficialTextureEntry[0];
        public static OfficialDataEntry[]    OfficialData     = new OfficialDataEntry[0];
        public static bool Ready = false;

        // material name (lowercase) → Texture2D loaded from file
        private static Dictionary<string, Texture2D> _texCache   = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        // map id → Texture2D thumbnail
        private static Dictionary<string, Texture2D> _thumbCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        public static Texture2D GetMapThumbnail(string id)
        {
            Texture2D t;
            return _thumbCache.TryGetValue(id, out t) ? t : null;
        }

        void Start()
        {
            EnsureDirs();
            // Load cached manifest synchronously so map list is ready before the lobby opens
            LoadCachedManifest();
            StartCoroutine(FetchAndSync());
        }

        void OnLevelWasLoaded(int level)
        {
            if (_texCache.Count > 0)
                StartCoroutine(ApplyTextureSwaps());
        }

        static void EnsureDirs()
        {
            try
            {
                string[] dirs = new string[]
                {
                    "/storage/emulated/0/CNRMods/content_cache/",
                    MapCacheDir, TexCacheDir, ThumbCacheDir, DataCacheDir
                };
                foreach (string d in dirs)
                    if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            }
            catch (Exception ex) { ModEntry.Log("ContentManager EnsureDirs: " + ex.Message); }
        }

        // Load everything from the local cache file (no network call)
        static void LoadCachedManifest()
        {
            if (!File.Exists(ManifestCache)) { Ready = true; return; }
            try
            {
                ParseManifest(File.ReadAllText(ManifestCache));
                // Pre-load cached texture bytes
                foreach (var te in OfficialTextures)
                {
                    string p = TexCacheDir + te.Id + ".png";
                    if (File.Exists(p)) LoadTexFile(te.Id, p);
                }
                // Pre-load cached map thumbnails
                foreach (var om in OfficialMaps)
                {
                    foreach (string ext in new[]{"jpg","png"})
                    {
                        string p = ThumbCacheDir + om.Id + "." + ext;
                        if (File.Exists(p)) { LoadThumbFile(om.Id, p); break; }
                    }
                }
                // Verify hashes; delete bad files and force a re-download on next sync
                if (!VerifyAndClean())
                {
                    ModEntry.Log("ContentManager: hash mismatch(es) found — clearing version to force re-download");
                    PlayerPrefs.SetString(VersionPref, "");
                    PlayerPrefs.Save();
                }
                ModEntry.Log("ContentManager: cache loaded — maps=" + OfficialMaps.Length
                    + " tex=" + OfficialTextures.Length + " data=" + OfficialData.Length);
            }
            catch (Exception ex) { ModEntry.Log("ContentManager: LoadCachedManifest error: " + ex.Message); }
            Ready = true;
        }

        // Compute MD5 hex string for a file (returns null on error)
        static string ComputeMD5(string path)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                using (var stream = File.OpenRead(path))
                {
                    byte[] h = md5.ComputeHash(stream);
                    var sb = new System.Text.StringBuilder(32);
                    foreach (byte b in h) sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }
            }
            catch { return null; }
        }

        // For each cached file that has a server-provided hash, verify MD5.
        // Deletes any file whose hash doesn't match the server manifest.
        // Returns true if everything is OK, false if anything was deleted.
        static bool VerifyAndClean()
        {
            bool allOk = true;
            foreach (var m in OfficialMaps)
            {
                // Map JSON
                if (!string.IsNullOrEmpty(m.Hash))
                {
                    string p = MapCacheDir + m.Id + ".json";
                    if (File.Exists(p))
                    {
                        string got = ComputeMD5(p);
                        if (got != m.Hash.ToLower())
                        {
                            ModEntry.Log("ContentManager: map hash mismatch [" + m.Id + "] server=" + m.Hash + " local=" + got + " — deleting");
                            try { File.Delete(p); } catch {}
                            allOk = false;
                        }
                    }
                }
                // Thumbnail
                if (!string.IsNullOrEmpty(m.ThumbnailHash))
                {
                    foreach (string ext in new[]{"jpg","png","gif","webp"})
                    {
                        string tp = ThumbCacheDir + m.Id + "." + ext;
                        if (!File.Exists(tp)) continue;
                        string got = ComputeMD5(tp);
                        if (got != m.ThumbnailHash.ToLower())
                        {
                            ModEntry.Log("ContentManager: thumb hash mismatch [" + m.Id + "] — deleting");
                            try { File.Delete(tp); } catch {}
                            allOk = false;
                        }
                        break;
                    }
                }
            }
            foreach (var te in OfficialTextures)
            {
                if (!string.IsNullOrEmpty(te.Hash))
                {
                    string p = TexCacheDir + te.Id + ".png";
                    if (File.Exists(p))
                    {
                        string got = ComputeMD5(p);
                        if (got != te.Hash.ToLower())
                        {
                            ModEntry.Log("ContentManager: tex hash mismatch [" + te.Id + "] — deleting");
                            try { File.Delete(p); } catch {}
                            allOk = false;
                        }
                    }
                }
            }
            foreach (var d in OfficialData)
            {
                if (!string.IsNullOrEmpty(d.Hash))
                {
                    string p = DataCacheDir + d.Id + ".json";
                    if (File.Exists(p))
                    {
                        string got = ComputeMD5(p);
                        if (got != d.Hash.ToLower())
                        {
                            ModEntry.Log("ContentManager: data hash mismatch [" + d.Id + "] — deleting");
                            try { File.Delete(p); } catch {}
                            allOk = false;
                        }
                    }
                }
            }
            return allOk;
        }

        // Fetch fresh manifest, compare version, download anything new
        IEnumerator FetchAndSync()
        {
            var www = new WWW(ContentUrl);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                ModEntry.Log("ContentManager: fetch error: " + www.error);
                Ready = true;
                yield break;
            }

            string json    = www.text;
            string newVer  = ModEntry.ParseJsonValue(json, "manifest_version") ?? "";
            string oldVer  = PlayerPrefs.GetString(VersionPref, "");

            // Always re-parse in case names/urls changed
            ParseManifest(json);

            try { File.WriteAllText(ManifestCache, json); } catch {}

            if (newVer != oldVer || newVer == "")
            {
                ModEntry.Log("ContentManager: new version (" + oldVer + " -> " + newVer + "), downloading items");
                yield return StartCoroutine(DownloadItems());
                PlayerPrefs.SetString(VersionPref, newVer);
                PlayerPrefs.Save();
            }
            else
            {
                ModEntry.Log("ContentManager: up to date (" + newVer + ")");
            }

            Ready = true;
            ModEntry.Log("ContentManager ready — maps=" + OfficialMaps.Length
                + " tex=" + OfficialTextures.Length + " data=" + OfficialData.Length);
        }

        static void ParseManifest(string json)
        {
            try
            {
                // Extract the three arrays from the wrapper object manually,
                // then defer to JsonFx for each typed array.
                var maps     = ExtractArray(json, "maps");
                var textures = ExtractArray(json, "textures");
                var data     = ExtractArray(json, "data");

                var rawMaps = string.IsNullOrEmpty(maps) ? new CManifestMap[0]
                    : JsonReader.Deserialize<CManifestMap[]>(maps) ?? new CManifestMap[0];
                var rawTex  = string.IsNullOrEmpty(textures) ? new CManifestTexture[0]
                    : JsonReader.Deserialize<CManifestTexture[]>(textures) ?? new CManifestTexture[0];
                var rawData = string.IsNullOrEmpty(data) ? new CManifestData[0]
                    : JsonReader.Deserialize<CManifestData[]>(data) ?? new CManifestData[0];

                var mList = new List<OfficialMapEntry>();
                foreach (var m in rawMaps)
                    if (!string.IsNullOrEmpty(m.id) && !string.IsNullOrEmpty(m.url))
                        mList.Add(new OfficialMapEntry { Id = m.id, Name = m.name, Url = m.url, ThumbnailUrl = m.thumbnail_url ?? "", Hash = m.hash ?? "", ThumbnailHash = m.thumbnail_hash ?? "" });
                OfficialMaps = mList.ToArray();

                var tList = new List<OfficialTextureEntry>();
                foreach (var t in rawTex)
                    if (!string.IsNullOrEmpty(t.id) && !string.IsNullOrEmpty(t.url))
                        tList.Add(new OfficialTextureEntry { Id = t.id, MaterialName = t.material_name, Url = t.url, Hash = t.hash ?? "" });
                OfficialTextures = tList.ToArray();

                var dList = new List<OfficialDataEntry>();
                foreach (var d in rawData)
                    if (!string.IsNullOrEmpty(d.id) && !string.IsNullOrEmpty(d.url))
                        dList.Add(new OfficialDataEntry { Id = d.id, Key = d.key, Url = d.url, Hash = d.hash ?? "" });
                OfficialData = dList.ToArray();
            }
            catch (Exception ex) { ModEntry.Log("ContentManager: ParseManifest error: " + ex.Message); }
        }

        // Extract a named JSON array string (e.g. [...]  from  {"maps":[...], ...})
        static string ExtractArray(string json, string key)
        {
            try
            {
                string k = "\"" + key + "\":";
                int ki = json.IndexOf(k);
                if (ki < 0) return null;
                int start = json.IndexOf('[', ki + k.Length);
                if (start < 0) return null;
                int depth = 0, end = start;
                for (int i = start; i < json.Length; i++)
                {
                    if (json[i] == '[') depth++;
                    else if (json[i] == ']') { depth--; if (depth == 0) { end = i; break; } }
                }
                return json.Substring(start, end - start + 1);
            }
            catch { return null; }
        }

        // Download all content items (maps → MapCacheDir, textures → TexCacheDir, data → DataCacheDir)
        IEnumerator DownloadItems()
        {
            foreach (var m in OfficialMaps)
            {
                string path = MapCacheDir + m.Id + ".json";
                yield return StartCoroutine(DownloadFile(m.Url, path, "map:" + m.Id));
                if (File.Exists(path) && !string.IsNullOrEmpty(m.Hash))
                {
                    string got = ComputeMD5(path);
                    if (got != m.Hash.ToLower())
                    {
                        ModEntry.Log("ContentManager: map download hash mismatch [" + m.Id + "] expected=" + m.Hash + " got=" + got + " — deleting");
                        try { File.Delete(path); } catch {}
                    }
                }

                // Download thumbnail if URL is provided
                if (!string.IsNullOrEmpty(m.ThumbnailUrl))
                {
                    string ext       = m.ThumbnailUrl.EndsWith(".png") ? "png" : "jpg";
                    string thumbPath = ThumbCacheDir + m.Id + "." + ext;
                    yield return StartCoroutine(DownloadFile(m.ThumbnailUrl, thumbPath, "thumb:" + m.Id));
                    if (File.Exists(thumbPath))
                    {
                        if (!string.IsNullOrEmpty(m.ThumbnailHash) && ComputeMD5(thumbPath) != m.ThumbnailHash.ToLower())
                        {
                            ModEntry.Log("ContentManager: thumb download hash mismatch [" + m.Id + "] — deleting");
                            try { File.Delete(thumbPath); } catch {}
                        }
                        else LoadThumbFile(m.Id, thumbPath);
                    }
                }
            }
            foreach (var te in OfficialTextures)
            {
                string path = TexCacheDir + te.Id + ".png";
                yield return StartCoroutine(DownloadFile(te.Url, path, "tex:" + te.Id));
                if (File.Exists(path))
                {
                    if (!string.IsNullOrEmpty(te.Hash) && ComputeMD5(path) != te.Hash.ToLower())
                    {
                        ModEntry.Log("ContentManager: tex download hash mismatch [" + te.Id + "] — deleting");
                        try { File.Delete(path); } catch {}
                    }
                    else LoadTexFile(te.Id, path);
                }
            }
            foreach (var d in OfficialData)
            {
                string path = DataCacheDir + d.Id + ".json";
                yield return StartCoroutine(DownloadFile(d.Url, path, "data:" + d.Id));
                if (File.Exists(path) && !string.IsNullOrEmpty(d.Hash))
                {
                    string got = ComputeMD5(path);
                    if (got != d.Hash.ToLower())
                    {
                        ModEntry.Log("ContentManager: data download hash mismatch [" + d.Id + "] — deleting");
                        try { File.Delete(path); } catch {}
                    }
                }
            }
        }

        IEnumerator DownloadFile(string url, string dest, string label)
        {
            var www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            { ModEntry.Log("ContentManager: download error [" + label + "]: " + www.error); yield break; }
            try { File.WriteAllBytes(dest, www.bytes); ModEntry.Log("ContentManager: saved " + label); }
            catch (Exception ex) { ModEntry.Log("ContentManager: write error [" + label + "]: " + ex.Message); }
        }

        static void LoadTexFile(string id, string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.name = id;
                    _texCache[id] = tex;
                }
            }
            catch (Exception ex) { ModEntry.Log("ContentManager: LoadTexFile error [" + id + "]: " + ex.Message); }
        }

        static void LoadThumbFile(string id, string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.name = "thumb_" + id;
                    _thumbCache[id] = tex;
                    ModEntry.Log("ContentManager: loaded thumbnail for " + id);
                }
            }
            catch (Exception ex) { ModEntry.Log("ContentManager: LoadThumbFile error [" + id + "]: " + ex.Message); }
        }

        // After each scene load, swap any scene material whose name matches a loaded texture entry
        IEnumerator ApplyTextureSwaps()
        {
            yield return new WaitForSeconds(0.3f);

            // Build a lookup: materialName (from entry) → Texture2D
            var matToTex = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            foreach (var te in OfficialTextures)
            {
                if (_texCache.ContainsKey(te.Id))
                    matToTex[te.MaterialName] = _texCache[te.Id];
            }
            if (matToTex.Count == 0) yield break;

            int swapped = 0;
            var renderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
            var seen = new HashSet<int>();
            foreach (Renderer r in renderers)
            {
                if (r == null || r.sharedMaterial == null) continue;
                int iid = r.sharedMaterial.GetInstanceID();
                if (seen.Contains(iid)) continue;
                seen.Add(iid);

                string mname = r.sharedMaterial.name;
                int pi = mname.IndexOf(" (Instance)", StringComparison.OrdinalIgnoreCase);
                if (pi >= 0) mname = mname.Substring(0, pi).Trim();

                if (matToTex.ContainsKey(mname))
                {
                    r.material.mainTexture = matToTex[mname];
                    swapped++;
                }
            }
            if (swapped > 0) ModEntry.Log("ContentManager: swapped " + swapped + " material texture(s)");
        }

        // Read a cached data file by its registered key. Returns null if not downloaded yet.
        public static string GetData(string key)
        {
            foreach (var d in OfficialData)
            {
                if (string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    string path = DataCacheDir + d.Id + ".json";
                    if (File.Exists(path))
                    {
                        try { return File.ReadAllText(path); }
                        catch { return null; }
                    }
                }
            }
            return null;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ECONOMY HOOK — server-authoritative coins/gems sync
    // ══════════════════════════════════════════════════════════════════════════
    public class EconomyHook : MonoBehaviour
    {
        // Public state read by CNRSettingsMod to show spinner in HUD
        public static bool  Ready       = false;  // true once server balance loaded
        public static bool  ServerUp    = false;  // true when last call succeeded
        public static int   ServerCoins = 0;
        public static int   ServerGems  = 0;

        // ── Mail inbox (parallel arrays, populated by FetchInbox) ─────────────
        public static int[]    MailIds      = new int[0];
        public static string[] MailSubjects = new string[0];
        public static string[] MailBodies   = new string[0];
        public static int[]    MailCoins    = new int[0];
        public static int[]    MailGems     = new int[0];
        public static bool[]   MailClaimed  = new bool[0];
        public static int      MailUnread   = 0;

        // ── CNRSettingsMod integration ────────────────────────────────────────
        /// <summary>Set by CNRSettingsMod on startup; changes Account button label to "Settings".</summary>
        public static bool          SettingsModPresent     = false;
        /// <summary>Registered by CNRSettingsMod; called when Account/Settings button is tapped.</summary>
        public static System.Action OnAccountButtonClicked = null;

        private const string PREF_PLAYER_ID      = "CNRMod_EcoPlayerId";
        private const string PREF_TOKEN          = "CNRMod_EcoToken";
        private const string PREF_LAST_SVR_COINS = "CNRMod_SvrCoins";  // last server-acknowledged balance
        private const string PREF_LAST_SVR_GEMS  = "CNRMod_SvrGems";
        private const string PREF_PROG_TS        = "CNRMod_ProgUpdatedAt"; // unix_ts of last local progression change
        private const string COINS_KEY           = "GameCoins";
        private const string GEMS_KEY            = "GameGems";

        private string _playerId = "";
        private string _token    = "";

        // Pending outgoing queue: each entry is { delta_coins, delta_gems, reason, match_id }
        private readonly List<PendingTx> _queue = new List<PendingTx>();
        private bool _sending = false;

        private struct PendingTx
        {
            public int    deltaCoins;
            public int    deltaGems;
            public string reason;
            public string matchId;   // null = earn, non-null = dedup
            public bool   isSpend;
        }

        private void Start()
        {
            _playerId = PlayerPrefs.GetString(PREF_PLAYER_ID, "");
            _token    = PlayerPrefs.GetString(PREF_TOKEN, "");
            StartCoroutine(RegisterAndSync());
            // Initialise menu overlay for the already-loaded scene
            _ecoScene = Application.loadedLevelName ?? "";
            if (_ecoScene == "MainMenu") StartCoroutine(EcoPatchDelay());
        }

        // ── Reconnect (called from Update when not Ready) ─────────────────────
        private IEnumerator ReconnectAttempt()
        {
            if (string.IsNullOrEmpty(ModEntry.EconomyUrl)) { _reconnectRunning = false; yield break; }
            string androidId = GetAndroidId();
            if (string.IsNullOrEmpty(androidId))            { _reconnectRunning = false; yield break; }
            string displayName = PlayerPrefs.GetString("LocalMultiplayerNickName", "Player");
            if (_playerId == androidId && !string.IsNullOrEmpty(_token))
                yield return StartCoroutine(ReLogin(androidId, displayName));
            else
                yield return StartCoroutine(Register(androidId, displayName));
            if (Ready) StartCoroutine(FetchInbox());
            if (Ready) StartCoroutine(SyncProgression());
            _reconnectRunning = false;
        }

        // ── Registration / initial sync ───────────────────────────────────────
        private IEnumerator RegisterAndSync()
        {
            if (string.IsNullOrEmpty(ModEntry.EconomyUrl)) yield break;

            // Get ANDROID_ID as stable device identifier
            string androidId = GetAndroidId();
            if (string.IsNullOrEmpty(androidId)) { ModEntry.Log("EcoHook: could not get ANDROID_ID"); yield break; }

            string displayName = PlayerPrefs.GetString("LocalMultiplayerNickName", "Player");

            // If we already have a stored player_id that matches this device, re-login
            // Otherwise register fresh
            if (_playerId == androidId && !string.IsNullOrEmpty(_token))
            {
                yield return StartCoroutine(ReLogin(androidId, displayName));
            }
            else
            {
                yield return StartCoroutine(Register(androidId, displayName));
            }

            // Fetch inbox + sync progression immediately after login/register
            if (Ready) StartCoroutine(FetchInbox());
            if (Ready) StartCoroutine(SyncProgression());
        }

        private IEnumerator Register(string androidId, string displayName)
        {
            string url  = ModEntry.EconomyUrl + "/register.php";
            string body = "player_id=" + Uri.EscapeDataString(androidId) +
                          "&display_name=" + Uri.EscapeDataString(displayName) +
                          "&token=" + Uri.EscapeDataString(_token); // send existing token if we have one
            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(url, System.Text.Encoding.UTF8.GetBytes(body), hdrs);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                ModEntry.Log("EcoHook register error: " + www.error);
                _connectError = www.error;
                Ready = false; ServerUp = false;
                yield break;
            }

            string newToken = ModEntry.ParseJsonStringValue(www.text, "token");
            if (string.IsNullOrEmpty(newToken))
            {
                // Could be 409 conflict if already registered — try re-login
                if (!string.IsNullOrEmpty(_token))
                    yield return StartCoroutine(ReLogin(androidId, displayName));
                else
                    ModEntry.Log("EcoHook register failed: " + www.text);
                yield break;
            }

            _playerId = androidId;
            _token    = newToken;
            PlayerPrefs.SetString(PREF_PLAYER_ID, _playerId);
            PlayerPrefs.SetString(PREF_TOKEN, _token);
            PlayerPrefs.Save();
            ModEntry.Log("EcoHook registered. playerId=" + _playerId.Substring(0, 4) + "...");

            ApplyServerBalance(www.text);
            ApplyDisplayName(www.text);
        }

        private IEnumerator ReLogin(string androidId, string displayName)
        {
            string url  = ModEntry.EconomyUrl + "/register.php";
            string body = "player_id=" + Uri.EscapeDataString(androidId) +
                          "&display_name=" + Uri.EscapeDataString(displayName) +
                          "&token=" + Uri.EscapeDataString(_token);
            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(url, System.Text.Encoding.UTF8.GetBytes(body), hdrs);
            yield return www;

            if (!string.IsNullOrEmpty(www.error)) { Ready = false; ServerUp = false; _connectError = www.error; ModEntry.Log("EcoHook relogin error: " + www.error); yield break; }
            ModEntry.Log("EcoHook relogin OK");
            ApplyServerBalance(www.text);
            ApplyDisplayName(www.text);
        }

        private void ApplyServerBalance(string json)
        {
            string coinsStr = ModEntry.ParseJsonValue(json, "coins");
            string gemsStr  = ModEntry.ParseJsonValue(json, "gems");
            int coins, gems;
            if (int.TryParse(coinsStr, out coins) && int.TryParse(gemsStr, out gems))
            {
                // Detect unsynced local delta vs the last server-acknowledged balance.
                // Positive delta = earned offline and not yet sent to server.
                // Negative delta = spent offline and not yet sent to server.
                int lastSvrC = PlayerPrefs.GetInt(PREF_LAST_SVR_COINS, -1);
                int lastSvrG = PlayerPrefs.GetInt(PREF_LAST_SVR_GEMS,  -1);
                int localC   = PlayerPrefs.GetInt(COINS_KEY, 0);
                int localG   = PlayerPrefs.GetInt(GEMS_KEY,  0);

                int deltaC = (lastSvrC >= 0) ? (localC - lastSvrC) : 0;
                int deltaG = (lastSvrG >= 0) ? (localG - lastSvrG) : 0;

                // Final balance = server truth adjusted for what hasn't been synced yet
                int finalCoins = Math.Max(0, coins + deltaC);
                int finalGems  = Math.Max(0, gems  + deltaG);

                if (deltaC != 0 || deltaG != 0)
                {
                    long ts = (long)(DateTime.UtcNow - new DateTime(1970,1,1)).TotalSeconds;
                    string pfx = _playerId.Substring(0, Math.Min(8, _playerId.Length));

                    // If coins and gems moved in the same direction use one transaction;
                    // if they moved in opposite directions use two (different endpoints).
                    bool coinSpend = deltaC < 0;
                    bool gemSpend  = deltaG < 0;

                    if (deltaC != 0 && deltaG != 0 && coinSpend != gemSpend)
                    {
                        // Opposite directions — split into two transactions
                        if (deltaC != 0)
                            _queue.Add(new PendingTx { deltaCoins=deltaC, deltaGems=0,
                                reason="reconcile", matchId=pfx+"_rc_c_"+ts, isSpend=coinSpend });
                        if (deltaG != 0)
                            _queue.Add(new PendingTx { deltaCoins=0, deltaGems=deltaG,
                                reason="reconcile", matchId=pfx+"_rc_g_"+ts, isSpend=gemSpend });
                    }
                    else
                    {
                        bool isSpend = (deltaC < 0 || deltaG < 0);
                        _queue.Add(new PendingTx { deltaCoins=deltaC, deltaGems=deltaG,
                            reason="reconcile", matchId=pfx+"_rc_"+ts, isSpend=isSpend });
                    }
                    ModEntry.Log("ApplyBalance: queuing reconcile deltaC=" + deltaC + " deltaG=" + deltaG);
                }

                ServerCoins = finalCoins;
                ServerGems  = finalGems;
                PlayerPrefs.SetInt(COINS_KEY, finalCoins);
                PlayerPrefs.SetInt(GEMS_KEY,  finalGems);
                PlayerPrefs.SetInt(PREF_LAST_SVR_COINS, finalCoins);
                PlayerPrefs.SetInt(PREF_LAST_SVR_GEMS,  finalGems);
                PlayerPrefs.Save();
                _lastCoins = finalCoins;
                _lastGems  = finalGems;
                Ready    = true;
                ServerUp = true;
                ModEntry.Log("EcoHook balance synced: coins=" + finalCoins + " gems=" + finalGems
                    + (deltaC != 0 || deltaG != 0 ? " (reconcile dC=" + deltaC + " dG=" + deltaG + ")" : ""));
            }
            else
            {
                ModEntry.Log("EcoHook: could not parse balance from: " + json);
            }
        }

        // Applies server's canonical display_name to LocalMultiplayerNickName if different.
        // Called after every register/relogin so all linked devices stay name-synced.
        private void ApplyDisplayName(string json)
        {
            string serverName = ModEntry.ParseJsonStringValue(json, "display_name");
            if (string.IsNullOrEmpty(serverName)) return;
            string localName = PlayerPrefs.GetString("LocalMultiplayerNickName", "");
            if (localName != serverName)
            {
                ModEntry.Log("EcoHook: name sync " + localName + " → " + serverName);
                PlayerPrefs.SetString("LocalMultiplayerNickName", serverName);
                PlayerPrefs.Save();
            }
        }

        // ── Progression sync helpers ──────────────────────────────────────────
        private static string[] _upgradeWeapons = new string[]
        {
            "AK", "M4", "Deagle", "Rifle", "AWP", "RPG", "M67", "BallisticKnife"
        };
        private static string[] _unlockWeapons = new string[]
        {
            "GLOCK21","MP5KA5","UZI","G36K","AUG","M3","M134","G36K1",
            "RAZER","FRF2","M1Carbine","MiniCannon","TeslaP1","MilkBomb",
            "CandyRifle","ChristmasSniper","GingerbreadBomb","GingerbreadKnife","SantaGun"
        };
        private static string[] _armorKeys = new string[]
        {
            "BodyArmor_1", "HeadArmor_1", "HeadNBodyArmor_1"
        };

        // POST local progression to sync.php; apply server's authoritative merged state.
        private IEnumerator SyncProgression()
        {
            if (!Ready || string.IsNullOrEmpty(ModEntry.EconomyUrl)) yield break;
            if (string.IsNullOrEmpty(_playerId) || string.IsNullOrEmpty(_token)) yield break;

            long updatedAt = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            var sb = new System.Text.StringBuilder();
            sb.Append("player_id=").Append(Uri.EscapeDataString(_playerId));
            sb.Append("&token=").Append(Uri.EscapeDataString(_token));
            sb.Append("&level=").Append(PlayerPrefs.GetInt("CharacterLevel", 1));
            sb.Append("&exp=").Append(PlayerPrefs.GetInt("CharacterExp", 0));

            // Weapon upgrade levels
            foreach (var wk in _upgradeWeapons)
                sb.Append("&wl_").Append(wk).Append("=").Append(PlayerPrefs.GetInt(wk, 1));

            // Weapon unlock flags (only send if unlocked; absence = still locked)
            foreach (var wk in _unlockWeapons)
                if (PlayerPrefs.GetInt(wk, 0) == 1)
                    sb.Append("&wl_").Append(wk).Append("=1");

            // Skin unlocks
            for (int i = 1; i <= 33; i++)
                if (PlayerPrefs.GetInt("Skin_" + i, 0) == 1)
                    sb.Append("&su_Skin_").Append(i).Append("=1");

            // Armor unlocks
            foreach (var ak in _armorKeys)
                if (PlayerPrefs.GetInt(ak, 0) == 1)
                    sb.Append("&au_").Append(ak).Append("=1");

            // Equipped slots
            for (int i = 1; i <= 8; i++)
            {
                string slot = PlayerPrefs.GetString("CurWeaponEquiped_" + i, "");
                sb.Append("&eq_").Append(i).Append("=").Append(Uri.EscapeDataString(slot));
            }

            sb.Append("&current_skin=").Append(Uri.EscapeDataString(PlayerPrefs.GetString("CurSettedSkinName", "Skin_1")));
            sb.Append("&current_armor=").Append(Uri.EscapeDataString(PlayerPrefs.GetString("CurSettedArmorName", "")));
            sb.Append("&client_updated_at=").Append(updatedAt);

            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(ModEntry.EconomyUrl + "/sync.php",
                              System.Text.Encoding.UTF8.GetBytes(sb.ToString()), hdrs);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                ModEntry.Log("SyncProgression error: " + www.error);
                yield break;
            }

            ApplyProgression(www.text);
        }

        // Parse sync.php response and write authoritative values to PlayerPrefs.
        private void ApplyProgression(string json)
        {
            int level, exp;
            if (int.TryParse(ModEntry.ParseJsonValue(json, "level"), out level) && level > 0)
            {
                int local = PlayerPrefs.GetInt("CharacterLevel", 1);
                if (level > local) PlayerPrefs.SetInt("CharacterLevel", level);
            }
            if (int.TryParse(ModEntry.ParseJsonValue(json, "exp"), out exp) && exp >= 0)
            {
                int local = PlayerPrefs.GetInt("CharacterExp", 0);
                if (exp > local) PlayerPrefs.SetInt("CharacterExp", exp);
            }

            // Weapon upgrade levels — take max
            foreach (var wk in _upgradeWeapons)
            {
                string val = ModEntry.ParseJsonValue(json, "wl_" + wk);
                int sv;
                if (!string.IsNullOrEmpty(val) && int.TryParse(val, out sv) && sv > 0)
                {
                    int local = PlayerPrefs.GetInt(wk, 1);
                    if (sv > local) PlayerPrefs.SetInt(wk, sv);
                }
            }

            // Weapon unlock flags (0=locked, 1=unlocked) — take max
            foreach (var wk in _unlockWeapons)
            {
                string val = ModEntry.ParseJsonValue(json, "wl_" + wk);
                int sv;
                if (!string.IsNullOrEmpty(val) && int.TryParse(val, out sv) && sv > 0)
                    PlayerPrefs.SetInt(wk, 1);
            }

            // Skin unlocks — union (server sends su_Skin_N=1 for each unlocked skin)
            for (int i = 1; i <= 33; i++)
            {
                string val = ModEntry.ParseJsonValue(json, "su_Skin_" + i);
                if (val == "1") PlayerPrefs.SetInt("Skin_" + i, 1);
            }

            // Armor unlocks
            foreach (var ak in _armorKeys)
            {
                string val = ModEntry.ParseJsonValue(json, "au_" + ak);
                if (val == "1") PlayerPrefs.SetInt(ak, 1);
            }

            // Equipped slots + current skin/armor (server is authoritative for these)
            for (int i = 1; i <= 8; i++)
            {
                string sv = ModEntry.ParseJsonValue(json, "eq_" + i);
                if (sv != null) PlayerPrefs.SetString("CurWeaponEquiped_" + i, sv);
            }
            string cSkin = ModEntry.ParseJsonStringValue(json, "current_skin");
            if (!string.IsNullOrEmpty(cSkin)) PlayerPrefs.SetString("CurSettedSkinName", cSkin);
            string cArmor = ModEntry.ParseJsonStringValue(json, "current_armor");
            if (cArmor != null) PlayerPrefs.SetString("CurSettedArmorName", cArmor);

            PlayerPrefs.Save();
            ModEntry.Log("SyncProgression applied from server.");
        }

        // ── Per-frame: watch for local PlayerPrefs coin/gem changes ────────────
        private int  _lastCoins = -1;
        private int  _lastGems  = -1;
        private float _watchTimer = 0f;
        private const float WatchInterval = 0.5f;
        private float _retryDelay  = 0f;   // backoff when server unreachable
        private float _retryTimer  = 0f;
        private const float RetryDelayMin = 5f;
        private const float RetryDelayMax = 120f;
        private float _reconnectTimer   = 0f;   // countdown until next login retry
        private bool  _reconnectRunning = false;
        private string _connectError    = "";   // last connection error to show in UI
        private float _inboxTimer = 0f;
        private const float InboxInterval = 60f;

        // ── Main-menu IMGUI overlay ───────────────────────────────────────────
        private const float ECO_REF_W   = 600f;
        private string      _ecoScene   = "";
        private bool        _ecoPatched = false;
        private bool        _ecoDbgLog  = false;
        private GameObject  _goHelpBtn  = null;   // hidden ? button GO — position anchor
        private Camera      _ecoNguiCam = null;
        private static Font _ecoFont    = null;
        private bool        _showEcoMail    = false;
        private bool        _showEcoAccount = false;
        private Vector2     _ecoMailScroll  = Vector2.zero;
        private Vector2     _ecoAcctScroll  = Vector2.zero;
        private float       _ecoLastToggle  = -10f;
        private string      _ecoPinInput      = "";
        private string      _ecoPinPassword   = "";   // recovery password (set-credentials section)
        private string      _ecoClaimName     = "";
        private string      _ecoClaimPin      = "";
        private string      _ecoClaimPassword = "";   // recovery password (transfer section)
        private string      _ecoAccountMsg    = "";
        private Rect        _ecoMailWinRect;
        private Rect        _ecoAcctWinRect;
        private UICamera[]  _nguiCameras = null;   // cached for click-through blocking
        private bool        _nguiBlocked = false;
        private GameObject  _goRecordBtn    = null;   // Recordings button GO — anchor for settings btn
        private GameObject  _goAgreementBtn = null;   // User agreement button GO — anchor for mail btn
        private GameObject  _goMultiplayerBtn = null;  // GotoHall GO — intercepted
        private bool        _showMpDialog     = false; // show "requires mods" dialog
        private bool        _mpMissingCnr     = false;
        private bool        _mpMissingStg     = false;
        private static FieldInfo _modMgrShowFI = null;  // cached: ModManagerHook._showWindow (to trigger open)
        private static readonly string _SettingsIconB64 =
            "iVBORw0KGgoAAAANSUhEUgAAAwEAAAK1CAYAAACQM+LCAAAAAXNSR0IArs4c6QAAAARzQklUCAgICHwIZIgAACAASURBVHic7L3d" +
            "siQ5jh4IeJzMyuqqnv6Z7dFIfaE1zYXMpIt9hX1fPcGa6UIvobnY2b2ZNdPuqE090+quqqw8Jxx74U53/gAg+OfucTK+qpPh4SRB" +
            "EARBgKR7ADzxxBNPPPHEE0888cQTTzzxxBNPPPHEE0888cQTTzzxxBNPPPHEE0888cQT7wQIAEBEuH3/T/8J/8vv/h7/5g//dTqR" +
            "ryeq8V8H0f2PSh3/Eb4+aHJ+j/IYpVc59JDlEWPiiesh0+9/DwD/gUsY0a+ddPDv+5A5DKx8K1Ha9p51j8Kj9WcOFpmPbPPQPrfa" +
            "hY7zzQBZ/fSvvyUAgD//v7+k//0//I6QiG4A8ALwTy9/+sd/+Pjj//mff/nDP/9/388//fF7ovuHF5bM23b1IVvlQuE1m+8tm8OG" +
            "fE02vAUf9lJygZcgn3f3tVfbNVjr4Hucv1tbq0Itr1BZvGz0td7gsObupUISkjbWSrcUb0ltonyKZDBKf0fIpZXXo/qqEp7F7s3p" +
            "EVbKjEQ/jdy9KXlfzurbEt4LjdPoTktExhjwF/GLgDf2shpqlYX8qCjom0sNph6Q+t2TSfc2Z5yF6uE80g6UjHUAVafekgsVNCHN" +
            "NN9v83y/0+0+wfQDTfc/vwDADf74x2/g43ffAH38fn67/S0S/StE/Fua4RPNABMAABJOhDTjHSeaCOAOMAHMAAAICNNMME8IE9BS" +
            "5W2t+o4ASAiIEyD5TM1wxwmQ5plwmiaCu5e21Uc40UxL/hU4IdBM26e7ghmAXhBwppn88hTWG9BGIQ2XNHS8AgARAs4EtNYbNgZh" +
            "muk2A8KEURohTEi0fvrSge0eIQASeHni8mIaEQES3qO2IBESIi2fEKUBEgK5z50nWtuJtH0udxe4e0HptV4khOlGPr83ti33VVci" +
            "GcKEQECLLIGRb5x2C+Wz8rIXnOAW8Bn32crDDN4nEvh7YLNfPtQjVgZJGqZ9do/701f8yRtLEr9MHi0tkGFGD93ntBcS9e/u2gkE" +
            "BAirHrnWcDoGQlqgh44WzZ6u3VL5JjzNTJ9F47uAJzYN52BY3TheLGP4HhD3amXauY5vmG46TQAAui+6AAC0jpMJAGaccMLVjtJy" +
            "7T53cku5CWfabK3Xrzf3HQCWsTQttn/L6+kbTns+d72UpyDdz+Pnddd3AIAZ9jE/r7q+cAwwIcBM8wxIExLcZ0AkdKpwR68fiJZy" +
            "TltjnuYZ3XigrdwESITLHLjaxGnLtNs98O75/Qnzfg+icmvZGQCIgBABiZbcd9cvNOGm1wgEjm23ey/Rntd76Nc377zS2paND6c/" +
            "SltcHkShLRPeYIalxLSOF8DJyZfAa0vUL758N3qrtnl9dfd0DNGdZIB9pp6W73v6BNMMsM3bGNoqoDmVQcITeH3ut8WVczzh2l+u" +
            "PkW+Uyy7TH+ueRZdcXP6/rkk7+XI63NEQrgvTiAi4T2mjQyfS/ouA44nJAS4wQ0AiOZdf4N+ua3l7tuYmuP2Mv4GK7vt/m2RLy59" +
            "dwO/zydPBrsNvxEsskNPV9bvSznvHpEvWnSyZGXn8+q3wZ97Yp3eygFu92J92vT+BgB3b1zP+9i+rbYfpuU+erq56atnW+9IiPBl" +
            "nm5f8A6v8zT/geb5vyHR//Mt/PlX333+cP/u8z/9X7/9yz/8H//u53/5x7+7//Snv6P72/e31e952V1wWFzCebNHMBEEI9XZKZj2" +
            "LxpCNzPCzFwt19N6MW33yMs3+55rngexXlJv+QmUyCjF3utMVQDLvFaMef9XLO4nzFHa5F3x/ZXcFbt1pxV5M1IBr8gEYDqExmXy" +
            "x1msEZpM5+gTlj6IRRTnMWNKLxePooJWH6zWVx8Vm1gMbd5io7lKQjGm7V/0xGRRjHzts8drDVeAyN3tBInarKd7/rQ/lnZ7XMol" +
            "L5vFTMu2Q5fpJFyX8jOzt5wF3hJIGfWzrwP++Nz95pjXgOOioWvQSYh0MuE9jHGXW4wc/LzRrSJt5zKLJndzArYsmx+2fkyTT8Ai" +
            "PE8eES9zkB5lZ8njWr+PKUg38YN77bxixTczEo+T/cAkA3We152AfAXS0EQUeBQ8hoTOzItPZwjTHNOeAv4lw0U09bI94s0vLtqV" +
            "dEJlNU5EKc04/8y+Nznvc2yqN+tNzb9bWZpxBqCfEOEzzfQTTPCPSPgPLwAfph9vP3+4vcI3t7e3X8yvn387//zTv75//uF/hfn1" +
            "VwQAt40hWgW7B77r6sLSW4mljJdzpQbLjMfSDf1lCrKkLkip9fPrWyYUVWG9IIOCQCYMB+LxsIbRAOBPQ7qk1GbM3oUpCJCtJqlG" +
            "cR1K0qhnDIglEJiVtLT2ZPTv6bhL0i35bAGS0nf8Z8Acy7HjiedUTPT4Pj4QWKyqvxghQAh02YxecIzgj5q69tFWdHcwAlh9yDm5" +
            "WNquTuLMvOP4YPrt7pyfPn62jCx9jMSNAb9o7AotaA5t3FoHU1rkTyynMgOc7UrXStJJkpKAxcubdLG3cM4I+w4Nsbu/qxYhcWq9" +
            "hrH8k8vOLE7FtLzLeNwb1txCbG2I9Mwp1uR8RAC6rzdwSd9kZxYeM1eRsMTAtSNStW2MShmkpLiSxJNMgzedMZm0FRkfXyAa+kFJ" +
            "cV8421J3LIjYvri7TOSiyTBmwKCSLHHvYy/nBwghE3dgEPEp+z7SwmYESmmW64anU77928Z8ON+GRd0uG78QAEh3oukHAvoRiX4k" +
            "ws+E+E8v8IcJ8XucEOF2p88fXn/807evP/7LL19/+Off4P3Lr6d1Sp+nhXDg1uPiZGHkGC5BY9ysKWJc08qY+zCPM5yua+5eXn+6" +
            "378wKwcIfKdtkVV0K5kAOb6Xzlo2a/bEO622cqsP/RLRhCZMwBArvJ9IclqGro/FgUsHAktb2xrwLim+mYhwH825Ntw3XlhrpIeR" +
            "iuPnJsi9/t1JQPJkj2k5x1PCDPvVczRih02C5GdhdO1uxYqCO6+bbhLBhPLwCOvL6018SmojqY0zrm50OrhxHBXUJmzJuQ8r2VnV" +
            "3F2xRpanOyQbBGOg6Qw3JFamlhMwthVPPTbhdo+QTVflVuj4eOSDwpt6eX3Kqau054WMYrrRgl4uh2Csm/qbqTcpR+G/vhonxaPV" +
            "/2SSjVrKzE8ybYYtTqmT4Yie7Z4AfV/Ri5pCvYrmCr8bKGmU8G29xyhsvPHF22l+vkqd2iSSynAkMBYOk3qYNiMovRXkI75vOfgK" +
            "Lyzy7ZTiDPKOonn5wK2SBquqqVMu9q0FTNAQ8mRbwNjbJfmJ4VjXKPlu8maVt0Vv8rMm3AIQr2+EdwD6OAF+vCN8wBm+pwk/vSwz" +
            "xc8T4O1lfv3y4e3nH7/98sP//OXbj3/6Dd6//OaGMxAQ3GAGRIAZ/bG/BAGu0uW7855S1tIb0sS93ycIP/2rmRWAu8V1ROyVKEYm" +
            "+CJMjZat2GyK1SboxzcCORmcIR+49ZUxAOCphF8je5Aaa23QySCOuJ/G2SFxL9Bfh+Plm+qfn8Y5kpz7AMFKWj6gqoOuH14wAMLK" +
            "SFKoQJ9naSyVNS416nF5Rs/81dMs5fVbYh4osAxsqWjiRO/flnm9CInqY/Cxj+Ldxd2e0WDGjeT2a3KU+1brhyhvk8DWwjM/PgPr" +
            "rjhtHAsY9Kffw8tFHdtk2MkJJ/6gbPLVmx9jhuJGS1OsRHoDykPXVzk3d6yHqpfYwRu9WDs+FA3UJ7k9ZQtOUh5Yp3Gyz8XpnV0n" +
            "LfkrqlDTTdl5hecJxGsMqhnPLUhw8kozsr6i6IBgwHtAT2GnfF4ynmZJ6EsIlqm9guxAhmDnOnD+Od8jkmogOwQAeAOCD3ekD0h4" +
            "m4G+R4JvXmCaEH/G6fXD2+0O84f7l5++vX/5y/dvn//8a7z//FsCAsAZAJc1YkQ3sBeHH10QMFEYBGScVt+Qp1JJJ5N4BS8pJAUT" +
            "cZqQTytuGsSNXoCmkht3eqZqED92ArL5IcNPGvluOWIJVYKxXzMsSsmyXJUJViNYhdoJyDR62ZtCqFnMAluCONkVrHBC2DzuaJTs" +
            "LDB0GEfRjJZ+TioM2xHflQ8c5+x0nNuelm1eL98o1rvA8V3mKitvu+wwuOdujelnbf6JGhd9hkOVuA9Tp4W+AvsljeO8BcEwfRcU" +
            "ed/tIhPawWSJZRf3DztGfd65Qhbe1DsQd0wpyaK04uKGOS3AcJtWECgGvDMLCyYfhMkklsssQmVB0aXQyiQtntPSAb9d2sQAQPgG" +
            "AC9AcKOJJgL8bgL6FLwLCeGO8wzwNhPe74Q4EyLOMMMME8xeOOhi62WXAIFgeRTbBQEUcaE542uDlJEuplFyJ7iKj0ZpdWi31Gmy" +
            "wfkWVxylShhrZzitkefDkVfGhnmA+oXishn5hxO3ntfHsDDCn3CVSiTZyHIlW+QlpReB7HqWJOZ7XevbFtVMFoACDy+S3WoNzfUR" +
            "r3VC1oAnictYDv7KmVhD6WTMMYb7Da6f8+PX3v7SfFpZ7tiqtQ7VnsRzrsCFFhRo45k4geb6MJs3mtSDpMzAinU5Kc8X42/4YJZg" +
            "Y3118qBF390pE78dyzhYMhKATd8zk1qYHOZNxyEzRkHoS9wbGLNptReC0TIlcy6TkaycV0rkbI+kp76s+OTwS5FNY/SMAXtKzOs/" +
            "lNJ4aumV38CkI7wgxTFibaPWcT7fnLXiGr2KS7NtLiVkEQGWVwzhuiKECIR3ZF6IOsMMNAPciWBaKyOg5A2I+87L7vwvbzALGReG" +
            "KLdUxVr3xPC7K8OoMA/cLVfJzNTi4qQUSuYRW/V2/gIbyKWJqziCKeC6SChrm6J19O0JD8aBngtZ9nR72BdkawwEeD1rk5pW2tSn" +
            "xRMZprxn9SwzCWdY0ebI+A6/ApmB2q/GTvcmpX1iwMS8LvZZIWOrzTT5rOzodGxzv0hfp8vX7ngPHAZxxVjubS6mr3MKxFtrQsYZ" +
            "NlBR5Swt1kj0kjaGeoYAayDgOU7e5OLigTLkNE6fR9nDaxj3ZWqXrR6DPe9YOmZ7rOmplMbM/6zMMVcBB07rLCsoEIwPm3+k8OY7" +
            "P87pAalnsLCzuEGv9XncLt73ju1ObJ8Sybr+icgtQcAnWA6O3pc3AhKtbwaclweCN0OSeIluAopZpTQPV85xTIHck2CA72BNfIkq" +
            "iZyUDfeyMhpsvNlgc3V0qHMFSfOqbfYrkTvfc3KZYbsABdCkHcqVMSbWCiobyutZu/7mqKg1FFbvzDEznStkSxwoWx7GB2Lz5jZ3" +
            "xExsCVvHY2Cfo9dOSzxY+CG7XHNWP6nKJKg0Kdt369P8Ur5g+iB/12YZaDmfmU1vGKMBX9sX+yDJLl5xesoVKebf07O1/f66e7ig" +
            "ilJsptIvTdEDgv1m2Jdh59lmnzobKs3Vpg2S6joqYbRvge/WYyC42iKFyY1LM13ubnLMVMhLyK+WltQdub+6ZjEBADM1xLZXlAkt" +
            "wQCRG6HzGgR8BoAPy++OzTPAfXbBAMGNlldxzrDsDCyVO05oj/Ddd2kqKHnI0HebkrhDo5OKbxvsfCRhQrdBxdDsNFzUO1Y6mvGT" +
            "+ZRTSzjhFT+/ct5LjqNQK7uWrHExCIoOCABISVMLltcVGs3yVrUG2yWTtcn57oTQ7df5MdMsYLxmrI8csyU6GL4sJWPPgrxMhoYx" +
            "GjBkyEjxTUMAJaK2M6I3zQRzLkQjlH2eRyQsVldCwT4m9tz58ds3AOD5qavtGFtz/HyrtauWn8TnIBAHd/BMauAHF9aUjGEq1zNt" +
            "xdYrKaypb166I+PtBHwE95Ngd6LlONC8nPqfgZZXhNKyATitElnOAe6OfygTr0aF4+BJf8Y46zeS2ozV26yl7nbqZKyISXBRtn1d" +
            "vI0htS5t8mNK9jVcthyXDQRUx0EVbJQXqhu5FOU1Bt1FAS3+S/8AgCMRT9zWcZAxReX1Z/ojKwtTX+5t3DZkpZwZZ1a1NaZ+LLmr" +
            "gDf3qe1jCIt1aWbZwOC6QLYXUMakOA2tHaR2a5SY6qRtQFHMSEkAwOVtMaBr2e2ZAE98qaNl+c0Kvl01Qzcxr1E7w/EcWReWzxJB" +
            "K0mCEmVtlDd2+F3ejlB0YvODDXmr604v1ey56k3yoZSSanNKdEQYiGxuqdLg/mJwtKyLPq2v7HG6Q/s1whTuBAB4x4EIAGaCOxDc" +
            "wP0AEMI0+U8tr1HM6kUQ0f4g0FapYPHjmwQB3UxW7gYPbelAo5mjG1RRM/zQ+1evUlEVIW8tN2t5FNI6eDVl/I3MfQAwIztTYnip" +
            "Ogw6FZtd0ZAbHrUTtiEz+6CtMmOW1G8Zv+h1JtcF5vmP6ZB87aEFYLXF8cbqB2drPDuuMU+FsizIy9q+Qmc2FxTk0jc+MO5XmRE5" +
            "oEf/I1tvmrQkJi+0kC+Wbwan0QRLXsnpIQh3BOJx4XmLzS+zsHpN/phA9FngS66+i5Qvy4KYwepIZG5b3ZMWvh2iZWTk0mJ6Z07A" +
            "hXZqLZJCXBFLFV/64UkuZzkjGWxVMwPS7581n3P8t+8Ay4/6YfxMAKzHge4A852AZoIbLDsBhOQR8jQE15vbHOlbX4OV3rIpeQcH" +
            "AOyNoQGAq2BfxTx7FTtw4SN/3ube53NezkkfjXRuTNKVw3t7IpvPJk1735XhyAAAAPgVvQ6wDt9l21bX7YKF/eBrD1h26kRb08Np" +
            "KMzrm/usz5xzjoQbuXT/duB4ZzpS2wko8CEimnuhRJdq+2eEwc3ZrOjIj+/ANv2gXk1DfV6Jtt8yyJFRbXaOlSSDvRMO78tcfeKO" +
            "CZzvsFSirO+ihOjYm0ZCfrKolCE+b05XyHMdlkV9hNkLBObV3swAzE4ALM8EvM0I0x3gjQBwLXmbAKYbwA0AJtz/NuMXbQVaW2MJ" +
            "AEi+ka0il5ZsqBQ4Jr2gkzrBfRaclQcd+6eii+wSw+sOhxxYf3qZTzPSNGU/efLJ2TZzcwa1w8knvy3e354Utb2krDW/ZsINzAVj" +
            "pLZ/VkdzoyNWEBeTGSxax7oANNGVnHxMiPZgqKBfOV5NbCSZDHZacX/sFefzdtWXKwUC1kWCHvXEO4Z61r51G7O5hRMCgPuMcKd1" +
            "cX8LAFa7Qsu7PYOdgPsbwDxPcL/jshsww1aKcIbbDeG2RhTTBPAyAcAEcFsfFN4GDnkcJVzG7vw+ArJOtfRKKM0rkfjI0cyVk8kU" +
            "YFEVXVnKCWdLCI6C1H7zAmzTUo+B/gPCXxlkjwlYrD9/wBYsE0z3xfO6odUMyxZ9ff2WgApFmj0cmyZRYSgfnha5rMWo5S1bjmz8" +
            "FJt3zSmI0pIjSdExCCv2n9BZbHpQXB2ESm+JPKC+A32QI8gGOriPEe4ISU+bkEO4ci2P36ScgdeyZqC94bk6j5wgo8A1r7pXiQpW" +
            "tAQH2u69Ccgutph3i1Urnt7YXe7F9szrPPA2L4HAm3vhDwDMM8F9PfY/AbcTsEYM9xkB7rhGPgQzznCjCW4EcCOAlwkBbsv5uWlt" +
            "GbrtAFH6SgDAN5ErGpTL4rIBwF7R4Y4vBXbRkh2c+ma3UxvswHsLAADCNmGtfNQlND2yCOpXy8S588lHBQC5uvoUqFfcs3coYnPY" +
            "ZbdiBJjKa/rVEjeztw3113ZjqAMUWktRPyodefJp2wdl777PqX0kkgPHCDbVL5nbKvlthdrOFx0eAITVG2R3rQCgy3MnzU1KVxSy" +
            "ZGtffrPWR7Q6+rQcAbrPAK93gNcZ4O2+3L/PE8y0HPffnwnwMM8uIyzUYFFBAoKZZphpWvi8LQPlNkG6A8AFvsl3ii6FyEcpl0Wt" +
            "l3JYAPB4iBYH+AwVg+drEGOTXaneSw/rz+88ldAS0s7uzDPqPzsQ8LCtxJ55dCp7oxNda6Kx/qZh5ulAYicT/WgJAK6DC6n9ioyz" +
            "XbDwVbkpZIeZcMFOwhNfh6gSvxi3QHEJBJYTO28zwOuM8OW+BAH3dZF/eQHQ4stPfwRYjgPB+jsB/h8tWwfLH4R/a2X708e4rX/s" +
            "ncDvIwVvNvMDAAr/soFEiZAkBlrw4NpWxr4fb5qzPhFjuGxyFYzvnGs5BV8vzlw41G/0ItwhjcveTXDSKn1lANDKTg+g+jXBeJ5b" +
            "nASdrHNFWmioMBtK+kqNaoX0eypcF0OWlhGpWMkzKu+eq3Vn/ueZ4O2+BAFv9+X61QUA991/Fx8MptXJB1qOMCCsfwSARHAnXH5D" +
            "YAsGlvNPbjMA3bYAgXfXb8DyPQ4ArMLxvyKTntbXAwoTBcWa0dkYlKwamp94B1jbbZgxLCTfgQFkFwGL23WEN9UHllP2IqzyuYRX" +
            "dGHQsSu1XbvjIIe/iE7VDic/byTPIijVFDVJsbH+yjZCqBvxPS7NfyiyRBTV5k6kVJPvotvTqDk+cV443uYVBCq4/duT1bOi4vyD" +
            "ECa9FhLZZ8ssbEltXgMBWp/Zvc8A95ngfl+eB3hzb/1cSdC8V5gcB4J5DwTIBQG0OICIyw99IBLM0+r8O6oQTzjMgxG+b84FAIZO" +
            "zQdn7zgAcDQHzOq5bfC6KiNmY95L5PjAgUDvo1OPgqqJ4OnYd8fDqdnJAcDZ47V7NdFAxOiTu5ekYXr/WLR27hVHgRedXTEQKBDX" +
            "rje7nIeyOlwGur5QdF1kMwTesxqaazMBEBHQjMuzvc75Xz/f1tV/t2Pg4s/0mQCA9YiP+w+YP9wijhmWVxAB8AZkv+OvMdjblUMo" +
            "uDpqtTyc5q9c0Z5Z8Ig8P/GEBRfX7dEm4whbeJS9PcK8xivtWr4uaPHITtfteikEfVn9ZoYDcNVA4MHQVyx2fTEHAg0MWrt98dv3" +
            "3wWYvT9HB2Dx9RGYHwubYF4Hi/fnLwW4c/+wP3zgBwFLe71f3YtPhJC9OekiP1/G1ZmAqaa8D6iu3IgNiYj+kQ/+WdrAs9PB8F7U" +
            "blshyq5056XTHGbaxuRQ+Hxald7HZxEsec/Gg+hnqaikZp0h8q3Owj7vucBjsrcFY7Td+afxzmOJbm9nc3vOBO3aRuD33VUCAZu/" +
            "ImK07SsUUbq0O0bGtXagpUYytkVlR+huY+mtgP+jhpQrJrA9R0kvv4XlkQCALwDwBjAt53u2PQB3NIoACBEI150A2N9HGgcBk6cD" +
            "8e7A8rubO+fBKv5KIHX+lYauGUS18yKwMp0h5qqoWHnZQnR4YUxXbH2Q8FQpBbFtPRtd6xKXWOv8rZhuqs/RGcVOwUAJGSpwOpr0" +
            "XlpBUPN6tR7ppTb3A0egpJOlxrbrM6cffURbxltyrNS4qtZVDdDJw8B7xzGaEmWgqZAlbyt62ftRwFiXL8afj9JAQENNM0uOAEVT" +
            "UnHl7A/nKNkTBuqqLa+PXRJopmybf/enLOL18yCudVxhRDOqIAii1rSJr3hePPl1R4CCQrgEA4DLUSBYf1eAYDuHdKdwC8K9s9Q9" +
            "PuA9RrDtN0hM52Fz1gv1rQuOMDVXe0HRMez0qoWE654QHBhL3guhZIye0wrfEh5UZZd6YmmV6OSIcSCn9OtXO6XaX1HpHQAMoWtG" +
            "Ra0XWhxyOIWl0/uuAr0EVUqnIgBoApbYASvNKk5Og94ufw+CNhd8/2FCoVgsA9TrefkjAPzi835jntcC64PBbsuB1jQAAFo7j972" +
            "Xx+bgGBCAsR1nwC3mGG7XtIAJiSY1vuTxzNyxw1K91gElO0ePYy56IdOg6fIOERRbVswWAO/1qtbj9J1+8HQBlS3rVprm08Yr4d1" +
            "hSQDS4BwUdmNwogjAuwq95HjkFlEKFkpHt29FzJJCVD9em306rsDxkR9FfYeKarjsKNC5aDkwnf195tu9R8Z/2TzkVe/3H9V/339" +
            "hWB/AZ7I7SPgeoYHgWCGCZaX96RvB/KJw06caP01svXpYyJaX0VEMM1LJRPuB4VwdfLBOfrO6UeC27T8yvANlxX6af1TF8VkcZqB" +
            "LiISi15Uc0aj1jr2MLIYXrYFajUcdGr8dsvaAjmfzBExifVTW5eTLIcEAhV4qBl/1Ip/QwfEK6jdTeMYW0s9HQBRh06eJ84+/mPB" +
            "2Q74iPpHHL/J1SctkpXS6QRpka+cPww+LIWLA4EYXfrHJkzdxdxde+fQEwBgZLyWRfTwAOJiizEo6/9mV3gKB7f7GldhEHD3K6Ot" +
            "mAsEZgLAebk743KN6Fb93VMCtK34747/8ofTEgS8TAAvEyzRwbQ3GJPWSqjvTbc7kR6jeQYARdkGWfW2sWtZNR3N+DKQVUe+uY6Y" +
            "XlmbukrgkEDguhg215wJrlEP0Jcbez14faggEi7VPyOeUysiOaLvLDRHrbp7gUAOPasvnfPtMZKwgGYMBHIQyTT3T2MA4KWFp2yW" +
            "RfW9jv3oDxBEz17gtki/LdZ7P957J3ckf387UMr9HPAUBAH3NXle3yXkv1N034Gg7fFiDFjb9g6CAGAPApZg4TYB0A3CRc1pjQVM" +
            "UeFFLB2HC7PG4kEmuvIdAnM02Qdnr3odcESB+dUPP/GrDAQkib/jJo9HpeCSYi2d8CB2McEFFO/0F1WcXf8IFPTrMBXoINfc8th7" +
            "x36qZv+bnbPues4L+NyCdSy3bffA2wmY5z0IeKP19wG8nQF+N4BggigIuAFsQcLO5MoE7U6Ai1AoOP6ws+lHMIgAM+zPAPjvKt2C" +
            "BIL1YWPKPCuiq7dZvSgeLB2GDalfh+KUtwSN9zsb4JjjemEs4/yvKmv89MKlO+TdISfpC/hj9eBU6YjGlNRB7OUhMI+0YMI8CCcq" +
            "3sg56HTrZmHgYQd8ChS/tNK1v3KzO4z9I2er00LyPp3DvrxVc/9Rr8UvdoN3qcP3p30f3gUSAKGf7ugvOwEEd8I0EHCL9+ua/Qzc" +
            "LwbDXskeuZD3C7/emqC7h/u12x3woxhwuwIAMM3Lgwn+MaFpDQ5uJKjIVvcOtivE4xLppZ+9eeyWTEiu97qNAwzJHjm+DrTMNUNP" +
            "H8xKRduALGEiZ9qu5kWVo9t4eXBYdfHhA4Gr1ndiABDXaw4Gjg4ERpHhBH5Q286IqUwMXGyQ5+x0kfwGCHv3IAdHjUOKCTnE2+EK" +
            "vnul/n3G5Vd87+5hRHXItAAAIABJREFUXpdLmDVop0Xbd+enU3Bkf16d+3m9734A2PEQmyPvdwLiChc3yjHuqKT6T9EOw5rPa4tz" +
            "THHdEVjeDoRwX48HzTN4r/DkAgFDAOCyYXxjMIomr2j26jIGQkKHr5icvkQjo9gJC0LvqGFNAcD7wkM7t40o7eevWVZmPFgAUIUL" +
            "20kOIqtPhX4YcF11dgDgk1Z+3akNwwKAWqr7Yfl5DQDuM8Drff+7z34JTJnx3OzgWBEA0PpAcXofgCPl/PHwF4MjzI53ZgV+ObKD" +
            "K/GIvOfk+lsP2y4Ccr8fsPzyMHlMbaILHpaIke8yDKgVwBs92ZIY+vZ5wivVzjsBPmpXALQh+WBzGAC0DGosnOwOWCNnBnNc+1GQ" +
            "+LC2/irtOAI1GlFT5qFlJy9+NeEIOR7hD5/Rt6q9x1yGsfBlfgW913TgTP44vszdNpxxYae9cUBRJR1LdoveSTIH2HcC3G7A9rta" +
            "cxgEBHSYRY94VT/5vmbe3Hdu58rdRyEI8PNRVAYIlt8JUKVGwdXmInmRiXP63YMR2+uMtvzLfoC8I5B2Q5yX4pQC5ci9ai5IivKm" +
            "xShicMwIszj5XE5k8+TTLmGBK8B2K9cWBADCzBEr8v4dJJAM2dJJSAutcxCNXHYM1NEejbOcvt5tPUN2XWG1zS4YFvIeGTgUjx9j" +
            "+6zFRptfld3Cyi3Za8STzMOF9RfrizhPdKKv1duJmGVhU/yqdaTVX0rSoh33Hu1cF2drlo9LA4JsHs9Zd6/Z535UV6SRCQRYfgoC" +
            "9SQImLlcHgNtjsPqMHmvL3JPRs+w7Ai4w0DuAWH38PFSr1x7/PYS8efdS6wR+l/2y1jg+ejT7p63wOYM8jsHLIfYacGnaCQaa6tk" +
            "qsbo6zJo0DMRPJdSoCH1nStTt7sjhbKrojMsbkf6DFEWl2WInmUQyqC3K80zROKXei7y0q0mJmOrptPMbSCjVSkGqIUsWCEtqjRx" +
            "EQ3K2rHdB439Gk2dlf7jzgqTr0RVMSrQrrGe5RAWtqvBGYkzFt20+S9WD2G1TJQzP43Y/QVlvnE+Wcx7q+3N+wL+oiBEb9aEYDcg" +
            "/tPqZG2dLz9OlkZ9EXYCEMituDDTi97VUmt2DzoWxn2G9RkB5+ovmfZB64cAKVdbKqZvF2oKWhjvPv61Np2AFUdNol5oxORNNiqI" +
            "SSvlJ6KTIEgz1GKhaamqElkOG/iTiEjOP1tKqF8rzaelBm3PuQcC4qiP+1UbLyZ+DAUaZL5bmF6BAB9ssxAWF7hsOdk0t0MoylND" +
            "L8FWp6lvjfVzk2NyqyYKoBIdtOQsUFBBjCa73RXRWG9Bhl9TAEBtQc9mG7sMb98TaaWioDHI7jUFJXyuN3DrEwxsWPG0X8KoVoEw" +
            "x4hBgKGsVg7Zb0tfBUHASnt7E5B3BMi9KYg7Uq7ywwUFNbgrx4EAAPaf2E0rNK08MaN2EwYS4B0BiJYfO5gJ7vf9Z5K35wEw+Fiv" +
            "fbfI+xfDrkiaw7AnIRRy/EakMJ2Sixr4PxHdQAol44LhlSAgyTBVTzRKwXASL6gB1a9pHYZ7hVXmKxayldUdBaAF9VhkZAsryLv2" +
            "dT3ljTfsGH2X22ESpzQBNHhCe9FeAYCjxQQCShV9g9SeAc2CkFrarzlIzoRQgV6/Mmnbn9HiIdpHvraSzAUMCF8VGz2MgQJdSmyd" +
            "cSHJRL1WpRkfIpd3A1vf0ufc6n8JxOKdhm2veKfWJ8jWX7AwFBCsdxUSUvYFjxxt3iv2z+3faXkb0Ou8vMffBQAWHrSgiZjrErny" +
            "QYBnBcn7S52ZeFrgWpMuAxARzDPCG6wBwARwvwO8ov9bA+QX2S7CYCAMCLgAwKovmtAXY+a5Cp4GxYYuTPf4MDCCnsxbgMmFMV0Y" +
            "8OW2Ll9Ci8iTy0qHVg0AesykDA3RYJYavBXW3x4297lynSuZ8h/qKxsY8wVNUAOvxkFiXsgQYFGf6JdV9vvKmkkLUp46CSuhgtzN" +
            "7Wu6QuYv2vgkGK+uIEjKBlQk1KFgf5NdCWT6ob1hVsQAQPtxHIkfBFTL+Tn7gB8trC1mbZ1N6dlchcGiVDYriazH6Od1HpDNEnAV" +
            "aCWRqZe3v7owWhYTla/LPePqVo6H0qDdEqQXaT0FH9msyfyJfmoI93pOItre2f/mdgJo3wWIX0YoUtx4ZRaYaqIkyO0EMPXz5ElI" +
            "50McAoD7vL/D9L46/9P6XlHOmQf/HvpOP27XUiAg0Yo5lAKB5OELvyP8NyJxRCImxq/ibFVFF4Y0hr8R/OYCANNEwNzS5gqxztoG" +
            "Wo1QpRXWimVJMoLYxgXjVLSs6IhjxErAAFZfK2hqrprVVQxkRbzs1FWc6KYpr5WfKAH9L8aZNt/XgiPLlN0PH4Z2PZw5wzTNhrKy" +
            "UvOGdeSgjZFSZG0YRQlFgYCTbC4Q6Gu9TTWphpZXlmJbZx0kJXOGHNemN4O8slekUAvI5IIBnoJbgNED6SJ7Yp3TOowPAIO/lGEm" +
            "XVQA0SbHZXcGBL6MJLZrodIlCFi0ZHsrEEUBgF+xoof6okfs7dh7pygIyMFarf9zxwAA8+bUs9MMS393+vd3CMWBAEcrHjK5azkA" +
            "cN+jR5KJVyLNKeduN4wN9QY7cCBV5LpgQAsRZcSyZdMEw2PhS3W0WixZri+NTqKG6mCAMYrBZ6EhV/lgHNogf4Uys3w1BgE+Sqdu" +
            "Vu9Uoyx813RczpIg5zgk/Wsgmh+nTK1i8Ifev+5qte/JtqjsIEp85fQr5VUPBHz7bHHOLAiPi+SoeQ497bfcRyLNYEJRZxwzSu2S" +
            "aIvFrtT7OcdDsU0R5rOsbeGqEYKAFi3J8mQouXtM6aCxiCieX3P8aA5vNUoUjwvsBN6T+Fji2ygrkR3FYPiv7dx/xAuCH/EKnPt1" +
            "YOUDgH0E1swfPoQgYAJc3esqZFadyPvENXsgzMh54saf/+k43f8wKSg52YH8yZ4W3t8HYXbBjVNiIWtpZ2rGzZq29APGWdbreN1D" +
            "1o8qR1eb/JV2lDhzwb0K9dbapQYoBQ5jbf2bKmb4qZ18pLpF5z+6WSK71O1xXwzEDAj9rHASNbtUJsc6tUVacYoTlA7KTdRW/bPw" +
            "JU04cZ7If2VpyGk8dbFOcfKOnSS5jmSMFAbHWXgTW7YLMNUVfufOH+S5cNaO6iBAmGcTXd5ylPdzKX9xlapt1qvVCWeJyYlldjg3" +
            "13py1eMspmxahX2e7R0RGCGI3hTAe34mB7OexX4rFwS4Py8YiO8ldRoWQZb7BNl32fNsB6yGQcANYJrWa/RXIFKIQla84O3XhXl9" +
            "VUMabnKJHX+ExSYmzyYoE7AzVHFnxOn7hzfBRCMo3hFgoTwckNFNM7LR+upMpQMovIOJUeg/4ElotXRGLuDH0yNOrFvfeVXkH+ZK" +
            "M6T9kXOFou+NxkbOxz6Fk1SoTTZhv9qCdz6NF6w6sSMk/QPg9eWaFmmlgVXBK1mTgmCJfBnQnsbQlwKVrINvnJR5p4mtWE0SHVlV" +
            "ZoX9lyNXXJOeUlqXNmZyd6WVRQsfCXUs4CXeCUj44V58nShyeN8kOGKyyq2OSWJ0kx0+krcp0GTTTGPDm8y9idAcBDixRX6KlrcW" +
            "ZUGAjtg3SdPkFLY/2XtMimDDTWkdEPjfFrvH1Ws0Lln1w7Sh5H3bPn1/JKDC94fIizhh5OF+mPcOLgj4tH5jgclVVQDgJ1HIeuL7" +
            "xz68QNYPUhAIJtqDgYQwMxFvHaQFAUk8sWt13AfJ+byE78SjSdrDFiuE+hAy+hf8prm7wkiQtsm7hHsvoMpaXKXK9GuWEz7NakTz" +
            "vMY62GB7srzygYD6Neznah7a0iSEk6OgWzmH1vTQJFeztY4IWrGjggDPvGRttaGCEQFAvtY+dTTrHTN+rDTjPjHZGoTdO+AC0Iho" +
            "OvoZD8js3Mity5JQguXatzTpAQSD3MSp2MLCWSfPQ2OxUjKj7XKveUIt2yEg8H1BANYfT+sr6PisTiKGZFHIGgdGgVzjX7sqYagc" +
            "BABwc0HAZwD4EGZAQKD1FaGbgP3oOO44halFKGRjPHG6DdnXTPNaTbwyAeAZJNo/Yoffz0eQ1p28FYhN0/jmZSCNAVM/I3OZUXw5" +
            "0t8nFPT6i8/vK0GtRnKRb4aWUiW3WlFmCC13bGBXI6QOMlQSGpJw6mKLbwMD94mO0xVfJyuMcYaLTEqUAf1J2fVmMtKMREtQ3h8u" +
            "o+roaCZPSCD/wjC8/BWwbWxHaw35bi13AP0+spVoQG33SGAEkjiDgp3UaTIlxLFlbRRv1eSFOCmBT1RHbclQq/I4y+xtMtZM0w+x" +
            "fdk058aoiCTS8Vk6e6U5egYEqbwGje3GKMxf7IhtoYjMWLPlTTMG66vlBMw5bfk4Jz0EAsDE/07ABDDBNpFE/obuaLKsMIWlssUG" +
            "bA9S9p9RpjQI8Cdj7zr4lO7H5d1w8OinboplWMk5isYEM8mwNQlEMVKWZSAphpNV8BpnoNKB0LJhaqzstiCVWpkJTlhJs8XiKujo" +
            "0EGJozO51cHagp8JATA+G7XxV9rySjczurnFnd4ArnN21vTS3QBWUFJeZVrUnKcM6eQhMQHB8XHa77nv8VgNg/yAksyLzqpg5zqC" +
            "Id+lRqvNVO7zNONxKdVZEl7E4yAsK/crZFKE3PsAjFIESpZxaEjvFENECWk7pDmtSa8qC4eBAOdkNbCg+Cfp/dw8JmmfgQ+tTG0A" +
            "wIzb7TPv+yrIC43XLE9KbgGGYFlITbPJ9DN8l4tKLjF5qeIvBvvLcsERmyIWRM+T85yVrFKPLD0++4EAMXO/s22GAMDd4wOApRJt" +
            "nu5ljkdAm+5jp3Wfq5BPCzLVtC1vmupIYjDP2icXe/CQwyYRsYmdhvM2YXMTCK3/Cr1OSqrFBy4tYADngwf8ZVQtt9jKFlA7aqeT" +
            "jnN9lqgJALh6YmzxI+fIEvM9zbZ/x7wjL8dFUhA01s51pR7LRxgq9rnPkDPWt2wRDBa54rJbvKwtYrp0i67HdWgpVZ57nE7hVwMp" +
            "qw7kVq/ZBc0aLzcqZyUXO5IpP2RXKYG/EtnJfFe/IkZH6WD2Fs/88coGAv4NMy+o9l3MLnopbnhtn3GgYuAFCZp/gM6CuAo2CJi2" +
            "qzAYuBp8B94FAEEQQGk+LhBgr7l8wgRfa6SOhNaFcZr/fVlJRjGvOANVoV1yBfGlKb3GTsmOUw3FTOnY80syyP0j6oSxO5t7S1FI" +
            "drNEyF+xsZIWMBbO2QJrmkacy6K1MU4LvmM8nr00jxj5iUxFab27XpGSqwkKuS41xXpGSXK/umKiDgpxJ13yGfXTKdXHGGG6oSUl" +
            "46A5CEhnkmwRY/7cfGepz8SEf18LxBgewjk3nGer2MrpQIYnl6b5A6diZc6PZ93uJ2fjaqtwhERz6H2JgxH07m16xg9fj1hsR4/F" +
            "9mDwR//BYJMUOVeaSy9LYrMiQrx6uvj7iwZs71r1/5i6/A7WAoE4CAjyaRO8cn8oDKNUc9U5B8EdA9t/hSGis/VJz9C11dzgPuAy" +
            "/ZRLs6RLZeKdkxAI0ltAinlBAHnpINMvkqiNXdBNz2NCm97ZNatdC71KM/XEN8QimTQxKTORczsj+5j0vgueDierLQDIeAvh13it" +
            "UCLCo6RvuxtVTzZWJ81ENOddRkRti87yIlxO79MifQa35BCV0vFfb9hCpqRMduHDUhlns6xlvazxTkD6NfV7siypY1a+59+Pees9" +
            "y1chGrO+ePxVeHDXBQN4t13hTdEcMnTdWty01j0RhL6IIsD9ZTPGia/RtY6rCB4MfgFYtwFwz8YKs9J9avLvMHXsKPrBhRkWyTMz" +
            "x34rjbrYQGCrK6IjGGMN8eSp9nHNaDN6G76yb9VEi8jbQKIwyk3orLLUAgtLU1IfOS1VJBIKupmtSJ9w7FxLbWQdtiCDXod5mNSO" +
            "J83eZB0Ae2/k2GMpCf0k1ooA8RG9bB1bOVNO0fyxdTI2isuZc25NuhUrGkcPU1oa3yFBJYuY3tFdUJz0Nror1SSi2q/2egWhsnT1" +
            "5DgAkFb4S+iWywcLC3mS6OWpB2O8YNyNhCTjLBOckkYrUSp2f4RdPFJf9bfnYx1WyQ/iSdhgHt72yMg0wqL+cU3zAwB/xT19iDx1" +
            "8rO8RI6Eb9ODucn34SeA2xoAAAEQWmdM3UGp9LhFuFeEAqx+/6dPAK/rTsB2FMiFNOYAgPT0yhEdGGQ/MF47wgUANO+BQPwDXiE1" +
            "np04ECCvUksgwCHNFgYg7BzU1fLFMgjbz0XLmzHiIm8GmoORCwQ42ykNgCLXQlkF0H9621rLrhM52YCSLuUfivIlzg0V5iybr0Q2" +
            "Sd6tLfKKmV4Ossyqfo+mS7FT7d3QTKp1/Gzjd/0S5A1mRpnHGvTyA831DRkUCOa31Sk2lM0KYKPL/DBYF7pWmGkxry3spAS6LT4B" +
            "2ngRGWIK+XZJbUkfXVLfTKik5bqx3j/B6FoupInbUn8QAEi+CuvHruVVGx5SCmRHe1nnR22f698SlPQxwgNMORAAzDfn83/2k6at" +
            "UhuZ0pQyyG78muiCAEO95P1JdVCGSH0AkN7leDkS6tED6xwJuuxJ+RMJlSWFUBQ3P+nYakmCQlOpHM1zUNrHvfLV5q8hoiYri7p9" +
            "AoCQUKn/VMIDeRfE8dGArDwq6PXIUwfduB0xFoe/WakTRnF5VgBQRVstVD8qCPSXjFgwIgBg85v9gX47xVpbcuVa7MtenqLvu13l" +
            "ZHk5vbUgfEXoR3Bfcf1XjoNkWIYEqyYFoY6UlVbvNR+ahCvycap/J55YNarBDZTSl0Ru9bx2I31pNiUE0PtX4HQ91i8c6/EYi5ok" +
            "Zm0GR0hZNBN29FOS5uCtriVZ3tRysZeoHceIeqLh9EX5AqMl+ClnSKo/R4mEjuXKZQMBs37IeeO0Xk6zplvxsT1pnObGb5hX1+Ze" +
            "Y12ze22LLcYSgVAo4CXkLbShLN+5YxtbPo6SIbN4pwx5ubbP8w6l/crOowX1aQzEs6A833Vov0F5/R/i9J+723SLYTKVjzyqSfvh" +
            "1gKDFw+R1V1wXCa88dyErWNZ0+ZuW9akX6UHcgOa0W5qnMEfo7TlQ50Rl5ZszZZjZGDhMOUyiA57siRvn/TEfAndAmieROSQxxF4" +
            "XJxzzIoCAO+TL0dbdNnayVsAwFRmpq2cqS5xoKXy0p+5tGrQVnQIAHojUoVMLv+SCdYkio1907/sYGGv41mzOG0Og6yhZwQAFhpa" +
            "3cHQ0exr1vbmbXwvjDkCBMByn7GZ2Xlik5uB6eK5LdXDpjGre65m6lYeEtkdGQBkaATzBkq5smSrEcqCmX9NAYDATTYAYCpQwPcj" +
            "vyigywZl1gqEmvfdQFw43K2YrO9BajDfhFbQp5A+5tfizPp1jUc2CFiwshO3KWmjnW1rzlIxajrWQf/KeDEStjmLNYjflKydlVG/" +
            "ViFHw16HrgXqc7YnBQAW1LOWmyGO4GE8EgPeugyag7+kzqFwPId0+0ObfIv61V8kaW9+MexOhFyuGwMJspPeYPSpLy+r1gPAev7R" +
            "QbEFqj05ulvB6Mwy+XL07H5ObSCAal16/eP1WeJVpccFDRW2kL1/gm6VQPixsOug2UBQmQFiHwgWdnPEDnd1stEhX4bkasTSFKeY" +
            "ttT0N/r4FH2euB+ySLfKpTrlFFcHD8O2m0Jn64N4eUDgKOU5lkg5Fr60jf9oz1rVM1l/7Egz632g8TMeAW+JDspci23KrdoisJ5w" +
            "qktR2pouB/7aZFQv0a2dmO/HPZ0/JxEMk4Ojw2TyzdRfx55t/MRyTG3zbpcovNPOiyFb9XjNylXXHpPMmTri+kvoNKmhJjuMvksF" +
            "mPLWuypWJjhdA7R0FcrySR6EdX3HtU+qSbCBK3n/OwfFDEb2tbaHMejLRI4EwQm9dDE0M4rEsiEFGR59w4A9czHOCwK+bFe+U1Rq" +
            "cI5AVmCR/KXC+Ugc92tOKUioQ1MgxolBL2/5ojZlOigdcJL8/PCAVfxoxqueiE0L2fFdvpEmc1KzukkArb+TuPerFHgxYYfo5MbB" +
            "Ux1vlgCwB3r8xuQmP4TkBwDzem/Tc7aUZ+HjdrAr5b4dYGcH2suxAyuqI7lgingXvt1IbIifTqkTJFYjikge/FYdMmlGjr8cNMNf" +
            "xMiScS/N610uUK8aDZhcMPVV0GT7LkdR0FEtKzFVcrKTB2q2PmU4yrYumoK2WdHSSazsmIKal8jWvyTEDjZPxhOsdxH8qnQgu5Bo" +
            "Ss+mnSFv/Htv8v5LLOeG8Dlj+/bHJL0cscwS/gBI+ZEB/yerVM4pYEDNdjainYC37Wo0c7UugikAWD+TCbsoADDUj1B2zlF4eCxW" +
            "5KSYwk8u1aeX+YWCKG/IG5dm5SGooWDiNVIs1CU90NnTynsipbH868svpFAwsXYKxRd5jQ3re9kOAm/i8QMBAGiTh8Yhbh/qmzei" +
            "IsnZ+8DWoJiWBncynThfvNKV+IyCiLrogObkZFAyZvvNQ6h+tZOpK8i3WaE1coiyfZd33/hveUj5g8CgZB6N0qSlElbmKKUZBS4E" +
            "+SwpxUve6w9tQzxPCK4oX/tWnz/HeRQy/oq5HpC7jJN5mE/qLTt227c78mIgEHzHrGNOauetKYwc7Z7V9SAfB5phKO/qRGCYYLSV" +
            "AT2KDtMcD+5nESx1lQQCW370S9rNT4+5wDTpInvJ+hOpnAoUZdDkZp+byfs3vit/1+7mwMlTQkkNpdzsdrPXwJZb06MGz84v8WNQ" +
            "nWU5JkM7QqAZ26BJV4yScsoA0+WQppbILdZ5PwhA/zNOBwD29SMKzAsjRgwPAnLGP1O0LlEpxpZTWofiFzsNrUgUQJqK8dNXVfU9" +
            "80tOpxQAsGkl2HRr5yzyvbMVhUtRmIzVTPGwXpa3dCGLkvQcdN6sPMq0jbk4O7fOY/Fihj7XUpLo7hABzKsvN3MlInsimRe1zovB" +
            "tWEJAj4BwN0lzWHOQv+uW5NLiHEhKmpk9hgcAWBCgAkIbuincuTDc72BgsQF1FEjL+81G6cexZlBF5NOqzKYqxEBQBVN/WEhcryS" +
            "rDs2tij6rlNQ1d3YTrvfI1j2ogEsZ262AzF7a3/UdDcxV9k+QO8Oqwchb/pkLKdlEa9WMMnJPQw/k3x+2wpQ1EaOr6LaypH0bkE7" +
            "OdvX2xbz9KRIzkflipiUpShgDYPiXvN705hg4K0VBDelPmzWRSmI4vo9EzwhY9hq+eMWJBKfpcbGm/U5TOvqD7K3ZJtukaErPSPA" +
            "nQDuztgLuq64mpncPLr6zAWY1npvwO0EbDsAGmt+8ylJyZU2AyH/w45MdBY/F7NzybuuExC8IMALLgMSAYJ34dJWnnMnGF5KtNET" +
            "YeVOs0y3RxHkg4A9uabHM3KMGTKE3LYVE5lgfGdfgQiHOQEl2m+baGgnV8K4TpSFXIVvNr1DKv4w7qSDPcb/WKdRdXmK09h8BfrL" +
            "1qHYk5zOcYFAK9j2V9K3FrPrkU+xthejdcXgi86J2p4k0crreLB2T8lZxG3Gw4ltJ/de95wXYuWDDZat5TMQx4Xz7RV/gFMN5LPm" +
            "eYgEmvStZ09qtC5nS3raa4ut0eSk8hItXBAAzATw6s2PfjDF+5JxPbmn4OQJtsyDsi/zSYhzZt4OpM1e+gjPRTglA9ginThYsNaP" +
            "AHBbA4CPSDD5Ti8CLO/Qj2POxgkxVuZec0KPCVkYaLLT0TaJmeToj5ImS2OfyCj4tLUxN9GwE0It4Sp4wVc8MZbMvmwBrbZySAHp" +
            "TryMuj13vImeclIc7taujAgizkkeVwM2Kojq1IzOWIyD/IpTnms2UHd3cb+fQ1kbe0qw3zhQA4JoQjXXapy/ayFKhkmoca6tlbOe" +
            "klKJeJQPGnjz+YkY8u2QPfDL8zNkbOfkqnw38bPqpD/Hv61XRLitg8++v+HlX4qHzogtAHDXtVIbM5CWIOAzAHyw1s3vAOTAPUxh" +
            "gRb31IrEXy2bAOBlAvg4AXyaloDABQGuDtfhan2agTT0effB1MkJ2FYU9ap4RBF3UKDU8vDhdxGKV3ILyjiIBsprM7f6U12BAs3c" +
            "BGlVg0k3Zt13Amo6TyiWW+0EgPBNGwe5sQIrKSz2xMvTi/uhQV0XSH1VsJ+AMZ36BYBWUHAltKvjnMreN4ynqso6o0S/Rsy1FUlp" +
            "rGkpU4BE9MJ83BIEWNKrgbYgwMwLo8vu1usMADPAjAT3NRBY14AD2lsggLD+KjMCese7fd+xBAcNExbMMwEAm3jIXceg6NMGLLOr" +
            "/RFNjAjL8wAvCPDxBvDNzR0JApjIDwIA0hVCBtwgK5z9+gwoWoMAzQXUgNxHOW9iBLdE3GWFtsLq1xydkim9p9OjvHXMLFg05o2r" +
            "ycZT1ePS4k6XAOVvFF5YA/KgBFNIW9IggHWHkdnqPSIuKJ0BhbxjWC0Zv8iXqIryauq3F98DY4WOkKTJ2eJg68hkrnTW2YdHK6rP" +
            "19MLio2oomBIEwpk4/QSu+5/j/wU6zKExT/h7Vy+d84IBESuCoUhmRny/hAQZgJ4Q4DbeqyEmxuW76s9w4hgBZs6hTGI9emcHwtr" +
            "WcbvyALiGgRMSxDw6QXgAy72f3JsUhgxWpDPu3TDFlUW8p2lvU3+nPmwtGQt1+pESDPfFmLHuys6b4vaCCaxwKGw9k8PYHLB5zEt" +
            "/I5yPNsWTzsiJ/c0yjY5V8IKGABsb4wTgwCNq6FOtg1qN+FI3uy2hbdD/i2pXGn92ULZ/N3kJXQMMemJ/pWsg+TqUeEF1EKAPALJ" +
            "IoSxrtj+13R1zsKIaZnKVLqFaurb+5IggONJv2ELADScafviyrmx5b7H7oEfALhRcCeEVwJ49e7Pbn4IAjN2WYiVRU4+Z7nBs/f5" +
            "AgDw+TPA7YOX6D8cfLKzLkGY58XvCTAMBG4IcFuPA020BwKEeyBQylNp3j7BAEL8OoLqgdo4QbLyQO+zp25VrJAfhV4PZ3Z9cDwg" +
            "LNw7XGC2BlIBbyV2wdpcKSg7ckI08XoifxxQ/ZJfACiroC2/ONZKlERT6JMsAAAgAElEQVQJPiXU9Gs1nYq8tRhZRy+9VukUVJJk" +
            "rSk7IoCv1MkS8kdg452pkA18fFmu3+MgwD8G7mMLwgiU540uvBBkwL4T8Lo+HDEDzEDLf6NH7iD6xQGBNDq01ZriOvx8vMoEkWxm" +
            "lVGtjzCYxMK8FsHjVlCm4900LAjubdPWNgp4U/lQ6DBVp4ajzxLvtsoVLylTlKGUTm/E/JwSLVkatgpP2wkQecdk1avkh/60xddT" +
            "xabV68avN/EFhZp0yTZ+/WUIvX6p7J49X38hIh6C+rgx2qGDzU6+tvBUyIecPZX52Wt+eeuf9nsNz5z2KKajiKbkiFrIE+xz7jF9" +
            "kdrFstI7DtedSK60/ePfSC7LwLor/SZhuxvM5+oh8/A40B1g2yhYJdq6XaSi8+zJkdFX3NcuyPTpkov/mWzVCck6m0qlXkxSswLM" +
            "/jCetIRZSifJVJqsUZTTsn5LUJGS05vgE8dE+y7BEpdQ9JmjHxm3hI4VJbpDpQXqocpdhWI0NMfJK0tM5TkW9vTUDRkVm+V5MeR1" +
            "47fcOFYiJKDGu0kiX7aVFyYE2e74v5nG8jrU8Q91mTVfjfXni++tPTMAsNbda6yZ6BRWpNJUEuP1KzmrhaG8JHsG1Iq7cwwo+Eju" +
            "iwXUu2hcrKscpJHQ9ECgJqUMk7v4sn7OK3Uiit6QMRAdt1nsPtayWi4/PNvChYGBApHW7sj06rUWOr15oJ40DxBQdRWkfq2mczaS" +
            "/mtaovHIZAMAIz+GOrkyR4i5VveLylxMX3pA6y+CjnbAUO8O3/lW3jzX4FkVBYv11bxPDJjys/OEuutuZUjPN7Kfe87NpfXqN/Sy" +
            "OZ8Ro8/0my5z9P60PDx3PLrJ+eYFAXwFZFhdG4tSP7olGkVvZtBeDnE0ukXYF2pTC4asOAyUzekO/FX7vYkvYi+HVvnAODsQuLLc" +
            "uZ2d8fD3lkasIjxxBprmptoIoirfOBzNQe3OnbYwkLt/yq7HCNyTtwPdAeb1hamiSB4H9vNWYZkNvaOMCiTHQArPJyYopHMVsEfz" +
            "eqC3o23Z8i2le9j+d2d0Mx15hz831ntasQcbOhuKzFmT4dRJHic/fR0+GBJS1gZm84cF/ECg1w5ZSf1h3jNneiuvvXRnhA7W0kx8" +
            "jm6Dkq/rqF1LV18POj44mgiZHb1ePy7pCVD1SaSlfy7COHlCeQH4IwDcAODjfpfO2tjpg2JFp4J+GNhpJrKG+nvRuRoegl1Fro8X" +
            "CDSsfRwYADhIjsQzAJDRzQwUEGqt0zK31tJhCTcKyDofbfrbeeq1yquXXFswenz11rtWiM/KDBREp8dNTGhpisQfS1Pw4QigekAl" +
            "XrB/LojCW2bExgCBu3EAnBRn/jhQmBGOtwSjEck5ePC251JE+SGwJ945HnconcH5EXU+B+Lj6uSOZy+OwVOuC55yOBY5m2TeNSs0" +
            "bkNsoeZfXsD4ZoIAAy4+OrLsxRmYArmzYmUVyqDoj0XBtruKi/ebj1guvnxa0lr5YVHQP6X7bVo7JJrBd/PSp//JpdnRS+YAUK/7" +
            "yhhHK+EKXlrQU19L6ysZW9s1lusyAFTJkeWr87hr4W9D5SLQsN8D0arX+BnJyBUxqsFWuiMFftHONPk+TBkgxS6VDmL06OaKdTn7" +
            "Bcxc5KBLIa2+bIZwP4Z7A+0Xg0toCvswR247mYHRZ2m5FeoPFiWH/IQKMzwoJXVgwzbckbtRjVnUBQClHTX6qLnENSKLHfWNTgEx" +
            "KRAASNtfxmvp0gpjkoT6zdVIEGxNllRsjJhAoMvrkDuMnyssFlk1QOLVIgbzz3F0tEm+XuZ4rbahPizzBJN+xLvik+NHAwOBS/kA" +
            "R6CXk1ia1yLoswIBhrcmvSDZFqUngLzBFus7RtmQK88M124OLnpXZCaZVm+zvnEquxMQOyOtzuQZ5/DifmWJMZ0vEWMdrCLm6jWl" +
            "qCQzydWUHQ5jADSCdm/0muC6TZSdA6A8SPmmoFc/dTqiZ3tdcCc+BLwXZ8kclFny9oLQP6Uhb696zemDccrOw3vGWQHA1WFZGLKi" +
            "dtCaFm51lOyi1aJNFcp3BWpqeXjUbD8BMCvMqmzOX9M73CGrqbtTOQC4VDuK8Q7G2RmB5yUc5/fSjo4Q23NBZ2d4IFCJd2ASvi6c" +
            "HQAY6Ryp04ePH4MMUs/MrcELL+wt7NdHGrfpcSB3WMhB287MobJsS5W1EN11dkshAsuw1IK6TWZTAKusdKk1nuXAMNtpjxoANC8w" +
            "KNv/vZvVcoyJo9OMhgFfrXe122tK8ntz4nsgsT2Z1Xguby99zUGzk8P7VjsadFT9gys5Y14/FBUKSms57WSPZfwkfCiCvtrxwlYa" +
            "rEgiGdTM465vTPDqcztr5wYCdqspPxMQIzje41q8tzy76lOoBX4f5o1HWSUmNzye2M1buhYeiNlSqAOBbTt3a/MVV8pFJ0pQ5CM8" +
            "ZaGaGhqtAViXs8m1dC1HKHoIqsJDOCZgfKQ1nWvCZHswyuuVTegMxBF1qIgrp/D2UAfuwGNB5jWzI3FGxwt676Nq7hYEfQUx1yC3" +
            "U6cFArk2E1damo+0gXjJqSJv0fTjQFzZzeMME1vary2wa2UwyWXkwuzQp9+79fMJP0ncKe4IaQ6jk8TkMi7crl44hZ+SVZAj64Pr" +
            "9c8TOlTbc8nJszOOOCbyxGPhgsfi3htyLwhhAwAQ5hfrLkwJDpnI+Eqm6JMFGwMU5reklUCkU+rYA/88QCquUs6PHa1HPNQ1GryK" +
            "KsFdRdxXgp7jsid7I+xFzbntHmcmW1BSf7nMBivXV4zSLfmzAr3LBZgjFqFORtKOA3chShOPkHnrbvGQsiNQe2SqIg8pheOdbKLd" +
            "H8wfBRfOb3Gw5KPkohhl7IT1mI8DVRyNKk4DY9nsyYHCaIQLBNJdhogEaluaPIei4avp+4qgp3sdMSraYTvmVZZUVsc4jDrC05u2" +
            "9VWNWTkaj/QM6w+vAW11cIP7OGxnUS/njdqRPdqQwQM3XUZLw7lAQDrzXG0c0t39tn7QD2WXBgJFR4gKAqez/WTVXTljHi9BL9+F" +
            "oVNK2npENfC5czqHqB832ghGowXB3E7TmSVDH+pZSBzP2bcD1eiP7RhPXdkmZLeGjJklpymOFjvwVZX/LMNRWOaIif49BQBDYHSc" +
            "zXIsncgHoEsdJ3XgNp+cyEMPdNGlk3Cq2CuPiCBzr6Xi7irYgZA5aHgGAMdh+BGPClgCAO+7lN0/KiSfVijYFXBFVI5EZrphiq7F" +
            "IMBXwBplPE1/u2zTHKHZgyR0sODP2Na14hkAnIT3IgBUv3ZHEAA80RePJNdKXuudxlHC6U83O98MXHvrjaMDgCN2YHth3NytOfZN" +
            "3nwxhdoSLeCC++Q40LTlyK3Z2yqsORpUXElxxvwSAvdYqonfpGFcSxvkeoTOFNZhavIAXHFFsbh7Ks73PMSUHelA0U7CiR0r6fJp" +
            "AcDJ8mjBptqd1kIu5+OOhLXfmUC1TF3ywmlTQa90cHyiHuJ8UxC0n60Sh6xBehhuQjraqeZdxCwvFgGHPx3JxZqpDkqThZdbfKDt" +
            "PCMvPBOwC8C6LTjiOQAzimSoBQAykSIeLcvjFQZ+CDrVwQ6KGB31/Gp+UbMYjYelR6jEMDVblaK4r7oZhjocvbtletaiK8GDsPLN" +
            "qnamTUc7ST6szy0MR6UNtU+Hsdcst9o0JImE+vfS6H+VaBrbqN8oKDsYRevLXZjTVrt9dDYUtU6iLbnOB6tqIjJXuVzO+Y9LRAyI" +
            "g9M2aktMglWV8r8YjGgidomo+xEjaAkPFAAMIifiKj7O0XioAOCwCh4bQ3T5wWV+ZgBwUBXDYVvf9792aDWmi4dKjfL9gR3w/gOA" +
            "EhxcYUN11Xbyyh3eqXDJWEtwE4KACRaFvP6DKTKD3HEeCy7h9A+SO4pfDsLl9akeD/PAqw8cz3cT/XesL+8eD9x3l15kODkIKgku" +
            "0rz5nYaCip5g8ZiCO/pFApRchMe+j3gmIYVeq1Syide7+opQhOBc1OV0q38AoBc64NzWKBnjhbrv5KMeI5EcIzhoZ6qqGq9QxWMJ" +
            "WXTr3svry7nnOS8JZC+Lyp2NEWOiG4xnArah0/mYIafxoZ+w53BXQWhA8rtWDnm27ABDd/xuVmaSF+XYykx5Bx29KMvSEM78Ee1p" +
            "7rLteGA6Aqxc+lOf9RhQDZ/MTgDz0iB0IcFVYOOkVoFSo6ltcl4beKUAwMclmWrH9oDnge2rPnPfSkfA13PE5XHtwhBEev+oAYCP" +
            "keFdVzOhnLVx9fh/ParT6Sk7AoZnD+QbjehEb2t7qXzPCACG1VtO+BKnMiTCyuqxuC7cRa7lOwDdFgNBeybA0+iQOEWfY1HasBau" +
            "qh6mqsVFJ70nnuBwjrrW1focWg+Gr7DDULjuRrQr4VooDJQEAr1wtjwG7vSfWv+VYTwWIu4WWNAk1/N3kdMgIPOo8NLesYxrtuyI" +
            "+lXyPQfS4FWOM8b8EXVe2ZZd/zmaE9Akk9ql5SfOhNZVtWln4Gr8ZGHcnLpEu440lh13xEfsVlSTQIAif+gSHd8Ia3Nr3kwn1HU1" +
            "sfWM9/lnAuJAwH9SwhtMNPjxCVK+D32RW1rZfl8p4qPojaUdxDjEnvqH0sYXq6rjVFyCicFgfPCaV/qZ9zTFwpE1zpRNstX0VYUC" +
            "V9uBA9DUBQ11WNIs6UeihZeasuj+KV2tbGCgaox0VxhcaUovFe1TRQ+MOuZTspCfPDOxoeAplhKfI0fuSAN30lkiHOhqtqCMJT93" +
            "KJz8K0JPRL6RB2igYWWFSyrqoEYFGxoAxNeVJN4d3nXjVght7LYSdsAg2Zyr6sJt2a+iJhIfV9nYvIqcAE5qR+U80wNFY2T0XNO5" +
            "ukMCgAPoJuohFjT6RJaKe+XpgcoAoFl/auq/HJR9A+kVoZfGuhOBseUq7O34dVDBCws6dfgh4+PCe+tZMV5p5i/Bo/LdCfJ7DCpQ" +
            "Gwic3ge2DdlLvHIYbM/CjQSKXy7QlR1RJc+ee/uPihHz2CPJMcOrvANQSMiatUR2Fz4De4h9u8TTzvVQXhHqwW0frX19Vju2XSyE" +
            "5RU+CH0HOu1ke6HTaR+ZeE1aTd4Ceszurk73ggNDxHXtnZ03Zuc4+UlAgdawN/+0HA1aIW2IN6mYZZmU3EepchQsWdXKxys30g6Z" +
            "W66qWcvg6te61t3/ovLaFs0RdhErqivODLYCGt0T7e7wqo0VbL5PK6G4SCzzmgYjgvTq11x1DwtTQ3qc9W4nsdFh2LEFAR4jFFA7" +
            "q0vRZ6maDbVIzzNlNeT8ji99H9SJW7vJ+VLrefEzI8v3gJp2EEDyi+AKnaFd1OiFxBt50uIqX4U22AyCrdbftaA1CDdvrcmBQG8U" +
            "LQIOCwBc+eu4F5sOtjSLK9vaRIUfC6v1zw8Y++fobqyRR0/+SqLFLW+pQ9CLhxwdNK0AlrgGR+AYPozPbIzyRzD69BpdcRwopjYW" +
            "aS16vVf16bryNWLr9Gycwfejyuq9oqg/6jrvel1+AEejJ5aKvH0DgL7oxs21mtWFnwOG6HFyawyIRtZflvdCinYhVgB0dtxR8CPr" +
            "Rzwh9OGEMO33mSBgBphnhZ5/mj5flwW5jjplsAaE9QcGct1qUjYUrnNptRi5C/DEsWjsSxK/1OG4eQCZK4M9Ee82NL660aNGzfhe" +
            "uNh83w1demSQcKqPX2fLjbbehfSRveyH3js0I4EADzO7XozNVna6N8e48XsklONAsuuKuL4clDkDVrub54yb6JAYCG+HlNbjaTW8" +
            "yB0UUUImdeBW7dP5z+BqI2sUBrUzOEIj7DaXnE44bst3H+VtoukY/YikxkpDXpYpf1ohJmMt32uluJTfU05oDCOQIc/Q79N+2zET" +
            "3PLuaWL9FgdWMyLR4z+jA7SS0zlDoNItaP2Io2RaFQztS/gFHdD6jFCAVacPOfVmZDrdCZA3Acz00V6/TjeefASi6F/UnlYqYVrK" +
            "97U4olfD1yL3we2seUYmx9JZhwbbc9ZxgWIlRwcAPtpaffUAoLbMEJzEyLhqOc1Kd87Mi2ccCha/RgZop+vQ6IW+jg3MBQCdq6tG" +
            "Lx5arLfkkg6VT8ZX9t1d+ZkAtxFgeOK7gacsyFiwdaWrC7Grh75Hj8qry+ORcAWLKuCMY44pzhVQcBwpkYcsoGM2+Oo76MJv/7se" +
            "TpbVMYFArEtaIHAJw7Dj3eryxeQs4QQ2tSqR/dYUzl46IJIgBAGG7buDOrTZuVf6FsN/2nDSOLTslGTTevFA0Z+1XJfK3zGu0kZa" +
            "1wSiv1xX91oh7lW4rIrCgR0dX7CUR8t4MY0nZdW2Qq4lAYCatWA1t6X7ewyTahoXGaNjAwFJCbXdgvNgnhvzyePRzACxpxlGtCuZ" +
            "7y3oYN9GoOjUyjr/iW5ORRtKdLQVEvmqZwJ6M6HVMvYM4H7S0LK9JeIABS6ZZO0FO6HhjNvuuHBuk/4cRk1tl8eFWfQPAdSctttp" +
            "ZJ4ksBBXDyEbHG9DFYbK1GzI32aBsAZYMWOFgwqBMuezBZq1jnomUuBT5acXWlEi82oeLjxGAeptcdu4M9ToZ+ksw9r5sXpevRoQ" +
            "QLLMXDOs+iGKoETBhjt4rSjs6EjMBNHub6nNxqpiKR2leq2F+nEg7vo9AAFw+YePBEt04r3JpgHlNpNXMkzSqohn6rsgLjzpyIcA" +
            "RtZSixME2YH1Y44G5QuMDQByNZyD9xIAOLTqQD/CAjoakGcA4H+xCdbSvK9hyq1u5YCp6iiVi+vRfyfgAh14ybGYPhc1FJfeoib2" +
            "sqxgci/ambmkErwDMHLttcAjV9lp0GR14mCleSQdReG6mkhhKvo5kt+q7oISmo/UdSW4fGCzmfuMTXinjvzZ7I07QDCgZRUkE63C" +
            "6LOSbr6i4xE0qfBIGH8ciGa4RMugzxYJxTd6YJR4ch0YncFmm2NykCoaUO3wK4SUNAQwPxj+0DijjUqd6QGtfF7um5QUa9/2vfaM" +
            "kZrBxlMXtOy7d2XC8ErQlgyMjbKe9Nh+VBRGuf8pH6yuQZsI4mwDT7o0IZ3/OnCn6Hn+OBapX7N1klG+VvU9obOaqmzlN+qgcbpb" +
            "PnuYSRaePFNpSYW7sF9u/HM2S8of3yhpnvJMwGMjNvZVTsbRsERwkUYk+mqe2QoDgUMCgAhYHa48DoQ+H9rmIeOgzNmOWUAQzsQ3" +
            "Q9GgI5TrUAWuHNsinXwSt7gmFjVMUCPA6VpxoUzSlaeWjTchAOj63J3gXHaryaLa7zEA6M2rJ8fzdLfSTvVa21kXJNiFxua5qO08" +
            "fW+bpWXzjgO9AsAbbD8UQOs/nV4RWovafng4x9G4hZN0R5GArjxVRUD28n1B6fMrtPnhxtBVUdKZ7/RZrId61egj8doB76q576ox" +
            "TxyOSptbsyPeUl+CAr2Pq1SeCfDPnZw3G1lr1ubOombgnjU++k/R39HIxmOqIrRZx37tNizTMFnenW03NGhYm7dOzNfQ1OdnDZQr" +
            "omSVplpuvR5W4svX6uNDjd2HYrYAB0dhsgYNOo6k3W/N+8Q4DOgHq6pTZC4t/h2fh5J/r4xYPOlxoBmg32RyPpIWaNs8ivJwkhhy" +
            "ekGqvzoAuKK1yxzRKC/1WDDu+ri0/m3GgoPRlXgXHbWgWx8YCbUdCesl+N26tajIFa0PC/MCSq0hPmpAFBiXAbC1ssOIeq8R6buZ" +
            "5Aow4Azs9trNjjTzPqDRuI95CMOM2G9ldgLm/SgQ+J/noEVGIudcglBRrvVHSKdtB+CqqGP6IZvqo2KO7tvmAyT4jiaxkjPvRQT7" +
            "Zh2Eto48n/+jkQvnD8alO+DSzJ2D7sbmCassJQdfSy/NdxUozwRE6PYgqFCzEaOEShcJciwQt7cwJ9ac0M+2Nl9pIFCBfu4FqV/r" +
            "0GcMYUdaV0LSPxdW4JQ1uT+KdHJ4tzZU8FCBmYZrc/dEBke7JEeoi1rHMWO2pdz7m41S7MeBXgHgPri2C2x1FW/fXATbq/W8Yxxt" +
            "AYCW70i5lI5m2krpXF5A2RTU2DCuRXW2kAsErFsUBQ89VWBp45EH7WQ+pHs1zUz6ruBo0Pk+QcpBVVBasAM7DL5AC+u+nka2Z+2N" +
            "a1vd82DukqOFV9thJcaQraPbZGHL11Dle9dn8RWhasNbZ8OCsiU6Wn905zG6Gbd/TDkba7qqTHbeZC7dG8jDHFdp0TXPWEeON+uH" +
            "jw0AHPYdAVtrpapHyapl3qwNBHJoFX+JrLoeeukd72nzS+Vxi3FjzjoZPkYAELNwFXt7Nnp1ybDlupbpvsoR7zxZKOR6ejK+/N+T" +
            "bi/HgX4qKOFJ4mqvfUOjj/KeOvChzhp0AzJX+x2Mvst5j8cVeJChHRU6Y9TYHzO03OuJbrvR11aIY/CuDLIEbQ/va1SCr6LTL2uH" +
            "xhHKYe/3x+P9fUJ8JuAycrV47wPtSeXiUV0lzXWNWFK7PuzT6eO06TI4fa4uDwSu3stX5M/64Jv7opli/w+i67jslkZp3qoj0nFl" +
            "jejTVwcOosqqDtHJkxX/iuOuBPauPVgJlHKHybz3dkvGJ+PnmzwTwWmOiyhk8y8GH7b1Ryf4IyM6KUPTXCWKX8qg7pc9zsZuvP4f" +
            "p6ZXIXq08CJjujOEgXdoY21nRrquLDUc06k+JtuKluOZoEvZTyPPYd/ICAU15z/J51UiBQKWPj5+HF7MTjY889C/BRHFE0R1zeOX" +
            "dWg7JGkoWds/SjnLCb2Rx5rEOiTPPvKt9uOpUnG/4vwocoHAGdaC0wDlx8L0gjV5eiFY7GFnk0bgMqltOoHb7Wa6DckVGY10svSu" +
            "ZgpjtPPXSuHqEqrDRZwaADiMlwNXaS6hM9oJMD1rmNarexrpnBcAvA+8r9a8rwDAoX6IHGBDK3cFjjgaJK7mi2X8pcX+2nCmfvl1" +
            "s0GAi1QwyX4tcNvOtTSs0LaFHhrvpiFPHIJTHrIZUNGAmamoWKcmPYdvBl+jgI4+GiQs+vet5CuFEKyXdXGB0If1zxXODjFVB+d0" +
            "mql1onMMxONACJhuPXFtU3Y/mrcX3UkEC5HYKTH0g/+IChVw6tp1ZlfX7tZJdLYxIBFO3lF6Nh5roD0OCvr3FFXIjbwKprQBXWnE" +
            "TMXUDNE5HSV5u4XtK/JXHlUyb3yj45W+TC4FtfsjF0DDqZDiZzF60OkItpkFR6Va+e4+lrhAYOTxgdrOU/2I9R/JUA1WmPhoEF8d" +
            "Cmm5FSPyFtBtR4S6NLdR0ZIgINgawPCy5uh49Zkvi91l8gTHhMzHXHKWUjsPliEzALU7HrE42LiJk5kWKVx8DnziPUIa3A3KqC1Q" +
            "MWlqTSSTFI8xSsxI+QQR4JleVzFkofMpkmDTBQpuqz800/rk0Fm7rgXjCpZZBhnBtDrDlXFMnujgiPeQhcIiP6cCihKo+pHzibQV" +
            "i5NsGEZ/LAsFjh9uS+l7bk2XLU3mqm8VFXMcaAJ3IN5/23rCwEhUBgBFdHq1BoXrC2LIuLp4m594r4i1ecysUaXeuR1vir6319id" +
            "xFi4aVZOFVOkJ5C9+Sq8l9LLiUdKv7xYS9DrqJAlGu6Md9UPPXCEnxORNI8RdfE8ZyjHod2uy3aIu2Wp7yy7tOwEfAvLLwZHhGkL" +
            "XTC831ipispl7m0hiCIytLYDAA4JZ3Lh3kVATfxEjXyo1ccTcTEdeD+4oLOhjYl4+Kw7bPtittFB9lYBqyVQ0Eg1a7Gw+AJ5MpQ6" +
            "++5+tJUpr8Jhtck63tQNNBqtS9UHCWKYzM8+03sIHriRvTve0fPpni6aykam6x1ZKlJT9+NAHwDgBgATwISwRWm4ngX3NzaWCmNp" +
            "NiJDpmgDIJ4cg3EwYAegJK1j1S0LAO09x2nhY0cCJdty1cQfADlWr9PL4zjp0l3CIHO+anwsBbclNoTI2MpQtuZ9MqUI6ekzZu2P" +
            "RtpWvjC+EVxvsxJh+H39ht4iEO7ZdhKV8+/4MXCQwajwEZ3cZBkYpSPVqxwr7o7BHVntgpcWUofoAI+3oO92hRGWyYVymykc0UdD" +
            "FGqxDOWkdyFkfZCWzRPBcGV+J0BbkSp7mLYnSPwS3g90rmdAfBGHTpyMzuKvx5OJF0D3Sf4i+mKBddvy8XtZRlczoQyJoB50OwG4" +
            "fS9xhpKsvuNrYZatQ3IevMBlaAAQf1uM+F7n+h0A9p+LX797Qt9kgIw8Ghbi3s0YKJ0bvcAzlUEHqbwj4R5u+tW+PGBXgO07pzBC" +
            "ICCWW5NGBwLNIvGd/xbl3cuyVFr4dGUnAJzDJDkIwJ0T97BZzFjyfVBnhbsAiyK7kz/bCaBoZeKd2JBynO1wviMD/sTXB+vwyal5" +
            "vGgd283lAoFgAsDlD30b5uZLb9726WBEjXVuo0vLsOR3AFjuWZqm1SjIuCMofMHV8ge3KEpfMc/rZDRvVMTNlcvZrLON+BOn44FP" +
            "8YS41MDKo8uihmZQrtepXX4x+IhAIIQ8QragwFx2j7y0wNBfTToV3iTaG+JOXikuN6k+0RtDuvjssVUISQaS/XBX20o2TgD4AoAv" +
            "QHgDIgIgWj5XULwjsK1mY0TX7c5uJRNn2iZeii7TQCB1/OW0FMisyHttSYIEjBoZtsvtRiPinpcIAN4A6Q4wz2IfXSsQiCI+qAiw" +
            "WqpuKPo09U/wSDWjVl+KyhVWgsnFCJQT5xd9+tdrDwK07Zoo6exAwFZW++7BX3VCfdLXYr9mcQQT33GoVsKW0e5X3JrviWHo1gUP" +
            "5vz70FjnfnB+efHa6qyuQQBNHwCmj+vq9bw6rvuOZ+Ade4TSMEDmZ+NBOr/jOfxbvyJ5htztw6a2M2vrg5X7VSbof1vyUPKdCQQw" +
            "5GfD5N5qhwA0L828K1vr3P3TPdpUX3x0XxzuRIwj8zTL10Vpt/N9Ke8OaqX83KU6YuF7t3OVlbiy3RS4jdhI598hHwSo+6h8NoDj" +
            "AwFbVXkzGqeW7ABw4kElzYxHdZJKG13TztMn7iea8Ki6nYH0g/O47gQgItB0A5g+ANw+AUyfAGgGmu8A8x1wJiBcH2wNViWP7PUA" +
            "ACAASURBVMP3lfQ9DIhXKiix29y+wQJmQYRcDrcrsfx5YcmaLSxr6krf+UdfRnsbUcxLa1sinteACidcPukN4A2WYOC+PrsmMHe9" +
            "QEDHo5wSubgYv1ocM8We1/O14xk3m+qtuXwlCqwGAazCbMK5noSKOTJZ1HSiy8XAlrSWvLWoreOwScf1R8HK4ih0swGPMGM/sWGI" +
            "k+WUCQFwmgBvE+A0we2bX8L03e8Af/E3MH33uzUIcDsB8+poO894d+PdqzDDvQAM6xMZ8S+5lRpvpd05+tFuwL76r+2oxjsX4Rff" +
            "6QfwH/bdgxy/4JJO3q2wp7Z0BJh//gHuf/kD3P/8B7j/+b8Dkf8kXL8jCiNwGB9D7JI+V36NeJSgrQ1+K3M9LudV9aWXMlmDgg5V" +
            "nQtPzkJjJliemEqDgMmVi9+y7OGigUCuf6WjLahOhnI9nGwtQYG0W2BFSodb6ULlm72mZNWtB7yOSqgz1WFycQy+yt2bFsRt1oR3" +
            "YfkUHYGzKgmuru80Ab68wPTyAi/f/RW8/Pb38OGv/w4+/PW/AwBaggBaHmoND+fsNPyVfXmFf20J+vdCy7Xv1nLRN3k2Pnb6vQDB" +
            "zy+IJXT4XSqu5/jdrdih56WP7jjQ1hTXCAKgOxDMcP/LH+HLH/5v+Pn+Bd7+8j883gmAYl4oIHcWHsr5Z2kQ+7Varlaf8iwU8GWy" +
            "J8iE0FIdJ9jOfJWptyiLKO9bmZlQ/AUSb+Sw2MPTpqhuAY8cgrq7E0g7AczvCIeg861mF+B6xLS8IS0KUlxW6EvdOS5fg+DDhr2j" +
            "jWPQVgkXdwlwc/0ZqGrzhR3cEUAA/s1vvaLek5AdQekQ0bMiAEwTTLcbTB8+wO0Xv4Rv/vrfwDe///fw6ff/Gyz2yAUBzslOV3Nw" +
            "2xXgnH8/f1x+YXT/qSxc25kJAuJPV4qi/HyrPdZ2HpHld8mDft6NLIX5gjoIYL4D0BvQ/Aav//LfAO5f4PUv/2M5crXKM2gXRuU5" +
            "0kKrHhbDxp3giUldVkLtin5GJT+qPYkSS/JeFbVdV1ROydw2nvVAgPwKjItAprwFc4qJjkFZhONAbjsA9+ewNng3Dhqg/pSlRdUs" +
            "q/Fc8lWhl7Uo2REoHJUPiCvOS088BnBCgOkGeHuB6eMnuH33K/j4638F3/wv/3b9YUYC9N8O5JdNnOl9RwCCT79QcrF/iwNxhOiG" +
            "v13HBQExh8laZniZBAFMnnXXIJx+Y8c9bgkBza9A9+UPaIbbd7+B6cO3sLyGdQ1+1CXGuoWgpx1QUCmgpNhXIOijmvcg8UO3QKCs" +
            "WIl0CvJeXOCGtwM1jECt6ACy7H3iVymXLjzr5876oIT3Vj0sk1O0DDRayCdPEih+eSxUs54rmIveHxXcArqW1Q0JXP7BaQKcPgDe" +
            "PsL04RMgTgCryyrOMduKvp+sCBX1dD7Fa5ASFPChilBLNEiSXYx4ECUrZ/Gysrc0RARwv8HytiVcHraeXpbva7CBZxuJThg2Rpvw" +
            "PmRrQmMz1eK9dgMy/qnGw3syzxY0tbdkB8D/XrOeWsJoYaOUICCmFHNjnAE7BQLW3QAfwfzhzxlRWT4UcNMGXc7E1fBy/ApAFAgc" +
            "UV0LCoWaVPdg1rMLu8xCrrrY+h5h1ZttAK5OKSIgrjsCL14QkKyCr4jOwxW59cWyp+iSayTpyWL9/KtN92R5zzEt4XgggLdpL3v7" +
            "AIC35Y+XZjmESSCr9wXkNTrXdP4NFW0NK19GAvDk0UvQJThj0ifmaxwjx3m5xMpFl/q1mjM66AwYPaloo7MaA51/ByYIyD4Q4NVY" +
            "6MFraRfztMNVtpMZ62TITw0Ero4WVh/MwR3J7gP1+HGg4MNtBQBME+DtBfDlI+CHTzBNq9PqAoFg1ftgZodD3oOwbCwteWn9TjAD" +
            "rrsttOyuTC+AOMFykLFxRkbvUxBPL73n6DyYeeFR+c7wRB5HGZgrGbETzvCUVRnt6l1KeD1x4XY16AcTBMzBt14HZkarBmdj1Dof" +
            "5XBcR5Q11xdQbc916vGj+uk926/e+MrGTjWS0zQIABPgdFt2AvC2OKvrD10heG/NOeVp+NErenqbtNrTnfX1QCcBgHP43ZEg8P98" +
            "CmPadUmzcbkxWif/oNTXFgCsIKo0Bw1+jr2ol7PTj0ONtD5m2sZjJ7GF0Wq4oouhHAdqZzVu8AgBWFaNgjrNmn2hrop4LpOjWy2r" +
            "tSC1yJc1tePoiaxEuJebZA/Co7a76NhK3zr2SXxaHf+XbbV6cWDd6r/bAThbyOfWb63dvbJgEZk7ZoV5h6njItCoOa2JxbPVh4PT" +
            "60RgxrlixJR8oWk+B1oV4jL7golj1Q8jneXiQCB7O1npUfce9fp7LsLYNEUMAgj67AIUBQLS2bZMryUBm7Mz3pF06mj0m9Cpj8ub" +
            "wgkg7plesDdOFUeHHXwNqh52dBiTrCXLnS7rFSarK4yfGlhl12ojpHri8ww4Afi7AN6U4f7FU3YBLCgV0hjDu+0AwLoLvIVRTnbo" +
            "LZ2iX7ArO0N3twcUuoRWRfbV9NQdDXAML2BTi90BgvSnLs7ABWR3BEr6xz3NZOkbydUNc6S12vmxa4jwAMDM365Euo0bfTdutfA3" +
            "oiTMZjtPgdN9o4OhNbyXUM4/AtRFtKP6R9OB0y37E9Ug+atbkCByzv5yHAim2xoMTNsK9vae/MuilLsxrYmlhLj/uQDg2nI8HpeS" +
            "B7KXPLzBdKk2nIWzHfCz669FpHP1uiQ759b7V0IaBMywxwBsZ5OWaIbm/+QEFxp7YyVn90Zr/d399zTubB/d51uHEjFn8/bWmbMD" +
            "m68RpSpZo8LSESAuyTn7MAGCexagos4nYBdcFBAAbatywZZwDO62dk8J9Mrw7PAYp0jkAt3A2ghjuTGZB4LYS+FGISomfvKuueJ5" +
            "khW9p5kiQxtq9UXDfhzoFQDu1mL9AgHpqFCyGYIAuO0A+1vnMl0E4H/J1MqUtWxuj6aXsdG2ss0870zyWS2VNEA6gdSBdI2Ys9tr" +
            "vfag2e0uKj5WdBV7fhqOEIB1HFfzMq1Hgvb32D9RC21UeIFAvDWDaRafokSqHc++lnCKfXtgo1rkpvQ4Blcrpzh4HiXvkr40HJsr" +
            "Y9OYG137oxpONAvLTsBP5zGQ3RHwF8qkzLjnHYLSYKCmrBWHGKyoEjyg4kY5DR1DrcTF827lhL9qF+JBJ+sQ61EVjAMBP8tX3cuV" +
            "8Jb0xMieV6BatXqO3v74WncEDkGL/exke4c/21ZzFMA3HYM3ZsOnBpRtiAMh/yhAn8X+Hd0aKhAyRHbVvGgysB5HOgrdBpm/dzfe" +
            "+8K4ioIqe4i56GjQyYO2pfqHne+uEAAkB/3LysSHVoLL57GgRrglfbfs709g65+xzyq69olmIHP1hAklithFaQuIXHiQ2I77cNdH" +
            "MVE7EsrK6a8IZdqtbZNkt1CaCvPZ4kUz7YiRCSWZpZV/69bbqK3IEYFAL/htZjYcglurHC8TZ11odjp0F/vgdifD50oTieFcuCwu" +
            "Cnc1n45/Z8THf6h6AeNKKvduwZ359dZKn32Qx2Y+So77NAlWKfxgHWZjd6APZOovw0hoWuHmggDDi4G0c9RNZ6yVwqoomEQxf4mM" +
            "pI7K0ajZknqwAdSETD8HUAbL1+4/lahOjUpKBfn6+sxAfnx4xCm0Fmgn0YOdLdruLt9jAT+P/3QECX9P9IRlGtewlX3YQKCGs77j" +
            "vGSu7IP3EwCYEK8m90QRvbEOEH8cyF9JqezdXrsc1gV1Le+W/pxrHxOWYOGyOJ/TngFAKb2S0smJsAtPLKWsYWxK/bPrAFdQk8cF" +
            "e/bf7QKQN5c90QPafFulxkmhqx8NqtWlfjooymWYmr+/AKBItx60jRbIzwR4eJT2Sye4ttXFqzaklK+rtmMUzKvRj4jrTHPVwcJA" +
            "nN3PVfUzhdRdzILz6k8wcEvGtDr+bFDA3zpO7AeMmBNMyZhTy1cfDOfbbFVCR4qvoK4r9mq8FqOi12SQcHAu9GcC2OsIXBs69nbp" +
            "Ef1s/syWGaG3lX9E//RcVDhfn/pD66t8loPRMiX2N5G9dwC4MkcY9uG73Ib6wcJDyyRxHSV+MHgr/v49qTNWZeqrt62HYzqxcFIV" +
            "yCUaBMxLrUPPlEwMcXXmdpx/jldqZnz6EIR83Rjok+0xYNUttyhhPvfvEz8W7E7ADI3sdNS2VlLWdpxycrR3he9qtMGDOUatzHY+" +
            "L1qwxNHyXNHXsiMQo7bdZF1cecKA5ADZfi8+AjRA1Oj92/FwTC0j51XR0PTQTHUMACzkcgGAdK8sw3kg9WtX2o3Z3iFqthrP0aXs" +
            "caDqThzcnhbyyesoGSTPKj1xLL5Kmfdp9OgdgJb6WqaFsycU1b9QJ1xK71E0SRCd3r73A39JJw0Eesn5a+mv6uf7rnzouvSIQRd0" +
            "IhSRqbdLlZVeXfG76aSCkQHWwVCOA/VF7dEBv4yjUULLz9vl+EJP5/So8xRXRKd2XydW4A+tlPNXrhRsCeN2fA/YuW3v9LNPWJa0" +
            "lQABLSXYzruOZj8GPKd/c/aJf0SggTqfIo3Zg/rxLHWx1NkwxxUXk7rA8eFu1fDTPFfXd1DNqSaNUlMgEAnvvVsp9YRINO7YfCMf" +
            "OOg07g8LAgDqTj6VnLbkAoZcmSJO3EN8PfGogUDtEdiObb2eAdobx/Nm5bgmEIi1P19DQXYrEwo6L8PqtTCs2A5zlkkxVwOZ7fTT" +
            "7e8BWv+n6DRQm9LlS2s5+s5CYh0EUPz6u8yWeDuXuH8UdkFRdkvmHubH3I6+/bvZBjPZQZaEUtp5u3WSc4NMd43QQS7IdN9HBgBK" +
            "/aUwvR2oN0ZNduqDS7U8uNGHwY2+eLTZ/wJBy5VFJvN2yh60TrlDNfkJ6myFUTfMTaVaajDb6bPF9A6wHaraVizdVkCdcMd0SU+q" +
            "/nGnWo1N0cf6eHWMMmcFTe4i9bMmnrMnPEV4R2hSKdgndbyY9MC1ysvUJIEPAjwpDTteheylAfx2VBEN6zbmI+JsnSpdFHuH6NPM" +
            "ccIa7djUniHuOeT0NvKptXKxLkQGgcKj2pcLgzZn/ysxNIXtDHJn9O9rkWCAqkYfIyl7LecYFu0kVoAD2Mv6rFoGij4b8QjjKD0O" +
            "NC2RQXiaVZaaelwH5UUYhHUiFNKEYlFKjUYJG1jMrc76INIPq+8Qo446W2A5bxXnK8U7cY7KNLRtlRLdvzEZpfLeKuJOm9LOjdwq" +
            "JbHn6bh8IGCUgJK15nyxbxPpq3JaxyGQIxFs/5FtWHTYVS+HVqlTkmKGDOEv2siedUTNdIpr1A5A0emtA8Zt7sx5lJllNafcnTra" +
            "LA2jka+ZC6RTH0kTLYFAI84aP6XQjwOJ52ucFdknfO0ojnRUMReV5QVIxnxr3o3RmnXCI6AEW7jK0Xrus3cjuIilt4ZfecRQ9FdQ" +
            "rDVHrpR42sHIKxr+tHISZ9kdAcUuWOpvhzGgFrJWBwCbc/d0/ruDvOcBkkGSXCbfD+sRrdIwSuwHbzBZyZ6hodmh0WB/S+ybXt+x" +
            "AYA1s8qVtlx/dEcb22a1/7l8YlrnyeUscbZgCQK+ZVKch45RdLk5ocblhAy2anJ5eoNzZEc5twxSuz+g0iM08cqO+xNdNpbKqzMG" +
            "AsV0Dy6teIs5sWW3x71XgopB3BOFsEfq1xb3tbir56Zg3BkXt64lGQ6dLF5FAFCMs4X59B0uAeXtQJ7zz+6nXAdVr/yK4EeSmKRw" +
            "uesrZUtmSIZbpD0PTRjhV+nrwkmG5FB1bGwjkTbHderLaIzaZYNl40fcng4df6KdGQTgz8Dn6j1BzTd4C7HqFnuGBPsCFndEJfml" +
            "W7+gicsnVMS/vaB9C++PFD/7Q6JrpW4s+uYVXRqEaTxxJUOUFo5QJq//lbH3uZ38BMp43mwEyXlLg2UpEMfoOksybqNY6IxBW1in" +
            "udFMNSNssZGulu0SpjKW69kBVgH2IOADANwAls2BaW3PHgggjm/XWXO+76ChcJXyVc5tsbHxCyaFT/DC/Spbqu3U0cGkaJohKyso" +
            "ZoZJdpM7m94xEHAfOaeb45djwVsJCCbhYPcusoDkvu2Z0Tm+BSu0QTuoVeOV0hmCrJ4ZkbQH3AssvSjjgSaMy4LRbW7NwoIiE1Lp" +
            "lLLFiE+Pxx2xsxRTQLMB6+BizRb59a9hQi5W9Ya/+rri7eueZw98cA0EvEjI78tO4yTbDw7q8YR84FQFVfk6TW61fI5yOQoCgcMR" +
            "jDsZ29zAzaeXiFJkvCRfJwCY1nXxaAnvCCe9tI7thzcrGNsebRD4yPNy5lLlg8LfbuklutiqHz3outTfUSDozVHSchiszgS5utOJ" +
            "FuOVuahh4VSfLtttLHjBCRW0U3YyWiRVX7pZtci3U773/7QhXbCJNHXzzjALPrr1sLkhGT1P6IQhfXARBx6RY4ReAnr2xMofYuT3" +
            "IwJ2OC7XZCdqlKXVhB8RAPTACLfnRFdKrNYPyksMyIOZcy8I+AAA+5PC/uTrr4Bdz+31e0fmzH/bERfEO6e/HAdIRK3ihB7pVWXi" +
            "ZNahxwSfNKelfQpDRARY+qM+DHntJhEEW+vBCjbjd0pv6SLfCHgffpWuKeStGyS0gvNQsmBPsSsHVkrMFd8xF5rwHw6r00+cjPUy" +
            "/jfGJc4VK0RDP3tFVSpqor+06e/yLfcDx5sb9+sF4jL2nfO/h1xp1am4JMMj8VyGU7rnAg7Sw1qQC8hOhTavy0nHoIEB5pmACRCn" +
            "ZTdgXo8DuRl+xfX6yjc9OwI+0f8uvErLKxde8DWGEuF5ytTAlmBTryb0noFABxDzRSLdRYwWv0IJBOrBzMg74W3y9idx9BadMSmH" +
            "25gQq9HGgXP+EQAQgXBfM3QqgrC2ubave+m9bwxaSWgZLOTFSO4hp++LgIKPbedF6jQCEH81t2D5uUg9OUNlhT94/SB+TWM1h6si" +
            "3gEgRxJ3mQHzrFCwo7fvFC5HhclbXFsSuKoDmsHuIhX/4LGEcnOh6EDNA8ulc2NSBerJpfw00BFR1MaCzFfzc8A+ZEOTcUJDKjud" +
            "/Z0AmGBZrVz25mCd30PHQqhzJJTFTBHx+qO/CovRZ3wt8mFZhREl5BWW6Et0ewg9McINyJW/4IrIoSx19+k8YuxCJwLMADPR5vy4" +
            "s/jO1+H9Hn6lP/fdbZHiBECIAJN7imif0AOVrZGHpK+lHXlEx/uG6enLXxfrIHCr2clD64cEABXQ9Gpth0ntIj4WW+ECANy+k1fA" +
            "X6Ba6sHdkOAyr5J3pgf9kb9euh9z2xYdvEgCg7OHPJ98IqZ31dU0cwVrFsoGAiyVqvEvLw42mRPJX2nVR4Wp2O8Kgj13JdXfajs7" +
            "2voaUlybT4NBedi3A/lusr+Lf6UgTXKvS/ljg3BhBYSrtywQQDnJipZOOLrzLqAwp/tj3RjIeegrpgkQbrD95N86oWNUArd/o5R4" +
            "ldBffQTwzu/Sfhfn7Q9hBqAZAGbgOh85p0tCL905MgA4q/6vHl6U6+sm8EMwcYaCIXDBAMCnowQCNlvjL38hIN6AcAKAGyBMgDAB" +
            "wW05EQDLg+y4VuAW7xEQcA2kcAv6Vydhc/P37+QWJAjW8/4ERAQE8yrvGYh2m6Gf5ooTlcWRHrbXEAiMRnVTTmBbqzKeXp628QBk" +
            "lGcNAj7BR5jh5bYn7DsBoFNIA/4maOUd/SF6zc0SykK+DwtPXX8HwCporU1HGYcz6hRwmr3ptZSz0QuXdhBx/TE5BPz0K5g+/Qrw" +
            "01/B9PJpLY1LmqOGvvO//jGbAf4GwzJRr3/r7gIRAcx3mO8/wXz/DHT/CeYvn4FePwN9+Qz09iWgG6wmWpvN6E5xP46ebDjacZ8z" +
            "vgr3a7ZP9ESBdF3/0IA5pncnKwzmefeP/yEA3mD6+D3cPn4P08fvYfr4C8APv4Dpw7eAHz6B+/XlBTMAkUdhcdoRaLvegn+iLT/A" +
            "qusrmfntFe5ffoL59Se4f/kJ4O1noLefAV4/A81v4AuMFx1vGHq8KrwU/ao8yVsfPDc/gp9/sltyOl4AfgsAP69fbzCtK4gL4vVD" +
            "AZ0CAb1cat5izoYrmyIKue6iPbMuvAwp1wKD3N79QDQGlEmRZNncCwDAHdVbgoDpdoOX734FL7/+Pbz8+vdw+/av1jRcg3pcSuw/" +
            "Pw37HsE6cpOlGlfZPtHTdtyIYH77Avef/wRvn/8E989/gvuPf4L7j/8T5vkO89uX7R3nAOmcYw4EfHnUomCsFekksZfsjY2Fq8+K" +
            "D40G4Y7oF4WmlJRfUIJ9AUChm86P+7y+vRlsusH06Zfw8v3v4OX7v4Hbd7+Fl1/8Bm6/+DXcvv3VQo3cuF93+Mg59zMguZ2/+3r/" +
            "7l3T9umCAAKA+88/wesP/wxvP/wLwA//DPPnPwN8/jPMr69A8+vOvD/2E2G9lxnD3o4ifbGS7bw2WVxFiw80EsyOUu14vTrC3wkA" +
            "WE8RGJ1/H513BEJoG7ssC3XgSKOS1guPEC4PwIidWw2XE3Ou0ZkAYCm/OvfTHgR889e/h2/+9t/D7a9+B4gI0zQBTgiA0x4M4Brs" +
            "B8FALB1cWXQrfMukP68z+jwTzK8/wetf/gBvf/nv8PqXP8DrywQ032H+/OP+zEBrINATmUqLdFILAISEZwBwFOoFXeI/qbVUBAAu" +
            "TeKBrBkT+AEArDsey1Eg/OZ7uP3yb+DDX/9b+PjrfwMffvW3y98vfwf/P3vv2e04jmwL7gBAUvZ4U9707Z7n1nyb//8jZq35+u7r" +
            "29XdlZWZlXmsJBrEfAANaEVSlDmZ3FUnRYkgPIHYEQEgJQHxH3MEcGQ+tbnO/ekQ4DC+jl18tDYjSLzeIFw9QXz+Hfj8O7QwrotR" +
            "GAJ4BUcAiXzOT/mV2W0M26P6/cDYOcmTm5xjtBA9DyG77BMKACYTIIgKdyj9pxdOtU1HfF14qy9nN81XLNQLCenNoZY3cK9+gHPx" +
            "DSAEhCCzViC1CIjMAtCK8HNGBFinlgBmhvZXENM55HQOMV1AqCmIPIAFQBKIAnDkm09O3AeqiUCnMWMPA8y+x6tym44j5LDgwmf/" +
            "WNqMG/sgAF1Rcle3Mm+XI7EqGv2BAMfrAEhNICZnZsy4/A7u5Q9wz7+Bc34PdXZnafoNGWCdkACdCv72NTiMyYHOEQFmY31gBsTL" +
            "AxgCOtKIfB8IAuj1C5hU6kVU3qt8RB3eqhB6bBmxizdUOZ/5Wk++1ZXnZNpIl38yloA1MkuADbtUh2ythtrcueO0XuRzMs32xaLN" +
            "S9jYCls6Qp1hp1P/aQq8r+6xNYOUBjPuPQIsBCAUyJ1BTs+hljdwzu6NwC8IJGLBnxCTgCSeREVP8btRzENSY8mfNvJALNHqcANy" +
            "p5CTM8j5NaR7DuEsQWoG4c6hV4/Q6wdEq0cgChJppBUR2Fq9fQeDiueaBu+mvPQdi1JP68Y9LEcMgRp72m5o2WT9+0eHsFVEALBc" +
            "GCjtZswACwlIBUgHcGcQ03OoxTXc83u453dQi0vI6RLSNWsC0vVAsQIArMEFtyATcSz8I7McGFcgjvMZWxalB+2vEa1XUOtX8PoF" +
            "pB4AkhlRTspkV0SXhmtrwmuN/vJA9ZNfjmyxS0mOTgRKjbNbOxevj2r1ttGQuCEBEwBFS8ARkKuogjSQbHXYOZ4EubbtquvZHvbo" +
            "jfxGkX//LGZt7WPJQO123pXxtEizVVttC9S+e7THNtLBdrBEmx9r3YWCcKaQ0zOoxQ3U2X2s/c/cgABY2+RSReQELhUs2x0E4NxE" +
            "zTqAmJxBz66h/BcI9wyk5oD0IJSH4PEdQmho/zX2900kk5qzgy0e0qpeu6twikWr0fKUfymeLlL/XDlTJa1sY+ZG9EdznbbqUtsE" +
            "z6EIQF8CWxUVF26nr3P8vsWuOJoJgAAJB5ATkDOLx4truBffwD2/hZyeQ04XhgTEGaX4neWESdhiTvq9fJ0R3SxjJFyEMQFQq2dE" +
            "z59BagKGNEQhOXm4gdw019G+3qtyBtpOEdmTb0v4b5orhyrJUWSnChGw3KrFEnab8GulzL7Edk+o3CL0mBkrEoG2wn+V0iD/nNUk" +
            "/clebdrVaY44BPo05da26qSK65mJhjRr85ekk1joUxJgNHuJJcBZXMNZ3hXWACRRVGe2l35MR5CRDw596GgD4SxAwgOEAxIKAEP7" +
            "r8DLn0C4AaBN6pxVm/05OLZE3IYA2HfKgnzVc/kOYQsCXLw6gUnga0CXam7UFw3VSfvG00V/RTZ1N2MFM4FIgYUHcqZmPcDsAs7y" +
            "Gu75HdyzOwh3CnJnEO4kN7fVZrvi7B0TNh86tT6SQrh6Qfj6DDl7gvA+gNQEIAXNgMiOIM7ITdsG3BsBqEkOX+6c3yTXDD1sHbQe" +
            "95zQW+sPORIg60K1LtXwTXnICi1pUQp52GtevuTRZEQ9ukjADKSmfRgfWyEcCHcGNZnDmZ9Bzc4h3RlIOhYByIbzpsG7Txc0aRj3" +
            "AgGG9BZwFtdmHQBH0P4K4etnCHcODgNAR6DUb7ggJuyr/9cUjBvu1WH3LBYEI0uDO/KBocC5QbtSfzewIqgiBz1vDoVE4UUQQgLS" +
            "A6RrLHTTC8j5FcT8Cs7ZHSb3f4V78S3U9AzCmYCUCxKysnpKr0uDdE45q6KVJxIQyoX0ZlDTM8jJAsKbglwPpByA8tuNllJoars9" +
            "t+vJY8Cy71U5czIod5hs6ODMkpYjl13UV9z49RT6a+WaAHuT0HZ4+12l0urbo3F2qgmqSrStHaRt2NNCnYGtuuqra3fQ9+gYVWgJ" +
            "gsn35PRPtu4wZxM7MwFMIOFAunM48wu4Z9dwZheQ3gxCKqP9p+T5dobozv2XKD6gTAEkICdzcHRlFH4kEL0+IHz4A74zB6k1EPlA" +
            "xCCKkG5TaAqXz0QxU/aXPlvs1BD7XPw7tv02Y3G1k9Dbe2dPFjltDecIQGP4gQWntgHqwnbOTklbnrzr8b8kQe4UwluAvCXU2T2c" +
            "82/hXHwH5+I7TK6+h3v+DeT0zGj/hQJE087hpAAAIABJREFUphKseh05n2BDnzdEwB59SAiQciG8GeTsDHIyh/BmMQlwQQhB8fqC" +
            "nEXGTojjXFQkzKV2NWH3IWtta+/qd776zs75oOHlyX1bAE4DRoKobkveIgcWyUHeglJClctKJ1TlcrdWqXYHKkbfKBm8fQKQQ1yc" +
            "IsfrUsJKQtH2ybKqpUPqb6st6h0oqtBknNxDho6FZELnPB/M/PAzAsAgkHQg3DnU7ALu2S2coiUgnRnaDxSdhhSGERhIAMwQ3gKK" +
            "CMLxQNJB8Pge/uyfIHcOWr+YBzgCtBEPsvetZlIvEgAA1rHF3WC92033+6CxLxf4jfnjQrOcQud766ghVR21BG2DFrvLEAQgudc6" +
            "u6WI8gQAIJCQkM4UcnYBsbiGd/Uj3Jtf4d38CvfqRzjzS6j5hSEByjXvcs6CWEaXMaKofiCSEE5sCZidQU6NJUA4HshxzPpEzaB0" +
            "ACxUnL2miTn9WqqKQtjs5OPh0H/IGFgFbNXBvgT1L5MAJMjXHFfMFVQIXX6+HJYK17uN8nVP79bqW0lAY9oHwFAiX6t4BlPN9Emf" +
            "8pejTNAaX8LgRIAlAxM4XcgbT8ipfU6AEAvdkGZR3/wCzvIGztmtmei9qdHmVS4AHjrjlGn7CBDKM2cTSAcAmwXKixuoxa3ZJnRj" +
            "1gmwDuIThxPBjVF2H2hOdwiLwHDoFzFZbT5iF3Duo/OjPaq/U1L76HMl1wKrECI+A0AIs1nA7BzO2R3Uxbfwbn6Cd/0TvJuf4V7+" +
            "AOlNIdypcQUStU7Bw0EICOVBTuZQkQ9ncQXn7BbOxT1Y+0CwBsIVEKzAOspvS4oCObEPLoOlTABQbNTqs1COiYFE9nHoGAj7oIkG" +
            "tbqek3EHSnYHktJksUi8D56tfPpNqDVVdnimDQ45fOTzd0qD1rAoDdeJ+rtycVfRNlO+s0v6B0MhwZLGIPlHSLNvtpAgMuZ586kg" +
            "pAOhHAjpwl1ewr38Bs7FHdTZHeT8HMKdWZP5Yd9eSnYqkg6EmkDNLuCcfwPv5icIKRA9f4AmIIp8kI7A0EAUIWFATUSgWJL9tF1d" +
            "r9hup+L036LwgfTgNPvXUfjfFxIVXlMPydo51z6cv9cFtU9V/LgthcYxrXJ4pNwISaSMa41yzcLf83t41z/Bvf0FzoVx/1HzC0MA" +
            "lAuSqinFQWGIiQvpzQEwostvwMEKggB/eQm9esj+ghV0sDZ/UYDkxHMiYc4g0KHZejjSluLfJgnFYu066g9dRztKWAccPoaRBU+N" +
            "iLVDHzmzNiJ7sDmB4T+/JiCKCEg3HsltG34sDNllmsrSzTUFe+vL+yAAXbLapb2HstDkf9g2NbaIo0f6/U26xbgactPoh2ImMALA" +
            "RGChAOEAwgVLByRc4+IjXShvAuVOIL0J3OUV3It7c7jPeWwJcKcxCTjc25to55ji8woIIGdqdio6v4e3eQERISBCGK4RrR8BhAAH" +
            "MAsBbaEs8c/cYjurKF6pRSqsBdvbfLvAX5Umcf5u03uX9pWi0DJiR7QhAAmynpBY4sr3uiF7KrNM1PaBhntpDLStv1mpWvEZEuDF" +
            "fvfncM7u4V7/iMn93+Asb6HmVykJIKFAUja6/wwJIgmhPKiJOeWcL76BIEB6UzhnV4ie3iN8+oDw6Q9Eq0eEmydg/QT2V/F2x8JY" +
            "QqMIFGzAWiPZ49ycmbBtw4E6RVNFuA6h+s+H1pNUutirh8I25MYwqrCx9JaEez98UDRVcbfqT8aDQtkPpWVvqPKcJSB3VMDpt0+K" +
            "PvJ4qd7tAbnwTnLxmipuDoB9EYDkc2gLydvk9NXoXpYqApD8vqUmc/0rTwAIRqBmkoB0wXICUh5IemZSdyaQszmc6RzObA53eQ3n" +
            "/M78WZYAyHaefkPDnFwMgAnCMZYA9/zemPShgXADvXoAxAeACaQ1mKJYICpoZ6vijz+b5kW2Qza4DQ0ybiTx1Fh4StoFULoHejk+" +
            "Kv0yoiVygv8pjEpZv6vsZ1b7N+a2MJxUnSSeeyc4UTRKcxqwOzeWgDNjCZje/w1qdgHhzSDcGYQzwaHJqLEEeCCp4oXIBOVN4Syv" +
            "ED7fIfj0T/if/gnfdRA8e+BnCYaGhjYuTiSNkiM0p5BTFOYsbXlbHFdcocHinOaye7mKafSJoG3EByIAafJd8tYpheO9q1z4GwLV" +
            "1WRP+JyvzEMP9wKlU4MVAKzXZntQBnM+QF5VUV1RXX7tjloSb/dQ6/dO6fZhYYXwpVMNe8POzK7mylOYBA+HQ5HpfIpDIK/tSU/+" +
            "FC7U4gbO4g60uDV7djsepPIgHA9qMjWTpjeFmps1AWp+CTm/gJycm/2/6QC+vXWlIgJDgISE8GZQ8ytwfPKoIGHIwfQc4csnhK+f" +
            "EL1+QrR5Ne5BiQ8wkgV/1Ta6ZltBsTdUvxM7teLWcTGfcpWwb+4VB7IR/ZGcbZtpxlOtTaNZe8uYyduDdIq3YzxJ1lNKkSoRLWGX" +
            "zAJgxFpytbiGc2FOAHavf4R387NREkzPzE48ygjhB1k3VALBnGSOVFmAydK4PAoJgoBQE8jJEs7qAeHqEdHqAeH6GRwF4DAARwH0" +
            "6gn66ROiUIP1KuXQeaG1QbHQOHEU2VdT2APjQPlo3e0HEDkOKrHU64XeMFp2UF3+KacupPR0kSI3Sg65r0mk5YTYH5QX8uNON+y7" +
            "kOW8Th9XZMMV48xgeRiCYe9aPyXT3wjUkt5Es9emsuJZqkpbzBoQngdncQfn/m9w7v4KMVkaEuB4xpc2/hOOA+lOjcnfm1vavWlu" +
            "i7+DInk3iQAhId0ZMNdmFyPlQLozOPMrBBffwv/zH9j8+Rt8EmAW4HADhGYLUSZtdcCSOj2XXP7XFiqrXSYBbvy69RnK/TwSgH2B" +
            "M7ua/WPHqi4820g8m/JRPUFujSen3UY6AZmtggsrT0iaA/qkA5IKankD7+ZnTG5/iRcAfw9neQfpzi0CkBwgeOD+l1odRLyJgAe4" +
            "AAllzj2RLuT0DM7ZHaLNK6LgFdpfIfJfEL4+ZqTg4T3CkKFXK7Amo+WMo6ZcexWIQP4l7EYECpeDeFC1nTOGC9YaqbzVJuGWL0ad" +
            "VexQaF2mnjjeaN5ORi9CAX8CmMMHAETQMVVgmIkyL/5XJNJzcOyFYkejit+LL/iW6JJwWfmyO/aA0WzB4YrRpSoTHbtHT8NAujiq" +
            "StJpmZ1q/evXju0NkWrzq5D207JeONFaMgMkXDiLO0zv/y9Mf/1/IGfnRvB3vXgRn9GykxCxP2/2B6HMb8ciAQBSFxwhzSJl6UBO" +
            "FlDTJaL5NfTFd4heH/A6OQNIItqsoAMfGsIQAA6Rbh9arPOq4Qew3IkaMwbE41mvd6sPAciSzf+UbN9alecD+WZ/acjEvIpOUhwT" +
            "t1ZxVYAOu1fBbvK4A3QiAKm6P5cXe3tgO58ECRIOyPFAzgTq7Bbezc+Yfv8/Mbn9BXJ2GbsBzVMCYE4RP0ZfiwlMYkZXLoRUgBO7" +
            "L03PwZFvNP46gI5CsA4Q+Sv4D+/gP/4O/+EdAAW9XoMeP4F1XBKBfHuDc/8CyHsSwPxcvxd8efbLzY1N4327qhgEfbntoNgm/pS/" +
            "HocA2OhSF1tkS6oZzg+L7tJabAlYA3DBoTXyWMJ9twwMi047AfbQnmdb9NnoUpG2mq84mrTPzH76Tr0Wak8pHAWHee+GKiVlH5bB" +
            "jcjSjDlTOPMreFc/YvrNf4OzuDAWANeFUNapfqnh7tgtYCM1BYBAIMcDHA8AwN4CanoOHWygN6/QUYBo84rw5TM49BGJR2gW4FAD" +
            "CAGK4s9EMVEuZ17HV/ytIX87ok8MVLgi6+todRsadUSgx7ODYBdVqfV7QgASawBl2nTyFhDTBcR0DvfiW7hXP2Jy+yu8m1/M9p/O" +
            "xLjeJFLrMcmmfYq5FKUaT+wcHGtGGAwdbiCmZyB3ChYK0foV4uGDOQ05VgLk5bBky8cyESj1hy4WgaHQGGX7NE+CAHzlOC29Tbf+" +
            "mnMHivIb8A6Vo52wLRdVxoD0nk0g+jZSXJ/p7pVbc5c0wID11zE6BnLm0Mp4KuqjLucn0b9PgW20QPO4bgl/tkKSAVIOpHQhlAt3" +
            "cQ41W0B6E0jXASkFkqI80nAXveQhUdNYJACpQMwQzFCLa3jXP4IjH2IyR/jwAcHje7MgOloBegPWa0AHYGgQx6aSirhzJyxzIQeF" +
            "7DSeR7Clnw0hDlDxejS1DQYuXuXavaAQydU5Vf1YgQMMRKXOW7gdWwcSK6AQDuTszGwTfH4LdX6Lyf1f4V58Czk9BzkTQLnxmoEk" +
            "ktPucLlThgkgppjsuObcA28B6S0gnClIOnk1bDr3ZQcS2i3LsC5snUzL97CpeU56mhqwyfuIVQeply1jd1u0MhRWVAKVfzp55EiA" +
            "zKkUOf1LjI/H6Nx90s21DxWe36F18mbEplxV39upY7QwtWW/N6zg2JIJO8q2+S0NrvtCSZgbPvr9xWfza2uyissjpAPhTaEmMzgJ" +
            "CZhMIRxjKjcTeN3R5qeIgtYNMNYBoQBFMIsXr+CGP4KUhJwssPF+iwkAQ/tP0OEzEDA40ub00HQrwLwQT1YyBKSnPRZlPJsXZ9uQ" +
            "2rCZGWo7dJ93JAtPaZ5tS0BZufCWppHTAieWbKs5K2uzNIkXZvPal63dYFzqRl0GytIxuIVScEICjPuPnF3AufgOk7uf4d6ZNQDu" +
            "xbeQM0MCSEhAxGcBnDgBSMB2AxEDJMzaImcKOVnGJGACkgosBIiKb2YyMMS0v67ZLCJQfPXqmoqA2ld0azO3rv5mUbtLK+6ryduM" +
            "VH3nrK3PHYCLN77pWwpee/vEXr+yJUBrAPEpfJz3sj1VllskZUWNdnrds/Jz5c7U7MdHZYPkdWGH7G8H6R9VAt4JopYAgMp5Z4Ck" +
            "Y3b8mZ+lJEB5hgSQUtne2G8K+emQ4vMPSJjDxNTyGqQU1GwJNT0HSResgWizBlYC2DA0AgABEIVIzHHFcx1tC13yelb1Q5sgpN/L" +
            "ITqXrn1oyq4o+SW2boxy/0Dg3HWR6rVqYVud17o71Acu3WkRb9qf005rIXEFEgpCeRDODGp2Ce/yO0zu/4rZD/8Dcn5p/qbn1jag" +
            "b4cAlGFOTSfpQriGBIiYBCC1BFihE7V+LNnbMkGp+ovvXtFCUJ2dFjmueLZX9ZcHh1MgAAmahq69yQNHFMG6DAtv4W2r3kzc2kbI" +
            "Ng6ciOhbi1MlKcdE5aC3x7RGNL/4ZF2REEjWAajZOdTFPdzLe7g3v8A5vzf+rzIhABXk4c2BYsscgYiNBpO1IQXMiAIfmgEIhfD1" +
            "A/TrR0SrD4jWn8HrF2j/Bbx5AXRkGRk4N7kjuSZUriWqFMhg/9jAEnq9SOZcgOKJtMSExIM58cJ8++17ZDRYRu1OslUItJBakmuJ" +
            "Yzt0WVKcy4ptNQSZrX+VAAkXan4OtbyBu7iBd/MjvNtf4F59D+f8zgjIkzlIuany4FCHge0NRBDS7DDGUQhndg5/uoSaLiAnMxBH" +
            "ZmzQEcxK4cLLa20T26UPjNgT9mYi6BjXV97ule5AxTrmmt8OgSH6CRcvvpRGb1k5u9Zhm+o6GAE4aSsANWpdMh/XeEFfupOPglpe" +
            "w736Ad79X+Dd/gp19YMx5cduQCdc6O0oSORmAk5cgzwIEORcw400SE2gZlcIX/5A+PwO4csfiJ7/QPhoThGNgg3AOu5vnO4KlCMC" +
            "yXxvy081wnzJSsh1AfLPFpuDKq6Kycb2kNJ/IwZEPFklLqxkNSNZjdlOmM/IWaaM5+7n0VSEbpW+RQCYAQiRuvWQO4G6uMHk5hdM" +
            "bn6Gd/2jIQAXRnkgnAkoWQeAN2wAsEEEUq7ZdhhGceLML6AW51CLMyDYgIMNEG7AUbbLYa6222j5h8xyMY0BZJC30JTb+3ZyUdEK" +
            "h9Ymcu6jFexc51U4p6IKbc9uahYGc/pfXfRvBZV5fevs78ANsK26DpKdouB1Yu/cNi1bIpymEzsJs6hPmcXAcnkN5/oneN/+d7i3" +
            "v0Iurg0JkCrb8vUt99miuR5sTjUmMgv7hAKpCeTiGu7VK8Lndwge/4Xg6QrB56XZ1jDyoV8+gXUIgjlHwF4QnH7GX+zdIIuntdt1" +
            "STDqXk46GZcC5DhB6XtTsWE5pMRuHMnvOSJQ2blH7AR7zEisALE7Wfl1Ktd5nvslunwCqGYz2prJpm6IamzlIgFI+o6UIOVATKZw" +
            "zm8xuf8F8x/+F7yrHywXoLPY5S7ZCrQpobcDgrEEkEcg5RgCMD+HMz+HsziHXj+DiaF1AET5sQFA9l5bA8MhppGq6m+7CHlbPKeI" +
            "xrqsvGm1wiG1y7aSqAeq2+MQ1LItrMmuoaNbJCDI37FPFuPKy4PglKp0RBmHapvaAfAkOkgLAlD8JVnk5k4g3SnU4grO5fdw7/4K" +
            "9/ZXCHcSL+izdwV6K9NAG8SWAJKABKQzhZydw4kn6ODxEmq+hJwvIBwXHGygXz6DlAMKNwCAZMegHAFAPPknRACotQJkPwCJu1Xe" +
            "bScfrCRQWt/zglb2JZ9+IvILU24IpC5BX1TbngA40f5XKDHInhELbKBADrn4PWGeVYNOyQeti9XBfszOUPL+Gy24cKcgbwI5P4Nz" +
            "eYfJ/c+Y/fjf4V58H+8CZMaNLEtfUL8iglAOmB0IZ4JoZgiAsQRcQBMh0hoIfOgoRKLMrGqTY6Nrn/iCWrEeh5IzE80/Z69azaZz" +
            "PXESQkmGwnBSPDW4YXegbbEdB4NUb82gXzNlnB5OqI8dLCtl+9vJgBr6TE5wBJAsAiUl410uzs16gPkVxHQJcmcg5QHCiQXFL5EA" +
            "1CErI0kXwltATa+gF69Qs3cIp2eQkwWgQ3AUgKIgNfsnKBGBJNoiEYhFRI7PZ8g09ElEtnCYqYxsYT13lfMesogFwwy6DAACpGYg" +
            "NQXJidnfXDgwPtsn8kK/KeQHBTOPJ+0qoElCkwSIs+YkINdYyTe7rUtNn5GGzK6TaC3N99LUmfijtTjDg4sXSfZIQngzkDMFOVPI" +
            "+TnU4tIoDM5uMfv2b3CvfoKcnJsxQzqx+0/cl7/oIcNYEMXkDGp5C/fqB4TOHKSmCIUHrJ+hQ98cOhZtwGw2PDHCHicGneph1fpt" +
            "fCvboVJ3cmqV15SfzEj71aHgDhSRmbHSWcvCnmuowVyx3WTXQRTd8tK/mZ5wDCJQpwCLP5vM3oOY3E6waTKhMZ25y/fSMObPnG5p" +
            "SICansNZ3kDNLiEnZ0bTp9zYRSaZ0E+w4AMh0VRmQlS8+490zemhsxAqWJv6mZ5BTubgcAMOyEzsOjTPIy/fARkRKM/1mZAOkmAS" +
            "YJIV4n22eDf7bt/Pk4KMSGTxM8OcdZYsBHamgIr/ciTAxhtQRJwMzOiS6dxjUZ2EIQLxovPSaZ72YoH0k9LrzHhjP2RL6cnLHKfI" +
            "bPXhxAxVZ1bKx5a7bUUNZU7clvMryPklnPN7uFffwb38Dt7lt3DPv4F7cQ+RnAUgJCBkhfD/JfUlSrcCFdKFnCyhFrdwLn4AiSkg" +
            "PDAcsPBA/guweQbrCOAortbIxMIAE5ldhGqUgoNOrz2b4C213BAEYG8iTVXEWyrXfhWrgp6oTjJDy4xV7w50THSWGO1RvKMho9IK" +
            "8IZwyCy3qOamPtcnqyf7csWonjsKLgacyRvMDOhYOCAJoaaQ03M4y1vI+QXkxLYExFuCnnolDAQiyhEBQwIWAAlwGGTbHXpzCP8V" +
            "zBF05Fe4eiC/FgC5pkAqopMRFEHC7J9esLpQKjXGLjv2Cad5yTEn9Jt04mcJ8fkGiK0IwlgB1BRILQFJ2iP6w377KP5LLAEKEAxh" +
            "BcsRgvjlZJu8FyxAeaE6fqET/wE2rD4n7qf/UOEzF0v5uy11MIEgQe4ccnEF5/JbeDe/YHr/F0zu/oLJzc9mq0x3ZqwF8foa26r1" +
            "5SJuF+UYErC8hfbXhgCQA9YSGobcQ0dmoXBC0lgDTGDieksARgKwE06ZAPQgA8ljVeqAqnsngQ4Z6ugOtAP2LbASwHW+mj3iemt8" +
            "4EvDyb1UBdQoj8z3VMAQICGQ+ICTcAByAeFCza/gXn1vdvS4+h7O2V2m0ZPmUB8qqS+/dFiqeyFByoMggpycQU4voRbXUMs7gBmR" +
            "UAAY0JGxCKQTPKfvb1H8MsK8iAmWscTAnYPcOeBMs606KRH0Y4Eq8cm2hfsCEaiSMFO3IE1IXImc2QLOdA5nNoc6u4eYXUCoiRXv" +
            "iM4gmG10hTR757tTc3jW+T3cm59AOjStmdRv1oyZ+33BIpAnlpS2qJEcjfBv9ztmDdYhWEfgKIqtVT442IC1zpMGG5kxwcDaMUxM" +
            "lnDObs04cfMzJre/wLv5Bd71z3AvfzBji5DZDmJfGUg6xmVwcQNoDZITkDMDuUuI2QXClz8hXj8hfP0EHazAwQo6XIPDDaAjEIcg" +
            "DsGsc/ak7Ql3yWSfkg2Hr69XFNBLoVz9WJOw354InF6L1OwO1AFdBOYdhGsufNpxVllTqxNPGquimF1KnnM47oKT5IxbUeoVPdvx" +
            "zXKrQqZtRWEqO1g/ZhxAQEjXLOhTHoR3DpqcQ3rx/t6X38K9NCZ9dfkt5PwcQrmFRL4+GIWmALECg8HOFHJ6AbW8h3P1M0g4IOUi" +
            "pNglKAqAKACHIcBROcJE9iICpAJJ12gRFzdQy3uo82+gZldG2EsIQCrMUywE5oX8xAYA+5PyvyVjDafWIAHlTSC9CZQ3gbO8gXPx" +
            "LcR0WVRPD1qfXypsl3sSEkI6gDOBmp7Du/oe7K8gpAJiIY+Qvad2dXPhJS6T+uQXe3bRsXAfE4AogA596MiHDlaIXj4hfPmE6OWT" +
            "2bYy0iCtwTpzXErJqUUEyHGNtcszFgD39kdM7n41WwdffAfn7MbsiS+lyddXKPwnMGsCFlA6NO3vLSDnV3DOnxC+PiJafUb4+oBo" +
            "9YDw9TPC1WeEqwdE60cgeAWCFyBYgaIwJnTGWpsb7tPqLVh4W2Vw++26+XDQVn2bYsfu6EkA+kaZjg5NGsJDgGq/ZD9ZBWltCag3" +
            "bFrptKn0HpLgtopvey8bfgdAutVjV7G2vydZ8YmmAWQIYbtxftnVxPkW2AA3F5OseSHZ5tEmCEJICMeFdKdGY3X2DeTZt1Bn30It" +
            "7+Ce38I5v4FzdgMxvYCYnYMcN5NQvrKB2wh1iTZeAFKaenQmxhKw/AbOZg2Q0Xwazesa7K+hfcSHBFmSYeKtYZEAEg6gPMCdQi3v" +
            "4N39BZO7vxrNKolUsMq27SxKjbagaN/LlaR8HacvlAOpXAjlQEzmkLNLiMky0zZ/ZW2+KygZ7IQ0hBsEnp1jcv0DhHLgLK6MhQjF" +
            "5qxk8BXx2/csVVSq/Y/AWkOHa+hgZQjA+gn+p98AKaCjlTnEClFp8TCxtZYh7qNCuRDTJeTyEs7Ft3BvfoR3/ytm3/wNanEDNV1C" +
            "eov4oEHKl+MrA0kH0pvHBGAOOd/ACTbQwRrR5hXR6gnh+hHR6gn+0zv4j+9Aj78DT++B9WdgRWAEYNJmS9FIw/YltL26TN+JXccK" +
            "FV6q/g7tsa+mK8XbX+x4m9iJAFRVUg9pq7GuuwjNLaLpetNSgrSyBAzeb3pKqbY1IKfxt+b9bc9uzdfB1NXd6HlVyG2ahF2KsXcF" +
            "06mbBYoEoGgNyCl/M5EwETQIhgRIx4WczKBmZ3Auv4Fz/Sucm/+Ac/4N1PISanEJZ3Fh/MNjq8GXuSVoO2T9Lj4lmQRgWQJ0FE/E" +
            "2ocOX6D9R2gApCMg9GEW/lV3LoaIDyibgNwZ5PIW3u2vmP34f2N6/99AIrYEUOIGVJWxpK0Lgn5J+1I165pTom3XFZKO+SstDh7R" +
            "GkmbSQdEEs7U7CfvzC8R3fxoTQ4WdUuE50YzZ5XNOJl5tHH7iYlA5L8i8p8RbZ4Rvn4EJEFHKwSvH4BwY/IQWbFZRDXdq5BhrIaz" +
            "JeTZDdT1t3BiS8D0m7+ZNUNSZS5AuTx/fTAuUwsIb265Z2lAM3S4QbR+Qbh5RrR+gfzzH6A//w52PWgpgGcCwweHz9AcxQv4Ob+W" +
            "+41W6xvN9nAYnAAkvxfHdKq4tyWaIdGXABTQuDD4aJ3JqtNKq0P6gvan3PGYm35mNyqeHcYZbMRbRs73IP0H5rRaYRYWChH79Jpd" +
            "OsT8AursBs7yGursFs7VT3AufoRz9R3k/BpyGk9g0trebxQGDdL3nIymz51Czi7hMIOIQUoacrW4RPj0HuHjB4SP78HrF3AUAmEI" +
            "DkMk7tg63kmUNZnFuhAgUhDSg3BmkN7MCJJkrcUgOyt5rf8WXWD+nnWb4sXIFO9M9LX6cw+OpM2EAKSK11nALLwuDOo2cW9T9/kQ" +
            "iTaY0/UorDXImUJ4M4jJ0uxh//qA8OUj1PwCiCJoXkFHAILEIhD3IyHNbmDKAykPzvUPcG9/gnvzk1kHcPUTnMUVhOPFBwcWx4iv" +
            "uO8Q5WQBs7kAwRzm5kK4ESQZsuBGG4A0hONATWYIPk4QghFs1uCIzFai7OfcCetEjOPor9oLGo0h9y2vDOW2sQv2qR3tiMz93K74" +
            "ggKpjz/9QAQA2OIOlH05wkCztSGL2rfC/W1xF1CZVFMax0Ypw1x/q2f0uxW9+HSPjn5qsAWGVOVvtpc0W00qCOWlk7pY3EFdfg/n" +
            "+ju4V9/FbkD3cM6+gZicQbgTCCe2AAgTR7Wm+WtFXAfCbJWomM0py44DOZ0iWlwgOr/D5uN/geQEWmtoEGizho42xlXDeG7EHiEU" +
            "L9I1RIDixdqGCExzBIAKbV0l+pfyubU41nqD5DrZBWrEjjDWAAbH1h6GEPFp1LlprWjhKV1UxJzAjsdonjn2NRNRAB3NISMfkZog" +
            "fP6A8OkPBPNLsO+DIgB+iIjNu82xrwlJZdzCpkvI6RLe7Y/w7v8D3v1f4V3/BGdxAzW/MiQhdoMrL0r/2kHptJH1AQlyPEgSZq0I" +
            "sSEA0yWc6RlWANhfw3/6DO2HsTIwAmsCRMEMnFoHioqBmhbY5nqzTalYi/bzYsk33Z5RqqJCDD0YAAAgAElEQVQZPK8dwu5jP5pt" +
            "8mPPSNt6VZkdymzHsS2Koi7y0cCvfYM70AkMMIbUVxzQ2JC3bfWZVx40RHcC5a9DAwEYCsMSgOS3bfm0G2R/RCBJpXcKli+Q2WZS" +
            "gUmBhAuWM7MXvDeHWN5BXv0E9+4v8O5+gppdQ86uzKTuTGOLgci0wZSdIDsifvmRWQJIKuP7O12AF5fQ/jeIVp8BNQFrjXD9jCjw" +
            "jdtFEIGZwJqhLRLAmgAtABYgxCRAWSQgEdKTLFg54tJFHMj62Foqe+BJBbqxvXdGLKQRUbwNkACxA1a6+TFrPihQhRpYypacS0/m" +
            "GiTcGZynPxAs/g01u4BevwJ+AC1WSE8dTlKRCtKbQS0v4ZzdwLv9CdP7/8D0u/8B7/onCGcC4UwhpGvGirR/jn0GgKWYsawrMH1A" +
            "xC5izBrCcaGmC+izWwTzC+jNCv7TJ7DzL2ixAkQEhp+5B2SrNXLCgmXEKcrYWTZgXdfpwpruDYVcGsnJ6jX9Zp95PTQBSNBF5GiB" +
            "tjnNK887Kj8rJ5xCsIFR4w4kjEtuoj07Isr10I8AJES+HXqW+SgK7FPTmg/FePeD3XuzETBSoV1NQGoGqCnImUHGPqpysoBz9SOc" +
            "q5/h3PwM5/pns9Wlt4CcnIGkUz5lFMDoGlIBEhDKBSvXaF29KRAtwdEl9OYK0foZwcsniId3oNUr4GswNtARoDUZN2GdaGAFzN7/" +
            "EkTK7DQkHZB0kS3OtfQ32wbl1mWovKz9ZURXWIyNjIte/GubJxvC1jd6KXy8DSgJCTk9N4f/TZYQzgxavoBIWUKmIRFEsZVrdgHn" +
            "/A7uxT28y+/gXX4P7+K7uFxFoX/sL2Vk1gDDq00fSPWajgfmOcARpHLhf/4Dzp+/Qy7uoIPQWAt9H4xVeoBY3EqwNYeVM1jb5jjG" +
            "1FcQ4Ftb+It5tR8cjAB0iKcvBhI5ytXR1lbfgwg03d4DKtyBKvwNC3k7pCi3Na0eyuN6xX/dW14Hzn10x65ON3tqBeqbs7ZTLpcu" +
            "D4HG3LUdrAQZdwPpmG0mZ1eQixvI+Q3k7MII+d4cYrKAe34H9/o7OMs7IxA409zC3/wBWUCfHXq/bNgdJNGUxG4fQoHggVQEcuYQ" +
            "3hnk9AJi8gisfDC/IAqMJYA5cb8QAEsQHBC5hgCQtNwsUE8A4u+9FFeNzTq2+Wmj48TCscBO0pwGLTywcKGhEGmBKIx5QPyAYAXI" +
            "CYS7yE4Nd6ZmjBkJwFaUWsd6SUvjKREACZIu1PQc7sU3mN3/BRuhEAqFIAwRbl5hTvdLzYcl0aAkC29zmTm27qstEdiWxyHLwENH" +
            "2JTOfqIo/j54Mxenv4GiK0ZV6Q5khh06ar9NMGQe6uuxT/MNlbO+RGB/BKD+3lATkCVNtYkyDdqsitjiBbY9UGWkpm8kYxZBAMIB" +
            "OR7InUCe3cK9/hnu9U9wzr6B9ObmbzKHmp1DzS+g5pep9p+kA9sHfBT8tyFRRHC8oBdgkiBhXLFIMcg1JEBMLyC8z4B8gWYHUYCM" +
            "ADBBEAFaAjCuW0SO0c6mpwJbqda0y9hcXyOqGp2tf5NQlGqhzRqhmASQC4YDrQVCe4cgAhgSJCeQ3hJqdgXlxSRAOsgr5OryMaJU" +
            "K1UvKRHAAiAGpAM5O4d3+R04DCAgsApDRK9P4Kc/AU7OG2FjFbDizMllhGweq/Ils9xpes3WXRTIbeLqYxHojYbMH0KwHCiNoYwe" +
            "vbGlkXKSa5HztkyiYAmQJj5R0IZVKG4PRW5bpXNwpr1X570jpB+DKi/jHwYeMrqoVQmpALgl2LBudEmaiY9ook2WyTaTc6jlLdzb" +
            "n2Mf3p8hvVlMBGbGn1eZxb+JBSDncz6iAzIhPdmNiaDATBDOAsJbQkwvQJNzkHwApyQgfhYAhACzBMgxJEA4ZptOEpm6D2PzjGiD" +
            "8mhjRnEC4hPCIRywNCeEayhELBBFdlcjMBsSILxlbAlYZpaAsSMOCErrPLEEeBffQAgFiiKEL4/wH/6AFg5IAwQGaUqfI0Z+/Ujq" +
            "gdAwj+1i9N7HFH8wIvC2CECfLFVZAQ4Nsj53yY9FApx+JwbvGbu64dY9X1nQkyv98bB/TcEuOAzrq7XkkjAndioHwp1ATpZQ8yuo" +
            "5TWkMzXbWbpTCOmYbf3ivb1HDISYTDFzfFDQDGp+DffiO+hAA9oBxBTCXSYPABAQjgtncQFncQ5neQ7v+geoxRWEO8FIAEZ0R350" +
            "SM+RFgLSncGZX8G7+B6IAEETSDWHM72CfcCks7zC5O5XuFc/wDm7N4fHubPYHWjEPpBsNKCm5wAD0dULpptXsA5B0gVvHqDXD+D1" +
            "AzhcG6uAjsAcIXGVzU+Q7eajvc1apz1Znzy6Vt0peMiU0Ln9swdqR5q6OI/m3tYz4Z3ye3Iv1kl2v5NAW5f+3vFaNjdzsqzZ4z05" +
            "EdhYABbxTjNu7Ppjzgs4wY70RSA53Ve6czjLawARSE6hJpdwlveYXP8UBzTbcArpQM0WUNMF1HQJ9/JbuBd3EN4sju94ZRnxNpHs" +
            "9pNbJkgCcrKEe3YPcATlLhDMb+FdfIfg6TOAZOtAhpqdwbv+Ht7V9+YAwfkVhDeP3YFGDItYGiAyWzNPFiAhzbkiAIQ3g3t+h+Dz" +
            "bwge/ong82+IVg9AsAEHPjjMnyhcWIyQ/dAw4aShhh5rdiECg5GIFjNts0dvqxSOOUzXZ3vHgnVEsQ6KXjrtIZrPCbATOJrwX8zI" +
            "dstbKdhJ5H1nHLYEXV+0tm6J+8ChaoYRC4pkTn4V0hz0I5wJpGvWAiTaf6GM739yONSIPYEEhDeHwg1IeZCTC7jLe3jXj4heH+MG" +
            "Myf0CqEgPGOlke4UcnZmFnNPZiMBGNEbxX1CSEjIyQIu3xnr0/wK0fkTwtUTotULkC4NZkh3as4CWF7DWVybMcSdgeRoCdgPjBJH" +
            "KM98Op45XM6bwTm/RXjzI1Z/nGP9ToKxAlMAXhNYayDwKzQFnLcIlPwyyo4amTwy8MxVIwU2iqfWuoXdpOuOZekhlPXPqh26f523" +
            "e3K/0mZTuZvrpL4OGs4JaBPx/lF2uWvXBbY1xdZY+lOrPeC0Kczhq+fw9ZGagGH2Iqd4f38hFYTjmoO/3Jn5zd77HzQKmHsEgdIz" +
            "BKQ3h55fgoM1dLAGB+vUCgASIKEgpGuIm3LThdok3WMXY8QXBCIB6c4hpISazKEX14j8FXTcL836IkMCSLrxjmILSG8Z90cVn3Q8" +
            "Yi8gAVLxJg1g4xo0O4MO7qE3zyAZQodPCF7/QOQ/Q0ca8P2cHEK5f7rPRyehmBwsA4dxzS1+P+S02q2E+2nd/uVtshs0uAPtmuxQ" +
            "aFuVVZ2kaxyNEey9KuoTOX4rDIMmC+p2HPC1r8hTuhgstgSAyBzeQyJ1S0luZbYze+nOiL2AjNAFIQF2IBTAMAI/Sze2wsTtI6RZ" +
            "CCyT8wGURdZGjBgKRkkAdgBpiKokszWlcKYpAQBgiGl8IBhJx/RjkuOuYXtA9XRDmYJAMTjyQM4Mwp1DeAuQMweUD9Aq2yAid6Zq" +
            "YeVYSXlYN8lx/s7OAkvh+cIC4NI9rgg69BTboRx9F+funN1j+xeVcHh62OAOdBo1U10lzSr6pvepU5sfvINUDlH7iLY24i7p9cnb" +
            "NiLQHOd+X45E2092UlZXSy27RCBh/szppJQSARNwr9kckUNS/xKQRvhiocCRC6jQapMsXEIazKcYBa4Rw4JgBHkJ464WE1JSZoFp" +
            "bnAhaVmknMxyNQ4ie0E23VgDekwCAIJwIkPI3DnIXZpPuQKRgjlDjDK5ICfkF4hAKdUEXHvH3M7/Ui/7bEFKWKruNXytKkJtV2zI" +
            "yzY3jBbBcgFq8tBLRDsZD486HJYIVLgDmRM18xk6HpqrorkL1FkHTo78VWCw/NVVYEUl7JsAFJ9l+4ea8eqgqNCO5F7HTJa0hEr7" +
            "z7IEHL0wXw8SYd64WSiQZEDVdHyLFJgmGttqxNCISSkJQCiz13ys/efMrzAJaY0llvA/EtO9IUcEEI8fksy6IWaQO4Nw5hDuEuQs" +
            "QPIJIGUEa+J03DDx2ARgV+Gt3OblGNukUTl7dcJ2C8HuBKBVJloG66TYfRM4HBGocAfSuW/HrrMuVbFz32vVmw5TI7VZ2cNLdBLY" +
            "ZiI4OpIh32ICIvE3H91+jguK/x8F+hEngmQ9EJDrk2P3PA3klTsUe/kQOHYZhHQAZdYPQSjYmzvYe0ENN0vV94x+RCDGUBrPtvHs" +
            "2QXoENg9X6dasmrkti0x7kDlnUwyD8YG7KncXaJtJb9z7deBc7M7SqntmQB0r4+B0WRGPdLsWWW85diPFPGhVZyQguKMP2LEiBEj" +
            "Th/JicCxlZeENMK/kLGyJxn78+g+X1bND30da/eLkjzA9h8VvhcD16Cju0GS1DHQ34YyRD85qjtQcxZ2EkxPGcdiOS1Spa7J75jV" +
            "o7pLVVoECpdtTxreAWkdWPlhIPYfJzAEODH5Q4BH4X/EiBEj3i7S3dyS3cTic15ia29uXtxpuB9qrig51u4F9fJAiwUHTY9tCcp1" +
            "SewRu9VkngDYcXUrxmFlzQpLwLGyckBww9dKRnvcmugk834JjXZq8jQVXuxU8y/AqHAHOrX8jxgxYsSIRmQCfuzqWSABVeP61zLU" +
            "txcrOtTICVfe8TT3hxfgciTAWAJ0flnAAbSuR0PrhQJHRNcFs20Dbwl3mFJTzfWe0HbRSIMtr3j2SyL/p0a0Ex7YRowYMWJEGcVh" +
            "m6z/0s0E0ptFXe8ApvfjPLwnHH4SbK6F3euoc4m4dNHloe53dyhizTkBGmWVOOdcHk6x69lokunKgTuFPhy6EoAEA7yDh3iNyfq3" +
            "HtvbY6t1scuK8aJZqGqcT+yUhayPG3qMGDFixNuAvacPAGusTxZ1W0TAfigX2EaHxbrbMrVTRKcgwxx+Y48kteppeBgiUIylaYUH" +
            "MdcZkCrQLn91oXIu4x3lkEZ3oHLS1Yse9old5apTXvhbBNl/fQnAKaKgSGmPbu0x+G5KW8b59GUvSP9fTLuNGDFixBeMKpk+na6o" +
            "QAAqH+5oDRhs8ewpCPrb8OXNhFT4qwSbVYPJNZrC2oGGQsfoLBIQ5BYG512CdsvTSWIgS97XiL24J5VwgIbhysvyPauv8NaO8+UN" +
            "fCNGjBjx1SDeESJxBUrOgDmwRrLhsQ4RDTQd9Y/m1FyD2oU5XK6HkXN2iaXgDhSWIjy2jLxr+p2ta0UXjwHy0BnHrvRTQJeKH6KR" +
            "Kjo929exdxyxHXQ7kxxdhEaMGDHibSHe9yE7BqZqi77BhPqqDNQFPDwB2B2mMCcr1gydsV2WOlbIoL0Qx2MXrS7aHAmw3YFOtsFa" +
            "4ijC+66oajGuv7UPNMV98DGlwdm/VB8D7ZaWvAK2GZjTNogHsvQE0HKCuff/ZAbhESNGjBhho2pOJVibv1WsCS5KaSWBqYos1KSZ" +
            "R8PWQ32ksrZzzxbBYih5tDqm40houeqsUPpWXfdC3wiqqqVjXLYCc1s8FbsDZU+fghA9RCfs42J3cJ/8Bu+SQ7XDSRGAhsQb62On" +
            "jNo6/pzKH8wcy/4MTulAvSVgJAAjRowY8TZQUuUkloAiGagSGlu6TjTP4w13txCL6vAtsCXK4QjAPlPohqplf5y7GChXQxetQ/PX" +
            "Ba2TVsrHA0MnDImS2hlcCB2gQIcM2++BHeNumd6hXqPexd9HvTWRggEzyoU/OwOJpYASM0TOHHFA5jZixIgRIwYBA0hODCahIKQC" +
            "xScGp9NOcdLNqY+3CfO9c9UOAwoEX9oUVkUAqn+s+L3uty8AlVuExi4POXMAD+mrtCfYLkB15LxtMZpMRoOiqT4KzLQuK4dyfepd" +
            "FfsiAramvmc6XPy3oTKZAXubXKq5zkU+WgRGjBgx4uRQHOoTdScRgYQASQUSyhACEsgdBZPzmUBunN9qqa4NsIVAbJvf9jDXNE1h" +
            "OQ+ovtiT8LJTlC1kshxOZI7vW+aSJSDZFCgRi4pa1r1Vbg261O828tYn7yUt85AdtmNcp+Cuc1Jk2C70jgSgToZP/zj7tEe//D7S" +
            "tr24OEuMGDFixIhTQuWmP0QgkhBCGSJAEiRsy29GBIrPthruKxNt8VjRLan4tyfsQ67KYc/5r8Qe5LhjFGMI1JwTYKhAzdrHfjiA" +
            "MNTUAIN22iGwh0x8lURgJ9SUJPHmqVlsnJqMDzUKjxgxYsSI/YAZxg1agzlWg6YuQQIkCtuE2lr/r2TY/yLm/CHl2QJOuxs0Fzjn" +
            "DhQRETTyZwRUxVcjHPXLQv+wxYcGads62jtkK1ekMVT0h3IN2j+qSzJU2Yrn4jW5jxWzQolKRpCh0adgohkxYsSIES2RuDUwmDVY" +
            "GwKgww048gEOAUQgMIjMX6IyJeJYJb/jbNQximPO632nuO1FPAGJxZIfbUP/ob1eWsW5szxRyJi2SUAAILLDNkjWexD+d0wKQE13" +
            "2sWHK37Xd23PXHJHtogcKAsDIWObgwn/gGXiqrm/7TeyiEByLnhqqrXe1AqPoJETjBgxYsSRwfGswmwIgA6hoxA62EBHAaBDEHRM" +
            "AADEZwbkIqi87oiOCtVDo26+6jKPbS/ikYjAvtfs7SP+3gsxuHwZf5bdgU7spOCtZW2ibTu+m23ruSnsoQlAG7w9QXTILQ/aN0LO" +
            "JSjJie0Mmp4mmawK4CzMuCRgxIgRI04UjPS8F44MAQj92BIQgDnKSIDIiMDX5Pk5dDGb4zt8peaWFA653vMQvuc7x5tEoGNLwAqA" +
            "ss4JSCvEXOzRlWpr3B1ktvYZ3BMDPAHDVmt0yys1fNs/hns3O8RUrCAiozViBkcBtL9BtH5B8PwJm8//hpxeQWuGUA5IuhDKBUnH" +
            "/CkHJByACAxDGkaMGDFixGHArGOBX4OjENpfQfuviPyVuQ7XiII1tP+K8PkdouffET6/Q/j0HtHrIzjcVMabP0F+BxxEeChqtA6R" +
            "5umiqehfU7WoPwHMAGT+QPGi4LgaOPetGn0rrM1zuX7a5oF9MpYWsPNb3nUAJ9W7GvOaC1X3bf8Yrrp6xBRXUOISRkQAa+jQR7h+" +
            "AQRh8+e/ATGBDhnu00dIbwE5mcef2Z9w56DkHHokZ9GPGDFixIh9wqz71WAdAdoQgODxPYLHPxA8vUe0ekLkvyDyX6A3z9Drz4jW" +
            "n83n6yeET39Cb15RWnw4FAFIsFf5oGa+SdI80HTUpYgnJi4NiyHqvLdrUB75cwLYrI3PbxPaLh9d0UVp3zmhHpmyuUNapz0rt/Gx" +
            "E/MB7GKiO6TYOmz19Ist8fxh+wfW0MEGWAOsQ7BwoSONYPUM5/M7OPNLOIvk7xocXYOEgFAuIFQcacUZfSNGjBgxYlCkSxtjCy5H" +
            "PqLNM/yH37H54z+xfv+fCJ8/Ilo/Ito8QW+ewOEKHKzMZ7iOrQbrSq3/4IJqb/mg/+w8xLrHTunFn52VwHtK4+CoFDZ3jK9jPLb3" +
            "U0wC1gBU5g5UcAGqddXplm5jhvaNNp2JrYBDkIG3jeNaAIZD1uq7DNgEgAlgaCD0EXFkFpFpRrheQT59gpq/g3t2C/f8DtHqFhwY" +
            "E7J0PLA7BWR81JiUeMs1OmLEiBGnDtuVmHVk3DiDNaLVI4KHd1i//0+8/vb/IXh4h2j9GdHqAXrzCHAEQgSwRnHGSIlAwShwXOxA" +
            "AKzP0yhLGTvP2y3upTV4qpWwR5QsAfnb9TXSSqDu+WyWnW0h9qGqr3+kcqFovxycOMqlYj6cB8vR1tLkRoy6hjZ+pdBkSIG/gcYL" +
            "oBmIIiAMwJtX6OfP0KtnaH8NDnxw4ENMzyAnS8jJGUi5A5ZsxIgRI0YkyJyZjdTOwQrh6hOi108IHn5H8PBPBI/vED59QPT6GTp4" +
            "AcINoGOX6JqVom1+GQStJd/dCcCwsXZLnxt/aHWrMmyXPPR+2EKiND4ah9jBqqAAYwdwKmOtT2/oe6WwduBK37uGElP+sn/DFJ7+" +
            "YgjANn5cDY472pDl7t02+xp7icrk01obkCbOERAxNDYgzeDQLBbmzSv0ywNC9z306gUcBIYcaA3n7A6AMOsDqsrxdjvUiBEjRhwF" +
            "1VNBQgDMZg46XCFafULw9Dv8z/+A//BvBE/vED5/QPT6AI42QOSDdJQ9a8dG+ajtFZN7QxsJtWcWynZ+3n362RZBRV5LbjsdiEBV" +
            "0KosdJMB+zvaN6cx/MKLUnpVGaDyZTEnCgAmiI8IsNyB6mI+OAGozE2xMq3iUe4jvdv9XYnjJAKBwQMLv8dDkwGwRQkH7MuDDaEt" +
            "I9puGjQFizcCyt/LEYDEaMaA3oDDANisAPEM/aIghAMhFPT6NdYsxQyCBIQ7Ay+uO+V7xIgRI0aUUTuEMlICYNZxJSTg3/A//wPB" +
            "478QPr5D8PwBev0Cgk7/AMRzXBx7wWn+IAQAxWl2y8LenVPZMaI2MkFbAX9LuK7JVoWrlZfZDjGUxGc513NSOEvU7pFM65ZqkVSN" +
            "O9Api7vbukF9D8pVf2MRaf+2naPYjtpZT7ZhCNegTkUvBu5Zb0OR1EwVZCYYTvzEGNAkoEEgCJByINwJIJ10ZyBSDoQ3NVon6YCE" +
            "iu+TiQN0OL+rESNGjHjzyMZjTvb+1yF05INDHxxuEDy+g//pX/A//gb/z98QPLxD+PoJ7L+Cww3s4dcWFEy02YR3KALw1WPP8lFt" +
            "9PueeqvMGfuGLf3r8u28O1BsCbB4Cw5OCBqsAH2e34pM4Q+yylplLhscbejpqWIHotydAPStGK646vJUPVKbGSeKoqRCkqc1dLBG" +
            "+PIJJBWYIzAYxBGIQ+izG4jJGaS3hJwswUICQoHEuGh4xIgRI7aBrbmBwYCOsm1A4wXA0eoB4foR/se/Y/PhP7H58Hds/vwN4dNH" +
            "s+0nR9lJwNQw9jODKS8HlMMON4F3mgGO6ozeEV38/g9SroIgs7uoUYGC9r/j/D5IFTTI1Xl3oFyg4wr/TUFSV426ZxrcuipLZb3c" +
            "qZWgJOSWvNfaoakauXCzwuWo7Pc18FuxSzP3IFyNj7RycmuDfsJ/n/BkdwuLQjOAKFwBr5+gdYDQjycc9kG8AftPUMs7YBlBOC4A" +
            "J+57tiVgJAMjRowYYSObAuOxNnbN5CgEosDsArR+Qvj0Hv7jOwRPf2Dz8R/YfPg/2Hz4LwSf/w3evEBvXkCIAIrjqR1uLY1P/DU3" +
            "T9iKqm1D9hYtcO8Rv6d4MmQarYveEE8pjiqZYfAyWgJjnQDWtmFq80bV97coU5uLWshci7xWJbdld6DjCyElMhh/2VUWrnQJsk2B" +
            "pcC1OdoxEzG4urbL7JiGIwKH5nmdrAenrdqwPXYyazGn3wGGDtbgKES4eoJ4/gTiAMQbCN4A0RpgQwB4dhb3RwGGOoG3bsSIESNO" +
            "H6n7T2IJiAKzt//6GcHzB/iffsPm49+x+fgbNh/+gc3H3xA+vgchMlZZaGMJ4OI8m0sl90OjBWAHC/kg4/6+tOdUuO4hKfbV9O/L" +
            "QlCOpsEqsEO7NibamwAUQnIh3x3yWukO9FZQRarqkKv7sgJ+P+gScRcz2Zuy/xlsze1JFKf+7Wms8dKLbbkGcWT88KKN2Z/68T3W" +
            "BOgwRBSG0IEP7a8hZ5fpH7xZ7JpGyGzVI0aMGDEiAUfG7Uf7a+hgBb1+gt48Qq+fED1/xObzP+F//heCz/9C+PAe+vUTEKxAHMIM" +
            "yhUO0q0ShqXNPomJ67g4gNS4D4nnKC3X0RLQPs4KAtMUr0aq9FYNwd4c0o5SU/hKL5/0ZqGbbZX6UB+gr698nOZWi0Bixti/nawW" +
            "vVM6CBtoYW+sbTquzYFpAyq1T9ocOYswx2RAm++aoNfP8B8JOtgg8teIfB96/Ypo9QTn6gc4OgQ5njlcjIRZTDxixIgRI1IkxnCO" +
            "AkTrJ4SvDwhfP0M/f0D08hH65QPC5w8Int4jeHyP4Ok9otdH8OoJCNeg+BAwQmxByEVel2qDU3Vd0KaJfJ+C89ASc105ekZVaXHp" +
            "ahHYl/vTYPHVZLCDJaAVSm4yNZaM+lD1awLeLJpegILLF6UCXUPLN7pn74GfUsUYYQmX9SbLihBvTkkxdIY7mVdyt2zk39kGa0GJ" +
            "l8UrazgCNCNaP0GHG4QvD4hWz4g2K+jNE6L1A5g1SHlQ82uIyRIklCEctO8ZY8SIESPeFogSEvCM4OmDOfX34V8IH/+F6OHfiJ7+" +
            "SMlB+PrZWFxDHwgDUIW006Q87I26ofsQw3lVGl2n10r5ok9EW6Kt+LHOwLJN5Cmic04Hb5s9em1URrtdXijejS0BEwDh7pk6Ngit" +
            "zXOp51PR1y2Np1OibQPvjBPO2pcFq+62+g5WPxZfxWE5gg4iINgYLVYQn04ZbswCNWcGMT2HOruDmMxB0jOnCpOI1xuMRGDEiBEj" +
            "EiSWgODpAzZ//oboc/YXPn9AtHlGtH5BtHkB63gsZv56p8WdCr5lUcAgyOLttPSxSrteO3fXP/am0JjxZiKQuxsV3IEki5Ork+0Z" +
            "svyvC6j2pUeFQ0c9bOvdoSpnUHGvyUfqpJTMLWu3F7Vvtga0sezmn6uutGLYjFPmnEgBIrAOEfmvCF4kmCPI5e8Q8yuI+QVYCMjJ" +
            "GdTkDHKiwEIi4wEn02AjRowYcWBkA7YO43VWD++w+fhf0E/voJ8+QL8+QG9eoAMfHEWpMMnIf/ZM/QgYcMxvK8iUkizMYU1Bh8Iu" +
            "QlfLZ0uOHk3uNW8KzYpKG/k1AaRjMYMypfqbqIP61i5nn8pXJTeh5KL9G5M/YWFLDlt4HzWntjWawgMlP5VyXo7Wzh3e8kG0GPkI" +
            "uaEyy1vGNbProgaiFCJuBwKDdYRo82o+gzXE/HfQ9BxitgSkhLMMQEJCeHMQk1kj8OUcWz1ixIgRrcG5C/ONQx/h6wOCx3fYfPg7" +
            "+PUTePUn+PUR7L9CRyG0DvPPb0mlOtyxBt09pbtNPV6LQ2wcn03Iqc6rA2nJWQ/iZ3fzjDqygNQzeU7/Lau8S2sCrmB2BzL7A0kI" +
            "KyRVPXFg7C0B/CgAACAASURBVFUDH8tUOTZoac7NpfHLbrIGZIrh+tzm7tQE61rN7erGcnvapvI+eDsfggC0jLJQP9V2pYZ4rE5E" +
            "tYYHTodRjkJEOkLkr4HXR4jZOWi2hJgtAMcFhIJwF1DzyBwiduwXccSIESOOgGrXZ44tAQ/wY0sANs+A/2Q+ww0YDGad7ny+zb2k" +
            "WYX3hY29VfLAliIetgZa+vQUnyq6ETXIWt1EiiP0AS5ct0y+6Lpc3Myk2RLAzMC2HUmqxeF9CerleOtro0se+jZnyf2sd7/o8GCn" +
            "HmsH/sIGro44mOm2U1MmRMD4pzKbbeqi1ROCh/cQ7sRkPNKAJoAkpLeAcKeQztSsE0he6re1o++IESNG7AAGWJsxkyNw6MdbhL4g" +
            "2jyBghUo3ABRGIdj+8m6GA0ah9JjjLMHSLOLcL3XjLTMwCEm9NYFPRIpqPZb6vKgpYo0KLgDbXF2LmWAc98qH91Lw1U3QLt+0rbx" +
            "uFDKcjoHIQDFRPcVP7C7/1enFyh/1TXVw/lmblePlGjXlrZKtPo2ndabF4SP7wHW0P4GOgihwwhaR3AWt1DzS2B+BSkVCBQfYU9t" +
            "cjhixIgRbxbpcMowBECHYB2CIx8cbcCh+UPkA9ocAJYIHlzh4lMamhu8ZXceXVtOcEcbw4m3ikQnM7+0lIF24guND56ActVyh6u8" +
            "VftYvStXxTkBDQdoVHSWwxOAYspU+qXds9VV0qp5d3LNbvukHa67WaxzdvJOl+hVwlYcolo704V+HE7D374OSiFL9VpEtksFAWZ9" +
            "AGuzdejqGTrU0JqhGdCBDwCQ7hTSm5v1Aanf0egoNGLEiC8bGRHQ4PhkYB35KRFAuAZHAaBDiwCUUSIE+7AAVEzddYN0lxSo9ks1" +
            "GuTF1pmoTLOlIN7nuQS1QTsQgfp4iv5C9V87IdPL7RHdCYAdpip7PdyBuiW8K7rIvdvDHltcGiDtIYnAPqqisYoPp79vh0PZGMup" +
            "AnnHOvZXCP01gM+I1i9gCDBRRsmUCzlZQsZnCEAqEKtR+h8xYsQXBM59mMtYW83aCP+x5l8Ha+hgDQ7X4GgDiiJAR2DmCiGwwiJw" +
            "aBegHcSPPo/tOrvVko6tlu7dM9QYtG88heeqojmkc0d37EdWaecOVJGFQ4lOzenks7s9TwMJ4cfEoBVf9zYNYALdEkWdlqbyMa68" +
            "bIeaIlbGk/PrGaahSxtMUdlCZoLEC9EJZvvQ9RPCxz8AIpAQICFB0gWDICdzKG8O6c1BUll1duzOOWLEiBEdwQyA43HRXCdqbNYR" +
            "WMfafx1A+6/mL3hF+PwBev0IDtYAR2BEMCNpIvBXW51zPzYqrPq61WK/+qUO2eq03/4+UMxrZ7fmmsBN8VTMuXUTPtlh+s75b3za" +
            "be0OZLPmvCPDsXpYu5ovO7d0b7G0lDs2dvHxYg02OuLsrZr7uFO1QDzyVJW5KRu5+0NaPMoKpnzCNV97JtNsDq0YbOw3CjqE3jwh" +
            "eBTQ4TomAQ6gPEBIOItrEAjSmRirQBrdCfgsjtg7yBJzsjPPueZeJg41zY2lLlpzrypc1XNtZoa696VKO3dqNsQRA4CzmY+ZLTIQ" +
            "+/QzQ0dBrP33ocMN9PoJ0eYJevNkDgRbPUAHK7COQKyRLghmtuaUht7TMGRWP0cN3yri3RVD6C23EoFtEnX7n/PoIcB3CVzKQ3VB" +
            "qXSLCs9Tf0+eE5lqm92fmtHoDlSsM/PbYUtd3QWq81AMW7zukvOkU6TkZw/FtqNuzOsBZsBhk6iOrTGNYqGHLvOeJYlW3STXofLh" +
            "U8GJyJwdsH6CDtYIXz+aO9IFnIn5jAkAz87Nk+P5AV8NEqG/uPuz/b28IRzX9s+m77vea5qYmt6Xut9GIvBlIhHaORXiNaDj7T1D" +
            "H1Hs9qP9V0Srz+lf+PwB0foRHBpLgInDklAY4Lr9mkuZQEHOaRdwB3vBdgwYcR0RyJQIVW/YrhnoqcnvFbhOmiqHKBGCvsXs81wx" +
            "awN3oD7jZKM7UHXeWuZ6b6YwAth87GViONRM0yXzb2oG5NK3k5FPT7kec5WkgSgwO2CEhPDlI8TD7yBnBoYA/DXgr4DNq1kjoByQ" +
            "ciCka65l/CfksUozYk9o2uWhzdOngm7jAtXkvGjXGPEmYAv8mmMXnxUi/xUcbMA69u3XkdH+ByvoYI3IX0GvHxCtH6HXj/A//xPh" +
            "80fozSvAGrZlwfxbJXGdDg7dY5ssAvubGg9JBNqnuU1zTlbYQVFljt1zEttQYQlok0DRgaWAASuuynzd1s+t2txcN/1w7lb3IrR5" +
            "opAbK4PlvHJ9gL6oqJBDDIut09hzZrpE37W62zniUKkNyroXS8yLtVrsrxA9fYQPBb1ZQz9+QPjnv+Avr6FmF5Czc8jpEnJ6DjVd" +
            "muvJEhByFI3eDBp0j2z3qzqBuF1c+3muC/IjcTu0UCxQuzdwxPHBrMFRaP50iODxPfyHPxA8/IHo9bPZ5Sc0bkAcbqCjjSED4QYc" +
            "vMak4BXRy58In95Dr59B2rgSIfEqOpDA36m39eyaQ/boJjmjWl7qloF2Php7RJOQWFHAdKdtQnXmhx5O9mwJSNDFrtN6YXDrZjwk" +
            "2a7JbW1nTtFABCqeGn7StdIvyPmN8XZuhIoytjJ5VuOUFen7wDZtQRWqexY1fi3+XhycebNCiI+INj7Cp88IJwv4kyXUdAlneQPn" +
            "7A7O+T2c83vos1u4RBDuFATvtCwxI2rQTADyIdr2xopwRx/ACbXHaXdEaYRLJv4Rpw9msA5TP3//8T3Wf/xvrN/9bwSffwcHGyBY" +
            "A+EmXhPgQ2vfkAIdANp86mAFvXkBb15iS8DhilAmoTs8O1DY/nFy/b3BytUkb+0JxfGgSA6p4jIhA5xc9pECmvJUiGqPQ1bbqCsW" +
            "Bu+APbRpW8Gzqm7Lz9WIRCV21kV02kXrViYCW7E1LBeuh+llZH1+TUQA2HOZK5vHtFt6qJi/gvZ9MH8Gk4JQLoTjQSgP7vk9vNuf" +
            "4a2eoMMAiAmAnF922Ox3xPFQeF+54k75okOc23/uEagnqOeQVPcAZ9GRHfdIBk4ZzAyOQqPd91cIHt9j/cd/4uXv/y827/8P4K/A" +
            "/sp8xgI/cwjmCCCOtbeWNwJn9jE+BhF8QwSgE/ZUrvxTR5AmilriKisAAfn1e+3yevQ2a4FcHmVCAqYAAgDC6J1E3QMngFJTFKTT" +
            "5qaqYIanUMAjvAd9bRcnRwA6ZOhQ+W/drbZYBNj+zjATIDTAkVkzwGz2yY4nTfZX4MDPHZgz4nTBHAFRGAs6IThYgYO1+YwCMIRl" +
            "r67puyVLQfwt/b0ywHZUacmSb432Ylj9NZ/HyvgrCU5enZMuXmRt7rEGSMTrXhRIOhDOFMKdQjjTeMesUxjYRzTB9GwGxe8BQh8I" +
            "NuYzMhp/M5Yl/v6xu0/qMFneCtT4zhXM3RUDf6lLdrBQHaJnER1ZODlI0nuao5om+uLYZN9qGtqqvDLaomksbGjmfDGGq6tcklHR" +
            "EqCzt6dNPR4KiZJnW6ImX3FLcvV69yxkAaWAGVXcX3mTyfpwL/su5ahcMlKvpGvGEEXuUZhDEoFGeclGjYtW8jYmE53RdmlAh6Yt" +
            "KAL7a2h/DZ1oz8INEEVp/x9xmmAA0NqcehqswOEK0etn6NdPiF4/gf1XsJAACYBk2i+qZGa2fqy7Lj7TCMpfkK1dj6/zQWzGUNPr" +
            "uC55tm6ylcnk5AxLTcdhumiUpANyYqHfnULNLqFml4YACAUqCoMjTgL2FEKItfqsQToERT4o3IAjHxT54Cg0Cg9EhgikWj57yW8u" +
            "wjgNzg+82yznVQJh5aPFPbnao5MFIA1sq6sHQqeotqXfxO7bhEVdRae3es/VbR6Oh5ayESDbM6nymTZIhrJKC0P7+EzwYSWWYpIZ" +
            "CXAASFQeGFzlCnJwIoDYOlN459M5yMoYcWwWrNSGVlgDakJlDbCnySQ3Se9/wtqpveo0y1VZb5PQrkXeoTBVyQ7ely0BPp9eYQSo" +
            "IgBFSwAQa6rYCPccAVobE7kfWwI2K+iNsQRwoj0bcZJIxVzW8cmnL9CbZ0RP7xA+/Avhw7+hVw9gaQRaCJVSQLbl5SS2RMaPb+Q/" +
            "Yd+Mn2jq7ZQOqpR+zX5LvwOWtJK/X1vgQraz/MelS7S91gxKtiUkMpph1gFIeRCTJYR3BjE5A0chSDgQ3hJSMZhoJAInBpuUpvM5" +
            "GBSTAIQ+EG5AoZ+2tSEBdp9ASgRK42RR47q16asJgA2quPr/2XuzJtdxLE3wOyAl+Xb9LhEZkZ2VOWU9Y9Zv8///x9i8TI/ZVHV1" +
            "V2ZGxN3dXRtJnHkAQIIgAC4iJcovvwhdl0gQAIGDg7MB6IvOTwaNRhf2CvSa5AcKBAGBMqIjeNNF8zQiITlJyPYCRHr8Il3gGPNK" +
            "RXe8EsZdE3BGuAoJ0FQSopjDuHqN6PNeQ/tggrYbVant/E4ejhRJZ6wTzKzO9GMlGKmdNvRuGjIHpFy8AFcAtUtKprw5hyfkz5+Q" +
            "ffkHjh//HcXLZyBJwUliKQHkKAGsDB5lftpn1FACXDM8V/8GZjoj+NsKQCXzE0p69CkC7nta/zarwvo9KiU3pARALwzlIoPY3CG5" +
            "ew9x9wHJ/QFifYfk9h1SWXhaesFcoXqXAS6UN0AW2ohR6I+sDCAGRgFgDw33UQTOwCR7eQAmq8UYmNFk3QcNRcAyDnteh/yX54Ep" +
            "KpYETwwup5XqT1sFztDXpj87C2zaGlSGCA2BXWAjjzMTeGtx/gST17JXpwSe7ZN+zvC8i6JATwNpS5ieBltRs+BqE4axprncq5nf" +
            "pbXeBYArAEu9QFKFc+Uv33H89hmHj78j+/47IBIgEUCSWF4AfSIw+zK0aMQj9LfUxrGaGyVAfa8Fitrf3WuO5d21WNXqbicohX/7" +
            "e6UEEKAV3ALMOZKbN0hzRsoJKNlAHvc6fGTxgM0WJQv00AsZc6xtna9b6lVYsEcmcVlbJ0VghhNJlEXPiX/X286v9g8QCAKPDPIC" +
            "BPJV06blEnAfrrFAajKwDhitl0p3WdNjNZrR0l0TkNSCvhvcuyrdd31sdPUDteYTqvCArqqNwzkpAOGb7bU8oT36FTTNs5dEjyYz" +
            "k5drLK2spOTn8+Vob95kbSFrbGIQVFbnNJH8wNByMOutEjk/Qh62yLdKCdh//B3HL/8AEgIEAUKUfVx6AswPF/45w7ngS+AkrM2P" +
            "1KBbZ7as6QNsmWdr8r79j08RKK/XTHelDwz6gCmwRHK/ByMBxBq0vleHTBVZ66R9rulrQQA2f6KK1mr3CLB3/yFNr2ZBcMkz+ygC" +
            "wFlZ3+CionXtQq1TvmSfdWYDR5bVZycL/25iRxFoCNLkS0sVvfWoz2gehZLxNsvvJSKboeZJXFMCCu85ATNz5wx4bDStCTqjkfa6" +
            "7lXmJI8tU+BgDB3hQWs9NxXWhsTCVur6BMqa61j2VG8piyIwI0gGFxlkdoA8bpUn4Otn7D/+hsPHv6v1WdY6rapvPbYaqpOPEZxr" +
            "wntAbveCvF9raIgEhhYBgOveLa9jonkIgv5pv5+lBBiBEEB6PAJmcfDto/IE5JneQaj1lWrXFi54AdiKgAnItj8Aylg3AgCudoG1" +
            "FwZ3VQTOiFGKHFz36V76rE05xcBsKALW9cjLqcfiFfKxWN/v4QiXP6Sp7Geudk1ACN0bpMdgOQszGVrIWCNlEQ7PgX6tHNDJlRkZ" +
            "zDpGXF9bdgW9ApQWLgZDqnUBXACc612f1CmqLPWCyJrSWFcByPzQTM9MalRe1iKzNUOV2VEL53DyCrxKPbH1ADfSeIz83u813xhg" +
            "3oEJRFy1gWknLsBSf8qtJIHGVpELZoZK8mL9W7Gx5rqXGjz672AsU15nXL6ZzjG5tWgCc0Ifyb+l7pUSkOlwoFcSUhm0QgVTVVfI" +
            "+2Ng4b0fjBU40kDo+F4X8KJ63/Bc5fvH1UgzxQlZuI9KKBmHJZc75zU2VlkwY7DVXwxwoYRZsx5LABAMEihlJSZLuLf71wj2jimq" +
            "Ll7BT38xmmzzFMB2p7N7qwI7l12fuYfJWMZf6zmupxMMJlbtxpYCYD6k1k6Qm/mCWcAop6wJvFQASqXPUQQIUStHF5bnpYKZKALB" +
            "unVKOC0u3zwXVABOeflQxEhlpTkNXRSBDmXUw4Hk6zOddPcKnLXAlgx83TCiAhArxlPaufhkzOJ4TkWgqkvHxorc5dhNX+JGhIUd" +
            "Y6G/MJWCv+0JWHANqMzkDNaeAO0NgARI7YZSbooj1Kc2b4ToyRHcCaEfzeve21G65VqSJvn5xW92vhFRQ7YrFRhyZ0vjGWFUW+ZK" +
            "7QVQigDr/+qlz0TSW9AEGcHf3v1KU4CjQNrcuJuRryMuSB69i+1Y19dB7eea1DytNSTGpvZ8y8NjdVCL86JRC89FpQTsMK/AoI4N" +
            "RNZnbHJxzxs5L9yRPrICMEPMr2rT1ChKU3asBulYaFKnxhIbc3AVI00gdWiS+Qh9giq5Z37Pr3V/TLBW2lgvCjahLPrUYNa72xBX" +
            "wr/xBMDfk50EfaBxiGo4kx73ouCAUttMZx/B5FO+7WFRKtXEAJTwD3Pisg6jYikBQaCyAau8FswFFW9Tu2CtgHQNpBs1+UqpPlyA" +
            "yOxYKOvGEPTjbNEIsWsij5a6XstrnILx3vEMrXVB701jzvAMmPDuQBekJNLGzjaMHf3QOZ+JjfXxQi6BaYXIuYmo57Q/eMsigCgB" +
            "iQQQCQgJiBIACYgEBBIwEjAlWL35GavHn7F6/Anp4wckt28g1jfq2WCpC84L1v9XlmsppVoQnO1RHHfI9zsU2QFSZlAeASih11j3" +
            "A8QS8w7Y90JKhO1oioVL9BK2eqRVdbAZvu9pq3amvszqfIxsD3nYQh53kMc9iuMeSX4AJSlEkipFoJnL8LouOB1EICEUb0tWSDb3" +
            "SO/fYfX4i+7LLVj/RXEEF0ewzACZKSVZSr1muG25Jvz2tLEVgUtM1dEy5yQ7DMQrHZhTmeTsfN2eb4s+CuwOZFtiYkVO9ErUN+fY" +
            "BDIRzlKUr1unLPj8I+88JXYr5Rx1cQejf2DqSTJJtXVff0j9hlgpy5lYYa0VgNWbn7B6o5WA1cZRAq58Qrhq2AqAtv4XuT4f4IAi" +
            "26M47FActpBmm0tIQFRmy9ouFv4SwnINRbij81CQ/kOzS9uzMWGrkbZt9HE5L6h2kerwsHwPPm7Bxy1ktlNKVX5EAoBJqN2V2uq5" +
            "4MzQXgCRgoTUB7+9Q/r2FxT5AXL/DLl/Bg7Pqm+zHZDtwBnUWRECgCy6d+a5FAEMfHYovOPIvjkPvm9Xs7VGHft0Pm/XH13qPYRP" +
            "DXXoxoOAYqajM4QZdCuh/nonhXGd8Oz58HrCO+akAJwLnWiMCKBECf7pBiJZg5INSKi/YrUBpeqzevxT6QlYvfkJ6Z3yBECI9nIW" +
            "nAVs+SyVEpCBc7M1qPYEHLYosj2kzMEs1XaIpBWB0vodK8NDWzEr/0BBp4uHdlKU8Z9ShVDp7VXlcYfiuEOR7ZFkBxARKElaYkAW" +
            "XAKk+RuEBCUriPU9kvv3SB9/gSwyFLsn5Otb8G4DeViD9gkY6kwNAIDUoUF9ePs5FIFTnx0DtfIvXZmes++8puqLYjIpj5r2FqUE" +
            "3ELtDiSYYe1IMW1tIrC9v9zXI9Dd6mtSt/k9FiyIMdTR2Cybs4UFxPoWyc0jxM0jkvU9xPoOYnUPsbqtKwEPH7B69ytWb39F+qA8" +
            "AbSKhQMtGA/s+ari/cFS/ZW5CmcoMiXgHLbI90roz58/Ifv+d2Tf/o7s2+/IX75CHrZqbQBBKwJVEb1Y8RRz/1h5Rl7E9VqEIx60" +
            "JyDbQ+5fUOy+I3/5guzpIyhdgzf3QHEPkrnyqun1AVS5E3QBerKj6nTisvEXTAciECVgASQ3D1i9+Qky20OsNij2Tyj2LygOT8if" +
            "PyN//oj8+ZM6Ofu4A447tYVu3wn78jLxWTCn13wtJsvX8h4NJMHdgWpHzcSDiqZqHa79sQqLJvc800znc4+Hn5nLcALOSYaxGLPr" +
            "gae9WgSQeACcRT0dG6drO1aHpjIoTSHWb5C++QWrx1+R3r1DevsWye07JDcPoHStLGjpGsnNGyT371Sau3dINncQqxu9hmDBdLAZ" +
            "lDYlMKpTgGVexaxnL5DHFxSHZ2Qv35E9f8Px+Tvyl8/IXz6ieP5D/f3+EcX+SYUE2azXIsoQjVLwx0jj15kKxoQv2DHsuVBzEzGD" +
            "8xzyuAeJZxQvX5A//Y7j13cAGHz3CM4fgeJRLZYXao0NkdBhRVrQF0J53SgBCwGQ0Avrha8WC05ARTuk2lgAIFJKwOMvIEqQ3r1V" +
            "6zv0Wpns2z9x/PKfOK7WyARB7r5DsoTMD2pReEw26YAutNw761MydZ8d6rHD/PTY7tU5r7jdtV7q3JVm3aZq5nMpHpUSsNKeALsK" +
            "BNROoAkhGHQ6Jvo3tVsd2+LfntulR5BvarxcDa4LLe0VodcYKZPvToOYmq3WuR1ZC3yUINk8YPXmF2x++les3/6C9OFnrN78jPTu" +
            "nd4NKNWKwEYtBF7dQqxvVOhQulrCgc6BSnPT1n/tASgycH6EzI+QhycUuy8odl+Qb7/g8PWj/nxC/vIFxf475OEb5P4beP+sPkXe" +
            "YL0l1VGfSav5O6bkBh8s3zJ6exR4x1/plaZKsJG6nbM9JAj59ivE0x+gzT0ACc4+AMUBxAWE3j1LaGUAQgmhpHemIbECJymAtLRO" +
            "k9mXdcGoME4YtaRXKWXJzQNIJEg291hlB+U5y4+QxRGHT+8h1muAVPhXzgyZHYDd88l1GbqkYBT4MvVV6IRBN5dIuH5VmLe9nQZ0" +
            "xClBNV11xd6ZWhl41gRoC4gerTSk5n3Rmv/plDxv0lpw1RgpDlNtHQmAEojNA9KHn7H+8FdsPvwV67d/xvrdn7F685O2ahqBRlkv" +
            "y4/x4c2B+79aKGFfndos9XepD2/LIbN9+Sm2X5C//I78+Q/kz79j//E37D/9E/tPvyF7+QrkO3C2A+c7oMhAxREkswYJncqGYxPR" +
            "HCjFNXmUygA10xCRknCKDMwMKSWK7VdkT3+A07WyEOd7kDwCXCBZbUDJGiJZqx2DjBIgCEjUuhuSayDZqN8gIJlTUMVrgw690s2b" +
            "bB4g1ndI7z/oMLoC0Fu9JusbMOcoDi/Id9+V52f3BJAodXAKEc+C2SgC3dAv8HsMDHck9Xtidl1g2QibuwOVN22dh5y/1a/zLe6M" +
            "NKNt4h+7QvbigbP2pC6wg1G7cwhLI/8fGCe0a7fMezKJmg9OCSlCpHpRcP0sAOjt9VALW5gdm3m14CIHH1WIjzy+gHX4gsz2aqHv" +
            "QS9SPexR7L+j2H1DsfuKfPcV2dMXZE9fIbdfgOMLuDgCxQGQud7xhK3wn/Hiemw25ubThVqn5BZxJ3N19JdSvoxLXgKSABQAMsjd" +
            "E4rkD9V++xfI7x+R371FevsISld6LKVlSJDaop4gNg9IbvXaG/P35hHi5g0oWU/41gtqIFvGEADpE7NFqgwehv+R3vZVH5gYnNpO" +
            "ZYe1PCbiraHY5Nb6DC/mtWC8dxqa03DVYQxeOkY+Amp5feOcAAFtVATAZmWa5Zt2C+/iYg5Wttdb9Gh0dn/08KGPVIVRENsOxEI3" +
            "ReDM6ODq7ML/Jmnujs1xNkXAifkmghJUkrS0YJpJEIk+DIwI5WE7tQl0weSQOeT+O/KXTyieP6LYfYXcfUex+4pi94xsv0W+03v/" +
            "H7eQh636e9yqrUAPO7UAOD8ArNYPQObaCipBzKjt7QkM71rruRgtT8Uh2saQ77WqkBHz25WWlCeAIdVOMcxqLYUOFSmevyA34XHr" +
            "WwizHkAkICEgdFgRESG9f4/0zZ+QPv6C9M0vSB9/ASBAqzssy2ouAL14m4US9NV5KUoREGKlvJ+UwBAIucKIwSiKgCeDsVlsn4HX" +
            "uhgo/FyrKXAkEely6BToGHnWYxnx3m4nrLbZuKve14ZT5BNTB4HgOQF2ssrOOFTY9z47SBBr6awgzKQ6Amc4lyLQs3e79M8J2feL" +
            "TQvN7IFCQ7cuqQDYdejyiL+uHYiFmykJVIX8JCtQstYfrQgAsHc1qX4vmBoEqJ1+9k8onn5D9uV/In/6Hfn3P9Tf5y/Idlsct1tk" +
            "ux2K4wEyz9UzeQ5CocKHyr9SK/v6L8uGkWUMBeASGKyaUvWnxvtLg5T2b3KhvAQkwfsn8HGPYvsNJFIlKFqLgYlIhwGpj9BDZ/X4" +
            "K9Y//Q3rD38DZweABMTqHrj/6eT3X9ARxshYetyFJgEu+aBSANZ1T4BWBksB1mG33HOurssxgQfnZlbvaWty9aQh+cwB3ava9cUC" +
            "6ewok9azTCo0nEgeIbmDnbRzOacoFP5zAvTKfZJ6S7XOm0OP5ewYAPaVHK6Lr8tLRtIl8QzRp/Unc0vF2mmAItAfkVwGFjCZR8Bh" +
            "CNX5l0ZYqWL/zenBxvpfU9EdSXFZDzAtWBbKqr/9ivz7b8i+/QPZ138i//ZPZE8fke32yPY7ZLs9ijxXawUKtWUoCUAIKp04tfAc" +
            "s7gYfIIEfaHnzgLbQ8Ja/tPhU1KCkcG8ALMRI6HHEmAWFgt9TRDAeabGlUhBqxuIm7eQD79UW1DOuj1eIcygIAGAASGsNVBq4bZS" +
            "AlAOHmJrvFh91sq3A/079LmLIVqf+s1Y0jmtH2ibb8fuAp4kV09uZ5GB+sG7MFjp2NTPsh+9MRwNbbUlDYeTeVVhtuvsMJBrxBSE" +
            "VMqYbRl3abSOFRzW/uMrAAatnpYod/XcY+tZK0lVhjJVkhDl4l9jxazu93yJBSdCH/vFUu1ectii2H1Hsf2uDjjaPSPfbVFkR33y" +
            "bwEiCSIGCy4nWSKud11JH1wnAhpItjNRAKzX8Jc1yP1XZ9q1MjymTYo8I5kg80wd1rZ/hth+R3LYYpUf1KFubGzCy1g7DxyiIGgv" +
            "jkB9i1ezTSwa8zWbfzyKgMuntVPJy0rttSjerh9JXmzLxh0i5ylEqQAAIABJREFUHWX96M0YT7mYImCFPnfleX3Yh9eQb7MF617H" +
            "KOwWTCWJ+fM8pbSmEiCgt1BDlBrKAeRWYQz/hIsR87J3FAgSkUcpuAR8RXdpiq4E0fvVAg90yafVW8B969ODKJykbeUEhf2oVFN/" +
            "sOwDz3ioP24mGmvlRmm1FNaHUN+5q3qy+rNIKtOBLemxgMwPamHw7ps6qGr7hHz7jHz/AlkUepvPAkIrAADqFkudZZM+uOziGj1M" +
            "jT5l9BzwQTYazcc/2Gwxvt6URmSrhH8zokiHgtpCgJkHiiKDOO5B+2eI3TcUhxcU2UF7Auw5bRlb08EV/qmaD8xZDkKUBhGtApRP" +
            "khHiLGG+DAXyCbWu8hD0CLTMMSPJJaFppWYQsq4FxbJofVqEGeuWy6Z8WY87GvorALGcXINAQ5xzhMAy0IWd8k/uX0WArW1F3q81" +
            "+Gghko3nOftC/aqlBKz0z3LHEc36qqwbwiXBMaW794L17Yy2bNj52xmO6dWnJV4Soa7uI+Bf+BVqaK3PVBXuqQAEq8LO9w781LY0" +
            "taVtMlnSE6AKAaKGJ8DNdxFSzgO2PAEvlifgO7LtE/Ld1vgLAHC1ZCNBNcnYpiYPfVRHNp5pDI+tAJyMbm+tmo7LH5W1TysCZKdE" +
            "LV2ZXnsCsH8Gbb9jfXiBzI+lJ4CI2ob7glGhBSfdeWSMIY4nAEBlwTXCvyXQtY4dt1Nn1snc+GUpAj7FpnOugQedW+drjvEUgDKf" +
            "iEGR2RbKnXYdsxIl6orA0OyHzgWN5wT0rmrVVa0E3EBtFlRtH0pUTVWXHBtzEmQV5lejEPxK22XqchGMxlUC1zoMDHvwu8lDvztp" +
            "9DOasF4/WMWcywLgAvK4U/uW75+Rbb8hPzyX1mPWfNRYodlYFyyjsrv3fWOMovbIvNBVOnBebCyhojMLczeBcK185rLMUWR7YP8E" +
            "2n5Bvv2M/PkT8uePakHqaq3OEVhtRqj9gv5QxhDSoZGkdw8iLejV9DxCjdDIKAUh4puZIuDYJTulnaQCHeozM50piFDdyy+Xfomz" +
            "HMQVRwp8AHAwPwAYRUBbHVl9tzWa2UxOZ6/IGQs8VXW0sgkJGa8KUwr8sbSuFTeQkJy7PgXAZFdObvbOP5bxixpPLpgM2hTCslBx" +
            "/sURfNyiODwj3z8h231Dvn9Bke/BnJuHnL9oChuo32IrzWz4awgDFYEh8LFBL/3rsup7WPic6PWnpcyBbAccBGi7Qv7yGfnLR+RP" +
            "f4CSFZLbN0iIgNW68eyC8UBE9TUYFjdUXtH6YYgl2NL3tIXc9r7WFAE0H/cqArWKnfZe3jJaknZN6Et7cjy/RxGIJQWuYVT436Km" +
            "DFyU6U4rVbfpubU1AZyIsiblMDSDC5dXAcqptW5cGzXvU1JMghEnUht9swy613rmMwkuoQDYz1A7IyQ7sede/Y6rMtSrx8EUCyaD" +
            "LIDiqNYCZFsdCvSEbPsdxeEZ0sSRU+kD0O5lrnewu/+/RkznH8keMC56KgKnUuqpbJDtf8jkCL2+Yw/eS4CA/EV5AfKnPyDWN2oX" +
            "oXQN4AHzMB2+XpARNhqCODU/oLjV37lnknsR69ZTu7zJtCfFKAt7e0r3VzkqPHRyWVxOtq4pAdUGaua3vnreOtXQaBqjAAxor9gj" +
            "8Xec1fQ7CsYiuT75jEfmM+uPFkYSoy1y/qqsqljy8MOz4FyvG1wxG2apTgrOD+pU4OMOuQ4Jksc9OD+CuTAPlopAA6W00g1Nq3c3" +
            "2j8LdZzRI3ByNux8N14XWQC670gQiu03FM+fkT39DrG5g0jX4JsH/8MLJgDVJfqSD9oKgINQl9jX24S+KRWBsfNZMAweB633/oSI" +
            "87DLKALCe9XWtlvhc7leBmaL7cb1k3M+75sFPNqjopHtQCG2L8j6DCvoXA3SHdT2fOCe73XrbWPveLLgrHC2rGOWkLKALAoURY4i" +
            "z1BkGYqj+iulhGRW4r/hQ6Fuixw64/P+1LMamRedibX1o+BWDbhfAb57tkAgWa35KHLwcYdi+xXF948onj6h2D0pLw/D+izjcZbQ" +
            "3RIaXieRescub527R9FiW1JGk/aowJB37oV6AaOwIitKpPObjtpPPlDt+6XlZBf1cCAw19SCzopJc4q6BJssq2tbekbNfdwcY6XE" +
            "ihmjJt6u9UUqdCikn6rY8pynYuz5NhpOFf7dvDpak0LJKiWpUgCoVapccBJie1SbNQHMYCkhixwyzyAzowQcwXkOcKGESa5yKr3q" +
            "XuI3s1WTEtrHd8iU6fMcBHKa20zUgL+CvfgTtyRhaKuzUgKQF5CHLeT2G/Kn35HcvEFy/wEyO2rBv9pxqIxfn8vpSq8JbocZK4u9" +
            "Gti2lsS6wMd3nWudPdkcTtuLCnqw8Xq+mqdEC9OzBtu/3Mz6KgI+waAvC+nowYy08ehwPUXm70gyQd2gVxdIa11B3YqcSgKtRP6V" +
            "xeLIHWlXyOgmI6bp2qJvzmPXZLh1vlvenanJm2DmCkCXPH2M2ZdnjUP4hLsFo6GTVVcJicoTkEPmyhMgS09Ars4GYFnPVn+iJfQ4" +
            "ij4O18LkTkc/LqJvz8rLYzwB8rhDsf2G/PsfyJ8+Kk9AftBkwiMdJLQgipCLFEBX1bh1WFn3u4yOmL4x1egKltXD0s/hWz3QfHAK" +
            "BaBtfpwzXJ3Ux30JaL4c1dN0LWtMeE4MDpQ6wCMwBr/0WavrmrxPnWveOqUukWF1cYzVzueGt8e0wcHdW18xvtpxWh0yG1qJ9iw7" +
            "GGKCaUOCmtkRg8r/NDMJxbgtGAbdnmy+syz/qlAerv7q+A+WDHl4hjy8QB63kPkOXGRgmcN4aohRhSvPvbvGmkXmPFv3iQXQvIWY" +
            "wXkGedhB7p6VMvD8Gdn3j0je/AZzcrDau157AbTBTO1jnwIiBemzPUACJK7UiHZJtLD5Xmi1uKCadzpmYaedomcnpZaYt/r82cwL" +
            "J/Bttz28zqyZQikBNwAKgJOkoTgaoWxWL+FVBALoEaLh3mpe47omPmWj9JDuhyoCUzOz1sID5doedvuwCgq9qbG2xl4iRsOR59ra" +
            "pXu7UXjXBq4UAGiB0nZLqt8dTh5cEASX/7Il/BdqYags1KJeKcGsLPos6x95fEFx3GpFYAcuDiCWEAJgQVpZQ9Vn6DG2HLIePhc1" +
            "GKNzD+N5HuZMjB5bUOCW4gvGUMQMznPI7IBiv0Xx8h350yeIr/8Abe5BQm2bLYj0d3VwFQkBSjcQq1vQ6ladK5CkSimg1bzb6kpg" +
            "rKhRlcoW5kPWl4iDzH0kJjqc1KWejHvn18M61eBDp1q2ItmEn7+AZcQvxDV/jzQ+nWl7qmJOh9Md9d2BiqKLWH1W9CGfYLq2HmjR" +
            "IdTfSozwOiFGQYAzdPBBn6IIjIIeGbkac2jDB7ARnLl2zZeht3iupwki4hpmz7c+ygR5btR+sZ7WdLgBlU+QtjDPZSReL9j+Yqz+" +
            "LIEiB8us/Msy158CXKhFwObD2Q4y24PzHWS2A4ojwAUEEdg+wdQqr9fYGsFj2a3UMIWPVsSl0PIqodts/mHWIUFHFMkO+fY76OkT" +
            "sHkArzal8C+IIIQAJeokWwgBsXlAsnkDcVNAMAO8AVICkhTzWwp4xbCMRwSUR6iURxpZaWqag/2VHFoIdE9IjwAiY7xtOLH1t+MG" +
            "AX2cWjFSi9723hyTbn15TTi3demHiYdlXfyYwtzaVcDxwNKmnXMCkquWOKKCcKjTnYk7/PDUOJ04LqRvD1YAKv5MlaWftCjMlrxf" +
            "CsiBsvjE9w4pEWVxYY7RyxPgXmJCGU6gBX71S8BcJaC8vuBUKCGPi6MK5ymO4PxQ/ZWZvp6r2P+igMwLyDwHFwed9oBi/w3yuAVk" +
            "BtKLFdlSTi82DgehZ20HEuKs6ddsrs6s+j07QNIW+eo78P0PcLJCAUYiCEIoBUApAYkKA0oSJLfvwPcHJFKCmSCYVRpez/zlZwiX" +
            "JEtpnwCI6jsJlHF4Hgs/6bnE/m3ul1ML1R6pISa2DerSkAWxVv3mZKaao+MYnZXJuQ0TccquitjZ4Poi59VBSgnYA1iFktgOjvNP" +
            "bVOVWHYF+Y3Lhj59/GgKzIsszgGtihKBKAFEAlAKiES1hRF82cf+uMyhvgtCIA6ghYgo9Jz34WZmcc8AOakU5dU9BEoAIalCgUS6" +
            "ASVrkEhBJEBC+N0lCzrBrCbh/IDs5Qvy50/IX76AD8+Q2Q6cbcHZXikIMteKQqG2/TSegCIDZAaWGYrtV2Sf/xPF9qs6RIwlIitW" +
            "Wmvn/hrK8+Y3vVRgiho+x0GvxnNbS60FYZlDFnsUGYF3KTgRKGQGcXhSHgAiCIIKATLKgBBIHn7G6s2vSB9fkL7ZI7l/D0CNZYhk" +
            "3Pf8kVAK+wkgVkCyBtIbYHUHFFJtbWJ0AzieZaoUAVcBYCt7r4EmUJVWx2w55wSCQupuSechlHMeIAFU65JK7yWs+ocqHWEEc+YR" +
            "s8fkjXcKgxxeudqaADscyM06cvzN+dEYO7105Vo2DRdilV3t2lSesh9uQJoGL2NqBSBSINkAYq3+grUSIGHHwtctM1oRML78Rue3" +
            "uHhs74J9MZRP8HcFP42ETE115YCkUQIYlGwgkjWEXmRIpH0CiyIwAIZOGLI4In/+hMOn/4H9x/8Buf2qFIH9E/j4UoYBsSzUOgAp" +
            "wYWElAxwrtYNsAoNKl4+Q26/ATIHsdSTtVN0QCg1CmCIkk5VBKoy5oNyKIw9gfiYQyfvrklhtRRrJSA/gJkhIFHII2j/BHr6AwLm" +
            "OE21NsCsCxBESN/+iuL9C9bHA7hQh8bRagO+ezu7vpg93FgdEmCRgEVqKQG3dSVAewNKOqux10oJMFu6Euz73Qw5Lql5jUXmDzsJ" +
            "rDVD5ZbPpbDvppXqOksQK15k8mC9QLM0gA1UBNrwmmh2VCOur/EmyfSUfPpVaPbhQI15o1HDarT3tsa5beUI/bZlweVLY+A1DbT+" +
            "MOYbARIrcLIB0jsgvQOxZoBaCbCfULC0QHb+uqXUNL36fWpYZtC04NT2kK88EL78mvRE9b+1NHWtk6QESYaQDJFsQMkKJBIIEsob" +
            "QEtk8WCYHYFypQTs//h3bP/z/0b+/Q/w7ivk9hv48FQtBC4KfS6A3hnI3u+TGOACVByBIgPJQu8ZH6AFr+BL4Vuxx14Dpnqxmnm3" +
            "WUbDclob99aIljkKlqAiQ5Htgf2TtkKnak7gam4QROVOQevnr5AHowDohcK3j+r8gQX94JjqmYyXeAVOLU+A5FL4L5UAZUqvrus8" +
            "zIgrWXL5j74eqod9X9erKY9w7Tfb4Zvak032HMWoCfoNq6Y2NgAFIAWAHFxIrUBUwr951eAB5CdYrl+rx2CS9xop0/FYY78K1cKB" +
            "Cu0JkOwQ5dB6TAlv/iM048Vm30jHvSKJQL0lVR4AkYJWdxC378uPstAzSIdZuFZTajBNnwBvW17cBqxz8hqDLpPYwr9zz/rdxYJU" +
            "n3m8CWuegOTmEat3/wXpm58gbt9ArG9B6XoJKxiCUkCXysp7fEbx8gnHr39H8f13yN038PYb5P5ZCfNS6t2BuDwdtrK4cXmEijBe" +
            "JE+Mi1fmHzB+X9Gwr+MML8ZuGbE5kUwHM9iEdrEEkINzO0PUFAFZyp8Esb6DuH1EcvcW+d07pIdncH6snR2xoD9IpEhWt0huH5E+" +
            "/AQ+HFXUAqWQx12phJWCvu0w1Re41AeMEkD27aosAFZi62JEf7RhQletuaTmEbBCXCslwPIGmIMJiwM436u/2R7FcQcctigKqXYz" +
            "q70fHNHBIfTXKs13xNn45+B2duWM86PyBGTuLS63Bu39fiO+zTkaptyJRb9t9F0nHVC+lu7XAhcRHHwFhgReUi1MlILFCpSuQTeP" +
            "WL39M1bv/4rV+3/Rlu/KmGOL21Qr0BHefXGYbFfHL8i3egMCHqZWUrAZcwtRkV4TAGYk6zusPvwFq/d/QfLmZ4jbR9D6Vm05CCyn" +
            "lHZGpQCAJSDNAt8d+PgMmW3B2QGyyJTLvTwbQD1NtoCvv1eWxnopte++7nEG5ryE+45c4wSya3C2KRiVa6mNTV6he5UJV/0kz+2a" +
            "gq/SMgqwzCDzHWS2VSFFRYbyHApfZgvCIABMEKsN0ru3WMsMJFKs1g9YP/yE/P1fwPmx6gpLCajlYfWTbe9vdgXVvzWIFeE1Afa6" +
            "NecL6/uw/iqS0TH+XD8RngHIwxPk7huK/TcU22+g5y/ImIDjQXsI6lV258hm/cK3YugyPE+n6KZ7ZV68sQdMN0ZuD7k3NVLgM4B7" +
            "/bPQ7kt7BUDP6l1RD1LJFnwmI881cq9PgUD+XUZbTeC9cFeYwhsWCgIgwJSAkhWQ3EDcPGL17s+4+fV/x82f/5veY5vKMJgqh5BF" +
            "3/EMOF/9FXMv+b0AbvrGa4UQsszEkrNeE5BukNy9Q3r/HuLunfIErG5UuyyCREc4/ckS4Bwojmoh8OFZrQPIzYLgokoLpQCYTWNs" +
            "az+RSw+Gg3CzqwPW6D6k2ZaIPN+Gof58IwTSU3b9uXjlQzaB0Q1GbnWiHoDQb9ZCJVeZGiWwNEaw1Z+kzpeQR8hsB3l80btN5cqT" +
            "1FLcFU2ZZ4KZbzUvvH3EWiRI1w8oHj5AvlfndXCRW/yQ6v1n50X1ceJnoU3674qmzchn3fUYrEwoUC3yglE8f0L29Dvypz+QffsN" +
            "zITicABYrUEy6yDsHfRaaWkij8DJ2VqDdujWCpOi84TfMz/zc6J+6QPtCdgDWCsdANC0GBaEgpigD6eybDejAANeAHL+XsbW3g0z" +
            "rhpgWleZ+NX+2mohMN08In37Z9z8+n/g/l//T4h0XW2/JwQAp7+8tNnVlBBT1QOegrYsERvHXUc4awMzA5QoD0m6ASV6dxGh2mNB" +
            "F7j0oV3vslAhGpnyBPBxC5kd9Om/eueNhiXR4gnk8Aeu/anTiM03uo7JnuO3TlnjzSZ29FpT5fAZTPypuxU27LHR8y3f2VIA9PWa" +
            "okVc50WWJ0DKo+MJyCtPgDEjL0p8D5DaKU0kSDf34Hu1exdkDubc4ddd2jVyYKObB3UfTfVpw280aiQ0YUC2AqCVhOzbP3D89D9x" +
            "/PIAShLI4wH50zcAiSIhY5ulnmQ+V0VgrnCnkTFeMsZCL4Q0cq9pJHdxBoFzrCICIv7rw9wUAT14auOJjVCvPgSCEGbP7ZX+pNoj" +
            "oARfn/DVHKXhn91v+u/FmrWVqhqD3pNbKXMwQEJ5ScRKKwBi8QAMhLHfAijPZVNbvKgPCSPuNS35VS/pXOyJwEMM9V49t1A8En1Q" +
            "Nd+djY2MUNhJjzeazp3xOXirivTQC8qLXCuVuT59mq3nlzHcG6Q8wkysDl4jAoQAZIIh0lTJfv13hlXRfHGUgajcaOjCXX/JDFrd" +
            "qROqN2/UZ3Wnds2jBMoNYK0tgNYrtQ7hnFvYD7UK92uPU2Xk0XhNh4xaTIgjI5zz0DK7Pde9RyJKgFkRV/28diZm5ppXv8/KWDN4" +
            "jzxi01yNjACYWINSBSABokTtiy9StUd+kpY75DQYd6NeQ1428AyH751GNbGn62IXEQGkFSCR6K1UqSWPBTaaPh216qemABBZioGV" +
            "mOyndO/UZou4fbzLnSgqRlXlUJJIVdHRObIt3HZJ1AOtdXXHd+y3uYaW1u1Z1bqe7Ur7KInAF+KvFpMX6rCxPIM0IWZ6+1gCNdac" +
            "zslWM2uQUBvJ6cMVmYVeH9W3Bcn6t3PyOPpUocE72M9PVreg9QPE+hG0eQRW96BkXVMCags2dV3VgvaO9Y7WsYcbxHnUO/+fUJ3e" +
            "FZgV/BU6pZr9nu1GDDFPQJVRzfJ6XkFkbItUbXGQ83lVOLXhBigA5ntoojNW2co6S8YPUCoBwngC0jVEsiq35isfaRR8KnWciXOU" +
            "DROQZkqYBW6kPAK28L94AwbBts6zXnFOQihloNxKsDqR2jX2l/HfHUilmewERcD+StX8XF4flzFOil7Zu4l9DxOa0XsnwD+0HE0j" +
            "4AUAoHaVkupQOWk8AVKWngCGE0a0oBMq5SkBSGiFyvauTFVud3DgR1eFjxxCptU9xPpBewIeK0+AUG1gDhKzNrVSZegwtlDUWXuL" +
            "uUpv6wPeMi6i6A4oaFovwPi5DsuxvSNrSkDCnhUtAwWtsZpgKiL6IdjxGXz6vuwd3lRLV5GTUQK0oKv3w1ex78YKnlo74pzzDSYs" +
            "ovEitZu1O0sIUH9w7XRNc/BXDnABme/1ji0FUAoSSqioCdilFjCMNkZTBM6FayWzczSrN//mmFWegByyyCDzI2R2gMz1JzsofkZC" +
            "hbKUj2rLtFH0F29fE2aFutaEy925ZoRGj7meb1+aSD4i3UCs75HcPiK5fYfk9i2S20eImzdqYTBnAGd6XUQ1paiFwgPOSxoZZzcV" +
            "z4scohha1SlfsaYEFESkdgcy+9b6Bbo2TCm49yGwajjEn7DtChW/MaamH5Ape5gY0PA+RvvZCPt1RYDKv+q7PjFYW3mM5duckFvK" +
            "ZK1dEEvQRo1n6N/OcRDW1UUBGAYuwMURsjioRcAyAxdHcJEhf/kDcv8NnG0BmYG4qE77BRxm59BNX6bWYJwV9xpNdh0jo1dKZsPM" +
            "Vv5Qq7ggp1IyS0iZgfIDZLZDcXxBvvuO/OUrALURgjCbHRhvlIlvFynUYVh26OMr7ZgBICJw70XAF4TrxesKE+OfriA2d0iLt+qc" +
            "gLd/Qv7hV6xfPiFfr9QWoodnyCyv5V+PcaiPgN7j4cQmnrMHYHgxfvPOdOWdB/5wIAm9tVn/pbTnrHw37ZoCw6E+ZNwU1LmELnW4" +
            "EmU1IPz7rjUO1vX+siZV27NOQBmMbYR/OKEvMBNlleswwTj0zFm5Rx3le3hvTliZ1wsjJLAs1M4s2QvkcauVgT1kfkDx8kkrATtt" +
            "UTO7c6itWbuRSottzx7sje6NTdMD8aOTixPB4P/RFWFFwPe7LIYYrA+j40JtE1ocXlDsn5DtvgEkIJIUnCQQSapD0QRICFCSAsla" +
            "2UNIqNAXd/HAAov3z3wmbZlDozCeoWSFZHOrJBeZI3/7J6xf/oxi+xWUANkTQcoM8rgFEZsdQ+tZWf4Atx5RDjYjurNljEtUi61P" +
            "qCZ9PD21B+zfIR52JoTDgYCyRl3t4VO8wFjGrmAeVMmjU9W/tQ5zwAmunvCjDHYWMlbKAIEhwMYbQMozUJ4N0CC4NgrsK+yfia0M" +
            "8AIsOAEswfkB8rhFsf8GmW/B2R4y2yJ/+aiuZVutBBSob9OH+g5AQESy9HDFnoN99jzhmjG4cR1FgFBbe9Ho9VI2LXQ40AGF8QTs" +
            "n5C/fAMgkKQpknQFTlJQorwCSATAa5UN6d1u2MTA/6Be6FbMvE0GegFs0YuSFMnmDiJdgQgo3v2CYvcFxf4bmDMUxRF8eIIEILSw" +
            "H/AnI+YRCNLyDHHu0dDOOgbWxu2SGbS5pQRkKhwIUJU0p52PufoqgBi/Hi0Mqakqh9OMODsPCae6JjTfq7rS2BXI1uz1WQHVLkDa" +
            "Qqa9Ae4iuiU6ZkEI1QTKkNkB2fNnZN//juO3v4OPO+UROO5QbL8g+/yfyL9/AR8O4DwHigIIeAH8zt/etp/RcM185NWLtFKqsLMs" +
            "gTw8If/+O46rO4AJyc0bJNoLkCQpKE0hzOfmAcn9z+VHbYks1OdVN9gCG3bIkzobJwVAEOs7JPfvsX7/F7As1Nkx6xtgdQNa3QDZ" +
            "rvxwkfUuN+77mhfOVbtL8dh+5faVjIOWLTccKNdJKidI0AU6Mk6a4LgljwvT9bVO3A2copG5vwWBEgFK1aRIidkOU4UB9euz+TKu" +
            "BecCAwwU2R7Z00fsfv837H777+DDFvKwV3/3T5Avn1BsP0Pu9+BM7eAS8zuXS4NCCc4Mw+OukeIvpz6F0cm/GGl0c4tZgvJMfSdC" +
            "/u0fQJGj2H6HWN1AiASJEBAigVitIFZriNUa6cNPWP30X7FmAXHzVuUplKeUsIQF/UioFAEBECsH+WqD9P69UgBWG4jNPWijzhEQ" +
            "N3conj9BPn9C8aQPUgNgeGFX2mkqAo2anfZiI2Jq3jeWrOZ6XIAx6z2kluFnPOFAsnzE52aaUqD1CfGtykFbhcjugmudPmeAU8KF" +
            "2LphBHyzE5DeEhQmVra2J77tmrH/tgTWLfjxwIpfyWyP4/c/sPv93/D8P/4vyP0OOOzA+x042wP5Dih2QHFQOweBg54AA7NByVy0" +
            "+euldONovtCxjQ77iNWhTzQiAeo0agDgAlLm4CJHsf0G+vIPtSgYBKEPvkrWGyTrGyTrDVbv/wXQCkD67q+K/0EZSMq6Xm+HL+gJ" +
            "Ir2GkRKABSgFkvv3Shl4eA9x+6iUgNs70M0tso8bZFyg2Om1TrDIhWEfKxJFnMzOT4QxVjtVbcZWALzC/8miy/iTUHN3IABVLNBk" +
            "5QZxUihnI7OWnPqGkS9oBzs96P+qwoGSFCJdqdjHJC33Qa52zzhXpRe8CjDAeYZ89x2Hr//E7vd/A++24MMe2O2APIMQrMmsEvzZ" +
            "1jcDaOVLC622gJzvZ2ayTvGxqWFQV7LUZwUQUBwhj3swfy0t+gRAkRwh2dwi3dwgWd+CswzJwy9Y/fSkrL1msfoi/P+wqBQBAokV" +
            "EvEGYnMPcKFCgJIEWK1A6QooMhS776B0DXMShSIdTeAWHV2TWHPNoY9RnLQmYJoW6XBY2HnR9zUHhfHHhP8zMt4pizuFXLo+20jX" +
            "EPjdnZlQvjSJRMXErjfqk66qnTNKb0Cfmp0rcG3BLGHiNswWsyTqB4IJ0jvSGgmQrOfa6YRNWk/SYAhiS7Y/DnWGFmBdsAWiyzsG" +
            "x1Kg9HNo15ERy2prnLjUGZS8z2bL5A7a6IIfArXtzfWW2YwEYnWD9O6dDv1hFE+fkd3+A0j1YWLMcDc7ADqMNLq03nnd3NC0XZuE" +
            "EvIEXPLtG+FA6iy66nMuhMoaUodoZ8yMx8bjxfxv0qZIthLiAMQE/ubPeCMTqxNbRbJCslojWW8gViu1LsA+JdfHSN3yAAAgAElE" +
            "QVTspT28lgteMcreJgCsPEelF0moj9qKkcACWiEwIWlWCFBk+8HGFYrc86UNJLpuSu0jLrQF3XRpiXD/DC7ac7/+M8aZPcyPqDxW" +
            "hnRomvlBpXeUyuRcKgKkdlDTeRi+116HBa8ddUVA7QNEqxskt2/Vb5Eg+/x3pLdvQOlaKwEFIKlXKFDNHnIR9OeG51BYbDm4S1l+" +
            "Gch5N48n4NJzQSAc6LyINUKXacI16gS1stjb+TS0i6Kbj8Ml0MsrAB0yI4BIQKSpUgDWa4h0rXYIosoT4O4QtGCBQYPhWgI9kSg/" +
            "NQ+A2VCbUApa7HJk6/coLMAzfC/OWk4CW397StnBNF1bZDrvQbimXTmsLeRbCoAvZ2P9l0AV/bN4AhY0QSAwVXQv0hvQrUCyvkW6" +
            "ucXx93/H/uYRZDwBsmm+jY6Yi5PbsPF87moPVzra5bdLI3BOgPClvQj6NVKf1IFuDbhrpka9GLfAipB6CfkcudcHkYd7KwAGpKwb" +
            "EOYUTd+C4AUL+oJgy1MmGMO+7wlSKzHM6tP93gKF7mx1XrwgXhv7rYwptimQMTOkjvuXku0z6xYsaKDkYQS1hSytVKhjuUZgVa6n" +
            "YwgQzCJ16Dk1QFgOMcfH5ASC0KWYaAdbgu/2KC1wIfkyhsCaAOm/fAEMtxN1eYoD3wNJh0amdHyOo4nDLeG9OoICUDKSDhl1ax5y" +
            "vlfeDga0YkCetAsWdEU1ivyqM1u06nf/nTI3LXKcjVPFiml4wLScpa4IMEx4kBHldIiHZEgplRLA+qP/owbjXbDAhjag2R8VLGRR" +
            "mULFDbk5/XoQD0AbUWI9gcmOUgOqF9VptBm93pvVMC/ldL7N7phFOFAMwxro1MAXi0Lc+KK2FgqpkFa2YYzgXu+7ICiUjftwh4zC" +
            "zeNY9xnlBhhsKwCLS3zBKNBmVdIf6yrVfvm+x3Mdcm8uuMxWp0MUgfHG/1g5RfPx3uSynavpQ/3LDBAzSCqPgDRKAFePLRxwQRhm" +
            "jjTCv/IAsN7mjIByfQqApkegixgRTDYCdV5aAXAyHBzNoDNg6rHlsSv1U72bLoFa3M8cw4HOwwyd5o+dkjxZT43/pqOFAPXIqJnU" +
            "43P0prGE/5pHYMGCfvBRDltCmXvnR4AZTpcZVfE2nqoH5qAAGH2UtYRv/jXCv5QSLLUCoD0BPwpNLhgCE+dIZfiP8QLYn/CzY+AE" +
            "+pyTAjAYTrTCSMP1Uu9Xk/aVJ+D8CkDs5cdih30bmK0vna3il44VGCEEqM+zIVdaed2uj++7r6BF+F9wAmxLP4GVxbWcFuuDuc8Y" +
            "uWqxzBlSpw2x8cdnffHslY1/H/Or8Tfrgvmt9gVVigFbgn/M+LRggYYav5bBzOtBP20cNZ5ukOYAWo3lceWk74qH9deZN0+LnBNg" +
            "jnY9D06OjbIzCFS7b7S5j79T6GZXeAXfgXkF8o1WzXV6nF5ca76uQkW1jlrCgBaMDwIcIjSCv4q5Zue0qHZb9fghKmeZ90K8sK8b" +
            "PJbpCS9Cnm+n5zUO/KFjHvjmnZDXiYEq7sdSBIwysCgCC1phNjfw7T1V3/Yg5KgK34yMozEUgU75zkknIJgT5UN1qvg5eeQebqRr" +
            "Kc15anqk/p+NKfQs8DXApRdOuEYdnkI1OlXGmJkCELplQjLKebBERFJZsKAjKiWfy7/18Ao/obYrAObvuPQ4OW9rqe5g76ibB/ll" +
            "19iENveRHatf6F6zDWya0ZNHmdYI/FyGCkUyWrDAgdlCW31sfyfVOKEVHORhYRcfhwFSv7TcV58yKkXAnzS+45w3X/u3J99zvr8V" +
            "+7MKJLk4mfTExE03VfYjSOQXGTSdFYBQ8oqVXR2pLZgtKkXAAje+9BwzVyScnXksxfT1ZVizFRpkFABoD4C5XykFCxYEEYj4IQyw" +
            "mS2k5sBtwHpw6ThFjBqbOQoa5wQYreASVfPRZDudGlVqBIoOaMo+5e0URN1tfRr+BAWgj8cgeJl7ltswytatGQsWjANtEXNduKOQ" +
            "WH2QLvNohQ4RmaNhzHY/F+fxBqCxzUTZ4Y8LFjgIudWcrbXH91ueF2cj/2BD9Wy91gava2zMtvzvl1+bV6fxD3i3CJ2LAtDraZ/Z" +
            "ORK6eskBEqWXAQ0xpO285NRm1e9akC/sKPasZmLLCcELxoHlup2EpNQIPolnXSL48wyYu/A/Sf0Cc3N0vmH7YzwB7o0FCxx4aK0y" +
            "oZ17Bp2OiY0v7jp1jQphA1uR4Rm+zTcpI4xqJYUVAfcKV7kEUvdruVnsBdomH05dhu8mhUPARsFYBD7aQOkY1nNyPnqA1D0AAR/n" +
            "ggU9QR5SmoaiThh5C7lfBF2buleXdFQ22ffD9gLYlpJF/l/QC2T9W2FaMromxhWxBp8Z8aiSbm3q7+3hk4pWAm4A2OcEnBexKjcr" +
            "dEIVuzxq1LTYcyO10qnZnCKcd352TPd0LXzJxMDC0ZyXGXDBiQjFWfYmrfADo8WIXhFi0855xu3wMibrra6hk/AY/81Vtv56p+CF" +
            "Jy7QcKMc7DCgi7GkEyzn05ZwNlzLCLWt/qZNU+ADgAOAlnMCJg4yC7l//NcD8T5d0NENxNG0ZCUYACvPmFMnhqhy1Efgj1nu+7xe" +
            "LG3A01IVsri/F4yEmiDFzo2+GYUw8ynJs65pzKxrO474CvKNd/h7o3vd2JtPF0zeWzF+y5EJx9hBausC3JxmTmsLzofAuAJgnR1w" +
            "KXpxRmaMdDsOYJPj+d+oXmpXftMmxTS8gSe/WCyDeOMLqz6OxJ/Ey5xYTvNVO16kJ9ZqtJK75DmNBtz2HlepAHROs0x6C05AST42" +
            "UYXHRxNtrPwM9Dl0Lneem0Yk6OADCSTw1Ydr30Lt3nSah4IJO1YliiHzSJPNuYJ9tVltKfdzlYwDuSxYACBAEibeUY2CakHnJRUB" +
            "ZSAlWIY+Gz1J+3LSQH1E9n+yQ4KLDPNmi9aUgDIciC63VCDucu73bBBtnTBjz3ZUARiniF4ZeQd65yzNTGguLB6BBcNQk/37WxP6" +
            "ljLaY9EcfxB9mKMem24d52uqoc03RbM3rICa99nbhdaTLHxwQQha2NYCv7249KI6QFkLC1dFxldV2cGwKSaBowSY3YF8j9X/BnPv" +
            "UxMvhk0Bp4LD8idb8ukExfa51VnD7JnWvdz5VUNe7LZnbJMYgGV/7AV9EXV21sytPb1aU8+gU2V/RQpDu6FnmKnsWtZbM9hZE6xN" +
            "IuXJwQsWdISR+i2XWEdpDcAEs25McLlGwu7TmAMxpFnGrE4avuU6WjsWG4kL9aa1DcEXh6mF/1194Z0nd0ZnRaB7Cw0OAepcQt/E" +
            "gWccC5h2Qw3IeMGPiMAo9Xy6o06DzTCUfrCej6xDapYW50ODK9UnoL5z2j4Mv/lUGP787KvkuXAK9zjH2gG2Cyo9AeZkayshelHB" +
            "gh8QJeeg+ncA3l3SQhiFztqG/zwEPC+C65umGoClu6ad4zf4XeN3cIVW53r7w4HGQNeGozh9nIMxd7nObddngkkUgBhl9kRlqVUT" +
            "n/rNli4wtxZdcF2wFnSdTEqncJ+uhVfpyPndmscQz2uXZzqnvaztfUxOcY7a2yyODc+r6ar6y8ICF7SgGqKWImB7Bc6JK6bXMxj6" +
            "B5fa3qzGZOVJ2eOFOoYDTYypSo0JvdG5NvLgRVrozKNsgpis0tLltvuyO9CCUcF1oaoRatbH9ej64s5AoxfghZOUf1apOvB7Joh7" +
            "dMtgoNnWf8Gc0BxYtTUBl5BPrthVFYteugSGx3v0qXU9bT9PwIhLArpiaIfUrC5j5d94wcpt21bG8ACFkdDfMDlSsf43tluuJLvL" +
            "HFOx4JWgIeqz+2M4o6yuTqAKlIahHhadIcy2rQkmcXu3oWNrcjNLb/YXYCHt80vXWInFGLKgDT4eod0AJ4zfk4e+sx7h2nDaqKs/" +
            "TbXr/XNuf6p5t+lFboICOUfWBFgPkP1wvJuHRYiOg6BWx/BWu1MdvZ6bkIDQPXImUKXxcbrcM3IVFC2VLizuRsALFsTgo7HaPozu" +
            "7aHbWlUZNK7243328xxnBudgFFMuWPY2SI/2t2XkLrJOt6lqFMQcyuE39NxpKKwLFvjgH1BU/tuf6McYJp4w96tBbAz72ib0ioRz" +
            "HCEZbuBqjUDonh+BcCDZ7ekILuFVjqadmYVodhhY2T790yBPOxToqhprwZxhy1HcuBG6OagUL9rHxMzsZVNX5xxLKy6A/vaVLv7i" +
            "BQt6oDRS2qFA3Qfc6EN/Zqythpbh1XX0+V7xPK89DX/ovTB46MsONXSdiy2OVQ47f7uXOcGb9vO0T1kEAMf2aYUAcTBcY5kUF1wH" +
            "5jj3dR49MXdlA7FFbSO3QtC1G7jX5fkRcKq4zp5vvrsLFnSCvRCg7xAcndxavJoXwdgvyaXVfejz9b8ToEfWtXCg0MJg39ZEQ6of" +
            "e+4cYUR23v7tlsYvp2vaUUNiziD8d8u9clJyLYTCHQTLxLfgNDRjxT2i2kSTU0wcvjRldw477MUzyPrXdy8UrtADbUbzvvnMTjBR" +
            "CHsILk05C+aJGF0EiLyNCYwWm2zVrS2/qcm7kX97gX2boC6zcRWN5Y3Kchv5DOO7UQR5w2Bbjwb22n0Ig1eht0WtzZRXXw9mMXdU" +
            "A6MtRKs6MXP6Wi34sdAegnEaukTgzoGfTdUCpwREDcz4FYHBBGtSttZJLVgQhLUCN7ZKvi8pnUx6PTOYkjGeYEAYtqqi7aE5GDvD" +
            "HtyoEuBap6m6qH6fYYbrFXMeaeO25r80+x2j/Eu/g4K/Fs1+tCV/sy7AuTaPF1pwZWizq/6IZDU1f+ln+79AD5y5yF5xtZZRbQ5K" +
            "44JrgCEYS7jTAlqHPUxGxsBCfkhiH94hw5+Mx704uwMVAGSpDXUpNBriQ2HBvGv+ndJYiXoZlUsPTVXRSwoIvrJ7TaU9Ks/BHx3Q" +
            "Jx5Mp2X7d61s9hCJ5TpjPo+2ueAqESJDpuo+24lbSKkP37s2TMHb4nlWrVk1/etXwXzcLHSvgWskrAUXgkcRaIPDA2ceLXceXMgm" +
            "QY0rbSBEzgfunIuLFPgM4L6RFRFT1yxbFYFA7U6Nm20T+LtFgcX3/e5dN/uBERbp+OSWk2h2LILv03k6FK2ptFg96CoC9osvisAC" +
            "D1ojqR3fbtfQVzqZMy2oYNYPvO62DCqjHb4vnG3BKVBsroWKWphfV974qlCTMTo+E0nftf3s+If+hhHSz9WfOYW76nCgffOOlo27" +
            "vthUcf6+pRQ+4Z8bXzrmzFUJo09TfTI8IZRpSBm9+iSSuLNs7gbblZ1phQGZ7/07c8EPhkFexE60Ggu0XbAgji5Cfy08147mWEKC" +
            "FvRGnYhqysAyfY6LmBzUngSAT24d1kljnkjQujAYOO+02GikmC+1b/v5YmC6Hho05YDqkDc7f6coY8izXbP1d6MJFXDdBL2CuhYs" +
            "8MBMiB6O1cN7Hst/OHoFiLwC8CLZWgyca+Y1quxRqC4tDbagF2onBi+0Mzk6C8Xt/P2USJhTnjeorQmonRNAHHy1PiHhfRANy2kr" +
            "0J1XQ7Wv3esofY8135+IiygAXTJ2XWRdCIR8OvCPIBAtmB5+Dj3e1DhGTj8KrXNdNvlRXtsHn60DpoXsj0JriMeCBTXQSazpIiFB" +
            "vgIvwCMGmR1bPXfdcxza9mM0VfjEYEdGi68vPh+4/Me+EErYI9cuL3SNBuqpFQAbjns7CGu1CWvTGFtXFiw4HVWkrCtMnca7FsGs" +
            "K8jntP7Rm48Rn0cWL8CC3gh4PAfkMgucsSKniHTxVu+f66UkH8+JwbK647zhlH3TuQEismJDN4goCL0WPLxCnEZwHZ922ncR7xec" +
            "C1T+S34DWaex71LsD8wwFgxHkPE569HId6f5a8GCChVzI4vf1e5fI6602teIxpoAAWEuN9d8znSHFlf4d12uJwufXdciXImUe/Fq" +
            "Oj6mi9dnwdUhyok0nxrGrmIeqSErYH5cDG6FeU4zk0I54bu6Uhf8uAiMqhqzG8czEEZ3FXXuVNzdml9v97m/l4sYL66tCajCgQAC" +
            "cS1OaaYKQBtKj4C1+KrzOo62tQVXiHNWOxoCbCtnV0pbCy4LdylKdZVP3GolujrJKb3jc/Yt99HYvVbE6nN5DK7dK10/4HutJfZ/" +
            "QTtig8HwutOE//a49GH+qZmE/UcRbrnrFv67wBMOBAj7KmHWQlpnux3H0/4IONd7d2NFjuWLFo/AgmFoLgO2J8Wp0NU9GL81Xh2u" +
            "aPScIj28AvhFKQL1OfRpwQLUqGe0vLrcPbW0a6Twa6yzjdAM0dwi1Fwphf9rf3WFK5oiJ8FJ7x8JqO66YLxBSfoC20rmK6K3BZdD" +
            "Uym4MGKW/x66xKtBn3ccqwNb8jl7s5vw7RrLI8zZ4LZghrAiHEKYPohxGM1ORukzHkJzqJq9508BJxyIyOZAHas7pdu2xXR/6vzZ" +
            "dWV4qBpz6NAuGKV7Gv1cXTDfYgqAvw7kxMLalrDKM3At7bzgcqjTmP5m7ysccA74Q4q6Iv5UYLi0Pzp4wLI1Iv2I6PMToOaX0Rjw" +
            "cqfOMR3f0VdEl0f7VI2Bakv3mvDfs9AFPxj8g4AbTAZl1INNRrXf7XE/A+sXQ0gCaFk3f8rWPRcyqLT58wi63wJzUf+S2hBviJon" +
            "gMtzAqhrTEdVl7GJKrL7T+iVzun8j9VjThi1jl5zvvutX3ZqRrQtYM2crqGdF1weinKMGMzO9TiNnoF9TcMnvWWGR0zjzsSDq/m6" +
            "AxvAtRV0yWaE9m5rnmEBWZXQX7K/Xi+24MeEhz6o/qU0bNpr7jw2kbMvDox4+aNDe8i4t5+9EKJFs34VHvZaY0Ba3+vhQLZfoMvM" +
            "ORYGmPTHDW8ZjjkLqOepW7wxXQNX9VGzHwMRBWCZEBecCss23kJOY1HbJXhCX2v0ORBsz7FCXq6MPdSNWNoLatwCy4mvC4aAvV/L" +
            "X74jOQMPTI8eZB1NeiWKwAyK16hqYQv/pvu1EnCjfyZqVTAJEKqFSmcNU/QQdZhWx6KqPl4Gj0uuey3Oism6rXNMgWXx8gQFVOKZ" +
            "91ghzLdlF8wfXFKV8Qqcw/LSSrG95+TQA212/x6hShcZZmeYVEZ6r67ZtFn/3PhTm/8ZlwCdtKvVgh8RiqwC4TaXXnM0FR1PbLF5" +
            "reZIYf0175QCHwAcACjbBFD33BBRFUPG5Z9pENRqfTjBrdyhGn2DUryT6ng6ymB4RWv3YscOdd10HL5by5eItCvStvhbk1/5d8GC" +
            "7oiRrRL4q5OoCU26Nz/Zc61PWZ3ScPRny2MexthpuMQT+3jWaBa4NhjXjBWjEMs+GjccSzCAF/fJJla8e68+d2pPKFk8EAKo/V6w" +
            "oAdiY2mkdQBn4w+B7BpLIIYKo057OPp5mf3rGYVhTlWFA60ATioqsj0BZRZnapGhCsAp9BB7nixBonNmp1RoSpDnPTq8mC9J22MU" +
            "S1USlLs16BwbbcHcEFUAwADZngD9OcET0Jb+LOE4g3lL98R9FJlRQFEuUSVrzWeMykyMhjeg8gBUXoDl1IAFA9AeNtGKk8bgmYi2" +
            "q+wS5fGOTWVqiYOcv5dFsxbNE4OFgFDMqUxNASvtKC6TFuJtXg6XMrSR59VJE4K8XwMXxi2vNiqNAkDuB1hiYxeMAWPEqBkvAp6A" +
            "LlR20kTRy8M5LN9rRtdRPgduMKzJHQ8oibAH4JzWtgVXCoc+2iMGO2EwO+lFrqfTdpvs0kX4H6823TCvEV2vjVoKfAOsC4AKLe2r" +
            "gCEmY51w/OXEIRfnaR6auaDWRFYo1KCe7NAYk0bC9Kl7oPNOUu5CmZUmWQJE3D4bUkIXLGjAViQ1bVFNsUT53R7aMYzFzyYx3nfO" +
            "VL/tax5Koc4c+Z27NTk5aW2hXwv+UOvv7JBIeu19tGBEWITCrMOBzN9h6B09N8SlyqdLiY0cTshymuF2PZKwUgL2AFZAUu4ORBBE" +
            "kM4e7uq1uJxLrA1F+ysCM2ifhnEaLQQREKZ9z3R9PYrkOxp65O2Vt/v2lS/I2ryjeWGCEv5dL4D1HAUrtGBBCIpeiATIUgDYo2dW" +
            "fIr19yatzVoBGFILxmzG1CS1uOCrud1WBZGqTxWcZikA+nu1IHgefbNg7rDpZHxXYyeRZCiplt6u0+rt9QicoAiMP/LmqwhIVO8r" +
            "AODGbA7ECQsIa3lwi1WCvF+9v8fDhRu1Y/FdDe9zQrA+fSoa9QBYWmM5HzqKgH1zbg204CpQWv6pCrPQNtbaR6ES/adanTIPBeDM" +
            "ZbTgxxvalgJQhgLVFwJXHs8fr3UWnADW/5zgAeiNUUg0IkCOkOWY2Q7H5WtgI7xF6F7/IqJylQARnxKiOJo3u8diOOMNmxwc+DlV" +
            "7O8ZcJJSH7rgu1nOcwRQoj4i1VYxgiOhLVjgRYxEmAHJBKnnRQbA5U4s6ulSGYhMGpcmw2vjITZ6td0rsX77DGHlNQZYAlIyWALM" +
            "esc046Kqed3xatpkwdRgz0fDQ0I8BoObijQnyLcRMrSggXJNAIp1dVUIy5FhLa7jsINjyMYVXdKV5dUS6x+Got2wpJgva6zQm5Ai" +
            "0BcXJMx+E7X+G1kzwO4FV6QyHWSkLxJKARCJ+lBjnfqCBUEEeZEW/qUWtgDSspZxbhrO1pwwy3UCgVDHXjHnHZnCuT0OtlPOf7Mv" +
            "6q00LAsPg7lCLchmcSVNWfIZSyhznASQqBChMkzIOT196ijRBdcMW+BvKgKN2H6qPznY0T41QYaY+ql5YoJ8Z4HTuUTdE8DM1TEC" +
            "9XCgrh6BMRWAYLqS/iO5+AM054Vr5PCROgdvuQszoRUAElr4T1BbINelsAUL0KSQ0vLPrDwBUAE/bAlX6rkqQtubr+OpjgrObVaR" +
            "mSFatd71roewnD5iJwgRGBkU+Nj33fTM2gMgK+WUubGJbaOsGZPRgovBNbkyAKlF/0hIY32oDh/qgVunfrqUM0aVX4+TjZ2/w2B5" +
            "AgATlKj/8R/iOgFir9BqiRvSDm3Kk3uq0GKSGQ7fjGgkKrK7zTKVsa00LA2/IA6bR1CSQqzvkdz/hNXbv4A3W+C4rz7ZDpztgHwP" +
            "ZgmAy0mB4eE3XTGQR1xk3cFooMivHxkEEgJEysPJSEHrFSTWYKxBq1vQ6g60usX6zU9Ib99ArG/0M/SapJQFU0FZOlDfEUgCriIw" +
            "FimdmySn8Ag42WPaIrphBrJlWvuViECbXK6W5+0k12e2wIvIAHVvqd8MMiEZtrefJcA5mHOwLCBlAZYS5aI50DIfLugEQ1pidYPV" +
            "46+4+eW/KevrYQfOlAIg999RfP8NxfffkH//DVxkAAxJOtx4pPHfK5sz8pwZzD0RhBnMxLJBJ4TargwFIgKla4h0DVptQKt70OoR" +
            "tH4Erd5AaAVArO+wfvdn3Pz8L0jv34J0SCShUgSW7ZEXALAI33ArS/hnCXCh/1pxjB2y9P/oZ5kfU8+Yemz7bJIXx4WZcdq4ImBC" +
            "M3Q40NDgsWtCgPxG6pw5TFyjw9cuDv9h3YDl5Giul7EVBZgLsMz1X/UbbPbL5tId9epJcMHJIBgl4M9gZiS3j5DHPTjfg7M9iueP" +
            "OPz2/4I5B7af1V9jURtZARhV+J+IeZwUGxzCaMwubKsbWoRrnDglXSPm2v5NBJGuIDZ3EDf3SO4+IH34BcnDr0jv/qQVgFuI1R3S" +
            "hw/YfPgLVloJIFGG4y4KwII6bEWAWRk5TIyZ+ZTeAOe58M94BF5LCNDYmMJCfxWj6IKKQIoPqNYEQNrVOFs4UAzXLkDbYXjX/B6d" +
            "4Lxk/aem8pKmFNNirhQBaG+AIAEIoSKGZkCDC64HYnWL1eOvSG7eYP3+b5C5UgJkvkf27e9gzpG/fAJ//DdwLgBIHZ2mKXVug3Tq" +
            "+ow5+ZyR2fUtYmh1hjSP8gSsIDa3SO8esXr3J6ze/w3rD/+K9bu/QqzuSiUguXlAcvuI5O4tKKk2R1j43oI4bC+A/WmuC7AxZwXA" +
            "zf+crHgWw+1CioDjCQiFA10Os6vQWJgF1U2A2OjVi02IABQZ5OEZxfYz8qcHZHdvcLx9QHJzh2Rzj2R9h2RzB0o3ANTCTlp2D1rQ" +
            "AiIBkayA1S1ACUS6giw2EMUN+LhFsnmAWN+C0jUoOwCy0K70GXKayaoUn226z0U1/x64h9I+73CkgSjXpxFIrNXalNu3SO4/YPXm" +
            "T1g9/orVu/8CkW705wZidQuxuYVI15YC8OpaZsFIYC7ARQYujpD7Z8jjFjLbq9BGLsAs1f4uQDhmzfcdzph8rSTYR7uoLTabpjrD" +
            "4VscO6ySzXAgT/aNxSaTYWZTw8hVKWnqtUdYeZkPgUz3EsDFQSkBzyny1RrZ5hbJegOxSrG6/wC+/6B3ETVxskkVXvSqG2/BaSAV" +
            "VpGsIIjAiQAVKViuIDdvINb3EKsbULIGJamW/VlPnDyAy/l5FlEthNf7VGu2Q57rjOGKQH1erNfIp0vF8gnWIKKUnSNu2C6jIQfE" +
            "KqAZPCVriPUdktu3SO8+IH34qVQEKFlBiBQkVtXagSRFPQRoYXILKpQ0KQtwvkdx3KLYf0VxfILMd0oxkHpdALjaWyNETjHy6rMg" +
            "4GREAoCmiA1y845fGv/1owbSUzPxXW/P2FECJAHiQuyHrb/DajAqrfTQGPvQ6o8qwBKgrPlaA+LiCHl4AlAgS0SpAFAigOIIIkKy" +
            "UpYyEglYVJ6APhbHBT8YSCkBggSYU0Cm4GQN5gJy84RkfQeR3oDSFSBSkGBwUZiH0TqKa4M9zrPMJOzKs5dXANwc/YPJ91ZDY/F7" +
            "KQIXVAAMfDydAWXIcNI4odqgUgmoPAHp/c9I3/yilAC9PTKRqNYBiGRRAC6MLq0+hP58nMFvaG3PR8oCMttDHp+R776hODwrT4A8" +
            "KgWBPWsC3IoEXpQa985JhxHeO3JsUC27tvUSY8P3Lp0LHZv7KrSEA52HCNoiSIZ4b0atEIVv9Sp/4e26DRgoMvBxi0JmIEHIVmul" +
            "AIABLlRYR7pRFrP0BrS6UVvuLSRuQo8AACAASURBVNL/ggjInETNDILeopElmCXE+g5idQOhra8ySbUCQMHxXs/c+d51pWkfzCwq" +
            "qR7wE743VhlzR7CuXH2YCUSpCvdZPyC5eaOUgdu3SO7eWbKYPbFcSwv82DhdxqCT8uEiQ3F8QfbyGcen35BtvyhFIM/UphqlVxOw" +
            "CK1P1V4vrEaPGW7P2gwzaPMUnwHc1S9y9Ns46DoIplz8VQU51d+zT7/Mxqt2BSCjABApt6XMgQLg4w75yxeACDI7grMMyHJwlkEe" +
            "tkjv3iO5ew+6T0EiBYi0N2ConfJSnTFEfVwI5zSYEAuqLK/JSllqxQqScjCE2lRDh6qFmECjlwbO5CMbtkaA88Kxn9aPSSgz0jCn" +
            "tNmQNu/qsSm9AGZdpiSABQgphFhB0ApEibWmyTU/LmN8Dugy959ui1W/uJEinoOR64tsj+PTJ+w+/wf2H/8dh8//iePLFzVvmpMS" +
            "a7qlovxWEmu579o/xsdlOOK5R978eL/rCSgkqV1bLG83c42uxnyBsRvklPy49i/5iaPD5Lew8xgMJTGIpRK8AMjDFgBBHg/IX76D" +
            "syOQZ+DsoD7vM0CkSG4flducoRUB1V/9VQH/c5eDv8Y2TS6LBfvBtBeDAdYKAAmQSFVMdrqGTFYgOoJA9QiUvorAkPq5+VwcTmiQ" +
            "L4TJ8L94FFENgz3dVgN18Z53acvWuugy/Xk53iJWtFXOizoUiCVABcAsACQgWqnYf0qgA4WcbJdx3R0jjJqWDQBGH5MlHxpeSlVl" +
            "hsx2OD5/xO7Tf2D7z/8H2ee/I3/5iiI/ArIycxvh36pCVaVGHbvX5ZwKwBQyp7cA9/vEmPy9PIgpuM2FwdJ6quXhMTCHyZCdvzX0" +
            "daktCEBRUsUSC8W0WIJlgTw7AvQNTAn4eACyIzg7AEWuYrxv3oBlAWJr0ZOnU1zPThOuTebciIk04XvMvCgCA2DWoABQJ7KKBCJJ" +
            "y8WYJBKAhT+0p6siMLhul+d9vcD9ZNaTFAD915XZYpFaJ7WlzecbGTmlsnXd3lrW3q6dBYjU4l9BqeMJMI8v47k7oh3UDXPcAawz" +
            "VJhPcdzj+PwJu4//ged//Hfw81fI56/KcKYXyxEAi+1B/3S+BH5HMCRCoqtBri2vSXuO+r3biMWehf+78q07rdWVADZ7ZTiJ5zR2" +
            "XCtdjDe0SXmXkQJ/cOhGN8ecM4EhARQ6PIsg90/IX74iWd0g29wjefgZ6f4J8riDWVDHJCxrb6VeqAPGfGiyxEGDsI2eohgSDgTo" +
            "bZL0iy4E2wtl0+o2TFbA6ga0vgOlt0CSAWKvQoJqtIlgt1SH3REI7Z7SaO/OibcC6MUUx+Cfjfe3Qhp8bR8ocrRmZHuKIT32qpAy" +
            "lUZfQ/WXWYAlgVio/f/v35XrAMTNg9qRSlTT7aLQ94Xrhulp0bclmzGVASr/CVekra9rUpd7Tx8KBhVrJrM95OEFcvcNcvsFfHgB" +
            "8gPAha5OPY6hVvJABWA6Su3RD2NKzKG8yPt1Wkwsg3ZpMsWV9gBW9mUJMFs+cm64SQeKM2G0PWjfj0wQ3outlj018ZvBs7Dn8VDr" +
            "NrOtT3lCa+WurHVNcYQ8bpFvvyN5/oJi+x3F7hnFcauUALOjBiVlIY2T7YI+T1cB6GMGaEkbY+aw79mEHEhjCa/qFFGu3neh0H4w" +
            "oWMk1HqA9AZYPwDrZ2B/AItVpQTYtFkRLkybc22iKClI3UO9Z1zngl8wifwOoS3dRNa9WtkdTH1dXqeejfVEn3xjzrWBMCGHRAKU" +
            "iOokXxJgWDv7UKK8SUjBUH/F+gGrxz8hffMz0oefkNy+1UqnmmQXBeAElLvfsIdcWujHbAPMsUQRuIO43AfY3PQ9Q+3FNJQS8256" +
            "ka/2ljNLcLYDsh2Q7UHZDsiPam2dFRpLdbOYX0/pRILDJdQxPACNVF2Fyy5uCKtxXHuym2RyTKQIRJuKqwRKCbgBUBwBIpI6Hkg2" +
            "xlXF/dusXScpArUyO6bXBRLqk3RN5vI2crPATn0RSjQWgc4RARm7ldDcR42QZe+1x5qqSFlWUWTgwxZF8h3Zy1esdt9R7J8hDy96" +
            "UlYLhNUZAsb96aug0S5cBYBK47D/HX0dXLlZm2m49sczJVlpmi1GNgGbrO1kMgH0gmggweTmg1cG1de2J+AWtHkAre6A9EW3rfEE" +
            "1BUBY/RnIs1bjMEAegKp+rohI4R+ewWUPi/TIU0f3XYIRiLBMpvoAQMRbhNqyw51q2YyJzFD21MJwoSQpQmYlOGBSQBCLfqlJFWH" +
            "04k1QGsQrSE2j0gff0b6+BOSh5+Q3L2F2NwpBXRRAAbCNkhqRYA9fJaBhi+4fNSSfGwDZyNxSKB3vjQmEXcO6qAAeOrCtrJiFABZ" +
            "ADIH5wewVgAo2yu5TeYAJEAqZFQ5reoGCv97VNWMVK75UCdeFRhb3TPwp+ojXPr6i7j5PFVJQiLB5OJarLlOyK7LzbongFlFZki2" +
            "NAVPo3XAYEXAzYesMdKWaeR+85Y9kC1B7BR0qV/XtK8BwXc0NGUlMHoBAVxkVejP6g758xfkL+rDRa4WdiYrUJLASDtV7Lc9msNK" +
            "QI1BdFIGPEqAT6qzflc6gTvh2O9frzaIywukwxA4WYOSNYCNfmeT1yJQtMOmMQFKNxCbB4jbdxB3W4jDEbQ/gI56ImXWwmElMBDU" +
            "OhTzUekKEOdqjYpWFuweCbEs75AYUwE4J8ZSBLjhx6vyB5zx6I61YXXzm7Kqu8QACYIQKcRqDbFeAyIFixSc6Fj/dA2RrEHpBiQ2" +
            "oOQGSG6QbB6xevdnpI9/QvrwE8TtW4j1nT4QbEE/2EI7q5NxZQ6WGUyojJ2U61+sLKoDAes8O6QMAEHLV+2H9TdoRLLmpJq1wDYe" +
            "VfyG9SrzyguQa0UgR7H7Bt4/A8etUgLyXJ16bvGqcq7yvVcvBSCAVtnFbXyPwewUjCZcosG3gVFYWhhn4OF9i4hwJVbR1ZWMZhWh" +
            "m64jP/bC6Ug7urYm+AcebckuUMbQPqgLq12TNq5fKzx17zz/E6PunnFzMV8JTKwUgSIH53tIEIqXLzh++g8QEeThGcn6rvQCKE+A" +
            "Kccqg5whrRUBS+T2KAy+d7WeqaUJWDec+UTxdu9Fx4qkNCDlDYGyPK425Se5fYfk/j2Suw8QN290+ZrQiK6atM4Bw1tEskJ69x6b" +
            "D38FuED+8CcUH74if/oCuX1SB+1UKzvVRAwGIMFcgDlXn2wHuf2KYvcVcvtVp4+VbyuEkXsxnE0BiFPT6Kpnl/jsstCeE06wsp6H" +
            "zSWJcoGvSDZIbj9g9fgeqzcfgPWdCutZ34FW+gyTZKX50RoQK5BYg9Z3WL/9Fau3f0b68B7JzQPEalNbE7CgI5jBUindnB+Rv3xC" +
            "/vwJxfMnyGyvhGQGIKUjV3Nz0JlxWhuvlfAN+CjD8Fnz3XffMkCVSWM25fqor1Zi1hWecj0AF+r9uMDhy//C8fd/Q/H0GchykHVK" +
            "MMgU2yyjqkJ1vZ8C0HXkt2nmI2BoNg0l7Aywy/M03xxswVY4EICiUNVUGx4zW1YxexMEW9o56QW8ioD+7lEEfFa1GFnaxmBzwX7G" +
            "+6xdSC2hGegd3rjr4Oo5ptyqTUI8PRhD5/JdF1xNAK6nYQAocgAHxfzBOH4i8OEZ+bd/gtI1hF4cDBJWhzqCuleyt9SAmtLgecRm" +
            "7OaKOxE0deE6H7d+VD+5mihqBZuxBoh0heTuDdJb9Vm9/xesihyU3kBsHqzy1SDhRRFohTrFdYX0/h3Af0OyvkPx7gly/4Jiv4U8" +
            "7LTL3Qj8RiFQi/K42IOLA7jYo9h+QfbpfyH7zDhun5qKnkWKPtpo1i16e8SBPg6V1JZCN+rmr+wpJXNtzPR+2BmyscZUkwUXDBQA" +
            "3SglYP3uf8Pml79p75E68Eus7/Qpv9XaAJAK26NkjeT2EentI5LbNxDrW302xaIE9AXr82RY5pDHLbKv/8Dhj/8Ph/+fvTddkhzH" +
            "0sW+A5D0NbaMzKyte3qmR2aS6b7/e8hM12SSae4ddU9P1VRVVi6x+UISRz8AkiAJrk66MzL5pXkGFxDrAXA2AL//T8S7R31Kbqxy" +
            "QkDeOmBLd5wx2DCB6wSASoJzMfhZp6dSmGrKdQoruXwmgoAeh6LnTwg//xfip4/aCsC67GW6dgkA2Q1RVT+o4ywKzEjN65YvumFI" +
            "hucSE2bN+H9JQcCxMFiZ7mLYfQYq6WUI1GjrE0GgjWWgYWivfN+aFnoSTeME35Sco34qXp2OMTtGKbOFnNvjtorASoGiI+LwALV/" +
            "QvTlF5AMzH7v1n7bhIyhLwx0Gc1YjDys8ParJApC1QPkBY3sOhWFEwUOAO07kvQhSzLI8YuZSJoI2wSGCJYIru/hX7+Ff3UPFR5B" +
            "cgG5eQveZoxm+u0sCLQCGUuADNYIrt9DRfpgOhWF4CiEMr63qQ8uZwIBh89Q4RM4fNa0CEDtn0AffwEnDAaAlAB6NIazP09MAMgT" +
            "cDHO4SeKou7gFEGgTgBI9DucWAJigBDAW97Dv/snLH/43+FdvYN3/R7e1TuI5VXq35/uHEQEQAAiOY/C/FKFhahMf0YFmHV/jMNU" +
            "CNj9/H/j5e//B6LHD0Ac61O/48hWoCOz4gEZs19099PvitNEqugsKn2cyKsiM9uxS+2bPeP8fyXlKqUFyYsqeszaQR13oCiy3lr5" +
            "dckopXvXaJNXSnVGX2ZnBLTOykQmzUsKAnnVhJQ6H3pOI5sIXZmsy3SnAlmRn6si+jP//ZrrpEa+tKh4bhgtLIP04B7uzXL1TMdC" +
            "CROdjMFWO7EVNl9vVNbSFvh6/beK8bcCFegip8xhgNk2E9jMv635MRlPwpqfXCy1zysUSADe/jvEx532h00XrZp454WGrUFCgPwl" +
            "4AUAtobZV+BY5Zh/tqwBWhMZQx0foQ4PUMdHQEWQq58hvAUSd6yqMTJ9xxVdeKR+/bVSRW9BoBIFLYBJhBRAkCBvCbm4gre+g7d9" +
            "C//6O3g33+uDC8tfmgflHHKdFmtGDZIBVfvGx4dnow3/BeGXX8FRZNxH4zQ4p3xLTisDi7HJxV2cIrJ7w1jntwTLPgWQuWbar7O5" +
            "J70vKqjsVCxhIPkwl0fKVETpiXSs9NrNgq6q5Hlbe9+kHuXc3TkxlDKkLes0au8cftAaFIUTg2NC5i5n2I28jJq+LMbENe9aoiqN" +
            "cw6f7dqrHWn1bXfndxVJvkb5wLmDkzsk8rWRLMBMBlWuCKpDcXZpHus9P3JJspWflJOzQ1A5j2mcVjp2PkyEua3obCEA9vdJGqnt" +
            "DcQMlazRByHTXRWkndLdjGaQ1sgym0sBQGntrZLG9K5Sd6BkcR4QAeoIqIPlBy71wm1LEMjosJqwc2G/KrhHozY02qUqOs+pKU9m" +
            "en/Dx9n6NOhtjZMfCn+dp8GiMH7MOA3ZGJlq15Mdc2JjsYvNzjkAyoOx+Wu71xTf2fKfHQnZ9y6lUvJBxvpbM1Q+uGNdnEswTHiu" +
            "Yh6zrQcYdv7SfLaxADhhqzAGwolMyZC9pymu0ntHNfTKD5cumuMcoAn65DW/JkAyJ65AnGgsHbE6+9kJmXDhlHj60CAXrhvbg7Ju" +
            "3qvtKhj6+jTrHw8lPY+GhJ8u8NiwnpX5besJJXsfJGtUCgNfFQOWU/LVEHQj4eQnAmfQdNymXEC2A6Q3SYVkQgCDAWWEAE7XKYId" +
            "2sWcvWLi2oZpgKw5zxzyRKxdzBhgqQUwthg97bOrtAAQH8DRXrulmbMqzFlS2fliqcxYJqaCPArYIWwaPK2Evd61QS29p2lUjkbD" +
            "ZQQd2JZCoKQfuUXprIXSTbrSSVBZ9GDStwSBGSMi6WSJ0GUWCyeCAFRktskEyhOC/T/nnmfjsz3HWF/XMTq5wMmspK9rp5FkvM/P" +
            "CI77jP6YCfkttR2CQFXWWqE6cG/qbjEMFNGZXXFlrgfP4xQE6ub4VhEmg0TL9E7EKaxe9UqlbItsR7caPiNFJrwPSgrdE/LQKqGJ" +
            "oVTmiZkJbN43/yDPM7u+yXY9sLUeGbuVhm/gDmppue5BF2HX1vrUxpkJFcyASoQbVlDglPlnh8YxZWVm5r8jMmaiKEPmq9E6VIhZ" +
            "LwqO9mBvARLGEpCuTzEfsyWkVqfuPBsod92zPccUAJyolrWbvzuHIFCnGS19aGtzkwT0RE7pjlEqx4DNXe9cMCc2J2sqbCFARdoS" +
            "EEdGUED+LIbKzpZIee6BvdyDazhOqlL4OL4qTSnVM0lZLKjIUi8LQDMmTdt1ws8QPM9IvNMYAsCpEACw3wMIgTjWvUeZlTWpb90p" +
            "UlFL1I7PLd91Q0NpWkvYw8zYE6CF86KJgShq8FrctaHP9mGrM9j4baZG1H/Tb4txGoaC9C91L01/nIUBUFpYNmNEFHTFpHd9Ed4K" +
            "MlhD+Eu9hasXQHh+enBd8m3deNZ2rJs+6nrlJXLQ5WUCq62MtpmIAEFmga8wO/74mgbInBw848yg8phqjZOp/z84/Zex8mz9zL1j" +
            "frFj6AY77vKbYuyAnd8G5OaPV4ih8j6QYuTSVXnp9F3IWQIYzFAq1UwCSFxnGzWsQ8MWxIYVyuyCtBQEqiTuXMDmHDo19cCguy9N" +
            "RvnfIxNV9eOOMm8F6JJc27BOqb2o5qmMjPOZK9KOTdz2J5ai2p7o2GitZu3jBUDQZzfIAPCXEP4K0l9C+gtI30/9kYkBVpx26Jxv" +
            "OTqQzol5rXzUKdEaSuPKG2f0zlhc9N8TtX3C9bIkhRl/czanQicnrwrSbl/mcDAt6AnMLkBnhNHup+Y2eycm20JK2eiYW8dIdRp1" +
            "F7VWhWZ3Jy4M4DX2Auub2hANIOflkCDHVTVaCjJDaed7fjbUvNl6fKvBZHg0Aw/4CEDiiABAbK8LpvScANsUkBI+Y+ji1E2Up6dU" +
            "ZP6ruLKazNW+S0abCg0y529r4z6xSvswxoPihIS75n0sprgyfRdDUTUyFBmdlAYKmihTiDQaYr2Pc6PwOeNc0MygsQT4awh/ZQ5z" +
            "C8BxqP2UY2UUJonWMlmK6hYEhs9kza1NS42ZsO1ltZJ4K3Rm0pvgGCR6jQOOvqwXekPbyIUApDTbfWaLwecOeW5Y6vB0fUDG5CcC" +
            "QLI83ybROv18+WkNcVeOxXmOxU2HA0yIubiqt4NuLm/+y9MpueWodmGmpCj3t89GlvGqbxrHHsfY2ygsnhE5S0DiDgQg26wExUJ2" +
            "mk0642was7aZaOolbamrS0EuUeih5rW++Z6SaHwqqroIQS8mrAhv+EZLwZRolZNfEbNd4DwggIRmBMFaAAhW2iUoWEJFEUix8U22" +
            "2X7re4cgcDGN0NRUUV3IeGByJ2jGXy8OJ7A0VgEiiCA5tXsJ4S3TBeGzJeD8cGrTKbOQAgBTnkEuk3g10Z947Gkh/qKGpydeDZlN" +
            "bUBphjPHpfrOHjQs9+4lCEwFOSFAcrI8MUPiDtSEunJ1KXMbg12/dIrceuGyrYa3CbmtQk6I80RiybTKLeI502BTmZWhO8UAvHGn" +
            "6u8iNLaJK53YstMiM0FgqIRmNMNW3cj0xFfyFyB/BVpsQIsrUKRPGkYUgSl0x9J7PGn3XW++tNI0WSV09kNjl2zbZ7sYoF1DPuWH" +
            "/FQAMKf+CumB4IPYh7+9g7e5hVxf6cPBgpURBPQCVZqFgTMhG/uSnQvtMwDy2kq2LHHFOKpib0FMbeiTXUH7dfzqUZ5KabTvEm4b" +
            "xTDT12kMy3h8cTe7jD3DFv82Icdz1dZHl0FsfJR3B1J6gGO0EwCGYv77hO8HR4mqCllX+Np3NbavWrMYlW+5MrATldHWWSjOiFLx" +
            "R1KUpIcznVC+TiY717zTNh0izeg72ymZ7KyfNikUcjhbBcaBbhQivUAURCBvqRnCxQa0vALCIxCFgNjnv7T0AQNkofJdr1Zv/Kh2" +
            "oBoHrqRadEKqvLGtauXnBDK+/wLCnPIr5BJCrEBiBW97C29zA7m6hlhuIYIVyPPTXaFmnAHZFl1mKixaRTn939bb2g+qGFzOvW2T" +
            "l+5BKZdyfQSZrbAqnurstFeAtounH3V3/WoY+4sbLfJSUBK46qEPD5sJA02CwDSQdweiy20/UsejjqY8d2nqXZG4wg2OikiHEhjb" +
            "lOuMIJzGILWRz8aQtaviPEUASP8ym4WJQMb829sSWr/c3tEzQzIuSPuHM4EgM0tAsAEtt6DDDjjuASHTphi8RSr672tp+bHyWSsA" +
            "VIzbqQUAWhAQxhIgpAcZLCH8LaS/hb+9g0yFgCuIYAnyAt3OM86H9JT1/BhYPGyrpAYxw2Tl+7MzYk0z0nR683lUSmNpwzvmfIyC" +
            "pkzIdDT+VXC6A1mrg2sxRNGa4uiSRlP3yptr3GFqXjtf1tNPDQF0km4GUilOiB4nko1eKFZjtzHE+rrIpRBZawa0AKBPro2z65Ig" +
            "0KcEM9qg2O3I7ExCXgC5voF38z2Ct38F+WvAXwHSB7wAHB6goiM4PBghDkja3d51rYjGPlEgvNaGyoFo5CLDRwM3woWbinOfcoM7" +
            "mYPiiAiev4ZcXUOur+AtryEXN5CLa8jFNfzbnxC8+Qly+yYVAMhYg2acE0YZoswp3kr/wLa7ZBqyld59TB10Pp2ChaImXA5Uft+K" +
            "6mbSPAHfZuU5DgsTgzLmfePJecFwuzRdzFkT41//mtI4CCid/E2tc1XzqpMgUBegZURV9nE7mkH6QosBr2a0bvCAqMS5urFNF50/" +
            "LBiJ8+/NFnfMetJTcW7Sq1xvMmMUJIIAWRRJMoBc3cG//RNYxaDFFgjWID8A/ADx8wPUyyPiYwjEytrQRDdachp7yfxs9QeGQ0Y0" +
            "/zUdVDuMAODufX0EgZNJtYXpLT3g3rbHJ9fEWbdLt/4UECSBYAt5/R2Cux+wuPkOcnmb/rztW3i338O7ugf5S0B6ercgiFNLNKMF" +
            "2HaB5EwA0AIBZ2Miapj/Au2w6+HgY2nZQtHpu4r81Nl99fhRvWvQVFHY0HUiaOK1vi6UhIDECMBwb19/dgGgELbxuwJKk1ZrYYDy" +
            "jH9R6ZrOKj1rxNGjqfiuGHtl77dfNuWnhVptEEHAnR9n7iqqsUsWTg3bpxXbCCMt+Bd3+ZnBrE/FhIrTSVBzgOz4YMZYyBhuI7xJ" +
            "H3J9B59jCH8JCtaAtwCkBAsBhgTCCIqfAKW/FyKxJOjOxfb2azaxWPNPFbPfVgDor7B201YdE3Iq2sRZYu4oP2QZ3tAylnEp0yRg" +
            "XIAkFCRosYW8/h6Ld/+K5ft/gVzdwVvfQa7uIJfGDWix1UIAmcPDZkvACageGXNvUubeaPpZK0S0YoRzloBE8tMkUZioS8lUmOCm" +
            "0qQt8lHMbqZAyDZKfU3Qo+KE3BRsjDnoTQSONQFGDFCnWcy6fFp5sndHVJFRTpvfIa7KeIcgiCZVNzcH65fo5VBbjhMKOoSwMGQ9" +
            "d7JUWEJm2bfBWAI4EQJszVhDQjNGA0kfcnUFEhJysQVLCRYECICJEMcEcQiB52foDeeV9QPSxd32YvCCINArX5U3XTDBidig2EXs" +
            "AywZBDYmF2bKtFgJh0RGCBMC5Hkg3wd5PsT6Ht7ND/Df/jMW3/+vmQCwuoPwl/q0YCGzdQCzADA+cvuT63GQUyWIsQYopQ/mS3cK" +
            "QioUVrdQDW1PYTztm37uuykUpDsmKgJk6FitfZWKl6gDx5qABlNnpd2t4Zvq2xIGr4zW2v8+cY3UdLXajLrk60ShHml3Rev8DIe6" +
            "/nnpwaV2OUcx03Zm08kvNi5BmTWAWYCMFoxSf5XXN/C/WhDpE2T9JQQB3uYeUDGEF0AsriCCG8jlNeTyCvHLZ6j9M3j/BD68gKEA" +
            "0ms7qGRvdZwIbZq2q32vtHnUK2UOmqHLJPwAJAPA04d6AQIEfcKv3tlJ/yXPhwxWEIsV5GKFxdu/YPn+rwju/gRv+xZisYUI1iDP" +
            "NwKArfn/Guvv3Kim5MwtRCs52FhDoWIgjsBxBDZjYWIByFvTKI0hZ7wruhbUZeCVNHG1UrPqwLLChx3SGBS1E/L5ZuucLZ3zqVbm" +
            "oEuF2CbKM9DUKTXnWBOQxtpboK560MYylyXfv1CV3xpLoXtIr0mxcQ5oIWVUHhLlSLcYtEkgsIQyMg961d1QxFoSEl0RV9VHu6Bp" +
            "+BYM9rnG9UpBpCkDdjmMWRfgTABIFgarGMwKxApaWOdUAGAgW1Q8CwSjgkgA0ocggIWEt2EIL4Bc38LbvIVc3kAuryBXVwg//4rw" +
            "8+8I8RviYwhCDCA2f5MIAd3eZtB1rPto26Ilvj/3cEK6traCcdW3FtMHIi2ALdeQy7XewlV4+Z80f/0FvNU15Ooa3uoKwe2PCO7/" +
            "CcHdT/C270DeAsJb5A8GS34zRganQxosFyDN+EdgFYJVZMZCLggClCOJvB6K25N+R6btklSRLxI53znzV1zcWBmwit04cRxpFASq" +
            "Um4TcfHbmtyyVRVc/rL4XaUsSRUcToGW2tDKqaXv813dFqGd4xtKAEhgW8q7Wg/S+9qaT5fqNWemKfEEVQy8UyVsSxdcfuyKs4nx" +
            "5eZgZ8NQUr8rqFV1pcUrFxiZeydZQQKcmr6jkiUg8YUlM4jNzP+ZQaQPDhMS5DHIWwCrG7CKEF8/GQFgC7ne4LBYARCI93swPQII" +
            "jSKCUdqGrYFZ6WoNyH34WkijKa9ODRtB+D681Rre9gZisQHJAMLT1oHs2ocI1vA3b+Bt38Bbv4F/9R7e1Xf67+YeIKHblUSWQCk/" +
            "p0zVM9ywG9ash2I2myLYgoDZKIE5P51SFen0mBAvrS3vjHp9bVnZ2SpgTZzZct7eaGyPrhxMnQaxvmBc+NsZxSQGGBb68m99vqu2" +
            "BJyKKYyPBSksN2ZQ1QAPOO3vlWreduk3vaTezf4NolCvQwzEk6j5lATMRJib/DJTOEiAOG8JyGF2DxoZWjOcDiFCArwAwCAvAMcH" +
            "MJQ5W0CAI0AdY3DM4OMzOHwBH58Bdcw0nmCQOVY4pUWHJukUBfqpGGyEKkTSL++JRDRbnAAAIABJREFUAodSS4BcbOCtbiE3NxDB" +
            "BnKxgQg2IC+AkIH+G6zgre/grW+N//+t9v9fXunFv51yNI/Zp8No822f/1hBJcx++AJ1fAYfnxDvn6AOO739rooTk0ExtmH6wqsT" +
            "BAbAucs8WvfJIuYuuaxRWjfpxKdEA12r1bEmII2pf5kGatiTJTQbZFkWnIx/w31FnM5nVRTR6Nzrbr7S05bWgEmg0jrRMZO19Yrs" +
            "dOseVDul6ko0WokGTEUhVGT8YWNtDiejrew4xM0YEwRjlpEQwQbe+o1+GBM4JoA9CG+J6OmD/j0qqCODEOs2tBiaqjPg+k40TlN1" +
            "j3iSuIbsL87yVPkylBI2B34RQcgAwl9DLG/grd9Cbu7grd/A29zlLQH+AnKxNUKC/iuCNUj6dTlqKMGkRpBXgsz/Qu+AZhQdxvdf" +
            "xRFUHEMdn6EOj1CHR0TPHxHtn6DCfbYugFqQCXoyaB2Z4jpccpzuRJ3syKsz8wQeff7p37e65oxdNymJtstD65zW8TKOYH3Q5dtB" +
            "3YHMdydjzCHVFgCodFEK7XjG1a8a4yu/IyvKujRLBug6i/TUrNVWfqrMiK2y2lCvVbxDHcrpnlppQwyLWjOmVAyKI6g4hDICAMcR" +
            "WEi9LsAaSGZh4JIwGnzSVgERbOBBQPhrgHwAHkguQMEahz9WYDCi/Qs4POrPWUEkE04N+TV16y400JVeCp4X50HVRGkOydOu+pQK" +
            "AdLfQC5v4G3ewb/+Dv7N9/Cvv9cCgOdrIUD6+tAvucgsBNIHCb+QWJdBdBYEeiE9OS/WCo74CI6OUFGIOAqhwiPU4Qnx4QFq/4Dw" +
            "6RPi/SPU8ZC5RxqraRNR9tbUDiRR9JmbLoVSXisrL1mEPCrHVsjV8CgLAJaqsmPDlW1TLlChcmtDtghV/W2b7xzuQC2PC3ZhhHYa" +
            "q+kTrTEh+6UXnIZwi8XOExR65iG5aBllp+lmanPTxPIzvACQxNFzqLc+Y1bgOLEEhFDRESo6gKMDmAhMEhCRoWEB7acp+ptDZpyE" +
            "VEwXEsLXi1Ml32pmUwQgfwWx2IChEB9egIeP4OMOFJMW6CjOeBl7IrJpAqVHrWF/M+aEMnhkpe5E5v/k1F+ASOiFwcFaL8revIV/" +
            "8xOCN3/G4s2fDbPvm8XBescg3XFEGic50hivUN862LJ8sVZsRAdwuIMK94jDI+LwCBUeEO8fEe++IN5/Rvj8EdH+EXG4B8ex1kQz" +
            "tz44fUouG18TzlOvw/evWmeysj/QRYmnb+nbfGeEgCUABcmC9WbXSCytObe7vk3Q1RzVCcleeASATztyIplekricrT61UeQrmXsu" +
            "UYypVZ1thoMyE+ORED19wOGPv4FkgGj3qA+o8pYQ/hIiWEEGa4hgBRGsIbwFhK9/eqvEGedAjpaIjGCmQN4CcnkF/yoCiMBRCBIe" +
            "xHKL6OE38P4LePcFav8FCA/g5BeHzjSAHkOkw/p4UUGgdyS2Gdcw8ST0Vp7+GnJ1A3/7Fv7VO8j1LeRiA/IW2tVHSIBsAYDSOKkk" +
            "OCdjf1VGXXPDlEaSiYM5t9A3fPiA8PF3hA+/I3r+jDg8QIVHxMcDlFk/o45PiPcPOP7xM6KXz6Z/NOld3WqeS7vmDJ5+60jHKvnU" +
            "ZtJmsH3RplqmxvcNCA94A+AAAIhJOd2BGNU8cRNGFQBO/7AAyv5PVXKWmmEgQjg1mtNdgy7YaWvyemquJq2/62LtYaX3xQ73UKwQ" +
            "Pf6Og/ChoiOOD7+B/AWEpwUBb3UNb/MG/lb7P3tLvf1hsiXijPNBN3FmQSQICG+RHiwmfL11pVxu4V+9RfjwK6KH/0L05RdEj79C" +
            "vTxA7R6BWO+LrjWmKFkD6ui8uPtfVVgqhaV6X6RcGds97Y7mHpydCKuFABZSu/MEa71F69U7+Ffv4K1v9Um/3gJkdvvJ9vwvCgAu" +
            "NJXndTE9UwIzG7//IzgKcXz4Hbvf/h27X/8njp9/1ULA8QB13IPjIxAf9GL7cIf45RPil8+ACvWuaGaezjYKbZG+49k5ebxR0m80" +
            "E45dwvHddoZCa9fjHjwvIadfGBRj1XBhYbCoj79jpQwhANRNLzk3nh41Y00FaQTl4hUEgRMxJF2Uit1UWbab06Wl94rkXfXTJpen" +
            "1OvYNZEMClyTUG7MYTaaLgWoEOHTB8TREceXTxDLLUguzV7mS/jbewR3PyK4/RGL+AhWCiQ98GIzYolmVEEz19bI4geQQmhXlfW1" +
            "EQDuEbz5CeGXX3D4cINDEICF0gu+4xjxfmfHlqPP2v7RMEzlJpFc2ERwIXDlNsZZSuS8O4MIn26sYAQAkiDywEJv/SlWN/Cv3sG/" +
            "eqt3/Vls9HoAyg78olzBv2L13pRhtj5W0REq3OP48AEvv/47Hv/237H//W9Qx722Ahz2INZnahBigCOQOgLxEVAhhnLLBb4iC4Ez" +
            "onOWbNpWgc4WgA4o6lTKD4dLZ8garlMVjrveY0DYDNRprkBfO15nKZvadYhSjTZ0Uf6yJLQ5wQArIGawIvD+CSoKgcOzcW8IUl/z" +
            "eP8EgEFCQhoLgVpe6VM2Z1wEKetOAJHxQwcAXumdahZreJtbyGAJIAarI1S8QxQr8GEPJR9SyytlsaWoY9Nb5S8RSs3XNlOfnkDt" +
            "iLm4PUHxlBVbmdK+L3XVKtmqGwHt4qMXXgt/pU/8XWwh/JVeACw8SwCYMQUwzP7/cQSOQkT7Rxwf/8Dh439i9/vf9cLfcA91PIDA" +
            "IGIIwSAChGAIAsjcJzHaV6+1ncfJ+2utjRHAhb8DoqmWp9wKhd2BVN1ck0PpZZeK7RB2CPfRprgzY+L4g8iQ8b8SGa0AixWemNJg" +
            "8Oy4GrrKzEj59BOLAABAKeMectQnZVIMEiFI+FD+Cmr3CN4/gg9PQLgD4qPed3vGxZCnpazRSUgIbwEAEMsr7cp1/QAV7oDDEer5" +
            "CRB/6NODGQAxirs1F7UzbWk2b6VwkqDhswtvrO6a/MeF77KwZYa7Xf7coyLnIrAsAaztwARhNP36b3KdRJdtYzjlafgbAwOAOROA" +
            "Y3MgYrYNMjgCWDP5BEZiyEk9uVIa5PR6CI3lRd2EcoJ5Y7DaJ0MxGY0eRmfHxBiGlphO/bnhAR8BaNcB2x2oqqrPxfwn6NzsJ9V4" +
            "osWiyr26+6BYhlP76HjdIK/pGw8FQaDlF04MnNX22voTE6m4zym3kswoBeZIb9wVxwBCgCSYJNhfgneP4N0TeP8EPu7MRDoLAZdG" +
            "iXUn1pppDxBCQq6O8DZ3UOFOL5J8fkT8+Q+Q8MEsQOZU6EQYqI5Zo9QVXHTlfG8JBobrzgkwuQSq84GUKXNlpgpl7qeUgpZeTPI2" +
            "J6gX+RKktgpAc4uUcYpZhHTqqDtjSHB6QJg5ET0RAGJ9T1BaALaYfy0MkG5LgnnBRjZ0HWzUggjr6JUykh+NcjpGnFFxw4dtJZq6" +
            "7lxKcwp4nYLAlGEsAXsAAYC8C8GgVX2OdjuRPjIJaDiyJ+vvkILAuDhHRxsojRGymmlEh423MjHnPSNZRMBxDBCDY4WEAWKjDVXe" +
            "Emr3AGUsARzugSgE1DxQTg0EAoQECQlmhlzG4M0brQklIP78B8TiZ0AEuu1TOZmNrrO+TduQa5W2P+WFUs1q8iALk7zLxWOD865F" +
            "zZnIRZx+V7IiJPXAGd1T8oPQAkBiBUBBbZxsIdl2H8kZZwDrH6v8iehxCFYhwFoISJl/IwCksl1OxqubABomhzp6oML1hIbT3ryD" +
            "s3ONnOYomFiD1GDCZJSi+rAwg76ZHquwSUWOUaHVRG53gVG8XZsjGqKwk6PCqQkCWUSNhzt3QCW91lgEco8piYPTCFPmiJX5xdpt" +
            "yD5AZ8b0kPjck3YNIhlA+EvIYAvy14C3BIsFlAgAqV0hWCZtWSEGFGca13WRuNoOTkXNRe6ynJvSuuJKMiRnmGI/0feJtSCx0JLe" +
            "/Uou9EnB3gJSBiDhg4QHIonESmBCV2VixqXADEDphejMZgzj7GfGsETlYX1YjCh31amlL83Z9k67y4eXLuSEMBbz2KF6J8eCobgw" +
            "mJlPOiwMwxewyqp1/orslqKLLux8d+6W1scnlX9yVDigIDAIKHdVxex0RaMAUHqX5+YyEVRrNDMXkWSy1JNq7mCPGdMEkdneMGFm" +
            "lyB/BfgrsLeCkgvEcoF03WtJCHC0b6K/oey+ksSoOA7VeFRzYa9oKxeJJFrFZhQFguyWylc5QSC/4SMltpCM3PXpv95CCwDeUgsD" +
            "0k8PA0tOEp4ZoKmCjQCgjFuQJQSkWg4dDsjsn8U48tdVs24xbEUUdvCqaC6OPhkZSRCwou3N1/RKdAC0zGiJO+nI8LeK88Jw7A4k" +
            "kOgSu2JKBeuDofKfb/y8XuukTlIQBIqxd4qnCwpp9orDiqeMfLeYEh1lmsjhckWFgbNVYJMbKnyYLJ5LtWmpIDBjikgpicjsXe+D" +
            "vAVEsAb5a7C3hJJLKLEASX3GFadKbc4z5SWtP6V/7d00y4GR5AKpL7X1Oh1buHDBhuVn+3t9n3n0JGGrR8E0BSJtFHFoerMeZ/z+" +
            "E+bf/BXSh5BaANBCwAJC+BDCS88FSOphxpRACRlDU4/mNtiyBhBnVoAMTQJAQkcnsF6TppVTMzewIMDW34JRb7xqHGFea8ystQea" +
            "23iZ6V0KP2cAlyJ3UF6qOyZ/mpBTEhuJx6lti17UPRLR9o62ZxdNNM92HvqgNu/Zy6lJyoPmp6nuatw57NvszAF7NJ5Src1oBAmQ" +
            "9CD8BTjeQCyuINd3kFf3kIcdpASkAKRMdj+xtKQFOkkEC5i/OZmgkug4DVQMQbB4f3ORMG7Ji5xA4DQFuC4zQcVOq0zLyRsCQ+g7" +
            "hmESjRAQLCD9BeRyA299A7HY6O1zhQeQhMO7dcZkkCguNPOf0DdZc0AiCBDlqAdV49xJDOglSGVcjvl8CZ5VEBgTRQtlcTI+YX6t" +
            "EgRG4aW6IS8EEBGU0r7FCUbmK9ppQwsBz0Jhp5Kyq2QVcdZWAlfeOj8zddVcr3X5uxQzOWVBoHphJtXc1b1ylpGRNxXkNKz2IMU4" +
            "2bI04yJI2p1IaLcWfwkAWNy+R/zDX0FQCO9/gBBaCBDC3gKxYA1IIgTyViOnO1AiJBSZKvdVquUH8tYlLrglpSe3OmDItpyiK+Wi" +
            "djfRqYn0USIwkJRaEPACiGCJxbt/RnD7PeTqCsILAOlpM0oync8CwURQMaobCwAV1gMkKNFI5+l5SrOJhR5sxkmUbNwQx6uObGTo" +
            "ks/JtI5lOSV7gm4zd/dMrn083VMt2tPycQGAcKwJENnnQzdNbWxUHShRHNUeZmlLWJTNS11KUA5rzWClyCxxrvhVF6a+f+aqg9YW" +
            "vCmiivdDzaEtzG+1QXMaw3oMPbCcxHJXfFome7LIjjLGh+xQrcWOGROFblOCkL6+FxLB7XcgAvzVBvHLA4TZ6EZYs1Hmj1/QmhNZ" +
            "1JGp9t29qTx2OWk7tx7AYvtzA5xbNHbSJNW+zcWZaOKSpaG5HDJAQmi3H+FBeD7k1Tt41+8gl1cgLwCEp4UsALNP0NRgjXYJ84/M" +
            "7gNYY55lpbI3RhhMADgLaTQkUipPdX4Hye6Z3EXbNNNFmf9i5mxlisVLFpGbhaviMN9mdF2dbndBwOSihmfmXFj3e4JeAVx2B8qM" +
            "AIN2j9aFrKmR3GGWTZ/1ENXcBXaRck3EY1B1FwEguZiaKn0odCjXZKqAnJelIDmp3SI7hlbeFnc5LA0uM14ViAQgPUghwV6Axe17" +
            "+Ostlm++B0cH3b65WchmuBOLQMFEmtJIA2WUhIQK5Jj/9D9HfpzR1zx0p1x+mnCCdqmSe7P4lyQoWEH4a1CwAmRgnmffzZgWyuMY" +
            "QyDRQZapyrl03VbOVTbzxAWABLkyuGeu10jJdU0zibkZKE2kKXPfxGtWzet2fC0b7SRexflxc8IJq192B8qQeX5OprWq4WR+nZJB" +
            "39grxK6aVxfHZLjggVEnDU4c9doR91vXeJIdiGS/fY3TxDcM0vvcMzGIBUSwghAC7AeAilDFqbMRAPSN1Q8cqqnKIYBaUgvbf7Q/" +
            "PhdftgG5hmZq5uFyAkBBzWa9J+kDIjB/RfZuxkSRuP5oFC0BtjW7RtmKMg0ysoPDpjBHdDZbOHFxSh6Dz5lC8xTQmncvBciPtO3b" +
            "K/uuu0XAHbraBciNkiVAmWguii7E5ihvM99frfOvxpm0/8P7sFy8OUdBS0FgisV3O/UkeTWjLVXnPVMYGW0ohLUd4iwMvDYQzGnC" +
            "JMDSBxGBlZcxvTkDkTXE57aDdVBTC21/M6Vwaokqf9O9Z+UE2TR/5Hzv+ML6k3xlngmzBoAEWhV8xiSgHb60BUCQ/iUGJ2355EJ4" +
            "G1X059i2tmAw65bHU9FVEJjajGXQM1vOz85cxE6eKMllzQRcVGhkV11Z8Oy7/FW37/LPusFxTkB6SRcnxrY1UrHQpWSsdmjKSq8v" +
            "ibGqe6qWilNBDQUraJKmVnzX1KBJNGH1uMjqmPcEfTqr/mcfrTkvFX5lMDMNMYGF3tGGSYKEtTkDOS8rhOC2Kv4OWcwSbNGJLKqm" +
            "wuNch2yTSZdIUIzb9A/Spwbnz0uY+8L04GhTAog4896y3XxyX7ZQxFH+ERXjuBhJ1KpzasJ8ZbhAEdsmmRtSqniH0licZ+DJRbjl" +
            "aGvfdhcEToMlBIRI3IFsC/DFybKxRgqtcoK0etGyXryiv0JM0QzQBIuHqZTx2WaGMu1/KhDMftCvDInlR4BJ5LZEdAVNUbdTwlgk" +
            "UNufajiuYl6p8qYBVHOHmfZfBYwzI1m2SyIzdJmRL5E3jYBQiS5K9pk0XjH6TeTlr5qZgkoyoeJF9jcvoPZlPPoKAqeh4A4U5W9d" +
            "uZiUD3xTr7bVCj1hayXGHEQGaPWLN8fFUFN5ExYEnORUoUhNpHNbStfhjQCQaEKzaXXw/M4YGeYkYW5kZBM1lSvcyO1eSbQNabdi" +
            "zrvlfRYAXikIxmojtRuX9LO/0gNUDHAMso1h1pBWNZw7W98e/8eewzujbYYmlenTMKn5+Fz1ahd63DT7VK3DHSjtedW5nUQjdhX/" +
            "2xUn1y2LfXSigsBJzTFkeTqNzj3iqcXrEgSKDH4ue4WbxOxIhfepqS4VBKTZFpG+qnnjW8MJuqoL4fz5yRsSplYfM+pBqRBAIi8A" +
            "kPR048YMUJwFN18ysuauc/N3GprqltBcDPWLFSaTzSHRaT5mx9Wp9XKmWs2xni4KHZZf6cvi1O4O1DPO0ZAJ9O5GHDLDlcLAxASB" +
            "yQgASXynr1M5gWmfjiBQZyxrp7/NE1piEc/vzgIwJyoyYX40M0WvGbmtMGfM+BphxithLAHCN0KA+YFBHNvbleuBj9rpVFv1nclZ" +
            "BcqYePZOQ6v52C0AvErkGAJbOqhwkzwjH1i5MDhnQpsUqrtGl3pjKoev1sbOaI2La92rMzDFnUWdGizSmjIS2sWHyAORDxIeQD5A" +
            "HsiY0v3r9/Cv38Hb3kOubyGWW5C30O9nzJgxY2IgIpDwQDIAeTHk8gre9g2C2x+gjjuowyPU/hFqrwCOkUkD2a4/OV7Kxitg7mcY" +
            "nMArdHGkGmLKP0kvOUhE46F8WFiGyXWlpP5OqcfSkgarlK0LPOZAMzECAU4o6il1NEJnOWfV9l0WpCdIAfJ8M0n6IG+tD0JK/y4h" +
            "vCXIW8K/uod/+z2Cux8Q3H0P7+otxHKrfWtnzJgxY1Jg6HMdPAhegEDwr99i+e6fwVEIuVwj/Pwzjl/+CyEfwdExXSOQ6igT9yAz" +
            "R5TG2jbz8+S4G8DO+CSzd1aw9X91iKZ66jIPO/XeNtNZgZLrGRUftoRN1A15PAXF4rwqd6BTkZCVroRCKw3qt96m6qbZzQfJlaNq" +
            "T5Kip0KJQ/mTOsqjdzc0O2QIAeEFEMESwl9BLG/M7xZieQO52EIGW/13cwf/6h7e9Vt4V/eQy2uIxQYkvOm5v86YMWNGIgQQgaWE" +
            "f/UOHIUg6UGuNtgHARSHiPYfwYiBmMGx0gaBVADQoxoX/SMTDLU+7cxwb4M68UwPim4uQP3muDYbGSTBKPXFrdv+s0cmar5zMz6n" +
            "sEF1cky1O9BXBsfxIaXbYQrf2iGpnIELY4zc9HRxK0cyQOOcFE3Rb6xnZVXvdpdsm0cQQkB4PmSwglhuIDe3kJv3kJt3kJu38Na3" +
            "+re6hVzfQK6vIVf6R94C5AWzJWDGjBmTBJEAhHYJAgL4V29B0oO3voZcX4HVAeHuI+jz34H4oH04VWy+1f8lRwMljFkrzmVa020J" +
            "1dmbHq8wDoZnP8tzfpeJvAP3MpgAMDxsCwflnhIA5XYHIqg0dOWpaZeCwzyTCGy1A0HfxZK2/1FjFFOqqOnh0qR08dbhMhml5MUE" +
            "BQKxAGgJubiFvHoH//od5NU7eNv3kNt3kOt7eEtjBVhsIRYbiMUaYrEG+Quz24YsGPVmzJgxY0JIF2gRSPqQwRpghh8dsHj/VzAr" +
            "iGCJ6OkT4pcHxC8PUPsncHwEqyM4PgIqApQy7kJ63UAblu714fXm/NIozfm5hYF2vZLjWZXzeZEpvDhncRIq3IHEtItm1T+3XR1S" +
            "/6DmqYVaQaBdbeXJ7wTT3yuwGrqycx7XoIn5EXH5Nj/UaCdCZgkoCdAStLiDvP4R/tt/gn/9HbztO3hX7yDXbyD8BYS3hPCXIC+A" +
            "8LX2n2RgBIBsUfDESGLGjBkzNAxDRtKHCFaAEAAYrGLIYA3/+j3CL7/i+OV3HL/8ivDxA9ThSS8cPjyCwz1A5mwjZu1KUzEvvt5x" +
            "8JXk/DX5ntYoyMrkU7l1TM2zpvS7fzImSu5AwhGopcvS6Cgu3EiTtzjLsuTX+KB7JkpRdBMAKqNpeNOY3MhWw6GirhMEGkmrkxQx" +
            "UVE2masoW5vCrHcCYpYQtAQWd5BXPyF4+7/Av/ke/tVbeFfv4K3vQEKChNQMv5CAEGa3oGyL0NkSMGPGjMnDrA8gIq3Q8BYQwRrB" +
            "zXdYvv9XHD7+jP2Hv4GWfwf8DaLnD8CzB6VifbBeBEAp47Fg/LftIX8WAM6L1+a5lC7GtW6/MVQ4DquElyhbPIoWlLF5rIr4OymE" +
            "m55Thzj75aRHnCe6Lw2MoaN0kU7rNCbK2zeitO2A6WREILkAxAokV5Dbd/Cuv4d3+xP8uz/Dv/kO/vYN/O095Ooa2cz2LQ5ZM2bM" +
            "+JqQKjQAwFtALNaAegPmGCLYAjIAm7ME5HKDMFiCPB/xy2eowxN4/wilYmjtChtBIFnMOWOyOMMc3swqkPPyW0LZHUikRw610KqP" +
            "jDMzelS8GTj9KlnKjQpuvpAnZzZdn460rmVIjOlhN2ScDD3P2MJjWy+xdJc7okyjLyXk+l7/Nm/h3/6A5fu/YnH/J/hX7+CtbiCC" +
            "9bzYd8aMGd8ACCDtkyAWG/hXb8FKQfgrRNdvET9/h+jlD4QPvyH8/F8IP/8CFUd6bYBS+qCx5GwBNv9NYYKrQW+vgNeK16jEOyOG" +
            "rJ6mJa0VXEXmFJRYA7LV+Gdqv06JFHPl0jGb3J+jT9XkvVvy7UQG19u6RdKDVMGpkVRaeFpSWAeXtLakVLAi177nwoOmbwGrTRhg" +
            "Iu3CIwMIz9eH5bz5M/w3f0Hw5k9Y3P2ExZuf4F2/hVxstP+/9Gv9GWfMmDHjVSO1cAoQSK8NuHoL4S3hbW4R794j3n1BvP+Cw8f/" +
            "wF4GUOERx+cvIIQghEYQILAZlRmU8jBpGsyOMbsFf+DUuJ2G+iTPLQhYPEebSe2UJIYK25C3Slah4NXSr4htOeJ27TgWb1104LGR" +
            "EwJkskWoUFnoS0hsnQWA5G+1szyBSouIS8ryIQh9IhJuVW0M6dePijRaR1DZKzsQXkPQvvkrL949AYVFLMxmLYDwQNIH+Qt4V/cI" +
            "3v4Zqx//Nyzu/wJvew//6i387T2E55cW/H61GqIZM2Z8wzDjGmn/frHYwPeX8NZ3COIQ6vgMdXxCfHiGXF1BhSGOj58A+Z/mezaW" +
            "AEudpAfc/JheOD6e0VK/UppvTmOS2o3i5xIE2q1aHCyJocK2zGplSzXwhfUx1sZsYDMATXr588NhCRD6R6Bk0eLZtwkdIL26z213" +
            "arvj10lLJyd6ARSrcTABoFBn4xS7ZcxjDCpWcKM06vRRKkwmq3/ttFN3IA/krSAWW8jVFt7mLfyr9/Cvv4N/8x7e8lpbALxAuwyR" +
            "mBf7zpgx46sHEZkji8zp6STAwgM8HyTMQWP+EvH+EcHdT1g8fUJ8eIZ6+QLefYHafQGrnRm4Fch2DbIWDndzzx0eX+1oXsXjjjhX" +
            "nxfjtNylipwTAuKUyyAQU2FpTVbwHHtm3QzKEBYiq42X8++rwrbJH7UJyKWL3hhbJhxC7swJR46ILiYINBGFHUPPDJYEgAaapNwL" +
            "MrRJeTMTAxA+hLfWJwCv3+hDwDb38DZv4K1uIYIVhLfQvrH6OGG4+uKMGTNmfG2wBQGA9T2E3k4UAAsBb/0Gi9sfoQ4vAIDw08+I" +
            "Pv2CMIqgwhhA8gP0oFt0aCaQk3OomDUr55DudnGqvJkQyGi0+uTvFIbgkgJAURM8YF64eDORdi+4A0krn2Wmoyg55wQB6AddukNj" +
            "WJNILYPfsZFc9U5wMbd1ziDDUEbRSDQWBok7tQsVkRcVy29OTriURjNTn3+ZnNLbOT+1AoAjtoThz6duBIDMx5JAIPIh/A3k8hZy" +
            "8w7e5q22Bmzu4a1vQZ4PIX29eDgRAGwT1owZM2Z8xUgFAUr4DwHIAEQSLH14mzsEdz8CYIhgib1cYB/FiJ4fAdpBKzOBdJEwkAkW" +
            "1Ue3o2S2tcbuyrym/w/AkAyOE+wddMK3pfRbxDMFC8DYni/nYvxawhICfPM30TwaloOqO0BdXTXVY+uyt20QV5iaSnbxU+ntGQjR" +
            "xV9W18lEqMWJ6gYari8VNDY9zYqd8tNF7qsS8XNWADJdylyLACLYQK7u4F99p4WA9RvI9S3k8lr3u9J+/1OlgRkzZswYHloQSG9A" +
            "JNIRh1X4AAAgAElEQVTtROXyBsF1BBISYrEGH0PEL084fvkI2u+AeA/EB1AUG6VNIgxozaI9tNbPC/Xzb+9R+awCQHJ97jmkw2Q9" +
            "BQGgFQasxwmwdhXuQAAIrTdqaYPCOpzWaP6maOLrn+ESL1dMYkCM5a/fCS3Klb2+JLVy1so9BQDHbevv2gsAQKUJmQkgvR82SIIW" +
            "W8j1Hfzr91i8+RHB9VvI1RbC81MBoNEPa8aMGTO+clTxDiQlhL8yZ6cwgtvvEe+eoMIjZLBE/PwR6uUT4pcYUPpkYb0+oMgzADW6" +
            "ThMAr4hJHQNDzf9V8ZxWuanBqIXFJg1SJoOuqWYRnFg9Q3i49C1GaWFwckZAprWsT6yT+0+XwKnrRE0FcfFBRTzkuG6DkfneblEP" +
            "YZZrjrbCG9Lcd6+QxEzqdJ9pzFv+m95MfKc028VF5l2l4Gj3Hzb3JAER6PUAgRYCguv3WN7/BP/qHt7qCsILkB4glkY1CwAzZsz4" +
            "dpEXBMysIjyIYAWPAJI+glstAIAB4QcIPy5w5BjR7hEgzfwTkx0FbNegulGWkQ3DehYcaT4uoD52dszIdDpvm0VfyMRQUtA40lR6" +
            "Bk9DuHRtuMvjm9ExexUeCl0MIEMp2rslm6Jii9A0zvSijXtP6ww0Be77ru6bFn31dbBaI0kmlnm0aeDpln4iBnQUBC6hdelhAUhJ" +
            "2RU2GVDSiUcCwge8JSjYwtvcwr95h8WbH+Ft38BbbfWWocItgM+YMWPGt4qSRUB46fkpIliBjwctAMgAJD1AxYh2D2D6FUBs2GNl" +
            "FJxmHqOEpXeZGsxfLhhlc4HG0xQ2z8OuMIx0V0cMNI1OwGVlMLjm+Kqy9eU1L8jm9GnzanegnLA0JQowrfZNm+YujaJ5pS3OYFMd" +
            "W1nh4vgdLkEMaF9VEQDkg+QCcnUDsbqBXN1gcf8nLN7+BcHdD/Cv7iGWVxDBSk9ek+pvM2bMmDEN2IIAEQHC02sE2INcX8OPQ4AI" +
            "zAoq3CM+7hAfd+D9Azh8AYcveq2AEQg0HLrzoitmtcn360HfaX1iaCOz1HqYjIkKw8El4bYE4MJsiNVC7orqKJrmglZ8y9kr2/Wl" +
            "pyPLyJiCaN63VloKAgSjQR9GrO6b27KprqCBKbgAsXUNGYCCrV4AvLhGcPsD/NvvEdz8gODND1iYn1zfQARLkLcACYEZM2bMmOFG" +
            "ziJAMKcDE4S/1OsDSADM4PgAQIGkQPTwO6KnD4iffke8iwGOjUtnnEQD9yLIgp/smTDU7N4525didqbHZA2DxI+skk+4PByHhaWE" +
            "Q7kHqfb9TNlvlEh7+PjkODf3t/ngUxQAEpzHJ3EctFQ3pHTXEK4L828/aKi6crScPnT5YiZ/2bj/kPQhFlt46zfwtm+x/P5fsXz/" +
            "V6y++1f4N+/gra8h11eQqyuQ9LQVgGYhYMaMGTPqoAWBhEFnEIR2DSKRWlQZSi8eXixx+PB3kJRQ4R44vACstx8lzqwBVV4PtZrR" +
            "ln7og6BrIoW8TZKXmWSmxsXUilzpDpSyXrYUc2ZpOE2+ryBQetVCcDAWgdfDWk/BKjAy+m4t1YSaqivKu5aRrIL5T671ol4GgbwF" +
            "xHILub2Hf/cjlu//is2f/xs2//Tf4F/dA8KcApzT/n/lbTljxowZAyCbFrT5XvhLwF9CEiAWK5AUEMECcrUFCQEV7hE+fwJePgMx" +
            "AYoBik0k1axyLduTLCsYG32mBYeH09QY0G8NU6x/IwQsAShIFnlOZwJUU598C4a+LshXwT8PU4BJV8MYAkApjfx1jvQLpjykW3gK" +
            "cHKuBrR/qvSXIH8B4S/hXb1DcPsTgpsfEdz9hOD+T/C2dxB+gPQU4PkcgBkzZszohexk4eJzAeGv4K1uAFaI9y9Q4REEgrfcQu0f" +
            "oA6PUPtHcHQAq8j8YiTbiJLRCDJP3SvABpVvW2d8AgzfmeAs5WDT7+uqQw94A+CQPTFKSS5tVN4d45qhasx29jPbz7+r4aAvRqOB" +
            "k5ukKtbp4NyrdYrRc/aXYC1CSzyBCGASICHBJPTCNJIAeVrzv7qGt9Y/7fv/Jyzu/qSFgJv38LZ3xvc/kbIzN6IZM2bMmNENRUGA" +
            "mQEhIYIVJCuQ9MFRBCKC8Bfw1jcIH39HZH7x4RkI9+DoAER6HYFeKxDreYCMIMAVLgI1bkJZgJp5rNfQ33FNZCdBoIjXwNRWu0f3" +
            "zv2rWVBxGhzuQFqrOaQDxinypfvban/+gkI3vWAH/0xWoMFk4DEFgCT+AfnFSbGe5xYAWiRXpgsyWnwJSA8QPlj4APmgYKNdf67f" +
            "Irh5h8XdT1jc/wWL+3/C4u4nyOUGcrHOLAFJAtNqhRkzZsx4VSAyuvpkPjdCAEkfvNgYy0AAubxCuLnD4Y9/4OAFYMVg8qBI78rG" +
            "SoHMouFkS1HbN5Tbzr+pBtRW8lRMMOfASQzOa7IQnMgg9faAeS31U0bl7kDTxUC9xqIV4i5NeIkqGmekmDbrOQFStKwCbH4kBUgu" +
            "QMFSTzL+CuStQf4KYnmNxc07BOa3uPsRi7ufjBXgO7P419fCw3wA2IwZM2aMAxIgGYCED3gLgBVI6MXDMljp58IDkQ/5/Anx4RHx" +
            "4Qnq8AQkW4lGL0B8BKtY7yaq4mwdQuXwXcdBOpz0z4ium+3lMYIg0GVDj+GTmGHg3B2oGq+9SvOuFxaPN2E4Ropqy9cpsQ4StjWc" +
            "8mYyuk6sRRIhQAGQPmSwhdzewtveQW7u9O4/6zt4mzt46xv4mxv9d/sG3vYecrkFeT5ISJAQoImLXzNmzJjx2kAwFn+L29WKeC0Q" +
            "iMUaHvR6ARBBBCv42zeIdw9Qh2fExxeo/SOip18RPf6G6OlXqP0jEIUAhwDHqUtoPlVnTpC677qms05TwOnzxcRm1NEFgC5KXcrN" +
            "yOdYKNqGzzlfi3nARwAbAEBMiqBURdDLkNHwqfZt4NNzYsfQLhcnLnzuF2vncJ3QaGya3HCVCgCsACIPtNjC275D8OZ7BLc/ILj7" +
            "EYvbH+Bv30IsVpCB/olgDRGsIRcbbQWgZBHxjBkzZswYGokgoOdGSg4EBnmBdnSWPshfQgRreJs3WNz9qJn/4wvUcYd4/4DDh/+B" +
            "/e8elNpDxaGOK1ZgDrNELAbfPa+T62GP0pyO4WbUgVZ5nsEC0A1FRqqGseqtoyxqbqchAACpJWAPIMi7A/HZ8+LENPTCw+egmX9v" +
            "OQB0FAQuqoO+fEP2AkMLAsoIASLYwrt6i+D+z1i+/xes3/8LVu/+Rbv8CAkSZqEwJesHzO5Bs///jBkzZowEzS3oYdYsFjZul0L6" +
            "gPTBWEOyArZvgDjWOwJFB6hwDxXuEO8eIAIfKt4jfPkAHB6BOAYotBS41gLDAoMynB55agKAjRO4sokJAJbNCI2CQMPyjmoUV6rW" +
            "te35a6HCHcim6stybqXULUtb8fmQWS0l0aZ3d0z/HIYnG5dhP+1SjjACdPmuaxqG1si+AZBtEUqa6fcCCN+sETBMPxnGn9NvZ8yY" +
            "MWPGuZDbNYiSUdxSzJAEsQ8WEiQ8kOdrN6HFFuSvABEAZHZ/MxumlFCYU6Y00g/KuQ3BrExMACiDJ+uqW1s/J1Ze5ZoABlMa+0Ss" +
            "AikqBIEhrAYllrUlDztEeuU3qHzbFX1iGSoHtevNT6nTMQWAAvSmAckQwdArxWLzF+mkkngYTqm7zJgxY8a3gTwXUHWOAFJhgEGG" +
            "DRKC9DkB/hKQAVj4YPLAkGAIPVcbN6P8+l7uwCM3MRPTtQCk8pT1/9Bu0pfGyXkh69eUUIumbsxPiba700/zicHtsnI5JBXOw+Vy" +
            "ZL6/Nr2ub13oO4xUlfkUJUCtdWbyAoApNXGucxOSRQJxGrk2QgvL5WeaGoUZM2bM+LqRn8mowLRnQTTjwEKv1yKWYFYQ3hIkF0Ai" +
            "ACQHOxrlTnlk76P2cakspysAFOPPr33oU3Yd04Q5y+5o23xs/a35prsA0CJSB9xbhFatDb4AupBYl7CuqrokQQ4lCAwxjAy1DmO0" +
            "+jyHAJBMHNZkkbH2xjTGlonMPv133v5zxowZMy4M24UThbkgW6OldZ8CYAnhxYAXAMIDCw9sXIHYmlnzTiPTYmOnlRsX8vU4tRxT" +
            "4W/nD8+FRv+g9hmq3KqEz1+sVmheUlHPSle/ZG3q+1ZhKUuc8mWP9Q61FX6Guu4sK9h2sAIoNzwYn3+R+YpS5fAxyW40Y8aMGd8W" +
            "KofixMRL4ETjT0JfW8/cFt5TxnequB4mxtIMOPCayVMivQir1SLRwfLFFddDpD6wd3WNOxDzVIw1za45maaWrXsXrA0Dyu/S/+s7" +
            "pB3z+Vi8eh/C3vkofFjLt3dendK8kGoMnORlZCwAtrkz8fnM3IMSQUCCSCLd/WcWAGbMmDFjQnAsHCyCs5lfCwCG8U8EAVBqCaAk" +
            "YMmNwPAc1JU/GH6OSKzVaT46TYjtVwJOhT90ohWvUl9Gy9O8fx5aNe8wXH3ffNZvWj7hNs7QUgPLzkt0DeTyl780hhIA+qOqFs5f" +
            "O6elWKahsohFIGS7S4CEuW+Ka8aMGTNmTA6EVOtv/7QwkAgCKKw7tJQ+nPEgF95M0cBSWfUSANp8OImCunGC5t01a7edyVOK6F3n" +
            "7V+dELSEnBCg1wQoAAqciL7ToOqWqBIEKgQCTr27C1Lz8GVuirE/y/iK2qeVIDYVOGiGs3dktgEla9FY5bczZsyYMWOicPAJyRbQ" +
            "FeEr568OlvWxkXdxbYNvfQ7rv6tf79qaACPkdgdigLiDO9ClC1KRfuYkVAycLe9s7ShW08qnFv/U7tbegDdgol0xsgCQj7OjEa9B" +
            "GGfWdMTgnNaHqGLv6BkzZsyY8WpguxInK7wIMAc+Us4akBoAigsna92Mm9MfDpkDaxp56+mweU1lZh3vOpNPZa7M8lHvYj4CnAsu" +
            "x0uuDZoPC2vCpQUAA5skm8mzyJHml7mQ82YcDBl9ZXYv2fdKa5OGX1HhbuvmlSR1r7O6dFATGUsAEmuAaz3AjBkzZsx4LbBHcUKy" +
            "a1DFTGImCEb1GsMuGJLVyK1fS3J9knN72VHoxJWIk0LOyA9g1HJVtUMvTe4wcLgDaZQOC5squPC363cV356jyGO0dynfE+qnkyOj" +
            "ugxV2FKzATYxF88CwIwZM2Z8LUh0OqkloID6eeyyc0Ft3uZpaoYDOSGgeFiYvpoc6+bEELl0xlFYPzAkhomvpmefrdNX2UHbmN06" +
            "omRZ6BC4R1KZK1A+toz1z+tdZsyYMWPG64YWALRjTZOmv68e8jywMs/zLJXDKd7uheUTnOMGTkgz4bpPJqb2ETgPC5smMbdHvlOe" +
            "bmjLLRoeuB81NjbZSXZIvDboibZBuD5vy/DbGevoGtRpXcFQVOyoq6SXMOfsAPMQO2PGjBlfEVoO6m53kolwUh2Y/05eKRMpXr+J" +
            "t4KZs7R9ZTeh+nRbHX7WEKQbA95EZ+0Y1trDwqbSxv1RlNH7G/K4dDEA2sSVWiCGFAA6xldEExEPHWnh9fgCQP0KLyq1yOvvKd8a" +
            "ZieuGecGVfxmTBDEIOITZ0vK/lyqoXsu7zxZo31OdHYF50Zuu782/BIN3YpzrYQlBIQ5d6BWX08JQ6+wrbsfI43asEMVbspTTk3e" +
            "OtX/mciWjZ7ADCj9NxebcW5MuRfMmDHjwiC9MDjdJjRl4nuMHFRxPQMXYTHrXC8uMoVfnigc7kAqF6BRhz6AZ8lJUdoub6fEUwxb" +
            "twaAy3x5p6bsVF/VvmZVuxiV6JwGJLWKvPcjgam6AFWAklQSQ6FCYjScRYDXgcsPuTO+Rcx095pAgNC7A5Eg6yAxpFsBZVMuWXfZ" +
            "LMBZ0FLUU50silkbN5sDx15ugtYgHrJ/9inXqURxWl0WtgiNKpOo9T4aSRCoTK/mG5cg0DWetLwOZj+Ny+UmjnpiqspbNepj0/lM" +
            "OdPqT7lmUGqLcwjQHdJINO/nH08zsZhZH6w32VH9G8PMaM2YGmaafD1IVDua59cCACXCAIq8RLYXf37NHqc8Q6VHtuvhRKaQ/qsZ" +
            "6jigfoWzvxptjcIJwsMwGUigM9Gt/ochmsrdgTrjkqPdQGm3ieZ8fZWcl8VcFHXQyUDGXLYIXNqraQyMTnbOBLjiN2PGjBkaswDw" +
            "epAfvbXWn4Q5A0ZQQYNWXCkwwOqwV00s49sPxppdi9Xevxla5rA2GDmuxkflOQFZVrr4krweSrbdnIZk46ri6BZ3Uz26HQ1LaRSj" +
            "6VvAifG34/nfW1Rh+1pZ67u0WSUGcwSoEMwxwKoY0YwZM75xTGzYnFEJo4FN1nlBgZjNuK43iKbEZ4SQXTtQftySJxqIWKaxenAc" +
            "oaB9TP1D1n45eofu7bx+ErzybSIXdCGD4f2BzjGAutKoNOGdGG8lSom1Tb1e+1Bl02nO21CSQw/U2MKMoXV0MZPsVEj7gyV7RXOq" +
            "EFKAisDqCFaRdgtK5Ochjo+c0QttKHVunRnnxCwIvBKwYfZTE7q5B6dKJ6K80wtX6EftVQL5J650BytBm9QGiaed3eO0HNQpU9uN" +
            "4adwcjXfVhbL1FZTsSsb5zICAFASAgwuzMj0KaJdtxNee5NHgwBQ5x+We0fudwNkCBepTaccMr4AkE8+cxZMmP/08BhW2hqgQkBF" +
            "SNcFzALACOjgJdmi/munhtNPaJkxY8arhGUFMGM8sREKEkGg4P1TN9q4LQJn8M8tpH9qEtVlHHesrOM6hhYE3F4gbVMZgj9q+/04" +
            "dW4JAX5+TYAAEKNFGafJcrfKdg2GsAh0g1sAOC2WLi8njDOQWD4Ja7WQWRTG4NQKwBxBRQfExx3i4zPiQ/YDCRAJ/V3h74yBYCZm" +
            "Zvfw3fSt/sJWGSSXSXsRXm9nmXF+MFgpgBVYJe6BhUVZlPzn8EJO3qVjhZjHjFGRtJfe3IFVCI5DcHyEOjwhev6EeP8EDndAfARU" +
            "jETRQ0ROxVs7TJNXeo0YxyLQhfG3/56Cy9ODY4tQyx2ICHSBo6ZP6SqV3zZyx2fe5pFON1gl8Xz94FqiGHVoTawQBNM1Yqhoj2j/" +
            "CPH4EWL1O2jxC+BtEUUKwl9AeAvz14eQPsj8ndEHDh0NJ+5YMaBi3XNThosdVllO21F/q1JhgFITjwBJH8ILAOmDhBy7YDNeOyya" +
            "4+gAdXyBCndQ4R7gWFsLWWvSiAQgpBY0c0wEaVoTEiQkhLcAeXoMoXnMGAXMCio+QkUHcHREtHtA/PIJ0csnRM9/4PDb/4vDh/8P" +
            "0cMHqN0T+LgDx5FpNgaBhuEWChOXQy1xElJhZfDJ8fKMK9BDEKiy9LL1h1G4Gcqm0h1dmu6UZna6A5HmdigbqrR2jLnVwciDYLB0" +
            "HIqXqlR6u+efkJ9cxz8hvVKxToStLR1LzrDb15UGF9umQRCoi78NyPERJwKIAKC0uViFe8S7Rxw9D7T4DfC3UHKJIIohl1t4yw28" +
            "5RVksAKCFQQJQJiuNmv3WqCh97MCxxE4OoDjUN8bX162RvGMRzPUzAxWkRYc4tjwYEb7LzzIxQoAQwpPfzO31YwGJAKoivaI9w+I" +
            "Xj5B7R+0dlmFgAo1HQkfJDxAJIy99jYnklr4lD5I+pCLLcRyCxJyFgJGgRknooOx3j7h+PArjp/+gcOnf+D46T8Rf/kF0ZdfED1+" +
            "gNo9gsMQiMNsVCoNCyWVQ4rGEcTBY57qhVAaOQfjYwfi/Ab0Uaqsq1IajkSLH7MjaLrWr0vGhuOQu8TYl2fOCQGpO5AASHvE5WOn" +
            "GmlqBDQViqxfq1w5G9LxdbHmh+IFRuIphhYANMrOMWOi3B9PV/t37hRcvKEsT8SWJeCAaP8EBQa8NZRcQcFHHEbwN3dQmztwrLTJ" +
            "mQTIC7rk4huHrSWt0twobb6PDkbrqjJ3jIThT8TY5Ij4xPQfh1BxaDR7iYZWGEsNa0bNXwGQsyAwowbGFc3sIsPhAfH+C6Kn3xE9" +
            "fwDHB3C8B+KDFjTlAiQDQARIZi0GgaSnLYfeAsJbglnBExLCX124fF8jkjFBQUVHxIdnhC+fcfj8C3a//ht2v/4/2P/2b8D+Mf1x" +
            "eDTjC7eaBHtzRw6rQJ+RZzzP7YkIAEkcdYJAhzRSh1LXN1y4rmyQ8VXjbVPokxMjBCyRnhRsbw6UqF9zsVYlQ0h2U7mUuaiUcsde" +
            "dEkjV9VOA62+RR//uOrSVvWHIdmhujSGSn/o9tSWAgXEIfi4hwIQPX4E4EOFMaLdI6LNHaLNHcLNLYKrd1jcfAcigvSD1BVg9jmv" +
            "QxUVZD4+6vCC8OF3RA+/IXr6CL1IOwYrZQ5wM2c6c/aNtgJon20VR+A4BHkBhL8E+UvIxRrBdQwiDzLYgqRn5WNuqxl5sBEsteCp" +
            "EB93CJ8+4/DpF4RffgaHe3C00z+Q1uoLHyR8sHG1BQgkA8hgDbFYQwZrBFGoaXCxBRaXLuXXBJvDVlChVuSET58RPv6B6OEDoi/6" +
            "R9EeFO6AyKwHSJQITfFWvG01elToIbvia1l10FiGJkGgYxrD1ll1KxBPr3084A2AAwBAskjzV8eT1vrEDUSF56io4n7zVQV2WYnO" +
            "iTZV6jZTVoaseVed2jnK35S780aU9VhCIt8yEEdgOmiGkz6B4xjx/hnR40dE62uE6xt46xvE938GsYIMluD1tfb7TX2DuxTmW0OF" +
            "6dbobeL9E8KP/4n9f/0bDr//TW/TqiKt3WdlQnH6CTOnTJuKFZRxBxLLLeT6GnJ1A+/qDQAJEWzgbSMQL/R+4GyUGyXMDfgaUDr6" +
            "piOK52dyRlRmfYn2+1fHF4RPH3H4+AsOH/4GFb6Ajy/g8EV/JDyApPklFnaC8BaQq2vI5TW85RXAAiLYgLf3KKmGzTczuoE5r8Vn" +
            "VojDA+LdE8KnT4iePiN6foDaPQL7F+3GFUcgxem4k2cYy/MkW/+X0nc8q9Ncn9rCr1kQOGWO76WuGU3HY1qBUfKg6ds+vTwbWpTN" +
            "4Q5kFi4RsV4Y7M6Evq7I1oluQ+cn4LoujMZ350Ct/n5QIrZbuPjslWCohnQMzgyV+qEjChFHEeLdMyD+APkryNWV/q2voKI9ZLCE" +
            "f3UPFYd6uzmpz+KgmYnsAGO0NbsBqf0jjn/8A7v/+L/w8vf/bu3uEWqXH2imP/3LeqxixWDFUEqb9+X2Dbzrd/Cv3yPY7yH8Nbzt" +
            "Gy1MQJ8XQZUj6WwhmDpOFQCSOBJBIBUAkv8tQSA+vCB8+oTDx1+w++1vUIcn8OEZ6vAMmMXBnLMEAiBABCt46zt4G/0T/gre9h4q" +
            "OlqZyGVoFgQ6IKWBtLtqi6AKj4h2TwgfPyF8/IT4+QvUyxN4/wKC2Q0osSoaDVuO5acs3mbuwZEvJFHk29Ldst3puGrEmjKGyF+r" +
            "UdnF4oyCGotA5ZvmGNHmW1tz3VDO/MJg14YYlbltKMaZ1w90BRX+ut+eES15iotI+RNvy7GRE36Z9Y4fKtas6XEPxaTnC+FBLDcQ" +
            "y7X+Kz0E23uEdz8g3n1ndv0I9A40wkNy+ACRdXD3PMFXIBEElHYH+vIr9r/+D7z8/f/MtvfLCQGJS5AlBOjPoZSe2L3r9/B3z4iP" +
            "RzAJ+Ff3iHdPUNERHMcgoRm3rEmKbTMLAlPFEAKAFRvsUSCxKiU7TbGKocI9opcvOD78jv3HX8D7Ry0I7J8soTKLSnsDEUSwgr+9" +
            "R7x7hNo/w9++RfDyBSrc692vQFphMI8L1Si0dcqUJ1p8e+cwsF4PcHxBtHtE+PwJ0fNnxC+6/vmw13pQwbCrPZcCFa570hoXVEHV" +
            "LTzMrP+aLQRdUDsqT6gCRmuPYqQN01R+i1BI/blmTqzPegxAqZQ7dq3bJSymVfDpbaiM9FXCb7iSGAXlyC/PXtj1du6eM6GeioIi" +
            "Lrko9g4BgBjEISg+ACEhfvqA44d/x4sfgA8vkKsrvWvQagsZbFJ/dBEsMzchiJILwjePlOkywpcKjftPCBUf9XqAhPm3BQAgbadU" +
            "jycAQYlCNQLxAYhfwMdHqMMXRLsvOD59Bkhogc33ITwfFudmNX3RVWhut+nh1LFEC48p/eS2ok2ojPNjJGUMJBNAwppMbH6eGASl" +
            "FxGHz4gPEtHuM6KXz4iePyN8/mK2GQ5Anm8pCy4/O0wCbBj8VGOv20Hz/ipdA6T/Rukv2j8ifPwN8fPv4Jc/gMMXIHoBqVA7QMDi" +
            "KEpMgMvzAbn2bYeu7XdpFv7S6Y+A1Bxzub50tlpNiqnKr/LuQIgJmVKS01Gsa2I5nEsQqEJB1q7prMluSJbl8EwmozQDuUdVWW1t" +
            "EmpA8/fsTH9cXHqgsRq9oeBkhdH8IYM40juChArx0+84fFiA4yPCpz8QbO/hX72Ff3UPf30Hub6BXN/qyV3qrqi1z7OzUBF6go8B" +
            "1hO5MgKAikPohcF6gWZeADDMl+G60vYyGkIhYiMEPIPDB8SHLwh3n3F8/gRID95iBQ8rkBBIhTNK2ifx+WzhNlfbmHNLjwceYDjh" +
            "bNow/+mWzwsAyX3qPZ6MB4kAYGmYyBpfiGMgPkAdn0BCId59QvTyGeHzZ8iXL5CLNRAAUkprXOjFdX51YGMZTJQAye5gehOAGBwl" +
            "igJ9uKOK9NkA0e4LwoffED99gNr9AU6EAI7Sk+GdcK4NSt7V57RlwAZc2slnKM5javhKBAEn7133XiMTAnxAysQS4IigVybszIxP" +
            "OP1TqM7fuLmuqOMCTVaR6LmkyPN1kdNa8LQY7K/dJXb1sVyoVAhQAIeInz/gEEcInz9CfvxPLG9/xOLuR6i7H8E3L/DjUGuc/SUI" +
            "xvdYyKL9agYYiY+u1uxFYBVCKe0GxAkTxoktABnjZqT5nMCGRDaIU0sAwkeow0PKgOltXRVICAg/ABGDIZBoSfT3LZmxRvv03NLD" +
            "wmm7Gya69IERBhJrgG0VAOesAZlijY1MwVaTx2B1AIcKMY6IjSUgfPkM+fIAAHobUV7O40IR6Ym/yZhgFAGpi9YRKjyCE/ef44tx" +
            "A/qsdxZ7/gB+yYQAGEuAxlC1PPYMfQkN/Su3CjibeM3o78gAACAASURBVPyeVVdrJ9dokwBQA+dhYVkshVnzJAxLOINUWqGeLjm4" +
            "njvt6XXhgXJ0roLZ7kCpSdEkbjRT6rjTO4oe93rxcBRBHQ9Q+x3i3Qviwx5BFAEMyMUGYrmFWALkL5A4IMyuQXqiV+Ee6vgMdXxG" +
            "9PwR8f4JKjyAOU4XbKZNX9OvCci24+VYHzh2eEb84iP68ivIXwEkEb98Qby5Qby5Rry+MYc3efp0V5LZScOwF3omelpLnWgtAtW3" +
            "AiCRCXxSHyI1Hwx1IgwTnhwIp8I9OEzOkaj5rOau7gtW5qwK45Z2/PIroudPUMcXIA4BFYFYgY0wAM4Louklsz79OmYwYsQvnxA+" +
            "/AL54d9BJKCu3oLDt4CKIIKVFggMzSB1R/tGxwgVQx13ZmzYQYUH/YuO2XWor+PjC+Jwp//uHxA//KJ/z3/ow93CnRYCkBj3dL02" +
            "UcSQ08033JLnATsvHYGaWsGe74drsUuJVloIWAKIgTiOqaS2wIVy5oBd7a4s5Z53bRsmoLCV2KA4Md7aJmigxSGab1zj8+k5zMz1" +
            "p6DcsdtHaXaTSXbvYAbHCkAIMEEpIMQf4GOM6OkF4eMT4t0L1EEzK971Pbz4LTypmc2UUfxmpwVOG5XjCPH+CdHzB4RPf+Dw+Rd9" +
            "Mmu4z+3YAqBVdWVK2Cg97wHMOJIPFYaInj7D377BcX0Df3MDf30NkoE+V8AzDBgJ7SZEmZsQJacPE5nnyb1JlAgkfAjpacbfX2rh" +
            "b7GxziWY0QcMvUgXHEMdXhA9fUT49Aeip4/pSfeFD0p9m4vuHpy/YJsmk1OrjbvJ8cPfcPz4M+LnT5qhjI5GEEgsVK48GzZT6W1t" +
            "iYH45TPCj/8BMKD2D4jf/AR1+BM4OsBb30AstsBiA7GQRogwY9Y3qCzgOIQ6PJmFvZ8RvTzo3+4B8f65LBREe+0WFL5A7T5B7T6B" +
            "d5+gjs96HIijnC24cS7gwrhT9PpxrqWrn6zHFgQuusfHGdPtxA8Mq4LvHXF96PEqL78wWErOrxx4hQNLpyznnUhOsKg0J9HvdXtU" +
            "cOlDk87wg9TULADVg36VyxFbV5z6oUBP7hwCsQJRZASAZ5D4A9HDZ20pCPeAOuhtR4XU1oBgZdwIxDet6UtP/VUR4v0jjg+/4/jp" +
            "Hzh8+hnhsxYCcgxa0QLg1BZYgVSsT3qNY6jjEeqoBQD6+DO81RW8zS18c+YD+YtsIbfnmzMfpLYQCML/z957NkmOI2mDDyIyS7SY" +
            "np3b93a+3P//QWdndmf22s6O6J7uklmpI0jC7wMooDVIRlY+3VkRQUI4AIfD3eEEcRBKP5s/x3uzgcCAA1veDnv9Fod3P4mXER2u" +
            "cHjz4/c6zHUwPjNCvAc/P6C7+4Dnz//E6fM/Ac7lkPzlQzYEGCnzWCl3/iqFnREfXzon/vrbP9Dd/Ab++BXonubdgCm/y3FFYx2M" +
            "i3j24fEGAIS3+uEj+OkbqD+BgUDDGVec43C8At68H3cKx9InOr8jY2B2Dtx9QvftD5y/fUD37SPOtx/QP96Oir8wBMSuTTeO2Rno" +
            "n4Dhafw8i3Hkg/EIJLlWPF2TJu1H0GsHe7lYyRCQyWiBrR3HKQZAk84mx/eSctrAEg50MC9dCpL2UxJGvtGMbC2uW7FPvXmzNwOg" +
            "UpE09tD4NlFQD6IzeP8o9IIe6H76KhYjfgZDNx8vyn/+T9DwM3AEGLHvd494dsnx8USPe3R3n/D8+Z843/x7DL14cu/eMelTW6Dn" +
            "BzOHAbwfAJwAAvqHbwBjoMMBhzc/4Go0AK5++AWHNz/g8PY9Dm9+ALt+K4UHXQnF/3AAOx7FMwSHq+U+OwgjgTGwwwHHN6KM45v3" +
            "OP74F7DDFQ5vf2zUid8LCPPzIkMHfnpAd/cRp4//g8ff/l/FCJieFZePj5/NTens9ynhYhrQ8tDp+PZpznvQML6B+vkb6PEr6PFG" +
            "GAE0gHE+V+SUJ9NOwRiiNjx+w3C6A/v2G/rbf4P6EwCGw/ENALGTdHz7I46cA+ygGgLfGWjowJ/v0d99xvnrrzh9+idOn/+F06d/" +
            "orv/IhkBZ3GyGC0PEB8YgR3EJwBVXgCaMaAJYa8rvc5i1NwQQEP1cs8GgOseSX+7wTrECCPgGcD1FA5kwcgxu+qfxBliML3hYW3Q" +
            "uu9PLidgV9yUjgj2ockYmNIdADa/i6PHcBKK7dPVAXT9Hrh6Czq+Ae97HN/9KI4TffcjGLO9wONlg3g/ntd/Rv9wg/72E843v+P8" +
            "5Td03z6Kc73HFyoZPhd93lm8e0welykZwygXuHjAu38GnY4YwEHnR/CT8OKzq2vxXMDhKHYEDmwOD1p2A6SdACzhQlfvf5r/0Pc4" +
            "Xr8H/fCX79bWq4JJq59OiBkNgeHxBv3tR2DoASw6vrIbAJpjwE11XTIAAMxHUgLzA6l8Ooby9ACcH4HhDBAXYUCuHWZXG6YQEk4g" +
            "cPDuhP7hBucvvwI4gp+fQednUcfQiZ2pN+9xuP5hDCcb87/o3YBl8KZnP/j5HsPjDYbx3Qy8exLGExcnhzHGxU4KxvCrceovpzRp" +
            "hkBE9fXa4la54svIQ5khsFUEe2OwGq0iU4xUnpK+3o8dmQNE3I/yTIAZDkSyU0S6ujJsHRjRsdYwPcgy0ozVjB6vlyxnR/g2LWN2" +
            "O72Rjzn9t7HMMar3zQvSOImRUBYxKZs9hvM9zncHDEMHunoLOlyDjtcg4njz8/8CYwcc3v4ASG8t/V5AQy8e9js/on/4iu72I7qv" +
            "f+D8+Vd0N2K7n7qzl4/8/Csx6SgUpge8GQiM96DuCZw40J9Bxyuw4xWG4/X4cPAU7y8+lz/xNmg2PRMwrypiJ+D6x1/w5qdfQD/+" +
            "CQzA8Yc/jw+vjqbJ9zXMxZiV8tmNx0XYx+lBnLX/7SMwva3b8mzA9BzAEk5mzuj5X3lnYD6ZZnxpWH8GeqGgM5LeNqsTbFmhZf1f" +
            "LLYc4ADvz+gfvgI4imeHTuOzBjSAEeH4459xpP8QuwSH48hqS2jQi5UZ05jzHtQ/YzjdY3j6Jp6hOD8II2A4gfggjmAdeYNpz3xY" +
            "u4cpH9JQhbyguYuTqXGsYQDIdb0aAhKKmuOY2K6khdOzxBCQ/JLqTsByWU+6YK9DLjfa2QGm82+E7gVi/jF6obJVRg1D1p6PfDfj" +
            "CNoAoerNl8pIYAQQw6QT4sAAdBjO9+DDGezpFrh6Axyux1Ni2GwAXBMHcBQPN77URd0CGoSXb3i6E0bA3Sd0N3/g/Pk39HefwJ8f" +
            "wfuTmkf/HtldbMohyQfGO6DjoL7DcHiUlP3DZMUpnySVtrzlgYERGw+uEWPK//QX0C//AZz+A8erN+B/+qt4LuQVyVDeBDsp5cTF" +
            "26PPjxgexFGQ1Hdgk3eeSHpZ15hb3yKQftucRNI/mB/65YN4EJgPY+iJlGZakZwhakqjRhEygLoz+vsb8NMJ/e1X0PkJDBzswMR7" +
            "A2gAO74BvfsZDG+wHHv1gjEPDYlwvu5ZPBz8dIPhdCce8u2exzAqYVAJ5Z9gnPXDli/GQ+FSEmUovUTlYuGTNQ2ACa+GwILU12JJ" +
            "Oa1fvUl1P2FEVh0hQyCGHPVlYUY40AUNblVe1F/ofUFosPWUXL/2k8XdjC4zG63Y2VmupASMrj7xoKg4DpD6ZxHycjrgePtRbO8f" +
            "r4DDAYfrtzj+8At4d8JhUjgP8tuEL5Q/ZUgPWoowjum872E+3aW7+4zz199w/vwbzjd/oL/9jOHxVrwfYAzzAKB0R/LunnJzUe7E" +
            "qS29o8xJ4VpUC1knBTA/0zFFhjAccWCE45HArxj46W70Wi7tePHRHE0xGgOcjyfCPGF4vh+95+LhWzY9QzJOqWVHSBo8SSGMFhnz" +
            "EaWYQ080yuKbMKUeehB/AD+fwNgd2OE4ywjGDuJ4YTCw4zUO73tx/bg4EgiT4+ClMBQB4ylQwtg7iZ3Cp1v0D18xPN2Cnx/BhxOI" +
            "9+P0pPkTtPxOWgpWUoO2MADkui9I23uFhNKxU8KBBHiM6WvCk7wpc1WRb7ZuJPv1teRpiTJf0xBIGTxHWtJuKr1K02Jshx6KptxL" +
            "IK0Ff7rzaQNgeZBMtEs8sMiIRPjC3acxqoTh+PZHXP30F/S//BeOxMVCf3U1ng8uF2SUnNmatbG86AckXuwzPD9gOD1geH5Ad/sB" +
            "55vf0X37Heev/8bp8z/QffsgTlQaujEfnz3+cw9Xar6qBloKZeNJUDJ3S6FFYwulAoXxxxgXg884ltDL1+W3Dpj5nbHRpUMjnyw7" +
            "PrJckh8AVj/NX7bVYjYFGWkPHUvfHEuKFfMaTAAG8e/5AefbjwCOGM4duucn9OdnDKcnXP38v3B8/ycc3/+M4/s/CYcBxJuuX4xV" +
            "SRDzno9vDT8/itOBHm/Q3X/B8HSH4fwE4j2A5dhVGhcRpoy1NDq+NwEHx6rm3I1ZuNvJijxl8oJll6W7faxQozqfhyqnal+e2PKU" +
            "cKD5jcFzKepkSa1p/+zBZsFg969qiqtnfjaZupIjOTvv/MMXnBZZSfaAuhdUwKojR1XppNzI5FfX7fBZJroCYeu7ZcWXlUUGwnS2" +
            "NwPD9OwAne7RA6DzE8A5rn74M65/+S90T7cAYzi8eQfGGOhwnEhYCDE82Vst+kkaDkBiQedDh+H0gO7+i9gBuPuM05dfcfr8L5y/" +
            "iNOAhoev4u/0COo7iIdASR0miw6YCjtPSW1ikCSGsGAV/h13fMR3ea+Xlj9MBsBoFLyiDmZLTJPmTNxc3hege+r1dS4gr+RvisI/" +
            "jbsl9WxbpAhRJvh8zMdPD+huP2E4nXF+uEX//AR+egI/P+HN6R7Xv/wVYMDh7Q9gOAIHgNiyxl2Og8CGcYxInAA17QIMz3foH76h" +
            "u/syPkP0LI77HMO+pnc7TJJYyF0p/Nf5bggXDa2xrdbkWsns2L+G54RrmWw9RfQlMldx9Kgnvmw2aG7FaVuaAJJV3p0PdpYJawZo" +
            "Wnli6mzmriaGb4q2bIr0OtmzpReSQFElA2B9ZIow/cFePWOUy0A1AJZKx7JpMgQgjIDzE/r7L+BDj6tf/k+8+c//C/3TrTiXnjFx" +
            "PjhNbwrVpufmhoA5n+xY+JHG4z9FDPcD+vsvOH35Dacvv+H54//g9OFveP74N3Tf/i3eozB0QN9hVggqKP0hKn0paHoRgVWwSxen" +
            "4ZANAcbH3YAIR8sromH05GgTzO/wm2S9EvJDSm45djyGD+LvRY4z6V/ENv1wekB/OoFwA3b8gOH0BN6Nf/0JBIbDm/e4+ukvIhtj" +
            "YHS4bN0fgLKG8QHgHWh86dfwfDfvBND4boZ5J4AWMTm6WrCMNiX2y/c7R+0S/QL7g7RPB3bjQvM5RwPqSQoWI6ADhoEx6XAgxbkR" +
            "QlLFF8g/E1INuW1hW4RY8J4xPJSrVq4w0DpttfhV0+GUvHrMsEfvZbMhoKeZqJ5WqtGzTRzUPWB4+Izz13/i6fdf0N/9B67e/YSr" +
            "dz/h+Ob9cjQlO45hQm/Arq7HeOAxZOhwHE+nWQtSe5xJxpNUIMKA+ocb8dDvww266QjQr9Pfb+juPohTP7qn+W2w00LuMwD08DIf" +
            "33p52jPZ45VDplXCAYwvi6BePEQKjlXmyncAcwdn8f7KBtekFIr5q2oHcc9lUJLXOGmPTEks8RH4yDccBAJ//ob+7h3OR+lUKeIg" +
            "GnB897P4e/8zDtfvACxvtt77quUCTf9NR8FO74bg4s3NGN8FMO+yWUpQPl3vGNFge8bjFTuFb6CmeeWbjLQkbQdJY9G+xsIqIjKh" +
            "vjF4PntrEpRkKFe2/lvPAGg/NLFU5CjFct+p6nZmKI6Lech61VqQ+ctNiyEKV5KM0f3sMKOt/RNjRVseSHDuj4UMAX29IYmL2OSB" +
            "hPAO8xP6x084ff4b6Ei4+uHPuHr7I47vfsLxzY84XF3jcHwjPt/8IN4y/PYnHN/9NL6N9h0O12+BVY0AIMoA4AOIxKLd3X/G6fO/" +
            "8Pz5Xzh//Te620/ov31Ed/sB/aMwEKh7HBf20Ysn78Aw5cO4FpqjUXzlK8ga2uHZbZtCgWgAqBNvk57b9or6IPufLDf1k4GmX14Z" +
            "6qrLfzlC9/AkHo2ZMXyMoQd19xgejuioE95x3oP3JwznR7z55b9w/ct/jS+yEy+vIxzGd45o3o1dPzMwqSSqLBbDM80nvnySfgqQ" +
            "JFst18XcDo9znBGXO49b9H+JTNkzP0ho3MQ6vRAx461qYARPhvwQenrYlzLLG4MFOEg69sI9CdYxANZhypRacimy58sxKRzZEhYv" +
            "eyqXYbKNATBVFeOYs/2wG0j2Qq1NchhazrSOSWLO78nduFjXbHIa8xOGx884fWHou3tcvfuTMADe/oSrtz/icP0Ox1HRP/7wC65+" +
            "/D9w/OkvICIc34o3ibLj9fo2gAvzk5IEGmN6eX9Cf/8Fz5/+gcd//X94/vxPDPdf0D98wXD/Bbx7BvXiqD9Gw6QKmIzA3PxRbADY" +
            "Ers20ma4J4YQqNMbpDuAOgAD5rjvXDnwCg0O5R9TcJ7s7rMohtF1pCex2pKu/PoOF2gUGxxgHeh8j4F60Pl+DAk6Yxjj5Hl3Ag5H" +
            "HN/9PL+RWrynZOQxpdmTLNon5OOXaTpRDNLJYrIhMCv9duPO+jty2rkNgdLFsPa83xs9uZDbEbNYJ0KZ/zaPUoS4j4ZLr9KSJD6Z" +
            "HLvD6BtN7YhQxqYTK5ZDDtxVrGMAtEXuErAXmLFhJQZAbKUlmRsgl54C28tZUGhWGsYKGw0ASTGhToTB8DO6pxsc3/yI41uxCyD+" +
            "3uH49j2O1+9x9fN/gp9PuCYuPH5gOByvgTfv0xtWHYT5LavEQaOSwrtHDKd7nL7+huePf8fj7/8bp49/B3++BX+6xfD0TSzqbHmQ" +
            "E0BYfkZSlb286cZALN9Nxh3DqKyMIQxjOBCBzw8y7mcBvlDo57TOBoDtu5IxtoJ82opB45wYZ8XwhGE4g58O4J14ORbvThieH8DY" +
            "Ecc3b3H1/mccrq7Brt+JXUIadwjZ+D6LKTxoM0NAlRFLyCAt3+cXsw2g/km8xK9/xnB6BO+fxyN2J2Na3glwjTXM68WGQCn2Nu+/" +
            "R3oIBS8KiK/D0Y5c3irlSXMnYH4mYLSy96bwBdBuku4YE18lNDycXGZWLeUOO9k2taJInOa9J3GyoaiXp8eBScROL7OfNwYgzjlH" +
            "34FwADiBugH8dAa/esRwdYfj9Rscr97icP0WdHoC44TD4Yjj9VvwwxH05p3kXd4QRKPXvwMNHYanG5zvPqC7+4jz7R94/v1vOH34" +
            "G4ZvH8CfbkHnR9BwhqysqcdsooohULqczBTFzIMxzdKa0ZtJwxzbvLe5dLkY+WZet0hiJdmlVeI1SIDBHybD5FAy8y8BYOIBWBo6" +
            "DKd74HAE8QHHN+/ADgeA9xgevuD4w59x9cMvOP7wZ7CrN2DjiwnZ4QrLC/CA1d6PI508yPsTqD+Lh337M6jvxs8z+CBCB/nQzTuI" +
            "NP6dPv8d55sP4l0Qk+Eg/xk7PQoB6s/IZtsdQLWwN8U7E631g1plR+pNa4nnWUwl9l8WfXyshwFXwBcAYrvwOL2CcQqtqwxdL6oH" +
            "TVl1dGK0g9ZS+u7X6SYEegqtt09WDbkLqitjmq/QZigx7bf0Xck6njQzbetMRgAnUNeBHc7ghycMhyscDtc4HK9wPF7jeHUNnE9g" +
            "7ArHN2/B3/8Iun4Hev+zOE97S4zePT50wnPXPeF8+wHPn/4bzx//hudPf0P35SPOXz+gv/0A/ngL4uIEIAZyO2RcfSjdnm95Jm6M" +
            "wycmdMNrQI731HJIeP7BQZyP4Q07MNguHcrzGUIJ1E1JsRsTkGnOsVxukPHFXZxcJgUrscO2bs7GDInwIOJn8NO9eIvu6VGE/vB+" +
            "PGTgC65/+Sv4n/+KaxpwfPuT2Bmg98AVAw4EhiMA8ebc9obAND6jt78XtA+ne/DTg/Dwn54wnB4xdM/g52fx2Z9Ao0OBDx36299x" +
            "/vYHhtO9kBljOBDNk87Wz8u1eXyUOxGLQdPuSV3F9mI0uHWwoq5rpV9E2FuL/CippF22bNq0jONOwDOANxgYZ9MZ1kx6KXsNtBhL" +
            "Yciwhd9kPkyQtfryMRVzEcr/1rjgTkoxCrOaKN5UI/02C5p12vkMQ7Ew8qEDoR8dZgeRmRiAA46HA46HIw6HA9Cdcbx+g/79D7j+" +
            "6RfQu59B3UkYEhWQZevJzwD03fjm1jt0dx/w/PG/8fCv/xsP//p/wB8fwB8exWc37gCMR2cG2cojxAlSZIOtoASvH9N+6/dj6pDp" +
            "IWDcCRhPScp9OeMrFkxdN84d5U3Urn7VecDLLxYDwJXWURdlZTRhsv24szR0YMMAzh4BdhQPCp8fMTx+xXD/Gfz8AFAvjhsmjgNx" +
            "gB1xOF6NDxiy+e3C4kCQhsolYRkf4qD+hOF0j+HpK4aHG/SPt+PfN/TjCwT750fxIrChH98sPoA/3WC4/wN83Alg8/MCU08FEGsA" +
            "7Bp72z2QeNvg+0TUHg4La5C2ROdWzbRPU6usi5olqqcDyXo/xyJUMygJERnPuv5Uipj3uOXMq3ubPBeKleSmc7R2L7fj+YyWw8wx" +
            "hYkIPWYAOJstXc4YwA6gAxPesvMT+PkR/PQoHggcz8kuRdbskBQwGrpR+f+I7vYjTh//gdOnX3H+8ju6mw+g8xl06kD9WZz/zeD3" +
            "1CYQV2t2R5cTsSMwlzgpqpzPiqr4oFERe0UOJP+yutBD2gVYUV74eaeG92R2IWA6MlN878Cf79CP4UDgA3AYTwUijquf/hPHH/+C" +
            "449nHN/3OFy9Bbt+C7C34+lBrUEAJ4BGZf78gOHxC/rb39HdfUJ/f4Pu4Rv6+xv0p0ch48ZdAeJ8+esewJ9vQd0zpjcEq/1iqVf7" +
            "ZVfavNl2rjpssSB6OkNjc1fXGVQbF9br8Jwe3DVLyNAbNzhOBxLWkXoYYniQom4lpgsbAMY1n90iyUzxQUYtFzGQW2Pi+Ex5Y+Ol" +
            "mH63VVd7vKqKUBfB5Pw577/NrDo18CD4mo19T6NNwMHBhwHU9+NWeS9CgQobktuvBAK4OKtcnAD0FafPv+L50z9w+vg3nD//G8Pt" +
            "DfB8AvpRQQEB42YHGFuOAc304E/pFuUP2oXUNrmrsTp4fXVMGupkANgE1s5Pa9kvRiV/3gEQf/M7AmLnhGccnWPupiiiojhyljIF" +
            "cYoCSyqT86EDOz2in7qBMWA4gz/f4fqXv+LqT3/F9fkZ1HfiBCH6GcfjNYD2RoB4CHgYZVWH4ekW/d1HnL/8E93Nv9Hdf0N3f4Pu" +
            "/ptwcPRn8O4MPpyXQwaIgP4E6h/EG4Rp6Rlbt7pmkyptzXzuRqS0OIBqUz2BqKikPmHmE3bu0KAoUuYLhR0TaKPNtisZ1tz8a5ht" +
            "vpFUTwdi0qY1qaJUGVYH1XUbUz4znPQYPaI+APG6/FpgmzGlxUR4CGLKS8vnTm36iTIRyMqY6q1kLroYFIWQsSkkiAFsjIAlEt60" +
            "oRcP0w39HGueiyL+Hx96BRdGQPdwg9OX3/D47/+N88e/o/v2O4a7G9DzCdNpIOINumN2WWt3eW4TCFT4o8LE1sOjrEphiKL5NBSq" +
            "HHT5/cIMA7H3qz48zt4PODeVvFkxc3LmeHfYrOSCQMSmr1A4nY1hePQoHAP9WbyV+3SH/v4ThsdbvDk/C+fLeI4wO14Db38Uxrhc" +
            "YSMIb75wXPDnOwx3H9F9/RfOn/+B7u4G5/tv6O6/CeV/eiEYl2L+QePbg88i9El/EYtGP8G1yZhhANSG5pyMg5y4tvJvy+BT9l1Q" +
            "Q4NsdbsNgEJciFhdk0xZMsoj5w4HsuXWvzdDugRKJWvyqL4q/OvhQubliBrb9bZSaSldCxNRTNHRADAcwpOyPObjxMH5AD504N1Z" +
            "vDmT57+Btng+kFicBT3P6B++4vTl33j6/b9x/vwv8Mcb0MMtcDrPjzpMuwCLgmMhn2mfKSTlZYtGEqcQJANgOv7QshvwigLIy920" +
            "GwCnhzi35+tJiNyS9Oj9hdOJ9xiGHgzP4OfH+fmA/vY9aDIAjm9wuHoHHN/g8OYHEOfrrIejjBCn/ZwxPAvjpPv6K86f/47znTAA" +
            "znffwPmgGssM0kuPaVzDaXYYkCYwdVVZvbsDA+DFw20IbGIArOLA3jF0UXP0vCwMlnUz2xbwbR84MyS6+xIx1XDpg3+xbXB4Bdru" +
            "BqjLZUr5tSAW7dgSHQJ0OkWISITe8F54+7rx9AzeI/eIUFtfprSfD90Yw/uA7u4zzvdf0D18wfD4Ffx0B+qeQNRL4T9Shbonr9Ik" +
            "3Y2hP7ttR6XfCAMaP16dEw3gZqLpMZxi+Hi1pqCeJylTfypppMlDBD70QHcGATg83ePwcIPD7Wcc3vwE9uYHHN//Mu8gMsak91bA" +
            "VnoZ+TSAD+dFTjzeonu4RffwDd3DHYbTkzhWeJorTBIHFnlB833W3uJviPgoQJtnKIK5svmv8vi7SlxRmQkaIBepVPkhL6kyrOFA" +
            "01pUpeZsD1fWHpmjHHsZLqPm0uRI7twp2sUuqdmTxXYrfSw8IT+2W07vQHkP2aEp92MV9jADl4YivIBEXIQAdWdQ9yziY0fvWW5v" +
            "Zvkkp9OF+w796RH9wzecbj+hu/uM/uEr+scbDKc7oBcPLk9RTfN4+MgqmIy7nMeTU8T1TMArqmDWF5coGYiJpu24oUyGkveCdg+Z" +
            "FWmYQ4GkIpn8Q0lL4n0BHcA4R/90j+PDDY5vP+H45gccf/gP8J+f52eJCDQeVCZKZKzyisj5/Hbj/ukW3dMtzo+3ON/fonu8w3A+" +
            "Yeg7qSGyx3+8pJGj7KKSOu6XBJsoYJZvvlRSaQlpW8JkfOcU8DQjxXVmyR4Pqb7pL6XH5Hw+ujaDJqOMcCD5Bfauzovu1CqL2zIE" +
            "kkhIJEZ2KZpDsaoB0EivXH9bW5bGCbkLdm1ClGTDMiZkJKgzYKpAJ9h++msTvTGdJgPiwCB2Anj3LE7aQbF02QAAIABJREFUGXpY" +
            "HuVX8reCOBHoEeeHG5xvpZ2Apxvw5zuABjCaTgIKk6IMTWTaXcEYzGknQHowGHqaV6RDlkfSLFNsaHkFMbdbUtQob76pzka7AvJ7" +
            "tJkms6xx70Ti8ICBA10H9kYYAYfr9zhcv8fx5//C1elJHL1Jy15ldeV/JoeLQwPOj+iexHGg3cM3nB++4fxwJ94MPNJirX4aOod/" +
            "RDGGdikU0iCakdMQG6Nt2SGZjM9W8pMEnJNVes7HsA3baNB+WL76woHK0HjQDC8+Ld7I5f7OVlamfd+RIZBeUwYaERc7QbfhhpD5" +
            "EmEIyPbryOyMjWeeTG+fnR6yGzpwLt6ySUPvICdl/HxpafYcTnG7/ekR3eNXnL99wOnmN3R3nzA83YojTIcTpq1G27a+zr8s2jBa" +
            "Z2krqmPMLIZP3gGgF6Os7AlMU/Jzu7faDkE1pPokxbn/xMfgGgJ494zh9IDh6RbD4w2G5zvw8zP40IMNw/gCYUnTnqsMyDIvactN" +
            "4hy878VLwMaXgw2nJ0HD+QQhUXR/KqnFuLY9XijyRcR6WsFeIPdVbsub9ZixyHnuVaoiBMfpQIXIbIgvm9XBYbk+vzHesfW0q/V2" +
            "Hq16FLFR6GeTEkzgcM1sLGiq1O5tRoJYSRpXbYdq/GnUYmxkCY+d8NaJh0w58fmUIN6dMHRPms4/vVpvuSjPlSXZkma6o8QHT2fb" +
            "AwBJb77lHP3jV3S3H3D6+k+cPv8d3e0fGJ7vhEEy0W8s5PZe0du9PZcVQm7v7LnYiJYXBsEbgkkmAzOCzbaDwcwx3M2UlEz+Mf8m" +
            "yDGP87uS5ZO2+CB2Ds9PGE4P4Ocn8O4ZvDuD9WcwxnA4YPQWyk8wMS1mXephw7kgW2ByOrYcZDDuXvL+LJ5jmo4LBo0v/rI1P9G9" +
            "NztPLDkuZu7Jstp2JyZ/Zc0nWxinqeftlXi5IY6tJbnQiC5UZ6n+PYGc8mQSPW7EnQ6UgloGADnuRbl8F6lo7BggdixdKWpKjpD3" +
            "JLfUye1oM4Tck9AkhYUSSFX4JretkZUlsDzfUu7pCMqoyAGzdZ2rTP3JRL0KXZbMay6NPE7ifQE0gPNOPHTXP+PQPc9pJ8FEspak" +
            "EaTOs0mTYpCNAfEnveSK8/GIUvHin/7hBufbDzh9+SeeP/8dw91HYQRwbVdCd/PrTSc1qXMIQ8Ph7HPPvcQqkqHvBGiE7FJpvQAw" +
            "6ZtsAMRMrZhyq+uMNmbO4clJHsyTRFvhpHVAxPoP4sjQ7gn8LIyA4fyMoTuBdWccDgz8CDDOtCVKNwjYUigs32fldZQj4xHHxCdn" +
            "RYehOy0hjJxDPtCAzbFNy6fZPREd1kLZr12mdd2yCUaTjHhDoDKKlvWwAF7PRvP0jb7wROoSYhawacVN6/0EQ0AmyXYvBnXDgRoY" +
            "AMYtB9e7dgrqI9sEXh9C4ouvGIXxchP+diQou8EthKyM8fBt9fjurQQmf9Gb7NBWlJ+aU0FZYhkBEJ54Pr4vYJB3AhQj4DAu3Gwu" +
            "yLoTMC7aYvE+zL9nDwcJs2PaBRALeg8+dOgev407Af/C6fPfQU934M93oGGY64EcauDigfGWS5nTCHYjoc9d2atiehnctGWpa6iv" +
            "FkA6mPyn7VppyWxZUzxqzSV/RiV2+SIJu2kusfHlP9pOwHB+nHcCeN8BB4DRuBugkDMZBaNyz8adRc1hAEnGzJJFxBhB7ASIwwyU" +
            "nYChA4iLHQDrHJDWsdi3irdCK6NCWbfiBcFGy5odSfy7rS5ldcHE6g6tOz3RSVXSi94jQv1tbDR41mL9Pa77C2zZ9HvTrWArdjXD" +
            "MuBtpOtmmwY3mfIN7AhAsZ+qlG3tUf2ir4MYAJrMuCUj8R7D+Qns8QY4HIDrd+Cco3+6xfH9L6oRwBgYFqVeNQFUkkgxFqZap8nE" +
            "Z0OAOAcNYieA80HsAHz4bww3f4Ce70DnJ2DohFePQYo1Djd6FkClLBrs14SycrLpicfvS+jiK2phUYjV0BWfOXApbp1cGpf1kQFs" +
            "DB0czsD5EezxG85ffgWuf8TAOY7vf57DgdjBVtrUr4tsYFYjAIrjgNgBU6HD6R7d7Qd0dx/F582/MDx+FSeHYXmng7O9njU5TXXO" +
            "QEtG8ekaG+ohVZu8p8mm6YcEv0xeZwikDlqpr6zPBAj7nkXoQIUUJu3kWTR6p/ZvKcAyek57I1i8TzvZGxala6baafrmVyEKjkua" +
            "3WsNutu7F9LCEAh1t0UGsKmEOS5GOrx06MHPD+gZxlCgTryl9+uvOLx5j+kpgHnhZgzK0QD68SIKIZO3T7s+nWwDDuIkPHucC+Pj" +
            "4Qu6u4/o7z4Az/dAL97oKYwAJrPj/J0sXKE4+1SHZh58/W4pOGZWyNlCdoYBV7fj8v0Om2L0Nsv9txjOugGqponhrRQelKZrHDw8" +
            "kYu5nJlwJs7p7zuAGHrcAl/+iX7ocH74MsoMKLr8lF9xQMhvMZ8SyzsDTHIaMDYbAHQ4iF3Kp28Ynm4wPH7DcPcZw+MX0PA8vk2d" +
            "qSTrsgGwRgRkGQA7W8KlSGbLTcs15rg19k+qHFmlO5RJtANdKqHq2i5Te+vXNQSMZwKsxr/RzApUZW11mDPftoA668uZEd48ezJr" +
            "PdDItG3xeFsRverFpc3qtZUNgAmpj8pbhbH0Nao4mwdzUv41S4K4eDkXHzqw0wO6h29gV7+BXb0FOxzH9ZhpRsD0KZm5BuFsVmCk" +
            "2pYVeDqelAicA5wInBOoPwH9M6h/ErG+NIARB4FEmIGzuTqTaom0xTFWYYtGDlMyKZtrdfAZF2Tef0UORCdOJ2bJU2QMQ9eTmswx" +
            "KUzaePgW/WgejHEmtDQAFGIgDHY6ix28vhNvFr7/DPbpf4DDFWQng5xvMd6Xzl1OYRqNAMkYmB4DAAAcjqDDEXQ4iJPMumfx4sDu" +
            "CXR6Bp2fhOwARkMAy/yydIgyVkkTfccGgPY9qlm2hOS5F1l/cxiycWVdauobQkrklbeoEljcYMuVmh4hSxe7w4GYNPUVCtedOV6l" +
            "3vYZypdQ74vwxjHpM7dTPHnNuexZ8QoM3EsZj5ayTVHG5xV9AO85MJwBxox3TzFgNgSM7fq5rOUr2X+MtY0XpIdaxRGlAB8vTQv/" +
            "Qa7GU6WhbZB+M49hsnil5jpkmXe6AbCRSH1h0K2shc+V51/yvE67RxafExcvGaQBIPGSQbAvImRnSiJZtovyP31IFtb0KckYNv1h" +
            "HorRCDiADkdRLu+BoQd4B3AC4xwYOJaz7VpI+/0ObBFl8uKo60EU58japGc8c60JPT6fV2GF8fqJO5XXEKiFAwCuXnIfETo/iB9J" +
            "hKdXc5uRqiS6PGvKYpsrW0q10C00WIu3K1qZb0KAfjfhONNRPwx2o6/Nl2BFuGC0XzCkPstkL6j4Oi7Zykkbi2IkK/d+FpeXZhLl" +
            "SQQdJvoYVA+g/hmaRz5GjLQJioY5diJY+Gw+hdFDAGG0oS6aGfeECv24s6FIkcWu6aTPaLtjYnoId2Jc6WQegkc2M/U7QTUIwOY3" +
            "gk/GAREH8QNwGN9mTgPAxU4i44ssmaWMLMMU34Bu9DlINFDZS1gJ1WSYpz0hkVu9KwqViTWHZtIbJ7mcRITWsaT9qQiPqL3KuM4s" +
            "6fKs04HCGwPkvhUJb17JIWm9LQ2O1TaJVUL0dLmGwNqLTKh9OjRPpbVbXWXaFHO2hJI4y5nrZPHc4jMEUtvcEEY/aoIiihxH35GR" +
            "iC2f0klQy8O8dm+e4hYxyfQQQUqVk9EhmxTqOe3uRdvFI+qibzCMwa/VkVjwTFIgnzYSWIyxV5SDqX+SP8uUT8vX0lCAMqR4BW0w" +
            "UwSvSMxK03fiAGNgJMtti5k6r4va4soANnWksv239C9xMSY0yaUxlJBNxw1L8902JtNxoVE6hXXRb4zUKhxD34odV7d5YjwiFrSg" +
            "M0ZPzCbCs6Av8l6S/Mq6RpEKQVtDwP6eACFHp1ltrcRdYfkwukpwWm2OxE0EfKohcAlrvDaYSczkSMyAZZEJ5FsUyExDYKd9bGtR" +
            "NKnRgyAbApg9b2A+X7NaMAtW5Bhg+etk+Fk1LoePg0wemVUIps9xg2F2iSBpcycZFp1SxisKMBugbHkmwGFUb8dK4VEO08Zg7gV6" +
            "ylKMH1JFx6iIR+3OTjw8aQj6/B0VfRpl0WwIuAqTp0Ip81+CATDl0dr6cuZ9npxeZR5aKpEvZR0EkqITKhORqZMnnLFSKhXKc8Bm" +
            "OJDWMk/puuAIL4S1MNU8vWhc8hi4Btx0gsbD51YqK6gOdkTOXjy1W8JHapVmMFgEic+MNv/c04DG/yJoAKmxp7JjNhfNO68NrKRN" +
            "OpNmvZKmUL0iEzZ+q9CnPgN5DV9QKJ9i6nvX59jC/YFqsoKgvuxOX33FfZrSWV+QR0G6IxNUzrdSFeT9+R2ivhCMYa3KIuPi4AkH" +
            "0oQLaRaT47vtt3Ivk9O9+bSbyjbP+F2nN2uwtS6x75EkFlQTpcU2cLYmH5MXicm7p26x+QixX1pT8NqGx0uDx4NpYuFI8W05am++" +
            "6CnGasJL9XtptNDgKkfPK+8GuJRn0014OTsCTPuta6vTA5Tf5/JTH84FXR+QKCwj5mKzFBbMZddQPoKxbSbdc/2wVBCQ/y7/l27H" +
            "EiBFBWlyyFOOHWT55k0Wkzo7aXVYDAEW8ySvJa9bP1kbpZ4fqWG5+mLzDC8HlnCgA8az/JwjuaUBYNQne/Ul54IyC6TvsqyLUiCZ" +
            "9umoIl7E15+aeoimv/ZJUXSkZpavqR4mW5HudSofo4ERtaM2pdG6X1Mr7TdzCJvrDT8C6lUyWKCPHUr45JHz8bh82UXjHOrrLYGF" +
            "mcXT0aaH3EKcko9Z7+UNV1jJsJWbMteWC3pJzH499VzaVygsIvOTMn+MRD5II8ykrA7+XcMQcFS/aOF2Uz6NIJv8D9Ejr63Td1kk" +
            "SBUoYxNDHyLakWsA7E7xG2U3RYgAG+2aw3NbQ6AEI1O2NAD0zrnMjqqCQDiQH2sYAOFaw3XrCQxhXuA0UH/HqHvtEC5dNlkSaPEk" +
            "TWrRjidaO9LiSralstif6k3rDdWlHsvasv1sy2uWkzmBYuHQl11p88ePWb7FVRtbrnsA7f3+ikJo/JBuU9lHJEYpjkFVWeMyjlNR" +
            "YznQBNZsIIx/zCmzClCyA7Bj+CMf4vNdbm80WJFJ+9Rq21o92ap+y7vBOEAc9kXeZX7WQ4phkVqmMUHI3Dww8ko3w7T4UmzNYjYK" +
            "tqdpQjYlldivmbCsY2up94JeItXiTWmbby64KbIQFCsqUgzMWlpYJKoYANKl+ap0gsp8hOsrikDajyIT9ZI8hJkOmlrN8nWVvgvp" +
            "WttjN+PDKPDmvQC8rCZV2gaI2AXXr72sfgzDCAeS1f/pUdvplxvl3ebbind7IcPiweoosOksmrLPpmuyV8O5v1ZPjJVCrim0HcjG" +
            "f+XedO6zePaj5y3gQjbwbnl7bpRse64y4YMNM5M67zG1GGfR+nMywfIXRg+SyYwvdsR2ro+39FsMy+lzOgU5W+haLSm7aal1kpKB" +
            "6bdfUQH6zkp2/+5d+Zdho5XUWzYZr3/XZYurzJjqlWseAZ0iIvxpI0u65Ann2wFYj4rGKGhJgQEwXdzaEEhQFarBEQ40PdW/DhH5" +
            "1WSKBccq4ZL7yS+RULDdahJbs7wVFszj8zxVamqqnbVrA0BGaf+w5UPfvrRds8Hd5n170ZT2sczt2xLmtt2PIUCOCjKIviRN80KQ" +
            "tHP7gmFhVZuMZ1DltpcjI9k1y0+WW26twveO78IAKICuo6UYAI7rW/brmiuDYgTM7wmYQSBK6dH63VZUYsRWpPWar1JjdF7eFNxz" +
            "2JAN+x6BStTtewjqwBpPEJk2t45KxaSRyqQ9h/BbGl4RAc8ytQtsMX8z60wyBBLs4hpdYJaxb+fF1risZcOjsEVmy0WxA/OCecvy" +
            "TADkt4cDUOXr/BnT6MK9lVA2ffPet83jqyNK+QcyDYDL5I5cj1CtRaCZ8JIGvJ2ArLypmEho3hgwb4JdOrCbGAKuPQZT24kxBHxV" +
            "yYbAKyqipcjNlH16AiNtiZc9I0Pinpd63bEVGZpWtYaFTTI8Mf50/7NMC+Ek6c+4G13M5akgqfQWDizLqdOGvTsiHFCeCRgYY8IA" +
            "4DAPB1qiB5OeXM/olPCWjX3UnXGDjphE126AwRTF3n9PUOTW8ASheeMwA/kct5Ji3prGx41D0q6ObXcAUh43jVdIUiJ4KyE0QB4m" +
            "LVNK4tqgVx+TixLKf0UuylZkL9vZhk5zLHjlpiutV+D6qw/WaUnsaYa3HlvZTPudVkgENKUjZx25DCx6lutuVBSintBf7H6QM2Uv" +
            "abPdph9vPCaWcKBlG2A2BOSPFAOgCVieeA/QHbRdigdqhyZiRJtSvFspeWvuCmQPjTQku5SN2UQVGAB7RSIzRS2UubQU1zXJMDks" +
            "6BVVYHio0leLrBFJCItpHjNTgGjFX9oJiCU5X06XG3MvCdEBCztUObzIpbdgkJ3Gayu0MpILcPgyf+0wsGEhadyDUk4IahTWsxdc" +
            "Cp1rI4lPd8DU1VCbIb4bBsttaEEHZe+Y1EGa2TX+SSsQYyz+TaGvcGAJwxMOq7g4CvOpjA3HYS0ZYVNGYpR6n7FjvPQuh7BwnS2q" +
            "2B/8jOALOPXmXHENWq2qhA3qOZwtLrmnnrats5fehtuXcKAOwCD2AbhMxSRHM+2AS9J71C6+lP2zCERuM1crQtvyrsEDrnJqjo61" +
            "jlrbdWtMhBQl0hEOUA+ZDS6JJWOBNnnChlry6FQ1UzQtBuAgFP8XIGL2g1HrHxes8Zw7i6I/pRWYxo4ZV1Q0D1EsRijIR0/KVE+7" +
            "ZAh4yTHY1m8AFLO4hyDbLaW+S1JCFIRDg1xXvEqkLYS3cICs62ZrBMJAvRGkbPm05XXXSeWdFarCe7fWarVAeSYARATOAZqOCP0+" +
            "dgAm2OndQdBWCZj0WcEQQGwxmiEQnS+i/paw1lHAArYQupT+iF7SEwST4g3Z6yT18avjXrAHAopECnyk2dNKd6azQpu8QvV7hUPJ" +
            "Hz1YZBi9bgVfveJWp6pOnabz0DXZZYNUuqfJbWdxjgspOlKycwn2DM4q9yrfkhCzAMU01F1OiW67JwNARvQSYjEE7HklfbiR2I7r" +
            "urrS5wAAz0/ix/yegMmZAsA4NdS7EXVBuHDyLwIvSb+pFOGS0iWutIE1uBF2NmFyPY2V+iqpGOUg9sUAELsAr+8Mrgd5F4BGH5ap" +
            "3KbBPTaXPWqZMTaVDYBQda+IQR3ZXPqyz/QKV65PB7N+jQBtT3vFWSM9GDzGAwFYHgj2bW5rVzbvlAi47Jnxe2637rrpPpstyZ5j" +
            "lm9J2dLurYWUwUsd6ABP1e2aeB9CYUURhVZAy0m1Kt/ZVPxXxb8FlI3r6f02Mx8REgUegDJZV01OVivCp3Y73KH6nydPqieZoI7K" +
            "K3yI18Xyyhnvkjkutu/OsYslp2DQiQLZE/kwe+pRjDyJb2iNeZBThhkONJ4ORIbwtO2B0GUo/zI8WznZTWFL2MfelnbZGeYMdYmG" +
            "aghQbAGhfblkOlIRqDxley+DzmrKfiCxPe45ogZvkhUmeIJEZ+5bjrIrxf94inCTz4y5xzBtBgit6tUYqIzpYHX1ouN7HHzjbNzT" +
            "LiSHu8Qm9oXEeVnKvEn6PbLf9VWSPC8DVWwFU8PZEFPlTL9Qq2AZzHq7bOY4avbpI768GgFG98jI0Cu8+pHl5uxsYP5Z5b2cont4" +
            "UDI2407As/iY3xMAAFzYBB6BsKO5m4banD1hZ+u5bymsASb9G5l4A0S2+mKZWcDdvSUN27EBEIsVTtyx18AcNxflfwoHWoPG7wPa" +
            "220Kn2nTkRslU31HIMGhn1Wl4v1nlt2AigZAbp4GqNSdddBKR4mqcJ1qkjbhPYl95YREa7T/y2GABAkIVZgdklQP6huDiYgD4Jyr" +
            "g5XBH7vXqSzrw/wmwlpYaVR339cx2GQGvIiea4itV+cVtLcmsFRoKE+jRvXKguUwYhRoeZZtj1vVIZJy+bUZn+t826DKrUUNdjYV" +
            "d9Afa6BuQE0E1vK6xzQsp9yofGmNVMOBeqngcVuVmKQtR5S9l4nkMuIUpd+Txw9PR0zbtCtP4lIGspHr3W6TkbTnXZwtErElbzBY" +
            "F4W2oxRGpfFp2gyWXPxe5OTLwbLqziEH0yl30He0V4ItJKEw3MdZRyHC1ZkVFe8ApBORVybiy23CJplRmlvpEmuDPL1eYzwYi/cF" +
            "hLq6BX/U3au3t0BmI52lFiPgDIBxNh0RqgjTSENgjwubT/6S60YpLmTSypOPtOhkvd+iDAE9YwQyswWQWlq0ufMykNzpGxoCDFiU" +
            "uMLx8Vm6FQp0UUfTXS2cokaTXqFh3s2VjQE5wdZGbQJWJtXLimvxaas2bzXsrjpjlfsXLx/aGgATplNwVwmOlXcBAp5mKjDyUgwA" +
            "Pb0sFg8A8G6+c1j0YgKrHVO5JZK2m0pivC4EZhNfwCC/Igz3zn5CxpWQTeueoTZkj9EqF4t5kSfTGFBcHiszU4mnYw98vzYNe2jz" +
            "Glhj7r/Kl2SU+4o8qT3PBJTVkD9p1GcCGBcl6QcEWSreI2/NHrd2hQev77FfZBiOsQhsLpOTCC4YgT0NXkKnbz4+u0cgfK92mT4Q" +
            "AeAAjX/gr5ZANUyhPyR9x+X374s0hr8DxLDdrlizLTHeaZh7LxEx06dpL5D2teJOdKA6J64AcTbQNTrpMle2EHTFcVd8a4W092dE" +
            "eoT3X4wU04XIrZuXEFySxl5tgnqylX8baanFbDl4EXUbz5k2rH7t+d7msJwp3E0OgZtvJZUThLfDRiV1MgJIl65VKHhx8PeQrPST" +
            "cWn+Hfkc9i77txVRO3c2bKtrZK5rjqUoJn0UmPdnfPnWjLbEaSOfrNz6vM0NIKtydauKKE1/PqF4Utl5NGXXUwsH4ub4XZCn24jQ" +
            "lYhlSs+7g2GC7UvogD33VRmSpErdKsoTb1ZkFGK7sKCr59D0yGLXXPxbnZbJvN/iS/H1XVzBphFACd7qXSqoK8DdbpsGYbquXq4s" +
            "LsDODYAt683efnmRIT75RlBS2pwwhUz4n9/KQVzO9eVQuEYtHOiw9M2KA5KDXAfvi8MmUlJWpBIJ+F61mBg08oCvWl0qtLX2Rc5T" +
            "qY1EBCIO8AHEOYgLY4DGe5A+X+GD5vmaTwKSvk8urUo6zIsalV1M/lhUInblNjfjF60de+LLxtEtDcG0T/vdYhg+i1olq6p7CqRw" +
            "IADEiXuTO+pe0Xprs40jYCuTtM7dlezcxaRpNB6lD9PtbrACWNkAmFNI25ObdVfEojaHP+1pxfNBp3U+YU3sAhAfgKEfv49hQSTU" +
            "WMYYiMg6HlHNr+/mKkMzxtL2b+f3A9D4v3r4oNp8Nv4bFyDkO8YwHRsLpguRi+Z4kfE1Gdl50zKuZQDo9cXKDNeOa2o5VmwyPnXA" +
            "9B9Sh0ST5rcnHBlq7CTkdd6V72a0kdIoJDy22jVK10JLV4fRxZa4wC31I3/9GvUpxOY2bIVBqtbnVWjNL4RtNH9TMM+7nGYG29VI" +
            "a7YwCIHGHYBh/pNfbsWUtJmjastYPLYFBXgbUjqgi2Qm0LjToqr/BP0AZDk3U/nKsgtDwGgt63WnYnvt26b8FbNGgzlpv83Mrzm7" +
            "ZkWGAIKZ1zYA9Lp1GWIrJyQiosrxYOKzrE3N1ZUZT8fW1rMM51B8yfEBo2kUqkbAGA60Y12gKbJE9MpyncE0zmTDc3+GgB5buaIh" +
            "0BDV+jzV0H5FZfg6tgLjjUXMpYzhQDRIf8SF5584aIzQnBfRAIVOVJ8vhQX6XIxZ5WkqC01/fDYE5Ieuff2o3NPeLEQyzTuUQylw" +
            "eX83aVZVD8qahoAfWxoAqeW0cmhW82mtwpQmtSH6o0hL3g2ILrl6OaoRQJym80GzGrErIUmqzsnqkaZMno3DThrIhspYOsi3qV7G" +
            "OtsyXrFjqQoFK1d5QWDGAC0X4scu+ekXtboRRGIngDMOPgzgQw/ed6D+BE5XYIwJgiWNjSzfkupWNNlc6O6rRFqs1esOghQaMCvr" +
            "RAQ+dOB9N/ZlDxoGcE7zAUyT8yRqN0k23PS0jteP7mbZawBvd12yQHGycYEauAMdaFpxY8jwpU0pJyufL8FqfZiiwE2tsr0hfgcD" +
            "PyI1eNGyE3BwJI2mwESlvtGLWX7rAyDdSVD+XeW7JshU82pg4TrjNiztedq1yj1SCh2+HfcgX60zCV09E6w9t0uzj8sxx7INr6aU" +
            "ur6QZMYX84J/7EYD1me96iDrV2AMWeFEYAPHIBkBvD/hQMNoABwAp9xyS0GDGEVeZLmmrDUqd6w3KULblqgyeFz77XljjZAXHNR3" +
            "oL4D73vwoRc7LJyUE0MnQ8BT01Kul3SLgWb0w25Wjaja5nUjsn9aYztHsM570RnVW458MufqJchuCXvp8cpVSt+10MVrvBMgRpep" +
            "w58eQ8CYsr71tC3XhvsjTs/S4X0moBoq982yyTsVblYi/4qtPoehVtsISKwkVlzYix1blWNRRMBrCIQSGInltPmMVoM/nPcD8sVd" +
            "IIseArUK3SBuxaGp5a7nLalFGfMliLAIbY544oSBcfCBg/c9qD+DurN4PuBwAA4MpD+oodTtc0/o9LFZpWCKsh3uIW/pVmvdksNp" +
            "hTEph40+W3GSCkXqd96fx12AyRCYTl8aa1uceEaJjltpMPjDx+urrRpptbC1qIpDo+XHU4el9bkiy0J82HO86DJkGALZ7qf1UcEA" +
            "kLEOTzqEg+e2nUPbj0e2euSAJRwIYIfD7MtRSivBDng1RMJaqlMyGhHiLbZFnbpyFLIGsvglL2MoV+PusN7cDf9hWpZeNlI4J4nL" +
            "ZANgPr2Gg3cn9I83ON/8juePfwM7HnFgTGzIGt7mUU0xXM+WnQDLYrUo2DauMq/JdVphNUoseWar3lXXQhfT01rc7Apd0wcRePc8" +
            "/51vfkf/8BX8/AR5K4CzVaA/AAAgAElEQVR2NqcmtFoaUw2AVVGtwfkFxSnZaDZA4qF010zbgcKkoIVGtKf2TYhtpy7T1jUETOTX" +
            "dwWIl4UNAOT3BDAGIn3hYLZYqAW1mh5yqjD9XlTlAS+Yur1QDcZuUioqzju5i5oZADHatG4IyMiZS0leuIRitrIELcqKQhsz7/lR" +
            "2pBaHk1dmVsQV8rGpnn0BJLSMvnndFG4p4fzPU5ffwWu36M7P4MdDmCMjY8EuBR6kspdetHrsZl3lGTDwt8AUv8JJdIvOoS07fd0" +
            "Oo/PSDGNDOVdCgTw4SxCgoYO/d0nnD79A8PjNz/9llqWfgrnC6fYkSK3J+snsUvcvVirb/16TYAIPzVjPrcE1QSF616E3AkOcTEP" +
            "VJC/cz9aemRH06UcnsY4hzyu8aldJFcn550C/6X3BFwrNwzhLHmmpm82Ynz3ovcnbEmZ8IJZ/F72C87+Dy9+rWze9pM0DG+fJcEj" +
            "lULjLN0P6u8h+hxkLD4WOW3csm0t35soEZa55KyMWb9mIFZ7dfeRPae5ca2XwrQrthrijeVYq7ARIsbDbqxJkoeEIcBPDzh/+RX9" +
            "+YTnm9+FAYDRI64bAZrxLD+k4Hdv2O/6jIDFAe8zAHyGoU6Iq66lccywfM1FdNmLIvNDOnKVnx8w3H/G8PjNkAI0CmLf2KkGU6lW" +
            "ElfONEdo+hWSnRE1xqZfFZndaZMwdfRFr/kc5VsiOTN57nkLDtM353Mkt8lQvf5ydshZELU82QpkwqQo9r7WQMAQmNFG+XdWp/3W" +
            "woEOJMyA8p7zGnW5Fp8rn+16bP+PmJT/LJpqYAtmLTYApu+OgkLjLOk5ik6Rwh+WtBa9abzAogyBIEqsxJQHffe2gDugR7CaPew3" +
            "AOqjhRkfDx/7MullVsPpHt35GXTzBzi7wqT7s6kQyBdM5U5X7/0tlhVL9yuyVDV78rp4XJnW3DZiLMoWYZwPDuotFjkZknpp16wW" +
            "0gA29GC8F/2tn3WhsYefW3IXq9RyaEkVSuYZ6F2LjGqTX+qronLCvRV0UsUVE1FwAlLE254ZwisoPfcMWCa049b6iJv3a0LvDu10" +
            "IK46QcLLSnPxGFtHKi3S2jrnj8uo/d6cyVpg/QYZ+nkBc01Dkpa9UZstisy2DOOrP6/DZ4Ux7PKqJjMuCXKbVd4kgIaR8Yf5vmLF" +
            "yp8aZh0aAa5yuiNt/lX5UzICfCDtR8AAMDnCQZ+1QfKLwByNJy7+wBf7IjDl5CK2nqHVKHhx65KJWn6dFgiSVU0YqgX51r82vK2V" +
            "qlRea3ASyono16jS5HJ8nVbUoduviMszAW8gdgIOk8xk89CG2p+lwMsS15M21D1ZtHkc19mVlWBXwtpHTMh/6Ckugs+zDQFLHbF8" +
            "p15sCKWKQKO85NQQGK2ETly5rmHdwzSYaCulxWzjIuiIZF4XzwccpNRMW8Xd77vFfAa+5Aef67dTpVDjLNdImcIyEQsnm9L5TizS" +
            "jYspi60CwxAZdwx0b4+HZt2octKlp2ii56Rx4R7mzlbIMQRq9VeOzmIQksM/xhQwDYFQ1hBZeQRZySlERkGeBpD2mVtO1H1LvfZC" +
            "wtS0MhekZwJgHBgq+28UAiwKPGAnMKgDugzICTHKo5YsVfkPVlVTwq4trRUPWUpiHYXsF8nBxrsCUjg/Na38ZS/epOYGgLvKNbtg" +
            "z0pLLdp87Dg554VCy2HwocT/zncUKEklI8JTr03PdqrhMQyRJfSlZJHGOI2J5xTaYkTSfBZ9Ku1IeDYbjHoIkRF7Yy83dXTGuCHX" +
            "mUsxa3wuanSh9X1Amn7SAvpcy2pLiXanTOB6amKpI4SN/9iMs4BJHbxagqolVmWseEMAUSnjoYUDTeLvYF0ZGKCe5tLANDEU+gQr" +
            "v6kyU2PA96z5rIFYQ0BP1soQCFqglRA77hsYADXQsusuHf6+oTmNcxnULzuNetMLqLCTRzhmL/heCye3UDv0QyGkO/P9WfMYifAp" +
            "8942x3aI94zjdbCmATB9r93iWmXa1o2QAb0bVOvYnfGkRo6LX7emWvcJxmfIrslxb/1eUB+Zms5dm64qQhV2+htLoaIucbq4wsmr" +
            "1L83JPaHO3MLaBvwenXVqve4BWsLgGKakzYui2opqeFFzZFGsCrkM3RfouePtE/P6Bk7tx4EajVrCg16Rp2htHF3ltI8Lxoud+Kn" +
            "dq73oib78ijaBDV3zWoUHL/EFVBetI62Rh1p3Gxco9NSk771Dl2zdT1hKzIbPvPKD8dOAMDYAQQ2h1RO221WW2VrM86ARJCvDzSP" +
            "j68JDZxbFqSYIZkmS1EDKgy0SwGybRvq3kvPlq9SdHBLyOyElDU9qlQbrZ60bsQfhecbnWabKa+IhpU/orVqCc6k2o0AD2pJo+vz" +
            "lpkwf/SksbTK+ydqzuUak2/pSSX5QLA4HFKImWtzZPCW474ZEmFr7wCsUXfRDrCWLfdupUqcqDIfi0ourDtjTNIMAarCA3F1MOnf" +
            "yDqdjdEzWZSX6EpykceUB0A8GKxeGYtMMWAaSYa0YhNMdJIW4Qh7QcrmrrIIKSJ3bfHcqo70SeCy3q1UFSgkqbAp9VGjlLlD5Urj" +
            "45bSckJ5XpEGNv+jI8UvH+tL98NZKiXUkMgIOVST5ZtZolayb0cAlh2DLFga38iSvjQpX1TvCxIuKeyQxjorGgDBRCUwzftW9bGx" +
            "PqfGGKV2MUfC2IL2wdwHQDwYDADKaxj3qg1U6j99MzZ7GV3FZboPZllgp6dZVwSav3Xv+Oq32f97BmmfKXleUQN+NXdVWAjIdGyn" +
            "VlM/59admVn/3uVFNBLa367NeSXvfgxiQhjWqq8Ilp0854VcOIjPKn9roVIH6k4A0zZGHR0TZS1WhGGtOQyBkiGxBYjkKETtwKQ/" +
            "G8q9gWlQ6SiqOSYzs37dXECnCKsSWnMNjSgjJOA4TcWexueyoPZ8cd/lbigEREnQEKi3QWEgbVNafqdAABKt+5D3C5hEXBJP7KUh" +
            "uSE9NfincfRPyy5Oan4lQ8BbZ+54pCxerRZQCXIzysdvL5MsAVo/am8M9m+MylFM3oimJnFc4SJzq53y2QyBJcWekekRK26W6Lns" +
            "oU7NKIXZ7WFEYsN6atHqL0e+S1Fyl+Z/ymCbd3sYHy+yYv9q4gIXDwl2eVkjcRipRZExHzwlVKa1FqbwhZoxjoocaIXcwqmiGlGp" +
            "oQ3UmihEj7ovYSLrVJ8GvjEw7nl6ulButxu/C5Pnkj9Zf6H6kqBGJTtCyCOqs130kCZOrPrYdkvcr26+XMQaAHtGLbIvTPzFN7xx" +
            "7Oul45JaQZ5fOvKHfQ1BUF/e75nN9yZadx/euXZoUDO06tndjdjmGJ8JGAOC9HCgUjhCOPaCKjRVC55eY4ZmmTlRpaVnrtjepO3G" +
            "lbFn2i5qUUjBCtsyCfAFzm3NAiW4JNoNWj3yp4po2vnc2rNYaoYLbpiHW1ekYk2s0S47Q5Dtdjbv2NpRHoxUzspTCQcRDvQOwAAA" +
            "dKjf85KLXd/1WXt7LdRxq/hxtL5Qf4TCjxqvTuT9WQe1YlB0+JgpcgvRtmOph8BFl7sHZjMq3GnQs45grGFBuS3yRPSnL3COjf/6" +
            "d8qbimajy218v8/gSD9VXlppDHqwJcptLNmkiAu5cSoJxFmS2nLtYUzDrXJxZAiWxu9dBlpgkmxe2cM4lkO0q91QBZR/JWksFZZY" +
            "Er/wCZQXxhQiWBqNIYUDXZuXaoGZP5n91ipoqWLHgpw/rBcc19rhImPnSphJWyd0Hr18A2D83Pvi5xUMKxsAVcsmOx9JBfhZqs0m" +
            "rY3PXXzvpn8PyFWIyMjKtNt5JKQIgdxeTQsN2vP4hfeofSayD440e+2IaMT30mUhfh6352d1juryMZoCBoh21R6dyVgqK1c9IrR2" +
            "ONCKSCacSR9MuZRXXim2CBWqEh20M7Gzdw7eEX21Rm5HTbJjcwJ3Nkd2hnrDU/AUaoVSsrEH9tgDDU7smrg6SGriS+2P+Ha1MqHj" +
            "KEjeHkyuYU2obn/ldCDK1at3hSBNFYjOtfGMfAqvTB4qUtKSLV9MJY4MpNzL3ZZOwBpMkltHbL7cXYCmbffFGU5hQKRfqVpzdQRC" +
            "GFLL2AaJHur8m9HYbCNqlTrqGQLznyqKg9lFPoqT1cWTp6KHcQOhYKM+zC4+Wbc/1OWBiMS+CiPrar2Epmbe1EFrgZMGc7sgEi12" +
            "CuKgHhHKGDtAWAYMLOv5qL2E3OkxrfZ4L/+lKZ8zv4YpbSqUfLpV4KmYAPsr7xPqNepfY+RiO3SLOgqkn4/PQsW6SE1rhsvSU+9t" +
            "PTfDkKRIRN+VI7ZHSjxA9pJi5VKovGDWHJQySoCYKrRaCymRxI4KRlHMrH2SObeqTsTcNluK0ZFSbEKb0pX/zIo8aL0UGWXnPnUu" +
            "Z4vtKBdLeBabEg5qN58xLQU52YJIGhGLY6qcf7ZdkdWXhWkoiblay/kTS2OIgfRY2CQiKiDbf7VDJ1AU9rwjkFGPj3dy+TOLHwNo" +
            "Mc51u5l5frVAK85fX7BX76uGnX8542ruELjub2cANC20CfZAaQv5ugoadd7mBkADpNhLuYWVtX37maA+E2B5Wdj2JLpRQpv5gEcd" +
            "IrborxxDwBM00rANZP3aFCtIJ6+BubV01Pp5a3K8YPrPvW0CxzLtNlKzSa179wJtUIxN7U/ue2VLYc+rLJqwfVXlrEIJNePK3Ykr" +
            "7AJEVKwHE0RXswYbriFPpLRku76HpaQ64gZPjv+fusEIB6pD0D6gb/UxwHzSQdkGqLM56Dt5rhXItl3GbNuRy73lQkYw0PZur3i0" +
            "2vN1jPEcRpbIAPL2YjHveNob7I5QXznC6HK3Vf2397YP0oCRmDEps+u0Tu9SuEIKmOd3brkVkCLNZkMzcUJU4QLmD/fMg73AmK72" +
            "hSTa/TdlZ1bpMsNU2Gr1j8mc83Kv05NY39wGRz5RZkGMVCitYwAUORASL3I+siStPU9z12OdDl8ZoTocBkGJjULNg8tCiBfCByyU" +
            "eh4M9qAkTqgxvKQZSpsWBMRQ7rqV3Olrs0NwG8eQuJm+/0syACY0EGStdgBaGgDBOgqCKJW5p01E2dZmTPtt+auPOnzYVvQluvxi" +
            "0IpYmzOlJH8lpDsMd+CgaFBk6nzK64Xyt1e46bNpo2a++D+K6g+WM8F9nUC+o4ETyqmRJ9cWaeRAq1KGdzDzaEknjUnkbK0Ypw+W" +
            "95kAK5jj+8qwjX/amMv/mimymuZTuldC/ibzTpT1GJSY62uQsLUciIRB5oq7Jevigni7BbYfgFWQ38yXYwi0GOq9BS61HueLmy6x" +
            "oUF7g9HRmT2fawjY7lUZ/DU4yGt5enPq1Ek7AR3AOAP4bjnHsYMzYzODlTyVN9oJDJXTbAh3yhvRqDA/dx00lzs+Fcd16+5pWX/T" +
            "uRVAUbu2HhQJe5k/ly7KklHQ4EpLXEJtrdAinM93byVmj23W7pm+gSGQgppKWO0yCwucukjPfQWIB4OvAS0ciDsCzXcARxTL1hFZ" +
            "TiTEyyaE70WUIiOiRF/le+tYF0en5JXhDYj1Z90FMseHjXlrD+9W/bTb8amEVQ2BykzRSh8qKTbnUYaLRp0FJlBsSiXxTFaP5Hqj" +
            "PlPvW48mxm/95G3ssh+51m0HlyYXWPRrKIA1+kDWT6v3aX0eGsOBxvOB5AeD54bsTPvLDGPPQdXxW60bM/01e9vzjUWtQYrYGtyN" +
            "jKwJf+jtReEltGFXqNmhOzQAJlySuKuGXWwVb2EApNftQxpdrxIqHpkxPi96zc7nWb0fuFTaAWthhdEwu8hdqfeZklae8C1WG+cT" +
            "PgH3bxWjYIUGt6rikoRJpRCgpDbubPt51+OThJ1Z45U69uWMTy4uNXA7BJVfyXnPl8+P1XmnUQgv24IHatlknnt7ntvJ3XrRczEN" +
            "07ipR4SqpwPVH9vI7ZrcXR17PqkZ0/mgqQoeiR29Krs7W2xXOQ0B2z3mTLr5DGldvbVvKdDlG4rABk63pLnnmxDFhmN8v6opMyve" +
            "lfDfFTHF2+ytZkiNcs2t75Wx5bM4MXVHPxRn/mKWe75qbMtNG97Rw0rMNS+K5b1Ks/0mc99qh9TorASZzpw/IlES1rtXhB5ebVRl" +
            "aBbK5MhHhKrPBKjvCWgzLAmGQDMiXIzuGLSJZOacII0CLV1VNa3G1ZYXbgCMSFco0wdkDhENFVtyPzct4jfClOcJAl2QzrI5jN7e" +
            "APCG/pYXXxVVxcTKD1xF056rsGyNwr5coy2zwppZWWpWJW0zXnNJN+bcGbWS4jMWfFo+rT6VlLqLDQEJRpJMNYhh9M/uwRDYlZBI" +
            "g0H6uEBPbkzXsK4XDiRjjx0da715veN74OJX5GIVtoytZI9zJBP5TWlo6RRm+y6xEk9WqUYv5HU+ZdUxGfovHiuEzcj3ds+Ogf6o" +
            "RX+803ylHqvF68zxfS14LGr51hExLwvbeC+3SfX6ADEbM+bu6dThojwv8QuV1ps1K2bsE2LLUsPQmPVrZOb2uHhuy2gAebLZroeq" +
            "2HOoTDV8F41MRC7tK+tCJXP8kodnQmr7s3c+GmHPY1Cm+Gdq2XvukFpw6BmupqvPBDDPIW4tOi9iC6jcQJc3QQj6OXX+qKDcRqcE" +
            "C4RL8UYgyRcugcGTpOqKYVZOVBIwnlAzwNMt0hwxpssOtlDl6tuQU4kHVuqnnPGReWDOouWtMwNWYJgCQr1Zc8rdTGwkDvxOQc4f" +
            "DvicFruBLrHiUvqgy3CG6RHgcAmtZuTe7e74cjIWVVeyi/daRcCpZ7jX0fBOQGtU5FazKM2fEdgibiu48rs2nJNKq9g5LqBhFl5i" +
            "lnsuHotVgBi0tDtZbXP3zeJRwANbsE9Jh0h5jfHehKDEKmpn3QmPx2GFft4rHMvsJSJHZJi+6bieqN1fL8cA2KCuS2beTNqLngkw" +
            "68yjIuklMiwjTwkuQP9sgwbPOrzUvlzVmNwGL7FNq2FvMQI7G801DIB9tfjloko/v5B1YnfTHngxfRuLsuZe0POeBQw0vixsKkhX" +
            "rUN+KDYq4/Y0SXSl7Oww6buEKkE4TcfaF1HsxnYLWB69Slb5LxkMYR6sg+waAgaA6R2KQErf7eDEluQ2Z/ODK2N6YauL9JjB1+Sa" +
            "R7RWQGbBWVtZmVRcpOZeJPAuGhdrAFjqrMN6oeOlW9QZgT1Gqu0lmtl68AsZX7eBQxeK7jh7QvWIUCUcyLdJoEayTWfouyZTdDgh" +
            "Q9QZunOZFgWMAt+TID9OUBVx5souJkUOLmz9i+9nrWFSvE+FyEVrFcXpKiO2LVHpsudXYoCyll4OEkytPhRe6pU1gcp0mRWbLx8s" +
            "oiubaUirtPFiZeiF4SINgEB9/rkeT2xIZuhpV+kGh/DbQvm31R/XB5HPi6V0aEwHpCwcukcsdelyJkjlEnehgZ0AX2HqZy0laLUw" +
            "Hx92oMTuoRuysIO+S0G5AVARF9Z3LwE1u1yXjBeBjYh9NQBeDi7SAChCW2K32hHY03wJ05IZqlNr6HJ18FDDCr36OWkPgNgJKIc/" +
            "NCixKP9vBS5maDhRI4tO64lcesn7c1VclCCvJ/T2JDwvDs14Ju5h+dyxSxJRBeXWQ05Htxmc1/mSj9333X7YrFnFuQ7PXYYG+dB6" +
            "XDzlXxhLJMCzcqw16Fo9V4DYCRgA1DgdSIQGhbZn4mN+lNAfZzaS/oUvYYAuxA8E+YPY8sYzLYbQU4ybiMgdtOg6iqvIG6sp/Cxq" +
            "88qC/Stte0XSJIlL29oQkH+yumNvtJApH1lNqxkSoLbVHhbl7JAG47LGTq+3ilb1r6C1tOy6En41cFHOoHxDICckUNyLqzNVDviD" +
            "UsN1WdEiJJqsX/eJDH3J6/RXhK5UuJG4StxQVFb1mYBcjcpSvrcJjAFEYcGjFxQoOGWCGdCzeYuRvIyVlYqq0CewvlGSW2GkBR+u" +
            "oswAAITdmsq2tby/3x9SoulLIu/d8Nm1G5ATDGtJqUoXdbVhE6dJGTyIWrIyFlLvTVulMfcKYa2ipuXmqbM19Hq2VNIa6p5VEPKx" +
            "uZ4lSJFh6dEEDdbCmjJz71p/JX1JNgTk72SkcjmTGwsUCcH3BGyuMLXad0+sdsHrFvnmKBiC137ORYqVvO4cCY7p3heeEWt4elvV" +
            "ESq3ugFgSxh7rwC+Klp17pYya6u69ySnq+lAzC2I6rV33/rJnsbVh7p0Sns0SXJpnd5KfE9AyYaTWVYMvLsE8cX4Cw/sLiyfF6JN" +
            "tERCjHXsLgB5/mzZNjdMUxHDNomstW5bXMTZ9nVtaevMGydPhDLFXNsJWu0AtK6jVrnJa6JvHWht+ayAPZDuo2ENfm1W7gqdu1RB" +
            "q9UZK+BYfNKdITIUIaIEI7kjf/qw2aTueG3yiEQV2p5hrtT6Fn+N2IyYCJc3NbAcCSoji5ky93Vr7pQEw4siErrKjBi/lCGuyQ4u" +
            "BdtZWULzvXRSWo+6ujLoeYwoO63GfMjhSzXHMM7ISq3RHBX/OC13GY2/rGExZRNWtjFid2a9YkKJzYzE5ruQ69aRM2LZ8qyk0Zla" +
            "ahOjxbekrYSUcJPoMiuJxa2NGtt4xT6iCPj60e/ljwkdc/VNQdCstQajHs/YttZPzH6tFM4dkWQxBKQVRWqEqvXmwLMCJRVcU+E1" +
            "oR4ROoYD6Q4Um7Cua1W7Vbyaew+5mBSYKEr0ODJvuSk01IVTELwAlBsA+vd8bNOtddsRq3DPn2S/V0yDbeMhhp5XZKFl31UzAGrU" +
            "f5EVrFhtfbG4K5SHqoU7pURfqumOaqVzlNLYKtIibBC3Zu5ao9dOoKhHhDLGkiOEIpG7JV11sSfnj2gktWNrDcRDbLWHP1KQoMTF" +
            "o5Vv3dN5BVVGBNZcPFqwUuq5ZRGRfq+IwdYy7BVJaMLvPrH4UpCt8RZ4PF5qX/rgaHOCmzUJvhgT81eqVvoyVpmrcJJ6mDY12PQD" +
            "WL//iCTTP98QcIYkNJrYyZS66MjcOq+1IxVxJlQArSWnu3wCq1L9GixfP6jJDVsdU2hQjsc3FGpYy4HwikiUzP3QYL1kRWijNa5G" +
            "dYb8eMnjNCGFzxncnonkciLTtkLJHM3gC/K2WS2Qxn+91UT2n17t8tvtHSWEwk+3HrxyqEYAEQF8/NHmRGfrtlh0XP4IbQcnbh5J" +
            "Q6kYArZCw9iVTFyLmBr6O1l/JFS+InY1yPHQNzhbNSN57YggppYBsAViaLvYJaN5POILxh7aWhBqvQfyV0P0GhdQPPZsCMQOaGXl" +
            "H0hb/qWnzUCJoUzZcNgDcfNgTddbPajPBCjhQCSPQVuklD+RVWnSXOyifIF47evLRTURkMIEOzMAatS/dRte8Z0igfFayemXw/tS" +
            "D9VSlNd25O0BSbR4Eme0KZQlfQ5c7oMz6jMBIWzAQPbuXKGTG7e1WvFbTOpNBEncmO9Jxm0BvZeU0LuKaN7Pe5OjFRt8STzagtZL" +
            "av/3iNfxyYc10sF1L5i5Mi54YJuQTpay97burIArQOwEDABsLwubsTsGcm/QyDH7RWNaqc21us6IptkSm8S7+rfbtu6SNRHT7UY8" +
            "fuFYNevfSMfaVmgRGGkrcq/rT81ohebjG5BLe+SvPaJmP3nLqu+S3RwGC0oTKDjvHZOteDycBfjimbZFNAU5PPLdxbm5oR4FxNo8" +
            "B9AO5ugnRRbFJCrskZoGQPVCa2AntOyEjN3A2h977KSdL/JrSsQ9Ds+ElxAKtXX9rcCw37YVGQCxaXaK2DXbt3tQn5BY7FwwR8Lq" +
            "5fc5nS6Y33IxhgONTwXMOwFs35LFgeoGQFLCdmCO75eGerSrg9KkTy64oy+Y9A1QZ4JfmvskByVNXMWJ4RGUL3V4Xsra4MUKDVv7" +
            "odPgrSbeQx2BE1lqVFENeXK66FCzKpNrBV2lENpLAY5mij1S7cDq+nqgb5p4z9hFDYkJBiwHbyVkMTAe7vWdBi27SLwA0pfhrzhh" +
            "y9ptJyS6zO/oeYEc+lY3ALRre+/TXHiaHJ+hIZzV5TgY1zYEPOEzRQr7moaANX+E8G3Y13lF11sogvXruwQjrzq70ltQ4boSUVYN" +
            "qEeEKuFAl7cVUDN2FYA7bqyRlylFCMTUuYNNDANsOloNCBMYNLIyj+S6LLZ24oU0oxpaGAKhGtZ6XmCLgn3HoNeofzPjoKQYqU+2" +
            "lK++ZoUMAQIKiddPW0+rPxuhvBUGxMaTZOkwYwmLXdOmtLFzi63MZyvsANjyrXkAzNT91mFI1e2iVRBVmdy6D2RIR4ReL+FAa+9t" +
            "b3HcJ3l/bq9Bf+8aXqtdlu+9X19RDRcVApRBa9X2bWEAVMLedt2aOIgrYdOdiA0MyTXWodX4bEOa1h46/XrbPtYfRojdIWivhA7Q" +
            "woGO8nsC5o2AtvEnejNJu8pkd5S1T0i6TFHdpp/oqgyNYdn5S2y2C7DTMvcA5vmVkPFyEWRy5vhu/nxFALs5jqsALb2vLxzVQzQK" +
            "sY/15vtiCib9+72ANbF8QnXmVhSZjmmfRah/pN1Wfucr/cJh/GNjsEXN8Q5525WfpJkDDluApLSTRu97nxjB4+GSGizsH/KGGNXo" +
            "m7WcI7426Ju7MWnzQ69S9k3duW2/vPlYHu3VQ8zgKNDHk1mIEOLWsnM4cuttsxrwtFsThBelDhQSO/mBPIdHmxkulB10+eZNqH2V" +
            "+ydWTqbI5pZOojyZ3n4WpKxNej6gxlqll8ks5ZB3HG33ctbRUDmhctXf5irqD/CS7jUedrM/AnI5skwjuWUOMwAsGKLs1or3tC6E" +
            "eFFuxtVfIL0sTH1PgLIFUKa6pRoA7rSy114xBjII89YJtwGwD29MvbpStsZ0H3O+cE3PHWsAuLx3Hv94sN5qeo2rIJtsaa1MjQ2z" +
            "9UX6yMTtmKU2qVTupNXiwTg+exL0QVQklsUYAvJqegGGgK17cruMaW1OkTUpsrkF6sn0enS4rqXKbfl7jXbZlVQK1uFrR6pTKqVc" +
            "816+ArtW+GNUf2QMplEuc1+T/+bqmDnytjpy0XruzSoGAVy6LmJ/3k9UMJM/6jgAACAASURBVDZuAyxG7woLX1wQjzVje2gK5EUp" +
            "ASugqD9cUsVyOdYAMJK9DpgXru7ZW7e1oye+5L31iRcNiE1SAnIt7pXQhKQdttOJS6I1BVu3K3OObE12iIC1n39qVp1T5fBreCXG" +
            "aCxqqbO+crjlmhIOdDwcx/xjQNDc0opNTtgS8GwIxBUdAHP+mC6oFn7NgfeWtblESEOUl9CXOem5C+a7uf+uszS3pi2b3H5P96d5" +
            "Jvypc9poE7x17f6E3lpjFaiFlrS5BiHBLeaXu+2x56FbFZUnVOmaVm2NdeyoFjXXm1nTFVJd+2ttveRq8lvIk5r5pLTz+LjSrYr6" +
            "A+8t0THBFiPgevoiNgdEDHVlV6rPAPAo/8Y1sn+P4QvvPJiaS/IFc6svqUfKXOWJ6T3aXGbWWGTJF6Wf9QIj6oypw5Voy3AFaY+x" +
            "iAxL24x5nhJwmnbLmrrWljsAK+02epxb8D6DK9d4rCUOlX3mRnVEFBWh15jl5dBXRQ7aqW2zdpcqTKUzIVdwOfJVP++1QTEsc42V" +
            "EZhPTk4qlJMy8Uz9aYclCH/TpSrGYM8pL6VfW8aKzg8FLmHeqnYnFemTfTIsYxiPCqMqzelwoJIb0ulA1ximcKDDqA2zigI2a9ZZ" +
            "UpLtYiTkyRaYeEsWlUHWMxYrrbR7dnsxz19E1qJydtAvTo9EDDx59RjHNVFaXS39cjZKbcr+XgyAmmUGqklK4+i7LAKKhaa/s3Zl" +
            "AFQro4HgahjPsQNRuiBS/jehOUWuJBBQldaEdbLYAFgLwfpMZW/+ypZ7itjL9p6E4M6Y1G3RFmYY6hGh6oPBRWPpJyPTAJBgo40Q" +
            "ZyMEa1cKL7DYNplFNk5vXOXKYPKX0q6KyF/TE0NQ+XRr1KajtLwcAWTUqXvhEg3MTbEyfba+8/6OQdN+VguuP49qEr43bagNPVXG" +
            "QCOt2rg6mhzkewdSdIc0PaMxMuZki3GNTVtvXud5fIqHJsNZ3azNAViPCBX8wmB5QTYYChXsFZoaoi+qi9hSUqjN9rzu+tUkeexG" +
            "Y1b7kVYsT6iFGqrfl39P32UbRL8H9b4vr4+EYAK9DVMmGz22QkdaaN4z1IJK9U9LlbHw7gZIdDP9WqhcoxymluNKO6W3bb/p/dgI" +
            "tTwRwdAChS8ZlGOJ5bYm0WB6nXyE2LxOxSERWjk6acmVePg+TdY56HHNV72QuW6mXCMbXTrttt/Jgj0epfI9tvTUbCTLAa3/ydbv" +
            "SjpSr0k/ibHlnT7BdYTZy9cQiOxxwi7PIgba1h8u3vHVnyInN3FKWBRjR//QKBfjdCZ7B1n7Q5evuUJPz6fMbbbICLloSx1xYqDe" +
            "YPmbWyKU0mlUjIBhflmYskEwG5Eh0loYADFyvTZEN/rfEWDJEARp35m8RZtqMY5ZJz7PgaHsyL9d0O8z9z2FNlu5nry269HtjCjL" +
            "qvhL95S+kYUbg/GZ+lB0Mu+yDF1ULyC1HKb/IMc9tJ2MNeHkwal/GIyTylI6PeG0qwn6eBgyIqH6lHqTC7bwvTMdRbQjZu670vmu" +
            "heRMTN0V+DnXdqxVvw1zsbocYJYqrf3okgPjismmtwvBsUizMP8EMNMdYzgoMl23WjyVMLWv9Hu+vNEOjLVlpnchhkOBZ557rgrU" +
            "tPb+kJQOe7Z42Ay16YukI0202JymrmqzZS9JnwmFiKasawAA1nAg9RAhtsbZULGTqmY1+svINBg6kPWG5Xckai3uzZSEPaBWOxPr" +
            "alnfpejLJvbOLAFcOPnJ2KK90oIeq+O/YhsUOxRyktZihNhyahrBFapco/7N6wpW3UAyeD0M5lVmo8Lj46oFyfSZ4fqeX3o6LDsB" +
            "gGoIRJLmU+Qz7wF2A5Ecn8465C9Mv2M3Qa1lZvSzPPBGdqn6FOeq56V1abS5m78L5Mj67KZYMi5d47dSU7qwxOGRj4Vh8stxUN2Y" +
            "d6oJZGnOMOjzcZkITsdSVB2EaTcghR/037k7485C14ImT1rpgE2xlSzMlMUpzuY6OrGNwAjimZwuHyGfpJuKtI719mtMU2NqjSyn" +
            "0nIfKC1zXCPSMms6V501sNRhtFaTT7qsndOWdkcCHPsmMRmkn/lcYjwT4KzJB59+kHEvk4q4vFZDwERwAUvs75ntNf53vabaVTwZ" +
            "X7T01oweIyeqg1OZS51aphDIA7N8c6fxwb8t7FQCKWHr0FNu1lS1ZEorx93vxdueFwAGLM94wDa/qWx8JoxeDW8ZEYpMGI7ZUHUw" +
            "g1Io6VY2sgbGp+gw9WdmKXaod9OMyFgK4hBSRqfQnTpDtpTi58eY2hxtjuyK0oOKQ7mZixbL+hAkOUIfrq8yp5TkGNeIct21xOlc" +
            "NUZxOt2RmF3Xsu5LWJyyc4njIlLXoZGna7u1ujTq1JeFkbSCMTAwj9bjpqT8ngSX4UWOzxQoeZj0J92Yn2GycksYygRmcd4YX1vS" +
            "mS9uwoXLyGH7Mh9GSs54Qek3AKayjFsW70FMFcFyY1HXHTRj7wZAzSbrhoBeh218crm+PSyLUfXBtDFdfOuqjB1p36ML1RNrBSWe" +
            "z1tlhyYL4UbHmGp1dgHWgmeSaj897q1shPrDWYOmaCZRYhE+ehv3N05+5NBbe/eDQABT5aSNd5h0wV2v6rOvMx61DAA9YTx1jnAg" +
            "AGVPKHjRqmB9vfDVSRATlpgIfhoI6EcLkAPgtDwwkUNvUGxX6gSrsura/8qCzZsfoie05ER4F5xC0Z4j3gjQCiZ3PnL+CNSV0d2u" +
            "LDUETaiMUt2xxXxuveDVEuKpXjqqVnm6Nyq8f6Z78mw54uVJdSNg+h3tXrVcS3A+heB2r9SaEXnKvww/JTVprYVwq/xmqdSmEgdm" +
            "blby/owqOGXeeMsv3EJYQwYrdZX0Hdm4mZkXic3OIMLy1wMYwJTr2bQY8LkQtp+D9p0AD6p53gvy+GhIHUCCUPgHArqRAY4kGOIw" +
            "JlCOoNMKTPLEWQlhxjevgImV/KNLwqeUW69GSRVfxTFZA+WM/R32XiX0nWLVuhUCg3dCzJMIa5ZQOb7GBXaXXEWFroWQeiKSkjex" +
            "7pqLUSrZzXYCPIkCYsJySeNn5swSqMmRY9KpWCBdXGniXuRAkPylUKmxF65JElc/B/VKP1cr+aLHx0qJ86rSV7Z6rVf9VLR0Tjhh" +
            "WyusDiGZJyXpHrvLE5qDqXO0IB1gV4Rrq4ipsteF3Lyp618S/zkSkzzHaZnIihFADAMBA9TrarFqrS61PoFi6V7cKMfzwtjAiIN9" +
            "PM8E1CCkPlLrNtIzTZcm4fnvJSNgIPOgVEUouIS4R0lnnl++lEvltjrIuL2kW4jUU/knkHk3fsIzDz16YY6zh507AK5v42/bWqGt" +
            "vAZdjsWSpglkkuK5mAGPEcL0iw62YNYMC5xKP5N/B3jOgsUI8PCgL/9EgkcPbaZoEKzRIKXGVLgPllyhtE5ejVAgdZ5wp41oMUm0" +
            "jINm0m5ecZWcFU070WCbB4Vz0SbDlXUzNA/cJVlTxcxZD3X+uw6ZFT86JqwBAWQzmirO1uh1FNJcHo8njbWgQ16MoNFXzxCw8mBk" +
            "2XOWBIEUPFbcV3+Egy5QtPIjpg89S4T22yJfFb2GFANx8vpzCN1vAMBHPUH80cImnga7xJKHjS0IGwJJom62YHSBpoPLRkCnhgNV" +
            "Rm3DIYd59BGZBmreCeAAZ6MBMDG6PjY+JTU4jr4lMND1MjNKjO2L2iItrU6cKXyK1AWFTGch+qTUK3AWYI/rm35MC6tPaY+5R2OB" +
            "siFgRQNDwCvsLLwr3/N5Vo2+kgoIKmWuhWT+ZzL84jtEn0523m5kBDCZduttf2bll6rcBSuOSBs/f3w1wH2keyLkvnKfihKcvPDO" +
            "38j6rRlj508A1jniKth/2Vq6+f4Jps3ZfOqV3vf0hw/utCbtzBiPPO7y5ootUl9TjBdbuc1RnyHgkxFhIysPhnIbU3BAnoXSep0J" +
            "U1qLURy7k6eDdDnoeklZoBzTEPDJVyaloZl2PqalUffrSeiC3FaQodHbe66MF8KGQD0sDQqGA5H0FyrSf2EPMG0zwmIAAMBhVKiY" +
            "nNJteiaKP1E/U365aQuXFtYIfGqKtTYHGYqy5qXJT481JVOT2ElQ+04p2melU3hqyVyvz/umfJxadlBihy/La/akvjtNgdAiONtJ" +
            "gcXUh9kYgfZlQcqsiO1Sly7r7weZM1VlI1zvkjeGRiNNoBO8Smw4e7h+WOaG5a67HpPP5HmpmxFJy6HPyNfqWyhxJ/CJqhAZUe2Q" +
            "tCtxn9RbiTBMsMj+kOGbwa57NYx0n1053bPZffK6qXi2JXkyySYmzbulTFrKoVp9lZfHuqYlVpIUAeXi8wQaksd+5kl1lriqiLRp" +
            "pO8+/YnmDlJkNi267aQDDkQgEtyh0GAVChIXkl0PiZYJITRUsLUHgwc2WUdmhW7VrBFtYcTWNfX6rNWx5Y3SJBgAHOCjF4FJ2Yxy" +
            "lK92jUVhn1kosUXiWDLp6gEpd024FmEvfDPXovHmCPmk4Y+sQLfxQ4aAQkOAIKeyE1KAp18O6Rv1ksVwMeGxV3jSzlvzvxbjNV4t" +
            "tdAR00eSZmIzQ93+FM8eRab2Ed9KXTW1lxJDhsEv0WkT4VjYa85hCtx3IWb+1kBu/1nlSWTh0XUmWNneOamVE3saTYjOuHWcYPez" +
            "k34hvmIXUphE5n0H3bkkREYNOTI7UjG41w39t8dAmU8udBSk0B7bnxLdJfPCtfyR9K980ZlWL2j8zay/7TqqdT4phjKBT88EEBQD" +
            "QDaalivaQjqVRe626HUr9JvUGXTK7Yji5qCut3w6ngmgRWc2rnpKb2AAWJkppR5Z21Em5RL7BQ5wRugRMVcUL9vYH5LOYAzRfG/x" +
            "SJgw7/nmb3UDQC+GxU92XzHF9EQkLVUovAqPpyGqmLAtjBkE5DbGvfrB4C2mToncar0vANTI8ft7XJyfaAjI9QUsJ+/C7Zq/ESS4" +
            "UCIWU5RM1TNWv855ZPQEoQWnIm0+1Fh+nLSVFl5D3tl9TnG8nXPPas2rhoBCknt5i4LV9I7qN9kwr8NdtWwZOafpEPBT6zPw5Hy+" +
            "XQAlX2LXGHM2shOMvsvkT19at7PD0q/km9fT6iT4mk9KPFs+nRRZdUtLFRZ6dcTpuRRTXTIs4UB8ooqFNKGSE0KKENsL3p5fhoaw" +
            "tCVdhIiHTeauipAePu9nOioOQojWiKpaskRNYyBEJxlf5Hs2ceVYGGsSZUOEu8ruNck3Avx9o/4IN0k2Usx+TVrUFe9Owj1vgT53" +
            "W3tDIDVfqbIdNABsiVyiy7Fmun6XoLbcKXGCNKl/osFmCETwdrS8s150KbJj/YGyc9eNWfEMruP67zqmgEsq5cLmvPM+JRhhBITe" +
            "XlxCu3W+JhoCOguF5Evoml6+/ZrZr/bQpoUo2RCw103ad4YI5jRyRMvOqpAq4SYR9vcECK3Yv4RncVe6DROTOrpEY9z8Q23Nb34F" +
            "TXt2Nn1ByUNjGumiYnyMhTDIv1TCmPyzjnhixheDPOf9HKRQrfSz7T5z3zMyOhQV7+IZrQ0UzGYffTGZPfkUvlIvRokyZ99YMhkC" +
            "P3LVsAcKyfciO8bnEUvwKKo1EnwPnawiwxNAiJwTKWW6NJAQLPyhz2dX15KNrT1pYxE7XsaUXGGQZdrmJjnWHCWfax7GVuqCNH/V" +
            "ZIlcH5HUphnESA57VWbYUuBnZOmpGPsuuDg5qIn18HvSGv1awMfTixaT+y7ALq7u8WmLtiLVuHzV6iBXfK5U0GwIGItFmnmdYiwF" +
            "iy/Iq89Z+QkkuYWOB4OlJwOmkmqtKDEuIy21867fSRes3hlHHpHdsMADZanxe1IKJm26WHLbFoMwHK1xLaLQGNdnhBQitT2h5SZq" +
            "OcpUDtN3uvx87GT9li5SuSKPVmUVqr7iNG1FURUmXi/gGUMRd9WvZ7Lci6ZFmqNzUbbMAeXWUbRSroHS+aXJnzJfqCaNYhgjcjys" +
            "hoAtG4vrVyt9HlJcdbrz6af46KVFIlChzHc+g9xqNJH1axldwTLj56R3Dbdkj1Vw5cRTvy38oJZi47HUGRLbDoO8YEWOUlM6LZRE" +
            "W4CyxI1NL5DLI0cNHp4N0ZHU57b1bWYqm15FGtONepjyjqV4egxdyaYf5jCRC9EGwHRNlevTL997Alj0SBWjvGeqCb+YerQB9S7w" +
            "8sTRV7/5q2XZdqQ17tkpTIZOn6EMVVBMc0c4aAj4vBOevgvGtVdSxr3j6qujiTEATVr5DQEnJAVPF5kxZyvHVpFURIu+jCwzllar" +
            "pMvRSAIIngUeXZDla2zBIafD9Fteqy11uubPOuGo5Bbc9WowfjvbrN+T7idRFqN9VcDUdSkrfEpa3biDg+9L17FsHa6EXQrnmYmF" +
            "l6vqotpaYD+gYiFBThsDG61Gk2VjUNfGJQHjfmZzvCd7BbVKfDwQZecFOj1pTBINAB/k92Hp7wlwWAA2M6cdXF4B5vjcAs6e0D2G" +
            "gS4j2491url5laVlevNT+uQJGgCtkCL5ViBh29kdRlO+iUhL2p8vQ0xdpfSkoDi/zQBoAXn+WipSxoAcY+Ev3l8mPGPsV2mK4Spb" +
            "b3MoT4li6stbytM58sQ6zyx/Vhk+Xp//9NuZPJ3VjkLGia4zoyHVxjUkFCPLS6XDqitJv1V5QkX169UYpSmVldQSl7eWPJqU/+B7" +
            "Avx2dU2S4jDtalknmMNyI08aFybeiTUuXMaKkS7QwzbmDo2Ar1ry3Jvu1/Zz1ZzsgH+nRTb6SbkQX5FPoMS0xenpj6y/pcIvVxe7" +
            "CRE7ftUU2onPPZ7j2ijlUWPrN7ZcSxtrGwZeeeIcdJukkJa7SgbBLL91UiwKmxMptDjGxznlZtrUFNltLmXeyYNJEXLauVAsPkav" +
            "YhdJa7Q8Sdyh8OmTIfkx02RZAEm7LseOB/nBR58rXcKYh9SY1Dpta1GUnh7QV+ys1V5pjXHWzbTJrnRDnkRSYdkIcGq7TuI8WpfT" +
            "3U/OOl0ll2Cq5wBtJ6BttXnw7sZH7gIw44sf8qKXbM2z+HqU+hLu11KyyXG9BGsrbAmOiHD+DAMgVEYIjGWxTBa2GucYAwDImGuZ" +
            "SKXd5t/ZQiksMs58k93Z8eYBcaVDpJcXm8f1F65MLSNUZrwnIQKFY01AmufayqRLEEMNAyBYXQAuWRc0ADxMoIwrLX/Qrnt/R9Ae" +
            "gxrlpJbhVlITyrLKhdQMuakceSMza2xeHfFF5q4UZpoUx28pfOFAqyxYa0HvVK9xsTfidTSgb+smFwmLGvkKFoVcuZPCk69YH83m" +
            "hHPVXqkOb516DGMdcnKxhbGV1D8N6k9yenixsUTRiGWO76F8vkanGDaGUin9rtZTleZLtWmX661ZYd4n8cAK8OqEgbyxuyLVXfkV" +
            "oBgBRyICx/yqgFxs5XGMLczWf/r2fhSDJlvPdoRO8rAqi8563GX5lM7/v71n2bEdx43yqXvnETQaCJIsGlnOauYn8r9Z5gvyAVkm" +
            "i2yyCRozQdKDTqcfVXVsZmHLpiiSImX5VN07l8C9ZVsSSVEUX36cMxugt2+Gs2usvSTUQqw90iHidNLmayO1F9UwwVGOvjOg0+yn" +
            "Uo/U7/2rVAbtn6vg0hDKVcouwa+TZC00GkX5WymjYW2bzsrE2nNm/wic2M++Z1ga2E7qcGWXI7qy2z7Ty1VjeuTsKmJ4fC4YenFC" +
            "llx21DbvGt85dxEG4ur1ubY/cmAVdE0e5bvKH/lzgScG4zTymAiNoJ0odLT4h8ddl+ruy4ERYd2WWKFJNUrPHAYpW/07AXta0JcW" +
            "jvPh1tNR2uyxHCI8jlW20S+nQvUT3ZFMtXjUyxBCNODXOngTgRY+X/DfV39qjYrriswt0haJuEoI6+491aFU/NnwKMGTgFT8afik" +
            "Jze23DQLznSiEJoypEGhkB1WV0qc0hxbBARujunI2Fp7MKpzmiyOr57ELLFqzZzycVFL4mFzkCa74xPG6Ap0tHlQVUyQ3Ith26G0" +
            "tTSp6gFJw53IXWwpVLKU9NVgWQ2ok0Bb3JAiWpDXGfd2w1RWePhY0PaE6Q8l9gVD15Mwa/Ztk+M+D2ILRR1J/JK9WS2fYrDVBMkS" +
            "4rYha75TOadGGNXePSDsfd9MPD7WRb9FR5Szb99R3hJr4abC3COFTHXvIHhIC7MOncMo1O8EnLgLMD4BiDpZv2OhmKlD6tmgEUd7" +
            "Gs84IQegz2yNqoq0sJnVhkig0SvboHFM7J94zZhHSItw1W9OT+bbt2bhde3eW4ZUzUDHi/EEFEmPH9TgrgubAp34rD1QBn4OPBZ0" +
            "2zDLBkhtTgk0grRun1AEsf7A2ATLvnXbOiVRUvEq+xLb/rqyQSLNiB+XaWR+QDqGHDASu8htoTI24iekvmMSAHrd0PtAFa65exzJ" +
            "fwva9uUkbvHct+9k+ii27fOg/xh6+svDrIsBnVI4KTzh60BbFoDn4qJe2DPtLMTG2yEprf+mzW7glMoxWjWAtiFsvyh3cFAeyXj8" +
            "WZ7POIZgbIR9KXpzBR0VUP6qStV+qpFUnTp8T9IPigAfAOKfsaX9nZXiPPD4zcxU4+KnKbjegqyQbRZkcuXDNbD4qEMPHZP5I3AG" +
            "DS/k/dzzaWLkJ4PiQwqFfYuAUIQt2hJZ2w7YddKyyzJLpIdQoiVtsus2kYrb5LQNpHu2PbEmYVF2vEwZ4K0cru8nLDrqD4hJOtdi" +
            "SaJIq7LicQTYuqaNycwrt4M7A1pUjBmJYxcQO3lGl8QkqTilezL5dUJos6Y+LBhsrGW3D1bOy0DPIpyY3zj2BLfZlX/ZzqcpbR/7" +
            "QEgp7fFpoVs6A1aje5jtGWuoHwfKNweqjwmcVeU2ID9TM/mVlwQkAZiAfBlADnyqS+LCKF9RMKbuWl8FWXCPssZrUzTN2EdBDSjA" +
            "DtYqfhxrEIdU8tODQXXaZaAo5AluKB2x3ZMHQYkyUfBX8+NhreQFi/MyqCgu9Mn2RGsvTS90B9oZmEk9G1qLLUH+9JoLkrZzfiAV" +
            "ilcT1GJcuQfvF+ANSX9jD/gwVt5LgDH+M51WvBWwMioyTiVfUoEXH5KIRcZ98JXIcWS0zCUVmZgAAJFrFWuUKCl/Fng/uO6B9ueT" +
            "nZgVJyQXLu1zFbA6kOkIEWYc9CiljuRqn1TR1/wY1NOSzOE0wRH4V1iSEaGPSQAoPWn2UhSv/k6AkP8oKMaApRzSRBMgQMoZ/Src" +
            "tCcBNZuVQlCr0JJ/15Q1s9V+wKnZ3nCiGhctvFZ9rReMmCKu9mZSl7dZfyjV9YlKwZnQBloE0Ps6eKsOdIaqGlXiUuJN0doBkPUj" +
            "1RIt4O8w9ldYmZEJwZmEbgfLGcU5ka/28Ceu47bOZ4W4B2KJXECpC2dFhFOJk2Lw+goz6zwi/Hj7puqkseYR+hjzIV7qNGg9kgJP" +
            "olTaunws22aNd9lRHHvieMtl783lWkR4GFJKM5ZxQr3mAZom0j6DlZRjFfZEyd4TVnGrneLViUxi7VKq06RpFK6qRIAJY0o8EdA4" +
            "JJBfZojqSqO/d6WfrMYssiuraQcdGVomKsEqbNz6pVQnAU1h5A5DcxzGeTRR72yXIDItqe9Zkaj2G5yBt8HAUF6dOWExRCSYirZk" +
            "9vWDlNyqfSWuxGBHD8ZMSLT3Zuyp8Wxkn611H50EjC5fDPmFciWwGBJnn+GPjW2tZRh1YD8DBBMBZ0yo9T17t3F0EmDSDCQvIv19" +
            "U/gTAW/6kdF6C08qf7mPO/9UOGRZsWwLm2jabTBmv1yXBJzkxYsmUPjx/CCYh36dBFSpKex+SkHSTALYRkByDLAmATQBqH+8jgen" +
            "LEa8OtgWwPg6UAkP4y1I6LjtknYZHoLXK0xHO8kARkcejDjJQxV+wNXWQdo812zWKB5ojqW1uRCYXcZpqDtpVC9tekWc4OnADEiC" +
            "a8JRn5f4kds6+dkxYBVlJ2LQutd9IFg6qPX1dOpNysdBjW2ErlX4hiNDN97W2vXsVxXbvq6jQ36VVMe4cj/3gfBlNJOmdS5lU/I4" +
            "Cy9P5nJxL8FacfV9/lGrtuY/aPCn66R7pQnfjPR5OJmgrh37yyHR/QpgJ4+9bQedMqKyEwAyzvBNPA6XziuazO8VdHcFbl50wDVZ" +
            "QnknABFhKT8PxLfFZclAhThVR/yeREoI+WWOtAXxoqGokgIyJyuqGrJ7adBvI+x33mdqJA8KyLDeQKeJqw47rqUeDL5gI22HKF0+" +
            "Dx141iEoXRQQBmWnFjL4fQKRqetBmXZofZWG/iBO4IEIyW/qZQ7Upe2FyqudRUciMBWdkty0OPBHQ4117ZvnudemW7hptHGSCokF" +
            "83y9GJNxRi/XLTaFXR0Yb5GwVSy08Q3BEwLw+GYmH0dG+rBCR1jn9XUY4I6Lk2KLs4O9Tc/Z2Di5h52gSlAXYjVbXPVxJLc5EUhm" +
            "z94YhXCzHXY9xgywv/5bPw40TbBMZMLdFM6A5tyk552P75zKCiXxf5iVI2tnislQ0WueLFWmLMtS2jzNfgHgRpQa1tbqjjJkVmVE" +
            "ky0/ppuxNO6+uysRcOFJ0inRLdp2grErqkra3bIocD0q9gwLbPk4k79TXNUgVXw8kACq5z4lp6NVi6y9ZsnEx19DinqcGwarYsbn" +
            "KM255ONQirUP3y+25WzJzZIKpyHz5wpzRTzt/tLocqzuX8rouLKHEaJbMOjlW6KRGAYa1HF/02JH61vYfYfDEmkxWyc/ClT6fyk4" +
            "LOJngZBHltYUogkPH2zp0nq+YsfUT6uHN2nvWzGWDxyxVIsphaAVn+iU2dhC50Z7M0YzdYTphKUyCeDfYhQQa4GadK6CUJ3rDUUS" +
            "EbJsNC0m5HHSkkWMWrTf3vciXUnsr3btSsi34KQV0WRrHie57QyEgjKDaKVPJxk8sz/yePvCCdybASoN0eFQj7PLWLBhE57X2Uiy" +
            "sni19hFvazmRLmNuwEgZe22i9rcaSzqk4jlZ27l7dClsdyvwfL6h5KZ3j7bkKtHkbjq8zgl2RfMENpyG5QvPrIVGtwjCGoKux7nk" +
            "mQAAIABJREFUAmGDIa2Jz8+qEreaout7Rl5eWh6bdRZEvSICUpMsDVlwc1n2fqdt8WO0tWRU6ZWVB7RNn0ElbnGmPJRgEB8HmhZK" +
            "oua+SgRQbhNB3UC5ym+LuDIKO1IrhAsy82AokkZ3504anwvkDTpoYmHDrFiEZDWegJFBRrg4oRBG8QSVDkF6ltE8CZosI+tMm0cH" +
            "26OskiehMHn3OK4OSBlHhdQ3dpjVNuaRfZE+zZqLobwZgNjirYmhuuLl3UUzEKSPBjk+qBmQ2kbw1qR/Fbj3ZK7EBbjrmAju/1VX" +
            "K9SdpsANnEarCCQlAhIebbx4PtKnNYrzHphIf+FOQPlmcH5ujhPZz9Fo49Dgsvxwl9VPwomikK/a7L1wOtO//u6STZ/BibtQTTxu" +
            "h9Po2CuuCH2tohjHqku0T3ZG9Npip+nMyw7Fvj+TAOS/PXm9E8I6YQaM4xMBDzsjwOTdWoOOSV9aZYyCax6eslQJQ3hzQZy3Flzt" +
            "Vh7ld6+qtI+gfymEFCKwGk28rJFlAK09IcdzfojEjN6+RSJABvbYgh2c66PGWc34WRnXAOVbQCXqtB/pFNQMKMKNNC7SYxiNa+B0" +
            "AvCG8Nb03zUMLU2Ohqtcr4xXrvJ+/vDoKX9q+/Fd83vB4r3r+X6BzxdO6PK4O0uGb4igeUOoeB1V8OgI5DvIm+PotRvwJID8WBjg" +
            "Vt/EyNropbJTGZS3s/hoxgAabwxJPXkD+oG2SOdmEtkJD6n8WPN6AANt2T3Yul4153e8aYex9o7n+N7gETL/shxvD90BWSAe+Gzg" +
            "xCTHeIky2BsVH0SGheKVC5TiMp9/Qj753xHxZ2ST8WJwYt8gszSkaDvun/BbMOpjQgJYt2Tk3V3ffgrRuBCG3QF4ZwkA7cPlSK/Z" +
            "axmj5R1wSlQNvPzRN63jI5erTav7gas+IOscekSixdI78ORuHTDGZzD3iAUOuabtP9dteC+Mqoj1dB7wLKyblq+La/wVPuZUMPOO" +
            "Kq5eqNb5Sj38VOHkJMXhbpxMqST/5/EF0TmQx3S8T0OJutSIR9DJ28iKvdgneSJd3eKn43AH9mLwDQEAFgONmx6RLA8IvShii9Xn" +
            "DkYah9Fh1iMSgCsr7vwmjWctTxFsN4Vwania+B+cAPTDA14wIesc2fvvFRL7OwpfPnbLoyFX2vC+ktETAx3OOkP43bILYXSx6XQ1" +
            "81HVr8HwadjUN4IrhNPA2VW0G+0LPHbQHqZcqPtfBaO2bzJ7JPEQoHgc6EN9iaGqCL+FITEl9pmaic90Wt3w2crjU5rYeV4/wTjk" +
            "04G/LFX6y4UrS48D4ctevxAGreUnu0ZXVjI/C7AnVj8ONK1pwJpLHIOPmn5xAewUqm5zP1UUgEg90+r7Vq9lmvxcUfHmNC7A26Kx" +
            "E7pA6F0yFyZt3b1qIe58NQoeWqZrPbzYZKUsw6ifpBTwfKoO5z3w3TS9gzZwDxqj/jSGiGN7eNfoPaxlD4QeGxPgkbFOS8ZhfRlE" +
            "9z3BqTkPXkzkJ8Zd7bOV+6Ewysec4E82xe3HC0aJo4xtU3VFg/p3AgAAppwG5EkkKJBVXCetQWGi7hcShEDOM10kf3nftzQaKu+N" +
            "CV31MMeO9yxydMj1jbLvyBzPy6Mn0pG4OAG9bytZW3vvgEfC2noA3aMTHXBVMNGEAXvkVPcLJzrCvhSJdE82LWFTmobqlenScqZ/" +
            "ndegmGO+gTbI/DXtmeXGBz5nJaHqKryQwZ9S8J+he86PMHKbTIe+7Bpxb4reifZ+8z+ndUDBY4mgprlmT5c/cMuQl7rko8x+FEBY" +
            "6gHRMrK/HGn3oikCsHg4TfMi6HXul81jhOa+89trEdnhO5+LCSMsuPcxPEfi+tnAg/fIW8huGM3TCUADydVByt6W5OOB8IjCSVfR" +
            "zUP7Lf3Wp2yjo/DguV5ue6T59MxxlFzO4rFq5qNg0FyV3wkgrwb73y+ob32wfkm8CpCGSUlHVPP6vqDi53OIlj6HOWwwTke7Oegf" +
            "OoJ3Awe+8UJbFcXL4A0MyFvYrLexk9rqvYGeVUUwlI8fBY941Msq/EWf7+mh3wtvbqPPwbtm/2rmgkY8pL+94K9tmQxcousK0qQ3" +
            "1UO3g+JxoBsiwrKsOQDgdjMj/2NY8LjhsLYiO3cAHr1D4wg9ylILMg0t6XyrTajyfvm9JBm6+Bn5YJsFbymPU/eqLfDeeIzck2f4" +
            "I/d0K/Rv557OsJIfURq+XKMRBpb1kXZqyDS75R+f5WnZeLfdxcE/JZekBhPavLnQcGGGbpv6uu0kUn29l0avhRwOnX7M62J6VFDS" +
            "pZBLsx4TOwORRzgcpPOjQWe3aQ5zPQXAVZfr8v+Ym4WlJfDjZONoEzkpkoA5zbRf8VRUldsQIXN7YRnjvQ3r3n55OTVDiK2qgE4Z" +
            "2kHZBWGduDgRCOlT/mtVis6AB88DE6PHJGdWCcQycz3P5WHbgnwmCUAx5kGPikQCELGvc1mvSASu2FKP+KG8iuaVAwcEFhGIJwBB" +
            "nN7OF845ZfwRxt5ZslyBJzxhc5YeVx5dbytwIhSPuHabybP2VagvB4eokIJKoOY4BI++JZTnYBT5xGPdDZGVMBfMGQHbtukmSACw" +
            "yL8TsB7jkQYgNX6pYO04SdVjAVYhAYuDk1uWxkwaIY1EkOzDDcuF1kxFbckqKMcw654Kygh59Dq48Lg3iIQ08MoOqwNf9+bFGOoI" +
            "XhW2wO30KgTX3ctm7xzfvNJpwVuWY3toOvdFcFuMgfdS2r6Sfo9NH7wPh0PQj0X4HDInIaHtiudH+ONHxGCjbCoGeRDk0yUyyw5o" +
            "bUrftC30Auv7AE+8zwL5jYD1J4MRARZMm8LkBICnsFieNvjYk4tqJu4cT0ZiDe8wGiJHIu9+CFdiPFWFDiJmF2SdWDaXtDb9kk1q" +
            "hAUMWq/EF9qLrjHOiYXg0ha5hZyNc5VQoiae0Iga0Ub/4cmh0s31LsfJ8lOqDtiQxjJJNsYDIwMct2Y4ZaXJ5BQYEz4VoDv3fxP1" +
            "FRHn4BrCOypJHPBGkbqb7ED+xCcROivyvfUrOji7IIumK1YIDZLHXTJvZ0zojjzV2FW5JCQAnuFlI8q+O6+bkMytWZ4wZBuT0tpl" +
            "Bul3AjYkiIDL9loAIsCEaUdeB25CIIOEmQq0VwllxgukBc2abAgyigCOPHd5SBuR665zsPpT3RKKyCQJODJYgQ/WTB5H7bWpm6LZ" +
            "FWO8Vw82eXlC7qrTCOdAkus4SiIDZeB5p88TfvW0btMqAIOhN4gGAPFWbxfkCksnTxhMuJxoXdCet6RnKPVgw1r2vBOswsMZO6Cf" +
            "WqSDHYIwMGrvQxWJ8q6M5tP2f6ehc9gud+jRCRWLQ+/ydCqK4js4tnCRwFvI6LS9OnpPVVKu1rRYpgG32TfiU1zxWqqPmb+hgPn/" +
            "vT3tyWVKuP8OGGyJgPg7AQsCLJAAMcGCCdIC+50AhOywDnJ6QpCUANsxczXj8ixyAxRF9SYDqPZtz6sr2PTiit5MAXAFwPUwrpRi" +
            "SK+c1HAki9FEAGLMS+M8CcBQYy1AILiu4NIEQIBIItC7909ARO9GJQDas6f+RCCVe8Ds66AThExVlkESDymvanXrLfbLQJo0hwlN" +
            "p4MHVf5vfidAmrmG5dq9ffge5Bf9ZBv266pE4JSvjmP3DxuZAHSyEn1csx1hBWPEQNiRWuZZUU9xg4d1oC7G6HXJJM5rAgBMAFNa" +
            "7VuaiyTgFQAA7vf13+sdYbkjwIKQ9i+GIiBsz/4XE6WkkFQBc04SKKFGAqJRZbCA30Wzr58hSekrHTkVmZSHptIq/OjdqUYrCYBL" +
            "mLRbYBFGOEYBh4k2XRRUA/TEfTWc8AMucBpKrA7UHkPhNFZpH0SQWoKM5rZnksLhQPZ60FZoHU9V/czql94ehVQdBMBV9IgxcoXt" +
            "GY/zjTTVq2coHornFYxMLvf/2h19aySnkGYRooFYTAQiYxp9mwXXR9hAQx+svi6g8Vcjh8btWnutaUnC15OepARwSwC3af034boO" +
            "T98BwG8BAF4B5mVOL3eA5zvC8yvC/ILbbYH1H0oE9gsotWyL3UjDNHRmL6vBsWJE6C2FFA3GycAtsb9FW9A6dzmsoBGowbmmrY3G" +
            "5ehOAgaYhWSeuseFxnIYkAC0xg5z9hGjKQ8DgGsCml4Z8EctrcDhND9qgaovaiwu90bWrnHo6loHAdcGhVaQc8Jn121O2XK/lXib" +
            "1L8BUvA1av948Sgeuj4duLEdIW0/qPvwgdBI7uIFQGkGCpLscwwatEnipVUwc4HH922baqyPY6VH5+KrfY3spzxjg4MBx9Hd98s8" +
            "PDWcEsDTBPB0Q/hwO9qKx4Hur4gv8wKvrwjPrwDzKwLOsCYAM0J+bvVwCZSVOkWQ3j6XTxqTCWWNRueT2ak02xZJC4pEINXXmwNb" +
            "XWwbEEF3Dgz5FL+90+gLADE5tdg4W2UbUaU7Ebjs407qn3pxUDXm4fo2mkDESSgNXbITnWSjnNeau8aIU2bDbsIFELlkdzKwM5OA" +
            "VgdKz5kEhHk7YfcqXEGIFeA6iQRQeGUXSejPJgE8GnINgJrHZLTF4QgDxXilI44Qi5bqSRsk31WE6A/Pzjr0PcNe+KiFoL7LAp7l" +
            "iFkLXhxKG+4pAXy4rTV9gPWZniltScAvPwN8eAKYU0r3ZYGXBeD1dYH7fQv+Z1j/bRMsq1dyaFwFdx3zyka1e1E4KBUVK/hsGe+z" +
            "ARit4osGwRrsJBQ2UB1kVLCyfdoW3e+u22ciOQlVHEbeDYDze+QMiEZ8QHJijR2i2xY4qqfNeVkdnPI5sz61bWkg6i3tBmU+ZIkc" +
            "+zcstpM6a+lkF68o294u3gbdDRhlM9UOl94NOLmuTh/fA8U6B2SgrWsOvof6FM/+MPSspYNJa1DAihFP/xhaEE7b8II4TwSw7tIJ" +
            "7VgOC2oJ1nVLaU0CEHB/J2ACgHnKdwJ+A/mVAIBlDfgXBMBl+4eQfzaATImTVq5ZiucNcsCrFDFh56VCAFO6cpoDcuBq8RkxDhYq" +
            "OdlU+6r8CzQ5GW9fzxhLXpF1zi9itvjz2pGwvVGQazLx3NqN6JG6rzoNJ4onNc5eZymNU+9CNBbVvYWwPi32usWcE6d2WXUaDVpa" +
            "s74niCHYJtflaIILezZo8uzfKGh7wENDyHvLPWsVMlp8WYGOY3zmg39FReW1xU+4od1cxT+DQN0PzvGSrEbxYflKtw8Dtq4EKQKU" +
            "n6NUik42qdSMUSr8gp61dDDz3uOH9HgKC5oWVPRPgsuWFBuw7ks7juBP02Vd5uuTOymtepSTAcS174IJUloL/MfjQB8AABGXjAIx" +
            "LfkHw4QkwLtBY8ZZtrZW4K1WC5LdR8StIGs5ecnpizy2GCdNTYWJeLc8pGXoA0GWW6E1J8jlGggKxK8J8Lk2WPLMM+owtbs6ra8f" +
            "mPM25IKOPhzMRO8qBy7w17r7sF9mwZiLRWXvW06qKzjy4rGCSIMp33Jg9cdVkWvYIk/g4xokIEAUfl2zgdyWXZwV2qu0B+vRvmcT" +
            "7ekDyR+Y/Ek6IAZeqQjK9lENWmcrkK25Y0OXItBVXHKOSQ5Ebh+kNQSr4DzZ3HXRUZErL8mEfT62biiD/1Twyv0nAjR/d0fiKu8T" +
            "iaeez7a7yZtc6edqjJcPhGx4j+VQ6NLBmyexxYKxVNiSBRIsiNsjQRPUvxi8/U7AvCyw4PrBUKSJAEEtLlyDQXthawmri0IkIso/" +
            "91GUNgRS1YqdF/NS+JYCmsz/WSOtghR8aAGZszp3OkmwZBmgj8KFSKAdduoOyHLMFRweRIQdnCGPyoA6mRTX3glRRywHOuTUWrt0" +
            "zFF0im7CNd6q20Bl4Dh9NGQn7CWrBtQnIhrLvkdxSess7d9uMGSnoz2cdD5L+ShJSmKSdPFmy5QaDaj2xNq08qZVjx0snOrosh1n" +
            "1tE5PEJCLAJ0gstXO3wYwLF2OdmsCnAo7BGV/q65Pp4KnaxHUd7Ww0PvmncBTEbqxsrOCYavKU9edGv1b4AuK6WjYcOon3MVdwWw" +
            "5mb6WFzXbtowLGn7xs+yvReQAHDZ7gT8GtZfDsu/E7AjRIJtv8wsVXGUCqVyC5OD0ZcH3ABQ/SaNendAaXOyUp0judCaE+cP2XHC" +
            "wxD08mrRtHjc5+G4E9DizZKD9RJwT7xSVHYUHejZdJEATBzMdi2toLh5M4wfsvYzhuWsweQgys6aiKVIdD849VPjZ5+vsD5hZI0m" +
            "yZF5BvbqnTbHURWx04OsPXrG2A2UHb8o+ZSwqigD1GCCHmv0hX1x6lPSKlPm5artskJWg4fo2LN7ImTDHfhc/p/YLGm79PpckzfF" +
            "/lLo08FjhjTEPAjHeM2dm19586Fx0a/aLD+mtPfUtPgYr+x23cU1+J8hwYzbR38S+zrQbSeGsIrVDEMK2JWZJQKtkcd5uxSt3Umg" +
            "jl0y1nQznXnkxSMN1LxFKg2HWkVJ6mkTXAY7lXKsqg8GwUjsVDkwj8Nuq0DBB98EjgKeCH4tb+PJc6XVHYzy5gg2+V8vGo/z6q1Y" +
            "aLRVXI49W+yBTTd6gh7NYDZ5jNIJRKPIFd8Y1lqv3KeuHsfBs/4teaqDkvAo0N7mxMNotmQls7fOsrZp6bBXybdfKtZYZ1+QxqQu" +
            "+rS0J07Uj/HENiTGwMTO2JMeGi099LIu7QlOJwqh/WHwRPmhfwG8W0LvYdngKu6SMO/xQBJ5ivBaUhBoOoJYk1thv/bqnUVf5JUe" +
            "CETpns3naowagCJBq1k7zvE4Xub1jsB8B5g3BE9/DQC/ZKTb40Brb/IckEC4mAizBDkR4JM1jTNSRKxd1ReiyhtT0locCpHcgQR3" +
            "yRrvYjAmVbyg5E3b/FLAmEQsGq9112oNktwGAMezfYJ81I3VcHpuHYBjo6j8gbzGALXsPCAZFJVfzbKzzb3/Jc46+quIHl59wY6v" +
            "T2+Cx3G41joVf9S9oga3Hetcw4FAn29DEq1gz+lIMj8aNVEHlWJBJSvo0zvN2dqf9HPgpXvCgUji3aLHZWXzlgq3U9lpFMXt5qXV" +
            "Xsuy1AHJpxW8kaBH0rXWuod4Vzojk52FJ8qPZUt4RzGoSjJ9ic5o22z1aQX7vA32cx+XUT+BzJhk2WE+F3RQs9MSl/I+l3d2xZtm" +
            "YxXDoMtVll1IVk17AnWBlcqOxTSVz00ClypB3HGg0jXHIJBgf5x/SQD3GfZ1xlTcCfgAiIjTAoAJMSVEQFhyACNBRmK0ao1+Q27C" +
            "QYMqbD4f4QwlfrjQW8ZKMkJ8MxX9qyAnsWNDQqyrWoFX0Hicnj/jt6HH6IsOkbYHAsSoAfDgkxIBUHQyAl4da40FhQePo/YGDS25" +
            "WnypewL617kGTwJAOWqD7Oh8nd1duRK8USIg8uaEvCe8d2SluXj5cfOXHeV2qhVtaJsXrdrm0AHJp1F+InKMgmX7tH2bj0/TG9DG" +
            "Zeeh/yjeNf9v89DHXcjH0iBViaVE3gdtiu792yCn3Ht0jbXOW23W/j14YwPc1Th/fL2vGcKyLIApAb4i4JTW8P0J5q8XxOc7wv15" +
            "QfzpDvg/uKRvZ0z/scD0ZwCEJaUi2i+cPTKixXld3Vr2CwtrkCfQmiDP9faXWSZmaBMAwgRTxmEYCDpsEZjb57/U/UOKSztPx+m0" +
            "8ZczQ9y5LrkSUTEGljBTGxpjDOeGQ81dfdEV1PZYAWfcFg6qnbxIRhIRdqG1ZMdh2f+rWamCbiY0y0FGzaJ3KUTD2WnNDz5rbve9" +
            "HhXoDtNOQ4Ys9FSeckAFB1kUq92le1Kjpedb8h+pHrbI7cc73/0uGlOuisV5M4M+6dzDJpNldH94eTquH4uvqRXlbRIRpZLvjmXW" +
            "ZKTpq8eeWLJryTUuR6OTwJBFvzfhtfykJ4YreCL2jI+dENagRuG1iFmKxmVXMlecQvRptyG4xiAp87YxPNExqZiWyptGbmUylbxb" +
            "m4MGSwa9CaT4qYaKFJNZew4UQ0lPfApAYckfd63yAlhf9NX5gkPPJrhPmH5cAH6cEP/vDvAnAPj+Ce73ZZ6eX+f04Xmepx9xTn++" +
            "L+mP85K+vs/wFQDAhAkAp0pQCCBsxKOX1D/PdOFi4VISJlbQmoyOkIW5dpq2Wxa7gqatRRF4gjIEQJg2nktGjrcmFoc1J+Sm2nAC" +
            "Tiufe790GH9Tg3DvUsl7G19eX2JBGaM9AVG/AJ5FsL5IGIsG3VIstBusjjsBrsALoBbyxBnYN9y2xpsOkrXg7DVTvFQmo4Vhop35" +
            "JBS1nISuGooeUPe9AYklwXkgbotarDPpKyMXLCwv77Y5IsdLWXUXZF6hddgDrauZOCV971GZ7Gu8KV4ksN3xCHOw1M1LKD9mM1mL" +
            "QZbQs2SLcOKN/6WAgl7idsJVrNNrXNU4i8+EpS/a/06423f6HprEG18OiZ7r3QDJnkyyjPZGxkP2oom0lYGgzmNBtBghBwsTAHtH" +
            "r9Q4j6wkfqqgT2FWs70VvmmVBy1i8v7LnjjboRLSa8IXAtw+F+F4DGhay7krulTaF2aPXOYVpcOjYLy0FkPBVVj93Renui3TYd2p" +
            "7Gm8Uq19tbCaVTiyJR6vpE2wXF4T7cQ4LWOHCfZiQtLdDeV3mmFeJvwJEH5GSD8Bwp9wun3/BM/Py/L0q/v9N/D8cp9/fFnSd68z" +
            "fPu8TB8Rb3+1Lz4e6kWD4XoDp6b/WwBNJykLeoN9V/ALwkbOmSseGXQWsBSMcTzl+fFpNjrnNQsjKmZNnvFexI8TwIwAU5o2BwmF" +
            "nHWoZ5HHVRWBggknMNpzAEvl+PgFX+7UhMqwRpITT5vVaSbHm0PMi3cEYlsieXQr/mro93XcBIWV8xNOlm1/GdDaWj0g4Qyt7Uzk" +
            "sd81SaWRz/t5Q5yaDJNqfzi7oWn1JAfE7kn71mPx8Mj7kO0/SQ3z0RYOpo32Ih2VDWbFXwlJ1bcJoNxXDn5a11xAaNK9yflsBRQS" +
            "LNVxPN0uQuqZXFB1AHyUBDvssn0AAPPWZbLsx2oQJ8IhsjYgbRLZmo1tHPe/pHmBLQeumiknJXDVrWWX459JY8ynf5QvEg/sRm6j" +
            "P5P+kxEUyzYkATIjpVWNVR6B8YYHX5PzrnsL1oSlHTe2gG4JapqKWMqAIoHaDuj0VJ8rCsHYDXtSkO/VHn1nWHW2TG5p6E/JE4VQ" +
            "J7jsfxZIc8L0y4zwS4Lll4TpTwvO3z/BTz8tL393e/316+2WbvNPy7x897pMH5f5hneYf32bnyDBfKShy3S4x84gToxNpVPuCG7W" +
            "ZA+ggVhhWCaACUnSIBiu2vgck8zJiWig6EXJgTHeC8WcAKYFqHZszmcqB4iwlEcLYWFEhK3ADMfXpDTwbKAlI3sj8Mb36gC2LjMA" +
            "3LhPW8q1zLLT0jKtakjXV+R7Pri+ev1F0gY0E4+J4KCeD26HoolytWAq9liTGS/QNXBN3O7EW3uTtB3Pfpvztp9Gpiz2pXaFHPWI" +
            "MoeBM7cgk9vEMy5kqOxyQV86qfFyflqyHC07yt68y+dYW4oous4CizuTTZPMs6PZ8gmW5sptfB41PxO0ZryvXdHlTKnjgIrqLB7K" +
            "/PCTicxkKfmj9pHGBvlSFXYUvpUsUsRmSTCVvE0TrAFUxmkmgQQK/3UwO8JfFbrH6rJTg7tV9kd6N1N2mj5XA2rQpmOdWzYJ8lLz" +
            "ciElpwjLkCEusGCaX9KSXjCll2VO/317gu+f4Pf/tfzyn7+7T7/95Xl6+VVa4Om7BAmXCX6GOX1ME64szeShL5hXJm/b6V0nLEOe" +
            "1F183CQbFPW+lxJ9FpfxeOc5kdefkUpJSODEQhvl47YpyX3nFhKdxyzwnXknDFI6N3zaX9EubyrZvHKQeAWAbX0mejIEIo+hZurF" +
            "us4Czx1wphghkZ6NNhNBNhZb3LreOXqq1PWJnRvF3Q1y2HToWdGHMDzv1YU4NLaXCprKU9DWKFUHnJ9lT6owM4YtHg8JT08Hjzvc" +
            "wLkNJrljXgPPxOkABpqeNWXlILfLzsBnDK+g5nU96tl7B46lavDqoGePuuTYECrnpzXf0bLjvB78LKZPiULKwb9kX1zMrVCyRC1d" +
            "fvThLrZB0Va605IUt57KHs3FupvGjy0vrn+H/SA+nzamtk4WukRPEAAZbwV9ITYAqB9CKQbej8B3pn7DbbMo40/HI7ZP9FPyZXzi" +
            "0j9Lv4p4JQ5p408CbGQYSaCfk4GWz631lzCxb4hFlNFNsHSlbMos4wka4qkEm6tr923BpvuS4D4B3G/T/X9vL/D9E8A/LH//p3+5" +
            "w9dfL3/8eF8+pPuyfJh+xoTfYbrf1gT0ts3rGeB5R73Dc3cQp5j6VoVPXWj57HWT46/4gF/ViyLRfX6ur9Fbcy9CVUYEwvdHchkB" +
            "97V+PVirePWAxGupGNEQT4fXdpcKCvkuAC8D+PjY7qJCmL71QGaGp3WjrnzVr1Cqa8yAL+VE1q7gu+CBtHTuy3Fp4gGa1u2yUffN" +
            "x52fj0UybwFpJY/DFOCWjb5fXjyVqwQQ1bKmrFqw8/WR6KEfRG4rea29eqzJMQ/GGZHnCB28AYjrbN2J1mDXwUa/0bLTeL1nThSf" +
            "EgWXLlOgT8sxONau1NiVPyudUuxbAfa4Cp5a/PiocPvh9vkCFLr9lHnRohdoxgaSyy8fESNy7aq242F754O3gh9nfMKZbfEegVcA" +
            "sTLvZQ1g40d5vE73uRkU3Tb2KMK94vD14KSCnphrz4YBYHp6mXGGBW+wwB2en2f4+SmltOD2S8G//93vXv7xn//tp3+f/yn94RXS" +
            "tz9AigZJ33wF+O0PsWUMj+mMHKthIyJQB+zzI/RapP+Gy8TB6zdfAf7wcrau/n5B0xNLND36eBpeij+tbqfgm68Avw2u+UP26DA4" +
            "pPRy4X79tGTihRfyfxyul8mDDPBAqOyyA66RY21k3rs0KX8P22+GUN6FvMgyeufH5fjDA+Ro2d4XztQbA5+fh7VLbXlzj14vvG++" +
            "qrPnf/1bwD/844i3Or7AF/gCX+ALfIEv8AUT7KQJAAAADUlEQVS+wBf4Ap8U/D+HZ6yTl7TtSAAAAABJRU5ErkJggg==";
        private static readonly string _MailIconB64 =
            "iVBORw0KGgoAAAANSUhEUgAAAwEAAAK1CAYAAACQM+LCAAAAAXNSR0IArs4c6QAAAARzQklUCAgICHwIZIgAACAASURBVHic7L3b" +
            "si07bx4G9lxr7/+gX5ZkW7YTV8pJXElFrspVcpWbvETeJA9gPU8eIslVXiCJKyn7SnZkS5a0/9Pea605RzMXfeIBAAEeutlj8Kua" +
            "c4xukiAIgiBAsnsADAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwM" +
            "DAwMDAwMDAz0BVNK4H/9l/Dpn/8aPn/5gM/fPsGnGkzdFdM3ML+5mokAfxh9uR+mb+V6ejV+W5HWryrRmb5rJ9ea7c1FLTlh4Nr3" +
            "9n5/fe0R05BrGr/UZR8ybQPDyfUXJzLyZJg+7quvP15diYH3+St8++03+Po//jP4Bv8S7HI7A9Ye5f7f/xn+/sf83T+C9/kfmTf7" +
            "D60FO69pjy3TW0jhEd7AEZUj0mcwMIF9iwo8AAwYsEtjI3BpM532IC+QvFT6jMveTGDsvNYbNMdYMNbgPJkJzDQ7aUHZN/IixltK" +
            "7i7sKsWcNIdfV0yeDJQwNqHTXNtWPaqW9kC/FmNvgmF4WnV7wpIMGGvBArzFOranxfe9G4Fis+0LEh9znIWqF6alTyeD6zY3JlrC" +
            "qzdo38yN0yDN09fJHxPZwMadFdCmyrn3LJhHKo+UdgCNjUrW5YDSzb0vlIOTs0/V2qCEN3442x9gpsYdALwFxmNO2dZaqDUOakIy" +
            "fqhyod6HHeLKNTU3qxlAwPk9iXKPnHLVELeenDcAmbNcSGzWiklhIJCpbcdj/0cUpPrlwc+v4Vz02Og9YBXZG8wAMFn4Aczj3xsD" +
            "//7nfwL//q8A5r/+v8Bmr9z/+Z8vAv6fZvij6ePxX9jJ/jcwm39uHcFvX2YbtGvCXBMEM3hhSlRsI2u276H8pvgWVp4CoUJmpvPM" +
            "+z+KxLzUS6mnBTDm+B7WZNw2O5hgAnDl45Q1JlBO62QNigEAhN3lVoWKLFfGbt8GZUzudBO2FeFnOvL6tXBqqU2bAWaHugnSePAZ" +
            "XM9+mgEfT6vcbSDHfaiYNZNb1RT3/U4uuu/XGXaX5zAY8McENa6Q+5u+W+tU6fDiDftADEIrsyDZJ35Wr96Qb+uMUwRkWoqHyAlj" +
            "CgR1zKHNoYpivIX2QJCHpM10yhxeJGzARioltoiMaxttwlnAwNinXA9JxQCS2VvScwQyhVNgYCu4pcCwLSmbHEyL+QEDNz9u6VlQ" +
            "DHKsTo8nRpFd/kxYDkEwwbJND3RVLOSQXaYgJ97p0nV4Yr0LmfOTtj+22WTLrGImcX3DkCcT1hs2x7me3bRpLcaZeze/dWitE5VZ" +
            "5va/MObtXz3mx/wTwH/8p38DD/gX8FAHAe4uwJ/9GZj5//j8RwYe/6UB899ZA/89wDrnm4OHqN0Hx7HgEafFqZs1RHOkob6U2a50" +
            "O4/pHBL2aKtvdPE6JGTZ8RZMqNb30kwoCkbDo4mArTdlBDINdLJYxfWH3THW0VSZPxspAkJJWj+n8wbAUJ47JCbu7EgLYk+QcRQj" +
            "RyNml1bPhDdhQroFwSML13Gkk2S0hPVESFZM545YyuQRXWsRc1Etq5d3grK1HpUZSKyGe4mN5KShNUO4wKJgSsFT6J9ozbU4aNAS" +
            "Du1QLrhIMpEUmdkwSGiLpYZIEfJx3VaAXFhqHhnS0YYBFzDkBq+GWVhKrfUYf751V+Gt+8XYfwUA8wRv//Hjbx6ffv33Af4UMncC" +
            "/vzPwfzZn4H5h/83mGmyP7Nv8PcswD8BgH+2MbYHI05jWAFFLTjKhI3yBlWw7yDpAzSPG7SEDpxzbYNrFGu6l2Vz2hWOCpk1vGmc" +
            "vGva7FwbcOQYBmfGUaSQlsMHWXcF1DQqSVpI+2ojGUNu+kPqkZPA9TUAgDHBNZM3rM5YfzNE64URY8ICLNuQ2xhw2urZBI3Da5D7" +
            "kSz8Iux4x+oTKCLat1r7UIEPTV5U5rmTVapsQZnQhqXySkAOLzdRIwtKLzVAyrA2yTBVsRFyWZ3S8dPannqQ2I4w/z4XlwxMBykS" +
            "gbO9y5HTnZQAw3kb8OsQob+E5XfvVQ2YhUjxtFzH23XcFMrFWRIZsjKhBO4USvoA0vTQbyQybouOm499fJr92oD5K2vgV/YTfP89" +
            "wDT9Dsx3X8HUeZD302TcFYZIAIFXbbnWBAI2rjFyBbxdmLhDfa/gqIdV9CjYIzK6Kws2vr+VCwOAg57OEGE544nSoA7RrjdOIOCW" +
            "x+Uacxg6WSz3GiuBybwWbSR7GAQ1R6gHbr2SaMXNhOq58XU69OeJtvppjtaIZ3Lrfbi3oyFhkTSieIhQT10ew4mNi2VUfc1ltujX" +
            "4x6lzyXKxpbVRy61eBIVx/QJuUcGqUiG7NgFSawiixXo3CIYT6G9JtNd2kLHf+eJipIx2lvOcHwJcYpd1VYUdErZsXahkVyrQOVI" +
            "BAOULQ8v93l6Nd1RawKvVuKQSu9p0jXg6zeCPMe1xP6LZbIKW/T4ABcAJBgzwfy468KmR853t+w2A+zpBsDOC7fWqdMaR2/Ab055" +
            "EPBpOT4TGkFrwVdmEww+bFsAOWwaOaab4+MqeWDkdpUJMqaM7DG50V4KGhxQAUBIxrUChE657aWw9y1iTEJdcwOAcFJggwGI+zSK" +
            "BthGCCF1KIS0WR8OmyCBlzVXT2iYUalwjqMkEGCMORYg73m5SSRKsyQdDF5bCesZyiAcN5SMwvaGse6eZhB1xGjHySxCPjk3m2o7" +
            "xoOs4pz88tbRJ0E4D1mVRIAPMvEx6MxyRXXHZC3HjEYWnK6m4NjaZGDjGPsoL1OYPEonkKk2mEkSbAydbc2w+Jltioq5cxDSD6Jn" +
            "TQxvl85Avbqd9WOi7drAP/a38uD6WMln4tG6LGKmiQkdcfy33NtRby8Q8JxrZ9Jxv64KdWS1ADCBmSzAx1FHlSAAHMb2yh34TjoX" +
            "h8eqHTmqDj2zJaJGbaWVUCzMeIjnSsTJTwcAVMWRTrAOWW4g4BZhr4k4xd+UU5giQXu5vLVB1e0PfCENgeNXpX3E5I31LTnRE4HQ" +
            "ng9JD/NE1+F4Dy7IAICTA+ckhWlC+yyeTLjAjckb3SrR45pjgLNpynqzA4DErbifEduSG61LGBCXpcmpA4G1rLicYXSZC7AS80iq" +
            "ziyc5KGy1VzpJadALOCIdIHTgxNQTaybXQpdCqI+bj5KFsiAxI5T8y1ejOnoyPF3ru2xTLenIYvoBuDYBdiCK+PI2AAYG79pqDgI" +
            "eF/pTytf1jqfW+XgMEMINLztOShrY6JpYRMkWpj2zNKu6yp0gbYLfPtDDpG3SdNL8eg6fmgaU1aEFAGqEmHlpNzEnnchuElT4NTn" +
            "gNP/XJDiZgfU8ZV7aE3NKiE3f/VCSJfSLTfN+P2Fglh4ETcuEbxiCaL+0KBQZ9LtllsMHSuSZU0FmSpjx+CkvChZWVGW968pK4gy" +
            "JONFUERaVozGjni2LbkCmNNH+i8MQhteypcCddcmRJ6zl1TVFFCVCO+7O/F4XsHRM9JhDL+uC+iWzArWLqdyZru8hns/DrTp2OqX" +
            "h6/+rfBMwAfMMC0d6kyCNhRQ4AHFW9P+jejIwh5UWPCWPddXBnnZE29AkPmZhqVDTaxYEdT5yzHMK3blcw0JRcBRALd/vMc0EOcK" +
            "qfUo69UTFGaUFLuZbG+r0c5MmkVVJoyYNO+OVN8GaeQRAKIsquIUDYQMp9dUYKcNhuzGk1sm0OcIgVqSE2VGZ1Pt8vJc4PRnyXWH" +
            "bkJWI+Gp4K53MCBLvR2x3aG20egsqQDXkBfgnwcW1kOmczxp5Hex4xx1ecF8WQ2lekjNze41QHQcJCqTIEtkK4LWtkRlSziKFnKo" +
            "lR30Rh5S8wZIbXzwNIHM+USv/aDJDy/s9mcBHrOBx2z210KbPRBYSwRbAcVBwAccOwEbY0l/F9Eoqi/3uTzcGnDCMDTeKhiofvSF" +
            "Zok9gdDgcp3NGf2gOMaB+8BHWFcUnQYBALqaatfbkbA1iMvo9D0xi0oIapGaYIUkVMUkQRKVwQRfQz2SbB9xFTu6kdemYxajgr9U" +
            "UMhFH7vaS3bVUDnTAohI1AzmBKotrywzm9gBbgSm0uQYylz+Uw3vbJ0nKkOyYw4tt6MrqkfCtFJ+e3+UeJJFDqN/rdn8qMFDEqV0" +
            "sf4gBoFn8xR114qbFVUyZRU7gqwtjxPJO0WrME7VwvnUX4Qu003ShyRsuOuP7jsBFmBeXxFptkDATmDeahwHMgDwLwHgfwGAfwEA" +
            "HwDzGxw/EyzayrL+ZSq7+2UfDMvwMMGCPfFDWzRdLE3lAPijN6l7tSZjxDq6wV7k71Cr/e715qwwK8Ea9lTpZzr/Id0KlrLKoJfS" +
            "SM2KmlkzFRhIEHgMlvmRhGD4Exn45GhVVRr4eISCcctkjYqqlVuUVKTnmjnvkiCAgCiIzhinFN2U3cmWTQmPmrI5NksZsOxFLlAU" +
            "rH+osXkqexkyVNGSLmrUXuJPoEQP2ACAL0Qjc0GgtH60FU6fhHJSLxRx+UO6yLWFZQ7YjgQ9ZoB5NusugN0DATMDTBa819gW7wT8" +
            "9AEwOaHFZACmyYCZQsERYW/KKUa2To/7dnU4jmg5JwCkipCkLH6Rqrsmb27i3nasoImDA/a6EiTk1AMlZShzIaXrBptIUZUIU84H" +
            "wQe6A+AkivuytjH1tqdscvLI0jdEd5MBQDQogoY7Y4hckUXqkNkGPL0kYEwWEZZVVZG5KEDVkR13JsapmK6kD3LB6fzKIDn0hJGo" +
            "2l4Ldw+KbVpFhD6vamHuDJRUGqwgRwt3wTxj0QtZFVXNe27ZGgzsE98hPPoYXYUaXd+KosgMmLz5IRio4dzltXdxdq2T1QLADAZm" +
            "a2CeDTzsQnHaSK/iC1koCgL+9/8N4H/4b5dfIt+c809vBt4A4JNZAoGFw826uR7/cS1xdAHAc3Q3BdhOXG3kolVCAVk0TWN0MCch" +
            "WUGaD3Y+cBI3fg2StiuOccaRkycaSApDQ6PsTcwIufLIGgPmSLRwoiCtU9IKorcABWVskEjqkMaxlYJQejQ4pUgwaSZsnyAAiNvP" +
            "8Fihf7j0ZJEspZLTz0Zlpzn0eUQQOLISumg7hPY4iUSDwhcOUPY6Be1pzehZGoouV6euymoKc1UQgiJLcRk6EPTl5niGc7tzsS34" +
            "0TBeOf8Kz++nlg32kv6S+YHLF3R9pyJEQVTKR/SiNz7v4c9GLu86LzgcOXTtng7HTsB8HAcyawRg7PpEwQzwFrBTvhPwBeDt08Ki" +
            "WeqA74yB6c0cCrif2UGbSE9m1C4ABAIzgYHNCIPDrMminAJkpimq8BLD9/uHBY0rZk42lQIAgo1ipN4nrCfoX5JP+zNliqqkaDHt" +
            "Y99cFPRtTndW7H4+KScYUIxrtQmQ6JSUWKpthelcXbXHnOR3Izhw/qeKV4ZQDt3aO5/Jik3cBM9ea+Qs5J3dIc5Edp89E0raxSjr" +
            "Po8T9KMXgjDEfftHzQQ+M1HeC4IBSTl2XivUuRo7KH5Z3qmw4afF+iR0bte77u3Vv9ueCZjn5QHhCSzAtB4BMgBmBni8+TwELwvS" +
            "4wMAPj4APh7L32NemFj43UIDosXh9/AyMcG5RjT6NbWWBqiSk18LSaeCvEDSrP8VI23JP8uWy4XF+rcB9tfakhkKaIfFM2lp+Ds1" +
            "AGA6XdN2TndUAbRWCbm8FRW6VQDQyuTVtGfVSJXq+RXOaeNgjazyqv571gCgFAW+Q8vnfqL8BfOTje5k8iCug7qpR6nzr5GjJq+B" +
            "eEFmuw53hyJ/zNp9PrQA9d8O5NXshmhL7U7NAFEra1pjbCXFEveF5FDuEiwnV1+rrLYHJLk2GqUItjNDayl5WVu41EdQJT1CqLcj" +
            "ENBFH4wubEvU7Zmykh4RS+Ylimc1UzMmSsePdTJKoNETLG/tsZoimdkJDczKQrfiGKtGqlDPmwkrVSd92bKqqnSHb1+IxFydLMra" +
            "MypRM4mmVsJyIDPCGv3yqDV2I8sQMhfv1vg3bHyLud6OwW+OvzHHAa/lB8Ps/hzAbJdndkPUCwJgXpk0fki0LUd41xB7dyVOHVdO" +
            "6DRg20C+8yLXtNSJG1k784SRCgjIyyjK5NuraULuKkPJW1jEIGxmtjpyR3kc+ppydCGnIOtk6HVXxIlC/pofYBPJ3emgLNFRA8WS" +
            "F27hVI0sJPaB1n1agsUTWAPvESPJ2Z16tYQ45EbaYM2qnRTaRRieVFG5uA16itoSI2hYkfI/hP4JACR8muI9gPpQbEm1DI6r1aWZ" +
            "Frhf43Sz2YBANLdZ/34wn26r/hMAGOQVmdZaAGNhtlP9ZwJcmKDi/akX60YnQR73Msfzqqw1YTRqYBVgBVo66IVRLwBI1CNNdPQ2" +
            "WgVPCOaSLfuQB1D2gDSzerVZsHxOXtYX5DImFAWcvFVXE23BSnWyc69VwPDVxwsa8nTS7NummlRnKrbZiKxZenthAEAFX34bWm2n" +
            "cnW+IGoGAC5adV/NTuu086sHAFtaRDj0Jgk+1l0AN0d0/Ae5MGud1i7L8CbYTbAA2+/pgp0BLPIAQNUgIJSPdRoVpx0gH8iMA6P9" +
            "gt0m2ZS40gAxZv05Ms0y5lnKL1kNVQQARTLDaHGBAIQ3nWRKJ05EkwCAK0s2mLDKSJ2nraRoJopWY6JoabShA8TJxpFFSoTJBwFL" +
            "J+uaY71hVWLq5Oq+QlAMoxo1FrVXKBSN7FjbDqEkGvYKZ/tfCYmFuVQPkOmtDX1mp2nn+bNRLDamPdELWqJyx5xjwiTke4iQ7iFr" +
            "91Whq8dvFu97y3PUPEe1VA0CNka3v4MpAO8dnsH86654uQ9HG/CjoUhA3CAqcG4x/TfWgPt6ouRuvTMv6fVfp6pGU6TQKKnppiYD" +
            "btI9w3Cc5jULkIzkMDMQfRBEhcLULJqmZMewW6NrS4K09Jih5IYXilbtKdoS+xDunBg8ja2HAWZHz0DKQa1AlUy2Gq3TbbxlZWoh" +
            "C+lcGLohqrGoyRzM3arfL3lWJPwZ5jK3GnH+5Nx81oJDqp4zFiBSa26u/yrMt1xjgYCzA0CtTdko5zqejgBgBljP/9vd3lkAb0EV" +
            "I10pCJijO9aA/6Oc1HcAQPfD9sgJwGzBg9MEVUCQAKZz0bwb1m+91CCfTzTDpS/OguXnJommjgFl+JjB7smyM3AGkxm/PKykufJZ" +
            "Ik7iObNcNqoOgbFGhom2GpZ+VpaS6IHKJZ0wHftAGuZAbuykop2YTfQlo2yT7HkQVrJMJ8JAgKGZ6QOL6VfIfpRBClqIE3KGg9Sm" +
            "eUU7tedNkWpzam6m0hrIMiKJeLdZcZygELp7IKGdgeIgnpnL0HYY/174ywx7Nrv/i+gdDwDv0YBH5Xgz46ZQ68RhwnwGpuD1QPV2" +
            "ArYKt7+tAZ7zb2D/vQDnp38xIbpzlSeSwJKwE2QlRPOu109HA73Vjixe2s0OnJPfNADg0Hiwl4ILDjF3guJZ4noUTZDy2IAloX4O" +
            "Q2Ao97b30KGhnCrxhMqNou3YB1QvOLlp6kmgpr5VyloGznH3bLIwENBXo6d10nzl23o/tdkuQMhIlejpScDZ6wq2vBqcBQv3lnr0" +
            "CBdrUvNPiZ+ikmOKD4dYfGKEIekE0Hhgt3n21qNtAcBasyft95267J7PwryGGR4rzmLSR1BtcRDw8QFgt18MnpYAYAaAD2vhLXwf" +
            "kXWatcUCBN3JAHyaAN4mADNtgg9NWB2UTQsBLZNW5joVNSqboPvs9tlFqBddtr1o0ghaWGsgICslZwyJELwsKvZmowZGZGsZlxLv" +
            "s8dBIFltjZbortBIh4MOFlpKyV4rwZuiJydfipM6W2XeOhfc4pDD+sCu2R/cnXfvHNkNsL6oD2f/+GVgt6i7W7yRfX838O0D4NvH" +
            "8ptd27qHgdXmfAB8MgA/fAb4BstPBpQHAQBg5wkALJjH0tCHBXifAX/NJHJMBuvPtwng+0/LH0yx809Fz7m6qtHz+IE9E6e3HDSV" +
            "dgCiyxEARNj0Amu7qzMSPwQg1rEimVaZUIz3TXpiIkny4kDgTF2tJjeKtnfDlNXzQjsAXpbIJl9nzXQBgLyzc1ZKVWobGLGSsiiB" +
            "Vl2imtwVeXP4oC6RtuOrxesnJ8cWcHeTQie0ZjWRbxVUr9SR9EIQ1QqsZJwXXV+A1fmfDTzs4vg/5uNvjo7IwBoAGPfS+0XgLQAI" +
            "fyX4yL8I7v1x/HDv7IxTAFh/QwDg3QD8PYfl4iDg/R3AvsGxsm8BzAwAH4yxCwMBZEfg8xuAMQY+vfktkTpbaHVCJ5faoo/yhvOx" +
            "2y6BstYeRJpJQBUcqCvWF5XIvAiNFj/JNIL/WvNc06MEiG2s0R0ys0rLSGOuz0ZruVVZyM5SZFGyuGAunSpylCpgQZ1s+zSN3+dE" +
            "SWe7hPVcK8XiO4RIAWlZsnJBMa4etJ+De0keayyESJMR/ydJKyXHlGGtsJBgoM1i525Lc8qpc2OdTVHCFQPN7Tn/Bt4fh4P+2Lxz" +
            "WN466a6VbzLdNwzc6/1IkPW4cHcEtiDjMft9s/E4wbJg9QMA/GK9V+WZgI/1ueDtSJPdnhMWjFwsCNgE8fltaYybznU05yQk32RS" +
            "A2dE5EzV/A154bMDgObowUusiFOOEpyky6WLdN11bSO5FQcCdxinV4CR44kirkirjgKWUKk9BDQumdQ2NjVvyoC6WuAYljvDfpvG" +
            "px5uim3VfnPIPx4A7+sxnY957RwbnDKwcXm7f19y2XUXwXtl/UpgO3601YvRnNb8P/wA8NsHwD/4o0rPBEyfjwAANqUglNDuXC9v" +
            "MbVwlHUjwL19Zs0VBAJUoMsGAsI2aSZcNC/IKms2TpWrEIqiBZn5otVWO6kKzkarzj2zXU4bzopvC2JXVeamgRQntwJBZtNKjXFG" +
            "FkViKrEtCKlb+xrNFBvAdyVSUvLcjiQbYpkHmWv1V4pTr57aUVirVW73muO5tD1uG5ITbqV6LoJeVIyTSrbHqupxA4E9GFj/Nvpe" +
            "9wT37PrvWP0/vH2PvTB4gD3bfnNr7WzWQMBBlZ2AeQbYTgT5PBnvw3qtXAMA43fH/vPJ3l6GWZ4vsPiOgCuQZCDgs8TmBTjY4I7i" +
            "upHcEbz4HZysh+M5cS1FavLnggOOL8rWpNIpdlwZhnRc2pQssCkx7D+OZ0rm3idhO9w6t8A11XZJutbIUe8GJt2FpHJvX+3yC4QM" +
            "j9u45sgSVcREsbzGu5IVdPK6pahJWCJzu/9jq4tsiSsoTGfoyjarideT4vlID4wgVS7DiUd5SNDRLM4Uw0ZfnPmpTZ0sjZRBjNkk" +
            "8mq8sNgyCNQ4Lh5eSvSlQWcnAwVs7kZsWAkPImjGE5OoYqswyJCs8u82ubcoHWsfM+lF/o9ogiRgwTnGY2G2y/MBH+uuAMYKNS8f" +
            "JsvnAePIDRz2+cmuvrcFMMbCA5aFewCAf/2vK74i9AGwrugfbvrmz/tWImiIPVLMrk0rnfVdp8sDDWY3dDmOMeVopXTXOBmSen6w" +
            "GNWVrEeRJjUCGiOjDQDCbFgRND0V9SC0ONpkWuznFNEN7xumQ7X9VZoe5UfajtEK+zxt8O1Bn/ZFVXrPIqswVWBxT1M6K0nz8ojk" +
            "tuYP8yptS6oiuR5ZAJN401pGAIDmE9A51X9g5NiCj6IAYPtMOazZzqofForbjgQnuWVbw7WFXrVnO61XBAAVoH5jTy+BACUogkc0" +
            "+55X3yh3rcHC8hzAYzbwMS8vzaEL0GkGy2fw4t4DxOvCnbWrS21heSjgV0ueKm8Hmta/PfrwuDmY5VYbDlnbfeRGg3bLaZlAgOh8" +
            "Tj/dIij/Bs8bQbJ8eDEiZ1yYxtLKYqAhKteBdqtzs6XdqyHjMO5yr92xJz4iYwGsoXlz5dVkSBQQDHW89FiQpn1FsrDAR54qrI6f" +
            "IgrK5puzM1eA6YRW5pukqYz+2wQCC9Rtb+3w1eoIc7B6md5pxoCAyTPbYTXzQmNUUzdncoqbJl+moHzdZfX/OAY02+3aLg/3UlFV" +
            "qoHU0r8JsljnYq17u7fFH58dEtV2Aqz3bRGa91PtTAP3+W2FCahtf9EEHpA1AN4DwJTzw4HLk1RCNpqI+WkNMrql8odpLVckOjEs" +
            "KB/MKjd2M9mUMzpcuOKKXXvjJr0cvU+sElbUOu9FJsBUpFzqAWflu1IAsJMSBoPuj0G6K0UBW/jik8u85km8qI0Gv03e1C8OYAQ0" +
            "/m61zR+yIlxHIh6Sq1bC6tgbfCGGXR3NRMGQhKh5zFzbhl9decwObeAWDEvRenGsl6kTRZYipZEM4kJHUJI3GvB0wehpABtfHqvv" +
            "Fh7WLM8BrH/HYwCWJpIY59HLbRJCkYq+3i8GowzY5D3jJhpvqvbK7f0WOTtOyEBMNmalotETLOP+8LOynCIpb/cpmqWxB1hor1U7" +
            "UalXjOplE0Mlwl1sNmZGHPkxGVtGfwLnX0pGxl5eDVK77EUmUaKUIL++Y9Ab7pe8jjJOUfQRMifD/tUEtdmYv8jn536yMiibTEpE" +
            "c6LeNt5HMdR0hN1GzDw0SS64EzCZ6wySARoXlJQiGAMiExgqbkgGcZSKoIhGozFOkWqCgpYyjOt4Ti++ithxxlYWKdHYbDlBBtXs" +
            "Ntc3vPszbNFEFHqOgMrCfY+/dQKA2S7PzG67Ae4SOckjhqSd3/rbuFfRJ4YmQUAsIJqF7VjBEdSEgYPzE8gmHCOHwUL6LjJkoXxz" +
            "JptWxsPzP0uWEnYC/IBKGsmaKxMnrnJodn2iit1ffLKHzqFZo1oYmWcz5VMvy5AA4nw6SVWAsciKpoLcIh7CoCl0LITjh8L2CjcJ" +
            "y96YD9oavBcBKeznR6EN5kvsAWF3TwEji6yFn60ssibg3chQEZVdDevRNKBgHGMvUUCJpRwWg17WQY4cb4FKTsY2l+XQIhxOtVUU" +
            "DT4jG04VJyEbznUhn86SezjPh66se/Z+cfTt/iageXZ2Auzq+Oe2QxSVL4lhgJNCnSBghv29Q+HrQVO8GNdCODJ6zMs7Vd++WTDG" +
            "wJux+y+eHYPa8dQYYdGYiwAAIABJREFUw2WQIMQlcRY0QR6aaWujK2NxQ5bMNZ3/BClhBlU2EdRdunlhUfTK2zEymueQ4zgkb1SY" +
            "6OosHJFIsYfWa1MZdPzSAYBPKXRaojkiWdNBwR2vBux+xhZdF8FWTiXDGoNAH8L4J5URzUaUzepvJSSmL6ceS1wk57JUuiYoE0/6" +
            "crBkPGHKjIn1/+XVrxrAbqGYR01Qq4FujivtrBohE2ZI8shQRdVjgVCr6G1nXF1UsxgmaTNrIVw1P8r4pSxiA6zTnu3evNZn4XgV" +
            "6Mfs/kiYyfMZQnCTkOsXCquqtxMwbzI1vsPqIrjhTUKOPw9rJPXtfftuYXImIndyXrLjrd0CjL2MExir3qgmRJHNUUw2pZWQk5Fg" +
            "NS/kzQQ3rZMglUeLhRqRDIuisnxIyIsXXrdx0SIIqCSHVOBJwh6fNCvO+5OjJIsK5jD9sZKKm4zRjsaBQoBedI8lOfUlyTp0dmPp" +
            "8xuudZFQBgCabi5RL3J1WsBEal4i07GFppT4GJuqGrNCYUmyEbPl/iFyVcj5qqXxlEdGtRanksW4BTWFLGIbVlOOeucijO1yuaHF" +
            "g+yWKqJ6LABged7nEP+JU6qMW9Y9IrSn746kX2b7e4Dzi8H22BnYf/UXa5QGkZNv0klENRWPA03gvkCcf3bNeg/J7R3rBgPrTsBj" +
            "Bvj6fhhMzvh7BtoeRnoLArZ7blotSGyKyOdM8FTKMrn6p3Dadz5sUr/QqsKEFkEAQHNfPoLWjHPBPFuH61iEQQDu9/I8MIa1FOHq" +
            "Jhd/Ys5XaKSjSySD3CkpdV8KzTmaWc6vhngYlGgCM9EKNpYnx5ZJeLJtbEbkACAMhrcQMxoBm7sE6zD+4liCD45XalHQD2i4MDuN" +
            "c2ytr7/CmI+HoHDUJ9tHELOX6aQTeLmruXT2kqrwycoRqg2Sti+palH9Ry5CfSP9EgzOvBCOVYzX/XMthLXt+DTe9f7dAoCxfn6z" +
            "+qzbJwDYdbBvDwLvf/b4i0GN0AQQ3Yt+qBf7/gYA70fZ+s8EWIifYg7So4DPcWC2W1v0pJkTvXO0rsO/fp+ce7DerwXJZOCmJau2" +
            "6NdqSL0JSDSnc84ZQ8dN1MhNgpCHGrLDjBiXN9f5Z+sN0wzxiZVFCHFnG1VMMkitfCbnOMfiRv3K8Kd2lTPbenaQudWZOWXECCYR" +
            "1u9ILBJwzy34zmaCd6ETIMqeokXYL2wMcDyzNszEY9ILBhQ8Jm1b4ZjI0SlpGWpuUM8ZBLLnDWUQkLRhChvnQj1vtTI+iL5F44JA" +
            "ai5Hrx3Dk7MIGR3VCZ1/LBhwPuNAYXESo/kQYD/p4gUAu1O5BgDGwn5ufWufKFCsZtXVJJu9HYiGTQa5TlYeYeSKNXrrULtsz2zO" +
            "//6jCcKqqOq5a0kZst5KTg7KkwH/WQwtUQKI6PnJM3DuSocBVrbasGLklDMBKVQ7qsgAeGfL90+njGEJOXwwRlTEaAIp2bDBpju2" +
            "BQEnSUeQsVRPrggIMGj5kOwacat0bLApZMbLRvCD3rYJHoRKYv1LkjmKHLv6iQQBXDkW68TJNgtxXogseXAmb7XjmoHmAYEiMxXc" +
            "FdW/4jT7IaiI6lcJj+xYKLwf8RPquqXT3LmNCgK871h5AO8YkJlWH/8twbAI+RpAyktI8oQgAJ/JJYZMtBodOjnuklZo4C0AzHDs" +
            "BCQZQepK3PKupbRzJksFokl8azsmOw0Ihhzxs/xY9yKjegEr9YxrQk65vGOBE3Yf4AjcNkO0HanbFh/cACB0pDkHDQsEKP6qCFRB" +
            "I2oHdpnBExt8Kzuz+gSOEKxqSrgxZ4j7iXSD3WTg9avAcRcHk7m6kHKemUARk2HIDxUIcEDHcYIINV7QPDnY5GC8W81RMjegtEoW" +
            "v9xFNIx2BmrJMMchlASM6sWFknyKQAUbu5ED71yHTr/75wYJ7nVU37q6PwHAm1k3AzjHTzGnsGt2SAJqd238nVP3c3YCLGybKOn+" +
            "DRwQ0cIOEQi4v2o6Bz0eHgliDXzJKnBQJmrPCRYUc/i9Hx+tuBuwgfI9o2ocJS2pXmLIsiCKRCtMUsTqmkt7l+k6MOz63QaTkstL" +
            "9JpBByXOf05woPmNKzY/45BpgQYTpZ2pXY0OeEgVyW37Xi61ismtxhvkHntDwxhNSxwEZNSHzJnoDWpsUmVMkC/Fh9DU6BqbGreS" +
            "+ihamcEy186azn6IYucfo7PBMmkClNqzXGdamr2aa5ATPAXKQa3Su8EA58TblQYWCGxpEOQJWTfTEgDABM4LaJaJGX1/AxEIiHdO" +
            "GvqJVYKAGZZnDSi0HNheJYSg9k6flz8sCEjRZSfNHJzg/AM4YlmVcLeBrlIqIlVNvRtpSV4un3hyVNTJOdtRRoFsirsTqSdy/pFr" +
            "LxDA0qnZlQgAwnakAtVkIBvSJepJ1oHdNMGnC0qpAjmg/As8kuSkpYxqRXLOSKMKWAB6UgpshVdPaEfcck56FTi0Ul2JdT9Kkpkj" +
            "pHlT5TDbxy0UqlFYPisQQAWkCwRSw6LBNLQjd5GJnHvccYDZUibQ9vIi97TzHcqn9dNCXcydI4tg48t9kVZSYShncOQdXIdO//49" +
            "4fy7nxDk32EAzHqiZLJuO/xeiOQedG41XceUxuI8YKi2EzCjFcbVkysnpUvBIcl1ddSuvFm7vsfVeWx7CwTYjrFxdB/mEXVmhbaV" +
            "LLJtK/97cyvLuyUYHU+W09xHcZacBPVwE8QWCICTx3PQEAMc0UFuhs47ms4IOvKvE51CTYzojfCTLRTcTykU0x/JrgozUJ6ioE8k" +
            "aZicvXqkRF0bxzl4iM20THoRkIaVLuZiOsuqi7IiTMVIHe4EInbCMZoR6OY6uaWwAP7uNwVOr8OszLjj0jbi4oDUIGkCgVHjhL3m" +
            "5oAGcNuT4gNLpsqHR4C2tM0XRIMAJw2sX275NDvtyWx5eS1BgzOtbWw8OKoeB6IDAD8FNQKZDU0Nwm011P0VN+P0OuYgmeAz2g1A" +
            "onzRdlYmcic995ya8/bW6PhIC0c3yw8gClDs5TpNqqZeOFmTMgwDOvAdj7CcpAmhs4OW4RwnrCxBj/uhrGQ/V+yP4jHLOckpEA2t" +
            "rtOkgSTyphTHBDtMFE3r387qtsxAgq2P0DNO37N4TzmBnQUBITRBgTbbbqsMkyYjrUL0S7FopqN+NgAAiIJklc3V6FbKLiNIBiHp" +
            "qooR0RIQD+chNA81rzg3rJMPc/SjV3gagwcAjhzd/vYDhCPhrAXhmmj0TIC+lTmOLjspOjsBFpYgYJ4BHo+1PusTcDtvDwIcplzj" +
            "4f4IGVYeuy6BRiYun67jv9GJnMaGK90pI+rmy+HjlCCgIki/SdP2IC8Wy5V0KRuMc0EAUja6Di7YICJj4pMiZWuSY7dkKTqxA5Ei" +
            "yXZPqAxSQpSRQBDuEHi6FpTFxKQSGZE5waKIrigI2NKYxJTTqhlPWoTFmzjODWi681Oq3qptModtIRdXNHD9ABOMgwQfHhnpRImR" +
            "bjV3V6QrJsXMCdgtan5xz/QDIA4/+L/uiwYANqC5/XN0N3Ahd5SsDZ2Fn34B5g/W742CgD7OmrgOkYVDGcA6CmQBfbPKttrlDnD3" +
            "tZpXtVA0txOzpMvzPkGveVtMIBw8PkzMF5kXeLkXOVHO97OCOLe/Utvk+wqsOa69vI4ua/hPTV6hU86VJa8REli9lKw0TpuLlBwk" +
            "MqfST9URSTq2DZTBR2jbqBVOzJ6gfDWAxl5Eac5OWrKOlN4T1+FNzIal5pBUV2I85HQ9tZvYAlob2yQYAKR9xh8+VPvD/gOAOChG" +
            "JqqdJuY7IBXuepoCosupcUmhZO4sBTouiY6QjL2NHhYo2OC7heVX5z2HH/n0cLbDJETO2G34dqCFncXJQ3qDLXV8l+ZFbzjV7kqh" +
            "WfUC8B64dJnai4bXDmkxOD4wQ8LA5SkkbQHIHwlDWWgZ0mL1hg6sjfN6bCTkpmGzqP8K6gEAsn1OkpdOBUNuAFDCv8YhpQy2da7R" +
            "MuFAd/ormgAFQYKEX3fS5gJKTg+ModOSwHSVcgAYsP2jnZyC/OGCiFtfaDsip/GkiZHtHybNu58YJKFek44Gh0TfWkHfa8aixHmV" +
            "0NHwoIZi/FS1x6GeIxeh00cFkdx8GvkjyH1UlxJjh22/iWW185grOLnLttdZCnRcMvMI5eSTTiQ2P3nOBh8IaH7sMuIpZr0pJIHm" +
            "hqavCDWu868I+zUCCtsWGVZzKIYFYgBudQoDgk0hUo6jWEm4jIjDojHonq4TAUBYNs6bqf1ChIGW3YNHpkwqXchjyjhv0MhcUmeO" +
            "AWH1wJ1wwsAVkIyRY423wka1Ms5VkDXsAxvWGxhr62SkJtMUDxgwuXFjHpO5mybqO845SOTl+GHTOMYlMLGMdlIm5sMNqghyeUwk" +
            "4DroWP+4nEbUUp0LcTtt8J5di+SnuMZWhT0ogsBUcC8ud1KwFkLlDCHzax0YVJeDZACg+Q0DAdRxwTzWMI8LypklspngRuw3p+dQ" +
            "kgXC0ch19Lc5hBqL8XiDuEFukUSQYMF6AcLm921093yRHtiAzlJBZN+M/9UEVA6WwhS9b+vyk/IAKfvg3Z/i9PpBwKbrxrmWao8m" +
            "L1eMCQQ8H0gwaboTnUHyYhGil8bwLX24Lux6STAQ8uAFMKnCXLCQawkS9fhyNGCt9dlw8ybkVvM8404zrqaqKHJ42K9dnTa++rOT" +
            "LDto3ABg+54OBABiIx5WE55djx70YoQsDdoc8qjc3DHBgep3tlw4aWUiVZbyM7JATPicnFI6pnL6BKlYVpyHo9ejvg8cAa9sNCb8" +
            "MbBM6vgYSPYzN7eh9dLFS3RK8gvRFEocGopndCyF80ItnlKNZ3XK58fny0D0rnFq8hLW7daHgZvHlzRkDmWQmAqygs+DmMuXn0Tq" +
            "PccD5ghhDXCd/KBTwznHgk/CCyjWQpuP4vuBdv/quJcOlRKjfFRjI+5ii+fnP2x3ymA02Qkob3YFMErkKpPFs3p0DCyvhNo72uxJ" +
            "3heq3eQkGXolWEHrX0roRmRc/hwlRsub4GulPTDMAGB1GzQzXcY1hta7EdfLGVQOXejzitDwi66JK4DQdrpWOVjFQJwnlIYkfxgk" +
            "EP1K6YFoorTEeAnGqngMpe4J+NRAHQCUQOIbkTrFl+dIc7opGpPaerdFFSQRmwNCF8o6OS1ViAORP6TFBU85/e7OU6W2LLm4ICgb" +
            "EpDQIudQad5UJUQ6N+5lfr5eUtJAIDm2jAEjWBEj7beAhy1RuiCD3dQEAhFB64yfrbzDjw3+Hhu/SL0AEB0T2lkIx07C2fMc8USR" +
            "NKRhVnBTaCyqBwE6U84QyJzlvLYjSrJNAG6ESDk2Bgy8gQVjliDADQRgTfd49ppg/euwp2zAJ9cgIotJFvbLmtXDNmECRkVppENo" +
            "zyZjPGF1hwMeIyRVHbmK+SMqHl+axtK1Gikdg8vLLZ1c8UOCJa+gpdPEEwLmGIcWkZtAPFbijFxAkopdDZEvwUo22DW5lM0zRyIn" +
            "ezwtn3M3DKTkRI1biodSORJkEbq+Hm82OFwI8spZPw8EeY/rglkO7euDSRulJegos6rlX7lA2DTed05bWZnsdZMZ5/i7302Yo5Jy" +
            "o8coMX6YCGh/gyHz2r2UHY9v0UYgS2X3oNzGtznP2c0eDOzNP4h+CyAYd3NAjqsz6U9z/b5OeCU7cBwP0gDA1VkDAB9OWrUgYIKw" +
            "b9yrDCUsEJqBwNnAR+3CQ2AfDmVYWjCBATBHILCQMwdZclwco5HsqJCX8DYhIO2UuvF5BA2Wd5IQsrLJ/oB4Miuog6xXkY7md24a" +
            "T3nEw5DB1hE2sDkZAUBQjFFzlA4GbhVG5IAmHNrkeU+Kvj3kRtaNJFSRlSSvmxgFUIIAYPtOyoafimoHAG753ZZn+DlkYCugxekD" +
            "V5a1ZUjQ6dqddAAg4y8Jrzv9Y0ZbGjmUlJ0QZjcBZd4RJ3hAaeOU0LJiIxWXJoYaQ0NTJ501tBlJcjnDT2gP97yEvTCeDsVyjHs+" +
            "ePor4t3QPOUOBMdRQ3U8pXS74289LzsMAAD86xlg//Ve71iQccgGcqW6Mj4eRLAbOXd6qItvMkxEMVWCgPhZA3l8XmRIOVrUAHEM" +
            "fWobysDSyW8G4NME8GkPAo40jp89DtA1hWAqn5bq+FJkMy12M7oMRb1f5wYCTlG3jvC7xDGl8nHOI8WTNIUlzRmLFIi+5Gi4toDl" +
            "i0kQBQDCBBteMB2p5YuYEz1E4yHMzzmKETEqAacjKkt0VlYfVIDIbnDliMRWQQCa7hgDrm8liwSihQQOnK6HgSuFjInAOP+Xb4IZ" +
            "OKiHtiMyhqI5OgnfU8OKiORfEABQ19X7icir1S+DfQ87zh4PvJooDfzdCKTTQ0dZ5cslBlBER2T4Dib3IMCxv1sAYOxy/7G+Lz58" +
            "caXnAyZgUEH3BUm/XP5MgEZ5SoIGm/h06wA4IrzPE8D3nwB+/gbw3dvq/FsL1FEg/5b1bkQRIWVRBY0Mg7xdNptiI6TNzgDGK3Uj" +
            "uFVL2dk25rs0mpKiVQ2z9TW2YnFImXO043p8vQhpSeAGmdF9pgxXS+nYUhGysf4CBLpM1SQITCJehLLibAwW8MoQzqo0A/vkSnqx" +
            "ZA1qqN02geOOlUd3A5hAWNMWNz/mrOUuRuzFCGeoyssHHOZSQWe9/l0aYbbvlew5ZY+2GnMRSiY0mWnaTqcp20rZBy5dRawk34aU" +
            "fWNXUYzzX19leQG/A22QFF5yizTeDWv3IGBL2t4E9GUGgMfybMDHDN5v64jHtAF/MeHqAKCw/qpBQC4vzQIBLHpFPsM8biDw6Q3g" +
            "Z58AfvkZ4Gdve/8fD9wUrDAkkWyoTBKYgUZ52Sa4KI9zR9nepGubGHnhJK7pfzcfxgtLx7hfsQAApyKO6VChKM0r40il7yMKwXvg" +
            "SXDOc2y0yysRsctMktJ6xCCY4bRHC10X4blM9CWNsnkm4AP1yerOpLuTS7AgISAxv14eYkhRZZN5tnxcpKNAbM/LaUYkCmj5TqCN" +
            "7qM2XLUS4BDKgMyulhA7FpTCuRgFFuFuYytHLpXyihaFwnvYwg5TpcXSkSV96/4ZAPOxHAf6Ztcj7Abi59Sk2JzBq1Cp7qa/E6CB" +
            "ZgVOvSJngk+nLLYC4/59no4g4Befj8HlDbJUZ+jjBbR8FhKVus8Nke/DXaNlgUsvYueY146GmeOmzIjos6B8kInIpaSOZBCgFB8Z" +
            "QAi6gk/md7P2264T4goiDBpUg1LeoZTT691LkCsKBgU0yzIVF1HDXeS4BGQAXM4Q6ujmGIdSJjDF1QbYqQmxNYT1mPCCarOE3urd" +
            "hWM7FUxht6kxHpoxKh2jw0GUj18RAzDi10McZRogR8VQToQBANnfTP9StEyQNhs/GHi3y/FuM/t+hxirc3il/1+z8m6CAAB6sm8F" +
            "tt8N7A8Dv5llR+DztPa/jfnKdRD1jBUgJ9LNzJuyddI0NOLPIAtA80Q52qRDjySI5zcNAqJaO5WdXmrgsBlYa2i1tBDaTZx7jW+Z" +
            "KcSWgQCyHnIOTqqQWWA9t2JK0JqJoyXfFWlTv0KfVZe7Am6Ray7/BajZReqgvIF+VCHpOvVUlEVcRz+uZxLBBbFCvD/4C8dLa2bj" +
            "v+UxZFEEasHs0oigDF0FAd3A6dA9grTOthHmqAogXVWwAMLtPAU0Thi5qhtzbwjaMmeKYUq5IyBpGiV/dKXeEqtJBMupvs0KAAp0" +
            "IFUUPanNtL2YAfda8gsmElqpVcJMaE8qsdkFtHK6mvGH1OWSKJzg9h0ljA/jf9EE6vxYXqWa0+Bqy6Ado9IugwFY3txVQguZZ6yg" +
            "71r8KKQW2SwgsrHWhMNBULkB7Zn+JMlatAQLNdGiioCBJN2ApufDOdceJEY4jBzcvzOxG9Q6FdcNAhieCn2aKpA4aRufofNn7fFq" +
            "qX0nwMsoG7VhABvWv6W4A0Lf1XFBz8BgiLxdIUpWeKOKkSyo82DpqlMTB5XgBT4HT1GAUBiIiNDDYGnJQiDjMjrl2VhnUjIpFVTu" +
            "dnWNQKA3cKocxoJsXgVdP1cG7iDYDsD2gcaGoTY+vcR/627imFfIzgCA7WUZGlscs0SGlPMvte1BPm8aD5x+/9fCzVFgc5eM66Hx" +
            "wCRepRtUNChvMs7BoV4QEP5QgAN3Z/SqgRvuznKTDRUAWE+jMOoSLlw3Nk73ujV7pdMskQqx+oauAOeumBV0qDuAUyv0NpI3M1y1" +
            "QYxXphOD+uyoZAhqrAZiayqa4DLJgoJHTXPcvJ3EjSo8Y9Bza1ypRLh/SNwQJd0TmgBgOyrTmxBIBz9unGe/c40fc9tz2axEXoIO" +
            "MOxlPhq4HhJ1il/x/yQQnfniFhuk2zyqAZjKXGs0n2QVSqohAhT02oZd0ZvVq4wOJuKmdVSsp4eQrQcennxEPH37ukBNIVdw6AZu" +
            "iNREzkFjSDOM7nFowzltAUr1S29QxfUp6OahbADV2wl4gK0RUriRS+nkmrWwYfyvxoD/q3AosLVELI+keodWVgMij9mpX/vmAR7Z" +
            "tLbtN6D7ek+LdguOG6RopHIj5YQknu3p5e7MlGYVqDKbnGmPSsyY5JiftnykQgmdyjkaIaqXQa6tPGtySh0/1PBfZ17A9nmxPaCc" +
            "GqRzQE59knIprRG2SShkEbXMQU2PgfjuvXbAeKlpjpP0cgJoB7HEnuofrZ1VEQfcrwCizmS1ApmTfgxXtnlf8oKq+0xArv0McKWj" +
            "6jr+mB/o7TBs7bXbRRlC99bs/2rB5zGPdFzKuxOeUUBYSGXZ05B3RfOcSBMliKKPQnAHnlKHoRQTeCGvxq1aQ6tQ/XPZjqutoeNE" +
            "WWQGEfdSqouldIKiRXNLaouUvJNodY1gtDAvrZDsUiVyD3O+tXVq6qtdLswrXAiqNJe7QB0wUV6+hIbutTiE6olWE3Bd6PyTJixx" +
            "JpNi2bVhuYGASXT+ftsgnwa8HxTzCcvqR3kK6mVNf7P+lI+K13k7kNv5Vh9whs5/9bf3wLnju1ZdKjrIDgCdl3bmTjX2zTrFBt8v" +
            "a+GOnIC5FKUtpZ3Tc6RYtBJZULh4BbSRo36lkzLQL85Qi3vsChC/oE2gl+HUmo+cQEDKkyG+lxBOZRHtgDQPAGR42mcCUsgyFja3" +
            "YH+w5AWHWLnYB3uItCcRoQLYoFSbJhkKhBsVTdCy0ZeT8QyKlNn1z9D0e4AzcHfENW2wwae4zDOI3ANyvOAGIPehMt/McGa3YiOY" +
            "FrtuVyPcYHAzlBzZyi26lLP7Z5RuY9JPtROAR1+V1gcUJHJOU9Qom1OXcS/Yiplhwx1rCNKezqYnUXRgIw/qs1NKWvLkenhmxckw" +
            "UdeuepYdASpDg3MqojqfCWfOMnGtory3EPkFXt6F8BcO8zvoiq5d1m9NeGPHYk/LOPNOjCh2E8Ja66gGvnhhTbwB8XQ7AX5HrAey" +
            "tpBMKl1XQZSr/yXrRlesOclWf/PUUnL69t4wEGgckt4J7iz8O/MuRUeqosZJvBtmhWsgB33KcQQAneNmAcAC5uDouuXkzubcrI5T" +
            "dO4r+1dTZ4oGMJ8A+E7A0wUBUmBCi3DuBsJT4/nlcAPLXumM5al4fsUZEOFQhLSeDqWhkPMM18DAs0KzqED5i13OmwqcHgT0KrDU" +
            "Wq4BOH4p+GQeTof2cHiClLp05tsGmoBlJbV3kyO3WnQU5HuFilf8IGB1IDxVqadyvyR5Euu1AM11Kl6+MmSld1LwHtDLeT+qvl77" +
            "M48v7Igsm7dOtfVQuE2jOhzLZNbbXJ9vb3WceTwv5+Fj9/qsB+JLFOOSZwKos1BXI9QFryOd40Hs4Czs9ag4R1dZZ9kqUOMICK2T" +
            "BieO6mDP2Kdq1QU0dG7aiKlrDRJF3SrJ1NuAhopsV9hFIUlkyq14OLL2A9k3TtFqYYdYOjrBcbmvWoApGTL1eO4sEChAv085EEW9" +
            "k8pC7pXzpHROkXBQQxU4nshAoMqpDNrg7b++LOHJucfED8n79UEbYY6HS48D9bDyzXUwd38PEloaSG6hWfkAQY+rlbVp97NDIING" +
            "91rk0eRL4kaOQoiz9KbLlT2AersbZ9mhTFw/SeN159Tfw9w5UAdHX9Y1BvxqdN7Oab0AvnUBHzb4bFBFJ9DrUKMg4F7ijKI550Hi" +
            "LUIXb+/krLIK6FTPn4senBYGdwkEztsmrASurZ3rRDFata9TuRXpTadt2nCvmQnHM7RhYEUnx5+u0Kkz69z9N+vfq3G+3+QWPAES" +
            "tmodByLWkizync5VFcpOifgw6NcFDMNhq7OQe9w1TPe2Gisi1a2UEFJ5K6GZTmFIVYQIP9Ufqu3SE2mhu42dO30oEtaqZb2n6qYG" +
            "a9822zGsRbsCqtjoypDqRS/8no6ag6YjIdKr9GVMYs7tIULd0VKMdq3uOM0eKk4zq3nqSJ82aNrQ+JkAGStFx74qdoD7gMgSJfr8" +
            "k1UFY5ZlyW0klrHm6nLrmY7g1fs1ZctmbWYBuhmXmX1Qk//TdgR6R8tz/YpCzepRosReVAkgL0RHrOzokacuUHswNJoX65KjG51b" +
            "T4s5pfUzAi0QvuCFOpYr4ilF6GRgfKfa0tUrQs8+J4bR2n5aILof3rJQrrncOf+aOHGE9XjudmCgFrKc3zPqOZH+aUccBwZugrPm" +
            "t97m0cueETihziRPvXVGACl7VYIAlIjkSQyiCIUa57e6RkGDepFF7kmmgYG7QDXWnmBMV8PTNWigNsY8MdAT0j4nbdTuYu7a7gQI" +
            "z2FpFtWrBwLY0yGFKNokyOClN2WzyN9Lo7cOelVUHOt3DgRKjhNk8zPGwACDU+eJynbgDNU+q54cnMZXLSEon9MzWB7vxAj2jaHb" +
            "uDPJ400Gr7XeMwFTXrtyBn54xinrQY5ii4OfiguPw2f1tZC/Xo3CwIrRQX0i1S9C26AyIwWHaHtQo7OCnoHXQ1Pnv7MAPAe98MGh" +
            "5jMCospqPYwABy2OrHccnPTsLUoHO16ueQlArbwTkaerZwI0KN4RKBrG43ByAAAgAElEQVRZ8qdBztwRGBgYqADF2HuFYToCgIHL" +
            "8OzbyInxMoYTgVLBEOU1x3240mScUERVD0nZy4OAK5W8OA6gCGgM17MbuYGBO6KVYRqz+sDAwCvg1Ff+1Clc7YCI9H6d7EVo/IpQ" +
            "GWrt7tSHlXUGlklzFqiz1+cNDAyA+ljeGUeDBgaeEs8+Fl5tfj/zHcfCuvhslTtIQK4Xlbh8J2DD2QJBH/YI0kmeNM69FM9uBAcG" +
            "7oiWR4N6mQUGBq7Es899rzbOO+zP3BfPZOFm/d1NEABwneziesNf1MvX6pvpw0AtjI5/HoxnBAYGBgYGKPRi+DP4KA4C3ibOQ24Y" +
            "EvYi9ABJtjRnxmq1sVNZDQzcBq0Cgd7GpmEvVWUHBkTo8Pmbaiz1wMPZOOup15AG9angIctjPaGjWu4yt3smwKzitPu/RP5mnIgg" +
            "f9+PjlZehsy8A+dg9Mlrgnr2h8iqfkZAQLcqSt5M0snuSIcnDwZy0IlNLXu+tF4jrhVHhVGlMYCSxuaylHrvZ2lTazqOgqpYdrcM" +
            "bkZBGxsdB3JqlQimEwMA0BUrAz1iKMiAC0Yfqr2FuAWeIAA4g/7A62AEAJU5qNkQ7ap+pnN+vex5qJ5TFTamTRBw45VubWDYz3MM" +
            "A80xhD6gRNeBwJNgiGlgYEACmzAWr7izWDUIsAC6iKQz622Dz2J01r6BAoy+HKDAvkrsdQOBM1m/sZgGOsDQnxAG+bsYJSy45/47" +
            "aMrpYNpc75mAtRLylfedL5lXfYVUw7ZeIkaq0lcJm1/RaAzowZy/LDp+2vLMbBUY5spFWya3el/FLJ2D55LqMOUO1F3bgS5ksLAv" +
            "7p54fh+vCGH6DN/KfVYAmYiq7ATMAZVuzEXDTndj47P0qTsD1h1DDfAKbRyoh1Y7AgX1toU0AEinDvSGS7ymgTNw965txPOpouhE" +
            "7lWCgJBIJ23zo5HKLzK10OD4UI/opjMHBgaqoOSh4Bvg7vz3izaSvf8i2hNonKoJHT5p+wRd4CH7uVq9IKq/IrS0L2q8tclDZQ+d" +
            "PO70jHiZhqbwjIJ46tBVAaxvC2Vz1a55BTWVk9DsAOTlLAE9jwy9L0P1GbohVZ/+Xag2BceyyhQqMreyhyFd99hL8IJKCwDGmHt1" +
            "mZrXvMZVDQIk+iXRg9OOoLe2PHfFnQZKczyrMIby828sOPld2RdDp+XmFqMC5/FGndIRfIm1CwTug3txCwB5LKu6OpG51dBz6VJ1" +
            "rK9w7eQR5wbIb1Wj3wnw8ZxCXzCmk2fGM2suwPO3j8NJbX86Ed8jABioi7jPhxYMYOhQLxQsPZs/J2nPKUGAi65URNPjLOPtW3XJ" +
            "w+wt8g90htGBOF7nAEGPPLXF67VYj5Tb/2zukgaI/lQUh/us4T2lfNXbEai6goULgodQ1hbwPrDWz++lS9t3kQnC9KnucSBhw8jT" +
            "ZK22ioKvRbtSZOF228zx6duSF5pS5/YKtLJUoddRFXNW4xBZTq5Xw5VS4X7T/WpIeBDopokvk6WSZ3HLbY32CFAXqGZmGzwLclto" +
            "nuiIZXQvqTXUY/QYSjkp96TLfcCMr9OemfKfAfBdHJ4JG9zefgNL1AecjbrQ+aeeZ622EzAVNs5AmR9KEt2+muiWnk6ycP0exh+/" +
            "k9TDnXcOb11sXgw2tRjiexnuNWG9ArC+vdN0l8drnimpNyZuGwC4n80qeCVo2oxbz1eUmhgNdgjuC2Q1pFlVRF3Obevc8IIuStBI" +
            "ZEbmvdGgaPKK0B4gmi6lHVXt2JAOPKmSinrU0Gdrz8AAj6G1CnR7HvLOeKW2AnR1BuN6UnI8kZqY6L+PlHzDo0CCyujrTNTujv78" +
            "9xYKV77YIaN9+mChVlE1OwXXjnALbjTdnqcnsmdPhNftFc1BjPPrfFW8goRy2kjPLa8gsSL0EAiUPGRwQgf3oEPVeWjk0qjczu2n" +
            "ki0AzHFy9VeERv5cjtLlHAVX/CKzCiXH74OkM54YOK9sOfCttDY89WBgBji4PWSJ+88J3j482dt4cndfT/+Blid+ZqD4+Cdy6Nme" +
            "eNQ7wnU1q1D5mYHsXswtzJ11rwSKrUJX8Fo0sluykyLprYtqOwFkwHOWjypd/NZMQt1p0/PgTNE+lRM18FJ4Wd1ttJVehi6YKEO1" +
            "579oOudK6dKt+HIUTISvdDSotiv43NBJoM1xoLMin6YPlhz0sUWpgUKohJg8qVfAyMBAr0gZuDHdnY+by/zpTeXN+0eJoqNBp1bY" +
            "FmyvK1Si0+YlUKbzVY8DAUD9MRhsQe2Xps7uVPjAd/TuVyIQGMhEs3fBnn5eYGCgDax23es861Q8wnLfd9jV8C6ReQfHjKrJkj6A" +
            "0V4ruQZ0oygyFL4BO7s7tWOx5SvchW9aVp2oyhBKrt5eY57q1FhvJ2CC5Ln4XJbNVnj9a/FGy+hZBi5PZdzMZOWjWQDgVjDCtYEb" +
            "QxUAbGhvQUrsN4qSFzB0gZr9dEHjqppK+mFh7K9VfU+NRF8VdaWkcOtpVdilIn26QD3uGgAAnPR2oBqPBZQ3OaYQrfrLi1bioGb7" +
            "7oDhoA+kMHQkD+0syGvYpoEinKYkL6yNV5nGs+o9rWut83/g9FeEtjgttH+v/IxQbSUpp9dIbc8YDc6rgNpXV6mGF55v+kHvpron" +
            "/p5MYZ+qOR0+33GDhzwH5GiyG6AkyvdtQ1tJVEzzY9c06+eruCtyiZ5nVFr/mQABuHNXpYIzhusj2QE05Ifh9mcQrkfhAcKIVvDQ" +
            "A3lk1dJkFWP7PJcp9/BxWbHXRolucmVbWYtS9BQAuNPZOsH1KjYNunw8QlOJtgFa+plooQOKoa1v6VlKW/E9nkm6JWh5JroM2JkL" +
            "MemcZiX06zAL4cl9hiu75uWCC8YlovhI1HoJLvuxsNLzgdyif5quvuYaQ+68uVcTzlr8NnYTS+syACiotfrh51eBVM6FStQN+uLZ" +
            "58bQP2cfZ34OnHauslUAUKNcH+Q19clYuTIA4O6X0u2NZjtcP6WmD38fJsTJ22CHpIksCvyW/n4xOBMlj2nFZYmwYnswObPefoat" +
            "xnNngoR+GjTwknguBTSdNqcZW522d+AVcUdlDEP/HnHw2Cd/KdxRL3QoDgKw80QaJ7kXxaCeLeiFPz0KQtht4d/a/SUS3t+zvoTn" +
            "vp19MbTKUDOSvEoRKysLs6WtpyUf40KCfQUC3GbS09ilhud1rpBR8W5AS9xZaXrmPeat2So4Q1wsIcqGVBZxkx4r8MmqPRMQHsfs" +
            "9CQki/jVo6cdMm2EPN6XRwBkz02EIsvVgUthqNYM8CgZGwVlo6ICWi3eK5xrH5giSS6T1WWczxOJxq7nWvs9i3xrU42ioT0Kj0ef" +
            "AUEgIIlfixBNUDVs2LXzhrmkMznwMm3qVa3EraYSzenpII7WtKNX81T1OFDNc/5nwQRf3KAS4yeVLqqrexQED8VUzsY435SHi2SV" +
            "W20z70I5qpvGTZnEVXHDGCNPgw67skmsviFn8UBE7DpBHuLqsDMZXP2UR37GzpHRjipBwNM8WECglsJqd7Gv0MuSgx016Z6G2u+V" +
            "HWiHu54cqoFWAcDA66JXc9edhxhm7mOs9dp9d4Jahn10fVVc8orQSxD6eid2ZqoqLh1LaxmU1KZzjzEzzGl3aKU4VXfOM47eiDOa" +
            "+JaSOFUMbX4fJxsGzkLH/WxAeZyjBKoJVjLTCV9BWYiOuw9SDlY3PoHSfHMyl0wrvR4uf/ZFfK9n+h44ctRYEO1RGQcGAKC9clah" +
            "3yIAQAo12AFgSwzD8Py4wURooPHRIA5FY8BC7RnWIH/9A+e0B/NirPMYoE3LU8KztE80/XdWXz93EOAc4N+/mjsNJBo9DKaBgeo4" +
            "S7HvUE/yOYZhBQaUaDbxNSJ8y0CgHu7up3QHp18lsm11CCznxTmtUCUImJ3vnYydBdiBe4TBUiEb5qoVhnEYGNDCspfZdF4NL9v8" +
            "J2j4EzShDgSC6FFWPfJE4lbMdoUz/btqzwR0291CxtzzWton7uNcbV96OgKAgetQSacvMxjB6c1aD910awAboeqzFXfAE3VwswfN" +
            "Gs171clWPsrXYBywJG8x9p5ovKRg6S5xpYB3WfykwNldW7wT8FGDizMg2NcJjwmVHxsySTqpOu55HnDgOXH3AEDIQKtXZD0bXqbd" +
            "L9DQqk3s+WhQg75sFPOcWWdddM1cE6QCAL5U2k9sied+JiAbNugMi37VIuXsa/IPDAyUoGSieuWRGbT96ef7p2/ggRdq6sAAissW" +
            "gOg5pfWwHEGABuresMzVwBl4ZXdtoADJwRr8dOSFOJuFDpo80D1qWd7aFrz/GUE0vqJmtG2Xbsz3JmPrfQsfFd0/pS9kc8E0VSsF" +
            "rF6cl7ryfZ3fCTgdY6q8Gr2ZooGbgXt3vt0Szh3n11kVfG90jLEBGpR2XHTQnqXd13ztj6+A3xMGHeeQyqq/Qsb8bzSQtZeylRkI" +
            "YI+68Ky06fixEyCFSlHOiN8GOAxZD1RDOJy96/M0rZcAwEVfrtPAc+AK693rjNFHAFAH18m4wc+tVEN+UFUHjYKAXgeUj9QvwOVh" +
            "TIu3wuiugW5xDzv63Bh9UB9Dpj3jnr2j45pd12mGNh5nKaoFAXY+zlstf+F7dnqC/NWfe85KTehZKi+LEQgIUaix3oC6GVzjdgpi" +
            "WZ9vL8b7yBa8kAzuOj6roKd+NmAu5Id7m+G9Qb7ypQhyOnH9tuagy+i4us8EBIfG7Hpmth/F6cPC9SOPgR23ePdyD8g862nb/nbG" +
            "c4FWxPOkOAaDjzOe/+jkSYtO2LgG3AiTpFHpkjqxq+vQCx91IGtN7gjXycqsddl6Bl1CZ1vTCfJU/8XgflHhnP5YGLsFsrto+KeZ" +
            "SEjcuuljAPG4+oRoe+r3RUu5yB8PHDgDYV8PG/YKOLVnz6osUc+tHgzudehp3/8/0BZFv7835t9MaOQ8RkUpFjUdcjwfZ8n8VQ3R" +
            "XXSaCwqGLbw3jPPNIHfr13OlGtzuFaHubsb5L+jTQduvYdvqo3eJ1cPmJGXJsWw394WhnfzurouN+GfIxkl6ObKHD4YuC3DW6w9T" +
            "x07ujDsszqR4HMt/zwvf+U+pX/5pYoe60pTX8hVvsRNQEmefBYP8ldCpDxN8Pju2c3cn4M4PvF6KO+tiDwHABvkRIkNcjaOOpRg7" +
            "BM2gfj33MMgDeaC0piQcTAOxwwlHsvTw4HfvB+Xug4BXc10HamBoy31wx77q0cHg5XhHKQ9cjB7VXIXbN2CgIfrSDsZCB0m1+a4S" +
            "BHBEepl87PrPWuS+oGz4E9P3wqs95HRRL91TOdqi519puVmH8SO1oC2FJuBeUjwTrSTzGhK3+z9VCfzeTUTGDsXbT9V9dQImTomI" +
            "27TiOtk0fSbAZDSs5Bx9Cvd25DfUsgS3tygJpE7M1XonKBFVPrc1l4EdaOcfp/GwdwGnB/1Zitje8TyepYYv/VZJFq3O9efoZka9" +
            "uecjOFqCRyriagvGabjyp0Z7rX6m6cKVtgnuGiTPVWBCRvRe2A15/uSRu5dZp9pxoPgok39qqfaRU2wtm6SPSDXn1OD1insza9AF" +
            "uGFd2qO55a/XpOa4IgDQwJIXzL0+EE6s6XzaxIG2uEq3KtbbcPLEA4CcSvOKNCJC4pnGaSgpi97to1naVf9yLZBROFs2bZ4JKI68" +
            "r4OI8x40eEAMfuX04hXpgXp4IZl3Y4Ian1cdeEJUNcFZT9IPnIZ7dUJbu1qHem0e6x0HqnW6QlFd+J1UtxZvI+tmFh5Iw8L269Ub" +
            "Rvc9GYrmmp60QWpIC3YBaqEnsV2Oezk7VVCiqltZ8WSrdDDO7I5W46Cz8RWKtIS9Zxstcu3UHuiUUSxB3WcCTg4EaqLoTKv0/VHP" +
            "pvm3Aj7wDHI1cCMUj6ke+1xxeDrAaa3pUWyXIKWAT278S5qlKtsqACjoH9No3uhwbGnOymvp3BXuMXRdly2BAH9I6jw73/0rQlNQ" +
            "j/crB1iHg/vVUTcmH7gX7jYg+w8A7ibRtnj2t7B1iOzAQtk/xq2o4rzRoZq80qx4Xlv76eg6QcBchUpVqKPT5NsJTmBkYOAZ8Eqz" +
            "Rk1EcrufIO/HcQlSreUcxdeS1ClQi1TjyDdy+gcqQe/AeVmM9xHePr6bVi4dV3Nb1DkOdOF+gnvCSvO6UBTBcaacNwh5oF5Ee7EN" +
            "6YCFjiHZ5KwhvZseQXoqxcHOL17UQLJa+vB0aPtEuJm69QupnnD5nmownYz8VTt/1Gv6YM1bcQwZ8uIe4Fk+W7+t2PnXhHS7nWUa" +
            "m3MS3kDqJfom/tpApHWCgBlODwQo578LB1f6jEAJChtZU6e6kHl1nNUiynx0GCSIV6o74jmJizVXVD2dSSXpO3VLt3g+S3ceGslO" +
            "SDZ0/NTDoWD8PNNrQAF6c/6J6qVTqPPcOfpiS8Hqf24Xqp/6quBsGetXW8d17+DJgqqnL1sdDeoQpbIywedADrijAp1on+qoSic8" +
            "DxwYA3TgUlwbABRjjJ9+ETr9VLogyXTQz0kWKvPYyH3vQJLQCxeN0GHjOmQpiTvyPFABo+MHsjAUpyVUPn1hADCWKmS47rR6PUh5" +
            "jnTigsYm9bKy4tZ9RWh0wK2PQzp3VFoRmomVkli6sjsdDbq3XmRtaFeq9+a4d8fr8EptPQ01rdydLGY7nO34DOjgnJjpCwq9iHhH" +
            "yhoAsIJGtjgBgTfFkXqjMVAvCEClcb2Be/rjKrUO94sEFPQnUaeE1Nlacff+9/k/9j9t9ZZxBymfYCa+uyJo8EptPR015jZ3dnqC" +
            "sZWJqwKAs4bHswzD7tqReWpWEgxoqq4B/FEEu6ZZWWSSiTbHgQx7OdAbaj7V0r5o13Wdj5rmiLOqT+CkPLciDAw8H57A7CQf9BzQ" +
            "o1QvMstji8omvFEFB4NZpBWFTnmkt5dx3MTX7eXA3CmOPLF/1jEuZS/rgCtWKLXkUfOVpci96gP4gl7pUU97MYxaJPi+a7POR+qJ" +
            "xmcB3T625ZXF0tYEPHsfypByh6r0QY6oiQMMGD8Uj9zrXA1TLk0s3umPaAmJ58i33nGgFicTaqCQJ/Fbh3poeyYPkhNFBrCfua5T" +
            "b4RnsqfEK8fkmSVpW3r2409M1ladUekcWw/jTotn0O+EXuc08dquvKpTOlCGZixY5Ftb5Dp49WtPwERf6qCDs7gxC76tVx1+q8gr" +
            "NZVtDjyvOzHXJvT8TdmbfZYkpsHSKVP5OG4HL/dsCMSDxyI2ylicb0SuBR0BWzb9NEaeBUrDJheHhLAmAJBnzcezd3aADny+Zrht" +
            "227LeDlOCACuwvUcELja5F1Wv0G+MagZAAjo0jz5h4Dc14hiR4HUuwIpxgRA6xQycUoQcIXOcVs32610px/XV4/b69CtKR0YGKDw" +
            "ugZLiRe2by/c9DPBD8WKndDlmD+fKXY/vXTj2VlM1hcuq5ukQ/i3JN78y9OfCbhiNVkSIXmbBhSTlxvNsxm4vMEvj9R46dLuD1yL" +
            "s5fZKuAaPe5LBqeiadMbEbfOX3id+ivBTV6A0Q+wx2fPQdYTc1I2JWfDi5qsKCwUcZQtPLT/z2v/ToAQxaeBGz0kETv/y4EqA9DR" +
            "M1y1Tttyr4LMrWdAg1YLAxhGb74IVEqVsgG14NcTPgRH85CrtWNEsChq+gVyq/EuBM0jUxuU+V/T4adwrjQoFUm9UmOHs8IvVS1y" +
            "V6DIwVUUJkRMtWFy7/8hAHw77l+GM7dU9rNbyPkt94sbOeWf7eoNoUqc8baZAQ5n69Vz6PEAi+xObjnefVuDBwC1eZA8RP+iuFsA" +
            "UBMN2R/2tS9wHk8pyJMinQHdLEA8/ls9GBwKn+8L4T4Jl25ub/bUkDw0fQpuMtDuhKxF4oEXQMXBltCbPtTqRY3LRcI/K+7oQ7du" +
            "gt6HQOuX0mW0f1lIJpzQbt8ik2bixONAmo0WojhCxb0+vmecrXp6yGW/yZHSa3wDv4EgDTR8ReV1uFLlDBy/Q7jjchHf5EhaZ+zU" +
            "QQNtFAQC16+mlTJQSRmeTacuMCvNX3BUuJ5YraLaKHTJcpGssgZP7vFt5comKRaTyME5TIVdW95VbrQyeyknBAGIm15ZIH4NC/HW" +
            "zxs8OzgxNHf+PfKI+t904uxFtY4nXWznsswzfU1OhHctpxxcq43W9hAIlKCCB/VsOvVMAUBYCaKr9dV3qagGXbFcTg4ETgkAQlrc" +
            "iiZRpyEvkMKSUyUVIHtaINya2BpPl7rgOFAdieDBXd6TPHKHd+AaqM6BDQhgAJ7PCYFGqvGEcuoBT7jJJ8eztf3Z2hMiaF/vU5CK" +
            "v14a8+w6pALdKbW765wgwBDfm1WSyIE4/+nI75UgFcBZgrq/dYgk1XwPO422vVdzT1cGTXuecoirxHXGmOLqsEgg2upB4c5wadNy" +
            "K2fKXdge9QJe52pVyy7120yihQUMy4rWkci+zh4YLwNxy+rNMZqnX93tD0tl8lDlOJA1YE1Kxi1nXcm+som/hizHz3j0O5Saw5iL" +
            "mi+s9NQ903ywbDY7Cqp//qMNMMr1z//nig975OSiY7L1oBH5SS31ZYo8tUUGAgzBJDrpxU7Y8NDLFkwlNsL5PFzsQ+vNMBqmmb3O" +
            "WLnvpAuzYOmDLW2bVeFwvkcr7fiXdVWt3QCegzrPBMxw3dJaRgCAPk+Q2h0oAHeWy62jm3HtRkOnMn2yBBoY0yydqWmbMoBV3TYw" +
            "CGtst+KfYoU7KupkuzcE+lUSmkn6o6p6XzxexOhRcXrhqQEf6G5rQSDAxs6F+pddPDU3947LN5QqPHexRoOU60kGpUJUl0NCV+oc" +
            "B7rqRaPa8Nnds0F2Bg4477NWH56+w+ykQLs9rqfDM4nmmdqCQjGkn0IWmTNLt22/owM0sKCXvuuFjxdBLO5urQuD9q1oLpU3/7K+" +
            "+95rv5r9XwRrwnxI1oInbYLTWX2DWxpt3rcXKE+lKovJnK0glymkZO2dRzHrtxmMlUG0OyV99bnrs9BrP/Zq8E/k6eITaPL6clem" +
            "c/vYcLQFo8kGn1haDioPZIv89YGloec/saZDaXdoXLa6rwg9e0awkD6oF902SxmsmHRfW9T7V+zXSRrQwBOoitoVKc4Yi/vVz1zz" +
            "eEo+6m86Fp3ejzIJFQx7uAg5Q+rvyKs6MAvqGmqdoymE6oiEhk5IU12oErRdTvIkZFb7/EIPaMVmgm43J1ZS5zKYMZE80iEdT8jZ" +
            "kexTReF5Zvd+yXMLlcxoF32Ooo1Bqj5t2yOplizXNXDshesVdwKuOBLkCRARFxYOuTsCJr4lr0+CM6MiaV1Ivl53b6pA0biM3Z4+" +
            "RFdi8c8sKUA4jNFtOjfruT0gqk1quRvPlqesxnK0evQGUJ76GMVNcHEf3EayCTkldwU4MM8tNuuei/q9xyG/oI4mVm0fxRJ7VF1O" +
            "UrobcIrr3o0hMMRl6uhPwwZwu3vt0E2P3AwmuOpBjiU8NOJfvQugyctt3XXQH9qB3GjgdyCJBf16BSs6HD93wB2fqWmli900sBKe" +
            "sD0XnC8oQ24gkHF0u/kvBjfTJ0NeyIobWN6U5uwG2JDUSYdhyxWqYCWY2QYVUW2851vnRSA1mAwDgJq0c1FDCfP4J0sVBQAbjY06" +
            "v1SCpzbsjwbbtDvdivakuzkcE1YXTF43fprgomM/EhRLKbnaXkJcx4Lc9ehCyftC+5ObcV34JQvWZG322hY0Q3ryJKMOb6wJBl67" +
            "IGCLvlp0dkYAYJycrly877xf7BOjzuU1xTmegrs7IQ4ESmHpy3qBQO2c+SVyqbSxnXn8e6Us+rUABvmmL1sNQaOwg0nF9IvZvpHT" +
            "UTnwwXCeNC6Wu1D9srTURl8cGCwjCUxK1Z4vsuu/KKl+3+yqe6Ph1h1ax84YfXP4gVzXNY1PMs73uI+wsvnAWdhWNKLNcSB3+6X2" +
            "QCndAUAuTPg9j/R9IAgAqOtnx1Xd/szqNiDEqw22hu0d4ymGWiZe/6RKV1hIKC5xYq93rmBF7J1lh86UIXJMRhuUeke3c2SU215B" +
            "OTSLsL4qQUBIpPPxETFoQ4axI0HdN0oBpQKfYhMc+WL1nWWXrvLDbu3/NWb+ctlIlmFOq6sSDzWEennH0Ch6kLN2uZZQ8KRiP/kA" +
            "dR2dT/KkfsYmwWOlB9l7VIUG3aNClzLBQGwFkPz30LCGsW/xcaDJ6NYLukHq2PGaJ9r26+gIqBoO7/svxws77ITde+7XxLnbigNh" +
            "bO3JemqB4uQytap+lIetQgzdYYNMVCNcOjq4TtAvBYm4yWy79iQkZ2o2O1QkPaJwaSDQwt41CU5KoRr/Cm+kYnt0AQzCY2hMNEaY" +
            "GFu2UEPc6c6QlWcSBcJVafu4VAdAWi3ZxGKOI28H843xVWffGTjhaLgBXK3TCA9Wxz1V7ThQ6pzV5ShY0S8+ItSDcJz2e2rQyepH" +
            "20hSQvC8TurHYEK3AUBJuX4q0IDSPUWUTl82Q6qecAqSplVlohOyvQcAvdKtQqrknCs6tsrnipiF+vOPN6vdZJzkI+HkMSc+RLZI" +
            "ekShkStRRpbupSve7t8PwucAgqRoF6CgDrKim6GfAX+gR55ugRsIrv8dgJrgziUODLRFl0OiJmoGAgVlL8HLBADt4D4L0E+7y3FB" +
            "EOCLr4dpDl2FrjEfV21cIbHwGBCSpiTVFtVkpyTE/LDLUwHpxFYtL6H7Ir3hoGDL0unTlnLLXddoGuI0MEqn6V4nuwDV2vtMHpKL" +
            "vV3r4Z3CQACXd/1ne26w2dOAR4Ov5NZQcoRZi/y1WgDOsqPMNkLz3wmQbLR0c8y+ptW/yokl4EaxdAZ5dWc8IxAdvssqnFPUCASW" +
            "wYXyoQb5UwzShDQqnmxv75Rmyi0bDY2UfGkkPkQf65gRC0Ejq5yjr5LjuCXpHnL6p7GcIhq6w+1HyQq6JyERnU0X1SuVDn/gPmU7" +
            "1CKQsKWyySaa+KxNOPNuIjGnxMeNlS0mxHra8d0O6OAocP4l/pB8QFWHluwWA8xI2ok7AbzErlzxq3LsJyLYD6Gseac23W/ZUr0A" +
            "ACAASURBVBKoxVAmNwN1NgREAQCRJn2KQULrTLQ+f+rhxN2M1u2R8R07Z3i5NLM550vPttGn1Cfo12pyamWIhcjVMVUZdZ7jfsp2" +
            "qPWhRIRRWeIAeaoahA459rg6tThtwaJD1FzJSqtt/3DkgTn8J+wEXIQWTr1ky+L1zi90CWpVCUC5QFQw2KPdEm7VCZlYrtgJKIVE" +
            "bmFStFjGQCuTKg7ZCfLMrYLUk4QCnbKTV4g78ChGkUNaRxJpFjTeLVY2l0enrMJuaEm71zWGNEIWSSkilI3XPAJUDmk4exe7JHFZ" +
            "uwkCrlQIMgDXLsd2qhnq41ZCLa8dZ+XqQOxEWojep5VY4PJoBNu2JacMRMEEcWzmFD+15YpRwW62KC+hNCIZ5rT7JCOFVxOsmDpt" +
            "J9lCBmiufqVQk1YrHsnKcnVIC5vhiO+367oecS0ZrYyKYDxK6QZlE7ISUeVO1UjOodngOmAgTLbbt8QxRQsGjCEmJANgLAAYW26i" +
            "Kg+eHp32GrDBZ3h/gwkvGh+5ygFqziyZAgAdBAHVhdjwDBZLutMAYIP6XCW1et6wnSkeDXll/cIWwIZW2vrlIpvOWU9qXkZ4xq69" +
            "NMdxw46h0f3j1+APaWS2Yju6wqgjJlaLpplatZI8cE5b1AfrNTUPH+UsmSbhSQpVAIzIGB2SBFHP1ao0lmsHoE0m1sg32wYifaCq" +
            "jngyFhNIubRzOUqCZVSuShphRqzI7mSzJZ0r7sXqqcU7yqAHZVzHfr/GogMT2PyQt7WgsUeZ/dG0MONeFG01zm8henN2tdCs6wLQ" +
            "7bUA+1Hh3bfuxPfDTf5612PWfzLg8iCgKjI7Q7sK3UmfZyFnxf3s9mI8aniw4Dj1SJhPOeZ7RoVzwq0ehO0IV8ZdZ45vXxjNmAKj" +
            "LAtwVKQw559xsFN9y7FEyYlb7CPYSuxUFLhcyoKaAIDzsaKVKqo+g+l9HopJnOFduPMghKOHdvfLA4GMAAAp0FJERbSt98HLSlFR" +
            "yl7wabK+3W0WNSFSqzvbJdH2jaZKb5xAYblcFx+wQEBKOGeiJ/AMAUCqDdQcEeZJzSlXocRO3SsIaOSN4tFTeH1FV9dbj3Ip3hGk" +
            "JLalFQMAxoCZPoExnwGmT2CNAWPtYk2Rn/UzzrUJZ7Sodqns8J0GPrcEoqk2oGyR7whJqii3vB7ec1dHiLwa3bPG75+QveOClweZ" +
            "6nY3ynTeSInrE/ZXEJGwtRt79I/nPATLm8a9CjwTYxS6h3NEy44qsXMGabnUsHt0hI1qVLBFQtsbDQuh7cC9+8jC7F9TzOfISWPP" +
            "WBIeYrYMmTdJev3HmGPGETvs3t6Tbt86jJKbBWFjwrGEZXP3BaLVert/N9YCKpT5A2B+ANgPADsHZePaXNx1Xj8Nrr10bqnKUtc3" +
            "R+MgoKJqPpHQdaincXcyFJjbii/YuAHABPD5Z2A+/xzg888BpgmMnQHmeTWqDnUDYGw82RvvhjNZ2uDaKUn3TjqdTIsmETRRDs1y" +
            "c7BMvPua4eQb0bTR5TGNKaIAE/XIetsP2+iJ2vWOkQl060u3T3d/0a4lrTxwCXzIQ1YmCGYYBM406qbZ4MILAnyZGTc43vRsvxem" +
            "czyFFR9C8fo0DGII0TvcHImEg1UHiCOJBpjrWHV1zyDunjSm24XkjABXnhbLu/1jhLfzGfAu4MvXqVC/MWVHIk0C8Xh07nFqTCR4" +
            "vWadq9CJs9yOnfHlZIzXv9YEfYqJPAgMlybFGXyTGTj+m0Nv50Ufts+9lWZpx8dXgI9vAB8A9vEO+CyoM+W3mvRbI3yuBlF5VjdN" +
            "9LUzMFyFOu6gYRBAx+lqXCbxXkZRudr10AotGL2NcxoDML2B+fw9mJ/9AUw//xXA22cw28rK/AjyL58GDqO+T6n7YPdXYsIHhrEJ" +
            "Ou4l14lF7qVbtjul24Sx3DsmWFRLJarrNYVycqwXBIR+aEzIv785PmgQ4Km1cxE5OFv/mGMSDjvL8ySDRni+jDkcM3eHyHViXccN" +
            "5NirtQ4/SJ87mYKvmMN43I/FbRH6h+OzOzzGHLIzmzzXT8zHc52hXecDOdmtTxH5bW3wrl1eYP++8xzdc9skASdP5woLAIxjBczx" +
            "uXPmBQZkrTE/q+w2Z/XoV+vIFZx0gqAzHoynVy6vdCBgo28WrFef9XlQYreeuziRMSlAqPOevbUxj77qhby7tmLt3033o/48Bq4F" +
            "59r7cOXryB31Ijfn3wLAugC1/c0PsPvC1FLfYoYsmG/L7rWdHwDzY7H5QdvUPSSfRJ8c7lxOD2J22lS4Yed7jokAgGGmShBgTbir" +
            "VrH5JwQAqg67LC7IDwTuPv45kdvVsTFrEDB993OYfvGHMP3B34fp8/frFuvHEgh4FJfPbfo/HCePOmyO1mGLpUFAnIY5IGSPRnzE" +
            "vOOZA4dxy0H6GAcf1pMB5diDJ4dQLgYgmrwhuufUfRQ4JuT903V8/ADAdXA9WpFcXI/WZcniQYD7MAnjl3lVxF8DnuLUI6iDQK/s" +
            "4WCDeytkJpi5gqDJ7I4PBNeHI2RMzHHM6+HQ7D2+OY52/R4db8B0MA5Sdkc24N8bI9jgR+/Z4J51uxC2cezx4gUhgX65zn/oZKfM" +
            "sA0kYF2nOwgGtvQ9DcEeOCEy9MaDIxgDYANBHXW6/UnwJIC35OHILubRR9hNaLp1ZePI7lA/h2eca7cPjZkOnd+C34A36xf2KYU6" +
            "GpZ3x+Xen5vz/9gDAJgfYOctGLDrH4CdZ5h/+s3yyObH+7ITYOdVr5d+vPs83g0cs59T1iR8wBNc1qqoEgQY69uD8xA4xpl+smiS" +
            "p3yuU5HXwJK4pQuFjph33QQDME3L0Z/P38HbL/8YPv/JP4XPf/KfwdvPfgVmXYkxMDtkXOfycCj9VTa3Ytfh5YIAdzKOeY7uritJ" +
            "Zqdu4Hg9BNl4wJXSLSHobeuUd52j8BbjCeNOQ+iMQeBAbtpIaKU7uSKrsYejYZD8Puc4TOBggN+ndnOdMkbM3jRDDtXwlu+IBXc3" +
            "RyfOTBD29Xl3ELfAAGAPAI6AQNAud/XV9cLWACAeF0j/Iiv8RyB33PPH9vHf7w+XdqxH7oO/vG0P7Mh6zwTpcbBkVjUK2hd6Fuu9" +
            "0KmN+/Tob0zvTCC7XS7euD343qUficYZh8E4Pdoi1fuwn8DxjtzFFMpGUTdcWwsOn/a457TrYNs6/ekMPnc3zNF7T5sM9h4wN6AK" +
            "gzA42uiIcS9hXfn6OwF23gKC5bvdPh8f8P53/x+8//CX8P7xDvb9q9M+dxc7AwXRQ9PAI9sTR4nB3j9JsrFxlvhIkSZb4n4H8NqT" +
            "aNi9HgxG4Q5ByA4EONpJ//9ULZAMHMQ5FZbkqZwMd45d/9swfTPubxOYT9/Bp1/+MXz/x/8pfP+P/2v49Ad/suwSQOCDYE7mluKe" +
            "GaU9X58OCmdCPFhF8hCUkPlsu4xKuBOjCcrsr5jAErE6KZ5cIxtPmSF3mAWylmbTq8W4knM48T6iaMW5J9Bcx7Ewq/MVi044Yph+" +
            "TWVH3dXwfL11y1ABVOjQG0dExuvbeBclAXenBnMcsQCGUuCdvbD/3L72e9+lKmCWlD4iMVfZ9rs+SwE1Q3Hlt5fcubF+ud1p3M8g" +
            "hv1r9q482OR4koDahYrzxSMWqd+9m3D+Mer7NRIZ+OzF9oVlfzX8boDnfUphQm10K0AY8AI7C0sgsH7O87qDNoN9fIB9vIOdP2D+" +
            "+ArTp+/APj7g43c/gP3x13AEa4rZu5JffR/n3yO8fiJzYyGySfBDBq0nTzrKihw0Og50BYJIUMGQNAq8vo1S4ALogX/CbMaIMgTT" +
            "n3XuT9P6PMASBHz3x/8Ufv6P/yv47u/9EzDTG5hpWh4aJiZP7EGvmJnQqRE41TkgSbbuPZo+5gb4YHozmsAx2WHu2Ra84KyZ1dFy" +
            "3uvEUhbUxpY7D3TQJJIh4eD4RzdydCkcA+D3bSRUvg4LEPQrZ6+W9lnvLt0r+SMlDKR4emm9oJzEQo3CdiYO4hBzHAYr52q0FiR3" +
            "kce/jgobHkVYg/tQTlGAQtnuQEbeA+ICex/xuTnzy59dHwy2j2/L38dXmN9/AvvxDu+//zWYv/13YKc3gHkJFpYdB0G/9d2tC5oF" +
            "AF4lAGAOeQinaGqO4MK/8B5ZjYAHE3y6ReU45kSvPQyRmx8HYlDqmxnnYztQjYzD/k1qZRTINXS1s+Xm2+fFSJsJzNsbTN//HD79" +
            "8o/g8x/+KXz+o/8EprdPYKY3mN4wVd8mfWr49QlObmUtKCn9UqMggrT1MgMfr3hiZVnC8sRysMuxDIxbyoRJeDUysp2gpBWvDZWU" +
            "uJ2ASigj5Xij2/MhWxDw8WUJAr79Hj794o9h+v6XAG+fV1fOHAshJ+L5NFTRopqNr0SrtY95+XGgZAMrL7RSVaDwVg7uNjQaCu6E" +
            "PmHr9rA/Nrqaze3b5KRMe461iFc+tSLZG9pyWLpK/JrIbT1bTitWNO8JxrOgCurhUTRvfjUXItwNuEMrsLXQFntjdD3xYTAi765/" +
            "BI9RWktQ9fi8LlcGwEwA5m3fyYZpQtTDCSBSqnNHN6UZ+L1rtES4wQn+dUr8JSO8/ZxOy6GL40CiQGDL2BqRQ3jnUSXhPVOozOTP" +
            "Uzw6k9gsTxdFanQDgP3PYJ8Bj+ixhX4n6nNXQ1uaPaycZo8jlbeWpHg6udaB5xALVslb18JEX0TQ9E6JjPsBxU3Z5n8bOAsm3r0a" +
            "dPl7+BMd1E4tt4DTpvd5qpjcjv6027NZxoIBuwQB0wQwv4Exb3A8v7BpvXQbEGGwBzW6HAIhZMqpRLzn2SWZ9ezmOJCI3ZMWHCVV" +
            "PM9YKxAqUlQWAPiFxX1PwJ0ijmBgvbO/rWfbDbDMWVHyRjeo59yXOMm1ovLiZe6T89Yt3a+Wtcd5PdQ7MGf7ylXrLa1VnaU5egbH" +
            "vQFj7Xq03wAYu8xNWyAwTevOQCUJ3H398mRIHlWgPCWNmNvrNxUw05ia8BH6eieguBobfIJ87WbgOmxhxPIWoPUI0GZcjXMMaB8b" +
            "XACAreQ8GzRGotUIoORMrUBK8wLUaZ+knoGBUlwxw7j6nLsLmjMmrh5DresX9GX0exPOApU7V23PDqy7AckdwwEPnC9Y47nk59iZ" +
            "PNAmCACIdrMqZfXKYN+9aywTqyFKJgbOAzmCtpX/bSVlWr9vBtcExtccf8b4108PzMGW5tWm1y4noVezfaX5BwY4tBpfWprc+Mmh" +
            "V5KvNa7a4aAWNpY5yKzHgLDXU6fdll5kez0oP9ICxL9nCHjmmi5gycg6E5c/GFwq9GS/5kQYKCEa+4/6vSzaPLSxUbPhjfVi2wXw" +
            "Hf3VqJojEDCAEGCCitdCjwdaajojNXl8Nd1oibOOvNwZZ/Hfow1ohav43c7nrJ/73GTWh4SPhajD8bfB3JU6vvW6q5iSlpN5gtMf" +
            "KVoaDbrD6KiyE2BNH9pXcq5clC/cWSCeJn8ttD/7xa5f7cZ0Whx/d2s1+pl4vwz+NzAw0B5XHc8YGLgKwTEsA/sCllldsW39P/6x" +
            "Obf8QAnQXYEXRpUgwNA/0tgfbPAnLcPgPo1vgTqbXumDHHgnGPeLcbZU3V+KHBgYeCGMcT9wNxBOhhHkUZCToN3o6cDzbsDC3a1N" +
            "veNAM5yymJrdh9zq/t178XLUcbfdDVMceKoxxlO97fqgOjAw0B9aHGMY433gxth+mDRaqVQ8YFmDjXqkVpwcACjFFhZL5Wn3MO35" +
            "qPc7Aa7QT7LD1dRKwrNwN6CDWPd0XHPq2u+05TkAcF7+M473DAzcA2OcDrwqJG8vOTcA2FDPp7nIK8qotuSZgLt6HHWOAz3OaXsr" +
            "VWKZpx43HwCAK5Q+EP7u+Nvl+8bTXUfkwMDAwMAL4ZkPqT9ru3zc2dWos6tx5t7I1Tp1df09QaX5rTYX3Yeo/DruPDAHBgYGBp4c" +
            "nv9vnadWnQTRRNbOMbnVPDr8MzUuf0VoFrjjO5lvqxRnr39Y7p6IHljiJFguMGxr0qLnJ23M3sDAwMDAQE8InX+7zlvW+n9YEXDn" +
            "uPYOiWQ+vcwtyjw9Vau6u+O+zzekjulk9lTwEq8BDOgbCyTvVY3JaE/tmOhLGACsf3YEAwMDAwMDPSNwWgjnH5vIrPO/B1w+154k" +
            "isvbWRmNfifggieDcxSgn/FzH6geoGglYItcat/7OjAwMDAw0AOIxSz34Gs0995hrqMdhqud6TtI7wxUOQ5kHmCsCU/KLFfG+d4E" +
            "HFm7Dhzr+IlUufVES/LNUkx991Sqa4ZiSa2xNvmr//4TA+Do4HYTO2t5tUkaGBgYGHg9bM+1HZ+oNyG7BQC9zWZ9n6GucIBEdEJd" +
            "ipy+w9qQ9GVXVH0wOKrMXPtTwpHj7yX430vXkPtVcQrXvD4n+/gPScd9KPgIBOL+SFnQsYswMDAwMHAWnDnLQrCI5c5H2KJVmnJf" +
            "wJnv7SV+ubzUCgByy5SgShDwcH398FD9RT3cTJA9aWw27tEIqQoZYgtVpAP9WcuBgYGBgReAu2eNz1rPNEH1czRIukpeo44zy+a0" +
            "q87vBFi+D89RY/53Zjk+9I+05uV7foR9IB/a+SVXrCsp3u4Psc83+mtgYGBg4DwgK/vrVwsWrF2PAtkjHEjjHot5HkwfPFdbo942" +
            "c0DgV2Q6Hq39lTqvCJ1BIM2W58LSAYB7nToD5t0LSd/eg6w9CNPr9FKhbTkxihEVx55a647EcEvVIWrtTny5ZXxafdingYGbQ3NS" +
            "eQy+gSeB80YfesZb1/utXU8BLZ8zwH6t8zFM/DV8s1BvMKYij+UOmkvBul8Yk0QuHNuEJVOYO64l+2KnKbecdd4O9Mbxa4jvtSCj" +
            "mb3J1vl4kkNz+g47XlNyZl5ed6gt3PUC6zwKcBhYfzT72Y+vdNrAwEAOklMXcT0G38CNkQwAnGOqztwU/iRAlRm3k9V2FlV4pJYL" +
            "82kvv9OwXmiFr8lfau7cRdBCUtXeDnTNr23xlaZqEXGBnVMZ81U/2AfDsqUK8wxgZwD7AJgfS4YJwM72sDt7lH88zGLArCsUR9rA" +
            "wMCZSCy/DQx0hW0BaluFWv7YXYDdabNgHx9gHx8wP95h/ngHOz/AzjNgL7nwnFMJqq62N8LFPIosDWKSwluWyNcKkcQK665zHGgC" +
            "VkHPkE21NaWUQCtEXveGLyC5/umlJqVt7bz8PT5gfv8J5q+/gcePfwvT9z8HYyYw0wRmevMPApotbl1WDoyZAN6+A/P2GWD6brne" +
            "kodjMjAggGSM72fzGvMyMNAAq8e3HUFdnPgPsB/vYB/vh1ab7cM619Y7ujp/fIX5/cs6Z/0eHj/+APPXH8E+PpbzQWG1N4B+ZN/P" +
            "FlzuDbDiwuwrL98qQYA1YA1Rj+TEOEA9NagdAIRbdIbS2dMiQe5crRa5TPsH26ja/VBh/WbDNJ4Fi104R/mttWBmCzA/lpWV9y8w" +
            "f/ktPH78G5i++24JAMwbmGlaHP81AFgW/tetQ2PATJ/BfPoFwOdfgPn8BhbWvNsPYAwMvDQs+rWYlirNgdSADAzUghsA2Bmstcsq" +
            "/rcv8Pj6Ezy+fVkyGee0y+r8GxOUBQv2/SvM336E+f0neHz5HTx+/PUSBHy8OzsL4Ku3Ozw6VXu9T3evQOCy/coqdjcmUuc4EPJ2" +
            "IK2QLlUDrlcTW0HejebawTGZe4CthGm+7JKKOw9eSQkLYWDrFLZ2Xk8ALTsBj6+/gcdPfwvm8yeYpjcw09saDBiAaXX+DQCY9R5M" +
            "YN6+A/P9DJOZwLx9D/A2gbXTLY5XDgy0Re8T9DhGNNAY3vyzzDkwz2A/3uHx9Sf4+Ol38PHltwCwBgAGDsfIuEHActzHwgzzty8w" +
            "f/39Egh8+S18/Ph3MH/9PdiPD4DZnq/SlVdkdV7JfQKBewUA6Qccmu8EXIVq7ITOp439/ih/9/NRgXRU7VPUw9Hldl6285jzDPD4" +
            "gPnr7+Dxu7+B9x/+HdiPL2sQMK3HggyYadsFAAAzrbsCBqZPv4Dpl98AfmnAfPrZchzIWADzpmnwwMCTwbKXl4FdjRkYaINlJf+x" +
            "7zzP336Ej9//HXz77d/At9/8DQDM65SyDBRj1p2B7XkBJwiw719g/vbT8vfld/D4zV/D/NNvAT6+LVPPuqtAqnZNlQ/fwHHJOL9H" +
            "IHAnSyORZpUg4M2CFwOUHDBphnRApMmmQC+KzZ3FZcMaBoqeDsiiJVXH2SwYuz7IOy+Z7Mc7PH73A7z/7b8DsAAfP/8VmMksq/uT" +
            "8XYCzAT7ko0xE0zf/wo+/cl/DjADmM+/hGn6BGZ6AzvDGjjcZdgPDJTAPXPXaQCwYQQCA2fA7v8WB35+gH18A/vxDR4//Rq+/fCX" +
            "8OU//gV8+eu/ALAPMDCDMTMAzPuB2e1zOw4EYJdnCD6+gn3/BvP7F/j4zV/B/Lu/A/j4CtNeH2QFAkYzWLsaMhn+klnW66wrp5a2" +
            "KmFmqldtvQ91ORdzfKvSg8ErSg6VXI3guHnkF1somWJqP/mQiwpncQHW0Qa6lXsHrAy5AMA5ImnX7dX9nOZsliDg9z8AWAvzl9/B" +
            "9Pn71eE3MK3PAuyL/9NCaXsuYPrFH8P3awDw9gd/CvbzzwDg8/IsAcCy2zACgYGnBTHYsKSewBrugYF6sPtc87EGAV/g8dOv4f2H" +
            "v4Qv/+HfwI//9v8BmN/BwAOMfQDAA4zzuzVmCya2h4rndUdhXt4SZL/+DuYvvwXz/hWPbUWBgD9YI3/YyMfHdcuXeTUnvawGZqGq" +
            "fBhiqnoQH8oCTePy40Bdzi+qFWkNetkVKIEzkjB7UhIAcHTCnYQ9INjOZ1mw7x/w+P0PMH/5HXz8+j+sx4DsEgis5zKXI0HrPYDd" +
            "sX/71Z/CtAYA8z/4PUyPX63RgjNERiAw8JS4u00KMQKBgZrYXvu5PQvwOIKAH3+A9x/+Er7+h38Dv/+L/xPg4wsY+wHGvq+fAPvb" +
            "MCx4QcH+uR39gWUXAey87ARYAGvMsRHBBQJ7Bc5Xc9yyAMekKRgfxvm8UyCw7wpg5NzPTOySs3Dl203TSPD25R3Md+v3Zg8G3wms" +
            "vGp2dNda0x6c7UIvU4EAhIsgy4Na8HgHu74JaDufuX2HcCdgo/TpZ/D4+iPM71+XVZl5BpgtmLeyPaCBgVvjLiZrHA0aaATr/F8C" +
            "gs0DXH+P5vEN7PsXgK8/gv34CcB+gLXvYOYPJwCwcPjoyz3P0XZPnK7fvfiBOuoSLn8bXOs3egOVYKDcvASnTa5CteNAVBtub4Zz" +
            "H3CIVshr9PLVE1tZ3fE8bffTktSuCxUQUIsiy0qLAQM2WrRfNwwAjG9cDYDzq412edOQtWDWn3YPeU5i7BYM3AJU9H1TjEBgQAlr" +
            "fYc8St/zLVf7L9OvTuCyuLQcN53M5qwfR348Is5XE1Rq3GnQAlhrvPx2yx9NeN7HkjcIKNw8fGsDujb6qkaybKV6aiG3/mxLU6vB" +
            "STq0dKsdB6KY6MYMn/3AgiUvCuGZEQU8M6EuKx+g2DIFUue+Zbd+cRZMUIrOfZeXSAp2CwDMsfISnGDy3v2/Vj85AQDMFmBeAwBv" +
            "+1Yo83FsaKB7PFkAsGEEAgNCWGeyQdft9s9lgrLW+o7Our287TJP5njwcnfmXYKhM+6u2juTmt1eeAF4eY9RZA3NACw/4DrFKdhI" +
            "IIe+ib/mrGUmF7vdI0ypvI0h8nMQIfZrYUJvyUD4eHCdICBedO0X2nCzuGH3n129bcsCKu4bC7wVfvv/s/emXZLjSpbYNYCkL7Hl" +
            "Vturt6gXjaSj//9DdKSjc2Za/aal6Xq1ZmZkRoRvJGD6YAAIbr7SPdwjeKs8Ge4kQRAEAbtmBrNocGkb/NA++HilS6XzRSZT/3t9" +
            "LVSFVLiDLCJLAHcRgIEIDHgJeKEEwGMgAgPWgZ0o7xfosh/dmy8Cw5MFNy9YA7ABswHYig8/2MncUlBtKquUHFx/UP5AjkiI8O8F" +
            "tY74Pm1EILpOaT+Qjw9Vus5je5d5nSIr+q7Y6To7HDsgwh6N1mt0oC5c1BAcVbaugT6vXnlIq+76Om5ZjVa1RbmPnSaCdAKVTaDS" +
            "CSibQKVjUOK+J6NmUfG1aqUT4BMAg9jArh5h80fw6lH8NK2kc4fNawQjesgEcJ6jePyE/I//wCK7gb7/GSodS93SMYLtN6hzyN9N" +
            "1SRLACUjUDqGyiagdAzSKUglgE5BSq9vzwEDTomzGtN6wsVMNgNOBbam/BS5ZOhdPMHMH8HFUjSZLItyvcDvtVPsfpetARcLOccs" +
            "sPr4E1a//h351z9gi5VbT2ZkTVmXtMwkmimdQqUpVJJBpRn06AZqdAM1voVKJs1XM0x6NRU9V3dzPoOZfZTP0x9gW/SrYT+WhN53" +
            "ua6ZmRo/74a6qNX2/ZlxiDR4EhJwzmh7fnV99TbMt+2Q489Fh/S+/WrX+p6uE/6j/SGij8qgJ3fQ1++QXL+Fnr6Dnrrt5K67ai0k" +
            "APAkQOIuF19/Dh8zvwcvZ8BqBl4aMGx5JpclMQO2KGAeP2H1x/8Ltgw9uZOkYckIKhmjXFnstpJowJmDqSQABKjJjdzP1Vvo6VtH" +
            "dCYuAZkehJQB54EzmLx6x/BuDWgBWyNCerGCWc6Q3/+G1f2vyO9/g5k/SEhPawBbwKWfrwj+sFbmDxcZSJRLKxQPn7C8/xXF198l" +
            "5r8tyoRg9fkqtkYzQekElE2hxldIJldI7/6E9M2fkL75UeYN1Jx5qLQOxHbpaGUdAMA8fcbqt/+G5W//Bl58gbVGrhkKivyUqPrT" +
            "Nq+Pv5d9XYPWntajC1LD5WoLdDqIdTUOb7F7Dwl9n2GsvZnqWuzmUa+eBKwFtzTZDh3yoiwgO2AnrYKn4X6QYYB0Cj25RXr3HdL3" +
            "P8rAdyeDX3r7XfNisTdOSz38BTifY/n7v2H5238DJwB/ASyRDMzLeUUlwLVwCVzkKB4/gq2Fmd1DZVOQHsknGaGSYIAIRNr9rYI5" +
            "V3Yx9PUHJG9+QFr8CSBAWQNAQesRoPFymURpXAAAIABJREFUO8aAy8FAAAa8IgQLwGqBYv6A5edfsPjl71j88neYx0+AyUHWWYwd" +
            "GWBbyN9cfiRqnOxjayTJ12IGs5yB82XpOgRu90ON+6hKoEYT6Ks76Ou3yL77Z4y//y8Yf/+/InvzQ3RgLBlXmETrvRb3P2GWavDy" +
            "K4qP/wHOFzWBuCWmeywsbzk27OsatKtVglrm/o3neivATjUrSVV5lY7nV5On1xKAlqI24RARYTMZqOLZ8wScLSpPYX1vWnfrL1ne" +
            "28jq63Y4/+KoBCq7gr56i/Tue2Qf/obR+39C9uGfkL37S/UCzT/Lsio7GXb5BCSANTMUi48wq0dQvgItZyKhsw1RF0Ix/ntRwMy+" +
            "wuYLmKePIJWBdAb4rRP4obzgL1p9Up4UuF0EJG9+ANiAkhRqNJXjkhHYmhfbFwZcEF7aWA283EF2QD9gC7aSodeuZigeP2L16Scs" +
            "fvl3FF9+FRJgVoB3HXUEgG1RkoCwHoDDmgBmhmWArXMh8rqlMDkyutaGkZJ5QWUT6OkN0rtvkX3zN0x+/F+Qvf8bfDG7koDV9AbF" +
            "/U9Y/vJfxQ0V5E6LVz2smb139RY+xXjSuM7+ngy7YTuLQOvPPbXLsZt3yBPQhcj9+3Q9/QWhboXyf7a9KVv7UrWxgtiuSeDoUx5R" +
            "/l2Plxw/WWYDmAIWklAMxoBUAdCqYgXwfxNUaRlQUS6C0RRq8Qi1nMHkS5DJZUGZW0S25gYHDDgyXug49pK1LQN6AMOFfgO4AMwS" +
            "nM9hXZZemMIRgbwq8Ft/jkG5XsBvvRxecyD2/dD5zdRdgaJlZTJTUdus1dKZ62SiVRXfLKGc88rZTv46d7mmVr/oaysn2IG4HGOo" +
            "6HsIOtWTGdyB1qKjZ9V/2uHpNxTYO+6/CDSabE07RoOo512eKDS0IK2oa0kobBkkbj+1Y+u1IV8Za8EoZL4gAyB3BEBVyq38TSp8" +
            "JRJNEI2voRcPsKsZbL6ALVZQtqiEpBuklgGnxzlP+D1g4NcDuhAW+hqQLcBmCc5n4OVDSQKsARm/JsB/GAxbno84TGjVcaQy13hB" +
            "MxL4yy/lb54IKNhABMrjY9TnMUbHgShn0paQoA3isEkKeW7UiU/zz61S90QntPCxnXFOLbQrFFc7xqt3B9rbyhRNOPvy6boYeKFN" +
            "WEUbQQJQbyUfe9lnUowHtc5HsDHkZps1gFrPazwzhgz2Bk77E+llgvmiuuVIpSCcgMVjaHILvXhE4jMQFznYTy6D8D/gWfAiRpft" +
            "MLxiA1pAbGUBMBdAISRALAFfAWMdCfCaf8DPT6X1mitTQKz1D3/HgmaNANT1UdXZyjqLgBfduzpwPAd1mNAjXVUZxa6sO2905t9e" +
            "ojkHW8KmOsSKxcqPPeHSVXv9uAOZNsr5vNjROnSSjvzcL8vRUWdTFdto/bB4pHLf1wn5awlA0xrAwRrQckxrfT0ZsaXSKD6G66dS" +
            "+XPgGjLQ2mIJa1awzg1I/Ee5OvAy0Fg4NmDAUfDiR54BA1pQF8/8x0f8KQC7khDSxkUAMpHLZt2yVBmqSwXQJgG0VSPP5dxHDFCU" +
            "IGx/NCsa/mpbXdsDzmJk2eHW+rACvDT04w6kcCa9YT/UZcFde8emW1+3/4KbrYqtGsG/rTW3Gu9uA0TbNpPlBiJQC+HJzt4aGVnL" +
            "y9c1IRWJ34/utcmgUrN6YLY2I3Hk8dQowddl8xQyYMD+eDEjzPYYXpvXh9p43jbmcjwaV5Tt3N5n6lr++HfiqmIqmrYonsLqxCHs" +
            "IiECrNyHHBHYUtkVvq/3gevQw8k0yYePDudgCQAibT9HX/w8v2k8iG5i73s5h0bYE31lDF7bdU+JZqdsEyr3L/uSse6F3ftl3uEk" +
            "kbMbo2ntE++L/6SOOtZtrNFaAKq5BcUVCaul6mSjNhowKn/Up5q2ji9TTTX/ZDsRQDlYtWL9AD9gwHpc8My0D4bX5PWhU/jv8Eul" +
            "eB97NXxJEIJkHJ0fUs+j1se4ur+2i2I/lDC1xAqp6GM9AajOhfW7aF5px05fE/4PFeLPaYRxYkJtycOaO2zZdS6k5pToxx2IKn4X" +
            "zf19XGRTHdDWsbe7cuuDb1MY7HL+mSHWpfdOBPaqTc0KQNXBr9US4NYLtLkzEkV36AlAlOAraF685j0e48NfHKkTomOCvB7r+pvC" +
            "e7w3Nj7X9zfO8qRkwIDecO4jUs8YXp8BADqF//hvajsuPiI+zn0nqkyicfL4hso/aKPjn+P97uSYBDhrQIfpeH/ERXa8I5cgv+yC" +
            "ilWg/KX8c0AF/S8Mrr8A3HhFToZtO/e6Y8K61T3PvwQc7dmsfQAlEaCweGl9UeHvGtsPBIAIUBqUZKB0ApVNYdIJkGZAkgbzIEUi" +
            "OsWdNCqT60qh+IubRNiPrLX6lIEkOBznaQFXzJPNiaO9FZ7rDRow4CVgeHcGeFRtBfGwL8M7Vw+Nu048RgeNo1NkKbeNpiIhChSd" +
            "JH8rEAAFGo2hsgnUaAo1ugKlY5BOJfdMH6Dobte8Apcuv6xDqwhywHAQG4V2Lu5Mp/HjLgy++N71fDdwzL5St5ocFTUSHl9XMu1K" +
            "xt2mycUdHWv4471O8KZIg04gkEqgkjFUOgVlV1DpIzidAMnImUIl9Bu5SD3eWYiZQT4ohOWSvHI8L3DT3Ij62gBAUsZz4AEMjn6z" +
            "AMcRGyLLB9POfnVnOKYMGHBGGN6Q1412TVQp+FfdRRux84NbDxoTmMj1BCgFVhqsdRQ52s9pVWWXzFIKBIIejaHHU+jRFdT4Giqb" +
            "gBKXmLIHdBGAzdbpNWW14QxNCftWaeN9Rn+3jSwbR5s+Ba+eSEV/C4PPrhsI2kXIPQrAaYjcqaask0+NXuZF6ZFDfmD0Sbgab9b6" +
            "16wtljLpxGVhvILKrsHZA5COxRpADNgCxMa5gjIAK3Wyzt3Hm36cpB/WBruH72P9+5jQQXsUjzqxJcB9wn91SwBz5UZ2DRh0psqF" +
            "AQPOAMObMQBo9gPv9OOF/5o3c4d3T0wEvDgPCAGAToAkARSVHq6KYpMAJMEkQblEk2rkrADja+jxNciTgL4sAa133f192/NaEemy" +
            "zgUNIrCF+n4ry8HBQiW2nLjjmnQwuR4EgFeRJ2BTGzUffNSywR9kl/MHVFAXkFFqReAsAQRqEeqjAoI5dd11CKRSqHQCPb6BntyC" +
            "F49Q40fY8TWQJ1C2AHEBsoXEjQbDJ4bhwsAWki0yuPQg2lZMGmvePmaXcl7SzluTwxYr2GIJKpYgUs4RyU8Q/h6p7HnhZ9EcVZxQ" +
            "h/UDAwZswPCOvFw4Sy1bCevpx29vvm2Zjc1qDhs+C1iXu4Utx3nAKpeoCLbR36ScCysBUAmQjEDZCDwaQzkSoDQJCfDjurN6K9JC" +
            "AIiQTN9CX7+BvnoDdfUWenQNlYxApPtoIQGdWDY5V2GoRbm4bzXbuETDkeFg7FC7A4lAP+5ADX+IdThlD9mudbauUceBfd7Ri9fs" +
            "urdFuEDLnR7SAKSgRldIrt5jZHIolcGM38DcfA/z9m+AWYKscSTAAGBnDWBwvkDx8Anm4ROKh0+gfOHcgjho+r3SPij+43pGRIFX" +
            "C5jZPYr7n7HSGezsHubxDxRffoEeXzvTsAruUIAKFpF4eQMpBcquyk8yAikt6x5ID2RgwIABrwfOpRIQJUuxeICZP6CYP8Cu5iUZ" +
            "4CjDr7P22uUjzOIBdvmA4vEjFr/8HfmX32GXc7DLERDncmlJNO/keXE5Ja2htIaa3kLdvi8/SSJEQCsopSpWbnIKHa/00uMbpNM3" +
            "SKZvkEzfInnzA/TVG1A6ar39/Ub7NhX9OUrpx4awk6Pd+Smn4p5voh93oLV47s7X8zXbbGqEXqX34xCBrnboulLb8S3HbmUGLNUD" +
            "pYYilnYjk6k/aI8GIKWgsyuk1x+gVIJkfAtz+z3s/AF28QjYHGQlUYxfE+Dde+z8Aer3vyOn/+4mhgKAKZPHRASgohWq3JMcZ/Ml" +
            "zNM9iuQfgDUwj39IFuHJrfh9khIBP2y1+6iyORRAOoG++gB1/Q3U1TdQ4xtAZ2Iy1odriwYMGDDg7BEssqXV1toCxdMXLO9/wer+" +
            "FxSze7AtACPWV38c2ABsYZZPsMsHmOUTzOwLVh9/xer+N9jlXLIF15I5xhbZeEukhACkGVSSIbl+i/T9X5B89zek3/4NKsmEHGgt" +
            "ZCGa20qLt2xVOobKptCZLAzWkzuo6Z2M7w6HygHeMMJhaj2BDHaGrkGCZmvuU8WKeLKFi1Fv6KE9qWX1bn95AtoaIPKfO8MeEbDR" +
            "gtUm+Pvf6trgsyYC216pqzXW1GqjMx2FAahCBMIg2QNIQY+uQCpBMrqGLVbgYuW2OWBlQqBgPnZ3xYB5+ghSGexyDvXlF9jlDGCW" +
            "LMDutj0BIDeoxtwvNkVzvoB5+gywgV0+SQSIbArKJlDpSAR/91GUyGJmpUGUiClZsaw3SzPYt39FYgtApyDtXlelQDrFqXrIgAED" +
            "Bjw32LluMhuwyVHM7rH8/BPmv/47Vve/usy/uWytywjM4t5pVk+OCDzBLJ5gZ08wsxnMYg62pjJ3NbyAKtHn4nVnYyTX75B9+AvG" +
            "P/5vGP/1f4fKxlA6gXLjtdf8I7i7eldYCElQiYzrOgXpTKIDRSSgh0ar3Uz156PiTF2DDq1SY9Y9xTR8xHbsL09A+NLcbpO0bT+c" +
            "0Hmm5SFUXMXb/AdfG7Z96Wvm1Vbs0Y5EJC4zOgV4ClgGO/MwW6dF8iZi5goJKB5+Q/HlV+g//rtzu1FgqwDYan3irTcP+P2+vCKH" +
            "XT7JuoDVTEKWOg0+6RRKK5DSQgKUaIxKMgAQMZRiqGwMhgYlU9DkHSi7Fp9SnR7zpRowYMCA80DNkYCZxXXHFjDLR+QPf2D58Scs" +
            "Pv4PUfQYUfhwIACytas57LJcF8C5AfICXBhx+4wF5Ej771EG+SGxBCQJVJpBj6dIr98ie/MdRh/+6khABpWkIJVWtP/enOAogCu4" +
            "nienduEemq6+3f7M14dt7rxLbdrvdHw6BnUCd6Dzxy5NXZEBNx10Ri/T+k5aZzHHQiRF1xe60tp8czuVTyAwKUA5Nx6fJIwVAgkA" +
            "KreqdAKlElnopTRAGiALsWBUjT+VLccGVmcNsBZcFGAsYZlBRQHSOUitQFqDgxVAyd+kYUlDkYYldmGnhQTQ5Buo60eoxRw0WYHU" +
            "CJzayh134zUz0gGvBs+imhtwOsQjrpWPteJ2uXhE/vQZ+cNHGXNNDi4KJ/w7dyBYcL4E50vYfCWCvxHhn0tXBQFt7j2yLoCgtBK3" +
            "H62dUqf8yPwRB3SoMYvwd50APC/iOfiYYujJLBFb/NYgSfGN7/BYdrqnrcqNtY7HQ78k4Fn68ean9Syi+Blqate3VK2Vtql7W8O2" +
            "ncdrDqgNgM067tqIpbaFARBpyQHAtVedfURoksih3nyrxDffa2ji1GLBt9IL/jFx8aFDAdEsmUIIgDHR5JC4dQCyINhHjCAXMs6S" +
            "EisAAdZZAtT1F6jZI/RyBrVagpOxTGDMKCNyrenhVB/WBwx4QaDOLwNeBGpKFnjrrgEXS9jFI4rHz8i/fpRoP4WR9VzWkQU4109T" +
            "APHHOotC2/C4zs/bWQOUUsH3X6lyS0749+N6U+FVK7RBAPrtw+tlH275a9tz98OzqkV3adpjDSU7l7tZWju0qv2RgGcff7sZ00Zm" +
            "j1rn3Erdv6kua6v0rNhYrW3rvKv7T50MNEygVTJA+wyOkYbFr0Joehy1WwKEBHhLQBnZIeQIcCeVbkTVmy+/WnDBABkAZXi4alSg" +
            "6iIxqS85EiBrEdRoAnV7DzV/gFnOofIVVJGDK5PXPnasAQNeAAYC8IrgVDHRgl+bL2AWjyie7h0JMOWnEveTQdaCLMt6MOt+C+Wi" +
            "oaivzBt1GV6RWHK1ihYBRx8qLQF+bG8K/7XvR+6/zVnifLwUekebXPKCh4dDZ/W+1gSs7VHn3v5eyxtXNNb+bjx3wy+H1OsYbdcL" +
            "AejhwiE3QK+hLqMRgKh1PIhH9KAoVxKqU2rlNDjsJX8qk4RFJ1XK5uh7CE/n4v4TgWHd1k0IkZmY4espJEAOYYA0bL6CzV1Ma1M4" +
            "AlC6A21EoxMNRGDAC8DQhV8B4lASHCldSkLAVhYJy0cIgC2Ms8RHx4fEjW5sRTQS1i0AfoiOf4vmqqDECRr/qkU3Vuw0CQC19N3T" +
            "EoBN318jns09aQ90WW3qsk7DzcnB1L73liegEjO9xYT2nD2tIsyvq0csH9X/3nTukXBSke2EF6J4cD1C+fHDWtv9ajvJqfzJEwAm" +
            "wEazQk37X+8iLfOJ23K0JcSbChiw3gnJxcVmZliXFIedK5L/t3Ji68217eLuBdkDBpw7hq57meD6120mVC7dLyMiUBmGCXAp4OUr" +
            "Sdmxpr9b7nbqF68Xqg3cpaIqfHO/1QjAGoGfou9tWe5PBh4IQC+IhLJG+3U9zh4e89pn1fJs285vO6a3jMFADxrmI/XIndq/Tfiv" +
            "7z8E9cFrh1OoctLuFXk2C0ADrpHjAbN3oZTWfPO14MpO374UWQDEGhBbF9D8HpUfIksg3rbxdITkY/GUFXZAFP6i+GewZVjrJkFe" +
            "M5RX+lc3hWQeiMCAC8DQRV8G9iIAiISbUsph9iWI4B+nmWESUScmALHltnXe9CE7o6mIou+II/w4N9GK5h+Re6cX+X2dovmt7MrH" +
            "79QcfeLf6scMOAKO8Hh3elYRIdhGf33cPAG74pksBo3L1phexZLQJ3ZW8+/fyOdDAOLr1cXlU1eDGpNRqBF7MhARgnBQKdR7i7O3" +
            "hHVZAippteP+VTchwAnobmcQ/q2Vj7MGbPV6b+hfAxEYcNYYuuaLxPYEwAn6Pqs7vCWUq2U4F0o4C2psJSWqjpPxcFedhrzWviQB" +
            "XpAPmn8v7MckICwAVtWTKio7qnx7DjQIwMAA9kKY+ctueXbYtUr95wnY9Vw8UztGD7DOljex6J1k902WhRPhpG5FuyCMuLWfT10N" +
            "pSVr4/V7pG9/BFQqC3FXKwkrBwaTyzJMfmFuPEE51x1YF4EiB4oV2BQgVEeMqkaq/mQiGm8tbLGAXX5FMfsIeryWsKeKXNKatGqJ" +
            "CKZyLusHBtjKgrU4X4FXnzGOYIUZcNZ4tkF3wKsEc6m9txZsC/HltyaSRp1YH9ZTybjlSYDkenEhQM0KNp+hmN3DrGYS+YdtONcT" +
            "gXhUC5p9phDrHyoJEeH84l6ldfD5V4oiEiBCjkpSpJMbJJMb2d5+cJngx5Lskfy6spIIlDU4NfyLXiVCG1/9vsaGix1j4gHy+efG" +
            "fZtxW6LXf56APdqsMicdQWBuFLWDRF9/hfbqEr3dy2EFNerfhxVi3+cV23DjAbP0kdl05Z7gep9KoMbXSG4+IH3/F1A2hcmFAJhV" +
            "DvHUl4+EnfP+qT4ZmYVlI5Pcag5ePoHxBNgn9zbKOYEAtHYmF83Ik1NrJfvw4gH09BEYTWXtspKIQypJ3R1EbecJQBRFg9lIOvvR" +
            "NdT4GlopsNIguDClR23fAc+DDS/mOROBnTvk0IPPFn6c9OubTAFbrGALidnvFSil8O/GrWD1jNZCWSEAbFawqzny2RfJAlyswGwi" +
            "IlDvERzcOGXaIVGEpCNQOoJKRpL4K8lknPR5XJQjAih1VUonSMbXSCY3SMY3SG4+QE/voNNJNbw0nUr7v5uwus4K44XG55VxzgPM" +
            "zVY4hq5s3dPbp0nbBP9NZKC36EBUn1S2EA7rh1cO3bXB11xnY1Ed0n3sBnQwETgThPr37YbUJVRs6gcbKnOatibRAo2voW+/Qfr+" +
            "z1DjGyEAPjoPWwDGbW2I0sMuQoUxBtYW8pl/BZOGtQacL0AwEH8hQhnZR5yRAN/PIvITkpkZ2GIBs/gqJCAbAQpgJ8QrZwkIfqh+" +
            "svXZkW0uSXNsAZVNoK0BlIJKx6Ub0BklqhlwDKyR9s/AQtlA3+PSgLOAF/StKSTJ12oGs5zLnpDJ3SktrJFs69YEEmCtrZEAZwlY" +
            "ziRTsCMNpdqu7NQywjkCwACRkoy+2QRqdAU1mkBn5UfCfko2d7EGiHVBEUBaIxndIBldQ49vkFyLJYCySZQjoKbYOmKrVv/uuF6s" +
            "WeoqKdq9s4xzTuPHFth0b51Cc1vDHPCI1z29Q5u0lQx08JjjuAPVrWBbKqMOUk71rNnqcgl6CdNOvHh1M3a44y4i2CiGwpbQUhkn" +
            "EJ+yrUklUKNrJNfvgXwBPXkDW+SweQ5T5E6jXgr+8Fp/trCmgDY5jJFjbTKGYRYCQA8lm4w61Nqouj4ahrWw+QK0+Iri8Q+w1m5C" +
            "NLCmAOk0mKmJyK0iZpcIxwA2B9sCsCvo8Y1cNxmBx7dwaYklidpL6NQD1uCc1f4RBgLw4sBAUJSwMbD5HMXsC4rZV+SzL04p4i2l" +
            "1hEAUVywFeE/WFhNDrYrsMlh8zlWXz/CzB9gi2U59tUctavTjnPz0QlUNoGa3kFfvYGe3EKPp0hGV9CjK+gkccm/JBeABHG2krtF" +
            "aejsSj6jK+hrsQQoZwkAlaGmy8s+U1/1xhX3d4tyOxx2qupcMgKXoubvl4z+3YGOiH2nsrog37Uv/i1+2Ed9yM/ELNoue26d+RRN" +
            "E/qU0lCjKfj6PRIA9moOW0hsfm2cqRl+Ua63BJS+qiZfwRQr2GKFIpsgtwa8eIJVn6MOVYvvH2yB7k6jrMMEmTztcg483cOCRIP2" +
            "+AXq/ncUk3+AdCKWgNiCwAxYSFIcLkBsQGyQ3LwHMUOlE/D1ezdhEcDqyC084OxxIRxhwAWCrWjvnQtQ8fARy0//cJ+fIWNimZdd" +
            "SIAjDTZytbTs1hLIx5oVVp//gfzLR9jlouJyFFBRQik35mkgHSO5fo/03Z+QvvsRyc07JKMrJKMp9PhKSIJPCOb8gAhWyIDSUMkI" +
            "OplApWOoyS3U1VvQaCq5Zpwr0PNw1A0zZphYqj8N7/5uqLRyTzq051Rp9Bsd6IyVM7F+YBdrT7xv8xX2xKmIwFoG21MFDiwmHqfi" +
            "7TFBAKA0aHQFDdGWK7MqI/JYb652E5X/20eqMAVMvpAMlvkSihR4+QT78BGGEhe2zgKg0ipL0d22jMTyk1tf8KSAIgfNH0DJH6B0" +
            "Ih8VTTgkJMJFyAOxaK+U0AfYt3+CSqfQVx/ARQ5WqZsYhxlgAJ6XCAwWgJcJv7jXae7Naob84Tcs//gPzP7xb5j/4/8BYMvlYZCF" +
            "wwjjrltHYBmW/XfjSEEBM/8CO/8Cu1w4S4CUAaBphVYkmeBVCkon0Nfvkb3/C8Y//CuyN98JARhdIRlfucy/UVKwSHIQS0IKUilI" +
            "p+JaOZqCRldSfhQl6LksAFz9p2Vn/bgty3TY6E6zXZEXi76e6rmMZM8eHWir8tF/x2pz9fFbf71KgsKNpfVQoWO24tqyT9sdPWEM" +
            "bukbjj8JEVAKanQFJCOoyW1Fq8RtHSHS2rPJYVdzmNUcNp+DTAHz8BHF6CeZdKx1ZmJqnN/oO65RxB2IgdUCtsjB84doktEAtPAH" +
            "RSURcFoJHzFPEaBJfFmxmiG5/oD03V9hixyUGJDVGLyBBgQ8xxqBgQC8TIR1TSUJsMsHFI+/Y/n7f2D+n/83Hv7+f4CoJAFEkPVM" +
            "kfAvgj9cjhSUyRPBIFsAXACco/R7qSF4nioXDUjWAiTX7zF6/2dMfvgvGH/4syMA4hIUlCu1IsJ3UoBSZd4Ab2EgHU44j3AL1OkK" +
            "tEuI0F2Gg5dOAIBnc944Gs4rT0AL2tZhnENHO0pHeGm965JACqQVoNOdZSE2OczyCZRmMMtUFpulI5BKUGE7WxXm1gO4lfYhLB4i" +
            "YuomRFmRDxctyK2wiBRhWhFYkZCB+TvY1QxscoRONoQGHTBgQO+oqZvdGipwAS6WQgaePqH4+qssuiWAlFNkWHZBfkoCwMHdXwJ/" +
            "+gBopPy5CNGOKxNoxRpQMg1SSqIBjaZIpjdIru4cCbh2loCawqYDRF7ILhVC54FIwVTXbmK7em7yiojdYQZcNi7CEgC0C//N3w6Q" +
            "orc5ldd8HQT47dDG6nY85RRYP7bVel2IylNOdrI1kMhA1jsRReims078L4+JDiWv/XcqfO/WHy8L8IutWZGLJETgRANJAiQJKEnd" +
            "egL1bCbrAWeMc9G0VDD00/PBhs7B8R+lkE8u9CYpBaVJlOdAKaBDNNasGGwJrJx87ciBd50Myv3gCunGy5axzGsnRax3UX7Ygjhe" +
            "gOzHa1vJsdLa46JrcIc6/Xl7Kjf+avN42K2kzQec3XBRx4XJZ40h+Ihj8sEkQKt+q7bNc1rfHhWn68o565jtmlMbP3W6UFxYR9sb" +
            "+7oNtD447xu0/lKnQqN6DWfIaJCNOk4ZMUgmFk8AiA0YBn5CXC/811PclMcFIT98d1YAIpBCZIqjYElgpcDabZMEnCRAkkp4PJ2W" +
            "i4O3aIvX0K0HRHgO16BWDD3vfLCjCFkOVpJZV4m1lTSBFEEpSDx28u4qFEZJViz7vPnTaxpbBmgxitbme3JifEVl7aL8eBLAJgqj" +
            "LPlUynFa6tO8rXay0XH0M6L5rHZ6lTcN/mdOAFrr1FNFe33Oa8bZxq4jjcln7w60P0qJvGIVrB3hf+vU6nvXxq6Grwv+F0oE9upf" +
            "+7LTM9X+N1C/ty725xfAsQ0EwFsDCAaMMolN1/QSLz5rI7DRfOoVYjLREWSZQIUkOM2aJlitAK2DJUCsABmgk4oGbpumOItnMuC0" +
            "eFarwNDjLhNVHTSRCP3kwm6KNUB+86r+OLJILIaDuEzcFFsB4DXx5NyEvFUh6jWlycD9zi5ggrPSeoWNLUKyMW/VJRCYuN23fw0R" +
            "ODe0WQP2KqRdN3X2WKvUO6DMZh+rfba51pZd6NhWgYtxBzomLqhPHwV1krRWCbDTk45el0bB3dp/OF/LZ+9UGxqjSR45LFxjy2Bj" +
            "YYwFm3KRm5i1gUhcl4kuLp4Y9WQO9XHFE4DSF7ZSEfigexS+R/TDRb4YEoUNeFYMXe/lw48wj1soAAAgAElEQVQzSosLok4aC2+9" +
            "sl8sqlwZ0HayPbAbOsPA54R1HzINDJBEH/LJyLwVIM42XDKJC+6gxyIpvPbrWWBTndYRo53v5xwbYEdcVJ6AfbHXc+qSQjt6Tlsi" +
            "jksdQjqJZh831FlGtCPStJyaDHSybmrZ6+aXpsGAYa2FsUIC4ImAgcTwdyotTwCCtcmV6W0GwTUo0jY0lQwcTqr2Qa6QEr/gmKlW" +
            "4JZtUVrVL0cLNiDGAeqjY1gDNnahoY+dH3bsCMFnXwMqAXQK6ATs3BDZ/xeNofK3/OC3QDkmBkSSHLnzZMhjH425JADk1SAWTC7k" +
            "s8tKjEAEohwumwjAGY1/nrc0q9SqHTrgQmu/Hg3+uTdt413HYqvKtRGBk8nz59N9ALwGEnAIqV/XKyLpaFtPoUsCdX7Zp6BIs935" +
            "psbCNTX2nJoIdP9QkpPya6xvj2JbW4YNBMACRiwByingCdzM3uyV86pylbDLEwVyvzC84cCzACEP4a8QZYOrM8aWCW2qvILL7RlN" +
            "hAO2xbZ26jWn9o6hH10SqD7+tXzzOUv8GVCSqIt0ItYApcHO2usJQOzaIyXWCEDsj9ulwnWRhUoSIIuSZcAVluBJQEkE6paAmnX0" +
            "jMe5SpOEIXlNfS9MIOHav1XbUVNG6ChAsKFZToYzbP+XTQK2U3hu1m9wdxG9dqAz7CD9IXpxNwxWzHz2UWsqfSbMG/IHg8CkwFCw" +
            "SMJvfoGcuPE4awDVynT7WHmLAUdXqur/S3LVNigyiN2kyn7iM2Aj2TahlKsyNc5tv2E5rnpO7QYGDBjwQuCF8Ij4uwzp0RHRQFgb" +
            "o9xvkvl35TL9+kzrthT+vSWAyxE1jrrDlbLRnHAJACtJwxJ+cFDkwofKegTSCZTSsi5KaUcO1uDCxrZABIJJmHwjX6hssUkL299N" +
            "7S3H7VKFdccew9q6JV7uwuCoPpvadmPbr9H2V9652nV3xrm+rEepVzxpNDXelwSJfJFCpSOAr5DevEf24a+wy7mYwa0RlyBrJDIF" +
            "MRQYimz58jvnfubcTZo5rFlKxuDVAnY1dzH+Ad9eTb2uf1BCDrxpnBiwiyesPv+E+T/+LyBJoJIRiBSUIyYIJTobA9ugHVPpCCqb" +
            "Qo2moGwKSkagdCwfnR29fQf0jWeccSq4tDf99YCtAUxR5ilZzcG5jEVsi8iaSAhBEdj52scuiLaAMUvYYglrlpj/+t+x/PQPmPlX" +
            "WR8Vhv+qJQD+JwAyAXPtN7iFxkmwMJDKZDxSqRP4fSQiBZWk0EkGpTMkkxuMf/gXZO/+jOT6A/T4BiqbSMS0S1/g6FEnT419l4cu" +
            "MaSP29llRKxax3u4eJ/leOxwQy9rYXDftWgT7lsaN3bTOGZ1Dkd7z2h7uSJjbvdB+1ZhHSquJnLRsyOYdRBJCvlkDICQXH/A6MMc" +
            "BECPr13UIC9UWxAsFCyILEQXJkI4g8D5DJzPYPMn2OUjzONnmMfPgC1gTYFY0G9/x2MiAJmIQTCLR6w+/yeQJDD5E1SSQZECkYYK" +
            "2YwVCEpqYn0EDQM9uYG+fo/k5gP0zXuo8R1ofAultEy4Z/+ABjTx3ERg6DPngxbB2xSihCiW4NUM5uke9ukz7NM9uFg6Fx/JnCva" +
            "/txp+01pdbQW1hSwxQqmWMIUSyw//4LVp59RzB7KJGCeAHRp/P3kGsXwBwiKEqh0BJ2NoLKxJGkcXcs2SaG89j/R0OkYKhlDpWMk" +
            "42tk7/+C0fsfkdy8hxrfQKUTUJJWtP+X30Nb7uBCCYDHYdXvR4g5V10tgPZhfcNQ/7LdgRz2emA1V6Lg7dHRmOcvB3WzmC4C4P/u" +
            "hQhULuv9qyJrgF/ZVT/t7NsVkGzDCRTGkiX45gMAghpPkbz5DnHoObAFkeQPUGSiLJjiMmSXX2Hn9+DlPczTZxRJIhq5+SNAi/Ua" +
            "noBSa+bN6WYulgCTP2H19SconUIpDUUaihIAylkElITRswVgcpAtkNx8QPb+z+D3fwHsEmwKaNLgbApKcRHuWwPa0LQlne6aA84D" +
            "7YI3WyPZfVcz2MVXmIffUNz/DHP/C3g1B7T4+UMrZy1YwZoVuMhhrQEXEn3HFDlMsUJRrFDkSxSPX7D68jvM7EGipQFrJyCK5f6K" +
            "xxGDlHaZfydIpjfQV2+RXL+FvnoDPRo7IpDINptAZ1OobAo9ukFy8w3Smw9Irt9Dj29ASQbSaUtlXgjOWnI9BeLOU22INfrdVlDH" +
            "l8hJtnLFZ8OWN/Ry3YF2QJdYHN/Ssz/QE+H09xnPPOXfl9SdCASoBASSyBeTW0Ap6PEUyc17AHBmbpHKCQUIBoTCKbdKEmBmn2Fn" +
            "f8DOrqCyFLyawz59hfETVMUHtzmYVZ5f6XgLmy/Aj59g8kfQ029QKhEC4LaAFrcmaKmjzaHMCmQK8OILSAEqG0FPbkDZFFzciptT" +
            "ebFGfQYMGHDOWDPacwEuVmKVXDzAPH5Ecf8zij/+P9jFI6A1yOcgsYW4+xQrRwaEBFhjYEzhSECOIl/BLOcws0eY5TxYKaUmHFwi" +
            "N1aZZcwllUCnY+jxNZKrN0jvvkH65lukd99Aja+gPQFwJEBlU6h0CjW6hh7fQo9voMe3UOlYQpgqfRlKpwEHonuuaov2tw0uudu8" +
            "LHegCsoq7fxQqc72CPSCacDW4tuh6x3WfT/4AqdHLHRLchklk5NOxbzsJio5JlZpGRAsCCYiAaEUGJuDzBK8nJVmap/dl4GQRyB6" +
            "cGFpga9czK0ILkqQAfLcWdcNmDRYFWBosWbAZfZ0lgC2BcgW0PkKbGRhH6Eeri3GQAQuE6dyDRr6xvmAu78yi0uPycH5Erx6gpl/" +
            "gXn4hOL+N/HnVwrQ4g5kbQFb5LAmhzXiEmSNde5Ajgi4j82XMKulWAp8fwgejG19sO6Tyy4Ogh9nx9CTayRXb5HefYvs7Y/I3v0I" +
            "Pb6C0omsG9BuDVQyctuxEIJsUi4SJu0soU4BOLCBZ8POI1HDdaxld31qaliWtr9wXBTxeY5qu4zoR3MHiitw2kba/WpdjUVO+D/H" +
            "h3xyHKMRXgCvIiDE3icogBhKpyAAVmlQOmpx4ZG1AARbuu67Q4gNyCxhzAK8fBQSoDM3UflJs2pwlPmTK0QgXC0sD2DAGPndWjfp" +
            "aTApWFLOCkCQfwHYQupiDWyRg00BRJFB6qbPEgMRuEwMz+z1oMP3Hgh++bIweAUuFuISNP8K8/gJxZffUDzdA0qy/LIiWO/7b4yQ" +
            "AEcirN9aA2MtrLXgonBkwci4VF3+1VpXAqEMO+qEdCYR7rOJaPSv3yK9FRIw+uZ/ksW+PjsxufCkKpHFvyoBuUXC/nucPf3VEIBS" +
            "g9X3wXufu1EkWHfAGjLAiDjmgXJHzVP8iDikzbfHUdyBnk+26zDx7FHKKxkGAGxxr302RptFgLt2XgbIm448I0gkOgXxqEW7Vfrr" +
            "e+1XxVJgC6BYAPkcNruG8hF4lILEwZMg2BwmxIgItGzhqgRmFxfbAkQwJIuAKSYAFKZcRwAsiC1svpJFydZu+W4MRGDAgPNE9xgb" +
            "x+YHG7EEFAvY1RPM7AuKx4/I73+DefwIS44EEIXEiNYYEfSZw4JfGxIWAtbnK4E7xlfHrw9ryDxUHchquyUi28S5A71Fcvstsnd/" +
            "xujDP4mfP1H0kfVOPkxzZRvMqZc9ZjXIS90NPh6W6wLz1re+69i+/YV6IwC7lLEHMWC4ND7Aiaa57du86p3QohRswQt2B9oe9YZr" +
            "7Fx3Yt8VeTFoG3miXR4VqfLSG4Dc/xLpogy9ueksDgt4VZKBdQqrZVEbqUSicFTKco3m8wPwepukPIVykvVWAxBLkjEIMSizCTN8" +
            "THBiC8s2aO0kyU+pNRswYMALQc0lCPChPi3gQ4UWS5h8GbwRLYlwb62FtQxjrU8pgDIbMIffSltD7Vp1V41ADOruQH63HwOVs2om" +
            "otlPRkIMsmkQTMjnOKnlRHlpGn8foIH20ch2EYSdCtn1QjuedtA1eyjGoVL7vpqmIamfTiH6bNGBTqnz3eZaO3vFUv1LTzam/XYf" +
            "hI2vZW+KXVdQZ1mXaQloA1Et5vX6oyVBmJ/wvPYqMjy6aQzB6ScW2KleWnNMrzc5hQRj5fnsTw7mGWejIP9Rbm2C9519WZPogAGv" +
            "Ey3qUeceQ24bMvySE+qdMB+E/EAYovL832stvVwOObHsGgaj6OMtmqBSGWHZfazLO1Beo0oAqld9aQQgxt6z6Mmm320ECm79c+vi" +
            "/WbrOXgzNspILcfvLFNuLzJ0HrvrdZ8lOtC5inrNW+D2yraaDjpp9NpdHRfeZtdp0Wkq2fbk+ole+CyFzZeGbSaakigIEYhN2CUZ" +
            "aJSMNsNf+MU1bcQp3MXqT4LDv2USYP+bn1C5tACQch9qfeF7oMIDBgw4Kbr8IwB4i59zmwEBTCUFcKNDxf2nov2HJwmIhLGqkF65" +
            "bhiIuqdMDkokQhiiDAOGwZZbF2pSUJi4sl8wAQD6kxnWjedt19ht7O8gAj0J7V0EgCoHHB+bRL/WE3YhAlHh+86/J3cHeg4hYfeX" +
            "IuqgQVDt4eJtT+kMCMDzDYnc8ffrQdVi0GGurpix652o9p2re2JIlKD6oZHQXzlDMhtzhQQIESiFgqiuHbUbMGDApaA2eBAARSAV" +
            "WwM4EAHLVaG/MQ41LABdI0MUfKNTo0jBUin5JKk0NFhZgFwyjvgG6mPqFs0wYK/xfPexv9X80xPi2a11Vz/Ysj/t1DZ7TKKHdOsX" +
            "mydgm3bcdMyh8v9GnKzNGroR7K3aj9/bI7bNNgbDl4PoeRABpAGVSO4BlQCkSssAZMr0QWtreTYbk1zVIr9Li1KZv429GxKBoVCJ" +
            "7e0H28oDi+6HEf1dZyjRVHNuA8iAAS8OsZC8xezo/O3ZRRGDc8GpyNpx0Zs4gC+5cj5XquVHGRkPykHFKzDY+vKovAjLPoq/oz7a" +
            "UW07YFu0qZ22Pfbc0HvddilwW6Fm10pu0AtuQr9rAnqS3PYUTxuoOUnsX0ifqI5tJ0DXhapOITtVZ9eOf0A57OeDF4+oY5B2C4JT" +
            "RwZcHH9SYBcdyIX8kTOp5RlGE+s2BIArkyRXJldZ1FcmNPMfX3bIodGwMviyvIRQO4AACasat8GAAQMORqdWdQe9riMBfvyRoMZu" +
            "bUCHEbdBEDrqQd56EA8L7IR9xEQgIgDRmBR2MEBRUkQ593WPI8cQwncRW3aTuNoY5fGwf884nN4QotehqyL7XuKA6vXvDnSggFsf" +
            "Ww57nX3Fuluoy0Nnnd7gYNeHk4xRZzgQcm277WmviQg4YR+UlNYA0i5EqJvi3GQZ90CvOAuiudvvF9jV27DynoXzZAZmJqdV40AA" +
            "ZKwmSHA0im0AYdu4l0AArPtwuQsQCwOhkqRnwIABB2KTW4W3zq0bVMOC4MgaABVF/onLAhoTN7f9WN0XC/a+UElY2DQUBp0HA+Da" +
            "QBYRAJSnuG3TJvDSURXGjlHudsdudfVzNhsEHCzxNUqqWs79jwcVvXf1TuIOdFLF9x5Y6xzT0ajd93PuBrEzQ5uWOOzDeXecvhB1" +
            "GSIlgr/OQKlLdT+6hhrfQE1mErLTGhBbJ357+7gNRXkXIcsAWXZBM6zzm3W5Ahware5mW2l6kmdjJduwLXKYfAFazUDLJ9j0oRQk" +
            "Ks+Qyg9buR7LtYk0oLWEPXXuTgwfBrX/ph0w4FWh4pMj7yRHf5fCOddO43LLDFusYHLJAmyKQpKBReE/qyfXtv5L2zTopHtvASRI" +
            "tl6QBqCgXDAERRASgigYAagsljSS6R2SyQ30+Bp6NAWlI8nPEtYrva4BpXm3z3v/L1ESeok9qi8SENYwrnvo59ohKvWqmwCe8amf" +
            "WgY+zfPpmBki7c5rkf1boRJQNoWyBbQ1SN/9DbZgIJkgffwUhHmukIDgKFturYUxfvIuYJaS9dPOv8LMv4K4FPRbn4ibQ0kR2Cxh" +
            "Zp+R3/8ElaUoHn6D+nwHPbmDyqYAJKcAvD3fWw2YQGwBayT5GAhqcgM9uYGa3EKNrqCyKSibgFLnejBgwIA9EQnyAMBGMv9aI++n" +
            "Gzc8MfcRwJhtOa64j5l/QjH7DDP/hPzrr8gfP6FYPEm2X0coKLpq0/jgwx5XqgZSGqRdBl+dQI1uoEa3UONbqOwKighKOTLg8qOw" +
            "cmsS4rGKFLLrt0hv3iK7fovszbfI3v6AZHoHSlKEJGDRRPLSogLtLWQfPNFvO0MfW6Loqse5Sprnid7WBDhP5U5cxGPpbYyIh8fL" +
            "wPFrWnthG+ZioOqI9crgbpu0ZMFkIkBpJIYBPYG6+gA7fywnbC6Ff3aeumVCMNHcm3wBWyxg8gWKx08oPv+MHAy7kHLCNb393X/3" +
            "zkHkVtTYJcz8M/L7FOAVKJ2K8J5OQEkWXH7YJxWzQgCY4bIOGyg2IFJI33yP5M33SN/8AH39ATw10KTAyfi1PvkBA/ZAu+U0LNZn" +
            "gK3L+us/nhBYCzYGzOV3awuwMbBGtsXso/t8QvHwG1aPn1AsHmGKIopmxm0bAKJEYGdVDP8yi7tjkkFnI6h0hOT2OyR3PyC5/QHJ" +
            "1XsoraCVglIapF0kMqXBSiMaoAAiJONrJJMbJOMrJFd3SG7eQU9vS2uAP5jKMe2lYee76m2iPxfZZrd67Fvrk3pwH6Fp/fTeVvTB" +
            "JKBAlEL5QkGouhlWcNADOcwg9jKHLaDNHN2Ol9sCXSCdADQFJRkomQB6Cj39gOTtX2GLlRO2nQXAb90nhOpjOO3/I8xKPvmnn0Bg" +
            "2OUjivtfAS/gR4J/aG3PCdyKY28JYM5hll9cxCK3cJmUq5NxmkYhAfIBFFsoW4DYQCuN7Lt/wWjxr0EggFJQyRgY22ZjDBgwoAVd" +
            "YycH90p2JMCaHFwswMVSBH9TgIsCbAuwsxSyLWCLvPIpZh9RPP2BfPYRxcPvKB4+oViKJaCeBHH9SE4VMwGRgkpSqNEYejxF+vZb" +
            "ZN/8M0bf/AvSt3+G1lo+iQYptyhZazfmRAufiKDSEVQygk5HUOkYKhuDsjFIpwiDGF4uAdgJr9q8XmKfJlh7Dh1BZj+iH5UCYGq/" +
            "PVvG4IvBK3IH2hW79FNu+3vn9RYvE5V3npwGTCUglQEqBWVXUNNcJnFR8aEkUiUZ8L6/DIgv7/IhfGANzMMfUNlEzOxsQv/imtYs" +
            "VAoQv1xbwOYzMMu2XBzs3LgcCZA1BwAbF0nIEshaKDZQ1kAnCSi7hp6+g779DjpfQhVOQzlgwIBeIGOJhc0XMPMv8lk+gPMcXKzA" +
            "Re6IQC6kwJEAE5OA+T3M7DOK+WcUs8+wT59hFzM5vhLql0KysM41ABx9iEBaQyUpdDZCMr1BdvsO2bsfkH34ayAAWmsoHxlN+whF" +
            "YUmBK0fWFCm3hXc1itYXDQRgQAzpov33id5ltZ6IQFsxdafbo5KAcxdiPTqtADGe6Ub2a8N+K3tIX+yD0F5CH+ofJIl6dAIFwBKB" +
            "dOnHK/B++OUM7C0BZAsErZki6NENdDqB0imUIlgrrcpB7g/qf1eK/1XKtsaAkMtaAvaLfp3J31knGOw4ibcEKCi2ksyHLWAZxgCW" +
            "FTgsCPTxxyGh/sLtv86nPmDAQQjhexhm+YTV19+wuv8J+ddfgNUSnK9kW6zEXcgWzm3IrR9yLkF2+QizehJr4vIJdvYVdjVzhF1m" +
            "pVjpEGcIBqqKnpgDyFoB5/evFXSSCBkYT5BOplA6EQKgvSXARUtzJMAVISSA5BjyIZSVqvj9v5YRpG8566TtFkmpp1lI3HF3+178" +
            "RP5FO1fPnxBvO3AUElDX+j7Xy9jZaHGL7lq50/TUCqpteN5DW8UvdMP+9h+qOO+77Q+lKw6FB85QICV7FWkwW3dc7JPrZ92qny5Z" +
            "I6H2lAIpDTO+hsomUEkKIpcFNDqD/LWDqq060cviQgaMKXmH9YmDnA8yHEFgIQDlugCnKbQMa4SASNIx/6EgLIRahXCitLFPDRjw" +
            "qtEYAhjM4vqXf/0Fi9/+HYvf/w4s58BqAVouhAS4iF1CBoxE/3HrBWyxko9ZwuZLcO4IhDVB3SBwGc8r5t31b6kff7TS0GmKxJGA" +
            "ZHoFpRIorWVNgIqzkyt/OV+IW8ukQohhn9m4GrP8ZSMe9Xt3dTkVIl/4w9ziN53d0i/qsuCest3W7dg4cI0gWrudraq3xz2cxB3o" +
            "OYjALo0Vt9va856BAHhIG57FK9uJetPUn3vb/vZbOvc7PTIo6ObBbjIMyXVa0fxdSACJBk0lpSUgEUsA265Bh4J1ICgSxL9IBAbm" +
            "4IHkQ//H2j8R6BXACswKxASyEMtAwjAFYLmMP14P5dfoM68nScSAAYcjhAK1Ygn48isWv/07Zj/9n8BiDlrMhAzkS7Hgwa3nYXYW" +
            "u2gbfUK40SjbkfxZUoIQZhQsCgBfpbAfYnCsWwJGmZCAyZUT/oUIUBD8ozGi1JZUt1T+/VpGi8aov+M4ec7ttLuoRbW/62dvebd7" +
            "ynhbybidBGBDCftaS+rWgA70EyKUSo+aMC6cUQ/bZBHY1wp02luk/a95Rs/Cgxt/1PeeYaVPDnJRNiCTZ01YliPib/GpLFmHdeL8" +
            "bydIJtfIrt/C3H0LW+RhYmZEgjbVymN2iwlX4HwlWxJBIc4aFGwSIUSo/9W7HREYBYriCavZZ6gvv8KmKZLlA5LZFySPH6GSBH66" +
            "J6qWLQo+R4iUAukMSMagdAxKxpJMDV47OPSdAa8PBLj1PS68JvysYcG2AMwKXCyB1Qy8WsKzefZbRhD4K/kAIiIusraSxbc6lTVL" +
            "zh3HZxaWAz0JoNhDCcl4jOzq2n1ukN68l1j/2QjKWS1Jq9IKEAv7rWrc2i+v+NXnOFZ7Dd7AHL6fpkq7oVcl6xp3D2r+tFOxcRm7" +
            "lFM1de9Zgf510f1kDGYZa7o1u35H/2p0KbXfLh0EGrTXuI1ctR3XX626NaU7nHo0bPVU1xz0TMaViwG5xXeV3xrfWjQfzm9WKQ2d" +
            "jpBM7pDdfAN+92NY4Bdr8yCePBCNX6TBW85hFo+wiwdYNk5osLBBvVfCn0+w4b1kJTss5ShWD1g+/Q77eYLcLpFMrpGMb5CMr8UN" +
            "gJyDkCcjTnogIlnPkKRQSSYJ1KbvQNN3UNN3gJbMyqwU6vHBBwx4+Sh9KST3h6rG408SsFLC0V1uAPh1PO70YERwKj0f57/hZ6+0" +
            "i8RzBcp8JLNMEhy6GP2AIyQ+wpiTmPRohGwyRTqZIp1Okd19QDK9gUoycRMKyg73iTT8UQ1a0ZR/X8sg0CUVNOeMU6vXdpb69tV6" +
            "t5ZF2xVQu1BQvKHWVn00nJ9vdy2romXf8CwJiHRoG3ECd6A6I+tH5CtLopbfmsceE1131M8L1yxh63KfgwBsUjd0NZafwKJzXrNW" +
            "p45yIuaOJqaqPE4AucVyUBoqHSOd3gC3H0DzP4NNHjT5oUQqzfkWBK/sN/OvUErD2AJmNSuFf8uw4ZyyPv6XiuWCZOAr8gfYx9+R" +
            "a0AvvyLJJkjSMZJsAq0UNFkoYmiK1IdsQaSgs7F80hH01QfoNwsoKFB2LYKPBqjuyzRgwIVi++Gz/JVcZl0idrH2ZZGt0gmsUs76" +
            "J1nHy9M52sTSBrk4AfFMyyClodIJ1OQGavpGsppnE/mkYyDyzXepf8FE8g6nGZLxGOlojGQ0Rnr3DZLpLXSauVNKa14l1n9no7zW" +
            "l3xb6bb2S02YPAW6ntBaD436n7WD11Z/jRGgfhx1FLRO9jhMLonN23ucvu5VQLvwv0nq7itjMK9ttJ2qtAtKE+UhJXadv02Zm445" +
            "TBbpPnNjuec8NnL9z3Va7gFVdOX4rWsw/EQq4UZ1OgKmd6Dbb6ELCfVX+gM5FSCx0xRK3CHDQgSKx4+S+Tefg+dfABYrAikbBp2Y" +
            "ohB8dCGn8fAuBsQo8kfgSYHtEmr2CYlO5ZOk0ETQykKThSaWazID1kKRkqRA4yl4dAW++wqoBMhuQdc5lHIJgqjWuYbONOAC0dVt" +
            "m3NVc8r3wQQQ3GsSUJI4FxuXVpBdXo44NB5Xywk/kow5Is6TkIrRBHp6B3X7DfT4Bnp8LZ/RVFzzlCPj3q3HRe5RiVsInGVI0gz6" +
            "+hvJIJ5mTQLQRgIG1PBK7Oh9iY2RgaluXzp+S572WW17T/24A134Os7WhroAAWJtFc+6/rVX0PuduJ/YdaizvoUzRUkECCBZZEcA" +
            "kF0DVx8AY8Sf1xqUPj8irDPKrWVCIsp+6NHPEr5zNQPPPwOUg7ECGwuCjcUFVwlfJjnhv/yduQDMArxSsDCwSQqrM1ibujlfFisa" +
            "eBJgQcxQ5LSYpACVAkUBMgYq+CvHfWqTOWrAgDOGC7u51hLgyW6X9ooQaeM1QAmYNJiUo+nt70WL7liOVkrkekXQkwmS23dI3v0Z" +
            "+t3foKe30KMr6NFUSIC3AIQ6lBF+JA9ACpUm0EkKPb6DGt+C0omQetUMFjBgQB3bCrhbC/enYQFniaO7A3W+yvUd5/oAzrVeDmut" +
            "XmclRW9REX8z0UK0s7qFs8H6EUtcIf3iwAQgBZVdAVfvQSoFj27cGoNYkCj9FEVTWPIDlY7B+Rw8+wzz8IvLA2BhqSijirpawVsG" +
            "COJ7HLE5BssCxWIp7kGcg00GTlJYmwEALCyITbACkCcBSoNJCwHQI6AooIzPnhxrDREtpA4319GGAwacEToz8bb0Ya5+4fpEEBQB" +
            "XhOvSwF7jZBdFhNdwIX0VFpBaUIymSC9eYfk3Y9Iv/9X6OldIAA6GyO8jzERcb9JVCBZp6S0BqUTt65g7LICq411HDAAWD8LtqqD" +
            "KNoXTVpdLkFdOHeZZFc+06s70NYN0+rM5Avb8eJxWRvO9WOSF1Q24swJwFY4ux4bJE1sW7Gzu4WzwPoWIWLI4kDRxtPoSiwAo1vw" +
            "9TflgdEKKC88V9YJMECkYGefYb/+jCKbgA2Di0JCjaIynkbnl38LAXAfWwAFgzkHzBJsM1ibQdlUUp9ZK1YKK8K/WAIstE7AKgGS" +
            "EZBMoPIC1vHGuj0AACAASURBVBpZn+AEBu+TSKhLRG0YetWAMwK3TDaN38p3tfOwSremiAAkYgXwloDY1NpSLEduh+LVo6ASBZ0o" +
            "6PEYye07ZO9/RPb9/ww9fSNrdUYT6HRclbJqkpd3+aHgqqhdtl9nBWi7wQEDOrCt2Bil32k9PxxzwTLfvsaMy3YH2vWqVP2TW/6+" +
            "VJyFa9CaRtytfQ8IhzrAIVpYR5DJVo+AEXeOdA09pLMQ2Pk99OROFgAmKZTWsErieHt9XUWeiEdm72bkrQ1sRNDnQrKVsgFzAcup" +
            "7LYWbCzYGrEAsIs0pFNQuoTKVlBFDmMMjLVgtiHMKcWxCIFS0EHtfadaBQcMeE7E63I4Xvgfa6taOrL7o/o6l1+szSURWCiTap/q" +
            "8dzQikZvNsFF6CJx4xlNkUzvkNx8QHL1FjobQ2UT6GwUnVsrq8IH6kIDtfw14CAMxhQA7fJdvWlCT99EBHpqz75lzkOq1V+egAuW" +
            "oi+46ueDjkbsclkFUMs0OeC4qE7oaxH709QEDGK/QJDLaB6hdA4uQUBVqPACObnyGQCxy04KyOIDMRVIDgJrATAsJGQok3JRTbjx" +
            "kYyntlT3hA7m77VzKfUQgWrA8yCWNFhIL+KwnZ40ywFe3G/8XpIGjs6VrV3NYHPJDgxbSLZfa2vjbu0lb1jWnYWNIe++Ld31iGVN" +
            "EFje1boFriLWV96zbgIw4Aig2vZS0Sk5U79a/K7r1FlD/PUAhXSJmmnuEOxQzEkyBu+Cs+qnL5pJb+MysWNRO+PFNu55IgyU9Una" +
            "w2sbZY/P1stRdBCJG8TBtY5qkUM4Koe4qo0vGQKJ1t5aMApYtiL0+5ClTjOqHGtgAKwsrLWwbGECASh/Axsoi3LwY3KkoK7eLMlQ" +
            "HEb6wmMbDLhUsJBYEdAL906UTLbMD+KE/bCWxyf4iiwILv6/RP6xMKsn2GIONiuXF8QTjdor6SWYRs6Pcpzw5J0sQ1krGckDESiX" +
            "GTfeo4Z0RN3HDhiwCY0uUw7ivWrX13RNiv/wrw52IAJU2TQt8Ieg3ghbNEp/ycIuGK3tdNF3tAsOdIk4pM+6l4jrPww4EqoaueqT" +
            "j75xc6/fQf7jCYAiuBQA8rsbDdlFHIqVG+GrFzYYgEs+ZlBE1y7PUwSwApi1CDmWYZlhuEoEyFpYkIQlB1CJGMTxfcfWEIoG4mpe" +
            "gwEDjobQ/70A7zP6OvedWKsfCf+yteU5wY3IEwLR9Fs2YGtglk+w+QI2sgSU13R1WaPdDBY9xJYAgKwVIuAW7Xs3vIa430IAui82" +
            "YMCuaPahvt1s2sBUu/IuXbnl2E2Gh4N0rFucfJI8AUfDnvJr7DEAHLnTHChjv3S0mvCG9noGRCMGodSgh+fjBJOgsecgABBQcRMC" +
            "Awri4RMLHJ4X+GswWAwL8O5B7fWSSKEWbHLYfAFazWBmX1A8/I78/j9BLoEYaQ2lJFOqJwFlvPHSWUGlY+hsCjWaQo+uemzDAQO2" +
            "Qam5hy1gF48w8wfJyJ0vXDZf69bLREQA7v2Dfwed1cC5zomrj4F17kWr+5+Qf/4HzONH2MUjeOVdg0prXpgMKyQZwYTHLInHmAAY" +
            "t89wsNwR6q5AHusIwDDED+gRHcqmY1zGb9f37g0F7HHdY8qovS4M7kWe2/WOdzi2LvzvePr+qBGB7XlBzy47Dfp6HIaydaldjT8Q" +
            "p5Ngkx4lfPOuBm4hLvzHiuafmKHccXDneDcG735saybKCiHwJ/ndXP2ZALEAFCvQagYiBZOkWCUJ2KxQzL9ApZkQAKWhdBmXnHyc" +
            "cndhZiC5eov07nukd99DZdMWjeWAAUeCT5znXYDMCsXjJ+RffkVx/wvM/AuscYvmbRE0/uVyYVvT/nNJCJw1wLpt8fgRxcOvMA+/" +
            "gZ++gBdzIF8CtqhZ41zRtRevtEB4QuCOLfy775UAOxKA4X07Kk4i05w7NqnX1xy7qwh63r1Z7mbdPR19TcBe8tzBdpD9EJHJ/i/N" +
            "lU17u3Q2VE9SceOm6natTSduPrjrvePafo53bln2gMOxuZVrRCC8i87f2C/C9UKAFd9967T/3m3BAlCBAERaRyK/CRBC4KWOGlv2" +
            "G2ZwkYPpCZYNCohlwCy+QH39BcplRBUiUH48EfAuDQxC9uZPgC2gsinSu+8H0jngNAjEWNawsC3AxRLm6RPyj/8Di9/+juLLb2Cz" +
            "gi1W4ssfrREoc3uUJCD+CFkQMmDZwi6fYJdfYZcPsMtHcJ4DeS5uQf59Y5RMPVJ1CrkQK1ywRPhjDYOMvPvBPbBzjii/D8L/afCq" +
            "W3lrCZ4iywE3Tu1qw+M6tMUl7iuBdteqa08vJMDCCQAd2HuOPbIdhFv+jut6FDJQu2ZDG3psInBQsesPrrfnxmIrz3fQX5wPavZV" +
            "iBggJMAt4rVWhAAnQMREoNQgytgg1gJfdtQzgkWAq/3ApxgOCxYJsAwuVrDWSLKxfAYz/wJ8/RlIxpJ4SClJauQsAiURcO5ATAAT" +
            "7LdfoLIrJLffh8vRQAQGHBOVaEDeElCAixWKp89YffpPLP7xX7H6+D/A+QI2X4LzRXAJ8v8hCNx+bQDk3fBuQywRtUSIL0C2ADiX" +
            "a1m4yFtu7Q67736ii/0dxLhQcgUGWDFYASjYRfPyLkGlEFXFQACeDUNz74VtRM7jEoG+QO5f3ng/ZxcdqAvPISK2+X4dgrMVczeS" +
            "j/3LXTv2N4p2P2xroBhwPESjISUp1OgG+vob6Ld/Bi8XUMs5eLkA56tSSPGhCVk+7IiBDbIKwxYFrMklWokpZJBi7+JQ64jeEhCE" +
            "EgaTFdnDKBBWcoC1sEq5hcoKrCQRESm3cFniDIlmk0n8o/OFuGNEVxx4wIDTgaINg2EALgCbg20ONktYs3S5NEoCEC8Y9t8BBNId" +
            "RwsCTIgIRIBYykgBmgDSACWSgZsS975AfHtdVC3/zhAIqUL5ef9nJG++gZ7eQWVjkM5ctt9B6D8bnK2wUeLYvu4nBdW2e5zOte+7" +
            "XjouZxvh3+MsSUDfHWNTedteb99hbaf7aTv4FOMpt32tRXzYox6l73dVs1y5aOXaXjV70GUH7IF2wwyB9Ag0uYW6/Rb63V9lgeFq" +
            "AawW4GKJsECRJVwnsxWB31rRV7IjA5ZhFjOYxRxmOQPzErBG1h+zQeNJ19wCQ3QgMIhM6J3kcwT4cKYEEf793/DuQM4isJyB8yXY" +
            "FOFSDIB8/oJBgBlwJDC41C75RetOLc8w8rE5rM1hzcqtCyjdfEq1vbee1QfuaP0AR4uI4UiASqBUCtIZkExA6US2kSWNlEJZSQUF" +
            "QqKBVBESTUjvvkfy9lvoqzuodAxKUjnfrb2pvz/D+3RaXJJg/XxEwL0TTaP3Vmceo0d7wb2t7K7qNY/dvXZnRwKO0SF2fWB9WwAO" +
            "hm+UXivVzTtrOSvLvYf2/oqE2UYAmtU5q+fwClCTuwVJBjW5g7r5Dnr5BORz0aTnC7BZOrcAA9E8utj91sC6xEQW3oPIAA9fweoL" +
            "jLGywNCp+ckvdIwr4StC0icJFBZCsrEgLhzLNKXwD3esl6/gE8mUb7VdzsDFSlwywtse9XnmQXAZ0DsaQ11lohGBnbmQj83dmoDC" +
            "xf6PNfxcGaPro3djyxLdRyst2X6TEVQ6Aca3oPEtaHQDSjMhCVpBaS2lkhILARESpZBogtaE5PoDkjffQl/fgWJLgLPGhXoN79Cz" +
            "4NJavXXOOcH1WndsUYmu831370i7sRXWPbtNz7XNmlAu6+/G2ZGAY+DQzlUnBZf2kjXR3SJnoUVwSaouv50vF+Rj/QOgJIMa30Lf" +
            "fovEFkCxAIoFuFgAZgWCKUmANY4AGJfIi0tLQFEAagS2gFmsYFe5CxvkSARzmfkx1tAATpZxvdPa4EYBtqGveOtVOCG4WkAK8paA" +
            "PA6V2IXBOWjAEeEtAd73kYCSCJhABKwpIkubIwDBIiDwBLhES8x+iIZfJRn0aAw9vgZdvQVdvQddvYdKx0IAEi0kIBAA+WhN0Npt" +
            "J2+g7r6FmoolAEkKqNISAAwEYMD5o82NZp3ETPWToh0HegMdjNZ7WYN0DMJc/u4rRChXFBCbTCzbzq87mGfW1m+7YtaWv59w3LNI" +
            "vaHdelPUryu37kS9bbkbjt2HNQ84DUgLCUhNDiINmBVgctnaHCLEewHGuQNZn7jIugRfFjZfATyGXQLF4wK0WAoJIOMGEW8V8N2s" +
            "HEQIVFm8GxKTIR6AORqQWzpUPFITghtGK9jvH4SZAf0gniaDixqVn3IxOyDk1b1TXptfE/7L18Pr+/w7Uwo0kV4e0AkoG4FGU6jp" +
            "HfTtt1BvfoS++5OEytXOEqB0CK3rLQFKRZ/RNdT0A2h8ByQjQCUg5XNzDBhw2egUomMPhTCH7GJZeA7np83+Tn0lC2t//dfd74Ft" +
            "0cUtjnCpPcWAw2nDdm3KlQObV+1nYK54JG26tTVyVcNaXT+GI3MahnnlOeG1eSrJoMc3IhBkU4kzbp3234cbdNFK2IcSrYQUdZaB" +
            "5SIQAJV9BqkHQBkw5RCxqKQAFYE//M0hO3GoI6quO4EYxB213omo3FQIRPhe90dqtMzGthswoA2VqdhZA/yCdviP1+y7+P9VP/+a" +
            "syZHx1LLOB06OIF0AkozqPEU6uoW+u5bJO//guT9P0OProM7EAW3HirrRwTlt8kIGF2BsmsgyUCUuHqrwQIwoAecVljuUO63/rCz" +
            "R0hXQQ2c4n79jFe1gF+8O9A2TberL9XhOJXdIPKr77zJE2szWxRVjX1rCMCA8wPpDHp8C5VNoe27IJxQcFHw8O4KQCAEVqIAsS1g" +
            "Fk8onhZY3X+Gyn6WxYkmB5T4ILfpK2IhvXQQK7MTk5N8YlNt3TTb9n6XJIEbx29+ZwYLwYB2rJ/iI62ci8BDEI07SNVC2gJiBXAT" +
            "NpfvVVQgQJLV1xcdG2m9ax25xTJCAkpLQHL3LdJ3f0H27b9CT27DomCldHk33i0z3pICVOIsAD4q0OC+OaAPrBu1j3/lTRaA+vdd" +
            "+/yOThR7oD6brbcCAH25AzGqTogXPEc2NYGxg8I2OFXH5ebXExCBtd5Au976IPRfBEhp8fnFqPp769FVYUVCgeZgk4PUSMhEOgWp" +
            "FEQaDAWCAjuhaNOg5QX/qquQ2xdbDDw5iIhB6SJRulV0vxUXPIgNODPUtR9R33bWAJ/hOkS58sc54b8S8K/Ou6PXJrYEyE+xUOBD" +
            "5yoonUClE+jxDfTVW+jJnQshGkcHQsWK1tTyD+/H2eIiH83zV3qzpOfnnv3K7r7O6UmPRz/uQPV1SWeGXbxX9jb+7ykzbOZpOxy9" +
            "FRE4vKO1lrCni9BW5w54dnRp6LuPrJ/hBG+XXIjYryNw1gRuO78LTrzhiBBEzJSIoyK8OjRykYjvgeql1i6zrg5nPeoNOCo6zJbd" +
            "XWYNEQCkLylHBFwv9K+F59Tcclr9EhVXyrY6WwMyBWDEpY/YuwaUmv6KBcDta4LWfBvw7Li4ObXnHlS//13ks3jqajmH24hAy3G7" +
            "S1vPQwTOzh1ovTm13zK3O3OHKx9ABLYvbkN9NhKBI2CXYi9ucBrgsXvXrtuMPBEQ4Z98uE9/OBPYqfPrxsXy/LqU07QINIR+Asos" +
            "xN6a0AKvcd02MpUXBAcy8Hqwl89i2f9bT48sAUxiFfN5eINrXeXaLSp//3OFm9ZcA5glIpbJQSaX9Twcr7Px1oL/n703f3ZdN/I8" +
            "vwlS0tnu9jaX7Sq7Jro6Yqb//39mZiq6Jira7me7nv3ues6RROb8AIAEQYA7KVLKzw3dI3EBQayZyATg3Vcp3iL8b4etdLaXKEUt" +
            "jjkNUQp5ixThkFsh+78ZBeI1dy6q9kvaIX9zshnydM5iMirskMY5EYPjtWTdH9onuh/hNmDjisPGEmAmE+tTVlBxp+g2WwOsMF+s" +
            "FGStDFaEKtZWR/GdKsOrMJYEJ4LOn+7vJYVYCOE2cK6bXKkMlAYwrQC4y4YWd3I1NLcI+9/d6ysxYAI416P/2Vl/8nNgmVyCW/eI" +
            "bA2p10dRAFaMb+ZcLTPGsZN80f/5tSAnSutYCHPn4jzuQANHxOciJKdzw/k+zGnAGZWMvW52L57nbbj43zWkOTkhctSVY0qAs9Y5" +
            "W39nK6+j3DORnXJBlaJpBP/KQdclKFSQvJH//Aw+PyN//YLz8z9AUNWGnKqro1iKESD3nFlGEVAQi8CV4a3KY1fqqRwHUOsFitF7" +
            "VxEtN/ziPAP4rFfOOj0jO35DdnrRc2fyzKyqpaV7uzcGxx5XKXPe5HpGWY5hxfgcCjkUZSAwFHHlbJMgIk30hnAUTlcRXRcXaC8n" +
            "SIJ257htMZ070ISCv2vpjDPM72aRQbshz4i8Tre06BGPUa5CU2RwxMzV5Ae25RomOJSqIKOc8GjG6FEqiI5iSGWdrY80AIUC4EGF" +
            "9uAUICtQ5Sfkx2dkzx9x/vI3vfeBXZ3FrNRif1tjKdmRWXKvSUAqBSv9VxSB64GLoXWzRCfngNn7wm5sZy1WXNxQ3G2Efqs06A3A" +
            "7DHOzyasM/LTM84vX5Adn5GdXpFnZ73HRqE0+CKc66JDjgCPouwV8h+ZK4wcqJfTZSjKzYcL3dfutt2E53kkrBTfMWOdrFMB6JJy" +
            "vkjiOc91Zi31abrVgYBJBDZ/hL4e3LAHdLIMNcYmctqTMwbTEoYd7+wct4HPCTNdEbUFxd1djttyRxSBK8KOpLqKgDdWzyhc+EtZ" +
            "3ikfjgDkKgBU++L+LgPl7Iz89A3Zy69aCTCbHZV/zWpIKtVCP6wCQHpFI5WArAKQ5CDsAUpMW+CaLoQtUnHbyRlgvdcFnOVuK0pA" +
            "cb0VwPWkdz3in4PZCP0mHM7PxSc/PuP88hnZ8Rvy8xF5fgZnWtmwlrIy9LroUSyc6/vxO+/iruqj4CoBOYgi82MiSMleP4PzaF5n" +
            "gMCDFqKH/NDF4tUUlLW7dU3CNdSn8UrACeCdXpBjq9QzzS81nqDBKDYlqphfx7KUsLsaodofs2D/9EriKUxCOShZugIByNkIUna9" +
            "cwAVixGbOuacrolD/onK4KguSMVYRX5C/voV52//wOnTX6GSHShJoZJUr6WepIDaaaXA+GnbEVdS5XmoHWh3p5c3VSkICYRtU/Gn" +
            "Z9Yj+LlZ5vZ8LD+c6/X54QxsWGWA2bj1lCP+evTfEf6NQpGfnnH69hHZy1dkpxfk59IlyDWIldQbRLtGlv3unCgUAL00KOldgVOF" +
            "JFGgxBwjrw4JV0BPicRvUGeT59ZdyIZYAkKs2X3O1/untQSslVDsIoPPITNPcdkwc8JkcDnuszpGJ8u6S5DQi7BazSAwEzJWyDjB" +
            "mRU4Nx9WKCb2klPOyboxAKQIUAAnehWhQgQjt2KaMVM2/7kmBRMvfn3G+eNfQfv/CT7nWgFQCShJ9EZJxhpASQJAwZ0aqdI91G4P" +
            "tTtA7e6gnn6EevwR9PgDcHhTjsiKNWDTMDP4fER+egafn5G9fEH25VdkX/+B7Muv4Oyk5X19dWUVHzZLceqR/7ywACC3loAMubUK" +
            "nI84ffwZ508/4/zxF+RfvyA/PoOPJyBnILclr7qmlf/NF/4BQKUJkiSBShOkuz12b35E+uEnpB9+QvL9PyN59y9Q9x/0pn12l+DG" +
            "fQGE9bFmcdNnHeWJI3+7ssxbLOcsNN3E4AXLYZ9Htc4BcOWEALYJXqyetXogsRGMJnpOp4CaL54kadbRPgiTUTZijFKQyUHImXDO" +
            "Fc55As4TwCgDdlUfZe8y5bzYRykhwHyYAOYMBO16USgEhWuGMTuUO4hp1yEm5MdvOH/6K/Isx/nLJ7NLqhaC7K6tpJTZII2gTDAE" +
            "INnfITncIz3cI7l/BL77P7TysnsE9o/mavtIKdTbRI/m59kR+fErspfPyL78F47/9WecfvlfOP3yZ/DpxelcnPktxYR3486T23kA" +
            "xjXI/jbn8vwMfvmI/PkT8pdfwcdvWsE4n0G5Lc96Oq/eWK+MovOnEnXbF2uldq8VgPt7pG9+wu7DvyL96Q9Iv/sD1PvfQz18B6Q7" +
            "2I3KSh1Wyu66WU5InIZLxLGfK0FXOWY5B4VlnrK6fQL60Jpp3OG7/d2S3quqZux5TowMq3tA9Yu3MgYhXIJChQagZaaclbYE5Arn" +
            "XAHGEgAmECskyli8HLceZYUTpbQCkCo96pAT9CRNANZ9gvRyoewqApVyy+CjtgTgyyfgv/5khH/jFlF813+1AqA3JlNMSO/ukT48" +
            "gh8egaf3+qX2T1BPvwGZVWPI910SNkExmq+H87Ul4PgN2fOvOH36K45/+w+8/On/weuf/l/w6zcY2xbsakGc53rRW6MI5K5CkFs3" +
            "IXOsuC4H5ScgP4LyE4jP2hUIrMs07MpTCRiJsZZZFzmrZNsXQLEELjFAO6ME7O+R3r/B7s1vkH73R6Q//p9Iv/8D6P49cP8OlOx1" +
            "eS9ch6Tgbh/PMkTo2dfPFJfFcMf5488fYg24ttqxiBIwJtF8n/vOmcXdr200qK1Vyr1qf/mrfbHboFahCIAZWU8PUIdHqIf3SB6/" +
            "B/bPwOkFOD2DkCNRRtZXgFIERfo7KQJS/YNTAiEDnV+gzi+g8wvIuFtoKSxrnOejhbsj8vwTOLdLjrofKr9DC1TKCFb5wyPw9Bb0" +
            "8gbq/BX05rdQr1/A55ORzKbU0IVLoYX6M/LzK/LTM7LnTzh9/htOf/8TXv/6H8ifPxeWKyA3K4eyUXS1UpAXS+BqwV/L9UZwZ+se" +
            "x1CmzCtTzrUrmtJuOskenBygkjuwOpTOP4Uy4LggwWz8xQzKGbuHe+yfHrF7fMD+7TukH36P9P2/IP3wB6h3/wza3QG7A5Ck4sZ2" +
            "jVzYfVmzhvI0rbBUC+3iaTyOyZSApmQemwWxzrzjDa151HieuoVxCa5hIZLqWLHlqjWc24KUXm0HDJUesH/zHe5//APy0wmHt98D" +
            "5yOQHcHZSTs8FAKRVgCIzF8F7Qpk5gTg9BX511/AX/8O/vYL+PgMnI7g86tWBgBUypFjHSjqspHyC3dAKu/Qn3IugN3BWEEhoxSJ" +
            "2iNPDmC1B1MKUIJYaRa2QqCVJzKTavWysEzGFU1VrUzGBqR3vDYuPNYipaes6OtJodg6wFxVeI4xoBWA3QFqv4faH6Du30Hdfwe6" +
            "/6A/sLMDdCB2GVI2+xeQWZKUmJHe3yO9f0T68IDd4xvsv/9XJO9/C7p/B9odgGRnXN7su0rpvR4aJJYVCjPLR8kfXp4ivJXQ87Um" +
            "tQQs56HWICSG2vH5nnZx2ERu1vjNkbHkfSZ/gHBRCsGa9KRDpIUS8PDjCSrd4/7735vVVzIg18sVKkVQirSLMkpFQEvgbIR2Rv78" +
            "D5z//p84/+M/ccYZrJQW0/OzNw/IukxQ9bf9qag86ij81gJgD1thTZFCTilytUeu7pCrPUAp7N4ClQLtaOlTdjfC3HDRsBIqJiJA" +
            "UaEIwLp9wQy6EoPZnDNLWtl1+gsXNbbKgXU/0iWjCIUUaLdHcv+E5P4R6dvfIHn/e6Tv/xnpu99XFACCnV+g5xpYC4FWAnKowz2S" +
            "uwckd49aGXj6EenTD1D374D0Tk98V0lp8hKumkJnxXqy+7JtYgdFKZBQoX1pyvu4X+KGrh2aKA2dTLX3qzKhEpAXbz+X0NxqERiY" +
            "eLG0KxvqSxfWFpbSUqZ8jit1CVeL9i9OgERBEWH/5juodI/dmw/IT0d9EZfXancIrQgUQlRRTLSgw8iRffoLXvd3eEWG/PUTcs6B" +
            "LNMTNoNoAaxiIXfNaKRDN18rRZ1MI6DfRCGjpFACtCUgARcKgP9YLn2tsfJ2RKhu0GX8xIrN4VSpCICovMZ4gOlCU3UHK/bwZb3i" +
            "lbUCWD2hMAFYvUMloP0B6v4RyZv32H33T9j/9N+w/+m/4/DTf4de3y8v3JBQ7DCsJx4XH+R69ar9A9T+0fy9h9o/gPb32tXIKDYy" +
            "B+AaqeYpe333mgc2Lw5737s4ngxJ0Nj1QzqKSFh+UCoQ9CxzAvz4LCWfhr6PCQfAZmrKKNegNl+u2LWTSDQ1c0D1lLA5Kg2PEeSt" +
            "q0OyvwNIQe0OWnBx85+MBcBMzC3W57c7/9pdWJHjxBnOn95DHZ50WMkOrBIzGh8gWFbLShNvP4opxlbsQiGGkR2XdcqvjKpuHutf" +
            "byV2dpagZXO+3OBQD/O7ygM7S+VZAYHIKALWiwhUFuvCGqDLvUp2ULsDksMD0oe32L35gP27H7H/8E/6OnLmIpgdjNlVAMzEYkoP" +
            "oPQOanen/6Y7kNqBjBsQMKLPEFbLVgYaVhnP1kiFL3CPTlKlJhox6hLEIvsErF3rbI1810AuvWPamIQO3Rt7nZbXjEZjjJlL2Cil" +
            "gKMVAQWVpmYk1CoB5jq7CpA/OlkondrNgTmDUnptf2V376VE+27DG+1HmzUvXijLcuwqAtoS4SokVRu7qzKUWzjZkMQasDbcwlEr" +
            "OTrnjVBduN6YlX/KklbugG4n6pbhmFLjmg2cMl+91jggqQQq3SPZ30HZz+EO6nBv1E02FgejGNs4esqL3vhuD0p2+q9K9PK3xSpA" +
            "flpIQ3t1UNx5JXT8pksAV7+Sf7zu5dkWzDgm7ixsa+VHfdJ9AtZQgCbvYNteypqC9ZDO5RNhrCIwweN7QZHvTceE1VMxFjmyD6lE" +
            "mySV0h6ElZuscBIYTbcCD+cgVnrnXpVqwcYoAIVFwdn4wx/Vj8c0cB2hnHPDjrjHVhEwrhiO8FgtzuHK2NXoJiwEe3+NIM2uNQDl" +
            "BNxitR/YJWHzUv6uifWA9RWi4jtQKgD6u1UWiZRWblO9IV2yvy8/h3tzNYrwyjii/GvKI6kEoMRMarYKQHU/ADghCtfD0LbEk3eH" +
            "09CsropAnPzmoKIMuB3bEu8ztUWAUNMKJ3MHWlszsmR5IzimkK0rAiMf2xqFWsZQ1BtI2D5FG2bcgmA34rJKs8UfXvEbP2a9OZiZ" +
            "BFlRAJSxAsT88huJtBSeVY+db+yOCkd6AynOG6HScJcCdfm/MUST/AAAIABJREFUsfZwXlUEcjMab+epFHK4W1LKUqDnA/jT08ko" +
            "mKUCSUR6B+t0pzemKz5aESi8zpz/Q1DldPW66hwAKanXysVk7liRWrMZtGu8fCvAhhSBGOOVgF27O9Ai9MjE6PFQo1kZylyHjL8l" +
            "msdC/b/+eeEaIFdNLp2iA9nuHagNwZhRfrtSC0Kf2hO9AEvlozDgOYaHopb7Gm3x3Qp+GZjP0JuVlW5BDJhNw6h4AJWzRmvB1S0e" +
            "7nGpB/3h4k97t8TOHzvCb0fXcyC3G30Z16+8vEbfZ8zfEX8zXQap9BLzlILysFMnQLCbg4FSQJnlZ+0Ifu0dnDA7FBeZBHwrcL/W" +
            "wx/lHsqaipfvfTcQP4haUl1aERj5fpO6A03KhOFVrLxtF1J/S0/vqHb1Spgg2ImC7kenikcNgo6oWtdGVREAyuVU7AVN+a39cYj1" +
            "ZNzKcVQH7UsxqRwrrThoOK4ZdodYcmQ4WyT9ulR5RjH5MtNKAKxFIAfMEpFktQvATBR139cfZKi+q+1YtO836vcIEUxOl7450Wsq" +
            "J22CO2411tKDXO8EXCgE1g3ILgFaKAJOmDXbuz9u7yqr+gYqFAG71GwCphRMiVECqvNkhpSGugIgZepaGZSzY2WuNRanykBS5UAP" +
            "qotDRHWlucWWEWH7cX4D4Gy+T7s60BQJMJNW5Qr2nR7BQLF8g3toiohEnzc28MWDDj+s8aGuPdsZgl1lCyJMCbmCsK8ABmdbceV0" +
            "eTsVo6IhxwYigiubFSOy7DTmVr52FH9flivCdco02VFitgpApic4GwGRjMBfKBgVhceKfOZ7rMgXcXJHfkUxbqYU4K1AX7MEVDK3" +
            "buphwNnd11mDn9lYA0y47gRc1xLgB2ugSn77SmqpcVJxXhnhv7QC6EnvIfpZAvx7hGulqjBOLlYVhW2JYfABRF96mna03npsk3/D" +
            "QqsDrbbJCcXaKSPsfWJMVsm23s/7iVB7H2cEjN1jMkJ1O0RqS1CCKa8txu+JYHdqqoTSWEmpqkWwtUuUFgE7EOuK6zVrAFCOEudn" +
            "cHZEfn4Fn1+Qn74hP34tX6PQdatKL7sn66EXf8kud6oSs+NyGYbgU47ic56B8zPy/GzWzufAdXVFwJYDqwTk2Rn5yxdkL1+Rv35D" +
            "fnwBn47g7GwuBIoVegp5yH1W1QpQeZxbJKxSay1HRDrfKdGKAOyyt1SpI8NLgZSfa2f+HPaHXrYmBg8TtJruGrVE+4z4uaPMx2Ua" +
            "JSAw43gZ2jKz4fyc8V2DMD9LHCYMMDCSe/lEE9aHbsa41pxZtwy9ZCjblXqYtacOXFGPg6PArotPV0WA8wz5+RXZ6zecnlPQx5+B" +
            "+/8Ap3ucn//eOlpb7g0bUHzJKA1EUPtHJPdvkdy9hTq80ROqjXAoft0OZsK4HbnPXr8ie/6E88snZM+filF9fwnNSoflDGrqSd86" +
            "n7NvvyJ7/ojs2684/foXnD79FdnLZyA/A2aScKEIeEFVShBT8Le1TvmjTcSkP0WXbVrKNfQrwkZYUiCLKQJeYd2arhBjqnq4kvo8" +
            "1ZwAjs0JmP8dveGV6PnqoUIcmCAjZrQ6jaMtaRpvI+f/uTDPcEc4RcC5AbqYksuOhQuhyQrPZGQ5rQTkrgJQuGt4T+BaqOaAXbqx" +
            "HNR1VhitGbI4z5Cfjjgr0jvHfvoLeHdAzhmSzz9XxvNdvcPWKS7ex1EGzCRnPd9Z75icPP2A3dvfgt/mSNMDiPUkUVIKHJwgeoPo" +
            "xATyHMxncH5G9vIJx08/4/jxZxw//kVbAwqrQA6yCoDZuKsIyG4aZ4Ry5lxbAV4+I3v5ivOXf+D88W/IjRJAjgLA5EfK1SpKwZ8d" +
            "K1Tl6ooCgMI64P4LBl/DFcZ8wUxKjDAhtaabKn+ar90ADTpNqAoOrl0ddKfatRNX5UmUAJWvIZv7S90VASGgtHZ9qahBbA2KADAg" +
            "HvXxrGlhL8E8oaZuIhCukvZMZq+clMqAHbVl4wKSI8/zcsC3EkjxHwCu6Zme2AY7j7cIhkqrAXOG/HwEyOwTkP4FOWfIjp+h7p4K" +
            "hYLYfaKNk6sEWIVG6RFhu1uyUQJ23/0ByHPQ7h7JwwcggRFUk4ql4mYplCttCeDsBM5OWgn4+DNe/vY/8fy3/wBnJyA7aTceI7wT" +
            "uJzQaxSvaiNOujwdX5C9vui/z1+Rff2I/PkLkFlLAEo/IsAac2BH+Ytyx6SvMX/LuR72JahaWArBX2lrAHdRBMj7638XbosBed9l" +
            "bKYNv2yuQDLsTSzOfkfhCImTtsehwLjl/AimmRisQMgnCen68AvOypkqiqEuKXiBM4IgRgChSmhk0xQUlYCSA2h/D7p7AzoekeTQ" +
            "G4mR36wVEjwABudnINdCI+encvSfPVkwAOc58uwMRq6tAl9/Rc45stMz1P6uUBaoWG6yjAGzXfnHjgWr0rJBBKW0AkBKC/vJ4T3S" +
            "t78FZxlAGShPALWBRmRJ7CTePAPnR+SvX3H++g+8/voXvPzXf4LPJ3B2BJ+PQHbWCgAYyq7uU2xMV9cM89MRfDoiP52QH1+Rvz4j" +
            "P71ATwpHqen57TsRiBQ4MZt1GbceskI9qLB+FkqjGRhJ7p6QHh6R7B+QpHegdA9KUuh5ATGk4RRG4uuO0syE8QTw2ZJpIkHfj18O" +
            "gBNg5xybRgnwLQFDIz86RZtTrm+0ul7fGu3NVahhJXBY2xG54+aHOgVAi0nlugPlZEuV7KDunpA8/Yj0/T9DpU/g+y/IHz+DX745" +
            "I63m4yztyMdvyJ4/IX/5iOz5VD6LqqXRLc+l1ZD1cpGm0ePXV+T4CnXOQem3QpFw9Y5KGFw6g3AxwVlbApSxAihFSO9+RPb+G/LT" +
            "EVXzxuYak5lhkz56pF/P13jG+esXHD99BJ+P4OwInPWkXgIb1YtL2d91r3KSmbOz+WRamThpRcKWpdrKr4BZHpaAZA/a3QHpAZTs" +
            "oVQKlez0JnektMXHfnL9DsQ51P4Bu/c/IH36oOeEHB6g0kM5OdzGWRAa4cuNqgXqReiS9kC6PqjnbTMxqbdOyN9ozAMa7pnHEtBk" +
            "zpjbZ6wwzVYf1JZufvTcjrxTAFfHsBI3NpkqbhmIZqdwQ1hFQI/YmwnlyR7q8BbJ04/YvX5DfngPfv0Kfv0GPj5bxxtY/2/mvPib" +
            "f/0V9OlnnPMT8ufPjpJhn6cJ9WV6RD8Hcq0x5PwKZAx6PYIoKa6quAOxG4a+j50nsXlHRQRFCooI58ePyJ+/6hVpmI1lwQieUilQ" +
            "lbxzIM+0def0iuzlG85fP+P06RP4/Ko/2RGcnRzbS6kE2InWRZtvdvbl3EwoznOzV0Cm3YrgTOuuuT+wLqPJHtg/gA5PUPt7JLsD" +
            "kvQOye4AUgqKlNnpmkCs5y1QnkHt7pC+/QHp03ukD2+R7B+hdgdtDQBuPM+F7jhl+hI0jO0NurH1+uWVgbGPaBQvQzL0iMy0PUbo" +
            "3LT7BLTEoPg+l89Y5TmlJhzzkow+OjYKHXHrEeuZYUhChDo1sQIIHu6Ci0wApTuouzfaEpDl4Iev4NMLcHoGTq/QglpuRifsOu96" +
            "06fzx5/B+Qn586eizBbhwo7Yh+NRTBwF69UQshx4PRaipT+ibMOKVQt7vqIEgLB79xH58zfw6QTkxpJRWd5SAADrDgTWS4Py+RX5" +
            "i2sJMErA+QUwrl968L9wxNKKAKDzwfaW7KhpVD1WywP2vxhLwP4RuH8Hun+D5PCA9PCA9HAPlaRQKoFSCkopvdpQdgLlZ6hkh+Tp" +
            "B6hHbQlQhwfQ7g6klumqheviJlqLBY2ksYGhqcItmFL+aZHL1uUOdCGKTp/iabWxV5qQERJ52201c0v3W4UbpFDCCaR2SPYP4If3" +
            "Wsg+vQDZq/H9PhXCv93cq9j9lXOAEmTfPuH88Wdo/+zAjpDmWcUy8FR+r7gGmQ3ICo8jj2IT2sDr+JOemYyzCpH2cWdnGcpghRBt" +
            "GYW7l93AzSoEetSec/vJtLUAxv+eyruLpWHZ/mVnyViqTBRvTm1jsSIFdXiEevoeybvfIn36gN39I9K7J+zuHqGSBEol2iJglADK" +
            "zcRllULdvYO6e4vk/p1WBg6PoHQnVgChB+sT/6e3AmyXYH+Ay7Tm07sDbbmdiuQCeX9DtwmGiiQ1XZDCjVIbxTBLaSYp1OEBCTNI" +
            "7fQqMPkJyLTA1+QOxFmG88efofb3lb0BLOXofPl8RvWiQn+156lHOQ1YsBkA6zVCARBYme/GZx2oyoBUuXNJ1lAb/Xd28zoHkOuR" +
            "fnftTXuN15hXLAFkLAFUbmYYsyLHY0SASqEOT9i9+RG7H/6I3dsfsXt4g93DE3b3b0BJohUFMyfAKi7EuZ4vsHsA7e+hdg9Qd096" +
            "1al0PyShhBtkbfJIt/hMGGsnqNAguG+tnZs+jyn6oplkqFBcprUxrqF/GEnV17Nb9ok7kEevctA2tibcPOR8sW4aKkWyf9AKwP5B" +
            "j/66m0K5rjP2t91L4PUZp4f/D2qnlQArwZftLhf/kxHwYb/D0XOtdcBRBNzo+taFuEuQWQKSdBhshX+jCBCVgTpfL0SX8fD5Y1B+" +
            "s5+8+NgVgAolzvlYnapYFAilclVVBKp9QVfBQZcHBXX3hPTtTzh8/0fsv/sddo9vtSLw+BZKJSZ8ZcqOjj+ZwkRqByQpSO306kC7" +
            "gygBQmfI+f/SsPN/OE4zSk5u0E2DNAtFoc89UcMvYieHM4kSkANQTSuYrRy3g+ZC+K+NDRbX+ogCMIZ46q2jGRPWgpbXTReXpCCV" +
            "QO3ui/OVkuT62jgrA4EZ+bdfkTy+h9rfwW53XhnJZyok/uK4851dodDqJYFOxhf8C3mP/KvsV/I+5elyf22pFXWFSgv9zHlhAdKj" +
            "/o6SQI41x1Oq3Lyr/PatAS3aXDEBnBKo/SN2T99j/93vcfjhj9g9vcP+6R12j+9AprN0d34mP1AuzxTlQRB6cvlSw973y8SoVm0X" +
            "ENrGPKKWUgOT0X9vuwe5yzSbhfW94XJlIYjfUVe/tCOWgIkphvAEoQE7FG9/mr9sz7kwKq4gBGeUvXqZ1Ta0EOm4AhWWgOpj46bW" +
            "gNdKt7bCC7xQCNyRb5Smh/DTEa9DgWtrlzbVv6iBfQFshrDzm2FXeHK3kvNvK+R/9pqYpgzhDtfY82yyKGezopBesQh8gp6vwMWu" +
            "0JbQt9rPhlOC0EQxeExd2p0lcIW/KWPUHG4x+BJoGoBqu9zURpP3dxFCkZlQhr7ckgPrKJE1KtFqMiHdFNPZoeohdAhTRsEEQ62B" +
            "9iVyhORZfaQYsTfCv12n3d6hT5c+P27HYfSCuksQ6rWjIqPCPei8g68QFDf7rXtgFLh4YBfJtCOtGowTn5abo09tqMbDajg7/1sF" +
            "wD3tD595z2xs1urlyj0VsgoQw3r2AHlu5qfouSrEmTlhru3VppUdkbSEQhuVWuFZvXgVBWiE8Nd4a3O4seocolyU+oJ0ad4niOJo" +
            "JUBl4AuqEtPj97c9zC5AJN/mUHwvwvBSF9euWxJFFADBIybkx2C7O6y527UCkCJXNtMbeHmSfuH7j2YFoCjJDQqA/R2sD6HqRah0" +
            "5OVQVmhoL2jTbMARFDrfN8IK0NB89GtZTBpw+b2wAJCvDFS7cjL56WZcXZdyx/u49srkXwIAdj+3nM0nK5b+1NaADITcUTzN7cV3" +
            "5yEyEiWMwLZzobZk06JIF4tdh8NNwxuuAtWmCKyiWk6gCFyT+O7RnjLx0byokXZcNJrsTJM8bBtQw6/q4RtJEGFWyNjCuRiCNzu1" +
            "goy8b8pZw1BRZPA3eM5XCBoVBHO9vtG5m3PzV39nNhNHK+4ubv3o0637I+a1iPQMq6ciMCokM7fDpo27oRebTb2KvQO4TCrvQcVk" +
            "YOehpZLonHNyr9NbFopArpWA/KitAWx3Gi6pWgMo+LXloCA0Mk2paRNc1ktfkavLm66uJo5UBKbdJ6DT5IAZC1ORg91GT7qbre1W" +
            "RT2iMPiCGVhdqRWEy8JcymoVWdEoBczFHsUAB2q/I0QysbPGfFgBKEbgfBcS5zzZmxgo17o/grMjQARWCpwrx/JBwardrXkJjHEx" +
            "4laV7o3lgMv8MbjAw92B8spyr0fk9nPWuwLr/QH0NdWdllGdXgHHKuB0GfXlQfVF5PUr7jhFntvYmzxhowRkRyA7gvOzUew6JUgg" +
            "fQShBy0iUH8xxG/J2plUzOkZ2Dgxq6r4VweIw1dfnEonE/kbYdp9AoJlhCPfZ6LWiXUrDuGB5+q9XTObgGY3oqVKzSpKZxtc+0WB" +
            "74IwFl3H7ag/AKZCCdCHrPBvfhcjwX5A5Iw3FCaGaofB1c6j0pFQVWHQCkB1tFmPamuf8jx7hSIFVglYpSBWRWjdWraoaN3h3s4X" +
            "NxOszPW0tYpQOWjOzh+jKJilYLUScAKfj8UnN0qAdsfhcrdlq+Y5GaHT3bP6smsRKK+vjNpTeY29XREjBxUaJdlJwdkrkB+1QsCh" +
            "zXSkhRMugy8zdr+jme7tSuDKCUXEWstXsfjVYxM7OqqGuhGYu6q3pV1EoJp+x+Co5LbE8HcsezsoAlNmEA2pYBOzhb4lJmQJwizY" +
            "kX3HY8SzBLAV5KA9Qu1dVlwsRvIJsEtuWFE8pMCGFIHiN6Huj15oBznA50LIhUqAPAE4BRsloG4HoFpQsV/ttc67YoqGLGZOCVoC" +
            "yvV+HBONsQBkRgnIjBXgVFgD+GwsAVlemHnIyWCbbeXcAC4EfHJjxNV4VVeNru7dYBt8Inutyb+stAQUSkBIKhGEGRk3Kt6PXgML" +
            "S8bMbWqoWtenCNYJuuQSwl/MCtAQl3nmBIRMEwvTpXgNLYKNxXjj7fqSDYYgLIZTsGl3h+TpB6Tf/xGH3/0PPaIMp9xzKYLaW8sg" +
            "7L8cjBz56zfw8Rvy12/aHYUB5FyoECGrlmstcM9Z1yIwkB9fcP7ydxz//r+gHt5CpXdQuzuo9A6U7CrxijU6QZmbc+/qkKXD+dJn" +
            "SY3Gh8ce4j/fSXl2erBCETBuQEaDy89H8OkrcvM5/tefcP70C/jlixbCHYuAFfztEqFWIbCxUEmqP2oHlaRAegCZD5IUdi4JFFUs" +
            "x0QKecbgMyM/A2p3j/1v/hWH73+P9M33SO7fINnf63yThQ6ERSDn/2K8InrlZfp8bvy5RrZWe/0kzRngDNg5x6baLIxsJ3Zp4d8l" +
            "WrhbSj05n9I83HBx6Hu3R80DNf4cE9Q0rKBsbJMpE25rzdkEmOF3df8W+x/+CD4foQ6PWrA0l/jj0e6tAMCcgfMTmM/g7IjTrz/j" +
            "/PF/4/TxZ+DrR+S53rWWMz0izP798JtIdrprK+wSzs+f8frLn8CU4PztMyjZQSU7ULIDqUCzTZVQGpLAtwhQ3XhbyP7dzfXc4ZpQ" +
            "bMLXOvYANx6eIgBmnRfnV/D5Ffn5BaePv+D4y8/IPv8Kyk5aCTBr9BeaniP4u5szJ7sddncPSO8ekdw/Qj18D3r4DvTwPejwqBUA" +
            "IkApowQ4ZqGcYQwUQLLH7sNPSD/8qP+++R7p/Xuo/X1pdRBlQFgCZ3i6qW3wrZZ9CYcbOtqlAZmHqUb/bVhR1ibftMRnqs3CCq9L" +
            "4LLiRcjsHvre6XrusVJsw4WLpcdED5LuaW3M0arURL8rx/aEDHX3Brvv/wh1eMTuu39Bddw/gpVD8yM4ewVnr8hP3/D6v/9vvCQK" +
            "+fEr8tcvUGc92mJ9JF1XIScWhddPeU5PRLbWhuz5C46//AnZ81ccf/mz3mWWEiiVAOSswNAlC6l+WfNt7tlA2vgDeJXdmQPBhRrY" +
            "mvmUi781/cPdsMFRBADoydN5pifeZmdkL99w/vYZ+bcvoPMJdpUlNm150AJgPrvdDvvHJ+zfvMf+7fdQ7/8F6sMfoN7/QSsDRIBK" +
            "TF5QEWWyUTSuZaQSqLtHqPsnrUwc7qF296DdXWlNEIRRsPe9KunESlibIjA2JrEjfU5PzRLj0kXaXaxbbX5ofaf6ktFKwBlAgmoi" +
            "cKDTmRtfk/X7HWu+ddeIjhYKs4ygc6ASlvvX6Z4uy1b6Ffb+Xj7lbphbUwQAgJDcPWkF4Pt/scu6FBRtiFv/XRn3/IL8/A18+ob8" +
            "9RNUkiA/fsH5459x/pxos2jOTrugh9X9jihsGSgnxGbPn3F++QL8/c8AyBFanXiRF17bQIQj+EauCOApAX51ZcTP++cA1NyL2qp/" +
            "4Hn+d3ZG+fV3BudmLgC8Ul5NPigGlNKWgHS3x/7hEffvP+Dw/T8h+c2/Ifnp/0Lym/8BevMbkEqMElD2eJVw2TlirQbme7my0K3V" +
            "N2Fe4hXIllBt7au0SJfrda+ou/clxPXU7H4xmWpOQPWpF1IE6lnRLWtqlaL3yJnQGV+LclL2itqHCZFUmRxSRj5L9AiGeyp0fSUL" +
            "ziBOgFyBVCnrEew8AC4D6pl1Vka0LjDErJe6jATlrkBWKi/hsN32OBytuBIQ+xn0GGo73xB047mmcAsFoEO47j0EcJGBVLT7xAzF" +
            "ORT0pGFSCpQk2g3LKgFmZnA91aSnENbNvD3KRvqrS1TTnprCUgPM060OFNgj4DLaUUgRaIGcxA5EuO0dNlLs148kpIMkxtpg41YC" +
            "ru4GS3kGqmzmZa839GzJrbWSzXe7W3GnODouKmWAne7sd7bh8j4lt0+4rZdWvYqKvyErgL7f2IjNZF+b8JTnoOwMZJlZ6jMD5TlA" +
            "ebk8K8OsBiRCv7AdRAEIsKRpxDcFd7w8enKC5mf2HYPXoQh0w/UTDZ4fHB8hjlPMJYENG21MN06o+NVF+tysOHNy1n+3yz96ioDv" +
            "o9iG58NI/r1DikXDc3lMuLEwx1w7UAHwb+uiABU9BBGYyg3YyCh5lJ9BuVldyF1L1lhoyjkBoggIt85G+6uBwv8ktX1ss+FaPUdG" +
            "qNMev72pjbhcgvanDk07cRUaSWWkMux7LQhrQJdO1xeFoWeAZkb4PxvhPwcVfil9wzffrTuK/TiDEr3aGBsFz/+9csmGGy23/6tY" +
            "W8zfzslPVLoD2Qm/QLnOv1Xsiv0GnJ3lpLES1owz/0RKbHe6ehJelAZXySFMtWPwJEybuC29HKE2R82NR5u7ak+HI8FD0kxYG6FB" +
            "lWAHakf7iwmpbJaiJ7DZTYxrEioX33yaLBAh1yB7U3Qgq+98rK4jYhNLE6G5WEPDLt637f6KdkUglYCSFJSmQLIH0j043YPTHaBS" +
            "JwNKBUCXk9rkJvRMdUGYB68ezentsuV+vGjOmsyKHe63X3p6+kzKGIPAbBOD+06AmI6eSeENRIdMyW2Wl8kr2a30JQSIO5CwJrrX" +
            "Yy3l26nARhco/torXItXvXjXn2ZH6Cs7CfuKQP1U+dt7SJtrY20kPZYAM/X2YxSBNotseFm8UgGAUqUSkOxA6R6U7oDdTisDiV6O" +
            "VXv9cPEXCPUF0ngJ62ROQX1q2cdv2+aC/YchPijcGo7fQM+V4hz8Wmu6Q09vat6nGcPPh7/1ahSAQI/CzvFR8exjz7+lvsT3Wdjy" +
            "sIJwWzi9iK8AFKecRiUksJqFI4t16n1XILfZsL/jYXVXABqvaTJLzAT5n3Yjbudj5MWdzANIAaRIr/qT7KCSPSjVlgBYS0CSgJUx" +
            "v1RcgTQczRFBuCQh9XT9ZfQS3X8xYNPx4XVB2wqIM6dvRAEI/e7LVO5Aqx4U6ZNIna711KrGe3xrcdPFK0u32bm19xU2SW1lbSKA" +
            "EoBSUHoHdXhC8vgByfMX0DmHyjLk53IiaSkustccuONI2r1IKxKsN8DK9EZYlGUVU7M7Cl0byXei2JU53QWG0qWpLF2mjEpFBFJm" +
            "VD/ZgZK9oz3pkX+tAOiPSndI9geku73+++YtkrfvoN6+Az19B9x/AO0fQcnO5Hef0RxBuCwTe+9tl+Xk85XQzxVn9tWBliX+8m6F" +
            "qLjrYkQmtg0ELeI7tDGayqb0r8LaoQSgHZDsQek91MN3SN//Hvvnb8DuEXmWgc9n5Ge7YhDDbiVrJw4TGWXAnWTKDOYczBk4z5Ed" +
            "X5A/PyN7+YY8e9aPJhQ735q7Cvk2GNXoO9SboKaqxz1ddDpd2nRRlwDIjPIXCpnSk3sPT0gf3yN9eI/k8Z1e25+UdvlR5hql5wKo" +
            "NNWKQLqH2u2we3jC7vEJ6cMTkse3UE8/QT3+CByegHRn9glwbTaCcCnc4YAuY8Prdwpam1t1tI3k0I/y6slTunOD2vDCkVP/jumU" +
            "gHA0LyLwVqdn9BX2B5ebPjfeav/RVh5uWTnqw1KOk4KD9SVXRhjcA+kDkocPSN/9DvssB92/B2cn5Gf9sctLMuu/dkMxorxsnczo" +
            "P1iP/Of5GZyfob5+xln9ijzLgNcXFN6njntiZTQc/pf4K/iXtVW7QtHwfWjbH1OnzXrqGlzaLrG+Q8rkSZJCPb5F+t1vcfjud9h/" +
            "+J3x9d+V1gGVmI2/EqgkBSVp8Te9u0dyuDd/H0CHN6DDW9D+yVgVlFY4auZdQbgk9TIYrV+zyWQ12+SoUGahY+Ch+QLB+7n2JXLj" +
            "QHoH1T4IbvkM4N58n2disHvkYiPf3U0ibhR7R5VkcH880pEOpp/lTxgLKb1qTHIA0gzJw3fYZQykB6g3P4DPR+TnI/h8BOdncJ5p" +
            "AT/PoJAbBYBBxjqgOxwGc4Y8O4KzEzg74ZTukWcZ6PkbGFQqDah2t5Wsp4ai4JzoZQXwL3S0j97tnifg1+7tYW0ofxg3H7O6j3p4" +
            "i92Hf8L+t/8N9//0b1C7O1B6gEoPUOneTAIuhX+oBERGKUj3ULu9vi7dA0lqFL7UWBvM0/v4WgnCBanINYvIZCuQhkJR6FNlI9G3" +
            "24NUrnEGNSZtFaZMwpYsmd4daEXtY9d07DOwKjLXBMSs6ZKw7cQsvZJ2s+EuzUmkwCrVI8PMUId3SJi0AHr/DnzWQnyenYD8DGa9" +
            "uRhzDoL5kHENYrO2EAOcn5GfX8DnV+TZKzhPkT2/QqUfYTO3zOLq3ALrFdP5fdDSNjoXTNqlO4ENGWyJ3UTWFSi7pOQgAAAgAElE" +
            "QVRJkezukNy/we7pA3bvf4JK70C7A1R6p4V7lRpLgHEPIgWiBFAEUrvCKkBJakb+7UZiIvwLgs+1DX62vg9r98jZVZ3RD+guFMzr" +
            "DtThLRbTG/uYgjocqwd+bdVhKcZIBtdGQwK0pc2svm5CoQiYyafmKNQdI1V6gnByftUTevMMnBkFwMwLYOsORGZeAEoFAGCtOJye" +
            "kZ++gc/P4Ncc54+foNK/Qq8iFHcHas3WwAV9FIGxBK0JQ54ZOE8gEJVLfardHunhDsndE9KHt1oJSO/M3z1IqVJpsAI+6UnFUEn5" +
            "6aAAkCgFwg1xrd1zyAsk5g206HjboDbYjWE3uWo+d6AeflLTeJO1BI7xmddUAMpuuv4Ws77fVqn401l/6wvFZRWMUAD6POKm03gc" +
            "WubTlgA7N0CpVE8QPrzRwr8V/HMr5Guhn8FFC1Hpdmw7nR2RH78gP31BfvyK7Nszkvu/gdI7M/RExoBWFobKvIAmVyCPYh4Btbjk" +
            "TKAIBG9vUwScG2vvFPDdJEVQhSVgj2R/j/TuEenDO6MAHEDpHSjZF4I/kSPYm7/Vc6o4V9nhQQR/4QbZhuzSOouoE6t610kGY7ix" +
            "75/JEjAs1pOPp5P3F/V0qHSkgfN9H1dbTrB2XgDgJLRYAm74xbcHmZFk+zO9q55vbQnDec3nV+Svn5AdPyF//Yzzx1+R3P2nHsG2" +
            "/6hqDbCh9W2zCCg3JfNj5B8Y0WiNsjQQamv8hy+zlgAFlSRIUq0EJHePSO/faOHffpK9vsf69rfHEs4NgrARKPB9eEXeRu/UVEen" +
            "kbxcWXHRNJlZcJxqs7CSwGDXpZpQ8uOCMgPdjPRtFsNcgoTukPMRFqVx6FcYTWORjigAbG0FxnLAZStkB6wrg9aOh4q7qVjn2kRh" +
            "MSF24FK1tLVkOi9NsGlh3a260nyluPwI26Mq0RQeCiGB51roW00jgzVDdg1eE939b0rm2SzMicXFm1AOfu1z2/AHBrhda4BXEmrD" +
            "j/VLhJmomb3ET2h6utVyNnMCqiMTXOmNyEj6etS7bEGKkSnf0tkywu5mf+h76CRxw3n/nfzr2uLUFle/H/GeXVGGyCvWtZiFynn4" +
            "ZUQBEK6ORYSPCzlANzZoATylaOsKQIy215ptYvBams94h9DOZE5NsZG1kILS5hPb9ugAvd5j9ooQEvhv3R3oQlRkIlEEpqOfAsCF" +
            "DmD+md5ITySGGfUvZx3Z+QHRetOQjRX3RyMwc6ytoWrH2Oo6FIhK07WdMfGoKQKeScO3ihAY5OzaXNyG0LtK2Reui8t3pxcY8hz6" +
            "yMsn1qT0eZ1p3IEmYo35MDpOba5q7lfPRj+FAtCbpfpCrn0RGfTirLEG3giFJuC4AzkuQIXMS3VB1nUXasO9B4Hvseu7XDsrEZ8l" +
            "IqpYAkJ+Ua1RlnZHuEpYWvQ2pkigFSdyKGqZ93u0JSBF2Frbl4unY2QgtCac24sX6A2nGptdnQtSbTiRAsdvhbW9tFgEpqSp7tll" +
            "R4lgFgBiI9A6Pu1O9XDbIhuu66bTpUlyLQEV67nzw6+exMblqIMfkbunQittDZOrqFSKpVEACq3I+VS0API+zc8QBGGLxCvxLLKP" +
            "0xb5PvgXa046PDyWDpO4AynT7gcHazrkwKxiUNT+G4kI1Q8Fw2GnFwy9OFXubo9f11MDS1kR7WG3z4PrA31zrPmdRREYhSeNl7/K" +
            "PPcF5bqrDVfFV8cqYO8noNi4xk4raI0aqnMJ7OIJ1j3IjW+liWtSBPxn9GlsuhazIm6egE9AxRTgJ1jTw6SIC1fO9Rfx7m/Y9cqY" +
            "SGKXVG5q3i6uCPQ/Nd2OwdEXb1EE1qAAVKLoD5VFL/RvCDx3aj10gtK1OquATb91RUoQRWAGytpXsQJUPOLYfOAItWates8i4PrJ" +
            "M3XLrZolwPwIdWyVuIXMB0vgWgM8BUAnTakAcMUSELEA9BkUEoStEhipvj5mqsSxRHMtAFw9vCp6RmgSJSAHypWzr6yRHfsaV5IM" +
            "83Kt0/I3jygCi9PDwT8qjzdUJ1+Wrwj/AWWAvHPs/AVQuiM1VWH/XB8LgX1OME3qAn+pCIQCi/0WhCvD1psR914TsXHdPhTulw0D" +
            "LltsWSazBJQdS7UErSlRYgP0UZ/dgc8YWod8a3v9wDSsxxqwjljcNK3lSxSBafFrn/G18X3aSRXfra8lsZcPfQTvQAz85sWLlQ7G" +
            "E/pLFz5UBI2KJbVrXDoqA1SxFFYFfmLSvTKb45U0in0XhNugtYp5ctC115LVSBxzJnRPAW+iOQE5ayN2/clrKVh949Dreu8lx7xv" +
            "7d6ZEi8U7LIVxJcqVlM9b5PGirqWWnwthMbb7V+tDNipwcyRS/3jPly/xY9BSBGoBek/x1RVf98A8q/3fjdZLKrxay5n1i/X3s9s" +
            "D7gKQMgaIAg3iO1aWzr8WDOzeoKNVvvlUzyy9aJQnFYk/FtGKwFnAEnFH2h9uBaA0CoYsVGwRjZXW9pZ3kIggv+qEEVgWXynezLH" +
            "oK0AviLgtl1tSkDIRSfm6tPY/vnuPo7OHmwvIkpA7NrOCoBjEaAibUJCv7j+CIKlopy7VeEaut4LVe1gT+gacam8rnbNXIwIe94d" +
            "gw1rFR9sxxSsD0MjvMYXXTViBdgOa63JVwABgAIoASgFkh1odwfsH0B3TyDKATCIdH0hZpMbXN4Ob2zCcaEpxGRvyN6teSHFwW5b" +
            "YM9xubNZXQ/x5/bUftbdmaJKgDeYT1S+h7tuEiU7JIc7JId7JIc7pLs9VLIDqQTu5gFSaoWro8PoZc2qt/xIX5ULP99v73jC+CzV" +
            "xkydhJPNCRjKJctE63NjuerfGBuEEhzcFihghxTWQauFSxSBeSBtTiUFqBSUHIDdPejuEXT/xpkyUNgHYKTyahC1/CFXZPa0g/Je" +
            "BtWFdiv0Q//l4pFslACuuCtxa72mymEdlcDIvV231JwiawWwOyU7bj+kEiT7OySHA5L9Acn+Dmq3g0oSs5GYtbCg63xrQdgOrjmv" +
            "C2voby+tiHiMjcolmpWQG2eYcpCIIwOu0ygBOXjM3sM93brWiXQwPQnl9qZLwHUhrkEXQGkrAKVAsgft7kCHJ+DhrRFiyVklJzci" +
            "tasEsNOW6m8EKoTocnCdPWXAfq26H1mBvxD2mUtlIDddirFIlKaCarjV0X9XCfBH5/3fXArvAOwyqcVV5tVVkiDZ7ZHs90j3e6T7" +
            "A5J0B0oSlKqPlFXhitli8b60ImAbww2LHP2y3e0bqiziDtSVKfOkS1ghn9jaaNEWK9gIlq0XG66BN48oAtNCIFJgSkBqB0rvQYc3" +
            "UA/vkbz5oVAAiJRpoxhAjoqDPhxLgOP7Q0RQNSWgwXXHfC8sAVYZMB8YRcC3FBQ3BYO1T3ZG8f33r/zk4m9pBTBKkH1lZpBSSHc7" +
            "JOaT3j8hOdwblyBV7CPghi4WAeFWWHVRX1DYmOwxkQRdazqHkjiB7jksF3cH6kOvMtMzV9rCHp7J21I354ppQMZofpjImJenNQ8k" +
            "k6ZAy+sKSFIo3oMBpI8fcPj+nwHOkdw9GgsAFUJt6VbHVScbqoUcsATAud+7yTUOOKP/+nepEADsKQHuzeyEU/r0sPvbj2MwUdjo" +
            "M6UyA5DRD/QzSCkkSQKVJkiSFOm73yB9/zskDx9AyR5IdoBSnuQv5Va4DVYveQwVj0ZW38ZHtsSp66P9jSBH0RSnkWmxCnegPhBK" +
            "l1A/XXxzR99Bfap9aTjvZ4jTr9Tj5MWsmzPXdulb8EMawrWmTcFGnOBaGx4RqIai2zJHwFUpkDCIFNLH77QCsLvD/u0PxWh4OcoP" +
            "VIRuRHLBWARK56D6ff5X95iWt43w77j7FMdRHILbvjWX6qoqUnPXKUb/yyuK+QC28bXeQkQgpaCUAiUKyf07qKfvoR7fg9IDoJJS" +
            "Cag85mYamgUZ2pZJHswCO581J3EfaXnEe/QqnQ3P6ZOc/kpBo3r9CfPwDfTKnsBC7kCTlj/qNmrvn6fIueJ3QyRb4+/4FNXmufkh" +
            "+ftNr7lyDqFldL9y2h+cW7k8PA/bshQVVMruNRbkZSgHFsyEYFIA77B7IiT7O+zffI/89IJqC+YTswS4h70TXTvdws2nEP2rz6zp" +
            "Ea0agLkqHNEyPQLDO1ahceJeuPuQUQjSA2h3D9o/AOneUZzs5msNozfCCMa0YZIHW2O6IazSUtjaJg0oIpGxjcXpOyDtMmd8/w3T" +
            "uQM1xvMyVZwC36q/Xe2sa0IXg1MxvJe9qeZtkAVggwKwEOCmSvr0GH9/PYqgQOoBKj0A/AZg7cHZMN7fj85Vzhfo227krjqAFyJV" +
            "XZosUdNGWIXQ8r5WpkCptgLYkAt3oI0q3YIwhpma50lr06T+M32fjdUasuZusTY1JyBEOIH65Uoogcl+/MEvrl9XM673qXBrlJ3m" +
            "KHWhAbji782aAbDp966V3TUW5q3hCKvFDowEf5Wd0FdNrDULXdKz7HW8LyjQB4OrrwhEpgg1t+nh8JkAgllm1VoAzP++AbbiVSWs" +
            "AGk7BMRljwmLRtkyjAyk++HV8u+YTglY/N3dslItN75/qTkbiGHMbSj4Mi0W91q57eNy2nTtpVxXZ1I/66+xYSF4FFfy3rXyKZ35" +
            "KMj+Z3YNJgJYuSfrqds3uWcpetw72EqRiZ1reF7oJoaxqFQmqZXKhqvHMCArBa0KmaexRaZzDfICXIhinYUhMs/YuHYSJqfnM4B7" +
            "831zE4ObCSkAcHK5Sps7EHmfxfGtEBtWBIqg3EH/K5GDBR9RBIZRHeHnQMtDlcsGpvFMWaODHVmp61MNmi8MREJbGMKttt+kMYsi" +
            "IFwvSxbtrTnaBVuIDi9R8SyMhIv2YOKZs0BC7k7l0xdxB5qsIDYE1DvdWm5o6xjK21sudOWhDi61tyA7dfJgEDRbSRtxDZoYKl1k" +
            "7VhG7ZIrS1/vXYcX/StLl5tD2o6pWHf3sd08DsV80reZSBEIBZFB7xVgmWb8fqLNwhqZ8wlN/qHmRDg/OkZqKpf3LbvOx3ystvo+" +
            "S7C1tKmVz629wHrZjgIwIl6+IXfQc9x5Bu2t9mqTcTPM1SltubPbHhT5HoMjn2FP3AZ+jAd7iXS9aeIkiuXRrKsDdX6HzqPuM0DB" +
            "r5ED1ZPxeQkzs6WBkljCVCYZSmNfY8tJUimfWyqs64M2KaX2mQjVfGsnRSBWV8ieDLsFCWNZopGS9mMplkvlG87Pvq8+o0XAMpsn" +
            "/1TZ3OJ+NU3g3rH2Z4WvWLRor1FIbPJxq8GNP2+aq0uLq3shYTQTtpattnkpf4KwDprq/XbraafWbGiTN7NgOZUSMNzNe9ZJav2v" +
            "6SXH9niWpkchX0t9GJM/TQncqcSsJRGE8UheCgvRz6QrDELq8zZY20oca4jDQKh9UrB3+SaYSAmoLmFnGTI7uq9vWnPwVW+1IZaY" +
            "LnQr1h0Lfx/vmPGOed3ok3BdKkkf5eBSm4cIMyB5KbgM8qiNB+X+rZ2IXbyVrloQOjJQDmi/bar2u0VwuXA3MUmLYJqWYruSkYGO" +
            "n0ZgQ6iL/DPMCag6MQU9+jrMAZgKv2/wXazaXK7cxG+yEixTeToyhxtlKLxGX9zIcfZv7JA2so7fFSE+voLPxIrAks8UhBVDVr5u" +
            "Ke5+j1y/nCtnyTs6GdXHXAyK/gj8do8HpPURM6SCjwhdG8+Pdul0kdX91zb+5+fVkIHu7qzt7SdkdCWtmimL4Mg9J1wfkreCIAjz" +
            "wfXF9wY3u9JeuwQHtTc8pjDbnIDo2RUlVtRTjqrXtI1br7KKrDJSFr9AUPXwquMudKI1DyWTBWGbSN1dNc7oJg0d7WyEO3ocjSgn" +
            "l5QT+8irK5JnhzKtO9A0s2cno4xUuDDWznQwmY19Dd8Vqe/5QQ+bMe2nltuvoE4Jltbyt0ABFQRhQkQB2ASmSZ0vt9ra7oFPXktX" +
            "0BCPa3NoXcQd6HKUKnDTCD45l8ZG/dvmDQzBD3O2CrtAuz24UhQjFfUQrqmi3TQiNwjCFSAVWfDZjG9EK73ljSsRUObZMXjAxIip" +
            "GVoMryRfN4Kb2ttsOIQpkLwXhHUjdXSLLC/PrLycRBJkco+pFUOevD67JWAriTomnnMV+5VXJ83UGewsCSqrg14RrU6kktmCsE6k" +
            "bm6RoQueVO/rkvdr2ocgQNPqPhcWUDuv5tri3jVmdfhp5gTk4eeT53Tvp/faisxQ77b2RZiunKkmBjBfnb+d4NGYwZL7grAubrpn" +
            "uyniy4J2YaJy0metzL6ElIELFm+2cSh+THNtX6Z3B7ILvXj2lVCebaW7v1Q8t5I+Q+iz7YBwZYhFQBA2gNTF7VITyXpygbxfUucY" +
            "IVxNJpcNtUi0TFruyyITg5cTZucruFua17AFGBSdEFy8+TVrQUID11zyBWELSB3cNl7+SV86GbPUjAtWt9HuQCkAyqvu232Vmmnf" +
            "f7hLQeOddFlFYAvLhfaC/B8NEVtLnJfiFvzLOi8f6nJrBUEQlmBhVw4XqdLCXEjZQn2Ny3olncQSkKtyTHeoq9Y8+dW9VWq8cgWF" +
            "aTKZcMTyvf6nfwjuXzdU+5Xqga8g7S/CfJViXfQqj9euGQnC0ly4TkmVXpaJ+pXFuqchD7qU7/kYAWn2+MUr2iRKgMrBFF6KvxfT" +
            "pcPwqLQpA3QNglnDS/apP7XjndOGwps0uOevIZ2nQNLBQ6QGQVgdY6qlVOnNMmv3tNLBwGBUVhS/vlz3ZmGDJ5rE15S6ZFt3ibay" +
            "l2tXJzdE19/fqMzm+4brkTAGEQIEYbvMXn+lgRjFyI61aYXNWWnbrbXDyqRzxPXaSuM0S4QqELjuytyYAUv5PTN3f05tUsC0kZwi" +
            "tNj9vQt7g0+2jedQ167gvcQgtuYi/y3GrmSwRWZa72ur9CrYa5vgIgg3wsWaK6nzQyDXdWGmFXF692RTrgLUI6xVl5zZ65Xrjp1X" +
            "zkxjCcjB7iNaXaJC7uGTw40/+9xaO7zq0tSTyLuOecWKFa/JharmClQtOdeUzHFu4y0HI8uJCsI6WEV1W0UkVgn5fenC88o6PUqy" +
            "b2Hac+Xy7kAzFdI+wXZRNv1r2HFrb7p/qTq4iro1xG5ItS89bhYEQRCGsYpeQ5gIbvWfuV0uI13En7Km3Jl+s7DQ6000wWNMRhJQ" +
            "kebZ/NclM6LPibjS3DRtCWBWAbL/9LGbTzVhMOFlzwRBaGJAnVlFM72KSGyTLqOW18BF36/dzYWbT89LYOrlNHMC8py138fAGRqx" +
            "cxF9IjiHt0X3qAXrVoheI9aRqQMdnnuNBJM/Nn0+UERKRaDtCYLQxPD9QQThthghJYWq2CJCl9TtSbh2BcDF7RIm7B7ag2lQAJpc" +
            "oy+UN9O5A82xnFMknL5LNNVO1Xx7OsZhYR+7LdHFAFD5RoGvkrbCKG6phxOEIcxQR2Zvt6Vj6I+kGYBZivvQINfaO11+TsAGqGVe" +
            "s/OT0ELUitKU0FfNrbynIAiXY8beaqqVJISJ6DPSKczNpKk/cVbOMCdgYtpCZufTgabLYi5zQ15OqlwMf/KS+HRrpCcUBGEupI0V" +
            "Lsgs3fw2yjRHf3Q43nDdVEk6iRJA7C08OjVj3HCoW2L1SsyWi29l/k2QqFwfEv5R/XvzcvDNJ4AgCJOzUE/Ut58W99oLcIFBt1kf" +
            "t7ZBxKoAFIzZ0Chz8Ovot9+WO1DfBsObqD3UQNbHS+WmjXAdX55rCkDz9beF9IqCIEzFBRrVngttCFfMDffpPOXLRxSAKZhECWC+" +
            "XJVe4sGtE4tHMJXVYI1tajzd/EkV3KQ3C7dM74ItZUgQNFIXbp4Fi8AWStukclLMs3nw6PJlmGaJ0AlpXSnJX/JppoRcMn+uUQEo" +
            "CL0cs1NZ2Luci2VD3e83zQWXD7sog7NelgsVbo2VNRCxpbt7VUupw63Y9O28ZGKPNG1rRmccnZ6EQNzZ/TJnpDekCEwzJ4CmfRWK" +
            "fLxnFhfb70ObjFDkGxetmfA5Y9iKS6XdqK3YsM2HoRUDIU6XSnFNjH43KU/CrbDisj64vbrmxm0iGh3DRy5h2OaisGYFoEX3qZwe" +
            "G/nVvXwcinjsbN4dqGBEDLqvcT/uWZdPpDWxodojLMtkFUXKmCBsD+kpB9G49OGING2Y6yctLDafCNuaGLwWVpLpk0Rj0fZ2JQm3" +
            "enpkyrX1l9f2PoIwG2tbGUVYDxOXi4BlQEpeyVxpsYSTxCRzAqZ2B6rR6Pc2DR1d31bHqKS5iMAV8wcSqvSYCBDzv90aogAIQke2" +
            "XtlDSAMwNxV5wRMemmSJwPQ9ocDtq4dP4KvdtVB6L+YONNiFeQUrnE3lDrQqVvUOq4rMiuiZLlueL7C1+ArCxbhGaUwagGmpp2dl" +
            "CoE3n6BpesE1lbbpS5kdAZ97JHw+FpkYvNbq3TVea50YvH0kRdqZcbLLWpgtnltJAEEQhCnp2bdOufvUjVMmX/f+ZzErQA4gqx5a" +
            "/5wAb8OvVTCgft1yvYq/+5oydc1csSKw9vgJgjAz0gjMghE8OsseNyCoLPN6hD5r5cwdJxt+HnnW6DkBrMDMiohmXLj/Ym2ENfSs" +
            "lzXHzYcD3+pQ5PutM6JuRZ08Z+aqtxAUBGEcUk9n4QaE+aHMXeJqWyu4+UD1a5eimNMRSIAZ3IGuoWK3eMWt5BVXEo0o7RaAhhoi" +
            "BJgwjeZOblEABEGIIvV0MdrW/BcmYqVlukUpXL870MK0DZrKLPkpkBWChrPShkYQBEFYFY1bBzRcs2m9IRBp930qMjHVz88Wj6WX" +
            "V+1oEZpkiVBmEK1ENunkNOFf1BL3Lq+2ycqyGiT1+jGR291M3ntiBRAEIY7U09mJCWTc+LPzubkYvRp8LNLcvl8a+z8mKKZTTvjt" +
            "HFbgeFNXv419AnrSR7Yhil/rO61MVC6ECtXUlxkBXZlINb2KRL6KlxCEDSB1TZgH3+Oid0kLjLwzEBYq7DLabWaQlRT3KZQJFbnt" +
            "atyBeueVs5Z66N6V5P3NIOk9B7eQqrfwjoKwBqSubYnovgAboVeU59rcoCWctVlSQn5NbbV2ps3CLtRYNA0j94xSMAMdHyvX3WqD" +
            "9Wty2tJgkxtJCIIgrA5pMIXbYBWyVYdIsDvZ4AKRbn1sQ5MxmSWg/v4Xcuxo2i21azTcySINk0ziB26LTq/flvbSr83EFrcP7sK1" +
            "vpcgdGHJ8i91Tbgcl5qbUPk4g7/uNY0BrI1IFZ5sTkD4neeaeTiCpigF5gdw7Yt38sbbxk6TsJ0LxSJwKVZYF6NIQRCEbkhdEa6f" +
            "JUWtLr1kSBlYizhYi39L1z+JJSBvi4TDconU80l9I9ZFqBXacdNdEnNm1tBECYIgCIsiTb9mxDKdIWtAKPDZxZgxD1AAkvqh0ZA3" +
            "J+Dy5a0eA9+guYyBc77icJk0HvDUC/nICVvl8q2HIAjCZgnMh1x7qzqniFCIIP7wPU8hnoRnhs72PjMEPIk7EBApZBEzRGi/2Omo" +
            "1wB/BdPRjhGdbl7m7ZZj5JNjtY3F33RZJK0FQRBuDTL/RVcKmlcwCzLXo0Jr/vv+/TM9zfk9cV87U2JNYwlwpeweq/JML47EHk49" +
            "rpgCUQCi+MqAyKSCIAiCMC0h9wdE9hG7sQ16Gud6jgp3Jkl9RsVspiVCMUtB6hNk47WtikldVagYfS5USSYrBxev5I7qxf4xQRAE" +
            "QRAmY+SS6XPQ2yd/8DN63DmpsL0NP+gFNwsLl7oxZZEafo0IqCCahRdcG2p06HNW/l5h+1v3CYIgCIIwjo7bNs3jBjEr/eWfSykA" +
            "a6H9paZ3B6qfjXyvHu2zhH81xPJISNmlmKwZqAC1hWpYH6nNCh9qZpiIwWV17qiFIlZ7ppcJG2qABEEQBGH19BGoNkb3ybwdJaWr" +
            "FP59bKr5a3nO6Q4UJX5pf3efsPBfCzCmh8RuHLGM1FL0jtcSlb1pxKGmkfk3bbA1EgRBEIS1crPdak8Jaaygt1ZBsQMLWAIuRFPh" +
            "b6sYsbXr69siC4O52dZJEARBEGZFetjuhBf6nO++NTF6idAU1QWR/MRYuiASCi+e2vGmgeqwq8+Ws/YCDDZPiDVAEARBEJbgAquB" +
            "lnR56AVEgWuR9vougT/ZZmG+6/1i0z39BwbczMn5Elweq3ZhgBlLyM2KvmT+u9kEEARBEIT5GOMUMTld5agNSeRrsQYMzcurcQfq" +
            "O5+AfNmz0QIw/86/Igejqq0JgiAIgtCNwCInXXrS1fa2c4hdbRulTRDUJRiTh5MoAfX5xsszJkNqW0qPYk1FQxAEQRCEq6NB8rvk" +
            "fkZRhi/03+9U6MQNimVdLRSj5wQA2h1odQWuA7XEadjtuXsZ6uuRdUV0ePV60so+AYIgCILQm0jXGfdywPLiyRwr71D1VFSCYOdk" +
            "kxWAEZVf/DmvvaSV1ghOT9/knmaJ0I25A/n31O4NvI2IqR3ouFtE+AI7oSOycYMgCIIgCN0J9aFL9qtzSYbc+LN6sE8cvLSJ3dr7" +
            "tS4uIcdZcMfgZbmc/Lji3B7EnCkpUr4gCIIgTEmvrZuujCklsFGpuNzU0tZHF+QAsuqhyVYHmiKcqWmLVKOi6O0PUJ03MJl+OJhe" +
            "CT44d6bL1mtTjQRBEARhjfTtuZcW4Oaa87tqOeNSUnJLokwyJ2AthFy/2pbHakwf5yTVNIbLFLdlhP+BN3eYE8ANvwRBEARBWJ5Q" +
            "jz91D73FHn8y2b33hIKJiTz/auYEuEyqBa/ozVatAKz6OYIgCIJwa7gLb/Tvb6WHnpBLKwARpp8T4I+Yr0WIHpoBUgu605bX0eWY" +
            "OtwrCIIgCEIrfnd6lWLMYJlhYGq07Gs6WxoPeM8+cZl3TsDKBbte0bvwrl5bqsStcywqx1ZeSARBEARhjUQc4S+9KNBiNEwEGDwO" +
            "3SGhKPK9kYn3SYgRjE/DSuyTzAlgAq+2gDW8vHVhZ/fAylhhlBrpXGabrAKCIAiCIMTxFi9xhYW2JfGvnW7vWJ1FWkmbnopAr8d1" +
            "ZWRGFfsaEMCLugOtiY6JTm3XXlH8gYMAACAASURBVMgKsGUFoM/KS2UJvYXmSRAEQRAmxl3IpOGyrckVU1CXLCj4q0vahKwAk6fp" +
            "nKJQUv151UuEtlERPSOJvtK5HNvial5EEARBEK6dCSSfRfv90BBk9zu6MMgFaOgDpxQ8/ed6+wRc1RKho3G2eF5E22thM5pVU+Hm" +
            "FqtAMLDNvLkgCIIgXCE9nIdWMdDXHAl2Pk13hpaa988NYuiylTOn7VUuEdoJbhDuOXJ8IA3TErZPQ85vr1AIgiAIgqDpILmsuKNv" +
            "29a1KerkfRrpIuANTacphMeGCdS36w5Ew6wzY150e4k0A50SfcWtiiAIgiAIvVhzr95ZNqPg101z3RODW4hmYuzEzKX4WgpVnJD+" +
            "7amoa24pBEEQBGHDXKKLvVS3PqlM1bDKZGdWKN/crjtQiJjdp/+ck9FRuF6FwFEE7KpAdma2n7bXUaoEQRAEYRkiwkODR8isrKUb" +
            "H+fP790dEdI6PWPg2v9zyYS36w7k0/YGaynJ14psHCYIgiAI0+CMKF6qZ11bj940yBr3DPGcwSPLBC1hEZhD0BZLwEi2r/0sTCXB" +
            "HGtAkJstVoIgCIIwCOk5q8TktNAAf+wAmRtqOsDGhcCrmxPQx+RVOKZsPBM3R60WNS0xJM2ZIAiCIHTCnXp3KR+gDeEL8lT7Ur/D" +
            "Vx4qbCzNJ9kngBjU2Z1mRoF7ULoPiA813rih3L8ogQkArp3O3S6Aua6yC4IgCIJQ4+K9ZY8tBi6GjaP71zkVuyV+BesRZVr7i1eZ" +
            "Zk5AmzvQghNr+2Dlyv4Vhiry6sUr3GZxhipimzUIgiAIgnAheko4GxGMBkfPyo0rf7+uTKIE5FMEgnFi33pExq2XjJnj77v/5Lke" +
            "6a9t3nbDE00EQRAEoQG/p3Z/t/adHbv53n1wU6QuyCSyRJdB45W8b42GeE3mDsQUtgAFnx1YDTJ+cQsTuRkFLEL1C9wLO4XWfGaW" +
            "8jJuHaypYtEI2/+ZwcxgO0G4SDKqXLvWeiUIgiAIlyImtzTKMRz4jjaxhuJXtXXQK3ENGhuF6lTGjv5DbQG1MqEEFAmmtyXAX23q" +
            "jHJ1oFAidz3WfKJDvCZYcilqxRpk3oqv7Nq45mufBWEp8BkK0bThBZ9RfmWjAMB+ikvqD11B+yEIgiAIq6NzN92wLmb3PnagUHCh" +
            "kbxJth6qyULOzOuYANk+qaADoc1VB9DyzEVWB1piSsCYcFc50rxkpC7m3MYA54VFQBAEQRCEGbmUy84qBa1+BJ2U+wzadmYmeUgB" +
            "SIDXcxmbRZcI7fRaHd+9k2IxIKxV0VRopqpQYxWAUb6F7Bznyi9BEARBEBZgJsUg6lWxMqJiUGt8Jxqtb6OrxWZA+k4yJ4AZ1CZL" +
            "9kqiBqf5PuadwkdpgvxpC2I2t7c5ffwnUyQQf/kGDY0ZVSsAN2a9IAiCIAgGt3sd5Q1s//MCnMJAXxMPVjJHAEDnSZrNadvR12WE" +
            "ez/bSDSl28B0XWaJ0KFw489hikWAKWXh9bCQAtAUXiDDtE5mx/zdb4IgCIIgdGGJQcepPIXXahEIufQ3XTPZw6YOY0TYV7djsGUt" +
            "QmXvucRLMNdM9j7XFlY0Z4LwanJNEARBEG6QVtegYYLKWhUBl0mjNJcXxxjXrRxAVj00iRLA3BKNMYmxgFw45SMGv+pSpW/Ic7re" +
            "0+qbVT1QTgYW4V8QBEEQurKoDL0VRSAkSoSEZnd1T9/60XBrbwatLFm/eVS6tYhXk8wJ8J8TjB8FLuwa1hocxQMTMzpbZtrefSlz" +
            "wWinQY+hsjuXX5hzuBM3aBWZLQiCIAjrJtZLzuYqVHPub6Mek9nmCLAjaMWUASq/Nj13jRaBQcnvEpmTMI0lwJsT0JifYyYQzzRg" +
            "3BglcjxXqO3iAQ/aggLQNcw+bYK7P0DQHUisA4IgCILQl9mG0NY6R4DdALoFtsVhxsFxbhCnppkY3OYO5DLnevADtY8uK//UDnR9" +
            "46aZNVtWALo8xjtQE/HN6kCCIAiCIPSg0yCcO7h2ib72UqL2fM9t8S7aHAtMDHYK3hICX5tfek93JBeCl+FtOR+6cJCP2PoVAAC1" +
            "DfQAraAH09Ndv9WxAsheAYIgCILQg4YBt8uzUkWgi6zofuwpR6eiK1jPZBZ3oPBFC6ZUXOpsva1rLCn6AxHVMKAI9H9SP5aue46/" +
            "XUgZABzZ3/ylinlAlgsVBEEQhE5EhqHXNzJ9qfHyDs/suvb8lXorL+MOdIkEW2Um9a0EG1IAmnDjwuWYf3mAa0riKrNPEARBEFaP" +
            "9KAlEwpDV5CspKoJMo8lYEKNaTpvtoC9jKcMP/yYiwW00sJK5I4JmEgSKnMnvJ+CIAiCIHTElWsmFwUmWu2mwkKLvhSrTa5UProE" +
            "ky0R6q/syFz5OUnQ466NnIksm9SZ/pPSWwKZkLHvNoTIcqiV5a3ILRxcuYhAzvpdAIk2IAiCIAjrYdSynt7NowTy7vLBHP77tWUx" +
            "NyiuTO8O5CXykgpX52d19QHrw9oUAMulNN6In6LvGciOak4gYykgYw3YYI0SBEEQhGtnlJu/O0lwZBhtTCgDhTxH2D+wMSaxBDCD" +
            "K/LaRjWiq2VN+UHQBYYzcHZEfnpFfnpBfnxG9voV2cvX8jpjFag6CvnhKZBSAOmPVhyq1gRBEARBENYDgRZbDdC1PUwlFWxS7rd7" +
            "szqS/3TuQBPQKVEHC7Rx+9VlM3MBCX3G4IkaFn7yk9z8Zs6QH59xfv4M9fkXIL1HTjvkOSE7nbQMT2arbKKKYE9OoASAdneg/T3U" +
            "7h60uwOUAigBlAKRb+gSpUAQBEHwcfvhtj55anFyCP5YdMQP1xJyjl+FFLuaiGiiK0vW4zlotfcJCIlVbdc0MYkSYOW1Kt0ztncR" +
            "GCQ3x5/SmmCz5/JMisDYuQ4dM4bsXgCh6204VgEgBucZstMLzt8+Abs7MKXIcyA/nXF+fdYuQUQgpRUAIipG+bVCUPqfJXdvoR7e" +
            "AffvoQiASoFkB6IdGGwmGDc1kJdqyKdq+MbGfy3xWDtzdVTXnm7XRlM5WEtdbKJvHIfGaa3luul9+q5oMkd+9Us3dhetry63UX5z" +
            "gqyoL0632CZUzoWZ+eetGLggnbN8RYoKuudRqxj373O5A9WvGHCm7aGYrLR2ioPr/zZLeZhYEZhisnOkBIWSobGwOQpAYQk4veD0" +
            "/BmsEuQ5kJ0zZK+vSL59gUq0sK+UUQTMqD4pVVgCbHbkTz8gzc965D/dAekBRARWif7LBKKmtF2Tr9QQxsR/XQ2bIGybLdTFPnEc" +
            "E6c1tqtbaO+6p1tdAfBHrKvhxGwHoXm6lx7dXpwtFI0O+Lnf5bUmcweqeoXMn6J65JnRfS3JepJUEqtPyZ+txK6x4YxTS4bagfD7" +
            "MOfIT6/A61ecicA5g89n5McXJM+fjOBPUOQpAMYSUOoohN3pqK0EuzvkhyejPCQgzgEkAFgXk65vUbtwjvyYuvCsodyswUQ+F3O2" +
            "Z9ecbtdG19HhpUbbh9IljlciFW2SUP7E8iOkCLTd0x7yZXqUi6sCt8MXAE/662TuQLn93vGesWMM5Y9mRYCLD3u/IxdfvC++cCQm" +
            "NkawNgHAjt7rr7meFHz8BnAOZBn4fET++hXZ/h+lsG/dgsgoAoUSUP5lJtDuAerhPZLTK0AJWO1ASQ4oRy2Nvhc7r01OwfDsHRfJ" +
            "krZa4kZqysaTg187xYX6xmOJFuMSdInvXPk3F3NVgi28u48f5yXycmy4S5e3LeTrjHHs03zXDrS1wa4U41sC9LG2x489f0usQjSc" +
            "icknBs89xhCuD2FFwL+203NXkdsXiMRMj9MyYdkgEWCUgBPAjCw7gU+vyF+/gtI9VLIHUF0ilEgVgn85N8BM/E0OUPfvkLz9Cfnp" +
            "BUrtwGkG5hxgf+3aZmUR4HISsl9amBbOkrlGHXs8s1dFNRdbbatztK5xVLJrfH2HurUztdVia/lqiY2dxpgivaZIqz5hbKE8juWC" +
            "CoB7DfkHuoTDgb+232qybAt9GVJKutpx5nUx78ZkcwL8ySdNHtiDn9MSCVcRqF/bw9R0a4rAAgN8ldTnHDgfkeMIEIGhtDnJLvNZ" +
            "tGvW9cfsHFCxCiQgpaD2T0je/oTs+TP49ApOD+DsrMsDa+WDioeHhBg3khRIdm/21CJ7F/SpJZcqrLGRUBEUhyFuQdtgSHkcm7eX" +
            "qANSHgczW3aFXH3cfIr3BbfSik6Nn+JNtYGcT+hcLQ+o7YLpYWWmSDosskToEmJKu3tPPIUbMyd+aAB9c3kV2kiU1rdpLNhs3HkY" +
            "evFaM6uE8nK3abarBpiqRQCgFQWmHJQrnF+/4PTl70h+/RnYPyC5e4Q6PEEdHqHSfREPAtoFeCqVjeq1RQDFdf5rVt8s9sp+w+0f" +
            "r90QiqT3UPL+hn52UMk50MHUHt/Dvh1N6y7H22x4bvzmaDk75Ci3nI/Gq6EM1tKsbxrG6DFK3cVjpFb2uj66Y742Pbsrvb3M+gzd" +
            "diVu6WFjpeweYpcrp+4reoRHwa/Dw+tqlKxV1aZ04pYYxAY20MG5xg9m4ryotHXsxIeLAVCGAkA4ffwZr7/+GdnzR3CeuQE0hH1r" +
            "tMtW0V6ZG7q3DTKNJYDa06TXxNs+z+5ywSoMAENr2voUgV5v0qoIlJdVAmfd1FlbAIO1UkCsXX0AEDPOr99w/PwL8Pc/IwMhMXsG" +
            "qP0dVLKDlemrQnw4YmSFf9KNabG8qC3dZUCRViAiPHoNuP7jF0xf8G6RwIp3CMTHfddgvPxncFVBqQnX3CPTPYWpprTE4hOxiwYV" +
            "J25JpyF1raWOBRWO9vyrx6T+HAoonIGagchazDPQoHxETlWSJ1rnIyOZftr2EsxDtPZG/Y41KTJROb9PnWkiVB99mhTLjteVT/Ou" +
            "7lDiGi9pGzvtEJFBzw2VNf9nU/6GAu+WoeXAlUege+DQBQ39ZdlvVL9zMTimcP7ydxz/8b9xfv5UWsW9N1iXRDExsarcQ2EMnooV" +
            "iY71PNLDLUJTfi+zWZjbj09Y+vol5gB7S9Pgaq9gYgJBGW5zzNanCPSi5QWtmF9+d4oMl376ui8srQaMHOfXb6AvfwenB5zPJyTp" +
            "Hsp8KEnKQX2CtjyEZKvii1YAqHJT/TtVhKGYsM1OI+8L22UjbstGSBAqflXkaEcpcb6zjX9x3O/EqRa9sgVzlABXOakpKkC84XSs" +
            "Nq6CUiu2gQyI4sbFPsGNmz5R1o7+zWt9bNB2xBGBP6QgNZ6vXuW+f7UIecqck5eFKtyg3IVTM6a0dBDMas/yfjtfg0JPkQ61Gu0p" +
            "dp6Q66Z7q/AbIq6oBCJYfyZsiXKCCZ1vUgzantf50q73uA1sl5dvvobhjil07He86+N3dQivQc/qF1y1nlIosNa01mkb7sHjkmKw" +
            "XfHrTIOeElzJgu12r1x+NwNiTApMCUAJsufPOP76Z2TfXEvADdK1SnTt4vzmbKA031USnUppcJ/nb58KTLU6EAfKfIxLybONZkLn" +
            "Mv9AiyLQTYCPX0sN5+rhrFsRaHyHwMnybRgo1vN3rvPkArIFjakscMdvwNdftRXg+A1KpVBJqpcJVcrI9VTIpCYgJxZcHrJzEjzB" +
            "v7QQOMqA7excwdsNtdLKu4KOFYLyipBNjmBRubVW4PSzyXxnVykICJBW+SkDM+XIVUZc5aQWTzf+TsQqslpAKaJKggeUplhZ9nrG" +
            "WjranDcds2cp6KYMkHdFV8GkroDU/jrpVI9F6N09hY1Q5mtxrHpfzXJQ+9VFWKnfWzvuPifQ/HRv9ANxqSjDQC2vuV7OOlFJGqrW" +
            "o6IhQaBc2V8RycyX2poUlFr4gfCaaAo7+sCm36jXvzZBu6LvhfLZKRCNxaBD3EJEXj8Wz7Aa5NdPRPKxel+9bYjnYzyXnDYm4ifh" +
            "F8HieZVAnVhV+g3nLxhMSfHJjy/IPv0F2ctnwFEC/JJ9cWlirHQ76sED336ihOuqCPS9dshNs7kDraKQVeiWKv44hpZLm+8dlEkI" +
            "N4+TKAIXTPgm7ZUaX5D1cp/guOu8/ev4lumlRb+AicDnI0glUEpBUVII/1qIr0bST0krVFeEfjj3h4TtmpDtxJP9Iw3CD5wugyNJ" +
            "5HTiRVxhBfCA0GZXVnLiWQmoMsrvCPwcOBbUTuxXqvdbKNMu8ALe9xh1YYwirkFl599fYvR1m/ZoRYTEoGDhCwv1Gk+Vw/V8KhWs" +
            "cpJ8FD/JBxMKhYI/W5OtdoGfZl7Z6p0hgaB7JYJf7hueHxpIMvFmRGS9Me8zVHmLHm5PGPcV+/s9U0sPNUwB6JVylUdU24vm1Yv9" +
            "kzHBvVtsuGWVNL9YlPppTGmy7THXvhO0O5BWApTuE58/go8vsJYCG9KgKrIA8+oDPUK/mGKCSgYtmT+zugOttcDFCHfR3ca7WgV4" +
            "c0EwLKpd1kB/QWdRbNsXHmxpfkHmSuNYlTlt98Kw+w4wAJxfkb0SKMuQv35z9hMwuws7MrIWVsuH1/KbHKG+WJ7UbcxLgaz2wpF0" +
            "KBMgIDCypwB41I4WQp4v7FMhj7sPj5v03UxwFRI0f2+MnB++f3nHchiUNZuEwylb7QYVNvj6oWczoqcqz4D3rqHjHYRx98xkVb17" +
            "QMNTP1Qf4iH3f7XYHe3Cfc0dKHS26cXJFzjnliyaFcOwcN5WxmPlNI57e13g7pGDsb4jcqRXgP1iAttWdh4j6HisEr7TvFHlBk+r" +
            "9623KPsOPVigtEqQZ+DTN/DpGZyPrUcz0Ji/02D7jV6DFIMj46tX3QKKyoIjE4UoPF4RY3l3oIvRL2WbXseRKasNX+wpsRsiD5o2" +
            "KTs1/73oNI7bpNUG0qEc5GXvd3lxWbDLm/l8BOdn4PgMUAItEltRmcogvPtC71IOqJfCIPkNcaNgHfoZVzqqwm0DsdYiFPnG+xuG" +
            "KiuNYUzgbxYcavoRqFdj1BR4m/IcjVSQjsNzgfviwv9UDKj90Vuo4VcT4Xzu/daDG7Kp0ndoj9ompbSHOX93SB0fUg4xlJdH4j8y" +
            "2Wu3V0cmarGo3xVXoMPKgB0UMuGSc6zlqe0nWk/Gkyv4Gm1tivkVUsRqioC+0K/dperKAM4AZyhW3ptdEe3IYtGo9h29lIFeNAl3" +
            "I4LsGVTD8FUrV7NE6BhCwnyXezqF3ewDsxDL50DrW9cuiF3NXuPmneUz+KzPM5cjKfFK0dBBULVK19rgPpJov8PNdNFIez0wkjv+" +
            "obb88Q41qBjdq0Df15iSzkrACumYQO1JFhcQl1MCFg3SY9ocniW+dW07SF2FblGqq5d0pk0YLmlsIWqHw2ca2qGOid0x+aKBtiUR" +
            "OxfFxqKrOeKJ87X3ifRmTv9UyVmCnl+n4K2Mt67Wa12xWRlTZFdHzWCqOQGhuewNN+ACWsEylaDylNA7uhesWTMaQf9UbrijSC9d" +
            "aOJXcnkt9RA+qVocGajtGRYc0CqONT/Ab8cnLYGV3iwQcqsC0BKbtipjB9+6hEb18x2Cr4fdWGe6jdBGr2qtj/7YXptFoQOztgH1" +
            "GI4e8HJC4PAFkxMsB42Ehw2afrdd68end7ntQ6ekDNWmrtfaI5E2o+nFW6JUKXG167s0Fnaku36oc70NxHN4njRf3RSnpjvrVoKy" +
            "g6gLU/U0ibeNJmdpAYlnlsCnj3VFFmBMFvzs0uRCetsyS4SGWFwQHpKaw3KgPtLgCWqj3nmM4Wd+RisAjiBfnDbHQqMqWkCncvTf" +
            "bx2DaR0Oh8j5XoQdCKJt9N057QsWg2kbuuvsEN4zhyrB1ntjjnRMlTvig32Nz+yUdj0TtfuoZVjIqnfe4+M0GYGCOqz8cfDnIHFo" +
            "grQYWof8GDX99lWb7gJc+/l5rAER9S4oZ7uNWKkghW4Pn2wmJITWE4kafraUtw7Xhh7R6bLGd+1vBbAXdWonQqK8373//+29Qe8s" +
            "Pa8n5PR53vtKDKyAxSCxm9XwJfhQ7Bm+GF+ANUhIs5jZIN2LkEDce4f3PuffYdFdVYljO3bipFLd9ZPO+XdXJY7jOI6duKqJN9ll" +
            "n4r1iViwqOZ7lXKo+9Hv/Ub8xbobUvin+cBM874mBAJezwS0/4oN66jNA+MSiiVEiB5Lz4iufXTQHQBsoLbokOgKI2vbCCvbS5pJ" +
            "nfdixDKnt9IuZsNp+Gijr5O+5KCoxw+dAFTrOzuC2tMAxcalkiYurfQ2tBg2rXWuSBUGj1E51Zqh7cNKiVtNnLTqI7XEEJO1cCe1" +
            "SxPHFzZulVMAsSn18SLDT0V2+vFo0yG9nSecn8L5JDbJCCr7raC0Uz2YMrUmbYNTwLsDSSB2irs6WBTUbweYEVexuI1cmObs9u9L" +
            "IcnKNjkmCDGgv844Sw1aXTtxd9PYmTWngF4yjvtMXwZ+P10jlTmS07/NZVmYOmBwmtsbcaNlanXtfa8CbhthLVBujlSbHTV5LjAp" +
            "W1m8iJoebP7nx8Xz0oFSeB5XeYZqOCI8HcSWeANaKWjEqqPdxn/66itVO61iMujQ9pMFquKOQs+PO9uVXnu83Vy3A7UMKMXFgTzm" +
            "Mu9qZwnbogHHqGUS9HdXbsYWlDShZ8fcu52ssMZYHBebWBR9yYodGmawERWbOrqh/U1o/U4LXZs5csEpR1oYyhc9mhacvup6bJ5H" +
            "9MHtFcynRAyMRP7NCulAHIpjMWNdT36WWqR9mBmph/IE7OQ/IU71AU/crJ6leYsOxeLU0A9qPgYo6Ui9V5xan3L0akJM/v92xGkD" +
            "dqpejLAfbrBpYi+LprB70iRJ7X+cGAj0vQoZwOLw9GQbiddrtIxF92YmBwAbmExidy64jDSSxqm5ROH975mV+Kx0oBReurRezy4B" +
            "dmeihRZKeZUmktno9DxLcKMNDQvKjRspbr24UcXswGMC3HxIC9Md9toWHPkFACOw9SX7VW1UJhDX2MJn4Vf+1SUIuAZeI8D+8vtk" +
            "Puaio81ZVQ2Fq0W9HlygZrgH3W9HYD7fuD5u7/zALYtzMcm23CbMDqfELxcq3w63dCDVqwKZe3Nw5McN3+ivdvZiZqMjwU6sKqTo" +
            "1LJ3tnf5s3WDsAvBiD9IZSoBwJARvZiamPBBfWtNsf1YOD77cnmcv/ANw7T3txgmWCg+zIG0HikpdNxthOvgSQ8f1EqudwIwTbcX" +
            "QHcQ8PyB+MtwnnBGqmQE4phqVsOfYvw7PJ2eCSXl9dWc/eYfazYEAEOeA6i06UXqW4zcLHzTwuGOT7KVFD60f1MDYO0EO0nOe6qq" +
            "wCN964QAwBVch0ul9w0AxkIcSi7mSfOB1u1aBpd0oB/0TMCafS8fXJBU94YDKqdDxc1Ga6ei3UrsjADAERdg8aNwy3sObjl/KWoD" +
            "v7hi9C5HN8ZCk/Hb6a7MR4XRtZ8JcJVy6dprnwml3jhzBwq+KMaiIR2nO/sKlROd/JEWYJVc1gWs3AIs3JiBqwx0K59X6V8jlrAl" +
            "q8jYzIfSm1ilfwJqy/YFupCB4zcA5PlfxA/yzewrle1y3Hs5SBHwe4FeWPcVoSMkiHamqZOBgL5nrxxL6l1NmU+DNZ/TmN6zf323" +
            "Q/2SbcaHFjgg6KGVVl0lfWizXQq6hUg7ng+ZgpMn56fahjM2PpaMtYmKWtl4poyZ+B8pSIMtaaVN0RHlaGirVlRsp2G90WbK27CO" +
            "1dEc1lxpE3VP8wLN2JWvKS2vjsXRFpI0MzBrviJ0iD7To2j2na6kvSuhwyhLDnhA/1x0BwWLHxcAGOtUT2l8m+vDOmvhx2G2aJcM" +
            "ADroeaYRLBMAzKBvbdYxAKiWWcbeXMsxWUZsGojHAX0k/EFbGUk7BqUD9XhKflzk0E0S9+cErjU3B8NpcIXcn65AQMwp6qDbW3cU" +
            "VuSpB5/Wny/HlUynhtdT+jNrTpywwUHKcwAf1XFbxu5cacZ8ABYXt3Z++KUDFY2kRxHKAyDPyVTs5gbYDixU3KQ5P8rBJt9eg2l9" +
            "MV5yV0g/zbmK+aWi3AZ06hW95e0QAOhfoman3VXf/DaLej01HUSyGff8cscKa9yQOcPQnQGvdlV0Ok+9zejM8+i2NQ4d7HrD55kT" +
            "5nY2WJzyZkgCM1Skkp1GJrK4BAE8hG576emEXMfWotl0XPjVZjMR3v/HhoehMm1iI67XPY0oXXaSOoODNEw+DVTjcdAOmCLgcEvr" +
            "utGMFQIADrtd5XRkVOQgoZYbvoD9n+IeNjbA2vVOupa6ap0fwaMr3TsQwJAenG0G3oRUiNy4r9wMzuumUn/OeSbgFP08TgHmt3qj" +
            "RNtIZAdNXPpO0P0L1PUmZhqwuo0+4Xh/eNs3VLiCzXI/6esFZ4ek+7X6A7Di2GY8rXSCO5MmR7e5rRVH+sOQitg4TiuZL5cg4FfM" +
            "+zStg4MaiuyX/FoUityQUR06oUBxa6UZdaOOUc9d3PgaiGqCb149L94RK7I4jKcK4RVl4cvUDM+kp40LeE7ateoCXeHg/mAw+4rG" +
            "7b5nW9kn362W9Pgo4gu44I0mBGHcaikhXoFAsz/qeAqw5GIEg/lidr1WlcU3YfUxUL1lK6C/s1A7oeTqTMDK41rsIvYy2xAAmJr0" +
            "4LFGu5V+Vi9W/vXgOgEAKU4p/aom/7M2GvpA9tjlmYAYktNZRhjj5suLcl2l2pQOvwE2pJHBNQZ+aYT3/9QzAlsgUNxJHhbGFVQP" +
            "fJM8iAy24Yo7UQgUjx7muxbkTYFX+x+4EcCJZnZXu46Yz9YvCoN4WrGrVqxwwmtukqhw6mvELSloAND+VNq1AgAxc8fKznUnG8m5" +
            "y0lAiGeJJRCfxiD1/fcYWvPwZEsjXwl+BC2LQ0+a0UwswkYTrsz7jevg1jMdbjndAICJimBxVFZ5N88NDv6/E7DqGKdnyN2TZdVO" +
            "NkLbnZHdXnElG3gKcHXcoknwcR3icVp6/RfJ+Ma1cU1VneEErOU3rcXNYAhK6Z8OtBo6cuvkB39RPpDhVE0setY7I6V2Z82WrW0h" +
            "ZSe2lOUKjELPScWFgI9ZtXVGo4Uvl0a/BNX55UR/JiSd6e1vSptKTeC+a8q6wZPYc4JvKQAAIABJREFUV3lXNCh9mmWX2oey5ny0" +
            "cr+WQtS4GW3fTgMzrH4/FnbWg1gS8AMyW0CgGF1dyv+7lOH5gOIZgz4G/LHCsw7CGBW3KmVr6J3oXTnLH4DVuryiGUrxSQvLqjLW" +
            "gOK91h+v06/aM4WWst0YMYinROHrQaNj3mLyGU7KCfjcAZWCf3WvV/CbOFQ6cs7vBFwI5geOhQqXFNKCTK8y11bh48aNG3rc8/bG" +
            "UJz+hKQ3xjgBZ85DbY+ogPzT7MelngnoE/67NuOzi2x39kl6GRc+GYjczZlQCWUwLIPdoRhuE/rTLAPA5fpkYvfjFuobNxbCregq" +
            "DBVT11tAnZyAC+hBS8b4qt1q4cs/HWgw2vO1jK/y4V5DOQDc8/Oh+DIRK5wAtDwjgKHoR5doJ47LKWm7Fzjav4Lzb2VhcZEjeOcQ" +
            "+2CBof5uLKIW5ofM9Yf79UY71p/aMypqNKeqOA1Uz/N7g8ClalF8cOl5q9mX1qXaPx1ocjDQBIOkllmMl2HkBPQ8aLvaTG2Edzeu" +
            "5jhz+JR+YFyIVQHn9eIz5HejF+YAgCiz0iZRM7kV/IeedXwSVuHDih6+/dOBlgUvpmJ+JBeukLf2Gbhgb68Q8HphCSa+C9cQ+TW4" +
            "vLEQzlKZSSmmXliAhY9Fs7fBHSGExoCzp92O4v8q+ez/Y2HL+nIEY5H5h9B0/Fb710Iqwrm/RjgMfTk7p5wGfEAAYKa92Kr0qacA" +
            "14F2W3Wu8O+hVuJMQU1oO7Bf7AS6TwMaCbimn3Q9I+AEQRbDLIUXUTwYCcNsE8LTxOaY1HKKlfxNq1EOv8szASQTy6GeMZWmzgUq" +
            "smsGE4A0ho4xfpIDZLBKitxCqe7Z9i/7wTpYYFgi/1PfrKxOYnpMsyOour501sDhGO32y/McrzhuLagTlU+3KDzElJbTLc8L7mxo" +
            "xsNo8RSBgHpjrjFpe4iYzlYBQRZU8MbJmOqGZu3q2gBj/bZNsJG+XSG7QXwWJNT1beMg5SRt51fy+d/AV6UDITCCJC93TZgx7ww1" +
            "VV12rWpkzGFXZT6WXIZZrMTfSrzUcYVcByvV9UfgnB3EReVyhQBgCGp9k+53yOViaUYAsIZPYBiuQJTlqotkO0+ENDd7TzSqYtES" +
            "V25kj3tF6AqvmcygY6RJsXrQ8U7QZUSrgSJ6nYFVbPAaWF8a63NIoZXrFXu7Ik+TcCkDS+P7Rm+k++Xf4o02iEkUaSoMlY6DKysH" +
            "UEPKC5b9h4IfI1Mu6UAxQCTbXcqI1s/iNIGpPsozJv6H9IuGG76V5rHw0uiODX4+QQURVx/Zc+14wia4qywa/nKyt39dnME9s10W" +
            "mc8AqgE+Ww8kDJGyqbMraWnO+PecAGCcEwi8pG94vm3VSTUTBnGzO+CEfZNE7KEdM2aTZHcDAERqpz/pP2X2KVzudwJWgdzdhtld" +
            "5Om1J+51vxa4Zyy7DFtQhmqR/apvqUQb63phXXGKtPA8M970xNk8ufsEHrtgaFFVFHXFlDFJG7mIY0bLhTOGZ2v2d2BLnYtaJVo5" +
            "ujZC43QP6yrl8VIbHsgOBoP8z5pBtUCAKkuJQIL/7wSAsuXp4B/WWMZELik3AzoDgPJTvawn7FSX0ZyloDlRu1FiSblYdurOa/rG" +
            "jsZchxuO+C6Za3u7hFRQvoyGp7P5Hs2j/ytCJ8Hub4a93v66TXWtidiZOlH1pgUj849t/fCZKUDfio8bH88OnRAITBuPj/SZZyYt" +
            "3AAAQtS37FcEOyrcDWEYP2WE/V8ROhgj/NMazfmBAJPbO5WHWQ15BwJ9jOuOLj87BcgDZ/ZbbHvxI3hS/1ZQIuPReY+ITd0dIZuP" +
            "yNleQWkWRTXZ2ovu4sbmS8DPhAAQkt3gxilz9Zk2Jh1oEFobee3+85l61CAG5vp4MK3etkQJn1ETDUc3jRujUA0AqoXWQCg+LIAJ" +
            "JwKnBwAz6d+YD8kGeNiH+0TgYvAdnyuO9mXTgUbiEzrz3THDqEDgEzTjBgBcYyhX5HFgILBUAHDju3HrVxM+Mrvuw/HZPxaW/VZB" +
            "yK950B2Cyo+LGd88+r049zHF2/jNh+oUYMN3T44+DAgE7gDgxnSkx/23TjXh+mKLik4s96NXZrxVnezA2N8JWAHp+I16oMC98xWi" +
            "1p8gWHZwRqM/JzO8/9dS+VpRm+GXeG1y/q+AK/BseUbgCv258b249bMJDc/SroE98FO+HQYArhwAJCCH5ovSgSL5sYvcWScCVkof" +
            "ob+t6FXNkPw/tqXvQWA+91GaU3EwVuWLwj0pbtz4Slx7Wmt2/z8TVLcv9WCwyIOl8FAFOF0UJNbkanUE4duNdkTmszPuARuPT5fx" +
            "p/fvhj8sOnPr1/L4dN/J7RWhHj8229NuHSNf1xUHpQX54gIsDkLLq0TvZwDGYrBpvdqAXI3fFJRpvXJ/MK7ypsdPkvkKSDMWrdmL" +
            "Fp05Vb+sShOT/9se/j+lq51zA/8S73btE+ASBIQIYXsoYKajaRuEeDwI5D16lIYsiguwOAnSVPZ4QeiNaWgZlBYbcA8+j0+XzQzv" +
            "5dNleFXg7EXrA2KWXyWd6lm2KFwugBZ/4jQn2nF+fUoAAOCUDvSD0oHWfUo4gROPBZkLaMcFWDwRg5T3CnPiimiV69D3V974SIzU" +
            "gVu/roNPGKtvfVo/ZH9MuP47gmh8z4PBIeRMSiO5fm/sSPp7he5dgccbJ2OWkpykjPccWBALDMoCLNz4alxIAxWs9vUmDw1i8m9F" +
            "UHy5PROwKuL+LmDUfe5Y70L6bUa81gbAVVJxRVxI3ksjiF9daK6Ij5gDn4YT9eYCKntjNk55EHnbVD3XOon2MaC/9Fca8Z3opCvM" +
            "fF4H0jMcY38n4OwVjGAqvVSw9uEWdo+FLtTPs1WoCxeS89IYEQBcCL39vez8ubHj23T+hhInvYkoZJ/OsTCMj/9yRhUsseKIye5+" +
            "1AYC18W4dCBuhCaAzt0KRaEg3NbgxC6aoU6FWhBXkO8lES6gCF8eAHjgltmNG/B5E+HT+uOEHrGUv6l0/ho5mgOXIAATKQYBP13v" +
            "ATMdWZSUQ69+EGRE/3Qt3+jCN8v3m/v+fbj9hRtfDe8JINAT/Z8Oul0YagAWsC6YBYc3xu35/fiHhYlyBrLk/fQ5gkhcqwHX17S7" +
            "wS0dCKAid089YWjV3XxdILDRwjWqmTRD5sJZv8DgBev71KS7scF15eh+shNs6Ntp72tDqKj3VbV/JVAyPHvYb9wYjtGONTGJilsW" +
            "OzvBV/Knq2nI6D1b0bqWEeXJA4GQ+IBR56SnDnmRtqRjpRs1mm7pQNMWaeeTBC0502nAEHz7cv3qv13Ms6baKmjs25le9h0AnIZb" +
            "tjc+GjMUfNSpQC+WO1W4dm5nmioUt//e/6iAoPZ9BFpeY+oSBMQgt3nGWJcM8SxeTBcvBIsq6sve43Xjhh/u+XTjI3Er9hCcltHt" +
            "0Ab/MHDxgS1HlojHH40nMy4gKF9ZSuGPvx6i+PDfCUBxEfckr4L7oCu2JK7DN7drX17v6xNV+8oj7IQzuv/lIr9xo4Z7ijRgIaEt" +
            "xEo3vFM+hj+qEPKvMriHAtAtRVYT53xb8vz7wbf0+2/HzaG/E7CG8iNBpIn+azA4DLburSIU24lA+4Q6u5+LYtQzAre4l8Yqj4Z8" +
            "G+5pMQAGoc7S+4aM+XNBMDwyu8in70mON36St8o8zvVhiiCFWWrMWETRuxuWDrS8cVuewT60BQD48/r48GE8D7dgvxL3sM/DLesB" +
            "aBTqCmOxAg8cRvPWT1+iECr0DT4P4ypdy2vK8bHpQNsT3Nn3FFcetRsZrvQryF+JjvG5h/bGJ+LW6xsY3+GSfI7mFw/+cs8LLI6h" +
            "6UBniyQAED/+kGBL0PocvQSAs7pz8liHylh/PAZ03uOs/JsDgCt0QPN6wxuuuIJafCO60lJaXgE6JaVkRW3zSwA6Hdjxv6A/6fY7" +
            "AWW/10gxCQDVH0aNFxw4CnkXInvHF+tM5hCY9/t+PAZ3csLc+IDpl+MqHRLW48vlMRtwleG5MReUXph03OLfDveFV9byawYCm6tY" +
            "OP4Xhn86EM7DGQrXx0k+GMTD0R+KU9/LfAouboHgA4foah26Gr+d+LLu3mCg1QOzvhgqELunXwTPztvWQa+Wr7/6DnoweDm9Nm5p" +
            "fcLA5p1ebkSGogwEPmNEb9wYhgs/UHnjBgBcUhmvwPKKPNI8xcp9ns6KfRyJ9HcC3J8JmC/M4+VHZFqe4j2xxe9EBILOJRHEr58M" +
            "KhCoPzOwsoCur40YK0vbA7h/y49gY/5PyzgOkUW44/2vRcNrQYfC8IzA9mKLM55pW94Ga15VyiTlcwlHmj5fM1lJj/R3AtzSgR7h" +
            "7Le0EFEgo0AB3fqKNwctP9vHo66fqw78qny149PVkerfp/fZAndZhDfNW8jfh9UCgMYGZ/tPV5wqJM8VB47rZ63/V5RPC4b9TsBy" +
            "0ESUNz4f96Cfjk8fgo/u36KdC+yXGx+NS4z1ekyuxxGBaYkMYSdebeMSgpPxjBDSdCC3k4AnbPHX+vHAje/FtN1Ctzbu+XQl1Hbz" +
            "PmANceuElyxImX+EoG+ImDjG/U3pKLh1qZUQTpMYDUW6du1erXTxPn/yuqKFI1ZY4lmCHs8gTQdyeibgCVsW1Ye8bfPGB0MTCJzv" +
            "ep/PAcDnz+UzFt3G1Pu1sFgg4EZ8ecHrQHX9Ul1b0PD0y1RHwWwfWnfMV5Fx8vxEH0t87e0nobJiIQkI4naj433JE7E9PxKhP41s" +
            "SDrQpYyNBh/XoRs1LDbnT8Gny+D0Xbe5JG98CVpzoJfBZRidd0LA1rp6AOAGXYd2p58tvn7eOPVLxVYMSQfyoNOPQd76Ir2TILJ4" +
            "Af5XxC22G2fi1r8TcAv9hhGuKtPxIPHlVfeEDlBpPalcs89EWfrCYDi4ue5vB8IYKpPsXIcrMAYh+bcS7gBgHM4R3/lHT7farIGv" +
            "HIeI/s3GhYV+v/HkHEwPBNCDrB8zrmYnq80jq86ThKzJvxqdh8fR0tlKsoTLMwExQPyl4sER4sMH459MWGXS3QHAeHjlcOuHQ8hN" +
            "vPF1uHx+txbSAjfbliV5yiuiRxxc3dld/TQr56oyzs8YfSb6nP/iId+Qp9ak+tks4/Qp5N6B0ioWX47kwOUk4FeEEJebz4uxMwB3" +
            "ALAOfMX9+bp7ow9fN73vKfFR+Lgd7BsfAdNDtppTAC8MtH8uQcDPYr8T8GKmY0RwVaJ3S3UY47asy+EekrVwj8diWNqgroVRujtr" +
            "Tsz0nZZop9rwbY3OAhmMEs8BZN/fqUJXHrX0wWCnV4Sug+5nlKnq6Bp9snPS4eaZK8K9cGfgjoP1Q3QLdAaubLxTLJ6xoof23XLF" +
            "O/7orx8Lb8eDEHV9FevTNnrd7F89cepGRPeaCDnxo2MiEO06MfLtaDEXQiCAr10xI8P9dwJ+RQj8K5fmwSsA4I4q+fywtMbEJdnx" +
            "sENzs+jZu7t3vJCjbVi+SULzsIBZGo5ZqaZD0PRyabQNM/mZgVN0akSjzHLFB5fy+LTbvSMQsLWYt6t2zDSERgQC5gqdjHyD8bPg" +
            "vYOPPjaR2U8DTpCxt5d57XQgS6u1sm6D+bkzT3FIYqp748aNL8YF497bjo2A8UlGw/1mOBE2k7kV7Bwo5U6+NegdDPQEFrNA7dm6" +
            "PRjsQacJ0kJC3bMuPIvm8/VEoD0c2/Mdfdr9bFzQG7oApujbByj1Ol1Yh5MdlbSAT8bc/tI28FTLeFYgcOFW58JHO7S/t2C9Pgr2" +
            "XsscuqQDPavNDIZ0HJy+PxX/rWHBZK+znH+RjiE5ufXItx/ntcxjVPsfky3ehGkz84PW2d7Z4ScKm+5O0fAtF59r7IPzIedaEn1e" +
            "16lzvEEYJjl2Cf2DjJIKDS/xxPljxMMktc3Oa0hZlzjkchIQV3g7kDcHdwAwDeN5WyWGTzFjyqw86mNwBwB+0B5vjxHFOs5gKD40" +
            "MHBxfZnHft0uni7KWQzcuUTjQIgqMNelAGA9iVNPyMhwCQLCmelAGebGItaDhXXhpNaLaEGOJZm6ceNGFw6rK+7SdyKzHj1tXH+R" +
            "OAWfZr3t/fk0CayN8gTgwvJX2hyXIGAVvIZL2XNUDA+1PZ4aizm75av01guf1h8tYvLvezB1tL9ItJJcz51hYwMBVQAQjWUujLP0" +
            "YO2dVztMfQhgd0Q/cSn3gHZZ3J7+XSEAmLCMuzwTEAOs8IZQADgCgZbDCTx3VujTCjysA0uy5C25b8FpI939m/LXwbpdzF8xGYW0" +
            "8uY+1MyNJhD4EJylB+vqXxvMaf9Bl9+9Kk7lnrIJPW+LDOhfrW0NzVrd1joKGh+WDpSi0nuBY+5UoKGVLiwo1BNhCc9uyX0L7pG+" +
            "oTkRGBYA3LgxDde0dqtlVVwCvXbHUN8nHSjAE30/B5rXhaoS+fMOnBXjnLL+nDR26xiIe9X/DqyjcV+LiUPQNKtXMQXLqOoyjIzB" +
            "CePtJ9F1x+b8Zyfrv0Oh3WJcLVOkFX/89WC/Px3oD4AYHmE/hw25wGcLSjoOtmvh6xCr/WGeNVYRNf8eg0XRUIphnQPPyT9BemMy" +
            "nMb2VpF+TJz0plld5QkXWE2nqA54Gfg1rPQQnJDi1y/R9Q3RGRoTO1qujclUn+oNTxn+/ttBzu/B4D0/Cu+iz0MsPpyNcyen6fmg" +
            "0U92KemvY86WUaIbbnB8Ym4dRb0+Jj7IqJrVYiHuST0HezE0AJCuW3Ervzfap8A9FhQ0AQA+BbCkgdfgPSojvRGf3wl4LugxaVKD" +
            "HEitCvMbCBbCOuxcceRv3LjRjHvKK7GOlR6CWw8uDjkMKLQ3MNcb4DIzBk+vNB3I58Hgx8UsQoVb1csezngN19UNU5e8LK/g0Arq" +
            "LIFefSBv3FgX3/0QsJdt7G1nVdoGnPDK12s5Uoug4Y042jdvUs8AaF4ItIgG50gy29N0IJdXhLIoHxE4F8YZVrMBRPbTeFRyFpc3" +
            "Ih3PDNim1kqKl+IcvrRvMttyIQNxbyUsr+c3ToVZP1ZQcnM+eivTPYnvswSl3sMdi5MeFl5BHS8By34fpzrpolfklIfz1hrvR0sD" +
            "TcnnJOD5+p2A7F84yUmuoZGfmPxbok+er8E7G5dl3Io1A4CtzIjcyBFYjZ8bF8dqHlf/gwueDTm3eUOD28YpoDn0Up4K7Aj4ygI6" +
            "76UMzDtuxqQDTfqltU+fKJ/evwwf39l1A4CZdG7c0GHiK4PWJLZAOyeh+mD2t+LT+t7RH6Fql5Q+RsTkLvGYk4C/AEBMiYfig1tQ" +
            "QFERKRO5YrFxkLX5YJ4wv6VuOpwZCOTHk3HGjpsP1pHhKjh9wtxQoWWcovjVp5khRHkyJClv+os+P/VlgUAk/pUluJpXBM83JYvs" +
            "X5TvsS1xzw7gvwtCv5YTpxhEv3x/J+ANOvUnSbJS9ELjoEtpUmQuXTKjqLGW8u/ws79nBAIyIi8I+QIsq/E2lZnSiwBRbGeIPiz4" +
            "0Ll3BvE5ua89+dDtuPN8N4yUQkMgMASODU/pw6KaaQ0Epk1p34aK1aUpOA0rOSY5SKdMWbS1ndQ1eounv4Fz0d6F3QHeH3Bw/52A" +
            "kBDvomOgQTnnalSkqMmJnjHf/N5ycTHNb9SDEagFfqvaXTMGdaQmt/PkN29OnN/XGze+ANOmtF9Dfkv1omv8GQFAjVZ61OJ8CpA+" +
            "F7sUYvI/QncQ8HxAjEyuURNGSU/gUPvmlLVQ4erLjlNnYNoDs8tZkD6s/qDxjUVwK8aNXlwuEPA8Nbruut7PeTsFi9P+KSbKPR1I" +
            "dxJgOBA/++w8EF/T7bwJvNHiNDacHoN9sOM/aki4E6DI3BvW6AfAU27nJPT44GzTtjwWEdDQOX5jLKYNXq0hQZFH6XiM017MIvNR" +
            "vaC4091oEz4gc6iKOb8TsEsyterGQABDyn8f5QXidycOaGfolP2S1cyqBrVnQTRtdWNKJLGOKfPu7lVVm+N5nZE6GScGAvixuivq" +
            "1w1YIBA4IQDY6Z/8jMApAYAd1Al16/OiV4bLMwFFOpCYsN8hOimvYOCIjFTUaXbqS6CRp5SjfdFny2YQvjEY98glWEQYX2Q6Pw+n" +
            "pQatoDUr8ACwegDQm6q67DMABvg8GHzij6qp4Rh7fBxWsRcOqHXl8/PT8V7mDR63fObilveNyVjtGYGZU+DCzwi4o1EU1D7zp70k" +
            "xDcdyOMNQe+/7Ji1hmqQD6LVWfx4XDnRGnRz/NMmL49rGX9yzvd2QT2o1ob02tKiV+MzYTypN+ZDTwA+1r/xxZiVGrSi073xJPb/" +
            "w2YIl9PXEQgsOLJucAkCYoQYHuAqKWOWnZqIZUC9psalHjS5YCKs9KjIjetgn5seE2WYHusI9zTdUlcnMm8L1JAPPQFUcsZtD74c" +
            "o5XgEos7hw+dIenrPx0CgVXQEZSQ3fiedKAUBm57HxKd9NjCxQZgLG5ZXBiei+mwhVkmfIb+1duc5aVc2hsqcNuSD8KwN/IMouuJ" +
            "Baf/zLklsfVFc5wUg8+DwfIPqs5Dq8fd+YRoqxJ9kfLZYNSmy8nxcgxPwggrclIg8B049dF6Fj3P/ZzP/Y2hGHEY1oGp+iby6iAY" +
            "w1uBVsOszdpmJEx5S3XcK0JPQgjK5wm2MxVpxAcEAt4K5kmvRmv4lO5oYMmJizH91UMLG+GpD8jBlNSgs3WQa3+MqFsM57xBH5qO" +
            "1fpu4ROxXtLWiVis41OttZg7KzwYaGZOV2Fm3y1tjHwOAP+MgzoFNrwLxWZD89npQOpIzhryCWXO7vTMAMC7vRszsOiInbEIDz4R" +
            "WFTSADCCt5V7OxiLOZAafPFoXQqnjZP7Dr69/lkplJI7OIIn6nfcdO0kMg3N4/MF6UAStMJnRgQrSyCu37jBYvopQEPDCyjzUBYG" +
            "WakFxKaA92sOzuThJNT0Z/1VMMPFR+OGJyTdvZhep+h991vxncuAVPiX4tnp5GX6j78eZD7mJKAXqVPPdUa6Nxtn8TG03VWEe1lI" +
            "mttJYkE0seq8oF3F9IXk/14q02isJNqLOkK9L7a4MRenjoebjl9IqxS7uXuRhslk2Sym6UegBiYt22JRf//tIDrtmYDVchLTRwKo" +
            "TDjX3X7HxLd504vP69Xw0NzVC6Szt8M2er7PaBhNxYkPeQ3X8eZnBILwbX1sgUDbwe3s5END0dG6egFb1Hc+c94zHGeIVi2rd8Eq" +
            "j+4bC0PI6uD2/NQFF3LhQQDsoIcgZJKgrlvEGbK6MSVTKavCeelADvuTQyEe0yjuuT6D0MiLL/pbWmVs14FvAGCnaERB/MNGtPNH" +
            "R64sDfvpxeK9HamrF/Jh2nDu2M5uvaW9ap1BnThtZD5e5wVw6T7p18Txs6Tx2Ab0GARH/dtLfkU6EGYp88U1xzqOzvvpMPPfbwW+" +
            "2Y7QsElkPfk5cbRex5SIwrdrwX4ScLXeXo3fM7HWD7st016yZs4+CRhMdsnGhzZX8X/Um7hMWbZ+3/FcXz1cH313TweKEaLkWM//" +
            "PbojAzYeX8lSEX3O/P8076rFkZY0ZtYkU/JdsnMvpC7I5K96D5iptA8mtRTJj0Mwxt7UAoEFd0MSLvtkLr5bcAnIh+c6rNUjPfoO" +
            "tgbOxgWXkfY3LGJCTnTmkLUjZcScz92Zo+IBwe8TqwW7tcP7xurGREZALxzJrzycW5LalGcCzggA9m9J7tY2UPF9PcZ8wPHmf0hG" +
            "1qSomlc49QrF5k/eOANN8p8dMn9eALC1MV/947vdVSaeVwAg0V+jr14vqFunRwMxs4OrpYbfAYAfWCf15ABA4TyzTntyv1aX2+N1" +
            "VfkWYumOdlo30kvTh6UDKc5uiPsBXwv5vZrCLInLMXzjY3HSynapBdUd3937GwzudeGGJz5En9TdIFzMy4iAOQk4/XcCep6rqH23" +
            "tBNAcPiJgCA9JTjl+YGezrbSvWHD7YedLoPvHAIqUenDYc/XpMmkX1qU5zsV7rswcIzXmadR/CpfX6cXNYT9PwXXgSnDXD9DCi1t" +
            "uqQDhQABnjwXtSPWntQnXLe8z1MPwOcH7srBRRrJ65+2/rnlGhLN2W/qaMf0C3eP+C7Rsoznx0NUfnkApTHwk3HtvLGjpR4GezuX" +
            "iJZK9+sgVwzp8BQSijgrH15wvvNyrYX+Za8DxAjQ2su9R71CstYfJMp2smtYcE8OWFl0NjJCStVxIxrteyZEaogwpOoG6hWmZotF" +
            "9DfhYUsLbz0R6Cl29kzzfyaAkKRGED1mh6obsv/zEtiZyuol2/145z8EgICf901OAzxMp4ejYnlcII2Ea7xoTlO4smcr+qkgrUsQ" +
            "vvHXsYx9nTp6FjW1dGYAsNFwCJI116cHANv1Qk66RbdfvGsFACm2Z71MdcawoodzFNlHqmPOO2P4eurQvelSYhobxwetnDpfQ8/V" +
            "cDlSjn8rD0p/SQtPPW+hMyYdqLFHFiHaF2I9ddo9K6/Sd2644MuEukaQJAl90oAMFIQb6S/TzRwX6LwxNWiNubcCpoa3U5CN7fW7" +
            "w2JdHV5b6GaN53Y6B204zYBPOhDz1PFc+DLAU7OlcFjRsylkaZdLA7rxQi31yYTKKYCmzXnYNJg62jJoN1F0av+YQbLML026K79H" +
            "5gDN0VtlfHxlfiFjYTwSyPRi4vZu3NpLmh4GkbjpyIkFW/LEg4UITFzowNP0LjWcdFE0egvbWJAFPUWGCQvFr/9aaFTuj5q/I6fP" +
            "lFeEjsXhrNSORLaUnj2HHxd2WuC5FBtxEGP50a5QBvdG6ZhW6YyEhiVnFjTkxPEJ7JcLQfJKAjFxBDLvYisEABua5xczp4OG2sjV" +
            "gbkQiU/UVyXh64Hy/AQPqggEBoPipF03RzKsM8Rlf7gFdX40wM5TtdiE4Hpid3DQKBckYEq38NQpOno5bV3YUjm0DGC1ebud6Svo" +
            "s3Ja2onTWvVfK/db4ZMOxLx6aDxCLvTyY+Wi4b6lWgut7vnWQ6C17gKOwoksFEq/gDimwLSQNDxP1oNpzvaBeFYAICCXudXx+WBF" +
            "rjg/F3oUAAAgAElEQVQ2sxYy13aGBgA6lHNA4ukcfoeN7aTudNvR0wKAgTTNPKCPGpaIACAFfiFMERho6M44DWQWKp/fCaiugoNQ" +
            "azWZNWmcYj0V5Rx9a6e34NPS9hB4R/gfgu5F4lsSUAFgWP/WyIkyYYk5XcUyjCyP0SpoOW28Cq6hXWdslJ2EXgUapYCJGGfoODdq" +
            "2d5xYP1xOQAIxx8y5hqmMs071qTIXdKB0pOAaVNF1VAUNW0/sUFRGB5nMc+xAbgaldZrI20oPeS823Kutga8uC3ovBNQL7ZkNEIe" +
            "d1PuqveATBmAyjifqAT0yNTm6XdorQY9z2bV6FrL5nysO0aB8zKY0v2Yrcsh+xRTHkblarSA48FqG10nQbYjOyWT2HxygitYD1Ap" +
            "soFYB7tk6q/X7s8EUHpD6lKD3+rxPMz2TAD3kEhWFl3b+4EVxnAqQPXBIpv9lpAcqHmgcbsk5Z/ZH/5RcXI6hj8sGV9HhOsu156g" +
            "Z+cpAQCmOXQA1g0AMAtiatDZ6DGcQ5jIn6TwlFZrF158LDZuDKxc9g0rt0rNkVV4/x8nO7gstO1aFNtlEhCMaWhKQ2t7JI2kEdCG" +
            "6+4bKtijUn7YU4fgpRKdA8G8wGdIOlA18GnZuHZG9ou/nUzkt+XhDsk/K6RjKuWtogx5jGWgczUMDwDGNHQpfEPXVw8AUizGzoFl" +
            "GTsw7MTwBgCMUIEzlMphy3g2pm3S8JXNYsIpGj1gnByV74QyRyzNtSMQn8wkyMEY8mCw6PS7ThCjggXhXrXyu0WsPDjKG2gALIF+" +
            "BnqzmqUp3VsSSpn796eSa/bJEMN+fP/iEObPjUnw1KeCljyaq6ZXA8Dl59lMmzxOVItYA9djmAX6ZPEbtdvyVDHtqX2nAvVVj8Qn" +
            "dZUCf/z1YMf9dwLYjg6bgfwBznYcxO28iyd3yNk/8v/oMDC+z5UCcc8MYfDqAYd+8kolhWyjfoywL4jXNOVLU74dE4+jV8hNSBEI" +
            "QihNjmtHFTgzz8ZY0t3MUFeK7w0BucJoH83+CMTbhrUex6eY7YB68JzSysqODQQMjNiq9jRLgOYkElWESfu+V7XBJlhzWHL4pVMp" +
            "O+SpqxZY1UkU6/hAYGu66odpKuO+W2RBuHUtvdfEKu1StVt7iEm1xE///beDDZeTgI0tySyMB70biY9uiI38vAD3PWynAIdnksVm" +
            "6TWPo6omGk6Sbm7fQH8gdilw7Qx3XgZovDfPLvQUJwA9u+j4lI2q66mrFjp72bOcx5y2rh3jGZ80f1bdgW4aw7NArkK2qj3NqsDp" +
            "inyGXLXBJtj0NhD/fGai82wepX+uJwJz0CUK6cQg26jNL1sPFyzsaGKXVlQfv+en5rx0IIzTbW0F2YZewezm4DNdxCPu2VmRFr7p" +
            "GADMwFntrK6MFFZZLDIIx+5XkHkPjwv2p85Sx27zKPs2CKZX812gPwWmBQBOmBg0jmtmUJqRobKpnWl+yCLNYBtFfG/ZExy1lJ20" +
            "D7k3OzQdqJsW8Ice4mkPF/0QfIXkluW0aF9cYoTtNZB459+yAIU3qWoh1UXUmwpd7gRk6qYANaAcDIylZPbxmbzbUVWDVZyPQbKR" +
            "yJrmHSqcfRVkWJs2QSyoJTismqoytV/Blz0k1/SzLqvoqxLFGzlqqYGT91Dy9FK5ceua0hPQlty8rpAkyWO5V5/IXjXb92TFZtZz" +
            "FZ2aXDp0oFhzrJUHZIRFLV3nbesqT5W60rqR0cIfQvl927+NyXduT6MmLimI8DaPEi/UbJTP5Uqk6UBuvxPAuaUtCOJi9m4z+S7q" +
            "eZdxR94HoEBgvx7IH5E4tI8Ha+C7kuQqxYSGMueox6Bqm3R2wnBQpXb23GZxEL65ke1HyxhQdbVkQ35f51TQhYu6VmeZuyjsFumI" +
            "6tE7nKSoGP5jWsDatnZFbiTTC9IxiMQ9vHDUCA3G0ZxeMtzGjUdd2RzkiwG2qcwKki2DNgabCuzFmoeysSLunkhG0L8uMXGZmR0B" +
            "oXy/NqFsTXS5BJRT+P6eOv/w2rslnfbwLku9ztPTt7WglLDQasRl6dsUBfffCdjQLCRDuqo6ECDqcX8PJsJeYVOaEPJbO0EAQqvg" +
            "JXIpojE72VpPuF4kN9VxvxBxIYpW7wyw0rWcAiDaqqpnBQCTHY+i7VbPTKiriR3F+NbKkzUAoOapjpTbeJnIMIWLy4qNE0VRdftW" +
            "9KibRJNsR+rwCEYQeQ3Mat7hzGn0ui6WVwnKsa/WrTHQOeddgg2nU4AqqQ7bKbbdsu6RhDT3sQNUtiZuRhA3tX1nQ49I3NxcOBwc" +
            "BPQ12TR4J3moXDMcTIxaztHMk2WF9p25PdE//grh99/en12YROlAZ/g2KiXSaNlbmeL74zMC/I4Af0aAf3m+FCbEdPKFPdo0czZ6" +
            "NdY6BsnP2pGTq62JMWhokKyCgzkGUhF2U+dM596K5gE8dIYl6aH2SlZMt5hFqIV+C1TkWttUCnf6vB3UptoBw7sbpl+zs8HVzww5" +
            "xR49lQJiHW94602uaN4/qjHBTmaioml7XsODzUUXSzqsqWx3BFGwDjTy3aT7zXxVCrT0WeWrxMOXA3j5cb8jwBOO65Z5aY3F/azM" +
            "4bB00NyruqcDBcuvhVeJwdAVinF1yzJv5fn9BPjzCfD//bwWjgDvyDECBHjnQQU00QImlneKdR5Zjjq8EKIx2pa+eKTWxpbhYB3v" +
            "GejoQ3XnWFersbF+MtbFJz2oMg8Pk3Qq0XEJ1AHUspPGs5irDU035dfjZkPD6UDjXMLVsIOwXcAba55m2UJHGavraKFAwLJstfqU" +
            "GroyrVxhe9sNxYecFwMFsWKTP98MjXYaWuwIBNQaZQyeOG6oa7X6Jv+j2BXnWn1RFu+aTht2kvu94nYk+iJsPEUA+Nvbn0sDAY6V" +
            "UXNeakOGpzU84JoOJLGnYX0XCBuyJvebViTZEcc8PgHg530C8PgBeMZQngQA7IGARAu4JUfrpKd3rXpAePY0iZzH2o6udEu3IzcH" +
            "mn4AMOZNkLWqK4NPBQLRCM9XbjUDKtw2lnIHuwICLSpzj12YnHa4Wvux0ZR0DJvCrey2USGZQu5eTyDmOX0tzgopIoM3Q9sAavbo" +
            "mhD5aqDH0iFuWMpaigwbW/MaJ0DT+Qa9kLJ2KZ2pFZPIeY5BRise18x2l3Er1PwkBlUOE9AFoky1mQRx/0/C8craf3m+TgN+Yn4a" +
            "oGlLe48qV2vDth3BeW95KS18HgyOEDQLWQ2F8krhWEsgwDCCScVk6+snviJHAICf5+E4ZXlmFYXWLMQ6GeFtTG2NQAYCLIgIu2bU" +
            "xfKqGzq47EpGeioV3xUyFjeNOlGL++2LyaEHeOeXqyfKuTIIQwKARse9ZWwtTbb0Z1s3JT5FRmxTWl+1Y1yt0OoItQsoQvIJ0YUZ" +
            "QYBGZoH90ldWrJtg6LjWZM4E8qP2jLgNOVaUhsDYPNaGejU6GrlZnd4qXxX/pzbW+B63NnH32EAgJB/eDtuf75OAHzgCgDQQ0PgW" +
            "LUt7t8+S0YqAz1x63I310oEGoEVAWyDwE18pQQAAz7cu7YEAAHnE1bqo1PlMQn0j8K5vpYX6DnHl/sggwAORMfih+NBgHB0CAA2O" +
            "ZnJOtDu9rEGli4u0jLdyeMqLcSZ6mvN09DK6oaRvCgKMoHfDjXzHhjpG9NlIPa2zYNoAQrtJYvCaeoDEZ3G3m+HNHYrOd8S6ZqTO" +
            "X1ODwnwQ/VJn4Bdi7DxUvM/aRlNajm5Y/Fpe0wYAuOGWqKloOMJPBPiJAZ5oo/MQUxQddvzjs7iZSUu/CWlfnlD+ONiUkwCtovVC" +
            "HDyuLMPYFl0+AeA3AMR39LgPPNKgso+NqyXhGBi3/XPUtvCFywC6AwTrKYGVB8cqIlRS7lFkcmx51OcNf1cO3CK76yUFb7N2gFOQ" +
            "PWwcA2lH6kzjjW3nDF5ag/QzdKC5otd2qxOKphv1z7SzKtSfJoquyT4G3JpFsmAImhfsKomuuccxz9kxrs4w+8sRfjn/T3i98KVY" +
            "0xKnjvIlPbJd6qiPjPe89Xkm4AEiZzMVvrZRojmWySKnt6I8E6eptqteHIx0WIagIdAVyvP3uZ1DsXprEGAq2FWFRVWMDgFAL5kc" +
            "fO9VTh4KBDQnCMMcBy8D23pKdhaYwHBoQNp5ErDi7vrq0Mgs7P8py4oXdExMtZ8MVDwQDmbLCZZXWc3e2jBQO+YSpPluFiQDA0Oq" +
            "0ywA+ShFPC4koo5wVItvG4YDACyKUHzQoT3IHqdI0oGK1ytCP3Jd2BQlPF/R4wb2yOr9pflokSCu1r9awcbtC/Ob9CL50VzXodgY" +
            "OI7PCOdT48jrC6OiowRfWYhGBwEOVd0aNQUDPQw3ztOhevDBIEVGjb1StkSmQxMjlwgCUP9MGxemhvTli1uOgqzO+da20jmPPdUe" +
            "/i08BZDT0qTBxe2QHjvQJw9cWYDqCYCEM1KBOlSAZBGnBzUhVt6T1zU/JCl7pQWQqJ0R8gSLSFsLouyU9ZbhMQr3avDkO6J/p8F5" +
            "fLz7YqanHNuhMlfwoG6/0wqv4NtiHkafvDhvkN4goAkA2HI1mladT3dEjVVraKGnCgCYOmfo44w1iKXv5GVq9VGFBv9G3b+ahx2S" +
            "f0U9phV5B1dRnr8/Sx81SR2Mnu6X/vjrwfmUB4O7dVdzfNS64y7WS6Ib6ghOiC6zHbPWI96EVuvR1E6Dqof552RRkRUZ9GjKcu0o" +
            "KkxdAJiUjQ3WPql3sqxjbT3G5toQ9GCm3Hvl1DJVRvfPkrXRfqyshMBMLD4o6kmrzjehY8GrySubE72OW4u96CdZJ6JAZL/Y6ElL" +
            "dLFbLtFpFAC3rg+ZN8bNzBTazXttIc1GaeDuaXyVrRWN72i9l5Sxj5OgcQMMvoaUy0nAaFDBHllIQ0tFrCRJVumYVBSkAevZlWdh" +
            "PVmx7F557Zq/x+u004CeSF86YWms617HQGe2QzdMTifiCjvvnieZX4fBMmg+AaBQoTFVV1v7U100dbfFE7iBc2DIuu6MoWPbeJKV" +
            "nQKIJwL+PIUAxXMN7cGAvf0OkJR90oECIu7YCS9bZ/D97cQnoXUymnZRW3djHMt61l0VY4/OGtqs8LCEQzqx7o1+fJ38J60FXyfX" +
            "AaBk6La+Lu7UT4XnqdikDQnJme1wl9wgtVk5HSRV3PXB4AhlhDQPfg3j0ybVKYR0PGWwLlnqD6ZjI9UNNndQe4xsKVuDglZrRpil" +
            "AG6DZauSQrTRSusbVUWEyBe6sbpTYclI0dSdBSlrhj3qJsrWiPdYPveNBU8ltrR7NtycDh165sQonDI26tQQHdz0WlueSSk5eyxT" +
            "NIvYIVPCYis9EZL/NVnd6T152Ddptnbo4CzjS9S38ubffkP49f7s8zsBAcL2Vqae7rVBbq1P1MYKtZmiNCJbNHeWIaiyKfRHHb3H" +
            "jvQap9zjWsClpSHVrRmQ7EjaccCrJw1RUW5haAzz2dDkG6v4V6RtWOUw9ITFaO/c2h2FCSk+Hk20zAnTZteqmMW4Va8ddsFXGZMm" +
            "f8iDWPCThab+MbSB+GSgV3XKW1BJuCnaJC8AAMBf/4D4++f12e+ZgITSPGM9JgBohkPku8ys16AnZWSWw2ut28OXUFftOM0CswN1" +
            "YwIGHGvPGssrzvErYIX+rcDDJXClNfoMeAUAjtA2GwAgQHj/lcowtKWKXbD0ALiUnHHpQDFCPCcNKNlDUaadxLh9eP2Lta2QSH7U" +
            "NWa5h3YZBp+ql8ApIo0J/2aeW04EiLZbaWRy7hG6UFfcQBqxa2qkZf4tCBPxQXTDW+SLey71zRlLZZeiAMayuEJP3R5MH+pBJxpc" +
            "U+q0SOALN+kBU2GavGcudoPs7OJmaA5q88Vq+xJapqpEYVXqjIZ/eK05YSvX67/Mxe7huqcDhfAMVQ+6Fywp+2HqFgxgB8Ls+OJm" +
            "cX2LcV1BkzijpuEtrdvhkXTt6lsqB4CQTuDOAID9AZRQBpqZ2owad4tMhQKSrbSiy2GvzbUFMSsAaMW0QEBTWJMvZ233Ipi+4YMQ" +
            "nexvE87ovFd7nRtQNRQbVBIfyvnTjRofTnTxelrYUa464ZSnxTffJJA3j2s1H23MmG8Nl9S9p0maDuQSBByg2bS76VWS+hbSg4LN" +
            "8d9oGqO4omzN6e8YNWvA7EZXKqBdkJVBQ6t4YvGhjQgX6JhPtRR9kYyU2/GhUZnVU6ujGTdQfMhTvlrf0BRPV7hZtRfAWC7jCUBG" +
            "uleXDE63pn8Ayu2hQUrVFXMZx6AJhrS8ffdRaFikVWmoKUivdF5S5aI/mJ8GdoagJjdD2R3Y6e2g2yNH895KbY5rggSFQYhMOa6v" +
            "2bRAc0S6R0UM4jxIeMLBc0BF2jdZ6FnjcDBJVr14OlDGBdRmVuL7HwOojFDMBlIICDyMW69CTAkGjIz0irh3J2KXaQOdIi9wA7Fb" +
            "VMyVEaudRa7U1gnB9357u2/cnojpXMOf07Yp3ioLB7WhwzPC8KGopqKpgIlfx7o9sAQEFh5n9cHCozqoV5S1glNNbpoym5kFjYjq" +
            "ZTghCEtNyNmnIRosw5+0+Hc6Bua0tKQ5NxdQESAVNyNRlslPCdS9gOYO0ygrm1Bj1A+qMaIL7BwOSAca1P0oG4j8eijubZVDAHgE" +
            "gF+/3jQjADzedZEDkhvWAFHyhhT8F19dLckm+JxodxMKAp652Jbdq9GGWLWjoamvoCMZmyZYZGXZ3WKgYr1j+lgbFvkZoTidY2fR" +
            "lZE0Mhh0aLQjaSKnsVnou1lUtbnsSK9atHWcpR3j1vHzspUIyzjdkswMZQuE7I+tHrMBQt3fIO21cCC7Y3HSpfrKCUleVpblNqfZ" +
            "TTsI7CkExq9fAOEBRUAx5jm1hjCr5CMCADwjhP/MOx0oRoj7MQsXQW2RmqYftYUoUfIQACLKC8i+beUeAI8YXj2OtENOO4CJmx35" +
            "YWDHnbnRoyglDzRXXbv9CjRXtxjVChk2F78Hxrmm2qngLg0MAqq3GhwngAaWPcdI0ThbROKjdRwcxs9FBby3YQwnSSODATWpGUEA" +
            "U2kqnVbHMQUXmIv2QzsSes4aXAEzRp+axeJDvQZt/9tGtK1W5OuGtNSkre0KWBHh75t/VpsjzP2I7mPkQxzg8SvC41HzaT217nCy" +
            "1Qc+EUS/FcD9mQCgA5Z0QYkVodV6FdBfCOTT4Gnx7SQAhAGjd1p4wuIgUBd7dkTFSjyP1sCkFS7kKjsXNRSBHzXOVKDqcY7ZuaAP" +
            "Q8PAeOindYdvjaUmR20B6SZ+BSEkqDk7WjuoXrwYUml90+5bzGmw0742DtZd0J6glSjkriYRjQUrx0NpeVHzm3E96t5ik3pcjCZg" +
            "ORII+/94G98+qj16ECgDpDhq8txDYXVOs5bWfLiWuZIFPzK24OjxCBAe8eVTTrXfh/USXN/8fmL8fqEyfulAKScejpWu5fwjo9fb" +
            "PHs8IM9LZsqVV2NRFsOy606dQqiR1aN5VPNUpY+aMVZrhmEX0mVX2ktXO42PK4asdAu0NRqhc0hGjKfndmgPCwNOmnpQDUyM7dM7" +
            "tJrCleIVWurh6dXNFB3r0RWm+6ggoOfUy6IjVKDKBcLp32o7Rj5SjFrjzQj558Dcoi/oUfU9Qu0UQI/sTYXWulBucHD3qR8G80sH" +
            "Kq/RO/ShvlHrhaydkH8GKHnIg4BIR8wMNDtCe0qUqgICMcNjMf11PFGL425YGpXa0/dJeQgAasWYvvveuss3I0CWtuEIdM09ze4N" +
            "0+biG+AA4Luw9DJwlgOW2izKafTWc40Dj+05rhPQHFBtKCg9qF5bYxGNxJeWTtH3xJOsjS1DgSmzzozmfIweDovVNvXIFe3TEyff" +
            "2eXqc9/xX67O4UJomC0l1So3UnMEuZEQOlf038sxTz7nbL5kEwJxChADUDnKYjebd2uPhg9xIr819beZdsacBGwMCU7vrMUsvtsO" +
            "myTwbooUEDhzGULHKQDh678u2YltiyNlEDdlIQ2dlj4c9FuQNUcwGdEnE3viSYDdelC6pK47eL2Mh9WvYgQrbBC63Y9yuRrcZqeT" +
            "s9ZXiQE55xvgxdPbaWzdLFC3UaFPphNIgS+2+xIs86VBDu4BgJLfdN7F9Mb2jRpbs7KtcU5QiuRwsmUO5aPiWHxjGzTqm28GPukz" +
            "x4R/Saf2e03eBXEl0vNV4KPWQr5RGNj5XSNviUFwILDZqDJ+Sl1yPeyi2OijYICYvxFfT+DzTMCTbwCExkeAdUCTAVMHARW+VQsy" +
            "tYi3ruRKQ6ONOtnuBbRoMJ9rfPQGAwXhzGmMyCAb6ZGV9RSbnX+LM9KJ1JGa9su6iv4la0xvUx5kqvT1N+YwMbN5AHpDgNws8ILC" +
            "Cc3GHvG0fdmrNuw2tox9cUs5UNbgRDu/qLSA7QN2ZNM0WSxHTGu2/vmA3m3f7uBP9bqCUjHLSFVu2+JK2FB2uUoJc5s+yfWajuJ1" +
            "rcUxPVbohJRGgVo2rfYA6j0+A5XTSnrGkqsLcmROfNKBAsQQ+VB23CFhQlkh8cLRr0TDGp5VE/uN7RRAE2CQiORHG0+BrpvZEeIE" +
            "JxBla+0b5nQd2VCXJwCmNlKF7GRu5WAgG8vJK7fq4f9Onlz1SwGrs+YC5ZyvooOnwieXnApPMAsH9icwT6n9knbAOJjmSocM3Jx/" +
            "hgdyw4YLqgIhx+Q7t/lzLdAK9eqbY++sAQBayLixrF7k2sVjq+HRe2JrdmyM866rflKl6g8ZkGvYrFmD9DqyzZIScksHYvPMEx9d" +
            "HKNYfiXLFzdidjZkEXntTUVnGT3PdrW0yF2zRkMwln+auskhTHZcOikdpRsMmPZEpQU77TNW79pmQANPKzggqs0Mrb0jaLP0eju/" +
            "wDxuakfRd3xbKq6dbxwNbhdVIyepHa2DSOqUgQeq7Som2o85zVBSXMG6HGjVpyotcd3r21SRec4j8yIgYWq03GzZcKZUvEcjanXF" +
            "+7HdB9h8/xgBnu8v8fUfALD7v+MeDN5aLHaRAMR3u0f0WfTT6iWr2I5BMZ+TN06ztk+h4+D8k2gMqWVXX66nDgS0lBQELQ5iNx0F" +
            "DWu7oxGTv+nmgKYS2x+VTqldOhWy8akspppFVdWFVkesY9xn7V9VIYx1XwAQ9/9zMfFCO3TXrlPSvGbvMacg6vnDtGXGhEBgrhu+" +
            "ltPfguZ1YsAapJNmyMsGAClH1TIn2LrShGPIjtSMKu1kvRP7LtjCGPN/RwCwfSgrU28McsW86dbR0iI24TQ2ZjTc7YjqmfTrTkdY" +
            "3tfqKXVHoXmXhdq6CMxnVctNXNA1Z+vFxPZW1CEMjwBAWzrDkF8ntAXup4zPQP27gr6tiMvLrXJ83jMnVnjLoQlR/Cque5QtjM/3" +
            "v5hdD0Ak7XudBOC3se0gT6CEnYX0lm1A6qXF3ZiYR4/7dwWaJuOAGdzKB3mwQsH/nLBaXs1bApeNK+PWQNamYQcCl2ebVfIxbWEQ" +
            "dEEjf+2uSPGVqzir4wq9oOxfUZ8pX6Otuu+0rSVWH0lbUYkMAqKmbM3JF4r0vMxbgWwHU2qn08AVsspuCJdXWbNOhxPXHfN09M41" +
            "pqs/zbQ49sd84ny+bU5oXJIsvftNmowzlLZ7BLQ2VZziSGbblwgAPz8Av38i/PwO8HwC/Hq8/j0eAI9An714PROQph5pahCjHdO7" +
            "dB0HUDymEyok2jhEISoWlb4aagUMtCoVOnMXUkeYJUUpcY2uprCmTGtkR9TjAl+TBYV8c9FD52YsrNqUvqKesZ1jutBRefdbjzQe" +
            "eDbQlV8PqdkNI7+sw2Zpw2E3QxuoxFqBHQ32vGI3Cl2IdW7oseSt8EwngU0JsDBBeFWcrRLnUoPNXh6mvNGOANC6KSTUqzr/HgNE" +
            "6Fi6rqd85E1K+ZERlaVtG6X/CrU8yr59mOw7EMFA1X6Nh9amFsXQYMS3IF9BQIDfvwH+/DPC8wnwl18A8Mfr140BAB4Q4AHPjJ7P" +
            "K0JJHDlgAegFPIt8khAuH9hAFRdpqZEYtr31TYFaFmttSXfv3MoDUXer7OXBtdpMqZ6CZnWNjEc5FRR5eEVxpcHZKmh2OSg6py3I" +
            "ygDZuAkjFIyyM+w2V8s7W+pqaoWiFOFKi1pAtsbIIHc6KQUkuAB1MqviCeoB13FfG7G3bexIAXeNh8JpEIkfCAH1nzi9q6EuldTT" +
            "JrwtVET1MgJBl4omK96Wi73p2cvzMnho8PGcOHQk//kjTfxFZj5oZKeY2M1BqHVNt0XMAED+TBRJMJ2erM6FoyyWo9RCSL7g+VF7" +
            "CUzG02TgNkkxJxdiYh8iuv7zE+HP3wH+5U+A59vXfzxe9UOgZ193EPDnnwCPPyCECMlIhCwCC5DvHGJOuFNWfE01QJZRpHhibK9H" +
            "c/2VeK66AgAtAYVQmHWyWpa+kFyu8acVgHRCoairuKS8WZatnZwoL8+BcZ7gsps2U1q9ObtxL1nC5G9y9St1A/ulglrZQH8teFL2" +
            "TS0CTseU+pXyxwcw0qhRLbR7hawO1HhUAq8J6Y0eX5bGwS3bLrpn3shImtnHMpZyJOXmIVMPoXkYvSDLWNtmFlBKjrMBNb6M+3Do" +
            "QydBbPONC3wWd276JAQBts3Y928FMDy6y81KlCluURuU379/iNu9CPDn75dP/udvgOfPKxXo+et1D+9jbOgPAn4DwL9ATHcH859x" +
            "ej2JwA5KxhUXChik3WEkilYUhtbU3B7BtTA5JgAAYDSjBoadpiBAaj9u+qQhpIP+dZ6NqxbaJVTZYKkph4WP2m0gYehysJSPut3m" +
            "tLzwtQ2ZEW0YW8vuL+PosxeIDmrnpUU22mNx6laxCAlRvHpjoRP08iF4rC3DTgQB5ddeDze1FIFuFxUP+VcVeSyv+HYNqqIT5iPm" +
            "nLtXnRM1UE4QlHaF4kFquFoedb41BZH9LQax7fymSmaMnFTtVHTOViEB0ivNhoJWP4rpie20VdFMBlVToAzwNU0UZXd5hf37HgBA" +
            "gBhfpwB//gb4/ft17xlf93F7f/yG8Hv7rOBFxH/6ZwDNVMYAABYhSURBVHilyb5/NThdBrD7nsvLsgxH4Vu1uAjSvpuURlc4Z0nt" +
            "GoqtdK+x4iS0UhfkwHSXbSHjqzUA4Pk57hjMD0vOydNhDXzoakKs2rqj2VBPbfyKyLBy1NwgG17vbNtvqgCuVcYApdOW3zJEdjxU" +
            "WxLYTpB85Rf1w9Ku3OImQo2sZYoTXkavy19rqLYOpfdVvKCBzpywt95T8szCOlKm1l2DDtQiE23Darkx9DlRMMUtv0VU87NrPG+a" +
            "2Tyr6g2oim2IW2khCHh9LDmm2pDbjfu8yMqFYmbxvCoEp7L7dA26nthw4q0Q8zf9G2OAn+fr4eDnD0AIAZ7Pl2RjgO1xNgAA+P0H" +
            "RPh5fe4OAv75nwHCr4QZYsEouqR5zZpqdWJgrIcVRvjKNHY4Knpnnesg6wrKNGuMCjIpdbBF8Cjk4/gxOAqx+alPTQCwfSvNsH4M" +
            "nQIAAnmg0rZ6VrkjjCXPh/YGU6Sy65PBlFJiAOYhlPdY1GQj3W91fmoyo5wRb++UcRzz72RIINIaCiF42lELztKoNXN8jF5QA9Tr" +
            "kSXAxKYOyQjnFkNe9EWgsMcdAUDkb9VwVH3zJBrsY7yKYrUgi26UdWRVdBhExVhK9iQQNyTWVLrvhEh9Tm1b3BrNQ4Ei9uYaQLIL" +
            "x5Az3PR3sMm214TOzC9SZu8PL+c/+bv54DHA4wHwjGH3o+JLzeAHcsffJR0o/sBzt5mVIAAAbAY4gWkN0RQW+KCi96ojjsqSyi+W" +
            "Kl1UExSON3mr4pjr+SAmGONoSbJ78TTWY8h3BiJz78As/yVFSyBg5rMSCPQsFvttQ+BnKmvFqCCg5heaNxbeLEmy6JuoahT2oQgC" +
            "DOHxzElk9IBqAV16b6D/r2uHuKeep2j8svVaCPhGQyNT0adQNuLhs22yse6bkf4D04hl7Kl7FhGR9ByUHPOQ6hTr2CpY0fQ/MAUP" +
            "t7q82eyzWmWFaZGdT1Z/aSMGBQJZcF3KNsADAJ7O6UDvRgJmLGM6QXjPHvHHHGpOq/ZGbaGMtDrsb4IIeTXOkafit4jK80Z1K+2w" +
            "w0R1RmENsDGLSW/3T618bTyZxjQy162NlqCN5PYeiEDcozgDfZDSKLdSZIMCAUz2/V21mEQ6WJb4qNF74ZgTbn4IQajVqcCy4WTF" +
            "LUSakYzob/aZ41dB2KKO5CIO+RxQzRWKmIBeE7gTqdj/phgwRohI6bVjaUEg1khimrJt1HYqSb2CUsesvPcuE/gaWbaynpAIxUqr" +
            "4Am/H6icA1o2SJYNtjO1JVIfuDnL8cSWsQiqAk7X8kCK5zS9I07r95zZyxDrU96eoYMR8SFNRiOthCpLjHL+t0tpILBRkfALnNOB" +
            "AADCE/YHAFJmqemzGTYqI4h0NqP4lYZ2ZZJmUqFt+pC9XCxrfCdEuIK13atafQbUTtDh/hcXFQyUDAViYKUpzxqySt/IFgLdbqlm" +
            "xxWqW9j5V4tZKGiNg/GrdtlFUsPce1UJW/nUOUockKqDwwkUeIONDZrAYFKhoEBHIIpVb2tfIydJF7hgYPvCOmaYTWY1Jh0NPB1R" +
            "47VMS8rGivrO8EKl6mnnaQ1idUYvybYrymsompdFLx83dVdp33cnJrknOf6FPBS2Weec2aA2PcTFWt3iFa2GtRIgWda1aym81wXG" +
            "YFlkVNiCQF9P7+H6qqAPlxGY3G8xgTG7WCrB6hf+LmwmsPMc2YGdVbR2STb+9fNWwdw/as1s94+OAIarlPlmyP4fAcB2I+S6Eg/6" +
            "D0YcLkHA8/lW3KSF7DQgWZlCGixQqBggceLhuskX0y/HoYBM3HjAyoBvc4s21y5igWunZTOEQqpM6UVBJ1meMPIJWikMpfufsaRw" +
            "pski6AXBouMm0XnfUC+WFQNgGb+MZ0KMTQv4WywRfU/bCcI81dg3FyejKIiMjP5VTzmVIPMh6RSnQ/v3ihNCEts+YyMP+Rhl9FL7" +
            "VrGrxeIe2K/ygg2vedpqvyVUR1Iz1JVx5chY9PwYj5j8vk0dlgxHPOfTuVnj3zIlMv2C+thL0DZbXwkYHvDYUka0th5Z5ibTDiUT" +
            "k5wqAU82lug6H6wQc9i4FuC2VQ3ryJOfX99lDiU7k8qRmi862/4OBJRM4DXT5B8hhkS/BdGJScF0bLPrkWfnrcLhQdzzOQl4QEgZ" +
            "Sfjav8SNk+ImyJrNf+XvEQWlo5LUqKRR3pFTFci6e58ExtJBkviTDE0Q7nfMzcPRSDwOrODUwhXSD5G8Y+JR7SwychYnO7IGrOOm" +
            "ohnVPNUMhGX8srKEo1iU1Vj+AO/dg3AsADEfW8moRIDySFTgXS03gUjcGNsuvMe24LEyv2JarMJ3de4lc4SUI+KhiFsEjyJSZRg5" +
            "pikSko2j+kOVK76jhYi0sRwRJcQ5oXDEtDyk7WTT9G0qqF/wpnkK2bpCrfnqYJ9Z6wodI+gF9KU2b7Ezsd+j1oE6q5ktTfmr+UcZ" +
            "AYWQ2Pma+hfSeDDtqJwxanyIuVjrhqqrm/+BylbrJsZFvQ68CYsBakVuLDvU50z3ji8WkyHNifRe1a3cB55YrDn7mDQUhHIapWJd" +
            "BEbXyPuMPY4Q4XUqELPnBZ5PABwI+PxY2OMQIzk5EoEUA/C+ro6KuDLUAkEsiuJbABNjchwrBZk3xWKq2gGSjBPScu3uANmEoGDF" +
            "WyGSC6TfgldQhC100uRwxr1G+c5qijcKhSPPbAdQC1WhF8yibOJJGBxSp4iyhdwV+qa5H1IBoLmTnurt3ymaSstdOBscU2ps3raB" +
            "DDeeRH2NzAMc85JaeEi5MQabvIT7FuXxJ+W7MQpI32I5bUnfihASdY2qX3UqhEs1OysFOekF7fpc8C6U3a8zmwsSLY2ekirD2JFi" +
            "I6OyQZAS3flNy6cyE8YW30k3yWr9LbphCEhznuRJFItygU1zxGtBSTMWZfEtjdxS/4hEwgD+AdVCTIjZgzbNK4sAcgqhZXwYkHJ6" +
            "f2lZCzK7C/n4pfdI/WMNDaFR6EJmM2MQx4OtKxerFsCb7Ec/QzGOcRNwhP2tQSEC/Dyc3w70ZiykkVnEvFOzDBl1k0LV7gsLPjc4" +
            "2JByhs0CVvkryHgiJileE5g1ouQFfSkcjP2ikJefcchjl9v2QSGATZlfX2JyjeCZ4CSflGUAQBqN7APJUPG9xhNJg3LGCFRsiRlc" +
            "/QiprkMWXFpOBLYyVKM1OfELIl7+xkEz7ClwYJ4tPCGfi1q5sXIqPA7I52oNxIJYBCm1BX7jKeIbPD+1hZ2yVxqZa8ryOlWpj2UR" +
            "aLlt34r8dCVqOkE6Lvg+EiAVhJIEYi6fmF6D0sHQOn+bO1QUFxxu/gLdoGrdFxAgZmNJDZ+Wt2JaxvKzlceACuzrEjD8Ih1g7Xyo" +
            "qKckW4XzU/U7FLbfusbht96kNjm9Rz5b0bKgbjTRTspr3gm7KGh88BhbeYi4x4lNDvnX/e/2D+Lrh8MCoQ8uQUDacgRi4CXPh4ho" +
            "KboGFkx1d7Gmvuf2YzCRXrAszWgWJZEn7KS1KhFuAxucSN/DTWZv0a84EaSTUTGsx+0Axa4GUZdUKeYEAI9lzREq2w2FcNQP4TKG" +
            "K/ui1DcXbN4rMzdjTd/Eld2mUxwNCo2+V5WWpU6247otPIkTlo2hZN8UcsOf1fYE6Vzh/GvpABABAJoHWg8tHPxb9LzJBos2jPhS" +
            "eBSE3LjBNOou2x/ONqJoJFubtsBTmqeCDpH6xOhfLIgdRSSZttg0i33QiD8iuQHgsa0T56bl/l2wdxw0m0IUv1UHvWdNMeq1CpK5" +
            "aPCPsuWHmBMiXUoYKiXK60ZAu++W3QoNlDJL15z0L0RAvx3wrvCTk3IIAv4CMfzEcmsiAbJQeIEM+F6h4IqQFPLOW5DyE95Wdovy" +
            "LAsV/WXjya4RuzKnfARCXrgezYZo/PejI4LfmFDNol+CH/aagHK3gOZFkvHRdsInYiokN/CiRWpYIUAkkypDIb9DtU3U1ciO1Sby" +
            "BnFxk9MmlGTMSN03jGnqlGy7F9TxNkQ05vsfrqVA7u60OhjUAltFql/beGayZOwZ007uwMVijm5vokkDDNZZLHjd/uQ8B4IvTCBz" +
            "eOLxN0DMeYFYjm0xn2lIcqoCEafnbyzuk87q/p23ayH5hNcvLZuBu0FdZwx7xlOqb7X5mQhoH6+I5MZ6uccKEIXXhmnWAYlHLKt8" +
            "bvB1LGqD7YfIX+SH6SWLjYOEme2+UBcTolYsapxJHoOunapuVO4FfKFaPV+zuT0D7RhL7VG+ETknIjS/mh43SEpBuTbV9HX3xd4F" +
            "N7u7/X/QQN/TOQ5w7PrH17MA8f3G22f4BfBPP7v33x8E/AUgxkcQX2r8tjbUYAHwiyf/1DblZRPNZo4FrnI0ugl0V5yQfN7uMqzk" +
            "DizP1zEJSGZIC7rLZdfsYwHC/EmI2IneFwHs1Eb0OWT1Nudw/xZSPtLLvNeDncOjvdRgJF5HxnftXQK4wUReBUv8uNK+PR5bZhyB" +
            "edtAQHKkEKrvKcgfcGcXmrjfoxaYo71Q6NYWFOS6x1DZLqBpFnD7MSaf87lyXEuppx0TdEm+jXg8dDllm7QX3IKcyeJQ+pDIMbvO" +
            "dSF18rdPu7zi7lykc2VzKl5/qVFnLEEStOxje1wu6hRzMeVj5wGPZzJ6G3+xUI9MCCm3dQuWlin7Ts/Xdy12bSAuhPxvtiEk8YkV" +
            "MeEjzRwuAgKD05F9SXlKx7GyaRXL3ZZjwN80cErRq0rAFTBT+RXEE9kNAiFnhyhfCiwNoBluDl1N1vq0GNlOwsPG0+szXgc2inkA" +
            "L+p/xkI5ZtkKJtgS4dLRLjPPsYWTeExHkLVpAg/Fs1LJXXoZzfVUozfsnKDKgjD1KnOSHodSGOWUpe1D8SW+ysY0inw/zJLZXGJs" +
            "jv9fdR9b1RggPgF+wotoCAD/BAB/BYB/D15vB4JHTJ3qY3HMenaUR/eTZTXrWO1VWqSDll0j1OB9+TWhQ8LHtji+FvR8YZdB2yFC" +
            "wRMrQdZJNWxT6k2B9u+pnDTL5zGpUiOVLwjxKBfzcjkypjKH8VgH5Z2ybBImTv3m+MT8Qs7TVpMabrJB7PAIxp8SJbFmkp63cMpD" +
            "OUC1ZiXsAR2xSB0tEj99RntikAYCu+5DKIKBvRYzpdAUT3Yvjs8Hz2/udlFimUpSQSc6kjyTLwePh1Q2p+dFh7IXWF+Smbfv/id/" +
            "UycD6ReVdhZREB7eZ7bp3DwCgHcyxvYAVtax7SO1Mh9jegx5usmBaRzt5t+TMYzJfN2uJ4VDEnBhbGKgbJg8F3jnky7K6T1DhvAg" +
            "tqUsGBzZtET+4zlEXfXbDii7lY6ndi2I2SnAdi3lEdLL2wd2biYLZMy/pvNFmFVFc/n6QJXnGC2pZqm9cMiN29DD7aR27XBMMyuX" +
            "/HkLIea3kjtEY/S45vzmRTHr0s42nmtZ08W1cHSDKJtubhzXsZ0rVsrMxmYlEpuW3zvsR0mRBpbTPm9zr6GoI7hnGV3ue2Hkk0K0" +
            "riV2JGvsfT3G/LmDcNj7fRwj5M95ZAMcYAsYAiRvAYqv5wHg/WagZwT44y/ve/++MQj4H/8dxP/p3wH82/8V4H/5P//uT4Df/xwC" +
            "/N8Qwz9ki+XGdbICVne3AcmVfcVG+vEQYiYQeJZV3/efyRtTjwXyCAAej8RlIMY6I1n4hRH2uk90Hb0q55lWTPr62Bp+JDxCsoBD" +
            "vjhJeOlb6nCnv4UYAZ4vnsK+GMScL0CD9v77eLw5S4a74KkwHG8WCp5e7b6cjLdrkU6AvWy2TMDGOdfoyxjgE5TkPqpaGJ3MuUUm" +
            "NX1fGFE7ddcoI1RpmcFbf1K92jyxhEougQRbvUdy/63zu+4D7ILZnVya6eMengPJSdKW0rLrFCU2Ym1FH/exy2we4it7/RnuOl4x" +
            "U8el6Ft2IR56ncol7PMxpHM1HJw8MKmYDlzYZRIAID7fHna69f5aCY7FFMuJWWRyHT82OLZb6ZzAczTihlI93+Zn4vjsPYoJT7EQ" +
            "f8byvkimVlw0Z2jZlqbLLqyy2n6N+x4ibG/TzgNiuqpIWIpbEjlpSOHxTC8/cGHuh7bja4cxjXcLldoG8z2pQ+JxFA8eM4LZTw8f" +
            "Rxv4tYSSGj+feaG8d3o9CAHgCbncMlqIqaKdZ9xVPrxN7rFCJtY9bLIJ5JhK0zT9nsyJ43vIi4vTHwHf5z4DAMQHH0biIPPBrvHl" +
            "2ry7F5DdevcrfVz77f+mPhpjQyg8kCqGwG+WoGbyaUqMnyQ3aRcqtbnUzeoLX+KrfnwCBHjuJneXU0Zym2wBAJ7vtfytr/Gx+VX/" +
            "xzOE/ys+4z/9+g3P//SPLypdJwH/238HMf7PP//PrwD/AWL4VxHgb1hp997RM5qcxJT/SeLVX6FwrXLZaAwvBXqNyMEzR+lJZX+k" +
            "HnQRIOg4LFICHthE6N1G3FLcLBrBoLRuZUJ68xgeOE8tATKypYON+xKzBfwwuAWbiBo/zpt/nM8VrWnBQApXGQDK9lH3LNjkxM4R" +
            "FPdSTB6OV2LxwqFf26IHcAwRtZCnkJqN71XyWCtiXkxayIsrMbupMCdJcaRvWWV5hOjj3LeVTQxzyJRtdxw4srg7APnC+L7xGic+" +
            "FWgjRS84gZCTtHNccJDU39JDjqkbD59zCwIKvnQcVwYvou9yO5qfzOVovVfjbbcuJSVTze9Sv6pctKNFemoUw3sDBt6n5coz4fAW" +
            "Y2pLEZOqPTfhykEjiP0Th7raGTRpCuOT3k7LIiUSnL0nQO6lBYCQTObCgQtoXYhQvEWSRXxk7GB7ka10j+yW1PWymZwsusez+rY+" +
            "u/2PaG6V45Xo6iY3godau5xSh2f5FAF+x0VAqbXVF1MIN0VPQ7fpSaMYhMRmxwjb9gplykrT8nzXe9/fPkeAJLL/+xjj//4Ij7//" +
            "x7/7+Rv8I/z89b/NYzQ10gDtf/jv/4v/8o/n3/71E8K/hhj+G/j1uv4rrUB4D49f5TXiUoafyn2xBq78q2ztV3IvvfvY/2Pw1E7G" +
            "H3g+64Zh27AB+AW/cLu/UjlJTJVM7iLAsngeF7db8QkxFFtMSGaP95X35fCE+Hg8DDr1LMc0HbKnbsRjTaZoPFM8kAj36fKEGB8Q" +
            "5IH9Eb9mtxIGwsvsN829n4pIns+fGMuwlEYyeOk4/gLYJyheN9XIpl7OdHziN9ehElIfqbG0TIMUz6SpR93+vOblr0y2vyg78kti" +
            "6lCo8IRYdvUn6/9zuwYAzycUYxvCM8ZYmXNIZmGzL8nFxxPiE/vkoWzvpX8k1wCvHavXpmjeh13ft8uvOYD4rgzATjshFKDcoH7i" +
            "ZRzPYW5ofjFff7FFSMLxGWJ4xPDzo3fRHk+Iz8Le0vhVLAp13QWgp1XK4yOUOpCiamffCAFifKB5oqm48YS+p3WfT4iPx09gCzO0" +
            "XvZN5iIU+p77D3JTvK/xQ9JO2/2VyfUX++U1Rql/a/eJSLI1k0vU4GlRdCPi+7ivX0O12Nh9PCFutie1OwC5D/sk1mN8jbIz+Bqm" +
            "/esXLvMrq/d4L+axjJVe7f/8ZHz/PAEebxvx80PrVAgQH+9rvwDgr38H8a9/gfB3fwfwAPh/H/D4+/D88x/+7r+Cf/iP/wF+/9v/" +
            "GuL/D4WAP3Bwr6cFAAAAAElFTkSuQmCC";
        private Texture2D   _texSettingsIcon = null;
        private Texture2D   _texMailIcon     = null;
        // Set to true/false by CNRModManager via reflection — no reflection needed here.
        public  static bool  ModManagerOpen  = false;

        private bool ModManagerIsOpen() { return ModManagerOpen; }

        private void Update()
        {
            // Keep NGUI cameras disabled while any IMGUI overlay is open.
            // UICamera reads Input.touches/GetMouseButtonDown() directly, so
            // Event.current.Use() alone cannot block NGUI button clicks.
            SetNguiBlocking(_showEcoMail || _showEcoAccount || _showMpDialog);

            // Reconnect loop — runs even while GUI overlay is open
            if (!Ready && !_reconnectRunning)
            {
                _reconnectTimer -= Time.deltaTime;
                if (_reconnectTimer <= 0f)
                {
                    _reconnectTimer   = 15f;  // retry every 15 s
                    _reconnectRunning = true;
                    StartCoroutine(ReconnectAttempt());
                }
            }

            if (!Ready) return;

            // Periodically refresh mail inbox — only when server is reachable
            if (ServerUp)
            {
                _inboxTimer -= Time.deltaTime;
                if (_inboxTimer <= 0f)
                {
                    _inboxTimer = InboxInterval;
                    StartCoroutine(FetchInbox());
                }
            }

            // Backoff timer: wait after a failed send before retrying
            if (_retryDelay > 0f)
            {
                _retryTimer -= Time.deltaTime;
                if (_retryTimer > 0f) return;  // still cooling off
                _retryDelay = 0f;  // ready to try again
            }

            _watchTimer -= Time.deltaTime;
            if (_watchTimer > 0f) return;
            _watchTimer = WatchInterval;

            int nowCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
            int nowGems  = PlayerPrefs.GetInt(GEMS_KEY,  0);

            if (_lastCoins < 0) { _lastCoins = nowCoins; _lastGems = nowGems; return; }

            int dc = nowCoins - _lastCoins;
            int dg = nowGems  - _lastGems;

            if (dc != 0 || dg != 0)
            {
                // Local balance changed — queue a server transaction
                bool isSpend = (dc < 0 || dg < 0);
                string reason = isSpend ? "local_spend" : "local_earn";
                // Generate a match_id for dedup based on timestamp + amount to
                // prevent the same change being reported twice if the watch fires twice
                string matchId = _playerId.Substring(0, Math.Min(8, _playerId.Length)) +
                                 "_" + (long)(DateTime.UtcNow - new DateTime(1970,1,1)).TotalSeconds +
                                 "_c" + dc + "g" + dg;
                _queue.Add(new PendingTx { deltaCoins=dc, deltaGems=dg, reason=reason, matchId=matchId, isSpend=isSpend });
                _lastCoins = nowCoins;
                _lastGems  = nowGems;
                ModEntry.Log("EcoHook queued: dc=" + dc + " dg=" + dg + " reason=" + reason);
            }

            if (_queue.Count > 0 && !_sending)
                StartCoroutine(FlushQueue());
        }

        // ── Queue flush ───────────────────────────────────────────────────────
        private IEnumerator FlushQueue()
        {
            _sending = true;
            while (_queue.Count > 0)
            {
                PendingTx tx = _queue[0];
                _queue.RemoveAt(0);

                bool isEarn = !tx.isSpend;
                // For spends we use absolute values; server deducts
                int absDc = Math.Abs(tx.deltaCoins);
                int absDg = Math.Abs(tx.deltaGems);

                string endpoint = isEarn ? "/earn.php" : "/spend.php";
                string body = "player_id=" + Uri.EscapeDataString(_playerId) +
                              "&token="    + Uri.EscapeDataString(_token) +
                              "&coins="    + absDc +
                              "&gems="     + absDg +
                              "&reason="   + Uri.EscapeDataString(tx.reason) +
                              (tx.matchId != null ? "&match_id=" + Uri.EscapeDataString(tx.matchId) : "");
                var hdrs = new System.Collections.Hashtable();
                hdrs["Content-Type"] = "application/x-www-form-urlencoded";
                var www = new WWW(ModEntry.EconomyUrl + endpoint,
                                  System.Text.Encoding.UTF8.GetBytes(body), hdrs);
                yield return www;

                if (!string.IsNullOrEmpty(www.error))
                {
                    ModEntry.Log("EcoHook flush error (" + endpoint + "): " + www.error);
                    ServerUp = false;
                    // Re-queue for retry with exponential backoff
                    _queue.Insert(0, tx);
                    _retryDelay = (_retryDelay <= 0f) ? RetryDelayMin
                                  : Math.Min(_retryDelay * 2f, RetryDelayMax);
                    _retryTimer = _retryDelay;
                    ModEntry.Log("EcoHook: retry in " + _retryDelay.ToString("F0") + "s");
                    break;
                }

                ServerUp  = true;
                _retryDelay = 0f;  // reset backoff on success
                // Update local cache to server truth
                string coinsStr = ModEntry.ParseJsonValue(www.text, "coins");
                string gemsStr  = ModEntry.ParseJsonValue(www.text, "gems");
                int coins, gems;
                if (int.TryParse(coinsStr, out coins) && int.TryParse(gemsStr, out gems))
                {
                    ServerCoins = coins;
                    ServerGems  = gems;
                    // Correct any drift between local and server
                    if (PlayerPrefs.GetInt(COINS_KEY, 0) != coins)
                    {
                        PlayerPrefs.SetInt(COINS_KEY, coins);
                        _lastCoins = coins;
                    }
                    if (PlayerPrefs.GetInt(GEMS_KEY, 0) != gems)
                    {
                        PlayerPrefs.SetInt(GEMS_KEY, gems);
                        _lastGems = gems;
                    }
                    // Persist last-server-acknowledged balance so ApplyServerBalance
                    // can detect unsynced spends across restarts
                    PlayerPrefs.SetInt(PREF_LAST_SVR_COINS, coins);
                    PlayerPrefs.SetInt(PREF_LAST_SVR_GEMS,  gems);
                    PlayerPrefs.Save();
                }
            }
            _sending = false;
        }

        // ── Public API for CNRSettingsMod (wheel, explicit earn/spend) ──────────
        /// <summary>Start a gift wheel spin — calls /wheel.php?action=spin and awards prize locally.</summary>
        public static void RequestSetPin(string password, string pin)
        {
            var hook = (EconomyHook)(object)FindObjectOfType(typeof(EconomyHook));
            if (hook != null) hook.StartCoroutine(hook.DoSetPin(password, pin));
        }

        private IEnumerator DoSetPin(string password, string pin)
        {
            string body = "player_id=" + Uri.EscapeDataString(_playerId) +
                          "&token="    + Uri.EscapeDataString(_token) +
                          "&password=" + Uri.EscapeDataString(password) +
                          "&pin="      + Uri.EscapeDataString(pin);
            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(ModEntry.EconomyUrl + "/set_pin.php",
                              System.Text.Encoding.UTF8.GetBytes(body), hdrs);
            yield return www;
            ModEntry.Log("SetPin: " + (string.IsNullOrEmpty(www.error) ? www.text : www.error));
        }

        public static void RequestClaim(string displayName, string password, string pin)
        {
            var hook = (EconomyHook)(object)FindObjectOfType(typeof(EconomyHook));
            if (hook != null) hook.StartCoroutine(hook.DoClaim(displayName, password, pin));
        }

        private IEnumerator DoClaim(string displayName, string password, string pin)
        {
            string myId = GetAndroidId() ?? _playerId;
            string body = "display_name=" + Uri.EscapeDataString(displayName) +
                          "&password="    + Uri.EscapeDataString(password) +
                          "&pin="         + Uri.EscapeDataString(pin) +
                          "&player_id="   + Uri.EscapeDataString(myId);
            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(ModEntry.EconomyUrl + "/claim.php",
                              System.Text.Encoding.UTF8.GetBytes(body), hdrs);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                _ecoAccountMsg = "Network error. Try again.";
                ModEntry.Log("Claim error: " + www.error);
                yield break;
            }
            ModEntry.Log("Claim: " + www.text);
            // Check for server-side error first
            string errCode = ModEntry.ParseJsonStringValue(www.text, "error");
            if (!string.IsNullOrEmpty(errCode))
            {
                if (errCode == "multiple_match")
                    _ecoAccountMsg = "Multiple accounts share that name and PIN.\nChange your display name to something unique, then update your PIN, and try again.";
                else if (errCode == "no account found with that name and credentials" || errCode == "no account found with that name and PIN")
                    _ecoAccountMsg = "No account found. Check the name and credentials.";
                else if (errCode.StartsWith("device already linked"))
                    _ecoAccountMsg = "This device is already linked to an account. Use register to re-login.";
                else
                    _ecoAccountMsg = "Transfer failed: " + errCode;
                yield break;
            }
            string newToken = ModEntry.ParseJsonStringValue(www.text, "token");
            if (!string.IsNullOrEmpty(newToken))
            {
                // Server returns the new device's player_id back so we can confirm
                _token = newToken;
                PlayerPrefs.SetString(PREF_TOKEN, newToken);
                PlayerPrefs.Save();
                string coinsStr = ModEntry.ParseJsonValue(www.text, "coins");
                int coins; if (int.TryParse(coinsStr, out coins)) ServerCoins = coins;
                string gemsStr = ModEntry.ParseJsonValue(www.text, "gems");
                int gems; if (int.TryParse(gemsStr, out gems)) ServerGems = gems;
                // Apply canonical display name
                ApplyDisplayName(www.text);
                // Apply server progression (claim response includes flattened wl_*/su_* fields)
                ApplyProgression(www.text);
                Ready = true;
                _ecoAccountMsg = "Account linked! Welcome back.";
            }
            else
            {
                _ecoAccountMsg = "Transfer failed. Try again.";
            }
        }

        // ── Mail inbox ────────────────────────────────────────────────────────────
        public static void RequestClaimMail(int mailId)
        {
            var hook = (EconomyHook)(object)FindObjectOfType(typeof(EconomyHook));
            if (hook != null) hook.StartCoroutine(hook.DoClaimMail(mailId));
        }

        public static void RequestFetchInbox()
        {
            var hook = (EconomyHook)(object)FindObjectOfType(typeof(EconomyHook));
            if (hook != null) hook.StartCoroutine(hook.FetchInbox());
        }

        private IEnumerator DoClaimMail(int mailId)
        {
            string body = "player_id=" + Uri.EscapeDataString(_playerId) +
                          "&token="    + Uri.EscapeDataString(_token) +
                          "&action=claim&mail_id=" + mailId;
            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(ModEntry.EconomyUrl + "/mail.php",
                              System.Text.Encoding.UTF8.GetBytes(body), hdrs);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) { ModEntry.Log("ClaimMail error: " + www.error); yield break; }
            string coinsStr = ModEntry.ParseJsonValue(www.text, "coins");
            string gemsStr  = ModEntry.ParseJsonValue(www.text, "gems");
            int coins, gems;
            if (int.TryParse(coinsStr, out coins) && int.TryParse(gemsStr, out gems))
            {
                ServerCoins = coins; ServerGems = gems;
                PlayerPrefs.SetInt(COINS_KEY, coins); PlayerPrefs.SetInt(GEMS_KEY, gems);
                PlayerPrefs.SetInt(PREF_LAST_SVR_COINS, coins);
                PlayerPrefs.SetInt(PREF_LAST_SVR_GEMS,  gems);
                PlayerPrefs.Save();
                // Update _lastCoins so the watch loop does NOT re-report this as
                // a local delta (which would double-spend/earn on the server)
                _lastCoins = coins; _lastGems = gems;
            }
            // Mark as claimed locally so UI updates immediately
            for (int i = 0; i < MailIds.Length; i++)
                if (MailIds[i] == mailId) { MailClaimed[i] = true; break; }
            int unread = 0;
            for (int i = 0; i < MailClaimed.Length; i++) if (!MailClaimed[i]) unread++;
            MailUnread = unread;
            ModEntry.Log("ClaimMail " + mailId + ": " + www.text);
        }

        internal IEnumerator FetchInbox()
        {
            if (!Ready || string.IsNullOrEmpty(_playerId) || string.IsNullOrEmpty(_token)) yield break;
            string url = ModEntry.EconomyUrl + "/mail.php?action=inbox" +
                         "&player_id=" + Uri.EscapeDataString(_playerId) +
                         "&token="     + Uri.EscapeDataString(_token);
            var www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) { ModEntry.Log("FetchInbox error: " + www.error); yield break; }
            ParseInbox(www.text);
            ModEntry.Log("FetchInbox: " + MailIds.Length + " items, " + MailUnread + " unread");
        }

        private static void ParseInbox(string json)
        {
            int arrStart = json.IndexOf('[');
            int arrEnd   = json.LastIndexOf(']');
            if (arrStart < 0 || arrEnd <= arrStart)
            {
                MailIds = new int[0]; MailSubjects = new string[0]; MailBodies = new string[0];
                MailCoins = new int[0]; MailGems = new int[0]; MailClaimed = new bool[0]; MailUnread = 0;
                return;
            }
            string arr = json.Substring(arrStart + 1, arrEnd - arrStart - 1).Trim();
            if (arr.Length == 0)
            {
                MailIds = new int[0]; MailSubjects = new string[0]; MailBodies = new string[0];
                MailCoins = new int[0]; MailGems = new int[0]; MailClaimed = new bool[0]; MailUnread = 0;
                return;
            }
            var ids      = new System.Collections.Generic.List<int>();
            var subjects = new System.Collections.Generic.List<string>();
            var bodies   = new System.Collections.Generic.List<string>();
            var coinsL   = new System.Collections.Generic.List<int>();
            var gemsL    = new System.Collections.Generic.List<int>();
            var claimL   = new System.Collections.Generic.List<bool>();
            string[] objs = arr.Split(new string[]{"},{"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string obj in objs)
            {
                int id, c, g;
                int.TryParse(ModEntry.ParseJsonValue(obj, "id")    ?? "0", out id);
                int.TryParse(ModEntry.ParseJsonValue(obj, "coins") ?? "0", out c);
                int.TryParse(ModEntry.ParseJsonValue(obj, "gems")  ?? "0", out g);
                string sub   = ModEntry.ParseJsonValue(obj, "subject") ?? "";
                string bod   = ModEntry.ParseJsonValue(obj, "body")    ?? "";
                string clStr = ModEntry.ParseJsonValue(obj, "claimed") ?? "0";
                bool cl = (clStr == "1" || clStr == "true");
                ids.Add(id); subjects.Add(sub); bodies.Add(bod); coinsL.Add(c); gemsL.Add(g); claimL.Add(cl);
            }
            MailIds      = ids.ToArray();
            MailSubjects = subjects.ToArray();
            MailBodies   = bodies.ToArray();
            MailCoins    = coinsL.ToArray();
            MailGems     = gemsL.ToArray();
            MailClaimed  = claimL.ToArray();
            int unread = 0;
            for (int i = 0; i < claimL.Count; i++) if (!claimL[i]) unread++;
            MailUnread = unread;
        }

        public static IEnumerator RequestWheelSpin(System.Action<string,int,int,int> onDone)
        {
            // onDone(prizeType, prizeAmount, newCoins, newGems)
            var hook = FindObjectOfType<EconomyHook>();
            if (hook == null || !Ready) { onDone(null, 0, 0, 0); yield break; }
            yield return hook.StartCoroutine(hook.DoWheelSpin(onDone));
        }

        private IEnumerator DoWheelSpin(System.Action<string,int,int,int> onDone)
        {
            string body = "player_id=" + Uri.EscapeDataString(_playerId) +
                          "&token="    + Uri.EscapeDataString(_token) +
                          "&action=spin";
            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(ModEntry.EconomyUrl + "/wheel.php",
                              System.Text.Encoding.UTF8.GetBytes(body), hdrs);
            yield return www;

            if (!string.IsNullOrEmpty(www.error)) { onDone(null, 0, 0, 0); yield break; }

            string prizeType   = ModEntry.ParseJsonValue(www.text, "prize_type");
            string prizeAmtStr = ModEntry.ParseJsonValue(www.text, "prize_amount");
            string coinsStr    = ModEntry.ParseJsonValue(www.text, "coins");
            string gemsStr     = ModEntry.ParseJsonValue(www.text, "gems");
            int prizeAmt, coins, gems;
            int.TryParse(prizeAmtStr, out prizeAmt);
            int.TryParse(coinsStr,    out coins);
            int.TryParse(gemsStr,     out gems);
            // Apply server truth
            PlayerPrefs.SetInt(COINS_KEY, coins); PlayerPrefs.SetInt(GEMS_KEY, gems);
            PlayerPrefs.SetInt(PREF_LAST_SVR_COINS, coins);
            PlayerPrefs.SetInt(PREF_LAST_SVR_GEMS,  gems);
            PlayerPrefs.Save();
            ServerCoins = coins; ServerGems = gems;
            _lastCoins = coins; _lastGems = gems;
            onDone(prizeType, prizeAmt, coins, gems);
        }

        // ── Main-menu overlay — scene tracking, patching, drawing ─────────────
        // Disable NGUI UICamera components while an overlay is open so that
        // taps handled by IMGUI don't also fire NGUI buttons behind the overlay.
        private void SetNguiBlocking(bool block)
        {
            if (block == _nguiBlocked) return;
            if (_nguiCameras == null)
                _nguiCameras = (UICamera[])FindObjectsOfType(typeof(UICamera));
            foreach (var cam in _nguiCameras)
                if (cam != null) cam.enabled = !block;
            _nguiBlocked = block;
        }

        private void OnLevelWasLoaded(int lvl)
        {
            _ecoScene    = Application.loadedLevelName ?? "";
            _ecoPatched  = false;
            _ecoDbgLog   = false;
            _showEcoMail    = false;
            _showEcoAccount = false;
            _goHelpBtn        = null;
            _goRecordBtn      = null;
            _goAgreementBtn   = null;
            _goMultiplayerBtn = null;
            _showMpDialog     = false;
            _ecoNguiCam  = null;
            _nguiCameras = null;   // invalidate cache; new UICamera instances after scene load
            _nguiBlocked = false;
            if (_ecoScene == "MainMenu") StartCoroutine(EcoPatchDelay());
        }

        private IEnumerator EcoPatchDelay()
        {
            yield return null;
            yield return null;
            EcoPatchMainMenu();
        }

        private void EcoPatchMainMenu()
        {
            if (_ecoPatched) return;
            _ecoPatched = true;
            // Find + hide the ? (HelpScene) button; also cache Recordings and UserAgreement GOs as position anchors
            MonoBehaviour[] all = (MonoBehaviour[])(object)
                UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour));
            foreach (MonoBehaviour mb in all)
            {
                string typeName = mb.GetType().Name;
                if (typeName == "UIButtonEventKit")
                {
                    FieldInfo fi = mb.GetType().GetField("buttonName",
                        BindingFlags.Instance | BindingFlags.Public);
                    if (fi == null) continue;
                    int bval = (int)(object)fi.GetValue(mb);
                    if (bval == 59 && _goHelpBtn == null)        // ToHelpScene — hide it
                    {
                        _goHelpBtn = ((Component)(object)mb).gameObject;
                        _goHelpBtn.SetActive(false);
                    }
                    else if (bval == 63 && _goRecordBtn == null) // ShowVideoBtn — Recordings
                    {
                        _goRecordBtn = ((Component)(object)mb).gameObject;
                    }
                    else if (bval == 49 && _goMultiplayerBtn == null) // GotoHall — intercept
                    {
                        _goMultiplayerBtn = ((Component)(object)mb).gameObject;
                        // Disable original handler so our interceptor takes over
                        ((Behaviour)(object)mb).enabled = false;
                        var interceptor = _goMultiplayerBtn.AddComponent<MpButtonInterceptor>();
                        interceptor.hook = this;
                    }
                }
                else if (typeName == "RatingPopButtonEvent" && _goAgreementBtn == null)
                {
                    FieldInfo fi = mb.GetType().GetField("buttonName",
                        BindingFlags.Instance | BindingFlags.Public);
                    if (fi != null && (int)(object)fi.GetValue(mb) == 5) // ShowAgreement
                        _goAgreementBtn = ((Component)(object)mb).gameObject;
                }
            }
            // Load button icons (once per app session; texture persists across scene reloads)
            if (_texSettingsIcon == null) _texSettingsIcon = LoadB64Icon(_SettingsIconB64);
            if (_texMailIcon     == null) _texMailIcon     = LoadB64Icon(_MailIconB64);
            // Cache NGUI camera
            foreach (string n in new string[]{ "Camera", "UI Camera", "UICamera" })
            {
                GameObject g = GameObject.Find(n);
                if (g != null) { _ecoNguiCam = g.GetComponent<Camera>(); if (_ecoNguiCam != null) break; }
            }
            // Cache game font from any UILabel in the scene
            UILabel[] lbls = (UILabel[])(object)
                UnityEngine.Object.FindObjectsOfType(typeof(UILabel));
            foreach (UILabel lbl in lbls)
                if (lbl.font != null && lbl.font.dynamicFont != null)
                { _ecoFont = lbl.font.dynamicFont; break; }
            ModEntry.Log("EcoHook menu: helpBtn=" + (_goHelpBtn != null ? "found" : "null")
                + " recordBtn=" + (_goRecordBtn != null ? "found" : "null")
                + " agreementBtn=" + (_goAgreementBtn != null ? "found" : "null")
                + " multiBtn=" + (_goMultiplayerBtn != null ? "intercepted" : "null")
                + " settingsIcon=" + (_texSettingsIcon != null ? "ok" : "missing")
                + " mailIcon=" + (_texMailIcon != null ? "ok" : "missing")
                + " nguiCam=" + (_ecoNguiCam != null ? "found" : "null"));
            // Dump all component types on button GOs to identify what's present at runtime
            if (_goAgreementBtn != null)
                ModEntry.Log("AgreementBtn comps: " + EcoNguiDumpComponents(_goAgreementBtn));
            if (_goRecordBtn != null)
                ModEntry.Log("RecordBtn comps: " + EcoNguiDumpComponents(_goRecordBtn));
        }

        static Texture2D LoadB64Icon(string b64)
        {
            try
            {
                byte[] bytes = System.Convert.FromBase64String(b64);
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                return tex.LoadImage(bytes) ? tex : null;
            }
            catch { return null; }
        }

        static Texture2D LoadPngIcon(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                byte[] bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                return tex.LoadImage(bytes) ? tex : null;
            }
            catch { return null; }
        }

        private void OnGUI()
        {
            // Version watermark — top-left corner, visible in all scenes
            {
                var vs = new GUIStyle(GUI.skin.label);
                vs.fontSize  = 22;
                vs.fontStyle = FontStyle.Bold;
                vs.normal.textColor = new Color(1f, 1f, 1f, 0.55f);
                string verStr = "CNRMod v" + ModEntry.Version;
                try
                {
                    foreach (var kv in ModEntry.RegisteredMods)
                        if (!kv.Key.Equals("CNRMod", StringComparison.OrdinalIgnoreCase))
                            verStr += "  +  " + kv.Key + " v" + kv.Value;
                }
                catch { }
                GUI.color = Color.white;
                GUI.Label(new Rect(5f, 5f, 700f, 28f), verStr, vs);
            }

            if (_ecoScene != "MainMenu") return;
            if (!_ecoPatched) return;
            float sc = Screen.width / ECO_REF_W;
            GUIUtility.ScaleAroundPivot(new Vector2(sc, sc), Vector2.zero);

            if (_showEcoMail)
            {
                EcoDrawMailOverlay();
                if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                    Event.current.Use();
                return;
            }
            if (_showEcoAccount)
            {
                EcoDrawAccountOverlay();
                if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                    Event.current.Use();
                return;
            }

            if (_showMpDialog)
            {
                EcoDrawMpRequiredDialog();
                if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                    Event.current.Use();
                return;
            }

            // Don't draw mail/settings icons while the Mod Manager overlay is open
            if (ModManagerIsOpen()) return;

            float vh = Screen.height / sc;
            GUIStyle sb = EcoBtnSt(14, new Color(1f, 0.85f, 0.3f));
            sb.fontStyle = FontStyle.Bold;
            int unread = MailUnread;
            string mailLabel = unread > 0 ? "Mail (" + unread + ")" : "Mail";
            GUIStyle sbMail = new GUIStyle(sb);
            if (unread > 0) sbMail.normal.textColor = new Color(1f, 0.5f, 0.3f);
            string acctLabel = SettingsModPresent ? "Settings" : "Account";

            const float pad = 6f, textBtnH = 26f;

            // ── Mail button — to the right of the user agreement button ───────
            float mailW, mailH, mailX, mailY;
            if (_ecoNguiCam != null && _goAgreementBtn != null)
            {
                Rect ar = EcoNguiRect(_ecoNguiCam, _goAgreementBtn, sc);
                // match anchor button size exactly
                mailW = ar.width;
                mailH = ar.height;
                mailX = ar.x + ar.width + pad;
                mailY = ar.y;
                if (!_ecoDbgLog)
                {
                    _ecoDbgLog = true;
                    ModEntry.Log("Eco btns: ar=" + ar.x.ToString("F0") + "," + ar.y.ToString("F0")
                        + " " + ar.width.ToString("F0") + "x" + ar.height.ToString("F0")
                        + " Screen=" + Screen.width + "x" + Screen.height + " sc=" + sc.ToString("F2"));
                }
            }
            else
            {
                mailW = _texMailIcon != null ? textBtnH : 130f;
                mailH = textBtnH;
                mailX = ECO_REF_W - 10f - 260f; mailY = vh - 34f;
            }

            // ── Settings/Account button — to the left of the recordings button ─
            float acctW, acctH, acctX, acctY;
            if (_ecoNguiCam != null && _goRecordBtn != null)
            {
                Rect rr = EcoNguiRect(_ecoNguiCam, _goRecordBtn, sc);
                // match anchor button size exactly
                acctW = rr.width;
                acctH = rr.height;
                acctX = rr.x - acctW - pad;
                acctY = rr.y;
            }
            else
            {
                acctW = _texSettingsIcon != null ? textBtnH : 120f;
                acctH = textBtnH;
                acctX = ECO_REF_W - 10f - 120f; acctY = vh - 34f;
            }

            // ── Draw mail button ──────────────────────────────────────────────
            if (_texMailIcon != null)
            {
                GUIStyle iconSt = new GUIStyle();
                iconSt.imagePosition   = ImagePosition.ImageOnly;
                iconSt.normal.background  = null;
                iconSt.hover.background   = EcoMkTex(2, 2, new Color(1f, 1f, 1f, 0.2f));
                iconSt.active.background  = EcoMkTex(2, 2, new Color(1f, 1f, 1f, 0.4f));
                iconSt.border  = new RectOffset(0, 0, 0, 0);
                iconSt.padding = new RectOffset(0, 0, 0, 0);
                if (unread > 0) GUI.color = new Color(1f, 0.7f, 0.4f);
                if (GUI.Button(new Rect(mailX, mailY, mailW, mailH),
                               new GUIContent(_texMailIcon), iconSt)) EcoOpenMail();
                GUI.color = Color.white;
                if (unread > 0)
                {
                    GUIStyle badgeSt = new GUIStyle(GUI.skin.label);
                    badgeSt.fontSize  = 11;
                    badgeSt.fontStyle = FontStyle.Bold;
                    badgeSt.normal.textColor = new Color(1f, 0.3f, 0.2f);
                    GUI.Label(new Rect(mailX + mailW - 14f, mailY - 4f, 22f, 18f),
                              unread.ToString(), badgeSt);
                }
            }
            else
            {
                if (GUI.Button(new Rect(mailX, mailY, mailW, mailH), mailLabel, sbMail)) EcoOpenMail();
            }

            // ── Draw settings/account button ──────────────────────────────────
            if (_texSettingsIcon != null)
            {
                GUIStyle iconSt = new GUIStyle();
                iconSt.imagePosition   = ImagePosition.ImageOnly;
                iconSt.normal.background  = null;
                iconSt.hover.background   = EcoMkTex(2, 2, new Color(1f, 1f, 1f, 0.2f));
                iconSt.active.background  = EcoMkTex(2, 2, new Color(1f, 1f, 1f, 0.4f));
                iconSt.border  = new RectOffset(0, 0, 0, 0);
                iconSt.padding = new RectOffset(0, 0, 0, 0);
                if (GUI.Button(new Rect(acctX, acctY, acctW, acctH),
                               new GUIContent(_texSettingsIcon), iconSt)) EcoOpenAccount();
            }
            else
            {
                if (GUI.Button(new Rect(acctX, acctY, acctW, acctH), acctLabel, sb)) EcoOpenAccount();
            }
        }

        private void EcoOpenMail()
        {
            if (Time.unscaledTime - _ecoLastToggle < 0.5f) return;
            _ecoLastToggle = Time.unscaledTime;
            _showEcoMail   = true;
            _ecoMailScroll = Vector2.zero;
            StartCoroutine(FetchInbox());
        }

        private void EcoOpenAccount()
        {
            if (Time.unscaledTime - _ecoLastToggle < 0.5f) return;
            _ecoLastToggle = Time.unscaledTime;
            if (SettingsModPresent && OnAccountButtonClicked != null)
            {
                OnAccountButtonClicked();
                return;
            }
            _showEcoAccount = true;
            _ecoAcctScroll  = Vector2.zero;
            _ecoAccountMsg  = "";
        }

        // ── Multiplayer blocked dialog ────────────────────────────────────────
        public void ShowMpMissingDialog(bool cnrOk, bool stgOk)
        {
            _mpMissingCnr = !cnrOk;
            _mpMissingStg = !stgOk;
            _showMpDialog = true;
        }

        private void TryOpenModManager()
        {
            // Call ModManagerEntry.OpenWindow() (public static) via reflection.
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = asm.GetType("CNRModManager.ModManagerEntry");
                    if (t == null) continue;
                    MethodInfo m = t.GetMethod("OpenWindow",
                        BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (m != null) { m.Invoke(null, null); return; }
                    break; // type found but method not available
                }
            }
            catch { }
        }

        private void EcoDrawMpRequiredDialog()
        {
            float vw = ECO_REF_W;
            float vh = Screen.height / (Screen.width / ECO_REF_W);
            // Dim background
            GUI.Button(new Rect(0, 0, vw, vh), GUIContent.none, GUIStyle.none);
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, vw, vh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Dialog box
            float dw = Mathf.Min(vw * 0.90f, 420f);
            float dh = 210f;
            // Show "Open Mod Manager" button if CNRModManager is loaded in the app domain.
            bool modMgrLoaded = false;
            if (!modMgrLoaded) { foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) { if (a.GetType("CNRModManager.ModManagerEntry") != null) { modMgrLoaded = true; break; } } }
            if (modMgrLoaded) dh += 38f;
            float dx = (vw - dw) * 0.5f;
            float dy = (vh - dh) * 0.5f;

            GUIStyle bg = new GUIStyle();
            bg.normal.background = EcoMkTex(2, 2, new Color(0.10f, 0.10f, 0.12f, 0.97f));
            GUI.Box(new Rect(dx, dy, dw, dh), GUIContent.none, bg);

            float px = dx + 16f, py = dy + 14f, pw = dw - 32f;

            // Title
            GUIStyle titleSt = EcoBtnSt(17, new Color(1f, 0.65f, 0.2f));
            titleSt.fontStyle = FontStyle.Bold;
            titleSt.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(px, py, pw, 26f), "Multiplayer requires mods", titleSt);
            py += 30f;

            // Body
            GUIStyle bodySt = EcoBtnSt(13, Color.white);
            bodySt.alignment = TextAnchor.UpperLeft;
            bodySt.wordWrap  = true;
            string body = "The following mod" + ((_mpMissingCnr && _mpMissingStg) ? "s are" : " is")
                + " required to play multiplayer:\n";
            if (_mpMissingCnr) body += "  \u2022 CNRMod.dll\n";
            if (_mpMissingStg) body += "  \u2022 CNRSettingsMod.dll\n";
            body += "\nInstall the missing mod" + ((_mpMissingCnr && _mpMissingStg) ? "s" : "")
                + " and restart the game.";
            GUI.Label(new Rect(px, py, pw, 110f), body, bodySt);
            py += 118f;

            // Buttons
            float btnH = 34f, btnW = (dw - 32f - 8f) * 0.5f;
            GUIStyle closeSt = EcoBtnSt(14, Color.white);
            if (GUI.Button(new Rect(px, py, btnW, btnH), "Close", closeSt))
                _showMpDialog = false;

            if (modMgrLoaded)
            {
                GUIStyle dlSt = EcoBtnSt(14, new Color(0.4f, 0.85f, 1f));
                if (GUI.Button(new Rect(px + btnW + 8f, py, btnW, btnH), "Open Mod Manager", dlSt))
                {
                    _showMpDialog = false;
                    TryOpenModManager();
                }
            }
        }

        private void EcoDrawMailOverlay()
        {
            float vw = ECO_REF_W;
            float vh = Screen.height / (Screen.width / ECO_REF_W);
            GUI.Button(new Rect(0, 0, vw, vh), GUIContent.none, GUIStyle.none);
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, vw, vh), Texture2D.whiteTexture);
            GUI.color = Color.white;
            float w = Mathf.Min(vw * 0.96f, 420f);
            float h = Mathf.Min(vh * 0.92f, 525f);
            _ecoMailWinRect = new Rect((vw - w) * 0.5f, (vh - h) * 0.5f, w, h);
            GUIStyle wBg = new GUIStyle(GUI.skin.window);
            wBg.normal.background   = EcoMkTex(2, 2, new Color(0.10f, 0.10f, 0.12f, 0.97f));
            wBg.onNormal.background = wBg.normal.background;
            wBg.fontSize = 15;
            _ecoMailWinRect = GUI.Window(9903, _ecoMailWinRect, EcoMailWindow, "  Mail", wBg);
        }

        private void EcoMailWindow(int id)
        {
            float closeH  = 38f;
            float scrollH = _ecoMailWinRect.height - 52f - closeH;
            GUIStyle vScr = new GUIStyle(GUI.skin.verticalScrollbar); vScr.fixedWidth = 30f;
            _ecoMailScroll = GUILayout.BeginScrollView(_ecoMailScroll, false, true,
                GUIStyle.none, vScr,
                GUILayout.Width(_ecoMailWinRect.width - 4f), GUILayout.Height(scrollH));
            GUILayout.Space(6f);

            if (MailIds.Length == 0)
            {
                GUIStyle mt = EcoLblSt();
                mt.normal.textColor = new Color(0.55f, 0.55f, 0.65f);
                mt.alignment = TextAnchor.MiddleCenter;
                if (!Ready)
                {
                    int countdown = Mathf.CeilToInt(_reconnectTimer);
                    string errLine = string.IsNullOrEmpty(_connectError) ? "" : "\n" + _connectError;
                    GUILayout.Label("Server offline — retrying in " + countdown + "s" + errLine, mt,
                        GUILayout.Height(80f));
                }
                else
                {
                    GUILayout.Label("No mail yet.", mt, GUILayout.Height(80f));
                }
            }
            else
            {
                GUIStyle subjSt    = EcoLblSt(); subjSt.fontStyle = FontStyle.Bold;
                GUIStyle bodySt    = EcoHintSt();
                GUIStyle rewardSt  = EcoLblSt(); rewardSt.normal.textColor  = new Color(1f, 0.88f, 0.3f);
                GUIStyle claimedSt = EcoLblSt(); claimedSt.normal.textColor = new Color(0.45f, 0.75f, 0.45f);
                for (int i = 0; i < MailIds.Length; i++)
                {
                    bool cl = i < MailClaimed.Length && MailClaimed[i];
                    GUIStyle card = new GUIStyle(GUI.skin.box);
                    card.normal.background = EcoMkTex(2, 2, cl
                        ? new Color(0.07f, 0.10f, 0.07f, 0.80f)
                        : new Color(0.09f, 0.13f, 0.20f, 0.95f));
                    GUILayout.BeginVertical(card, GUILayout.ExpandWidth(true));
                    GUILayout.Space(4f);
                    string subj = i < MailSubjects.Length ? MailSubjects[i] : "";
                    string bod  = i < MailBodies.Length   ? MailBodies[i]   : "";
                    int    c    = i < MailCoins.Length    ? MailCoins[i]    : 0;
                    int    g    = i < MailGems.Length     ? MailGems[i]     : 0;
                    GUILayout.Label(subj, cl ? claimedSt : subjSt);
                    if (bod.Length > 0) GUILayout.Label(bod, bodySt);
                    if (c > 0 || g > 0)
                    {
                        string rwd = "";
                        if (c > 0) rwd += c + " coins";
                        if (c > 0 && g > 0) rwd += "  +  ";
                        if (g > 0) rwd += g + " gems";
                        GUILayout.Label("Reward: " + rwd, rewardSt);
                    }
                    GUILayout.Space(4f);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (cl)
                        GUILayout.Label("Claimed", claimedSt, GUILayout.Width(90f));
                    else if (GUILayout.Button("Claim Reward", EcoBtnSt(14, new Color(0.3f, 1f, 0.5f)),
                        GUILayout.Width(120f), GUILayout.Height(28f)))
                        RequestClaimMail(MailIds[i]);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(4f);
                    GUILayout.EndVertical();
                    GUILayout.Space(6f);
                }
            }
            GUILayout.Space(6f);
            GUILayout.EndScrollView();
            float bx = (_ecoMailWinRect.width - 160f) * 0.5f;
            float by = _ecoMailWinRect.height - closeH - 6f;
            if (GUI.Button(new Rect(bx, by, 160f, closeH - 4f), "  Close  ", EcoBtnSt(22, Color.white)))
            { _showEcoMail = false; _ecoLastToggle = Time.unscaledTime; }
        }

        // ── Account panel (standalone — only shown when CNRSettingsMod absent) ──
        private void EcoDrawAccountOverlay()
        {
            float vw = ECO_REF_W;
            float vh = Screen.height / (Screen.width / ECO_REF_W);
            GUI.Button(new Rect(0, 0, vw, vh), GUIContent.none, GUIStyle.none);
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, vw, vh), Texture2D.whiteTexture);
            GUI.color = Color.white;
            float w = Mathf.Min(vw * 0.96f, 420f);
            float h = Mathf.Min(vh * 0.92f, 525f);
            _ecoAcctWinRect = new Rect((vw - w) * 0.5f, (vh - h) * 0.5f, w, h);
            GUIStyle wBg = new GUIStyle(GUI.skin.window);
            wBg.normal.background   = EcoMkTex(2, 2, new Color(0.10f, 0.10f, 0.12f, 0.97f));
            wBg.onNormal.background = wBg.normal.background;
            wBg.fontSize = 15;
            _ecoAcctWinRect = GUI.Window(9904, _ecoAcctWinRect, EcoAccountWindow, "  Account", wBg);
        }

        private void EcoAccountWindow(int id)
        {
            float pw     = _ecoAcctWinRect.width - 28f;
            float closeH = 38f;
            float scrollH = _ecoAcctWinRect.height - 52f - closeH;
            GUIStyle vScr = new GUIStyle(GUI.skin.verticalScrollbar); vScr.fixedWidth = 30f;
            _ecoAcctScroll = GUILayout.BeginScrollView(_ecoAcctScroll, false, true,
                GUIStyle.none, vScr,
                GUILayout.Width(_ecoAcctWinRect.width - 4f), GUILayout.Height(scrollH));
            EcoSecHdr("Your Account");
            GUILayout.Space(6f);
            string pid    = PlayerPrefs.GetString(PREF_PLAYER_ID, "");
            string dispId = pid.Length >= 8 ? pid.Substring(0, 8) + "..." : (pid.Length > 0 ? pid : "(not registered)");
            GUILayout.Label("Device ID:  " + dispId, EcoLblSt());
            GUILayout.Space(4f);
            string statusTxt;
            Color  statusCol;
            if (Ready)      { statusTxt = "Connected";  statusCol = new Color(0.3f, 1f, 0.4f); }
            else if (ServerUp) { statusTxt = "Syncing\u2026"; statusCol = new Color(1f, 0.85f, 0.2f); }
            else
            {
                int cd = Mathf.CeilToInt(_reconnectTimer);
                statusTxt = "Offline \u2014 retrying in " + cd + "s";
                statusCol = new Color(1f, 0.4f, 0.4f);
            }
            GUIStyle stSt = EcoLblSt(); stSt.normal.textColor = statusCol;
            GUILayout.Label("Server:  " + statusTxt, stSt);
            if (!Ready && !string.IsNullOrEmpty(_connectError))
            {
                GUIStyle errSt = EcoHintSt(); errSt.normal.textColor = new Color(1f, 0.5f, 0.4f);
                GUILayout.Label(_connectError, errSt);
            }
            GUILayout.Space(10f);
            if (Ready)
            {
                GUILayout.Label("Coins:  " + ServerCoins, EcoLblSt());
                GUILayout.Label("Gems:   " + ServerGems,  EcoLblSt());
                GUILayout.Space(10f);
            }
            EcoSecHdr("Set Recovery Credentials");
            GUILayout.Space(4f);
            GUILayout.Label("Set a password (6+ chars) and 4-8 digit PIN to recover your account on a new phone.", EcoHintSt());
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password:", EcoLblSt(), GUILayout.Width(80f));
            _ecoPinPassword = GUILayout.PasswordField(_ecoPinPassword, '*', 32, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("PIN:", EcoLblSt(), GUILayout.Width(80f));
            _ecoPinInput = GUILayout.TextField(_ecoPinInput, 8, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            if (GUILayout.Button("Save Credentials", EcoBtnSt(18, new Color(0.4f, 0.8f, 1f))))
            {
                bool pinOk = _ecoPinInput.Length >= 4 && _ecoPinInput.Length <= 8;
                if (pinOk) { foreach (char ch in _ecoPinInput) if (ch < '0' || ch > '9') { pinOk = false; break; } }
                if (_ecoPinPassword.Length < 6)
                    _ecoAccountMsg = "Password must be at least 6 characters.";
                else if (!pinOk)
                    _ecoAccountMsg = "PIN must be 4-8 digits.";
                else
                { RequestSetPin(_ecoPinPassword, _ecoPinInput); _ecoAccountMsg = "Credentials saved!"; _ecoPinPassword = ""; _ecoPinInput = ""; }
            }
            GUILayout.Space(10f);
            EcoSecHdr("Transfer to This Phone");
            GUILayout.Space(4f);
            GUILayout.Label("Enter the Display Name, Password, and PIN you set on your other device.", EcoHintSt());
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", EcoLblSt(), GUILayout.Width(80f));
            _ecoClaimName = GUILayout.TextField(_ecoClaimName, 32, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password:", EcoLblSt(), GUILayout.Width(80f));
            _ecoClaimPassword = GUILayout.PasswordField(_ecoClaimPassword, '*', 32, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("PIN:", EcoLblSt(), GUILayout.Width(80f));
            _ecoClaimPin = GUILayout.PasswordField(_ecoClaimPin, '*', 8, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            if (GUILayout.Button("Transfer Account", EcoBtnSt(18, new Color(1f, 0.7f, 0.3f))))
            {
                if (_ecoClaimName.Length > 0 && _ecoClaimPassword.Length >= 6 && _ecoClaimPin.Length >= 4)
                { RequestClaim(_ecoClaimName, _ecoClaimPassword, _ecoClaimPin); _ecoAccountMsg = "Transfer requested..."; _ecoClaimName = ""; _ecoClaimPassword = ""; _ecoClaimPin = ""; }
                else _ecoAccountMsg = "Enter name, password (6+ chars), and PIN.";
            }
            GUILayout.Space(10f);
            if (_ecoAccountMsg.Length > 0)
            {
                GUIStyle ms = EcoLblSt(); ms.normal.textColor = new Color(1f, 0.9f, 0.4f); ms.wordWrap = true;
                GUILayout.Label(_ecoAccountMsg, ms);
            }
            GUILayout.Space(6f);
            GUILayout.EndScrollView();
            float bx = (_ecoAcctWinRect.width - 160f) * 0.5f;
            float by = _ecoAcctWinRect.height - closeH - 6f;
            if (GUI.Button(new Rect(bx, by, 160f, closeH - 4f), "  Close  ", EcoBtnSt(22, Color.white)))
            { _showEcoAccount = false; _ecoLastToggle = Time.unscaledTime; }
        }

        // ── IMGUI helpers ────────────────────────────────────────────────────────
        private static GUIStyle EcoLblSt()
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 15; s.normal.textColor = Color.white;
            if (_ecoFont != null) s.font = _ecoFont;
            return s;
        }
        private static GUIStyle EcoHintSt()
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 11; s.wordWrap = true;
            s.normal.textColor = new Color(0.72f, 0.72f, 0.72f);
            if (_ecoFont != null) s.font = _ecoFont;
            return s;
        }
        private static GUIStyle EcoBtnSt(int fontSize = 20, Color col = default(Color))
        {
            if (col == default(Color)) col = Color.white;
            GUIStyle s = new GUIStyle(GUI.skin.button);
            s.fontSize = fontSize;
            s.fixedHeight = fontSize < 15 ? 33f : 39f;
            s.normal.textColor  = col;
            s.hover.textColor   = col;
            s.active.textColor  = col;
            if (_ecoFont != null) s.font = _ecoFont;
            return s;
        }
        private static void EcoSecHdr(string title)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold; s.fontSize = 16;
            s.normal.textColor = new Color(1f, 0.85f, 0.3f);
            if (_ecoFont != null) s.font = _ecoFont;
            GUILayout.Label("--  " + title + "  --", s);
        }
        private static Texture2D EcoMkTex(int w, int h, Color col)
        {
            Color[] p = new Color[w * h];
            for (int i = 0; i < p.Length; i++) p[i] = col;
            Texture2D t = new Texture2D(w, h); t.SetPixels(p); t.Apply(); return t;
        }
        // Returns the names of all components on go + its children + its ancestors (for debug).
        private static string EcoNguiDumpComponents(GameObject go)
        {
            var sb = new System.Text.StringBuilder();
            // Dump self + full subtree first
            Component[] all = go.GetComponentsInChildren(typeof(Component));
            sb.Append("[subtree]:");
            foreach (Component c in all)
                sb.Append(c.GetType().Name + "@" + c.gameObject.name + ",");
            sb.Append(" ");
            // Then ancestors
            Transform cur = go.transform.parent;
            int depth = 0;
            while (cur != null)
            {
                sb.Append("[" + cur.name + "]:");
                Component[] ac = cur.gameObject.GetComponents(typeof(Component));
                foreach (Component c in ac)
                    sb.Append(c.GetType().Name + ",");
                sb.Append(" ");
                cur = cur.parent;
                if (++depth > 6) break;
            }
            return sb.ToString();
        }

        private static Rect EcoNguiRect(Camera cam, GameObject go, float sc)
        {
            if (go == null) return new Rect(0, 0, 0, 0);
            Vector3 sp = cam.WorldToScreenPoint(go.transform.position);
            float cx = sp.x / sc;
            float cy = (Screen.height - sp.y) / sc;
            float hw = 30f / sc, hh = 30f / sc;
            float bestArea = 0f;
            string bestName = "(fallback)";
            // Walk the full subtree first (self + all descendants), then ancestors.
            // Strategy 1: BoxCollider — project local-space corners through world transform + camera.
            // Using TransformPoint so UIRoot's scale is properly applied.
            BoxCollider bc = go.GetComponent<BoxCollider>();
            if (bc != null)
            {
                float wx = bc.size.x * 0.5f;
                float wy = bc.size.y * 0.5f;
                Vector3 spL = cam.WorldToScreenPoint(go.transform.TransformPoint(bc.center + new Vector3(-wx, 0, 0)));
                Vector3 spR = cam.WorldToScreenPoint(go.transform.TransformPoint(bc.center + new Vector3( wx, 0, 0)));
                Vector3 spD = cam.WorldToScreenPoint(go.transform.TransformPoint(bc.center + new Vector3(0, -wy, 0)));
                Vector3 spU = cam.WorldToScreenPoint(go.transform.TransformPoint(bc.center + new Vector3(0,  wy, 0)));
                float screenW = spR.x - spL.x;
                float screenH = spU.y - spD.y;
                if (screenW > 0f && screenH > 0f)
                {
                    hw = screenW * 0.5f / sc;
                    hh = screenH * 0.5f / sc;
                    bestName = "BoxCollider@" + go.name + " screen=" + (int)screenW + "x" + (int)screenH;
                }
            }
            // Strategy 2: UISprite/UIWidget via reflection — try property first, then field.
            // GetComponentsInChildren(typeof(Component)) works because Component is a Unity engine base.
            Component[] subtree = go.GetComponentsInChildren(typeof(Component));
            foreach (Component wc in subtree)
            {
                string tn = wc.GetType().Name;
                if (tn != "UISprite" && tn != "UIWidget" && tn != "UITexture" && tn != "UILabel") continue;
                float w = 0f, h = 0f;
                // Try property (NGUI >= 3.x)
                System.Reflection.PropertyInfo wp = wc.GetType().GetProperty("width",  System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
                System.Reflection.PropertyInfo hp = wc.GetType().GetProperty("height", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
                if (wp != null && hp != null) { w = System.Convert.ToSingle(wp.GetValue(wc, null)); h = System.Convert.ToSingle(hp.GetValue(wc, null)); }
                else
                {
                    // Try field (older NGUI)
                    System.Reflection.FieldInfo wf = wc.GetType().GetField("width",  System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
                    System.Reflection.FieldInfo hf = wc.GetType().GetField("height", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
                    if (wf != null && hf != null) { w = System.Convert.ToSingle(wf.GetValue(wc)); h = System.Convert.ToSingle(hf.GetValue(wc)); }
                }
                if (w * h > bestArea) { bestArea = w * h; hw = w * 0.5f / sc; hh = h * 0.5f / sc; bestName = tn + "@" + wc.gameObject.name + " " + (int)w + "x" + (int)h; }
            }
            return new Rect(cx - hw, cy - hh, hw * 2f, hh * 2f);
        }

        // ── ANDROID_ID helper ─────────────────────────────────────────────────

        private static string GetAndroidId()
        {
            try
            {
                var player    = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity  = player.GetStatic<AndroidJavaObject>("currentActivity");
                var resolver  = activity.Call<AndroidJavaObject>("getContentResolver");
                var secure    = new AndroidJavaClass("android.provider.Settings$Secure");
                string id     = secure.CallStatic<string>("getString", resolver, "android_id");
                return string.IsNullOrEmpty(id) ? null : id.ToLowerInvariant();
            }
            catch (Exception ex) { ModEntry.Log("GetAndroidId error: " + ex.Message); return null; }
        }
    }

    // ── Multiplayer button interceptor ──────────────────────────────────────────
    public class MpButtonInterceptor : MonoBehaviour
    {
        public EconomyHook hook;

        private void OnClick()
        {
            bool cnrOk = System.IO.File.Exists("/sdcard/CNRMods/CNRMod.dll");
            bool stgOk = System.IO.File.Exists("/sdcard/CNRMods/CNRSettingsMod.dll");
            if (cnrOk && stgOk)
            {
                // Both mods present — let the game proceed normally
                Application.LoadLevel("MultiPlayerSelect");
                return;
            }
            // Missing one or more — show dialog
            if (hook != null)
                hook.ShowMpMissingDialog(cnrOk, stgOk);
            else
                Application.LoadLevel("MultiPlayerSelect"); // fallback if hook lost
        }
    }
}
