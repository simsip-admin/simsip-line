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

        public Dictionary<GameModel, IList<ParticleEffectDesc>> DisplayParticleEffectEntries { get; private set; }

        public IList<IList<ParticleEffectDesc>> FinishParticleEffectEntries { get; private set; }
        
        public Dictionary<GameModel, IList<ParticleEffectDesc>> HitParticleEffectEntries { get; private set; }
        
        #endregion

        #region DrawableGameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Initialize state
            this.DisplayParticleEffectEntries = new Dictionary<GameModel, IList<ParticleEffectDesc>>();
            this.FinishParticleEffectEntries = new List<IList<ParticleEffectDesc>>();
            this.HitParticleEffectEntries = new Dictionary<GameModel, IList<ParticleEffectDesc>>();
            
            // CreateLineHitParticles required services
            this._spriteBatchRenderer = new SpriteBatchRenderer
            {
                GraphicsDeviceService = TheGame.SharedGame.TheGraphicsDeviceManager
            };
            this._spriteBatchRenderer.LoadContent(this.Game.Content);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            this.UpdateDisplayParticleEffects(gameTime);
            this.UpdateFinishParticleEffects(gameTime);
            this.UpdateHitParticleEffects(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var displayKeys = new List<GameModel>(DisplayParticleEffectEntries.Keys);
            foreach (GameModel key in displayKeys)
            {
                foreach (var particleEffectDesc in DisplayParticleEffectEntries[key])
                {
                    this._spriteBatchRenderer.RenderEffect(particleEffectDesc.TheParticleEffect);
                }
            }

            foreach (var entry in FinishParticleEffectEntries)
            {
                foreach (var particleEffectDesc in entry)
                {
                    this._spriteBatchRenderer.RenderEffect(particleEffectDesc.TheParticleEffect);
                }
            }

            var hitKeys = new List<GameModel>(HitParticleEffectEntries.Keys);
            foreach (GameModel key in hitKeys)
            {
                foreach (var particleEffectDesc in HitParticleEffectEntries[key])
                {
                    this._spriteBatchRenderer.RenderEffect(particleEffectDesc.TheParticleEffect);
                }
            }
        }

        #endregion

        #region IParticleCache Implementation

        public void AddDisplayParticleEffect(GameModel gameModel, CustomContentManager customContentManager)
        {
            // We can turn off particles via an admin setting on the Admin screen
            if (!GameManager.SharedGameManager.AdminAreParticlesAllowed)
            {
                return;
            }

            // Does the model to be displayed have any particle effects?
            var particleEffectDescs = gameModel.DisplayParticleEffectDescs;
            if (particleEffectDescs == null)
            {
                return;
            }

            // Have we already recorded this contact?
            if (this.DisplayParticleEffectEntries.ContainsKey(gameModel))
            {
                return;
            }

            // Loop over all particle effects for this model
            foreach (var particleEffectDesc in particleEffectDescs)
            {
                // Fill out the rest of the fields in the particle effect desc 
                // for this particle effect
                particleEffectDesc.TheGameModel = gameModel;
                particleEffectDesc.TheParticleEffect = ParticleEffectFactory.Create(particleEffectDesc, customContentManager);
                particleEffectDesc.TotalParticleEffectTime = 0f;
            }

            // Grab reference to this updated particle desc list
            this.DisplayParticleEffectEntries[gameModel] = particleEffectDescs;
        }

        public void TerminateAllObstacleEffects()
        {
            var keys = this.DisplayParticleEffectEntries.Keys;
            foreach (var key in keys)
            {
                foreach (var particleEffectDesc in this.DisplayParticleEffectEntries[key])
                {
                    particleEffectDesc.TheParticleEffect.Terminate();
                }
            }
            this.DisplayParticleEffectEntries.Clear();

            keys = this.HitParticleEffectEntries.Keys;
            foreach (var key in keys)
            {
                foreach (var particleEffectDesc in this.HitParticleEffectEntries[key])
                {
                    particleEffectDesc.TheParticleEffect.Terminate();
                }
            }
            this.HitParticleEffectEntries.Clear();
        }

        public void TerminateAllFinishEffects()
        {
            foreach (var finishParticleEffectEntry in FinishParticleEffectEntries)
            {
                foreach (var particleEffectDesc in finishParticleEffectEntry)
                {
                    particleEffectDesc.TheParticleEffect.Terminate();
                }
            }
            this.FinishParticleEffectEntries.Clear();
        }

        public void AddFinishParticleEffect(IList<ParticleEffectDesc> particleEffectDescs, CustomContentManager customContentManager)
        {
            // We can turn off particles via an admin setting on the Admin screen
            if (!GameManager.SharedGameManager.AdminAreParticlesAllowed)
            {
                return;
            }

            // Loop over all particle effects for this model
            foreach (var particleEffectDesc in particleEffectDescs)
            {
                // Fill out the rest of the fields in the particle effect desc 
                // for this particle effect
                particleEffectDesc.TheParticleEffect = ParticleEffectFactory.Create(particleEffectDesc, customContentManager);
                particleEffectDesc.TotalParticleEffectTime = 0f;
            }

            // Grab reference to this updated particle desc list
            this.FinishParticleEffectEntries.Add(particleEffectDescs);

        }

        public void AddHitParticleEffect(GameModel gameModel, Contact theContact, CustomContentManager customContentManager)
        {
            // We can turn off particles via an admin setting on the Admin screen
            if (!GameManager.SharedGameManager.AdminAreParticlesAllowed)
            {
                return;
            }

            // We currently only allow one particle effect to display at a time
            if (this.HitParticleEffectEntries.Count > 0)
            {
                return;
            }

            // Does the contacted model have any particle effects?
            var particleEffectDescs = gameModel.HitParticleEffectDescs;
            if (particleEffectDescs == null)
            {
                return;
            }

            // Have we already recorded this contact?
            if (this.HitParticleEffectEntries.ContainsKey(gameModel))
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
                particleEffectDesc.TheParticleEffect = ParticleEffectFactory.Create(particleEffectDesc, customContentManager);
                particleEffectDesc.TotalParticleEffectTime = 0f;
            }

            // Grab reference to this updated particle desc list
            this.HitParticleEffectEntries[gameModel] = particleEffectDescs;
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

        #region Helper methods

        private void UpdateDisplayParticleEffects(GameTime gameTime)
        {
            // Loop over all particle effect descs (Note: backwards so we can remove if neccessary
            var keys = new List<GameModel>(DisplayParticleEffectEntries.Keys);

            foreach (GameModel key in keys)
            {
                // Has this particle effect entry expired?
                // TODO: We only check the first ParticleEffectDesc in the ParticleEffectEntry.
                //       May want to enhance to have various durtions for ParticleEffectDescs in a ParticleEffectEntry.
                float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                var testParticleEffectDesc = this.DisplayParticleEffectEntries[key][0];
                testParticleEffectDesc.TotalParticleEffectTime += deltaSeconds;
                if (testParticleEffectDesc.TotalParticleEffectTime > GameConstants.DURATION_PARTICLE_DURATION)
                {
                    // Clean up particle effect entry
                    foreach (var particleEffectDesc in DisplayParticleEffectEntries[key])
                    {
                        particleEffectDesc.TheParticleEffect.Terminate();
                    }
                    this.DisplayParticleEffectEntries.Remove(key);

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

                    foreach (var particleEffectDesc in DisplayParticleEffectEntries[key])
                    {
                        particleEffectDesc.Trigger(particleEffectDesc, gameTime, cameraType);
                    }
                }
            }
        }

        private void UpdateFinishParticleEffects(GameTime gameTime)
        {
            // Loop over all particle effect descs (Note: backwards so we can remove if neccessary
            foreach(var entry in FinishParticleEffectEntries.ToList())
            {
                // Has this particle effect entry expired?
                // TODO: We only check the first ParticleEffectDesc in the ParticleEffectEntry.
                //       May want to enhance to have various durtions for ParticleEffectDescs in a ParticleEffectEntry.
                float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                var testParticleEffectDesc = entry[0];
                testParticleEffectDesc.TotalParticleEffectTime += deltaSeconds;
                if (testParticleEffectDesc.TotalParticleEffectTime > GameConstants.DURATION_PARTICLE_DURATION)
                {
                    // Clean up particle effect entry
                    foreach (var particleEffectDesc in entry)
                    {
                        particleEffectDesc.TheParticleEffect.Terminate();
                    }
                    this.FinishParticleEffectEntries.Remove(entry);
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

                    foreach (var particleEffectDesc in entry)
                    {
                        particleEffectDesc.Trigger(particleEffectDesc, gameTime, cameraType);
                    }
                }
            }
        }

        private void UpdateHitParticleEffects(GameTime gameTime)
        {
            // Loop over all particle effect descs (Note: backwards so we can remove if neccessary
            var keys = new List<GameModel>(HitParticleEffectEntries.Keys);

            foreach (GameModel key in keys)
            {
                // Has this particle effect entry expired?
                // TODO: We only check the first ParticleEffectDesc in the ParticleEffectEntry.
                //       May want to enhance to have various durtions for ParticleEffectDescs in a ParticleEffectEntry.
                float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                var testParticleEffectDesc = this.HitParticleEffectEntries[key][0];
                testParticleEffectDesc.TotalParticleEffectTime += deltaSeconds;
                if (testParticleEffectDesc.TotalParticleEffectTime > GameConstants.DURATION_PARTICLE_DURATION)
                {
                    // Clean up particle effect entry
                    foreach (var particleEffectDesc in HitParticleEffectEntries[key])
                    {
                        particleEffectDesc.TheParticleEffect.Terminate();
                    }
                    this.HitParticleEffectEntries.Remove(key);

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

                    foreach (var particleEffectDesc in HitParticleEffectEntries[key])
                    {
                        particleEffectDesc.Trigger(particleEffectDesc, gameTime, cameraType);
                    }
                }
            }
        }

        #endregion
    }
}