# Best Park ARM32/Cocos Dynarmic native execution probe

This standalone **Windows diagnostic harness** runs the original ARMv7 `libgame.so`
instructions under Dynarmic and models enough Android/Bionic/JNI behavior to
identify the next genuine compatibility gap. It does **not** replace, patch,
recompile, or distribute the game library. It is not an installable Android
wrapper and does not yet display gameplay.

## Verified execution

Against the locally retained reference `libgame.so`:

- All 324 static constructors return under Dynarmic.
- `JNI_OnLoad` returns `0x00010004`.
- `nativeInit` enters the Cocos application code.
- Offscreen GLES2 is initialized through the Android emulator's host SwiftShader.
- Actual GLES program and shader objects are created, guest GLSL sources copied
  into host memory with bounds checks, both shader stages compiled successfully,
  attributes bound, and program linking succeeds (`GL_LINK_STATUS=1`).
- The Cocos shader programs initialize through `glUniform1i`, after which the
  next intentionally unimplemented operation is
  `JNI_FindClass:org/cocos2dx/lib/Cocos2dxBitmap`.

Unknown imports/JNI slots deliberately fault with a named diagnostic. Do not
fake success: the next development task is a real JNI/Java-class bridge for
Cocos bitmap/text loading, followed by textures, filesystem, input and frames.

## Build and run (Windows, x64)

Requirements: CMake 3.22+, MSVC 2022 C++ build tools, a Dynarmic checkout
with Boost 1.85.0 in a local third_party directory, and an Android emulator
installation containing `gles_swiftshader/libEGL.dll` and `libGLESv2.dll`.

```powershell
cmake -S . -B build -DPV7_THIRD_PARTY="D:/Projects/copsnrobbers/cnr64-wrapper/third_party"
cmake --build build --config Release --target pv7-cocos-firstcall
& ".\build\Release\pv7-cocos-firstcall.exe" "C:\path\to\your\original\libgame.so" boot
```

The harness picks the host SwiftShader DLL directory from
`%LOCALAPPDATA%\Android\Sdk\emulator\lib64\gles_swiftshader`. Override
with `PV7_GLES_DIR` if the local emulator install differs.

Keep the original game assets and original ELF local. Do not commit them.
The EGL pbuffer is a smoke-test surface, not the Android wrapper's actual
window. Host GLES calls currently run on the single probe thread; production
integration must marshal these calls to the wrapper's EGL/render thread and
correctly manage context loss, resize, graphics-object lifetimes and errors.
