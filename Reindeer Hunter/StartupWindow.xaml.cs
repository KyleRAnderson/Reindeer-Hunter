using System.Windows;
using Reindeer_Hunter.FFA;
using Microsoft.Win32;
using Reindeer_Hunter.ThreadMonitors;
using System.Diagnostics;
using Reindeer_Hunter.Hunt;
using System.IO;

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

        #region Application Version information
        private const string NOTE_SEPARATOR = " - ";
        public const int MAJOR_VERSION = 2, MINOR_VERSION = 0, BUILD_VERSION = 101;
        public const string NOTE_VERSION = "";
        public static readonly string APPLICATION_VERSION = string.Format("{0}.{1}.{2}{3}{4}", MAJOR_VERSION, MINOR_VERSION, BUILD_VERSION, NOTE_SEPARATOR, NOTE_VERSION);
        #endregion

        private HomePage home;
        public HomePage Home
        {
            get
            {
                if (home == null) home = new HomePage(this);
                return home;
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
            Title = string.Format("{0} {1}", Title, APPLICATION_VERSION);

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
                InitialDirectory = DataFileIO.InitialDirectory,

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
                if (string.IsNullOrEmpty(path)) return;
            }

            ImportHandler importer = new ImportHandler(_School, csvopenDialog.FileNames, GoToHome);

            DataFileIO.LastOpenedDirectory = Path.GetDirectoryName(csvopenDialog.FileNames[0]);
        }

        /// <summary>
        /// Gets the build information for the given string containing the total application version.
        /// </summary>
        /// <param name="fromString">The application version total string.</param>
        /// <returns>A string array of the build information such that index 0 is MAJOR_VERSION, 1 is MINOR_VERSION,
        /// 2 is BUILD_VERSION and 3 is NOTE_VERSION</returns>
        public static string[] ParseBuildInformation(string fromString)
        {
            string note = string.Empty;
            if (fromString.Contains(NOTE_SEPARATOR))
            {
                int index = fromString.LastIndexOf(NOTE_SEPARATOR) + NOTE_SEPARATOR.Length;
                note = fromString.Substring(index, fromString.Length - index);
                fromString = fromString.Substring(0, fromString.Length + 1 - (index - NOTE_SEPARATOR.Length));
            }

            string[] versions = fromString.Split('.');
            string[] returnable = {
                versions[0],
                versions[1],
                versions[2],
                note
            };

            return versions;
        }
    }
}
