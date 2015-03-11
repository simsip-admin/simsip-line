using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class MoveTo : MoveBy
    {
        public MoveTo(float duration, Vector3 position)
            : base(duration, position)
        {
        }

        protected MoveTo(MoveTo moveTo) : base(moveTo)
        {
            InitWithDuration(moveTo.m_fDuration, moveTo.m_endPosition);
        }

        protected override bool InitWithDuration(float duration, Vector3 position)
        {
            if (base.InitWithDuration(duration))
            {
                m_endPosition = position;
                return true;
            }
            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = (MoveTo) zone;
                base.Copy(zone);
                ret.InitWithDuration(m_fDuration, m_endPosition);

                return ret;
            }
            else
            {
                return new MoveTo(this);
            }
        }

        public override void Update(float time)
        {
            if (m_pTarget != null)
            {
                // TODO: Is this necessary?
                /*
                Vector3 currentPos = m_pTarget.WorldOrigin;
                Vector3 diff = currentPos - m_previousPosition;
                m_startPosition = m_startPosition + diff;
                */

                Vector3 newPos = m_startPosition + m_positionDelta * time;
                m_pTarget.WorldOrigin = newPos;
                m_previousPosition = newPos;
            }
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_startPosition = target.WorldOrigin;
            m_positionDelta = m_endPosition - target.WorldOrigin;
        }
    }
}