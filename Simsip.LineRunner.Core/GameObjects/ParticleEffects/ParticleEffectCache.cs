using Engine.Common.Logging;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using ProjectMercury.Renderers;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Physics;
using System.Collections.Generic;
using Simsip.LineRunner.Utils;
using System.Linq;
using BEPUphysics.CollisionTests;
using ConversionHelper;


namespace Simsip.LineRunner.GameObjects.ParticleEffects
{
    public class ParticleEffectCache : DrawableGameComponent, IParticleEffectCache
    {
        // Required services
        private SpriteBatchRenderer _spriteBatchRenderer;

        // State we maintain
        private GameState _currentGameState;

        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public ParticleEffectCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(IParticleEffectCache), this); 
        }

        #region Properties

        public Dictionary<GameModel, IList<ParticleEffectDesc>> ParticleEffectEntries { get; private set; }
        
        #endregion

        #region DrawableGameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Initialize state
            this.ParticleEffectEntries = new Dictionary<GameModel, IList<ParticleEffectDesc>>();
            
            // Create required services
            this._spriteBatchRenderer = new SpriteBatchRenderer
            {
                GraphicsDeviceService = TheGame.SharedGame.TheGraphicsDeviceManager
            };
            this._spriteBatchRenderer.LoadContent(this.Game.Content);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // Loop over all particle effect descs (Note: backwards so we can remove if neccessary
            var keys = new List<GameModel>(ParticleEffectEntries.Keys);
            foreach (GameModel key in keys)
            {
                // Has this particle effect entry expired?
                // TODO: We only check the first ParticleEffectDesc in the ParticleEffectEntry.
                //       May want to enhance to have various durtions for ParticleEffectDescs in a ParticleEffectEntry.
                float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                var testParticleEffectDesc = this.ParticleEffectEntries[key][0];
                testParticleEffectDesc.TotalParticleEffectTime += deltaSeconds;
                if (testParticleEffectDesc.TotalParticleEffectTime > GameConstants.DURATION_PARTICLE_DURATION)
                {
                    // Clean up particle effect entry
                    foreach (var particleEffectDesc in ParticleEffectEntries[key])
                    {
                        particleEffectDesc.TheParticleEffect.Terminate();
                    }
                    this.ParticleEffectEntries.Remove(key);

                }
                else
                {
                    // Ok, this particle effect entry is still live, update via
                    // its defined trigger
                    var cameraType = XNAUtils.CameraType.Tracking;
                    if (this._currentGameState == GameState.World)
                    {
                        cameraType = XNAUtils.CameraType.Player;
                    }

                    foreach (var particleEffectDesc in ParticleEffectEntries[key])
                    {
                        particleEffectDesc.Trigger(particleEffectDesc, gameTime, cameraType);
                    }    
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // We need to short-circuit when we are in Refresh state they are reinitializing this game object's
            // state on a background thread. Note that the Update() short circuit for Refresh is handled
            // in the ActionLayer.Update.
            if (this._currentGameState == GameState.Refresh)
            {
                return;
            }

            var keys = new List<GameModel>(ParticleEffectEntries.Keys);
            foreach (GameModel key in keys)
            {
                foreach (var particleEffectDesc in ParticleEffectEntries[key])
                {
                    this._spriteBatchRenderer.RenderEffect(particleEffectDesc.TheParticleEffect);
                }
            }
        }

        #endregion

        #region IParticleCache Implementation

        public void AddParticleEffect(GameModel gameModel, Contact theContact)
        {
            // We can turn off particles via an admin setting on the Admin screen
            if (!GameManager.SharedGameManager.AdminIsParticlesAllowed)
            {
                return;
            }

            // We currently only allow one particle effect to display at a time
            if (this.ParticleEffectEntries.Count > 0)
            {
                return;
            }

            // Does the contacted model have any particle effects?
            var particleEffectDescs = gameModel.ParticleEffectDescs;
            if (particleEffectDescs == null)
            {
                return;
            }

            // Have we already recorded this contact?
            if (this.ParticleEffectEntries.ContainsKey(gameModel))
            {
                return;
            }

            // Loop over all particle effects for this model
            foreach(var particleEffectDesc in particleEffectDescs)
            {
                // Fill out the rest of the fields in the particle effect desc 
                // for this particle effect
                particleEffectDesc.TheGameModel = gameModel;
                particleEffectDesc.TheContact = theContact;
                particleEffectDesc.TheParticleEffect = ParticleEffectFactory.Create(particleEffectDesc);
                particleEffectDesc.TotalParticleEffectTime = 0f;
            }

            // Grab reference to this updated particle desc list
            this.ParticleEffectEntries[gameModel] = particleEffectDescs;
        }

        public void SwitchState(GameState state)
        {
            switch (state)
            {
                case GameState.World:
                    {
                        // Currently a no-op
                        break;
                    }
                case GameState.Intro:
                    {
                        // Set state
                        break;
                    }
                case GameState.Moving:
                    {
                        // Currently a no-op
                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        break;
                    }
                case GameState.MovingToStart:
                    {
                        break;
                    }
                case GameState.Refresh:
                    {

                        // No-op for now
                        break;
                    }
                case GameState.Start:
                    {
                        break;
                    }
            }

            // Update our state
            this._currentGameState = state;
        }

        #endregion

    }
}