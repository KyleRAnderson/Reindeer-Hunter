using System;
using System.Collections.Generic;
using System.Linq;
using FileHelpers;
using System.Windows.Forms;
using Reindeer_Hunter.Data_Classes;

namespace Reindeer_Hunter
{
    public static class CSVHandler
    {
        // ID for importing students
        public static int IMPORT_STUDENTS = 0;
        public static int IMPORT_MATCH_RESULTS = 1;

        /// <summary>
        /// Used to import data from a .csv file
        /// </summary>
        /// <param name="id">ID0 = Student, ID1 = ResultStudent</param>
        /// <returns>object list containing the objects made, or null when error occurred.</returns>
        public static List<object[]> Import(int id, String filePath = null, String[] pathsList = null)
        {
            // If importing students
            if (id == IMPORT_STUDENTS)
            {
                List<object[]> returnList = new List<object[]>();

                foreach (string path in pathsList)
                {
                    // In case the user presses the cancel button
                    if (path == "") return null;

                    // Begin processing the data
                    var engine = new FileHelperEngine<RawStudent>();

                    try
                    {
                        // Make result into an array of Student
                        var result = engine.ReadFile(path);
                        // If the user imports a csv with no students in it, error message.
                        if (result.Count() <= 0)
                        {
                            System.Windows.Forms.MessageBox.Show("The file you " +
                                "attempted to import students with contains " +
                                "no students in it.", "Error - No Students Imported",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        else returnList.Add(result);
                    }
                    catch (FileHelpers.ConvertException)
                    {
                        System.Windows.Forms.MessageBox.Show("The file you imported is invalid.",
                            "Error - Nothing imported.",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    catch (FileHelpers.FileHelpersException)
                    {
                        System.Windows.Forms.MessageBox.Show("The file you imported is invalid.",
                            "Error - Nothing imported.",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                return returnList;
            }

            // If importing match results
            else if (id == IMPORT_MATCH_RESULTS)
            {
                var engine = new FileHelperEngine<ResultStudent>();

                if (filePath == "") return null;
                try
                {
                    // Make result into an array of Student
                    var result = engine.ReadFile(filePath);
                    // If the user imports a csv with no students in it, error message.
                    if (result.Count() <= 0)
                    {
                        System.Windows.Forms.MessageBox.Show("The file you " +
                            "attempted to import students with contains " +
                            "no results in it.", "Error - No results Imported",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    else
                    {
                        List<object[]> returnList = new List<object[]>
                        {
                            result
                        };
                        return returnList;
                    }
                }
                catch (ConvertException)
                {
                    MessageBox.Show("The file you imported is invalid.",
                        "Error - Nothing imported.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            else return null;
        }

        /// <summary>
        /// Exports the given students to the given file path.
        /// </summary>
        /// <param name="students">The students to export</param>
        /// <param name="exportLocation"></param>
        public static void ExportStudents(List<RawStudent> students, string exportLocation)
        {
            var engine = new FileHelperEngine<RawStudent>();
            engine.HeaderText = engine.GetFileHeader();

            engine.WriteFile(exportLocation, students);
        }
    }
}
