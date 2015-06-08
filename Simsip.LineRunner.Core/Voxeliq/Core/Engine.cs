/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using Engine.Assets;
using Engine.Audio;
using Engine.Blocks;
using Engine.Chunks;
using Engine.Chunks.Processors;
using Engine.Core.Config;
using Engine.Debugging;
using Engine.Debugging.Console;
using Engine.Debugging.Graphs;
using Engine.Debugging.Ingame;
using Engine.Graphics;
using Engine.Input;
using Engine.Interface;
using Engine.Sky;
using Engine.Universe;
using Engine.Water;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner;

namespace Engine.Core
{
    public class Engine
    {
        /// <summary>
        /// The engine configuration.
        /// </summary>
        public EngineConfig Configuration { get; private set; }

        /// <summary>
        /// Attached game.
        /// </summary>
        public Game Game { get; private set; }

        public delegate void EngineStartHandler(object sender, EventArgs e);
        public event EngineStartHandler EngineStart;

        public GameConsole Console { get; private set; }

        public Rasterizer Rasterizer { get; private set; }

        public InputManager InputManagerComponent { get; private set; }
        public AssetManager AssetManagerComponent { get; private set; }
        public SkyDome SkyDomeComponent { get; private set; }
        public NewSky NewSkyComponent { get; private set; }
        public Fogger FoggerComponent { get; private set; }
        public ChunkStorage ChunkStorageComponent { get; private set; }
        public VertexBuilder VertexBuilderComponent { get; private set; }
        public ChunkCache ChunkCacheComponent { get; private set; }
        public BlockStorage BlockStorageComponent { get; private set; }
        public World WorldComponent { get; private set; }
        public WaterCache WaterCacheComponent { get; private set; }
        public UserInterface UserInterfaceComponent { get; private set; }
        public InGameDebugger InGameDebuggerComponent { get; private set; }
        public DebugBar DebugBarComponent { get; private set; }
        public GraphManager GraphManagerComponent { get; private set; }
        public AudioManager AudioManagerComponent { get; private set; }

        public Engine(Game game, EngineConfig config)
        {
            if (_instance != null)
                throw new Exception("You can not instantiate the Engine more than once.");

            _instance = this;

            this.Game = game;
            this.Configuration = config;

            config.Validate(); // validate the config.
        }

        public void Run()
        {
            this.AddComponents();

            this.TheCustomContentManager = new CustomContentManager(
                   TheGame.SharedGame.Services,
                   TheGame.SharedGame.Content.RootDirectory);

            this.NotifyEngineStart(EventArgs.Empty);
        }

        private void NotifyEngineStart(EventArgs e)
        {
            var handler = EngineStart;
            if (handler != null) 
                handler(typeof(Engine), e);
        }

        /// <summary>
        /// Adds game-components.
        /// 
        /// Also central location to enable/disable game components. Helpful
        /// when adding in new components/effects.
        /// </summary>
        private void AddComponents()
        {
            this.Rasterizer = new Rasterizer();

            InputManagerComponent = new InputManager(this.Game);
            InputManagerComponent.Enabled = false;
            // this.Game.Components.Add(inputManager);

            AssetManagerComponent = new AssetManager(this.Game);
            AssetManagerComponent.Enabled = false;
            // this.Game.Components.Add(AssetManagerComponent);

            SkyDomeComponent = new SkyDome(this.Game);
            SkyDomeComponent.Enabled = false;
            // this.Game.Components.Add(NewSkyComponent);
            
            NewSkyComponent = new NewSky(this.Game);
            NewSkyComponent.Enabled = false;
            // this.Game.Components.Add(NewSkyComponent);

            FoggerComponent = new Fogger(this.Game);
            FoggerComponent.Enabled = false;
            // this.Game.Components.Add(FoggerComponent);

            ChunkStorageComponent = new ChunkStorage(this.Game);
            ChunkStorageComponent.Enabled = false;
            // this.Game.Components.Add(ChunkStorageComponent);

            VertexBuilderComponent = new VertexBuilder(this.Game);
            VertexBuilderComponent.Enabled = false;
            // this.Game.Components.Add(VertexBuilderComponent);

            ChunkCacheComponent = new ChunkCache(this.Game);
            ChunkCacheComponent.Enabled = false;
            // this.Game.Components.Add(ChunkCacheComponent);

            BlockStorageComponent = new BlockStorage(this.Game);
            BlockStorageComponent.Enabled = false;
            // this.Game.Components.Add(BlockStorageComponent);

            WorldComponent = new World(this.Game, ChunkStorageComponent, ChunkCacheComponent);
            WorldComponent.Enabled = false;
            // this.Game.Components.Add(WorldComponent);

            WaterCacheComponent = new WaterCache(this.Game);
            WaterCacheComponent.Enabled = false;
            // this.Game.Components.Add(CameraComponent);

            UserInterfaceComponent = new UserInterface(this.Game);
            UserInterfaceComponent.Enabled = true;
            // this.Game.Components.Add(UserInterfaceComponent);

            InGameDebuggerComponent = new InGameDebugger(this.Game);
            InGameDebuggerComponent.Enabled = true;
            // this.Game.Components.Add(InGameDebuggerComponent);

            DebugBarComponent = new DebugBar(this.Game);
            DebugBarComponent.Enabled = true;
            // this.Game.Components.Add(DebugBarComponent);

            GraphManagerComponent = new GraphManager(this.Game);
            GraphManagerComponent.Enabled = true;
            // this.Game.Components.Add(GraphManagerComponent);

            AudioManagerComponent = new AudioManager(this.Game);
            AudioManagerComponent.Enabled = false;
            // this.Game.Components.Add(new AudioManager(this.Game));
            

            var  spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
            /*
            Console = new GameConsole(this.Game, spriteBatch,  new GameConsoleOptions
                                                             {
                                                                 Font = Game.Content.Load<SpriteFont>(@"Fonts/Verdana"),
                                                                 FontColor = Color.LawnGreen,
                                                                 Prompt = ">",
                                                                 PromptColor = Color.Crimson,
                                                                 CursorColor = Color.OrangeRed,
                                                                 BackgroundColor = Color.Black*0.8f,
                                                                 PastCommandOutputColor = Color.Aqua,
                                                                 BufferColor = Color.Gold
                                                             });
            */
        }

        private static Engine _instance; // the memory instance.

        /// <summary>
        /// Returns the memory instance of Engine.
        /// </summary>
        public static Engine Instance
        {
            get { return _instance; }
        }

        public CustomContentManager TheCustomContentManager { get; private set; }

        #region de-ctor

        // IDisposable pattern: http://msdn.microsoft.com/en-us/library/fs2xkftw(v=VS.100).aspx

        /// <summary>
        /// Is the engine instance disposed already?
        /// </summary>
        public bool Disposed = false;

        private void Dispose(bool disposing)
        {
            if (this.Disposed) 
                return; // if already disposed, just return

            if (disposing) // only dispose managed resources if we're called from directly or in-directly from user code.
            {
                _instance = null;
            }

            Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true); // Object being disposed by the code itself, dispose both managed and unmanaged objects.
            GC.SuppressFinalize(this); // Take object out the finalization queue to prevent finalization code for it from executing a second time.
        }

        ~Engine() { Dispose(false); } // finalizer called by the runtime. we should only dispose unmanaged objects and should NOT reference managed ones. 

        #endregion
    }
}
