using System;

namespace Simsip.LineRunner.Actions
{
    public class EaseIn : EaseRateAction
    {
        public EaseIn(ActionInterval pAction, float fRate) : base(pAction, fRate)
        {
        }

        public EaseIn(EaseIn easeIn) : base(easeIn)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update((float) Math.Pow(time, m_fRate));
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseIn;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()), m_fRate);

                return pCopy;
            }
            return new EaseIn(this);
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseIn((ActionInterval) m_pInner.Reverse(), 1 / m_fRate);
        }
    }
}