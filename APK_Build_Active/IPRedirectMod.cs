using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRMods
{
    /// <summary>
    /// IP Redirect Mod — overrides Photon server to point at the custom LAN server.
    /// Entry point: CNRMods.ModEntry.Load() — called by the patched Extensions static ctor.
    /// Config: /storage/emulated/0/CNRMods/server.cfg  (SERVER_IP=x.x.x.x)
    /// </summary>
    public class ModEntry
    {
        private const string LogPath    = "/storage/emulated/0/CNRMods/redir.log";
        private const string ConfigPath = "/storage/emulated/0/CNRMods/server.cfg";

        public static void Load()
        {
            Log("=== IPRedirectMod Load() ===");
            try
            {
                string ip = ReadServerIp();
                if (string.IsNullOrEmpty(ip))
                {
                    Log("No SERVER_IP in server.cfg — aborting");
                    return;
                }

                Log("Creating redirect MonoBehaviour for IP: " + ip);
                var go = new GameObject("CNRIPRedirect");
                var hook = go.AddComponent<RedirectHook>();
                hook.ServerIp = ip;
                GameObject.DontDestroyOnLoad(go);
            }
            catch (Exception ex)
            {
                Log("Load() exception: " + ex);
            }

            // Initialise additional mods bundled into this DLL
            CustomMapsEntry.Initialize();
        }

        private static string ReadServerIp()
        {
            if (!File.Exists(ConfigPath))
            {
                Log("server.cfg not found at " + ConfigPath);
                return null;
            }
            foreach (string line in File.ReadAllLines(ConfigPath))
            {
                string t = line.Trim();
                if (t.StartsWith("SERVER_IP="))
                    return t.Substring("SERVER_IP=".Length).Trim();
            }
            return null;
        }

        public static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); }
            catch { }
            try { Debug.Log("[IPRedirect] " + msg); } catch { }
        }
    }

    public class RedirectHook : MonoBehaviour
    {
        public string ServerIp = "";

        // Overlay state — set by ShowOverlay(), rendered in OnGUI(), faded by FadeOverlay()
        private string _overlayMessage = null;
        private float  _overlayOpacity = 0f;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        // Update() real-time heartbeat — logged 5 times then stops
        private DateTime _lastUpdateLog = DateTime.MinValue;
        private int _updateLogCount = 0;

        private void Start()
        {
            Application.runInBackground = true;
            _lastUpdateLog = DateTime.Now;
            ModEntry.Log("Start() called — starting RedirectCoroutine");
            StartCoroutine(RedirectCoroutine());
        }

        private void Update()
        {
            if (_updateLogCount >= 5) return;
            if ((DateTime.Now - _lastUpdateLog).TotalSeconds >= 3.0)
            {
                _updateLogCount++;
                ModEntry.Log("[Update tick " + _updateLogCount + "] deltaTime=" + Time.deltaTime.ToString("F4")
                    + " unscaled=" + Time.unscaledDeltaTime.ToString("F4")
                    + " realTime=" + Time.realtimeSinceStartup.ToString("F1")
                    + " timescale=" + Time.timeScale.ToString("F2"));
                _lastUpdateLog = DateTime.Now;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            ModEntry.Log("OnApplicationPause: " + paused);
        }

        private void OnDestroy()
        {
            ModEntry.Log("OnDestroy — MonoBehaviour destroyed");
        }

        private void ShowOverlay(string message)
        {
            ModEntry.Log("OVERLAY: " + message.Replace("\n", " | "));
            _overlayMessage = message;
            _overlayOpacity = 1f;
            StartCoroutine(FadeOverlay());
        }

        private IEnumerator FadeOverlay()
        {
            // Stay fully visible for 6 seconds, then fade out over 4 seconds
            yield return new WaitForSeconds(6f);
            float t = 4f;
            while (t > 0f)
            {
                _overlayOpacity = t / 4f;
                t -= Time.deltaTime;
                yield return null;
            }
            _overlayMessage = null;
            _overlayOpacity  = 0f;
        }

        private void OnGUI()
        {
            if (_overlayMessage == null || _overlayOpacity <= 0f) return;

            // Semi-transparent dark band at the top of the screen
            GUI.color = new Color(0f, 0f, 0f, 0.75f * _overlayOpacity);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, 140f), Texture2D.whiteTexture);

            // Red label text
            GUI.color = new Color(1f, 0.35f, 0.35f, _overlayOpacity);
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize  = Mathf.Max(22, Screen.width / 22);
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap  = true;
            GUI.Label(new Rect(20f, 8f, Screen.width - 40f, 124f),
                      "[CNR-Mod] " + _overlayMessage, style);

            GUI.color = Color.white;
        }

        private IEnumerator RedirectCoroutine()
        {
            // Step 1: wait until PhotonServerSettings object is available
            object settings = null;
            while (settings == null)
            {
                settings = GetPhotonServerSettings();
                if (settings == null) yield return null;
            }

            // Step 2: write our server into PhotonServerSettings
            Type t = settings.GetType();
            SetMember(t, settings, "ServerAddress", ServerIp);
            SetMember(t, settings, "ServerPort",    5055);
            SetMember(t, settings, "AppID",         "CNRLan");
            SetMember(t, settings, "HostType",      2); // SelfHosted
            ModEntry.Log("Override applied → " + ServerIp + ":5055  AppID=CNRLan");

            // Step 3: disconnect unconditionally (handles connecting + connected states)
            CallStaticVoid("PhotonNetwork", "Disconnect");
            ModEntry.Log("Disconnect() called");

            // Step 4: wait until connectionState == 0 (Disconnected)
            float timeout = 8f;
            while (timeout > 0f)
            {
                int state = GetConnectionState();
                if (state == 0) break;
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
            ModEntry.Log("Photon disconnected, swapping to TCP then reconnecting");

            // Step 5: swap the underlying socket from UDP (EnetPeer) to TCP (TPeer)
            // The game initialises networkingPeer with ConnectionProtocol.Udp (0), but our server
            // listens on TCP.  We replace peerBase inside the existing networkingPeer with a fresh
            // TPeer so the subsequent Connect() goes over TCP without rebuilding any other state.
            SwapToTcp();

            // Step 5b: disable encryption so the client calls OpAuthenticate directly
            // requestSecurity=true (default) causes EstablishEncryption() to be called instead of
            // OpAuthenticate() when OnStatusChanged(Connect) fires — our LAN server has no DH handler.
            DisableEncryption();

            // Step 6: reconnect using our overridden settings
            // Log BEFORE the call so we can tell if the call itself is what kills the coroutine
            ModEntry.Log("About to call ConnectUsingSettings...");
            bool connectCallSucceeded = false;
            try
            {
                CallStaticWithArg("PhotonNetwork", "ConnectUsingSettings", "v2.4");
                connectCallSucceeded = true;
                ModEntry.Log("ConnectUsingSettings() returned OK → connecting to " + ServerIp + ":5055");
            }
            catch (Exception ex)
            {
                ModEntry.Log("ConnectUsingSettings() THREW: " + ex.Message + "\n" + ex.StackTrace);
            }

            if (!connectCallSucceeded)
            {
                ModEntry.Log("ERROR: ConnectUsingSettings failed — giving up");
                ShowOverlay("LAN connect call FAILED.\n" + ServerIp + ":5055\nSee /CNRMods/redir.log");
                yield break;
            }

            // Step 7: monitor connection state for up to 30 seconds
            ModEntry.Log("Monitoring connection state (30s timeout)...");
            float connectTimeout = 30f;
            float heartbeatTimer = 0f;
            int lastDetailed = -999;
            while (connectTimeout > 0f)
            {
                int connState   = -1;
                int detailState = -1;
                try { connState   = GetConnectionState(); } catch (Exception ex) { ModEntry.Log("GetConnectionState threw: " + ex.Message); }
                try { detailState = GetDetailedState();   } catch (Exception ex) { ModEntry.Log("GetDetailedState threw: " + ex.Message); }
                if (detailState != lastDetailed)
                {
                    ModEntry.Log("connState=" + connState + "  detailState=" + detailState
                        + "  (" + (30f - connectTimeout).ToString("F1") + "s elapsed)"
                        + "  runInBg=" + Application.runInBackground);
                    lastDetailed = detailState;
                }
                // Heartbeat log every 3 seconds
                heartbeatTimer -= Time.unscaledDeltaTime;
                if (heartbeatTimer <= 0f)
                {
                    ModEntry.Log("[heartbeat] connState=" + connState + "  detailState=" + detailState
                        + "  (" + (30f - connectTimeout).ToString("F1") + "s elapsed)"
                        + "  runInBg=" + Application.runInBackground
                        + "  dt=" + Time.unscaledDeltaTime.ToString("F4"));
                    heartbeatTimer = 3f;
                }
                if (detailState == 0 || connState == 0)
                {
                    ModEntry.Log("Connection FAILED — Disconnected after " + (30f - connectTimeout).ToString("F1") + "s");
                    ShowOverlay("LAN server unreachable.\n" + ServerIp + ":5055\nCheck server is running.");
                    yield break;
                }
                // detailState >= 6 means JoinedLobby or further — master connection is fully up
                if (detailState >= 6)
                {
                    ModEntry.Log("Connected to lobby! detailState=" + detailState + " after " + (30f - connectTimeout).ToString("F1") + "s");
                    yield break;
                }
                connectTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            // Timeout — force disconnect so game is not stuck in Connecting limbo
            ModEntry.Log("Connection TIMED OUT after 30s (last detailState=" + lastDetailed + ") — forcing Disconnect()");
            CallStaticVoid("PhotonNetwork", "Disconnect");
            ShowOverlay("LAN connection timed out.\n" + ServerIp + ":5055\ndetailState was " + lastDetailed + " — is server running?");
        }

        // ---------------------------------------------------------------- helpers

        private static void DisableEncryption()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type pn = asm.GetType("PhotonNetwork");
                if ((object)pn == null) continue;
                FieldInfo fPeer = pn.GetField("networkingPeer",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if ((object)fPeer == null) continue;
                object peer = fPeer.GetValue(null);
                if ((object)peer == null) continue;
                FieldInfo fSec = peer.GetType().GetField("requestSecurity",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if ((object)fSec != null)
                {
                    fSec.SetValue(peer, false);
                    ModEntry.Log("requestSecurity = false → will use direct OpAuthenticate (no DH)");
                    return;
                }
            }
            ModEntry.Log("WARNING: requestSecurity field not found — encryption may block auth");
        }

        private static object GetPhotonServerSettings()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type pn = asm.GetType("PhotonNetwork");
                if ((object)pn == null) continue;
                PropertyInfo p = pn.GetProperty("PhotonServerSettings",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if ((object)p != null) { object r = p.GetValue(null, null); if (r != null) return r; }
                FieldInfo f = pn.GetField("PhotonServerSettings",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if ((object)f != null) { object r = f.GetValue(null); if (r != null) return r; }
            }
            return null;
        }

        private static int GetConnectionState()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type pn = asm.GetType("PhotonNetwork");
                if ((object)pn == null) continue;
                PropertyInfo p = pn.GetProperty("connectionState",
                    BindingFlags.Static | BindingFlags.Public);
                if ((object)p != null)
                {
                    object v = p.GetValue(null, null);
                    return Convert.ToInt32(v);
                }
                FieldInfo f = pn.GetField("connectionState",
                    BindingFlags.Static | BindingFlags.Public);
                if ((object)f != null)
                    return Convert.ToInt32(f.GetValue(null));
            }
            return -1;
        }

        // connectionStateDetailed returns NetworkingPeer.State (PeerState enum):
        //   Disconnected=0, PeerCreated=1, Connecting=2, Connected=3,
        //   Queued=4, Authenticated=5, JoinedLobby=6, ConnectingToGameserver=7,
        //   Joined=8, Leaving=9, Left=10, ...
        private static int GetDetailedState()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type pn = asm.GetType("PhotonNetwork");
                if ((object)pn == null) continue;
                PropertyInfo p = pn.GetProperty("connectionStateDetailed",
                    BindingFlags.Static | BindingFlags.Public);
                if ((object)p != null)
                {
                    object v = p.GetValue(null, null);
                    return Convert.ToInt32(v);
                }
            }
            return -1;
        }

        private static void CallStaticVoid(string typeName, string methodName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(typeName);
                if ((object)t == null) continue;
                MethodInfo m = t.GetMethod(methodName,
                    BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
                if ((object)m != null) { m.Invoke(null, null); return; }
            }
            ModEntry.Log("CallStaticVoid: " + typeName + "." + methodName + " not found");
        }

        private static void CallStaticWithArg(string typeName, string methodName, object arg)
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = asm.GetType(typeName);
                    if ((object)t == null) continue;
                    foreach (MethodInfo m in t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        if (m.Name != methodName) continue;
                        ParameterInfo[] prms = m.GetParameters();
                        if (prms.Length != 1) continue;
                        if (!prms[0].ParameterType.IsAssignableFrom(arg.GetType())) continue;
                        m.Invoke(null, new object[] { arg });
                        return;
                    }
                }
                ModEntry.Log("CallStaticWithArg: " + typeName + "." + methodName + " not found");
            }
            catch (Exception ex) { ModEntry.Log("CallStaticWithArg error: " + ex.Message); }
        }

        private static void SwapToTcp()
        {
            try
            {
                // 1. Get PhotonNetwork.networkingPeer
                Type pnType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    pnType = asm.GetType("PhotonNetwork");
                    if ((object)pnType != null) break;
                }
                if ((object)pnType == null) { ModEntry.Log("SwapToTcp: PhotonNetwork not found"); return; }

                FieldInfo peerFieldPN = pnType.GetField("networkingPeer",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if ((object)peerFieldPN == null) { ModEntry.Log("SwapToTcp: networkingPeer field not found"); return; }
                object peer = peerFieldPN.GetValue(null);
                if (peer == null) { ModEntry.Log("SwapToTcp: networkingPeer is null"); return; }

                // 2. Find peerBase field by walking up type hierarchy
                FieldInfo peerBaseField = null;
                Type search = peer.GetType();
                while ((object)search != null)
                {
                    peerBaseField = search.GetField("peerBase",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if ((object)peerBaseField != null) break;
                    search = search.BaseType;
                }
                if ((object)peerBaseField == null) { ModEntry.Log("SwapToTcp: peerBase field not found"); return; }

                object oldBase = peerBaseField.GetValue(peer);
                if ((object)oldBase != null)
                    ModEntry.Log("SwapToTcp: current peerBase is " + oldBase.GetType().Name);

                // 3. Find TPeer type in loaded assemblies (namespace ExitGames.Client.Photon)
                Type tpeerType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    tpeerType = asm.GetType("ExitGames.Client.Photon.TPeer");
                    if ((object)tpeerType == null) tpeerType = asm.GetType("TPeer");
                    if ((object)tpeerType != null) break;
                }
                if ((object)tpeerType == null) { ModEntry.Log("SwapToTcp: TPeer type not found"); return; }

                // 4. Instantiate a new TPeer (internal constructor, nonPublic=true)
                object newTPeer = Activator.CreateInstance(tpeerType, true);

                // 5. Set usedProtocol = Tcp (byte 1) on the new TPeer
                Type baseSearch = tpeerType;
                while ((object)baseSearch != null)
                {
                    FieldInfo upf = baseSearch.GetField("usedProtocol",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if ((object)upf != null)
                    {
                        upf.SetValue(newTPeer, Enum.ToObject(upf.FieldType, (byte)1));
                        ModEntry.Log("SwapToTcp: usedProtocol = Tcp");
                        break;
                    }
                    baseSearch = baseSearch.BaseType;
                }

                // 6. Set the Listener on newTPeer = networkingPeer (which implements IPhotonPeerListener).
                //    The Listener property in PeerBase is auto-generated; the backing field is
                //    '<Listener>k__BackingField' in Mono.  We try the property setter first,
                //    then the known backing-field names, so we don't depend on the exact name.
                bool listenerSet = false;
                // 6a. Try property setter
                {
                    Type ps = tpeerType;
                    while ((object)ps != null)
                    {
                        PropertyInfo lp = ps.GetProperty("Listener",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if ((object)lp != null)
                        {
                            MethodInfo setter = lp.GetSetMethod(true); // true = include non-public
                            if ((object)setter != null)
                            {
                                setter.Invoke(newTPeer, new object[] { peer });
                                ModEntry.Log("SwapToTcp: Listener set via property setter = " + peer.GetType().Name);
                                listenerSet = true;
                            }
                            break;
                        }
                        ps = ps.BaseType;
                    }
                }
                // 6b. Fall back: look for known backing field names
                if (!listenerSet)
                {
                    string[] candidateFields = new string[]
                    {
                        "<Listener>k__BackingField",
                        "listener",
                        "_listener",
                        "Listener"
                    };
                    Type bfs = tpeerType;
                    while ((object)bfs != null && !listenerSet)
                    {
                        foreach (string fn in candidateFields)
                        {
                            FieldInfo bf = bfs.GetField(fn,
                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                            if ((object)bf != null)
                            {
                                bf.SetValue(newTPeer, peer);
                                ModEntry.Log("SwapToTcp: Listener set via field '" + fn + "' = " + peer.GetType().Name);
                                listenerSet = true;
                                break;
                            }
                        }
                        if (!listenerSet) bfs = bfs.BaseType;
                    }
                }
                if (!listenerSet)
                    ModEntry.Log("SwapToTcp: WARNING — could not find Listener property/field on TPeer hierarchy!");

                // 8. Swap peerBase in networkingPeer
                peerBaseField.SetValue(peer, newTPeer);
                ModEntry.Log("SwapToTcp: peerBase swapped to TPeer - client will connect via TCP");
            }
            catch (Exception ex) { ModEntry.Log("SwapToTcp error: " + ex.Message + "\n" + ex.StackTrace); }
        }

        private static void SetMember(Type t, object inst, string name, object val)
        {
            FieldInfo f = t.GetField(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if ((object)f != null)
            {
                object v = f.FieldType.IsEnum
                    ? Enum.ToObject(f.FieldType, Convert.ChangeType(val, Enum.GetUnderlyingType(f.FieldType)))
                    : Convert.ChangeType(val, f.FieldType);
                f.SetValue(inst, v);
                ModEntry.Log("  " + name + " = " + v);
                return;
            }
            PropertyInfo p = t.GetProperty(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if ((object)p != null && p.CanWrite)
            {
                p.SetValue(inst, Convert.ChangeType(val, p.PropertyType), null);
                ModEntry.Log("  " + name + " = " + val);
            }
        }
    }
}
