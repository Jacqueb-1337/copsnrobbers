using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRMods
{
    /// <summary>
    /// CNR LAN IP Redirect Mod
    ///
    /// Reads /storage/emulated/0/CNRMods/server.cfg and overrides Photon server
    /// settings so the game connects to the custom LAN server instead of the
    /// official Photon cloud.
    ///
    /// server.cfg format (plain text, one key=value per line):
    ///   SERVER_IP=192.168.1.10
    ///   SERVER_PORT=5055
    ///   APP_ID=CNRLan
    ///
    /// How to deploy:
    ///   adb push CNRIPRedirectMod.dll /storage/emulated/0/CNRMods/CNRIPRedirectMod.dll
    ///   echo "SERVER_IP=192.168.1.10" > /sdcard/CNRMods/server.cfg
    /// </summary>
    public class CNRIPRedirectMod : MonoBehaviour
    {
        private const string ConfigPath = "/storage/emulated/0/CNRMods/server.cfg";
        private const string LogPath    = "/storage/emulated/0/CNRMods/ipredir.log";

        private static bool   _initialized = false;
        private static string _serverIp    = "";
        private static int    _serverPort  = 5055;
        private static string _appId       = "CNRLan";

        // ── Called by CNRModLoader on startup ─────────────────────────────────
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            Log("=== CNRIPRedirectMod Initialize() ===");

            try
            {
                LoadConfig();

                if (string.IsNullOrEmpty(_serverIp))
                {
                    Log("No SERVER_IP in server.cfg — mod disabled.");
                    return;
                }

                // Create persistent MonoBehaviour to re-apply settings each frame until connected
                var go = new GameObject("CNRIPRedirect");
                go.AddComponent<CNRIPRedirectMod>();
                DontDestroyOnLoad(go);

                Log($"Mod object created. Will redirect to {_serverIp}:{_serverPort}");
            }
            catch (Exception ex)
            {
                Log($"Initialize() error: {ex}");
            }
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            Log("Awake() — applying Photon server override");
            ApplyPhotonOverride();
        }

        // Re-check every 30 frames — SwitchToServer() can reset the address
        private int _updateTick = 0;
        private void Update()
        {
            _updateTick++;
            if (_updateTick % 30 != 0) return;
            ApplyPhotonOverride();
        }

        // ── Core redirect logic ────────────────────────────────────────────────
        private void ApplyPhotonOverride()
        {
            try
            {
                Type photonNetType = GetPhotonNetworkType();
                if (photonNetType == null) return;

                object settings = GetPhotonServerSettings(photonNetType);
                if (settings == null) return;

                Type settingsType = settings.GetType();

                // Check if address was changed away from ours (e.g. by SwitchToServer)
                FieldInfo addrField = settingsType.GetField("ServerAddress",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                string currentAddr = addrField != null ? addrField.GetValue(settings) as string : null;
                bool wasWrong = (currentAddr != _serverIp);

                if (!wasWrong) return; // already pointing at our server, nothing to do

                Log($"Address was '{currentAddr}', reapplying override → {_serverIp}:{_serverPort}");

                SetField(settingsType, settings, "ServerAddress", _serverIp);
                SetField(settingsType, settings, "ServerPort",    _serverPort);
                SetField(settingsType, settings, "AppID",         _appId);
                SetField(settingsType, settings, "HostType",      (int)2);

                Debug.Log($"[IPRedirect] Reapplied: {_serverIp}:{_serverPort}");

                // Force reconnect so the SDK uses our server
                ForceReconnectIfNeeded(photonNetType);
            }
            catch (Exception ex)
            {
                Log($"ApplyPhotonOverride error: {ex}");
                Debug.LogError("[IPRedirect] ApplyPhotonOverride error: " + ex);
            }
        }

        private static void ForceReconnectIfNeeded(Type photonNetType)
        {
            try
            {
                // Check PhotonNetwork.connected
                PropertyInfo connProp = photonNetType.GetProperty("connected",
                    BindingFlags.Static | BindingFlags.Public);
                if (connProp != null)
                {
                    bool isConnected = (bool)connProp.GetValue(null, null);
                    if (isConnected)
                    {
                        Log("Photon was connected — disconnecting to force reconnect to LAN server");
                        MethodInfo discMethod = photonNetType.GetMethod("Disconnect",
                            BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
                        discMethod?.Invoke(null, null);
                    }
                }
            }
            catch (Exception ex) { Log($"ForceReconnect error: {ex.Message}"); }
        }

        // ── Reflection helpers ────────────────────────────────────────────────
        private static Type GetPhotonNetworkType()
        {
            // Try direct type lookup first
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("PhotonNetwork");
                if (t != null) return t;
            }
            return null;
        }

        private static object GetPhotonServerSettings(Type photonNetType)
        {
            // Try as a static property
            PropertyInfo prop = photonNetType.GetProperty("PhotonServerSettings",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null) return prop.GetValue(null, null);

            // Try as a static field
            FieldInfo field = photonNetType.GetField("PhotonServerSettings",
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
                    // Convert int to the field's actual type (e.g., an enum or int)
                    object converted = Convert.ChangeType(val, f.FieldType.IsEnum
                        ? Enum.GetUnderlyingType(f.FieldType)
                        : f.FieldType);

                    if (f.FieldType.IsEnum)
                        converted = Enum.ToObject(f.FieldType, converted);

                    f.SetValue(inst, converted);
                    Log($"  Set {name} = {converted}");
                }
                else
                {
                    PropertyInfo p = t.GetProperty(name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (p != null && p.CanWrite)
                    {
                        object converted = Convert.ChangeType(val, p.PropertyType);
                        p.SetValue(inst, converted, null);
                        Log($"  Set property {name} = {converted}");
                    }
                    else
                    {
                        Log($"  WARNING: field/property '{name}' not found on {t.Name}");
                    }
                }
            }
            catch (Exception ex) { Log($"  SetField({name}) error: {ex.Message}"); }
        }

        // ── Config reader ────────────────────────────────────────────────────
        private static void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                Log($"Config not found at {ConfigPath}");
                Log("Create it with: SERVER_IP=<your-lan-ip>");
                return;
            }

            Log($"Reading config from {ConfigPath}");
            foreach (string line in File.ReadAllLines(ConfigPath))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                int eq = trimmed.IndexOf('=');
                if (eq < 0) continue;

                string key = trimmed.Substring(0, eq).Trim().ToUpperInvariant();
                string val = trimmed.Substring(eq + 1).Trim();

                switch (key)
                {
                    case "SERVER_IP":
                        _serverIp   = val;
                        Log($"  SERVER_IP  = {_serverIp}");
                        break;
                    case "SERVER_PORT":
                        if (int.TryParse(val, out int p)) _serverPort = p;
                        Log($"  SERVER_PORT = {_serverPort}");
                        break;
                    case "APP_ID":
                        _appId = val;
                        Log($"  APP_ID     = {_appId}");
                        break;
                }
            }
        }

        // ── Logger ───────────────────────────────────────────────────────────
        private static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss}] {msg}\n"); }
            catch { /* filesystem might not be ready */ }
            Debug.Log($"[IPRedirect] {msg}");
        }
    }
}
