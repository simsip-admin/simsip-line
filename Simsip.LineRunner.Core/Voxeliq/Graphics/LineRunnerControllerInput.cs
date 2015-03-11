/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using BEPUphysics;
using BEPUphysicsDemos.AlternateMovement;
using Engine.Common.Logging;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Cocos2D;
using BEPUphysicsDemos;
using Engine.Input;
using Simsip.LineRunner;

namespace Engine.Graphics
{
    public class LineRunnerControllerInput : FreeControllerInput
    {
        /// <summary>
        /// Proxies the player ready status so that other components
        /// know when they can start rendering themseleves.
        /// </summary>
        public bool Ready 
        { 
            get
            {
                return this._inputManager.ThePlayerControllerInput.Ready;
            }
        }

        /// <summary>
        /// Logging facility.
        /// </summary>
        private static readonly Logger Logger = LogManager.CreateLogger();

        public bool FlyingEnabled { get; set; }

        private const float MoveSpeed = 5f; // the move speed.
        private const float FlySpeed = 25f; // the fly speed.

        private IInputManager _inputManager;

        public LineRunnerControllerInput(Space owningSpace, Camera camera, Game game)
            : base(owningSpace, camera, game)
        {
            Initialize();
        }

        public void Initialize()
        {
            Logger.Trace("init()");

            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof (IInputManager));
        }

        /* TODO: Leaving in for when fixing up code can see previous implementation
        public void PositionTrackingCamera(Vector3 position, Vector3 target)
        {
            this.Position = position;

            // TODO: New
            var rotation = Matrix.CreateRotationX(CurrentElevation) * Matrix.CreateRotationY(CurrentRotation);

            // Transform camera position based on rotation and elevation.
            // IMPORTANT: Passing in Vector3.Zero for target allows us to specify
            // a default target.
            if (target == Vector3.Zero)
            {
                this.Target = Vector3.Transform(Vector3.Forward, rotation) + Position;
            }
            else
            {
                this.Target = target;
            }
            var upVector = Vector3.Transform(Vector3.Up, rotation);
            this.View = Matrix.CreateLookAt(this.Position, this.Target, upVector);
        }
        */

        public bool IsInFlyBy { get; set; }

        /* TODO: Leaving in to see previous implementation
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
                moveVector *= MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                var rotation = Matrix.CreateRotationY(this.CurrentRotation);
                var rotatedVector = Vector3.Transform(moveVector, rotation);
                TryMove(rotatedVector);
            }
            else
            {
                moveVector *= FlySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                var rotation = Matrix.CreateRotationX(this.CurrentElevation) *
                               Matrix.CreateRotationY(this.CurrentRotation);
                var rotatedVector = Vector3.Transform(moveVector, rotation);
                this.Position += (rotatedVector);
            }
        }
        */
    }
}