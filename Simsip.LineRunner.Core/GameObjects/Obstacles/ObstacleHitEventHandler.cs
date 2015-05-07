

namespace Simsip.LineRunner.GameObjects.Obstacles
{
    /// <summary>
    /// The function signature to use when you are subscribing to be notified of obstacle hits.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ObstacleHitEventHandler(object sender, ObstacleHitEventArgs e);
}