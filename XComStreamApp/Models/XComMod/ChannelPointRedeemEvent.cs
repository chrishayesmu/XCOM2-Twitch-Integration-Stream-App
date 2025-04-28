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

        private string _viewerInput = "";
        [JsonPropertyName("user_input")]
        public string ViewerInput { get => _viewerInput; set => SafeStringSet(ref _viewerInput, value); }

        [JsonPropertyName("reward_id")]
        public string RewardId { get; set; } = "";

        private string _rewardTitle = "";
        [JsonPropertyName("reward_title")]
        public string RewardTitle { get => _rewardTitle; set => SafeStringSet(ref _rewardTitle, value); }
    }
}
