using CocApi.Cache;
using CocApi.Cache.Context;
using CocApi.Cache.Services;
using CocApi.Rest.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinionBot.Maui.ViewModels
{
    public partial class CachedPlayerViewModel : ObservableObject
    {
        public CachedPlayerViewModel(ILogger<CachedPlayerViewModel> logger, CacheDbContext cacheDbContext, PlayersClient playersClient, ClansClient clansClient, CachingService cachingService, ClanService clanService)
        {
            Logger = logger;
            CacheDbContext = cacheDbContext;
            PlayersClient = playersClient;
            ClansClient = clansClient;
            CachingService = cachingService;
            ClanService = clanService;
        }

        public ILogger<CachedPlayerViewModel> Logger { get; }
        public CacheDbContext CacheDbContext { get; }
        public PlayersClient PlayersClient { get; }
        public ClansClient ClansClient { get; }
        public CachingService CachingService { get; }
        public ClanService ClanService { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        public ObservableCollection<CachedPlayer> Players { get; } = new();

        [ObservableProperty]
        private string _newPlayerTag = "";

        [RelayCommand]
        public async Task GetPlayersAsync()
        {
            if (IsBusy) // TODO: semaphore
                return;

            try
            {
                //await CachingService.StartAsync();
                //await ClanService.StartAsync(CancellationToken.None);

                IsBusy = true;
                Players.Clear();
                List<CachedPlayer> players = await CacheDbContext.Players.ToListAsync();
                foreach (CachedPlayer cachedPlayer in players)
                {
                    Players.Add(cachedPlayer);

                    if (cachedPlayer.ClanTag != null)
                        await ClansClient.AddOrUpdateAsync(cachedPlayer.ClanTag);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured getting players.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task AddPlayerAsync()
        {
            if (CocApi.Clash.TryFormatTag(NewPlayerTag.Trim(), out string? formattedTag))
            {
                await PlayersClient.AddOrUpdateAsync(formattedTag);
                await GetPlayersAsync();
            }
        }
    }
}
