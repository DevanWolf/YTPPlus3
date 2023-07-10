# Random sound plugin for YTP+++

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    # No query support
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a YTP+++ plugin."
    Write-Host "Usage: randomsound.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options>"
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

# Pick random sound from $library *.wav, *.mp3, *.ogg, *.m4a, *.flac
$librarypath = Join-Path $library audio
$librarypath = Join-Path $librarypath sfx
$librarypath = Join-Path $librarypath *
$randomSound = Get-ChildItem -Path $librarypath -File -Include *.wav, *.mp3, *.ogg, *.m4a, *.flac | Get-Random

# Pick whether or not to mute original audio
$muteOriginalAudio = Get-Random -Minimum 0 -Maximum 1

# Apply random sound
if (Test-Path $randomSound) {
    # Delete temp files
    if (Test-Path $temp1) {
        Remove-Item $temp1
    }

    # Rename input file to temp file
    if (Test-Path $video) {
        Rename-Item $video "temp.mp4"
    }

    $randomSound = $randomSound.FullName

    # Use original audio if equal to 0
    if ($muteOriginalAudio -eq 0) {
        ffmpeg -i "$temp1" -i "$randomSound" -filter_complex "[0:a]volume=1[a0];[1:a]volume=1[a1];[a0][a1]amix=inputs=2[a]" -map 0:v -map "[a]" -c:v copy -c:a aac -shortest "$video"
    } else {
        ffmpeg -i "$temp1" -i "$randomSound" -filter_complex "[0:a]volume=0[a0];[1:a]volume=1[a1];[a0][a1]amix=inputs=2[a]" -map 0:v -map "[a]" -c:v copy -c:a aac -shortest "$video"
    }
}
