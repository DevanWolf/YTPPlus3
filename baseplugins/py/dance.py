# Dance plugin for YTP+++
import os
import sys
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
    ytpplus.printColored("Dance plugin", 255, 0, 255)
    temp = os.path.join(os.getcwd(), "..", "temp", "temp.mp4")
    temp2 = os.path.join(os.getcwd(), "..", "temp", "temp2.mp4")
    temp3 = os.path.join(os.getcwd(), "..", "temp", "temp3.mp4")
    if os.path.exists(temp):
        os.remove(temp)
    if os.path.exists(temp2):
        os.remove(temp2)
    if os.path.exists(temp3):
        os.remove(temp3)
    if os.path.exists(sys.argv[2]):
        os.rename(sys.argv[2], temp)
    randomSound = ytpplus.pickRandomLibraryItem("Music")
    randomTime = random.uniform(0.2, 1.0)
    commands = []
    useOriginalAudioRoll = random.randint(0, 7) == 0
    if randomSound != None and not useOriginalAudioRoll:
        # commands.SetValue("-i \"" + toolBox.TEMP + "temp.mp4\" -an -vf setpts=.5*PTS -t " + randomTime + " -y \"" + toolBox.TEMP + "temp2.mp4\"", 0);
        # commands.SetValue("-i \"" + toolBox.TEMP + "temp2.mp4\" -vf reverse -y \"" + toolBox.TEMP + "temp3.mp4\"", 1);
        # commands.SetValue("-i \"" + toolBox.TEMP + "temp3.mp4\" -i \"" + toolBox.TEMP + "temp2.mp4\" -i \"" + toolBox.MUSIC + randomSound + "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out]\" -map [out] -map 2:a -shortest -y \"" + video + "\"", 2);
        commands.append("-i \"" + temp + "\" -an -vf setpts=.5*PTS -t " + str(randomTime) + " -y \"" + temp2 + "\"")
        commands.append("-i \"" + temp2 + "\" -vf reverse -y \"" + temp3 + "\"")
        commands.append("-i \"" + temp3 + "\" -i \"" + temp2 + "\" -i \"" + randomSound.path + "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out]\" -map [out] -map 2:a -shortest -y \"" + sys.argv[2] + "\"")
    else:
        # same commands but use original audio
        commands.append("-i \"" + temp + "\" -filter_complex \"[0:v]setpts=.5*PTS[v];[0:a]atempo=2.0[a]\" -map [v] -map [a] -y \"" + temp2 + "\"")
        commands.append("-i \"" + temp2 + "\" -vf reverse -y \"" + temp3 + "\"")
        commands.append("-i \"" + temp3 + "\" -i \"" + temp2 + "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out];[0:a][1:a][0:a][1:a][0:a][1:a][0:a][1:a]concat=n=8:v=0:a=1[out2]\" -map [out] -map [out2] -shortest -y \"" + sys.argv[2] + "\"")
    for command in commands:
        ytpplus.runCommand("ffmpeg " + command)
