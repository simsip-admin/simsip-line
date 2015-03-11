using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Simsip.LineRunner.GameFramework;


namespace Simsip.LineRunner.Effects.Deferred
{
    public interface IDeferredShadowMapping : IUpdateable, IDrawable
    {
        AmbientLight TheAmbientLight { get; }

        IList<Light> Lights { get; }

        void AddLight(Light light);

        void RemoveLight(Light light);

        /// <summary>
        /// Handle deferred shadow mapping category specific game state changes.
        /// </summary>
        /// <param name="state">The game state we are switching to.</param>
        void SwitchState(GameState state);
    }
}