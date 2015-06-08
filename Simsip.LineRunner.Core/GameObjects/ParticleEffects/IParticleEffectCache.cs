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
        /// Our current collection of display particle effects.
        /// </summary>
        Dictionary<GameModel, IList<ParticleEffectDesc>> DisplayParticleEffectEntries { get; }

        /// <summary>
        /// Our current collection of hit particle effects.
        /// </summary>
        Dictionary<GameModel, IList<ParticleEffectDesc>> HitParticleEffectEntries { get; }

        /// <summary>
        /// Used to add particle effects when displaying a game model.
        /// 
        /// Depending on an admin setting, we may skip creating the particle effect.
        /// </summary>
        /// <param name="obstacleModel">The model that is to be displayed.</param>
        void AddDisplayParticleEffect(GameModel gameModel, CustomContentManager customContentManager);

        /// <summary>
        /// Immediately terminate all particle effects that are running for obstacles.
        /// 
        /// (e.g., display and hit)
        /// </summary>
        void TerminateAllObstacleEffects();

        /// <summary>
        /// Immediately terminate all particle effects for the finish scene.
        /// </summary>
        void TerminateAllFinishEffects();

        /// <summary>
        /// Used to add particle effects when displaying the finish screen.
        /// </summary>
        /// <param name="particleEffectDescs"></param>
        void AddFinishParticleEffect(IList<ParticleEffectDesc> particleEffectDescs, CustomContentManager customContentManager);

        /// <summary>
        /// Called from our b2ContactListener whenever we hit a game object that causes
        /// a particle effect to be created.
        /// 
        /// Depending on an admin setting, we may skip creating the particle effect.
        /// </summary>
        /// <param name="obstacleModel">The model that was hit.</param>
        void AddHitParticleEffect(GameModel gameModel, Contact theContact, CustomContentManager customContentManager);
    }
}