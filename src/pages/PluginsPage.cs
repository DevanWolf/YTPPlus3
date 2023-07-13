using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tweening;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class PluginsPage : IPage
    {
        public string Name { get; set; } = "Plugins";
        public string Tooltip { get; } = "Add, remove, or modify external plugins.";
        private int scrollOffset = 0;
        private int maxScrollOffset = 0;
        private bool dragging = false;
        private int dragOffset = 0;
        private bool editingSettings = false;
        private int settingsIndex = 0;
        private int setting = 0;
        private string internalTooltip = "";
        private readonly InteractableController controller = new();
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // 135, 56 PluginPage
            // 136, 57 PluginEntry
            // 294, 69-214 ScrollHandle (9x9)
            Texture2D pluginPage = GlobalContent.GetTexture("PluginPage");
            Texture2D pluginSettings = GlobalContent.GetTexture("PluginSettings");
            Texture2D pluginEntry = GlobalContent.GetTexture("PluginEntry");
            Texture2D scrollHandle = GlobalContent.GetTexture("ScrollHandle");
            Texture2D interactiveSwitchOn = GlobalContent.GetTexture("InteractiveSwitchOn");
            Texture2D interactiveSwitchOff = GlobalContent.GetTexture("InteractiveSwitchOff");
            SpriteFont munroSmall = GlobalContent.GetFont("MunroSmall");
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            if(!editingSettings)
            {
                // Draw scroll bar
                spriteBatch.Draw(pluginPage, new Rectangle(GlobalGraphics.Scale(293), GlobalGraphics.Scale(57), pluginPage.Width * GlobalGraphics.scale, pluginPage.Height * GlobalGraphics.scale), Color.White);
                // Move the scroll handle relative to the scroll offset and the max scroll offset.
                if(maxScrollOffset > 0)
                {
                    spriteBatch.Draw(scrollHandle, new Rectangle(GlobalGraphics.Scale(294), GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset), scrollHandle.Width * GlobalGraphics.scale, scrollHandle.Height * GlobalGraphics.scale), Color.White);
                }
                // End existing spritebatch
                spriteBatch.End();
                // Mask to specific area
                spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(293), GlobalGraphics.Scale(236)); 
                RasterizerState rasterizerState = new RasterizerState();
                rasterizerState.ScissorTestEnable = true;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerState, null, Matrix.CreateTranslation(GlobalGraphics.Scale(ScreenManager.GetScreen<ContentScreen>("Content").offset.X / GlobalGraphics.scale), GlobalGraphics.Scale((ScreenManager.GetScreen<ContentScreen>("Content").offset.Y / GlobalGraphics.scale) + -scrollOffset), 0));
                for(int i = 0; i < PluginHandler.GetPluginCount(); i++)
                {
                    spriteBatch.Draw(pluginEntry, new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57 + i * pluginEntry.Height + i), pluginEntry.Width * GlobalGraphics.scale, pluginEntry.Height * GlobalGraphics.scale), Color.White);
                    if(PluginHandler.plugins[i].settings.Count > 0 && Global.canRender)
                    {
                        spriteBatch.Draw(pluginSettings, new Rectangle(GlobalGraphics.Scale(226), GlobalGraphics.Scale(59 + i * pluginEntry.Height + i), pluginSettings.Width * GlobalGraphics.scale, pluginSettings.Height * GlobalGraphics.scale), Color.White);
                        spriteBatch.DrawString(munroSmall, "Settings", new Vector2(GlobalGraphics.Scale(233+1), GlobalGraphics.Scale(58+1 + i * pluginEntry.Height + i)), Color.Black);
                        spriteBatch.DrawString(munroSmall, "Settings", new Vector2(GlobalGraphics.Scale(233), GlobalGraphics.Scale(58 + i * pluginEntry.Height + i)), Color.White);
                    }
                    spriteBatch.DrawString(munroSmall, Path.GetFileName(PluginHandler.plugins[i].path), new Vector2(GlobalGraphics.Scale(141+1), GlobalGraphics.Scale(58+1 + i * pluginEntry.Height + i)), Color.Black);
                    spriteBatch.DrawString(munroSmall, Path.GetFileName(PluginHandler.plugins[i].path), new Vector2(GlobalGraphics.Scale(141), GlobalGraphics.Scale(58 + i * pluginEntry.Height + i)), Color.White);
                    if(Global.canRender)
                    {
                        spriteBatch.Draw(PluginHandler.plugins[i].enabled ? interactiveSwitchOn : interactiveSwitchOff, new Rectangle(GlobalGraphics.Scale(271), GlobalGraphics.Scale(60 + i * pluginEntry.Height + i), interactiveSwitchOn.Width * GlobalGraphics.scale, interactiveSwitchOn.Height * GlobalGraphics.scale), Color.White);
                    }
                    else
                    {
                        spriteBatch.DrawString(munroSmall, "...", new Vector2(GlobalGraphics.Scale(277+1), GlobalGraphics.Scale(58 + 1 + i * pluginEntry.Height + i)), Color.Black);
                        spriteBatch.DrawString(munroSmall, "...", new Vector2(GlobalGraphics.Scale(277), GlobalGraphics.Scale(58 + i * pluginEntry.Height + i)), Color.White);
                    }
                }
                // End offset
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            else
            {
                // Draw background
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                // Interactable
                controller.Draw(gameTime, spriteBatch);
            }
            if (internalTooltip != "")
            {
                // Get text size
                Vector2 tooltipSize = GlobalGraphics.fontMunroSmall.MeasureString(internalTooltip);
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
                spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, internalTooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            int tempMaxScrollOffset = PluginHandler.GetPluginCount();
            tempMaxScrollOffset -= 11;
            if(tempMaxScrollOffset <= 0)
            {
                tempMaxScrollOffset = 0;
            }
            maxScrollOffset = tempMaxScrollOffset * 16;
            if(handleInput)
            {
                internalTooltip = "";
                if(!editingSettings)
                {
                    for (int i = 0; i < PluginHandler.GetPluginCount(); i++)
                    {
                        if (MouseInput.MouseState.X >= GlobalGraphics.Scale(269) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                            && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                        {
                            internalTooltip = Global.canRender ?"Click to toggle plugin." : "Loading...";
                        }
                        int inRange = 228;
                        if(PluginHandler.plugins[i].settings.Count <= 0 || !Global.canRender)
                        {
                            inRange = 267; // No settings button
                        }
                        if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(inRange)
                            && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                        {
                            internalTooltip = "Open plugin directory.";
                        }
                        if(PluginHandler.plugins[i].settings.Count > 0 && Global.canRender)
                        {
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(230) && MouseInput.MouseState.X < GlobalGraphics.Scale(267)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                            {
                                internalTooltip = "Edit plugin settings.";
                            }
                        }
                    }
                    if(maxScrollOffset > 0)
                    {
                        if (MouseInput.MouseState.ScrollWheelValue != MouseInput.LastMouseState.ScrollWheelValue)
                        {
                            // 120 is the scroll wheel value for one scroll.
                            // One entry is one offset.
                            int lines = (MouseInput.MouseState.ScrollWheelValue - MouseInput.LastMouseState.ScrollWheelValue) / 120;
                            int oldScrollOffset = scrollOffset;
                            // Round up or down scroll offset to the nearest 16 multiple.
                            scrollOffset = (int)Math.Round(scrollOffset / 16.0) * 16;
                            // If it's the same
                            if (scrollOffset == oldScrollOffset)
                            {
                                scrollOffset -= lines * 16;
                            }
                            // If it's less than 0, set it to 0.
                            if (scrollOffset < 0)
                            {
                                scrollOffset = 0;
                            }
                            // If it's more than the max scroll offset, set it to the max scroll offset.
                            if (scrollOffset > maxScrollOffset)
                            {
                                scrollOffset = maxScrollOffset;
                            }
                        }
                        // Scroll handle
                        if(!dragging)
                        {
                            if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                            {
                                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(303)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(78 + scrollOffset * (214 - 69) / maxScrollOffset))
                                {
                                    dragging = true;
                                    dragOffset = MouseInput.MouseState.Y - GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset);
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    return true;
                                }
                                // 293, 57, 11x11 Scroll Up
                                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(57) && MouseInput.MouseState.Y < GlobalGraphics.Scale(68))
                                {
                                    if(scrollOffset - 1 >= 0)
                                    {
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                        int oldScrollOffset = scrollOffset;
                                        // Round down scroll offset to the nearest 16 multiple.
                                        scrollOffset = (int)(Math.Floor((double)scrollOffset / 16) * 16);
                                        // If it's the same, subtract 16.
                                        if (scrollOffset == oldScrollOffset)
                                        {
                                            scrollOffset -= 16;
                                        }
                                        // If it's less than 0, set it to 0.
                                        if (scrollOffset < 0)
                                        {
                                            scrollOffset = 0;
                                        }
                                    }
                                    else
                                    {
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    }
                                    return true;
                                }
                                // 293, 224, 11x11 Scroll Down
                                if (MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(224) && MouseInput.MouseState.Y < GlobalGraphics.Scale(235))
                                {
                                    if (scrollOffset + 1 <= maxScrollOffset)
                                    {
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                        int oldScrollOffset = scrollOffset;
                                        // Round up scroll offset to the nearest 16 multiple.
                                        scrollOffset = (int)(Math.Ceiling((double)scrollOffset / 16) * 16);
                                        // If it's the same, add 16.
                                        if (scrollOffset == oldScrollOffset)
                                        {
                                            scrollOffset += 16;
                                        }
                                        // If it's more than the max, set it to the max.
                                        if (scrollOffset > maxScrollOffset)
                                        {
                                            scrollOffset = maxScrollOffset;
                                        }
                                    }
                                    else
                                    {
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    }
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (MouseInput.MouseState.LeftButton == ButtonState.Released)
                            {
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                dragging = false;
                            }
                            else
                            {
                                int newY = MouseInput.MouseState.Y - dragOffset;
                                if (newY >= GlobalGraphics.Scale(69) && newY <= GlobalGraphics.Scale(214))
                                {
                                    scrollOffset = (newY - GlobalGraphics.Scale(69)) * maxScrollOffset / (GlobalGraphics.Scale(214) - GlobalGraphics.Scale(69));
                                }
                            }
                            return true;
                        }
                    }
                    else
                    {
                        if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                        {
                            // 293, 57, 11x11 Scroll Up
                            if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(57) && MouseInput.MouseState.Y < GlobalGraphics.Scale(68))
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                return true;
                            }
                            // 293, 224, 11x11 Scroll Down
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(224) && MouseInput.MouseState.Y < GlobalGraphics.Scale(235))
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                return true;
                            }
                        }
                        dragging = false;
                    }
                    if(MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                    {
                        // Plugin entries
                        for (int i = 0; i < PluginHandler.GetPluginCount(); i++)
                        {
                            // Toggle button
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(269) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                            {
                                if(Global.canRender)
                                {
                                    PluginHandler.plugins[i].enabled = !PluginHandler.plugins[i].enabled;
                                    PluginHandler.SavePluginSettings();
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                }
                                else
                                {
                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                }
                                return true;
                            }
                            // Open folder containing plugin
                            int inRange = 228;
                            if(PluginHandler.plugins[i].settings.Count <= 0 || !Global.canRender)
                            {
                                inRange = 267; // No settings button
                            }
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(inRange)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                            {
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                // Open directory and select file
                                ProcessStartInfo startInfo = new()
                                {
                                    FileName = "explorer.exe",
                                    Arguments = "/select, \"" + Path.GetFullPath(PluginHandler.plugins[i].path) + "\""
                                };
                                Process.Start(startInfo);
                                return true;
                            }
                            // Settings button
                            if(PluginHandler.plugins[i].settings.Count > 0 && Global.canRender)
                            {
                                if (MouseInput.MouseState.X >= GlobalGraphics.Scale(230) && MouseInput.MouseState.X < GlobalGraphics.Scale(267)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                                {
                                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    // Open settings
                                    controller.Clear();
                                    settingsIndex = i;
                                    editingSettings = true;
                                    controller.Add("Back", new Button("Back", "Go back to plugin list.", new Vector2(119+36, 60+10+19*8), (int i) => {
                                        switch(i)
                                        {
                                            case 2: // left click
                                                GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                                controller.Clear();
                                                Name = "Plugins";
                                                editingSettings = false;
                                                return true;
                                        }
                                        return false;
                                    }));
                                    Name = Path.GetFileName(PluginHandler.plugins[i].path);
                                    controller.Add("ErrorLabel", new Label("Error sound means it didn't save.", new Vector2(139, 60)));
                                    int sindex = 0;
                                    List<TextEntry> tes = new();
                                    foreach(KeyValuePair<string, object> s in PluginHandler.plugins[i].settings)
                                    {
                                        TextEntry te = new TextEntry(s.Key.Replace("_", " "), PluginHandler.plugins[i].settingTooltips[s.Key].Replace("_", " "), PluginHandler.plugins[i].settings[s.Key].ToString(), new Vector2(139, 51+19+19*sindex), 50, 25, 6, (int i) => {
                                            if(i == 1)
                                            {
                                                // Set "setting" to the setting index where mouse cursor y is
                                                int y = MouseInput.MouseState.Y / GlobalGraphics.scale;
                                                Rectangle source = new(139, 70, 53, 15);
                                                int separator = 4;
                                                setting = (y - source.Y - separator) / (source.Height + separator);
                                                if(setting < 0)
                                                    setting = 0;
                                                if(setting >= PluginHandler.plugins[settingsIndex].settings.Count)
                                                    setting = PluginHandler.plugins[settingsIndex].settings.Count - 1;
                                                return false;
                                            }
                                            else if(i == 0)
                                            {
                                                // Use "setting" to get the key of the setting
                                                string keyFromIndex = PluginHandler.plugins[settingsIndex].settings.Keys.ElementAt(setting);
                                                string oldValue = PluginHandler.plugins[settingsIndex].settings[keyFromIndex].ToString();
                                                PluginHandler.plugins[settingsIndex].settings[keyFromIndex] = controller.interactables[keyFromIndex.ToString()].Tooltip;
                                                if(oldValue != PluginHandler.plugins[settingsIndex].settings[keyFromIndex].ToString())
                                                    PluginHandler.SavePluginSettings();
                                                else
                                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                            }
                                            return false;
                                        });
                                        te.Register();
                                        tes.Add(te);
                                        sindex++;
                                    }
                                    for(int i2 = tes.Count-1; i2 >= 0; i2--)
                                    {
                                        // Get name
                                        string name = tes[i2].Name.Replace(" ", "_");
                                        controller.Add(name, tes[i2]);
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Interactable
                    if(controller.Update(gameTime, handleInput))
                        return true;
                }
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("PluginPage", contentManager.Load<Texture2D>("graphics/pluginpage"));
            GlobalContent.AddTexture("PluginSettings", contentManager.Load<Texture2D>("graphics/pluginsettings"));
            GlobalContent.AddTexture("PluginEntry", contentManager.Load<Texture2D>("graphics/pluginentry"));
            GlobalContent.AddTexture("ScrollHandle", contentManager.Load<Texture2D>("graphics/scrollhandle"));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}