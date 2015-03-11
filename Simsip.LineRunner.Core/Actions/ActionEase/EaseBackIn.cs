using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class EaseBackIn : ActionEase
    {
        public EaseBackIn(ActionInterval pAction) : base(pAction)
        {
        }

        protected EaseBackIn(EaseBackIn easeBackIn) : base(easeBackIn)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.BackIn(time));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseBackOut((ActionInterval) m_pInner.Reverse());
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseBackIn;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseBackIn(this);
        }
    }
}