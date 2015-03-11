using BEPUphysics;
using BEPUphysics.Character;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using ConversionHelper;
using Microsoft.Xna.Framework;
using Vector2 = BEPUutilities.Vector2;
using Vector3 = BEPUutilities.Vector3;

namespace BEPUphysicsDemos.AlternateMovement
{
    /// <summary>
    /// Handles input and movement of a character in the game.
    /// Acts as a simple 'front end' for the bookkeeping and math of the character controller.
    /// </summary>
    public class ThirdPersonControllerInput : IControllerInput
    {
        /// <summary>
        /// Gets the camera to use for input.
        /// </summary>
        public Camera TheCamera { get; private set; }

        /// <summary>
        /// Physics representation of the character.
        /// </summary>
        public CharacterController TheCharacterController;

        /// <summary>
        /// Gets the camera control scheme used by the character input.
        /// </summary>
        public CameraControlScheme TheCameraControlScheme { get; private set; }

        /// <summary>
        /// Gets whether the character controller's input management is being used.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Owning space of the character.
        /// </summary>
        public Space TheSpace { get; private set; }

        /// <summary>
        /// Constructs the character and internal physics character controller.
        /// </summary>
        /// <param name="owningSpace">Space to add the character to.</param>
        /// <param name="camera">Camera to attach to the character.</param>
        /// <param name="game">The running game.</param>
        public ThirdPersonControllerInput(Space owningSpace, Camera camera, Game game)
        {
            TheCharacterController = new CharacterController();
            TheCamera = camera;
            TheCameraControlScheme = new FixedOffsetCameraControlScheme(TheCharacterController.Body, camera, game);

            TheSpace = owningSpace;
        }

        /// <summary>
        /// Gives the character control over the Camera and movement input.
        /// </summary>
        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                TheSpace.Add(TheCharacterController);
                // TODO
                // CharacterController.Body.Position = Camera.Position - CameraControlScheme.CameraOffset;
                var cameraControlScheme = TheCameraControlScheme as FixedOffsetCameraControlScheme;
                TheCharacterController.Body.Position = ConversionHelper.MathConverter.Convert(TheCamera.Position) - cameraControlScheme.CameraOffset;
            }
        }

        /// <summary>
        /// Returns input control to the Camera.
        /// </summary>
        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                TheSpace.Remove(TheCharacterController);
            }
        }


        /// <summary>
        /// Handles the input and movement of the character.
        /// </summary>
        /// <param name="dt">Time since last frame in simulation seconds.</param>
        /// <param name="previousKeyboardInput">The last frame's keyboard state.</param>
        /// <param name="keyboardInput">The current frame's keyboard state.</param>
        /// <param name="previousGamePadInput">The last frame's gamepad state.</param>
        /// <param name="gamePadInput">The current frame's keyboard state.</param>
        public void Update(float dt, KeyboardState previousKeyboardInput, KeyboardState keyboardInput, GamePadState previousGamePadInput, GamePadState gamePadInput)
        {
            if (IsActive)
            {
                //Note that the character controller's update method is not called here; this is because it is handled within its owning space.
                //This method's job is simply to tell the character to move around.

                TheCameraControlScheme.Update(dt);

                Vector2 totalMovement = Vector2.Zero;

#if XBOX360
                totalMovement += new Vector2(gamePadInput.ThumbSticks.Left.X, gamePadInput.ThumbSticks.Left.Y);

                CharacterController.HorizontalMotionConstraint.SpeedScale = Math.Min(totalMovement.Length(), 1); //Don't trust the game pad to output perfectly normalized values.
                CharacterController.HorizontalMotionConstraint.MovementDirection = totalMovement;
                
                CharacterController.StanceManager.DesiredStance = gamePadInput.IsButtonDown(Buttons.RightStick) ? Stance.Crouching : Stance.Standing;

                //Jumping
                if (previousGamePadInput.IsButtonUp(Buttons.LeftStick) && gamePadInput.IsButtonDown(Buttons.LeftStick))
                {
                    CharacterController.Jump();
                }
#else

                //Collect the movement impulses.

                if (keyboardInput.IsKeyDown(Keys.E))
                {
                    totalMovement += new Vector2(0, 1);
                }
                if (keyboardInput.IsKeyDown(Keys.D))
                {
                    totalMovement += new Vector2(0, -1);
                }
                if (keyboardInput.IsKeyDown(Keys.S))
                {
                    totalMovement += new Vector2(-1, 0);
                }
                if (keyboardInput.IsKeyDown(Keys.F))
                {
                    totalMovement += new Vector2(1, 0);
                }
                if (totalMovement == Vector2.Zero)
                    TheCharacterController.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                else
                    TheCharacterController.HorizontalMotionConstraint.MovementDirection = Vector2.Normalize(totalMovement);


                TheCharacterController.StanceManager.DesiredStance = keyboardInput.IsKeyDown(Keys.Z) ? Stance.Crouching : Stance.Standing;

                //Jumping
                if (previousKeyboardInput.IsKeyUp(Keys.A) && keyboardInput.IsKeyDown(Keys.A))
                {
                    TheCharacterController.Jump();
                }
#endif
                // TODO
                // CharacterController.ViewDirection = Camera.WorldMatrix.Forward
                TheCharacterController.ViewDirection = ConversionHelper.MathConverter.Convert(TheCamera.WorldMatrix.Forward);
            }
        }
    }
}