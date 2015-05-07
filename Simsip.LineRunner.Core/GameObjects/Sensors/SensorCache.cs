using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using ConversionHelper;
using Engine.Common.Logging;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Simsip.LineRunner.GameObjects.Sensors
{
    public delegate void SensorHitEventHandler(object sender, SensorHitEventArgs e);

    public class SensorHitEventArgs : EventArgs
    {
        private SensorModel _sensorModel;

        public SensorHitEventArgs(SensorModel sensorModel)
        {
            this._sensorModel = sensorModel;
        }

        public SensorModel TheSensorModel
        {
            get { return this._sensorModel; }
        }
    }

    public class SensorCache : GameComponent, ISensorCache
    {
        // Required services
        private IPhysicsManager _physicsManager;
        private IPageCache _pageCache;
        private ILineCache _lineCache;
        private IObstacleCache _obstacleCache;

        // State we maintain
        private int _currentPageNumber;
        private int _currentLineNumber;
        private GameState _currentGameState;
        private IList<SensorModel> _sensorModels;
        private IList<SensorModel> _sensorHitList;

        // Event handling support - let other game objects know when a sensor 
        // is hit (e.g., ActionLayer)
        public event SensorHitEventHandler SensorHit;

        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public SensorCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(ISensorCache), this); 
        }

        #region GameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Initialize state
            this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
            this._currentLineNumber = 1;
            this._sensorModels = new List<SensorModel>();
            this._sensorHitList = new List<SensorModel>();

            // Import required services.
            this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));
            this._lineCache = (ILineCache)TheGame.SharedGame.Services.GetService(typeof(ILineCache));
            this._obstacleCache = (IObstacleCache)TheGame.SharedGame.Services.GetService(typeof(IObstacleCache));
            this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // Loop over sensor hit list
            foreach (var sensor in this._sensorHitList.ToList())
            {
                // Emit event based on each sensor
                var args = new SensorHitEventArgs(sensor);
                if (SensorHit != null)
                {
                    // Careful of race condition here where we hit margin, which causes callback
                    // to ProcessNextLine below, which in turn clears out collections
                    SensorHit(this, args);
                }

                // Clear sensor from world if SensorModel says so
                if (sensor.RemoveAfterUpdate)
                {
                    if (sensor.PhysicsEntity != null &&
                        sensor.PhysicsEntity.Space != null)
                    {
                        _physicsManager.TheSpace.Remove(sensor.PhysicsEntity);
                    }
                }
            }

			// Clear sensor hit list after finishing loop
            this._sensorHitList.Clear();
        }

        #endregion

        #region ISensorCache Implementation

        public void SwitchState(GameState state)
        {
            // Update our overall game state
            this._currentGameState = state;

            switch (this._currentGameState)
            {
                case GameState.Intro:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Set up first page's first line's sensors
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.Moving:
                    {
                        // Currently a no-op
                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        // Set state
                        this._currentLineNumber++;

                        // Set up next line's sensors
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Set up next page's first line's sensors
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Set up first page's first line's sensors
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.Refresh:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        // Set up first page's first line's sensors
                        this.ProcessNextLine();

                        break;
                    }
                case GameState.Start:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.AdminStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.AdminStartLineNumber;

                        break;
                    }
            }
        }

        // Legacy
        public void AddSensorHit(SensorModel sensor)
        {
            if (!_sensorHitList.Contains(sensor))
            {
                _sensorHitList.Add(sensor);
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Used to handle a collision event triggered by an entity specified above.
        /// </summary>
        /// <param name="sender">Entity that had an event hooked.</param>
        /// <param name="other">Entity causing the event to be triggered.</param>
        /// <param name="pair">Collision pair between the two objects in the event.</param>
        private void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            // This type of event can occur when an entity hits any other object which can be collided with.
            // They aren't always entities; for example, hitting a StaticMesh would trigger this.
            // Entities use EntityCollidables as collision proxies; see if the thing we hit is one.
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation == null)
            {
                return;
            }

            var otherModel = otherEntityInformation.Entity.Tag as HeroModel;
            if (otherModel == null)
            {
                return;
            }

            // TODO: Flag to action layer we need to move to reset game
            //       after update/draw cycle has completed
            //We hit an entity! remove it.
            var sensor = sender.Entity.Tag as SensorModel;
            if (sensor != null &&
                !_sensorHitList.Contains(sensor))
            {
                _sensorHitList.Add(sensor);
            }
        }

        private void ProcessNextLine()
        {
            // Clear previous sensor collections if necessary
            if (_sensorModels.Count > 0)
            {
                // First clear from physics space
                foreach(var sensor in _sensorModels.ToList())
                {
                    if (sensor.PhysicsEntity != null &&
                        sensor.PhysicsEntity.Space != null)
                    {
                        _physicsManager.TheSpace.Remove(sensor.PhysicsEntity);
                    }
                }

                // Then clear our previous sensor collection
                _sensorModels.Clear();
            }
            if (_sensorHitList.Count > 0)
            {
                _sensorHitList.Clear();
            }

            // Which side of the line are we creating margin sensor on?
            var marginType = MarginSensorType.Right;
            if (this._currentLineNumber % 2 == 0)
            {
                // Even lines have sensors on left
                marginType = MarginSensorType.Left;
            }

            // Ok, proceed to build sensor
            var marginSensorModel = new MarginSensorModel(marginType);

            // Add new sensor to sensor collection
            this._sensorModels.Add(marginSensorModel);

            // What is the world origin for this margin sensor?
            // IMPORTANT: Adjusting for physics representation with origin in middle
            var lineModel = this._lineCache.GetLineModel(this._currentLineNumber);
            var marginWorldOrigin = new Vector3(
                lineModel.WorldOrigin.X - 0.1f,
                lineModel.WorldOrigin.Y + lineModel.WorldHeight +
                (0.5f*this._pageCache.CurrentPageModel.WorldLineSpacing),
                -this._pageCache.PageDepthFromCameraStart + (0.5f*lineModel.WorldDepth));   // IMPORTANT: We need to build up from page depth as line will be animated into place
                // lineModel.WorldOrigin.Z + (0.5f * lineModel.WorldDepth));
            if (marginSensorModel.TheMarginSensorType == MarginSensorType.Right)
            {
                marginWorldOrigin.X += lineModel.WorldWidth;
            }

            // Create physics box to represent margin and add to physics space
            var marginPhysicsBox = new Box(MathConverter.Convert(marginWorldOrigin),
                0.1f,
                this._pageCache.CurrentPageModel.WorldLineSpacing,
                lineModel.WorldDepth);
            marginSensorModel.PhysicsEntity = marginPhysicsBox;
            marginPhysicsBox.Tag = marginSensorModel;
            marginPhysicsBox.CollisionInformation.CollisionRules.Personal = CollisionRule.NoSolver;
            marginPhysicsBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
            this._physicsManager.TheSpace.Add(marginPhysicsBox);

            // Walk current obstacle collection to create goal sensors
            var currentLineObstacles = this._obstacleCache.ObstacleModels
                .Where(x => x.ThePageObstaclesEntity.LineNumber == this._currentLineNumber);
            foreach (var obstacleModel in currentLineObstacles)
            {
                // First, if we are not a goal obstacle, then just short-circuit
                // Example: We have top and bottom obstacles at same location - only one will be tagged as a goal.
                if (!obstacleModel.ThePageObstaclesEntity.IsGoal)
                {
                    continue;
                }

                // Ok, proceed to build our goal sensor
                var goalSensorModel = new GoalSensorModel();

                // Add new sensor to sensor collection
                _sensorModels.Add(goalSensorModel);

                // What is the world origin for this goal sensor?
                // IMPORTANT: Adjusting for physics representation with origin in middle
                // ALSO: Note how we determine X position for goal based on line
                // odd => moving right
                // even => moving left
                var adjustedGoalX = obstacleModel.WorldOrigin.X;
                if (this._currentLineNumber % 2 != 0)
                {
                    adjustedGoalX = obstacleModel.WorldOrigin.X + obstacleModel.WorldWidth;
                }
                var goalWorldOrigin = new Vector3(
                    adjustedGoalX - 0.1f,
                    lineModel.WorldOrigin.Y + lineModel.WorldHeight + (0.5f * this._pageCache.CurrentPageModel.WorldLineSpacing),
                    -this._pageCache.PageDepthFromCameraStart + (0.5f*lineModel.WorldDepth));   // IMPORTANT: We need to build up from page depth as line will be animated into place
                    // lineModel.WorldOrigin.Z + (0.5f * lineModel.WorldDepth));

                // Create physics box to represent goal and add to physics space
                var goalPhysicsBox = new Box(MathConverter.Convert(goalWorldOrigin),
                                   0.1f,
                                   this._pageCache.CurrentPageModel.WorldLineSpacing,
                                   lineModel.WorldDepth);
                goalSensorModel.PhysicsEntity = goalPhysicsBox;
                goalPhysicsBox.Tag = goalSensorModel;
                goalPhysicsBox.CollisionInformation.CollisionRules.Personal = CollisionRule.NoSolver;
                goalPhysicsBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
                this._physicsManager.TheSpace.Add(goalPhysicsBox);
            }
        }

        #endregion

    }
}