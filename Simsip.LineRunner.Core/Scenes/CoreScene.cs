using Cocos2D;
using Engine.Chunks.Generators.Biomes;
using Engine.Assets;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Scenes.Achievements;
using Simsip.LineRunner.Scenes.Action;
using Simsip.LineRunner.Scenes.Admin;
using Simsip.LineRunner.Scenes.Credits;
using Simsip.LineRunner.Scenes.Finish;
using Simsip.LineRunner.Scenes.Help;
using Simsip.LineRunner.Scenes.Hud;
using Simsip.LineRunner.Scenes.Login;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.Scenes.Options;
using Simsip.LineRunner.Scenes.Pads;
using Simsip.LineRunner.Scenes.Ratings;
using Simsip.LineRunner.Scenes.ResourcePacks;
using Simsip.LineRunner.Scenes.Start;
using Simsip.LineRunner.Scenes.Worlds;
using System;
using System.Collections.Generic;
using Simsip.LineRunner.Utils;
using Simsip.LineRunner.Scenes.Lines;


namespace Simsip.LineRunner.Scenes
{
    public class CoreScene : GameScene
    {
        // Services we need
        private IAssetManager _assetManager;
        private IPadRepository _padRepository;
        private IResourcePackRepository _resourcePackRepository;

        // State we maintain
        private Stack<LayerTags> _navigationStack;

        public CoreScene()
        {
            // CreateLineHitParticles services we need
            this._assetManager = (IAssetManager)TheGame.SharedGame.Services.GetService(typeof(IAssetManager)); 
            this._padRepository = new PadRepository();
            this._resourcePackRepository = new ResourcePackRepository();

            // Initialize state
            this._navigationStack = new Stack<LayerTags>();

            // Get our start and action layers displayed as quickly as possible
            this.AddChild(this.TheStartPage1Layer, SceneZOrder.UILayer);
            this.AddChild(this.TheActionLayer, SceneZOrder.ActionLayer);

            // Since we bypassed our navigation service above for the start layer, we need
            // to prime the service here
            _navigationStack.Push(LayerTags.StartPage1Layer);
        }

        #region Properties

        private AchievementsLayer _theAchievementsLayer;
        /// <summary>
        /// UI layer to display top 5 scores, rankings, etc.
        /// </summary>
        public AchievementsLayer TheAchievementsLayer
        {
            get
            {
                if (_theAchievementsLayer == null)
                {
                    _theAchievementsLayer = new AchievementsLayer(this);
                    _theAchievementsLayer.Tag = (int)LayerTags.AchievementsLayer;
                }
                return _theAchievementsLayer;
            }
        }

        private ActionLayer _theActionLayer;
        /// <summary>
        /// The main game layer.
        /// 
        /// UI layers are layered above this.
        /// </summary>
        public ActionLayer TheActionLayer
        {
            get
            {
                if (_theActionLayer == null)
                {
                    _theActionLayer = new ActionLayer(this);
                    _theActionLayer.Tag = (int)LayerTags.ActionLayer;
                }
                return _theActionLayer;
            }
        }

        private AdminLayer _theAdminLayer;
        /// <summary>
        /// Displays a variety of settings available only for debugging, testing, etc.
        /// </summary>
        public AdminLayer TheAdminLayer
        {
            get
            {
                if (_theAdminLayer == null)
                {
                    _theAdminLayer = new AdminLayer(this);
                    _theAdminLayer.Tag = (int)LayerTags.AdminLayer;
                }
                return _theAdminLayer;
            }
        }

        private CreditsMasterLayer _theCreditsMasterLayer;
        /// <summary>
        /// Displays creator of game and necessary asset attributions (e.g., icons)
        /// </summary>
        public CreditsMasterLayer TheCreditsLayer
        {
            get
            {
                if (_theCreditsMasterLayer == null)
                {
                    _theCreditsMasterLayer = new CreditsMasterLayer(this);
                    _theCreditsMasterLayer.Tag = (int)LayerTags.CreditsMasterLayer;
                }
                return _theCreditsMasterLayer;
            }
        }

        private FinishLayer _theFinishLayer;
        /// <summary>
        /// UI layer that is displayed as the game over screen.
        /// </summary>
        public FinishLayer TheFinishLayer
        {
            get
            {
                if (_theFinishLayer == null)
                {
                    _theFinishLayer = new FinishLayer(this);
                    _theFinishLayer.Tag = (int)LayerTags.FinishLayer;
                }
                return _theFinishLayer;
            }
        }

        private HelpMasterLayer _theHelpMasterLayer;
        /// <summary>
        /// The UI layer that displays the on-line help pages.
        /// </summary>
        public HelpMasterLayer TheHelpMasterLayer
        {
            get
            {
                if (_theHelpMasterLayer == null)
                {
                    this._theHelpMasterLayer = new HelpMasterLayer(this);
                    this._theHelpMasterLayer.Tag = (int)LayerTags.HelpMasterLayer;
                }
                return _theHelpMasterLayer;
            }
        }

        private HudLayer _theHudLayer;
        /// <summary>
        /// The UI layer that displays the on-going score as the game progresses.
        /// </summary>
        public HudLayer TheHudLayer
        {
            get
            {
                if (_theHudLayer == null)
                {
                    _theHudLayer = new HudLayer(this);
                    _theHudLayer.Tag = (int)LayerTags.HudLayer;
                }
                return _theHudLayer;
            }
        }

        private LinesLayer _theLinesLayer;
        /// <summary>
        /// The screen that is displayed for users to select a line model to display in game.
        /// </summary>
        public LinesLayer TheLinesLayer
        {
            get
            {
                if (_theLinesLayer == null)
                {
                    _theLinesLayer = new LinesLayer(this);
                    _theLinesLayer.Tag = (int)LayerTags.LinesLayer;
                }
                return _theLinesLayer;
            }
        }

        private LoginLayer _theLoginLayer;
        /// <summary>
        /// The screen that is displayed when we need to prompt the user
        /// to login or create a username/password for scoreboard purposes.
        /// </summary>
        public LoginLayer TheLoginLayer
        {
            get
            {
                if (_theLoginLayer == null)
                {
                    _theLoginLayer = new LoginLayer(this);
                    _theLoginLayer.Tag = (int)LayerTags.LoginLayer;
                }
                return _theLoginLayer;
            }
        }

        private MessageBoxLayer _theMessageBoxLayer;
        /// <summary>
        /// UI layer that provides generic message box capabilities (e.g., yes/no, ok/cancel, etc.).
        /// </summary>
        public MessageBoxLayer TheMessageBoxLayer
        {
            get
            {
                if (_theMessageBoxLayer == null)
                {
                    _theMessageBoxLayer = new MessageBoxLayer(this);
                    _theMessageBoxLayer.Tag = (int)LayerTags.MessageBoxLayer;
                }
                return _theMessageBoxLayer;
            }
        }

        private OptionsMasterLayer _theOptionsMasterLayer;
        /// <summary>
        /// UI layer that allows user to set a variety of options.
        /// </summary>
        public OptionsMasterLayer TheOptionsLayer
        {
            get
            {
                if (_theOptionsMasterLayer == null)
                {
                    _theOptionsMasterLayer = new OptionsMasterLayer(this);
                    _theOptionsMasterLayer.Tag = (int)LayerTags.OptionsMasterLayer;
                }
                return _theOptionsMasterLayer;
            }
        }

        private PadsLayer _thePadsLayer;
        /// <summary>
        /// UI layer that allows user to select a pad.
        /// </summary>
        public PadsLayer ThePadsLayer
        {
            get
            {
                if (_thePadsLayer == null)
                {
                    _thePadsLayer = new PadsLayer(this);
                    _thePadsLayer.Tag = (int)LayerTags.PadsLayer;
                }
                return _thePadsLayer;
            }
        }

        private RatingsLayer _theRatingsLayer;
        /// <summary>
        /// UI layer that programatically pops-up to prompt players to rate us.
        /// </summary>
        public RatingsLayer TheRatingsLayer
        {
            get
            {
                if (_theRatingsLayer == null)
                {
                    _theRatingsLayer = new RatingsLayer(this);
                    _theRatingsLayer.Tag = (int)LayerTags.RatingsLayer;
                }
                return _theRatingsLayer;
            }
        }

        private ResourcePacksLayer _theResourcePacksLayer;
        /// <summary>
        /// UI layer that allows user to select a resource pack.
        /// </summary>
        public ResourcePacksLayer TheResourcePacksLayer
        {
            get
            {
                if (_theResourcePacksLayer == null)
                {
                    _theResourcePacksLayer = new ResourcePacksLayer(this);
                    _theResourcePacksLayer.Tag = (int)LayerTags.ResourcePacksLayer;
                }
                return _theResourcePacksLayer;
            }
        }

        private StartPage1Layer _theStartPage1Layer;
        /// <summary>
        /// The UI layer that allows players to start the game or navigate
        /// to the options screen.
        /// </summary>
        public StartPage1Layer TheStartPage1Layer
        {
            get
            {
                if (_theStartPage1Layer == null)
                {
                    _theStartPage1Layer = new StartPage1Layer(this);
                    _theStartPage1Layer.Tag = (int)LayerTags.StartPage1Layer;
                }
                return _theStartPage1Layer;
            }
        }

        private StartPage2Layer _theStartPage2Layer;
        /// <summary>
        /// The UI layer that displays the initial tap indicator.
        /// </summary>
        public StartPage2Layer TheStartPage2Layer
        {
            get
            {
                if (_theStartPage2Layer == null)
                {
                    _theStartPage2Layer = new StartPage2Layer(this);
                    _theStartPage2Layer.Tag = (int)LayerTags.StartPage2Layer;
                }
                return _theStartPage2Layer;
            }
        }

        private WorldsLayer _theWorldsLayer;
        /// <summary>
        /// The UI layer that allows players to manage their worlds.
        /// </summary>
        public WorldsLayer TheWorldsLayer
        {
            get
            {
                if (_theWorldsLayer == null)
                {
                    _theWorldsLayer = new WorldsLayer(this);
                    _theWorldsLayer.Tag = (int)LayerTags.WorldsLayer;
                }
                return _theWorldsLayer;
            }
        }


        #endregion

        #region Events

        public event SwitchingUIEventHandler SwitchedUI;

        #endregion

        #region Navigation

        public void GoBack()
        {
            // Remove top of stack to get to back entry to go back to
            var currentLayer = this._navigationStack.Pop();

            // Get beck entry we want to navigate to
            var backEntry = this._navigationStack.Peek();

            // Restore our stack to the way it was
            this._navigationStack.Push(currentLayer);

            this.Navigate(newLayerTag: backEntry, goBack: true);
        }

        public void RemoveBackEntry()
        {
            // Remove top of stack to get to back entry to remove
            var currentLayer = this._navigationStack.Pop();

            // Remove back entry
            this._navigationStack.Pop();

            // Restore top of stack
            this._navigationStack.Push(currentLayer);
        }

        /// <summary>
        /// Adjusts navigation stack to current entry and start layer.
        /// 
        /// Use in  deeply navigated stack before you issue a GoBack()
        /// to get back to start layer and bypass all intermediate layers.
        /// </summary>
        public void AdjustNavigationStackToStart()
        {
            // IMPORTANT:
            // Assumes StartPage1Layer is already last in our stack and
            // we want to keep our current top entry layer in place.
            // Example:
            //     (top)                                (bottom)
            // <CurrentLayer><remove-1>...<remove-n><StartPage1Layer>
            // will be adjusted to:
            //     (top)          (bottom)
            // <CurrentLayer><StartPage1Layer>

            // Pop reference to current layer
            var currentLayer = _navigationStack.Pop();

            // Pop everything off stack except for last entry
            var stackCountToRemove = _navigationStack.Count - 1;
            for (var i = 0; i < stackCountToRemove; i++)
            {
                _navigationStack.Pop();
            }

            // Push reference to current entry back on
            _navigationStack.Push(currentLayer);
        }

        public void Navigate(LayerTags newLayerTag, bool goBack=false)
        {
            // First record where we are coming from
            var oldLayerTag = this._navigationStack.Peek();

            // If navigating back, adjust navigation stack accordingly
            if (goBack)
            {
                this._navigationStack.Pop();
                this._navigationStack.Pop();
            }

            var layerToRemove = this.GetChildByTag((int)oldLayerTag);
            layerToRemove.RemoveFromParent();

            // Now get the new layer and add it
            var layerToAdd = GetUILayer(newLayerTag);
            this.AddChild(layerToAdd, SceneZOrder.UILayer);

            // Record the switch into our navigation stack,
            _navigationStack.Push(newLayerTag);

            // And raise event for all other layers to adjust accordingly
            var args = new SwitchUIEventArgs(oldLayerTag, newLayerTag);
            SwitchedUI(this, args);
        }

        #endregion

        #region Api

        public void Refresh()
        {
            this.TheActionLayer.Refresh();
        }

        #endregion

        #region Helper methods

        private CCLayer GetUILayer(LayerTags tag)
        {
            switch (tag)
            {
                case LayerTags.AchievementsLayer:
                    {
                        return TheAchievementsLayer;
                    }
                case LayerTags.AdminLayer:
                    {
                        return TheAdminLayer;
                    }
                case LayerTags.CreditsMasterLayer:
                    {
                        return TheCreditsLayer;
                    }
                case LayerTags.FinishLayer:
                    {
                        return TheFinishLayer;
                    }
                case LayerTags.HelpMasterLayer:
                    {
                        return TheHelpMasterLayer;
                    }
                case LayerTags.HudLayer:
                    {
                        return TheHudLayer;
                    }
                case LayerTags.LinesLayer:
                    {
                        return TheLinesLayer;
                    }
                case LayerTags.LoginLayer:
                    {
                        return TheLoginLayer;
                    }
                case LayerTags.MessageBoxLayer:
                    {
                        return TheMessageBoxLayer;
                    }
                case LayerTags.OptionsMasterLayer:
                    {
                        return TheOptionsLayer;
                    }
                case LayerTags.PadsLayer:
                    {
                        return ThePadsLayer;
                    }
                case LayerTags.RatingsLayer:
                    {
                        return TheRatingsLayer;
                    }
                case LayerTags.ResourcePacksLayer:
                    {
                        return TheResourcePacksLayer;
                    }
                case LayerTags.StartPage1Layer:
                    {
                        return TheStartPage1Layer;
                    }
                case LayerTags.StartPage2Layer:
                    {
                        return TheStartPage2Layer;
                    }
                case LayerTags.WorldsLayer:
                    {
                        return TheWorldsLayer;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        #endregion
    }

    public delegate void SwitchingUIEventHandler(object sender, SwitchUIEventArgs e);

    public class SwitchUIEventArgs : EventArgs
    {
        private LayerTags _oldLayer;
        private LayerTags _newLayer;

        public SwitchUIEventArgs(LayerTags oldLayer, LayerTags newLayer)
        {
            this._oldLayer = oldLayer;
            this._newLayer = newLayer;
        }

        public LayerTags OldLayer
        {
            get { return this._oldLayer; }
        }

        public LayerTags NewLayer
        {
            get { return this._newLayer; }
        }
    }
}