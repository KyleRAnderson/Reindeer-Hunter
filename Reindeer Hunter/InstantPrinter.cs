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

            SendUpdateMessage(0, status: SETUP);

            /* Determine how many pages will be necessary. 
             * Keep in mind that 4 matches can be fit on each page,
             * Two of every match
             */
            int pagesNeeded = (int)Math.Ceiling(MatchList.Count / 4.0);

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
            StuffToDo = MatchList.Count();

            // Loop through all matches and write the information
            for (int a = 0; a < StuffToDo; a++)
            {
                Tuple<int, int> fraction = new Tuple<int, int>(StuffDone, StuffToDo);
                double percent = (double) StuffDone / StuffToDo;
                SendUpdateMessage(percent, FILLING, fraction);

                Match match = MatchList[a];

                // There are eight of the same textboxes a page, so after that increase page no.
                if (IndexNo > 8)
                {
                    IndexNo = 1;
                    PageNo += 1;
                }

                // Write things twice per match.
                for (int b = 0; b < 2; b++)
                {
                    string student1path;
                    string student2path;
                    string roundpath;
                    string datePath;

                    // If it's the match of the page, there's no underscore and id number
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
                        string.Format("{0} ({1})", match.FullName1, match.Home1));

                    // Set the round number.
                    formFields.SetField(roundpath, match.Round.ToString());

                    // Set the date
                    formFields.SetField(datePath, EndDate);

                    // Since the passing match has id2 of 0, don't do this for a pass match
                    if (match.Id2 != 0)
                    {
                        formFields.SetField(student2path,
                            string.Format("{0} ({1})", match.FullName2, match.Home2));
                    }
                    else
                    {
                        formFields.SetField(student2path, "Pass to next round");
                    }

                    IndexNo += 1;


                }

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
        /// Function that sends updates to the main thread whenever requested.
        /// </summary>
        /// <param name="percent">Decimal percent done</param>
        /// <param name="status">Integer status</param>
        /// <param name="fraction">Tuple (stuffDone/stuffToDo). Provide it when status = FILLING</param>
        /// <param name="path">The path at which the file was exported to temporarily.</param>
        private void SendUpdateMessage(double percent, int status, Tuple<int, int> fraction = null, string path = "")
        {
            string textMessage;

            if (status == SETUP)
            {
                textMessage = "Setup operations.";
            }
            else if (status == CREATINGPAGES)
            {
                textMessage = "Creating pages";
            }
            else if (status == FILLING)
            {
                textMessage = "Filling form. Match " + fraction.Item1.ToString() 
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
