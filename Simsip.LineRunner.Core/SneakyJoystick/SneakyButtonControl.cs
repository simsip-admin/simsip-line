using Cocos2D;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Simsip.LineRunner.SneakyJoystick
{
    public class SneakyButtonControl : CCNode
    {
        private float _radius; // Optimizations (keep Squared values of all radii for faster calculations) (updated internally when changing radii)
        private float _radiusSq;
        private bool _isHoldable;
        private bool _isToggleable;
        private float _rateLimit;

        protected bool _value;
        protected bool _active;
        protected bool _status;

        public SneakyButtonControl(CCRect rect, int id)
        {
            Position = rect.Origin;
            ID = id;

            // Defaults
            _status = true; 
            _value = false;
            _active = false;
            _isHoldable = true;
            _isToggleable = false;
            _radius = 32.0f;
            _rateLimit = 1.0f / 120.0f;

            buttonEvent = new EventCustom(SneakyPanelControl.BUTTON_LISTENER_ID);
        }

        #region Properties

        public int ID { get; set; }
        public bool IsDebug { get; set; }

        #endregion

        #region Events

        public event SneakyStartEndActionDelegate SneakyStartEndEvent;

        EventCustom buttonEvent;

        #endregion

        #region Cocos2d overrides

        public override void Draw()
        {
            base.Draw();
            CCDrawingPrimitives.Begin();
            CCDrawingPrimitives.DrawRect(new CCRect(0, 0, this.ContentSize.Width, this.ContentSize.Height), CCColor4B.Blue);
            CCDrawingPrimitives.End();
        }

        #endregion

        #region Api

        public void SetRadius(float r)
        {
            _radius = r;
            _radiusSq = r * r;
        }

        #endregion

        #region Touch implementation

        public virtual bool OnTouchesBegan(List<CCTouch> touches)
        {
            CCTouch touch = touches.FirstOrDefault();

            // TODO: Layer.ScreenToWorldspace(touch.LocationOnScreen);
            // TODO: location = WorldToParentspace(location);
            CCPoint location = this.ConvertTouchToNodeSpace(touch);

            // Adjust the location to be relative to the button's origin
            location -= BoundingBox.Origin;
			//Console.WriteLine(location + " radius: " + radius);
            //Do a fast rect check before doing a circle hit check:
            if (location.X < -_radius || location.X > _radius || location.Y < -_radius || location.Y > _radius)
            {
                return false;
            }
            else
            {

                float dSq = location.X * location.X + location.Y * location.Y;
#if !NETFX_CORE
				Console.WriteLine(location + " radius: " + _radius + " distanceSquared: " + dSq);
#endif

                if (_radiusSq > dSq)
                {
                    _active = true;
                    if (!_isHoldable && !_isToggleable)
                    {
                        _value = true;
                        //Schedule(limiter, rateLimit);
                    }
                    if (_isHoldable) _value = true;
                    if (_isToggleable) _value = !_value;

                    // Fire off our event to notify that movement was started
                    // TODO:
                    // buttonEvent.UserData = new SneakyButtonEventResponse(SneakyButtonStatus.Press, ID, this);
                    // DispatchEvent(buttonEvent);
                    buttonEvent.UserData = new SneakyButtonEventResponse(SneakyButtonStatus.Press, ID, this);
                    SneakyStartEndEvent(this, buttonEvent);

                    return true;
                }
            }
            return false;
        }

        public virtual void OnTouchesMoved(List<CCTouch> touches)
        {
            //base.TouchMoved(touch, touchEvent);
            CCTouch touch = touches.FirstOrDefault();

            if (!_active) return;

            // TODO Layer.ScreenToWorldspace(touch.LocationOnScreen);
            // TODO location = WorldToParentspace(location);
            CCPoint location = this.ConvertTouchToNodeSpace(touch);

#if !NETFX_CORE
			Console.WriteLine("Moved: " + location + " radius: " + _radius);
#endif
			//Console.WriteLine(location);
            //Do a fast rect check before doing a circle hit check:
            if (location.X < -_radius || location.X > _radius || location.Y < -_radius || location.Y > _radius)
            {
                return;
            }
            else
            {
                float dSq = location.X * location.X + location.Y * location.Y;
                if (_radiusSq > dSq)
                {
                    if (_isHoldable) _value = true;
                }
                else
                {
                    if (_isHoldable) _value = false; _active = false;
                }
            }
        }

        public virtual void OnTouchesEnded(List<CCTouch> touches)
        {
            CCTouch touch = touches.FirstOrDefault();

            if (!_active) return;
            if (_isHoldable) _value = false;
            if (_isHoldable || _isToggleable) _active = false;

            // Fire off our event to notify that movement was started
            // TODO
            // buttonEvent.UserData = new SneakyButtonEventResponse(SneakyButtonStatus.Release, ID, this);
            // DispatchEvent(buttonEvent);
            buttonEvent.UserData = new SneakyButtonEventResponse(SneakyButtonStatus.Release, ID, this);
            SneakyStartEndEvent(this, buttonEvent);
        }

        public virtual void OnTouchesCancelled(List<CCTouch> touches)
        {
            OnTouchesEnded(touches /* TODO, touchEvent*/);
        }

        #endregion

    }
}
