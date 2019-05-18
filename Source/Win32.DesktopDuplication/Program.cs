using System;

using Win32.Shared;

namespace Win32.DesktopDuplication
{
    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            var window = new DxWindow(".NET Window Capture Samples - Win32.DesktopDuplication", new DesktopDuplication());
            window.Show();
        }
    }
}