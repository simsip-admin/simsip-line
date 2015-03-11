using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Entities;
using Simsip.LineRunner.GameFramework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;


namespace Simsip.LineRunner.GameObjects.Pages
{
    public interface IPageCache : IUpdateable, IDrawable
    {
        /// <summary>
        /// The value class that holds all pertinent info regarding
        /// the current page we are displaying.
        /// </summary>
        PageModel CurrentPageModel { get;  }

        /// <summary>
        /// IMPORTANT: Not the depth from the origin, but the absolute depth from a camera's defined
        /// start position.
        /// </summary>
        float PageDepthFromCameraStart { get; }

        /// <summary>
        /// Absolute depth from page to character.
        /// </summary>
        float PageDepthFromCharacter { get; }

        /// <summary>
        /// Absolute depth from page to obstacles.
        /// </summary>
        float PageDepthFromObstacle { get; }

        /// <summary>
        /// Absolute depth from page to panes.
        /// </summary>
        float PageDepthFromPane { get; }

        /// <summary>
        /// Depth from camara that we will place characters.
        /// </summary>
        float CharacterDepthFromCameraStart { get; }

        /// <summary>
        /// Depth from camara that we will place obstacles.
        /// </summary>
        float ObstacleDepthFromCameraStart { get; }

        /// <summary>
        /// Depth from camara that we will place panes.
        /// </summary>
        float PaneDepthFromCameraStart { get; }

        /// <summary>
        /// Positioning of various game objects (e.g., obstacles, sensors), depends upon the stationary
        /// camera's original position/target.
        /// 
        /// We need to record this as during game-play, we will adjust the height of the stationary camera
        /// as the player moves down the page.
        /// </summary>
        Vector3 StationaryCameraOriginalWorldPosition { get; }

        /// <summary>
        /// Positioning of various game objects (e.g., obstacles, sensors), depends upon the stationary
        /// camera's original position/target.
        /// 
        /// We need to record this as during game-play, we will adjust the height of the stationary camera
        /// as the player moves down the page.
        /// </summary>
        Vector3 StationaryCameraOriginalWorldTarget { get; }

        /// <summary>
        /// Based on:
        /// - the current camera setup 
        /// - a desired depth from the camera defined by the page cache
        ///   (DapthFromCameraStart)
        /// - a desired set of logical Cocos2d coordinates defined by the page cache
        ///   (LogicalXXX)
        /// determine a resulting set of World propertices for the current page.
        /// </summary>
        void CalculateWorldCoordinates();

        /// <summary>
        /// Handle page category specific game state changes.
        /// </summary>
        /// <param name="state">The game state we are switching to.</param>
        void SwitchState(GameState state);

        void Draw(Effect effect = null, EffectType type = EffectType.None);
    }
}