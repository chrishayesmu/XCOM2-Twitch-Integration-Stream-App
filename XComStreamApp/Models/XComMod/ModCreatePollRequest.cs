using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.XComMod
{
    public class ModCreatePollRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("choices")]
        public string[] Choices { get; set; } = [];

        [JsonPropertyName("channel_points_per_vote")]
        public int ChannelPointsPerVote { get; set; }

        [JsonPropertyName("duration")]
        public int DurationInSeconds { get; set; }
    }
}
