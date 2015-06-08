using BEPUphysics;
using Engine.Common.Logging;
using Microsoft.Xna.Framework;


namespace Simsip.LineRunner.Physics
{
    /// <summary>
    /// Physics manager that sets up physics simulator and serves as
    /// central location for physics simulator properties and functions.
    /// </summary>
    public class PhysicsManager : GameComponent, IPhysicsManager
    {
        private static readonly Logger Logger = LogManager.CreateLogger(); // the logger.

        /// <summary>
        /// Creates a new PhysicsManager instance.
        /// </summary>
        /// <param name="game"></param>
        public PhysicsManager(Game game)
            : base(game)
        {
            // Register as service
            this.Game.Services.AddService(typeof(IPhysicsManager), this);

            // IMPORTANT: We initialize our physics space here as other service Initialize() functions
            // may depend on this already being in place (e.g., InputManager)
            this.TheSpace = new Space();
            this.TheSpace.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);
        }

        #region GameComponent Overrides

        /// <summary>
        /// Initializes the asset manager.
        /// </summary>
        public override void Initialize()
        {
            // Currently a no-op, see comment in constructor
        }

        #endregion

        #region IPhysicsManager Implementation

        public Space TheSpace {get; private set; }

        #endregion

    }
}