using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.MessageBox
{
    public enum MessageBoxType
    {
        MB_OK,
        MB_OK_CANCEL,
        MB_YES_NO,
        MB_SELECTION,
        MB_PROGRESS,
        MB_PROGRESS_OK
    }

    public enum MessageBoxResponseType
    {
        MB_OK,
        MB_CANCEL,
        MB_YES,
        MB_NO
    }

    public class MessageBoxResponse
    {
        public MessageBoxResponseType ResponseType;
        public string ResponseValue;
    }

    public class MessageBoxLayer : GameLayer
    {
        private CoreScene _parent;

        // Services we'll need
        private IPaneCache _paneCache;

        // Pane and pane actions
        private PaneModel _paneModel;
        private Simsip.LineRunner.Actions.Action _paneActionIn;
        private Simsip.LineRunner.Actions.Action _paneActionOut;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Title/description
        private CCLabelTTF _title;
        private CCLabelTTF _description;

        // Buttons
        private CCMenu _okButton;
        private CCMenu _cancelButton;
        private CCMenu _yesButton;
        private CCMenu _noButton;
        private IList<CCMenu> _selectionButtons;

        // Progress
        private CCSprite _progressBackground;
        private CCProgressTimer _progressTimer;

        // Tags to help in clean-up cycles
        private int _staticTag;
        private int _dynamicTag;

        // Callback
        private System.Action<MessageBoxResponse> _action;


        public MessageBoxLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get services we'll need
            this._paneCache = (IPaneCache)TheGame.SharedGame.Services.GetService(typeof(IPaneCache));

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(0.6f * screenSize.Width,
                                          0.6f * screenSize.Height);
            // Pane model
            var paneLogicalOrigin = new CCPoint(
                    0.2f * screenSize.Width, 
                    0.2f * screenSize.Height);
            var paneModelArgs = new PaneModelArgs()
            {
                ThePaneType = PaneType.Simple,
                LogicalOrigin = paneLogicalOrigin,
                LogicalWidth = this.ContentSize.Width,
                LogicalHeight = this.ContentSize.Height,
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
            var layerNavigateAction = new CCCallFunc(() => {
                this._paneCache.RemovePaneModel(this._paneModel);
                this._parent.GoBack();
            });
            this._layerActionOut = new CCEaseBackIn(
                new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, layerNavigateAction })
            );

            // Title
            this._title = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            this._title.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this._title.Tag = this._staticTag;
            this.AddChild(this._title);


            // Description
            this._description = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._description.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this._description.Tag = this._staticTag;
            this.AddChild(this._description);

            // Ok
            var okText = string.Empty;
#if ANDROID
            okText = Program.SharedProgram.Resources.GetString(Resource.String.CommonOK);
#elif IOS
            okText = NSBundle.MainBundle.LocalizedString(Strings.CommonOK, Strings.CommonOK);
#else
            okText = AppResources.CommonOK;
#endif
            var okLabel = new CCLabelTTF(okText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var okItem = new CCMenuItemLabel(okLabel,
                (obj) => { this.OnOK(); });
            this._okButton = new CCMenu(
               new CCMenuItem[] 
                    {
                        okItem
                    });
            this._okButton.Tag = this._dynamicTag;

            // Cancel
            var cancelText = string.Empty;
#if ANDROID
            cancelText = Program.SharedProgram.Resources.GetString(Resource.String.CommonCancel);
#elif IOS
            cancelText = NSBundle.MainBundle.LocalizedString(Strings.CommonCancel, Strings.CommonCancel);
#else
            cancelText = AppResources.CommonCancel;
#endif
            var cancelLabel = new CCLabelTTF(cancelText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var cancelItem = new CCMenuItemLabel(cancelLabel,
                (obj) => { this.OnCancel(); });
            this._cancelButton = new CCMenu(
               new CCMenuItem[] 
                    {
                        cancelItem
                    });
            this._cancelButton.Tag = this._dynamicTag;

            // Yes
            var yesText = string.Empty;
#if ANDROID
            yesText = Program.SharedProgram.Resources.GetString(Resource.String.CommonYes);
#elif IOS
            yesText = NSBundle.MainBundle.LocalizedString(Strings.CommonYes, Strings.CommonYes);
#else
            yesText = AppResources.CommonYes;
#endif
            var yesLabel = new CCLabelTTF(yesText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var yesItem = new CCMenuItemLabel(yesLabel,
                (obj) => { this.OnYes(); });
            this._yesButton = new CCMenu(
               new CCMenuItem[] 
                    {
                        yesItem
                    });
            this._yesButton.Tag = this._dynamicTag;

            // No
            var noText = string.Empty;
#if ANDROID
            noText = Program.SharedProgram.Resources.GetString(Resource.String.CommonNo);
#elif IOS
            noText = NSBundle.MainBundle.LocalizedString(Strings.CommonNo, Strings.CommonNo);
#else
            noText = AppResources.CommonNo;
#endif
            var noLabel = new CCLabelTTF(noText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var noItem = new CCMenuItemLabel(noLabel,
                (obj) => { this.OnNo(); });
            this._noButton = new CCMenu(
               new CCMenuItem[] 
                    {
                        noItem
                    });
            this._noButton.Tag = this._dynamicTag;

            // Progress
            this._progressBackground = new CCSprite("Images/Misc/ProgressBackground.png");
            this._progressBackground.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.5f * this.ContentSize.Height);
            this._progressBackground.Tag = this._dynamicTag;
            var progressHighlight = new CCSprite("Images/Misc/ProgressHighlight.png");
            this._progressTimer = new CCProgressTimer(progressHighlight);
            this._progressTimer.Type = CCProgressTimerType.Bar;
            this._progressTimer.Midpoint = new CCPoint(0.0f, 0.5f);  // Starts from left
            this._progressTimer.BarChangeRate = new CCPoint(1.0f, 0.0f); // Grow only in the "x" horizontal direction
            this._progressTimer.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.5f * this.ContentSize.Height); // Overlap the progress background added above
            var progressAnimation = new CCRepeatForever(new CCProgressFromTo(4, 0, 100));
            this._progressTimer.RunAction(progressAnimation);
            this._progressTimer.Tag = this._dynamicTag;

            // Selections
            this._selectionButtons = new List<CCMenu>();
        }


        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Register pane for drawing
            this._paneCache.AddPaneModel(this._paneModel);

            // Animate pane/layer
            this._paneModel.ModelRunAction(this._paneActionIn);
            this.RunAction(this._layerActionIn);
        }

    #endregion
    
        #region Api

        public void Show(string title,
                         string description,
                         MessageBoxType type,
                         System.Action<MessageBoxResponse> action = null,
                         IList<string> selections = null)
        {
            // Callback
            this._action = action;

            // Title/description
            this._title.Text = title;
            this._description.Text = description;

            // Clean-up
            this.RemoveAllChildrenByTag(this._dynamicTag, false);

            // MessageBox construction
            switch(type)
            {
                case MessageBoxType.MB_OK:
                    {
                        // Create
                        this.AddChild(this._okButton);
                        this._okButton.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.5f * this.ContentSize.Height);
                       

                        break;
                    }
                case MessageBoxType.MB_OK_CANCEL:
                    {
                        // Create
                        this.AddChild(this._okButton);
                        this.AddChild(this._cancelButton);
                        this._okButton.Position = new CCPoint(
                            0.25f * this.ContentSize.Width,
                            0.5f * this.ContentSize.Height);
                        this._cancelButton.Position = new CCPoint(
                            0.75f * this.ContentSize.Width,
                            0.5f * this.ContentSize.Height);

                        break;
                    }
                case MessageBoxType.MB_YES_NO:
                    {
                        // Create
                        this.AddChild(this._yesButton);
                        this.AddChild(this._noButton);
                        this._yesButton.Position = new CCPoint(
                            0.25f * this.ContentSize.Width,
                            0.5f * this.ContentSize.Height);
                        this._noButton.Position = new CCPoint(
                            0.75f * this.ContentSize.Width,
                            0.5f * this.ContentSize.Height);

                        break;
                    }

                case MessageBoxType.MB_SELECTION:
                    {
                        throw new NotSupportedException("Unknown message box type in MessageBox.Show");
                    }

                case MessageBoxType.MB_PROGRESS:
                    {
                        // Create
                        this.AddChild(this._progressBackground);
                        this.AddChild(this._progressTimer);
                        break;
                    }

                case MessageBoxType.MB_PROGRESS_OK:
                    {
                        throw new NotSupportedException("Unknown message box type in MessageBox.Show");
                    }
                default:
                    {
                        throw new NotSupportedException("Unknown message box type in MessageBox.Show");
                    }
            }

            // Navigate
            this._parent.Navigate(LayerTags.MessageBoxLayer);
        }

        public void Hide()
        {
            // Navigate back
            this.GoBack();
        }

        #endregion

        #region Helper methods

        private void GoBack()
        {
            // Will animate our exit
            // IMPORTANT: There is a CCCallFunc at end that will deregister pane model from cache
            // and navigate us back using GoBack()
            this._paneModel.ModelRunAction(this._paneActionOut);
            this.RunAction(this._layerActionOut);

        }

        private void OnOK()
        {
            // Callback
            if (this._action != null)
            {
                var response = new MessageBoxResponse()
                    {
                        ResponseType = MessageBoxResponseType.MB_OK,
                        ResponseValue = string.Empty
                    };
                this._action(response);
            }

            // Navigate back
            this.GoBack();
        }

        private void OnCancel()
        {
            // Callback
            if (this._action != null)
            {
                var response = new MessageBoxResponse()
                {
                    ResponseType = MessageBoxResponseType.MB_CANCEL,
                    ResponseValue = string.Empty
                };
                this._action(response);
            }

            // Navigate back
            this.GoBack();

        }

        private void OnYes()
        {
            // Callback
            if (this._action != null)
            {
                var response = new MessageBoxResponse()
                {
                    ResponseType = MessageBoxResponseType.MB_YES,
                    ResponseValue = string.Empty
                };
                this._action(response);
            }

            // Navigate back
            this.GoBack();
        }

        private void OnNo()
        {
            // Callback
            if (this._action != null)
            {
                var response = new MessageBoxResponse()
                {
                    ResponseType = MessageBoxResponseType.MB_NO,
                    ResponseValue = string.Empty
                };
                this._action(response);
            }

            // Navigate back
            this.GoBack();
        }

        private void OnSelected(string selection)
        {

        }

        private void OnProgressFinished()
        {

        }

        #endregion
    }
}