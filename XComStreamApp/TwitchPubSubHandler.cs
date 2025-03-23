using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Communication.Interfaces;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Enums;
using TwitchLib.PubSub.Events;
using XComStreamApp.Models;
using XComStreamApp.Models.Twitch.Chat;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static XComStreamApp.Models.SystemEvent;
using static XComStreamApp.Models.Twitch.Chat.TwitchUser;

namespace XComStreamApp
{
    public class TwitchPubSubHandler
    {
        private TwitchPubSub _client;
        private ILogger<TwitchPubSubHandler> _logger;
        private XComStreamAppForm _appForm;

        public TwitchPubSubHandler(ILoggerFactory loggerFactory, XComStreamAppForm appForm)
        {
            _appForm = appForm;
            _client = new TwitchPubSub(loggerFactory.CreateLogger<TwitchPubSub>());
            _logger = loggerFactory.CreateLogger<TwitchPubSubHandler>();

            _client.OnListenResponse += OnListenResponse;
            _client.OnPubSubServiceConnected += OnPubSubServiceConnected;
            _client.OnPubSubServiceClosed += OnPubSubServiceClosed;
            _client.OnPubSubServiceError += OnPubSubServiceError;

            _client.OnChannelSubscription += OnChannelSubscription;
            _client.OnMessageDeleted += OnMessageDeleted;
        }

        private void OnChannelSubscription(object? sender, OnChannelSubscriptionArgs e)
        {
            var subbedUser = new TwitchUser()
            {
                UserId = e.Subscription.UserId,
                UserLogin = e.Subscription.Username,
                UserName = e.Subscription.DisplayName,
                SubTier = e.Subscription.SubscriptionPlan switch
                {
                    SubscriptionPlan.Prime => SubscriberTier.Tier1,
                    SubscriptionPlan.Tier1 => SubscriberTier.Tier1,
                    SubscriptionPlan.Tier2 => SubscriberTier.Tier2,
                    SubscriptionPlan.Tier3 => SubscriberTier.Tier3,
                    _ => SubscriberTier.None // shouldn't happen in this event
                },
                IsBroadcaster = e.Subscription.UserId == TwitchState.ConnectedUser.Id
            };

            TwitchState.Channel.SubscribersByUserId[subbedUser.UserId] = subbedUser;

            // Upsert this user into the chatters list, since subbing implies they're present in chat
            var existingUser = TwitchState.Channel.Chatters.FirstOrDefault(chatter => chatter.UserId == subbedUser.UserId);

            if (existingUser != null)
            {
                existingUser.SubTier = subbedUser.SubTier;
            }
            else
            {
                TwitchState.Channel.Chatters.Add(subbedUser);
            }
        }

        public void Connect()
        {
            // Add listen calls here instead of constructor because there won't necessarily be a connected user during construction
            _client.ListenToChatModeratorActions(TwitchState.ConnectedUser.Id, TwitchState.ConnectedUser.Id);
            _client.ListenToSubscriptions(TwitchState.ConnectedUser.Id);

            _client.Connect();
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        private void OnListenResponse(object? sender, OnListenResponseArgs e)
        {
            if (!e.Successful)
            {
                _logger.LogError("Error in PubSub listen: {err}", e.Response.Error);
            }
        }

        private void OnMessageDeleted(object? sender, OnMessageDeletedArgs e)
        {
            // We don't know for sure if this is a chat command; only the game knows which strings are commands.
            // But since it could be, we'll forward it to the game anyway.
            if (e.Message.StartsWith('!'))
            {
                _appForm.AddEvent(new SystemEvent()
                {
                    Type = EventType.ChatEvent,
                    Description = $"Potential chat command deleted by {e.DeletedBy}"
                });

                TwitchState.PendingGameEvents.Enqueue(new Models.XComMod.ChatDeletionEvent()
                {
                    MessageId = e.MessageId
                });
            }
        }

        private void OnPubSubServiceError(object? sender, OnPubSubServiceErrorArgs e)
        {
            _logger.LogError("PubSub service error occurred: {err}", e.Exception);

            _appForm.AddEvent(new SystemEvent()
            {
                Type = EventType.AppEvent,
                Description = "Error occurred in Twitch PubSub"
            });
        }

        private void OnPubSubServiceClosed(object? sender, EventArgs e)
        {
            _logger.LogInformation("PubSub service connection closed");
        }

        private void OnPubSubServiceConnected(object? sender, EventArgs e)
        {
            _logger.LogInformation("Connection established to PubSub service");

            _appForm.AddEvent(new SystemEvent()
            {
                Type = EventType.AppEvent,
                Description = "Connection established to Twitch PubSub"
            });

            _client.SendTopics(TwitchState.AccessToken);
        }
    }
}
