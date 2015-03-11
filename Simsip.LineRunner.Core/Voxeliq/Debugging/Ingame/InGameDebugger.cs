/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using Engine.Assets;
using Engine.Chunks;
using Engine.Common.Logging;
using Engine.Graphics;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using BEPUphysicsDemos;
using Engine.Input;


namespace Engine.Debugging.Ingame
{
    public interface IInGameDebuggerService : IUpdateable, IDrawable
    {
        void ToggleInGameDebugger();

        void Draw(Effect effect, EffectType type);
    }

    /// <summary>
    /// Allows drawing ingame debugger visuals.
    /// </summary>
    public interface IInGameDebuggable
    {
        /// <summary>
        /// Draws an ingame debug visual for game component.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to draw.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="spriteBatch">Sprite batch for fonts.</param>
        /// <param name="spriteFont">Font used for drawing strings.</param>
        void DrawInGameDebugVisual(GraphicsDevice graphicsDevice, Camera camera, SpriteBatch spriteBatch,
                                   SpriteFont spriteFont);
    }

    public sealed class InGameDebugger : DrawableGameComponent, IInGameDebuggerService
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private bool _active;

        // Required services.
        private IInputManager _inputManager;
        private IWorld _world;
        private IChunkStorage _chunkStorage;
        private IAssetManager _assetManager;

        /// <summary>
        /// Logging facility.
        /// </summary>
        private static readonly Logger Logger = LogManager.CreateLogger();

        public InGameDebugger(Game game)
            : base(game)
        {
            // Export service.
            game.Services.AddService(typeof (IInGameDebuggerService), this); 
        }

        #region DrawableGameComponent overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Import required service.
            this._inputManager = (IInputManager) this.Game.Services.GetService(typeof (IInputManager));
            this._world = (IWorld) this.Game.Services.GetService(typeof (IWorld));
            this._chunkStorage = (IChunkStorage) this.Game.Services.GetService(typeof (IChunkStorage));
            this._assetManager = (IAssetManager)this.Game.Services.GetService(typeof(IAssetManager));

            this._spriteFont = this._assetManager.GetFont(Asset.VerdanaFont);
            this._spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!this._active)
            {
                return;
            }

            var viewFrustrum = new BoundingFrustum(this._inputManager.LineRunnerCamera.ViewMatrix*this._inputManager.LineRunnerCamera.ProjectionMatrix);

            // TODO: Will this affect our draw state?
            this._spriteBatch.Begin();
            
            //foreach (Chunk chunk in this._chunkStorage.Values)
            //{
            //    if (chunk != this._player.CurrentChunk)
            //        continue;

            //    if (!chunk.BoundingBox.Intersects(viewFrustrum)) 
            //        continue;

            //    chunk.DrawInGameDebugVisual(Game.GraphicsDevice, _camera, _spriteBatch, _spriteFont);
            //}

            this._inputManager.ThePlayerControllerInput.Weapon.DrawInGameDebugVisual(Game.GraphicsDevice, this._inputManager.LineRunnerCamera, _spriteBatch, _spriteFont);

            this._spriteBatch.End();
        }

        #endregion

        #region IInGameDebuggerService implementation

        public void ToggleInGameDebugger()
        {
            this._active = !this._active;
        }

        public void Draw(Effect effect, EffectType type)
        {

        }

        #endregion
    }
}