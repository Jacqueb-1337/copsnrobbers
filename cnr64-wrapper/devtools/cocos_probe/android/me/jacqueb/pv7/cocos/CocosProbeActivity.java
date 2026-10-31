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
    private TextView status;    private static volatile CocosProbeActivity active;
    public static String getGuestLanguage() { return java.util.Locale.getDefault().getLanguage(); }

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