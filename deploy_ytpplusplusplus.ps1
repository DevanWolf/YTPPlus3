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
    Written by KiwifruitDev.
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
        Write-Host "This script will install Chocolatey, .NET 6.0 Desktop Runtime, Python 3, Node.JS, ImageMagick, and FFmpeg.
Afterwards, it will download and extract YTP+++ to C:\YTPPlusPlusPlus.
It will also create a shortcut on your desktop and in your Start Menu.
" -ForegroundColor Yellow -BackgroundColor Black

        # Ask user if they want to continue.
        $continue = Read-Host "Do you want to continue? (y/N)"
        if ($continue -eq "y") {
            Write-Host "Continuing..." -ForegroundColor Green
            # Install Chocolatey.
            Write-Host "Installing Chocolatey..." -ForegroundColor Yellow
            Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

            # Refresh PATH.
            Write-Host "Refreshing PATH..." -ForegroundColor Yellow
            refreshenv

            # Install .NET 6.0 Desktop Runtime, Python 3, and Node.JS with Chocolatey.
            Write-Host "Installing .NET 6.0 Desktop Runtime, Python 3, Node.JS, ImageMagick, and FFmpeg..." -ForegroundColor Yellow
            choco install dotnet-6.0-desktopruntime python3 nodejs imagemagick ffmpeg -y

            # Refresh PATH.
            Write-Host "Refreshing PATH..." -ForegroundColor Yellow
            refreshenv    

            # Pull https://api.github.com/repos/YTP-Plus/YTPPlusPlusPlus/releases/latest to get the latest release and extract it to Desktop.
            Write-Host "Getting latest YTP+++ release..." -ForegroundColor Yellow
            $header = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
            $header.Add("User-Agent", "YTPPlusPlusPlus")
            $ytp = Invoke-WebRequest -Uri "https://api.github.com/repos/YTP-Plus/YTPPlusPlusPlus/releases/latest" -Headers $header -OutFile "$env:TEMP\ytp.json"

            # Get first asset URL.
            $ytpAsset = (Get-Content "$env:TEMP\ytp.json" | ConvertFrom-Json).assets[0].browser_download_url

            # Download asset.
            Write-Host "Downloading YTP+++..." -ForegroundColor Yellow
            $ytpAsset = Invoke-WebRequest -Uri $ytpAsset -OutFile "$env:TEMP\ytpplusplusplus.zip"

            # Create YTPPlusPlusPlus folder on Desktop if it doesn't exist.
            if (-not (Test-Path "C:\YTPPlusPlusPlus")) {
                Write-Host "Creating YTPPlusPlusPlus folder..."
                New-Item -ItemType Directory -Path "C:\YTPPlusPlusPlus"
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
            $shortcut.Description = "Nonsensical video generator."
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
            cd "C:\YTPPlusPlusPlus"

            # Done!
            Write-Host "Done!" -ForegroundColor Green

            # Write ascii art again and display instructions.
            Write-Host "$asciiart
    YTP+++ installation is complete!
    To run YTP+++, open the shortcut on your desktop or in your Start Menu.
    To check for updates, head to the ""Help"" tab and click ""Open Tutorial Window"".

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
