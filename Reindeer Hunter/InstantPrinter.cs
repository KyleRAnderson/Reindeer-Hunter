using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using Reindeer_Hunter.Data_Classes;

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
        protected static string TemplateLocation;

        // Path where the duplicated file will be exported.
        protected static string TempLocation = "Duplicate.pdf";

        // Path location where filled file will be exported.
        protected static string OutputLocation = "FilledLicenses.pdf";

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

        // TODO document
        public int IndexNo = 1;
        public int PageNo = 0;

        public InstantPrinter(List<Match> matches,  
            long roundNo, object key, Queue<PrintMessage> comms)
        {
            Key = key;
            MatchList = matches;
            RoundNo = roundNo;
            Print_Comms = comms;
            // Place where the template should always be.
            TemplateLocation = "TemplatePDF.pdf";

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
            int pagesNeeded = (int)Math.Ceiling(MatchList.Count() / 4.0);

            // Duplicate as many pages as is necessary
            Document document = new Document();
            PdfCopy copy = new PdfSmartCopy(document, new FileStream(
                TempLocation, FileMode.Create));

            // Used this to close all the readers later
            List<PdfReader> readers = new List<PdfReader>();

            copy.Open();
            copy.SetMergeFields();
            document.Open();
                       

            // TODO put in actual numbers.
            for (int copier = 0; copier < pagesNeeded; copier++)
            {
                PdfReader pdfreader = RenamePDFFields(TemplateLocation, "Temporary.pdf", copier);
                copy.AddDocument(pdfreader);
                readers.Add(pdfreader);
                
                File.Delete("Temporary.pdf");
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
                SendUpdateMessage(StuffDone/StuffToDo, FILLING, fraction);

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

                    if (IndexNo < 2)
                    {
                        student1path = "Student1P" + PageNo.ToString();
                        student2path = "Student2P" + PageNo.ToString();
                        roundpath = "RoundP" + PageNo.ToString();
                    }
                    else
                    {
                        student1path = "Student1_" + IndexNo.ToString() + "P" + PageNo.ToString();
                        student2path = "Student2_" + IndexNo.ToString() + "P" + PageNo.ToString();
                        roundpath = "Round_" + IndexNo.ToString() + "P" + PageNo.ToString();
                    }
                    
                    // Fill in the form fields.
                    formFields.SetField(student1path, match.First1 + " " +
                        match.Last1 + " (" + match.Home1.ToString() + ")");

                    // Since the passing match has id2 of 0.
                    if (match.Id2 != 0)
                    {
                        formFields.SetField(student2path, match.First2 + " " +
                        match.Last2 + " (" + match.Home2.ToString() + ")");
                        formFields.SetField(roundpath, match.Round.ToString());

                        IndexNo += 1;
                    }
                    else
                    {
                        formFields.SetField(student2path, "Pass to next round");
                        formFields.SetField(roundpath, match.Round.ToString());

                        IndexNo += 1;
                    }
                    

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
