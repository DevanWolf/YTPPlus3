using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class PluginsPage : IPage
    {
        public string Name { get; } = "Plugins";
        public string Tooltip { get; } = "Add, remove, or modify external plugins.";
        private readonly InteractableController controller = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
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
            // Add labels
            controller.Add("Plugin7", new Label("be sure to ask on the YTP+ Hub Discord.", new Vector2(139, 60+12*7)));
            controller.Add("Plugin6", new Label("If you have any questions or need help,", new Vector2(139, 60+12*6)));
            controller.Add("Plugin5", new Label("support is deprecated.", new Vector2(139, 60+12*4)));
            controller.Add("Plugin4", new Label("the most supported, while Node.js", new Vector2(139, 60+12*3)));
            controller.Add("Plugin3", new Label("folder. .bat and .ps1 plugins are", new Vector2(139, 60+12*2)));
            controller.Add("Plugin2", new Label("added or removed from the plugins", new Vector2(139, 60+12)));
            controller.Add("Plugin1", new Label("To toggle plugins, they must be", new Vector2(139, 60)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}