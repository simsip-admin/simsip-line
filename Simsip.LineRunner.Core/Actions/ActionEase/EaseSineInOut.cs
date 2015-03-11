using System;

namespace Simsip.LineRunner.Actions
{
    public class EaseSineInOut : ActionEase
    {
        public EaseSineInOut(ActionInterval pAction) : base(pAction)
        {
        }

        public EaseSineInOut(EaseSineInOut easeSineInOut) : base(easeSineInOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.SineInOut(time));
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = (EaseSineInOut) (pZone);
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseSineInOut(this);
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseSineInOut((ActionInterval) m_pInner.Reverse());
        }
    }
}