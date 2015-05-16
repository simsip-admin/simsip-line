using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class MoveTo : MoveBy
    {
        public MoveTo(float duration, Vector3 position)
            : base(duration, position)
        {
        }

        protected MoveTo(MoveTo moveTo) : base(moveTo)
        {
            InitWithDuration(moveTo.m_fDuration, moveTo.m_endPosition);
        }

        protected override bool InitWithDuration(float duration, Vector3 position)
        {
            if (base.InitWithDuration(duration))
            {
                m_endPosition = position;
                return true;
            }
            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = (MoveTo) zone;
                base.Copy(zone);
                ret.InitWithDuration(m_fDuration, m_endPosition);

                return ret;
            }
            else
            {
                return new MoveTo(this);
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

                // Grab our existing scale, rotate and translate matrices
                Vector3 scale = new Vector3();
                Quaternion rot = new Quaternion();
                Vector3 trans = new Vector3();
                m_pTarget.WorldMatrix.Decompose(out scale, out rot, out trans);

                // Make sure to only modify the rotate matrix
                Vector3 newPos = m_startPosition + m_positionDelta * time;
                var translateMatrix = Matrix.CreateTranslation(newPos);
                var rotateMatrix = Matrix.CreateFromQuaternion(rot);
                var scaleMatrix = Matrix.CreateScale(scale);

                m_pTarget.WorldMatrix = scaleMatrix * rotateMatrix * translateMatrix;

                // If we have a physics entity, have it follow the translation
                if (m_pTarget.PhysicsEntity != null)
                {
                    // m_pTarget.PhysicsEntity.Position = ConversionHelper.MathConverter.Convert(newPos);
                    m_pTarget.PhysicsEntity.WorldTransform = ConversionHelper.MathConverter.Convert(m_pTarget.WorldMatrix);
                }

                /* legacy
                Vector3 newPos = m_startPosition + m_positionDelta * time;
                m_pTarget.WorldOrigin = newPos;
                m_previousPosition = newPos;
                */
            }
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_startPosition = target.WorldOrigin;
            m_positionDelta = m_endPosition - target.WorldOrigin;
        }
    }
}