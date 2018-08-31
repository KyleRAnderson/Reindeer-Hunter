using Microsoft.Win32;
using System;
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
        protected StartupWindow masterWindow;

        public SetupPage(StartupWindow mainWindow)
        {
            masterWindow = mainWindow;
            InitializeComponent();
        }

        private void Import_button_Click(object sender, RoutedEventArgs e)
        {
            masterWindow.ImportStudents();
        }

        /// <summary>
        /// Opens the user manual with the click of the button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_User_manual_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DataFileIO.ManualLoc)) Process.Start(DataFileIO.ManualLoc);
        }

        /// <summary>
        /// Function to import exported reindeer hunter data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Import_Button_Click_1(object sender, RoutedEventArgs e)
        {

            OpenFileDialog askLoc = new OpenFileDialog
            {
                // Open the file dialog to the user's directory
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                // Filter only for comma-seperated value files. 
                Filter = "json files (*.json)|*.json",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            askLoc.ShowDialog();

            string openLoc = askLoc.FileName;

            // In case the user cancels
            if (openLoc == null || openLoc == "") return;

            masterWindow._School.DataFile.Import(openLoc);
        }
    }
}
