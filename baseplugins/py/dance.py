# Dance plugin for YTP+++
import os
import sys
import subprocess
from plugininterface import *

# Define library types
# ytpplus.addLibraryType(LibraryRootType.Video, LibrarySubType("WTF Boom"))

# Exported functions
if sys.argv[1] == "query":
    # Format: 0 = Video, 1 = Audio followed by a colon, the name of the library item followed by a colon, and finally the base name (without spaces)
    # Use _ for spaces
    # Add a semi-colon if there are more library items
    # Leave blank for no library items
    # print("1:Distort:distort;1:Sparta_Remix:spartaremix;1:Carriers:carriers;0:WTF Boom:wtfboom")
    pass
elif sys.argv[1] == "generate":
    temp = os.path.join(os.getcwd(), "..", "temp", "temp.mp4")
    temp2 = os.path.join(os.getcwd(), "..", "temp", "temp2.mp4")
    if os.path.exists(temp):
        os.remove(temp)
    if os.path.exists(temp2):
        os.remove(temp2)
    if os.path.exists(sys.argv[2]):
        os.rename(sys.argv[2], temp)
    randomSound = ytpplus.pickRandomLibraryItem("Music")
    randomTime = random.randint(3, 19) / 10
    commands = []
    useOriginalAudioRoll = random.randint(0, 4) == 0
    if randomSound != None and not useOriginalAudioRoll:
        commands.append("-i \"" + temp + "\" -an -c:v copy -to " + str(randomTime) + " -y \"" + temp2 + "\"")
        commands.append("-i \"" + temp2 + "\" -vf reverse -y \"" + temp + "\"")
        commands.append("-i \"" + temp + "\" -i \"" + temp2 + "\" -i \"" + randomSound.path + "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1,setpts=.5*PTS[out]\" -map [out] -map 2:a -ar 44100 -ac 2 -disposition:a:0 default -shortest -map_metadata -1 -y \"" + sys.argv[2] + "\"")
    else:
        commands.append("-i \"" + temp + "\" -c:v copy -c:a aac -b:a 192k -to " + str(randomTime) + " -y \"" + temp2 + "\"")
        commands.append("-i \"" + temp2 + "\" -vf reverse -y \"" + temp + "\"")
        commands.append("-i \"" + temp + "\" -i \"" + temp2 + "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1,setpts=.5*PTS[out];[0:a][1:a][0:a][1:a][0:a][1:a][0:a][1:a]concat=n=8:v=0:a=1,atempo=2[outa]\" -map [out] -map [outa] -map_metadata -1 -y \"" + sys.argv[2] + "\"")
    for command in commands:
        subprocess.run("ffmpeg " + command)
