#pragma once

#include <cstddef>
#include <string_view>

namespace pv7::compat::cocos2dx {

struct JniMethodContract {
    std::string_view class_name;
    std::string_view method_name;
    std::string_view signature;
    bool native_callback;
};

const char* Version();
std::size_t KnownMethodCount();
const JniMethodContract* KnownMethods();
bool KnownClass(std::string_view class_name);
const JniMethodContract* ResolveMethod(std::string_view class_name,
                                       std::string_view method_name,
                                       std::string_view signature);
}
