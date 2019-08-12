# .NET Window Capture Samples

> NOTE: This is an experimental project. Please use it at your own risk.

Captures a window or desktop screen and renders it in WPF or DirectX.


## System Requirements

* Windows 10 (64-bit, April 2018 Update)
* DirectX 11
* .NET Framework 4.7.2
* Visual Studio 2019


## Capture Methods

| Project Name               | Minimal Windows Version | Render To | Platform |
| -------------------------- | ----------------------- | --------- | -------- |
| `Win32.ApiHooks`           | Windows XP SP2          | DirectX   | Win32    |
| `Win32.BitBlt`             | Windows 95              | DirectX   | Win32    |
| `Win32.DesktopDuplication` | Windows 8               | DirectX   | Win32    |
| `Win32.DwmSharedSurface`   | Windows 7               | DirectX   | Win32    |
| `Win32.DwmThumbnail`       | Windows 7               | WPF       | Win32    |
| `Win32.PrintWindow`        | Windows XP              | WPF       | Win32    |
| `WinRT.GraphicsCapture`    | Windows 10 1803         | DirectX   | WinRT    |


## Compare

| Capture Method             | Window Capture | Desktop Capture | Outside of Desktop | HW Acceleration | DirectX Games |
| -------------------------- | :------------: | :-------------: | :----------------: | :-------------: | :-----------: |
| `Win32.ApiHooks`           |    Unknown     |     Unknown     |      Unknown       |     Unknown     |    Unknown    |
| `Win32.BitBlt`             |      Yes       |       Yes       |        Yes         |       No        |      Yes      |
| `Win32.DesktopDuplication` |       No       |       Yes       |         No         |       Yes       |      Yes      |
| `Win32.DwmSharedSurface`   |      Yes       |       No        |        Yes         |       No        |      Yes      |
| `Win32.DwmThumbnail`       |      Yes       |       No        |        Yes         |       Yes       |      Yes      |
| `Win32.PrintWindow`        |    Unknown     |     Unknown     |      Unknown       |     Unknown     |    Unknown    |
| `WinRT.GraphicsCapture`    |      Yes       |       Yes       |        Yes         |       Yes       |      Yes      |

* `Games` is checked using [Märchen Forest](https://anemonecoronaria.sakura.ne.jp/merufore/).


## License

This project is licensed under the MIT license.


## Third-Party Notices

### Dependencies

* [EasyHook](https://www.nuget.org/packages/EasyHook/)
* [Microsoft.Windows.SDK.Contracts](https://www.nuget.org/packages/Microsoft.Windows.SDK.Contracts/)
* [Microsoft.Wpf.Interop.DirectX](https://www.nuget.org/packages/Microsoft.Wpf.Interop.DirectX-x64/)
* [SharpDX](https://www.nuget.org/packages/SharpDX/)


### Code Includes

* [SharpDX Samples](https://github.com/sharpdx/SharpDX-Samples) (MIT)
* [Windows Universal Samples](https://github.com/microsoft/Windows-universal-samples) (MIT)
