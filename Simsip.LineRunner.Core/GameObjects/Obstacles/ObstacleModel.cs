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


namespace Simsip.LineRunner.GameObjects.Obstacles
{
    public class ObstacleModel : GameModel
    {
        // Services we'll need
        private IPageCache _pageCache;

        public ObstacleModel(ObstacleEntity obstacleEntity, PageObstaclesEntity pageObstaclesEntity)
        {
            TheObstacleEntity = obstacleEntity;
            ThePageObstaclesEntity = pageObstaclesEntity;

            Initialize();
        }

        #region Properties

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

        /// <summary>
        /// The LogicalHeight converted into world coordinates.
        /// </summary>
        public float WorldHeightTruncated { get; set; }

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
        /// clipping plane for it.
        /// </summary>
        public Vector4 ClippingPlane { get; set; }

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
                        XnaModel = _assetManager.GetModel(modelNameToLoad, ModelType.Obstacle);
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
            if (!GameModel._originalEffectsDictionary.ContainsKey(TheObstacleEntity.ModelName))
            {
                // Ok, let's get the original effects stored away for this model name
                GameModel._originalEffectsDictionary[TheObstacleEntity.ModelName] =
                    XNAUtils.GetOriginalEffects(this.XnaModel);
            }

            // Safe to proceed and grab the original effects based on the model name, 
            // critical for referencing original texture, etc.
            this._originalEffects =
                GameModel._originalEffectsDictionary[TheObstacleEntity.ModelName];

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
                var texture = this._assetManager.GetModelTexture(TheModelEntity.ModelName, ModelType.Obstacle, pipeTextureName);
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
                var texture = this._assetManager.GetModelTexture(TheModelEntity.ModelName, ModelType.Obstacle, canTextureName);
                this._textureOverrides.Add(texture);
            }

            // Do we have any particle effects?
            this.ParticleEffectDescs = ParticleEffectFactory.Create(this);
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect = null, EffectType type = EffectType.None)
        {
            // TODO: Do we have to do this every draw?
            // Adjust for physics centering at center of mesh
            if (this.PhysicsEntity != null) // &&
                // this.PhysicsLocalTransform != null) TODO: This is a struct, do we need some type of check?
            {
                var scale = this._pageCache.CurrentPageModel.ModelToWorldRatio;
                if (this.ThePageObstaclesEntity.LogicalScaleScaledTo100 != 0)
                {
                    scale *= this.ThePageObstaclesEntity.LogicalScaleScaledTo100 / 100;
                }

                var scaleMatrix = Matrix.CreateScale(scale);
                this.WorldMatrix = scaleMatrix * this.RotationMatrix * MathConverter.Convert(this.PhysicsLocalTransform * this.PhysicsEntity.WorldTransform);
            }

            // Assign an appropriate clipping plane to use
            effect.Parameters["ClippingPlane"].SetValue(this.ClippingPlane);

            base.Draw(view, projection, effect, type);
        }

        #endregion

    }
}