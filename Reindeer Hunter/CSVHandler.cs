using System;
using System.Collections.Generic;
using System.Linq;
using FileHelpers;
using System.Windows.Forms;
using Reindeer_Hunter.Data_Classes;
using System.Text;
using System.IO;

namespace Reindeer_Hunter
{
    public static class CSVHandler
    {
        // ID for importing students
        public enum ImportType { Students, MatchResults }

        /// <summary>
        /// Used to import data from a .csv file
        /// </summary>
        /// <param name="id">ID0 = Student, ID1 = ResultStudent</param>
        /// <returns>object list containing the objects made, or null when error occurred.</returns>
        public static List<object[]> Import(ImportType id, string filePath = null, string[] pathsList = null)
        {
            // If importing students
            if (id == ImportType.Students)
            {
                List<object[]> returnList = new List<object[]>();

                foreach (string path in pathsList)
                {
                    // In case the user presses the cancel button
                    if (path == "") return null;

                    // Begin processing the data
                    FileHelperEngine<RawStudent> engine = new FileHelperEngine<RawStudent>
                    {
                        Encoding = Encoding.UTF8
                    };

                    try
                    {
                        // Make result into an array of Student
                        RawStudent[] result = engine.ReadFile(path);
                        // If the user imports a csv with no students in it, error message.
                        if (result.Count() <= 0)
                        {
                            MessageBox.Show("The file you " +
                                "attempted to import students with contains " +
                                "no students in it.", "Error - No Students Imported",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        else returnList.Add(result);
                    }
                    catch (ConvertException)
                    {
                        MessageBox.Show("The file you imported is invalid.",
                            "Error - Nothing imported.",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    catch (FileHelpersException)
                    {
                        MessageBox.Show("The file you imported is invalid.",
                            "Error - Nothing imported.",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                return returnList;
            }

            // If importing match results
            else if (id == ImportType.MatchResults)
            {
                if (filePath == "") return null;
                FileHelperEngine<ResultStudent> engine = new FileHelperEngine<ResultStudent>
                {
                    Encoding = Encoding.UTF8
                };

                try
                {
                    // Make result into an array of Student
                    ResultStudent[] result = engine.ReadFile(filePath);
                    // If the user imports a csv with no students in it, error message.
                    if (result.Count() <= 0)
                    {
                        MessageBox.Show("The file you " +
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
                catch (ConvertException e)
                {
                    MessageBox.Show(String.Format("The file you imported is invalid.\nCheck line {0} column {1} of the file.\nCould not convert \"{2}\" properly.",
                        e.LineNumber, e.ColumnNumber, e.FieldStringValue),
                        "Error - Nothing imported.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                catch (FileHelpersException e)
                {
                    MessageBox.Show(String.Format("The file you imported is invalid. FileHelpers Exception:\n{0}", e.Message),
                        "Error - Nothing imported.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                catch (IOException e)
                {
                    MessageBox.Show(String.Format("Problem accessing the file. Might be in used by another process." +
                        "\nMessage:{0}", e.Message),
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
            var engine = new FileHelperEngine<RawStudent>
            {
                Encoding = Encoding.UTF8
            };

            engine.HeaderText = engine.GetFileHeader();

            engine.WriteFile(exportLocation, students);
        }
    }
}
