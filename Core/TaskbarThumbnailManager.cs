using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace YoutubeMusicDesktop.Core;

public sealed partial class TaskbarThumbnailManager : IDisposable
{
    private const int WM_DWMSENDICONICTHUMBNAIL = 0x0323;
    private const int WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326;
    private const int DWMWA_FORCE_ICONIC_REPRESENTATION = 7;
    private const int DWMWA_HAS_ICONIC_BITMAP = 10;

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
    
    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetIconicThumbnail(IntPtr hwnd, IntPtr hbitmap, int flags);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetIconicLivePreviewBitmap(IntPtr hwnd, IntPtr hbitmap, IntPtr ptClient, int flags);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmInvalidateIconicBitmaps(IntPtr hwnd);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(IntPtr hObject);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetClientRect(IntPtr hwnd, out RECT rect);
    
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    private readonly IntPtr _hwnd;

    private readonly HwndSource _source;

    private Bitmap? _albumArt;

    public TaskbarThumbnailManager(Window window)
    {
        _source = (HwndSource)PresentationSource.FromVisual(window)!;
        _hwnd = _source.Handle;
        _source.AddHook(WndProc);

        var trueVal = 1;
        DwmSetWindowAttribute(_hwnd, DWMWA_FORCE_ICONIC_REPRESENTATION, ref trueVal, sizeof(int));
        DwmSetWindowAttribute(_hwnd, DWMWA_HAS_ICONIC_BITMAP, ref trueVal, sizeof(int));
    }

    public void SetAlbumArt(Bitmap art)
    {
        _albumArt?.Dispose();
        _albumArt = art;
#pragma warning disable CA1806
        DwmInvalidateIconicBitmaps(_hwnd);
#pragma warning restore CA1806
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_DWMSENDICONICTHUMBNAIL:
            {
                var width = (int)((lParam.ToInt64() >> 16) & 0xFFFF);
                var height = (int)(lParam.ToInt64() & 0xFFFF);

                if (_albumArt != null && width > 0 && height > 0)
                {
                    var scale = Math.Min((float)width / _albumArt.Width, (float)height / _albumArt.Height);
                    var w = Math.Max(1, (int)(_albumArt.Width * scale));
                    var h = Math.Max(1, (int)(_albumArt.Height * scale));

                    var hBmp = CreateDibFromBitmap(_albumArt, w, h);
                    if (hBmp != IntPtr.Zero)
                    {
                        DwmSetIconicThumbnail(_hwnd, hBmp, 0);
                        DeleteObject(hBmp);
                    }
                }

                handled = true;
                break;
            }
            case WM_DWMSENDICONICLIVEPREVIEWBITMAP:
            {
                GetClientRect(_hwnd, out var rect);
                using var preview = RenderCropped(rect.Right - rect.Left, rect.Bottom - rect.Top);
                if (preview != null)
                {
                    var hBmp = preview.GetHbitmap();
                    DwmSetIconicLivePreviewBitmap(_hwnd, hBmp, IntPtr.Zero, 0);
                    DeleteObject(hBmp);
                }

                handled = true;
                break;
            }
        }

        return IntPtr.Zero;
    }

    private Bitmap? RenderCropped(int targetW, int targetH)
    {
        if (_albumArt is null || targetW <= 0 || targetH <= 0)
            return null;

        var canvas = new Bitmap(targetW, targetH, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(canvas);

        var scale = Math.Max((float)targetW / _albumArt.Width, (float)targetH / _albumArt.Height);
        var w = (int)(_albumArt.Width * scale);
        var h = (int)(_albumArt.Height * scale);
        g.DrawImage(_albumArt, (targetW - w) / 2, (targetH - h) / 2, w, h);

        return canvas;
    }
    
    // ---
    

// dentro da classe TaskbarThumbnailManager

[StructLayout(LayoutKind.Sequential)]
private partial struct BITMAPINFOHEADER
{
    public uint biSize;
    public int biWidth;
    public int biHeight;
    public ushort biPlanes;
    public ushort biBitCount;
    public uint biCompression;
    public uint biSizeImage;
    public int biXPelsPerMeter;
    public int biYPelsPerMeter;
    public uint biClrUsed;
    public uint biClrImportant;
}

[LibraryImport("gdi32.dll", SetLastError = true)]
private static partial IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFOHEADER bmi, uint usage, out IntPtr ppvBits, IntPtr hSection, uint offset);

// gera um HBITMAP 32bpp manualmente, sem passar por GetHbitmap()
private IntPtr CreateDibFromBitmap(Bitmap src, int w, int h)
{
    var header = new BITMAPINFOHEADER
    {
        biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
        biWidth = w,
        biHeight = -h, // negativo = top-down, bate com a ordem de linhas do LockBits
        biPlanes = 1,
        biBitCount = 32,
        biCompression = 0, // BI_RGB
    };

    var hBitmap = CreateDIBSection(IntPtr.Zero, ref header, 0, out var ppvBits, IntPtr.Zero, 0);
    if (hBitmap == IntPtr.Zero || ppvBits == IntPtr.Zero)
        return IntPtr.Zero;

    // resample a album art pro tamanho w x h usando GDI+ (só pra qualidade de escala)
    using var scaled = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(scaled))
    {
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
        g.DrawImage(src, 0, 0, w, h);
    }

    var rect = new Rectangle(0, 0, w, h);
    var data = scaled.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    try
    {
        // Format32bppArgb do GDI+ e DIB 32bpp BI_RGB usam o mesmo layout de bytes (BGRA), copy direto
        var byteCount = data.Stride * h;
        unsafe
        {
            Buffer.MemoryCopy((void*)data.Scan0, (void*)ppvBits, byteCount, byteCount);
        }
    }
    finally
    {
        scaled.UnlockBits(data);
    }

    return hBitmap;
}
    
    public void Dispose()
    {
        _source.RemoveHook(WndProc);
        _albumArt?.Dispose();
    }
}