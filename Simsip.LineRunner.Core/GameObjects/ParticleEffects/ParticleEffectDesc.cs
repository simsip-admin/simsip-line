using System;
using ProjectMercury;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Utils;
using BEPUphysics.CollisionTests;
using Cocos2D;


namespace Simsip.LineRunner.GameObjects.ParticleEffects
{
    /// <summary>
    /// The function signature to use when want to be notified from particle effect trigger events
    /// so that you can set the screen point accordingly.
    /// </summary>
    public delegate CCPoint GetParticleEffectScreenPoint(ParticleEffectDesc particleEffectDesc);

    public class ParticleEffectDesc
    {
        public ParticleEffectType TheParticleEffectType;
        public int ParticleEffectIndex;
        public Action<ParticleEffectDesc, GameTime, XNAUtils.CameraType> Trigger;
        public GameModel TheGameModel;
        public Contact TheContact;
        public ParticleEffect TheParticleEffect;
        public float TotalParticleEffectTime;
        public GetParticleEffectScreenPoint GetScreenPoint;
        public string Tag;
    }
}