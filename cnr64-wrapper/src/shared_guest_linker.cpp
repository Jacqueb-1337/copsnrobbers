#include "shared_guest_linker.h"

#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
#include "hotpatch_runtime.h"
#endif

#include <algorithm>
#include <array>
#include <cerrno>
#include <chrono>
#include <cctype>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <ctime>
#include <fstream>
#include <optional>
#include <sstream>
#include <string>
#include <thread>
#include <unordered_map>
#include <utility>
#include <vector>

#if defined(__ANDROID__)
#include <android/log.h>
#include <android/native_window.h>
#include <EGL/egl.h>
#include <GLES2/gl2.h>
#include <zlib.h>
#endif

#if !defined(_WIN32)
#include <arpa/inet.h>
#include <fcntl.h>
#include <netdb.h>
#include <netinet/in.h>
#include <poll.h>
#include <sys/epoll.h>
#include <sys/ioctl.h>
#include <sys/socket.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/file.h>
#include <sys/uio.h>
#include <utime.h>
#include <unistd.h>
#endif

#if defined(_WIN32)
extern "C" int _access(const char*, int);
#else
#include <dirent.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <unistd.h>
extern "C" int access(const char*, int);
#endif

#include "dynarmic/interface/A32/a32.h"
#include "dynarmic/interface/exclusive_monitor.h"

namespace {
constexpr std::uint32_t kMainBase = 0x00100000;
constexpr std::uint32_t kUnityBase = 0x01000000;
constexpr std::uint32_t kMonoBase = 0x03000000;
constexpr std::uint32_t kReturnStub = 0x00008000;
constexpr std::uint32_t kJniAttachStub = 0x00008100;
constexpr std::uint32_t kJniFindClassStub = 0x00008110;
constexpr std::uint32_t kJniRegisterNativesStub = 0x00008120;
constexpr std::uint32_t kJniFatalErrorStub = 0x00008130;
constexpr std::uint32_t kJniGetJavaVmStub = 0x00008140;
constexpr std::uint32_t kJniGetEnvStub = 0x00008150;
constexpr std::uint32_t kJniNewGlobalRefStub = 0x00008160;
constexpr std::uint32_t kJniDeleteGlobalRefStub = 0x00008170;
constexpr std::uint32_t kJniGetObjectClassStub = 0x00008180;
constexpr std::uint32_t kJniExceptionCheckStub = 0x00008190;
constexpr std::uint32_t kJniPushLocalFrameStub = 0x000081a0;
constexpr std::uint32_t kJniPopLocalFrameStub = 0x000081b0;
constexpr std::uint32_t kJniGetStaticFieldIdStub = 0x000081c0;
constexpr std::uint32_t kJniGetStaticObjectFieldStub = 0x000081d0;
constexpr std::uint32_t kJniNewStringUtfStub = 0x000081e0;
constexpr std::uint32_t kJniGetMethodIdStub = 0x000081f0;
constexpr std::uint32_t kJniCallObjectMethodStub = 0x00008200;
constexpr std::uint32_t kJniCallObjectMethodVStub = 0x00008210;
constexpr std::uint32_t kJniCallObjectMethodAStub = 0x00008220;
constexpr std::uint32_t kJniDeleteLocalRefStub = 0x00008230;
constexpr std::uint32_t kJniNewLocalRefStub = 0x00008240;
constexpr std::uint32_t kJniIsSameObjectStub = 0x00008250;
constexpr std::uint32_t kJniExceptionOccurredStub = 0x00008260;
constexpr std::uint32_t kJniExceptionDescribeStub = 0x00008270;
constexpr std::uint32_t kJniExceptionClearStub = 0x00008280;
constexpr std::uint32_t kPthreadOnceReturnStub = 0x00008290;
constexpr std::uint32_t kJniGetStaticIntFieldStub = 0x000082a0;
constexpr std::uint32_t kJniCallIntMethodStub = 0x000082b0;
constexpr std::uint32_t kJniCallIntMethodVStub = 0x000082c0;
constexpr std::uint32_t kJniCallIntMethodAStub = 0x000082d0;
constexpr std::uint32_t kJniCallBooleanMethodStub = 0x000082e0;
constexpr std::uint32_t kJniCallBooleanMethodVStub = 0x000082f0;
constexpr std::uint32_t kJniCallBooleanMethodAStub = 0x00008300;
constexpr std::uint32_t kJniCallVoidMethodStub = 0x00008310;
constexpr std::uint32_t kJniCallVoidMethodVStub = 0x00008320;
constexpr std::uint32_t kJniCallVoidMethodAStub = 0x00008330;
constexpr std::uint32_t kJniIsInstanceOfStub = 0x00008340;
constexpr std::uint32_t kJniGetStringUtfLengthStub = 0x00008370;
constexpr std::uint32_t kJniGetStringUtfCharsStub = 0x00008350;
constexpr std::uint32_t kJniReleaseStringUtfCharsStub = 0x00008360;
constexpr std::uint32_t kJniAllocObjectStub = 0x00008380;
constexpr std::uint32_t kJniNewObjectStub = 0x00008390;
constexpr std::uint32_t kJniNewObjectVStub = 0x000083a0;
constexpr std::uint32_t kJniNewObjectAStub = 0x000083b0;
constexpr std::uint32_t kJniGetFieldIdStub = 0x000083c0;
constexpr std::uint32_t kJniGetObjectFieldStub = 0x000083d0;
constexpr std::uint32_t kJniGetPrimitiveFieldStub = 0x000083e0;
constexpr std::uint32_t kJniSetFieldStub = 0x000083f0;
constexpr std::uint32_t kJniNewBooleanArrayStub = 0x00008400;
constexpr std::uint32_t kJniNewByteArrayStub = 0x00008410;
constexpr std::uint32_t kJniNewCharArrayStub = 0x00008420;
constexpr std::uint32_t kJniNewShortArrayStub = 0x00008430;
constexpr std::uint32_t kJniNewIntArrayStub = 0x00008440;
constexpr std::uint32_t kJniNewLongArrayStub = 0x00008450;
constexpr std::uint32_t kJniNewFloatArrayStub = 0x00008460;
constexpr std::uint32_t kJniNewDoubleArrayStub = 0x00008470;
constexpr std::uint32_t kJniGetArrayLengthStub = 0x00008480;
constexpr std::uint32_t kJniNewObjectArrayStub = 0x00008490;
constexpr std::uint32_t kJniGetObjectArrayElementStub = 0x000084a0;
constexpr std::uint32_t kJniSetObjectArrayElementStub = 0x000084b0;
constexpr std::uint32_t kJniGetPrimitiveArrayElementsStub = 0x000084c0;
constexpr std::uint32_t kJniReleasePrimitiveArrayElementsStub = 0x000084d0;
constexpr std::uint32_t kJniGetPrimitiveArrayRegionStub = 0x000084e0;
constexpr std::uint32_t kJniSetPrimitiveArrayRegionStub = 0x000084f0;
constexpr std::uint32_t kBsearchReturnStub = 0x00008500;
constexpr std::uint32_t kJniGetStaticMethodIdStub = 0x00008510;
constexpr std::uint32_t kJniCallStaticObjectMethodStub = 0x00008520;
constexpr std::uint32_t kJniCallStaticPrimitiveMethodStub = 0x00008530;
constexpr std::uint32_t kJniCallStaticVoidMethodStub = 0x00008540;
constexpr std::uint32_t kQsortReturnStub = 0x00008550;
constexpr std::uint32_t kSignalReturnStub = 0x00008560;
constexpr std::uint32_t kJniUnknownStubBase = 0x00008800;
constexpr std::uint32_t kJniVmObject = 0x00009000;
constexpr std::uint32_t kJniVmTable = 0x00009100;
constexpr std::uint32_t kJniEnvObject = 0x00009200;
constexpr std::uint32_t kJniEnvTable = 0x00009300;
constexpr std::uint32_t kJniFakeClassHandle = 0x0000a000;
constexpr std::uint32_t kJniFakeContextHandle = 0x0000a100;
constexpr std::uint32_t kJniFakeFieldId = 0x0000a200;
constexpr std::uint32_t kJniFakeObjectHandle = 0x0000a300;
constexpr std::uint32_t kJniFakeStringHandle = 0x0000a400;
constexpr std::uint32_t kJniFakeMethodId = 0x0000a500;
constexpr std::uint32_t kGuestNativeWindowHandle = 0x0000b000;
constexpr std::uint32_t kGuestLooperHandle = 0x0000b100;
constexpr std::uint32_t kGuestConfigurationHandle = 0x0000b200;
constexpr std::uint32_t kGuestSensorManagerHandle = 0x0000b300;
constexpr std::uint32_t kGuestSensorQueueHandle = 0x0000b400;
constexpr std::uint32_t kSvcJniAttach = 0x00fff001;
constexpr std::uint32_t kSvcJniFindClass = 0x00fff002;
constexpr std::uint32_t kSvcJniRegisterNatives = 0x00fff003;
constexpr std::uint32_t kSvcJniFatalError = 0x00fff004;
constexpr std::uint32_t kSvcJniGetJavaVm = 0x00fff005;
constexpr std::uint32_t kSvcJniGetEnv = 0x00fff006;
constexpr std::uint32_t kSvcJniNewGlobalRef = 0x00fff007;
constexpr std::uint32_t kSvcJniDeleteGlobalRef = 0x00fff008;
constexpr std::uint32_t kSvcJniGetObjectClass = 0x00fff009;
constexpr std::uint32_t kSvcJniNullCode = 0x00fff00a;
constexpr std::uint32_t kSvcJniExceptionCheck = 0x00fff00b;
constexpr std::uint32_t kSvcJniPushLocalFrame = 0x00fff00c;
constexpr std::uint32_t kSvcJniPopLocalFrame = 0x00fff00d;
constexpr std::uint32_t kSvcJniGetStaticFieldId = 0x00fff00e;
constexpr std::uint32_t kSvcJniGetStaticObjectField = 0x00fff00f;
constexpr std::uint32_t kSvcJniNewStringUtf = 0x00fff010;
constexpr std::uint32_t kSvcJniGetMethodId = 0x00fff011;
constexpr std::uint32_t kSvcJniCallObjectMethod = 0x00fff012;
constexpr std::uint32_t kSvcJniCallObjectMethodV = 0x00fff013;
constexpr std::uint32_t kSvcJniCallObjectMethodA = 0x00fff014;
constexpr std::uint32_t kSvcJniDeleteLocalRef = 0x00fff015;
constexpr std::uint32_t kSvcJniNewLocalRef = 0x00fff016;
constexpr std::uint32_t kSvcJniIsSameObject = 0x00fff017;
constexpr std::uint32_t kSvcJniExceptionOccurred = 0x00fff018;
constexpr std::uint32_t kSvcJniExceptionDescribe = 0x00fff019;
constexpr std::uint32_t kSvcJniExceptionClear = 0x00fff01a;
constexpr std::uint32_t kSvcJniGetStaticIntField = 0x00fff01b;
constexpr std::uint32_t kSvcJniCallIntMethod = 0x00fff01c;
constexpr std::uint32_t kSvcJniCallIntMethodV = 0x00fff01d;
constexpr std::uint32_t kSvcJniCallIntMethodA = 0x00fff01e;
constexpr std::uint32_t kSvcJniCallBooleanMethod = 0x00fff01f;
constexpr std::uint32_t kSvcJniCallBooleanMethodV = 0x00fff020;
constexpr std::uint32_t kSvcJniCallBooleanMethodA = 0x00fff021;
constexpr std::uint32_t kSvcJniCallVoidMethod = 16773154u;
constexpr std::uint32_t kSvcJniCallVoidMethodV = 16773155u;
constexpr std::uint32_t kSvcJniCallVoidMethodA = 16773156u;
constexpr std::uint32_t kSvcJniIsInstanceOf = 0x00fff025;
constexpr std::uint32_t kSvcJniGetStringUtfLength = 0x00fff028;
constexpr std::uint32_t kSvcJniGetStringUtfChars = 0x00fff026;
constexpr std::uint32_t kSvcJniReleaseStringUtfChars = 0x00fff027;
constexpr std::uint32_t kSvcJniAllocObject = 0x00fff029;
constexpr std::uint32_t kSvcJniNewObject = 0x00fff02a;
constexpr std::uint32_t kSvcJniNewObjectV = 0x00fff02b;
constexpr std::uint32_t kSvcJniNewObjectA = 0x00fff02c;
constexpr std::uint32_t kSvcJniGetFieldId = 0x00fff02d;
constexpr std::uint32_t kSvcJniGetObjectField = 0x00fff02e;
constexpr std::uint32_t kSvcJniGetPrimitiveField = 0x00fff02f;
constexpr std::uint32_t kSvcJniSetField = 0x00fff030;
constexpr std::uint32_t kSvcJniNewBooleanArray = 0x00fff031;
constexpr std::uint32_t kSvcJniNewByteArray = 0x00fff032;
constexpr std::uint32_t kSvcJniNewCharArray = 0x00fff033;
constexpr std::uint32_t kSvcJniNewShortArray = 0x00fff034;
constexpr std::uint32_t kSvcJniNewIntArray = 0x00fff035;
constexpr std::uint32_t kSvcJniNewLongArray = 0x00fff036;
constexpr std::uint32_t kSvcJniNewFloatArray = 0x00fff037;
constexpr std::uint32_t kSvcJniNewDoubleArray = 0x00fff038;
constexpr std::uint32_t kSvcJniGetArrayLength = 0x00fff039;
constexpr std::uint32_t kSvcJniNewObjectArray = 0x00fff03a;
constexpr std::uint32_t kSvcJniGetObjectArrayElement = 0x00fff03b;
constexpr std::uint32_t kSvcJniSetObjectArrayElement = 0x00fff03c;
constexpr std::uint32_t kSvcJniGetPrimitiveArrayElements = 0x00fff03d;
constexpr std::uint32_t kSvcJniReleasePrimitiveArrayElements = 0x00fff03e;
constexpr std::uint32_t kSvcJniGetPrimitiveArrayRegion = 0x00fff03f;
constexpr std::uint32_t kSvcJniSetPrimitiveArrayRegion = 0x00fff040;
constexpr std::uint32_t kSvcJniGetStaticMethodId = 0x00fff041;
constexpr std::uint32_t kSvcJniCallStaticObjectMethod = 0x00fff042;
constexpr std::uint32_t kSvcJniCallStaticPrimitiveMethod = 0x00fff043;
constexpr std::uint32_t kSvcJniCallStaticVoidMethod = 0x00fff044;
constexpr std::uint32_t kSvcPthreadOnceReturn = 0x00fff100;
constexpr std::uint32_t kSvcCxaThrowProbe = 0x00fff101;
constexpr std::uint32_t kSvcUnwindRaiseProbe = 0x00fff102;
constexpr std::uint32_t kSvcBsearchReturn = 0x00fff103;
constexpr std::uint32_t kSvcQsortReturn = 0x00fff104;
constexpr std::uint32_t kSvcSignalReturn = 0x00fff105;
constexpr std::uint32_t kSvcJniUnknownBase = 0x00ffe000;
constexpr std::uint32_t kJniEnvFunctionCount = 236;
constexpr std::uint32_t kThunkStubStart = 0x00010000;
constexpr std::uint32_t kDataSlotStart = 0x00050000;
constexpr std::uint32_t kBootstrapRuntimeBase = 0x000d0000;
constexpr std::uint32_t kBootstrapStringBase = 0x000e0000;
constexpr std::uint32_t kGuestHeapStart = 0x05000000;
constexpr std::uint32_t kGuestHeapEnd = 0x06e00000;
constexpr std::uint32_t kStackTop = 0x07000000;
// Keep cooperative guest-thread stacks entirely above the main stack and heap.
// The old 0x06ff0000-down layout eventually entered kGuestHeap and also left only
// 128 KiB between worker stacks, allowing busy Mono/Unity workers to overwrite
// parked thread frames.  The upper 16 MiB of the 128 MiB guest image is otherwise free.
constexpr std::uint32_t kGuestThreadStackTop = 0x0bfc0000;
constexpr std::uint32_t kGuestThreadStackStride = 0x00100000; // 1 MiB per guest thread
constexpr std::uint32_t kGuestThreadStackFloor = 0x07fc0000;
constexpr std::size_t kGuestMemorySize = 192u * 1024u * 1024u;
constexpr std::uint32_t kGuestSignalInfoSize = 128u;
constexpr std::uint32_t kGuestUcontextSize = 744u;
constexpr std::uint32_t kGuestSignalFrameReserve = 1024u;

constexpr std::uint16_t kElfTypeSharedObject = 3;
constexpr std::uint16_t kElfMachineArm = 40;
constexpr std::uint32_t kProgramTypeLoad = 1;
constexpr std::uint32_t kSectionTypeRel = 9;
constexpr std::uint32_t kSectionTypeDynsym = 11;
constexpr std::uint16_t kShnUndefined = 0;
constexpr std::uint8_t kSymbolTypeObject = 1;
constexpr std::uint8_t kSymbolTypeFunc = 2;

constexpr std::uint32_t kRelArmNone = 0;
constexpr std::uint32_t kRelArmAbs32 = 2;
constexpr std::uint32_t kRelArmRel32 = 3;
constexpr std::uint32_t kRelArmCopy = 20;
constexpr std::uint32_t kRelArmGlobDat = 21;
constexpr std::uint32_t kRelArmJumpSlot = 22;
constexpr std::uint32_t kRelArmRelative = 23;

#pragma pack(push, 1)
struct Elf32Ehdr {
    unsigned char ident[16];
    std::uint16_t type;
    std::uint16_t machine;
    std::uint32_t version;
    std::uint32_t entry;
    std::uint32_t phoff;
    std::uint32_t shoff;
    std::uint32_t flags;
    std::uint16_t ehsize;
    std::uint16_t phentsize;
    std::uint16_t phnum;
    std::uint16_t shentsize;
    std::uint16_t shnum;
    std::uint16_t shstrndx;
};

struct Elf32Phdr {
    std::uint32_t type;
    std::uint32_t offset;
    std::uint32_t vaddr;
    std::uint32_t paddr;
    std::uint32_t filesz;
    std::uint32_t memsz;
    std::uint32_t flags;
    std::uint32_t align;
};

struct Elf32Shdr {
    std::uint32_t name;
    std::uint32_t type;
    std::uint32_t flags;
    std::uint32_t addr;
    std::uint32_t offset;
    std::uint32_t size;
    std::uint32_t link;
    std::uint32_t info;
    std::uint32_t addralign;
    std::uint32_t entsize;
};

struct Elf32Sym {
    std::uint32_t name;
    std::uint32_t value;
    std::uint32_t size;
    std::uint8_t info;
    std::uint8_t other;
    std::uint16_t shndx;
};

struct Elf32Rel {
    std::uint32_t offset;
    std::uint32_t info;
};
#pragma pack(pop)

static_assert(sizeof(Elf32Ehdr) == 52);
static_assert(sizeof(Elf32Phdr) == 32);
static_assert(sizeof(Elf32Shdr) == 40);
static_assert(sizeof(Elf32Sym) == 16);
static_assert(sizeof(Elf32Rel) == 8);

struct GuestImage {
    std::string name;
    std::string path;
    std::uint32_t base = 0;
    std::uint32_t end = 0;
    std::vector<std::uint8_t> file;
    Elf32Ehdr ehdr{};
    std::size_t load_segments = 0;
};

struct ExportSymbol {
    std::string owner;
    std::uint32_t address = 0;
};

struct UnresolvedRelocation {
    std::string owner;
    std::string symbol;
    std::uint32_t target = 0;
    std::uint32_t type = 0;
    std::uint8_t symbol_type = 0;
};

struct LinkStats {
    std::size_t relative = 0;
    std::size_t internal = 0;
    std::size_t cross = 0;
    std::size_t unresolved = 0;
    std::size_t unsupported = 0;
    std::size_t unity_to_mono = 0;
    std::vector<std::string> cross_examples;
    std::vector<std::string> unresolved_examples;
};

bool RangeFits(std::size_t offset, std::size_t size, std::size_t total) {
    return offset <= total && size <= total - offset;
}

template <typename T>
bool ReadStruct(const std::vector<std::uint8_t>& bytes, std::size_t offset, T& out) {
    if (!RangeFits(offset, sizeof(T), bytes.size())) return false;
    std::memcpy(&out, bytes.data() + offset, sizeof(T));
    return true;
}

bool LoadFile(const std::string& path, std::vector<std::uint8_t>& bytes, std::string& error) {
    std::ifstream file(path, std::ios::binary | std::ios::ate);
    if (!file) {
        error = "Could not open " + path;
        return false;
    }
    const std::streamsize size = file.tellg();
    if (size <= 0) {
        error = "ELF file is empty: " + path;
        return false;
    }
    file.seekg(0, std::ios::beg);
    bytes.resize(static_cast<std::size_t>(size));
    if (!file.read(reinterpret_cast<char*>(bytes.data()), size)) {
        error = "Could not read " + path;
        return false;
    }
    return true;
}

bool GetSection(const GuestImage& image, std::uint32_t index, Elf32Shdr& out) {
    if (index >= image.ehdr.shnum || image.ehdr.shentsize < sizeof(Elf32Shdr)) return false;
    return ReadStruct(image.file,
                      static_cast<std::size_t>(image.ehdr.shoff) +
                          static_cast<std::size_t>(index) * image.ehdr.shentsize,
                      out);
}

std::string ReadString(const GuestImage& image, const Elf32Shdr& strtab, std::uint32_t offset) {
    if (offset >= strtab.size || !RangeFits(strtab.offset, strtab.size, image.file.size())) return {};
    const std::size_t start = static_cast<std::size_t>(strtab.offset) + offset;
    const std::size_t limit = static_cast<std::size_t>(strtab.offset) + strtab.size;
    std::size_t end = start;
    while (end < limit && image.file[end] != 0) ++end;
    return std::string(reinterpret_cast<const char*>(image.file.data() + start), end - start);
}

bool Read32(const std::vector<std::uint8_t>& memory, std::uint32_t address, std::uint32_t& value) {
    if (address > memory.size() || sizeof(value) > memory.size() - address) return false;
    std::memcpy(&value, memory.data() + address, sizeof(value));
    return true;
}

bool Write32(std::vector<std::uint8_t>& memory, std::uint32_t address, std::uint32_t value) {
    if (address > memory.size() || sizeof(value) > memory.size() - address) return false;
    std::memcpy(memory.data() + address, &value, sizeof(value));
    return true;
}

bool WriteBytes(std::vector<std::uint8_t>& memory, std::uint32_t address, const void* data, std::size_t size) {
    if (address > memory.size() || size > memory.size() - address) return false;
    std::memcpy(memory.data() + address, data, size);
    return true;
}

bool LoadAndMapImage(GuestImage& image, std::vector<std::uint8_t>& memory, std::string& error) {
    if (!LoadFile(image.path, image.file, error)) return false;
    if (!ReadStruct(image.file, 0, image.ehdr) ||
        image.ehdr.ident[0] != 0x7f || image.ehdr.ident[1] != 'E' ||
        image.ehdr.ident[2] != 'L' || image.ehdr.ident[3] != 'F' ||
        image.ehdr.ident[4] != 1 || image.ehdr.ident[5] != 1 ||
        image.ehdr.type != kElfTypeSharedObject || image.ehdr.machine != kElfMachineArm) {
        error = image.name + " is not ARM32 little-endian ET_DYN";
        return false;
    }

    for (std::uint32_t i = 0; i < image.ehdr.phnum; ++i) {
        Elf32Phdr phdr{};
        if (!ReadStruct(image.file,
                        static_cast<std::size_t>(image.ehdr.phoff) +
                            static_cast<std::size_t>(i) * image.ehdr.phentsize,
                        phdr)) {
            error = image.name + ": bad program header";
            return false;
        }
        if (phdr.type != kProgramTypeLoad) continue;
        const std::uint64_t guest_address = static_cast<std::uint64_t>(image.base) + phdr.vaddr;
        const std::uint64_t guest_end = guest_address + phdr.memsz;
        if (!RangeFits(phdr.offset, phdr.filesz, image.file.size()) || guest_end > memory.size()) {
            error = image.name + ": PT_LOAD outside shared guest memory";
            return false;
        }
        if (phdr.filesz) {
            std::memcpy(memory.data() + guest_address, image.file.data() + phdr.offset, phdr.filesz);
        }
        if (phdr.memsz > phdr.filesz) {
            std::memset(memory.data() + guest_address + phdr.filesz, 0, phdr.memsz - phdr.filesz);
        }
        image.end = std::max(image.end, static_cast<std::uint32_t>(guest_end));
        ++image.load_segments;
    }
    if (!image.load_segments) {
        error = image.name + ": no PT_LOAD segments";
        return false;
    }
    return true;
}

void CollectExports(const GuestImage& image,
                    std::unordered_map<std::string, ExportSymbol>& exports) {
    for (std::uint32_t i = 0; i < image.ehdr.shnum; ++i) {
        Elf32Shdr symtab{};
        if (!GetSection(image, i, symtab) || symtab.type != kSectionTypeDynsym ||
            symtab.entsize < sizeof(Elf32Sym) || !RangeFits(symtab.offset, symtab.size, image.file.size())) {
            continue;
        }
        Elf32Shdr strtab{};
        if (!GetSection(image, symtab.link, strtab)) continue;
        const std::size_t count = symtab.size / symtab.entsize;
        for (std::size_t n = 0; n < count; ++n) {
            Elf32Sym sym{};
            if (!ReadStruct(image.file, static_cast<std::size_t>(symtab.offset) + n * symtab.entsize, sym)) continue;
            if (sym.shndx == kShnUndefined || sym.name == 0) continue;
            const std::string name = ReadString(image, strtab, sym.name);
            if (name.empty()) continue;
            exports.emplace(name, ExportSymbol{image.name, image.base + sym.value});
            if (name == "JNI_OnLoad") {
                exports.emplace(image.name + "!JNI_OnLoad", ExportSymbol{image.name, image.base + sym.value});
            }
        }
    }
}

bool GetRelocationSymbol(const GuestImage& image,
                         const Elf32Shdr& relsec,
                         std::uint32_t symbol_index,
                         Elf32Sym& sym,
                         std::string& name) {
    Elf32Shdr symtab{};
    if (!GetSection(image, relsec.link, symtab) || symtab.entsize < sizeof(Elf32Sym) ||
        symbol_index >= symtab.size / symtab.entsize) {
        return false;
    }
    if (!ReadStruct(image.file,
                    static_cast<std::size_t>(symtab.offset) +
                        static_cast<std::size_t>(symbol_index) * symtab.entsize,
                    sym)) {
        return false;
    }
    Elf32Shdr strtab{};
    if (!GetSection(image, symtab.link, strtab)) return false;
    name = ReadString(image, strtab, sym.name);
    return true;
}

void AddExample(std::vector<std::string>& list, const std::string& value) {
    if (value.empty() || list.size() >= 12) return;
    if (std::find(list.begin(), list.end(), value) == list.end()) list.push_back(value);
}

bool ApplyRelocations(const GuestImage& image,
                      std::vector<std::uint8_t>& memory,
                      const std::unordered_map<std::string, ExportSymbol>& exports,
                      LinkStats& stats,
                      std::vector<UnresolvedRelocation>& unresolved_out,
                      std::string& error) {
    for (std::uint32_t i = 0; i < image.ehdr.shnum; ++i) {
        Elf32Shdr relsec{};
        if (!GetSection(image, i, relsec) || relsec.type != kSectionTypeRel ||
            relsec.entsize < sizeof(Elf32Rel)) {
            continue;
        }
        if (!RangeFits(relsec.offset, relsec.size, image.file.size())) {
            error = image.name + ": bad relocation section";
            return false;
        }
        const std::size_t count = relsec.size / relsec.entsize;
        for (std::size_t n = 0; n < count; ++n) {
            Elf32Rel rel{};
            if (!ReadStruct(image.file,
                            static_cast<std::size_t>(relsec.offset) + n * relsec.entsize,
                            rel)) {
                error = image.name + ": unreadable relocation";
                return false;
            }
            const std::uint32_t type = rel.info & 0xffu;
            const std::uint32_t symbol_index = rel.info >> 8;
            const std::uint32_t target = image.base + rel.offset;
            std::uint32_t addend = 0;
            if (!Read32(memory, target, addend)) {
                error = image.name + ": relocation target outside memory";
                return false;
            }

            if (type == kRelArmNone) continue;
            if (type == kRelArmRelative) {
                if (!Write32(memory, target, image.base + addend)) {
                    error = image.name + ": failed R_ARM_RELATIVE write";
                    return false;
                }
                ++stats.relative;
                continue;
            }

            Elf32Sym sym{};
            std::string symbol_name;
            if (!GetRelocationSymbol(image, relsec, symbol_index, sym, symbol_name)) {
                ++stats.unsupported;
                continue;
            }

            std::uint32_t symbol_address = 0;
            std::string owner;
            bool resolved = false;
            if (sym.shndx != kShnUndefined) {
                symbol_address = image.base + sym.value;
                owner = image.name;
                resolved = true;
            } else if (!symbol_name.empty()) {
                const auto found = exports.find(symbol_name);
                if (found != exports.end()) {
                    symbol_address = found->second.address;
                    owner = found->second.owner;
                    resolved = true;
                }
            }

            if (!resolved) {
                ++stats.unresolved;
                AddExample(stats.unresolved_examples, image.name + ":" + symbol_name);
                unresolved_out.push_back({image.name, symbol_name, target, type,
                                          static_cast<std::uint8_t>(sym.info & 0x0fu)});
                continue;
            }

            std::uint32_t relocated = 0;
            switch (type) {
            case kRelArmAbs32:
                relocated = symbol_address + addend;
                break;
            case kRelArmRel32:
                relocated = symbol_address + addend - target;
                break;
            case kRelArmGlobDat:
            case kRelArmJumpSlot:
                relocated = symbol_address;
                break;
            case kRelArmCopy:
                ++stats.unsupported;
                continue;
            default:
                ++stats.unsupported;
                continue;
            }
            if (!Write32(memory, target, relocated)) {
                error = image.name + ": failed relocation write";
                return false;
            }

            if (owner == image.name) {
                ++stats.internal;
            } else {
                ++stats.cross;
                AddExample(stats.cross_examples, image.name + " -> " + owner + ": " + symbol_name);
                if (image.name == "libunity.so" && owner == "libmono.so") ++stats.unity_to_mono;
            }
        }
    }
    return true;
}

std::optional<ExportSymbol> FindExport(const std::unordered_map<std::string, ExportSymbol>& exports,
                                       const std::string& name) {
    const auto found = exports.find(name);
    if (found == exports.end()) return std::nullopt;
    return found->second;
}

class BasicEnvironment : public Dynarmic::A32::UserCallbacks {
public:
    explicit BasicEnvironment(std::vector<std::uint8_t>& memory) : memory_(memory) {}
    Dynarmic::A32::Jit* jit = nullptr;
    bool saw_return = false;
    bool failed = false;
    bool executed_heap_code = false;
    std::uint32_t first_heap_code_pc = 0;
    std::uint32_t fault_pc = 0;
    int fault_exception = -1;
    std::size_t fallback_instruction_count = 0;
    std::uint32_t invalid_code_address = 0;
    std::uint32_t code_read_last = 0;
    std::uint32_t code_read_prev1 = 0;
    std::uint32_t code_read_prev2 = 0;
    std::uint32_t code_read_prev3 = 0;
    std::uint32_t code_read_last_image = 0;
    std::uint32_t watched_write_address = 0;
    std::size_t watched_write_count = 0;
    std::uint32_t bad_write_address = 0;
    std::size_t bad_write_size = 0;
    std::uint32_t bad_write_pc = 0;
    std::uint32_t bad_write_lr = 0;
    std::uint32_t bad_write_r0 = 0;
    std::uint32_t bad_write_r1 = 0;
    std::uint32_t bad_write_r2 = 0;
    std::uint32_t bad_write_r3 = 0;
    std::uint64_t ticks_left = 1000000;

    std::optional<std::uint32_t> MemoryReadCode(std::uint32_t address) override {
        code_read_prev3 = code_read_prev2;
        code_read_prev2 = code_read_prev1;
        code_read_prev1 = code_read_last;
        code_read_last = address;
        if (address >= kMainBase && address < kGuestHeapStart) code_read_last_image = address;
        if (address == 0) return 0xef000000u | kSvcJniNullCode;
        if (address > memory_.size() || 4 > memory_.size() - address) {
            invalid_code_address = address;
            return std::nullopt;
        }
        if (address >= kGuestHeapStart && address < kGuestHeapEnd) {
            if (!executed_heap_code) first_heap_code_pc = address;
            executed_heap_code = true;
        }
        return Read<std::uint32_t>(address);
    }
    std::uint8_t MemoryRead8(std::uint32_t a) override { return Read<std::uint8_t>(a); }
    std::uint16_t MemoryRead16(std::uint32_t a) override { return Read<std::uint16_t>(a); }
    std::uint32_t MemoryRead32(std::uint32_t a) override { return Read<std::uint32_t>(a); }
    std::uint64_t MemoryRead64(std::uint32_t a) override { return Read<std::uint64_t>(a); }
    void MemoryWrite8(std::uint32_t a, std::uint8_t v) override { Write(a, v); }
    void MemoryWrite16(std::uint32_t a, std::uint16_t v) override { Write(a, v); }
    void MemoryWrite32(std::uint32_t a, std::uint32_t v) override {
#if defined(__ANDROID__)
        if (watched_write_address != 0 && a == watched_write_address) {
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "WATCH write32 addr=0x%08x old=0x%08x new=0x%08x pc=0x%08x lr=0x%08x sp=0x%08x n=%zu",
                                a, Read<std::uint32_t>(a), v,
                                jit ? jit->Regs()[15] : 0u,
                                jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u,
                                watched_write_count++);
        }
#endif
        Write(a, v);
    }
    void MemoryWrite64(std::uint32_t a, std::uint64_t v) override { Write(a, v); }
    bool MemoryWriteExclusive8(std::uint32_t a, std::uint8_t v, std::uint8_t expected) override {
        if (Read<std::uint8_t>(a) != expected) return false;
        Write(a, v); return true;
    }
    bool MemoryWriteExclusive16(std::uint32_t a, std::uint16_t v, std::uint16_t expected) override {
        if (Read<std::uint16_t>(a) != expected) return false;
        Write(a, v); return true;
    }
    bool MemoryWriteExclusive32(std::uint32_t a, std::uint32_t v, std::uint32_t expected) override {
        if (Read<std::uint32_t>(a) != expected) return false;
        Write(a, v); return true;
    }
    bool MemoryWriteExclusive64(std::uint32_t a, std::uint64_t v, std::uint64_t expected) override {
        if (Read<std::uint64_t>(a) != expected) return false;
        Write(a, v); return true;
    }

    void InterpreterFallback(std::uint32_t pc, std::size_t num_instructions) override {
        failed = true;
        fault_pc = pc;
        fault_exception = -2;
        fallback_instruction_count = num_instructions;
        if (jit) jit->HaltExecution();
    }
    void CallSVC(std::uint32_t swi) override {
        if (swi == 0) saw_return = true;
        else failed = true;
        if (jit) {
            if (swi != 0) fault_pc = jit->Regs()[15];
            jit->HaltExecution();
        }
    }
    void ExceptionRaised(std::uint32_t pc, Dynarmic::A32::Exception exception) override {
        failed = true;
        fault_pc = pc;
        fault_exception = static_cast<int>(exception);
        if (jit) jit->HaltExecution();
    }
    void AddTicks(std::uint64_t ticks) override { ticks_left = ticks >= ticks_left ? 0 : ticks_left - ticks; }
    std::uint64_t GetTicksRemaining() override { return ticks_left; }

protected:
    template <typename T>
    T Read(std::uint32_t address) const {
        T value{};
        if (address > memory_.size() || sizeof(T) > memory_.size() - address) return value;
        std::memcpy(&value, memory_.data() + address, sizeof(T));
        return value;
    }
    template <typename T>
    void Write(std::uint32_t address, T value) {
        if (address > memory_.size() || sizeof(T) > memory_.size() - address) {
            failed = true;
            fault_pc = address;
            bad_write_address = address;
            bad_write_size = sizeof(T);
            if (jit) {
                bad_write_pc = jit->Regs()[15];
                bad_write_lr = jit->Regs()[14];
                bad_write_r0 = jit->Regs()[0];
                bad_write_r1 = jit->Regs()[1];
                bad_write_r2 = jit->Regs()[2];
                bad_write_r3 = jit->Regs()[3];
                jit->HaltExecution();
            }
            return;
        }
        std::memcpy(memory_.data() + address, &value, sizeof(T));
    }
    std::vector<std::uint8_t>& memory_;
};

bool WriteSvcStub(std::vector<std::uint8_t>& memory, std::uint32_t address, std::uint32_t svc) {
    return svc <= 0x00ffffffu &&
           Write32(memory, address, 0xef000000u | svc) &&
           Write32(memory, address + 4, 0xe12fff1eu);
}

bool WriteReturnStub(std::vector<std::uint8_t>& memory) {
    return WriteSvcStub(memory, kReturnStub, 0);
}

bool ExecuteSharedMonoProbe(std::vector<std::uint8_t>& memory,
                            const std::unordered_map<std::string, ExportSymbol>& exports,
                            std::uint32_t& root_domain,
                            std::uint32_t& fault_pc) {
    const auto symbol = FindExport(exports, "mono_get_root_domain");
    if (!symbol || symbol->owner != "libmono.so" || !WriteReturnStub(memory)) return false;

    BasicEnvironment env(memory);
    Dynarmic::ExclusiveMonitor global_monitor{1};
    Dynarmic::A32::UserConfig config;
    config.callbacks = &env;
    config.processor_id = 0;
    config.global_monitor = &global_monitor;
    config.arch_version = Dynarmic::A32::ArchVersion::v7;
    config.always_little_endian = true;
    config.enable_cycle_counting = true;
    config.code_cache_size = 32u * 1024u * 1024u;
    Dynarmic::A32::Jit jit(config);
    env.jit = &jit;
    jit.Regs().fill(0);
    jit.Regs()[13] = kStackTop;
    jit.Regs()[14] = kReturnStub;
    jit.Regs()[15] = symbol->address & ~1u;
    jit.SetCpsr((symbol->address & 1u) ? 0x20u : 0u);
    jit.Run();
    root_domain = jit.Regs()[0];
    fault_pc = env.fault_pc;
    return !env.failed && env.saw_return && root_domain == 0;
}

struct ThunkInstallResult {
    std::unordered_map<std::uint32_t, std::string> id_to_name;
    std::unordered_map<std::string, std::uint32_t> name_to_stub;
    std::unordered_map<std::string, std::uint32_t> name_to_data;
};

bool IsFunctionRelocation(const UnresolvedRelocation& rel) {
    if (rel.type == kRelArmJumpSlot || rel.symbol_type == kSymbolTypeFunc) return true;
    if (rel.symbol_type == kSymbolTypeObject) return false;
    // Bionic/compiler helper functions are sometimes STT_NOTYPE in old binaries.
    static const char* prefixes[] = {
        "pthread_", "__cxa_", "__aeabi_", "__gnu_", "__android_",
        "dl", "sig", "sem_"
    };
    for (const char* prefix : prefixes) {
        if (rel.symbol.rfind(prefix, 0) == 0) return true;
    }
    static const char* known_functions[] = {
        "malloc", "calloc", "realloc", "free", "memcpy", "memmove", "memset", "memcmp",
        "strlen", "strcmp", "strncmp", "strcpy", "strncpy", "strdup", "strchr", "strrchr",
        "strstr", "getenv", "setenv", "unsetenv", "mmap", "mprotect", "munmap", "mremap",
        "clock_gettime", "gettimeofday", "sysconf", "getpid", "gettid", "open", "close",
        "read", "write", "lseek", "fstat", "stat", "lstat", "access", "abort", "exit",
        "_exit", "printf", "fprintf", "snprintf", "vsnprintf", "puts", "putchar", "sleep",
        "usleep", "nanosleep", "sched_yield", "syscall", "prctl", "bsd_signal"
    };
    for (const char* name : known_functions) if (rel.symbol == name) return true;
    return false;
}

bool InstallTrapThunks(std::vector<std::uint8_t>& memory,
                       const std::vector<UnresolvedRelocation>& unresolved,
                       ThunkInstallResult& out,
                       std::string& error) {
    std::uint32_t next_stub = kThunkStubStart;
    std::uint32_t next_data = kDataSlotStart;
    std::uint32_t next_id = 1;

    for (const auto& rel : unresolved) {
        if (rel.symbol.empty()) continue;
        std::uint32_t address = 0;
        if (IsFunctionRelocation(rel)) {
            auto found = out.name_to_stub.find(rel.symbol);
            if (found == out.name_to_stub.end()) {
                if (next_stub + 8 >= kDataSlotStart || next_id > 0x00ffffffu) {
                    error = "Thunk stub region exhausted";
                    return false;
                }
                if (!Write32(memory, next_stub, 0xef000000u | next_id) ||
                    !Write32(memory, next_stub + 4, 0xe12fff1eu)) {
                    error = "Could not write thunk stub";
                    return false;
                }
                out.id_to_name.emplace(next_id, rel.symbol);
                out.name_to_stub.emplace(rel.symbol, next_stub);
                address = next_stub;
                next_stub += 8;
                ++next_id;
            } else {
                address = found->second;
            }
        } else {
            auto found = out.name_to_data.find(rel.symbol);
            if (found == out.name_to_data.end()) {
                if (next_data + 64 >= kBootstrapStringBase) {
                    error = "Data slot region exhausted";
                    return false;
                }
                address = next_data;
                out.name_to_data.emplace(rel.symbol, address);
                std::memset(memory.data() + address, 0, 64);
                if (rel.symbol == "__page_size") Write32(memory, address, 4096);
                if (rel.symbol == "__stack_chk_guard") Write32(memory, address, 0x6b8b4567u);
                next_data += 64;
            } else {
                address = found->second;
            }
        }

        std::uint32_t addend = 0;
        Read32(memory, rel.target, addend);
        std::uint32_t relocated = address;
        if (rel.type == kRelArmAbs32) relocated = address + addend;
        else if (rel.type == kRelArmRel32) relocated = address + addend - rel.target;
        if (!Write32(memory, rel.target, relocated)) {
            error = "Could not patch unresolved relocation for " + rel.symbol;
            return false;
        }
    }

    // Unity 4.6 resolves several GLES extensions dynamically through eglGetProcAddress.
    // Preinstall guest-callable thunks for the subset we bridge so the ARM32 caller never
    // receives an unusable arm64 host function pointer.
    static const char* dynamic_gles_symbols[] = {
        "glDiscardFramebufferEXT",
        "glMapBufferOES", "glUnmapBufferOES",
        "glMapBufferRangeEXT", "glFlushMappedBufferRangeEXT",
        "glGenQueriesEXT", "glDeleteQueriesEXT", "glBeginQueryEXT", "glEndQueryEXT",
        "glGetQueryObjectuivEXT", "glQueryCounterEXT", "glGetQueryObjectui64vEXT",
        "glDrawBuffersNV",
        "glRenderbufferStorageMultisampleEXT", "glFramebufferTexture2DMultisampleEXT",
        "glRenderbufferStorageMultisampleAPPLE", "glResolveMultisampleFramebufferAPPLE",
        "glGetProgramBinaryOES", "glProgramBinaryOES",
        "glPushGroupMarkerEXT", "glPopGroupMarkerEXT"
    };
    for (const char* symbol : dynamic_gles_symbols) {
        if (out.name_to_stub.find(symbol) != out.name_to_stub.end()) continue;
        if (next_stub + 8 >= kDataSlotStart || next_id > 0x00ffffffu) {
            error = "Thunk stub region exhausted while installing dynamic GLES bridge";
            return false;
        }
        if (!Write32(memory, next_stub, 0xef000000u | next_id) ||
            !Write32(memory, next_stub + 4, 0xe12fff1eu)) {
            error = "Could not write dynamic GLES thunk stub";
            return false;
        }
        out.id_to_name.emplace(next_id, symbol);
        out.name_to_stub.emplace(symbol, next_stub);
        next_stub += 8;
        ++next_id;
    }
    return true;
}

#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
std::string DeriveHotpatchDir(const std::string& managed_dir) {
    const std::string marker = "/Data/Managed";
    const std::size_t marker_pos = managed_dir.rfind(marker);
    if (marker_pos != std::string::npos) return managed_dir.substr(0, marker_pos) + "/projectv7-dev-hotpatch";
    const std::string windows_marker = "\\Data\\Managed";
    const std::size_t windows_pos = managed_dir.rfind(windows_marker);
    if (windows_pos != std::string::npos) return managed_dir.substr(0, windows_pos) + "\\projectv7-dev-hotpatch";
    return managed_dir + "/projectv7-dev-hotpatch";
}
#endif

class BootstrapEnvironment final : public BasicEnvironment {
public:
    BootstrapEnvironment(std::vector<std::uint8_t>& memory,
                         const std::unordered_map<std::uint32_t, std::string>& thunks,
                         const std::unordered_map<std::string, std::uint32_t>& name_to_stub,
                         const std::unordered_map<std::string, ExportSymbol>& exports,
                         std::string managed_dir,
                         void* host_native_window)
        : BasicEnvironment(memory), thunks_(thunks), name_to_stub_(name_to_stub), exports_(exports),
          managed_dir_(managed_dir), host_native_window_(host_native_window)
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
          , hotpatch_runtime_(DeriveHotpatchDir(managed_dir))
#endif
    {
        Write32(memory_, kBootstrapRuntimeBase, 0); // errno
        WriteSvcStub(memory_, kBsearchReturnStub, kSvcBsearchReturn);
        WriteSvcStub(memory_, kQsortReturnStub, kSvcQsortReturn);
        WriteSvcStub(memory_, kSignalReturnStub, kSvcSignalReturn);
        guest_thread_launches_.reserve(512);
        static constexpr char kLocaleC[] = "C";
        WriteBytes(memory_, kBootstrapRuntimeBase + 0x100, kLocaleC, sizeof(kLocaleC));
    }

    std::string first_thunk;
    std::uint32_t first_thunk_id = 0;
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
    std::size_t hotpatch_trap_count = 0;
    std::size_t hotpatch_handled_count = 0;
    std::string hotpatch_last_detail;
    std::string HotpatchStatusLine() const { return hotpatch_runtime_.StatusLine(); }
#endif
    bool live_ctor_trace = false;
    std::size_t handled_calls = 0;
    std::size_t executable_mprotect_calls = 0;
    std::size_t arm_cacheflush_calls = 0;
    bool pending_cacheflush = false;
    std::uint32_t pending_cacheflush_start = 0;
    std::uint32_t pending_cacheflush_size = 0;
    std::uint32_t first_exec_region = 0;
    std::uint32_t first_exec_region_size = 0;
    std::uint32_t last_mprotect_address = 0;
    std::uint32_t last_mprotect_length = 0;
    std::uint32_t last_mprotect_prot = 0;
    std::uint32_t last_mprotect_caller_lr = 0;
    std::vector<std::string> call_trace;
    struct JniNativeRegistration {
        std::string class_name;
        std::string name;
        std::string signature;
        std::uint32_t function = 0;
    };
    struct JniArrayRecord {
        std::uint32_t length = 0;
        std::uint32_t element_size = 1;
        bool object_array = false;
    };
    struct BsearchFrame {
        std::uint32_t key = 0;
        std::uint32_t base = 0;
        std::uint32_t size = 0;
        std::uint32_t comparator = 0;
        std::uint32_t low = 0;
        std::uint32_t high = 0;
        std::uint32_t mid = 0;
        std::uint32_t caller_lr = 0;
    };
    struct QsortFrame {
        std::uint32_t base = 0;
        std::uint32_t size = 0;
        std::uint32_t comparator = 0;
        std::uint32_t caller_lr = 0;
        std::uint32_t pivot_guest = 0;
        std::vector<std::pair<std::uint32_t, std::uint32_t>> ranges;
        std::uint32_t lo = 0;
        std::uint32_t hi = 0;
        std::uint32_t i = 0;
        std::uint32_t j = 0;
    };
    bool jni_attach_seen = false;
    bool jni_get_java_vm_seen = false;
    bool jni_get_env_seen = false;
    bool jni_new_global_ref_seen = false;
    bool jni_get_object_class_seen = false;
    bool jni_null_code_seen = false;
    std::uint32_t jni_null_code_lr = 0;
    std::string jni_static_field_name;
    std::string jni_static_field_signature;
    bool jni_get_static_object_field_seen = false;
    bool jni_get_static_int_field_seen = false;
    std::uint32_t jni_static_int_field_value = 0;
    std::string jni_new_string_utf;
    std::string jni_fake_object_string;
    std::string jni_method_name;
    std::string jni_method_signature;
    std::uint32_t jni_call_object_method_count = 0;
    std::uint32_t jni_call_int_method_count = 0;
    std::uint32_t jni_call_boolean_method_count = 0;
    std::uint32_t jni_unknown_slot = 0xffffffffu;
    std::uint32_t jni_unknown_lr = 0;
    std::uint32_t jni_unknown_caller_lr = 0;
    std::uint32_t jni_unknown_r0 = 0;
    std::uint32_t jni_unknown_r1 = 0;
    std::uint32_t jni_unknown_r2 = 0;
    std::uint32_t jni_unknown_r3 = 0;
    bool jni_fatal_error_seen = false;
    std::string jni_last_class;
    std::vector<std::string> jni_requested_classes;
    std::vector<JniNativeRegistration> jni_native_registrations;
    std::vector<std::string> diagnostic_formats;
    std::vector<std::pair<std::uint32_t, std::size_t>> allocation_trace;
    std::vector<std::string> file_trace;
    bool premature_return = false;
    std::uint32_t return_sp = 0;
    std::uint32_t return_r11 = 0;
    std::uint32_t premature_r0 = 0;
    std::vector<std::uint32_t> premature_regs;
    std::uint32_t unexpected_svc0_pc = 0;
    std::uint32_t unexpected_svc0_r7 = 0;
    std::vector<std::pair<std::uint32_t, std::uint32_t>> premature_frames;
    bool pending_guest_callback = false;
    std::size_t pthread_once_callbacks = 0;

    void ResetPhaseDiagnostics() {
        first_thunk.clear();
        first_thunk_id = 0;
        handled_calls = 0;
        executable_mprotect_calls = 0;
        arm_cacheflush_calls = 0;
        pending_cacheflush = false;
        pending_cacheflush_start = 0;
        pending_cacheflush_size = 0;
        first_exec_region = 0;
        first_exec_region_size = 0;
        last_mprotect_address = 0;
        last_mprotect_length = 0;
        last_mprotect_prot = 0;
        last_mprotect_caller_lr = 0;
        fault_exception = -1;
        fallback_instruction_count = 0;
        invalid_code_address = 0;
        code_read_last = 0;
        code_read_prev1 = 0;
        code_read_prev2 = 0;
        code_read_prev3 = 0;
        code_read_last_image = 0;
        bad_write_address = 0;
        bad_write_size = 0;
        bad_write_pc = 0;
        bad_write_lr = 0;
        bad_write_r0 = 0;
        bad_write_r1 = 0;
        bad_write_r2 = 0;
        bad_write_r3 = 0;
        executed_heap_code = false;
        first_heap_code_pc = 0;
        call_trace.clear();
        jni_attach_seen = false;
        jni_get_java_vm_seen = false;
        jni_get_env_seen = false;
        jni_new_global_ref_seen = false;
        jni_get_object_class_seen = false;
        jni_null_code_seen = false;
        jni_null_code_lr = 0;
        jni_static_field_name.clear();
        jni_static_field_signature.clear();
        jni_get_static_object_field_seen = false;
        jni_get_static_int_field_seen = false;
        jni_static_int_field_value = 0;
        jni_new_string_utf.clear();
        jni_fake_object_string.clear();
        jni_method_name.clear();
        jni_method_signature.clear();
        jni_call_object_method_count = 0;
        jni_call_int_method_count = 0;
        jni_call_boolean_method_count = 0;
        jni_unknown_slot = 0xffffffffu;
        jni_unknown_lr = 0;
        jni_unknown_caller_lr = 0;
        jni_unknown_r0 = 0;
        jni_unknown_r1 = 0;
        jni_unknown_r2 = 0;
        jni_unknown_r3 = 0;
        jni_fatal_error_seen = false;
        jni_last_class.clear();
        jni_requested_classes.clear();
        jni_native_registrations.clear();
        diagnostic_formats.clear();
        allocation_trace.clear();
        file_trace.clear();
        premature_return = false;
        return_sp = 0;
        return_r11 = 0;
        premature_r0 = 0;
        premature_regs.clear();
        unexpected_svc0_pc = 0;
        unexpected_svc0_r7 = 0;
        premature_frames.clear();
        pending_guest_callback = false;
        pthread_once_callbacks = 0;
        cooperative_yield_requested_ = false;
        thread_exit_requested_ = false;
    }

    void CallSVC(std::uint32_t swi) override {
        if (swi == 0) {
            const std::uint32_t svc_pc = jit ? jit->Regs()[15] : 0;
            if (jit && (svc_pc < kReturnStub || svc_pc > kReturnStub + 8)) {
                const std::uint32_t syscall_nr = jit->Regs()[7];
                if (syscall_nr == 0x000f0002u) { // __ARM_NR_cacheflush
                    const std::uint32_t start = jit->Regs()[0];
                    const std::uint32_t end = jit->Regs()[1];
                    if (end >= start) {
                        pending_cacheflush = true;
                        pending_cacheflush_start = start;
                        pending_cacheflush_size = end - start;
                    }
                    ++arm_cacheflush_calls;
                    jit->Regs()[0] = 0;
                    // Invalidate after jit.Run() returns. Calling InvalidateCacheRange
                    // from inside Dynarmic's SVC callback recursively takes its cache lock.
                    jit->HaltExecution();
                    return;
                }
                if (syscall_nr == 0x000f0005u) { // __ARM_NR_set_tls
                    arm_tls_value_ = jit->Regs()[0];
                    jit->Regs()[0] = 0;
                    return;
                }
                if (syscall_nr == 0x000f0006u) { // __ARM_NR_get_tls
                    jit->Regs()[0] = arm_tls_value_;
                    return;
                }
                unexpected_svc0_pc = svc_pc;
                unexpected_svc0_r7 = syscall_nr;
                first_thunk = "__kernel_svc0";
                first_thunk_id = unexpected_svc0_r7;
                jit->HaltExecution();
                return;
            }
            return_sp = jit ? jit->Regs()[13] : 0;
            return_r11 = jit ? jit->Regs()[11] : 0;
            // Worker entry points intentionally use per-thread guest stacks. Reaching
            // the common return sentinel from a worker is a normal thread-function
            // return, not the main-call stack corruption check used below.
            if (current_thread_id_ != 1) {
                saw_return = true;
                if (jit) jit->HaltExecution();
                return;
            }
            if (jit && return_sp != kStackTop) {
                premature_return = true;
                premature_r0 = jit->Regs()[0];
                premature_regs.assign(jit->Regs().begin(), jit->Regs().end());
                std::uint32_t fp = return_r11;
                for (int depth = 0; depth < 12; ++depth) {
                    if (fp < 8 || fp >= kStackTop || fp >= memory_.size()) break;
                    std::uint32_t saved_lr = 0;
                    std::uint32_t previous_fp = 0;
                    if (!Read32(memory_, fp, saved_lr) || !Read32(memory_, fp - 4, previous_fp)) break;
                    premature_frames.emplace_back(fp, saved_lr);
                    if (previous_fp <= fp || previous_fp > kStackTop) break;
                    fp = previous_fp;
                }
                jit->HaltExecution();
                return;
            }
            saw_return = true;
            if (jit) jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcUnwindRaiseProbe) {
            const std::uint32_t exception = jit->Regs()[0];
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "UNWIND RAISE exception=0x%08x r1=0x%08x r2=0x%08x r3=0x%08x sp=0x%08x lr=0x%08x",
                                exception, jit->Regs()[1], jit->Regs()[2], jit->Regs()[3],
                                jit->Regs()[13], jit->Regs()[14]);
#endif
            failed = true;
            fault_pc = kMainBase + 0x00004d74u;
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcCxaThrowProbe) {
            const std::uint32_t object = jit->Regs()[0];
            const std::uint32_t type_info = jit->Regs()[1];
            const std::uint32_t destructor = jit->Regs()[2];
            std::uint32_t name_ptr = 0;
            std::string type_name;
            if (type_info && Fits(type_info + 4u, 4u)) {
                name_ptr = Read<std::uint32_t>(type_info + 4u);
                if (name_ptr) type_name = ReadCString(name_ptr);
            }
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "CXX THROW object=0x%08x type=0x%08x name=%s dtor=0x%08x sp=0x%08x lr=0x%08x",
                                object, type_info, type_name.c_str(), destructor,
                                jit->Regs()[13], jit->Regs()[14]);
#endif
            failed = true;
            fault_pc = kUnityBase + 0x00939fd0u;
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcBsearchReturn) {
            if (bsearch_frames_.empty()) {
                failed = true;
                fault_pc = jit->Regs()[15];
                jit->HaltExecution();
                return;
            }
            BsearchFrame& frame = bsearch_frames_.back();
            const int comparison = static_cast<std::int32_t>(jit->Regs()[0]);
            if (comparison == 0) {
                const std::uint32_t result = frame.base + frame.mid * frame.size;
                const std::uint32_t caller_lr = frame.caller_lr;
                bsearch_frames_.pop_back();
                jit->Regs()[0] = result;
                jit->Regs()[14] = caller_lr;
                return;
            }
            if (comparison < 0) frame.high = frame.mid;
            else frame.low = frame.mid + 1u;
            if (frame.low >= frame.high) {
                const std::uint32_t caller_lr = frame.caller_lr;
                bsearch_frames_.pop_back();
                jit->Regs()[0] = 0;
                jit->Regs()[14] = caller_lr;
                return;
            }
            frame.mid = frame.low + (frame.high - frame.low) / 2u;
            const std::uint64_t element64 = static_cast<std::uint64_t>(frame.base) +
                                            static_cast<std::uint64_t>(frame.mid) * frame.size;
            if (element64 > UINT32_MAX || !Fits(static_cast<std::uint32_t>(element64), frame.size)) {
                const std::uint32_t caller_lr = frame.caller_lr;
                bsearch_frames_.pop_back();
                jit->Regs()[0] = 0;
                jit->Regs()[14] = caller_lr;
                return;
            }
            jit->Regs()[0] = frame.key;
            jit->Regs()[1] = static_cast<std::uint32_t>(element64);
            jit->Regs()[2] = 0;
            jit->Regs()[3] = 0;
            jit->Regs()[14] = kBsearchReturnStub;
            jit->Regs()[15] = frame.comparator & ~1u;
            jit->SetCpsr((frame.comparator & 1u) ? 0x20u : 0u);
            pending_guest_callback = true;
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcQsortReturn) {
            if (qsort_frames_.empty()) {
                failed = true;
                fault_pc = jit->Regs()[15];
                jit->HaltExecution();
                return;
            }
            HandleGuestQsortComparison(static_cast<std::int32_t>(jit->Regs()[0]));
            return;
        }
        if (jit && swi == kSvcSignalReturn) {
            auto frames = guest_signal_frames_.find(current_thread_id_);
            if (frames == guest_signal_frames_.end() || frames->second.empty()) {
                failed = true;
                fault_pc = jit->Regs()[15];
                jit->HaltExecution();
                return;
            }
            const GuestSignalFrame frame = frames->second.back();
            frames->second.pop_back();
            if (frames->second.empty()) guest_signal_frames_.erase(frames);
            jit->Regs() = frame.regs;
            jit->ExtRegs() = frame.ext_regs;
            jit->SetCpsr(frame.cpsr);
            jit->SetFpscr(frame.fpscr);
            const auto sigsuspend_stub = name_to_stub_.find("sigsuspend");
            if (sigsuspend_stub != name_to_stub_.end() && frame.regs[15] == sigsuspend_stub->second)
                guest_signal_interrupted_[current_thread_id_] = true;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL return thread=%u signal=%u resume_pc=0x%08x",
                                current_thread_id_, frame.signal, frame.regs[15]);
#endif
            return;
        }
        if (jit && swi == kSvcPthreadOnceReturn) {
            if (pthread_once_frames_.empty()) {
                failed = true;
                fault_pc = jit->Regs()[15];
                jit->HaltExecution();
                return;
            }
            const PthreadOnceFrame frame = pthread_once_frames_.back();
            pthread_once_frames_.pop_back();
            pthread_once_done_[frame.control] = true;
            if (frame.control && Fits(frame.control, 4)) Write32(memory_, frame.control, 2u);
            jit->Regs()[0] = 0;
            jit->Regs()[14] = frame.caller_lr;
            return;
        }
        if (jit && swi == kSvcJniNullCode) {
            jni_null_code_seen = true;
            jni_null_code_lr = jit->Regs()[14];
            failed = true;
            fault_pc = 0;
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcJniAttach) {
            jni_attach_seen = true;
            if (jit->Regs()[1]) Write32(memory_, jit->Regs()[1], kJniEnvObject);
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetJavaVm) {
            jni_get_java_vm_seen = true;
            if (jit->Regs()[1]) Write32(memory_, jit->Regs()[1], kJniVmObject);
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetEnv) {
            jni_get_env_seen = true;
            if (jit->Regs()[1]) Write32(memory_, jit->Regs()[1], kJniEnvObject);
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniNewGlobalRef) {
            jni_new_global_ref_seen = true;
            jit->Regs()[0] = jit->Regs()[1];
            return;
        }
        if (jit && (swi == kSvcJniAllocObject || swi == kSvcJniNewObject ||
                    swi == kSvcJniNewObjectV || swi == kSvcJniNewObjectA)) {
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && swi == kSvcJniDeleteGlobalRef) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetObjectClass) {
            jni_get_object_class_seen = true;
            jit->Regs()[0] = kJniFakeClassHandle;
            return;
        }
        if (jit && swi == kSvcJniGetFieldId) {
            jni_static_field_name = ReadCString(jit->Regs()[2]);
            jni_static_field_signature = ReadCString(jit->Regs()[3]);
            jit->Regs()[0] = kJniFakeFieldId;
            return;
        }
        if (jit && swi == kSvcJniGetObjectField) {
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && swi == kSvcJniGetPrimitiveField) {
            jit->Regs()[0] = 0;
            jit->Regs()[1] = 0;
            return;
        }
        if (jit && swi == kSvcJniSetField) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniExceptionCheck) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniExceptionOccurred) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniExceptionDescribe) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniExceptionClear) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniPushLocalFrame) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniPopLocalFrame) {
            jit->Regs()[0] = jit->Regs()[1];
            return;
        }
        if (jit && swi == kSvcJniGetStaticMethodId) {
            jni_method_name = ReadCString(jit->Regs()[2]);
            jni_method_signature = ReadCString(jit->Regs()[3]);
            jit->Regs()[0] = kJniFakeMethodId;
            return;
        }
        if (jit && swi == kSvcJniCallStaticObjectMethod) {
            jni_fake_object_string.clear();
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && swi == kSvcJniCallStaticPrimitiveMethod) {
            jit->Regs()[0] = 0;
            jit->Regs()[1] = 0;
            return;
        }
        if (jit && swi == kSvcJniCallStaticVoidMethod) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetStaticFieldId) {
            jni_static_field_name = ReadCString(jit->Regs()[2]);
            jni_static_field_signature = ReadCString(jit->Regs()[3]);
            jit->Regs()[0] = kJniFakeFieldId;
            return;
        }
        if (jit && swi == kSvcJniGetStaticObjectField) {
            jni_get_static_object_field_seen = true;
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && swi == kSvcJniGetStaticIntField) {
            jni_get_static_int_field_seen = true;
            jni_static_int_field_value = (jni_static_field_name == "FULL_WAKE_LOCK") ? 0x1au : 0u;
            jit->Regs()[0] = jni_static_int_field_value;
            return;
        }
        if (jit && swi == kSvcJniNewStringUtf) {
            jni_new_string_utf = ReadCString(jit->Regs()[1]);
            jit->Regs()[0] = kJniFakeStringHandle;
            return;
        }
        if (jit && swi == kSvcJniGetStringUtfLength) {
            if (jit->Regs()[1] == kJniFakeStringHandle) jit->Regs()[0] = static_cast<std::uint32_t>(jni_new_string_utf.size());
            else if (jit->Regs()[1] == kJniFakeObjectHandle) jit->Regs()[0] = static_cast<std::uint32_t>(jni_fake_object_string.size());
            else jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetStringUtfChars) {
            std::string value;
            if (jit->Regs()[1] == kJniFakeStringHandle) value = jni_new_string_utf;
            else if (jit->Regs()[1] == kJniFakeObjectHandle) value = jni_fake_object_string;
            const std::uint32_t ptr = Allocate(value.size() + 1u, 1u);
            if (!ptr || !WriteCString(ptr, value)) {
                jit->Regs()[0] = 0;
                return;
            }
            if (jit->Regs()[2] && Fits(jit->Regs()[2], 1u)) Write(jit->Regs()[2], static_cast<std::uint8_t>(1));
            jit->Regs()[0] = ptr;
            return;
        }
        if (jit && swi == kSvcJniReleaseStringUtfChars) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetMethodId) {
            jni_method_name = ReadCString(jit->Regs()[2]);
            jni_method_signature = ReadCString(jit->Regs()[3]);
            jit->Regs()[0] = kJniFakeMethodId;
            return;
        }
        if (jit && (swi == kSvcJniCallObjectMethod || swi == kSvcJniCallObjectMethodV || swi == kSvcJniCallObjectMethodA)) {
            ++jni_call_object_method_count;
            if (jni_method_name == "getPackageName") {
                jni_fake_object_string = "me.jacqueb.cnr64poc";
            } else {
                jni_fake_object_string.clear();
            }
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && (swi == kSvcJniCallIntMethod || swi == kSvcJniCallIntMethodV || swi == kSvcJniCallIntMethodA)) {
            ++jni_call_int_method_count;
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && (swi == kSvcJniCallBooleanMethod || swi == kSvcJniCallBooleanMethodV || swi == kSvcJniCallBooleanMethodA)) {
            ++jni_call_boolean_method_count;
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && (swi == kSvcJniCallVoidMethod || swi == kSvcJniCallVoidMethodV || swi == kSvcJniCallVoidMethodA)) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniIsInstanceOf) {
            jit->Regs()[0] = (jit->Regs()[1] != 0 && jit->Regs()[2] != 0) ? 1u : 0u;
            return;
        }
        if (jit && swi == kSvcJniDeleteLocalRef) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniNewLocalRef) {
            jit->Regs()[0] = jit->Regs()[1];
            return;
        }
        if (jit && swi == kSvcJniIsSameObject) {
            jit->Regs()[0] = jit->Regs()[1] == jit->Regs()[2] ? 1u : 0u;
            return;
        }
        if (jit && (swi == kSvcJniNewBooleanArray || swi == kSvcJniNewByteArray ||
                    swi == kSvcJniNewCharArray || swi == kSvcJniNewShortArray ||
                    swi == kSvcJniNewIntArray || swi == kSvcJniNewLongArray ||
                    swi == kSvcJniNewFloatArray || swi == kSvcJniNewDoubleArray)) {
            std::uint32_t element_size = 1;
            if (swi == kSvcJniNewCharArray || swi == kSvcJniNewShortArray) element_size = 2;
            else if (swi == kSvcJniNewIntArray || swi == kSvcJniNewFloatArray) element_size = 4;
            else if (swi == kSvcJniNewLongArray || swi == kSvcJniNewDoubleArray) element_size = 8;
            const std::uint32_t length = jit->Regs()[1];
            const std::uint64_t byte_count = static_cast<std::uint64_t>(length) * element_size;
            if (byte_count > SIZE_MAX - 16u) {
                jit->Regs()[0] = 0;
                return;
            }
            const std::uint32_t array = Allocate(static_cast<std::size_t>(byte_count) + 16u, 8u);
            if (array != 0) jni_arrays_[array] = {length, element_size, false};
            jit->Regs()[0] = array;
            return;
        }
        if (jit && swi == kSvcJniNewObjectArray) {
            const std::uint32_t length = jit->Regs()[1];
            const std::uint64_t byte_count = static_cast<std::uint64_t>(length) * 4u;
            if (byte_count > SIZE_MAX - 16u) {
                jit->Regs()[0] = 0;
                return;
            }
            const std::uint32_t array = Allocate(static_cast<std::size_t>(byte_count) + 16u, 4u);
            if (array != 0) {
                jni_arrays_[array] = {length, 4u, true};
                for (std::uint32_t index = 0; index < length; ++index)
                    Write32(memory_, array + 16u + index * 4u, jit->Regs()[3]);
            }
            jit->Regs()[0] = array;
            return;
        }
        if (jit && swi == kSvcJniGetArrayLength) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            jit->Regs()[0] = found == jni_arrays_.end() ? 0u : found->second.length;
            return;
        }
        if (jit && swi == kSvcJniGetObjectArrayElement) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            std::uint32_t value = 0;
            if (found != jni_arrays_.end() && found->second.object_array &&
                jit->Regs()[2] < found->second.length) {
                Read32(memory_, jit->Regs()[1] + 16u + jit->Regs()[2] * 4u, value);
            }
            jit->Regs()[0] = value;
            return;
        }
        if (jit && swi == kSvcJniSetObjectArrayElement) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            if (found != jni_arrays_.end() && found->second.object_array &&
                jit->Regs()[2] < found->second.length) {
                Write32(memory_, jit->Regs()[1] + 16u + jit->Regs()[2] * 4u, jit->Regs()[3]);
            }
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetPrimitiveArrayElements) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            if (found == jni_arrays_.end() || found->second.object_array) {
                jit->Regs()[0] = 0;
                return;
            }
            if (jit->Regs()[2] && Fits(jit->Regs()[2], 1u))
                Write(jit->Regs()[2], static_cast<std::uint8_t>(0));
            jit->Regs()[0] = jit->Regs()[1] + 16u;
            return;
        }
        if (jit && swi == kSvcJniReleasePrimitiveArrayElements) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && (swi == kSvcJniGetPrimitiveArrayRegion || swi == kSvcJniSetPrimitiveArrayRegion)) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            const std::uint32_t start = jit->Regs()[2];
            const std::uint32_t length = jit->Regs()[3];
            std::uint32_t guest_buffer = 0;
            Read32(memory_, jit->Regs()[13], guest_buffer);
            if (found != jni_arrays_.end() && !found->second.object_array &&
                start <= found->second.length && length <= found->second.length - start) {
                const std::size_t byte_count = static_cast<std::size_t>(length) * found->second.element_size;
                const std::uint32_t array_data = jit->Regs()[1] + 16u + start * found->second.element_size;
                if (guest_buffer != 0 && Fits(guest_buffer, byte_count) && Fits(array_data, byte_count)) {
                    if (swi == kSvcJniGetPrimitiveArrayRegion)
                        std::memcpy(memory_.data() + guest_buffer, memory_.data() + array_data, byte_count);
                    else
                        std::memcpy(memory_.data() + array_data, memory_.data() + guest_buffer, byte_count);
                }
            }
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi >= kSvcJniUnknownBase && swi < kSvcJniUnknownBase + kJniEnvFunctionCount) {
            jni_unknown_slot = swi - kSvcJniUnknownBase;
            jni_unknown_lr = jit->Regs()[14];
            Read32(memory_, jit->Regs()[13] + 20u, jni_unknown_caller_lr);
            jni_unknown_r0 = jit->Regs()[0];
            jni_unknown_r1 = jit->Regs()[1];
            jni_unknown_r2 = jit->Regs()[2];
            jni_unknown_r3 = jit->Regs()[3];
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
            const std::string hotpatch_name = "jni.slot." + std::to_string(jni_unknown_slot);
            if (TryApplyHotpatch(hotpatch_name, swi) || WaitForHotpatch(hotpatch_name, swi)) return;
#endif
            failed = true;
            fault_pc = jit->Regs()[15];
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcJniFindClass) {
            jni_last_class = ReadCString(jit->Regs()[1]);
            jni_requested_classes.push_back(jni_last_class);
            jit->Regs()[0] = kJniFakeClassHandle;
            return;
        }
        if (jit && swi == kSvcJniRegisterNatives) {
            const std::uint32_t methods = jit->Regs()[2];
            const std::uint32_t count = jit->Regs()[3];
            if (count <= 256 && Fits(methods, static_cast<std::size_t>(count) * 12u)) {
                for (std::uint32_t i = 0; i < count; ++i) {
                    const std::uint32_t entry = methods + i * 12u;
                    JniNativeRegistration reg;
                    reg.class_name = jni_last_class;
                    reg.name = ReadCString(Read<std::uint32_t>(entry));
                    reg.signature = ReadCString(Read<std::uint32_t>(entry + 4));
                    reg.function = Read<std::uint32_t>(entry + 8);
                    jni_native_registrations.push_back(std::move(reg));
                }
            }
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniFatalError) {
            jni_fatal_error_seen = true;
            failed = true;
            fault_pc = jit->Regs()[15];
            jit->HaltExecution();
            return;
        }
        const auto found = thunks_.find(swi);
        std::string name = found == thunks_.end() ? "<unknown>" : found->second;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        if (live_ctor_trace && handled_calls < 48) {
            const std::uint32_t lr = jit ? jit->Regs()[14] : 0u;
            const std::uint32_t caller_word = (lr >= 4u && Fits(lr - 4u, 4u))
                                                ? Read<std::uint32_t>(lr - 4u) : 0u;
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "CTOR thunk enter id=%u name=%s r0=0x%08x r1=0x%08x sp=0x%08x fp=0x%08x lr=0x%08x caller=0x%08x",
                                swi, name.c_str(), jit ? jit->Regs()[0] : 0u,
                                jit ? jit->Regs()[1] : 0u,
                                jit ? jit->Regs()[13] : 0u,
                                jit ? jit->Regs()[11] : 0u,
                                lr, caller_word);
        }
#endif
        const std::size_t versionSep = name.find('@');
        if (versionSep != std::string::npos) name.resize(versionSep);
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        if (call_trace.size() >= 160) call_trace.erase(call_trace.begin());
        call_trace.push_back(name);
        auto& thread_trace = guest_thread_call_traces_[current_thread_id_];
        if (thread_trace.size() >= 16) thread_trace.erase(thread_trace.begin());
        thread_trace.push_back(name);
#endif
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
        if (TryApplyHotpatch(name, swi) || Dispatch(name)) {
#else
        if (Dispatch(name)) {
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (live_ctor_trace && handled_calls < 48) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "CTOR thunk exit id=%u name=%s r0=0x%08x",
                                    swi, name.c_str(), jit ? jit->Regs()[0] : 0u);
            }
#endif
            ++handled_calls;
            return;
        }
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
        if (WaitForHotpatch(name, swi)) {
            ++handled_calls;
            return;
        }
#endif
        first_thunk_id = swi;
        first_thunk = name;
        if (jit) jit->HaltExecution();
    }

private:
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
    bool TryApplyHotpatch(const std::string& name, std::uint32_t svc_id) {
        if (!jit) return false;

        const HotpatchRule rule = hotpatch_runtime_.LookupRule(name);
        if (rule.kind == HotpatchRule::Kind::ReturnU32) {
            Ret(rule.value);
            ++hotpatch_handled_count;
            hotpatch_last_detail = "rule:return " + name;
            return true;
        }
        if (rule.kind == HotpatchRule::Kind::Alias) {
            if (Dispatch(rule.target)) {
                ++hotpatch_handled_count;
                hotpatch_last_detail = "rule:alias " + name + " -> " + rule.target;
                return true;
            }
        }

        Cnr64HotpatchContextV1 context{};
        context.abi_version = CNR64_HOTPATCH_ABI_VERSION;
        context.symbol = name.c_str();
        context.svc_id = svc_id;
        context.thread_id = current_thread_id_;
        context.regs = jit->Regs().data();
        context.ext_regs = jit->ExtRegs().data();
        context.ext_reg_count = static_cast<std::uint32_t>(jit->ExtRegs().size());
        context.cpsr = jit->Cpsr();
        context.fpscr = jit->Fpscr();
        context.guest_memory = memory_.data();
        context.guest_memory_size = static_cast<std::uint32_t>(memory_.size());
        context.host_native_window = host_native_window_;
        context.managed_dir = managed_dir_.c_str();
        context.host_context = this;
        context.allocate_guest = [](void* opaque, std::uint32_t size, std::uint32_t alignment) -> std::uint32_t {
            auto* self = static_cast<BootstrapEnvironment*>(opaque);
            return self ? self->Allocate(size, alignment ? alignment : 1u) : 0u;
        };
        context.log_message = [](void* opaque, const char* message) {
            auto* self = static_cast<BootstrapEnvironment*>(opaque);
            if (!self || !message) return;
            if (self->file_trace.size() >= 96) self->file_trace.erase(self->file_trace.begin());
            self->file_trace.push_back(std::string("hotpatch: ") + message);
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_INFO, "CNR64HOTPATCH", "%s", message);
#endif
        };

        std::string detail;
        const int result = hotpatch_runtime_.DispatchPlugins(context, detail);
        jit->SetCpsr(context.cpsr);
        jit->SetFpscr(context.fpscr);
        if (result == CNR64_HOTPATCH_PASS) return false;

        hotpatch_last_detail = detail.empty() ? name : detail + ": " + name;
        ++hotpatch_handled_count;
        if (result == CNR64_HOTPATCH_RETRY_GUEST) {
            const auto stub = name_to_stub_.find(name);
            if (stub != name_to_stub_.end()) {
                jit->Regs()[15] = stub->second;
                pending_guest_callback = true;
                jit->HaltExecution();
            }
        } else if (result == CNR64_HOTPATCH_HALT) {
            first_thunk_id = svc_id;
            first_thunk = "hotpatch requested halt: " + name;
            jit->HaltExecution();
        }
        return true;
    }

    bool WaitForHotpatch(const std::string& name, std::uint32_t svc_id) {
        ++hotpatch_trap_count;
        const std::uint32_t wait_ms = hotpatch_runtime_.TrapWaitMilliseconds();
#if defined(__ANDROID__)
        __android_log_print(ANDROID_LOG_WARN, "CNR64HOTPATCH",
                            "TRAP symbol=%s svc=%u pc=0x%08x lr=0x%08x wait_ms=%u dir=%s",
                            name.c_str(), svc_id, jit ? jit->Regs()[15] : 0u,
                            jit ? jit->Regs()[14] : 0u, wait_ms,
                            hotpatch_runtime_.Directory().c_str());
#endif
        if (wait_ms == 0) return false;
        const auto deadline = std::chrono::steady_clock::now() + std::chrono::milliseconds(wait_ms);
        auto next_log = std::chrono::steady_clock::now() + std::chrono::seconds(5);
        while (std::chrono::steady_clock::now() < deadline) {
            if (TryApplyHotpatch(name, svc_id)) {
#if defined(__ANDROID__)
                __android_log_print(ANDROID_LOG_INFO, "CNR64HOTPATCH",
                                    "RESUMED symbol=%s detail=%s", name.c_str(), hotpatch_last_detail.c_str());
#endif
                return true;
            }
            std::this_thread::sleep_for(std::chrono::milliseconds(250));
#if defined(__ANDROID__)
            if (std::chrono::steady_clock::now() >= next_log) {
                __android_log_print(ANDROID_LOG_WARN, "CNR64HOTPATCH",
                                    "WAITING symbol=%s dir=%s", name.c_str(), hotpatch_runtime_.Directory().c_str());
                next_log = std::chrono::steady_clock::now() + std::chrono::seconds(5);
            }
#endif
        }
        return false;
    }
#endif

    static std::uint32_t AlignUp(std::uint32_t value, std::uint32_t alignment) {
        return (value + alignment - 1u) & ~(alignment - 1u);
    }

    bool Fits(std::uint32_t address, std::size_t size) const {
        return address <= memory_.size() && size <= memory_.size() - address;
    }

    std::string NormalizeGuestPath(const std::string& path) const {
        if (managed_dir_.empty()) return path;
        static const std::string mono_profile = "/mono/2.0/";
        static const std::string packaged_data_prefix = "assets/bin/Data";
        const std::string managed_suffix = "/Managed";
        const std::string data_dir = managed_dir_.size() >= managed_suffix.size() &&
                                     managed_dir_.compare(managed_dir_.size() - managed_suffix.size(),
                                                          managed_suffix.size(), managed_suffix) == 0
            ? managed_dir_.substr(0, managed_dir_.size() - managed_suffix.size())
            : managed_dir_;

        if (path == packaged_data_prefix) return data_dir;
        if (path.rfind(packaged_data_prefix + "/", 0) == 0) {
            return data_dir + path.substr(packaged_data_prefix.size());
        }

        static const std::string current_managed_marker = "/files/Data/Managed";
        static const std::string legacy_managed_marker = "/files/Managed";
        std::size_t managed_pos = path.find(current_managed_marker);
        std::size_t marker_size = current_managed_marker.size();
        if (managed_pos == std::string::npos) {
            managed_pos = path.find(legacy_managed_marker);
            marker_size = legacy_managed_marker.size();
        }
        if (managed_pos != std::string::npos &&
            (path.rfind("/data/data/", 0) == 0 || path.rfind("/data/user/0/", 0) == 0)) {
            std::string suffix = path.substr(managed_pos + marker_size);
            if (suffix.rfind(mono_profile, 0) == 0) suffix = "/" + suffix.substr(mono_profile.size());
            return managed_dir_ + suffix;
        }
        const std::size_t profile_pos = path.rfind(mono_profile);
        if (profile_pos != std::string::npos) return managed_dir_ + "/" + path.substr(profile_pos + mono_profile.size());
        return path;
    }

    std::string ReadCString(std::uint32_t address, std::size_t max_len = 4096) const {
        if (!Fits(address, 1)) return {};
        std::string out;
        for (std::size_t i = 0; i < max_len && Fits(address + static_cast<std::uint32_t>(i), 1); ++i) {
            const char c = static_cast<char>(memory_[address + i]);
            if (c == '\0') break;
            out.push_back(c);
        }
        return out;
    }

    bool WriteCString(std::uint32_t address, const std::string& value, std::size_t max_bytes = SIZE_MAX) {
        const std::size_t count = std::min(value.size(), max_bytes == 0 ? 0 : max_bytes - 1);
        if (!Fits(address, count + 1)) return false;
        if (count) std::memcpy(memory_.data() + address, value.data(), count);
        memory_[address + count] = 0;
        return true;
    }

    std::uint32_t Allocate(std::size_t size, std::uint32_t alignment = 16) {
        if (size == 0) size = 1;
        const std::uint32_t aligned = AlignUp(heap_next_, alignment);
        const std::uint64_t end = static_cast<std::uint64_t>(aligned) + size;
        if (end > kGuestHeapEnd) return 0;
        heap_next_ = AlignUp(static_cast<std::uint32_t>(end), 16);
        allocations_[aligned] = size;
        if (allocation_trace.size() < 32) allocation_trace.emplace_back(aligned, size);
        std::memset(memory_.data() + aligned, 0, size);
        return aligned;
    }

    void SetErrno(std::uint32_t value) {
        Write32(memory_, kBootstrapRuntimeBase, value);
    }

    std::uint32_t Arg(unsigned index) const {
        if (!jit) return 0;
        if (index < 4) return jit->Regs()[index];
        std::uint32_t value = 0;
        const std::uint32_t sp = jit->Regs()[13];
        Read32(memory_, sp + static_cast<std::uint32_t>((index - 4) * 4), value);
        return value;
    }

    std::uint64_t Arg64(unsigned word_index) const {
        const std::uint64_t lo = Arg(word_index);
        const std::uint64_t hi = Arg(word_index + 1);
        return lo | (hi << 32);
    }

    float ArgFloat(unsigned word_index) const {
        const std::uint32_t bits = Arg(word_index);
        float value = 0.0f;
        std::memcpy(&value, &bits, sizeof(value));
        return value;
    }

    double ArgDouble(unsigned word_index) const {
        const std::uint64_t bits = Arg64(word_index);
        double value = 0.0;
        std::memcpy(&value, &bits, sizeof(value));
        return value;
    }

    void Ret(std::uint32_t value) {
        if (jit) jit->Regs()[0] = value;
    }

    void Ret64(std::uint64_t value) {
        if (!jit) return;
        jit->Regs()[0] = static_cast<std::uint32_t>(value);
        jit->Regs()[1] = static_cast<std::uint32_t>(value >> 32);
    }

    void RetFloat(float value) {
        std::uint32_t bits = 0;
        std::memcpy(&bits, &value, sizeof(bits));
        Ret(bits);
    }

    void RetDouble(double value) {
        std::uint64_t bits = 0;
        std::memcpy(&bits, &value, sizeof(bits));
        Ret64(bits);
    }

#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
    void RecordSyncEvent(const char* op, std::uint32_t object, std::uint32_t detail = 0) {
        auto& trace = guest_thread_sync_traces_[current_thread_id_];
        if (trace.size() >= 96) trace.erase(trace.begin());
        std::ostringstream line;
        line << op << "@0x" << std::hex << object;
        if (detail != 0) line << ":" << detail;
        if (jit) line << "/lr=0x" << jit->Regs()[14] << "/sp=0x" << jit->Regs()[13];
        trace.push_back(line.str());
    }
#endif

    struct VirtualFile {
        std::string data;
        std::size_t pos = 0;
    };

    std::uint32_t RegisterVirtualFile(std::string data) {
        const std::uint32_t handle = next_file_handle_++;
        virtual_files_[handle] = {std::move(data), 0};
        return handle;
    }

    std::uint32_t RegisterFileHandle(std::FILE* file, const std::string& path = {}) {
        if (!file) return 0;
        const std::uint32_t handle = Allocate(84, 4);
        if (!handle) return 0;
#if defined(_WIN32)
        const int fd = ::_fileno(file);
#else
        const int fd = ::fileno(file);
#endif
        const std::uint16_t flags = 0x0004u; // old Bionic __SRD
        const std::int16_t guest_fd = static_cast<std::int16_t>(fd);
        Write<std::uint16_t>(handle + 12, flags);
        Write<std::int16_t>(handle + 14, guest_fd);
        file_handles_[handle] = file;
        if (!path.empty()) file_paths_[handle] = path;
        return handle;
    }

    std::FILE* LookupFileHandle(std::uint32_t handle) const {
        const auto it = file_handles_.find(handle);
        return it == file_handles_.end() ? nullptr : it->second;
    }

#if !defined(_WIN32)
    bool WriteGuestStat(std::uint32_t guest, const struct stat& st) {
        if (!Fits(guest, 104)) return false;
        std::memset(memory_.data() + guest, 0, 104);
        Write32(memory_, guest + 0x10, static_cast<std::uint32_t>(st.st_mode));
        Write32(memory_, guest + 0x14, static_cast<std::uint32_t>(st.st_nlink));
        Write32(memory_, guest + 0x18, static_cast<std::uint32_t>(st.st_uid));
        Write32(memory_, guest + 0x1c, static_cast<std::uint32_t>(st.st_gid));
        const std::uint64_t size = static_cast<std::uint64_t>(st.st_size);
        std::memcpy(memory_.data() + guest + 0x30, &size, sizeof(size));
        Write32(memory_, guest + 0x38, static_cast<std::uint32_t>(st.st_blksize));
        const std::uint64_t blocks = static_cast<std::uint64_t>(st.st_blocks);
        std::memcpy(memory_.data() + guest + 0x40, &blocks, sizeof(blocks));
        Write32(memory_, guest + 0x48, static_cast<std::uint32_t>(st.st_atim.tv_sec));
        Write32(memory_, guest + 0x4c, static_cast<std::uint32_t>(st.st_atim.tv_nsec));
        Write32(memory_, guest + 0x50, static_cast<std::uint32_t>(st.st_mtim.tv_sec));
        Write32(memory_, guest + 0x54, static_cast<std::uint32_t>(st.st_mtim.tv_nsec));
        Write32(memory_, guest + 0x58, static_cast<std::uint32_t>(st.st_ctim.tv_sec));
        Write32(memory_, guest + 0x5c, static_cast<std::uint32_t>(st.st_ctim.tv_nsec));
        const std::uint64_t ino = static_cast<std::uint64_t>(st.st_ino);
        std::memcpy(memory_.data() + guest + 0x60, &ino, sizeof(ino));
        return true;
    }
#endif

#if defined(__ANDROID__)
    std::uint32_t RegisterEglHandle(std::uintptr_t host_handle) {
        if (host_handle == 0) return 0;
        for (const auto& entry : egl_handles_) {
            if (entry.second == host_handle) return entry.first;
        }
        const std::uint32_t guest_handle = next_egl_handle_++;
        egl_handles_[guest_handle] = host_handle;
        return guest_handle;
    }

    std::uintptr_t LookupEglHandle(std::uint32_t guest_handle) const {
        if (guest_handle == 0) return 0;
        const auto found = egl_handles_.find(guest_handle);
        return found == egl_handles_.end() ? 0 : found->second;
    }

    void UnregisterEglHandle(std::uint32_t guest_handle) {
        if (guest_handle != 0) egl_handles_.erase(guest_handle);
    }

    std::vector<EGLint> ReadEglAttribList(std::uint32_t guest_address) const {
        std::vector<EGLint> attributes;
        if (guest_address == 0) return attributes;
        for (std::size_t index = 0; index < 128; ++index) {
            std::uint32_t raw = 0;
            if (!Read32(memory_, guest_address + static_cast<std::uint32_t>(index * 4u), raw)) break;
            const EGLint value = static_cast<EGLint>(raw);
            attributes.push_back(value);
            if (value == EGL_NONE) break;
        }
        if (!attributes.empty() && attributes.back() != EGL_NONE) attributes.push_back(EGL_NONE);
        return attributes;
    }
#endif

#if defined(__ANDROID__)
    struct GuestZStream32 {
        std::uint32_t next_in = 0;
        std::uint32_t avail_in = 0;
        std::uint32_t total_in = 0;
        std::uint32_t next_out = 0;
        std::uint32_t avail_out = 0;
        std::uint32_t total_out = 0;
        std::uint32_t msg = 0;
        std::uint32_t state = 0;
        std::uint32_t zalloc = 0;
        std::uint32_t zfree = 0;
        std::uint32_t opaque = 0;
        std::int32_t data_type = 0;
        std::uint32_t adler = 0;
        std::uint32_t reserved = 0;
    };
    static_assert(sizeof(GuestZStream32) == 56, "ARM32 z_stream layout must remain 56 bytes");

    enum class HostZlibKind {
        Inflate,
        Deflate,
    };

    struct HostZlibStream {
        z_stream stream{};
        HostZlibKind kind = HostZlibKind::Inflate;
        bool initialized = false;
    };

    bool ReadGuestZStream(std::uint32_t address, GuestZStream32& stream) const {
        if (!Fits(address, sizeof(stream))) return false;
        std::memcpy(&stream, memory_.data() + address, sizeof(stream));
        return true;
    }

    bool WriteGuestZStream(std::uint32_t address, const GuestZStream32& stream) {
        if (!Fits(address, sizeof(stream))) return false;
        std::memcpy(memory_.data() + address, &stream, sizeof(stream));
        return true;
    }

    std::uint32_t GuestPointerFromHost(const Bytef* pointer) const {
        if (!pointer) return 0;
        const auto* begin = memory_.data();
        const auto* end = begin + memory_.size();
        if (pointer < begin || pointer > end) return 0;
        const std::size_t offset = static_cast<std::size_t>(pointer - begin);
        return offset <= UINT32_MAX ? static_cast<std::uint32_t>(offset) : 0u;
    }

    bool SyncGuestZlibBuffersToHost(std::uint32_t guest_address, HostZlibStream& state) {
        GuestZStream32 guest{};
        if (!ReadGuestZStream(guest_address, guest)) return false;

        if (guest.avail_in != 0) {
            if (guest.next_in == 0 || !Fits(guest.next_in, guest.avail_in)) return false;
            state.stream.next_in = reinterpret_cast<Bytef*>(memory_.data() + guest.next_in);
        } else if (guest.next_in != 0 && Fits(guest.next_in, 1)) {
            state.stream.next_in = reinterpret_cast<Bytef*>(memory_.data() + guest.next_in);
        } else {
            state.stream.next_in = Z_NULL;
        }
        state.stream.avail_in = static_cast<uInt>(guest.avail_in);

        if (guest.avail_out != 0) {
            if (guest.next_out == 0 || !Fits(guest.next_out, guest.avail_out)) return false;
            state.stream.next_out = reinterpret_cast<Bytef*>(memory_.data() + guest.next_out);
        } else if (guest.next_out != 0 && Fits(guest.next_out, 1)) {
            state.stream.next_out = reinterpret_cast<Bytef*>(memory_.data() + guest.next_out);
        } else {
            state.stream.next_out = Z_NULL;
        }
        state.stream.avail_out = static_cast<uInt>(guest.avail_out);
        return true;
    }

    bool SyncHostZlibStateToGuest(std::uint32_t guest_address,
                                  const HostZlibStream& state,
                                  bool expose_state) {
        GuestZStream32 guest{};
        if (!ReadGuestZStream(guest_address, guest)) return false;
        guest.next_in = GuestPointerFromHost(state.stream.next_in);
        guest.avail_in = static_cast<std::uint32_t>(state.stream.avail_in);
        guest.total_in = static_cast<std::uint32_t>(state.stream.total_in);
        guest.next_out = GuestPointerFromHost(state.stream.next_out);
        guest.avail_out = static_cast<std::uint32_t>(state.stream.avail_out);
        guest.total_out = static_cast<std::uint32_t>(state.stream.total_out);
        guest.state = expose_state ? guest_address : 0u;
        guest.data_type = static_cast<std::int32_t>(state.stream.data_type);
        guest.adler = static_cast<std::uint32_t>(state.stream.adler);
        guest.reserved = static_cast<std::uint32_t>(state.stream.reserved);
        if (state.stream.msg) {
            const std::size_t length = std::strlen(state.stream.msg);
            const std::uint32_t message = Allocate(length + 1u, 1u);
            if (message && WriteCString(message, state.stream.msg)) guest.msg = message;
        } else {
            guest.msg = 0;
        }
        return WriteGuestZStream(guest_address, guest);
    }

    void DisposeHostZlibStream(HostZlibStream& state) {
        if (!state.initialized) return;
        if (state.kind == HostZlibKind::Inflate) ::inflateEnd(&state.stream);
        else ::deflateEnd(&state.stream);
        state.initialized = false;
    }

    bool DispatchZlib(const std::string& name) {
        if (name == "crc32") {
            const std::uint32_t guest_buffer = Arg(1);
            const std::uint32_t length = Arg(2);
            if (length != 0 && (guest_buffer == 0 || !Fits(guest_buffer, length))) {
                Ret(0);
                return true;
            }
            const Bytef* buffer = length == 0 ? Z_NULL : reinterpret_cast<const Bytef*>(memory_.data() + guest_buffer);
            Ret(static_cast<std::uint32_t>(::crc32(static_cast<uLong>(Arg(0)), buffer, static_cast<uInt>(length))));
            return true;
        }

        if (name == "inflateInit_" || name == "inflateInit2_" || name == "deflateInit2_") {
            const std::uint32_t guest_address = Arg(0);
            const std::uint32_t guest_stream_size = name == "deflateInit2_" ? Arg(7)
                                                   : name == "inflateInit2_" ? Arg(3)
                                                   : Arg(2);
            if (guest_address == 0 || !Fits(guest_address, sizeof(GuestZStream32)) ||
                guest_stream_size != sizeof(GuestZStream32)) {
                Ret(static_cast<std::uint32_t>(Z_VERSION_ERROR));
                return true;
            }

            auto existing = zlib_streams_.find(guest_address);
            if (existing != zlib_streams_.end()) {
                DisposeHostZlibStream(existing->second);
                zlib_streams_.erase(existing);
            }

            auto [inserted_it, inserted] = zlib_streams_.try_emplace(guest_address);
            (void)inserted;
            HostZlibStream& state = inserted_it->second;
            state = HostZlibStream{};
            state.stream.zalloc = Z_NULL;
            state.stream.zfree = Z_NULL;
            state.stream.opaque = Z_NULL;
            int rc = Z_STREAM_ERROR;
            if (name == "inflateInit_") {
                state.kind = HostZlibKind::Inflate;
                rc = ::inflateInit(&state.stream);
            } else if (name == "inflateInit2_") {
                state.kind = HostZlibKind::Inflate;
                rc = ::inflateInit2(&state.stream, static_cast<int>(Arg(1)));
            } else {
                state.kind = HostZlibKind::Deflate;
                rc = ::deflateInit2(&state.stream,
                                    static_cast<int>(Arg(1)),
                                    static_cast<int>(Arg(2)),
                                    static_cast<int>(Arg(3)),
                                    static_cast<int>(Arg(4)),
                                    static_cast<int>(Arg(5)));
            }
            state.initialized = rc == Z_OK;
            SyncHostZlibStateToGuest(guest_address, state, rc == Z_OK);
            if (file_trace.size() < 96) {
                std::ostringstream trace;
                trace << name << " guest=0x" << std::hex << guest_address << std::dec
                      << " stream_size=" << guest_stream_size << " rc=" << rc;
                file_trace.push_back(trace.str());
            }
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }

        if (name == "inflate" || name == "deflate" ||
            name == "inflateReset" || name == "deflateReset" ||
            name == "inflateEnd" || name == "deflateEnd") {
            const std::uint32_t guest_address = Arg(0);
            auto it = zlib_streams_.find(guest_address);
            if (it == zlib_streams_.end() || !it->second.initialized) {
                Ret(static_cast<std::uint32_t>(Z_STREAM_ERROR));
                return true;
            }

            HostZlibStream& state = it->second;
            const bool wants_inflate = name.rfind("inflate", 0) == 0;
            if ((wants_inflate && state.kind != HostZlibKind::Inflate) ||
                (!wants_inflate && state.kind != HostZlibKind::Deflate)) {
                Ret(static_cast<std::uint32_t>(Z_STREAM_ERROR));
                return true;
            }

            int rc = Z_STREAM_ERROR;
            if (name == "inflate" || name == "deflate") {
                if (!SyncGuestZlibBuffersToHost(guest_address, state)) {
                    Ret(static_cast<std::uint32_t>(Z_STREAM_ERROR));
                    return true;
                }
                rc = wants_inflate
                    ? ::inflate(&state.stream, static_cast<int>(Arg(1)))
                    : ::deflate(&state.stream, static_cast<int>(Arg(1)));
                SyncHostZlibStateToGuest(guest_address, state, true);
            } else if (name == "inflateReset" || name == "deflateReset") {
                rc = wants_inflate ? ::inflateReset(&state.stream) : ::deflateReset(&state.stream);
                SyncHostZlibStateToGuest(guest_address, state, rc == Z_OK);
            } else {
                rc = wants_inflate ? ::inflateEnd(&state.stream) : ::deflateEnd(&state.stream);
                state.initialized = false;
                SyncHostZlibStateToGuest(guest_address, state, false);
                zlib_streams_.erase(it);
            }

            if (file_trace.size() < 96) {
                std::ostringstream trace;
                trace << name << " guest=0x" << std::hex << guest_address << std::dec << " rc=" << rc;
                file_trace.push_back(trace.str());
            }
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        return false;
    }
#endif

#include "gles_bridge_methods.inc"
#include "compat_bridge_methods.inc"

public:
    std::vector<std::string> guest_thread_pump_trace;
    bool allow_main_thread_cooperative_yield = false;
    struct WorkerBoundaryRecord {
        std::uint32_t thread_id = 0;
        std::uint32_t thread_start = 0;
        std::string thunk;
        std::uint32_t fault_pc = 0;
        std::uint32_t jni_slot = 0xffffffffu;
        std::array<std::uint32_t, 16> regs{};
    };
    bool worker_boundary_seen = false;
    std::uint32_t worker_boundary_thread_id = 0;
    std::string worker_boundary_thunk;
    std::uint32_t worker_boundary_fault_pc = 0;
    std::uint32_t worker_boundary_jni_slot = 0xffffffffu;
    std::array<std::uint32_t, 16> worker_boundary_regs{};
    std::vector<WorkerBoundaryRecord> worker_boundaries;

    void ResetWorkerBoundaryDiagnostics() {
        worker_boundary_seen = false;
        worker_boundary_thread_id = 0;
        worker_boundary_thunk.clear();
        worker_boundary_fault_pc = 0;
        worker_boundary_jni_slot = 0xffffffffu;
        worker_boundary_regs.fill(0);
        worker_boundaries.clear();
    }

    bool ConsumeCooperativeYieldRequest() {
        if (!cooperative_yield_requested_) return false;
        cooperative_yield_requested_ = false;
        return true;
    }

    bool QueueGuestSignal(std::uint32_t target_thread, std::uint32_t signal) {
        if (signal == 0) return target_thread == 1 ||
            std::any_of(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                        [&](const GuestThreadLaunch& item) { return item.id == target_thread && !item.finished; });
        if (target_thread != 1) {
            const auto target = std::find_if(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                                             [&](const GuestThreadLaunch& item) {
                                                 return item.id == target_thread && !item.finished;
                                             });
            if (target == guest_thread_launches_.end()) return false;
        }
        guest_pending_signals_[target_thread].push_back(signal);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                            "PV7SIGNAL queue sender=%u target=%u signal=%u",
                            current_thread_id_, target_thread, signal);
#endif
        return true;
    }

    bool DeliverPendingGuestSignal() {
        if (!jit) return false;
        auto pending = guest_pending_signals_.find(current_thread_id_);
        if (pending == guest_pending_signals_.end() || pending->second.empty()) return false;
        const std::uint32_t signal = pending->second.front();
        pending->second.erase(pending->second.begin());
        if (pending->second.empty()) guest_pending_signals_.erase(pending);

        const auto action_it = guest_signal_actions_.find(signal);
        if (action_it == guest_signal_actions_.end() || action_it->second.handler == 0u ||
            action_it->second.handler == 1u) {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL ignored/default thread=%u signal=%u handler=0x%08x",
                                current_thread_id_, signal,
                                action_it == guest_signal_actions_.end() ? 0u : action_it->second.handler);
#endif
            return true;
        }

        GuestSignalFrame frame{};
        frame.regs = jit->Regs();
        frame.ext_regs = jit->ExtRegs();
        frame.cpsr = jit->Cpsr();
        frame.fpscr = jit->Fpscr();
        frame.signal = signal;

        const GuestSignalAction action = action_it->second;
        const bool uses_siginfo = (action.flags & 0x4u) != 0u; // SA_SIGINFO
        std::uint32_t handler_sp = frame.regs[13];
        if (uses_siginfo) {
            // ARM32 Android delivers SA_SIGINFO handlers a real siginfo_t and
            // ucontext_t. Mono uses that context for exception/backtrace control,
            // so null placeholders are not sufficient.
            std::uint32_t frame_base = 0;
            if (frame.regs[13] > kGuestSignalFrameReserve) {
                frame_base = (frame.regs[13] - kGuestSignalFrameReserve) & ~7u;
            }
            if (frame_base != 0u && Fits(frame_base, kGuestSignalFrameReserve)) {
                std::memset(memory_.data() + frame_base, 0, kGuestSignalFrameReserve);
                handler_sp = frame_base;
                frame.guest_siginfo = frame_base + 64u;
                frame.guest_ucontext = frame.guest_siginfo + kGuestSignalInfoSize;
            } else {
                frame.guest_siginfo = Allocate(kGuestSignalInfoSize, 8u);
                frame.guest_ucontext = Allocate(kGuestUcontextSize, 8u);
                if (frame.guest_siginfo) std::memset(memory_.data() + frame.guest_siginfo, 0, kGuestSignalInfoSize);
                if (frame.guest_ucontext) std::memset(memory_.data() + frame.guest_ucontext, 0, kGuestUcontextSize);
            }
            frame.uses_siginfo = frame.guest_siginfo != 0u && frame.guest_ucontext != 0u;
            if (frame.uses_siginfo) {
                // siginfo_t: signo, errno, code. Raised/thread-directed signals use
                // SI_TKILL on Android/Linux; the remaining payload is zeroed.
                Write32(memory_, frame.guest_siginfo + 0u, signal);
                Write32(memory_, frame.guest_siginfo + 4u, 0u);
                Write32(memory_, frame.guest_siginfo + 8u, static_cast<std::uint32_t>(-6));

                // ARM32 ucontext_t starts with flags/link/stack_t (20 bytes), then
                // struct sigcontext. Populate the architectural state exactly where
                // old Bionic/Mono expects it.
                const std::uint32_t uc = frame.guest_ucontext;
                Write32(memory_, uc + 0u, 0u); // uc_flags
                Write32(memory_, uc + 4u, 0u); // uc_link
                // uc_stack reports the alternate signal stack state, not the
                // interrupted thread's normal pthread stack. No guest sigaltstack
                // has been installed, so report the kernel default: SS_DISABLE.
                Write32(memory_, uc + 8u, 0u); // uc_stack.ss_sp
                Write32(memory_, uc + 12u, 2u); // uc_stack.ss_flags = SS_DISABLE
                Write32(memory_, uc + 16u, 0u); // uc_stack.ss_size
                constexpr std::uint32_t mcontext = 20u;
                Write32(memory_, uc + mcontext + 0u, 0u); // trap_no
                Write32(memory_, uc + mcontext + 4u, 0u); // error_code
                Write32(memory_, uc + mcontext + 8u, 0u); // oldmask
                for (std::uint32_t reg = 0; reg <= 10u; ++reg)
                    Write32(memory_, uc + mcontext + 12u + reg * 4u, frame.regs[reg]);
                Write32(memory_, uc + mcontext + 56u, frame.regs[11]);
                Write32(memory_, uc + mcontext + 60u, frame.regs[12]);
                Write32(memory_, uc + mcontext + 64u, frame.regs[13]);
                Write32(memory_, uc + mcontext + 68u, frame.regs[14]);
                Write32(memory_, uc + mcontext + 72u, frame.regs[15]);
                Write32(memory_, uc + mcontext + 76u, frame.cpsr);
                Write32(memory_, uc + mcontext + 80u, 0u); // fault_address
            }
        }
        guest_signal_frames_[current_thread_id_].push_back(frame);

        jit->Regs()[0] = signal;
        jit->Regs()[1] = frame.uses_siginfo ? frame.guest_siginfo : 0u;
        jit->Regs()[2] = frame.uses_siginfo ? frame.guest_ucontext : 0u;
        jit->Regs()[13] = frame.uses_siginfo ? handler_sp : frame.regs[13];
        jit->Regs()[14] = kSignalReturnStub;
        jit->Regs()[15] = action.handler & ~1u;
        jit->SetCpsr((frame.cpsr & ~0x20u) | ((action.handler & 1u) ? 0x20u : 0u));
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                            "PV7SIGNAL deliver thread=%u signal=%u handler=0x%08x interrupted_pc=0x%08x interrupted_sp=0x%08x handler_sp=0x%08x ucontext=0x%08x",
                            current_thread_id_, signal, action.handler, frame.regs[15], frame.regs[13],
                            handler_sp, frame.guest_ucontext);
#endif
        return true;
    }

    std::size_t PumpQueuedGuestThreads(std::size_t max_threads, int max_slices_per_thread) {
        if (!jit || max_threads == 0 || max_slices_per_thread <= 0) return 0;

        const auto saved_regs = jit->Regs();
        const auto saved_ext_regs = jit->ExtRegs();
        const std::uint32_t saved_cpsr = jit->Cpsr();
        const std::uint32_t saved_fpscr = jit->Fpscr();
        const auto saved_tls_values = tls_values_;
        const std::uint32_t saved_arm_tls = arm_tls_value_;
        const std::uint32_t saved_thread_id = current_thread_id_;
        std::size_t pumped = 0;
        guest_thread_pump_trace.clear();
        const std::size_t thread_count_at_entry = guest_thread_launches_.size();
        std::size_t inspected = 0;

        // Round-robin across the guest thread table. Starting from element zero on
        // every cooperative yield can permanently starve later-created threads when
        // the runnable set is larger than max_threads (FMOD commonly exposes this).
        while (pumped < max_threads && inspected < thread_count_at_entry &&
               !guest_thread_launches_.empty()) {
            if (guest_thread_pump_cursor_ >= guest_thread_launches_.size())
                guest_thread_pump_cursor_ = 0;
            const std::size_t launch_index = guest_thread_pump_cursor_;
            guest_thread_pump_cursor_ = (guest_thread_pump_cursor_ + 1) % guest_thread_launches_.size();
            ++inspected;
            auto& launch = guest_thread_launches_[launch_index];
            if (launch.finished || launch.start < kMainBase || launch.start >= kGuestHeapStart) continue;

            if (!launch.started) {
                launch.started = true;
                launch.regs.fill(0);
                launch.ext_regs.fill(0);
                launch.regs[0] = launch.arg;
                const std::uint64_t stack_offset =
                    static_cast<std::uint64_t>(launch.id - 2u) * kGuestThreadStackStride;
                if (stack_offset > static_cast<std::uint64_t>(kGuestThreadStackTop - kGuestThreadStackFloor)) {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                    __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                        "PV7STACK exhausted guest thread stack region id=%u offset=0x%llx",
                                        launch.id, static_cast<unsigned long long>(stack_offset));
#endif
                    launch.finished = true;
                    continue;
                }
                launch.regs[13] = kGuestThreadStackTop - static_cast<std::uint32_t>(stack_offset);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                if (launch.id == 3u) {
                    // The Unity async-reader worker keeps its queue mutex pointer at
                    // sp+4 after a 56-byte entry prologue. Watch that slot to find
                    // the writer if the parked worker frame is corrupted.
                    watched_write_address = launch.regs[13] - 52u;
                    watched_write_count = 0;
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                        "PV7STACK watch thread=3 slot=0x%08x stack_top=0x%08x",
                                        watched_write_address, launch.regs[13]);
                }
#endif
                launch.regs[14] = kReturnStub;
                launch.regs[15] = launch.start & ~1u;
                launch.cpsr = (launch.start & 1u) ? 0x20u : 0u;
                launch.fpscr = 0;
                launch.tls_values.clear();
                launch.arm_tls = 0;
            }

            tls_values_ = launch.tls_values;
            arm_tls_value_ = launch.arm_tls;
            current_thread_id_ = launch.id;
            ResetPhaseDiagnostics();
            saw_return = false;
            premature_return = false;
            failed = false;
            fault_pc = 0;
            jit->ClearHalt();
            jit->Regs() = launch.regs;
            jit->ExtRegs() = launch.ext_regs;
            jit->SetCpsr(launch.cpsr);
            jit->SetFpscr(launch.fpscr);

            int slices_run = 0;
            std::size_t cooperative_yields = 0;
            for (; slices_run < max_slices_per_thread; ++slices_run) {
                ticks_left = 1000000;
                DeliverPendingGuestSignal();
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                const std::uint32_t watched_before = watched_write_address != 0
                    ? Read<std::uint32_t>(watched_write_address) : 0u;
                const std::uint32_t watched_pc_before = jit->Regs()[15];
                const std::uint32_t watched_lr_before = jit->Regs()[14];
#endif
                const auto halt_reason = jit->Run();
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                if (watched_write_address != 0) {
                    const std::uint32_t watched_after = Read<std::uint32_t>(watched_write_address);
                    if (watched_before != watched_after && watched_write_count < 64u) {
                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                            "PV7STACK slice-change thread=%u addr=0x%08x old=0x%08x new=0x%08x pc_before=0x%08x lr_before=0x%08x pc_after=0x%08x lr_after=0x%08x",
                                            current_thread_id_, watched_write_address, watched_before, watched_after,
                                            watched_pc_before, watched_lr_before, jit->Regs()[15], jit->Regs()[14]);
                        ++watched_write_count;
                    }
                }
#endif
                if (saw_return || premature_return || failed || !first_thunk.empty() || thread_exit_requested_) break;
                if (cooperative_yield_requested_) {
                    ++cooperative_yields;
                    cooperative_yield_requested_ = false;
                    jit->ClearHalt();
                    continue;
                }
                if (pending_guest_callback) {
                    pending_guest_callback = false;
                    jit->ClearHalt();
                    continue;
                }
                if (pending_cacheflush) {
                    const std::uint32_t start = pending_cacheflush_start;
                    const std::uint32_t size = pending_cacheflush_size;
                    pending_cacheflush = false;
                    pending_cacheflush_start = 0;
                    pending_cacheflush_size = 0;
                    jit->ClearHalt();
                    if (size != 0) jit->InvalidateCacheRange(start, size);
                    continue;
                }
                if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                    jit->ClearHalt();
                    continue;
                }
                jit->ClearHalt();
            }

            launch.regs = jit->Regs();
            launch.ext_regs = jit->ExtRegs();
            launch.cpsr = jit->Cpsr();
            launch.fpscr = jit->Fpscr();
            launch.tls_values = tls_values_;
            launch.arm_tls = arm_tls_value_;
            launch.total_slices += static_cast<std::size_t>(slices_run);
            if (saw_return || thread_exit_requested_) launch.finished = true;

            if (failed || premature_return || !first_thunk.empty() || jni_unknown_slot != 0xffffffffu) {
                WorkerBoundaryRecord boundary;
                boundary.thread_id = launch.id;
                boundary.thread_start = launch.start;
                boundary.thunk = first_thunk;
                boundary.fault_pc = fault_pc;
                boundary.jni_slot = jni_unknown_slot;
                boundary.regs = jit->Regs();
                const bool duplicate = std::any_of(worker_boundaries.begin(), worker_boundaries.end(),
                    [&](const WorkerBoundaryRecord& existing) {
                        return existing.thread_id == boundary.thread_id &&
                               existing.fault_pc == boundary.fault_pc &&
                               existing.jni_slot == boundary.jni_slot &&
                               existing.thunk == boundary.thunk &&
                               existing.regs[15] == boundary.regs[15];
                    });
                if (!duplicate && worker_boundaries.size() < 32) worker_boundaries.push_back(boundary);
                if (!worker_boundary_seen) {
                    worker_boundary_seen = true;
                    worker_boundary_thread_id = launch.id;
                    worker_boundary_thunk = first_thunk;
                    worker_boundary_fault_pc = fault_pc;
                    worker_boundary_jni_slot = jni_unknown_slot;
                    worker_boundary_regs = jit->Regs();
                }
                // A worker that reached an invalid/unhandled boundary cannot make
                // useful forward progress until a shim is supplied. Quarantine it
                // for the remainder of this diagnostic pass so other workers and
                // the Unity main thread can continue and expose more boundaries.
                launch.finished = true;
            }

            std::ostringstream thread_line;
            thread_line << "id=" << launch.id << " start=0x" << std::hex << launch.start
                        << " pc=0x" << launch.regs[15] << std::dec
                        << " slices=" << slices_run
                        << " total=" << launch.total_slices
                        << " yields=" << cooperative_yields
                        << " finished=" << (launch.finished ? "YES" : "NO")
                        << " failed=" << (failed ? "YES" : "NO");
            if (!first_thunk.empty()) thread_line << " thunk=" << first_thunk;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            const auto recent_calls = guest_thread_call_traces_.find(launch.id);
            if (recent_calls != guest_thread_call_traces_.end() && !recent_calls->second.empty()) {
                thread_line << " recent=[";
                for (std::size_t i = 0; i < recent_calls->second.size(); ++i) {
                    if (i != 0) thread_line << ',';
                    thread_line << recent_calls->second[i];
                }
                thread_line << ']';
            }
            const auto recent_sync = guest_thread_sync_traces_.find(launch.id);
            if (recent_sync != guest_thread_sync_traces_.end() && !recent_sync->second.empty()) {
                thread_line << " sync=[";
                for (std::size_t i = 0; i < recent_sync->second.size(); ++i) {
                    if (i != 0) thread_line << ',';
                    thread_line << recent_sync->second[i];
                }
                thread_line << ']';
            }
#endif
            guest_thread_pump_trace.push_back(thread_line.str());
            ++pumped;
        }

        tls_values_ = saved_tls_values;
        arm_tls_value_ = saved_arm_tls;
        current_thread_id_ = saved_thread_id;
        cooperative_yield_requested_ = false;
        thread_exit_requested_ = false;
        jit->ClearHalt();
        jit->Regs() = saved_regs;
        jit->ExtRegs() = saved_ext_regs;
        jit->SetCpsr(saved_cpsr);
        jit->SetFpscr(saved_fpscr);
        return pumped;
    }

    bool Dispatch(const std::string& name) {
        if (!jit) return false;
        if (DispatchCompatExtras(name)) return true;
#if defined(__ANDROID__)
        if (DispatchGles(name)) return true;
        if (DispatchZlib(name)) return true;
#endif
        if (name == "ALooper_forThread" || name == "ALooper_prepare") {
            Ret(kGuestLooperHandle);
            return true;
        }
        if (name == "ALooper_wake" || name == "AInputQueue_attachLooper" ||
            name == "AInputQueue_detachLooper" || name == "AInputQueue_finishEvent") {
            Ret(0);
            return true;
        }
        if (name == "ALooper_pollAll") {
            if (Arg(1) != 0) Write32(memory_, Arg(1), 0u);
            if (Arg(2) != 0) Write32(memory_, Arg(2), 0u);
            if (Arg(3) != 0) Write32(memory_, Arg(3), 0u);
            Ret(0xffffffffu); // ALOOPER_POLL_TIMEOUT
            return true;
        }
        if (name == "AInputQueue_getEvent") {
            if (Arg(1) != 0) Write32(memory_, Arg(1), 0u);
            Ret(0xffffffffu);
            return true;
        }
        if (name == "AInputQueue_preDispatchEvent") { Ret(0); return true; }
        if (name == "AConfiguration_new") { Ret(kGuestConfigurationHandle); return true; }
        if (name == "AConfiguration_delete" || name == "AConfiguration_fromAssetManager") { Ret(0); return true; }
        if (name == "AInputEvent_getType" || name == "AInputEvent_getSource" ||
            name == "AInputEvent_getDeviceId" || name == "AKeyEvent_getAction" ||
            name == "AKeyEvent_getKeyCode" || name == "AKeyEvent_getMetaState" ||
            name == "AMotionEvent_getAction" || name == "AMotionEvent_getFlags" ||
            name == "AMotionEvent_getMetaState" || name == "AMotionEvent_getEdgeFlags" ||
            name == "AMotionEvent_getHistorySize" || name == "AMotionEvent_getPointerId") {
            Ret(0);
            return true;
        }
        if (name == "AMotionEvent_getPointerCount") { Ret(1); return true; }
        if (name == "AMotionEvent_getDownTime" || name == "AMotionEvent_getEventTime" ||
            name == "AMotionEvent_getHistoricalEventTime") {
            Ret64(0);
            return true;
        }
        if (name == "AMotionEvent_getX" || name == "AMotionEvent_getY" ||
            name == "AMotionEvent_getPressure" || name == "AMotionEvent_getSize" ||
            name == "AMotionEvent_getTouchMajor" || name == "AMotionEvent_getTouchMinor" ||
            name == "AMotionEvent_getToolMajor" || name == "AMotionEvent_getToolMinor" ||
            name == "AMotionEvent_getOrientation" || name == "AMotionEvent_getXPrecision" ||
            name == "AMotionEvent_getYPrecision" || name == "AMotionEvent_getHistoricalX" ||
            name == "AMotionEvent_getHistoricalY" || name == "AMotionEvent_getHistoricalPressure" ||
            name == "AMotionEvent_getHistoricalSize") {
            RetFloat(0.0f);
            return true;
        }
        if (name == "ASensorManager_getInstance") { Ret(kGuestSensorManagerHandle); return true; }
        if (name == "ASensorManager_getDefaultSensor") { Ret(0); return true; }
        if (name == "ASensorManager_getSensorList") {
            if (Arg(1) != 0) Write32(memory_, Arg(1), 0u);
            Ret(0);
            return true;
        }
        if (name == "ASensorManager_createEventQueue") { Ret(kGuestSensorQueueHandle); return true; }
        if (name == "ASensorManager_destroyEventQueue" ||
            name == "ASensorEventQueue_disableSensor" || name == "ASensorEventQueue_enableSensor" ||
            name == "ASensorEventQueue_setEventRate") {
            Ret(0);
            return true;
        }
        if (name == "ASensorEventQueue_getEvents" || name == "ASensorEventQueue_hasEvents") {
            Ret(0);
            return true;
        }
        if (name == "ASensor_getName" || name == "ASensor_getVendor") { Ret(0); return true; }
        if (name == "ASensor_getType" || name == "ASensor_getMinDelay") { Ret(0); return true; }
        if (name == "ASensor_getResolution") { RetFloat(0.0f); return true; }

        // ARM Android uses the soft-float AAPCS at shared-library boundaries, so
        // scalar float/double libm calls arrive in the core-register word stream.
        if (name == "sinf") { RetFloat(std::sin(ArgFloat(0))); return true; }
        if (name == "cosf") { RetFloat(std::cos(ArgFloat(0))); return true; }
        if (name == "tanf") { RetFloat(std::tan(ArgFloat(0))); return true; }
        if (name == "asinf") { RetFloat(std::asin(ArgFloat(0))); return true; }
        if (name == "acosf") { RetFloat(std::acos(ArgFloat(0))); return true; }
        if (name == "atanf") { RetFloat(std::atan(ArgFloat(0))); return true; }
        if (name == "atan2f") { RetFloat(std::atan2(ArgFloat(0), ArgFloat(1))); return true; }
        if (name == "expf") { RetFloat(std::exp(ArgFloat(0))); return true; }
        if (name == "logf") { RetFloat(std::log(ArgFloat(0))); return true; }
        if (name == "sqrtf") { RetFloat(std::sqrt(ArgFloat(0))); return true; }
        if (name == "ceilf") { RetFloat(std::ceil(ArgFloat(0))); return true; }
        if (name == "floorf") { RetFloat(std::floor(ArgFloat(0))); return true; }
        if (name == "fmodf") { RetFloat(std::fmod(ArgFloat(0), ArgFloat(1))); return true; }
        if (name == "powf") { RetFloat(std::pow(ArgFloat(0), ArgFloat(1))); return true; }
        if (name == "__fpclassifyf") { Ret(static_cast<std::uint32_t>(std::fpclassify(ArgFloat(0)))); return true; }
        if (name == "isnan") { Ret(std::isnan(ArgDouble(0)) ? 1u : 0u); return true; }
        if (name == "sin") { RetDouble(std::sin(ArgDouble(0))); return true; }
        if (name == "cos") { RetDouble(std::cos(ArgDouble(0))); return true; }
        if (name == "tan") { RetDouble(std::tan(ArgDouble(0))); return true; }
        if (name == "asin") { RetDouble(std::asin(ArgDouble(0))); return true; }
        if (name == "acos") { RetDouble(std::acos(ArgDouble(0))); return true; }
        if (name == "atan") { RetDouble(std::atan(ArgDouble(0))); return true; }
        if (name == "atan2") { RetDouble(std::atan2(ArgDouble(0), ArgDouble(2))); return true; }
        if (name == "exp") { RetDouble(std::exp(ArgDouble(0))); return true; }
        if (name == "log") { RetDouble(std::log(ArgDouble(0))); return true; }
        if (name == "log10") { RetDouble(std::log10(ArgDouble(0))); return true; }
        if (name == "sqrt") { RetDouble(std::sqrt(ArgDouble(0))); return true; }
        if (name == "ceil") { RetDouble(std::ceil(ArgDouble(0))); return true; }
        if (name == "floor") { RetDouble(std::floor(ArgDouble(0))); return true; }
        if (name == "rint") { RetDouble(std::rint(ArgDouble(0))); return true; }
        if (name == "round") { RetDouble(std::round(ArgDouble(0))); return true; }
        if (name == "trunc") { RetDouble(std::trunc(ArgDouble(0))); return true; }
        if (name == "sinh") { RetDouble(std::sinh(ArgDouble(0))); return true; }
        if (name == "cosh") { RetDouble(std::cosh(ArgDouble(0))); return true; }
        if (name == "tanh") { RetDouble(std::tanh(ArgDouble(0))); return true; }
        if (name == "fmod") { RetDouble(std::fmod(ArgDouble(0), ArgDouble(2))); return true; }
        if (name == "pow") { RetDouble(std::pow(ArgDouble(0), ArgDouble(2))); return true; }
        if (name == "ldexp") { RetDouble(std::ldexp(ArgDouble(0), static_cast<int>(Arg(2)))); return true; }
        if (name == "frexp") {
            int exponent = 0;
            const double value = std::frexp(ArgDouble(0), &exponent);
            if (Arg(2)) Write32(memory_, Arg(2), static_cast<std::uint32_t>(exponent));
            RetDouble(value);
            return true;
        }
        if (name == "modff") {
            float integral = 0.0f;
            const float fraction = std::modf(ArgFloat(0), &integral);
            if (Arg(1) && Fits(Arg(1), 4)) std::memcpy(memory_.data() + Arg(1), &integral, 4);
            RetFloat(fraction);
            return true;
        }
        if (name == "div") {
            const std::int32_t denom = static_cast<std::int32_t>(Arg(1));
            const std::int32_t numer = static_cast<std::int32_t>(Arg(0));
            if (!denom) return false;
            jit->Regs()[0] = static_cast<std::uint32_t>(numer / denom);
            jit->Regs()[1] = static_cast<std::uint32_t>(numer % denom);
            return true;
        }

        if (name == "__aeabi_memmove") {
            const std::uint32_t dst = Arg(0), src = Arg(1), n = Arg(2);
            if (!Fits(dst, n) || !Fits(src, n)) return false;
            std::memmove(memory_.data() + dst, memory_.data() + src, n);
            Ret(dst);
            return true;
        }
        if (name == "__aeabi_memset") {
            const std::uint32_t dst = Arg(0), n = Arg(1);
            if (!Fits(dst, n)) return false;
            std::memset(memory_.data() + dst, static_cast<int>(Arg(2) & 0xffu), n);
            Ret(dst);
            return true;
        }
        if (name == "memchr") {
            const std::uint32_t ptr = Arg(0), n = Arg(2);
            if (!Fits(ptr, n)) return false;
            void* found = std::memchr(memory_.data() + ptr, static_cast<int>(Arg(1)), n);
            Ret(found ? ptr + static_cast<std::uint32_t>(static_cast<std::uint8_t*>(found) - (memory_.data() + ptr)) : 0u);
            return true;
        }
        if (name == "memmem") {
            const std::uint32_t hay = Arg(0), hay_len = Arg(1), needle = Arg(2), needle_len = Arg(3);
            if (!Fits(hay, hay_len) || !Fits(needle, needle_len)) return false;
            if (needle_len == 0) { Ret(hay); return true; }
            std::uint32_t result = 0;
            for (std::uint32_t i = 0; i + needle_len <= hay_len; ++i) {
                if (std::memcmp(memory_.data() + hay + i, memory_.data() + needle, needle_len) == 0) { result = hay + i; break; }
            }
            Ret(result);
            return true;
        }
        if (name == "memalign") { Ret(Allocate(Arg(1), std::max<std::uint32_t>(Arg(0), 1u))); return true; }

        if (name == "strpbrk") {
            const std::string src = ReadCString(Arg(0)), accept = ReadCString(Arg(1));
            const std::size_t pos = src.find_first_of(accept);
            Ret(pos == std::string::npos ? 0u : Arg(0) + static_cast<std::uint32_t>(pos));
            return true;
        }
        if (name == "strcasestr") {
            std::string hay = ReadCString(Arg(0)), needle = ReadCString(Arg(1));
            std::transform(hay.begin(), hay.end(), hay.begin(), [](unsigned char c){ return static_cast<char>(std::tolower(c)); });
            std::transform(needle.begin(), needle.end(), needle.begin(), [](unsigned char c){ return static_cast<char>(std::tolower(c)); });
            const std::size_t pos = hay.find(needle);
            Ret(pos == std::string::npos ? 0u : Arg(0) + static_cast<std::uint32_t>(pos));
            return true;
        }
        if (name == "strncat") {
            const std::uint32_t dst = Arg(0);
            const std::string addition = ReadCString(Arg(1)).substr(0, Arg(2));
            if (!WriteCString(dst, ReadCString(dst) + addition)) return false;
            Ret(dst); return true;
        }
        if (name == "strcoll") { Ret(static_cast<std::uint32_t>(ReadCString(Arg(0)).compare(ReadCString(Arg(1))))); return true; }
        if (name == "strxfrm") {
            const std::string src = ReadCString(Arg(1));
            if (Arg(2) && Arg(0)) WriteCString(Arg(0), src, Arg(2));
            Ret(static_cast<std::uint32_t>(src.size())); return true;
        }
        if (name == "strtoull") {
            const std::string text = ReadCString(Arg(0));
            char* end = nullptr;
            const unsigned long long value = std::strtoull(text.c_str(), &end, static_cast<int>(Arg(2)));
            if (Arg(1)) Write32(memory_, Arg(1), Arg(0) + static_cast<std::uint32_t>(end - text.c_str()));
            Ret64(static_cast<std::uint64_t>(value)); return true;
        }
        if (name == "strtod") {
            const std::string text = ReadCString(Arg(0));
            char* end = nullptr;
            const double value = std::strtod(text.c_str(), &end);
            if (Arg(1)) Write32(memory_, Arg(1), Arg(0) + static_cast<std::uint32_t>(end - text.c_str()));
            RetDouble(value); return true;
        }

        if (name == "access") {
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            if (file_trace.size() < 24) file_trace.push_back("access " + path);
            errno = 0;
#if defined(_WIN32)
            const int result = ::_access(path.c_str(), static_cast<int>(Arg(1)));
#else
            const int result = ::access(path.c_str(), static_cast<int>(Arg(1)));
#endif
            SetErrno(result == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(result));
            return true;
        }
        if (name == "__errno" || name == "__get_h_errno") {
            Ret(kBootstrapRuntimeBase);
            return true;
        }
        if (name == "malloc") {
            Ret(Allocate(Arg(0)));
            return true;
        }
        if (name == "calloc") {
            const std::uint64_t bytes = static_cast<std::uint64_t>(Arg(0)) * Arg(1);
            Ret(bytes > 0x01000000u ? 0 : Allocate(static_cast<std::size_t>(bytes)));
            return true;
        }
        if (name == "free") {
            Ret(0);
            return true;
        }
        if (name == "realloc") {
            const std::uint32_t old_ptr = Arg(0);
            const std::size_t new_size = Arg(1);
            if (!old_ptr) {
                Ret(Allocate(new_size));
                return true;
            }
            if (new_size == 0) {
                Ret(0);
                return true;
            }
            const std::uint32_t fresh = Allocate(new_size);
            if (!fresh) {
                Ret(0);
                return true;
            }
            const auto old = allocations_.find(old_ptr);
            if (old != allocations_.end() && Fits(old_ptr, old->second)) {
                std::memcpy(memory_.data() + fresh, memory_.data() + old_ptr,
                            std::min<std::size_t>(old->second, new_size));
            }
            Ret(fresh);
            return true;
        }
        if (name == "memcpy" || name == "__aeabi_memcpy") {
            const std::uint32_t dst = Arg(0), src = Arg(1), n = Arg(2);
            if (!Fits(dst, n) || !Fits(src, n)) return false;
            std::memcpy(memory_.data() + dst, memory_.data() + src, n);
            Ret(dst);
            return true;
        }
        if (name == "memmove") {
            const std::uint32_t dst = Arg(0), src = Arg(1), n = Arg(2);
            if (!Fits(dst, n) || !Fits(src, n)) return false;
            std::memmove(memory_.data() + dst, memory_.data() + src, n);
            Ret(dst);
            return true;
        }
        if (name == "memset") {
            const std::uint32_t dst = Arg(0), n = Arg(2);
            if (!Fits(dst, n)) return false;
            std::memset(memory_.data() + dst, static_cast<int>(Arg(1) & 0xffu), n);
            Ret(dst);
            return true;
        }
        if (name == "memcmp") {
            const std::uint32_t a = Arg(0), b = Arg(1), n = Arg(2);
            if (!Fits(a, n) || !Fits(b, n)) return false;
            Ret(static_cast<std::uint32_t>(std::memcmp(memory_.data() + a, memory_.data() + b, n)));
            return true;
        }
        if (name == "strlen") {
            Ret(static_cast<std::uint32_t>(ReadCString(Arg(0)).size()));
            return true;
        }
        if (name == "strcmp" || name == "strcasecmp" || name == "strncmp" || name == "strncasecmp") {
            std::string a = ReadCString(Arg(0));
            std::string b = ReadCString(Arg(1));
            std::size_t limit = (name == "strncmp" || name == "strncasecmp") ? Arg(2) : SIZE_MAX;
            if (name == "strcasecmp" || name == "strncasecmp") {
                std::transform(a.begin(), a.end(), a.begin(), [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
                std::transform(b.begin(), b.end(), b.begin(), [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
            }
            const std::size_t n = std::min({limit, a.size() + 1, b.size() + 1});
            int result = 0;
            for (std::size_t i = 0; i < n; ++i) {
                const unsigned char ca = i < a.size() ? static_cast<unsigned char>(a[i]) : 0;
                const unsigned char cb = i < b.size() ? static_cast<unsigned char>(b[i]) : 0;
                if (ca != cb) { result = ca < cb ? -1 : 1; break; }
                if (ca == 0) break;
            }
            Ret(static_cast<std::uint32_t>(result));
            return true;
        }
        if (name == "strcpy" || name == "strncpy" || name == "strlcpy") {
            const std::uint32_t dst = Arg(0);
            const std::string src = ReadCString(Arg(1));
            if (name == "strcpy") {
                if (!WriteCString(dst, src)) return false;
                Ret(dst);
            } else if (name == "strncpy") {
                const std::size_t n = Arg(2);
                if (!Fits(dst, n)) return false;
                const std::size_t copy = std::min(n, src.size());
                if (copy) std::memcpy(memory_.data() + dst, src.data(), copy);
                if (n > copy) std::memset(memory_.data() + dst + copy, 0, n - copy);
                Ret(dst);
            } else {
                const std::size_t n = Arg(2);
                if (n && !WriteCString(dst, src, n)) return false;
                Ret(static_cast<std::uint32_t>(src.size()));
            }
            return true;
        }
        if (name == "strdup") {
            const std::string src = ReadCString(Arg(0));
            const std::uint32_t dst = Allocate(src.size() + 1);
            if (dst) WriteCString(dst, src);
            Ret(dst);
            return true;
        }
        if (name == "strchr" || name == "strrchr") {
            const std::string src = ReadCString(Arg(0));
            const char needle = static_cast<char>(Arg(1));
            const std::size_t pos = name == "strchr" ? src.find(needle) : src.rfind(needle);
            Ret(pos == std::string::npos ? 0 : Arg(0) + static_cast<std::uint32_t>(pos));
            return true;
        }
        if (name == "strstr") {
            const std::string hay = ReadCString(Arg(0));
            const std::string needle = ReadCString(Arg(1));
            const std::size_t pos = hay.find(needle);
            Ret(pos == std::string::npos ? 0 : Arg(0) + static_cast<std::uint32_t>(pos));
            return true;
        }
        if (name == "strcat") {
            const std::uint32_t dst = Arg(0);
            const std::string combined = ReadCString(dst) + ReadCString(Arg(1));
            if (!WriteCString(dst, combined)) return false;
            Ret(dst);
            return true;
        }
        if (name == "getenv") {
            Ret(0);
            return true;
        }
        if (name == "setenv" || name == "unsetenv" || name == "chdir" || name == "chmod") {
            Ret(0);
            return true;
        }
        if (name == "getcwd") {
            const std::uint32_t buffer = Arg(0), size = Arg(1);
            if (!buffer || size < 2 || !WriteCString(buffer, "/", size)) Ret(0);
            else Ret(buffer);
            return true;
        }
        if (name == "setlocale") {
            Ret(kBootstrapRuntimeBase + 0x100);
            return true;
        }
        // Minimal C-locale wide-character support used by libstdc++ locale facets
        // during Unity's global constructor phase. Android ARM32 wchar_t/wint_t are
        // 32-bit here, and the C locale maps bytes 1:1 to Unicode code points 0..255.
        if (name == "wctob") {
            const std::uint32_t wc = Arg(0);
            Ret(wc <= 0xffu ? wc : 0xffffffffu); // EOF if not single-byte representable
            return true;
        }
        if (name == "btowc") {
            const std::uint32_t c = Arg(0);
            Ret(c == 0xffffffffu ? 0xffffffffu : (c & 0xffu)); // WEOF or byte value
            return true;
        }
        if (name == "mbrtowc") {
            const std::uint32_t out = Arg(0);
            const std::uint32_t src = Arg(1);
            const std::uint32_t count = Arg(2);
            if (!src) { Ret(0); return true; }
            if (count == 0) { Ret(0xfffffffeu); return true; } // (size_t)-2, incomplete
            const std::uint8_t c = Read<std::uint8_t>(src);
            if (out && Fits(out, 4)) Write32(memory_, out, static_cast<std::uint32_t>(c));
            Ret(c == 0 ? 0u : 1u);
            return true;
        }
        if (name == "wcrtomb") {
            const std::uint32_t out = Arg(0);
            const std::uint32_t wc = Arg(1);
            if (!out) { Ret(1); return true; } // reset conversion state in C locale
            if (wc > 0xffu || !Fits(out, 1)) { Ret(0xffffffffu); return true; }
            memory_[out] = static_cast<std::uint8_t>(wc);
            Ret(1);
            return true;
        }
        if (name == "wctype") {
            const std::string cls = ReadCString(Arg(0));
            static const std::unordered_map<std::string, std::uint32_t> classes = {
                {"alnum",1},{"alpha",2},{"blank",3},{"cntrl",4},{"digit",5},{"graph",6},
                {"lower",7},{"print",8},{"punct",9},{"space",10},{"upper",11},{"xdigit",12}
            };
            const auto it = classes.find(cls);
            Ret(it == classes.end() ? 0u : it->second);
            return true;
        }
        if (name == "iswctype") {
            const std::uint32_t wc = Arg(0);
            const std::uint32_t cls = Arg(1);
            const bool ascii = wc <= 0x7fu;
            const unsigned char c = static_cast<unsigned char>(wc & 0xffu);
            bool match = false;
            if (ascii) {
                switch (cls) {
                case 1: match = std::isalnum(c) != 0; break;
                case 2: match = std::isalpha(c) != 0; break;
                case 3: match = c == ' ' || c == '\t'; break;
                case 4: match = std::iscntrl(c) != 0; break;
                case 5: match = std::isdigit(c) != 0; break;
                case 6: match = std::isgraph(c) != 0; break;
                case 7: match = std::islower(c) != 0; break;
                case 8: match = std::isprint(c) != 0; break;
                case 9: match = std::ispunct(c) != 0; break;
                case 10: match = std::isspace(c) != 0; break;
                case 11: match = std::isupper(c) != 0; break;
                case 12: match = std::isxdigit(c) != 0; break;
                default: break;
                }
            }
            Ret(match ? 1u : 0u);
            return true;
        }
        if (name == "towlower" || name == "towupper") {
            const std::uint32_t wc = Arg(0);
            if (wc <= 0x7fu) {
                const unsigned char c = static_cast<unsigned char>(wc);
                Ret(static_cast<std::uint32_t>(name == "towlower" ? std::tolower(c) : std::toupper(c)));
            } else {
                Ret(wc);
            }
            return true;
        }
        if (name == "wcslen") {
            const std::uint32_t src = Arg(0);
            std::uint32_t length = 0;
            if (src) {
                while (length < 0x100000u && Fits(src + length * 4u, 4)) {
                    std::uint32_t wc = 0;
                    Read32(memory_, src + length * 4u, wc);
                    if (wc == 0) break;
                    ++length;
                }
            }
            Ret(length);
            return true;
        }
        if (name == "wcscoll") {
            std::uint32_t a = Arg(0), b = Arg(1);
            for (std::size_t i = 0; i < 0x100000u; ++i) {
                std::uint32_t ca = 0, cb = 0;
                if (!Read32(memory_, a + static_cast<std::uint32_t>(i * 4u), ca) ||
                    !Read32(memory_, b + static_cast<std::uint32_t>(i * 4u), cb)) {
                    Ret(0); return true;
                }
                if (ca != cb) { Ret(static_cast<std::uint32_t>(ca < cb ? -1 : 1)); return true; }
                if (ca == 0) { Ret(0); return true; }
            }
            Ret(0);
            return true;
        }
        if (name == "wcsxfrm") {
            const std::uint32_t dst = Arg(0), src = Arg(1), count = Arg(2);
            std::uint32_t length = 0;
            while (length < 0x100000u) {
                std::uint32_t wc = 0;
                if (!Read32(memory_, src + length * 4u, wc) || wc == 0) break;
                ++length;
            }
            if (dst && count) {
                const std::uint32_t copy_count = std::min(length, count - 1u);
                for (std::uint32_t i = 0; i < copy_count; ++i) {
                    std::uint32_t wc = 0;
                    Read32(memory_, src + i * 4u, wc);
                    Write32(memory_, dst + i * 4u, wc);
                }
                Write32(memory_, dst + copy_count * 4u, 0u);
            }
            Ret(length);
            return true;
        }
        if (name == "wcsftime") {
            // Locale facet construction only needs this symbol to be callable.
            // Full date formatting is deferred until real gameplay requests it.
            if (Arg(0) && Arg(1) > 0 && Fits(Arg(0), 4)) Write32(memory_, Arg(0), 0u);
            Ret(0);
            return true;
        }
        if (name == "__system_property_get") {
            const std::string property = ReadCString(Arg(0), 256);
            static const std::unordered_map<std::string, std::string> properties = {
                {"ro.build.version.sdk", "35"},
                {"ro.build.version.release", "15"},
                {"ro.build.version.codename", "REL"},
                {"ro.product.cpu.abi", "armeabi-v7a"},
                {"ro.product.cpu.abi2", "armeabi"},
                {"ro.product.cpu.abilist", "arm64-v8a,armeabi-v7a,armeabi"},
                {"ro.product.cpu.abilist32", "armeabi-v7a,armeabi"},
                {"ro.product.cpu.abilist64", "arm64-v8a"},
                {"ro.product.manufacturer", "Google"},
                {"ro.product.brand", "google"},
                {"ro.product.model", "CNR64"},
                {"ro.product.device", "cnr64"},
                {"ro.hardware", "goldfish"},
                {"ro.kernel.qemu", "1"},
                {"ro.debuggable", "0"},
                {"ro.secure", "1"}
            };
            const auto found = properties.find(property);
            const std::string value = found == properties.end() ? std::string{} : found->second;
            if (Arg(1) != 0 && !WriteCString(Arg(1), value, 92)) return false;
            if (file_trace.size() < 32) file_trace.push_back("property " + property + "=" + value);
            Ret(static_cast<std::uint32_t>(value.size()));
            return true;
        }
        if (name == "sysconf") {
            const std::uint32_t key = Arg(0);
            if (key == 39) Ret(4096);             // _SC_PAGESIZE on bionic/glibc
            else if (key == 84) Ret(4);           // _SC_NPROCESSORS_ONLN
            else if (key == 2) Ret(100);           // _SC_CLK_TCK
            else Ret(4);
            return true;
        }
        if (name == "getdtablesize") {
#if !defined(_WIN32)
            const long limit = ::sysconf(_SC_OPEN_MAX);
            Ret(limit > 0 ? static_cast<std::uint32_t>(std::min<long>(limit, 0x7fffffffL)) : 1024u);
#else
            Ret(1024u);
#endif
            return true;
        }
        if (name == "getpid") { Ret(1234); return true; }
        if (name == "gettid") { Ret(current_thread_id_); return true; }
        if (name == "getuid" || name == "geteuid" || name == "getresuid") { Ret(1000); return true; }
        if (name == "nanosleep") {
            // Old Unity uses nanosleep for main-thread pacing. A pure no-op makes
            // some render paths spin far faster than the original Android runtime.
            // Preserve cooperative waits on guest workers, but give the guest main
            // thread a bounded host-time sleep. The 1 ms cap keeps legacy callers
            // from freezing the wrapper when they pass unexpectedly large values.
            if (current_thread_id_ == 1) {
                const std::uint32_t request = Arg(0);
                if (!request || !Fits(request, 8u)) { Ret(0xffffffffu); return true; }
                std::uint32_t seconds = 0, nanoseconds = 0;
                Read32(memory_, request, seconds);
                Read32(memory_, request + 4u, nanoseconds);
                nanoseconds = std::min<std::uint32_t>(nanoseconds, 999999999u);
                std::uint64_t total_ns = static_cast<std::uint64_t>(seconds) * 1000000000ull + nanoseconds;
                total_ns = std::min<std::uint64_t>(total_ns, 1000000ull);
                if (total_ns != 0) std::this_thread::sleep_for(std::chrono::nanoseconds(total_ns));
                if (Arg(1) && Fits(Arg(1), 8u)) {
                    Write32(memory_, Arg(1), 0u);
                    Write32(memory_, Arg(1) + 4u, 0u);
                }
                Ret(0);
                return true;
            }
            Ret(0);
            if (jit) {
                // The guest call is complete. CallSVC exposes the already-advanced
                // guest PC, so preserve it while yielding and let the thunk return normally.
                cooperative_yield_requested_ = true;
                jit->HaltExecution();
            }
            return true;
        }
        if (name == "sched_yield" || name == "sleep" || name == "usleep") {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (name == "usleep" && guest_sleep_log_counts_[current_thread_id_]++ < 4u) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SLEEP usleep thread=%u usec=%u sp=0x%08x lr=0x%08x pc=0x%08x",
                                    current_thread_id_, Arg(0), jit ? jit->Regs()[13] : 0u,
                                    jit ? jit->Regs()[14] : 0u, jit ? jit->Regs()[15] : 0u);
            }
#endif
            Ret(0);
            if (jit && (current_thread_id_ != 1 || allow_main_thread_cooperative_yield)) {
                // These calls have completed too. Preserve Dynarmic's advanced PC
                // while the scheduler runs another guest thread.
                cooperative_yield_requested_ = true;
                jit->HaltExecution();
            }
            return true;
        }
        if (name == "clock") { Ret(0); return true; }
        if (name == "clock_gettime" || name == "clock_getres") {
            const std::uint32_t ts = Arg(1);
            if (!Fits(ts, 8)) return false;
            const auto now = std::chrono::system_clock::now().time_since_epoch();
            const auto sec = std::chrono::duration_cast<std::chrono::seconds>(now);
            const auto nsec = std::chrono::duration_cast<std::chrono::nanoseconds>(now - sec);
            Write32(memory_, ts, static_cast<std::uint32_t>(sec.count()));
            Write32(memory_, ts + 4, name == "clock_getres" ? 1000000u : static_cast<std::uint32_t>(nsec.count()));
            Ret(0);
            return true;
        }
        if (name == "gettimeofday") {
            const std::uint32_t tv = Arg(0);
            if (tv) {
                if (!Fits(tv, 8)) return false;
                const auto now = std::chrono::system_clock::now().time_since_epoch();
                const auto sec = std::chrono::duration_cast<std::chrono::seconds>(now);
                const auto usec = std::chrono::duration_cast<std::chrono::microseconds>(now - sec);
                Write32(memory_, tv, static_cast<std::uint32_t>(sec.count()));
                Write32(memory_, tv + 4, static_cast<std::uint32_t>(usec.count()));
            }
            Ret(0);
            return true;
        }
        if (name == "time") {
            const auto seconds = std::chrono::duration_cast<std::chrono::seconds>(
                std::chrono::system_clock::now().time_since_epoch()).count();
            if (Arg(0)) Write32(memory_, Arg(0), static_cast<std::uint32_t>(seconds));
            Ret(static_cast<std::uint32_t>(seconds));
            return true;
        }
        if (name == "mmap") {
            const std::uint32_t requested = Arg(0);
            const std::size_t length = Arg(1);
            const std::uint32_t prot = Arg(2);
            const std::uint32_t flags = Arg(3);
            const int fd = static_cast<int>(Arg(4));
            const std::uint32_t offset = Arg(5);
            if (length == 0 || length > UINT32_MAX) { SetErrno(22); Ret(0xffffffffu); return true; }
            const std::uint32_t ptr = Allocate(AlignUp(static_cast<std::uint32_t>(length), 4096), 4096);
            if (!ptr) { SetErrno(12); Ret(0xffffffffu); return true; }
            if (file_trace.size() < 32) {
                std::ostringstream line;
                line << "mmap req=0x" << std::hex << requested << " len=0x" << length
                     << " prot=0x" << prot << " flags=0x" << flags << std::dec
                     << " fd=" << fd << " off=" << offset << " -> guest=0x" << std::hex << ptr << std::dec;
                file_trace.push_back(line.str());
            }
#if !defined(_WIN32)
            if (fd >= 0) {
                std::size_t copied = 0;
                while (copied < length) {
                    const ssize_t rc = ::pread(fd,
                                               memory_.data() + ptr + copied,
                                               length - copied,
                                               static_cast<off_t>(offset) + static_cast<off_t>(copied));
                    if (rc < 0) { SetErrno(static_cast<std::uint32_t>(errno)); Ret(0xffffffffu); return true; }
                    if (rc == 0) break;
                    copied += static_cast<std::size_t>(rc);
                }
                if (file_trace.size() < 32) {
                    std::ostringstream line;
                    line << "mmap fd=" << fd << " offset=" << offset << " length=" << length
                         << " -> guest=0x" << std::hex << ptr << std::dec << " copied=" << copied;
                    file_trace.push_back(line.str());
                }
            }
#endif
            SetErrno(0);
            Ret(ptr);
            return true;
        }
        if (name == "mprotect") {
            const std::uint32_t address = Arg(0);
            const std::uint32_t length = Arg(1);
            const std::uint32_t prot = Arg(2);
            last_mprotect_address = address;
            last_mprotect_length = length;
            last_mprotect_prot = prot;
            if (jit) {
                const std::uint32_t fp = jit->Regs()[11];
                if (fp && Fits(fp, 4)) last_mprotect_caller_lr = Read<std::uint32_t>(fp);
            }
            if (file_trace.size() < 32) {
                std::ostringstream line;
                line << "mprotect addr=0x" << std::hex << address << " len=0x" << length
                     << " prot=0x" << prot << std::dec;
                file_trace.push_back(line.str());
            }
            if ((prot & 0x4u) != 0) { // PROT_EXEC
                ++executable_mprotect_calls;
                if (first_exec_region == 0) {
                    first_exec_region = address;
                    first_exec_region_size = length;
                }
                // Only executable mappings need Dynarmic code-cache invalidation, and it
                // must happen after jit.Run() returns rather than recursively in the callback.
                pending_cacheflush = true;
                pending_cacheflush_start = address;
                pending_cacheflush_size = length;
                if (jit) jit->HaltExecution();
            }
            Ret(0);
            return true;
        }
        if (name == "munmap" || name == "madvise") { Ret(0); return true; }
        if (name == "mremap") {
            const std::uint32_t fresh = Allocate(Arg(2), 4096);
            if (fresh && Arg(0) && Fits(Arg(0), std::min(Arg(1), Arg(2))))
                std::memcpy(memory_.data() + fresh, memory_.data() + Arg(0), std::min(Arg(1), Arg(2)));
            Ret(fresh ? fresh : 0xffffffffu);
            return true;
        }
        if (name == "pthread_once") {
            const std::uint32_t control = Arg(0);
            const std::uint32_t initializer = Arg(1);
            if (!control || !initializer) {
                Ret(22u); // EINVAL
                return true;
            }
            if (pthread_once_done_[control]) {
                Ret(0);
                return true;
            }
            pthread_once_frames_.push_back({control, jit->Regs()[14]});
            if (Fits(control, 4)) Write32(memory_, control, 1u);
            ++pthread_once_callbacks;
            jit->Regs()[0] = 0;
            jit->Regs()[1] = 0;
            jit->Regs()[2] = 0;
            jit->Regs()[3] = 0;
            jit->Regs()[14] = kPthreadOnceReturnStub;
            jit->Regs()[15] = initializer & ~1u;
            jit->SetCpsr((initializer & 1u) ? 0x20u : 0u);
            pending_guest_callback = true;
            jit->HaltExecution();
            return true;
        }
        if (name == "pthread_create") {
            const std::uint32_t thread_id = next_thread_id_++;
            if (Arg(0)) Write32(memory_, Arg(0), thread_id);
            guest_thread_launches_.push_back({thread_id, Arg(2), Arg(3), false});
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SYNC pthread_create creator=%u id=%u start=0x%08x arg=0x%08x",
                                current_thread_id_, thread_id, Arg(2), Arg(3));
#endif
            if (jit) {
                // Let the newly-created guest thread reach its initial wait point
                // before the creator can signal it. On a real OS pthread_create
                // makes the child runnable immediately; delaying first execution
                // until a later main-thread wait can lose legitimate cond signals.
                // pthread_create itself has completed. Preserve Dynarmic's advanced
                // guest PC while giving the newly-runnable child a scheduling point.
                cooperative_yield_requested_ = true;
                jit->HaltExecution();
            }
            if (file_trace.size() < 32) {
                std::ostringstream line;
                line << "pthread_create id=" << thread_id
                     << " start=0x" << std::hex << Arg(2)
                     << " arg=0x" << Arg(3) << std::dec;
                file_trace.push_back(line.str());
            }
            Ret(0);
            return true;
        }
        if (name == "pthread_getschedparam") {
            if (Arg(1) != 0 && !Write32(memory_, Arg(1), 0u)) return false; // SCHED_OTHER
            if (Arg(2) != 0 && !Write32(memory_, Arg(2), 0u)) return false; // sched_priority
            Ret(0);
            return true;
        }
        if (name == "sched_get_priority_max" || name == "sched_get_priority_min") {
            const int policy = static_cast<int>(Arg(0));
            if (policy == 1 || policy == 2) Ret(name == "sched_get_priority_max" ? 99u : 1u);
            else Ret(0);
            return true;
        }
        auto block_current_sync_call = [&](const std::string& symbol) -> bool {
            if (!jit) return false;
            const auto stub = name_to_stub_.find(symbol);
            if (stub == name_to_stub_.end()) return false;
            jit->Regs()[15] = stub->second;
            cooperative_yield_requested_ = true;
            jit->HaltExecution();
            return true;
        };
        auto make_wait_key = [&](std::uint32_t object) -> std::uint64_t {
            return (static_cast<std::uint64_t>(current_thread_id_) << 32) | object;
        };
        auto decode_deadline = [&](std::uint32_t guest_timespec,
                                   std::chrono::system_clock::time_point& out) -> bool {
            if (!guest_timespec || !Fits(guest_timespec, 8u)) return false;
            std::uint32_t seconds = 0;
            std::uint32_t nanoseconds = 0;
            if (!Read32(memory_, guest_timespec, seconds) ||
                !Read32(memory_, guest_timespec + 4u, nanoseconds)) return false;
            if (nanoseconds >= 1000000000u) return false;
            const auto duration = std::chrono::duration_cast<std::chrono::system_clock::duration>(
                std::chrono::seconds(seconds) + std::chrono::nanoseconds(nanoseconds));
            out = std::chrono::system_clock::time_point(duration);
            return true;
        };

        if (name == "pthread_exit") {
            Ret(0);
            if (current_thread_id_ != 1 && jit) {
                thread_exit_requested_ = true;
                jit->HaltExecution();
            }
            return true;
        }
        if (name == "pthread_mutexattr_init") {
            guest_mutex_attr_types_[Arg(0)] = 0u;
            Ret(0);
            return true;
        }
        if (name == "pthread_mutexattr_destroy") {
            guest_mutex_attr_types_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (name == "pthread_mutexattr_settype") {
            guest_mutex_attr_types_[Arg(0)] = Arg(1);
            Ret(0);
            return true;
        }
        if (name == "pthread_mutex_init") {
            GuestMutexState state{};
            const auto attr = guest_mutex_attr_types_.find(Arg(1));
            if (Arg(1) != 0 && attr != guest_mutex_attr_types_.end()) state.type = attr->second;
            guest_mutexes_[Arg(0)] = state;
            Ret(0);
            return true;
        }
        if (name == "pthread_mutex_destroy") {
            guest_mutexes_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (name == "pthread_mutex_lock" || name == "pthread_mutex_trylock") {
            if (Arg(0) == 0) { Ret(22u); return true; } // EINVAL
            auto& mutex = guest_mutexes_[Arg(0)];
            if (mutex.owner == 0) {
                mutex.owner = current_thread_id_;
                mutex.recursion = 1;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                RecordSyncEvent("lock", Arg(0));
#endif
                Ret(0);
                return true;
            }
            if (mutex.owner == current_thread_id_ && mutex.type == 1u) {
                ++mutex.recursion;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                RecordSyncEvent("relock", Arg(0), mutex.recursion);
#endif
                Ret(0);
                return true;
            }
            if (name == "pthread_mutex_trylock") {
                Ret(16u); // EBUSY
                return true;
            }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            const std::uint64_t block_key = (static_cast<std::uint64_t>(current_thread_id_) << 32) | Arg(0);
            if (!guest_mutex_block_logged_[block_key]) {
                guest_mutex_block_logged_[block_key] = true;
                std::uint32_t raw_mutex = 0;
                Read32(memory_, Arg(0), raw_mutex);
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7MUTEX block thread=%u mutex=0x%08x owner=%u recursion=%u type=%u raw=0x%08x lr=0x%08x",
                                    current_thread_id_, Arg(0), mutex.owner, mutex.recursion, mutex.type,
                                    raw_mutex, jit ? jit->Regs()[14] : 0u);
                const auto sync_trace = guest_thread_sync_traces_.find(current_thread_id_);
                if (sync_trace != guest_thread_sync_traces_.end()) {
                    const std::size_t start = sync_trace->second.size() > 24 ? sync_trace->second.size() - 24 : 0;
                    for (std::size_t i = start; i < sync_trace->second.size(); ++i) {
                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                            "PV7MUTEX prior[%zu]=%s",
                                            i - start, sync_trace->second[i].c_str());
                    }
                }
            }
#endif
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            RecordSyncEvent("block", Arg(0), mutex.owner);
#endif
            if (!block_current_sync_call(name)) return false;
            return true;
        }
        if (name == "pthread_mutex_unlock") {
            auto found = guest_mutexes_.find(Arg(0));
            if (found == guest_mutexes_.end() || found->second.owner == 0) {
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                RecordSyncEvent("unlock-noop", Arg(0),
                                found == guest_mutexes_.end() ? 0xffffffffu : 0u);
#endif
                Ret(0);
                return true;
            }
            if (found->second.owner != current_thread_id_) {
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                RecordSyncEvent("unlock-EPERM", Arg(0), found->second.owner);
#endif
                Ret(1u); // EPERM
                return true;
            }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            RecordSyncEvent("unlock", Arg(0), found->second.owner);
#endif
            if (found->second.recursion > 1) --found->second.recursion;
            else {
                found->second.owner = 0;
                found->second.recursion = 0;
            }
            Ret(0);
            return true;
        }
        if (name == "pthread_cond_init") {
            guest_conds_[Arg(0)] = {};
            Ret(0);
            return true;
        }
        if (name == "pthread_cond_destroy") {
            guest_conds_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (name == "pthread_cond_signal") {
            const std::uint32_t cond = Arg(0);
            bool signaled_waiter = false;
            for (auto& waiter : guest_cond_waits_) {
                if (static_cast<std::uint32_t>(waiter.first) == cond && !waiter.second.signaled) {
                    waiter.second.signaled = true;
                    signaled_waiter = true;
                    break;
                }
            }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7COND signal thread=%u cond=0x%08x waiter=%s",
                                current_thread_id_, cond, signaled_waiter ? "YES" : "NO");
#endif
            Ret(0);
            return true;
        }
        if (name == "pthread_cond_broadcast") {
            const std::uint32_t cond = Arg(0);
            ++guest_conds_[cond].broadcast_generation;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7COND broadcast thread=%u cond=0x%08x generation=%llu",
                                current_thread_id_, cond,
                                static_cast<unsigned long long>(guest_conds_[cond].broadcast_generation));
#endif
            Ret(0);
            return true;
        }
        if (name == "pthread_cond_wait" || name == "pthread_cond_timedwait" ||
            name == "pthread_cond_timedwait_monotonic_np") {
            const std::uint32_t cond = Arg(0);
            const std::uint32_t mutex_address = Arg(1);
            const std::uint64_t wait_key = make_wait_key(cond);
            auto wait = guest_cond_waits_.find(wait_key);
            if (wait == guest_cond_waits_.end()) {
                GuestCondWaitState state{};
                state.broadcast_generation = guest_conds_[cond].broadcast_generation;
                state.mutex = mutex_address;
                state.timed = name != "pthread_cond_wait";
                if (state.timed && !decode_deadline(Arg(2), state.deadline)) {
                    Ret(22u); // EINVAL
                    return true;
                }
                auto mutex = guest_mutexes_.find(mutex_address);
                if (mutex != guest_mutexes_.end() && mutex->second.owner == current_thread_id_) {
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                    RecordSyncEvent("cond-release", mutex_address, current_thread_id_);
#endif
                    mutex->second.owner = 0;
                    mutex->second.recursion = 0;
                }
                wait = guest_cond_waits_.emplace(wait_key, state).first;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                const auto mutex_now = guest_mutexes_.find(mutex_address);
                const std::uint32_t owner_now = mutex_now == guest_mutexes_.end() ? 0u : mutex_now->second.owner;
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7COND wait-enter thread=%u cond=0x%08x mutex=0x%08x owner_after_release=%u lr=0x%08x",
                                    current_thread_id_, cond, mutex_address, owner_now,
                                    jit ? jit->Regs()[14] : 0u);
#endif
            }

            auto& cond_state = guest_conds_[cond];
            const bool broadcasted = cond_state.broadcast_generation != wait->second.broadcast_generation;
            const bool signaled = wait->second.signaled;
            const bool timed_out = wait->second.timed &&
                                   std::chrono::system_clock::now() >= wait->second.deadline;
            if (!broadcasted && !signaled && !timed_out) {
                if (!block_current_sync_call(name)) return false;
                return true;
            }
            if (timed_out && !broadcasted && !signaled) {
                guest_cond_waits_.erase(wait);
                Ret(110u); // ETIMEDOUT
                return true;
            }

            auto& mutex = guest_mutexes_[mutex_address];
            if (mutex.owner != 0 && mutex.owner != current_thread_id_) {
                if (!block_current_sync_call(name)) return false;
                return true;
            }
            mutex.owner = current_thread_id_;
            mutex.recursion = 1;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            RecordSyncEvent("cond-reacquire", mutex_address, current_thread_id_);
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7COND wait-wake thread=%u cond=0x%08x mutex=0x%08x reason=%s",
                                current_thread_id_, cond, mutex_address,
                                broadcasted ? "broadcast" : (signaled ? "signal" : "timeout"));
#endif
            guest_cond_waits_.erase(wait);
            Ret(0);
            return true;
        }
        if (name == "sem_init") {
            guest_semaphores_[Arg(0)] = static_cast<std::int64_t>(Arg(2));
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SYNC sem_init thread=%u sem=0x%08x value=%u",
                                current_thread_id_, Arg(0), Arg(2));
#endif
            Ret(0);
            return true;
        }
        if (name == "sem_destroy") {
            guest_semaphores_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (name == "sem_getvalue") {
            const auto found = guest_semaphores_.find(Arg(0));
            const std::uint32_t value = found == guest_semaphores_.end()
                                            ? 0u
                                            : static_cast<std::uint32_t>(std::max<std::int64_t>(0, found->second));
            if (Arg(1) != 0 && !Write32(memory_, Arg(1), value)) return false;
            Ret(0);
            return true;
        }
        if (name == "sem_post") {
            auto& value = guest_semaphores_[Arg(0)];
            ++value;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SYNC sem_post thread=%u sem=0x%08x value=%lld",
                                current_thread_id_, Arg(0), static_cast<long long>(value));
#endif
            Ret(0);
            return true;
        }
        if (name == "sem_trywait") {
            auto& value = guest_semaphores_[Arg(0)];
            if (value > 0) {
                --value;
                Ret(0);
            } else {
                Write32(memory_, kBootstrapRuntimeBase, 11u); // EAGAIN
                Ret(0xffffffffu);
            }
            return true;
        }
        if (name == "sem_wait" || name == "sem_timedwait") {
            auto& value = guest_semaphores_[Arg(0)];
            const std::uint64_t wait_key = make_wait_key(Arg(0));
            if (value > 0) {
                --value;
                guest_sem_deadlines_.erase(wait_key);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                guest_sem_wait_logged_.erase(wait_key);
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SYNC %s acquired thread=%u sem=0x%08x remaining=%lld",
                                    name.c_str(), current_thread_id_, Arg(0), static_cast<long long>(value));
#endif
                Ret(0);
                return true;
            }
            if (name == "sem_timedwait") {
                auto deadline = guest_sem_deadlines_.find(wait_key);
                if (deadline == guest_sem_deadlines_.end()) {
                    std::chrono::system_clock::time_point parsed{};
                    if (!decode_deadline(Arg(1), parsed)) {
                        Write32(memory_, kBootstrapRuntimeBase, 22u); // EINVAL
                        Ret(0xffffffffu);
                        return true;
                    }
                    deadline = guest_sem_deadlines_.emplace(wait_key, parsed).first;
                }
                if (std::chrono::system_clock::now() >= deadline->second) {
                    guest_sem_deadlines_.erase(deadline);
                    Write32(memory_, kBootstrapRuntimeBase, 110u); // ETIMEDOUT
                    Ret(0xffffffffu);
                    return true;
                }
            }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (!guest_sem_wait_logged_[wait_key]) {
                guest_sem_wait_logged_[wait_key] = true;
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SYNC %s blocked thread=%u sem=0x%08x value=%lld lr=0x%08x",
                                    name.c_str(), current_thread_id_, Arg(0), static_cast<long long>(value),
                                    jit ? jit->Regs()[14] : 0u);
            }
#endif
            if (!block_current_sync_call(name)) return false;
            return true;
        }
        if (name == "pthread_kill") {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL source=pthread_kill sender=%u target=%u signal=%u lr=0x%08x sp=0x%08x",
                                current_thread_id_, Arg(0), Arg(1), jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
#endif
            Ret(QueueGuestSignal(Arg(0), Arg(1)) ? 0u : 3u); // ESRCH
            return true;
        }
        if (name == "pthread_join") {
            const std::uint32_t target = Arg(0);
            const auto thread = std::find_if(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                                             [&](const GuestThreadLaunch& item) { return item.id == target; });
            if (thread == guest_thread_launches_.end()) { Ret(3u); return true; } // ESRCH
            if (!thread->finished) {
                if (!block_current_sync_call(name)) return false;
                return true;
            }
            if (Arg(1) != 0) Write32(memory_, Arg(1), thread->regs[0]);
            Ret(0);
            return true;
        }
        if (name == "pthread_attr_init") {
            guest_pthread_attr_threads_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (name == "pthread_attr_destroy") {
            guest_pthread_attr_threads_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (name == "pthread_attr_setstacksize" || name == "pthread_attr_setdetachstate" ||
            name == "pthread_attr_setschedparam" || name == "pthread_attr_setschedpolicy" ||
            name == "pthread_setschedparam" || name == "pthread_detach" || name == "pthread_sigmask") {
            Ret(0);
            return true;
        }
        if (name == "pthread_self") { Ret(current_thread_id_); return true; }
        if (name == "pthread_equal") { Ret(Arg(0) == Arg(1) ? 1u : 0u); return true; }
        if (name == "pthread_key_create") {
            const std::uint32_t key = next_tls_key_++;
            if (Arg(0)) Write32(memory_, Arg(0), key);
            Ret(0);
            return true;
        }
        if (name == "pthread_key_delete") { tls_values_.erase(Arg(0)); Ret(0); return true; }
        if (name == "pthread_setspecific") { tls_values_[Arg(0)] = Arg(1); Ret(0); return true; }
        if (name == "pthread_getspecific") { Ret(tls_values_[Arg(0)]); return true; }
        if (name == "pthread_getattr_np") {
            const std::uint32_t thread_id = Arg(0);
            const std::uint32_t attr = Arg(1);
            const bool known_thread = thread_id == 1u ||
                std::any_of(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                            [&](const GuestThreadLaunch& item) { return item.id == thread_id; });
            if (!known_thread) { Ret(3u); return true; } // ESRCH
            if (attr) guest_pthread_attr_threads_[attr] = thread_id;
            Ret(0);
            return true;
        }
        if (name == "setjmp" || name == "sigsetjmp") {
            const std::uint32_t buffer = Arg(0);
            if (!jit || buffer == 0) {
                Ret(0);
                return true;
            }
            GuestJumpContext context{};
            context.regs = jit->Regs();
            context.cpsr = jit->Cpsr();
            context.resume_pc = jit->Regs()[14];
            context.thread_id = current_thread_id_;
            jump_contexts_[buffer] = context;
            Ret(0);
            return true;
        }
        if (name == "longjmp" || name == "siglongjmp") {
            const std::uint32_t buffer = Arg(0);
            const auto saved = jump_contexts_.find(buffer);
            if (!jit || saved == jump_contexts_.end()) {
                return false;
            }
            const std::uint32_t value = Arg(1) == 0 ? 1u : Arg(1);
            const GuestJumpContext context = saved->second;
            jit->Regs() = context.regs;
            jit->Regs()[0] = value;
            jit->Regs()[15] = context.resume_pc & ~1u;
            jit->SetCpsr((context.cpsr & ~0x20u) | ((context.resume_pc & 1u) ? 0x20u : 0u));
            pending_guest_callback = true;
            jit->HaltExecution();
            return true;
        }
        if (name == "pthread_attr_getdetachstate") {
            if (Arg(1)) Write32(memory_, Arg(1), 0);
            Ret(0); return true;
        }
        if (name == "pthread_attr_getstack") {
            std::uint32_t thread_id = current_thread_id_;
            if (const auto it = guest_pthread_attr_threads_.find(Arg(0)); it != guest_pthread_attr_threads_.end())
                thread_id = it->second;

            std::uint32_t stack_base = 0x06f00000u;
            std::uint32_t stack_size = 0x00100000u;
            if (thread_id >= 2u) {
                const std::uint64_t stack_offset =
                    static_cast<std::uint64_t>(thread_id - 2u) * kGuestThreadStackStride;
                if (stack_offset > static_cast<std::uint64_t>(kGuestThreadStackTop - kGuestThreadStackFloor)) {
                    Ret(22u); // EINVAL
                    return true;
                }
                const std::uint32_t stack_top =
                    kGuestThreadStackTop - static_cast<std::uint32_t>(stack_offset);
                stack_base = stack_top - kGuestThreadStackStride;
                stack_size = kGuestThreadStackStride;
            }
            if (Arg(1)) Write32(memory_, Arg(1), stack_base);
            if (Arg(2)) Write32(memory_, Arg(2), stack_size);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7STACK pthread_attr_getstack caller=%u target=%u attr=0x%08x base=0x%08x size=0x%08x sp=0x%08x",
                                current_thread_id_, thread_id, Arg(0), stack_base, stack_size,
                                jit ? jit->Regs()[13] : 0u);
#endif
            Ret(0);
            return true;
        }
        if (name == "__pthread_cleanup_push" || name == "__pthread_cleanup_pop") { Ret(0); return true; }
        if (name == "dlopen") { Ret(1); return true; }
        if (name == "dlclose") { Ret(0); return true; }
        if (name == "dlerror") { Ret(0); return true; }
        if (name == "dlsym") {
            const std::string symbol = ReadCString(Arg(1));
            const auto guest = exports_.find(symbol);
            if (guest != exports_.end()) Ret(guest->second.address);
            else {
                const auto thunk = name_to_stub_.find(symbol);
                Ret(thunk == name_to_stub_.end() ? 0u : thunk->second);
            }
            return true;
        }
        if (name == "sprintf" || name == "snprintf" || name == "vsprintf" || name == "vsnprintf" || name == "vasprintf") {
            const bool allocate_result = name == "vasprintf";
            const bool bounded = name == "snprintf" || name == "vsnprintf";
            const bool va_list_mode = name == "vsprintf" || name == "vsnprintf" || allocate_result;
            const std::uint32_t dst = Arg(0);
            const std::size_t capacity = bounded ? static_cast<std::size_t>(Arg(1)) : SIZE_MAX;
            const std::uint32_t fmt_addr = allocate_result ? Arg(1) : Arg(bounded ? 2 : 1);
            std::uint32_t va_ptr = va_list_mode ? (allocate_result ? Arg(2) : Arg(bounded ? 3 : 2)) : 0;
            unsigned word_index = bounded ? 3u : 2u;
            const std::string fmt = ReadCString(fmt_addr);
            auto next_word = [&]() -> std::uint32_t {
                if (!va_list_mode) return Arg(word_index++);
                std::uint32_t value = 0;
                if (!Read32(memory_, va_ptr, value)) return 0;
                va_ptr += 4;
                return value;
            };
            auto next_u64 = [&]() -> std::uint64_t {
                if (!va_list_mode && (word_index & 1u)) ++word_index;
                if (va_list_mode && (va_ptr & 7u)) va_ptr = AlignUp(va_ptr, 8);
                const std::uint64_t lo = next_word();
                const std::uint64_t hi = next_word();
                return lo | (hi << 32);
            };
            std::string out;
            for (std::size_t i = 0; i < fmt.size(); ++i) {
                if (fmt[i] != '%') { out.push_back(fmt[i]); continue; }
                if (++i >= fmt.size()) break;
                if (fmt[i] == '%') { out.push_back('%'); continue; }
                bool zero_pad = false;
                while (i < fmt.size() && (fmt[i] == '-' || fmt[i] == '+' || fmt[i] == ' ' || fmt[i] == '#' || fmt[i] == '0')) {
                    if (fmt[i] == '0') zero_pad = true;
                    ++i;
                }
                unsigned width = 0;
                while (i < fmt.size() && std::isdigit(static_cast<unsigned char>(fmt[i]))) width = width * 10u + static_cast<unsigned>(fmt[i++] - '0');
                int precision = -1;
                if (i < fmt.size() && fmt[i] == '.') {
                    ++i; precision = 0;
                    while (i < fmt.size() && std::isdigit(static_cast<unsigned char>(fmt[i]))) precision = precision * 10 + (fmt[i++] - '0');
                }
                bool wide = false;
                if (i < fmt.size() && (fmt[i] == 'l' || fmt[i] == 'z' || fmt[i] == 'j' || fmt[i] == 't')) {
                    const char length = fmt[i++];
                    wide = length == 'j' || (length == 'l' && i < fmt.size() && fmt[i] == 'l');
                    if (wide && i < fmt.size() && fmt[i] == 'l') ++i;
                } else if (i < fmt.size() && fmt[i] == 'h') {
                    ++i; if (i < fmt.size() && fmt[i] == 'h') ++i;
                }
                if (i >= fmt.size()) break;
                const char conv = fmt[i];
                std::string piece;
                if (conv == 's') {
                    piece = ReadCString(next_word());
                    if (precision >= 0 && piece.size() > static_cast<std::size_t>(precision)) piece.resize(static_cast<std::size_t>(precision));
                } else if (conv == 'c') {
                    piece.push_back(static_cast<char>(next_word() & 0xffu));
                } else if (conv == 'p') {
                    char buf[32]{};
                    std::snprintf(buf, sizeof(buf), "0x%08x", next_word());
                    piece = buf;
                } else if (conv == 'd' || conv == 'i' || conv == 'u' || conv == 'x' || conv == 'X' || conv == 'o') {
                    const std::uint64_t raw = wide ? next_u64() : next_word();
                    char buf[96]{};
                    if (conv == 'd' || conv == 'i') std::snprintf(buf, sizeof(buf), "%lld", static_cast<long long>(wide ? static_cast<std::int64_t>(raw) : static_cast<std::int32_t>(raw)));
                    else if (conv == 'u') std::snprintf(buf, sizeof(buf), "%llu", static_cast<unsigned long long>(wide ? raw : static_cast<std::uint32_t>(raw)));
                    else if (conv == 'x') std::snprintf(buf, sizeof(buf), "%llx", static_cast<unsigned long long>(wide ? raw : static_cast<std::uint32_t>(raw)));
                    else if (conv == 'X') std::snprintf(buf, sizeof(buf), "%llX", static_cast<unsigned long long>(wide ? raw : static_cast<std::uint32_t>(raw)));
                    else std::snprintf(buf, sizeof(buf), "%llo", static_cast<unsigned long long>(wide ? raw : static_cast<std::uint32_t>(raw)));
                    piece = buf;
                } else {
                    piece.push_back('%'); piece.push_back(conv);
                }
                if (width > piece.size()) out.append(width - piece.size(), zero_pad ? '0' : ' ');
                out += piece;
            }
            const std::size_t required = out.size();
            if (allocate_result) {
                const std::uint32_t allocated = Allocate(required + 1);
                if (!allocated || !dst || !WriteCString(allocated, out) || !Write32(memory_, dst, allocated)) {
                    Ret(0xffffffffu);
                    return true;
                }
            } else if (dst && (!bounded || capacity != 0)) {
                const std::size_t cap = bounded ? capacity : required + 1;
                WriteCString(dst, out, cap);
            }
            Ret(static_cast<std::uint32_t>(required));
            return true;
        }
#if defined(__ANDROID__)
        if (name == "eglGetDisplay") {
            const EGLDisplay display = eglGetDisplay(EGL_DEFAULT_DISPLAY);
            Ret(RegisterEglHandle(reinterpret_cast<std::uintptr_t>(display)));
            return true;
        }
        if (name == "eglInitialize") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            EGLint major = 0;
            EGLint minor = 0;
            const EGLBoolean ok = display != EGL_NO_DISPLAY
                ? eglInitialize(display, &major, &minor)
                : EGL_FALSE;
            if (ok == EGL_TRUE) {
                if (Arg(1)) Write32(memory_, Arg(1), static_cast<std::uint32_t>(major));
                if (Arg(2)) Write32(memory_, Arg(2), static_cast<std::uint32_t>(minor));
            }
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (name == "eglChooseConfig") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const auto attributes = ReadEglAttribList(Arg(1));
            EGLint capacity = static_cast<EGLint>(Arg(3));
            capacity = std::max<EGLint>(0, std::min<EGLint>(capacity, 256));
            std::vector<EGLConfig> configs(static_cast<std::size_t>(capacity));
            EGLint count = 0;
            const EGLBoolean ok = display != EGL_NO_DISPLAY
                ? eglChooseConfig(display,
                                  attributes.empty() ? nullptr : attributes.data(),
                                  capacity > 0 ? configs.data() : nullptr,
                                  capacity,
                                  &count)
                : EGL_FALSE;
            if (Arg(4)) Write32(memory_, Arg(4), static_cast<std::uint32_t>(count));
            if (ok == EGL_TRUE && Arg(2) && capacity > 0) {
                const EGLint written = std::min(count, capacity);
                for (EGLint index = 0; index < written; ++index) {
                    Write32(memory_, Arg(2) + static_cast<std::uint32_t>(index * 4),
                            RegisterEglHandle(reinterpret_cast<std::uintptr_t>(configs[static_cast<std::size_t>(index)])));
                }
            }
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (name == "eglGetConfigAttrib") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLConfig config = reinterpret_cast<EGLConfig>(LookupEglHandle(Arg(1)));
            EGLint value = 0;
            const EGLBoolean ok = display != EGL_NO_DISPLAY && config != nullptr
                ? eglGetConfigAttrib(display, config, static_cast<EGLint>(Arg(2)), &value)
                : EGL_FALSE;
            if (ok == EGL_TRUE && Arg(3)) Write32(memory_, Arg(3), static_cast<std::uint32_t>(value));
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (name == "eglCreateWindowSurface") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLConfig config = reinterpret_cast<EGLConfig>(LookupEglHandle(Arg(1)));
            const auto attributes = ReadEglAttribList(Arg(3));
            ANativeWindow* window = Arg(2) == kGuestNativeWindowHandle
                ? static_cast<ANativeWindow*>(host_native_window_)
                : nullptr;
            const EGLSurface surface = display != EGL_NO_DISPLAY && config != nullptr && window
                ? eglCreateWindowSurface(display, config, window,
                                         attributes.empty() ? nullptr : attributes.data())
                : EGL_NO_SURFACE;
            Ret(RegisterEglHandle(reinterpret_cast<std::uintptr_t>(surface)));
            return true;
        }
        if (name == "eglCreateContext") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLConfig config = reinterpret_cast<EGLConfig>(LookupEglHandle(Arg(1)));
            const EGLContext shared = reinterpret_cast<EGLContext>(LookupEglHandle(Arg(2)));
            const auto attributes = ReadEglAttribList(Arg(3));
            const EGLContext context = display != EGL_NO_DISPLAY && config != nullptr
                ? eglCreateContext(display, config, shared,
                                   attributes.empty() ? nullptr : attributes.data())
                : EGL_NO_CONTEXT;
            Ret(RegisterEglHandle(reinterpret_cast<std::uintptr_t>(context)));
            return true;
        }
        if (name == "eglMakeCurrent") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLSurface draw = reinterpret_cast<EGLSurface>(LookupEglHandle(Arg(1)));
            const EGLSurface read = reinterpret_cast<EGLSurface>(LookupEglHandle(Arg(2)));
            const EGLContext context = reinterpret_cast<EGLContext>(LookupEglHandle(Arg(3)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY
                ? eglMakeCurrent(display, draw, read, context)
                : EGL_FALSE;
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (name == "eglSwapBuffers") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLSurface surface = reinterpret_cast<EGLSurface>(LookupEglHandle(Arg(1)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY && surface != EGL_NO_SURFACE
                ? eglSwapBuffers(display, surface)
                : EGL_FALSE;
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (name == "eglDestroySurface") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLSurface surface = reinterpret_cast<EGLSurface>(LookupEglHandle(Arg(1)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY && surface != EGL_NO_SURFACE
                ? eglDestroySurface(display, surface)
                : EGL_FALSE;
            if (ok == EGL_TRUE) UnregisterEglHandle(Arg(1));
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (name == "eglDestroyContext") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLContext context = reinterpret_cast<EGLContext>(LookupEglHandle(Arg(1)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY && context != EGL_NO_CONTEXT
                ? eglDestroyContext(display, context)
                : EGL_FALSE;
            if (ok == EGL_TRUE) UnregisterEglHandle(Arg(1));
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (name == "eglTerminate") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY ? eglTerminate(display) : EGL_FALSE;
            if (ok == EGL_TRUE) UnregisterEglHandle(Arg(0));
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (name == "eglGetError") {
            Ret(static_cast<std::uint32_t>(eglGetError()));
            return true;
        }
        if (name == "eglGetProcAddress") {
            // Never expose an arm64 host function pointer to the ARM32 guest. Return a
            // guest SVC thunk only for extension entry points implemented by this bridge.
            const std::string symbol = ReadCString(Arg(0), 256);
            const auto found = name_to_stub_.find(symbol);
            Ret(found == name_to_stub_.end() ? 0u : found->second);
            return true;
        }
#endif
        if (name == "ANativeWindow_fromSurface") {
            Ret(host_native_window_ ? kGuestNativeWindowHandle : 0u);
            if (file_trace.size() < 64) {
                file_trace.push_back(host_native_window_
                    ? "ANativeWindow_fromSurface -> real host window"
                    : "ANativeWindow_fromSurface -> null (no host Surface)");
            }
            return true;
        }
        if (name == "ANativeWindow_setBuffersGeometry") {
            int result = -1;
#if defined(__ANDROID__)
            if (host_native_window_ && Arg(0) == kGuestNativeWindowHandle) {
                result = ANativeWindow_setBuffersGeometry(
                    static_cast<ANativeWindow*>(host_native_window_),
                    static_cast<int32_t>(Arg(1)),
                    static_cast<int32_t>(Arg(2)),
                    static_cast<int32_t>(Arg(3)));
            }
#endif
            Ret(static_cast<std::uint32_t>(result));
            return true;
        }
        if (name == "__android_log_print" || name == "__android_log_vprint" ||
            name == "printf" || name == "vprintf" || name == "fprintf" || name == "vfprintf" ||
            name == "puts" || name == "putchar" || name == "fputs" || name == "fputc" ||
            name == "fflush" || name == "perror") {
            std::uint32_t text_arg = 0;
            if (name == "printf" || name == "vprintf" || name == "puts" || name == "perror") text_arg = Arg(0);
            else if (name == "fprintf" || name == "vfprintf" || name == "fputs") text_arg = Arg(1);
            else if (name == "__android_log_print" || name == "__android_log_vprint") text_arg = Arg(2);
            if (text_arg && diagnostic_formats.size() < 16) {
                const std::string text = ReadCString(text_arg);
                if (!text.empty()) diagnostic_formats.push_back(name + ": " + text);
            }
            Ret(0);
            return true;
        }
        if (name == "__cxa_guard_acquire") {
            const std::uint32_t guard = Arg(0);
            if (!guard || !Fits(guard, 1)) { Ret(0); return true; }
            if (Read<std::uint8_t>(guard) != 0) { Ret(0); return true; }
            if (cxa_guards_in_progress_[guard]) { Ret(0); return true; }
            cxa_guards_in_progress_[guard] = true;
            Ret(1);
            return true;
        }
        if (name == "__cxa_guard_release") {
            const std::uint32_t guard = Arg(0);
            if (guard && Fits(guard, 1)) Write(guard, static_cast<std::uint8_t>(1));
            cxa_guards_in_progress_.erase(guard);
            Ret(0);
            return true;
        }
        if (name == "__cxa_guard_abort") {
            const std::uint32_t guard = Arg(0);
            if (guard && Fits(guard, 1)) Write(guard, static_cast<std::uint8_t>(0));
            cxa_guards_in_progress_.erase(guard);
            Ret(0);
            return true;
        }
        if (name == "__cxa_atexit" || name == "__aeabi_atexit" || name == "__cxa_finalize") { Ret(0); return true; }
        if (name == "__gnu_Unwind_Find_exidx") {
            const std::uint32_t pc = Arg(0);
            const std::uint32_t count_out = Arg(1);
            std::uint32_t table = 0;
            std::uint32_t count = 0;
            if (pc >= kMainBase && pc < 0x00107578u) {
                table = kMainBase + 0x00005bd0u;
                count = 0x000001c0u / 8u;
            } else if (pc >= kUnityBase && pc < 0x01bf9688u) {
                table = kUnityBase + 0x0093eb88u;
                count = 0x00026fa0u / 8u;
            } else if (pc >= kMonoBase && pc < 0x033d10a4u) {
                table = kMonoBase + 0x002e6648u;
                count = 0x0000b4f0u / 8u;
            }
            if (count_out >= 0x1000u && Fits(count_out, 4u)) {
                Write32(memory_, count_out, count);
            }
#if defined(__ANDROID__)
            else if (count_out != 0) {
                const std::uint32_t sp = jit ? jit->Regs()[13] : 0u;
                const std::uint32_t stack_return = sp >= 4u ? Read<std::uint32_t>(sp - 4u) : 0u;
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "UNWIND exidx rejected invalid count ptr=0x%08x pc=0x%08x code=[0x%08x 0x%08x 0x%08x 0x%08x] image=0x%08x lr=0x%08x sp[-4]=0x%08x",
                                    count_out, pc, code_read_prev3, code_read_prev2,
                                    code_read_prev1, code_read_last, code_read_last_image,
                                    jit ? jit->Regs()[14] : 0u, stack_return);
            }
#endif
            if (count_out != 0 && (count_out < 0x1000u || !Fits(count_out, 4u))) table = 0;
            Ret(table);
            return true;
        }
        if (name == "sigaction") {
            const std::uint32_t signal = Arg(0);
            const std::uint32_t action_ptr = Arg(1);
            const std::uint32_t old_action_ptr = Arg(2);
            const auto existing = guest_signal_actions_.find(signal);
            const GuestSignalAction old_action = existing == guest_signal_actions_.end()
                                                     ? GuestSignalAction{}
                                                     : existing->second;
            if (old_action_ptr != 0) {
                if (!Fits(old_action_ptr, 16u)) return false;
                Write32(memory_, old_action_ptr, old_action.handler);
                Write32(memory_, old_action_ptr + 4u, old_action.mask);
                Write32(memory_, old_action_ptr + 8u, old_action.flags);
                Write32(memory_, old_action_ptr + 12u, old_action.restorer);
            }
            if (action_ptr != 0) {
                if (!Fits(action_ptr, 16u)) return false;
                GuestSignalAction action{};
                Read32(memory_, action_ptr, action.handler);
                Read32(memory_, action_ptr + 4u, action.mask);
                Read32(memory_, action_ptr + 8u, action.flags);
                Read32(memory_, action_ptr + 12u, action.restorer);
                guest_signal_actions_[signal] = action;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SIGNAL sigaction signal=%u handler=0x%08x mask=0x%08x flags=0x%08x",
                                    signal, action.handler, action.mask, action.flags);
#endif
            }
            Ret(0);
            return true;
        }
        if (name == "bsd_signal" || name == "signal") {
            const std::uint32_t signal = Arg(0);
            const std::uint32_t handler = Arg(1);
            const auto existing = guest_signal_actions_.find(signal);
            const std::uint32_t old_handler = existing == guest_signal_actions_.end() ? 0u : existing->second.handler;
            guest_signal_actions_[signal].handler = handler;
            Ret(old_handler);
            return true;
        }
        if (name == "tkill") {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL source=tkill sender=%u target=%u signal=%u lr=0x%08x sp=0x%08x",
                                current_thread_id_, Arg(0), Arg(1), jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
#endif
            Ret(QueueGuestSignal(Arg(0), Arg(1)) ? 0u : 0xffffffffu);
            return true;
        }
        if (name == "kill") {
            const std::uint32_t target = (Arg(0) == 0u || Arg(0) == 1234u) ? current_thread_id_ : Arg(0);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL source=kill sender=%u target=%u signal=%u lr=0x%08x sp=0x%08x",
                                current_thread_id_, target, Arg(1), jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
#endif
            Ret(QueueGuestSignal(target, Arg(1)) ? 0u : 0xffffffffu);
            return true;
        }
        if (name == "raise") {
            const std::uint32_t signal = Arg(0);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL source=raise sender=%u target=%u signal=%u lr=0x%08x sp=0x%08x",
                                current_thread_id_, current_thread_id_, signal, jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
            if (jit && signal == 11u) {
                const std::uint32_t sp = jit->Regs()[13];
                std::uint32_t caller_fp = 0u;
                if (Fits(sp, 4u)) Read32(memory_, sp, caller_fp);
                if (caller_fp >= 36u && Fits(caller_fp - 36u, 40u)) {
                    std::uint32_t log_domain = 0u, log_level = 0u, log_format = 0u, log_args = 0u;
                    Read32(memory_, caller_fp - 24u, log_domain);
                    Read32(memory_, caller_fp - 28u, log_level);
                    Read32(memory_, caller_fp - 32u, log_format);
                    Read32(memory_, caller_fp - 36u, log_args);
                    std::uint32_t arg0 = 0u, arg1 = 0u, arg2 = 0u;
                    if (log_args && Fits(log_args, 12u)) {
                        Read32(memory_, log_args, arg0);
                        Read32(memory_, log_args + 4u, arg1);
                        Read32(memory_, log_args + 8u, arg2);
                    }
                    const std::string domain = log_domain && Fits(log_domain, 1u) ? ReadCString(log_domain) : std::string();
                    const std::string format = log_format && Fits(log_format, 1u) ? ReadCString(log_format) : std::string();
                    const std::string arg0_text = arg0 && Fits(arg0, 1u) ? ReadCString(arg0) : std::string();
                    const std::string arg2_text = arg2 && Fits(arg2, 1u) ? ReadCString(arg2) : std::string();
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                        "PV7FATAL fp=0x%08x level=0x%08x format=0x%08x args=0x%08x arg0=0x%08x arg1=%u arg2=0x%08x domain='%s' format='%s' arg0str='%s' arg2str='%s'",
                                        caller_fp, log_level, log_format, log_args, arg0, arg1, arg2,
                                        domain.c_str(), format.c_str(), arg0_text.c_str(), arg2_text.c_str());
                }
            }
#endif
            if (!QueueGuestSignal(current_thread_id_, signal)) {
                Ret(0xffffffffu);
                return true;
            }
            // POSIX raise() targets the calling thread synchronously. Save the
            // successful return value before entering the handler so the signal
            // frame resumes inside the compatibility thunk and then returns 0.
            Ret(0);
            if (DeliverPendingGuestSignal()) {
                pending_guest_callback = true;
                if (jit) jit->HaltExecution();
            }
            return true;
        }
        if (name == "sigsuspend") {
            auto interrupted = guest_signal_interrupted_.find(current_thread_id_);
            if (interrupted != guest_signal_interrupted_.end() && interrupted->second) {
                interrupted->second = false;
                Write32(memory_, kBootstrapRuntimeBase, 4u); // EINTR
                Ret(0xffffffffu);
                return true;
            }
            if (!block_current_sync_call(name)) return false;
            return true;
        }
        if (name == "sigprocmask" || name == "pthread_sigmask" ||
            name == "setitimer" || name == "prctl") {
            Ret(0); return true;
        }
        if (name == "fopen") {
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            const std::string mode = ReadCString(Arg(1));
            if (file_trace.size() < 24) file_trace.push_back("fopen " + path + " mode=" + mode);
            errno = 0;
            if (path == "/proc/cpuinfo" && mode.find('r') != std::string::npos) {
                static constexpr char kArmV7CpuInfo[] =
                    "Processor\t: ARMv7 Processor rev 0 (v7l)\n"
                    "processor\t: 0\n"
                    "BogoMIPS\t: 38.40\n"
                    "Features\t: swp half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt\n"
                    "CPU implementer\t: 0x51\n"
                    "CPU architecture: 7\n"
                    "CPU variant\t: 0x2\n"
                    "CPU part\t: 0x06f\n"
                    "CPU revision\t: 0\n"
                    "Hardware\t: CNR64 Virtual ARMv7\n";
                const std::uint32_t handle = RegisterVirtualFile(kArmV7CpuInfo);
                SetErrno(0);
                if (file_trace.size() < 24) file_trace.push_back("  -> virtual ARMv7 cpuinfo handle");
                Ret(handle);
                return true;
            }
            std::FILE* file = std::fopen(path.c_str(), mode.c_str());
            SetErrno(file ? 0u : static_cast<std::uint32_t>(errno));
            const std::uint32_t handle = RegisterFileHandle(file, path);
            if (path.find("mscorlib.dll") != std::string::npos && file_trace.size() < 96) {
                std::ostringstream trace;
                trace << "  -> fopen handle=0x" << std::hex << handle << std::dec << " errno=" << errno;
                file_trace.push_back(trace.str());
            }
            Ret(handle);
            return true;
        }
        if (name == "fclose") {
            const std::uint32_t handle = Arg(0);
            const auto virtual_it = virtual_files_.find(handle);
            if (virtual_it != virtual_files_.end()) {
                virtual_files_.erase(virtual_it);
                Ret(0);
                return true;
            }
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { SetErrno(9); Ret(0xffffffffu); return true; }
            const int rc = std::fclose(file);
            file_handles_.erase(handle);
            file_paths_.erase(handle);
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "fread" || name == "fwrite") {
            const std::uint32_t guest = Arg(0);
            const std::size_t size = Arg(1);
            const std::size_t count = Arg(2);
            const std::uint32_t handle = Arg(3);
            const std::uint64_t bytes = static_cast<std::uint64_t>(size) * count;
            if (bytes > memory_.size() || !Fits(guest, static_cast<std::size_t>(bytes))) {
                SetErrno(14); Ret(0); return true;
            }
            auto virtual_it = virtual_files_.find(handle);
            if (virtual_it != virtual_files_.end()) {
                if (name != "fread" || size == 0) { Ret(0); return true; }
                VirtualFile& vf = virtual_it->second;
                const std::size_t available = vf.data.size() - std::min(vf.pos, vf.data.size());
                const std::size_t copied = std::min<std::size_t>(available, static_cast<std::size_t>(bytes));
                if (copied) std::memcpy(memory_.data() + guest, vf.data.data() + vf.pos, copied);
                vf.pos += copied;
                Ret(static_cast<std::uint32_t>(copied / size));
                return true;
            }
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { SetErrno(9); Ret(0); return true; }
            const long before = std::ftell(file);
            const std::size_t done = name == "fread"
                ? std::fread(memory_.data() + guest, size, count, file)
                : std::fwrite(memory_.data() + guest, size, count, file);
            const auto path_it = file_paths_.find(handle);
            if (name == "fread" && path_it != file_paths_.end() &&
                path_it->second.find("mscorlib.dll") != std::string::npos && file_trace.size() < 96) {
                std::ostringstream trace;
                trace << "fread mscorlib pos=" << before << " request=" << bytes << " result=" << done * size << " head=";
                const std::size_t head = std::min<std::size_t>(done * size, 8);
                for (std::size_t i = 0; i < head; ++i) {
                    char hex[4]{};
                    std::snprintf(hex, sizeof(hex), "%02x", static_cast<unsigned>(memory_[guest + i]));
                    trace << hex;
                }
                file_trace.push_back(trace.str());
            }
            Ret(static_cast<std::uint32_t>(done));
            return true;
        }
        if (name == "fseek") {
            const std::uint32_t handle = Arg(0);
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { SetErrno(9); Ret(0xffffffffu); return true; }
            const auto path_it = file_paths_.find(handle);
            const long before = std::ftell(file);
            const int rc = std::fseek(file, static_cast<long>(static_cast<std::int32_t>(Arg(1))), static_cast<int>(Arg(2)));
            if (path_it != file_paths_.end() && path_it->second.find("mscorlib.dll") != std::string::npos && file_trace.size() < 96) {
                std::ostringstream trace;
                trace << "fseek mscorlib from=" << before << " offset=" << static_cast<std::int32_t>(Arg(1))
                      << " whence=" << Arg(2) << " rc=" << rc << " to=" << std::ftell(file);
                file_trace.push_back(trace.str());
            }
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "ftell") {
            std::FILE* file = LookupFileHandle(Arg(0));
            if (!file) { SetErrno(9); Ret(0xffffffffu); return true; }
            Ret(static_cast<std::uint32_t>(std::ftell(file)));
            return true;
        }
        if (name == "fgets") {
            const std::uint32_t dst = Arg(0);
            const int size = static_cast<int>(Arg(1));
            const std::uint32_t handle = Arg(2);
            if (size <= 0 || !Fits(dst, static_cast<std::size_t>(size))) { Ret(0); return true; }
            auto virtual_it = virtual_files_.find(handle);
            if (virtual_it != virtual_files_.end()) {
                VirtualFile& vf = virtual_it->second;
                if (vf.pos >= vf.data.size()) { Ret(0); return true; }
                std::size_t count = 0;
                const std::size_t limit = static_cast<std::size_t>(size - 1);
                while (count < limit && vf.pos < vf.data.size()) {
                    const char c = vf.data[vf.pos++];
                    memory_[dst + count++] = static_cast<std::uint8_t>(c);
                    if (c == '\n') break;
                }
                memory_[dst + count] = 0;
                Ret(dst);
                return true;
            }
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { Ret(0); return true; }
            Ret(std::fgets(reinterpret_cast<char*>(memory_.data() + dst), size, file) ? dst : 0u);
            return true;
        }
        if (name == "fflush") {
            std::FILE* file = LookupFileHandle(Arg(0));
            Ret(static_cast<std::uint32_t>(file ? std::fflush(file) : 0));
            return true;
        }
        if (name == "ferror" || name == "clearerr") {
            std::FILE* file = LookupFileHandle(Arg(0));
            if (!file) { Ret(name == "ferror" ? 1u : 0u); return true; }
            if (name == "clearerr") { std::clearerr(file); Ret(0); }
            else Ret(static_cast<std::uint32_t>(std::ferror(file)));
            return true;
        }
        if (name == "getc" || name == "ungetc") {
            std::FILE* file = LookupFileHandle(name == "getc" ? Arg(0) : Arg(1));
            if (!file) { Ret(0xffffffffu); return true; }
            const int rc = name == "getc" ? std::getc(file) : std::ungetc(static_cast<int>(Arg(0)), file);
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (name == "socket") {
            const std::uint32_t handle = next_socket_handle_++;
            socket_handles_[handle] = true;
            if (file_trace.size() < 32) {
                std::ostringstream line;
                line << "socket domain=" << static_cast<int>(Arg(0))
                     << " type=" << static_cast<int>(Arg(1))
                     << " protocol=" << static_cast<int>(Arg(2))
                     << " handle=0x" << std::hex << handle;
                file_trace.push_back(line.str());
            }
            SetErrno(0);
            Ret(handle);
            return true;
        }
        if (name == "bind" || name == "listen" || name == "setsockopt" || name == "shutdown") {
            Ret(0);
            return true;
        }
        if (name == "connect") {
            SetErrno(111); // ECONNREFUSED: offline adapter
            Ret(0xffffffffu);
            return true;
        }
        if (name == "accept" || name == "recv" || name == "recvfrom" || name == "recvmsg") {
            SetErrno(11); // EAGAIN
            Ret(0xffffffffu);
            return true;
        }
        if (name == "send" || name == "sendto") {
            Ret(Arg(2));
            return true;
        }
        if (name == "sendmsg") {
            Ret(0);
            return true;
        }
        if (name == "getsockname" || name == "getpeername") {
            const std::uint32_t address = Arg(1);
            const std::uint32_t length_ptr = Arg(2);
            std::uint32_t length = 16;
            if (length_ptr != 0) Read32(memory_, length_ptr, length);
            length = std::min<std::uint32_t>(length, 16u);
            if (address != 0) {
                if (!Fits(address, length)) return false;
                std::memset(memory_.data() + address, 0, length);
                if (length >= 2) {
                    memory_[address] = 2; // AF_INET in bionic sockaddr.sa_family
                    memory_[address + 1] = 0;
                }
            }
            if (length_ptr != 0 && !Write32(memory_, length_ptr, 16u)) return false;
            Ret(0);
            return true;
        }
        if (name == "getsockopt") {
            std::uint32_t length = 0;
            if (Arg(4) != 0 && !Read32(memory_, Arg(4), length)) return false;
            if (Arg(3) != 0 && length != 0) {
                if (!Fits(Arg(3), length)) return false;
                std::memset(memory_.data() + Arg(3), 0, length);
            }
            Ret(0);
            return true;
        }
        if (name == "gethostname") {
            const std::string hostname = "localhost";
            if (!WriteCString(Arg(0), hostname, Arg(1))) {
                SetErrno(14);
                Ret(0xffffffffu);
            } else {
                Ret(0);
            }
            return true;
        }
        if (name == "getaddrinfo") {
            if (Arg(3) != 0) Write32(memory_, Arg(3), 0u);
            Ret(static_cast<std::uint32_t>(-2)); // EAI_NONAME
            return true;
        }
        if (name == "freeaddrinfo") { Ret(0); return true; }
        if (name == "gai_strerror") {
            static const std::string message = "Name or service not known";
            const std::uint32_t guest = Allocate(message.size() + 1, 1);
            if (guest != 0) WriteCString(guest, message);
            Ret(guest);
            return true;
        }
        if (name == "gethostbyname" || name == "gethostbyaddr") { Ret(0); return true; }
#if !defined(_WIN32)
        if (name == "inet_pton") {
            const std::string source = ReadCString(Arg(1));
            const std::size_t destination_size = Arg(0) == AF_INET6 ? 16u : 4u;
            void* destination = Arg(2) != 0 && Fits(Arg(2), destination_size)
                ? static_cast<void*>(memory_.data() + Arg(2)) : nullptr;
            if (!destination) { Ret(0); return true; }
            Ret(static_cast<std::uint32_t>(::inet_pton(static_cast<int>(Arg(0)), source.c_str(), destination)));
            return true;
        }
        if (name == "inet_ntop") {
            const std::size_t source_size = Arg(0) == AF_INET6 ? 16u : 4u;
            const void* source = Arg(1) != 0 && Fits(Arg(1), source_size)
                ? static_cast<const void*>(memory_.data() + Arg(1)) : nullptr;
            char* destination = Arg(2) != 0 && Fits(Arg(2), Arg(3))
                ? reinterpret_cast<char*>(memory_.data() + Arg(2)) : nullptr;
            if (!source || !destination) { Ret(0); return true; }
            Ret(::inet_ntop(static_cast<int>(Arg(0)), source, destination, static_cast<socklen_t>(Arg(3))) ? Arg(2) : 0u);
            return true;
        }
        if (name == "inet_addr") {
            const std::string source = ReadCString(Arg(0));
            Ret(static_cast<std::uint32_t>(::inet_addr(source.c_str())));
            return true;
        }
        if (name == "inet_aton") {
            const std::string source = ReadCString(Arg(0));
            in_addr* destination = Arg(1) != 0 && Fits(Arg(1), sizeof(in_addr))
                ? reinterpret_cast<in_addr*>(memory_.data() + Arg(1)) : nullptr;
            Ret(destination ? static_cast<std::uint32_t>(::inet_aton(source.c_str(), destination)) : 0u);
            return true;
        }
        if (name == "inet_ntoa") {
            in_addr address{};
            const std::uint32_t raw = Arg(0);
            std::memcpy(&address, &raw, sizeof(raw));
            const std::string value = ::inet_ntoa(address);
            const std::uint32_t guest = Allocate(value.size() + 1, 1);
            if (guest != 0) WriteCString(guest, value);
            Ret(guest);
            return true;
        }
#endif
        if (name == "ioctl" || name == "fcntl") {
            if (socket_handles_.find(Arg(0)) != socket_handles_.end()) {
                SetErrno(0);
                Ret(0);
            } else {
                SetErrno(25); // ENOTTY
                Ret(0xffffffffu);
            }
            return true;
        }
        if (name == "poll") {
            const std::uint32_t descriptors = Arg(0);
            const std::uint32_t count = Arg(1);
            if (count > 4096u || (count != 0 && !Fits(descriptors, static_cast<std::size_t>(count) * 8u))) return false;
            for (std::uint32_t index = 0; index < count; ++index) {
                const std::uint32_t revents = descriptors + index * 8u + 6u;
                memory_[revents] = 0;
                memory_[revents + 1u] = 0;
            }
            Ret(0);
            return true;
        }
        if (name == "select") { Ret(0); return true; }
        if (name == "epoll_create") {
            const std::uint32_t handle = next_socket_handle_++;
            socket_handles_[handle] = true;
            Ret(handle);
            return true;
        }
        if (name == "epoll_ctl" || name == "epoll_wait") { Ret(0); return true; }
        if (name == "pipe") {
            if (Arg(0) == 0 || !Fits(Arg(0), 8u)) return false;
            const std::uint32_t read_handle = next_socket_handle_++;
            const std::uint32_t write_handle = next_socket_handle_++;
            socket_handles_[read_handle] = true;
            socket_handles_[write_handle] = true;
            Write32(memory_, Arg(0), read_handle);
            Write32(memory_, Arg(0) + 4u, write_handle);
            Ret(0);
            return true;
        }
        if (name == "open") {
#if !defined(_WIN32)
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            if (file_trace.size() < 24) file_trace.push_back("open " + path);
            errno = 0;
            const int fd = ::open(path.c_str(), static_cast<int>(Arg(1)), static_cast<mode_t>(Arg(2)));
            SetErrno(fd >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(fd));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (name == "close") {
            const auto fake_socket = socket_handles_.find(Arg(0));
            if (fake_socket != socket_handles_.end()) {
                socket_handles_.erase(fake_socket);
                SetErrno(0);
                Ret(0);
                return true;
            }
#if !defined(_WIN32)
            const int rc = ::close(static_cast<int>(Arg(0)));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (name == "read" || name == "write") {
#if !defined(_WIN32)
            const int fd = static_cast<int>(Arg(0));
            const std::uint32_t guest = Arg(1);
            const std::size_t count = Arg(2);
            if (!Fits(guest, count)) { SetErrno(14); Ret(0xffffffffu); return true; }
            const ssize_t rc = name == "read"
                ? ::read(fd, memory_.data() + guest, count)
                : ::write(fd, memory_.data() + guest, count);
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (name == "lseek") {
#if !defined(_WIN32)
            const off_t rc = ::lseek(static_cast<int>(Arg(0)), static_cast<off_t>(static_cast<std::int32_t>(Arg(1))), static_cast<int>(Arg(2)));
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (name == "fstat" || name == "stat" || name == "lstat") {
#if !defined(_WIN32)
            struct stat st{};
            int rc = -1;
            std::uint32_t guest_stat = 0;
            if (name == "fstat") {
                rc = ::fstat(static_cast<int>(Arg(0)), &st);
                guest_stat = Arg(1);
            } else {
                const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
                rc = name == "stat" ? ::stat(path.c_str(), &st) : ::lstat(path.c_str(), &st);
                guest_stat = Arg(1);
            }
            if (rc == 0 && !WriteGuestStat(guest_stat, st)) { SetErrno(14); Ret(0xffffffffu); return true; }
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (name == "readlink") {
#if !defined(_WIN32)
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            const std::uint32_t dst = Arg(1);
            const std::size_t size = Arg(2);
            if (!Fits(dst, size)) { SetErrno(14); Ret(0xffffffffu); return true; }
            const ssize_t rc = ::readlink(path.c_str(), reinterpret_cast<char*>(memory_.data() + dst), size);
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (name == "opendir") {
            SetErrno(2); Ret(0); return true;
        }
        if (name == "closedir") { Ret(0); return true; }
        if (name == "isatty") { Ret(0); return true; }
        if (name == "atoi" || name == "atol") {
            const std::string text = ReadCString(Arg(0));
            Ret(static_cast<std::uint32_t>(std::strtol(text.c_str(), nullptr, 10)));
            return true;
        }
        if (name == "strtol" || name == "strtoul") {
            const std::string text = ReadCString(Arg(0));
            char* end = nullptr;
            const int base = static_cast<int>(Arg(2));
            unsigned long value = name == "strtol"
                ? static_cast<unsigned long>(std::strtol(text.c_str(), &end, base))
                : std::strtoul(text.c_str(), &end, base);
            if (Arg(1)) Write32(memory_, Arg(1), Arg(0) + static_cast<std::uint32_t>(end - text.c_str()));
            Ret(static_cast<std::uint32_t>(value));
            return true;
        }
        if (name == "tolower") { Ret(static_cast<std::uint32_t>(std::tolower(static_cast<unsigned char>(Arg(0))))); return true; }
        if (name == "isalnum") { Ret(std::isalnum(static_cast<unsigned char>(Arg(0))) ? 1u : 0u); return true; }
        if (name == "isalpha") { Ret(std::isalpha(static_cast<unsigned char>(Arg(0))) ? 1u : 0u); return true; }
        if (name == "isspace") { Ret(std::isspace(static_cast<unsigned char>(Arg(0))) ? 1u : 0u); return true; }
        if (name == "isxdigit") { Ret(std::isxdigit(static_cast<unsigned char>(Arg(0))) ? 1u : 0u); return true; }
        if (name == "strerror") {
            static constexpr char kError[] = "CNR64 guest errno";
            WriteBytes(memory_, kBootstrapRuntimeBase + 0x200, kError, sizeof(kError));
            Ret(kBootstrapRuntimeBase + 0x200); return true;
        }
        if (name == "uname") {
            const std::uint32_t p = Arg(0);
            if (!Fits(p, 390)) return false;
            std::memset(memory_.data() + p, 0, 390);
            WriteCString(p, "Linux", 65);
            WriteCString(p + 65, "cnr64", 65);
            WriteCString(p + 130, "6.0", 65);
            WriteCString(p + 260, "aarch64", 65);
            Ret(0); return true;
        }
        if (name == "getrusage") {
            if (Arg(1) && Fits(Arg(1), 72)) std::memset(memory_.data() + Arg(1), 0, 72);
            Ret(0); return true;
        }
        if (name == "bsearch") {
            const std::uint32_t key = Arg(0);
            const std::uint32_t base = Arg(1);
            const std::uint32_t nmemb = Arg(2);
            const std::uint32_t size = Arg(3);
            const std::uint32_t compar = Arg(4);
            constexpr std::uint32_t kMonoClassFieldComparator = kMonoBase + 0x00146b38u;
            if (compar == kMonoClassFieldComparator && Fits(key + 58, 2) && size >= 4) {
                const std::uint16_t key_value = Read<std::uint16_t>(key + 58);
                std::uint32_t lo = 0, hi = nmemb;
                while (lo < hi) {
                    const std::uint32_t mid = lo + (hi - lo) / 2;
                    const std::uint64_t element64 = static_cast<std::uint64_t>(base) + static_cast<std::uint64_t>(mid) * size;
                    if (element64 > UINT32_MAX) break;
                    const std::uint32_t element = static_cast<std::uint32_t>(element64);
                    if (!Fits(element, 4)) break;
                    const std::uint32_t object = Read<std::uint32_t>(element);
                    if (!Fits(object + 58, 2)) break;
                    const std::uint16_t element_value = Read<std::uint16_t>(object + 58);
                    if (key_value == element_value) { Ret(element); return true; }
                    if (key_value < element_value) hi = mid;
                    else lo = mid + 1;
                }
                Ret(0);
                return true;
            }
            constexpr std::uint32_t kMonoMetadataRowComparator = kMonoBase + 0x001c9a80u;
            constexpr std::uint32_t kMonoMetadataRangeComparator = kMonoBase + 0x001c9944u;
            if ((compar == kMonoMetadataRowComparator || compar == kMonoMetadataRangeComparator) && Fits(key, 16)) {
                const std::uint32_t target = Read<std::uint32_t>(key);
                const std::uint32_t column = Read<std::uint32_t>(key + 4);
                const std::uint32_t table = Read<std::uint32_t>(key + 8);
                if (!Fits(table, 12)) { Ret(0); return true; }
                const std::uint32_t table_base = Read<std::uint32_t>(table);
                const std::uint32_t row_info = Read<std::uint32_t>(table + 4);
                const std::uint32_t column_info = Read<std::uint32_t>(table + 8);
                const std::uint32_t row_count = row_info & 0x00ffffffu;
                const std::uint32_t row_size = Read<std::uint8_t>(table + 7);
                const std::uint32_t column_count = column_info >> 24;
                if (!row_size || column >= column_count || !size || nmemb > row_count) { Ret(0); return true; }

                auto decode_column = [&](std::uint32_t element, std::uint32_t& value, std::uint32_t& row) -> bool {
                    if (element < table_base) return false;
                    const std::uint32_t delta = element - table_base;
                    row = delta / row_size;
                    if (row >= row_count) return false;
                    std::uint32_t offset = 0;
                    for (std::uint32_t i = 0; i < column; ++i) {
                        offset += ((column_info >> (i * 2)) & 3u) + 1u;
                    }
                    const std::uint32_t width = ((column_info >> (column * 2)) & 3u) + 1u;
                    const std::uint32_t address = table_base + row * row_size + offset;
                    if (!Fits(address, width)) return false;
                    if (width == 1) value = Read<std::uint8_t>(address);
                    else if (width == 2) value = Read<std::uint16_t>(address);
                    else if (width == 4) value = Read<std::uint32_t>(address);
                    else {
                        value = static_cast<std::uint32_t>(memory_[address]) |
                                (static_cast<std::uint32_t>(memory_[address + 1]) << 8) |
                                (static_cast<std::uint32_t>(memory_[address + 2]) << 16);
                    }
                    return true;
                };

                std::uint32_t lo = 0, hi = nmemb;
                while (lo < hi) {
                    const std::uint32_t mid = lo + (hi - lo) / 2;
                    const std::uint64_t element64 = static_cast<std::uint64_t>(base) + static_cast<std::uint64_t>(mid) * size;
                    if (element64 > UINT32_MAX) break;
                    const std::uint32_t element = static_cast<std::uint32_t>(element64);
                    std::uint32_t decoded = 0, row = 0;
                    if (!decode_column(element, decoded, row)) break;

                    int cmp = 0;
                    if (compar == kMonoMetadataRowComparator) {
                        cmp = target < decoded ? -1 : (target > decoded ? 1 : 0);
                    } else {
                        if (target < decoded) {
                            cmp = -1;
                        } else if (row + 1 >= row_count) {
                            cmp = 0;
                        } else {
                            const std::uint32_t next_element = table_base + (row + 1) * row_size;
                            std::uint32_t next_decoded = 0, next_row = 0;
                            if (!decode_column(next_element, next_decoded, next_row)) break;
                            if (target >= next_decoded || decoded == next_decoded) cmp = 1;
                            else cmp = 0;
                        }
                    }

                    if (cmp == 0) {
                        Write<std::uint32_t>(key + 12, row);
                        Ret(element);
                        return true;
                    }
                    if (cmp < 0) hi = mid;
                    else lo = mid + 1;
                }
                Ret(0);
                return true;
            }
            if (nmemb == 0 || size == 0) {
                Ret(0);
                return true;
            }
            const std::uint64_t total = static_cast<std::uint64_t>(nmemb) * size;
            if (total > SIZE_MAX || !Fits(base, static_cast<std::size_t>(total)) ||
                !Fits(compar & ~1u, 4u)) {
                return false;
            }
            BsearchFrame frame;
            frame.key = key;
            frame.base = base;
            frame.size = size;
            frame.comparator = compar;
            frame.low = 0;
            frame.high = nmemb;
            frame.mid = nmemb / 2u;
            frame.caller_lr = jit->Regs()[14];
            bsearch_frames_.push_back(frame);
            const std::uint32_t element = base + frame.mid * size;
            jit->Regs()[0] = key;
            jit->Regs()[1] = element;
            jit->Regs()[2] = 0;
            jit->Regs()[3] = 0;
            jit->Regs()[14] = kBsearchReturnStub;
            jit->Regs()[15] = compar & ~1u;
            jit->SetCpsr((compar & 1u) ? 0x20u : 0u);
            pending_guest_callback = true;
            jit->HaltExecution();
            return true;
        }
        return false;
    }

    struct GuestJumpContext {
        std::array<std::uint32_t, 16> regs{};
        std::uint32_t cpsr = 0;
        std::uint32_t resume_pc = 0;
        std::uint32_t thread_id = 0;
    };
    struct GuestSignalAction {
        std::uint32_t handler = 0;
        std::uint32_t mask = 0;
        std::uint32_t flags = 0;
        std::uint32_t restorer = 0;
    };
    struct GuestSignalFrame {
        std::array<std::uint32_t, 16> regs{};
        std::array<std::uint32_t, 64> ext_regs{};
        std::uint32_t cpsr = 0;
        std::uint32_t fpscr = 0;
        std::uint32_t signal = 0;
        std::uint32_t guest_siginfo = 0;
        std::uint32_t guest_ucontext = 0;
        bool uses_siginfo = false;
    };

    const std::unordered_map<std::uint32_t, std::string>& thunks_;
    const std::unordered_map<std::string, std::uint32_t>& name_to_stub_;
    const std::unordered_map<std::string, ExportSymbol>& exports_;
    std::uint32_t heap_next_ = kGuestHeapStart;
    std::uint32_t next_tls_key_ = 1;
    struct GuestThreadLaunch {
        std::uint32_t id = 0;
        std::uint32_t start = 0;
        std::uint32_t arg = 0;
        bool started = false;
        bool finished = false;
        std::array<std::uint32_t, 16> regs{};
        std::array<std::uint32_t, 64> ext_regs{};
        std::uint32_t cpsr = 0;
        std::uint32_t fpscr = 0;
        std::unordered_map<std::uint32_t, std::uint32_t> tls_values;
        std::uint32_t arm_tls = 0;
        std::size_t total_slices = 0;
    };
    std::uint32_t next_thread_id_ = 2;
    std::uint32_t current_thread_id_ = 1;
    bool cooperative_yield_requested_ = false;
    bool thread_exit_requested_ = false;
    std::vector<GuestThreadLaunch> guest_thread_launches_;
    std::size_t guest_thread_pump_cursor_ = 0;
    struct GuestMutexState {
        std::uint32_t owner = 0;
        std::uint32_t recursion = 0;
        std::uint32_t type = 0;
    };
    struct GuestCondState {
        std::uint64_t broadcast_generation = 0;
    };
    struct GuestCondWaitState {
        std::uint64_t broadcast_generation = 0;
        std::uint32_t mutex = 0;
        bool signaled = false;
        bool timed = false;
        std::chrono::system_clock::time_point deadline{};
    };
    std::unordered_map<std::uint32_t, GuestMutexState> guest_mutexes_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_mutex_attr_types_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_pthread_attr_threads_;
    std::unordered_map<std::uint32_t, GuestCondState> guest_conds_;
    std::unordered_map<std::uint64_t, GuestCondWaitState> guest_cond_waits_;
    std::unordered_map<std::uint32_t, std::int64_t> guest_semaphores_;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
    std::unordered_map<std::uint64_t, bool> guest_sem_wait_logged_;
    std::unordered_map<std::uint64_t, bool> guest_mutex_block_logged_;
    std::unordered_map<std::uint32_t, std::vector<std::string>> guest_thread_call_traces_;
    std::unordered_map<std::uint32_t, std::vector<std::string>> guest_thread_sync_traces_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_sleep_log_counts_;
#endif
    std::unordered_map<std::uint64_t, std::chrono::system_clock::time_point> guest_sem_deadlines_;
    std::unordered_map<std::uint32_t, std::size_t> allocations_;
    std::unordered_map<std::uint32_t, std::uint32_t> tls_values_;
    std::unordered_map<std::uint32_t, GuestJumpContext> jump_contexts_;
    std::unordered_map<std::uint32_t, GuestSignalAction> guest_signal_actions_;
    std::unordered_map<std::uint32_t, std::vector<std::uint32_t>> guest_pending_signals_;
    std::unordered_map<std::uint32_t, std::vector<GuestSignalFrame>> guest_signal_frames_;
    std::unordered_map<std::uint32_t, bool> guest_signal_interrupted_;
    std::unordered_map<std::uint32_t, JniArrayRecord> jni_arrays_;
    std::vector<BsearchFrame> bsearch_frames_;
    std::vector<QsortFrame> qsort_frames_;
    std::uint32_t strtok_state_ = 0;
    std::uint64_t rand48_state_ = 0x1234abcd330eULL;
    std::unordered_map<std::string, std::string> guest_environment_;
    std::unordered_map<std::uint32_t, Sha1State32> sha1_states_;
    std::unordered_map<std::uint32_t, bool> cxa_guards_in_progress_;
    struct PthreadOnceFrame {
        std::uint32_t control = 0;
        std::uint32_t caller_lr = 0;
    };
    std::vector<PthreadOnceFrame> pthread_once_frames_;
    std::unordered_map<std::uint32_t, bool> pthread_once_done_;
    std::uint32_t arm_tls_value_ = 0;
    std::uint32_t next_file_handle_ = 0x70000000u;
    std::uint32_t next_socket_handle_ = 0x72000000u;
    std::unordered_map<std::uint32_t, bool> socket_handles_;
    std::unordered_map<std::uint32_t, std::FILE*> file_handles_;
    std::unordered_map<std::uint32_t, std::string> file_paths_;
    std::unordered_map<std::uint32_t, VirtualFile> virtual_files_;
#if !defined(_WIN32)
    std::uint32_t next_dir_handle_ = 0x73000000u;
    std::unordered_map<std::uint32_t, DIR*> dir_handles_;
    std::unordered_map<std::uint32_t, std::uint32_t> dir_guest_entries_;
#endif
#if defined(__ANDROID__)
    std::uint32_t next_egl_handle_ = 0x71000000u;
    std::unordered_map<std::uint32_t, std::uintptr_t> egl_handles_;
    std::unordered_map<std::uint32_t, std::uint32_t> gl_string_cache_;
    std::unordered_map<std::uint32_t, GuestGlMappedRange> gl_mapped_ranges_;
    std::unordered_map<std::uint32_t, HostZlibStream> zlib_streams_;
#endif
    std::string managed_dir_;
    void* host_native_window_ = nullptr;
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
    HotpatchRuntime hotpatch_runtime_;
#endif
};

std::string RunMonoBootstrapTrapProbe(std::vector<std::uint8_t> memory,
                                      const std::unordered_map<std::string, ExportSymbol>& exports,
                                      const std::vector<UnresolvedRelocation>& unresolved,
                                      const std::string& managed_dir,
                                      const std::string& package_code_path,
                                      void* host_native_window) {
    std::ostringstream report;
    report << "  Mono JIT bootstrap trap probe:\n";

    ThunkInstallResult thunks;
    std::string error;
    if (!InstallTrapThunks(memory, unresolved, thunks, error)) {
        report << "    Thunk installation: FAIL (" << error << ")";
        return report.str();
    }
    report << "    Function trap stubs installed: " << thunks.name_to_stub.size() << "\n";
    report << "    Data import slots installed: " << thunks.name_to_data.size() << "\n";
    std::uint32_t unity_free_got_after_thunks = 0;
    Read32(memory, kUnityBase + 0x00a89000u, unity_free_got_after_thunks);
    const auto free_stub_after_thunks = thunks.name_to_stub.find("free");
    const std::uint32_t free_stub_after_thunks_address =
        free_stub_after_thunks != thunks.name_to_stub.end() ? free_stub_after_thunks->second : 0u;
    report << "    Unity free GOT after thunk install: got=0x" << std::hex
           << unity_free_got_after_thunks << " stub=0x" << free_stub_after_thunks_address
           << std::dec << "\n";
#if defined(__ANDROID__)
    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                        "Unity free GOT after thunk install: got=0x%08x stub=0x%08x",
                        unity_free_got_after_thunks, free_stub_after_thunks_address);
#endif

    const auto init = FindExport(exports, "mono_jit_init_version");
    if (!init || init->owner != "libmono.so") {
        report << "    mono_jit_init_version lookup: FAIL";
        return report.str();
    }
    report << "    mono_jit_init_version lookup: PASS @ 0x" << std::hex << init->address << std::dec << "\n";

    static constexpr char kDomainName[] = "CNR64 Root Domain";
    static constexpr char kRuntimeVersion[] = "v2.0.50727";
    const std::uint32_t domain_string = kBootstrapStringBase;
    const std::uint32_t runtime_string = kBootstrapStringBase + 0x100;
    const std::uint32_t managed_string = kBootstrapStringBase + 0x200;
    if (!WriteBytes(memory, domain_string, kDomainName, sizeof(kDomainName)) ||
        !WriteBytes(memory, runtime_string, kRuntimeVersion, sizeof(kRuntimeVersion)) ||
        !WriteBytes(memory, managed_string, managed_dir.c_str(), managed_dir.size() + 1) ||
        !WriteReturnStub(memory) ||
        !WriteSvcStub(memory, kPthreadOnceReturnStub, kSvcPthreadOnceReturn)) {
        report << "    Guest bootstrap data setup: FAIL";
        return report.str();
    }
    report << "    Managed assemblies path: " << managed_dir << "\n";

    BootstrapEnvironment env(memory, thunks.id_to_name, thunks.name_to_stub, exports, managed_dir, host_native_window);
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
    report << "    Developer hotpatch runtime: " << env.HotpatchStatusLine() << "\n";
#endif
    Dynarmic::ExclusiveMonitor global_monitor{1};
    Dynarmic::A32::UserConfig config;
    config.callbacks = &env;
    config.processor_id = 0;
    config.global_monitor = &global_monitor;
    config.arch_version = Dynarmic::A32::ArchVersion::v7;
    config.always_little_endian = true;
    config.enable_cycle_counting = true;
    config.code_cache_size = 32u * 1024u * 1024u;
    Dynarmic::A32::Jit jit(config);
    env.jit = &jit;

    constexpr std::uint32_t kUnityCxaThrow = kUnityBase + 0x00939fd0u;
    constexpr std::uint32_t kMainUnwindRaise = kMainBase + 0x00004d74u;
    if (!WriteSvcStub(memory, kUnityCxaThrow, kSvcCxaThrowProbe) ||
        !WriteSvcStub(memory, kMainUnwindRaise, kSvcUnwindRaiseProbe)) {
        report << "    C++ exception probe setup: FAIL\n";
        return report.str();
    }
    report << "    C++ exception probes armed: throw=0x" << std::hex << kUnityCxaThrow
           << " unwind=0x" << kMainUnwindRaise << std::dec << "\n";

    // Android's dynamic linker runs dependency init arrays before JNI_OnLoad or
    // any exported entrypoints. The original Unity binary has hundreds of C++
    // static constructors (std::string globals, registries, locks, etc.); leaving
    // these zeroed causes later code to treat null std::string data as a live rep.
    constexpr std::uint64_t kCtorTicksPerSlice = 100000;
    constexpr int kCtorMaxSlices = 100;
    std::size_t init_array_executed = 0;
    std::size_t init_array_null_entries = 0;

    auto run_guest_initializer = [&](const char* owner, std::size_t index,
                                     std::uint32_t function) -> bool {
        if (function == 0) {
            ++init_array_null_entries;
            return true;
        }
        if (std::string_view(owner) == "libunity.so" && function == kUnityBase + 0x00058ba4u) {
            report << "    ELF init-array diagnostic skip: libunity locale bootstrap fn=0x"
                   << std::hex << function << std::dec << "\n";
            return true;
        }

#if defined(__ANDROID__)
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                            "CTOR enter %s[%zu] fn=0x%08x", owner, index, function);
#endif

        env.ResetPhaseDiagnostics();
        env.saw_return = false;
        env.failed = false;
        env.fault_pc = 0;
        jit.ClearHalt();
        jit.Regs().fill(0);
        jit.Regs()[13] = kStackTop;
        jit.Regs()[14] = kReturnStub;
        jit.Regs()[15] = function & ~1u;
        jit.SetCpsr((function & 1u) ? 0x20u : 0u);

        std::uint32_t previous_pc = 0xffffffffu;
        int same_pc_slices = 0;
        std::size_t cache_restarts = 0;
        int slices = 0;
        for (; slices < kCtorMaxSlices; ++slices) {
            env.ticks_left = kCtorTicksPerSlice;
            const auto halt_reason = jit.Run();
#if defined(__ANDROID__)
            std::uint32_t helper_word = 0;
            std::uint32_t throw_word = 0;
            Read32(memory, kUnityBase + 0x0090d604u, helper_word);
            Read32(memory, kUnityCxaThrow, throw_word);
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "CTOR slice %s[%zu] n=%d pc=0x%08x halt=0x%llx helper=0x%08x throw=0x%08x",
                                owner, index, slices, jit.Regs()[15],
                                static_cast<unsigned long long>(halt_reason), helper_word, throw_word);
#endif
            if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
            if (env.pending_guest_callback) {
                env.pending_guest_callback = false;
                jit.ClearHalt();
                continue;
            }
            if (env.pending_cacheflush) {
                const std::uint32_t start = env.pending_cacheflush_start;
                const std::uint32_t size = env.pending_cacheflush_size;
                env.pending_cacheflush = false;
                env.pending_cacheflush_start = 0;
                env.pending_cacheflush_size = 0;
                jit.ClearHalt();
                if (size != 0) jit.InvalidateCacheRange(start, size);
                ++cache_restarts;
                previous_pc = 0xffffffffu;
                same_pc_slices = 0;
                continue;
            }
            if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                jit.ClearHalt();
                ++cache_restarts;
                previous_pc = 0xffffffffu;
                same_pc_slices = 0;
                continue;
            }
            const std::uint32_t pc = jit.Regs()[15];
            if (pc == previous_pc) ++same_pc_slices;
            else same_pc_slices = 0;
            previous_pc = pc;
            if (same_pc_slices >= 2) break;
        }

        const bool ok = env.saw_return && !env.failed && !env.premature_return && env.first_thunk.empty();
        if (!ok) {
            report << "    ELF init-array failure: " << owner << "[" << index << "] fn=0x"
                   << std::hex << function << std::dec << " slices=" << slices
                   << " cache_restarts=" << cache_restarts << "\n";
            if (!env.first_thunk.empty())
                report << "      blocked by thunk: " << env.first_thunk << " (SVC " << env.first_thunk_id << ")\n";
            if (env.unexpected_svc0_pc != 0)
                report << "      unexpected svc0: pc=0x" << std::hex << env.unexpected_svc0_pc
                       << " r7=0x" << env.unexpected_svc0_r7 << std::dec << "\n";
            if (env.bad_write_address != 0)
                report << "      bad write: addr=0x" << std::hex << env.bad_write_address
                       << " pc=0x" << env.bad_write_pc << " lr=0x" << env.bad_write_lr << std::dec << "\n";
            if (env.failed) {
                report << "      fault_pc=0x" << std::hex << env.fault_pc
                       << " exception=" << std::dec << env.fault_exception
                       << " fallback_count=" << env.fallback_instruction_count << "\n";
                report << "      registers:";
                for (std::size_t r = 0; r < 16; ++r)
                    report << " r" << r << "=0x" << std::hex << jit.Regs()[r] << std::dec;
                report << "\n";
                report << "      code-read trail: 0x" << std::hex << env.code_read_prev3
                       << " 0x" << env.code_read_prev2 << " 0x" << env.code_read_prev1
                       << " 0x" << env.code_read_last << std::dec << "\n";
            }
            if (!env.saw_return && !env.failed && !env.premature_return && env.first_thunk.empty()) {
                report << "      constructor budget exhausted: pc=0x" << std::hex << jit.Regs()[15]
                       << " lr=0x" << jit.Regs()[14] << std::dec << "\n";
            }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (!env.call_trace.empty()) {
                report << "      thunk trace:";
                const std::size_t trace_start = env.call_trace.size() > 24 ? env.call_trace.size() - 24 : 0;
                for (std::size_t i = trace_start; i < env.call_trace.size(); ++i)
                    report << " " << env.call_trace[i];
                report << "\n";
            }
#endif
            return false;
        }
        ++init_array_executed;
        return true;
    };

    auto run_init_array = [&](const char* owner, std::uint32_t array_address,
                              std::size_t count) -> bool {
        for (std::size_t i = 0; i < count; ++i) {
            std::uint32_t function = 0;
            if (!Read32(memory, array_address + static_cast<std::uint32_t>(i * 4u), function)) {
                report << "    ELF init-array read failure: " << owner << "[" << i << "]\n";
                return false;
            }
            if (!run_guest_initializer(owner, i, function)) return false;
        }
        return true;
    };

    // DT_NEEDED ordering for libunity: dependencies first, then libunity itself.
    env.live_ctor_trace = true;
    const bool elf_init_ok =
        run_init_array("libmono.so", kMonoBase + 0x003ac680u, 1) &&
        run_init_array("libmain.so", kMainBase + 0x000073a4u, 1) &&
        run_init_array("libunity.so", kUnityBase + 0x00a87f3cu, 940u / 4u);
    env.live_ctor_trace = false;
    report << "    ELF init arrays: " << (elf_init_ok ? "PASS" : "FAIL")
           << " executed=" << init_array_executed
           << " null_entries=" << init_array_null_entries << "\n";
    if (!elf_init_ok) return report.str();

    const auto set_assemblies = FindExport(exports, "mono_set_assemblies_path");
    bool assemblies_path_ok = false;
    if (set_assemblies && set_assemblies->owner == "libmono.so") {
        env.ticks_left = 5000000;
        jit.Regs().fill(0);
        jit.Regs()[0] = managed_string;
        jit.Regs()[13] = kStackTop;
        jit.Regs()[14] = kReturnStub;
        jit.Regs()[15] = set_assemblies->address & ~1u;
        jit.SetCpsr((set_assemblies->address & 1u) ? 0x20u : 0u);
        jit.Run();
        assemblies_path_ok = env.saw_return && !env.failed && env.first_thunk.empty();
    }
    report << "    mono_set_assemblies_path: " << (assemblies_path_ok ? "PASS" : "FAIL") << "\n";
    if (!assemblies_path_ok) {
        if (!env.first_thunk.empty()) report << "      stopped at thunk: " << env.first_thunk << "\n";
        if (env.failed) report << "      fault_pc=0x" << std::hex << env.fault_pc << std::dec << "\n";
        return report.str();
    }

    env.ResetPhaseDiagnostics();
    env.saw_return = false;
    env.failed = false;
    env.fault_pc = 0;
    env.first_thunk.clear();
    env.first_thunk_id = 0;
    jit.ClearHalt();
    jit.Regs().fill(0);
    jit.Regs()[0] = domain_string;
    jit.Regs()[1] = runtime_string;
    jit.Regs()[13] = kStackTop;
    jit.Regs()[14] = kReturnStub;
    jit.Regs()[15] = init->address & ~1u;
    jit.SetCpsr((init->address & 1u) ? 0x20u : 0u);

    // Real Unity launches must let Unity own Mono initialization. Running
    // mono_jit_init_version here first was useful as an early standalone JIT
    // validation probe, but it makes Unity's later normal initialization a
    // second mono_init_internal() call and trips Mono's domain.c assertion.
    constexpr bool kUnityOwnsMonoInitialization = true;
    constexpr std::uint64_t kTicksPerSlice = 1000000;
    constexpr int kMaxSlices = 100;
    int slices = 0;
    std::size_t cache_invalidation_restarts = 0;
    std::uint32_t previous_pc = 0xffffffffu;
    int same_pc_slices = 0;
    for (; !kUnityOwnsMonoInitialization && slices < kMaxSlices; ++slices) {
        env.ticks_left = kTicksPerSlice;
        const auto halt_reason = jit.Run();
        if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
        if (env.pending_guest_callback) {
            env.pending_guest_callback = false;
            jit.ClearHalt();
            continue;
        }
        if (env.pending_cacheflush) {
            const std::uint32_t start = env.pending_cacheflush_start;
            const std::uint32_t size = env.pending_cacheflush_size;
            env.pending_cacheflush = false;
            env.pending_cacheflush_start = 0;
            env.pending_cacheflush_size = 0;
            jit.ClearHalt();
            if (size != 0) jit.InvalidateCacheRange(start, size);
            ++cache_invalidation_restarts;
            previous_pc = 0xffffffffu;
            same_pc_slices = 0;
            continue;
        }
        if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
            ++cache_invalidation_restarts;
            previous_pc = 0xffffffffu;
            same_pc_slices = 0;
            continue;
        }
        if (env.ConsumeCooperativeYieldRequest()) {
            env.PumpQueuedGuestThreads(32, 4);
            env.ResetPhaseDiagnostics();
            env.saw_return = false;
            env.premature_return = false;
            env.failed = false;
            env.fault_pc = 0;
            jit.ClearHalt();
            previous_pc = 0xffffffffu;
            same_pc_slices = 0;
            continue;
        }
        const std::uint32_t pc = jit.Regs()[15];
        if (pc == previous_pc) ++same_pc_slices;
        else same_pc_slices = 0;
        previous_pc = pc;
        if (same_pc_slices >= 2) break;
    }

    const bool init_returned = env.saw_return;
    const bool init_premature_return = env.premature_return;
    const std::uint32_t init_return_sp = env.return_sp;
    const std::uint32_t init_return_r11 = env.return_r11;
    const bool init_failed = env.failed;
    const std::uint32_t init_fault_pc = env.fault_pc;
    const std::string init_first_thunk = env.first_thunk;
    const std::uint32_t init_first_thunk_id = env.first_thunk_id;
    const std::uint32_t init_domain = init_returned ? jit.Regs()[0] : 0u;

    // The original Mono startup first stores the created root domain at 0x3b3cb0,
    // then copies it to the global read by mono_get_root_domain() at 0x3b3c78.
    constexpr std::uint32_t kMonoCreatedRootDomainGlobal = kMonoBase + 0x003b3cb0u;
    constexpr std::uint32_t kMonoRootDomainGlobal = kMonoBase + 0x003b3c78u;
    std::uint32_t created_root_global_after_init = 0;
    std::uint32_t root_global_after_init = 0;
    Read32(memory, kMonoCreatedRootDomainGlobal, created_root_global_after_init);
    Read32(memory, kMonoRootDomainGlobal, root_global_after_init);
    bool root_domain_match = false;
    std::uint32_t root_domain_after_init = 0;
    std::uint32_t current_domain_after_init = 0;
    if (init_returned && init_domain != 0) {
        const auto root = FindExport(exports, "mono_get_root_domain");
        if (root && root->owner == "libmono.so") {
            env.saw_return = false;
            env.failed = false;
            env.fault_pc = 0;
            env.first_thunk.clear();
            env.first_thunk_id = 0;
            env.ticks_left = 1000000;
            jit.ClearHalt();
            jit.Regs().fill(0);
            jit.Regs()[13] = kStackTop;
            jit.Regs()[14] = kReturnStub;
            jit.Regs()[15] = root->address & ~1u;
            jit.SetCpsr((root->address & 1u) ? 0x20u : 0u);
            jit.Run();
            root_domain_after_init = jit.Regs()[0];
            root_domain_match = env.saw_return && !env.failed && root_domain_after_init == init_domain;
        }

        const auto current = FindExport(exports, "mono_domain_get");
        if (current && current->owner == "libmono.so") {
            env.saw_return = false;
            env.failed = false;
            env.fault_pc = 0;
            env.first_thunk.clear();
            env.first_thunk_id = 0;
            env.ticks_left = 1000000;
            jit.ClearHalt();
            jit.Regs().fill(0);
            jit.Regs()[13] = kStackTop;
            jit.Regs()[14] = kReturnStub;
            jit.Regs()[15] = current->address & ~1u;
            jit.SetCpsr((current->address & 1u) ? 0x20u : 0u);
            jit.Run();
            if (env.saw_return && !env.failed && env.first_thunk.empty())
                current_domain_after_init = jit.Regs()[0];
        }
    }

    report << "    Execution slices: " << (slices + 1) << " x up to " << kTicksPerSlice << " ticks\n";
    report << "    Cache invalidation restarts: " << cache_invalidation_restarts << "\n";
    report << "    Implemented thunk calls handled: " << env.handled_calls << "\n";
    report << "    Executable mprotect calls: " << env.executable_mprotect_calls;
    if (env.first_exec_region) report << " first=0x" << std::hex << env.first_exec_region << "+0x" << env.first_exec_region_size << std::dec;
    report << "\n";
    report << "    ARM cacheflush syscalls: " << env.arm_cacheflush_calls << "\n";
    report << "    Dynarmic fetched code from Mono guest heap: " << (env.executed_heap_code ? "YES" : "no");
    if (env.executed_heap_code) report << " first_pc=0x" << std::hex << env.first_heap_code_pc << std::dec;
    report << "\n";
    report << "    Saved mono_jit_init_version return: 0x" << std::hex << init_domain << std::dec << "\n";
    report << "    Root globals after init: created=0x" << std::hex << created_root_global_after_init
           << " published=0x" << root_global_after_init << std::dec << "\n";
    if (init_domain != 0) {
        report << "    Root domain validation: " << (root_domain_match ? "PASS" : "FAIL")
               << " init=0x" << std::hex << init_domain
               << " get_root=0x" << root_domain_after_init
               << " domain_get=0x" << current_domain_after_init << std::dec << "\n";
    }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
    if (!env.call_trace.empty()) {
        report << "    Early thunk trace:";
        for (const auto& name : env.call_trace) report << " " << name;
        report << "\n";
    }
    if (!env.allocation_trace.empty()) {
        report << "    Allocation trace:";
        for (const auto& item : env.allocation_trace)
            report << " 0x" << std::hex << item.first << ":0x" << item.second << std::dec;
        report << "\n";
    }
    if (!env.file_trace.empty()) {
        report << "    File trace:\n";
        for (const auto& item : env.file_trace) report << "      " << item << "\n";
    }
    if (!env.diagnostic_formats.empty()) {
        report << "    Guest diagnostics:\n";
        for (const auto& text : env.diagnostic_formats) report << "      " << text << "\n";
    }
#endif
    if (kUnityOwnsMonoInitialization) {
        report << "    Mono pre-initialization: SKIPPED (Unity owns runtime initialization)\n";
    } else if (init_premature_return) {
        report << "    PREMATURE sentinel return: sp=0x" << std::hex << init_return_sp
               << " expected=0x" << kStackTop << " r11=0x" << init_return_r11
               << " r0=0x" << env.premature_r0 << std::dec << "\n";
        if (!env.premature_frames.empty()) {
            report << "    Guest frame unwind (fp -> saved lr):";
            for (const auto& frame : env.premature_frames)
                report << " 0x" << std::hex << frame.first << "->0x" << frame.second << std::dec;
        }
    } else if (!init_first_thunk.empty()) {
        report << "    Next unsupported external thunk: " << init_first_thunk
               << " (SVC " << init_first_thunk_id << ")\n";
        report << "    Bootstrap dispatch: PASS (advanced through implemented services, stopped safely)";
    } else if (init_returned) {
        report << "    mono_jit_init_version RETURNED, saved r0=0x"
               << std::hex << init_domain << std::dec;
    } else if (init_failed) {
        report << "    Bootstrap execution: FAIL before unsupported thunk, fault_pc=0x"
               << std::hex << init_fault_pc << std::dec;
    } else {
        report << "    Bootstrap execution stopped without return/thunk, pc=0x"
               << std::hex << jit.Regs()[15] << " sp=0x" << jit.Regs()[13]
               << " lr=0x" << jit.Regs()[14] << " r0=0x" << jit.Regs()[0]
               << " r1=0x" << jit.Regs()[1] << " r2=0x" << jit.Regs()[2]
               << " r3=0x" << jit.Regs()[3] << std::dec;
    }
    if (kUnityOwnsMonoInitialization || (init_returned && init_domain != 0 && root_domain_match)) {
        const auto unity_jni = FindExport(exports, "libunity.so!JNI_OnLoad");
        report << "    Shared-state Unity JNI lookup: ";
        if (!unity_jni) {
            report << "FAIL (libunity.so JNI_OnLoad not indexed)\n";
        } else {
            report << "PASS owner=" << unity_jni->owner << " @ 0x"
                   << std::hex << unity_jni->address << std::dec << "\n";

            bool jni_stubs_ok =
                WriteSvcStub(memory, kJniAttachStub, kSvcJniAttach) &&
                WriteSvcStub(memory, kJniFindClassStub, kSvcJniFindClass) &&
                WriteSvcStub(memory, kJniRegisterNativesStub, kSvcJniRegisterNatives) &&
                WriteSvcStub(memory, kJniFatalErrorStub, kSvcJniFatalError) &&
                WriteSvcStub(memory, kJniGetJavaVmStub, kSvcJniGetJavaVm) &&
                WriteSvcStub(memory, kJniGetEnvStub, kSvcJniGetEnv) &&
                WriteSvcStub(memory, kJniNewGlobalRefStub, kSvcJniNewGlobalRef) &&
                WriteSvcStub(memory, kJniDeleteGlobalRefStub, kSvcJniDeleteGlobalRef) &&
                WriteSvcStub(memory, kJniGetObjectClassStub, kSvcJniGetObjectClass) &&
                WriteSvcStub(memory, kJniExceptionCheckStub, kSvcJniExceptionCheck) &&
                WriteSvcStub(memory, kJniPushLocalFrameStub, kSvcJniPushLocalFrame) &&
                WriteSvcStub(memory, kJniPopLocalFrameStub, kSvcJniPopLocalFrame) &&
                WriteSvcStub(memory, kJniGetStaticFieldIdStub, kSvcJniGetStaticFieldId) &&
                WriteSvcStub(memory, kJniGetStaticObjectFieldStub, kSvcJniGetStaticObjectField) &&
                WriteSvcStub(memory, kJniGetStaticIntFieldStub, kSvcJniGetStaticIntField) &&
                WriteSvcStub(memory, kJniCallIntMethodStub, kSvcJniCallIntMethod) &&
                WriteSvcStub(memory, kJniCallIntMethodVStub, kSvcJniCallIntMethodV) &&
                WriteSvcStub(memory, kJniCallIntMethodAStub, kSvcJniCallIntMethodA) &&
                WriteSvcStub(memory, kJniCallBooleanMethodStub, kSvcJniCallBooleanMethod) &&
                WriteSvcStub(memory, kJniCallBooleanMethodVStub, kSvcJniCallBooleanMethodV) &&
                WriteSvcStub(memory, kJniCallBooleanMethodAStub, kSvcJniCallBooleanMethodA) &&
                WriteSvcStub(memory, kJniCallVoidMethodStub, kSvcJniCallVoidMethod) &&
                WriteSvcStub(memory, kJniCallVoidMethodVStub, kSvcJniCallVoidMethodV) &&
                WriteSvcStub(memory, kJniCallVoidMethodAStub, kSvcJniCallVoidMethodA) &&
                WriteSvcStub(memory, kJniIsInstanceOfStub, kSvcJniIsInstanceOf) &&
                WriteSvcStub(memory, kJniGetStringUtfLengthStub, kSvcJniGetStringUtfLength) &&
                WriteSvcStub(memory, kJniGetStringUtfCharsStub, kSvcJniGetStringUtfChars) &&
                WriteSvcStub(memory, kJniReleaseStringUtfCharsStub, kSvcJniReleaseStringUtfChars) &&
                WriteSvcStub(memory, kJniAllocObjectStub, kSvcJniAllocObject) &&
                WriteSvcStub(memory, kJniNewObjectStub, kSvcJniNewObject) &&
                WriteSvcStub(memory, kJniNewObjectVStub, kSvcJniNewObjectV) &&
                WriteSvcStub(memory, kJniNewObjectAStub, kSvcJniNewObjectA) &&
                WriteSvcStub(memory, kJniGetFieldIdStub, kSvcJniGetFieldId) &&
                WriteSvcStub(memory, kJniGetObjectFieldStub, kSvcJniGetObjectField) &&
                WriteSvcStub(memory, kJniGetPrimitiveFieldStub, kSvcJniGetPrimitiveField) &&
                WriteSvcStub(memory, kJniSetFieldStub, kSvcJniSetField) &&
                WriteSvcStub(memory, kJniNewBooleanArrayStub, kSvcJniNewBooleanArray) &&
                WriteSvcStub(memory, kJniNewByteArrayStub, kSvcJniNewByteArray) &&
                WriteSvcStub(memory, kJniNewCharArrayStub, kSvcJniNewCharArray) &&
                WriteSvcStub(memory, kJniNewShortArrayStub, kSvcJniNewShortArray) &&
                WriteSvcStub(memory, kJniNewIntArrayStub, kSvcJniNewIntArray) &&
                WriteSvcStub(memory, kJniNewLongArrayStub, kSvcJniNewLongArray) &&
                WriteSvcStub(memory, kJniNewFloatArrayStub, kSvcJniNewFloatArray) &&
                WriteSvcStub(memory, kJniNewDoubleArrayStub, kSvcJniNewDoubleArray) &&
                WriteSvcStub(memory, kJniGetArrayLengthStub, kSvcJniGetArrayLength) &&
                WriteSvcStub(memory, kJniNewObjectArrayStub, kSvcJniNewObjectArray) &&
                WriteSvcStub(memory, kJniGetObjectArrayElementStub, kSvcJniGetObjectArrayElement) &&
                WriteSvcStub(memory, kJniSetObjectArrayElementStub, kSvcJniSetObjectArrayElement) &&
                WriteSvcStub(memory, kJniGetPrimitiveArrayElementsStub, kSvcJniGetPrimitiveArrayElements) &&
                WriteSvcStub(memory, kJniReleasePrimitiveArrayElementsStub, kSvcJniReleasePrimitiveArrayElements) &&
                WriteSvcStub(memory, kJniGetPrimitiveArrayRegionStub, kSvcJniGetPrimitiveArrayRegion) &&
                WriteSvcStub(memory, kJniSetPrimitiveArrayRegionStub, kSvcJniSetPrimitiveArrayRegion) &&
                WriteSvcStub(memory, kJniGetStaticMethodIdStub, kSvcJniGetStaticMethodId) &&
                WriteSvcStub(memory, kJniCallStaticObjectMethodStub, kSvcJniCallStaticObjectMethod) &&
                WriteSvcStub(memory, kJniCallStaticPrimitiveMethodStub, kSvcJniCallStaticPrimitiveMethod) &&
                WriteSvcStub(memory, kJniCallStaticVoidMethodStub, kSvcJniCallStaticVoidMethod) &&
                WriteSvcStub(memory, kJniNewStringUtfStub, kSvcJniNewStringUtf) &&
                WriteSvcStub(memory, kJniGetMethodIdStub, kSvcJniGetMethodId) &&
                WriteSvcStub(memory, kJniCallObjectMethodStub, kSvcJniCallObjectMethod) &&
                WriteSvcStub(memory, kJniCallObjectMethodVStub, kSvcJniCallObjectMethodV) &&
                WriteSvcStub(memory, kJniCallObjectMethodAStub, kSvcJniCallObjectMethodA) &&
                WriteSvcStub(memory, kJniDeleteLocalRefStub, kSvcJniDeleteLocalRef) &&
                WriteSvcStub(memory, kJniNewLocalRefStub, kSvcJniNewLocalRef) &&
                WriteSvcStub(memory, kJniIsSameObjectStub, kSvcJniIsSameObject) &&
                WriteSvcStub(memory, kJniExceptionOccurredStub, kSvcJniExceptionOccurred) &&
                WriteSvcStub(memory, kJniExceptionDescribeStub, kSvcJniExceptionDescribe) &&
                WriteSvcStub(memory, kJniExceptionClearStub, kSvcJniExceptionClear) &&
                WriteSvcStub(memory, kPthreadOnceReturnStub, kSvcPthreadOnceReturn) &&
                Write32(memory, kJniVmObject, kJniVmTable) &&
                Write32(memory, kJniVmTable + 0x10u, kJniAttachStub) &&
                Write32(memory, kJniVmTable + 0x18u, kJniGetEnvStub) &&
                Write32(memory, kJniEnvObject, kJniEnvTable) &&
                Write32(memory, kJniEnvTable + 0x18u, kJniFindClassStub) &&
                Write32(memory, kJniEnvTable + 0x3cu, kJniExceptionOccurredStub) &&
                Write32(memory, kJniEnvTable + 0x40u, kJniExceptionDescribeStub) &&
                Write32(memory, kJniEnvTable + 0x44u, kJniExceptionClearStub) &&
                Write32(memory, kJniEnvTable + 0x48u, kJniFatalErrorStub) &&
                Write32(memory, kJniEnvTable + 0x4cu, kJniPushLocalFrameStub) &&
                Write32(memory, kJniEnvTable + 0x50u, kJniPopLocalFrameStub) &&
                Write32(memory, kJniEnvTable + 0x54u, kJniNewGlobalRefStub) &&
                Write32(memory, kJniEnvTable + 0x58u, kJniDeleteGlobalRefStub) &&
                Write32(memory, kJniEnvTable + 0x5cu, kJniDeleteLocalRefStub) &&
                Write32(memory, kJniEnvTable + 0x60u, kJniIsSameObjectStub) &&
                Write32(memory, kJniEnvTable + 0x64u, kJniNewLocalRefStub) &&
                Write32(memory, kJniEnvTable + 0x6cu, kJniAllocObjectStub) &&
                Write32(memory, kJniEnvTable + 0x70u, kJniNewObjectStub) &&
                Write32(memory, kJniEnvTable + 0x74u, kJniNewObjectVStub) &&
                Write32(memory, kJniEnvTable + 0x78u, kJniNewObjectAStub) &&
                Write32(memory, kJniEnvTable + 0x7cu, kJniGetObjectClassStub) &&
                Write32(memory, kJniEnvTable + 0x80u, kJniIsInstanceOfStub) &&
                Write32(memory, kJniEnvTable + 0x84u, kJniGetMethodIdStub) &&
                Write32(memory, kJniEnvTable + 0x88u, kJniCallObjectMethodStub) &&
                Write32(memory, kJniEnvTable + 0x8cu, kJniCallObjectMethodVStub) &&
                Write32(memory, kJniEnvTable + 0x90u, kJniCallObjectMethodAStub) &&
                Write32(memory, kJniEnvTable + 0xc8u, kJniCallIntMethodStub) &&
                Write32(memory, kJniEnvTable + 0xccu, kJniCallIntMethodVStub) &&
                Write32(memory, kJniEnvTable + 0xd0u, kJniCallIntMethodAStub) &&
                Write32(memory, kJniEnvTable + 0x98u, kJniCallBooleanMethodStub) &&
                Write32(memory, kJniEnvTable + 0x9cu, kJniCallBooleanMethodVStub) &&
                Write32(memory, kJniEnvTable + 0xa0u, kJniCallBooleanMethodAStub) &&
                Write32(memory, kJniEnvTable + 0xf8u, kJniCallVoidMethodStub) &&
                Write32(memory, kJniEnvTable + 0xfcu, kJniCallVoidMethodVStub) &&
                Write32(memory, kJniEnvTable + 0x100u, kJniCallVoidMethodAStub) &&
                Write32(memory, kJniEnvTable + 0x178u, kJniGetFieldIdStub) &&
                Write32(memory, kJniEnvTable + 0x17cu, kJniGetObjectFieldStub) &&
                Write32(memory, kJniEnvTable + 0x180u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x184u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x188u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x18cu, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x190u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x194u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x198u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x19cu, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1a0u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1a4u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1a8u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1acu, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1b0u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1b4u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1b8u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1bcu, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1c0u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x2acu, kJniGetArrayLengthStub) &&
                Write32(memory, kJniEnvTable + 0x2b0u, kJniNewObjectArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2b4u, kJniGetObjectArrayElementStub) &&
                Write32(memory, kJniEnvTable + 0x2b8u, kJniSetObjectArrayElementStub) &&
                Write32(memory, kJniEnvTable + 0x2bcu, kJniNewBooleanArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2c0u, kJniNewByteArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2c4u, kJniNewCharArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2c8u, kJniNewShortArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2ccu, kJniNewIntArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2d0u, kJniNewLongArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2d4u, kJniNewFloatArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2d8u, kJniNewDoubleArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2dcu, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2e0u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2e4u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2e8u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2ecu, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2f0u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2f4u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2f8u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2fcu, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x300u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x304u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x308u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x30cu, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x310u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x314u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x318u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x31cu, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x320u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x324u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x328u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x32cu, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x330u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x334u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x338u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x33cu, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x340u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x344u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x348u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x34cu, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x350u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x354u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x358u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x35cu, kJniRegisterNativesStub) &&
                Write32(memory, kJniEnvTable + 0x240u, kJniGetStaticFieldIdStub) &&
                Write32(memory, kJniEnvTable + 0x244u, kJniGetStaticObjectFieldStub) &&
                Write32(memory, kJniEnvTable + 0x258u, kJniGetStaticIntFieldStub) &&
                Write32(memory, kJniEnvTable + 0x29cu, kJniNewStringUtfStub) &&
                Write32(memory, kJniEnvTable + 0x2a0u, kJniGetStringUtfLengthStub) &&
                Write32(memory, kJniEnvTable + 0x2a4u, kJniGetStringUtfCharsStub) &&
                Write32(memory, kJniEnvTable + 0x2a8u, kJniReleaseStringUtfCharsStub) &&
                Write32(memory, kJniEnvTable + 0x36cu, kJniGetJavaVmStub) &&
                Write32(memory, kJniEnvTable + 0x390u, kJniExceptionCheckStub);

            if (jni_stubs_ok) {
                jni_stubs_ok =
                    Write32(memory, kJniEnvTable + 113u * 4u, kJniGetStaticMethodIdStub) &&
                    Write32(memory, kJniEnvTable + 114u * 4u, kJniCallStaticObjectMethodStub) &&
                    Write32(memory, kJniEnvTable + 115u * 4u, kJniCallStaticObjectMethodStub) &&
                    Write32(memory, kJniEnvTable + 116u * 4u, kJniCallStaticObjectMethodStub);
                for (std::uint32_t slot = 117u; jni_stubs_ok && slot <= 140u; ++slot)
                    jni_stubs_ok = Write32(memory, kJniEnvTable + slot * 4u, kJniCallStaticPrimitiveMethodStub);
                for (std::uint32_t slot = 141u; jni_stubs_ok && slot <= 143u; ++slot)
                    jni_stubs_ok = Write32(memory, kJniEnvTable + slot * 4u, kJniCallStaticVoidMethodStub);
            }

            for (std::uint32_t slot = 4; jni_stubs_ok && slot < kJniEnvFunctionCount; ++slot) {
                std::uint32_t entry = 0;
                if (!Read32(memory, kJniEnvTable + slot * 4u, entry)) {
                    jni_stubs_ok = false;
                    break;
                }
                if (entry != 0) continue;
                const std::uint32_t stub = kJniUnknownStubBase + slot * 8u;
                jni_stubs_ok = WriteSvcStub(memory, stub, kSvcJniUnknownBase + slot) &&
                               Write32(memory, kJniEnvTable + slot * 4u, stub);
            }

            if (!jni_stubs_ok) {
                report << "    Shared-state Unity JNI setup: FAIL writing VM/env tables\n";
            } else {
                env.ResetPhaseDiagnostics();
                env.saw_return = false;
                env.premature_return = false;
                env.failed = false;
                env.fault_pc = 0;
                env.ticks_left = kTicksPerSlice;
                jit.ClearHalt();
                jit.Regs().fill(0);
                jit.Regs()[0] = kJniVmObject;
                jit.Regs()[1] = 0;
                jit.Regs()[13] = kStackTop;
                jit.Regs()[14] = kReturnStub;
                jit.Regs()[15] = unity_jni->address & ~1u;
                jit.SetCpsr((unity_jni->address & 1u) ? 0x20u : 0u);

                constexpr int kUnityJniMaxSlices = 20;
                int unity_jni_slices = 0;
                std::size_t unity_jni_cache_restarts = 0;
                std::uint32_t unity_previous_pc = 0xffffffffu;
                int unity_same_pc_slices = 0;
                for (; unity_jni_slices < kUnityJniMaxSlices; ++unity_jni_slices) {
                    env.ticks_left = kTicksPerSlice;
                    const auto halt_reason = jit.Run();
                    if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
                    if (env.pending_guest_callback) {
                        env.pending_guest_callback = false;
                        jit.ClearHalt();
                        continue;
                    }
                    if (env.pending_cacheflush) {
                        const std::uint32_t start = env.pending_cacheflush_start;
                        const std::uint32_t size = env.pending_cacheflush_size;
                        env.pending_cacheflush = false;
                        env.pending_cacheflush_start = 0;
                        env.pending_cacheflush_size = 0;
                        jit.ClearHalt();
                        if (size != 0) jit.InvalidateCacheRange(start, size);
                        ++unity_jni_cache_restarts;
                        unity_previous_pc = 0xffffffffu;
                        unity_same_pc_slices = 0;
                        continue;
                    }
                    if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                        ++unity_jni_cache_restarts;
                        unity_previous_pc = 0xffffffffu;
                        unity_same_pc_slices = 0;
                        continue;
                    }
                    const std::uint32_t pc = jit.Regs()[15];
                    if (pc == unity_previous_pc) ++unity_same_pc_slices;
                    else unity_same_pc_slices = 0;
                    unity_previous_pc = pc;
                    if (unity_same_pc_slices >= 2) break;
                }

                const std::uint32_t unity_jni_result = env.saw_return ? jit.Regs()[0] : 0u;
                report << "    Shared-state Unity JNI execution: slices=" << unity_jni_slices
                       << " cache_restarts=" << unity_jni_cache_restarts
                       << " returned=" << (env.saw_return ? "YES" : "NO")
                       << " result=0x" << std::hex << unity_jni_result << std::dec << "\n";
                report << "    Shared-state Unity JNI AttachCurrentThread: "
                       << (env.jni_attach_seen ? "YES" : "NO") << "\n";
                report << "    Shared-state Unity JNI classes (" << env.jni_requested_classes.size() << "):\n";
                for (const auto& class_name : env.jni_requested_classes) {
                    report << "      " << class_name << "\n";
                }
                report << "    Shared-state Unity JNI native registrations ("
                       << env.jni_native_registrations.size() << "):\n";
                std::uint32_t unity_init_jni = 0;
                std::uint32_t unity_native_file = 0;
                std::uint32_t unity_native_init_www = 0;
                std::uint32_t unity_native_set_default_display = 0;
                std::uint32_t unity_native_recreate_gfx_state = 0;
                std::uint32_t unity_native_resize = 0;
                std::uint32_t unity_native_resume = 0;
                std::uint32_t unity_native_render = 0;
                for (const auto& reg : env.jni_native_registrations) {
                    report << "      " << reg.class_name << " :: " << reg.name << reg.signature
                           << " -> 0x" << std::hex << reg.function << std::dec << "\n";
                    if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                        reg.name == "initJni" && reg.signature == "(Landroid/content/Context;)V") {
                        unity_init_jni = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeFile" && reg.signature == "(Ljava/lang/String;)V") {
                        unity_native_file = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeInitWWW" && reg.signature == "(Ljava/lang/Class;)V") {
                        unity_native_init_www = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeSetDefaultDisplay" && reg.signature == "(I)V") {
                        unity_native_set_default_display = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeRecreateGfxState" && reg.signature == "(Landroid/view/Surface;)V") {
                        unity_native_recreate_gfx_state = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeResize" && reg.signature == "(IIII)V") {
                        unity_native_resize = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeResume" && reg.signature == "()V") {
                        unity_native_resume = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeRender" && reg.signature == "()Z") {
                        unity_native_render = reg.function;
                    }
                }
                const bool unity_jni_ready = env.saw_return && !env.failed && env.first_thunk.empty() &&
                                             !env.jni_fatal_error_seen && unity_jni_result == 0x00010006u;
                if (!env.first_thunk.empty()) {
                    report << "    Shared-state Unity JNI blocked by thunk: " << env.first_thunk
                           << " (SVC " << env.first_thunk_id << ")\n";
                }
                if (env.failed) {
                    report << "    Shared-state Unity JNI fault PC: 0x" << std::hex
                           << env.fault_pc << std::dec << "\n";
                }
                if (env.jni_fatal_error_seen) {
                    report << "    Shared-state Unity JNI FatalError: YES\n";
                }

                report << "    Shared-state Unity initJni lookup: ";
                if (!unity_jni_ready) {
                    report << "SKIPPED because JNI_OnLoad did not complete cleanly\n";
                } else if (unity_init_jni == 0) {
                    report << "FAIL (registered initJni not found)\n";
                } else {
                    report << "PASS @ 0x" << std::hex << unity_init_jni << std::dec << "\n";

                    // Unity 4.6.1f1's per-thread Android state getter performs a legacy
                    // process-signal initialization during its first allocation. Unity 5.5's
                    // initJni path no longer does this. The ARM32 guest must not install host
                    // process signal handlers, so bypass this one version-verified call while
                    // retaining the TLS object allocation itself.
                    constexpr std::uint32_t kLegacySignalInitCall = kUnityBase + 0x003d29b8u;
                    constexpr std::uint32_t kLegacySignalInitBl = 0xeb0010f3u;
                    constexpr std::uint32_t kArmNop = 0xe1a00000u;
                    std::uint32_t legacy_signal_instruction = 0;
                    const bool legacy_signal_patch =
                        Read32(memory, kLegacySignalInitCall, legacy_signal_instruction) &&
                        legacy_signal_instruction == kLegacySignalInitBl &&
                        Write32(memory, kLegacySignalInitCall, kArmNop);
                    if (legacy_signal_patch) jit.InvalidateCacheRange(kLegacySignalInitCall, 4);
                    report << "    Unity 4.6 legacy signal-init bypass: "
                           << (legacy_signal_patch ? "APPLIED" : "SKIPPED")
                           << " instruction=0x" << std::hex << legacy_signal_instruction << std::dec << "\n";

                    env.ResetPhaseDiagnostics();
                    env.saw_return = false;
                    env.premature_return = false;
                    env.failed = false;
                    env.fault_pc = 0;
                    env.ticks_left = kTicksPerSlice;
                    jit.ClearHalt();
                    jit.Regs().fill(0);
                    jit.Regs()[0] = kJniEnvObject;
                    jit.Regs()[1] = kJniFakeClassHandle;
                    jit.Regs()[2] = kJniFakeContextHandle;
                    jit.Regs()[13] = kStackTop;
                    jit.Regs()[14] = kReturnStub;
                    jit.Regs()[15] = unity_init_jni & ~1u;
                    jit.SetCpsr((unity_init_jni & 1u) ? 0x20u : 0u);

                    constexpr int kUnityInitJniMaxSlices = 30;
                    int unity_init_jni_slices = 0;
                    std::size_t unity_init_jni_cache_restarts = 0;
                    std::uint32_t unity_init_previous_pc = 0xffffffffu;
                    int unity_init_same_pc_slices = 0;
                    for (; unity_init_jni_slices < kUnityInitJniMaxSlices; ++unity_init_jni_slices) {
                        env.ticks_left = kTicksPerSlice;
                        const auto halt_reason = jit.Run();
                        if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
                        if (env.pending_guest_callback) {
                            env.pending_guest_callback = false;
                            jit.ClearHalt();
                            continue;
                        }
                        if (env.pending_cacheflush) {
                            const std::uint32_t start = env.pending_cacheflush_start;
                            const std::uint32_t size = env.pending_cacheflush_size;
                            env.pending_cacheflush = false;
                            env.pending_cacheflush_start = 0;
                            env.pending_cacheflush_size = 0;
                            jit.ClearHalt();
                            if (size != 0) jit.InvalidateCacheRange(start, size);
                            ++unity_init_jni_cache_restarts;
                            unity_init_previous_pc = 0xffffffffu;
                            unity_init_same_pc_slices = 0;
                            continue;
                        }
                        if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                            ++unity_init_jni_cache_restarts;
                            unity_init_previous_pc = 0xffffffffu;
                            unity_init_same_pc_slices = 0;
                            continue;
                        }
                        const std::uint32_t pc = jit.Regs()[15];
                        if (pc == unity_init_previous_pc) ++unity_init_same_pc_slices;
                        else unity_init_same_pc_slices = 0;
                        unity_init_previous_pc = pc;
                        if (unity_init_same_pc_slices >= 2) break;
                    }

                    report << "    Shared-state Unity initJni execution: slices=" << unity_init_jni_slices
                           << " cache_restarts=" << unity_init_jni_cache_restarts
                           << " returned=" << (env.saw_return ? "YES" : "NO") << "\n";
                    report << "    Shared-state Unity initJni GetJavaVM: "
                           << (env.jni_get_java_vm_seen ? "YES" : "NO") << "\n";
                    report << "    Shared-state Unity initJni GetEnv: "
                           << (env.jni_get_env_seen ? "YES" : "NO") << "\n";
                    report << "    Shared-state Unity initJni NewGlobalRef: "
                           << (env.jni_new_global_ref_seen ? "YES" : "NO") << "\n";
                    report << "    Shared-state Unity initJni GetObjectClass: "
                           << (env.jni_get_object_class_seen ? "YES" : "NO") << "\n";
                    if (!env.jni_static_field_name.empty()) {
                        report << "    Shared-state Unity initJni static field: "
                               << env.jni_static_field_name << " " << env.jni_static_field_signature << "\n";
                    }
                    report << "    Shared-state Unity initJni GetStaticObjectField: "
                           << (env.jni_get_static_object_field_seen ? "YES" : "NO") << "\n";
                    if (env.jni_get_static_int_field_seen) {
                        report << "    Shared-state Unity initJni GetStaticIntField: YES value=0x"
                               << std::hex << env.jni_static_int_field_value << std::dec << "\n";
                    }
                    if (!env.jni_new_string_utf.empty()) {
                        report << "    Shared-state Unity initJni NewStringUTF: " << env.jni_new_string_utf << "\n";
                    }
                    if (!env.jni_method_name.empty()) {
                        report << "    Shared-state Unity initJni GetMethodID: " << env.jni_method_name
                               << " " << env.jni_method_signature << "\n";
                    }
                    if (env.jni_call_object_method_count != 0) {
                        report << "    Shared-state Unity initJni CallObjectMethod count: "
                               << env.jni_call_object_method_count << "\n";
                    }
                    if (env.jni_call_int_method_count != 0) {
                        report << "    Shared-state Unity initJni CallIntMethod count: "
                               << env.jni_call_int_method_count << "\n";
                    }
                    if (env.jni_call_boolean_method_count != 0) {
                        report << "    Shared-state Unity initJni CallBooleanMethod count: "
                               << env.jni_call_boolean_method_count << "\n";
                    }
                    if (env.jni_unknown_slot != 0xffffffffu) {
                        report << "    Shared-state Unity initJni unknown JNI slot: index=" << env.jni_unknown_slot
                               << " offset=0x" << std::hex << (env.jni_unknown_slot * 4u)
                               << " lr=0x" << env.jni_unknown_lr
                               << " caller_lr=0x" << env.jni_unknown_caller_lr
                               << " args=[0x" << env.jni_unknown_r0 << ",0x" << env.jni_unknown_r1
                               << ",0x" << env.jni_unknown_r2 << ",0x" << env.jni_unknown_r3 << "]"
                               << std::dec << "\n";
                    }
                    if (env.jni_null_code_seen) {
                        report << "    Shared-state Unity initJni null-call trap: lr=0x"
                               << std::hex << env.jni_null_code_lr << std::dec << "\n";
                    }
                    if (!env.first_thunk.empty()) {
                        report << "    Shared-state Unity initJni blocked by thunk: " << env.first_thunk
                               << " (SVC " << env.first_thunk_id << ")\n";
                    }
                    if (env.premature_return) {
                        report << "    Shared-state Unity initJni premature return: sp=0x" << std::hex
                               << env.return_sp << " expected=0x" << kStackTop
                               << " r11=0x" << env.return_r11 << " r0=0x" << env.premature_r0
                               << std::dec << "\n";
                        if (env.premature_regs.size() >= 16) {
                            report << "    Shared-state Unity initJni registers:";
                            for (std::size_t i = 0; i < 16; ++i)
                                report << " r" << i << "=0x" << std::hex << env.premature_regs[i] << std::dec;
                            report << "\n";
                        }
                        if (!env.premature_frames.empty()) {
                            report << "    Shared-state Unity initJni frame unwind (fp -> saved lr):";
                            for (const auto& frame : env.premature_frames)
                                report << " 0x" << std::hex << frame.first << "->0x" << frame.second << std::dec;
                            report << "\n";
                        }
                    }
                    if (env.failed) {
                        report << "    Shared-state Unity initJni fault PC: 0x" << std::hex
                               << env.fault_pc << std::dec << "\n";
                    }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                    if (!env.call_trace.empty()) {
                        report << "    Shared-state Unity initJni thunk trace:";
                        for (const auto& name : env.call_trace) report << " " << name;
                        report << "\n";
                    }
#endif

                    const bool unity_init_jni_clean = env.saw_return && !env.failed &&
                                                      !env.premature_return && env.first_thunk.empty();
                    auto run_unity_registered_native = [&](const char* label,
                                                           std::uint32_t function,
                                                           std::uint32_t arg2,
                                                           std::uint32_t arg3,
                                                           std::uint32_t arg4,
                                                           std::uint32_t arg5,
                                                           const std::string* jni_string_arg = nullptr) -> bool {
                        report << "    Shared-state Unity " << label << " lookup: ";
                        if (function == 0) {
                            report << "FAIL (registered native not found)\n";
                            return false;
                        }
                        report << "PASS @ 0x" << std::hex << function << std::dec << "\n";

                        env.ResetPhaseDiagnostics();
                        env.ResetWorkerBoundaryDiagnostics();
                        const bool is_render_call = std::strncmp(label, "nativeRender", 12) == 0;
                        env.allow_main_thread_cooperative_yield = is_render_call;
                        if (jni_string_arg) env.jni_new_string_utf = *jni_string_arg;
                        env.saw_return = false;
                        env.premature_return = false;
                        env.failed = false;
                        env.fault_pc = 0;
                        env.ticks_left = kTicksPerSlice;
                        jit.ClearHalt();
                        jit.Regs().fill(0);
                        jit.Regs()[0] = kJniEnvObject;
                        jit.Regs()[1] = kJniFakeObjectHandle;
                        jit.Regs()[2] = arg2;
                        jit.Regs()[3] = arg3;
                        Write32(memory, kStackTop, arg4);
                        Write32(memory, kStackTop + 4u, arg5);
                        jit.Regs()[13] = kStackTop;
                        jit.Regs()[14] = kReturnStub;
                        jit.Regs()[15] = function & ~1u;
                        jit.SetCpsr((function & 1u) ? 0x20u : 0u);

#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        const int kUnityFollowupMaxSlices = is_render_call ? 8000 : 1000;
#else
                        const int kUnityFollowupMaxSlices = is_render_call ? 2000 : 1000;
#endif
                        constexpr int kUnityPeriodicWorkerPumpSlices = 16;
                        int followup_slices = 0;
                        std::size_t followup_cache_restarts = 0;
                        std::size_t followup_worker_pumps = 0;
                        std::array<std::uint32_t, 8> followup_previous_state{};
                        bool followup_have_previous_state = false;
                        int followup_same_state_slices = 0;
                        auto pump_render_workers = [&]() -> bool {
                            followup_worker_pumps += env.PumpQueuedGuestThreads(32, 4);
                            env.ResetPhaseDiagnostics();
                            env.saw_return = false;
                            env.premature_return = false;
                            env.failed = false;
                            env.fault_pc = 0;
                            env.allow_main_thread_cooperative_yield = is_render_call;
                            jit.ClearHalt();
                            followup_have_previous_state = false;
                            followup_same_state_slices = 0;
                            return true;
                        };
                        for (; followup_slices < kUnityFollowupMaxSlices; ++followup_slices) {
                            env.ticks_left = kTicksPerSlice;
                            const auto halt_reason = jit.Run();
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                            if ((followup_slices & 63) == 63) {
                                const char* last_call = env.call_trace.empty() ? "-" : env.call_trace.back().c_str();
                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                    "Unity %s progress slice=%d pc=0x%08x lr=0x%08x sp=0x%08x last=%s",
                                                    label, followup_slices + 1, jit.Regs()[15], jit.Regs()[14],
                                                    jit.Regs()[13], last_call);
                            }
#endif
                            if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
                            if (env.pending_guest_callback) {
                                env.pending_guest_callback = false;
                                jit.ClearHalt();
                                continue;
                            }
                            if (env.pending_cacheflush) {
                                const std::uint32_t start = env.pending_cacheflush_start;
                                const std::uint32_t size = env.pending_cacheflush_size;
                                env.pending_cacheflush = false;
                                env.pending_cacheflush_start = 0;
                                env.pending_cacheflush_size = 0;
                                jit.ClearHalt();
                                if (size != 0) jit.InvalidateCacheRange(start, size);
                                ++followup_cache_restarts;
                                followup_have_previous_state = false;
                                followup_same_state_slices = 0;
                                continue;
                            }
                            if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                                ++followup_cache_restarts;
                                followup_have_previous_state = false;
                                followup_same_state_slices = 0;
                                continue;
                            }
                            if (env.ConsumeCooperativeYieldRequest()) {
                                if (!pump_render_workers()) break;
                                continue;
                            }
                            if (is_render_call && ((followup_slices + 1) % kUnityPeriodicWorkerPumpSlices) == 0) {
                                if (!pump_render_workers()) break;
                                continue;
                            }
                            const std::array<std::uint32_t, 8> followup_state = {
                                jit.Regs()[15], jit.Regs()[0], jit.Regs()[1], jit.Regs()[2],
                                jit.Regs()[3], jit.Regs()[13], jit.Regs()[14], jit.Cpsr()
                            };
                            if (followup_have_previous_state && followup_state == followup_previous_state)
                                ++followup_same_state_slices;
                            else
                                followup_same_state_slices = 0;
                            followup_previous_state = followup_state;
                            followup_have_previous_state = true;
                            if (followup_same_state_slices >= 8) break;
                        }

                        const std::uint32_t followup_result = env.saw_return ? jit.Regs()[0] : 0u;
                        report << "    Shared-state Unity " << label << " execution: slices=" << followup_slices
                               << " cache_restarts=" << followup_cache_restarts
                               << " worker_pumps=" << followup_worker_pumps
                               << " returned=" << (env.saw_return ? "YES" : "NO")
                               << " result=0x" << std::hex << followup_result << std::dec << "\n";
                        if (!env.saw_return && !env.failed && !env.premature_return && env.first_thunk.empty()) {
                            report << "    Shared-state Unity " << label << " stopped without return: pc=0x"
                                   << std::hex << jit.Regs()[15] << " lr=0x" << jit.Regs()[14]
                                   << " sp=0x" << jit.Regs()[13] << " r0=0x" << jit.Regs()[0]
                                   << " r1=0x" << jit.Regs()[1] << " r2=0x" << jit.Regs()[2]
                                   << " r3=0x" << jit.Regs()[3] << std::dec
                                   << " same_state_slices=" << followup_same_state_slices << "\n";
                        }
                        if (env.pthread_once_callbacks != 0) {
                            report << "    Shared-state Unity " << label << " pthread_once initializers executed: "
                                   << env.pthread_once_callbacks << "\n";
                        }
                        if (env.jni_unknown_slot != 0xffffffffu) {
                            report << "    Shared-state Unity " << label << " unknown JNI slot: index="
                                   << env.jni_unknown_slot << " offset=0x" << std::hex
                                   << (env.jni_unknown_slot * 4u) << " lr=0x" << env.jni_unknown_lr
                                   << " caller_lr=0x" << env.jni_unknown_caller_lr << std::dec << "\n";
                        }
                        if (!env.first_thunk.empty()) {
                            report << "    Shared-state Unity " << label << " blocked by thunk: "
                                   << env.first_thunk << " (SVC " << env.first_thunk_id << ")\n";
                        }
                        if (env.premature_return) {
                            report << "    Shared-state Unity " << label << " premature return: sp=0x"
                                   << std::hex << env.return_sp << " r0=0x" << env.premature_r0
                                   << " r11=0x" << env.return_r11 << std::dec << "\n";
                        }
                        if (env.executable_mprotect_calls != 0) {
                            report << "    Shared-state Unity " << label << " executable mprotect calls: "
                                   << env.executable_mprotect_calls << " first=0x" << std::hex
                                   << env.first_exec_region << " size=0x" << env.first_exec_region_size
                                   << std::dec << "\n";
                        }
                        if (env.last_mprotect_length != 0) {
                            report << "    Shared-state Unity " << label << " last mprotect: addr=0x" << std::hex
                                   << env.last_mprotect_address << " len=0x" << env.last_mprotect_length
                                   << " prot=0x" << env.last_mprotect_prot << " caller_lr=0x"
                                   << env.last_mprotect_caller_lr << std::dec << "\n";
                        }
                        if (env.failed) {
                            report << "    Shared-state Unity " << label << " fault PC: 0x"
                                   << std::hex << env.fault_pc << std::dec
                                   << " exception=" << env.fault_exception << "\n";
                            if (env.fallback_instruction_count != 0) {
                                report << "    Shared-state Unity " << label << " interpreter fallback instructions: "
                                       << env.fallback_instruction_count << "\n";
                            }
                            if (env.invalid_code_address != 0) {
                                report << "    Shared-state Unity " << label << " invalid code fetch: 0x" << std::hex
                                       << env.invalid_code_address << " previous=[0x" << env.code_read_prev1
                                       << ",0x" << env.code_read_prev2 << ",0x" << env.code_read_prev3
                                       << "]" << std::dec << "\n";
                            }
                            if (env.bad_write_address != 0) {
                                report << "    Shared-state Unity " << label << " bad guest write: addr=0x" << std::hex
                                       << env.bad_write_address << " size=0x" << env.bad_write_size
                                       << " pc=0x" << env.bad_write_pc << " lr=0x" << env.bad_write_lr
                                       << " args=[0x" << env.bad_write_r0 << ",0x" << env.bad_write_r1
                                       << ",0x" << env.bad_write_r2 << ",0x" << env.bad_write_r3 << "]"
                                       << std::dec << "\n";
                            }
                            report << "    Shared-state Unity " << label << " fault registers:";
                            for (std::size_t i = 0; i < 16; ++i)
                                report << " r" << i << "=0x" << std::hex << jit.Regs()[i] << std::dec;
                            report << "\n";
                        }
                        if (!env.worker_boundaries.empty()) {
                            report << "    Shared-state Unity " << label << " worker compatibility boundaries: "
                                   << env.worker_boundaries.size() << "\n";
                            for (const auto& boundary : env.worker_boundaries) {
                                report << "      thread=" << boundary.thread_id
                                       << " start=0x" << std::hex << boundary.thread_start << std::dec;
                                if (!boundary.thunk.empty()) report << " thunk=" << boundary.thunk;
                                if (boundary.jni_slot != 0xffffffffu) report << " jni_slot=" << boundary.jni_slot;
                                if (boundary.fault_pc != 0)
                                    report << " fault_pc=0x" << std::hex << boundary.fault_pc << std::dec;
                                report << " pc=0x" << std::hex << boundary.regs[15]
                                       << " lr=0x" << boundary.regs[14]
                                       << " sp=0x" << boundary.regs[13] << std::dec << "\n";
                            }
                        }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        if (!env.guest_thread_pump_trace.empty()) {
                            report << "    Shared-state Unity " << label << " final guest thread states:\n";
                            for (const auto& thread_line : env.guest_thread_pump_trace)
                                report << "      " << thread_line << "\n";
                        }
                        if (!env.file_trace.empty()) {
                            report << "    Shared-state Unity " << label << " file/memory trace:\n";
                            const std::size_t start = env.file_trace.size() > 16 ? env.file_trace.size() - 16 : 0;
                            for (std::size_t i = start; i < env.file_trace.size(); ++i)
                                report << "      " << env.file_trace[i] << "\n";
                        }
                        if (!env.call_trace.empty()) {
                            report << "    Shared-state Unity " << label << " thunk trace:";
                            for (const auto& name : env.call_trace) report << " " << name;
                            report << "\n";
                        }
#endif
                        return env.saw_return && !env.failed && !env.premature_return &&
                               env.first_thunk.empty();
                    };

                    if (unity_init_jni_clean) {
                        const bool native_file_ok = run_unity_registered_native(
                            "nativeFile", unity_native_file, kJniFakeStringHandle, 0, 0, 0,
                            &package_code_path);
                        if (native_file_ok) {
                            const bool init_www_ok = run_unity_registered_native(
                                "nativeInitWWW", unity_native_init_www, kJniFakeClassHandle, 0, 0, 0);
                            if (init_www_ok) {
                            const bool default_display_ok = run_unity_registered_native(
                                "nativeSetDefaultDisplay", unity_native_set_default_display, 0, 0, 0, 0);
                            if (default_display_ok) {
                                const bool recreate_gfx_ok = run_unity_registered_native(
                                    "nativeRecreateGfxState", unity_native_recreate_gfx_state,
                                    kJniFakeObjectHandle, 0, 0, 0);
                                if (recreate_gfx_ok) {
                                    std::uint32_t surface_width = 1;
                                    std::uint32_t surface_height = 1;
#if defined(__ANDROID__)
                                    if (host_native_window) {
                                        const int width = ANativeWindow_getWidth(static_cast<ANativeWindow*>(host_native_window));
                                        const int height = ANativeWindow_getHeight(static_cast<ANativeWindow*>(host_native_window));
                                        if (width > 0) surface_width = static_cast<std::uint32_t>(width);
                                        if (height > 0) surface_height = static_cast<std::uint32_t>(height);
                                    }
#endif
                                    report << "    Shared-state Unity host surface size: "
                                           << surface_width << "x" << surface_height << "\n";
                                    const bool resize_ok = run_unity_registered_native(
                                        "nativeResize", unity_native_resize,
                                        surface_width, surface_height, surface_width, surface_height);
                                    if (resize_ok) {
                                        const bool resume_ok = run_unity_registered_native(
                                            "nativeResume", unity_native_resume, 0, 0, 0, 0);
                                        if (resume_ok) {
                                            std::uint32_t unity_free_got_before_render = 0;
                                            Read32(memory, kUnityBase + 0x00a89000u, unity_free_got_before_render);
                                            report << "    Shared-state Unity free GOT before nativeRender: got=0x"
                                                   << std::hex << unity_free_got_before_render << std::dec << "\n";
#if defined(__ANDROID__)
                                            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                                "Unity free GOT before nativeRender: got=0x%08x",
                                                                unity_free_got_before_render);
#endif
                                            const bool first_render_ok = run_unity_registered_native(
                                                "nativeRender", unity_native_render, 0, 0, 0, 0);
                                            if (first_render_ok) {
                                                const std::size_t pumped_threads =
                                                    env.PumpQueuedGuestThreads(32, 4);
                                                report << "    Shared-state Unity queued worker threads pumped: "
                                                       << pumped_threads << "\n";
                                                for (const auto& thread_line : env.guest_thread_pump_trace)
                                                    report << "      " << thread_line << "\n";
                                                const bool second_render_ok = run_unity_registered_native(
                                                    "nativeRenderFrame2", unity_native_render, 0, 0, 0, 0);
                                                if (second_render_ok) {
                                                    run_unity_registered_native(
                                                        "nativeRenderFrame3", unity_native_render, 0, 0, 0, 0);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        }
    } else {
        report << "    Shared-state Unity JNI: SKIPPED because Mono root-domain validation did not pass\n";
    }

    return report.str();
}
} // namespace

SharedGuestLinkResult RunSharedGuestLinkProbe(const std::string& libMainPath,
                                              const std::string& libUnityPath,
                                              const std::string& libMonoPath,
                                              const std::string& managedDirPath,
                                              const std::string& packageCodePath,
                                              void* hostNativeWindow) {
    std::ostringstream report;
    report << "\nShared ARM32 guest linker probe:\n";
    std::vector<std::uint8_t> memory(kGuestMemorySize, 0);

    std::vector<GuestImage> images = {
        {"libmain.so", libMainPath, kMainBase},
        {"libunity.so", libUnityPath, kUnityBase},
        {"libmono.so", libMonoPath, kMonoBase},
    };

    std::string error;
    for (auto& image : images) {
        if (!LoadAndMapImage(image, memory, error)) {
            report << "  Map " << image.name << ": FAIL (" << error << ")";
            return {false, report.str()};
        }
        report << "  Map " << image.name << ": PASS base=0x" << std::hex << image.base
               << " end=0x" << image.end << std::dec << " (" << image.load_segments << " PT_LOAD)\n";
    }

    std::unordered_map<std::string, ExportSymbol> exports;
    for (const auto& image : images) CollectExports(image, exports);
    report << "  Global guest exports indexed: " << exports.size() << "\n";

    LinkStats total{};
    std::vector<UnresolvedRelocation> unresolved;
    for (const auto& image : images) {
        LinkStats local{};
        if (!ApplyRelocations(image, memory, exports, local, unresolved, error)) {
            report << "  Link " << image.name << ": FAIL (" << error << ")";
            return {false, report.str()};
        }
        total.relative += local.relative;
        total.internal += local.internal;
        total.cross += local.cross;
        total.unresolved += local.unresolved;
        total.unsupported += local.unsupported;
        total.unity_to_mono += local.unity_to_mono;
        for (const auto& s : local.cross_examples) AddExample(total.cross_examples, s);
        for (const auto& s : local.unresolved_examples) AddExample(total.unresolved_examples, s);
        report << "  Link " << image.name << ": PASS"
               << " relative=" << local.relative
               << " internal=" << local.internal
               << " cross=" << local.cross
               << " unresolved-system=" << local.unresolved
               << " unsupported=" << local.unsupported << "\n";
    }

    report << "  Cross-library relocations resolved: " << total.cross << "\n";
    report << "  Unity -> Mono relocations resolved: " << total.unity_to_mono << "\n";
    if (!total.cross_examples.empty()) {
        report << "  Cross-link examples:\n";
        for (const auto& item : total.cross_examples) report << "    " << item << "\n";
    }
    report << "  Remaining Android/system relocations: " << total.unresolved << "\n";
    if (!total.unresolved_examples.empty()) {
        report << "  System-thunk examples:\n";
        for (const auto& item : total.unresolved_examples) report << "    " << item << "\n";
    }

    std::uint32_t root_domain = 0xffffffffu;
    std::uint32_t fault_pc = 0;
    const bool shared_exec_ok = ExecuteSharedMonoProbe(memory, exports, root_domain, fault_pc);
    report << "  Execute Mono from shared image: " << (shared_exec_ok ? "PASS" : "FAIL")
           << " (root domain=0x" << std::hex << root_domain << std::dec << ")";
    if (!shared_exec_ok && fault_pc) report << " fault_pc=0x" << std::hex << fault_pc << std::dec;
    report << "\n";

    report << RunMonoBootstrapTrapProbe(memory, exports, unresolved, managedDirPath, packageCodePath, hostNativeWindow) << "\n";

    const bool cross_ok = total.cross > 0 && total.unity_to_mono > 0;
    const bool ok = cross_ok && shared_exec_ok;
    report << "  Shared three-library address space: " << (ok ? "PASS" : "FAIL") << "\n";
    if (ok) {
        report << "  Result: libmain + libunity + libmono are mapped together and real guest imports resolve across libraries.";
    }
    return {ok, report.str()};
}
