using System;
using Microsoft.Xna.Framework;

namespace Simsip.LineRunner.Actions
{
    public class EaseSineIn : ActionEase
    {
        public EaseSineIn(ActionInterval pAction) : base(pAction)
        {
        }

        public EaseSineIn(EaseSineIn easesineIn) : base(easesineIn)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.SineIn(time));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseSineOut((ActionInterval) m_pInner.Reverse());
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = (EaseSineIn) (pZone);
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseSineIn(this);
        }
    }
}