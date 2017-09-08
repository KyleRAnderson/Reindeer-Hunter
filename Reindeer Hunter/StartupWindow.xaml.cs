using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileHelpers;
using System.Windows.Forms;
using Reindeer_Hunter.Data_Classes;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for SetupScreen.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {

        public School SacredHeart { get; set; }
        public Importer ImporterSystem { get; set; }
        public static HomePage home;

        public StartupWindow()
        {
            InitializeComponent();

            SacredHeart = new School();
            home = new HomePage(this);
            ImporterSystem = new Importer();
            if (SacredHeart.IsData())
            {
                // TODO Figure out how to change screens.
                SetPage(home);
            }

            else
            {
                // TODO switch to setup user control.
                SetPage(new SetupPage(this));
            }
        }

        /// <summary>
        /// Sets the page to the main home page.
        /// </summary>
        public void SetPageToHome()
        {
            SetPage(home);
        }

        public void SetPage(System.Windows.Controls.UserControl page)
        {
            this.Content = page;
        }

        public bool ImportStudents()
        {
            ImportedStudent[] result = (ImportedStudent[])ImporterSystem.Import(0);

            // In case of any import errors.
            if (result == null) return false;

            int grade = result[0].Grade;

            List<Student> students_to_add = new List<Student>();
            long round = SacredHeart.GetCurrRoundNo();

            foreach (ImportedStudent importedStudent in result)
            {
                // Make new student, set the student's round number and add them to the new list
                Student student = new Student
                {
                    First = importedStudent.First.ToUpper(),
                    Last = importedStudent.Last.ToUpper(),
                    Id = importedStudent.Id,
                    Grade = importedStudent.Grade,
                    Homeroom = importedStudent.Homeroom,
                    LastRoundParticipated = round,
                    In = true,
                    MatchesParticipated = new List<string>()
                };
                students_to_add.Add(student);
            }

            if (!SacredHeart.AddStudents(students_to_add)) return false;

            return true;
        }
    }
}
