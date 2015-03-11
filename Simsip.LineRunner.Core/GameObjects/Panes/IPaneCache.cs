using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Simsip.LineRunner.Effects.Deferred;


namespace Simsip.LineRunner.GameObjects.Panes
{
    public interface IPaneCache : IUpdateable, IDrawable
    {
        /// <summary>
        /// Add a pane model to be drawn during normal game
        /// drawing cycle.
        /// </summary>
        /// <param name="paneModel"></param>
        void AddPaneModel(PaneModel paneModel);

        /// <summary>
        /// Remove pane model from being drawn.
        /// </summary>
        /// <param name="paneModel"></param>
        void RemovePaneModel(PaneModel paneModel);

        /// <summary>
        /// Our current collection of pane models for drawing.
        /// </summary>
        List<PaneModel> PaneModels { get; }

        void Draw(Effect effect = null, EffectType type = EffectType.None);
    }
}