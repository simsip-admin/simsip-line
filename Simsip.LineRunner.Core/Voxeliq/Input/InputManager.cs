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
using Simsip.LineRunner.Utils;
using Microsoft.Xna.Framework.Input.Touch;
using Simsip.LineRunner.GameObjects.Pages;


namespace Engine.Input
{
    public enum MoveDirection
    {
        None,
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Handles user mouse & keyboard input.
    /// </summary>
    public class InputManager : GameComponent, IInputManager
    {
        private float _defaultNearPlaneDistance;
        private float _defaultFarPlaneDistance;

        //Input
        public KeyboardState KeyboardInput;
        public KeyboardState PreviousKeyboardInput;
        public GamePadState GamePadInput;
        public GamePadState PreviousGamePadInput;
        public MouseState MouseInput;
        public MouseState PreviousMouseInput;


        private GameState _currentGameState;

        private float _joystickDelta;

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
        private IPageCache _pageCache;

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

        public Matrix DefaultCameraProjection { get; private set; }

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
            this._pageCache = (IPageCache)this.Game.Services.GetService(typeof(IPageCache));

            // get current mouse & keyboard states.
            this._previousKeyboardState = Keyboard.GetState();
            this._previousMouseState = Mouse.GetState();

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
            this.DefaultCameraProjection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.Pi / 3.0f, 
                TheGame.SharedGame.GraphicsDevice.Viewport.AspectRatio, 
                this._defaultNearPlaneDistance,
                this._defaultFarPlaneDistance);
            this.PlayerCamera = new Camera(Vector3.Zero, 0f, 0f, this.DefaultCameraProjection);
            this.ThePlayerControllerInput = new PlayerControllerInput(this._physicsManager.TheSpace, this.PlayerCamera, TheGame.SharedGame);
            this.LineRunnerCamera = new Camera(Vector3.Zero, 0f, 0f, this.DefaultCameraProjection);
            this.TheLineRunnerControllerInput = new LineRunnerControllerInput(this._physicsManager.TheSpace, this.LineRunnerCamera, TheGame.SharedGame);

            // Construct our stationary camera
            this.TheStationaryCamera = new StationaryCamera(Vector3.Zero, 0f, 0f, this.DefaultCameraProjection);
            
            // Set inital current controller and camera
            this.CurrentCamera = this.TheLineRunnerControllerInput.TheCamera;
            this.CurrentControllerInput = this.TheLineRunnerControllerInput;

            base.Initialize();
        }

        public void SwitchState(GameState gameState)
        {
            this._currentGameState = gameState;
        }

        public void HudOnGestureOffset(CCGesture g)
        {
            // 1. want a full swipe to equate to 45 degrees
            var xRadiansForSwipe = (g.Delta.X / TheGame.SharedGame.Window.ClientBounds.Width)  * (Microsoft.Xna.Framework.MathHelper.PiOver4);
            var yRadiansForSwipe = (g.Delta.Y / TheGame.SharedGame.Window.ClientBounds.Height) * (Microsoft.Xna.Framework.MathHelper.PiOver4);

            // 2. account for if we have rotated completely around
            this.HudCameraOffsetYaw = (this.HudCameraOffsetYaw + xRadiansForSwipe) % Microsoft.Xna.Framework.MathHelper.TwoPi;
            this.HudCameraOffsetPitch = (this.HudCameraOffsetPitch + yRadiansForSwipe) % Microsoft.Xna.Framework.MathHelper.TwoPi;

            // 3. Save for when we start up again
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_YAW, this.HudCameraOffsetYaw);
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_PITCH, this.HudCameraOffsetPitch);
        }
        public void HudOnGestureOrbit(CCGesture g)
        {
            // 1. want a full swipe to equate to 45 degrees
            var xRadiansForSwipe = (g.Delta.X / TheGame.SharedGame.Window.ClientBounds.Width) * (Microsoft.Xna.Framework.MathHelper.PiOver4);
            var yRadiansForSwipe = (g.Delta.Y / TheGame.SharedGame.Window.ClientBounds.Height) * (Microsoft.Xna.Framework.MathHelper.PiOver4);

            // 2. account for if we have rotated completely around
            this.HudCameraOrbitYaw = (this.HudCameraOrbitYaw + xRadiansForSwipe) % Microsoft.Xna.Framework.MathHelper.TwoPi;
            this.HudCameraOrbitPitch = (this.HudCameraOrbitPitch + yRadiansForSwipe) % Microsoft.Xna.Framework.MathHelper.TwoPi;

            // 3. Save for when we start up again
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_ORBIT_YAW, this.HudCameraOrbitYaw);
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_ORBIT_PITCH, this.HudCameraOrbitPitch);
        }

        public void HudOnGestureReset()
        {
            this.HudCameraOffsetX = GameConstants.USER_DEFAULT_INITIAL_HUD_OFFSET_X;
            this.HudCameraOffsetY = GameConstants.USER_DEFAULT_INITIAL_HUD_OFFSET_Y;
            this.HudCameraOffsetYaw = GameConstants.USER_DEFAULT_INITIAL_HUD_OFFSET_YAW;
            this.HudCameraOffsetPitch = GameConstants.USER_DEFAULT_INITIAL_HUD_OFFSET_PITCH;
            this.HudCameraOrbitYaw = GameConstants.USER_DEFAULT_INITIAL_HUD_ORBIT_YAW;
            this.HudCameraOrbitPitch = GameConstants.USER_DEFAULT_INITIAL_HUD_ORBIT_PITCH;

            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_X, this.HudCameraOffsetX);
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_Y, this.HudCameraOffsetY);
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_YAW, this.HudCameraOffsetYaw);
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_PITCH, this.HudCameraOffsetPitch);
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_ORBIT_YAW, this.HudCameraOrbitYaw);
            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_ORBIT_PITCH, this.HudCameraOrbitPitch);
            UserDefaults.SharedUserDefault.SetBoolForKey(GameConstants.USER_DEFAULT_KEY_HUD_ZOOM, false);
        }

        public void HudOnZoom(bool zoomIn)
        {
            if (zoomIn)
            {
                this._pageCache.CharacterDepthFromCameraStart =
                    0.5f * (this._pageCache.PageDepthFromCameraStart - GameConstants.DEFAULT_CHARACTER_DEPTH_FROM_PAGE);
            }
            else
            {
                this._pageCache.CharacterDepthFromCameraStart =
                    this._pageCache.PageDepthFromCameraStart - GameConstants.DEFAULT_CHARACTER_DEPTH_FROM_PAGE;
            }

            UserDefaults.SharedUserDefault.SetBoolForKey(GameConstants.USER_DEFAULT_KEY_HUD_ZOOM, zoomIn);
        }

        public void HudOnJoystick(MoveDirection moveDirection)
        {
            switch(moveDirection)
            {
                case MoveDirection.Left:
                    {
                        var newPosition = 
                            this.LineRunnerCamera.Position.X + 
                            this.HudCameraOffsetX - 
                            this._joystickDelta;
                        var limitLeft = 
                            this._pageCache.CurrentPageModel.WorldOrigin.X - 
                            (0.5f * this._pageCache.CurrentPageModel.WorldWidth);
                        if (newPosition > limitLeft)
                        {
                            this.HudCameraOffsetX -= this._joystickDelta;
                            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_X, this.HudCameraOffsetX);
                        }
                        break;
                    }
                case MoveDirection.Right:
                    {
                        var newPosition = 
                            this.LineRunnerCamera.Position.X + 
                            this.HudCameraOffsetX + 
                            this._joystickDelta;
                        var limitRight = 
                            this._pageCache.CurrentPageModel.WorldOrigin.X + 
                            (1.5f * this._pageCache.CurrentPageModel.WorldWidth);
                        if (newPosition < limitRight)
                        {
                            this.HudCameraOffsetX += this._joystickDelta;
                            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_X, this.HudCameraOffsetX);
                        }
                        break;
                    }
                case MoveDirection.Up:
                    {
                        var newPosition = 
                            this.LineRunnerCamera.Position.Y + 
                            this.HudCameraOffsetY + 
                            this._joystickDelta;
                        var limitUp = 
                            this._pageCache.CurrentPageModel.WorldOrigin.Y + 
                            (1.1f * this._pageCache.CurrentPageModel.WorldHeight);
                        if (newPosition < limitUp)
                        {
                            this.HudCameraOffsetY += this._joystickDelta;
                            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_Y, this.HudCameraOffsetY);
                        }
                        break;
                    }
               case MoveDirection.Down:
                    {
                        var newPosition = 
                            this.LineRunnerCamera.Position.Y + 
                            this.HudCameraOffsetY - 
                            this._joystickDelta;
                        var limitDown = this._pageCache.CurrentPageModel.WorldOrigin.Y;
                        if (newPosition > limitDown)
                        {
                            this.HudCameraOffsetY -= this._joystickDelta;
                            UserDefaults.SharedUserDefault.SetFloatForKey(GameConstants.USER_DEFAULT_KEY_HUD_OFFSET_Y, this.HudCameraOffsetY);
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