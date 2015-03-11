using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos
{
    /// <summary>
    /// Superclass of implementations which control the behavior of a camera.
    /// </summary>
    public abstract class CameraControlScheme
    {
        /// <summary>
        /// Gets the game associated with the camera.
        /// </summary>
        // public DemosGame Game { get; private set; }
        public Game TheGame { get; private set; }

        /// <summary>
        /// Gets the camera controlled by this control scheme.
        /// </summary>
        public Camera Camera { get; private set; }

        protected CameraControlScheme(Camera camera, Game game)
        {
            Camera = camera;
            TheGame = game;
        }

        /// <summary>
        /// Updates the camera state according to the control scheme.
        /// </summary>
        /// <param name="dt">Time elapsed since previous frame.</param>
        public virtual void Update(float dt)
        {
#if XBOX360
            Yaw += Game.GamePadInput.ThumbSticks.Right.X * -1.5f * dt;
            Pitch += Game.GamePadInput.ThumbSticks.Right.Y * 1.5f * dt;
#else
            //Only turn if the mouse is controlled by the game.
            if (!TheGame.IsMouseVisible)
            {
                /* TODO
                Camera.Yaw((200 - TheGame.MouseInput.X) * dt * .12f);
                Camera.Pitch((200 - TheGame.MouseInput.Y) * dt * .12f);
                */
            }
#endif
        }
    }
}
