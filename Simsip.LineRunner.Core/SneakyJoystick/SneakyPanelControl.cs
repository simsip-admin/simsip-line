using Cocos2D;
using System;
using System.Collections.Generic;


namespace Simsip.LineRunner.SneakyJoystick
{
    public delegate void SneakyStartEndActionDelegate(object sender, EventCustom e);

    public class SneakyJoystickEventResponse
    {
        public SneakyJoystickMovementStatus ResponseType;
        public object UserData;

        public SneakyJoystickEventResponse(SneakyJoystickMovementStatus responseType, object userData)
        {
            ResponseType = responseType;
            UserData = userData;
        }
    }

    public class SneakyButtonEventResponse
    {
        public SneakyButtonStatus ResponseType;
        public int ID;
        public object UserData;

        public SneakyButtonEventResponse(SneakyButtonStatus responseType, int id, object userData)
        {
            ResponseType = responseType;
            ID = id;
            UserData = userData;
        }
    }

    public enum SneakyJoystickMovementStatus
    {
        None = 0,
        Start = 1,
        OnMove = 2,
        End = 3,
    }

    public enum ButtonsOrientation
    {
        Horizontal = 1,
        Vertical = 2,
    }

    public enum SneakyButtonStatus
    {
        None = 0,
        Press = 1,
        Release = 2,
    }

    public class SneakyPanelControl : CCLayer
    {
        public const string JOY_LISTENER_ID = "SneakyJoystick_JOY_LISTENER";
        public const string BUTTON_LISTENER_ID = "SneakyJoystick_BUTTON_LISTENER";

        public SneakyPanelControl(CCSize contentSize, int buttons)
        {
            this.ContentSize = contentSize;

            this.InitializeJoyStick();

            this.Buttons = new List<SneakyButtonControlSkinnedBase>(buttons);
            // this.InitializeButtons(this.Buttons.Capacity);
        }

        #region Properties

        public SneakyJoystickControlSkinnedBase JoyControl { get; set; }

        public List<SneakyButtonControlSkinnedBase> Buttons { get; set; }

        private ButtonsOrientation _orientation;
        public ButtonsOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;
                ReorderButtons();
            }
        }

        private byte _opacity;
        public byte Opacity
        {
            get
            {
                return _opacity;
            }

            set
            {
                if (JoyControl != null)
                    JoyControl.Opacity = value;

                foreach (var item in Buttons)
                {
                    item.Opacity = value;
                }

                _opacity = value;
            }
        }

        public bool HasAnyDirection
        {
            get
            {
                return JoyControl.HasAnyDirection;
            }
        }

        public bool IsRight
        {
            get
            {
                return JoyControl.IsRight;
            }
        }
        public bool IsLeft
        {
            get
            {
                return JoyControl.IsLeft;
            }
        }
        public bool IsDown
        {
            get
            {
                return JoyControl.IsDown;
            }
        }
        public bool IsUp
        {
            get
            {
                return JoyControl.IsUp;
            }
        }
        public bool IsUpLeft
        {
            get
            {
                return JoyControl.IsUpLeft;
            }
        }
        public bool IsUpRight
        {
            get
            {
                return JoyControl.IsUpRight;
            }
        }
        public bool IsDownLeft
        {
            get
            {
                return JoyControl.IsDownLeft;
            }
        }
        public bool IsDownRight
        {
            get
            {
                return JoyControl.IsDownRight;
            }
        }

        public bool HasPlayer { get { return Player != null; } }

        public CCNode Player { get; set; }

        public CCPoint GetPlayerPosition(float dt, CCSize wSize)
        {
            if (JoyControl != null && Player != null)
                return JoyControl.GetNextPositionFromImage(Player, dt, wSize);
#if !NETFX_CORE
            Console.WriteLine("SNEAKYCONTROL > GETPLAYERPOSITION() : ERROR. NOT PLAYER ASSIGNED");
#endif
            return CCPoint.Zero;
        }

        public static CCPoint GetPlayerPosition(CCNode player, float dt, CCSize wSize)
        {
            if (player != null)
            {
                return SneakyJoystickControl.GetPositionFromVelocity(SneakyJoystickControl.Velocity, player, dt, wSize);
            }
            return CCPoint.Zero;
        }

        public bool IsListenerDisabled { get; set; }

        public bool IsDebug
        {
            get
            {
                if (JoyControl != null)
                {
                    return JoyControl.IsDebug;
                }
                return false;
            }

            set
            {
                if (JoyControl != null)
                {
                    JoyControl.IsDebug = value;
                }

                foreach (var item in Buttons)
                {
                    item.IsDebug = value;
                }
            }
        }

        #endregion

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            this.TouchMode = CCTouchMode.OneByOne;
            this.TouchEnabled = true;

            // Opacity = DEFAULT_TRANSPARENCY;

			// Joystick Init
            // TODO: Moved to constructor
			// InitializeJoyStick();

			// Buttons init
            // TODO: Moved to constructor
			// InitializeButtons(Buttons.Capacity);

            if (!IsListenerDisabled)
            {
                /* TODO
                tListener = new CCEventListenerTouchAllAtOnce();
                tListener.OnTouchesBegan = OnTouchesBegan;
                tListener.OnTouchesMoved = OnTouchesMoved;
                tListener.OnTouchesCancelled = OnTouchesCancelled;
                tListener.OnTouchesEnded = OnTouchesEnded;
                AddEventListener(tListener, this);
                */
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (!IsListenerDisabled)
            {
                // TODO
                // RemoveEventListener(tListener);
            }
        }

        public override void Draw()
        {
            CCSize visibleBoundsSize = this.ContentSize;

            if (IsDebug)
            {
                CCDrawingPrimitives.Begin();
                CCDrawingPrimitives.DrawRect(new CCRect(0, 0, visibleBoundsSize.Width, visibleBoundsSize.Height), CCColor4B.Blue);
                CCDrawingPrimitives.End();
            }
        }

        #endregion

        #region Touch implementation

        /*
        http://www.cocos2d-iphone.org/forums/topic/tutorials-dont-mention-cctouchdispatcherremovedelegate/
         
        Setting self.isTouchEnabled to YES in a CCLayer causes RegisterWithTouchDispatcher 
        to be called in onEnter, and CCDirector.SharedDirector.TouchDispatcher.RemoveDelegate(this)
        to be called in onExit.

        RegisterWithTouchDispatcher in CCLayer registers as a Standard touch delegate. 
        So you only need to override it if you want the Targeted touch messages.
            
        Note if you don't set CCTouchMode it will default to CCTouchMode.AllAtOnce, which means
        override TouchesBegan. Otherwise set CCTouchMode to CCTouchMode.OneByOne and override
        TouchBegan.
         
        In TouchBegan, If you return true then ccTouchEnded will called. 
        If you return false then ccTouchEnded will not be called, and the event 
        will go the parent layer
        */

        public override void RegisterWithTouchDispatcher()
        {
            CCDirector.SharedDirector.TouchDispatcher.AddTargetedDelegate(this, 0, true);
        }

        public override void TouchesEnded(List<CCTouch> touches)
        {
            JoyControl.OnTouchesEnded(touches /*TODO , touchEvent*/);

            foreach (var button in Buttons)

                button.OnTouchesEnded(touches /*TODO, touchEvent*/);
        }

        public override void TouchesCancelled(List<CCTouch> touches)
        {
            JoyControl.OnTouchesCancelled(touches /*TODO , touchEvent*/);

            foreach (var button in Buttons)
            {
                button.OnTouchesCancelled(touches /*TODO , touchEvent*/);
            }
        }

        public override void TouchesMoved(List<CCTouch> touches)
        {
            JoyControl.OnTouchesMoved(touches /*TODO , touchEvent*/);

            foreach (var button in Buttons)
            {
                button.OnTouchesMoved(touches /*TODO , touchEvent*/);
            }
        }

        public override void TouchesBegan(List<CCTouch> touches)
        {
            if (JoyControl != null)
            {
                JoyControl.OnTouchesBegan(touches /*TODO, touchEvent*/);
            }

            foreach (var button in Buttons)
            {
                button.OnTouchesBegan(touches /*, touchEvent*/);
            }
        }

        #endregion

        #region Helper methods

        private void InitializeJoyStick()
        {
            this.JoyControl = new SneakyJoystickControlSkinnedBase(new CCRect(
                0f,
                0f,
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height));

            // TODO: Is this the right position for this panel?
            // Or should this be setting the JoyControl position here?
            this.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);

            this.AddChild(this.JoyControl);
        }

        private void InitializeButtons(int buttons)
        {
            SneakyButtonControlSkinnedBase tmp = null;
            for (int i = 0; i < buttons; i++)
            {
                tmp = new SneakyButtonControlSkinnedBase(i);
                this.AddChild(tmp);
                this.Buttons.Add(tmp);
            }

            // This will also arrange buttons
            Orientation = ButtonsOrientation.Horizontal;
        }

        private void ReorderButtons()
        {
            float x;
            float y;

            // TODO VisibleBoundsWorldspace.Size;

            if (Orientation == ButtonsOrientation.Vertical)
            {
                y = 0.03f;
                for (int i = 0; i < Buttons.Count; i++)
                {
                    x = (i % 2 == 0) ? 0.8f : 0.9f;

                    if (i % 2 == 0)
                    {
                        y += 0.12f;
                    }

                    // TODO: Buttons[i].Position = new CCPoint(visibleBoundsSize.Width * x, visibleBoundsSize.Height * y);
                    Buttons[i].Position = new CCPoint(
                        this.ContentSize.Width * x,
                        this.ContentSize.Height * y);
                    Buttons[i].AnchorPoint = CCPoint.AnchorLowerLeft;
                }
            }
            else
            {
                y = .15f;
                // TODO: x = visibleBoundsSize.Width;
                x = this.ContentSize.Width;
                for (int i = 0; i < Buttons.Count; i++)
                {
                    // TODO: 
                    Buttons[i].Position = new CCPoint(
                        x,
                        this.ContentSize.Height * y);
                    Buttons[i].AnchorPoint = CCPoint.AnchorLowerRight;
                    x -= (Buttons[i].DefaultSprite.BoundingBox.Size.Width + 20f);
                }
            }
        }

        #endregion

    }
}
