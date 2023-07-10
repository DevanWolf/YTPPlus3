::YTP+++ Sparta Remix Plugin v2.0
::Written by KiwifruitDev
@ECHO OFF

::Set variables
SET VIDEO=%1
SET WIDTH=%2
SET HEIGHT=%3
SET TEMP=%4
SET FFMPEG=%5
SET FFPROBE=%6
SET MAGICK=%7
SET RESOURCES=%8
SET SOUNDS=%9
SHIFT
SET SOURCES=%9
SHIFT
SET MUSIC=%9
SHIFT
SET LIBRARY=%9

::Query
IF "%VIDEO%"=="query" (
	echo 1:Sparta_Remix:spartaremix:140_BPM_11_11_111_1_1_11_222222_22_222222_222222_22
	exit 0
)

::Error messages
IF "%MUSIC%"=="" (
	echo This is a YTP+++ plugin, it may not be executed outside of the application.
	pause
	exit 0
)
IF "%LIBRARY%"=="" (
	echo This plugin is not compatible with YTP++.
	pause
	exit 0
)

::Delete leftovers
IF EXIST "%TEMP%sparta1.mp4" del /F "%TEMP%sparta1.mp4"
IF EXIST "%TEMP%sparta2.mp4" del /F "%TEMP%sparta2.mp4"
IF EXIST "%TEMP%sparta3.mp4" del /F "%TEMP%sparta3.mp4"

::Find mp3 sparta remix music in directory
Set PrevCwd=%CD%
Cd %LIBRARY%audio\spartaremix\
Set "ExtLst=*.wav,*.mp3,*.ogg,*.m4a,*.flac"
For /F "Delims=" %%A In ('
powershell -Nop -C "(Get-ChildItem * -File -Incl %ExtLst%|Get-Random).FullName"
') Do Set SFX="%%A"
Cd %PrevCwd%

::Slice videos
%FFMPEG% -i "%VIDEO%" -ss 0 -t 0.100 -y "%TEMP%sparta1.mp4"
%FFMPEG% -i "%VIDEO%" -ss 0.140 -t 0.050 -y "%TEMP%sparta2.mp4"
%FFMPEG% -i "%VIDEO%" -ss 0 -t 0.050 -y "%TEMP%sparta3.mp4"
::%FFMPEG% -f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 -f lavfi -i color=c=black:s=%WIDTH%x%HEIGHT%:r=5 -t 0.053 -y "%TEMP%sparta3.mp4"

::Delete video
del /F "%VIDEO%"

::Mix videos into Sparta remix

::Does the audio file exist?
IF EXIST %SFX% (
	ffmpeg -r 30 -i "%TEMP%sparta1.mp4" -i "%TEMP%sparta2.mp4" -i "%TEMP%sparta3.mp4" -i %SFX% -filter_complex "[0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a]concat=n=42:v=1:a=1[remixv][remixa];[remixa]volume=volume=1[ogaudio];[3:a]volume=volume=0.85[sfx];[sfx][ogaudio]amix=inputs=2:duration=shortest:dropout_transition=0:weights='0.3 1':normalize=0[music]" -map "[remixv]" -map "[music]" -fps_mode vfr -y "%VIDEO%"
)
::If not, don't add it
IF NOT EXIST %SFX% (
	ffmpeg -r 30 -i "%TEMP%sparta1.mp4" -i "%TEMP%sparta2.mp4" -i "%TEMP%sparta3.mp4" -filter_complex "[0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a]concat=n=42:v=1:a=1[remixv][remixa];[remixa]volume=volume=1[ogaudio]" -map "[remixv]" -map "[ogaudio]" -fps_mode vfr -y "%VIDEO%"
)


