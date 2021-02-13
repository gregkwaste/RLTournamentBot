using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace TournamentBackend
{

    [Serializable]
    public class TeamInvitation
    {
        [BsonId]
        public long UID { get; set; }
        public long teamID { get; set; }
        [BsonIgnore]
        public Team team { get; set; }
        public long playerID { get; set; }
        [BsonIgnore]
        public Player player { get; set; }
        public long tournamentID { get; set; }
        public bool pending { get; set; } = true;
        public bool accepted { get; set; } = false;
    }


    [Serializable]
    public class MainPlayer
    {
        [BsonId]
        public long ID { get; set; }

        [BsonField]
        public ulong DiscordID { get; set; }

        [BsonField]
        public long ActiveTournamentID { get; set; }

    }

    public class TeamPlayerAssoc
    {
        [BsonId]
        public long UID { get; set; }
        public long PlayerID { get; set; }
        public long TeamID { get; set; }
        public long TournamentID { get; set; }
    }


    [Serializable]
    public class Player
    {
        [BsonId]
        public long UID { get; set; }

        public int ID { get; set; } //Local Tournament Player ID

        public long MainPlayerID { get; set; } //DB Main Player ID

        [BsonIgnore]
        public MainPlayer mainPlayer; //Ref to main player ID

        public long TournamentID { get; set; } //Associated Tournament ID

        public string Name { get; set; }

        public int RankID { get; set; }

        [BsonIgnore]
        public Rank Rank {
            get
            {
                return  Common.getRankFromID(RankID);
            }
        }

        [BsonIgnore]
        public Team team { get; set; }

        [BsonIgnore]
        public List<TeamInvitation> Invitations = new List<TeamInvitation>();


        public Player()
        {
            
        }
        
        public Player(string name, int rank_id)
        {
            Name = name;
            RankID = rank_id;
        }
        
        public void setRankFromText(string rank_text)
        {
            //TODO: CHeck if parse failed
            RankID = Common.getRankIDFromText(rank_text);
        }

        public void acceptInvitation(TeamInvitation inv)
        {
            if (inv.team is Team2s)
            {
                (inv.team as Team2s).addPlayer(this);
                Invitations.Remove(inv); //Simply remove invitation
            } else if (inv.team is Team3s)
            {
                (inv.team as Team3s).addPlayer(this);
                Invitations.Remove(inv); //Simply remove invitation
            }
            
            
        }

        public void rejectInvitation(TeamInvitation inv)
        {
            Invitations.Remove(inv); //Simply remove invitation
        }

    }

    
    public class LobbyInfo
    {
        [BsonField]
        public string Name { get; set; }
        [BsonField]
        public string Pass { get; set; }
    }

    
    
}
