# PV7 Cocos2d-x compatibility module

Best Park engine version was retrieved directly from the original ARM32
libgame.so through the Dynarmic cocos2dVersion() diagnostic harness.
The returned version is cocos2d-2.1rc0-x-2.1.2.

Matching upstream source: cocos2d/cocos2d-x Git tag
cocos2d-2.1rc0-x-2.1.2, commit 66f9623.
Local checkout: D:/Projects/pv7-cocos2dx-bestpark-source.

Cocos2d-x code is MIT-licensed; preserve the original notices if
upstream code is vendored.

This optional PV7 module currently supplies strict ABI method contracts,
not a complete Cocos JNI runtime. It must not be treated as playable
or full ARM64 compatibility based on registration alone.

Remaining: JNI reference and method dispatch, Java calls and native
callbacks back into the ARM32 guest, rendering, input, audio, assets,
lifecycle and end-to-end first-frame validation.
Never alter the original game's ARM32 libgame.so.
Bitmap bridge note: Android renderer compiles using API 35 but is source-guided, not exact historical layout parity. BitmapPixels validates native-order ARGB_8888 byte count and dimensions. Real ARM32 nativeInitBitmapDC dispatch is not connected; probe remains fail-closed.
