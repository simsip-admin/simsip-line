using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;

namespace Simsip.LineRunner.SneakyJoystick
{


    public class SneakyJoystickControl : CCNode
    {
        #region Custom Events

        // TODO
        public event SneakyStartEndActionDelegate SneakyStartEndEvent;

        // TODO
        EventCustom joystickEvent = new EventCustom(SneakyPanelControl.JOY_LISTENER_ID);

        #endregion

        #region Private properties

        private bool isDPad;
        private float joystickRadius;
        private float thumbRadius;
        private float deadRadius; //Size of deadzone in joystick (how far you must move before input starts). Automatically set if isDpad == YES



        private bool isMoving;

        private CCRect ControlSize { get; set; }

        #endregion

        #region Public properties

        public CCPoint StickPosition { get; set; }

        public CCPoint StickDirection { get; set; }

        public CCPoint Center { get; set; }

        public CCPoint StickPreviousPosition { get; set; }
        public static CCPoint Velocity { get; set; }

        public float Degrees { get; set; }
        public bool AutoCenter { get; set; }

        public float JoystickRadiusSq { get; set; } //Optimizations (keep Squared values of all radii for faster calculations) (updated internally when changing joy/thumb radii)

        public float JoystickRadius
        {
            get
            {
                return joystickRadius;
            }
            set
            {
                JoystickRadiusSq = value * value;
                joystickRadius = value;

            }
        }

        public float ThumbRadius
        {
            get
            {
                return thumbRadius;
            }
            set
            {
                ThumbRadiusSq = value * value;
                thumbRadius = value;
            }
        }
        public float ThumbRadiusSq { get; set; }

        public float DeadRadius
        {
            get
            {
                return deadRadius;
            }
            set
            {
                DeadRadiusSq = value * value;
                deadRadius = value;
            }
        }
        public float DeadRadiusSq { get; set; }


        public bool IsDebug;

        //DPAD =====================================================

        public bool IsDPad
        {
            get
            {
                return isDPad;
            }
            set
            {

                isDPad = value;
                if (isDPad)
                {
                    HasDeadzone = true;
                    deadRadius = 10.0f;
                }
            }
        }

        public bool HasDeadzone { get; set; } //Turns Deadzone on/off for joystick, always YES if ifDpad == YES
        public int NumberOfDirections { get; set; } //Used only when isDpad == YES

        //DIRECTIONS ===============================================

        public bool HasAnyDirection
        {
            get
            {
                return (IsDown || IsLeft || IsUp || IsRight);

            }
        }

        public bool IsUp
        {
            get
            {
                return StickDirection.Y == 1;
            }
        }
        public bool IsRight
        {
            get
            {
                return StickDirection.X == 1;
            }
        }
        public bool IsLeft
        {
            get
            {
                return StickDirection.X == -1;
            }
        }
        public bool IsDown
        {
            get
            {
                return StickDirection.Y == -1;
            }
        }
        public bool IsUpLeft
        {
            get
            {
                return StickDirection.Y == 1 && StickDirection.X == -1;
            }
        }
        public bool IsUpRight
        {
            get
            {
                return StickDirection.Y == 1 && StickDirection.X == 1;
            }
        }
        public bool IsDownLeft
        {
            get
            {
                return StickDirection.Y == -1 && StickDirection.X == -1;
            }
        }
        public bool IsDownRight
        {
            get
            {
                return StickDirection.Y == 1 && StickDirection.X == 1;
            }
        }

        #endregion

        public SneakyJoystickControl(CCRect rect) : base()
        {

            Degrees = 0.0f;
            Velocity = CCPoint.Zero;
            AutoCenter = true;

            HasDeadzone = false;
            NumberOfDirections = 4;

            isDPad = false;
            joystickRadius = rect.Size.Width / 2;

            ThumbRadius = 32.0f;
            DeadRadius = 0.0f;

            AnchorPoint = CCPoint.AnchorMiddle;

			this.ControlSize = rect;
        }

        public override void OnEnter()
        {
            base.OnEnter();

			var rect = ControlSize;

			this.ContentSize = rect.Size;
			this.Position = rect.Center;

			this.StickPosition = ContentSize.Center;
			this.Center = ContentSize.Center;
		}

        public virtual void UpdateVelocity(CCPoint point)
        {
            // Calculate distance and angle from the center.
            float dx = point.X - ContentSize.Width / 2;
            float dy = point.Y - ContentSize.Height / 2;
            float dSq = dx * dx + dy * dy;

            if (dSq <= DeadRadiusSq)
            {
                Velocity = CCPoint.Zero;
                Degrees = 0.0f;
                StickPosition = point;
                return;
            }

            float angle = (float)Math.Atan2(dy, dx); // in radians
            if (angle < 0)
            {
                // TODO
                // angle += CCMathHelper.TwoPi;
                angle += Microsoft.Xna.Framework.MathHelper.TwoPi;
            }
            float cosAngle;
            float sinAngle;

            if (isDPad)
            {
                float anglePerSector = 360.0f / CCMathHelper.ToRadians(NumberOfDirections); //  NumberOfDirections * ((float)Math.PI / 180.0f);
                angle = (float)Math.Round(angle / anglePerSector) * anglePerSector;
            }

            cosAngle = CCMathHelper.Cos(angle);
            sinAngle = CCMathHelper.Sin(angle);

            // NOTE: Velocity goes from -1.0 to 1.0.
            if (dSq > JoystickRadiusSq || isDPad)
            {
                dx = cosAngle * joystickRadius;
                dy = sinAngle * joystickRadius;
            }

            Velocity = new CCPoint(dx / joystickRadius, dy / joystickRadius);
            Degrees = CCMathHelper.ToDegrees(angle);

            // Update the thumb's position
			var newLoc = new CCPoint(dx + ContentSize.Width / 2, dy + ContentSize.Height / 2);
			StickPosition = newLoc;
        }


        public virtual void OnTouchesBegan(List<CCTouch> touches /* TODO , CCEvent touchEvent */)
        {
            CCTouch touch = touches.First();

            // TODO Layer.ScreenToWorldspace(touch.LocationOnScreen);
			// TODO: location = WorldToParentspace(location);
            CCPoint location = this.ConvertTouchToNodeSpace(touch);

            //Do a fast rect check before doing a circle hit check:
            if (location.X - joystickRadius < -joystickRadius || location.X - joystickRadius > joystickRadius || location.Y - joystickRadius < -joystickRadius || location.Y - joystickRadius > joystickRadius)
            {
                return;
            }
            else
            {
                float dSq = (location.X - joystickRadius) * (location.X - joystickRadius) + (location.Y - joystickRadius) * (location.Y - joystickRadius);
				if (JoystickRadiusSq > dSq)
                {
                    isMoving = true;

                    UpdateVelocity(location);

                    // TODO
                    // Fire off our event to notify that movement was started
                    // joystickEvent.UserData = new SneakyJoystickEventResponse(SneakyJoystickMovementStatus.Start, null);
                    // DispatchEvent(joystickEvent);
                    joystickEvent.UserData = new SneakyJoystickEventResponse(SneakyJoystickMovementStatus.Start, null);
                    SneakyStartEndEvent(this, joystickEvent);

                    return;
                }
            }

        }

        public virtual void OnTouchesMoved(List<CCTouch> touches /* TODO , CCEvent object touchEvent*/)
        {
            //base.TouchesMoved(touches, touchEvent);
            if (!isMoving)
            {
                return;
            }

            CCTouch touch = touches.First();

            // TODO Layer.ScreenToWorldspace(touch.LocationOnScreen);
            // TODO location = WorldToParentspace(location);
            CCPoint location = this.ConvertTouchToNodeSpace(touch);

            //Check direction
            StickDirection = new CCPoint(
                (location.X > Center.X) ? 1 : (location.X < Center.X) ? -1 : 0,
                (location.Y > Center.Y) ? 1 : (location.Y < Center.Y) ? -1 : 0
                );

            StickPreviousPosition = location;

            // Fire off our event to notify that movement was started
            // TODO joystickEvent.UserData = new SneakyJoystickEventResponse(SneakyJoystickMovementStatus.OnMove, StickDirection);
            // TODO DispatchEvent(joystickEvent);
            joystickEvent.UserData = new SneakyJoystickEventResponse(SneakyJoystickMovementStatus.OnMove, StickDirection);
            SneakyStartEndEvent(this, joystickEvent);

            UpdateVelocity(location);
        }

        public virtual void OnTouchesCancelled(List<CCTouch> touches /* TODO , CCEvent object touchEvent*/)
        {
            OnTouchesEnded(touches /*TODO , touchEvent*/);
        }

        public virtual void OnTouchesEnded(List<CCTouch> touches /* TODO , CCEvent touchEvent*/)
        {
            if (!isMoving)
                return;

            ResetDirections();

            isMoving = false;

            CCTouch touch = touches.First();

            CCPoint location = new CCPoint(ContentSize.Width / 2, ContentSize.Height / 2);
            if (!AutoCenter)
            {
                // TODO
                // location = Layer.ScreenToWorldspace(touch.LocationOnScreen);
                // location = WorldToParentspace(location);
                location = this.ConvertTouchToNodeSpace(touch);
            }

            UpdateVelocity(location);

            // TODO
            // Fire off our event to notify that movement has ended.
            // joystickEvent.UserData = new SneakyJoystickEventResponse(SneakyJoystickMovementStatus.End, null);
            // DispatchEvent(joystickEvent);
            joystickEvent.UserData = new SneakyJoystickEventResponse(SneakyJoystickMovementStatus.End, null);
            SneakyStartEndEvent(this, joystickEvent);
        }

        public void ResetDirections()
        {
            StickDirection = CCPoint.Zero;
        }

        public void ForceCenter()
        {
            // StickPosition = new CCPoint(Center.X, Center.Y);
        }

        public CCPoint GetNextPositionFromImage(CCNode node, float dt, CCSize wSize)
        {
            return GetPositionFromVelocity(Velocity, node, dt, wSize);
        }

        public void RefreshImagePosition(CCNode node, float dt, CCSize wSize)
        {
            if (node != null)
                node.Position = GetPositionFromVelocity(Velocity, node, dt, wSize);
        }


        #region Static methods

        public static CCPoint GetPositionFromVelocity(CCPoint velocity, CCNode node, float dt, CCSize winSize)
        {
			return GetPositionFromVelocity(velocity, node.Position, node.BoundingBox.Size, winSize, dt);
        }

        public static CCPoint GetPositionFromVelocity(CCPoint velocity, CCPoint actualPosition, CCSize size, CCSize winSize, float dt)
        {
            return GetPositionFromVelocity(velocity, actualPosition, size.Width, size.Height, winSize.Width, winSize.Height, dt);
        }

        public static CCPoint GetPositionFromVelocity(CCPoint velocity, CCPoint actualPosition, float Width, float Height, float maxWindowWidth, float maxWindowHeight, float dt)
        {

            CCPoint scaledVelocity = velocity * 240;
			CCPoint newPosition = new CCPoint(actualPosition.X + scaledVelocity.X * dt, actualPosition.Y + scaledVelocity.Y * dt);

            if (newPosition.Y > maxWindowHeight - Height / 2)
            {
                newPosition.Y = maxWindowHeight - Height / 2;
            }
            if (newPosition.Y < (0 + Height / 2))
            {
                newPosition.Y = (0 + Height / 2);
            }

            if (newPosition.X > maxWindowWidth - Width / 2)
            {
                newPosition.X = maxWindowWidth - Width / 2;
            }
            if (newPosition.X < (0 + Width / 2))
            {
                newPosition.X = (0 + Width / 2);
            }

            return newPosition;

        }


        public override void Draw()
        {
            base.Draw();

            if (!IsDebug)
                return;

            CCRect rect = new CCRect(0, 0, ContentSize.Width, ContentSize.Height);

//            CCPoint[] vertices = new CCPoint[] {
//        new CCPoint(rect.Origin.X,rect.Origin.Y),
//        new CCPoint(rect.Origin.X+rect.Size.Width,rect.Origin.Y),
//        new CCPoint(rect.Origin.X+rect.Size.Width,rect.Origin.Y+rect.Size.Height),
//        new CCPoint(rect.Origin.X,rect.Origin.Y+rect.Size.Height),
//    };
//
//            CCDrawingPrimitives.Begin();
//            CCDrawingPrimitives.DrawCircle(AnchorPointInPoints, 10, 0, 8, true, CCColor4B.Blue);
//            CCDrawingPrimitives.DrawSolidPoly(vertices, 4, CCColor4B.Red, true);
//            CCDrawingPrimitives.End();

        }

        #endregion

    }
}
