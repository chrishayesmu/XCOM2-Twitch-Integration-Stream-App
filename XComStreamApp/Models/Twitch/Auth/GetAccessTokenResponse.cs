using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.Twitch.Auth
{
    public class GetAccessTokenResponse
    {
        // The authenticated token, to be used for various API endpoints and EventSub subscriptions.
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = "";

        // Time until the code is no longer valid (in seconds).
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public IList<string> Scope { get; set; } = [];

        // A token used to refresh the access token.
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = "";

        // Will generally be “bearer”.
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "";
    }
}
