using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace TournamentBackend
{
    public class Rank
    {
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public RL_RANK _rank;

        public Rank(string rank_name)
        {
            Name = rank_name;
            _rank = getRankFromText(rank_name);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static RL_RANK getRankFromText(string rank_text)
        {
            //Convert string
            string[] sp = rank_text.ToUpper().Split(' ');
            string rk_str = string.Join("_", sp);

            try
            {
                return (RL_RANK)Enum.Parse(typeof(RL_RANK), rk_str);
            }
            catch
            {
                return RL_RANK.NONE;
            }
        }

    }
}
