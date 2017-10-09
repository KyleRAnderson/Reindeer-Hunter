using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using Reindeer_Hunter.Data_Classes;
using System.Windows;
using System.Security.Cryptography;
using System.Diagnostics;
using Microsoft.Win32;

namespace Reindeer_Hunter
{
    public class DataFileIO
    {
        public string DataLocation { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Reindeer Hunter Data");

        // The master location of the data file
        protected readonly string dataFileLocation;

        protected static bool dataFileExists = false;

        // This is what other threads must have to access the sensitive code.
        static object key = new object();

        public DataFileIO()
        {
            if (!Directory.Exists(DataLocation)) DataLocation = Environment.CurrentDirectory;

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
                    (Newtonsoft.Json.Linq.JObject)data_hashtable["students"];

                Newtonsoft.Json.Linq.JObject matchesJarray =
                    (Newtonsoft.Json.Linq.JObject)data_hashtable["matches"];

                Newtonsoft.Json.Linq.JObject variousJarray =
                    (Newtonsoft.Json.Linq.JObject)data_hashtable["misc"];

                Newtonsoft.Json.Linq.JObject victorsJarray = null;
                // Only do this if the victors key exists
                if (data_hashtable.ContainsKey("victors")) {
                        victorsJarray =
                            (Newtonsoft.Json.Linq.JObject)data_hashtable["victors"];
                }   

            // Convert them to their proper type
                Dictionary<int, Student> students =
                   studentsJarray.ToObject<Dictionary<int, Student>>();

                Dictionary<string, Match> matches =
                   matchesJarray.ToObject<Dictionary<string, Match>>();

                Hashtable various_data =
                   variousJarray.ToObject<Hashtable>();

                Dictionary<int, Victor> victors;                

                // Re-create the hashtable
                Hashtable data = new Hashtable {
                {"students", students },
                {"matches", matches },
                {"misc", various_data }
                };

            if (victorsJarray != null)
            {
                victors = victorsJarray.ToObject<Dictionary<int, Victor>>();
                data.Add("victors", victors);
            }

            return data;
        }

        /// <summary>
        /// Function to save the victor classes to the file.
        /// </summary>
        /// <param name="victorsToSave"></param>
        public void SaveVictors(Dictionary<int, Victor> victorsToSave)
        {
            // Make a new hashtable and fill it with what's already there
            Hashtable dataToWrite = new Hashtable(Read());

            // Make sure the victors part exists
            if (!dataToWrite.ContainsKey("victors")) dataToWrite.Add("victors", null);

            dataToWrite["victors"] = victorsToSave;

            Write(dataToWrite);
        }

        /// <summary>
        /// Function to retrieve the dictionary of victors, if it exists
        /// </summary>
        /// <returns>Dictionary of the victor's student number and the victor object.</returns>
        public Dictionary<int, Victor> GetVictors()
        {
            Hashtable data = Read();

            // If the key doesn't exist, it's the first time where we're setting this up.
            if (!data.ContainsKey("victors")) return null;

            return (Dictionary<int, Victor>)data["victors"];
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
                // TODO error handling
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

            foreach (FileInfo file in dir.GetFiles())
            {
                // In case there's other stuff there.
                if (file.Extension == "pdf" || file.Extension == ".json") file.Delete();
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
    }

    /// <summary>
    /// A custom exception for when the this is the first time using the program
    /// </summary>
    class ProgramNotSetup : Exception
    {
    }
}
