using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Data_Classes
{
    /// <summary>
    /// Class that represents each student.
    /// </summary>
    public class Student
    {
        public string First { get; set; }

        public string Last { get; set; }

        public int Id { get; set; }

        public int Grade { get; set; }

        public int Homeroom { get;  set; }

        private string matchId;


        // The last round the student participated in
        public long LastRoundParticipated { get; set; }

        public List<string> MatchesParticipated { get; set; }

        public string CurrMatchID
        {
            get
            {
                return matchId;
            }
            set
            {
                // Retain the old value for history's sake
                if (!MatchesParticipated.Contains(value))
                    MatchesParticipated.Add(value);
                matchId = value;
            }
        }

        // Indicates whether or not the student is in the hunt still
        // True until not true.
        public bool In { get; set; }

        /// <summary>
        /// Returns the name of the student as a tuple
        /// </summary>
        /// <returns>The student's (FIRSTNAME, LASTNAME)</returns>
        public Tuple<string, string> GetName()
        {
            return new Tuple<string, string>(First, Last);
        }
    }
}
