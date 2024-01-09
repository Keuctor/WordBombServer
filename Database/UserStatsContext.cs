using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordBombServer.Common.Packets.Response;

namespace WordBombServer.Database
{
    public class UserStat
    {
        public uint Id { get; set; }
        public short Day { get; set; }
        public string LastLogin { get; set; }
    }

    public class UserStatsData
    {
        public List<UserStat> UserStats = new List<UserStat>();
    }

    public class DailyBonus
    {
        public short Emerald { get; set; }
        public bool Chest { get; set; }
    }

    public class UserStatsContext : SimpleJsonDBContext<UserStatsData>
    {
        public List<DailyBonus> Bonuses = new List<DailyBonus>();
        public UserStatsContext() : base("userstats.json")
        {
            Bonuses.Add(new DailyBonus() { Emerald = 10, Chest = false }); //day 1
            Bonuses.Add(new DailyBonus() { Emerald = 0, Chest = true });//day 2
            Bonuses.Add(new DailyBonus() { Emerald = 50, Chest = false });//day 3
            Bonuses.Add(new DailyBonus() { Emerald = 20, Chest = false });//day 4
            Bonuses.Add(new DailyBonus() { Emerald = 0, Chest = true });//day 5
            Bonuses.Add(new DailyBonus() { Emerald = 50, Chest = true });//day 6
            Bonuses.Add(new DailyBonus() { Emerald = 100, Chest = true });//day 7
        }

        public bool RemovePlayerStat(uint userId) {

            UserStat stat = null;
            for (int i = 0; i < Data.UserStats.Count; i++)
            {
                if (Data.UserStats[i].Id == userId)
                {
                    stat = Data.UserStats[i];
                }
            }
            if (stat != null) {
                Data.UserStats.Remove(stat);
                return true;
            }
            return false;
        }
        public UserStat GetOrCreatePlayerStat(uint userId)
        {
            for (int i = 0; i < Data.UserStats.Count; i++)
            {
                if (Data.UserStats[i].Id == userId)
                {
                    return Data.UserStats[i];
                }
            }

            var newStat = new UserStat()
            {
                Day = 0,
                Id = userId,
                LastLogin = DateTime.Now.ToString("MM-dd-yy")
            };
            Data.UserStats.Add(newStat);
            return newStat;
        }
    }
}
