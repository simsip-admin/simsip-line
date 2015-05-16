using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.GameFramework;
using System.Collections.Generic;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Effects.Stock;


namespace Simsip.LineRunner.GameObjects.Obstacles
{
    public interface IObstacleCache : IUpdateable
    {
        event LoadContentAsyncFinishedEventHandler LoadContentAsyncFinished;

        /// <summary>
        /// Handle obstacle category specific game state changes.
        /// </summary>
        /// <param name="state">The game state we are switching to.</param>
        void SwitchState(GameState state);

        /// <summary>
        /// Our current collection of obstacle models for the current page.
        /// </summary>
        IList<ObstacleModel> ObstacleModels { get; }

        /// <summary>
        /// Called from our b2ContactListener whenever we hit an obstacle.
        /// 
        /// Depending on debug flags, we may signal that hero is to be killed.
        /// </summary>
        /// <param name="obstacleModel">The obstacle model that was hit.</param>
        void AddObstacleHit(ObstacleModel obstacleModel);

        /// <summary>
        /// Allows other game components (i.e., the ActionLayer) to be notified when
        /// an obstacle is hit.
        /// </summary>
        event ObstacleHitEventHandler ObstacleHit;

        void Draw(StockBasicEffect effect = null, EffectType type = EffectType.None);
    }
}