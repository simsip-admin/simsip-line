using Cocos2D;
using Engine.Assets;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Simsip.LineRunner.Effects.Deferred;
using ConversionHelper;
using Simsip.LineRunner.Effects.Stock;
using System.Linq;


namespace Simsip.LineRunner.GameObjects.Obstacles
{
    public static class ObstacleConvexHulls
    {
        static ObstacleConvexHulls()
        {
            InitializeConvexHulls();
        }

        #region Properties

        public static IDictionary<string, IList<BEPUutilities.Vector3>> ConvexHullTable { get; private set; }

        #endregion

        #region Helper methods

        private static void InitializeConvexHulls()
        {
            ConvexHullTable = new Dictionary<string, IList<BEPUutilities.Vector3>>();
            int pointCounter = 0;
            var pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            var scale = pageCache.CurrentPageModel.ModelToWorldRatio;

            // IMPORTANT: Key must match model name
            ConvexHullTable["Gramophone01"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(1.74760f, 4.94566f, 0f),  // 1 Hood - Top
                    new BEPUutilities.Vector3(2.93736f, 4.41286f, 0f),  // 2 Hood - Right Quarter
                    new BEPUutilities.Vector3(3.29145f, 3.16422f, 0f),  // 3 Hood - Right Side
                    new BEPUutilities.Vector3(4.36906f, 1.42903f, 0f),  // 4 Base - Right Top
                    new BEPUutilities.Vector3(4.57394f, 0.02717f, 0f),  // 5 Base - Right Bottom
                    new BEPUutilities.Vector3(1.2325f,  0.03556f, 0f),  // 6 Base - Left Bottom
                    new BEPUutilities.Vector3(1.43962f, 1.38248f, 0f),  // 7 Base - Left Top
                    new BEPUutilities.Vector3(0.02962f, 3.16835f, 0f),  // 8 Hood - Left Side
                    new BEPUutilities.Vector3(0.49636f, 4.41014f, 0f),  // 9 Hood - Left Quarter

                    new BEPUutilities.Vector3(1.74760f, 4.94566f, -4.327f),  // 1 Hood - Top
                    new BEPUutilities.Vector3(2.93736f, 4.41286f, -4.327f),  // 2 Hood - Right Quarter
                    new BEPUutilities.Vector3(3.29145f, 3.16422f, -4.327f),  // 3 Hood - Right Side
                    new BEPUutilities.Vector3(4.36906f, 1.42903f, -4.327f),  // 4 Base - Right Top
                    new BEPUutilities.Vector3(4.57394f, 0.02717f, -4.327f),  // 5 Base - Right Bottom
                    new BEPUutilities.Vector3(1.2325f,  0.03556f, -4.327f),  // 6 Base - Left Bottom
                    new BEPUutilities.Vector3(1.43962f, 1.38248f, -4.327f),  // 7 Base - Left Top
                    new BEPUutilities.Vector3(0.02962f, 3.16835f, -4.327f),  // 8 Hood - Left Side
                    new BEPUutilities.Vector3(0.49636f, 4.41014f, -4.327f),  // 9 Hood - Left Quarter
                };
            pointCounter = 0;
            foreach (var point in ConvexHullTable["Gramophone01"].ToList())
            {
                ConvexHullTable["Gramophone01"][pointCounter] *= scale;
                pointCounter++;
            }

            ConvexHullTable["Hero"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(0.96531f, 2.01085f, 0),  // 1 Top
                    new BEPUutilities.Vector3(1.21891f, 1.71405f, 0),  // 2 Forehead
                    new BEPUutilities.Vector3(1.05135f, 1.31298f, 0),  // 3 Brow
                    new BEPUutilities.Vector3(1.46406f, 0.97206f, 0),  // 4 Nose
                    new BEPUutilities.Vector3(1.4876f,  0.43779f, 0),  // 5 Lip
                    new BEPUutilities.Vector3(1.18052f, 0.14824f, 0),  // 6 Chin
                    new BEPUutilities.Vector3(0.43166f, 0.01126f, 0),  // 7 Bottom
                    new BEPUutilities.Vector3(0.01033f, 0.75503f, 0),  // 8 Back
                    new BEPUutilities.Vector3(0.96531f, 2.01085f, -0.27824f),  // 1 Top
                    new BEPUutilities.Vector3(1.21891f, 1.71405f, -0.27824f),  // 2 Forehead
                    new BEPUutilities.Vector3(1.05135f, 1.31298f, -0.27824f),  // 3 Brow
                    new BEPUutilities.Vector3(1.46406f, 0.97206f, -0.27824f),  // 4 Nose
                    new BEPUutilities.Vector3(1.4876f,  0.43779f, -0.27824f),  // 5 Lip
                    new BEPUutilities.Vector3(1.18052f, 0.14824f, -0.27824f),  // 6 Chin
                    new BEPUutilities.Vector3(0.43166f, 0.01126f, -0.27824f),  // 7 Bottom
                    new BEPUutilities.Vector3(0.01033f, 0.75503f, -0.27824f),  // 8 Back
                };
        }

        #endregion
    }
}

