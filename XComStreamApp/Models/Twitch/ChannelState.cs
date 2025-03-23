using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Chat.GetChatters;
using TwitchLib.Api.Helix.Models.Polls;
using XComStreamApp.Models.Twitch.Chat;

namespace XComStreamApp.Models.Twitch
{
    public class ChannelState
    {
        public ChannelInformation? ChannelInfo;

        /// <summary>
        /// Users who are currently connected to the channel's chat.
        /// </summary>
        public IList<TwitchUser> Chatters = [];

        /// <summary>
        /// Users who are subscribed to this channel. These users may not be present in the chat currently.
        /// </summary>
        public IDictionary<string, TwitchUser> SubscribersByUserId = new Dictionary<string, TwitchUser>();

        /// <summary>
        /// Whether the channel has the XCom Twitch extension enabled.
        /// </summary>
        public bool HasExtensionEnabled = false;

        /// <summary>
        /// The poll currently running on this channel, if any.
        /// </summary>
        /// TODO: need our own poll object, and to track whether the poll was created by us
        public Poll? CurrentPoll;
    }
}
