
using FileHelpers;
using Reindeer_Hunter.Hunt;

namespace Reindeer_Hunter.Data_Classes
{
    /// <summary>
    /// Data class to represent a match which can be exported to file.
    /// </summary>
    [DelimitedRecord(",")]
    public class ExportableMatch
    {
        [FieldOrder(2)]
        public long Round { get; private set; }
        [FieldOrder(1)]
        public string MatchID { get; private set; }
        [FieldOrder(3)]
        public string MatchStatus { get; private set; }
        [FieldOrder(4)]
        public string Student1Name { get; private set; }
        [FieldOrder(8)]
        public string Student2Name { get; private set; }
        [FieldOrder(5)]
        public string Student1Id { get; private set; }
        [FieldOrder(9)]
        public string Student2Id { get; private set; }
        [FieldOrder(7)]
        public int Student1Grade { get; private set; }
        [FieldOrder(11)]
        public int Student2Grade { get; private set; }
        [FieldOrder(6)]
        public int Homeroom1 { get; private set; }
        [FieldOrder(10)]
        public int Homeroom2 { get; private set; }


        /// <summary>
        /// Creates a new ExportableMatch object from a match object.
        /// </summary>
        /// <param name="from">The match from which to make the exportable match.</param>
        /// <returns>A new ExportableMatch object which represents the Match.</returns>
        public static ExportableMatch FromMatch(Match from)
        {
            return new ExportableMatch
            {
                Round = from.Round,
                MatchID = from.MatchId,
                MatchStatus = (from.Closed) ? "Closed" : "Open",
                Student1Name = from.FullName1,
                Student2Name = from.FullName2,
                Student1Id = from.Id1,
                Student2Id = from.Id2,
                Student1Grade = from.Grade1,
                Student2Grade = from.Grade2,
                Homeroom1 = from.Home1,
                Homeroom2 = from.Home2
            };
        }
    }
}
