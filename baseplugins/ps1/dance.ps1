# Dance plugin for YTP+++

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    # No query support
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a YTP+++ plugin."
    Write-Host "Usage: dance.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options>"
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
$temp2 = Join-Path $temp "temp2.mp4"
$temp3 = Join-Path $temp "temp3.mp4"

# Delete temp files
if (Test-Path $temp1) {
    Remove-Item $temp1
}
if (Test-Path $temp2) {
    Remove-Item $temp2
}
if (Test-Path $temp3) {
    Remove-Item $temp3
}

# Rename input file to temp file
if (Test-Path $video) {
    Rename-Item $video "temp.mp4"
}

# Pick random sound from $library *.wav, *.mp3, *.ogg, *.m4a, *.flac
$librarypath = Join-Path $library audio
$librarypath = Join-Path $librarypath music
$randomSound = Get-ChildItem -Path $librarypath -R -File -Include *.wav, *.mp3, *.ogg, *.m4a, *.flac | Get-Random
$randomSound = $randomSound.FullName

# Load options as json
$optionsjson = Get-Content $options | ConvertFrom-Json

# Get stream duration from options
$minStreamDuration = $optionsjson.MinStreamDuration
$maxStreamDuration = $optionsjson.MaxStreamDuration

# Parse as float
$minStreamDuration = [float]$minStreamDuration
$maxStreamDuration = [float]$maxStreamDuration

# Pick random time
$randomTime = Get-Random -Minimum $minStreamDuration -Maximum $maxStreamDuration

# Pick random roll
$useOriginalAudioRoll = Get-Random -Minimum 0 -Maximum 7

# Use original audio if equal to 0
if (($null -ne $randomSound) -and ($useOriginalAudioRoll -eq 0)) {
    ffmpeg -i "$temp1" -filter_complex "[0:v]setpts=.5*PTS[v];[0:a]atempo=2.0[a]" -map "[v]" -map "[a]" -y "$temp2"
    ffmpeg -i "$temp2" -vf reverse -y "$temp3"
    ffmpeg -i "$temp3" -i "$temp2" -filter_complex "[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out];[0:a][1:a][0:a][1:a][0:a][1:a][0:a][1:a]concat=n=8:v=0:a=1[out2]" -map "[out]" -map "[out2]" -shortest -y "$video"
}

# Use random audio
else {
    # Seek audio 1-5 seconds ahead to avoid silence at the beginning
    $seek = Get-Random -Minimum 1 -Maximum 5
    ffmpeg -i "$temp1" -an -vf setpts=.5*PTS -t $randomTime -y "$temp2"
    ffmpeg -i "$temp2" -vf reverse -y "$temp3"
    ffmpeg -i "$temp3" -i "$temp2" -ss $seek -i "$randomSound" -filter_complex "[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out]" -map "[out]" -map 2:a -shortest -y "$video"
}
