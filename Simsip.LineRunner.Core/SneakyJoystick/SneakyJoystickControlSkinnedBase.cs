using Cocos2D;

namespace Simsip.LineRunner.SneakyJoystick
{
    public class SneakyJoystickControlSkinnedBase : SneakyJoystickControl
    {
		public SneakyJoystickControlSkinnedBase(CCRect rect)
			: base(rect)
		{
			BackgroundSprite = new ColoredCircleSprite( 
                CCColor4B.Red, 
                0.5f * rect.Size.Width);

			ThumbSprite = new ColoredCircleSprite(
                CCColor4B.Blue,
                0.25f * rect.Size.Width);
		}

        #region Properties

        private CCSprite _backgroundSprite;
        public CCSprite BackgroundSprite
        {
            get
            {
                return _backgroundSprite;
            }
            set
            {

                if (_backgroundSprite != null)
                {
                    if (_backgroundSprite.Parent != null)
                    {
                        _backgroundSprite.Parent.RemoveChild(_backgroundSprite, true);
                    }
                }

                _backgroundSprite = value;
                if (value != null)
                {
                    AddChild(_backgroundSprite);
                    RefreshBackgroundSpritePosition();
                }

            }
        }

        private CCSprite _thumbSprite;
        public CCSprite ThumbSprite
        {
            get
            {
                return _thumbSprite;
            }
            set
            {
                if (_thumbSprite != null)
                {
                    if (_thumbSprite.Parent != null)
                    {
                        _thumbSprite.Parent.RemoveChild(_thumbSprite, true);
                    }
                }

                _thumbSprite = value;
                if (value != null)
                {
                    AddChild(_thumbSprite);
                    RefreshThumbSpritePosition();
                }
            }
        }

        private byte _opacity;
        public byte Opacity
        {
            get { return _opacity; }
            set
            {

                _opacity = value;

                if (_backgroundSprite != null)
                {
                    _backgroundSprite.Opacity = value;
                }
                if (_thumbSprite != null)
                {
                    _thumbSprite.Opacity = value;
                }
            }
        }
    
        #endregion

        #region Cocos2D overrides

        public override CCPoint Position
        {
            get
            {

                return base.Position;
            }
            set
            {
                base.Position = value;

                this.RefreshAllPosition();
            }
        }

        public override CCSize ContentSize
        {
            get
            {
                return base.ContentSize;
            }
            set
            {

                if (_backgroundSprite != null)
                {
                    this._backgroundSprite.ContentSize = value;
                    this._backgroundSprite.Position = value.Center;
                }

                if (_thumbSprite != null)
                {
                    this._thumbSprite.ContentSize = value;
                    this._thumbSprite.Position = value.Center;
                }

                this.JoystickRadius = 0.5f * value.Width;

                base.ContentSize = value;
            }
        }

        #endregion

        #region Api

        public override void UpdateVelocity(CCPoint point)
        {
            base.UpdateVelocity(point);

            if (this._thumbSprite != null)
            {
                this._thumbSprite.Position = StickPosition;
            }
        }

        public void RefreshBackgroundSpritePosition()
        {
            this.ContentSize = _backgroundSprite.ContentSize;
            this.JoystickRadius = 0.5f * _backgroundSprite.ContentSize.Width;
        }

        public void RefreshThumbSpritePosition()
        {
			this._thumbSprite.Position = ContentSize.Center;
            this.ThumbRadius = 0.5f * _thumbSprite.ContentSize.Width;
        }

        public void RefreshAllPosition()
        {
            RefreshBackgroundSpritePosition();
            RefreshThumbSpritePosition();
        }

        #endregion

    }
}
