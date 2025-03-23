using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Timers;
using System.Web;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using XComStreamApp.Models;
using XComStreamApp.Models.Twitch.Auth;
using XComStreamApp.Models.XComMod;
using XComStreamApp.Services;
using static XComStreamApp.Models.SystemEvent;

namespace XComStreamApp
{
    public partial class XComStreamAppForm : Form
    {
        private readonly string EmoteFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                                                               "my games", 
                                                               "XCOM2 War of the Chosen", 
                                                               "XComGame",
                                                               "Photobooth", 
                                                               "Campaign_123456789", 
                                                               "UserPhotos");

        private readonly TimeSpan RemainingTokenLifespanToTriggerRefresh = TimeSpan.FromMinutes(45);

        private readonly string CredentialsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XCOM 2 Stream Companion", "credentials-do-not-share.json");

        private readonly string[] Scopes = ["channel:manage:polls",          // Read/create polls (mirror game polls to channel, sync channel poll to game state, check if a poll is running before making one)
                                            "channel:moderate",              // Needed to get PubSub events when a chat message is deleted by a mod
                                            "channel:read:subscriptions",    // Check who subs are for raffles
                                            "channel:read:vips",             // Check who VIPs are for raffles
                                            "chat:read",                     // Watch incoming messages for chat commands
                                            "moderator:manage:banned_users", // Timing out users who lose an event
                                            "moderation:read",               // Check who moderators are for raffles
                                            "moderator:read:chatters",       // Check who chatters are for raffles
                                            "moderator:read:followers",      // Check who followers are for raffles
                                           ];

        private HttpClient httpClient = new HttpClient();

        private Font timestampFont;
        private ILoggerFactory loggerFactory;
        private ILogger<XComStreamAppForm> logger;

        private XComModService? XComModServiceConnection = null;

        private System.Timers.Timer refreshChannelStateChannelInfoTimer = new System.Timers.Timer(TimeSpan.FromMinutes(5));
        private System.Timers.Timer refreshChannelStateChattersTimer = new System.Timers.Timer(TimeSpan.FromSeconds(65));
        private System.Timers.Timer refreshChannelStatePollTimer = new System.Timers.Timer(TimeSpan.FromSeconds(65));
        private System.Timers.Timer validateAccessTokenTimer = new System.Timers.Timer(TimeSpan.FromMinutes(30)); // needs to be 30 minutes so we refresh token before it expires

        public XComStreamAppForm(ILoggerFactory loggerFactory)
        {
            InitializeComponent();

            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<XComStreamAppForm>();

            // Don't show the login button until after we check for stored credentials
            btnTwitchLogInOrOut.Visible = false;

            txtChatEvents.Text = "";
            txtSystemEvents.Text = "";

            refreshChannelStateChannelInfoTimer.AutoReset = false;
            refreshChannelStateChannelInfoTimer.Elapsed += (_, _) => RefreshChannelInfo();

            refreshChannelStateChattersTimer.AutoReset = false;
            refreshChannelStateChattersTimer.Elapsed += (_, _) => RefreshChannelChatters();

            refreshChannelStatePollTimer.AutoReset = false;
            refreshChannelStatePollTimer.Elapsed += (_, _) => RefreshChannelPoll();

            validateAccessTokenTimer.AutoReset = true;
            validateAccessTokenTimer.Elapsed += (_, _) => ValidateAccessTokenFromTimer();
            validateAccessTokenTimer.Start(); // start now; it won't do anything if the user isn't authed

            timestampFont = new Font(txtChatEvents.Font, FontStyle.Bold);

            // Position the loading spinner
            imgTwitchLoadingSpinner.Location = new Point(btnTwitchLogInOrOut.Left + btnTwitchLogInOrOut.Width / 2 - imgTwitchLoadingSpinner.Width / 2, btnTwitchLogInOrOut.Top);
            imgTwitchLoadingSpinner.Visible = false;

            lblTwitchConnectionStatus.Text = "Checking for stored credentials..";

            // Initial load has to be on a timer so the form can finish construction before we begin calling Invoke on it
            var initTimer = new System.Timers.Timer(TimeSpan.FromSeconds(0.5));
            initTimer.AutoReset = false;
            initTimer.Elapsed += async (_, _) =>
            {
                if (await LoadAndValidateCredentials())
                {
                    PostSuccessfulTwitchUserAuth(isManualOrFirstTimeAuth: true);
                }
                else
                {
                    Invoke(ShowBeginOAuthFlowUI);
                }
            };

            initTimer.Start();

            // Make sure our emote folder exists before we try to download anything into it
            Directory.CreateDirectory(EmoteFolderPath);
        }

        public void AddEvent(SystemEvent e)
        {
            Invoke(() =>
            {
                string timestampString = $"[{e.Timestamp:H:mm:ss}]";
                string eventString = $" {e.Description}\n";

                logger.LogInformation("AddEvent: {description}", e.Description);

                RichTextBox targetTextbox = e.Type switch
                {
                    EventType.AppEvent => txtSystemEvents,
                    EventType.GameEvent => txtSystemEvents,
                    EventType.ChatEvent => txtChatEvents,
                    EventType.ExtensionEvent => txtChatEvents,
                    _ => throw new Exception($"Unhandled event type {e.Type}")
                };

                var originalFont = targetTextbox.SelectionFont;

                targetTextbox.SelectionFont = timestampFont;
                targetTextbox.AppendText(timestampString);

                targetTextbox.SelectionFont = originalFont;
                targetTextbox.AppendText(eventString);
            });
        }

        public void RequestReceivedFromGame()
        {
            Invoke(() =>
            {
                lblGameConnectionStatus.Text = $"Last connection from game: {DateTime.Now:H:mm:ss}";
            });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            int panelWidth = ClientRectangle.Width / 2;
            int panelHeight = ClientRectangle.Height;

            pnlLeftColumn.Left = 0;
            pnlLeftColumn.Width = panelWidth;
            pnlLeftColumn.Top = 0;
            pnlLeftColumn.Height = panelHeight;

            pnlMiddleColumn.Left = panelWidth;
            pnlMiddleColumn.Width = panelWidth;
            pnlMiddleColumn.Top = 0;
            pnlMiddleColumn.Height = panelHeight;
        }

        private void btnConnectToXCom2_Click(object sender, EventArgs e)
        {
        }

        private void btnLogInToTwitch_Click(object? sender, EventArgs e)
        {
            StartOAuthDeviceCodeFlow();
        }

        private void btnLogOutOfTwitch_Click(object? sender, EventArgs e)
        {
            LogOutOfTwitch();
        }

        private async void InitializeChannelState()
        {
            TwitchState.Channel = new Models.Twitch.ChannelState();

            // We need the subscriber info before we can correctly populate the chatters list
            var subscribers = await TwitchApiService.GetSubscribers(TwitchState.ConnectedUser!.Id);
            TwitchState.Channel.SubscribersByUserId = subscribers.ToDictionary(user => user.UserId);

            RefreshChannelChatters();
            RefreshChannelInfo();
            RefreshChannelPoll();
        }

        private void InitializeTwitchChatClient()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(TwitchState.ConnectedUser.Login, TwitchState.AccessToken);

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            WebSocketClient customClient = new WebSocketClient(clientOptions);
            TwitchState.ChatClient = new TwitchClient(customClient);
            TwitchState.ChatClient.Initialize(credentials, TwitchState.ConnectedUser.Login);

            TwitchState.ChatClient.OnJoinedChannel += Client_OnJoinedChannel;
            TwitchState.ChatClient.OnMessageReceived += Client_OnMessageReceived;
            TwitchState.ChatClient.OnConnected += Client_OnConnected;

            TwitchState.ChatClient.Connect();
        }

        private void InitializeTwitchPubSubClient()
        {
            TwitchState.PubSubConnection = new TwitchPubSubHandler(loggerFactory, this);
            TwitchState.PubSubConnection.Connect();
        }

        /// <summary>
        /// Attempts to load a stored access token and refresh token from disk.
        /// </summary>
        /// <returns>True if valid credentials were found and the user is now authenticated.</returns>
        private async Task<bool> LoadAndValidateCredentials()
        {
            logger.LogInformation("Checking if Twitch credentials already exist on disk at {path}", CredentialsFilePath);

            try
            {
                if (!File.Exists(CredentialsFilePath))
                {
                    logger.LogInformation("No credentials file found");
                    return false;
                }

                var text = File.ReadAllText(CredentialsFilePath);
                var json = JsonNode.Parse(text);

                if (json == null)
                {
                    logger.LogWarning("Credentials file did not contain valid JSON");
                    return false;
                }

                string? accessToken = json["accessToken"]?.GetValue<string>();
                string? refreshToken = json["refreshToken"]?.GetValue<string>();

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    logger.LogWarning("Credentials file did not contain tokens");
                    return false;
                }

                // Need to check with Twitch whether these credentials are still valid. Use a local TwitchAPI object
                // for now, so we aren't populating global state with potentially invalid objects.
                var api = new TwitchAPI();
                api.Settings.ClientId = TwitchState.AppClientId;
                api.Settings.AccessToken = accessToken;

                Invoke(() => {
                    imgTwitchLoadingSpinner.Visible = true; 
                    btnTwitchLogInOrOut.Visible = false; 
                });

                ValidateAccessTokenResponse? validateResponse = null;

                try
                {
                    validateResponse = await api.Auth.ValidateAccessTokenAsync(accessToken);
                }
                catch
                {
                    // Expected if the token is invalid
                }

                if (validateResponse != null)
                {
                    // In case of app updates, the user may have a stored credential which is lacking a scope
                    // that we now need. If that occurs, we should require re-auth.
                    foreach (string scope in Scopes)
                    {
                        if (!validateResponse.Scopes.Contains(scope))
                        {
                            logger.LogInformation("Stored credential is missing at least one scope ({scope}). Reauthorization is required", scope);
                            return false;
                        }
                    }

                    logger.LogInformation("Stored token validated successfully");
                }

                if (validateResponse == null || validateResponse.ExpiresIn <= RemainingTokenLifespanToTriggerRefresh.TotalSeconds)
                {
                    var result = await RefreshAccessToken(refreshToken);
                    
                    if (!result.Succeeded)
                    {
                        return false;
                    }

                    accessToken = result.AccessToken;
                    refreshToken = result.RefreshToken;
                }

                logger.LogInformation("Stored credentials will be used to log in automatically");

                TwitchState.AccessToken = accessToken;
                TwitchState.RefreshToken = refreshToken;

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("An error occurred while loading credentials from disk: {err}", e);
                return false; 
            }
        }

        private void LogOutOfTwitch()
        {
            logger.LogInformation("Logging out of Twitch..");

            try
            {
                if (File.Exists(CredentialsFilePath))
                {
                    logger.LogInformation("Deleting cached credentials file at {path}", CredentialsFilePath);
                    File.Delete(CredentialsFilePath);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Exception occurred while trying to delete credentials file: {err}", e);
            }

            // We don't strictly need the revoke to succeed, so we don't wait on the result
            if (!string.IsNullOrEmpty(TwitchState.AccessToken))
            {
                logger.LogInformation("Attempting to revoke current access token");
                _ = TwitchApiService.RevokeAccessToken(TwitchState.AccessToken, TwitchState.AppClientId);
            }

            TwitchState.API = null;
            TwitchState.AccessToken = "";
            TwitchState.RefreshToken = "";
            TwitchState.ConnectedUser = null;

            TwitchState.ChatClient?.Disconnect();
            TwitchState.ChatClient = null;

            TwitchState.PubSubConnection?.Disconnect();
            TwitchState.PubSubConnection = null;

            TwitchState.Channel = null;

            Invoke(ShowBeginOAuthFlowUI);

            // TODO: bring up the loading spinner for a bit; right now logging out is so fast that it doesn't seem to do anything at all
        }

        private void PersistCredentials(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return;
            }

            logger.LogInformation("Persisting auth credentials to {path}", CredentialsFilePath);
            var credentials = new Dictionary<string, string>()
            {
                { "accessToken", accessToken },
                { "refreshToken", refreshToken }
            };

            string text = JsonSerializer.Serialize(credentials);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CredentialsFilePath)!);
                File.WriteAllText(CredentialsFilePath, text);
            }
            catch (Exception e)
            {
                logger.LogError("An error occurred while persisting credentials: {exception}", e);
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            XComModServiceConnection?.Close();
        }

        private void Client_OnConnected(object? sender, OnConnectedArgs e)
        {
            Debug.WriteLine($"Connected to {e.AutoJoinChannel}");

            AddEvent(new SystemEvent()
            {
                Type = EventType.AppEvent,
                Description = "Connected to Twitch chat server"
            });
        }

        private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            Debug.WriteLine($"Joined channel {e.Channel} as {e.BotUsername}");

            AddEvent(new SystemEvent()
            {
                Type = EventType.AppEvent,
                Description = $"Joined chat for channel {e.Channel} as {e.BotUsername}"
            });
        }

        private async void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            Debug.WriteLine($"Received message: {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");

            if (e.ChatMessage.Message.StartsWith('!'))
            {
                string command = "", body = "";
                int emoteOffset = 0;

                await DownloadEmotes(e.ChatMessage.EmoteSet);

                if (e.ChatMessage.Message.Contains(' '))
                {
                    // Read from after the ! until the first space
                    emoteOffset = e.ChatMessage.Message.IndexOf(' ') + 1;
                    command = e.ChatMessage.Message.Substring(1, e.ChatMessage.Message.IndexOf(' ') - 1);

                    // Read the remaining string after the first space
                    body = e.ChatMessage.Message.Substring(e.ChatMessage.Message.IndexOf(' ') + 1);
                }
                else
                {
                    // No space: the whole thing is the command (except the exclamation point)
                    command = e.ChatMessage.Message.Substring(1);
                }

                AddEvent(new SystemEvent()
                {
                    Type = EventType.ChatEvent,
                    Description = $"[Chat] {e.ChatMessage.Username}: {e.ChatMessage.Message}",
                    TwitchName = e.ChatMessage.Username
                });

                // TODO: we might need to html encode the body for xsay
                // TODO pass emote data as well
                TwitchState.PendingGameEvents.Enqueue(new Models.XComMod.ChatCommandEvent()
                {
                    ViewerLogin = e.ChatMessage.Username,
                    Command = command,
                    Body = body,
                    EmoteData = e.ChatMessage.EmoteSet.Emotes.Select(e => new ChatEmoteData()
                    {
                        EmoteCode = e.Name,
                        StartIndex = e.StartIndex - emoteOffset,
                        EndIndex = e.EndIndex - emoteOffset
                    }),
                    MessageId = e.ChatMessage.Id
                });
            }
        }

        private async Task DownloadEmotes(EmoteSet emoteSet)
        {
            foreach (var emote in emoteSet.Emotes)
            {
                string emoteCode = emote.Name;
                string imageUrl = emote.ImageUrl.Substring(0, emote.ImageUrl.Length - 3) + "3.0"; // replace "1.0" with "3.0" to get the full-sized emote
                string outputFilePath = Path.Combine(EmoteFolderPath, $"TwitchEmote_{emoteCode}.png");

                if (File.Exists(outputFilePath))
                {
                    continue;
                }

                logger.LogInformation("Attempting to download Twitch emote {emoteCode}", emoteCode);

                using var httpStream = await httpClient.GetStreamAsync(imageUrl);
                using var fileStream = new FileStream(outputFilePath, FileMode.OpenOrCreate);

                await httpStream.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Updates the application state after the user's Twitch auth is updated.
        /// </summary>
        /// <param name="isManualOrFirstTimeAuth">Whether this auth required manual user action, or is the first time authenticating since program start.</param>
        private async void PostSuccessfulTwitchUserAuth(bool isManualOrFirstTimeAuth)
        {
            if (TwitchState.API == null)
            {
                TwitchState.API = new TwitchAPI();
                TwitchState.API.Settings.ClientId = TwitchState.AppClientId;
            }

            TwitchState.API.Settings.AccessToken = TwitchState.AccessToken;

            if (isManualOrFirstTimeAuth)
            {
                // TODO: validate the token instead and use the info from that
                var getUsersResponse = await TwitchState.API.Helix.Users.GetUsersAsync();

                if (getUsersResponse.Users.Length != 1)
                {
                    Debug.WriteLine($"Warning: expected one user in response, got {getUsersResponse.Users.Length}");
                }
                else
                {
                    TwitchState.ConnectedUser = getUsersResponse.Users[0];

                    InitializeChannelState();

                    // Twitch chat connection only checks the access token on connection, so no need to update it if we're refreshing
                    // an existing token. If we are re-initializing though, we should close the existing connection (if any), because
                    // the user may have logged into a different account.
                    TwitchState.ChatClient?.Disconnect();
                    TwitchState.ChatClient = null;
                    InitializeTwitchChatClient();

                    TwitchState.PubSubConnection?.Disconnect();
                    TwitchState.PubSubConnection = null;
                    InitializeTwitchPubSubClient();

                    Invoke(() =>
                    {
                        lblTwitchConnectionStatus.Text = $"You are connected to Twitch as {TwitchState.ConnectedUser.DisplayName}.";
                        btnTwitchLogInOrOut.Visible = false;
                        imgTwitchLoadingSpinner.Visible = false;
                    });

                    AddEvent(new SystemEvent()
                    {
                        Type = EventType.AppEvent,
                        Description = "Connection established to Twitch API"
                    });
                }
            }

            Invoke(ShowLogOutOfTwitchUI);
        }

        private async Task<(bool Succeeded, string AccessToken, string RefreshToken)> RefreshAccessToken(string refreshToken)
        {
            logger.LogInformation("Attempting to refresh access token");

            var refreshResponse = await TwitchApiService.RefreshAccessToken(refreshToken, TwitchState.AppClientId);

            // Null response means we failed to refresh
            if (refreshResponse == null)
            {
                logger.LogWarning("Existing access token could not be refreshed. Reauthorization is required");
                return (false, "", "");
            }

            // TODO: check the scopes here too, we might be refreshing a token with out-of-date auth scopes
            logger.LogInformation("Existing access token refreshed successfully");

            PersistCredentials(refreshResponse.AccessToken, refreshResponse.RefreshToken);

            return (true, refreshResponse.AccessToken, refreshResponse.RefreshToken);
        }

        private async Task RefreshChannelChatters()
        {
            if (TwitchState.ConnectedUser == null || TwitchState.Channel == null || string.IsNullOrEmpty(TwitchState.AccessToken))
            {
                return;
            }

            try
            {
                var chatters = await TwitchApiService.GetChatters(TwitchState.ConnectedUser.Id, TwitchState.AccessToken, TwitchState.AppClientId);

                // If we have subscriber data for a chatter, just use that user object instead; everything else will match
                for (int i = 0; i < chatters.Count; i++)
                {
                    if (TwitchState.Channel.SubscribersByUserId.TryGetValue(chatters[i].UserId, out var user))
                    {
                        chatters[i] = user;
                    }
                }

                TwitchState.Channel!.Chatters = chatters;
            }
            catch (BadScopeException e)
            {
                _ = ValidateAccessTokenFromTimer();
            }
            catch (Exception e)
            {
                logger.LogError("Exception occurred while refreshing channel chatters: {e}", e);
            }

            refreshChannelStateChattersTimer.Start();
        }

        private async void RefreshChannelInfo()
        {
            if (TwitchState.API == null || TwitchState.ConnectedUser == null || TwitchState.Channel == null)
            {
                return;
            }

            try
            {
                var response = await TwitchState.API!.Helix.Channels.GetChannelInformationAsync(TwitchState.ConnectedUser.Id);

                TwitchState.Channel.ChannelInfo = response.Data[0];

                refreshChannelStateChannelInfoTimer.Start();
            }
            catch (BadScopeException e)
            {
                _ = ValidateAccessTokenFromTimer();
            }
            catch (Exception e)
            {
                logger.LogError("Exception occurred while refreshing channel info: {e}", e);
            }
        }

        private async void RefreshChannelPoll()
        {
            if (TwitchState.API == null || TwitchState.ConnectedUser == null || TwitchState.Channel == null)
            {
                return;
            }

            try
            {
                // Polls are returned ordered by their start time; just grab the most recent one
                var response = await TwitchState.API!.Helix.Polls.GetPollsAsync(TwitchState.ConnectedUser.Id, first: 1);

                if (response.Data.Length == 0 || response.Data[0].Status != "ACTIVE")
                {
                    TwitchState.Channel.CurrentPoll = null;
                }
                else
                {
                    TwitchState.Channel.CurrentPoll = response.Data[0];
                }

                refreshChannelStatePollTimer.Start();
            }
            catch (BadScopeException e)
            {
                _ = ValidateAccessTokenFromTimer();
            }
            catch (Exception e)
            {
                logger.LogError("Exception occurred while refreshing channel poll: {e}", e);
            }
        }

        /// <summary>
        /// Sets the UI to a state that prompts the user to begin the OAuth flow.
        /// </summary>
        private void ShowBeginOAuthFlowUI()
        {
            logger.LogDebug("Displaying OAuth flow to the user");

            btnTwitchLogInOrOut.Click -= btnLogOutOfTwitch_Click;
            btnTwitchLogInOrOut.Click += btnLogInToTwitch_Click;

            btnTwitchLogInOrOut.Visible = true;
            btnTwitchLogInOrOut.Text = "Log in to Twitch";
            imgTwitchLoadingSpinner.Visible = false;
            lblTwitchConnectionStatus.Text = "You are not connected to Twitch services.";
        }

        private void ShowLogOutOfTwitchUI()
        {
            logger.LogDebug("Displaying log out flow to the user");

            btnTwitchLogInOrOut.Click += btnLogOutOfTwitch_Click;
            btnTwitchLogInOrOut.Click -= btnLogInToTwitch_Click;

            btnTwitchLogInOrOut.Visible = true;
            btnTwitchLogInOrOut.Text = "Log out of Twitch";
            imgTwitchLoadingSpinner.Visible = false;
        }

        private async void StartOAuthDeviceCodeFlow()
        {
            logger.LogInformation("Beginning OAuth device code flow");

            lblTwitchConnectionStatus.Text = "Contacting Twitch servers..";
            btnTwitchLogInOrOut.Visible = false;
            imgTwitchLoadingSpinner.Visible = true;

            var authScopes = string.Join(" ", Scopes);
            var formParams = new Dictionary<string, string>
            {
                { "client_id", TwitchState.AppClientId },
                { "scopes", authScopes }
            };

            logger.LogInformation("Requesting device code from Twitch with {numScopes} auth scopes", authScopes.Length);
            var httpResponse = await httpClient.PostAsync("https://id.twitch.tv/oauth2/device", new FormUrlEncodedContent(formParams));

            // TODO verify that request succeeded
            if (!httpResponse.IsSuccessStatusCode)
            {
                logger.LogError("Error occurred when requesting device code: {err}", await httpResponse.Content.ReadAsStringAsync());
                return;
            }

            var deviceCodeResponse = await httpResponse.Content.ReadFromJsonAsync<GetDeviceCodeResponse>();

            lblTwitchConnectionStatus.Text = "Follow the prompts in your browser to connect.";

            logger.LogInformation("Directing user to open the verification URI");

            Process.Start(new ProcessStartInfo(deviceCodeResponse!.VerificationUri)
            {
                UseShellExecute = true,
                Verb = "open",
            });

            var timerStartedAt = DateTime.Now;
            var checkDeviceCodeTimer = new System.Timers.Timer(TimeSpan.FromSeconds(3));
            checkDeviceCodeTimer.AutoReset = false;
            checkDeviceCodeTimer.Elapsed += async (_, e) =>
            {
                var timeSinceAuthBegan = DateTime.Now - timerStartedAt;
                
                // Time out the auth after a bit
                // TODO: check the device code response to see how long the code is valid
                if (timeSinceAuthBegan > TimeSpan.FromMinutes(10))
                {
                    logger.LogWarning("Authorization process has timed out after {elapsedTime}", timeSinceAuthBegan.ToString());

                    lblTwitchConnectionStatus.Text = "Authorization has timed out. Please try again.";
                    btnTwitchLogInOrOut.Visible = true;
                    imgTwitchLoadingSpinner.Visible = false;
                    return;
                }

                // Check if we're authorized yet
                formParams = new Dictionary<string, string>
                {
                    { "client_id", TwitchState.AppClientId },
                    { "scopes", authScopes },
                    { "device_code", deviceCodeResponse.DeviceCode },
                    { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" }
                };

                logger.LogInformation("Checking with Twitch to see if device code has been authorized..");
                httpResponse = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(formParams));

                // TODO: there doesn't appear to be any indication from the server if the user cancels the request. Might need a button
                // to let them reopen the URL in that case.
                if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    // May indicate that the user hasn't authenticated the app yet
                    var errorResponse = await httpResponse.Content.ReadFromJsonAsync<AuthTokenErrorResponse>();

                    logger.LogInformation("No authorization yet; response text is {response}", errorResponse!.Message);

                    if (errorResponse.IsAuthorizationPending)
                    {
                        // Not authed yet; check again in a bit
                        checkDeviceCodeTimer.Start();
                        return;
                    }
                    else
                    {
                        // TODO
                        return;
                    }
                }

                var accessTokenResponse = await httpResponse.Content.ReadFromJsonAsync<GetAccessTokenResponse>();

                // Update UI state and trigger the next connection steps
                Invoke(() =>
                {
                    lblTwitchConnectionStatus.Text = "Authentication successful. Connecting to chat..";
                });

                logger.LogInformation("Device code auth flow complete; access token and refresh token retrieved successfully");

                TwitchState.AccessToken = accessTokenResponse!.AccessToken;
                TwitchState.RefreshToken = accessTokenResponse.RefreshToken;

                PersistCredentials(TwitchState.AccessToken, TwitchState.RefreshToken);
                PostSuccessfulTwitchUserAuth(isManualOrFirstTimeAuth: true);
            };

            checkDeviceCodeTimer.Start();
        }

        private async Task ValidateAccessTokenFromTimer()
        {
            if (TwitchState.API == null || TwitchState.AccessToken == null || TwitchState.RefreshToken == null)
            {
                return;
            }

            logger.LogInformation("Timer elapsed: validating access token");

            var validateResponse = await TwitchState.API.Auth.ValidateAccessTokenAsync();

            logger.LogInformation("Token valid? {IsValid}; expires in: {ExpiresIn}", validateResponse != null, validateResponse?.ExpiresIn);

            if (validateResponse == null || validateResponse.ExpiresIn <= RemainingTokenLifespanToTriggerRefresh.TotalSeconds)
            {
                var refreshResult = await RefreshAccessToken(TwitchState.RefreshToken);

                if (refreshResult.Succeeded)
                {
                    TwitchState.API.Settings.AccessToken = refreshResult.AccessToken;
                    TwitchState.AccessToken = refreshResult.AccessToken;
                    TwitchState.RefreshToken = refreshResult.RefreshToken;
                }
                else
                {
                    logger.LogInformation("Failed to refresh access token. Returning to OAuth flow.");

                    TwitchState.API = null;
                    TwitchState.AccessToken = "";
                    TwitchState.RefreshToken = "";
                    TwitchState.ConnectedUser = null;

                    Invoke(ShowBeginOAuthFlowUI);
                }
            }
        }

        private void RichTextBox_SelectionChanged(object sender, EventArgs e)
        {
            if (sender != null)
            {
                (sender as RichTextBox).SelectionLength = 0;
            }
        }
    }
}
