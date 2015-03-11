using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Utils;


namespace Simsip.LineRunner.GameFramework
{
    public class GameManager
    {
        //
        // Singleton implementation/defaults
        //
        private static GameManager _sharedGameManager = new GameManager();
        private GameManager()
        {
            this.CurrentScore = 0;
            this.TheActionManager = new ActionManager();

            this.AdminStartPageNumber = 1;
            this.AdminStartLineNumber = 1;
            this.AdminStartScore = 0;
            this.AdminIsKillAllowed = false;
            this.AdminIsParticlesAllowed = true;
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
        public bool AdminIsKillAllowed { get; set; }

        /// <summary>
        /// Controls whether particles are displayed.
        /// (e.g., colliding with obstacles, lines, etc.).
        /// 
        /// Debug screen has an option to toggle this flag.
        /// </summary>
        public bool AdminIsParticlesAllowed { get; set; }
                    
        /// <summary>
        /// Allow admins to set page to start on via the Admin screen.
        /// </summary>
        public int AdminStartPageNumber { get; set; }

        /// <summary>
        /// Allow admins to set line to start on via the Admin screen.
        /// </summary>
        public int AdminStartLineNumber { get; set; }

        /// <summary>
        /// Centralized access to current score.
        /// </summary>
        public int CurrentScore { get; set; }

        /// <summary>
        /// When admins set page to start, we will also update score to represent what
        /// player should have when getting to that page.
        /// </summary>
        public int AdminStartScore { get; set; }

        #endregion

    }
}