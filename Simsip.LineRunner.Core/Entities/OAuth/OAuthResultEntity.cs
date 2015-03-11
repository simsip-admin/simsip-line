using System;
using SQLite;


namespace Simsip.LineRunner.Entities.OAuth
{
    /// <summary>
    /// Represents the authentication result of an OAuth acccess request.
    /// </summary>
    public class OAuthResultEntity
    {
        /// <summary>
        /// The id created on the device to identify this OAuth result model.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int OAuthResultId { get; set; }

        public OAuthType Type { get; set; }

        /// <summary>
        /// The access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Date and Time when the access token expires.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Error that happens when using OAuth2 protocol.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Short error reason for failed authentication if there was an error.
        /// </summary>
        public string ErrorReason { get; set; }

        /// <summary>
        /// Long error description for failed authentication if there was an error.
        /// </summary>
        public string ErrorDescription { get; set; }
        
        /// <summary>
        /// The code used to exchange access token.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets an opaque state used to maintain application state between the request and callback.
        /// </summary>
        public string State { get; set; }
    }
}
