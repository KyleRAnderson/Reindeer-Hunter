using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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

            // Send this object's data to the command manager
            ((CommandManager)DataContext).SetHomePage(this);

            // Focus to this user control
            Focusable = true;
            Focus();
        }


        private void ComboBox_ImportStudentButton (object semder, EventArgs e)
        {
            MasterWindow.ImportStudents();
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