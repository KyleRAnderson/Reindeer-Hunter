using Reindeer_Hunter.Hunt;

namespace Reindeer_Hunter.Data_Classes
{
    /// <summary>
    /// Class for keeping track of a student passing in a match.
    /// </summary>
    public class PassingStudent
    {
        public Match AffectedMatch { get; set; }
        public Student AffectedStudent { get; set; }
    }
}
