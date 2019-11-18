# .NET Window Capture Samples

> NOTE: This is an experimental project. Please use it at your own risk.

Captures a window or desktop screen and renders it in WPF or DirectX.

## System Requirements

- Windows 10 (64-bit, April 2018 Update)
- DirectX 11
- .NET Framework 4.7.2
- Visual Studio 2019

## Capture Methods

| Project Name               | Minimal Windows Version | Render To | Platform |
| -------------------------- | ----------------------- | --------- | -------- |
| `Win32.BitBlt`             | Windows 95              | DirectX   | Win32    |
| `Win32.DesktopDuplication` | Windows 8               | DirectX   | Win32    |
| `Win32.DwmSharedSurface`   | Windows 7               | DirectX   | Win32    |
| `Win32.DwmThumbnail`       | Windows 7               | WPF       | Win32    |
| `WinRT.GraphicsCapture`    | Windows 10 1803         | DirectX   | WinRT    |

## Comparison

### Compare with capture target

| Capture Method             | Window Capture | Desktop Capture | Outside of Desktop | HW Acceleration | DirectX Games |
| -------------------------- | :------------: | :-------------: | :----------------: | :-------------: | :-----------: |
| `Win32.BitBlt`             |      Yes       |       Yes       |        Yes         |       No        |      Yes      |
| `Win32.DesktopDuplication` |       No       |       Yes       |         No         |       Yes       |      Yes      |
| `Win32.DwmSharedSurface`   |      Yes       |       No        |        Yes         |       No        |      Yes      |
| `Win32.DwmThumbnail`       |      Yes       |       No        |        Yes         |       Yes       |      Yes      |
| `WinRT.GraphicsCapture`    |      Yes       |       Yes       |        Yes         |       Yes       |      Yes      |

- `Games` is checked using [Märchen Forest](https://anemonecoronaria.sakura.ne.jp/merufore/).

### Compare with capture source

| Capture Method             | Window Handle | Monitor Handle |     Another      |
| -------------------------- | :-----------: | :------------: | :--------------: |
| `Win32.BitBlt`             |      Yes      |       No       |        -         |
| `Win32.DesktopDuplication` |      No       |       No       | Device (Monitor) |
| `Win32.DwmSharedSurface`   |      Yes      |       No       |        -         |
| `Win32.DwmThumbnail`       |      Yes      |       No       |        -         |
| `WinRT.GraphicsCapture`    |    Yes \*     |     Yes \*     | Embedded Picker  |

- \*: Require Windows 10 1903 or greater.

### Compare with delay

| Capture Method             | Delay (ms) |
| -------------------------- | :--------: |
| `Win32.BitBlt`             |   ~ 20ms   |
| `Win32.DesktopDuplication` |    N/A     |
| `Win32.DwmSharedSurface`   |   ~ 20ms   |
| `Win32.DwmThumbnail`       |  **0ms**   |
| `WinRT.GraphicsCapture`    |   ~ 40ms   |

- I used [this video](https://www.youtube.com/watch?v=rf2Lmfqi5ZM) to investigate the delay.
- It is just a reference value, but it is certain that the delay of `Win32.DwmThumbnail` is 0 ms.
  - This is because it uses a drawing method that is entirely common to other windows.

## License

This project is licensed under the MIT license.

## Third-Party Notices

### Dependencies

- [Microsoft.Windows.SDK.Contracts](https://www.nuget.org/packages/Microsoft.Windows.SDK.Contracts/)
- [SharpDX](https://www.nuget.org/packages/SharpDX/)

### Code Includes

- [SharpDX Samples](https://github.com/sharpdx/SharpDX-Samples) (MIT)
- [Windows Universal Samples](https://github.com/microsoft/Windows-universal-samples) (MIT)
- [Windows.UI.Composition Win32 Samples](https://github.com/microsoft/Windows.UI.Composition-Win32-Samples) (MIT)
