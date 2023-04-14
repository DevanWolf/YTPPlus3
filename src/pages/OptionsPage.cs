using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class OptionsPage : IPage
    {
        public string Name { get; } = "Options";
        public string Tooltip { get; } = "Change application settings.";
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
            // Add dials
            controller.Add("BGSaturation", new Dial("Background Saturation", "Adjust color in the animated background.", new Vector2(139, 51+19*4), int.Parse(SaveData.saveValues["BackgroundSaturation"]), 0, 100, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["BackgroundSaturation"]);
                SaveData.saveValues["BackgroundSaturation"] = (i + 1).ToString();
                if(oldValue != int.Parse(SaveData.saveValues["BackgroundSaturation"]))
                    SaveData.Save();
                return false;
            }));
            controller.Add("Scale", new Dial("Screen Scale", "Screen size multiplier, requires restart.", new Vector2(139, 51+19*3), int.Parse(SaveData.saveValues["ScreenScale"]) - 1, 1, 4, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["ScreenScale"]);
                SaveData.saveValues["ScreenScale"] = (i + 1).ToString();
                if(oldValue != int.Parse(SaveData.saveValues["ScreenScale"]))
                    SaveData.Save();
                return false;
            }));
            controller.Add("SFXVolume", new Dial("Sound Effect Volume", "Sound effect volume level.", new Vector2(139, 51+19*2), int.Parse(SaveData.saveValues["SoundEffectVolume"]), 0, 100, (int i) => {
                string oldValue = SaveData.saveValues["SoundEffectVolume"];
                SaveData.saveValues["SoundEffectVolume"] = i.ToString();
                if(oldValue != SaveData.saveValues["SoundEffectVolume"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MusicVolume", new Dial("Music Volume", "Background music volume level.", new Vector2(139, 51+19), int.Parse(SaveData.saveValues["MusicVolume"]), 0, 100, (int i) => {
                string oldValue = SaveData.saveValues["MusicVolume"];
                SaveData.saveValues["MusicVolume"] = i.ToString();
                if(oldValue != SaveData.saveValues["MusicVolume"])
                    SaveData.Save();
                return false;
            }));
            // Add labels
            controller.Add("DialLabel", new Label("Click and rotate dials slowly.", new Vector2(139, 60)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}