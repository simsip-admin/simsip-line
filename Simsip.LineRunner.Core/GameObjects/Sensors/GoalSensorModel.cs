using Cocos2D;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Lines;
using System;
using Simsip.LineRunner.Physics;

namespace Simsip.LineRunner.GameObjects.Sensors
{
    public class GoalSensorModel : SensorModel
    {

        public GoalSensorModel()
        {
            this.Initialize();

        }

        #region Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            this.TheSensorType = SensorType.Goal;
        }

        #endregion
    }
}