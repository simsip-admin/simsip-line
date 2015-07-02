using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects;
#if IOS
using Foundation;
#endif
#if NETFX_CORE
using Windows.Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Pads
{
    public class PadsLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Pads
        private IPadRepository _padRepository;
        private IList<PadEntity> _padEntities;
        private IList<CCRect> _padBoundingBoxes;

        // Services we'll need
        private ICharacterCache _characterCache;

        public PadsLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.VisibleSize;
            this.ContentSize = new CCSize(
                0.8f * screenSize.Width,
                0.8f * screenSize.Height);

            // We want touches so we can handle selection of pad models
            this.TouchEnabled = true;
            TouchMode = CCTouchMode.OneByOne;

            // Layer transition in/out
            var layerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                0.1f * screenSize.Width,
                0.1f * screenSize.Height);
            var layerStartPosition = new CCPoint(
                layerEndPosition.X,
                screenSize.Height);
            var layerStartPlacementAction = new CCPlace(layerStartPosition);
            var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
            this._layerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction })
            );
            var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
            var layerNavigateAction = new CCCallFunc(() => { this._parent.GoBack(); });
            this._layerActionOut = new CCEaseBackIn(
                new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, layerNavigateAction })
            );

            // Pads menu
            this._padBoundingBoxes = new List<CCRect>();
            this._padRepository = new PadRepository();
            this._padEntities = this._padRepository.GetPads();
            var y = 0.6f*this.ContentSize.Height;
            foreach(var pad in this._padEntities)
            {
                var padImage = new CCSprite("Models/Pads/" + pad.ModelName + "-thumbnail");
                Cocos2DUtils.ResizeSprite(padImage,
                    0.5f * this.ContentSize.Width,
                    0.5f * this.ContentSize.Width);
                padImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
                padImage.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    y);
                this.AddChild(padImage);

                this._padBoundingBoxes.Add(padImage.WorldBoundingBox);

                var padLabel = new CCLabelTTF(pad.DisplayName, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                var padsButton = new CCMenuItemLabel(padLabel,
                                                     (obj) => { PadSelected(pad); });
                var labelMenu = new CCMenu();
                labelMenu.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    y);
                labelMenu.AddChild(padsButton);
                this.AddChild(labelMenu);

                y -= 0.4f * this.ContentSize.Height;
            }
            

            // Back
            CCMenuItemImage backButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => { this._parent.GoBack(); });
            var backMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        backButton, 
                    });
            backMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.1f * this.ContentSize.Height);
            this.AddChild(backMenu);
            var backText = string.Empty;
#if ANDROID
            backText = Program.SharedProgram.Resources.GetString(Resource.String.CommonBack);
#elif IOS
            backText = NSBundle.MainBundle.LocalizedString(Strings.CommonBack, Strings.CommonBack);
#else
            backText = AppResources.CommonBack;
#endif
            var backLabel = new CCLabelTTF(backText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            backLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.02f * this.ContentSize.Height);
            this.AddChild(backLabel);

            // Grab reference to services we'll need
            this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate layer
            this.RunAction(this._layerActionIn);
        }

        #endregion

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

        /// <summary>
        /// If you return true then ccTouchEnded will called. 
        /// If you return false then ccTouchEnded will not be called, and the event will go the parent layer
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
        public override bool TouchBegan(CCTouch touch)
        {
            var location = touch.Location;

            // Did we touch a pad model?
            for(int i = 0; i < this._padBoundingBoxes.Count; i++)
            {
                var boundingBox = this._padBoundingBoxes[i];
                if (CCRect.ContainsPoint(ref boundingBox, ref location))
                {
                    PadSelected(this._padEntities[i]);
                    break;
                }
            }

            return true;
        }

        #endregion

        #region Helper methods

        private void PadSelected(PadEntity pad)
        {
            // Provide immediate feedback
            var switchingPadsText = string.Empty;
#if ANDROID
            switchingPadsText = Program.SharedProgram.Resources.GetString(Resource.String.PadsSwitchingPads);
#elif IOS
            switchingPadsText = NSBundle.MainBundle.LocalizedString(Strings.PadsSwitchingPads, Strings.PadsSwitchingPads);
#else
            switchingPadsText = AppResources.PadsSwitchingPads;
#endif
            this._parent.TheMessageBoxLayer.Show(
                switchingPadsText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);

            // Record what was selected
            UserDefaults.SharedUserDefault.SetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_PAD,
                pad.ModelName);

            // Hook up an event handler for end of content loading caused by
            // refresh kicking off background load
            this._characterCache.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;

            // Start the background refresh to get the new pad displayed. See 
            // LoadContentAsyncFinishedHandler for how we clean-up after refresh is finished
            this._parent.Refresh();
        }

        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            // We only want to react to a refresh event
            if (args.TheLoadContentAsyncType == LoadContentAsyncType.Refresh)
            {
                // Unhook so we are a one-shot event handler
                this._characterCache.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;

                // We can now let this layer's ui resume
                this._parent.TheMessageBoxLayer.Hide();
            }
        }

        #endregion
    }
}