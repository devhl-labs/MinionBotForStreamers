namespace MinionBot.Streamers
{
    public sealed class UserSettings
    {
        public Settings Settings { get; }

        public UserSettings(Settings settings)
        {
            Settings = settings;
        }
    }
}
