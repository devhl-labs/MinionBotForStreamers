namespace MinionBot.Streamers
{
    public sealed class ClanOption
    {
        public string Tag { get; set; }

        public string? Folder { get; set; }

        public ClanOption()
        {

        }

        public ClanOption(string tag, string folder)
        {
            Tag = tag;
            Folder = folder;
        }

        public override string ToString() => $"{Tag} {Folder}";
    }
}
