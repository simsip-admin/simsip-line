namespace Simsip.LineRunner.Actions
{
    public class DelayTime : ActionInterval
    {
        public DelayTime(float d)
        {
            InitWithDuration(d);
        }

        protected DelayTime(DelayTime delayTime) : base(delayTime)
        {
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = (DelayTime) (pZone);
                base.Copy(pZone);

                return pCopy;
            }
            else
            {
                return new DelayTime(this);
            }
        }

        public override void Update(float time)
        {
        }

        public override FiniteTimeAction Reverse()
        {
            return new DelayTime(m_fDuration);
        }
    }
}