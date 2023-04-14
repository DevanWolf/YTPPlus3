# Plugin interface for YTP+++
import os
import random
import sys
import json

# There are two root types, video and audio
class LibraryRootType:
    Video = 0
    Audio = 1

# And a variable number of subtypes, we'll store them as strings
class LibrarySubType:
    def __init__(self, name):
        self.name = name


# The library type class stores both the root type and the subtype
class LibraryType:
    def __init__(self, rootType, subType):
        self.rootType = rootType
        self.subType = subType

# The library item class stores the type and the path
class LibraryItem:
    def __init__(self, type, path):
        self.type = type
        self.path = path
    
class YTPPlusPlusPlus:
    def __init__(self):
        # Rebuild the library
        self.subTypes = {
            LibraryRootType.Video: [
                LibrarySubType("Renders"),
                LibrarySubType("Materials"),
                LibrarySubType("Transitions"),
                LibrarySubType("Intros"),
                LibrarySubType("Outros"),
                LibrarySubType("Overlays"),
            ],
            LibraryRootType.Audio: [
                LibrarySubType("SFX"),
                LibrarySubType("Music"),
            ],
        }
        self.library = []
        for rootType in self.subTypes:
            for subType in self.subTypes[rootType]:
                # Get the path to the library item
                path = "../library/" + ("video" if rootType == LibraryRootType.Video else "audio") + "/" + subType.name.lower() + "/"
                # Add a library item for each file in the path
                for file in os.listdir(path):
                    self.library.append(LibraryItem(LibraryType(rootType, subType), os.path.join(path, file)))
        # YTP+++ will always save its configuration to JSON, so we'll load it here
        # The configuration will be passed to the plugin when necessary
        self.config = None
        with open("../Options.json", "r") as file:
            self.config = json.load(file)
    def pickRandomLibraryItem(self, type):
        # Pick a random library item of the specified type
        # We'll use this to get a random transition, intro, outro, etc.
        # First, sort the library by type
        library = []
        for item in self.library:
            if item.type.subType.name == type:
                library.append(item)
        # Then, pick a random item
        if len(library) == 0:
            return None
        return library[random.randint(0, len(library) - 1)]
    def getLibraryItems(self, type):
        # Get all library items of the specified type
        # We'll use this to get all transitions, intros, outros, etc.
        # First, sort the library by type
        library = []
        for item in self.library:
            if item.type == type:
                library.append(item)
        # Then, return the library
        return library
    def addLibraryType(self, rootType, subType):
        # Add a new library type
        self.subTypes[rootType].append(subType)
    def addLibraryItem(self, type, path):
        # Add a new library item
        self.library.append(LibraryItem(type, path))

# Global variable
ytpplus = YTPPlusPlusPlus()
