using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache;
using CocApi.Cache.Services;
using CocApi.Cache.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MinionBot.Streamers
{
    public class ClansClient : CocApi.Cache.ClansClient, IHostedService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<Settings> _settings;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public ClansClient(
            IHttpClientFactory httpClientFactory,
            ILogger<ClansClient> logger,
            CocApi.Rest.IApis.IClansApi clansApi,
            IServiceScopeFactory scopeFactory,
            Synchronizer synchronizer,
            ClanService clanService,
            NewWarService newWarService,
            NewCwlWarService newCwlWarService,
            WarService warService,
            CwlWarService cwlWarService,
            IOptions<CacheOptions> cacheOptions,
            IOptions<Settings> settings
        ) : base(
            logger, clansApi, scopeFactory, synchronizer, clanService, newWarService, newCwlWarService, warService, cwlWarService, cacheOptions)
        {
            _httpClientFactory = httpClientFactory;
            ClanWarAdded += ClansClient_ClanWarAdded;
            ClanWarUpdated += ClansClient_ClanWarUpdated;
            _settings = settings;
        }

        private async Task ClansClient_ClanWarUpdated(object sender, ClanWarUpdatedEventArgs e) => await UpdateAsync(e.Fetched);

        private async Task ClansClient_ClanWarAdded(object sender, WarAddedEventArgs e) => await UpdateAsync(e.War, true);

        private async Task UpdateAsync(CocApi.Rest.Models.ClanWar clanWar, bool downloadBadges = false)
        {
            Logger.LogTrace("Running UpdateAsync...");

            foreach (ClanOption clanOption in _settings.Value.Clans.Where(c => clanWar.Clans.Any(cw => c.Tag == cw.Key)))
            {
                if (clanOption.Folder == null || clanWar.Clans.FirstOrDefault(c => c.Value.Tag == clanOption.Tag).Value == null)
                    continue;

                await _semaphoreSlim.WaitAsync();

                try
                {
                    string clanFolder = Path.Combine(Program.ClansFolder, clanOption.Folder, "clan");
                    string opponentFolder = Path.Combine(Program.ClansFolder, clanOption.Folder, "enemy");

                    Logger.LogTrace("Creating clan folders...");
                    Directory.CreateDirectory(clanFolder);
                    Directory.CreateDirectory(opponentFolder);

                    string clanStatsFolder = Path.Combine(clanFolder, "stats");
                    string opponentStatsFolder = Path.Combine(opponentFolder, "stats");

                    Logger.LogTrace("Creating stats folders...");
                    Directory.CreateDirectory(clanStatsFolder);
                    Directory.CreateDirectory(opponentStatsFolder);

                    Logger.LogTrace("Writing stats to files...");
                    File.WriteAllText(Path.Combine(clanFolder, "attacks.txt"), clanWar.Attacks.Count.ToString());
                    File.WriteAllText(Path.Combine(clanFolder, "endTime.txt"), clanWar.EndTime.ToString());
                    File.WriteAllText(Path.Combine(clanFolder, "preparationStartTime.txt"), clanWar.PreparationStartTime.ToString());
                    File.WriteAllText(Path.Combine(clanFolder, "serverExpiration.txt"), clanWar.ServerExpiration.ToString());
                    File.WriteAllText(Path.Combine(clanFolder, "startTime.txt"), clanWar.StartTime.ToString());
                    File.WriteAllText(Path.Combine(clanFolder, "state.txt"), clanWar.State.ToString());
                    File.WriteAllText(Path.Combine(clanFolder, "warTag.txt"), clanWar.WarTag);

                    Update(clanWar, clanWar.Clans.First(c => c.Key == clanOption.Tag).Value, clanFolder, clanStatsFolder);
                    Update(clanWar, clanWar.Clans.First(c => c.Key != clanOption.Tag).Value, opponentFolder, opponentStatsFolder);

                    if (downloadBadges)
                    {
                        await DownloadBadgesAsync(clanWar.Clans.First(c => c.Key == clanOption.Tag).Value, clanFolder);
                        await DownloadBadgesAsync(clanWar.Clans.First(c => c.Key != clanOption.Tag).Value, opponentFolder);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "An exception occured at UpdateAsync");

                    throw;
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
        }

        private async Task DownloadBadgesAsync(CocApi.Rest.Models.WarClan warClan, string folder)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(warClan.BadgeUrls.Large);
            if (response.IsSuccessStatusCode)
            {
                HttpContent content = response.Content;
                using Stream stream = await content.ReadAsStreamAsync();
                using var fs = new FileStream(Path.Combine(folder, "large.png"), FileMode.Create);
                await stream.CopyToAsync(fs);
            }

            response = await httpClient.GetAsync(warClan.BadgeUrls.Medium);
            if (response.IsSuccessStatusCode)
            {
                HttpContent content = response.Content;
                using Stream stream = await content.ReadAsStreamAsync();
                using var fs = new FileStream(Path.Combine(folder, "medium.png"), FileMode.Create);
                await stream.CopyToAsync(fs);
            }

            response = await httpClient.GetAsync(warClan.BadgeUrls.Small);
            if (response.IsSuccessStatusCode)
            {
                HttpContent content = response.Content;
                using Stream stream = await content.ReadAsStreamAsync();
                using var fs = new FileStream(Path.Combine(folder, "small.png"), FileMode.Create);
                await stream.CopyToAsync(fs);
            }
        }

        private void Update(CocApi.Rest.Models.ClanWar clanWar, CocApi.Rest.Models.WarClan warClan, string clanFolder, string statsFolder)
        {
            Logger.LogTrace("Running update...");

            try
            {
                File.WriteAllText(Path.Combine(clanFolder, "name.txt"), warClan.Name);
                File.WriteAllText(Path.Combine(clanFolder, "attacks.txt"), warClan.Attacks.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "clanLevel.txt"), warClan.ClanLevel.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "clanProfileUrl.txt"), warClan.ClanProfileUrl);
                File.WriteAllText(Path.Combine(clanFolder, "destructionPercentage.txt"), FormatNumber(warClan.DestructionPercentage));
                File.WriteAllText(Path.Combine(clanFolder, "members.txt"), warClan.Members.Count.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "result.txt"), warClan.Result.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "stars.txt"), warClan.Stars.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "tag.txt"), warClan.Tag.ToString());

                Logger.LogTrace("Created various stats files.");

                List<CocApi.Rest.Models.ClanWarAttack> bestClanAttacks = new();

                CocApi.Rest.Models.ClanWarAttack[] clanAttacks = clanWar.Attacks.Where(a => a.AttackerClanTag == warClan.Tag && a.DefenderTag != null).ToArray();

                var grouped = clanAttacks.GroupBy(a => a.DefenderTag);

                foreach (IGrouping<string, CocApi.Rest.Models.ClanWarAttack> group in grouped)
                    bestClanAttacks.Add(group
                        .OrderByDescending(a => a.Stars)
                        .ThenByDescending(a => a.DestructionPercentage)
                        .ThenByDescending(a => a.Duration)
                        .First());

                for (int i = _settings.Value.MinTownhall; i < _settings.Value.MaxTownhall + 1; i++)
                {
                    File.WriteAllText(Path.Combine(statsFolder, $"{i} three.txt"), bestClanAttacks.Count(a => a.DefenderTownHall == i && a.Stars == 3).ToString());
                    File.WriteAllText(Path.Combine(statsFolder, $"{i} two.txt"), bestClanAttacks.Count(a => a.DefenderTownHall == i && a.Stars == 2).ToString());
                    File.WriteAllText(Path.Combine(statsFolder, $"{i} one.txt"), bestClanAttacks.Count(a => a.DefenderTownHall == i && a.Stars == 1).ToString());

                    int[] attackerThs = new int[] { i--, i, i++ };

                    foreach (int attackerTh in attackerThs)
                    {
                        if (attackerTh < _settings.Value.MinTownhall || attackerTh > _settings.Value.MaxTownhall)
                            continue;

                        CocApi.Rest.Models.ClanWarAttack[] scope = clanAttacks.Where(a => a.AttackerTownHall == attackerTh && a.DefenderTownHall == i).ToArray();
                        CocApi.Rest.Models.ClanWarAttack[] scopeBest = bestClanAttacks.Where(a => a.AttackerTownHall == attackerTh && a.DefenderTownHall == i).ToArray();

                        int tries = scope.Length;
                        int threes = scopeBest.Count(a => a.Stars == 3);

                        if (tries == 0)
                            File.WriteAllText(Path.Combine(statsFolder, $"{attackerTh}v{i}P.txt"), "0");
                        else
                        {
                            float percent = (float)threes / tries * 100;
                            string s = FormatNumber(percent);
                            File.WriteAllText(Path.Combine(statsFolder, $"{attackerTh}v{i}P.txt"), s);
                        }
                        File.WriteAllText(Path.Combine(statsFolder, $"{attackerTh}v{i}Three.txt"), threes.ToString());
                        File.WriteAllText(Path.Combine(statsFolder, $"{attackerTh}v{i}Tries.txt"), tries.ToString());
                    }
                }

                Logger.LogTrace("Done writing files to stats.");
            }
            catch (Exception err)
            {
                Logger.LogError(err, "An error occured at {0}", nameof(Update));
            }
        }

        private string FormatNumber(float value)
        {
            string percent = _settings.Value.IncludePercentSign ? "%" : string.Empty;

            if (value == 100)
                return "100" + percent;

            string zeroes = ".";

            for (int i = 0; i < _settings.Value.DigitsAfterDecimal; i++)
                zeroes += "0";

            return zeroes == "."
                ? value.ToString("0") + percent
                : value.ToString($"0{zeroes}") + percent;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_settings.Value.Token) || _settings.Value.Token == "put your token here")
                throw InvalidToken();

            try
            {
                await ClansApi.FetchClanAsync("#929YJPYJ", cancellationToken);
            }
            catch (CocApi.Rest.Client.ApiException e) when (e.Message == "Forbidden")
            {
                throw InvalidToken();
            }
        }

        private Exception InvalidToken()
        {
            string input = InteractiveMenu.GetStringInput(
    @"You don't have a token set or it is not valid. You an get a token from developer.clashofclans.com.
You will also need your public IPv4 address. You can get that from whatismyipaddress.com

Enter your clash of clans token:");
            _settings.Value.Token = input;
            _settings.Value.Write();
            return new Exception("Restart the application for this change to take effect.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
