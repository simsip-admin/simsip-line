using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Simsip.LineRunner.GameFramework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Effects.Stock;


namespace Simsip.LineRunner.GameObjects.Lines
{
    public interface ILineCache : IUpdateable
    {
        void LoadContentAsync(LoadContentAsyncType loadContentType);

        event LoadContentAsyncFinishedEventHandler LoadContentAsyncFinished;

        /// <summary>
        /// Handle line category specific game state changes.
        /// </summary>
        /// <param name="state">The game state we are switching to.</param>
        void SwitchState(GameState state);

        /// <summary>
        /// Our current collection of line models for the current page.
        /// </summary>
        IList<LineModel> LineModels { get; }

        /// <summary>
        /// Called from our b2ContactListener whenever we hit a line.
        /// 
        /// Depending on debug flags, we may signal that hero is to be killed.
        /// </summary>
        /// <param name="obstacleModel">The line model that was hit.</param>
        void AddLineHit(LineModel lineModel);

        /// <summary>
        /// Allows other game components (i.e., the ActionLayer) to be notified when
        /// a line is hit.
        /// </summary>
        event LineHitEventHandler LineHit;

        /// <summary>
        /// Given a 1 based lineNumber as a parameter, will return the line
        /// representing that lineNumber.
        /// 
        /// Example:
        /// Parameter lineNumber 1 gives 1st line
        ///
        /// LineModels collection reference for 5 lines:
        /// Index 0 represents => 5th line
        /// Index 1 represents => 4th line
        /// Index 2 represents => 3rd line
        /// Index 3 represents => 2nd line
        /// Index 4 represents => 1st line
        /// Index 5 represents => Header line
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        LineModel GetLineModel(int lineNumber);

        void Draw(StockBasicEffect effect = null, EffectType type = EffectType.None);
    }
}