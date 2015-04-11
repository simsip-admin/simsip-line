using Cocos2D;
using Engine.Assets;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using BEPUphysics.CollisionTests;
using Engine.Input;
using Simsip.LineRunner.GameFramework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.ParticleEffects;
using Simsip.LineRunner.Effects.Stock;


namespace Simsip.LineRunner.GameObjects.Lines
{
    public class LineModel : GameModel
    {
        // Required services
        private IInputManager _inputManager;

        // Allows us to control if we will allow a cached version of this model
        // to be retrieved from AssetManager
        private CustomContentManager _customContentManager;
        private bool _allowCached;

        public LineModel(LineEntity lineEntity, PageLinesEntity pageLinesEntity, CustomContentManager customContentManager=null, bool allowCached=true)
        {
            this.TheLineEntity = lineEntity;
            this.ThePageLinesEntity = pageLinesEntity;

            this._customContentManager = customContentManager;
            this._allowCached = allowCached;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// The addtional database entries that define this line type.
        /// </summary>
        public LineEntity TheLineEntity { get; private set; }

        /// <summary>
        /// The addtional database entries that define how this line is placed on a page.
        /// </summary>
        public PageLinesEntity ThePageLinesEntity { get; private set; }

        public float LogicalLineWidth { get; set; }

        /// <summary>
        /// Physics contact point when hit.
        /// </summary>
        public Contact TheContact { get; set; }

        /// <summary>
        /// An on-demand produced value that gives the line spacing height in screen coordinates.
        /// 
        /// This can be used, for instance, in our particle effect system which renders
        /// using screen coordinates.
        /// </summary>
        public float ScreenLineSpacing
        {
            get
            {
                var trackingCamera = this._inputManager.LineRunnerCamera;
                var pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));

                var screenPointOrigin = XNAUtils.WorldToScreen(new Vector3(
                    trackingCamera.Position.X,
                    trackingCamera.Position.Y,
                    this.WorldOrigin.Z),
                    XNAUtils.CameraType.Tracking);

                var screenPointOriginPlusSpacing = XNAUtils.WorldToScreen(new Vector3(
                    trackingCamera.Position.X,
                    trackingCamera.Position.Y + pageCache.CurrentPageModel.WorldLineSpacing,
                    this.WorldOrigin.Z),
                    XNAUtils.CameraType.Tracking);

                // We need to take the absolute value here as we are using the tracking camera as
                // the origin, hence depending on where the tracking camera is positioned, the screen
                // point may be above (negative Y value) or below (positive Y value).
                // However, we are just interested in the actual line spacing, hence we'll
                // just grab the absolute value
                var screenSpacing = Math.Abs(screenPointOriginPlusSpacing.Y - screenPointOrigin.Y);

                return screenSpacing;
            }
        }

        #endregion

        #region Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            this.GameObjType = GameObjectType.Line;

            // Get additional services we need
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));

            // Determine and load appropriate xna model for line along with it's matching
            // meta-data info from database
            var lineType = (LineType)Enum.Parse(typeof(LineType), ThePageLinesEntity.LineType);
            var modelRepository = new ModelRepository();
            switch (lineType)
            {
                case LineType.Simple:
                    {
                        TheModelEntity = modelRepository.GetModel(TheLineEntity.ModelName);
                        var modelNameToLoad = string.IsNullOrEmpty(TheModelEntity.ModelAlias) ? TheModelEntity.ModelName : TheModelEntity.ModelAlias;
                        XnaModel = _assetManager.GetModel(
                            modelNameToLoad, 
                            ModelType.Line,
                            this._customContentManager,
                            this._allowCached);
                        break;
                    }
                default:
                    {
                        throw new Exception("Unknown line type in LineModel.Initialize()");
                    }
            }

            // Now that we have our model, we can initialize model transforms
            _modelTransforms = new Matrix[XnaModel.Bones.Count];
            XnaModel.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            // Did we do our initial load of the original effects for this model name?
            // IMPORTANT: Note if we are not allowing a cached version of this model
            //            to be used (e.g, displaying on options page), then we
            //            allways pull a new set of effects.
            if (this._customContentManager == null ||
                this._allowCached)
            {
                if (!GameModel._originalEffectsDictionary.ContainsKey(TheLineEntity.ModelName))
                {
                    // Ok, let's get the original effects stored away for this model name
                    GameModel._originalEffectsDictionary[TheLineEntity.ModelName] =
                        XNAUtils.GetOriginalEffects(this.XnaModel);
                }
                
                // Safe to proceed and grab the original effects based on the model name, 
                // critical for referencing original texture, etc.
                this._originalEffects =
                    GameModel._originalEffectsDictionary[TheLineEntity.ModelName];
            }
            else
            {
                this._originalEffects = XNAUtils.GetOriginalEffects(this.XnaModel);
            }

            // Do we have any texture overrides?
            this._textureOverrides = new List<Texture2D>();
            var textureRepository = new TextureRepository();
            var textureEntities = textureRepository.GetTextures(TheModelEntity.ModelName);
            foreach (var textureEntity in textureEntities)
            {
                var texture = this._assetManager.GetModelTexture(TheModelEntity.ModelName, ModelType.Line, textureEntity.TextureName);
                this._textureOverrides.Add(texture);
            }

            // Do we have any particle effects?
            this.ParticleEffectDescs = ParticleEffectFactory.Create(this);
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
        public void DrawViaStationaryCamera()
        {
            TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            TheGame.SharedGame.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            base.Draw(this._inputManager.TheStationaryCamera.ViewMatrix, this._inputManager.TheStationaryCamera.ProjectionMatrix);
        }

        #endregion

    }
}