using System;
using System.Collections.Generic;
using System.Text;

namespace TournamentBackend
{
    [Serializable]
    public class Round
    {
        public List<Matchup> Matchups = new List<Matchup>();
        public int ID { get; set; }
    }
}
