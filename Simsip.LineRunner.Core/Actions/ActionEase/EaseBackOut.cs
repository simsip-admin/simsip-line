namespace Simsip.LineRunner.Actions
{
    public class EaseBackOut : ActionEase
    {
        public EaseBackOut(ActionInterval pAction) : base(pAction)
        {
        }

        protected EaseBackOut(EaseBackOut easeBackOut) : base(easeBackOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.BackOut(time));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseBackIn((ActionInterval) m_pInner.Reverse());
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseBackOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseBackOut(this);
        }
    }
}