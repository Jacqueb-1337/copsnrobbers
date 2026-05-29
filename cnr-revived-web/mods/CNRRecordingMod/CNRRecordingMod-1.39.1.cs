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
using System.Runtime.InteropServices;
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
        public  const string Version = "1.39.1";

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
        private byte[]        _nv12Buf;       // pre-allocated NV12 scratch for Phase 1 writes
        internal RenderTexture _captureRT;    // game cameras redirect here; RT reads are reliable on Android

        private GameObject    _displayCamGo;  // depth-1000 camera that blits captureRT to screen
        // Tightly packed NV12 — no alignment padding in capture files.
        // EncodeThread reads the real encoder stride via getInputFormat and re-strides.
        private const int CaptureStride = VideoWidth;   // 854, no padding
        private const int CaptureSliceH = VideoHeight;  // 480

        // Phase 2 state (background encode thread)
        private string    _encodeOutputPath;
        private string    _encodeError;

        // Viewer
        private bool         _viewerOpen;
        private Rect         _viewerRect;
        private List<string> _recordings = new List<string>();
        private Vector2      _listScroll;
        private string       _statusMsg;

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
            // Auto-record 5 seconds immediately on load for diagnostics:
            // if pixels are grey here (main menu, no Kamcord active) then the problem
            // is not Kamcord at all and we need a different approach.
            StartCoroutine(AutoRecordDiagnostic());
        }

        private IEnumerator AutoRecordDiagnostic()
        {
            yield return new WaitForSeconds(1f); // let scene finish initialising
            RecordingModEntry.Log("AutoRecord: starting 5s diagnostic capture (scene=" + Application.loadedLevelName + ")");
            StartCapture();
            yield return new WaitForSeconds(5f);
            if (IsCapturing)
            {
                RecordingModEntry.Log("AutoRecord: stopping");
                StopCapture();
            }
        }

        private void OnLevelWasLoaded(int level)
        {
            _btnHooked = false;
            if (System.Array.IndexOf(GameScenes, Application.loadedLevelName) >= 0)
                StartCoroutine(HookRecordButton());
            // OnRenderImage capture: no camera re-hooking needed on scene load.
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

        private void OnDestroy()
        {
            if (IsCapturing) StopCapture();
            if (_displayCamGo != null) { Destroy(_displayCamGo); _displayCamGo = null; }
            if (_captureRT   != null) { Destroy(_captureRT);   _captureRT   = null; }
            if (_readTex     != null) { Destroy(_readTex);     _readTex     = null; }
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
            _encodeOutputPath = Path.Combine(RecordingsDir, timestamp + ".mp4");

            try { Directory.CreateDirectory(_sessionDir); }
            catch (Exception ex) { RecordingModEntry.Log("StartCapture: mkdir failed: " + ex.Message); return; }

            int scrW = Screen.width, scrH = Screen.height;
            if (_readTex != null && (_readTex.width != scrW || _readTex.height != scrH))
            { Destroy(_readTex); _readTex = null; }
            if (_readTex == null)
                _readTex = new Texture2D(scrW, scrH, TextureFormat.RGBA32, false);
            _scrW = scrW; _scrH = scrH;

            int nv12Size = CaptureStride * CaptureSliceH * 3 / 2;
            if (_nv12Buf == null || _nv12Buf.Length != nv12Size)
                _nv12Buf = new byte[nv12Size];

            // captureRT is the destination for CaptureDisplay.OnRenderImage.
            // OnRenderImage blits the composited scene (src) into this RT each frame;
            // CaptureFrameCoroutine then does ReadPixels from it after WaitForEndOfFrame.
            if (_captureRT != null && (_captureRT.width != scrW || _captureRT.height != scrH))
            { Destroy(_captureRT); _captureRT = null; }
            if (_captureRT == null)
            {
                _captureRT = new RenderTexture(scrW, scrH, 24, RenderTextureFormat.ARGB32);
                _captureRT.Create();
            }

            // Create the display camera if needed (no camera hooking —
            // CaptureDisplay.OnRenderImage captures the composited scene from src).
            if (_displayCamGo == null)
            {
                _displayCamGo = new GameObject("__CNRDisplayCam__");
                GameObject.DontDestroyOnLoad(_displayCamGo);
                var dCam = _displayCamGo.AddComponent<Camera>();
                dCam.depth       = 1000f;
                dCam.clearFlags  = CameraClearFlags.Nothing;
                dCam.cullingMask = 0;
                var disp = _displayCamGo.AddComponent<CaptureDisplay>();
                disp.hook = this;
                RecordingModEntry.Log("StartCapture: CaptureDisplay created (depth=1000)");
            }

            IsCapturing = true;
            RecordingModEntry.Log("StartCapture: session=" + timestamp
                + " scrn=" + scrW + "x" + scrH
                + " enc=" + VideoWidth + "x" + VideoHeight + "@" + VideoFps
                + " captureStride=" + CaptureStride + " captureSliceH=" + CaptureSliceH
                + " mode=OnRenderImage");
        }

        public void StopCapture()
        {
            if (!IsCapturing) { RecordingModEntry.Log("StopCapture: not capturing"); return; }
            IsCapturing = false;
            if (_displayCamGo != null) { Destroy(_displayCamGo); _displayCamGo = null; }
            if (_captureRT    != null) { Destroy(_captureRT);    _captureRT    = null; }
            RecordingModEntry.Log("StopCapture: " + _capturedFrames + " frames captured -> starting encode thread");

            // Spawn background encode thread now that capture is done.
            // MediaCodec runs here, AFTER all ReadPixels are finished.
            IsEncoding = true;
            var t = new Thread(EncodeThread);
            t.IsBackground = true;
            t.Start();
        }

        // Camera hooking removed — OnRenderImage capture is used instead.
        // The display camera (depth=1000, cullingMask=0, clearFlags=Nothing) receives
        // src = the fully composited game frame from all lower cameras, and blits it
        // into _captureRT so CaptureFrameCoroutine can ReadPixels it.

        // RehookCameras removed — using OnRenderImage capture, no camera hooking needed.

        public void OpenViewer()
        {
            _viewerRect = new Rect(
                Screen.width  * 0.05f,
                Screen.height * 0.05f,
                Screen.width  * 0.70f,
                Screen.height * 0.75f);
            RefreshRecordings();
            _viewerOpen = true;
        }

        private void Update()
        {
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
                // Read from captureRT — game cameras render into it every frame.
                // RT readback is reliable on Android; backbuffer readback always returns grey.
                bool rtReady = _captureRT != null && _captureRT.IsCreated();
                if (!rtReady)
                {
                    if (verbose) RecordingModEntry.Log("  WARN: captureRT not ready — OnRenderImage hasn't fired yet");
                    _encodingFrame = false; yield break;
                }
                // CaptureDisplay.OnRenderImage already blitted the composited scene
                // into _captureRT this frame. Just read it out.
                _readTex.Apply(false);

                // First-frame diagnostic PNG
                if (_capturedFrames == 0)
                {
                    try
                    {
                        byte[] png = _readTex.EncodeToPNG();
                        string pngPath = Path.Combine(_sessionDir, "frame0_screen.png");
                        File.WriteAllBytes(pngPath, png);
                        RecordingModEntry.Log("  diag PNG -> " + pngPath);
                    }
                    catch (Exception pex) { RecordingModEntry.Log("  diag PNG err: " + pex.Message); }
                }

                Color32[] px = _readTex.GetPixels32();
                int texW = _readTex.width;

                if (verbose)
                {
                    Color32 pC  = px[(_scrH / 2) * texW + _scrW / 2];
                    Color32 pTL = px[(_scrH - 1) * texW];
                    Color32 pBR = px[_scrW - 1];
                    RecordingModEntry.Log("  pC=(" + pC.r + "," + pC.g + "," + pC.b + ")"
                        + " pTL=(" + pTL.r + "," + pTL.g + "," + pTL.b + ")"
                        + " pBR=(" + pBR.r + "," + pBR.g + "," + pBR.b + ")");
                }

                // Convert RGBA -> NV12 into _nv12Buf (tightly packed, stride=VideoWidth)
                for (int i = 0; i < _nv12Buf.Length; i++) _nv12Buf[i] = 128;
                int yBase  = 0;
                int uvBase = CaptureStride * CaptureSliceH;
                for (int row = 0; row < VideoHeight; row++)
                {
                    int srcRow = ((VideoHeight - 1 - row) * _scrH) / VideoHeight;
                    for (int col = 0; col < VideoWidth; col++)
                    {
                        int srcCol = (col * _scrW) / VideoWidth;
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

                // Write raw NV12 frame to disk
                string framePath = Path.Combine(_sessionDir,
                    "frame_" + _capturedFrames.ToString("D5") + ".nv12");
                File.WriteAllBytes(framePath, _nv12Buf);
                _capturedFrames++;
                if (verbose) RecordingModEntry.Log("  wrote " + framePath);
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("CaptureFrame[" + _capturedFrames + "] error: " + ex.Message);
            }
            _encodingFrame = false;
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
                    "video/avc", VideoWidth, VideoHeight);
                mediaFmt.Call("setInteger", "bitrate",          VideoBitrate);
                mediaFmt.Call("setInteger", "frame-rate",       VideoFps);
                mediaFmt.Call("setInteger", "i-frame-interval", 2);
                mediaFmt.Call("setInteger", "color-format",     COLOR_FMT_YUV420);

                var codec = new AndroidJavaClass("android.media.MediaCodec")
                    .CallStatic<AndroidJavaObject>("createEncoderByType", "video/avc");
                codec.Call("configure", mediaFmt, null, null, 1);
                codec.Call("start");
                RecordingModEntry.Log("  codec.start OK");
                // Query which color formats this device's AVC encoder supports.
                // API 21+ uses new MediaCodecList(int).getCodecInfos(); the old static
                // getCodecs() was removed in later Android releases.
                try
                {
                    var mcList = new AndroidJavaObject("android.media.MediaCodecList", 0 /*REGULAR_CODECS*/);
                    var infos  = mcList.Call<AndroidJavaObject[]>("getCodecInfos");
                    foreach (var info in infos)
                    {
                        if (!info.Call<bool>("isEncoder")) continue;
                        string name = info.Call<string>("getName");
                        if (!name.ToLower().Contains("avc")) continue;
                        var caps  = info.Call<AndroidJavaObject>("getCapabilitiesForType", "video/avc");
                        int[] fmts = caps.Get<int[]>("colorFormats");
                        string s = "";
                        foreach (int f in fmts) s += f + " ";
                        RecordingModEntry.Log("  encoder " + name + " colorFormats: " + s.Trim());
                        break;
                    }
                }
                catch (Exception ex) { RecordingModEntry.Log("  colorFormat query: " + ex.Message); }

                int stride = CaptureStride, sliceH = CaptureSliceH;
                try
                {
                    var inFmt = codec.Call<AndroidJavaObject>("getInputFormat");
                    stride = inFmt.Call<int>("getInteger", "stride");
                    sliceH = inFmt.Call<int>("getInteger", "slice-height");
                    RecordingModEntry.Log("  stride=" + stride + " sliceH=" + sliceH);
                }
                catch (Exception ex) { RecordingModEntry.Log("  getInputFormat: " + ex.Message); }

                var muxer      = new AndroidJavaObject("android.media.MediaMuxer", _encodeOutputPath, 0);
                var bufferInfo = new AndroidJavaObject("android.media.MediaCodec$BufferInfo");
                bool muxerStarted  = false;
                int  videoTrackIdx = -1;
                long ptsUsec       = 0;

                int encBufSize = stride * sliceH * 3 / 2;
                var bufCls  = AndroidJNI.FindClass("java/nio/Buffer");
                var fidAddr = AndroidJNI.GetFieldID(bufCls, "address", "J");

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

                    if (f < 3) RecordingModEntry.Log("  encoding frame " + f
                        + " captureLen=" + nv12.Length + " feedLen=" + feedBuf.Length
                        + " Y[0]=" + feedBuf[0]
                        + " UV=(" + feedBuf[stride * sliceH] + "," + feedBuf[stride * sliceH + 1] + ")");

                    ptsUsec += (long)(1000000L / VideoFps);

                    int inIdx = codec.Call<int>("dequeueInputBuffer", (long)100000);
                    if (inIdx >= 0)
                    {
                        // getInputBuffer(idx) is the modern API 21+ single-buffer call.
                        // getInputBuffers() is deprecated and may return detached buffers on API 36.
                        var rawBuf   = codec.Call<AndroidJavaObject>("getInputBuffer", inIdx);
                        int bufCap   = rawBuf.Call<int>("capacity");
                        int putLen   = System.Math.Min(bufCap, feedBuf.Length);
                        long nAddr   = AndroidJNI.GetLongField(rawBuf.GetRawObject(), fidAddr);
                        Marshal.Copy(feedBuf, 0, new IntPtr(nAddr), putLen);
                        codec.Call("queueInputBuffer", inIdx, 0, putLen, ptsUsec, 0);
                    }

                    // drain
                    for (int d = 0; d < 10; d++)
                    {
                        int outIdx = codec.Call<int>("dequeueOutputBuffer", bufferInfo, (long)10000);
                        if (outIdx == INFO_TRY_AGAIN_LATER) break;
                        if (outIdx == INFO_OUTPUT_FORMAT_CHANGED)
                        {
                            if (!muxerStarted)
                            {
                                var fmt = codec.Call<AndroidJavaObject>("getOutputFormat");
                                videoTrackIdx = muxer.Call<int>("addTrack", fmt);
                                muxer.Call("start");
                                muxerStarted = true;
                                RecordingModEntry.Log("  muxer started track=" + videoTrackIdx);
                            }
                            // Break instead of continue: dequeueOutputBuffer immediately
                            // after muxer.start() throws IllegalStateException on SDK 33.
                            // The next frame's drain loop will pick up the pending output.
                            break;
                        }
                        if (outIdx < 0) break;
                        int flags = bufferInfo.Get<int>("flags");
                        int size  = bufferInfo.Get<int>("size");
                        if ((flags & 2) == 0 && size > 0 && muxerStarted)
                        {
                            // getOutputBuffer(idx) returns a properly-positioned ByteBuffer for
                            // this specific frame. getOutputBuffers() array is deprecated API 21+
                            // and on API 36 the buffers may be detached/invalid.
                            var outBuf = codec.Call<AndroidJavaObject>("getOutputBuffer", outIdx);
                            muxer.Call("writeSampleData", videoTrackIdx, outBuf, bufferInfo);
                        }
                        codec.Call("releaseOutputBuffer", outIdx, false);
                        if ((flags & 4) != 0) break;
                    }
                }

                // EOS
                int eosIdx = codec.Call<int>("dequeueInputBuffer", (long)100000);
                if (eosIdx >= 0) codec.Call("queueInputBuffer", eosIdx, 0, 0, ptsUsec, 4);

                // final drain
                for (int d = 0; d < 30; d++)
                {
                    int outIdx = codec.Call<int>("dequeueOutputBuffer", bufferInfo, (long)50000);
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
            _statusMsg = null;
            try
            {
                if (!Directory.Exists(RecordingsDir)) return;
                _recordings.AddRange(Directory.GetFiles(RecordingsDir, "*.mp4"));
                _recordings.Sort((a, b) => string.Compare(b, a, StringComparison.Ordinal));
            }
            catch (Exception ex) { _statusMsg = "Error: " + ex.Message; }
        }

        private void OnGUI()
        {
            // Status indicator (top-right). Read-only — start/stop come from the
            // existing Kamcord "Start Recording" / "Stop Recording" OnGUI buttons.
            if (IsCapturing || IsEncoding)
            {
                float bw = Screen.width  * 0.09f;
                float bh = Screen.height * 0.06f;
                GUI.Label(new Rect(Screen.width - bw - 8f, 8f, bw, bh),
                    IsEncoding ? "[Encoding]" : "\u25cf REC");
            }

            // Recordings viewer overlay
            if (_viewerOpen)
                _viewerRect = GUI.Window(0xCEC0, _viewerRect, DrawViewerWindow, "CNR Recordings");
        }

        private void DrawViewerWindow(int id)
        {
            float w    = _viewerRect.width;
            float h    = _viewerRect.height;
            float btnH = 22f;
            GUI.DragWindow(new Rect(0, 0, w - 28, 18));
            if (GUI.Button(new Rect(w - 26, 1, 24, 16), "X")) { _viewerOpen = false; return; }

            string encState = IsEncoding ? "  [Encoding...]" : (_encodeError != null ? "  [Encode ERR: " + _encodeError + "]" : "");
            GUI.Label(new Rect(4, 22, w - 8, 20), "MP4 recordings" + encState);

            float innerH = h - 44f;
            _listScroll = GUI.BeginScrollView(
                new Rect(2, 44, w - 4, innerH - btnH - 4),
                _listScroll,
                new Rect(0, 0, w - 24, Mathf.Max(innerH, _recordings.Count * 28)));
            for (int i = 0; i < _recordings.Count; i++)
            {
                string name = Path.GetFileName(_recordings[i]);
                long   sz   = 0;
                try { sz = new FileInfo(_recordings[i]).Length; } catch { }
                GUI.Label(new Rect(0, i * 28, w - 24, 26), name + "  (" + (sz / 1024) + " KB)");
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(2, h - btnH - 4, 100, btnH), "Refresh"))
                RefreshRecordings();
            if (_statusMsg != null)
                GUI.Label(new Rect(110, h - btnH - 4, w - 114, btnH), _statusMsg);
        }
    }

    // Depth-1000 display camera — blits captureRT to the screen so the game looks normal
    // while game cameras are redirected to captureRT.
    internal class CaptureDisplay : MonoBehaviour
    {
        public RecordingHook hook;
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (hook != null && hook.IsCapturing && hook._captureRT != null && hook._captureRT.IsCreated())
            {
                // src is the fully composited game frame from all lower-depth cameras.
                // Blit it into _captureRT so CaptureFrameCoroutine can ReadPixels from it,
                // then pass it through to dest so the screen shows the game normally.
                Graphics.Blit(src, hook._captureRT);
                Graphics.Blit(src, dest);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
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
}
