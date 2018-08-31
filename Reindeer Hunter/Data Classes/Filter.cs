using System;
using System.Collections.Generic;

namespace Reindeer_Hunter.Data_Classes
{
    public class Filter
    {
        public List<Tuple<long, bool>> Round { get; set; }
        public bool Open { get; set; }
        public bool Closed { get; set; }

        public List<long> SelectedRounds
        {
            get
            {
                List<long> rounds = new List<long>();

                foreach (Tuple<long, bool> round in Round)
                {
                    if (round.Item2) rounds.Add(round.Item1);
                }

                return rounds;
            }
        }
    }
}
