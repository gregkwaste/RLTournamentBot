using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MySqlConnector;

namespace TournamentBackend
{
    public class BotConfig
    {
        public char Prefix { get; set; }
    }

    public class TournamentChannelManager
    {
        public ulong RegistationChannelID { get; set; }
        public ulong ManagementChannelID { get; set; }
        public ulong ScoreReportChannelID { get; set; }
        public ulong AnnouncementChannelID { get; set; }
        public ulong CategoryChannelID { get; set; }
        public ulong RoleID { get; set; }
    }


    public enum RL_RANK
    {
        UNRANKED = 0x0,
        BRONZE_I,
        BRONZE_II,
        BRONZE_III,
        SILVER_I,
        SILVER_II,
        SILVER_III,
        GOLD_I,
        GOLD_II,
        GOLD_III,
        PLATINUM_I,
        PLATINUM_II,
        PLATINUM_III,
        DIAMOND_I,
        DIAMOND_II,
        DIAMOND_III,
        CHAMPION_I,
        CHAMPION_II,
        CHAMPION_III,
        GRAND_CHAMPION_I,
        GRAND_CHAMPION_II,
        GRAND_CHAMPION_III,
        SUPERSONIC_LEGEND,
        NONE
    }

    //Delegates
    public delegate void Log(string msg);

    public static class Common
    {
        //Server Stuff 
        public static BotLiteDBInterface dbInterface;
        public static string dbConnectionString = "test.db";
        public static Dictionary<string, Rank> RankStringMap = new Dictionary<string, Rank>();
        public static Dictionary<RL_RANK, Rank> RankEnumMap = new Dictionary<RL_RANK, Rank>();
        public static List<string> RankNames = new List<string>();
        public static Log loggerFunc;
        
        public static void Populate()
        {
            //Clear
            RankNames.Clear();
            RankStringMap.Clear();
            RankEnumMap.Clear();
            
            RankNames.Add("Bronze I");
            RankNames.Add("Bronze II");
            RankNames.Add("Bronze III");
            RankNames.Add("Silver I");
            RankNames.Add("Silver II");
            RankNames.Add("Silver III");
            RankNames.Add("Gold I");
            RankNames.Add("Gold II");
            RankNames.Add("Gold III");
            RankNames.Add("Platinum I");
            RankNames.Add("Platinum II");
            RankNames.Add("Platinum III");
            RankNames.Add("Diamond I");
            RankNames.Add("Diamond II");
            RankNames.Add("Diamond III");
            RankNames.Add("Champion I");
            RankNames.Add("Champion II");
            RankNames.Add("Champion III");
            RankNames.Add("Grand Champion I");
            RankNames.Add("Grand Champion II");
            RankNames.Add("Grand Champion III");
            RankNames.Add("SuperSonic Legend");

            foreach (string r in RankNames)
            {
                RankStringMap[r] = new Rank(r);
                RankEnumMap[RankStringMap[r]._rank] = RankStringMap[r];
            }
        }

        public static int getRankIDFromText(string rank_text)
        {
            return RankNames.IndexOf(rank_text);
        }
        
        public static Rank getRankFromID(int rank_id)
        {
            return RankEnumMap[(RL_RANK) rank_id];
        }

        public static bool rankExists(string rank_text)
        {
            if (RankStringMap.Keys.Contains(rank_text))
                return true;
            return false;
        }
        
        public static bool rankExists(int rank_id)
        {
            if (RankEnumMap.Keys.Contains((RL_RANK) rank_id))
                return true;
            return false;
        }
        
        public static string emoteMap(Settings settings, RL_RANK rank)
        {
            switch (rank)
            {
                case RL_RANK.BRONZE_I:
                    return settings.emoteSettings.RL_RANK_BRONZE_I;
                case RL_RANK.BRONZE_II:
                    return settings.emoteSettings.RL_RANK_BRONZE_II;
                case RL_RANK.BRONZE_III:
                    return settings.emoteSettings.RL_RANK_BRONZE_III;
                case RL_RANK.SILVER_I:
                    return settings.emoteSettings.RL_RANK_SILVER_I;
                case RL_RANK.SILVER_II:
                    return settings.emoteSettings.RL_RANK_SILVER_II;
                case RL_RANK.SILVER_III:
                    return settings.emoteSettings.RL_RANK_SILVER_III;
                case RL_RANK.GOLD_I:
                    return settings.emoteSettings.RL_RANK_GOLD_I;
                case RL_RANK.GOLD_II:
                    return settings.emoteSettings.RL_RANK_GOLD_II;
                case RL_RANK.GOLD_III:
                    return settings.emoteSettings.RL_RANK_GOLD_III;
                case RL_RANK.PLATINUM_I:
                    return settings.emoteSettings.RL_RANK_PLATINUM_I;
                case RL_RANK.PLATINUM_II:
                    return settings.emoteSettings.RL_RANK_PLATINUM_II;
                case RL_RANK.PLATINUM_III:
                    return settings.emoteSettings.RL_RANK_PLATINUM_III;
                case RL_RANK.DIAMOND_I:
                    return settings.emoteSettings.RL_RANK_DIAMOND_I;
                case RL_RANK.DIAMOND_II:
                    return settings.emoteSettings.RL_RANK_DIAMOND_II;
                case RL_RANK.DIAMOND_III:
                    return settings.emoteSettings.RL_RANK_DIAMOND_III;
                case RL_RANK.CHAMPION_I:
                    return settings.emoteSettings.RL_RANK_CHAMPION_I;
                case RL_RANK.CHAMPION_II:
                    return settings.emoteSettings.RL_RANK_CHAMPION_II;
                case RL_RANK.CHAMPION_III:
                    return settings.emoteSettings.RL_RANK_CHAMPION_III;
                case RL_RANK.GRAND_CHAMPION_I:
                    return settings.emoteSettings.RL_RANK_GRAND_CHAMPION_I;
                case RL_RANK.GRAND_CHAMPION_II:
                    return settings.emoteSettings.RL_RANK_GRAND_CHAMPION_II;
                case RL_RANK.GRAND_CHAMPION_III:
                    return settings.emoteSettings.RL_RANK_GRAND_CHAMPION_III;
                case RL_RANK.SUPERSONIC_LEGEND:
                    return settings.emoteSettings.RL_RANK_SUPER_SONIC_LEGEND;
            }
            return "";
        }

    }
}
