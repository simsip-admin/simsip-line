using Cocos2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simsip.LineRunner.Data;
using System;
using System.Diagnostics;
using Simsip.LineRunner.Utils;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.GameFramework;


namespace Simsip.LineRunner
{
    public class TheGame : Game
    {
        // Staging for controlling fps in future release
        private const float timeToNextUpdate = 1.0f / 30.0f;
        private float timeSinceLastUpdate;

        /// <summary>
        /// Used to signal first stage of initialization is done.
        /// </summary>
        public bool Ready { get; private set; }

        /// <summary>
        /// Allow other components to easily access Game members (e.g., GraphicsDevice)
        /// </summary>
        public static TheGame SharedGame { get; private set; }

        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        /// <summary>
        /// Needs to be exposed for particles. 
        /// 
        /// See ParticleCache.Initialize for details.
        /// </summary>
        public GraphicsDeviceManager TheGraphicsDeviceManager
        {
            get
            {
                return this._graphicsDeviceManager;
            }
        }

        public TheGame()
        {
            this._graphicsDeviceManager = new GraphicsDeviceManager(this);

            this.Content.RootDirectory = "Content";

            this._graphicsDeviceManager.IsFullScreen = true;
            this._graphicsDeviceManager.PreferredBackBufferWidth = Math.Min(Window.ClientBounds.Height, Window.ClientBounds.Width);
            this._graphicsDeviceManager.PreferredBackBufferHeight = Math.Max(Window.ClientBounds.Height, Window.ClientBounds.Width);

            // Needed for stencil clipping via CCClippingNode
            this._graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            this.TimeStepDuration = 1f / 30;
            TargetElapsedTime = TimeSpan.FromTicks((int)(this.TimeStepDuration * 10000000));
            // Original:
            // Frame rate is 30 fps by default for Windows Phone.
            // TargetElapsedTime = TimeSpan.FromTicks(333333);
            // Divide by 2 to make it 60 fps
            // TargetElapsedTime = TimeSpan.FromTicks(333333 / 2);

            // Extend battery life under lock.
            // InactiveSleepTime = TimeSpan.FromSeconds(1);

            CCApplication application = new AppDelegate(this, this._graphicsDeviceManager);
            Components.Add(application);

            TheGame.SharedGame = this;
        }

        /// <summary>
        /// Our defined frame rate.
        /// 
        /// Example: 1f/30 equals 30 fps
        /// </summary>
        public float TimeStepDuration { get; private set; }

        protected override void Initialize()
        {
            // IMPORTANT: Due to awaits below on other platforms, we call this first
            base.Initialize();

            // Get folder structure in place if not done yet
            FileUtils.InitializeFolders();

            // Copy over our database if not copied yet
            if (!Database.Exists())
            {
                Database.CopyFromAssets();
            }

            // Take care of any database upgrade needs
            Database.HandleUpgrade();

            Debug.WriteLine("After Database copy");

            // Initialize user defaults
            UserDefaults.Initialize();
            if (!UserDefaults.SharedUserDefault.ContainsKey(GameConstants.USER_DEFAULT_KEY_INSTALL_DATE))
            {
                UserDefaults.SharedUserDefault.SetDateForKey(GameConstants.USER_DEFAULT_KEY_INSTALL_DATE, DateTime.Now);
            }

            Debug.WriteLine("After UserDefaults.Initialize()");

            // Signal first stage of initialization is complete
            this.Ready = true;
        }

        protected override void Update(GameTime gameTime)
        {
            // Protect against battery drain
            if (!this.IsActive)
            {
                return;
            }

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                ProcessBackClick();
            }

            // Now let all other game compenents update
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Protect against battery drain
            if (!this.IsActive)
            {
                return;
            }

            // Draw game
            base.Draw(gameTime);

            /*
            timeSinceLastUpdate += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceLastUpdate >= timeToNextUpdate)
            {
                // Update game
                base.Draw(gameTime);

                timeSinceLastUpdate = 0;
            }

            // Systems you don't want to limit would be updated here
            */
        }

        private void ProcessBackClick()
        {
            if (CCDirector.SharedDirector.CanPopScene)
            {
                CCDirector.SharedDirector.PopScene();
            }
            else
            {
                // TODO: New Monogame
                // Exit();
            }
        }
    }
}