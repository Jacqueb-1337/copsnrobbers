// CNRRecordingMod.cs - hardware-encoded MP4 recording for Cops N Robbers
//
// HOW IT WORKS
//   Replaces the defunct Kamcord SDK (via reflection into Kamcord.implementation_)
//   so the existing in-game Record button and main-menu Recordings viewer work.
//
//   Recording uses Android MediaCodec (H.264) + MediaMuxer via JNI:
//     1. COLOR_FormatYUV420Flexible encoder configured via MediaFormat.
//     2. Each frame (WaitForEndOfFrame):
//          ReadPixels -> Texture2D (RGBA32)
//          RGBA -> YUV420 planar conversion in C# (~5-8 ms at 854x480)
//          dequeueInputBuffer -> copy bytes -> queueInputBuffer
//          DrainEncoder -> writeSampleData to MediaMuxer
//     3. Output: /sdcard/CNRMods/recordings/<yyyyMMdd_HHmmss>.mp4
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
    // ------------------------------------------------------------------------
    //  Entry point - CNRMod DLL scanner calls the first public static Load()
    // ------------------------------------------------------------------------
    public static class RecordingModEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/recording.log";
        public  const string Version = "1.2.0";

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

    // ------------------------------------------------------------------------
    //  Kamcord stub replacement - injected into Kamcord.implementation_
    // ------------------------------------------------------------------------
    internal class RecordingKamcordImpl : Kamcord.Implementation
    {
        private readonly RecordingHook _hook;
        public RecordingKamcordImpl(RecordingHook hook) { _hook = hook; }

        public override bool IsEnabled()      { return true; }
        public override bool IsRecording()    { return _hook.IsCapturing; }
        public override void StartRecording() { _hook.StartCapture(); }
        public override void StopRecording()  { _hook.StopCapture(); }
        public override void ShowView()       { _hook.OpenViewer(); }
        public override void ShowWatchView()  { _hook.OpenViewer(); }
    }

    // ------------------------------------------------------------------------
    //  Main MonoBehaviour
    // ------------------------------------------------------------------------
    public class RecordingHook : MonoBehaviour
    {
        // ---- paths -----------------------------------------------------------
        private const string RecordingsDir = "/storage/emulated/0/CNRMods/recordings";

        // ---- encode settings -------------------------------------------------
        private const int   VideoWidth   = 854;
        private const int   VideoHeight  = 480;
        private const int   VideoBitrate = 2000000; // 2 Mbps H.264
        private const int   VideoFps     = 30;

        // COLOR_FormatYUV420Flexible = 0x7F420888
        // Use this so we can feed raw YUV bytes via dequeueInputBuffer.
        // COLOR_FormatSurface would require an EGL surface bound to the encoder,
        // which Unity does not do automatically.
        private const int COLOR_FORMAT_YUV420 = 0x7F420888;

        // ---- GUI scale (virtual 600px wide canvas, same as CNRModManager) -----
        private const float REF_W = 600f;

        // ---- capture state ---------------------------------------------------
        public  bool IsCapturing { get; private set; }

        // Android MediaCodec / MediaMuxer JNI handles
        private AndroidJavaObject _codec;       // android.media.MediaCodec
        private AndroidJavaObject _muxer;       // android.media.MediaMuxer
        private AndroidJavaObject _bufferInfo;  // MediaCodec.BufferInfo
        private int               _videoTrackIdx = -1;
        private long              _ptsUsec       = 0;
        private string            _outputPath;
        private bool              _muxerStarted;

        // Reusable read-back texture and YUV byte array
        private Texture2D         _readTex;
        private byte[]            _yuvBuf;      // VideoWidth * VideoHeight * 3/2 bytes

        // ---- viewer state ----------------------------------------------------
        private bool         _viewerOpen;
        private Rect         _viewerRect;
        private List<string> _recordings = new List<string>();
        private Vector2      _listScroll;
        private string       _statusMsg;

        // ---------------------------------------------------------------------
        private void Awake()
        {
            try { Directory.CreateDirectory(RecordingsDir); } catch { }
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
                    RecordingModEntry.Log("WARNING: Kamcord.implementation_ not found - injection skipped");
                    return;
                }
                fi.SetValue(null, new RecordingKamcordImpl(this));
                RecordingModEntry.Log("Kamcord implementation injected OK");
            }
            catch (Exception ex) { RecordingModEntry.Log("InjectKamcord error: " + ex.Message); }
        }

        private void OnDestroy()
        {
            if (IsCapturing) StopCapture();
            if (_readTex != null) { Destroy(_readTex); _readTex = null; }
        }

        // ---------------------------------------------------------------------
        //  Capture API
        // ---------------------------------------------------------------------
        public void StartCapture()
        {
            if (IsCapturing) return;
            try
            {
                _outputPath = Path.Combine(RecordingsDir,
                    DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4");
                _ptsUsec       = 0;
                _muxerStarted  = false;
                _videoTrackIdx = -1;

                // Allocate reusable read-back texture and YUV buffer
                if (_readTex == null)
                    _readTex = new Texture2D(VideoWidth, VideoHeight, TextureFormat.RGBA32, false);
                int yuvSize = VideoWidth * VideoHeight * 3 / 2;
                if (_yuvBuf == null || _yuvBuf.Length != yuvSize)
                    _yuvBuf = new byte[yuvSize];

                // MediaFormat
                var fmtClass  = new AndroidJavaClass("android.media.MediaFormat");
                var mediaFmt  = fmtClass.CallStatic<AndroidJavaObject>(
                    "createVideoFormat", "video/avc", VideoWidth, VideoHeight);
                mediaFmt.Call("setInteger", "bitrate",          VideoBitrate);
                mediaFmt.Call("setInteger", "frame-rate",       VideoFps);
                mediaFmt.Call("setInteger", "i-frame-interval", 2);
                mediaFmt.Call("setInteger", "color-format",     COLOR_FORMAT_YUV420);

                // MediaCodec encoder
                _codec = new AndroidJavaClass("android.media.MediaCodec")
                    .CallStatic<AndroidJavaObject>("createEncoderByType", "video/avc");
                _codec.Call("configure", mediaFmt, null, null, 1); // CONFIGURE_FLAG_ENCODE=1
                _codec.Call("start");

                // MediaMuxer (MUXER_OUTPUT_MPEG_4 = 0)
                _muxer = new AndroidJavaObject("android.media.MediaMuxer", _outputPath, 0);

                // BufferInfo
                _bufferInfo = new AndroidJavaObject("android.media.MediaCodec$BufferInfo");

                IsCapturing = true;
                RecordingModEntry.Log("StartCapture -> " + _outputPath);
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("StartCapture error: " + ex);
                CleanupCodec();
            }
        }

        public void StopCapture()
        {
            if (!IsCapturing) return;
            IsCapturing = false;
            RecordingModEntry.Log("StopCapture: flushing encoder");
            try
            {
                // Queue EOS input buffer
                int eosIdx = _codec.Call<int>("dequeueInputBuffer", (long)100000);
                if (eosIdx >= 0)
                    _codec.Call("queueInputBuffer", eosIdx, 0, 0, _ptsUsec,
                        4); // BUFFER_FLAG_END_OF_STREAM = 4
                DrainEncoder(endOfStream: true);
            }
            catch (Exception ex) { RecordingModEntry.Log("StopCapture drain error: " + ex.Message); }
            CleanupCodec();
            RecordingModEntry.Log("StopCapture done -> " + _outputPath);
        }

        private void CleanupCodec()
        {
            try { if (_muxerStarted && _muxer != null) _muxer.Call("stop"); } catch { }
            try { if (_muxer != null) { _muxer.Call("release");  _muxer.Dispose();  _muxer  = null; } } catch { }
            try { if (_codec != null) { _codec.Call("stop"); _codec.Call("release"); _codec.Dispose(); _codec = null; } } catch { }
            try { if (_bufferInfo != null) { _bufferInfo.Dispose(); _bufferInfo = null; } } catch { }
            _muxerStarted  = false;
            _videoTrackIdx = -1;
        }

        // ---------------------------------------------------------------------
        //  Per-frame encode
        // ---------------------------------------------------------------------
        private void Update()
        {
            if (!IsCapturing) return;
            StartCoroutine(EncodeFrameCoroutine());
        }

        private IEnumerator EncodeFrameCoroutine()
        {
            yield return new WaitForEndOfFrame();
            if (!IsCapturing || _codec == null) yield break;
            try
            {
                // 1. ReadPixels into our reusable Texture2D
                _readTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
                _readTex.Apply(false);
                Color32[] px = _readTex.GetPixels32();

                // 2. RGBA -> YUV420 planar (I420)
                RgbaToI420(px, VideoWidth, VideoHeight, _yuvBuf);

                // 3. Dequeue an input buffer from the encoder
                int inIdx = _codec.Call<int>("dequeueInputBuffer", (long)10000);
                if (inIdx < 0)
                {
                    // Encoder congested - skip this frame but still drain
                    DrainEncoder(false);
                    yield break;
                }

                // 4. Copy YUV bytes into the encoder's input ByteBuffer via JNI
                var inBuf = _codec.Call<AndroidJavaObject>("getInputBuffer", inIdx);
                inBuf.Call("clear");
                // AndroidJavaObject.Call doesn't have a put(byte[]) overload we can
                // call directly, so we use the raw JNI helper:
                IntPtr   bufPtr   = inBuf.GetRawObject();
                // Use AndroidJNI to call ByteBuffer.put(byte[])
                AndroidJNI.AttachCurrentThread();
                IntPtr putMethod = AndroidJNIHelper.GetMethodID(
                    AndroidJNI.GetObjectClass(bufPtr), "put", "([B)Ljava/nio/ByteBuffer;");
                IntPtr jArray = AndroidJNIHelper.ConvertToJNIArray(_yuvBuf);
                AndroidJNI.CallObjectMethod(bufPtr, putMethod, new jvalue[]
                {
                    new jvalue { l = jArray }
                });
                AndroidJNI.DeleteLocalRef(jArray);

                // 5. Queue the buffer
                _ptsUsec += (long)(1000000L / VideoFps);
                _codec.Call("queueInputBuffer", inIdx, 0, _yuvBuf.Length, _ptsUsec, 0);

                // 6. Drain any available encoded output into the muxer
                DrainEncoder(false);
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("EncodeFrame error: " + ex.Message);
            }
        }

        // RGBA pixels (bottom-left origin from ReadPixels) -> I420 planar YUV
        // I420: Y plane (W*H bytes), then U plane (W/2 * H/2), then V plane (W/2 * H/2)
        // ReadPixels returns rows bottom-to-top, so we flip vertically.
        private static void RgbaToI420(Color32[] px, int w, int h, byte[] yuv)
        {
            int yBase = 0;
            int uBase = w * h;
            int vBase = uBase + (w / 2) * (h / 2);

            for (int row = 0; row < h; row++)
            {
                // Flip: ReadPixels row 0 = bottom of screen
                int srcRow = h - 1 - row;
                for (int col = 0; col < w; col++)
                {
                    Color32 c = px[srcRow * w + col];
                    int R = c.r, G = c.g, B = c.b;

                    // BT.601 limited range
                    int Y = ((66 * R + 129 * G + 25  * B + 128) >> 8) + 16;
                    yuv[yBase + row * w + col] = (byte)(Y < 0 ? 0 : Y > 255 ? 255 : Y);

                    if ((row & 1) == 0 && (col & 1) == 0)
                    {
                        int U = ((-38 * R - 74  * G + 112 * B + 128) >> 8) + 128;
                        int V = ((112 * R - 94  * G - 18  * B + 128) >> 8) + 128;
                        int uvIdx = (row / 2) * (w / 2) + (col / 2);
                        yuv[uBase + uvIdx] = (byte)(U < 0 ? 0 : U > 255 ? 255 : U);
                        yuv[vBase + uvIdx] = (byte)(V < 0 ? 0 : V > 255 ? 255 : V);
                    }
                }
            }
        }

        // ---- Drain encoder output -> muxer ----------------------------------
        private const int INFO_TRY_AGAIN_LATER      = -1;
        private const int INFO_OUTPUT_FORMAT_CHANGED = -2;
        private const int BUFFER_FLAG_CODEC_CONFIG   =  2;
        private const int BUFFER_FLAG_END_OF_STREAM  =  4;

        private void DrainEncoder(bool endOfStream)
        {
            int timeoutUs = endOfStream ? 10000 : 0;
            for (int attempt = 0; attempt < 200; attempt++)
            {
                int idx = _codec.Call<int>("dequeueOutputBuffer", _bufferInfo, (long)timeoutUs);
                if (idx == INFO_TRY_AGAIN_LATER) break;
                if (idx == INFO_OUTPUT_FORMAT_CHANGED)
                {
                    if (_muxerStarted)
                    {
                        RecordingModEntry.Log("DrainEncoder: unexpected format change");
                        break;
                    }
                    var newFmt = _codec.Call<AndroidJavaObject>("getOutputFormat");
                    _videoTrackIdx = _muxer.Call<int>("addTrack", newFmt);
                    _muxer.Call("start");
                    _muxerStarted = true;
                    RecordingModEntry.Log("DrainEncoder: muxer started, track=" + _videoTrackIdx);
                    continue;
                }
                if (idx < 0) break;

                int  flags = _bufferInfo.Get<int>("flags");
                int  size  = _bufferInfo.Get<int>("size");

                if ((flags & BUFFER_FLAG_CODEC_CONFIG) != 0)
                {
                    _codec.Call("releaseOutputBuffer", idx, false);
                    continue;
                }

                if (size > 0 && _muxerStarted)
                {
                    var buf = _codec.Call<AndroidJavaObject>("getOutputBuffer", idx);
                    buf.Call("position", _bufferInfo.Get<int>("offset"));
                    buf.Call("limit",    _bufferInfo.Get<int>("offset") + size);
                    _muxer.Call("writeSampleData", _videoTrackIdx, buf, _bufferInfo);
                }

                _codec.Call("releaseOutputBuffer", idx, false);

                if ((flags & BUFFER_FLAG_END_OF_STREAM) != 0) break;
            }
        }

        // ---------------------------------------------------------------------
        //  Viewer
        // ---------------------------------------------------------------------
        public void OpenViewer()
        {
            float sc = Screen.width / REF_W;
            float vw = REF_W;
            float vh = Screen.height / sc;
            _viewerRect = new Rect(vw * 0.05f, vh * 0.05f, vw * 0.90f, vh * 0.85f);
            RefreshRecordings();
            _viewerOpen = true;
        }

        private void RefreshRecordings()
        {
            _recordings.Clear();
            _statusMsg = null;
            try
            {
                if (!Directory.Exists(RecordingsDir)) return;
                var files = new List<string>(Directory.GetFiles(RecordingsDir, "*.mp4"));
                files.Sort((a, b) => string.Compare(
                    Path.GetFileName(b), Path.GetFileName(a), StringComparison.Ordinal));
                _recordings = files;
            }
            catch (Exception ex)
            {
                _statusMsg = "Error: " + ex.Message;
                RecordingModEntry.Log("RefreshRecordings error: " + ex.Message);
            }
        }

        // ---- IMGUI -----------------------------------------------------------
        private void OnGUI()
        {
            if (!_viewerOpen) return;
            float sc = Screen.width / REF_W;
            GUIUtility.ScaleAroundPivot(new Vector2(sc, sc), Vector2.zero);
            _viewerRect = GUI.Window(0xCEC0, _viewerRect, DrawViewerWindow, "CNR Recordings");
        }

        private void DrawViewerWindow(int id)
        {
            float w    = _viewerRect.width;
            float h    = _viewerRect.height;
            float btnH = 24f;

            GUI.DragWindow(new Rect(0, 0, w - 30, 18));

            if (GUI.Button(new Rect(w - 28, 1, 26, 18), "X"))
            {
                _viewerOpen = false;
                return;
            }

            float y = 24f;

            if (IsCapturing)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(4, y, w - 8, btnH), "* RECORDING  " + Path.GetFileName(_outputPath));
                GUI.color = Color.white;
                y += btnH + 2;
            }

            if (GUI.Button(new Rect(4, y, 80, btnH), "Refresh"))
                RefreshRecordings();
            GUI.Label(new Rect(90, y + 3, w - 94, btnH - 3),
                _recordings.Count + " recording(s) in " + RecordingsDir);
            y += btnH + 4;

            if (_statusMsg != null)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(4, y, w - 8, btnH), _statusMsg);
                GUI.color = Color.white;
                y += btnH + 2;
            }

            float listH = h - y - 4;
            _listScroll = GUI.BeginScrollView(
                new Rect(4, y, w - 8, listH), _listScroll,
                new Rect(0, 0, w - 28, Mathf.Max(listH, _recordings.Count * (btnH + 2))));

            for (int i = 0; i < _recordings.Count; i++)
            {
                string name  = Path.GetFileName(_recordings[i]);
                long   bytes = 0;
                try { bytes = new FileInfo(_recordings[i]).Length; } catch { }
                string label = name + "  (" + (bytes / 1024 / 1024) + " MB)";
                GUI.Label(new Rect(4, i * (btnH + 2), w - 36, btnH), label);
            }

            GUI.EndScrollView();
        }
    }
}
