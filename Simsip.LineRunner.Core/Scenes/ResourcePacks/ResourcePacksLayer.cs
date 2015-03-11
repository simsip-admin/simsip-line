using System.Collections.Generic;
using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using Simsip.LineRunner.Scenes.MessageBox;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.ResourcePacks
{
    public class ResourcePacksLayer : GameLayer
    {
        private CoreScene _parent;

        // Pane and pane actions
        private PaneModel _paneModel;
        private Simsip.LineRunner.Actions.Action _paneActionIn;
        private Simsip.LineRunner.Actions.Action _paneActionOut;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Resource packs
        private IResourcePackRepository _resourcePackRepository;
        private IList<ResourcePackEntity> _resourcePackEntities; 
        private IList<CCRect> _resourcePackBoundingBoxes;

        public ResourcePacksLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.8f * screenSize.Width,
                0.8f * screenSize.Height);

            // We want touches so we can handle selection of resource packs
            this.TouchEnabled = true;
            TouchMode = CCTouchMode.OneByOne;

            // Pane model
            var paneLogicalOrigin = new CCPoint(
                0.1f * screenSize.Width,
                0.1f * screenSize.Height);
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

            // Resource packs
            this._resourcePackBoundingBoxes = new List<CCRect>();
            this._resourcePackRepository = new ResourcePackRepository();
            this._resourcePackEntities = this._resourcePackRepository.GetResourcePacks();
            var y = 0.75f * this.ContentSize.Height;
            foreach(var pack in this._resourcePackEntities)
            {
                var packImage = new CCSprite("ResourcePacks/" + pack.ResourcePackName + "/Thumbnail");
                Cocos2DUtils.ResizeSprite(packImage,
                    0.3f * this.ContentSize.Width,
                    0.3f * this.ContentSize.Width);
                packImage.AnchorPoint = CCPoint.AnchorMiddleBottom;
                packImage.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    y);
                this.AddChild(packImage);

                this._resourcePackBoundingBoxes.Add(packImage.WorldBoundingBox);

                var packLabel = new CCLabelTTF(pack.DisplayName, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                var packButton = new CCMenuItemLabel(packLabel,
                                                     (obj) => { ResourcePackSelected(pack); });
                var labelMenu = new CCMenu();
                labelMenu.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    y - (0.03f * this.ContentSize.Height));
                labelMenu.AddChild(packButton);
                this.AddChild(labelMenu);

                y -= 0.26f * this.ContentSize.Height;
            }

            // Back
            CCMenuItemImage backButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => { _parent.GoBack(); });
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

            // Did we touch a pad model?
            for (int i = 0; i < this._resourcePackBoundingBoxes.Count; i++)
            {
                var boundingBox = this._resourcePackBoundingBoxes[i];
                if (CCRect.ContainsPoint(ref boundingBox, ref location))
                {
                    ResourcePackSelected(this._resourcePackEntities[i]);
                    break;
                }
            }

            return true;
        }

        #endregion
        #region Helper methods

        private void ResourcePackSelected(ResourcePackEntity resourcePack)
        {
            // Provide feedback
            var switchingResourcePacksText = string.Empty;
#if ANDROID
            switchingResourcePacksText = Program.SharedProgram.Resources.GetString(Resource.String.ResourcePacksSwitchingResourcePacks);
#elif IOS
            switchingResourcePacksText = NSBundle.MainBundle.LocalizedString(Strings.ResourcePacksSwitchingResourcePacks, Strings.ResourcePacksSwitchingResourcePacks);
#else
            switchingResourcePacksText = AppResources.ResourcePacksSwitchingResourcePacks;
#endif
            /* TODO
            this._parent.TheMessageBoxLayer.Show(
                switchingResourcePacksText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);
            */

            // Record what was selected
            UserDefaults.SharedUserDefault.SetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_RESOURCE_PACK,
                resourcePack.ResourcePackName);

            // Go for a refresh to get the new resource pack displayed
            this._parent.Refresh();

            // Ok, we're done here
            // TODO
            // this._parent.TheMessageBoxLayer.Hide();
            this._parent.GoBack();
        }

        #endregion
    }
}