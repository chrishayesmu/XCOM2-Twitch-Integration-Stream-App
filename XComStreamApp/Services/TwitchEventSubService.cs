using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TwitchLib.Api.Core.Enums;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;
using static XComStreamApp.Models.Twitch.Chat.TwitchUser;
using XComStreamApp.Models.Twitch.Chat;
using XComStreamApp.Models;
using XComStreamApp.Models.XComMod;

namespace XComStreamApp.Services
{
    public class TwitchEventSubService : IHostedService
    {
        private readonly ILogger<TwitchEventSubService> _logger;
        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

        private System.Timers.Timer connectToEventSubTimer = new System.Timers.Timer(TimeSpan.FromSeconds(3));

        private bool _wasDisconnectRequested;

        public TwitchEventSubService(ILogger<TwitchEventSubService> logger, EventSubWebsocketClient eventSubWebsocketClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _eventSubWebsocketClient = eventSubWebsocketClient ?? throw new ArgumentNullException(nameof(eventSubWebsocketClient));
            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;
            _eventSubWebsocketClient.ChannelPointsCustomRewardRedemptionAdd += OnChannelPointsRewardRedeemed;

            _eventSubWebsocketClient.ChannelSubscribe += OnChannelSubscribe;

            connectToEventSubTimer.AutoReset = false;
            connectToEventSubTimer.Elapsed += TryConnectToEventSub;
        }

        private async Task OnChannelPointsRewardRedeemed(object sender, ChannelPointsCustomRewardRedemptionArgs args)
        {
            var e = args.Notification.Payload.Event;

            if (TwitchState.ConnectedUser == null || TwitchState.Channel == null)
            {
                return;
            }

            Program.Form.AddEvent(new SystemEvent() 
            { 
                Description = $"{e.UserName} redeemed \"{e.Reward.Title}\" (reward ID: {e.Reward.Id})",
                Timestamp = DateTime.Now,
                TwitchName = e.UserName,
                Type = SystemEvent.EventType.ChatEvent
            });

            TwitchState.PendingGameEvents.Enqueue(new ChannelPointRedeemEvent()
            {
                ViewerLogin = e.UserLogin,
                ViewerName = e.UserName,
                ViewerInput = e.UserInput,
                RewardId = e.Reward.Id,
                RewardTitle = e.Reward.Title
            });
        }

        public async Task DisconnectFromTwitch()
        {
            _wasDisconnectRequested = true;
            await _eventSubWebsocketClient.DisconnectAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting WebsocketHostedService");
            connectToEventSubTimer.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping WebsocketHostedService");
            await _eventSubWebsocketClient.DisconnectAsync();
        }

        private async Task OnChannelSubscribe(object sender, ChannelSubscribeArgs args)
        {
            if (TwitchState.ConnectedUser == null || TwitchState.Channel == null)
            {
                return;
            }

            var e = args.Notification.Payload.Event;

            var subbedUser = new TwitchUser()
            {
                UserId = e.UserId,
                UserLogin = e.UserLogin,
                UserName = e.UserName,
                SubTier = e.Tier switch
                {
                    "1000" => SubscriberTier.Tier1,
                    "2000" => SubscriberTier.Tier2,
                    "3000" => SubscriberTier.Tier3,
                    _ => SubscriberTier.None // shouldn't happen in this event
                },
                IsBroadcaster = e.UserId == TwitchState.ConnectedUser.Id
            };

            TwitchState.Channel.SubscribersByUserId[subbedUser.UserId] = subbedUser;

            // Update this user's sub tier, if they're in chat. (They might not be, e.g. if they're subscribing from a console device)
            var existingUser = TwitchState.Channel.Chatters.FirstOrDefault(chatter => chatter.UserId == subbedUser.UserId);

            if (existingUser != null)
            {
                existingUser.SubTier = subbedUser.SubTier;
            }
        }

        private async void TryConnectToEventSub(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (TwitchState.API == null || TwitchState.ConnectedUser == null)
            {
                connectToEventSubTimer.Start();
                return;
            }

            if (!await _eventSubWebsocketClient.ConnectAsync())
            {
                connectToEventSubTimer.Start();
            }
        }

        private async Task OnWebsocketConnected(object? sender, WebsocketConnectedArgs e)
        {
            _logger.LogInformation("Websocket {SessionId} connected!", _eventSubWebsocketClient.SessionId);

            Program.Form.AddEvent(new SystemEvent()
            {
                Description = "Connection established to EventSub",
                Timestamp = DateTime.Now,
                Type = SystemEvent.EventType.AppEvent
            });

            if (!e.IsRequestedReconnect)
            {
                if (TwitchState.API == null || TwitchState.ConnectedUser == null)
                {
                    // The user must have logged out between our connection request and now;
                    // go back to checking on a timer
                    await _eventSubWebsocketClient.DisconnectAsync();
                    connectToEventSubTimer.Start();
                    return;
                }

                var conditions = new Dictionary<string, string>()
                {
                    { "broadcaster_user_id", TwitchState.ConnectedUser.Id }
                };

                await TwitchState.API.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscribe", "1", conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                await TwitchState.API.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.channel_points_custom_reward_redemption.add", "1", conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);

                conditions.Add("user_id", TwitchState.ConnectedUser.Id);
                await TwitchState.API.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.chat.message_delete", "1", conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
            }
        }

        private async Task OnWebsocketDisconnected(object? sender, EventArgs e)
        {
            _logger.LogInformation("Websocket {SessionId} disconnected!", _eventSubWebsocketClient.SessionId);

            // Check if this was a deliberate disconnect
            if (_wasDisconnectRequested)
            {
                _wasDisconnectRequested = false;
                connectToEventSubTimer.Start();
                return;
            }

            int numRetries = 0;

            while (!await _eventSubWebsocketClient.ReconnectAsync())
            {
                numRetries++;

                // Exponential backoff, maxing out at 2^5 = 32 seconds between retries
                int delayMs = 1000 * (int) Math.Pow(2, Math.Max(numRetries, 5));
                _logger.LogError("Websocket reconnect failed! Trying again in {delayMs} milliseconds. {numRetries} retries so far.", delayMs, numRetries);

                Program.Form.AddEvent(new SystemEvent()
                {
                    Description = "Failed to reconnect to EventSub",
                    Timestamp = DateTime.Now,
                    Type = SystemEvent.EventType.AppEvent
                });

                await Task.Delay(1000);
            }
        }

        private async Task OnWebsocketReconnected(object? sender, EventArgs e)
        {
            _logger.LogWarning("Websocket {SessionId} reconnected", _eventSubWebsocketClient.SessionId);
        }

        private async Task OnErrorOccurred(object? sender, ErrorOccuredArgs e)
        {
            _logger.LogError("Websocket {SessionId} - Error occurred! Exception: {e}", _eventSubWebsocketClient.SessionId, e);

            Program.Form.AddEvent(new SystemEvent()
            {
                Description = "Error occurred with EventSub",
                Timestamp = DateTime.Now,
                Type = SystemEvent.EventType.AppEvent
            });
        }
    }
}