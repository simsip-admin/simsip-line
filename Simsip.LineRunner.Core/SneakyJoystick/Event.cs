using Cocos2D;


namespace Simsip.LineRunner.SneakyJoystick
{
    public enum EventType
    {
        TOUCH,
        KEYBOARD,
        ACCELERATION,
        MOUSE,
        GAMEPAD,
        CUSTOM
    }

    public class Event
    {
        /// <summary>
        /// The event type
        /// </summary>
        internal EventType Type { get; private set; }

        /// <summary>
        /// Returns or sets whether propogation for the event is stopped or not
        /// </summary>
        public bool IsStopped { get; set; }

        /// <summary>
        /// The current target that this event is working on
        /// </summary>
        public CCNode CurrentTarget { get; internal set; }

        internal Event(EventType type)
        {
            Type = type;
            IsStopped = false;
        }

        /// <summary>
        /// Stops propagation for current event
        /// </summary>
        public void StopPropogation()
        {
            IsStopped = true;
        }
    }
}