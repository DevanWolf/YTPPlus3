# YTP+++

![YTP+++](https://i.imgur.com/BVumJkW.png)

[YTP+ Beta](https://github.com/YTP-Plus/YTPPlusUI) & [YTP+ Beta (Library)](https://github.com/YTP-Plus/YTPPlus) > [YTP++ Beta](https://github.com/KiwifruitDev/ytpplus-node-ui) & [YTP+ Node](https://github.com/KiwifruitDev/ytpplus-node) > [YTP++](https://github.com/YTP-Plus/YTPPlusPlus) > [YTP+ Studio](https://github.com/YTP-Plus/YTPPlusStudio) & [YTP+ CLI](https://github.com/YTP-Plus/YTPPlusCLI) > ***YTP+++***

_____

A nonsensical video generator software written in C# using MonoGame.

Currently only supports Windows and is in heavy development, so expect bugs.

## Installation

This section will guide you through installing YTP+++. There are two methods of installation:

### Deployment Installation

This installation method uses PowerShell to automatically install YTP+++.

It is recommended to use this method if you are installing YTP+++ for the first time.

You must have administrator privileges to use this method.

#### Steps

1. Open the [deployment script](https://raw.githubusercontent.com/YTP-Plus/YTPPlusPlusPlus/main/deploy_ytpplusplusplus.ps1) in a new tab.
1. Select all of the text and copy it.
1. Open PowerShell as an administrator.
1. Paste the text into the PowerShell window.
1. Press enter.
    - You may need to press enter several times.

#### Script Details

- [Chocolatey](https://chocolatey.org/) will be used to install the [prerequisites](#prerequisites) automatically.
  - After installation, it is safe to uninstall Chocolatey.
- YTP+++ will be installed to `C:\YTPPlusPlusPlus`.
- The script will automatically update YTP+++ to the latest version.
- Shortcuts will be created on the desktop and in the start menu.
- The script will automatically run YTP+++ after installation.
  - After running the script, it is safe to close the PowerShell window.

### Manual Installation

1. Download and install the [prerequisites](#prerequisites).
1. Download the latest release from the [releases page](https://github.com/YTP-Plus/YTPPlusPlusPlus/releases).
1. Extract the zip file to a directory that does not include spaces in the path.
    - For example, `C:\YTPPlusPlusPlus` is a good path, but `C:\Program Files\YTPPlusPlusPlus` is not.
1. Run `YTP+++.exe`.

## Usage

Using YTP+++ is fairly straightforward. The following steps will guide you through the process of creating a video and watching it.

1. Start YTP+++.
1. Follow the initial setup.
    - If you encounter any issues, please ensure that you have installed all of the [prerequisites](#prerequisites).
    - Update to the latest version of YTP+++ if prompted.
1. Click on the **Library** tab.
1. Click on the **Materials** tab.
1. Click on any plus sign to add a material to the library.
1. Click on the **Generate** tab.
1. Click on the **Start Rendering** button.
1. Wait for the video to render, the progress will be displayed at the top.
    - You may also press ~ to open the console.
1. Once finished, click on the **Library** tab and click on the **Renders** tab.
1. Click on the thumbnail of the video you just rendered to play it.
1. The default video player will be used to play the video.
    - If you do not have a default video player, Windows may prompt you to select one.
    - A recommended video player is [VLC](https://www.videolan.org/vlc/index.html).

## Updating

YTP+++ supports automatic updates. The update process is as follows:

1. Click on the **Help** tab.
1. Click on the **Show Tutorial Window** button.
1. Follow the initial setup to the last step.
1. Update to the latest version of YTP+++ if prompted.

## Prerequisites

These software packages are required to run YTP+++. They are not included in the release.

### Required

- Windows
  - YTP+++ is only supported on Windows, but it may be possible to run it on other operating systems with Wine or a similar tool.
    - Support will not be provided for Wine due to lack of knowledge.
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

### Optional

- Node.JS
  - This is only required if you want to use legacy Node.JS plugins from YTP+ CLI.
  - Download Node.JS from [here](https://nodejs.org/en/download/).
  - Install Node.JS.
- ImageMagick
  - Plugins may require ImageMagick to be installed. It is not required for YTP+++ to function.
  - Download ImageMagick from [here](https://imagemagick.org/script/download.php#windows).
    - Please ensure that you download the **Win64 *static* at 16 bits-per-pixel component with high dynamic-range imaging enabled** version.
    - Do not download the dynamic version.
  - Install ImageMagick.

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

## Soundtrack

![Ambience of YTP+](https://i.imgur.com/6muqeuY.png)

The official YTP+ soundtrack, **Ambience of YTP+**, is available on [GitHub](https://github.com/ytp-plus/soundtrack).

## Credits

These people have helped make YTP+ and its successors possible:

- [hellfire](https://github.com/hellfire3d): Creating the original YTP+ software
- [KiwifruitDev](https://github.com/KiwifruitDev): Programming, UI, maintenance, Discord management
- [nuppington](https://github.com/nuppington-bit): General help and Discord management
- [GMM](https://github.com/gmm2003): UI sound effects, and plugin development
- [Bobby I Guess](https://www.youtube.com/@CrazyGoldenGamer): UI music, general help
- [DevanWolf](https://github.com/DevanWolf): Providing support and creating fixes for YTP++
- [Supositware](https://github.com/Supositware): Creating YTP5K, a Twitter bot using YTP+ code
- [DeeMacias](https://www.youtube.com/channel/UCAoRaLkle7tcUvr9oKlLgDw): Plugin development and general help
- [0znə](https://www.youtube.com/channel/UC7-Jkq6BHMxBOHNzkLXRi8g): Discord moderation and continued involvement
- [Spiral](https://github.com/Spiral2839): Discord moderation and continued involvement

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
