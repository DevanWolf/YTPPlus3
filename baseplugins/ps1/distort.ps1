# Dance plugin for YTP+++

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    Write-Host "1:Distort:distort:These_audio_clips_should_be_in_rhythm."
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a YTP+++ plugin."
    Write-Host "Usage: distort.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options>"
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

# Check if command "magick" exists
if (-not (Get-Command magick -ErrorAction SilentlyContinue)) {
    Write-Host "This plugin requires ImageMagick."
    exit 1
}

# Delete distort files
for ($i = 0; $i -lt 6; $i++) {
    if (Test-Path $temp"distort$i.png") {
        Remove-Item $temp"distort$i.png"
    }
}

# Pick random sound from $library *.wav, *.mp3, *.ogg, *.m4a, *.flac
$librarypath = Join-Path $library audio
$librarypath = Join-Path $librarypath distort
$randomSound = Get-ChildItem -Path $librarypath -R -File -Include *.wav, *.mp3, *.ogg, *.m4a, *.flac | Get-Random
$randomSound = $randomSound.FullName

# Random sound not found?
if ($null -eq $randomSound) {
    Write-Host "No random sound found."
    exit 0
}

# Load options as json
$optionsjson = Get-Content $options | ConvertFrom-Json

# Get stream duration from options
$minStreamDuration = $optionsjson.MinStreamDuration
$maxStreamDuration = $optionsjson.MaxStreamDuration

# Parse as float
$minStreamDuration = [float]$minStreamDuration
$maxStreamDuration = [float]$maxStreamDuration

# Set distorts
$black = Join-Path $temp black.png
$distort = Join-Path $temp distort
$distort0 = Join-Path $temp distort0.png
$distort1 = Join-Path $temp distort1.png
$distort2 = Join-Path $temp distort2.png
$distort3 = Join-Path $temp distort3.png
$distort4 = Join-Path $temp distort4.png
$distort5 = Join-Path $temp distort5.png
$concatdistort = Join-Path $temp concatdistort.txt

# Create black frame
magick convert -size $width"x"$height canvas:black $black

# Create one frame from video
ffmpeg -i $video -ss 0 -update 1 -q:v 1 -y $distort0

# Apply effect 5 times
for ($i = 1; $i -lt 6; $i++) {
    $effect = Get-Random -Minimum 0 -Maximum 7
    $command = ""
    switch($effect) {
        0 {$command = "-flop"}
        1 {$command = "-flip"}
        2 {$command = "-implode $(Get-Random -Minimum -3 -Maximum -1)"}
        3 {$command = "-implode $(Get-Random -Minimum 1 -Maximum 3)"}
        4 {$command = "-swirl $(Get-Random -Minimum 1 -Maximum 180)"}
        5 {$command = "-swirl $(Get-Random -Minimum -180 -Maximum -1)"}
        6 {$command = "-channel RGB -negate"}
    }
    $magickexec = "magick convert $distort0 $command $distort$i.png"
    Invoke-Expression $magickexec
}

# Delete concatdistort.txt
if (Test-Path $temp"concatdistort.txt") {
    Remove-Item $temp"concatdistort.txt"
}

# Create concatdistort.txt
Add-Content $temp"concatdistort.txt" "file 'distort0.png'"
Add-Content $temp"concatdistort.txt" "duration 0.467"
Add-Content $temp"concatdistort.txt" "file 'distort1.png'"
Add-Content $temp"concatdistort.txt" "duration 0.434"
Add-Content $temp"concatdistort.txt" "file 'distort2.png'"
Add-Content $temp"concatdistort.txt" "duration 0.4"
Add-Content $temp"concatdistort.txt" "file 'black.png'"
Add-Content $temp"concatdistort.txt" "duration 0.834"
Add-Content $temp"concatdistort.txt" "file 'distort3.png'"
Add-Content $temp"concatdistort.txt" "duration 0.467"
Add-Content $temp"concatdistort.txt" "file 'distort4.png'"
Add-Content $temp"concatdistort.txt" "duration 0.4"
Add-Content $temp"concatdistort.txt" "file 'distort5.png'"
Add-Content $temp"concatdistort.txt" "duration 0.467"

# Delete input file
if (Test-Path $video) {
    Remove-Item $video
}

# Concat distort
ffmpeg -f concat -i $concatdistort -i $randomSound -c:v libx264 -c:a aac -pix_fmt yuv420p -y "$video"

