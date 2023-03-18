using CocApi.Cache;
using CocApi.Cache.Extensions;
using CocApi.Rest.Client;
using CocApi.Rest.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using MinionBot.Maui.Models;
using Serilog;
using MinionBot.Maui.ViewModels;
using MinionBot.Maui.View;

namespace MinionBot.Maui
{
    public static class MauiProgram
    {
        public static string SettingsJsonPath { get; } = Path.Combine(FileSystem.AppDataDirectory, "settings.json");
        public static string DatabaseFolder { get; } = Path.Combine(FileSystem.AppDataDirectory, "database");
        public static string ClansFolder { get; } = Path.Combine(FileSystem.AppDataDirectory, "clans");
        public static string LogPath { get; } = Path.Combine(FileSystem.AppDataDirectory, "log.txt");

        public static MauiApp CreateMauiApp()
        {
            CreateUserSettingsFile();

            var hostBuilder = MauiApp.CreateBuilder()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            hostBuilder.ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windows =>
                        windows.OnClosed((Window, AccelerometerChangedEventArgs) =>
                        {
                            CachingService cachingService = MinionBot.Maui.WinUI.App.MauiApp.Services.GetRequiredService<CachingService>();

                            cachingService.StopAsync();

                            Serilog.Log.CloseAndFlush();
                        }));
#endif
                });

            var config = new ConfigurationBuilder().AddJsonFile(SettingsJsonPath).Build();
            hostBuilder.Configuration.AddConfiguration(config);

            Serilog.ILogger logger = SetupSerilog(hostBuilder.Configuration);
            hostBuilder.Logging.AddSerilog(logger, dispose: true);

            hostBuilder.Services.AddSingleton<CachedPlayerViewModel>();
            hostBuilder.Services.AddSingleton<CachedPlayerView>();

            hostBuilder.Services.Configure<Settings>(hostBuilder.Configuration.GetSection("Settings"));
            hostBuilder.Services.AddCocApi(options =>
            {
                string token = hostBuilder.Configuration["Settings:Token"];
                TimeSpan tokenTimeout = TimeSpan.FromMilliseconds(500);
                options.AddTokens(new ApiKeyToken(token, timeout: tokenTimeout));

                options.AddCocApiHttpClients(
                client =>
                {
                    client.BaseAddress = new Uri(hostBuilder.Configuration["Settings:BaseAddress"]);
                    client.Timeout = TimeSpan.FromSeconds(1000);
                },
                    builder =>
                    {
                        builder
                            .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler()
                            {
                                MaxConnectionsPerServer = 10
                            });

                        if (hostBuilder.Configuration["Settings:UseFastApi"].ToLower() == "true")
                        {
                            //builder.AddHttpMessageHandler(() => new AppendRealTimeQuery());
                            //builder.AddHttpMessageHandler(() => new PatchResponse());
                        }
                    }
                );
            });

            hostBuilder.Services.AddCocApiCache(
                dbContextOptions =>
                {
                    dbContextOptions.UseSqlite($"Data Source={Path.Combine(DatabaseFolder, "cocapi.cache.sqlite")};Cache=Shared");
                    CacheDbContext dbContext = new((DbContextOptions<CacheDbContext>)dbContextOptions.Options);
                    dbContext.Database.EnsureCreated();
                },
                options =>
                {
                    options.ActiveWars.Enabled = true;
                    options.ClanMembers.Enabled = false;
                    options.Clans.Enabled = true;
                    options.CwlWars.Enabled = true;
                    options.NewCwlWars.Enabled = true;
                    options.NewWars.Enabled = true;
                    options.Players.Enabled = true;
                    options.Wars.Enabled = true;
                });

            return hostBuilder.Build();
        }

        private static void CreateUserSettingsFile()
        {
            Directory.CreateDirectory(DatabaseFolder);
            Directory.CreateDirectory(ClansFolder);

            if (!File.Exists(SettingsJsonPath))
                new Settings().Write();
        }

        private static Serilog.ILogger SetupSerilog(IConfiguration configuration)
        {
            LoggerConfiguration logger = new();
            logger.ReadFrom.Configuration(configuration, Serilog.Settings.Configuration.ConfigurationAssemblySource.AlwaysScanDllFiles)
                .Enrich.FromLogContext()
                .Enrich.With<UtcTimestampEnricher>();

#if DEBUG
            logger.WriteTo.Debug();
#endif

            return logger.CreateLogger();
        }
    }
}