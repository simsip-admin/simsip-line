using System;

namespace Simsip.LineRunner.Actions
{
    public partial class EaseCustom : ActionEase
    {
        private Func<float, float> m_EaseFunc;

        public Func<float, float> EaseFunc
        {
            get { return m_EaseFunc; }
            set { m_EaseFunc = value; }
        }

        public EaseCustom(ActionInterval pAction, Func<float, float> easeFunc)
        {
            InitWithAction(pAction, easeFunc);
        }

        protected EaseCustom(EaseCustom easeCustom)
            : base(easeCustom)
        {
            InitWithAction((ActionInterval) easeCustom.InnerAction.Copy(), easeCustom.EaseFunc);
        }

        public void InitWithAction(ActionInterval action, Func<float, float> easeFunc)
        {
            base.InitWithAction(action);
            m_EaseFunc = easeFunc;
        }

        public override void Update(float time)
        {
            m_pInner.Update(m_EaseFunc(time));
        }

        public override FiniteTimeAction Reverse()
        {
            return new ReverseTime(new EaseCustom(this));
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseCustom;
                base.Copy(pCopy);
                pCopy.InitWithAction((ActionInterval) m_pInner.Copy(), m_EaseFunc);

                return pCopy;
            }
            return new EaseCustom(this);
        }
    }
}