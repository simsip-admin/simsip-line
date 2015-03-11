using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Simsip.LineRunner.Effects.Deferred;


namespace Engine.Water
{
    public interface IWaterCache : IUpdateable, IDrawable
    {
        void Draw(Effect effect = null, EffectType type = EffectType.None);
    }
}