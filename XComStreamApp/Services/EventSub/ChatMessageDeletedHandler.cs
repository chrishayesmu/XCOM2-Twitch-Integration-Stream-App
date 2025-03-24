using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.Handler;
using TwitchLib.EventSub.Websockets.Core.Models;
using static XComStreamApp.Models.SystemEvent;
using XComStreamApp.Models;

namespace XComStreamApp.Services.EventSub
{
    public class ChatMessageDeletedHandler(ILogger<ChatMessageDeletedHandler> logger) : INotificationHandler
    {
        public string SubscriptionType => "channel.chat.message_delete";

        public void Handle(EventSubWebsocketClient client, string jsonString, JsonSerializerOptions serializerOptions)
        {
            var data = JsonSerializer.Deserialize<EventSubNotification<ChannelChatMessageDelete>>(jsonString.AsSpan(), serializerOptions);

            if (data is null)
            {
                throw new InvalidOperationException("Parsed JSON cannot be null!");
            }

            // We don't know if this message was a chat command, but since it could be, we'll forward it to the game anyway
            TwitchState.PendingGameEvents.Enqueue(new Models.XComMod.ChatDeletionEvent()
            {
                MessageId = data.Payload.Event.MessageId
            });
        }
    }
}
