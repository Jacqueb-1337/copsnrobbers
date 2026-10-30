#pragma once
// Development probe only. Owns an offscreen GLES2 context for real GL calls.
// Do not use stub GL object identifiers: the guest must receive host GL handles
// only while this context remains current.
#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <cstdint>
#include <string>
#include <vector>
#include <filesystem>
class Pv7ProbeGles2 {
public:
    ~Pv7ProbeGles2() {
        if (makeCurrent_ && display_) makeCurrent_(display_, nullptr, nullptr, nullptr);
        if (destroyContext_ && context_) destroyContext_(display_, context_);
        if (destroySurface_ && surface_) destroySurface_(display_, surface_);
        if (terminate_ && display_) terminate_(display_);
        if (gles_) FreeLibrary(gles_);
        if (egl_) FreeLibrary(egl_);
    }
    bool Initialize(const std::string& directory, std::string& error) {
        if (ready_) return true;
        const auto root = std::filesystem::path(directory);
        egl_ = LoadLibraryExW((root / L"libEGL.dll").c_str(), nullptr, LOAD_WITH_ALTERED_SEARCH_PATH);
        if (!egl_) { error = "load libEGL.dll failed: " + std::to_string(GetLastError()); return false; }
        gles_ = LoadLibraryExW((root / L"libGLESv2.dll").c_str(), nullptr, LOAD_WITH_ALTERED_SEARCH_PATH);
        if (!gles_) { error = "load libGLESv2.dll failed: " + std::to_string(GetLastError()); return false; }
        auto sym = [&](const char* name) -> FARPROC { return GetProcAddress(egl_, name); };
        auto getDisplay = reinterpret_cast<void* (__stdcall*)(void*)>(sym("eglGetDisplay"));
        auto initialize = reinterpret_cast<unsigned (__stdcall*)(void*, int*, int*)>(sym("eglInitialize"));
        auto bindAPI = reinterpret_cast<unsigned (__stdcall*)(unsigned)>(sym("eglBindAPI"));
        auto chooseConfig = reinterpret_cast<unsigned (__stdcall*)(void*,const int*,void**,int,int*)>(sym("eglChooseConfig"));
        auto createSurface = reinterpret_cast<void* (__stdcall*)(void*,void*,const int*)>(sym("eglCreatePbufferSurface"));
        auto createContext = reinterpret_cast<void* (__stdcall*)(void*,void*,void*,const int*)>(sym("eglCreateContext"));
        makeCurrent_ = reinterpret_cast<unsigned (__stdcall*)(void*,void*,void*,void*)>(sym("eglMakeCurrent"));
        destroyContext_ = reinterpret_cast<unsigned (__stdcall*)(void*,void*)>(sym("eglDestroyContext"));
        destroySurface_ = reinterpret_cast<unsigned (__stdcall*)(void*,void*)>(sym("eglDestroySurface"));
        terminate_ = reinterpret_cast<unsigned (__stdcall*)(void*)>(sym("eglTerminate"));
        createProgram_ = reinterpret_cast<unsigned (__stdcall*)()>(GetProcAddress(gles_, "glCreateProgram"));
        createShader_ = reinterpret_cast<unsigned (__stdcall*)(unsigned)>(GetProcAddress(gles_, "glCreateShader"));
        shaderSource_ = reinterpret_cast<void (__stdcall*)(unsigned,int,const char* const*,const int*)>(GetProcAddress(gles_, "glShaderSource"));
        compileShader_ = reinterpret_cast<void (__stdcall*)(unsigned)>(GetProcAddress(gles_, "glCompileShader"));
        getShaderiv_ = reinterpret_cast<void (__stdcall*)(unsigned,unsigned,int*)>(GetProcAddress(gles_, "glGetShaderiv"));
        attachShader_ = reinterpret_cast<void (__stdcall*)(unsigned,unsigned)>(GetProcAddress(gles_, "glAttachShader"));
        linkProgram_ = reinterpret_cast<void (__stdcall*)(unsigned)>(GetProcAddress(gles_, "glLinkProgram"));
        getProgramiv_ = reinterpret_cast<void (__stdcall*)(unsigned,unsigned,int*)>(GetProcAddress(gles_, "glGetProgramiv"));
        bindAttrib_ = reinterpret_cast<void (__stdcall*)(unsigned,unsigned,const char*)>(GetProcAddress(gles_, "glBindAttribLocation"));
        getUniform_ = reinterpret_cast<int (__stdcall*)(unsigned,const char*)>(GetProcAddress(gles_, "glGetUniformLocation"));
        getAttrib_ = reinterpret_cast<int (__stdcall*)(unsigned,const char*)>(GetProcAddress(gles_, "glGetAttribLocation"));
        useProgram_ = reinterpret_cast<void (__stdcall*)(unsigned)>(GetProcAddress(gles_, "glUseProgram"));
        deleteShader_ = reinterpret_cast<void (__stdcall*)(unsigned)>(GetProcAddress(gles_, "glDeleteShader"));
        uniform1i_ = reinterpret_cast<void (__stdcall*)(int,int)>(GetProcAddress(gles_, "glUniform1i"));
        if (!getDisplay || !initialize || !bindAPI || !chooseConfig || !createSurface ||
            !createContext || !makeCurrent_ || !createProgram_ || !createShader_ || !shaderSource_ || !compileShader_ || !getShaderiv_ || !attachShader_ || !linkProgram_ || !getProgramiv_ || !bindAttrib_ || !getUniform_ || !getAttrib_ || !useProgram_ || !deleteShader_ || !uniform1i_) {
            error = "missing EGL/GLES entrypoint"; return false;
        }
        display_ = getDisplay(nullptr);
        int major = 0, minor = 0;
        if (!display_ || !initialize(display_, &major, &minor)) {
            error = "eglInitialize failed"; return false;
        }
        if (!bindAPI(0x30A0)) { error = "eglBindAPI(EGL_OPENGL_ES_API) failed"; return false; }
        constexpr int configAttribs[] = {0x3024,8,0x3023,8,0x3022,8,0x3021,8,
                                          0x3033,0x0001,0x3040,0x0004,0x3038};
        void* config = nullptr;
        int found = 0;
        if (!chooseConfig(display_, configAttribs, &config, 1, &found) || !found) {
            error = "eglChooseConfig failed"; return false;
        }
        constexpr int surfaceAttribs[] = {0x3057,16,0x3056,16,0x3038};
        surface_ = createSurface(display_, config, surfaceAttribs);
        if (!surface_) { error = "eglCreatePbufferSurface failed"; return false; }
        constexpr int contextAttribs[] = {0x3098,2,0x3038};
        context_ = createContext(display_, config, nullptr, contextAttribs);
        if (!context_) { error = "eglCreateContext(ES2) failed"; return false; }
        if (!makeCurrent_(display_, surface_, surface_, context_)) {
            error = "eglMakeCurrent failed"; return false;
        }
        ready_ = true;
        return true;
    }
    unsigned CreateProgram() const { return ready_ && createProgram_ ? createProgram_() : 0; }
    unsigned CreateShader(unsigned type) const { return ready_ && createShader_ ? createShader_(type) : 0; }
    void ShaderSource(unsigned shader, const std::vector<std::string>& chunks) const {
        std::vector<const char*> pointers; std::vector<int> lengths;
        for (const auto& str : chunks) { pointers.push_back(str.c_str()); lengths.push_back(static_cast<int>(str.size())); }
        shaderSource_(shader, static_cast<int>(chunks.size()), pointers.data(), lengths.data());
    }
    void CompileShader(unsigned shader) const { compileShader_(shader); }
    int GetShaderParameter(unsigned shader, unsigned pname) const { int value=0; getShaderiv_(shader,pname,&value); return value; }
    void AttachShader(unsigned program,unsigned shader) const { attachShader_(program,shader); }
    void LinkProgram(unsigned program) const { linkProgram_(program); }
    int GetProgramParameter(unsigned program,unsigned pname) const { int value=0; getProgramiv_(program,pname,&value); return value; }
    void BindAttrib(unsigned program,unsigned index,const std::string& name) const { bindAttrib_(program,index,name.c_str()); }
    int GetUniform(unsigned program,const std::string& name) const { return getUniform_(program,name.c_str()); }
    int GetAttrib(unsigned program,const std::string& name) const { return getAttrib_(program,name.c_str()); }
    void UseProgram(unsigned program) const { useProgram_(program); }
    void DeleteShader(unsigned shader) const { deleteShader_(shader); }
    void Uniform1i(int location,int value) const { uniform1i_(location,value); }
private:
    HMODULE egl_ = nullptr, gles_ = nullptr;
    void *display_ = nullptr, *surface_ = nullptr, *context_ = nullptr;
    bool ready_ = false;
    unsigned (__stdcall *makeCurrent_)(void*,void*,void*,void*) = nullptr;
    unsigned (__stdcall *destroyContext_)(void*,void*) = nullptr;
    unsigned (__stdcall *destroySurface_)(void*,void*) = nullptr;
    unsigned (__stdcall *terminate_)(void*) = nullptr;
    unsigned (__stdcall *createProgram_)() = nullptr;
    unsigned (__stdcall *createShader_)(unsigned) = nullptr;
    void (__stdcall *shaderSource_)(unsigned,int,const char* const*,const int*) = nullptr;
    void (__stdcall *compileShader_)(unsigned) = nullptr;
    void (__stdcall *getShaderiv_)(unsigned,unsigned,int*) = nullptr;
    void (__stdcall *attachShader_)(unsigned,unsigned) = nullptr;
    void (__stdcall *linkProgram_)(unsigned) = nullptr;
    void (__stdcall *getProgramiv_)(unsigned,unsigned,int*) = nullptr;
    void (__stdcall *bindAttrib_)(unsigned,unsigned,const char*) = nullptr;
    int (__stdcall *getUniform_)(unsigned,const char*) = nullptr;
    int (__stdcall *getAttrib_)(unsigned,const char*) = nullptr;
    void (__stdcall *useProgram_)(unsigned) = nullptr;
    void (__stdcall *deleteShader_)(unsigned) = nullptr;
    void (__stdcall *uniform1i_)(int,int) = nullptr;
};
#endif
