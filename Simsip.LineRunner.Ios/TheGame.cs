using Cocos2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simsip.LineRunner.Data;
using System;
using System.Diagnostics;
using Simsip.LineRunner.Utils;


namespace Simsip.LineRunner
{
    public class TheGame : Game
    {
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
            
            this._graphicsDeviceManager.IsFullScreen = false;

            // Frame rate is 30 fps by default for Windows Phone.
            // Divide by 2 to make it 60 fps
            TargetElapsedTime = TimeSpan.FromTicks(333333 / 2);

            // Extend battery life under lock.
            // InactiveSleepTime = TimeSpan.FromSeconds(1);

            CCApplication application = new AppDelegate(this, this._graphicsDeviceManager);
            Components.Add(application);

            TheGame.SharedGame = this;
        }

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

            Debug.WriteLine("After UserDefaults.Initialize()");

            // Signal first stage of initialization is complete
            this.Ready = true;
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                ProcessBackClick();
            }

            // Now let all other game compenents update
            base.Update(gameTime);
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