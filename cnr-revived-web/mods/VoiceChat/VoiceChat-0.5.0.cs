// VoiceChat.cs - v0.2.0
//
// Photon custom-event voice chat prototype for Cops N Robbers.
// Entry point: VoiceChatEntry.Load() - discovered by CNRMod's DLL scanner.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExitGames.Client.Photon;
using UnityEngine;

namespace CNRVoiceChat
{
    public class VoiceChatEntry
    {
        public const string Version = "0.5.0";
        public const byte VoiceEvent = 197;
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
            if (ev.Code == VoiceChatEntry.VoiceEvent && _hook != null)
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

        void Start()
        {
            ProbeRuntime();
            EnsureHudTextures();
            _webrtcBridge = new WebRtcBridge();
            _webRtcSupported = _webrtcBridge.ProbeSupport();
            _webRtcActive = false;
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
            bool inGame = IsInGameRoom();
            if (_wasInGame && !inGame)
            {
                ShutdownVoicePath();
            }
            _wasInGame = inGame;
            TryStartWebRtc();
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
                Hashtable ht = ExtractPayload(ev);
                if (ht == null || !ht.ContainsKey("vc")) return;
                string sender = ht.ContainsKey("id") ? ht["id"] as string : null;
                if (string.IsNullOrEmpty(sender)) sender = SenderId(ev);
                if (string.IsNullOrEmpty(sender)) sender = "unknown";
                if (sender == LocalPeerId()) return;

                string kind = ht.ContainsKey("kind") ? ht["kind"] as string : null;
                if (string.IsNullOrEmpty(kind)) kind = "pcm";
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
            RaiseVoiceEvent(ht);
        }

        private void TryStartWebRtc()
        {
            if (_webrtcBridge == null || !_webRtcSupported || _webRtcActive) return;
            if (_webrtcBridge.TryConnect(LocalPeerId(), IsInGameRoom(), GetRoomName(), LocalDisplayName()))
            {
                _webRtcActive = true;
                VoiceChatEntry.Log("WebRTC session requested");
                SyncWebViewMuteState();
            }
        }

        private void SyncWebViewMuteState()
        {
            if (_webrtcBridge == null) return;
            _webrtcBridge.UpdateMuteState(_micMuted, VoiceChatEntry.SpeakerMuted);
        }

        private void PollWebMediator()
        {
            try
            {
                Type webType = FindType("WebMediator");
                if (webType == null) return;
                MethodInfo poll = webType.GetMethod("PollMessage", BindingFlags.Public | BindingFlags.Static);
                if (poll == null) return;
                object msg = poll.Invoke(null, null);
                if (msg == null) return;
                Type mt = msg.GetType();
                FieldInfo pathField = mt.GetField("path", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo argsField = mt.GetField("args", BindingFlags.Public | BindingFlags.Instance);
                string path = pathField != null ? Convert.ToString(pathField.GetValue(msg)) : null;
                Hashtable args = argsField != null ? argsField.GetValue(msg) as Hashtable : null;
                if (string.IsNullOrEmpty(path)) return;
                if (path == "/note" && args != null && args.ContainsKey("text"))
                {
                    VoiceChatEntry.Log("WebView note: " + Convert.ToString(args["text"]));
                }
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("PollWebMediator error: " + ex.Message);
            }
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
            try
            {
                object peer = GetNetworkingPeer();
                if (peer == null) return;
                MethodInfo raise = peer.GetType().GetMethod("OpRaiseEvent",
                    new Type[] { typeof(byte), typeof(Hashtable), typeof(bool), typeof(byte) });
                if (raise == null) return;
                raise.Invoke(peer, new object[] { VoiceChatEntry.VoiceEvent, ht, false, (byte)0 });
            }
            catch (Exception ex) { VoiceChatEntry.Log("RaiseVoiceEvent error: " + ex.Message); }
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
        private const string VoicePageUrl = "https://play.jacqueb.me/voicechat/index.html";
        private readonly Dictionary<string, WebRtcPeerState> _peers = new Dictionary<string, WebRtcPeerState>();
        private bool _probed;
        private bool _supported;
        private bool _connected;
        private string _localPeerId;
        private string _roomName;
        private string _displayName;
        private Type _webMediatorType;
        private MethodInfo _miInstall;
        private MethodInfo _miLoadUrl;
        private MethodInfo _miSetMargin;
        private MethodInfo _miShow;
        private MethodInfo _miHide;
        private MethodInfo _miTransparent;

        public bool ProbeSupport()
        {
            if (_probed) return _supported;
            _probed = true;
            _webMediatorType = FindType("WebMediator");
            if (_webMediatorType != null)
            {
                _miInstall = _webMediatorType.GetMethod("Install", BindingFlags.Public | BindingFlags.Static);
                _miLoadUrl = _webMediatorType.GetMethod("LoadUrl", BindingFlags.Public | BindingFlags.Static);
                _miSetMargin = _webMediatorType.GetMethod("SetMargin", BindingFlags.Public | BindingFlags.Static);
                _miShow = _webMediatorType.GetMethod("Show", BindingFlags.Public | BindingFlags.Static);
                _miHide = _webMediatorType.GetMethod("Hide", BindingFlags.Public | BindingFlags.Static);
                _miTransparent = _webMediatorType.GetMethod("MakeTransparentWebViewBackground", BindingFlags.Public | BindingFlags.Static);
            }
            _supported = _webMediatorType != null && _miLoadUrl != null && _miShow != null && _miHide != null;
            VoiceChatEntry.Log("WebRTC support probe: " + _supported);
            if (_supported)
            {
                VoiceChatEntry.Log("WebView voice bridge enabled via WebMediator.");
            }
            return _supported;
        }

        public bool TryConnect(string localPeerId, bool inGame, string roomName, string displayName)
        {
            if (!_supported) return false;
            if (!inGame) return false;
            _connected = true;
            _localPeerId = localPeerId;
            _roomName = string.IsNullOrEmpty(roomName) ? "offline" : roomName;
            _displayName = string.IsNullOrEmpty(displayName) ? "local" : displayName;
            TryInvoke(_miInstall);
            TryInvoke(_miTransparent);
            int left = Screen.width / 4;
            int right = Screen.width / 4;
            int top = Screen.height / 4;
            int bottom = Screen.height / 4;
            TryInvoke(_miSetMargin, left, top, right, bottom);
            TryInvoke(_miLoadUrl, BuildVoicePageUrl());
            TryInvoke(_miLoadUrl, "https://play.jacqueb.me/note?text=" + Uri.EscapeDataString("voice bridge bootstrap " + _localPeerId + " " + _roomName));
            TryInvoke(_miShow);
            VoiceChatEntry.Log("WebView voice bridge connected peer=" + localPeerId + " room=" + _roomName + " inGame=" + inGame);
            return true;
        }

        public bool CanCaptureAudio()
        {
            return _supported && _connected;
        }

        public void CaptureLocalAudioFrame()
        {
            if (!_connected) return;
            // The browser page handles mic capture and transport.
        }

        public void Tick()
        {
            if (!_supported) return;
            float now = Time.realtimeSinceStartup;
            List<string> expired = null;
            foreach (KeyValuePair<string, WebRtcPeerState> kv in _peers)
            {
                if (kv.Value != null && now - kv.Value.LastSignalTime > 30f)
                {
                    if (expired == null) expired = new List<string>();
                    expired.Add(kv.Key);
                }
            }
            if (expired == null) return;
            for (int i = 0; i < expired.Count; i++) _peers.Remove(expired[i]);
        }

        public bool IsSessionActive(string peerId)
        {
            WebRtcPeerState peer;
            return !string.IsNullOrEmpty(peerId) && _peers.TryGetValue(peerId, out peer) && peer != null && peer.State != "new";
        }

        public void DisconnectAll()
        {
            _connected = false;
            _localPeerId = null;
            _roomName = null;
            _displayName = null;
            _peers.Clear();
            TryInvoke(_miHide);
            TryInvoke(_miLoadUrl, "about:blank");
            VoiceChatEntry.Log("WebRTC bridge disconnected");
        }

        public void OnSignal(string sender, Hashtable payload)
        {
            if (!_supported) return;
            if (string.IsNullOrEmpty(sender)) sender = "unknown";
            string kind = payload.ContainsKey("sig") ? Convert.ToString(payload["sig"]) : "unknown";
            WebRtcPeerState peer;
            if (!_peers.TryGetValue(sender, out peer))
            {
                peer = new WebRtcPeerState { PeerId = sender, State = "new" };
                _peers[sender] = peer;
            }
            peer.LastSignalTime = Time.realtimeSinceStartup;
            if (kind == "offer") peer.LastOffer = payload.ContainsKey("sdp") ? Convert.ToString(payload["sdp"]) : null;
            else if (kind == "answer") peer.LastAnswer = payload.ContainsKey("sdp") ? Convert.ToString(payload["sdp"]) : null;
            else if (kind == "ice") peer.LastCandidate = payload.ContainsKey("candidate") ? Convert.ToString(payload["candidate"]) : null;
            peer.State = kind;
            VoiceChatEntry.Log("WebRTC signal from " + sender + " kind=" + kind);
        }

        public void SendSignal(string kind, string sdp, string candidate, int? mline)
        {
            if (!_connected || string.IsNullOrEmpty(_localPeerId)) return;
            string js = "if(window.__cnrVoice&&window.__cnrVoice.onSignal){window.__cnrVoice.onSignal(" +
                        JsQuote(kind) + "," +
                        JsQuote(sdp ?? "") + "," +
                        JsQuote(candidate ?? "") + "," +
                        (mline.HasValue ? mline.Value.ToString() : "null") +
                        ");}";
            ExecuteJs(js);
            VoiceChatEntry.Log("WebView signal queued kind=" + kind + " from=" + _localPeerId);
        }

        public void UpdateMuteState(bool micMuted, bool speakerMuted)
        {
            if (!_connected) return;
            ExecuteJs("if(window.__cnrVoice){window.__cnrVoice.setMicMuted(" + (micMuted ? "true" : "false") + ");window.__cnrVoice.setSpeakerMuted(" + (speakerMuted ? "true" : "false") + ");}");
        }

        private static Type FindType(string name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        private string BuildVoicePageUrl()
        {
            string room = Uri.EscapeDataString(_roomName ?? "offline");
            string peer = Uri.EscapeDataString(_localPeerId ?? "local");
            string name = Uri.EscapeDataString(_displayName ?? "local");
            return VoicePageUrl + "?room=" + room + "&peer=" + peer + "&name=" + name;
        }

        private void ExecuteJs(string js)
        {
            if (!_supported || string.IsNullOrEmpty(js)) return;
            TryInvoke(_miLoadUrl, "javascript:" + js);
        }

        private static string JsQuote(string value)
        {
            if (value == null) value = "";
            value = value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n");
            return "'" + value + "'";
        }

        private void TryInvoke(MethodInfo mi, params object[] args)
        {
            try
            {
                if (mi != null) mi.Invoke(null, args);
            }
            catch (Exception ex)
            {
                VoiceChatEntry.Log("WebMediator invoke failed: " + ex.Message);
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
