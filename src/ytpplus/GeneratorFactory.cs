using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace YTPPlusPlusPlus
{
    public enum ProgressState
    {
        Idle,
        Parsing,
        Rendering,
        Concatenating,
        Completed,
        Failed
    }
    public class GeneratorFactory
    {
        public Random globalRandom = new Random();
        public BackgroundWorker? vidThreadWorker { get; set; }
        public float progress { get; set; } = 0;
        public ProgressState progressState { get; set; } = ProgressState.Idle;
        public string progressText { get; set; } = "Idle";
        public string failureReason { get; set; } = "";
        public bool generatorActive = false;
        public void VidThread(object? sender, DoWorkEventArgs e)
        {
            if (vidThreadWorker?.CancellationPending == true)
                return;
            // Reset progress state
            generatorActive = true; // first time use
            progress = 0;
            progressState = ProgressState.Parsing;

            // Load library.
            progressText = "Parsing library...";
            LibraryData.Load();

            // Check to ensure that the source pool is not empty.
            if(LibraryData.GetFileCount(DefaultLibraryTypes.Material) == 0)
            {
                ConsoleOutput.WriteLine("No material files found in library.");
                failureReason = "No material files found in library.";
                progressText = failureReason;
                CancelGeneration();
                return;
            }

            // Set global random with seed.
            progressText = "Planting seeds...";
            //int seed = int.Parse(SaveData.saveValues["RandomSeed"]);
            // Convert ProjectTitle to int seed
            string seedString = SaveData.saveValues["ProjectTitle"];
            int seed = 0;
            foreach (char c in seedString)
            {
                seed += (int)c;
            }
            ConsoleOutput.WriteLine("Seed: " + seed);
            globalRandom = new Random(seed);

            string tempOutput = Path.Combine(Utilities.temporaryDirectory, "tempoutput.mp4");
            int maxClips = int.Parse(SaveData.saveValues["MaxClipCount"]);
            
            // Clean up previous temporary files.
            progressText = "Cleaning up...";
            CleanUp();

            if (vidThreadWorker?.CancellationPending == true)
                return;

            // Make sure the temporary directory exists.
            Directory.CreateDirectory(Utilities.temporaryDirectory);

            // Delete the temporary output file if it exists.
            if (File.Exists(tempOutput))
                File.Delete(tempOutput);

            progressText = "Starting generation...";
            progressState = ProgressState.Rendering;

            try
            {
                for (int i = 0; i < maxClips; i++)
                {
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    progressText = "Starting clip " + (i + 1) + " of " + maxClips + "...";
                    bool intro = false;
                    if (i == 0 && bool.Parse(SaveData.saveValues["IntrosEnabled"]))
                    {
                        intro = true;
                        // Add the intro.
                        string introPath = LibraryData.PickRandom(DefaultLibraryTypes.Intro, globalRandom);
                        if(introPath == "")
                        {
                            intro = false;
                        }
                        else
                        {
                            maxClips++;
                            ConsoleOutput.WriteLine("Intro clip enabled, adding 1 to max clips. New max clips is " + maxClips + ".");
                            progress = Convert.ToInt32(((float)i / (float)maxClips));
                            progressText = "Introducing ourselves... (" + (i + 1) + " of " + maxClips + ")";
                            Utilities.CopyVideo(introPath, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                        }
                    }
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    if(!intro)
                    {
                        bool rolledForOverlay = RandomInt(0, 100) <= int.Parse(SaveData.saveValues["OverlayChance"]) && bool.Parse(SaveData.saveValues["OverlaysEnabled"]);
                        string overlayPath = "";
                        progress = Convert.ToInt32(((float)i / (float)maxClips));
                        progressText = "Clipping... (" + (i + 1) + " of " + maxClips + ")";
                        string sourceToPick = LibraryData.PickRandom(DefaultLibraryTypes.Material, globalRandom);
                        if(sourceToPick == "")
                        {
                            ConsoleOutput.WriteLine("No material files found in library.");
                            progressState = ProgressState.Failed;
                            continue;
                        }
                        ConsoleOutput.WriteLine(sourceToPick);
                        float source = float.Parse(Utilities.GetLength(sourceToPick), NumberStyles.Any, new CultureInfo("en-US"));
                        string output = source.ToString("0.#########################", new CultureInfo("en-US"));
                        //ConsoleOutput.WriteLine(Utilities.GetLength(sourceToPick) + " -> " + output + " -> " + float.Parse(output, NumberStyles.Any, new CultureInfo("en-US")));
                        float outputDuration = float.Parse(output, NumberStyles.Any, new CultureInfo("en-US"));
                        ConsoleOutput.WriteLine("CLIP DURATION: " + outputDuration);
                        ConsoleOutput.WriteLine("STARTING CLIP " + "video" + i);
                        float startOfClip = RandomFloat(0f, outputDuration - float.Parse(SaveData.saveValues["MinStreamDuration"]));
                        float endOfClip = startOfClip + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"]), float.Parse(SaveData.saveValues["MaxStreamDuration"]));
                        // Ensure that the start is not less than 0 and the end is not greater than the source length.
                        if (startOfClip < 0)
                            startOfClip = 0;
                        if (endOfClip > outputDuration)
                            endOfClip = outputDuration;
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        // Add an overlay to the video, if rolled for.
                        if(rolledForOverlay)
                        {
                            // Get random overlay.
                            overlayPath = LibraryData.PickRandom(DefaultLibraryTypes.Overlay, globalRandom);
                            if(overlayPath == "")
                            {
                                rolledForOverlay = false;
                            }
                        }
                        ConsoleOutput.WriteLine("Beginning of clip " + i + ": " + startOfClip.ToString("0.#########################", new CultureInfo("en-US")));
                        ConsoleOutput.WriteLine("Ending of clip " + i + ": " + endOfClip.ToString("0.#########################", new CultureInfo("en-US")) + ", in seconds: ");
                        // Insert transition if rolled, ensure that there is a transition as well.
                        //bool alreadySnipped = false;
                        bool rolledForTransition = RandomInt(0, 100) <= int.Parse(SaveData.saveValues["TransitionChance"]) && bool.Parse(SaveData.saveValues["TransitionsEnabled"]);
                        if (!rolledForOverlay && rolledForTransition && LibraryData.GetFileCount(DefaultLibraryTypes.Transition) > 0)
                        {
                            string transitionPath = LibraryData.PickRandom(DefaultLibraryTypes.Transition, globalRandom);
                            if(transitionPath == "")
                            {
                                ConsoleOutput.WriteLine("No transitions found in library.");
                                continue;
                            }
                            progressText = "Transitioning... (" + (i + 1) + " of " + maxClips + ")";
                            Utilities.CopyVideo(transitionPath, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                        }
                        else
                        {
                            //alreadySnipped = true;
                            // No transition, just snip the video.
                            Utilities.SnipVideo(sourceToPick, startOfClip, endOfClip, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                        }
                        if(!rolledForOverlay && rolledForTransition)
                            ConsoleOutput.WriteLine("No transitions found in library.");
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        // Parse overlay if rolled.
                        if(rolledForOverlay)
                        {
                            if(overlayPath == null)
                            {
                                ConsoleOutput.WriteLine("No overlays found in library.");
                                continue;
                            }
                            progressText = "Chroma keying... (" + (i + 1) + " of " + maxClips + ")";
                            ConsoleOutput.WriteLine("Rolled for overlay, adding overlay to clip " + i + ".");
                            // We snip the clip here in case it was a transition
                            //if(!alreadySnipped)
                                //Utilities.SnipVideo(sourceToPick, startOfClip, endOfClip, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                            // Now we'll snip the overlay with another random duration.
                            float overlayDuration = float.Parse(Utilities.GetLength(overlayPath), NumberStyles.Any, new CultureInfo("en-US"));
                            float startOfOverlay = RandomFloat(0f, overlayDuration - float.Parse(SaveData.saveValues["MinStreamDuration"]));
                            float endOfOverlay = startOfOverlay + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"]), float.Parse(SaveData.saveValues["MaxStreamDuration"]));
                            // Ensure we're not going into negative time or over the length of the overlay.
                            if (startOfOverlay < 0)
                                startOfOverlay = 0;
                            if (endOfOverlay > overlayDuration)
                                endOfOverlay = overlayDuration;
                            Utilities.SnipVideo(overlayPath, startOfOverlay, endOfOverlay, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tempoverlay.mp4"));
                            Utilities.OverlayVideo(Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"), Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tempoverlay.mp4"));
                            // The result is a video with an overlay at random points.
                        }
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        if(!rolledForTransition && SaveData.saveValues["PluginTestEnabled"] != "true")
                        {
                            int numberOfPlugins = PluginHandler.GetPluginCount();
                            if(numberOfPlugins > 0)
                            {
                                // Roll for effect
                                if(RandomInt(0, 100) <= int.Parse(SaveData.saveValues["EffectChance"]))
                                {
                                    progressText = "Baking effects... (" + (i + 1) + " of " + maxClips + ")";
                                    // We rolled for an effect, let's pick one.
                                    if(!PluginHandler.PickRandom(globalRandom, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4")))
                                    {
                                        ConsoleOutput.WriteLine("Failed to apply effect to clip " + i + ".");
                                    }
                                }
                            }
                        }
                        else
                        {
                            progressText = "Baking effects... (" + (i + 1) + " of " + maxClips + ")";
                            // Plugin testing will force a specific plugin to be applied to every clip.
                            if(!PluginHandler.PickNamed(SaveData.saveValues["PluginTest"], Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4")))
                            {
                                ConsoleOutput.WriteLine("Failed to apply effect to clip " + i + ".");
                            }
                        }
                    }
                }
                if (bool.Parse(SaveData.saveValues["OutrosEnabled"]))
                {
                    string outroPath = LibraryData.PickRandom(DefaultLibraryTypes.Outro, globalRandom);
                    if(outroPath == "")
                    {
                        ConsoleOutput.WriteLine("No outros found in library.");
                        outroPath = "";
                    }
                    else
                    {
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        maxClips++;
                        progressText = "Closing the film spool... (" + maxClips + " of " + maxClips + ")";
                        ConsoleOutput.WriteLine("Outro clip enabled, adding 1 to max clips. New max clips is " + maxClips + ".");
                        ConsoleOutput.WriteLine("STARTING CLIP " + "video" + maxClips);
                        Utilities.CopyVideo(outroPath, Path.Combine(Utilities.temporaryDirectory, "video" + maxClips + ".mp4"));
                        maxClips++;
                    }
                }
                if (vidThreadWorker?.CancellationPending == true)
                    return;
                // Concatenate all clips into one video.
                ConsoleOutput.WriteLine("Concatenating clips...");
                progressText = "Concatenating clips...";
                progressState = ProgressState.Concatenating;
                Utilities.ConcatenateVideo(maxClips, tempOutput);
                if(vidThreadWorker != null)
                    vidThreadWorker.ReportProgress(100);
                // Save to library if enabled.
                if (bool.Parse(SaveData.saveValues["AddToLibrary"]))
                {
                    ConsoleOutput.WriteLine("Saving to library...");
                    LibraryFile libraryFile = new LibraryFile(SaveData.saveValues["ProjectTitle"], tempOutput, DefaultLibraryTypes.Render);
                    progressText = "Saving to library...";
                    LibraryData.Load(libraryFile);
                }
                progressText = "Completed!";
                progressState = ProgressState.Completed;
                generatorActive = false;
            }
            catch(Exception ex)
            {
                progressState = ProgressState.Failed;
                failureReason = ex.Message;
                progressText = failureReason;
                ConsoleOutput.WriteLine("An error occurred while generating the video.");
                ConsoleOutput.WriteLine(ex.Message);
            }
            //CleanUp();
        }
        public void StartGeneration()
        {
            // Create dummy event handlers for the background worker.
            StartGeneration((sender, e) => { }, (sender, e) => { });
        }
        public void StartGeneration(ProgressChangedEventHandler progressReporter, RunWorkerCompletedEventHandler completedReporter)
        {
            if(vidThreadWorker == null)
            {
                vidThreadWorker = new BackgroundWorker();
                vidThreadWorker.DoWork += VidThread;
                vidThreadWorker.WorkerReportsProgress = true;
                vidThreadWorker.WorkerSupportsCancellation = true;
                vidThreadWorker.ProgressChanged += progressReporter;
                vidThreadWorker.RunWorkerCompleted += completedReporter;
            }
            else
            {
                vidThreadWorker.CancelAsync();
            }
            if(vidThreadWorker.IsBusy)
            {
                ConsoleOutput.WriteLine("Generation already in progress.");
                return;
            }
            vidThreadWorker.RunWorkerAsync();
            ConsoleOutput.WriteLine("Generation started.");
        }
        public void ToggleGeneration(ProgressChangedEventHandler progressReporter, RunWorkerCompletedEventHandler completedReporter)
        {
            StartGeneration(progressReporter, completedReporter);
        }
        public void CancelGeneration(bool user = false)
        {
            if(vidThreadWorker != null)
            {
                // Make sure it's not completed or cancelled already.
                //if(vidThreadWorker.IsBusy)
                vidThreadWorker.CancelAsync();
                generatorActive = false;
                progressState = ProgressState.Failed;
                if(user)
                {
                    failureReason = "Generation cancelled.";
                    progressText = failureReason;
                    ConsoleOutput.WriteLine("Generation cancelled.");
                }

            }
        }
        public float RandomFloat(float min, float max)
        {
            return (float)globalRandom.NextDouble() * (max - min) + min;
        }
        public int RandomInt(int min, int max)
        {
            return globalRandom.Next(min, max);
        }
        public void CleanUp()
        {
            if (Directory.Exists(Utilities.temporaryDirectory))
            {
                try
                {
                    Directory.Delete(Utilities.temporaryDirectory, true);
                    ConsoleOutput.WriteLine("Temporary directory deleted.");
                }
                catch
                {
                    ConsoleOutput.WriteLine("Temporary directory could not be deleted.");
                }
            }
        }
    }
}