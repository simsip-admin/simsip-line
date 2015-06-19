using Cocos2D;
using Engine.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using BEPUphysics.CollisionTests;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.GameObjects.ParticleEffects;
using ConversionHelper;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Effects.Stock;


namespace Simsip.LineRunner.GameObjects.Obstacles
{
    public class ObstacleModel : GameModel
    {
        // Services we'll need
        private IPageCache _pageCache;

        // Allows us to control if we will allow a cached version of this model
        // to be retrieved from AssetManager
        private bool _allowCached;

        public ObstacleModel(ObstacleEntity obstacleEntity, PageObstaclesEntity pageObstaclesEntity, CustomContentManager customContentManager, bool allowCached = true)
        {
            TheObstacleEntity = obstacleEntity;
            ThePageObstaclesEntity = pageObstaclesEntity;

            this.TheCustomContentManager = customContentManager;

            this._allowCached = allowCached;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// Controls loading and unloading of XNA resources for this obstacle model.
        /// 
        /// IMPORTANT: In most cases the control of loading and unloading XNA resources
        /// for this model will be controled by the obstacle's containing line model.
        /// </summary>
        public CustomContentManager TheCustomContentManager { get; set; }


        public ObstacleAnimationType TheObstacleAnimationType { get; set; }

        /// <summary>
        /// The additional database entries that describe this obstacle.
        /// </summary>
        public ObstacleEntity TheObstacleEntity { get; private set; }

        /// <summary>
        /// The additional database entries that describe the placement of this obstacle on a page.
        /// </summary>
        public PageObstaclesEntity ThePageObstaclesEntity { get; private set; }

        /// <summary>
        /// TODO: Don't we have this via the worlds translation?
        /// The world-based offset in the X dimension for positioning this obstacle.
        /// </summary>
        public float WorldX { get; set; }

        public ObstacleType TheObstacleType { get; set; }

        public LineModel Line { get; set; }

        public bool RemoveAfterUpdate { get; set; }

        public Matrix RotationMatrix { get; set; }

        /// <summary>
        /// Physics contact point when hit.
        /// </summary>
        public Contact TheContact { get; set; }

        /// <summary>
        /// Once the obstacle model has been positioned, we can determine an appropriate
        /// bottom clipping plane for it.
        /// </summary>
        public Vector4 BottomClippingPlane { get; set; }

        /// <summary>
        /// Once the obstacle model has been positioned, we can determine an appropriate
        /// top clipping plane for it.
        /// </summary>
        public Vector4 TopClippingPlane { get; set; }

        #endregion

        #region Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            // Import additional required services.
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));

            // Determine and load appropriate xna model for line
            this.TheObstacleType = (ObstacleType)Enum.Parse(typeof(ObstacleType), ThePageObstaclesEntity.ObstacleType);
            var modelRepository = new ModelRepository();
            switch (this.TheObstacleType)
            {
                case ObstacleType.SimpleBottom:
                case ObstacleType.SimpleTop:
                    {
                        TheModelEntity = modelRepository.GetModel(TheObstacleEntity.ModelName);
                        var modelNameToLoad = string.IsNullOrEmpty(TheModelEntity.ModelAlias) ? TheModelEntity.ModelName : TheModelEntity.ModelAlias;
                        XnaModel = _assetManager.GetModel(
                            modelNameToLoad, 
                            ModelType.Obstacle,
                            this.TheCustomContentManager,
                            this._allowCached);
                        break;
                    }
                default:
                    {
                        throw new Exception("Unknown obstacle type in ObstacleModel.Initialize()");
                    }
            }

            // Now that we have our model, we can initialize model transforms
            _modelTransforms = new Matrix[XnaModel.Bones.Count];
            XnaModel.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            // Did we do our initial load of the original effects for this model name?
            // IMPORTANT: Note if we are not allowing a cached version of this model
            //            to be used (e.g, displaying on options page), then we
            //            allways pull a new set of effects.
            if (this._allowCached)
            {
                if (!this.TheCustomContentManager.OriginalEffectsDictionary.ContainsKey(TheObstacleEntity.ModelName))
                {
                    // Ok, let's get the original effects stored away for this model name
                    this.TheCustomContentManager.OriginalEffectsDictionary[TheObstacleEntity.ModelName] =
                        XNAUtils.GetOriginalEffects(this.XnaModel);
                }

                // Safe to proceed and grab the original effects based on the model name, 
                // critical for referencing original texture, etc.
                this._originalEffects =
                    this.TheCustomContentManager.OriginalEffectsDictionary[TheObstacleEntity.ModelName];
            }
            else
            {
                this._originalEffects = XNAUtils.GetOriginalEffects(this.XnaModel);
            }

            // Do we have any texture overrides?
            /* Trying something different for obstacles
            this._textureOverrides = new List<Texture2D>();
            var textureRepository = new TextureRepository();
            var textureEntities = textureRepository.GetTextures(TheModelEntity.ModelName);
            foreach(var textureEntity in textureEntities)
            {
                var texture = this._assetManager.GetModelTexture(TheModelEntity.ModelName, ModelType.Obstacle, textureEntity.TextureName);
                this._textureOverrides.Add(texture);
            }
            */

            
            this._textureOverrides = new List<Texture2D>();
            if (this.TheObstacleEntity.TextureFamily == "SimplePipe")
            {
                var randomNumberGenerator = new Random();
                var pipeTextureNumber = randomNumberGenerator.Next(4, 10);
                var pipeTextureName = "Pipe0" + pipeTextureNumber + "-texture_0";
                var texture = this._assetManager.GetModelTexture(
                    TheModelEntity.ModelName, 
                    ModelType.Obstacle, 
                    pipeTextureName,
                    this.TheCustomContentManager);
                this._textureOverrides.Add(texture);
            }
            else if (this.TheObstacleEntity.TextureFamily == "SimpleCan")
            {
                var randomNumberGenerator = new Random();
                var canTextureNumber = randomNumberGenerator.Next(1, 4);
                var canTextureName = string.Empty;
                if (canTextureNumber == 1)
                {
                    canTextureName = "Can0" + canTextureNumber + "-texture_0";
                }
                else
                {
                    canTextureName = "Can0" + canTextureNumber + "-texture";
                }
                var texture = this._assetManager.GetModelTexture(
                    TheModelEntity.ModelName, 
                    ModelType.Obstacle, 
                    canTextureName,
                    this.TheCustomContentManager);
                this._textureOverrides.Add(texture);
            }
            
            // Do we have any particle effects?
            this.DisplayParticleEffectDescs = ParticleEffectFactory.CreateObstacleDisplayParticles(this);
            this.HitParticleEffectDescs = ParticleEffectFactory.CreateObstacleHitParticles(this);

            // Default to no additional animation
            this.TheObstacleAnimationType = ObstacleAnimationType.None;
        }

        public override void Draw(Matrix view, Matrix projection, StockBasicEffect effect = null, EffectType type = EffectType.None)
        {
            // Assign appropriate clipping planes to use
            effect.BottomClippingPlane = this.BottomClippingPlane;
            effect.TopClippingPlane = this.TopClippingPlane;

            base.Draw(view, projection, effect, type);
        }

        #endregion

    }
}