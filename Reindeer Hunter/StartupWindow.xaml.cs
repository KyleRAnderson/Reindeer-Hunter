using System.Windows;
using Reindeer_Hunter.FFA;
using System;
using Microsoft.Win32;
using Reindeer_Hunter.ThreadMonitors;
using System.Diagnostics;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for SetupScreen.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        /// <summary>
        /// Whether or not the application should be in dev mode.
        /// </summary>
        public static bool IsDevMode
        {
            get
            {
                return Debugger.IsAttached;
            }
        }

        public School _School { get; set; }
        private HomePage home;
        public HomePage Home
        {
            get
            {
                if (home == null) home = new HomePage(this);
                return home;
            }
        }

        /// <summary>
        /// The current application version number.
        /// </summary>
        public static string ApplicationVersionNumber
        {
            get
            {
                return "1.2.51";
            }
        }

        private FreeForAll _ffaPage;
        public FreeForAll FFAPage
        {
            get
            {
                if (_ffaPage == null) _ffaPage = new FreeForAll(_School);
                return _ffaPage;
            }
        }

        public StartupWindow()
        {
            InitializeComponent();

            _School = new School();

            // Add the version to the title.
            Title = Title + " " + ApplicationVersionNumber;

            if (_School.IsData() && !_School.IsFFARound)
            {
                GoToHome();
            }

            // Basically, if it is the FFA round.
            else if (_School.IsData() && _School.IsFFARound)
            {
                GoToFFA();
            }
            else
            {
                SetPage(new SetupPage(this));
            }
        }

        /// <summary>
        /// Sets the page to the main home page.
        /// </summary>
        public void GoToHome()
        {
            SetPage(Home);
        }

        public void GoToFFA()
        {
            SetPage(FFAPage);
        }

        public void SetPage(System.Windows.Controls.UserControl page)
        {
            // Only do this if the current page isn't already set to that page.
            if (!Content.Equals(page))
                Content = page;
        }

        /// <summary>
        /// Prompts the user to import students from .csv file(s)
        /// </summary>
        /// <returns>A true or false value, true if the operation succeeded, false otherwise.</returns>
        public void ImportStudents()
        {

            OpenFileDialog csvopenDialog = new OpenFileDialog
            {

                // Open the file dialog to the user's directory
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                // Filter only for comma-seperated value files. 
                Filter = "csv files (*.csv)|*.csv",
                FilterIndex = 2,
                RestoreDirectory = true,
                Multiselect = true
            };

            csvopenDialog.ShowDialog();

            // If use selects nothing, return
            if (csvopenDialog.FileNames.Length == 0) return;
            foreach (string path in csvopenDialog.FileNames)
            {
                if (String.IsNullOrEmpty(path)) return;
            }

            ImportHandler importer = new ImportHandler(_School, csvopenDialog.FileNames, GoToHome);

            return;
        }
    }
}
