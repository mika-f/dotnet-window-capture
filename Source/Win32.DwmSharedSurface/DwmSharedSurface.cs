using System;

using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Win32.DwmSharedSurface.Interop;
using Win32.Shared;
using Win32.Shared.Interfaces;

using Device = SharpDX.Direct3D11.Device;

namespace Win32.DwmSharedSurface
{
    internal class DwmSharedSurface : ICaptureMethod
    {
        private IntPtr _hWnd;

        public DwmSharedSurface()
        {
            IsCapturing = false;
        }

        public void Dispose()
        {
            StopCapture();
        }

        public bool IsCapturing { get; private set; }

        public void StartCapture(IntPtr hWnd, Device device)
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

            NativeMethods.DwmGetDxSharedSurface(_hWnd, out var phSurface, out _, out _, out _, out _);
            if (phSurface == IntPtr.Zero)
            {
                // Window Lost
                StopCapture();
                return null;
            }

            using var surfaceTexture = device.OpenSharedResource<Texture2D>(phSurface);

            // using var surfaceTexture = new Texture2D(phSurface);
            var texture2dDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = surfaceTexture.Description.Height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Width = surfaceTexture.Description.Width
            };
            var texture2d = new Texture2D(device, texture2dDescription);
            device.ImmediateContext.CopyResource(surfaceTexture, texture2d);

            return texture2d;
        }

        public void StopCapture()
        {
            _hWnd = IntPtr.Zero;
            IsCapturing = false;
        }
    }
}