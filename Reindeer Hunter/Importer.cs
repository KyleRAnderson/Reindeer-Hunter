using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;
using System.Windows.Forms;
using Reindeer_Hunter.Data_Classes;

namespace Reindeer_Hunter
{
    public class Importer
    {
        private int IMPORT_STUDENTS = 0;
        private int IMPORT_RESULTS = 1;

        /// <summary>
        /// Used to import data from a .csv file
        /// </summary>
        /// <param name="id">ID0 = Student, ID1 = ResultStudent</param>
        /// <returns>object list containing the objects made, or null when error occurred.</returns>
        public List<object[]> Import(int id)
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

            // If importing students, multiselect is enabled.
            if (id == IMPORT_STUDENTS) csvopenDialog.Multiselect = true;

            csvopenDialog.ShowDialog();

            // If importing students
            if (id == IMPORT_STUDENTS)
            {
                List<object[]> returnList = new List<object[]>();

                foreach(string path in csvopenDialog.FileNames)
                {
                    // In case the user presses the cancel button
                    if (path == "") return null;

                    // Begin processing the data
                    var engine = new FileHelperEngine<ImportedStudent>();

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
                }
                return returnList;
            }

            // If importing match results
            else
            {
                string path = csvopenDialog.FileName;
                var engine = new FileHelperEngine<ResultStudent>();

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
                    else
                    {
                        List<object[]> returnList = new List<object[]>();
                        returnList.Add(result);
                        return returnList;
                    }
                }
                catch (FileHelpers.ConvertException)
                {
                    System.Windows.Forms.MessageBox.Show("The file you imported is invalid.",
                        "Error - Nothing imported.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }
    }
}
