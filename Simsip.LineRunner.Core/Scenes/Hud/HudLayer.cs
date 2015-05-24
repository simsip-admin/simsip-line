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
using Simsip.LineRunner.SneakyJoystick;
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
#endif
#if IOS
using Foundation;
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
        private CCAction _startTapAction;
        private CCMenuItemLabel _startButtonItem;

        // Tap text
        private CCLabelTTF _startTapDescription1;
        private CCLabelTTF _startTapDescription2;

        // Base layer
        private GameLayer _baseLayer;

        // Header layer
        private UILayer _headerLayer;
        private CCAction _headerLayerActionIn;
        private CCAction _headerLayerActionOut;

        // Footer pane/layer
        private UILayer _footerLayer;
        private CCAction _footerLayerActionIn;
        private CCAction _footerLayerActionOut;
        
        // Trackball
        private CCMenuItemLabel _trackballItem;

        // Top score
        private CCLabelTTF _topScoreLabel;

        // Status label
        private CCLabelTTF _statusLabel;
        private CCAction _statusLabelAction;

        // Score label
        private CCLabelTTF _scoreLabel;
        private CCAction _scoreLabelAction;

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

        // Pause/resume
        private bool _paused;
        private string _pauseText;
        private string _resumeText;

        // Dragging support
        private enum Dragging
        {
            None,
            Offset,
            Orbit
        }
        private Dragging _dragging;

        // Free flight/resume
        private bool _inFlight;
        private CCLabelTTF _flightLabel;
        private CCMenu _flightMenu;
        
        private GameState _currentGameState;

        public HudLayer(CoreScene parent)
        {
            this._parent = parent;

            // Not draffing anything at start
            this._dragging = Dragging.None;

            // Grab rerences to services we'll need
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
            this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                screenSize.Width,
                screenSize.Height);
            var baseContentSize = new CCSize(   // IMPORTANT: Needs to be full size for proper placement of hero, see OnEnter()
                screenSize.Width,
                screenSize.Height);
            var headerContentSize = new CCSize(
                0.96f * screenSize.Width,
                0.2f  * screenSize.Height);
            var footerContentSize = new CCSize(
                0.96f * screenSize.Width,
                0.2f * screenSize.Height);

            // Base layer
            this._baseLayer = new GameLayer();
            this._baseLayer.ContentSize = baseContentSize;
            this._baseLayer.Position = new CCPoint(
                0,
                0);
            this.AddChild(this._baseLayer);

            // Start button
            // Transparent button sized/positioned between header/footer
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

            // Tap text
            var startTapText = string.Empty;
#if ANDROID
            startTapText = Program.SharedProgram.Resources.GetString(Resource.String.StartTap);
#elif IOS
            startTapText = NSBundle.MainBundle.LocalizedString(Strings.StartTap, Strings.StartTap);
#else
            startTapText = AppResources.StartTap;
#endif
            this._startTapDescription1 = new CCLabelTTF(startTapText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._startTapDescription1.AnchorPoint = CCPoint.AnchorMiddleRight;
            this._startTapDescription1.Color = CCColor3B.Green;
            this._baseLayer.AddChild(this._startTapDescription1);
            this._startTapDescription2 = new CCLabelTTF(startTapText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._startTapDescription2.AnchorPoint = CCPoint.AnchorMiddleLeft;
            this._startTapDescription2.Color = CCColor3B.Green;
            this._baseLayer.AddChild(this._startTapDescription2);

            // Header pane transition in/out
            var headerLayerEndPosition = new CCPoint(
                0.02f * screenSize.Width,
                0.78f * screenSize.Height);
            var headerLayerStartPosition = new CCPoint(
                headerLayerEndPosition.X, 
                screenSize.Height);

            // Header layer
            this._headerLayer = new UILayer();
            this._headerLayer.ContentSize = headerContentSize;
            this._headerLayer.Position = headerLayerEndPosition;
            this.AddChild(this._headerLayer);

            // Header layer transition in/out
            var headerLayerStartPlacementAction = new CCPlace(headerLayerStartPosition);
            var headerLayerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, headerLayerEndPosition);
            this._headerLayerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { headerLayerStartPlacementAction, headerLayerMoveInAction })
            );
            var headerLayerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, headerLayerStartPosition);
            this._headerLayerActionOut = new CCEaseBackIn(headerLayerMoveOutAction);

            // Trackball
            var trackballImage = new CCSprite("Images/Misc/Trackball.png");
            trackballImage.AnchorPoint = CCPoint.AnchorMiddle;
            trackballImage.Position = new CCPoint(
                0.2f * headerContentSize.Width,
                0.6f * headerContentSize.Height);
            this._headerLayer.AddChild(trackballImage);
            var trackballDummyLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._trackballItem = new CCMenuItemLabel(trackballDummyLabel);
            this._trackballItem.ContentSize = new CCSize(
                0.4f * headerContentSize.Width,
                headerContentSize.Height);
            var trackballMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        this._trackballItem
                    });
            trackballMenu.AnchorPoint = CCPoint.AnchorMiddle;
            trackballMenu.Position = new CCPoint(
                0.2f * this._headerLayer.ContentSize.Width,
                0.5f * this._headerLayer.ContentSize.Height);
            this._headerLayer.AddChild(trackballMenu);

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
                0.2f * headerContentSize.Width,
                0.2f * headerContentSize.Height);
            this._headerLayer.AddChild(trackballLabel);

            // Header divider left
            var headerDividerLeft = new CCSprite("Images/Misc/MenuDivider.png");
            headerDividerLeft.AnchorPoint = CCPoint.AnchorMiddle;
            headerDividerLeft.Opacity = 128;
            headerDividerLeft.Position = new CCPoint(
                0.4f * headerContentSize.Width,
                0.5f * headerContentSize.Height);
            this._headerLayer.AddChild(headerDividerLeft);

            // Top score
            var topScoreHeaderText = string.Empty;
#if ANDROID
            topScoreHeaderText = Program.SharedProgram.Resources.GetString(Resource.String.HudTopScore);
#elif IOS
            topScoreHeaderText = NSBundle.MainBundle.LocalizedString(Strings.HudTopScore, Strings.HudTopScore);
#else
            topScoreHeaderText = AppResources.HudTopScore;
#endif
            var topScoreHeaderLabel = new CCLabelTTF(topScoreHeaderText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            topScoreHeaderLabel.AnchorPoint = CCPoint.AnchorMiddle;
            topScoreHeaderLabel.Position = new CCPoint(
                0.58f * headerContentSize.Width,
                0.8f  * headerContentSize.Height);
            this._headerLayer.AddChild(topScoreHeaderLabel);
            this._topScoreLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._topScoreLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._topScoreLabel.Position = new CCPoint(
                0.58f * headerContentSize.Width,
                0.6f  * headerContentSize.Height);
            this._headerLayer.AddChild(this._topScoreLabel);

            // Status
            this._statusLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._statusLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._statusLabel.Position = new CCPoint(
                0.58f * headerContentSize.Width,
                0.2f  * headerContentSize.Height);
            this._headerLayer.AddChild(this._statusLabel);

            // Header divider right
            var headerDividerRight = new CCSprite("Images/Misc/MenuDivider.png");
            headerDividerRight.AnchorPoint = CCPoint.AnchorMiddle;
            headerDividerRight.Opacity = 128;
            headerDividerRight.Position = new CCPoint(
                0.76f * headerContentSize.Width,
                0.5f  * headerContentSize.Height);
            this._headerLayer.AddChild(headerDividerRight);

            // Score label
            this._scoreLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_EXTRA_LARGE);
            this._scoreLabel.Color = CCColor3B.Green;
            this._statusLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._scoreLabel.Position = new CCPoint(
                0.88f * headerContentSize.Width, 
                0.7f  * headerContentSize.Height);
            this._headerLayer.AddChild(this._scoreLabel);

            // Score label action
            var scaleStartScore = new CCScaleTo(0f, 0f);
            var scaleUpScore = new CCScaleTo(0.5f, 1.2f);
            var scaleBackScore = new CCScaleTo(0.1f, 1.0f);
            this._scoreLabelAction = new CCSequence(new CCFiniteTimeAction[] { scaleStartScore, scaleUpScore, scaleBackScore });

            // Page number label action
            var scaleStartPageNumber = new CCScaleTo(0f, 0f);
            var scaleUpPageNumber = new CCScaleTo(0.5f, 1.2f);
            var scaleBackPageNumber = new CCScaleTo(0.1f, 1.0f);
            this._statusLabelAction = new CCSequence(new CCFiniteTimeAction[] { scaleStartPageNumber, scaleUpPageNumber, scaleBackPageNumber });

            // Timer
            this._timerLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._timerLabel.AnchorPoint = CCPoint.AnchorMiddle;
            this._timerLabel.Position = new CCPoint(
                0.88f * headerContentSize.Width,
                0.4f  * headerContentSize.Height);
            this._timerLabelText = string.Empty;
            this._headerLayer.AddChild(_timerLabel);
#if !NETFX_CORE
            this._timer = new Timer(TimerCallback);
#endif

            // Footer pane model
            var footerLayerEndPosition = new CCPoint(
                0.02f * screenSize.Width,
                0.02f * screenSize.Height);
            var footerLayerStartPosition = new CCPoint(
                footerLayerEndPosition.X,
                -footerContentSize.Height);

            // Footer layer
            this._footerLayer = new UILayer();
            this._footerLayer.ContentSize = footerContentSize;
            this._footerLayer.Position = footerLayerEndPosition;
            this.AddChild(this._footerLayer);

            // Footer layer transition in/out
            var footerLayerStartPlacementAction = new CCPlace(footerLayerStartPosition);
            var footerLayerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, footerLayerEndPosition);
            this._footerLayerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { footerLayerStartPlacementAction, footerLayerMoveInAction })
            );
            var footerLayerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, footerLayerStartPosition);
            this._footerLayerActionOut = new CCEaseBackIn(footerLayerMoveOutAction);

            // Joystick (note: start with enumaration not moving)
            this._joystickMoveDirection = MoveDirection.None;
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
                0.05f * footerContentSize.Width,
                0.5f * footerContentSize.Height);
            this._footerLayer.AddChild(joystickLeftMenu);
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
                0.35f * footerContentSize.Width,
                0.5f  * footerContentSize.Height);
            this._footerLayer.AddChild(joystickRightMenu);
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
                0.2f  * footerContentSize.Width,
                0.75f * footerContentSize.Height);
            this._footerLayer.AddChild(joystickUpMenu);
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
                0.2f  * footerContentSize.Width,
                0.25f * footerContentSize.Height);
            this._footerLayer.AddChild(joystickDownMenu);
            var joystickText = string.Empty;
#if ANDROID
            joystickText = Program.SharedProgram.Resources.GetString(Resource.String.HudJoystick);
#elif IOS
            joystickText = NSBundle.MainBundle.LocalizedString(Strings.HudJoystick, Strings.HudJoystick);
#else
            joystickText = AppResources.HudJoystick;
#endif
            var joystickLabel = new CCLabelTTF(joystickText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            joystickLabel.AnchorPoint = CCPoint.AnchorMiddle;
            joystickLabel.Position = new CCPoint(
                0.2f * footerContentSize.Width,
                0.5f * footerContentSize.Height);
            this._footerLayer.AddChild(joystickLabel);


            // Footer divider
            var footerDivider = new CCSprite("Images/Misc/MenuDivider.png");
            footerDivider.AnchorPoint = CCPoint.AnchorMiddle;
            footerDivider.Opacity = 128;
            footerDivider.Position = new CCPoint(
                0.42f * headerContentSize.Width,
                0.5f  * headerContentSize.Height);
            this._footerLayer.AddChild(footerDivider);

            // Pause toggle
            CCMenuItemImage pauseToggleOn =
                new CCMenuItemImage("Images/Icons/PauseButtonNormal.png",
                                    "Images/Icons/ResumeButtonNormal.png");
            CCMenuItemImage pauseToggleOff =
                new CCMenuItemImage("Images/Icons/ResumeButtonNormal.png",
                                    "Images/Icons/PauseButtonNormal.png");
            CCMenuItemToggle pauseToggle =
                new CCMenuItemToggle((obj) => PauseTogglePressed(),
                new CCMenuItem[] { pauseToggleOn, pauseToggleOff });
            var pauseMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        pauseToggle,
                    });
            pauseMenu.Position = new CCPoint(
                0.53f * footerContentSize.Width,
                0.6f  * footerContentSize.Height);
            this._footerLayer.AddChild(pauseMenu);
            this._pauseText = string.Empty;
#if ANDROID
            this._pauseText = Program.SharedProgram.Resources.GetString(Resource.String.HudPause);
#elif IOS
            this._pauseText = NSBundle.MainBundle.LocalizedString(Strings.HudPause, Strings.HudPause);
#else
            this._pauseText = AppResources.HudPause;
#endif
            var pauseLabel = new CCLabelTTF(this._pauseText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var pauseItem = new CCMenuItemLabel(pauseLabel,
                (obj) => { this.PauseTogglePressed(); });
            var pauseLabelMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        pauseItem
                    });
            pauseLabelMenu.Position = new CCPoint(
                0.53f  * footerContentSize.Width,
                0.25f * footerContentSize.Height);
            this._footerLayer.AddChild(pauseLabelMenu);

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
                0.05f * footerContentSize.Width);
            speedMenu.AnchorPoint = CCPoint.AnchorMiddle;
            speedMenu.Position = new CCPoint(
                0.82f * footerContentSize.Width,
                0.6f  * footerContentSize.Height);
            this._footerLayer.AddChild(speedMenu);

            var speedText = string.Empty;
#if ANDROID
            speedText = Program.SharedProgram.Resources.GetString(Resource.String.HudSpeed);
#elif IOS
            speedText = NSBundle.MainBundle.LocalizedString(Strings.HudSpeed, Strings.HudSpeed);
#else
            speedText = AppResources.HudSpeed;
#endif
            var speedLabel = new CCLabelTTF(speedText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            speedLabel.Position = new CCPoint(
                0.82f * footerContentSize.Width,
                0.25f * footerContentSize.Height);
            this._footerLayer.AddChild(speedLabel);

            // Admin
#if DEBUG
            var adminText = string.Empty;
#if ANDROID
            adminText = Program.SharedProgram.Resources.GetString(Resource.String.StartAdmin);
#elif IOS
            adminText = NSBundle.MainBundle.LocalizedString(Strings.StartAdmin, Strings.StartAdmin);
#else
            adminText = AppResources.StartAdmin;
#endif
            var adminLabel = new CCLabelTTF(adminText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var adminItem = new CCMenuItemLabel(adminLabel,
                (obj) => { this.NavigateAdmin(); });
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


            // TODO: Add in when Worlds are ready
            /*
            var freeFlightText = string.Empty;
#if ANDROID
            freeFlightText = Program.SharedProgram.Resources.GetString(Resource.String.HudFreeFlight);
#elif IOS
            freeFlightText = NSBundle.MainBundle.LocalizedString(Strings.HudFreeFlight, Strings.HudFreeFlight);
#else
            freeFlightText = AppResources.HudFreeFlight;
#endif
            this._flightLabel = new CCLabelTTF(freeFlightText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            var flightButton = new CCMenuItemLabel(this._flightLabel,
                                                 (obj) => { this.ToggleWorld(); });
            this._flightMenu = new CCMenu(
               new CCMenuItem[] 
                    {
                        flightButton
                    });
            this._flightMenu.Position = new CCPoint(
                0.5f * headerContentSize.Width,
                0.15f * headerContentSize.Height);
            this._headerLayer.AddChild(this._flightMenu);
            */
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // IMPORTANT: Had to place this here as it was previously in constructor
            // and it was not being enabled again when entering OnEnter()
            this.ScheduleUpdate();

            // Top score display
            var scoreRepository = new FacebookScoreRepository();
            var topScore = scoreRepository.GetTopScoreForPlayer();
            if (topScore != null)
            {
                var inText = string.Empty;
#if ANDROID
                inText = Program.SharedProgram.Resources.GetString(Resource.String.HudIn);
#elif IOS
                inText = NSBundle.MainBundle.LocalizedString(Strings.HudIn, Strings.HudIn);
#else
                inText = AppResources.HudIn;
#endif
                this._topScoreLabel.Text = 
                    topScore.Score.ToString()  + " " + 
                    inText + " " + 
                    topScore.ScoreTime.ToString(@"h\:mm\:ss");
            }

            // Animate panes/layers
            this._headerLayer.RunAction(this._headerLayerActionIn);
            this._footerLayer.RunAction(this._footerLayerActionIn);

            // Initialize timer display
#if NETFX_CORE
            this._timer = ThreadPoolTimer.CreatePeriodicTimer(TimerCallback, TimeSpan.FromSeconds(1));
#else
            this._timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
#endif
            this._timerStartTime = DateTime.Now;

            // IMPORTANT: We do not want touches. We will only be handling
            //            responsed from menus and free drags.
            this.TouchEnabled = false;

            // Enable support for gestures
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete | GestureType.DoubleTap | GestureType.Hold;
            CCApplication.SharedApplication.OnGesture += this.HudOnGesture;

            // Box out positioning around hero
            var heroStartOrigin = this._pageCache.CurrentPageModel.HeroStartOrigin;
            var heroModel = this._characterCache.TheHeroModel;
            var heroBottomMiddle = XNAUtils.WorldToLogical(new Vector3(
                heroStartOrigin.X + (0.5f * heroModel.WorldWidth),
                heroStartOrigin.Y,
                heroStartOrigin.Z),
                XNAUtils.CameraType.Tracking);
            var heroMiddleLeft = XNAUtils.WorldToLogical(new Vector3(
                heroStartOrigin.X,
                heroStartOrigin.Y + (0.5f * heroModel.WorldHeight),
                heroStartOrigin.Z),
                XNAUtils.CameraType.Tracking);
            var heroMiddleRight = XNAUtils.WorldToLogical(new Vector3(
                heroStartOrigin.X + heroModel.WorldWidth,
                heroStartOrigin.Y + (0.5f * heroModel.WorldHeight),
                heroStartOrigin.Z),
                XNAUtils.CameraType.Tracking);

            // Base layer on
            this._baseLayer.Visible = true;

            // Start image
            this._startTapImage.Position = new CCPoint(
                heroBottomMiddle.X,
                heroBottomMiddle.Y - (0.2f * this.ContentSize.Height));
            this._startTapImage.RunAction(this._startTapAction);

            // Tap text
            this._startTapDescription1.Position = new CCPoint(
                heroMiddleLeft.X - (0.1f * this.ContentSize.Width),
                heroMiddleLeft.Y);
            this._startTapDescription2.Position = new CCPoint(
                heroMiddleRight.X + (0.1f * this.ContentSize.Width),
                heroMiddleRight.Y);
        }

        public override void Update(float dt)
        {
            this._timerLabel.Text = this._timerLabelText;

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
        }

        public override void OnExit()
        {
            base.OnExit();

            // Turn off timer display
#if NETFX_CORE
            this._timer.Cancel();
#else
            this.StopTimer();
#endif

            // Disable support for gestures
            TouchPanel.EnabledGestures = GestureType.None;
            CCApplication.SharedApplication.OnGesture -= this.HudOnGesture;
        }

        #endregion

        #region Api

        public void DisplayScore(int score)
        {
            // Animate display of new score
            this._scoreLabel.StopAllActions();
            this._scoreLabel.Text =  score.ToString();
            this._scoreLabel.RunAction(this._scoreLabelAction);
        }

        public void DisplayPageLineNumber(int pageNumber, int lineNumber)
        {
            // Animate display of new page number
            this._statusLabel.StopAllActions();
            var pageLineText = string.Empty;
#if ANDROID
            pageLineText = Program.SharedProgram.Resources.GetString(Resource.String.HudPageLine);
#elif IOS
            pageLineText = NSBundle.MainBundle.LocalizedString(Strings.HudPageLine, Strings.HudPageLine);
#else
            pageLineText = AppResources.HudPageLine;
#endif
            this._statusLabel.Text = 
                pageLineText + " " + pageNumber.ToString() + "/" + lineNumber.ToString();
            this._statusLabel.Color = CCColor3B.White;
            this._statusLabel.RunAction(this._statusLabelAction);
        }

        public void SwitchState(GameState gameState)
        {
            this._currentGameState = gameState;

            /* TODO Add in when world is ready
            if (this._currentGameState == GameState.Moving ||
                this._currentGameState == GameState.World)
            {
                this._flightMenu.Enabled = true;
            }
            else
            {
                this._flightMenu.Enabled = false;
            }
            */
        }

        public void StopTimer()
        {
#if NETFX_CORE
            this._timer.Cancel();
#else
            this._timer.Change(-1, -1);
#endif
        }

        public TimeSpan GetTime()
        {
            return this._elapsedTime;
        }

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
                        if (CCRect.ContainsPoint(ref tb, ref p))
                        {
                            this._inputManager.HudOnGestureReset();
                        }

                        break;
                    }
                case GestureType.Hold:
                    {
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

        private void HudStickStartEndEvent(object sender, EventCustom e)
        {
            this._inputManager.HudStickStartEndEvent(sender, e);
        }

        private void HudButtonStartEndEvent(object sender, EventCustom e)
        {
            var sneakyButtonEventResponse = e.UserData as SneakyButtonEventResponse;
            var sneakyButtonStatus = sneakyButtonEventResponse.ResponseType;
            var sneakyButtonId = sneakyButtonEventResponse.ID;

            if (sneakyButtonStatus != SneakyButtonStatus.Press)
            {
                return;
            }

            var requiresStatus = false;
            MoveDirection moveDirection = (MoveDirection)sneakyButtonId;
            this._inputManager.HudButtonStartEndEvent(sender, e);

            if (requiresStatus)
            {
                var statusText = string.Empty;
#if ANDROID
                statusText = Program.SharedProgram.Resources.GetString(Resource.String.HudJoystickLimit);
#elif IOS
                statusText = NSBundle.MainBundle.LocalizedString(Strings.HudJoystickLimit, Strings.HudJoystickLimit);
#else
                statusText = AppResources.HudJoystickLimit;
#endif
                this._statusLabel.Text = statusText; ;

                // Clear out any previous activity for status label
                this._statusLabel.ActionManager.RemoveAllActionsFromTarget(this);

                // Construct and run an action to display the status for only a short duration
                var statusLabelAction = new CCSequence(new CCFiniteTimeAction[]
                {
                    new CCDelayTime(GameConstants.DURATION_STATUS_LABEL),
                    new CCCallFunc(() => this._statusLabel.Text = string.Empty)
                }
                );
                this._statusLabel.RunAction(statusLabelAction);
            }
        }

        #endregion

        #region Helper methods

        private void StartPressed()
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            // Base layer off
            this._baseLayer.Visible = false;

            // Get game going
            this._parent.TheActionLayer.SwitchState(GameState.Moving);
        }

        private void PauseTogglePressed()
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            if (this._paused)
            {
                this._paused = false;
                this._characterCache.Pause(false);

                // Clear out any previous activity for status label
                // and clear out the label itself
                this._statusLabel.ActionManager.RemoveAllActionsFromTarget(this);
                this._statusLabel.Color = CCColor3B.White;
                this._statusLabel.Text = string.Empty;
            }
            else
            {
                this._paused = true;
                this._characterCache.Pause(true);

                var statusText = string.Empty;
#if ANDROID
                statusText = Program.SharedProgram.Resources.GetString(Resource.String.HudPaused);
#elif IOS
                statusText = NSBundle.MainBundle.LocalizedString(Strings.HudPaused, Strings.HudPaused);
#else
                statusText = AppResources.HudPaused;
#endif
                // Clear out any previous activity for status label
                // and set new text
                this._statusLabel.ActionManager.RemoveAllActionsFromTarget(this);
                this._statusLabel.Color = CCColor3B.Red;
                this._statusLabel.Text = statusText;
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
#if ANDROID
                statusText = Program.SharedProgram.Resources.GetString(Resource.String.HudSpeed);
#elif IOS
                statusText = NSBundle.MainBundle.LocalizedString(Strings.HudSpeed, Strings.HudSpeed);
#else
                statusText = AppResources.HudSpeed; 
#endif
            }
            else
            {
                #if ANDROID
                    statusText = Program.SharedProgram.Resources.GetString(Resource.String.HudCannotGoSlowerThan);
                #elif IOS
                    statusText = NSBundle.MainBundle.LocalizedString(Strings.HudCannotGoSlowerThan, Strings.HudCannotGoSlowerThan);
                #else
                    statusText = AppResources.HudCannotGoSlowerThan; 
                #endif
            }
            this._statusLabel.Text = statusText + " " + (this._characterCache.GetLinearVelocityX() * 100);

            // Clear out any previous activity for status label
            this._statusLabel.ActionManager.RemoveAllActionsFromTarget(this);

            // Construct and run an action to display the status for only a short duration
            var statusLabelAction = new CCSequence(new CCFiniteTimeAction[]
                    {
                        new CCDelayTime(GameConstants.DURATION_STATUS_LABEL),
                        new CCCallFunc(() => this._statusLabel.Text = string.Empty)
                    }
                );
            this._statusLabel.RunAction(statusLabelAction);
        }

        private void IncreaseVelocity()
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            this._characterCache.IncreaseVelocity();

            var statusText = string.Empty;
#if ANDROID
            statusText = Program.SharedProgram.Resources.GetString(Resource.String.HudSpeed);
#elif IOS
            statusText = NSBundle.MainBundle.LocalizedString(Strings.HudSpeed, Strings.HudSpeed);
#else
            statusText = AppResources.HudSpeed; 
#endif
            this._statusLabel.Text = statusText + " " + (this._characterCache.GetLinearVelocityX() * 100);

            // Clear out any previous activity for status label
            this._statusLabel.ActionManager.RemoveAllActionsFromTarget(this);

            // Construct and run an action to display the status for only a short duration
            var statusLabelAction = new CCSequence(new CCFiniteTimeAction[]
                {
                    new CCDelayTime(GameConstants.DURATION_STATUS_LABEL),
                    new CCCallFunc(() => this._statusLabel.Text = string.Empty)
                }
            );
            this._statusLabel.RunAction(statusLabelAction);
        }

        private void NavigateAdmin()
        {
            // Short-circuit if dragging
            if (this._dragging != Dragging.None)
            {
                return;
            }

            if (!this._paused)
            {
                this.PauseTogglePressed();
            }

            this._parent.Navigate(LayerTags.AdminLayer);
        }

        private void ToggleWorld()
        {
            // TODO: Move to better setup
            // this._deferredShadowMapping.CaptureSnapshots();

            // Are we "resuming" from a free flight?
            if (this._inFlight)
            {
                // Update state
                this._inFlight = false;

                // Update ui
                var freeFlightText = string.Empty;
#if ANDROID
            freeFlightText = Program.SharedProgram.Resources.GetString(Resource.String.HudFreeFlight);
#elif IOS
            freeFlightText = NSBundle.MainBundle.LocalizedString(Strings.HudFreeFlight, Strings.HudFreeFlight);
#else
                freeFlightText = AppResources.HudFreeFlight;
#endif
                this._flightLabel.Text = freeFlightText;

                // Switch state so we resume from flight
                this._parent.TheActionLayer.ResumeFromWorld();
            }
            else
            {
                // Ok, we are starting a free flight
                
                // Update state
                this._inFlight = true;

                // Update ui
                var resumeText = string.Empty;
#if ANDROID
                resumeText = Program.SharedProgram.Resources.GetString(Resource.String.HudResume);
#elif IOS
                resumeText = NSBundle.MainBundle.LocalizedString(Strings.HudResume, Strings.HudResume);
#else
                resumeText = AppResources.HudResume;
#endif
                this._flightLabel.Text = resumeText;
                
                // Switch state to flight
                this._parent.TheActionLayer.StartWorld();
            }
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