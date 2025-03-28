using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.XComMod
{
    public class ChannelPointRedeemEvent : GameEvent
    {
        [JsonPropertyName("user_login")]
        public string ViewerLogin { get; set; } = "";

        [JsonPropertyName("user_name")]
        public string ViewerName { get; set; } = "";

        [JsonPropertyName("user_input")]
        public string ViewerInput { get; set; } = "";

        [JsonPropertyName("reward_id")]
        public string RewardId { get; set; } = "";

        [JsonPropertyName("reward_title")]
        public string RewardTitle { get; set; } = "";
    }
}
