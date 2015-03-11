namespace Simsip.LineRunner.Actions
{
    public class EaseBounceOut : ActionEase
    {
        public EaseBounceOut(ActionInterval pAction) : base(pAction)
        {
        }

        public EaseBounceOut(EaseBounceOut easeBounceOut) : base(easeBounceOut)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.BounceOut(time));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseBounceIn((ActionInterval) m_pInner.Reverse());
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseBounceOut;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseBounceOut(this);
        }
    }
}