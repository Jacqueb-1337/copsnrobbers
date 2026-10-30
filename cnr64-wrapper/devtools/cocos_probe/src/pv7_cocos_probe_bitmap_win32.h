#pragma once
#include "../../../compat_modules/cocos2dx/cocos2dx_jni_marshaller.h"
#include "../../../compat_modules/cocos2dx/cocos2dx_bitmap_pixels.h"
#include <string>
namespace pv7::compat::cocos2dx {
bool RenderProbeBitmap(const BitmapTextRequest& request,
                       BitmapPixels& output, std::string& error);
}