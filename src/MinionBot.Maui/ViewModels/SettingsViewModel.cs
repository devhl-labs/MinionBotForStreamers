using CocApi.Cache;
using CocApi.Rest.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MinionBot.Maui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinionBot.Maui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(ILogger<SettingsViewModel> logger, IOptions<Settings> settings)
    {
        Logger = logger;
        Settings = settings;
    }

    public ILogger<SettingsViewModel> Logger { get; }
    public IOptions<Settings> Settings { get; }
    public int MAX_TOWN_HALL_LEVEL { get; } = CocApi.Clash.MAX_TOWN_HALL_LEVEL;


    [RelayCommand]
    private async Task SaveAsync()
    {
        Settings.Value.Write();
    }
}
