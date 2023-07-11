using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Tweening;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// This is the about screen.
    /// </summary>
    public class TutorialScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Initial Setup";
        public int layer { get; } = 7;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        private bool check = false;
        private bool check2 = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        private readonly InteractableController controller = new();
        public void Show()
        {
            toggle = true;
            offset = new(0, GlobalGraphics.Scale(240)); // from bottom to top
            tween.TweenTo(this, t => t.offset, new Vector2(0, 0), 0.5f)
                .Easing(EasingFunctions.ExponentialOut);
            showing = true;
        }
        public void Hide()
        {
            toggle = false;
            offset = new(0, 0); // from top to bottom
            tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(240)), 0.5f)
                .Easing(EasingFunctions.ExponentialOut);
            hiding = true;
        }
        public bool Toggle(bool useBool = false, bool toggleTo = false)
        {
            if (useBool)
            {
                if (toggleTo)
                {
                    Show();
                    return true;
                }
                else
                {
                    Hide();
                    return false;
                }
            }
            else
            {
                if (toggle)
                {
                    Hide();
                    return false;
                }
                else
                {
                    Show();
                    return true;
                }
            }
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // When animation is done, set screen type
            if (hiding && offset.Y == GlobalGraphics.Scale(240))
            {
                screenType = ScreenType.Hidden;
                hiding = false;
            }
            else if (showing)
            {
                screenType = ScreenType.Drawn;
                showing = false;
                hiding = false;
            }
            // Tween
            tween.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if(hiding || screenType == ScreenType.Hidden)
                return false;
            // Store mouse state
            MouseState mouseState = MouseInput.MouseState;
            // Offset mouse position
            MouseState offsetMouseState = new MouseState(mouseState.X - (int)offset.X, mouseState.Y - (int)offset.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
            // Set new mouse state
            MouseInput.MouseState = offsetMouseState;
            // Repeat for last mouse state
            MouseState lastMouseState = MouseInput.LastMouseState;
            MouseState offsetLastMouseState = new MouseState(lastMouseState.X - (int)offset.X, lastMouseState.Y - (int)offset.Y, lastMouseState.ScrollWheelValue, lastMouseState.LeftButton, lastMouseState.MiddleButton, lastMouseState.RightButton, lastMouseState.XButton1, lastMouseState.XButton2);
            MouseInput.LastMouseState = offsetLastMouseState;
            // Update controller
            if(controller.Update(gameTime, handleInput))
                return true;
            // Revert
            MouseInput.MouseState = mouseState;
            MouseInput.LastMouseState = lastMouseState;
            return false;
        }
        public List<string>[] tutorialText = new List<string>[3]
        {
            new List<string>()
            { // PAGE 1
                "Welcome to YTP+++!",
                "This initial setup will help you get started with the program.",
                "",
                "This screen may be shown if there is an issue with your setup.",
                "Otherwise, this may be the first boot of the program.",
                "On the next page, we will check prerequisites.",
                "",
                "The following software is required to run YTP+++:",
                " - FFmpeg",
                " - FFprobe",
                "",
                "The following software is optional, but recommended:",
                " - ImageMagick",
                "",
                "If you have any issues, please refer to the installation guide.",
                "You may check console at any time by pressing ~ (tilde).",
                "Click \"Next Page\" to continue."
            },
            new List<string>()
            { // PAGE 2
                "Below shows the status of the prerequisites.",
                "On the next page, we will check for updates.",
                "",
                "Required software:",
                " - FFmpeg: %FFMPEG%",
                " - FFprobe: %FFPROBE%",
                "",
                "Optional software:",
                " - ImageMagick: %IMAGEMAGICK%",
                "",
                "If any of the required software is missing, please install it.",
                "You must add the software to your PATH environment variable.",
                "Instructions may be found in the installation guide.",
                "",
                "Click \"Next Page\" to continue if the required software is installed."
            },
            new List<string>()
            { // PAGE 3
                "Next, we will check for updates.",
                "",
                " - Update check: %UPDATECHECK%",
                "",
                "Download the latest version above if an update is available.",
                "",
                "Information about YTP+++ and its features may be found in the",
                "YTP+ Hub Discord.",
                "To refer back to this setup, it may be accessed at any time from",
                "the \"Help\" tab.",
                "",
                "Enjoy using YTP+++, and be sure to report any issues!",
                "Click \"Continue\" to load plugins and proceed to YTP+++.",
                "",
                "If there are still issues, the continue button will be disabled.",
                "Broken plugins may be the culprit, if the installation is correct.",
                "Try restarting the program without plugins, or reinstalling.",
                "You may also check console with ~ (tilde) for troubleshooting."
            }
        };
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            // Tutorial window
            Texture2D tutorialWindow = GlobalContent.GetTexture("TutorialWindow");
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8+320), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8+640), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            controller.Draw(gameTime, spriteBatch);
            // Draw the center title bar text
            Vector2 titleSize1 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 1/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 1/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize1.X / 2, (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize2 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 2/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 2/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize2.X / 2 + GlobalGraphics.Scale(320), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize3 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 3/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 3/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize3.X / 2 + GlobalGraphics.Scale(640), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            // Draw tutorial text
            for(int i = 0; i < 3; i++)
            {
                int offsetText = 0;
                for(int j = 0; j < tutorialText[i].Count; j++)
                {
                    string dummyText = tutorialText[i][j];
                    dummyText = dummyText.Replace("%FFMPEG%", "Checking...");
                    dummyText = dummyText.Replace("%FFPROBE%", "Checking...");
                    dummyText = dummyText.Replace("%IMAGEMAGICK%", "Checking...");
                    dummyText = dummyText.Replace("%UPDATECHECK%", "Checking...");
                    Vector2 textSize = GlobalGraphics.fontMunroSmall.MeasureString(dummyText);
                    spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, dummyText, new Vector2(GlobalGraphics.Scale(8+16+1+320*i), GlobalGraphics.Scale(60+offsetText+1)), Color.Black);
                    spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, dummyText, new Vector2(GlobalGraphics.Scale(8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.White);
                    // Draw red overlay if prerequisite is not met
                    if(GlobalGraphics.scale == 2)
                    {
                        if(tutorialText[i][j].Contains("Not installed"))
                        {
                            int offset = 0;
                            switch(j)
                            {
                                case 4:
                                    offset = 43;
                                    break;
                                case 5:
                                    offset = 47;
                                    break;
                                case 8:
                                    offset = 64;
                                    break;
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Not installed", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.OrangeRed);
                        }
                        // Draw green overlay if prerequisite is met
                        if(tutorialText[i][j].Contains("Installed"))
                        {
                            int offset = 0;
                            switch(j)
                            {
                                case 4:
                                    offset = 43;
                                    break;
                                case 5:
                                    offset = 47;
                                    break;
                                case 8:
                                    offset = 64;
                                    break;
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Installed", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.LimeGreen);
                        }
                        // Draw green overlay if update is available
                        if(tutorialText[i][j].Contains("Available"))
                        {
                            int offset = 68;
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Available", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.LimeGreen);
                        }
                        // Draw blue overlay if up to date
                        if(tutorialText[i][j].Contains("Up to date"))
                        {
                            int offset = 68;
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Up to date", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.SkyBlue);
                        }
                        // Draw red overlay if update check failed
                        if(tutorialText[i][j].Contains("Failed"))
                        {
                            int offset = 68;
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Failed", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.OrangeRed);
                        }
                        // Draw yellow overlay if checking
                        if(dummyText.Contains("Checking..."))
                        {
                            int offset = 68;
                            switch(j)
                            {
                                case 4:
                                    offset = 43;
                                    break;
                                case 5:
                                    offset = 47;
                                    break;
                                case 8:
                                    offset = 64;
                                    break;
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Checking...", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.Yellow);
                        }
                    }
                    offsetText += 8;
                }
            }
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, null);
        }
        private BackgroundWorker dependencyWorker;
        private void DependencyCheckThread(object? sender, DoWorkEventArgs e)
        {
            // Get dependencies.
            UpdateManager.GetDependencyStatus();
            foreach(string s in UpdateManager.GetDependencies().Split('\n'))
            {
                ConsoleOutput.WriteLine(s);
            }
            for (int h = 0; h < tutorialText.Length; h++)
            {
                for (int j = 0; j < tutorialText[h].Count; j++)
                {
                    tutorialText[h][j] = tutorialText[h][j].Replace("%FFMPEG%", UpdateManager.ffmpegInstalled ? "Installed" : "Not installed");
                    tutorialText[h][j] = tutorialText[h][j].Replace("%FFPROBE%", UpdateManager.ffprobeInstalled ? "Installed" : "Not installed");
                    tutorialText[h][j] = tutorialText[h][j].Replace("%IMAGEMAGICK%", UpdateManager.imagemagickInstalled ? "Installed" : "Not installed");
                }
            }
            // Dispose of worker
            dependencyWorker.Dispose();
            dependencyWorker = null;
        }
        private BackgroundWorker updateWorker;
        private void UpdateCheckThread(object? sender, DoWorkEventArgs e)
        {
            // Check for updates.
            UpdateManager.CheckForUpdates();
            for (int h = 0; h < tutorialText.Length; h++)
            {
                for (int j = 0; j < tutorialText[h].Count; j++)
                {
                    tutorialText[h][j] = tutorialText[h][j].Replace("%UPDATECHECK%", UpdateManager.updateFailed ? "Failed (v" + Global.productVersion + ")" : (UpdateManager.updateAvailable ? "Available (v" + Global.productVersion + " -> " + UpdateManager.updateTag + ")" : "Up to date (v" + Global.productVersion + ")"));
                }
            }
            // Dispose of worker
            updateWorker.Dispose();
            updateWorker = null;
        }
        private bool check3 = false;
        private BackgroundWorker pluginWorker;
        private void PluginCheckThread(object? sender, DoWorkEventArgs e)
        {
            if(!Global.pluginsLoaded)
                Global.pluginsLoaded = PluginHandler.LoadPlugins();
            if(Global.pluginsLoaded)
            {
                SaveData.saveValues["FirstBoot"] = "false";
                SaveData.Save();
                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                toggle = false;
                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-640), GlobalGraphics.Scale(240)), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
                hiding = true;
                ScreenManager.PushNavigation("Main Menu");
                ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Show();
                ScreenManager.PushNavigation("Video");
                ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                ScreenManager.PushNavigation("Content");
                ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                ScreenManager.PushNavigation("Socials");
                ScreenManager.GetScreen<SocialScreen>("Socials")?.Show();
            }
            else
            {
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                ScreenManager.PushNavigation("Console");
                ScreenManager.GetScreen<ConsoleScreen>("Console")?.Show();
            }
            check3 = false;
            // Dispose of worker
            pluginWorker.Dispose();
            pluginWorker = null;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Tutorial window
            GlobalContent.AddTexture("TutorialWindow", contentManager.Load<Texture2D>("graphics/tutorialwindow"));
            // PAGE 1
            controller.Add("Button1", new Button("Next Page", "", new Vector2(237+32+2, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        // Get dependencies.
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        if(!check)
                        {
                            check = true;
                            dependencyWorker = new BackgroundWorker();
                            dependencyWorker.DoWork += DependencyCheckThread;
                            dependencyWorker.RunWorkerAsync();
                        }
                        tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320), 0), 0.5f)
                            .Easing(EasingFunctions.ExponentialOut);
                        return true;
                }
                return false;
            }));
            // PAGE 2
            controller.Add("Button2", new Button("Next Page", "", new Vector2(237+32+320+2, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        if(UpdateManager.ffmpegInstalled && UpdateManager.ffprobeInstalled)
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            if(!check2)
                            {
                                check2 = true;
                                updateWorker = new BackgroundWorker();
                                updateWorker.DoWork += UpdateCheckThread;
                                updateWorker.RunWorkerAsync();
                            }
                            tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-640), 0), 0.5f)
                                .Easing(EasingFunctions.ExponentialOut);
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                        return true;
                }
                return false;
            }));
            controller.Add("Button3", new Button("Previous Page", "", new Vector2(28+32+320-4, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(0), 0), 0.5f)
                            .Easing(EasingFunctions.ExponentialOut);
                        return true;
                }
                return false;
            }));
            // PAGE 3
            controller.Add("Button4", new Button("Previous Page", "", new Vector2(28+32+640-4, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320), 0), 0.5f)
                            .Easing(EasingFunctions.ExponentialOut);
                        return true;
                }
                return false;
            }));
            controller.Add("Button5", new Button("Continue", "", new Vector2(238+32+640+5, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        if(!check3)
                        {
                            check3 = true;
                            pluginWorker = new BackgroundWorker();
                            pluginWorker.DoWork += PluginCheckThread;
                            pluginWorker.RunWorkerAsync();
                        }
                        return true;
                }
                return false;
            }));
            controller.Add("Button6", new Button("Download Update", "", new Vector2(228+32+640-4, 78+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        if(UpdateManager.updateUrl != "")
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            UpdateManager.DownloadUpdate();
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                        return true;
                }
                return false;
            }));
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
