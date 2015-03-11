using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class ScaleBy : ScaleTo
    {
        protected ScaleBy(ScaleBy copy)
            : base(copy)
        {
            // Handled by the base class.
        }

        public ScaleBy(float duration, float s)
            : base(duration, s)
        {
        }

        public ScaleBy(float duration, float sx, float sy, float sz)
            : base(duration, sx, sy, sz)
        {
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_fDeltaX = m_fStartScaleX * m_fEndScaleX - m_fStartScaleX;
            m_fDeltaY = m_fStartScaleY * m_fEndScaleY - m_fStartScaleY;
            m_fDeltaZ = m_fStartScaleZ * m_fEndScaleZ - m_fStartScaleZ;
        }

        public override FiniteTimeAction Reverse()
        {
            return new ScaleBy(m_fDuration, 1 / m_fEndScaleX, 1 / m_fEndScaleY, 1 / m_fEndScaleZ);
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as ScaleBy;
                base.Copy(zone); // Handles all data copying.
                return ret;
            }
            else
            {
                return new ScaleBy(this);
            }
        }
    }
}