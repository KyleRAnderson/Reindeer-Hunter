using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.Subsystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace Reindeer_Hunter.ThreadMonitors
{
    public class InstantPrintHandler
    {
        /* Used in the lock blocks to make sure that only one thread accesses
         * Sensitive stuff at once */
        private readonly object Key = new object();

        private Queue<PrintMessage> comms;
        private School school;
        private Thread printThread;
        private InstantPrinter printer;
        private ProcessButtonSubsystem subsystem;

        /// <summary>
        /// True when we are currently in the printing process, false otherwise.
        /// </summary>
        public bool IsPrinting { get; set; }

        public InstantPrintHandler(School school, ProcessButtonSubsystem subsystemInCharge)
        {
            IsPrinting = true;
            subsystem = subsystemInCharge;

            // Create the queue for communications purposes.
            comms = new Queue<PrintMessage>();

            // Instantiate school object for simplicity
            this.school = school;

            // Create the matchmaker and then assign the thread target to it
            // +1 to current round because we want next round's matches.
            try
            {
                printer = new InstantPrinter(school.GetCurrRoundMatches(),
                school.GetCurrRoundNo() + 1, Key, comms, school.DataFile.DataLocation);
            }
            catch (IOException)
            {
                return;
            }


            printThread = new Thread(printer.Print)
            {
                Name = "Instant Printer"
            };
            printThread.Start();

            // Put the execute function into the mainloop to be executed
            CompositionTarget.Rendering += PrintMonitor;
        }

        public void PrintMonitor(object sender, EventArgs e)
        {
            lock (Key)
            {
                if (comms.Count() <= 0) return;

                // Convert queue to list and retrieve last value
                List<PrintMessage> returnList = comms.ToList<PrintMessage>();
                PrintMessage returnValue = returnList[returnList.Count() - 1];

                // Clear queue
                comms.Clear();

                // Update the progress display
                subsystem.UpdateOperationStatus(returnValue.Message, returnValue.Progress);

                // Progress is 100 % when complete
                if (returnValue.Progress == 1)
                {
                    // Give the second thread the info it needs to move the file
                    string path;
                    SaveFileDialog fileDialog = new SaveFileDialog
                    {
                        // Open the file dialog to the user's directory
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                        // Filter only for comma-seperated value files. 
                        Filter = "pdf files (*.pdf)|*.pdf",
                        FilterIndex = 2,
                        RestoreDirectory = true
                    };

                    fileDialog.ShowDialog();

                    if (fileDialog.FileName == "")
                    {
                        path = System.IO.Path.Combine(Environment.GetFolderPath(
                            Environment.SpecialFolder.Desktop), "FilledLicenses.pdf");

                        System.Windows.Forms.MessageBox.Show("Export location Error. File was outputted to "
                            + path, "Export Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else path = fileDialog.FileName;

                    // Join the thread
                    printThread.Join();

                    // Unsubscribe from event.
                    CompositionTarget.Rendering -= PrintMonitor;

                    // In case the user tries to overwrite another file
                    if (File.Exists(path)) File.Delete(path);

                    // Move the temporary file out of the code's folder.
                    File.Move(returnValue.Path, path);

                    System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(path));

                    IsPrinting = false;
                }
            }
        }
    }
}
