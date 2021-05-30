namespace MinionBot.Streamers
{
    public sealed class ClanOption
    {
        public string Tag { get; set; }

        public string Name { get; set; }

        public ClanOption()
        {

        }

        public ClanOption(string tag, string name)
        {
            Tag = tag;
            Name = name;
        }

        public override string ToString() => $"{Tag} {Name}";
    }
}
