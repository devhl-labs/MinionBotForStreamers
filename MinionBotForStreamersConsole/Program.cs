using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace DiscordExampleBot
{
    #region current war
    public class war
    {
        public string state { get; set; }
        public int teamSize { get; set; }
        public string preparationStartTime { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public bool timersSet { get; set; }
        public Clan clan { get; set; }
        public Opponent opponent { get; set; }
        public DateTime DateUpdated { get; set; }

    }

    public class Clan
    {
        public string tag { get; set; }
        public string name { get; set; }
        public Badgeurls1 badgeUrls { get; set; }
        public int clanLevel { get; set; }
        public int attacks { get; set; }
        public int stars { get; set; }
        public float destructionPercentage { get; set; }
        [JsonProperty(PropertyName = "members")]
        public List<Members> member { get; set; }
    }

    public class Badgeurls1
    {
        public string small { get; set; }
        public string large { get; set; }
        public string medium { get; set; }
    }

    public class Members
    {
        public string tag { get; set; }
        public string name { get; set; }
        public int townhallLevel { get; set; }
        public int mapPosition { get; set; }
        [JsonProperty(PropertyName = "attacks")]
        public List<Attacks> attack { get; set; }
        public int opponentAttacks { get; set; }
        public Bestopponentattack bestOpponentAttack { get; set; }
    }

    public class Bestopponentattack
    {
        public string attackerTag { get; set; }
        public string defenderTag { get; set; }
        public int stars { get; set; }
        public int destructionPercentage { get; set; }
        public int order { get; set; }
    }

    public class Attacks
    {
        public string attackerTag { get; set; }
        public string defenderTag { get; set; }
        public int stars { get; set; }
        public int destructionPercentage { get; set; }
        public int order { get; set; }
        public int manualOrder { get; set; }
    }

    public class Opponent
    {
        public string tag { get; set; }
        public string name { get; set; }
        public Badgeurls2 badgeUrls { get; set; }
        public int clanLevel { get; set; }
        public int attacks { get; set; }
        public int stars { get; set; }
        public float destructionPercentage { get; set; }
        [JsonProperty(PropertyName = "members")]
        public List<Members1> member { get; set; }
    }

    public class Badgeurls2
    {
        public string small { get; set; }
        public string large { get; set; }
        public string medium { get; set; }
    }

    public class Members1
    {
        public string tag { get; set; }
        public string name { get; set; }
        public int townhallLevel { get; set; }
        public int mapPosition { get; set; }
        public int opponentAttacks { get; set; }
        public Bestopponentattack1 bestOpponentAttack { get; set; }
        [JsonProperty(PropertyName = "attacks")]
        public List<Attacks1> attack { get; set; }
    }

    public class Bestopponentattack1
    {
        public string attackerTag { get; set; }
        public string defenderTag { get; set; }
        public int stars { get; set; }
        public int destructionPercentage { get; set; }
        public int order { get; set; }
    }

    public class Attacks1
    {
        public string attackerTag { get; set; }
        public string defenderTag { get; set; }
        public int stars { get; set; }
        public int destructionPercentage { get; set; }
        public int order { get; set; }
        public int manualOrder { get; set; }
    }


    #endregion

    #region stats

    public class stats
    {
        public int stars { get; set; }
        public int destructionPercentage { get; set; }
        public int friendlyTH { get; set; }
        public int enemyTH { get; set; }
        public string attackerTag { get; set; }
        public string defenderTag { get; set; }
        public int mapPosition { get; set; }
    }

    public class WarStats
    {
        public void Process(war war, int p)
        {
            try
            {
                elevenLeft = war.clan.member.Where(a => a.townhallLevel == 11).Count() * 2;
                tenLeft = war.clan.member.Where(a => a.townhallLevel == 10).Count() * 2;
                nineLeft = war.clan.member.Where(a => a.townhallLevel == 9).Count() * 2;
                //eightLeft = saved.war.clan.member.Where(a => a.townhallLevel == 8).Count() * 2;

                elevenLeftE = war.opponent.member.Where(a => a.townhallLevel == 11).Count() * 2;
                tenLeftE = war.opponent.member.Where(a => a.townhallLevel == 10).Count() * 2;
                nineLeftE = war.opponent.member.Where(a => a.townhallLevel == 9).Count() * 2;
                //eightLeft = saved.war.opponent.member.Where(a => a.townhallLevel == 8).Count() * 2;

                decimal DestructionSum = 0;
                decimal DestructionSumE = 0;
                int maxOrder = 0;

                nines = war.clan.member.Where(a => a.townhallLevel == 9).Count();
                ninesE = war.opponent.member.Where(a => a.townhallLevel == 9).Count();
                tens = war.clan.member.Where(a => a.townhallLevel == 10).Count();
                tensE = war.opponent.member.Where(a => a.townhallLevel == 10).Count();
                elevens = war.clan.member.Where(a => a.townhallLevel == 11).Count();
                elevensE = war.opponent.member.Where(a => a.townhallLevel == 11).Count();

                #region old code to compute enemy stats
                ////compute enemy stats
                //foreach (Members friend in saved.war.clan.member ?? new List<Members>()) {
                //    int tempStars = 0;
                //    int tempDest = 0;
                //    int tempDefTh = 0;

                //    foreach (Members1 enemy in saved.war.opponent.member ?? new List<Members1>()) {
                //        foreach(Attacks1 c in (enemy.attack ?? new List<Attacks1>()).Where(d=>d.defenderTag == friend.tag)) {
                //            if (c.order > maxOrder) { maxOrder = c.order; }
                //            attacksE += 1;
                //            if (c.order > maxOrder) { maxOrder = c.order; }
                //            if (enemy.townhallLevel == 11) { elevenLeftE -= 1; }
                //            if (enemy.townhallLevel == 10) { tenLeftE -= 1; }
                //            if (enemy.townhallLevel == 9) { nineLeftE -= 1; }
                //            //if (enemy.townhallLevel == 8) { eightLeftE -= 1; }

                //            if (friend.townhallLevel == 11 && enemy.townhallLevel == 11) {
                //                elevenV11TE += 1;
                //                if(c.stars == 3) { elevenV11E += 1; }
                //            }
                //            if (friend.townhallLevel == 11 && enemy.townhallLevel == 10) {
                //                tenV11TE += 1;
                //                if (c.stars >= 2) { tenV11E += 1; }
                //            }
                //            if (friend.townhallLevel == 10 && enemy.townhallLevel == 11) { //dip
                //                elevenV10E += 1;
                //                if (c.stars == 3) { elevenV10TE += 1; }
                //            }
                //            if (friend.townhallLevel == 10 && enemy.townhallLevel == 10) {
                //                tenV10TE += 1;
                //                if (c.stars == 3) { tenV10E += 1; }
                //            }
                //            if (enemy.townhallLevel == 10 && friend.townhallLevel == 9) { //dip
                //                tenV9TE += 1;
                //                if (c.stars == 3) { tenV9E += 1; }
                //            }
                //            if (friend.townhallLevel == 9 && enemy.townhallLevel == 9) {
                //                nineV9TE += 1;
                //                if (c.stars == 3) { nineV9E += 1; }
                //            }

                //            if (c.stars > tempStars) {
                //                tempStars = c.stars;
                //                tempDest = c.destructionPercentage;
                //                tempDefTh = friend.townhallLevel;
                //            }
                //        }
                //    }
                //    starsE += tempStars;
                //    DestructionSumE += tempDest;
                //    if (tempDefTh == 11 && tempStars == 3) { elevenThreeE += 1; }
                //    if (tempDefTh == 11 && tempStars == 2) { elevenTwoE += 1; }
                //    if (tempDefTh == 10 && tempStars == 3) { tenThreeE += 1; }
                //    if (tempDefTh == 10 && tempStars == 2) { tenTwoE += 1; }
                //}
                //destE = Math.Round(DestructionSumE / saved.war.teamSize, 2);
                #endregion
                //compute enemy stats
                List<stats> enemyAttacks = new List<stats>();
                foreach (Members1 enemy in (war.opponent.member ?? new List<Members1>()))
                {
                    foreach (Attacks1 attack in (enemy.attack ?? new List<Attacks1>()))
                    {
                        stats b = new stats();
                        b.destructionPercentage = attack.destructionPercentage;
                        b.enemyTH = war.opponent.member.Where(x => x.tag == attack.attackerTag).FirstOrDefault().townhallLevel; //GetEnemyTh(attack.attackerTag, saved);
                        b.friendlyTH = war.clan.member.Where(x => x.tag == attack.defenderTag).FirstOrDefault().townhallLevel; //GetFriendlyTh(attack.defenderTag, saved);
                        b.stars = attack.stars;
                        b.attackerTag = attack.attackerTag;
                        b.defenderTag = attack.defenderTag;
                        b.mapPosition = war.clan.member.Where(c => c.tag == attack.defenderTag).First().mapPosition;
                        enemyAttacks.Add(b);
                    }
                }

                foreach (Members friend in war.clan.member)
                {
                    bool found = false;
                    foreach (stats a in (enemyAttacks ?? new List<stats>()).Where(a => a.defenderTag == friend.tag).OrderByDescending(a => a.stars).ThenByDescending(a => a.destructionPercentage))
                    {
                        if (!found)
                        {
                            found = true;
                            starsE += a.stars;
                            DestructionSumE += a.destructionPercentage;
                            if (friend.townhallLevel == 11 && a.stars == 3) { elevenThreeE += 1; }
                            if (friend.townhallLevel == 11 && a.stars == 2) { elevenTwoE += 1; }
                            if (friend.townhallLevel == 10 && a.stars == 3) { tenThreeE += 1; }
                            if (friend.townhallLevel == 10 && a.stars == 2) { tenTwoE += 1; }

                        }
                        if (a.enemyTH == 11) { elevenLeftE -= 1; }
                        if (a.enemyTH == 10) { tenLeftE -= 1; }
                        if (a.enemyTH == 9) { nineLeftE -= 1; }
                        //if (a.enemyTH == 8) { eightLeftE -= 1; }

                        attacksE += 1;
                        if (friend.townhallLevel == 11 && a.enemyTH == 11)
                        {
                            elevenV11TE += 1;
                            if (a.stars == 3) { elevenV11E += 1; }
                        }
                        if (a.enemyTH == 11 && friend.townhallLevel == 10)
                        { //dip
                            elevenV10TE += 1;
                            if (a.stars == 3) { elevenV10E += 1; }
                        }
                        if (a.enemyTH == 10 && friend.townhallLevel == 11)
                        {
                            tenV11TE += 1;
                            if (a.stars >= 2) { tenV11E += 1; }
                        }
                        if (friend.townhallLevel == 10 && a.enemyTH == 10)
                        {
                            tenV10TE += 1;
                            if (a.stars == 3) { tenV10E += 1; }
                        }
                        if (a.enemyTH == 10 && friend.townhallLevel == 9)
                        { //dip
                            tenV9TE += 1;
                            if (a.stars == 3) { tenV9E += 1; }
                        }
                        if (friend.townhallLevel == 9 && a.enemyTH == 9)
                        {
                            nineV9TE += 1;
                            if (a.stars == 3) { nineV9E += 1; }
                        }
                    }
                }
                destE = Math.Round(DestructionSumE / war.teamSize, 2);

                if (nineV9TE == 0)
                {
                    nineV9PE = Math.Round((decimal)0, p);
                }
                else
                {
                    nineV9PE = Math.Round((decimal)nineV9E / (decimal)nineV9TE * 100, p);
                }

                if (tenV10TE == 0)
                {
                    tenV10PE = Math.Round((decimal)0, p);
                }
                else
                {
                    tenV10PE = Math.Round((decimal)tenV10E / (decimal)tenV10TE * 100, p);
                }

                if (tenV11TE == 0)
                {
                    tenV11PE = Math.Round((decimal)0, p);
                }
                else
                {
                    tenV11PE = Math.Round((decimal)tenV11E / (decimal)tenV11TE * 100, p);
                }

                if (elevenV10TE == 0)
                {
                    elevenv10PE = Math.Round((decimal)0, p);
                }
                else
                {
                    elevenv10PE = Math.Round((decimal)elevenV10E / (decimal)elevenV10TE * 100, p);
                }

                if (elevenV11TE == 0)
                {
                    elevenV11PE = Math.Round((decimal)0, p);
                }
                else
                {
                    elevenV11PE = Math.Round((decimal)elevenV11E / (decimal)elevenV11TE * 100, p);
                }


                //compute friendly stats
                List<stats> friendAttacks = new List<stats>();
                foreach (Members friend in (war.clan.member ?? new List<Members>()))
                {
                    foreach (Attacks attack in (friend.attack ?? new List<Attacks>()))
                    {
                        stats b = new stats();
                        b.destructionPercentage = attack.destructionPercentage;
                        b.enemyTH = war.opponent.member.Where(x => x.tag == attack.defenderTag).FirstOrDefault().townhallLevel; //GetEnemyTh(attack.defenderTag, saved);
                        b.friendlyTH = war.clan.member.Where(x => x.tag == attack.attackerTag).FirstOrDefault().townhallLevel; //GetFriendlyTh(attack.attackerTag, saved);
                        b.stars = attack.stars;
                        b.attackerTag = attack.attackerTag;
                        b.defenderTag = attack.defenderTag;
                        b.mapPosition = war.opponent.member.Where(c => c.tag == attack.defenderTag).First().mapPosition;
                        friendAttacks.Add(b);
                    }
                }

                foreach (Members1 enemy in war.opponent.member)
                {
                    bool found = false;
                    foreach (stats a in (friendAttacks ?? new List<stats>()).Where(a => a.defenderTag == enemy.tag).OrderByDescending(a => a.stars).ThenByDescending(a => a.destructionPercentage))
                    {
                        if (!found)
                        {
                            found = true;
                            stars += a.stars;
                            DestructionSum += a.destructionPercentage;
                            if (enemy.townhallLevel == 11 && a.stars == 3) { elevenThree += 1; }
                            if (enemy.townhallLevel == 11 && a.stars == 2) { elevenTwo += 1; }
                            if (enemy.townhallLevel == 10 && a.stars == 3) { tenThree += 1; }
                            if (enemy.townhallLevel == 10 && a.stars == 2) { tenTwo += 1; }
                        }
                        if (a.friendlyTH == 11) { elevenLeft -= 1; }
                        if (a.friendlyTH == 10) { tenLeft -= 1; }
                        if (a.friendlyTH == 9) { nineLeft -= 1; }
                        //if (a.friendlyTH == 8) { eightLeft -= 1; }

                        attacks += 1;
                        if (enemy.townhallLevel == 11 && a.friendlyTH == 11)
                        {
                            elevenV11T += 1;
                            if (a.stars == 3) { elevenV11 += 1; }
                        }
                        if (a.friendlyTH == 11 && enemy.townhallLevel == 10)
                        { //dip
                            elevenV10T += 1;
                            if (a.stars == 3) { elevenV10 += 1; }
                        }
                        if (a.friendlyTH == 10 && enemy.townhallLevel == 11)
                        {
                            tenV11T += 1;
                            if (a.stars >= 2) { tenV11 += 1; }
                        }
                        if (enemy.townhallLevel == 10 && a.friendlyTH == 10)
                        {
                            tenV10T += 1;
                            if (a.stars == 3) { tenV10 += 1; }
                        }
                        if (a.friendlyTH == 10 && enemy.townhallLevel == 9)
                        { //dip
                            tenV9T += 1;
                            if (a.stars == 3) { tenV9 += 1; }
                        }
                        if (enemy.townhallLevel == 9 && a.friendlyTH == 9)
                        {
                            nineV9T += 1;
                            if (a.stars == 3) { nineV9 += 1; }
                        }
                    }
                }
                dest = Math.Round(DestructionSum / war.teamSize, 2);

                if (nineV9T == 0)
                {
                    nineV9P = Math.Round((decimal)0, p);
                }
                else
                {
                    nineV9P = Math.Round((decimal)nineV9 / (decimal)nineV9T * 100, p);
                }

                if (tenV10T == 0)
                {
                    tenV10P = Math.Round((decimal)0, p);
                }
                else
                {
                    tenV10P = Math.Round((decimal)tenV10 / (decimal)tenV10T * 100, p);
                }

                if (tenV11T == 0)
                {
                    tenV11P = Math.Round((decimal)0, p);
                }
                else
                {
                    tenV11P = Math.Round((decimal)tenV11 / (decimal)tenV11T * 100, p);
                }

                if (elevenV10T == 0)
                {
                    elevenV10P = Math.Round((decimal)0, p);
                }
                else
                {
                    elevenV10P = Math.Round((decimal)elevenV10 / (decimal)elevenV10T * 100, p);
                }

                if (elevenV11T == 0)
                {
                    elevenV11P = Math.Round((decimal)0, p);
                }
                else
                {
                    elevenV11P = Math.Round((decimal)elevenV11 / (decimal)elevenV11T * 100, p);
                }


                this.OrderOfLastAttack = maxOrder;
                this.err = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                this.err = true;
            }

        }
        public string WarId { get; set; }
        public int OrderOfLastAttack { get; set; }
        public bool DownloadedFromSC { get; set; }
        public bool err { get; set; }

        public int elevenLeft { get; set; }
        public int tenLeft { get; set; }
        public int nineLeft { get; set; }
        public int eightLeft { get; set; }

        public int elevenLeftE { get; set; }
        public int tenLeftE { get; set; }
        public int nineLeftE { get; set; }
        public int eightLeftE { get; set; }

        public int stars { get; set; }
        public int starsE { get; set; }
        public int attacks { get; set; }
        public int attacksE { get; set; }
        public decimal dest { get; set; }
        public decimal destE { get; set; }
        //public string breakdown { get; set; }
        //public string breakdownE { get; set; }
        public int nines { get; set; }
        public int ninesE { get; set; }
        public int tens { get; set; }
        public int tensE { get; set; }
        public int elevens { get; set; }
        public int elevensE { get; set; }



        public int elevenThree { get; set; }
        public int elevenThreeE { get; set; }
        public int elevenTwo { get; set; }
        public int elevenTwoE { get; set; }

        public int tenThree { get; set; }
        public int tenThreeE { get; set; }
        public int tenTwo { get; set; }
        public int tenTwoE { get; set; }

        public int elevenV11 { get; set; }
        public int elevenV11T { get; set; }
        public decimal elevenV11P { get; set; }
        public int elevenV11E { get; set; }
        public int elevenV11TE { get; set; }
        public decimal elevenV11PE { get; set; }

        public int elevenV10 { get; set; }
        public int elevenV10T { get; set; }
        public decimal elevenV10P { get; set; }
        public int elevenV10E { get; set; }
        public int elevenV10TE { get; set; }
        public decimal elevenv10PE { get; set; }

        public int tenV11 { get; set; }
        public int tenV11T { get; set; }
        public decimal tenV11P { get; set; }
        public int tenV11E { get; set; }
        public int tenV11TE { get; set; }
        public decimal tenV11PE { get; set; }

        public int tenV10 { get; set; }
        public int tenV10T { get; set; }
        public decimal tenV10P { get; set; }
        public int tenV10E { get; set; }
        public int tenV10TE { get; set; }
        public decimal tenV10PE { get; set; }

        public int tenV9 { get; set; }
        public int tenV9T { get; set; }
        public decimal tenV9P { get; set; }
        public int tenV9E { get; set; }
        public int tenV9TE { get; set; }
        public decimal tenV9PE { get; set; }

        public int nineV9 { get; set; }
        public int nineV9T { get; set; }
        public decimal nineV9P { get; set; }
        public int nineV9E { get; set; }
        public int nineV9TE { get; set; }
        public decimal nineV9PE { get; set; }
    }
    #endregion

    public class Program
    {
        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            var timer = new System.Threading.Timer(e => update(false), null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            GC.KeepAlive(timer);
            ConsoleKeyInfo cki;
            Console.TreatControlCAsInput = true;
            Console.WriteLine(@"https://github.com/realdevhl/Clash-of-Clans-Stream-Overlay");
            Console.WriteLine("Press any key to update...");
            do
            {
                cki = Console.ReadKey();
                Console.WriteLine();
                update(true);
            } while (
                cki.Key != ConsoleKey.Escape
            );
            await Task.Delay(-1);
        }

        public static void update(bool folderOpened)
        {
            try
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var directory = System.IO.Path.GetDirectoryName(path);

                if (!File.Exists(directory + @"\documents"))
                {
                    Directory.CreateDirectory(directory + @"\documents");
                }
                if (!File.Exists(directory + @"\documents\output"))
                {
                    Directory.CreateDirectory(directory + @"\documents\output");
                }

                if (!File.Exists(directory + @"\documents\clantag.txt"))
                {
                    File.WriteAllText(directory + @"\documents\clantag.txt", "");
                }
                if (!File.Exists(directory + @"\documents\clash api token.txt"))
                {
                    File.WriteAllText(directory + @"\documents\clash api token.txt", "");
                }

                string clanTag = File.ReadAllText(directory + @"\documents\clantag.txt");
                string clashToken = File.ReadAllText(directory + @"\documents\clash api token.txt");
                string url = @"https://api.clashofclans.com/v1/clans/" + clanTag.Replace("#", "%23") + "/currentwar";
                string docs = directory + @"\documents\output\";

                if (clanTag == "")
                {
                    Console.WriteLine("Please enter your clan tag.");
                    if (!folderOpened)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", directory + @"\documents");
                        folderOpened = true;
                    }
                    return;
                }
                if (clashToken == "")
                {
                    Console.WriteLine("Please enter your clash api token.  To create one, visit developer.clashofclans.com");
                    if (!folderOpened)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", directory + @"\documents");
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

                war war = JsonConvert.DeserializeObject<war>(json.ToString());
                Console.WriteLine("attacks: " + war.clan.attacks + " || defenses: " + war.opponent.attacks);
                File.WriteAllText(docs + @"\clan.txt", war.clan.name);

                //used to determine how many digits after the decimal
                //acceptable values are 0, 1, 2, or 3
                if (!File.Exists(docs + @"\digits after decimal.txt"))
                {
                    File.WriteAllText(docs + @"\digits after decimal.txt", "2");
                }
                string p = File.ReadAllText(docs + @"\digits after decimal.txt");
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
                    default:
                        precision = 2;
                        File.WriteAllText(docs + @"\digits after decimal.txt", "2");
                        Console.WriteLine("Valid numbers for digits after text are 0, 1, and 2.");
                        break;
                }

                //used to determine how many digits after the decimal
                //acceptable values are 0, 1, 2, or 3
                if (!File.Exists(docs + @"\include percent sign.txt"))
                {
                    File.WriteAllText(docs + @"\include percent sign.txt", "true");
                }
                string signText = File.ReadAllText(docs + @"\include percent sign.txt");
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
                        File.WriteAllText(docs + @"\include percent sign.txt", "true");
                        Console.WriteLine("Include percent sign can only be true or false.");
                        break;
                }

                //only download the images if the enemy name is different
                if (File.Exists(docs + @"\clanE.txt"))
                {
                    if (File.ReadAllText(docs + @"\clanE.txt") != war.opponent.name)
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(war.opponent.badgeUrls.large, docs + @"\shieldE.png");
                        webClient.DownloadFile(war.clan.badgeUrls.large, docs + @"\shield.png");
                    }
                }
                else
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(war.opponent.badgeUrls.large, docs + @"\shieldE.png");
                    webClient.DownloadFile(war.clan.badgeUrls.large, docs + @"\shield.png");
                }


                File.WriteAllText(docs + @"\clanE.txt", war.opponent.name);
                File.WriteAllText(docs + @"\attacks.txt", war.clan.attacks.ToString());
                File.WriteAllText(docs + @"\attacksE.txt", war.opponent.attacks.ToString());
                File.WriteAllText(docs + @"\start time.txt", war.startTime);
                File.WriteAllText(docs + @"\prep time.txt", war.preparationStartTime);
                File.WriteAllText(docs + @"\end time.txt", war.endTime);


                WarStats ws = new WarStats();
                ws.Process(war, precision);

                File.WriteAllText(docs + @"\nines.txt", ws.nines.ToString());
                File.WriteAllText(docs + @"\ninesE.txt", ws.ninesE.ToString());

                File.WriteAllText(docs + @"\tens.txt", ws.tens.ToString());
                File.WriteAllText(docs + @"\tensE.txt", ws.tensE.ToString());

                File.WriteAllText(docs + @"\elevens.txt", ws.elevens.ToString());
                File.WriteAllText(docs + @"\elevensE.txt", ws.elevensE.ToString());

                File.WriteAllText(docs + @"\9v9Three.txt", ws.nineV9.ToString());
                File.WriteAllText(docs + @"\9v9ThreeE.txt", ws.nineV9E.ToString());

                File.WriteAllText(docs + @"\9v9Tries.txt", ws.nineV9T.ToString());
                File.WriteAllText(docs + @"\9v9TriesE.txt", ws.nineV9TE.ToString());

                File.WriteAllText(docs + @"\9v9PE.txt", DecimalToString(ws.nineV9PE, precision, includeSign));
                File.WriteAllText(docs + @"\10v10PE.txt", DecimalToString(ws.tenV10PE, precision, includeSign));
                File.WriteAllText(docs + @"\10v11PE.txt", DecimalToString(ws.tenV11PE, precision, includeSign));
                File.WriteAllText(docs + @"\11v10PE.txt", DecimalToString(ws.elevenv10PE, precision, includeSign));
                File.WriteAllText(docs + @"\11v11PE.txt", DecimalToString(ws.elevenV11PE, precision, includeSign));


                File.WriteAllText(docs + @"\9v9P.txt", DecimalToString(ws.nineV9P, precision, includeSign));
                File.WriteAllText(docs + @"\10v10P.txt", DecimalToString(ws.tenV10P, precision, includeSign));
                File.WriteAllText(docs + @"\10v11P.txt", DecimalToString(ws.tenV11P, precision, includeSign));
                File.WriteAllText(docs + @"\11v10P.txt", DecimalToString(ws.elevenV10P, precision, includeSign));
                File.WriteAllText(docs + @"\11v11P.txt", DecimalToString(ws.elevenV11P, precision, includeSign));

                File.WriteAllText(docs + @"\10v10Three.txt", ws.tenV10.ToString());
                File.WriteAllText(docs + @"\10v10ThreeE.txt", ws.tenV10E.ToString());

                File.WriteAllText(docs + @"\10v10Tries.txt", ws.tenV10T.ToString());
                File.WriteAllText(docs + @"\10v10TriesE.txt", ws.tenV10TE.ToString());

                File.WriteAllText(docs + @"\10v11Two+.txt", ws.tenV11.ToString());
                File.WriteAllText(docs + @"\10v11Two+E.txt", ws.tenV11E.ToString());

                File.WriteAllText(docs + @"\10v11Tries.txt", ws.tenV11T.ToString());
                File.WriteAllText(docs + @"\10v11TriesE.txt", ws.tenV11TE.ToString());

                File.WriteAllText(docs + @"\11v10Three.txt", ws.elevenV10.ToString());
                File.WriteAllText(docs + @"\11v10ThreeE.txt", ws.elevenV10E.ToString());

                File.WriteAllText(docs + @"\11v10Tries.txt", ws.elevenV10T.ToString());
                File.WriteAllText(docs + @"\11v10TriesE.txt", ws.elevenV10TE.ToString());

                File.WriteAllText(docs + @"\11v11Three.txt", ws.elevenV11.ToString());
                File.WriteAllText(docs + @"\11v11ThreeE.txt", ws.elevenV11E.ToString());

                File.WriteAllText(docs + @"\11v11Tries.txt", ws.elevenV11T.ToString());
                File.WriteAllText(docs + @"\11v11TriesE.txt", ws.elevenV11TE.ToString());

                File.WriteAllText(docs + @"\10 two.txt", ws.tenTwo.ToString());
                File.WriteAllText(docs + @"\10 twoE.txt", ws.tenTwoE.ToString());

                File.WriteAllText(docs + @"\10 three.txt", ws.tenThree.ToString());
                File.WriteAllText(docs + @"\10 threeE.txt", ws.tenThreeE.ToString());

                File.WriteAllText(docs + @"\11 two.txt", ws.elevenTwo.ToString());
                File.WriteAllText(docs + @"\11 twoE.txt", ws.elevenTwoE.ToString());

                File.WriteAllText(docs + @"\11 three.txt", ws.elevenThree.ToString());
                File.WriteAllText(docs + @"\11 threeE.txt", ws.elevenThreeE.ToString());

                File.WriteAllText(docs + @"\stars.txt", ws.stars.ToString());
                File.WriteAllText(docs + @"\starsE.txt", ws.starsE.ToString());

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
