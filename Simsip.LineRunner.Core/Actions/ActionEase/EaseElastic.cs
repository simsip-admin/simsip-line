namespace Simsip.LineRunner.Actions
{
    public class EaseElastic : ActionEase
    {
        protected float m_fPeriod;

        public EaseElastic(ActionInterval pAction) : base(pAction)
        {
            InitWithAction(pAction);
        }

        public EaseElastic(ActionInterval pAction, float fPeriod) : base(pAction)
        {
            m_fPeriod = fPeriod;
        }

        protected EaseElastic(EaseElastic easeElastic) : base(easeElastic)
        {
            InitWithAction((ActionInterval) (easeElastic.m_pInner.Copy()), easeElastic.m_fPeriod);
        }

        public float Period
        {
            get { return m_fPeriod; }
            set { m_fPeriod = value; }
        }

        protected bool InitWithAction(ActionInterval pAction, float fPeriod)
        {
            if (base.InitWithAction(pAction))
            {
                m_fPeriod = fPeriod;
                return true;
            }
            return false;
        }

        protected new bool InitWithAction(ActionInterval pAction)
        {
            return InitWithAction(pAction, 0.3f);
        }

        public override FiniteTimeAction Reverse()
        {
            //assert(0);
            return null;
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseElastic;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()), m_fPeriod);

                return pCopy;
            }
            return new EaseElastic(this);
        }
    }
}