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
        public static string ModVersion    = "2.0.1";
        public static bool   KickNoMod     = true;
        public static string WebUrl        = "";    // http://<host>:1337 for node server; derived from SERVER_IP if not set
        public static string EconomyUrl    = "";    // https://<host>/economy  for PHP economy API
        public static bool   IsMaster      = false;  // set by RedirectHook.OnEnteredRoom so MapLoader can pick team spawn

        // ── CNRMod binary version (hardcoded; separate from the kick-threshold in server.cfg) ─────
        public const  string Version = "2.0.1";

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

        private const string PREF_PLAYER_ID    = "CNRMod_EcoPlayerId";
        private const string PREF_TOKEN        = "CNRMod_EcoToken";
        private const string PREF_LAST_SVR_COINS = "CNRMod_SvrCoins";  // last server-acknowledged balance
        private const string PREF_LAST_SVR_GEMS  = "CNRMod_SvrGems";
        private const string COINS_KEY          = "GameCoins";
        private const string GEMS_KEY           = "GameGems";

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

            // Fetch inbox immediately after login/register
            if (Ready) StartCoroutine(FetchInbox());
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
        private string      _ecoPinInput    = "";
        private string      _ecoClaimName   = "";
        private string      _ecoClaimPin    = "";
        private string      _ecoAccountMsg  = "";
        private Rect        _ecoMailWinRect;
        private Rect        _ecoAcctWinRect;
        private UICamera[]  _nguiCameras = null;   // cached for click-through blocking
        private bool        _nguiBlocked = false;
        private GameObject  _goRecordBtn    = null;   // Recordings button GO — anchor for settings btn
        private GameObject  _goAgreementBtn = null;   // User agreement button GO — anchor for mail btn
        private Texture2D   _texSettingsIcon = null;
        private Texture2D   _texMailIcon     = null;

        private void Update()
        {
            // Keep NGUI cameras disabled while any IMGUI overlay is open.
            // UICamera reads Input.touches/GetMouseButtonDown() directly, so
            // Event.current.Use() alone cannot block NGUI button clicks.
            SetNguiBlocking(_showEcoMail || _showEcoAccount);

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
        public static void RequestSetPin(string pin)
        {
            var hook = (EconomyHook)(object)FindObjectOfType(typeof(EconomyHook));
            if (hook != null) hook.StartCoroutine(hook.DoSetPin(pin));
        }

        private IEnumerator DoSetPin(string pin)
        {
            string body = "player_id=" + Uri.EscapeDataString(_playerId) +
                          "&token="    + Uri.EscapeDataString(_token) +
                          "&pin="      + Uri.EscapeDataString(pin);
            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(ModEntry.EconomyUrl + "/set_pin.php",
                              System.Text.Encoding.UTF8.GetBytes(body), hdrs);
            yield return www;
            ModEntry.Log("SetPin: " + (string.IsNullOrEmpty(www.error) ? www.text : www.error));
        }

        public static void RequestClaim(string displayName, string pin)
        {
            var hook = (EconomyHook)(object)FindObjectOfType(typeof(EconomyHook));
            if (hook != null) hook.StartCoroutine(hook.DoClaim(displayName, pin));
        }

        private IEnumerator DoClaim(string displayName, string pin)
        {
            string body = "display_name=" + Uri.EscapeDataString(displayName) +
                          "&pin="         + Uri.EscapeDataString(pin) +
                          "&new_device="  + Uri.EscapeDataString(GetAndroidId());
            var hdrs = new System.Collections.Hashtable();
            hdrs["Content-Type"] = "application/x-www-form-urlencoded";
            var www = new WWW(ModEntry.EconomyUrl + "/claim.php",
                              System.Text.Encoding.UTF8.GetBytes(body), hdrs);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) { ModEntry.Log("Claim error: " + www.error); yield break; }
            string newToken = ModEntry.ParseJsonStringValue(www.text, "token");
            string newId    = ModEntry.ParseJsonStringValue(www.text, "player_id");
            if (!string.IsNullOrEmpty(newToken) && !string.IsNullOrEmpty(newId))
            {
                _playerId = newId; _token = newToken;
                PlayerPrefs.SetString(PREF_PLAYER_ID, newId);
                PlayerPrefs.SetString(PREF_TOKEN, newToken);
                PlayerPrefs.Save();
                Ready = false;
                yield return StartCoroutine(RegisterAndSync());
            }
            ModEntry.Log("Claim: " + www.text);
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
            _goHelpBtn      = null;
            _goRecordBtn    = null;
            _goAgreementBtn = null;
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
            if (_texSettingsIcon == null) _texSettingsIcon = LoadPngIcon("/sdcard/CNRMods/settings.png");
            if (_texMailIcon     == null) _texMailIcon     = LoadPngIcon("/sdcard/CNRMods/mail.png");
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
                + " settingsIcon=" + (_texSettingsIcon != null ? "ok" : "missing")
                + " mailIcon=" + (_texMailIcon != null ? "ok" : "missing")
                + " nguiCam=" + (_ecoNguiCam != null ? "found" : "null"));
            // Dump all component types on button GOs to identify what's present at runtime
            if (_goAgreementBtn != null)
                ModEntry.Log("AgreementBtn comps: " + EcoNguiDumpComponents(_goAgreementBtn));
            if (_goRecordBtn != null)
                ModEntry.Log("RecordBtn comps: " + EcoNguiDumpComponents(_goRecordBtn));
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

        // ── Mail panel ─────────────────────────────────────────────────────────
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
            EcoSecHdr("Set Recovery PIN");
            GUILayout.Space(4f);
            GUILayout.Label("4-8 digits. Used to reclaim your account on a new phone.", EcoHintSt());
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("PIN:", EcoLblSt(), GUILayout.Width(60f));
            _ecoPinInput = GUILayout.TextField(_ecoPinInput, 8, GUI.skin.textField, GUILayout.Width(pw - 70f));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            if (GUILayout.Button("Save PIN", EcoBtnSt(18, new Color(0.4f, 0.8f, 1f))))
            {
                if (_ecoPinInput.Length >= 4 && _ecoPinInput.Length <= 8)
                {
                    bool ok = true;
                    foreach (char ch in _ecoPinInput) if (ch < '0' || ch > '9') { ok = false; break; }
                    if (ok) { RequestSetPin(_ecoPinInput); _ecoAccountMsg = "PIN saved!"; _ecoPinInput = ""; }
                    else _ecoAccountMsg = "PIN must be digits only.";
                }
                else _ecoAccountMsg = "PIN must be 4-8 digits.";
            }
            GUILayout.Space(10f);
            EcoSecHdr("Transfer to This Phone");
            GUILayout.Space(4f);
            GUILayout.Label("Enter the Display Name and PIN from your other device.", EcoHintSt());
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", EcoLblSt(), GUILayout.Width(60f));
            _ecoClaimName = GUILayout.TextField(_ecoClaimName, 32, GUI.skin.textField, GUILayout.Width(pw - 70f));
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("PIN:", EcoLblSt(), GUILayout.Width(60f));
            _ecoClaimPin = GUILayout.PasswordField(_ecoClaimPin, '*', 8, GUI.skin.textField, GUILayout.Width(pw - 70f));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            if (GUILayout.Button("Transfer Account", EcoBtnSt(18, new Color(1f, 0.7f, 0.3f))))
            {
                if (_ecoClaimName.Length > 0 && _ecoClaimPin.Length >= 4)
                { RequestClaim(_ecoClaimName, _ecoClaimPin); _ecoAccountMsg = "Transfer requested..."; _ecoClaimName = ""; _ecoClaimPin = ""; }
                else _ecoAccountMsg = "Enter name and PIN.";
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
}
