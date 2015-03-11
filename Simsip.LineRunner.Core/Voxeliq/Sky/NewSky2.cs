/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Collections.Generic;
using Engine.Assets;
using Engine.Blocks;
using Engine.Common.Logging;
using Engine.Graphics;
using Engine.Graphics.Texture;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Simsip.LineRunner.Shaders;
using OpenTK.Graphics.ES20;


namespace Engine.Sky
{
    public interface INewSky2
    {
    }

    public class NewSky2 : DrawableGameComponent, INewSky2
    {
        public RenderTarget2D BackgroundRenderTarget { get; private set; }

        private const int size = 150;
        // private const int size = 10;

        private bool[,] Clouds = new bool[size, size];

        /// <summary>
        /// The vertex buffer list.
        /// </summary>
        public BlockVertexBuffer BlockVertBuff;

        /// <summary>
        /// The vertex list.
        /// </summary>
        public List<BlockVertex> VertexList;

        /// <summary>
        /// The index list.
        /// </summary>
        public List<int> IndexList;

        /// <summary>
        /// Vertex buffer for chunk's blocks.
        /// </summary>
        public VertexBuffer VertexBuffer { get; set; }

        /// <summary>
        /// Index buffer for chunk's blocks.
        /// </summary>
        public IndexBuffer IndexBuffer { get; set; }

        public int Index;

        private bool _meshBuilt = false;

        private Effect _blockEffect; // block effect.
        private Texture2D _blockTextureAtlas; // block texture atlas

        private ICamera _camera;
        private IAssetManager _assetManager;
        private IFogger _fogger;

        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility.

        private ShaderProgram _shaderProgram;

        // test
        private readonly string vPositionAttribKey = "vPosition";
        private float[] vVertices;

        // Uniforms
        private readonly string WorldUniformKey = "World";   
        private readonly string ViewUniformKey = "View"; 
        private readonly string ProjectionUniformKey = "Projection"; 
        private readonly string CameraPositionUniformKey = "CameraPosition"; 
        private readonly string TimeOfDayUniformKey = "TimeOfDay"; 
        private readonly string HorizonColorUniformKey = "HorizonColor"; 
        private readonly string SunColorUniformKey = "SunColor";
        private readonly string NightColorUniformKey = "NightColor";
        private readonly string MorningTintUniformKey = "MorningTint";
        private readonly string EveningTintUniformKey = "EveningTint";
        private readonly string FogNearUniformKey = "FogNear";
        private readonly string FogFarUniformKey = "FogFar";
        
        // Samplers
        private readonly string BlockAtlasSamplerKey = "BlockTextureAtlasSampler";
        
        // Attributes
        private readonly string PositionInAttribKey = "PositionIn";
        private readonly string BlockTextureCoordInAttribKey = "BlockTextureCoordIn"; 
        private readonly string SunLightInAttribKey = "SunLightIn";

        // Texture support
        private readonly string BlockAtlasTextureKey = "BlockTextureAtlas";
        private readonly int BlockAtlasTextureUnit = 0;

        // Vertex buffer support
        private readonly string InterleavedVertexBufferKey = "InterleavedBuffer";
        private readonly string IndicesVertexBufferKey = "IndicesBuffer";
        

        public NewSky2(Game game)
            : base(game)
        {
            this.BlockVertBuff = new BlockVertexBuffer();

            this.VertexList = new List<BlockVertex>();
            this.IndexList = new List<int>();
            this.Index = 0;

            this.Game.Services.AddService(typeof(INewSky), this); // export service.

            BackgroundRenderTarget = new RenderTarget2D(GraphicsDevice,
                                GraphicsDevice.PresentationParameters.BackBufferWidth,
                                GraphicsDevice.PresentationParameters.BackBufferHeight,
                                false,
                                GraphicsDevice.PresentationParameters.BackBufferFormat,
                                GraphicsDevice.PresentationParameters.DepthStencilFormat);
        }


        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }

        public override void Initialize()
        {
            Logger.Trace("init()");

            this._camera = (ICamera)this.Game.Services.GetService(typeof(ICamera));
            this._fogger = (IFogger)this.Game.Services.GetService(typeof(IFogger));
            this._assetManager = (IAssetManager)this.Game.Services.GetService(typeof(IAssetManager));

            if (this._assetManager == null)
                throw new NullReferenceException("Can not find asset manager component.");

            var colors = TextureTo2DArray(_assetManager.CloudTexture);

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    this.Clouds[x, z] = colors[x, z] == Color.White;

                    //float cloudiness = SimplexNoise.noise(x*0.009f, 0, z*0.009f)*1f;
                    //this.Clouds[x, z] = cloudiness > 0.5f;
                }
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            try
            {
                this._blockEffect = this._assetManager.BlockEffect;
                this._blockTextureAtlas = this._assetManager.BlockTextureAtlas;

                _shaderProgram = new ShaderProgram("BlockEffect.vert", "BlockEffect.frag");

#if DEBUG
                // Don't let incoming opengl errors confuse us
                ShaderProgram.ClearErrors();
#endif

                // TODO: Do we need to do this?
                _shaderProgram.UseProgram(true);

                // Initialize uniform and attribute locations we are interested in
                /*
                _shaderProgram.InitUniformLocation(WorldUniformKey);
                _shaderProgram.InitUniformLocation(ViewUniformKey);
                _shaderProgram.InitUniformLocation(ProjectionUniformKey);
                _shaderProgram.InitUniformLocation(CameraPositionUniformKey);

                _shaderProgram.InitUniformLocation(TimeOfDayUniformKey);

                _shaderProgram.InitUniformLocation(HorizonColorUniformKey);
                _shaderProgram.InitUniformLocation(SunColorUniformKey);
                _shaderProgram.InitUniformLocation(NightColorUniformKey);
                _shaderProgram.InitUniformLocation(MorningTintUniformKey);
                _shaderProgram.InitUniformLocation(EveningTintUniformKey);

                _shaderProgram.InitUniformLocation(FogNearUniformKey);
                _shaderProgram.InitUniformLocation(FogFarUniformKey);
                
                 _shaderProgram.InitUniformLocation(BlockAtlasSamplerKey);
                 */

                _shaderProgram.InitAttribLocation(vPositionAttribKey);
                /*
                _shaderProgram.InitAttribLocation(PositionInAttribKey);
                _shaderProgram.InitAttribLocation(BlockTextureCoordInAttribKey);
                _shaderProgram.InitAttribLocation(SunLightInAttribKey);

                // Bind our uniforms
                _shaderProgram.UniformMatrix(WorldUniformKey, Matrix.Identity);
                _shaderProgram.UniformMatrix(ViewUniformKey, this._camera.View);
                _shaderProgram.UniformMatrix(ProjectionUniformKey, this._camera.Projection);
                _shaderProgram.Uniform3(CameraPositionUniformKey, this._camera.Position);

                _shaderProgram.Uniform1(TimeOfDayUniformKey, Time.GetGameTimeOfDay());

                _shaderProgram.Uniform4(HorizonColorUniformKey, World.HorizonColor);
                _shaderProgram.Uniform4(SunColorUniformKey, World.SunColor);
                _shaderProgram.Uniform4(NightColorUniformKey, World.NightColor);
                _shaderProgram.Uniform4(MorningTintUniformKey, World.MorningTint);
                _shaderProgram.Uniform4(EveningTintUniformKey, World.EveningTint);

                _shaderProgram.Uniform1(FogNearUniformKey, this._fogger.FogVector.X);
                _shaderProgram.Uniform1(FogFarUniformKey, this._fogger.FogVector.Y);
                */

                // Load/Bind our texture
                /*
                _shaderProgram.LoadTexture(BlockAtlasTextureKey, this._blockTextureAtlas);
                _shaderProgram.BindTextureToUniform(BlockAtlasTextureUnit,
                                                   BlockAtlasTextureKey,
                                                   BlockAtlasSamplerKey);
  
                // Load our shape
                this.BuildMesh();
                BlockVertBuff.Update();
                */
                vVertices = new float[] {0.0f, 0.5f, 0.0f,
                                        -0.5f, -0.5f, 0.0f,
                                         0.5f, -0.5f, 0.0f};

#if DEBUG
                // Don't let incoming opengl errors from BuildMesh() confuse us
                ShaderProgram.ClearErrors();
#endif

                // Load vertex buffers
                /*
                _shaderProgram.LoadVertexBuffer(InterleavedVertexBufferKey,
                                    BlockVertBuff.Buffer.Length,
                                    BlockVertBuff.Buffer);
                _shaderProgram.LoadIndexBuffer(IndicesVertexBufferKey,
                                    IndexList.Count,
                                    IndexList.ToArray());

                // Bind previously loaded interleaved vertex buffer to attribute arrays
                _shaderProgram.BindVertexBuffer(InterleavedVertexBufferKey);
                _shaderProgram.EnableVertexAttribArray(PositionInAttribKey);
                _shaderProgram.EnableVertexAttribArray(BlockTextureCoordInAttribKey);
                _shaderProgram.EnableVertexAttribArray(SunLightInAttribKey);
                */
                _shaderProgram.EnableVertexAttribArray(vPositionAttribKey);

                // Point attribute arrays into interleaved vertex buffer
                _shaderProgram.VertexAttribPointer(vPositionAttribKey, 3, All.Float, 0, vVertices);
                /*
                _shaderProgram.VertexAttribPointer(PositionInAttribKey, 3, All.Float, 6 * sizeof(float), 0);
                _shaderProgram.VertexAttribPointer(BlockTextureCoordInAttribKey, 2, All.Float, 6 * sizeof(float), 3);
                _shaderProgram.VertexAttribPointer(SunLightInAttribKey, 1, All.Float, 6 * sizeof(float), 5);

                // Bind previously loaded index vertex buffer for subsequent call to DrawElements
                _shaderProgram.BindVertexBufferToIndices(IndicesVertexBufferKey);
                */
#if DEBUG
                _shaderProgram.ValidateProgram();
#endif

                // TODO: Is this needed (see above as well)
                _shaderProgram.UseProgram(false);
            }
            catch(Exception ex)
            {
                string temp = ex.ToString();
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            try
            {
                GraphicsDevice.SetRenderTarget(BackgroundRenderTarget);
#if DEBUG
                // Don't let incoming opengl errors confuse us
                ShaderProgram.ClearErrors();
#endif
                /* TODO: Need #defines for this
                // Did we screw anything up in last cycle?
                _shaderProgram.ValidateProgram();
                 */

                // Enable the program we want for subsequent call to draw elements
                _shaderProgram.UseProgram(true);

                //
                // TODO: Do we have to rebind (see LoadContent)
                //

                // Bind our uniforms
                /*
                _shaderProgram.UniformMatrix(WorldUniformKey, Matrix.Identity);
                _shaderProgram.UniformMatrix(ViewUniformKey, this._camera.View);
                _shaderProgram.UniformMatrix(ProjectionUniformKey, this._camera.Projection);
                _shaderProgram.Uniform3(CameraPositionUniformKey, this._camera.Position);

                _shaderProgram.Uniform1(TimeOfDayUniformKey, Time.GetGameTimeOfDay());

                _shaderProgram.Uniform4(HorizonColorUniformKey, World.HorizonColor);
                _shaderProgram.Uniform4(SunColorUniformKey, World.SunColor);
                _shaderProgram.Uniform4(NightColorUniformKey, World.NightColor);
                _shaderProgram.Uniform4(MorningTintUniformKey, World.MorningTint);
                _shaderProgram.Uniform4(EveningTintUniformKey, World.EveningTint);

                _shaderProgram.Uniform1(FogNearUniformKey, this._fogger.FogVector.X);
                _shaderProgram.Uniform1(FogFarUniformKey, this._fogger.FogVector.Y);
                */

                // Bind our texture
                /*
                _shaderProgram.BindTextureToUniform(BlockAtlasTextureUnit,
                                   BlockAtlasTextureKey,
                                   BlockAtlasSamplerKey);

                // Bind previously loaded interleaved vertex buffer to attribute arrays
                _shaderProgram.BindVertexBuffer(InterleavedVertexBufferKey);
                _shaderProgram.EnableVertexAttribArray(PositionInAttribKey);
                _shaderProgram.EnableVertexAttribArray(BlockTextureCoordInAttribKey);
                _shaderProgram.EnableVertexAttribArray(SunLightInAttribKey);
                */
                _shaderProgram.EnableVertexAttribArray(vPositionAttribKey);

                // Point attribute arrays into interleaved vertex buffer
                _shaderProgram.VertexAttribPointer(vPositionAttribKey, 3, All.Float, 0, vVertices);
                /*
                _shaderProgram.VertexAttribPointer(PositionInAttribKey, 3, All.Float, 6 * sizeof(float), 0);
                _shaderProgram.VertexAttribPointer(BlockTextureCoordInAttribKey, 2, All.Float, 6 * sizeof(float), 3);
                _shaderProgram.VertexAttribPointer(SunLightInAttribKey, 1, All.Float, 6 * sizeof(float), 5);

                // Bind previously loaded index vertex buffer for subsequent call to DrawElements
                _shaderProgram.BindVertexBufferToIndices(IndicesVertexBufferKey);
                */

                /* TODO: Need #defines for this
#if DEBUG
                _shaderProgram.ValidateProgram();
#endif
                 */

                // Render
                _shaderProgram.DrawArrays(All.Triangles, 3);
                // _shaderProgram.DrawElements(All.Triangles, IndexList.Count, 0);

                // Clean-up
                /*
                _shaderProgram.UnbindVertexBufferForIndices();
                _shaderProgram.DisableVertexAttribArray(PositionInAttribKey);
                _shaderProgram.DisableVertexAttribArray(BlockTextureCoordInAttribKey);
                _shaderProgram.DisableVertexAttribArray(SunLightInAttribKey);
                _shaderProgram.UnbindVertexBuffer();
                 */
                _shaderProgram.UseProgram(false);

                GraphicsDevice.SetRenderTarget(null);

                /*
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                Game.GraphicsDevice.BlendState = BlendState.Opaque;

                // general parameters
                _blockEffect.Parameters["World"].SetValue(Matrix.Identity);
                _blockEffect.Parameters["View"].SetValue(this._camera.View);
                _blockEffect.Parameters["Projection"].SetValue(this._camera.Projection);
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

                foreach (var pass in this._blockEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    if (IndexBuffer == null || VertexBuffer == null)
                        continue;

                    if (VertexBuffer.VertexCount == 0)
                        continue;

                    if (IndexBuffer.IndexCount == 0)
                        continue;

                    try
                    {
                        Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);
                        Game.GraphicsDevice.Indices = IndexBuffer;
                        Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);
                    }
                    catch(Exception ex)
                    {
                        string temp = ex.ToString();
                    }
                }
                */

                base.Draw(gameTime);
            }
            catch(Exception ex)
            {
                string temp = ex.ToString();
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

            VertexBuffer = new VertexBuffer(this.Game.GraphicsDevice, typeof(BlockVertex), vertices.Length, BufferUsage.WriteOnly);
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

            // TODO: Swapping back
            /*
            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(x,z, new Vector3(1, 1, 1), textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(1, 1, 0), textureUVMappings[1]);
                        AddVertex(x, z, new Vector3(1, 0, 1), textureUVMappings[2]);
                        AddVertex(x, z, new Vector3(1, 0, 0), textureUVMappings[5]);
                        AddIndex( 0, 1, 2, 2, 1, 3);
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(x, z, new Vector3(0, 1, 0), textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(0, 1, 1), textureUVMappings[1]);
                        AddVertex(x, z, new Vector3(0, 0, 0), textureUVMappings[5]);
                        AddVertex(x, z, new Vector3(0, 0, 1), textureUVMappings[2]);
                        AddIndex( 0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        //BL,BR,TR,BL,TR,TL
                        AddVertex(x, z, new Vector3(1, 1, 1), textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(0, 1, 1), textureUVMappings[2]);
                        AddVertex(x, z, new Vector3(1, 1, 0), textureUVMappings[4]);
                        AddVertex(x, z, new Vector3(0, 1, 0), textureUVMappings[5]);
                        AddIndex( 3, 2, 0, 3, 0, 1);
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        //TR,BR,TL,TL,BR,BL
                        AddVertex(x, z, new Vector3(1, 0, 1), textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(0, 0, 1), textureUVMappings[2]);
                        AddVertex(x, z, new Vector3(1, 0, 0), textureUVMappings[4]);
                        AddVertex(x, z, new Vector3(0, 0, 0), textureUVMappings[5]);
                        AddIndex( 0, 2, 1, 1, 2, 3);
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(x, z, new Vector3(0, 1, 1), textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(1, 1, 1), textureUVMappings[1]);
                        AddVertex(x, z, new Vector3(0, 0, 1), textureUVMappings[5]);
                        AddVertex(x, z, new Vector3(1, 0, 1), textureUVMappings[2]);
                        AddIndex( 0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(x, z, new Vector3(1, 1, 0), textureUVMappings[0]);
                        AddVertex(x, z, new Vector3(0, 1, 0), textureUVMappings[1]);
                        AddVertex(x, z, new Vector3(1, 0, 0), textureUVMappings[2]);
                        AddVertex(x, z, new Vector3(0, 0, 0), textureUVMappings[5]);
                        AddIndex( 0, 1, 2, 2, 1, 3);
                    }
                    break;
            }
            */
        }

        private void AddVertex(int x, int z, Vector3 addition, Vector2 textureCoordinate)
        {
            // TODO: Swapping back
            /*
            var vertex = new BlockVertex(new Vector3(x, 128, z) + addition, textureCoordinate, 1f);

            BlockVertBuff.Add(vertex);
            VertexList.Add(vertex);
            */
        }

        private void AddIndex( int i1, int i2, int i3, int i4, int i5, int i6)
        {
            IndexList.Add(Index + i1);
            IndexList.Add(Index + i2);
            IndexList.Add(Index + i3);
            IndexList.Add(Index + i4);
            IndexList.Add(Index + i5);
            IndexList.Add(Index + i6);
            Index += 4;
        }
    }
}
