using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class ExitPage : IPage
    {
        public string Name { get; } = "Exit";
        public string Tooltip { get; } = "Exit the application.";
        private readonly InteractableController controller = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(Global.exiting)
                return false;
            if(Pagination.GetSubPage() == Pagination.GetTopPageCount() - 1)
            {
                // Exit page is always the last
                Global.exiting = true;
                GlobalContent.GetSound("Quit").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Hide();
                ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                ScreenManager.GetScreen<ContentScreen>("Content")?.Hide();
                ScreenManager.GetScreen<HeaderScreen>("Header")?.Hide();
            }
            // Interactable
            if(controller.Update(gameTime, handleInput))
                return true;
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            controller.Draw(gameTime, spriteBatch);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Add label
            //controller.Add("TestLabel", new Label("Test Label", new Vector2(139, 60)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}