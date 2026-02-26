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

        // Scene-aware connection state
        private bool     _inConnectScene   = false;
        private Coroutine _connectCoroutine = null;

        // "Connect" scenes: multiplayer lobby and any in-game scene
        private static bool IsConnectScene(string name)
        {
            return name == "MultiplayerSelect" || name.StartsWith("CRScene");
        }

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            Application.runInBackground = true;
            string scene = Application.loadedLevelName;
            ModEntry.Log("Start() — scene=" + scene);
            if (IsConnectScene(scene))
            {
                _inConnectScene   = true;
                _connectCoroutine = StartCoroutine(ConnectWithRetry());
            }
        }

        // Fires after all Awake()/Start() in the newly loaded scene — including CNRConnectMenu.Awake()
        // which calls SwitchToServer().  By the time we get here that reset has already happened,
        // so applying our settings now will win.
        private void OnLevelWasLoaded(int level)
        {
            string scene = Application.loadedLevelName;
            ModEntry.Log("OnLevelWasLoaded: scene=" + scene + " level=" + level);
            bool shouldConnect = IsConnectScene(scene);

            if (shouldConnect && !_inConnectScene)
            {
                ModEntry.Log("Entered connect scene — starting LAN connection");
                _inConnectScene = true;
                if (_connectCoroutine != null) { StopCoroutine(_connectCoroutine); _connectCoroutine = null; }
                _connectCoroutine = StartCoroutine(ConnectWithRetry());
            }
            else if (scene == "MainMenu" && _inConnectScene)
            {
                // Only disconnect when explicitly back on the main menu.
                // Do NOT disconnect for loading screens or other intermediate scenes —
                // those fire between MultiplayerSelect and CRScene* during room join
                // and would kill the game-server connection mid-handoff.
                ModEntry.Log("Returned to MainMenu — stopping coroutine and disconnecting");
                _inConnectScene = false;
                if (_connectCoroutine != null) { StopCoroutine(_connectCoroutine); _connectCoroutine = null; }
                try
                {
                    int cs = GetConnectionState();
                    if (cs != 0)
                    {
                        ModEntry.Log("Disconnecting from LAN server (main menu)");
                        CallStaticVoid("PhotonNetwork", "Disconnect");
                        ShowOverlay("Left multiplayer — LAN disconnected");
                    }
                }
                catch (Exception ex) { ModEntry.Log("Disconnect-on-leave error: " + ex.Message); }
            }
            else
            {
                ModEntry.Log("Scene '" + scene + "' is not a connect scene and not MainMenu — leaving connection untouched");
            }
        }

        // ---- Retry loop — retries every 5 s until in a connect scene and connected ----

        private IEnumerator ConnectWithRetry()
        {
            ModEntry.Log("ConnectWithRetry: started");
            while (_inConnectScene)
            {
                ShowOverlay("Connecting to LAN server...\n" + ServerIp + ":5055");

                bool connected = false;
                yield return StartCoroutine(TryConnect(r => connected = r));

                if (!_inConnectScene) break;

                if (connected)
                {
                    ModEntry.Log("ConnectWithRetry: connected! Monitoring...");
                    ShowOverlay("Connected to LAN server  " + ServerIp);

                    // Monitor connection until it drops or we leave the scene
                    while (_inConnectScene)
                    {
                        // While in a game scene, Photon manages the game-server connection
                        // internally.  Don't poll detailState here — it may transiently hit 0
                        // during the master→game-server handoff and trigger a spurious reconnect
                        // that would Disconnect() right after the server sends the join response.
                        string curScene = Application.loadedLevelName;
                        if (curScene.StartsWith("CRScene"))
                        {
                            yield return new WaitForSeconds(1f);
                            continue;
                        }

                        int ds = -1;
                        try { ds = GetDetailedState(); } catch { }
                        if (ds == 0 || ds == -1)
                        {
                            ModEntry.Log("ConnectWithRetry: connection lost (detailState=" + ds + ") — retry in 5s");
                            ShowOverlay("LAN connection lost.  Retrying in 5 s...");
                            break;
                        }
                        yield return null;
                    }
                    if (!_inConnectScene) break;
                    yield return new WaitForSeconds(5f);
                }
                else
                {
                    if (!_inConnectScene) break;
                    ModEntry.Log("ConnectWithRetry: attempt failed — retry in 5s");
                    ShowOverlay("LAN server unreachable.  Retrying in 5 s...\n" + ServerIp + ":5055");
                    yield return new WaitForSeconds(5f);
                }
            }
            _connectCoroutine = null;
            ModEntry.Log("ConnectWithRetry: loop exited (left connect scene)");
        }

        // Single connection attempt.  Calls callback(true) on success, callback(false) on failure.
        private IEnumerator TryConnect(Action<bool> callback)
        {
            ModEntry.Log("TryConnect: starting");

            // Step 1 — wait for PhotonServerSettings
            float settingsWait = 10f;
            while (settingsWait > 0f)
            {
                if (!_inConnectScene) { callback(false); yield break; }
                if (GetPhotonServerSettings() != null) break;
                yield return new WaitForSeconds(0.5f);
                settingsWait -= 0.5f;
            }
            if (GetPhotonServerSettings() == null)
            {
                ModEntry.Log("TryConnect: PhotonServerSettings never appeared");
                callback(false);
                yield break;
            }

            // Step 2 — apply server settings
            try
            {
                object s = GetPhotonServerSettings();
                Type   t = s.GetType();
                SetMember(t, s, "ServerAddress", ServerIp);
                SetMember(t, s, "ServerPort",    5055);
                SetMember(t, s, "AppID",         "CNRLan");
                SetMember(t, s, "HostType",      2); // SelfHosted
                ModEntry.Log("TryConnect: settings written → " + ServerIp + ":5055");
            }
            catch (Exception ex) { ModEntry.Log("TryConnect: SetMember error: " + ex.Message); callback(false); yield break; }

            // Step 3 — disconnect (clears any existing cloud session)
            try { CallStaticVoid("PhotonNetwork", "Disconnect"); } catch { }
            yield return new WaitForSeconds(0.3f);

            float disconnWait = 8f;
            while (disconnWait > 0f)
            {
                if (!_inConnectScene) { callback(false); yield break; }
                int cs = -1;
                try { cs = GetConnectionState(); } catch { }
                if (cs == 0) break;
                yield return new WaitForSeconds(0.2f);
                disconnWait -= 0.2f;
            }

            if (!_inConnectScene) { callback(false); yield break; }

            // Step 4 — configure TCP transport and disable encryption
            try { SwapToTcp();        } catch (Exception ex) { ModEntry.Log("SwapToTcp error: " + ex.Message); }
            try { DisableEncryption();} catch (Exception ex) { ModEntry.Log("DisableEncryption error: " + ex.Message); }

            // Step 5 — re-apply settings (SwitchToServer may have run in Awake; writing after
            //           disconnect ensures our values survive into ConnectUsingSettings)
            try
            {
                object s = GetPhotonServerSettings();
                Type   t = s.GetType();
                SetMember(t, s, "ServerAddress", ServerIp);
                SetMember(t, s, "ServerPort",    5055);
                SetMember(t, s, "AppID",         "CNRLan");
                SetMember(t, s, "HostType",      2);
                ModEntry.Log("TryConnect: settings re-applied");
            }
            catch (Exception ex) { ModEntry.Log("TryConnect: re-apply error: " + ex.Message); }

            // Step 6 — connect
            try
            {
                CallStaticWithArg("PhotonNetwork", "ConnectUsingSettings", "v2.4");
                ModEntry.Log("TryConnect: ConnectUsingSettings → " + ServerIp + ":5055");
            }
            catch (Exception ex)
            {
                ModEntry.Log("TryConnect: ConnectUsingSettings threw: " + ex.Message);
                callback(false);
                yield break;
            }

            // Step 7 — wait up to 30 s for detailState >= 6 (JoinedLobby)
            float connectTimeout = 30f;
            float heartbeat      = 3f;
            int   lastDs         = -999;
            while (connectTimeout > 0f)
            {
                if (!_inConnectScene) { callback(false); yield break; }
                int connState = -1, ds = -1;
                try { connState = GetConnectionState(); } catch { }
                try { ds        = GetDetailedState();   } catch { }

                if (ds != lastDs)
                {
                    ModEntry.Log("TryConnect: connState=" + connState + " detailState=" + ds
                        + " (" + (30f - connectTimeout).ToString("F1") + "s)");
                    lastDs = ds;
                }
                heartbeat -= Time.unscaledDeltaTime;
                if (heartbeat <= 0f)
                {
                    ModEntry.Log("[heartbeat] connState=" + connState + " detailState=" + ds);
                    heartbeat = 3f;
                }
                if (ds == 0 || connState == 0)
                {
                    ModEntry.Log("TryConnect: disconnected during connect (" + (30f - connectTimeout).ToString("F1") + "s)");
                    callback(false);
                    yield break;
                }
                if (ds >= 6)
                {
                    ModEntry.Log("TryConnect: SUCCESS detailState=" + ds);
                    callback(true);
                    yield break;
                }
                connectTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            ModEntry.Log("TryConnect: TIMEOUT (last detailState=" + lastDs + ")");
            try { CallStaticVoid("PhotonNetwork", "Disconnect"); } catch { }
            ShowOverlay("LAN timed out.  Is server running?\n" + ServerIp + ":5055");
            callback(false);
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
