# YTP+++

A nonsensical video generator written in C# using MonoGame.

Currently only supports Windows.

In Beta, so expect bugs.

## Requirements

- Windows
  - YTP+++ is only supported on Windows, but it may be possible to run it on other operating systems with Wine or a similar tool.
    - Support will not be provided for Wine due to lack of knowledge.
- DirectX 11
  - YTP+++ requires DirectX 11 to run.
    - This is included with Windows 7 and newer.
    - If you are using Windows 7, please note that YTP+++ has not been tested for your operating system.
    - You can try installing the **DirectX End-User Runtime** if you are having issues.
- FFmpeg
  - Download the gyan.dev essential release from [here](https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip).
  - Extract the zip file to a directory that does not include spaces in the path.
    - For example, `C:\FFMPEG` is a good path, but `C:\Program Files\FFMPEG` is not.
  - Add the `bin` directory to your PATH environment variable.
    - See [Adding an Environment Variable](#adding-an-environment-variable) for instructions.
- .NET 6.0
  - Download the .NET Desktop Runtime from [here](https://dotnet.microsoft.com/download/dotnet/6.0/runtime).
    - Please ensure that you download the **Run Desktop Apps** x64 version.
  - Install the .NET Desktop Runtime.
- Python 3
  - Download the latest Python 3 release from [here](https://www.python.org/downloads/).
  - Install Python 3 and ensure that the option to add Python to your PATH environment variable is selected.
    - If Python is already installed or it was not added to your PATH environment variable, you will need to add it manually.
    - See [Adding an Environment Variable](#adding-an-environment-variable) for instructions.
- Node.JS (Optional)
  - This is only required if you want to use legacy Node.JS plugins from YTP+ CLI.
  - Download Node.JS from [here](https://nodejs.org/en/download/).
  - Install Node.JS.

## Usage

1. Download the latest release from the [releases page](https://github.com/YTP-Plus/YTPPlusPlusPlus.git).
1. Extract the zip file to a directory that does not include spaces in the path.
    - For example, `C:\YTPPlusPlusPlus` is a good path, but `C:\Program Files\YTPPlusPlusPlus` is not.
1. Run `YTP+++.exe`.
1. Click on the **Library** tab.
1. Click on the **Materials** tab.
1. Click on any plus sign to add a material to the library.
1. Click on the **Generate** tab.
1. Click on the **Start Rendering** button.
1. Wait for the video to render, the progress will be displayed at the top.
    - You may also press ~ to open the console.
1. Once finished, click on the **Library** tab and click on the **Renders** tab.
1. Click on the thumbnail of the video you just rendered to play it.

## Adding an Environment Variable

1. Press the Windows key.
1. Type `Edit the system environment variables` and press enter.
1. Click on the `Environment Variables...` button.
1. Select `Path` from the `System variables` list.
1. Click on the `Edit...` button.
1. Click on the `New` button.
1. Type the path containing the executable.
    - For example, `C:\FFMPEG\bin`.
1. Click on the `OK` button.
1. You may need to restart your computer for the changes to take effect.

## Building

Distributed YTP+++ builds are compiled with Visual Studio Code and the C# extension.

However, you can also build it with Visual Studio 2019.

### Visual Studio Code

1. Install [Visual Studio Code](https://code.visualstudio.com/).
1. Install the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).
1. Open the project folder in Visual Studio Code.
1. Open the command palette (Ctrl+Shift+P).
1. Type `.NET: Restore Project` and press enter.
1. Select `YTP+++.csproj` from the list if prompted.
1. Open the command palette (Ctrl+Shift+P).
1. Type `.NET: Generate Assets for Build and Debug` and press enter.
    - Alternatively, try Ctrl+Shift+B. This may not function if the CMake Tools extension is installed.
1. Select `YTP+++.csproj` from the list if prompted.
1. YTP+++ may be launched by pressing F5 or executing `YTP+++.exe` in the `bin/Debug/net6.0-windows/` directory.

### Visual Studio 2019

1. Install [Visual Studio 2019](https://visualstudio.microsoft.com/).
1. Create a solution and add the project.
1. Build the solution.
1. YTP+++ may be launched by pressing F5 or executing `YTP+++.exe` in the `bin/Debug/net6.0-windows/` directory.

## Contributing

Contributions are welcome.

Please open an issue or pull request if you have any suggestions or bug reports.

## Third Party Credits

- [MonoGame](https://www.monogame.net/)
- [FFmpeg](https://ffmpeg.org/)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [Munro](https://www.tenbytwenty.com/#munro)

## License

YTP+++ is licensed under the [MIT License](LICENSE).
