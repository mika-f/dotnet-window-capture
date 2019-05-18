using System;

using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Win32.Shared.Interfaces;

using Device = SharpDX.Direct3D11.Device;

namespace Win32.DesktopDuplication
{
    internal class DesktopDuplication : ICaptureMethod
    {
        private OutputDuplication _duplication;

        public void Dispose()
        {
            StopCapture();
        }

        public bool IsCapturing { get; private set; }

        public void StartCapture(IntPtr hWnd, Device device, Factory factory)
        {
            using var factory1 = factory.QueryInterface<Factory1>();
            using var adapter = factory1.GetAdapter1(0);
            using var output1 = adapter.GetOutput(0).QueryInterface<Output1>();

            _duplication = output1.DuplicateOutput(device);
            IsCapturing = true;
        }

        public Texture2D TryGetNextFrameAsTexture2D(Device device)
        {
            if (_duplication == null)
                return null;

            var hr = _duplication.TryAcquireNextFrame(100, out _, out var desktopResourceOut);
            if (hr.Failure)
                return null;

            using var desktopTexture = desktopResourceOut.QueryInterface<Texture2D>();
            var texture2dDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = desktopTexture.Description.Height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Width = desktopTexture.Description.Width
            };
            var texture2d = new Texture2D(device, texture2dDescription);
            device.ImmediateContext.CopyResource(desktopTexture, texture2d);

            // release resources
            desktopResourceOut.Dispose();
            _duplication.ReleaseFrame();

            return texture2d;
        }

        public void StopCapture()
        {
            _duplication?.Dispose();
            IsCapturing = false;
        }
    }
}