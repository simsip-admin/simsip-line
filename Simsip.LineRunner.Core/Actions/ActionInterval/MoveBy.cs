using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class MoveBy : ActionInterval
    {
        protected Vector3 m_positionDelta;
        protected Vector3 m_endPosition;
        protected Vector3 m_startPosition;
        protected Vector3 m_previousPosition;

        public MoveBy(float duration, Vector3 position) 
        {
            InitWithDuration(duration, position);
        }

        protected MoveBy(MoveBy moveBy) : base(moveBy)
        {
            InitWithDuration(moveBy.m_fDuration, moveBy.m_positionDelta);
        }

        protected virtual bool InitWithDuration(float duration, Vector3 position)
        {
            if (base.InitWithDuration(duration))
            {
                m_positionDelta = position;
                return true;
            }
            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as MoveBy;

                if (ret == null)
                {
                    return null;
                }

                base.Copy(zone);

                ret.InitWithDuration(m_fDuration, m_positionDelta);

                return ret;
            }
            else
            {
                return new MoveBy(this);
            }
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_previousPosition = m_startPosition = target.WorldOrigin;
        }

        public override void Update(float time)
        {
            if (m_pTarget != null)
            {
                // Grab our existing scale, rotate and translate matrices
                Vector3 scale = new Vector3();
                Quaternion rot = new Quaternion();
                Vector3 trans = new Vector3();
                m_pTarget.WorldMatrix.Decompose(out scale, out rot, out trans);

                // Make sure to only modify the translate matrix
                Vector3 newPos = m_startPosition + m_positionDelta * time;
                var translateMatrix = Matrix.CreateTranslation(newPos);
                var rotateMatrix = Matrix.CreateFromQuaternion(rot);
                var scaleMatrix = Matrix.CreateScale(scale);

                // Update our 3d model's world matrix
                m_pTarget.WorldMatrix = scaleMatrix * rotateMatrix * translateMatrix;

                // If we have a physics entity, have it follow the translation
                if (m_pTarget.PhysicsEntity != null)
                {
                    // IMPORTANT: Note how we account for the physic's position being
                    //            at the center of the physic entity.
                    m_pTarget.PhysicsEntity.Position =
                        ConversionHelper.MathConverter.Convert(newPos) +
                        m_pTarget.PhysicsLocalTransform.Translation;
                }
            }
        }

        public override FiniteTimeAction Reverse()
        {
            return new MoveBy(m_fDuration, new Vector3(-m_positionDelta.X, -m_positionDelta.Y, -m_positionDelta.Z));
        }
    }
}