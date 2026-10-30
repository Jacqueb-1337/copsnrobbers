#include "cocos2dx_compat_module.h"
#include <array>

namespace pv7::compat::cocos2dx {
namespace {
// Signatures verified against Best Park's original APK, cross-referenced with
// historical cocos2d-x Android JNI source. Keep all other calls fail-closed.
// The original game's ARM32 libgame.so remains authoritative and unmodified.
constexpr std::array<JniMethodContract, 5> methods{{
    {"org/cocos2dx/lib/Cocos2dxBitmap", "createTextBitmap",
      "(Ljava/lang/String;Ljava/lang/String;IIII)V", false},
    {"org/cocos2dx/lib/Cocos2dxBitmap", "getFontSizeAccordingHeight",
      "(I)I", false},
    {"org/cocos2dx/lib/Cocos2dxBitmap", "getStringWithEllipsis",
      "(Ljava/lang/String;FF)Ljava/lang/String;", false},
    {"org/cocos2dx/lib/Cocos2dxBitmap", "setContext",
      "(Landroid/content/Context;)V", false},
    {"org/cocos2dx/lib/Cocos2dxBitmap", "nativeInitBitmapDC",
      "(II[B)V", true}
}};
}
const char* Version() { return "0.1.0-source-contract"; }
std::size_t KnownMethodCount() { return methods.size(); }
const JniMethodContract* KnownMethods() { return methods.data(); }
bool KnownClass(std::string_view name) {
    for (const auto& method : methods) {
        if (method.class_name == name) return true;
    }
    return false;
}
const JniMethodContract* ResolveMethod(std::string_view class_name,
                                        std::string_view method_name,
                                        std::string_view signature) {
    for (const auto& method : methods) {
        if (method.class_name == class_name &&
            method.method_name == method_name &&
            method.signature == signature) return &method;
    }
    return nullptr;
}
}
