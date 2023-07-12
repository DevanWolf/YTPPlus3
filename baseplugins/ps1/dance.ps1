# Dance plugin for YTP+++

# Default settings
$randomTime = 0.15
$noMusicChance = 50

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    Write-Host ";|Segment_Length:0.15:The_amount_of_time_for_each_segment,_in_seconds.;No_Music_Chance:25:A_percentage_where_the_video_will_have_no_music."
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
$settingcount = $args[13]

$offset = 0
for ($i = 0; $i -lt $settingcount; $i++) {
    $settingname = $args[14 + $i + $offset]
    $settingvalue = $args[15 + $i + $offset]
    if ($settingname -eq "Segment_Length") {
        $randomTime = [float]$settingvalue
    }
    if ($settingname -eq "No_Music_Chance") {
        $noMusicChance = [int]$settingvalue
    }
    $offset++
}

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
$librarypath = Join-Path $librarypath *
$randomSound = Get-ChildItem -Path $librarypath -File -Include *.wav, *.mp3, *.ogg, *.m4a, *.flac | Get-Random

# Make randomsound "" if it doesn't exist
if ($null -ne $randomSound) {
    $randomSound = $randomSound.FullName.Trim('"')
}

# Pick random roll using $noMusicChance out of 100
$useOriginalAudioRoll = Get-Random -Minimum 0 -Maximum 100
$useOriginalAudio = $false
if ($useOriginalAudioRoll -lt $noMusicChance) {
    $useOriginalAudio = $true
}

# Apply effects
if (($null -eq $randomSound) -or $useOriginalAudio) {
    .\ffmpeg.exe -i "$temp1" -t $randomTime -filter_complex "[0:v]setpts=.5*PTS[v];[0:a]atempo=2.0[a]" -map "[v]" -map "[a]" -y "$temp2"
    .\ffmpeg.exe -i "$temp2" -vf reverse -af areverse -y "$temp3"
    .\ffmpeg.exe -i "$temp3" -i "$temp2" -filter_complex "[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out];[0:a][1:a][0:a][1:a][0:a][1:a][0:a][1:a]concat=n=8:v=0:a=1[out2]" -map "[out]" -map "[out2]" -shortest -y "$video"
}
else {
    # Seek audio 1-5 seconds ahead to avoid silence at the beginning
    $seek = Get-Random -Minimum 1 -Maximum 5
    .\ffmpeg.exe -i "$temp1" -an -t $randomTime -vf setpts=.5*PTS -y "$temp2"
    .\ffmpeg.exe -i "$temp2" -vf reverse -y "$temp3"
    .\ffmpeg.exe -i "$temp3" -i "$temp2" -ss $seek -i "$randomSound" -filter_complex "[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out]" -map "[out]" -map 2:a -shortest -y "$video"
}

