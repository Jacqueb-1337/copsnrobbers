#include "cocos2dx_jni_marshaller.h"
#include "cocos2dx_compat_module.h"
#include <cassert>
#include <string_view>
int main() {
    using namespace pv7::compat::cocos2dx;
    assert(KnownClass("org/cocos2dx/lib/Cocos2dxBitmap"));
    assert(!KnownClass("org/cocos2dx/lib/DoesNotExist"));
    assert(KnownMethodCount() == 5);
    const auto* text = ResolveMethod("org/cocos2dx/lib/Cocos2dxBitmap",
        "createTextBitmap", "(Ljava/lang/String;Ljava/lang/String;IIII)V");
    assert(text && !text->native_callback);
    assert(!ResolveMethod("org/cocos2dx/lib/Cocos2dxBitmap",
        "createTextBitmap", "(I)V"));
    const auto* callback = ResolveMethod("org/cocos2dx/lib/Cocos2dxBitmap",
        "nativeInitBitmapDC", "(II[B)V");
    assert(callback && callback->native_callback);
    std::vector<std::uint8_t> guest(128,0);
    const std::uint32_t words[] = {0xc100,0xc104,24,3,320,64};
    for (std::size_t i=0;i<6;++i)
        for (unsigned b=0;b<4;++b)
            guest[32+i*4+b]=static_cast<std::uint8_t>(words[i] >> (8*b));
    std::unordered_map<std::uint32_t,std::string> strings{
        {0xc100,"Test"},{0xc104,"Arial"}};
    BitmapJniMarshaller marshaller;
    BitmapTextRequest request;
    std::string error;
    assert(marshaller.Decode(guest,32,strings,request,error));
    assert(request.text=="Test" && request.font=="Arial");
    assert(request.font_size==24 && request.alignment==3);
    assert(request.width==320 && request.height==64);
    assert(!marshaller.Decode(guest,127,strings,request,error));
    assert(!error.empty());
    return 0;
}
