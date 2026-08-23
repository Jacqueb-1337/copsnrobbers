#include <iostream>
#include <string>

#include "a32_test_core.h"

int main(int argc, char** argv) {
    if (argc < 5) {
        std::cerr << "usage: cnr64-a32-poc <libmain.so> <libunity.so> <libmono.so> <managed-dir>\n";
        return 2;
    }

    const A32SelfTestResult result = RunA32SelfTest(argv[1], argv[2], argv[3], argv[4], nullptr);
    std::cout << result.report << '\n';
    return result.ok ? 0 : 1;
}
