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

        public DataFileIO()
        {
            // Determine if the data file exists.
            string directory = Directory.GetCurrentDirectory();
            if (!File.Exists(directory + dataFileLocation))
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
            // Open the data file for writing
            StreamWriter dataFileWrite = new StreamWriter(dataFileLocation);

            // Serialize the grades dictionary with Json and then write it
            string writable = JsonConvert.SerializeObject(data_to_write);
            dataFileWrite.WriteLine(writable);

            // Close the data file
            dataFileWrite.Close();
        }

        /// <summary>
        /// Reads the first line in the data file and returns it
        /// </summary>
        /// <returns>The first line in the data file deserialized as a dictionary</returns>
        public Hashtable Read()
        {
            if (!dataFileExists)
            {
                dataFileExists = true;
                throw new ProgramNotSetup();
            }

            // Open the data file for reading
            StreamReader dataFileRead = new StreamReader(dataFileLocation);

            // Get the serialized json dictionary. Close the dataFile.
            string readData = dataFileRead.ReadLine();
            dataFileRead.Close();

            // Deserialize and return the json dictionary
            Hashtable data_hashtable = 
                (Hashtable) JsonConvert.DeserializeObject<Hashtable>(readData);

            // Begin the reconstruction process of the data Hashtable

            // Load up the sub-collectives as JObjects
            Newtonsoft.Json.Linq.JObject gradesJarray =
                (Newtonsoft.Json.Linq.JObject)data_hashtable["grades"];

            Newtonsoft.Json.Linq.JArray matchesJarray =
                (Newtonsoft.Json.Linq.JArray)data_hashtable["matches"];

            // Convert them to their proper type
            Dictionary<int, List<Student>> grades =
               gradesJarray.ToObject<Dictionary<int, List<Student>>>();

            List<Match> matches =
               matchesJarray.ToObject<List<Match>>();

            // Re-create the hashtable
            Hashtable data = new Hashtable {
                {"grades", grades },
                {"matches", matches }
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
