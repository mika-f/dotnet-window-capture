using System;

using SharpDX.Direct3D11;

namespace Win32.Shared.Interfaces
{
    public interface ICaptureMethod : IDisposable
    {
        bool IsCapturing { get; }

        void StartCapture(IntPtr hWnd, Device device);

        Texture2D TryGetNextFrameAsTexture2D(Device device);

        void StopCapture();
    }
}