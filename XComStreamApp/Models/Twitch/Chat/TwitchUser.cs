using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.Twitch.Chat
{
    /// <summary>
    /// Model based on multiple APIs, including https://dev.twitch.tv/docs/api/reference/#get-chatters
    /// and https://dev.twitch.tv/docs/api/reference/#get-broadcaster-subscriptions
    /// </summary>
    public class TwitchUser
    {
        public enum SubscriberTier
        {
            None,
            Tier1,
            Tier2,
            Tier3
        }

        // The ID of a user that’s connected to the broadcaster’s chat room.
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = "";

        // The user’s login name.
        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; } = "";

        // The user’s display name.
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = "";

        // What tier of subscription this user has to the broadcaster, if any.
        [JsonPropertyName("sub_tier")]
        public SubscriberTier SubTier { get; set; } = SubscriberTier.None;

        // Whether this user is the broadcaster who's currently authenticated in the app.
        [JsonPropertyName("is_broadcaster")]
        public bool IsBroadcaster { get; set; }
    }
}
