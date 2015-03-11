using System;
using System.Diagnostics;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    // Extra action for making a Sequence or Spawn when only adding one action to it.
    internal class ExtraAction : FiniteTimeAction
    {
        public override Action Copy()
        {
            return new ExtraAction();
        }

        public override FiniteTimeAction Reverse()
        {
            return new ExtraAction();
        }

        public override void Step(float dt)
        {
        }

        public override void Update(float time)
        {
        }
    }

    public class ActionInterval : FiniteTimeAction
    {
        protected bool m_bFirstTick;
        protected float m_elapsed;

        protected ActionInterval()
        {
        }

        public ActionInterval(float d)
        {
            InitWithDuration(d);
        }

        protected ActionInterval(ActionInterval actionInterval) : base(actionInterval)
        {
            InitWithDuration(actionInterval.m_fDuration);
        }

        public float Elapsed
        {
            get { return m_elapsed; }
        }

        protected bool InitWithDuration(float d)
        {
            m_fDuration = d;

            // prevent division by 0
            // This comparison could be in step:, but it might decrease the performance
            // by 3% in heavy based action games.
            if (m_fDuration == 0)
            {
                m_fDuration = float.Epsilon;
            }

            m_elapsed = 0;
            m_bFirstTick = true;

            return true;
        }

        public override bool IsDone
        {
            get { return m_elapsed >= m_fDuration; }
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = (ActionInterval) (zone);
                base.Copy(zone);

                ret.InitWithDuration(m_fDuration);
                return ret;
            }
            else
            {
                return new ActionInterval(this);
            }
        }

        public override void Step(float dt)
        {
            if (m_bFirstTick)
            {
                m_bFirstTick = false;
                m_elapsed = 0f;
            }
            else
            {
                m_elapsed += dt;
            }

            Update(Math.Max(0f,
                            Math.Min(1, m_elapsed /
                                        Math.Max(m_fDuration, float.Epsilon)
                                )
                       )
                );
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_elapsed = 0.0f;
            m_bFirstTick = true;
        }

        public override FiniteTimeAction Reverse()
        {
            throw new NotImplementedException();
        }

        public virtual float AmplitudeRate
        {
            get
            {
                Debug.Assert(false);
                return 0;
            }
            set { Debug.Assert(false); }
        }
    }
}