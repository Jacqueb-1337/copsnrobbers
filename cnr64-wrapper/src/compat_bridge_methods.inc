    bool WriteGuestTm32(std::uint32_t address, const std::tm& value) {
        if (!address || !Fits(address, 44u)) return false;
        Write<std::int32_t>(address + 0u, value.tm_sec);
        Write<std::int32_t>(address + 4u, value.tm_min);
        Write<std::int32_t>(address + 8u, value.tm_hour);
        Write<std::int32_t>(address + 12u, value.tm_mday);
        Write<std::int32_t>(address + 16u, value.tm_mon);
        Write<std::int32_t>(address + 20u, value.tm_year);
        Write<std::int32_t>(address + 24u, value.tm_wday);
        Write<std::int32_t>(address + 28u, value.tm_yday);
        Write<std::int32_t>(address + 32u, value.tm_isdst);
        Write<std::int32_t>(address + 36u, 0); // tm_gmtoff, conservative guest value
        Write32(memory_, address + 40u, 0u);   // tm_zone
        return true;
    }

    bool ReadGuestTm32(std::uint32_t address, std::tm& value) const {
        if (!address || !Fits(address, 36u)) return false;
        value = {};
        value.tm_sec = Read<std::int32_t>(address + 0u);
        value.tm_min = Read<std::int32_t>(address + 4u);
        value.tm_hour = Read<std::int32_t>(address + 8u);
        value.tm_mday = Read<std::int32_t>(address + 12u);
        value.tm_mon = Read<std::int32_t>(address + 16u);
        value.tm_year = Read<std::int32_t>(address + 20u);
        value.tm_wday = Read<std::int32_t>(address + 24u);
        value.tm_yday = Read<std::int32_t>(address + 28u);
        value.tm_isdst = Read<std::int32_t>(address + 32u);
        return true;
    }

    static std::uint32_t Rotl32(std::uint32_t value, unsigned bits) {
        return (value << bits) | (value >> (32u - bits));
    }

    struct Sha1State32 {
        std::uint32_t h[5]{};
        std::uint64_t bits = 0;
        std::uint8_t block[64]{};
        std::uint32_t used = 0;
    };

    static void Sha1Transform(Sha1State32& state, const std::uint8_t* block) {
        std::uint32_t w[80]{};
        for (unsigned i = 0; i < 16; ++i) {
            w[i] = (static_cast<std::uint32_t>(block[i * 4u]) << 24) |
                   (static_cast<std::uint32_t>(block[i * 4u + 1u]) << 16) |
                   (static_cast<std::uint32_t>(block[i * 4u + 2u]) << 8) |
                   static_cast<std::uint32_t>(block[i * 4u + 3u]);
        }
        for (unsigned i = 16; i < 80; ++i) w[i] = Rotl32(w[i - 3] ^ w[i - 8] ^ w[i - 14] ^ w[i - 16], 1);
        std::uint32_t a = state.h[0], b = state.h[1], c = state.h[2], d = state.h[3], e = state.h[4];
        for (unsigned i = 0; i < 80; ++i) {
            std::uint32_t f = 0, k = 0;
            if (i < 20) { f = (b & c) | ((~b) & d); k = 0x5a827999u; }
            else if (i < 40) { f = b ^ c ^ d; k = 0x6ed9eba1u; }
            else if (i < 60) { f = (b & c) | (b & d) | (c & d); k = 0x8f1bbcdcu; }
            else { f = b ^ c ^ d; k = 0xca62c1d6u; }
            const std::uint32_t temp = Rotl32(a, 5) + f + e + k + w[i];
            e = d; d = c; c = Rotl32(b, 30); b = a; a = temp;
        }
        state.h[0] += a; state.h[1] += b; state.h[2] += c; state.h[3] += d; state.h[4] += e;
    }

    static void Sha1InitState(Sha1State32& state) {
        state = {};
        state.h[0] = 0x67452301u;
        state.h[1] = 0xefcdab89u;
        state.h[2] = 0x98badcfeu;
        state.h[3] = 0x10325476u;
        state.h[4] = 0xc3d2e1f0u;
    }

    static void Sha1UpdateState(Sha1State32& state, const std::uint8_t* data, std::size_t length) {
        state.bits += static_cast<std::uint64_t>(length) * 8u;
        while (length != 0) {
            const std::size_t take = std::min<std::size_t>(64u - state.used, length);
            std::memcpy(state.block + state.used, data, take);
            state.used += static_cast<std::uint32_t>(take);
            data += take;
            length -= take;
            if (state.used == 64u) {
                Sha1Transform(state, state.block);
                state.used = 0;
            }
        }
    }

    static void Sha1FinalState(Sha1State32& state, std::uint8_t digest[20]) {
        const std::uint64_t original_bits = state.bits;
        const std::uint8_t marker = 0x80;
        Sha1UpdateState(state, &marker, 1);
        const std::uint8_t zero = 0;
        while (state.used != 56u) Sha1UpdateState(state, &zero, 1);
        std::uint8_t length_bytes[8]{};
        for (unsigned i = 0; i < 8; ++i) length_bytes[7u - i] = static_cast<std::uint8_t>(original_bits >> (i * 8u));
        Sha1UpdateState(state, length_bytes, sizeof(length_bytes));
        for (unsigned i = 0; i < 5; ++i) {
            digest[i * 4u] = static_cast<std::uint8_t>(state.h[i] >> 24);
            digest[i * 4u + 1u] = static_cast<std::uint8_t>(state.h[i] >> 16);
            digest[i * 4u + 2u] = static_cast<std::uint8_t>(state.h[i] >> 8);
            digest[i * 4u + 3u] = static_cast<std::uint8_t>(state.h[i]);
        }
    }

    bool StartGuestQsort(std::uint32_t base, std::uint32_t nmemb, std::uint32_t size, std::uint32_t comparator) {
        if (nmemb <= 1u || size == 0u) { Ret(0); return true; }
        const std::uint64_t bytes = static_cast<std::uint64_t>(nmemb) * size;
        if (bytes > SIZE_MAX || !Fits(base, static_cast<std::size_t>(bytes)) || !Fits(comparator & ~1u, 4u)) return false;
        const std::uint32_t pivot = Allocate(size, 8u);
        if (!pivot || !Fits(pivot, size)) return false;
        QsortFrame frame{};
        frame.base = base;
        frame.size = size;
        frame.comparator = comparator;
        frame.caller_lr = jit->Regs()[14];
        frame.pivot_guest = pivot;
        frame.ranges.push_back({0u, nmemb - 1u});
        qsort_frames_.push_back(std::move(frame));
        BeginNextQsortRange();
        return true;
    }

    void LaunchQsortComparison(QsortFrame& frame) {
        const std::uint32_t element = frame.base + frame.j * frame.size;
        jit->Regs()[0] = element;
        jit->Regs()[1] = frame.pivot_guest;
        jit->Regs()[2] = 0;
        jit->Regs()[3] = 0;
        jit->Regs()[14] = kQsortReturnStub;
        jit->Regs()[15] = frame.comparator & ~1u;
        jit->SetCpsr((frame.comparator & 1u) ? 0x20u : 0u);
        pending_guest_callback = true;
        jit->HaltExecution();
    }

    void FinishGuestQsort() {
        if (qsort_frames_.empty()) return;
        const std::uint32_t caller_lr = qsort_frames_.back().caller_lr;
        qsort_frames_.pop_back();
        jit->Regs()[0] = 0;
        jit->Regs()[14] = caller_lr;
    }

    void BeginNextQsortRange() {
        if (qsort_frames_.empty()) return;
        QsortFrame& frame = qsort_frames_.back();
        while (!frame.ranges.empty()) {
            const auto range = frame.ranges.back();
            frame.ranges.pop_back();
            frame.lo = range.first;
            frame.hi = range.second;
            if (frame.lo >= frame.hi) continue;
            frame.i = frame.lo;
            frame.j = frame.lo;
            const std::uint32_t pivot_source = frame.base + frame.hi * frame.size;
            std::memcpy(memory_.data() + frame.pivot_guest, memory_.data() + pivot_source, frame.size);
            LaunchQsortComparison(frame);
            return;
        }
        FinishGuestQsort();
    }

    void HandleGuestQsortComparison(int comparison) {
        if (qsort_frames_.empty()) return;
        QsortFrame& frame = qsort_frames_.back();
        if (comparison <= 0) {
            if (frame.i != frame.j) {
                const std::uint32_t a = frame.base + frame.i * frame.size;
                const std::uint32_t b = frame.base + frame.j * frame.size;
                std::vector<std::uint8_t> temp(frame.size);
                std::memcpy(temp.data(), memory_.data() + a, frame.size);
                std::memcpy(memory_.data() + a, memory_.data() + b, frame.size);
                std::memcpy(memory_.data() + b, temp.data(), frame.size);
            }
            ++frame.i;
        }
        ++frame.j;
        if (frame.j < frame.hi) {
            LaunchQsortComparison(frame);
            return;
        }
        const std::uint32_t pivot_target = frame.base + frame.i * frame.size;
        const std::uint32_t pivot_source = frame.base + frame.hi * frame.size;
        if (frame.i != frame.hi) {
            std::vector<std::uint8_t> temp(frame.size);
            std::memcpy(temp.data(), memory_.data() + pivot_target, frame.size);
            std::memcpy(memory_.data() + pivot_target, memory_.data() + pivot_source, frame.size);
            std::memcpy(memory_.data() + pivot_source, temp.data(), frame.size);
        }
        if (frame.i > frame.lo + 1u) frame.ranges.push_back({frame.lo, frame.i - 1u});
        if (frame.i + 1u < frame.hi) frame.ranges.push_back({frame.i + 1u, frame.hi});
        BeginNextQsortRange();
    }

    int ScanGuestText(const std::string& input, const std::string& format, unsigned first_output_word) {
        std::size_t ip = 0;
        unsigned output_word = first_output_word;
        int assigned = 0;
        auto skip_space = [&]() { while (ip < input.size() && std::isspace(static_cast<unsigned char>(input[ip]))) ++ip; };
        for (std::size_t fp = 0; fp < format.size();) {
            if (std::isspace(static_cast<unsigned char>(format[fp]))) { while (fp < format.size() && std::isspace(static_cast<unsigned char>(format[fp]))) ++fp; skip_space(); continue; }
            if (format[fp] != '%') {
                if (ip >= input.size() || input[ip] != format[fp]) break;
                ++ip; ++fp; continue;
            }
            ++fp;
            if (fp < format.size() && format[fp] == '%') {
                if (ip >= input.size() || input[ip] != '%') break;
                ++ip; ++fp; continue;
            }
            bool suppress = false;
            if (fp < format.size() && format[fp] == '*') { suppress = true; ++fp; }
            unsigned width = 0;
            while (fp < format.size() && std::isdigit(static_cast<unsigned char>(format[fp]))) width = width * 10u + static_cast<unsigned>(format[fp++] - '0');
            while (fp < format.size() && (format[fp] == 'h' || format[fp] == 'l' || format[fp] == 'L' || format[fp] == 'z' || format[fp] == 'j' || format[fp] == 't')) ++fp;
            if (fp >= format.size()) break;
            const char conv = format[fp++];
            if (conv != 'c' && conv != '[') skip_space();
            const std::uint32_t output = suppress ? 0u : Arg(output_word++);
            const std::size_t start = ip;
            if (conv == 's') {
                while (ip < input.size() && !std::isspace(static_cast<unsigned char>(input[ip])) && (width == 0u || ip - start < width)) ++ip;
                if (ip == start) break;
                if (!suppress) { if (!WriteCString(output, input.substr(start, ip - start))) break; ++assigned; }
            } else if (conv == 'c') {
                const std::size_t count = width ? width : 1u;
                if (ip + count > input.size()) break;
                if (!suppress) { if (!output || !Fits(output, count)) break; std::memcpy(memory_.data() + output, input.data() + ip, count); ++assigned; }
                ip += count;
            } else if (conv == 'd' || conv == 'i' || conv == 'u' || conv == 'o' || conv == 'x' || conv == 'X') {
                const char* begin = input.c_str() + ip;
                char* end = nullptr;
                const int base = conv == 'o' ? 8 : ((conv == 'x' || conv == 'X') ? 16 : (conv == 'i' ? 0 : 10));
                unsigned long value = (conv == 'd' || conv == 'i')
                    ? static_cast<unsigned long>(std::strtol(begin, &end, base))
                    : std::strtoul(begin, &end, base);
                if (end == begin) break;
                ip += static_cast<std::size_t>(end - begin);
                if (!suppress) { if (!Write32(memory_, output, static_cast<std::uint32_t>(value))) break; ++assigned; }
            } else if (conv == 'f' || conv == 'e' || conv == 'E' || conv == 'g' || conv == 'G') {
                const char* begin = input.c_str() + ip;
                char* end = nullptr;
                const float value = std::strtof(begin, &end);
                if (end == begin) break;
                ip += static_cast<std::size_t>(end - begin);
                if (!suppress) { if (!output || !Fits(output, 4u)) break; std::memcpy(memory_.data() + output, &value, 4u); ++assigned; }
            } else if (conv == 'n') {
                if (!suppress) { if (!Write32(memory_, output, static_cast<std::uint32_t>(ip))) break; ++assigned; }
            } else {
                break;
            }
        }
        return assigned;
    }

    bool DispatchRealSockets(const std::string& name) {
#if !defined(_WIN32)
        if (name == "socket") {
            errno = 0;
            const int fd = ::socket(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)), static_cast<int>(Arg(2)));
            SetErrno(fd >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(fd));
            return true;
        }
        if (name == "bind" || name == "connect") {
            const std::uint32_t address = Arg(1), length = Arg(2);
            if (length && !Fits(address, length)) { SetErrno(14); Ret(0xffffffffu); return true; }
            errno = 0;
            const sockaddr* sa = length ? reinterpret_cast<const sockaddr*>(memory_.data() + address) : nullptr;
            const int rc = name == "bind"
                ? ::bind(static_cast<int>(Arg(0)), sa, static_cast<socklen_t>(length))
                : ::connect(static_cast<int>(Arg(0)), sa, static_cast<socklen_t>(length));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "listen" || name == "shutdown") {
            errno = 0;
            const int rc = name == "listen"
                ? ::listen(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)))
                : ::shutdown(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "accept") {
            sockaddr_storage storage{};
            socklen_t length = sizeof(storage);
            if (Arg(2) && Fits(Arg(2), 4u)) length = std::min<socklen_t>(Read<std::uint32_t>(Arg(2)), sizeof(storage));
            errno = 0;
            const int fd = ::accept(static_cast<int>(Arg(0)), Arg(1) ? reinterpret_cast<sockaddr*>(&storage) : nullptr,
                                    Arg(1) ? &length : nullptr);
            if (fd >= 0 && Arg(1)) {
                if (!Fits(Arg(1), length)) { ::close(fd); SetErrno(14); Ret(0xffffffffu); return true; }
                std::memcpy(memory_.data() + Arg(1), &storage, length);
                if (Arg(2)) Write32(memory_, Arg(2), static_cast<std::uint32_t>(length));
            }
            SetErrno(fd >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(fd));
            return true;
        }
        if (name == "send" || name == "recv") {
            const std::uint32_t buffer = Arg(1), length = Arg(2);
            if (length && !Fits(buffer, length)) { SetErrno(14); Ret(0xffffffffu); return true; }
            errno = 0;
            const ssize_t rc = name == "send"
                ? ::send(static_cast<int>(Arg(0)), memory_.data() + buffer, length, static_cast<int>(Arg(3)))
                : ::recv(static_cast<int>(Arg(0)), memory_.data() + buffer, length, static_cast<int>(Arg(3)));
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "sendto" || name == "recvfrom") {
            const std::uint32_t buffer = Arg(1), length = Arg(2);
            if (length && !Fits(buffer, length)) { SetErrno(14); Ret(0xffffffffu); return true; }
            errno = 0;
            ssize_t rc = -1;
            if (name == "sendto") {
                const std::uint32_t address = Arg(4), address_length = Arg(5);
                if (address_length && !Fits(address, address_length)) { SetErrno(14); Ret(0xffffffffu); return true; }
                const sockaddr* sa = address_length ? reinterpret_cast<const sockaddr*>(memory_.data() + address) : nullptr;
                rc = ::sendto(static_cast<int>(Arg(0)), memory_.data() + buffer, length, static_cast<int>(Arg(3)), sa,
                              static_cast<socklen_t>(address_length));
            } else {
                sockaddr_storage storage{};
                socklen_t address_length = sizeof(storage);
                if (Arg(5) && Fits(Arg(5), 4u)) address_length = std::min<socklen_t>(Read<std::uint32_t>(Arg(5)), sizeof(storage));
                rc = ::recvfrom(static_cast<int>(Arg(0)), memory_.data() + buffer, length, static_cast<int>(Arg(3)),
                                Arg(4) ? reinterpret_cast<sockaddr*>(&storage) : nullptr, Arg(4) ? &address_length : nullptr);
                if (rc >= 0 && Arg(4)) {
                    if (!Fits(Arg(4), address_length)) { SetErrno(14); Ret(0xffffffffu); return true; }
                    std::memcpy(memory_.data() + Arg(4), &storage, address_length);
                    if (Arg(5)) Write32(memory_, Arg(5), static_cast<std::uint32_t>(address_length));
                }
            }
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "sendmsg" || name == "recvmsg") {
            const std::uint32_t guest_header = Arg(1);
            if (!guest_header || !Fits(guest_header, 28u)) { SetErrno(14); Ret(0xffffffffu); return true; }
            const std::uint32_t guest_name = Read<std::uint32_t>(guest_header + 0u);
            std::uint32_t guest_name_length = Read<std::uint32_t>(guest_header + 4u);
            const std::uint32_t guest_iov = Read<std::uint32_t>(guest_header + 8u);
            const std::uint32_t guest_iov_count = Read<std::uint32_t>(guest_header + 12u);
            const std::uint32_t guest_control = Read<std::uint32_t>(guest_header + 16u);
            const std::uint32_t guest_control_length = Read<std::uint32_t>(guest_header + 20u);
            if (guest_iov_count > 1024u || (guest_iov_count && !Fits(guest_iov, static_cast<std::size_t>(guest_iov_count) * 8u))) return false;
            std::vector<iovec> vectors(guest_iov_count);
            for (std::uint32_t i = 0; i < guest_iov_count; ++i) {
                const std::uint32_t base = Read<std::uint32_t>(guest_iov + i * 8u);
                const std::uint32_t len = Read<std::uint32_t>(guest_iov + i * 8u + 4u);
                if (len && !Fits(base, len)) return false;
                vectors[i].iov_base = len ? memory_.data() + base : nullptr;
                vectors[i].iov_len = len;
            }
            if (guest_name_length && !Fits(guest_name, guest_name_length)) return false;
            if (guest_control_length && !Fits(guest_control, guest_control_length)) return false;
            msghdr host{};
            host.msg_name = guest_name_length ? memory_.data() + guest_name : nullptr;
            host.msg_namelen = static_cast<socklen_t>(guest_name_length);
            host.msg_iov = vectors.data();
            host.msg_iovlen = vectors.size();
            host.msg_control = guest_control_length ? memory_.data() + guest_control : nullptr;
            host.msg_controllen = guest_control_length;
            host.msg_flags = static_cast<int>(Read<std::uint32_t>(guest_header + 24u));
            errno = 0;
            const ssize_t rc = name == "sendmsg"
                ? ::sendmsg(static_cast<int>(Arg(0)), &host, static_cast<int>(Arg(2)))
                : ::recvmsg(static_cast<int>(Arg(0)), &host, static_cast<int>(Arg(2)));
            if (name == "recvmsg" && rc >= 0) {
                Write32(memory_, guest_header + 4u, static_cast<std::uint32_t>(host.msg_namelen));
                Write32(memory_, guest_header + 20u, static_cast<std::uint32_t>(host.msg_controllen));
                Write32(memory_, guest_header + 24u, static_cast<std::uint32_t>(host.msg_flags));
            }
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "getsockname" || name == "getpeername") {
            if (!Arg(2) || !Fits(Arg(2), 4u)) { SetErrno(14); Ret(0xffffffffu); return true; }
            socklen_t length = static_cast<socklen_t>(Read<std::uint32_t>(Arg(2)));
            if (length && !Fits(Arg(1), length)) { SetErrno(14); Ret(0xffffffffu); return true; }
            errno = 0;
            const int rc = name == "getsockname"
                ? ::getsockname(static_cast<int>(Arg(0)), reinterpret_cast<sockaddr*>(memory_.data() + Arg(1)), &length)
                : ::getpeername(static_cast<int>(Arg(0)), reinterpret_cast<sockaddr*>(memory_.data() + Arg(1)), &length);
            Write32(memory_, Arg(2), static_cast<std::uint32_t>(length));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "setsockopt" || name == "getsockopt") {
            const int level = static_cast<int>(Arg(1)), option = static_cast<int>(Arg(2));
            errno = 0;
            int rc = -1;
            if (name == "setsockopt") {
                const std::uint32_t guest = Arg(3), length = Arg(4);
                if (length && !Fits(guest, length)) return false;
                if (level == SOL_SOCKET && (option == SO_RCVTIMEO || option == SO_SNDTIMEO) && length >= 8u) {
                    timeval tv{}; tv.tv_sec = Read<std::int32_t>(guest); tv.tv_usec = Read<std::int32_t>(guest + 4u);
                    rc = ::setsockopt(static_cast<int>(Arg(0)), level, option, &tv, sizeof(tv));
                } else rc = ::setsockopt(static_cast<int>(Arg(0)), level, option, length ? memory_.data() + guest : nullptr, static_cast<socklen_t>(length));
            } else {
                if (!Arg(4) || !Fits(Arg(4), 4u)) return false;
                socklen_t length = static_cast<socklen_t>(Read<std::uint32_t>(Arg(4)));
                if (length && !Fits(Arg(3), length)) return false;
                if (level == SOL_SOCKET && (option == SO_RCVTIMEO || option == SO_SNDTIMEO) && length >= 8u) {
                    timeval tv{}; socklen_t host_length = sizeof(tv);
                    rc = ::getsockopt(static_cast<int>(Arg(0)), level, option, &tv, &host_length);
                    if (rc == 0) { Write<std::int32_t>(Arg(3), static_cast<std::int32_t>(tv.tv_sec)); Write<std::int32_t>(Arg(3) + 4u, static_cast<std::int32_t>(tv.tv_usec)); length = 8u; }
                } else rc = ::getsockopt(static_cast<int>(Arg(0)), level, option, length ? memory_.data() + Arg(3) : nullptr, &length);
                Write32(memory_, Arg(4), static_cast<std::uint32_t>(length));
            }
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "poll") {
            const std::uint32_t guest = Arg(0), count = Arg(1);
            if (count > 4096u || (count && !Fits(guest, static_cast<std::size_t>(count) * 8u))) return false;
            std::vector<pollfd> fds(count);
            for (std::uint32_t i = 0; i < count; ++i) {
                fds[i].fd = Read<std::int32_t>(guest + i * 8u);
                fds[i].events = Read<std::int16_t>(guest + i * 8u + 4u);
                fds[i].revents = 0;
            }
            errno = 0;
            const int rc = ::poll(fds.data(), static_cast<nfds_t>(fds.size()), static_cast<int>(Arg(2)));
            for (std::uint32_t i = 0; i < count; ++i) Write<std::int16_t>(guest + i * 8u + 6u, fds[i].revents);
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "select") {
            const int nfds = static_cast<int>(Arg(0));
            fd_set readfds{}, writefds{}, exceptfds{};
            fd_set* r = nullptr; fd_set* w = nullptr; fd_set* e = nullptr;
            if (Arg(1)) { if (!Fits(Arg(1), sizeof(fd_set))) return false; std::memcpy(&readfds, memory_.data() + Arg(1), sizeof(fd_set)); r = &readfds; }
            if (Arg(2)) { if (!Fits(Arg(2), sizeof(fd_set))) return false; std::memcpy(&writefds, memory_.data() + Arg(2), sizeof(fd_set)); w = &writefds; }
            if (Arg(3)) { if (!Fits(Arg(3), sizeof(fd_set))) return false; std::memcpy(&exceptfds, memory_.data() + Arg(3), sizeof(fd_set)); e = &exceptfds; }
            timeval tv{}; timeval* tvp = nullptr;
            if (Arg(4)) { if (!Fits(Arg(4), 8u)) return false; tv.tv_sec = Read<std::int32_t>(Arg(4)); tv.tv_usec = Read<std::int32_t>(Arg(4) + 4u); tvp = &tv; }
            errno = 0; const int rc = ::select(nfds, r, w, e, tvp);
            if (rc >= 0) {
                if (r) std::memcpy(memory_.data() + Arg(1), r, sizeof(fd_set));
                if (w) std::memcpy(memory_.data() + Arg(2), w, sizeof(fd_set));
                if (e) std::memcpy(memory_.data() + Arg(3), e, sizeof(fd_set));
                if (tvp) { Write<std::int32_t>(Arg(4), static_cast<std::int32_t>(tv.tv_sec)); Write<std::int32_t>(Arg(4) + 4u, static_cast<std::int32_t>(tv.tv_usec)); }
            }
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "epoll_create") {
            errno = 0; const int rc = ::epoll_create(static_cast<int>(Arg(0)));
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "epoll_ctl") {
            epoll_event event{}; epoll_event* ptr = nullptr;
            if (Arg(3)) { if (!Fits(Arg(3), 12u)) return false; std::memcpy(&event, memory_.data() + Arg(3), 12u); ptr = &event; }
            errno = 0; const int rc = ::epoll_ctl(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)), static_cast<int>(Arg(2)), ptr);
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "epoll_wait") {
            const int max_events = static_cast<int>(Arg(2));
            if (max_events < 0 || (max_events && !Fits(Arg(1), static_cast<std::size_t>(max_events) * 12u))) return false;
            std::vector<epoll_event> events(static_cast<std::size_t>(max_events));
            errno = 0; const int rc = ::epoll_wait(static_cast<int>(Arg(0)), events.data(), max_events, static_cast<int>(Arg(3)));
            if (rc > 0) std::memcpy(memory_.data() + Arg(1), events.data(), static_cast<std::size_t>(rc) * 12u);
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "pipe") {
            if (!Arg(0) || !Fits(Arg(0), 8u)) return false;
            int fds[2]{}; errno = 0; const int rc = ::pipe(fds);
            if (rc == 0) { Write<std::int32_t>(Arg(0), fds[0]); Write<std::int32_t>(Arg(0) + 4u, fds[1]); }
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "fcntl") {
            errno = 0;
            const int rc = ::fcntl(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)), static_cast<long>(static_cast<std::int32_t>(Arg(2))));
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "ioctl") {
            errno = 0;
            void* argument = Arg(2) && Fits(Arg(2), 4u) ? memory_.data() + Arg(2) : reinterpret_cast<void*>(static_cast<std::uintptr_t>(Arg(2)));
            const int rc = ::ioctl(static_cast<int>(Arg(0)), static_cast<unsigned long>(Arg(1)), argument);
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "getaddrinfo") {
            const std::string node = Arg(0) ? ReadCString(Arg(0), 4096) : std::string{};
            const std::string service = Arg(1) ? ReadCString(Arg(1), 256) : std::string{};
            addrinfo hints{}; addrinfo* hint_ptr = nullptr;
            if (Arg(2)) {
                if (!Fits(Arg(2), 20u)) return false;
                hints.ai_flags = Read<std::int32_t>(Arg(2) + 0u);
                hints.ai_family = Read<std::int32_t>(Arg(2) + 4u);
                hints.ai_socktype = Read<std::int32_t>(Arg(2) + 8u);
                hints.ai_protocol = Read<std::int32_t>(Arg(2) + 12u);
                hint_ptr = &hints;
            }
            addrinfo* result = nullptr;
            const int rc = ::getaddrinfo(node.empty() ? nullptr : node.c_str(), service.empty() ? nullptr : service.c_str(), hint_ptr, &result);
            if (rc != 0) { if (Arg(3)) Write32(memory_, Arg(3), 0u); Ret(static_cast<std::uint32_t>(rc)); return true; }
            std::vector<std::uint32_t> guest_nodes;
            for (addrinfo* item = result; item; item = item->ai_next) {
                const std::uint32_t node_guest = Allocate(32u, 4u);
                const std::uint32_t addr_guest = item->ai_addr && item->ai_addrlen ? Allocate(item->ai_addrlen, 4u) : 0u;
                std::uint32_t canon_guest = 0;
                if (item->ai_canonname) { const std::string canon = item->ai_canonname; canon_guest = Allocate(canon.size() + 1u, 1u); if (canon_guest) WriteCString(canon_guest, canon); }
                if (!node_guest || (item->ai_addrlen && !addr_guest)) { ::freeaddrinfo(result); if (Arg(3)) Write32(memory_, Arg(3), 0u); Ret(static_cast<std::uint32_t>(EAI_MEMORY)); return true; }
                std::memset(memory_.data() + node_guest, 0, 32u);
                Write<std::int32_t>(node_guest + 0u, item->ai_flags);
                Write<std::int32_t>(node_guest + 4u, item->ai_family);
                Write<std::int32_t>(node_guest + 8u, item->ai_socktype);
                Write<std::int32_t>(node_guest + 12u, item->ai_protocol);
                Write32(memory_, node_guest + 16u, static_cast<std::uint32_t>(item->ai_addrlen));
                Write32(memory_, node_guest + 20u, addr_guest);
                Write32(memory_, node_guest + 24u, canon_guest);
                if (addr_guest) std::memcpy(memory_.data() + addr_guest, item->ai_addr, item->ai_addrlen);
                if (!guest_nodes.empty()) Write32(memory_, guest_nodes.back() + 28u, node_guest);
                guest_nodes.push_back(node_guest);
            }
            ::freeaddrinfo(result);
            if (Arg(3)) Write32(memory_, Arg(3), guest_nodes.empty() ? 0u : guest_nodes.front());
            Ret(0); return true;
        }
        if (name == "freeaddrinfo") { Ret(0); return true; }
        if (name == "gai_strerror") {
            const char* text = ::gai_strerror(static_cast<int>(Arg(0)));
            const std::string copy = text ? text : "getaddrinfo error";
            const std::uint32_t guest = Allocate(copy.size() + 1u, 1u); if (guest) WriteCString(guest, copy); Ret(guest); return true;
        }
        if (name == "gethostbyname") {
            const std::string host = ReadCString(Arg(0), 4096);
            addrinfo hints{}; hints.ai_family = AF_INET; hints.ai_socktype = SOCK_STREAM;
            addrinfo* result = nullptr;
            if (::getaddrinfo(host.c_str(), nullptr, &hints, &result) != 0 || !result) { Ret(0); return true; }
            const sockaddr_in* sin = reinterpret_cast<const sockaddr_in*>(result->ai_addr);
            const std::uint32_t name_guest = Allocate(host.size() + 1u, 1u);
            const std::uint32_t aliases_guest = Allocate(4u, 4u);
            const std::uint32_t address_guest = Allocate(4u, 4u);
            const std::uint32_t list_guest = Allocate(8u, 4u);
            const std::uint32_t entry_guest = Allocate(20u, 4u);
            if (!name_guest || !aliases_guest || !address_guest || !list_guest || !entry_guest) { ::freeaddrinfo(result); Ret(0); return true; }
            WriteCString(name_guest, host); Write32(memory_, aliases_guest, 0u); std::memcpy(memory_.data() + address_guest, &sin->sin_addr, 4u);
            Write32(memory_, list_guest, address_guest); Write32(memory_, list_guest + 4u, 0u);
            Write32(memory_, entry_guest + 0u, name_guest); Write32(memory_, entry_guest + 4u, aliases_guest);
            Write<std::int32_t>(entry_guest + 8u, AF_INET); Write<std::int32_t>(entry_guest + 12u, 4); Write32(memory_, entry_guest + 16u, list_guest);
            ::freeaddrinfo(result); Ret(entry_guest); return true;
        }
        if (name == "gethostbyaddr") { Ret(0); return true; }
#endif
        return false;
    }

    bool DispatchCompatExtras(const std::string& name) {
        if (DispatchRealSockets(name)) return true;
        if (name == "getenv") {
            const std::string key = ReadCString(Arg(0), 1024);
            if (guest_environment_.empty()) {
                std::string home = "/data/user/0/me.jacqueb.cnr64poc";
                const std::size_t files = managed_dir_.find("/files/");
                if (files != std::string::npos) home = managed_dir_.substr(0, files);
                guest_environment_["HOME"] = home;
                guest_environment_["TMPDIR"] = home + "/cache";
                guest_environment_["LANG"] = "C";
                guest_environment_["LC_ALL"] = "C";
                guest_environment_["PATH"] = "/system/bin:/system/xbin";
            }
            const auto found = guest_environment_.find(key);
            if (found == guest_environment_.end()) { Ret(0); return true; }
            const std::uint32_t guest = Allocate(found->second.size() + 1u, 1u);
            if (guest) WriteCString(guest, found->second);
            Ret(guest); return true;
        }
        if (name == "setenv") {
            const std::string key = ReadCString(Arg(0), 1024), value = ReadCString(Arg(1), 8192);
            if (key.empty() || key.find('=') != std::string::npos) { SetErrno(22); Ret(0xffffffffu); return true; }
            if (Arg(2) || guest_environment_.find(key) == guest_environment_.end()) guest_environment_[key] = value;
            SetErrno(0); Ret(0); return true;
        }
        if (name == "unsetenv") {
            guest_environment_.erase(ReadCString(Arg(0), 1024)); SetErrno(0); Ret(0); return true;
        }
        if (name == "chdir" || name == "chmod") {
#if !defined(_WIN32)
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            errno = 0;
            const int rc = name == "chdir" ? ::chdir(path.c_str()) : ::chmod(path.c_str(), static_cast<mode_t>(Arg(1)));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno)); Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }

        if (name == "__android_log_write") {
            const std::string tag = ReadCString(Arg(1), 256);
            const std::string text = ReadCString(Arg(2), 4096);
#if defined(__ANDROID__)
            __android_log_write(static_cast<int>(Arg(0)), tag.empty() ? "CNR64GUEST" : tag.c_str(), text.c_str());
#endif
            if (diagnostic_formats.size() < 16) diagnostic_formats.push_back("__android_log_write: " + text);
            Ret(0); return true;
        }
        if (name == "__cxa_pure_virtual" || name == "__stack_chk_fail") {
            failed = true;
            fault_pc = jit ? jit->Regs()[15] : 0u;
            fault_exception = -3;
            if (file_trace.size() < 96) file_trace.push_back("fatal guest runtime call: " + name);
            if (jit) jit->HaltExecution();
            return true;
        }
        if (name == "qsort") return StartGuestQsort(Arg(0), Arg(1), Arg(2), Arg(3));

        if (name == "strsep") {
            const std::uint32_t string_ptr_ptr = Arg(0);
            if (!string_ptr_ptr || !Fits(string_ptr_ptr, 4u)) { Ret(0); return true; }
            const std::uint32_t current = Read<std::uint32_t>(string_ptr_ptr);
            if (!current) { Ret(0); return true; }
            const std::string delimiters = ReadCString(Arg(1));
            std::uint32_t cursor = current;
            while (Fits(cursor, 1u) && memory_[cursor] != 0 && delimiters.find(static_cast<char>(memory_[cursor])) == std::string::npos) ++cursor;
            if (Fits(cursor, 1u) && memory_[cursor] != 0) { memory_[cursor] = 0; Write32(memory_, string_ptr_ptr, cursor + 1u); }
            else Write32(memory_, string_ptr_ptr, 0u);
            Ret(current); return true;
        }
        if (name == "strtok" || name == "strtok_r") {
            std::uint32_t current = Arg(0);
            const std::string delimiters = ReadCString(Arg(1));
            std::uint32_t* state = &strtok_state_;
            std::uint32_t external_state = 0;
            if (name == "strtok_r") {
                if (!Arg(2) || !Fits(Arg(2), 4u)) { Ret(0); return true; }
                external_state = Read<std::uint32_t>(Arg(2));
                if (!current) current = external_state;
            } else if (!current) current = *state;
            if (!current) { Ret(0); return true; }
            while (Fits(current, 1u) && memory_[current] != 0 && delimiters.find(static_cast<char>(memory_[current])) != std::string::npos) ++current;
            if (!Fits(current, 1u) || memory_[current] == 0) {
                if (name == "strtok_r") Write32(memory_, Arg(2), 0u); else *state = 0;
                Ret(0); return true;
            }
            const std::uint32_t token = current;
            while (Fits(current, 1u) && memory_[current] != 0 && delimiters.find(static_cast<char>(memory_[current])) == std::string::npos) ++current;
            std::uint32_t next = 0;
            if (Fits(current, 1u) && memory_[current] != 0) { memory_[current] = 0; next = current + 1u; }
            if (name == "strtok_r") Write32(memory_, Arg(2), next); else *state = next;
            Ret(token); return true;
        }

        if (name == "wmemchr" || name == "wmemcmp" || name == "wmemcpy" || name == "wmemmove" || name == "wmemset") {
            const std::uint32_t count = name == "wmemcmp" ? Arg(2) : (name == "wmemset" ? Arg(2) : Arg(2));
            const std::size_t bytes = static_cast<std::size_t>(count) * 4u;
            if (name == "wmemchr") {
                if (!Fits(Arg(0), bytes)) return false;
                std::uint32_t result = 0;
                for (std::uint32_t i = 0; i < count; ++i) if (Read<std::uint32_t>(Arg(0) + i * 4u) == Arg(1)) { result = Arg(0) + i * 4u; break; }
                Ret(result); return true;
            }
            if (name == "wmemcmp") {
                if (!Fits(Arg(0), bytes) || !Fits(Arg(1), bytes)) return false;
                int result = 0;
                for (std::uint32_t i = 0; i < count; ++i) {
                    const std::uint32_t a = Read<std::uint32_t>(Arg(0) + i * 4u), b = Read<std::uint32_t>(Arg(1) + i * 4u);
                    if (a != b) { result = a < b ? -1 : 1; break; }
                }
                Ret(static_cast<std::uint32_t>(result)); return true;
            }
            if (name == "wmemset") {
                if (!Fits(Arg(0), bytes)) return false;
                for (std::uint32_t i = 0; i < count; ++i) Write32(memory_, Arg(0) + i * 4u, Arg(1));
                Ret(Arg(0)); return true;
            }
            if (!Fits(Arg(0), bytes) || !Fits(Arg(1), bytes)) return false;
            if (name == "wmemmove") std::memmove(memory_.data() + Arg(0), memory_.data() + Arg(1), bytes);
            else std::memcpy(memory_.data() + Arg(0), memory_.data() + Arg(1), bytes);
            Ret(Arg(0)); return true;
        }

        if (name == "lrand48") {
            rand48_state_ = (0x5deece66dULL * rand48_state_ + 0xbULL) & ((1ULL << 48) - 1ULL);
            Ret(static_cast<std::uint32_t>(rand48_state_ >> 17)); return true;
        }
        if (name == "srand48") {
            rand48_state_ = (static_cast<std::uint64_t>(Arg(0)) << 16) | 0x330eULL;
            Ret(0); return true;
        }

        if (name == "gmtime" || name == "localtime") {
            if (!Arg(0) || !Fits(Arg(0), 4u)) { Ret(0); return true; }
            const std::time_t value = static_cast<std::time_t>(static_cast<std::int32_t>(Read<std::uint32_t>(Arg(0))));
            std::tm copy{};
            const std::tm* source = name == "gmtime" ? std::gmtime(&value) : std::localtime(&value);
            if (!source) { Ret(0); return true; }
            copy = *source;
            const std::uint32_t guest = name == "gmtime" ? kBootstrapRuntimeBase + 0x400u : kBootstrapRuntimeBase + 0x440u;
            if (!WriteGuestTm32(guest, copy)) { Ret(0); return true; }
            Ret(guest); return true;
        }
        if (name == "mktime") {
            std::tm value{};
            if (!ReadGuestTm32(Arg(0), value)) { Ret(0xffffffffu); return true; }
            const std::time_t result = std::mktime(&value);
            WriteGuestTm32(Arg(0), value);
            Ret(static_cast<std::uint32_t>(static_cast<std::int64_t>(result))); return true;
        }
        if (name == "strftime") {
            const std::uint32_t dst = Arg(0), capacity = Arg(1);
            if (!dst || capacity == 0 || !Fits(dst, capacity)) { Ret(0); return true; }
            std::tm value{};
            if (!ReadGuestTm32(Arg(3), value)) { Ret(0); return true; }
            std::vector<char> buffer(capacity);
            const std::size_t written = std::strftime(buffer.data(), buffer.size(), ReadCString(Arg(2)).c_str(), &value);
            if (written) std::memcpy(memory_.data() + dst, buffer.data(), written + 1u);
            else memory_[dst] = 0;
            Ret(static_cast<std::uint32_t>(written)); return true;
        }

        if (name == "sscanf") {
            Ret(static_cast<std::uint32_t>(ScanGuestText(ReadCString(Arg(0), 65536), ReadCString(Arg(1), 4096), 2u)));
            return true;
        }
        if (name == "fscanf") {
            std::FILE* file = LookupFileHandle(Arg(0));
            if (!file) { Ret(0xffffffffu); return true; }
            char buffer[4096]{};
            if (!std::fgets(buffer, sizeof(buffer), file)) { Ret(0xffffffffu); return true; }
            Ret(static_cast<std::uint32_t>(ScanGuestText(buffer, ReadCString(Arg(1), 4096), 2u)));
            return true;
        }

        if (name == "fdopen") {
#if !defined(_WIN32)
            const int fd = static_cast<int>(Arg(0));
            const std::string mode = ReadCString(Arg(1), 32);
            errno = 0;
            std::FILE* file = ::fdopen(fd, mode.c_str());
            SetErrno(file ? 0u : static_cast<std::uint32_t>(errno));
            Ret(RegisterFileHandle(file, std::string("<fd:") + std::to_string(fd) + ">"));
#else
            SetErrno(38); Ret(0);
#endif
            return true;
        }
        if (name == "putc" || name == "getwc" || name == "putwc" || name == "ungetwc") {
            const std::uint32_t handle = (name == "getwc") ? Arg(0) : Arg(1);
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { Ret(0xffffffffu); return true; }
            int rc = -1;
            if (name == "getwc") rc = std::fgetc(file);
            else if (name == "putc" || name == "putwc") rc = std::fputc(static_cast<int>(Arg(0) & 0xffu), file);
            else rc = std::ungetc(static_cast<int>(Arg(0) & 0xffu), file);
            Ret(static_cast<std::uint32_t>(rc)); return true;
        }
        if (name == "setvbuf") { Ret(0); return true; }

        if (name == "opendir") {
#if !defined(_WIN32)
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            errno = 0;
            DIR* dir = ::opendir(path.c_str());
            if (!dir) { SetErrno(static_cast<std::uint32_t>(errno)); Ret(0); return true; }
            const std::uint32_t handle = next_dir_handle_++;
            dir_handles_[handle] = dir;
            dir_guest_entries_[handle] = Allocate(280u, 8u);
            SetErrno(0); Ret(handle);
#else
            SetErrno(38); Ret(0);
#endif
            return true;
        }
        if (name == "readdir") {
#if !defined(_WIN32)
            const auto found = dir_handles_.find(Arg(0));
            if (found == dir_handles_.end()) { SetErrno(9); Ret(0); return true; }
            errno = 0;
            dirent* entry = ::readdir(found->second);
            if (!entry) { SetErrno(static_cast<std::uint32_t>(errno)); Ret(0); return true; }
            const std::uint32_t guest = dir_guest_entries_[Arg(0)];
            if (!guest || !Fits(guest, 280u)) return false;
            std::memset(memory_.data() + guest, 0, 280u);
            Write<std::uint64_t>(guest + 0u, static_cast<std::uint64_t>(entry->d_ino));
            Write<std::int64_t>(guest + 8u, static_cast<std::int64_t>(entry->d_off));
            Write<std::uint16_t>(guest + 16u, 280u);
            Write<std::uint8_t>(guest + 18u, static_cast<std::uint8_t>(entry->d_type));
            WriteCString(guest + 19u, entry->d_name, 256u);
            SetErrno(0); Ret(guest);
#else
            SetErrno(38); Ret(0);
#endif
            return true;
        }
        if (name == "closedir") {
#if !defined(_WIN32)
            const auto found = dir_handles_.find(Arg(0));
            if (found == dir_handles_.end()) { SetErrno(9); Ret(0xffffffffu); return true; }
            const int rc = ::closedir(found->second);
            dir_handles_.erase(found);
            dir_guest_entries_.erase(Arg(0));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }

        if (name == "dup2" || name == "fsync" || name == "ftruncate" || name == "truncate" ||
            name == "mkdir" || name == "rmdir" || name == "unlink" || name == "rename" || name == "mkstemp" ||
            name == "flock" || name == "utime" || name == "writev" || name == "sendfile") {
#if !defined(_WIN32)
            errno = 0;
            int rc = -1;
            if (name == "dup2") rc = ::dup2(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)));
            else if (name == "fsync") rc = ::fsync(static_cast<int>(Arg(0)));
            else if (name == "ftruncate") rc = ::ftruncate(static_cast<int>(Arg(0)), static_cast<off_t>(static_cast<std::int32_t>(Arg(1))));
            else if (name == "truncate") rc = ::truncate(NormalizeGuestPath(ReadCString(Arg(0))).c_str(), static_cast<off_t>(static_cast<std::int32_t>(Arg(1))));
            else if (name == "mkdir") rc = ::mkdir(NormalizeGuestPath(ReadCString(Arg(0))).c_str(), static_cast<mode_t>(Arg(1)));
            else if (name == "rmdir") rc = ::rmdir(NormalizeGuestPath(ReadCString(Arg(0))).c_str());
            else if (name == "unlink") rc = ::unlink(NormalizeGuestPath(ReadCString(Arg(0))).c_str());
            else if (name == "rename") rc = ::rename(NormalizeGuestPath(ReadCString(Arg(0))).c_str(), NormalizeGuestPath(ReadCString(Arg(1))).c_str());
            else if (name == "mkstemp") {
                const std::string original = NormalizeGuestPath(ReadCString(Arg(0)));
                std::vector<char> path(original.begin(), original.end()); path.push_back(0);
                rc = ::mkstemp(path.data());
                if (rc >= 0) WriteCString(Arg(0), path.data());
            } else if (name == "flock") rc = ::flock(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)));
            else if (name == "utime") {
                struct utimbuf times{}; struct utimbuf* ptr = nullptr;
                if (Arg(1) && Fits(Arg(1), 8u)) { times.actime = static_cast<time_t>(Read<std::int32_t>(Arg(1))); times.modtime = static_cast<time_t>(Read<std::int32_t>(Arg(1) + 4u)); ptr = &times; }
                rc = ::utime(NormalizeGuestPath(ReadCString(Arg(0))).c_str(), ptr);
            } else if (name == "writev") {
                const std::uint32_t count = Arg(2), vector = Arg(1);
                if (count > 1024u || (count && !Fits(vector, static_cast<std::size_t>(count) * 8u))) return false;
                ssize_t total = 0;
                for (std::uint32_t i = 0; i < count; ++i) {
                    const std::uint32_t base = Read<std::uint32_t>(vector + i * 8u), len = Read<std::uint32_t>(vector + i * 8u + 4u);
                    if (!Fits(base, len)) return false;
                    const ssize_t part = ::write(static_cast<int>(Arg(0)), memory_.data() + base, len);
                    if (part < 0) { total = -1; break; }
                    total += part;
                    if (static_cast<std::uint32_t>(part) != len) break;
                }
                rc = static_cast<int>(total);
            } else if (name == "sendfile") {
                const int out_fd = static_cast<int>(Arg(0)), in_fd = static_cast<int>(Arg(1));
                std::uint32_t offset = 0; if (Arg(2) && Fits(Arg(2), 4u)) offset = Read<std::uint32_t>(Arg(2));
                std::size_t remaining = Arg(3), sent = 0; std::array<std::uint8_t, 16384> buffer{};
                while (remaining) {
                    const std::size_t chunk = std::min<std::size_t>(remaining, buffer.size());
                    const ssize_t got = ::pread(in_fd, buffer.data(), chunk, static_cast<off_t>(offset + sent));
                    if (got <= 0) { if (got < 0 && sent == 0) sent = static_cast<std::size_t>(-1); break; }
                    const ssize_t wrote = ::write(out_fd, buffer.data(), static_cast<std::size_t>(got));
                    if (wrote < 0) { if (sent == 0) sent = static_cast<std::size_t>(-1); break; }
                    sent += static_cast<std::size_t>(wrote); remaining -= static_cast<std::size_t>(wrote);
                    if (wrote != got) break;
                }
                if (Arg(2) && sent != static_cast<std::size_t>(-1)) Write32(memory_, Arg(2), offset + static_cast<std::uint32_t>(sent));
                rc = sent == static_cast<std::size_t>(-1) ? -1 : static_cast<int>(sent);
            }
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }

        if (name == "getpriority") { Ret(0); return true; }
        if (name == "setpriority" || name == "setresuid") { Ret(0); return true; }
        if (name == "getgrgid" || name == "getgrnam" || name == "getpwnam" || name == "getpwuid") { Ret(0); return true; }
        if (name == "fork" || name == "execl" || name == "execv" || name == "execve" || name == "system" ||
            name == "popen" || name == "pclose" || name == "waitpid" || name == "ptrace") {
            SetErrno(name == "ptrace" ? 1u : 38u); // EPERM or ENOSYS
            Ret(0xffffffffu); return true;
        }

        if (name == "SHA1Init" || name == "SHA1Update" || name == "SHA1Final") {
            if (name == "SHA1Init") {
                Sha1State32 state{}; Sha1InitState(state); sha1_states_[Arg(0)] = state; Ret(0); return true;
            }
            auto found = sha1_states_.find(name == "SHA1Final" ? Arg(1) : Arg(0));
            if (found == sha1_states_.end()) { Ret(0); return true; }
            if (name == "SHA1Update") {
                if (!Fits(Arg(1), Arg(2))) return false;
                Sha1UpdateState(found->second, memory_.data() + Arg(1), Arg(2)); Ret(0); return true;
            }
            if (!Arg(0) || !Fits(Arg(0), 20u)) return false;
            std::uint8_t digest[20]{}; Sha1FinalState(found->second, digest);
            std::memcpy(memory_.data() + Arg(0), digest, sizeof(digest)); sha1_states_.erase(found); Ret(0); return true;
        }

        return false;
    }
