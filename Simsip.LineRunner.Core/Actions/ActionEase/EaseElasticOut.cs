using System;
using Microsoft.Xna.Framework;

namespace Simsip.LineRunner.Actions
{
    public class EaseElasticOut : EaseElastic
    {
        public EaseElasticOut(ActionInterval pAction) : base(pAction, 0.3f)
        {
        }

        public EaseElasticOut(ActionInterval pAction, float fPeriod) : base(pAction, fPeriod)
        {
        }

        protected EaseElasticOut(EaseElasticOut easeElasticOut) : base(easeElasticOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.ElasticOut(time, m_fPeriod));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseElasticIn((ActionInterval) m_pInner.Reverse(), m_fPeriod);
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseElasticOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()), m_fPeriod);

                return pCopy;
            }
            return new EaseElasticOut(this);
        }
    }
}