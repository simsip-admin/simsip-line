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
            ConvexHullTable["Can01"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(2.06164f, 4.62427f, 0f),  // 1 Lid - Right Top
                    new BEPUutilities.Vector3(2.06164f, 4.42666f, 0f),  // 2 Lid - Right Bottom
                    new BEPUutilities.Vector3(2.243f,   4.0623f,  0f),  // 3 Can - Right Top
                    new BEPUutilities.Vector3(2.243f,   0.23932f, 0f),  // 4 Can - Right Bottom
                    new BEPUutilities.Vector3(1.92778f, 0.00314f, 0f),  // 5 Base - Right Bottom
                    new BEPUutilities.Vector3(0.26181f, 0.00314f, 0f),  // 6 Base - Left Bottom
                    new BEPUutilities.Vector3(-0.013f,  0.23932f, 0f),  // 7 Can - Left Bottom
                    new BEPUutilities.Vector3(-0.013f,  4.0623f,  0f),  // 8 Can - Left Top
                    new BEPUutilities.Vector3(0.16836f, 4.42666f, 0f),  // 9 Lid - Left Bottom
                    new BEPUutilities.Vector3(0.16836f, 4.62427f, 0f),  // 10 Lid - Left Top

                    new BEPUutilities.Vector3(2.06164f, 4.62427f, -2.255f),  // 1 Lid - Right Top
                    new BEPUutilities.Vector3(2.06164f, 4.42666f, -2.255f),  // 2 Lid - Right Bottom
                    new BEPUutilities.Vector3(2.243f,   4.0623f,  -2.255f),  // 3 Can - Right Top
                    new BEPUutilities.Vector3(2.243f,   0.23932f, -2.255f),  // 4 Can - Right Bottom 
                    new BEPUutilities.Vector3(1.92778f, 0.00314f, -2.255f),  // 5 Base - Right Bottom
                    new BEPUutilities.Vector3(0.26181f, 0.00314f, -2.255f),  // 6 Base - Left Bottom
                    new BEPUutilities.Vector3(-0.013f,  0.23932f, -2.255f),  // 7 Can - Left Bottom
                    new BEPUutilities.Vector3(-0.013f,  4.0623f,  -2.255f),  // 8 Can - Left Top
                    new BEPUutilities.Vector3(0.16836f, 4.42666f, -2.255f),  // 9 Lid - Left Bottom
                    new BEPUutilities.Vector3(0.16836f, 4.62427f, -2.255f),  // 10 Lid - Left Top
                };

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

            ConvexHullTable["Pipe01"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(3.04302f, 10.02957f, 0f),  // 1 Lid - Right Top
                    new BEPUutilities.Vector3(2.99137f, 9.72998f,  0f),  // 2 Lid - Right Bottom
                    new BEPUutilities.Vector3(2.70091f, 9.72998f,  0f),  // 3 Pipe - Right Top
                    new BEPUutilities.Vector3(2.70484f, 0.25958f,  0f),  // 4 Pipe - Right Bottom
                    new BEPUutilities.Vector3(3.04724f, 0.26268f,  0f),  // 5 Base - Right Top
                    new BEPUutilities.Vector3(3.04724f, 0.02958f,  0f),  // 6 Base - Right Bottom
                    new BEPUutilities.Vector3(0.05146f, 0.02957f,  0f),  // 7 Base - Right Bottom
                    new BEPUutilities.Vector3(0.05146f, 0.26268f,  0f),  // 8 Base - Right Top
                    new BEPUutilities.Vector3(0.32129f, 0.32916f,  0f),  // 9 Pipe - Right Bottom
                    new BEPUutilities.Vector3(0.28255f, 9.72998f,  0f),  // 10 Pipe - Right Top
                    new BEPUutilities.Vector3(0.04724f, 9.79647f,  0f),  // 11 Lid - Right Bottom
                    new BEPUutilities.Vector3(0.04724f, 10.02957f, 0f),  // 12 Lid - Right Top

                    new BEPUutilities.Vector3(3.04302f, 10.02957f, -3.0f),  // 1 Lid - Right Top
                    new BEPUutilities.Vector3(2.99137f, 9.72998f,  -3.0f),  // 2 Lid - Right Bottom
                    new BEPUutilities.Vector3(2.70091f, 9.72998f,  -3.0f),  // 3 Pipe - Right Top
                    new BEPUutilities.Vector3(2.70484f, 0.25958f,  -3.0f),  // 4 Pipe - Right Bottom
                    new BEPUutilities.Vector3(3.04724f, 0.26268f,  -3.0f),  // 5 Base - Right Top
                    new BEPUutilities.Vector3(3.04724f, 0.02958f,  -3.0f),  // 6 Base - Right Bottom
                    new BEPUutilities.Vector3(0.05146f, 0.02957f,  -3.0f),  // 7 Base - Left Bottom
                    new BEPUutilities.Vector3(0.05146f, 0.26268f,  -3.0f),  // 8 Base - Left Top
                    new BEPUutilities.Vector3(0.32129f, 0.32916f,  -3.0f),  // 9 Pipe - Left Bottom
                    new BEPUutilities.Vector3(0.28255f, 9.72998f,  -3.0f),  // 10 Pipe - Left Top
                    new BEPUutilities.Vector3(0.04724f, 9.79647f,  -3.0f),  // 11 Lid - Left Bottom
                    new BEPUutilities.Vector3(0.04724f, 10.02957f, -3.0f),  // 12 Lid - Left Top
                };

            ConvexHullTable["Pipe02"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(2.21461f, 9.99992f, 0f),  // 1 Right Top
                    new BEPUutilities.Vector3(2.21461f, 0.00008f, 0f),  // 2 Right Bottom
                    new BEPUutilities.Vector3(0.00584f, 0.00008f, 0f),  // 3 Left Bottom
                    new BEPUutilities.Vector3(0.00584f, 9.99992f, 0f),  // 4 Left Top

                    new BEPUutilities.Vector3(2.21461f, 9.99992f, -2.25f),  // 1 Right Top
                    new BEPUutilities.Vector3(2.21461f, 0.00008f, -2.25f),  // 2 Right Bottom
                    new BEPUutilities.Vector3(0.00584f, 0.00008f, -2.25f),  // 3 Left Bottom
                    new BEPUutilities.Vector3(0.00584f, 9.99992f, -2.25f),  // 4 Left Top
                };

            ConvexHullTable["Pipe03"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(0.00158f, 4.97044f, 0f),  // 1 Lid - Right Top
                    new BEPUutilities.Vector3(0.18088f, 4.96117f, 0f),  // 2 Lid - Right Bottom
                    new BEPUutilities.Vector3(0.23962f, 4.94139f, 0f),  // 3 Pipe - Right Top
                    new BEPUutilities.Vector3(1.71178f, 4.74564f, 0f),  // 4 Pipe - Right Upper Quarter
                    new BEPUutilities.Vector3(3.81244f, 3.38972f, 0f),  // 5 Pipe - Right Middle
                    new BEPUutilities.Vector3(4.59556f, 2.13051f, 0f),  // 6 Pipe - Right Lower Quarter
                    new BEPUutilities.Vector3(4.944f,   0.23782f, 0f),  // 7 Pipe - Right Bottom
                    new BEPUutilities.Vector3(5.00257f, 0.17959f, 0f),  // 8 Base - Right Top
                    new BEPUutilities.Vector3(4.97232f, 0.00031f, 0f),  // 9 Base - Right Bottom
                    new BEPUutilities.Vector3(3.41762f, 0.00031f, 0f),  // 10 Base - Left Bottom
                    new BEPUutilities.Vector3(3.38737f, 0.17959f, 0f),  // 11 Base - Left Top
                    new BEPUutilities.Vector3(3.44788f, 0.23935f, 0f),  // 12 Pipe - Left Bottom
                    new BEPUutilities.Vector3(3.24698f, 1.48285f, 0f),  // 13 Pipe - Left Lower Quarter
                    new BEPUutilities.Vector3(2.46844f, 2.63337f, 0f),  // 14 Pipe - Left Middle
                    new BEPUutilities.Vector3(1.63651f, 3.18225f, 0f),  // 15 Pipe - Left Upper Quarter
                    new BEPUutilities.Vector3(0.24283f, 3.44815f, 0f),  // 16 Pipe - Left Top
                    new BEPUutilities.Vector3(0.18088f, 3.38568f, 0f),  // 17 Lid - Left Bottom
                    new BEPUutilities.Vector3(0.00158f, 3.41593f, 0f),  // 18 Lid - Left Top

                    new BEPUutilities.Vector3(0.00158f, 4.97044f, -1.615f),  // 1 Lid - Right Top
                    new BEPUutilities.Vector3(0.18088f, 4.96117f, -1.615f),  // 2 Lid - Right Bottom
                    new BEPUutilities.Vector3(0.23962f, 4.94139f, -1.615f),  // 3 Pipe - Right Top
                    new BEPUutilities.Vector3(1.71178f, 4.74564f, -1.615f),  // 4 Pipe - Right Upper Quarter
                    new BEPUutilities.Vector3(3.81244f, 3.38972f, -1.615f),  // 5 Pipe - Right Middle
                    new BEPUutilities.Vector3(4.59556f, 2.13051f, -1.615f),  // 6 Pipe - Right Lower Quarter
                    new BEPUutilities.Vector3(4.944f,   0.23782f, -1.615f),  // 7 Pipe - Right Bottom
                    new BEPUutilities.Vector3(5.00257f, 0.17959f, -1.615f),  // 8 Base - Right Top
                    new BEPUutilities.Vector3(4.97232f, 0.00031f, -1.615f),  // 9 Base - Right Bottom
                    new BEPUutilities.Vector3(3.41762f, 0.00031f, -1.615f),  // 10 Base - Left Bottom
                    new BEPUutilities.Vector3(3.38737f, 0.17959f, -1.615f),  // 11 Base - Left Top
                    new BEPUutilities.Vector3(3.44788f, 0.23935f, -1.615f),  // 12 Pipe - Left Bottom
                    new BEPUutilities.Vector3(3.24698f, 1.48285f, -1.615f),  // 13 Pipe - Left Lower Quarter
                    new BEPUutilities.Vector3(2.46844f, 2.63337f, -1.615f),  // 14 Pipe - Left Middle
                    new BEPUutilities.Vector3(1.63651f, 3.18225f, -1.615f),  // 15 Pipe - Left Upper Quarter
                    new BEPUutilities.Vector3(0.24283f, 3.44815f, -1.615f),  // 16 Pipe - Left Top
                    new BEPUutilities.Vector3(0.18088f, 3.38568f, -1.615f),  // 17 Lid - Left Bottom
                    new BEPUutilities.Vector3(0.00158f, 3.41593f, -1.615f),  // 18 Lid - Left Top
                };

            ConvexHullTable["Pipe04"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(1.82298f, 4.47096f, 0f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(4.36137f, 4.47938f, 0f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(4.36137f, 2.66418f, 0f),  // 3 Middle - Right Start
                    new BEPUutilities.Vector3(1.87878f, 2.65558f, 0f),  // 4 Middle - Right End
                    new BEPUutilities.Vector3(1.87878f, 0.10809f, 0f),  // 5 Base - Right Top
                    new BEPUutilities.Vector3(1.72099f, 0.0331f,  0f),  // 6 Base - Right Bottom
                    new BEPUutilities.Vector3(0.18911f, 0.0331f,  0f),  // 7 Base - Left Bottom
                    new BEPUutilities.Vector3(0.02816f, 0.10809f, 0f),  // 8 Base - Left Top
                    new BEPUutilities.Vector3(0.02816f, 3.25449f, 0f),  // 9 Middle - Left Start
                    new BEPUutilities.Vector3(0.2066f,  3.90389f, 0f),  // 10 Middle - Curve 1
                    new BEPUutilities.Vector3(0.62737f, 4.21237f, 0f),  // 11 Middle - Curve 2
                    new BEPUutilities.Vector3(1.11964f, 4.39016f, 0f),  // 12 Middle - Curve 3

                    new BEPUutilities.Vector3(1.82298f, 4.47096f, -1.823f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(4.36137f, 4.47938f, -1.823f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(4.36137f, 2.66418f, -1.823f),  // 3 Middle - Right Start
                    new BEPUutilities.Vector3(1.87878f, 2.65558f, -1.823f),  // 4 Middle - Right End
                    new BEPUutilities.Vector3(1.87878f, 0.10809f, -1.823f),  // 5 Base - Right Top
                    new BEPUutilities.Vector3(1.72099f, 0.0331f,  -1.823f),  // 6 Base - Right Bottom
                    new BEPUutilities.Vector3(0.18911f, 0.0331f,  -1.823f),  // 7 Base - Left Bottom
                    new BEPUutilities.Vector3(0.02816f, 0.10809f, -1.823f),  // 8 Base - Left Top
                    new BEPUutilities.Vector3(0.02816f, 3.25449f, -1.823f),  // 9 Middle - Left Start
                    new BEPUutilities.Vector3(0.2066f,  3.90389f, -1.823f),  // 10 Middle - Curve 1
                    new BEPUutilities.Vector3(0.62737f, 4.21237f, -1.823f),  // 11 Middle - Curve 2
                    new BEPUutilities.Vector3(1.11964f, 4.39016f, -1.823f),  // 12 Middle - Curve 3
                };

            ConvexHullTable["Pipe05"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(2.61493f, 4.48294f, 0f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(4.82638f, 3.58589f, 0f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(4.09865f, 1.85462f, 0f),  // 3 Middle - Right Start
                    new BEPUutilities.Vector3(1.94411f, 2.73486f, 0f),  // 4 Middle - Right End
                    new BEPUutilities.Vector3(1.92482f, 0.06204f, 0f),  // 5 Base - Right
                    new BEPUutilities.Vector3(0.04889f, 0.06204f, 0f),  // 6 Base - Left
                    new BEPUutilities.Vector3(0.0292f, 3.35444f,  0f),  // 7 Middle - Start
                    new BEPUutilities.Vector3(0.21385f, 4.02623f, 0f),  // 8 Middle - Curve 1
                    new BEPUutilities.Vector3(0.61788f, 4.34486f, 0f),  // 9 Middle - Curve 2
                    new BEPUutilities.Vector3(1.37092f, 4.5813f,  0f),  // 10 Middle - Curve 3
                    new BEPUutilities.Vector3(1.9092f, 4.60128f,  0f),  // 11 Middle - Curve 4

                    new BEPUutilities.Vector3(2.61493f, 4.48294f, -1.886f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(4.82638f, 3.58589f, -1.886f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(4.09865f, 1.85462f, -1.886f),  // 3 Middle - Right Start
                    new BEPUutilities.Vector3(1.94411f, 2.73486f, -1.886f),  // 4 Middle - Right End
                    new BEPUutilities.Vector3(1.92482f, 0.06204f, -1.886f),  // 5 Base - Right
                    new BEPUutilities.Vector3(0.04889f, 0.06204f, -1.886f),  // 6 Base - Left
                    new BEPUutilities.Vector3(0.0292f, 3.35444f,  -1.886f),  // 7 Middle - Start
                    new BEPUutilities.Vector3(0.21385f, 4.02623f, -1.886f),  // 8 Middle - Curve 1
                    new BEPUutilities.Vector3(0.61788f, 4.34486f, -1.886f),  // 9 Middle - Curve 2
                    new BEPUutilities.Vector3(1.37092f, 4.5813f,  -1.886f),  // 10 Middle - Curve 3
                    new BEPUutilities.Vector3(1.9092f, 4.60128f,  -1.886f),  // 11 Middle - Curve 4
                };

            ConvexHullTable["Pipe06"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(1.7682f, 4.75409f,  0f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(4.18599f, 5.83559f, 0f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(4.94867f, 4.11946f, 0f),  // 3 Middle - Right Start
                    new BEPUutilities.Vector3(1.94534f, 2.71948f, 0f),  // 4 Middle - Right End
                    new BEPUutilities.Vector3(1.94534f, 0.08415f, 0f),  // 5 Base - Right
                    new BEPUutilities.Vector3(0.03069f, 0.08415f, 0f),  // 6 Base - Left
                    new BEPUutilities.Vector3(0.03069f, 3.33905f, 0f),  // 7 Middle - Start
                    new BEPUutilities.Vector3(0.07659f, 3.69883f, 0f),  // 8 Middle - Curve 1
                    new BEPUutilities.Vector3(0.21531f, 4.01084f, 0f),  // 9 Middle - Curve 2
                    new BEPUutilities.Vector3(0.6064f, 4.31575f,  0f),  // 10 Middle - Curve 3
                    new BEPUutilities.Vector3(1.00218f, 4.51935f, 0f),  // 11 Middle - Curve 4

                    new BEPUutilities.Vector3(1.7682f, 4.75409f,  -1.886f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(4.18599f, 5.83559f, -1.886f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(4.94867f, 4.11946f, -1.886f),  // 3 Middle - Right Start
                    new BEPUutilities.Vector3(1.94534f, 2.71948f, -1.886f),  // 4 Middle - Right End
                    new BEPUutilities.Vector3(1.94534f, 0.08415f, -1.886f),  // 5 Base - Right
                    new BEPUutilities.Vector3(0.03069f, 0.08415f, -1.886f),  // 6 Base - Left
                    new BEPUutilities.Vector3(0.03069f, 3.33905f, -1.886f),  // 7 Middle - Start
                    new BEPUutilities.Vector3(0.07659f, 3.69883f, -1.886f),  // 8 Middle - Curve 1
                    new BEPUutilities.Vector3(0.21531f, 4.01084f, -1.886f),  // 9 Middle - Curve 2
                    new BEPUutilities.Vector3(0.6064f, 4.31575f,  -1.886f),  // 10 Middle - Curve 3
                    new BEPUutilities.Vector3(1.00218f, 4.51935f, -1.886f),  // 11 Middle - Curve 4
                };

            ConvexHullTable["Pipe07"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(0.49255f, 4.20523f, 0f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(5.2219f,  4.24438f, 0f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(5.64679f, 4.01f,    0f),  // 3 Middle - Curve 1
                    new BEPUutilities.Vector3(5.81165f, 3.41459f, 0f),  // 4 Middle - Curve 2
                    new BEPUutilities.Vector3(5.69492f, 2.81202f, 0f),  // 5 Middle - Curve 3
                    new BEPUutilities.Vector3(5.33421f, 2.52495f, 0f),  // 6 Middle - Start
                    new BEPUutilities.Vector3(3.80356f, 2.52373f, 0f),  // 7 Middle - End
                    new BEPUutilities.Vector3(3.80805f, 0.05618f, 0f),  // 8 Base - Right
                    new BEPUutilities.Vector3(2.04264f, 0.05618f, 0f),  // 9 Base - Left
                    new BEPUutilities.Vector3(1.99839f, 2.49621f, 0f),  // 10 Middle - Start
                    new BEPUutilities.Vector3(0.61039f, 2.50295f, 0f),  // 11 Middle - End
                    new BEPUutilities.Vector3(0.26947f, 2.6397f,  0f),  // 12 Middle - Curve 1
                    new BEPUutilities.Vector3(0.0161f,  3.35356f, 0f),  // 13 Middle - Curve 2
                    new BEPUutilities.Vector3(0.18056f, 3.94138f, 0f),  // 14 Middle - Curve 3

                    new BEPUutilities.Vector3(0.49255f, 4.20523f, -3.78f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(5.2219f,  4.24438f, -3.78f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(5.64679f, 4.01f,    -3.78f),  // 3 Middle - Curve 1
                    new BEPUutilities.Vector3(5.81165f, 3.41459f, -3.78f),  // 4 Middle - Curve 2
                    new BEPUutilities.Vector3(5.69492f, 2.81202f, -3.78f),  // 5 Middle - Curve 3
                    new BEPUutilities.Vector3(5.33421f, 2.52495f, -3.78f),  // 6 Middle - Start
                    new BEPUutilities.Vector3(3.80356f, 2.52373f, -3.78f),  // 7 Middle - End
                    new BEPUutilities.Vector3(3.80805f, 0.05618f, -3.78f),  // 8 Base - Right
                    new BEPUutilities.Vector3(2.04264f, 0.05618f, -3.78f),  // 9 Base - Left
                    new BEPUutilities.Vector3(1.99839f, 2.49621f, -3.78f),  // 10 Middle - Start
                    new BEPUutilities.Vector3(0.61039f, 2.50295f, -3.78f),  // 11 Middle - End
                    new BEPUutilities.Vector3(0.26947f, 2.6397f,  -3.78f),  // 12 Middle - Curve 1
                    new BEPUutilities.Vector3(0.0161f,  3.35356f, -3.78f),  // 13 Middle - Curve 2
                    new BEPUutilities.Vector3(0.18056f, 3.94138f, -3.78f),  // 14 Middle - Curve 3
                };

            ConvexHullTable["Pipe08"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(2.83157f, 4.65228f, 0f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(7.61805f, 4.70968f, 0f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(7.61628f, 2.79587f, 0f),  // 3 Middle - Start
                    new BEPUutilities.Vector3(4.94674f, 2.74947f, 0f),  // 4 Middle - End
                    new BEPUutilities.Vector3(4.94674f, 0.09258f, 0f),  // 5 Base - Right
                    new BEPUutilities.Vector3(3.01482f, 0.09258f, 0f),  // 6 Based - Left
                    new BEPUutilities.Vector3(3.01482f, 2.74947f, 0f),  // 7 Middle - Start
                    new BEPUutilities.Vector3(0.77639f, 1.88983f, 0f),  // 8 Middle - End Bottom
                    new BEPUutilities.Vector3(0.03282f, 3.65328f, 0f),  // 9 Middle - End Top

                    new BEPUutilities.Vector3(2.83157f, 4.65228f, -1.909f),  // 1 Top - Right Start
                    new BEPUutilities.Vector3(7.61805f, 4.70968f, -1.909f),  // 2 Top - Right End
                    new BEPUutilities.Vector3(7.61628f, 2.79587f, -1.909f),  // 3 Middle - Start
                    new BEPUutilities.Vector3(4.94674f, 2.74947f, -1.909f),  // 4 Middle - End
                    new BEPUutilities.Vector3(4.94674f, 0.09258f, -1.909f),  // 5 Base - Right
                    new BEPUutilities.Vector3(3.01482f, 0.09258f, -1.909f),  // 6 Based - Left
                    new BEPUutilities.Vector3(3.01482f, 2.74947f, -1.909f),  // 7 Middle - Start
                    new BEPUutilities.Vector3(0.77639f, 1.88983f, -1.909f),  // 8 Middle - End Bottom
                    new BEPUutilities.Vector3(0.03282f, 3.65328f, -1.909f),  // 9 Middle - End Top
                };

            ConvexHullTable["Pipe09"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(0.10988f, 4.54223f, 0f),  // 1 Top - Start
                    new BEPUutilities.Vector3(7.09026f, 4.54249f, 0f),  // 2 Top - End
                    new BEPUutilities.Vector3(7.08855f, 2.70608f, 0f),  // 3 Middle - Start
                    new BEPUutilities.Vector3(4.52697f, 2.66129f, 0f),  // 4 Middle - End
                    new BEPUutilities.Vector3(4.52697f, 0.11185f, 0f),  // 5 Base - Right
                    new BEPUutilities.Vector3(2.67318f, 0.11185f, 0f),  // 6 Based - Left
                    new BEPUutilities.Vector3(2.67318f, 2.66129f, 0f),  // 7 Middle - Start
                    new BEPUutilities.Vector3(0.11158f, 2.70582f, 0f),  // 8 Middle - End

                    new BEPUutilities.Vector3(0.10988f, 4.54223f, -1.832f),  // 1 Top - Start
                    new BEPUutilities.Vector3(7.09026f, 4.54249f, -1.832f),  // 2 Top - End
                    new BEPUutilities.Vector3(7.08855f, 2.70608f, -1.832f),  // 3 Middle - Start
                    new BEPUutilities.Vector3(4.52697f, 2.66129f, -1.832f),  // 4 Middle - End
                    new BEPUutilities.Vector3(4.52697f, 0.11185f, -1.832f),  // 5 Base - Right
                    new BEPUutilities.Vector3(2.67318f, 0.11185f, -1.832f),  // 6 Based - Left
                    new BEPUutilities.Vector3(2.67318f, 2.66129f, -1.832f),  // 7 Middle - Start
                    new BEPUutilities.Vector3(0.11158f, 2.70582f, -1.832f),  // 8 Middle - End
                };

            ConvexHullTable["Pipe10"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(1.94708f, 7.76523f, 0f),  // 1 Lip - Start
                    new BEPUutilities.Vector3(2.01834f, 5.60179f, 0f),  // 2 Curve - 1
                    new BEPUutilities.Vector3(2.65749f, 3.78637f, 0f),  // 3 Curve - 2
                    new BEPUutilities.Vector3(3.41975f, 2.52952f, 0f),  // 4 Curve - 3
                    new BEPUutilities.Vector3(4.58437f, 1.38644f, 0f),  // 5 Base - Start
                    new BEPUutilities.Vector3(3.23979f, 0.0772f,  0f),  // 6 Base - End
                    new BEPUutilities.Vector3(2.00413f, 1.64672f, 0f),  // 7 Curve - 1
                    new BEPUutilities.Vector3(0.98327f, 3.44252f, 0f),  // 8 Curve - 2
                    new BEPUutilities.Vector3(0.28826f, 5.56512f, 0f),  // 9 Curve - 3
                    new BEPUutilities.Vector3(0.06764f, 7.78878f, 0f),  // 10 Lip - End

                    new BEPUutilities.Vector3(1.94708f, 7.76523f, -1.892f),  // 1 Lip - Start
                    new BEPUutilities.Vector3(2.01834f, 5.60179f, -1.892f),  // 2 Curve - 1
                    new BEPUutilities.Vector3(2.65749f, 3.78637f, -1.892f),  // 3 Curve - 2
                    new BEPUutilities.Vector3(3.41975f, 2.52952f, -1.892f),  // 4 Curve - 3
                    new BEPUutilities.Vector3(4.58437f, 1.38644f, -1.892f),  // 5 Base - Start
                    new BEPUutilities.Vector3(3.23979f, 0.0772f,  -1.892f),  // 6 Base - End
                    new BEPUutilities.Vector3(2.00413f, 1.64672f, -1.892f),  // 7 Curve - 1
                    new BEPUutilities.Vector3(0.98327f, 3.44252f, -1.892f),  // 8 Curve - 2
                    new BEPUutilities.Vector3(0.28826f, 5.56512f, -1.892f),  // 9 Curve - 3
                    new BEPUutilities.Vector3(0.06764f, 7.78878f, -1.892f),  // 10 Lip - End

                };

            ConvexHullTable["Stove01"] = new List<BEPUutilities.Vector3>
                {
                    new BEPUutilities.Vector3(1.26127f, 5.9608f,  0f),  // 1 Top - Start
                    new BEPUutilities.Vector3(1.27994f, 5.0174f,  0f),  // 2 Top - Middle
                    new BEPUutilities.Vector3(1.46706f, 5.0174f,  0f),  // 3 Top - End
                    new BEPUutilities.Vector3(1.44679f, 3.2777f,  0f),  // 4 Curve - Start
                    new BEPUutilities.Vector3(2.04474f, 2.67979f, 0f),  // 5 Curve - 1
                    new BEPUutilities.Vector3(2.10857f, 2.3016f,  0f),  // 6 Curve - 2
                    new BEPUutilities.Vector3(1.78539f, 1.58483f, 0f),  // 7 Curve - 3
                    new BEPUutilities.Vector3(1.44679f, 1.3255f,  0f),  // 8 Curve - End
                    new BEPUutilities.Vector3(1.39298f, 0.06956f, 0f),  // 9 Base - Right
                    new BEPUutilities.Vector3(0.73446f, 0.06956f, 0f),  // 10 Base - Left
                    new BEPUutilities.Vector3(0.69035f, 1.3255f,  0f),  // 11 Curve - End
                    new BEPUutilities.Vector3(0.09241f, 1.9234f,  0f),  // 12 Curve - 1
                    new BEPUutilities.Vector3(0.02857f, 2.3016f,  0f),  // 13 Curve - 2
                    new BEPUutilities.Vector3(0.09241f, 2.67979f, 0f),  // 14 Curve - 3
                    new BEPUutilities.Vector3(0.69035f, 3.2777f,  0f),  // 15 Curve - Start
                    new BEPUutilities.Vector3(0.66038f, 5.0174f,  0f),  // 16 Top - End
                    new BEPUutilities.Vector3(0.86616f, 5.0174f,  0f),  // 17 Top - Middle
                    new BEPUutilities.Vector3(0.86616f, 5.9608f,  0f),  // 18 Top - End

                    new BEPUutilities.Vector3(1.26127f, 5.9608f,  -2.04f),  // 1 Top - Start
                    new BEPUutilities.Vector3(1.27994f, 5.0174f,  -2.04f),  // 2 Top - Middle
                    new BEPUutilities.Vector3(1.46706f, 5.0174f,  -2.04f),  // 3 Top - End
                    new BEPUutilities.Vector3(1.44679f, 3.2777f,  -2.04f),  // 4 Curve - Start
                    new BEPUutilities.Vector3(2.04474f, 2.67979f, -2.04f),  // 5 Curve - 1
                    new BEPUutilities.Vector3(2.10857f, 2.3016f,  -2.04f),  // 6 Curve - 2
                    new BEPUutilities.Vector3(1.78539f, 1.58483f, -2.04f),  // 7 Curve - 3
                    new BEPUutilities.Vector3(1.44679f, 1.3255f,  -2.04f),  // 8 Curve - End
                    new BEPUutilities.Vector3(1.39298f, 0.06956f, -2.04f),  // 9 Base - Right
                    new BEPUutilities.Vector3(0.73446f, 0.06956f, -2.04f),  // 10 Base - Left
                    new BEPUutilities.Vector3(0.69035f, 1.3255f,  -2.04f),  // 11 Curve - End
                    new BEPUutilities.Vector3(0.09241f, 1.9234f,  -2.04f),  // 12 Curve - 1
                    new BEPUutilities.Vector3(0.02857f, 2.3016f,  -2.04f),  // 13 Curve - 2
                    new BEPUutilities.Vector3(0.09241f, 2.67979f, -2.04f),  // 14 Curve - 3
                    new BEPUutilities.Vector3(0.69035f, 3.2777f,  -2.04f),  // 15 Curve - Start
                    new BEPUutilities.Vector3(0.66038f, 5.0174f,  -2.04f),  // 16 Top - End
                    new BEPUutilities.Vector3(0.86616f, 5.0174f,  -2.04f),  // 17 Top - Middle
                    new BEPUutilities.Vector3(0.86616f, 5.9608f,  -2.04f),  // 18 Top - End
                };
        }

        #endregion
    }
}

