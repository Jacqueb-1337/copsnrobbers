// CNRRecordingMod.cs — screenshot-based in-game recording for Cops N Robbers
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
//   CaptureFps   — frames captured per second (default 5)
//   CaptureScale — downscale factor applied before PNG encode (default 0.5)
//                  At 0.5x and 5fps a 720p screen ≈ 1 MB/s (~60 MB/min).
//
// ENTRY POINT
//   CNRRecordingMod.RecordingModEntry.Load()
//   Called automatically by CNRMod's LoadExternalMods() scanner.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRRecordingMod
{
    // ──────────────────────────────────────────────────────────────────────────
    //  Entry point — CNRMod DLL scanner calls the first public static Load()
    // ──────────────────────────────────────────────────────────────────────────
    public static class RecordingModEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/recording.log";
        public  const string Version = "1.0.1";

        private static bool _loaded = false;

        public static void Load()
        {
            if (_loaded) { Log("already loaded, skipping"); return; }
            _loaded = true;
            Log("=== CNRRecordingMod v" + Version + " loading ===");
            try
            {
                TryRegisterWithCNRMod();
                var go = new GameObject("CNRRecordingMod_Root");
                go.AddComponent<RecordingHook>();
                GameObject.DontDestroyOnLoad(go);
                Log("Load OK");
            }
            catch (Exception ex) { Log("Load() error: " + ex); }
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
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); } catch { }
            try { Debug.Log("[CNRRecording] " + msg); } catch { }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Kamcord.Implementation replacement
    //
    //  Injected into Kamcord.implementation_ (private static) so that all calls
    //  through VideoRecordController → Kamcord.* → implementation().* reach us.
    //  Because Kamcord.Implementation is a public class its virtual methods are
    //  overridable from another assembly; we just need Assembly-CSharp-firstpass
    //  in the compile references (handled in build_mod.ps1).
    // ──────────────────────────────────────────────────────────────────────────
    internal class RecordingKamcordImpl : Kamcord.Implementation
    {
        private readonly RecordingHook _hook;

        public RecordingKamcordImpl(RecordingHook hook)
        {
            _hook = hook;
        }

        public override bool IsEnabled()      { return true; }
        public override bool IsRecording()    { return _hook.IsCapturing; }
        public override void StartRecording() { _hook.StartCapture(); }
        public override void StopRecording()  { _hook.StopCapture(); }
        public override void ShowView()       { _hook.OpenViewer(); }
        public override void ShowWatchView()  { _hook.OpenViewer(); }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  MonoBehaviour: lives for the whole session (DontDestroyOnLoad)
    //  Handles frame capture and the IMGUI recordings viewer.
    // ──────────────────────────────────────────────────────────────────────────
    public class RecordingHook : MonoBehaviour
    {
        // ── paths ─────────────────────────────────────────────────────────────
        private const string RecordingsDir = "/storage/emulated/0/CNRMods/recordings";

        // ── capture config (tweak and rebuild to change) ─────────────────────
        private const float CaptureFps   = 5f;    // frames captured per second
        private const float CaptureScale = 0.5f;  // render-texture downscale factor

        // ── GUI scaling: all coordinates are in virtual pixels at REF_W wide ──
        // GUIUtility.ScaleAroundPivot maps virtual → physical so the window
        // looks the same physical size regardless of screen resolution.
        private const float REF_W = 600f;

        // ── capture runtime state ─────────────────────────────────────────────
        public  bool   IsCapturing  { get; private set; }
        private string _sessionDir;
        private int    _frameCount;
        private float  _nextCapture;

        // ── viewer state ──────────────────────────────────────────────────────
        private bool   _viewerOpen;
        private Rect   _viewerRect;
        private List<SessionInfo> _sessions = new List<SessionInfo>();
        private Vector2 _sessionScroll;
        private int     _selectedIdx = -1;
        private Texture2D _previewTex;
        private int     _previewFrame;
        private string  _statusMsg;

        private struct SessionInfo
        {
            public string Dir;
            public string Name;
            public int    FrameCount;
        }

        // ── lifecycle ─────────────────────────────────────────────────────────
        private void Awake()
        {
            // Ensure recordings folder exists at startup
            try { Directory.CreateDirectory(RecordingsDir); } catch { }

            // Size viewer relative to screen; will be set properly on first open
            _viewerRect = new Rect(40, 40, 600, 400);

            InjectKamcord();
        }

        private void InjectKamcord()
        {
            try
            {
                FieldInfo fi = typeof(Kamcord).GetField("implementation_",
                    BindingFlags.NonPublic | BindingFlags.Static);
                if (fi == null)
                {
                    RecordingModEntry.Log("WARNING: Kamcord.implementation_ field not found — injection skipped");
                    return;
                }
                fi.SetValue(null, new RecordingKamcordImpl(this));
                RecordingModEntry.Log("Kamcord.implementation_ replaced OK (Kamcord stub overridden)");
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("InjectKamcord error: " + ex.Message);
            }
        }

        private void OnDestroy()
        {
            ClearPreview();
        }

        // ── capture API (called by RecordingKamcordImpl) ──────────────────────
        public void StartCapture()
        {
            if (IsCapturing) return;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _sessionDir  = Path.Combine(RecordingsDir, timestamp);
            _frameCount  = 0;
            _nextCapture = Time.time; // capture immediately on first Update

            try { Directory.CreateDirectory(_sessionDir); }
            catch (Exception ex)
            {
                RecordingModEntry.Log("StartCapture: mkdir failed: " + ex.Message);
                return;
            }

            IsCapturing = true;
            RecordingModEntry.Log("StartCapture: session=" + timestamp +
                "  fps=" + CaptureFps + "  scale=" + (CaptureScale * 100f) + "%");
        }

        public void StopCapture()
        {
            if (!IsCapturing) return;
            IsCapturing = false;

            RecordingModEntry.Log("StopCapture: " + _frameCount + " frames → " +
                Path.GetFileName(_sessionDir));

            // Write metadata file so the viewer shows frame count instantly
            try
            {
                string meta =
                    "frames=" + _frameCount + "\n" +
                    "fps="    + CaptureFps   + "\n" +
                    "scale="  + CaptureScale + "\n" +
                    "date="   + DateTime.Now.ToString("o");
                File.WriteAllText(Path.Combine(_sessionDir, "recording.meta"), meta);
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("StopCapture: meta write failed: " + ex.Message);
            }

            _sessionDir = null;
        }

        public void OpenViewer()
        {
            // Compute virtual screen dimensions using the same scale factor applied in OnGUI
            float sc = Screen.width / REF_W;
            float vw = REF_W;
            float vh = Screen.height / sc;

            // Size the viewer to ~90% of the virtual screen
            _viewerRect = new Rect(
                vw * 0.05f,
                vh * 0.05f,
                vw * 0.90f,
                vh * 0.85f);

            RefreshSessions();
            _viewerOpen  = true;
            _selectedIdx = -1;
            ClearPreview();
        }

        // ── frame capture coroutine ───────────────────────────────────────────
        private void Update()
        {
            if (!IsCapturing) return;
            if (Time.time < _nextCapture) return;
            _nextCapture = Time.time + (1f / CaptureFps);
            StartCoroutine(CaptureFrameCoroutine());
        }

        private IEnumerator CaptureFrameCoroutine()
        {
            // Wait for the frame to be fully rendered before reading pixels
            yield return new WaitForEndOfFrame();
            if (!IsCapturing || _sessionDir == null) yield break;

            try
            {
                int sw = Screen.width;
                int sh = Screen.height;
                int dw = Mathf.Max(1, Mathf.RoundToInt(sw * CaptureScale));
                int dh = Mathf.Max(1, Mathf.RoundToInt(sh * CaptureScale));

                // Read the backbuffer at full resolution
                var fullTex = new Texture2D(sw, sh, TextureFormat.RGB24, false);
                fullTex.ReadPixels(new Rect(0, 0, sw, sh), 0, 0);
                fullTex.Apply();

                // GPU-accelerated downscale via RenderTexture + Blit
                var rt = RenderTexture.GetTemporary(dw, dh, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(fullTex, rt);
                Destroy(fullTex);

                RenderTexture prevActive = RenderTexture.active;
                RenderTexture.active = rt;
                var scaledTex = new Texture2D(dw, dh, TextureFormat.RGB24, false);
                scaledTex.ReadPixels(new Rect(0, 0, dw, dh), 0, 0);
                scaledTex.Apply();
                RenderTexture.active = prevActive;
                RenderTexture.ReleaseTemporary(rt);

                byte[] png = scaledTex.EncodeToPNG();
                Destroy(scaledTex);

                string framePath = Path.Combine(_sessionDir,
                    "frame_" + _frameCount.ToString("D5") + ".png");
                File.WriteAllBytes(framePath, png);
                _frameCount++;
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("CaptureFrame[" + _frameCount + "] error: " + ex.Message);
            }
        }

        // ── viewer: session list persistence ─────────────────────────────────
        private void RefreshSessions()
        {
            _sessions.Clear();
            _statusMsg = null;
            try
            {
                if (!Directory.Exists(RecordingsDir)) return;
                foreach (string dir in Directory.GetDirectories(RecordingsDir))
                {
                    int frames = 0;
                    string metaFile = Path.Combine(dir, "recording.meta");
                    if (File.Exists(metaFile))
                    {
                        foreach (string line in File.ReadAllLines(metaFile))
                        {
                            if (line.StartsWith("frames="))
                                int.TryParse(line.Substring(7), out frames);
                        }
                    }
                    else
                    {
                        // Fall back to counting PNG files (incomplete / legacy sessions)
                        frames = Directory.GetFiles(dir, "frame_*.png").Length;
                    }
                    _sessions.Add(new SessionInfo
                    {
                        Dir        = dir,
                        Name       = Path.GetFileName(dir),
                        FrameCount = frames
                    });
                }
                // Newest first (folder names are timestamps so lexicographic desc = newest first)
                _sessions.Sort((a, b) =>
                    string.Compare(b.Name, a.Name, StringComparison.Ordinal));
            }
            catch (Exception ex)
            {
                _statusMsg = "Error reading recordings: " + ex.Message;
                RecordingModEntry.Log("RefreshSessions error: " + ex.Message);
            }
        }

        // ── viewer: preview texture loading ──────────────────────────────────
        private void LoadPreviewFrame(int frame)
        {
            if (_selectedIdx < 0 || _selectedIdx >= _sessions.Count) return;
            ClearPreview();
            _previewFrame = frame;
            try
            {
                string path = Path.Combine(_sessions[_selectedIdx].Dir,
                    "frame_" + frame.ToString("D5") + ".png");
                if (!File.Exists(path))
                {
                    _statusMsg = "Frame file not found";
                    return;
                }
                byte[] data = File.ReadAllBytes(path);
                _previewTex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                _previewTex.LoadImage(data);
                _statusMsg  = null;
            }
            catch (Exception ex)
            {
                _statusMsg = "Load error: " + ex.Message;
                RecordingModEntry.Log("LoadPreviewFrame error: " + ex.Message);
            }
        }

        private void ClearPreview()
        {
            if (_previewTex != null) { Destroy(_previewTex); _previewTex = null; }
        }

        // ── IMGUI viewer ──────────────────────────────────────────────────────
        private void OnGUI()
        {
            if (!_viewerOpen) return;

            // Scale GUI so all virtual-pixel coordinates map to a consistent
            // physical size regardless of screen resolution (same technique as
            // CNRModManager).
            float sc = Screen.width / REF_W;
            GUIUtility.ScaleAroundPivot(new Vector2(sc, sc), Vector2.zero);

            _viewerRect = GUI.Window(0xCEC0, _viewerRect, DrawViewerWindow, "CNR Recordings");
        }

        private void DrawViewerWindow(int id)
        {
            float w    = _viewerRect.width;
            float h    = _viewerRect.height;
            float btnH = 22f;

            // Drag handle (leave the close button area free)
            GUI.DragWindow(new Rect(0, 0, w - 28, 18));

            // Close button
            if (GUI.Button(new Rect(w - 26, 1, 24, 16), "X"))
            {
                _viewerOpen = false;
                ClearPreview();
                return;
            }

            float innerH  = h - 22f;           // usable height below title bar
            float listW   = 185f;
            float midX    = listW + 6f;
            float thumbW  = w - midX - 4f;
            float navH    = btnH + 6f;
            float thumbH  = innerH - navH - 2f;

            // ── Left: session list ────────────────────────────────────────────
            float listAreaH = innerH - btnH - 4f;
            _sessionScroll = GUI.BeginScrollView(
                new Rect(2, 22, listW, listAreaH),
                _sessionScroll,
                new Rect(0, 0, listW - 20, Mathf.Max(listAreaH, _sessions.Count * 52)));

            for (int i = 0; i < _sessions.Count; i++)
            {
                SessionInfo s = _sessions[i];
                bool active = (i == _selectedIdx);
                GUI.backgroundColor = active ? Color.cyan : Color.white;
                if (GUI.Button(new Rect(0, i * 52, listW - 22, 48),
                    s.Name + "\n" + s.FrameCount + " frames"))
                {
                    if (_selectedIdx != i)
                    {
                        _selectedIdx  = i;
                        _previewFrame = 0;
                        LoadPreviewFrame(0);
                    }
                }
            }
            GUI.backgroundColor = Color.white;
            GUI.EndScrollView();

            // Refresh button below the list
            if (GUI.Button(new Rect(2, 22 + listAreaH + 2, listW, btnH), "Refresh"))
            {
                RefreshSessions();
                _selectedIdx = -1;
                ClearPreview();
            }

            // ── Right: preview frame ──────────────────────────────────────────
            float previewY = 22f;
            if (_previewTex != null)
            {
                GUI.DrawTexture(
                    new Rect(midX, previewY, thumbW, thumbH),
                    _previewTex, ScaleMode.ScaleToFit);
            }
            else
            {
                string placeholder = _selectedIdx >= 0
                    ? (_statusMsg ?? "Loading…")
                    : "← Select a recording";
                GUI.Label(new Rect(midX, previewY + 10, thumbW, thumbH - 10), placeholder);
            }

            // Frame navigation row
            float navY   = 22f + thumbH + 2f;
            int   maxF   = _selectedIdx >= 0
                ? Mathf.Max(0, _sessions[_selectedIdx].FrameCount - 1)
                : 0;

            bool prevEnabled = (_selectedIdx >= 0 && _previewFrame > 0);
            bool nextEnabled = (_selectedIdx >= 0 && _previewFrame < maxF);

            GUI.enabled = prevEnabled;
            if (GUI.Button(new Rect(midX, navY, 36, btnH), "<<"))
                LoadPreviewFrame(_previewFrame - 1);

            GUI.enabled = true;
            string frameLabel = _selectedIdx >= 0
                ? "Frame " + (_previewFrame + 1) + " / " + _sessions[_selectedIdx].FrameCount
                : "—";
            GUI.Label(new Rect(midX + 40, navY + 2, thumbW - 80, btnH), frameLabel);

            GUI.enabled = nextEnabled;
            if (GUI.Button(new Rect(midX + thumbW - 38, navY, 36, btnH), ">>"))
                LoadPreviewFrame(_previewFrame + 1);
            GUI.enabled = true;

            // Status / error line
            if (_statusMsg != null)
                GUI.Label(new Rect(midX, navY + btnH + 2, thumbW, 18), _statusMsg);
        }
    }
}
