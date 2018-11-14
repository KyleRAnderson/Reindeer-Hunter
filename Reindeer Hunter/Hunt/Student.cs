using System;
using System.Collections.Generic;

namespace Reindeer_Hunter.Hunt
{
    /// <summary>
    /// Class that represents each student.
    /// </summary>
    public class Student
    {
        public string First { get; set; }

        public string Last { get; set; }

        public string Id { get; set; }

        public int Grade { get; set; }

        public int Homeroom { get;  set; }

        private string matchId;


        // The last round the student participated in
        public long LastRoundParticipated { get; set; }

        public List<string> MatchesParticipated { get; set; } = new List<string>();

        public string CurrMatchID
        {
            get
            {
                return matchId;
            }
            set
            {
                // Retain the old value for history's sake
                if (!MatchesParticipated.Contains(value) && value != null)
                    MatchesParticipated.Add(value);
                matchId = value;
            }
        }

        // Indicates whether or not the student is in the hunt still
        // True until not true.
        public bool In { get; set; }

        public Student Clone()
        {
            return new Student
            {
                First = First,
                Last = Last,
                LastRoundParticipated = LastRoundParticipated,
                MatchesParticipated = MatchesParticipated,
                CurrMatchID = CurrMatchID,
                Homeroom = Homeroom,
                Grade = Grade,
                Id = Id,
                In = In
            };
        }

        /// <summary>
        /// If the student is passed through a round, this is set to true; 
        /// </summary>
        public bool HasBeenPassed { get; set; } = false;
        
        public string Status
        {
            get
            {
                if (In) return "In";
                else return "Out";
            }
        }

        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", First, Last);
            }
        }
    }
}
