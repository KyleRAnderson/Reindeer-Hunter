using System;

namespace Reindeer_Hunter.Hunt
{
    public class Match
    {
        public long Round { get; set; }
        public string MatchId { get; set; }
        public long MatchNumber { get; set; }
        public bool Closed { get; set; }

        // Students' homeroom
        public int Home1 { get; set; }
        public int Home2 { get; set; }

        // To be displayed on the MainDisplay DataGrid

        public bool Pass1 { get; set; } = false;
        public string First1 { get; set; }
        public string Last1 { get; set; }
        public int Id1 { get; set; }
        public int Grade1 { get; set; }
        public string First2 { get; set; }
        public string Last2 { get; set; }
        public int Id2 { get; set; }
        public bool Pass2 { get; set; } = false;
        public int Grade2 { get; set; }

        public string FullName1
        {
            get
            {
                return string.Format("{0} {1}", First1, Last1);
            }
        }

        public string FullName2
        {
            get
            {
                return string.Format("{0} {1}", First2, Last2);
            }
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

        /// <summary>
        /// Determines if the given student is the first student in this match
        /// </summary>
        /// <param name="student">The student</param>
        /// <returns>True if the student is the first student in the match, false otherwise.</returns>
        public bool IsStudent1(Student student)
        {
            return student.Id == Id1;
        }

        /// <summary>
        /// Determines if the given student is the second student in this match
        /// </summary>
        /// <param name="student">The student</param>
        /// <returns>True if the student is the second student in the match, false otherwise.</returns>
        public bool IsSudent2(Student student)
        {
            return student.Id == Id2;
        }

        /// <summary>
        /// Creates a match object copy of this match.
        /// </summary>
        /// <returns>A new match object with the same properties as this one.</returns>
        public Match Clone()
        {
            return new Match
            {
                Pass1 = Pass1,
                Pass2 = Pass2,
                First1 = string.Copy(First1),
                First2 = string.Copy(First2),
                Last1 = string.Copy(Last1),
                Last2 = string.Copy(Last2),
                Id1 = Id1,
                Id2 = Id2,
                MatchId = string.Copy(MatchId),
                MatchNumber = MatchNumber,
                Round = Round,
                Closed = Closed,
                Home1 = Home1,
                Home2 = Home2
            };
        }
    }
}
