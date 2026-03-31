"""Convert a raw NV12 file (854x480, stride=854) to PNG.
Usage: python nv12_to_png.py path/to/frame_00000.nv12
"""
import sys, struct, zlib, pathlib

W, H = 854, 480  # must match VideoWidth / VideoHeight in CNRRecordingMod

def nv12_to_rgb(data, w, h):
    stride = w  # CaptureStride = VideoWidth
    rgb = bytearray(w * h * 3)
    uv_base = stride * h
    for row in range(h):
        uv_row = row >> 1
        for col in range(w):
            uv_col = col & ~1  # round down to even
            Y = data[row * stride + col]
            U = data[uv_base + uv_row * stride + uv_col]     - 128
            V = data[uv_base + uv_row * stride + uv_col + 1] - 128
            R = int(Y + 1.402  * V)
            G = int(Y - 0.344136 * U - 0.714136 * V)
            B = int(Y + 1.772  * U)
            off = (row * w + col) * 3
            rgb[off]     = max(0, min(255, R))
            rgb[off + 1] = max(0, min(255, G))
            rgb[off + 2] = max(0, min(255, B))
    return bytes(rgb)

def write_png(path, rgb, w, h):
    def chunk(tag, data):
        c = struct.pack('>I', len(data)) + tag + data
        return c + struct.pack('>I', zlib.crc32(tag + data) & 0xFFFFFFFF)

    raw_rows = b''
    for row in range(h):
        raw_rows += b'\x00' + rgb[row*w*3:(row+1)*w*3]

    ihdr = struct.pack('>IIBBBBB', w, h, 8, 2, 0, 0, 0)  # 8-bit RGB
    png  = b'\x89PNG\r\n\x1a\n'
    png += chunk(b'IHDR', ihdr)
    png += chunk(b'IDAT', zlib.compress(raw_rows, 6))
    png += chunk(b'IEND', b'')
    path.write_bytes(png)

if __name__ == '__main__':
    if len(sys.argv) < 2:
        print(f"Usage: python nv12_to_png.py frame_00000.nv12 [...]")
        sys.exit(1)

    for arg in sys.argv[1:]:
        src = pathlib.Path(arg)
        data = src.read_bytes()
        expected = W * H * 3 // 2
        if len(data) != expected:
            print(f"WARN: {src.name}: expected {expected} bytes, got {len(data)} — check W/H constants")
        rgb = nv12_to_rgb(data, W, H)
        dst = src.with_suffix('.png')
        write_png(dst, rgb, W, H)
        print(f"  {src.name}  ->  {dst.name}")
