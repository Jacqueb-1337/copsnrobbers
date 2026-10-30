#include "../../../compat_modules/cocos2dx/cocos2dx_jni_marshaller.h"
#include "../../../compat_modules/cocos2dx/cocos2dx_bitmap_pixels.h"
#include <algorithm>
#include <cstdint>
#include <cstring>
#include <string>
#include <vector>

#if defined(_WIN32)
#define NOMINMAX
#include <windows.h>
#endif

namespace pv7::compat::cocos2dx {

// Windows-only diagnostic backend. The Android host uses Android Paint/Canvas.
// This draws actual text into a GDI DIB, not placeholder/synthetic pixels.
bool RenderProbeBitmap(const BitmapTextRequest& request,
                       BitmapPixels& output, std::string& error) {
#if !defined(_WIN32)
    error="probe text rendering requires Windows; Android uses its own renderer";
    return false;
#else
    const auto toWide=[](const std::string& s)->std::wstring {
        if (s.empty()) return {};
        int n=MultiByteToWideChar(CP_UTF8,0,s.data(),static_cast<int>(s.size()),nullptr,0);
        if(n<=0) return {};
        std::wstring w(static_cast<std::size_t>(n),L'\0');
        MultiByteToWideChar(CP_UTF8,0,s.data(),static_cast<int>(s.size()),w.data(),n);
        return w;
    };
    const auto text=toWide(request.text), face=toWide(request.font);
    if(text.empty() && !request.text.empty()) {
        error="text is not valid UTF-8"; return false;
    }
    HDC dc=CreateCompatibleDC(nullptr);
    if(!dc) {error="CreateCompatibleDC failed";return false;}
    HFONT font=CreateFontW(-request.font_size,0,0,0,FW_NORMAL,FALSE,FALSE,FALSE,
                           DEFAULT_CHARSET,OUT_DEFAULT_PRECIS,CLIP_DEFAULT_PRECIS,
                           ANTIALIASED_QUALITY,DEFAULT_PITCH,
                           face.empty()?L"Arial":face.c_str());
    if(!font) {DeleteDC(dc); error="CreateFontW failed";return false;}
    HGDIOBJ oldFont=SelectObject(dc,font);
    TEXTMETRICW metrics{};
    SIZE extent{};
    bool ok=GetTextMetricsW(dc,&metrics)!=0 &&
            GetTextExtentPoint32W(dc,text.c_str(),static_cast<int>(text.size()),&extent)!=0;
    if(!ok) {
        SelectObject(dc,oldFont);DeleteObject(font);DeleteDC(dc);
        error="Windows text metrics failed";return false;
    }
    const int width=request.width>0?request.width:std::max(1L,extent.cx);
    const int height=request.height>0?request.height:std::max(1L,metrics.tmHeight);
    if(width<=0||height<=0||static_cast<std::uint64_t>(width)*height*4u>64ull*1024*1024) {
        SelectObject(dc,oldFont);DeleteObject(font);DeleteDC(dc);
        error="invalid diagnostic bitmap size";return false;
    }
    BITMAPINFO info{};
    info.bmiHeader.biSize=sizeof(BITMAPINFOHEADER);
    info.bmiHeader.biWidth=width;
    info.bmiHeader.biHeight=-height;
    info.bmiHeader.biPlanes=1;
    info.bmiHeader.biBitCount=32;
    info.bmiHeader.biCompression=BI_RGB;
    void* raw=nullptr;
    HBITMAP dib=CreateDIBSection(dc,&info,DIB_RGB_COLORS,&raw,nullptr,0);
    if(!dib||!raw) {
        SelectObject(dc,oldFont);DeleteObject(font);DeleteDC(dc);
        error="CreateDIBSection failed";return false;
    }
    HGDIOBJ oldBmp=SelectObject(dc,dib);
    const std::size_t bytes=static_cast<std::size_t>(width)*height*4;
    std::memset(raw,0,bytes);
    SetBkMode(dc,TRANSPARENT);
    SetTextColor(dc,RGB(255,255,255));
    int x=0;
    const int align=request.alignment & 0x0f;
    if(align==2) x=width-extent.cx;
    else if(align==3) x=(width-extent.cx)/2;
    int y=0;
    const int vertical=(request.alignment>>4)&0x0f;
    if(vertical==2) y=height-metrics.tmHeight;
    else if(vertical==3) y=(height-metrics.tmHeight)/2;
    ok=TextOutW(dc,x,y,text.c_str(),static_cast<int>(text.size()))!=0;
    std::vector<std::uint8_t> pixels(bytes);
    if(ok) {
        const auto* bgra=static_cast<const std::uint8_t*>(raw);
        for(std::size_t i=0;i<bytes;i+=4) {
            const auto coverage=std::max({bgra[i],bgra[i+1],bgra[i+2]});
            // Premultiplied white: uniform channels remain correct after the
            // historical Cocos shift/rotate alpha conversion.
            pixels[i]=pixels[i+1]=pixels[i+2]=pixels[i+3]=coverage;
        }
    }
    SelectObject(dc,oldBmp);
    DeleteObject(dib);
    SelectObject(dc,oldFont);
    DeleteObject(font);
    DeleteDC(dc);
    if(!ok) {error="TextOutW failed";return false;}
    return ReadBitmapPixels(width,height,pixels,output,error);
#endif
}
}