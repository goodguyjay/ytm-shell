using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

// ReSharper disable InconsistentNaming

namespace YoutubeMusicDesktop.Core;

public sealed partial class TaskbarThumbnailManager : IDisposable
{
    private const int WM_DWMSENDICONICTHUMBNAIL = 0x0323;
    private const int WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326;
    private const int DWMWA_FORCE_ICONIC_REPRESENTATION = 7;
    private const int DWMWA_HAS_ICONIC_BITMAP = 10;

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attr,
        ref int value,
        int size
    );

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetIconicThumbnail(IntPtr hwnd, IntPtr hbitmap, int flags);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetIconicLivePreviewBitmap(
        IntPtr hwnd,
        IntPtr hbitmap,
        IntPtr ptClient,
        int flags
    );

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
        public int Left,
            Top,
            Right,
            Bottom;
    }

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
    private static partial IntPtr CreateDIBSection(
        IntPtr hdc,
        ref BITMAPINFOHEADER bmi,
        uint usage,
        out IntPtr ppvBits,
        IntPtr hSection,
        uint offset
    );

    private readonly IntPtr _hwnd;

    private readonly HwndSource _source;

    private Bitmap _albumArt;

    public TaskbarThumbnailManager(Window window)
    {
        _source = (HwndSource)PresentationSource.FromVisual(window)!;
        _hwnd = _source.Handle;
        _source.AddHook(WndProc);

        _albumArt = LoadFallbackArt();

        var trueVal = 1;
        DwmSetWindowAttribute(_hwnd, DWMWA_FORCE_ICONIC_REPRESENTATION, ref trueVal, sizeof(int));
        DwmSetWindowAttribute(_hwnd, DWMWA_HAS_ICONIC_BITMAP, ref trueVal, sizeof(int));
    }

    private static Bitmap LoadFallbackArt()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "PNGs", "logo.png");
        return new Bitmap(path);
    }

    public void SetAlbumArt(Bitmap art)
    {
        _albumArt.Dispose();
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

                if (width > 0 && height > 0)
                {
                    var scale = Math.Min(
                        (float)width / _albumArt.Width,
                        (float)height / _albumArt.Height
                    );
                    var w = Math.Max(1, (int)(_albumArt.Width * scale));
                    var h = Math.Max(1, (int)(_albumArt.Height * scale));

                    var hBmp = CreateDibLetterboxed(_albumArt, w, h);
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
        if (targetW <= 0 || targetH <= 0)
            return null;

        var canvas = new Bitmap(targetW, targetH, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(canvas);

        var scale = Math.Max((float)targetW / _albumArt.Width, (float)targetH / _albumArt.Height);
        var w = (int)(_albumArt.Width * scale);
        var h = (int)(_albumArt.Height * scale);
        g.DrawImage(_albumArt, (targetW - w) / 2, (targetH - h) / 2, w, h);

        return canvas;
    }

    private IntPtr CreateDibLetterboxed(Bitmap src, int targetW, int targetH)
    {
        var header = new BITMAPINFOHEADER
        {
            biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
            biWidth = targetW,
            biHeight = -targetH,
            biPlanes = 1,
            biBitCount = 32,
            biCompression = 0,
        };

        var hBitmap = CreateDIBSection(IntPtr.Zero, ref header, 0, out var ppvBits, IntPtr.Zero, 0);
        if (hBitmap == IntPtr.Zero || ppvBits == IntPtr.Zero)
        {
            var err = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.WriteLine(
                $"CreateDibSection failed with error code {err}, w={targetW}, h={targetH}"
            );
            return IntPtr.Zero;
        }

        var stride = targetW * 4;
        var totalBytes = stride * targetH;

        unsafe
        {
            new Span<byte>((void*)ppvBits, totalBytes).Clear();
        }

        var scale = Math.Min((float)targetW / src.Width, (float)targetH / src.Height);
        var w = Math.Max(1, (int)(src.Width * scale));
        var h = Math.Max(1, (int)(src.Height * scale));
        var offsetX = (targetW - w) / 2;
        var offsetY = (targetH - h) / 2;

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
            unsafe
            {
                var srcPtr = (byte*)data.Scan0;
                var dstBase = (byte*)ppvBits;
                for (var row = 0; row < h; row++)
                {
                    var srcRow = srcPtr + row * data.Stride;
                    var dstRow = dstBase + (offsetY + row) * stride + offsetX * 4;
                    Buffer.MemoryCopy(srcRow, dstRow, w * 4, w * 4);
                }
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
        _albumArt.Dispose();
    }
}
