using SQLite;


namespace Simsip.LineRunner.Entities.OAuth
{
    public class OAuthAccessEntity
    {
        /// <summary>
        /// The id created on the device to identify this OAuth access model.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int OAuthAccessId { get; set; }

        /// <summary>
        /// An enumeration to allow us to identify the type of OAuth access
        /// this model represents (e.g., Facebook, Groupme, Youtub).
        /// 
        /// This is important to know for implementations to add in
        /// implementation specific behavior.
        /// </summary>
        public OAuthType Type { get; set; }

        public string AuthorizationEndpoint { get; set; }

        /// <summary>
        /// The authorization server endpoint you will access to exchange the code you have received
        /// for an access token.
        /// </summary>
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// The OAuth2.0 production redirect uri the authorization server will use to return the code to.
        /// 
        /// Use the code received to exchange for an access token.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// The OAuth2.0 App Id of your project.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// A part of an OAuth2.0 client secret of your project.
        /// </summary>
        public string AppSecretPart1 { get; set; }

        /// <summary>
        /// A part of an OAuth2.0 client secret of your project.
        /// </summary>
        public string AppSecretPart2 { get; set; }

        /// <summary>
        /// A part of an OAuth2.0 client secret of your project.
        /// </summary>
        public string AppSecretPart3 { get; set; }

        /// <summary>
        /// A part of an OAuth2.0 client secret of your project.
        /// </summary>
        public string AppSecretPart4 { get; set; }

        /// <summary>
        /// A part of an OAuth2.0 client secret of your project.
        /// </summary>
        public string AppSecretPart5 { get; set; }

        /// <summary>
        /// A part of an OAuth2.0 client secret of your project.
        /// </summary>
        public string AppSecretPart6 { get; set; }

        /// <summary>
        /// A part of an OAuth2.0 client secret of your project.
        /// </summary>
        public string AppSecretPart7 { get; set; }

        /// <summary>
        /// A part of an OAuth2.0 client secret of your project.
        /// </summary>
        public string AppSecretPart8 { get; set; }

        /// <summary>
        /// The complete list of scopes we require
        /// </summary>
        public string Scopes { get; set; }
    }
}
