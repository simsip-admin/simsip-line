/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using Engine.Assets;
using Engine.Common.Logging;
using Engine.Graphics;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;


namespace Engine.Universe
{
    /// <summary>
    /// Allows interaction with sky service.
    /// </summary>
    public interface ISkyDome : IUpdateable, IDrawable
    {
        /// <summary>
        /// Toggles dynamic clouds.
        /// </summary>
        void ToggleDynamicClouds();

        void Draw(Effect effect = null, EffectType type = EffectType.None);
    }

    /// <summary>
    /// Sky.
    /// </summary>
    public class SkyDome : DrawableGameComponent, ISkyDome
    {
        // settings
        private bool _dynamicCloudsEnabled;

        private Model _skyDome; // Sky dome model
        // private Texture2D _cloudMap; // Cloud map.
        private Texture2D _starMap; // Star map.

        private Texture2D _staticCloudMap; // gpu generated cloud maps.
        private Effect _perlinNoiseEffect; // noise used for generating clouds.
        private RenderTarget2D _cloudsRenderTarget; // render target for clouds.
        private VertexPositionTexture[] _fullScreenVertices; // vertices.

        protected Vector3 SunColor = Color.White.ToVector3();

        protected Vector4 OverheadSunColor = Color.DarkBlue.ToVector4();
        protected Vector4 NightColor = Color.Black.ToVector4();
        protected Vector4 HorizonColor = Color.White.ToVector4();
        protected Vector4 EveningTint = Color.Red.ToVector4();
        protected Vector4 MorningTint = Color.Gold.ToVector4();
        protected float CloudOvercast = 1.1f;

        protected float RotationClouds;
        protected float RotationStars;

        // Logging
        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility

        // Required services.
        private IInputManager _inputManager;
        private IAssetManager _assetManager;

        public SkyDome(Game game, bool enableDynamicClouds = true)
            : base(game)
        {
            // Export the service.
            this.Game.Services.AddService(typeof(ISkyDome), this); 

            this._dynamicCloudsEnabled = enableDynamicClouds;
        }

        #region DrawableGameComponent overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Import require services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof (IInputManager));
            this._assetManager = (IAssetManager)this.Game.Services.GetService(typeof(IAssetManager));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load the dome.
            /* TODO: This is a .x model that is not loading with the new pipeline tool
            this._skyDome = this._assetManager.GetModel(Asset.SkyDomeModel, ModelType.Voxeliq);
            this._skyDome.Meshes[0].MeshParts[0].Effect = this._assetManager.GetEffect(Asset.SkyDomeEffect);
            */

            // Load maps.
            // this._cloudMap = this._assetManager.GetTexture(Asset.CloudMapTexture);
            this._starMap = this._assetManager.GetTexture(
                Asset.StarMapTexture,
                Engine.Core.Engine.Instance.TheCustomContentManager);

            // For gpu generated clouds.
            this._perlinNoiseEffect = this._assetManager.GetEffect(Asset.PerlinNoiseEffect);
            var presentationParameters = GraphicsDevice.PresentationParameters;
            this._cloudsRenderTarget = new RenderTarget2D(GraphicsDevice, 
                                                          presentationParameters.BackBufferWidth,
                                                          presentationParameters.BackBufferHeight, 
                                                          false, 
                                                          SurfaceFormat.Color, 
                                                          DepthFormat.Depth16); // the mipmap does not work on all configurations            

            this._staticCloudMap = this.CreateStaticCloudMap(32);
            this._fullScreenVertices = SetUpFullscreenVertices();
        }

        public override void Update(GameTime gameTime)
        {
            if (!this._dynamicCloudsEnabled)
                return;

            this.GeneratePerlinNoise(gameTime); // if dynamic-cloud generation is on, generate them.
        }

        /// <summary>
        /// Draws the sky.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            this.Draw(null);   
        }

        #endregion

        #region ISkyDome implementation

        public void ToggleDynamicClouds()
        {
            this._dynamicCloudsEnabled = !this._dynamicCloudsEnabled;
        }

        public void Draw(Effect effect=null, EffectType type=EffectType.None)
        {
            // We just draw during the initial step of our deffered lighting pass.
            // ALSO: We are using the special SkyDome effect instead of our deffered effect
            if (type != EffectType.Deferred1SceneEffect)
            {
                return;
            }

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.None; // disable the depth-buffer for drawing the sky because it's the farthest object we'll be drawing.

            var modelTransforms = new Matrix[this._skyDome.Bones.Count]; // transform dome's bones.
            this._skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            RotationStars += 0.0001f;
            RotationClouds = 0;

            // draw stars
            Matrix wStarMatrix = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(100) * Matrix.CreateTranslation(new Vector3(this._inputManager.CurrentCamera.Position.X, this._inputManager.CurrentCamera.Position.Y - 40, this._inputManager.CurrentCamera.Position.Z)); // move sky to camera position and should be scaled -- bigger than the world.
            foreach (ModelMesh mesh in this._skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wStarMatrix;

                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyStarDome"];

                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(_inputManager.CurrentCamera.ViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(_inputManager.CurrentCamera.ProjectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(this._starMap);
                    currentEffect.Parameters["xNightColor"].SetValue(NightColor);
                    currentEffect.Parameters["xSunColor"].SetValue(OverheadSunColor);
                    currentEffect.Parameters["xHorizonColor"].SetValue(HorizonColor);

                    currentEffect.Parameters["xMorningTint"].SetValue(MorningTint);
                    currentEffect.Parameters["xEveningTint"].SetValue(EveningTint);
                    currentEffect.Parameters["xTimeOfDay"].SetValue(Time.GetGameTimeOfDay());
                }
                mesh.Draw();
            }

            // draw clouds
            var matrix = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(100) * Matrix.CreateTranslation(new Vector3(this._inputManager.CurrentCamera.Position.X, this._inputManager.CurrentCamera.Position.Y - 40, this._inputManager.CurrentCamera.Position.Z)); // move sky to camera position and should be scaled -- bigger than the world.
            foreach (var mesh in _skyDome.Meshes)
            {
                foreach (var currentEffect in mesh.Effects)
                {
                    var worldMatrix = modelTransforms[mesh.ParentBone.Index] * matrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyStarDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(_inputManager.CurrentCamera.ViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(_inputManager.CurrentCamera.ProjectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue((Texture2D)this._cloudsRenderTarget);
                    currentEffect.Parameters["xNightColor"].SetValue(NightColor);
                    currentEffect.Parameters["xSunColor"].SetValue(OverheadSunColor);
                    currentEffect.Parameters["xHorizonColor"].SetValue(HorizonColor);
                    currentEffect.Parameters["xMorningTint"].SetValue(MorningTint);
                    currentEffect.Parameters["xEveningTint"].SetValue(EveningTint);
                    currentEffect.Parameters["xTimeOfDay"].SetValue(Time.GetGameTimeOfDay());
                }
                mesh.Draw();
            }

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default; // reset back the depth-buffer.
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Sets screen vertices for skydome.
        /// </summary>
        /// <returns></returns>
        private static VertexPositionTexture[] SetUpFullscreenVertices()
        {
            var vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, -1f), new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, -1f), new Vector2(1, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, -1f), new Vector2(0, 1));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, -1f), new Vector2(1, 1));

            return vertices;
        }

        private Texture2D CreateStaticCloudMap(int resolution)
        {
            var rand = new Random();
            var noisyColors = new Color[resolution*resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                    noisyColors[x + y*resolution] = new Color(new Vector3(rand.Next(1000)/1000.0f, 0, 0));

            var noiseImage = new Texture2D(GraphicsDevice, resolution, resolution, true, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        /// <summary>
        /// Generates dynamic clouds within GPU.
        /// </summary>
        private void GeneratePerlinNoise(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(this._cloudsRenderTarget);
            //GraphicsDevice.Clear(Color.White);

            _perlinNoiseEffect.CurrentTechnique = _perlinNoiseEffect.Techniques["PerlinNoise"];
            _perlinNoiseEffect.Parameters["xTexture"].SetValue(this._staticCloudMap);
            _perlinNoiseEffect.Parameters["xOvercast"].SetValue(CloudOvercast);
            _perlinNoiseEffect.Parameters["xTime"].SetValue((float) gameTime.TotalGameTime.TotalMilliseconds/100000.0f);

            foreach (EffectPass pass in _perlinNoiseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, _fullScreenVertices, 0, 2);
            }

            GraphicsDevice.SetRenderTarget(null);
            // this._cloudMap = _cloudsRenderTarget;
        }

        #endregion

    }
}