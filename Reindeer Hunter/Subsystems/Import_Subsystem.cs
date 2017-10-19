using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Subsystems
{
    /// <summary>
    /// Subsystem in charge of importing students and match results.
    /// </summary>
    public class Import_Subsystem : Subsystem
    {
        public RelayCommand ImportStudents { get; } = new RelayCommand();
        public RelayCommand ImportMatchResults { get; } = new RelayCommand();

        private Importer _Importer;
        private StartupWindow MasterWindow;

        protected override void OnHomePageSet(object sender, EventArgs e)
        {
            base.OnHomePageSet(sender, e);

            // Set functions for the Relay Commands
            ImportStudents.FunctionToExecute = Import_Students;
            ImportStudents.CanExecuteDeterminer = Can_Import_Students;
            ImportMatchResults.FunctionToExecute = Import_Results;
            ImportMatchResults.CanExecuteDeterminer = Can_Import_Results;

            MasterWindow = Manager.Home.MasterWindow;
            _Importer = MasterWindow.ImporterSystem;

            Refresh();

            // Subscribe to events that will merit refresh
            _School.MatchChangeEvent += Refresh;
            _School.RoundIncreased += Refresh;
        }

        /// <summary>
        /// Determines if we can import students still
        /// </summary>
        /// <param name="parameter"></param>
        public bool Can_Import_Students()
        {
            // You can only import during round 0.
            if (_School == null ||_School.GetCurrRoundNo() > 0 || _Importer == null) return false;
            else return true;
        }

        /// <summary>
        /// Function that the realay command calls to execute student import
        /// </summary>
        /// <param name="parameter"></param>
        public void Import_Students(object parameter)
        {
            MasterWindow.ImportStudents();
        }

        public bool Can_Import_Results()
        {
            if (_School.NumOpenMatches <= 0) return false;
            else return true;
        }

        /// <summary>
        /// Function that the relat command calls to import results.
        /// </summary>
        /// <param name="parameter"></param>
        public void Import_Results(object parameter)
        {
            List<ResultStudent> results = new List<ResultStudent>();
            object[] inputtedResults;
            try
            {
                inputtedResults = _Importer.Import(1).ElementAt<object[]>(0);
            }
            catch (System.ArgumentNullException)
            {
                return;
            }

            foreach (ResultStudent student in inputtedResults)
            {
                student.First = student.First.ToUpper();
                student.Last = student.Last.ToUpper();
                results.Add(student);
            }

            _School.AddMatchResults(results);
        }

        /// <summary>
        /// Function that refreshes the commands when needed
        /// </summary>
        private void Refresh(object sender = null, EventArgs e = null)
        {
            ImportStudents.RaiseCanExecuteChanged();
            ImportMatchResults.RaiseCanExecuteChanged();
        }
    }
}
