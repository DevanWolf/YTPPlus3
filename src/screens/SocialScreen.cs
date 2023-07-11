using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// This is the social screen.
    /// </summary>
    public class SocialScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Socials";
        public int layer { get; } = 5;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        public string tooltip = "";
        private readonly Tweener tween = new();
        // Three buttons, 74-87x/221-233y, 88-101x/221-233y, 102-115x/221-233y
        List<Rectangle> buttons = new()
        {
            new(74, 221, 13, 12),
            new(88, 221, 13, 12),
            new(102, 221, 13, 12)
        };
        public void Show()
        {
            toggle = true;
            offset = new(0, GlobalGraphics.Scale(240)); // from left to right
            tween.TweenTo(this, t => t.offset, new Vector2(0, 0), 0.5f)
                .Easing(EasingFunctions.ExponentialOut);
            showing = true;
        }
        public void Hide()
        {
            toggle = false;
            offset = new(0, 0); // from right to left
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
            if (hiding && offset.Y == GlobalGraphics.Scale(0))
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
            if(handleInput)
            {
                tooltip = "";
                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(74) && MouseInput.MouseState.X <= GlobalGraphics.Scale(119) && MouseInput.MouseState.Y >= GlobalGraphics.Scale(221) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(233))
                {
                    for(int i = 0; i < buttons.Count; i++)
                    {
                        if(buttons[i].Contains(MouseInput.MouseState.X / GlobalGraphics.scale, MouseInput.MouseState.Y / GlobalGraphics.scale))
                        {
                            switch(i)
                            {
                                case 0:
                                    // Discord
                                    tooltip = "Join our Discord server!";
                                    break;
                                case 1:
                                    // Steam
                                    tooltip = "Coming soon!";
                                    break;
                                case 2:
                                    // GitHub
                                    tooltip = "View our GitHub repository!";
                                    break;
                            }
                            if(MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                            {
                                GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                ProcessStartInfo psi = new();
                                switch(i)
                                {
                                    case 0:
                                        // Discord
                                        psi.FileName = "https://discord.gg/8ppmspR6Wh";
                                        break;
                                    case 1:
                                        // Steam
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                        return true; // Not implemented yet
                                    case 2:
                                        // GitHub
                                        psi.FileName = "https://github.com/YTP-Plus/YTPPlusPlusPlus";
                                        break;
                                }
                                psi.UseShellExecute = true;
                                Process.Start(psi);
                            }
                            return true;
                        }
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
            Texture2D socialscreen = GlobalContent.GetTexture("SocialScreen");
            spriteBatch.Draw(socialscreen, new Rectangle(GlobalGraphics.Scale(69), GlobalGraphics.Scale(203), GlobalGraphics.Scale(socialscreen.Width), GlobalGraphics.Scale(socialscreen.Height)), Color.White);
            // Draw text
            SpriteFont munroSmall = GlobalContent.GetFont("MunroSmall");
            spriteBatch.DrawString(munroSmall, title, new Vector2(GlobalGraphics.Scale(83), GlobalGraphics.Scale(204)), Color.White);
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, null);
            if(tooltip != "")
            {
                // Get text size
                Vector2 tooltipSize = GlobalGraphics.fontMunroSmall.MeasureString(tooltip);
                // Position is relative to mouse position but tries to avoid going off screen
                Vector2 position = new(MouseInput.MouseState.Position.X + 10, MouseInput.MouseState.Position.Y + 10);
                // Make sure it doesn't go off the right side of the screen
                if (position.X + tooltipSize.X + GlobalGraphics.Scale(6) > GlobalGraphics.scaledWidth)
                    position.X = GlobalGraphics.scaledWidth - tooltipSize.X - GlobalGraphics.Scale(6);
                // Make sure it doesn't go off the bottom of the screen
                if (position.Y + tooltipSize.Y + GlobalGraphics.Scale(2) > GlobalGraphics.scaledHeight)
                    position.Y = GlobalGraphics.scaledHeight - tooltipSize.Y - GlobalGraphics.Scale(2); 
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle((int)position.X, (int)position.Y, (int)tooltipSize.X + GlobalGraphics.Scale(2), (int)tooltipSize.Y - GlobalGraphics.Scale(2)), new Color(0, 0, 0, 255));
                // White text
                spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, tooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("SocialScreen", contentManager.Load<Texture2D>("graphics/socialscreen"));
        }
    }
}
