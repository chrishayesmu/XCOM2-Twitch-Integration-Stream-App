using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.Twitch.Auth
{
    /// <summary>
    /// Models the response from Twitch when retrieving a device code from https://id.twitch.tv/oauth2/device.
    /// 
    /// See: https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#device-code-grant-flow
    /// </summary>
    public class GetDeviceCodeResponse
    {
        // The identifier for a given user.
        [JsonPropertyName("device_code")]
        public string DeviceCode { get; set; } = "";

        // Time until the code is no longer valid (in seconds).
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        // Time until another valid code can be requested (in seconds).
        [JsonPropertyName("interval")]
        public int Interval { get; set; }

        // The code that the user will use to authenticate.
        [JsonPropertyName("user_code")]
        public string UserCode { get; set; } = "";

        // The address you will send users to, to authenticate.
        [JsonPropertyName("verification_uri")]
        public string VerificationUri { get; set; } = "";
    }
}
