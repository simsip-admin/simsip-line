using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Data.OAuth;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.ParticleEffects;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Services.Facebook;
using Simsip.LineRunner.Services.OAuth;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
#if IOS
using Foundation;
#endif



namespace Simsip.LineRunner.Scenes.Finish
{
    public class FinishLayer : UILayer
    {
        CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // New top score
        private int _newTopScore;
        private CCLabelTTF _newTopScoreLabel;
        private CCLabelTTF _newTopTimeLabel;

        // Top 5 score/date
        private List<CCLabelTTF> _userTop5Score;
        private List<CCLabelTTF> _userTop5Date;

        // Particle effects
        private CustomContentManager _customerContentManager;
        private IParticleEffectCache _particleEffectCache;


        public FinishLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.6f * screenSize.Width,
                0.4f * screenSize.Height);

            // Layer transition in/out
            var layerEndPosition = new CCPoint(
                0.2f * screenSize.Width,
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
            var layerNavigateAction = new CCCallFunc(() => { this._parent.GoBack(); });
            this._layerActionOut = new CCEaseBackIn(
                new CCSequence(new CCFiniteTimeAction[] { layerMoveOutAction, layerNavigateAction })
            );

            // New top score header
            var newHighScoreText = string.Empty;
#if ANDROID
            newHighScoreText = Program.SharedProgram.Resources.GetString(Resource.String.FinishNewHighScore);
#elif IOS
            newHighScoreText = NSBundle.MainBundle.LocalizedString(Strings.FinishNewHighScore, Strings.FinishNewHighScore);
#else
            newHighScoreText = AppResources.FinishNewHighScore;
#endif
            var newHighScoreHeader = new CCLabelTTF(newHighScoreText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            newHighScoreHeader.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(newHighScoreHeader);

            // New top score
            this._newTopScoreLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_X_LARGE);
            this._newTopScoreLabel.Color = CCColor3B.Green;
            this._newTopScoreLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.6f * this.ContentSize.Height);
            this.AddChild(this._newTopScoreLabel);

            // New top time
            this._newTopTimeLabel = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            this._newTopTimeLabel.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.4f * this.ContentSize.Height);
            this.AddChild(this._newTopTimeLabel);

            /* TODO: Add back in when first version of finish layer is stablized
            // Top 5 scores
            var topScoresForText = string.Empty;
#if ANDROID
            topScoresForText = Program.SharedProgram.Resources.GetString(Resource.String.FinishTopScoresFor);
#elif IOS
            topScoresForText = NSBundle.MainBundle.LocalizedString(Strings.FinishTopScoresFor, Strings.FinishTopScoresFor);
#else
            topScoresForText = AppResources.FinishTopScoresFor;
#endif
            var userScoresHeader = new CCLabelTTF(topScoresForText + " " + "", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            userScoresHeader.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.5f * this.ContentSize.Height);
            this.AddChild(userScoresHeader);

            this._userTop5Score = new List<CCLabelTTF>();
            for (int i = 0; i < 5; i++)
            {
                var score = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
                this._userTop5Score.Add(score);
                score.Position = new CCPoint(
                    0.2f * this.ContentSize.Width, 
                    userScoresHeader.Position.Y - (0.1f*this.ContentSize.Height * (i + 1)));
            }
            this._userTop5Date = new List<CCLabelTTF>();
            for (int i = 0; i < 5; i++)
            {
                var date = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
                this._userTop5Date.Add(date);
                date.Position = new CCPoint(
                    0.6f * this.ContentSize.Width, 
                    userScoresHeader.Position.Y - (0.1f*this.ContentSize.Height * (i + 1)));
            }
            */

            // Back
            CCMenuItemImage backButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => 
                                    {
                                        this.GoBack();
                                    });
            var backMenu = new CCMenu(
                new CCMenuItem[] 
                    {
                        backButton, 
                    });
            backMenu.Position = new CCPoint(
                0.5f  * this.ContentSize.Width, 
                0.15f * this.ContentSize.Height);
            this.AddChild(backMenu);

            // Particle effects
            this._customerContentManager = new CustomContentManager(
                TheGame.SharedGame.Services,
                TheGame.SharedGame.Content.RootDirectory);
                /* Debug
                "Finish");
                */
            this._particleEffectCache = (IParticleEffectCache)TheGame.SharedGame.Services.GetService(typeof(IParticleEffectCache));
        }

        #region Cocos2D overrides

        public async override void OnEnter()
        {
            base.OnEnter();

            // TODO: Get top fb scores
            // await GetTopScoresAll();

            // IMPORTANT: We'll use this function to update our game count as FinishLayer is a singleton
            // and hence the constructor might only be called once for several games
            var previousGameCount = 
                UserDefaults.SharedUserDefault.GetIntegerForKey(GameConstants.USER_DEFAULT_KEY_GAMES_PLAYED_COUNT, 0);
            var newGameCount = previousGameCount + 1;
            UserDefaults.SharedUserDefault.SetIntegerForKey(GameConstants.USER_DEFAULT_KEY_GAMES_PLAYED_COUNT, newGameCount);

            // Animate layer
            this.RunAction(this._layerActionIn);

            // Handles all logic to display score with optional post if 
            // we have a new highest score
            this.DisplayScore();

            // TODO: Add in after first version of finish layer is stabilized
            // For reference, show them the current top 5 scores
            // this.DisplayTopScores();
        }

        public override void OnExit()
        {
            base.OnExit();

            this._particleEffectCache.TerminateAllFinishEffects();
            this._customerContentManager.Unload();
        }
        #endregion

        #region Api

        public void SetNewTopScore(int newTopScore)
        {
            this._newTopScore = newTopScore;
        }

        #endregion

        #region Helper methods

        private void GoBack()
        {
            // Clean-up navigation stack and then go back to start
            this._parent.GoBack();
        }

        // Handles all logic to display score with optional post if 
        // we have a new highest score
        private void DisplayScore()
        {
            // First, update our new high score locally
            var scoreRepository = new FacebookScoreRepository();
            var newTopTimeSpan = this._parent.TheHudLayer.GetTime();
            var score = new FacebookScoreEntity
            {
                Score = this._newTopScore,
                ScoreTime = newTopTimeSpan,
                CreateDate = DateTime.Now
            };
            scoreRepository.Create(score);

            this._newTopScoreLabel.Text = this._newTopScore.ToString();

            this._newTopTimeLabel.Text = newTopTimeSpan.ToString(@"h\:mm\:ss");

            var finishParticleDescs = ParticleEffectFactory.CreateFinishParticles(GetParticleEffectScreenPoint);
            int i = 0;
            foreach (var finishParticleDesc in finishParticleDescs)
            {
                finishParticleDesc.ParticleEffectIndex = i++;
            }
            this._particleEffectCache.AddFinishParticleEffect(finishParticleDescs, this._customerContentManager);

            /* TODO: Add back in when facebook support is in place
            var postText = string.Empty;
#if ANDROID
            postText = Program.SharedProgram.Resources.GetString(Resource.String.FinishPost);
#elif IOS
            postText = NSBundle.MainBundle.LocalizedString(Strings.FinishPost, Strings.FinishPost);
#else
            postText = AppResources.FinishPost;
#endif
            var postLabel = new CCLabelTTF(postText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            var postButton = new CCMenuItemLabel(postLabel,
                                                (obj) => 
                                                { 
                                                    this.CreateScore();
                                                    this._parent.GoBack();
                                                });
            
            CCMenu postMenu = new CCMenu(
                new CCMenuItem[] 
                {
                    postButton
                });
            postMenu.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.6f * this.ContentSize.Height);
            this.AddChild(postMenu);
            */
        }

        private CCPoint GetParticleEffectScreenPoint(ParticleEffectDesc particleEffectDesc)
        {
            var returnPoint = CCPoint.Zero;

            switch (particleEffectDesc.ParticleEffectIndex)
            {
                case 0:
                    {
                        returnPoint = this.ConvertToWorldSpace(new CCPoint(
                            0.2f * this.ContentSize.Width, 
                            0.2f * this.ContentSize.Height));
                        break;
                    }
                case 1:
                    {
                        returnPoint = this.ConvertToWorldSpace(new CCPoint(
                            0.4f * this.ContentSize.Width, 
                            0.2f * this.ContentSize.Height));
                        break;
                    }
                case 2:
                    {
                        returnPoint = this.ConvertToWorldSpace(new CCPoint(
                            0.8f * this.ContentSize.Width, 
                            0.2f * this.ContentSize.Height));
                        break;
                    }
            }

            return returnPoint;
        }

        private async Task CreateScore()
        {

            // Determine if we need to navigate to login screen first
            var oauthResultRepository = new OAuthResultRepository();
            var facebookOAuthResult = oauthResultRepository.Read(OAuthType.Facebook);
            if (facebookOAuthResult != null &&
                !string.IsNullOrEmpty(facebookOAuthResult.AccessToken))
            {
                //
                // We can create right away
                //

                // Then follow up by creating remote record
                var score = new FacebookScoreEntity
                    {
                        Score = this._newTopScore
                    };
                var scoreService = new FacebookScoreService();
                var success = await scoreService.AddScoreAsync(score);

                return;
            }

#if ANDROID
            // Ok, we need to login first
            Program.SharedProgram.LaunchOAuth();
#elif IOS
            // Ok, we need to login first
            Program.SharedProgram.LaunchOAuth();
#elif WINDOWS_PHONE
            // Ok, we need to login first
            Program.SharedProgram.LaunchOAuth();
#elif NETFX_CORE
            // Ok, we need to login first
            var app = App.Current as App;
            app.LaunchOAuth();
#endif
        }

        private void DisplayTopScores()
        {
            try
            {
                // Get local top 5 (or up to top 5)
                var scoreRepository = new FacebookScoreRepository();
                var topScores = scoreRepository.GetTopScoresForPlayer(5);
                var topScoresCount = (topScores.Count > 5) ? 5 : topScores.Count;
                
                // Update display
                for (int i = 0; i < topScoresCount; i++)
                {
                    this._userTop5Score[i].Text = topScores[i].Score.ToString();
                    // TODO
                    // this._userTop5Date[i].Text = topScores[i].CreatedDate.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in DisplayTopScores: " + ex);
            }
        }

        #endregion

    }
}