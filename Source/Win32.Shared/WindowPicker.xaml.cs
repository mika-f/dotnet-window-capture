using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using Win32.Shared.Interop;

namespace Win32.Shared
{
    /// <summary>
    ///     MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowPicker : Window
    {
        private readonly string[] _ignoreProcesses = { "applicationframehost", "shellexperiencehost", "systemsettings", "winstore.app", "searchui" };

        public WindowPicker()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FindWindows();
        }

        public IntPtr PickCaptureTarget(IntPtr hWnd)
        {
            new WindowInteropHelper(this).Owner = hWnd;
            ShowDialog();

            return ((CapturableWindow?) Windows.SelectedItem)?.Handle ?? IntPtr.Zero;
        }

        private void FindWindows()
        {
            var wih = new WindowInteropHelper(this);
            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                // ignore invisible windows
                if (!NativeMethods.IsWindowVisible(hWnd))
                    return true;

                // ignore untitled windows
                var title = new StringBuilder(1024);
                NativeMethods.GetWindowText(hWnd, title, title.Capacity);
                if (string.IsNullOrWhiteSpace(title.ToString()))
                    return true;

                // ignore me
                if (wih.Handle == hWnd)
                    return true;

                NativeMethods.GetWindowThreadProcessId(hWnd, out var processId);

                // ignore by process name
                var process = Process.GetProcessById((int) processId);
                if (_ignoreProcesses.Contains(process.ProcessName.ToLower()))
                    return true;

                Windows.Items.Add(new CapturableWindow
                {
                    Handle = hWnd,
                    Name = $"{title} ({process.ProcessName}.exe)"
                });

                return true;
            }, IntPtr.Zero);
        }

        private void WindowsOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }

    public struct CapturableWindow
    {
        public string Name { get; set; }
        public IntPtr Handle { get; set; }
    }
}