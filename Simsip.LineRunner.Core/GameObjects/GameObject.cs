using Cocos2D;
using Engine.Assets;
using Engine.Graphics;
using Engine.Input;
using Simsip.LineRunner.Physics;


namespace Simsip.LineRunner.GameObjects
{
    /// <summary>
    ///  Derive or instantiate from this class for simple images.
    /// </summary>
    public class GameObject : CCSprite
    {
        // Required services
        protected IAssetManager _assetManager;
        protected IPhysicsManager _physicsManager;
        protected IInputManager _inputManager;
        public GameObject() 
        {
            GameObjType = GameObjectType.None;
        }

        #region Properties

        public GameObjectType GameObjType { get; set; }

        #endregion

        #region GameObject API

        public virtual void Initialize()
        {
            // Import required services
            this._assetManager = (IAssetManager)TheGame.SharedGame.Services.GetService(typeof(IAssetManager));
            this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));

        }

        #endregion
    }
}