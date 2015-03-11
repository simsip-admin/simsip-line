using Simsip.LineRunner.GameObjects.Lines;

namespace Simsip.LineRunner.GameObjects.Sensors
{
    public enum MarginSensorType
    {
        Left,
        Middle,
        Right
    }

    public class MarginSensorModel : SensorModel
    {
        public MarginSensorModel(MarginSensorType marginSensorType)
        {
            this.TheMarginSensorType = marginSensorType;

            this.Initialize();
        }

        #region Properties

        public MarginSensorType TheMarginSensorType { get; private set; }

        #endregion

        #region Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            this.TheSensorType = SensorType.Margin;
        }

        #endregion
    }
}