// CNRDevSettingsMod.cs -- developer diagnostics/settings extension for Cops N Robbers
// Entry point: CNRDevSettingsMod.DevSettingsModEntry.Load()

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRDevSettingsMod
{
    public static class DevSettingsModEntry
    {
        public const string Version = "1.0.0";

        public static void Load()
        {
            try
            {
                if (GameObject.Find("CNRDevSettingsMod") != null) return;

                GameObject go = new GameObject("CNRDevSettingsMod");
                go.AddComponent<DevSettingsHook>();
                GameObject.DontDestroyOnLoad(go);
                Debug.Log("[CNRDevSettings] loaded v" + Version);
            }
            catch (Exception ex)
            {
                try { Debug.LogError("[CNRDevSettings] Load failed: " + ex); } catch { }
            }
        }
    }

    public class DevSettingsHook : MonoBehaviour
    {
        private const string DiagnosticsDir = "/storage/emulated/0/CNRMods/diagnostics";
        private const string FramesFile = "frames.log";
        private const string HitchesFile = "hitches.log";
        private const string MemoryFile = "memory.log";
        private const string ScenesFile = "scenes.log";
        private const string UnityFile = "unity.log";

        private const string PrefShowFps = "CNRDev_ShowFPS";
        private const string PrefVerbose = "CNRDev_Verbose";
        private const string PrefFrames = "CNRDev_Diag_Frames";
        private const string PrefHitches = "CNRDev_Diag_Hitches";
        private const string PrefMemory = "CNRDev_Diag_Memory";
        private const string PrefScenes = "CNRDev_Diag_Scenes";
        private const string PrefUnity = "CNRDev_Diag_Unity";

        private static DevSettingsHook _instance;
        private static readonly object WriterLock = new object();
        private static readonly Dictionary<string, StreamWriter> Writers = new Dictionary<string, StreamWriter>();

        private bool _showFps;
        private volatile bool _verbose;
        private volatile bool _diagFrames;
        private volatile bool _diagHitches;
        private volatile bool _diagMemory;
        private volatile bool _diagScenes;
        private volatile bool _diagUnity;
        private bool _diagnosticsExpanded = true;
        private bool _logSuppressExpanded;
        private bool _registeredTab;
        private bool _registerWaitLogged;
        private float _nextRegisterTry;

        private float _lastRealtime;
        private float _frameWindowSeconds;
        private int _frameWindowCount;
        private float _frameWindowTotalMs;
        private float _frameWindowMinMs = 999999f;
        private float _frameWindowMaxMs;
        private float _fps;
        private float _avgFrameMs;
        private float _nextMemorySample;
        private float _nextFlush;
        private int _lastGc0;
        private int _lastGc1;
        private int _lastGc2;
        private int _mainThreadId;
        private long _lastNativeHeap;
        private GUIStyle _fpsStyle;
        private GUIStyle _devRowStyle;
        private GUIStyle _devHintStyle;
        private GUIStyle _devHeaderStyle;

        private void Awake()
        {
            _instance = this;
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            LoadPrefs();
            ResetDiagnosticFiles();
            RegisterUnityLogCapture();
        }

        private void Start()
        {
            _lastRealtime = Time.realtimeSinceStartup;
            _nextMemorySample = _lastRealtime + 1f;
            _nextFlush = _lastRealtime + 1f;
            _lastGc0 = SafeGcCount(0);
            _lastGc1 = SafeGcCount(1);
            _lastGc2 = SafeGcCount(2);
            Debug.Log("[CNRDevSettings] Start; locating Settings tab host");
            TryRegisterDevTab();
            WriteScene("START", Application.loadedLevelName, Application.loadedLevel);
        }

        private void LoadPrefs()
        {
            _showFps = PlayerPrefs.GetInt(PrefShowFps, 0) != 0;
            _verbose = PlayerPrefs.GetInt(PrefVerbose, 0) != 0;
            _diagFrames = PlayerPrefs.GetInt(PrefFrames, 1) != 0;
            _diagHitches = PlayerPrefs.GetInt(PrefHitches, 1) != 0;
            _diagMemory = PlayerPrefs.GetInt(PrefMemory, 1) != 0;
            _diagScenes = PlayerPrefs.GetInt(PrefScenes, 1) != 0;
            _diagUnity = PlayerPrefs.GetInt(PrefUnity, 1) != 0;
        }

        private void Update()
        {
            float now = Time.realtimeSinceStartup;
            float frameSeconds = now - _lastRealtime;
            _lastRealtime = now;
            if (frameSeconds < 0f) frameSeconds = 0f;

            if (!_registeredTab && now >= _nextRegisterTry)
            {
                _nextRegisterTry = now + 1f;
                TryRegisterDevTab();
            }

            float frameMs = frameSeconds * 1000f;
            if (frameSeconds > 0f)
            {
                _frameWindowSeconds += frameSeconds;
                _frameWindowCount++;
                _frameWindowTotalMs += frameMs;
                if (frameMs < _frameWindowMinMs) _frameWindowMinMs = frameMs;
                if (frameMs > _frameWindowMaxMs) _frameWindowMaxMs = frameMs;
            }

            if (_verbose && _diagHitches && frameSeconds >= 0.100f)
                WriteHitch(frameSeconds, now);

            if (_frameWindowSeconds >= 1f)
            {
                _fps = _frameWindowSeconds > 0f ? _frameWindowCount / _frameWindowSeconds : 0f;
                _avgFrameMs = _frameWindowCount > 0 ? _frameWindowTotalMs / _frameWindowCount : 0f;

                if (_verbose && _diagFrames)
                {
                    WriteDiagnostic(FramesFile,
                        "fps=" + _fps.ToString("F1") +
                        " avg_ms=" + _avgFrameMs.ToString("F2") +
                        " min_ms=" + (_frameWindowMinMs < 999999f ? _frameWindowMinMs.ToString("F2") : "0") +
                        " max_ms=" + _frameWindowMaxMs.ToString("F2") +
                        " frames=" + _frameWindowCount +
                        " scene=" + SafeSceneName());
                }

                _frameWindowSeconds = 0f;
                _frameWindowCount = 0;
                _frameWindowTotalMs = 0f;
                _frameWindowMinMs = 999999f;
                _frameWindowMaxMs = 0f;
            }

            if (_verbose && _diagMemory && now >= _nextMemorySample)
            {
                _nextMemorySample = now + 2f;
                WriteMemorySample();
            }

            if (now >= _nextFlush)
            {
                _nextFlush = now + 1f;
                FlushWriters();
            }
        }

        private void WriteHitch(float frameSeconds, float now)
        {
            int gc0 = SafeGcCount(0);
            int gc1 = SafeGcCount(1);
            int gc2 = SafeGcCount(2);
            bool gcChanged = gc0 != _lastGc0 || gc1 != _lastGc1 || gc2 != _lastGc2;
            long managed = SafeManagedBytes();
            long native = SafeNativeHeapBytes();

            WriteDiagnostic(HitchesFile,
                "HITCH ms=" + (frameSeconds * 1000f).ToString("F1") +
                " realtime=" + now.ToString("F3") +
                " frame=" + Time.frameCount +
                " delta_ms=" + (Time.deltaTime * 1000f).ToString("F1") +
                " smooth_ms=" + (Time.smoothDeltaTime * 1000f).ToString("F1") +
                " timescale=" + Time.timeScale.ToString("F2") +
                " scene=" + SafeSceneName() +
                " main_thread=" + _mainThreadId +
                " gc=" + gc0 + "/" + gc1 + "/" + gc2 +
                " gc_changed=" + (gcChanged ? "1" : "0") +
                " managed_mb=" + BytesToMb(managed) +
                " native_mb=" + BytesToMb(native));

            _lastGc0 = gc0;
            _lastGc1 = gc1;
            _lastGc2 = gc2;
        }

        private void WriteMemorySample()
        {
            long managed = SafeManagedBytes();
            long native = SafeNativeHeapBytes();
            _lastNativeHeap = native;
            int gc0 = SafeGcCount(0);
            int gc1 = SafeGcCount(1);
            int gc2 = SafeGcCount(2);

            WriteDiagnostic(MemoryFile,
                "managed_mb=" + BytesToMb(managed) +
                " native_mb=" + BytesToMb(native) +
                " gc0=" + gc0 +
                " gc1=" + gc1 +
                " gc2=" + gc2 +
                " system_mb=" + SystemInfo.systemMemorySize +
                " scene=" + SafeSceneName());

            _lastGc0 = gc0;
            _lastGc1 = gc1;
            _lastGc2 = gc2;
        }

        private void OnLevelWasLoaded(int level)
        {
            WriteScene("LEVEL_LOADED", Application.loadedLevelName, level);
        }

        private void OnApplicationPause(bool paused)
        {
            WriteScene(paused ? "APP_PAUSE" : "APP_RESUME", SafeSceneName(), Application.loadedLevel);
            if (paused) FlushWriters();
        }

        private void OnApplicationFocus(bool focused)
        {
            WriteScene(focused ? "FOCUS_GAINED" : "FOCUS_LOST", SafeSceneName(), Application.loadedLevel);
        }

        private void WriteScene(string evt, string scene, int level)
        {
            if (!_verbose || !_diagScenes) return;
            WriteDiagnostic(ScenesFile,
                "event=" + evt +
                " scene=" + (string.IsNullOrEmpty(scene) ? "<none>" : scene) +
                " level=" + level +
                " frame=" + Time.frameCount +
                " realtime=" + Time.realtimeSinceStartup.ToString("F3"));
        }

        private void RegisterUnityLogCapture()
        {
            try
            {
                Application.RegisterLogCallbackThreaded(OnUnityLog);
            }
            catch
            {
                try { Application.RegisterLogCallback(OnUnityLog); } catch { }
            }
        }

        private void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            if (!_verbose || !_diagUnity) return;
            string msg = "type=" + type + " thread=" + System.Threading.Thread.CurrentThread.ManagedThreadId + " msg=" + OneLine(condition);
            if (!string.IsNullOrEmpty(stackTrace)) msg += " stack=" + OneLine(stackTrace);
            WriteDiagnostic(UnityFile, msg);
        }

        private void TryRegisterDevTab()
        {
            bool foundHost = false;
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Type t = assemblies[i].GetType("CNRSettingsMod.SettingsExternalTabs", false);
                    if (t == null) continue;
                    foundHost = true;
                    MethodInfo register = t.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
                    if (register == null)
                    {
                        if (!_registerWaitLogged)
                        {
                            _registerWaitLogged = true;
                            Debug.LogWarning("[CNRDevSettings] SettingsExternalTabs found but Register() is missing");
                        }
                        return;
                    }
                    Action<float> drawer = DrawDevTabStatic;
                    register.Invoke(null, new object[] { "DEV", drawer });
                    _registeredTab = true;
                    Debug.Log("[CNRDevSettings] registered DEV settings tab");
                    return;
                }
            }
            catch (Exception ex)
            {
                if (!_registerWaitLogged)
                {
                    _registerWaitLogged = true;
                    Debug.LogWarning("[CNRDevSettings] DEV tab registration failed; retrying: " + ex);
                }
                return;
            }

            if (!foundHost && !_registerWaitLogged)
            {
                _registerWaitLogged = true;
                Debug.Log("[CNRDevSettings] waiting for CNRSettingsMod external-tab host");
            }
        }

        public static void DrawDevTabStatic(float contentWidth)
        {
            if (_instance != null) _instance.DrawDevTab(contentWidth);
        }

        private void DrawDevTab(float contentWidth)
        {
            EnsureDevStyles();
            DrawHeader("Developer Tools");
            GUILayout.Space(4f);

            bool showFps = DrawCheckRow("SHOW FPS", _showFps);
            if (showFps != _showFps)
            {
                _showFps = showFps;
                PlayerPrefs.SetInt(PrefShowFps, _showFps ? 1 : 0);
                PlayerPrefs.Save();
            }
            GUILayout.Label("Displays live FPS and average frame time without requiring verbose logging.", _devHintStyle);
            GUILayout.Space(8f);

            bool verbose = DrawCheckRow("Verbose logging", _verbose);
            if (verbose != _verbose)
            {
                _verbose = verbose;
                PlayerPrefs.SetInt(PrefVerbose, _verbose ? 1 : 0);
                PlayerPrefs.Save();
                if (_verbose) WriteScene("VERBOSE_ENABLED", SafeSceneName(), Application.loadedLevel);
                else FlushWriters();
            }
            GUILayout.Label("Master switch for diagnostics below. Logs are recreated on every app launch.", _devHintStyle);
            GUILayout.Space(8f);

            string arrow = _diagnosticsExpanded ? "v" : ">";
            if (GUILayout.Button(arrow + "  DIAGNOSTICS  (individual files)", _devRowStyle, GUILayout.Height(36f)))
                _diagnosticsExpanded = !_diagnosticsExpanded;

            if (_diagnosticsExpanded)
            {
                GUILayout.Space(4f);
                _diagFrames = SetCategory(PrefFrames, _diagFrames, DrawCheckRow("Frame timing / FPS        -> frames.log", _diagFrames));
                _diagHitches = SetCategory(PrefHitches, _diagHitches, DrawCheckRow("Main-thread hitches/stalls -> hitches.log", _diagHitches));
                _diagMemory = SetCategory(PrefMemory, _diagMemory, DrawCheckRow("Memory / GC                -> memory.log", _diagMemory));
                _diagScenes = SetCategory(PrefScenes, _diagScenes, DrawCheckRow("Scene / load activity      -> scenes.log", _diagScenes));
                _diagUnity = SetCategory(PrefUnity, _diagUnity, DrawCheckRow("Unity log capture           -> unity.log", _diagUnity));
                GUILayout.Space(4f);
                GUILayout.Label("/storage/emulated/0/CNRMods/diagnostics/", _devHintStyle);
                GUILayout.Label("Hitch records include frame gap, delta/smoothed delta, GC counters, managed/native heap, scene, frame number, and main-thread ID.", _devHintStyle);
            }

            GUILayout.Space(14f);
            DrawHeader("Planned Diagnostics");
            GUILayout.Space(4f);

            string logArrow = _logSuppressExpanded ? "v" : ">";
            if (GUILayout.Button(logArrow + "  Suppress mod .log writes  [COMING SOON]", _devRowStyle, GUILayout.Height(36f)))
                _logSuppressExpanded = !_logSuppressExpanded;
            if (_logSuppressExpanded)
            {
                GUILayout.Label("Placeholder: this will become a per-mod checklist plus an ALL MOD LOGS master switch.", _devHintStyle);
            }

            DrawPlaceholder("Live frame-time graph");
            DrawPlaceholder("Collider / collision-chunk overlay");
            DrawPlaceholder("Water + climbable volume overlay");
            DrawPlaceholder("Photon / network inspector");
            DrawPlaceholder("Map streaming / active-chunk inspector");
        }

        private bool SetCategory(string pref, bool current, bool value)
        {
            if (value == current) return current;
            PlayerPrefs.SetInt(pref, value ? 1 : 0);
            PlayerPrefs.Save();
            if (!value) FlushWriters();
            return value;
        }

        private bool DrawCheckRow(string label, bool value)
        {
            string text = (value ? "[x]  " : "[ ]  ") + label;
            if (GUILayout.Button(text, _devRowStyle, GUILayout.Height(34f))) value = !value;
            return value;
        }

        private void DrawPlaceholder(string label)
        {
            GUILayout.Label("[ ]  " + label + "   [COMING SOON]", _devRowStyle, GUILayout.Height(30f));
        }

        private void DrawHeader(string text)
        {
            GUILayout.Label(text.ToUpperInvariant(), _devHeaderStyle, GUILayout.Height(28f));
        }

        private void EnsureDevStyles()
        {
            if (_devRowStyle == null)
            {
                _devRowStyle = new GUIStyle(GUI.skin.button);
                _devRowStyle.alignment = TextAnchor.MiddleLeft;
                _devRowStyle.fontSize = 15;
                _devRowStyle.normal.textColor = Color.white;
                _devRowStyle.hover.textColor = Color.white;
                _devRowStyle.active.textColor = Color.white;
                _devRowStyle.padding = new RectOffset(12, 8, 4, 4);
            }
            if (_devHintStyle == null)
            {
                _devHintStyle = new GUIStyle(GUI.skin.label);
                _devHintStyle.fontSize = 13;
                _devHintStyle.wordWrap = true;
                _devHintStyle.normal.textColor = new Color(0.74f, 0.78f, 0.82f, 1f);
            }
            if (_devHeaderStyle == null)
            {
                _devHeaderStyle = new GUIStyle(GUI.skin.label);
                _devHeaderStyle.fontSize = 18;
                _devHeaderStyle.fontStyle = FontStyle.Bold;
                _devHeaderStyle.normal.textColor = new Color(0.92f, 0.92f, 1f, 1f);
            }
        }

        private void OnGUI()
        {
            if (!_showFps) return;
            if (_fpsStyle == null)
            {
                _fpsStyle = new GUIStyle(GUI.skin.box);
                _fpsStyle.alignment = TextAnchor.MiddleLeft;
                _fpsStyle.fontSize = 18;
                _fpsStyle.fontStyle = FontStyle.Bold;
                _fpsStyle.normal.textColor = Color.white;
                _fpsStyle.padding = new RectOffset(10, 10, 5, 5);
            }

            GUI.depth = -500;
            string text = "FPS  " + _fps.ToString("F1") + "\n" + _avgFrameMs.ToString("F1") + " ms";
            GUI.Box(new Rect(10f, 10f, 150f, 55f), text, _fpsStyle);
        }

        private static void ResetDiagnosticFiles()
        {
            try
            {
                lock (WriterLock)
                {
                    CloseWritersLocked();
                    Directory.CreateDirectory(DiagnosticsDir);
                    string[] names = new string[] { FramesFile, HitchesFile, MemoryFile, ScenesFile, UnityFile };
                    for (int i = 0; i < names.Length; i++)
                    {
                        string p = Path.Combine(DiagnosticsDir, names[i]);
                        try { if (File.Exists(p)) File.Delete(p); } catch { }
                        try
                        {
                            File.WriteAllText(p,
                                "# CNRDevSettingsMod " + DevSettingsModEntry.Version + " diagnostics\n" +
                                "# launch=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\n");
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private static void WriteDiagnostic(string fileName, string message)
        {
            try
            {
                lock (WriterLock)
                {
                    StreamWriter writer;
                    if (!Writers.TryGetValue(fileName, out writer) || writer == null)
                    {
                        Directory.CreateDirectory(DiagnosticsDir);
                        writer = new StreamWriter(Path.Combine(DiagnosticsDir, fileName), true);
                        writer.AutoFlush = false;
                        Writers[fileName] = writer;
                    }
                    writer.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message);
                }
            }
            catch { }
        }

        private static void FlushWriters()
        {
            try
            {
                lock (WriterLock)
                {
                    foreach (KeyValuePair<string, StreamWriter> kv in Writers)
                    {
                        try { if (kv.Value != null) kv.Value.Flush(); } catch { }
                    }
                }
            }
            catch { }
        }

        private static void CloseWritersLocked()
        {
            foreach (KeyValuePair<string, StreamWriter> kv in Writers)
            {
                try
                {
                    if (kv.Value != null)
                    {
                        kv.Value.Flush();
                        kv.Value.Close();
                    }
                }
                catch { }
            }
            Writers.Clear();
        }

        private void OnApplicationQuit()
        {
            FlushWriters();
            lock (WriterLock) { CloseWritersLocked(); }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            FlushWriters();
            lock (WriterLock) { CloseWritersLocked(); }
        }

        private static int SafeGcCount(int generation)
        {
            try { return GC.CollectionCount(generation); } catch { return -1; }
        }

        private static long SafeManagedBytes()
        {
            try { return GC.GetTotalMemory(false); } catch { return -1L; }
        }

        private static long SafeNativeHeapBytes()
        {
            try
            {
                if (Application.platform != RuntimePlatform.Android) return -1L;
                using (AndroidJavaClass debugClass = new AndroidJavaClass("android.os.Debug"))
                    return debugClass.CallStatic<long>("getNativeHeapAllocatedSize");
            }
            catch { return -1L; }
        }

        private static string BytesToMb(long bytes)
        {
            if (bytes < 0L) return "?";
            return (bytes / 1048576.0).ToString("F2");
        }

        private static string SafeSceneName()
        {
            try { return Application.loadedLevelName ?? ""; } catch { return ""; }
        }

        private static string OneLine(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\r", " ").Replace("\n", " | ");
        }
    }
}
