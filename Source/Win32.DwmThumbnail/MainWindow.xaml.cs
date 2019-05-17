using System;
using System.Windows;
using System.Windows.Interop;

using Win32.DwmThumbnail.Interop;
using Win32.Shared;

namespace Win32.DwmThumbnail
{
    /// <summary>
    ///     MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr _hThumbnail = IntPtr.Zero;
        private IntPtr _hWnd = IntPtr.Zero;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            do
            {
                _hWnd = new WindowPicker().PickCaptureTarget(new WindowInteropHelper(this).Handle);
            } while (_hWnd == IntPtr.Zero);

            var hr = NativeMethods.DwmRegisterThumbnail(new WindowInteropHelper(this).Handle, _hWnd, out _hThumbnail);
            if (hr != 0)
                return;

            UpdateThumbnailProperties();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_hThumbnail == IntPtr.Zero)
                return;

            UpdateThumbnailProperties();
        }

        private void UpdateThumbnailProperties()
        {
            var dpi = GetDpiScaleFactor();
            var props = new DWM_THUMBNAIL_PROPERTIES
            {
                fVisible = true,
                dwFlags = (int) (DWM_TNP.DWM_TNP_VISIBLE | DWM_TNP.DWM_TNP_OPACITY | DWM_TNP.DWM_TNP_RECTDESTINATION | DWM_TNP.DWM_TNP_SOURCECLIENTAREAONLY),
                opacity = 255,
                rcDestination = new RECT { left = 0, top = 0, bottom = (int) (Grid.ActualHeight * dpi.Y), right = (int) (Grid.ActualWidth * dpi.X) },
                fSourceClientAreaOnly = true
            };

            NativeMethods.DwmUpdateThumbnailProperties(_hThumbnail, ref props);
        }

        private Point GetDpiScaleFactor()
        {
            var source = PresentationSource.FromVisual(this);
            return source?.CompositionTarget != null ? new Point(source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22) : new Point(1.0d, 1.0d);
        }
    }
}