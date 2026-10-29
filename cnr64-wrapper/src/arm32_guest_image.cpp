#include "arm32_guest_image.h"
#include <algorithm>
#include <cstring>
#include <limits>

namespace pv7 {
namespace {
bool U16(const std::vector<std::uint8_t>& a,std::size_t n,std::uint16_t& x){
 if(n>a.size()||a.size()-n<2)return false;
 x=a[n]|(a[n+1]<<8);return true;
}
bool U32(const std::vector<std::uint8_t>& a,std::size_t n,std::uint32_t& x){
 if(n>a.size()||a.size()-n<4)return false;
 x=a[n]|(std::uint32_t(a[n+1])<<8)|(std::uint32_t(a[n+2])<<16)|
   (std::uint32_t(a[n+3])<<24);return true;
}
}
bool Arm32GuestImage::CopyToGuestMemory(
    std::span<std::uint8_t> guest, std::string& error) const {
    if (memory_.empty() || base_ > guest.size() ||
        memory_.size() > guest.size() - base_) {
        error = "Mapped ARM32 image exceeds guest memory";
        return false;
    }
    std::copy(memory_.begin(), memory_.end(), guest.begin() + base_);
    return true;
}
bool Arm32GuestImage::Range(std::uint32_t p,std::size_t n)const{
 return p<=memory_.size()&&n<=memory_.size()-p;
}
bool Arm32GuestImage::Read32(std::uint32_t p,std::uint32_t& x)const{
 return Range(p,4)&&U32(memory_,p,x);
}
bool Arm32GuestImage::Write32(std::uint32_t p,std::uint32_t x){
 if(!Range(p,4))return false;
 for(int i=0;i<4;++i)memory_[p+i]=std::uint8_t(x>>(8*i));
 return true;
}
std::optional<std::uint32_t> Arm32GuestImage::ReadWord(std::uint32_t a)const{
 std::uint32_t x;
 return a>=base_&&Read32(a-base_,x)?std::optional{x}:std::nullopt;
}
std::optional<std::uint32_t> Arm32GuestImage::FindExport(std::string_view n)const{
 auto it=exports_.find(std::string(n));
 return it==exports_.end()?std::nullopt:std::optional{it->second};
}
bool Arm32GuestImage::Symbol(std::uint32_t i,std::string& n,std::uint32_t& a,bool& defined)const{
 if(!dyn_.symtab||dyn_.syment<16||i>1000000)return false;
 auto p=std::uint64_t(dyn_.symtab)+std::uint64_t(i)*dyn_.syment;
 if(p>memory_.size()||memory_.size()-p<16)return false;
 std::uint32_t off=0,v=0;std::uint16_t section=0;
 if(!U32(memory_,p,off)||!U32(memory_,p+4,v)||!U16(memory_,p+14,section)||
    off>=dyn_.strsz)return false;
 const char* start=(const char*)memory_.data()+dyn_.strtab+off;
 const char* end=(const char*)memchr(start,0,dyn_.strsz-off);
 if(!end)return false;
 n.assign(start,end);defined=section!=0;a=defined?base_+v:0;return true;
}
bool Arm32GuestImage::Load(const std::vector<std::uint8_t>& elf,
                            std::uint32_t base,std::string& error){
 *this={};
 if(elf.size()<52||elf.size()>256u*1024*1024||memcmp(elf.data(),"\177ELF",4)||
    elf[4]!=1||elf[5]!=1){error="Not ELF32 little-endian";return false;}
 std::uint16_t type=0,machine=0,entsize=0,count=0;
 std::uint32_t phoff=0;
 if(!U16(elf,16,type)||type!=3||!U16(elf,18,machine)||machine!=40||
    !U32(elf,28,phoff)||!U16(elf,42,entsize)||!U16(elf,44,count)||
    entsize<32||!count||count>4096||phoff>elf.size()||
    std::uint64_t(count)*entsize>elf.size()-phoff){
    error="Invalid ARM32 ET_DYN program headers";return false;
 }
 base_=base;
 struct Seg{std::uint32_t v,off,size;};
 std::vector<Seg> load;
 std::vector<Seg> dynamic;
 std::uint32_t span=0;
 for(std::uint32_t i=0;i<count;++i){
    const auto p=std::size_t(phoff)+i*entsize;
    std::uint32_t kind=0,off=0,v=0,sz=0,mem=0;
    if(!U32(elf,p,kind)||!U32(elf,p+4,off)||!U32(elf,p+8,v)||
       !U32(elf,p+16,sz)||!U32(elf,p+20,mem)){error="Bad program header";return false;}
    if(kind!=1&&kind!=2)continue;
    if(sz>mem||off>elf.size()||sz>elf.size()-off||
       v>256u*1024*1024||mem>256u*1024*1024-v||
       base>std::numeric_limits<std::uint32_t>::max()-(v+mem)){
       error="Invalid guest segment bounds";return false;
    }
    if(kind==1){load.push_back({v,off,sz});span=std::max(span,v+mem);}
    else dynamic.push_back({v,off,sz});
 }
 if(!span||load.empty()){error="No PT_LOAD segments";return false;}
 memory_.assign(span,0);
 for(auto s:load)memcpy(memory_.data()+s.v,elf.data()+s.off,s.size);
 for(auto s:dynamic){
   if(!Range(s.v,s.size)||s.size%8){error="Bad ELF PT_DYNAMIC";return false;}
   for(std::uint32_t p=0;p<s.size;p+=8){
     std::uint32_t tag=0,val=0;
     if(!Read32(s.v+p,tag)||!Read32(s.v+p+4,val))break;
     if(!tag)break;
     switch(tag){
       case 4:dyn_.hash=val;break;case 5:dyn_.strtab=val;break;
       case 6:dyn_.symtab=val;break;case 10:dyn_.strsz=val;break;
       case 11:dyn_.syment=val;break;case 17:dyn_.rel=val;break;
       case 18:dyn_.relsz=val;break;case 19:dyn_.relent=val;break;
       case 23:dyn_.jmprel=val;break;case 2:dyn_.pltrelsz=val;break;
       case 12:dyn_.init=val;break;case 25:dyn_.init_array=val;break;
       case 27:dyn_.init_arraysz=val;break;case 1:load_ranges_.push_back(val);break;
       default:break;
     }
   }
 }
 if(!dyn_.strtab||!dyn_.strsz||!Range(dyn_.strtab,dyn_.strsz)){
   error="Invalid dynamic string table";return false;
 }
 for(auto off:load_ranges_){
   if(off>=dyn_.strsz){error="Invalid DT_NEEDED offset";return false;}
   const char* a=(const char*)memory_.data()+dyn_.strtab+off;
   const char* b=(const char*)memchr(a,0,dyn_.strsz-off);
   if(!b){error="Unterminated DT_NEEDED";return false;}
   dependencies_.emplace_back(a,b);
 }
 load_ranges_.clear();
 std::uint32_t symbols=0;
 if(dyn_.hash&&!Read32(dyn_.hash+4,symbols)){error="Invalid dynamic hash";return false;}
 if(symbols>1000000){error="Too many exports";return false;}
 for(std::uint32_t i=0;i<symbols;++i){
   std::string name;std::uint32_t addr=0;bool defined=false;
   if(!Symbol(i,name,addr,defined)){error="Malformed dynamic symbol";return false;}
   if(defined&&!name.empty())exports_.emplace(name,addr);
 }
 if(dyn_.init)constructors_.push_back(base_+dyn_.init);
 if(dyn_.init_arraysz){
   if(!dyn_.init_array||dyn_.init_arraysz%4||
      !Range(dyn_.init_array,dyn_.init_arraysz)){
      error="Malformed initializer array";return false;
   }
 }
 return true;
}
} // namespace pv7
