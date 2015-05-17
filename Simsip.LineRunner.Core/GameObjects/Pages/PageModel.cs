using Cocos2D;
using Engine.Assets;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;
using Engine.Input;
using Simsip.LineRunner.GameFramework;
using System;
using Simsip.LineRunner.Effects.Stock;


namespace Simsip.LineRunner.GameObjects.Pages
{
    public class PageModel : GameModel
    {
        // Required services
        private IInputManager _inputManager;

        // Allows us to control if we will allow a cached version of this model
        // to be retrieved from AssetManager
        private CustomContentManager _customContentManager;
        private bool _allowCached;

        public PageModel(PadEntity padEntity, CustomContentManager customContentManager=null, bool allowCached=true)
        {
            this.ThePadEntity = padEntity;

            this._customContentManager = customContentManager;
            this._allowCached = allowCached;

            Initialize();
        }

        #region Properties

        public CustomContentManager TheCustomContentManager 
        {
            get
            {
                return _customContentManager;
            }
        }

        /// <summary>
        /// Holds additional database entries that define this page.
        /// </summary>
        public PadEntity ThePadEntity { get; private set; }

        /// <summary>
        /// The screen coordinates defining the initial position we want
        /// our hero to start in.
        /// 
        /// This is important to reference when setting up scaling into the 3D world so that
        /// the scaled object will be sized correctly when reaching this point.
        /// </summary>

        /// </summary>
        public CCPoint LogicalStartOrigin { get; set; }

        /// <summary>
        /// The screen coordinates designating where a top line will be positioned.
        /// </summary>
        public CCPoint LogicalLineSpacingTop { get; set; }

        /// <summary>
        /// The screen coordinates designating where a bottom line will be positioned.
        /// </summary>
        public CCPoint LogicalLineSpacingBottom { get; set; }

        /// <summary>
        /// The height of a line margin in screen coordinates.
        /// </summary>
        public float LogicalLineSpacingHeight 
        { 
            get
            {
                return LogicalLineSpacingTop.Y - LogicalLineSpacingBottom.Y;
            }
        }

        /// <summary>
        /// The position in the world for this page corresponding to the LogicalStartOrigin.
        /// </summary>
        public Vector3 PageStartOrigin { get; set; }

        /// <summary>
        /// The position in the world for the Hero corresponding to the LogicalStartOrigin.
        /// </summary>
        public Vector3 HeroStartOrigin { get; set; }

        /// <summary>
        /// The line margin in world coordinates corresponding to LogicalLineMarginTop/Bottom/SpaceHeight
        /// </summary>
        public float WorldLineSpacing { get; set; }

        /// <summary>
        /// The height of the page header in world coordinates.
        /// </summary>
        public float WorldHeaderMargin { get; set; }

        /// <summary>
        /// The height of the page footer in world coordinates.
        /// </summary>
        public float WorldFooterMargin { get; set; }

        #endregion

        #region Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            this.GameObjType = GameObjectType.Page;

            // Get additional services we need
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));

            // Determine and load appropriate xna model for page along with it's matching
            // meta-data info from database
            var modelRepository = new ModelRepository();
            TheModelEntity = modelRepository.GetModel(ThePadEntity.ModelName);
            var modelNameToLoad = string.IsNullOrEmpty(TheModelEntity.ModelAlias) ? TheModelEntity.ModelName : TheModelEntity.ModelAlias;
            XnaModel = _assetManager.GetModel(
                modelNameToLoad, 
                ModelType.Pad, 
                this._customContentManager, 
                this._allowCached);

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
                if (!GameModel._originalEffectsDictionary.ContainsKey(ThePadEntity.ModelName))
                {
                    // Ok, let's get the original effects stored away for this model name
                    GameModel._originalEffectsDictionary[ThePadEntity.ModelName] =
                        XNAUtils.GetOriginalEffects(this.XnaModel);
                }

                // Safe to proceed and grab the original effects based on the model name, 
                // critical for referencing original texture, etc.
                this._originalEffects =
                    GameModel._originalEffectsDictionary[ThePadEntity.ModelName];
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
                var texture = this._assetManager.GetModelTexture(TheModelEntity.ModelName, ModelType.Pad, textureEntity.TextureName);
                this._textureOverrides.Add(texture);
            }
        }

        #endregion

        #region Api

        /// <summary>
        /// Allows drawing via Cocos2D view, projection and draw state.
        /// </summary>
        public void DrawViaCocos2D()
        {
            TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            TheGame.SharedGame.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            base.Draw(CCDrawManager.ViewMatrix, CCDrawManager.ProjectionMatrix);
        }

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