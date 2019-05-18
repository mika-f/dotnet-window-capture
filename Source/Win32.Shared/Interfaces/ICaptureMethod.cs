using System;

using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;

namespace Win32.Shared.Interfaces
{
    public interface ICaptureMethod : IDisposable
    {
        bool IsCapturing { get; }

        void StartCapture(IntPtr hWnd, Device device, Factory factory);

        Texture2D TryGetNextFrameAsTexture2D(Device device);

        void StopCapture();
    }
}