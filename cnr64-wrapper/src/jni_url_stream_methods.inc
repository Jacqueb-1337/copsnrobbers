    struct UrlStreamProxy {
        std::string url, method = "GET";
        std::map<std::string,std::string> headers;
        int connect_ms = 15000, read_ms = 30000, transfer = 0;
        unsigned refs = 1;
        std::uint64_t received = 0;
        enum Kind { Url, Connection, Input } kind = Url;
    };
    std::unordered_map<std::uint32_t,UrlStreamProxy> url_streams_;
    std::uint32_t next_url_stream_ = 0x0f600000u;
#if defined(__ANDROID__)
    jclass HostStreamClass(JNIEnv* h) {
        auto c=h->FindClass("me/jacqueb/cnr64poc/HostStream");
        if(!c && h->ExceptionCheck())h->ExceptionClear();
        if(!c)c=h->FindClass("app/projectv7/wrapper/HostStream");
        if(!c && h->ExceptionCheck())h->ExceptionClear();
        return c;
    }
    void CloseHostStream(int transfer) {
        auto h=static_cast<JNIEnv*>(host_jni_env_); if(!h || transfer<=0)return;
        auto c=HostStreamClass(h);if(!c)return;
        auto m=h->GetStaticMethodID(c,"close","(I)V");
        if(m)h->CallStaticVoidMethod(c,m,transfer);
        if(h->ExceptionCheck())h->ExceptionClear();h->DeleteLocalRef(c);
    }
#endif
    void CloseUrlStreams() {
#if defined(__ANDROID__)
        for(const auto& p:url_streams_)CloseHostStream(p.second.transfer);
#endif
        url_streams_.clear();
    }
    bool DispatchUrlStreamJni(std::uint32_t swi) {
        if(!jit)return false;
        auto put=[&](UrlStreamProxy p) {
            if(url_streams_.size()>=512u)return 0u;
            const auto id=next_url_stream_;next_url_stream_+=4u;
            url_streams_.emplace(id,std::move(p));return id;
        };
        if(swi==kSvcJniNewObject || swi==kSvcJniNewObjectV || swi==kSvcJniNewObjectA) {
            const auto owner=jni_method_classes.find(jit->Regs()[2]);
            if(JniClassNameFor(jit->Regs()[1])!="java/net/URL" &&
               (owner==jni_method_classes.end() || owner->second!="java/net/URL"))return false;
            if(JniMethodSignatureFor(jit->Regs()[2])!="(Ljava/lang/String;)V")return false;
            UrlStreamProxy p;p.url=JniStringValue(JniConstructorArg32(swi,0));
            jit->Regs()[0]=put(std::move(p));return true;
        }
        const auto found=url_streams_.find(jit->Regs()[1]);
        if(found==url_streams_.end())return false;
        auto& p=found->second;
        if(swi==kSvcJniNewGlobalRef) {++p.refs;jit->Regs()[0]=found->first;return true;}
        if(swi==kSvcJniDeleteLocalRef || swi==kSvcJniDeleteGlobalRef) {
            if(--p.refs==0u) {
#if defined(__ANDROID__)
                if(p.kind!=UrlStreamProxy::Url)CloseHostStream(p.transfer);
#endif
                url_streams_.erase(found);
            }jit->Regs()[0]=0;return true;
        }
        if(swi==kSvcJniGetObjectClass) {
            jit->Regs()[0]=GetOrCreateJniClassHandle(p.kind==UrlStreamProxy::Url?"java/net/URL":
                p.kind==UrlStreamProxy::Input?"java/io/InputStream":"java/net/HttpURLConnection");return true;
        }
        const bool object_call=swi==kSvcJniCallObjectMethod || swi==kSvcJniCallObjectMethodV || swi==kSvcJniCallObjectMethodA;
        const bool int_call=swi==kSvcJniCallIntMethod || swi==kSvcJniCallIntMethodV || swi==kSvcJniCallIntMethodA;
        const bool void_call=swi==kSvcJniCallVoidMethod || swi==kSvcJniCallVoidMethodV || swi==kSvcJniCallVoidMethodA;
        if(!object_call && !int_call && !void_call)return false;
        const auto method=JniMethodNameFor(jit->Regs()[2]);
        auto arg=[&](unsigned i){return JniArg32(jit,swi,i);};
        if(object_call && method=="openConnection" && p.kind==UrlStreamProxy::Url) {
            auto connection=p;connection.kind=UrlStreamProxy::Connection;connection.refs=1;
            jit->Regs()[0]=put(std::move(connection));return true;
        }
        if(void_call) {
            if(method=="setConnectTimeout")p.connect_ms=static_cast<int>(arg(0));
            else if(method=="setReadTimeout")p.read_ms=static_cast<int>(arg(0));
            else if(method=="setRequestMethod")p.method=JniStringValue(arg(0));
            else if(method=="setRequestProperty")p.headers[JniStringValue(arg(0))]=JniStringValue(arg(1));
            else if(method=="disconnect" || method=="close") {
#if defined(__ANDROID__)
                CloseHostStream(p.transfer);
#endif
            } else if(method!="setUseCaches")return false;
            jit->Regs()[0]=0;return true;
        }
#if defined(__ANDROID__)
        auto h=static_cast<JNIEnv*>(host_jni_env_);if(!h) {jit->Regs()[0]=int_call?0xffffffffu:0u;return true;}
        auto c=HostStreamClass(h);if(!c){jit->Regs()[0]=int_call?0xffffffffu:0u;return true;}
        auto finish=[&](std::uint32_t value) {
            if(h->ExceptionCheck()){h->ExceptionDescribe();h->ExceptionClear();value=int_call?0xffffffffu:0u;}
            h->DeleteLocalRef(c);jit->Regs()[0]=value;return true;
        };
        auto wait=[&]() {
            h->DeleteLocalRef(c);
            jit->Regs()[15]=swi==kSvcJniCallIntMethodA?kJniCallIntMethodAStub:
                           swi==kSvcJniCallIntMethodV?kJniCallIntMethodVStub:kJniCallIntMethodStub;
            cooperative_yield_requested_=true;jit->HaltExecution();return true;
        };
        if(int_call && method=="getResponseCode") {
            if(!p.transfer) {
                auto m=h->GetStaticMethodID(c,"open","(Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;II)I");
                auto stringClass=h->FindClass("java/lang/String");
                auto headers=h->NewObjectArray(static_cast<jsize>(p.headers.size()*2),stringClass,nullptr);
                unsigned i=0;for(const auto& kv:p.headers)for(const auto* value:{&kv.first,&kv.second}) {
                    auto v=h->NewStringUTF(value->c_str());h->SetObjectArrayElement(headers,i++,v);h->DeleteLocalRef(v);
                }
                auto url=h->NewStringUTF(p.url.c_str()),verb=h->NewStringUTF(p.method.c_str());
                p.transfer=h->CallStaticIntMethod(c,m,url,verb,headers,p.connect_ms,p.read_ms);
                __android_log_print(ANDROID_LOG_INFO,"CNR64POC","PV7STREAM open id=%d method=%s url=%s",p.transfer,p.method.c_str(),p.url.c_str());
                h->DeleteLocalRef(url);h->DeleteLocalRef(verb);h->DeleteLocalRef(headers);h->DeleteLocalRef(stringClass);
            }
            auto m=h->GetStaticMethodID(c,"status","(I)I");int status=h->CallStaticIntMethod(c,m,p.transfer);
            if(status==-2 && !h->ExceptionCheck())return wait();return finish(static_cast<std::uint32_t>(status));
        }
        if(object_call && method=="getInputStream") {
            auto input=p;input.kind=UrlStreamProxy::Input;input.refs=1;
            return finish(put(std::move(input)));
        }
        if((object_call && method=="getHeaderField") || (int_call && method=="getContentLength")) {
            const auto key=method=="getContentLength"?std::string("Content-Length"):JniStringValue(arg(0));
            auto m=h->GetStaticMethodID(c,"header","(ILjava/lang/String;)Ljava/lang/String;");
            auto k=h->NewStringUTF(key.c_str());auto v=static_cast<jstring>(h->CallStaticObjectMethod(c,m,p.transfer,k));h->DeleteLocalRef(k);
            std::string text;if(v){const char* chars=h->GetStringUTFChars(v,nullptr);if(chars)text=chars;
                if(chars)h->ReleaseStringUTFChars(v,chars);h->DeleteLocalRef(v);}
            return finish(int_call?static_cast<std::uint32_t>(text.empty()?-1:std::strtol(text.c_str(),nullptr,10)):
                          text.empty()?0u:NewJniString(text));
        }
        if(int_call && method=="read" && JniMethodSignatureFor(jit->Regs()[2])=="([B)I") {
            const auto buffer=arg(0);const auto a=jni_arrays_.find(buffer);
            if(a==jni_arrays_.end() || !Fits(buffer+16u,a->second.length))return finish(0xffffffffu);
            if(a->second.length==0)return finish(0);
            auto m=h->GetStaticMethodID(c,"read","(II)[B");
            auto bytes=static_cast<jbyteArray>(h->CallStaticObjectMethod(c,m,p.transfer,static_cast<jint>(a->second.length)));
            if(!bytes && !h->ExceptionCheck())return wait();
            if(!bytes)return finish(0xffffffffu);
            const auto n=h->GetArrayLength(bytes);
            if(!p.received || !n)__android_log_print(ANDROID_LOG_INFO,"CNR64POC","PV7STREAM read id=%d chunk=%d total=%llu",p.transfer,n,static_cast<unsigned long long>(p.received+n));
            p.received+=n;if(n)h->GetByteArrayRegion(bytes,0,n,reinterpret_cast<jbyte*>(memory_.data()+buffer+16));
            h->DeleteLocalRef(bytes);return finish(n?static_cast<std::uint32_t>(n):0xffffffffu);
        }
        h->DeleteLocalRef(c);
#endif
        return false;
    }
