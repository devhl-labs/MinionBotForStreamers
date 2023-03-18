namespace MinionBot.Maui.Models
{
    internal sealed class UserSettings
    {
        public Settings Settings { get; }

        public UserSettings(Settings settings)
        {
            Settings = settings;
        }
    }
}
