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
    public interface IControllerInput
    {
        /// <summary>
        /// Gets the camera to use for input.
        /// </summary>
        Camera TheCamera { get; }

        /// <summary>
        /// Gets the camera control scheme used by the character input.
        /// </summary>
        CameraControlScheme TheCameraControlScheme { get; }

        /// <summary>
        /// Gets whether the character controller's input management is being used.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Owning space of the character.
        /// </summary>
        Space TheSpace { get; }

        /// <summary>
        /// Gives the character control over the Camera and movement input.
        /// </summary>
        void Activate();

        /// <summary>
        /// Returns input control to the Camera.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Handles the input and movement of the character.
        /// </summary>
        /// <param name="dt">Time since last frame in simulation seconds.</param>
        /// <param name="previousKeyboardInput">The last frame's keyboard state.</param>
        /// <param name="keyboardInput">The current frame's keyboard state.</param>
        /// <param name="previousGamePadInput">The last frame's gamepad state.</param>
        /// <param name="gamePadInput">The current frame's keyboard state.</param>
        void Update(
            float dt, 
            KeyboardState previousKeyboardInput, 
            KeyboardState keyboardInput, 
            GamePadState previousGamePadInput, 
            GamePadState gamePadInput);
    }
}