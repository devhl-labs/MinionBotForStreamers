using MinionBot.Maui.ViewModels;

namespace MinionBot.Maui.View;

public partial class SettingsView : ContentPage
{
    public SettingsView(SettingsViewModel settingsViewModel)
    {
        InitializeComponent();
        BindingContext = settingsViewModel;

        // there is a bug that prevents sliders from being set correctly initially
        MaxTownhallSlider.Value = settingsViewModel.Settings.Value.MaxTownhall;
        MinTownhallSlider.Value = settingsViewModel.Settings.Value.MinTownhall;
    }

    void OnMaxTownhallChanged(object sender, ValueChangedEventArgs args)
    {
        MaxTownhallSlider.Value = (int)args.NewValue;
    }

    void OnMinTownhallChanged(object sender, ValueChangedEventArgs args)
    {
        MinTownhallSlider.Value = (int)args.NewValue;
    }
}