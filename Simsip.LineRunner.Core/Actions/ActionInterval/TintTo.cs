using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;

namespace Simsip.LineRunner.Actions
{
    public class TintTo : TintBy
    {
        public TintTo(float duration, Color tint, float endAmount)
            : base(duration, tint, endAmount)
        {
            InitWithDuration(duration, tint, endAmount);
        }

        protected TintTo(TintTo tintTo) : base(tintTo)
        {
            InitWithDuration(tintTo.m_fDuration, tintTo._tint, tintTo._endAmount);
        }

        public bool InitWithDuration(float duration, Color tint, float endAmount)
        {
            if (base.InitWithDuration(duration))
            {
                _tint = tint;
                _endAmount = endAmount;
                return true;
            }
            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = (TintTo)zone;
                base.Copy(zone);
                ret.InitWithDuration(m_fDuration, _tint, _endAmount);

                return ret;
            }
            else
            {
                return new TintTo(this);
            }
        }

        public override void Update(float time)
        {
            if (m_pTarget != null)
            {
                // TODO: Is this necessary?
                /*
                Vector3 currentPos = m_pTarget.WorldOrigin;
                Vector3 diff = currentPos - m_previousPosition;
                m_startPosition = m_startPosition + diff;
                */

                float newAmount = _startAmount + _deltaAmount * time;
                m_pTarget.IsTinted = true;
                m_pTarget.BlendFactorTint = _tint;
                m_pTarget.BlendFactorAmount = newAmount;
                _previousAmount = newAmount;
            }
        }
        
        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            _startAmount = target.BlendFactorAmount;
            _deltaAmount = _endAmount - target.BlendFactorAmount;
        }
    }
}