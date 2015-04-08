using System;
using Microsoft.Xna.Framework;
using ProjectMercury;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.Mercury.EffectLibrary;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;
using Cocos2D;
using ProjectMercury.Emitters;
using Simsip.LineRunner.GameObjects.Pages;
using System.Diagnostics;
using BEPUutilities;
using Simsip.LineRunner.GameObjects.Characters;


namespace Simsip.LineRunner.GameObjects.ParticleEffects
{

    public static class ParticleEffectFactory
    {
        public static IList<ParticleEffectDesc> Create(LineModel lineModel)
        {
            var particleEffectDescs = new List<ParticleEffectDesc>();
            particleEffectDescs.Add
            (
                 new ParticleEffectDesc()
                 {
                     TheParticleEffectType = ParticleEffectType.BeamMeUp,
                     ParticleEffectIndex = 0,
                     Trigger = ParticleEffectFactory.BaseLineModelTrigger,
                     TheGameModel = lineModel
                 }
            );

            return particleEffectDescs;
        }

        public static IList<ParticleEffectDesc> Create(ObstacleModel obstacleModel)
        {
            // Short circuit if no particle effects defined for this obstacle
            if (string.IsNullOrEmpty(obstacleModel.TheObstacleEntity.ParticleEffectType))
            {
                return null;
            }

            var particleEffectType = (ParticleEffectType)Enum.Parse(typeof(ParticleEffectType), obstacleModel.TheObstacleEntity.ParticleEffectType);
            var particleEffectDescs = new List<ParticleEffectDesc>();
            particleEffectDescs.Add
            (
                 new ParticleEffectDesc()
                {
                    TheParticleEffectType = particleEffectType,
                    ParticleEffectIndex = 0,
                    Trigger = ParticleEffectFactory.BaseObstacleModelTrigger,
                    TheGameModel = obstacleModel
                }
            );

            return particleEffectDescs;
        }

        public static ParticleEffect Create(ParticleEffectDesc particleEffectDesc)
        {
            ParticleEffect particleEffect = null;
            switch(particleEffectDesc.TheParticleEffectType)
            {
                case ParticleEffectType.BasicFireball:
                    {
                        particleEffect = new BasicFireball();
                        break;

                    }
                case ParticleEffectType.BasicSmokePlume:
                    {
                        particleEffect = new BasicSmokePlume();
                        break;

                    }
                case ParticleEffectType.BeamMeUp:
                    {
                        particleEffect = new BeamMeUp();
                        var lineModel = particleEffectDesc.TheGameModel as LineModel;
                        if (lineModel != null)
                        {
                            var characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));

                            var particles = particleEffect["Particles"] as RectEmitter;
                            particles.Width = 2f * characterCache.TheHeroModel.ScreenWidth;
                            particles.Height = lineModel.ScreenLineSpacing;

                            var fastBeams = particleEffect["Fast Beams"] as RectEmitter;
                            fastBeams.Width = 2f * characterCache.TheHeroModel.ScreenWidth;
                            fastBeams.Height = 0.4f * lineModel.ScreenLineSpacing;

                            var slowBeams = particleEffect["Slow Beams"] as RectEmitter;
                            slowBeams.Width = 2.4f * characterCache.TheHeroModel.ScreenWidth;
                            slowBeams.Height = 0.4f * lineModel.ScreenLineSpacing;
                        }

                        break;

                    }
                case ParticleEffectType.CampFire:
                    {
                        particleEffect = new CampFire();
                        break;

                    }
                case ParticleEffectType.FlowerBloom:
                    {
                        particleEffect = new FlowerBloom();
                        break;

                    }
                case ParticleEffectType.MagicTrail:
                    {
                        particleEffect = new MagicTrail();
                        break;

                    }
                case ParticleEffectType.Paparazzi:
                    {
                        particleEffect = new Paparazzi();
                        break;

                    }
                case ParticleEffectType.SimpleRain:
                    {
                        particleEffect = new SimpleRain();
                        break;

                    }
                case ParticleEffectType.StarTrail:
                    {
                        particleEffect = new StarTrail();
                        break;

                    }
                case ParticleEffectType.WaterJet:
                    {
                        particleEffect = new WaterJet();
                        var obstacleModel = particleEffectDesc.TheGameModel as ObstacleModel;
                        if (obstacleModel != null)
                        {
                            var mist1 = particleEffect["Mist1"] as ConeEmitter;
                            mist1.Direction = (float)Math.Atan2(
                                particleEffectDesc.TheContact.Normal.Y,
                                particleEffectDesc.TheContact.Normal.X);

                            var spray = particleEffect["Spray"] as ConeEmitter;
                            spray.Direction = (float)Math.Atan2(
                                particleEffectDesc.TheContact.Normal.Y,
                                particleEffectDesc.TheContact.Normal.X);

                            var mist2 = particleEffect["Mist2"] as ConeEmitter;
                            mist2.Direction = (float)Math.Atan2(
                                particleEffectDesc.TheContact.Normal.Y,
                                particleEffectDesc.TheContact.Normal.X);
                        }

                        break;
                    }
            }

            particleEffect.LoadContent(TheGame.SharedGame.Content);
            particleEffect.Initialise();

            return particleEffect;
        }

        public static void BaseLineModelTrigger(ParticleEffectDesc particleEffectDesc, 
                                                GameTime gameTime,
                                                XNAUtils.CameraType cameraType)
        {
            // This will center horizontally the effect over the hero
            var characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));
            var horizontalOffset = -characterCache.TheHeroModel.WorldWidth;

            // Use the contact normal's Y component to determine if we hit the top
            // or bottom line and create an appropriate offset to center the effect
            var pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            float verticalOffset = 0.5f * pageCache.CurrentPageModel.WorldLineSpacing;
            if (particleEffectDesc.TheContact.Normal.Y < 0)
            {
                verticalOffset = -verticalOffset;
            }
            // Convert world point for contact to screen point
            var screenPoint = XNAUtils.WorldToScreen(
                ConversionHelper.MathConverter.Convert(new BEPUutilities.Vector3(
                    particleEffectDesc.TheContact.Position.X + horizontalOffset,
                    particleEffectDesc.TheContact.Position.Y + verticalOffset,
                    particleEffectDesc.TheContact.Position.Z)),
                cameraType);

            // Update the particle effects location
            particleEffectDesc.TheParticleEffect.Trigger(screenPoint);

            // Let the particle effect know the delta time that has passed
            float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            particleEffectDesc.TheParticleEffect.Update(deltaSeconds);
        }

        public static void BaseObstacleModelTrigger(ParticleEffectDesc particleEffectDesc, 
                                                    GameTime gameTime,
                                                    XNAUtils.CameraType cameraType)
        {
            // Convert world point for contact to screen point
            var screenPoint = XNAUtils.WorldToScreen(
                ConversionHelper.MathConverter.Convert(particleEffectDesc.TheContact.Position),
                cameraType);

            // Update the particle effects location
            particleEffectDesc.TheParticleEffect.Trigger(screenPoint);

            // Let the particle effect know the delta time that has passed
            float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            particleEffectDesc.TheParticleEffect.Update(deltaSeconds);
        }

    }
}