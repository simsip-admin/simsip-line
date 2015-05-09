using System.Collections.Generic;
using System;
using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Start
{
    public class StartPage2Layer : GameLayer
    {
        private CoreScene _parent;

        public StartPage2Layer(CoreScene parent)
        {
            this._parent = parent;

            // We want touches any tap will get us started
            this.TouchEnabled = true;

            // Start button
            var startTapImage = new CCSprite("Images/Icons/TapButton");
            Cocos2DUtils.ResizeSprite(startTapImage,
                0.2f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            startTapImage.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.3f * this.ContentSize.Height);
            this.AddChild(startTapImage);
            var startTapText = string.Empty;
#if ANDROID
            startTapText = Program.SharedProgram.Resources.GetString(Resource.String.StartTap);
#elif IOS
            startTapText = NSBundle.MainBundle.LocalizedString(Strings.StartTap, Strings.StartTap);
#else
            startTapText = AppResources.StartTap;
#endif
            var startTapDescription1 = new CCLabelTTF(startTapText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            startTapDescription1.Color = CCColor3B.Green;
            startTapDescription1.Position = new CCPoint(
                0.2f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(startTapDescription1);

            var startTapDescription2 = new CCLabelTTF(startTapText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            startTapDescription2.Color = CCColor3B.Green;
            startTapDescription2.Position = new CCPoint(
                0.8f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
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