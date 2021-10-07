using System;
using System.Threading.Tasks;

namespace MinionBot.Streamers
{
    public sealed class MenuOption
    {
        public string Option { get; }

        public Func<InteractiveMenu, Task>? Func { get; }

        public MenuOption(string option, Func<InteractiveMenu, Task>? func)
        {
            Option = option;
            Func = func;
        }
    }
}
