/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using Engine.Input;
using Cocos2D;
using Engine.Blocks;
using Engine.Chunks;
using Engine.Common.Logging;
using Engine.Common.Vector;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.IO;
#if WINDOWS_PHONE ||NETFX_CORE
using Windows.System.Threading;
using System.Threading.Tasks;
#endif

namespace Engine.Universe
{
    /// <summary>
    /// World service interface.
    /// </summary>
    public interface IWorld : IUpdateable
    {
        void SpawnWorld(Vector2Int relativePosition);

        bool SaveInProgress { get; }

#if ANDROID || IOS || DESKTOP
        string New(string worldName);
#elif WINDOWS_PHONE || NETFX_CORE
        Task<string> NewAsync(string worldName);
#endif

#if NETFX_CORE
        void Save(ThreadPoolTimer timer=null);
#else
        void Save(object state=null);
#endif

        void SwitchState(GameState gameState);
    }

    /// <summary>
    /// World.
    /// </summary>
    public class World : GameComponent, IWorld
    {
        // Required services.
        private ChunkCache _chunkCache;
        private IInputManager _inputManager;
        private IBlockStorage _blockStorage;
        private IDeferredShadowMapping _deferredShadowMapping;

        // Atmospheric settings
        private DirectedLight _sunLight;
        private DirectedLight _moonLight;
        private PointLight _torchLight1;
        private PointLight _torchLight2;
        private SpotLight _spotLight1;
        private SpotLight _spotLight2;

        public static Vector4 NightColor = Color.Black.ToVector4();
        public static Vector4 SunColor = Color.White.ToVector4();
        public static Vector4 HorizonColor = Color.DarkGray.ToVector4();

        public static Vector4 EveningTint = Color.Red.ToVector4();
        public static Vector4 MorningTint = Color.Gold.ToVector4();

        // Saving support
        private GameState _currentGameState;
#if NETFX_CORE
        private ThreadPoolTimer _saveTimer;
#else
        private Timer _saveTimer;
#endif

        private IList<Chunk> _saveChunks;
        private Block[] _saveBlocks;

        // Logging
        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility.

        /// <summary>
        /// Creates a new world.
        /// </summary>
        /// <param name="game"> </param>
        /// <param name="chunkStorage"> </param>
        /// <param name="chunkCache"> </param>
        public World(Game game, ChunkStorage chunkStorage, ChunkCache chunkCache)
            : base(game)
        {
            this.Chunks = chunkStorage;
            this._chunkCache = chunkCache;

            // export services.
            game.Services.AddService(typeof (IWorld), this);
        }

        #region Properties

        private volatile bool _saveInProgress;
        public bool SaveInProgress 
        {
            get
            {
                return this._saveInProgress;
            }
            private set
            {
                this._saveInProgress = value;
            }
        }

        public ChunkStorage Chunks { get; set; }

        #endregion

        #region GameComponent overrides

        public void Initialize()
        {
            Logger.Trace("init()");

            // import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof (IInputManager));
            this._blockStorage = (IBlockStorage)this.Game.Services.GetService(typeof(IBlockStorage));
            this._deferredShadowMapping = (IDeferredShadowMapping)this.Game.Services.GetService(typeof(IDeferredShadowMapping));

            // TODO: What is this for?
            this._inputManager.LineRunnerCamera.Target = Vector3.Down;

            // Will result in calling back to World.SpawnWorld (see below in Api)
            this._inputManager.ThePlayerControllerInput.SpawnPlayerCamera(new Vector2Int(0, 0));

            // Setup lights
            this._sunLight = new DirectedLight()
                {
                    Position = new Vector3(-150f,150f,150f),
                    Direction = Vector3.Normalize(new Vector3(1f, -1f, -1f)),
                    DiffuseColor = new Vector3(1f, 1f, 1f),
                    SpecularColor = new Vector3(1f, 1f, 1f),
                    SpecularPower = 1f
                };
            this._sunLight.ViewMatrix = Matrix.CreateLookAt(
                this._sunLight.Position, 
                this._sunLight.Position + this._sunLight.Direction,
                Vector3.Up);
            this._sunLight.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.Pi / 3.0f,
                Game.GraphicsDevice.Viewport.AspectRatio,
                0.01f,
                1500f);

            this._moonLight = new DirectedLight()
            {
                Position = new Vector3(10f, -10f, -10f),
                Direction = Vector3.Normalize(new Vector3(-1f, 1f, 1f)),
                DiffuseColor = new Vector3(1f, 1f, 1f),
                SpecularColor = new Vector3(1f, 1f, 1f),
                SpecularPower = 1f
            };
            this._moonLight.ViewMatrix = Matrix.CreateLookAt(
                this._moonLight.Position,
                this._moonLight.Position + this._moonLight.Direction,
                Vector3.Up);
            this._moonLight.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.Pi / 3.0f,
                Game.GraphicsDevice.Viewport.AspectRatio,
                0.01f,
                1500f);

            this._torchLight1 = new PointLight();
            this._torchLight2 = new PointLight();
            this._spotLight1 = new SpotLight();
            this._spotLight2 = new SpotLight();

            // Start with morning sunlight
            this._deferredShadowMapping.AddLight(this._sunLight);
        }

        public void Update(GameTime gameTime)
        {
            // TODO: Adjust sun/moon light and ambient light
            // based on time of day
            // TODO: Add in point lights (flames) and spotlights
            // at night
            this._sunLight.Direction = Vector3.Normalize(
                this._inputManager.CurrentCamera.Position - this._sunLight.Position);
            this._sunLight.ViewMatrix = Matrix.CreateLookAt(
                this._sunLight.Position,
                this._sunLight.Position + this._sunLight.Direction,
                Vector3.Up);
            var timeOfDay = Time.GetGameTimeOfDay();
            // this._deferredShadowMapping.TheAmbientLight = new Vector3(?);
            // this._sunLight.Direction = Vector3.Normalize(new Vector3(?));
        }

        #endregion

        #region IWorld implementation

#if ANDROID || IOS || DESKTOP
        public void SpawnWorld(Vector2Int relativePosition)
#elif WINDOWS_PHONE || NETFX_CORE
        public async void SpawnWorld(Vector2Int relativePosition)
#endif
        {
            this.Chunks.Clear();
            this._blockStorage.Clear();

            // TODO: Change to "default"
            var currentWorld = UserDefaults.SharedUserDefault.GetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_WORLD,
                GameConstants.WORLD_GENERATE_NEW);
            if (currentWorld == GameConstants.WORLD_GENERATE_NEW)
            {
                for (int z = -ChunkCache.CacheRange; z <= ChunkCache.CacheRange; z++)
                {
                    for (int x = -ChunkCache.CacheRange; x <= ChunkCache.CacheRange; x++)
                    {
                        var chunk = new Chunk(new Vector2Int(relativePosition.X + x, relativePosition.Z + z));
                        this.Chunks[chunk.RelativePosition.X, chunk.RelativePosition.Z] = chunk;

                        if (chunk.RelativePosition == relativePosition)
                        {
                            this._inputManager.ThePlayerControllerInput.CurrentChunk = chunk;
                        }
                    }
                }
            }
            else
            {
                // Make sure bad things from a load don't make the app crash
                try
                {
                    var currentWorldFolderPath = Path.Combine(GameConstants.FOLDER_SAVES, currentWorld);
                    var currentWorldFilePath = Path.Combine(currentWorldFolderPath, currentWorld);
#if ANDROID || IOS || DESKTOP
                    var stringWorld = FileUtils.LoadText(currentWorldFilePath);
#elif WINDOWS_PHONE || NETFX_CORE
                    // TODO: Race condition
                    var stringWorld = await FileUtils.LoadTextAsync(currentWorldFilePath);
#endif
                    var jsonWorld = JObject.Parse(stringWorld);

                    // Loop over chunks
                    int chunkCount = jsonWorld["world"]["chunks"].Count();
                    for (int i = 0; i < chunkCount; i++)
                    {
                        // Make sure one bad chunk doesn't spoil it for everyone else
                        try
                        {
                            JObject jsonChunk = (JObject)jsonWorld["world"]["chunks"][i];
                            var relativePositionParsed = new Vector2Int(
                                (int)jsonChunk["relative_position_x"],
                                (int)jsonChunk["relative_position_z"]);

                            var chunk = new Chunk(relativePositionParsed);
                            this.Chunks[chunk.RelativePosition.X, chunk.RelativePosition.Z] = chunk;

                            if (chunk.RelativePosition == relativePosition)
                            {
                                this._inputManager.ThePlayerControllerInput.CurrentChunk = chunk;
                            }

                            // Set chunk state to generating.
                            chunk.ChunkState = ChunkState.Generating;

                            int blockCount = jsonChunk["blocks"].Count();
                            for (int y = 0; y < blockCount; y++)
                            {
                                JObject jsonBlock = (JObject)jsonChunk["blocks"][y];

                                // Type
                                // TODO: Use dirt if not there
                                // TODO: Does json.net parse out bytes?
                                var blockTypeParsed = (int)jsonBlock["type"];
                                var blockType = (BlockType)blockTypeParsed;

                                // BlockStorage offset
                                var blockStorageOffset = (int)jsonBlock["offset"];

                                BlockStorage.Blocks[blockStorageOffset] = new Block(blockType);
                            }

                            // Chunk should be lighten now.            
                            chunk.ChunkState = ChunkState.AwaitingLighting;
                        }
                        catch (Exception exInner)
                        {
                            // We have a bad chunk, just note it and continue with the rest of the chunks
                            Debug.WriteLine("Error parsing chunk: " + exInner.ToString());
                        }
                    }
                }
                catch (Exception exOuter)
                {
                    Debug.WriteLine("Exception parsing chunks: " + exOuter);
                }
            }

            this.Chunks.SouthWestEdge = new Vector2Int(relativePosition.X - ChunkCache.ViewRange,
                                                       relativePosition.Z - ChunkCache.ViewRange);
            this.Chunks.NorthEastEdge = new Vector2Int(relativePosition.X + ChunkCache.ViewRange,
                                                       relativePosition.Z + ChunkCache.ViewRange);

            ChunkCache.BoundingBox =
                new BoundingBox(
                    new Vector3(this.Chunks.SouthWestEdge.X * Chunk.WidthInBlocks, 0, this.Chunks.SouthWestEdge.Z * Chunk.LengthInBlocks),
                    new Vector3((this.Chunks.NorthEastEdge.X + 1) * Chunk.WidthInBlocks, Chunk.HeightInBlocks, (this.Chunks.NorthEastEdge.Z + 1) * Chunk.LengthInBlocks));
        }


#if ANDROID || IOS || DESKTOP
        public string New(string worldName)
#elif WINDOWS_PHONE || NETFX_CORE
        public async Task<string> NewAsync(string worldName)
#endif
        {
            var successStr = string.Empty;

            // If we can't grab our save lock, fail with error message
            if (this.SaveInProgress)
            {
                return "Unable to save new world now, try again later.";
            }

            // Grab our save lock
            this.SaveInProgress = true;

            // Create copies of the current set of chunks/blocks
            this._saveChunks = this.Chunks.Values.ToList();
            var blockCount = BlockStorage.Blocks.Count();
            this._saveBlocks = new Block[blockCount];
            Array.Copy(BlockStorage.Blocks, 0, this._saveBlocks, 0, blockCount);

            // Create the new world
            // IMPORTANT: Will blindly delete any file with this world name there,
            // so provide checks for file existence in calling function if necessary
#if ANDROID || IOS || DESKTOP
            var errorStr = this.SaveImpl(worldName);
#elif WINDOWS_PHONE || NETFX_CORE
            var errorStr = await this.SaveImpl(worldName);
#endif

            // Clear our chunk/block copies
            this._saveChunks = null;
            this._saveBlocks = null;

            // Release our save lock
            this.SaveInProgress = false;

            // If we had an error, return its error message
            if (errorStr != string.Empty)
            {
                return errorStr;
            }

            // Otherwise, signal success
            return successStr;
        }

#if ANDROID || IOS || DESKTOP
        public void Save(object state=null)
#elif WINDOWS_PHONE
        public async void Save(object state=null)
#elif NETFX_CORE
        public async void Save(ThreadPoolTimer timer=null)
#endif
        {
            // If we already have another save in progress, just quietly fail
            if (this.SaveInProgress)
            {
                return;
            }

            // Grab our save lock
            this.SaveInProgress = true;

            // Create copies of the current set of chunks/blocks
            this._saveChunks = this.Chunks.Values.ToList();
            var blockCount = BlockStorage.Blocks.Count();
            this._saveBlocks = new Block[blockCount];
            Array.Copy(BlockStorage.Blocks, 0, this._saveBlocks, 0, blockCount);

            // What world are we saving?
            var worldName = CCUserDefault.SharedUserDefault.GetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_WORLD,
                GameConstants.WORLD_DEFAULT);

            // Go for the save
#if ANDROID || IOS || DESKTOP
            this.SaveImpl(worldName);
#elif WINDOWS_PHONE || NETFX_CORE
            await this.SaveImpl(worldName);
#endif

            // Clear our chunk/block copies
            this._saveChunks = null;
            this._saveBlocks = null;

            // Release our save lock
            this.SaveInProgress = false;
        }

        public void SwitchState(GameState gameState)
        {
            switch(gameState)
            {
                case GameState.World:
                    {
#if NETFX_CORE
                        // Stop any previous timer
                        if (this._saveTimer != null)
                        {
                            this._saveTimer.Cancel();
                        }

                        // Get our periodic save going every 15 seconds
                        this._saveTimer = ThreadPoolTimer.CreatePeriodicTimer(
                            this.Save,
                            TimeSpan.FromSeconds(15));

#else
                        // Stop any previous timer
                        if (this._saveTimer != null)
                        {
                            this._saveTimer.Dispose();
                        }

                       // Get our periodic save going every 15 seconds
                        this._saveTimer = new Timer(this.Save,                               // Kicks off a save
                                          null,                                              // No state needed
                                          (int)TimeSpan.FromSeconds(15).TotalMilliseconds,   // First save comes after polling period in seconds
                                          (int)TimeSpan.FromSeconds(15).TotalMilliseconds);  // Then perform a save every polling period in seconds after

#endif

                        break;
                    }
                default:
                    {
#if NETFX_CORE
                        // Stop any previous timer
                        if (_saveTimer != null)
                        {
                            _saveTimer.Cancel();
                        }
#else
                        // Stop any previous timer
                        if (_saveTimer != null)
                        {
                            _saveTimer.Dispose();
                        }
#endif
                        break;
                    }
            }

            // Save state
            this._currentGameState = gameState;
        }

        #endregion

        #region Api

        #endregion

        #region Helper methods

#if ANDROID || IOS || DESKTOP
        private string SaveImpl(string worldName)
#elif WINDOWS_PHONE || NETFX_CORE
        private async Task<string> SaveImpl(string worldName)
#endif
        {
            var errorString = string.Empty;
            
            if (worldName == GameConstants.WORLD_DEFAULT)
            {
                return "Unable to create world with name 'default'.";
            }

            var jsonWorld = new JObject();

            // TODO: Create world values (world name, etc.)
            jsonWorld.Add("version", 1);

            var jsonChunks = new JArray();
            foreach (var chunk in this.Chunks.Values)
            {
                var jsonChunk = new JObject();
                var relativePosition = chunk.RelativePosition;
                jsonChunk.Add("relative_position_x", relativePosition.X);
                jsonChunk.Add("relative_position_z", relativePosition.Z);

                var jsonBlocks = new JArray();
                for (byte x = 0; x < Chunk.WidthInBlocks; x++)
                {
                    var worldPositionX = chunk.WorldPosition.X + x;

                    for (byte z = 0; z < Chunk.LengthInBlocks; z++)
                    {
                        int worldPositionZ = chunk.WorldPosition.Z + z;

                        for (byte y = 0; y < Chunk.HeightInBlocks; z++)
                        {
                            // TODO: Confirm this is in right order
                            var block = this._blockStorage.BlockAt(x, y, z);

                            var jsonBlock = new JObject();
                            jsonBlock.Add("type", (int)block.Type);
                            jsonBlock.Add("offset", BlockStorage.BlockIndexByWorldPosition(x, y, z));

                            jsonBlocks.Add(jsonBlock);
                        }
                    }
                }
                jsonChunk.Add("blocks", jsonBlocks);

                jsonChunks.Add(jsonChunk);
            }
            jsonWorld.Add("chunks", jsonChunks);

            var jsonRoot = new JObject();
            jsonRoot.Add("world", jsonWorld);

            var stringRoot = jsonRoot.ToString();
            if (errorString != string.Empty)
            {
                return errorString;
            }

            var worldFolderPath = Path.Combine(GameConstants.FOLDER_SAVES, worldName);
            var worldFilePath = Path.Combine(worldFolderPath, worldName);
#if ANDROID || IOS || DESKTOP
            if (FileUtils.FolderExists(worldFolderPath))
            {
                FileUtils.DeleteFolder(worldFolderPath);
            }

            FileUtils.SaveText(worldFilePath, stringRoot);
#elif WINDOWS_PHONE || NETFX_CORE
            if (await FileUtils.FolderExistsAsync(worldFolderPath))
            {
                await FileUtils.DeleteFolderAsync(worldFolderPath);
            }

            await FileUtils.SaveTextAsync(worldFilePath, stringRoot);
#endif
            return errorString;
        }

        #endregion
    }
}