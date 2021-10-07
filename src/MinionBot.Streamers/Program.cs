using System;
using System.IO;
using System.Net.Http;
using CocApi;
using CocApi.Cache;
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

                    services.AddCocApi("cocApi", tokenProvider =>
                    {
                        string token = hostBuilder.Configuration["Settings:Token"];
                        TimeSpan tokenTimeout = TimeSpan.FromMilliseconds(500);
                        tokenProvider.Tokens.Add(new(token, tokenTimeout));
                    });

                    services.AddCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(
                        (services, dbContextOptions) =>
                        {
                            dbContextOptions.UseSqlite($"Data Source={ Path.Combine(DatabaseFolder, "cocapi.cache.sqlite") };Cache=Shared");
                            CacheDbContext dbContext = new((DbContextOptions<CacheDbContext>) dbContextOptions.Options);
                            dbContext.Database.EnsureCreated();
                        },
                        c =>
                        {
                            c.ActiveWars.Enabled = true;
                            c.ClanMembers.Enabled = false;
                            c.Clans.Enabled = true;
                            c.NewCwlWars.Enabled = true;
                            c.NewWars.Enabled = true;
                            c.Wars.Enabled = true;
                            c.CwlWars.Enabled = true;
                            c.Players.Enabled = false;
                        });

                    services.AddHttpClient("cocApi", config =>
                    {
                        config.BaseAddress = new Uri(hostBuilder.Configuration["Settings:BaseAddress"]);
                        config.Timeout = TimeSpan.FromSeconds(10000);
                    })
                    .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler()
                    {
                        MaxConnectionsPerServer = 10
                    });

                    services.AddHostedService(services => services.GetRequiredService<ClansClient>());
                    services.AddHostedService<InteractiveMenu>();
                });
    }
}
