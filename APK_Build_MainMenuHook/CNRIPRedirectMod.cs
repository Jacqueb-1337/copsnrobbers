using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRMods
{
    /// <summary>
    /// CNR LAN IP Redirect + Custom Map + Version-Kick Mod
    ///
    /// server.cfg keys (all optional â€” omit any you don't need):
    ///   SERVER_IP    = 192.168.1.10      redirect Photon to LAN server
    ///   SERVER_PORT  = 5055
    ///   APP_ID       = CNRLan
    ///   MAP_URL      = http://192.168.1.10:8080/maps/mymap.json   (master: broadcast to room)
    ///   MOD_VERSION  = 1.0.0
    ///   KICK_NO_MOD  = true              master kicks players with no/wrong major version
    ///
    /// Room flow:
    ///   Master enters room  â†’ sets CNR_MAP_URL + CNR_MOD_VERSION in room custom props
    ///                       â†’ polls otherPlayers; kicks those lacking mod or wrong major ver
    ///   Client enters room  â†’ sets own CNR_MOD_VERSION player prop (master can verify)
    ///                       â†’ if room has CNR_MAP_URL â†’ downloads JSON â†’ saves to MapCachePath
    /// </summary>
    public class CNRIPRedirectMod : MonoBehaviour
    {
        // â”€â”€ Paths â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private const string ConfigPath   = "/storage/emulated/0/CNRMods/server.cfg";
        private const string LogPath      = "/storage/emulated/0/CNRMods/ipredir.log";
        private const string CacheDir     = "/storage/emulated/0/CNRMods/";
        private const string MapCachePath = "/storage/emulated/0/CNRMods/custom_map_cache.json";

        // â”€â”€ Config (loaded once from server.cfg) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static string _serverIp    = "";
        private static int    _serverPort  = 5055;
        private static string _appId       = "CNRLan";
        private static string _mapUrl      = "";
        private static string _modVersion  = "1.0.0";
        private static bool   _kickNoMod   = true;

        // â”€â”€ Runtime state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static bool _initialized    = false;
        private bool        _overrideApplied = false;

        // Room state tracking
        private bool  _inRoom    = false;
        private bool  _isMaster  = false;
        private float _pollTimer = 0f;
        private const float PollInterval = 1.0f;

        // Kick: actorNr -> Time.time when first seen without a version
        private readonly Dictionary<int, float> _pendingVerify = new Dictionary<int, float>();
        private const float KickGraceSeconds = 5f;

        // Cached Photon type
        private static Type _photonNetType = null;

        // â”€â”€ Called by CNRModLoader on startup â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            Log("=== CNRIPRedirectMod Initialize() ===");
            try
            {
                LoadConfig();

                var go = new GameObject("CNRIPRedirect");
                go.AddComponent<CNRIPRedirectMod>();
                DontDestroyOnLoad(go);

                Log("Mod object created.  IP=" + (_serverIp != "" ? _serverIp : "(none)") + "  " +
                    "MAP_URL=" + (_mapUrl != "" ? _mapUrl : "(none)") + "  " +
                    "MOD_VERSION=" + _modVersion + "  KICK_NO_MOD=" + _kickNoMod);
            }
            catch (Exception ex) { Log("Initialize() error: " + (ex)); }
        }

        // â”€â”€ Unity lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void Awake()
        {
            Log("Awake()");
            if (_serverIp != "") ApplyPhotonOverride();
        }

        private void Update()
        {
            // Phase 1: apply Photon server override (one-shot, only when SERVER_IP configured)
            if (!_overrideApplied && _serverIp != "")
                ApplyPhotonOverride();

            // Phase 2: room-state polling (runs every second regardless)
            _pollTimer -= Time.deltaTime;
            if (_pollTimer > 0f) return;
            _pollTimer = PollInterval;
            PollRoomState();
        }

        // â”€â”€ Room state machine â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void PollRoomState()
        {
            try
            {
                Type pnt = GetPhotonNetType();
                if (pnt == null) return;

                bool nowInRoom = GetStaticBool(pnt, "inRoom");
                bool nowMaster = nowInRoom && GetStaticBool(pnt, "isMasterClient");

                if (!_inRoom && nowInRoom)  OnEnteredRoom(pnt, nowMaster);
                else if (_inRoom && !nowInRoom) OnLeftRoom();

                _inRoom   = nowInRoom;
                _isMaster = nowMaster;

                if (_inRoom && _isMaster && _kickNoMod)
                    CheckKickPlayers(pnt);
            }
            catch (Exception ex) { Log("PollRoomState error: " + (ex.Message)); }
        }

        private void OnEnteredRoom(Type pnt, bool asMaster)
        {
            Log("Entered room (asMaster=" + (asMaster) + ")");
            _pendingVerify.Clear();

            if (asMaster)
            {
                // Broadcast map URL + version via room custom properties.
                // PlayerPrefs (set by the in-game URL picker) takes priority over server.cfg.
                string urlToUse = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "");
                if (string.IsNullOrEmpty(urlToUse)) urlToUse = _mapUrl;
                if (!string.IsNullOrEmpty(urlToUse)) SetRoomCustomProp(pnt, "CNR_MAP_URL", urlToUse);
                SetRoomCustomProp(pnt, "CNR_MOD_VERSION", _modVersion);
            }
            else
            {
                // Advertise own version so master can verify
                SetMyPlayerProp(pnt, "CNR_MOD_VERSION", _modVersion);

                // Download map if room has one
                string roomMapUrl = GetRoomProp(pnt, "CNR_MAP_URL");
                if (!string.IsNullOrEmpty(roomMapUrl))
                {
                    Log("Room has MAP_URL: " + (roomMapUrl) + " â€” downloading");
                    StartCoroutine(DownloadMap(roomMapUrl));
                }
            }
        }

        private void OnLeftRoom()
        {
            Log("Left room");
            _pendingVerify.Clear();
        }

        // â”€â”€ Kick logic â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void CheckKickPlayers(Type pnt)
        {
            try
            {
                PropertyInfo otherProp = pnt.GetProperty("otherPlayers",
                    BindingFlags.Static | BindingFlags.Public);
                if (otherProp == null) return;

                var othersRaw = otherProp.GetValue(null, null) as System.Array;
                if (othersRaw == null) return;

                var currentPids = new HashSet<int>();

                foreach (object player in othersRaw)
                {
                    if (player == null) continue;
                    int    pid      = GetIntProperty(player, "ID");
                    string theirVer = GetPlayerProp(player, "CNR_MOD_VERSION");
                    currentPids.Add(pid);

                    if (!_pendingVerify.ContainsKey(pid))
                    {
                        if (string.IsNullOrEmpty(theirVer))
                        {
                            _pendingVerify[pid] = Time.time;
                            Log("Player " + (pid) + " joined â€” grace window started (" + (KickGraceSeconds) + "s)");
                        }
                        // else: version already present â€” no action needed
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(theirVer))
                        {
                            _pendingVerify.Remove(pid);
                            if (!VersionCompatible(theirVer))
                            {
                                Log("Kicking player " + (pid) + ": version '" + (theirVer) + "' incompatible with '" + (_modVersion) + "'");
                                KickPlayer(pnt, player);
                            }
                            else Log("Player " + (pid) + " version OK: " + (theirVer));
                        }
                        else if (Time.time - _pendingVerify[pid] > KickGraceSeconds)
                        {
                            Log("Kicking player " + (pid) + ": no CNR_MOD_VERSION after " + (KickGraceSeconds) + "s");
                            _pendingVerify.Remove(pid);
                            KickPlayer(pnt, player);
                        }
                    }
                }

                // Prune players who left during grace window
                var gone = new List<int>();
                foreach (int pid in _pendingVerify.Keys)
                    if (!currentPids.Contains(pid)) gone.Add(pid);
                foreach (int pid in gone) _pendingVerify.Remove(pid);
            }
            catch (Exception ex) { Log("CheckKickPlayers error: " + (ex.Message)); }
        }

        private void KickPlayer(Type pnt, object player)
        {
            try
            {
                MethodInfo m = pnt.GetMethod("CloseConnection",
                    BindingFlags.Static | BindingFlags.Public,
                    null, new Type[] { player.GetType() }, null);
                if (m != null) m.Invoke(null, new object[] { player });
                else Log("CloseConnection not found on PhotonNetwork");
            }
            catch (Exception ex) { Log("KickPlayer error: " + (ex.Message)); }
        }

        // â”€â”€ Map download â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private IEnumerator DownloadMap(string url)
        {
            var www = new WWW(url);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Log("DownloadMap error: " + (www.error));
                yield break;
            }

            string json = www.text;
            if (string.IsNullOrEmpty(json))
            {
                Log("DownloadMap: empty response");
                yield break;
            }

            try
            {
                if (!Directory.Exists(CacheDir)) Directory.CreateDirectory(CacheDir);
                File.WriteAllText(MapCachePath, json);
                Log("Map saved â†’ " + (MapCachePath) + "  (" + (json.Length) + " bytes)");
            }
            catch (Exception ex) { Log("DownloadMap save error: " + (ex.Message)); }
        }

        // â”€â”€ Photon room/player prop helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void SetRoomCustomProp(Type pnt, string key, string value)
        {
            try
            {
                PropertyInfo roomProp = pnt.GetProperty("room",
                    BindingFlags.Static | BindingFlags.Public);
                if (roomProp == null) { Log("'room' not found on PhotonNetwork"); return; }
                object room = roomProp.GetValue(null, null);
                if (room == null) { Log("PhotonNetwork.room is null"); return; }

                var ht = new System.Collections.Hashtable();
                ht[key] = value;
                MethodInfo m = room.GetType().GetMethod("SetCustomProperties",
                    new Type[] { typeof(System.Collections.Hashtable) });
                if (m != null) m.Invoke(room, new object[] { ht });
                else Log("SetCustomProperties not found on " + (room.GetType().Name));
                Log("SetRoomProp " + (key) + "=" + (value));
            }
            catch (Exception ex) { Log("SetRoomCustomProp error: " + (ex.Message)); }
        }

        private void SetMyPlayerProp(Type pnt, string key, string value)
        {
            try
            {
                PropertyInfo playerProp = pnt.GetProperty("player",
                    BindingFlags.Static | BindingFlags.Public);
                if (playerProp == null) return;
                object player = playerProp.GetValue(null, null);
                if (player == null) return;

                var ht = new System.Collections.Hashtable();
                ht[key] = value;
                MethodInfo m = player.GetType().GetMethod("SetCustomProperties",
                    new Type[] { typeof(System.Collections.Hashtable) });
                if (m != null) m.Invoke(player, new object[] { ht });
                Log("SetMyPlayerProp " + (key) + "=" + (value));
            }
            catch (Exception ex) { Log("SetMyPlayerProp error: " + (ex.Message)); }
        }

        private string GetRoomProp(Type pnt, string key)
        {
            try
            {
                PropertyInfo roomProp = pnt.GetProperty("room",
                    BindingFlags.Static | BindingFlags.Public);
                if (roomProp == null) return null;
                object room = roomProp.GetValue(null, null);
                if (room == null) return null;
                PropertyInfo cpProp = room.GetType().GetProperty("customProperties",
                    BindingFlags.Instance | BindingFlags.Public);
                if (cpProp == null) return null;
                var ht = cpProp.GetValue(room, null) as System.Collections.Hashtable;
                return (ht != null && ht.ContainsKey(key)) ? ht[key] as string : null;
            }
            catch { return null; }
        }

        private string GetPlayerProp(object player, string key)
        {
            try
            {
                PropertyInfo cpProp = player.GetType().GetProperty("customProperties",
                    BindingFlags.Instance | BindingFlags.Public);
                if (cpProp == null) return null;
                var ht = cpProp.GetValue(player, null) as System.Collections.Hashtable;
                return (ht != null && ht.ContainsKey(key)) ? ht[key] as string : null;
            }
            catch { return null; }
        }

        private int GetIntProperty(object obj, string name)
        {
            try
            {
                PropertyInfo p = obj.GetType().GetProperty(name,
                    BindingFlags.Instance | BindingFlags.Public);
                if (p != null) return (int)p.GetValue(obj, null);
            }
            catch { }
            return -1;
        }

        private bool VersionCompatible(string other)
        {
            try   // Only major version must match
            {
                int myMajor    = int.Parse(_modVersion.Trim().Split('.')[0]);
                int theirMajor = int.Parse(other.Trim().Split('.')[0]);
                return myMajor == theirMajor;
            }
            catch { return true; } // lenient on parse failure
        }

        // â”€â”€ Photon server override (existing redirect logic) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void ApplyPhotonOverride()
        {
            try
            {
                Type pnt = GetPhotonNetType();
                if (pnt == null) { Debug.LogError("[IPRedirect] PhotonNetwork type not found"); return; }

                object settings = GetPhotonServerSettings(pnt);
                if (settings == null) { Log("PhotonServerSettings null â€” retrying next frame"); return; }

                Type st = settings.GetType();
                SetField(st, settings, "ServerAddress", _serverIp);
                SetField(st, settings, "ServerPort",    _serverPort);
                SetField(st, settings, "AppID",         _appId);
                SetField(st, settings, "HostType",      (int)2);

                _overrideApplied = true;
                Debug.Log("[IPRedirect] Photon â†’ " + (_serverIp) + ":" + (_serverPort) + "  AppID=" + (_appId));
                Log("Override applied: " + (_serverIp) + ":" + (_serverPort) + "  AppID=" + (_appId));
                ForceReconnectIfNeeded(pnt);
            }
            catch (Exception ex)
            {
                Log("ApplyPhotonOverride error: " + (ex));
                Debug.LogError("[IPRedirect] ApplyPhotonOverride error: " + ex);
            }
        }

        private static void ForceReconnectIfNeeded(Type pnt)
        {
            try
            {
                PropertyInfo connProp = pnt.GetProperty("connected",
                    BindingFlags.Static | BindingFlags.Public);
                if (connProp != null && (bool)connProp.GetValue(null, null))
                {
                    Log("Photon was connected â€” disconnecting to apply LAN server");
                    MethodInfo di = pnt.GetMethod("Disconnect",
                        BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
                    if (di != null) di.Invoke(null, null);
                }
            }
            catch (Exception ex) { Log("ForceReconnect error: " + (ex.Message)); }
        }

        // â”€â”€ Reflection helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static Type GetPhotonNetType()
        {
            if (_photonNetType != null) return _photonNetType;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("PhotonNetwork");
                if (t != null) { _photonNetType = t; return t; }
            }
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

        private static object GetPhotonServerSettings(Type pnt)
        {
            PropertyInfo prop = pnt.GetProperty("PhotonServerSettings",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null) return prop.GetValue(null, null);
            FieldInfo field = pnt.GetField("PhotonServerSettings",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field.GetValue(null);
            Log("Could not find PhotonServerSettings");
            return null;
        }

        private static void SetField(Type t, object inst, string name, object val)
        {
            try
            {
                FieldInfo f = t.GetField(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null)
                {
                    object conv = Convert.ChangeType(val,
                        f.FieldType.IsEnum ? Enum.GetUnderlyingType(f.FieldType) : f.FieldType);
                    if (f.FieldType.IsEnum) conv = Enum.ToObject(f.FieldType, conv);
                    f.SetValue(inst, conv);
                    Log("  Set " + (name) + " = " + (conv));
                }
                else
                {
                    PropertyInfo p = t.GetProperty(name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (p != null && p.CanWrite)
                    {
                        p.SetValue(inst, Convert.ChangeType(val, p.PropertyType), null);
                        Log("  Set prop " + (name) + " = " + (val));
                    }
                    else Log("  WARNING: '" + (name) + "' not found on " + (t.Name));
                }
            }
            catch (Exception ex) { Log("  SetField(" + (name) + ") error: " + (ex.Message)); }
        }

        // â”€â”€ Config loader â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                Log("Config not found: " + (ConfigPath));
                Log("Keys: SERVER_IP, SERVER_PORT, APP_ID, MAP_URL, MOD_VERSION, KICK_NO_MOD");
                return;
            }

            Log("Reading " + (ConfigPath));
            foreach (string line in File.ReadAllLines(ConfigPath))
            {
                string t = line.Trim();
                if (string.IsNullOrEmpty(t) || t.StartsWith("#")) continue;
                int eq = t.IndexOf('=');
                if (eq < 0) continue;
                string key = t.Substring(0, eq).Trim().ToUpperInvariant();
                string val = t.Substring(eq + 1).Trim();
                switch (key)
                {
                    case "SERVER_IP":
                        _serverIp = val;
                        Log("  SERVER_IP   = " + (val));
                        break;
                    case "SERVER_PORT":
                        int p; if (int.TryParse(val, out p)) _serverPort = p;
                        Log("  SERVER_PORT = " + (_serverPort));
                        break;
                    case "APP_ID":
                        _appId = val;
                        Log("  APP_ID      = " + (val));
                        break;
                    case "MAP_URL":
                        _mapUrl = val;
                        Log("  MAP_URL     = " + (val));
                        break;
                    case "MOD_VERSION":
                        _modVersion = val;
                        Log("  MOD_VERSION = " + (val));
                        break;
                    case "KICK_NO_MOD":
                        _kickNoMod = val.ToLower() != "false" && val != "0";
                        Log("  KICK_NO_MOD = " + (_kickNoMod));
                        break;
                }
            }
        }

        // â”€â”€ Logger â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); }
            catch { /* filesystem may not be ready */ }
            Debug.Log("[IPRedirect] " + (msg));
        }
    }
}
