namespace Simsip.LineRunner.Actions
{
    public class EaseBackInOut : ActionEase
    {
        public EaseBackInOut(ActionInterval pAction) : base(pAction)
        {
        }

        protected EaseBackInOut(EaseBackInOut easeBackInOut) : base(easeBackInOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.BackInOut(time));
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseBackInOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseBackInOut(this);
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseBackInOut((ActionInterval) m_pInner.Reverse());
        }
    }
}