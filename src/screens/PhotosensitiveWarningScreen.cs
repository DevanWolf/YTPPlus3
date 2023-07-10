using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// This screen displays a warning about photosensitive epilepsy.
    /// </summary>
    public class PhotosensitiveWarningScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Photosensitive Warning";
        public int layer { get; } = 99;
        public ScreenType screenType { get; set; } = ScreenType.Drawn;
        public int currentPlacement { get; set; } = -1;
        private int overlayOpacity = 255;
        private int lastTextOpacity = 255;
        private bool accepted = false;
        private bool fadingIn = false;
        private bool textFadedIn = false;
        private double timeText = 0;
        // shamelessly copied from tutorial screen
        private BackgroundWorker updateWorker;
        private void UpdateCheckThread(object? sender, DoWorkEventArgs e)
        {
            // Check for updates.
            UpdateManager.GetDependencyStatus();
            Global.pluginsLoaded = PluginHandler.LoadPlugins();
            UpdateManager.CheckForUpdates();
            if(UpdateManager.updateAvailable || !Global.pluginsLoaded)
            {
                ScreenManager.PushNavigation("Initial Setup");
                ScreenManager.GetScreen<TutorialScreen>("Initial Setup")?.Show();
                GlobalContent.GetSound("Prompt").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
            }
            // Dispose of worker
            updateWorker.Dispose();
            updateWorker = null;
        }
        private List<string> warningText = new List<string>()
        {
            "WARNING:",
            "A very small percentage of people may experience a seizure",
            "when exposed to certain visual images, including flashing",
            "lights or patterns that may appear during usage of YTP+++.",
            " ",
            "When using YTP+++, please be aware of",
            "this and take any necessary precautions.",
            " ",
            "If you are sensitive to flashing lights,",
            "please do not use this program.",
            " ",
            "Click anywhere to continue."
        };
        public void Show()
        {
        }
        public void Hide()
        {
        }
        public bool Toggle(bool useBool = false, bool toggleTo = false)
        {
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteFont fontMunro = GlobalGraphics.fontMunro;
            if(!fadingIn)
            {
                // Draw black background.
                spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, 255));
                // Draw text center aligned.
                for (int i = 0; i < warningText.Count; i++)
                {
                    string text = warningText[i];
                    Vector2 textSize = fontMunro.MeasureString(text);
                    spriteBatch.DrawString(fontMunro, text, new Vector2(GlobalGraphics.scaledWidth / 2 - textSize.X / 2, GlobalGraphics.Scale(24 + i * 16)), Color.White);
                }
            }
            // Draw overlay over last text.
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, GlobalGraphics.Scale(24 + (warningText.Count - 1) * 16), GlobalGraphics.scaledWidth, GlobalGraphics.Scale(16)), new Color(0, 0, 0, lastTextOpacity));
            // Draw black overlay.
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, overlayOpacity));
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(!accepted)
            {
                overlayOpacity -= 16;
                if(overlayOpacity <= 0)
                {
                    overlayOpacity = 0;
                    // Flash text.
                    if(!textFadedIn)
                    {
                        lastTextOpacity -= 16;
                        if(lastTextOpacity <= 0)
                        {
                            lastTextOpacity = 0;
                            textFadedIn = true;
                            timeText = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                    lastTextOpacity = (int)(Math.Sin((gameTime.TotalGameTime.TotalMilliseconds - timeText) / 100) * 128 + 128);
                    if(handleInput)
                    {
                        if (MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                        {
                            accepted = true;
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                    }
                }
            }
            else
            {
                lastTextOpacity -= 16;
                overlayOpacity += 16;
                if (overlayOpacity >= 255)
                {
                    overlayOpacity = 255;
                    lastTextOpacity = 0;
                    ConsoleOutput.WriteLine("User acknowledged photosensitive warning.", Color.LightGreen);
                    ScreenManager.PushNavigation("Main Menu");
                    ScreenManager.PushNavigation("Content");
                    ScreenManager.PushNavigation("Video");
                    ScreenManager.PushNavigation("Background");
                    ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                    ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Show();
                    ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                    ScreenManager.GetScreen<BackgroundScreen>("Background")?.Show();
                    overlayOpacity = 255;
                    updateWorker = new BackgroundWorker();
                    updateWorker.DoWork += UpdateCheckThread;
                    updateWorker.RunWorkerAsync();
                    Global.ready = true;
                    Global.readyTime = gameTime.TotalGameTime.TotalMilliseconds;
                    // Play startup sound.
                    GlobalContent.GetSound("Start").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    fadingIn = true;
                    accepted = false;
                }
            }
            if (fadingIn)
            {
                overlayOpacity -= 16;
                if (overlayOpacity <= 0)
                {
                    overlayOpacity = 0;
                    screenType = ScreenType.Hidden;
                    return false;
                }
            }
            return true;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
        }
    }
}
