using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameFramework;


namespace Simsip.LineRunner.GameObjects.Sensors
{
    public interface ISensorCache : IUpdateable
    {
        /// <summary>
        /// Handle sensor category specific game state changes.
        /// </summary>
        /// <param name="state">The game state we are switching to.</param>
        void SwitchState(GameState state);

        /// <summary>
        /// Called from our b2ContactListener whenever we hit a sensor.
        /// 
        /// Currently we have:
        /// - Margin sensors - signals we have hit a line margin and need to move to next line
        /// - Goal sensors - signals we have hit an obstacle's goal and need to increment score
        /// </summary>
        /// <param name="obstacleModel">The sensor model that was hit.</param>
        void AddSensorHit(SensorModel sensorModel);

        /// <summary>
        /// Allows other game components (i.e., the ActionLayer) to be notified when
        /// a sensor is hit.
        event SensorHitEventHandler SensorHit;
    }
}