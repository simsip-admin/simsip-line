using Cocos2D;
using Simsip.LineRunner.GameFramework;
using System;
using Simsip.LineRunner.Physics;

namespace Simsip.LineRunner.GameObjects.Sensors
{
    public abstract class SensorModel : GameModel
    {
        public SensorModel()
        {
            Initialize();
        }

        #region Properties

        public SensorType TheSensorType { get; set; }

        public CCPoint Location { get; set; }

        public bool RemoveAfterUpdate { get; set; }

        #endregion

        #region Box2dSprite Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            RemoveAfterUpdate = true;

            this.GameObjType = GameObjectType.Sensor;
        }

        #endregion
    }
}