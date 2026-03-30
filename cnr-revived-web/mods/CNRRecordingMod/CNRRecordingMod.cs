// CNRRecordingMod.cs - hardware-encoded MP4 recording for Cops N Robbers
// v1.3.0 - verbose diagnostic logging + JNI byte-copy fix (NewByteArray/SetByteArrayRegion)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRRecordingMod
{
    public static class RecordingModEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/recording.log";
        public  const string Version = "1.7.0";
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
        private const int    COLOR_FMT_YUV420 = 0x7F420888; // COLOR_FormatYUV420Flexible - actual layout determined at runtime via getInputImage()
        private const float  REF_W            = 600f;

        public  bool IsCapturing { get; private set; }

        private AndroidJavaObject _codec;
        private AndroidJavaObject _muxer;
        private AndroidJavaObject _bufferInfo;
        private int               _videoTrackIdx = -1;
        private long              _ptsUsec       = 0;
        private string            _outputPath;
        private bool              _muxerStarted;

        private Texture2D  _readTex;

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
            if (_readTex != null) { Destroy(_readTex); _readTex = null; }
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

                // CPU buffers
                if (_readTex == null)
                {
                    _readTex = new Texture2D(VideoWidth, VideoHeight, TextureFormat.RGBA32, false);
                    RecordingModEntry.Log("  Texture2D created " + VideoWidth + "x" + VideoHeight);
                }


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

                // Prime: drain immediately after start to consume FORMAT_CHANGED.
                // Encoders typically emit this right away. If we miss it here, the
                // muxer can't be started and every frame is discarded.
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

                // MediaMuxer
                RecordingModEntry.Log("  creating MediaMuxer...");
                _muxer = new AndroidJavaObject("android.media.MediaMuxer", _outputPath, 0);
                RecordingModEntry.Log("  muxer=" + (_muxer != null ? "OK" : "NULL!"));

                // BufferInfo
                _bufferInfo = new AndroidJavaObject("android.media.MediaCodec$BufferInfo");
                RecordingModEntry.Log("  bufferInfo=" + (_bufferInfo != null ? "OK" : "NULL!"));

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
            yield return new WaitForEndOfFrame();
            if (!IsCapturing || _codec == null) yield break;

            bool verbose = (_frameCount < 5) || (_frameCount % 60 == 0);
            if (verbose) RecordingModEntry.Log("Frame " + _frameCount + " (screen=" + Screen.width + "x" + Screen.height + ")");

            try
            {
                // 1. ReadPixels into fixed-size texture (captures top-left VideoWidth x VideoHeight of screen)
                _readTex.ReadPixels(new Rect(0, 0, VideoWidth, VideoHeight), 0, 0, false);
                _readTex.Apply(false);
                if (verbose) RecordingModEntry.Log("  ReadPixels OK");

                Color32[] px = _readTex.GetPixels32();
                if (verbose) RecordingModEntry.Log("  GetPixels32 len=" + px.Length);

                // 2. Dequeue input buffer
                int inIdx = _codec.Call<int>("dequeueInputBuffer", (long)10000);
                if (verbose) RecordingModEntry.Log("  dequeueInputBuffer=" + inIdx);
                if (inIdx < 0)
                {
                    if (verbose) RecordingModEntry.Log("  encoder congested, draining only");
                    DrainEncoder(false);
                    yield break;
                }

                // 3. Write pixels via getInputImage() + Image.Plane JNI.
                // This respects the encoder's actual Y/UV row strides, fixing the green-frame
                // bug where UV was written at width*height but the encoder read it at
                // rowStride*height (row stride is typically aligned to 16, e.g. 864 for 854px).
                // NOTE: when using getInputImage, queueInputBuffer size MUST be 0.
                bool bufWritten = false;
                AndroidJavaObject img = null;
                try
                {
                    img = _codec.Call<AndroidJavaObject>("getInputImage", inIdx);
                    if (img == null) throw new Exception("getInputImage returned null");

                    IntPtr imgRaw    = img.GetRawObject();
                    IntPtr imgCls    = AndroidJNI.GetObjectClass(imgRaw);
                    IntPtr midPlanes = AndroidJNI.GetMethodID(imgCls, "getPlanes", "()[Landroid/media/Image$Plane;");
                    IntPtr plArr     = AndroidJNI.CallObjectMethod(imgRaw, midPlanes, new jvalue[0]);
                    AndroidJNI.DeleteLocalRef(imgCls);

                    IntPtr p0 = AndroidJNI.GetObjectArrayElement(plArr, 0); // Y
                    IntPtr p1 = AndroidJNI.GetObjectArrayElement(plArr, 1); // U (or interleaved UV)
                    IntPtr p2 = AndroidJNI.GetObjectArrayElement(plArr, 2); // V
                    AndroidJNI.DeleteLocalRef(plArr);

                    IntPtr planeCls     = AndroidJNI.GetObjectClass(p0);
                    IntPtr midGetBuf    = AndroidJNI.GetMethodID(planeCls, "getBuffer",      "()Ljava/nio/ByteBuffer;");
                    IntPtr midRowStride = AndroidJNI.GetMethodID(planeCls, "getRowStride",   "()I");
                    IntPtr midPixStride = AndroidJNI.GetMethodID(planeCls, "getPixelStride", "()I");
                    AndroidJNI.DeleteLocalRef(planeCls);

                    int yRS  = AndroidJNI.CallIntMethod(p0, midRowStride, new jvalue[0]);
                    int uvRS = AndroidJNI.CallIntMethod(p1, midRowStride, new jvalue[0]);
                    int uvPS = AndroidJNI.CallIntMethod(p1, midPixStride, new jvalue[0]);

                    IntPtr yBuf = AndroidJNI.CallObjectMethod(p0, midGetBuf, new jvalue[0]);
                    IntPtr uBuf = AndroidJNI.CallObjectMethod(p1, midGetBuf, new jvalue[0]);
                    IntPtr vBuf = AndroidJNI.CallObjectMethod(p2, midGetBuf, new jvalue[0]);
                    AndroidJNI.DeleteLocalRef(p0);
                    AndroidJNI.DeleteLocalRef(p1);
                    AndroidJNI.DeleteLocalRef(p2);

                    if (verbose)
                        RecordingModEntry.Log("  Image planes: yRS=" + yRS + " uvRS=" + uvRS + " uvPS=" + uvPS);

                    // ByteBuffer.put([B) found on parent class to avoid Unity 4.6 DirectByteBuffer inheritance bug
                    IntPtr bbCls  = AndroidJNI.FindClass("java/nio/ByteBuffer");
                    IntPtr midPut = AndroidJNI.GetMethodID(bbCls, "put", "([B)Ljava/nio/ByteBuffer;");
                    AndroidJNI.DeleteLocalRef(bbCls);
                    var jArgs = new jvalue[1];

                    // Y plane (respects actual row stride - padding bytes stay zero/16)
                    byte[] yBytes = new byte[yRS * VideoHeight];
                    for (int row = 0; row < VideoHeight; row++)
                    {
                        int srcRow = VideoHeight - 1 - row;
                        for (int col = 0; col < VideoWidth; col++)
                        {
                            Color32 c = px[srcRow * VideoWidth + col];
                            int R = c.r, G = c.g, B = c.b;
                            int Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
                            yBytes[row * yRS + col] = (byte)(Y < 0 ? 0 : Y > 255 ? 255 : Y);
                        }
                    }
                    sbyte[] yS = new sbyte[yBytes.Length];
                    Buffer.BlockCopy(yBytes, 0, yS, 0, yBytes.Length);
                    IntPtr jyArr = AndroidJNIHelper.ConvertToJNIArray(yS);
                    jArgs[0].l = jyArr;
                    AndroidJNI.CallObjectMethod(yBuf, midPut, jArgs);
                    AndroidJNI.DeleteLocalRef(jyArr);
                    AndroidJNI.DeleteLocalRef(yBuf);

                    int uvH = VideoHeight / 2;
                    if (uvPS == 2)
                    {
                        // NV12 or NV21 (interleaved UV): planes[1] buffer covers the whole UV plane
                        byte[] uvBytes = new byte[uvRS * uvH];
                        for (int i = 0; i < uvBytes.Length; i++) uvBytes[i] = 128; // neutral chroma
                        for (int row = 0; row < uvH; row++)
                        {
                            int srcRow = VideoHeight - 1 - row * 2;
                            for (int col = 0; col < VideoWidth / 2; col++)
                            {
                                Color32 c = px[srcRow * VideoWidth + col * 2];
                                int R = c.r, G = c.g, B = c.b;
                                int U = ((-38 * R -  74 * G + 112 * B + 128) >> 8) + 128;
                                int V = ((112 * R -  94 * G -  18 * B + 128) >> 8) + 128;
                                int off = row * uvRS + col * 2;
                                uvBytes[off]     = (byte)(U < 0 ? 0 : U > 255 ? 255 : U);
                                uvBytes[off + 1] = (byte)(V < 0 ? 0 : V > 255 ? 255 : V);
                            }
                        }
                        sbyte[] uvS = new sbyte[uvBytes.Length];
                        Buffer.BlockCopy(uvBytes, 0, uvS, 0, uvBytes.Length);
                        IntPtr juArr = AndroidJNIHelper.ConvertToJNIArray(uvS);
                        jArgs[0].l = juArr;
                        AndroidJNI.CallObjectMethod(uBuf, midPut, jArgs);
                        AndroidJNI.DeleteLocalRef(juArr);
                    }
                    else
                    {
                        // I420 (planar): separate U and V planes
                        byte[] uBytes = new byte[uvRS * uvH];
                        byte[] vBytes = new byte[uvRS * uvH];
                        for (int i = 0; i < uBytes.Length; i++) { uBytes[i] = 128; vBytes[i] = 128; }
                        for (int row = 0; row < uvH; row++)
                        {
                            int srcRow = VideoHeight - 1 - row * 2;
                            for (int col = 0; col < VideoWidth / 2; col++)
                            {
                                Color32 c = px[srcRow * VideoWidth + col * 2];
                                int R = c.r, G = c.g, B = c.b;
                                int U = ((-38 * R -  74 * G + 112 * B + 128) >> 8) + 128;
                                int V = ((112 * R -  94 * G -  18 * B + 128) >> 8) + 128;
                                int off = row * uvRS + col;
                                uBytes[off] = (byte)(U < 0 ? 0 : U > 255 ? 255 : U);
                                vBytes[off] = (byte)(V < 0 ? 0 : V > 255 ? 255 : V);
                            }
                        }
                        sbyte[] uS = new sbyte[uBytes.Length]; Buffer.BlockCopy(uBytes, 0, uS, 0, uBytes.Length);
                        sbyte[] vS = new sbyte[vBytes.Length]; Buffer.BlockCopy(vBytes, 0, vS, 0, vBytes.Length);
                        IntPtr juArr = AndroidJNIHelper.ConvertToJNIArray(uS);
                        jArgs[0].l = juArr;
                        AndroidJNI.CallObjectMethod(uBuf, midPut, jArgs);
                        AndroidJNI.DeleteLocalRef(juArr);
                        IntPtr jvArr = AndroidJNIHelper.ConvertToJNIArray(vS);
                        jArgs[0].l = jvArr;
                        AndroidJNI.CallObjectMethod(vBuf, midPut, jArgs);
                        AndroidJNI.DeleteLocalRef(jvArr);
                    }
                    AndroidJNI.DeleteLocalRef(uBuf);
                    AndroidJNI.DeleteLocalRef(vBuf);

                    bufWritten = true;
                    if (verbose) RecordingModEntry.Log("  WriteImagePlanes OK (yRS=" + yRS + " uvRS=" + uvRS + ")");
                }
                catch (Exception ex)
                {
                    RecordingModEntry.Log("  WriteImagePlanes FAILED: " + ex.Message);
                }
                finally
                {
                    // MUST close Image before queueInputBuffer
                    try { if (img != null) { img.Call("close"); img.Dispose(); img = null; } } catch { }
                }

                // 4. Queue buffer — size=0 required when using getInputImage
                _ptsUsec += (long)(1000000L / VideoFps);
                _codec.Call("queueInputBuffer", inIdx, 0, 0, _ptsUsec, 0);
                if (verbose) RecordingModEntry.Log("  queueInputBuffer OK (pts=" + _ptsUsec + " written=" + bufWritten + ")");

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
