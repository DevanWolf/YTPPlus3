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
            controller.Add("Plugin11", new Label("Python plugins have full Library access.", new Vector2(139, 60+12*13)));
            controller.Add("Plugin10", new Label("materials and the \"shared\" folder.", new Vector2(139, 60+12*11)));
            controller.Add("Plugin9", new Label("available for any media other than", new Vector2(139, 60+12*10)));
            controller.Add("Plugin8", new Label("For Node.JS plugins, the Library is not", new Vector2(139, 60+12*9)));
            controller.Add("Plugin7", new Label("legacy \"resources\" folder if needed.", new Vector2(139, 60+12*7)));
            controller.Add("Plugin6", new Label("music, and sound effects. Use the", new Vector2(139, 60+12*6)));
            controller.Add("Plugin5", new Label("available for materials, transitions,", new Vector2(139, 60+12*5)));
            controller.Add("Plugin4", new Label("For Batch plugins, the Library is only", new Vector2(139, 60+12*4)));
            controller.Add("Plugin3", new Label("Add plugin files manually for now.", new Vector2(139, 60+12*2)));
            controller.Add("Plugin2", new Label("Plugins will be managed here soon.", new Vector2(139, 60+12)));
            controller.Add("Plugin1", new Label("This page is under construction.", new Vector2(139, 60)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}