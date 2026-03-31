# APK Build & Deploy (copsnrobbers)

## Build & Sign
```powershell
cd D:\Projects\copsnrobbers\APK_Build_Active
apktool b apk_source
jarsigner -sigalg SHA256withRSA -digestalg SHA-256 -keystore debug.keystore -storepass android -keypass android "apk_source\dist\target copy.apk" debugkey
# If you get a parse error on install, jarsigner may not be enough (v1 only, no ZIP alignment).
# Try apksigner instead:
# & "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk\build-tools\36.1.0\apksigner.bat" sign --ks debug.keystore --ks-pass pass:android --key-pass pass:android --ks-key-alias debugkey "apk_source\dist\target copy.apk"
```

## Deploy (copy to web repo — version number stays the same, just overwrite)
```powershell
Copy-Item "apk_source\dist\target copy.apk" "..\cnr-revived-web\releases\CopsNRobbers-v2.0.2.apk" -Force
```

## Git push
```powershell
cd D:\Projects\copsnrobbers\cnr-revived-web
git add releases/CopsNRobbers-v2.0.2.apk
git commit -m "<message>"
git push origin master
```

## Notes
- Keystore: `debug.keystore`, alias `debugkey`, storepass/keypass `android`
- APK version stays at v2.0.2 — no bump needed, just overwrite the file
- Patch file: `APK_Build_Active\MainMenuDirector_LoadMods_patch.cs` — must be applied in dnSpy to Assembly-CSharp.dll before building
- Game package: `com.joydo.minestrikenew`
- adb device: `10.182.18.201:38099`
