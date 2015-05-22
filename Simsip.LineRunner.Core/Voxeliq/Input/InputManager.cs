/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using Cocos2D;
using System;
using BEPUphysicsDemos;
using BEPUphysicsDemos.AlternateMovement;
using Engine.Chunks;
using Engine.Common.Logging;
using Engine.Common.Vector;
using Engine.Debugging.Console;
using Engine.Debugging.Ingame;
using Engine.Graphics;
using Engine.Graphics.Effects.PostProcessing.Bloom;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simsip.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.SneakyJoystick;
using Simsip.LineRunner.Utils;
using Microsoft.Xna.Framework.Input.Touch;


namespace Engine.Input
{
    public enum MoveDirection
    {
        None,
        Forward,
        Backward,
        Left,
        Right,
    }

    /// <summary>
    /// Handles user mouse & keyboard input.
    /// </summary>
    public class InputManager : GameComponent, IInputManager
    {
        private float _defaultNearPlaneDistance;
        private float _defaultFarPlaneDistance;
        private Matrix _defaultCameraProjection;

        //Input
        public KeyboardState KeyboardInput;
        public KeyboardState PreviousKeyboardInput;
        public GamePadState GamePadInput;
        public GamePadState PreviousGamePadInput;
        public MouseState MouseInput;
        public MouseState PreviousMouseInput;


        private GameState _currentGameState;

        private CCPoint _previousStickDirection;
        private SneakyJoystickMovementStatus _previousJoystickMovementStatus;

        private int _previousButtonId;
        private SneakyButtonStatus _previousButtonStatus;

        private float _joystickDelta;

        private DateTime _previousDateTime;


        // properties.
        public bool CaptureMouse { get; private set; } // Should the game capture mouse?
        public bool CursorCentered { get; private set; } // Should the mouse cursor centered on screen?

        // previous input states.
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        // required services.
        private IWorld _world;
        private IGraphicsManager _graphicsManager;
        private IPhysicsManager _physicsManager;
        private IInGameDebuggerService _ingameDebuggerService;
        private IFogger _fogger;
        private ISkyDome _skyService;
        private IChunkCache _chunkCache;
        private IBloomService _bloomService;

        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility.

        /// <summary>
        /// Creates a new input manager.
        /// </summary>
        /// <param name="game"></param>
        public InputManager(Game game)
            : base(game)
        {
            this.Game.Services.AddService(typeof (IInputManager), this); // export service.

            this.CaptureMouse = true; // capture the mouse by default.
            this.CursorCentered = false; // only center the mouse in certain game states
        }

        public Camera CurrentCamera { get; private set; }
        public Camera LineRunnerCamera { get; private set; }
        public Camera PlayerCamera { get; private set; }
        public StationaryCamera TheStationaryCamera { get; private set; }

        public IControllerInput CurrentControllerInput { get; private set; }
        public LineRunnerControllerInput TheLineRunnerControllerInput { get; private set; }
        public PlayerControllerInput ThePlayerControllerInput { get; private set; }

        /// <summary>
        /// Determines whether or not the key was pressed this frame.
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <returns>Whether or not the key was pressed.</returns>
        public bool WasKeyPressed(Keys key)
        {
            return KeyboardInput.IsKeyDown(key) && PreviousKeyboardInput.IsKeyUp(key);
        }

        /// <summary>
        /// Determines whether or not the button was pressed this frame.
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Whether or not the button was pressed.</returns>
        public bool WasButtonPressed(Buttons button)
        {
            return GamePadInput.IsButtonDown(button) && PreviousGamePadInput.IsButtonUp(button);
        }

        /// <summary>
        /// Initializes the input manager.
        /// </summary>
        public override void Initialize()
        {
            Logger.Trace("init()");

            // import required services.
            this._world = (IWorld) this.Game.Services.GetService(typeof (IWorld));
            this._graphicsManager = (IGraphicsManager) this.Game.Services.GetService(typeof (IGraphicsManager));
            this._physicsManager = (IPhysicsManager)this.Game.Services.GetService(typeof(IPhysicsManager));
            this._ingameDebuggerService =(IInGameDebuggerService) this.Game.Services.GetService(typeof (IInGameDebuggerService));
            this._fogger = (IFogger) this.Game.Services.GetService(typeof (IFogger));
            this._skyService = (ISkyDome) this.Game.Services.GetService(typeof (ISkyDome));
            this._chunkCache = (IChunkCache) this.Game.Services.GetService(typeof (IChunkCache));
            this._bloomService = (IBloomService) this.Game.Services.GetService(typeof (IBloomService));

            // get current mouse & keyboard states.
            this._previousKeyboardState = Keyboard.GetState();
            this._previousMouseState = Mouse.GetState();

            this._previousJoystickMovementStatus = SneakyJoystickMovementStatus.End;
            this._previousStickDirection = new CCPoint();
            this._previousButtonId = -1;
            this._previousButtonStatus = SneakyButtonStatus.None;
            this._previousDateTime = DateTime.Now;

            this.HudCameraOffsetX = UserDefaults.SharedUserDefault.GetFloatForKey(
                GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_X,
                GameConstants.USER_DEFAULT_INITIAL_HUD_OFFSET_X);
            this.HudCameraOffsetY = UserDefaults.SharedUserDefault.GetFloatForKey(
                GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_Y,
                GameConstants.USER_DEFAULT_INITIAL_HUD_OFFSET_Y);
            this.HudCameraOffsetYaw = UserDefaults.SharedUserDefault.GetFloatForKey(
                GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_YAW,
                GameConstants.USER_DEFAULT_INITIAL_HUD_OFFSET_YAW);
            this.HudCameraOffsetPitch = UserDefaults.SharedUserDefault.GetFloatForKey(
                GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_PITCH,
                GameConstants.USER_DEFAULT_INITIAL_HUD_OFFSET_PITCH);
            this.HudCameraOrbitYaw = UserDefaults.SharedUserDefault.GetFloatForKey(
                GameConstants.USER_DEFAULT_KEY_HUD_ORBIT_YAW,
                GameConstants.USER_DEFAULT_INITIAL_HUD_ORBIT_YAW);
            this.HudCameraOrbitPitch = UserDefaults.SharedUserDefault.GetFloatForKey(
                GameConstants.USER_DEFAULT_KEY_HUD_ORBIT_PITCH,
                GameConstants.USER_DEFAULT_INITIAL_HUD_ORBIT_PITCH);
            
            this._joystickDelta = 0.1f;

            // Construct our controllers
            this._defaultNearPlaneDistance = 0.01f;
            this._defaultFarPlaneDistance = 1500f;
            this._defaultCameraProjection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.Pi / 3.0f, 
                TheGame.SharedGame.GraphicsDevice.Viewport.AspectRatio, 
                this._defaultNearPlaneDistance,
                this._defaultFarPlaneDistance);
            this.PlayerCamera = new Camera(Vector3.Zero, 0f, 0f, this._defaultCameraProjection);
            this.ThePlayerControllerInput = new PlayerControllerInput(this._physicsManager.TheSpace, this.PlayerCamera, TheGame.SharedGame);
            this.LineRunnerCamera = new Camera(Vector3.Zero, 0f, 0f, this._defaultCameraProjection);
            this.TheLineRunnerControllerInput = new LineRunnerControllerInput(this._physicsManager.TheSpace, this.LineRunnerCamera, TheGame.SharedGame);

            // Construct our stationary camera
            this.TheStationaryCamera = new StationaryCamera(Vector3.Zero, 0f, 0f, this._defaultCameraProjection);
            
            // Set inital current controller and camera
            this.CurrentCamera = this.TheLineRunnerControllerInput.TheCamera;
            this.CurrentControllerInput = this.TheLineRunnerControllerInput;

            base.Initialize();
        }

        public void SwitchState(GameState gameState)
        {
            this._currentGameState = gameState;
        }

        public void HudOnGesture(CCGesture g)
        {
            if (g.GestureType == GestureType.FreeDrag)
            {
                this.HudCameraOffsetYaw += g.Delta.X;
                this.HudCameraOrbitPitch += g.Delta.Y;
            }
        }

        public void HudButtonStartEndEvent(object sender, EventCustom e) 
        {
            var sneakyButtonEventResponse = e.UserData as SneakyButtonEventResponse;
            var sneakyButtonStatus = sneakyButtonEventResponse.ResponseType;
            var sneakyButtonId = sneakyButtonEventResponse.ID;

            if (sneakyButtonStatus != SneakyButtonStatus.Press)
            {
                return;
            }

            MoveDirection moveDirection = (MoveDirection)sneakyButtonId;
            switch (moveDirection)
            {
                case MoveDirection.Forward:
                    {
                        this.HudCameraOffsetY += this._joystickDelta;
                        break;
                    }
                case MoveDirection.Backward:
                    {
                        this.HudCameraOffsetY -= this._joystickDelta;
                        break;
                    }
                case MoveDirection.Left:
                    {
                        this.HudCameraOffsetX -= this._joystickDelta;
                        break;
                    }
                case MoveDirection.Right:
                    {
                        this.HudCameraOffsetX += this._joystickDelta;
                        break;
                    }
            }

            // movement keys.
            /*
            var moveDirection = MoveDirection.None;
            if (sneakyButtonId == (int)MoveDirection.Forward ||
                sneakyButtonId == (int)MoveDirection.Backward ||
                sneakyButtonId == (int)MoveDirection.Left ||
                sneakyButtonId == (int)MoveDirection.Right)
            {
                moveDirection = (MoveDirection)sneakyButtonId;
            }
            */

            // TODO: How to jump - 2nd joystick?
            /*
            if (_previousKeyboardState.IsKeyUp(Keys.Space) && currentState.IsKeyDown(Keys.Space))
            {
                _playerCamera.Jump();
            }
            */

            /* TODO: Leaving in until we know what we want to do here
            var newGameTime = new GameTime();
            var currentDateTime = DateTime.Now;
            newGameTime.ElapsedGameTime = currentDateTime - this._previousDateTime;
            this._previousButtonStatus = sneakyButtonStatus;
            this._previousButtonId = sneakyButtonId;
            this._previousDateTime = currentDateTime;
            */

            /* TODO: Leaving in until we know what we want to do here
            switch(this._currentGameState)
            {
                case GameState.Moving:
                    {
                        // TODO
                        // this.TheLineRunnerControllerInput.Move(newGameTime, moveDirection);

                        break;
                    }
                case GameState.World:
                    {
                        // TODO
                        // this.ThePlayerControllerInput.Move(newGameTime, moveDirection);

                        break;
                    }
            }
            */
        }

        public void HudStickStartEndEvent(object sender, EventCustom e)
        {
            var stickDirection = (CCPoint)e.UserData;

            if (stickDirection == null ||
                stickDirection == this._previousStickDirection)
            {
                return;
            }

            float rotation = stickDirection.X - Core.Engine.Instance.Configuration.Graphics.Width / 2;
            float elevation = stickDirection.Y - Core.Engine.Instance.Configuration.Graphics.Height / 2;

            // TODO: When we pull in ui
            /*
            if (stickDirection.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                this._player.Weapon.Use();
            if (stickDirection.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
                this._player.Weapon.SecondaryUse();
            */

            this._previousStickDirection = stickDirection;

            switch (this._currentGameState)
            {
                case GameState.Moving:
                    {
                        if (rotation != 0)
                        {
                            // TODO
                            // this.TheLineRunnerControllerInput.RotateCamera(rotation);
                        }
                        if (elevation != 0)
                        {
                            // TODO
                            // this.TheLineRunnerControllerInput.ElevateCamera(elevation);
                        }

                        break;
                    }
                case GameState.World:
                    {
                        if (rotation != 0)
                        {
                            // TODO
                            // this.ThePlayerControllerInput.RotateCamera(rotation);
                        }
                        if (elevation != 0)
                        {
                            // TODO
                            // this.ThePlayerControllerInput.ElevateCamera(elevation);
                        }

                        break;
                    }
            }
        }

        public float HudCameraOffsetX { get; private set; }
        public float HudCameraOffsetY { get; private set; }
        public float HudCameraOffsetYaw { get; private set; }
        public float HudCameraOffsetPitch { get; private set; }
        public float HudCameraOrbitYaw { get; private set; }
        public float HudCameraOrbitPitch { get; private set; }

        /// <summary>
        /// Handles input updates.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Only need this for touchdowns - how to optimize?
            this.ThePlayerControllerInput.Update(gameTime);

            PreviousKeyboardInput = KeyboardInput;
            KeyboardInput = Keyboard.GetState();
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            PreviousMouseInput = MouseInput;
            MouseInput = Mouse.GetState();

            //Keep the mouse within the screen
            /* TODO: Causes exception to be thrown
            if (!TheGame.SharedGame.IsMouseVisible)
                Mouse.SetPosition(200, 200);
            */

            PreviousGamePadInput = GamePadInput;

            for (int i = 0; i < 4; i++)
            {
                GamePadInput = GamePad.GetState((PlayerIndex)i);
                if (GamePadInput.IsConnected)
                    break;
            }

            // Allows the default game to exit on Xbox 360 and Windows
            if (KeyboardInput.IsKeyDown(Keys.Escape) || GamePadInput.Buttons.Back == ButtonState.Pressed)
                // TODO: Exit();

            //Toggle mouse control.  The camera will look to the IsMouseVisible to determine if it should turn.
            if (WasKeyPressed(Keys.Tab))
                TheGame.SharedGame.IsMouseVisible = !TheGame.SharedGame.IsMouseVisible;

            //TODO: Base.update updates the space, which needs to be done before the camera is updated.
            this.ThePlayerControllerInput.Update(dt, PreviousKeyboardInput, KeyboardInput, PreviousGamePadInput, GamePadInput);
        }

        /// <summary>
        /// Processes mouse input by user.
        /// </summary>
        private void ProcessMouse()
        {
            var currentState = Mouse.GetState();

            if (currentState == this._previousMouseState || !this.CaptureMouse) // if there's no mouse-state change or if it's not captured, just return.
                return;

            float rotation = currentState.X - Core.Engine.Instance.Configuration.Graphics.Width / 2;
            if (rotation != 0)
            {
                // TODO
                // this.TheLineRunnerControllerInput.RotateCamera(rotation);
            }

            float elevation = currentState.Y - Core.Engine.Instance.Configuration.Graphics.Height / 2;
            if (elevation != 0)
            {
                // TODO
                // this.TheLineRunnerControllerInput.ElevateCamera(elevation);
            }

            if (currentState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                this.ThePlayerControllerInput.Weapon.Use();
            if (currentState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released) 
                this.ThePlayerControllerInput.Weapon.SecondaryUse();

            this._previousMouseState = currentState;
            this.CenterCursor();
        }

        /// <summary>
        /// Processes keyboard input by user.
        /// </summary>
        /// <param name="gameTime"></param>
        private void ProcessKeyboard(GameTime gameTime)
        {
            var currentState = Keyboard.GetState();

            if (currentState.IsKeyDown(Keys.Escape))
            {
                // allows quick exiting of the game.
                // TODO: New Monogame
                // this.Game.Exit();

            }

            if (!Core.Engine.Instance.Console.Opened)
            {
                if (_previousKeyboardState.IsKeyUp(Keys.OemTilde) && currentState.IsKeyDown(Keys.OemTilde)) // tilda
                    KeyDown(null, new KeyEventArgs(Keys.OemTilde));

                // movement keys.
                if (currentState.IsKeyDown(Keys.Up) || currentState.IsKeyDown(Keys.W))
                {
                    // TODO
                    // this.ThePlayerControllerInput.Move(gameTime, MoveDirection.Forward);
                }
                if (currentState.IsKeyDown(Keys.Down) || currentState.IsKeyDown(Keys.S))
                {
                    // TODO
                    // this.ThePlayerControllerInput.Move(gameTime, MoveDirection.Backward);
                }
                if (currentState.IsKeyDown(Keys.Left) || currentState.IsKeyDown(Keys.A))
                {
                    // TODO
                    // this.ThePlayerControllerInput.Move(gameTime, MoveDirection.Left);
                }
                if (currentState.IsKeyDown(Keys.Right) || currentState.IsKeyDown(Keys.D))
                {
                    // TODO
                    // this.ThePlayerControllerInput.Move(gameTime, MoveDirection.Right);
                }
                if (_previousKeyboardState.IsKeyUp(Keys.Space) && currentState.IsKeyDown(Keys.Space))
                {
                    // TODO
                    // this.ThePlayerControllerInput.Jump();
                }

                // debug keys.

                if (_previousKeyboardState.IsKeyUp(Keys.F1) && currentState.IsKeyDown(Keys.F1)) // toggles infinitive world on or off.
                    Core.Engine.Instance.Configuration.World.ToggleInfinitiveWorld();

                if (_previousKeyboardState.IsKeyUp(Keys.F2) && currentState.IsKeyDown(Keys.F2)) // toggles flying on or off.
                    this.ThePlayerControllerInput.ToggleFlyForm();

                if (_previousKeyboardState.IsKeyUp(Keys.F5) && currentState.IsKeyDown(Keys.F5))
                {
                    this.CaptureMouse = !this.CaptureMouse;
                    this.Game.IsMouseVisible = !this.CaptureMouse;
                }

                if (_previousKeyboardState.IsKeyUp(Keys.F10) && currentState.IsKeyDown(Keys.F10))
                    this._ingameDebuggerService.ToggleInGameDebugger();
            }
            else
            {
                // console chars.
                foreach (var @key in Enum.GetValues(typeof(Keys)))
                {
                    if (_previousKeyboardState.IsKeyUp((Keys)@key) && currentState.IsKeyDown((Keys)@key))
                        KeyDown(null, new KeyEventArgs((Keys)@key));
                }
            }

            this._previousKeyboardState = currentState;
        }      

        /// <summary>
        /// Centers cursor on screen.
        /// </summary>
        private void CenterCursor()
        {
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
        }

        public delegate void KeyEventHandler(object sender, KeyEventArgs e);

        public event KeyEventHandler KeyDown;
    }
}