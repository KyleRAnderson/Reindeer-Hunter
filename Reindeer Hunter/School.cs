using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter
{
    /// <summary>
    /// This class will be in charge of evereything. Holds the lists of students and other information.
    /// </summary>
    public class School
    {
        // This dictionary will contain all data for the program
        protected static Hashtable data;

        protected static DataFileIO dataFile;

        public School() {
            try
            {
                // Make the new dataFile string and get the data from the data file.
                dataFile = new DataFileIO();
                data = dataFile.Read();
            }
            catch (ProgramNotSetup)
            {
                // Set up the program if it has no data yet.
                FirstTimeSetup();
                data = dataFile.Read(); // TODO Do this better?
            }
        }

        /// <summary>
        /// Returns true if students exist already, false otherwise.
        /// </summary>
        /// <returns>True if at least one student exists, false otherwise. </returns>
        public bool IsData()
        {
            // True when there is discovered data.
            bool isData = false;
            Dictionary<int, List<Student>> grades = GetGrades();
            
            foreach (KeyValuePair<int, List<Student>> pair in grades)
            {
                List<Student> grade = pair.Value;
                if (grade.Count() > 0)
                {
                    isData = true;
                    break;
                }
            }

            return isData;
        }

        /// <summary>
        /// Returns the dictionary of grades containing students.
        /// </summary>
        /// <returns>The dictionary of grades with their students in lists inside.</returns>
        private Dictionary<int, List<Student>> GetGrades()
        {
            return (Dictionary < int, List < Student >> ) data["grades"];
        }

        public void AddStudents(int grade, List<Student> students)
        {
            // Retrieve the grade's list
            Dictionary<int, List<Student>> grades = 
                (Dictionary < int, List < Student >> ) data["grades"];
            List<Student> student_list = grades[grade];

            // Add the students to the grade's list
            student_list.AddRange(students);

            Save();
        }

        /// <summary>
        /// Saves all students added and settings changed.
        /// </summary>
        public void Save()
        {
            dataFile.Write(data);
        }

        /// <summary>
        /// Loads the lists of students from the file.
        /// </summary>
        public void LoadStudents()
        {

        }

        /// <summary>
        /// Used to set up the file for first-time use. 
        /// </summary>
        protected void FirstTimeSetup()
        {

            // Make the lists of students TODO make this into a smarter loop of somekind
            List<Student> gr_9s = new List<Student>();
            List<Student> gr_10s = new List<Student>();
            List<Student> gr_11s = new List<Student>();
            List<Student> gr_12s = new List<Student>();

            // Create the grades dictionary
            Dictionary<int, List<Student>> grades = new Dictionary<int, List<Student>>
            {
                // Add the lists to the grades dictionary
                { 9, gr_9s },
                { 10, gr_10s },
                { 11, gr_11s },
                { 12, gr_12s }
            };

            // Create new matches list
            List<Match> matches = new List<Match>();

            // Create the data dictionary
            Hashtable data = new Hashtable
            {
                { "grades", grades },
                { "matches", matches }
            };

            dataFile.Write(data);
        }
    }
}
