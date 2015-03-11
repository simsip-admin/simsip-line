using System;

namespace Simsip.LineRunner.SneakyJoystick
{
    public class EventCustom : Event
    {
        public EventCustom(string eventName, object userData = null)
            : base(EventType.CUSTOM)
        {
            EventName = eventName;
            UserData = userData;
        }

        #region Properties

        public object UserData { get; set; }
        public string EventName { get; internal set; }

        #endregion
    }
}