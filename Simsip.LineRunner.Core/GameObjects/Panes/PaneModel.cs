using Cocos2D;
using Engine.Assets;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using Engine.Input;


namespace Simsip.LineRunner.GameObjects.Panes
{
    public class PaneModel : GameModel
    {
        private PaneModelArgs _paneModelArgs;
        private IPageCache _pageCache;
        private IInputManager _inputManager;

        public PaneModel(PaneModelArgs paneModelArgs)
        {
            this.Initialize();

            this._paneModelArgs = paneModelArgs;

            this.InitializeViaArgs();
        }

        #region Box2dSprite Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            this.GameObjType = GameObjectType.Pane;

            // Get additional services we need
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect = null, EffectType type = EffectType.None)
        {
            XNAUtils.DefaultDrawState();

            base.Draw(view, projection, effect, type);
        }

        #endregion

        #region Api

        /// <summary>
        /// Allows drawing via Cocos2D view, projection and draw state.
        /// </summary>
        public void DrawViaCocos2DCamera()
        {
            TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            TheGame.SharedGame.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            base.Draw(CCDrawManager.ViewMatrix, CCDrawManager.ProjectionMatrix);
        }

        /// <summary>
        /// Allows drawing via Cocos2D view, projection and draw state.
        /// </summary>
        public void DrawViaStationaryCamera(Effect effect = null, EffectType type=EffectType.None)
        {
            if (effect == null)
            {
                TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                TheGame.SharedGame.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }

            base.Draw(this._inputManager.TheStationaryCamera.ViewMatrix, this._inputManager.TheStationaryCamera.ProjectionMatrix, effect, type);
        }


        #endregion

        #region Helper methods

        private void InitializeViaArgs()
        {
            // Determine and load appropriate xna model for line along with it's matching
            // meta-data info from database
            var modelRepository = new ModelRepository();
            switch (this._paneModelArgs.ThePaneType)
            {
                case PaneType.Simple:
                    {
                        TheModelEntity = modelRepository.GetModel("Pane1");
                        var modelNameToLoad = string.IsNullOrEmpty(TheModelEntity.ModelAlias) ? TheModelEntity.ModelName : TheModelEntity.ModelAlias;
                        XnaModel = _assetManager.GetModel(modelNameToLoad, ModelType.Pane);
                        break;
                    }
                default:
                    {
                        throw new Exception("Unknown pane type in LineModel.Initialize()");
                    }
            }

            // Now that we have our model, we can initialize model transforms
            _modelTransforms = new Matrix[XnaModel.Bones.Count];
            XnaModel.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            // Did we do our initial load of the original effects for this model name?
            if (!GameModel._originalEffectsDictionary.ContainsKey(TheModelEntity.ModelName))
            {
                // Ok, let's get the original effects stored away for this model name
                GameModel._originalEffectsDictionary[TheModelEntity.ModelName] =
                    XNAUtils.GetOriginalEffects(this.XnaModel);
            }

            // Safe to proceed and grab the original effects based on the model name, 
            // critical for referencing original texture, etc.
            this._originalEffects =
                GameModel._originalEffectsDictionary[TheModelEntity.ModelName];

            // Do we have any texture overrides?
            this._textureOverrides = new List<Texture2D>();
            var textureRepository = new TextureRepository();
            var textureEntities = textureRepository.GetTextures(TheModelEntity.ModelName);
            foreach (var textureEntity in textureEntities)
            {
                var texture = this._assetManager.GetModelTexture(TheModelEntity.ModelName, ModelType.Pane, textureEntity.TextureName);
                this._textureOverrides.Add(texture);
            }

            // Create our translation matrix
            var translatePosition = XNAUtils.LogicalToWorld(
                this._paneModelArgs.LogicalOrigin,
                this._pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var translateMatrix = Matrix.CreateTranslation(translatePosition);
            
            // Create our scale matrix
            var topLeftLogical = new CCPoint(
                this._paneModelArgs.LogicalOrigin.X,
                this._paneModelArgs.LogicalOrigin.Y + this._paneModelArgs.LogicalHeight);
            var bottomRightLogical = new CCPoint(
                this._paneModelArgs.LogicalOrigin.X + this._paneModelArgs.LogicalWidth,
                this._paneModelArgs.LogicalOrigin.Y);
            var topLeftWorld = XNAUtils.LogicalToWorld(
                topLeftLogical,
                this._pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var bottomRightWorld = XNAUtils.LogicalToWorld(
                bottomRightLogical,
                this._pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var widthWorld = bottomRightWorld.X - translatePosition.X;
            var heightWorld = topLeftWorld.Y - translatePosition.Y;
            var scaleWidth = widthWorld / this.TheModelEntity.ModelWidth;
            var scaleHeight = heightWorld / this.TheModelEntity.ModelHeight;
            var scaleMatrix = Matrix.CreateScale(
                scaleWidth,
                scaleHeight,
                1.0f
                );
            
            // Translate and scale to desired position/size
            this.WorldMatrix = scaleMatrix * translateMatrix;
        }

        #endregion

    }
}