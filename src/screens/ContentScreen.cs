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
    /// This is the help screen.
    /// </summary>
    public class ContentScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Content";
        public int layer { get; } = 3;
        public ScreenType screenType { get; set; } = ScreenType.Drawn;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
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
            // Pagination
            if(Pagination.Update(gameTime, handleInput))
                return true;
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            // Main Window
            Texture2D mainwindow = GlobalContent.GetTexture("MainWindow");
            spriteBatch.Draw(mainwindow, new Rectangle(GlobalGraphics.Scale(128), GlobalGraphics.Scale(36), GlobalGraphics.Scale(mainwindow.Width), GlobalGraphics.Scale(mainwindow.Height)), Color.White);
            // Draw the center title bar text.
            string pageTitle = Pagination.GetSubPageName();
            // Center within bounds of x 128 and x 312
            Vector2 titleSize = GlobalGraphics.fontMunroSmall.MeasureString(pageTitle);
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, pageTitle, new Vector2(GlobalGraphics.Scale(220) - titleSize.X / 2, GlobalGraphics.Scale(37)), Color.White);
            // Pagination
            Pagination.Draw(gameTime, spriteBatch);
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
            // Main Window
            GlobalContent.AddTexture("MainWindow", contentManager.Load<Texture2D>("graphics/mainwindow"));
            // Pagination
            Pagination.LoadContent(contentManager, graphicsDevice);
            if(Global.pluginsLoaded)
                Show();
            else
                Hide();
        }
    }
}
