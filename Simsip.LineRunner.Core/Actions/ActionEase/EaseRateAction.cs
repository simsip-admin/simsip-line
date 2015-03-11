namespace Simsip.LineRunner.Actions
{
    public class EaseRateAction : ActionEase
    {
        protected float m_fRate;

        public EaseRateAction(ActionInterval pAction, float fRate) : base(pAction)
        {
            m_fRate = fRate;
        }

        public EaseRateAction(EaseRateAction easeRateAction) : base(easeRateAction)
        {
            InitWithAction((ActionInterval) (easeRateAction.m_pInner.Copy()), easeRateAction.m_fRate);
        }

        public float Rate
        {
            get { return m_fRate; }
            set { m_fRate = value; }
        }

        protected bool InitWithAction(ActionInterval pAction, float fRate)
        {
            if (base.InitWithAction(pAction))
            {
                m_fRate = fRate;
                return true;
            }

            return false;
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = (EaseRateAction) (pZone);
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()), m_fRate);

                return pCopy;
            }
            return new EaseRateAction(this);
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseRateAction((ActionInterval) m_pInner.Reverse(), 1 / m_fRate);
        }
    }
}