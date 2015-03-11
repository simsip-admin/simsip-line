using System;

namespace Simsip.LineRunner.Actions
{
    public class EaseExponentialInOut : ActionEase
    {
        public EaseExponentialInOut(ActionInterval pAction) : base(pAction)
        {
        }

        public EaseExponentialInOut(EaseExponentialInOut easeExponentialInOut) : base(easeExponentialInOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.ExponentialInOut(time));
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseExponentialInOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseExponentialInOut(this);
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseExponentialInOut((ActionInterval) m_pInner.Reverse());
        }
    }
}