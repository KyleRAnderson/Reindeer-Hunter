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
using System.Windows.Forms;
using FileHelpers;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for SetupScreen.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public static School school;

        public StartupWindow()
        {
            school = new School();
            if (school.IsData())
            {
                // TODO Figure out how to change screens.
            }
            InitializeComponent();
        }

        private void Import_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog csvopenDialog = new OpenFileDialog
            {

                // Open the file dialog to the user's directory
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                // Filter only for comma-seperated value files. 
                Filter = "csv files (*.csv)|*.csv",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            csvopenDialog.ShowDialog();
            string path = csvopenDialog.FileName;
            Console.WriteLine(path); // TODO remove print

            // Begin processing the data

            var engine = new FileHelperEngine<Student>();

            // Make result into an array of Student
            var result = engine.ReadFile(path);

            int grade = result[0].GetGrade();

            List<Student> students_to_add = new List<Student>();

            foreach (Student student in result)
            {
                students_to_add.Add(student);
            }

            school.AddStudents(grade, students_to_add);

        }
    }

    /// <summary>
    /// Class that represents each student.
    /// </summary>
    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    public class Student
    {
        public string name;

        public int id;

        public int grade;

        public int homeroom;

        /// <summary>
        /// Gets the grade that the student belongs to 
        /// </summary>
        /// <returns>An integer representing the student's grade.</returns>
        public int GetGrade()
        {
            return grade;
        }
    }
}
