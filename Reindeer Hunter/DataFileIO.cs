using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections;

namespace Reindeer_Hunter
{
    public class DataFileIO
    {
        // The master location of the data file
        protected readonly string dataFileLocation = "data.json";

        protected static bool dataFileExists = false;

        // This is what other threads must have to access the sensitive code.
        static object key = new object();

        public DataFileIO()
        {
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

                // Serialize the grades dictionary with Json and then write it
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

                // Convert them to their proper type
                Dictionary<int, Student> students =
                   studentsJarray.ToObject<Dictionary<int, Student>>();

                Dictionary<string, Match> matches =
                   matchesJarray.ToObject<Dictionary<string, Match>>();

                Hashtable various_data =
                   variousJarray.ToObject<Hashtable>();

                // Re-create the hashtable
                Hashtable data = new Hashtable {
                {"students", students },
                {"matches", matches },
                {"misc", various_data }
                };

            return data;
        }
    }

    /// <summary>
    /// A custom exception for when the this is the first time using the program
    /// </summary>
    class ProgramNotSetup : Exception
    {
    }
}
