using System;
using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameFramework;
using System.Collections.Generic;
using BEPUphysics.CollisionTests;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Effects.Stock;


namespace Simsip.LineRunner.GameObjects.Characters
{
    public interface ICharacterCache : IUpdateable
    {
        event LoadContentAsyncFinishedEventHandler LoadContentAsyncFinished;

        IList<CharacterModel> CharacterModels { get; }

         HeroModel TheHeroModel { get; }
        
        bool TouchBegan(CCTouch touch);

        void SwitchState(GameState state);

        void HandleKill(Contact theContact, Action handleMoveToFinish);

        void Draw(StockBasicEffect effect = null, EffectType type=EffectType.None);

        bool IsPaused();

        void Pause(bool pause);

        float GetLinearVelocityX();

        void IncreaseVelocity();

        bool DecreaseVelocity();
    }

}