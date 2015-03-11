
using BEPUphysicsDemos;
using Engine.Common.Logging;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Cocos2D;
using Engine.Input;
using Simsip.LineRunner;

namespace Engine.Graphics
{
    public class StationaryCamera : Camera
    {
        /// <summary>
        /// Proxies the player ready status so that other components
        /// know when they can start rendering themseleves.
        /// </summary>
        public bool Ready 
        { 
            get
            {
                return _inputManager.ThePlayerControllerInput.Ready;
            }
        }

        public float CurrentElevation { get; private set; }
        public float CurrentRotation { get; private set; }

        private const float RotationSpeed = 0.025f; // the rotation speed.
        private float _aspectRatio; //aspect ratio of the field of view (width/height). 

        private IInputManager _inputManager;

        /// <summary>
        /// Logging facility.
        /// </summary>
        private static readonly Logger Logger = LogManager.CreateLogger();

        public StationaryCamera(Vector3 position, float pitch, float yaw,  Matrix projectionMatrix)
            : base(position, pitch, yaw, projectionMatrix)
        {
            Initialize();
        }

        public void Initialize()
        {
            Logger.Trace("init()");

            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof (IInputManager));
            this._aspectRatio = TheGame.SharedGame.GraphicsDevice.Viewport.AspectRatio;

            this.Position = this._inputManager.PlayerCamera.Position;
        }

        public bool IsInFlyBy { get; set; }
    }
}