using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class HelpPage : IPage
    {
        public string Name { get; } = "Help";
        public string Tooltip { get; } = "Access help and re-visit the initial setup.";
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
            controller.Add("Help13", new Label("of the project title.", new Vector2(139, 60+12*11)));
            controller.Add("Help12", new Label("temporary output file under the name", new Vector2(139, 60+12*10)));
            controller.Add("Help11", new Label("Saving to the Library will copy the", new Vector2(139, 60+12*9)));
            controller.Add("Help10", new Label("chosen clip and may use a plugin.", new Vector2(139, 60+12*8)));
            controller.Add("Help9", new Label("Overlay clips will play on top of a", new Vector2(139, 60+12*7)));
            controller.Add("Help8", new Label("Transition clips will play in full.", new Vector2(139, 60+12*6)));
            controller.Add("Help7", new Label("min/max stream duration.", new Vector2(139, 60+12*5)));
            controller.Add("Help6", new Label("The length of a single clip is within", new Vector2(139, 60+12*4)));
            controller.Add("Help5", new Label("some cases may not be applied.", new Vector2(139, 60+12*3)));
            controller.Add("Help3", new Label("Plugins are randomly chosen, and in", new Vector2(139, 60+12*2)));
            controller.Add("Help2", new Label("Feed input (materials) to render.", new Vector2(139, 60+12)));
            controller.Add("Help1", new Label("This is a nonsensical video generator.", new Vector2(139, 60)));
            // Add buttons
            controller.Add("ViewTutorial", new Button("Show Tutorial Window", "Access the initial setup window.", new Vector2(151+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        GlobalContent.GetSound("Prompt").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        ScreenManager.PushNavigation("Initial Setup");
                        ScreenManager.GetScreen<TutorialScreen>("Initial Setup")?.Show();
                        ScreenManager.GetScreen<ContentScreen>("Content")?.Hide();
                        ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Hide();
                        ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                        return true;
                }
                return false;
            }));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}