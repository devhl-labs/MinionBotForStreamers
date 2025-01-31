using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache;
using CocApi.Cache.Extensions;
using CocApi.Rest.Client;
using CocApi.Rest.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MinionBot.Streamers
{
    public class Program
    {
        public static string AppDataFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MinionBot.Streamers");
        public static string SettingsJsonPath { get; } = Path.Combine(AppDataFolder, "settings.json");
        public static string DatabaseFolder { get; } = Path.Combine(AppDataFolder, "database");
        public static string ClansFolder { get; } = Path.Combine(AppDataFolder, "clans");
        public static string LogPath { get; } = Path.Combine(AppDataFolder, "log.txt");


        public static void Main(string[] args)
        {
            try
            {
                Directory.CreateDirectory(AppDataFolder);
                Directory.CreateDirectory(DatabaseFolder);
                Directory.CreateDirectory(ClansFolder);

                if (!File.Exists(SettingsJsonPath))
                    new Settings().Write();

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("\n\nPress any key to exit.");
                Console.ReadKey();
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, config) =>
                {
                    config.ReadFrom
                        .Configuration(context.Configuration, Serilog.Settings.Configuration.ConfigurationAssemblySource.AlwaysScanDllFiles)
                        .Enrich.FromLogContext()
                        .Enrich.With<UtcTimestampEnricher>();
                })
                .ConfigureHostConfiguration(config => config.AddJsonFile(SettingsJsonPath, false, true))
                .ConfigureServices((hostBuilder, services) =>
                {
                    services.Configure<Settings>(hostBuilder.Configuration.GetSection("Settings"));

                    services.AddCocApi(options =>
                    {
                        string token = hostBuilder.Configuration["Settings:Token"];
                        TimeSpan tokenTimeout = TimeSpan.FromMilliseconds(500);
                        options.AddTokens(new ApiKeyToken(token, ClientUtils.ApiKeyHeader.Authorization, timeout: tokenTimeout));

                        options.AddCocApiHttpClients(
                            client =>
                            {
                                client.BaseAddress = new Uri(hostBuilder.Configuration["Settings:BaseAddress"]);
                                client.Timeout = TimeSpan.FromSeconds(10000);
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
                                    builder.AddHttpMessageHandler(() => new AppendRealTimeQuery());
                                    builder.AddHttpMessageHandler(() => new PatchResponse());
                                }
                            }
                        );
                    });

                    services.AddCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(
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
                            options.Players.Enabled = false;
                            options.Wars.Enabled = true;
                        });

                    services.AddHostedService(services => services.GetRequiredService<ClansClient>());
                    services.AddHostedService<InteractiveMenu>();
                });
    }

    public class AppendRealTimeQuery : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string[] realTimeEligible = new string[] { "currentwar", "warTag" };

            if (realTimeEligible.Any(word => request.RequestUri?.ToString().Contains(word) == true))
                request.RequestUri = new Uri($"{request.RequestUri}?realtime=true");

            return base.SendAsync(request, cancellationToken);
        }
    }

    public class PatchResponse : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string[] realTimeEligible = new string[] { "currentwar", "warTag" };

            if (realTimeEligible.Any(word => request.RequestUri?.ToString().Contains(word) == true))
            {
                HttpResponseMessage result = await base.SendAsync(request, cancellationToken);

                result.Headers.CacheControl ??= new System.Net.Http.Headers.CacheControlHeaderValue();

                result.Headers.CacheControl.MaxAge = TimeSpan.Zero;

                return result;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
