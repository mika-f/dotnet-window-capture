using System;
using System.Runtime.InteropServices;

using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;

using WinRT.GraphicsCapture.Interop;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;

namespace WinRT.GraphicsCapture
{
    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            using var form = new RenderForm(".NET Window Capture Samples - WinRT.GraphicsCapture");

            // create a Device and SwapChain
            var swapChaiDescription = new SwapChainDescription
            {
                BufferCount = 2,
                Flags = SwapChainFlags.None,
                IsWindowed = true,
                ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, swapChaiDescription, out var device, out var swapChain);

            // ignore all Windows events
            using var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            using var vertexShaderByteCode = ShaderBytecode.CompileFromFile("./Shader.fx", "VS", "vs_5_0");
            using var vertexShader = new VertexShader(device, vertexShaderByteCode);

            using var pixelShaderByteCode = ShaderBytecode.CompileFromFile("./Shader.fx", "PS", "ps_5_0");
            using var pixelShader = new PixelShader(device, pixelShaderByteCode);

            using var layout = new InputLayout(device, vertexShaderByteCode, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
            });

            using var vertexes = Buffer.Create(device, BindFlags.VertexBuffer, new[]
            {
                new Vertex { Position = new RawVector3(-1.0f, 1.0f, 0.5f), TexCoord = new RawVector2(0.0f, 0.0f) },
                new Vertex { Position = new RawVector3(1.0f, 1.0f, 0.5f), TexCoord = new RawVector2(1.0f, 0.0f) },
                new Vertex { Position = new RawVector3(-1.0f, -1.0f, 0.5f), TexCoord = new RawVector2(0.0f, 1.0f) },
                new Vertex { Position = new RawVector3(1.0f, -1.0f, 0.5f), TexCoord = new RawVector2(1.0f, 1.0f) }
            });

            var samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear
            };

            device.ImmediateContext.InputAssembler.InputLayout = layout;
            device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexes, Utilities.SizeOf<Vertex>(), 0));
            device.ImmediateContext.VertexShader.Set(vertexShader);
            device.ImmediateContext.PixelShader.SetSampler(0, new SamplerState(device, samplerStateDescription));
            device.ImmediateContext.PixelShader.Set(pixelShader);

            // create a first views
            var backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);
            var renderView = new RenderTargetView(device, backBuffer);

            device.ImmediateContext.Rasterizer.SetViewport(0, 0, form.ClientSize.Width, form.ClientSize.Height);
            device.ImmediateContext.OutputMerger.SetTargets(renderView);

            // listen events (but processed in render loop)
            using var graphicsCapture = new GraphicsCapture();
            var isResized = false;
            form.UserResized += (_, __) => isResized = true;

            RenderLoop.Run(form, () =>
            {
                // ReSharper disable AccessToDisposedClosure
                if (!graphicsCapture.IsCapturing)
                    graphicsCapture.StartCapture(form.Handle, device);

                if (isResized)
                {
                    Utilities.Dispose(ref backBuffer);
                    Utilities.Dispose(ref renderView);

                    swapChain.ResizeBuffers(swapChaiDescription.BufferCount, form.ClientSize.Width, form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);
                    backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);
                    renderView = new RenderTargetView(device, backBuffer);

                    device.ImmediateContext.Rasterizer.SetViewport(0, 0, form.ClientSize.Width, form.ClientSize.Height);
                    device.ImmediateContext.OutputMerger.SetTargets(renderView);

                    isResized = true;
                }

                // clear view
                device.ImmediateContext.ClearRenderTargetView(renderView, new RawColor4(1.0f, 1.0f, 1.0f, 1.0f));

                using var texture2d = graphicsCapture.TryGetNextFrameAsTexture2D(device);
                if (texture2d != null)
                {
                    using var shaderResourceView = new ShaderResourceView(device, texture2d);
                    device.ImmediateContext.PixelShader.SetShaderResource(0, shaderResourceView);
                }

                // draw it
                device.ImmediateContext.Draw(4, 0);
                swapChain.Present(0, PresentFlags.None);

                // ReSharper restore AccessToDisposedClosure
            });

            renderView.Dispose();
            backBuffer.Dispose();
            swapChain.Dispose();
            device.Dispose();
        }
    }

    internal struct Vertex
    {
        public RawVector3 Position;
        public RawVector2 TexCoord;
    }

    internal class GraphicsCapture : IDisposable
    {
        private Direct3D11CaptureFramePool _captureFramePool;
        private GraphicsCaptureItem _captureItem;
        private GraphicsCaptureSession _captureSession;

        public bool IsCapturing { get; private set; }

        public GraphicsCapture()
        {
            IsCapturing = false;
        }

        public void Dispose()
        {
            StopCapture();
        }

        public void StartCapture(IntPtr hWnd, Device device)
        {
            var capturePicker = new GraphicsCapturePicker();

            // ReSharper disable once PossibleInvalidCastException
            // ReSharper disable once SuspiciousTypeConversion.Global
            var initializer = (IInitializeWithWindow) (object) capturePicker;
            initializer.Initialize(hWnd);

            _captureItem = capturePicker.PickSingleItemAsync().AsTask().Result;
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

        private void CaptureItemOnClosed(GraphicsCaptureItem sender, object args)
        {
            StopCapture();
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
    }
}