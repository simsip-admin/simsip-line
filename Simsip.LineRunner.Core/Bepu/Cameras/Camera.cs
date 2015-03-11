using System;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos
{
    /// <summary>
    /// Simple camera class for moving around the demos area.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Gets or sets the position of the camera.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the ProjectionMatrix matrix of the camera.
        /// </summary>
        public Matrix ProjectionMatrix { get; set; }

        /// <summary>
        /// Gets the view matrix of the camera.
        /// </summary>
        public Matrix ViewMatrix
        {
            // TODO:
            // get { return Matrix.CreateViewRH(Position, _viewDirection, lockedUp); }
            // original in Voxeliq
            // get { return Matrix.CreateLookAt(this.Position, this._viewDirection, this.lockedUp);  }
            // From BEPUutilities.Matrix
            get { return this.CreateViewRH(Position, _viewDirection, lockedUp); }
        }

        /// <summary>
        /// Gets the world transformation of the camera.
        /// </summary>
        public Matrix WorldMatrix
        {
            // TODO:
            // get { return Matrix.CreateWorldRH(Position, _viewDirection, lockedUp); }
            // From BEPUutilities.Matrix
            get { return this.CreateWorldRH(Position, _viewDirection, lockedUp); }

        }

        private Vector3 _target = Vector3.Forward;
        /// <summary>
        /// Gets or sets the target of the camera.
        /// 
        /// IMPORTANT: Will also set the ViewDirection of the camera.
        /// </summary>
        public Vector3 Target
        {
            get { return _target; }
            set
            {
                _target = value;
                ViewDirection = _target - Position;
            }
        }

        private Vector3 _viewDirection = Vector3.Forward;
        /// <summary>
        /// Gets or sets the view direction of the camera.
        /// </summary>
        public Vector3 ViewDirection
        {
            get { return _viewDirection; }
            set
            {
                float lengthSquared = value.LengthSquared();
                if (lengthSquared > BEPUutilities.Toolbox.Epsilon)
                {
                    Vector3.Divide(ref value, (float) Math.Sqrt(lengthSquared), out value);
                    //Validate the input. A temporary violation of the maximum pitch is permitted as it will be fixed as the user looks around.
                    //However, we cannot allow a view direction parallel to the locked up direction.
                    float dot;
                    Vector3.Dot(ref value, ref lockedUp, out dot);
                    if (Math.Abs(dot) > 1 - BEPUutilities.Toolbox.BigEpsilon)
                    {
                        //The view direction must not be aligned with the locked up direction.
                        //Silently fail without changing the view direction.
                        return;
                    }
                    _viewDirection = value;
                }
            }
        }

        private float maximumPitch = MathHelper.PiOver2 * 0.99f;
        /// <summary>
        /// Gets or sets how far the camera can look up or down in radians.
        /// </summary>
        public float MaximumPitch
        {
            get { return maximumPitch; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Maximum pitch corresponds to pitch magnitude; must be positive.");
                if (value >= MathHelper.PiOver2)
                    throw new ArgumentException("Maximum pitch must be less than Pi/2.");
                maximumPitch = value;
            }
        }

        private Vector3 lockedUp = Vector3.Up;
        /// <summary>
        /// Gets or sets the current locked up vector of the camera.
        /// </summary>
        public Vector3 LockedUp
        {
            get { return lockedUp; }
            set
            {
                var oldUp = lockedUp;
                float lengthSquared = value.LengthSquared();
                if (lengthSquared > BEPUutilities.Toolbox.Epsilon)
                {
                    Vector3.Divide(ref value, (float)Math.Sqrt(lengthSquared), out lockedUp);
                    //Move the view direction with the transform. This helps guarantee that the view direction won't end up aligned with the up vector.
                    /* TODO
                    Quaternion rotation;
                    Quaternion.GetQuaternionBetweenNormalizedVectors(ref oldUp, ref lockedUp, out rotation);
                    Quaternion.Transform(ref _viewDirection, ref rotation, out _viewDirection);
                    */
                    Quaternion rotation;
                    // Copied from BEPUutilities.Quarternion
                    this.GetQuaternionBetweenNormalizedVectors(ref oldUp, ref lockedUp, out rotation);
                    this.Transform(ref _viewDirection, ref rotation, out _viewDirection);

                }
                //If the new up vector was a near-zero vector, silently fail without changing the up vector.
            }
        }

        /// <summary>
        /// Creates a camera.
        /// </summary>
        /// <param name="position">Initial position of the camera.</param>
        /// <param name="pitch">Initial pitch angle of the camera.</param>
        /// <param name="yaw">Initial yaw value of the camera.</param>
        /// <param name="projectionMatrix">ProjectionMatrix matrix used.</param>
        public Camera(Vector3 position, float pitch, float yaw, Matrix projectionMatrix)
        {
            Position = position;
            Yaw(yaw);
            Pitch(pitch);
            ProjectionMatrix = projectionMatrix;
        }

        /// <summary>
        /// Moves the camera forward.
        /// </summary>
        /// <param name="distance">Distance to move.</param>
        public void MoveForward(float distance)
        {
            Position += WorldMatrix.Forward * distance;
        }

        /// <summary>
        /// Moves the camera to the right.
        /// </summary>
        /// <param name="distance">Distance to move.</param>
        public void MoveRight(float distance)
        {
            Position += WorldMatrix.Right * distance;
        }

        /// <summary>
        /// Moves the camera up.
        /// </summary>
        /// <param name="distance">Distance to move.</param>
        public void MoveUp(float distance)
        {
            Position += new Vector3(0, distance, 0);
        }

        /// <summary>
        /// Rotates the camera around its locked up vector.
        /// </summary>
        /// <param name="radians">Amount to rotate.</param>
        public void Yaw(float radians)
        {
            //Rotate around the up vector.
            /* TODO: http://gamedev.stackexchange.com/questions/16053/does-xna-have-3x3-matrix
            Matrix3x3 rotation;
            Matrix3x3.CreateFromAxisAngle(ref lockedUp, radians, out rotation);
            Matrix3x3.Transform(ref _viewDirection, ref rotation, out _viewDirection);
            */
            Matrix rotation = Matrix.Identity;
            Matrix.CreateFromAxisAngle(ref lockedUp, radians, out rotation);
            Vector3.Transform(ref _viewDirection, ref rotation, out _viewDirection);

            //Avoid drift by renormalizing.
            _viewDirection.Normalize();
        }

        /// <summary>
        /// Rotates the view direction up or down relative to the locked up vector.
        /// </summary>
        /// <param name="radians">Amount to rotate.</param>
        public void Pitch(float radians)
        {
            //Do not allow the new view direction to violate the maximum pitch.
            float dot;
            Vector3.Dot(ref _viewDirection, ref lockedUp, out dot);

            //While this could be rephrased in terms of dot products alone, converting to actual angles can be more intuitive.
            //Consider +Pi/2 to be up, and -Pi/2 to be down.
            float currentPitch = (float)Math.Acos(MathHelper.Clamp(-dot, -1, 1)) - MathHelper.PiOver2;
            //Compute our new pitch by clamping the current + change.
            float newPitch = MathHelper.Clamp(currentPitch + radians, -maximumPitch, maximumPitch);
            float allowedChange = newPitch - currentPitch;

            //Compute and apply the rotation.
            Vector3 pitchAxis;
            Vector3.Cross(ref _viewDirection, ref lockedUp, out pitchAxis);
            //This is guaranteed safe by all interaction points stopping ViewDirection from being aligned with lockedUp.
            pitchAxis.Normalize();
            /* TODO: http://gamedev.stackexchange.com/questions/16053/does-xna-have-3x3-matrix
            Matrix3x3 rotation;
            Matrix3x3.CreateFromAxisAngle(ref pitchAxis, allowedChange, out rotation);
            Matrix3x3.Transform(ref _viewDirection, ref rotation, out _viewDirection);
            */
            Matrix rotation = Matrix.Identity;
            Matrix.CreateFromAxisAngle(ref pitchAxis, allowedChange, out rotation);
            Vector3.Transform(ref _viewDirection, ref rotation, out _viewDirection);

            //Avoid drift by renormalizing.
            _viewDirection.Normalize();
        }

        // From BEPUutilities.Matrix
        /// <summary>
        /// Creates a view matrix pointing looking in a direction with a given up vector.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="forward">Forward direction of the camera.</param>
        /// <param name="upVector">Up vector of the camera.</param>
        /// <param name="viewMatrix">Look at matrix.</param>
        public void CreateViewRH(ref Vector3 position, ref Vector3 forward, ref Vector3 upVector, out Matrix viewMatrix)
        {
            Vector3 z;
            float length = forward.Length();
            Vector3.Divide(ref forward, -length, out z);
            Vector3 x;
            Vector3.Cross(ref upVector, ref z, out x);
            x.Normalize();
            Vector3 y;
            Vector3.Cross(ref z, ref x, out y);

            viewMatrix.M11 = x.X;
            viewMatrix.M12 = y.X;
            viewMatrix.M13 = z.X;
            viewMatrix.M14 = 0f;
            viewMatrix.M21 = x.Y;
            viewMatrix.M22 = y.Y;
            viewMatrix.M23 = z.Y;
            viewMatrix.M24 = 0f;
            viewMatrix.M31 = x.Z;
            viewMatrix.M32 = y.Z;
            viewMatrix.M33 = z.Z;
            viewMatrix.M34 = 0f;
            Vector3.Dot(ref x, ref position, out viewMatrix.M41);
            Vector3.Dot(ref y, ref position, out viewMatrix.M42);
            Vector3.Dot(ref z, ref position, out viewMatrix.M43);
            viewMatrix.M41 = -viewMatrix.M41;
            viewMatrix.M42 = -viewMatrix.M42;
            viewMatrix.M43 = -viewMatrix.M43;
            viewMatrix.M44 = 1f;
        }

        // From BEPUutilities.Matrix
        /// <summary>
        /// Creates a view matrix pointing looking in a direction with a given up vector.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="forward">Forward direction of the camera.</param>
        /// <param name="upVector">Up vector of the camera.</param>
        /// <returns>Look at matrix.</returns>
        public Matrix CreateViewRH(Vector3 position, Vector3 forward, Vector3 upVector)
        {
            Matrix lookat;
            CreateViewRH(ref position, ref forward, ref upVector, out lookat);
            return lookat;
        }

        // From BEPUutilities.Matrix
        /// <summary>
        /// Creates a world matrix pointing from a position to a target with the given up vector.
        /// </summary>
        /// <param name="position">Position of the transform.</param>
        /// <param name="forward">Forward direction of the transformation.</param>
        /// <param name="upVector">Up vector which is crossed against the forward vector to compute the transform's basis.</param>
        /// <param name="worldMatrix">World matrix.</param>
        public void CreateWorldRH(ref Vector3 position, ref Vector3 forward, ref Vector3 upVector, out Matrix worldMatrix)
        {
            Vector3 z;
            float length = forward.Length();
            Vector3.Divide(ref forward, -length, out z);
            Vector3 x;
            Vector3.Cross(ref upVector, ref z, out x);
            x.Normalize();
            Vector3 y;
            Vector3.Cross(ref z, ref x, out y);

            worldMatrix.M11 = x.X;
            worldMatrix.M12 = x.Y;
            worldMatrix.M13 = x.Z;
            worldMatrix.M14 = 0f;
            worldMatrix.M21 = y.X;
            worldMatrix.M22 = y.Y;
            worldMatrix.M23 = y.Z;
            worldMatrix.M24 = 0f;
            worldMatrix.M31 = z.X;
            worldMatrix.M32 = z.Y;
            worldMatrix.M33 = z.Z;
            worldMatrix.M34 = 0f;

            worldMatrix.M41 = position.X;
            worldMatrix.M42 = position.Y;
            worldMatrix.M43 = position.Z;
            worldMatrix.M44 = 1f;
        }

        // From BEPUutilities.Matrix
        /// <summary>
        /// Creates a world matrix pointing from a position to a target with the given up vector.
        /// </summary>
        /// <param name="position">Position of the transform.</param>
        /// <param name="forward">Forward direction of the transformation.</param>
        /// <param name="upVector">Up vector which is crossed against the forward vector to compute the transform's basis.</param>
        /// <returns>World matrix.</returns>
        public Matrix CreateWorldRH(Vector3 position, Vector3 forward, Vector3 upVector)
        {
            Matrix lookat;
            CreateWorldRH(ref position, ref forward, ref upVector, out lookat);
            return lookat;
        }

        // From BEPUutilities.Quarternion
        /// <summary>
        /// Transforms the vector using a quaternion.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public void Transform(ref Vector3 v, ref Quaternion rotation, out Vector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;
            float xx2 = rotation.X * x2;
            float xy2 = rotation.X * y2;
            float xz2 = rotation.X * z2;
            float yy2 = rotation.Y * y2;
            float yz2 = rotation.Y * z2;
            float zz2 = rotation.Z * z2;
            float wx2 = rotation.W * x2;
            float wy2 = rotation.W * y2;
            float wz2 = rotation.W * z2;
            //Defer the component setting since they're used in computation.
            float transformedX = v.X * (1f - yy2 - zz2) + v.Y * (xy2 - wz2) + v.Z * (xz2 + wy2);
            float transformedY = v.X * (xy2 + wz2) + v.Y * (1f - xx2 - zz2) + v.Z * (yz2 - wx2);
            float transformedZ = v.X * (xz2 - wy2) + v.Y * (yz2 + wx2) + v.Z * (1f - xx2 - yy2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }

        // From BEPUutilities.Quarternion
        /// <summary>
        /// Computes the quaternion rotation between two normalized vectors.
        /// </summary>
        /// <param name="v1">First unit-length vector.</param>
        /// <param name="v2">Second unit-length vector.</param>
        /// <param name="q">Quaternion representing the rotation from v1 to v2.</param>
        public void GetQuaternionBetweenNormalizedVectors(ref Vector3 v1, ref Vector3 v2, out Quaternion q)
        {
            float dot;
            Vector3.Dot(ref v1, ref v2, out dot);
            //For non-normal vectors, the multiplying the axes length squared would be necessary:
            //float w = dot + (float)Math.Sqrt(v1.LengthSquared() * v2.LengthSquared());
            if (dot < -0.9999f) //parallel, opposing direction
            {
                //If this occurs, the rotation required is ~180 degrees.
                //The problem is that we could choose any perpendicular axis for the rotation. It's not uniquely defined.
                //The solution is to pick an arbitrary perpendicular axis.
                //Project onto the plane which has the lowest component magnitude.
                //On that 2d plane, perform a 90 degree rotation.
                float absX = Math.Abs(v1.X);
                float absY = Math.Abs(v1.Y);
                float absZ = Math.Abs(v1.Z);
                if (absX < absY && absX < absZ)
                    q = new Quaternion(0, -v1.Z, v1.Y, 0);
                else if (absY < absZ)
                    q = new Quaternion(-v1.Z, 0, v1.X, 0);
                else
                    q = new Quaternion(-v1.Y, v1.X, 0, 0);
            }
            else
            {
                Vector3 axis;
                Vector3.Cross(ref v1, ref v2, out axis);
                q = new Quaternion(axis.X, axis.Y, axis.Z, dot + 1);
            }
            q.Normalize();
        }
    }
}