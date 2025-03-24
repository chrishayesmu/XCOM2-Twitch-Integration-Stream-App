using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Client;
using XComStreamApp.Models.Twitch;
using XComStreamApp.Models.XComMod;

namespace XComStreamApp
{
    public class TwitchState
    {
        /// <summary>
        /// Client ID which identifies the XCOM 2 Stream Companion app. This is a public client,
        /// and the ID does not need to be protected.
        /// </summary>
        public const string AppClientId = "uar5x1say96x1a0n4djlkpwjr2y02w";

        /// <summary>
        /// Authorization code which can be used to get an access token and a refresh token from Twitch.
        /// </summary>
        public static string AuthCode = "";

        /// <summary>
        /// Token which can be used to authenticate as the broadcaster and perform actions with the Twitch API.
        /// </summary>
        public static string AccessToken = "";

        /// <summary>
        /// Token which can be used to retrieve a new access token when it expires.
        /// </summary>
        public static string RefreshToken = "";

        /// <summary>
        /// The user who is currently connected to Twitch via this application.
        /// </summary>
        public static User? ConnectedUser = null;

        /// <summary>
        /// An instance of <see cref="TwitchAPI"/> which is already set up for convenience. May be null if the
        /// user is not authenticated.
        /// </summary>
        public static TwitchAPI? API = null;

        /// <summary>
        /// An instance of <see cref="TwitchClient"/> which is already set up for convenience. May be null if the
        /// user is not authenticated.
        /// </summary>
        public static TwitchClient? ChatClient = null;

        /// <summary>
        /// The current state of the channel we're connected to, if any.
        /// </summary>
        public static ChannelState? Channel = null;

        /// <summary>
        /// Events which are waiting to be processed by the game.
        /// </summary>
        public static ConcurrentQueue<GameEvent> PendingGameEvents = new ConcurrentQueue<GameEvent>();
    }
}
