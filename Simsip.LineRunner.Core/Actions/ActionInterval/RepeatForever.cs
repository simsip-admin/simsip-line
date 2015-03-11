using Simsip.LineRunner.GameObjects;
using System.Diagnostics;


namespace Simsip.LineRunner.Actions
{
    public class RepeatForever : ActionInterval
    {
        protected ActionInterval m_pInnerAction;

        public ActionInterval InnerAction
        {
            get { return m_pInnerAction; }
            set { m_pInnerAction = value; }
        }

        public RepeatForever(ActionInterval action)
        {
            InitWithAction(action);
        }

        protected RepeatForever(RepeatForever repeatForever) : base(repeatForever)
        {
            var param = repeatForever.m_pInnerAction.Copy() as ActionInterval;
            InitWithAction(param);
        }

        protected bool InitWithAction(ActionInterval action)
        {
            Debug.Assert(action != null);
            m_pInnerAction = action;
            // Duration = action.Duration;
            return true;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as RepeatForever;
                if (ret == null)
                {
                    return null;
                }
                base.Copy(zone);

                var param = m_pInnerAction.Copy() as ActionInterval;
                if (param == null)
                {
                    return null;
                }
                ret.InitWithAction(param);

                return ret;
            }
            else
            {
                return new RepeatForever(this);
            }
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_pInnerAction.StartWithTarget(target);
        }

        public override void Step(float dt)
        {
            m_pInnerAction.Step(dt);

            if (m_pInnerAction.IsDone)
            {
                float diff = m_pInnerAction.Elapsed - m_pInnerAction.Duration;
                m_pInnerAction.StartWithTarget(m_pTarget);
                m_pInnerAction.Step(0f);
                m_pInnerAction.Step(diff);
            }
        }

        public override bool IsDone
        {
            get { return false; }
        }

        public override FiniteTimeAction Reverse()
        {
            return new RepeatForever(m_pInnerAction.Reverse() as ActionInterval);
        }
    }
}