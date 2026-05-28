// CNRRecordingMod.cs G�� screenshot-based in-game recording for Cops N Robbers
//
// HOW IT WORKS
//   The original game used a third-party SDK called Kamcord (dead since 2016).
//   This mod injects a custom Kamcord.Implementation subclass at startup (via
//   reflection into Kamcord.implementation_) so that the existing in-game record
//   button and main-menu recordings viewer button route through our code instead
//   of the dead no-op stubs.
//
// STORAGE
//   /sdcard/CNRMods/recordings/<yyyyMMdd_HHmmss>/
//     frame_00000.png, frame_00001.png, ...
//     recording.meta  (frames, fps, scale, date)
//
// CONSTANTS
//   CaptureFps   G�� frames captured per second (default 5)
//   CaptureScale G�� downscale factor applied before PNG encode (default 0.5)
//                  At 0.5x and 5fps a 720p screen G�� 1 MB/s (~60 MB/min).
//
// ENTRY POINT
//   CNRRecordingMod.RecordingModEntry.Load()
//   Chain: MainMenuDirector.Awake() -> MainMenuDirector.LoadMods() finds CNRMods.ModEntry (CNRMod.dll)
//          -> CNRMod.LoadExternalMods() scans /sdcard/CNRMods/*.dll for the first public static Load()
//          -> RecordingModEntry.Load() is found and called automatically.
//   No patching or subclassing of game classes required.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
// System.Runtime.InteropServices removed -- no longer using Marshal.Copy for buffer writes
using System.Threading;
using UnityEngine;

namespace CNRRecordingMod
{
    // G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��
    //  Entry point G�� CNRMod DLL scanner calls the first public static Load()
    // G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��
    public static class RecordingModEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/recording.log";
        public  const string Version = "1.39.26";

        private static bool _loaded = false;

        public static void Load()
        {
            if (_loaded) { Log("already loaded, skipping"); return; }
            _loaded = true;

            try { File.WriteAllText(LogPath, ""); } catch { }

            Log("=== CNRRecordingMod v" + Version + " loading ===");
            Log("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            try { Log("Unity: " + Application.unityVersion + "  platform=" + Application.platform); } catch { }
            try
            {
                using (var v = new AndroidJavaClass("android.os.Build$VERSION"))
                    Log("Android SDK_INT=" + v.GetStatic<int>("SDK_INT"));
            }
            catch (Exception ex) { Log("Android SDK check failed: " + ex.Message); }

            try
            {
                TryRegisterWithCNRMod();
                var go = new GameObject("CNRRecordingMod_Root");
                go.AddComponent<RecordingHook>();
                GameObject.DontDestroyOnLoad(go);
                Log("Load OK");
            }
            catch (Exception ex) { Log("Load() FATAL: " + ex); }
        }

        // Attempt to register with CNRMod's shared mod registry (for the
        // CNRModManager version display).  Silently skips if CNRMod isn't loaded.
        private static void TryRegisterWithCNRMod()
        {
            try
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!asm.GetName().Name.Equals("CNRMod", StringComparison.OrdinalIgnoreCase)) continue;
                    Type t = asm.GetType("CNRMods.ModEntry");
                    if (t == null) break;
                    MethodInfo m = t.GetMethod("RegisterMod",
                        BindingFlags.Public | BindingFlags.Static,
                        null, new Type[] { typeof(string), typeof(string) }, null);
                    if (m != null) m.Invoke(null, new object[] { "CNRRecordingMod", Version });
                    break;
                }
            }
            catch { }
        }

        public static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + msg + "\n"); } catch { }
            try { Debug.Log("[CNRRecording] " + msg); } catch { }
        }
    }

    // G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��
    //  Kamcord.Implementation replacement
    //
    //  Injected into Kamcord.implementation_ (private static) so that all calls
    //  through VideoRecordController G�� Kamcord.* G�� implementation().* reach us.
    //  Because Kamcord.Implementation is a public class its virtual methods are
    //  overridable from another assembly; we just need Assembly-CSharp-firstpass
    //  in the compile references (handled in build_mod.ps1).
    // G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��
    internal class RecordingKamcordImpl : Kamcord.Implementation
    {
        private readonly RecordingHook _hook;

        public RecordingKamcordImpl(RecordingHook hook)
        {
            _hook = hook;
        }

        public override bool IsEnabled()      { return true; }
        // Return false so Kamcord never activates its camera redirect (kamcordPreCamera
        // clears the EGL surface to grey and pumps game cameras into a pbuffer; if we
        // return true here that blit never reaches the screen and ReadPixels reads grey).
        // Start/Stop buttons call StartRecording()/StopRecording() directly regardless.
        public override bool IsRecording()    { return false; }
        public override void StartRecording() { _hook.StartCapture(); }
        public override void StopRecording()  { _hook.StopCapture(); }
        public override void ShowView()       { _hook.OpenViewer(); }
        public override void ShowWatchView()  { _hook.OpenViewer(); }
    }

    // G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��
    //  MonoBehaviour: lives for the whole session (DontDestroyOnLoad)
    //  Handles frame capture and the IMGUI recordings viewer.
    // G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��G��
    public class RecordingHook : MonoBehaviour
    {
        // Phase 1: capture raw NV12 frames to disk (no MediaCodec, no EGL interference)
        // Phase 2: after StopCapture, encode all saved frames to MP4 on a background thread
        //
        // MediaCodec.configure/start corrupts Unity's EGL context on this device even when
        // called from a background thread - the encoder driver touches EGL globally.
        // Solution: never call MediaCodec while IsCapturing=true.

        private const string RecordingsDir    = "/storage/emulated/0/CNRMods/recordings";
        private const int    VideoWidth       = 854;
        private const int    VideoHeight      = 480;
        private const int    VideoBitrate     = 2000000;
        private const int    VideoFps         = 30;
        private const int    COLOR_FMT_YUV420 = 21;
        private const int    INFO_TRY_AGAIN_LATER       = -1;
        private const int    INFO_OUTPUT_FORMAT_CHANGED  = -2;

        public  bool IsCapturing  { get; private set; }
        public  bool IsEncoding   { get; private set; }

        // Phase 1 state
        private string        _sessionDir;
        private int           _capturedFrames;
        private bool          _encodingFrame;
        internal Texture2D     _readTex;
        internal int           _scrW, _scrH;
        private byte[]        _nv12Buf;
        private RenderTexture  _captureRT;     // kept for possible alternative capture paths
        private Camera         _captureCamera; // primary (Skybox) scene camera
        private Camera         _kamcordPreCam; // Kamcord cameras — disabled during capture
        private Camera         _kamcordPostCam;//   so EGL backbuffer has un-redirected scene
        private PostRenderCapture _postRenderCapture; // dead — kept for compilation
        private int          _convertedFrames; // frames converted PNG→NV12 by ConverterCoroutine
        private Texture2D    _screenshotTex;   // reused for CaptureScreenshot PNG loading
        // Tightly packed NV12 — no alignment padding in capture files.
        // EncodeThread reads the real encoder stride via getInputFormat and re-strides.
        private const int CaptureStride = VideoWidth;   // 854, no padding
        private const int CaptureSliceH = VideoHeight;  // 480

        // Phase 2 state (background encode thread)
        private string    _encodeOutputPath;
        private string    _encodeError;

        // Viewer
        // PageSize is dynamic — computed per-frame from available panel height in DrawListView.
        private bool         _viewerOpen;
        private int          _viewerPage;
        private string       _selectedPath;
        private List<string> _recordings  = new List<string>();
        private List<long>   _recBytes    = new List<long>();
        private List<float>  _recDuration = new List<float>();
        private string       _statusMsg;

        // UI style cache (static: allocated once, shared across scenes)
        private static bool      _vrStylesOk;
        private static Font      _vrFont;
        private static Texture2D _vrPanelBg, _vrBtnTex, _vrHoverTex, _vrActiveTex;
        private static GUIStyle  _gsVrStatus, _gsVrTitle;
        private static GUIStyle  _gsVrTimeLabel, _gsVrTimeBig, _gsVrDateLabel;
        private static GUIStyle  _gsVrDetailLabel, _gsVrDetailRight, _gsVrDetailCenter;
        private static GUIStyle  _gsVrBtn, _gsVrGhost, _gsVrPlayBtn;
        private UICamera[]       _blockedUiCams;
        // In-app video player (VideoView Android overlay)
        private AndroidJavaObject _videoView;
        private bool   _mpPlaying;
        private int    _mpDurMs;
        private int    _mpCurMs;
        private string _loadedVideoPath;

        // Game scenes that have the in-game HUD (and the record button).
        private static readonly string[] GameScenes = new string[]
        {
            "FreeRun3_1","FreeRun4_1","FreeRun5_1","FreeRun6_1","FreeRun7_1",
            "FreeRun8_1","FreeRun9_1","FreeRun10_1","FreeRun11_1","FreeRun12_1",
            "FreeRun13_1","FreeRun14_1","FreeRun15_1","CRScene1"
        };
        private bool _btnHooked = false;

        private void Awake()
        {
            RecordingModEntry.Log("RecordingHook.Awake()");

            try { Directory.CreateDirectory(RecordingsDir); RecordingModEntry.Log("  recordings dir OK"); }
            catch (Exception ex) { RecordingModEntry.Log("  CreateDirectory error: " + ex.Message); }
            // Clean up stale screenshot files left by a previous crashed session.
            try { foreach (string f in Directory.GetFiles(Application.persistentDataPath, "cnr_ss_*.png")) File.Delete(f); }
            catch { }
            // Inject our Kamcord implementation so the existing video/watch button
            // on the main menu calls OpenViewer() instead of the dead Kamcord SDK.
            try
            {
                Type kamcord = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    kamcord = asm.GetType("Kamcord");
                    if (kamcord != null) break;
                }
                if (kamcord != null)
                {
                    var fi = kamcord.GetField("implementation_",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    if (fi != null)
                    {
                        fi.SetValue(null, new RecordingKamcordImpl(this));
                        RecordingModEntry.Log("  Kamcord.implementation_ injected OK");
                    }
                    else RecordingModEntry.Log("  WARN: Kamcord.implementation_ field not found");
                }
                else RecordingModEntry.Log("  WARN: Kamcord type not found");
            }
            catch (Exception ex) { RecordingModEntry.Log("  Kamcord inject err: " + ex.Message); }
        }

        private void OnLevelWasLoaded(int level)
        {
            _btnHooked = false;
            // Scene change destroys UICamera instances; clear our stale refs and close viewer.
            if (_viewerOpen) VrCloseViewer();
            _blockedUiCams = null;
            if (System.Array.IndexOf(GameScenes, Application.loadedLevelName) >= 0)
                StartCoroutine(HookRecordButton());
            // Direct NGUI hook for the main-menu "Recordings" (ShowVideoBtn) button.
            // This bypasses the Kamcord injection chain entirely and is more reliable.
            if (Application.loadedLevelName == "MainMenu")
                StartCoroutine(HookMainMenuRecordingsButton());
            // Scene load destroys cameras, but null-RT ReadPixels needs no camera setup.
        }

        private IEnumerator HookRecordButton()
        {
            // Retry for up to 5 seconds — the HUD spawns asynchronously after scene load.
            int hooked = 0;
            float waited = 0f;
            while (hooked == 0 && waited < 5f)
            {
                yield return new WaitForSeconds(0.25f);
                waited += 0.25f;
                MonoBehaviour[] all = (MonoBehaviour[])(object)Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
                // Log all UIButtonEventKit values once so we can verify the names on device.
                if (waited <= 0.26f)
                {
                    foreach (MonoBehaviour mb in all)
                    {
                        if (mb.GetType().Name != "UIButtonEventKit") continue;
                        FieldInfo fi = mb.GetType().GetField("buttonName",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (fi == null) continue;
                        RecordingModEntry.Log("  btn: " + fi.GetValue(mb) + " go=" + ((Component)mb).gameObject.name);
                    }
                }
                foreach (MonoBehaviour mb in all)
                {
                    if (mb.GetType().Name != "UIButtonEventKit") continue;
                    FieldInfo fi = mb.GetType().GetField("buttonName",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fi == null) continue;
                    // Compare by enum name — robust against enum integer value changes.
                    if (fi.GetValue(mb).ToString() != "RecordBtnInGame") continue;
                    ((Behaviour)mb).enabled = false;
                    RecordBtnClick proxy = ((Component)mb).gameObject.GetComponent<RecordBtnClick>()
                        ?? ((Component)mb).gameObject.AddComponent<RecordBtnClick>();
                    proxy.hook = this;
                    hooked++;
                    RecordingModEntry.Log("HookRecordButton: hooked " + ((Component)mb).gameObject.name);
                }
            }
            if (hooked == 0)
                RecordingModEntry.Log("HookRecordButton: no RecordBtnInGame buttons found after " + waited + "s in " + Application.loadedLevelName);
            _btnHooked = hooked > 0;
        }

        // Hook the main-menu "Recordings" button (UIButtonEventKit.buttonName == ShowVideoBtn)
        // by disabling the original UIButtonEventKit and adding a RecordingsBtnClick proxy.
        // This is a direct NGUI hook that doesn't rely on the Kamcord injection chain.
        private IEnumerator HookMainMenuRecordingsButton()
        {
            float waited = 0f;
            while (waited < 5f)
            {
                yield return new WaitForSeconds(0.25f);
                waited += 0.25f;
                MonoBehaviour[] all = (MonoBehaviour[])(object)Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
                foreach (MonoBehaviour mb in all)
                {
                    if (mb.GetType().Name != "UIButtonEventKit") continue;
                    FieldInfo fi = mb.GetType().GetField("buttonName",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fi == null) continue;
                    if (fi.GetValue(mb).ToString() != "ShowVideoBtn") continue;
                    ((Behaviour)mb).enabled = false;
                    RecordingsBtnClick proxy = ((Component)mb).gameObject.GetComponent<RecordingsBtnClick>()
                        ?? ((Component)mb).gameObject.AddComponent<RecordingsBtnClick>();
                    proxy.hook = this;
                    RecordingModEntry.Log("HookMainMenuRecordingsButton: hooked " + ((Component)mb).gameObject.name);
                    yield break;
                }
            }
            RecordingModEntry.Log("HookMainMenuRecordingsButton: ShowVideoBtn not found after " + waited + "s");
        }

        private void OnDestroy()
        {
            if (IsCapturing) StopCapture();
            if (_postRenderCapture != null) { Destroy(_postRenderCapture); _postRenderCapture = null; }
            if (_captureRT     != null) { Destroy(_captureRT);     _captureRT     = null; }
            if (_readTex       != null) { Destroy(_readTex);       _readTex       = null; }
            if (_screenshotTex != null) { Destroy(_screenshotTex); _screenshotTex = null; }
        }

        // -----------------------------------------------------------------------
        //  Phase 1: capture
        // -----------------------------------------------------------------------
        public void StartCapture()
        {
            if (IsCapturing || IsEncoding) { RecordingModEntry.Log("StartCapture: busy"); return; }
            RecordingModEntry.Log("StartCapture BEGIN");

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _sessionDir     = Path.Combine(RecordingsDir, "raw_" + timestamp);
            _capturedFrames = 0;
            _encodingFrame  = false;
            _encodeOutputPath = Path.Combine(RecordingsDir, timestamp + ".webm");

            try { Directory.CreateDirectory(_sessionDir); }
            catch (Exception ex) { RecordingModEntry.Log("StartCapture: mkdir failed: " + ex.Message); return; }

            int scrW = Screen.width, scrH = Screen.height;
            _scrW = scrW; _scrH = scrH;

            int nv12Size = CaptureStride * CaptureSliceH * 3 / 2;
            if (_nv12Buf == null || _nv12Buf.Length != nv12Size)
                _nv12Buf = new byte[nv12Size];

            // Disable Kamcord cameras (harmless; prevents any EGL redirect side-effects).
            _kamcordPreCam = null; _kamcordPostCam = null;
            foreach (Camera c in Camera.allCameras)
            {
                if (c.name == "kamcordPreCamera")  _kamcordPreCam  = c;
                if (c.name == "kamcordPostCamera") _kamcordPostCam = c;
            }
            if (_kamcordPreCam  != null) { _kamcordPreCam.enabled  = false; RecordingModEntry.Log("  kamcordPreCamera disabled"); }
            if (_kamcordPostCam != null) { _kamcordPostCam.enabled = false; RecordingModEntry.Log("  kamcordPostCamera disabled"); }

            IsCapturing = true;
            _convertedFrames = 0;

            // Identify the primary scene camera for diagnostic logging only.
            _captureCamera = null;
            foreach (Camera c in Camera.allCameras)
            {
                if (c.clearFlags != CameraClearFlags.Skybox && c.clearFlags != CameraClearFlags.SolidColor) continue;
                if (_captureCamera == null || c.depth > _captureCamera.depth) _captureCamera = c;
            }
            if (_captureCamera == null)
                foreach (Camera c in Camera.allCameras)
                    if (_captureCamera == null || c.depth > _captureCamera.depth) _captureCamera = c;
            RecordingModEntry.Log("  captureCamera -> " + (_captureCamera != null ? _captureCamera.name + " d=" + _captureCamera.depth : "NONE"));

            // Start the coroutine that reads CaptureScreenshot PNGs and converts to NV12.
            StartCoroutine(ConverterCoroutine());

            RecordingModEntry.Log("StartCapture: session=" + timestamp
                + " scrn=" + scrW + "x" + scrH
                + " enc=" + VideoWidth + "x" + VideoHeight + "@" + VideoFps
                + " captureStride=" + CaptureStride + " captureSliceH=" + CaptureSliceH
                + " mode=captureScreenshot");
        }

        public void StopCapture()
        {
            if (!IsCapturing) { RecordingModEntry.Log("StopCapture: not capturing"); return; }
            IsCapturing = false;
            _captureCamera = null;
            // Re-enable Kamcord cameras now that capture is done.
            if (_kamcordPreCam  != null) { _kamcordPreCam.enabled  = true; _kamcordPreCam  = null; }
            if (_kamcordPostCam != null) { _kamcordPostCam.enabled = true; _kamcordPostCam = null; }
            if (_postRenderCapture != null) { Destroy(_postRenderCapture); _postRenderCapture = null; }
            if (_captureRT != null) { Destroy(_captureRT); _captureRT = null; }
            RecordingModEntry.Log("StopCapture: " + _capturedFrames + " frames captured -> waiting for PNG conversion");

            // ConverterCoroutine is still converting screenshot PNGs to NV12.
            // FinishAndEncode waits for it to complete before starting the encode thread.
            StartCoroutine(FinishAndEncode());
        }

        // Attach CaptureDisplay to whichever real game camera has the highest depth.
        // (Kept as dead code — null-RT ReadPixels is the active capture path.)

        public void OpenViewer()
        {
            RefreshRecordings();
            _viewerPage   = 0;
            _selectedPath = null;
            _viewerOpen   = true;
            // Disable NGUI's UICamera (input) so touches don't fall through to the game UI.
            try
            {
                _blockedUiCams = (UICamera[])FindObjectsOfType(typeof(UICamera));
                foreach (var c in _blockedUiCams) c.enabled = false;
            }
            catch { _blockedUiCams = null; }
        }

        private void VrCloseViewer()
        {
            VrStopVideo();
            _viewerOpen   = false;
            _selectedPath = null;
            if (_blockedUiCams != null)
            {
                foreach (var c in _blockedUiCams) if (c != null) c.enabled = true;
                _blockedUiCams = null;
            }
        }

        private void Update()
        {
            // Poll VideoView state for seekbar / play-pause button
            if (_videoView != null)
            {
                try
                {
                    int dur = _videoView.Call<int>("getDuration");
                    if (dur > 0) _mpDurMs = dur;
                    _mpCurMs   = _videoView.Call<int>("getCurrentPosition");
                    _mpPlaying = _videoView.Call<bool>("isPlaying");
                }
                catch { }
            }
            if (!IsCapturing || _encodingFrame) return;
            _encodingFrame = true;
            StartCoroutine(CaptureFrameCoroutine());
        }

        private IEnumerator CaptureFrameCoroutine()
        {
            yield return new WaitForEndOfFrame();
            if (!IsCapturing) { _encodingFrame = false; yield break; }

            bool verbose = (_capturedFrames < 5) || (_capturedFrames % 60 == 0);
            if (verbose) RecordingModEntry.Log("CaptureFrame " + _capturedFrames
                + " (screen=" + Screen.width + "x" + Screen.height + ")");

            try
            {
                // First-frame diagnostics: log all cameras.
                if (_capturedFrames == 0)
                {
                    Camera[] allCams = Camera.allCameras;
                    RecordingModEntry.Log("  [diag] " + allCams.Length + " cameras:");
                    foreach (Camera c in allCams)
                        RecordingModEntry.Log("    " + c.name + " d=" + c.depth
                            + " cf=" + c.clearFlags
                            + " bg=(" + (int)(c.backgroundColor.r * 255)
                            + "," + (int)(c.backgroundColor.g * 255)
                            + "," + (int)(c.backgroundColor.b * 255) + ")"
                            + " mask=" + c.cullingMask
                            + " on=" + c.gameObject.activeInHierarchy);
                }

                // Application.CaptureScreenshot is the only API that captures actual content
                // on WSA (Android via ANGLE/DirectX). ReadPixels and OnRenderImage see grey
                // because ANGLE keeps frames in a DirectX texture, not an accessible OpenGL FBO.
                // CaptureScreenshot goes through Unity's internal path and grabs the ANGLE swap
                // chain surface. ConverterCoroutine monitors the output file and saves NV12.
                string ssName = "cnr_ss_" + _capturedFrames.ToString("D5") + ".png";
                Application.CaptureScreenshot(ssName);
                if (verbose) RecordingModEntry.Log("  CaptureScreenshot queued: " + ssName);
                _capturedFrames++;
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("CaptureFrame[" + _capturedFrames + "] error: " + ex.Message);
            }
            _encodingFrame = false;
        }

        // -----------------------------------------------------------------------
        //  Phase 1b: PNG -> NV12 converter (coroutine, concurrent with capture)
        // -----------------------------------------------------------------------
        private IEnumerator ConverterCoroutine()
        {
            int waitFrames = 0;
            while (IsCapturing || _convertedFrames < _capturedFrames)
            {
                if (_convertedFrames >= _capturedFrames)
                {
                    yield return null;
                    continue;
                }
                string ssPath = Application.persistentDataPath
                    + "/cnr_ss_" + _convertedFrames.ToString("D5") + ".png";
                if (!File.Exists(ssPath))
                {
                    waitFrames++;
                    if (waitFrames > 600) // ~20s at 30fps — give up on this frame
                    {
                        RecordingModEntry.Log("ConverterCoroutine: timeout waiting for frame " + _convertedFrames + ", skipping");
                        _convertedFrames++;
                        waitFrames = 0;
                    }
                    yield return null;
                    continue;
                }
                waitFrames = 0;
                bool verbose = (_convertedFrames < 5) || (_convertedFrames % 60 == 0);
                try
                {
                    byte[] pngBytes = File.ReadAllBytes(ssPath);
                    File.Delete(ssPath);
                    if (_screenshotTex == null)
                        _screenshotTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    _screenshotTex.LoadImage(pngBytes);
                    int texW = _screenshotTex.width;
                    int texH = _screenshotTex.height;
                    Color32[] px = _screenshotTex.GetPixels32();
                    Color32 pC = px[(texH / 2) * texW + texW / 2];
                    if (verbose) RecordingModEntry.Log("  [conv " + _convertedFrames + "] "
                        + texW + "x" + texH + " center=(" + pC.r + "," + pC.g + "," + pC.b + ")");

                    // Convert RGBA -> NV12 into _nv12Buf (tightly packed, stride=VideoWidth).
                    for (int i = 0; i < _nv12Buf.Length; i++) _nv12Buf[i] = 128;
                    int yBase  = 0;
                    int uvBase = CaptureStride * CaptureSliceH;
                    for (int row = 0; row < VideoHeight; row++)
                    {
                        int srcRow = ((VideoHeight - 1 - row) * texH) / VideoHeight;
                        for (int col = 0; col < VideoWidth; col++)
                        {
                            int srcCol = (col * texW) / VideoWidth;
                            Color32 c  = px[srcRow * texW + srcCol];
                            int R = c.r, G = c.g, B = c.b;
                            int Y  = ((66 * R + 129 * G +  25 * B + 128) >> 8) + 16;
                            _nv12Buf[yBase + row * CaptureStride + col] = (byte)(Y < 0 ? 0 : Y > 255 ? 255 : Y);
                            if ((row & 1) == 0 && (col & 1) == 0)
                            {
                                int U = ((-38 * R -  74 * G + 112 * B + 128) >> 8) + 128;
                                int V = ((112 * R -  94 * G -  18 * B + 128) >> 8) + 128;
                                int off = uvBase + (row / 2) * CaptureStride + col;
                                _nv12Buf[off]     = (byte)(U < 0 ? 0 : U > 255 ? 255 : U);
                                _nv12Buf[off + 1] = (byte)(V < 0 ? 0 : V > 255 ? 255 : V);
                            }
                        }
                    }
                    string framePath = Path.Combine(_sessionDir, "frame_" + _convertedFrames.ToString("D5") + ".nv12");
                    File.WriteAllBytes(framePath, _nv12Buf);
                    if (verbose) RecordingModEntry.Log("  [conv " + _convertedFrames + "] wrote " + framePath);
                    _convertedFrames++;
                }
                catch (Exception ex)
                {
                    RecordingModEntry.Log("ConverterCoroutine[" + _convertedFrames + "] error: " + ex.Message);
                    _convertedFrames++; // skip on error
                }
                yield return null; // yield after each frame to avoid stalling the main thread
            }
            RecordingModEntry.Log("ConverterCoroutine done: converted " + _convertedFrames + " frames");
        }

        private IEnumerator FinishAndEncode()
        {
            RecordingModEntry.Log("FinishAndEncode: waiting for " + _capturedFrames + " PNG conversions...");
            float timeout = 120f; // max 2 minutes
            float elapsed = 0f;
            while (_convertedFrames < _capturedFrames && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            if (_convertedFrames < _capturedFrames)
                RecordingModEntry.Log("FinishAndEncode: timeout after " + elapsed + "s, "
                    + _convertedFrames + "/" + _capturedFrames + " converted");
            else
                RecordingModEntry.Log("FinishAndEncode: all " + _convertedFrames + " frames converted -> encode");
            // Set _capturedFrames to actual converted count so EncodeThread knows the real total.
            _capturedFrames = _convertedFrames;
            IsEncoding = true;
            var t = new Thread(EncodeThread);
            t.IsBackground = true;
            t.Start();
        }

        // -----------------------------------------------------------------------
        //  Phase 2: encode (background thread, runs after capture stops)
        // -----------------------------------------------------------------------
        private void EncodeThread()
        {
            RecordingModEntry.Log("EncodeThread START: " + _capturedFrames + " frames -> " + _encodeOutputPath);
            try
            {
                AndroidJNI.AttachCurrentThread();

                var fmtClass = new AndroidJavaClass("android.media.MediaFormat");
                var mediaFmt = fmtClass.CallStatic<AndroidJavaObject>("createVideoFormat",
                    "video/x-vnd.on2.vp8", VideoWidth, VideoHeight);
                mediaFmt.Call("setInteger", "bitrate",          VideoBitrate);
                mediaFmt.Call("setInteger", "frame-rate",       VideoFps);
                mediaFmt.Call("setInteger", "i-frame-interval", 1);
                // VP8 does not produce a CODEC_CONFIG output buffer (no SPS/PPS concept),
                // so the H.264-specific deadlock (releasing CODEC_CONFIG kills the encoder
                // on WSA Android 13 x86_64) does not apply.

                // Try named VP8 encoders first, fall back to system default.
                AndroidJavaObject codec = null;
                string chosenCodecName = null;
                var mcClass = new AndroidJavaClass("android.media.MediaCodec");
                foreach (string tryName in new string[] { "OMX.google.vp8.encoder", "c2.android.vp8.encoder", null })
                {
                    try
                    {
                        codec = tryName != null
                            ? mcClass.CallStatic<AndroidJavaObject>("createByCodecName", tryName)
                            : mcClass.CallStatic<AndroidJavaObject>("createEncoderByType", "video/x-vnd.on2.vp8");
                        chosenCodecName = tryName ?? "default VP8";
                        RecordingModEntry.Log("  using codec: " + chosenCodecName);
                        break;
                    }
                    catch (Exception ex)
                    {
                        RecordingModEntry.Log("  codec " + (tryName ?? "default VP8") + " unavailable: " + ex.Message.Split('\n')[0]);
                        codec = null;
                    }
                }
                if (codec == null) { RecordingModEntry.Log("EncodeThread: no VP8 encoder found"); return; }

                // Query supported color formats so we know what to request in configure().
                int colorFmtToUse = -1;
                try
                {
                    var mcList = new AndroidJavaObject("android.media.MediaCodecList", 0);
                    var infos  = mcList.Call<AndroidJavaObject[]>("getCodecInfos");
                    foreach (var info in infos)
                    {
                        if (!info.Call<bool>("isEncoder")) continue;
                        string nm = info.Call<string>("getName");
                        if (!nm.ToLower().Contains("vp8")) continue;
                        var caps = info.Call<AndroidJavaObject>("getCapabilitiesForType", "video/x-vnd.on2.vp8");
                        int[] fmts = caps.Get<int[]>("colorFormats");
                        string s = ""; foreach (int fmt2 in fmts) s += fmt2 + " ";
                        RecordingModEntry.Log("  " + nm + " colorFormats: " + s.Trim());
                        // Prefer 19 (I420) then 21 (NV12) then first available
                        foreach (int fmt2 in fmts) { if (fmt2 == 19) { colorFmtToUse = 19; break; } }
                        if (colorFmtToUse < 0) foreach (int fmt2 in fmts) { if (fmt2 == 21) { colorFmtToUse = 21; break; } }
                        if (colorFmtToUse < 0 && fmts.Length > 0) colorFmtToUse = fmts[0];
                        break;
                    }
                }
                catch (Exception ex) { RecordingModEntry.Log("  colorFormat query: " + ex.Message.Split('\n')[0]); }

                if (colorFmtToUse > 0)
                {
                    mediaFmt.Call("setInteger", "color-format", colorFmtToUse);
                    RecordingModEntry.Log("  configuring with color-format=" + colorFmtToUse);
                }
                else
                {
                    RecordingModEntry.Log("  configuring WITHOUT color-format (let encoder pick)");
                }

                try { codec.Call("configure", mediaFmt, null, null, 1); RecordingModEntry.Log("  configure OK"); }
                catch (Exception ex) { RecordingModEntry.Log("  configure threw: " + ex.Message.Split('\n')[0]); return; }
                try { codec.Call("start"); RecordingModEntry.Log("  codec.start OK"); }
                catch (Exception ex) { RecordingModEntry.Log("  start threw: " + ex.Message.Split('\n')[0]); return; }

                int stride = CaptureStride, sliceH = CaptureSliceH;
                int actualColorFmt = colorFmtToUse; // fallback to what we configured
                try
                {
                    var inFmt = codec.Call<AndroidJavaObject>("getInputFormat");
                    try { stride = inFmt.Call<int>("getInteger", "stride"); } catch { }
                    try { sliceH = inFmt.Call<int>("getInteger", "slice-height"); } catch { }
                    try { actualColorFmt = inFmt.Call<int>("getInteger", "color-format"); } catch { }
                    RecordingModEntry.Log("  stride=" + stride + " sliceH=" + sliceH + " colorFmt=" + actualColorFmt);
                }
                catch (Exception ex) { RecordingModEntry.Log("  getInputFormat: " + ex.Message.Split('\n')[0]); }

                var muxer      = new AndroidJavaObject("android.media.MediaMuxer", _encodeOutputPath, 1); // 1 = MUXER_OUTPUT_WEBM
                var bufferInfo = new AndroidJavaObject("android.media.MediaCodec$BufferInfo");
                bool muxerStarted  = false;
                int  videoTrackIdx = -1;
                AndroidJavaObject pendingFmt = null;
                long ptsUsec       = 0;

                int encBufSize = stride * sliceH * 3 / 2;

                // Pre-allocate a re-stride buffer if encoder stride differs from capture stride
                byte[] strideBuf = (stride != CaptureStride || sliceH != CaptureSliceH)
                    ? new byte[encBufSize] : null;

                for (int f = 0; f < _capturedFrames; f++)
                {
                    string framePath = Path.Combine(_sessionDir,
                        "frame_" + f.ToString("D5") + ".nv12");
                    if (!File.Exists(framePath))
                    {
                        RecordingModEntry.Log("  frame " + f + " missing, skipping");
                        continue;
                    }
                    byte[] nv12 = File.ReadAllBytes(framePath);

                    // Re-stride: expand tightly-packed capture rows to encoder-expected stride
                    byte[] feedBuf;
                    if (strideBuf != null)
                    {
                        // Fill UV plane with neutral (128); Y plane with 0 (black for any uncovered rows)
                        System.Array.Clear(strideBuf, 0, encBufSize);
                        int uvDst = stride * sliceH;
                        for (int i = uvDst; i < encBufSize; i++) strideBuf[i] = 128;
                        // Copy Y rows
                        for (int row = 0; row < VideoHeight; row++)
                        {
                            int src = row * CaptureStride;
                            int dst = row * stride;
                            if (src + VideoWidth <= nv12.Length)
                                Buffer.BlockCopy(nv12, src, strideBuf, dst, VideoWidth);
                        }
                        // Copy UV rows
                        int uvSrc = CaptureStride * CaptureSliceH;
                        for (int row = 0; row < VideoHeight / 2; row++)
                        {
                            int src = uvSrc + row * CaptureStride;
                            int dst = uvDst + row * stride;
                            if (src + VideoWidth <= nv12.Length)
                                Buffer.BlockCopy(nv12, src, strideBuf, dst, VideoWidth);
                        }
                        feedBuf = strideBuf;
                    }
                    else
                    {
                        feedBuf = nv12;
                    }

                    // Convert NV12 (interleaved UV) → I420 (separate U/V planes) for VP8 encoder.
                    // NV12 layout: Y[stride*sliceH] + UV[stride*sliceH/2] (UVUV...)
                    // I420 layout: Y[stride*sliceH] + U[(stride/2)*(sliceH/2)] + V[(stride/2)*(sliceH/2)]
                    if (actualColorFmt == 19)
                    {
                        int yBytes = stride * sliceH;
                        int uvCols = stride / 2;
                        int uvRows = sliceH / 2;
                        int uBytes = uvCols * uvRows;
                        var i420 = new byte[yBytes + uBytes * 2];
                        Buffer.BlockCopy(feedBuf, 0, i420, 0, yBytes);
                        for (int row = 0; row < uvRows; row++)
                        {
                            int nvRow = yBytes + row * stride;
                            int uRow  = yBytes + row * uvCols;
                            int vRow  = yBytes + uBytes + row * uvCols;
                            for (int col = 0; col < uvCols; col++)
                            {
                                i420[uRow + col] = feedBuf[nvRow + col * 2];
                                i420[vRow + col] = feedBuf[nvRow + col * 2 + 1];
                            }
                        }
                        feedBuf = i420;
                    }

                    if (f < 3) RecordingModEntry.Log("  encoding frame " + f
                        + " captureLen=" + nv12.Length + " feedLen=" + feedBuf.Length
                        + " Y[0]=" + feedBuf[0]
                        + " UV=(" + feedBuf[stride * sliceH] + "," + feedBuf[stride * sliceH + 1] + ")");

                    ptsUsec += (long)(1000000L / VideoFps);

                    int inIdx;
                    try { inIdx = codec.Call<int>("dequeueInputBuffer", (long)100000); }
                    catch (AndroidJavaException ex)
                    {
                        if (f < 5) RecordingModEntry.Log("  [e" + f + "] dequeueInputBuffer threw: " + ex.Message.Split('\n')[0]);
                        inIdx = -1;
                    }
                    if (inIdx >= 0)
                    {
                        // getInputBuffer(idx) is the modern API 21+ single-buffer call.
                        // getInputBuffers() is deprecated and may return detached buffers on API 36.
                        var rawBuf   = codec.Call<AndroidJavaObject>("getInputBuffer", inIdx);
                        int bufCap   = rawBuf.Call<int>("capacity");
                        int putLen   = System.Math.Min(bufCap, feedBuf.Length);
                        // Raw JNI with exact signature avoids Unity 4.6's broken sbyte[]->[]
                        // type mapping. GetMethodID searches class + superclasses, finding
                        // ByteBuffer.put(byte[]) on DirectByteBuffer correctly.
                        bool isDirect = rawBuf.Call<bool>("isDirect");
                        if (f < 3) RecordingModEntry.Log("  frame " + f + " bufCap=" + bufCap + " isDirect=" + isDirect);
                        IntPtr rawClass = rawBuf.GetRawClass();
                        IntPtr putMid   = AndroidJNI.GetMethodID(rawClass, "put", "([B)Ljava/nio/ByteBuffer;");
                        byte[] writeBytes;
                        if (putLen == feedBuf.Length) { writeBytes = feedBuf; }
                        else { writeBytes = new byte[putLen]; System.Array.Copy(feedBuf, writeBytes, putLen); }
                        IntPtr jniArr = AndroidJNIHelper.ConvertToJNIArray(writeBytes);
                        var jargs = new jvalue[1]; jargs[0].l = jniArr;
                        IntPtr res = AndroidJNI.CallObjectMethod(rawBuf.GetRawObject(), putMid, jargs);
                        if (res != System.IntPtr.Zero) AndroidJNI.DeleteLocalRef(res);
                        AndroidJNI.DeleteLocalRef(jniArr);
                        if (f < 3) RecordingModEntry.Log("  [e" + f + "] put OK, queueing inIdx=" + inIdx + " putLen=" + putLen + " pts=" + ptsUsec);
                        codec.Call("queueInputBuffer", inIdx, 0, putLen, ptsUsec, 0);
                        if (f < 3) RecordingModEntry.Log("  [e" + f + "] queued OK");
                    }

                    // drain
                    for (int d = 0; d < 10; d++)
                    {
                        int outIdx;
                        try { outIdx = codec.Call<int>("dequeueOutputBuffer", bufferInfo, (long)10000); }
                        catch (AndroidJavaException ex)
                        {
                            // IllegalStateException on SDK 33 after muxer.start() — skip drain
                            if (f < 5) RecordingModEntry.Log("  [drain f" + f + " d" + d + "] dequeue threw: " + ex.Message.Split('\n')[0]);
                            break;
                        }
                        if (f < 3) RecordingModEntry.Log("  [drain f" + f + " d" + d + "] outIdx=" + outIdx);
                        if (outIdx == INFO_TRY_AGAIN_LATER) break;
                        if (outIdx == INFO_OUTPUT_FORMAT_CHANGED)
                        {
                            if (pendingFmt == null)
                            {
                                pendingFmt = codec.Call<AndroidJavaObject>("getOutputFormat");
                                RecordingModEntry.Log("  FORMAT_CHANGED: saved pendingFmt (muxer NOT yet started)");
                                // Do NOT call muxer.start() here.
                                // Theory: muxer.start() corrupts the codec's internal state so that
                                // releaseOutputBuffer(CODEC_CONFIG) permanently kills the encoder.
                                // Instead, defer muxer.start() until the first real (non-config) sample
                                // arrives, by which time CODEC_CONFIG has already been safely released.
                            }
                            continue;
                        }
                        if (outIdx < 0) break;
                        int flags = bufferInfo.Get<int>("flags");
                        int size  = bufferInfo.Get<int>("size");
                        if ((flags & 2) != 0)
                        {
                            // CODEC_CONFIG: release WITHOUT muxer.start() having been called.
                            // This tests whether muxer.start() was causing the permanent error state.
                            if (f < 3) RecordingModEntry.Log("  [drain f" + f + " d" + d + "] CODEC_CONFIG releasing (no muxer.start yet)");
                            try { codec.Call("releaseOutputBuffer", outIdx, false); }
                            catch (Exception ex) { if (f < 3) RecordingModEntry.Log("  CODEC_CONFIG release threw: " + ex.Message.Split('\n')[0]); }
                            if (f < 3) RecordingModEntry.Log("  CODEC_CONFIG released");
                            continue;
                        }
                        // First real sample: start muxer now, before writeSampleData.
                        if (!muxerStarted && pendingFmt != null && size > 0)
                        {
                            videoTrackIdx = muxer.Call<int>("addTrack", pendingFmt);
                            muxer.Call("start");
                            muxerStarted = true;
                            RecordingModEntry.Log("  muxer started (deferred) track=" + videoTrackIdx);
                            System.Threading.Thread.Sleep(200);
                        }
                        if ((flags & 2) == 0 && size > 0 && muxerStarted)
                        {
                            var outBuf = codec.Call<AndroidJavaObject>("getOutputBuffer", outIdx);
                            if (f < 3) RecordingModEntry.Log("  [drain f" + f + " d" + d + "] writeSampleData size=" + size);
                            muxer.Call("writeSampleData", videoTrackIdx, outBuf, bufferInfo);
                            if (f < 3) RecordingModEntry.Log("  [drain f" + f + " d" + d + "] writeSampleData OK");
                        }
                        if (f < 3) RecordingModEntry.Log("  [drain f" + f + " d" + d + "] release flags=" + flags);
                        codec.Call("releaseOutputBuffer", outIdx, false);
                        if (f < 3) RecordingModEntry.Log("  [drain f" + f + " d" + d + "] released OK");
                        if ((flags & 4) != 0) break;
                    }
                }

                // EOS + final drain wrapped in try-catch (codec may be in error state on some paths)
                try
                {
                int eosIdx = codec.Call<int>("dequeueInputBuffer", (long)100000);
                if (eosIdx >= 0) codec.Call("queueInputBuffer", eosIdx, 0, 0, ptsUsec, 4);
                } catch { }

                // final drain
                for (int d = 0; d < 30; d++)
                {
                    int outIdx;
                    try { outIdx = codec.Call<int>("dequeueOutputBuffer", bufferInfo, (long)50000); }
                    catch { break; }
                    if (outIdx == INFO_TRY_AGAIN_LATER) break;
                    if (outIdx == INFO_OUTPUT_FORMAT_CHANGED) { continue; }
                    if (outIdx < 0) break;
                    int flags = bufferInfo.Get<int>("flags");
                    int size  = bufferInfo.Get<int>("size");
                    if ((flags & 2) == 0 && size > 0 && muxerStarted)
                    {
                        var outBuf = codec.Call<AndroidJavaObject>("getOutputBuffer", outIdx);
                        muxer.Call("writeSampleData", videoTrackIdx, outBuf, bufferInfo);
                    }
                    codec.Call("releaseOutputBuffer", outIdx, false);
                    if ((flags & 4) != 0) { RecordingModEntry.Log("  final drain: EOS flag seen"); break; }
                }

                if (muxerStarted) muxer.Call("stop");
                muxer.Call("release");
                codec.Call("stop");
                codec.Call("release");
                bufferInfo.Dispose();

                long sz = 0;
                try { sz = new FileInfo(_encodeOutputPath).Length; } catch { }
                RecordingModEntry.Log("EncodeThread DONE: " + sz + " bytes -> " + _encodeOutputPath);

                // Clean up raw frames
                try { Directory.Delete(_sessionDir, true); }
                catch (Exception ex) { RecordingModEntry.Log("  cleanup error: " + ex.Message); }
            }
            catch (Exception ex)
            {
                _encodeError = ex.Message;
                RecordingModEntry.Log("EncodeThread EXCEPTION: " + ex);
            }
            finally
            {
                try { AndroidJNI.DetachCurrentThread(); } catch { }
                IsEncoding = false;
                _sessionDir = null;
            }
        }

        // -----------------------------------------------------------------------
        //  Viewer
        // -----------------------------------------------------------------------
        private void RefreshRecordings()
        {
            _recordings.Clear();
            _recBytes.Clear();
            _recDuration.Clear();
            _statusMsg = null;
            try
            {
                if (!Directory.Exists(RecordingsDir)) return;
                var fileList = new System.Collections.Generic.List<string>();
                fileList.AddRange(Directory.GetFiles(RecordingsDir, "*.webm"));
                fileList.AddRange(Directory.GetFiles(RecordingsDir, "*.mp4"));
                string[] files = fileList.ToArray();
                System.Array.Sort(files, (a, b) => string.Compare(b, a, StringComparison.Ordinal));
                foreach (string f in files)
                {
                    _recordings.Add(f);
                    long sz = 0;
                    try { sz = new FileInfo(f).Length; } catch { }
                    _recBytes.Add(sz);
                    float dur = -1f;
                    string rawDir = Path.Combine(RecordingsDir,
                        "raw_" + Path.GetFileNameWithoutExtension(f));
                    if (Directory.Exists(rawDir))
                        try
                        {
                            int n = Directory.GetFiles(rawDir, "*.nv12").Length;
                            if (n > 0) dur = n / (float)VideoFps;
                        }
                        catch { }
                    if (dur < 0f && sz > 0)
                        dur = sz * 8f / (float)VideoBitrate;
                    _recDuration.Add(dur);
                }
            }
            catch (Exception ex) { _statusMsg = "Error: " + ex.Message; }
        }

        private void OnGUI()
        {
            // Render this MonoBehaviour's OnGUI last (on top of all others, including CNRMod's
            // EcoHook which draws the mod buttons). Lower GUI.depth = drawn later = on top.
            GUI.depth = -100;

            float vw = Screen.width;
            float vh = Screen.height;

            // REC / Encoding indicator (always visible, top-right)
            if (IsCapturing || IsEncoding)
            {
                VrEnsureStyles();
                string badge = IsEncoding ? "[Encoding]" : "\u25cf REC";
                GUI.Label(new Rect(vw - 112f, 6f, 106f, 32f), badge, _gsVrStatus ?? GUI.skin.label);
            }

            if (!_viewerOpen) return;

            // Dim overlay (visual only — no GUI.Button here; that would eat all sub-button clicks).
            // NGUI click-through is blocked by disabling UICamera instances in OpenViewer().
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, vw, vh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            VrEnsureStyles();

            float pw = vw * 0.94f;
            float ph = vh * 0.90f;
            float px = Mathf.Round((vw - pw) * 0.5f);
            float py = Mathf.Round((vh - ph) * 0.5f);

            // Panel background
            if (_vrPanelBg != null)
                GUI.DrawTexture(new Rect(px, py, pw, ph), _vrPanelBg, ScaleMode.StretchToFill);
            else
            {
                GUI.color = new Color(0.10f, 0.10f, 0.12f, 0.97f);
                GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            if (_selectedPath == null)
                DrawListView(px, py, pw, ph);
            else
                DrawDetailView(px, py, pw, ph);

            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                Event.current.Use();
        }

        // ---- List view -------------------------------------------------------
        private void DrawListView(float px, float py, float pw, float ph)
        {
            const float hTop  = 44f;
            const float hBot  = 46f;
            const float hItem = 58f;
            const float padX  = 10f;

            // How many items fit in the available vertical space between header and footer.
            int pageSize = Mathf.Max(1, (int)((ph - hTop - 6f - hBot) / (hItem + 1f)));

            // Title bar
            GUI.Label(new Rect(px + padX, py + 8f, pw - 60f, 30f),
                "  [CNR]  Recordings", _gsVrTitle);
            string encBadge = IsEncoding ? "Encoding..." : (_encodeError != null ? "Err!" : null);
            if (encBadge != null)
                GUI.Label(new Rect(px + pw - 138f, py + 12f, 88f, 22f), encBadge, _gsVrStatus);
            if (GUI.Button(new Rect(px + pw - 46f, py + 9f, 36f, 28f), "X", _gsVrBtn))
            { VrCloseViewer(); return; }

            // Separator under title bar
            float ty = py + hTop;
            GUI.color = new Color(0.35f, 0.35f, 0.45f, 1f);
            GUI.DrawTexture(new Rect(px + padX, ty, pw - padX * 2f, 1f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            ty += 5f;

            // Recordings list
            int total = _recordings.Count;
            int pages = total > 0 ? (total + pageSize - 1) / pageSize : 1;
            if (_viewerPage >= pages) _viewerPage = Mathf.Max(0, pages - 1);
            int start = _viewerPage * pageSize;
            int end   = Mathf.Min(start + pageSize, total);

            if (total == 0)
            {
                GUI.Label(new Rect(px + padX, py + ph * 0.42f, pw - padX * 2f, 30f),
                    _statusMsg ?? "No recordings found.", _gsVrDetailCenter);
            }
            else
            {
                for (int i = start; i < end; i++)
                {
                    Rect ir = new Rect(px + padX, ty, pw - padX * 2f, hItem);

                    // Subtle item background tint
                    GUI.color = new Color(1f, 1f, 1f, 0.04f);
                    GUI.DrawTexture(ir, Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    // Parse metadata
                    string name = Path.GetFileNameWithoutExtension(_recordings[i]);
                    long   sz   = _recBytes[i];
                    float  dur  = _recDuration[i];
                    string tStr = "??:??:??", dStr = "?? ??? ????";
                    if (name.Length >= 15)
                        try
                        {
                            var dt = DateTime.ParseExact(name, "yyyyMMdd_HHmmss", null);
                            tStr = dt.ToString("HH:mm:ss");
                            dStr = dt.ToString("MMM dd  yyyy");
                        }
                        catch { }

                    // TOP-LEFT: play icon + time (game font, white, 20 pt)
                    GUI.Label(new Rect(ir.x + 8f, ir.y + 3f, ir.width * 0.62f, 28f),
                        "\u25b6  " + tStr, _gsVrTimeLabel);
                    // TOP-RIGHT: date (game font, grey, 13 pt, right-aligned)
                    GUI.Label(new Rect(ir.x, ir.y + 7f, ir.width - 8f, 22f),
                        dStr, _gsVrDateLabel);
                    // BOTTOM-LEFT: duration (system font, light grey, 11 pt)
                    GUI.Label(new Rect(ir.x + 8f, ir.yMax - 22f, ir.width * 0.5f, 20f),
                        dur >= 0f ? VrFmtDur(dur) : "? s", _gsVrDetailLabel);
                    // BOTTOM-RIGHT: file size (system font, light grey, 11 pt, right-aligned)
                    GUI.Label(new Rect(ir.x, ir.yMax - 22f, ir.width - 8f, 20f),
                        sz > 0 ? VrFmtBytes(sz) : "? B", _gsVrDetailRight);

                    // Invisible click zone covering the whole item
                    if (GUI.Button(ir, GUIContent.none, _gsVrGhost))
                        _selectedPath = _recordings[i];

                    // Separator between items
                    if (i < end - 1)
                    {
                        GUI.color = new Color(0.25f, 0.25f, 0.35f, 1f);
                        GUI.DrawTexture(new Rect(ir.x + 8f, ir.yMax, ir.width - 16f, 1f),
                            Texture2D.whiteTexture);
                        GUI.color = Color.white;
                    }
                    ty += hItem + 1f;
                }
            }

            // Bottom bar
            float by = py + ph - hBot + 4f;
            GUI.color = new Color(0.35f, 0.35f, 0.45f, 1f);
            GUI.DrawTexture(new Rect(px + padX, by - 6f, pw - padX * 2f, 1f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(px + padX, by, 72f, 32f), "Refresh", _gsVrBtn))
            { RefreshRecordings(); _viewerPage = 0; }

            // Page controls — centred
            float cx   = px + pw * 0.5f;
            float bW   = 32f;
            float lblW = 54f;
            GUI.enabled = _viewerPage > 0;
            if (GUI.Button(new Rect(cx - lblW * 0.5f - bW - 4f, by, bW, 32f), "<", _gsVrBtn))
                _viewerPage--;
            GUI.enabled = true;
            string pStr = total > 0 ? (_viewerPage + 1) + " / " + pages : "\u2013";
            GUI.Label(new Rect(cx - lblW * 0.5f, by + 4f, lblW, 24f), pStr, _gsVrDetailCenter);
            GUI.enabled = _viewerPage < pages - 1;
            if (GUI.Button(new Rect(cx + lblW * 0.5f + 4f, by, bW, 32f), ">", _gsVrBtn))
                _viewerPage++;
            GUI.enabled = true;
        }

        // ---- Detail / player view --------------------------------------------
        private void DrawDetailView(float px, float py, float pw, float ph)
        {
            const float padX = 12f;
            const float hTop = 44f;
            const float hBot = 90f;

            // Start video when a new recording is selected
            if (_selectedPath != _loadedVideoPath)
                VrStartVideo(_selectedPath);

            // ← Back
            if (GUI.Button(new Rect(px + padX, py + 8f, 80f, 28f), "\u2190  Back", _gsVrBtn))
            { VrStopVideo(); _selectedPath = null; return; }

            // Title: time + date from filename
            string name = Path.GetFileNameWithoutExtension(_selectedPath ?? "");
            string tStr = name, dStr = "";
            if (name.Length >= 15)
                try
                {
                    var dt = DateTime.ParseExact(name, "yyyyMMdd_HHmmss", null);
                    tStr = dt.ToString("HH:mm:ss");
                    dStr = "  " + dt.ToString("MMM dd  yyyy");
                }
                catch { }
            GUI.Label(new Rect(px + padX + 88f, py + 10f, pw - padX * 2f - 134f, 26f),
                tStr + dStr, _gsVrDateLabel);

            // X close
            if (GUI.Button(new Rect(px + pw - 46f, py + 8f, 36f, 28f), "X", _gsVrBtn))
            { VrCloseViewer(); return; }

            // Header separator
            GUI.color = new Color(0.35f, 0.35f, 0.45f, 1f);
            GUI.DrawTexture(new Rect(px + padX, py + hTop, pw - padX * 2f, 1f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Black background for the video area (VideoView overlay renders on top)
            float vidY = py + hTop + 1f;
            float vidH = ph - hTop - hBot - 1f;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(px, vidY, pw, vidH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // ---- Bottom controls bar ----
            float by = py + ph - hBot;
            GUI.color = new Color(0.35f, 0.35f, 0.45f, 1f);
            GUI.DrawTexture(new Rect(px + padX, by, pw - padX * 2f, 1f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            by += 6f;

            // Play / Pause
            if (GUI.Button(new Rect(px + padX, by + 6f, 52f, 38f),
                _mpPlaying ? "| |" : " > ", _gsVrBtn))
            {
                var vv = _videoView;
                if (vv != null)
                {
                    bool wasPlaying = _mpPlaying;
                    VrMpOnUi(() => { if (wasPlaying) vv.Call("pause"); else vv.Call("start"); });
                    _mpPlaying = !_mpPlaying;
                }
            }

            // Time label
            float durSec = _mpDurMs > 0 ? _mpDurMs / 1000f : 0f;
            float curSec = _mpCurMs / 1000f;
            string timeStr = VrFmtDur(curSec) + " / " + VrFmtDur(durSec);
            float timeLblW = 110f;
            GUI.Label(new Rect(px + pw - padX - timeLblW, by + 14f, timeLblW, 22f),
                timeStr, _gsVrDetailRight);

            // Seekbar
            float sbX = px + padX + 62f;
            float sbW = pw - padX * 2f - 62f - timeLblW - 8f;
            float newSec = GUI.HorizontalSlider(
                new Rect(sbX, by + 16f, sbW, 22f), curSec, 0f, Mathf.Max(1f, durSec));
            if (Mathf.Abs(newSec - curSec) > 0.3f)
            {
                int ms = (int)(newSec * 1000f);
                _mpCurMs = ms;
                var vv = _videoView;
                if (vv != null) VrMpOnUi(() => vv.Call("seekTo", ms));
            }

            // Status / error
            if (_statusMsg != null)
                GUI.Label(new Rect(px + padX, by + 52f, pw - padX * 2f, 22f),
                    _statusMsg, _gsVrStatus);
        }

        // ---- In-app video player (Android VideoView overlay) -----------------
        private void VrMpOnUi(AndroidJavaRunnable action)
        {
            try
            {
                using (var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var ac = up.GetStatic<AndroidJavaObject>("currentActivity"))
                    ac.Call("runOnUiThread", action);
            }
            catch { }
        }

        private void VrStartVideo(string path)
        {
            VrStopVideo();
            _loadedVideoPath = path;
            _statusMsg = null;
            _mpDurMs = 0; _mpCurMs = 0; _mpPlaying = false;

            float vw = Screen.width, vh = Screen.height;
            float pw = vw * 0.94f, ph = vh * 0.90f;
            float px = Mathf.Round((vw - pw) * 0.5f);
            float py = Mathf.Round((vh - ph) * 0.5f);
            int vidX = (int)px,    vidY = (int)(py + 44f + 1f);
            int vidW = (int)pw,    vidH = (int)(ph - 44f - 90f - 1f);

            string pathCopy = path;
            var mod = this;
            VrMpOnUi(() =>
            {
                try
                {
                    using (var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var ac = up.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        var vv = new AndroidJavaObject("android.widget.VideoView", ac);
                        var lp = new AndroidJavaObject("android.view.ViewGroup$LayoutParams", vidW, vidH);
                        vv.Call("setX", (float)vidX);
                        vv.Call("setY", (float)vidY);
                        vv.Call("setVideoPath", pathCopy);
                        ac.Call("addContentView", vv, lp);
                        vv.Call("start");
                        mod._videoView = vv;
                        mod._mpPlaying = true;
                    }
                }
                catch (Exception ex)
                {
                    mod._statusMsg = "Video error: " + ex.Message;
                    RecordingModEntry.Log("VrStartVideo: " + ex.Message);
                }
            });
        }

        private void VrStopVideo()
        {
            var vv = _videoView;
            _videoView       = null;
            _loadedVideoPath = null;
            _mpPlaying       = false;
            if (vv == null) return;
            VrMpOnUi(() =>
            {
                try
                {
                    vv.Call("stopPlayback");
                    var parent = vv.Call<AndroidJavaObject>("getParent");
                    if (parent != null) parent.Call("removeView", vv);
                    vv.Dispose();
                }
                catch { }
            });
        }

        // ---- Style initialisation --------------------------------------------
        // Called on first OnGUI; borrows textures + font from CNRSettingsMod when
        // available so the viewer shares the exact same visual assets.
        private void VrEnsureStyles()
        {
            if (_vrStylesOk) return;
            _vrStylesOk = true;

            _vrPanelBg   = VrBorrowTex("_spPanelBack")
                        ?? VrMakeTex(new Color(0.10f, 0.10f, 0.12f, 0.97f));
            _vrBtnTex    = VrBorrowTex("_spButtonNull");
            _vrHoverTex  = VrMakeTex(new Color(1f, 1f, 1f, 0.09f));
            _vrActiveTex = VrMakeTex(new Color(1f, 1f, 1f, 0.18f));

            if (_vrFont == null) _vrFont = VrFindFont();

            // REC / error badge
            _gsVrStatus = new GUIStyle(GUI.skin.label);
            _gsVrStatus.fontSize = 13;
            _gsVrStatus.fontStyle = FontStyle.Bold;
            _gsVrStatus.normal.textColor = new Color(1f, 0.30f, 0.30f);
            _gsVrStatus.alignment = TextAnchor.MiddleRight;
            if (_vrFont != null) _gsVrStatus.font = _vrFont;

            // Window title
            _gsVrTitle = new GUIStyle(GUI.skin.label);
            _gsVrTitle.fontSize = 18;
            _gsVrTitle.fontStyle = FontStyle.Bold;
            _gsVrTitle.normal.textColor = new Color(1f, 0.85f, 0.28f);
            _gsVrTitle.alignment = TextAnchor.MiddleLeft;
            if (_vrFont != null) _gsVrTitle.font = _vrFont;

            // Item time (game font, white, 20 pt, left)
            _gsVrTimeLabel = new GUIStyle(GUI.skin.label);
            _gsVrTimeLabel.fontSize = 20;
            _gsVrTimeLabel.fontStyle = FontStyle.Bold;
            _gsVrTimeLabel.normal.textColor = Color.white;
            _gsVrTimeLabel.alignment = TextAnchor.MiddleLeft;
            if (_vrFont != null) _gsVrTimeLabel.font = _vrFont;

            // Detail view big time (game font, gold, 26 pt)
            _gsVrTimeBig = new GUIStyle(GUI.skin.label);
            _gsVrTimeBig.fontSize = 26;
            _gsVrTimeBig.fontStyle = FontStyle.Bold;
            _gsVrTimeBig.normal.textColor = new Color(1f, 0.85f, 0.25f);
            _gsVrTimeBig.alignment = TextAnchor.MiddleLeft;
            if (_vrFont != null) _gsVrTimeBig.font = _vrFont;

            // Date (game font, grey, 13 pt, right-aligned in list / left in detail)
            _gsVrDateLabel = new GUIStyle(GUI.skin.label);
            _gsVrDateLabel.fontSize = 13;
            _gsVrDateLabel.normal.textColor = new Color(0.55f, 0.55f, 0.65f);
            _gsVrDateLabel.alignment = TextAnchor.MiddleRight;
            if (_vrFont != null) _gsVrDateLabel.font = _vrFont;

            // Small detail text (system/default font, 11 pt light grey, left)
            _gsVrDetailLabel = new GUIStyle(GUI.skin.label);
            _gsVrDetailLabel.fontSize = 11;
            _gsVrDetailLabel.normal.textColor = new Color(0.62f, 0.62f, 0.70f);
            _gsVrDetailLabel.alignment = TextAnchor.MiddleLeft;
            // font deliberately not set → Unity default sans-serif for small text

            _gsVrDetailRight = new GUIStyle(_gsVrDetailLabel);
            _gsVrDetailRight.alignment = TextAnchor.MiddleRight;

            _gsVrDetailCenter = new GUIStyle(_gsVrDetailLabel);
            _gsVrDetailCenter.alignment = TextAnchor.MiddleCenter;

            // Action button (reuses _vrBtnTex for the menu-button look)
            _gsVrBtn = new GUIStyle(GUI.skin.button);
            _gsVrBtn.fontSize    = 16;
            _gsVrBtn.fixedHeight = 0f;
            _gsVrBtn.normal.textColor  = Color.white;
            _gsVrBtn.hover.textColor   = new Color(0.30f, 0.85f, 1f);
            _gsVrBtn.active.textColor  = Color.white;
            if (_vrFont != null) _gsVrBtn.font = _vrFont;
            if (_vrBtnTex != null)
            {
                _gsVrBtn.normal.background  = _vrBtnTex;
                _gsVrBtn.hover.background   = _vrBtnTex;
                _gsVrBtn.active.background  = _vrBtnTex;
            }

            // Ghost button: invisible background, subtle hover/active highlight
            _gsVrGhost = new GUIStyle();
            _gsVrGhost.normal.background = null;
            _gsVrGhost.hover.background  = _vrHoverTex;
            _gsVrGhost.active.background = _vrActiveTex;

            // Big play button (inherits _gsVrBtn, green tint)
            _gsVrPlayBtn = new GUIStyle(_gsVrBtn);
            _gsVrPlayBtn.fontSize = 22;
            _gsVrPlayBtn.normal.textColor = new Color(0.25f, 1f, 0.55f);
            _gsVrPlayBtn.hover.textColor  = Color.white;
        }

        // ---- Style utilities -------------------------------------------------
        // Tries to borrow Font from CNRSettingsMod._gameFont; falls back to UILabel scan.
        private Font VrFindFont()
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var t = asm.GetType("CNRSettingsMod.SettingsModHook");
                    if (t == null) continue;
                    var fi = t.GetField("_gameFont",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    if (fi != null) { Font f = fi.GetValue(null) as Font; if (f != null) return f; }
                    break;
                }
            }
            catch { }
            try
            {
                var lbls = (UILabel[])UnityEngine.Object.FindObjectsOfType(typeof(UILabel));
                foreach (var l in lbls)
                    if (l.font != null && l.font.dynamicFont != null)
                        return l.font.dynamicFont;
            }
            catch { }
            return null;
        }

        // Borrows a Texture2D from CNRSettingsMod's static cache by reflection.
        private static Texture2D VrBorrowTex(string fieldName)
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var t = asm.GetType("CNRSettingsMod.SettingsModHook");
                    if (t == null) continue;
                    var fi = t.GetField(fieldName,
                        BindingFlags.Static | BindingFlags.NonPublic);
                    if (fi != null) return fi.GetValue(null) as Texture2D;
                }
            }
            catch { }
            return null;
        }

        private static Texture2D VrMakeTex(Color col)
        {
            Texture2D t = new Texture2D(2, 2);
            Color[] p = { col, col, col, col };
            t.SetPixels(p); t.Apply();
            return t;
        }

        private static string VrFmtDur(float sec)
        {
            if (sec < 0f) return "? s";
            int s = (int)sec;
            int h = s / 3600; s -= h * 3600;
            int m = s / 60;   s -= m * 60;
            if (h > 0) return h + ":" + m.ToString("D2") + ":" + s.ToString("D2");
            if (m > 0) return m + ":" + s.ToString("D2");
            return s + " s";
        }

        private static string VrFmtBytes(long b)
        {
            if (b <= 0)          return "0 B";
            if (b < 1024)        return b + " B";
            if (b < 1024 * 1024) return (b / 1024f).ToString("F1") + " KB";
            return (b / (1024f * 1024f)).ToString("F1") + " MB";
        }
    }

    // CaptureDisplay removed — OnRenderImage approach is the active capture path.

    // Intercepts the camera's rendered output via OnRenderImage, which provides the
    // actual internal RT ('src') before Unity blits it to the GL backbuffer on swap.
    // Attached to the primary (Skybox) camera so src contains actual scene content.
    public class PostRenderCapture : MonoBehaviour
    {
        internal RenderTexture CapRT;
        internal bool          FrameReady;
        // Src probe: submitted in OnRenderImage, read at WaitForEndOfFrame by RecordingHook.
        internal bool      SrcProbeSubmitted;
        internal Texture2D SrcProbeTex;
        private  int       _srcProbeCount;

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (CapRT != null)
            {
                Graphics.Blit(src, CapRT);
                // Probe src center pixel on first 5 frames (diagnostic).
                // ReadPixels is submitted to render thread here; result read at WaitForEndOfFrame.
                if (_srcProbeCount < 5)
                {
                    if (SrcProbeTex == null) SrcProbeTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    RenderTexture.active = src;
                    SrcProbeTex.ReadPixels(new Rect(src.width / 2, src.height / 2, 1, 1), 0, 0, false);
                    RenderTexture.active = null;
                    SrcProbeSubmitted = true;
                    _srcProbeCount++;
                }
                FrameReady = true;
            }
            Graphics.Blit(src, dest);  // must pass through or screen goes black
        }

        private void OnDestroy()
        {
            if (SrcProbeTex != null) { Destroy(SrcProbeTex); SrcProbeTex = null; }
        }
    }

    // Proxy component added to the RecordBtnInGame GameObject.
    // Replaces the disabled UIButtonEventKit; NGUI calls OnClick() via SendMessage.
    public class RecordBtnClick : MonoBehaviour
    {
        public RecordingHook hook;

        private void OnClick()
        {
            if (hook == null) return;
            if (hook.IsCapturing)
            {
                hook.StopCapture();
                // Reset sprite back to "start" state
                var imgBtn = GetComponent("UIImageButton");
                if (imgBtn != null)
                {
                    var t = imgBtn.GetType();
                    var f1 = t.GetField("normalSprite",  System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var f2 = t.GetField("hoverSprite",   System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var f3 = t.GetField("pressedSprite", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    if (f1 != null) f1.SetValue(imgBtn, "VideoRecord_Start");
                    if (f2 != null) f2.SetValue(imgBtn, "VideoRecord_Start");
                    if (f3 != null) f3.SetValue(imgBtn, "VideoRecord_Start");
                }
                var sprite = GetComponentInChildren<UISprite>();
                if (sprite != null) { sprite.spriteName = "VideoRecord_Start"; sprite.MarkAsChanged(); }
                var label = GetComponentInChildren<UILabel>();
                if (label != null) label.text = string.Empty;
            }
            else if (!hook.IsEncoding)
            {
                hook.StartCapture();
                var imgBtn = GetComponent("UIImageButton");
                if (imgBtn != null)
                {
                    var t = imgBtn.GetType();
                    var f1 = t.GetField("normalSprite",  System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var f2 = t.GetField("hoverSprite",   System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var f3 = t.GetField("pressedSprite", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    if (f1 != null) f1.SetValue(imgBtn, "VideoRecord_Stop");
                    if (f2 != null) f2.SetValue(imgBtn, "VideoRecord_Stop");
                    if (f3 != null) f3.SetValue(imgBtn, "VideoRecord_Stop");
                }
                var sprite = GetComponentInChildren<UISprite>();
                if (sprite != null) { sprite.spriteName = "VideoRecord_Stop"; sprite.MarkAsChanged(); }
                var label = GetComponentInChildren<UILabel>();
                if (label != null) label.text = ".REC";
            }
        }
    }

    // Proxy added to the main-menu ShowVideoBtn GameObject.
    // Replaces the disabled UIButtonEventKit; NGUI calls OnClick() via SendMessage.
    public class RecordingsBtnClick : MonoBehaviour
    {
        public RecordingHook hook;
        private void OnClick() { if (hook != null) hook.OpenViewer(); }
    }
}
