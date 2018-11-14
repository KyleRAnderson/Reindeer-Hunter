namespace Reindeer_Hunter.Data_Classes
{
    /// <summary>
    /// Class for containing decrypted search results
    /// </summary>
    public class SearchQuery
    {
        public string StudentNo { get; set; }
        public string StudentName { get; set; }
        public string MatchId { get; set; }
        public int Homeroom { get; set; }
    }
}
