using System;
using Microsoft.Xna.Framework;

namespace Simsip.LineRunner.Actions
{
    public class EaseElasticIn : EaseElastic
    {
        public EaseElasticIn(ActionInterval pAction) : base(pAction, 0.3f)
        {
        }

        public EaseElasticIn(ActionInterval pAction, float fPeriod) : base(pAction, fPeriod)
        {
        }

        protected EaseElasticIn(EaseElasticIn easeElasticIn) : base(easeElasticIn)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.ElasticIn(time, m_fPeriod));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseElasticOut((ActionInterval) m_pInner.Reverse(), m_fPeriod);
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseElasticIn;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()), m_fPeriod);

                return pCopy;
            }
            return new EaseElasticIn(this);
        }
    }
}