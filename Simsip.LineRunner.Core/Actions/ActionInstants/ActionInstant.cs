namespace Simsip.LineRunner.Actions
{
    public class ActionInstant : FiniteTimeAction
    {
        protected ActionInstant()
        {
        }

        protected ActionInstant(ActionInstant actionInstant) : base(actionInstant)
        {
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = (ActionInstant) zone;
                base.Copy(zone);
                return ret;
            }
            return new ActionInstant(this);
        }

        public override bool IsDone
        {
            get { return true; }
        }

        public override void Step(float dt)
        {
            Update(1);
        }

        public override void Update(float time)
        {
            // ignore
        }

        public override FiniteTimeAction Reverse()
        {
            return (FiniteTimeAction) Copy();
        }
    }
}