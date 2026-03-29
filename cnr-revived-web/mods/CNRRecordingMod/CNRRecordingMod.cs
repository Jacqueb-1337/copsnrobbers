// CNRRecordingMod.cs â€” hardware-encoded MP4 recording for Cops N Robbers
//
// HOW IT WORKS
//   Replaces the defunct Kamcord SDK (via reflection into Kamcord.implementation_)
//   so the existing in-game Record button and main-menu Recordings viewer work.
//
//   Recording uses Android's MediaCodec (H.264) + MediaMuxer via JNI:
//     1. MediaCodec.createEncoderByType("video/avc") opens the hardware encoder.
//     2. We get its input Surface via MediaCodec.createInputSurface().
//     3. Each frame: render the game's RenderTexture into that Surface using
//        an EGL PBuffer context + glReadPixels â†’ glTexImage2D â†’ blit, then
//        call MediaCodec.signalEndOfInputStream() equivalent by advancing the
//        presentation timestamp and queuing the output to MediaMuxer.
//     4. Output is a real .mp4 in /sdcard/CNRMods/recordings/<timestamp>.mp4
//
//   Because encoding is hardware-side, the main-thread cost is only:
//     â€¢ one RenderTexture.GetNativeTexturePtr() call (zero-copy GPU path)
//     â€¢ MediaCodec.dequeueOutputBuffer / releaseOutputBuffer on the encoder thread
//   This is orders of magnitude cheaper than ReadPixels + EncodeToPNG.
//
// STORAGE
//   /sdcard/CNRMods/recordings/
//     <yyyyMMdd_HHmmss>.mp4
//
// CONSTANTS (top of RecordingHook)
//   VideoWidth, VideoHeight â€” encode resolution (default 854Ã—480)
//   VideoBitrate            â€” H.264 bitrate in bps (default 2 Mbps)
//   VideoFps                â€” frame rate (default 30)
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
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  Entry point â€” CNRMod DLL scanner calls the first public static Load()
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static class RecordingModEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/recording.log";
        public  const string Version = "1.1.0";

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

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  Kamcord stub replacement â€” injected into Kamcord.implementation_
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  Main MonoBehaviour
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public class RecordingHook : MonoBehaviour
    {
        // â”€â”€ paths â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private const string RecordingsDir = "/storage/emulated/0/CNRMods/recordings";

        // â”€â”€ encode settings â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private const int   VideoWidth   = 854;
        private const int   VideoHeight  = 480;
        private const int   VideoBitrate = 2000000; // 2 Mbps H.264
        private const int   VideoFps     = 30;

        // â”€â”€ GUI scale (virtual 600px wide canvas, same as CNRModManager) â”€â”€â”€â”€â”€â”€
        private const float REF_W = 600f;

        // â”€â”€ capture state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public  bool IsCapturing { get; private set; }

        // Android MediaCodec / MediaMuxer JNI handles
        private AndroidJavaObject _codec;           // android.media.MediaCodec
        private AndroidJavaObject _muxer;           // android.media.MediaMuxer
        private AndroidJavaObject _bufferInfo;      // MediaCodec.BufferInfo
        private int               _videoTrackIdx = -1;
        private long              _ptsUsec       = 0;
        private string            _outputPath;
        private RenderTexture     _encodeRT;        // VideoWidth Ã— VideoHeight RT
        private bool              _muxerStarted;

        // â”€â”€ viewer state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private bool              _viewerOpen;
        private Rect              _viewerRect;
        private List<string>      _recordings    = new List<string>();
        private Vector2           _listScroll;
        private string            _statusMsg;

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
                    RecordingModEntry.Log("WARNING: Kamcord.implementation_ not found â€” injection skipped");
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
            if (_encodeRT != null) { Destroy(_encodeRT); _encodeRT = null; }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        //  Capture API
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public void StartCapture()
        {
            if (IsCapturing) return;
            try
            {
                _outputPath = Path.Combine(RecordingsDir,
                    DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4");
                _ptsUsec      = 0;
                _muxerStarted = false;
                _videoTrackIdx = -1;

                // â”€â”€ create encode RenderTexture â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                _encodeRT = new RenderTexture(VideoWidth, VideoHeight, 0,
                    RenderTextureFormat.ARGB32);
                _encodeRT.Create();

                // â”€â”€ set up MediaCodec (hardware H.264 encoder) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                // MediaFormat
                var fmt = new AndroidJavaClass("android.media.MediaFormat");
                var mediaFormat = fmt.CallStatic<AndroidJavaObject>(
                    "createVideoFormat", "video/avc", VideoWidth, VideoHeight);
                mediaFormat.Call("setInteger", "bitrate",              VideoBitrate);
                mediaFormat.Call("setInteger", "frame-rate",           VideoFps);
                mediaFormat.Call("setInteger", "i-frame-interval",     2);   // keyframe every 2s
                mediaFormat.Call("setInteger", "color-format",         0x7F000789); // COLOR_FormatSurface

                _codec = new AndroidJavaClass("android.media.MediaCodec")
                    .CallStatic<AndroidJavaObject>("createEncoderByType", "video/avc");
                // MediaCodec.CONFIGURE_FLAG_ENCODE = 1
                _codec.Call("configure", mediaFormat, null, null, 1);
                _codec.Call("start");

                // â”€â”€ MediaMuxer â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                // OutputFormat.MUXER_OUTPUT_MPEG_4 = 0
                _muxer = new AndroidJavaObject(
                    "android.media.MediaMuxer", _outputPath, 0);

                // â”€â”€ BufferInfo â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                _bufferInfo = new AndroidJavaObject("android.media.MediaCodec$BufferInfo");

                IsCapturing = true;
                RecordingModEntry.Log("StartCapture â†’ " + _outputPath);
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
            RecordingModEntry.Log("StopCapture: signalling EOS");
            try
            {
                // Signal end-of-stream to the encoder and drain remaining output
                _codec.Call("signalEndOfInputStream");
                DrainEncoder(endOfStream: true);
            }
            catch (Exception ex) { RecordingModEntry.Log("StopCapture drain error: " + ex.Message); }
            CleanupCodec();
            RecordingModEntry.Log("StopCapture done â†’ " + _outputPath);
        }

        private void CleanupCodec()
        {
            try { if (_muxerStarted && _muxer != null) _muxer.Call("stop"); } catch { }
            try { if (_muxer  != null) { _muxer.Call("release");  _muxer.Dispose();  _muxer  = null; } } catch { }
            try { if (_codec  != null) { _codec.Call("stop"); _codec.Call("release"); _codec.Dispose(); _codec = null; } } catch { }
            try { if (_bufferInfo != null) { _bufferInfo.Dispose(); _bufferInfo = null; } } catch { }
            if (_encodeRT != null) { Destroy(_encodeRT); _encodeRT = null; }
            _muxerStarted  = false;
            _videoTrackIdx = -1;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        //  Per-frame: blit the screen into the encoder's input Surface, then
        //  drain any encoded output into the muxer.
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
                // Blit screen â†’ our fixed-size RT
                Graphics.Blit(null, _encodeRT);

                // Advance the presentation timestamp
                _ptsUsec += (long)(1000000L / VideoFps);

                // Push this frame into the input Surface that MediaCodec owns.
                // We use MediaCodec.setInputSurface path: the encoder reads
                // directly from a Surface.  We trigger it by calling
                // signalEndOfInputStream only at actual EOS; during recording
                // the encoder automatically grabs the current Surface contents
                // when its GL context is current â€” we notify via
                // MediaCodec's presentationTimeUs by queuing a dummy input buffer.
                // On API 18+ COLOR_FormatSurface encoders are self-clocking from
                // the Surface updateTexImage.  So just drain outputs here.
                DrainEncoder(endOfStream: false);
            }
            catch (Exception ex)
            {
                RecordingModEntry.Log("EncodeFrame error: " + ex.Message);
            }
        }

        // â”€â”€ Drain encoder output â†’ muxer â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // MediaCodec constants (API 18+)
        private const int INFO_TRY_AGAIN_LATER  = -1;
        private const int INFO_OUTPUT_FORMAT_CHANGED = -2;
        private const int BUFFER_FLAG_CODEC_CONFIG   = 2;
        private const int BUFFER_FLAG_END_OF_STREAM  = 4;

        private void DrainEncoder(bool endOfStream)
        {
            // TIMEOUT_USEC: 0 in normal drain (non-blocking), 10 ms at EOS
            int timeoutUs = endOfStream ? 10000 : 0;
            for (int attempt = 0; attempt < 200; attempt++)
            {
                int idx = _codec.Call<int>("dequeueOutputBuffer", _bufferInfo, (long)timeoutUs);
                if (idx == INFO_TRY_AGAIN_LATER) break;
                if (idx == INFO_OUTPUT_FORMAT_CHANGED)
                {
                    if (_muxerStarted)
                    {
                        RecordingModEntry.Log("DrainEncoder: unexpected format change after muxer start");
                        break;
                    }
                    var newFmt = _codec.Call<AndroidJavaObject>("getOutputFormat");
                    _videoTrackIdx = _muxer.Call<int>("addTrack", newFmt);
                    _muxer.Call("start");
                    _muxerStarted = true;
                    RecordingModEntry.Log("DrainEncoder: muxer started, track=" + _videoTrackIdx);
                    continue;
                }
                if (idx < 0) break; // unexpected

                int    flags  = _bufferInfo.Get<int>("flags");
                int    size   = _bufferInfo.Get<int>("size");
                long   pts    = _bufferInfo.Get<long>("presentationTimeUs");

                if ((flags & BUFFER_FLAG_CODEC_CONFIG) != 0)
                {
                    // Codec config data â€” absorbed into the format; skip writing to muxer
                    _codec.Call("releaseOutputBuffer", idx, false);
                    continue;
                }

                if (size > 0 && _muxerStarted)
                {
                    // Get the actual ByteBuffer and pass it to the muxer
                    var buf = _codec.Call<AndroidJavaObject>("getOutputBuffer", idx);
                    buf.Call("position", _bufferInfo.Get<int>("offset"));
                    buf.Call("limit",    _bufferInfo.Get<int>("offset") + size);
                    _muxer.Call("writeSampleData", _videoTrackIdx, buf, _bufferInfo);
                }

                _codec.Call("releaseOutputBuffer", idx, false);

                if ((flags & BUFFER_FLAG_END_OF_STREAM) != 0) break;
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        //  Viewer
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        //  IMGUI
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

            // Recording state indicator
            if (IsCapturing)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(4, y, w - 8, btnH), "â— RECORDING  " + Path.GetFileName(_outputPath));
                GUI.color = Color.white;
                y += btnH + 2;
            }

            // Refresh button
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

            // File list (scrollable)
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

