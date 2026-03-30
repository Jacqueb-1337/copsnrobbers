// CNRRecordingMod.cs - hardware-encoded MP4 recording for Cops N Robbers
// v1.3.0 - verbose diagnostic logging + JNI byte-copy fix (NewByteArray/SetByteArrayRegion)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CNRRecordingMod
{
    public static class RecordingModEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/recording.log";
        public  const string Version = "1.17.0";
        private static bool _loaded = false;

        public static void Load()
        {
            if (_loaded) { Log("already loaded, skipping"); return; }
            _loaded = true;

            // Clear log on every boot so file stays fresh
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

    public class RecordingHook : MonoBehaviour
    {
        private const string RecordingsDir    = "/storage/emulated/0/CNRMods/recordings";
        private const int    VideoWidth       = 854;
        private const int    VideoHeight      = 480;
        private const int    VideoBitrate     = 2000000;
        private const int    VideoFps         = 30;
        private const int    COLOR_FMT_YUV420 = 21;           // COLOR_FormatYUV420SemiPlanar (NV12)
        private const float  REF_W            = 600f;

        public  bool IsCapturing { get; private set; }

        private AndroidJavaObject _codec;
        private AndroidJavaObject _muxer;
        private AndroidJavaObject _bufferInfo;
        private int               _videoTrackIdx = -1;
        private long              _ptsUsec       = 0;
        private string            _outputPath;
        private bool              _muxerStarted;

        private Texture2D    _readTex;
        private int           _scrW, _scrH;  // screen dims at capture start (readtex size)
        private byte[]        _yuvBuf;
        private sbyte[]       _sbyteTemp;  // Unity JNI maps Java byte -> C# sbyte
        private int           _encStride;  // actual encoder row stride (may be > VideoWidth due to alignment)
        private int           _encSliceH;  // actual encoder slice height

        // Captures one frame per camera render in OnPostRender (before GLES back-buffer swap).
        // WaitForEndOfFrame fires AFTER the swap so ReadPixels from the coroutine reads undefined
        // (grey/black) content on Android. OnPostRender fires while the rendered frame is still live.
        private class FrameGrabber : MonoBehaviour
        {
            internal Texture2D Tex;
            internal bool      Ready;
            void OnPostRender()
            {
                if (Tex == null) return;
                Tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
                Tex.Apply(false);
                Ready = true;
            }
        }
        private FrameGrabber  _grabber;

        private int        _frameCount;
        private int        _drainLogCount;

        private bool         _viewerOpen;
        private Rect         _viewerRect;
        private List<string> _recordings = new List<string>();
        private Vector2      _listScroll;
        private string       _statusMsg;

        private void Awake()
        {
            RecordingModEntry.Log("RecordingHook.Awake()");
            try
            {
                Directory.CreateDirectory(RecordingsDir);
                RecordingModEntry.Log("  recordings dir OK: " + RecordingsDir);
            }
            catch (Exception ex) { RecordingModEntry.Log("  CreateDirectory error: " + ex.Message); }
            InjectKamcord();
        }

        private void InjectKamcord()
        {
            try
            {
                RecordingModEntry.Log("InjectKamcord: searching Kamcord.implementation_...");
                FieldInfo fi = typeof(Kamcord).GetField("implementation_",
                    BindingFlags.NonPublic | BindingFlags.Static);
                if (fi == null)
                {
                    RecordingModEntry.Log("  WARN: field not found. All static fields on Kamcord:");
                    foreach (var f in typeof(Kamcord).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                        RecordingModEntry.Log("    field: " + f.Name + " type=" + f.FieldType.Name);
                    return;
                }
                RecordingModEntry.Log("  found, type=" + fi.FieldType.Name);
                fi.SetValue(null, new RecordingKamcordImpl(this));
                RecordingModEntry.Log("  injected OK");
            }
            catch (Exception ex) { RecordingModEntry.Log("InjectKamcord EXCEPTION: " + ex); }
        }

        private void OnDestroy()
        {
            RecordingModEntry.Log("RecordingHook.OnDestroy()");
            if (IsCapturing) StopCapture();
            if (_grabber  != null) { Destroy(_grabber);  _grabber  = null; }
            if (_readTex  != null) { Destroy(_readTex);  _readTex  = null; }
        }

        // -----------------------------------------------------------------------
        //  Capture
        // -----------------------------------------------------------------------
        public void StartCapture()
        {
            if (IsCapturing) { RecordingModEntry.Log("StartCapture: already capturing, ignoring"); return; }
            RecordingModEntry.Log("StartCapture BEGIN");
            try
            {
                _outputPath    = Path.Combine(RecordingsDir, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4");
                _ptsUsec       = 0;
                _muxerStarted  = false;
                _videoTrackIdx = -1;
                _frameCount    = 0;
                _drainLogCount = 0;
                RecordingModEntry.Log("  output path: " + _outputPath);

                // CPU buffers — capture full screen, scale down during conversion
                int scrW = Screen.width, scrH = Screen.height;
                if (_readTex != null && (_readTex.width != scrW || _readTex.height != scrH))
                {
                    Destroy(_readTex); _readTex = null;
                }
                if (_readTex == null)
                {
                    _readTex = new Texture2D(scrW, scrH, TextureFormat.RGBA32, false);
                    RecordingModEntry.Log("  Texture2D created " + scrW + "x" + scrH + " (full screen)");
                }
                _scrW = scrW; _scrH = scrH;

                // Attach FrameGrabber to the highest-depth camera that renders to screen.
                // Camera.main (tagged "MainCamera") may not be the last camera — the 3D game
                // scene is often rendered by a camera with a higher depth on top of it.
                // OnPostRender fires per-camera in depth order; attaching to the deepest
                // screen camera gives us the fully-composited frame before the buffer swap.
                if (_grabber != null) { Destroy(_grabber); _grabber = null; }
                Camera bestCam = null;
                foreach (Camera c in Camera.allCameras)
                {
                    if (c.targetTexture != null) continue; // skip render-to-texture cameras
                    bool isKamcord = c.name.IndexOf("kamcord", System.StringComparison.OrdinalIgnoreCase) >= 0;
                    RecordingModEntry.Log("  cam: " + c.name + " depth=" + c.depth + " rt=null tag=" + c.tag + (isKamcord ? " [kamcord,skip]" : ""));
                    if (isKamcord) continue; // Kamcord pre/post cameras clear the framebuffer
                    if (bestCam == null || c.depth > bestCam.depth) bestCam = c;
                }
                if (bestCam == null) bestCam = Camera.main;
                if (bestCam != null)
                {
                    _grabber = bestCam.gameObject.AddComponent<FrameGrabber>();
                    _grabber.Tex = _readTex;
                    RecordingModEntry.Log("  FrameGrabber attached to: " + bestCam.name + " (depth=" + bestCam.depth + ")");
                }
                else RecordingModEntry.Log("  WARNING: no suitable camera found");


                // MediaFormat
                RecordingModEntry.Log("  creating MediaFormat...");
                var fmtClass = new AndroidJavaClass("android.media.MediaFormat");
                var mediaFmt = fmtClass.CallStatic<AndroidJavaObject>("createVideoFormat", "video/avc", VideoWidth, VideoHeight);
                RecordingModEntry.Log("  MediaFormat created, setting params...");
                mediaFmt.Call("setInteger", "bitrate",          VideoBitrate);
                mediaFmt.Call("setInteger", "frame-rate",       VideoFps);
                mediaFmt.Call("setInteger", "i-frame-interval", 2);
                mediaFmt.Call("setInteger", "color-format",     COLOR_FMT_YUV420);
                RecordingModEntry.Log("  MediaFormat params: bitrate=" + VideoBitrate
                    + " fps=" + VideoFps + " colorFmt=0x" + COLOR_FMT_YUV420.ToString("X8"));

                // MediaCodec
                RecordingModEntry.Log("  creating MediaCodec for video/avc...");
                _codec = new AndroidJavaClass("android.media.MediaCodec")
                    .CallStatic<AndroidJavaObject>("createEncoderByType", "video/avc");
                RecordingModEntry.Log("  codec=" + (_codec != null ? "OK" : "NULL!"));

                RecordingModEntry.Log("  codec.configure...");
                _codec.Call("configure", mediaFmt, null, null, 1);
                RecordingModEntry.Log("  codec.configure OK");

                RecordingModEntry.Log("  codec.start...");
                _codec.Call("start");
                RecordingModEntry.Log("  codec.start OK");

                // Query actual encoder input strides. Encoders align row stride to 16
                // (e.g. 864 for 854px). UV must start at encStride*encSliceH, not
                // VideoWidth*VideoHeight — that offset mismatch was causing green frames.
                _encStride = (VideoWidth  + 15) & ~15; // fallback: 16-byte aligned
                _encSliceH = (VideoHeight + 15) & ~15;
                try
                {
                    var inFmt = _codec.Call<AndroidJavaObject>("getInputFormat");
                    int s = inFmt.Call<int>("getInteger", "stride");
                    int h = inFmt.Call<int>("getInteger", "slice-height");
                    _encStride = s; _encSliceH = h;
                    RecordingModEntry.Log("  getInputFormat stride=" + s + " sliceH=" + h);
                    try { int cf = inFmt.Call<int>("getInteger", "color-format"); RecordingModEntry.Log("  getInputFormat color-format=0x" + cf.ToString("X8")); } catch { }
                }
                catch (Exception ex) { RecordingModEntry.Log("  getInputFormat: " + ex.Message + ", using stride=" + _encStride + " sliceH=" + _encSliceH); }

                // Allocate NV12 buffer sized for encoder strides: stride*sliceH (Y) + stride*sliceH/2 (UV)
                int yuvSize = _encStride * _encSliceH * 3 / 2;
                _yuvBuf    = new byte[yuvSize];
                _sbyteTemp = new sbyte[yuvSize];
                RecordingModEntry.Log("  YUV buf: " + yuvSize + " bytes (stride=" + _encStride + " sliceH=" + _encSliceH + ")");

                // MediaMuxer + BufferInfo must be created before the prime loop
                RecordingModEntry.Log("  creating MediaMuxer...");
                _muxer = new AndroidJavaObject("android.media.MediaMuxer", _outputPath, 0);
                RecordingModEntry.Log("  muxer=" + (_muxer != null ? "OK" : "NULL!"));

                _bufferInfo = new AndroidJavaObject("android.media.MediaCodec$BufferInfo");
                RecordingModEntry.Log("  bufferInfo=" + (_bufferInfo != null ? "OK" : "NULL!"));

                // Prime: drain immediately after start to consume FORMAT_CHANGED
                RecordingModEntry.Log("  priming output drain...");
                for (int p = 0; p < 30; p++)
                {
                    int pidx = _codec.Call<int>("dequeueOutputBuffer", _bufferInfo, (long)5000);
                    RecordingModEntry.Log("  prime[" + p + "] idx=" + pidx);
                    if (pidx == INFO_OUTPUT_FORMAT_CHANGED)
                    {
                        var fmt = _codec.Call<AndroidJavaObject>("getOutputFormat");
                        RecordingModEntry.Log("  prime FORMAT_CHANGED: " + fmt.Call<string>("toString"));
                        _videoTrackIdx = _muxer.Call<int>("addTrack", fmt);
                        _muxer.Call("start");
                        _muxerStarted = true;
                        RecordingModEntry.Log("  prime muxer STARTED, track=" + _videoTrackIdx);
                        break;
                    }
                    if (pidx == INFO_TRY_AGAIN_LATER) break;
                    if (pidx >= 0) { _codec.Call("releaseOutputBuffer", pidx, false); }
                }

                IsCapturing = true;
                RecordingModEntry.Log("StartCapture COMPLETE - IsCapturing=true");
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("StartCapture EXCEPTION: " + ex);
                CleanupCodec();
            }
        }

        public void StopCapture()
        {
            if (!IsCapturing) { RecordingModEntry.Log("StopCapture: not capturing"); return; }
            IsCapturing = false;
            RecordingModEntry.Log("StopCapture BEGIN (frames=" + _frameCount + " muxerStarted=" + _muxerStarted + ")");
            try
            {
                int eosIdx = _codec.Call<int>("dequeueInputBuffer", (long)100000);
                RecordingModEntry.Log("  EOS dequeueInputBuffer -> " + eosIdx);
                if (eosIdx >= 0)
                {
                    _codec.Call("queueInputBuffer", eosIdx, 0, 0, _ptsUsec, 4); // BUFFER_FLAG_END_OF_STREAM=4
                    RecordingModEntry.Log("  EOS queueInputBuffer OK");
                }
                DrainEncoder(true);
            }
            catch (Exception ex) { RecordingModEntry.Log("StopCapture error: " + ex); }

            CleanupCodec();

            long finalSize = 0;
            try { if (File.Exists(_outputPath)) finalSize = new FileInfo(_outputPath).Length; } catch { }
            RecordingModEntry.Log("StopCapture DONE, file size=" + finalSize + " bytes");
        }

        private void CleanupCodec()
        {
            RecordingModEntry.Log("CleanupCodec (muxerStarted=" + _muxerStarted + ")");
            try
            {
                if (_muxerStarted && _muxer != null) { _muxer.Call("stop"); RecordingModEntry.Log("  muxer.stop OK"); }
            }
            catch (Exception ex) { RecordingModEntry.Log("  muxer.stop ERROR: " + ex.Message); }
            try
            {
                if (_muxer != null) { _muxer.Call("release"); _muxer.Dispose(); _muxer = null; RecordingModEntry.Log("  muxer released"); }
            }
            catch (Exception ex) { RecordingModEntry.Log("  muxer.release ERROR: " + ex.Message); }
            try
            {
                if (_codec != null) { _codec.Call("stop"); _codec.Call("release"); _codec.Dispose(); _codec = null; RecordingModEntry.Log("  codec released"); }
            }
            catch (Exception ex) { RecordingModEntry.Log("  codec.release ERROR: " + ex.Message); }
            try { if (_bufferInfo != null) { _bufferInfo.Dispose(); _bufferInfo = null; } } catch { }
            if (_grabber != null) { Destroy(_grabber); _grabber = null; }
            _muxerStarted  = false;
            _videoTrackIdx = -1;
        }

        // -----------------------------------------------------------------------
        //  Per-frame encode
        // -----------------------------------------------------------------------
        private void Update()
        {
            if (!IsCapturing) return;
            StartCoroutine(EncodeFrameCoroutine());
        }

        private IEnumerator EncodeFrameCoroutine()
        {
            // Signal grabber to capture this frame, then wait.
            // OnPostRender fires BEFORE WaitForEndOfFrame so pixels are ready when we resume.
            if (_grabber != null) _grabber.Ready = false;
            yield return new WaitForEndOfFrame();
            if (!IsCapturing || _codec == null) yield break;

            bool verbose = (_frameCount < 5) || (_frameCount % 60 == 0);
            if (verbose) RecordingModEntry.Log("Frame " + _frameCount + " (screen=" + Screen.width + "x" + Screen.height + ")");

            try
            {
                // 1. Get pixels captured by FrameGrabber.OnPostRender (fires before buffer swap).
                if (_grabber != null)
                {
                    if (!_grabber.Ready)
                    {
                        if (verbose) RecordingModEntry.Log("  FrameGrabber not ready, skipping frame");
                        yield break;
                    }
                    if (verbose) RecordingModEntry.Log("  FrameGrabber captured OK");
                }
                else
                {
                    // Fallback: ReadPixels here (may read grey on some devices after buffer swap)
                    if (verbose) RecordingModEntry.Log("  ReadPixels fallback (no grabber)");
                    _readTex.ReadPixels(new Rect(0, 0, _scrW, _scrH), 0, 0, false);
                    _readTex.Apply(false);
                }

                Color32[] px = _readTex.GetPixels32();
                if (verbose)
                {
                    Color32 pMid = px[(_scrH / 2) * _scrW + _scrW / 2];
                    RecordingModEntry.Log("  GetPixels32 len=" + px.Length
                        + " px[scrC]=(" + pMid.r + "," + pMid.g + "," + pMid.b + ")");
                }

                // 2. Convert RGBA -> NV12 into _yuvBuf at actual encoder strides.
                // Y at [row*_encStride + col]; UV (interleaved) at [_encStride*_encSliceH + (row/2)*_encStride + col*2].
                // Padding columns (col >= VideoWidth) stay 0 (luma black) or 128 (chroma neutral).
                for (int i = 0; i < _yuvBuf.Length; i++) _yuvBuf[i] = 128;
                int _yBase  = 0;
                int _uvBase = _encStride * _encSliceH;
                for (int row = 0; row < VideoHeight; row++)
                {
                    // nearest-neighbour scale: map output row/col to source pixel
                    int srcRow = ((VideoHeight - 1 - row) * _scrH) / VideoHeight;
                    for (int col = 0; col < VideoWidth; col++)
                    {
                        int srcCol = (col * _scrW) / VideoWidth;
                        Color32 c = px[srcRow * _scrW + srcCol];
                        int R = c.r, G = c.g, B = c.b;
                        int Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
                        _yuvBuf[_yBase + row * _encStride + col] = (byte)(Y < 0 ? 0 : Y > 255 ? 255 : Y);
                        if ((row & 1) == 0 && (col & 1) == 0)
                        {
                            int U = ((-38 * R -  74 * G + 112 * B + 128) >> 8) + 128;
                            int V = ((112 * R -  94 * G -  18 * B + 128) >> 8) + 128;
                            int off = _uvBase + (row / 2) * _encStride + col;
                            _yuvBuf[off]     = (byte)(U < 0 ? 0 : U > 255 ? 255 : U);
                            _yuvBuf[off + 1] = (byte)(V < 0 ? 0 : V > 255 ? 255 : V);
                        }
                    }
                }
                if (verbose) RecordingModEntry.Log("  RgbaToNV12 OK (stride=" + _encStride + ") Y[0]=" + _yuvBuf[0] + " UV[0,1]=(" + _yuvBuf[_encStride * _encSliceH] + "," + _yuvBuf[_encStride * _encSliceH + 1] + ")");

                // 3. Dequeue input buffer
                int inIdx = _codec.Call<int>("dequeueInputBuffer", (long)10000);
                if (verbose) RecordingModEntry.Log("  dequeueInputBuffer=" + inIdx);
                if (inIdx < 0)
                {
                    if (verbose) RecordingModEntry.Log("  encoder congested, draining only");
                    DrainEncoder(false);
                    yield break;
                }

                // 4. Write _yuvBuf into the encoder's native buffer memory.
                //
                // The core problem: Unity 4.6 cannot call ByteBuffer.put(byte[]) through
                // ANY path:
                //   - AndroidJavaObject.Call produces wrong JNI sig "([)Ljava/lang/Object;"
                //     (sbyte[] maps to "[" not "[B") -> NoSuchMethodError
                //   - Raw CallObjectMethod + ConvertToJNIArray -> silent Java exception
                //     (res=null, posAfter=0) every frame
                //
                // Solution: DirectByteBuffer inherits java.nio.Buffer.address (a long field
                // holding the native backing memory pointer). We read it via GetFieldID +
                // GetLongField (JNI ignores Java access modifiers) then Marshal.Copy straight
                // into that memory -- zero ByteBuffer API involved.
                bool bufWritten = false;
                int putLen = _yuvBuf.Length;
                try
                {
                    var inBuf     = _codec.Call<AndroidJavaObject>("getInputBuffer", inIdx);
                    IntPtr rawBuf = inBuf.GetRawObject();

                    IntPtr bufCls  = AndroidJNI.FindClass("java/nio/Buffer");
                    IntPtr midCap  = AndroidJNI.GetMethodID(bufCls, "capacity", "()I");
                    IntPtr fidAddr = AndroidJNI.GetFieldID(bufCls,  "address",  "J");
                    AndroidJNI.DeleteLocalRef(bufCls);

                    int bufCap = AndroidJNI.CallIntMethod(rawBuf, midCap, new jvalue[0]);
                    putLen = bufCap < _yuvBuf.Length ? bufCap : _yuvBuf.Length;

                    long nativeAddr = AndroidJNI.GetLongField(rawBuf, fidAddr);
                    if (verbose) RecordingModEntry.Log("  bufCap=" + bufCap + " nativeAddr=0x" + nativeAddr.ToString("X16"));

                    if (nativeAddr != 0)
                    {
                        Marshal.Copy(_yuvBuf, 0, new IntPtr(nativeAddr), putLen);
                        bufWritten = true;
                        if (verbose) RecordingModEntry.Log("  Marshal.Copy OK (" + putLen + " bytes via Buffer.address)");
                    }
                    else
                    {
                        RecordingModEntry.Log("  Buffer.address=0, not a DirectByteBuffer - no write");
                    }
                }
                catch (Exception ex)
                {
                    RecordingModEntry.Log("  buffer write FAILED: " + ex.Message);
                }

                // 5. Queue the input buffer back
                _ptsUsec += (long)(1000000L / VideoFps);
                _codec.Call("queueInputBuffer", inIdx, 0, bufWritten ? putLen : 0, _ptsUsec, 0);
                if (verbose) RecordingModEntry.Log("  queueInputBuffer OK (pts=" + _ptsUsec + " size=" + putLen + ")");

                // 6. Drain
                int written = DrainEncoder(false);
                if (verbose) RecordingModEntry.Log("  drain wrote=" + written);

                _frameCount++;
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("EncodeFrame[" + _frameCount + "] EXCEPTION: " + ex);
            }
        }



        // -----------------------------------------------------------------------
        //  DrainEncoder - returns number of data buffers written to muxer
        // -----------------------------------------------------------------------
        private const int INFO_TRY_AGAIN_LATER      = -1;
        private const int INFO_OUTPUT_FORMAT_CHANGED = -2;
        private const int BUFFER_FLAG_CODEC_CONFIG   =  2;
        private const int BUFFER_FLAG_END_OF_STREAM  =  4;

        private int DrainEncoder(bool endOfStream)
        {
            bool verbose = (_drainLogCount < 30);
            int timeoutUs = endOfStream ? 10000 : 0;
            int written   = 0;

            for (int attempt = 0; attempt < 300; attempt++)
            {
                int idx = _codec.Call<int>("dequeueOutputBuffer", _bufferInfo, (long)timeoutUs);

                if (verbose)
                {
                    string extra = "";
                    if (idx >= 0) extra = " flags=" + _bufferInfo.Get<int>("flags") + " size=" + _bufferInfo.Get<int>("size");
                    RecordingModEntry.Log("  drain[" + attempt + "] idx=" + idx + extra);
                    _drainLogCount++;
                }

                if (idx == INFO_TRY_AGAIN_LATER) break;

                if (idx == INFO_OUTPUT_FORMAT_CHANGED)
                {
                    if (_muxerStarted) { RecordingModEntry.Log("  drain: unexpected FORMAT_CHANGED (muxer already started)"); break; }
                    var newFmt = _codec.Call<AndroidJavaObject>("getOutputFormat");
                    RecordingModEntry.Log("  FORMAT_CHANGED: " + newFmt.Call<string>("toString"));
                    _videoTrackIdx = _muxer.Call<int>("addTrack", newFmt);
                    _muxer.Call("start");
                    _muxerStarted = true;
                    RecordingModEntry.Log("  muxer STARTED, trackIdx=" + _videoTrackIdx);
                    continue;
                }

                if (idx < 0)
                {
                    RecordingModEntry.Log("  drain: unknown idx=" + idx);
                    break;
                }

                int flags = _bufferInfo.Get<int>("flags");
                int size  = _bufferInfo.Get<int>("size");

                if ((flags & BUFFER_FLAG_CODEC_CONFIG) != 0)
                {
                    if (verbose) RecordingModEntry.Log("  drain: CODEC_CONFIG, skipping");
                    _codec.Call("releaseOutputBuffer", idx, false);
                    continue;
                }

                if (size > 0 && _muxerStarted)
                {
                    var outBuf = _codec.Call<AndroidJavaObject>("getOutputBuffer", idx);
                    // Do NOT call position()/limit() - Unity 4.6 JNI can't resolve inherited Buffer methods.
                    // MediaMuxer.nativeWriteSampleData uses bufferInfo.offset and bufferInfo.size directly.
                    _muxer.Call("writeSampleData", _videoTrackIdx, outBuf, _bufferInfo);
                    written++;
                    if (verbose) RecordingModEntry.Log("  drain: wrote " + size + " bytes to muxer");
                }
                else if (size > 0)
                {
                    RecordingModEntry.Log("  drain: size=" + size + " but muxer NOT started - discarding");
                }

                _codec.Call("releaseOutputBuffer", idx, false);
                if ((flags & BUFFER_FLAG_END_OF_STREAM) != 0) { RecordingModEntry.Log("  drain: EOS"); break; }
            }
            return written;
        }

        // -----------------------------------------------------------------------
        //  Viewer
        // -----------------------------------------------------------------------
        public void OpenViewer()
        {
            float sc = Screen.width / REF_W;
            float vh = Screen.height / sc;
            _viewerRect = new Rect(REF_W * 0.05f, vh * 0.05f, REF_W * 0.90f, vh * 0.85f);
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
            catch (Exception ex) { _statusMsg = "Error: " + ex.Message; }
        }

        private void OnGUI()
        {
            if (!_viewerOpen) return;
            float sc = Screen.width / REF_W;
            GUIUtility.ScaleAroundPivot(new Vector2(sc, sc), Vector2.zero);
            _viewerRect = GUI.Window(0xCEC0, _viewerRect, DrawViewerWindow, "CNR Recordings");
        }

        private void DrawViewerWindow(int id)
        {
            float w = _viewerRect.width, h = _viewerRect.height, btnH = 24f;
            GUI.DragWindow(new Rect(0, 0, w - 30, 18));
            if (GUI.Button(new Rect(w - 28, 1, 26, 18), "X")) { _viewerOpen = false; return; }
            float y = 24f;
            if (IsCapturing)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(4, y, w - 8, btnH), "* RECORDING  " + Path.GetFileName(_outputPath));
                GUI.color = Color.white;
                y += btnH + 2;
            }
            if (GUI.Button(new Rect(4, y, 80, btnH), "Refresh")) RefreshRecordings();
            GUI.Label(new Rect(90, y + 3, w - 94, btnH - 3), _recordings.Count + " recording(s)");
            y += btnH + 4;
            if (_statusMsg != null)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(4, y, w - 8, btnH), _statusMsg);
                GUI.color = Color.white;
                y += btnH + 2;
            }
            float listH = h - y - 4;
            _listScroll = GUI.BeginScrollView(new Rect(4, y, w - 8, listH), _listScroll,
                new Rect(0, 0, w - 28, Mathf.Max(listH, _recordings.Count * (btnH + 2))));
            for (int i = 0; i < _recordings.Count; i++)
            {
                long bytes = 0;
                try { bytes = new FileInfo(_recordings[i]).Length; } catch { }
                GUI.Label(new Rect(4, i * (btnH + 2), w - 36, btnH),
                    Path.GetFileName(_recordings[i]) + "  (" + (bytes / 1024 / 1024) + " MB)");
            }
            GUI.EndScrollView();
        }
    }
}
