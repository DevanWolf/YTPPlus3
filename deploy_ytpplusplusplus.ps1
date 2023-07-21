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
    This software is no longer supported.
    https://store.steampowered.com/app/2516360/Nonsensical_Video_Generator/
    Discord: https://discord.gg/8ppmspR6Wh
" -ForegroundColor White -BackgroundColor Black
    # Ask user if they want to continue.
    Read-Host "Press any key to close this window"
}

# Run script.
Install-YTPPlusPlusPlus
