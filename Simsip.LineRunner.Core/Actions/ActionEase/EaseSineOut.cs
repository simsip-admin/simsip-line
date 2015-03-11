using System;

namespace Simsip.LineRunner.Actions
{
    public class EaseSineOut : ActionEase
    {
        public EaseSineOut(ActionInterval pAction) : base(pAction)
        {
        }

        public EaseSineOut(EaseSineOut easeSineOut) : base(easeSineOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.SineOut(time));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseSineIn((ActionInterval) m_pInner.Reverse());
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseSineOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseSineOut(this);
        }
    }
}