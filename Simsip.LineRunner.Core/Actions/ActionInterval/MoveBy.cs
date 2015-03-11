using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class MoveBy : ActionInterval
    {
        protected Vector3 m_positionDelta;
        protected Vector3 m_endPosition;
        protected Vector3 m_startPosition;
        protected Vector3 m_previousPosition;

        public MoveBy(float duration, Vector3 position) 
        {
            InitWithDuration(duration, position);
        }

        protected MoveBy(MoveBy moveBy) : base(moveBy)
        {
            InitWithDuration(moveBy.m_fDuration, moveBy.m_positionDelta);
        }

        protected virtual bool InitWithDuration(float duration, Vector3 position)
        {
            if (base.InitWithDuration(duration))
            {
                m_positionDelta = position;
                return true;
            }
            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as MoveBy;

                if (ret == null)
                {
                    return null;
                }

                base.Copy(zone);

                ret.InitWithDuration(m_fDuration, m_positionDelta);

                return ret;
            }
            else
            {
                return new MoveBy(this);
            }
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_previousPosition = m_startPosition = target.WorldOrigin;
        }

        public override void Update(float time)
        {
            if (m_pTarget != null)
            {
                // TODO: Is this necessary?
                Vector3 currentPos = m_pTarget.WorldOrigin;
                Vector3 diff = currentPos - m_previousPosition;
                m_startPosition = m_startPosition + diff;

                Vector3 newPos = m_startPosition + m_positionDelta * time;
                m_pTarget.WorldOrigin = newPos;
                m_previousPosition = newPos;
            }
        }

        public override FiniteTimeAction Reverse()
        {
            return new MoveBy(m_fDuration, new Vector3(-m_positionDelta.X, -m_positionDelta.Y, -m_positionDelta.Z));
        }
    }
}