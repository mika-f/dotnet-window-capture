using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Win32.Shared;
using Win32.Shared.Interfaces;

using WinRT.GraphicsCapture.Interop;

using Device = SharpDX.Direct3D11.Device;

namespace WinRT.GraphicsCapture
{
    internal class GraphicsCapture : ICaptureMethod
    {
        private static readonly Guid _graphicsCaptureItemIid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");
        private Direct3D11CaptureFramePool _captureFramePool;
        private GraphicsCaptureItem _captureItem;
        private GraphicsCaptureSession _captureSession;

        public GraphicsCapture()
        {
            IsCapturing = false;
        }

        public bool IsCapturing { get; private set; }

        public void Dispose()
        {
            StopCapture();
        }

        public void StartCapture(IntPtr hWnd, Device device, Factory factory)
        {
            #region GraphicsCapturePicker version

            /*
            var capturePicker = new GraphicsCapturePicker();

            // ReSharper disable once PossibleInvalidCastException
            // ReSharper disable once SuspiciousTypeConversion.Global
            var initializer = (IInitializeWithWindow)(object)capturePicker;
            initializer.Initialize(hWnd);

            _captureItem = capturePicker.PickSingleItemAsync().AsTask().Result;
            */

            #endregion

            #region Window Handle version

            var capturePicker = new WindowPicker();
            var captureHandle = capturePicker.PickCaptureTarget(hWnd);
            if (captureHandle == IntPtr.Zero)
                return;

            _captureItem = CreateItemForWindow(captureHandle);

            #endregion

            if (_captureItem == null)
                return;

            _captureItem.Closed += CaptureItemOnClosed;

            var hr = NativeMethods.CreateDirect3D11DeviceFromDXGIDevice(device.NativePointer, out var pUnknown);
            if (hr != 0)
            {
                StopCapture();
                return;
            }

            var winrtDevice = (IDirect3DDevice) Marshal.GetObjectForIUnknown(pUnknown);
            Marshal.Release(pUnknown);

            _captureFramePool = Direct3D11CaptureFramePool.Create(winrtDevice, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, _captureItem.Size);
            _captureSession = _captureFramePool.CreateCaptureSession(_captureItem);
            _captureSession.StartCapture();
            IsCapturing = true;
        }

        public Texture2D TryGetNextFrameAsTexture2D(Device device)
        {
            using var frame = _captureFramePool?.TryGetNextFrame();
            if (frame == null)
                return null;

            // ReSharper disable once SuspiciousTypeConversion.Global
            var surfaceDxgiInterfaceAccess = (IDirect3DDxgiInterfaceAccess) frame.Surface;
            var pResource = surfaceDxgiInterfaceAccess.GetInterface(new Guid("dc8e63f3-d12b-4952-b47b-5e45026a862d"));

            using var surfaceTexture = new Texture2D(pResource); // shared resource
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

        public void StopCapture() // ...or release resources
        {
            _captureSession?.Dispose();
            _captureFramePool?.Dispose();
            _captureSession = null;
            _captureFramePool = null;
            _captureItem = null;
            IsCapturing = false;
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        private static GraphicsCaptureItem CreateItemForWindow(IntPtr hWnd)
        {
            var factory = WindowsRuntimeMarshal.GetActivationFactory(typeof(GraphicsCaptureItem));
            var interop = (IGraphicsCaptureItemInterop) factory;
            var pointer = interop.CreateForWindow(hWnd, typeof(GraphicsCaptureItem).GetInterface("IGraphicsCaptureItem").GUID);
            var capture = Marshal.GetObjectForIUnknown(pointer) as GraphicsCaptureItem;
            Marshal.Release(pointer);

            return capture;
        }

        private void CaptureItemOnClosed(GraphicsCaptureItem sender, object args)
        {
            StopCapture();
        }
    }
}