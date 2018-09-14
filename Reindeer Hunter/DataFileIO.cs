using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using Reindeer_Hunter.Data_Classes;
using System.Windows;
using System.Security.Cryptography;
using System.Diagnostics;
using Microsoft.Win32;
using Reindeer_Hunter.Hunt;

namespace Reindeer_Hunter
{
    public class DataFileIO
    {
        /// <summary>
        /// The location (should be the user's AppData folder) where the data for the user will be saved.
        /// </summary>
        public string DataLocation { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Reindeer Hunter Data");
        public static readonly string ProgramLocation = Environment.CurrentDirectory;

        // The master location of the data file
        protected readonly string dataFileLocation;

        protected static bool dataFileExists = false;

        // This is what other threads must have to access the sensitive code.
        private static object key = new object();

        public static readonly string FFADataLoc = "FFA";
        public static readonly string StudentDataLoc = "students";
        public static readonly string MatchDataLoc = "matches";
        public static readonly string MiscDataLoc = "misc";
        public static readonly string TerminatedLoc = "terminated";

        // Locations of FFA data
        public static readonly string winnerDataLoc = "winner";
        public static readonly string victorDataLoc = "victors";

        // Location of the user manual
        public static string ManualLoc
        {
            get
            {
                return Path.Combine(ProgramLocation, "User_Manual.pdf");
            }
        }

        public bool ManualExists
        {
            get
            {
                return File.Exists(ManualLoc);
            }
        }

        /// <summary>
        /// Boolean telling if the Reindeer Hunt is over or not.
        /// True when it's over, false otherwise.
        /// </summary>
        public bool IsTerminated
        {
            get
            {
                Hashtable miscData = (Hashtable)Read()[MiscDataLoc];
                if (miscData.ContainsKey(TerminatedLoc)) return (bool)miscData[TerminatedLoc];
                else return false;
            }

            set
            {
                // If it's false, we just don't set anything
                if (!value) return;
                Hashtable data = Read();
                
                // If it exists, modify it. Otherwise, create it. 
                if (((Hashtable)data[MiscDataLoc]).ContainsKey(TerminatedLoc)) ((Hashtable)data[MiscDataLoc])[TerminatedLoc] = value;
                else ((Hashtable)data[MiscDataLoc]).Add(TerminatedLoc, value);

                Write(data);
            }
        }

        public DataFileIO()
        {
            if (StartupWindow.IsDevMode || !Directory.Exists(DataLocation)) DataLocation = Environment.CurrentDirectory;

            dataFileLocation = Path.Combine(DataLocation, "data.json");

            // Determine if the data file exists.
            string directory = Directory.GetCurrentDirectory();
            if (!File.Exists(Path.Combine(directory, dataFileLocation)))
            {
                Create();

                /* Lets the School dynamic know that we're not ready to proceed
                 * Because the program has not been set up
                 */
            }

            else dataFileExists = true;
        }

        /// <summary>
        /// Create and close the data file
        /// </summary>
        private void Create()
        {
            File.Create(dataFileLocation).Close();
        }

        /// <summary>
        /// Serializes and writes the given data to the data file.
        /// </summary>
        /// <param name="data_to_write"> The unserialized data to be written to the file</param>
        public void Write(Hashtable data_to_write)
        {
            lock(key)
            {
                // Open the data file for writing
                StreamWriter dataFileWrite = new StreamWriter(dataFileLocation);

                // Serialize the dictionary with Json and then write it
                string writable = JsonConvert.SerializeObject(data_to_write);
                dataFileWrite.WriteLine(writable);

                // Close the data file
                dataFileWrite.Close();
            }
        }

        /// <summary>
        /// Reads the first line in the data file and returns it
        /// </summary>
        /// <returns>The first line in the data file deserialized as a dictionary</returns>
        public Hashtable Read()
        {
            string readData;
            lock (key)
            {
                if (!dataFileExists)
                {
                    dataFileExists = true;
                    throw new ProgramNotSetup();
                }

                // Open the data file for reading
                StreamReader dataFileRead = new StreamReader(dataFileLocation);

                // Get the serialized json dictionary. Close the dataFile.
                readData = dataFileRead.ReadLine();
                dataFileRead.Close();
            }
                // Deserialize and return the json dictionary
                Hashtable data_hashtable =
                    (Hashtable)JsonConvert.DeserializeObject<Hashtable>(readData);

                /* 
                 * Begin the reconstruction process of the data Hashtable
                 */

                // Load up the sub-collectives as JObjects
                Newtonsoft.Json.Linq.JObject studentsJarray =
                    (Newtonsoft.Json.Linq.JObject)data_hashtable[StudentDataLoc];

                Newtonsoft.Json.Linq.JObject matchesJarray =
                    (Newtonsoft.Json.Linq.JObject)data_hashtable[MatchDataLoc];

                Newtonsoft.Json.Linq.JObject variousJarray =
                    (Newtonsoft.Json.Linq.JObject)data_hashtable[MiscDataLoc];

                Newtonsoft.Json.Linq.JObject victorsJarray = null;
                // Only do this if the victors key exists
                if (data_hashtable.ContainsKey(FFADataLoc)) {
                        victorsJarray =
                            (Newtonsoft.Json.Linq.JObject)data_hashtable[FFADataLoc];
                }   

            // Convert them to their proper type
                Dictionary<int, Student> students =
                   studentsJarray.ToObject<Dictionary<int, Student>>();

                Dictionary<string, Match> matches =
                   matchesJarray.ToObject<Dictionary<string, Match>>();

                Hashtable various_data =
                   variousJarray.ToObject<Hashtable>();

                Hashtable FFAData;                

                // Re-create the hashtable
                Hashtable data = new Hashtable {
                {StudentDataLoc, students },
                {MatchDataLoc, matches },
                {MiscDataLoc, various_data }
                };

            if (victorsJarray != null)
            {
                FFAData = victorsJarray.ToObject<Hashtable>();
                data.Add(FFADataLoc, FFAData);
            }

            return data;
        }

        /// <summary>
        /// Function to save the victor classes to the file.
        /// </summary>
        /// <param name="dataToSave"></param>
        public void SaveVictors(Hashtable dataToSave)
        {
            // Make a new hashtable and fill it with what's already there
            Hashtable dataToWrite = new Hashtable(Read());

            // Make sure the victors part exists
            if (!dataToWrite.ContainsKey(FFADataLoc)) dataToWrite.Add(FFADataLoc, null);

            dataToWrite[FFADataLoc] = dataToSave;

            Write(dataToWrite);
        }

        /// <summary>
        /// Function to retrieve the dictionary of victors, if it exists
        /// </summary>
        /// <returns>Dictionary of the victor's student number and the victor object.</returns>
        public Hashtable GetFFAData()
        {
            Hashtable returnable = new Hashtable();
            Hashtable data = (Hashtable)Read();

            // If the key doesn't exist, it's the first time where we're setting this up.
            if (!data.ContainsKey(FFADataLoc)) return null;

            data = (Hashtable)data[FFADataLoc];

            // Reconstruct the hashtable
            Newtonsoft.Json.Linq.JObject victorsJarray =
                    (Newtonsoft.Json.Linq.JObject)data[victorDataLoc];

            Dictionary<int, Victor> victors =
                  victorsJarray.ToObject<Dictionary<int, Victor>>();

            returnable.Add(victorDataLoc, victors);

            if (data.ContainsKey(winnerDataLoc))
            {
                Newtonsoft.Json.Linq.JArray winnersJarray =
                    (Newtonsoft.Json.Linq.JArray)data[winnerDataLoc];

                List<Victor> winner =
                  winnersJarray.ToObject<List<Victor>>();

                returnable.Add(winnerDataLoc, winner);
            }

            return returnable;
        }

        /// <summary>
        /// Function for importing and validating a new data file
        /// </summary>
        /// <param name="openLoc">String location of the file to import.</param>
        public void Import (string openLoc)
        {
            // The appropriate length of the checksum in bytes.
            int lengthOfChecksum = 16;

            MD5 md5 = MD5.Create();
            FileStream stream = File.OpenRead(openLoc);
            int byteLength = (int)stream.Length;
            byte[] bytes = new byte[byteLength];
            stream.Read(bytes, 0, byteLength);

            // Close the stream now that we're done
            stream.Close();

            // Make a checksum for the part of the file that contains data
            byte[] dataBytes = new byte[byteLength - lengthOfChecksum];
            Buffer.BlockCopy(bytes, 0, dataBytes, 0, byteLength - lengthOfChecksum);
            byte[] newChecksum = md5.ComputeHash(dataBytes);

            // Find the original checksum
            byte[] oldChecksum = new byte[lengthOfChecksum];
            Buffer.BlockCopy(bytes, byteLength - lengthOfChecksum, oldChecksum, 0, lengthOfChecksum);

            // Compare
            if (!oldChecksum.SequenceEqual(newChecksum))
            {
                MessageBox.Show("Error - File has been edited externally and cannot be used with this program. " +
                    "Nothing has been imported.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // If they are equal, proceed.
            else
            {
                // Move the selected file to import.
                File.Delete(dataFileLocation);
                File.Copy(openLoc, dataFileLocation);

                // Restart the application.
                RestartApplication();
            }
        }

        /// <summary>
        /// Function for exporting the data file, includign a checksum
        /// </summary>
        public void Export()
        {
            // Generate checksum for the file
            MD5 md5 = MD5.Create();
            FileStream stream = File.OpenRead(dataFileLocation);
            byte[] checkSum = md5.ComputeHash(stream);
            stream.Close();

            // Copy the file.
            SaveFileDialog askLoc = new SaveFileDialog
            {

                // Open the file dialog to the user's directory
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                // Filter only for comma-seperated value files. 
                Filter = "json files (*.json)|*.json",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            askLoc.ShowDialog();

            string copyLoc = askLoc.FileName;
            // In case they cancel.
            if (copyLoc == null || copyLoc == "") return;

            // If the file exists already, delete it, else we get errors.
            if (File.Exists(copyLoc)) File.Delete(copyLoc);

            // Copy the data file over
            File.Copy(dataFileLocation, copyLoc);

            // Open the file and add the checksum.
            stream = new FileStream(copyLoc, FileMode.Append, FileAccess.Write);
            var bw = new BinaryWriter(stream);
            bw.Write(checkSum);

            // Close all streams
            bw.Close();
            stream.Close();
        }

        /// <summary>
        /// Simple function to erase the program's data and then restart the program.
        /// </summary>
        public void EraseData()
        {
            // Get directory and loop around deleting all the files.
            DirectoryInfo dir = new DirectoryInfo(DataLocation);
            string[] acceptable_paths = { ".pdf", ".json", ".txt" };

            List<FileInfo> filesInSaveDir = new List<FileInfo>(dir.GetFiles());
            foreach (FileInfo file in filesInSaveDir)
            {
                // In case there's other stuff there.
                if (acceptable_paths.Contains(file.Extension)) file.Delete();
            }
        }

        /// <summary>
        /// Function used to restart this application.
        /// </summary>
        public void RestartApplication()
        {
            Process.Start(Application.ResourceAssembly.Location);
            QuitApplication();
        }

        /// <summary>
        /// Function to shutdown the application
        /// </summary>
        public void QuitApplication()
        {
            Application.Current.Shutdown();
        }

        public void Import_Template_PDF()
        {
            // Get the pdf that the user wants to import.
            OpenFileDialog templateOpenDialog = new OpenFileDialog
            {

                // Open the file dialog to the user's directory
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                // Filter only for comma-seperated value files. 
                Filter = "pdf files (*.pdf)|*.pdf",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            templateOpenDialog.ShowDialog();

            if (templateOpenDialog.FileName == "" || templateOpenDialog.FileName == null)
            {
                throw new IOException("User Cancelled");
            }

            // Delete the old one and replace it with the new one. 
            if (TemplatePDFExists) File.Delete(TemplatePDFLoc);
            File.Copy(templateOpenDialog.FileName, TemplatePDFLoc);
        }

        public string TemplatePDFLoc
        {
            get
            {
                return Path.Combine(DataLocation, TemplatePDFName);
            }
        }

        public bool TemplatePDFExists
        {
            get
            {
                return File.Exists(TemplatePDFLoc);
            }
        }

        /// <summary>
        /// The name, as in the last part of the path, of the template pdf file. 
        /// </summary>
        public static string TemplatePDFName = "TemplatePDF.pdf";
    }

    /// <summary>
    /// A custom exception for when the this is the first time using the program
    /// </summary>
    class ProgramNotSetup : Exception
    {
    }
}
