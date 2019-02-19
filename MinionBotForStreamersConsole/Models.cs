using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitchOverlayConsoleAppCore
{
    #region current war
    public class War
    {
        public string state { get; set; }
        public int teamSize { get; set; }
        public string preparationStartTime { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public bool timersSet { get; set; }
        private Clan _clan;

        public Clan clan
        {
            get
            {
                return _clan;
            }

            set
            {
                _clan = value;
                clans.Add(_clan);
                clans = clans.OrderBy(c => c.tag).ToList();
            }
        }
        private Clan _opponent;

        public Clan opponent
        {
            get
            {
                return _opponent;
            }

            set
            {
                _opponent = value;
                clans.Add(_opponent);
                clans = clans.OrderBy(c => c.tag).ToList();
            }
        }
        public DateTime DateUpdated { get; set; }

        public List<Clan> clans { get; private set; } = new List<Clan>();

    }

    public class Clan
    {
        public string tag { get; set; }
        public string name { get; set; }
        public Badgeurls badgeUrls { get; set; }
        public int clanLevel { get; set; }
        public int attacks { get; set; }
        public int stars { get; set; }
        public float destructionPercentage { get; set; }
        [JsonProperty(PropertyName = "members")]
        public List<Members> member { get; set; }
    }

    public class Badgeurls
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

    //public class Opponent
    //{
    //    public string tag { get; set; }
    //    public string name { get; set; }
    //    public Badgeurls2 badgeUrls { get; set; }
    //    public int clanLevel { get; set; }
    //    public int attacks { get; set; }
    //    public int stars { get; set; }
    //    public float destructionPercentage { get; set; }
    //    [JsonProperty(PropertyName = "members")]
    //    public List<Members1> member { get; set; }
    //}

    //public class Badgeurls2
    //{
    //    public string small { get; set; }
    //    public string large { get; set; }
    //    public string medium { get; set; }
    //}

    //public class Members1
    //{
    //    public string tag { get; set; }
    //    public string name { get; set; }
    //    public int townhallLevel { get; set; }
    //    public int mapPosition { get; set; }
    //    public int opponentAttacks { get; set; }
    //    public Bestopponentattack1 bestOpponentAttack { get; set; }
    //    [JsonProperty(PropertyName = "attacks")]
    //    public List<Attacks1> attack { get; set; }
    //}

    //public class Bestopponentattack1
    //{
    //    public string attackerTag { get; set; }
    //    public string defenderTag { get; set; }
    //    public int stars { get; set; }
    //    public int destructionPercentage { get; set; }
    //    public int order { get; set; }
    //}

    //public class Attacks1
    //{
    //    public string attackerTag { get; set; }
    //    public string defenderTag { get; set; }
    //    public int stars { get; set; }
    //    public int destructionPercentage { get; set; }
    //    public int order { get; set; }
    //    public int manualOrder { get; set; }
    //}


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
        public WarStats(string clanTag)
        {
            ClanTag = clanTag;
        }

        public void Process(War war, int p)
        {
            try
            {
                twelveLeft = war.clans.First(c => c.tag == ClanTag).member.Where(a => a.townhallLevel == 12).Count() * 2;
                elevenLeft = war.clans.First(c => c.tag == ClanTag).member.Where(a => a.townhallLevel == 11).Count() * 2;
                tenLeft = war.clans.First(c => c.tag == ClanTag).member.Where(a => a.townhallLevel == 10).Count() * 2;
                nineLeft = war.clans.First(c => c.tag == ClanTag).member.Where(a => a.townhallLevel == 9).Count() * 2;
                //eightLeft = saved.war.clan.member.Where(a => a.townhallLevel == 8).Count() * 2;

                twelveLeftE = war.clans.First(c => c.tag != ClanTag).member.Where(a => a.townhallLevel == 12).Count() * 2;
                elevenLeftE = war.clans.First(c => c.tag != ClanTag).member.Where(a => a.townhallLevel == 11).Count() * 2;
                tenLeftE = war.clans.First(c => c.tag != ClanTag).member.Where(a => a.townhallLevel == 10).Count() * 2;
                nineLeftE = war.clans.First(c => c.tag != ClanTag).member.Where(a => a.townhallLevel == 9).Count() * 2;
                //eightLeft = saved.war.opponent.member.Where(a => a.townhallLevel == 8).Count() * 2;

                decimal DestructionSum = 0;
                decimal DestructionSumE = 0;
                int maxOrder = 0;

                nines = war.clans.First(c => c.tag == ClanTag).member.Where(a => a.townhallLevel == 9).Count();
                ninesE = war.clans.First(c => c.tag != ClanTag).member.Where(a => a.townhallLevel == 9).Count();
                tens = war.clans.First(c => c.tag == ClanTag).member.Where(a => a.townhallLevel == 10).Count();
                tensE = war.clans.First(c => c.tag != ClanTag).member.Where(a => a.townhallLevel == 10).Count();
                elevens = war.clans.First(c => c.tag == ClanTag).member.Where(a => a.townhallLevel == 11).Count();
                elevensE = war.clans.First(c => c.tag != ClanTag).member.Where(a => a.townhallLevel == 11).Count();
                twelves = war.clans.First(c => c.tag == ClanTag).member.Where(a => a.townhallLevel == 12).Count();
                twelvesE = war.clans.First(c => c.tag != ClanTag).member.Where(a => a.townhallLevel == 12).Count();

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
                foreach (Members enemy in (war.clans.First(c => c.tag != ClanTag).member ?? new List<Members>()))
                {
                    foreach (Attacks attack in (enemy.attack ?? new List<Attacks>()))
                    {
                        stats b = new stats();
                        b.destructionPercentage = attack.destructionPercentage;
                        b.enemyTH = war.clans.First(c => c.tag != ClanTag).member.Where(x => x.tag == attack.attackerTag).FirstOrDefault().townhallLevel; //GetEnemyTh(attack.attackerTag, saved);
                        b.friendlyTH = war.clans.First(c => c.tag == ClanTag).member.Where(x => x.tag == attack.defenderTag).FirstOrDefault().townhallLevel; //GetFriendlyTh(attack.defenderTag, saved);
                        b.stars = attack.stars;
                        b.attackerTag = attack.attackerTag;
                        b.defenderTag = attack.defenderTag;
                        b.mapPosition = war.clans.First(c => c.tag == ClanTag).member.Where(c => c.tag == attack.defenderTag).First().mapPosition;
                        enemyAttacks.Add(b);
                    }
                }

                foreach (Members friend in war.clans.First(c => c.tag == ClanTag).member)
                {
                    bool found = false;
                    foreach (stats a in (enemyAttacks ?? new List<stats>()).Where(a => a.defenderTag == friend.tag).OrderByDescending(a => a.stars).ThenByDescending(a => a.destructionPercentage))
                    {
                        if (!found)
                        {
                            found = true;
                            starsE += a.stars;
                            DestructionSumE += a.destructionPercentage;
                            if (friend.townhallLevel == 12 && a.stars == 3) { twelveThreeE += 1; }
                            if (friend.townhallLevel == 12 && a.stars == 2) { twelveTwoE += 1; }
                            if (friend.townhallLevel == 11 && a.stars == 3) { elevenThreeE += 1; }
                            if (friend.townhallLevel == 11 && a.stars == 2) { elevenTwoE += 1; }
                            if (friend.townhallLevel == 10 && a.stars == 3) { tenThreeE += 1; }
                            if (friend.townhallLevel == 10 && a.stars == 2) { tenTwoE += 1; }

                        }
                        if (a.enemyTH == 12) { twelveLeftE -= 1; }
                        if (a.enemyTH == 11) { elevenLeftE -= 1; }
                        if (a.enemyTH == 10) { tenLeftE -= 1; }
                        if (a.enemyTH == 9) { nineLeftE -= 1; }
                        //if (a.enemyTH == 8) { eightLeftE -= 1; }

                        attacksE += 1;
                        if (friend.townhallLevel == 12 && a.enemyTH == 12)
                        {
                            twelveV12TE += 1;
                            if (a.stars == 3) { twelveV12E += 1; }
                        }
                        if (friend.townhallLevel == 11 && a.enemyTH == 12)
                        {//dip
                            twelveV11TE += 1;
                            if (a.stars == 3) { twelveV11E += 1; }
                        }
                        if (friend.townhallLevel == 12 && a.enemyTH == 11)
                        {//hitup
                            elevenV12TE += 1;
                            if (a.stars >= 2) { elevenV12E += 1; }
                        }
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
                        {//hitup
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
                if (elevenV12TE == 0)
                {
                    elevenV12PE = Math.Round((decimal)0, p);
                }
                else
                {
                    elevenV12PE = Math.Round((decimal)elevenV12E / (decimal)elevenV12TE * 100, p);
                }
                if (twelveV11TE == 0)
                {
                    twelveV11PE = Math.Round((decimal)0, p);
                }
                else
                {
                    twelveV11PE = Math.Round((decimal)twelveV11E / (decimal)twelveV11TE * 100, p);
                }
                if (twelveV12TE == 0)
                {
                    twelveV12PE = Math.Round((decimal)0, p);
                }
                else
                {
                    twelveV12PE = Math.Round((decimal)twelveV12E / (decimal)twelveV12TE * 100, p);
                }

                //compute friendly stats
                List<stats> friendAttacks = new List<stats>();
                foreach (Members friend in (war.clans.First(c => c.tag == ClanTag).member ?? new List<Members>()))
                {
                    foreach (Attacks attack in (friend.attack ?? new List<Attacks>()))
                    {
                        stats b = new stats();
                        b.destructionPercentage = attack.destructionPercentage;
                        b.enemyTH = war.clans.First(c => c.tag != ClanTag).member.Where(x => x.tag == attack.defenderTag).FirstOrDefault().townhallLevel; //GetEnemyTh(attack.defenderTag, saved);
                        b.friendlyTH = war.clans.First(c => c.tag == ClanTag).member.Where(x => x.tag == attack.attackerTag).FirstOrDefault().townhallLevel; //GetFriendlyTh(attack.attackerTag, saved);
                        b.stars = attack.stars;
                        b.attackerTag = attack.attackerTag;
                        b.defenderTag = attack.defenderTag;
                        b.mapPosition = war.clans.First(c => c.tag != ClanTag).member.Where(c => c.tag == attack.defenderTag).First().mapPosition;
                        friendAttacks.Add(b);
                    }
                }

                foreach (Members enemy in war.clans.First(c => c.tag != ClanTag).member)
                {
                    bool found = false;
                    foreach (stats a in (friendAttacks ?? new List<stats>()).Where(a => a.defenderTag == enemy.tag).OrderByDescending(a => a.stars).ThenByDescending(a => a.destructionPercentage))
                    {
                        if (!found)
                        {
                            found = true;
                            stars += a.stars;
                            DestructionSum += a.destructionPercentage;
                            if (enemy.townhallLevel == 12 && a.stars == 3) { twelveThree += 1; }
                            if (enemy.townhallLevel == 12 && a.stars == 2) { elevenTwo += 1; }
                            if (enemy.townhallLevel == 11 && a.stars == 3) { elevenThree += 1; }
                            if (enemy.townhallLevel == 11 && a.stars == 2) { elevenTwo += 1; }
                            if (enemy.townhallLevel == 10 && a.stars == 3) { tenThree += 1; }
                            if (enemy.townhallLevel == 10 && a.stars == 2) { tenTwo += 1; }
                        }
                        if (a.friendlyTH == 12) { twelveLeft -= 1; }
                        if (a.friendlyTH == 11) { elevenLeft -= 1; }
                        if (a.friendlyTH == 10) { tenLeft -= 1; }
                        if (a.friendlyTH == 9) { nineLeft -= 1; }
                        //if (a.friendlyTH == 8) { eightLeft -= 1; }

                        attacks += 1;
                        if (enemy.townhallLevel == 12 && a.friendlyTH == 12)
                        {
                            twelveV12T += 1;
                            if (a.stars == 3) { twelveV12 += 1; }
                        }
                        if (enemy.townhallLevel == 11 && a.friendlyTH == 12)
                        {//dip
                            twelveV11T += 1;
                            if (a.stars == 3) { twelveV11 += 1; }
                        }
                        if (enemy.townhallLevel == 12 && a.friendlyTH == 11)
                        {//hitup
                            elevenV12T += 1;
                            if (a.stars >= 2) { elevenV12 += 1; }
                        }
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
                if (elevenV12T == 0)
                {
                    elevenV12P = Math.Round((decimal)0, p);
                }
                else
                {
                    elevenV12P = Math.Round((decimal)elevenV12 / (decimal)elevenV12T * 100, p);
                }
                if (twelveV11T == 0)
                {
                    twelveV11P = Math.Round((decimal)0, p);
                }
                else
                {
                    twelveV11P = Math.Round((decimal)twelveV11 / (decimal)twelveV11T * 100, p);
                }
                if (twelveV12T == 0)
                {
                    twelveV12P = Math.Round((decimal)0, p);
                }
                else
                {
                    twelveV12P = Math.Round((decimal)twelveV12 / (decimal)twelveV12T * 100, p);
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

        public string ClanTag { get; private set; }
        public string WarId { get; set; }
        public int OrderOfLastAttack { get; set; }
        public bool DownloadedFromSC { get; set; }
        public bool err { get; set; }

        public int twelveLeft { get; set; }
        public int elevenLeft { get; set; }
        public int tenLeft { get; set; }
        public int nineLeft { get; set; }
        public int eightLeft { get; set; }

        public int twelveLeftE { get; set; }
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
        public int twelves { get; set; }
        public int twelvesE { get; set; }


        public int twelveThree { get; set; }
        public int twelveThreeE { get; set; }
        public int twelveTwo { get; set; }
        public int twelveTwoE { get; set; }

        public int elevenThree { get; set; }
        public int elevenThreeE { get; set; }
        public int elevenTwo { get; set; }
        public int elevenTwoE { get; set; }

        public int tenThree { get; set; }
        public int tenThreeE { get; set; }
        public int tenTwo { get; set; }
        public int tenTwoE { get; set; }

        public int twelveV12 { get; set; }
        public int twelveV12T { get; set; }
        public decimal twelveV12P { get; set; }
        public int twelveV12E { get; set; }
        public int twelveV12TE { get; set; }
        public decimal twelveV12PE { get; set; }

        public int twelveV11 { get; set; }
        public int twelveV11T { get; set; }
        public decimal twelveV11P { get; set; }
        public int twelveV11E { get; set; }
        public int twelveV11TE { get; set; }
        public decimal twelveV11PE { get; set; }

        public int elevenV12 { get; set; }
        public int elevenV12T { get; set; }
        public decimal elevenV12P { get; set; }
        public int elevenV12E { get; set; }
        public int elevenV12TE { get; set; }
        public decimal elevenV12PE { get; set; }

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
}
