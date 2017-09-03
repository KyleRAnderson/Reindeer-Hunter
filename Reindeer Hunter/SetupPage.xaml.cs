using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            masterWindow.SetPageToHome();
        }
    }
}
