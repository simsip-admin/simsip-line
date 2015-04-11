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


namespace Simsip.LineRunner.GameObjects.Characters
{
    public class HeroModel : CharacterModel
    {
        // Aditional service we require
        private IPageCache _pageCache;

        // Defined jump directions
        public enum JumpDirection
        {
            UpperLeft,
            UpperRight
        }

        //
        // Physics control variables
        //
        // Velocity controls
        public  BEPUutilities.Vector3 DEFAULT_VELOCITY = new  BEPUutilities.Vector3(0.1f, 0f, 0f);
        public BEPUutilities.Vector3 OPPOSITE_DEFAULT_VELOCITY = new BEPUutilities.Vector3(-0.1f, 0f, 0f);
        public BEPUutilities.Vector3 STOP_VELOCITY = BEPUutilities.Vector3.Zero;
        private float VELOCITY_X_LIMIT = -1;
        //
        // Jumping controls
        private BEPUutilities.Vector3 DEFAULT_TAP_FOWARDS = new BEPUutilities.Vector3(2f, 4f, 0f);
        private BEPUutilities.Vector3 DEFAULT_TAP_BACKWARDS = new BEPUutilities.Vector3(-2f, 4f, 0f);
        private BEPUutilities.Vector3 DEFAULT_TAP_LOCATION = BEPUutilities.Vector3.Zero;
        private float VELOCITY_Y_UPPER_LIMIT = 80f;
        private float VELOCITY_Y_LOWER_LIMIT = -100;
        //
        // Rotational controls
        private float ANGULAR_VELOCITY_UPPER_LIMIT = 4f;
        private float ANGULAR_VELOCITY_LOWER_LIMIT = 0f;
        private float ANGLE_UPPER_LIMIT = 30f;
        private float ANGLE_LOWER_LIMIT = -30f;

        public HeroModel(CharacterEntity characterEntity, PageCharactersEntity pageCharactersEntity)
        {
            TheCharacterEntity = characterEntity;
            ThePageCharactersEntity = pageCharactersEntity;

            this.Initialize();
        }

        #region Properties

        /// <summary>
        /// A value calculated upon loading our hero that will position the hero exactly
        /// straddling the halfway depth of the lines.
        /// </summary>
        public float DefinedDepthFromCamera { get; set; }

        public IList<BEPUutilities.Vector3> ConvexHullPoints { get; set; }

        /// <summary>
        /// The database entity that defines this character. 
        /// </summary>
        public CharacterEntity TheCharacterEntity { get; private set; }


        /// <summary>
        /// The database entity that defines how this character is placed on a page.
        /// </summary>
        public PageCharactersEntity ThePageCharactersEntity { get; private set; }

        #endregion

        #region Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            this.GameObjType = GameObjectType.Hero;

            // Import additional required services.
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));

            // Determine and load appropriate xna model for page
            var characterType = (CharacterType)Enum.Parse(typeof(CharacterType), ThePageCharactersEntity.CharacterType);
            var modelRepository = new ModelRepository();
            switch (characterType)
            {
                case CharacterType.Hero:
                    {
                        TheModelEntity = modelRepository.GetModel(TheCharacterEntity.ModelName);
                        var modelNameToLoad = string.IsNullOrEmpty(TheModelEntity.ModelAlias) ? TheModelEntity.ModelName : TheModelEntity.ModelAlias;
                        XnaModel = _assetManager.GetModel(modelNameToLoad, ModelType.Character);
                        break;
                    }
                default:
                    {
                        throw new Exception("Unknown hero type in HeroModel.Initialize()");
                    }
            }

            // Now that we have our model, we can initialize model transforms
            this._modelTransforms = new Matrix[XnaModel.Bones.Count];
            XnaModel.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            // Did we do our initial load of the original effects for this model name?
            if (!GameModel._originalEffectsDictionary.ContainsKey(TheCharacterEntity.ModelName))
            {
                // Ok, let's get the original effects stored away for this model name
                GameModel._originalEffectsDictionary[TheCharacterEntity.ModelName] =
                    XNAUtils.GetOriginalEffects(this.XnaModel);
            }

            // Safe to proceed and grab the original effects based on the model name, 
            // critical for referencing original texture, etc.
            this._originalEffects =
                GameModel._originalEffectsDictionary[TheCharacterEntity.ModelName];

            // Do we have any texture overrides?
            this._textureOverrides = new List<Texture2D>();
            var textureRepository = new TextureRepository();
            var textureEntities = textureRepository.GetTextures(TheModelEntity.ModelName);
            foreach (var textureEntity in textureEntities)
            {
                var texture = this._assetManager.GetModelTexture(TheModelEntity.ModelName, ModelType.Character, textureEntity.TextureName);
                this._textureOverrides.Add(texture);
            }

            this.InitializeConvexHull();
        }

        public override void Draw(Matrix view, Matrix projection, StockBasicEffect effect = null, EffectType type = EffectType.None)
        {
            if (effect == null)
            {
                XNAUtils.DefaultDrawState();
            }

            base.Draw(view, projection, effect, type);
        }

        #endregion

        #region Api

        /// <summary>
        /// Apply physics constraints to Hero physics model.
        /// 
        /// This is usually done once per update frame. We need to:
        /// 1. Keep hero's depth at a constant value.
        /// 2. Keep hero's linear velocity to 2 dimensions (x,y).
        /// </summary>
        public void AdjustHeroPhysics()
        {
            // Keep hero in xy plane
            var position = this.PhysicsEntity.Position;
            position.Z = this.DefinedDepthFromCamera;
            this.PhysicsEntity.Position = position;

            // Keep linear velocity in xy plane and limited
            // This is easily done by taking the min value of the max allowed value and the input value, then the max value of the min allowed value and the result.
            var linearVelocity = this.PhysicsEntity.LinearVelocity;
            linearVelocity.X = Math.Max(-1.0f, Math.Min(1.0f, linearVelocity.X));
            linearVelocity.Y = Math.Max(-2.0f, Math.Min(2.0f, linearVelocity.Y));
            linearVelocity.Z = 0;
            this.PhysicsEntity.LinearVelocity = linearVelocity;
        }

        public void Update()
        {
            // Position hero at new world coordinates
            var scaleMatrix = Matrix.CreateScale(this._pageCache.CurrentPageModel.ModelToWorldRatio);
            this.WorldMatrix = scaleMatrix * MathConverter.Convert(this.PhysicsLocalTransform * this.PhysicsEntity.WorldTransform);
            // this.WorldMatrix = scaleMatrix * MathConverter.Convert(this.PhysicsEntity.WorldTransform);
        }

        public void Jump(JumpDirection jumpDirection)
        {
            // TODO: Cache this string for faster access
            SoundUtils.PlaySoundEffect(_assetManager.GetSound(Asset.SoundHeroWinging));

            if (jumpDirection == JumpDirection.UpperRight)
            {
                this.PhysicsEntity.ApplyLinearImpulse(ref DEFAULT_TAP_FOWARDS);
            }
            else if (jumpDirection == JumpDirection.UpperLeft)
            {
                this.PhysicsEntity.ApplyLinearImpulse(ref DEFAULT_TAP_BACKWARDS);
            }

        }

        #endregion

        #region Helper methods

        private void InitializeConvexHull()
        {
            this.ConvexHullPoints = new List<BEPUutilities.Vector3>();
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(0.96531f, 2.01085f, 0));  // 1 Top
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.21891f, 1.71405f, 0));  // 2 Forehead
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.05135f, 1.31298f, 0));  // 3 Brow
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.46406f, 0.97206f, 0));  // 4 Nose
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.4876f,  0.43779f, 0));  // 5 Lip
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.18052f, 0.14824f, 0));  // 6 Chin
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(0.43166f, 0.01126f, 0));  // 7 Bottom
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(0.01033f, 0.75503f, 0));  // 8 Back
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(0.96531f, 2.01085f, -0.27824f));  // 1 Top
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.21891f, 1.71405f, -0.27824f));  // 2 Forehead
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.05135f, 1.31298f, -0.27824f));  // 3 Brow
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.46406f, 0.97206f, -0.27824f));  // 4 Nose
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.4876f,  0.43779f, -0.27824f));  // 5 Lip
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(1.18052f, 0.14824f, -0.27824f));  // 6 Chin
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(0.43166f, 0.01126f, -0.27824f));  // 7 Bottom
            this.ConvexHullPoints.Add(new BEPUutilities.Vector3(0.01033f, 0.75503f, -0.27824f));  // 8 Back
        }

        #endregion
    }
}

