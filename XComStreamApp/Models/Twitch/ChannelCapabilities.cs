using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.Twitch
{
    public class ChannelCapabilities
    {
        [JsonPropertyName("receive_bits")]
        public bool CanReceiveBits { get; set; }

        [JsonPropertyName("run_polls")]
        public bool CanRunPolls { get; set; }
    }
}
