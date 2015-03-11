namespace Simsip.LineRunner.Actions
{
    public class EaseBounceInOut : ActionEase
    {
        public EaseBounceInOut(ActionInterval pAction) : base(pAction)
        {
        }

        public EaseBounceInOut(EaseBounceInOut easeBounceInOut) : base(easeBounceInOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.BounceInOut(time));
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseBounceInOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseBounceInOut(this);
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseBounceInOut((ActionInterval) m_pInner.Reverse());
        }
    }
}