using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using XComStreamApp.Models.Twitch.Auth;
using XComStreamApp.Models.Twitch.Chat;
using static XComStreamApp.Models.Twitch.Chat.TwitchUser;

namespace XComStreamApp.Services
{
    /// <summary>
    /// Contains Twitch Helix APIs for situations where the ones in Twitchlib are lacking. Also sometimes wraps Twitchlib
    /// calls for convenience.
    /// </summary>
    public class TwitchApiService
    {
        private static HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Retrieves all chatters currently connected to a broadcaster's chat room.
        /// </summary>
        /// <returns>All connected chatters.</returns>
        /// <remarks>This is used because Twitchlib.Helix.GetChattersAsync doesn't return users' display names.</remarks>
        public static async Task<List<TwitchUser>> GetChatters(string broadcasterId, string accessToken, string clientId)
        {
            string? paginationCursor = null;
            bool isInitialRequest = true;
            var chatters = new List<TwitchUser>();
            int numRetries = 0;

            while (isInitialRequest || !string.IsNullOrEmpty(paginationCursor))
            {
                isInitialRequest = false;

                string url = $"https://api.twitch.tv/helix/chat/chatters?broadcaster_id={broadcasterId}&moderator_id={broadcasterId}&first=1";

                if (paginationCursor != null)
                {
                    url += $"&after={paginationCursor}";
                }

                var httpMessage = new HttpRequestMessage(HttpMethod.Get, url);
                httpMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpMessage.Headers.Add("Client-Id", clientId);

                try
                {
                    var httpResponse = await httpClient.SendAsync(httpMessage);
                    var getChattersResponse = await httpResponse.Content.ReadFromJsonAsync<GetChattersResponse>();

                    chatters.AddRange(getChattersResponse!.Data);
                    paginationCursor = getChattersResponse.Pagination?.Cursor;
                }
                catch (Exception e)
                {
                    // TODO inject a logger
                    // logger.LogError("Exception occurred while getting chatters: {e}", e);

                    numRetries++;

                    if (numRetries > 3)
                    {
                        throw;
                    }

                    Thread.Sleep(1000);
                }
            }

            return chatters;
        }

        public static async Task<List<TwitchUser>> GetSubscribers(string broadcasterId)
        {
            if (TwitchState.API == null)
            {
                throw new Exception("TwitchState.API is null!");
            }

            string? paginationCursor = null;
            bool isInitialRequest = true;
            var allSubs = new List<TwitchUser>();

            while (isInitialRequest || !string.IsNullOrEmpty(paginationCursor))
            {
                isInitialRequest = false;

                var subsResponse = await TwitchState.API.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(broadcasterId, first: 100, after: paginationCursor);

                paginationCursor = subsResponse.Pagination.Cursor;

                var users = subsResponse.Data.Select(sub => new TwitchUser()
                {
                    UserId = sub.UserId,
                    UserLogin = sub.UserLogin,
                    UserName = sub.UserName,
                    SubTier = sub.Tier switch {
                        "1000" => SubscriberTier.Tier1,
                        "2000" => SubscriberTier.Tier2,
                        "3000" => SubscriberTier.Tier3,
                        _ => SubscriberTier.None
                    },
                    IsBroadcaster = sub.UserId == TwitchState.ConnectedUser.Id
                });

                allSubs.AddRange(users);
            }

            return allSubs;
        }

        public static async Task<GetAccessTokenResponse?> RefreshAccessToken(string refreshToken, string clientId)
        {
            var formParams = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            var httpResponse = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(formParams));

            if (httpResponse.IsSuccessStatusCode)
            {
                var accessTokenResponse = await httpResponse.Content.ReadFromJsonAsync<GetAccessTokenResponse>();

                if (accessTokenResponse != null && !string.IsNullOrEmpty(accessTokenResponse.AccessToken) && !string.IsNullOrEmpty(accessTokenResponse.RefreshToken))
                {
                    return accessTokenResponse;
                }
            }

            // TODO log error info
            return null;
        }
    
        /// <summary>
        /// Revokes an access token, making it unusable in the future.
        /// </summary>
        /// <param name="accessToken">An OAuth2 access token to revoke.</param>
        /// <param name="clientId">The client ID that generated the token.</param>
        public static async Task RevokeAccessToken(string accessToken, string clientId)
        {
            var formParams = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "token", accessToken }
            };

            await httpClient.PostAsync("https://id.twitch.tv/oauth2/revoke", new FormUrlEncodedContent(formParams));
        }
    }
}
