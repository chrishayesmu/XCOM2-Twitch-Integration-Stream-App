using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.XComMod
{
    public class ChatCommandEvent : GameEvent
    {
        /// <summary>
        /// The login of the viewer who sent the chat command.
        /// </summary>
        [JsonPropertyName("user_login")]
        public string ViewerLogin { get; set; } = "";

        /// <summary>
        /// The command itself, e.g. if a viewer does "!xsay", then this would be "xsay".
        /// </summary>
        [JsonPropertyName("command")]
        public string Command { get; set; } = "";

        /// <summary>
        /// The body of the command, e.g. if the chat message is "!xsay some text",  then this would be "some text".
        /// </summary>
        [JsonPropertyName("body")]
        public string Body { get; set; } = "";

        /// <summary>
        /// The unique ID of the message, which may be needed if the message is later deleted.
        /// </summary>
        [JsonPropertyName("message_id")]
        public string MessageId { get; set; } = "";

        [JsonPropertyName("emote_data")]
        public IEnumerable<ChatEmoteData> EmoteData { get; set; } = [];
    }
}
