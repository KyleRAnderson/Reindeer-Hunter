using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter
{
    public class Match
    {
        public long Round { get; set; }
        public string MatchId { get; set; }
        public long MatchNumber { get; set; }
        public bool Closed { get; set; }

        // To be displayed on the MainDisplay DataGrid
        public bool Pass1 { get; set; }
        public string First1 { get; set; }
        public string Last1 { get; set; }
        public int Id1 { get; set; }
        public string First2 { get; set; }
        public string Last2 { get; set; }
        public int Id2 { get; set; }
        public bool Pass2 { get; set; }

        /// <summary>
        /// The class that contains details of all the matches
        /// </summary>
        public Match()
        {
            // Set true when student is passed to next round, false otherwise.
            Pass1 = false;
            Pass2 = false;
        }

        /// <summary>
        /// Generate the match's official number/id
        /// </summary>
        public void GenerateID(long number)
        {
            // Generate the match's official number/id
            MatchId = "MAR" + Round.ToString() + "-" + number.ToString();
            MatchNumber = number;
        }

        public Match Clone()
        {
            return new Match
            {
                Pass1 = Pass1,
                Pass2 = Pass2,
                First1 = First1,
                First2 = First2,
                Last1 = Last1,
                Last2 = Last2,
                Id1 = Id1,
                Id2 = Id2,
                MatchId = MatchId,
                MatchNumber = MatchNumber,
                Round = Round,
                Closed = Closed
            };
        }
    }
}
