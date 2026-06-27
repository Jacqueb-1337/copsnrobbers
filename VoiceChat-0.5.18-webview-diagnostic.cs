// VoiceChat.cs - v0.5.18 WebView diagnostic build
//
// Photon custom-event voice chat prototype for Cops N Robbers.
// Entry point: VoiceChatEntry.Load() - discovered by CNRMod's DLL scanner.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;

namespace CNRVoiceChat
{
    public class VoiceChatEntry
    {
        public const string Version = "0.5.18";
        public const byte VoiceEvent = 197;
        public const byte VoiceRoomEvent = 196;
        private const string LogPath = "/storage/emulated/0/CNRMods/voicechat.log";
        private static bool _loaded;
        private static bool _audioPermissionRequested;
        public static bool SpeakerMuted;

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                RequestAudioRecordingPermission();
                GameObject root = new GameObject("CNRVoiceChat_Root");
                root.AddComponent<VoiceChatHook>();
                UnityEngine.Object.DontDestroyOnLoad(root);
                RegisterWithCnrMod();
                Log("=== VoiceChat v" + Version + " loaded ===");
            }
            catch (Exception ex) { Log("Load error: " + ex); }
        }

        public static bool HasAudioRecordingPermission()
        {
            try
            {
                if (Application.platform != RuntimePlatform.Android) return true;
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        return activity.Call<int>("checkSelfPermission", new object[] { "android.permission.RECORD_AUDIO" }) == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Audio permission check failed: " + ex.Message);
                return true;
            }
        }

        public static void RequestAudioRecordingPermission()
        {
            try
            {
                if (Application.platform != RuntimePlatform.Android) return;
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        if (activity.Call<int>("checkSelfPermission", new object[] { "android.permission.RECORD_AUDIO" }) != 0)
                        {
                            if (_audioPermissionRequested) return;
                            _audioPermissionRequested = true;
                            activity.Call("requestPermissions", new object[]
                            {
                                new string[] { "android.permission.RECORD_AUDIO" },
                                197
                            });
                            Log("Audio recording permission requested.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Audio permission request failed: " + ex.Message);
            }
        }

        public static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); }
            catch { }
            try { Debug.Log("[VoiceChat] " + msg); } catch { }
        }

        private static void RegisterWithCnrMod()
        {
            try
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type me = asm.GetType("CNRMods.ModEntry");
                    if (me == null) continue;
                    MethodInfo reg = me.GetMethod("RegisterMod",
                        BindingFlags.Public | BindingFlags.Static, null,
                        new Type[] { typeof(string), typeof(string) }, null);
                    if (reg != null) reg.Invoke(null, new object[] { "VoiceChat", Version });
                    return;
                }
            }
            catch (Exception ex) { Log("RegisterWithCnrMod error: " + ex.Message); }
        }
    }

    public class VoicePhotonProxy : IPhotonPeerListener
    {
        private readonly IPhotonPeerListener _original;
        private readonly VoiceChatHook _hook;

        public VoicePhotonProxy(IPhotonPeerListener original, VoiceChatHook hook)
        {
            _original = original;
            _hook = hook;
        }

        public void OnEvent(EventData ev)
        {
            try
            {
                VoiceChatEntry.Log("VoicePhotonProxy.OnEvent code=" + (ev != null ? ev.Code.ToString() : "null") + " hook=" + (_hook != null ? "1" : "0"));
            }
            catch { }
            if ((ev.Code == VoiceChatEntry.VoiceEvent || ev.Code == VoiceChatEntry.VoiceRoomEvent) && _hook != null)
                _hook.OnVoiceEvent(ev);
            if (_original != null) _original.OnEvent(ev);
        }

        public void DebugReturn(DebugLevel level, string message)
        {
            if (_original != null) _original.DebugReturn(level, message);
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            if (_original != null) _original.OnOperationResponse(operationResponse);
        }

        public void OnStatusChanged(StatusCode statusCode)
        {
            if (_original != null) _original.OnStatusChanged(statusCode);
        }
    }

    public class VoiceChatHook : MonoBehaviour
    {
        private const int CaptureRate = 8000;
        private const int ClipSeconds = 2;
        private const int FrameSamples = 160; // 20 ms at 8 kHz
        private const float SendInterval = 0.02f;
        private const float SilenceThreshold = 0f;
        private const float MicGain = 3.0f;
        private const float RemoteGain = 3.0f;

        private bool _probed;
        private bool _proxyInstalled;
        private Type _pnTypeCache;
        private string _micDevice;
        private AudioClip _micClip;
        private int _lastMicPos;
        private float _sendTimer;
        private int _seq;
        private bool _micMuted;
        private bool _hudReady;
        private bool _wasInGame;
        private Texture2D _micOnTex;
        private Texture2D _micOffTex;
        private Texture2D _speakerOnTex;
        private Texture2D _speakerOffTex;
        private readonly float[] _capture = new float[FrameSamples];
        private readonly byte[] _encoded = new byte[FrameSamples];
        private readonly Dictionary<string, RemoteVoice> _remotes = new Dictionary<string, RemoteVoice>();
        private readonly Dictionary<string, WebRtcPeerState> _webrtcPeers = new Dictionary<string, WebRtcPeerState>();
        private WebRtcBridge _webrtcBridge;
        private bool _webRtcSupported;
        private bool _webRtcActive;
        private bool _browserVisible;
        private bool _reportedBridgeBoot;
        private string _lastReportedScene;
        private string _lastReportedRoom;
        private float _lastReportTime;
        private float _lastVoiceRoomBroadcast;
        private string _forcedVoiceRoom;

        void Start()
        {
            ProbeRuntime();
            EnsureHudTextures();
            _webrtcBridge = new WebRtcBridge();
            _webRtcSupported = _webrtcBridge.ProbeSupport();
            _webRtcActive = false;
            _browserVisible = false;
            _reportedBridgeBoot = false;
            _lastReportedScene = null;
            _lastReportedRoom = null;
            _lastReportTime = -999f;
            _lastVoiceRoomBroadcast = -999f;
            _forcedVoiceRoom = null;
        }

        void OnLevelWasLoaded(int level)
        {
            _probed = false;
            ProbeRuntime();
            StartMicrophone();
        }

        void Update()
        {
            TryInstallProxy();
            PollWebMediator();
            if (_webrtcBridge != null) _webrtcBridge.Tick();
            if (!_reportedBridgeBoot)
            {
                _reportedBridgeBoot = true;
                ReportBridgeBootstrap();
            }
            MaybeReportState();
            MaybeBroadcastVoiceRoom();
            bool inGame = IsInGameRoom();
            if (_wasInGame && !inGame)
            {
                ShutdownVoicePath();
            }
            _wasInGame = inGame;
            bool isMenu = SceneName() == "MainMenu";
            if (isMenu)
            {
                TryStartWebRtc();
            }
            else
            {
                if (_webrtcBridge != null && _webrtcBridge.IsConnected())
                {
                    ShutdownVoicePath();
                }
                StopMicrophone();
                CleanupRemotes();
                return;
            }
            if (!inGame) return;
            if (_webRtcActive)
            {
                StopMicrophone();
                SyncWebViewMuteState();
                CleanupRemotes();
                return;
            }
            if (_micClip == null) StartMicrophone();
            SyncWebViewMuteState();
            CaptureAndSend();
            CleanupRemotes();
        }

        void OnGUI()
        {
            if (SceneName() == "MainMenu")
            {
                DrawMenuBrowserButton();
                return;
            }
            if (!IsInGameRoom()) return;
            EnsureHudTextures();
            float size = 56f;
            float pad = 10f;
            Rect micRect = new Rect(pad, pad, size, size);
            Rect spkRect = new Rect(pad, pad + size + 8f, size, size);
            if (GUI.Button(micRect, GUIContent.none, CreateHudStyle(_micMuted ? _micOffTex : _micOnTex)))
            {
                _micMuted = !_micMuted;
                VoiceChatEntry.Log("Mic muted=" + _micMuted);
                SyncWebViewMuteState();
                if (_micMuted) StopMicrophone();
                else if (IsInGameRoom()) StartMicrophone();
            }
            if (GUI.Button(spkRect, GUIContent.none, CreateHudStyle(VoiceChatEntry.SpeakerMuted ? _speakerOffTex : _speakerOnTex)))
            {
                VoiceChatEntry.SpeakerMuted = !VoiceChatEntry.SpeakerMuted;
                VoiceChatEntry.Log("Speaker muted=" + VoiceChatEntry.SpeakerMuted);
                SyncWebViewMuteState();
                if (VoiceChatEntry.SpeakerMuted) StopAllRemoteVoices();
            }
        }

        private void DrawMenuBrowserButton()
        {
            EnsureHudTextures();

            float pad = 10f;
            float btnW = 150f;
            float btnH = 54f;

            Rect btnRect = new Rect(pad, pad, btnW, btnH);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 18;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            if (GUI.Button(btnRect, _browserVisible ? "Hide Voice" : "Show Voice", buttonStyle))
            {
                _browserVisible = !_browserVisible;

                if (_webrtcBridge != null)
                {
                    _webrtcBridge.SetVisible(_browserVisible);
                }

                VoiceChatEntry.Log("Browser visible=" + _browserVisible);
            }

            DrawWebViewDiagnosticPanel();
        }

        private void DrawWebViewDiagnosticPanel()
        {
            if (_webrtcBridge == null) return;

            Rect rect = new Rect(10f, 74f, Screen.width - 20f, 260f);

            GUIStyle box = new GUIStyle(GUI.skin.box);
            box.alignment = TextAnchor.UpperLeft;
            box.fontSize = 14;
            box.normal.textColor = Color.white;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("VoiceChat WebView Diagnostic");
            sb.AppendLine("Scene: " + SceneName());
            sb.AppendLine("Room: " + GetVoiceRoomName());
            sb.AppendLine("Peer: " + LocalPeerId());
            sb.AppendLine(_webrtcBridge.GetStatusLine());
            sb.AppendLine("");

            string[] notes = _webrtcBridge.GetRecentNotes();
            for (int i = 0; i < notes.Length; i++)
            {
                sb.AppendLine(notes[i]);
            }

            GUI.Box(rect, sb.ToString(), box);
        }


        void OnDestroy()
        {
            ShutdownVoicePath();
        }

        private void ShutdownVoicePath()
        {
            try
            {
                StopMicrophone();
            }
            catch { }
            try
            {
                if (_webrtcBridge != null) _webrtcBridge.DisconnectAll();
            }
            catch { }
            _webRtcActive = false;
            try
            {
                foreach (RemoteVoice rv in _remotes.Values)
                {
                    if (rv != null) rv.Destroy();
                }
                _remotes.Clear();
            }
            catch { }
        }

        public void OnVoiceEvent(EventData ev)
        {
            try
            {
                VoiceChatEntry.Log("OnVoiceEvent code=" + (ev != null ? ev.Code.ToString() : "null"));
                Hashtable ht = ExtractPayload(ev);
                VoiceChatEntry.Log("OnVoiceEvent payload keys=" + DumpHashtableKeys(ht));
                if (ht == null) return;
                string sender = ht.ContainsKey("id") ? ht["id"] as string : null;
                if (string.IsNullOrEmpty(sender)) sender = SenderId(ev);
                if (string.IsNullOrEmpty(sender)) sender = "unknown";
                if (sender == LocalPeerId()) return;

                string kind = ht.ContainsKey("kind") ? ht["kind"] as string : null;
                if (string.IsNullOrEmpty(kind)) kind = "pcm";
                VoiceChatEntry.Log("OnVoiceEvent sender=" + sender + " kind=" + kind + " keys=" + DumpHashtableKeys(ht));
                if (kind == "webrtc")
                {
                    if (_webrtcBridge != null)
                    {
                        _webrtcBridge.OnSignal(sender, ht);
                        if (_webrtcBridge.IsSessionActive(sender))
                        {
                            _webRtcActive = true;
                        }
                    }
                    return;
                }
                if (kind == "voice_room")
                {
                    VoiceChatEntry.Log("Voice room event received from=" + sender + " keys=" + DumpHashtableKeys(ht));
                    if (ht.ContainsKey("room"))
                    {
                        _forcedVoiceRoom = Convert.ToString(ht["room"]);
                        VoiceChatEntry.Log("Voice room sync from " + sender + " room=" + _forcedVoiceRoom);
                        PostPhpLog(GetRoomName(), LocalPeerId(), "voice_room sync from=" + sender + " room=" + _forcedVoiceRoom + " scene=" + SceneName());
                    }
                    return;
                }
                if (!ht.ContainsKey("vc")) return;

                byte[] data = ht["vc"] as byte[]; 
                if (data == null || data.Length == 0) return;
                RemoteVoice rv;
                if (!_remotes.TryGetValue(sender, out rv))
                {
                    rv = new RemoteVoice(sender);
                    _remotes[sender] = rv;
                    VoiceChatEntry.Log("Remote voice created sender=" + sender);
                }
                rv.Push(data);
            }
            catch (Exception ex) { VoiceChatEntry.Log("OnVoiceEvent error: " + ex.Message); }
        }

        private void CaptureAndSend()
        {
            if (_micMuted) return;
            if (_webRtcActive && _webrtcBridge != null && _webrtcBridge.CanCaptureAudio())
            {
                _webrtcBridge.CaptureLocalAudioFrame();
                return;
            }
            _sendTimer -= Time.deltaTime;
            if (_sendTimer > 0f || _micClip == null) return;
            _sendTimer = SendInterval;

            int pos;
            try { pos = Microphone.GetPosition(_micDevice); }
            catch { return; }
            if (pos < 0) return;
            int available = pos >= _lastMicPos ? pos - _lastMicPos : (_micClip.samples - _lastMicPos) + pos;
            if (available < FrameSamples) return;

            int start = pos - FrameSamples;
            if (start < 0) start += _micClip.samples;
            ReadMicFrame(start, _capture);
            _lastMicPos = pos;

            float peak = 0f;
            for (int i = 0; i < _capture.Length; i++)
            {
                float a = Mathf.Abs(_capture[i]);
                if (a > peak) peak = a;
            }
            if (peak < SilenceThreshold) return;

            EncodePcm8(_capture, _encoded);
            Hashtable ht = new Hashtable();
            ht["id"] = LocalPeerId();
            ht["seq"] = _seq++;
            ht["kind"] = "pcm";
            ht["rate"] = CaptureRate;
            ht["vc"] = (byte[])_encoded.Clone();
            VoiceChatEntry.Log("Voice pcm send keys=" + DumpHashtableKeys(ht) + " seq=" + _seq);
            RaiseVoiceEvent(ht);
        }

        private void TryStartWebRtc()
        {
            if (_webrtcBridge == null || !_webRtcSupported) return;
            if (SceneName() != "MainMenu") return;

            string voiceRoom = GetVoiceRoomName();

            if (!_webrtcBridge.HasRequested())
            {
                _webrtcBridge.TryConnect(LocalPeerId(), IsInGameRoom(), voiceRoom, LocalDisplayName());
            }

            _webRtcActive = _webrtcBridge.IsConnected();
        }


        private void SyncWebViewMuteState()
        {
            if (_webrtcBridge == null) return;
            _webrtcBridge.UpdateMuteState(_micMuted, VoiceChatEntry.SpeakerMuted);
        }

        private void MaybeBroadcastVoiceRoom()
        {
            try
            {
                if (Time.realtimeSinceStartup - _lastVoiceRoomBroadcast < 5f) return;
                _lastVoiceRoomBroadcast = Time.realtimeSinceStartup;
                string currentRoom = GetRoomName();
                string room = GetVoiceRoomName();
                if (string.IsNullOrEmpty(room)) return;
                Hashtable ht = new Hashtable();
                ht["id"] = LocalPeerId();
                ht["kind"] = "voice_room";
                ht["room"] = room;
                ht["scene"] = SceneName();
                VoiceChatEntry.Log("Voice room send code=" + VoiceChatEntry.VoiceRoomEvent + " keys=" + DumpHashtableKeys(ht) + " room=" + room + " current=" + currentRoom + " forced=" + (_forcedVoiceRoom ?? "null"));
                RaiseVoiceEvent(VoiceChatEntry.VoiceRoomEvent, ht);
                VoiceChatEntry.Log("Voice room broadcast room=" + room);
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("MaybeBroadcastVoiceRoom error: " + ex.Message);
            }
        }

        private void PollWebMediator()
        {
            try
            {
                Type webType = FindType("WebMediator");
                if (webType == null) return;

                MethodInfo poll = webType.GetMethod("PollMessage", BindingFlags.Public | BindingFlags.Static);
                if (poll == null) return;

                for (int i = 0; i < 10; i++)
                {
                    object msg = null;

                    try
                    {
                        msg = poll.Invoke(null, null);
                    }
                    catch (TargetInvocationException ex)
                    {
                        VoiceChatEntry.Log("PollWebMediator target error: " + ex.Message +
                            " inner=" + (ex.InnerException != null ? ex.InnerException.ToString() : "null"));
                        return;
                    }

                    if (msg == null) return;

                    string path = Convert.ToString(ReadMember(msg, "path"));
                    object rawArgs = ReadMember(msg, "args");
                    Hashtable args = rawArgs as Hashtable;

                    VoiceChatEntry.Log("PollWebMediator raw msgType=" + msg.GetType().FullName +
                        " path=" + path +
                        " argsType=" + (rawArgs != null ? rawArgs.GetType().FullName : "null") +
                        " args=" + DumpHashtableKeys(args));

                    if (!string.IsNullOrEmpty(path))
                    {
                        if (_webrtcBridge != null)
                        {
                            _webrtcBridge.OnWebMessage(path, args);
                        }

                        if (path == "/note" && args != null && args.ContainsKey("text"))
                        {
                            VoiceChatEntry.Log("WebView note: " + Convert.ToString(args["text"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("PollWebMediator error: " + ex.ToString());
            }
        }


        private void ReportBridgeBootstrap()
        {
            try
            {
                string scene = string.Empty;
                try { scene = Application.loadedLevelName; } catch { scene = "unknown"; }
                string room = GetRoomName() ?? "offline";
                string peer = LocalPeerId() ?? "unknown";
                string text = "bridge boot scene=" + scene + " peer=" + peer + " room=" + room + " mic=" + VoiceChatEntry.HasAudioRecordingPermission();
                byte[] body = Encoding.UTF8.GetBytes("{\"room\":\"" + EscapeJson(room) + "\",\"peer\":\"" + EscapeJson(peer) + "\",\"text\":\"" + EscapeJson(text) + "\"}");
                using (WWW www = new WWW("https://play.jacqueb.me/voicechat/api.php?action=log", body, new Hashtable { { "Content-Type", "application/json" } }))
                {
                    // fire-and-forget
                }
                VoiceChatEntry.Log("Bridge bootstrap reported via PHP.");
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("ReportBridgeBootstrap error: " + ex.Message);
            }
        }

        private string GetVoiceRoomName()
        {
            string current = GetRoomName();
            if (!string.IsNullOrEmpty(current) && current != "offline")
            {
                if (!string.IsNullOrEmpty(_forcedVoiceRoom) && !string.Equals(_forcedVoiceRoom, current, StringComparison.Ordinal))
                {
                    VoiceChatEntry.Log("Voice room override ignored current=" + current + " forced=" + _forcedVoiceRoom);
                }
                return current;
            }
            if (!string.IsNullOrEmpty(_forcedVoiceRoom)) return _forcedVoiceRoom;
            return current;
        }

        private static object ReadMember(object obj, string name)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return null;

            try
            {
                Type t = obj.GetType();

                FieldInfo f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null) return f.GetValue(obj);

                PropertyInfo p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.CanRead) return p.GetValue(obj, null);
            }
            catch
            {
            }

            return null;
        }

        private static string DumpHashtableKeys(Hashtable ht)
        {
            if (ht == null) return "null";
            List<string> keys = new List<string>();
            foreach (object key in ht.Keys)
            {
                keys.Add(Convert.ToString(key));
            }
            keys.Sort();
            return string.Join(",", keys.ToArray());
        }

        private void RaiseVoiceEvent(byte eventCode, Hashtable ht)
        {
            try
            {
                object peer = GetNetworkingPeer();
                if (peer == null) return;
                MethodInfo raise = peer.GetType().GetMethod("OpRaiseEvent",
                    new Type[] { typeof(byte), typeof(Hashtable), typeof(bool), typeof(byte) });
                if (raise == null) return;
                raise.Invoke(peer, new object[] { eventCode, ht, false, (byte)0 });
            }
            catch (Exception ex) { VoiceChatEntry.Log("RaiseVoiceEvent error: " + ex.Message); }
        }

        private void MaybeReportState()
        {
            try
            {
                if (Time.realtimeSinceStartup - _lastReportTime < 5f) return;
                string scene = SceneName();
                string room = GetRoomName();
                _lastReportedScene = scene;
                _lastReportedRoom = room;
                _lastReportTime = Time.realtimeSinceStartup;
                VoiceChatEntry.Log("State heartbeat scene=" + scene + " room=" + room + " forced=" + (_forcedVoiceRoom ?? "null"));
                ReportState(scene, room);
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("MaybeReportState error: " + ex.Message);
            }
        }

        private void ReportState(string scene, string room)
        {
            try
            {
                string peer = LocalPeerId() ?? "unknown";
                string name = LocalDisplayName() ?? "local";
                string mic = VoiceChatEntry.HasAudioRecordingPermission() ? "1" : "0";
                string devs = string.Join(",", Microphone.devices ?? new string[0]);
                string bridge = _webrtcBridge != null && _webrtcBridge.IsConnected() ? "1" : "0";
                string peers = _webrtcBridge != null ? _webrtcBridge.PeerCount().ToString() : "0";
                string forced = string.IsNullOrEmpty(_forcedVoiceRoom) ? "null" : _forcedVoiceRoom;
                string text = "state scene=" + scene + " room=" + room + " forced=" + forced + " peer=" + peer + " name=" + name + " mic=" + mic + " devices=" + devs + " web=" + (_webRtcSupported ? "1" : "0") + " bridge=" + bridge + " peers=" + peers;
                PostPhpLog(room, peer, text);
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("ReportState error: " + ex.Message);
            }
        }

        private void PostPhpLog(string room, string peer, string text)
        {
            try
            {
                string json = "{\"room\":\"" + EscapeJson(room) + "\",\"peer\":\"" + EscapeJson(peer) + "\",\"text\":\"" + EscapeJson(text) + "\"}";
                byte[] body = Encoding.UTF8.GetBytes(json);
                Hashtable headers = new Hashtable();
                headers["Content-Type"] = "application/json";
                using (WWW www = new WWW("https://play.jacqueb.me/voicechat/api.php?action=log", body, headers))
                {
                }
                VoiceChatEntry.Log("PHP log posted: " + text);
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("PostPhpLog error: " + ex.Message);
            }
        }

        private string SceneName()
        {
            try { return Application.loadedLevelName; } catch { return "unknown"; }
        }

        private static string EscapeJson(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private void ReadMicFrame(int start, float[] dst)
        {
            if (start + dst.Length <= _micClip.samples)
            {
                _micClip.GetData(dst, start);
                return;
            }

            int first = _micClip.samples - start;
            float[] tmpA = new float[first];
            float[] tmpB = new float[dst.Length - first];
            _micClip.GetData(tmpA, start);
            _micClip.GetData(tmpB, 0);
            Array.Copy(tmpA, 0, dst, 0, tmpA.Length);
            Array.Copy(tmpB, 0, dst, tmpA.Length, tmpB.Length);
        }

        private static void EncodePcm8(float[] src, byte[] dst)
        {
            for (int i = 0; i < src.Length && i < dst.Length; i++)
            {
                float s = Mathf.Clamp(src[i] * MicGain, -1f, 1f);
                int v = Mathf.RoundToInt((s * 127f) + 128f);
                if (v < 0) v = 0;
                if (v > 255) v = 255;
                dst[i] = (byte)v;
            }
        }

        private void StartMicrophone()
        {
            try
            {
                if (_micClip != null) return;
                if (!IsInGameRoom()) return;
                VoiceChatEntry.RequestAudioRecordingPermission();
                if (!VoiceChatEntry.HasAudioRecordingPermission())
                {
                    VoiceChatEntry.Log("StartMicrophone: RECORD_AUDIO permission not granted yet");
                    return;
                }
                string[] devices = Microphone.devices;
                if (devices == null || devices.Length == 0)
                {
                    VoiceChatEntry.Log("StartMicrophone: no devices");
                    return;
                }
                _micDevice = devices[0];
                _micClip = Microphone.Start(_micDevice, true, ClipSeconds, CaptureRate);
                _lastMicPos = 0;
                VoiceChatEntry.Log("StartMicrophone: device=" + _micDevice + " rate=" + CaptureRate);
                if (devices.Length > 0)
                {
                    string devList = string.Join(",", devices);
                    VoiceChatEntry.Log("Mic devices: " + devList);
                }
                PostPhpLog(GetRoomName(), LocalPeerId(), "mic start device=" + _micDevice + " rate=" + CaptureRate + " scene=" + SceneName());
            }
            catch (Exception ex) { VoiceChatEntry.Log("StartMicrophone error: " + ex.Message); }
        }

        private void StopMicrophone()
        {
            try
            {
                if (!string.IsNullOrEmpty(_micDevice) && Microphone.IsRecording(_micDevice))
                    Microphone.End(_micDevice);
            }
            catch { }
            _micClip = null;
        }

        private void RaiseVoiceEvent(Hashtable ht)
        {
            RaiseVoiceEvent(VoiceChatEntry.VoiceEvent, ht);
        }

        private void TryInstallProxy()
        {
            if (_proxyInstalled) return;
            try
            {
                object peer = GetNetworkingPeer();
                if (peer == null) return;
                FieldInfo lf = peer.GetType().GetField("externalListener",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (lf == null) return;
                IPhotonPeerListener cur = lf.GetValue(peer) as IPhotonPeerListener;
                if (cur == null) return;
                if (cur is VoicePhotonProxy)
                {
                    _proxyInstalled = true;
                    return;
                }
                lf.SetValue(peer, new VoicePhotonProxy(cur, this));
                _proxyInstalled = true;
                VoiceChatEntry.Log("Photon proxy installed wrapping " + cur.GetType().Name);
            }
            catch (Exception ex) { VoiceChatEntry.Log("TryInstallProxy error: " + ex.Message); }
        }

        private object GetNetworkingPeer()
        {
            try
            {
                Type t = GetPNType();
                if (t == null) return null;
                FieldInfo fi = t.GetField("networkingPeer",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                return fi != null ? fi.GetValue(null) : null;
            }
            catch { return null; }
        }

        private Type GetPNType()
        {
            if (_pnTypeCache != null) return _pnTypeCache;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("PhotonNetwork");
                if (t != null)
                {
                    _pnTypeCache = t;
                    return t;
                }
            }
            return null;
        }

        private bool IsInRoom()
        {
            try { return PhotonNetwork.room != null; }
            catch { return false; }
        }

        private bool IsInGameRoom()
        {
            if (!IsInRoom()) return false;
            try
            {
                string level = Application.loadedLevelName;
                if (string.IsNullOrEmpty(level)) return true;
                if (level.IndexOf("MainMenu", StringComparison.OrdinalIgnoreCase) >= 0) return false;
                if (level.IndexOf("Menu", StringComparison.OrdinalIgnoreCase) >= 0) return false;
                return true;
            }
            catch
            {
                return true;
            }
        }

        private void EnsureHudTextures()
        {
            if (_hudReady) return;
            _hudReady = true;
            _micOnTex = BuildMicTexture(false);
            _micOffTex = BuildMicTexture(true);
            _speakerOnTex = BuildSpeakerTexture(false);
            _speakerOffTex = BuildSpeakerTexture(true);
        }

        private GUIStyle CreateHudStyle(Texture2D tex)
        {
            GUIStyle st = new GUIStyle(GUI.skin.button);
            st.normal.background = tex;
            st.hover.background = tex;
            st.active.background = tex;
            st.focused.background = tex;
            st.border = new RectOffset(0, 0, 0, 0);
            st.margin = new RectOffset(0, 0, 0, 0);
            st.padding = new RectOffset(0, 0, 0, 0);
            st.overflow = new RectOffset(0, 0, 0, 0);
            st.alignment = TextAnchor.MiddleCenter;
            return st;
        }

        private Texture2D BuildMicTexture(bool muted)
        {
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color white = new Color(0.95f, 0.95f, 0.95f, 1f);
            Color red = new Color(0.95f, 0.15f, 0.15f, 1f);
            FillTexture(tex, clear);
            DrawRect(tex, 28, 12, 8, 22, white);
            DrawCircle(tex, 32, 30, 12, white);
            DrawRect(tex, 29, 42, 6, 8, white);
            DrawRect(tex, 22, 48, 20, 3, white);
            if (muted) DrawLine(tex, 14, 50, 50, 14, red, 5);
            tex.Apply();
            return tex;
        }

        private Texture2D BuildSpeakerTexture(bool muted)
        {
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color white = new Color(0.95f, 0.95f, 0.95f, 1f);
            Color red = new Color(0.95f, 0.15f, 0.15f, 1f);
            FillTexture(tex, clear);
            DrawRect(tex, 14, 24, 8, 16, white);
            DrawPolygon(tex, new int[] { 22, 32, 32, 22 }, new int[] { 24, 18, 46, 40 }, white);
            DrawArc(tex, 34, 32, 8, 12, white);
            DrawArc(tex, 40, 32, 14, 18, white);
            if (muted) DrawLine(tex, 14, 50, 50, 14, red, 5);
            tex.Apply();
            return tex;
        }

        private static void FillTexture(Texture2D tex, Color color)
        {
            Color[] pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
        }

        private static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color color)
        {
            for (int py = y; py < y + h; py++)
            for (int px = x; px < x + w; px++)
                if (px >= 0 && py >= 0 && px < tex.width && py < tex.height) tex.SetPixel(px, py, color);
        }

        private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color, int thickness)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                DrawDot(tex, x0, y0, color, thickness);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        private static void DrawDot(Texture2D tex, int cx, int cy, Color color, int thickness)
        {
            int r = Math.Max(1, thickness / 2);
            for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
                if (x >= 0 && y >= 0 && x < tex.width && y < tex.height) tex.SetPixel(x, y, color);
        }

        private static void DrawCircle(Texture2D tex, int cx, int cy, int r, Color color)
        {
            for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
            {
                int d = x * x + y * y;
                if (d <= r * r && d >= (r - 3) * (r - 3))
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && py >= 0 && px < tex.width && py < tex.height) tex.SetPixel(px, py, color);
                }
            }
        }

        private static void DrawPolygon(Texture2D tex, int[] xs, int[] ys, Color color)
        {
            int count = Math.Min(xs.Length, ys.Length);
            if (count < 2) return;
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                DrawLine(tex, xs[i], ys[i], xs[j], ys[j], color, 3);
            }
        }

        private static void DrawArc(Texture2D tex, int cx, int cy, int innerR, int outerR, Color color)
        {
            for (int y = -outerR; y <= outerR; y++)
            for (int x = 0; x <= outerR; x++)
            {
                int d = x * x + y * y;
                if (d <= outerR * outerR && d >= innerR * innerR)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && py >= 0 && px < tex.width && py < tex.height) tex.SetPixel(px, py, color);
                }
            }
        }

        private void StopAllRemoteVoices()
        {
            foreach (RemoteVoice rv in _remotes.Values)
            {
                if (rv != null) rv.StopPlayback();
            }
        }

        private string LocalPeerId()
        {
            try
            {
                if (PhotonNetwork.player != null)
                {
                    string nm = Convert.ToString(PhotonNetwork.player.name);
                    if (!string.IsNullOrEmpty(nm)) return nm;
                    string id = Convert.ToString(PhotonNetwork.player.ID);
                    if (!string.IsNullOrEmpty(id)) return id;
                }
                Type mgrType = FindType("CNRMultiplayerManager");
                if (mgrType != null)
                {
                    FieldInfo instField = mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                    object mgrInst = instField != null ? instField.GetValue(null) : null;
                    if (mgrInst != null)
                    {
                        FieldInfo playerField = mgrType.GetField("myPlayerInfo", BindingFlags.Public | BindingFlags.Instance);
                        object playerInfo = playerField != null ? playerField.GetValue(mgrInst) : null;
                        if (playerInfo != null)
                        {
                            FieldInfo idField = playerInfo.GetType().GetField("mId", BindingFlags.Public | BindingFlags.Instance);
                            object id = idField != null ? idField.GetValue(playerInfo) : null;
                            if (id != null && !string.IsNullOrEmpty(Convert.ToString(id)))
                                return Convert.ToString(id);
                        }
                    }
                }
                if (PhotonNetwork.player != null) return Convert.ToString(PhotonNetwork.player.ID);
            }
            catch { }
            return "local";
        }

        private string LocalDisplayName()
        {
            try
            {
                Type mgrType = FindType("CNRMultiplayerManager");
                if (mgrType != null)
                {
                    FieldInfo instField = mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                    object mgrInst = instField != null ? instField.GetValue(null) : null;
                    if (mgrInst != null)
                    {
                        FieldInfo playerField = mgrType.GetField("myPlayerInfo", BindingFlags.Public | BindingFlags.Instance);
                        object playerInfo = playerField != null ? playerField.GetValue(mgrInst) : null;
                        if (playerInfo != null)
                        {
                            FieldInfo nameField = playerInfo.GetType().GetField("mName", BindingFlags.Public | BindingFlags.Instance);
                            object name = nameField != null ? nameField.GetValue(playerInfo) : null;
                            if (name != null && !string.IsNullOrEmpty(Convert.ToString(name)))
                                return Convert.ToString(name);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(PhotonNetwork.playerName))
                    return PhotonNetwork.playerName;
            }
            catch { }
            return "local";
        }

        private string GetRoomName()
        {
            try
            {
                if (PhotonNetwork.room != null && !string.IsNullOrEmpty(PhotonNetwork.room.name))
                    return PhotonNetwork.room.name;
            }
            catch { }
            return "offline";
        }

        private Type FindType(string name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        private string SenderId(EventData ev)
        {
            try
            {
                object sender = ev.Parameters != null && ev.Parameters.ContainsKey((byte)254)
                    ? ev.Parameters[(byte)254] : null;
                if (sender != null) return Convert.ToString(sender);
            }
            catch { }
            return null;
        }

        private Hashtable ExtractPayload(EventData ev)
        {
            try
            {
                if (ev == null || ev.Parameters == null) return null;
                if (ev.Parameters.ContainsKey((byte)245))
                {
                    Hashtable payload = ev.Parameters[(byte)245] as Hashtable;
                    if (payload != null) return payload;
                    IDictionary dict = ev.Parameters[(byte)245] as IDictionary;
                    if (dict != null)
                    {
                        Hashtable flat = new Hashtable();
                        foreach (object de in dict)
                        {
                            object key = GetDictKey(de);
                            object value = GetDictValue(de);
                            if (key != null) flat[key] = value;
                        }
                        return flat;
                    }
                }
                foreach (object entry in ev.Parameters)
                {
                    object value = GetDictValue(entry);
                    Hashtable ht = value as Hashtable;
                    if (ht != null) return ht;
                    IDictionary dict = value as IDictionary;
                    if (dict != null)
                    {
                        Hashtable flat = new Hashtable();
                        foreach (object inner in dict)
                        {
                            object key = GetDictKey(inner);
                            object innerValue = GetDictValue(inner);
                            if (key != null) flat[key] = innerValue;
                        }
                        return flat;
                    }
                }
            }
            catch (Exception ex) { VoiceChatEntry.Log("ExtractPayload error: " + ex.Message); }
            return null;
        }

        private object GetDictKey(object entry)
        {
            if (entry == null) return null;
            try
            {
                if (entry is DictionaryEntry)
                {
                    DictionaryEntry de = (DictionaryEntry)entry;
                    return de.Key;
                }
                Type t = entry.GetType();
                PropertyInfo p = t.GetProperty("Key", BindingFlags.Public | BindingFlags.Instance);
                if (p != null) return p.GetValue(entry, null);
                FieldInfo f = t.GetField("Key", BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return f.GetValue(entry);
            }
            catch { }
            return null;
        }

        private object GetDictValue(object entry)
        {
            if (entry == null) return null;
            try
            {
                if (entry is DictionaryEntry)
                {
                    DictionaryEntry de = (DictionaryEntry)entry;
                    return de.Value;
                }
                Type t = entry.GetType();
                PropertyInfo p = t.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (p != null) return p.GetValue(entry, null);
                FieldInfo f = t.GetField("Value", BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return f.GetValue(entry);
            }
            catch { }
            return null;
        }

        private void CleanupRemotes()
        {
            List<string> dead = null;
            foreach (KeyValuePair<string, RemoteVoice> kv in _remotes)
            {
                if (kv.Value == null || Time.time - kv.Value.LastPushTime > 15f)
                {
                    if (dead == null) dead = new List<string>();
                    dead.Add(kv.Key);
                }
            }
            if (dead == null) return;
            for (int i = 0; i < dead.Count; i++)
            {
                RemoteVoice rv;
                if (_remotes.TryGetValue(dead[i], out rv) && rv != null) rv.Destroy();
                _remotes.Remove(dead[i]);
            }
        }

        private void ProbeRuntime()
        {
            if (_probed) return;
            _probed = true;
            try
            {
                string levelName = "";
                try { levelName = Application.loadedLevelName; } catch { }
                VoiceChatEntry.Log("Probe: scene=" + levelName);
                ProbeMicrophone();
                ProbeNetworkingApis();
            }
            catch (Exception ex) { VoiceChatEntry.Log("ProbeRuntime error: " + ex); }
        }

        private void ProbeMicrophone()
        {
            try
            {
                string[] devices = Microphone.devices;
                VoiceChatEntry.Log("Microphone.devices=" + (devices != null ? devices.Length : 0));
                if (devices != null)
                {
                    for (int i = 0; i < devices.Length; i++)
                    {
                        int min;
                        int max;
                        Microphone.GetDeviceCaps(devices[i], out min, out max);
                        VoiceChatEntry.Log("Mic[" + i + "] name=" + devices[i] + " caps=" + min + "-" + max);
                    }
                }
            }
            catch (Exception ex) { VoiceChatEntry.Log("ProbeMicrophone error: " + ex.Message); }
        }

        private void ProbeNetworkingApis()
        {
            ProbeType("Unity.WebRTC.RTCPeerConnection");
            ProbeType("Unity.WebRTC.RTCDataChannel");
            ProbeType("Unity.WebRTC.AudioStreamTrack");
            ProbeType("Unity.WebRTC.MediaStream");
            ProbeType("Unity.WebRTC.RTCIceCandidate");
            ProbeType("PhotonVoiceNetwork");
            ProbeType("ExitGames.Client.Photon.PhotonPeer");
            ProbeType("PhotonNetwork");
            ProbeType("System.Net.Sockets.UdpClient");
            ProbeType("System.Net.WebSockets.ClientWebSocket");
        }

        private void ProbeType(string typeName)
        {
            try
            {
                bool found = false;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.GetType(typeName) != null)
                    {
                        found = true;
                        break;
                    }
                }
                VoiceChatEntry.Log("TypeProbe: " + typeName + "=" + found);
            }
            catch (Exception ex) { VoiceChatEntry.Log("TypeProbe " + typeName + " error: " + ex.Message); }
        }
    }

    public class RemoteVoice
    {
        private const int Rate = 8000;
        private readonly string _sender;
        private readonly GameObject _go;
        private readonly AudioSource _source;
        private readonly VoicePlaybackFilter _filter;
        public float LastPushTime;

        public RemoteVoice(string sender)
        {
            _sender = sender;
            _go = new GameObject("VoiceChat_Remote_" + sender);
            UnityEngine.Object.DontDestroyOnLoad(_go);
            Camera cam = Camera.main;
            if (cam != null) _go.transform.parent = cam.transform;
            _go.transform.localPosition = Vector3.zero;
            _source = _go.AddComponent<AudioSource>();
            _source.volume = 1f;
            _source.playOnAwake = false;
            _filter = _go.AddComponent<VoicePlaybackFilter>();
            _filter.Init(Rate);
            LastPushTime = Time.time;
        }

        public void Push(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            if (VoiceChatEntry.SpeakerMuted) return;
            LastPushTime = Time.time;
            float[] decoded = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                decoded[i] = Mathf.Clamp((((float)data[i] - 128f) / 127f) * VoiceChatHookRemoteGain(), -1f, 1f);
            }
            try
            {
                if (_filter != null) _filter.Enqueue(decoded);
                if (_source != null && !_source.isPlaying && _filter != null && _filter.BufferedSampleCount() >= 4000)
                    _source.Play();
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("Remote voice push error: " + ex.Message);
            }
        }

        private static float VoiceChatHookRemoteGain()
        {
            return 2.5f;
        }

        public void Destroy()
        {
            try
            {
                if (_go != null) UnityEngine.Object.Destroy(_go);
            }
            catch { }
        }

        public void StopPlayback()
        {
            try
            {
                if (_source != null) _source.Stop();
                if (_filter != null) _filter.Clear();
            }
            catch { }
        }
    }

    public class WebRtcPeerState
    {
        public string PeerId;
        public string State;
        public string LastOffer;
        public string LastAnswer;
        public string LastCandidate;
        public float LastSignalTime;
    }

    public class WebRtcBridge
    {
        private const string DebugPageUrl = "https://play.jacqueb.me/voicechat/debug.html";

        private bool _probed;
        private bool _supported;
        private bool _connectRequested;
        private bool _pageReady;
        private bool _visible;
        private bool _methodDumped;

        private string _localPeerId;
        private string _roomName;
        private string _displayName;

        private Type _webMediatorType;
        private MethodInfo _miInstall;
        private MethodInfo _miLoadUrl;
        private MethodInfo _miShow;
        private MethodInfo _miHide;
        private MethodInfo _miSetMargin;
        private MethodInfo _miTransparent;
        private MethodInfo _miPoll;

        private float _lastPageMessageTime = -999f;
        private string _lastPageMessage = "none";
        private readonly List<string> _recentNotes = new List<string>();

        public bool ProbeSupport()
        {
            if (_probed) return _supported;
            _probed = true;

            _webMediatorType = FindType("WebMediator");

            if (_webMediatorType != null)
            {
                _miInstall = GetStaticMethod("Install");
                _miLoadUrl = GetStaticMethod("LoadUrl");
                _miShow = GetStaticMethod("Show");
                _miHide = GetStaticMethod("Hide");
                _miSetMargin = GetStaticMethod("SetMargin");
                _miTransparent = GetStaticMethod("MakeTransparentWebViewBackground");
                _miPoll = GetStaticMethod("PollMessage");

                DumpWebMediatorMethods();
            }

            _supported = _webMediatorType != null && _miLoadUrl != null && _miShow != null;

            VoiceChatEntry.Log("WebView diagnostic support=" + _supported +
                " type=" + (_webMediatorType != null ? _webMediatorType.FullName : "null") +
                " install=" + (_miInstall != null) +
                " loadUrl=" + (_miLoadUrl != null) +
                " show=" + (_miShow != null) +
                " hide=" + (_miHide != null) +
                " setMargin=" + (_miSetMargin != null) +
                " transparent=" + (_miTransparent != null) +
                " poll=" + (_miPoll != null));

            return _supported;
        }

        public bool TryConnect(string localPeerId, bool inGame, string roomName, string displayName)
        {
            if (!_supported) return false;

            if (_connectRequested)
            {
                return _pageReady;
            }

            _connectRequested = true;
            _pageReady = false;
            _visible = true;

            _localPeerId = string.IsNullOrEmpty(localPeerId) ? "local" : localPeerId;
            _roomName = string.IsNullOrEmpty(roomName) ? "offline" : roomName;
            _displayName = string.IsNullOrEmpty(displayName) ? "local" : displayName;

            VoiceChatEntry.Log("WebView diagnostic connect requested peer=" + _localPeerId +
                " room=" + _roomName +
                " name=" + _displayName +
                " scene=" + SafeSceneName() +
                " inGame=" + inGame +
                " screen=" + Screen.width + "x" + Screen.height);

            TryInvoke("Install", _miInstall);

            string url = BuildDebugPageUrl();

            VoiceChatEntry.Log("WebView diagnostic loading url=" + url);

            TryInvoke("LoadUrl", _miLoadUrl, url);
            TryInvoke("Show", _miShow);

            VoiceChatEntry.Log("WebView diagnostic LoadUrl and Show completed. Waiting for page_ready.");

            return false;
        }

        public void Tick()
        {
            if (!_supported) return;

            if (_connectRequested && !_pageReady)
            {
                float age = Time.realtimeSinceStartup - _lastPageMessageTime;
                if (_lastPageMessageTime > 0f && age > 10f)
                {
                    VoiceChatEntry.Log("WebView diagnostic still waiting for page_ready. Last message age=" +
                        age.ToString("0.0") + " text=" + _lastPageMessage);
                    _lastPageMessageTime = Time.realtimeSinceStartup;
                }
            }
        }

        public void OnWebMessage(string path, Hashtable args)
        {
            string text = "";

            try
            {
                if (args != null && args.ContainsKey("text"))
                {
                    text = Convert.ToString(args["text"]);
                }
            }
            catch
            {
                text = "";
            }

            _lastPageMessageTime = Time.realtimeSinceStartup;
            _lastPageMessage = "path=" + path + " text=" + text;

            AddRecentNote(_lastPageMessage);

            VoiceChatEntry.Log("WebView diagnostic message " + _lastPageMessage);

            if (path == "/note")
            {
                if (text.IndexOf("page_ready", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _pageReady = true;
                    VoiceChatEntry.Log("WebView diagnostic page_ready received. Bridge is now connected.");
                }
            }
        }

        public bool IsConnected()
        {
            return _pageReady;
        }

        public bool HasRequested()
        {
            return _connectRequested;
        }

        public bool CanCaptureAudio()
        {
            return false;
        }

        public int PeerCount()
        {
            return 0;
        }

        public bool IsSessionActive(string peerId)
        {
            return false;
        }

        public void CaptureLocalAudioFrame()
        {
        }

        public void OnSignal(string sender, Hashtable payload)
        {
            VoiceChatEntry.Log("WebView diagnostic ignoring WebRTC signal while in diagnostic mode sender=" + sender);
        }

        public void SendSignal(string kind, string sdp, string candidate, int? mline)
        {
            VoiceChatEntry.Log("WebView diagnostic SendSignal ignored kind=" + kind);
        }

        public void UpdateMuteState(bool micMuted, bool speakerMuted)
        {
            VoiceChatEntry.Log("WebView diagnostic mute state ignored micMuted=" + micMuted + " speakerMuted=" + speakerMuted);
        }

        public void SetVisible(bool visible)
        {
            if (!_supported) return;

            _visible = visible;

            if (visible)
            {
                TryInvoke("Show", _miShow);
            }
            else
            {
                TryInvoke("Hide", _miHide);
            }

            VoiceChatEntry.Log("WebView diagnostic visible=" + visible);
        }

        public void DisconnectAll()
        {
            _connectRequested = false;
            _pageReady = false;
            _visible = false;

            TryInvoke("Hide", _miHide);
            TryInvoke("LoadUrlBlank", _miLoadUrl, "about:blank");

            VoiceChatEntry.Log("WebView diagnostic disconnected.");
        }

        public string GetStatusLine()
        {
            return "WebView supported=" + _supported +
                " requested=" + _connectRequested +
                " pageReady=" + _pageReady +
                " visible=" + _visible +
                " last=" + _lastPageMessage;
        }

        public string[] GetRecentNotes()
        {
            return _recentNotes.ToArray();
        }

        private MethodInfo GetStaticMethod(string name)
        {
            if (_webMediatorType == null) return null;
            return _webMediatorType.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        }

        private void DumpWebMediatorMethods()
        {
            if (_methodDumped) return;
            _methodDumped = true;

            try
            {
                if (_webMediatorType == null)
                {
                    VoiceChatEntry.Log("WebMediator dump skipped, type null.");
                    return;
                }

                VoiceChatEntry.Log("=== WebMediator method dump begin ===");

                MethodInfo[] methods = _webMediatorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo mi = methods[i];
                    if (mi == null) continue;

                    ParameterInfo[] ps = mi.GetParameters();
                    StringBuilder sb = new StringBuilder();
                    sb.Append(mi.IsPublic ? "public " : "nonpublic ");
                    sb.Append(mi.IsStatic ? "static " : "instance ");
                    sb.Append(mi.ReturnType != null ? mi.ReturnType.Name : "void");
                    sb.Append(" ");
                    sb.Append(mi.Name);
                    sb.Append("(");

                    for (int p = 0; p < ps.Length; p++)
                    {
                        if (p > 0) sb.Append(", ");
                        sb.Append(ps[p].ParameterType != null ? ps[p].ParameterType.FullName : "null");
                        sb.Append(" ");
                        sb.Append(ps[p].Name);
                    }

                    sb.Append(")");
                    VoiceChatEntry.Log(sb.ToString());
                }

                VoiceChatEntry.Log("=== WebMediator method dump end ===");
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("DumpWebMediatorMethods error: " + FullException(ex));
            }
        }

        private string BuildDebugPageUrl()
        {
            string url = DebugPageUrl;
            url += "?room=" + Url(_roomName);
            url += "&peer=" + Url(_localPeerId);
            url += "&name=" + Url(_displayName);
            url += "&scene=" + Url(SafeSceneName());
            url += "&screen=" + Url(Screen.width + "x" + Screen.height);
            url += "&t=" + Url(DateTime.UtcNow.Ticks.ToString());
            return url;
        }

        private static string Url(string value)
        {
            try
            {
                if (value == null) value = "";
                return Uri.EscapeDataString(value);
            }
            catch
            {
                return "";
            }
        }

        private void TryInvoke(string label, MethodInfo mi, params object[] args)
        {
            try
            {
                if (mi == null)
                {
                    VoiceChatEntry.Log("WebMediator invoke skipped label=" + label + " method=null");
                    return;
                }

                VoiceChatEntry.Log("WebMediator invoke begin label=" + label + " method=" + mi.Name + " args=" + DescribeArgs(args));
                object result = mi.Invoke(null, args);
                VoiceChatEntry.Log("WebMediator invoke ok label=" + label + " result=" + (result != null ? Convert.ToString(result) : "null"));
            }
            catch (TargetInvocationException ex)
            {
                VoiceChatEntry.Log("WebMediator invoke target failed label=" + label + " error=" + FullException(ex));
                if (ex.InnerException != null)
                {
                    VoiceChatEntry.Log("WebMediator inner error label=" + label + " inner=" + FullException(ex.InnerException));
                }
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("WebMediator invoke failed label=" + label + " error=" + FullException(ex));
            }
        }

        private static string DescribeArgs(object[] args)
        {
            if (args == null || args.Length == 0) return "none";

            try
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0) sb.Append(" | ");

                    object a = args[i];
                    if (a == null)
                    {
                        sb.Append("null");
                    }
                    else
                    {
                        string s = Convert.ToString(a);
                        if (s != null && s.Length > 180) s = s.Substring(0, 180) + "...";
                        sb.Append(a.GetType().FullName);
                        sb.Append("=");
                        sb.Append(s);
                    }
                }

                return sb.ToString();
            }
            catch
            {
                return "describe_failed";
            }
        }

        private static string FullException(Exception ex)
        {
            try
            {
                if (ex == null) return "null";
                return ex.GetType().FullName + ": " + ex.Message + "\n" + ex.StackTrace;
            }
            catch
            {
                return "exception_to_string_failed";
            }
        }

        private static Type FindType(string name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type t = asm.GetType(name);
                    if (t != null) return t;
                }
                catch
                {
                }
            }
            return null;
        }

        private static string SafeSceneName()
        {
            try
            {
                return Application.loadedLevelName;
            }
            catch
            {
                return "unknown";
            }
        }

        private void AddRecentNote(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text)) return;
                _recentNotes.Add(text);
                while (_recentNotes.Count > 8) _recentNotes.RemoveAt(0);
            }
            catch
            {
            }
        }
    }


    public class VoicePlaybackFilter : MonoBehaviour
    {
        private readonly Queue<float> _queue = new Queue<float>();
        private readonly object _lock = new object();
        private int _srcRate = 8000;
        private int _dstRate = 44100;
        private float _phase;
        private float _playbackRateFactor = 0.18f;
        private bool _hasCurrent;
        private bool _hasNext;
        private float _current;
        private float _next;
        private bool _hasLast;
        private float _lastOutput;
        private int _outputSamples;

        public void Init(int rate)
        {
            _srcRate = rate;
            _dstRate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
            _phase = 0f;
            _hasCurrent = false;
            _hasNext = false;
            _hasLast = false;
            _lastOutput = 0f;
            _outputSamples = 0;
        }

        public void Enqueue(float[] samples)
        {
            if (samples == null || samples.Length == 0) return;
            lock (_lock)
            {
                for (int i = 0; i < samples.Length; i++) _queue.Enqueue(samples[i]);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
                _hasCurrent = false;
                _hasNext = false;
                _hasLast = false;
                _lastOutput = 0f;
            }
        }

        public int BufferedSampleCount()
        {
            lock (_lock)
            {
                int count = _queue.Count;
                if (_hasCurrent) count++;
                if (_hasNext) count++;
                return count;
            }
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            lock (_lock)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    EnsureSamples();
                    if (_hasCurrent && _hasNext)
                        data[i] = Mathf.Lerp(_current, _next, _phase);
                    else if (_hasCurrent)
                        data[i] = _current;
                    else if (_hasLast)
                        data[i] = _lastOutput;
                    else
                        data[i] = 0f;

                    _lastOutput = data[i];
                    _hasLast = true;

                    _phase += ((float)_srcRate * _playbackRateFactor) / (float)_dstRate;
                    _outputSamples++;
                    while (_phase >= 1f)
                    {
                        AdvanceSource();
                        _phase -= 1f;
                    }
                }
            }
        }

        private void EnsureSamples()
        {
            if (!_hasCurrent && _queue.Count > 0)
            {
                _current = _queue.Dequeue();
                _hasCurrent = true;
            }
            if (!_hasNext && _queue.Count > 0)
            {
                _next = _queue.Dequeue();
                _hasNext = true;
            }
            if (!_hasNext && !_hasCurrent && _queue.Count == 0)
            {
                _current = 0f;
                _next = 0f;
            }
        }

        private void AdvanceSource()
        {
            if (!_hasCurrent)
            {
                if (_queue.Count > 0)
                {
                    _current = _queue.Dequeue();
                    _hasCurrent = true;
                }
                else
                {
                    return;
                }
            }

            if (_hasNext)
            {
                _current = _next;
                _hasCurrent = true;
                _hasNext = false;
                if (_queue.Count > 0)
                {
                    _next = _queue.Dequeue();
                    _hasNext = true;
                }
            }
            else if (_queue.Count > 0)
            {
                _next = _queue.Dequeue();
                _hasNext = true;
            }
        }

    }
}
