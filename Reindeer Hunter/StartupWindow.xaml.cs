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

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for SetupScreen.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public static School school;
        public static Importer importer;
        public static HomePage home;

        public StartupWindow()
        {
            InitializeComponent();
            school = new School();
            home = new HomePage(this);
            importer = new Importer();
            if (school.IsData())
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
            Student[] result = (Student[])importer.Import(0);

            // In case of any import errors.
            if (result == null) return false;

            int grade = result[0].Grade;

            List<Student> students_to_add = new List<Student>();
            long round = school.GetCurrRoundNo();

            foreach (Student student in result)
            {
                // Set the student's round number and add them to the new list
                student.LastRoundParticipated = round;
                student.In = true;
                students_to_add.Add(student);
            }

            school.AddStudents(students_to_add);

            return true;
        }

        /// <summary>
        /// Returns the current school object.
        /// </summary>
        /// <returns>The school object.</returns>
        public School GetSchool()
        {
            return school;
        }

        /// <summary>
        /// Returns the importer object used for importing stuff from files
        /// </summary>
        /// <returns>The importer object.</returns>
        public Importer GetImporter()
        {
            return importer;
        }
    }

    /// <summary>
    /// Class that represents each student.
    /// </summary>
    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    public class Student
    {
        [FieldNotEmpty]
        public string First;
        [FieldNotEmpty]
        public string Last;
        [FieldNotEmpty]
        public int Id;
        [FieldNotEmpty]
        public int Grade;
        [FieldNotEmpty]
        public int Homeroom;

        [FieldHidden]
        // The last round the student participated in
        public long LastRoundParticipated;

        [FieldHidden]
        public string CurrMatchID;

        [FieldHidden]
        // Indicates whether or not the student is in the hunt still
        // True until not true.
        public bool In;

        /// <summary>
        /// Returns the name of the student as a tuple
        /// </summary>
        /// <returns>The student's (FIRSTNAME, LASTNAME)</returns>
        public Tuple<string, string> GetName ()
        {
            return new Tuple<string, string>(First, Last);
        }
    }
}
