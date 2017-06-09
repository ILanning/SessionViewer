using CommonCode;
using CommonCode.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace SessionViewer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Color ClearColor = Color.Black;
        public GameTime globalTime;
        ScreenManager screenManager;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = ".\\Content";
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;

            InputManager inputManager = new InputManager(this);
            Components.Add(inputManager);
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
            //AudioManager audioManager = new AudioManager(this);
            //Components.Add(audioManager);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            GameSettings.Initialize(Path.Combine(Content.RootDirectory, "settings.txt"));

            Window.Title = "Session Viewer";
            Window.AllowUserResizing = false;
            IsMouseVisible = true;
            Window.ClientSizeChanged += OnClientSizeChanged;
            ScreenManager.AddScreen(new MainMenuScreen());
            DynamicContentManager.RegisterCustomLoader(typeof(SessionManager), SessionManager.BuildSession);
            DynamicContentManager.RegisterCustomLoader(typeof(Skaia), Skaia.BuildSkaia);
            DynamicContentManager.RegisterCustomLoader(typeof(GenericMeteor), GenericMeteor.BuildGenericMeteor);
            DynamicContentManager.RegisterCustomLoader(typeof(PlayerWorld), PlayerWorld.BuildPlayerWorld);
            DynamicContentManager.RegisterCustomLoader(typeof(SessionLocation), SessionLocation.BuildSessionLocation);
        }

        protected bool IsFullScreen
        {
            get { return graphics.IsFullScreen; }
            set
            {
                if (value != graphics.IsFullScreen)
                {
                    graphics.IsFullScreen = !graphics.IsFullScreen;
                    graphics.ApplyChanges();
                }
            }
        }

        protected void OnClientSizeChanged(object sender, EventArgs e)
        {
            ResetRenderStates();
        }

        protected void ResetRenderStates()
        {
            ScreenManager.Globals.Camera.RemakeProjection();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ScrollBar.spriteSheet = ScreenManager.Content.Load<Texture2D>(".\\UI\\Scroll Bar.png");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            globalTime = gameTime;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
