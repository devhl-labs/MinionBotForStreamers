using CocApi.Cache;
using CocApi.Cache.Services;
using CocApi.Rest.Apis;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MinionBot.Maui.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        const int WindowWidth = 600;
        const int WindowHeight = 900;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                var mauiWindow = handler.VirtualView;
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();
                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));
            });
        }

        public static MauiApp MauiApp { get; private set; }

        protected override MauiApp CreateMauiApp()
        {
            var result = MauiProgram.CreateMauiApp();

            CachingService cachingService = result.Services.GetRequiredService<CachingService>();

            var a = cachingService.StartAsync();

            //Thread.Sleep(2000);

            //PlayersClient playersClient = result.Services.GetRequiredService<PlayersClient>();

            //playersClient.AddOrUpdateAsync("#20LRPJG2U");

            MauiApp = result;

            return result;
        }
    }
}