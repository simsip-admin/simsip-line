/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using Engine.Assets;
using Engine.Blocks;
using Engine.Common.Logging;
using Engine.Graphics;
using Engine.Graphics.Texture;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Input;
using Simsip.LineRunner.Effects.Deferred;


namespace Engine.Sky
{
    public interface INewSky : IUpdateable, IDrawable
    {
        void AdjustHeight(byte _highestSolidBlockOffset);

        void Draw(Effect effect = null, EffectType type = EffectType.None);
    }

    public class NewSky : DrawableGameComponent, INewSky
    {
        // Sky movement implementation
        private bool _skyMoveInitialized;               // One time initialization for sky movement
        private const float _skyMoveDuration = 300f;    // In seconds
        private float _skyMoveTimeAccumulated;          // We accumulate elapsed time up to duration then move in other direction
        private Matrix _skyViewMatrix;                  // Our own personal view matrix to display cloud movement
        private const float _skyMoveDistance = 100f;    // Depends on size below - we want to have a movement length that stays within cloud bounds
        private float _skyMoveDirection = 1f;           // We will flip the sign of this and use in calculations to control direction of movement

        private const int size = 150;
        
        private bool[,] Clouds = new bool[size, size];

        /// <summary>
        /// The vertex list.
        /// </summary>
        // public List<BlockVertex> VertexList;
        // public List<VertexPositionColorTexture> VertexList;
        // public VertexPositionColorTexture[] VertexArray;
        public List<VertexPositionNormalTexture> VertexList;
        public VertexPositionNormalTexture[] VertexArray;
        
        /// <summary>
        /// The index list.
        /// </summary>
        public List<short> IndexList;
        
        /// <summary>
        /// Vertex buffer for chunk's blocks.
        /// </summary>
        public VertexBuffer VertexBuffer { get; set; }

        /// <summary>
        /// Index buffer for chunk's blocks.
        /// </summary>
        public IndexBuffer IndexBuffer { get; set; }

        public short Index;

        private bool _meshBuilt = false;

        private BasicEffect _basicEffect;
        private Effect _blockEffect; // block effect.
        private Texture2D _blockTextureAtlas; // block texture atlas

        private IInputManager _inputManager;
        private IAssetManager _assetManager;
        private IFogger _fogger;

        private bool _initializedOnMainThread;

        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility.

        public NewSky(Game game)
            : base(game)
        {
            // this.VertexList = new List<VertexPositionColorTexture>();
            this.VertexList = new List<VertexPositionNormalTexture>();
            this.IndexList = new List<short>();
            this.Index = 0;

            this.Game.Services.AddService(typeof(INewSky), this); // export service.
        }

        public void AdjustHeight(byte highestSolidBlockOffset)
        {
            float adjustedHeight = highestSolidBlockOffset + 10;

            int vertexCount = VertexList.Count;
            for(int i = 0; i < vertexCount; i++)
            {
                var vertex = VertexList[i];
                vertex.Position.Y = adjustedHeight;
                VertexList[i] = vertex;
            }

            VertexBuffer.SetData(VertexList.ToArray());
        }

        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[,] colors2D = null;
            try
            {
                Color[] colors1D = new Color[texture.Width * texture.Height];
                texture.GetData(colors1D);

                colors2D = new Color[texture.Width, texture.Height];
                for (int x = 0; x < texture.Width; x++)
                    for (int y = 0; y < texture.Height; y++)
                        colors2D[x, y] = colors1D[x + y * texture.Width];
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception in TextureTo2DArray: " + ex);
            }
            return colors2D;
        }

        public override void Initialize()
        {
            Logger.Trace("init()");

            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._fogger = (IFogger)this.Game.Services.GetService(typeof(IFogger));
            this._assetManager = (IAssetManager)this.Game.Services.GetService(typeof(IAssetManager));

            if (this._assetManager == null)
                throw new NullReferenceException("Can not find asset manager component.");

            base.Initialize();
        }

        private void InitializeOnMainThread()
        {
            var colors = TextureTo2DArray(_assetManager.GetTexture(Asset.CloudTexture));

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    this.Clouds[x, z] = colors[x, z] == Color.White;

                    //float cloudiness = SimplexNoise.noise(x*0.009f, 0, z*0.009f)*1f;
                    //this.Clouds[x, z] = cloudiness > 0.5f;
                }
            }
        }

        protected override void LoadContent()
        {
            // this._blockEffect = this._assetManager.GetEffect(Asset.BlockEffect);
            this._blockTextureAtlas = this._assetManager.GetTexture(Asset.BlockTextureAtlas);

            this._basicEffect = new BasicEffect(Game.GraphicsDevice);
            _basicEffect.LightingEnabled = false;
            _basicEffect.TextureEnabled = true;
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.Texture = this._blockTextureAtlas;
        }

        public override void Update(GameTime gameTime)
        {
            // Some opengl initialization has to be done on main thread
            if (!_initializedOnMainThread)
            {
                InitializeOnMainThread();

                _initializedOnMainThread = true;
            }

            this.BuildMesh();

            // The rest we don't start updating until we have set down
            // and positioned ourselves
            if (!this._inputManager.TheLineRunnerControllerInput.Ready)
            {
                return;
            }

            // One time sky movement initialization
            if (!this._skyMoveInitialized)
            {
                // Grab our stationary camera's position
                var cameraPositionAdjusted = this._inputManager.TheStationaryCamera.Position; // Matrix.Invert(this._player.ViewDirection).Translation;

                // Adjust x to be at start position of our movement - but still within bounds of clouds
                var adjustToX = NewSky._skyMoveDistance / 2f;
                cameraPositionAdjusted.X -= adjustToX;

                // Ok, we are good to go to get our initial view matrix created
                this._skyViewMatrix = Matrix.CreateLookAt(cameraPositionAdjusted, this._inputManager.TheStationaryCamera.Target, Vector3.Up);

                this._skyMoveInitialized = true;
            }

            // Get how much time has elapsed weighted by our duration
            var adjustedIncrement = (float)gameTime.ElapsedGameTime.TotalSeconds / NewSky._skyMoveDuration;
            this._skyMoveTimeAccumulated += adjustedIncrement;

            // Have we reached end for current movement?
            if (this._skyMoveTimeAccumulated > 1.0)
            {
                // Reset
                this._skyMoveTimeAccumulated = 0f;

                // Change direction
                this._skyMoveDirection = -this._skyMoveDirection;
            }
            else
            {
                // Ok, we can still move so increment our sky camera's view in +/- direction by appropriate increment
                var incrementDistanceToCover = adjustedIncrement * NewSky._skyMoveDistance;
                Vector3 viewPositionAdjusted = Matrix.Invert(this._skyViewMatrix).Translation;
                viewPositionAdjusted.X += incrementDistanceToCover * this._skyMoveDirection;
                Vector3 targetPositionAdjusted = viewPositionAdjusted + Vector3.Forward;
                this._skyViewMatrix = Matrix.CreateLookAt(viewPositionAdjusted, targetPositionAdjusted, Vector3.Up);
            }

            _basicEffect.World = this._inputManager.LineRunnerCamera.WorldMatrix;
            _basicEffect.View = this._skyViewMatrix;
            _basicEffect.Projection = this._inputManager.LineRunnerCamera.ProjectionMatrix;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            this.Draw(null);
        }

        public void Draw(Effect effect=null, EffectType type=EffectType.None)
        {
            try
            {
                if (effect != null)
                {
                    // TODO: Take out when moving to skydome
                    var skyColor = new Color(128, 173, 254);
                    Game.GraphicsDevice.Clear(skyColor);

                    switch (type)
                    {
                        case EffectType.Deferred1SceneEffect:
                            {
                                effect.Parameters["xWorld"].SetValue(Matrix.Identity);
                                effect.Parameters["xTexture"].SetValue(this._blockTextureAtlas);
                                break;
                            }
                        case EffectType.ShadowMapEffect:
                            {
                                effect.Parameters["xWorld"].SetValue(Matrix.Identity);
                                break;
                            }
                    }

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        if (IndexBuffer == null || VertexBuffer == null)
                            continue;

                        if (VertexBuffer.VertexCount == 0)
                            continue;

                        if (IndexBuffer.IndexCount == 0)
                            continue;

                        Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);
                        Game.GraphicsDevice.Indices = IndexBuffer;
                        Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);
                    }
                }
                else
                {
                    var skyColor = new Color(128, 173, 254);
                    Game.GraphicsDevice.Clear(skyColor);

                    RasterizerState rasterizerState1 = new RasterizerState();
                    rasterizerState1.CullMode = CullMode.None;
                    Game.GraphicsDevice.RasterizerState = rasterizerState1;

                    foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        if (IndexBuffer == null || VertexBuffer == null)
                            continue;

                        if (VertexBuffer.VertexCount == 0)
                            continue;

                        if (IndexBuffer.IndexCount == 0)
                            continue;

                        Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);
                        Game.GraphicsDevice.Indices = IndexBuffer;
                        Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);
                    }
                    return;

                    Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    Game.GraphicsDevice.BlendState = BlendState.Opaque;

                    /*
                    _basicEffect.World = Matrix.Identity;
                    _basicEffect.ViewDirection = this._camera.ViewDirection;
                    _basicEffect.ProjectionMatrix = this._camera.ProjectionMatrix;
                    _basicEffect.EnableDefaultLighting();
                    _basicEffect.TextureEnabled = true;
                    _basicEffect.Texture = _blockTextureAtlas;
                    */

                    // general parameters
                    _blockEffect.Parameters["World"].SetValue(Matrix.Identity);
                    _blockEffect.Parameters["ViewDirection"].SetValue(this._inputManager.LineRunnerCamera.ViewMatrix);
                    _blockEffect.Parameters["ViewDirection"].SetValue(this._skyViewMatrix);
                    _blockEffect.Parameters["ProjectionMatrix"].SetValue(this._inputManager.LineRunnerCamera.ProjectionMatrix);

                    /*
                    _blockEffect.Parameters["CameraPosition"].SetValue(this._camera.Position);

                    // texture parameters
                    _blockEffect.Parameters["BlockTextureAtlas"].SetValue(_blockTextureAtlas);

                    // atmospheric settings
                    _blockEffect.Parameters["SunColor"].SetValue(World.SunColor);
                    _blockEffect.Parameters["NightColor"].SetValue(World.NightColor);
                    _blockEffect.Parameters["HorizonColor"].SetValue(World.HorizonColor);
                    _blockEffect.Parameters["MorningTint"].SetValue(World.MorningTint);
                    _blockEffect.Parameters["EveningTint"].SetValue(World.EveningTint);

                    // time of day parameters
                    _blockEffect.Parameters["TimeOfDay"].SetValue(Time.GetGameTimeOfDay());

                    // fog parameters
                    _blockEffect.Parameters["FogNear"].SetValue(this._fogger.FogVector.X);
                    _blockEffect.Parameters["FogFar"].SetValue(this._fogger.FogVector.Y);
                    */

                    foreach (var pass in this._blockEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        if (IndexBuffer == null || VertexBuffer == null)
                            continue;

                        if (VertexBuffer.VertexCount == 0)
                            continue;

                        if (IndexBuffer.IndexCount == 0)
                            continue;

                        Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);
                        Game.GraphicsDevice.Indices = IndexBuffer;
                        Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);
                    }
                }
            }
            catch(Exception ex)
            {
                string remove = ex.ToString();
            }
        }

        private void BuildMesh()
        {
            if (this._meshBuilt)
                return;

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (this.Clouds[x, z] == false)
                        continue;

                        this.BuildBlockVertices(x, z);
                }
            }

            var vertices = VertexList.ToArray();
            var indices = IndexList.ToArray();

            if (vertices.Length == 0 || indices.Length == 0) 
                return;

            VertexBuffer = new VertexBuffer(this.Game.GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);

            IndexBuffer = new IndexBuffer(this.Game.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);

            this._meshBuilt = true;
        }

        private void BuildBlockVertices(int x, int z)
        {
            var north = z != size-1 && this.Clouds[x, z + 1];
            var south = z != 0 && this.Clouds[x, z - 1];
            var east = x != size-1 && this.Clouds[x + 1, z];
            var west = x != 0 && this.Clouds[x - 1, z];
            
            if (!west) // -xface (if block on west doesn't exist.)
            {
                BuildFaceVertices(x, z, BlockFaceDirection.XDecreasing);
            }
            if (!east) // +xface (if block on east doesn't exist.)
            {
                BuildFaceVertices(x, z, BlockFaceDirection.XIncreasing);
            }
            
            // -yface (as clouds are one block in height, nothing exists on bottom of them)
            BuildFaceVertices(x, z, BlockFaceDirection.YDecreasing);

            // +yface (as clouds are on block in height, nothing exists on top of them).
            BuildFaceVertices(x, z, BlockFaceDirection.YIncreasing);

            if (!south) // -zface (if block on south doesn't exist.)
            {
                BuildFaceVertices(x, z, BlockFaceDirection.ZDecreasing);
            }
            if (!north) // +zface (if block on north doesn't exist.)
            {
                BuildFaceVertices(x, z, BlockFaceDirection.ZIncreasing);
            }
        }

        private void BuildFaceVertices(int x, int z, BlockFaceDirection faceDir)
        {
            BlockTexture texture = Block.GetTexture(BlockType.Snow, faceDir);
            int faceIndex = 0;

            var textureUVMappings = TextureHelper.BlockTextureMappings[(int)texture * 6 + faceIndex];


            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(x, z, new Vector3(1, 1, 1), Vector3.Right, textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(1, 1, 0), Vector3.Right, textureUVMappings[1]);
                        AddVertex(x, z, new Vector3(1, 0, 1), Vector3.Right, textureUVMappings[2]);
                        AddVertex(x, z, new Vector3(1, 0, 0), Vector3.Right, textureUVMappings[5]);
                        AddIndex( 0, 1, 2, 2, 1, 3);
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(x, z, new Vector3(0, 1, 0), Vector3.Left, textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(0, 1, 1), Vector3.Left, textureUVMappings[1]);
                        AddVertex(x, z, new Vector3(0, 0, 0), Vector3.Left, textureUVMappings[5]);
                        AddVertex(x, z, new Vector3(0, 0, 1), Vector3.Left, textureUVMappings[2]);
                        AddIndex( 0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        //BL,BR,TR,BL,TR,TL
                        AddVertex(x, z, new Vector3(1, 1, 1), Vector3.Up, textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(0, 1, 1), Vector3.Up, textureUVMappings[2]);
                        AddVertex(x, z, new Vector3(1, 1, 0), Vector3.Up, textureUVMappings[4]);
                        AddVertex(x, z, new Vector3(0, 1, 0), Vector3.Up, textureUVMappings[5]);
                        AddIndex( 3, 2, 0, 3, 0, 1);
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        //TR,BR,TL,TL,BR,BL
                        AddVertex(x, z, new Vector3(1, 0, 1), Vector3.Down, textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(0, 0, 1), Vector3.Down, textureUVMappings[2]);
                        AddVertex(x, z, new Vector3(1, 0, 0), Vector3.Down, textureUVMappings[4]);
                        AddVertex(x, z, new Vector3(0, 0, 0), Vector3.Down, textureUVMappings[5]);
                        AddIndex( 0, 2, 1, 1, 2, 3);
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(x, z, new Vector3(0, 1, 1), Vector3.Backward, textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(1, 1, 1), Vector3.Backward, textureUVMappings[1]);
                        AddVertex(x, z, new Vector3(0, 0, 1), Vector3.Backward, textureUVMappings[5]);
                        AddVertex(x, z, new Vector3(1, 0, 1), Vector3.Backward, textureUVMappings[2]);
                        AddIndex( 0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(x, z, new Vector3(1, 1, 0), Vector3.Forward, textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(0, 1, 0), Vector3.Forward, textureUVMappings[1]);
                        AddVertex(x, z, new Vector3(1, 0, 0), Vector3.Forward, textureUVMappings[2]);
                        AddVertex(x, z, new Vector3(0, 0, 0), Vector3.Forward, textureUVMappings[5]);
                        AddIndex( 0, 1, 2, 2, 1, 3);
                    }
                    break;
            }
        }

        private void AddVertex(int x, int z, Vector3 addition, Vector3 normal, Vector2 textureCoordinate)
        {
            // TODO: Move sky into position (come back to this to account for gravity in player)
            int adjX = x - (size/2);
            int adjY = 128;
            int adjZ = z - (size/2);

            VertexList.Add(
                new VertexPositionNormalTexture(new Vector3(adjX, adjY, adjZ) + addition,
                normal,
                textureCoordinate));

            /*
            VertexList.Add(
                new VertexPositionColorTexture(new Vector3(adjX, adjY, adjZ) + addition, 
                Color.White, 
                textureCoordinate));
            */

            /*
            VertexList.Add(
                new BlockVertex(
                    new Vector3(x, 128, z) + addition, 
                    textureCoordinate, 
                    1f));
            */
        }

        private void AddIndex( short i1, short i2, short i3, short i4, short i5, short i6)
        {
            IndexList.Add((short)(Index + i1));
            IndexList.Add((short)(Index + i2));
            IndexList.Add((short)(Index + i3));
            IndexList.Add((short)(Index + i4));
            IndexList.Add((short)(Index + i5));
            IndexList.Add((short)(Index + i6));
            Index += 4;
        }
    }
}
