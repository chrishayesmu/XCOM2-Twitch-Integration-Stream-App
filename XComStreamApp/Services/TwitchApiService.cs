#if DEBUG
#define MOCK_CHATTERS
#endif

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Core.Exceptions;
using XComStreamApp.Models.Twitch.Auth;
using XComStreamApp.Models.Twitch.Chat;
using static XComStreamApp.Models.Twitch.Chat.TwitchUser;

namespace XComStreamApp.Services
{
    /// <summary>
    /// Contains Twitch Helix APIs for situations where the ones in Twitchlib are lacking. Also sometimes wraps Twitchlib
    /// calls for convenience.
    /// </summary> 
    public class TwitchApiService(ILogger<TwitchApiService> logger)
    {
        private static HttpClient httpClient = new HttpClient();

#if MOCK_CHATTERS
        private static Random RandomGen = new Random();
#endif

        /// <summary>
        /// Retrieves all chatters currently connected to a broadcaster's chat room.
        /// </summary>
        /// <returns>All connected chatters.</returns>
        /// <remarks>This is used because Twitchlib.Helix.GetChattersAsync doesn't return users' display names.</remarks>
        public async Task<List<TwitchUser>> GetChatters(string broadcasterId, string accessToken, string clientId)
        {
#if MOCK_CHATTERS
            var chatters = new List<TwitchUser>();

            for (int i = 0; i < 10; i++)
            {
                var userLogin = GenerateString("123456789abcdefghijklmnopqrstuvwxyz_", 5, 30);

                if (i == 0)
                {
                    userLogin = "레비아탄12";
                }
                else if (i == 1)
                {
                    userLogin = "她是我在Bilibili上最喜欢的直播";
                }

                chatters.Add(new TwitchUser() { 
                    SubTier = RandomGen.Next(100) < 10 ? SubscriberTier.Tier1 : SubscriberTier.None,
                    IsBroadcaster = false,
                    UserId = GenerateString("123456789", 8, 10),
                    UserLogin = userLogin,
                    UserName = userLogin
                });
            }

            return chatters;
#else
            string? paginationCursor = null;
            bool isInitialRequest = true;
            var chatters = new List<TwitchUser>();
            int numPages = 0, numRetries = 0;

            logger.LogInformation("GetChatters start");

            while (isInitialRequest || !string.IsNullOrEmpty(paginationCursor))
            {
                string url = $"https://api.twitch.tv/helix/chat/chatters?broadcaster_id={broadcasterId}&moderator_id={broadcasterId}&first=1000";

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

                    numPages++;
                    isInitialRequest = false;
                }
                catch (TooManyRequestsException e)
                {
                    int waitDurationMilliseconds = CalculateTimeUntilNextRequest(e);
                    logger.LogWarning("Too many requests have been made. Waiting {waitDuration}ms to try again.", waitDurationMilliseconds);
                    await Task.Delay(waitDurationMilliseconds);
                }
                catch (Exception e)
                {
                    logger.LogError("Exception occurred while getting chatters: {e}", e);

                    numRetries++;

                    if (numRetries > 5)
                    {
                        throw;
                    }

                    await Task.Delay(250);
                }
            }

            logger.LogInformation("Retrieved {numChatters} chatters across {numPages} pages", chatters.Count, numPages);
            return chatters;
#endif
        }

        public async Task<List<TwitchUser>> GetSubscribers(string broadcasterId)
        {
            if (TwitchState.API == null)
            {
                throw new Exception("TwitchState.API is null!");
            }

            logger.LogInformation("GetSubscribers start");

            string? paginationCursor = null;
            bool isInitialRequest = true;
            var allSubs = new List<TwitchUser>();
            int numPages = 0, numRetries = 0;

            while (isInitialRequest || !string.IsNullOrEmpty(paginationCursor))
            {
                try
                {
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

                    numPages++;
                    isInitialRequest = false;
                }
                catch (TooManyRequestsException e)
                {
                    int waitDurationMilliseconds = CalculateTimeUntilNextRequest(e);
                    logger.LogWarning("Too many requests have been made. Waiting {waitDuration}ms to try again.", waitDurationMilliseconds);
                    await Task.Delay(waitDurationMilliseconds);
                }
                catch (Exception e)
                {
                    logger.LogError("Exception occurred while getting subscribers: {e}", e);

                    numRetries++;

                    if (numRetries > 5)
                    {
                        throw;
                    }

                    await Task.Delay(250);
                }
            }

            logger.LogInformation("Retrieved {numSubscribers} subscribers across {numPages} pages", allSubs.Count, numPages);
            return allSubs;
        }

        public async Task<GetAccessTokenResponse?> RefreshAccessToken(string refreshToken, string clientId)
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

            logger.LogError("Failed to refresh access token");

            return null;
        }
    
        /// <summary>
        /// Revokes an access token, making it unusable in the future.
        /// </summary>
        /// <param name="accessToken">An OAuth2 access token to revoke.</param>
        /// <param name="clientId">The client ID that generated the token.</param>
        public async Task RevokeAccessToken(string accessToken, string clientId)
        {
            var formParams = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "token", accessToken }
            };

            await httpClient.PostAsync("https://id.twitch.tv/oauth2/revoke", new FormUrlEncodedContent(formParams));
        }

        private int CalculateTimeUntilNextRequest(TooManyRequestsException e)
        {
            double? resetTimestamp = e.Data["Ratelimit-Reset"] as double?;

            if (resetTimestamp == null)
            {
                return 1000;
            }

            var resetDateTime = DateTimeOffset.FromUnixTimeSeconds((long) resetTimestamp).UtcDateTime;

            if (resetDateTime < DateTime.UtcNow)
            {
                return 1000;
            }

            return Math.Min(60000, (int) (DateTime.UtcNow - resetDateTime).TotalMilliseconds);
        }

#if MOCK_CHATTERS
        private string GenerateString(string allowedChars, int minLength, int maxLength)
        {
            int length = minLength + RandomGen.Next(maxLength - minLength + 1);
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[RandomGen.Next(allowedChars.Length)];
            }

            return new string(chars);
        }
#endif
    }
}
