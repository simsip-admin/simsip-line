using System;
using System;
using System;
using System;
using System;
using Simsip.LineRunner.GameObjects;
using System.Diagnostics;

namespace Simsip.LineRunner.Actions
{
    public enum ActionTag
    {
        //! Default tag
        Invalid = -1,
    }

    public class Action : ICopyable
    {
        protected int m_nTag;
        protected GameModel m_pOriginalTarget;
        protected GameModel m_pTarget;

        public Action()
        {
            m_nTag = (int) ActionTag.Invalid;
        }

        protected Action(Action action)
        {
            m_nTag = action.m_nTag;
        }

        public GameModel Target
        {
            get { return m_pTarget; }
            set { m_pTarget = value; }
        }

        public GameModel OriginalTarget
        {
            get { return m_pOriginalTarget; }
        }

        public int Tag
        {
            get { return m_nTag; }
            set { m_nTag = value; }
        }

        public virtual Action Copy()
        {
            return (Action) Copy(null);
        }

        public virtual object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                ((Action) zone).m_nTag = m_nTag;
                return zone;
            }
            else
            {
                return new Action(this);
            }
        }

        public virtual bool IsDone
        {
            get { return true; }
        }

        protected internal virtual void StartWithTarget(GameModel target)
        {
            m_pOriginalTarget = m_pTarget = target;
        }

        public virtual void Stop()
        {
            m_pTarget = null;
        }

        public virtual void Step(float dt)
        {
#if DEBUG
            Debug.WriteLine("[Action step]. override me");
#endif
        }

        public virtual void Update(float time)
        {
#if DEBUG
            Debug.WriteLine("[Action update]. override me");
#endif
        }
    }
}