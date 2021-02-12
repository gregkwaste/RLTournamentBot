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

        [BsonIgnore]
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

    
    [Serializable]
    public class Matchup : INotifyPropertyChanged
    {
        [BsonId]
        public long UID { get; set; }
        public int ID { get; set; } //Local Matchup ID
        public string LobbyName { get; set; }
        public string LobbyPass { get; set; }

        public long TournamentID { get; set; }
        
        public long Team1ID { get; set; }

        [BsonIgnore]
        private Team _team1;

        public long Team2ID { get; set; }


        [BsonIgnore]
        private Team _team2;
        
        [BsonIgnore]
        public Team Team1
        {
            get
            {
                return _team1;
            }
            
            set
            {
                _team1 = value;
                if (value != null)
                    Team1ID = _team1.UID;
                NotifyPropertyChanged("Team1");
                NotifyPropertyChanged("IsValid");
                NotifyPropertyChanged("Color");
            }
        }
        [BsonIgnore]
        public Team Team2
        {
            get
            {
                return _team2;
            }

            set
            {
                _team2 = value;
                if (value != null)
                    Team2ID = _team2.UID;
                NotifyPropertyChanged("Team2");
                NotifyPropertyChanged("IsValid");
                NotifyPropertyChanged("Color");
            }
        }

        [BsonIgnore]
        public bool _isFinished = false;

        public bool IsFinished
        {

            get
            {
                return _isFinished;
            }

            set
            {
                _isFinished = value;
            }
        }

        [BsonIgnore]
        public bool _inProgress = false;
        
        public bool InProgress
        {

            get
            {
                return _inProgress;
            }

            set
            {
                _inProgress = value;
            }
        }


        public long WinnerID { get; set; }
        
        [BsonIgnore]
        private Team _winner;
        [BsonIgnore]
        public Team Winner
        {
            get
            {
                return _winner;
            }

            set
            {
                _winner = value;
                if (value != null)
                    WinnerID = _winner.UID;
                _isFinished = (value is null) ? false : true;
                InProgress = false;

                //Progress winner to the next round
                if (Next != null)
                {
                    if (Next.Team1 != null)
                        Next.Team2 = _winner;
                    else
                        Next.Team1 = _winner;
                }
                
                NotifyPropertyChanged("Winner");
                NotifyPropertyChanged("Color");
            }
        }

        
        public long Team1ReportedWinnerID { get; set; }



        [BsonIgnore]
        public Team _team1ReportedWinner;
        [BsonIgnore]
        public Team Team1ReportedWinner
        {
            get
            {
                return _team1ReportedWinner;
            }

            set
            {
                _team1ReportedWinner = value;
                if (_team1ReportedWinner != null)
                    Team1ReportedWinnerID = _team1ReportedWinner.UID;
            }
        }


        [BsonIgnore]
        public Team _team2ReportedWinner;
        public long Team2ReportedWinnerID { get; set; }


        [BsonIgnore]
        public Team Team2ReportedWinner
        {
            get
            {
                return _team2ReportedWinner;
            }

            set
            {
                _team2ReportedWinner = value;
                if (_team2ReportedWinner != null)
                    Team2ReportedWinnerID = _team2ReportedWinner.UID;
            }
        }

        public int RoundID { get; set; }
        
        public long NextMatchupID { get; set; }

        [BsonIgnore]
        private Matchup _next;
        
        [BsonIgnore]
        public Matchup Next {
            get
            {
                return _next;
            }

            set
            {
                _next = value;
                if (value != null)
                    NextMatchupID = _next.UID;
            }
        }

        [BsonIgnore]
        public bool IsDummy
        {
            get
            {
                if (!IsValid)
                    return false;
                if (Team1.IsDummy && Team2.IsDummy)
                    return true;
                else
                    return false;
            }
        }

        [BsonIgnore]
        public bool IsValid
        {
            get
            {
                return (_team1 != null) && (_team2 != null);
            }
        }

        public void ResolveDummyness()
        {
            //Decide early winners
            if (Team1.IsDummy && !Team2.IsDummy)
                Winner = Team2;
            else if (!Team1.IsDummy && Team2.IsDummy)
                Winner = Team1;
            else if (Team1.IsDummy && Team2.IsDummy)
            {
                //Decide a random Winner
                float rand = (new Random()).Next() % 100 / 100.0f;
                Winner = (rand > 0.5f) ? Team1 : Team2;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public void Report()
        {
            Console.WriteLine("\t {0} vs {1} ", Team1?.ID, Team2?.ID);
        }

    }
}
