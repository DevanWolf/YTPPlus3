# BOOM plugin for YTP+++

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    Write-Host "0:BOOM:boom:Green_screen_for_25%_of_the_video,_then_an_explosion."
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a YTP+++ plugin."
    Write-Host "Usage: boom.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options> <settingcount> [<settingname> <settingvalue> ... ...]"
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
$temp2 = Join-Path $temp "temp2.mp4"
$temp3 = Join-Path $temp "temp3.mp4"
$temp4 = Join-Path $temp "temp4.mp4"
$frames = Join-Path $temp "frames"

# Load options as json
$optionsjson = Get-Content $options | ConvertFrom-Json

# Get options
$videoWidth = $optionsjson.VideoWidth
$videoHeight = $optionsjson.VideoHeight
$transitionsEnabled = $optionsjson.TransitionsEnabled
$transitionChance = $optionsjson.TransitionChance
$transitionEffects = $optionsjson.TransitionEffects
$transitionEffectChance = $optionsjson.TransitionEffectChance

# Parse options
$videoWidth = [int]$videoWidth
$videoHeight = [int]$videoHeight
$transitionsEnabled = $transitionsEnabled -eq "true"
$transitionChance = [int]$transitionChance
$transitionEffects = $transitionEffects -eq "true"
$transitionEffectChance = [int]$transitionEffectChance

# Set options
$isTransition = (($transitionsEnabled -and $transitionEffects) -and (Get-Random -Minimum 0 -Maximum 100) -lt $transitionChance) -and (Get-Random -Minimum 0 -Maximum 100) -lt $transitionEffectChance
$videoScale = "$videoWidth" + "x" + "$videoHeight"

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
if (Test-Path $temp4) {
    Remove-Item $temp4
}

# Delete frames directory and its contents
#if (Test-Path $frames) {
#    Remove-Item $frames -Recurse
#}

# Create frames directory
#if (-not (Test-Path "$frames")) {
#    New-Item -Path $frames -ItemType Directory
#}

# Pick random video from $library .mp4 .webm .mov .avi .mkv .wmv
$librarypath = Join-Path $library video
$librarypath = Join-Path $librarypath boom
$librarypath = Join-Path $librarypath *
$randomVideo = Get-ChildItem -Path $librarypath -File -Include *.mp4, *.webm, *.mov, *.avi, *.mkv, *.wmv | Get-Random

# Make randomsound "" if it doesn't exist
if ($null -eq $randomVideo) {
    Write-Host "No random video found."
    exit 0
}
else {
    $randomVideo = $randomVideo.FullName.Trim('"')
}

# Delete input file
if (Test-Path $video) {
    Remove-Item $video
}

# Pick random video from $library .mp4 .webm .mov .avi .mkv .wmv
$librarypath2 = Join-Path $library video
if ($isTransition -eq $true) {
    Write-Host("Using transition")
    $librarypath2 = Join-Path $librarypath2 transitions
}
else {
    Write-Host("Using material")
    $librarypath2 = Join-Path $librarypath2 materials
}
$librarypath2 = Join-Path $librarypath2 *
$randomMaterial = Get-ChildItem -Path $librarypath2 -File -Include *.mp4, *.webm, *.mov, *.avi, *.mkv, *.wmv | Get-Random

# Get length
$materialLength = Invoke-Command -ScriptBlock {&$ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 $randomMaterial}

# Extract only the top left corner pixel frames
#Invoke-Command -ScriptBlock {&$ffmpeg -i $randomVideo -filter_complex "crop=2:2:0:0" $frames\frame%d.png}
#$frameCount = (Get-ChildItem -Path $frames -File).Count

# Find video with last pure green frame
#$pureGreenLastFrame = [math]::Round($frameCount / 2)
#$done = $false
#if (Get-Command magick -ErrorAction SilentlyContinue) {
#    for ($i = $frameCount; $i -gt 0; $i--) {
#        $frame = Join-Path $frames "frame$i.png"
#        # Green pixel at top left corner
#        $pixel = Invoke-Command -ScriptBlock {&$magick $frame -format "%[pixel:p{0,0}]" info:}
#        # You can be up to 20r/20b off to compensate for compression artifacts
#        for ($r = 0; $r -le 20; $r++) {
#            for ($b = 0; $b -le 20; $b++) {
#                if ($pixel -eq "srgb($r,255,$b)") {
#                    $pureGreenLastFrame = $i
#                    $done = $true
#                    break
#                }
#            }
#            if ($done) {
#                break
#            }
#        }
#        if ($done) {
#            break
#        }
#    }
#}
# None of the above
$frameCount = Invoke-Command -ScriptBlock {&$ffprobe -v error -count_frames -select_streams v:0 -show_entries stream=nb_read_frames -of default=nokey=1:noprint_wrappers=1 $randomVideo}
$pureGreenLastFrame = [math]::Round($frameCount / 4)

# Trim random video to last pure green frame (set width/height to match input video)
$ss = 0
$to = $pureGreenLastFrame / 30
Invoke-Command -ScriptBlock {&$ffmpeg -i $randomVideo -ss $ss -to $to -c copy -y $temp2}

$ss2 = 0
$to2 = $to - $ss
if (-not $isTransition)
{
    # Trim random material to start at random point without trimming temp2 length
    $ss2 = Get-Random -Minimum 0 -Maximum ($materialLength - ($to - $ss))
    $to2 = $ss2 + ($to - $ss)
}
Invoke-Command -ScriptBlock {&$ffmpeg -i $randomMaterial -ss $ss2 -to $to2 -vf scale=$videoScale,setsar=1:1,fps=fps=30 -c:a copy -y $temp1}

# Trim random video from last pure green frame to end
$ss3 = $pureGreenLastFrame / 30
$to3 = $frameCount / 30
Invoke-Command -ScriptBlock {&$ffmpeg -i $randomVideo -ss $ss3 -to $to3 -vf scale=$videoScale,setsar=1:1,fps=fps=30 -y $temp3}

# Chroma key random video with $temp1 and mix audio between the two
# Convert to x264 mp4
Invoke-Command -ScriptBlock {&$ffmpeg -i $temp2 -i $temp1 -filter_complex "[0:v]chromakey=0x00FF00:0.1:0.0[ckout];[1:v][ckout]overlay[out];[1:a][0:a]amix=inputs=2:duration=first:dropout_transition=2[outa]" -map "[out]" -map "[outa]" -c:v libx264 -crf 18 -preset veryfast -c:a aac -y $temp4}

# Concatenate $temp4 and $temp3
Invoke-Command -ScriptBlock {&$ffmpeg -i $temp4 -i $temp3 -filter_complex "[0:v:0][0:a:0][1:v:0][1:a:0]concat=n=2:v=1:a=1[outv][outa]" -map "[outv]" -map "[outa]" -fps_mode vfr -y $video}
