using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TwitchLib.EventSub.Websockets.Core.Handler;
using TwitchLib.EventSub.Websockets.Extensions;
using XComStreamApp.Services;
using XComStreamApp.Services.EventSub;

namespace XComStreamApp
{
    internal static class Program
    {
        public static XComStreamAppForm Form;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var webApp = StartWebApplication();
            var loggerFactory = webApp.Services.GetRequiredService<ILoggerFactory>();
            var eventSubService = webApp.Services.GetRequiredService<TwitchEventSubService>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += HandleUIThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Form = new XComStreamAppForm(loggerFactory, eventSubService);
            Application.Run(Form);
        }

        static private WebApplication StartWebApplication()
        {
            var builder = WebApplication.CreateBuilder();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production"}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            builder.Services.AddControllers();
            builder.Services.AddSerilog();

            // Custom EventSub notification handlers to handle events that TwitchLib doesn't support
            builder.Services.AddSingleton(typeof(INotificationHandler), typeof(ChatMessageDeletedHandler));

            builder.Services.AddTwitchLibEventSubWebsockets();

            // Trick to make the service retrievable with GetRequiredService later
            // https://stackoverflow.com/a/65552373
            builder.Services.AddSingleton<TwitchEventSubService>();
            builder.Services.AddHostedService(p => p.GetRequiredService<TwitchEventSubService>());

            var app = builder.Build();
            app.UseMiddleware<DeChunkerMiddleware>();
            app.MapControllers();

            Task.Run(() => app.Run("http://localhost:5000"));

            Log.Logger.Information("Initialization complete.");

            return app;
        }

        static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var e = args.ExceptionObject as Exception;
            Log.Logger.Error("Unhandled exception: {e}", e);
            MessageBox.Show(e.Message, "Unhandled Exception");
        }

        static void HandleUIThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Logger.Error("Unhandled UI thread exception: {e}", e.Exception);
            MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception");
        }
    }
}