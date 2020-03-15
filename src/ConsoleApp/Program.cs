using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwitchOverlayConsoleAppCore
{
    class Program
    {
        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            var timer = new System.Threading.Timer(e => Update(false), null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            GC.KeepAlive(timer);
            ConsoleKeyInfo cki;
            Console.TreatControlCAsInput = true;
            Console.WriteLine(@"https://github.com/devhl-labs/MinionBotForStreamers");
            Console.WriteLine("Press any key to update...");
            do
            {
                cki = Console.ReadKey();
                Console.WriteLine();
                Update(true);
            } while (
                cki.Key != ConsoleKey.Escape
            );
            await Task.Delay(-1);
        }

        public static void Update(bool folderOpened)
        {
            try
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

                var directory = System.IO.Path.GetDirectoryName(path);


                Directory.CreateDirectory(Path.Combine(directory, "documents"));
                Directory.CreateDirectory(Path.Combine(directory, "documents", "output"));

                if (!File.Exists(directory + $"{Path.DirectorySeparatorChar}documents{Path.DirectorySeparatorChar}clantag.txt"))
                {
                    File.WriteAllText(Path.Combine(directory, "documents", "clantag.txt"), "");
                }
                if (!File.Exists(directory + $"{Path.DirectorySeparatorChar}documents{Path.DirectorySeparatorChar}clash api token.txt"))
                {
                    File.WriteAllText(Path.Combine(directory, "documents", "clash api token.txt"), "");
                }
                if (!File.Exists(directory + $"{Path.DirectorySeparatorChar}documents{Path.DirectorySeparatorChar}wartag.txt"))
                {
                    File.WriteAllText(Path.Combine(directory, "documents", "wartag.txt"), "");
                }

                string clanTag = File.ReadAllText(directory + $"{Path.DirectorySeparatorChar}documents{Path.DirectorySeparatorChar}clantag.txt");
                string clashToken = File.ReadAllText(directory + $"{Path.DirectorySeparatorChar}documents{Path.DirectorySeparatorChar}clash api token.txt");
                string warTag = File.ReadAllText(directory + $"{Path.DirectorySeparatorChar}documents{Path.DirectorySeparatorChar}wartag.txt");


                string url = "";

                if (string.IsNullOrEmpty(warTag))
                {
                    url = $"https://api.clashofclans.com/v1/clans/" + clanTag.Replace("#", "%23") + "/currentwar";
                }
                else
                {
                    Console.WriteLine("Using warTag " + warTag);
                    url = $"https://api.clashofclans.com/v1/clanwarleagues/wars/" + warTag.Replace("#", "%23");
                }

                string docs = directory + $"{Path.DirectorySeparatorChar}documents{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}";

                if (clanTag == "")
                {
                    Console.WriteLine("Please enter your clan tag in clantag.txt.");
                    if (!folderOpened)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", directory + $"{Path.DirectorySeparatorChar}documents");
                        folderOpened = true;
                    }
                    return;
                }
                if (clashToken == "")
                {
                    Console.WriteLine("Please enter your clash api token.  To create one, visit developer.clashofclans.com");
                    if (!folderOpened)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", directory + $"{Path.DirectorySeparatorChar}documents");
                        folderOpened = true;
                    }
                    return;
                }


                var json = GetData(url, clashToken);

                if (json.Substring(0, 3) == "err")
                {
                    if (json.ToString() == "err" + "The remote server returned an error: (403) Forbidden.")
                    {
                        Console.WriteLine("Your war log is private or your key is wrong.");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("An error occured in looking up the clan tag.  Server responded with: " + json.Substring(3, json.Length - 3));
                        return;
                    }
                }

                War war = JsonConvert.DeserializeObject<War>(json.ToString());

                if (war.clans.All(c => c.tag == null))
                {
                    Console.WriteLine("This clan is not in war.");

                    return;
                }

                Console.WriteLine("attacks: " + war.clans.First(c => c.tag == clanTag).attacks + " || defenses: " + war.clans.First(c => c.tag != clanTag).attacks);
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}clan.txt", war.clans.First(c => c.tag == clanTag).name);

                //used to determine how many digits after the decimal
                //acceptable values are 0, 1, 2, or 3
                if (!File.Exists(docs + $"{Path.DirectorySeparatorChar}digits after decimal.txt"))
                {
                    File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}digits after decimal.txt", "2");
                }
                string p = File.ReadAllText(docs + $"{Path.DirectorySeparatorChar}digits after decimal.txt");
                int precision = 2;
                switch (p)
                {
                    case "0":
                        precision = 0;
                        break;
                    case "1":
                        precision = 1;
                        break;
                    case "2":
                        precision = 2;
                        break;
                    case "3":
                        precision = 3;
                        break;
                    case "4":
                        precision = 4;
                        break;
                    default:
                        precision = 2;
                        File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}digits after decimal.txt", "2");
                        Console.WriteLine("Valid numbers for digits after text are 0, 1, and 2.");
                        break;
                }

                //used to determine how many digits after the decimal
                //acceptable values are 0, 1, 2, or 3
                if (!File.Exists(docs + $"{Path.DirectorySeparatorChar}include percent sign.txt"))
                {
                    File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}include percent sign.txt", "true");
                }
                string signText = File.ReadAllText(docs + $"{Path.DirectorySeparatorChar}include percent sign.txt");
                bool includeSign = true;
                switch (signText)
                {
                    case "true":
                        includeSign = true;
                        break;
                    case "false":
                        includeSign = false;
                        break;
                    default:
                        includeSign = true;
                        File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}include percent sign.txt", "true");
                        Console.WriteLine("Include percent sign can only be true or false.");
                        break;
                }

                //only download the images if the enemy name is different
                WebClient webClient = new WebClient();

                try
                {
                    if (File.Exists(docs + $"{Path.DirectorySeparatorChar}clanE.txt"))
                    {
                        if (File.ReadAllText(docs + $"{Path.DirectorySeparatorChar}clanE.txt") != war.clans.First(c => c.tag != clanTag).name)
                        {
                            webClient.DownloadFile(war.clans.First(c => c.tag != clanTag).badgeUrls.large, docs + $"{Path.DirectorySeparatorChar}shieldE.png");
                            webClient.DownloadFile(war.clans.First(c => c.tag == clanTag).badgeUrls.large, docs + $"{Path.DirectorySeparatorChar}shield.png");
                        }
                    }
                    else
                    {
                        webClient.DownloadFile(war.clans.First(c => c.tag != clanTag).badgeUrls.large, docs + $"{Path.DirectorySeparatorChar}shieldE.png");
                        webClient.DownloadFile(war.clans.First(c => c.tag == clanTag).badgeUrls.large, docs + $"{Path.DirectorySeparatorChar}shield.png");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("An error occured downloading a clan shield.");
                }
                finally
                {
                    webClient.Dispose();
                }


                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}clanE.txt", war.clans.First(c => c.tag != clanTag).name);
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}attacks.txt", war.clans.First(c => c.tag == clanTag).attacks.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}attacksE.txt", war.clans.First(c => c.tag != clanTag).attacks.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}start time.txt", war.startTime);
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}prep time.txt", war.preparationStartTime);
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}end time.txt", war.endTime);
                
                string percent = string.Empty;
                if (includeSign) percent = "%";
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}destruction.txt", $"{war.clans.First(c => c.tag == clanTag).destructionPercentage}{percent}");
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}destructionE.txt", $"{war.clans.First(c => c.tag != clanTag).destructionPercentage}{percent}");

                WarStats ws = new WarStats(clanTag);
                ws.Process(war, precision);

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}nines.txt", ws.nines.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}ninesE.txt", ws.ninesE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}tens.txt", ws.tens.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}tensE.txt", ws.tensE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}elevens.txt", ws.elevens.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}elevensE.txt", ws.elevensE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}twelves.txt", ws.twelves.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}twelvesE.txt", ws.twelvesE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}thirteens.txt", ws.thirteens.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}thirteensE.txt", ws.thirteensE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9v9Three.txt", ws.nineV9.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9v9ThreeE.txt", ws.nineV9E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9v9Tries.txt", ws.nineV9T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9v9TriesE.txt", ws.nineV9TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9v9PE.txt", DecimalToString(ws.nineV9PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v10PE.txt", DecimalToString(ws.tenV10PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v11PE.txt", DecimalToString(ws.tenV11PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v10PE.txt", DecimalToString(ws.elevenv10PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v11PE.txt", DecimalToString(ws.elevenV11PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v12PE.txt", DecimalToString(ws.elevenV12PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v11PE.txt", DecimalToString(ws.twelveV11PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v12PE.txt", DecimalToString(ws.twelveV12PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v13PE.txt", DecimalToString(ws.thirteenV13PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v12PE.txt", DecimalToString(ws.thirteenV12PE, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v13PE.txt", DecimalToString(ws.twelveV13PE, precision, includeSign));

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9v9P.txt", DecimalToString(ws.nineV9P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v10P.txt", DecimalToString(ws.tenV10P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v11P.txt", DecimalToString(ws.tenV11P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v10P.txt", DecimalToString(ws.elevenV10P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v11P.txt", DecimalToString(ws.elevenV11P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v12P.txt", DecimalToString(ws.elevenV12P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v11P.txt", DecimalToString(ws.twelveV11P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v12P.txt", DecimalToString(ws.twelveV12P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v13P.txt", DecimalToString(ws.thirteenV13P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v12P.txt", DecimalToString(ws.thirteenV12P, precision, includeSign));
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v13P.txt", DecimalToString(ws.twelveV13P, precision, includeSign));

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v10Three.txt", ws.tenV10.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v10ThreeE.txt", ws.tenV10E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v10Tries.txt", ws.tenV10T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v10TriesE.txt", ws.tenV10TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v11Two+.txt", ws.tenV11.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v11Two+E.txt", ws.tenV11E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v11Tries.txt", ws.tenV11T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10v11TriesE.txt", ws.tenV11TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v10Three.txt", ws.elevenV10.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v10ThreeE.txt", ws.elevenV10E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v10Tries.txt", ws.elevenV10T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v10TriesE.txt", ws.elevenV10TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v11Three.txt", ws.elevenV11.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v11ThreeE.txt", ws.elevenV11E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v11Tries.txt", ws.elevenV11T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v11TriesE.txt", ws.elevenV11TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v12Tries.txt", ws.elevenV12T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v12TriesE.txt", ws.elevenV12TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v11Tries.txt", ws.twelveV11T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v11TriesE.txt", ws.twelveV11TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v12Tries.txt", ws.twelveV12T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v12TriesE.txt", ws.twelveV12TE.ToString());


                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v13Tries.txt", ws.thirteenV13T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v13TriesE.txt", ws.thirteenV13TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v12Tries.txt", ws.thirteenV12T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v12TriesE.txt", ws.thirteenV12TE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v13Tries.txt", ws.twelveV13T.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v13TriesE.txt", ws.twelveV13TE.ToString());











                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9 two.txt", ws.nineTwo.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9 twoE.txt", ws.nineTwoE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9 three.txt", ws.nineThree.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}9 threeE.txt", ws.nineThreeE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10 two.txt", ws.tenTwo.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10 twoE.txt", ws.tenTwoE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10 three.txt", ws.tenThree.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}10 threeE.txt", ws.tenThreeE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11 two.txt", ws.elevenTwo.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11 twoE.txt", ws.elevenTwoE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11 three.txt", ws.elevenThree.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11 threeE.txt", ws.elevenThreeE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12 two.txt", ws.twelveTwo.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12 twoE.txt", ws.twelveTwoE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12 three.txt", ws.twelveThree.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12 threeE.txt", ws.twelveThreeE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13 two.txt", ws.thirteenTwo.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13 twoE.txt", ws.thirteenTwoE.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13 three.txt", ws.thirteenThree.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13 threeE.txt", ws.thirteenThreeE.ToString());





                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v13Three.txt", ws.thirteenV13.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v13ThreeE.txt", ws.thirteenV13E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v12Three.txt", ws.thirteenV12.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}13v12ThreeE.txt", ws.thirteenV12E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v13Three.txt", ws.twelveV13.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v13ThreeE.txt", ws.twelveV13E.ToString());



                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v12Three.txt", ws.twelveV12.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v12ThreeE.txt", ws.twelveV12E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v11Three.txt", ws.twelveV11.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}12v11ThreeE.txt", ws.twelveV11E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v12Two+.txt", ws.elevenV12.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}11v12Two+E.txt", ws.elevenV12E.ToString());

                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}stars.txt", ws.stars.ToString());
                File.WriteAllText(docs + $"{Path.DirectorySeparatorChar}starsE.txt", ws.starsE.ToString());

                ws = null;

                Console.WriteLine("Updated " + DateTime.Now.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static string DecimalToString(decimal d, int p, bool sign)
        {
            string s;
            switch (p)
            {
                case 1:
                    s = d.ToString("0.0");
                    if (s.EndsWith("0"))
                    {
                        s = d.ToString("0");
                    }
                    break;

                case 2:
                    s = d.ToString("0.00");
                    if (s.EndsWith("00"))
                    {
                        s = d.ToString("0");
                    }
                    break;
                case 3:
                    s = d.ToString("0.000");
                    if (s.EndsWith("000"))
                    {
                        s = d.ToString("0");
                    }
                    break;
                default:
                    s = d.ToString("0");
                    break;
            }

            if (sign) { s = s + "%"; }
            return s;
        }

        public static string GetData(string url, string clashToken)
        {
            try
            {
                var myUri = new Uri(url);
                var myWebRequest = WebRequest.Create(myUri);
                var myHttpWebRequest = (HttpWebRequest)myWebRequest;
                myHttpWebRequest.PreAuthenticate = true;
                myHttpWebRequest.Headers.Add("Authorization", "Bearer " + clashToken);
                myHttpWebRequest.Accept = "application/json";

                var myWebResponse = myWebRequest.GetResponse();
                var responseStream = myWebResponse.GetResponseStream();
                if (responseStream == null)
                {
                    return null;
                }

                var myStreamReader = new StreamReader(responseStream, Encoding.Default);
                var json = myStreamReader.ReadToEnd();

                myWebResponse.Close();
                return json;
            }
            catch (Exception e)
            {
                return "err" + e.Message;
            }

        }
    }
}