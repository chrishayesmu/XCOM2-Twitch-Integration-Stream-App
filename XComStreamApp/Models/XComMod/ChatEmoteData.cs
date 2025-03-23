using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.XComMod
{
    public class ChatEmoteData
    {
        [JsonPropertyName("start_index")]
        public int StartIndex { get; set; }

        [JsonPropertyName("end_index")]
        public int EndIndex { get; set; }

        [JsonPropertyName("emote_code")]
        public string EmoteCode { get; set; } = "";
    }
}
