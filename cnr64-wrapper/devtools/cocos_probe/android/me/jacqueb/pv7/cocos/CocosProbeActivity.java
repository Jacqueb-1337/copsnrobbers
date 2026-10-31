package me.jacqueb.pv7.cocos;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.widget.TextView;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;

/** Local-only Android Activity, not part of the published PV7 wrapper. */
public final class CocosProbeActivity extends Activity {
    private static native int nativeRun(String guestPath, String logPath);
    private static final String TAG = "PV7CocosProbe";
    private TextView status;
    private static volatile CocosProbeActivity active;
    public static String getGuestLanguage() { return java.util.Locale.getDefault().getLanguage(); }
    /** Original Cocos2dxHelper.getDeviceModel() returns android.os.Build.MODEL. */
    public static String getGuestDeviceModel() { return android.os.Build.MODEL; }
    // Mirrors original Cocos2dxMusic volume state. Audio playback is NOT wired
    // in this developer-only graphics diagnostic.
    private static volatile float guestMusicVolume = 1.0f;
    public static void setGuestBackgroundMusicVolume(float volume) {
        if (!Float.isFinite(volume)) throw new IllegalArgumentException("Invalid guest volume");
        guestMusicVolume = Math.max(0.0f, Math.min(1.0f, volume));
        Log.i(TAG, "Guest background music volume tracked, playback unwired: " + guestMusicVolume);
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
        status = new TextView(this);
        status.setText("Best Park guest ARM32 bitmap diagnostic running...");
        status.setTextSize(18);
        status.setPadding(24, 32, 24, 32);
        setContentView(status);
        new Thread(this::runDiagnostic, "pv7-cocos-diagnostic").start();
    }

    private void runDiagnostic() {
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
            System.loadLibrary("pv7cocosprobe");
            Log.i(TAG, "Invoking original Best Park ARM32 guest through Android JNI");
            int exit = nativeRun(guest.getAbsolutePath(), report.getAbsolutePath());
            Log.i(TAG, "Guest diagnostic exited=" + exit + " report=" + report);
            runOnUiThread(() -> status.setText("Cocos diagnostic exit " + exit
                    + "\nReport saved to app-private cocos-probe.log"));
        } catch (Throwable t) {
            Log.e(TAG, "Android Cocos diagnostic failed", t);
            runOnUiThread(() -> status.setText("Diagnostic failed: " + t));
        }
    }
}