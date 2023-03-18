using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinionBot.Maui.Models
{
    public class ClanOption
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
