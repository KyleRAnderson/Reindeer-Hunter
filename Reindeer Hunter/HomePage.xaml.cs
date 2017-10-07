using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : System.Windows.Controls.UserControl
    {

        // The thread used to create the matches.
        protected static System.Threading.Thread printing_thread;

        // The master window upon which this content will be displayed.
        public StartupWindow MasterWindow;


        public HomePage(StartupWindow mainWindow)
        {
            // Create necessary variables.
            MasterWindow = mainWindow;

            InitializeComponent();

            // Disable importing students if we are past round 0
            Import_Students_Button.IsEnabled = MasterWindow._School.GetCurrRoundNo() == 0;

            // Send this object's data to the command manager
            ((CommandManager)DataContext).SetHomePage(this);
        }

        protected virtual void OnMatchChangeEvent(object source, EventArgs e)
        {
            // Disable importing students if we are past round 0
            Import_Students_Button.IsEnabled = MasterWindow._School.GetCurrRoundNo() == 0;
        }

        private void Search_Box_GotFocus(object sender, RoutedEventArgs e)
        {
            search_box.Clear();
        }

        private void ComboBox_ImportStudentButton (object semder, EventArgs e)
        {
            MasterWindow.ImportStudents();
        }

        private void Import_Match_ResultsButton_Click(object sender, RoutedEventArgs e)
        {
            List<ResultStudent> results = new List<ResultStudent>();
            object[] inputtedResults;
            try
            {
                inputtedResults = MasterWindow.ImporterSystem.Import(1).ElementAt<object[]>(0);
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

            MasterWindow._School.AddMatchResults(results);
        }

        /// <summary>
        /// Function called whenever the help menu button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpMenuButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/eAUE/Reindeer-Hunter/wiki");
        }
    }
}