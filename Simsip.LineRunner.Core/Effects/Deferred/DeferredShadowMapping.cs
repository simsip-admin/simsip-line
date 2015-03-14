using Engine.Assets;
using Engine.Chunks;
using Engine.Debugging;
using Engine.Debugging.Graphs;
using Engine.Debugging.Ingame;
using Engine.Graphics;
using Engine.Interface;
using Engine.Sky;
using Engine.Universe;
using Engine.Water;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using BEPUphysicsDrawer.Lines;
using Engine.Input;
using Simsip.LineRunner.Physics;


namespace Simsip.LineRunner.Effects.Deferred
{
    public enum EffectType
    {
        None,
        Deferred1SceneEffect,
        Deferred2LightsEffect,
        Deferred3FinalEffect,
        ShadowMapEffect
    }

    public enum LightType
    {
        Ambient,
        Directed,
        Point,
        Spot
    }

    public abstract class Light
    {
        public LightType TheLightType { get; set; }
        public Vector3 Position { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }
    }

    public class AmbientLight : Light
    {
        public AmbientLight()
        {
            this.TheLightType = LightType.Ambient;
        }

        public float Value { get; set; }
    }

    public class DirectedLight : Light
    {
        public DirectedLight()
        {
            this.TheLightType = LightType.Directed;
        }

        public Vector3 Direction { get; set; }
        public Vector3 DiffuseColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float SpecularPower { get; set; }
    }
    
    public class PointLight : Light
    {
        public PointLight()
        {
            this.TheLightType = LightType.Point;
        }

        // TODO: Fall-off
    }

    public class SpotLight : Light
    {
        public SpotLight()
        {
            this.TheLightType = LightType.Spot;
        }

        public Vector3 Direction { get; set; }
        public float Strength { get; set; }
        public float ConeAngle { get; set; }
        public float ConeDecay { get; set; }
    }

    public class DeferredShadowMapping : DrawableGameComponent, IDeferredShadowMapping
    {
        private ContactDrawer ContactDrawer;
        private BoundingBoxDrawer BoundingBoxDrawer;
        private BasicEffect LineDrawer;
        private IPhysicsManager _physicsManager;

        // For debugging
        bool _captureSnapshots;

        // Required game services
        private IInputManager _inputManager;
        private IChunkCache _chunkCache;
        private INewSky _newSky;
        private ISkyDome _skyDome;
        private IWaterCache _waterCache;
        private IUserInterface _userInterface;
        private IInGameDebuggerService _inGameDebugger;
        private IStatistics _debugBar;
        private IGraphManager _graphManager;
        private IPageCache _pageCache;
        private ILineCache _lineCache;
        private IObstacleCache _obstacleCache;
        private ICharacterCache _characterCache;
        private IPaneCache _paneCache;
        
        // For ease in referencing
        private GraphicsDevice _device;

        // Effects
        private Effect _effect1Scene;
        private Effect _effect2Lights;
        private Effect _effect3Final;
        private Effect _effectShadowMap;

        // Full screen rendering support
        private VertexPositionTexture[] _fsVertices;
        private VertexDeclaration _fsVertexDeclaration;

        // Render targets
        private RenderTarget2D _colorTarget;
        private RenderTarget2D _normalTarget;
        private RenderTarget2D _depthTarget;
        private RenderTarget2D _shadingTarget;
        private RenderTarget2D _shadowTarget;

        private Texture2D _blackImage;
        private Color[] _blackImageData;

        private GameState _currentGameState;

        private Matrix _view;
        private Matrix _projection;

         public DeferredShadowMapping(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(IDeferredShadowMapping), this);

            // We need to initialize this here as other services (e.g., World) will need
            // this ready to accept lights
            this.Lights = new List<Light>();

             // TODO
            // spotLights = new SpotLight[NumberOfLights];            

        }

        #region Properties

        public AmbientLight TheAmbientLight { get; private set; }

        public IList<Light> Lights { get; private set; }

        #endregion

        #region DrawableComponent overrides

        public override void Initialize()
        {
            ContactDrawer = new ContactDrawer(TheGame.SharedGame);
            BoundingBoxDrawer = new BoundingBoxDrawer(TheGame.SharedGame);
            LineDrawer = new BasicEffect(TheGame.SharedGame.GraphicsDevice);

            // Start with default daylight ambient level of light, will
            // be adjusted as daytime progresses
            this.TheAmbientLight = new AmbientLight()
            {
                TheLightType = LightType.Ambient,
                Value = 0.55f
            };

            // Import required services
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
            this._skyDome = (ISkyDome)TheGame.SharedGame.Services.GetService(typeof(ISkyDome));
            this._newSky = (INewSky)TheGame.SharedGame.Services.GetService(typeof(INewSky));
            this._waterCache = (IWaterCache)TheGame.SharedGame.Services.GetService(typeof(IWaterCache));
            this._chunkCache = (IChunkCache)TheGame.SharedGame.Services.GetService(typeof(IChunkCache));
            this._userInterface = (IUserInterface)TheGame.SharedGame.Services.GetService(typeof(IUserInterface));
            this._inGameDebugger = (IInGameDebuggerService)TheGame.SharedGame.Services.GetService(typeof(IInGameDebuggerService));
            this._debugBar = (IStatistics)TheGame.SharedGame.Services.GetService(typeof(IStatistics));
            this._graphManager = (IGraphManager)TheGame.SharedGame.Services.GetService(typeof(IGraphManager));
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            this._lineCache = (ILineCache)TheGame.SharedGame.Services.GetService(typeof(ILineCache));
            this._obstacleCache = (IObstacleCache)TheGame.SharedGame.Services.GetService(typeof(IObstacleCache));
            this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));
            this._paneCache = (IPaneCache)TheGame.SharedGame.Services.GetService(typeof(IPaneCache));
            this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this._device = TheGame.SharedGame.GraphicsDevice;
            
            var assetManager = (IAssetManager)TheGame.SharedGame.Services.GetService(typeof(IAssetManager));
            this._effect1Scene = assetManager.GetEffect(Asset.Deferred1SceneEffect);
            this._effect2Lights = assetManager.GetEffect(Asset.Deferred2LightsEffect);
            this._effect3Final = assetManager.GetEffect(Asset.Deferred3FinalEffect);
            this._effectShadowMap = assetManager.GetEffect(Asset.ShadowMapEffect);

            PresentationParameters pp = _device.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            this._colorTarget = new RenderTarget2D(_device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            this._normalTarget = new RenderTarget2D(_device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            this._depthTarget = new RenderTarget2D(_device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24); // *Changed from book code*
            this._shadingTarget = new RenderTarget2D(_device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            this._shadowTarget = new RenderTarget2D(_device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24); // *Changed from book code*

#if DESKTOP
            this._colorTarget.SimsipSetPrivateData("ColorTarget");
            this._normalTarget.SimsipSetPrivateData("NormalTarget");
            this._depthTarget.SimsipSetPrivateData("DepthTarget");
            this._shadingTarget.SimsipSetPrivateData("ShadingTarget");
            this._shadowTarget.SimsipSetPrivateData("ShadowTarget");
#endif

            this._blackImage = new Texture2D(_device, width, height, false, SurfaceFormat.Color);
            this._blackImageData = new Color[width * height];
            for (int i = 0; i < this._blackImageData.Length; i++)
            {
                this._blackImageData[i] = Color.Black;
            }

            InitFullscreenVertices();
        }

        public override void Draw(GameTime gameTime)
        {
            // Get state correct before doing our drawing pass
            XNAUtils.DefaultDrawState();

            // Set appropriate view/projection matrices
            this._view = this._inputManager.CurrentCamera.ViewMatrix;
            this._projection = this._inputManager.CurrentCamera.ProjectionMatrix;

            // Render color, normal and depth into 3 render targets
#if DESKTOP
            this.GraphicsDevice.BeginEvent("RenderSceneTo3RenderTargets");
#endif
            this.RenderSceneTo3RenderTargets(gameTime);
#if DESKTOP
            this.GraphicsDevice.EndEvent();
#endif

            // Add lighting contribution of each light onto shadingMap
#if DESKTOP
            this.GraphicsDevice.BeginEvent("GenerateShadingMap");
#endif
            this.GenerateShadingMap();
#if DESKTOP
            this.GraphicsDevice.EndEvent();
#endif

            // Combine base color map and shading map
#if DESKTOP
            this.GraphicsDevice.BeginEvent("CombineColorAndShading");
#endif
            this.CombineColorAndShading();
#if DESKTOP
            this.GraphicsDevice.EndEvent();
#endif

            base.Draw(gameTime);
        }

        #endregion

        #region IDeferredShadowMapping implementation

        public void AddLight(Light light)
        {
            this.Lights.Add(light);
        }

        public void RemoveLight(Light light)
        {
            this.Lights.Remove(light);
        }

        public void SwitchState(GameState state)
        {
            this._currentGameState = state;
        }

        #endregion

        #region Helper methods

        private void InitFullscreenVertices()
        {
            this._fsVertices = new VertexPositionTexture[4];

            int i = 0;

#if DESKTOP || NETFX_CORE
            this._fsVertices[i++] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 0));
            this._fsVertices[i++] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 0));
            this._fsVertices[i++] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 1));
            this._fsVertices[i++] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 1));
#else
            this._fsVertices[i++] = new VertexPositionTexture(new Vector3(-1, 1, -1f), new Vector2(0, 0));
            this._fsVertices[i++] = new VertexPositionTexture(new Vector3(1, 1, -1f), new Vector2(1, 0));
            this._fsVertices[i++] = new VertexPositionTexture(new Vector3(-1, -1, -1f), new Vector2(0, 1));
            this._fsVertices[i++] = new VertexPositionTexture(new Vector3(1, -1, -1f), new Vector2(1, 1));
#endif
        }

        private void RenderSceneTo3RenderTargets(GameTime gameTime)
        {
            this._device.SetRenderTargets(new RenderTargetBinding[] 
                { 
                    this._colorTarget, 
                    this._normalTarget, 
                    this._depthTarget
                });

            // Clear all render targets
            this._device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            // Render the scene using custom effect that writes to all render targets simultaneously
            this._effect1Scene.CurrentTechnique = this._effect1Scene.Techniques["MultipleTargets"];
            this._effect1Scene.Parameters["xView"].SetValue(this._view);
            this._effect1Scene.Parameters["xProjection"].SetValue(this._projection);

            this._effect1Scene.Parameters["xUnitConverter"].SetValue(15000f);

            this._effect1Scene.Parameters["xIsClip"].SetValue(0f);
            this._effect1Scene.Parameters["xClippingPlane"].SetValue(Vector4.Zero);

            this.RenderScene(this._effect1Scene, EffectType.Deferred1SceneEffect);

            // this._device.SetRenderTargets(null);
        }

        private void GenerateShadingMap()
        {
            // Does this really erase?
            this._shadingTarget.SetData<Color>(this._blackImageData);

            // TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.Additive;

            foreach (var light in this.Lights)
            {
                this.RenderShadowMap(light);
                this.RenderLight(light);
            }

            // TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.Opaque;
        }

        private void RenderShadowMap(Light light)
        {
            this._device.SetRenderTarget(this._shadowTarget);

            // Note: effect parameter xWorld will be set in the resulting draw calls 
            // initiated within RenderScene
            this._effectShadowMap.CurrentTechnique = _effectShadowMap.Techniques["ShadowMap"];

            if (light.TheLightType == LightType.Directed)
            {
                this._effectShadowMap.Parameters["xView"].SetValue(light.ViewMatrix);
            }
            else
            {
                this._effectShadowMap.Parameters["xView"].SetValue(light.ViewMatrix);
            }
            this._effectShadowMap.Parameters["xProjection"].SetValue(light.ProjectionMatrix);

            this._effectShadowMap.Parameters["xUnitConverter"].SetValue(15000f);

            this._effectShadowMap.Parameters["xIsClip"].SetValue(0f);
            this._effectShadowMap.Parameters["xClippingPlane"].SetValue(Vector4.Zero);

            // IMPORTANT: Note how we specify false as second parameter here
            // to signal we want to use our own view/projection matrices and 
            // not the cameras.
            RenderScene(this._effectShadowMap, EffectType.ShadowMapEffect, false);

            // this._device.SetRenderTarget(null);
        }

        private void RenderLight(Light light)
        {
            this._device.SetRenderTarget(this._shadingTarget);

            this._effect2Lights.Parameters["xPreviousShadingContents"].SetValue((Texture2D)this._shadingTarget);
            this._effect2Lights.Parameters["xNormalMap"].SetValue((Texture2D)this._normalTarget);
            this._effect2Lights.Parameters["xDepthMap"].SetValue((Texture2D)this._depthTarget);
            this._effect2Lights.Parameters["xShadowMap"].SetValue((Texture2D)this._shadowTarget);
            
            _effect2Lights.Parameters["xLightPosition"].SetValue(light.Position);

            var viewProjInv = Matrix.Invert(this._view * this._projection);
            this._effect2Lights.Parameters["xViewProjectionInv"].SetValue(viewProjInv);
            this._effect2Lights.Parameters["xLightViewProjection"].SetValue(light.ViewMatrix * light.ProjectionMatrix);

            this._effect2Lights.Parameters["xUnitConverter"].SetValue(15000f);

            switch (light.TheLightType)
            {
                case LightType.Directed:
                    {
                        this._effect2Lights.CurrentTechnique = this._effect2Lights.Techniques["DeferredDirectedLight"];
                        
                        var directedLight = light as DirectedLight;
                        this._effect2Lights.Parameters["xLightDirection"].SetValue(directedLight.Direction);

                        break;
                    }
                case LightType.Point:
                    {
                        this._effect2Lights.CurrentTechnique = this._effect2Lights.Techniques["DeferredPointLight"];

                        var pointLight = light as PointLight;
                        // TODO: Fall-off

                        break;
                    }
                case LightType.Spot:
                    {
                        this._effect2Lights.CurrentTechnique = this._effect2Lights.Techniques["DeferredSpotLight"];

                        var spotLight = light as SpotLight;
                        _effect2Lights.Parameters["xLightStrength"].SetValue(spotLight.Strength);
                        _effect2Lights.Parameters["xConeDirection"].SetValue(spotLight.Direction);
                        _effect2Lights.Parameters["xConeAngle"].SetValue(spotLight.ConeAngle);
                        _effect2Lights.Parameters["xConeDecay"].SetValue(spotLight.ConeDecay);

                        break;
                    }
                default:
                    {
                        throw new NotSupportedException("Unsupported light type in RenderShadowMap");
                    }
            }


            foreach (EffectPass pass in this._effect2Lights.CurrentTechnique.Passes)
            {
                pass.Apply();
                this._device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, this._fsVertices, 0, 2);
            }

            // this._device.SetRenderTarget(null);
        }

        private void CombineColorAndShading()
        {
            this._device.SetRenderTarget(null);

            this._effect3Final.CurrentTechnique = _effect3Final.Techniques["CombineColorAndShading"];
            this._effect3Final.Parameters["xColorMap"].SetValue((Texture2D)this._colorTarget);
            this._effect3Final.Parameters["xShadingMap"].SetValue((Texture2D)this._shadingTarget);
            this._effect3Final.Parameters["xAmbient"].SetValue(this.TheAmbientLight.Value);

            foreach (EffectPass pass in _effect3Final.CurrentTechnique.Passes)
            {
                pass.Apply();
                this._device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, this._fsVertices, 0, 2);
            }
        }

        private void RenderScene(Effect effect, EffectType type, bool useCameraMatrices=true)
        {
            // TODO: Add back in when ready for skydome
            // this._skyDome.Draw(effect, type);
            this._newSky.Draw(effect, type);
            this._chunkCache.Draw(effect, type);
            // TODO: Add back in when ready for water
            // this._waterCache.Draw(effect, type);
            this._userInterface.Draw(effect, type);
            /* TODO: Add back in when ready for these
            this._inGameDebugger.Draw(effect, type);
            this._debugBar.Draw(effect, type);
            this._graphManager.Draw(effect, type);
            */

            this._pageCache.Draw(effect, type);
            this._lineCache.Draw(effect, type);

            effect.Parameters["xIsClip"].SetValue(1f);
            this._obstacleCache.Draw(effect, type);
            effect.Parameters["xIsClip"].SetValue(0f);
            
            this._characterCache.Draw(effect, type);

            // TODO: Why does physics bounding boxes have to be done here?
            /*
            LineDrawer.LightingEnabled = false;
            LineDrawer.VertexColorEnabled = true;
            LineDrawer.World = Matrix.Identity;
            LineDrawer.ViewDirection = this._trackingCamera.ViewDirection;
            LineDrawer.ProjectionMatrix = this._trackingCamera.ProjectionMatrix;
            ContactDrawer.Draw(LineDrawer, this._physicsManager.TheSpace);
            BoundingBoxDrawer.Draw(LineDrawer, this._physicsManager.TheSpace);
            */

            // TODO: Coming
            // this._playerComponent.Draw(_gameTime);
            // this._userInterfaceComponent.Draw(_gameTime);
            // this._inGameDebuggerComponent.Draw(_gameTime);
            // this._graphManagerComponent.Draw(_gameTime);

            // Panes need to be drawn via stationary camera to 
            // remain in place in front of player.
            // UNLESS, we are doing the shadow mapping pass.
            if (useCameraMatrices)
            {
                effect.Parameters["xView"].SetValue(this._inputManager.TheStationaryCamera.ViewMatrix);
                effect.Parameters["xProjection"].SetValue(this._inputManager.TheStationaryCamera.ProjectionMatrix);
            }
            this._paneCache.Draw(effect);
        }

        #endregion
    }
}