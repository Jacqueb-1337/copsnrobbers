// VoiceChat.cs - v0.1.0
//
// Standalone diagnostic scaffold for future in-game voice chat.
// Entry point: VoiceChatEntry.Load() - discovered by CNRMod's DLL scanner.

using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRVoiceChat
{
    public class VoiceChatEntry
    {
        public const string Version = "0.1.0";
        private const string LogPath = "/storage/emulated/0/CNRMods/voicechat.log";
        private static bool _loaded;

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                GameObject root = new GameObject("CNRVoiceChat_Root");
                root.AddComponent<VoiceChatHook>();
                UnityEngine.Object.DontDestroyOnLoad(root);
                RegisterWithCnrMod();
                Log("=== VoiceChat v" + Version + " loaded ===");
            }
            catch (Exception ex) { Log("Load error: " + ex); }
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

    public class VoiceChatHook : MonoBehaviour
    {
        private bool _probed;

        void Start()
        {
            ProbeRuntime();
        }

        void OnLevelWasLoaded(int level)
        {
            _probed = false;
            ProbeRuntime();
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
}
