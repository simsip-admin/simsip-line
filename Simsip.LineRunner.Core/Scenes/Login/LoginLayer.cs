using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Entities.Scoreoid;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Services.Scoreoid;
using Simsip.LineRunner.Utils;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Login
{
    public class LoginLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // Username
        private CCTextFieldTTF _usernameTextField;
        private KeyboardNotificationLayer _usernameNotificationLayer;

        // Password
        private CCTextFieldTTF _passwordTextField;
        private KeyboardNotificationLayer _passwordNotificationLayer;

        public LoginLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.VisibleSize;
            this.ContentSize = new CCSize(
                0.6f * screenSize.Width,
                0.6f * screenSize.Height);

            // Layer transition in/out
            var layerEndPosition = CCDirector.SharedDirector.VisibleOrigin + new CCPoint(
                0.2f * screenSize.Width,
                0.2f * screenSize.Height);
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

            // Username
            var userNameText = string.Empty;
#if ANDROID
            userNameText = Program.SharedProgram.Resources.GetString(Resource.String.LoginUserName);
#elif IOS
            userNameText = NSBundle.MainBundle.LocalizedString(Strings.LoginUserName, Strings.LoginUserName);
#else
            userNameText = AppResources.LoginUserName;
#endif
            var usernameLabel = new CCLabelTTF(userNameText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            usernameLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            usernameLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.8f * this.ContentSize.Height);
            this.AddChild(usernameLabel);
            this._usernameTextField = new CCTextFieldTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._usernameTextField.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            this._usernameTextField.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.8f * this.ContentSize.Height);
            this._usernameTextField.AutoEdit = true;
            this._usernameTextField.EditTitle = userNameText;
            var userNameDescriptionText = string.Empty;
#if ANDROID
            userNameDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.LoginUserNameDescription);
#elif IOS
            userNameDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.LoginUserNameDescription, Strings.LoginUserNameDescription);
#else
            userNameDescriptionText = AppResources.LoginUserNameDescription;
#endif
            this._usernameTextField.EditDescription = userNameDescriptionText;
            this._usernameNotificationLayer = new KeyboardNotificationLayer(this._usernameTextField);
            this.AddChild(this._usernameNotificationLayer);

            // Password
            var passwordText = string.Empty;
#if ANDROID
            passwordText = Program.SharedProgram.Resources.GetString(Resource.String.LoginPassword);
#elif IOS
            passwordText = NSBundle.MainBundle.LocalizedString(Strings.LoginPassword, Strings.LoginPassword);
#else
            passwordText = AppResources.LoginPassword;
#endif
            var passwordLabel = new CCLabelTTF(passwordText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            passwordLabel.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            passwordLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.6f * this.ContentSize.Height);
            this.AddChild(passwordLabel);
            this._passwordTextField = new CCTextFieldTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._passwordTextField.Scale = GameConstants.FONT_SIZE_NORMAL_SCALE;
            this._passwordTextField.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.6f * this.ContentSize.Height);
            this._passwordTextField.AutoEdit = true;
            this._passwordTextField.EditTitle = passwordText;
            var passwordDescriptionText = string.Empty;
#if ANDROID
            passwordDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.LoginPasswordDescription);
#elif IOS
            passwordDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.LoginPasswordDescription, Strings.LoginPasswordDescription);
#else
            passwordDescriptionText = AppResources.LoginPasswordDescription;
#endif
            this._passwordTextField.EditDescription = passwordDescriptionText;
            this._passwordNotificationLayer = new KeyboardNotificationLayer(this._passwordTextField);
            this.AddChild(this._passwordNotificationLayer);

            // Done
            CCMenuItemImage doneButton =
                new CCMenuItemImage("Images/Icons/DoneButtonNormal.png",
                                    "Images/Icons/DoneButtonSelected.png",
                        (obj) =>
                        {
                            this.CreateUser();
                            this._parent.GoBack();
                        });
            var doneMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        doneButton, 
                    });
            doneMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.4f * this.ContentSize.Height);
            this.AddChild(doneMenu);

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
                0.2f * this.ContentSize.Height);
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
            backLabel.Scale = GameConstants.FONT_SIZE_SMALL_SCALE;
            backLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.02f * this.ContentSize.Height);
            this.AddChild(backLabel);
        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Animate layer
            this.RunAction(this._layerActionIn);
        }

        #endregion

        #region Helper methods

        private async void CreateUser()
        {
            var playerEntity = new PlayerEntity()
            {
                Username = this._usernameTextField.Text,
                Password = this._passwordTextField.Text
            };
            var playerService = new ScoreoidPlayerService();
            await playerService.CreatePlayer(playerEntity);

            if (GameManager.SharedGameManager.CurrentScore != 0)
            {
                var scoreEntity = new ScoreEntity()
                {
                    Username = playerEntity.Username,
                    Score = GameManager.SharedGameManager.CurrentScore
                };
                var scoreService = new ScoreoidScoreService();
                await scoreService.CreateScore(scoreEntity);
            }
        }

        #endregion
    }
}