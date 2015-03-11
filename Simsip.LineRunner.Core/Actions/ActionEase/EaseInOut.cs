using System;

namespace Simsip.LineRunner.Actions
{
    public class EaseInOut : EaseRateAction
    {
        public EaseInOut(ActionInterval pAction, float fRate) : base(pAction, fRate)
        {
        }

        public EaseInOut(EaseInOut easeInOut) : base(easeInOut)
        {
        }

        public override void Update(float time)
        {
            time *= 2;

            if (time < 1)
            {
                m_pInner.Update(0.5f * (float) Math.Pow(time, m_fRate));
            }
            else
            {
                m_pInner.Update(1.0f - 0.5f * (float) Math.Pow(2 - time, m_fRate));
            }
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseInOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()), m_fRate);

                return pCopy;
            }
            return new EaseInOut(this);
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseInOut((ActionInterval) m_pInner.Reverse(), m_fRate);
        }
    }
}