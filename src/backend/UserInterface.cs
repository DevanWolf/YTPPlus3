using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace YTPPlusPlusPlus
{
    public enum WindowState
    {
        Focused,
        Unfocused,
    }
    public enum MusicState
    {
        Playing,
        Paused,
        Stopped,
    }
    public enum MusicActive
    {
        Theme, // Funtastic Power! & KiwifruitDev - 300 This Is Sparta (YTP+ Mix)
        Theme2, // Bobby I Guess - A Nonsensical Song
    }
    public class UserInterface : Game
    {
        public static UserInterface? instance;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;
        private WindowState _windowState = WindowState.Focused;
        private MusicState _musicState = MusicState.Stopped;
        private MusicActive _musicActive = MusicActive.Theme;
        public UserInterface()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            instance = this;
        }
        protected override void Initialize()
        {
            // Set window title.
            Window.Title = Global.productName;
            // Disable anti-aliasing.
            _graphics.PreferMultiSampling = false;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Set screen resolution.
            int scale = int.Parse(SaveData.saveValues["ScreenScale"]);
            _graphics.PreferredBackBufferWidth = (int)(int.Parse(SaveData.saveValues["ScreenWidth"]) * scale);
            _graphics.PreferredBackBufferHeight = (int)(int.Parse(SaveData.saveValues["ScreenHeight"]) * scale);
            _graphics.ApplyChanges();
            // Load all screens.
            ScreenManager.LoadScreens();
            ConsoleOutput.Clear();
            ConsoleOutput.WriteLine("Initialization complete.");
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load default content.
            GlobalContent.LoadDefaultContent(Content, GraphicsDevice);
            // Play startup sound.
            GlobalContent.GetSound("Start").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
            // Find music.
            FindMusic();
            // Load all screen content.
            ScreenManager.LoadContent(Content, GraphicsDevice);
            base.LoadContent();
        }
        protected override void UnloadContent()
        {
            if(_spriteBatch != null)
                _spriteBatch.Dispose();
            // Unload all content.
            GlobalContent.UnloadContent();
            base.UnloadContent();
        }
        private void FindMusic()
        {
            int music = int.Parse(SaveData.saveValues["ActiveMusic"]);
            if(!Global.shuffled && SaveData.saveValues["ShuffleMusic"] == "true")
            {
                // Shuffle music.
                Global.shuffled = true;
                music = Global.generatorFactory.globalRandom.Next(0, Enum.GetNames(typeof(MusicActive)).Length);
                SaveData.saveValues["ActiveMusic"] = music.ToString();
                SaveData.Save();
            }
            // Make sure music is in range.
            if(music < 0 || music >= Enum.GetNames(typeof(MusicActive)).Length)
            {
                music = 0;
                SaveData.saveValues["ActiveMusic"] = music.ToString();
                SaveData.Save();
            }
        }
        protected override void Update(GameTime gameTime)
        {
            // Play music after 500ms.
            if(gameTime.TotalGameTime.TotalMilliseconds > 2500)
            {
                // Exchange music if it's not the same as the active music.
                if(_musicActive != (MusicActive)int.Parse(SaveData.saveValues["ActiveMusic"]))
                {
                    _musicActive = (MusicActive)int.Parse(SaveData.saveValues["ActiveMusic"]);
                    MediaPlayer.Play(GlobalContent.GetSong(_musicActive.ToString()));
                    MediaPlayer.Volume = 0f;
                }
                    
                if(Global.exiting)
                {
                    if(MediaPlayer.Volume > 0.01f)
                        MediaPlayer.Volume -= 0.01f;
                    else
                    {
                        MediaPlayer.Pause();
                        _musicState = MusicState.Paused;
                    }
                }
                else
                {
                    switch(_windowState)
                    {
                        case WindowState.Focused:
                            if(_musicState == MusicState.Playing)
                            {
                                // Fade in music.
                                float vol = int.Parse(SaveData.saveValues["MusicVolume"]) / 100f;
                                if(MediaPlayer.Volume < vol)
                                    MediaPlayer.Volume += 0.1f;
                                // Clamp music if it's over the volume level.
                                if(MediaPlayer.Volume > vol)
                                    MediaPlayer.Volume = vol;
                            }
                            else if(_musicState == MusicState.Stopped)
                            {
                                MediaPlayer.Play(GlobalContent.GetSong(_musicActive.ToString()));
                                _musicState = MusicState.Playing;
                            }
                            else if(_musicState == MusicState.Paused)
                            {
                                MediaPlayer.Resume();
                                _musicState = MusicState.Playing;
                            }
                            // Loop music.
                            if(MediaPlayer.State == MediaState.Stopped)
                            {
                                MediaPlayer.Play(GlobalContent.GetSong(_musicActive.ToString()));
                            }

                            break;
                        case WindowState.Unfocused:
                            if(_musicState == MusicState.Playing)
                            {
                                // Fade out music.
                                if(MediaPlayer.Volume > 0.1f)
                                    MediaPlayer.Volume -= 0.1f;
                                else
                                {
                                    MediaPlayer.Pause();
                                    _musicState = MusicState.Paused;
                                }
                            }
                            break;
                    }
                }
            }
            // Update window state.
            if(IsActive)
                _windowState = WindowState.Focused;
            else
                _windowState = WindowState.Unfocused;
            // Update screens.
            ScreenManager.Update(gameTime);
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0, 0, 0)); // Black background.
            if(_spriteBatch != null)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null, null, null, null);
                ScreenManager.Draw(gameTime, _spriteBatch);
                _spriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}
