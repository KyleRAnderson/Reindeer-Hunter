using System;
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
    }
}