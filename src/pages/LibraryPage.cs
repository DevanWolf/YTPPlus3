using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System;
using System.ComponentModel;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class LibraryPage : IPage
    {
        public string Name { get; } = "Library";
        public string Tooltip { get; } = "View imported media and previous renders.";
        private readonly InteractableController controller = new();
        private readonly Dictionary<string, Rectangle> rects = new();
        private readonly Dictionary<LibraryRootType, List<string>> libraryTypes = new();
        private LibraryRootType currentRootType = LibraryRootType.Video;
        private LibraryType currentLibraryType = DefaultLibraryTypes.Render;
        private readonly Dictionary<LibraryType, List<LibraryFile>> libraryFileCache = new();
        private readonly List<Texture2D> videoPlayers = new();
        private int selectedFlags = 1 | 8; // 1 = Video, 8 = First SubType
        private int staticAnim = 0;
        private int audioAnim = 0;
        private double lastAnimTime;
        private double lastAnimTimeAudio;
        private int page = 0;
        private bool demandChange = true;
        private bool organizing = false;
        private int organizeFile = -1;
        private int organizeType = -1;
        private string tooltip = "";
        private bool loaded = false;
        public void CacheLibrary()
        {
            libraryFileCache.Clear();
            // Get library types
            libraryTypes[LibraryRootType.Video] = LibraryData.GetLibraryNames(LibraryRootType.Video);
            libraryTypes[LibraryRootType.Audio] = LibraryData.GetLibraryNames(LibraryRootType.Audio);
            // Preload library files
            LibraryData.Load();
            for(int i = 0; i < DefaultLibraryTypes.AllTypes.Count; i++)
            {
                LibraryType libraryType = DefaultLibraryTypes.AllTypes[i];
                if(!libraryType.Special)
                {
                    libraryFileCache[libraryType] = LibraryData.GetFiles(libraryType);
                }
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Library assets
            GlobalContent.AddTexture("AddVideoOverlay", contentManager.Load<Texture2D>("graphics/library/addvideooverlay"));
            GlobalContent.AddTexture("HeaderButton", contentManager.Load<Texture2D>("graphics/library/headerbutton"));
            GlobalContent.AddTexture("HeaderButtonSelected", contentManager.Load<Texture2D>("graphics/library/headerbuttonselected"));
            GlobalContent.AddTexture("Separator", contentManager.Load<Texture2D>("graphics/library/separator"));
            GlobalContent.AddTexture("SubTypeButton", contentManager.Load<Texture2D>("graphics/library/subtypebutton"));
            GlobalContent.AddTexture("SubTypeButtonSelected", contentManager.Load<Texture2D>("graphics/library/subtypebuttonselected"));
            GlobalContent.AddTexture("TypeButton", contentManager.Load<Texture2D>("graphics/library/typebutton"));
            GlobalContent.AddTexture("TypeButtonSelected", contentManager.Load<Texture2D>("graphics/library/typebuttonselected"));
            GlobalContent.AddTexture("VideoHolder", contentManager.Load<Texture2D>("graphics/library/videoholder"));
            GlobalContent.AddTexture("VideoOff", contentManager.Load<Texture2D>("graphics/library/videooff"));
            GlobalContent.AddTexture("VideoOn", contentManager.Load<Texture2D>("graphics/library/videoon"));
            GlobalContent.AddTexture("SubTypeButtonOrganize", contentManager.Load<Texture2D>("graphics/library/subtypebuttonorganize"));
            GlobalContent.AddTexture("StaticOverlay", contentManager.Load<Texture2D>("graphics/library/staticoverlay"));
            // TV Static Animation: 13 frames as graphics/library/staticanim/staticanim0 to graphics/library/staticanim/staticanim12
            for(int i = 0; i < 13; i++)
            {
                GlobalContent.AddTexture("StaticAnim" + i, contentManager.Load<Texture2D>("graphics/library/staticanim/staticanim" + i));
            }
            // Vinyl Record Animation: 2 frames as graphics/library/audioanim/audioanim0 to graphics/library/audioanim/audioanim1
            for(int i = 0; i < 2; i++)
            {
                GlobalContent.AddTexture("AudioAnim" + i, contentManager.Load<Texture2D>("graphics/library/audioanim/audioanim" + i));
            }
            // Get textures
            Texture2D addVideoOverlay = GlobalContent.GetTexture("AddVideoOverlay");
            Texture2D headerButton = GlobalContent.GetTexture("HeaderButton");
            Texture2D headerButtonSelected = GlobalContent.GetTexture("HeaderButtonSelected");
            Texture2D separator = GlobalContent.GetTexture("Separator");
            Texture2D subTypeButton = GlobalContent.GetTexture("SubTypeButton");
            Texture2D subTypeButtonSelected = GlobalContent.GetTexture("SubTypeButtonSelected");
            Texture2D typeButton = GlobalContent.GetTexture("TypeButton");
            Texture2D typeButtonSelected = GlobalContent.GetTexture("TypeButtonSelected");
            Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
            Texture2D videoOff = GlobalContent.GetTexture("VideoOff");
            Texture2D videoOn = GlobalContent.GetTexture("VideoOn");
            // Set up rectangles
            rects.Add("VideoButton", new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("AudioButton", new Rectangle(GlobalGraphics.Scale(166), GlobalGraphics.Scale(56), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("HeaderButton", new Rectangle(GlobalGraphics.Scale(200), GlobalGraphics.Scale(56), GlobalGraphics.Scale(headerButton.Width), GlobalGraphics.Scale(headerButton.Height)));
            rects.Add("PageLeftButton", new Rectangle(GlobalGraphics.Scale(200), GlobalGraphics.Scale(223), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("PageRightButton", new Rectangle(GlobalGraphics.Scale(276), GlobalGraphics.Scale(223), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if(!loaded)
                return;
            // changes
            if(demandChange)
            {
                // Load video players
                if(currentRootType == LibraryRootType.Video)
                    ChangeVideos(spriteBatch.GraphicsDevice);
                demandChange = false;
            }
            // Store textures in local variable for performance
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            Texture2D addVideoOverlay = GlobalContent.GetTexture("AddVideoOverlay");
            Texture2D headerButton = GlobalContent.GetTexture("HeaderButton");
            Texture2D headerButtonSelected = GlobalContent.GetTexture("HeaderButtonSelected");
            Texture2D separator = GlobalContent.GetTexture("Separator");
            Texture2D subTypeButton = GlobalContent.GetTexture("SubTypeButton");
            Texture2D subTypeButtonSelected = GlobalContent.GetTexture("SubTypeButtonSelected");
            Texture2D typeButton = GlobalContent.GetTexture("TypeButton");
            Texture2D typeButtonSelected = GlobalContent.GetTexture("TypeButtonSelected");
            Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
            Texture2D videoOff = GlobalContent.GetTexture("VideoOff");
            Texture2D videoOn = GlobalContent.GetTexture("VideoOn");
            SpriteFont munroSmall = GlobalContent.GetFont("MunroSmall");
            // Draw background
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(170), GlobalGraphics.Scale(15)), Color.Gray);
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(219), GlobalGraphics.Scale(170), GlobalGraphics.Scale(17)), Color.Gray);
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71), GlobalGraphics.Scale(65), GlobalGraphics.Scale(148)), Color.Gray);
            // Draw separators and video holders
            int a, b;
            for (a = 0; a < 3; a++)
            {
                for (b = 0; b < 4; b++)
                {
                    spriteBatch.Draw(separator, new Rectangle(GlobalGraphics.Scale(200 + 35 * a), GlobalGraphics.Scale(71 + 37 * b), GlobalGraphics.Scale(separator.Width), GlobalGraphics.Scale(separator.Height)), Color.White);
                    Rectangle videoHolderRect = new Rectangle(GlobalGraphics.Scale(201 + (33 * a) + (a * 2)), GlobalGraphics.Scale(72 + (35 * b) + (b * 2)), GlobalGraphics.Scale(videoHolder.Width), GlobalGraphics.Scale(videoHolder.Height));
                    Rectangle staticRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(2), videoHolderRect.Y + GlobalGraphics.Scale(2), GlobalGraphics.Scale(29), GlobalGraphics.Scale(22));
                    Rectangle addVideoOverlayRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(4), videoHolderRect.Y + GlobalGraphics.Scale(26), GlobalGraphics.Scale(addVideoOverlay.Width), GlobalGraphics.Scale(addVideoOverlay.Height));
                    // Get library item at this position and page
                    int position = a + (b * 3) + (12 * page);
                    bool video = false;
                    if(libraryFileCache[currentLibraryType].Count > position)
                    {
                        video = true;
                        if(currentRootType == LibraryRootType.Video)
                        {
                            spriteBatch.Draw(GlobalContent.GetTexture("StaticAnim" + staticAnim), staticRect, new Color(64, 64, 64, 255));
                            int pagelessPosition = position - (12 * page);
                            if(videoPlayers.Count > pagelessPosition)
                            {
                                spriteBatch.Draw(videoPlayers[pagelessPosition], staticRect, Color.White);
                            }
                        }
                        else
                        {
                            spriteBatch.Draw(GlobalContent.GetTexture("AudioAnim" + audioAnim), staticRect, Color.White);
                        }
                    }
                    else
                    {   
                        spriteBatch.Draw(GlobalContent.GetTexture("StaticAnim" + staticAnim), staticRect, Color.White);
                        spriteBatch.Draw(GlobalContent.GetTexture("StaticOverlay"), staticRect, new Color(255, 255, 255, 128));
                    }
                    // Draw video holder
                    spriteBatch.Draw(videoHolder, videoHolderRect, Color.White);
                    if(!video)
                        spriteBatch.Draw(addVideoOverlay, addVideoOverlayRect, Color.White);
                }
            }
            // Draw buttons
            spriteBatch.Draw(typeButton, rects["VideoButton"], Color.White);
            spriteBatch.Draw(typeButton, rects["AudioButton"], Color.White);
            spriteBatch.Draw(headerButton, rects["HeaderButton"], Color.White);
            // Draw subtypes
            for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
            {
                spriteBatch.Draw(subTypeButton, rects[currentRootType.ToString() + libraryTypes[currentRootType][i] + "Button"], Color.White);
            }
            // Draw selected buttons
            if((selectedFlags & 1) == 1)
                spriteBatch.Draw(typeButtonSelected, rects["VideoButton"], Color.White);
            if((selectedFlags & 2) == 2)
                spriteBatch.Draw(typeButtonSelected, rects["AudioButton"], Color.White);
            if((selectedFlags & 4) == 4)
                spriteBatch.Draw(headerButtonSelected, rects["HeaderButton"], Color.White);
            // Flags 8-32768 (0x8-0x8000, 13 bits) are for subtypes
            for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
            {
                if((selectedFlags & (8 << i)) == (8 << i))
                    spriteBatch.Draw(subTypeButtonSelected, rects[currentRootType.ToString() + libraryTypes[currentRootType][i] + "Button"], Color.White);
            }
            // Draw page buttons
            spriteBatch.Draw(typeButton, rects["PageLeftButton"], Color.White);
            spriteBatch.Draw(typeButton, rects["PageRightButton"], Color.White);
            // Draw text
            spriteBatch.DrawString(munroSmall, "Video", new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, "Video", new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(56)), Color.White);
            spriteBatch.DrawString(munroSmall, "Audio", new Vector2(GlobalGraphics.Scale(170 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, "Audio", new Vector2(GlobalGraphics.Scale(170), GlobalGraphics.Scale(56)), Color.White);
            spriteBatch.DrawString(munroSmall, "Back", new Vector2(GlobalGraphics.Scale(205 + 1), GlobalGraphics.Scale(223 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, "Back", new Vector2(GlobalGraphics.Scale(205), GlobalGraphics.Scale(223)), Color.White);
            spriteBatch.DrawString(munroSmall, "Next", new Vector2(GlobalGraphics.Scale(281 + 1), GlobalGraphics.Scale(223 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, "Next", new Vector2(GlobalGraphics.Scale(281), GlobalGraphics.Scale(223)), Color.White);
            // Page indicator is centered
            int maxPages = (int)Math.Ceiling((double)libraryFileCache[currentLibraryType].Count / 12);
            // If the last page is full of videos, add an extra page
            if(libraryFileCache[currentLibraryType].Count % 12 == 0)
                maxPages++;
            string pageIndicator = (page + 1) + " of " + maxPages;
            Vector2 pageIndicatorSize = munroSmall.MeasureString(pageIndicator);
            int pivot = 252; 
            spriteBatch.DrawString(munroSmall, pageIndicator, new Vector2(GlobalGraphics.Scale(pivot + 1) - pageIndicatorSize.X / 2, GlobalGraphics.Scale(223 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, pageIndicator, new Vector2(GlobalGraphics.Scale(pivot) - pageIndicatorSize.X / 2, GlobalGraphics.Scale(223)), Color.White);
            for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
            {
                spriteBatch.DrawString(munroSmall, libraryTypes[currentRootType][i], new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(71 + 13 * i + 1)), Color.Black);
                spriteBatch.DrawString(munroSmall, libraryTypes[currentRootType][i], new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(71 + 13 * i)), Color.White);
            }
            // Interactable
            controller.Draw(gameTime, spriteBatch);
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
        // Thread for loading videos
        private BackgroundWorker loadVideosThread;
        private void LoadVideosThread(object? sender, DoWorkEventArgs e)
        {
            // Args: GraphicsDevice graphicsDevice, int currentPage
            object[] args = (object[])e.Argument;
            GraphicsDevice graphicsDevice = (GraphicsDevice)args[0];
            int currentPage = (int)args[1];
            // Load videos
            int a, b;
            for (a = 0; a < 3; a++)
            {
                for (b = 0; b < 4; b++)
                {
                    // Check to make sure we're still on the same page
                    if (currentPage != page)
                        return;
                    // Cancelled?
                    if (loadVideosThread.CancellationPending)
                        return;
                    int position = a + (b * 3) + (12 * page);
                    if(libraryFileCache[currentLibraryType].Count > position)
                    {
                        LibraryFile libraryFile = libraryFileCache[currentLibraryType][position];
                        // Get thumbnail of video using shell
                        ShellFile shellFile = ShellFile.FromFilePath(libraryFile.Path);
                        BitmapSource bitmapSource = shellFile.Thumbnail.BitmapSource;
                        // Check to make sure we're still on the same page
                        if (currentPage != page)
                            return;
                        // Cancelled?
                        if (loadVideosThread.CancellationPending)
                            return;
                        // Convert to texture
                        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                        BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                        bitmapSource.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                        bitmap.UnlockBits(data);
                        Texture2D texture = new(graphicsDevice, bitmap.Width, bitmap.Height);
                        Color[] colorData = new Color[bitmap.Width * bitmap.Height];
                        for (int i = 0; i < colorData.Length; i++)
                        {
                            System.Drawing.Color color = bitmap.GetPixel(i % bitmap.Width, i / bitmap.Width);
                            colorData[i] = new Color(color.R, color.G, color.B, color.A);
                        }
                        texture.SetData(colorData);
                        // Check to make sure we're still on the same page
                        if (currentPage != page)
                            return;
                        // Cancelled?
                        if (loadVideosThread.CancellationPending)
                            return;
                        // Update video players
                        videoPlayers.Add(texture);
                    }
                }
            }
            // Done!
            loadVideosThread.Dispose();
            loadVideosThread = null;
        }
        public void ChangeVideos(GraphicsDevice graphicsDevice)
        {
            // Cancel previous thread
            if (loadVideosThread != null)
            {
                loadVideosThread.CancelAsync();
                loadVideosThread.Dispose();
            }
            // Clear video players
            for(int i = 0; i < videoPlayers.Count; i++)
            {
                videoPlayers[i].Dispose();
            }
            videoPlayers.Clear();
            // Start new thread
            loadVideosThread = new BackgroundWorker();
            loadVideosThread.DoWork += LoadVideosThread;
            loadVideosThread.WorkerSupportsCancellation = true;
            loadVideosThread.RunWorkerAsync(new object[] { graphicsDevice, page });
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(Global.justCompletedRender || !loaded)
            {
                // Reimport all and demand change
                CacheLibrary();
                demandChange = true;
                if(Global.justCompletedRender)
                    Global.justCompletedRender = false;
                if(!loaded)
                {
                    loaded = true;
                    Texture2D subTypeButton = GlobalContent.GetTexture("SubTypeButton");
                    // For one subtype, 60x13 at 135, 71 + 13 * i
                    foreach(LibraryRootType type in libraryTypes.Keys)
                    {
                        for(int i = 0; i < libraryTypes[type].Count; i++)
                        {
                            rects.Add(type.ToString() + libraryTypes[type][i] + "Button", new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71 + 13 * i), GlobalGraphics.Scale(subTypeButton.Width), GlobalGraphics.Scale(subTypeButton.Height)));
                        }
                    }
                }
            }
            // Organizing
            if(!organizing && organizeFile > -1 && organizeType > -1)
            {
                // Get library file
                LibraryFile libraryFile = libraryFileCache[currentLibraryType][organizeFile];
                LibraryType? oldLibraryType = libraryFile.Type;
                // Get library type
                string libraryName = libraryTypes[currentRootType][organizeType];
                LibraryType libraryType = DefaultLibraryTypes.All;
                foreach(KeyValuePair<LibraryType, string> kvPair in LibraryData.libraryNames)
                {
                    if(kvPair.Value == libraryName)
                    {
                        libraryType = kvPair.Key;
                        break;
                    }
                }
                if(!libraryType.Special)
                {
                    LibraryFile newFile = LibraryData.Organize(libraryFile, libraryType);
                    if(oldLibraryType != null)
                        libraryFileCache[oldLibraryType].Remove(libraryFile);
                    libraryFileCache[libraryType].Add(newFile);
                }
                organizeFile = -1;
                organizeType = -1;
                demandChange = true;
            }
            // staticAnim is a 15fps animation, so update every 66.666ms
            if (gameTime.TotalGameTime.TotalMilliseconds - lastAnimTime > 66.666)
            {
                // Update animation
                staticAnim++;
                if(staticAnim > 12)
                    staticAnim = 0;
                // Update lastAnimTime
                lastAnimTime = gameTime.TotalGameTime.TotalMilliseconds;
            }
            // audioanim is a 4fps animation
            if(gameTime.TotalGameTime.TotalMilliseconds - lastAnimTimeAudio > 250)
            {
                // Update animation
                audioAnim++;
                if(audioAnim > 1)
                    audioAnim = 0;
                // Update lastAnimTimeAudio
                lastAnimTimeAudio = gameTime.TotalGameTime.TotalMilliseconds;
            }
            // Standard input
            if(handleInput && !organizing)
            {
                // Left click
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                {
                    // Loop rects
                    foreach (KeyValuePair<string, Rectangle> rect in rects)
                    {
                        // Check if mouse is in rect
                        if (rect.Value.Contains(MouseInput.MouseState.Position))
                        {
                            // Check rect name
                            switch (rect.Key)
                            {
                                case "VideoButton":
                                    selectedFlags |= 1;
                                    selectedFlags &= ~2;
                                    for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                    {
                                        selectedFlags &= ~(8 << i);
                                    }
                                    selectedFlags |= 8;
                                    currentRootType = LibraryRootType.Video;
                                    currentLibraryType = DefaultLibraryTypes.Render;
                                    demandChange = true;
                                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    return true;
                                case "AudioButton":
                                    selectedFlags |= 2;
                                    selectedFlags &= ~1;
                                    for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                    {
                                        selectedFlags &= ~(8 << i);
                                    }
                                    selectedFlags |= 8;
                                    currentRootType = LibraryRootType.Audio;
                                    currentLibraryType = DefaultLibraryTypes.SFX;
                                    demandChange = true;
                                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    return true;
                                case "HeaderButton":
                                    selectedFlags ^= 4;
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    return true;
                                case "PageLeftButton":
                                    if (page > 0)
                                    {
                                        page--;
                                        demandChange = true;
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    }
                                    else
                                    {
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    }
                                    return true;
                                case "PageRightButton":
                                    int maxPages = (int)Math.Ceiling((double)libraryFileCache[currentLibraryType].Count / 12);
                                    // If the last page is full of videos, add an extra page
                                    if(libraryFileCache[currentLibraryType].Count % 12 == 0)
                                        maxPages++;
                                    if (page < maxPages - 1)
                                    {
                                        page++;
                                        demandChange = true;
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    }
                                    else
                                    {
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    }
                                    return true;
                                default:
                                    // Check if it's a subtype button
                                    if (rect.Key.StartsWith(currentRootType.ToString()))
                                    {
                                        // Get index
                                        int index = libraryTypes[currentRootType].IndexOf(rect.Key.Substring(currentRootType.ToString().Length, rect.Key.Length - currentRootType.ToString().Length - 6));
                                        // Deslect all other subtypes
                                        for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                        {
                                            selectedFlags &= ~(8 << i);
                                        }
                                        // Select this subtype
                                        selectedFlags |= 8 << index;
                                        // Get subtype
                                        foreach (KeyValuePair<LibraryType, string> type in LibraryData.libraryNames)
                                        {
                                            if (type.Value == libraryTypes[currentRootType][index])
                                            {
                                                currentLibraryType = type.Key;
                                                break;
                                            }
                                        }
                                        demandChange = true;
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                        return true;
                                    }
                                    break;
                            }
                        }
                    }
                    // Check if it's a video holder
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int position = i + (j * 3) + (12 * page);
                            Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
                            Rectangle videoHolderRect = new Rectangle(GlobalGraphics.Scale(201 + (33 * i) + (i * 2)), GlobalGraphics.Scale(72 + (35 * j) + (j * 2)), GlobalGraphics.Scale(videoHolder.Width), GlobalGraphics.Scale(videoHolder.Height));
                            Rectangle staticRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(2), videoHolderRect.Y + GlobalGraphics.Scale(2), GlobalGraphics.Scale(29), GlobalGraphics.Scale(22));
                            // button 1: organize 3, 27 13x13
                            Rectangle button1Rect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(3), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(13), GlobalGraphics.Scale(13));
                            // button 2: organize 25, 27 13x13
                            Rectangle button2Rect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(25), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(13), GlobalGraphics.Scale(13));
                            bool add = false;
                            if (staticRect.Contains(MouseInput.MouseState.Position))
                            {
                                if (libraryFileCache[currentLibraryType].Count > position)
                                {
                                    // Open video with shell using default program
                                    LibraryFile file = libraryFileCache[currentLibraryType][position];
                                    if(file.Path != null)
                                    {
                                        ProcessStartInfo startInfo = new()
                                        {
                                            FileName = file.Path,
                                            UseShellExecute = true
                                        };
                                        Process.Start(startInfo);
                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    }
                                    else
                                    {
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    }
                                    return true;
                                }
                                else
                                {
                                    add = true;
                                }
                            }
                            else if (button1Rect.Contains(MouseInput.MouseState.Position))
                            {
                                // If there is a video in this position, this is the organize button
                                if (libraryFileCache[currentLibraryType].Count > position)
                                {
                                    // Replicate subtype objects
                                    Texture2D subTypeButton = GlobalContent.GetTexture("SubTypeButtonOrganize");
                                    for(int s = 0; s < libraryTypes[currentRootType].Count; s++)
                                    {
                                        Rectangle subTypeRect = new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71 + 13 * s), GlobalGraphics.Scale(subTypeButton.Width), GlobalGraphics.Scale(subTypeButton.Height));
                                        Global.mask.AddUnmaskedObject("SubType" + s, new SimpleObject(subTypeRect, Color.Gray, subTypeButton, () => {
                                            if(subTypeRect.Contains(MouseInput.MouseState.Position))
                                            {
                                                organizeFile = position;
                                                // Mouse position used to determine subtype button
                                                Vector2 mousePosition = MouseInput.MouseState.Position.ToVector2();
                                                for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                                {
                                                    Rectangle subTypeRect = new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71 + 13 * i), GlobalGraphics.Scale(subTypeButton.Width), GlobalGraphics.Scale(subTypeButton.Height));
                                                    if (subTypeRect.Contains(mousePosition))
                                                    {
                                                        organizeType = i;
                                                        break;
                                                    }
                                                }
                                                organizing = false;
                                                Global.mask.Disable();
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                                return true;
                                            }
                                            return false;
                                        }));
                                    }
                                    // Replicate videoplayer
                                    Texture2D videoPlayerTexture = currentRootType == LibraryRootType.Video ? videoPlayers[position] : GlobalContent.GetTexture("AudioAnim" + audioAnim);
                                    Global.mask.AddUnmaskedObject("VideoPlayer", new SimpleObject(staticRect, Color.White, videoPlayerTexture, () => {
                                        return false;
                                    }));
                                    // Replicate video holder
                                    Global.mask.AddUnmaskedObject("VideoHolder", new SimpleObject(videoHolderRect, Color.White, videoHolder, () => {
                                        if(!organizing)
                                            return false;
                                        // If button 1 is pressed, undo
                                        if (button1Rect.Contains(MouseInput.MouseState.Position))
                                        {
                                            organizing = false;
                                            organizeFile = -1;
                                            organizeType = -1;
                                            Global.mask.Disable();
                                            GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                            return true;
                                        }
                                        return false;
                                    }));
                                    // Activate mask
                                    Global.mask.Enable();
                                    organizing = true;
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    return true;
                                }
                                else
                                {
                                    add = true;
                                }
                            }
                            else if (button2Rect.Contains(MouseInput.MouseState.Position))
                            {
                                // Remove video button
                                if (libraryFileCache[currentLibraryType].Count > position)
                                {
                                    // Remove video
                                    LibraryFile file = libraryFileCache[currentLibraryType][position];
                                    LibraryData.Unload(file);
                                    libraryFileCache[currentLibraryType].RemoveAt(position);
                                    demandChange = true;
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                    return true;
                                }
                                else
                                {
                                    add = true;
                                }
                            }
                            if(add)
                            {
                                // Add button: Open file dialog with filters from library type
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                if(!currentLibraryType.Special)
                                {
                                    string filter = LibraryData.libraryNames[currentLibraryType] + "|";
                                    foreach (string extension in LibraryData.libraryFileTypes[currentLibraryType])
                                    {
                                        filter += "*" + extension + ";";
                                    }
                                    // Trim last semicolon
                                    filter = filter[..^1];
                                    // Add all files filter
                                    filter += "|All files|*.*";
                                    System.Windows.Forms.OpenFileDialog openFileDialog = new()
                                    {
                                        Filter = filter,
                                        Multiselect = true,
                                        Title = "Add " + LibraryData.libraryNames[currentLibraryType]
                                    };
                                    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                    {
                                        bool success = true;
                                        foreach (string file in openFileDialog.FileNames)
                                        {
                                            LibraryFile libraryFile = new(Path.GetFileNameWithoutExtension(file), file, currentLibraryType);
                                            LibraryFile? newFile = LibraryData.Load(libraryFile);
                                            if(newFile == null)
                                            {
                                                success = false;
                                                break;
                                            }
                                            libraryFileCache[currentLibraryType].Add(newFile);
                                        }
                                        demandChange = true;
                                        if(!success)
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("AddSource").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                        }
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
                // Hovering over video holders will set tooltip
                tooltip = "";
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int position = i + (j * 3) + (12 * page);
                        Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
                        Rectangle videoHolderRect = new Rectangle(GlobalGraphics.Scale(201 + (33 * i) + (i * 2)), GlobalGraphics.Scale(72 + (35 * j) + (j * 2)), GlobalGraphics.Scale(videoHolder.Width), GlobalGraphics.Scale(videoHolder.Height));
                        if (videoHolderRect.Contains(MouseInput.MouseState.Position))
                        {
                            if (libraryFileCache[currentLibraryType].Count > position)
                            {
                                LibraryFile file = libraryFileCache[currentLibraryType][position];
                                if (file.Path != null)
                                {
                                    tooltip = Path.GetFileName(file.Path);
                                }
                            }
                            else
                            {
                                tooltip = "Add Media";
                            }
                        }
                    }
                }
            }
            // Interactable
            if(controller.Update(gameTime, handleInput))
                return true;
            return false;
        }
    }
}