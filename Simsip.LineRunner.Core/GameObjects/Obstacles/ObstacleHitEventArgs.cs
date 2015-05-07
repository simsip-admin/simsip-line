using System;


namespace Simsip.LineRunner.GameObjects.Obstacles
{
    /// <summary>
    /// The value class that will be passed to subscribers of obstacle hits.
    /// </summary>
    public class ObstacleHitEventArgs : EventArgs
    {
        private ObstacleModel _obstacleModel;

        public ObstacleHitEventArgs(ObstacleModel obstacleModel)
        {
            this._obstacleModel = obstacleModel;
        }

        /// <summary>
        /// The obstacle model that was hit.
        /// </summary>
        public ObstacleModel TheObstacleModel
        {
            get { return this._obstacleModel; }
        }
    }
}