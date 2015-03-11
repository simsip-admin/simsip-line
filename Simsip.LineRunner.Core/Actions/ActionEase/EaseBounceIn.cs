namespace Simsip.LineRunner.Actions
{
    public class EaseBounceIn : ActionEase
    {
        public EaseBounceIn(ActionInterval pAction) : base(pAction)
        {
        }

        public EaseBounceIn(EaseBounceIn easeBounceIn) : base(easeBounceIn)
        {
        }

        public override void Update(float time)
        {
            m_pInner.Update(EaseMath.BounceIn(time));
        }

        public override FiniteTimeAction Reverse()
        {
            return new EaseBounceOut((ActionInterval) m_pInner.Reverse());
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as EaseBounceIn;
                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new EaseBounceIn(this);
        }
    }
}