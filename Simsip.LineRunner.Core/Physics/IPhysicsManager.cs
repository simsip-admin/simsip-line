using BEPUphysics;


namespace Simsip.LineRunner.Physics
{
    /// <summary>
    /// Physics manager that sets up physics simulator and serves as
    /// central location for physics simulator properties and functions.
    /// </summary>
    public interface IPhysicsManager
    {
        Space TheSpace { get; }
    }
}