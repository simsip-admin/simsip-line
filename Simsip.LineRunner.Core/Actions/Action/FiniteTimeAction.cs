using System;
using System.Diagnostics;

namespace Simsip.LineRunner.Actions
{
    public class FiniteTimeAction : Action
    {
        protected float m_fDuration;

        protected FiniteTimeAction()
        {
        }

        protected FiniteTimeAction(FiniteTimeAction finiteTimeAction) : base(finiteTimeAction)
        {
        }

        public float Duration
        {
            get { return m_fDuration; }
            set { m_fDuration = value; }
        }

        public virtual FiniteTimeAction Reverse()
        {
            Debug.WriteLine("cocos2d: FiniteTimeAction#reverse: Implement me");
            return null;
        }
    }
}