using CocApi.Cache;
using CocApi.Cache.Context;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
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
        public CachedPlayerViewModel(ILogger<CachedPlayerViewModel> logger, CacheDbContext cacheDbContext)
        {
            Logger = logger;
            CacheDbContext = cacheDbContext;
        }

        public ILogger<CachedPlayerViewModel> Logger { get; }
        public CacheDbContext CacheDbContext { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        public ObservableCollection<CachedPlayer> Players { get; } = new();

        [RelayCommand]
        private async Task GetPlayersAsync()
        {
            if (IsBusy) // TODO: semaphore
                return;

            try
            {
                IsBusy = true;
                Players.Clear();
                List<CachedPlayer> players = await CacheDbContext.Players.ToListAsync();
                foreach (CachedPlayer cachedPlayer in players)
                    Players.Add(cachedPlayer);

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
    }
}
