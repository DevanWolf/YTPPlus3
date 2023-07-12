# Pitch plugin for YTP+++

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    # No query support
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a YTP+++ plugin."
    Write-Host "Usage: pitch.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options>"
    exit 1
}

# Get command line args
$video = $args[0]
$width = $args[1]
$height = $args[2]
$temp = $args[3]
$ffmpeg = $args[4]
$ffprobe = $args[5]
$magick = $args[6]
$resources = $args[7]
$sounds = $args[8]
$sources = $args[9]
$music = $args[10]
$library = $args[11]
$options = $args[12]
$settingcount = $args[13]

# Temp files
$temp1 = Join-Path $temp "temp.mp4"

# Delete temp files
if (Test-Path $temp1) {
    Remove-Item $temp1
}

# Rename input file to temp file
if (Test-Path $video) {
    Rename-Item $video "temp.mp4"
}

# Pick which direction of the effect to apply
$pitchUpOrDown = Get-Random -Minimum 0 -Maximum 2

# Apply pitch filter
if ($pitchUpOrDown -eq 0) {
    # Higher pitch
    .\ffmpeg.exe -i "$temp1" -filter:v setpts=0.5*PTS -af asetrate=44100*2,aresample=44100 -y "$video"
} else {
    # Lower pitch
    .\ffmpeg.exe -i "$temp1" -filter:v setpts=2.0*PTS -af asetrate=44100/2,aresample=44100 -y "$video"
}
