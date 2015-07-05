using Cocos2D;
using Engine.Graphics;
using Engine.Input;
using Microsoft.Xna.Framework;
using Org.BouncyCastle.Crypto.Engines;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System;
using Simsip.LineRunner.GameObjects.Characters;
using Microsoft.Xna.Framework.Input.Touch;
using Simsip.LineRunner.Data.Facebook;
#if NETFX_CORE
using Windows.System.Threading;
#else
using System.Threading;
using System.Diagnostics;
using Simsip.LineRunner.GameObjects;
#endif
#if IOS
using Foundation;
using GoogleAdMobAds;
#endif


namespace Simsip.LineRunner.Scenes.Hud
{
    public class HudLayer : GameLayer
    {
        private CoreScene _parent;

        // Services we need
        private IPageCache _pageCache;
        private ICharacterCache _characterCache;
        private IInputManager _inputManager;

        // Start tap image/animation/button
        private CCSprite _startTapImage;
        private float _hudCameraPreviousOffsetX;
        private float _hudCameraPreviousOffsetY;
        private float _hudCameraPreviousOffsetYaw;
        private float _hudCameraPreviousOffsetPitch;
        private float _hudCameraPreviousOrbitYaw;
        private float _hudCameraPreviousOrbitPitch;
        private CCAction _startTapAction;
        private CCMenuItemLabel _startButtonItem;

        // Base layer
        private GameLayer _baseLayer;

        // Header layer
        private GameLayer _headerLayer;
        private UILayer _headerLeftLayer;
        private UILayer _headerRightLayer;
        private CCAction _headerLayerActionIn;
        private CCAction _headerLayerActionOut;
        private CCAction _hideHeaderAnim;
        private CCAction _restoreHeaderAnim;

        // Footer pane/layer
        private GameLayer _footerLayer;
        private UILayer _footerLeftLayer;
        private UILayer _footerRightLayer;
        private CCAction _footerLayerActionIn;
        private CCAction _footerLayerActionOut;
        private CCAction _hideFooterAnim;
        private CCAction _restoreFooterAnim;
        
        // Trackball
        private CCMenuItemLabel _trackballItem;

        // High score
        private CCLabelTTF _highScoreHeaderLabel;
        private CCLabelTTF _highScoreLabel;
        private CCLabelTTF _highScoreTimeHeaderLabel;
        private CCLabelTTF _highScoreTimeLabel;

        // Current score
        private CCLabelTTF _scoreLabel;
        private CCAction _scoreLabelAction;

        // Status label
        private CCLabelTTF _status1Label;
        private CCAction _status1LabelAction;
        private CCLabelTTF _status2Label;
        private CCAction _status2LabelAction;

        // Timer
        private CCLabelTTF _timerLabel;
        private DateTime _timerStartTime;
        private TimeSpan _elapsedTime;
        private string _timerLabelText;
#if NETFX_CORE
        private ThreadPoolTimer _timer;
#else
        private Timer _timer;
#endif

        // Joystick
        private CCMenuItemImage _joystickLeftItem;
        private CCMenuItemImage _joystickRightItem;
        private CCMenuItemImage _joystickUpItem;
        private CCMenuItemImage _joystickDownItem;
        private MoveDirection _joystickMoveDirection;
        private CCLabelTTF _joystickLabel;

        // Pause/resume
        private bool _paused;
        private string _pauseText;
        private string _pausedText;
        private string _resumeText;
        private CCMenuItemToggle _pauseToggle;
        private CCLabelTTF _pauseLabel;

        // Additional text
        private string _doubleTapResetText;
        private string _hudSpeedText;
        private string _hudCannotGoSlowerThanText;
        private string _hudSpeedDefaultText;

        // Dragging support
        private enum Dragging
        {
            None,
            Offset,
            Orbit
        }
        private Dragging _dragging;

        private GameState _currentGameState;

        public HudLayer(CoreScene parent)
        {
            this._parent = parent;

            // Not dragging anything at start
            this._dragging = Dragging.None;

            // Grab rerences to services we'll need
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
            this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.VisibleSize;
            this.ContentSize = new CCSize(
                screenSize.Width,
                screenSize.Height);
            var baseContentSize = new CCSize(   // IMPORTANT: Needs to be full size for proper placement of hero, see OnEnter()
                this.ContentSize.Width,
                this.ContentSize.Height);
            var headerSize = new CCSize(
                0.96f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            var headerLeftSize = new CCSize(
                0.78f * headerSize.Width,
                headerSize.Height);
            var headerRightSize = new CCSize(
                0.2f * headerSize.Width,
                headerSize.Height);
            var footerSize = new CCSize(
                0.96f * this.ContentSize.Width,
                0.2f * this.ContentSize.Height);
            var footerLeftSize = new CCSize(
                0.78f * footerSize.Width,
                footerSize.Height);
            var footerRightSize = new CCSize(
                0.2f * footerSize.Width,
                footerSize.Height);

            // Status 1 action
            // IMPORTANT: Status 1 remains visible for a short duration before being removed - see UpdateStatus1()
            this._status1LabelAction = new CCSequence(new CCFiniteTimeAction[] 
                { 
                    new CCShow(),
                    new CCScaleTo(0f, 0f), 
                    new CCScaleTo(0.5f, 1.2f),
                    new CCScaleTo(0.1f, 1.0f),
                    new CCDelayTime(GameConstants.DURATION_STATUS_LABEL),
                    new CCHide()
                });

            // Status 2 action
            // IMPORTANT: Status 2 remains visible until manually taken down - see UpdateStatus2()
            this._status2LabelAction = new CCSequence(new CCFiniteTimeAction[] 
            { 
                new CCShow(),
                new CCScaleTo(0f, 0f), 
                new CCScaleTo(0.5f, 1.2f),
                new CCScaleTo(0.1f, 1.0f),
            });

            // Base layer
            this._baseLayer = new GameLayer();
            this._baseLayer.ContentSize = baseContentSize;
            this._baseLayer.Position = CCDirector.SharedDirector.VisibleOrigin;
            this.AddChild(this._baseLayer);

            // Start button: Large transparent button sized/positioned between header/footer
            // so that you can tap anywhere in middle of screen to get started
            var startButtonLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._startButtonItem = new CCMenuItemLabel(startButtonLabel,
                (obj) => { this.StartPressed(); });
            this._startButtonItem.ContentSize = new CCSize(
                screenSize.Width,
                0.52f * screenSize.Height);
            var startLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        this._startButtonItem
                    });
            startLabelMenu.AnchorPoint = CCPoint.AnchorMiddle;
            startLabelMenu.Position = new CCPoint(
                0.5f * this._baseLayer.ContentSize.Width,
                0.5f * this._baseLayer.ContentSize.Height);
            this._baseLayer.AddChild(startLabelMenu);

            // Start image
            this._startTapImage = new CCSprite("Images/Icons/TapButton");
            this._startTapImage.AnchorPoint = CCPoint.AnchorMiddleTop;
            this._baseLayer.AddChild(this._startTapImage);
            this._startTapAction = new CCRepeatForever(new CCSequence(new CCFiniteTimeAction[] 
                { 
                    new CCMoveBy(1, new CCPoint(
                        0,
                        0.1f * this.ContentSize.Height)),
                    new CCMoveBy(2, new CCPoint(
                        0,
                        -0.1f * this.ContentSize.Height)),
                }));
            this._hudCameraPreviousOffsetX = -1f;
            this._hudCameraPreviousOffsetY = -1f;
            this._hudCameraPreviousOffsetYaw = -1f;
            this._hudCameraPreviousOffsetPitch = -1f;
            this._hudCameraPreviousOrbitYaw = -1f;
            this._hudCameraPreviousOrbitPitch = -1f;

            // Header layers
            this._headerLayer = new GameLayer();
            this._headerLayer.ContentSize = headerSize;
            var headerLayerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                0.02f * screenSize.Width,
                0.78f * screenSize.Height);
#if IOS
            // With the ad banner overlaying our screen in ios, we need to adjust certain ui elements downward by size of ad banner
            headerLayerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                0.02f * screenSize.Width,
                0.78f * screenSize.Height - (float)GADAdSizeCons.Banner.Size.Height);
#endif
            var headerLayerStartPosition = new CCPoint(
                headerLayerEndPosition.X,
                screenSize.Height);
            this._headerLayer.Position = headerLayerEndPosition;
            this.AddChild(this._headerLayer);
            this._headerLeftLayer = new UILayer();
            this._headerLeftLayer.ContentSize = headerLeftSize;
            this._headerLayer.AddChild(this._headerLeftLayer);
            this._headerRightLayer = new UILayer();
            this._headerRightLayer.ContentSize = headerRightSize;
            this._headerRightLayer.Position = new CCPoint(
                0.8f * this._headerLayer.ContentSize.Width,
                0);
            this._headerLayer.AddChild(this._headerRightLayer);

            // Header animations
            var headerLayerStartPlacementAction = new CCPlace(headerLayerStartPosition);
            var headerLayerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, headerLayerEndPosition);
            this._headerLayerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { headerLayerStartPlacementAction, headerLayerMoveInAction })
            );
            var headerLayerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, headerLayerStartPosition);
            this._headerLayerActionOut = new CCEaseBackIn(headerLayerMoveOutAction);
            this._hideHeaderAnim = new CCScaleTo(GameConstants.DURATION_LAYER_TRANSITION, 0f);
            this._restoreHeaderAnim = new CCEaseBackOut(new CCScaleTo(GameConstants.DURATION_LAYER_TRANSITION, 1f));

            // Hud menu
            var homeText = string.Empty;
#if ANDROID
            homeText = Program.SharedProgram.Resources.GetString(Resource.String.HudHome);
#elif IOS
            homeText = NSBundle.MainBundle.LocalizedString(Strings.HudHome, Strings.HudHome);
#else
            homeText = AppResources.HudHome;
#endif
            var homeLabel = new CCLabelTTF(homeText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            homeLabel.Color = CCColor3B.Green;
            var homeItem = new CCMenuItemLabel(homeLabel,
                (obj) => { this._parent.GoBack(); });
            var upgradesText = string.Empty;
#if ANDROID
            upgradesText = Program.SharedProgram.Resources.GetString(Resource.String.UpgradesTitle);
#elif IOS
            upgradesText = NSBundle.MainBundle.LocalizedString(Strings.UpgradesTitle, Strings.UpgradesTitle);
#else
            upgradesText = AppResources.UpgradesTitle;
#endif

#if ANDROID || IOS
            var upgradesLabel = new CCLabelTTF(upgradesText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            upgradesLabel.Color = CCColor3B.Blue;
            var upgradesItem = new CCMenuItemLabel(upgradesLabel,
                (obj) => { this.NavigateBase(LayerTags.UpgradesMasterLayer); });
#endif

            var optionsText = string.Empty;
#if ANDROID
            optionsText = Program.SharedProgram.Resources.GetString(Resource.String.HudOptions);
#elif IOS
            optionsText = NSBundle.MainBundle.LocalizedString(Strings.HudOptions, Strings.HudOptions);
#else
            optionsText = AppResources.HudOptions;
#endif
            var optionsLabel = new CCLabelTTF(optionsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            optionsLabel.Color = CCColor3B.Green;
            var optionsItem = new CCMenuItemLabel(optionsLabel,
                (obj) => { this.NavigateBase(LayerTags.OptionsMasterLayer); });
            var helpText = string.Empty;
#if ANDROID
            helpText = Program.SharedProgram.Resources.GetString(Resource.String.HudHelp);
#elif IOS
            helpText = NSBundle.MainBundle.LocalizedString(Strings.HudHelp, Strings.HudHelp);
#else
            helpText = AppResources.HudHelp;
#endif
            var helpLabel = new CCLabelTTF(helpText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            helpLabel.Color = CCColor3B.Green;
            var helpItem = new CCMenuItemLabel(helpLabel,
                (obj) => { this.NavigateBase(LayerTags.HelpMasterLayer); });
            var hudMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        homeItem,
#if ANDROID || IOS
                        upgradesItem,
#endif
                        optionsItem,
                        helpItem
                    });
            hudMenu.AlignItemsVertically();
            hudMenu.Position = new CCPoint(
                 0.25f * headerLeftSize.Width,
                 0.5f  * headerLeftSize.Height);
            this._headerLeftLayer.AddChild(hudMenu);

            // Zoom
            CCMenuItemImage zoomInToggle =
                new CCMenuItemImage("Images/Icons/ZoomInButtonNormal.png",
                                    "Images/Icons/ZoomInButtonSelected.png");
            CCMenuItemImage zoomOutToggle =
                new CCMenuItemImage("Images/Icons/ZoomOutButtonNormal.png",
                                    "Images/Icons/ZoomOutButtonSelected.png");
            var zoomToggle =
                new CCMenuItemToggle((obj) => ZoomTogglePressed((obj as CCMenuItemToggle).SelectedIndex),
                new CCMenuItem[] { zoomInToggle, zoomOutToggle });
            var zoomMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        zoomToggle,
                    });
            zoomMenu.Position = new CCPoint(
                0.5f * headerLeftSize.Width,
                0.3f * headerLeftSize.Height);
            this._headerLeftLayer.AddChild(zoomMenu);

            // Speed
            var decreaseButtonNormal = new CCSprite("Images/Icons/DecreaseButtonNormal.png");
            var decreaseButtonSelected = new CCSprite("Images/Icons/DecreaseButtonSelected.png");
            var decreaseButton = new CCMenuItemImage((obj) => { this.DecreaseVelocity(); });
            decreaseButton.NormalImage = decreaseButtonNormal;
            decreaseButton.SelectedImage = decreaseButtonSelected;

            var increaseButtonNormal = new CCSprite("Images/Icons/IncreaseButtonNormal.png");
            var increaseButtonSelected = new CCSprite("Images/Icons/IncreaseButtonSelected.png");
            var increaseButton = new CCMenuItemImage((obj) => { this.IncreaseVelocity(); });
            increaseButton.NormalImage = increaseButtonNormal;
            increaseButton.SelectedImage = increaseButtonSelected;

            var speedMenu = new CCMenu(
            new CCMenuItem[] 
                    {
                        decreaseButton,
                        increaseButton,
                    });
            speedMenu.AlignItemsHorizontallyWithPadding(
                0.05f * headerLeftSize.Width);
            speedMenu.AnchorPoint = CCPoint.AnchorMiddle;
            speedMenu.Position = new CCPoint(
                0.75f * headerLeftSize.Width,
                0.6f  * headerLeftSize.Height);
            this._headerLeftLayer.AddChild(speedMenu);

            var speedText = string.Empty;
#if ANDROID
            speedText = Program.SharedProgram.Resources.GetString(Resource.String.HudSpeed);
#elif IOS
            speedText = NSBundle.MainBundle.LocalizedString(Strings.HudSpeed, Strings.HudSpeed);
#else
            speedText = AppResources.HudSpeed;
#endif
            var speedLabel = new CCLabelTTF(speedText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            speedLabel.AnchorPoint = CCPoint.AnchorMiddle;
            speedLabel.Position = new CCPoint(
                0.75f * headerLeftSize.Width,
                0.2f  * headerLeftSize.Height);
            this._headerLeftLayer.AddChild(speedLabel);

            // High score
            var highScoreHeaderText = string.Empty;
#if ANDROID
            highScoreHeaderText = Program.SharedProgram.Resources.GetString(Resource.String.HudHigh);
#elif IOS
            highScoreHeaderText = NSBundle.MainBundle.LocalizedString(Strings.HudHigh, Strings.HudHigh);
#else
            highScoreHeaderText = AppResources.HudHigh;
#endif
            this._highScoreHeaderLabel = new CCLabelTTF(highScoreHeaderText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._highScoreHeaderLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._highScoreHeaderLabel.Position = new CCPoint(
                0.5f * headerRightSize.Width,
                0.8f * headerRightSize.Height);
            this._headerRightLayer.AddChild(this._highScoreHeaderLabel);
            this._highScoreLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._highScoreLabel.Color = CCColor3B.Yellow;
            this._highScoreLabel.Position = new CCPoint(
                0.5f * headerRightSize.Width,
                0.6f * headerRightSize.Height);
            this._headerRightLayer.AddChild(this._highScoreLabel);
            var timeText = string.Empty;
#if ANDROID
            timeText = Program.SharedProgram.Resources.GetString(Resource.String.HudTime);
#elif IOS
            timeText = NSBundle.MainBundle.LocalizedString(Strings.HudTime, Strings.HudTime);
#else
            timeText = AppResources.HudTime;
#endif
            this._highScoreTimeHeaderLabel = new CCLabelTTF(timeText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._highScoreTimeHeaderLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._highScoreTimeHeaderLabel.Position = new CCPoint(
                0.5f * headerRightSize.Width,
                0.4f * headerRightSize.Height);
            this._headerRightLayer.AddChild(this._highScoreTimeHeaderLabel);
            this._highScoreTimeLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._highScoreTimeLabel.Position = new CCPoint(
                0.5f * headerRightSize.Width,
                0.2f * headerRightSize.Height);
            this._headerRightLayer.AddChild(this._highScoreTimeLabel);

            // Current score
            // IMPORTANT: Starts off not visible
            this._scoreLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            this._scoreLabel.Visible = false;
            this._scoreLabel.Color = CCColor3B.Yellow;
            this._scoreLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._scoreLabel.Position = new CCPoint(
                0.5f * headerRightSize.Width, 
                0.7f * headerRightSize.Height);
            this._headerRightLayer.AddChild(this._scoreLabel);

            // Score label action
            var scaleStartScore = new CCScaleTo(0f, 0f);
            var scaleUpScore = new CCScaleTo(0.5f, 1.2f);
            var scaleBackScore = new CCScaleTo(0.1f, 1.0f);
            this._scoreLabelAction = new CCSequence(new CCFiniteTimeAction[] { scaleStartScore, scaleUpScore, scaleBackScore });

            // Timer
            // IMPORTANT: Starts off not visible
            this._timerLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._timerLabel.Visible = false;
            this._timerLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._timerLabel.Position = new CCPoint(
                0.5f * headerRightSize.Width,
                0.3f * headerRightSize.Height);
            this._timerLabelText = string.Empty;
            this._headerRightLayer.AddChild(_timerLabel);
#if !NETFX_CORE
            this._timer = new Timer(TimerCallback);
#endif

            // Footer layers
            this._footerLayer = new GameLayer();
            this._footerLayer.ContentSize = footerSize;
            var footerLayerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                0.02f * screenSize.Width,
                0.02f * screenSize.Height);
            var footerLayerStartPosition = new CCPoint(
                footerLayerEndPosition.X,
                -footerSize.Height);
            this._footerLayer.Position = footerLayerEndPosition;
            this.AddChild(this._footerLayer);
            this._footerLeftLayer = new UILayer();
            this._footerLeftLayer.ContentSize = footerLeftSize;
            this._footerLayer.AddChild(this._footerLeftLayer);
            this._footerRightLayer = new UILayer();
            this._footerRightLayer.ContentSize = footerRightSize;
            this._footerRightLayer.Position = new CCPoint(
                0.8f * this._footerLayer.ContentSize.Width,
                0);
            this._footerLayer.AddChild(this._footerRightLayer);

            // Footer animations
            var footerLayerStartPlacementAction = new CCPlace(footerLayerStartPosition);
            var footerLayerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, footerLayerEndPosition);
            this._footerLayerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { footerLayerStartPlacementAction, footerLayerMoveInAction })
            );
            var footerLayerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, footerLayerStartPosition);
            this._footerLayerActionOut = new CCEaseBackIn(footerLayerMoveOutAction);
            this._hideFooterAnim = new CCScaleTo(GameConstants.DURATION_LAYER_TRANSITION, 0f);
            this._restoreFooterAnim = new CCEaseBackOut(new CCScaleTo(GameConstants.DURATION_LAYER_TRANSITION, 1f));

            // Joystick 
            // Notes: 
            // Start with enumaration not moving
            // Joystick label is on bottom so double tap has to be right in center of label, in case arrow buttons overlap
            this._joystickMoveDirection = MoveDirection.None;
            var joystickText = string.Empty;
#if ANDROID
            joystickText = Program.SharedProgram.Resources.GetString(Resource.String.HudJoystick);
#elif IOS
            joystickText = NSBundle.MainBundle.LocalizedString(Strings.HudJoystick, Strings.HudJoystick);
#else
            joystickText = AppResources.HudJoystick;
#endif
            this._joystickLabel = new CCLabelTTF(joystickText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._joystickLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._joystickLabel.Position = new CCPoint(
                0.3f * footerLeftSize.Width,
                0.5f * footerLeftSize.Height);
            this._footerLeftLayer.AddChild(this._joystickLabel);
            this._joystickLeftItem = new CCMenuItemImage(
                "Images/Icons/JoystickLeftNormal.png",
                "Images/Icons/JoystickLeftSelected.png",
                (obj) => { this._inputManager.HudOnJoystick(MoveDirection.Left); });
            var joystickLeftMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._joystickLeftItem
                    });
            joystickLeftMenu.AnchorPoint = CCPoint.AnchorMiddle;
            joystickLeftMenu.Position = new CCPoint(
                0.1f * footerLeftSize.Width,
                0.5f * footerLeftSize.Height);
            this._footerLeftLayer.AddChild(joystickLeftMenu);
            this._joystickRightItem = new CCMenuItemImage(
                "Images/Icons/JoystickRightNormal.png",
                "Images/Icons/JoystickRightSelected.png",
                (obj) => { this._inputManager.HudOnJoystick(MoveDirection.Right); });
            var joystickRightMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._joystickRightItem
                    });
            joystickRightMenu.AnchorPoint = CCPoint.AnchorMiddle;
            joystickRightMenu.Position = new CCPoint(
                0.5f * footerLeftSize.Width,
                0.5f  * footerLeftSize.Height);
            this._footerLeftLayer.AddChild(joystickRightMenu);
            this._joystickUpItem = new CCMenuItemImage(
                "Images/Icons/JoystickUpNormal.png",
                "Images/Icons/JoystickUpSelected.png",
                (obj) => { this._inputManager.HudOnJoystick(MoveDirection.Up); });
            var joystickUpMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._joystickUpItem
                    });
            joystickUpMenu.AnchorPoint = CCPoint.AnchorMiddle;
            joystickUpMenu.Position = new CCPoint(
                0.3f * footerLeftSize.Width,
                0.8f * footerLeftSize.Height);
            this._footerLeftLayer.AddChild(joystickUpMenu);
            this._joystickDownItem = new CCMenuItemImage(
                "Images/Icons/JoystickDownNormal.png",
                "Images/Icons/JoystickDownSelected.png",
                (obj) => { this._inputManager.HudOnJoystick(MoveDirection.Down); });
            var joystickDownMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._joystickDownItem
                    });
            joystickDownMenu.AnchorPoint = CCPoint.AnchorMiddle;
            joystickDownMenu.Position = new CCPoint(
                0.3f * footerLeftSize.Width,
                0.2f * footerLeftSize.Height);
            this._footerLeftLayer.AddChild(joystickDownMenu);

            // Trackball
            var trackballImage = new CCSprite("Images/Misc/Trackball.png");
            trackballImage.AnchorPoint = CCPoint.AnchorMiddle;
            trackballImage.Position = new CCPoint(
                0.8f * footerLeftSize.Width,
                0.6f * footerLeftSize.Height);
            this._footerLeftLayer.AddChild(trackballImage);
            var trackballDummyLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._trackballItem = new CCMenuItemLabel(trackballDummyLabel);
            this._trackballItem.ContentSize = new CCSize(
                0.4f * footerLeftSize.Width,
                footerLeftSize.Height);
            var trackballMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        this._trackballItem
                    });
            trackballMenu.AnchorPoint = CCPoint.AnchorMiddle;
            trackballMenu.Position = new CCPoint(
                0.8f * footerLeftSize.Width,
                0.5f * footerLeftSize.Height);
            this._footerLeftLayer.AddChild(trackballMenu);

            var trackballText = string.Empty;
#if ANDROID
            trackballText = Program.SharedProgram.Resources.GetString(Resource.String.HudTrackball);
#elif IOS
            trackballText = NSBundle.MainBundle.LocalizedString(Strings.HudTrackball, Strings.HudTrackball);
#else
            trackballText = AppResources.HudTrackball;
#endif
            var trackballLabel = new CCLabelTTF(trackballText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            trackballLabel.AnchorPoint = CCPoint.AnchorMiddle;
            trackballLabel.Position = new CCPoint(
                0.8f * footerLeftSize.Width,
                0.2f * footerLeftSize.Height);
            this._footerLeftLayer.AddChild(trackballLabel);

            // Pause toggle
            // IMPORTANT: Starts off disabled
            CCMenuItemImage pauseToggleOn =
                new CCMenuItemImage("Images/Icons/PauseButtonNormal.png",
                                    "Images/Icons/PauseButtonSelected.png",
                                    "Images/Icons/PauseButtonDisabled.png");
            CCMenuItemImage pauseToggleOff =
                new CCMenuItemImage("Images/Icons/ResumeButtonNormal.png",
                                    "Images/Icons/ResumeButtonSelected.png",
                                    "Images/Icons/ResumeButtonDisabled.png");
            this._pauseToggle =
                new CCMenuItemToggle((obj) => PauseTogglePressed((obj as CCMenuItemToggle).SelectedIndex),
                new CCMenuItem[] { pauseToggleOn, pauseToggleOff });
            var pauseMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        this._pauseToggle,
                    });
            pauseMenu.Position = new CCPoint(
                0.5f * footerRightSize.Width,
                0.6f * footerRightSize.Height);
            this._footerRightLayer.AddChild(pauseMenu);
            this._pauseText = string.Empty;
#if ANDROID
            this._pauseText = Program.SharedProgram.Resources.GetString(Resource.String.HudPause);
#elif IOS
            this._pauseText = NSBundle.MainBundle.LocalizedString(Strings.HudPause, Strings.HudPause);
#else
            this._pauseText = AppResources.HudPause;
#endif
            this._pausedText = string.Empty;
#if ANDROID
            this._pausedText = Program.SharedProgram.Resources.GetString(Resource.String.HudPaused);
#elif IOS
            this._pausedText = NSBundle.MainBundle.LocalizedString(Strings.HudPaused, Strings.HudPaused);
#else
            this._pausedText = AppResources.HudPaused;
#endif
            this._resumeText = string.Empty;
#if ANDROID
            this._resumeText = Program.SharedProgram.Resources.GetString(Resource.String.HudResume);
#elif IOS
            this._resumeText = NSBundle.MainBundle.LocalizedString(Strings.HudResume, Strings.HudResume);
#else
            this._resumeText = AppResources.HudResume;
#endif
            this._pauseLabel = new CCLabelTTF(this._pauseText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._pauseLabel.Position = new CCPoint(
                0.5f * footerRightSize.Width,
                0.2f * footerRightSize.Height);
            this._footerRightLayer.AddChild(this._pauseLabel);
            this.EnablePause(false);

            // Additional text
            this._doubleTapResetText = string.Empty;
#if ANDROID
            this._doubleTapResetText = Program.SharedProgram.Resources.GetString(Resource.String.CommonReset);
#elif IOS
            this._doubleTapResetText = NSBundle.MainBundle.LocalizedString(Strings.CommonReset, Strings.CommonReset);
#else
            this._doubleTapResetText = AppResources.CommonReset;
#endif
            this._hudSpeedText = string.Empty;
#if ANDROID
            this._hudSpeedText = Program.SharedProgram.Resources.GetString(Resource.String.HudSpeed);
#elif IOS
            this._hudSpeedText = NSBundle.MainBundle.LocalizedString(Strings.HudSpeed, Strings.HudSpeed);
#else
            this._hudSpeedText = AppResources.HudSpeed; 
#endif

#if ANDROID
            this._hudCannotGoSlowerThanText = Program.SharedProgram.Resources.GetString(Resource.String.HudCannotGoSlowerThan);
#elif IOS
            this._hudCannotGoSlowerThanText = NSBundle.MainBundle.LocalizedString(Strings.HudCannotGoSlowerThan, Strings.HudCannotGoSlowerThan);
#else
            this._hudCannotGoSlowerThanText = AppResources.HudCannotGoSlowerThan; 
#endif
            this._hudSpeedDefaultText = string.Empty;
#if ANDROID
            this._hudSpeedDefaultText = Program.SharedProgram.Resources.GetString(Resource.String.HudSpeedDefault);
#elif IOS
            this._hudSpeedDefaultText = NSBundle.MainBundle.LocalizedString(Strings.HudSpeed, Strings.HudSpeedDefault);
#else
            this._hudSpeedDefaultText = AppResources.HudSpeedDefault; 
#endif

            // Admin
#if DEBUG
            var adminText = string.Empty;
#if ANDROID
            adminText = Program.SharedProgram.Resources.GetString(Resource.String.CommonAdmin);
#elif IOS
            adminText = NSBundle.MainBundle.LocalizedString(Strings.CommonAdmin, Strings.CommonAdmin);
#else
            adminText = AppResources.CommonAdmin;
#endif
            var adminLabel = new CCLabelTTF(adminText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var adminItem = new CCMenuItemLabel(adminLabel,
                (obj) => { this.NavigateBase(LayerTags.AdminLayer); });
            var adminLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        adminItem
                    });
            adminLabelMenu.Position = new CCPoint(
                 0.5f * this.ContentSize.Width,
                -0.1f * this.ContentSize.Height);
            this.AddChild(adminLabelMenu);
#endif

        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // IMPORTANT: Had to place this here as it was previously in constructor
            // and it was not being enabled again when entering OnEnter()
            this.ScheduleUpdate();

            // High score display
            var scoreRepository = new FacebookScoreRepository();
            var highScore = scoreRepository.GetTopScoreForPlayer();
            if (highScore != null)
            {
                this.UpdateHighScore(highScore.Score.ToString(), highScore.ScoreTime.ToString(@"h\:mm\:ss"));
            }
            else
            {
                this.UpdateHighScore("0", string.Empty);
            }

            // Reset positioning of start image
            this._hudCameraPreviousOffsetX = -1f;
            this._hudCameraPreviousOffsetY = -1f;
            this._hudCameraPreviousOffsetYaw = -1f;
            this._hudCameraPreviousOffsetPitch = -1f;
            this._hudCameraPreviousOrbitYaw = -1f;
            this._hudCameraPreviousOrbitPitch = -1f;

            // Animate panes/layers
            this._headerLayer.RunAction(this._headerLayerActionIn);
            this._footerLayer.RunAction(this._footerLayerActionIn);

            // IMPORTANT: We do not want touches. We will only be handling
            //            responsed from menus and free drags.
            this.TouchEnabled = false;

            // Enable support for gestures
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete | GestureType.DoubleTap | GestureType.Hold;
            CCApplication.SharedApplication.OnGesture += this.HudOnGesture;

            // Careful here, we could be coming back from options, help etc. from a pause.
            // We only display our start tap if we are NOT coming back from a pause. The two
            // code paths (start tap vs pause/resume) could conflict in state restored.
            if (!this._characterCache.IsPaused())
            {
                // Activate tap animation
                this._startTapImage.RunAction(this._startTapAction);

                // Base layer on
                this._baseLayer.Visible = true;
            }
        }

        public override void Update(float dt)
        {
            // Update timer
            this._timerLabel.Text = this._timerLabelText;

            // Handle presses on joystick
            if (this._joystickMoveDirection != MoveDirection.None)
            {
                TouchCollection touchCollection = TouchPanel.GetState();
                if (touchCollection.Count == 0)
                {
                    this._joystickMoveDirection = MoveDirection.None;
                }
                else
                {
                    this._inputManager.HudOnJoystick(this._joystickMoveDirection);
                }
            }

            // If tap image visible, follow hero
            if (this._startTapImage.Visible)
            {
                // Do we need to adjust tap image?
                if (this._hudCameraPreviousOffsetX     != this._inputManager.HudCameraOffsetX || 
                    this._hudCameraPreviousOffsetY     != this._inputManager.HudCameraOffsetY ||
                    this._hudCameraPreviousOffsetYaw   != this._inputManager.HudCameraOffsetYaw ||
                    this._hudCameraPreviousOffsetPitch != this._inputManager.HudCameraOffsetPitch ||
                    this._hudCameraPreviousOrbitYaw    != this._inputManager.HudCameraOrbitYaw ||
                    this._hudCameraPreviousOrbitPitch  != this._inputManager.HudCameraOrbitPitch)
                {
                    // Make sure tracking camera is up to date
                    this._parent.TheActionLayer.UpdateTrackingCamera();

                    // Box out positioning around hero
                    var heroModel = this._characterCache.TheHeroModel;
                    var heroWorldOrigin = this._characterCache.TheHeroModel.WorldOrigin;
                    float xOffset = 0.5f * heroModel.WorldWidth;
                    if ((GameManager.SharedGameManager.GameStartLineNumber % 2) == 0)
                    {
                        xOffset = -xOffset;
                    }
                    var heroBottomMiddle = XNAUtils.WorldToLogical(new Vector3(
                        heroWorldOrigin.X + xOffset,
                        heroWorldOrigin.Y,
                        heroWorldOrigin.Z),
                        XNAUtils.CameraType.Tracking);

                    // Update start image to follow hero
                    this._startTapImage.Position = new CCPoint(
                        heroBottomMiddle.X,
                        heroBottomMiddle.Y - (0.1f * this.ContentSize.Height));

                    // Update state
                    this._hudCameraPreviousOffsetX     = this._inputManager.HudCameraOffsetX;
                    this._hudCameraPreviousOffsetY     = this._inputManager.HudCameraOffsetY;
                    this._hudCameraPreviousOffsetYaw   = this._inputManager.HudCameraOffsetYaw;
                    this._hudCameraPreviousOffsetPitch = this._inputManager.HudCameraOffsetPitch;
                    this._hudCameraPreviousOrbitYaw    = this._inputManager.HudCameraOrbitYaw;
                    this._hudCameraPreviousOrbitPitch  = this._inputManager.HudCameraOrbitPitch;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            // Restore scaling of secondary layers
            this._headerLeftLayer.ScaleX = 1f;
            this._headerLeftLayer.ScaleY = 1f;
            this._footerLeftLayer.ScaleX = 1f;
            this._footerLeftLayer.ScaleY = 1f;

            // Disable support for gestures
            TouchPanel.EnabledGestures = GestureType.None;
            CCApplication.SharedApplication.OnGesture -= this.HudOnGesture;

            // Stop tap animation
            this.ActionManager.RemoveAllActionsFromTarget(this._startTapImage);
        }

        #endregion

        #region Api

        public void EnablePause(bool enabled)
        {
            this._pauseToggle.SelectedIndex = 0;
            this._pauseLabel.Text = this._pauseText;

            if (enabled)
            {
                this._pauseToggle.Enabled = true;
                this._pauseLabel.Color = CCColor3B.White;
            }
            else
            {
                this._pauseToggle.Enabled = false;
                this._pauseLabel.Color = CCColor3B.Gray;
            }
        }

        public void HandleKill()
        {
            // Disable pause and reset pause text
            this.EnablePause(false);

            this.StopTimer();
        }

        public void RestoreStart()
        {
            // Base layer on
            this._baseLayer.Visible = true;

            // Restore secondary layers
            this.RestoreSecondaryLayers(displayHighScore: true);

            // Reset pause toggle
            this._pauseToggle.SelectedIndex = 0; // Pause visible
            this._pauseLabel.Text = this._pauseText;
        }

        public void DisplayScore(int score)
        {
            // Animate display of new score
            this._scoreLabel.StopAllActions();
            this._scoreLabel.Text =  score.ToString();
            this._scoreLabel.RunAction(this._scoreLabelAction);
        }

        public void UpdateHighScore(string newHighScore, string newHighTime)
        {
            this._highScoreLabel.Text = newHighScore;
            this._highScoreTimeLabel.Text = newHighTime;
        }

        public void DisplayPageLineNumber(int pageNumber, int lineNumber)
        {
            // Animate display of new page number
            // this._status1Label.StopAllActions();
            var pageLineText = string.Empty;
#if ANDROID
            pageLineText = Program.SharedProgram.Resources.GetString(Resource.String.HudPageLine);
#elif IOS
            pageLineText = NSBundle.MainBundle.LocalizedString(Strings.HudPageLine, Strings.HudPageLine);
#else
            pageLineText = AppResources.HudPageLine;
#endif
            var text = pageLineText + " " + pageNumber + "/" + lineNumber;

            this.UpdateStatus1(text);
        }

        public TimeSpan GetTime()
        {
            return this._elapsedTime;
        }

        /// <summary>
        /// Set to true if a "Kills Off" was enabled at start of a game run or
        /// if "Kills Off" was enabled during a game run. Set to false if a game
        /// run was completed without "Kills Off" enabled at any point in run.
        /// </summary>
        public bool KillsOffEventRecorded { get; set; }

        #endregion

        #region Event handlers

        private void HudOnGesture(CCGesture g)
        {
            switch (g.GestureType)
            {
                case GestureType.FreeDrag:
                    {
                        switch (this._dragging)
                        {
                            case Dragging.None:
                                {
                                    // Animate display of double tap help text
                                    this.UpdateStatus1(this._doubleTapResetText);

                                    // IMPORTANT: Gesture position differs from Touch position
                                    //            in that we have to invert the y value.
                                    var p = CCDirector.SharedDirector.ConvertToUi(g.Position);
                                    var sb = this._startButtonItem.WorldBoundingBox;
                                    var tb = this._trackballItem.WorldBoundingBox;
                                    if (CCRect.ContainsPoint(ref sb, ref p))
                                    {
                                        this._dragging = Dragging.Offset;
                                        this._inputManager.HudOnGestureOffset(g);
                                    }
                                    else if (CCRect.ContainsPoint(ref tb, ref p))
                                    {
                                        this._dragging = Dragging.Orbit;
                                        this._inputManager.HudOnGestureOrbit(g);
                                    }
                                    
                                    break;
                                }
                            case Dragging.Offset:
                                {
                                    this._inputManager.HudOnGestureOffset(g);
                                    break;
                                }
                           case Dragging.Orbit:
                                {
                                    this._inputManager.HudOnGestureOrbit(g);
                                    break;
                                }
                        }
                        break;
                    }
                case GestureType.DragComplete:
                    {
                        this._dragging = Dragging.None;
                        break;
                    }
                case GestureType.DoubleTap:
                    {
                        var p = CCDirector.SharedDirector.ConvertToUi(g.Position);
                        var tb = this._trackballItem.WorldBoundingBox;
                        var js = this._joystickLabel.WorldBoundingBox;
                        if (CCRect.ContainsPoint(ref tb, ref p))
                        {
                            this._inputManager.HudOnGestureReset();
                        }
                        /*
                        else if (CCRect.ContainsPoint(ref js, ref p))
                        {
                            this._inputManager.HudOnGestureCloseup();
                        }
                        */

                        break;
                    }
                case GestureType.Hold:
                    {
                        // Animate display of double tap help text
                        this.UpdateStatus1(this._doubleTapResetText);

                        var p = CCDirector.SharedDirector.ConvertToUi(g.Position);
                        var jl = this._joystickLeftItem.WorldBoundingBox;
                        var jr = this._joystickRightItem.WorldBoundingBox;
                        var ju = this._joystickUpItem.WorldBoundingBox;
                        var jd = this._joystickDownItem.WorldBoundingBox;
                        
                        if (CCRect.ContainsPoint(ref jl, ref p))
                        {
                            this._joystickMoveDirection = MoveDirection.Left;
                            this._inputManager.HudOnJoystick(this._joystickMoveDirection);
                        }
                        else if (CCRect.ContainsPoint(ref jr, ref p))
                        {
                            this._joystickMoveDirection = MoveDirection.Right;
                            this._inputManager.HudOnJoystick(this._joystickMoveDirection);
                        }
                        else if (CCRect.ContainsPoint(ref ju, ref p))
                        {
                            this._joystickMoveDirection = MoveDirection.Up;
                            this._inputManager.HudOnJoystick(this._joystickMoveDirection);
                        }
                        else if (CCRect.ContainsPoint(ref jd, ref p))
                        {
                            this._joystickMoveDirection = MoveDirection.Down;
                            this._inputManager.HudOnJoystick(this._joystickMoveDirection);
                        }

                        break;
                    }

            }
        }

        #endregion

        #region Helper methods

        private void StartTimer()
        {
            // IMPORTANT: Get this set before starting timer so initial time calculation is correct
            this._timerStartTime = DateTime.Now;
#if NETFX_CORE
            this._timer = ThreadPoolTimer.CreatePeriodicTimer(TimerCallback, TimeSpan.FromSeconds(1));
#else
            this._timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
#endif
        }

        private void StopTimer()
        {
#if NETFX_CORE
            this._timer.Cancel();
#else
            this._timer.Change(-1, -1);
#endif
        }

        private void HideSecondaryLayers()
        {
            this._headerLeftLayer.RunAction(this._hideHeaderAnim);
            this._footerLeftLayer.RunAction(this._hideFooterAnim);
        }

        private void RestoreSecondaryLayers(bool displayHighScore)
        {
            this._headerLeftLayer.RunAction(this._restoreHeaderAnim);
            this._footerLeftLayer.RunAction(this._restoreFooterAnim);

            this.ToggleScoreLabels(displayHighScore);
        }

        private void ToggleScoreLabels(bool displayHighScore)
        {
            if (displayHighScore)
            {
                // High score
                this._highScoreHeaderLabel.Visible = true;
                this._highScoreLabel.Visible = true;
                this._highScoreTimeHeaderLabel.Visible = true;
                this._highScoreTimeLabel.Visible = true;

                // Current score
                this._scoreLabel.Visible = false;
                this._timerLabel.Visible = false;

            }
            else
            {
                // High score
                this._highScoreHeaderLabel.Visible = false;
                this._highScoreLabel.Visible = false;
                this._highScoreTimeHeaderLabel.Visible = false;
                this._highScoreTimeLabel.Visible = false;

                // Current score
                this._scoreLabel.Visible = true;
                this._timerLabel.Visible = true;
            }
        }

        private void StartPressed()
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            // Sanity check, sometimes a race condition leaves us in a paused state
            this._paused = false;
            this._pauseLabel.Text = this._pauseText;
            this._pauseToggle.SelectedIndex = 0;
            this.UpdateStatus2(string.Empty);

            // Base layer off
            this._baseLayer.Visible = false;

            // Hide secondary layers
            this.HideSecondaryLayers();

            // Switch to display of current score and get timer going
            this.ToggleScoreLabels(displayHighScore: false);
            this.StartTimer();

            // Track if "Kills Off" is enabled during this game run
            this.KillsOffEventRecorded = !GameManager.SharedGameManager.GameAreKillsAllowed;

            // Enable pause and set pause text
            this.EnablePause(true);

            // Get game going
            this._parent.TheActionLayer.SwitchState(GameState.Moving);

            // Sanity check, sometimes a race condition leaves us in a paused state
            if (this._characterCache.IsPaused())
            {
                this._characterCache.Pause(false);
            }
        }

        private void PauseTogglePressed(int selectedIndex)
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            if (selectedIndex == 0) // Resume
            {
                // Set state
                this._paused = false;

                // Get hero moving again
                this._characterCache.Pause(false);

                // Flip menu text
                this._pauseLabel.Text = this._pauseText;

                // Clear out status 2 label
                this.UpdateStatus2(string.Empty);

                // Hide secondary layers
                this.HideSecondaryLayers();
            }
            else                    // Pause
            {
                // Set state
                this._paused = true;

                // Put hero in holding position
                this._characterCache.Pause(true);

                // Flip menu text
                this._pauseLabel.Text = this._resumeText;

                // Update status 2 label
                this.UpdateStatus2(this._pausedText);

                // Restore secondary layers
                this.RestoreSecondaryLayers(displayHighScore: false);
            }
        }

        private void ZoomTogglePressed(int selectedIndex)
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            if (selectedIndex == 0) // Zoom out
            {
                this._inputManager.HudOnZoom(zoomIn: false);
            }
            else                    // Zoom in
            {
                this._inputManager.HudOnZoom(zoomIn: true);
            }
        }

        private void UpdateStatus1(string text)
        {
            // Lazy initialization
            if (this._status1Label == null)
            {
                this._status1Label = new CCLabelTTF(text, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                this._status1Label.Color = CCColor3B.Red;
                this._status1Label.AnchorPoint = CCPoint.AnchorMiddle;
                this._status1Label.Position = new CCPoint(
                    0.5f  * this.ContentSize.Width,
                    0.75f * this.ContentSize.Height);
#if IOS
                // With the ad banner overlaying our screen in ios, we need to adjust certain ui elements downward by size of ad banner
                this._status1Label.Position = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                    0.5f  * this.ContentSize.Width,
                    0.75f * this.ContentSize.Height - (float)GADAdSizeCons.Banner.Size.Height);
#endif
                this.AddChild(this._status1Label);
            }

            // Clear out any previous actions
            this.ActionManager.RemoveAllActionsFromTarget(this._status1Label);

            // Can't set empty strings, so just turn visibility off
            if (text == string.Empty)
            {
                this._status1Label.Visible = false;
            }
            else
            {
                // Normal update of text, action will also turn visibility back on for short duration
                this._status1Label.Text = text;
                this._status1Label.RunAction(this._status1LabelAction);
            }
        }

        private void UpdateStatus2(string text)
        {
            // Lazy initialization
            if (this._status2Label == null)
            {
                this._status2Label = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                this._status2Label.Color = CCColor3B.Red;
                this._status2Label.AnchorPoint = CCPoint.AnchorMiddle;
                this._status2Label.Position = new CCPoint(
                    0.5f * this.ContentSize.Width,
                    0.65f * this.ContentSize.Height);
#if IOS
                // With the ad banner overlaying our screen in ios, we need to adjust certain ui elements downward by size of ad banner
                this._status2Label.Position = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                    0.5f * this.ContentSize.Width,
                    0.65f * this.ContentSize.Height - (float)GADAdSizeCons.Banner.Size.Height);
#endif
                this.AddChild(this._status2Label);
            }
            
            // Clear out any previous actions
            this.ActionManager.RemoveAllActionsFromTarget(this._status2Label);

            // Can't set empty strings, so just turn visibility off
            if (text == string.Empty)
            {
                this._status2Label.Visible = false;
            }
            else
            {
                // Normal update of text, action will also turn visibility back on
                this._status2Label.Text = text;
                this._status2Label.RunAction(this._status2LabelAction);
            }
        }

        private void DecreaseVelocity()
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            // Attempt to decrease velocity then determine an appropriate status text
            // based on if we have hit our lower velocity limit or not
            var statusText = string.Empty;
            if (this._characterCache.DecreaseVelocity())
            {
                statusText = this._hudSpeedText;
            }
            else
            {
                statusText = this._hudCannotGoSlowerThanText;
            }

            var newSpeed = this._characterCache.GetLinearVelocityX();
            var newSpeedText = newSpeed.ToString();
            if (newSpeed == GameConstants.USER_DEFAULT_INITIAL_HERO_LINEAR_VELOCITY_X)
            {
                this.UpdateStatus1(statusText + " " + newSpeedText + " " + this._hudSpeedDefaultText);
            }
            else
            {
                this.UpdateStatus1(statusText + " " + newSpeedText);
            }
        }

        private void IncreaseVelocity()
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            this._characterCache.IncreaseVelocity();


            var newSpeed = this._characterCache.GetLinearVelocityX();
            var newSpeedText = newSpeed.ToString();
            if (newSpeed == GameConstants.USER_DEFAULT_INITIAL_HERO_LINEAR_VELOCITY_X)
            {
                this.UpdateStatus1(this._hudSpeedText + " " + newSpeedText + " " + this._hudSpeedDefaultText);
            }
            else
            {
                this.UpdateStatus1(this._hudSpeedText + " " + newSpeedText);
            }
        }

        private void NavigateBase(LayerTags layer)
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            this._parent.Navigate(layer);
        }

#if NETFX_CORE
        private void TimerCallback(ThreadPoolTimer timer)
#else
        private void TimerCallback(object timer)
#endif
        {
            this._elapsedTime = DateTime.Now - this._timerStartTime;
            this._timerLabelText = this._elapsedTime.ToString(@"h\:mm\:ss");
        }

        #endregion
    }
}