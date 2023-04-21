# Deploy script for YTP+++.

# Complain if not run as administrator.
$administrator = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not ($administrator.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator))) {
    Write-Host "This script must be run as administrator." -ForegroundColor Red
}

# If run as administrator, continue.
if ($administrator.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)) {
    # Install Chocolatey.
    Write-Host "Installing Chocolatey..."
    Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

    # Refresh PATH.
    Write-Host "Refreshing PATH..."
    refreshenv

    # Install .NET 6.0 Desktop Runtime, Python 3, and Node.JS with Chocolatey.
    Write-Host "Installing .NET 6.0 Desktop Runtime, Python 3, Node.JS, ImageMagick, and FFmpeg..."
    choco install dotnet-6.0-desktopruntime python3 nodejs imagemagick ffmpeg -y

    # Refresh PATH.
    Write-Host "Refreshing PATH..."
    refreshenv    

    # Pull https://api.github.com/repos/YTP-Plus/YTPPlusPlusPlus/releases/latest to get the latest release and extract it to Desktop.
    Write-Host "Getting latest YTP+++ release..."
    $header = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
    $header.Add("User-Agent", "YTPPlusPlusPlus")
    $ytp = Invoke-WebRequest -Uri "https://api.github.com/repos/YTP-Plus/YTPPlusPlusPlus/releases/latest" -Headers $header -OutFile "$env:TEMP\ytp.json"

    # Get first asset URL.
    $ytpAsset = (Get-Content "$env:TEMP\ytp.json" | ConvertFrom-Json).assets[0].browser_download_url

    # Download asset.
    Write-Host "Downloading YTP+++..."
    $ytpAsset = Invoke-WebRequest -Uri $ytpAsset -OutFile "$env:TEMP\ytpplusplusplus.zip"

    # Create YTPPlusPlusPlus folder on Desktop if it doesn't exist.
    if (-not (Test-Path "C:\YTPPlusPlusPlus")) {
        Write-Host "Creating YTPPlusPlusPlus folder..."
        New-Item -ItemType Directory -Path "C:\YTPPlusPlusPlus"
    }

    # Extract asset to Desktop.
    Write-Host "Extracting YTP+++..."
    Expand-Archive -Path "$env:TEMP\ytpplusplusplus.zip" -DestinationPath "C:\YTPPlusPlusPlus" -Force

    # Create shortcut on Desktop and set "Start in" to YTPlusPlusPlus folder.
    Write-Host "Creating desktop shortcut..."
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut("$env:USERPROFILE\Desktop\YTP+++.lnk")
    $shortcut.TargetPath = "C:\YTPPlusPlusPlus\YTP+++.exe"
    $shortcut.WorkingDirectory = "C:\YTPPlusPlusPlus"
    $shortcut.Description = "Nonsensical video generator."
    $shortcut.Save()

    # Copy shortcut to Start Menu.
    Write-Host "Copying shortcut to Start Menu..."
    Copy-Item -Path "$env:USERPROFILE\Desktop\YTP+++.lnk" -Destination "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\YTP+++.lnk"

    # Delete temporary files.
    Write-Host "Deleting temporary files..."
    Remove-Item -Path "$env:TEMP\ytp.json"
    Remove-Item -Path "$env:TEMP\ytpplusplusplus.zip"

    # Change directory to YTPPlusPlusPlus folder.
    Write-Host "Changing directory to YTPPlusPlusPlus folder..."
    cd "C:\YTPPlusPlusPlus"

    # Start YTP+++.
    Write-Host "Starting YTP+++..."
    Start-Process -FilePath "C:\YTPPlusPlusPlus\YTP+++.exe"

    # Done!
    Write-Host "Done!" -ForegroundColor Green
}
