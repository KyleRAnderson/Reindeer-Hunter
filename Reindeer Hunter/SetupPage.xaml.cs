using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for SetupPage.xaml
    /// </summary>
    public partial class SetupPage : System.Windows.Controls.UserControl
    {
        protected static StartupWindow masterWindow;

        public SetupPage(StartupWindow mainWindow)
        {
            masterWindow = mainWindow;
            InitializeComponent();
        }

        private void Import_button_Click(object sender, RoutedEventArgs e)
        {
            bool success = masterWindow.ImportStudents();
            if (!success) return;
            masterWindow.GoToHome();
        }

        private void Open_User_manual_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DataFileIO.ManualLoc)) Process.Start(DataFileIO.ManualLoc);
        }
    }
}
