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


namespace Simsip.LineRunner.GameObjects.Characters
{
    public class CharacterCache : DrawableGameComponent, ICharacterCache
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

        #region DrawableGameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Initialize state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = 1;
            this.CharacterModels = new List<CharacterModel>();

            // Import required services.
            this._inputManager = (IInputManager)this.Game.Services.GetService(typeof(IInputManager));
            this._pageCache = (IPageCache)this.Game.Services.GetService(typeof(IPageCache));
            this._lineCache = (ILineCache)this.Game.Services.GetService(typeof(ILineCache));
            this._physicsManager = (IPhysicsManager)this.Game.Services.GetService(typeof(IPhysicsManager));
            this._pageCharactersRepository = new PageCharactersRepository();
            this._characterRepository = new CharacterRepository();

            // Initialize our drawing filter
            this._ocTreeRoot = new OcTreeNode(new Vector3(0, 0, 0), OcTreeNode.DefaultSize);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Currently a no-op
        }

        public override void Update(GameTime gameTime)
        {
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
        }

        public override void Draw(GameTime gameTime)
        {
            this.Draw();
        }

        #endregion

        #region ICharacterCache Implementation

        public void Draw(StockBasicEffect effect = null, EffectType type = EffectType.None)
        {
            // We need to short-circuit when we are in Refresh state they are reinitializing this game object's
            // state on a background thread. Note that the Update() short circuit for Refresh is handled
            // in the ActionLayer.Update.
            if (this._currentGameState == GameState.Refresh)
            {
                return;
            }

            var view = this._inputManager.CurrentCamera.ViewMatrix;
            var projection = this._inputManager.CurrentCamera.ProjectionMatrix;

            // Since the hero almost always gets drawn we can skip the filter here
            /*
            BoundingFrustum frustrum = new BoundingFrustum(view * projection);
            _ocTreeRoot.Draw(view, projection, frustrum, effect, type);
            */
            this.TheHeroModel.Draw(view, projection, effect, type);
        }

        public HeroModel TheHeroModel { get; private set; }
        
        public bool TouchBegan(CCTouch touch)
        {
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
            switch (state)
            {
                case GameState.Intro:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Get characters constructed for first page
                        this.ProcessNextPage();

                        // Attempt to suspend physics as best as possible
                        this.SuspendHeroPhysics();

                        // Position hero at defined start position
                        // Will include logic to handle admin setting of line number other than 1
                        this.InitHeroPosition();

                        // Create holder to keep hero in start location
                        this.CreateHolderForHero();

                        break;
                    }
                case GameState.Moving:
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
                            this.TheHeroModel.PhysicsEntity.LinearVelocity = this.TheHeroModel.DEFAULT_VELOCITY;
                        }
                        else
                        {
                            this.TheHeroModel.PhysicsEntity.LinearVelocity = this.TheHeroModel.OPPOSITE_DEFAULT_VELOCITY;
                        }

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

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Get characters constructed for next page
                        this.ProcessNextPage();

                        // Attempt to suspend physics as best as possible
                        this.SuspendHeroPhysics();
                        this.RemoveHeroPhysicsConstraints();

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Get characters constructed for first page
                        this.ProcessNextPage();

                        // See HandleKill() for additional steps taken for this state
                        // that have to be implemented after we animate the kill.

                        break;
                    }
                case GameState.Refresh:
                    {
                        // We need to set this up-front as refresh is done on a background thread
                        // and we need to know this to short-circuit draw while this is done
                        this._currentGameState = state;

                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Get characters constructed for first page
                        this.ProcessNextPage();

                        // Attempt to suspend physics as best as possible
                        this.SuspendHeroPhysics();
                        this.RemoveHeroPhysicsConstraints();

                        // Position hero at defined start position
                        // Will include logic to handle admin setting of line number other than 1
                        this.InitHeroPosition();

                        // Create holder to keep hero in start location
                        this.RemoveHolderForHero();
                        this.CreateHolderForHero();

                        // Turn this on so hero falls into place
                        this.TheHeroModel.PhysicsEntity.IsAffectedByGravity = true;

                        // Migrate to the start game state
                        state = GameState.Start;

                        break;
                    }
                case GameState.Start:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Attempt to suspend physics as best as possible
                        this.SuspendHeroPhysics();
                        this.ApplyHeroPhysicsConstraints();

                        // Position hero at defined start position
                        // Will include logic to handle admin setting of line number other than 1
                        this.InitHeroPosition();

                        // Create holder to keep hero in start location
                        this.CreateHolderForHero();

                        // Turn this on so hero falls into place
                        this.TheHeroModel.PhysicsEntity.IsAffectedByGravity = true;

                        break;
                    }
            }

            // Ok, now set our current state
            this._currentGameState = state;
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

                    // Create restraints to keep hero in start position
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

        #endregion

        #region Helper methods

        private void ProcessNextPage()
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

            // Grab our collection of characters for current page
            var pageCharactersEntities = this._pageCharactersRepository.GetCharacters(this._currentPageNumber);
            foreach (var pageCharactersEntity in pageCharactersEntities)
            {
                // Determine what tpe of model to construct
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
                    this.TheHeroModel = new HeroModel(characterEntity, pageCharactersEntity);
                    characterModel = this.TheHeroModel;   // Just to scale across multiple character types
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

                // And add to octree for filtered drawing
                this._ocTreeRoot.AddModel(characterModel);

                // Record to our collection
                this.CharacterModels.Add(characterModel);

                // Now scale our physics body for this obstacle model
                // TODO: Once we refactor a CharacterModel in place, take out this cast
                var heroModel = (HeroModel)characterModel;

                // IMPORTANT: Applying defined depth here as we are in code
                // that has determined we have the hero model
                heroModel.DefinedDepthFromCamera = halfwayDepth;

                // Scale our pre-defined points that represent the outer shape of the hero
                int pointCounter = 0;
                foreach(var point in heroModel.ConvexHullPoints.ToList())
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
                var physicsLocalTransform = (physicsMesh.Position +  ConversionHelper.MathConverter.Convert(characterModel.WorldMatrix.Translation)) -
                                            ConversionHelper.MathConverter.Convert(characterModel.WorldMatrix.Translation);
                characterModel.PhysicsLocalTransform = BEPUutilities.Matrix.CreateTranslation(-physicsLocalTransform);

                // And here we can finally set the position of the physics mesh
                physicsMesh.Position = ConversionHelper.MathConverter.Convert(characterModel.WorldMatrix.Translation);

                // Continue as normal for physics mesh setup
                characterModel.PhysicsEntity = physicsMesh;
                physicsMesh.Tag = characterModel;
                this._physicsManager.TheSpace.Add(physicsMesh);

                // Apply limits on hero to keep in xy plane and limit
                // rotation in xy plane
                this.ApplyHeroPhysicsConstraints();

                this.SuspendHeroPhysics();
            }
        }

        // Attempt to suspend physics as best as possible
        private void SuspendHeroPhysics()
        {
            this.TheHeroModel.PhysicsEntity.LinearVelocity = BEPUutilities.Vector3.Zero;
            this.TheHeroModel.PhysicsEntity.AngularVelocity = BEPUutilities.Vector3.Zero;
            this.TheHeroModel.PhysicsEntity.IsAffectedByGravity = false;
        }

        // Apply limits on hero to keep in xy plane and limit
        // rotation in xy plane
        private void ApplyHeroPhysicsConstraints()
        {
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

                    // Flip hero
                    var physicsRotation = Quaternion.CreateFromYawPitchRoll(
                        MathHelper.ToRadians(-180f), 
                        MathHelper.ToRadians(0), 
                        MathHelper.ToRadians(0));
                    this.TheHeroModel.PhysicsEntity.Orientation = ConversionHelper.MathConverter.Convert(physicsRotation);
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
            // Have we created holder for hero yet?
            if (this._holderForHero == null)
            {
                //
                // Create holder for hero as physics body with 3 edges
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