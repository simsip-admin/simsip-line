/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using Engine.Assets;
using Engine.Blocks;
using Engine.Chunks.Generators.Biomes;
using Engine.Chunks.Generators.Terrain;
using Engine.Chunks.Processors;
using Engine.Common.Logging;
using Engine.Common.Vector;
using Engine.Debugging.Timing;
using Engine.Graphics;
using Engine.Sky;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Input;
using Simsip.LineRunner.Effects.Stock;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Foundation;
#else
using System.Threading;
using Simsip.LineRunner;
#endif

namespace Engine.Chunks
{
    public interface IChunkCache : IUpdateable, IDrawable
    {
        bool Ready { get;  }

        /// <summary>
        /// Returns chunk that exists in given world position.
        /// </summary>
        /// <param name="x">X in world coordinate.</param>
        /// <param name="z">Z in world coordinate</param>
        /// <returns>Returns the <see cref="Chunk"/> that exists in given position or null if otherwise.</returns>
        Chunk GetChunkByWorldPosition(int x, int z);

        /// <summary>
        /// Returns chunk that exists in given relative position.
        /// </summary>
        /// <param name="x">X in relative coordinate.</param>
        /// <param name="z">Z in relative coordinate</param>
        /// <returns>Returns the <see cref="Chunk"/> that exists in given position or null if otherwise.</returns>
        Chunk GetChunkByRelativePosition(int x, int z);

        /// <summary>
        /// Returns the chunk in given neighborhood.
        /// </summary>
        /// <param name="origin">The origin chunk.</param>
        /// <param name="edge">The neighbor edge.</param>
        /// <returns></returns>
        Chunk GetNeighborChunk(Chunk origin, Chunk.Edges edge);

        /// <summary>
        /// Returns chunks drawn in last draw() call.
        /// </summary>
        int ChunksDrawn { get; }

        /// <summary>
        /// Returns true if given chunk is in view range.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        bool IsChunkInViewRange(Chunk chunk);

        /// <summary>
        /// Returns true if given chunk is in cache range.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        bool IsChunkInCacheRange(Chunk chunk);

        Dictionary<ChunkState, int> StateStatistics { get; }

        /// <summary>
        /// The terrain generator.
        /// </summary>
        TerrainGenerator Generator { get; set; }

        void SwitchState(GameState state);

        void Draw(Effect effect, EffectType type);
    }

    /// <summary>
    /// The chunk cache that consists of two seperate caches, one for chunks in view range, one for chunks in cache range.
    /// </summary>
    public class ChunkCache : DrawableGameComponent, IChunkCache
    {
        Quad quad;
        Matrix View;
        Matrix Projection;
        Texture2D texture;
        BasicEffect _basicEffect;
        private Random _random = new Random();
        private byte _highestSolidBlockOffset;

        // assets & resources
        private Effect _blockEffect; // block effect.
        // private BasicEffect _basicEffect; // block effect.
        private Texture2D _blockTextureAtlas; // block texture atlas
        private Texture2D _crackTextureAtlas; // crack texture atlas // TODO: implement crack textures!

        /// <summary>
        /// The chunk vertex builder.
        /// </summary>
        protected IVertexBuilder VertexBuilder { get; set; }        

        // Required services.
        private INewSky _newSky;
        private IChunkStorage _chunkStorage;
        private IBlockStorage _blockStorage;
        private IInputManager _inputManager;
        private IFogger _fogger;
        private IAssetManager _assetManager;
        private TimeRuler _timeRuler;

        private GameState _currentGameState;

        public bool CacheThreadStarted { get; private set; }

        public Dictionary<ChunkState, int> StateStatistics { get; private set; }

        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility.

        public ChunkCache(Game game)
            : base(game)
        {
            this.Game.Services.AddService(typeof (IChunkCache), this); // export service.

            if (ViewRange > CacheRange) // check if cache range is big enough to include view-range.
                throw new ChunkCacheException(); 

            this.CacheThreadStarted = false;

            this.StateStatistics = new Dictionary<ChunkState, int> // init. the debug stastics.
                                       {
                                           {ChunkState.AwaitingGenerate, 0},
                                           {ChunkState.Generating, 0},
                                           {ChunkState.AwaitingLighting, 0},
                                           {ChunkState.Lighting, 0},
                                           {ChunkState.AwaitingBuild, 0},
                                           {ChunkState.Building, 0},
                                           {ChunkState.Ready, 0},
                                           {ChunkState.AwaitingRelighting, 0},
                                           {ChunkState.AwaitingRebuild, 0},
                                           {ChunkState.AwaitingRemoval, 0},
                                       };
        }

        #region Properties

        public bool Ready { get; private set; }

        /// <summary>
        /// Bounding box for chunk cache.
        /// </summary>
        public static BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// The terrain generator.
        /// </summary>
        public TerrainGenerator Generator { get; set; }

        /// <summary>
        /// Range of cached chunk which can be greater than the view range. 
        /// Chunks in cache range will be only generated and lightened.
        /// </summary>
        public static byte CacheRange = Core.Engine.Instance.Configuration.Cache.CacheRange;

        /// <summary>
        /// Range of viewable chunks by the player.
        /// Chunks in view range will be always generated, lightend and built.
        /// </summary>
        public static byte ViewRange = Core.Engine.Instance.Configuration.Cache.ViewRange;

        /// <summary>
        /// Bounding box for view range.
        /// </summary>
        public BoundingBox ViewRangeBoundingBox { get; set; }

        /// <summary>
        /// Bounding box for cache range.
        /// </summary>
        public BoundingBox CacheRangeBoundingBox { get; set; }

        /// <summary>
        /// Chunks drawn statistics.
        /// </summary>
        public int ChunksDrawn { get; protected set; }

        #endregion

        #region DrawableGameComponent overrides

        public void SwitchState(GameState gameState)
        {
            this._currentGameState = gameState;

            switch (gameState)
            {
                case GameState.Refresh:
                {
                    // Currently the only parameter that can be user changed is the resource pack
                    // which the get on the Asset.BlockTextureAtlas will correctly resolve for us.
                    this._blockTextureAtlas = this._assetManager.GetTexture(Asset.BlockTextureAtlas);

                    break;
                }
            }
        }

        public override void Initialize()
        {
            Logger.Trace("init()");

            // import required services.
            this._newSky = (INewSky)this.Game.Services.GetService(typeof(INewSky));
            this._blockStorage = (IBlockStorage)this.Game.Services.GetService(typeof(IBlockStorage));
            this._chunkStorage = (IChunkStorage)this.Game.Services.GetService(typeof(IChunkStorage));
            this._inputManager = (IInputManager) this.Game.Services.GetService(typeof (IInputManager));
            this._fogger = (IFogger) this.Game.Services.GetService(typeof (IFogger));
            this.VertexBuilder = (IVertexBuilder) this.Game.Services.GetService(typeof (IVertexBuilder));            
            this._timeRuler = (TimeRuler) this.Game.Services.GetService(typeof (TimeRuler));

            this._assetManager = (IAssetManager)this.Game.Services.GetService(typeof(IAssetManager));
            if (this._assetManager == null)
                throw new NullReferenceException("Can not find asset manager component.");

            // Leaving in, in case we need to force a specific biome for testing
            // this.Generator = new BiomedTerrain(new AntarticTundra());
            int randomBiome = _random.Next(3);
            int randomSeed = _random.Next(100);
            switch (randomBiome)
            {
                case 0:
                    {
                        this.Generator = new BiomedTerrain(new AntarticTundra(), randomSeed);
                        break;
                    }
                case 1:
                    {
                        this.Generator = new BiomedTerrain(new Desert(), randomSeed);
                        break;
                    }
                case 2:
                    {
                        this.Generator = new BiomedTerrain(new RainForest(), randomSeed);
                        break;
                    }
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // this._blockEffect = this._assetManager.GetEffect(Asset.BlockEffect);
            this._blockTextureAtlas = this._assetManager.GetTexture(Asset.BlockTextureAtlas);
            this._crackTextureAtlas = this._assetManager.GetTexture(Asset.CrackTextureAtlas);

            _basicEffect = new BasicEffect(Game.GraphicsDevice);
            _basicEffect.LightingEnabled = false;
            _basicEffect.TextureEnabled = true;
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.Texture = this._blockTextureAtlas;
        }

        public override void Update(GameTime gameTime)
        {
                _basicEffect.World = this._inputManager.CurrentCamera.WorldMatrix;
                _basicEffect.View = this._inputManager.CurrentCamera.ViewMatrix;
                _basicEffect.Projection = this._inputManager.CurrentCamera.ProjectionMatrix;

            this.UpdateBoundingBoxes();

            if (this.CacheThreadStarted)
                return;

#if NETFX_CORE
            IAsyncAction asyncAction = 
                Windows.System.Threading.ThreadPool.RunAsync(
                    (workItem) =>
                    {
                        CacheThread(null);
                    });
#else
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("CacheThread.Start: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
             */

            ThreadPool.QueueUserWorkItem(CacheThread);
            /*
            var cacheThread = new Thread(CacheThread) { IsBackground = true };
            cacheThread.Start();
            */
#endif

            this.CacheThreadStarted = true;
        }

        public override void Draw(GameTime gameTime)
        {
            this.Draw();
        }
        
        #endregion

        #region IChunkCache implementation

        public void Draw(Effect effect = null, EffectType type = EffectType.None)
        {
            try
            {
                if (effect != null)
                {
                    switch (type)
                    {
                        case EffectType.Deferred1SceneEffect:
                            {
                                effect.Parameters["World"].SetValue(Matrix.Identity);
                                effect.Parameters["Texture"].SetValue(this._blockTextureAtlas);
                                break;
                            }
                        case EffectType.ShadowMapEffect:
                            {
                                effect.Parameters["World"].SetValue(Matrix.Identity);
                                break;
                            }
                        case EffectType.StockBasicEffect:
                            {
                                var stockBasicEffect = effect as StockBasicEffect;
                                stockBasicEffect.World = Matrix.Identity;
                                stockBasicEffect.Texture = this._blockTextureAtlas;
                                break;
                            }

                    }

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        foreach (Chunk chunk in this._chunkStorage.Values)
                        {
                            if (chunk.IndexBuffer == null || chunk.VertexBuffer == null)
                                continue;

                            if (chunk.VertexBuffer.VertexCount == 0)
                                continue;

                            if (chunk.IndexBuffer.IndexCount == 0)
                                continue;

                            if (!IsChunkInViewRange(chunk))
                                continue;

                            var viewFrustrum = new BoundingFrustum(this._inputManager.CurrentCamera.ViewMatrix * this._inputManager.CurrentCamera.ProjectionMatrix);
                            if (!chunk.BoundingBox.Intersects(viewFrustrum)) // if chunk is not in view frustrum,
                                continue; // pas it.
                            
                            Game.GraphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
                            Game.GraphicsDevice.Indices = chunk.IndexBuffer;

                            Game.GraphicsDevice.DrawUserIndexedPrimitives
                                <VertexPositionNormalTexture>(
                                    PrimitiveType.TriangleList,
                                    chunk.VertexArray,
                                    0,
                                    chunk.VertexCount,
                                    chunk.IndexArray,
                                    0,
                                    chunk.IndexCount / 3);
                        }
                    }
                }
                else
                {
                    /* TODO: Original
                    RasterizerState rasterizerState1 = new RasterizerState();
                    rasterizerState1.CullMode = CullMode.None;
                    Game.GraphicsDevice.RasterizerState = rasterizerState1;
                    */
                    XNAUtils.DefaultDrawState();

                    // TODO: Check with this below is not working
                    // var viewFrustrum = new BoundingFrustum(this._camera.ViewDirection * this._camera.ProjectionMatrix);

                    foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        /*
                        Game.GraphicsDevice.DrawUserIndexedPrimitives
                            <VertexPositionNormalTexture>(
                            PrimitiveType.TriangleList,
                            quad.Vertices, 0, 4,
                            quad.Indexes, 0, 2);
                         */

                        foreach (Chunk chunk in this._chunkStorage.Values)
                        {
                            if (chunk.IndexBuffer == null || chunk.VertexBuffer == null)
                                continue;

                            if (chunk.VertexBuffer.VertexCount == 0)
                                continue;

                            if (chunk.IndexBuffer.IndexCount == 0)
                                continue;

                            if (!IsChunkInViewRange(chunk))
                                continue;

                            /*
                            if (!chunk.BoundingBox.Intersects(viewFrustrum)) // if chunk is not in view frustrum,
                                continue; // pas it.
                            */

                            Game.GraphicsDevice.DrawUserIndexedPrimitives
                                <VertexPositionNormalTexture>(
                                    PrimitiveType.TriangleList,
                                    chunk.VertexArray,
                                    0,
                                    chunk.VertexCount,
                                    chunk.IndexArray,
                                    0,
                                    chunk.IndexCount / 3);

                            /*
                            Game.GraphicsDevice.DrawUserIndexedPrimitives
                                <VertexPositionColorTexture>(
                                    PrimitiveType.TriangleList,
                                    chunk.VertexArray, 
                                    0,
                                    chunk.VertexCount,
                                    chunk.IndexArray, 
                                    0, 
                                    chunk.IndexCount / 3);
                            */

                            /*
                            Game.GraphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
                            Game.GraphicsDevice.Indices = chunk.IndexBuffer;
                            Game.GraphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                0,
                                0,
                                chunk.VertexBuffer.VertexCount,
                                0,
                                chunk.IndexBuffer.IndexCount / 3);
                            */
                            this.ChunksDrawn++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string remove = ex.ToString();
            }
            return;


            // var viewFrustrum = new BoundingFrustum(this._camera.ViewDirection*this._camera.ProjectionMatrix);

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
            _blockEffect.Parameters["ViewDirection"].SetValue(this._inputManager.CurrentCamera.ViewMatrix);
            _blockEffect.Parameters["ProjectionMatrix"].SetValue(this._inputManager.CurrentCamera.ProjectionMatrix);
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

            this.ChunksDrawn = 0;
            foreach (EffectPass pass in this._blockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in this._chunkStorage.Values)
                {
                    if (chunk.IndexBuffer == null || chunk.VertexBuffer == null)
                        continue;

                    if (chunk.VertexBuffer.VertexCount == 0)
                        continue;

                    if (chunk.IndexBuffer.IndexCount == 0)
                        continue;

                    if (!IsChunkInViewRange(chunk))
                        continue;

                    /*
                    if (!chunk.BoundingBox.Intersects(viewFrustrum)) // if chunk is not in view frustrum,
                        continue; // pas it.
                    */
                    Game.GraphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
                    Game.GraphicsDevice.Indices = chunk.IndexBuffer;
                    // Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.VertexBuffer.VertexCount, 0, chunk.IndexBuffer.IndexCount/3);
                    this.ChunksDrawn++;
                }
            }

            this.StateStatistics[ChunkState.AwaitingGenerate] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.AwaitingGenerate);
            this.StateStatistics[ChunkState.Generating] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.Generating);
            this.StateStatistics[ChunkState.AwaitingLighting] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.AwaitingLighting);
            this.StateStatistics[ChunkState.Lighting] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.Lighting);
            this.StateStatistics[ChunkState.AwaitingRelighting] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.AwaitingRelighting);
            this.StateStatistics[ChunkState.AwaitingBuild] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.AwaitingBuild);
            this.StateStatistics[ChunkState.Building] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.Building);
            this.StateStatistics[ChunkState.AwaitingRebuild] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.AwaitingRebuild);
            this.StateStatistics[ChunkState.Ready] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.Ready);
            this.StateStatistics[ChunkState.AwaitingRemoval] = this._chunkStorage.Values.Count(chunk => chunk.ChunkState == ChunkState.AwaitingRemoval);
        }
        
        /// <summary>
        /// Returns a boolean stating if chunk is current in view range.
        /// </summary>
        /// <param name="chunk">Chunk to check.</param>
        /// <returns><see cref="bool"/></returns>
        public bool IsChunkInViewRange(Chunk chunk)
        {
            return ViewRangeBoundingBox.Contains(chunk.BoundingBox) == ContainmentType.Contains;
        }

        /// <summary>
        /// Returns a boolean stating if chunk is current in cache range.
        /// </summary>
        /// <param name="chunk">Chunk to check.</param>
        /// <returns><see cref="bool"/></returns>
        public bool IsChunkInCacheRange(Chunk chunk)
        {
            return CacheRangeBoundingBox.Contains(chunk.BoundingBox) == ContainmentType.Contains;
        }

        /// <summary>
        /// Returns chunk that exists in given world position.
        /// </summary>
        /// <param name="x">X in world coordinate.</param>
        /// <param name="z">Z in world coordinate</param>
        /// <returns>Returns the <see cref="Chunk"/> that exists in given position or null if otherwise.</returns>
        public Chunk GetChunkByWorldPosition(int x, int z)
        {
            // fix the negative x coordinates.
            if (x < 0)
                x -= Chunk.WidthInBlocks;

            // fix the negative z coordinates.
            if (z < 0)
                z -= Chunk.LengthInBlocks;

            return !this._chunkStorage.ContainsKey(x / Chunk.WidthInBlocks, z / Chunk.LengthInBlocks) ? null : this._chunkStorage[x / Chunk.WidthInBlocks, z / Chunk.LengthInBlocks];
        }

        /// <summary>
        /// Returns chunk that exists in given relative position.
        /// </summary>
        /// <param name="x">X in relative coordinate.</param>
        /// <param name="z">Z in relative coordinate</param>
        /// <returns>Returns the <see cref="Chunk"/> that exists in given position or null if otherwise.</returns>
        public Chunk GetChunkByRelativePosition(int x, int z)
        {
            return !this._chunkStorage.ContainsKey(x, z) ? null : this._chunkStorage[x, z];
        }

        /// <summary>
        /// Returns the chunk in given neighborhood.
        /// </summary>
        /// <param name="origin">The origin chunk.</param>
        /// <param name="edge">The neighbor edge.</param>
        /// <returns></returns>
        public Chunk GetNeighborChunk(Chunk origin, Chunk.Edges edge)
        {
            switch (edge)
            {
                case Chunk.Edges.XDecreasing:
                    return this.GetChunkByRelativePosition(origin.RelativePosition.X - 1, origin.RelativePosition.Z);
                case Chunk.Edges.XIncreasing:
                    return this.GetChunkByRelativePosition(origin.RelativePosition.X + 1, origin.RelativePosition.Z);
                case Chunk.Edges.ZDecreasing:
                    return this.GetChunkByRelativePosition(origin.RelativePosition.X, origin.RelativePosition.Z - 1);
                case Chunk.Edges.ZIncreasing:
                    return this.GetChunkByRelativePosition(origin.RelativePosition.X, origin.RelativePosition.Z + 1);
            }
            return null;
        }



        #endregion

        #region Api

        /// <summary>
        /// Check if given x, y and z coordinates are in bounds of chunk cache.
        /// </summary>
        /// <param name="x">The x coordinate to check.</param>
        /// <param name="y">The y coordinate to check.</param>
        /// <param name="z">The z coordinate to check.</param>
        /// <returns>True if given point/block is in bounds of chunk-cache.</returns>
        /// <remarks>Prefer this method instead of BoundingBox.Contains as blocks need special handling!</remarks>
        public static bool IsInBounds(int x, int y, int z)
        {
            if (x < BoundingBox.Min.X || z < BoundingBox.Min.Z || x >= BoundingBox.Max.X ||
                z >= BoundingBox.Max.Z || y < BoundingBox.Min.Y || y >= BoundingBox.Max.Y)
                return false;

            return true;
        }

        #endregion

        #region Helper methods

        protected void UpdateBoundingBoxes()
        {
            this.ViewRangeBoundingBox = new BoundingBox(
                        new Vector3(this._inputManager.ThePlayerControllerInput.CurrentChunk.WorldPosition.X - (ViewRange*Chunk.WidthInBlocks), 0,
                            this._inputManager.ThePlayerControllerInput.CurrentChunk.WorldPosition.Z - (ViewRange * Chunk.LengthInBlocks)),
                        new Vector3(this._inputManager.ThePlayerControllerInput.CurrentChunk.WorldPosition.X + ((ViewRange + 1)*Chunk.WidthInBlocks),
                            Chunk.HeightInBlocks, this._inputManager.ThePlayerControllerInput.CurrentChunk.WorldPosition.Z + ((ViewRange + 1) * Chunk.LengthInBlocks))
                );

            this.CacheRangeBoundingBox = new BoundingBox(
                        new Vector3(this._inputManager.ThePlayerControllerInput.CurrentChunk.WorldPosition.X - (CacheRange*Chunk.WidthInBlocks), 0,
                            this._inputManager.ThePlayerControllerInput.CurrentChunk.WorldPosition.Z - (CacheRange * Chunk.LengthInBlocks)),
                        new Vector3(this._inputManager.ThePlayerControllerInput.CurrentChunk.WorldPosition.X + ((CacheRange + 1)*Chunk.WidthInBlocks),
                            Chunk.HeightInBlocks,
                            this._inputManager.ThePlayerControllerInput.CurrentChunk.WorldPosition.Z + ((CacheRange + 1) * Chunk.LengthInBlocks))
                );
        }

        private void CacheThread(object state)
        {
            // Debug.WriteLine("Cache thread started");

            while (true)
            {
                if (this._inputManager.ThePlayerControllerInput.CurrentChunk == null)
                    continue;

                this.Process();
            }
        }

        protected void Process()
        {
            // Debug.WriteLine("Cache thread process");

            try
            {
                foreach (var chunk in this._chunkStorage.Values)
                {
                    if (this.IsChunkInViewRange(chunk))
                    {
                        this.ProcessChunkInViewRange(chunk);
                        if (chunk.ChunkState == ChunkState.Ready)
                        {
                            this.Ready = true;
                            /*
#if STOPWATCH
                            Program.TheStopwatch.Stop();
                            Debug.WriteLine("ChunkCache.Ready: " + Program.TheStopwatch.ElapsedMilliseconds);
                            Program.TheStopwatch.Restart();
#endif
*/

                            if (chunk.HighestSolidBlockOffset > _highestSolidBlockOffset)
                            {
                                _highestSolidBlockOffset = chunk.HighestSolidBlockOffset;
                                _newSky.AdjustHeight(_highestSolidBlockOffset);
                            }
                        }
                    }
                    else
                    {
                        if (this.IsChunkInCacheRange(chunk))
                        {
                            this.ProcessChunkInCacheRange(chunk);
                        }
                        else
                        {
                            chunk.ChunkState = ChunkState.AwaitingRemoval;
                            this._chunkStorage.Remove(chunk.RelativePosition.X, chunk.RelativePosition.Z);
                            chunk.Dispose();
                        }
                    }
                }

                if (Core.Engine.Instance.Configuration.World.IsInfinitive)
                    this.RecacheChunks();

                /*
                if (!this.Ready)
                {
                    this.Ready = this._chunkStorage.Values.All(x => x.ChunkState == ChunkState.Ready);
                }
                */
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception in chunk process thread: " + ex);
            }

        }

        private void RecacheChunks()
        {
            this._inputManager.ThePlayerControllerInput.CurrentChunk = this.GetChunkByWorldPosition((int)_inputManager.PlayerCamera.Position.X, (int)_inputManager.PlayerCamera.Position.Z);
            
            if (this._inputManager.ThePlayerControllerInput.CurrentChunk == null)
                return;

            for (int z = -CacheRange; z <= CacheRange; z++)
            {
                for (int x = -CacheRange; x <= CacheRange; x++)
                {
                    if (this._chunkStorage.ContainsKey(this._inputManager.ThePlayerControllerInput.CurrentChunk.RelativePosition.X + x, this._inputManager.ThePlayerControllerInput.CurrentChunk.RelativePosition.Z + z))
                        continue;

                    var chunk = new Chunk(new Vector2Int(this._inputManager.ThePlayerControllerInput.CurrentChunk.RelativePosition.X + x, this._inputManager.ThePlayerControllerInput.CurrentChunk.RelativePosition.Z + z));
                    this._chunkStorage[chunk.RelativePosition.X, chunk.RelativePosition.Z] = chunk;
                }
            }

            var southWestEdge = new Vector2Int(this._inputManager.ThePlayerControllerInput.CurrentChunk.RelativePosition.X - ViewRange, this._inputManager.ThePlayerControllerInput.CurrentChunk.RelativePosition.Z - ViewRange);
            var northEastEdge = new Vector2Int(this._inputManager.ThePlayerControllerInput.CurrentChunk.RelativePosition.X + ViewRange, this._inputManager.ThePlayerControllerInput.CurrentChunk.RelativePosition.Z + ViewRange);

            BoundingBox = new BoundingBox(
                    new Vector3(southWestEdge.X * Chunk.WidthInBlocks, 0, southWestEdge.Z * Chunk.LengthInBlocks),
                    new Vector3((northEastEdge.X + 1)*Chunk.WidthInBlocks, Chunk.HeightInBlocks,
                                (northEastEdge.Z + 1) * Chunk.LengthInBlocks));
        }

        /// <summary>
        /// Processes chunks in cache range and generates or lightens them.
        /// </summary>
        /// <param name="chunk"><see cref="Chunk"/></param>
        /// <remarks>Note that chunks in cache range only gets generated or lightened. They are built once they get in view-range.</remarks>
        private void ProcessChunkInCacheRange(Chunk chunk)
        {
            if (chunk.ChunkState != ChunkState.AwaitingGenerate && chunk.ChunkState != ChunkState.AwaitingLighting)
                return; // only generate or lighten the chunks.

            // note: we don't care about chunks that await re-lighting because re-lightig only occurs a chunk gets modified.       

            switch (chunk.ChunkState)
            {
                case ChunkState.AwaitingGenerate:
                {
                    Generator.Generate(chunk);
                    break;
                }
                case ChunkState.AwaitingLighting:
                {
                    // Testing out skipping
                    // Lightning.Process(chunk);
                    chunk.ChunkState = ChunkState.AwaitingBuild;
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        private void ProcessChunkInViewRange(Chunk chunk)
        {
            if (chunk.ChunkState == ChunkState.Ready || chunk.ChunkState == ChunkState.AwaitingRemoval)
                return;

            switch (chunk.ChunkState) // switch on the chunk state.
            {
                case ChunkState.AwaitingGenerate:
                {
                    Generator.Generate(chunk);
                    break;
                }
                case ChunkState.AwaitingLighting:
                case ChunkState.AwaitingRelighting:
                {
                    // Testing out skipping
                    // Lightning.Process(chunk);
                    chunk.ChunkState = ChunkState.AwaitingBuild;
                    break;
                }
                case ChunkState.AwaitingBuild:
                case ChunkState.AwaitingRebuild:
                {
                    this.VertexBuilder.Build(chunk);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        #endregion

    }

    public class ChunkCacheException : Exception
    {
        public ChunkCacheException() : base("ViewDirection range can not be larger than cache range!")
        { }
    }
}