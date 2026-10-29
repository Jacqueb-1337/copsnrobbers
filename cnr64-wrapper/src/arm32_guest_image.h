#pragma once

#include <cstdint>
#include <functional>
#include <optional>
#include <string>
#include <string_view>
#include <unordered_map>
#include <vector>

namespace pv7 {

// ARM32 ELF shared image mapped at a caller-selected guest address.
// A backend supplies guest-address thunks for unresolved system imports.
// This maps/relocates data; it does NOT execute guest ARM instructions.
class Arm32GuestImage {
 public:
    using SymbolResolver = std::function<std::optional<std::uint32_t>(std::string_view)>;
    bool Load(const std::vector<std::uint8_t>& elf, std::uint32_t image_base,
              std::string& error);
    bool Relocate(const SymbolResolver& external, std::string& error);
    std::optional<std::uint32_t> FindExport(std::string_view name) const;
    const std::vector<std::string>& dependencies() const { return dependencies_; }
    const std::vector<std::uint32_t>& constructors() const { return constructors_; }
    const std::vector<std::string>& unresolved_imports() const { return unresolved_; }
    std::uint32_t base() const { return base_; }
    std::uint32_t span() const { return static_cast<std::uint32_t>(memory_.size()); }
    const std::vector<std::uint8_t>& memory() const { return memory_; }
    std::optional<std::uint32_t> ReadWord(std::uint32_t guest_address) const;

 private:
    struct Dynamic {
        std::uint32_t strtab=0, strsz=0, symtab=0, syment=16;
        std::uint32_t hash=0, gnu_hash=0;
        std::uint32_t rel=0, relsz=0, relent=8, jmprel=0, pltrelsz=0;
        std::uint32_t init=0, init_array=0, init_arraysz=0;
    } dyn_;
    std::uint32_t base_ = 0;
    std::vector<std::uint8_t> memory_;
    std::vector<std::string> dependencies_;
    std::vector<std::uint32_t> constructors_;
    std::vector<std::string> unresolved_;
    std::unordered_map<std::string, std::uint32_t> exports_;
    std::vector<std::uint32_t> load_ranges_;
    bool Range(std::uint32_t vaddr, std::size_t size) const;
    bool Read32(std::uint32_t vaddr, std::uint32_t& out) const;
    bool Write32(std::uint32_t vaddr, std::uint32_t value);
    bool Symbol(std::uint32_t index, std::string& name, std::uint32_t& address,
                bool& defined) const;
    bool RelocateTable(std::uint32_t vaddr, std::uint32_t bytes,
                       const SymbolResolver& external, std::string& error);
};

} // namespace pv7
