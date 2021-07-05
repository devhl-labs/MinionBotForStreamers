using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MinionBot.Streamers
{
    public class ClansClient : CocApi.Cache.ClansClientBase, IHostedService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<Settings> _options;

        public ClansClient(IHttpClientFactory httpClientFactory, PlayersClientBase playersClient, PlayersApi playersApi, 
            ClansApi clansApi, CacheDbContextFactoryProvider provider, IOptions<ClanClientOptions> clanClientOptions, IOptions<Settings> options)
            : base(clansApi, playersClient, playersApi, provider, clanClientOptions)
        {
            ClanWarAdded += ClansClient_ClanWarAdded;

            ClanWarUpdated += ClansClient_ClanWarUpdated;
            _httpClientFactory = httpClientFactory;
            _options = options;

            if (_options.Value.Debug && _options.Value.Token != "put your token here")
            {
                CocApi.Cache.Library.Log += OnLog;

                CocApi.Library.HttpRequestResult += OnHttpRequestResult;
            }

            if (_options.Value.Token == "put your token here")
            {
                LogService.LogLine(
"\n\nYou don't have a token set. You an get a token from developer.clashofclans.com.\n" +
"You will also need your public IPv4 address. You can get that from whatismyipaddress.com\n" +
"Once you have a token from SuperCell, you can go to settings and provide your token.\n" +
"Then restart this program.\n\n");

                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    await StopAsync(CancellationToken.None);
                });
            }
        }

        public static void Debug(IOptions<Settings> options)
        {
            if (options.Value.Debug)
            {
                CocApi.Cache.Library.Log += OnLog;

                CocApi.Library.HttpRequestResult += OnHttpRequestResult;
            }
            else
            {
                CocApi.Cache.Library.Log -= OnLog;

                CocApi.Library.HttpRequestResult -= OnHttpRequestResult;
            }
        }

        private static Task OnHttpRequestResult(object sender, CocApi.HttpRequestResultEventArgs result)
        {
            string seconds = ((int)result.HttpRequestResult.Elapsed.TotalSeconds).ToString();

            if (result.HttpRequestResult is CocApi.HttpRequestException exception)
                LogService.Log(LogLevel.Warning, sender.GetType().Name, seconds, exception.Path, exception.Message, exception.InnerException?.Message);
            else if (result.HttpRequestResult is CocApi.HttpRequestNonSuccess nonSuccess)
                LogService.Log(LogLevel.Debug, sender.GetType().Name, seconds, nonSuccess.Path, nonSuccess.Reason);
            else
                LogService.Log(LogLevel.Information, sender.GetType().Name, seconds, result.HttpRequestResult.Path);

            return Task.CompletedTask;
        }

        private static Task OnLog(object sender, LogEventArgs log)
        {
            LogService.Log(LogLevel.Information, sender.GetType().Name, log.Message);

            return Task.CompletedTask;
        }

        private async Task ClansClient_ClanWarUpdated(object sender, ClanWarUpdatedEventArgs e)
        {
            try
            {
                await UpdateAsync(e.Fetched);
            }
            catch (Exception err)
            {
                LogService.Log(LogLevel.Error, err.Message, err.InnerException?.Message);
            }
        }

        private async Task ClansClient_ClanWarAdded(object sender, WarAddedEventArgs e)
        {
            try
            {
                await UpdateAsync(e.War, true);
            }
            catch (Exception err)
            {
                LogService.Log(LogLevel.Error, err.Message, err.InnerException?.Message);
            }
        }

        private async Task UpdateAsync(CocApi.Model.ClanWar clanWar, bool downloadBadges = false)
        {
            foreach (ClanOption clanOption in _options.Value.Clans)
            {
                if (clanOption.Folder == null || clanWar.Clans.FirstOrDefault(c => c.Value.Tag == clanOption.Tag).Value == null)
                    continue;

                string clanFolder = Path.Combine(Program.ClansFolder, clanOption.Folder, "clan");
                string opponentFolder = Path.Combine(Program.ClansFolder, clanOption.Folder, "enemy");

                Directory.CreateDirectory(clanFolder);
                Directory.CreateDirectory(opponentFolder);

                string clanStatsFolder = Path.Combine(clanFolder, "stats");
                string opponentStatsFolder = Path.Combine(opponentFolder, "stats");

                Directory.CreateDirectory(clanStatsFolder);
                Directory.CreateDirectory(opponentStatsFolder);

                File.WriteAllText(Path.Combine(clanFolder, "attacks.txt"), clanWar.Attacks.Count.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "endTime.txt"), clanWar.EndTime.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "preparationStartTime.txt"), clanWar.PreparationStartTime.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "serverExpiration.txt"), clanWar.ServerExpiration.ToString());
                File.WriteAllText(Path.Combine(clanFolder, "startTime.txt"), clanWar.StartTime.ToString());;
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
        }

        private async Task DownloadBadgesAsync(CocApi.Model.WarClan warClan, string folder)
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

        private void Update(CocApi.Model.ClanWar clanWar, CocApi.Model.WarClan warClan, string clanFolder, string statsFolder)
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

            List<CocApi.Model.ClanWarAttack> bestClanAttacks = new();

            CocApi.Model.ClanWarAttack[] clanAttacks = clanWar.Attacks.Where(a => a.AttackerClanTag == warClan.Tag && a.DefenderTag != null).ToArray();

            var grouped = clanAttacks.GroupBy(a => a.DefenderTag);

            foreach(IGrouping<string, CocApi.Model.ClanWarAttack> group in grouped)                
                bestClanAttacks.Add(group
                    .OrderByDescending(a => a.Stars)
                    .ThenByDescending(a => a.DestructionPercentage)
                    .ThenByDescending(a => a.Duration)
                    .First());

            for (int i = _options.Value.MinTownhall; i < _options.Value.MaxTownhall + 1; i++)
            {
                File.WriteAllText(Path.Combine(statsFolder, $"{i} three.txt"), bestClanAttacks.Count(a => a.DefenderTownHall == i && a.Stars == 3).ToString());
                File.WriteAllText(Path.Combine(statsFolder, $"{i} two.txt"), bestClanAttacks.Count(a => a.DefenderTownHall == i && a.Stars == 2).ToString());
                File.WriteAllText(Path.Combine(statsFolder, $"{i} one.txt"), bestClanAttacks.Count(a => a.DefenderTownHall == i && a.Stars == 1).ToString());

                int[] attackerThs = new int[] { i--, i, i++ };

                foreach(int attackerTh in attackerThs)
                {
                    if (attackerTh < _options.Value.MinTownhall || attackerTh > _options.Value.MaxTownhall)
                        continue;

                    CocApi.Model.ClanWarAttack[] scope = clanAttacks.Where(a => a.AttackerTownHall == attackerTh && a.DefenderTownHall == i).ToArray();
                    CocApi.Model.ClanWarAttack[] scopeBest = bestClanAttacks.Where(a => a.AttackerTownHall == attackerTh && a.DefenderTownHall == i).ToArray();

                    int tries = scope.Length;
                    int threes = scopeBest.Count(a => a.Stars == 3);

                    if (tries == 0)
                        File.WriteAllText(Path.Combine(statsFolder, $"{attackerTh}v{i}P.txt"), "0");
                    else
                    {
                        float percent = (float) threes / tries * 100;
                        string s = FormatNumber(percent);
                        File.WriteAllText(Path.Combine(statsFolder, $"{attackerTh}v{i}P.txt"), s);
                    }
                    File.WriteAllText(Path.Combine(statsFolder, $"{attackerTh}v{i}Three.txt"), threes.ToString());
                    File.WriteAllText(Path.Combine(statsFolder, $"{attackerTh}v{i}Tries.txt"), tries.ToString());
                }
            }
        }

        private string FormatNumber(float value)
        {
            string format = _options.Value.DigitsAfterDecimal > 0
                ? "0."
                : "0";

            for (int i = 0; i < _options.Value.DigitsAfterDecimal; i++)            
                format = $"{format}0";            

            string result = Math.Round(value, _options.Value.DigitsAfterDecimal).ToString(format);

            if (_options.Value.TrimTrailingZeroes && result.Split(".").Skip(1).First().ToCharArray().All(x => x.ToString() == "0"))
                result = Math.Round(value, _options.Value.DigitsAfterDecimal).ToString();

            return _options.Value.IncludePercentSign
                ? $"{result}%"
                : result;
        }

        protected override ValueTask<TimeSpan> TimeToLiveAsync<T>(ApiResponse<T> apiResponse) => new(TimeSpan.MinValue);

        protected override ValueTask<TimeSpan> TimeToLiveAsync<T>(Exception exception) => new(TimeSpan.FromSeconds(15));
    }
}
