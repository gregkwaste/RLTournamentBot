using System;
using System.Collections.Generic;
using System.Text;

namespace TournamentBackend
{
    [Serializable]
    public class EmoteSettings
    {
        public string RL_RANK_BRONZE_I;
        public string RL_RANK_BRONZE_II;
        public string RL_RANK_BRONZE_III;
        public string RL_RANK_SILVER_I;
        public string RL_RANK_SILVER_II;
        public string RL_RANK_SILVER_III;
        public string RL_RANK_GOLD_I;
        public string RL_RANK_GOLD_II;
        public string RL_RANK_GOLD_III;
        public string RL_RANK_PLATINUM_I;
        public string RL_RANK_PLATINUM_II;
        public string RL_RANK_PLATINUM_III;
        public string RL_RANK_DIAMOND_I;
        public string RL_RANK_DIAMOND_II;
        public string RL_RANK_DIAMOND_III;
        public string RL_RANK_CHAMPION_I;
        public string RL_RANK_CHAMPION_II;
        public string RL_RANK_CHAMPION_III;
        public string RL_RANK_GRAND_CHAMPION_I;
        public string RL_RANK_GRAND_CHAMPION_II;
        public string RL_RANK_GRAND_CHAMPION_III;
        public string RL_RANK_SUPER_SONIC_LEGEND;
    }

    [Serializable]
    public class TextSettings
    {
        public string desc_TOURNAMENT_START;
        public string thumbnail_URL;
        public string embed_footer;
        public string tournRoleName;
        public char Prefix;
    }

    [Serializable]
    public class Settings
    {
        public EmoteSettings emoteSettings = new EmoteSettings();
        public TextSettings textSettings = new TextSettings();
    }
}