using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Reindeer_Hunter.Subsystems.Stats;
using System.ComponentModel;

namespace Reindeer_Hunter.Subsystems
{
    public class Statistics : Subsystem, INotifyPropertyChanged
    {
        private DataGrid StatisticsDisplay;

        public event PropertyChangedEventHandler PropertyChanged;

        // All the functions that the various Statistic objects use to find their values.
        public List<Statistic> DisplayList { get; private set; }

        public string RoundNo()
        {
            return _School.GetCurrRoundNo().ToString();
        }

        public string NumInStudents()
        {
            return _School.GetNumStudentsStillIn().ToString();
        }

        public string NumOpenMatches()
        {
            return _School.NumOpenMatches.ToString();
        }

        public string NumGeneratedMatches()
        {
            return _School.GetNumMatchesGenerated().ToString();
        }

        public string PercentStudentsStillIn()
        {
            int percent = (int)Math.Round((double)(100 * ((double)_School.NumInStudents / (double)_School.TotalNumStudents)));
            return String.Format("{0}%", percent);
        }

        public Statistics() : base()
        {
        }

        /// <summary>
        /// Function called whenever the home page object is set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnHomePageSet(object sender, EventArgs e)
        {
            base.OnHomePageSet(sender, e);

            StatisticsDisplay = Manager.Home.StatisticsDisplay;

            DisplayList = new List<Statistic>
            {
                {new Statistic(RoundNo, "Round")},
                {new Statistic(NumInStudents, "Students Still In")},
                {new Statistic(NumOpenMatches, "Open Matches") },
                {new Statistic(NumGeneratedMatches, "Total Matches Generated") },
                {new Statistic(PercentStudentsStillIn, "Percentage of Students Still in")}
            };

            Refresh();

            // Subscribe to a bunch of events that may need the statistics to refresh
            _School.MatchChangeEvent += Refresh;
            _School.RoundIncreased += Refresh;
            _School.StudentsImported += Refresh;
        }

        /// <summary>
        /// Function to update the statistics and then update the GUI display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Refresh(object sender = null, EventArgs e = null)
        {
            // Refresh the GUI. Statistics will refresh themselves.
            PropertyChanged(this, new PropertyChangedEventArgs("DisplayList"));
            StatisticsDisplay.Items.Refresh();
        }
    }
}
