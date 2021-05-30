using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CocApi;
using CocApi.Cache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MinionBot.Streamers
{
    public class Program
    {
        public static string AppDataFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MinionBot.Streamers");

        public static string SettingsJson { get; } = Path.Combine(AppDataFolder, "settings.json");

        public static string DatabaseFolder { get; } = Path.Combine(AppDataFolder, "database");

        public static string ClansFolder { get; } = Path.Combine(AppDataFolder, "clans");

        public static void Main(string[] args)
        {
            try
            {
                LogService.LogLine("https://github.com/devhl-labs/MinionBotForStreamers");

                Directory.CreateDirectory(AppDataFolder);
                Directory.CreateDirectory(DatabaseFolder);
                Directory.CreateDirectory(ClansFolder);

                if (!File.Exists(SettingsJson))
                    new Settings().Write();

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);

                Console.WriteLine("An error occured. Press any key to exit.");

                Console.ReadKey();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(o => o.ClearProviders())
                .ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile(SettingsJson, false, true);
                })
                .ConfigureServices((hostBuilder, services) =>
                {
                    services.Configure<Settings>(hostBuilder.Configuration.GetSection("Settings"));

                    services.AddCocApi("cocApi", tokenProvider =>
                    {
                        string token = hostBuilder.Configuration["Settings:Token"];

                        TimeSpan tokenTimeout = TimeSpan.FromMilliseconds(500);

                        tokenProvider.Tokens.Add(new(token, tokenTimeout));
                    });

                    services.AddCocApiCache<ClansClient, PlayersClientBase>(
                        configure =>
                        {
                            DbContextProviderFactory factory = new();

                            CacheDbContext dbContext = factory.CreateDbContext(Array.Empty<string>());

                            dbContext.Database.EnsureCreated();

                            configure.Factory = factory;

                            configure.DbContextArgs = Array.Empty<string>();
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
                        },
                        p => p.Enabled = false);

                    services.AddHttpClient("cocApi", config =>
                    {
                        config.BaseAddress = new Uri("https://api.clashofclans.com/v1");
                        config.Timeout = TimeSpan.FromSeconds(10000);
                    })
                    .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler()
                    {
                        MaxConnectionsPerServer = 10
                    });

                    services.AddHostedService<Commands>();
                });
    }
}
