using Simsip.LineRunner.GameObjects;
using System.Diagnostics;

namespace Simsip.LineRunner.Actions
{
    public class ReverseTime : ActionInterval
    {
        protected FiniteTimeAction m_pOther;

        public ReverseTime(FiniteTimeAction action) : base(action.Duration)
        {
            m_pOther = action;
        }

        protected ReverseTime(ReverseTime copy)
            : base(copy)
        {
            m_pOther = copy.m_pOther;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as ReverseTime;
                base.Copy(zone);
                m_pOther = (FiniteTimeAction) ret.m_pOther; // .Copy() was in here before
                return ret;
            }
            else
            {
                return new ReverseTime(this);
            }
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_pOther.StartWithTarget(target);
        }

        public override void Stop()
        {
            m_pOther.Stop();
            base.Stop();
        }

        public override void Update(float time)
        {
            if (m_pOther != null)
            {
                m_pOther.Update(1 - time);
            }
        }

        public override FiniteTimeAction Reverse()
        {
            return m_pOther.Copy() as FiniteTimeAction;
        }
    }
}