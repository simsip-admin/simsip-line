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
#if NETFX_CORE
using Windows.System.Threading;
#else
using System.Threading;
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
        private ICharacterCache _characterCache;
        private IInputManager _inputManager;

        // Header layer
        private UILayer _headerLayer;
        private CCAction _headerLayerActionIn;
        private CCAction _headerLayerActionOut;

        // Footer pane/layer
        private UILayer _footerLayer;
        private CCAction _footerLayerActionIn;
        private CCAction _footerLayerActionOut;

        // Support for keeping hud on screen as hero moves
        private CCPoint _headerLayerOriginalPosition;
        private CCPoint _footerLayerOriginalPosition;
        private Vector3 _stationaryCameraOriginalPosition;
        private Vector3 _cocos2DCameraOriginalPosition;

        // Score label
        private CCLabelTTF _scoreLabel;
        private CCAction _scoreLabelAction;
        
        // Page label
        private CCLabelTTF _pageNumberLabel;
        private CCAction _pageNumberLabelAction;
        
        // Line label
        private CCLabelTTF _lineNumberLabel;
        private CCAction _lineNumberLabelAction;

        // Timer
        private CCLabelTTF _timerLabel;
        private DateTime _timerStartTime;
        private string _timerLabelText;
#if NETFX_CORE
        private ThreadPoolTimer _timer;
#else
        private Timer _timer;
#endif

        // Joystick
        private SneakyPanelControl _joystickPanel;

        // Pause/resume
        private bool _paused;
        private string _pauseText;
        private string _resumeText;

        // Free flight/resume
        private bool _inFlight;
        private CCLabelTTF _flightLabel;
        private CCMenu _flightMenu;
        
        private GameState _currentGameState;

        public HudLayer(CoreScene parent)
        {
            this._parent = parent;

            // Grab rerences to services we'll need
            this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
            this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));

            // Grab original Cocos2D DrawManager translation and stationary camera position
            // Will be used in positioning as game progresses
            this._cocos2DCameraOriginalPosition = Matrix.Invert(CCDrawManager.ViewMatrix).Translation;
            this._stationaryCameraOriginalPosition = this._inputManager.TheStationaryCamera.Position;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            var headerContentSize = new CCSize(0.4f * screenSize.Width,
                                               0.2f * screenSize.Height);

            // Header pane transition in/out
            var headerLayerEndPosition = new CCPoint(
                0.58f * screenSize.Width,
                0.78f * screenSize.Height);
            var headerLayerStartPosition = new CCPoint(
                headerLayerEndPosition.X, 
                screenSize.Height);

            // Header layer
            this._headerLayer = new UILayer();
            this._headerLayer.ContentSize = headerContentSize;
            this._headerLayer.Position = headerLayerEndPosition;
            this._headerLayerOriginalPosition = this._headerLayer.Position;
            this.AddChild(this._headerLayer);

            // Header layer transition in/out
            var headerLayerStartPlacementAction = new CCPlace(headerLayerStartPosition);
            var headerLayerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, headerLayerEndPosition);
            this._headerLayerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { headerLayerStartPlacementAction, headerLayerMoveInAction })
            );
            var headerLayerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, headerLayerStartPosition);
            this._headerLayerActionOut = new CCEaseBackIn(headerLayerMoveOutAction);

            // Score label
            this._scoreLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_EXTRA_LARGE);
            this._scoreLabel.Color = CCColor3B.Green;
            this._scoreLabel.Position = new CCPoint(
                0.5f * headerContentSize.Width, 
                0.8f * headerContentSize.Height);
            this._headerLayer.AddChild(this._scoreLabel);

            // Score label action
            var scaleStartScore = new CCScaleTo(0f, 0f);
            var scaleUpScore = new CCScaleTo(0.5f, 1.2f);
            var scaleBackScore = new CCScaleTo(0.1f, 1.0f);
            // Currently not using these - so score remains displayed on screen
            var delay = new CCDelayTime(2.0f);
            var fade = new CCFadeOut(0.5f);
            var hide = new CCHide();
            this._scoreLabelAction = new CCSequence(new CCFiniteTimeAction[] { scaleStartScore, scaleUpScore, scaleBackScore /*, delay, fade, hide*/ });

            // Page number label
            this._pageNumberLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._pageNumberLabel.Position = new CCPoint(
                0.25f * headerContentSize.Width,
                0.5f  * headerContentSize.Height);
            this._headerLayer.AddChild(this._pageNumberLabel);

            // Page number label action
            var scaleStartPageNumber = new CCScaleTo(0f, 0f);
            var scaleUpPageNumber = new CCScaleTo(0.5f, 1.2f);
            var scaleBackPageNumber = new CCScaleTo(0.1f, 1.0f);
            this._pageNumberLabelAction = new CCSequence(new CCFiniteTimeAction[] { scaleStartPageNumber, scaleUpPageNumber, scaleBackPageNumber });

            // Line number label
            this._lineNumberLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            _lineNumberLabel.Position = new CCPoint(
                0.7f * headerContentSize.Width,
                0.5f * headerContentSize.Height);
            this._headerLayer.AddChild(this._lineNumberLabel);

            // Line number label action
            var scaleStartLineNumber = new CCScaleTo(0f, 0f);
            var scaleUpLineNumber = new CCScaleTo(0.5f, 1.2f);
            var scaleBackLineNumber = new CCScaleTo(0.1f, 1.0f);
            this._lineNumberLabelAction = new CCSequence(new CCFiniteTimeAction[] { scaleStartLineNumber, scaleUpLineNumber, scaleBackLineNumber });

            // Timer
            this._timerLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._timerLabel.Position = new CCPoint(
                0.5f * headerContentSize.Width,
                0.2f * headerContentSize.Height);
            this._timerLabelText = string.Empty;
            this._headerLayer.AddChild(_timerLabel);
#if !NETFX_CORE
            this._timer = new Timer(TimerCallback);
#endif

            // Get this set up for relative positioning below
            var footerContentSize = new CCSize(
                0.96f * screenSize.Width,
                0.2f  * screenSize.Height);

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
            this._footerLayerOriginalPosition = this._footerLayer.Position;
            this.AddChild(this._footerLayer);

            // Footer layer transition in/out
            var footerLayerStartPlacementAction = new CCPlace(footerLayerStartPosition);
            var footerLayerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, footerLayerEndPosition);
            this._footerLayerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { footerLayerStartPlacementAction, footerLayerMoveInAction })
            );
            var footerLayerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, footerLayerStartPosition);
            this._footerLayerActionOut = new CCEaseBackIn(headerLayerMoveOutAction);

            // Joystick
            this._joystickPanel = new SneakyPanelControl(new CCSize(
                0.25f * footerContentSize.Width,
                footerContentSize.Height), 
                4);

            // TODO: Is this the right position for this? Try setting to middle of parent panel just to get to display
            this._joystickPanel.Position = new CCPoint(
                0.2f * footerContentSize.Width,
                0.5f * footerContentSize.Height);
            this._footerLayer.AddChild(this._joystickPanel);

            // Hook-up joystick events
            this._joystickPanel.JoyControl.SneakyStartEndEvent += this._inputManager.StickStartEndEvent;
            foreach (var button in this._joystickPanel.Buttons)
            {
                button.SneakyStartEndEvent += this._inputManager.ButtonStartEndEvent;
            }

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
            pauseToggle.Position = new CCPoint(
                0.5f * footerContentSize.Width,
                0.6f * footerContentSize.Height);
            this._footerLayer.AddChild(pauseToggle);
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
                0.5f  * footerContentSize.Width,
                0.25f * footerContentSize.Height);
            this._footerLayer.AddChild(pauseLabelMenu);

            // Speed
            var decreaseButtonNormal = new CCSprite("Images/Icons/DecreaseButtonNormal.png");
            var decreaseButtonSelected = new CCSprite("Images/Icons/DecreaseButtonSelected.png");
            var decreaseButton = new CCMenuItemImage((obj) => { this._characterCache.DecreaseVelocity(); });
            decreaseButton.NormalImage = decreaseButtonNormal;
            decreaseButton.SelectedImage = decreaseButtonSelected;

            var increaseButtonNormal = new CCSprite("Images/Icons/IncreaseButtonNormal.png");
            var increaseButtonSelected = new CCSprite("Images/Icons/IncreaseButtonSelected.png");
            var increaseButton = new CCMenuItemImage((obj) => { this._characterCache.IncreaseVelocity(); });
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
                0.8f * footerContentSize.Width,
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
                0.8f * footerContentSize.Width,
                0.25f * footerContentSize.Height);
            this._footerLayer.AddChild(speedLabel);

            this.ScheduleUpdate();

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
        }

        public override void OnExit()
        {
            base.OnExit();

            // Turn off timer display
#if NETFX_CORE
            this._timer.Cancel();
#else
            this._timer.Change(-1, -1);
#endif
        }

        public override void Update(float dt)
        {
            // Our Cocos2d draw manager is moving with hero, so determine where we are now
            // See ActionLayer.UpdateTrackingCamera for details.
            var drawManagerTranslationNew = Matrix.Invert(CCDrawManager.ViewMatrix).Translation;
            var offsetX = drawManagerTranslationNew.X - this._cocos2DCameraOriginalPosition.X;
            var offsetY = drawManagerTranslationNew.Y - this._cocos2DCameraOriginalPosition.Y;

            // Ok, now reposition Cocos2D ui so it stays in same place on screen
            this._headerLayer.Position = new CCPoint(
                 this._headerLayerOriginalPosition.X + offsetX,
                 this._headerLayerOriginalPosition.Y + offsetY);
            this._footerLayer.Position = new CCPoint(
                 this._footerLayerOriginalPosition.X + offsetX,
                 this._footerLayerOriginalPosition.Y + offsetY);

            this._timerLabel.Text = this._timerLabelText;
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

        public void DisplayPageNumber(int pageNumber)
        {
            // Animate display of new page number
            this._pageNumberLabel.StopAllActions();
            var pageText = string.Empty;
#if ANDROID
            pageText = Program.SharedProgram.Resources.GetString(Resource.String.HudPage);
#elif IOS
            pageText = NSBundle.MainBundle.LocalizedString(Strings.HudPage, Strings.HudPage);
#else
            pageText = AppResources.HudPage;
#endif
            this._pageNumberLabel.Text = pageText + " " + pageNumber.ToString();
            this._pageNumberLabel.RunAction(this._pageNumberLabelAction);
        }

        public void DisplayLineNumber(int lineNumber)
        {
            // Animate display of new line number
            this._lineNumberLabel.StopAllActions();
            var lineText = string.Empty;
#if ANDROID
            lineText = Program.SharedProgram.Resources.GetString(Resource.String.HudLine);
#elif IOS
            lineText = NSBundle.MainBundle.LocalizedString(Strings.HudLine, Strings.HudLine);
#else
            lineText = AppResources.HudLine;
#endif
            this._lineNumberLabel.Text = lineText + " " + lineNumber.ToString();
            this._lineNumberLabel.RunAction(this._lineNumberLabelAction);
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

        #endregion

        #region Helper methods

        private void PauseTogglePressed()
        {
            // TODO: Just call pause on charactercache?
            if (this._paused)
            {
                this._paused = false;
                this._parent.ResumeGame();
            }
            else
            {
                this._paused = true;
                this._parent.PauseGame();
            }
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
            var elapsed = DateTime.Now - this._timerStartTime;
            this._timerLabelText = elapsed.ToString(@"h\:mm\:ss");
        }

        #endregion
    }
}