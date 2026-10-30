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
    return 0;
}
