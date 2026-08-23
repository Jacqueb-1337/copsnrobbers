package me.jacqueb.cnr64poc;

import android.app.Activity;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.SharedPreferences;
import android.graphics.Typeface;
import android.os.Build;
import android.os.Bundle;
import android.os.Process;
import android.util.Log;
import android.view.Gravity;
import android.view.Surface;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.Arrays;

public final class MainActivity extends Activity {
    static {
        System.loadLibrary("cnr64poc");
    }

    private static native String nativeRunSelfTest(String originalLibMainPath,
                                                   String originalLibUnityPath,
                                                   String originalLibMonoPath,
                                                   String managedDirPath,
                                                   String packageCodePath,
                                                   Surface renderSurface);
    private static native String nativeRunUnityJniStage(String originalLibUnityPath,
                                                        int eventLimit);

    private static final String PREFS = "cnr64-poc";
    private static final String KEY_STAGE = "jni-stage-started";
    private static final String KEY_COMPLETED = "jni-stage-completed";
    private static final String KEY_LAST_SUCCESS = "jni-last-success";

    private TextView output;
    private SurfaceView renderSurface;
    private boolean baselineStarted;
    private File originalLibMain;
    private File originalLibUnity;
    private File originalLibMono;
    private File managedDir;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);

        LinearLayout root = new LinearLayout(this);
        root.setOrientation(LinearLayout.VERTICAL);
        root.setPadding(32, 32, 32, 32);

        TextView title = new TextView(this);
        title.setText("CNR64 Staged Unity JNI PoC");
        title.setTextSize(22f);
        title.setTypeface(Typeface.DEFAULT_BOLD);
        title.setPadding(0, 0, 0, 20);
        root.addView(title, new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MATCH_PARENT,
                LinearLayout.LayoutParams.WRAP_CONTENT));

        renderSurface = new SurfaceView(this);
        int renderHeight = (int) (220f * getResources().getDisplayMetrics().density);
        root.addView(renderSurface, new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MATCH_PARENT,
                renderHeight));
        renderSurface.getHolder().addCallback(new SurfaceHolder.Callback() {
            @Override
            public void surfaceCreated(SurfaceHolder holder) {
                startBaselineOnce();
            }

            @Override
            public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {
            }

            @Override
            public void surfaceDestroyed(SurfaceHolder holder) {
            }
        });

        output = new TextView(this);
        output.setTextSize(14f);
        output.setTypeface(Typeface.MONOSPACE);
        output.setTextIsSelectable(true);

        ScrollView scroll = new ScrollView(this);
        scroll.addView(output, new ScrollView.LayoutParams(
                ScrollView.LayoutParams.MATCH_PARENT,
                ScrollView.LayoutParams.WRAP_CONTENT));
        root.addView(scroll, new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MATCH_PARENT, 0, 1f));

        Button nextStage = new Button(this);
        nextStage.setText("Run Next JNI Checkpoint");
        nextStage.setGravity(Gravity.CENTER);
        nextStage.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                int last = getPreferencesStore().getInt(KEY_LAST_SUCCESS, 0);
                int next = Math.min(last + 1, 7);
                runUnityJniStage(next);
            }
        });
        root.addView(nextStage, fullWidthButtonParams());

        Button fullJni = new Button(this);
        fullJni.setText("Run Full Unity JNI_OnLoad");
        fullJni.setGravity(Gravity.CENTER);
        fullJni.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                runUnityJniStage(0);
            }
        });
        root.addView(fullJni, fullWidthButtonParams());

        Button copy = new Button(this);
        copy.setText("Copy All Results");
        copy.setGravity(Gravity.CENTER);
        copy.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                copyAllResults();
            }
        });
        root.addView(copy, fullWidthButtonParams());

        Button rerun = new Button(this);
        rerun.setText("Run Safe Baseline Again");
        rerun.setGravity(Gravity.CENTER);
        rerun.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                runSafeBaseline();
            }
        });
        root.addView(rerun, fullWidthButtonParams());

        setContentView(root);
        if (renderSurface.getHolder().getSurface().isValid()) {
            startBaselineOnce();
        }

        int requestedStage = getIntent().getIntExtra("jniStage", -1);
        if (requestedStage >= 0) {
            runUnityJniStage(requestedStage);
        }
    }

    private LinearLayout.LayoutParams fullWidthButtonParams() {
        return new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MATCH_PARENT,
                LinearLayout.LayoutParams.WRAP_CONTENT);
    }

    private SharedPreferences getPreferencesStore() {
        return getSharedPreferences(PREFS, MODE_PRIVATE);
    }

    private void startBaselineOnce() {
        if (baselineStarted) return;
        Surface surface = renderSurface.getHolder().getSurface();
        if (surface == null || !surface.isValid()) return;
        baselineStarted = true;
        prepareGuestsAndRunSafeBaseline();
    }

    private File prepareGuestAsset(String assetName, String outputName) throws Exception {
        File out = new File(getFilesDir(), outputName);
        File parent = out.getParentFile();
        if (parent != null && !parent.exists() && !parent.mkdirs()) {
            throw new IllegalStateException("Could not create " + parent);
        }
        try (InputStream input = getAssets().open(assetName);
             FileOutputStream stream = new FileOutputStream(out, false)) {
            byte[] buffer = new byte[64 * 1024];
            int read;
            while ((read = input.read(buffer)) >= 0) {
                if (read > 0) stream.write(buffer, 0, read);
            }
            stream.flush();
        }
        return out;
    }

    private void prepareGuestAssetTree(String assetPath, String outputPath) throws Exception {
        String[] children = getAssets().list(assetPath);
        if (children == null || children.length == 0) {
            prepareGuestAsset(assetPath, outputPath);
            return;
        }

        File dir = new File(getFilesDir(), outputPath);
        if (!dir.exists() && !dir.mkdirs()) {
            throw new IllegalStateException("Could not create " + dir);
        }
        for (String child : children) {
            prepareGuestAssetTree(assetPath + "/" + child, outputPath + "/" + child);
        }
    }

    private File prepareGuestDataDirectory() throws Exception {
        prepareGuestAssetTree("bin/Data", "Data");
        File managed = new File(getFilesDir(), "Data/Managed");
        if (!new File(managed, "mscorlib.dll").isFile()) {
            throw new IllegalStateException("Packaged Unity Data/Managed directory is incomplete");
        }
        return managed;
    }

    private String environmentHeader() {
        StringBuilder text = new StringBuilder();
        text.append("Android ABI(s): ").append(Arrays.toString(Build.SUPPORTED_ABIS)).append('\n');
        text.append("Process.is64Bit(): ").append(Process.is64Bit()).append('\n');
        text.append("Packaged native ABI: arm64-v8a only\n");
        text.append("ARM32 libmain/libunity/libmono are guest assets only.\n\n");
        return text.toString();
    }

    private void prepareGuestsAndRunSafeBaseline() {
        StringBuilder text = new StringBuilder(environmentHeader());
        SharedPreferences prefs = getPreferencesStore();
        int priorStage = prefs.getInt(KEY_STAGE, -1);
        boolean priorCompleted = prefs.getBoolean(KEY_COMPLETED, true);
        if (priorStage >= 0 && !priorCompleted) {
            text.append("IMPORTANT: previous process died/interrupted during Unity JNI ")
                .append(stageLabel(priorStage)).append(".\n")
                .append("That is the current crash boundary.\n\n");
        }
        output.setText(text.toString() + "Preparing original guest binaries...\n");

        try {
            originalLibMain = prepareGuestAsset("guest/libmain.so", "cnr-original-arm32-libmain.so");
            originalLibUnity = prepareGuestAsset("guest/libunity.so", "cnr-original-arm32-libunity.so");
            originalLibMono = prepareGuestAsset("guest/libmono.so", "cnr-original-arm32-libmono.so");
            managedDir = prepareGuestDataDirectory();
            runSafeBaseline();
        } catch (Throwable t) {
            output.setText(text.toString() + "GUEST PREPARATION ERROR\n\n" + t);
        }
    }

    private void runSafeBaseline() {
        if (originalLibMain == null || originalLibUnity == null || originalLibMono == null || managedDir == null) return;
        String prefix = environmentHeader();
        SharedPreferences prefs = getPreferencesStore();
        int priorStage = prefs.getInt(KEY_STAGE, -1);
        boolean priorCompleted = prefs.getBoolean(KEY_COMPLETED, true);
        if (priorStage >= 0 && !priorCompleted) {
            prefix += "Previous process died/interrupted during " + stageLabel(priorStage) + ".\n\n";
        }
        final String baselinePrefix = prefix;
        final Surface surface = renderSurface.getHolder().getSurface();
        if (surface == null || !surface.isValid()) {
            output.setText(baselinePrefix + "Waiting for a valid render Surface...\n");
            return;
        }

        final File hotpatchDir = new File(getFilesDir(), "cnr64-hotpatch");
        if (!hotpatchDir.exists()) hotpatchDir.mkdirs();
        output.setText(baselinePrefix + "Running SAFE baseline on worker thread...\n"
                + "Live compatibility extensions: " + hotpatchDir.getAbsolutePath() + "\n");

        new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    final String result = nativeRunSelfTest(originalLibMain.getAbsolutePath(),
                                                            originalLibUnity.getAbsolutePath(),
                                                            originalLibMono.getAbsolutePath(),
                                                            managedDir.getAbsolutePath(),
                                                            MainActivity.this.getPackageCodePath(),
                                                            surface);
                    Log.i("CNR64POC", "SAFE_BASELINE\n" + result);
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            output.setText(baselinePrefix + result + "\n\nUnity JNI_OnLoad is intentionally NOT auto-run.\n"
                                    + "Use the checkpoint button below.");
                        }
                    });
                } catch (final Throwable t) {
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            output.setText(baselinePrefix + "SAFE NATIVE TEST ERROR\n\n" + t);
                        }
                    });
                }
            }
        }, "CNR64-Harness").start();
    }

    private void runUnityJniStage(int eventLimit) {
        if (originalLibUnity == null) return;

        SharedPreferences prefs = getPreferencesStore();
        prefs.edit()
             .putInt(KEY_STAGE, eventLimit)
             .putBoolean(KEY_COMPLETED, false)
             .commit();

        output.setText(environmentHeader()
                + "Running " + stageLabel(eventLimit) + "...\n\n"
                + "If the process dies, reopen the app; this stage will be remembered.\n");
        Log.i("CNR64POC", "JNI_STAGE_BEGIN=" + eventLimit + " " + stageLabel(eventLimit));

        try {
            String result = nativeRunUnityJniStage(originalLibUnity.getAbsolutePath(), eventLimit);
            boolean passed = result.contains("PASS");
            SharedPreferences.Editor editor = prefs.edit().putBoolean(KEY_COMPLETED, true);
            if (eventLimit > 0 && passed) {
                editor.putInt(KEY_LAST_SUCCESS, eventLimit);
            }
            editor.commit();
            output.setText(environmentHeader() + result + "\n\n"
                    + (passed ? "Checkpoint returned to Java successfully." : "Checkpoint returned with a reported failure."));
            Log.i("CNR64POC", "JNI_STAGE=" + eventLimit + "\n" + result);
        } catch (Throwable t) {
            prefs.edit().putBoolean(KEY_COMPLETED, true).commit();
            output.setText(environmentHeader() + "JNI STAGE JAVA/JNI ERROR\n\n" + t);
        }
    }

    private void copyAllResults() {
        ClipboardManager clipboard = (ClipboardManager) getSystemService(CLIPBOARD_SERVICE);
        if (clipboard == null) return;
        clipboard.setPrimaryClip(ClipData.newPlainText("CNR64 results", output.getText()));
    }

    private String stageLabel(int eventLimit) {
        switch (eventLimit) {
            case 0: return "FULL JNI_OnLoad";
            case 1: return "checkpoint 1: AttachCurrentThread";
            case 2: return "checkpoint 2: first FindClass";
            case 3: return "checkpoint 3: first RegisterNatives";
            case 4: return "checkpoint 4: second FindClass";
            case 5: return "checkpoint 5: second RegisterNatives";
            case 6: return "checkpoint 6: third FindClass";
            case 7: return "checkpoint 7: third RegisterNatives";
            default: return "checkpoint " + eventLimit;
        }
    }
}
