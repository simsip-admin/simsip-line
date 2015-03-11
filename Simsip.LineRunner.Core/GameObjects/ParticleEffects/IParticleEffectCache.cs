using Microsoft.Xna.Framework;
using System.Collections.Generic;
using BEPUphysics.CollisionTests;
using Simsip.LineRunner.GameFramework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using ProjectMercury;


namespace Simsip.LineRunner.GameObjects.ParticleEffects
{
    public interface IParticleEffectCache : IUpdateable, IDrawable
    {
        /// <summary>
        /// Handle particle effect category specific game state changes.
        /// </summary>
        /// <param name="state">The game state we are switching to.</param>
        void SwitchState(GameState state);

        /// <summary>
        /// Our current collection of particle effects.
        /// </summary>
        Dictionary<GameModel, IList<ParticleEffectDesc>> ParticleEffectEntries { get; }
        
        /// <summary>
        /// Called from our b2ContactListener whenever we hit a game object that causes
        /// a particle effect to be created.
        /// 
        /// Depending on an admin setting, we may skip creating the particle effect.
        /// </summary>
        /// <param name="obstacleModel">The model that was hit.</param>
        void AddParticleEffect(GameModel gameModel, Contact theContact);
    }
}