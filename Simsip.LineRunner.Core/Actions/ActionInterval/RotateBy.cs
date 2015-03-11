using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class RotateBy : ActionInterval
    {
        protected Quaternion m_StartQ;
        protected Quaternion m_EndQ;

        protected float m_fAngleX;
        protected float m_fAngleY;
        protected float m_fAngleZ;
        protected float m_fStartAngleX;
        protected float m_fStartAngleY;
        protected float m_fStartAngleZ;

        /*
        public RotateBy(float duration, float fDeltaAngle)
        {
            InitWithDuration(duration, fDeltaAngle);
        }
        */

        public RotateBy(float duration, float yaw, float pitch, float roll)
        {
            InitWithDuration(duration, yaw, pitch, roll);
        }

        protected RotateBy(RotateBy rotateTo)
            : base(rotateTo)
        {
            InitWithDuration(rotateTo.m_fDuration, rotateTo.m_EndQ);
        }

        /*
        private bool InitWithDuration(float duration, float fDeltaAngle)
        {
            if (base.InitWithDuration(duration))
            {
                m_fAngleX = m_fAngleY = m_fAngleZ = fDeltaAngle;
                return true;
            }
            return false;
        }
        */

        private bool InitWithDuration(float duration, float yaw, float pitch, float roll)
        {
            if (base.InitWithDuration(duration))
            {
                /*
                m_fAngleX = fDeltaAngleX;
                m_fAngleY = fDeltaAngleY;
                m_fAngleZ = fDeltaAngleZ;
                */
                m_EndQ = Quaternion.CreateFromYawPitchRoll(
                    MathHelper.ToRadians(yaw),
                    MathHelper.ToRadians(pitch),
                    MathHelper.ToRadians(roll));

                return true;
            }
            return false;
        }

        private bool InitWithDuration(float duration, Quaternion quaternion)
        {
            if (base.InitWithDuration(duration))
            {
                /*
                m_fDstAngleX = fDeltaAngleX;
                m_fDstAngleY = fDeltaAngleY;
                m_fDstAngleZ = fDeltaAngleZ;
                */
                m_EndQ = quaternion;

                return true;
            }
            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as RotateBy;
                if (ret == null)
                {
                    return null;
                }
                base.Copy(ret);

                ret.InitWithDuration(m_fDuration, m_fAngleX, m_fAngleY, m_fAngleZ);

                return ret;
            }
            return new RotateBy(this);
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);

            Vector3 scale = new Vector3();
            Vector3 trans = new Vector3();
            target.WorldMatrix.Decompose(out scale, out m_StartQ, out trans);

            /*
            var testMatrix1 = Matrix.CreateRotationX(1);
            var testMatrix2 = Matrix.CreateRotationX(1);
            var testMatrix3 = Matrix.CreateRotationX(1);
            m_fStartAngleX = 1f; // TODO: target.WorldMatrix.
            m_fStartAngleY = 1f; // TODO: target.RotationY;
            m_fStartAngleZ = 1f; // TODO: target.RotationZ;
            */

        }

        public override void Update(float time)
        {
            // XXX: shall I add % 360
            if (m_pTarget != null)
            {
                // Grab our existing scale, rotate and translate matrices
                Vector3 scale = new Vector3();
                Quaternion rot = new Quaternion();
                Vector3 trans = new Vector3();
                m_pTarget.WorldMatrix.Decompose(out scale, out rot, out trans);

                // Make sure to only modify the rotate matrix
                var translateMatrix = Matrix.CreateTranslation(trans);
                var rotate = Quaternion.Lerp(m_StartQ, m_EndQ, time);
                var rotateMatrix = Matrix.CreateFromQuaternion(rotate);
                var scaleMatrix = Matrix.CreateScale(scale);

                m_pTarget.WorldMatrix = scaleMatrix * rotateMatrix * translateMatrix;

                /*
                m_pTarget.RotationX = m_fStartAngleX + m_fAngleX * time;
                m_pTarget.RotationY = m_fStartAngleY + m_fAngleY * time;
                // TODO
                // m_pTarget.RotationZ = m_fStartAngleZ + m_fAngleZ * time;
                */
            }
        }

        public override FiniteTimeAction Reverse()
        {
            return new RotateBy(m_fDuration, -m_fAngleX, -m_fAngleY, -m_fAngleZ);
        }
    }
}