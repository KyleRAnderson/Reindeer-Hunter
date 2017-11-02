using System.Collections.Generic;
using System.Windows;
using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.FFA;
using System.Deployment.Application;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for SetupScreen.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {

        public School _School { get; set; }
        public Importer ImporterSystem { get; set; }
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
            ImporterSystem = new Importer();


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
            this.Content = page;
        }

        /// <summary>
        /// Prompts the user to import students from .csv file(s)
        /// </summary>
        /// <returns>A true or false value, true if the operation succeeded, false otherwise.</returns>
        public bool ImportStudents()
        {
            List<object[]> resultList = ImporterSystem.Import(Importer.IMPORT_STUDENTS);
            List<Student> students_to_add = new List<Student>();
            long round = _School.GetCurrRoundNo();

            // In case of problems.
            if (resultList == null) return false;

            foreach (object[] result in resultList)
            {
                // In case of any import errors.
                if (result == null) return false;

                foreach (ImportedStudent importedStudent in result)
                {
                    // Make new student, set the student's round number and add them to the new list
                    Student student = new Student
                    {
                        First = importedStudent.First,
                        Last = importedStudent.Last,
                        Id = importedStudent.Id,
                        Grade = importedStudent.Grade,
                        Homeroom = importedStudent.Homeroom,
                        LastRoundParticipated = round,
                        In = true,
                        MatchesParticipated = new List<string>()
                    };
                    students_to_add.Add(student);
                }
            }

            if (!_School.AddStudents(students_to_add)) return false;

            return true;
        }
    }
}
