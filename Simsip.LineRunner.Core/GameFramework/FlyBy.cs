using ConversionHelper;
using Engine.Graphics;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameObjects;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using Engine.Input;


namespace Simsip.LineRunner.GameFramework
{
    public enum FlyByCameraUpdate
    {
        TrackingOnly,
        TrackingAndStationaryHeightOnly
    }
    public enum FlyByTargetAttachment
    {
        UseModel,
        UsePhysicsBody
    }

    public class FlyBy
    {
        private IInputManager _inputManager;

        private float _duration;
        private Vector3 _bezStartPosition;
        private Vector3 _bezMidPosition;
        private Vector3 _bezEndPosition;
        private Vector3 _bezStartTarget;
        private Vector3 _bezEndTarget;

        private BezierPath _bezPath;
        private BezierPath _targetPath;

        private FlyByCameraUpdate _cameraUpdate;
        private GameModel _targetAttachment;
        private FlyByTargetAttachment _targetAttachmentUse;

        public float BezTime { get; private set; }

        /// <summary>
        /// Allow for chaining flybys together.
        /// 
        /// Set to the next flyby you want to play and then start it in the FlyByFinished event handler.
        /// </summary>
        public FlyBy NextFlyBy { get; set; }

        public event FlyByFinishedEventHandler FlyByFinished;

        public FlyBy(FlyByCameraUpdate cameraUpdate=FlyByCameraUpdate.TrackingOnly,
                     GameModel targetAttachment=null, 
                     FlyByTargetAttachment targetAttachmentUse=FlyByTargetAttachment.UsePhysicsBody)
        {
            this._cameraUpdate = cameraUpdate;
            this._targetAttachment = targetAttachment;
            this._targetAttachmentUse = targetAttachmentUse;

            this.BezTime = 0.0f;
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
        }

        public bool Finished
        {
            get
            {
                return BezTime > 1.0;
            }
        }

        public void InitBezierControlPoints(float duration,
                                            List<Vector3> controlPoints,
                                            List<Vector3> targetPoints=null)
        {
            this._duration = duration;
            this._bezPath = new BezierPath();
            this._bezPath.SetControlPoints(controlPoints);

            if (targetPoints != null)
            {
                this._targetPath = new BezierPath();
                this._targetPath.SetControlPoints(targetPoints);
            }

            this.BezTime = 0.0f;
        }


        public void InitBezier(float duration,
                               Vector3 startPosition, 
                               Vector3 startTarget, 
                               Vector3 endPosition, 
                               Vector3 endTarget)
        {
            _bezStartPosition = startPosition;
            _bezEndPosition = endPosition;

            _bezMidPosition = (_bezStartPosition + _bezEndPosition) / 2.0f;
            Vector3 cameraDirection = endPosition - startPosition;
            Vector3 targDirection = endTarget - startTarget;
            Vector3 upVector = Vector3.Cross(new Vector3(targDirection.X, 0, targDirection.Z), new Vector3(cameraDirection.X, 0, cameraDirection.Z));
            Vector3 perpDirection = Vector3.Cross(upVector, cameraDirection);

            if (perpDirection == new Vector3())
                perpDirection = new Vector3(0, 1, 0);
            perpDirection.Normalize();

            Vector3 midShiftDirecton = new Vector3(0, 1, 0) + perpDirection;
            _bezMidPosition += cameraDirection.Length() * midShiftDirecton;

            _bezStartTarget = startTarget;
            _bezEndTarget = endTarget;

            _duration = duration;
            BezTime = 0.0f;
        }

        public void UpdateBezier(float dt)
        {
            BezTime += dt / _duration;
            if (BezTime > 1.0)
            {
                // Flag public state
                this._inputManager.TheLineRunnerControllerInput.IsInFlyBy = false;

                // Let anyone know who is interested
                var flyByFinishedEventArgs = new FlyByFinishedEventArgs
                    {
                        NextFlyBy = this.NextFlyBy
                    };
                FlyByFinished(this, flyByFinishedEventArgs);

                // Short-circuit
                return;
            }
            else
            {
                this._inputManager.TheLineRunnerControllerInput.IsInFlyBy = true;
            }

            float smoothValue = MathHelper.SmoothStep(0, 1, BezTime);
            Vector3 newCamPos = this._bezPath.CalculateBezierPoint(0, smoothValue);
            Vector3 newCamTarget = Vector3.Zero;
            if (_targetPath != null)
            {
                newCamTarget = this._targetPath.CalculateBezierPoint(0, smoothValue);
            }

            switch(this._cameraUpdate)
            {
                case FlyByCameraUpdate.TrackingOnly:
                    {
                        this._inputManager.LineRunnerCamera.Position = newCamPos;
                        this._inputManager.LineRunnerCamera.Target = newCamTarget;
                        break;
                    }
                case FlyByCameraUpdate.TrackingAndStationaryHeightOnly:
                    {
                        this._inputManager.LineRunnerCamera.Position = newCamPos;
                        this._inputManager.LineRunnerCamera.Target = newCamTarget;
                        var newStationaryPosition = new Vector3(this._inputManager.TheStationaryCamera.Position.X, 
                                                       newCamTarget.Y,
                                                       this._inputManager.TheStationaryCamera.Position.Z);
                        var newStationaryTarget = new Vector3(this._inputManager.TheStationaryCamera.Target.X, 
                                                       newCamTarget.Y,
                                                       this._inputManager.TheStationaryCamera.Target.Z);
                        this._inputManager.TheStationaryCamera.Position = newStationaryPosition;
                        this._inputManager.TheStationaryCamera.Target = newStationaryTarget;
                        break;
                    }
            }

            if (this._targetAttachment != null)
            {
                if (this._targetAttachmentUse == FlyByTargetAttachment.UseModel)
                {
                    this._targetAttachment.WorldOrigin = newCamTarget;
                }
                else if (this._targetAttachmentUse == FlyByTargetAttachment.UsePhysicsBody)
                {
                    this._targetAttachment.PhysicsEntity.Position = MathConverter.Convert(newCamTarget);

                    /*
                    var logicalPosition = XNAUtils.WorldToLogical(newCamTarget,XNAUtils.CameraType.Stationary);
                    var physicsPosition = new b2Vec2(
                        logicalPosition.X / this._physicsManager.PTM_RATIO,
                        logicalPosition.Y / this._physicsManager.PTM_RATIO);
                    this._targetAttachment.PhysicsBody.SetTransform(physicsPosition,
                                                              this._targetAttachment.PhysicsBody.Angle);
                    */
                }
            }
        }

        private Vector3 Bezier(Vector3 startPoint, Vector3 midPoint, Vector3 endPoint, float time)
        {
            float invTime = 1.0f - time;
            float timePow = (float)Math.Pow(time, 2);
            float invTimePow = (float)Math.Pow(invTime, 2);

            Vector3 result = startPoint * invTimePow;
            result += 2 * midPoint * time * invTime;
            result += endPoint * timePow;

            return result;
        }

        private void AnglesFromDirection(Vector3 direction, out float updownAngle, out float leftrightAngle)
        {
            Vector3 floorProjection = new Vector3(direction.X, 0, direction.Z);
            float directionLength = floorProjection.Length();
            updownAngle = (float)Math.Atan2(direction.Y, directionLength);
            leftrightAngle = -(float)Math.Atan2(direction.X, -direction.Z);
        }
    }

    public delegate void FlyByFinishedEventHandler(object sender, FlyByFinishedEventArgs e);

    /// <summary>
    /// Passed to the FlyByFinished event handler if assigned.
    /// 
    /// Has a field to record the next flyby in a chained set of flybys.
    /// </summary>
    public class FlyByFinishedEventArgs : EventArgs
    {
        public FlyBy NextFlyBy;
    }
}