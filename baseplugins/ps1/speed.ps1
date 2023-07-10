# Speed plugin for YTP+++

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    # No query support
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a YTP+++ plugin."
    Write-Host "Usage: speed.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options>"
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

# Pick slow down or speed up
$speed = Get-Random -Minimum 0 -Maximum 2

# Apply speed filter
if ($speed -eq 0) {
    # Speed up
    .\ffmpeg.exe -i "$temp1" -filter:v setpts=0.5*PTS -filter:a atempo=2.0 -y "$video"
} else {
    # Slow down
    .\ffmpeg.exe -i "$temp1" -filter:v setpts=2.0*PTS -filter:a atempo=0.5 -y "$video"
}
