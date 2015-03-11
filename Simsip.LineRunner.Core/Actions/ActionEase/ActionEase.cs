using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class ActionEase : ActionInterval
    {
        protected ActionInterval m_pInner;

        // This can be taken out once all the classes that extend it have had their constructors created.
        protected ActionEase()
        {
        }

        public ActionInterval InnerAction
        {
            get { return m_pInner; }
        }

        public ActionEase(ActionInterval pAction)
        {
            InitWithAction(pAction);
        }

        protected ActionEase(ActionEase actionEase) : base(actionEase)
        {
            InitWithAction((ActionInterval) (actionEase.m_pInner.Copy()));
        }

        protected bool InitWithAction(ActionInterval pAction)
        {
            if (base.InitWithDuration(pAction.Duration))
            {
                m_pInner = pAction;
                return true;
            }
            return false;
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pCopy = pZone as ActionEase;
                base.Copy(pZone);

                pCopy.InitWithAction((ActionInterval) (m_pInner.Copy()));

                return pCopy;
            }
            return new ActionEase(this);
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_pInner.StartWithTarget(m_pTarget);
        }

        public override void Stop()
        {
            m_pInner.Stop();
            base.Stop();
        }

        public override void Update(float time)
        {
            m_pInner.Update(time);
        }

        public override FiniteTimeAction Reverse()
        {
            return new ActionEase((ActionInterval) m_pInner.Reverse());
        }
    }
}