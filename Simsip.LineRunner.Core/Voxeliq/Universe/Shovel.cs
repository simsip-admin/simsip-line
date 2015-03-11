/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using BEPUphysicsDemos;
using Engine.Blocks;
using Engine.Chunks;
using Engine.Common.Vector;
using Engine.Graphics;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Universe
{
    public class Shovel : Weapon
    {
        // required services.
        private IInputManager _inputManager;
        private IWorld _world;
        private IChunkCache _chunkCache;
        private IBlockStorage _blockStorage;

        public Shovel(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            this._inputManager = (IInputManager) this.Game.Services.GetService(typeof (IInputManager));
            this._world = (IWorld) this.Game.Services.GetService(typeof (IWorld));
            this._chunkCache = (IChunkCache) this.Game.Services.GetService(typeof (IChunkCache));
            this._blockStorage = (IBlockStorage) this.Game.Services.GetService(typeof (IBlockStorage));
        }

        public override void Use()
        {
            if (!_inputManager.ThePlayerControllerInput.AimedSolidBlock.HasValue) 
                return;

            this._blockStorage.SetBlockAt(_inputManager.ThePlayerControllerInput.AimedSolidBlock.Value.Position,Block.Empty);
        }

        public override void SecondaryUse()
        {
            // Test for not pushing the player into walls.. this really should be handled differently /fasbat
            if (!_inputManager.ThePlayerControllerInput.AimedEmptyBlock.HasValue || _inputManager.ThePlayerControllerInput.AimedEmptyBlock.Value.Position == new Vector3Int(_inputManager.PlayerCamera.Position + new Vector3(0f, -0.5f, 0f)))
                return;

            this._blockStorage.SetBlockAt(_inputManager.ThePlayerControllerInput.AimedEmptyBlock.Value.Position, new Block(BlockType.Iron));
        }

        /// <summary>
        /// Draws an ingame debug visual on the block that is targeted.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="camera"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="spriteFont"></param>
        public virtual void DrawInGameDebugVisual(GraphicsDevice graphicsDevice, Camera camera, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            if (!_inputManager.ThePlayerControllerInput.AimedSolidBlock.HasValue) // make sure we have a solid block.
                return;

            var positionedBlock = _inputManager.ThePlayerControllerInput.AimedSolidBlock.Value;
            var hostChunk = this._chunkCache.GetChunkByWorldPosition(positionedBlock.Position.X, positionedBlock.Position.Z);


            var text = string.Format("Block: {0}, Pos: {1}, Chunk: {2}", positionedBlock.Block.ToString(), positionedBlock.Position, hostChunk.ToString());
            

            Vector3 projected = graphicsDevice.Viewport.Project(Vector3.Zero, camera.ProjectionMatrix, camera.ViewMatrix,
                                                                Matrix.CreateTranslation(new Vector3(positionedBlock.Position.X + 0.5f, positionedBlock.Position.Y + 0.5f, positionedBlock.Position.Z + 0.5f)));

            var textSize = spriteFont.MeasureString(text);
            spriteBatch.DrawString(spriteFont, text, new Vector2(projected.X - textSize.X/2, projected.Y - textSize.Y/2), Color.Yellow);
        }
    }
}