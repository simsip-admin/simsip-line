using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class ScaleTo : ActionInterval
    {
        protected float m_fDeltaX;
        protected float m_fDeltaY;
        protected float m_fDeltaZ;
        protected float m_fEndScaleX;
        protected float m_fEndScaleY;
        protected float m_fEndScaleZ;
        protected float m_fScaleX;
        protected float m_fScaleY;
        protected float m_fScaleZ;
        protected float m_fStartScaleX;
        protected float m_fStartScaleY;
        protected float m_fStartScaleZ;

        protected ScaleTo(ScaleTo copy)
            : base(copy)
        {
            m_fEndScaleX = copy.m_fEndScaleX;
            m_fEndScaleY = copy.m_fEndScaleY;
            m_fEndScaleZ = copy.m_fEndScaleZ;
        }

        public ScaleTo(float duration, float s) : base(duration)
        {
            m_fEndScaleX = s;
            m_fEndScaleY = s;
            m_fEndScaleZ = s;
        }

        public ScaleTo(float duration, float sx, float sy, float sz) : base(duration)
        {
            m_fEndScaleX = sx;
            m_fEndScaleY = sy;
            m_fEndScaleZ = sz;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as ScaleTo;
                base.Copy(zone);
                m_fEndScaleX = ret.m_fEndScaleX;
                m_fEndScaleY = ret.m_fEndScaleY;
                m_fEndScaleZ = ret.m_fEndScaleZ;
                return ret;
            }
            else
            {
                return new ScaleTo(this);
            }
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            /*
            m_fStartScaleX = target.ModelToWorldRatio; // target.ScaleX;
            m_fStartScaleY = target.ModelToWorldRatio; // target.ScaleY;
            m_fStartScaleZ = target.ModelToWorldRatio; // target.ScaleZ;
           */

            Vector3 scale = new Vector3();
            Quaternion rot = new Quaternion();
            Vector3 trans = new Vector3();
            target.WorldMatrix.Decompose(out scale, out rot, out trans);
            m_fStartScaleX = scale.X;
            m_fStartScaleY = scale.Y;
            m_fStartScaleZ = scale.Z;

            m_fDeltaX = m_fEndScaleX - m_fStartScaleX;
            m_fDeltaY = m_fEndScaleY - m_fStartScaleY;
            m_fDeltaZ = m_fEndScaleZ - m_fStartScaleZ;
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

                // Make sure to only modify the scale matrix
                var translateMatrix = Matrix.CreateTranslation(trans);
                var rotateMatrix = Matrix.CreateFromQuaternion(rot);
                var scaleMatrix = Matrix.CreateScale(
                    m_fStartScaleX + m_fDeltaX * time,
                    m_fStartScaleY + m_fDeltaY * time,
                    m_fStartScaleZ + m_fDeltaZ * time);

                m_pTarget.WorldMatrix = scaleMatrix * rotateMatrix * translateMatrix;

                /*
                m_pTarget.ScaleX = m_fStartScaleX + m_fDeltaX * time;
                m_pTarget.ScaleY = m_fStartScaleY + m_fDeltaY * time;
                m_pTarget.ScaleZ = m_fStartScaleZ + m_fDeltaZ * time;
                */
            }
        }
    }
}