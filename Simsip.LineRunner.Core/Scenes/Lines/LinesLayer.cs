using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;
using Simsip.LineRunner.Scenes.MessageBox;
using System.Threading;
#if IOS
using Foundation;
#endif
#if NETFX_CORE
using Windows.Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Lines
{
    public class LinesLayer : GameLayer
    {
        private CoreScene _parent;

        // Pane and pane actions
        private PaneModel _paneModel;
        private Simsip.LineRunner.Actions.Action _paneActionIn;
        private Simsip.LineRunner.Actions.Action _paneActionOut;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Lines
        private ILineRepository _lineRepository;
        private IPageLinesRepository _pageLinesRepository;
        private IList<LineEntity> _lineEntities;
        private IList<CCRect> _lineBoundingBoxes;

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

            // Pane model
            var paneLogicalOrigin = new CCPoint(
                0.05f * screenSize.Width,
                0.05f * screenSize.Height);
            var paneModelArgs = new PaneModelArgs()
            {
                ThePaneType = PaneType.Simple,
                LogicalOrigin = paneLogicalOrigin,
                LogicalWidth = this.ContentSize.Width,
                LogicalHeight = this.ContentSize.Height
            };
            this._paneModel = new PaneModel(paneModelArgs);

            // Pane transition in/out
            var pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            var layerStartPosition = new CCPoint(
                paneLogicalOrigin.X, 
                screenSize.Height);
            var layerEndPosition = paneLogicalOrigin;
            var paneStartPosition = XNAUtils.LogicalToWorld(
                layerStartPosition,
                pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var paneEndPosition = XNAUtils.LogicalToWorld(
                layerEndPosition,
                pageCache.PaneDepthFromCameraStart,
                XNAUtils.CameraType.Stationary);
            var paneStartPlacementAction = new Place(paneStartPosition);
            var paneMoveInAction = new MoveTo(GameConstants.DURATION_LAYER_TRANSITION, paneEndPosition);
            this._paneActionIn = new EaseBackOut(
                new Sequence(new FiniteTimeAction[] { paneStartPlacementAction, paneMoveInAction })
            );
            var paneMoveOutAction = new MoveTo(GameConstants.DURATION_LAYER_TRANSITION, paneStartPosition);
            this._paneActionOut = new EaseBackIn(paneMoveOutAction);

            // Layer transition in/out
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
                // lineImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
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

        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate pane/layer
            this._paneModel.ModelRunAction(this._paneActionIn);
            this.RunAction(this._layerActionIn);
        }

        public override void Draw()
        {
            // Draw pane with Cocos2D view, projection and game state
            this._paneModel.DrawViaStationaryCamera();

            base.Draw();
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
            // Provide feedback
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

#if NETFX_CORE
            IAsyncAction asyncAction = 
                Windows.System.Threading.ThreadPool.RunAsync(
                    (workItem) =>
                    {
                        RefreshThread();
                    });
#else

            var refreshThread = new Thread(RefreshThread) { IsBackground = true };
            refreshThread.Start();
#endif

        }

        private void RefreshThread()
        {
            // Go for a refresh to get the new lines displayed
            this._parent.Refresh();

            // Remove ui
            this._parent.TheMessageBoxLayer.Hide();

            // Ok, we're done here
            this._parent.GoBack();
        }

        #endregion
    }
}