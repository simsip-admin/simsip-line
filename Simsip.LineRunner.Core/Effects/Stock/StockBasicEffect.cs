using Engine.Assets;
using Engine.Chunks;
using Engine.Debugging;
using Engine.Debugging.Graphs;
using Engine.Debugging.Ingame;
using Engine.Graphics;
using Engine.Interface;
using Engine.Sky;
using Engine.Universe;
using Engine.Water;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using BEPUphysicsDrawer.Lines;
using Engine.Input;
using Simsip.LineRunner.Physics;


namespace Simsip.LineRunner.Effects.Stock
{
    public class StockBasicEffect : BasicEffect
    {
        static readonly byte[] StockBytecode = LoadEffectResource2(
            @"Content/Effects/Stock/StockBasicEffect.mgfxo"
        );

        public StockBasicEffect(GraphicsDevice device)
            : base(device, StockBytecode)
        {
        }

        internal static byte[] LoadEffectResource2(string path)
        {
            byte[] bytecode = null;

#if ANDROID
            using (Stream stream = Program.SharedProgram.Assets.Open(path))
#elif DESKTOP || IOS
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
#endif
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytecode = ms.ToArray();
                }
            }

            return bytecode;
        }

        // Couldn't get this one to work yet, copied from Monogame
        // Tried setting content to Embedded Resource
        internal static byte[] LoadEffectResource(string name)
        {
#if WINRT
            var assembly = typeof(Effect).GetTypeInfo().Assembly;
#else
            var assembly = typeof(Effect).Assembly;
#endif
            var stream = assembly.GetManifestResourceStream(name);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}