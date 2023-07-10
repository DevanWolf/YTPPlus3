# Deploy script for YTP+++.

$asciiart = "
            @@@@@@@@@@@@@@@@@@@@@@                                                                            
        @@@@@@                  @@@@@@                                                                        
      @@@@                          @@@@                                                                      
      @@          @@@@@@@@@@  @@      @@          @@@@@@@@@@                                                  
    @@@@      @@@@            @@      @@@@        @@      @@            @@@@@@@@@@  @@@@@@@@@@  @@@@@@@@@@    
    @@      @@            @@@@@@        @@@@@@@@@@@@  @@  @@@@@@@@@@@@@@@@      @@  @@      @@  @@      @@    
    @@      @@                          @@            @@              @@@@  @@  @@@@@@  @@  @@@@@@  @@  @@@@@@
    @@    @@          @@          @@    @@  @@    @@  @@@@    @@@@@@        @@          @@          @@      @@
    @@    @@          @@          @@    @@  @@    @@  @@      @@    @@  @@@@@@@@@@  @@@@@@@@@@  @@@@@@@@@@  @@
    @@    @@      @@@@@@@@@@      @@    @@  @@    @@  @@  @@  @@    @@      @@          @@          @@      @@
    @@    @@          @@          @@    @@  @@    @@  @@      @@    @@  @@  @@  @@@@@@  @@  @@@@@@  @@  @@@@@@
    @@    @@          @@          @@    @@    @@@@@@    @@@@  @@@@@@    @@      @@  @@      @@  @@      @@    
    @@                          @@      @@@@      @@          @@      @@@@@@@@@@@@  @@@@@@@@@@  @@@@@@@@@@    
    @@        @@@@@@            @@      @@@@  @@@@    @@@@@@  @@  @@@@@@                                      
    @@@@      @@            @@@@      @@@@@@        @@@@  @@      @@                                          
      @@      @@  @@@@@@@@@@          @@  @@@@@@@@@@@@    @@@@@@@@@@                                          
      @@@@                          @@@@                                                                      
        @@@@@@                  @@@@@@                                                                        
            @@@@@@@@@@@@@@@@@@@@@@                                                                            
"

# Function
function Install-YTPPlusPlusPlus {
    # Write ASCII art
    Write-Host "$asciiart
    Welcome to the YTP+++ deploy script!
    Written by KiwifruitDev. Press Ctrl + Left Click to open links in your web browser.
    Need help? Join the YTP+ Hub Discord: https://discord.gg/8ppmspR6Wh
" -ForegroundColor White -BackgroundColor Black

    # Complain if not run as administrator.
    $administrator = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not ($administrator.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator))) {
        Write-Host "This script must be run as administrator." -ForegroundColor Red
    }
    # If run as administrator, continue.
    if ($administrator.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)) {
        # Display warning.
        Write-Host "This script will install .NET 6 Desktop Runtime.
Afterwards, it will download and extract YTP+++ to C:\YTPPlusPlusPlus.
It will also create a shortcut on your desktop and in your Start Menu.
" -ForegroundColor Yellow -BackgroundColor Black

        # Ask user if they want to continue.
        $continue = Read-Host "Do you want to continue? (y/N)"
        if ($continue -eq "y") {
            Write-Host "Continuing..." -ForegroundColor Green

            # Check if .NET 6 Desktop Runtime is installed.
            Write-Host "Checking if .NET 6 Desktop Runtime is installed..." -ForegroundColor Yellow
            if (-not (Get-Command dotnet)) {
                # Check if winget is installed.
                Write-Host "Checking if winget is installed..." -ForegroundColor Yellow
                if (-not (Get-Command winget)) {
                    # If winget is not installed, install it.
                    Write-Host "Installing winget..." -ForegroundColor Yellow
                    Add-AppxPackage -RegisterByFamilyName -MainPackage Microsoft.DesktopAppInstaller_8wekyb3d8bbwe

                    # Refresh PATH.
                    Write-Host "Refreshing PATH..." -ForegroundColor Yellow
                    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
                }

                # Install .NET 6 Desktop Runtime with winget or msi.
                if (Get-Command winget) {
                    # Winget is now installed.
                    Write-Host "winget is now installed." -ForegroundColor Green
                    # If winget is installed, install .NET 6 Desktop Runtime with winget.
                    Write-Host "Installing .NET 6 Desktop Runtime with winget..." -ForegroundColor Yellow
                    winget install Microsoft.DotNet.DesktopRuntime.6
                }
                else {
                    Write-Host "Installation for winget failed. Trying msi..." -ForegroundColor Yellow
                    # If winget is not installed, install .NET 6 Desktop Runtime with msi.
                    Write-Host "Installing .NET 6 Desktop Runtime with msi..." -ForegroundColor Yellow
                    $dotneturl = "https://download.visualstudio.microsoft.com/download/pr/7bb7f85b-9bf0-4c6f-b3e4-a3832720f162/73e280cfd7f686c34748e0bf98d879c7/dotnet-runtime-6.0.19-win-x64.exe"
                    $dotnetpath = "$env:TEMP\dotnet-runtime-6.0.19-win-x64.exe"
                    Invoke-WebRequest -Uri $dotneturl -OutFile $dotnetpath
                    Start-Process -FilePath $dotnetpath -ArgumentList "/install /quiet /norestart" -Wait
                }

                # Refresh PATH.
                Write-Host "Refreshing PATH..." -ForegroundColor Yellow
                $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User") 

                # Check if .NET 6 Desktop Runtime is installed.
                Write-Host "Checking if .NET 6 Desktop Runtime is installed..." -ForegroundColor Yellow
                if (-not (Get-Command dotnet)) {
                    # If .NET 6 Desktop Runtime is not installed, complain.
                    Write-Host ".NET 6 Desktop Runtime could not be installed. Please install it manually." -ForegroundColor Red
                }
                else {
                    # If .NET 6 Desktop Runtime is installed, continue.
                    Write-Host ".NET 6 Desktop Runtime is now installed." -ForegroundColor Green
                }
            }
            else {
                # If .NET 6 Desktop Runtime is installed, continue.
                Write-Host ".NET 6 Desktop Runtime is already installed." -ForegroundColor Green
            }

            # Pull https://api.github.com/repos/YTP-Plus/YTPPlusPlusPlus/releases/latest to get the latest release and extract it to Desktop.
            Write-Host "Getting latest YTP+++ release..." -ForegroundColor Yellow
            $header = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
            $header.Add("User-Agent", "YTPPlusPlusPlus")
            $ytp = Invoke-WebRequest -Uri "https://api.github.com/repos/YTP-Plus/YTPPlusPlusPlus/releases/latest" -Headers $header -OutFile "$env:TEMP\ytp.json"
            
            # Convert JSON to PowerShell object.
            $ytp = Get-Content "$env:TEMP\ytp.json" | ConvertFrom-Json

            # Get asset URLs.
            $ytpAsset = $ytp.assets[0]
            $ytpDownload = $ytpAsset.browser_download_url
            $ytpVersion = $ytpAsset.tag_name

            # Download asset.
            Write-Host "Downloading YTP+++ $ytpVersion..." -ForegroundColor Yellow
            $ytpAsset = Invoke-WebRequest -Uri $ytpDownload -OutFile "$env:TEMP\ytpplusplusplus.zip"

            # Create YTPPlusPlusPlus folder on Desktop if it doesn't exist.
            if (-not (Test-Path "C:\YTPPlusPlusPlus")) {
                Write-Host "Creating YTPPlusPlusPlus folder..."
                New-Item -ItemType Directory -Path "C:\YTPPlusPlusPlus" -Force
            }

            # Extract asset to Desktop.
            Write-Host "Extracting YTP+++..." -ForegroundColor Yellow
            Expand-Archive -Path "$env:TEMP\ytpplusplusplus.zip" -DestinationPath "C:\YTPPlusPlusPlus" -Force

            # Create shortcut on Desktop and set "Start in" to YTPlusPlusPlus folder.
            Write-Host "Creating desktop shortcut..." -ForegroundColor Yellow
            $shell = New-Object -ComObject WScript.Shell
            $shortcut = $shell.CreateShortcut("$env:USERPROFILE\Desktop\YTP+++.lnk")
            $shortcut.TargetPath = "C:\YTPPlusPlusPlus\YTP+++.exe"
            $shortcut.WorkingDirectory = "C:\YTPPlusPlusPlus"
            $shortcut.Description = "Nonsensical video generator"
            $shortcut.Save()

            # Copy shortcut to Start Menu.
            Write-Host "Copying shortcut to Start Menu..." -ForegroundColor Yellow
            Copy-Item -Path "$env:USERPROFILE\Desktop\YTP+++.lnk" -Destination "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\YTP+++.lnk"

            # Delete temporary files.
            Write-Host "Deleting temporary files..." -ForegroundColor Yellow
            Remove-Item -Path "$env:TEMP\ytp.json"
            Remove-Item -Path "$env:TEMP\ytpplusplusplus.zip"

            # Change directory to YTPPlusPlusPlus folder.
            Write-Host "Changing directory to YTPPlusPlusPlus folder..." -ForegroundColor Yellow
            Set-Location -Path "C:\YTPPlusPlusPlus"

            # Done!
            Write-Host "Finished installing YTP+++!" -ForegroundColor Green

            # Check if magick is installed.
            Write-Host "Checking if ImageMagick is installed..." -ForegroundColor Yellow
            if (-not (Get-Command magick)) {
                # If ImageMagick is not installed, ask user if they would like to install it.
                Write-Host "ImageMagick is not installed." -ForegroundColor Yellow

                # Ask user if they would like to install ImageMagick.
                Write-Host "Would you like to install ImageMagick? (y/n)"
                $continue = Read-Host

                # If user wants to install ImageMagick, open web browser to download page.
                if ($continue -eq "y") {
                    # Give instructions.
                    Write-Host "You can install ImageMagick from https://imagemagick.org/script/download.php#windows"
                    Write-Host "Download ""Win64 dynamic at 16 bits-per-pixel component with High-dynamic-range imaging enabled""."
                    Write-Host "Do NOT download a static release, only select the dynamic release."
                    Write-Host "Ensure that the checkbox to install FFmpeg is NOT checked."
                    Write-Host "Once downloaded, run the installer and follow the instructions."
                    Write-Host "Would you like to open the download page now? (y/n)"
                    $continue = Read-Host
                    if ($continue -eq "y") {
                        Write-Host "Opening web browser to ImageMagick download page..."
                        Start-Process -FilePath "https://imagemagick.org/script/download.php#windows"
                    }
                }
                else {
                    Write-Host "You can download ImageMagick later from https://imagemagick.org/script/download.php#windows"
                }
            }
            else {
                # If ImageMagick is installed, continue.
                Write-Host "ImageMagick is already installed." -ForegroundColor Green
            }

            # We're completely done!
            Write-Host "YTP+++ is now ready to use!" -ForegroundColor Green

            # Write ascii art again and display instructions.
            Write-Host "$asciiart
    YTP+++ installation is complete!
    To run YTP+++, open the shortcut on your desktop or in your Start Menu.
    Need help? Join the YTP+ Hub Discord: https://discord.gg/8ppmspR6Wh
    Want to contribute? Head to https://github.com/YTP-Plus/YTPPlusPlusPlus and make a pull request!
    Have fun making nonsensical videos! It is now safe to close PowerShell if you want to.
    " -ForegroundColor White -BackgroundColor Black
        }
        if (-not ($continue -eq "y")) {
            Write-Host "Exiting..." -ForegroundColor Red
        }
    }
}

# Run script.
Install-YTPPlusPlusPlus
