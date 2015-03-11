using System;
using System.Collections.Generic;
using SQLite;


namespace Simsip.LineRunner.Entities.Facebook
{
    /// <summary>
    /// Reference:
    /// https://developers.facebook.com/docs/opengraph/using-actions/
    /// 
    /// Example:
    /// POST /me/cookbook:eat?
    /// recipe=http://www.example.com/recipes/pizza/&
    /// start_time=2013-03-06T15:18:26-08:00&
    /// end_time=2013-03-06T15:18:27-08:00&
    /// access_token=VALID_ACCESS_TOKEN
    /// 
    /// </summary>
    public class FacebookActionEntity
    {
        #region Data Model

        #region Fields

        /// <summary>
        /// The id created on the device to identify this action.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int DeviceId { get; set; }

        /// <summary>
        /// The action id.
        /// 
        /// This should be unique from facebook.
        /// </summary>
        [Indexed]
        public string ServerId { get; set; }

        /// <summary>
        /// The type of action (e.g., like, cook, eat, build, etc.)
        /// 
        /// Required: Yes
        /// 
        /// For common actions, the {action-type} is the name of the action, such as og.likes:
        /// og.likes
        /// 
        /// For custom actions, it is a composite of the app namespace and the custom action type:
        /// {namespace}:{action-type-name}
        /// Example: cookbook:eat
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// The type of the object associated with this action (e.g., recipe)
        /// 
        /// The type of object used here is determined by the action type being 
        /// published, for example og.likes uses an object type, fitness.runs uses
        /// an course type. The value of this object is the URL or ID of an 
        /// appropriate object instance.
        /// 
        /// Required: Yes
        /// 
        /// Example: recipe=http://www.example.com/recipes/pizza/
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// The value of the object associated with this action (e.g., recipe)
        /// 
        /// The type of object used here is determined by the action type being 
        /// published, for example og.likes uses an object type, fitness.runs uses
        /// an course type. The value of this object is the URL or ID of an 
        /// appropriate object instance.
        /// 
        /// Required: Yes
        /// 
        /// Example: recipe=http://www.example.com/recipes/pizza/
        /// </summary>
        public string ObjectValue { get; set; }

        /// <summary>
        /// The time this action started.
        /// 
        /// Required: No
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The time this action ended.
        /// 
        /// Required: No
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// The number of seconds before this action is considered “old”. 
        /// 
        /// Required: No
        /// 
        /// From the time the action until expires_in seconds have elapsed, 
        /// the action is considered “present tense”, and afterwards, it is 
        /// considered “past tense”.
        /// 
        /// Expires_in is a shortcut for specifying end_time when it is more 
        /// convenient to provide a delta in seconds between when an action 
        /// started and when it ends. For example, when a user starts watching a 
        /// movie, when you post a watch action, expires_in should be the length 
        /// of the movie in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// If false we will suppress notifications resulting from the published action. 
        /// This is only applicable to likes.
        /// 
        /// Required: No
        /// </summary>
        public bool Notify { get; set; }

        /// <summary>
        /// A JSON-encoded object in the standard privacy parameter format that defines the 
        /// privacy setting for the action.
        /// 
        /// Required: No
        /// </summary>
        public string Privacy { get; set; }

        /// <summary>
        /// The URL of an image that is added to the story and overrides the image that is 
        /// associated with the object properties. 
        /// 
        /// See the image capabilities for information about publishing large-format photos with stories.
        /// 
        /// Capabilities:
        /// Various parameters that when used together indicate the inclusion of user generated photos 
        /// that display in a large format. Read the adding photos to stories guide for full parameter 
        /// information.
        /// 
        /// Required: No
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// When users click through to your site from an Open Graph action displayed in Facebook, 
        /// this value will be passed in the fb_ref query parameter which Facebook will add to 
        /// external links to your objects. 
        /// 
        /// This is useful for A/B tests and tracking other data associated with the action.
        /// 
        /// By default, the ref parameter will accept 500 unique values for ref in a 7 day period. 
        /// These values will be used to display graphs grouped by each unique value of ref in your 
        /// application's insights.
        /// 
        /// If you need more values 500 per 7 days, for example if you are passing globally unique IDs 
        /// via the ref parameter, you may use the "__" separator. Text to the left of the separator 
        /// will be used as indexes in Insights and used to help you analyze A/B test results. Text 
        /// to the right of the separator will be ignored for insights purposes, will not form part of 
        /// the 500 by 7 quota, but will still be passed in the fb_ref parameter the user follows a link
        /// from the story.
        /// 
        /// Required: No
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// If true we will scrape your object and update its properties prior to creating the action. 
        /// 
        /// Scraping will slow down the request and has the potential to hit rate limits so only scrape 
        /// when the object has been updated.
        /// 
        /// Required: No
        /// </summary>
        public bool Scrape { get; set; }

        /// <summary>
        /// Setting this to true will stop the action from publishing a story to news feed. 
        /// 
        /// This can be helpful when you are adding multiple actions at once and do not want to spam.
        /// 
        /// Required: No
        /// </summary>
        public bool NoFeedStory { get; set; }

        #endregion

        #region Capabilities

        /// <summary>
        /// Allows users to write a personalized message attached to this action. 
        /// 
        /// You can only use this when the text is entered by the user and not pre-populated. 
        /// You can mention users and pages inline using mention tagging.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Specifies whether the published action was explicitly shared by the user. Read the 
        /// Explicit Sharing guide for more info.
        /// </summary>
        public bool ExplicitlyShared { get; set; }

        /// <summary>
        /// The physical location represented by a Facebook Place where this action occurred, used 
        /// when tagging places.
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// Allows someone to tag people in a story. 
        /// 
        /// This string is composed of a comma-separated list of the IDs of those who are to be tagged.
        /// </summary>
        public string Tags { get; set; }

        #endregion

        #region Connections
        
        #endregion

        #endregion
    }
}
