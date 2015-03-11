using System;
using Microsoft.Xna.Framework;

namespace Simsip.LineRunner.Actions
{
    public class EaseElasticInOut : EaseElastic
    {
        public EaseElasticInOut(ActionInterval pAction) : base(pAction, 0.3f)
        {
        }

        public EaseElasticInOut(ActionInterval pAction, float fPeriod) : base(pAction, fPeriod)
        {
        }

        protected EaseElasticInOut(EaseElasticInOut easeElasticInOut) : base(easeElasticInOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.ElasticInOut(time, m_fPeriod));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseElasticInOut((ActionInterval) m_pInner.Reverse(), m_fPeriod);
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseElasticInOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()), m_fPeriod);

                return pCopy;
            }
            return new EaseElasticInOut(this);
        }
    }
}