using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;


namespace Simsip.LineRunner.Actions
{
    public class RotateTo : ActionInterval
    {
        protected Quaternion m_StartQ;
        protected Quaternion m_EndQ;

        protected float m_fDiffAngleX;
        protected float m_fDiffAngleY;
        protected float m_fDiffAngleZ;
        protected float m_fDstAngleX;
        protected float m_fDstAngleY;
        protected float m_fDstAngleZ;
        protected float m_fStartAngleX;
        protected float m_fStartAngleY;
        protected float m_fStartAngleZ;

        private RotateTo()
        {
        }

        /// <summary>
        /// Create a rotate action for a model by specifying the
        /// yaw - the angle, in degrees, around the y-axis.
        /// pitch - the angle, in degrees, around the x-axis.
        /// roll - the angle, in degrees, around the z-axis.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="yaw">The yaw angle, in degrees, around the y-axis.</param>
        /// <param name="pitch">The pitch angle, in degrees, around the x-axis.</param>
        /// <param name="roll">The roll angle, in degrees, around the z-axis.</param>
        public RotateTo(float duration, float yaw, float pitch, float roll)
        {
            InitWithDuration(duration, yaw, pitch, roll);
        }

        protected RotateTo(RotateTo rotateTo)
            : base(rotateTo)
        {
            InitWithDuration(rotateTo.m_fDuration, rotateTo.m_EndQ);
        }

        private bool InitWithDuration(float duration, float yaw, float pitch, float roll)
        {
            if (base.InitWithDuration(duration))
            {
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
                m_EndQ = quaternion;

                return true;
            }
            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                var ret = zone as RotateTo;
                if (ret == null)
                {
                    return null;
                }
                base.Copy(ret);

                ret.InitWithDuration(m_fDuration, m_EndQ);
                return ret;
            }
            return new RotateTo(this);
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);

            Vector3 scale = new Vector3();
            Vector3 trans = new Vector3();
            target.WorldMatrix.Decompose(out scale, out m_StartQ, out trans);

            /*
            // Calculate X
            m_fStartAngleX = m_pTarget.RotationX;
            if (m_fStartAngleX > 0)
            {
                m_fStartAngleX = m_fStartAngleX % 360.0f;
            }
            else
            {
                m_fStartAngleX = m_fStartAngleX % -360.0f;
            }

            m_fDiffAngleX = m_fDstAngleX - m_fStartAngleX;
            if (m_fDiffAngleX > 180)
            {
                m_fDiffAngleX -= 360;
            }
            if (m_fDiffAngleX < -180)
            {
                m_fDiffAngleX += 360;
            }

            //Calculate Y: It's duplicated from calculating X since the rotation wrap should be the same
            m_fStartAngleY = m_pTarget.RotationY;

            if (m_fStartAngleY > 0)
            {
                m_fStartAngleY = m_fStartAngleY % 360.0f;
            }
            else
            {
                m_fStartAngleY = m_fStartAngleY % -360.0f;
            }

            m_fDiffAngleY = m_fDstAngleY - m_fStartAngleY;
            if (m_fDiffAngleY > 180)
            {
                m_fDiffAngleY -= 360;
            }

            if (m_fDiffAngleY < -180)
            {
                m_fDiffAngleY += 360;
            }

            //Calculate Z: It's duplicated from calculating X since the rotation wrap should be the same
            // TODO m_fStartAngleZ = m_pTarget.RotationZ;

            if (m_fStartAngleZ > 0)
            {
                m_fStartAngleZ = m_fStartAngleZ % 360.0f;
            }
            else
            {
                m_fStartAngleZ = m_fStartAngleZ % -360.0f;
            }

            m_fDiffAngleZ = m_fDstAngleZ - m_fStartAngleZ;
            if (m_fDiffAngleZ > 180)
            {
                m_fDiffAngleZ -= 360;
            }

            if (m_fDiffAngleZ < -180)
            {
                m_fDiffAngleZ += 360;
            }
            */
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

                // Make sure to only modify the rotate matrix
                var translateMatrix = Matrix.CreateTranslation(trans);
                var rotate = Quaternion.Lerp(m_StartQ, m_EndQ, time);
                var rotateMatrix = Matrix.CreateFromQuaternion(rotate);
                var scaleMatrix = Matrix.CreateScale(scale);

                // Update our 3d model's world matrix
                m_pTarget.WorldMatrix = scaleMatrix * rotateMatrix * translateMatrix;

                // If we have a physics entity, have it follow the rotation
                if (m_pTarget.PhysicsEntity != null)
                {
                    m_pTarget.PhysicsEntity.Orientation = ConversionHelper.MathConverter.Convert(rotate); 
                }
            }
        }
    }
}