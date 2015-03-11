using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class Parallel : ActionInterval
    {
        protected FiniteTimeAction[] m_pActions;

        public Parallel()
        {
        }

        /// <summary>
        /// Constructs the parallel sequence from the given array of actions.
        /// </summary>
        /// <param name="actions"></param>
        public Parallel(params FiniteTimeAction[] actions)
        {
            m_pActions = actions;
            float duration = 0f;
            for (int i = 0; i < m_pActions.Length; i++)
            {
                var actionDuration = m_pActions[i].Duration;
                if (duration < actionDuration)
                {
                    duration = actionDuration;
                }
            }

            for (int i = 0; i < m_pActions.Length; i++)
            {
                var actionDuration = m_pActions[i].Duration;
                if (actionDuration < duration)
                {
                    m_pActions[i] = new Sequence(m_pActions[i], new DelayTime(duration - actionDuration));
                }
            }

            base.InitWithDuration(duration);
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            for (int i = 0; i < m_pActions.Length; i++)
            {
                m_pActions[i].StartWithTarget(target);
            }
        }

        public Parallel(Parallel copy) : base(copy)
        {
            FiniteTimeAction[] cp = new FiniteTimeAction[copy.m_pActions.Length];
            for (int i = 0; i < copy.m_pActions.Length; i++)
            {
                cp[i] = copy.m_pActions[i].Copy() as FiniteTimeAction;
            }
            m_pActions = cp;
        }

        /// <summary>
        /// Reverses the current parallel sequence.
        /// </summary>
        /// <returns></returns>
        public override FiniteTimeAction Reverse()
        {
            FiniteTimeAction[] rev = new FiniteTimeAction[m_pActions.Length];
            for (int i = 0; i < m_pActions.Length; i++)
            {
                rev[i] = m_pActions[i].Reverse();
            }

            return new Parallel(rev);
        }

        /// <summary>
        /// Makea full copy of this object and does not make any reference copies.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public override object Copy(ICopyable zone)
        {
            ICopyable tmpZone = zone;
            Parallel ret;

            if (tmpZone != null && tmpZone != null)
            {
                ret = zone as Parallel;
                base.Copy(zone);

                FiniteTimeAction[] cp = new FiniteTimeAction[m_pActions.Length];
                for (int i = 0; i < m_pActions.Length; i++)
                {
                    cp[i] = m_pActions[i].Copy() as FiniteTimeAction;
                }
                ret.m_pActions = cp;
                return ret;
            }
            else
            {
                return new Parallel(this);
            }
        }

        public override void Stop()
        {
            for (int i = 0; i < m_pActions.Length; i++)
            {
                m_pActions[i].Stop();
            }
            base.Stop();
        }

        public override void Update(float time)
        {
            for (int i = 0; i < m_pActions.Length; i++)
            {
                m_pActions[i].Update(time);
            }
        }
    }
}