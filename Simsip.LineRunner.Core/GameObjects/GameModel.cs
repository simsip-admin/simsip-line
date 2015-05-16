using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.ParticleEffects;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Graphics;
using Engine.Input;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Effects.Stock;


namespace Simsip.LineRunner.GameObjects
{
    public class GameModel : GameObject
    {
        protected IList<BasicEffect> _originalEffects;
        protected static Dictionary<string, IList<BasicEffect>> _originalEffectsDictionary =
            new Dictionary<string, IList<BasicEffect>>();
        protected IList<Texture2D> _textureOverrides;

        protected Matrix[] _modelTransforms;

        protected ActionManager _modelActionManager;

        public GameModel()
        {
            // No-op constructer will depend on properties being set in 
            // overridden Initialize()
        }

        public GameModel(int modelID, Model model, Matrix worldMatrix)
        {
            ModelID = modelID;
            XnaModel = model;
            _modelTransforms = new Matrix[XnaModel.Bones.Count];
            WorldMatrix = worldMatrix;

            _modelActionManager = GameManager.SharedGameManager.TheActionManager;
        }

        #region Properties

        /// <summary>
        /// Allows us to remove a set of original effects for when we are
        /// refreshing.
        /// </summary>
        public static Dictionary<string, IList<BasicEffect>> OriginalEffectsDictionary
        {
            get { return _originalEffectsDictionary; }
        }

        /// <summary>
        /// An ID used to uniquely identify a model within a model category (e.g., character, hero, pad, line, obstacle, etc.)
        /// 
        /// Used by octree drawing support in cache classes (e.g., IPageCache, ILineCache, IObstacleCache, etc.)
        /// </summary>
        public int ModelID { get; set; }

        /// <summary>
        /// The imported fbx representation for this model.
        /// </summary>
        public Model XnaModel { get; protected set; }

        /// <summary>
        /// The physics representation for this model.
        /// </summary>
        public Entity PhysicsEntity { get; set; }

        /// <summary>
        /// The offset to display our model at to coincide with the physics entity.
        /// </summary>
        public BEPUutilities.Matrix PhysicsLocalTransform;

        /// <summary>
        /// The base set of database info on a model.
        /// 
        /// Currently just contains the ModelWidth, ModelHeight and ModelDepth.
        /// </summary>
        public ModelEntity TheModelEntity { get; protected set; }

        /// <summary>
        /// Ratio to convert model coordinates to world coordinates.
        /// </summary>
        public float ModelToWorldRatio { get; set; }

        private Matrix _worldMatrix;

        /// <summary>
        /// The combined location, rotation and scale in world coordinates.
        /// 
        /// IMPORTANT: Note how changes to WorldOrigin are tied to WorldMatrx and vice-versa.
        /// </summary>
        public Matrix WorldMatrix
        {
            get { return _worldMatrix; }
            set
            {
                _worldMatrix = value;
                // Position3D = new Vector3(value.M41, value.M42, value.M43);
                _worldOrigin = _worldMatrix.Translation;
            }
        }

        private Vector3 _worldOrigin;

        /// <summary>
        /// The left-bottom-front location in world coordinates.
        /// 
        /// IMPORTANT: Note how changes to WorldOrigin are tied to WorldMatrx and vice-versa.
        /// </summary>
        public Vector3 WorldOrigin
        {
            get { return _worldOrigin; }
            set
            {
                _worldOrigin = value;
                _worldMatrix.Translation = _worldOrigin;
            }
        }

        /// <summary>
        /// The width of the scaled current page at CameraDepth in our 3D world.
        /// </summary>
        public float WorldWidth { get; set; }

        /// <summary>
        /// The height of the scaled current page at CameraDepth in our 3D world.
        /// </summary>
        public float WorldHeight { get; set; }

        /// <summary>
        /// The depth of the scaled current page at CameraDepth in our 3D world.
        /// </summary>
        public float WorldDepth { get; set; }

        /// <summary>
        /// Optional particle effect description structures associated with this model to be used when obstacle is displayed.
        /// </summary>
        public IList<ParticleEffectDesc> DisplayParticleEffectDescs { get; protected set; }

        /// <summary>
        /// Optional particle effect description structures associated with this model to be used when obstacle is hit.
        /// </summary>
        public IList<ParticleEffectDesc> HitParticleEffectDescs { get; protected set; }

        /// <summary>
        /// Allows us to flag if we want model tinted.
        /// 
        /// We have this hack as we can't depend on testing BlendFactorAmount against 0
        /// as it is a float.
        /// </summary>
        public bool IsTinted { get; set; }

        /// <summary>
        /// If the model is to be tinted, this is the color that will be used for the 
        /// BlendFactor color.
        /// </summary>
        public Color BlendFactorTint { get; set; }
        
        /// <summary>
        /// If the model is to be tinted, this is the amount of the BlendFactorColor
        /// we will apply.
        /// </summary>
        public float BlendFactorAmount { get; set; }

        /// <summary>
        /// An on-demand produced value that gives the model's width in screen coordinates.
        /// 
        /// This can be used, for instance, in our particle effect system which renders
        /// using screen coordinates.
        /// </summary>
        public float ScreenWidth
        {
            get
            {
                var inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));

                var screenPointOrigin = XNAUtils.WorldToScreen(new Vector3( 
                    inputManager.LineRunnerCamera.Position.X,
                    inputManager.LineRunnerCamera.Position.Y,
                    this.WorldOrigin.Z),
                    XNAUtils.CameraType.Stationary);
                
                var screenPointOriginPlusWidth = XNAUtils.WorldToScreen(new Vector3(
                    inputManager.LineRunnerCamera.Position.X + this.WorldWidth,
                    inputManager.LineRunnerCamera.Position.Y,
                    this.WorldOrigin.Z),
                    XNAUtils.CameraType.Stationary);

                // We need to take the absolute value here as we are using the tracking camera as
                // the origin, hence depending on where the tracking camera is positioned, the screen
                // point may be above (negative Y value) or below (positive Y value).
                // However, we are just interested in the actual line spacing, hence we'll
                // just grab the absolute value
                var screenWidth = System.Math.Abs(screenPointOriginPlusWidth.X - screenPointOrigin.X);

                return screenWidth;
            }
        }

        /// <summary>
        /// An on-demand produced value that gives the model's height in screen coordinates.
        /// 
        /// This can be used, for instance, in our particle effect system which renders
        /// using screen coordinates.
        /// </summary>
        public float ScreenHeight
        {
            get
            {
                var inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));

                var screenPointOrigin = XNAUtils.WorldToScreen(new Vector3(
                    inputManager.LineRunnerCamera.Position.X,
                    inputManager.LineRunnerCamera.Position.Y,
                    this.WorldOrigin.Z),
                    XNAUtils.CameraType.Stationary);

                var screenPointOriginPlusHeight = XNAUtils.WorldToScreen(new Vector3(
                    inputManager.LineRunnerCamera.Position.X,
                    inputManager.LineRunnerCamera.Position.Y + this.WorldHeight,
                    this.WorldOrigin.Z),
                    XNAUtils.CameraType.Stationary);

                // We need to take the absolute value here as we are using the tracking camera as
                // the origin, hence depending on where the tracking camera is positioned, the screen
                // point may be above (negative Y value) or below (positive Y value).
                // However, we are just interested in the actual line spacing, hence we'll
                // just grab the absolute value
                var screenHeight = System.Math.Abs(screenPointOriginPlusHeight.Y - screenPointOrigin.Y);

                return screenHeight;
            }
        }

        #endregion

        #region Cocos2d overrides

        // In derrived classes first call this base implementation, then add in
        // derived class specific initializtion

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            _modelActionManager = GameManager.SharedGameManager.TheActionManager;
        }

        public virtual void Draw(Matrix viewMatrix, 
                                 Matrix projectionMatrix,
                                 StockBasicEffect effect = null, 
                                 EffectType type = EffectType.None)
        {
            // IMPORTANT: Assumes this has been setup in derived model class
            // modelTransforms = new Matrix[XnaModel.Bones.Count];
            // XnaModel.CopyAbsoluteBoneTransformsTo(modelTransforms);

            if (effect != null)
            {
                if (this.IsTinted)
                {
                    // Reference: http://www.catalinzima.com/2012/10/postprocessing-effects-on-wp7-part-i/
                    var TintBlendFactor = new BlendState()
                    {
                        BlendFactor = this.BlendFactorTint * this.BlendFactorAmount,

                        AlphaSourceBlend = Blend.BlendFactor,
                        ColorSourceBlend = Blend.BlendFactor,
                        
                        AlphaDestinationBlend = Blend.SourceColor,
                        ColorDestinationBlend = Blend.SourceColor
                    };

                    if (this.BlendFactorAmount >= 0)
                    {
                        TintBlendFactor.ColorBlendFunction = BlendFunction.Add;
                        TintBlendFactor.AlphaBlendFunction = BlendFunction.Add;

                    }
                    else
                    {
                        TintBlendFactor.ColorBlendFunction = BlendFunction.ReverseSubtract;
                        TintBlendFactor.AlphaBlendFunction = BlendFunction.ReverseSubtract;
                    }

                    // Add back in for debugging purposes
                    // Debug.WriteLine("Amount: " + BlendFactorAmount + " Tint: " + BlendFactorTint + " BlendFactor: " + TintBlendFactor.BlendFactor);
                    
                    TheGame.SharedGame.GraphicsDevice.BlendState = TintBlendFactor;
                }

                // TODO: Can we just set the effect directly since we 
                // have copied the textures on initialization?
                // XNAUtils.ChangeEffect(this.XnaModel, effect);

                int i = 0;
                foreach (ModelMesh mesh in this.XnaModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        switch (type)
                        {
                            case EffectType.Deferred1SceneEffect:
                                {
                                    effect.Parameters["World"].SetValue(this._modelTransforms[mesh.ParentBone.Index] * this.WorldMatrix);
                                    if (this._textureOverrides.Count > 0)
                                    {
                                        effect.Parameters["Texture"].SetValue(this._textureOverrides[i]);
                                    }
                                    else
                                    {
                                        effect.Parameters["Texture"].SetValue(this._originalEffects[i].Texture);
                                    }
                                    break;
                                }
                            case EffectType.ShadowMapEffect:
                                {
                                    effect.Parameters["World"].SetValue(this._modelTransforms[mesh.ParentBone.Index] * this.WorldMatrix);
                                    break;
                                }
                            case EffectType.StockBasicEffect:
                                {
                                    var stockBasicEffect = effect as StockBasicEffect;
                                    stockBasicEffect.World = this._modelTransforms[mesh.ParentBone.Index] * this.WorldMatrix;
                                    if (this._textureOverrides.Count > 0)
                                    {
                                        stockBasicEffect.Texture = this._textureOverrides[i];
                                    }
                                    else
                                    {
                                        stockBasicEffect.Texture = this._originalEffects[i].Texture;
                                    }
                                    break;
                                }

                        }
                        i++;

                        part.Effect = effect;
                    }
                    mesh.Draw();
                }

                if (this.IsTinted)
                {
                    XNAUtils.DefaultDrawState();
                }
            }
            else
            {
                int i = 0;
                foreach (ModelMesh mesh in this.XnaModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        var originalEffect = this._originalEffects[i];
                        originalEffect.LightingEnabled = true;
                        originalEffect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
                        originalEffect.DirectionalLight0.Enabled = true;
                        originalEffect.DirectionalLight0.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
                        originalEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, -1.0f));

                        if (this._textureOverrides.Count > 0)
                        {
                            originalEffect.Texture = this._textureOverrides[i];
                        }

                        originalEffect.World = this._modelTransforms[mesh.ParentBone.Index] * this.WorldMatrix;
                        originalEffect.View = viewMatrix;
                        originalEffect.Projection = projectionMatrix;

                        i++;

                        part.Effect = originalEffect;
                    }
                    mesh.Draw();
                }
            }
        }

        #endregion

        /*
        public virtual void Draw(IMainRenderColorPass mainRenderColorPass)
        {

        }

        public virtual void Draw(IMainRenderShadowPass mainRenderShadowPass)
        {

        }

        public virtual void Draw(IMainRenderSinglePass mainRenderShadowPass)
        {

        }
        */

        #region Actions

        public ActionManager ModelActionManager
        {
            get { return _modelActionManager; }
            set
            {
                if (value != _modelActionManager)
                {
                    ModelStopAllActions();
                    _modelActionManager = value;
                }
            }
        }

        public Action ModelRunAction(Action action)
        {
            Debug.Assert(action != null, "Argument must be non-nil");
            _modelActionManager.AddAction(action, this, false);
            return action;
        }

        public void ModelStopAllActions()
        {
            m_pActionManager.RemoveAllActionsFromTarget(this);
        }

        public void ModelStopAction(Action action)
        {
            _modelActionManager.RemoveAction(action);
        }

        public void ModelStopActionByTag(int tag)
        {
            // Debug.Assert(tag != (int)CCNodeTag.Invalid, "Invalid tag");
            m_pActionManager.RemoveActionByTag(tag, this);
        }

        public Action ModelGetActionByTag(int tag)
        {
            // Debug.Assert(tag != (int)CCNodeTag.Invalid, "Invalid tag");
            return _modelActionManager.GetActionByTag(tag, this);
        }

        public int ModelNumberOfRunningActions()
        {
            return _modelActionManager.NumberOfRunningActionsInTarget(this);
        }

        #endregion

    }
}