using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace TournamentBackend
{
    [Serializable]
    public abstract class Team
    {
        [BsonId]
        public long UID { get; set; } //Database row
        [BsonIgnore]
        public int ID { get; set; }
        [BsonIgnore]
        public bool IsDummy { get; set; } = false;
        private string _name = "";
        [BsonField]
        public long TournamentID { get; set; }
        [BsonField]
        public string Name
        {
            get
            {
                if (_name == "")
                    return "Team " + ID.ToString();
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [BsonField]
        public long CaptainID { get; set; }


        private Player _captain;

        [BsonIgnore]
        public Player Captain {

            get
            {
                return _captain;
            }

            set
            {
                if (value != null)
                {
                    _captain = value;
                    CaptainID = _captain.UID;
                }
            }
        }

        [BsonIgnore]
        public virtual List<Player> Players
        {
            get { return new List<Player>() { }; }
        }

        [BsonIgnore]
        public string Repr
        {
            get
            {
                List<string> Names = new List<string>();
                foreach (Player p in Players)
                    Names.Add(p.Name);
                return string.Join("\n", Names);
            }
        }
    }

    [Serializable]
    public class Team1s : Team
    {

        [BsonIgnore]
        public long Player1ID { get; set; }
        [BsonIgnore]
        public Player Player1 { get; set; }

        [BsonIgnore]
        public override List<Player> Players
        {
            get { return new List<Player>() { Player1 }; }
        }

        public bool addPlayer(Player p)
        {
            if (Player1 == null)
            {
                Player1 = p;
                Captain = p;
                p.team = this;
                return true;
            }
            return false;
        }

    }

    [Serializable]
    public class Team2s : Team
    {
        [BsonIgnore]
        public Player Player1 { get; set; }
        [BsonIgnore]
        public Player Player2 { get; set; }

        public override List<Player> Players
        {
            get { return new List<Player>() { Player1, Player2 }; }
        }

        public bool addPlayer(Player p)
        {
            if (Player1 == null)
            {
                Player1 = p;
                Captain = p;
                p.team = this;
                return true;
            }
            else if (Player2 == null)
            {
                Player2 = p;
                p.team = this;
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class Team3s : Team
    {
        [BsonIgnore]
        public Player Player1 { get; set; }
        [BsonIgnore]
        public Player Player2 { get; set; }
        [BsonIgnore]
        public Player Player3 { get; set; }

        [BsonIgnore]
        public override List<Player> Players
        {
            get { return new List<Player>() { Player1, Player2, Player3 }; }
        }

        public bool addPlayer(Player p)
        {
            if (Player1 == null)
            {
                Player1 = p;
                Captain = p;
                p.team = this;
                return true;
            }
            else if (Player2 == null)
            {
                Player2 = p;
                p.team = this;
                return true;
            }
            else if (Player3 == null)
            {
                Player3 = p;
                p.team = this;
                return true;
            }
            return false;
        }
    }

}
