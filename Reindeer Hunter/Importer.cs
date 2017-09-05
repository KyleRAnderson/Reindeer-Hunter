using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;
using System.Windows.Forms;

namespace Reindeer_Hunter
{
    public class Importer
    {
        /// <summary>
        /// Used to import data from a .csv file
        /// </summary>
        /// <param name="id">ID0 = Student, ID1 = ResultStudent</param>
        /// <returns>object list containing the objects made, or null when error occurred.</returns>
        public object[] Import(int id)
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

            // In case the user presses the cancel button
            if (path == "") return null;

            // TODO do it better!
            // Begin processing the data
            if (id == 0)
            {
                var engine = new FileHelperEngine<Student>();

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
                    else return result;
                }
                catch (FileHelpers.ConvertException)
                {
                    System.Windows.Forms.MessageBox.Show("The file you imported is invalid.",
                        "Error - Nothing imported.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            else
            {
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
                    else return result;
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
