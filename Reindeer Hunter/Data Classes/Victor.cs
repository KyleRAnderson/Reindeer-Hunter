using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Data_Classes
{
    /// <summary>
    /// A data class to hold data on students in the FFA round.
    /// </summary>
    public class Victor : Student
    {
        public string FullName
        {
            get
            {
                return String.Format("{0} {1}", First, Last);
            }
        }

        /// <summary>
        /// List of student numbers representing the students that this victor
        /// has pinned.
        /// </summary>
        public List<int> Kills { get; set; }

        /// <summary>
        /// Total number of kills this person has done
        /// </summary>
        public int NumKills { get; set; }

        public string Status
        {
            get
            {
                if (In) return "In";
                else return "Out";
            }
        }

        /// <summary>
        /// A data class to hold data on students in the FFA round.
        /// </summary>
        /// <param name="student">The student to inherit properties from.</param>
        public Victor(Student student)
        {
            // Inherit the values
            First = student.First;
            Last = student.Last;
            LastRoundParticipated = student.LastRoundParticipated;
            Id = student.Id;
            Grade = student.Grade;
            MatchesParticipated = new List<string>(student.MatchesParticipated);
            CurrMatchID = student.CurrMatchID;
            HasBeenPassed = student.HasBeenPassed;
            Homeroom = student.Homeroom;
            Grade = student.Grade;
            In = student.In;

            NumKills = 0;
            Kills = new List<int>();
        }

        /// <summary>
        /// Constructor if they already exist
        /// </summary>
        public Victor()
        {

        }
    }
}
