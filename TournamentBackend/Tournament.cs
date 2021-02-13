using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using LiteDB;

namespace TournamentBackend
{
    public enum TournamentType
    {
        SOLO = 0x1,
        DOUBLES,
        TRIPLES
    }

    public enum TournamentTeamGenMethod
    {
        RANDOM = 0x1,
        REGISTER
    }

    [Serializable]
    public class TournamentInfo
    {
        [BsonId]
        public long UID { get; set; }
        public long GuildID { get; set; } //Guild DB ID
        public int TypeID { get; set; }

        public string Name { get; set; }

        [BsonIgnore]
        public TournamentType Type
        {
            get
            {
                return (TournamentType) TypeID;
            }
        }
        public int TeamGenMethodID { get; set; }
        [BsonIgnore]
        public TournamentTeamGenMethod TeamGenMethod
        {
            get
            {
                return (TournamentTeamGenMethod) TeamGenMethodID;
            }
        }

        //Channel Properties
        public ulong RegistationChannelID { get; set; } = 0;
        public ulong ManagementChannelID { get; set; } = 0;
        public ulong ScoreReportChannelID { get; set; } = 0;
        public ulong AnnouncementChannelID { get; set; } = 0;
        public ulong CategoryChannelID { get; set; } = 0;
        public ulong RoleID { get; set; } = 0;

        public bool RegistrationsEnabled { get; set; } = false;
        public bool IsStarted { get; set; } = false;
        public bool IsFinished { get; set; } = false;
        public bool BracketGenerated { get; set; } = false;
        public bool TeamsGenerated { get; set; } = false;
        public int RoundNumber { get; set; } = 0;
    }

    [Serializable]
    public class Tournament
    {
        public TournamentInfo info = new TournamentInfo();
        public List<Player> _players = new List<Player>();
        public List<Team> _teams = new List<Team>();
        private Bracket _bracket;
        public Bracket bracket
        {
            get
            {
                return _bracket;
            }
        }

        public ObservableCollection<Player> Players
        {
            get
            {
                return new ObservableCollection<Player>(_players);
            }
        }

        public ObservableCollection<Team> Teams
        {
            get
            {
                return new ObservableCollection<Team>(_teams);
            }
        }

        public void ClearPlayers()
        {
            _players.Clear();
        }

        public void ClearTeams()
        {
            _teams.Clear();
        }

        public Player getPlayer(string username)
        {
            foreach (Player p in _players)
            {
                if (p.Name == username)
                    return p;
            }
            return null;
        }

        public Player getPlayerbyDiscordID(ulong id)
        {
            foreach (Player p in _players)
            {
                if (p.mainPlayer.DiscordID == id)
                    return p;
            }
            return null;
        }

        public Player getPlayerByID(long id)
        {
            foreach (Player p in _players)
            {
                if (p.ID == (int)id)
                    return p;
            }
            return null;
        }

        public Player getPlayerByMainPlayerUID(long uid)
        {
            foreach (Player p in _players)
            {
                if (p.MainPlayerID == uid)
                    return p;
            }
            return null;
        }

        public Player getPlayerByUID(long uid)
        {
            foreach (Player p in _players)
            {
                if (p.UID == uid)
                    return p;
            }
            return null;
        }

        public Team getTeamByID(ulong id)
        {
            foreach (Team t in _teams)
            {
                if (t.ID == (int)id)
                    return t;
            }
            return null;
        }

        public Matchup getmatchupByUID(long uid)
        {
            if (bracket.MatchupTree.UID == uid)
                return bracket.MatchupTree;

            foreach (Round r in bracket.Rounds)
            {
                foreach (Matchup m in r.Matchups)
                {
                    if (m.UID == uid)
                        return m;
                }
            }
            return null;
        }

        public Team getTeamByUID(long uid)
        {
            foreach (Team t in _teams)
            {
                if (t.UID == uid)
                    return t;
            }
            return null;
        }

        public void Clear()
        {
            _bracket = null;
            ClearTeams();
            ClearPlayers();
        }

        public void AddPlayer(Player p)
        {
            p.ID = _players.Count;
            _players.Add(p);
        }

        public void AddRandomPlayers()
        {
            for (int i = 0; i < 32; i++)
            {
                Player p = new Player("test", 1);
                AddPlayer(p);
            }
        }

        public bool CreateTeams()
        {
            bool status = false;
            if (info.Type == TournamentType.SOLO)
                status = CreateTeams1s();
            if (info.Type == TournamentType.DOUBLES)
                status = CreateTeams2s();
            if (info.Type == TournamentType.TRIPLES)
                status = CreateTeams3s();

            if (status)
            {
                info.TeamsGenerated = true;
                return true;
            }
            return false;
        }

        public bool CreateTeams1s()
        {
            //By default we're generating teams for 2s

            List<Player> tempPlayers = new List<Player>();
            foreach (Player p in _players)
                tempPlayers.Add(p);

            _teams.Clear();

            while (tempPlayers.Count > 0)
            {
                Team1s t = new Team1s();
                t.TournamentID = info.UID;
                t.ID = _teams.Count;
                t.Player1 = tempPlayers.First();
                t.Captain = t.Player1;

                //Remove the entires
                tempPlayers.RemoveAt(0);

                _teams.Add(t);
            }

            return true;
        }

        public bool CreateTeams2s()
        {
            //By default we're generating teams for 2s

            List<Player> tempPlayers = new List<Player>();
            foreach (Player p in _players)
                tempPlayers.Add(p);
            tempPlayers.Sort((a, b) => a.Rank._rank.CompareTo(b.Rank._rank));

            _teams.Clear();

            if (tempPlayers.Count % 2 > 0)
            {
                //MessageBox.Show("ZIMA BALE ALLON ENAN XUMA PAIXTH KAI KSANAPATA");
                return false;
            }


            while (tempPlayers.Count > 0)
            {
                Team2s t = new Team2s();
                t.TournamentID = info.UID;
                t.ID = _teams.Count;
                t.Player1 = tempPlayers.First();
                t.Player2 = tempPlayers.Last();
                t.Captain = t.Player2; //Always set captain to player 2

                //Remove the entires
                tempPlayers.RemoveAt(0);
                tempPlayers.RemoveAt(tempPlayers.Count - 1);

                _teams.Add(t);
            }

            return true;
        }

        public bool CreateTeams3s()
        {
            //By default we're generating teams for 2s

            List<Player> tempPlayers = new List<Player>();
            foreach (Player p in _players)
                tempPlayers.Add(p);

            Random rnd = new Random();
            List<Player> randPlayerList = tempPlayers.Select(x => new { value = x, order = rnd.Next() })
            .OrderBy(x => x.order).Select(x => x.value).ToList();


            if (tempPlayers.Count % 3 > 0)
            {
                //MessageBox.Show("ZIMA BALE PAIXTES NA BGAINOUN TRIADES");
                return false;
            }

            _teams.Clear();

            while (tempPlayers.Count > 0)
            {
                Team3s t = new Team3s();
                t.TournamentID = info.UID;
                t.ID = _teams.Count;
                t.Player1 = tempPlayers[0];
                t.Player2 = tempPlayers[1];
                t.Player3 = tempPlayers[2];
                t.Captain = t.Player1; //Always set captain to player 1 (Random anyway)

                //Remove the entries
                tempPlayers.RemoveAt(0);
                tempPlayers.RemoveAt(1);
                tempPlayers.RemoveAt(2);
                _teams.Add(t);
            }

            return true;
        }

        public void ExportBracket()
        {
            _bracket.GenerateSVG();
        }

        public void CreateBracket()
        {
            _bracket = new Bracket(info.UID);
            _bracket.GenerateBracket(_teams.Count);
            _bracket.Populate(_teams.ToList());
            _bracket.Report();
            //Update bracket info
            info.BracketGenerated = true;
            info.RoundNumber = _bracket.Rounds.Count;
        }

        public void RemovePlayer(Player p)
        {
            _players.Remove(p);
        }

        public void ImportPlayersFromCSV(string filename)
        {
            //Parse CSV
            string text = System.IO.File.ReadAllText(filename);
            //Console.WriteLine(text);

            string[] lines = text.Split('\n');

            for (int i = 1; i < lines.Length; i++)
            {
                string l = lines[i];

                //Format is as follows
                //Column 0: Date
                //Column 1: Player Twitch Name
                //Column 2: Player Steam Name
                //Column 3: Rank Text

                string[] _l = l.Trim().Split(',');

                Player p = new Player();
                p.ID = _players.Count;
                p.Name = _l[1];

                //Fix ranktext

                string _rank_curated_name = _l[3];
                _rank_curated_name = _rank_curated_name.Replace("1", "I");
                _rank_curated_name = _rank_curated_name.Replace("2", "II");
                _rank_curated_name = _rank_curated_name.Replace("3", "III");

                p.RankID = Common.getRankIDFromText(_rank_curated_name);
                _players.Add(p);
            }
        }



    }
}
