package me.jacqueb.pv7.cocos;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.view.Surface;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.widget.FrameLayout;
import android.widget.TextView;
import android.graphics.Color;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;

/** Local-only Android Activity, not part of the published PV7 wrapper. */
public final class CocosProbeActivity extends Activity {
    private static native int nativeRun(String guestPath, String logPath, Surface surface);
    private static native void nativeCancel();
    private static native void nativeQueueTouch(int phase, int pointer, float x, float y);
    private static final String TAG = "PV7CocosProbe";
    private TextView status;
    private SurfaceView gameSurface;
    private volatile Thread diagnosticWorker;
    private volatile boolean surfaceLive;
    private static volatile CocosProbeActivity active;

    /** Hide status over the game as soon as a real EGL frame is presented. */
    public static void onGuestFirstFrame() {
        CocosProbeActivity owner = active;
        if (owner != null) owner.runOnUiThread(() -> {
            if (owner.status != null) owner.status.setVisibility(View.GONE);
        });
    }

    public static String getGuestLanguage() { return java.util.Locale.getDefault().getLanguage(); }
    /** Original Cocos2dxHelper.getDeviceModel() returns android.os.Build.MODEL. */
    public static String getGuestDeviceModel() { return android.os.Build.MODEL; }
    /** Original regularshow.isKindle checks Build.MANUFACTURER == Amazon. */
    public static boolean isGuestKindle() { return "Amazon".equals(android.os.Build.MANUFACTURER); }
    private static final Object effectsLock = new Object();
    private static android.media.SoundPool effectPool;
    private static final java.util.Map<String, Integer> effectSoundIds =
            new java.util.HashMap<>();
    private static final java.util.Map<Integer, Integer> effectLoaded =
            new java.util.HashMap<>();

    /** Source-compatible original Cocos2dxSound behavior through SoundPool. */
    public static int playGuestEffect(String path, boolean loop, float volume) {
        if (!Float.isFinite(volume)) return -1;
        final String asset;
        try {
            asset = resolveGuestAudioFile(path);
        } catch (java.io.IOException e) {
            Log.w(TAG, "Original guest effect missing: " + path, e);
            return -1; // Same as original preloadEffect failure.
        }
        synchronized (effectsLock) {
            if (effectPool == null) {
                effectPool = new android.media.SoundPool.Builder()
                        .setMaxStreams(16).build();
                effectPool.setOnLoadCompleteListener((pool, id, status) -> {
                    synchronized (effectsLock) {
                        effectLoaded.put(id, status);
                        effectsLock.notifyAll();
                    }
                });
            }
            Integer id = effectSoundIds.get(path);
            if (id == null) {
                int loaded = effectPool.load(asset, 1);
                if (loaded <= 0) {
                    Log.w(TAG, "SoundPool rejected original asset " + path);
                    return -1;
                }
                id = loaded;
                effectSoundIds.put(path, id);
            }
            long deadline = android.os.SystemClock.uptimeMillis() + 5000;
            while (!effectLoaded.containsKey(id)) {
                long remaining = deadline - android.os.SystemClock.uptimeMillis();
                if (remaining <= 0) {
                    Log.w(TAG, "Timed out loading original Cocos sound " + path);
                    return -1;
                }
                try {
                    effectsLock.wait(remaining);
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                    return -1;
                }
            }
            if (effectLoaded.get(id) != 0) return -1;
            float gain = Math.max(0f, Math.min(1f, volume));
            int stream = effectPool.play(id, gain, gain, 1, loop ? -1 : 0, 1f);
            Log.i(TAG, "Original guest effect " + path + " stream=" + stream +
                    " looping=" + loop + " volume=" + gain);
            return stream == 0 ? -1 : stream;
        }
    }

    private static void releaseGuestEffects() {
        synchronized (effectsLock) {
            if (effectPool != null) effectPool.release();
            effectPool = null;
            effectSoundIds.clear();
            effectLoaded.clear();
        }
    }

    private static final Object musicLock = new Object();
    private static android.media.MediaPlayer backgroundPlayer;
    private static String backgroundPath;
    private static float guestMusicVolume = 1.0f;

    /** Original Cocos2dxMusic volume behavior; also updates active playback. */
    public static void setGuestBackgroundMusicVolume(float volume) {
        if (!Float.isFinite(volume)) throw new IllegalArgumentException("Invalid guest volume");
        synchronized (musicLock) {
            guestMusicVolume = Math.max(0.0f, Math.min(1.0f, volume));
            if (backgroundPlayer != null)
                backgroundPlayer.setVolume(guestMusicVolume, guestMusicVolume);
        }
        Log.i(TAG, "Guest background music volume=" + guestMusicVolume);
    }

    /** Resolve only files actually packaged with this diagnostic's original guest. */
    public static String resolveGuestAudioFile(String requested) throws java.io.IOException {
        CocosProbeActivity owner = active;
        if (owner == null || requested == null || requested.isEmpty() ||
            requested.length() > 300 || requested.indexOf('\\') >= 0 ||
            requested.indexOf(':') >= 0 || requested.indexOf('\0') >= 0)
            throw new java.io.IOException("Invalid guest music path");
        if (requested.startsWith("/")) {
            // Absolute paths are permitted only under this app's private directories.
            File absolute = new File(requested).getCanonicalFile();
            String files = owner.getFilesDir().getCanonicalPath() + File.separator;
            String cache = owner.getCacheDir().getCanonicalPath() + File.separator;
            if (!(absolute.getPath().startsWith(files) ||
                  absolute.getPath().startsWith(cache)) || !absolute.isFile())
                throw new java.io.FileNotFoundException("Guest audio path outside private storage");
            return absolute.getAbsolutePath();
        }
        if (java.util.Arrays.asList(requested.split("/")).contains("..") ||
            requested.startsWith("./") || requested.startsWith("//"))
            throw new java.io.IOException("Invalid guest music traversal");
        File obb = new File(expansionPath);
        if (obb.isFile()) {
            try (java.util.zip.ZipFile zip = new java.util.zip.ZipFile(obb)) {
                java.util.zip.ZipEntry entry = zip.getEntry(requested);
                if (entry != null && !entry.isDirectory() &&
                    entry.getSize() <= 32L * 1024L * 1024L) {
                    // Extract to a unique, canonical app-private music path.
                    File root = new File(owner.getCacheDir(), "guest-audio").getCanonicalFile();
                    File dest = new File(root, requested).getCanonicalFile();
                    if (!dest.getPath().startsWith(root.getPath() + File.separator))
                        throw new java.io.IOException("Guest music path escape");
                    if (dest.isFile() && entry.getSize() >= 0 &&
                        dest.length() == entry.getSize()) return dest.getAbsolutePath();
                    if (!dest.getParentFile().isDirectory() && !dest.getParentFile().mkdirs())
                        throw new java.io.IOException("Could not create guest audio folder");
                    File temp = new File(dest.getParentFile(), dest.getName() + ".part");
                    long size = 0;
                    try (java.io.InputStream input = zip.getInputStream(entry);
                         FileOutputStream output = new FileOutputStream(temp)) {
                        byte[] buffer = new byte[32768];
                        int count;
                        while ((count = input.read(buffer)) != -1) {
                            size += count;
                            if (size > 32L * 1024L * 1024L)
                                throw new java.io.IOException("Guest audio asset exceeds limit");
                            output.write(buffer, 0, count);
                        }
                    } catch (Exception e) {
                        temp.delete();
                        throw e;
                    }
                    if (dest.exists() && !dest.delete()) {
                        temp.delete();
                        throw new java.io.IOException("Unable to replace guest audio");
                    }
                    if (!temp.renameTo(dest))
                        throw new java.io.IOException("Unable to stage guest audio");
                    Log.i(TAG, "Guest music staged from original OBB: " + requested);
                    return dest.getAbsolutePath();
                }
            }
        }
        // Original Cocos2dxMusic falls back to APK AssetManager when OBB absent;
        // for this diagnostic, allow APK assets even if OBB is present but lacks a file.
        File root = new File(owner.getCacheDir(), "guest-audio-apk").getCanonicalFile();
        File dest = new File(root, requested).getCanonicalFile();
        if (!dest.getPath().startsWith(root.getPath() + File.separator))
            throw new java.io.IOException("Guest music APK path escape");
        if (!dest.getParentFile().isDirectory() && !dest.getParentFile().mkdirs())
            throw new java.io.IOException("Could not create guest APK audio folder");
        try (java.io.InputStream input = owner.getAssets().open(requested);
             FileOutputStream output = new FileOutputStream(dest)) {
            byte[] buffer = new byte[32768];
            int count;
            long size = 0;
            while ((count = input.read(buffer)) != -1) {
                size += count;
                if (size > 32L * 1024L * 1024L)
                    throw new java.io.IOException("Guest APK music exceeds limit");
                output.write(buffer, 0, count);
            }
        }
        Log.i(TAG, "Guest music staged from original APK: " + requested);
        return dest.getAbsolutePath();
    }

    /** Real Android MediaPlayer bridge for original Cocos2dxHelper.playBackgroundMusic. */
    public static void playGuestBackgroundMusic(String requested, boolean loop)
            throws java.io.IOException {
        String filePath = resolveGuestAudioFile(requested);
        synchronized (musicLock) {
            if (backgroundPlayer != null) {
                backgroundPlayer.release();
                backgroundPlayer = null;
            }
            android.media.MediaPlayer player = new android.media.MediaPlayer();
            try {
                player.setDataSource(filePath);
                player.setAudioStreamType(android.media.AudioManager.STREAM_MUSIC);
                player.setLooping(loop);
                player.prepare();
                player.setVolume(guestMusicVolume, guestMusicVolume);
                player.start();
                backgroundPlayer = player;
                backgroundPath = requested;
            } catch (java.io.IOException | RuntimeException e) {
                player.release();
                throw e;
            }
        }
        Log.i(TAG, "Guest background music PLAYING from original asset: " +
                requested + " looping=" + loop);
    }

    private static void releaseGuestBackgroundMusic() {
        synchronized (musicLock) {
            if (backgroundPlayer != null) {
                backgroundPlayer.release();
                backgroundPlayer = null;
            }
            backgroundPath = null;
        }
    }

    @Override protected void onDestroy() {
        surfaceLive = false;
        nativeCancel();
        releaseGuestBackgroundMusic();
        releaseGuestEffects();
        active = null;
        super.onDestroy();
    }

    /** Mirrors the original Cocos2dxHelper.getBoolForKey() implementation. */
    public static boolean readCocosBoolean(String key, boolean fallback) {
        CocosProbeActivity owner = active;
        if (owner == null) throw new IllegalStateException("Activity context not initialized");
        return owner.getSharedPreferences("Cocos2dxPrefsFile", MODE_PRIVATE)
                .getBoolean(key, fallback);
    }
    /** Mirrors original Cocos2dxHelper.setBoolForKey: synchronous commit. */
    public static void writeCocosBoolean(String key, boolean value) {
        CocosProbeActivity owner = active;
        if (owner == null) throw new IllegalStateException("Activity context not initialized");
        boolean saved = owner.getSharedPreferences("Cocos2dxPrefsFile", MODE_PRIVATE)
                .edit().putBoolean(key, value).commit();
        if (!saved) Log.w(TAG, "Original Cocos boolean preference commit returned false");
    }

    /** Mirrors original setIntegerForKey with a synchronous commit. */
    public static void writeCocosInteger(String key, int value) {
        CocosProbeActivity owner = active;
        if (owner == null) throw new IllegalStateException("Activity not initialized");
        if (key == null) throw new IllegalArgumentException("Null guest preference key");
        if (!owner.getSharedPreferences("Cocos2dxPrefsFile", MODE_PRIVATE)
                .edit().putInt(key, value).commit())
            Log.w(TAG, "Original guest integer preference commit failed");
        Log.i(TAG, "Original guest integer preference persisted key=" + key + " value=" + value);
    }

    public static int readCocosInteger(String key, int fallback) {
        CocosProbeActivity owner=active;
        if (owner==null) throw new IllegalStateException("Activity context not initialized");
        return owner.getSharedPreferences("Cocos2dxPrefsFile", MODE_PRIVATE)
                .getInt(key, fallback);
    }

    public static String readCocosPreference(String key, String fallback) {
        CocosProbeActivity owner=active;
        if (owner==null) throw new IllegalStateException("Activity context not initialized");
        return owner.getSharedPreferences("Cocos2dxPrefsFile", MODE_PRIVATE)
                .getString(key, fallback);
    }
    /** Mirrors original setStringForKey with a synchronous preference commit. */
    public static void writeCocosString(String key, String value) {
        CocosProbeActivity owner = active;
        if (owner == null) throw new IllegalStateException("Activity not initialized");
        if (key == null || value == null) throw new IllegalArgumentException("Null guest preference");
        boolean committed = owner.getSharedPreferences("Cocos2dxPrefsFile", MODE_PRIVATE)
                .edit().putString(key, value).commit();
        if (!committed) Log.w(TAG, "Original guest string preference commit failed");
        Log.i(TAG, "Original guest string preference persisted key=" + key);
    }
    private static volatile long animationIntervalNanos = 16_666_667L;

    public static void setGuestAnimationInterval(double seconds) {
        if (!Double.isFinite(seconds) || seconds <= 0 || seconds > 10) {
            throw new IllegalArgumentException("Invalid Cocos frame interval: " + seconds);
        }
        animationIntervalNanos = (long)(1_000_000_000d * seconds);
        Log.i(TAG, "Original Cocos animation interval ns=" + animationIntervalNanos);
    }

    public static String getGuestWritablePath() { CocosProbeActivity owner=active; if(owner==null) throw new IllegalStateException("Activity not ready"); return owner.getFilesDir().getAbsolutePath(); }
    public static String getGuestApkPath() {
        CocosProbeActivity owner = active;
        if (owner == null) throw new IllegalStateException("Activity not ready");
        return owner.getApplicationInfo().sourceDir;
    }
    public static String resolveGuestObbFile(String virtualPath) throws java.io.IOException {
        CocosProbeActivity owner=active;
        if(owner==null || virtualPath==null || !virtualPath.startsWith("/")) return null;
        final String relative=virtualPath.substring(1);
        if(!(relative.startsWith("sd/") || relative.startsWith("hd/") ||
             relative.startsWith("ipad3/") || relative.startsWith("sounds/")) ||
           relative.contains("\\") || relative.contains(":") ||
           java.util.Arrays.asList(relative.split("/")).contains("..")) return null;
        File expansion=new File(expansionPath);
        if(!expansion.isFile()) return null;
        try(java.util.zip.ZipFile zip=new java.util.zip.ZipFile(expansion)) {
            java.util.zip.ZipEntry entry=zip.getEntry(relative);
            String sourceName=relative;
            if(entry==null) sourceName=relative.substring(relative.indexOf('/')+1);
            if(entry!=null && (entry.isDirectory() || entry.getSize()>16*1024*1024L))
                return null;
            File root=new File(owner.getCacheDir(),"guest-resource-files").getCanonicalFile();
            File output=new File(root,relative).getCanonicalFile();
            if(!output.getPath().startsWith(root.getPath()+File.separator)) return null;
            if(output.isFile() && (entry==null ||
                (entry.getSize()>=0 && output.length()==entry.getSize())))
                return output.getAbsolutePath();
            File parent=output.getParentFile();
            if(!parent.isDirectory()&&!parent.mkdirs()) return null;
            InputStream input;
            try {
                input=entry!=null ? zip.getInputStream(entry) : owner.getAssets().open(sourceName);
            } catch(java.io.FileNotFoundException missing) {
                return null;
            }
            File temp=new File(parent,output.getName()+".part");
            long total=0;
            try(InputStream data=input;FileOutputStream stream=new FileOutputStream(temp)) {
                byte[] buffer=new byte[32768];
                int count;
                while((count=data.read(buffer))!=-1) {
                    total+=count;
                    if(total>16*1024*1024L) throw new java.io.IOException("Resource too large");
                    stream.write(buffer,0,count);
                }
            }
            if(!temp.renameTo(output)) {temp.delete();return null;}
            Log.i(TAG,"Guest resource staged "+virtualPath+
                    (entry==null?" from APK assets":" from OBB"));
            return output.getAbsolutePath();
        }
    }
    private static volatile String expansionPath = "";

    /** Equivalent to the original game version-16 expansion lookup, with
     * the wrapper's own Android OBB directory substituted for guest storage. */
    public static String resolveExpansionPath() {
        return expansionPath;
    }

    @Override
    protected void onCreate(Bundle state) {
        super.onCreate(state);
        active=this;
        // Preserve the original 480:320 aspect ratio on modern wide displays.
        FrameLayout content = new FrameLayout(this) {
            @Override protected void onSizeChanged(int w, int h, int oldW, int oldH) {
                super.onSizeChanged(w, h, oldW, oldH);
                if (gameSurface != null && w > 0 && h > 0) {
                    int width = Math.min(w, Math.round(h * 1.5f));
                    int height = Math.min(h, Math.round(w / 1.5f));
                    gameSurface.setLayoutParams(new FrameLayout.LayoutParams(
                            width, height, Gravity.CENTER));
                }
            }
        };
        content.setBackgroundColor(Color.BLACK);
        gameSurface = new SurfaceView(this);
        gameSurface.setKeepScreenOn(true);
        gameSurface.setOnTouchListener((view, event) -> {
            if (!surfaceLive || view.getWidth() <= 0 || view.getHeight() <= 0) return true;
            int kind = event.getActionMasked();
            int phase;
            if (kind == MotionEvent.ACTION_DOWN || kind == MotionEvent.ACTION_POINTER_DOWN)
                phase = 0;
            else if (kind == MotionEvent.ACTION_UP ||
                     kind == MotionEvent.ACTION_POINTER_UP ||
                     kind == MotionEvent.ACTION_CANCEL)
                phase = 1;
            else return true; // Movement/arrays require a separate verified JNI bridge.
            int index = event.getActionIndex();
            float x = Math.max(0, Math.min(480,
                    event.getX(index) * 480.0f / view.getWidth()));
            float y = Math.max(0, Math.min(320,
                    event.getY(index) * 320.0f / view.getHeight()));
            nativeQueueTouch(phase, event.getPointerId(index), x, y);
            return true;
        });
        gameSurface.getHolder().setFixedSize(480, 320);
        content.addView(gameSurface, new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT, FrameLayout.LayoutParams.MATCH_PARENT));
        status = new TextView(this);
        status.setText("Best Park original ARM32 guest: preparing visible GLES2 surface");
        status.setTextColor(Color.WHITE);
        status.setBackgroundColor(0x99000000);
        status.setTextSize(12);
        status.setPadding(12, 6, 12, 6);
        FrameLayout.LayoutParams label = new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT, FrameLayout.LayoutParams.WRAP_CONTENT,
                Gravity.BOTTOM);
        content.addView(status, label);
        System.loadLibrary("pv7cocosprobe");
        gameSurface.getHolder().addCallback(new SurfaceHolder.Callback() {
            @Override public void surfaceCreated(SurfaceHolder holder) {
                surfaceLive = true;
                if (diagnosticWorker != null && diagnosticWorker.isAlive()) return;
                Surface surface = holder.getSurface();
                if (surface == null || !surface.isValid()) {
                    Log.e(TAG, "Android game surface is not valid");
                    return;
                }
                diagnosticWorker = new Thread(() -> runDiagnostic(surface),
                        "pv7-cocos-onscreen-diagnostic");
                diagnosticWorker.start();
            }
            @Override public void surfaceChanged(SurfaceHolder holder, int format,
                                                  int width, int height) {
                Log.i(TAG, "Android SurfaceView changed width=" + width +
                        " height=" + height);
            }
            @Override public void surfaceDestroyed(SurfaceHolder holder) {
                surfaceLive = false;
                nativeCancel();
            }
        });
        setContentView(content);
    }

    private void runDiagnostic(Surface surface) {
        try {
            File guest = new File(getFilesDir(), "libgame.so");
            try (InputStream input = getAssets().open("guest/libgame.so");
                 FileOutputStream output = new FileOutputStream(guest)) {
                byte[] buffer = new byte[65536];
                int count;
                while ((count = input.read(buffer)) != -1) {
                    output.write(buffer, 0, count);
                }
            }
            File expansion = new File(getObbDir(),
                    "main.16.com.turner.bestparkintheuniverse.obb");
            expansionPath = expansion.isFile() ? expansion.getAbsolutePath() : "";
            Log.i(TAG, "Original version-16 OBB exists=" + expansion.isFile());
            File report = new File(getFilesDir(), "cocos-probe.log");
            if (!surfaceLive || !surface.isValid())
                throw new IllegalStateException("Android game surface was destroyed");
            Log.i(TAG, "Invoking original Best Park ARM32 guest with visible SurfaceView");
            int exit = nativeRun(guest.getAbsolutePath(), report.getAbsolutePath(), surface);
            Log.i(TAG, "Guest diagnostic exited=" + exit + " report=" + report);
            if (exit != 0 && surfaceLive)
                runOnUiThread(() -> {
                    status.setVisibility(View.VISIBLE);
                    status.setText("Cocos diagnostic exit " + exit +
                            "\nReport saved to app-private cocos-probe.log");
                });
        } catch (Throwable t) {
            Log.e(TAG, "Android Cocos diagnostic failed", t);
            runOnUiThread(() -> status.setText("Diagnostic failed: " + t));
        } finally {
            // Never leave this diagnostic app looping guest audio.
            releaseGuestBackgroundMusic();
            releaseGuestEffects();
        }
    }
}