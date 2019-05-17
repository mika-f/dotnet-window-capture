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
| `Win32.DwmThumbnail`       | Windows 7               | WPF       | Win32    |
| `WinRT.GraphicsCapture`    | Windows 10 1803         | DirectX   | WinRT    |


## Compare

| Capture Method             | Window Capture | Desktop Capture | Outside of Desktop | HW Acceleration | DirectX Games |
| -------------------------- | :------------: | :-------------: | :----------------: | :-------------: | :-----------: |
| `Win32.DwmThumbnail`       |      Yes       |       No        |         No         |       Yes       |      Yes      |
| `WinRT.GraphicsCapture`    |      Yes       |       Yes       |        Yes         |       Yes       |      Yes      |



## License

This project is licensed under the MIT license.


## Third-Party Notices

### Dependencies

* [Microsoft.Windows.SDK.Contracts](https://www.nuget.org/packages/Microsoft.Windows.SDK.Contracts/)
* [SharpDX](https://www.nuget.org/packages/SharpDX/)


### Code Includes

* [SharpDX Samples](https://github.com/sharpdx/SharpDX-Samples) (MIT)
* [Windows Universal Samples](https://github.com/microsoft/Windows-universal-samples) (MIT)
