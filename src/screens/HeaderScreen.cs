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
    public class HeaderScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Header";
        public int layer { get; } = 4;
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
            offset = new(GlobalGraphics.Scale(-320), 0); // from left to right
            tween.TweenTo(this, t => t.offset, new Vector2(0, 0), 0.5f)
                .Easing(EasingFunctions.ExponentialOut);
            showing = true;
        }
        public void Hide()
        {
            toggle = false;
            offset = new(0, 0); // from right to left
            tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320), 0), 0.5f)
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
            if (screenType == ScreenType.Drawn && hiding && offset.X == GlobalGraphics.Scale(-320))
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
            if(handleInput)
            {
                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(4) && MouseInput.MouseState.X <= GlobalGraphics.Scale(68) && MouseInput.MouseState.Y >= GlobalGraphics.Scale(8) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(32))
                {
                    if(MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                    {
                        // Play sound
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f); 
                        Hide();
                        ScreenManager.PushNavigation("April Fools");
                        ScreenManager.GetScreen<AprilFoolsScreen>("April Fools")?.Show();
                        ScreenManager.GetScreen<HeaderScreen>("Header")?.Hide();
                        ScreenManager.GetScreen<ContentScreen>("Content")?.Hide();
                        ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                        ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Hide();
                        ScreenManager.GetScreen<TutorialScreen>("Tutorial")?.Hide();
                        return true;
                    }
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            // Logo.
            Texture2D logobg = GlobalContent.GetTexture("LogoBG");
            spriteBatch.Draw(logobg, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(8), GlobalGraphics.Scale(logobg.Width), GlobalGraphics.Scale(logobg.Height)), Color.White);
            Texture2D logo = GlobalContent.GetTexture("Logo");
            spriteBatch.Draw(logo, new Rectangle(GlobalGraphics.Scale(9), GlobalGraphics.Scale(10), GlobalGraphics.Scale(logo.Width), GlobalGraphics.Scale(logo.Height)), Color.White);
            // Draw rendering progress
            if(Global.generatorFactory.progressText != "")
            {
                SpriteFont font = GlobalContent.GetFont("MunroSmall");
                string rendering = SaveData.saveValues["ProjectTitle"];
                // measure to center horizontally (one on top of the other)
                Vector2 renderingSize = font.MeasureString(rendering);
                Vector2 progressSize = font.MeasureString(Global.generatorFactory.progressText != "" ? Global.generatorFactory.progressText : (Global.generatorFactory.failureReason != "" ? Global.generatorFactory.failureReason : Global.generatorFactory.progress + "%"));
                spriteBatch.DrawString(font, rendering, new Vector2(GlobalGraphics.Scale(320/2) - renderingSize.X/2 + GlobalGraphics.Scale(1), GlobalGraphics.Scale(8 + 1)), Color.Black);
                spriteBatch.DrawString(font, Global.generatorFactory.progressText, new Vector2(GlobalGraphics.Scale(320/2) - progressSize.X/2 + GlobalGraphics.Scale(1), GlobalGraphics.Scale(8 + 1) + renderingSize.Y), Color.Black);
                spriteBatch.DrawString(font, rendering, new Vector2(GlobalGraphics.Scale(320/2) - renderingSize.X/2, GlobalGraphics.Scale(8)), Color.White);
                spriteBatch.DrawString(font, Global.generatorFactory.progressText, new Vector2(GlobalGraphics.Scale(320/2) - progressSize.X/2, GlobalGraphics.Scale(8) + renderingSize.Y), Color.White);
            }
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
            // Logo.
            GlobalContent.AddTexture("LogoBG", contentManager.Load<Texture2D>("graphics/bannerbg"));
            GlobalContent.AddTexture("Logo", contentManager.Load<Texture2D>("graphics/logo"));
            Show();
        }
    }
}
