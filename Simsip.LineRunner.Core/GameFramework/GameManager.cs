using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Utils;
using System;
using System.Diagnostics;
using System.Linq;


namespace Simsip.LineRunner.GameFramework
{
    public class GameManager
    {
        private int _gameStartPageNumber;
        private int _gameStartLineNumber;
        private int _gameStartScore;
        private bool _gameKillsAllowed;
        private string _linerunnerPack;

        //
        // Singleton implementation/defaults
        //
        private static GameManager _sharedGameManager = new GameManager();

        private GameManager()
        {
                this.TheActionManager = new ActionManager();

                this.GameStartPageNumber = UserDefaults.SharedUserDefault.GetIntegerForKey(
                    GameConstants.USER_DEFAULT_KEY_START_PAGE,
                    GameConstants.USER_DEFAULT_INITIAL_START_PAGE);
                this.GameStartLineNumber = UserDefaults.SharedUserDefault.GetIntegerForKey(
                    GameConstants.USER_DEFAULT_KEY_START_LINE,
                    GameConstants.USER_DEFAULT_INITIAL_START_LINE);
                this.GameAreKillsAllowed = UserDefaults.SharedUserDefault.GetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_KILLS_ALLOWED,
                    GameConstants.USER_DEFAULT_INITIAL_KILLS_ALLOWED);
                this.LinerunnerPack = UserDefaults.SharedUserDefault.GetStringForKey(
                    GameConstants.USER_DEFAULT_KEY_LINERUNNER_PACK,
                    GameConstants.USER_DEFAULT_INITIAL_LINERUNNER_PACK);

                this.CurrentScore = this.GameStartScore;

                this.AdminAreParticlesAllowed = true;
        }

        #region Properties

        /// <summary>
        /// A singleton instance of our GameManager.
        /// </summary>
        public static GameManager SharedGameManager { get { return _sharedGameManager; } }

        /// <summary>
        /// Similar to Cocos2D's CCActionManager, this manager will control all animation
        /// actions for Box2DModels.
        /// 
        /// It mimics the Cocos2D CCActions for 3D models.
        /// </summary>
        public ActionManager TheActionManager { get; private set; }

        /// <summary>
        /// Used to flag when we have created and initialized all game services
        /// so that other game components (e.g., layers) can intelligently start
        /// to update/draw.
        /// </summary>
        public bool ServicesReadyForUpdate { get; set; }

        /// <summary>
        /// Controls whether hero will be killed via normal game mechanics
        /// (e.g., colliding with obstacles, lines, etc.).
        /// 
        /// Debug screen has an option to toggle this flag.
        /// </summary>
        public bool GameAreKillsAllowed 
        {
            get
            {
                return this._gameKillsAllowed;
            }
            set
            {
                this._gameKillsAllowed = value;
                UserDefaults.SharedUserDefault.SetBoolForKey(
                    GameConstants.USER_DEFAULT_KEY_KILLS_ALLOWED,
                    this._gameKillsAllowed);
            }
        }

        /// <summary>
        /// Controls whether particles are displayed.
        /// (e.g., colliding with obstacles, lines, etc.).
        /// 
        /// Admin screen has an option to toggle this flag.
        /// </summary>
        public bool AdminAreParticlesAllowed { get; set; }
                    
        /// <summary>
        /// Allow admins to set page to start on via the Admin screen.
        /// </summary>
        public int GameStartPageNumber 
        { 
            get
            {
                return this._gameStartPageNumber;
            }
            set
            {
                this._gameStartPageNumber = value;
                UserDefaults.SharedUserDefault.SetIntegerForKey(
                    GameConstants.USER_DEFAULT_KEY_START_PAGE,
                    this._gameStartPageNumber);

                this.SetStartScore();
            }
        }

        /// <summary>
        /// Allow admins to set line to start on via the Admin screen.
        /// </summary>
        public int GameStartLineNumber
        {
            get
            {
                return this._gameStartLineNumber;
            }
            set
            {
                this._gameStartLineNumber = value;
                UserDefaults.SharedUserDefault.SetIntegerForKey(
                    GameConstants.USER_DEFAULT_KEY_START_LINE,
                    this._gameStartLineNumber);

                this.SetStartScore();
            }

        }

        /// <summary>
        /// The Linerunner 3D pack that is currently selected to be played.
        /// </summary>
        public string LinerunnerPack
        {
            get
            {
                return this._linerunnerPack;
            }
            set
            {
                this._linerunnerPack = value;
                UserDefaults.SharedUserDefault.SetStringForKey(
                    GameConstants.USER_DEFAULT_KEY_LINERUNNER_PACK,
                    this._linerunnerPack);
            }
        }

        /// <summary>
        /// Controls whether upgrade pages are displayed.
        /// 
        /// Admin screen has an option to toggle this flag.
        /// </summary>
        public bool AdminAreUpgradesAllowed { get; set; }

        /// <summary>
        /// Centralized access to current score.
        /// </summary>
        public int CurrentScore { get; set; }

        /// <summary>
        /// When admins set page to start, we will also update score to represent what
        /// player should have when getting to that page.
        /// </summary>
        public int GameStartScore 
        {
            get
            {
                return this._gameStartScore;
            }
        }

        #endregion

        #region Helper methods

        private void SetStartScore()
        {
            // Can we short circuit on determining start score?
            if (this.GameStartLineNumber == 0 &&
                this.GameStartPageNumber == 0)
            {
                this._gameStartScore = 0;
                return;
            }

            // Ok, we need to determine the start score
            var pageObstaclesRepository = new PageObstaclesRepository();
            var obstacles = pageObstaclesRepository.GetObstacles(GameManager.SharedGameManager.LinerunnerPack);
            var filteredObstacles = obstacles
                                    .Where(x =>
                                    (x.IsGoal == true ||
                                     x.ModelName.StartsWith(GameConstants.RandomPrefix))
                                    &&
                                    ( 
                                    (x.PageNumber < this.GameStartPageNumber)
                                    ||
                                    (x.PageNumber == this.GameStartPageNumber)
                                    &&
                                    x.LineNumber < this.GameStartLineNumber) 
                                    );

            int startScore = 0;
            foreach (var obstacle in filteredObstacles)
            {
                if (obstacle.ModelName.StartsWith(GameConstants.RandomPrefix))
                {
                    // Grab the "count" of this random obstacle entry
                    var count = int.Parse(obstacle.ModelName.Substring(6, 2));
                    startScore += count;
                }
                else
                {
                    startScore++;
                }
            }

            this._gameStartScore = startScore;
        }

        #endregion

    }
}