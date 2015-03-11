using SQLite;
using System;


namespace Simsip.LineRunner.Entities.Scoreoid
{
    /// <summary>
    /// This holds scoreboard information about a score.
    /// 
    /// Reference:
    /// http://wiki.scoreoid.net/api/engagement/getnotification/
    /// notifications => "Your game's in game notification content" [String]
    /// upon failure you well receive an error message, example - "Sorry there is no data for your current querry"
    /// title => "Notification title" [String]
    /// content => "Your game notification content" [String / HTML]
    /// status => "Notification status" [JSON Bool (0/1), XML Bool (true/false)]
    /// debug => "Notification debug mode" [JSON Bool (0/1), XML Bool (true/false)]  
    /// </summary>
    public class NotificationEntity
    {
        /// <summary>
        /// Unique identifier for this notification.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        [PrimaryKey, AutoIncrement]
        public int NotificationId { get; set; }
        
        /// <summary>
        /// "Your game's in game notification content" [String]
        /// upon failure you well receive an error message, example 
        /// - "Sorry there is no data for your current querry"
        /// </summary>
        public string Notification { get; set; }

        /// <summary>
        /// "Notification title" [String]
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// "Your game notification content" [String / HTML]
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// "Notification status" [JSON Bool (0/1), XML Bool (true/false)]
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// "Notification debug mode" [JSON Bool (0/1), XML Bool (true/false)]  
        /// </summary>
        public int Debug { get; set; }
    }
}

