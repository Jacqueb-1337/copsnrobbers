#if defined(__ANDROID__)
    void* GuestGlBytes(std::uint32_t address, std::size_t size = 1) {
        if (address == 0) return nullptr;
        if (!Fits(address, size)) return nullptr;
        return memory_.data() + address;
    }

    template <typename T>
    T* GuestGlArray(std::uint32_t address, std::size_t count = 1) {
        if (address == 0) return nullptr;
        if (count > SIZE_MAX / sizeof(T)) return nullptr;
        return static_cast<T*>(GuestGlBytes(address, count * sizeof(T)));
    }

    const void* GuestGlDataOrOffset(std::uint32_t address, GLenum binding_name) {
        GLint binding = 0;
        glGetIntegerv(binding_name, &binding);
        if (binding != 0) return reinterpret_cast<const void*>(static_cast<std::uintptr_t>(address));
        return GuestGlBytes(address);
    }

    struct GuestGlMappedRange {
        std::uint32_t guest = 0;
        std::uint32_t offset = 0;
        std::uint32_t length = 0;
    };

    template <typename T>
    T HostGlExtension(const char* name) {
        return reinterpret_cast<T>(eglGetProcAddress(name));
    }

    bool DispatchGles(const std::string& name) {
#if PROJECTV7_FAST_SYMBOL_DISPATCH
        // All supported graphics exports belong to the GL or EGL namespace.
        if (name.compare(0, 2, "gl") != 0 && name.compare(0, 3, "egl") != 0) return false;
#endif
        const Pv7DispatchNameKey& fast_name{name};
        if (fast_name == "glDiscardFramebufferEXT") {
            using Proc = void (*)(GLenum, GLsizei, const GLenum*);
            const GLsizei count = static_cast<GLsizei>(Arg(1));
            const GLenum* attachments = count > 0 ? GuestGlArray<GLenum>(Arg(2), static_cast<std::size_t>(count)) : nullptr;
            if (count < 0 || (count > 0 && !attachments)) return false;
            if (const Proc proc = HostGlExtension<Proc>("glDiscardFramebufferEXT")) proc(static_cast<GLenum>(Arg(0)), count, attachments);
            return true;
        }
        if (fast_name == "glMapBufferOES" || fast_name == "glMapBufferRangeEXT") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            GLint host_size = 0;
            glGetBufferParameteriv(target, GL_BUFFER_SIZE, &host_size);
            if (host_size < 0) { Ret(0); return true; }
            const std::uint32_t offset = fast_name == "glMapBufferRangeEXT" ? Arg(1) : 0u;
            const std::uint32_t length = fast_name == "glMapBufferRangeEXT" ? Arg(2) : static_cast<std::uint32_t>(host_size);
            if (offset > static_cast<std::uint32_t>(host_size) || length > static_cast<std::uint32_t>(host_size) - offset) { Ret(0); return true; }
            const std::uint32_t guest = Allocate(length ? length : 1u, 16u);
            if (!guest) { Ret(0); return true; }
            if (length) std::memset(memory_.data() + guest, 0, length);
            gl_mapped_ranges_[static_cast<std::uint32_t>(target)] = {guest, offset, length};
            Ret(guest);
            return true;
        }
        if (fast_name == "glFlushMappedBufferRangeEXT") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            const auto found = gl_mapped_ranges_.find(static_cast<std::uint32_t>(target));
            if (found == gl_mapped_ranges_.end()) return true;
            const std::uint32_t relative = Arg(1), length = Arg(2);
            if (relative > found->second.length || length > found->second.length - relative) return false;
            if (length) glBufferSubData(target, static_cast<GLintptr>(found->second.offset + relative), static_cast<GLsizeiptr>(length), memory_.data() + found->second.guest + relative);
            return true;
        }
        if (fast_name == "glUnmapBufferOES") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            const auto found = gl_mapped_ranges_.find(static_cast<std::uint32_t>(target));
            if (found != gl_mapped_ranges_.end()) {
                if (found->second.length) glBufferSubData(target, static_cast<GLintptr>(found->second.offset), static_cast<GLsizeiptr>(found->second.length), memory_.data() + found->second.guest);
                gl_mapped_ranges_.erase(found);
            }
            Ret(GL_TRUE);
            return true;
        }
        if (fast_name == "glGenQueriesEXT" || fast_name == "glDeleteQueriesEXT") {
            using Proc = void (*)(GLsizei, GLuint*);
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = count > 0 ? GuestGlArray<GLuint>(Arg(1), static_cast<std::size_t>(count)) : nullptr;
            if (count < 0 || (count > 0 && !values)) return false;
            if (const Proc proc = HostGlExtension<Proc>(name.c_str())) proc(count, values);
            else if (fast_name == "glGenQueriesEXT" && values) std::memset(values, 0, static_cast<std::size_t>(count) * sizeof(GLuint));
            return true;
        }
        if (fast_name == "glBeginQueryEXT") {
            using Proc = void (*)(GLenum, GLuint);
            if (const Proc proc = HostGlExtension<Proc>("glBeginQueryEXT")) proc(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1)));
            return true;
        }
        if (fast_name == "glEndQueryEXT") {
            using Proc = void (*)(GLenum);
            if (const Proc proc = HostGlExtension<Proc>("glEndQueryEXT")) proc(static_cast<GLenum>(Arg(0)));
            return true;
        }
        if (fast_name == "glGetQueryObjectuivEXT") {
            using Proc = void (*)(GLuint, GLenum, GLuint*);
            GLuint* value = GuestGlArray<GLuint>(Arg(2));
            if (!value) return false;
            if (const Proc proc = HostGlExtension<Proc>("glGetQueryObjectuivEXT")) proc(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), value);
            else *value = 0;
            return true;
        }
        if (fast_name == "glQueryCounterEXT") {
            using Proc = void (*)(GLuint, GLenum);
            if (const Proc proc = HostGlExtension<Proc>("glQueryCounterEXT")) proc(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)));
            return true;
        }
        if (fast_name == "glGetQueryObjectui64vEXT") {
            using Proc = void (*)(GLuint, GLenum, std::uint64_t*);
            if (!Arg(2) || !Fits(Arg(2), 8u)) return false;
            std::uint64_t value = 0;
            if (const Proc proc = HostGlExtension<Proc>("glGetQueryObjectui64vEXT")) proc(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), &value);
            Write<std::uint64_t>(Arg(2), value);
            return true;
        }
        if (fast_name == "glDrawBuffersNV") {
            using Proc = void (*)(GLsizei, const GLenum*);
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLenum* values = count > 0 ? GuestGlArray<GLenum>(Arg(1), static_cast<std::size_t>(count)) : nullptr;
            if (count < 0 || (count > 0 && !values)) return false;
            if (const Proc proc = HostGlExtension<Proc>("glDrawBuffersNV")) proc(count, values);
            return true;
        }
        if (fast_name == "glRenderbufferStorageMultisampleEXT" || fast_name == "glRenderbufferStorageMultisampleAPPLE") {
            using Proc = void (*)(GLenum, GLsizei, GLenum, GLsizei, GLsizei);
            if (const Proc proc = HostGlExtension<Proc>(name.c_str())) proc(static_cast<GLenum>(Arg(0)), static_cast<GLsizei>(Arg(1)), static_cast<GLenum>(Arg(2)), static_cast<GLsizei>(Arg(3)), static_cast<GLsizei>(Arg(4)));
            return true;
        }
        if (fast_name == "glFramebufferTexture2DMultisampleEXT") {
            using Proc = void (*)(GLenum, GLenum, GLenum, GLuint, GLint, GLsizei);
            if (const Proc proc = HostGlExtension<Proc>("glFramebufferTexture2DMultisampleEXT")) proc(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)), static_cast<GLenum>(Arg(2)), static_cast<GLuint>(Arg(3)), static_cast<GLint>(Arg(4)), static_cast<GLsizei>(Arg(5)));
            return true;
        }
        if (fast_name == "glResolveMultisampleFramebufferAPPLE") {
            using Proc = void (*)();
            if (const Proc proc = HostGlExtension<Proc>("glResolveMultisampleFramebufferAPPLE")) proc();
            return true;
        }
        if (fast_name == "glGetProgramBinaryOES") {
            using Proc = void (*)(GLuint, GLsizei, GLsizei*, GLenum*, void*);
            const GLsizei capacity = static_cast<GLsizei>(Arg(1));
            GLsizei* length = Arg(2) ? GuestGlArray<GLsizei>(Arg(2)) : nullptr;
            GLenum* format = Arg(3) ? GuestGlArray<GLenum>(Arg(3)) : nullptr;
            void* binary = capacity > 0 ? GuestGlBytes(Arg(4), static_cast<std::size_t>(capacity)) : nullptr;
            if (capacity < 0 || (Arg(2) && !length) || (Arg(3) && !format) || (capacity > 0 && !binary)) return false;
            if (const Proc proc = HostGlExtension<Proc>("glGetProgramBinaryOES")) proc(static_cast<GLuint>(Arg(0)), capacity, length, format, binary);
            else { if (length) *length = 0; if (format) *format = 0; }
            return true;
        }
        if (fast_name == "glProgramBinaryOES") {
            using Proc = void (*)(GLuint, GLenum, const void*, GLint);
            const GLint length = static_cast<GLint>(Arg(3));
            const void* binary = length > 0 ? GuestGlBytes(Arg(2), static_cast<std::size_t>(length)) : nullptr;
            if (length < 0 || (length > 0 && !binary)) return false;
            if (const Proc proc = HostGlExtension<Proc>("glProgramBinaryOES")) proc(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), binary, length);
            return true;
        }
        if (fast_name == "glPushGroupMarkerEXT") {
            using Proc = void (*)(GLsizei, const GLchar*);
            const std::string marker = ReadCString(Arg(1), 4096);
            if (const Proc proc = HostGlExtension<Proc>("glPushGroupMarkerEXT")) proc(static_cast<GLsizei>(Arg(0)), marker.c_str());
            return true;
        }
        if (fast_name == "glPopGroupMarkerEXT") {
            using Proc = void (*)();
            if (const Proc proc = HostGlExtension<Proc>("glPopGroupMarkerEXT")) proc();
            return true;
        }

        if (fast_name == "glActiveTexture") { glActiveTexture(static_cast<GLenum>(Arg(0))); return true; }
        if (fast_name == "glAttachShader") { glAttachShader(static_cast<GLuint>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (fast_name == "glBindAttribLocation") {
            const std::string value = ReadCString(Arg(2));
            glBindAttribLocation(static_cast<GLuint>(Arg(0)), static_cast<GLuint>(Arg(1)), value.c_str());
            return true;
        }
        if (fast_name == "glBindBuffer") { glBindBuffer(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (fast_name == "glBindFramebuffer") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            const GLuint requested = static_cast<GLuint>(Arg(1));
            glBindFramebuffer(target, requested);
#if defined(__ANDROID__)
            static int fbo_bind_log_count = 0;
            if (fbo_bind_log_count < 160) {
                GLint actual = -1;
                glGetIntegerv(GL_FRAMEBUFFER_BINDING, &actual);
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7FBO bind target=0x%x requested=%u actual=%d",
                                    target, requested, actual);
                ++fbo_bind_log_count;
            }
#endif
            return true;
        }
        if (fast_name == "glBindRenderbuffer") { glBindRenderbuffer(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (fast_name == "glBindTexture") { glBindTexture(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (fast_name == "glBlendEquationSeparate") {
            glBlendEquationSeparate(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)));
            return true;
        }
        if (fast_name == "glBlendFuncSeparate") {
            glBlendFuncSeparate(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                static_cast<GLenum>(Arg(2)), static_cast<GLenum>(Arg(3)));
            return true;
        }
        if (fast_name == "glBufferData") {
            const GLsizeiptr size = static_cast<GLsizeiptr>(static_cast<std::int32_t>(Arg(1)));
            if (size < 0) return false;
            const std::uint32_t guest_data = Arg(2);
            const void* data = guest_data == 0 ? nullptr : GuestGlBytes(guest_data, static_cast<std::size_t>(size));
            if (guest_data != 0 && !data) return false;
            glBufferData(static_cast<GLenum>(Arg(0)), size, data, static_cast<GLenum>(Arg(3)));
            return true;
        }
        if (fast_name == "glBufferSubData") {
            const GLintptr offset = static_cast<GLintptr>(static_cast<std::int32_t>(Arg(1)));
            const GLsizeiptr size = static_cast<GLsizeiptr>(static_cast<std::int32_t>(Arg(2)));
            if (offset < 0 || size < 0) return false;
            const std::uint32_t guest_data = Arg(3);
            const void* data = guest_data == 0 ? nullptr : GuestGlBytes(guest_data, static_cast<std::size_t>(size));
            if (guest_data != 0 && !data) return false;
            glBufferSubData(static_cast<GLenum>(Arg(0)), offset, size, data);
            return true;
        }
        if (fast_name == "glCheckFramebufferStatus") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            const GLenum status = glCheckFramebufferStatus(target);
#if defined(__ANDROID__)
            static int fbo_status_log_count = 0;
            if (fbo_status_log_count < 80) {
                GLint actual = -1;
                glGetIntegerv(GL_FRAMEBUFFER_BINDING, &actual);
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7FBO status target=0x%x bound=%d status=0x%x",
                                    target, actual, status);
                ++fbo_status_log_count;
            }
#endif
            Ret(static_cast<std::uint32_t>(status));
            return true;
        }
        if (fast_name == "glClear") { glClear(static_cast<GLbitfield>(Arg(0))); return true; }
        if (fast_name == "glClearColor") { glClearColor(ArgFloat(0), ArgFloat(1), ArgFloat(2), ArgFloat(3)); return true; }
        if (fast_name == "glClearDepthf") { glClearDepthf(ArgFloat(0)); return true; }
        if (fast_name == "glClearStencil") { glClearStencil(static_cast<GLint>(Arg(0))); return true; }
        if (fast_name == "glColorMask") {
            glColorMask(static_cast<GLboolean>(Arg(0)), static_cast<GLboolean>(Arg(1)),
                        static_cast<GLboolean>(Arg(2)), static_cast<GLboolean>(Arg(3)));
            return true;
        }
        if (fast_name == "glCompileShader") {
            const GLuint shader = static_cast<GLuint>(Arg(0));
            glCompileShader(shader);

            GLint compiled = GL_FALSE;
            glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
            if (compiled != GL_TRUE) {
                GLint source_length = 0;
                glGetShaderiv(shader, GL_SHADER_SOURCE_LENGTH, &source_length);
                if (source_length > 1 && source_length < (1024 * 1024)) {
                    std::string source(static_cast<std::size_t>(source_length), '\0');
                    GLsizei written = 0;
                    glGetShaderSource(shader, source_length, &written, source.data());
                    if (written > 0) source.resize(static_cast<std::size_t>(written));
                    else source.clear();

                    // Some modern Adreno drivers reject old Unity 4.x generated GLSL
                    // when the final preprocessor line is not newline-terminated.
                    // Retry only shaders that already failed, leaving normal shaders untouched.
                    if (!source.empty() && source.back() != '\n') {
                        source.push_back('\n');
                        const GLchar* retry_source = source.c_str();
                        const GLint retry_length = static_cast<GLint>(source.size());
                        glShaderSource(shader, 1, &retry_source, &retry_length);
                        glCompileShader(shader);
                        glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
#if defined(__ANDROID__)
                        if (compiled == GL_TRUE) {
                            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                "PV7GL shader recovered after newline retry shader=%u source=%d",
                                                shader, retry_length);
                        }
#endif
                    }

#if defined(__ANDROID__)
                    if (compiled != GL_TRUE) {
                        GLint log_length = 0;
                        glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &log_length);
                        std::string info;
                        if (log_length > 1 && log_length < 65536) {
                            info.resize(static_cast<std::size_t>(log_length));
                            GLsizei log_written = 0;
                            glGetShaderInfoLog(shader, log_length, &log_written, info.data());
                            if (log_written > 0) info.resize(static_cast<std::size_t>(log_written));
                        }
                        const std::size_t tail_start = source.size() > 512 ? source.size() - 512 : 0;
                        const std::string tail = source.substr(tail_start);
                        __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                            "PV7GL shader compile failed shader=%u source=%zu log=%s tail=%s",
                                            shader, source.size(), info.c_str(), tail.c_str());
                    }
#endif
                }
            }
            return true;
        }
        if (fast_name == "glCompressedTexImage2D") {
            const GLsizei size = static_cast<GLsizei>(Arg(6));
            if (size < 0) return false;
            const std::uint32_t guest_data = Arg(7);
            const void* data = guest_data == 0 ? nullptr : GuestGlBytes(guest_data, static_cast<std::size_t>(size));
            if (guest_data != 0 && !data) return false;
            glCompressedTexImage2D(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)),
                                   static_cast<GLenum>(Arg(2)), static_cast<GLsizei>(Arg(3)),
                                   static_cast<GLsizei>(Arg(4)), static_cast<GLint>(Arg(5)),
                                   size, data);
            return true;
        }
        if (fast_name == "glCompressedTexSubImage2D") {
            const GLsizei size = static_cast<GLsizei>(Arg(7));
            if (size < 0) return false;
            const std::uint32_t guest_data = Arg(8);
            const void* data = guest_data == 0 ? nullptr : GuestGlBytes(guest_data, static_cast<std::size_t>(size));
            if (guest_data != 0 && !data) return false;
            glCompressedTexSubImage2D(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)),
                                      static_cast<GLint>(Arg(2)), static_cast<GLint>(Arg(3)),
                                      static_cast<GLsizei>(Arg(4)), static_cast<GLsizei>(Arg(5)),
                                      static_cast<GLenum>(Arg(6)), size, data);
            return true;
        }
        if (fast_name == "glCopyTexSubImage2D") {
            glCopyTexSubImage2D(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)),
                                static_cast<GLint>(Arg(2)), static_cast<GLint>(Arg(3)),
                                static_cast<GLint>(Arg(4)), static_cast<GLint>(Arg(5)),
                                static_cast<GLsizei>(Arg(6)), static_cast<GLsizei>(Arg(7)));
            return true;
        }
        if (fast_name == "glCreateProgram") {
            using Proc = GLuint (*)();
            if (const Proc proc = HostGlExtension<Proc>("glCreateProgram")) {
                Ret(static_cast<std::uint32_t>(proc()));
            } else {
                Ret(0);
            }
            return true;
        }
        if (fast_name == "glCreateShader") {
            Ret(static_cast<std::uint32_t>(glCreateShader(static_cast<GLenum>(Arg(0)))));
            return true;
        }
        if (fast_name == "glCullFace") { glCullFace(static_cast<GLenum>(Arg(0))); return true; }
        if (fast_name == "glDeleteBuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glDeleteBuffers(count, values);
            return true;
        }
        if (fast_name == "glDeleteFramebuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
#if defined(__ANDROID__)
            static int fbo_delete_log_count = 0;
            if (fbo_delete_log_count < 80) {
                std::string ids;
                for (GLsizei i = 0; i < count && i < 8; ++i) {
                    if (!ids.empty()) ids += ",";
                    ids += std::to_string(values[i]);
                }
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7FBO delete count=%d ids=%s", count, ids.c_str());
                ++fbo_delete_log_count;
            }
#endif
            glDeleteFramebuffers(count, values);
            return true;
        }
        if (fast_name == "glDeleteProgram") { glDeleteProgram(static_cast<GLuint>(Arg(0))); return true; }
        if (fast_name == "glDeleteRenderbuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glDeleteRenderbuffers(count, values);
            return true;
        }
        if (fast_name == "glDeleteShader") { glDeleteShader(static_cast<GLuint>(Arg(0))); return true; }
        if (fast_name == "glDeleteTextures") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glDeleteTextures(count, values);
            return true;
        }
        if (fast_name == "glDepthFunc") { glDepthFunc(static_cast<GLenum>(Arg(0))); return true; }
        if (fast_name == "glDepthMask") { glDepthMask(static_cast<GLboolean>(Arg(0))); return true; }
        if (fast_name == "glDisable") { glDisable(static_cast<GLenum>(Arg(0))); return true; }
        if (fast_name == "glDisableVertexAttribArray") { glDisableVertexAttribArray(static_cast<GLuint>(Arg(0))); return true; }
        if (fast_name == "glDrawArrays") {
            glDrawArrays(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)), static_cast<GLsizei>(Arg(2)));
            return true;
        }
        if (fast_name == "glDrawElements") {
            const std::uint32_t guest_indices = Arg(3);
            const void* indices = GuestGlDataOrOffset(guest_indices, GL_ELEMENT_ARRAY_BUFFER_BINDING);
            if (guest_indices != 0 && !indices) return false;
            glDrawElements(static_cast<GLenum>(Arg(0)), static_cast<GLsizei>(Arg(1)),
                           static_cast<GLenum>(Arg(2)), indices);
            return true;
        }
        if (fast_name == "glEnable") { glEnable(static_cast<GLenum>(Arg(0))); return true; }
        if (fast_name == "glEnableVertexAttribArray") { glEnableVertexAttribArray(static_cast<GLuint>(Arg(0))); return true; }
        if (fast_name == "glFinish") { glFinish(); return true; }
        if (fast_name == "glFlush") { glFlush(); return true; }
        if (fast_name == "glFramebufferRenderbuffer") {
            glFramebufferRenderbuffer(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                      static_cast<GLenum>(Arg(2)), static_cast<GLuint>(Arg(3)));
            return true;
        }
        if (fast_name == "glFramebufferTexture2D") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            const GLenum attachment = static_cast<GLenum>(Arg(1));
            const GLenum textarget = static_cast<GLenum>(Arg(2));
            const GLuint texture = static_cast<GLuint>(Arg(3));
            const GLint level = static_cast<GLint>(Arg(4));
            glFramebufferTexture2D(target, attachment, textarget, texture, level);
#if defined(__ANDROID__)
            static int fbo_attach_log_count = 0;
            if (fbo_attach_log_count < 100) {
                GLint actual = -1;
                glGetIntegerv(GL_FRAMEBUFFER_BINDING, &actual);
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7FBO attachTex bound=%d attachment=0x%x texTarget=0x%x tex=%u level=%d",
                                    actual, attachment, textarget, texture, level);
                ++fbo_attach_log_count;
            }
#endif
            return true;
        }
        if (fast_name == "glFrontFace") { glFrontFace(static_cast<GLenum>(Arg(0))); return true; }
        if (fast_name == "glGenBuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glGenBuffers(count, values);
            return true;
        }
        if (fast_name == "glGenFramebuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glGenFramebuffers(count, values);
#if defined(__ANDROID__)
            static int fbo_gen_log_count = 0;
            if (fbo_gen_log_count < 80) {
                std::string ids;
                for (GLsizei i = 0; i < count && i < 8; ++i) {
                    if (!ids.empty()) ids += ",";
                    ids += std::to_string(values[i]);
                }
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7FBO gen count=%d ids=%s", count, ids.c_str());
                ++fbo_gen_log_count;
            }
#endif
            return true;
        }
        if (fast_name == "glGenRenderbuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glGenRenderbuffers(count, values);
            return true;
        }
        if (fast_name == "glGenTextures") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glGenTextures(count, values);
            return true;
        }
        if (fast_name == "glGenerateMipmap") { glGenerateMipmap(static_cast<GLenum>(Arg(0))); return true; }
        if (fast_name == "glGetActiveAttrib" || fast_name == "glGetActiveUniform") {
            const GLsizei buffer_size = static_cast<GLsizei>(Arg(2));
            if (buffer_size < 0) return false;
            GLsizei* length = Arg(3) == 0 ? nullptr : GuestGlArray<GLsizei>(Arg(3));
            GLint* size = Arg(4) == 0 ? nullptr : GuestGlArray<GLint>(Arg(4));
            GLenum* type = Arg(5) == 0 ? nullptr : GuestGlArray<GLenum>(Arg(5));
            GLchar* out_name = Arg(6) == 0 ? nullptr : GuestGlArray<GLchar>(Arg(6), static_cast<std::size_t>(buffer_size));
            if ((Arg(3) && !length) || (Arg(4) && !size) || (Arg(5) && !type) ||
                (Arg(6) && buffer_size != 0 && !out_name)) return false;
            if (fast_name == "glGetActiveAttrib") {
                glGetActiveAttrib(static_cast<GLuint>(Arg(0)), static_cast<GLuint>(Arg(1)),
                                  buffer_size, length, size, type, out_name);
            } else {
                glGetActiveUniform(static_cast<GLuint>(Arg(0)), static_cast<GLuint>(Arg(1)),
                                   buffer_size, length, size, type, out_name);
            }
            return true;
        }
        if (fast_name == "glGetAttribLocation" || fast_name == "glGetUniformLocation") {
            const std::string value = ReadCString(Arg(1));
            const GLint result = fast_name == "glGetAttribLocation"
                ? glGetAttribLocation(static_cast<GLuint>(Arg(0)), value.c_str())
                : glGetUniformLocation(static_cast<GLuint>(Arg(0)), value.c_str());
            Ret(static_cast<std::uint32_t>(result));
            return true;
        }
        if (fast_name == "glGetError") { Ret(static_cast<std::uint32_t>(glGetError())); return true; }
        if (fast_name == "glGetFramebufferAttachmentParameteriv") {
            GLint* value = GuestGlArray<GLint>(Arg(3));
            if (!value) return false;
            glGetFramebufferAttachmentParameteriv(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                                  static_cast<GLenum>(Arg(2)), value);
            return true;
        }
        if (fast_name == "glGetIntegerv") {
            GLint* value = GuestGlArray<GLint>(Arg(1), 16);
            if (!value) value = GuestGlArray<GLint>(Arg(1));
            if (!value) return false;
            glGetIntegerv(static_cast<GLenum>(Arg(0)), value);
            return true;
        }
        if (fast_name == "glGetProgramInfoLog" || fast_name == "glGetShaderInfoLog") {
            const GLsizei buffer_size = static_cast<GLsizei>(Arg(1));
            if (buffer_size < 0) return false;
            GLsizei* length = Arg(2) == 0 ? nullptr : GuestGlArray<GLsizei>(Arg(2));
            GLchar* log = Arg(3) == 0 ? nullptr : GuestGlArray<GLchar>(Arg(3), static_cast<std::size_t>(buffer_size));
            if ((Arg(2) && !length) || (Arg(3) && buffer_size != 0 && !log)) return false;
            if (fast_name == "glGetProgramInfoLog") {
                glGetProgramInfoLog(static_cast<GLuint>(Arg(0)), buffer_size, length, log);
            } else {
                glGetShaderInfoLog(static_cast<GLuint>(Arg(0)), buffer_size, length, log);
            }
            return true;
        }
        if (fast_name == "glGetProgramiv" || fast_name == "glGetShaderiv") {
            GLint* value = GuestGlArray<GLint>(Arg(2));
            if (!value) return false;
            if (fast_name == "glGetProgramiv") glGetProgramiv(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), value);
            else glGetShaderiv(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), value);
            return true;
        }
        if (fast_name == "glGetString") {
            const GLenum token = static_cast<GLenum>(Arg(0));
            const auto cached = gl_string_cache_.find(static_cast<std::uint32_t>(token));
            if (cached != gl_string_cache_.end()) {
                Ret(cached->second);
                return true;
            }
            const GLubyte* value = glGetString(token);
            if (!value) {
                Ret(0);
                return true;
            }
            const std::string copy(reinterpret_cast<const char*>(value));
            const std::uint32_t guest = Allocate(copy.size() + 1, 1);
            if (guest == 0 || !WriteCString(guest, copy)) {
                Ret(0);
                return true;
            }
            gl_string_cache_[static_cast<std::uint32_t>(token)] = guest;
            Ret(guest);
            return true;
        }
        if (fast_name == "glGetVertexAttribPointerv") {
            void* pointer = nullptr;
            glGetVertexAttribPointerv(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), &pointer);
            std::uint32_t guest_pointer = static_cast<std::uint32_t>(reinterpret_cast<std::uintptr_t>(pointer));
            const auto* base = memory_.data();
            const auto* bytes = static_cast<const std::uint8_t*>(pointer);
            if (bytes >= base && bytes <= base + memory_.size()) {
                guest_pointer = static_cast<std::uint32_t>(bytes - base);
            }
            std::uint32_t* out = GuestGlArray<std::uint32_t>(Arg(2));
            if (!out) return false;
            *out = guest_pointer;
            return true;
        }
        if (fast_name == "glGetVertexAttribiv") {
            GLint* value = GuestGlArray<GLint>(Arg(2));
            if (!value) return false;
            glGetVertexAttribiv(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), value);
            return true;
        }
        if (fast_name == "glLinkProgram") { glLinkProgram(static_cast<GLuint>(Arg(0))); return true; }
        if (fast_name == "glPixelStorei") { glPixelStorei(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1))); return true; }
        if (fast_name == "glPolygonOffset") { glPolygonOffset(ArgFloat(0), ArgFloat(1)); return true; }
        if (fast_name == "glReadPixels") {
            const std::uint32_t guest_pixels = Arg(6);
            void* pixels = guest_pixels == 0 ? nullptr : GuestGlBytes(guest_pixels);
            if (guest_pixels != 0 && !pixels) return false;
            glReadPixels(static_cast<GLint>(Arg(0)), static_cast<GLint>(Arg(1)),
                         static_cast<GLsizei>(Arg(2)), static_cast<GLsizei>(Arg(3)),
                         static_cast<GLenum>(Arg(4)), static_cast<GLenum>(Arg(5)), pixels);
            return true;
        }
        if (fast_name == "glRenderbufferStorage") {
            glRenderbufferStorage(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                  static_cast<GLsizei>(Arg(2)), static_cast<GLsizei>(Arg(3)));
            return true;
        }
        if (fast_name == "glScissor") {
            GLint x = static_cast<GLint>(Arg(0));
            GLint y = static_cast<GLint>(Arg(1));
            GLsizei width = static_cast<GLsizei>(Arg(2));
            GLsizei height = static_cast<GLsizei>(Arg(3));
#if defined(__ANDROID__)
            GLint framebuffer = 0;
            glGetIntegerv(GL_FRAMEBUFFER_BINDING, &framebuffer);
            if (framebuffer == 0 && x == 0 && y == 0 && width > 0 && height > 0 && host_native_window_) {
                const int host_width = ANativeWindow_getWidth(static_cast<ANativeWindow*>(host_native_window_));
                const int host_height = ANativeWindow_getHeight(static_cast<ANativeWindow*>(host_native_window_));
                if (host_width > 0 && host_height > 0 &&
                    (width != host_width || height != host_height)) {
                    width = static_cast<GLsizei>(host_width);
                    height = static_cast<GLsizei>(host_height);
                }
            }
#endif
            glScissor(x, y, width, height);
            return true;
        }
        if (fast_name == "glShaderSource") {
            const GLsizei count = static_cast<GLsizei>(Arg(1));
            if (count < 0) return false;
            const std::uint32_t* guest_strings = GuestGlArray<std::uint32_t>(
                Arg(2), static_cast<std::size_t>(count));
            const GLint* lengths = Arg(3) == 0 ? nullptr : GuestGlArray<GLint>(
                Arg(3), static_cast<std::size_t>(count));
            if ((count != 0 && !guest_strings) || (Arg(3) != 0 && !lengths)) return false;
            std::vector<std::string> owned;
            std::vector<const GLchar*> strings;
            owned.reserve(static_cast<std::size_t>(count));
            strings.reserve(static_cast<std::size_t>(count));
            for (GLsizei index = 0; index < count; ++index) {
                const std::uint32_t guest_string = guest_strings[index];
                if (!Fits(guest_string, 1)) return false;
                if (lengths && lengths[index] >= 0) {
                    const std::size_t length = static_cast<std::size_t>(lengths[index]);
                    if (!Fits(guest_string, length)) return false;
                    owned.emplace_back(reinterpret_cast<const char*>(memory_.data() + guest_string), length);
                } else {
                    owned.emplace_back(ReadCString(guest_string, 1024 * 1024));
                }
            }
            for (const auto& value : owned) strings.push_back(value.c_str());
            glShaderSource(static_cast<GLuint>(Arg(0)), count, strings.data(), lengths);
            return true;
        }
        if (fast_name == "glStencilFunc") {
            glStencilFunc(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)), static_cast<GLuint>(Arg(2)));
            return true;
        }
        if (fast_name == "glStencilFuncSeparate") {
            glStencilFuncSeparate(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                  static_cast<GLint>(Arg(2)), static_cast<GLuint>(Arg(3)));
            return true;
        }
        if (fast_name == "glStencilMask") { glStencilMask(static_cast<GLuint>(Arg(0))); return true; }
        if (fast_name == "glStencilOp") {
            glStencilOp(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)), static_cast<GLenum>(Arg(2)));
            return true;
        }
        if (fast_name == "glStencilOpSeparate") {
            glStencilOpSeparate(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                static_cast<GLenum>(Arg(2)), static_cast<GLenum>(Arg(3)));
            return true;
        }
        if (fast_name == "glTexImage2D") {
            const std::uint32_t guest_pixels = Arg(8);
            const void* pixels = guest_pixels == 0 ? nullptr : GuestGlBytes(guest_pixels);
            if (guest_pixels != 0 && !pixels) return false;
            glTexImage2D(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)),
                         static_cast<GLint>(Arg(2)), static_cast<GLsizei>(Arg(3)),
                         static_cast<GLsizei>(Arg(4)), static_cast<GLint>(Arg(5)),
                         static_cast<GLenum>(Arg(6)), static_cast<GLenum>(Arg(7)), pixels);
            return true;
        }
        if (fast_name == "glTexParameteri") {
            glTexParameteri(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)), static_cast<GLint>(Arg(2)));
            return true;
        }
        if (fast_name == "glTexSubImage2D") {
            const std::uint32_t guest_pixels = Arg(8);
            const void* pixels = guest_pixels == 0 ? nullptr : GuestGlBytes(guest_pixels);
            if (guest_pixels != 0 && !pixels) return false;
            glTexSubImage2D(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)),
                            static_cast<GLint>(Arg(2)), static_cast<GLint>(Arg(3)),
                            static_cast<GLsizei>(Arg(4)), static_cast<GLsizei>(Arg(5)),
                            static_cast<GLenum>(Arg(6)), static_cast<GLenum>(Arg(7)), pixels);
            return true;
        }
        if (fast_name == "glUniform1fv" || fast_name == "glUniform1iv" ||
            fast_name == "glUniform2fv" || fast_name == "glUniform2iv" ||
            fast_name == "glUniform3fv" || fast_name == "glUniform3iv" ||
            fast_name == "glUniform4fv" || fast_name == "glUniform4iv") {
            const GLsizei count = static_cast<GLsizei>(Arg(1));
            if (count < 0) return false;
            const int components = name[9] - '0';
            const std::size_t values = static_cast<std::size_t>(count) * static_cast<std::size_t>(components);
            if (name.back() == 'v' && name[name.size() - 2] == 'f') {
                const GLfloat* data = GuestGlArray<GLfloat>(Arg(2), values);
                if (values != 0 && !data) return false;
                if (components == 1) glUniform1fv(static_cast<GLint>(Arg(0)), count, data);
                else if (components == 2) glUniform2fv(static_cast<GLint>(Arg(0)), count, data);
                else if (components == 3) glUniform3fv(static_cast<GLint>(Arg(0)), count, data);
                else glUniform4fv(static_cast<GLint>(Arg(0)), count, data);
            } else {
                const GLint* data = GuestGlArray<GLint>(Arg(2), values);
                if (values != 0 && !data) return false;
                if (components == 1) glUniform1iv(static_cast<GLint>(Arg(0)), count, data);
                else if (components == 2) glUniform2iv(static_cast<GLint>(Arg(0)), count, data);
                else if (components == 3) glUniform3iv(static_cast<GLint>(Arg(0)), count, data);
                else glUniform4iv(static_cast<GLint>(Arg(0)), count, data);
            }
            return true;
        }
        if (fast_name == "glUniform1i") {
            glUniform1i(static_cast<GLint>(Arg(0)), static_cast<GLint>(Arg(1)));
            return true;
        }
        if (fast_name == "glUniformMatrix3fv" || fast_name == "glUniformMatrix4fv") {
            const GLsizei count = static_cast<GLsizei>(Arg(1));
            if (count < 0) return false;
            const std::size_t components = fast_name == "glUniformMatrix3fv" ? 9u : 16u;
            const GLfloat* data = GuestGlArray<GLfloat>(Arg(3), static_cast<std::size_t>(count) * components);
            if (count != 0 && !data) return false;
            if (components == 9) {
                glUniformMatrix3fv(static_cast<GLint>(Arg(0)), count, static_cast<GLboolean>(Arg(2)), data);
            } else {
                glUniformMatrix4fv(static_cast<GLint>(Arg(0)), count, static_cast<GLboolean>(Arg(2)), data);
            }
            return true;
        }
        if (fast_name == "glUseProgram") { glUseProgram(static_cast<GLuint>(Arg(0))); return true; }
        if (fast_name == "glVertexAttribPointer") {
            const std::uint32_t guest_pointer = Arg(5);
            const void* pointer = GuestGlDataOrOffset(guest_pointer, GL_ARRAY_BUFFER_BINDING);
            if (guest_pointer != 0 && !pointer) return false;
            glVertexAttribPointer(static_cast<GLuint>(Arg(0)), static_cast<GLint>(Arg(1)),
                                  static_cast<GLenum>(Arg(2)), static_cast<GLboolean>(Arg(3)),
                                  static_cast<GLsizei>(Arg(4)), pointer);
            return true;
        }
        if (fast_name == "glViewport") {
            GLint x = static_cast<GLint>(Arg(0));
            GLint y = static_cast<GLint>(Arg(1));
            GLsizei width = static_cast<GLsizei>(Arg(2));
            GLsizei height = static_cast<GLsizei>(Arg(3));
#if defined(__ANDROID__)
            GLint framebuffer = 0;
            glGetIntegerv(GL_FRAMEBUFFER_BINDING, &framebuffer);
            if (framebuffer == 0 && x == 0 && y == 0 && width > 0 && height > 0 && host_native_window_) {
                guest_logical_viewport_width = static_cast<std::uint32_t>(width);
                guest_logical_viewport_height = static_cast<std::uint32_t>(height);
                const int host_width = ANativeWindow_getWidth(static_cast<ANativeWindow*>(host_native_window_));
                const int host_height = ANativeWindow_getHeight(static_cast<ANativeWindow*>(host_native_window_));
                if (host_width > 0 && host_height > 0) {
                    static int viewport_log_count = 0;
                    if (viewport_log_count < 40) {
                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                            "PV7GL default viewport %dx%d -> %dx%d",
                                            width, height, host_width, host_height);
                        ++viewport_log_count;
                    }
                    if (width != host_width || height != host_height) {
                        width = static_cast<GLsizei>(host_width);
                        height = static_cast<GLsizei>(host_height);
                    }
                }
            }
#endif
            glViewport(x, y, width, height);
            return true;
        }
        return false;
    }
#endif
