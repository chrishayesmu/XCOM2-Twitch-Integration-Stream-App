using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.Twitch.Chat
{
    public class GetChattersResponse
    {
        [JsonPropertyName("data")]
        public IList<TwitchUser> Data { get; set; } = [];

        [JsonPropertyName("pagination")]
        public Pagination? Pagination { get; set; } = null;

        [JsonPropertyName("total")]
        public int Total;
    }
}
