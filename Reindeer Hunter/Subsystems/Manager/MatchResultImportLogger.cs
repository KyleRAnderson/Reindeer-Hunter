using System;
using System.Collections.Generic;
using System.IO;

namespace Reindeer_Hunter.Subsystems.Manager
{
    /// <summary>
    /// Used by the school object to log errors while importing
    /// match results.
    /// </summary>
    public class MatchResultImportLogger
    {
        /// <summary>
        /// The location of the logs
        /// </summary>
        public string LogLocation { get; private set; }
        private List<string> Messages = new List<string>();

        public MatchResultImportLogger(DataFileIO dataFile, long currentRoundNumber)
        {
            // Get current time
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            // Make log location
            LogLocation = Path.Combine(dataFile.DataLocation, 
                string.Format("MatchResultImportLog {0}.txt", timestamp));

            Messages.Add(string.Format("Log generated {0} during round {1}", timestamp, currentRoundNumber));
        }

        /// <summary>
        /// Adds an error line to the log file
        /// </summary>
        /// <param name="message">The log message to put in the file</param>
        public void AddLine(string message)
        {
            // Just add the message
            Messages.Add(message);
        }

        /// <summary>
        /// Saves the log messages and closes. If this is not called,
        /// nothing will be saved.
        /// </summary>
        public void SaveAndClose()
        {
            // Make the file and write to it.
            File.WriteAllLines(LogLocation, Messages);
        }
    }
}
