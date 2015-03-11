using System;

namespace Simsip.LineRunner.Actions
{
    public class EaseExponentialIn : ActionEase
    {
        public EaseExponentialIn(ActionInterval pAction) : base(pAction)
        {
        }

        public EaseExponentialIn(EaseExponentialIn easeExponentialIn) : base(easeExponentialIn)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.ExponentialIn(time));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseExponentialOut((ActionInterval) m_pInner.Reverse());
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseExponentialIn;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseExponentialIn(this);
        }
    }
}