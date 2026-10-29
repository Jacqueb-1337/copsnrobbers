#include "arm32_guest_image.h"

namespace pv7 {
bool Arm32GuestImage::RelocateTable(std::uint32_t address,std::uint32_t bytes,
    const SymbolResolver& resolver,std::string& error) {
    if (!bytes) return true;
    if (dyn_.relent!=8 || bytes%8 || !Range(address,bytes)) {
        error="Bad ELF REL table"; return false;
    }
    for (std::uint32_t i=0;i<bytes;i+=8) {
        std::uint32_t location=0,info=0,addend=0;
        if (!Read32(address+i,location) || !Read32(address+i+4,info) ||
            !Read32(location,addend)) {
            error="Relocation out of guest range"; return false;
        }
        const auto type=info&255u;
        if (!type) continue;
        std::uint32_t symbol=0;
        if (type!=23) {
            std::string name;
            bool defined=false;
            if (!Symbol(info>>8,name,symbol,defined)) {
                error="Bad import symbol"; return false;
            }
            if (!defined) {
                const auto symoff = std::uint64_t(dyn_.symtab) +
                    std::uint64_t(info>>8)*dyn_.syment + 12;
                if (symoff >= memory_.size()) {
                    error="Bad guest symbol info"; return false;
                }
                imports_.push_back(Arm32GuestImport{
                    name,
                    static_cast<std::uint8_t>(memory_[symoff] & 0x0fu),
                    static_cast<std::uint8_t>(type)
                });
                auto resolved=resolver?resolver(name):std::nullopt;
                if (!resolved) {
                    unresolved_.push_back(name);
                    error="Missing guest import: "+name; return false;
                }
                symbol=*resolved;
            }
        }
        std::uint32_t value=0;
        switch(type) {
          case 23: value=base_+addend; break;
          case 2: value=symbol+addend; break;
          case 3: value=symbol+addend-(base_+location); break;
          case 21: case 22: value=symbol; break;
          default: error="Unsupported ARM relocation"; return false;
        }
        if (!Write32(location,value)) return false;
    }
    return true;
}
bool Arm32GuestImage::Relocate(const SymbolResolver& resolver,std::string& error) {
    unresolved_.clear();
    imports_.clear();
    if (!RelocateTable(dyn_.rel,dyn_.relsz,resolver,error)) return false;
    if (!RelocateTable(dyn_.jmprel,dyn_.pltrelsz,resolver,error)) return false;
    constructors_.clear();
    if (dyn_.init) constructors_.push_back(base_+dyn_.init);
    for (std::uint32_t i=0;i<dyn_.init_arraysz;i+=4) {
        std::uint32_t address=0;
        if (!Read32(dyn_.init_array+i,address)) {
            error="Invalid constructor entry"; return false;
        }
        if (address) constructors_.push_back(address);
    }
    return true;
}
} // namespace pv7
