using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += HandleUIThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Form = new XComStreamAppForm(loggerFactory);
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

            var app = builder.Build();
            app.UseMiddleware<DeChunkerMiddleware>();
            app.MapControllers();

            Task.Run(() => app.Run("http://localhost:5000"));

            return app;
        }

        static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var e = args.ExceptionObject as Exception;
            MessageBox.Show(e.Message, "Unhandled Exception");
        }

        static void HandleUIThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception");
        }
    }
}