using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Lines;
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


namespace Simsip.LineRunner.Scenes.Lines
{
    public class LinesLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Lines
        private ILineRepository _lineRepository;
        private IPageLinesRepository _pageLinesRepository;
        private IList<LineEntity> _lineEntities;
        private IList<CCRect> _lineBoundingBoxes;

        // Services we'll need
        private ICharacterCache _characterCache;

        public LinesLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.9f * screenSize.Width,
                0.9f * screenSize.Height);

            // We want touches so we can handle selection of pad models
            this.TouchEnabled = true;
            TouchMode = CCTouchMode.OneByOne;

            // Layer transition in/out
            var layerEndPosition = new CCPoint(
                0.05f * screenSize.Width,
                0.05f * screenSize.Height);
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

            // Lines menu
            this._lineEntities = new List<LineEntity>();
            this._lineBoundingBoxes = new List<CCRect>();
            this._lineRepository = new LineRepository();
            this._pageLinesRepository = new PageLinesRepository();
            var pageLinesEntity = this._pageLinesRepository.GetLine(1, 1);
            this._lineEntities = this._lineRepository.GetLines();
            var y = 0.9f * this.ContentSize.Height;
            foreach (var line in this._lineEntities)
            {
                var lineImage = new CCSprite("Models/Lines/" + line.ModelName + "-thumbnail");
                Cocos2DUtils.ResizeSprite(lineImage,
                    0.5f * this.ContentSize.Width,
                    0.2f * this.ContentSize.Width);
                lineImage.Position = new CCPoint(
                    0.65f * this.ContentSize.Width,
                    y + (0.01f * this.ContentSize.Height));
                this.AddChild(lineImage);

                this._lineBoundingBoxes.Add(lineImage.WorldBoundingBox);

                var lineLabel = new CCLabelTTF(line.DisplayName, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                var lineButton = new CCMenuItemLabel(lineLabel,
                                                     (obj) => { LineSelected(line); });

                var labelMenu = new CCMenu();
                labelMenu.Position = new CCPoint(
                    0.25f * this.ContentSize.Width,
                    y);
                labelMenu.AddChild(lineButton);
                this.AddChild(labelMenu);

                y -= 0.09f * this.ContentSize.Height;
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

            // Did we touch a line model?
            for (int i = 0; i < this._lineBoundingBoxes.Count; i++)
            {
                var boundingBox = this._lineBoundingBoxes[i];
                if (CCRect.ContainsPoint(ref boundingBox, ref location))
                {
                    LineSelected(this._lineEntities[i]);
                    break;
                }
            }

            return true;
        }

        #endregion

        #region Helper methods

        private void LineSelected(LineEntity line)
        {
            // Provide immediate feedback
            var switchingLinesText = string.Empty;
#if ANDROID
            switchingLinesText = Program.SharedProgram.Resources.GetString(Resource.String.LinesSwitchingLines);
#elif IOS
            switchingLinesText = NSBundle.MainBundle.LocalizedString(Strings.LinesSwitchingLines, Strings.LinesSwitchingLines);
#else
            switchingLinesText = AppResources.LinesSwitchingLines;
#endif

            this._parent.TheMessageBoxLayer.Show(
                switchingLinesText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);

            // Record what was selected
            UserDefaults.SharedUserDefault.SetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_LINE,
                line.ModelName);

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