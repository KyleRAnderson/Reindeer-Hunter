using Reindeer_Hunter.Hunt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;

namespace Reindeer_Hunter.ThreadMonitors
{
    public class ImportHandler
    {
        private Queue<bool> comms;
        private Thread importThread;
        public event EventHandler<List<object[]>> ImportOver;
        private School _School;
        private ImportStudents importer;
        private Action EndFunction;

        /// <summary>
        /// A class to handle the import students or match results threads.
        /// </summary>
        /// <param name="filePath">The file to import students from</param>
        /// <param name="school">The school object</param>
        /// <param name="endFunction">The function to call if the import is successful at the end.</param>
        public ImportHandler(School school, string[] filePath, Action endFunction = null)
        {
            _School = school;
            EndFunction = endFunction;

            comms = new Queue<bool>();
            importer = new ImportStudents(filePath, school, comms);

            // Make a seperate thread for the import process and start it.
            importThread = new Thread(importer.Import)
            {
                Name = "CSV Importer",
                IsBackground = true
            };
            importThread.Start();

            CompositionTarget.Rendering += Monitor;
        }

        /// <summary>
        /// Monitors the thread, and does stuff when it's done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Monitor(object sender, EventArgs e)
        {
            if (importThread.IsAlive) return;

            importThread.Join();

            // Only run it if there is success.
            if (comms.Count > 0 && comms.ToList<bool>()[0])
                EndFunction?.Invoke();

            // Clear queue
            comms.Clear();


            // Unsubscribe from event
            CompositionTarget.Rendering -= Monitor;
        }
    }
}
