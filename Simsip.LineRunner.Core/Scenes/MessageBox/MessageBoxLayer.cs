using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
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

    public class MessageBoxLayer : UILayer
    {
        private CoreScene _parent;

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
        private CCSprite _progressSpinner;

        // Tags to help in clean-up cycles
        private int _staticTag = 0;
        private int _dynamicTag = 1;

        // Callback
        private System.Action<MessageBoxResponse> _action;

        public MessageBoxLayer(CoreScene parent)
        {
            this._parent = parent;

            // Title
            this._title = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            this._title.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
            this._title.Tag = this._staticTag;
            this.AddChild(this._title);

            // Description
            this._description = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            this._description.Scale = GameConstants.FONT_SIZE_LARGE_SCALE;
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
            okLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
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
            cancelLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
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
            yesLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
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
            noLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            var noItem = new CCMenuItemLabel(noLabel,
                (obj) => { this.OnNo(); });
            this._noButton = new CCMenu(
               new CCMenuItem[] 
                    {
                        noItem
                    });
            this._noButton.Tag = this._dynamicTag;

            // Progress
            this._progressSpinner = new CCSprite("Images/Misc/ChatSpinner.png");
            this._progressSpinner.Tag = this._dynamicTag;

            // Selections
            this._selectionButtons = new List<CCMenu>();
        }


        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate layer
            if (this._layerActionIn != null)
            {
                this.RunAction(this._layerActionIn);
            }
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

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;

            // Clean-up
            this.RemoveAllChildrenByTag(this._dynamicTag, false);

            // MessageBox construction
            switch(type)
            {
                case MessageBoxType.MB_OK:
                    {
                        // Default background
                        this.Color = DEFAULT_COLOR;
                        this.Opacity = DEFAULT_OPACITY;

                        // Size screen accordingly
                        this.ContentSize = new CCSize(
                            0.4f * screenSize.Width,
                            0.4f * screenSize.Height);

                        // Layer transition in/out
                        var layerEndPosition = new CCPoint(
                                0.3f * screenSize.Width,
                                0.3f * screenSize.Height);
                        var layerStartPosition = new CCPoint(
                            layerEndPosition.X,
                            screenSize.Height);
                        var layerStartPlacementAction = new CCPlace(layerStartPosition);
                        var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
                        this._layerActionIn = new CCEaseBackOut(
                            new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction })
                        );
                        var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
                        var navigateAction = new CCCallFunc(() => { this._parent.GoBack(); });
                        this._layerActionOut = new CCEaseBackIn(
                            new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, navigateAction })
                        );

                        // Position title/description
                        this._title.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.9f * this.ContentSize.Height);
                        this._description.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.8f * this.ContentSize.Height);

                        // Create body
                        this.AddChild(this._okButton);
                        this._okButton.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.5f * this.ContentSize.Height);

                        break;
                    }
                case MessageBoxType.MB_OK_CANCEL:
                    {
                        // Default background
                        this.Color = DEFAULT_COLOR;
                        this.Opacity = DEFAULT_OPACITY;

                        // Size screen accordingly
                        this.ContentSize = new CCSize(
                            0.4f * screenSize.Width,
                            0.4f * screenSize.Height);

                        // Layer transition in/out
                        var layerEndPosition = new CCPoint(
                                0.3f * screenSize.Width,
                                0.3f * screenSize.Height);
                        var layerStartPosition = new CCPoint(
                            layerEndPosition.X,
                            screenSize.Height);
                        var layerStartPlacementAction = new CCPlace(layerStartPosition);
                        var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
                        this._layerActionIn = new CCEaseBackOut(
                            new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction })
                        );
                        var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
                        var navigateAction = new CCCallFunc(() => { this._parent.GoBack(); });
                        this._layerActionOut = new CCEaseBackIn(
                            new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, navigateAction })
                        );

                        // Position title/description
                        this._title.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.9f * this.ContentSize.Height);
                        this._description.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.8f * this.ContentSize.Height);

                        // Create body
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
                        // Default background
                        this.Color = DEFAULT_COLOR;
                        this.Opacity = DEFAULT_OPACITY;

                        // Size screen accordingly
                        this.ContentSize = new CCSize(
                            0.4f * screenSize.Width,
                            0.4f * screenSize.Height);

                        // Layer transition in/out
                        var layerEndPosition = new CCPoint(
                                0.3f * screenSize.Width,
                                0.3f * screenSize.Height);
                        var layerStartPosition = new CCPoint(
                            layerEndPosition.X,
                            screenSize.Height);
                        var layerStartPlacementAction = new CCPlace(layerStartPosition);
                        var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
                        this._layerActionIn = new CCEaseBackOut(
                            new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction })
                        );
                        var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
                        var navigateAction = new CCCallFunc(() => { this._parent.GoBack(); });
                        this._layerActionOut = new CCEaseBackIn(
                            new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, navigateAction })
                        );

                        // Position title/description
                        this._title.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.9f * this.ContentSize.Height);
                        this._description.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.8f * this.ContentSize.Height);

                        // Create body
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

                        // Are we just showing the progress spinner?
                        if (title == string.Empty &&
                            description == string.Empty)
                        {
                            // Ok, just progress spinner:
                            // 1. No shaded background
                            // 2. Full screen size to center the progress indicator within
                            // 3. No animation in/out
                            // IMPORTANT: This means you will be responsible for removing messageboxlayer

                            this.Color = DEFAULT_COLOR;
                            this.Opacity = 0;

                            this.ContentSize = new CCSize(
                                1.0f * screenSize.Width,
                                1.0f * screenSize.Height);

                            this._layerActionIn = null;
                            this._layerActionOut = null;
                        }
                        else
                        {
                            // Ok, progress spinner with text:
                            // 1. Default background
                            // 2. Adjusted screen size to center the progress indicator within
                            // 3. Standard animation in/out

                            this.Color = DEFAULT_COLOR;
                            this.Opacity = DEFAULT_OPACITY;

                            this.ContentSize = new CCSize(
                                0.8f * screenSize.Width,
                                0.3f * screenSize.Height);

                            var layerEndPosition = new CCPoint(
                                    0.1f  * screenSize.Width,
                                    0.35f * screenSize.Height);
                            var layerStartPosition = new CCPoint(
                                layerEndPosition.X,
                                screenSize.Height);
                            var layerStartPlacementAction = new CCPlace(layerStartPosition);
                            var layerMoveInAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerEndPosition);
                            this._layerActionIn = new CCEaseBackOut(
                                new CCSequence(new CCFiniteTimeAction[] { layerStartPlacementAction, layerMoveInAction })
                            );
                            var layerMoveOutAction = new CCMoveTo(GameConstants.DURATION_LAYER_TRANSITION, layerStartPosition);
                            var navigateAction = new CCCallFunc(() => { this._parent.GoBack(); });
                            this._layerActionOut = new CCEaseBackIn(
                                new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, navigateAction })
                            );
                        }

                        // Position title/description
                        this._title.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.8f * this.ContentSize.Height);
                        this._description.Position = new CCPoint(
                            0.5f * this.ContentSize.Width,
                            0.6f * this.ContentSize.Height);

                        // Create body
                        if (description == string.Empty)
                        {
                            this._progressSpinner.Position = new CCPoint(
                                0.5f * this.ContentSize.Width,
                                0.5f * this.ContentSize.Height);
                        }
                        else
                        {
                            this._progressSpinner.Position = new CCPoint(
                                0.5f * this.ContentSize.Width,
                                0.3f * this.ContentSize.Height);
                        }
                        var progressAnimation = new CCRepeatForever(new CCRotateBy(2, 360));
                        this.AddChild(this._progressSpinner);
                        this._progressSpinner.RunAction(progressAnimation);

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

            // Title/description
            this._title.Text = title;
            this._description.Text = description;

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
            // IMPORTANT: There is a CCCallFunc at end that will navigate us back using GoBack()
            if (this._layerActionOut != null)
            {
                this.RunAction(this._layerActionOut);
            }
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