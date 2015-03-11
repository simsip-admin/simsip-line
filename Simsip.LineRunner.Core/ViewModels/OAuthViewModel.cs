using System;
using System.Collections.Generic;
using System.Globalization;
using Simsip.LineRunner.Data.OAuth;
using Simsip.LineRunner.Entities.OAuth;

namespace Simsip.LineRunner.ViewModels
{
    /// <summary>
    /// Important: We have only implemented the "Implicit Grant Flow" for the OAuth 2.0 spec.
    /// We have not yet implemented the "Authorization Code Grant Flow" - see TODO below.
    /// 
    /// Reference:
    /// http://msdn.microsoft.com/en-us/library/live/hh243647.aspx
    /// </summary>
    public class OAuthViewModel : BaseViewModel
    {
        private OAuthType _oauthType;
        private readonly IOAuthAccessRepository _oauthAccessRepository;
        private readonly IOAuthResultRepository _oAuthResultRepository;

        public const string OAUTH_TYPE_PARAMETER_KEY = "type";
        public const string OAUTH_TYPE_FACEBOOK = "facebook";
        public const string OAUTH_TYPE_GROUPME = "groupme";
        public const string OAUTH_TYPE_YOUTUBE = "youtube";

        public class Parameters
        {
            public OAuthType Type { get; set; }
        }

        public OAuthViewModel()
        {
            // Important: initialize IOAuthResultRepository before IOAuthAccessRespository 
            // to avoid possible race condition
            // TODO: Document what this race condition is
            _oAuthResultRepository = new OAuthResultRepository();
            _oauthAccessRepository = new OAuthAccessRepository();
        }

        public void Init(Parameters parameters)
        {
            _oauthType = parameters.Type;

            var oauthAccessModel = _oauthAccessRepository.Read(_oauthType);

            switch (_oauthType)
            {
                case OAuthType.Facebook:
                    {
                        var authorizationUrl = oauthAccessModel.AuthorizationEndpoint +
                            "?client_id=" + oauthAccessModel.AppId +
                            "&redirect_uri=https://www.facebook.com/connect/login_success.html" +
                            "&response_type=token" +
                            "&scope=" + Uri.EscapeUriString(oauthAccessModel.Scopes);
                        AuthorizationUri = new Uri(authorizationUrl);
                        RedirectUri = new Uri("https://www.facebook.com/connect/login_success.html");
                        break;
                    }
                case OAuthType.Groupme:
                    {
                        var authorizationUrl = oauthAccessModel.AuthorizationEndpoint;
                        AuthorizationUri = new Uri(authorizationUrl);
                        RedirectUri = new Uri(oauthAccessModel.RedirectUri);
                        break;
                    }
            }

            TokenUri = new Uri(oauthAccessModel.AuthorizationEndpoint);
            
        }

        private Uri _authorizationUri;
        public Uri AuthorizationUri
        {
            get { return _authorizationUri; }
            set { _authorizationUri = value; /* TODO: RaisePropertyChanged(() => AuthorizationUri); */ }
        }

        private Uri _tokenUri;
        public Uri TokenUri
        {
            get { return _tokenUri; }
            set { _tokenUri = value; /* TODO: RaisePropertyChanged(() => TokenUri); */ }
        }

        private Uri _redirectUri;
        public Uri RedirectUri
        {
            get { return _redirectUri; }
            set { _redirectUri = value; /* RaisePropertyChanged(() => RedirectUri); */ }
        }

        public void NavigatingCommand(Uri uri)
        {
            // Whenever the browser navigates to a new url, try parsing the url.
            // the url may be the result of OAuth 2.0 authentication.
            OAuthResultEntity oauthResult;
            if (TryParseOAuthCallbackUrl(uri, out oauthResult))
            {
                //
                // TODO: Add in analyis of OAuthResult.Error
                //

                //
                // TODO: At this point, to implement the "Authorization Code Grant Flow",
                // we would want to evalued the OAuthResult.Code and OAuthResult.AccessToken
                // values. Based on what we have or don't hav for these values, proceed
                // to implement the final step in the "Authorization Code Grant Flow".
                //
                // For instance, by using the Messenger to notify the Views browser
                // to navigate to the TokenUri.
                //
                // Reference: http://msdn.microsoft.com/en-us/library/live/hh243647.aspx
                //

                _oAuthResultRepository.Create(oauthResult);

                switch (_oauthType)
                {
                    case OAuthType.Facebook:
                        {
                            /* TODO
                            App.IsAuthenticated = true;
                            App.FacebookAccessToken = oauthResult.AccessToken;
                            */
                            break;
                        }
                    case OAuthType.Groupme:
                        {
                            // TODO
                            // App.GroupmeAccessToken = oauthResult.AccessToken;
                            break;
                        }
                    case OAuthType.Youtube:
                        {
                            break;
                        }
                }

                // TODO
                // Close(this);
            }
        }

        /// <summary>
        /// Try parsing the url to <see cref="OAuthResultModel"/>.
        /// </summary>
        /// <param name="url">The url to parse</param>
        /// <param name="oauthResult">The oauth result.</param>
        /// <returns>True if parse successful, otherwise false.</returns>
        private bool TryParseOAuthCallbackUrl(Uri url, out OAuthResultEntity oauthResult)
        {
            try
            {
                oauthResult = ParseOAuthCallbackUrl(url);

                if (oauthResult != null)
                {
                    return true;
                }

                return false;
            }
            catch(Exception ex)
            {
                oauthResult = null;
                return false;
            }
        }

        /// <summary>
        /// Parse the url to <see cref="OAuthResultModel"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>An <see cref="OAuthResultModel" containing values representing 
        /// the result of the OAuth transaction.</returns>
        private OAuthResultEntity ParseOAuthCallbackUrl(Uri uri)
        {
            var parameters = new Dictionary<string, string>();

            bool found = false;
            if (!string.IsNullOrEmpty(uri.Fragment))
            {
                // #access_token and expries_in are in fragment
                var fragment = uri.Fragment.Substring(1);
                parameters = ParseQueryString("?" + fragment);
                if (parameters.ContainsKey("access_token"))
                {
                    found = true;
                }
            }

            // code, state, error_reason, error and error_description are in query
            // ?error_reason=user_denied&error=access_denied&error_description=The+user+denied+your+request.
            var queryPart = ParseQueryString(uri.Query);

            bool hasCode = queryPart.ContainsKey("code");
            bool hasAccessToken = queryPart.ContainsKey("access_token");
            bool hasError = queryPart.ContainsKey("error") && queryPart.ContainsKey("error_description");
            if (hasCode || 
                hasAccessToken ||
                hasError)
            {
                found = true;
            }

            foreach (string key in queryPart.Keys)
            {
                parameters[key] = queryPart[key];
            }

            OAuthResultEntity oauthResultModel = null;
            if (found)
            {
                oauthResultModel = CreateOAuthResultModel(_oauthType, parameters);
            }

            return oauthResultModel;
        }

        private Dictionary<string, string> ParseQueryString(string str)
        {
            int questionMarkIndex = str.IndexOf('?');
            if (questionMarkIndex != -1)
            {
                string paramsString = str.Substring(questionMarkIndex + 1);

                int poundIndex = paramsString.IndexOf("#");
                if (poundIndex != -1)
                {
                    paramsString = paramsString.Substring(0, poundIndex);
                }

                string[] keyValues = paramsString.Split(new [] {'&'}, StringSplitOptions.RemoveEmptyEntries);
                var result = new Dictionary<string, string>();

                foreach (string keyValue in keyValues)
                {
                    string[] splitKeyValue = keyValue.Split(new [] {'='});
                    if (splitKeyValue.Length == 2 && !string.IsNullOrWhiteSpace(splitKeyValue[0]))
                    {
                        string unencodedKey = Uri.UnescapeDataString(splitKeyValue[0].Replace("+", "%20"));
                        string unencodedValue = Uri.UnescapeDataString(splitKeyValue[1].Replace("+", "%20"));
                        result[unencodedKey] = unencodedValue;
                    }
                }

                return result;
            }

            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthResultModel"/> class.
        /// </summary>
        /// <param name="type">The type of identity provider (e.g., Facebook, Groupme, Youtube, etc.</param>
        /// <param name="parameters">The parameters retrieved from an OAuth transaction. </param>
        /// <remarks>
        /// The values of parameters should not be url encoded.
        /// </remarks>
        private OAuthResultEntity CreateOAuthResultModel(OAuthType type, Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            var oauthResult = new OAuthResultEntity();

            oauthResult.Type = type;

            if (parameters.ContainsKey("state"))
            {
                oauthResult.State = parameters["state"];
            }

            if (parameters.ContainsKey("error"))
            {
                oauthResult.Error = parameters["error"];

                if (parameters.ContainsKey("error_reason"))
                {
                    oauthResult.ErrorReason = parameters["error_reason"];
                }

                if (parameters.ContainsKey("error_description"))
                {
                    oauthResult.ErrorDescription = parameters["error_description"];
                }

                return oauthResult;
            }

            if (parameters.ContainsKey("code"))
            {
                oauthResult.Code = parameters["code"];
            }

            if (parameters.ContainsKey("access_token"))
            {
                oauthResult.AccessToken = parameters["access_token"];
            }

            if (parameters.ContainsKey("expires_in"))
            {
                var expiresIn = Convert.ToDouble(parameters["expires_in"], CultureInfo.InvariantCulture);
                oauthResult.Expires = expiresIn > 0 ? DateTime.UtcNow.AddSeconds(expiresIn) : DateTime.MaxValue;
            }

            return oauthResult;
        }
    }
}
