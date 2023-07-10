# Rave plugin for YTP+++

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    Write-Host "1:Rave_Music:rave:Music_with_an_intense_vibe_to_it."
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a YTP+++ plugin."
    Write-Host "Usage: rave.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options>"
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
$frames = Join-Path $temp "frames"

# Delete temp files
if (Test-Path $temp1) {
    Remove-Item $temp1
}
if (Test-Path $temp2) {
    Remove-Item $temp2
}

# Pick random sound from $library *.wav, *.mp3, *.ogg, *.m4a, *.flac
$librarypath = Join-Path $library audio
$librarypath = Join-Path $librarypath rave
$librarypath = Join-Path $librarypath *
$randomSound = Get-ChildItem -Path $librarypath -File -Include *.wav, *.mp3, *.ogg, *.m4a, *.flac | Get-Random

# Make randomsound "" if it doesn't exist
if ($null -eq $randomSound) {
    $randomSound = ".\.null"
}
else {
    $randomSound = $randomSound.FullName
}

# Create frames directory
if (-not (Test-Path $frames)) {
    New-Item -Path $frames -ItemType Directory
}

# Rename input file to temp file
if (Test-Path $video) {
    Rename-Item $video "temp.mp4"
}

# Extract frames
.\ffmpeg.exe -i $temp1 -vf fps=30 $frames\frame%0d.png

$frameCount = (Get-ChildItem -Path $frames -File).Count

# Rename frames in random order with prefix _frame
Push-Location $frames
$files = Get-ChildItem -File
$files | Sort-Object {Get-Random} | ForEach-Object -Begin { $count = 0 } -Process { Rename-Item $_ -NewName ("_{0}{1}" -f $count++, $_.Extension) }
Pop-Location

# Rave effect on frames
.\ffmpeg.exe -i $frames\_%0d.png -vf "hue='H=PI*t: s=sin(PI*t)+1.5: enable=between(t,0,10)'" -y $frames\_%0d.png

# Create video from frames
.\ffmpeg.exe -framerate 30 -i $frames\_%0d.png -i $temp1 -map 0:v -map 1:a -c:v libx264 -crf 18 -preset veryfast -y $temp2

# Finalize
if(-not (Test-Path $randomSound)) {
    .\ffmpeg.exe -i $temp2 -filter_complex "[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v];[0:a]atempo=2.0[a1];[0:a]atempo=0.75,areverse[a2];[a1][a2]concat=n=2:v=0:a=1[a]" -map "[v]" -map "[a]" -shortest -y $video
}
else {
    .\ffmpeg.exe -i $temp2 -i $randomSound -filter_complex "[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v]" -map "[v]" -map "1:a" -shortest -y $video
}
