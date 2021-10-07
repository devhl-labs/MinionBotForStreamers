using System.Collections.Generic;
using System.IO;

namespace MinionBot.Streamers
{
    public sealed class Settings
    {
        public string BaseAddress { get; set; } = "https://api.clashofclans.com/v1";
        public bool TrimTrailingZeroes { get; set; } = true;
        public string Token { get; set; } = "put your token here";
        public int DigitsAfterDecimal { get; set; } = 2;
        public bool IncludePercentSign { get; set; }
        public int MaxTownhall { get; set; } = CocApi.Clash.MAX_TOWN_HALL_LEVEL;
        public int MinTownhall { get; set; } = 9;
        public List<ClanOption> Clans { get; set; } = new();

        public void Write()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(new UserSettings(this), Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(Program.SettingsJsonPath, json);
        }
    }
}
