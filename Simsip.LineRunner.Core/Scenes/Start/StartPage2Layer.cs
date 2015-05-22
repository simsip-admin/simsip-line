using System.Collections.Generic;
using System;
using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using Simsip.LineRunner.GameObjects.Characters;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Start
{
    public class StartPage2Layer : GameLayer
    {
        private CoreScene _parent;

        // Start tap button/animation
        private CCSprite _startTapImage;
        private CCAction _startTapAction;

        // Services we'll need
        private IPageCache _pageCache;
        private ICharacterCache _characterCache;

        public StartPage2Layer(CoreScene parent)
        {
            this._parent = parent;

            // We want touches any tap will get us started
            this.TouchEnabled = true;

            // Box out positioning around hero
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));
            var heroStartOrigin = this._pageCache.CurrentPageModel.HeroStartOrigin;
            var heroModel = this._characterCache.TheHeroModel;
            var heroBottomMiddle = XNAUtils.WorldToLogical(new Vector3(
                heroStartOrigin.X + (0.5f * heroModel.WorldWidth),
                heroStartOrigin.Y,
                heroStartOrigin.Z),
                XNAUtils.CameraType.Stationary);
            var heroMiddleLeft = XNAUtils.WorldToLogical(new Vector3(
                heroStartOrigin.X,
                heroStartOrigin.Y + (0.5f * heroModel.WorldHeight),
                heroStartOrigin.Z),
                XNAUtils.CameraType.Stationary);
            var heroMiddleRight = XNAUtils.WorldToLogical(new Vector3(
                heroStartOrigin.X + heroModel.WorldWidth,
                heroStartOrigin.Y + (0.5f * heroModel.WorldHeight),
                heroStartOrigin.Z),
                XNAUtils.CameraType.Stationary);

            // Start button
            this._startTapImage = new CCSprite("Images/Icons/TapButton");
            this._startTapImage.AnchorPoint = CCPoint.AnchorMiddleTop;
            this._startTapImage.Position = new CCPoint(
                heroBottomMiddle.X,
                heroBottomMiddle.Y - (0.2f * this.ContentSize.Height));
            this.AddChild(this._startTapImage);
            this._startTapAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                { 
                    new CCMoveBy(1, new CCPoint(
                        0,
                        0.1f * this.ContentSize.Height)),
                    new CCMoveBy(2, new CCPoint(
                        0,
                        -0.1f * this.ContentSize.Height)),
                }));
            this._startTapImage.RunAction(this._startTapAction);

            var startTapText = string.Empty;
#if ANDROID
            startTapText = Program.SharedProgram.Resources.GetString(Resource.String.StartTap);
#elif IOS
            startTapText = NSBundle.MainBundle.LocalizedString(Strings.StartTap, Strings.StartTap);
#else
            startTapText = AppResources.StartTap;
#endif
            var startTapDescription1 = new CCLabelTTF(startTapText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            startTapDescription1.AnchorPoint = CCPoint.AnchorMiddleRight;
            startTapDescription1.Color = CCColor3B.Green;
            startTapDescription1.Position = new CCPoint(
                heroMiddleLeft.X - (0.1f * this.ContentSize.Width),
                heroMiddleLeft.Y);
            this.AddChild(startTapDescription1);

            var startTapDescription2 = new CCLabelTTF(startTapText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            startTapDescription1.AnchorPoint = CCPoint.AnchorMiddleLeft;
            startTapDescription2.Color = CCColor3B.Green;
            startTapDescription2.Position = new CCPoint(
                heroMiddleRight.X + (0.1f * this.ContentSize.Width),
                heroMiddleRight.Y);
            this.AddChild(startTapDescription2);

        }

        #region Touch Implementation

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

        public override void TouchesBegan(List<CCTouch> touches)
        {
            this._parent.Navigate(LayerTags.HudLayer);
        }

        #endregion
    }
}