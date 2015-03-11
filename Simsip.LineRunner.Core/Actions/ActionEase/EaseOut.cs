using System;

namespace Simsip.LineRunner.Actions
{
    public class EaseOut : EaseRateAction
    {
        public EaseOut(ActionInterval pAction, float fRate) : base(pAction, fRate)
        {
        }

        public EaseOut(EaseOut easeOut) : base(easeOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update((float) (Math.Pow(time, 1 / m_fRate)));
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = (EaseOut) (pZone);
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()), m_fRate);

                return pCopy;
            }
            return new EaseOut(this);
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseOut((ActionInterval) m_pInner.Reverse(), 1 / m_fRate);
        }
    }
}