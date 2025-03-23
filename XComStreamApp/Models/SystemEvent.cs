using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XComStreamApp.Models
{
    public class SystemEvent
    {
        public enum EventType
        {
            /// <summary>
            /// Events triggered by a viewer saying something in Twitch chat.
            /// </summary>
            ChatEvent,

            /// <summary>
            /// Events triggered by a viewer interacting with the Twitch extension.
            /// </summary>
            ExtensionEvent,

            /// <summary>
            /// Events triggered by the XComStreamApp itself.
            /// </summary>
            AppEvent,

            /// <summary>
            /// Events triggered by XCOM 2.
            /// </summary>
            GameEvent
        }

        /// <summary>
        /// What type of event this is.
        /// </summary>
        public EventType Type;

        /// <summary>
        /// The name of the Twitch viewer who initiated this event. Will be null unless <see cref="Type"/>
        /// is either <see cref="EventType.ChatEvent"/>  or <see cref="EventType.ExtensionEvent"/>.
        /// </summary>
        public string? TwitchName;

        /// <summary>
        /// Description of the event to display to the app's user.
        /// </summary>
        public string Description = "";

        /// <summary>
        /// When this event occurred.
        /// </summary>
        public DateTime Timestamp = DateTime.Now;
    }
}
