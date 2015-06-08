/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using Engine.Assets;
using Engine.Blocks;
using Engine.Chunks;
using Engine.Common.Logging;
using Engine.Common.Vector;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner;
using Simsip.LineRunner.GameFramework;
using BEPUphysics;
using BEPUphysicsDemos;
using BEPUphysicsDemos.AlternateMovement;
using Engine.Input;
using System.Diagnostics;


namespace Engine.Graphics
{
    public class PlayerControllerInput : FirstPersonControllerInput
    {
        private bool _touchDownExists;
        private bool _touchDownHandled;

        /// <summary>
        /// Used to signal to everyone else that we have touched down
        /// and positioned ourselves in the world for proper rendering.
        /// </summary>
        public bool Ready { get; private set; }
        public bool FlyingEnabled { get; set; }
        public Vector2Int RelativePosition { get; private set; }

        public Chunk CurrentChunk { get; set; }
        public Chunk LastChunk { get; set; }
        public Weapon Weapon { get; set; }
        public Vector3 LookVector { get; set; }
        public PositionedBlock? AimedSolidBlock { get; private set; } // nullable object.        
        public PositionedBlock? AimedEmptyBlock { get; private set; } // nullable object.        

        public Vector3 Velocity;

        private BasicEffect _aimedBlockEffect;
        private Model _aimedBlockModel;
        private Texture2D _aimedBlockTexture;

        private Model _sampleModel;

        private const float MoveSpeed = 5f; // the move speed.
        private const float FlySpeed = 25f; // the fly speed.
        private const float Gravity = -15f;
        private const float JumpVelocity = 6f;

        // required services.
        private IChunkCache _chunkCache;
        private IAssetManager _assetManager;
        private IBlockStorage _blockStorage;
        private IPageCache _pageCache;
        private IInputManager _inputManager;
        private IWorld _world;

        // misc
        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility

        private const int _initialHeight = 150;

        public PlayerControllerInput(Space owningSpace, Camera camera, Game game)
            : base(owningSpace, camera, game)
        {
            Initialize();
        }

        private void Initialize()
        {
            Logger.Trace("init()");

            // this.FlyingEnabled = true;
            this.FlyingEnabled = false;
            this.Weapon = new Shovel(TheGame.SharedGame);

            // import required services.
            this._chunkCache = (IChunkCache)TheGame.SharedGame.Services.GetService(typeof(IChunkCache));
            this._blockStorage = (IBlockStorage)TheGame.SharedGame.Services.GetService(typeof(IBlockStorage));
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            this._assetManager = (IAssetManager)TheGame.SharedGame.Services.GetService(typeof(IAssetManager));
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
            this._world = (IWorld)TheGame.SharedGame.Services.GetService(typeof(IWorld));

            this.LoadContent();

            this.Weapon.Initialize();
        }

        protected void LoadContent()
        {
            // TODO: This is a .x model that is not compiling with the new Pipeline tool
            // this._aimedBlockModel = this._assetManager.GetModel(Asset.AimedBlockModel, ModelType.Voxeliq);
            this._aimedBlockEffect = new BasicEffect(TheGame.SharedGame.GraphicsDevice);
            this._aimedBlockTexture = this._assetManager.GetTexture(
                Asset.AimedBlockTexture,
                Engine.Core.Engine.Instance.TheCustomContentManager);
        }

        public void Update(GameTime gameTime)
        {
            // We analyze our initial condition based on existing blocks
            if (!this._chunkCache.Ready)
            {
                return;
            }

            this.ProcessPosition(gameTime);

            // TODO: Come back to this for aimed block needs
            // this.ProcessView();

            // TODO: Leaving in for reference to previous code
            /*
            this.Target = this.Position + Vector3.Forward;
            var upVector = Vector3.Up;
            this.View = Matrix.CreateLookAt(this.Position, this.Target, upVector);
            */
        }


        // TODO: Leaving in for reference
        /*
        public void PositionPlayerCamera(Vector3 position, Vector3 target)
        {
            this.Position = position;

            // Transform camera position based on rotation and elevation.
            // IMPORTANT: Passing in Vector3.Zero for target allows us to specify
            // a default target.
            if (target == Vector3.Zero)
            {
                this.Target = this.Position + Vector3.Forward;
            }
            else
            {
                this.Target = target;
            }
            var upVector = Vector3.Up;
            this.View = Matrix.CreateLookAt(this.Position, this.Target, upVector);
        }
        */

        private void ProcessPosition(GameTime gameTime)
        {
            if (FlyingEnabled)
            {
                return;
            }

            // Only process position up till player touches down on solid block for first time
            if (!_touchDownExists)
            {
                // When we have our origin chunk in place, loop down to first solid block
                // and place camera there.
                if ( (this.CurrentChunk != null) &&
                     (this.CurrentChunk.ChunkState != ChunkState.AwaitingGenerate ||
                      this.CurrentChunk.ChunkState != ChunkState.Generating) )
                {
                    for (var y = 1; y < Chunk.HeightInBlocks; y++)
                    {
                        var footPosition = TheCamera.Position + new Vector3(0f, -1f * y, 0f);
                        var standingBlock = this._blockStorage.BlockAt(footPosition);

                        if (standingBlock.Exists)
                        {
                            this._touchDownExists = true;
                            TheCamera.Position = footPosition;
                            break;
                        }
                    }
                }
            }

            // Given this initial touch-down position, can we position our pad at a desired
            // depth without it being obstructed by a block(s)?
            if (this._touchDownExists)
            {
                if (!this._touchDownHandled)
                {
                    // Do a first time adjustment to our position
                    this._inputManager.TheStationaryCamera.Position = this.TheCamera.Position;
                    this._inputManager.TheStationaryCamera.Target = this.TheCamera.Position + Vector3.Forward;
                    this._inputManager.LineRunnerCamera.Position = this.TheCamera.Position;
                    this._inputManager.LineRunnerCamera.Target = this.TheCamera.Position + Vector3.Forward;

                    // Now update pad accordingly
                    this._pageCache.CalculateWorldCoordinates();

                    // Clear out all blocks in front and sides
                    this.ClearOutBlocks();

                    // Flag so we only do this logic once
                    this._touchDownHandled = true;

                    // Signal to everyone else we are ready
                    this.Ready = true;
#if STOPWATCH
                    Program.TheStopwatch.Stop();
                    Debug.WriteLine("PlayerControllerInput.Ready: " + Program.TheStopwatch.ElapsedMilliseconds);
                    Program.TheStopwatch.Restart();
#endif

                    /* Moving this to RepositionPad so we can call RepositionPad from refreshes
                    // Do a first time adjustment to our position
                    this._stationaryCamera.PositionStationaryCamera(this.Position, Vector3.Zero);
                    this._trackingCamera.PositionTrackingCamera(this.Position, Vector3.Zero);

                    // Now update pad accordingly
                    this._pageCache.CalculateWorldCoordinates();
                    */

                    // TODO:
                    // Come back to this with a destructive block algorithm to make sure pad is visible
                    /*
                    // Loop over grid to pad looking back from camera to find highest block
                    // in grid
                    var pageWidth = (int)this._pageCache.CurrentPageModel.WorldWidth;
                    var pageLeft = (int)this._pageCache.CurrentPageModel.WorldOrigin.X;
                    var pageRight = pageLeft + pageWidth;
                    var pageFront = (int)this.Position.Z;
                    var pageBack = (int)(this._pageCache.CurrentPageModel.WorldOrigin.Z -
                                         this._pageCache.CurrentPageModel.WorldDepth);

                    var highestBlockInGrid = (int)this.Position.Y;
                    for (var i = pageLeft; i <= pageRight; i++)
                    {
                        for (var j = pageBack; j <= pageFront; j++)
                        {
                            var highestBlockInCell = highestBlockInGrid;

                            while( _blockStorage.BlockAt(i, highestBlockInCell, j).Exists)
                            {
                                highestBlockInCell++;
                            }

                            if (highestBlockInCell > highestBlockInGrid)
                            {
                                highestBlockInGrid = highestBlockInCell;
                            }
                        }
                    }
            
                    // Adjust our highest block reference if higher than one we are standing on
                    if (highestBlockInGrid > (int)this.Position.Y)
                    {
                        var highestBlockAdjustment = highestBlockInGrid - (int)this.Position.Y;
                        this.Position += new Vector3(0, highestBlockAdjustment, 0);
                        this._stationaryCamera.PositionStationaryCamera(this.Position, Vector3.Zero);
                        this._trackingCamera.PositionTrackingCamera(this.Position, Vector3.Zero);
                        this._pageCache.CalculateWorldCoordinates();
                    }

                    // Determine amount we need to adjust camera so origin y of pad (bottom of pad)
                    // will rest on top of highest block (or at least not be obustructed by it
                    var padAdjustment = this.Position.Y - this._pageCache.CurrentPageModel.WorldOrigin.Y;
                    
                    // Adjust camera and pad by amount needed
                    this.Position += new Vector3(0, padAdjustment, 0);
                    this._stationaryCamera.PositionStationaryCamera(this.Position, Vector3.Zero);
                    this._trackingCamera.PositionTrackingCamera(this.Position, Vector3.Zero);
                    this._pageCache.CalculateWorldCoordinates();
                    */
                }
            }
        }

        // Clear out blocks around pad
        private void ClearOutBlocks()
        {
            var pageWidth = (int)this._pageCache.CurrentPageModel.WorldWidth;
            var pageHeight = (int)this._pageCache.CurrentPageModel.WorldHeight;
            var pageOriginX = (int)this._pageCache.CurrentPageModel.WorldOrigin.X;
            var pageOriginY = (int)this._pageCache.CurrentPageModel.WorldOrigin.Y;
            var pageOriginZ = (int)this._pageCache.CurrentPageModel.WorldOrigin.Z;

            var clearXStart = pageOriginX - 10;
            var clearXStop = clearXStart + 
                             pageWidth + 
                             10;
            
            // IMPORTANT: We currently only have one layer high of chunks, hence we limit our height to chunk height
            var clearYStart = pageOriginY;
            var clearYStop = Chunk.HeightInBlocks - 1;
            var clearZStart = pageOriginZ - 4;
            var clearZStop = clearZStart + 
                             10;
            for (var x = clearXStart; x <= clearXStop; x++)
            {
                for (var y = clearYStart; y <= clearYStop; y++)
                {
                    for (var z = clearZStart; z <= clearZStop; z++)
                    {
                        this._blockStorage.SetBlockAt(x, y, z, Block.Empty);
                    }
                }
            }
        }

        public void RepositionPad()
        {
            // Will re-position pad in update cycle
            this._touchDownExists = false;
            this._touchDownHandled = false;
        }

        public void RefreshPad()
        {
            // Reposition cameras to original positions/targets from previous touchdown
            this._inputManager.TheStationaryCamera.Position = this._pageCache.StationaryCameraOriginalWorldPosition;
            this._inputManager.TheStationaryCamera.Target = this._pageCache.StationaryCameraOriginalWorldTarget;
            this._inputManager.LineRunnerCamera.Position = this._pageCache.StationaryCameraOriginalWorldPosition;
            this._inputManager.LineRunnerCamera.Target = this._pageCache.StationaryCameraOriginalWorldTarget;
            
            // Now update pad accordingly
            this._pageCache.CalculateWorldCoordinates();
        }

        private bool IsSolidCubeBlockingView()
        {
            // Get dimensions of our pad at desired depth
            this._pageCache.CalculateWorldCoordinates();
            var pageWidth = (int)this._pageCache.CurrentPageModel.WorldWidth;
            var pageLeft = (int)this._pageCache.CurrentPageModel.WorldOrigin.X;
            var pageRight = pageLeft + pageWidth;

            var pageHeight = (int)this._pageCache.CurrentPageModel.WorldHeight;
            var pageBottom = (int)this._pageCache.CurrentPageModel.WorldOrigin.Y;
            var pageTop = pageBottom + pageHeight;

            var pageFront = (int)this.TheCamera.Position.Z;
            var pageBack = (int)(this._pageCache.CurrentPageModel.WorldOrigin.Z -
                                this._pageCache.CurrentPageModel.WorldDepth);

            // Based on these dimensions, construct a cube of blocks to test
            bool foundSolid = false;
            for (var i = pageLeft; i <= pageRight; i++)
            {
                for (var j = pageBottom; j <= pageTop; j++)
                {
                    for (var k = pageBack; k <= pageFront; k++)
                    {
                        // Is any block in this cube solid?
                        Block block = _blockStorage.BlockAt(i, j, k);
                        if (block.Exists)
                        {
                            // Flag this is no good and break out of loops
                            foundSolid = true;
                            goto FOUND_SOLID;
                        }
                    }
                }
            }

            FOUND_SOLID:

            return foundSolid;
        }

        /* TODO: Leaving in for reference
        private void ProcessView()
        {
            if (FlyingEnabled) return;
            var rotationMatrix = Matrix.CreateRotationX(this.CurrentElevation)*
                                 Matrix.CreateRotationY(this.CurrentRotation);
            this.LookVector = Vector3.Transform(Vector3.Forward, rotationMatrix);
            this.LookVector.Normalize();
            this.FindAimedBlock();
        }
        */

        /* TODO: Leaving in for reference
        public void Jump()
        {
            var footPosition = Position + new Vector3(0f, -1.5f, 0f);
            Block standingBlock = _blockStorage.BlockAt(footPosition);

            if (!standingBlock.Exists && this.Velocity.Y != 0) return;
            float amountBelowSurface = ((ushort) footPosition.Y) + 1 - footPosition.Y;
            Position += new Vector3(0, amountBelowSurface + 0.01f, 0);

            this.Velocity.Y = JumpVelocity;
        }
        */

        /* TODO: Leaving in for reference
        public void Move(GameTime gameTime, MoveDirection direction)
        {
            var moveVector = Vector3.Zero;

            switch (direction)
            {
                case MoveDirection.Forward:
                    moveVector.Z--;
                    break;
                case MoveDirection.Backward:
                    moveVector.Z++;
                    break;
                case MoveDirection.Left:
                    moveVector.X--;
                    break;
                case MoveDirection.Right:
                    moveVector.X++;
                    break;
            }

            if (moveVector == Vector3.Zero) return;

            if (!FlyingEnabled)
            {
                moveVector *= MoveSpeed*(float) gameTime.ElapsedGameTime.TotalSeconds;
                var rotation = Matrix.CreateRotationY(this.CurrentRotation);
                var rotatedVector = Vector3.Transform(moveVector, rotation);
                TryMove(rotatedVector);
            }
            else
            {
                moveVector *= FlySpeed*(float) gameTime.ElapsedGameTime.TotalSeconds;
                var rotation = Matrix.CreateRotationX(this.CurrentElevation)*
                               Matrix.CreateRotationY(this.CurrentRotation);
                var rotatedVector = Vector3.Transform(moveVector, rotation);
                this.Position += (rotatedVector);
            }
        }

        private void TryMove(Vector3 moveVector)
        {
            // build a test move-vector slightly longer than moveVector.
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector *= moveVector.Length() + 0.3f;
            var footPosition = Position + new Vector3(0f, -0.5f, 0f);
            Vector3 testPosition = footPosition + testVector;
            if (_blockStorage.BlockAt(testPosition).Exists) return;

            // There should be some bounding box so his head does not enter a block above ;) /fasbat
            testPosition -= 2*new Vector3(0f, -0.5f, 0f);
            if (_blockStorage.BlockAt(testPosition).Exists) return;


            this.Position += moveVector;
        }
        */

        public void SpawnPlayerCamera(Vector2Int relativePosition)
        {
            this.RelativePosition = relativePosition;
            this.TheCamera.Position = new Vector3(relativePosition.X*Chunk.WidthInBlocks, _initialHeight,
                                        relativePosition.Z * Chunk.LengthInBlocks);
            this._world.SpawnWorld(relativePosition);
        }

        private void FindAimedBlock()
        {
            for (float x = 0.5f; x < 8f; x += 0.1f)
            {
                Vector3 target = this.TheCamera.Position + (LookVector*x);
                var block = _blockStorage.BlockAt(target);
                if (!block.Exists) this.AimedEmptyBlock = new PositionedBlock(new Vector3Int(target), block);
                else
                {
                    this.AimedSolidBlock = new PositionedBlock(new Vector3Int(target), block);
                    return;
                }
            }

            this.AimedSolidBlock = null;
        }

        public void Draw(GameTime gameTime)
        {
            if (this.AimedSolidBlock.HasValue) RenderAimedBlock();

            //// draw sample model
            //var transforms = new Matrix[_sampleModel.Bones.Count];
            //_sampleModel.CopyAbsoluteBoneTransformsTo(transforms);

            //Vector3 modelPosition = new Vector3(this.Position.X, this.Position.Y, this.Position.Z);
            //float modelRotation = 0.0f;

            //foreach(var mesh in _sampleModel.Meshes)
            //{
            //    foreach(BasicEffect effect in mesh.Effects)
            //    {
            //        effect.EnableDefaultLighting();

            //        effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateRotationY(modelRotation) * Matrix.CreateTranslation(modelPosition);
            //        effect.ViewDirection = Matrix.CreateLookAt(this._camera.Position, Vector3.Zero, Vector3.Up);
            //        effect.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), Game.GraphicsDevice.Viewport.AspectRatio, 1.0f, 10000.0f);
            //    }

            //    mesh.Draw();
            //}
        }

        private void RenderAimedBlock()
        {
            TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                // allows any transparent pixels in original PNG to draw transparent
            TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var position = this.AimedSolidBlock.Value.Position.AsVector3() + new Vector3(0.5f, 0.5f, 0.5f);
            Matrix matrix_a, matrix_b;
            Matrix identity = Matrix.Identity; // setup the matrix prior to translation and scaling  
            Matrix.CreateTranslation(ref position, out matrix_a);
                // translate the position a half block in each direction
            Matrix.CreateScale(0.505f, out matrix_b);
                // scales the selection box slightly larger than the targetted block
            identity = Matrix.Multiply(matrix_b, matrix_a); // the final position of the block

            _aimedBlockEffect.World = identity;
            _aimedBlockEffect.View = this.TheCamera.ViewMatrix;
            _aimedBlockEffect.Projection = this.TheCamera.ProjectionMatrix;
            _aimedBlockEffect.Texture = _aimedBlockTexture;
            _aimedBlockEffect.TextureEnabled = true;

            foreach (EffectPass pass in _aimedBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                for (int i = 0; i < _aimedBlockModel.Meshes[0].MeshParts.Count; i++)
                {
                    ModelMeshPart parts = _aimedBlockModel.Meshes[0].MeshParts[i];
                    if (parts.NumVertices == 0) continue;

                    TheGame.SharedGame.GraphicsDevice.Indices = parts.IndexBuffer;
                    TheGame.SharedGame.GraphicsDevice.SetVertexBuffer(parts.VertexBuffer);
                    TheGame.SharedGame.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, parts.NumVertices,
                                                              parts.StartIndex, parts.PrimitiveCount);
                }
            }
        }

        public void ToggleFlyForm()
        {
            this.FlyingEnabled = !this.FlyingEnabled;
        }

        public void SwitchState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Refresh:
                    {
                        // TODO: This should cause a new world to be generated
                        // this.SpawnWorld(new Vector2Int(0, 0));

                        break;
                    }
            }
        }
    }
}