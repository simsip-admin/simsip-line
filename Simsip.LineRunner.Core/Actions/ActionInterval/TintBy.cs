using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;

namespace Simsip.LineRunner.Actions
{
    public class TintBy : ActionInterval
    {
        protected Color _tint;

        protected float _deltaAmount;
        protected float _endAmount;
        protected float _startAmount;
        protected float _previousAmount;

        public TintBy(float duration, Color tint, float deltaAmount)
        {
            InitWithDuration(duration, tint, deltaAmount);
        }

        protected TintBy(TintBy tintBy) : base(tintBy)
        {
            InitWithDuration(tintBy.m_fDuration, tintBy._tint, tintBy._deltaAmount);
        }

        public bool InitWithDuration(float duration, Color tint, float deltaAmount)
        {
            if (base.InitWithDuration(duration))
            {
                _tint = tint;
                _deltaAmount = deltaAmount;

                return true;
            }

            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as TintBy;
                if (ret == null)
                {
                    return null;
                }
                base.Copy(zone);

                ret.InitWithDuration(m_fDuration, _tint, _deltaAmount);

                return ret;
            }
            else
            {
                return new TintBy(this);
            }
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            _previousAmount = _startAmount = target.BlendFactorAmount;

            // Set tint?
        }

        public override void Update(float time)
        {
            if (m_pTarget != null)
            {
                // TODO: Is this necessary?
                float currentAmount = m_pTarget.BlendFactorAmount;
                float diffAmount = currentAmount - _previousAmount;
                _startAmount = _startAmount + diffAmount;

                float newAmount = _startAmount + _deltaAmount * time;
                m_pTarget.IsTinted = true;
                m_pTarget.BlendFactorTint = _tint;
                m_pTarget.BlendFactorAmount = newAmount;
                _previousAmount = newAmount;
            }
        }

        public override FiniteTimeAction Reverse()
        {
            return new TintBy(m_fDuration, _tint, -_deltaAmount);
        }
    }
}