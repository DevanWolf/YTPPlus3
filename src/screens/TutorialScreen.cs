using System;
using System.Collections.Generic;
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
        public string title { get; } = "Tutorial";
        public int layer { get; } = 6;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
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
            if (screenType == ScreenType.Drawn && hiding && offset.Y == GlobalGraphics.Scale(240))
            {
                screenType = ScreenType.Hidden;
                hiding = false;
            }
            else if (screenType == ScreenType.Hidden && showing)
            {
                screenType = ScreenType.Drawn;
                showing = false;
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
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, null);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Tutorial window
            GlobalContent.AddTexture("TutorialWindow", contentManager.Load<Texture2D>("graphics/tutorialwindow"));
            controller.Add("Button1", new Button("Next Page", "", new Vector2(237+32+2, 217+12-6), (int i) => {
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
            controller.Add("Button2", new Button("Next Page", "", new Vector2(237+32+320+2, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-640), 0), 0.5f)
                            .Easing(EasingFunctions.ExponentialOut);
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
                        return true;
                }
                return false;
            }));
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
