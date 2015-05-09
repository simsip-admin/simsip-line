using Cocos2D;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data;
using Simsip.LineRunner.Entities;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.Panes;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.Resources;
using Simsip.LineRunner.Utils;
using Engine.Graphics;
using Engine.Common.Vector;
using Engine.Universe;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Worlds
{
    public class WorldsLayer : UILayer
    {
        private CoreScene _parent;

        // Services we require
        private IWorld _world;

        // Layer actions
        private CCAction _layerActionIn;
        private CCAction _layerActionOut;

        // World selections menus
        private IList<CCMenu> _worldSelectionMenus;
        private IList<CCMenu> _worldDeleteMenus;

        // World selection navigation
        private int _currentWorldsPage;
        private int _totalWorldsPages;
        private CCMenu _navigateWorldsBack;
        private CCMenu _navigateWorldsForward;

        // World save name
        private KeyboardNotificationLayer _worldSaveNotificationLayer;
        private CCTextFieldTTF _worldSaveTextField;

        // Used in refreshing page when we enter
        private int _staticTag = 1;
        private int _dynamicTag = 2;

        public WorldsLayer(CoreScene parent)
        {
            this._parent = parent;

            // Import required services.
            this._world = (IWorld)TheGame.SharedGame.Services.GetService(typeof(IWorld));

            // Get these set up for relative positioning below
            var screenSize = CCDirector.SharedDirector.WinSize;
            this.ContentSize = new CCSize(
                0.6f * screenSize.Width,
                0.6f * screenSize.Height);

            // Layer transition in/out
            var layerEndPosition = new CCPoint(
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

            // Title
            var worldsText = string.Empty;
#if ANDROID
            worldsText = Program.SharedProgram.Resources.GetString(Resource.String.WorldsWorlds);
#elif IOS
            worldsText = NSBundle.MainBundle.LocalizedString(Strings.WorldsWorlds, Strings.WorldsWorlds);
#else
            worldsText = AppResources.WorldsWorlds;
#endif
            var title = new CCLabelTTF(worldsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_LARGE);
            title.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.9f * this.ContentSize.Height);
            this.AddChild(title);
            title.Tag = this._staticTag;

            // Use this world
            var useThisWorldText = string.Empty;
#if ANDROID
            useThisWorldText = Program.SharedProgram.Resources.GetString(Resource.String.WorldsUseThisWorld);
#elif IOS
            useThisWorldText = NSBundle.MainBundle.LocalizedString(Strings.WorldsUseThisWorld, Strings.WorldsUseThisWorld);
#else
            useThisWorldText = AppResources.WorldsUseThisWorld;
#endif
            var use = new CCLabelTTF(useThisWorldText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            use.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this.AddChild(use);
            use.Tag = this._staticTag;

            // Select new world
            var selectNewWorldText = string.Empty;
#if ANDROID
            selectNewWorldText = Program.SharedProgram.Resources.GetString(Resource.String.WorldsSelectNewWorld);
#elif IOS
            selectNewWorldText = NSBundle.MainBundle.LocalizedString(Strings.WorldsSelectNewWorld, Strings.WorldsSelectNewWorld);
#else
            selectNewWorldText = AppResources.WorldsSelectNewWorld;
#endif
            var select = new CCLabelTTF(selectNewWorldText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            select.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.5f * this.ContentSize.Height);
            this.AddChild(select);
            select.Tag = this._staticTag;

            // Save world
            var saveWorldAsText = string.Empty;
#if ANDROID
            saveWorldAsText = Program.SharedProgram.Resources.GetString(Resource.String.WorldsSaveWorldAs);
#elif IOS
            saveWorldAsText = NSBundle.MainBundle.LocalizedString(Strings.WorldsSaveWorldAs, Strings.WorldsSaveWorldAs);
#else
            saveWorldAsText = AppResources.WorldsSaveWorldAs;
#endif
            this._worldSaveTextField = new CCTextFieldTTF(saveWorldAsText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_NORMAL);
            this._worldSaveTextField.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.8f * this.ContentSize.Height);
            this._worldSaveTextField.AutoEdit = true;
            this._worldSaveTextField.EditTitle = saveWorldAsText;
            var saveWorldAsDescriptionText = string.Empty;
#if ANDROID
            saveWorldAsDescriptionText = Program.SharedProgram.Resources.GetString(Resource.String.WorldsSaveWorldAsDescription);
#elif IOS
            saveWorldAsDescriptionText = NSBundle.MainBundle.LocalizedString(Strings.WorldsSaveWorldAsDescription, Strings.WorldsSaveWorldAsDescription);
#else
            saveWorldAsDescriptionText = AppResources.WorldsSaveWorldAsDescription;
#endif
            this._worldSaveTextField.EditDescription = saveWorldAsDescriptionText;
            this._worldSaveTextField.ContentSize = new CCSize(      // Makes it easier to touch
                4f * this._worldSaveTextField.ContentSize.Width,
                1f * this._worldSaveTextField.ContentSize.Height);
            this._worldSaveNotificationLayer = new KeyboardNotificationLayer(this._worldSaveTextField);
            AddChild(this._worldSaveNotificationLayer);
            this._worldSaveNotificationLayer.Tag = this._staticTag;

            // Navigate back
            var navigateBackButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => { this.NavigateWorldsBack(); });
            this._navigateWorldsBack = new CCMenu(
                new CCMenuItem[] 
                    {
                        navigateBackButton, 
                    });
            this._navigateWorldsBack.Position = new CCPoint(
                0.25f * this.ContentSize.Width, 
                0.2f  * this.ContentSize.Height);
            this.AddChild(this._navigateWorldsBack);
            this._navigateWorldsBack.Tag = this._staticTag;
            this._navigateWorldsBack.Visible = false;

            // Navigate forward
            var navigateForwardButton =
                new CCMenuItemImage("Images/Icons/BackButtonNormal.png",
                                    "Images/Icons/BackButtonSelected.png",
                                    (obj) => { this.NavigateWorldsForward(); });
            this._navigateWorldsForward = new CCMenu(
                new CCMenuItem[] 
                    {
                        navigateForwardButton, 
                    });
            this._navigateWorldsForward.Position = new CCPoint(
                0.75f * this.ContentSize.Width, 
                0.2f  * this.ContentSize.Height);
            this.AddChild(this._navigateWorldsForward);
            this._navigateWorldsForward.Tag = this._staticTag;
            this._navigateWorldsForward.Visible = false;

            // Back
            var backButton =
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
            backMenu.Tag = this._staticTag;

        }

        #region Cocos2D overrides

        public override void OnEnter()
        {
            base.OnEnter();

            // Centrally handles refreshing all dynamic fields
            this.RefreshDisplay();

            // Animate layer
            this.RunAction(this._layerActionIn);
        }

        #endregion
        
        #region Helper methods

#if ANDROID || IOS || DESKTOP
        private void RefreshDisplay()
#elif WINDOWS_PHONE || NETFX_CORE
        private async void RefreshDisplay()
#endif
        {
            // Clear out previous fields that are dynamic (will be recrated below)
            this.RemoveAllChildrenByTag(this._dynamicTag, true);

            // Curent world
            var currentWorld = CCUserDefault.SharedUserDefault.GetStringForKey(GameConstants.USER_DEFAULT_KEY_CURRENT_WORLD, GameConstants.WORLD_DEFAULT);
            var current = new CCLabelTTF(currentWorld, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            current.Position = new CCPoint(
                0.5f * this.ContentSize.Width,
                0.7f * this.ContentSize.Height);
            this.AddChild(current);
            current.Tag = this._dynamicTag;

            // Selections
            this._worldSelectionMenus = new List<CCMenu>();
            var masterListWorlds = new List<CCMenuItemLabel>();
            var defaultText = string.Empty;
#if ANDROID
            defaultText = Program.SharedProgram.Resources.GetString(Resource.String.WorldsDefault);
#elif IOS
            defaultText = NSBundle.MainBundle.LocalizedString(Strings.WorldsDefault, Strings.WorldsDefault);
#else
            defaultText = AppResources.WorldsDefault;
#endif
            var defaultLabel = new CCLabelTTF(defaultText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            var defaultItem = new CCMenuItemLabel(defaultLabel,
                (obj) => { this.OnWorldSelected(GameConstants.WORLD_DEFAULT); });
            var generateNewText = string.Empty;
#if ANDROID
            generateNewText = Program.SharedProgram.Resources.GetString(Resource.String.WorldsGenerateNew);
#elif IOS
            generateNewText = NSBundle.MainBundle.LocalizedString(Strings.WorldsGenerateNew, Strings.WorldsGenerateNew);
#else
            generateNewText = AppResources.WorldsGenerateNew;
#endif
            var generateLabel = new CCLabelTTF(generateNewText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
            var generateItem = new CCMenuItemLabel(defaultLabel,
                (obj) => { this.OnWorldSelected(GameConstants.WORLD_GENERATE_NEW); });
            masterListWorlds.Add(defaultItem);
            masterListWorlds.Add(generateItem);

            this._worldDeleteMenus = new List<CCMenu>();
            var masterListDeletes = new List<CCMenuItemLabel>();
            masterListDeletes.Add(null);
            masterListDeletes.Add(null);

            // TODO: Move worlds to a designated folder
#if ANDROID || IOS || DESKTOP
            var worldNames = FileUtils.GetFolderNames(GameConstants.FOLDER_SAVES);
#elif WINDOWS_PHONE || NETFX_CORE
            var worldNames = await FileUtils.GetFolderNamesAsync(GameConstants.FOLDER_SAVES);
#endif
            foreach (var worldName in worldNames)
            {
                var worldLabel = new CCLabelTTF(worldName, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
                var worldItem = new CCMenuItemLabel(worldLabel,
                    (obj) => { this.OnWorldSelected(worldName); });
                masterListWorlds.Add(worldItem);

                var deleteText = string.Empty;
#if ANDROID
                deleteText = Program.SharedProgram.Resources.GetString(Resource.String.CommonDelete);
#elif IOS
                deleteText = NSBundle.MainBundle.LocalizedString(Strings.CommonDelete, Strings.CommonDelete);
#else
                deleteText = AppResources.CommonDelete;
#endif
                var deleteLabel = new CCLabelTTF(deleteText, GameConstants.FONT_FAMILY_NORMAL, GameConstants.FONT_SIZE_SMALL);
                var deleteItem = new CCMenuItemLabel(deleteLabel,
                    (obj) => { this.OnWorldDeleted(worldName); });
                masterListDeletes.Add(deleteItem);
            }

            var worldSubsetsList = masterListWorlds.InSetsOf(4);
            foreach (var subset in worldSubsetsList)
            {
                var worldSelectionMenu = new CCMenu();
                foreach (var menuItem in subset)
                {
                    worldSelectionMenu.AddChild(menuItem);
                }
                worldSelectionMenu.Position = new CCPoint(
                     0.25f * this.ContentSize.Width,
                     0.2f * this.ContentSize.Height);
                worldSelectionMenu.Tag = this._dynamicTag;

                this._worldSelectionMenus.Add(worldSelectionMenu);
            }

            var deleteSubsetsList = masterListDeletes.InSetsOf(4);
            foreach (var subset in deleteSubsetsList)
            {
                var worldDeleteMenu = new CCMenu();
                foreach (var menuItem in subset)
                {
                    if (menuItem != null)
                    {
                        worldDeleteMenu.AddChild(menuItem);
                    }
                }
                worldDeleteMenu.Position = new CCPoint(
                     0.75f * this.ContentSize.Width,
                     0.2f * this.ContentSize.Height);
                worldDeleteMenu.Tag = this._dynamicTag;

                this._worldDeleteMenus.Add(worldDeleteMenu);
            }

            // Only display first menu set
            this.AddChild(this._worldSelectionMenus[0]);
            this.AddChild(this._worldDeleteMenus[0]);

            // Set state and if we have more than one set of worlds, display navigation
            this._currentWorldsPage = 1;
            this._totalWorldsPages = worldSubsetsList.Count();
            if (this._totalWorldsPages > 1)
            {
                this._navigateWorldsForward.Visible = true;
            }
        }

        private void NavigateWorldsBack()
        {
            // Remove previous set of world selections
            this.RemoveChild(this._worldSelectionMenus[this._currentWorldsPage - 1]);
            this.RemoveChild(this._worldDeleteMenus[this._currentWorldsPage - 1]);

            // Proceed to set new state
            this._currentWorldsPage--;
            if (this._currentWorldsPage <= 1)
            {
                this._currentWorldsPage = 1;
            }

            // Turn on/off navigation elements as appropriate
            if (this._currentWorldsPage > 1)
            {
                this._navigateWorldsBack.Visible = true;
            }
            else
            {
                this._navigateWorldsBack.Visible = false;
            }
            if (this._currentWorldsPage < this._totalWorldsPages )
            {
                this._navigateWorldsForward.Visible = true;
            }
            else
            {
                this._navigateWorldsForward.Visible = false;
            }

            // Add in new world selection menu set
            this.AddChild(this._worldSelectionMenus[this._currentWorldsPage - 1]);
            this.AddChild(this._worldDeleteMenus[this._currentWorldsPage - 1]);
        }

        private void NavigateWorldsForward()
        {
            // Remove previous set of world selections
            this.RemoveChild(this._worldSelectionMenus[this._currentWorldsPage - 1]);
            this.RemoveChild(this._worldDeleteMenus[this._currentWorldsPage - 1]);

            // Proceed to set new state
            this._currentWorldsPage++;
            if (this._currentWorldsPage > this._totalWorldsPages)
            {
                this._currentWorldsPage = this._totalWorldsPages;
            }

            // Turn on/off navigation elements as appropriate
            if (this._currentWorldsPage > 1)
            {
                this._navigateWorldsBack.Visible = true;
            }
            else
            {
                this._navigateWorldsBack.Visible = false;
            }
            if (this._currentWorldsPage < this._totalWorldsPages)
            {
                this._navigateWorldsForward.Visible = true;
            }
            else
            {
                this._navigateWorldsForward.Visible = false;
            }

            // Add in new world selection menu set
            this.AddChild(this._worldSelectionMenus[this._currentWorldsPage - 1]);
            this.AddChild(this._worldDeleteMenus[this._currentWorldsPage - 1]);
        }

        private void OnWorldSelected(string name)
        {
            // Provide feedback
            var switchingWorldsText = string.Empty;
#if ANDROID
            switchingWorldsText = Program.SharedProgram.Resources.GetString(Resource.String.WorldsSwitchingWorlds);
#elif IOS
            switchingWorldsText = NSBundle.MainBundle.LocalizedString(Strings.WorldsSwitchingWorlds, Strings.WorldsSwitchingWorlds);
#else
            switchingWorldsText = AppResources.WorldsSwitchingWorlds;
#endif
            this._parent.TheMessageBoxLayer.Show(
                switchingWorldsText,
                string.Empty,
                MessageBoxType.MB_PROGRESS);

            // Record what was selected
            UserDefaults.SharedUserDefault.SetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_WORLD, 
                name);

            // Go for a refresh to get the new resource pack displayed
            this._parent.Refresh();

            // Ok, we're done here
            this._parent.TheMessageBoxLayer.Hide();
            this._parent.GoBack();
        }

#if ANDROID || IOS || DESKTOP
        private void OnWorldDeleted(string name)
#elif WINDOWS_PHONE || NETFX_CORE
        private async void OnWorldDeleted(string name)
#endif
        {
            // Cannot delete curent world
            var currentWorld = CCUserDefault.SharedUserDefault.GetStringForKey(
                GameConstants.USER_DEFAULT_KEY_CURRENT_WORLD, 
                GameConstants.WORLD_DEFAULT);
            if (currentWorld == name)
            {
                var errorText = string.Empty;
                var errorWorldActive = string.Empty;
#if ANDROID
                errorText = Program.SharedProgram.Resources.GetString(Resource.String.CommonError);
                errorWorldActive = Program.SharedProgram.Resources.GetString(Resource.String.WorldsErrorWorldActive);
#elif IOS
                errorText = NSBundle.MainBundle.LocalizedString(Strings.CommonError, Strings.CommonError);
                errorWorldActive = NSBundle.MainBundle.LocalizedString(Strings.WorldsErrorWorldActive, Strings.WorldsErrorWorldActive);
#else
                errorText = AppResources.CommonError;
                errorWorldActive = AppResources.WorldsErrorWorldActive;
#endif
                this._parent.TheMessageBoxLayer.Show(errorText,
                         errorWorldActive,
                         MessageBoxType.MB_OK);
                return;
            }

            // Ok to delete
            var worldPath = Path.Combine(GameConstants.FOLDER_SAVES, name);
#if ANDROID || IOS || DESKTOP
            FileUtils.DeleteFolder(worldPath);
#elif WINDOWS_PHONE || NETFX_CORE
            await FileUtils.DeleteFolderAsync(worldPath);
#endif

            this.RefreshDisplay();
        }

#if ANDROID || IOS || DESKTOP
        private void OnNew()
#elif WINDOWS_PHONE || NETFX_CORE
        private async void OnNew()
#endif
        {
            // What name will we try to save with
            var name = this._worldSaveTextField.Text;

            // Don't allow saves with our special pre-defined names
            if (name == GameConstants.WORLD_DEFAULT ||
                name == GameConstants.WORLD_GENERATE_NEW)
            {
                var errorText = string.Empty;
                var errorWorldCannotUseName = string.Empty;
#if ANDROID
                errorText = Program.SharedProgram.Resources.GetString(Resource.String.CommonError);
                errorWorldCannotUseName = Program.SharedProgram.Resources.GetString(Resource.String.WorldsErrorCannotUseName);
#elif IOS
                errorText = NSBundle.MainBundle.LocalizedString(Strings.CommonError, Strings.CommonError);
                errorWorldCannotUseName = NSBundle.MainBundle.LocalizedString(Strings.WorldsErrorCannotUseName, Strings.WorldsErrorCannotUseName);
#else
                errorText = AppResources.CommonError;
                errorWorldCannotUseName = AppResources.WorldsErrorCannotUseName;
#endif
                this._parent.TheMessageBoxLayer.Show(errorText,
                    errorWorldCannotUseName,
                    MessageBoxType.MB_OK);
                    return;
            }

            // Don't allow saves for worldnames that match folders that already exist
            var worldPath = Path.Combine(GameConstants.FOLDER_SAVES, name);
#if ANDROID || IOS || DESKTOP
            if (FileUtils.FolderExists(worldPath))
#elif WINDOWS_PHONE || NETFX_CORE
            if (await FileUtils.FolderExistsAsync(worldPath))
#endif
            {
                var errorText = string.Empty;
                var errorWorldAlreadyExists = string.Empty;
#if ANDROID
                errorText = Program.SharedProgram.Resources.GetString(Resource.String.CommonError);
                errorWorldAlreadyExists = Program.SharedProgram.Resources.GetString(Resource.String.WorldsErrorWorldAlreadyExists);
#elif IOS
                errorText = NSBundle.MainBundle.LocalizedString(Strings.CommonError, Strings.CommonError);
                errorWorldAlreadyExists = NSBundle.MainBundle.LocalizedString(Strings.WorldsErrorWorldAlreadyExists, Strings.WorldsErrorWorldAlreadyExists);
#else
                errorText = AppResources.CommonError;
                errorWorldAlreadyExists = AppResources.WorldsErrorWorldAlreadyExists;
#endif
                this._parent.TheMessageBoxLayer.Show(errorText,
                    errorWorldAlreadyExists,
                    MessageBoxType.MB_OK);
                return;
            }

            // Ok to save
#if ANDROID || IOS || DESKTOP
            var errorStr = this._world.New(name);
#elif WINDOWS_PHONE || NETFX_CORE
            var errorStr = await this._world.NewAsync(name);
#endif

            // Did we have an error saving?
            if (errorStr != string.Empty)
            {
                var errorText = string.Empty;
#if ANDROID
                errorText = Program.SharedProgram.Resources.GetString(Resource.String.CommonError);
#elif IOS
                errorText = NSBundle.MainBundle.LocalizedString(Strings.CommonError, Strings.CommonError);
#else
                errorText = AppResources.CommonError;
#endif
                this._parent.TheMessageBoxLayer.Show(errorText,
                    errorStr,
                    MessageBoxType.MB_OK);
                return;
            }

            this.RefreshDisplay();
        }

        #endregion
    }
}