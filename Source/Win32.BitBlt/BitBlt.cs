using System;
using System.Drawing;
using System.Drawing.Imaging;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Win32.BitBlt.Interop;
using Win32.Shared;
using Win32.Shared.Interfaces;

using Device = SharpDX.Direct3D11.Device;
using Rectangle = System.Drawing.Rectangle;

namespace Win32.BitBlt
{
    internal class BitBlt : ICaptureMethod
    {
        private IntPtr _hWnd;

        public void Dispose()
        {
            // Nothing to do
        }

        public bool IsCapturing { get; private set; }

        public void StartCapture(IntPtr hWnd, Device device, Factory factory)
        {
            var picker = new WindowPicker();
            _hWnd = picker.PickCaptureTarget(hWnd);
            if (_hWnd == IntPtr.Zero)
                return;

            IsCapturing = true;
        }

        public Texture2D TryGetNextFrameAsTexture2D(Device device)
        {
            if (_hWnd == IntPtr.Zero)
                return null;

            var hdcSrc = NativeMethods.GetDCEx(_hWnd, IntPtr.Zero, DeviceContextValues.Window | DeviceContextValues.Cache | DeviceContextValues.LockWindowUpdate);
            var hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
            NativeMethods.GetWindowRect(_hWnd, out var rect);
            var (width, height) = (rect.Right - rect.Left, rect.Bottom - rect.Top);
            var hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, width, height);
            var hOld = NativeMethods.SelectObject(hdcDest, hBitmap);
            NativeMethods.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, TernaryRasterOperations.SRCCOPY);
            NativeMethods.SelectObject(hdcDest, hOld);
            NativeMethods.DeleteDC(hdcDest);
            NativeMethods.ReleaseDC(_hWnd, hdcSrc);

            using var img = Image.FromHbitmap(hBitmap);
            NativeMethods.DeleteObject(hBitmap);

            using var bitmap = img.Clone(Rectangle.FromLTRB(0, 0, width, height), PixelFormat.Format32bppArgb);
            var bits = bitmap.LockBits(Rectangle.FromLTRB(0, 0, width, height), ImageLockMode.ReadOnly, img.PixelFormat);

            var data = new DataBox { DataPointer = bits.Scan0, RowPitch = bits.Width * 4, SlicePitch = bits.Height };

            var texture2dDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Width = width
            };
            var texture2d = new Texture2D(device, texture2dDescription, new[] { data });
            bitmap.UnlockBits(bits);

            return texture2d;
        }

        public void StopCapture()
        {
            _hWnd = IntPtr.Zero;
        }
    }
}