using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simsip.LineRunner.Services.OAuth
{
    public class OAuthCredentials
    {
        #region Groupme

        /// <summary>
        /// The OAuth groupme authorization server endpoint you will access to get the code you will use to exchange for
        /// an access token.
        /// </summary>
        public const string GROUPME_AUTHORIZATION_ENDPOINT = "https://api.groupme.com/oauth/authorize?client_id=3pAGCsJ5uivI42RvAo3yLkNRrrdljngCruMV2IkYTTSPmYID";

        /// <summary>
        /// The OAuth groupme redirect uri the authorization server will use to return the code to.
        /// 
        /// Use the code received to exchange for an access token.
        /// </summary>
        public const string GROUPME_REDIRECT_URI = "https://groupme-xamarin.azurewebsites.net/oauth_callback";

        /// <summary>
        /// The OAuth App Id of your groupme project.
        /// </summary>
        public const string GROUPME_APP_ID = "3pAGCsJ5uivI42RvAo3yLkNRrrdljngCruMV2IkYTTSPmYID";

        #endregion

        #region Facebook

#if DEV
        /// <summary>
        /// The OAuth facebook development redirect uri the authorization server will use to return the code to.
        /// 
        /// Use the code received to exchange for an access token.
        /// </summary>
        public const string FACEBOOK_REDIRECT_URI = "http://localhost:53748/Secure/OpenIdRpxHandler.aspx";

        /// <summary>
        /// The OAuth App Id of your facebook development project.
        /// </summary>
        public const string FACEBOOK_APP_ID = "504818862887045";

        /// <summary>
        /// The OAuth App Secret of your facebook development project.
        /// </summary>
        public const string FACEBOOK_APP_SECRET = "59abf8af730dac1eeb9636ace58cad4c";
#else
        /// <summary>
        /// The OAuth facebook production redirect uri the authorization server will use to return the code to.
        /// 
        /// Use the code received to exchange for an access token.
        /// </summary>
        // public const string FACEBOOK_REDIRECT_URI = "http://simsip.com/Secure/OpenIdRpxHandler.aspx";
        public const string FACEBOOK_REDIRECT_URI = "https://simsip.azure-mobile.net/";

        /// <summary>
        /// The OAuth App Id of your facebook production project.
        /// </summary>
        public const string FACEBOOK_APP_ID = "522929554418822";

        /// <summary>
        /// The OAuth App Secret of your facebook production project.
        /// </summary>
        public const string FACEBOOK_APP_SECRET = "5803c29d2c01b6058e56aea01bc41a69";
        /*
        5803
        c29d
        2c01
        b605
        8e56
        aea0
        1bc4
        1a69"
        */
#endif
        /// <summary>
        /// The OAuth facebook authorization server endpoint you will access to get the code you will use to exchange for
        /// an access token.
        /// </summary>
        public const string FACEBOOK_AUTHORIZATION_ENDPOINT = "https://m.facebook.com/dialog/oauth";

        /// <summary>
        /// The OAuth facebook authorization server endpoint you will access to exchange the code you have received
        /// for an access token.
        /// </summary>
        public const string FACEBOOK_TOKEN_ENDPOINT = "https://graph.facebook.com/oauth/access_token";

        /// <summary>
        /// The complete list of facebook scopes we require.
        /// 
        /// email:
        /// Provides access to the user's primary email address in the email property. 
        /// Do not spam users. Your use of email must comply both with Facebook policies and with the CAN-SPAM Act.
        /// Note: There is no way for apps to obtain email addresses for a user's friends.
        /// 
        /// user_photos:
        /// Provides access to the photos the user has uploaded, and photos the user has been tagged in.
        /// 
        /// friends_photos:
        /// Provides access to the photos the user's friends have uploaded, and photos the user's friends have been tagged in.
        /// 
        /// user_groups:
        /// Provides access to the list of groups the user is a member of as the groups connection.
        /// 
        /// friends_groups:
        /// Provides access to the list of groups the user's friends is a member of as the groups connection.
        /// </summary>
        public const string FACEBOOK_SCOPES = "email user_photos user_videos user_groups user_subscriptions";

        #endregion

        #region Youtube

        /// <summary>
        /// The OAuth youtube authorization server endpoint you will access to get the code you will use to exchange for
        /// an access token.
        /// </summary>
        public const string YOUTUBE_AUTHORIZATION_ENDPOINT = "https://accounts.google.com/o/oauth2/auth";

        /// <summary>
        /// The OAuth youtube authorization server endpoint you will access to exchange the code you have received
        /// for an access token.
        /// </summary>
        public const string YOUTUBE_TOKEN_ENDPOINT = "https://accounts.google.com/o/oauth2/token";

#if DEV
        /// <summary>
        /// The OAuth youtube development redirect uri the authorization server will use to return the code to.
        /// 
        /// Use the code received to exchange for an access token.
        /// </summary>
        public const string YOUTUBE_REDIRECT_URI = "http://localhost:53748/securesimsip/youtubehandler.aspx";
#else
        /// <summary>
        /// The OAuth youtube production redirect uri the authorization server will use to return the code to.
        /// 
        /// Use the code received to exchange for an access token.
        /// </summary>
        public const string YOUTUBE_REDIRECT_URI = "http://www.simsip.com/securesimsip/youtubehandler.aspx";

#endif

        /// <summary>
        /// The OAuth App Id of your youtube project.
        /// </summary>
        public const string YOUTUBE_APP_ID = "771968873764.apps.googleusercontent.com";

        /// <summary>
        /// The OAuth App Secret of your youtube project.
        /// </summary>
        public const string YOUTUBE_APP_SECRET = "D5iQANtmZzz9-lvzo8viweNF";

        /*
        D5i
        QAN
        tmZ
        zz9
        -lv
        zo8
        viw
        eNF
        */
        /// <summary>
        /// The complete list of youtube scopes we require.
        /// 
        /// https://www.googleapis.com/auth/youtube:
        /// Manage your YouTube account
        /// 
        /// https://www.googleapis.com/auth/youtube.readonly:
        /// ViewDirection your YouTube account
        /// 
        /// https://www.googleapis.com/auth/youtube.upload:
        /// Manage your YouTube videos
        /// 
        /// TODO: Dupe of scope above
        /// https://www.googleapis.com/auth/youtubepartner:
        /// ViewDirection and manage your assets and associated content on YouTube
        /// </summary>
        public const string YOUTUBE_SCOPES = "https://www.googleapis.com/auth/youtube https://www.googleapis.com/auth/youtube.readonly https://www.googleapis.com/auth/youtube.upload https://www.googleapis.com/auth/youtube.upload";

        #endregion
    }
}
