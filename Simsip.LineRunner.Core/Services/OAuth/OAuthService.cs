using Simsip.LineRunner.Data.OAuth;
using Simsip.LineRunner.Entities.OAuth;

namespace Simsip.LineRunner.Services.OAuth
{
    public class OAuthService
        : IOAuthService
    {
        private readonly IOAuthAccessRepository _accessRepository;
        private readonly IOAuthResultRepository _resultRepository;


        public OAuthService()
        {
            _accessRepository = new OAuthAccessRepository();
            _resultRepository = new OAuthResultRepository();
        }

        public void InitializeAccessModels()
        {
            // Facebook
            var accessFacebook = new OAuthAccessEntity();
            accessFacebook.Type = OAuthType.Facebook;
            accessFacebook.AuthorizationEndpoint = OAuthCredentials.FACEBOOK_AUTHORIZATION_ENDPOINT;
            accessFacebook.TokenEndpoint = OAuthCredentials.FACEBOOK_TOKEN_ENDPOINT;
            accessFacebook.RedirectUri = OAuthCredentials.FACEBOOK_REDIRECT_URI;
            accessFacebook.AppId = OAuthCredentials.FACEBOOK_APP_ID;
            // accessFacebook.AppSecret = OAuthCredentials.FACEBOOK_APP_SECRET;
            accessFacebook.Scopes = OAuthCredentials.FACEBOOK_SCOPES;
            _accessRepository.Create(accessFacebook);

            // Groupme
            var accessGroupme = new OAuthAccessEntity();
            accessGroupme.Type = OAuthType.Groupme;
            accessGroupme.AuthorizationEndpoint = OAuthCredentials.GROUPME_AUTHORIZATION_ENDPOINT;
            accessGroupme.RedirectUri = OAuthCredentials.GROUPME_REDIRECT_URI;
            accessGroupme.AppId = OAuthCredentials.GROUPME_APP_ID;
            _accessRepository.Create(accessGroupme);

            // Youtube
            var accessYoutube = new OAuthAccessEntity();
            accessYoutube.Type = OAuthType.Youtube;
            accessYoutube.AuthorizationEndpoint = OAuthCredentials.YOUTUBE_AUTHORIZATION_ENDPOINT;
            accessYoutube.TokenEndpoint = OAuthCredentials.YOUTUBE_TOKEN_ENDPOINT;
            accessYoutube.RedirectUri = OAuthCredentials.YOUTUBE_REDIRECT_URI;
            accessYoutube.AppId = OAuthCredentials.YOUTUBE_APP_ID;
            // accessYoutube.AppSecret = OAuthCredentials.YOUTUBE_APP_SECRET;
            accessYoutube.Scopes = OAuthCredentials.YOUTUBE_SCOPES;
            _accessRepository.Create(accessYoutube);
        }
    }
}
