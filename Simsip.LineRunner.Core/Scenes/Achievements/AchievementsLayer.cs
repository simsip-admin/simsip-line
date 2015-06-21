using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Services.Facebook;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Simsip.LineRunner.Data.OAuth;
using Simsip.LineRunner.Entities.OAuth;

#if IOS
using Foundation;
#endif



namespace Simsip.LineRunner.Scenes.Achievements
{
    public class AchievementsLayer : UILayer
    {
        private CoreScene _parent;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // User scores
        private List<CCLabelTTF> _userScoreScore;
        private List<CCLabelTTF> _userScoreTime;
        private List<CCLabelTTF> _userScoreDate;

        // Global scores
        private List<CCLabelTTF> _globalScoreUsername;
        private List<CCLabelTTF> _globalScoreScore;
        private List<CCLabelTTF> _globalScoreDate;

        // Average score
        private CCLabelTTF _averageScoreLabel;

        public AchievementsLayer(CoreScene parent)
        {
            this._parent = parent;

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.8f * screenSize.Width,
                0.6f * screenSize.Height);

            // Layer transition in/out
            var layerEndPosition = new CCPoint(
                0.1f * screenSize.Width,
                0.2f  * screenSize.Height);
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

            // Leaderboards title
            var leaderboardsText = string.Empty;
#if ANDROID
            leaderboardsText = Program.SharedProgram.Resources.GetString(Resource.String.AchievementsLeaderboards);
#elif IOS
            leaderboardsText = NSBundle.MainBundle.LocalizedString(Strings.AchievementsLeaderboards, Strings.AchievementsLeaderboards);
#else
            leaderboardsText = AppResources.AchievementsLeaderboards;
#endif

            var leaderboardTitle = new CCLabelTTF(leaderboardsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_X_LARGE);
            leaderboardTitle.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.9f * this.ContentSize.Height);
            this.AddChild(leaderboardTitle);

            // Top user scores
            var topScoresForText = string.Empty;
#if ANDROID
            topScoresForText = Program.SharedProgram.Resources.GetString(Resource.String.AchievementsTopScoresFor);
#elif IOS
            topScoresForText = NSBundle.MainBundle.LocalizedString(Strings.AchievementsTopScoresFor, Strings.AchievementsTopScoresFor);
#else
            topScoresForText = AppResources.AchievementsTopScoresFor;
#endif
            var userScoresHeader = new CCLabelTTF(topScoresForText + " " + "", GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            userScoresHeader.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.8f * this.ContentSize.Height);
            this.AddChild(userScoresHeader);

            // Column headers
            var scoreColumnHeaderText = string.Empty;
#if ANDROID
            scoreColumnHeaderText = Program.SharedProgram.Resources.GetString(Resource.String.AchievementsColumnScore);
#elif IOS
            scoreColumnHeaderText = NSBundle.MainBundle.LocalizedString(Strings.AchievementsColumnScore, Strings.AchievementsColumnScore);
#else
            scoreColumnHeaderText = AppResources.AchievementsColumnScore;
#endif
            var scoreColumnHeader = new CCLabelTTF(scoreColumnHeaderText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            scoreColumnHeader.AnchorPoint = CCPoint.AnchorMiddleRight;
            scoreColumnHeader.Position = new CCPoint(
                0.25f * this.ContentSize.Width,
                0.7f  * this.ContentSize.Height);
            this.AddChild(scoreColumnHeader);
            var timeColumnHeaderText = string.Empty;
#if ANDROID
            timeColumnHeaderText = Program.SharedProgram.Resources.GetString(Resource.String.AchievementsColumnTime);
#elif IOS
            timeColumnHeaderText = NSBundle.MainBundle.LocalizedString(Strings.AchievementsColumnTime, Strings.AchievementsColumnTime);
#else
            timeColumnHeaderText = AppResources.AchievementsColumnTime;
#endif
            var timeColumnHeader = new CCLabelTTF(timeColumnHeaderText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            timeColumnHeader.AnchorPoint = CCPoint.AnchorMiddleLeft;
            timeColumnHeader.Position = new CCPoint(
                0.35f * this.ContentSize.Width,
                0.7f  * this.ContentSize.Height);
            this.AddChild(timeColumnHeader);
            var dateColumnHeaderText = string.Empty;
#if ANDROID
            dateColumnHeaderText = Program.SharedProgram.Resources.GetString(Resource.String.AchievementsColumnDate);
#elif IOS
            dateColumnHeaderText = NSBundle.MainBundle.LocalizedString(Strings.AchievementsColumnDate, Strings.AchievementsColumnDate);
#else
            dateColumnHeaderText = AppResources.AchievementsColumnDate;
#endif
            var dateColumnHeader = new CCLabelTTF(dateColumnHeaderText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            dateColumnHeader.AnchorPoint = CCPoint.AnchorMiddleLeft;
            dateColumnHeader.Position = new CCPoint(
                0.65f * this.ContentSize.Width,
                0.7f  * this.ContentSize.Height);
            this.AddChild(dateColumnHeader);

            var tableStartHeight = scoreColumnHeader.Position.Y; // - (0.05f * this.ContentSize.Height);
            this._userScoreScore = new List<CCLabelTTF>();
            for (int i = 0; i < 5; i++)
            {
                var score = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                score.AnchorPoint = CCPoint.AnchorMiddleRight;
                score.Position = new CCPoint(
                    0.25f * this.ContentSize.Width, 
                    tableStartHeight - (0.08f * this.ContentSize.Height * (i+1)));
                this.AddChild(score);
                this._userScoreScore.Add(score);
            }

            this._userScoreTime = new List<CCLabelTTF>();
            for (int i = 0; i < 5; i++)
            {
                var time = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                time.AnchorPoint = CCPoint.AnchorMiddleLeft;
                time.Position = new CCPoint(
                    0.35f * this.ContentSize.Width,
                    tableStartHeight - (0.08f * this.ContentSize.Height * (i + 1)));
                this.AddChild(time);
                this._userScoreTime.Add(time);
            }

            this._userScoreDate = new List<CCLabelTTF>();
            for (int i = 0; i < 5; i++)
            {
                var date = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
                date.AnchorPoint = CCPoint.AnchorMiddleLeft;
                date.Position = new CCPoint(
                    0.65f * this.ContentSize.Width,
                    tableStartHeight - (0.08f * this.ContentSize.Height * (i + 1)));
                this.AddChild(date);
                this._userScoreDate.Add(date);
            }

            /* TODO: Add back in when facebook support is here
            // Top global scores
            var topGlobalScoresText = string.Empty;
#if ANDROID
            topGlobalScoresText = Program.SharedProgram.Resources.GetString(Resource.String.AchievementsTopGlobalScores);
#elif IOS
            topGlobalScoresText = NSBundle.MainBundle.LocalizedString(Strings.AchievementsTopGlobalScores, Strings.AchievementsTopGlobalScores);
#else
            topGlobalScoresText = AppResources.AchievementsTopGlobalScores;
#endif
            var globalScoresHeader = new CCLabelTTF(topGlobalScoresText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            globalScoresHeader.Position = new CCPoint(
                0.5f * this.ContentSize.Width, 
                0.5f * this.ContentSize.Height);
            this.AddChild(globalScoresHeader);
            this._globalScoreScore = new List<CCLabelTTF>();
            for (int i = 0; i < 5; i++)
            {
                var score = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
                this._globalScoreScore.Add(score);
                score.Position = new CCPoint(
                    0.1f * this.ContentSize.Width, 
                    globalScoresHeader.Position.Y - (0.05f * this.ContentSize.Height * (i + 1)));
            }
            this._globalScoreUsername = new List<CCLabelTTF>();
            for (int i = 0; i < 5; i++)
            {
                var username = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
                this._globalScoreUsername.Add(username);
                username.Position = new CCPoint(
                    0.6f * this.ContentSize.Width, 
                    userScoresHeader.Position.Y - (0.05f * this.ContentSize.Height * (i + 1)));
            }
            this._globalScoreDate = new List<CCLabelTTF>();
            for (int i = 0; i < 5; i++)
            {
                var date = new CCLabelTTF(string.Empty, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
                this._globalScoreDate.Add(date);
                date.Position = new CCPoint(
                    0.8f * this.ContentSize.Width, 
                    userScoresHeader.Position.Y - (0.05f * this.ContentSize.Height * (i + 1)));
            }
            */

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

            // Animate layer
            this.RunAction(this._layerActionIn);

            // Get latest scores and display them
            this.UpdateUserScores();
            
            // TODO: Add back in when facebook scores is here
            // this.UpdateGlobalScores();
        }

        #endregion

        #region Helper methods

        private async void UpdateUserScores() 
        {
            try
            {
                // Get local top 5 (actually up to top 5, could be less if we do not have 5 scores set yet)
                var scoreRepository = new FacebookScoreRepository();
                var topScores = scoreRepository.GetTopScoresForPlayer(5);
                var topScoresCount = topScores.Count() > 5 ? 5 : topScores.Count();

                // Update display
                for(int i = 0; i < topScoresCount; i++)
                {
                    this._userScoreScore[i].Text = topScores[i].Score.ToString();
                    this._userScoreTime[i].Text = topScores[i].ScoreTime.ToString(@"h\:mm\:ss");
                    this._userScoreDate[i].Text = topScores[i].CreateDate.ToString("d");
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception in UpdateUserScores: " + ex);
            }
        }

        private async void UpdateGlobalScores()
        {
            try
            {
                // Get remote top 5
                var oauthResultRepository = new OAuthResultRepository();
                var facebookOAuthResult = oauthResultRepository.Read(OAuthType.Facebook);
                if (facebookOAuthResult != null &&
                    !string.IsNullOrEmpty(facebookOAuthResult.AccessToken))
                {
                    var scoreService = new FacebookScoreService();
                    var topScoresForAll = await scoreService.GetScoresForAllPlayersAsync();

                    // Update display
                    for (int i = 0; i < 5; i++)
                    {
                        this._globalScoreScore[i].Text = topScoresForAll[i].Score.ToString();

                        /* TODO
                        this._globalScoreUsername[i].Text = topScoresForAll[i].Username;
                        this._globalScoreDate[i].Text = topScoresForAll[i].CreatedDate.ToString();
                        */
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in UpdateGlobalScores: " + ex);
            }
        }

       #endregion

    }
}