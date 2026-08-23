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
        if (name == "glDiscardFramebufferEXT") {
            using Proc = void (*)(GLenum, GLsizei, const GLenum*);
            const GLsizei count = static_cast<GLsizei>(Arg(1));
            const GLenum* attachments = count > 0 ? GuestGlArray<GLenum>(Arg(2), static_cast<std::size_t>(count)) : nullptr;
            if (count < 0 || (count > 0 && !attachments)) return false;
            if (const Proc proc = HostGlExtension<Proc>("glDiscardFramebufferEXT")) proc(static_cast<GLenum>(Arg(0)), count, attachments);
            return true;
        }
        if (name == "glMapBufferOES" || name == "glMapBufferRangeEXT") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            GLint host_size = 0;
            glGetBufferParameteriv(target, GL_BUFFER_SIZE, &host_size);
            if (host_size < 0) { Ret(0); return true; }
            const std::uint32_t offset = name == "glMapBufferRangeEXT" ? Arg(1) : 0u;
            const std::uint32_t length = name == "glMapBufferRangeEXT" ? Arg(2) : static_cast<std::uint32_t>(host_size);
            if (offset > static_cast<std::uint32_t>(host_size) || length > static_cast<std::uint32_t>(host_size) - offset) { Ret(0); return true; }
            const std::uint32_t guest = Allocate(length ? length : 1u, 16u);
            if (!guest) { Ret(0); return true; }
            if (length) std::memset(memory_.data() + guest, 0, length);
            gl_mapped_ranges_[static_cast<std::uint32_t>(target)] = {guest, offset, length};
            Ret(guest);
            return true;
        }
        if (name == "glFlushMappedBufferRangeEXT") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            const auto found = gl_mapped_ranges_.find(static_cast<std::uint32_t>(target));
            if (found == gl_mapped_ranges_.end()) return true;
            const std::uint32_t relative = Arg(1), length = Arg(2);
            if (relative > found->second.length || length > found->second.length - relative) return false;
            if (length) glBufferSubData(target, static_cast<GLintptr>(found->second.offset + relative), static_cast<GLsizeiptr>(length), memory_.data() + found->second.guest + relative);
            return true;
        }
        if (name == "glUnmapBufferOES") {
            const GLenum target = static_cast<GLenum>(Arg(0));
            const auto found = gl_mapped_ranges_.find(static_cast<std::uint32_t>(target));
            if (found != gl_mapped_ranges_.end()) {
                if (found->second.length) glBufferSubData(target, static_cast<GLintptr>(found->second.offset), static_cast<GLsizeiptr>(found->second.length), memory_.data() + found->second.guest);
                gl_mapped_ranges_.erase(found);
            }
            Ret(GL_TRUE);
            return true;
        }
        if (name == "glGenQueriesEXT" || name == "glDeleteQueriesEXT") {
            using Proc = void (*)(GLsizei, GLuint*);
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = count > 0 ? GuestGlArray<GLuint>(Arg(1), static_cast<std::size_t>(count)) : nullptr;
            if (count < 0 || (count > 0 && !values)) return false;
            if (const Proc proc = HostGlExtension<Proc>(name.c_str())) proc(count, values);
            else if (name == "glGenQueriesEXT" && values) std::memset(values, 0, static_cast<std::size_t>(count) * sizeof(GLuint));
            return true;
        }
        if (name == "glBeginQueryEXT") {
            using Proc = void (*)(GLenum, GLuint);
            if (const Proc proc = HostGlExtension<Proc>("glBeginQueryEXT")) proc(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1)));
            return true;
        }
        if (name == "glEndQueryEXT") {
            using Proc = void (*)(GLenum);
            if (const Proc proc = HostGlExtension<Proc>("glEndQueryEXT")) proc(static_cast<GLenum>(Arg(0)));
            return true;
        }
        if (name == "glGetQueryObjectuivEXT") {
            using Proc = void (*)(GLuint, GLenum, GLuint*);
            GLuint* value = GuestGlArray<GLuint>(Arg(2));
            if (!value) return false;
            if (const Proc proc = HostGlExtension<Proc>("glGetQueryObjectuivEXT")) proc(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), value);
            else *value = 0;
            return true;
        }
        if (name == "glQueryCounterEXT") {
            using Proc = void (*)(GLuint, GLenum);
            if (const Proc proc = HostGlExtension<Proc>("glQueryCounterEXT")) proc(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)));
            return true;
        }
        if (name == "glGetQueryObjectui64vEXT") {
            using Proc = void (*)(GLuint, GLenum, std::uint64_t*);
            if (!Arg(2) || !Fits(Arg(2), 8u)) return false;
            std::uint64_t value = 0;
            if (const Proc proc = HostGlExtension<Proc>("glGetQueryObjectui64vEXT")) proc(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), &value);
            Write<std::uint64_t>(Arg(2), value);
            return true;
        }
        if (name == "glDrawBuffersNV") {
            using Proc = void (*)(GLsizei, const GLenum*);
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLenum* values = count > 0 ? GuestGlArray<GLenum>(Arg(1), static_cast<std::size_t>(count)) : nullptr;
            if (count < 0 || (count > 0 && !values)) return false;
            if (const Proc proc = HostGlExtension<Proc>("glDrawBuffersNV")) proc(count, values);
            return true;
        }
        if (name == "glRenderbufferStorageMultisampleEXT" || name == "glRenderbufferStorageMultisampleAPPLE") {
            using Proc = void (*)(GLenum, GLsizei, GLenum, GLsizei, GLsizei);
            if (const Proc proc = HostGlExtension<Proc>(name.c_str())) proc(static_cast<GLenum>(Arg(0)), static_cast<GLsizei>(Arg(1)), static_cast<GLenum>(Arg(2)), static_cast<GLsizei>(Arg(3)), static_cast<GLsizei>(Arg(4)));
            return true;
        }
        if (name == "glFramebufferTexture2DMultisampleEXT") {
            using Proc = void (*)(GLenum, GLenum, GLenum, GLuint, GLint, GLsizei);
            if (const Proc proc = HostGlExtension<Proc>("glFramebufferTexture2DMultisampleEXT")) proc(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)), static_cast<GLenum>(Arg(2)), static_cast<GLuint>(Arg(3)), static_cast<GLint>(Arg(4)), static_cast<GLsizei>(Arg(5)));
            return true;
        }
        if (name == "glResolveMultisampleFramebufferAPPLE") {
            using Proc = void (*)();
            if (const Proc proc = HostGlExtension<Proc>("glResolveMultisampleFramebufferAPPLE")) proc();
            return true;
        }
        if (name == "glGetProgramBinaryOES") {
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
        if (name == "glProgramBinaryOES") {
            using Proc = void (*)(GLuint, GLenum, const void*, GLint);
            const GLint length = static_cast<GLint>(Arg(3));
            const void* binary = length > 0 ? GuestGlBytes(Arg(2), static_cast<std::size_t>(length)) : nullptr;
            if (length < 0 || (length > 0 && !binary)) return false;
            if (const Proc proc = HostGlExtension<Proc>("glProgramBinaryOES")) proc(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), binary, length);
            return true;
        }
        if (name == "glPushGroupMarkerEXT") {
            using Proc = void (*)(GLsizei, const GLchar*);
            const std::string marker = ReadCString(Arg(1), 4096);
            if (const Proc proc = HostGlExtension<Proc>("glPushGroupMarkerEXT")) proc(static_cast<GLsizei>(Arg(0)), marker.c_str());
            return true;
        }
        if (name == "glPopGroupMarkerEXT") {
            using Proc = void (*)();
            if (const Proc proc = HostGlExtension<Proc>("glPopGroupMarkerEXT")) proc();
            return true;
        }

        if (name == "glActiveTexture") { glActiveTexture(static_cast<GLenum>(Arg(0))); return true; }
        if (name == "glAttachShader") { glAttachShader(static_cast<GLuint>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (name == "glBindAttribLocation") {
            const std::string value = ReadCString(Arg(2));
            glBindAttribLocation(static_cast<GLuint>(Arg(0)), static_cast<GLuint>(Arg(1)), value.c_str());
            return true;
        }
        if (name == "glBindBuffer") { glBindBuffer(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (name == "glBindFramebuffer") { glBindFramebuffer(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (name == "glBindRenderbuffer") { glBindRenderbuffer(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (name == "glBindTexture") { glBindTexture(static_cast<GLenum>(Arg(0)), static_cast<GLuint>(Arg(1))); return true; }
        if (name == "glBlendEquationSeparate") {
            glBlendEquationSeparate(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)));
            return true;
        }
        if (name == "glBlendFuncSeparate") {
            glBlendFuncSeparate(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                static_cast<GLenum>(Arg(2)), static_cast<GLenum>(Arg(3)));
            return true;
        }
        if (name == "glBufferData") {
            const GLsizeiptr size = static_cast<GLsizeiptr>(static_cast<std::int32_t>(Arg(1)));
            if (size < 0) return false;
            const std::uint32_t guest_data = Arg(2);
            const void* data = guest_data == 0 ? nullptr : GuestGlBytes(guest_data, static_cast<std::size_t>(size));
            if (guest_data != 0 && !data) return false;
            glBufferData(static_cast<GLenum>(Arg(0)), size, data, static_cast<GLenum>(Arg(3)));
            return true;
        }
        if (name == "glBufferSubData") {
            const GLintptr offset = static_cast<GLintptr>(static_cast<std::int32_t>(Arg(1)));
            const GLsizeiptr size = static_cast<GLsizeiptr>(static_cast<std::int32_t>(Arg(2)));
            if (offset < 0 || size < 0) return false;
            const std::uint32_t guest_data = Arg(3);
            const void* data = guest_data == 0 ? nullptr : GuestGlBytes(guest_data, static_cast<std::size_t>(size));
            if (guest_data != 0 && !data) return false;
            glBufferSubData(static_cast<GLenum>(Arg(0)), offset, size, data);
            return true;
        }
        if (name == "glCheckFramebufferStatus") {
            Ret(static_cast<std::uint32_t>(glCheckFramebufferStatus(static_cast<GLenum>(Arg(0)))));
            return true;
        }
        if (name == "glClear") { glClear(static_cast<GLbitfield>(Arg(0))); return true; }
        if (name == "glClearColor") { glClearColor(ArgFloat(0), ArgFloat(1), ArgFloat(2), ArgFloat(3)); return true; }
        if (name == "glClearDepthf") { glClearDepthf(ArgFloat(0)); return true; }
        if (name == "glClearStencil") { glClearStencil(static_cast<GLint>(Arg(0))); return true; }
        if (name == "glColorMask") {
            glColorMask(static_cast<GLboolean>(Arg(0)), static_cast<GLboolean>(Arg(1)),
                        static_cast<GLboolean>(Arg(2)), static_cast<GLboolean>(Arg(3)));
            return true;
        }
        if (name == "glCompileShader") { glCompileShader(static_cast<GLuint>(Arg(0))); return true; }
        if (name == "glCompressedTexImage2D") {
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
        if (name == "glCompressedTexSubImage2D") {
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
        if (name == "glCopyTexSubImage2D") {
            glCopyTexSubImage2D(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)),
                                static_cast<GLint>(Arg(2)), static_cast<GLint>(Arg(3)),
                                static_cast<GLint>(Arg(4)), static_cast<GLint>(Arg(5)),
                                static_cast<GLsizei>(Arg(6)), static_cast<GLsizei>(Arg(7)));
            return true;
        }
        if (name == "glCreateProgram") {
            using Proc = GLuint (*)();
            if (const Proc proc = HostGlExtension<Proc>("glCreateProgram")) {
                Ret(static_cast<std::uint32_t>(proc()));
            } else {
                Ret(0);
            }
            return true;
        }
        if (name == "glCreateShader") {
            Ret(static_cast<std::uint32_t>(glCreateShader(static_cast<GLenum>(Arg(0)))));
            return true;
        }
        if (name == "glCullFace") { glCullFace(static_cast<GLenum>(Arg(0))); return true; }
        if (name == "glDeleteBuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glDeleteBuffers(count, values);
            return true;
        }
        if (name == "glDeleteFramebuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glDeleteFramebuffers(count, values);
            return true;
        }
        if (name == "glDeleteProgram") { glDeleteProgram(static_cast<GLuint>(Arg(0))); return true; }
        if (name == "glDeleteRenderbuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glDeleteRenderbuffers(count, values);
            return true;
        }
        if (name == "glDeleteShader") { glDeleteShader(static_cast<GLuint>(Arg(0))); return true; }
        if (name == "glDeleteTextures") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            const GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glDeleteTextures(count, values);
            return true;
        }
        if (name == "glDepthFunc") { glDepthFunc(static_cast<GLenum>(Arg(0))); return true; }
        if (name == "glDepthMask") { glDepthMask(static_cast<GLboolean>(Arg(0))); return true; }
        if (name == "glDisable") { glDisable(static_cast<GLenum>(Arg(0))); return true; }
        if (name == "glDisableVertexAttribArray") { glDisableVertexAttribArray(static_cast<GLuint>(Arg(0))); return true; }
        if (name == "glDrawArrays") {
            glDrawArrays(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)), static_cast<GLsizei>(Arg(2)));
            return true;
        }
        if (name == "glDrawElements") {
            const std::uint32_t guest_indices = Arg(3);
            const void* indices = GuestGlDataOrOffset(guest_indices, GL_ELEMENT_ARRAY_BUFFER_BINDING);
            if (guest_indices != 0 && !indices) return false;
            glDrawElements(static_cast<GLenum>(Arg(0)), static_cast<GLsizei>(Arg(1)),
                           static_cast<GLenum>(Arg(2)), indices);
            return true;
        }
        if (name == "glEnable") { glEnable(static_cast<GLenum>(Arg(0))); return true; }
        if (name == "glEnableVertexAttribArray") { glEnableVertexAttribArray(static_cast<GLuint>(Arg(0))); return true; }
        if (name == "glFinish") { glFinish(); return true; }
        if (name == "glFlush") { glFlush(); return true; }
        if (name == "glFramebufferRenderbuffer") {
            glFramebufferRenderbuffer(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                      static_cast<GLenum>(Arg(2)), static_cast<GLuint>(Arg(3)));
            return true;
        }
        if (name == "glFramebufferTexture2D") {
            glFramebufferTexture2D(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                   static_cast<GLenum>(Arg(2)), static_cast<GLuint>(Arg(3)),
                                   static_cast<GLint>(Arg(4)));
            return true;
        }
        if (name == "glFrontFace") { glFrontFace(static_cast<GLenum>(Arg(0))); return true; }
        if (name == "glGenBuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glGenBuffers(count, values);
            return true;
        }
        if (name == "glGenFramebuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glGenFramebuffers(count, values);
            return true;
        }
        if (name == "glGenRenderbuffers") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glGenRenderbuffers(count, values);
            return true;
        }
        if (name == "glGenTextures") {
            const GLsizei count = static_cast<GLsizei>(Arg(0));
            GLuint* values = GuestGlArray<GLuint>(Arg(1), count < 0 ? 0 : static_cast<std::size_t>(count));
            if (count < 0 || (count != 0 && !values)) return false;
            glGenTextures(count, values);
            return true;
        }
        if (name == "glGenerateMipmap") { glGenerateMipmap(static_cast<GLenum>(Arg(0))); return true; }
        if (name == "glGetActiveAttrib" || name == "glGetActiveUniform") {
            const GLsizei buffer_size = static_cast<GLsizei>(Arg(2));
            if (buffer_size < 0) return false;
            GLsizei* length = Arg(3) == 0 ? nullptr : GuestGlArray<GLsizei>(Arg(3));
            GLint* size = Arg(4) == 0 ? nullptr : GuestGlArray<GLint>(Arg(4));
            GLenum* type = Arg(5) == 0 ? nullptr : GuestGlArray<GLenum>(Arg(5));
            GLchar* out_name = Arg(6) == 0 ? nullptr : GuestGlArray<GLchar>(Arg(6), static_cast<std::size_t>(buffer_size));
            if ((Arg(3) && !length) || (Arg(4) && !size) || (Arg(5) && !type) ||
                (Arg(6) && buffer_size != 0 && !out_name)) return false;
            if (name == "glGetActiveAttrib") {
                glGetActiveAttrib(static_cast<GLuint>(Arg(0)), static_cast<GLuint>(Arg(1)),
                                  buffer_size, length, size, type, out_name);
            } else {
                glGetActiveUniform(static_cast<GLuint>(Arg(0)), static_cast<GLuint>(Arg(1)),
                                   buffer_size, length, size, type, out_name);
            }
            return true;
        }
        if (name == "glGetAttribLocation" || name == "glGetUniformLocation") {
            const std::string value = ReadCString(Arg(1));
            const GLint result = name == "glGetAttribLocation"
                ? glGetAttribLocation(static_cast<GLuint>(Arg(0)), value.c_str())
                : glGetUniformLocation(static_cast<GLuint>(Arg(0)), value.c_str());
            Ret(static_cast<std::uint32_t>(result));
            return true;
        }
        if (name == "glGetError") { Ret(static_cast<std::uint32_t>(glGetError())); return true; }
        if (name == "glGetFramebufferAttachmentParameteriv") {
            GLint* value = GuestGlArray<GLint>(Arg(3));
            if (!value) return false;
            glGetFramebufferAttachmentParameteriv(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                                  static_cast<GLenum>(Arg(2)), value);
            return true;
        }
        if (name == "glGetIntegerv") {
            GLint* value = GuestGlArray<GLint>(Arg(1), 16);
            if (!value) value = GuestGlArray<GLint>(Arg(1));
            if (!value) return false;
            glGetIntegerv(static_cast<GLenum>(Arg(0)), value);
            return true;
        }
        if (name == "glGetProgramInfoLog" || name == "glGetShaderInfoLog") {
            const GLsizei buffer_size = static_cast<GLsizei>(Arg(1));
            if (buffer_size < 0) return false;
            GLsizei* length = Arg(2) == 0 ? nullptr : GuestGlArray<GLsizei>(Arg(2));
            GLchar* log = Arg(3) == 0 ? nullptr : GuestGlArray<GLchar>(Arg(3), static_cast<std::size_t>(buffer_size));
            if ((Arg(2) && !length) || (Arg(3) && buffer_size != 0 && !log)) return false;
            if (name == "glGetProgramInfoLog") {
                glGetProgramInfoLog(static_cast<GLuint>(Arg(0)), buffer_size, length, log);
            } else {
                glGetShaderInfoLog(static_cast<GLuint>(Arg(0)), buffer_size, length, log);
            }
            return true;
        }
        if (name == "glGetProgramiv" || name == "glGetShaderiv") {
            GLint* value = GuestGlArray<GLint>(Arg(2));
            if (!value) return false;
            if (name == "glGetProgramiv") glGetProgramiv(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), value);
            else glGetShaderiv(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), value);
            return true;
        }
        if (name == "glGetString") {
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
        if (name == "glGetVertexAttribPointerv") {
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
        if (name == "glGetVertexAttribiv") {
            GLint* value = GuestGlArray<GLint>(Arg(2));
            if (!value) return false;
            glGetVertexAttribiv(static_cast<GLuint>(Arg(0)), static_cast<GLenum>(Arg(1)), value);
            return true;
        }
        if (name == "glLinkProgram") { glLinkProgram(static_cast<GLuint>(Arg(0))); return true; }
        if (name == "glPixelStorei") { glPixelStorei(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1))); return true; }
        if (name == "glPolygonOffset") { glPolygonOffset(ArgFloat(0), ArgFloat(1)); return true; }
        if (name == "glReadPixels") {
            const std::uint32_t guest_pixels = Arg(6);
            void* pixels = guest_pixels == 0 ? nullptr : GuestGlBytes(guest_pixels);
            if (guest_pixels != 0 && !pixels) return false;
            glReadPixels(static_cast<GLint>(Arg(0)), static_cast<GLint>(Arg(1)),
                         static_cast<GLsizei>(Arg(2)), static_cast<GLsizei>(Arg(3)),
                         static_cast<GLenum>(Arg(4)), static_cast<GLenum>(Arg(5)), pixels);
            return true;
        }
        if (name == "glRenderbufferStorage") {
            glRenderbufferStorage(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                  static_cast<GLsizei>(Arg(2)), static_cast<GLsizei>(Arg(3)));
            return true;
        }
        if (name == "glScissor") {
            glScissor(static_cast<GLint>(Arg(0)), static_cast<GLint>(Arg(1)),
                      static_cast<GLsizei>(Arg(2)), static_cast<GLsizei>(Arg(3)));
            return true;
        }
        if (name == "glShaderSource") {
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
        if (name == "glStencilFunc") {
            glStencilFunc(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)), static_cast<GLuint>(Arg(2)));
            return true;
        }
        if (name == "glStencilFuncSeparate") {
            glStencilFuncSeparate(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                  static_cast<GLint>(Arg(2)), static_cast<GLuint>(Arg(3)));
            return true;
        }
        if (name == "glStencilMask") { glStencilMask(static_cast<GLuint>(Arg(0))); return true; }
        if (name == "glStencilOp") {
            glStencilOp(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)), static_cast<GLenum>(Arg(2)));
            return true;
        }
        if (name == "glStencilOpSeparate") {
            glStencilOpSeparate(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)),
                                static_cast<GLenum>(Arg(2)), static_cast<GLenum>(Arg(3)));
            return true;
        }
        if (name == "glTexImage2D") {
            const std::uint32_t guest_pixels = Arg(8);
            const void* pixels = guest_pixels == 0 ? nullptr : GuestGlBytes(guest_pixels);
            if (guest_pixels != 0 && !pixels) return false;
            glTexImage2D(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)),
                         static_cast<GLint>(Arg(2)), static_cast<GLsizei>(Arg(3)),
                         static_cast<GLsizei>(Arg(4)), static_cast<GLint>(Arg(5)),
                         static_cast<GLenum>(Arg(6)), static_cast<GLenum>(Arg(7)), pixels);
            return true;
        }
        if (name == "glTexParameteri") {
            glTexParameteri(static_cast<GLenum>(Arg(0)), static_cast<GLenum>(Arg(1)), static_cast<GLint>(Arg(2)));
            return true;
        }
        if (name == "glTexSubImage2D") {
            const std::uint32_t guest_pixels = Arg(8);
            const void* pixels = guest_pixels == 0 ? nullptr : GuestGlBytes(guest_pixels);
            if (guest_pixels != 0 && !pixels) return false;
            glTexSubImage2D(static_cast<GLenum>(Arg(0)), static_cast<GLint>(Arg(1)),
                            static_cast<GLint>(Arg(2)), static_cast<GLint>(Arg(3)),
                            static_cast<GLsizei>(Arg(4)), static_cast<GLsizei>(Arg(5)),
                            static_cast<GLenum>(Arg(6)), static_cast<GLenum>(Arg(7)), pixels);
            return true;
        }
        if (name == "glUniform1fv" || name == "glUniform1iv" ||
            name == "glUniform2fv" || name == "glUniform2iv" ||
            name == "glUniform3fv" || name == "glUniform3iv" ||
            name == "glUniform4fv" || name == "glUniform4iv") {
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
        if (name == "glUniform1i") {
            glUniform1i(static_cast<GLint>(Arg(0)), static_cast<GLint>(Arg(1)));
            return true;
        }
        if (name == "glUniformMatrix3fv" || name == "glUniformMatrix4fv") {
            const GLsizei count = static_cast<GLsizei>(Arg(1));
            if (count < 0) return false;
            const std::size_t components = name == "glUniformMatrix3fv" ? 9u : 16u;
            const GLfloat* data = GuestGlArray<GLfloat>(Arg(3), static_cast<std::size_t>(count) * components);
            if (count != 0 && !data) return false;
            if (components == 9) {
                glUniformMatrix3fv(static_cast<GLint>(Arg(0)), count, static_cast<GLboolean>(Arg(2)), data);
            } else {
                glUniformMatrix4fv(static_cast<GLint>(Arg(0)), count, static_cast<GLboolean>(Arg(2)), data);
            }
            return true;
        }
        if (name == "glUseProgram") { glUseProgram(static_cast<GLuint>(Arg(0))); return true; }
        if (name == "glVertexAttribPointer") {
            const std::uint32_t guest_pointer = Arg(5);
            const void* pointer = GuestGlDataOrOffset(guest_pointer, GL_ARRAY_BUFFER_BINDING);
            if (guest_pointer != 0 && !pointer) return false;
            glVertexAttribPointer(static_cast<GLuint>(Arg(0)), static_cast<GLint>(Arg(1)),
                                  static_cast<GLenum>(Arg(2)), static_cast<GLboolean>(Arg(3)),
                                  static_cast<GLsizei>(Arg(4)), pointer);
            return true;
        }
        if (name == "glViewport") {
            glViewport(static_cast<GLint>(Arg(0)), static_cast<GLint>(Arg(1)),
                       static_cast<GLsizei>(Arg(2)), static_cast<GLsizei>(Arg(3)));
            return true;
        }
        return false;
    }
#endif
