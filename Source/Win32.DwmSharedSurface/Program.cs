using System;

using Win32.Shared;

namespace Win32.DwmSharedSurface
{
    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            using var window = new DxWindow(".NET Window Capture Samples - Win32.DwmSharedSurface", new DwmSharedSurface());
            window.Show();
        }
    }
}