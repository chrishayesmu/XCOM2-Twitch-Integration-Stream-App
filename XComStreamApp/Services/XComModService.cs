using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static XComStreamApp.Services.XComModService;

namespace XComStreamApp.Services
{
    /// <summary>
    /// <para>
    /// Class which handles communicating with the XCOM 2 Twitch Integration mod.
    /// </para>
    /// <para>
    /// Communication is via a localhost TCP connection, on the port <see cref="ListenPort"/>.
    /// </para>
    /// </summary>
    public class XComModService
    {
        public const int ListenPort = 39201;

        /// <summary>
        /// Whether a game client is currently connected to the server.
        /// </summary>
        public bool IsConnected => connectedClient != null && connectedClient.Connected;

        public delegate Task<string?> GameMessageHandler(string message);

        private static TimeSpan KeepAliveDisconnectTime = TimeSpan.FromMilliseconds(2500);

        private GameMessageHandler messageHandler;
        private TcpListener? server = null;

        private volatile TcpClient? connectedClient = null;
        private volatile StreamReader? streamReader = null;
        private volatile StreamWriter? streamWriter = null;

        // Provides a cancellation token that will be canceled when the application is shutting down.
        private CancellationTokenSource appClosingTokenSource = new CancellationTokenSource();

        // Provides a cancellation token that will be canceled if XCOM doesn't respond to keepalives fast enough.
        private CancellationTokenSource? keepAliveTokenSource = null;

        private System.Timers.Timer checkKeepAliveTimer;
        private System.Timers.Timer sendKeepAliveTimer;
        private DateTime lastKeepAliveTime;

        public XComModService(GameMessageHandler messageHandler)
        {
            checkKeepAliveTimer = new System.Timers.Timer(1000);
            checkKeepAliveTimer.Elapsed += CheckKeepAliveMessage;
            checkKeepAliveTimer.AutoReset = true;
            checkKeepAliveTimer.Start();

            sendKeepAliveTimer = new System.Timers.Timer(250);
            sendKeepAliveTimer.Elapsed += SendKeepAliveMessage;
            sendKeepAliveTimer.AutoReset = true;
            sendKeepAliveTimer.Start();

            this.messageHandler = messageHandler;
        }

        /// <summary>
        /// Closes any outstanding connection and shuts down the listen server.
        /// </summary>
        public void Close()
        {
            appClosingTokenSource.Cancel();
        }

        /// <summary>
        /// Starts a listen server for incoming connections from XCOM 2.
        /// </summary>
        public void Listen()
        {
            if (server != null)
            {
                return;
            }

            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, ListenPort);
            server.Start();

            Task.Run(async () =>
            {
                byte[] buffer = new byte[512];

                while (true)
                {
                    connectedClient = await server.AcceptTcpClientAsync(appClosingTokenSource.Token);

                    keepAliveTokenSource = new CancellationTokenSource();
                    var readTokenSource = CancellationTokenSource.CreateLinkedTokenSource(appClosingTokenSource.Token, keepAliveTokenSource.Token);

                    streamReader = new StreamReader(connectedClient.GetStream());
                    streamWriter = new StreamWriter(connectedClient.GetStream());

                    lastKeepAliveTime = DateTime.Now;

                    while (IsConnected)
                    {
                        try
                        {
                            string? message = await streamReader.ReadLineAsync(readTokenSource.Token);

                            if (message == null)
                            {
                                continue;
                            }

                            if (message == "KEEPALIVE")
                            {
                                lastKeepAliveTime = DateTime.Now;
                            }
                            else
                            {
                                Debug.WriteLine($"Received message from game: {message}");

                                string? response = await messageHandler.Invoke(message);

                                if (response != null)
                                {
                                    TryWriteLine(response);
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // This means the keepalive with XCOM failed, or the app is closing
                            DropConnection();
                        }

                        appClosingTokenSource.Token.ThrowIfCancellationRequested();
                    }

                    appClosingTokenSource.Token.ThrowIfCancellationRequested();
                }
            });
        }

        private void DropConnection()
        {
            connectedClient?.Close();

            connectedClient = null;
            streamReader = null;
            streamWriter = null;
            _ = messageHandler.Invoke("CONN LOST");
        }

        private void CheckKeepAliveMessage(object? sender, ElapsedEventArgs e)
        {
            if (!IsConnected)
            {
                return;
            }

            TimeSpan timeSinceLastKeepAlive = DateTime.Now.Subtract(lastKeepAliveTime);

            if (timeSinceLastKeepAlive >= KeepAliveDisconnectTime)
            {
                Debug.WriteLine($"No keepalive in {KeepAliveDisconnectTime.TotalMilliseconds} ms; terminating connection (last keepalive was {timeSinceLastKeepAlive.TotalMilliseconds} ms ago)");
                keepAliveTokenSource?.Cancel();
            }
        }

        private void SendKeepAliveMessage(object? sender, ElapsedEventArgs e)
        {
            if (!IsConnected)
            {
                return;
            }

            TryWriteLine("KEEPALIVE");
        }

        private bool TryWriteLine(string message)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (message != "KEEPALIVE")
            {
                Debug.WriteLine($"Sending message to game: {message}");
            }

            try
            {
                streamWriter!.WriteLine(message);
                streamWriter.Flush();
                return true;
            }
            catch (IOException)
            {
                DropConnection();

                return false;
            }
        }
    }
}
