#include "cocos2dx_guest_math.h"
#include <bit>
#include <cmath>
#include "cocos2dx_jni_refs.h"
#include "cocos2dx_guest_jni_arrays.h"
#include "cocos2dx_bitmap_pixels.h"
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
    BitmapPixels bitmap;
    std::vector<std::uint8_t> pixels(4*3*2,0x7f);
    assert(ReadBitmapPixels(3,2,pixels,bitmap,error));
    assert(bitmap.width==3 && bitmap.height==2);
    assert(bitmap.bytes==pixels);
    assert(!ReadBitmapPixels(3,2,std::span<const std::uint8_t>(pixels.data(),23),bitmap,error));
    assert(!ReadBitmapPixels(50000,50000,pixels,bitmap,error));
    assert(bitmap.bytes==pixels); // failed conversion never damages last frame
    GuestJniByteArrays arrays;
    std::uint32_t arrayHandle=0;
    assert(arrays.Create(bitmap,arrayHandle,error));
    assert(arrayHandle!=0);
    std::int32_t byteCount=0;
    assert(arrays.Length(arrayHandle,byteCount) && byteCount==24);
    std::uint8_t sample[4]{};
    assert(arrays.CopyRegion(arrayHandle,2,4,sample));
    for(auto byte:sample) assert(byte==0x7f);
    assert(!arrays.CopyRegion(arrayHandle,22,4,sample));
    assert(!arrays.CopyRegion(arrayHandle,-1,2,sample));
    assert(!arrays.CopyRegion(arrayHandle,0,2,nullptr));
    assert(arrays.Release(arrayHandle));
    assert(!arrays.Length(arrayHandle,byteCount));
    assert(!arrays.Release(arrayHandle));
    GuestJniRefs refs;
    assert(refs.RegisterLocal(0xc010,JniRefKind::Class));
    assert(refs.RefType(0xc010)==1);
    assert(refs.RegisterLocal(0xc100,JniRefKind::String));
    std::uint32_t globalAlias=0, localAlias=0;
    assert(refs.Duplicate(0xc100,JniRefScope::Global,globalAlias,error));
    assert(refs.Duplicate(0xc100,JniRefScope::Local,localAlias,error));
    assert(globalAlias!=localAlias && refs.IsSame(globalAlias,localAlias));
    assert(refs.RefType(globalAlias)==2);
    assert(refs.Delete(0xc100,JniRefScope::Local));
    assert(refs.IsSame(globalAlias,localAlias));
    assert(!refs.Delete(globalAlias,JniRefScope::Local));
    assert(refs.Delete(localAlias,JniRefScope::Local));
    assert(refs.Delete(globalAlias,JniRefScope::Global));
    assert(!refs.Resolve(globalAlias));
    assert(refs.IsSame(0,0));
    assert(!refs.IsSame(0,0xc010));
    // ARM32 base AAPCS: sin(pi/6) passes the double in r0:r1.
    std::array<std::uint32_t,16> mathRegs{};
    const auto putDouble=[&](int base,double value) {
        const auto bits=std::bit_cast<std::uint64_t>(value);
        mathRegs[base]=static_cast<std::uint32_t>(bits);
        mathRegs[base+1]=static_cast<std::uint32_t>(bits>>32);
    };
    const auto getDouble=[&]() {
        const auto bits=static_cast<std::uint64_t>(mathRegs[0]) |
                        (static_cast<std::uint64_t>(mathRegs[1])<<32);
        return std::bit_cast<double>(bits);
    };
    putDouble(0,3.14159265358979323846/6);
    assert(DispatchGuestMath("sin",mathRegs));
    assert(std::fabs(getDouble()-0.5)<1e-12);
    putDouble(0,2.0);
    putDouble(2,5.0);
    assert(DispatchGuestMath("pow",mathRegs));
    assert(getDouble()==32.0);
    mathRegs[0]=std::bit_cast<std::uint32_t>(0.25f);
    assert(DispatchGuestMath("sinf",mathRegs));
    assert(std::fabs(std::bit_cast<float>(mathRegs[0])-std::sin(0.25f))<1e-7f);
    const auto saved=mathRegs;
    assert(!DispatchGuestMath("unsupported_libm",mathRegs));
    assert(mathRegs==saved);
    return 0;
}
