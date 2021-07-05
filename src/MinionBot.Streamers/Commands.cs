using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MinionBot.Streamers
{
    public sealed class Commands : IHostedService
    {
        private readonly IOptions<Settings> _options;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ClansClient _clansClient;

        public Commands(IOptions<Settings> options, IHostApplicationLifetime hostApplicationLifetime, ClansClient clansClient)
        {
            _options = options;
            _hostApplicationLifetime = hostApplicationLifetime;
            _clansClient = clansClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async() =>
            {
                bool stopRequested = false;

                while (!stopRequested)
                {
                    try
                    {
                        LogService.LogLine("1. Add a clan\n2. Remove a clan\n3. Start polling\n4. Stop polling\n5. Settings\n6. View all clans\n7. Quit\nChoose an option:");

                        //ConsoleKeyInfo inputInfo = Console.ReadKey();

                        //string input = inputInfo.KeyChar.ToString();

                        string? input = Console.ReadLine();

                        LogService.LogLine("");

                        if (input == "1")
                        {
                            string tag = GetStringInput("Enter the clan tag:");

                            if (!Clash.TryFormatTag(tag, out string? formattedTag))
                                throw new Exception("Invalid tag");

                            ClanOption? exists = _options.Value.Clans.FirstOrDefault(c => c.Tag == formattedTag);

                            if (exists != null)
                            {
                                LogService.LogLine(
$"This clan tag is already entered. The folder name is { _options.Value.Clans.First(c => c.Tag == formattedTag).Folder ?? "null" }. Enter the new folder name, or just press enter.");

                                string? newFolderName = Console.ReadLine();

                                if (newFolderName != null)
                                {
                                    foreach (ClanOption clanOption in _options.Value.Clans.Where(c => c.Folder?.ToLower() == newFolderName.ToLower()))
                                        clanOption.Folder = null;

                                    exists.Folder = newFolderName;

                                    _options.Value.Write();
                                }
                            }
                            else
                            {
                                string name = GetStringInput("Enter the clan name. This will be used as the name of the folder. Case is insensitive. Use valid characters for your operating system. If you will be changing clans often you can just use a generic folder name like 'clan a'");

                                foreach (ClanOption clanOption in _options.Value.Clans.Where(c => c.Folder?.ToLower() == name.ToLower()))
                                    clanOption.Folder = null;

                                _options.Value.Clans.Add(new ClanOption(formattedTag, name));

                                _options.Value.Write();

                                await _clansClient.AddOrUpdateAsync(formattedTag, false, true, false, true, false);

                                LogService.LogLine($"Added { formattedTag } { name }");
                            }
                        }

                        if (input == "2")
                        {
                            string name = GetStringInput("Enter the clan tag or folder name to remove:");

                            Clash.TryFormatTag(name, out string? formattedTag);

                            if (string.IsNullOrWhiteSpace(name))
                                throw new Exception("Invalid input");

                            ClanOption? clan = _options.Value.Clans.FirstOrDefault(c => c.Tag == formattedTag || c.Folder == name);

                            clan ??= _options.Value.Clans.FirstOrDefault(c => c.Folder != null && c.Folder.ToLower() == name.ToLower());

                            if (clan == null)
                                throw new Exception("Clan not found.");

                            _options.Value.Clans.Remove(clan);

                            _options.Value.Write();

                            await _clansClient.AddOrUpdateAsync(clan.Tag, false, false, false, false, false);

                            LogService.LogLine($"Removed { clan }");
                        }

                        if (input == "3")
                            await _clansClient.StartAsync(CancellationToken.None);

                        if (input == "4")
                            await _clansClient.StopAsync(CancellationToken.None);

                        if (input == "5")
                            ConfigureSettings();

                        if (input == "6")
                        {
                            LogService.LogLine($"This information is found in { Program.SettingsJson }");

                            foreach(ClanOption clanOption in _options.Value.Clans)
                            {
                                string clanName = "unknown";

                                if (clanOption.Folder != null)
                                {
                                    string clanFolder = Path.Combine(Program.ClansFolder, clanOption.Folder, "clan", "name.txt");

                                    if (File.Exists(clanFolder))
                                        clanName = File.ReadAllText(clanFolder);
                                }

                                LogService.LogLine($"{clanOption.Tag} {clanName} - { clanOption.Folder }");
                            }
                        }

                        if (input == "7")
                        {
                            _hostApplicationLifetime.StopApplication();
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.Log(LogLevel.Error, "",  e.Message);
                    }

                    LogService.LogLine("");
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        private void ConfigureSettings()
        {
            bool stopRequested = false;

            LogService.LogLine($"All information can be found at {Program.AppDataFolder}");

            while (!stopRequested)
            {
                try
                {
                    LogService.LogLine(
"1. Add your token\n2. Configure decimals\n3. Configure percent sign\n4. Set minimum townhall\n5. Set maximum townhall\n6. Toggle trim trailing zeroes.\n7. Toggle logging\n8. Main menu\nChoose an option:");

                    ConsoleKeyInfo menuInfo = Console.ReadKey();

                    string menu = menuInfo.KeyChar.ToString();

                    LogService.LogLine("");

                    if (menu == "1")
                    {
                        string input = GetStringInput("Enter your clash of clans token:");
                        _options.Value.Token = input;
                        _options.Value.Write();
                        LogService.LogLine("Restart the application for this change to take effect.");
                    }
                    else if (menu == "2")
                    {
                        int input = GetIntegerInput("Enter the number of digits after the decimal:");
                        _options.Value.DigitsAfterDecimal = input;
                        _options.Value.Write();
                    }
                    else if (menu == "3")
                    {
                        bool input = GetBoolInput("Display the percent symbol? Enter yes or no.");
                        _options.Value.IncludePercentSign = input;
                        _options.Value.Write();
                    }
                    else if (menu == "4")
                    {
                        int input = GetIntegerInput("Enter the minimum townhall level:");
                        _options.Value.MinTownhall = input;
                        _options.Value.Write();
                    }
                    else if (menu == "5")
                    {
                        int input = GetIntegerInput("Enter the maximum townhall level:");
                        _options.Value.MaxTownhall = input;
                        _options.Value.Write();
                    }
                    else if (menu == "6")
                    {
                        _options.Value.TrimTrailingZeroes = !_options.Value.TrimTrailingZeroes;

                        _options.Value.Write();

                        string enabled = _options.Value.TrimTrailingZeroes ? "enabled" : "disabled";

                        LogService.LogLine($"Trimming of trailing zeroes is now {enabled}.");
                    }
                    else if (menu == "7")
                    {
                        _options.Value.Debug = !_options.Value.Debug;

                        _options.Value.Write();

                        string enabled = _options.Value.Debug ? "enabled" : "disabled";

                        LogService.LogLine($"Debug is now {enabled}.");

                        ClansClient.Debug(_options);
                    }
                    //else if (menu == "8")
                    //{
                    //_options.Value.Verbose = !_options.Value.Verbose;

                    //_options.Value.Write();

                    //string enabled = _options.Value.Verbose ? "enabled" : "disabled";

                    //LogService.Log($"Verbose is now {enabled}.");
                    //}
                    else                    
                        stopRequested = true;

                    LogService.LogLine("");
                }
                catch (Exception e)
                {
                    LogService.Log(LogLevel.Error, "", e.Message);
                }
            }

            return;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static string GetStringInput(string message)
        {
            LogService.LogLine(message);

            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("Invalid token");            

            return input;
        }

        private static int GetIntegerInput(string message)
        {
            LogService.LogLine(message);

            string? input = Console.ReadLine()?.Trim();

            if (!int.TryParse(input, out int result))
                throw new Exception("Invalid number");

            return result;
        }

        private static bool GetBoolInput(string message)
        {
            LogService.LogLine(message);

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
    }
}
