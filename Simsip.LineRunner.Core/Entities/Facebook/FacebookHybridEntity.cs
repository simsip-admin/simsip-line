using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Facebook
{
    /// <summary>
    /// A synthesized model for view models and views that need to represent a combined view
    /// of albums and videos interweaved together.
    /// </summary>
    public class FacebookHybridEntity
    {
        public const string AlbumType = "album";
        public const string VideoType = "video";

        #region Data Model

        #region Fields

        /// <summary>
        /// The id for the album or video on the device.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// The facebook album or video id.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// Currently one of 'album' or 'video'.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The profile id that created this album or video.
        /// </summary>
        [Indexed]
        public string FromId { get; set; }

        /// <summary>
        /// The profile name that created this album or video.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// The title of the album or video.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the album or video.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Temporary
        /// </summary>
        public string CoverPhotoThumbnailUrl { get; set; }

        /// <summary>
        /// If this is an album, the number of photos in this album
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// The last time the photo album was updated
        /// 
        /// A string containing ISO-8601 date-time
        /// </summary>
        public string UpdatedTime	{ get; set; }

        /// <summary>
        /// Does this album have new photos or has this video not been seen.
        /// </summary>
        public bool IsUnseen { get; set; }

        /// <summary>
        /// If this is a video, the link to the actual video to play
        /// </summary>
        public string Source { get; set; }

        #endregion

        #endregion
    }
}
