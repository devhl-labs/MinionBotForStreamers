using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi;
using CocApi.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MinionBot.Streamers
{
    public sealed class InteractiveMenu : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;


        public MenuOption[] MainMenu => new MenuOption[]
        {
            new("Add a clan", AddAClan), 
            new("Remove a clan", RemoveAClan), 
            new("Settings", ConfigureSettings), 
            new("View all clans", ViewAllClans), 
            new("About", About),
            new("Quit", null)
        };
        public MenuOption[] SettingsMenu => new MenuOption[]
        {
            new("Add your token", ConfigureToken),
            new("Configure decimals", ConfigureDigitsAfterDecimal),
            new("Configure percent sign", ConfigureIncludePercentSign),
            new("Set minimum townhall", ConfigureMinimumTownhall),
            new("Set maximum townhall", ConfigureMaximumTownhall),
            new("Toggle trim trailing zeroes.", ConfigureTrailingZeroes),
            new("Main menu", null)
        };
        public IOptions<Settings> Settings { get; }
        public ClansClient ClansClient { get; }
        public ILogger<InteractiveMenu> Logger { get; }


        public InteractiveMenu(
            ILogger<InteractiveMenu> logger, 
            IHostApplicationLifetime hostApplicationLifetime, 
            ClansClient clansClient, 
            IOptions<Settings> settings)
        {
            Logger = logger;
            Settings = settings;
            _hostApplicationLifetime = hostApplicationLifetime;
            ClansClient = clansClient;
        }

        private static void PrintMenu(MenuOption[] menu)
        {
            for (int i = 0; i < menu.Length; i++)            
                Console.WriteLine($"{i + 1}. {menu[i].Option}");            

            Console.WriteLine("Choose an option:");
        }

        private static async Task ConfigureSettings(InteractiveMenu commands)
        {
            bool stopRequested = false;

            while (!stopRequested)
            {
                try
                {
                    MenuOption? menuOption = await commands.ExecuteMenu(commands.SettingsMenu);

                    Console.WriteLine();

                    if (menuOption != null && menuOption.Func == null)
                        stopRequested = true;
                }
                catch (Exception e)
                {
                    commands.Logger.LogError(e, "An error occured at {0}", nameof(ConfigureSettings));
                }
            }

            return;
        }

        private static async Task AddAClan(InteractiveMenu commands)
        {
            string tag = GetStringInput("Enter the clan tag:");

            if (!Clash.TryFormatTag(tag, out string? formattedTag))
                throw new Exception("Invalid tag");

            await commands.ClansClient.AddOrUpdateAsync(formattedTag, false, true, false, true, false);

            ClanOption? exists = commands.Settings.Value.Clans.FirstOrDefault(c => c.Tag == formattedTag);

            if (exists != null)
            {
                Console.WriteLine(
$"This clan tag is already entered. The folder name is { commands.Settings.Value.Clans.First(c => c.Tag == formattedTag).Folder ?? "null" }. Enter the new folder name, or just press enter.");

                string? newFolderName = Console.ReadLine();

                if (newFolderName != null)
                {
                    foreach (ClanOption clanOption in commands.Settings.Value.Clans.Where(c => c.Folder?.ToLower() == newFolderName.ToLower()))
                        clanOption.Folder = null;

                    exists.Folder = newFolderName;

                    commands.Settings.Value.Write();
                }
            }
            else
            {
                string name = GetStringInput("Enter the clan name. This will be used as the name of the folder. Case is insensitive. Use valid characters for your operating system. If you will be changing clans often you can just use a generic folder name like 'clan a'");

                foreach (ClanOption clanOption in commands.Settings.Value.Clans.Where(c => c.Folder?.ToLower() == name.ToLower()))
                    clanOption.Folder = null;

                commands.Settings.Value.Clans.Add(new ClanOption(formattedTag, name));

                commands.Settings.Value.Write();

                await commands.ClansClient.AddOrUpdateAsync(formattedTag, false, true, false, true, false);

                Console.WriteLine($"Added { formattedTag } { name }");
            }
        }

        private static async Task RemoveAClan(InteractiveMenu commands)
        {
            string name = GetStringInput("Enter the clan tag or folder name to remove:");

            Clash.TryFormatTag(name, out string? formattedTag);

            if (formattedTag != null)
                await commands.ClansClient.DeleteAsync(formattedTag);

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Invalid input");

            ClanOption? clan = commands.Settings.Value.Clans.FirstOrDefault(c => c.Tag == formattedTag || c.Folder == name);

            clan ??= commands.Settings.Value.Clans.FirstOrDefault(c => c.Folder != null && c.Folder.ToLower() == name.ToLower());

            if (clan == null)
                throw new Exception("Clan not found.");

            commands.Settings.Value.Clans.Remove(clan);

            commands.Settings.Value.Write();

            await commands.ClansClient.AddOrUpdateAsync(clan.Tag, false, false, false, false, false);

            Console.WriteLine($"Removed { clan }");
        }

        private static async Task ViewAllClans(InteractiveMenu commands)
        {
            foreach (ClanOption clanOption in commands.Settings.Value.Clans)
            {
                Clan clan = await commands.ClansClient.GetOrFetchClanAsync(clanOption.Tag);

                Console.WriteLine($"{clanOption.Tag} {clan.Name} Folder: { clanOption.Folder }");
            }
        }

        private static Task About(InteractiveMenu commands)
        {
            Console.WriteLine(@$"You can find this program at https://github.com/devhl-labs/MinionBotForStreamers
All data for this program is stored at {Program.AppDataFolder}
If you have any issues you can always delete that data folder and start over.
In that case you will have to enter your token and clan tags again.");

            return Task.CompletedTask;
        }

        private static Task ConfigureToken(InteractiveMenu commands)
        {
            string input = GetStringInput("Enter your clash of clans token:");
            commands.Settings.Value.Token = input;
            commands.Settings.Value.Write();
            Console.WriteLine("Restart the application for this change to take effect.");
            return Task.CompletedTask;
        }

        private static Task ConfigureDigitsAfterDecimal(InteractiveMenu commands)
        {
            int input = GetIntegerInput("Enter the number of digits after the decimal:");
            commands.Settings.Value.DigitsAfterDecimal = input;
            commands.Settings.Value.Write();
            return Task.CompletedTask;
        }

        private static Task ConfigureIncludePercentSign(InteractiveMenu commands)
        {
            bool input = GetBoolInput("Display the percent symbol? Enter yes or no.");
            commands.Settings.Value.IncludePercentSign = input;
            commands.Settings.Value.Write();
            return Task.CompletedTask;
        }

        private static Task ConfigureMinimumTownhall(InteractiveMenu commands)
        {
            int input = GetIntegerInput("Enter the minimum townhall level:");
            commands.Settings.Value.MinTownhall = input;
            commands.Settings.Value.Write();
            return Task.CompletedTask;
        }

        private static Task ConfigureMaximumTownhall(InteractiveMenu commands)
        {
            int input = GetIntegerInput("Enter the maximum townhall level:");
            commands.Settings.Value.MaxTownhall = input;
            commands.Settings.Value.Write();
            return Task.CompletedTask;
        }

        private static Task ConfigureTrailingZeroes(InteractiveMenu commands)
        {
            commands.Settings.Value.TrimTrailingZeroes = !commands.Settings.Value.TrimTrailingZeroes;
            commands.Settings.Value.Write();
            string enabled = commands.Settings.Value.TrimTrailingZeroes ? "enabled" : "disabled";
            Console.WriteLine($"Trimming of trailing zeroes is now {enabled}.");
            return Task.CompletedTask;
        }

        public static string GetStringInput(string message)
        {
            Console.WriteLine(message);

            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("Invalid input");            

            return input;
        }

        private static int GetIntegerInput(string message)
        {
            Console.WriteLine(message);

            string? input = Console.ReadLine()?.Trim();

            if (!int.TryParse(input, out int result))
                throw new Exception("Invalid number");

            return result;
        }

        private static bool GetBoolInput(string message)
        {
            Console.WriteLine(message);

            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("Invalid input");

            input = input.ToLower();

            if (input == "y" || input == "yes")
                input = "true";

            if (input == "n" || input == "no")
                input = "false";

            if (!bool.TryParse(input, out bool result))
                throw new Exception("Invalid input");

            return result;
        }

        private static bool TryParse(string? input, MenuOption[] options, [NotNullWhen(true)] out MenuOption? menuOptions)
        {
            menuOptions = null;

            if (!int.TryParse(input, out int i))
                return false;

            i--;

            if (i < 0 || i >= options.Length)
                return false;

            menuOptions = options[i];

            return true;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool stopRequested = false;

            while (!stopRequested)
            {
                try
                {
                    MenuOption? menuOptions = await ExecuteMenu(MainMenu);

                    Console.WriteLine();

                    if (menuOptions != null && menuOptions.Func == null)
                    {
                        stopRequested = true;
                        _hostApplicationLifetime.StopApplication();
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "An error occured at {0}", nameof(StartAsync));
                }
            }
        }

        private async Task<MenuOption?> ExecuteMenu(MenuOption[] menuOptions)
        {
            PrintMenu(menuOptions);

            ConsoleKeyInfo key = Console.ReadKey();

            Console.WriteLine();

            string input = key.KeyChar.ToString();

            if (!TryParse(input, menuOptions, out MenuOption? options))
            {
                Console.WriteLine("Invalid option!");
                return null;
            }

            if (options.Func != null)
                await options.Func(this);

            return options;
        }
    }
}
