#include "arm32_guest_image.h"
#include <fstream>
#include <iostream>
#include <iterator>

int main(int argc,char**argv){
    if(argc!=2){std::cerr<<"Usage: pv7-arm32-elf-probe libgame.so\n";return 2;}
    std::ifstream file(argv[1],std::ios::binary);
    if(!file){std::cerr<<"Cannot read guest library\n";return 1;}
    std::vector<std::uint8_t> data(std::istreambuf_iterator<char>{file},{});
    pv7::Arm32GuestImage guest;
    std::string error;
    if(!guest.Load(data,0x10000000,error)){
        std::cerr<<"ELF mapping failed: "<<error<<"\n";return 1;
    }
    std::cout<<"Mapped ELF32 guest, span="<<guest.span()<<" bytes\n";
    for(const auto&dep:guest.dependencies())std::cout<<"NEEDED "<<dep<<"\n";
    for(const auto*name:{"JNI_OnLoad",
        "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeInit",
        "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender"}) {
        auto symbol=guest.FindExport(name);
        if(symbol) std::cout<<"EXPORT "<<name<<" @0x"<<std::hex<<*symbol<<std::dec<<"\n";
    }
    // No ARM execution or fake imported-library shims here.
    return 0;
}
