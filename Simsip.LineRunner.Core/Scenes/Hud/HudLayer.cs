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
using Microsoft.Xna.Framework.Input.Touch;
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

        // Status label
        private CCLabelTTF _statusLabel;

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
        private TimeSpan _elapsedTime;
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

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                screenSize.Width,
                screenSize.Height);
            this.Position = new CCPoint(0, 0);
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
            this.AddChild(this._headerLayer);

            // Header layer transition in/out
            var headerLayerStartPlacementAction = new CCPlace(headerLayerStartPosition);
            var headerLayerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, headerLayerEndPosition);
            this._headerLayerActionIn = new CCEaseBackOut(
                new CCSequence(new CCFiniteTimeAction[] { headerLayerStartPlacementAction, headerLayerMoveInAction })
            );
            var headerLayerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, headerLayerStartPosition);
            this._headerLayerActionOut = new CCEaseBackIn(headerLayerMoveOutAction);

            // Status label
            this._statusLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._statusLabel.Color = CCColor3B.Red;
            this._statusLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(this._statusLabel);

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
            this._joystickPanel.JoyControl.SneakyStartEndEvent += this.HudStickStartEndEvent;
            foreach (var button in this._joystickPanel.Buttons)
            {
                button.SneakyStartEndEvent += this.HudButtonStartEndEvent;
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
            var pauseMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        pauseToggle,
                    });
            pauseMenu.Position = new CCPoint(
                0.5f * footerContentSize.Width,
                0.6f * footerContentSize.Height);
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
                0.5f  * footerContentSize.Width,
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

            // Enable support for gestures
            TouchPanel.EnabledGestures = GestureType.FreeDrag;
            CCApplication.SharedApplication.OnGesture += this.HudOnGesture;
        }

        public override void Update(float dt)
        {
            this._timerLabel.Text = this._timerLabelText;
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
            if (g.GestureType == GestureType.FreeDrag)
            {
                this._inputManager.HudOnGesture(g);
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

        private void PauseTogglePressed()
        {
            if (this._paused)
            {
                this._paused = false;
                this._characterCache.Pause(false);

                // Clear out any previous activity for status label
                // and clear out the label itself
                this._statusLabel.ActionManager.RemoveAllActionsFromTarget(this);
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
                this._statusLabel.Text = statusText;
            }
        }

        private void DecreaseVelocity()
        {
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