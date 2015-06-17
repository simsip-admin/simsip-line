using Cocos2D;
using Engine.Common.Logging;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionTests;
using BEPUphysics.Constraints.TwoEntity.JointLimits;
using BEPUphysics.Paths.PathFollowing;
using Engine.Input;
using Simsip.LineRunner.ContentPipeline;
using Simsip.LineRunner.Data.Scoreoid;
using Simsip.LineRunner.Entities.Scoreoid;
using Simsip.LineRunner.Scenes;
using Simsip.LineRunner.Effects.Stock;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Foundation;
#else
using System.Threading;
#endif
#if WINDOWS_PHONE
using Simsip.LineRunner.Concurrent;
#else
using System.Collections.Concurrent;
#endif


namespace Simsip.LineRunner.GameObjects.Characters
{
    public class CharacterCache : GameComponent, ICharacterCache
    {
        // Required services
        private IInputManager _inputManager;
        private IPageCache _pageCache;
        private ILineCache _lineCache;
        private IPhysicsManager _physicsManager;
        private IPageCharactersRepository _pageCharactersRepository;
        private ICharacterRepository _characterRepository;

        // State we maintain
        private int _currentPageNumber;
        private int _currentLineNumber;
        private GameState _currentGameState;

        // Determines what gets asked to be drawn
        private OcTreeNode _ocTreeRoot;

        // Physics body with fixtures to hold hero in place at start position
        private CompoundBody _holderForHero;

        // Support for limiting physics of hero
        private RevoluteLimit _limitHeroRevoluteX;
        private RevoluteLimit _limitHeroRevoluteY;
        private RevoluteLimit _limitHeroRevoluteZ;
        private EntityRotator _heroRotator;

        // Support for pausing
        private bool _paused;

        // Support for staging the results of asynchronous loads and then signaling
        // we need the results processed on the next update cycle
        private class LoadContentThreadArgs
        {
            public LoadContentAsyncType TheLoadContentAsyncType;
            public GameState TheGameState;
            public int PageNumber;
            public int[] LineNumbers;
            public IList<CharacterModel> CharacterModelsAsync;
        }
        private ConcurrentQueue<LoadContentThreadArgs> _loadContentThreadResults;

        // Used for controlling loading/unloading of hero resources.
        // IMPORTANT: As more characters are added, additional custom content
        //            managers can be added to control level loading, etc.
        private CustomContentManager _heroCustomContentManager;

        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public CharacterCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(ICharacterCache), this); 
        }

        #region Properties

        public IList<CharacterModel> CharacterModels { get; private set; }

        #endregion

        #region GameComponent Overrides

        public override void Initialize()
        {
            // Initialize state
            this._currentPageNumber = GameManager.SharedGameManager.GameStartPageNumber;
            this._currentLineNumber = 1;
            this.CharacterModels = new List<CharacterModel>();
            this._loadContentThreadResults = new ConcurrentQueue<LoadContentThreadArgs>(); 

            // Import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._pageCache = (IPageCache)this.Game.Services.GetService(typeof(IPageCache));
            this._lineCache = (ILineCache)this.Game.Services.GetService(typeof(ILineCache));
            this._physicsManager = (IPhysicsManager)this.Game.Services.GetService(typeof(IPhysicsManager));
            this._pageCharactersRepository = new PageCharactersRepository();
            this._characterRepository = new CharacterRepository();

            // Initialize our drawing filter
            this._ocTreeRoot = new OcTreeNode(new Vector3(0, 0, 0), OcTreeNode.DefaultSize);

            this._heroCustomContentManager = new CustomContentManager(
               TheGame.SharedGame.Services,
               TheGame.SharedGame.Content.RootDirectory);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (this.TheHeroModel == null)
            {
                return;
            }

            switch (this._currentGameState)
            {
                case GameState.Intro:
                    {
                        this.TheHeroModel.Update();

                        break;
                    }
                case GameState.Moving:
                    {
                        this.TheHeroModel.Update();

                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        this.TheHeroModel.Update();

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        this.TheHeroModel.Update();

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        this.TheHeroModel.Update();

                        break;
                    }
                case GameState.Start:
                    {
                        this.TheHeroModel.Update();

                        break;
                    }
            }

            // Did we signal we need an async content load processed?
            if (this._loadContentThreadResults.Count > 0)
            {
                LoadContentThreadArgs loadContentThreadArgs = null;
                if (this._loadContentThreadResults.TryDequeue(out loadContentThreadArgs))
                {
                    // Load in new content from staged collection in args
                    ProcessLoadContentAsync(loadContentThreadArgs);

                    // Did anyone need to know we finished?
                    if (LoadContentAsyncFinished != null)
                    {
                        var eventArgs = new LoadContentAsyncFinishedEventArgs(
                            loadContentThreadArgs.TheLoadContentAsyncType,
                            loadContentThreadArgs.TheGameState);
                        LoadContentAsyncFinished(this, eventArgs);
                    }
                }
            }

        }

        #endregion

        #region ICharacterCache Implementation

        public event LoadContentAsyncFinishedEventHandler LoadContentAsyncFinished;

        public void Draw(StockBasicEffect effect = null, EffectType type = EffectType.None)
        {
            var view = this._inputManager.CurrentCamera.ViewMatrix;
            var projection = this._inputManager.CurrentCamera.ProjectionMatrix;

            // Since the hero almost always gets drawn we can skip the filter here
            /*
            BoundingFrustum frustrum = new BoundingFrustum(view * projection);
            _ocTreeRoot.Draw(view, projection, frustrum, effect, type);
            */
            if (this.TheHeroModel != null)
            {
                this.TheHeroModel.Draw(view, projection, effect, type);
            }
        }

        public HeroModel TheHeroModel { get; private set; }
        
        public bool TouchBegan(CCTouch touch)
        {
            if (this._paused)
            {
                return true;
            }

            // Only process touches (i.e., jumps), when moving
            if (this._currentGameState == GameState.Moving)
            {
                // Depending on line we are on, set our jump direction
                // Odd => moving to right
                // Even => moving to left
                if (this._currentLineNumber % 2 != 0)
                {
                    this.TheHeroModel.Jump(HeroModel.JumpDirection.UpperRight);
                }
                else
                {
                    this.TheHeroModel.Jump(HeroModel.JumpDirection.UpperLeft);
                }
            }

            return true;
        }

        public void SwitchState(GameState state)
        {
            // Update our overall game state
            this._currentGameState = state;

            switch (state)
            {
                case GameState.Intro:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.GameStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.GameStartLineNumber;

                        // In background/event handler when finished:
                        // 1. Get initial characters constructed for first page
                        // 2. Attempt to suspend physics as best as possible
                        // 3. Position hero at defined start position
                        // 4. Will include logic to handle admin setting of line number other than 1
                        // 5. CreateLineHitParticles holder to keep hero in start location
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Initialize, state);

                        break;
                    }
                case GameState.Moving:
                    {
                        this.ResumeHeroPhysics();

                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        // Set state
                        this._currentLineNumber++;

                        // Attempt to suspend physics as best as possible
                        this.SuspendHeroPhysics();
                        this.RemoveHeroPhysicsConstraints();

                        // Rotate model into position for next line
                        // IMPORTANT: The position of the model and physics entity will be taken care of by the FlyBy
                        // See ActionLayer.SwitchState for GameState.MovingToNextLine for details.
                        // IMPORTANT: The orientation of the physics entity will be taken care of by RotateTo()
                        float rotateToYaw1 = 90f;
                        float rotateToYaw2 = 0f;
                        float rotateToRoll1 = 45f;
                        if (this._currentLineNumber % 2 == 0)
                        {
                            rotateToYaw1 = -90f;
                            rotateToYaw2 = -180f;
                            rotateToRoll1 = -45f;
                        }
                        var rotateToAction1 = new RotateTo(
                            (GameConstants.DURATION_MOVE_TO_NEXT_LINE/2f), 
                            rotateToYaw1,
                            0f,
                            rotateToRoll1);
                        var rotateToAction2 = new RotateTo(
                            (GameConstants.DURATION_MOVE_TO_NEXT_LINE/2f),
                            rotateToYaw2,
                            0f,
                            0f);
                        var rotateAction = new Sequence(new FiniteTimeAction[] 
                            { 
                                rotateToAction1, 
                                rotateToAction2
                            });
                        this.TheHeroModel.ModelRunAction(rotateAction);

                        // In background load up the line following our current line we are moving to
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Next, state);

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Attempt to suspend physics as best as possible
                        this.SuspendHeroPhysics();
                        this.RemoveHeroPhysicsConstraints();

                        // In background load up the line following our current line we are moving to
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Next, state);

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.GameStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.GameStartLineNumber;

                        // In background load up initial characters for first page
                        // See HandleKill() for additional steps taken for this state
                        // that have to be implemented after we animate the kill.
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Initialize, state);

                        break;
                    }
                case GameState.Refresh:
                    {
                        // We need to set this up-front as refresh is done on a background thread
                        // and we need to know this to short-circuit draw while this is done
                        this._currentGameState = state;

                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.GameStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.GameStartLineNumber;

                        // Get characters constructed for first page
                        // this.ProcessNextPage();

                        // Attempt to suspend physics as best as possible
                        // this.SuspendHeroPhysics();
                        // this.RemoveHeroPhysicsConstraints();

                        // Position hero at defined start position
                        // Will include logic to handle admin setting of line number other than 1
                        // this.InitHeroPosition();

                        // CreateLineHitParticles holder to keep hero in start location
                        // this.RemoveHolderForHero();
                        // this.CreateHolderForHero();

                        // Turn this on so hero falls into place
                        // this.TheHeroModel.PhysicsEntity.IsAffectedByGravity = true;

                        // In background load up the line following our current line we are moving to
                        this.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;
                        this.LoadContentAsync(LoadContentAsyncType.Refresh, state);

                        break;
                    }
                case GameState.Start:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.GameStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.GameStartLineNumber;

                        // Attempt to suspend physics as best as possible
                        this.SuspendHeroPhysics();
                        this.ApplyHeroPhysicsConstraints();

                        // Position hero at defined start position
                        // Will include logic to handle admin setting of line number other than 1
                        this.InitHeroPosition();

                        // CreateLineHitParticles holder to keep hero in start location
                        this.CreateHolderForHero();

                        // Turn this on so hero falls into place
                        if (this.TheHeroModel != null)
                        {
                            this.TheHeroModel.PhysicsEntity.IsAffectedByGravity = true;
                        }

                        break;
                    }
            }
        }

        public void HandleKill(Contact theContact, System.Action handleMoveToFinish)
        {
            // Bounce back
            /*
            this.TheHeroModel.PhysicsEntity.LinearVelocity = BEPUutilities.Vector3.Zero;
            this.TheHeroModel.PhysicsEntity.ApplyLinearImpulse(ref theContact.Normal);
            */

            // Turn off physics constraints for a more realistic kill animation
            this.RemoveHeroPhysicsConstraints();

            // Allow a slight duration to let physics animate to ground
            var delayAction = new DelayTime(GameConstants.DURATION_HERO_KILL);
            var finishAction = new CallFunc(() =>
                {
                    // Attempt to suspend physics as best as possible
                    this.SuspendHeroPhysics();

                    // Position hero at defined start position
                    // Will include logic to handle admin setting of line number other than 1
                    this.InitHeroPosition();

                    // CreateLineHitParticles restraints to keep hero in start position
                    this.CreateHolderForHero();

                    // Let our callback determine where to navigate to next
                    handleMoveToFinish();
                });

            var heroKilledAction = new Sequence(new FiniteTimeAction[] 
                { 
                    delayAction, 
                    finishAction
                });

            this.TheHeroModel.ModelRunAction(heroKilledAction);
        }

        public void Pause(bool pause)
        {
            this._paused = pause;

            if (pause)
            {
                this.SuspendHeroPhysics();
            }
            else
            {
                this.ResumeHeroPhysics();
            }
        }

        public float GetLinearVelocityX()
        {
            return this.TheHeroModel.LinearVelocityX;
        }

        public void IncreaseVelocity()
        {
            // Update our state
            this.TheHeroModel.LinearVelocityX += 0.1f;

            // Store away the new value
            UserDefaults.SharedUserDefault.SetFloatForKey(
                GameConstants.USER_DEFAULT_KEY_HERO_LINEAR_VELOCITY_X, 
                this.TheHeroModel.LinearVelocityX);
            
            // Depending on line we are on, increase our velocity
            // Odd => moving to right
            // Even => moving to left
            if (this._currentLineNumber % 2 != 0)
            {
                this.TheHeroModel.PhysicsEntity.LinearVelocity =
                    new BEPUutilities.Vector3(
                        this.TheHeroModel.LinearVelocityX, 
                        0f, 
                        0f);
            }
            else
            {
                this.TheHeroModel.PhysicsEntity.LinearVelocity =
                    new BEPUutilities.Vector3(
                        -this.TheHeroModel.LinearVelocityX,
                        0f,
                        0f);
            }
        }

        public bool DecreaseVelocity()
        {
            // Update our state with sanity check for lowest speed and
            // short circuit with failure if trying to decrease below lower limit
            this.TheHeroModel.LinearVelocityX -= 0.01f;
            if (this.TheHeroModel.LinearVelocityX < GameConstants.VELOCITY_LOWER_LIMIT_X)
            {
                this.TheHeroModel.LinearVelocityX = GameConstants.VELOCITY_LOWER_LIMIT_X;
                return false;
            }

            // Store away the new value
            UserDefaults.SharedUserDefault.SetFloatForKey(
                GameConstants.USER_DEFAULT_KEY_HERO_LINEAR_VELOCITY_X,
                this.TheHeroModel.LinearVelocityX);

            // Depending on line we are on, increase our velocity
            // Odd => moving to right
            // Even => moving to left
            if (this._currentLineNumber % 2 != 0)
            {
                this.TheHeroModel.PhysicsEntity.LinearVelocity =
                    new BEPUutilities.Vector3(
                        this.TheHeroModel.LinearVelocityX,
                        0f,
                        0f);
            }
            else
            {
                this.TheHeroModel.PhysicsEntity.LinearVelocity =
                    new BEPUutilities.Vector3(
                        -this.TheHeroModel.LinearVelocityX,
                        0f,
                        0f);
            }

            // Signal success
            return true;
        }

        #endregion

        #region Event Handlers

        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            this.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;

            if (args.TheLoadContentAsyncType == LoadContentAsyncType.Initialize ||
                args.TheLoadContentAsyncType == LoadContentAsyncType.Refresh)
            {
                // Attempt to suspend physics as best as possible
                this.SuspendHeroPhysics();

                // Position hero at defined start position
                // Will include logic to handle admin setting of line number other than 1
                this.InitHeroPosition();

                // CreateLineHitParticles holder to keep hero in start location
                this.CreateHolderForHero();

                // TODO: Once we have more characters besides hero
                // this.ProcessNextLine();
            }
        }

        #endregion

        #region Helper methods

        private void LoadContentAsync(LoadContentAsyncType loadContentAsyncType, GameState gameState)
        {
            // Determine which page/line number we are loading
            var pageNumber = -1;
            int[] lineNumbers = null;
            switch (loadContentAsyncType)
            {
                case LoadContentAsyncType.Initialize:
                case LoadContentAsyncType.Refresh:
                    {
                        pageNumber = 1;
                        lineNumbers = new int[] { 1, 2 };
                        break;
                    }
                case LoadContentAsyncType.Next:
                    {
                        // Are we on the last line of the page
                        var lineCount = this._pageCache.CurrentPageModel.ThePadEntity.LineCount;
                        if (this._currentLineNumber == lineCount)
                        {
                            // Ok, on last line, move to first line of next page
                            pageNumber = this._currentPageNumber + 1;
                            lineNumbers = new int[] { 1 };
                        }
                        else
                        {
                            // Not on last line, just go for next line
                            pageNumber = this._currentPageNumber;
                            lineNumbers = new int[] { this._currentLineNumber + 1 };
                        }
                        break;
                    }
            }

            // Build our state object for this background content load request
            var loadContentThreadArgs = new LoadContentThreadArgs
            {
                TheLoadContentAsyncType = loadContentAsyncType,
                TheGameState = gameState,
                PageNumber = pageNumber,
                LineNumbers = lineNumbers,
                CharacterModelsAsync = new List<CharacterModel>()
            };

#if NETFX_CORE

            IAsyncAction asyncAction = 
                Windows.System.Threading.ThreadPool.RunAsync(
                    (workItem) =>
                    {
                        LoadContentAsyncThread(loadContentThreadArgs);
                    });
#else
            ThreadPool.QueueUserWorkItem(LoadContentAsyncThread, loadContentThreadArgs);
#endif
        }

        private void LoadContentAsyncThread(object args)
        {
            var loadContentThreadArgs = args as LoadContentThreadArgs;
            var loadContentAsyncType = loadContentThreadArgs.TheLoadContentAsyncType;
            var pageNumber = loadContentThreadArgs.PageNumber;
            var lineNumbers = loadContentThreadArgs.LineNumbers;
            var characterModels = loadContentThreadArgs.CharacterModelsAsync;

            // Grab our collection of characters for current page
            var pageCharactersEntities = this._pageCharactersRepository.GetCharacters(pageNumber, lineNumbers);
            foreach (var pageCharactersEntity in pageCharactersEntities)
            {
                // Determine what type of model to construct
                CharacterModel characterModel = null;
                CharacterEntity characterEntity = null;
                var characterType = (CharacterType)Enum.Parse(typeof(CharacterType), pageCharactersEntity.CharacterType);
                if (characterType == CharacterType.Hero)
                {
                    // Only create the hero once
                    if (this.TheHeroModel != null)
                    {
                        continue;
                    }

                    characterEntity = this._characterRepository.GetCharacter(pageCharactersEntity.ModelName);
                    // this.TheHeroModel = new HeroModel(characterEntity, pageCharactersEntity);
                    // characterModel = this.TheHeroModel;   // Just to scale across multiple character types
                    characterModel = new HeroModel(
                        characterEntity, 
                        pageCharactersEntity,
                        this._heroCustomContentManager);
                }
                else
                {
                    // TODO: Implement new model types here when we have additional characters
                    continue;
                }

                // Now get our new world dimensions
                var scaleMatrix = Matrix.CreateScale(this._pageCache.CurrentPageModel.ModelToWorldRatio);
                Vector3 scale = new Vector3();
                Quaternion rot = new Quaternion();
                Vector3 trans = new Vector3();
                scaleMatrix.Decompose(out scale, out rot, out trans);
                characterModel.ModelToWorldRatio = scale.X;
                characterModel.WorldWidth = characterModel.TheModelEntity.ModelWidth * scale.X;
                characterModel.WorldHeight = characterModel.TheModelEntity.ModelHeight * scale.Y;
                characterModel.WorldDepth = characterModel.TheModelEntity.ModelDepth * scale.Z;

                // Get a depth that will position obstacle exactly
                // straddling the halfway depth of the line
                var lineModel = this._lineCache.GetLineModel(_currentLineNumber);
                var halfwayDepth = -this._pageCache.PageDepthFromCameraStart +      // Start back at page depth
                                   (0.5f * lineModel.WorldDepth) +                  // Move forward to line depth halfway mark
                                   (0.5f * characterModel.WorldDepth);              // Add in 1/2 of obstacle depth so obstacle
                // will be positioned exactly straddling line deph halfway
                // Adjust HeroStartOrigin to this new depth
                this._pageCache.CurrentPageModel.HeroStartOrigin = new Vector3(
                    this._pageCache.CurrentPageModel.HeroStartOrigin.X,
                    this._pageCache.CurrentPageModel.HeroStartOrigin.Y,
                    halfwayDepth);

                // To determine origin of character model
                var translateMatrix = Matrix.CreateTranslation(this._pageCache.CurrentPageModel.HeroStartOrigin);

                // Translate and scale our model
                characterModel.WorldMatrix = scaleMatrix * translateMatrix;

                // Uniquely identity character for octree
                characterModel.ModelID = pageCharactersEntity.CharacterNumber;

                // Record to our staged collection
                characterModels.Add(characterModel);

                // Now scale our physics body for this obstacle model
                // TODO: Once we refactor a CharacterModel in place, take out this cast
                var heroModel = (HeroModel)characterModel;

                // IMPORTANT: Applying defined depth here as we are in code
                // that has determined we have the hero model
                heroModel.DefinedDepthFromCamera = halfwayDepth;

                // Scale our pre-defined points that represent the outer shape of the hero
                int pointCounter = 0;
                foreach (var point in heroModel.ConvexHullPoints.ToList())
                {
                    heroModel.ConvexHullPoints[pointCounter] *= ConversionHelper.MathConverter.Convert(scale);
                    pointCounter++;
                }

                // Simplistically create the physics convex hull 
                // IMPORTANT: Don't position yet
                // IMPORTANT: Note how LocalInertiaTensorInverse is set to restrict rotation
                var physicsMesh = new ConvexHull(heroModel.ConvexHullPoints, 1);
                // physicsMesh.LocalInertiaTensorInverse = new BEPUutilities.Matrix3x3(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
                physicsMesh.Material.Bounciness = 10f;


                // Now determine an appropriate transform to use when positioning the ui representation of the hero
                var physicsLocalTransform = (physicsMesh.Position + ConversionHelper.MathConverter.Convert(characterModel.WorldMatrix.Translation)) -
                                            ConversionHelper.MathConverter.Convert(heroModel.WorldMatrix.Translation);
                heroModel.PhysicsLocalTransform = BEPUutilities.Matrix.CreateTranslation(-physicsLocalTransform);

                // And here we can finally set the position of the physics mesh
                physicsMesh.Position = ConversionHelper.MathConverter.Convert(heroModel.WorldMatrix.Translation);

                // Continue as normal for physics mesh setup
                heroModel.PhysicsEntity = physicsMesh;
                physicsMesh.Tag = characterModel;
                // this._physicsManager.TheSpace.Add(physicsMesh);

                // We can now assign our property for the hero which multiple helper functions
                // will check for null against to make sure it is complete and ready to go
                this.TheHeroModel = heroModel;
            }

            this._loadContentThreadResults.Enqueue(loadContentThreadArgs);
        }

        // Migrate staged collection in args to public collection
        private void ProcessLoadContentAsync(LoadContentThreadArgs loadContentThreadArgs)
        {
            // If we are moving to first/next page, clear out previous page
            if (this._currentLineNumber == 1)
            {
                // Remove all previous models from our drawing filter and physics world
                // EXCEPT for hero
                foreach (var characterModel in this.CharacterModels.ToList())
                {
                    if (characterModel == this.TheHeroModel)
                    {
                        continue;
                    }
                    this._ocTreeRoot.RemoveModel(characterModel.ModelID);

                    if (characterModel.PhysicsEntity != null &&
                        characterModel.PhysicsEntity.Space != null)
                    {
                        this._physicsManager.TheSpace.Remove(characterModel.PhysicsEntity);
                    }
                }

                // Now recreate our character collection with only the hero in place
                // if we have a hero
                this.CharacterModels.Clear();
                if (this.TheHeroModel != null)
                {
                    this.CharacterModels.Add(this.TheHeroModel);
                }
            }

            // Populate our public collection from our staged collection
            foreach (var characterModel in loadContentThreadArgs.CharacterModelsAsync)
            {
                // Add to octree for filtered drawing
                this._ocTreeRoot.AddModel(characterModel);

                this.CharacterModels.Add(characterModel);

                if (characterModel.PhysicsEntity != null)
                {
                    this._physicsManager.TheSpace.Add(characterModel.PhysicsEntity);
                }

                if (characterModel == this.TheHeroModel)
                {
                    // Apply limits on hero to keep in xy plane and limit
                    // rotation in xy plane
                    this.ApplyHeroPhysicsConstraints();

                    this.SuspendHeroPhysics();
                }
            }
        }

        // Centralized handling to apply all physics to hero
        private void ResumeHeroPhysics()
        {
            // If necessary, remove restraints holding hero in start position
            this.RemoveHolderForHero();

            // Restore physics for hero
            this.TheHeroModel.PhysicsEntity.IsAffectedByGravity = true;
            this.ApplyHeroPhysicsConstraints();

            // Depending on line we are on, set our velocity
            // Odd => moving to right
            // Even => moving to left
            if (this._currentLineNumber % 2 != 0)
            {
                this.TheHeroModel.PhysicsEntity.LinearVelocity = new BEPUutilities.Vector3(
                        this.TheHeroModel.LinearVelocityX,
                        0f,
                        0f);
            }
            else
            {
                this.TheHeroModel.PhysicsEntity.LinearVelocity = new BEPUutilities.Vector3(
                        -this.TheHeroModel.LinearVelocityX,
                        0f,
                        0f);
            }
        }

        // Attempt to suspend physics as best as possible
        private void SuspendHeroPhysics()
        {
            if (this.TheHeroModel == null)
            {
                return;
            }

            this.TheHeroModel.PhysicsEntity.LinearVelocity = BEPUutilities.Vector3.Zero;
            this.TheHeroModel.PhysicsEntity.AngularVelocity = BEPUutilities.Vector3.Zero;
            this.TheHeroModel.PhysicsEntity.IsAffectedByGravity = false;
        }

        // Apply limits on hero to keep in xy plane and limit
        // rotation in xy plane
        private void ApplyHeroPhysicsConstraints()
        {
            if (this.TheHeroModel == null)
            {
                return;
            }

            // Don't allow hero to rotate around X axis
            if (this._limitHeroRevoluteX == null)
            {
                this._limitHeroRevoluteX = new RevoluteLimit(
                    this.TheHeroModel.PhysicsEntity,
                    this._pageCache.CurrentPageModel.PhysicsEntity,
                    BEPUutilities.Vector3.Right,
                    BEPUutilities.Vector3.Up,
                    0,
                    0);
            }
            if (this._limitHeroRevoluteX.space == null)
            {
                this._physicsManager.TheSpace.Add(this._limitHeroRevoluteX);
            }

            // Don't allow hero to rotate around Y axis
            if (this._limitHeroRevoluteY == null)
            {
                this._limitHeroRevoluteY = new RevoluteLimit(
                    this.TheHeroModel.PhysicsEntity,
                    this._pageCache.CurrentPageModel.PhysicsEntity,
                    BEPUutilities.Vector3.Up,
                    BEPUutilities.Vector3.Right,
                    0,
                    0);
            }
            if (this._limitHeroRevoluteY.space == null)
            {
                this._physicsManager.TheSpace.Add(_limitHeroRevoluteY);
            }

            // Limit hero's rotation around Z axis
            if (this._limitHeroRevoluteZ == null)
            {
                this._limitHeroRevoluteZ = new RevoluteLimit(
                    this.TheHeroModel.PhysicsEntity,
                    this._pageCache.CurrentPageModel.PhysicsEntity,
                    BEPUutilities.Vector3.Backward,
                    BEPUutilities.Vector3.Right,
                    0,
                    0);
            }
            if (this._currentLineNumber%2 == 0)
            {
                this._limitHeroRevoluteZ.MinimumAngle = MathHelper.PiOver4;
                this._limitHeroRevoluteZ.MaximumAngle = -MathHelper.PiOver4;
            }
            else
            {
                this._limitHeroRevoluteZ.MinimumAngle = -MathHelper.PiOver4;
                this._limitHeroRevoluteZ.MaximumAngle = MathHelper.PiOver4;
            }
            if (this._limitHeroRevoluteZ.space == null)
            {
                this._physicsManager.TheSpace.Add(_limitHeroRevoluteZ);
            }

            // Add in motor to return hero to foward looking position
            // IMPORTANT: Note how the target orientation is determine based on current line number
            if (this._heroRotator == null)
            {
                this._heroRotator = new EntityRotator(this.TheHeroModel.PhysicsEntity);
                this._heroRotator.AngularMotor.Settings.Servo.SpringSettings.Damping = 0.1f;
                this._heroRotator.AngularMotor.Settings.Servo.SpringSettings.Stiffness = 0.1f;
            }
            if (this._currentLineNumber % 2 == 0)
            {
                this._heroRotator.TargetOrientation = BEPUutilities.Quaternion.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0);
            }
            else
            {
                this._heroRotator.TargetOrientation = BEPUutilities.Quaternion.CreateFromYawPitchRoll(0, 0, 0);
            }
            if (this._heroRotator.Space == null)
            {
                this._physicsManager.TheSpace.Add(this._heroRotator);
            }
        }

        // Remove physics constraints that kept hero in xy plane and limited
        // rotation in xy plane
        private void RemoveHeroPhysicsConstraints()
        {
            if (this._limitHeroRevoluteX.space != null)
            {
                this._physicsManager.TheSpace.Remove(_limitHeroRevoluteX);
            }
            if (this._limitHeroRevoluteY.space != null)
            {
                this._physicsManager.TheSpace.Remove(_limitHeroRevoluteY);
            }
            if (this._limitHeroRevoluteZ.space != null)
            {
                this._physicsManager.TheSpace.Remove(_limitHeroRevoluteZ);
            }

            if (this._heroRotator.Space != null)
            {
                this._physicsManager.TheSpace.Remove(this._heroRotator);
            }
        }

        private void InitHeroPosition()
        {
            if (this.TheHeroModel == null)
            {
                return;
            }

            // Our standard starting position
            var heroStartOrigin = this._pageCache.CurrentPageModel.HeroStartOrigin;
            var physicsRotation = Quaternion.CreateFromYawPitchRoll(
                        MathHelper.ToRadians(0f), 
                        MathHelper.ToRadians(0f), 
                        MathHelper.ToRadians(0f));
            this.TheHeroModel.PhysicsEntity.Orientation = ConversionHelper.MathConverter.Convert(physicsRotation);

            // Do we need to adjust for admin setting of line number?
            var adminLineNumber = GameManager.SharedGameManager.GameStartLineNumber;
            if (adminLineNumber > 1)
            {
                heroStartOrigin -= new Vector3(
                    0,
                    (adminLineNumber - 1) * this._pageCache.CurrentPageModel.WorldLineSpacing,
                    0);

                // Do we need to adjust for starting on an even line number?
                if (adminLineNumber % 2 == 0)
                {
                    // Move to other side
                    heroStartOrigin += new Vector3(
                        this._pageCache.CurrentPageModel.WorldWidth - 1,
                        0,
                        0);

                    // Flip hero
                    var flipRotation = Quaternion.CreateFromYawPitchRoll(
                        MathHelper.ToRadians(-180f), 
                        MathHelper.ToRadians(0f), 
                        MathHelper.ToRadians(0f));
                    this.TheHeroModel.PhysicsEntity.Orientation = ConversionHelper.MathConverter.Convert(flipRotation);
                }
            }

            // Adjust to physics center of object position
            heroStartOrigin -= ConversionHelper.MathConverter.Convert(this.TheHeroModel.PhysicsLocalTransform.Translation);

            // Now apply position with any adjustments accounted for
            this.TheHeroModel.PhysicsEntity.Position = ConversionHelper.MathConverter.Convert(heroStartOrigin);
        }

        // IMPORTANT: Assumes hero is positioned at desired start position.
        // (i.e. PageCache.CurrentPageModel.HeroStartOrigin adjusted by Admin setting for line number if necessary)
        private void CreateHolderForHero()
        {
            if (this.TheHeroModel == null)
            {
                return;
            }

            // Have we created holder for hero yet?
            if (this._holderForHero == null)
            {
                //
                // CreateLineHitParticles holder for hero as physics body with 3 edges
                // similar to a cup.
                //
                var bottomEdge = new BoxShape(
                    2f*this.TheHeroModel.WorldWidth,
                    0.1f,
                    4f);// 2f*this.TheHeroModel.WorldDepth);
                var leftEdge = new BoxShape(
                    0.1f,
                    2f*this.TheHeroModel.WorldHeight,
                    2f*this.TheHeroModel.WorldDepth);
                var rightEdge = new BoxShape(
                    0.1f,
                    2f*this.TheHeroModel.WorldHeight,
                    2f*this.TheHeroModel.WorldDepth);
                var bodies = new List<CompoundShapeEntry>
                {
                    new CompoundShapeEntry(bottomEdge, new BEPUutilities.Vector3(
                        0, 
                        0, 
                        0)),
                    new CompoundShapeEntry(leftEdge, new BEPUutilities.Vector3(
                        -this.TheHeroModel.WorldWidth,
                        this.TheHeroModel.WorldHeight,
                        0)),
                    new CompoundShapeEntry(rightEdge, new BEPUutilities.Vector3(
                        this.TheHeroModel.WorldWidth,
                        this.TheHeroModel.WorldHeight,
                        0)),
                };

                this._holderForHero = new CompoundBody(bodies);
                this._physicsManager.TheSpace.Add(this._holderForHero);
            }

            // Our standard starting position
            var heroStartOrigin = this._pageCache.CurrentPageModel.HeroStartOrigin;

            // Do we need to adjust for admin setting of line number?
            if (this._currentLineNumber > 1)
            {
                heroStartOrigin -= new Vector3(
                    0,
                    (_currentLineNumber - 1) * this._pageCache.CurrentPageModel.WorldLineSpacing,
                    0);

                // Do we need to adjust for starting on an even line number?
                if (this._currentLineNumber % 2 == 0)
                {
                    // Move to other side
                    heroStartOrigin += new Vector3(
                        this._pageCache.CurrentPageModel.WorldWidth - 1,
                        0,
                        0);
                }
            }

            // Adjust to physics center of object position
            heroStartOrigin -= ConversionHelper.MathConverter.Convert(this.TheHeroModel.PhysicsLocalTransform.Translation);

            // Adjust holder position to be below hero
            var holderPosition = ConversionHelper.MathConverter.Convert(heroStartOrigin)  +
                new BEPUutilities.Vector3(
                    0, 
                    -this.TheHeroModel.WorldHeight, 
                    0);

            // Now apply position with any adjustments accounted for
            this._holderForHero.Position = holderPosition;
        }

        private void RemoveHolderForHero()
        {
            if (this._holderForHero != null &&
                this._holderForHero.Space != null)
            {
                this._physicsManager.TheSpace.Remove(this._holderForHero);
                this._holderForHero = null;
            }
        }

        #endregion

    }
}