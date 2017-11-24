using System;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;
using Reindeer_Hunter.Data_Classes;
using System.Windows.Forms;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Class responsible for outputting the match data
    /// into a PDF file format ready for printing.
    /// </summary>
    public class InstantPrinter
    {
        // List of matches to create form from
        protected static List<Match> MatchList;

        // Path location of the template PDF file
        private string TemplateLocation;

        // Path where the duplicated file will be exported.
        private string TempLocation;

        // Path location where filled file will be exported.
        private string OutputLocation;

        // Temporary path for stuff
        private string Temp2Location;

        // The end date to be put on the licenses.
        private string EndDate;

        // Queue used for communication
        protected static Queue<PrintMessage> Print_Comms;

        protected readonly object Key;

        protected int StuffDone;
        protected int StuffToDo;

        // The round number
        protected static long RoundNo;

        // Statuses
        public static int SETUP = 0;
        public static int CREATINGPAGES = 1;
        public static int FILLING = 2;
        public static int COMPLETED = 3;
        public static int GENERATING_LICENSE_OBJECTS = 4;
        public Stopwatch stopwatch;

        // Index number is the index value of the form field.
        public int IndexNo = 1;
        // Page number is the current page we're creating .
        public int PageNo = 0;

        public InstantPrinter(List<Match> matches,  
            long roundNo, object key, Queue<PrintMessage> comms, string DataPath, string endDate)
        {
            // Set up file locations
            TempLocation = Path.Combine(DataPath, "Duplicate.pdf");
            OutputLocation = Path.Combine(DataPath, "FilledLicenses.pdf");
            Temp2Location = Path.Combine(DataPath, "Temporary.pdf");
            TemplateLocation = Path.Combine(DataPath, "TemplatePDF.pdf");


            Key = key;
            MatchList = matches;
            RoundNo = roundNo;
            Print_Comms = comms;

            EndDate = endDate;

            if (!File.Exists(TemplateLocation))
            {
                // Get template location
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
                string path = templateOpenDialog.FileName;

                if (path == "" || !templateOpenDialog.CheckFileExists) throw new IOException("User canceled.");
                else
                {
                    File.Copy(path, TemplateLocation);
                }
            }            
        }

        /// <summary>
        /// The function responsible for completely filling the PDF
        /// </summary>
        public void Print()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            SendUpdateMessage(0, status: GENERATING_LICENSE_OBJECTS);

            // Make license objects.
            List<License> licenses = Generate_License_Objects(MatchList, EndDate);

            SendUpdateMessage(0, status: SETUP);

            /* 
             * Determine how many pages will be necessary. 
             * Keep in mind that 8 licenses can be fit on each page.
             * So, we divide the license number by 8.
             */
            int pagesNeeded = (int)Math.Ceiling((double)licenses.Count / 8);

            // Duplicate as many pages as is necessary
            Document document = new Document();
            PdfCopy copy = new PdfSmartCopy(document, new FileStream(
                TempLocation, FileMode.Create));

            // Used this to close all the readers later
            List<PdfReader> readers = new List<PdfReader>();

            copy.Open();
            copy.SetMergeFields();
            document.Open();

            for (int copier = 0; copier < pagesNeeded; copier++)
            {
                PdfReader pdfreader = RenamePDFFields(TemplateLocation, Temp2Location, copier);
                copy.AddDocument(pdfreader);
                readers.Add(pdfreader);
                
                File.Delete(Temp2Location);
            }

            copy.CloseStream = true;
            copy.Close();
            document.Close();

            foreach (PdfReader readerToClose in readers) readerToClose.Close();

            PdfReader reader = new PdfReader(TempLocation);
            PdfStamper stamper = new PdfStamper(reader, 
                new FileStream(OutputLocation, FileMode.Create));
            AcroFields formFields = stamper.AcroFields;

            StuffDone = 0;
            StuffToDo = licenses.Count;

            // Loop through all matches and write the information
            for (int a = 0; a < StuffToDo; a++)
            {
                Tuple<int, int> fraction = new Tuple<int, int>(StuffDone, StuffToDo);
                double percent = (double) StuffDone / StuffToDo;
                SendUpdateMessage(percent, FILLING, fraction);

                License license = licenses[a];

                // There are eight of the same textboxes a page, so after that increase page no.
                if (IndexNo > 8)
                {
                    IndexNo = 1;
                    PageNo += 1;
                }

                string student1path;
                string student2path;
                string roundpath;
                string datePath;

                // If it's the first license of the page, there's no underscore and id number
                if (IndexNo < 2)
                {
                    student1path = string.Format("Student1P{0}", PageNo);
                    student2path = string.Format("Student2P{0}", PageNo);
                    roundpath = string.Format("RoundP{0}", PageNo);
                    datePath = string.Format("DateP{0}", PageNo);
                }
                else
                {
                    student1path = string.Format("Student1_{0}P{1}", IndexNo, PageNo);
                    student2path = string.Format("Student2_{0}P{1}", IndexNo, PageNo);
                    roundpath = string.Format("Round_{0}P{1}", IndexNo, PageNo);
                    datePath = string.Format("Date_{0}P{1}", IndexNo, PageNo);
                }

                // Fill in the form fields.
                formFields.SetField(student1path,
                    license.Student1Field);

                // Set the round number.
                formFields.SetField(roundpath, license.Round.ToString());

                // Set the date
                formFields.SetField(datePath, EndDate);

                formFields.SetField(student2path,
                    license.Student2Field);

                IndexNo += 1;

                StuffDone += 1;
            }
            stamper.FormFlattening = false;
            stamper.Close();
            reader.Close();

            File.Delete(TempLocation);

            SendUpdateMessage(1, COMPLETED);
        }

        private PdfReader RenamePDFFields(string source, string output, int pageNo)
        {
            PdfReader reader = new PdfReader(source);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(output, FileMode.Create));
            AcroFields forms = stamper.AcroFields;
            List<string> keys = new List<string>(forms.Fields.Keys);

            foreach (string formKey in keys)
            {
                forms.RenameField(formKey, String.Format("{0}P{1}", formKey, pageNo));
            }

            stamper.Close();
            return reader;
        }

        /// <summary>
        /// Creates all the licenses as well as fake licenses so we only get
        /// one grade per page.
        /// </summary>
        /// <param name="matches">The matches to create the licenses with</param>
        /// <param name="date">The date to put on the licenses.</param>
        /// <returns>The list of licenses generated</returns>
        private List<License> Generate_License_Objects(List<Match> matches, string date)
        {
            List<License> licenses = new List<License>();

            foreach (Match match in matches) licenses.AddRange(License.CreateFromMatch(match, date));

            licenses = FormatLicenseList(licenses);

            return licenses;
        }

        /// <summary>
        /// Handles sorting the licenses by grade and adding the fake licenses.
        /// </summary>
        /// <param name="licenses">The list of licenses to sort and format.</param>
        /// <returns>The properly formatted list of licenses.</returns>
        private List<License> FormatLicenseList(List<License> licenses)
        {
            List<License> sortedLicenses;

            sortedLicenses = licenses.
                OrderBy(x => x.Homeroom1).ToList();

            List<Tuple<int, int>> indexesToAddFakeLicensesTo = new List<Tuple<int, int>>();

            int lastGrade = sortedLicenses[0].Grade;
            foreach (License license in sortedLicenses)
            {
                if (license.Grade != lastGrade && license.Grade != 0)
                {
                    // Figure out the index in the list where the grade changed.
                    int insertIndex = sortedLicenses.IndexOf(license);

                    // Add it to the list and move on
                    indexesToAddFakeLicensesTo.Add(new Tuple<int, int>(insertIndex, lastGrade));

                    // Set the new last grade.
                    lastGrade = license.Grade;

                }
            }

            // The adder is needed because as we add these fake licenses, the indexes change.
            int adder = 0;

            // Now make the fake licenses.
            foreach (Tuple<int, int> info in indexesToAddFakeLicensesTo)
            {
                // Figure out what grade we are doing right now.
                int grade = info.Item2;

                /* Figure out the index of the last license of that grade, by adding the adder.
                 * The adder is needed because as we add these fake licenses, the indexes change.
                 */
                int index = info.Item1 + adder;

                // 8 licenses per page, so the 8 - the remainder is how many we need to make.
                int numLicensesToMake = (8 - (sortedLicenses.Count(license => license.Grade == grade) % 8)) % 8;

                // Add to the adder so that next time, the index is increased properly.
                adder += numLicensesToMake;

                // Create fake licenses and add them to the sorted licenses at the proper index (info.Item1)
                sortedLicenses.InsertRange(info.Item1, Create_Fake_Licenses(numLicensesToMake));
            }
            

            return sortedLicenses;
        }

        /// <summary>
        /// Creates the given number of empty, fake licenses.
        /// Fake licenses are used as a space filler 
        /// </summary>
        /// <param name="num">The number of fake licenses to generate</param>
        /// <returns>A list of the given number of fake licenses</returns>
        private License[] Create_Fake_Licenses(int num)
        {
            License[] fake_licenses = new License[num];

            for (int a = 0; a < num; a++)
            {
                fake_licenses.SetValue(new License(), a);
            }

            return fake_licenses;
        }

        /// <summary>
        /// Function that sends updates to the main thread whenever requested.
        /// </summary>
        /// <param name="percent">Decimal percent done</param>
        /// <param name="status">Integer status</param>
        /// <param name="fraction">Tuple (stuffDone/stuffToDo). Provide it when status = FILLING</param>
        /// <param name="path">The path at which the file was exported to temporarily.</param>
        private void SendUpdateMessage(double percent, int status, Tuple<int, int> fraction = null, string path = "")
        {
            string textMessage;

            if (status == GENERATING_LICENSE_OBJECTS)
            {
                textMessage = "Making License Objects";
            }
            else if (status == SETUP)
            {
                textMessage = "Getting Ready";
            }
            else if (status == CREATINGPAGES)
            {
                textMessage = "Creating pages";
            }
            else if (status == FILLING)
            {
                textMessage = "Filling form. License " + fraction.Item1.ToString() 
                    + "/" + fraction.Item2.ToString();
            }

            // If we're completed.
            else
            {
                double milisPassed = stopwatch.ElapsedMilliseconds;
                double seconds = milisPassed / 1000;
                stopwatch.Stop();

                textMessage = "Completed " + seconds.ToString() + " seconds.";
                path = OutputLocation;
            }

            PrintMessage message = new PrintMessage
            {
                Message = textMessage,
                Progress = percent,
                Path = path
            };

            lock(Key)
            {
                Print_Comms.Enqueue(message);
            }
        }
    }
}
