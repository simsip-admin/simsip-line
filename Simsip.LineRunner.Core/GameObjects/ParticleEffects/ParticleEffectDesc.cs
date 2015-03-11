using System;
using ProjectMercury;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Utils;
using BEPUphysics.CollisionTests;


namespace Simsip.LineRunner.GameObjects.ParticleEffects
{
    public class ParticleEffectDesc
    {
        public ParticleEffectType TheParticleEffectType;
        public int ParticleEffectIndex;
        public Action<ParticleEffectDesc, GameTime, XNAUtils.CameraType> Trigger;
        public GameModel TheGameModel;
        public Contact TheContact;
        public ParticleEffect TheParticleEffect;
        public float TotalParticleEffectTime;
    }
}