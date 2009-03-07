using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Xml;
using MyFirstGame.GameObject;
using MyFirstGame.InputObject;
using MyFirstGame.LevelObject;
using MyFirstGame.References;
using MyFirstGame.Utilities;
using MyFirstGame.Graphics;
using SpriteSheetRuntime;

namespace MyFirstGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private Resolution resolution;
        private Rectangle viewportRectangle;
        private SpriteBatch spriteBatch;
        
        private List<PlayerSprite> players;
        private List<Level> levels;
        private int currentLevel;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            players = new List<PlayerSprite>();
            levels = new List<Level>();
            resolution = new Resolution(graphics, ScreenMode.tv720p);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            LoadSettings();
            LoadTextures();            
            LoadViewport();
            LoadPlayers();
            LoadLevels();
        }           

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Settings.Instance.GameTime = gameTime;

#if !XBOX
            //TODO: factor this out into a Menu item or something, also when you hold F1 down it keeps switching rezzes
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                if (resolution.Mode == ScreenMode.tv720p)
                    resolution.Mode = ScreenMode.SVGA;
                else
                    resolution.Mode = ScreenMode.tv720p;
                resolution.SetResolution(graphics);
            }
#endif

            //TODO: this is just to loop the level over and over for debug
            if (levels[0].IsEnded)
            {
                levels[0] = new TestLevel();
            }

            //TODO: this will not be hardcoded (this forces level start when game is loaded)
            if (!levels[0].IsStarted)
            {
                levels[0].StartLevel();
            }

            if (!levels[0].IsEnded)
            {
                foreach (PlayerSprite player in players)
                {
                    UpdatePlayer(player);
                }
                
                levels[0].UpdateLevel();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, resolution.Scale);
            
            //Draw level bg and its sprites
            spriteBatch.Draw(Textures.Instance.SpriteSheet.Texture, viewportRectangle, Textures.Instance.SpriteSheet.SourceRectangle(levels[0].BackgroundSpriteSheetName),
                Color.White, 0.0f, new Vector2(0, 0), SpriteEffects.None, 1.0f);

            foreach (Sprite sprite in levels[0].Sprites)
            {
                spriteBatch.Draw(Textures.Instance.SpriteSheet.Texture, sprite.DrawRectangle, Textures.Instance.SpriteSheet.SourceRectangle(sprite.GetSpriteSheetIndex()),
                    Color.White, sprite.Rotation, new Vector2(0,0), SpriteEffects.None, 0.9f);

            }

            //Draw players
            foreach (PlayerSprite player in players)
            {
                if (player.IsActive)
                {
                    Color playerColor = player.SpriteColor;
                    //TODO: this should be done in player class
                    if (player.IsFiring)
                    {
                        playerColor = Color.Red;
                    }
                    //player.Rotation += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    spriteBatch.Draw(Textures.Instance.SpriteSheet.Texture, player.DrawRectangle, Textures.Instance.SpriteSheet.SourceRectangle(player.GetSpriteSheetIndex()),
                        playerColor, player.Rotation, player.Origin, SpriteEffects.None, 0.0f);
                    
                    #if DEBUG
                    spriteBatch.Draw(Textures.Instance.SpriteSheet.Texture, new Rectangle((int)player.GetShotLocation().X, (int)player.GetShotLocation().Y, 5, 5), Textures.Instance.SpriteSheet.SourceRectangle("hitbox"),
                        playerColor, 0.0f, new Vector2(0,0), SpriteEffects.None, 0.0f);
                    #endif

                }
            }

            //draw targets
            //TODO: of course this has to be the current, i.e. remove all references to levels[0]
            if (!levels[0].IsEnded)
            {
                foreach (Target target in levels[0].Waves[levels[0].CurrentWaveIndex].Targets)
                {                    
                    if (target.IsActive)
                    {
                        //target.Rotation += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        spriteBatch.Draw(Textures.Instance.SpriteSheet.Texture, target.DrawRectangle, Textures.Instance.SpriteSheet.SourceRectangle(target.GetSpriteSheetIndex()),
                            Color.White, target.Rotation, target.Origin, SpriteEffects.None, 0.5f);

                        #if DEBUG
                        spriteBatch.Draw(Textures.Instance.SpriteSheet.Texture, target.BoundingBox, Textures.Instance.SpriteSheet.SourceRectangle("hitbox"),
                            Color.White, 0.0f, new Vector2(0,0), SpriteEffects.None, 0.6f);
                        #endif
                    }
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }



        //TODO: refactor to PlayerSprite()
        private void UpdatePlayer(PlayerSprite player)
        {
            player.UpdatePlayerIsActive();
            if (player.IsActive)
            {
                player.UpdatePauseState();
                if (player.IsPaused)
                    this.Exit();

                player.UpdateFiringState();
                if (player.IsFiring)
                {
                    Vector2 shotLocation = player.GetShotLocation();
                    foreach (Target target in levels[0].Waves[levels[0].CurrentWaveIndex].Targets)
                    {
                        //TODO: factor this out so pixel based hit detection can be used elsewhere
                        //TODO: pixel based hit detection does not work when target is rotated...
                        
                        //only do pixel detection if mouse in area
                        if (target.BoundingBox.Contains(new Rectangle((int)shotLocation.X, (int)shotLocation.Y, 1, 1)))
                        {
                            //do we hit alpha pixel or 'body' pixel?                            
                            
                            Vector2 relativeShotLocation = new Vector2(shotLocation.X - target.BoundingBox.X, shotLocation.Y - target.BoundingBox.Y);
                            relativeShotLocation = Vector2.Divide(relativeShotLocation, target.Scale);
                            
                            Rectangle targetTexturePixels = Textures.Instance.SpriteSheet.SourceRectangle(target.GetSpriteSheetIndex());
                            Vector2 hitPointOnSpriteSheet = new Vector2(targetTexturePixels.X + relativeShotLocation.X, targetTexturePixels.Y + relativeShotLocation.Y);
                            
                            int hitPointOnSpriteSheetIndex = (int)hitPointOnSpriteSheet.Y * Textures.Instance.SpriteSheet.Texture.Width + (int)hitPointOnSpriteSheet.X;
                            Color pixelColor = Textures.Instance.SpriteSheetColors[hitPointOnSpriteSheetIndex];
                            
                            if (pixelColor.A != 0)
                            {
                                target.IsActive = false;
                                //TODO: give points to correct player...                        
                            }                            
                        }
                    }
                }

                player.UpdatePlayerPosition();
                //TODO: here is where we'd check for collisions with other objects, change target pos
                player.MoveTo(new Vector2(player.Position.X, player.Position.Y));
            }
        }
        
        private void LoadPlayers()
        {
#if !XBOX
            PlayerInput[] playerInputs = LoadPCPlayerInputs(); // move to Initialize?
#endif
#if XBOX
            PlayerInput[] playerInputs = LoadXBOXPlayerInputs(); // move to Initialize?
#endif
            int playerNumber = 1;
            foreach (PlayerInput playerInput in playerInputs)
            {
                players.Add(LoadPlayer(playerInput, playerNumber));
                playerNumber += 1;
            }
        }

        private PlayerSprite LoadPlayer(PlayerInput playerInput, int playerNumber)
        {
            //TODO: this should be done via different sprites
            Color playerColor = Color.White;
            switch (playerNumber)
            {
                case 1:
                    playerColor = Color.Brown;
                    break;
                case 2:
                    playerColor = Color.Blue;
                    break;
                case 3:
                    playerColor = Color.DimGray;
                    break;
                case 4:
                    playerColor = Color.Green;
                    break;
            }
            PlayerSprite player = new PlayerSprite(playerInput, playerColor, playerNumber);
            player.Position = new Vector2(Settings.Instance.ScreenSize.X / 2, Settings.Instance.ScreenSize.X / 2);
            return player;
        }

#if !XBOX
        private PlayerInput[] LoadPCPlayerInputs()
        {
            PlayerInput[] playerInputs = new PlayerInput[4];
            XmlDocument configDocument = new XmlDocument();
            configDocument.Load(".//Config//PCconfig.xml");
            XmlNodeList inputNodes = configDocument.SelectNodes("/config/input");
            try
            {
                for (int i = 0; i < playerInputs.Length; i++)
                {
                    XmlNode inputNode = inputNodes[i];
                    string activeInputAttribute = inputNode.Attributes["activeInput"].Value;
                    float scrollSpeed;

                    if (String.Compare(activeInputAttribute, "Keyboard", true) == 0)
                    {
                        scrollSpeed = float.Parse(inputNode.Attributes["scrollSpeed"].Value);
                        playerInputs[i] = new KeyboardInput(scrollSpeed);
                    }
                    else if (String.Compare(activeInputAttribute, "Wiimote", true) == 0)
                    {
                        //if we have Wiimote, Mouse, Wiimote, 
                        //the last Wiimote is player 3 but Wiimote index 1.
                        int numWiimotePlayers = 0;
                        foreach (PlayerInput input in playerInputs)
                        {
                            if (input is WiiInput)
                            {
                                numWiimotePlayers += 1;
                            }
                        }
                        playerInputs[i] = new WiiInput(numWiimotePlayers);
                    }
                    else if (String.Compare(activeInputAttribute, "Mouse", true) == 0)
                    {
                        playerInputs[i] = new MouseInput();
                    }
                    else
                    {
                        //if we have Gamepad, Mouse, Gamepad
                        //the last Gamepad is player 3 but Gamepad PlayerIndex.Two.
                        int numGamepadPlayers = 0;
                        foreach (PlayerInput input in playerInputs)
                        {
                            if (input is GamepadInput)
                            {
                                numGamepadPlayers += 1;
                            }
                        }
                        scrollSpeed = float.Parse(inputNode.Attributes["scrollSpeed"].Value);
                        playerInputs[i] = new GamepadInput(UtilityMethods.NumToEnum<PlayerIndex>(numGamepadPlayers), scrollSpeed);
                    }
                }
                return playerInputs;
            }
            catch (Exception ex)
            {
                throw;
            }            
        }
#endif

#if XBOX
        private PlayerInput[] LoadXBOXPlayerInputs()
        {
            PlayerInput[] playerInputs = new PlayerInput[4];
            XmlDocument configDocument = new XmlDocument();
            configDocument.Load(".//Config//XBOXconfig.xml");
            XmlNodeList inputNodes = configDocument.SelectNodes("/config/input");
            try
            {
                for (int i = 0; i < playerInputs.Length; i++)
                {
                    XmlNode inputNode = inputNodes[i];
                    float scrollSpeed = float.Parse(inputNode.Attributes["scrollSpeed"].Value);
                    playerInputs[i] = new GamepadInput(UtilityMethods.NumToEnum<PlayerIndex>(i), scrollSpeed);
                }
                return playerInputs;
            }
            catch (Exception ex)
            {
                throw;
            }            
        }

#endif

        private void LoadSettings()
        {
            References.Settings.Instance.ScreenSize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            References.Settings.Instance.AspectRatio = GraphicsDevice.Viewport.AspectRatio;
        }

		private void LoadLevels()
        {
            levels.Add(new TestLevel());
        }

        private void LoadTextures()
        {
            Textures.Instance.SpriteSheet = Content.Load<SpriteSheet>("Sprites\\SpriteSheet");
            Color[] spriteSheetColors = new Color[Textures.Instance.SpriteSheet.Texture.Width * Textures.Instance.SpriteSheet.Texture.Height];
            Textures.Instance.SpriteSheet.Texture.GetData<Color>(spriteSheetColors);
            Textures.Instance.SpriteSheetColors = spriteSheetColors;
        }

        private void LoadViewport()
        {
            viewportRectangle = new Rectangle(0, 0, (int)Settings.Instance.ScreenSize.X, (int)Settings.Instance.ScreenSize.Y);         
        }
       
    }
}
