using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.XComMod
{
    public class ChatDeletionEvent : GameEvent
    {
        [JsonPropertyName("message_id")]
        public string MessageId { get; set; } = "";
    }
}
