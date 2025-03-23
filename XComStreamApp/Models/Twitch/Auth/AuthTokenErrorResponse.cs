using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.Twitch.Auth
{
    public class AuthTokenErrorResponse
    {
        public bool IsAuthorizationPending => Message == "authorization_pending";

        public bool IsDeviceCodeInvalid => Message == "invalid device code";

        public bool IsRefreshTokenInvalid => Message == "Invalid refresh token";

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }
}
