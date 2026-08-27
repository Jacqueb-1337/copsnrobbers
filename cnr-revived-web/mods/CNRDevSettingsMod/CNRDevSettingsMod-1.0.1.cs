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
        public const string Version = "1.0.1";

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
        private const string PrefShowSpeed = "CNRDev_ShowSpeed";
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
        private bool _showSpeed;
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
        private float _speed;
        private Transform _speedTarget;
        private Vector3 _lastSpeedPosition;
        private bool _haveSpeedPosition;
        private readonly List<string> _overlayRows = new List<string>();
        private float _nextMemorySample;
        private float _nextFlush;
        private int _lastGc0;
        private int _lastGc1;
        private int _lastGc2;
        private int _mainThreadId;
        private long _lastNativeHeap;
        private GUIStyle _overlayRowStyle;
        private static MethodInfo _uiSection;
        private static MethodInfo _uiLabel;
        private static MethodInfo _uiHint;
        private static MethodInfo _uiSpace;
        private static MethodInfo _uiToggle;
        private static MethodInfo _uiDisclosure;

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
            _showSpeed = PlayerPrefs.GetInt(PrefShowSpeed, 0) != 0;
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

            UpdateSpeed(frameSeconds);

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

        private void UpdateSpeed(float frameSeconds)
        {
            if (!_showSpeed)
            {
                _speed = 0f;
                _speedTarget = null;
                _haveSpeedPosition = false;
                return;
            }

            Transform target = FindLocalPlayerTransform();
            if (target == null)
            {
                _speed = 0f;
                _speedTarget = null;
                _haveSpeedPosition = false;
                return;
            }

            if (_speedTarget != target)
            {
                _speedTarget = target;
                _lastSpeedPosition = target.position;
                _haveSpeedPosition = true;
                _speed = 0f;
                return;
            }

            Vector3 pos = target.position;
            if (_haveSpeedPosition && frameSeconds > 0.0001f)
            {
                Vector3 delta = pos - _lastSpeedPosition;
                delta.y = 0f;
                _speed = delta.magnitude / frameSeconds;
            }
            _lastSpeedPosition = pos;
            _haveSpeedPosition = true;
        }

        private static Transform FindLocalPlayerTransform()
        {
            try
            {
                GameObject player = GameObject.FindWithTag("Player");
                return player != null ? player.transform : null;
            }
            catch { return null; }
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
                    CacheSettingsUI(assemblies[i]);
                    MethodInfo registerTab = t.GetMethod("RegisterTab", BindingFlags.Public | BindingFlags.Static, null,
                        new Type[] { typeof(string), typeof(string), typeof(Action<float>) }, null);
                    MethodInfo register = t.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
                    if (registerTab == null && register == null)
                    {
                        if (!_registerWaitLogged)
                        {
                            _registerWaitLogged = true;
                            Debug.LogWarning("[CNRDevSettings] SettingsExternalTabs registration API is missing");
                        }
                        return;
                    }
                    Action<float> drawer = DrawDevTabStatic;
                    if (registerTab != null)
                        registerTab.Invoke(null, new object[] { "CNRDevSettingsMod", "DEV", drawer });
                    else
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

        private static void CacheSettingsUI(Assembly assembly)
        {
            if (assembly == null) return;
            try
            {
                Type ui = assembly.GetType("CNRSettingsMod.SettingsUI", false);
                if (ui == null) return;
                _uiSection = ui.GetMethod("Section", BindingFlags.Public | BindingFlags.Static);
                _uiLabel = ui.GetMethod("Label", BindingFlags.Public | BindingFlags.Static);
                _uiHint = ui.GetMethod("Hint", BindingFlags.Public | BindingFlags.Static);
                _uiSpace = ui.GetMethod("Space", BindingFlags.Public | BindingFlags.Static);
                _uiToggle = ui.GetMethod("Toggle", BindingFlags.Public | BindingFlags.Static);
                _uiDisclosure = ui.GetMethod("Disclosure", BindingFlags.Public | BindingFlags.Static);
            }
            catch { }
        }

        public static void DrawDevTabStatic(float contentWidth)
        {
            if (_instance != null) _instance.DrawDevTab(contentWidth);
        }

        private void DrawDevTab(float contentWidth)
        {
            HostSection("Overlay");
            HostSpace(4f);

            bool showFps = HostToggle("Show FPS", _showFps);
            if (showFps != _showFps)
            {
                _showFps = showFps;
                PlayerPrefs.SetInt(PrefShowFps, _showFps ? 1 : 0);
                PlayerPrefs.Save();
            }
            HostHint("Displays live FPS and average frame time without requiring verbose logging.");
            HostSpace(8f);

            bool showSpeed = HostToggle("Show speed", _showSpeed);
            if (showSpeed != _showSpeed)
            {
                _showSpeed = showSpeed;
                PlayerPrefs.SetInt(PrefShowSpeed, _showSpeed ? 1 : 0);
                PlayerPrefs.Save();
                _haveSpeedPosition = false;
            }
            HostHint("Displays actual horizontal player speed in world units per second. Overlay rows stack automatically in enabled order.");
            HostSpace(14f);

            HostSection("Diagnostics");
            HostSpace(4f);
            bool verbose = HostToggle("Verbose logging", _verbose);
            if (verbose != _verbose)
            {
                _verbose = verbose;
                PlayerPrefs.SetInt(PrefVerbose, _verbose ? 1 : 0);
                PlayerPrefs.Save();
                if (_verbose) WriteScene("VERBOSE_ENABLED", SafeSceneName(), Application.loadedLevel);
                else FlushWriters();
            }
            HostHint("Master switch for diagnostic capture. Log files are recreated on every app launch.");
            HostSpace(8f);

            _diagnosticsExpanded = HostDisclosure("Diagnostic files", _diagnosticsExpanded);
            if (_diagnosticsExpanded)
            {
                HostSpace(4f);
                _diagFrames = SetCategory(PrefFrames, _diagFrames, HostToggle("Frame timing / FPS", _diagFrames));
                HostHint("frames.log");
                _diagHitches = SetCategory(PrefHitches, _diagHitches, HostToggle("Main-thread hitches / stalls", _diagHitches));
                HostHint("hitches.log");
                _diagMemory = SetCategory(PrefMemory, _diagMemory, HostToggle("Memory / GC", _diagMemory));
                HostHint("memory.log");
                _diagScenes = SetCategory(PrefScenes, _diagScenes, HostToggle("Scene / load activity", _diagScenes));
                HostHint("scenes.log");
                _diagUnity = SetCategory(PrefUnity, _diagUnity, HostToggle("Unity log capture", _diagUnity));
                HostHint("unity.log");
                HostSpace(6f);
                HostHint("/storage/emulated/0/CNRMods/diagnostics/");
                HostHint("Hitch records include frame gap, delta/smoothed delta, GC counters, managed/native heap, scene, frame number, and main-thread ID.");
            }

            HostSpace(16f);
            HostSection("Planned diagnostics");
            HostSpace(4f);

            _logSuppressExpanded = HostDisclosure("Suppress mod .log writes  [COMING SOON]", _logSuppressExpanded);
            if (_logSuppressExpanded)
                HostHint("Placeholder: this will become a per-mod checklist plus an ALL MOD LOGS master switch.");

            DrawPlaceholder("Live frame-time graph");
            DrawPlaceholder("Collider / collision-chunk overlay");
            DrawPlaceholder("Water + climbable volume overlay");
            DrawPlaceholder("Photon / network inspector");
            DrawPlaceholder("Map streaming / active-chunk inspector");
        }

        private void HostSection(string text)
        {
            if (InvokeHostVoid(_uiSection, new object[] { text })) return;
            GUILayout.Space(2f);
            GUILayout.Label((text ?? "").ToUpperInvariant());
        }

        private void HostLabel(string text)
        {
            if (InvokeHostVoid(_uiLabel, new object[] { text })) return;
            GUILayout.Label(text ?? "");
        }

        private void HostHint(string text)
        {
            if (InvokeHostVoid(_uiHint, new object[] { text })) return;
            GUILayout.Label("  " + (text ?? ""));
        }

        private void HostSpace(float pixels)
        {
            if (InvokeHostVoid(_uiSpace, new object[] { pixels })) return;
            GUILayout.Space(Mathf.Max(0f, pixels));
        }

        private bool HostToggle(string label, bool value)
        {
            if (_uiToggle != null)
            {
                try
                {
                    object result = _uiToggle.Invoke(null, new object[] { label, value });
                    if (result is bool) return (bool)result;
                }
                catch { }
            }
            bool fallback = GUILayout.Toggle(value, label ?? "");
            return fallback;
        }

        private bool HostDisclosure(string label, bool expanded)
        {
            if (_uiDisclosure != null)
            {
                try
                {
                    object result = _uiDisclosure.Invoke(null, new object[] { label, expanded });
                    if (result is bool) return (bool)result;
                }
                catch { }
            }
            if (GUILayout.Button((expanded ? "v  " : ">  ") + (label ?? ""))) expanded = !expanded;
            return expanded;
        }

        private static bool InvokeHostVoid(MethodInfo method, object[] args)
        {
            if (method == null) return false;
            try
            {
                method.Invoke(null, args);
                return true;
            }
            catch { return false; }
        }

        private bool SetCategory(string pref, bool current, bool value)
        {
            if (value == current) return current;
            PlayerPrefs.SetInt(pref, value ? 1 : 0);
            PlayerPrefs.Save();
            if (!value) FlushWriters();
            return value;
        }

        private void DrawPlaceholder(string label)
        {
            HostSpace(5f);
            HostLabel(label + "  [COMING SOON]");
        }

        private void OnGUI()
        {
            if (!_showFps && !_showSpeed) return;
            if (_overlayRowStyle == null)
            {
                _overlayRowStyle = new GUIStyle(GUI.skin.box);
                _overlayRowStyle.alignment = TextAnchor.MiddleLeft;
                _overlayRowStyle.fontSize = 16;
                _overlayRowStyle.fontStyle = FontStyle.Bold;
                _overlayRowStyle.normal.textColor = Color.white;
                _overlayRowStyle.padding = new RectOffset(10, 10, 4, 4);
            }

            _overlayRows.Clear();
            if (_showFps)
                _overlayRows.Add("FPS  " + _fps.ToString("F1") + "   " + _avgFrameMs.ToString("F1") + " ms");
            if (_showSpeed)
                _overlayRows.Add("SPEED  " + _speed.ToString("F2") + " u/s");

            GUI.depth = -500;
            const float x = 10f;
            const float y = 10f;
            const float width = 205f;
            const float rowHeight = 34f;
            const float rowGap = 2f;
            for (int i = 0; i < _overlayRows.Count; i++)
            {
                float rowY = y + i * (rowHeight + rowGap);
                GUI.Box(new Rect(x, rowY, width, rowHeight), _overlayRows[i], _overlayRowStyle);
            }
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
