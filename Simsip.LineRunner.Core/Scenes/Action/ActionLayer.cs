using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysicsDrawer.Lines;
using Cocos2D;
using ConversionHelper;
using Engine.Assets;
using Engine.Chunks;
using Engine.Debugging;
using Engine.Debugging.Graphs;
using Engine.Debugging.Ingame;
using Engine.Graphics;
using Engine.Input;
using Engine.Interface;
using Engine.Sky;
using Engine.Universe;
using Engine.Water;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Actions;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.GameObjects.ParticleEffects;
using Simsip.LineRunner.GameObjects.Sensors;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Scenes.Hud;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using BEPUphysics.CollisionTests;
using Engine.Blocks;
using Engine.Audio;
using System.Diagnostics;
using Simsip.LineRunner.GameObjects;
using Simsip.LineRunner.Scenes.MessageBox;
using Simsip.LineRunner.Resources;
using BEPUphysicsDemos;
#if IOS
using Foundation;
#endif


namespace Simsip.LineRunner.Scenes.Action
{
    public class ActionLayer : GameLayer
    {
        // The parent scene to all the layers
        private CoreScene _parent;

        // Our current action state
        private GameState _currentGameState;

        // Our current line and page number we are on
        private int _currentLineNumber;
        private int _currentPageNumber;

        // Control our first time fly by upon startup
        private bool _firstTimeSetupPage;
        private FlyBy _currentFlyBy;

        // Allows us to create an appropriate time input to XNA game components
        // from our Cocos2d framework
        private float _totalGameTime;
        private GameTime _gameTime;

        // Required Voxeliq services
        private IAssetManager _assetManager;
        private ISkyDome _skyDome;
        private INewSky _newSky;
        private IChunkCache _chunkCache;
        private IInputManager _inputManager;
        private IWorld _world;
        private IWaterCache _waterCache;
        private IUserInterface _userInterface;
        private IInGameDebuggerService _inGameDebugger;
        private IStatistics _debugBar;
        private IGraphManager _graphManager;
        private IAudioManager _audioManager;
        
        // Required game services
        private IPhysicsManager _physicsManager;
        private IPageCache _pageCache;
        private ILineCache _lineCache;
        private IObstacleCache _obstacleCache;
        private ICharacterCache _characterCache;
        private IParticleEffectCache _particleCache;
        private ISensorCache _sensorCache;
        private IDeferredShadowMapping _deferredShadowMapping;

        // Support for communicating score to ui layer
        private HudLayer _hudLayer;

        // Manages periodic adjustments in update cycle
        private double _timeAccumulator = 0;

        // Allows us to gate kill logic so we only handle one kill at a time
        private bool _handlingKill;

        // Message box text to display when moving to new page
        string _movingToPageText;
        string _movingToPage2Text;
        string _movingToPage3Text;
        string _movingToPage4Text;
        string _movingToPage5Text;
        string _movingToPage6Text;


        public ActionLayer(CoreScene parent)
        {
            this._parent = parent;

            // Hook-up initial event handlers we'll need
            _parent.SwitchedUI += OnSwitchingUI;

            // Set state
            this._currentPageNumber = 1;
            this._currentLineNumber = 1;
            this._currentGameState = GameState.None;

            // Set text we may need
            this._movingToPageText = string.Empty;
#if ANDROID
            this._movingToPageText = Program.SharedProgram.Resources.GetString(Resource.String.MessageBoxMovingToPage);
#elif IOS
            this._movingToPageText = NSBundle.MainBundle.LocalizedString(Strings.MessageBoxMovingToPage, Strings.MessageBoxMovingToPage);
#else
            this._movingToPageText = AppResources.MessageBoxMovingToPage;
#endif
            this._movingToPage2Text = string.Empty;
#if ANDROID
            this._movingToPage2Text = Program.SharedProgram.Resources.GetString(Resource.String.MessageBoxMovingToPage2);
#elif IOS
            this._movingToPage2Text = NSBundle.MainBundle.LocalizedString(Strings.MessageBoxMovingToPage2, Strings.MessageBoxMovingToPage2);
#else
            this._movingToPage2Text = AppResources.MessageBoxMovingToPage2;
#endif
            this._movingToPage3Text = string.Empty;
#if ANDROID
            this._movingToPage3Text = Program.SharedProgram.Resources.GetString(Resource.String.MessageBoxMovingToPage3);
#elif IOS
            this._movingToPage3Text = NSBundle.MainBundle.LocalizedString(Strings.MessageBoxMovingToPage3, Strings.MessageBoxMovingToPage3);
#else
            this._movingToPage3Text = AppResources.MessageBoxMovingToPage3;
#endif
            this._movingToPage4Text = string.Empty;
#if ANDROID
            this._movingToPage4Text = Program.SharedProgram.Resources.GetString(Resource.String.MessageBoxMovingToPage4);
#elif IOS
            this._movingToPage4Text = NSBundle.MainBundle.LocalizedString(Strings.MessageBoxMovingToPage4, Strings.MessageBoxMovingToPage4);
#else
            this._movingToPage4Text = AppResources.MessageBoxMovingToPage4;
#endif
            this._movingToPage5Text = string.Empty;
#if ANDROID
            this._movingToPage5Text = Program.SharedProgram.Resources.GetString(Resource.String.MessageBoxMovingToPage5);
#elif IOS
            this._movingToPage5Text = NSBundle.MainBundle.LocalizedString(Strings.MessageBoxMovingToPage5, Strings.MessageBoxMovingToPage5);
#else
            this._movingToPage5Text = AppResources.MessageBoxMovingToPage5;
#endif
            this._movingToPage6Text = string.Empty;
#if ANDROID
            this._movingToPage6Text = Program.SharedProgram.Resources.GetString(Resource.String.MessageBoxMovingToPage6);
#elif IOS
            this._movingToPage6Text = NSBundle.MainBundle.LocalizedString(Strings.MessageBoxMovingToPage6, Strings.MessageBoxMovingToPage6);
#else
            this._movingToPage6Text = AppResources.MessageBoxMovingToPage6;
#endif

            // Dummy up an XNA gametime for us convert Cocos2d times into
            this._gameTime = new GameTime();

            // Schedule Update/Draw to be called
            this.ScheduleUpdate();

        }

        #region Properties

        /// <summary>
        /// We use this one-time flag to signal that the page layer has reached a ready state
        /// (e.g., for update/draw calls for instance)
        /// </summary>
        public bool Ready { get; private set; }

        #endregion

        #region Api

        public void SwitchState(GameState gameState)
        {
            // Ok, now set our current state
            this._currentGameState = gameState;

            switch (this._currentGameState)
            {
                case GameState.World:
                    {
                        var blockStorage = (IBlockStorage)TheGame.SharedGame.Services.GetService(typeof(IBlockStorage));

                        var footPosition = this._inputManager.LineRunnerCamera.Position +
                            new Vector3(20f, 0f, 0f);
                        var standingBlock = blockStorage.BlockAt(footPosition);
                        while (!standingBlock.Exists)
                        {
                            footPosition += new Vector3(0f, -1f, 0f);
                            standingBlock = blockStorage.BlockAt(footPosition);
                        }

                        this._inputManager.PlayerCamera.Position = footPosition;
                        this._inputManager.PlayerCamera.Target = Vector3.Forward;

                        break;
                    }
                case GameState.Intro:
                    {
                        this.TouchEnabled = false;

                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.GameStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.GameStartLineNumber;
                        GameManager.SharedGameManager.CurrentScore = GameManager.SharedGameManager.GameStartScore;

                        // Update page/line number in hud
                        this._hudLayer.DisplayPageLineNumber(this._currentPageNumber, this._currentLineNumber);

                        var pageWidth = this._pageCache.CurrentPageModel.WorldWidth;
                        var pageHeight = this._pageCache.CurrentPageModel.WorldHeight;
                        var pageDepthFromCamera = this._pageCache.PageDepthFromCameraStart;
                        var cameraStartingPoint = this._inputManager.LineRunnerCamera.Position;
                        var cameraTargetPoint = this._inputManager.LineRunnerCamera.Target;
                        var adminLineNumber = GameManager.SharedGameManager.GameStartLineNumber;
                        if (adminLineNumber > 1)
                        {
                            cameraStartingPoint -= new Vector3(
                                0,
                                (adminLineNumber - 1) * this._pageCache.CurrentPageModel.WorldLineSpacing,
                                0);
                            cameraTargetPoint -= new Vector3(
                                0,
                                (adminLineNumber - 1) * this._pageCache.CurrentPageModel.WorldLineSpacing,
                                0);
                        }

                        var controlPoints = new List<Vector3>()
                        {
                            cameraStartingPoint, 
                            cameraStartingPoint + new Vector3(0.25f * pageWidth, 0, 4.0f * pageDepthFromCamera),
                            cameraStartingPoint + new Vector3(0.75f * pageWidth, 0, 4.0f * pageDepthFromCamera),
                            cameraStartingPoint + new Vector3(1.1f * pageWidth, 0, 0)
                        };
                        var targetPoints = new List<Vector3>()
                        {
                            cameraTargetPoint,
                            cameraTargetPoint,
                            cameraTargetPoint,
                            cameraTargetPoint
                        };
                        var controlPointsReversed = new List<Vector3>()
                        {
                            cameraStartingPoint + new Vector3(1.1f * pageWidth, 0, 0),
                            cameraStartingPoint + new Vector3(0.75f * pageWidth, 0, 4.0f * pageDepthFromCamera),
                            cameraStartingPoint + new Vector3(0.25f * pageWidth, 0, 4.0f * pageDepthFromCamera),
                            cameraStartingPoint
                        };
                        var targetPointsReversed = new List<Vector3>()
                        {
                            cameraTargetPoint,
                            cameraTargetPoint,
                            cameraTargetPoint,
                            cameraTargetPoint,
                        };

                        this._currentFlyBy = new FlyBy();
                        this._currentFlyBy.FlyByFinished += OnCurrentFlyByFinished;
                        this._currentFlyBy.InitBezierControlPoints(GameConstants.DURATION_INTRO, controlPoints, targetPoints);

                        var nextFlyBy = new FlyBy();
                        nextFlyBy.FlyByFinished += OnCurrentFlyByFinished;
                        nextFlyBy.InitBezierControlPoints(GameConstants.DURATION_INTRO, controlPointsReversed, targetPointsReversed);

                        this._currentFlyBy.NextFlyBy = nextFlyBy;

                        break;
                    }
                case GameState.Moving:
                    {
                        // Enable touches for hero
                        this.TouchMode = CCTouchMode.OneByOne;
                        this.TouchEnabled = true;

                        break;
                    }
                case GameState.MovingToNextLine:
                    {
                        this.TouchEnabled = false;

                        // Set state
                        this._currentLineNumber++;

                        // Update hud
                        this._hudLayer.DisplayPageLineNumber(this._currentPageNumber, this._currentLineNumber);
                        this._hudLayer.EnablePause(enabled: false);

                        // Create a default bezier
                        this._currentFlyBy = new FlyBy(
                            cameraUpdate: FlyByCameraUpdate.TrackingAndStationaryHeightOnly,    // We will update our tracking camera and only the height of the stationary camera
                            targetAttachment: this._characterCache.TheHeroModel,                // We will have the hero model follow our target bezier
                            targetAttachmentUse: FlyByTargetAttachment.UsePhysicsBody);         // We will use the physics model for the hero when following the target bezier
                        this._currentFlyBy.FlyByFinished += OnCurrentFlyByFinished;             // Will switch state here in this event handler when finished

                        // Grab easy to reference positions
                        var heroPosition = this._characterCache.TheHeroModel.WorldOrigin;
                        var previousLineModel = this._lineCache.GetLineModel(this._currentLineNumber - 1);
                        var currentLineModel = this._lineCache.GetLineModel(this._currentLineNumber);
                        var lineSpacing = this._pageCache.CurrentPageModel.WorldLineSpacing;
                        var pageWidth = this._pageCache.CurrentPageModel.WorldWidth;
                        var pageDepth = this._pageCache.CurrentPageModel.WorldDepth;
                        var previousCenterLineWorldHeight = previousLineModel.WorldOrigin.Y + (0.5f * lineSpacing);
                        var currentCenterLineWorldHeight = currentLineModel.WorldOrigin.Y + (0.5f * lineSpacing);

                        // Create defined camera start and stop positions
                        var cameraStartPosition = this._inputManager.LineRunnerCamera.Position;
                        var cameraStartTarget = this._inputManager.LineRunnerCamera.Target;
                        var cameraStop = new Camera(
                            Vector3.Zero,
                            0f,
                            0f,
                            this._inputManager.DefaultCameraProjection);
                        var heroStopPosition = new float[]
                            {
                                heroPosition.X,
                                currentCenterLineWorldHeight,
                                heroPosition.Z
                            };
                        UpdateTrackingCamera(cameraStop, heroStopPosition);
                        var cameraStopPosition = cameraStop.Position;
                        var cameraStopTarget = cameraStop.Target;

                        // We use this direction modifier to control our curve
                        // line number even => construct curve on right of pad
                        // line number odd => construct curve on left of pad
                        float direction = 1f;
                        if (this._currentLineNumber % 2 != 0)
                        {
                            direction = -direction;
                        }

                        // Create control and target points for bezier
                        // Note, hero will be positioned at target points
                        var controlPoints = new List<Vector3>()
                        {
                            cameraStartPosition,                                              
                            cameraStartPosition + new Vector3(0.75f * pageWidth * direction, -0.75f * lineSpacing, -1.2f * this._pageCache.CharacterDepthFromCameraStart),
                            cameraStartPosition + new Vector3(1.5f  * pageWidth * direction, -1.0f  * lineSpacing, -1.2f * this._pageCache.CharacterDepthFromCameraStart),
                            cameraStopPosition
                        };
                        var targetPoints = new List<Vector3>()
                        {
                            cameraStartTarget,
                            cameraStartTarget + new Vector3(0.5f * pageWidth * direction, -0.75f * lineSpacing, 0f),
                            cameraStartTarget + new Vector3(0.5f * pageWidth * direction, -1.0f  * lineSpacing, 0f),
                            cameraStopTarget
                        };
                        this._currentFlyBy.InitBezierControlPoints(GameConstants.DURATION_MOVE_TO_NEXT_LINE, controlPoints, targetPoints);

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        this.TouchEnabled = false;

                        // Set state
                        this._currentPageNumber++;
                        this._currentLineNumber = 1;

                        // Update hud
                        // Important: Don't call HudLayer.DisplayPageLineNumber as we are updating both status lines below
                        this._hudLayer.EnablePause(enabled: false);

                        // Construct appropriate messages to display while we are moving to next page.
                        // Will be removed in FlyByFinished event handler
                        var movingToNextPageTitle = string.Empty;
                        switch (this._currentPageNumber)
                        {
                            case 2:
                                {
                                    movingToNextPageTitle = this._movingToPage2Text;
                                    break;
                                }
                            case 3:
                                {
                                    movingToNextPageTitle = this._movingToPage3Text;
                                    break;
                                }
                            case 4:
                                {
                                    movingToNextPageTitle = this._movingToPage4Text;
                                    break;
                                }
                            case 5:
                                {
                                    movingToNextPageTitle = this._movingToPage5Text;
                                    break;
                                }
                            case 6:
                                {
                                    movingToNextPageTitle = this._movingToPage6Text;
                                    break;
                                }

                        }
                        this._hudLayer.UpdateStatus1(movingToNextPageTitle);
                        var movingToNextPageDescription = this._movingToPageText + " " + this._currentPageNumber;
                        this._hudLayer.UpdateStatus2(movingToNextPageDescription);

                        // Create a default bezier
                        this._currentFlyBy = new FlyBy(
                            cameraUpdate: FlyByCameraUpdate.TrackingAndStationaryHeightOnly,
                            targetAttachment: this._characterCache.TheHeroModel,
                            targetAttachmentUse: FlyByTargetAttachment.UsePhysicsBody);
                        this._currentFlyBy.FlyByFinished += OnCurrentFlyByFinished;     // Will switch state here in this event handler when finished

                        // Create easy to reference positions
                        var pageModel = this._pageCache.CurrentPageModel;
                        var pageWidth = this._pageCache.CurrentPageModel.WorldWidth;
                        var pageHeight = this._pageCache.CurrentPageModel.WorldHeight;
                        var pageDepth = this._pageCache.CurrentPageModel.WorldDepth;

                        // Create defined cmaera start and stop positions
                        var cameraStartPosition = this._inputManager.LineRunnerCamera.Position;
                        var cameraStartTarget = this._inputManager.LineRunnerCamera.Target;
                        var cameraStop = new Camera(
                            Vector3.Zero,
                            0f,
                            0f,
                            this._inputManager.DefaultCameraProjection);
                        var heroStopPosition = new float[] 
                            {
                                pageModel.HeroStartOrigin.X,
                                pageModel.HeroStartOrigin.Y,
                                pageModel.HeroStartOrigin.Z
                            };
                        UpdateTrackingCamera(cameraStop, heroStopPosition);
                        var cameraStopPosition = cameraStop.Position;
                        var cameraStopTarget = cameraStop.Target;

                        // Create control and target points for bezier
                        // Note, hero will be positioned at target points
                        var controlPoints = new List<Vector3>()
                        {
                            cameraStartPosition,                                              
                            cameraStartPosition + new Vector3(-1.5f * pageWidth, 0.75f * pageHeight, 10.0f * pageDepth),  
                            cameraStartPosition + new Vector3(-1.5f * pageWidth, 1.25f * pageHeight, 10.0f * pageDepth),  
                            cameraStopPosition
                        };
                        var targetPoints = new List<Vector3>()
                        {
                            cameraStartTarget,                                                
                            controlPoints[1] + new Vector3(0.25f * pageWidth, -0.25f * pageHeight, 0),
                            controlPoints[2] + new Vector3(0.25f * pageWidth, -0.25f * pageHeight, 0),
                            cameraStopTarget
                        };
                        this._currentFlyBy.InitBezierControlPoints(GameConstants.DURATION_MOVE_TO_NEXT_PAGE, controlPoints, targetPoints);

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.GameStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.GameStartLineNumber;
                        GameManager.SharedGameManager.CurrentScore = GameManager.SharedGameManager.GameStartScore;

                        // Update score/page/line number in hud
                        this._hudLayer.DisplayScore(GameManager.SharedGameManager.GameStartScore);
                        this._hudLayer.DisplayPageLineNumber(this._currentPageNumber, this._currentLineNumber);

                        break;
                    }
                case GameState.ResumingFromWorld:
                    {
                        // Currently a no-op
                        break;
                    }
                case GameState.Refresh:
                case GameState.Start:
                    {
                        this._handlingKill = false;
                        this.TouchEnabled = false;

                        // Set state
                        this._currentPageNumber = GameManager.SharedGameManager.GameStartPageNumber;
                        this._currentLineNumber = GameManager.SharedGameManager.GameStartLineNumber;
                        GameManager.SharedGameManager.CurrentScore = GameManager.SharedGameManager.GameStartScore;

                        // Update score/page/line number in hud
                        this._hudLayer.DisplayScore(GameManager.SharedGameManager.GameStartScore);
                        this._hudLayer.DisplayPageLineNumber(this._currentPageNumber, this._currentLineNumber);

                        break;
                    }
            }

            this.SwitchServicesState(gameState);
        }

        public void Refresh()
        {
            this.SwitchState(GameState.Refresh);
        }

        public void UpdateTrackingCamera(Camera virtualCamera=null, float[] virtualHeroPosition=null)
        {
            //
            // TODO: Get setup once and then just add in deltas
            //

            // Determine camera and hero position we will use
            var camera = virtualCamera == null ? this._inputManager.LineRunnerCamera : virtualCamera;
            var heroPosition = virtualHeroPosition == null ? 
                this._characterCache.TheHeroModel.WorldOrigin : 
                new Vector3(virtualHeroPosition[0], virtualHeroPosition[1], virtualHeroPosition[2]);
            
            // Determine center line we will use
            float centerLineWorldHeight;
            var lineModel = this._lineCache.GetLineModel(this._currentLineNumber);
            if (lineModel != null)
            {
                var lineSpacing = this._pageCache.CurrentPageModel.WorldLineSpacing;
                centerLineWorldHeight = lineModel.WorldOrigin.Y +
                                        (0.5f * lineSpacing);
            }
            else if (virtualHeroPosition != null)
            {
                centerLineWorldHeight = virtualHeroPosition[1];
            }
            else
            {
                return;
            }

            // 1. Set camera target, accounting for any adjustments
            //    made by user in xy plane
            var isEvenLine = this._currentLineNumber % 2 == 0;
            var offsetX = heroPosition.X + this._inputManager.HudCameraOffsetX;
            if (isEvenLine)
            {
                offsetX = heroPosition.X - this._inputManager.HudCameraOffsetX;
            }
            camera.Target = new Vector3(
                offsetX,
                centerLineWorldHeight + this._inputManager.HudCameraOffsetY,
                heroPosition.Z);

            // 2. We now want to add any orbit adjustments made by the user. We do this by
            //    rotating around the just set camera target
            //    Reference: http://stackoverflow.com/questions/10372495/rotation-around-a-point

            // 2a. Construct a vector representing the orbit offset (distance) 
            //     from the camera target (what we will rotate around)
            //     (e.g. 0, 0, this._pageCache.CharacterDepthFromCameraStart)
            Vector3 orbitOffset = Vector3.UnitZ * this._pageCache.CharacterDepthFromCameraStart;

            // Note:
            // Not using this for now, but this will take into account initial position of camera. Be sure to set it first.
            // Vector3 orbitOffset = this._inputManager.LineRunnerCamera.Position - this._inputManager.LineRunnerCamera.Target;       

            // 2b. Now construct a rotation matrix based on what the user has set
            var orbitYaw = this._inputManager.HudCameraOrbitYaw;
            if (isEvenLine)
            {
                orbitYaw = -this._inputManager.HudCameraOrbitYaw;
            }
            Matrix orbitRotation = Matrix.CreateFromYawPitchRoll(
                orbitYaw,
                this._inputManager.HudCameraOrbitPitch,
                0f);

            // 2c. Then transform our original orbit offset vector with the desired rotation
            Vector3.Transform(ref orbitOffset, ref orbitRotation, out orbitOffset);

            // 2d. This final orbit offset vector can now be added to the camera target to correctly
            //     position our camara
            camera.Position = camera.Target + orbitOffset;

            // 3. Finally, add in any in-place yaw/pitch to the camera as defined by the user
            var offsetYaw = this._inputManager.HudCameraOffsetYaw;
            if (isEvenLine)
            {
                offsetYaw = -this._inputManager.HudCameraOffsetYaw;
            }
            camera.Yaw(offsetYaw);
            camera.Pitch(this._inputManager.HudCameraOffsetPitch);
        }

        public void StartWorld()
        {
            this.SwitchState(GameState.World);
        }

        public void ResumeFromWorld()
        {
            this.SwitchState(GameState.Moving);
        }

        #endregion

        #region Event handlers


        // We hook this up upon handling kills so we can maintain state appropriately after
        // reloading for start.
        private void LoadContentAsyncFinishedHandler(object sender, LoadContentAsyncFinishedEventArgs args)
        {
            if (args.TheLoadContentAsyncType == LoadContentAsyncType.Cache ||
                args.TheLoadContentAsyncType == LoadContentAsyncType.Initialize)
            {
                // Unhook so we are a one-shot event handler
                this._obstacleCache.LoadContentAsyncFinished -= this.LoadContentAsyncFinishedHandler;

                this._handlingKill = false;

                // We can now let the hud layer's ui resume
                this._hudLayer.RestoreStart();
            }
            else
            {
                Debug.WriteLine("This should never happen:");
            }
        }

        private void OnSwitchingUI(object sender, SwitchUIEventArgs e)
        {
            switch (e.NewLayer)
            {
                case LayerTags.HudLayer:
                    {
                        // Immediately unhook any fly-bys in progress
                        if (this._currentFlyBy != null)
                        {
                            this._currentFlyBy.FlyByFinished -= OnCurrentFlyByFinished;
                            this._currentFlyBy = null;
                        }

                        // IMPORTANT: The switch state to GameState.Moving will originate
                        //            from the existing HudLayer.

                        break;
                    }
                case LayerTags.StartLayer:
                    {
                        this.UpdateTrackingCamera();

                        SwitchState(GameState.Start);

                        break;
                    }
            }
        }

        private void OnCurrentFlyByFinished(object sender, FlyByFinishedEventArgs e)
        {
            // Immediately unhook to make this a one-shot event
            this._currentFlyBy.FlyByFinished -= OnCurrentFlyByFinished;
            this._currentFlyBy = null;

            if (e.NextFlyBy != null)
            {
                this._currentFlyBy = e.NextFlyBy;
                return;
            }

            switch(this._currentGameState)
            {
                case GameState.MovingToNextLine:
                    {
                        // Update hud
                        this._hudLayer.EnablePause(enabled: true);

                        // Moving to next line is finished while in-game, get
                        // hero moving again
                        this.SwitchState(GameState.Moving);

                        break;
                    }
                case GameState.MovingToNextPage:
                    {
                        // Update hud
                        this._hudLayer.EnablePause(enabled: true);
                        this._hudLayer.UpdateStatus1(string.Empty);
                        this._hudLayer.UpdateStatus2(string.Empty);


                        // Moving to next page is finished while in-game, get
                        // hero moving again
                        this.SwitchState(GameState.Moving);

                        break;
                    }
                case GameState.MovingToStart:
                    {
                        // Moving to start has finished (e.g., from finish layer)

                        // IMPORTANT: Turn on ability to start game now that flyby created
                        // is finished. This precaution is in place in case user quickly
                        // exits from finish screen while flyby is in process.
                        //
                        // Currently, a complete flyby is important as it will position hero
                        // correctly for a clean start
                        this._parent.TheStartLayer.EnableStart(true);

                        break;
                    }
            }
        }

        private void OnSensorHit(object sender, SensorHitEventArgs e)
        {
            var sensorModel = e.TheSensorModel;

            if (e.TheSensorModel is GoalSensorModel)
            {
                // TODO: Cache string for faster reference
                SoundUtils.PlaySoundEffect(_assetManager.GetSound(Asset.SoundHeroPoint));

                GameManager.SharedGameManager.CurrentScore++;
                _hudLayer.DisplayScore(GameManager.SharedGameManager.CurrentScore);
            }
            else if (e.TheSensorModel is MarginSensorModel)
            {
                if (this._currentLineNumber == this._pageCache.CurrentPageModel.ThePadEntity.LineCount &&
                    this._currentPageNumber == 6)
                {
                    this.HandleMoveToFinish(isWinner: true);
                }
                else if (this._currentLineNumber != this._pageCache.CurrentPageModel.ThePadEntity.LineCount)
                {
                    this.SwitchState(GameState.MovingToNextLine);
                }
                else
                {
                    this.SwitchState(GameState.MovingToNextPage);
                }
            }

        }

        private void OnObstacleHit(object sender, ObstacleHitEventArgs e)
        {
            // Don't let hits outside of game play sneak in
            if (this._currentGameState != GameState.Moving)
            {
                return;
            }

            // Play particle effect animation
            this._particleCache.AddHitParticleEffect(
                e.TheObstacleModel, 
                e.TheObstacleModel.TheContact, 
                this._obstacleCache.TheCustomContentManager); // this._obstacleCache.ContentManagers[this._currentLineNumber]);

            // Play glow animation
            var glowUpAction = new TintTo(GameConstants.DURATION_OBSTACLE_GLOW, Microsoft.Xna.Framework.Color.White, 0.1f);
            var glowDownAction = new TintTo(GameConstants.DURATION_OBSTACLE_GLOW, Microsoft.Xna.Framework.Color.White, 0.0f);
            var turnOffTint = new CallFunc(() => { e.TheObstacleModel.IsTinted = false; });
            var glowAction = new Sequence(new FiniteTimeAction[] 
                { 
                    glowUpAction, 
                    glowDownAction,
                    turnOffTint
                });
            e.TheObstacleModel.ModelRunAction(glowAction);

            // Are we allowing kills
            // AND we are not already handling a previous kill?
            if (GameManager.SharedGameManager.GameAreKillsAllowed &&
                !this._handlingKill)
            {
                // Make sure we only handle one kill at at a time
                this._handlingKill = true;

                // Don't let touches sneak in
                this.TouchEnabled = false;

                // Adjust hud ui accordingly
                this._hudLayer.HandleKill();

                // Animate hero being killed then determine where to navigate to in callback
                this._characterCache.HandleKill(
                    e.TheObstacleModel.TheContact,
                    () => HandleMoveToFinish(isWinner: false));
            }
        }

        private void OnLineHit(object sender, LineHitEventArgs e)
        {
            // Don't let hits outside of game play sneak in
            if (this._currentGameState != GameState.Moving)
            {
                return;
            }

            // Play particle effect animation
            this._particleCache.AddHitParticleEffect(
                e.TheLineModel, 
                e.TheLineModel.TheContact,
                e.TheLineModel.TheCustomContentManager);

            // Play glow animation
            var glowUpAction = new TintTo(GameConstants.DURATION_OBSTACLE_GLOW, Microsoft.Xna.Framework.Color.White, 0.1f);
            var glowDownAction = new TintTo(GameConstants.DURATION_OBSTACLE_GLOW, Microsoft.Xna.Framework.Color.White, 0.0f);
            var turnOffTint = new CallFunc(() => { e.TheLineModel.IsTinted = false; });
            var glowAction = new Sequence(new FiniteTimeAction[] 
                { 
                    glowUpAction, 
                    glowDownAction,
                    turnOffTint
                });
            e.TheLineModel.ModelRunAction(glowAction);

            // Are we allowing kills?
            if (GameManager.SharedGameManager.GameAreKillsAllowed &&
                !this._handlingKill)
            {
                // Make sure we only handle one kill at at a time
                this._handlingKill = true;

                // Don't let touches sneak in
                this.TouchEnabled = false;

                // Adjust hud ui accordingly
                this._hudLayer.HandleKill();

                // Animate hero being killed then determine where to navigate to in callback
                this._characterCache.HandleKill(
                    e.TheLineModel.TheContact,
                    () => HandleMoveToFinish(isWinner: false));
            }
        }

        #endregion

        #region CCLayer Framework Overrides

        public override void Update(float dt)
        {
            // Don't kick off our second stage of initialization until
            // core app initialization is complete (e.g., database copied into place,
            // user settings file in place, etc.)
            if (!TheGame.SharedGame.Ready)
            {
                return;
            }

            if (!this.Ready &&                               // If we have not flipped our one-time flag that says we are ready
                GameManager.SharedGameManager.ServicesReadyForUpdate)   // AND the game services are ready to be referenced
            {
                // Get references to services we need
                this._assetManager = (IAssetManager)TheGame.SharedGame.Services.GetService(typeof(IAssetManager));
                this._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));
                this._skyDome = (ISkyDome)TheGame.SharedGame.Services.GetService(typeof(ISkyDome));
                this._newSky = (INewSky)TheGame.SharedGame.Services.GetService(typeof(INewSky));
                this._chunkCache = (IChunkCache)TheGame.SharedGame.Services.GetService(typeof(IChunkCache));
                this._world = (IWorld)TheGame.SharedGame.Services.GetService(typeof(IWorld));
                this._waterCache = (IWaterCache)TheGame.SharedGame.Services.GetService(typeof(IWaterCache));
                this._physicsManager = (IPhysicsManager)TheGame.SharedGame.Services.GetService(typeof(IPhysicsManager));
                this._pageCache = (IPageCache)TheGame.SharedGame.Services.GetService(typeof(IPageCache));
                this._lineCache = (ILineCache)TheGame.SharedGame.Services.GetService(typeof(ILineCache));
                this._obstacleCache = (IObstacleCache)TheGame.SharedGame.Services.GetService(typeof(IObstacleCache));
                this._characterCache = (ICharacterCache)TheGame.SharedGame.Services.GetService(typeof(ICharacterCache));
                this._particleCache = (IParticleEffectCache)TheGame.SharedGame.Services.GetService(typeof(IParticleEffectCache));
                this._sensorCache = (ISensorCache)TheGame.SharedGame.Services.GetService(typeof(ISensorCache));
                this._deferredShadowMapping = (IDeferredShadowMapping)TheGame.SharedGame.Services.GetService(typeof(IDeferredShadowMapping));
                this._userInterface = (IUserInterface)TheGame.SharedGame.Services.GetService(typeof(IUserInterface));
                this._inGameDebugger = (IInGameDebuggerService)TheGame.SharedGame.Services.GetService(typeof(IInGameDebuggerService));
                this._debugBar = (IStatistics)TheGame.SharedGame.Services.GetService(typeof(IStatistics));
                this._graphManager = (IGraphManager)TheGame.SharedGame.Services.GetService(typeof(IGraphManager));
                this._audioManager = (IAudioManager)TheGame.SharedGame.Services.GetService(typeof(IAudioManager));

                // Register event handlers
                this._lineCache.LineHit += OnLineHit;
                this._obstacleCache.ObstacleHit += OnObstacleHit;
                this._sensorCache.SensorHit += OnSensorHit;

                // Flip our one-time flag that says we are ready (e.g., for Update/Draw)
                this.Ready = true;

#if STOPWATCH
                Program.TheStopwatch.Stop();
                Debug.WriteLine("ActionLayer.Ready: " + Program.TheStopwatch.ElapsedMilliseconds);
                Program.TheStopwatch.Restart();
#endif

            }

            // We are not ready to update/draw so just return until we are
            if (!this.Ready)
            {
                return;
            }

            // Take care of any animations in progress
            GameManager.SharedGameManager.TheActionManager.Update(dt);

            // Construct an XNA time from a Cocos2d time
            this._totalGameTime += dt;
            this._gameTime.ElapsedGameTime = TimeSpan.FromSeconds(dt);
            this._gameTime.TotalGameTime = TimeSpan.FromSeconds(_totalGameTime);

            // Do a physics simulation step
            // IMPORTANT: Needs to come before InputManager.Update() as InputManager.Update's
            //            XxxControllers will depend upon a physics's space update happening
            //            before their XxxController.Update()'s are called.
            this._physicsManager.TheSpace.Update();
/*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("PhysicsManager.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
*/

            // Let voxeliq services update
            this._inputManager.Update(this._gameTime);
/*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("InputManager.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
*/
            // TODO: Add back in when ready to tackle skydome again
            // this._skyDome.Update(this._gameTime);
            this._newSky.Update(this._gameTime);
            /*
#if STOPWATCH

            Program.TheStopwatch.Stop();
            Debug.WriteLine("NewSky.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
             */
            this._chunkCache.Update(this._gameTime);
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("ChunkCache.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            */
            this._world.Update(this._gameTime);
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("World.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            */

            // TODO: Add back in when ready to tackle water again
            // this._waterCache.Update(this._gameTime);
            this._deferredShadowMapping.Update(this._gameTime);
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("DeferredShadowMapping.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            */
            this._userInterface.Update(this._gameTime);
            /*
#if STOPWATCH
            Program.TheStopwatch.Stop();
            Debug.WriteLine("UserInterface.Update: " + Program.TheStopwatch.ElapsedMilliseconds);
            Program.TheStopwatch.Restart();
#endif
            */

            /* TODO: Add back in when ready to investigate these
            this._inGameDebugger.Update(this._gameTime);
            this._debugBar.Update(this._gameTime);
            this._graphManager.Update(this._gameTime);
            */

            // We depend on InputManager.ThePlayerControllerInput.CurrentChunk to be set
            // AND the resulting PageCache.CalculateWorldCoordinates() to be called before
            // we allow our second tier of GameObjects to start updating.
            // See World.SpawnWorld for details.
            if (!this._inputManager.TheLineRunnerControllerInput.Ready)
            {
                return;
            }

            // Do a one-time prep of our first page
            if (!this._firstTimeSetupPage)
            {
                // It is now safe to lazy load our hud layer 
                // (depends upon stationary camera being in place for display of panes)
                this._hudLayer = _parent.TheHudLayer;

                // Now that our page components are ready to be accessed, get our intro mechanics going (e.g., flyby),
                // and background loading of models going.
                // UNLESS we user has already indicated they want to skip intro by tapping start
                // right away.
                if (this._currentGameState != GameState.Start)
                {
                    this.SwitchState(GameState.Intro);
                }

                this._firstTimeSetupPage = true;

#if STOPWATCH
                Program.TheStopwatch.Stop();
                Debug.WriteLine("ActionLayer.FirstTimeSetupPage: " + Program.TheStopwatch.ElapsedMilliseconds);
                Program.TheStopwatch.Restart();
#endif
            }

            // Let game services update
            this._pageCache.Update(this._gameTime);
            this._lineCache.Update(this._gameTime);
            this._obstacleCache.Update(this._gameTime);
            this._characterCache.Update(this._gameTime);
            this._particleCache.Update(this._gameTime);
            this._sensorCache.Update(this._gameTime);

            // Adjust hero physics so it stays in 2D plane
            if (this._characterCache.TheHeroModel != null)
            {
                this._characterCache.TheHeroModel.AdjustHeroPhysics();
            }

            // Position camera based on whether we are in a fly by or simply
            // tracking the hero position
            if (this._currentFlyBy != null)
            {
                this._currentFlyBy.UpdateBezier(dt);
            }
            // else if (this._characterCache.TheHeroModel != null)
            else if (this._currentGameState == GameState.Moving &&
                     this._characterCache.TheHeroModel != null)
            {
                this.UpdateTrackingCamera();
            }
            else
            {
                this.UpdateTrackingCamera();
                // this.UpdateStartingCamera();
            }
        }

        public override void Draw()
        {
            // We are dependent on this layer being ready (see Update())
            // AND the camera being ready before we start drawing
            if (!this.Ready ||                    
                !this._inputManager.TheLineRunnerControllerInput.Ready) /* ||
                !this._chunkCache.Ready)*/
            {
                return;
            }

            //
            // Note, unlike our short-cicuit for Update above when in refresh state,
            // we do those checks for drawing at the individual GameObject level
            // (e.g., see LineCache.Draw)
            //

            // Get state correct before doing our drawing pass
            // XNAUtils.DefaultDrawState();
            var previousDepthStencilState = TheGame.SharedGame.GraphicsDevice.DepthStencilState;
            var previousBlendState = TheGame.SharedGame.GraphicsDevice.BlendState;
            var previousSamplerState = TheGame.SharedGame.GraphicsDevice.SamplerStates[0];
            var previousRasterizerState = TheGame.SharedGame.GraphicsDevice.RasterizerState;

            TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.Opaque;
            TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            TheGame.SharedGame.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            this._deferredShadowMapping.Draw(this._gameTime);

            this._particleCache.Draw(this._gameTime);

            // XNAUtils.Cocos2dDrawState();
            TheGame.SharedGame.GraphicsDevice.DepthStencilState = previousDepthStencilState;
            TheGame.SharedGame.GraphicsDevice.BlendState = previousBlendState;
            TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = previousSamplerState;
            TheGame.SharedGame.GraphicsDevice.RasterizerState = previousRasterizerState;

            /* TODO: Why is this not working here?
             * We currently have this in the DeferredShadowMapping drawing pipeline.
            LineDrawer.LightingEnabled = false;
            LineDrawer.VertexColorEnabled = true;
            LineDrawer.World = Matrix.Identity;
            LineDrawer.ViewDirection = this._trackingCamera.ViewDirection;
            LineDrawer.ProjectionMatrix = this._trackingCamera.ProjectionMatrix;
            ContactDrawer.Draw(LineDrawer, this._physicsManager.TheSpace);
            BoundingBoxDrawer.Draw(LineDrawer, this._physicsManager.TheSpace);
            */
            
            // TODO: Is this OK?
            // base.Draw();
        }

        #endregion
        
        #region Touch Implementation

        /*
        http://www.cocos2d-iphone.org/forums/topic/tutorials-dont-mention-cctouchdispatcherremovedelegate/
         
        Setting self.isTouchEnabled to YES in a CCLayer causes RegisterWithTouchDispatcher 
        to be called in onEnter, and CCDirector.SharedDirector.TouchDispatcher.RemoveDelegate(this)
        to be called in onExit.

        RegisterWithTouchDispatcher in CCLayer registers as a Standard touch delegate. 
        So you only need to override it if you want the Targeted touch messages.
            
        Note if you don't set CCTouchMode it will default to CCTouchMode.AllAtOnce, which means
        override TouchesBegan. Otherwise set CCTouchMode to CCTouchMode.OneByOne and override
        TouchBegan.
         
        In TouchBegan, If you return true then ccTouchEnded will called. 
        If you return false then ccTouchEnded will not be called, and the event 
        will go the parent layer
        */

        /// <summary>
        /// If you return true then ccTouchEnded will called. 
        /// If you return false then ccTouchEnded will not be called, and the event will go the parent layer
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
        public override bool TouchBegan(CCTouch touch)
        {
            // Flow touch down to hero if we are in game play
            if (this._currentGameState == GameState.Moving)
            {
                _characterCache.TouchBegan(touch);
            }

            return true;
        }

        #endregion

        #region Helper Methods

        // Helper function to centralize calling SwitchState on all appropriate game services
        private void SwitchServicesState(GameState gameState)
        {
            this._assetManager.SwitchState(gameState);
            this._world.SwitchState(gameState);
            this._chunkCache.SwitchState(gameState);
            this._inputManager.SwitchState(gameState);
            this._audioManager.SwitchState(gameState);

            // IMPORTANT DEPENDENCY GRAPH:
            // PageCache <- LineCache <- ObstacleCache
            // Hence SwitchState's are handled for other Game objects include character and sensor
            // in the appropriate parent SwitchState implementation
            this._pageCache.SwitchState(gameState);
            // this._lineCache.SwitchState(gameState);
            // this._obstacleCache.SwitchState(gameState);
            // this._characterCache.SwitchState(gameState);
            // this._sensorCache.SwitchState(gameState);

            this._particleCache.SwitchState(gameState);
            this._deferredShadowMapping.SwitchState(gameState);
        }

        // Callback used when line/obstacle hit to determine
        // where to navigate to next
        public void HandleMoveToFinish(bool isWinner)
        {
            // Hook up an event handler for end of content loading caused by
            // moving to start kicking off background load
            this._obstacleCache.LoadContentAsyncFinished += this.LoadContentAsyncFinishedHandler;

            // IMPORTANT: grab these HERE as it will be reset in SwitchState(MovingToStart) immediately below
            var currentScore = GameManager.SharedGameManager.CurrentScore;
            var killsOffEventRecorded = this._hudLayer.KillsOffEventRecorded;
            var startingPageEventRecorded = this._hudLayer.StartingPageEventRecorded;
            var startingLineEventRecorded = this._hudLayer.StartingLineEventRecorded;

            // Flip tracking of upgrade events for start of next game run
            this._hudLayer.KillsOffEventRecorded = false;
            this._hudLayer.StartingPageEventRecorded = false;
            this._hudLayer.StartingLineEventRecorded = false;

            // Did we have any upgrade events that would invalidate this run
            var upgradeEventRecorded = false;
            /*
            var upgradeEventRecorded = 
                killsOffEventRecorded ||
                startingPageEventRecorded ||
                startingLineEventRecorded;
            */

            // Now go for the async load
            this.SwitchState(GameState.MovingToStart);

            // Did we get a new high score without having "Kills On"?
            var scoreRepository = new FacebookScoreRepository();
            var previousTopScore = scoreRepository.GetTopScoresForPlayer(1);
            var qualifiesAsNewTopScore = 
                !upgradeEventRecorded                           // We did not have upgrade event enabled during this game run
                &&                                              // AND
                currentScore > 0                                // We have a score to check
                &&                                              // AND
                (previousTopScore.Count == 0 ||                 // This is our first top score or we beat our previous top score
                 previousTopScore[0].Score < currentScore);

            if (isWinner ||
                qualifiesAsNewTopScore)
            {
                // Record new high score and give them a chance to post
                this._parent.TheFinishLayer.SetNewHighScore(currentScore);
                this._parent.TheFinishLayer.SetIsWinner(isWinner);
                this._parent.Navigate(LayerTags.FinishLayer);
            }

        }

        #endregion
    }
}