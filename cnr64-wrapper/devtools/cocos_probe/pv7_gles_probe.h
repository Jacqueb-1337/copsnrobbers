#pragma once
// Android diagnostic GLES2 bridge. All returned values and object handles
// are obtained from the actual Android EGL/GLES2 implementation.
#if !defined(__ANDROID__)
#error "This probe header is for the Android first-call diagnostic only"
#endif
#include <EGL/egl.h>
#include <GLES2/gl2.h>
#include <dlfcn.h>
#include <optional>
#include <string>
#include <vector>

class Pv7ProbeGles2 {
public:
    ~Pv7ProbeGles2() {
        if (display_ != EGL_NO_DISPLAY) {
            eglMakeCurrent(display_, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
            if (context_ != EGL_NO_CONTEXT) eglDestroyContext(display_, context_);
            if (surface_ != EGL_NO_SURFACE) eglDestroySurface(display_, surface_);
            eglTerminate(display_);
        }
        if (gles_handle_) dlclose(gles_handle_);
    }

    bool Initialize(const std::string&, std::string& error) {
        if (ready_) return true;
        display_ = eglGetDisplay(EGL_DEFAULT_DISPLAY);
        if (display_ == EGL_NO_DISPLAY || !eglInitialize(display_, nullptr, nullptr)) {
            error = "Android eglInitialize failed"; return false;
        }
        if (!eglBindAPI(EGL_OPENGL_ES_API)) {
            error = "Android eglBindAPI failed"; return false;
        }
        const EGLint attributes[] = {
            EGL_RED_SIZE, 8, EGL_GREEN_SIZE, 8, EGL_BLUE_SIZE, 8,
            EGL_ALPHA_SIZE, 8, EGL_SURFACE_TYPE, EGL_PBUFFER_BIT,
            EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT, EGL_NONE
        };
        EGLConfig config = nullptr;
        EGLint count = 0;
        if (!eglChooseConfig(display_, attributes, &config, 1, &count) || count < 1) {
            error = "Android EGL pbuffer config unavailable"; return false;
        }
        const EGLint surface_attributes[] = { EGL_WIDTH, 480, EGL_HEIGHT, 320, EGL_NONE };
        surface_ = eglCreatePbufferSurface(display_, config, surface_attributes);
        const EGLint context_attributes[] = { EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE };
        context_ = eglCreateContext(display_, config, EGL_NO_CONTEXT, context_attributes);
        if (surface_ == EGL_NO_SURFACE || context_ == EGL_NO_CONTEXT ||
            !eglMakeCurrent(display_, surface_, surface_, context_)) {
            error = "Android eglMakeCurrent failed"; return false;
        }
        gles_handle_ = dlopen("libGLESv2.so", RTLD_NOW | RTLD_LOCAL);
        if (!gles_handle_) {
            error = "Android GLESv2 library unavailable"; return false;
        }
        ready_ = true;
        return true;
    }

    std::optional<std::string> GetString(unsigned pname) const {
        if (!ready_) return std::nullopt;
        const GLubyte* result = glGetString(static_cast<GLenum>(pname));
        if (!result) return std::nullopt;
        return std::string(reinterpret_cast<const char*>(result));
    }
    bool GetIntegerv(unsigned name, int* out) const {
        if (!ready_ || !out) return false;
        glGetIntegerv(static_cast<GLenum>(name), out);
        return true;
    }
    bool GetFloatv(unsigned name, float* out) const {
        if (!ready_ || !out) return false;
        glGetFloatv(static_cast<GLenum>(name), out);
        return true;
    }
    bool GetBooleanv(unsigned name, unsigned char* out) const {
        if (!ready_ || !out) return false;
        glGetBooleanv(static_cast<GLenum>(name), out);
        return true;
    }
    std::optional<unsigned> GetError() const { if (!ready_) return std::nullopt; return glGetError(); }
    void* GetProcedure(const char* name) const {
        if (!ready_ || !name || !gles_handle_) return nullptr;
        if (void* found = dlsym(gles_handle_, name)) return found;
        return reinterpret_cast<void*>(eglGetProcAddress(name));
    }
    unsigned CreateProgram() const { return ready_ ? glCreateProgram() : 0; }
    unsigned CreateShader(unsigned type) const {
        return ready_ ? glCreateShader(static_cast<GLenum>(type)) : 0;
    }
    void ShaderSource(unsigned shader, const std::vector<std::string>& chunks) const {
        if (!ready_) return;
        std::vector<const GLchar*> pointers;
        std::vector<GLint> lengths;
        for (const auto& item : chunks) {
            pointers.push_back(item.c_str());
            lengths.push_back(static_cast<GLint>(item.size()));
        }
        glShaderSource(shader, static_cast<GLsizei>(chunks.size()), pointers.data(), lengths.data());
    }
    void CompileShader(unsigned shader) const { if (ready_) glCompileShader(shader); }
    int GetShaderParameter(unsigned shader, unsigned name) const {
        GLint result = 0;
        if (ready_) glGetShaderiv(shader, name, &result);
        return result;
    }
    void AttachShader(unsigned program, unsigned shader) const {
        if (ready_) glAttachShader(program, shader);
    }
    void LinkProgram(unsigned program) const { if (ready_) glLinkProgram(program); }
    int GetProgramParameter(unsigned program, unsigned name) const {
        GLint result = 0;
        if (ready_) glGetProgramiv(program, name, &result);
        return result;
    }
    void BindAttrib(unsigned program, unsigned index, const std::string& name) const {
        if (ready_) glBindAttribLocation(program, index, name.c_str());
    }
    int GetUniform(unsigned program, const std::string& name) const {
        return ready_ ? glGetUniformLocation(program, name.c_str()) : -1;
    }
    int GetAttrib(unsigned program, const std::string& name) const {
        return ready_ ? glGetAttribLocation(program, name.c_str()) : -1;
    }
    void UseProgram(unsigned program) const { if (ready_) glUseProgram(program); }
    void DeleteShader(unsigned shader) const { if (ready_) glDeleteShader(shader); }
    void Uniform1i(int location, int value) const { if (ready_) glUniform1i(location, value); }
    void UniformMatrix4fv(int location, int count, bool transpose, const float* value) const {
        if (ready_) glUniformMatrix4fv(location, count, transpose ? GL_TRUE : GL_FALSE, value);
    }
    void VertexAttribPointer(unsigned index, int size, unsigned type, bool normalized,
                             int stride, const void* value) const {
        if (ready_) glVertexAttribPointer(index,size,type,normalized?GL_TRUE:GL_FALSE,stride,value);
    }
    void DrawArrays(unsigned mode, int first, int count) const {
        if (ready_) glDrawArrays(mode, first, count);
    }
    // Developer-only offscreen framebuffer snapshot. No guest pointers involved.
    bool ReadFramebuffer(std::vector<std::uint8_t>& rgba, std::string& error) const {
        constexpr int width = 480, height = 320;
        if (!ready_) { error = "EGL context not initialized"; return false; }
        rgba.resize(static_cast<std::size_t>(width) * height * 4u);
        glFinish();
        glPixelStorei(GL_PACK_ALIGNMENT, 1);
        glReadPixels(0, 0, width, height, GL_RGBA, GL_UNSIGNED_BYTE, rgba.data());
        const GLenum status = glGetError();
        if (status != GL_NO_ERROR) {
            error = "glReadPixels failed with GLES error " + std::to_string(status);
            rgba.clear();
            return false;
        }
        return true;
    }

private:
    EGLDisplay display_ = EGL_NO_DISPLAY;
    EGLSurface surface_ = EGL_NO_SURFACE;
    EGLContext context_ = EGL_NO_CONTEXT;
    void* gles_handle_ = nullptr;
    bool ready_ = false;
};