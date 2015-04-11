using Cocos2D;
using Engine.Common.Logging;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Effects.Stock;


namespace Simsip.LineRunner.GameObjects.Panes
{
    public class PaneCache : DrawableGameComponent, IPaneCache
    {
        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public PaneCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(IPaneCache), this); 
        }

        #region Properties
        public List<PaneModel> PaneModels { get; private set; }

        #endregion

        #region DrawableGameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            this.PaneModels = new List<PaneModel>();

            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            this.Draw();
        }

        #endregion

        #region IPaneCache Implementation

        public void Draw(StockBasicEffect effect = null, EffectType type = EffectType.None)
        {
            foreach (var paneModel in this.PaneModels.ToList())
            {
                // Draw panes so that they stay in place
                paneModel.DrawViaStationaryCamera(effect, type);
            }
        }

        public void AddPaneModel(PaneModel paneModel)
        {
            // Record to our collection
            this.PaneModels.Add(paneModel);
        }

        public void RemovePaneModel(PaneModel paneModel)
        {
            this.PaneModels.Remove(paneModel);
        }

        #endregion
    }
}