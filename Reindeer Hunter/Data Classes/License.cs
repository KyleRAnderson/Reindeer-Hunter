using System;

namespace Reindeer_Hunter.Data_Classes
{
    public class License
    {
        private string First1 { get; set; } = "";
        private string Last1 { get; set; } = "";
        private string First2 { get; set; } = "";
        private string Last2 { get; set; } = "";
        public int Homeroom1 { get; set; } = 0;
        public int Homeroom2 { get; set; } = 0;
        public string Date { get; set; } = "";
        public long Round { get; set; } = 0;

        public int Grade
        {
            get
            {
                return (int)Math.Floor((double)Homeroom1 / 100);
            }
        }

        public string Student1Field
        {
            get
            {
                return string.Format("{0} {1} ({2})", First1, Last1, Homeroom1);
            }
        }

        public string Student2Field
        {
            get
            {
                return string.Format("{0} {1} ({2})", First2, Last2, Homeroom2);
            }
        }

        public static License[] CreateFromMatch(Match match, string date)
        {
            int numLicensesTomake;
            string first2;
            string last2;

            if (School.IsPassMatch(match))
            {
                numLicensesTomake = 1;
                first2 = "Move";
                last2 = "on";
            }
            else
            {
                numLicensesTomake = 2;
                first2 = match.First2;
                last2 = match.Last2;
            }

            License[] returnable = new License[numLicensesTomake];

            returnable.SetValue(new License
            {
                First1 = match.First1,
                First2 = first2,
                Last1 = match.Last1,
                Last2 = last2,
                Homeroom1 = match.Home1,
                Homeroom2 = match.Home2,
                Round = match.Round,
                Date = date
            }, 0);

            // Don't do student 2 if it is a pass match, since student2 is the passer.
            if (numLicensesTomake == 2)
            {
                returnable.SetValue(new License
                {
                    First1 = match.First2,
                    First2 = match.First1,
                    Last1 = match.Last2,
                    Last2 = match.Last1,
                    Homeroom1 = match.Home2,
                    Homeroom2 = match.Home1,
                    Round = match.Round,
                    Date = date
                }, 1);
            }

            return returnable;
        }
    }
}
