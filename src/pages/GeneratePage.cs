using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class GeneratePage : IPage
    {
        public string Name { get; } = "Generate";
        public string Tooltip { get; } = "Render a nonsensical video.";
        private readonly InteractableController controller = new();
        private readonly InteractableController controllerAdvanced = new();
        private readonly InteractableController controllerTennis = new();
        private readonly InteractableController controllerRendering = new();
        private bool advanced = false;
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(Global.tennisMode)
            {
                if(controllerTennis.Update(gameTime, handleInput))
                    return true;
            }
            else if(advanced)
            {
                if(controllerAdvanced.Update(gameTime, handleInput))
                    return true;
            }
            else if(Global.generatorFactory.generatorActive)
            {
                if(controllerRendering.Update(gameTime, handleInput))
                    return true;
            }
            else
            {
                if(controller.Update(gameTime, handleInput))
                    return true;
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            if(advanced || Global.tennisMode)
            {
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                if(Global.tennisMode)
                {
                    controllerTennis.Draw(gameTime, spriteBatch);
                }
                else
                {
                    controllerAdvanced.Draw(gameTime, spriteBatch);
                }
            }
            else if(Global.generatorFactory.generatorActive)
            {
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                // Draw text to indicate that rendering is in progress
                SpriteFont font = GlobalContent.GetFont("Munro");
                string text = "Rendering is in progress.";
                Vector2 textSize = font.MeasureString(text);
                spriteBatch.DrawString(font, "Rendering is in progress.", new Vector2(GlobalGraphics.Scale(1) + GlobalGraphics.Scale(135) + (GlobalGraphics.Scale(306) - GlobalGraphics.Scale(135) - textSize.X) / 2, GlobalGraphics.Scale(1) + GlobalGraphics.Scale(58) + (GlobalGraphics.Scale(236) - GlobalGraphics.Scale(58) - textSize.Y) / 2), Color.Black);
                spriteBatch.DrawString(font, "Rendering is in progress.", new Vector2(GlobalGraphics.Scale(135) + (GlobalGraphics.Scale(306) - GlobalGraphics.Scale(135) - textSize.X) / 2, GlobalGraphics.Scale(58) + (GlobalGraphics.Scale(236) - GlobalGraphics.Scale(58) - textSize.Y) / 2), Color.White);
                controllerRendering.Draw(gameTime, spriteBatch);
            }
            else
            {
                controller.Draw(gameTime, spriteBatch);
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // RENDERING MODE
            controllerRendering.Add("Cancel", new Button("Cancel", "Stop rendering.", new Vector2(119+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        Global.generatorFactory.CancelGeneration(true);
                        return true;
                }
                return false;
            }));
            // TENNIS MODE
            controllerTennis.Add("TennisLabel", new Label("Tennis Options", new Vector2(144, 64+19*8)));
            controllerTennis.Add("TennisEnabled", new Switch("Use Tennis Mode", "Send generations back & forth!", new Vector2(139, 60), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["TennisMode"];
                    SaveData.saveValues["TennisMode"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["TennisMode"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["TennisMode"] == "true"));
            controllerTennis.Add("TennisBackToRegularOptions", new Button("Next Page", "Next page of options.", new Vector2(239+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        advanced = false;
                        Global.tennisMode = false;
                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        return true;
                }
                return false;
            }));
            // ADVANCED MODE
            controllerAdvanced.Add("ImageChance", new Dial("Image Chance", "How often image types are rolled.", new Vector2(139, 60+19*6), int.Parse(SaveData.saveValues["ImageChance"]), 0, 100, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["ImageChance"]);
                SaveData.saveValues["ImageChance"] = i.ToString();
                if(oldValue != int.Parse(SaveData.saveValues["ImageChance"]))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("OverlayChance", new Dial("Overlay Chance", "How often overlays are rolled.", new Vector2(139, 60+19*5), int.Parse(SaveData.saveValues["OverlayChance"]), 0, 100, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["OverlayChance"]);
                SaveData.saveValues["OverlayChance"] = i.ToString();
                if(oldValue != int.Parse(SaveData.saveValues["OverlayChance"]))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("TransitionChance", new Dial("Transition Chance", "How often transitions are rolled.", new Vector2(139, 60+19*4), int.Parse(SaveData.saveValues["TransitionChance"]), 0, 100, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["TransitionChance"]);
                SaveData.saveValues["TransitionChance"] = i.ToString();
                if(oldValue != int.Parse(SaveData.saveValues["TransitionChance"]))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("EffectChance", new Dial("Effect Chance", "How often any plugin effect is rolled.", new Vector2(139, 60+19*3), int.Parse(SaveData.saveValues["EffectChance"]), 0, 100, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["EffectChance"]);
                SaveData.saveValues["EffectChance"] = i.ToString();
                if(oldValue != int.Parse(SaveData.saveValues["EffectChance"]))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("Height", new TextEntry("Height", "How tall the result is.", SaveData.saveValues["VideoHeight"], new Vector2(139, 60+19*2), 24, 4, 2, (int i) => {
                string oldValue = SaveData.saveValues["VideoHeight"];
                SaveData.saveValues["VideoHeight"] = controllerAdvanced.interactables["Height"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoHeight"])
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("Width", new TextEntry("Width", "How wide the result is.", SaveData.saveValues["VideoWidth"], new Vector2(139, 60+19), 24, 4, 2, (int i) => {
                string oldValue = SaveData.saveValues["VideoWidth"];
                SaveData.saveValues["VideoWidth"] = controllerAdvanced.interactables["Width"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoWidth"])
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("OverlaysEnabled", new Switch("Use Overlays", "Insert overlay (green screen) transitions.", new Vector2(139, 60), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["OverlaysEnabled"];
                    SaveData.saveValues["OverlaysEnabled"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["OverlaysEnabled"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["OverlaysEnabled"] == "true"));
            controllerAdvanced.Add("AdvancedLabel", new Label("Advanced Options", new Vector2(144, 64+19*8)));
            controllerAdvanced.Add("BackToRegularOptions", new Button("Next Page", "Next page of options.", new Vector2(239+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        advanced = false;
                        if(int.Parse(SaveData.saveValues["AprilFoolsFlappyBirdScore"]) >= Global.tennisScore)
                        {
                            Global.tennisMode = true;
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                        else
                        {
                            GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                        return true;
                }
                return false;
            }));
            // REGULAR MODE
            // Add buttons
            controller.Add("MoreOptions", new Button("Next Page", "Next page of options.", new Vector2(239+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        advanced = true;
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        return true;
                }
                return false;
            }));
            controller.Add("StartRendering", new Button("Start Rendering", "Start generating a new video.", new Vector2(139+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        Global.generatorFactory.StartGeneration((sender, e) => {
                            if(e.ProgressPercentage == 100)
                                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            else
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            Global.justCompletedRender = true;
                        }, (sender, e) => {});
                        return true;
                }
                return false;
            }));
            // Add text entries
            controller.Add("ProjectTitle", new TextEntry("Project Name", "The name of the output video file.", SaveData.saveValues["ProjectTitle"], new Vector2(139, 60+19*7), 101, 20, 5, (int i) => {
                string oldValue = SaveData.saveValues["ProjectTitle"];
                SaveData.saveValues["ProjectTitle"] = controller.interactables["ProjectTitle"].Tooltip;
                if(oldValue != SaveData.saveValues["ProjectTitle"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("ClipCount", new TextEntry("Clip Count", "How many clips to generate.", SaveData.saveValues["MaxClipCount"], new Vector2(139, 60+19*6), 24, 4, 2, (int i) => {
                string oldValue = SaveData.saveValues["MaxClipCount"];
                SaveData.saveValues["MaxClipCount"] = controller.interactables["ClipCount"].Tooltip;
                if(oldValue != SaveData.saveValues["MaxClipCount"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MaxStreamDuration", new TextEntry("Maximum Stream Duration", "End of random length.", SaveData.saveValues["MaxStreamDuration"], new Vector2(139, 60+19*5), 26, 5, 2, (int i) => {
                string oldValue = SaveData.saveValues["MaxStreamDuration"];
                SaveData.saveValues["MaxStreamDuration"] = controller.interactables["MaxStreamDuration"].Tooltip;
                if(oldValue != SaveData.saveValues["MaxStreamDuration"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MinStreamDuration", new TextEntry("Minimum Stream Duration", "Start of random length.", SaveData.saveValues["MinStreamDuration"], new Vector2(139, 60+19*4), 26, 5, 2, (int i) => {
                string oldValue = SaveData.saveValues["MinStreamDuration"];
                SaveData.saveValues["MinStreamDuration"] = controller.interactables["MinStreamDuration"].Tooltip;
                if(oldValue != SaveData.saveValues["MinStreamDuration"])
                    SaveData.Save();
                return false;
            }));
            // Add switches
            controller.Add("InsertTransitionClips", new Switch("Insert Transition Clips", "Randomly plays a transition clip in full.", new Vector2(139, 60+19*3), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["TransitionsEnabled"];
                    SaveData.saveValues["TransitionsEnabled"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["TransitionsEnabled"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["TransitionsEnabled"] == "true"));
            controller.Add("InsertOutro", new Switch("Insert Outro", "Ends with a random outro.", new Vector2(139, 60+19*2), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["OutrosEnabled"];
                    SaveData.saveValues["OutrosEnabled"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["OutrosEnabled"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["OutrosEnabled"] == "true"));
            controller.Add("InsertIntro", new Switch("Insert Intro", "Begins with a random intro.", new Vector2(139, 60+19), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["IntrosEnabled"];
                    SaveData.saveValues["IntrosEnabled"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["IntrosEnabled"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["IntrosEnabled"] == "true"));
            controller.Add("SaveToLibrary", new Switch("Play Immediately", "Automatically start playing once complete.", new Vector2(139, 60), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["AddToLibrary"];
                    SaveData.saveValues["AddToLibrary"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["AddToLibrary"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["AddToLibrary"] == "true"));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
            controllerAdvanced.LoadContent(contentManager, graphicsDevice);
            controllerRendering.LoadContent(contentManager, graphicsDevice);
        }
    }
}