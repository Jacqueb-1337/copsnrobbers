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
        public const string Version = "0.2.3";
        public const byte VoiceEvent = 197;
        private const string LogPath = "/storage/emulated/0/CNRMods/voicechat.log";
        private static bool _loaded;
        private static bool _audioPermissionRequested;

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
        private const int FrameSamples = 320; // 40 ms at 8 kHz
        private const float SendInterval = 0.04f;
        private const float SilenceThreshold = 0.018f;
        private const float RemoteGain = 1.4f;

        private bool _probed;
        private bool _proxyInstalled;
        private Type _pnTypeCache;
        private string _micDevice;
        private AudioClip _micClip;
        private int _lastMicPos;
        private float _sendTimer;
        private int _seq;
        private readonly float[] _capture = new float[FrameSamples];
        private readonly byte[] _encoded = new byte[FrameSamples];
        private readonly Dictionary<string, RemoteVoice> _remotes = new Dictionary<string, RemoteVoice>();

        void Start()
        {
            ProbeRuntime();
            StartMicrophone();
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
            if (!IsInRoom()) return;
            if (_micClip == null) StartMicrophone();
            CaptureAndSend();
            CleanupRemotes();
        }

        void OnDestroy()
        {
            try
            {
                if (!string.IsNullOrEmpty(_micDevice) && Microphone.IsRecording(_micDevice))
                    Microphone.End(_micDevice);
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
            ht["rate"] = CaptureRate;
            ht["vc"] = (byte[])_encoded.Clone();
            RaiseVoiceEvent(ht);
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
                float s = Mathf.Clamp(src[i], -1f, 1f);
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

        private string LocalPeerId()
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
        private const int BufferSamples = Rate * 3;
        private readonly string _sender;
        private readonly GameObject _go;
        private readonly AudioSource _source;
        private readonly AudioClip _clip;
        private readonly float[] _zeros = new float[BufferSamples];
        private int _writePos;
        private bool _started;
        public float LastPushTime;

        public RemoteVoice(string sender)
        {
            _sender = sender;
            _go = new GameObject("VoiceChat_Remote_" + sender);
            UnityEngine.Object.DontDestroyOnLoad(_go);
            _source = _go.AddComponent<AudioSource>();
            _source.loop = true;
            _source.volume = 1f;
            _clip = AudioClip.Create("VoiceChat_" + sender, BufferSamples, 1, Rate, false, false);
            _clip.SetData(_zeros, 0);
            _source.clip = _clip;
            LastPushTime = Time.time;
        }

        public void Push(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            LastPushTime = Time.time;
            float[] decoded = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                decoded[i] = Mathf.Clamp((((float)data[i] - 128f) / 127f) * VoiceChatHookRemoteGain(), -1f, 1f);
            }

            if (_writePos + decoded.Length <= BufferSamples)
            {
                _clip.SetData(decoded, _writePos);
                _writePos = (_writePos + decoded.Length) % BufferSamples;
            }
            else
            {
                int first = BufferSamples - _writePos;
                float[] a = new float[first];
                float[] b = new float[decoded.Length - first];
                Array.Copy(decoded, 0, a, 0, a.Length);
                Array.Copy(decoded, a.Length, b, 0, b.Length);
                _clip.SetData(a, _writePos);
                _clip.SetData(b, 0);
                _writePos = b.Length;
            }

            if (!_started)
            {
                int start = _writePos - (Rate / 5);
                if (start < 0) start += BufferSamples;
                _source.timeSamples = start;
                _source.Play();
                _started = true;
                VoiceChatEntry.Log("Remote voice playback started sender=" + _sender);
            }
            else if (!_source.isPlaying)
            {
                _source.Play();
            }
        }

        private static float VoiceChatHookRemoteGain()
        {
            return 1.4f;
        }

        public void Destroy()
        {
            try { if (_go != null) UnityEngine.Object.Destroy(_go); }
            catch { }
        }
    }
}
