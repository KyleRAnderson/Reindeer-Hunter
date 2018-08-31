using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Subsystems.Student_Manager
{
    public class ExportStudents
    {
        private List<Match> RelevantMatches;
        private School _School;
        private String ExportLocation;

        public ExportStudents(List<Match> relevantMatches, School school, string exportLocation)
        {
            _School = school;
            RelevantMatches = relevantMatches;
            ExportLocation = exportLocation;

            // Set up this thread
            Thread thread = new Thread(Execute)
            {
                Name = "Export Thread",
                IsBackground = true
            };

            thread.Start();
        }

        /// <summary>
        /// Function that actually executes the exporting of the students
        /// </summary>
        private void Execute()
        {
            // Make students list from the relevant matches.
            List<Student> studentsToPrint = new List<Student>();

            // Add the relevant students to the list.
            foreach (Match match in RelevantMatches)
            {
                studentsToPrint.AddRange(_School.GetStudentsInMatch(match));
            }

            // Convert the full students to Raw Students (students ready for export)
            List<RawStudent> rawStudents = new List<RawStudent>();
            foreach (Student student in studentsToPrint)
            {
                rawStudents.Add(RawStudent.CreateFromStudent(student));
            }

            // Export the students to csv.
            CSVHandler.ExportStudents(rawStudents, ExportLocation);

            // Show the directory in the file explorer.
            System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(ExportLocation));
        }
    }
}
