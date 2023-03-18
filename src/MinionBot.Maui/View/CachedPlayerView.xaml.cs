using MinionBot.Maui.ViewModels;

namespace MinionBot.Maui.View;

public partial class CachedPlayerView : ContentPage
{
	public CachedPlayerView(CachedPlayerViewModel cachedPlayerViewModel)
	{
		InitializeComponent();
        BindingContext = cachedPlayerViewModel;
    }
}