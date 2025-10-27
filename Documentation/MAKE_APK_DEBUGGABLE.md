# Make APK Debuggable to Check PlayerPrefs

Since both apps have identical code but separate package IDs, their **isolated PlayerPrefs** could have different `CurrentVersion` values set on first launch.

## Quick Fix: Force Both to Same Version

The simplest solution is to make both APKs debuggable and then check/fix the PlayerPrefs:

### Step 1: Decompile APK with apktool

```powershell
cd d:\Projects\copsnrobbers
apktool d target_compiled_final.apk -o apk_debuggable -f
```

### Step 2: Make debuggable - Edit AndroidManifest.xml

Find the `<application>` tag and add `android:debuggable="true"`:

**Before:**
```xml
<application android:label="@string/app_name" android:icon="@drawable/ic_launcher">
```

**After:**
```xml
<application android:label="@string/app_name" android:icon="@drawable/ic_launcher" android:debuggable="true">
```

### Step 3: Recompile and sign

```powershell
apktool b apk_debuggable -o target_debuggable.apk --use-aapt1

jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore debug.keystore `
  -storepass android -keypass android target_debuggable.apk debugkey
```

### Step 4: Modify package name for second app

Edit `apk_debuggable/AndroidManifest.xml` and change package name:

**Before:**
```xml
<manifest ... package="com.joydo.minestrikenew" ...>
```

**After:**
```xml
<manifest ... package="com.joydo.minestrikenex" ...>
```

Then rebuild and sign as `target_debuggable_ex.apk`

### Step 5: Install both debuggable versions

```powershell
adb uninstall com.joydo.minestrikenew
adb uninstall com.joydo.minestrikenex

adb install target_debuggable.apk
adb install target_debuggable_ex.apk
```

### Step 6: Check PlayerPrefs for both

```powershell
# Check version in minestrikenew
adb shell "run-as com.joydo.minestrikenew cat /data/data/com.joydo.minestrikenew/shared_prefs/unity.xml" | Select-String "CurrentVersion"

# Check version in minestrikenex
adb shell "run-as com.joydo.minestrikenex cat /data/data/com.joydo.minestrikenex/shared_prefs/unity.xml" | Select-String "CurrentVersion"
```

### Step 7: If they differ, clear the data

If they have different versions, clear one:

```powershell
adb shell pm clear com.joydo.minestrikenex
```

Then launch it once to reinitialize PlayerPrefs.

## Why This Matters

When you installed the second APK with a different package name, Android created a **separate data directory** with separate PlayerPrefs. On first launch, each app calls:

```csharp
public static string GetStrVersion()
{
    return PlayerPrefs.GetString("CurrentVersion", "v3.0.2");
}
```

If either app had previously run with different data, or if they got different initialization timing, the stored `CurrentVersion` values could differ. This is the most likely explanation.

