

namespace Simsip.LineRunner.GameObjects.Obstacles
{
    public enum ObstacleType
    {
        /// <summary>
        /// Simple obstacle hanging down from top line.
        /// 
        /// Defined Length:
        /// Will use a logical width from the obstacle (e.g., pipe width)
        /// </summary>
        SimpleTop,

        /// <summary>
        /// Simple obstacle rising up from the bottom line.
        /// 
        /// Defined Length:
        /// Will use a logical width from the obstacle (e.g., pipe width)
        /// </summary>
        SimpleBottom,
    }
}