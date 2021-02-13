using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using LiteDB;

namespace TournamentBackend
{
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
        public Matchup Next
        {
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
