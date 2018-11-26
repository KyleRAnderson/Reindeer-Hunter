using System;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;
using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.Hunt;

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
        private string templateLocation;

        // Path location where filled file will be exported.
        private string outputLocation;

        // The end date to be put on the licenses.
        private string endDate;

        // Queue used for communication
        private Queue<PrintMessage> Print_Comms;

        // The URL of the form
        private string formURL = "";

        protected readonly object Key;

        private int StuffDone;
        private int StuffToDo;

        // The round number
        protected static long RoundNo;

        // Statuses
        public enum PrintStatus { Setup, CreatingPages, Filling, Completed, Generating_License_Object };
        public Stopwatch stopwatch;

        /// <summary>
        /// Whether or not the form url will be used.
        /// </summary>
        private bool UsingFormURL
        {
            get
            {
                return formURL != string.Empty;
            }
        }

        // Index number is the index value of the form field.
        public int IndexNo = 1;
        // Page number is the current page we're creating .
        public int PageNo = 0;

        public InstantPrinter(List<Match> matches,  
            long roundNo, object key, Queue<PrintMessage> comms, string DataPath, string endDate, string formURL)
        {
            // Set up file locations
            outputLocation = Path.Combine(DataPath, "FilledLicenses.tmp");
            templateLocation = Path.Combine(DataPath, DataFileIO.TemplatePDFName);


            Key = key;
            MatchList = matches;
            RoundNo = roundNo;
            Print_Comms = comms;
            this.endDate = endDate;
            this.formURL = formURL;
        }

        /// <summary>
        /// Copies the template PDF file (at the given file path) to the given stream.
        /// </summary>
        /// <param name="document">Document the document to use in copying the PDF file.</param>
        /// <param name="stream">The stream to copy the document to.</param>
        /// <param name="pagesNeeded">Number of pages needed total</param>
        /// <returns>The PdfSmartCopy object created.</returns>
        private PdfSmartCopy CopyDocument(Document document, Stream stream, int pagesNeeded)
        {
            PdfSmartCopy working_document = new PdfSmartCopy(document, stream);
            working_document.SetMergeFields();

            // Used this to close all the readers later
            PdfReader[] readers = new PdfReader[pagesNeeded];

            document.Open();

            for (int copier = 0; copier < pagesNeeded; copier++)
            {
                PdfReader pdfreader = RenamePDFFields(copier);
                working_document.AddDocument(pdfreader);
                readers[copier] = pdfreader;
            }

            document.Close();

            foreach (PdfReader readerToClose in readers) readerToClose.Close();

            return working_document;
        }
        
        /// <summary>
        /// Fills in all the PDF forms on the page as well as the QR codes if the form URL is set.
        /// </summary>
        /// <param name="document">The document to work with</param>
        /// <param name="memoryStream">The memory stream containing the copied document</param>
        /// <param name="licenses">The licenses to print.</param>
        private void FillFormFields(Document document, MemoryStream memoryStream, License[] licenses)
        {
            // Get ready to fill the forms.
            PdfReader reader = new PdfReader(memoryStream.ToArray());
            MemoryStream memStream = new MemoryStream();
            PdfStamper stamper = new PdfStamper(reader,
                new FileStream(outputLocation, FileMode.Create));
            AcroFields formFields = stamper.AcroFields;

            StuffDone = 0;
            StuffToDo = licenses.Length;

            // Loop through all matches and write the information
            for (int a = 0; a < StuffToDo; a++)
            {
                Tuple<int, int> fraction = new Tuple<int, int>(StuffDone, StuffToDo);
                double percent = (double)StuffDone / StuffToDo;
                SendUpdateMessage(percent, PrintStatus.Filling, fraction);

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

                student1path = string.Format("Student1_{0}P{1}", IndexNo, PageNo);
                student2path = string.Format("Student2_{0}P{1}", IndexNo, PageNo);
                roundpath = string.Format("Round_{0}P{1}", IndexNo, PageNo);
                datePath = string.Format("Date_{0}P{1}", IndexNo, PageNo);

                // Fill in the form fields.

                // Set the first student's info.
                formFields.SetField(student1path,
                    license.Student1Field);

                // Set the round number.
                formFields.SetField(roundpath, license.Round.ToString());

                // Set the date
                formFields.SetField(datePath, endDate);

                // Set the second student's info.
                formFields.SetField(student2path,
                    license.Student2Field);

                // Now deal with inserting the QR code, if it is possible

                if (UsingFormURL)
                {
                    // Since the formfield pages start at 0, and we want better than that.
                    int actualPageNo = formFields.GetFieldPositions(roundpath)[0].page + 1;

                    // Add a new, empty page, but only the first time.
                    if (IndexNo < 2) stamper.InsertPage(actualPageNo, PageSize.LETTER);

                    // Get the location of the QR.
                    AcroFields.FieldPosition pos = formFields.GetFieldPositions(datePath)[0];

                    // The approximate top and bottom of the license. Since this is the smaller size, set it to the QR size.
                    float topOfLicense = formFields.GetFieldPositions(student1path)[0].position.Top;
                    float bottomOfLicense = pos.position.Bottom;

                    int size = (int)Math.Round(topOfLicense - bottomOfLicense) + 50;

                    float pageWidth = reader.GetPageSize(actualPageNo).Width;

                    float posx;
                    float posy;

                    /* Reflect the positions over the middle of the page, so that the
                     * QR code is on the back of the proper license.
                     * 
                     * Essentially, in doing these calculations, we're moving the origin
                     * point from the bottom left of the page to the bottom right.
                     */
                    posx = pageWidth - pos.position.Right;
                    posy = pos.position.Bottom - 20;

                    // Make the QR code
                    Image qr = GenerateQRCode(license.First1, license.Last1, license.Homeroom1, license.Id1,
                        posx, posy, size, size);

                    // Add the QR code to the PDF.
                    stamper.GetOverContent(actualPageNo).AddImage(qr);
                }

                IndexNo += 1;

                StuffDone += 1;
            }
            stamper.FormFlattening = false;
            stamper.Close();
            reader.Close();
        }

        /// <summary>
        /// The function responsible for completely filling the PDF
        /// </summary>
        public void Print()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            SendUpdateMessage(0, status: PrintStatus.Generating_License_Object);

            // Make license objects.
            License[] licenses = Generate_License_Objects(MatchList, endDate).ToArray();

            SendUpdateMessage(0, status: PrintStatus.Setup);

            /* 
             * Determine how many pages will be necessary. 
             * Keep in mind that 8 licenses can be fit on each page.
             * So, we divide the license number by 8.
             */
            int pagesNeeded = (int)Math.Ceiling((double)licenses.Length / 8);

            // Duplicate as many pages as is necessary
            MemoryStream memoryStream = new MemoryStream();
            Document document = new Document();
            // Copy the document from storage.
            PdfSmartCopy working_document = CopyDocument(document, memoryStream, pagesNeeded);

            FillFormFields(document, memoryStream, licenses);

            SendUpdateMessage(1, PrintStatus.Completed);
        }

        /// <summary>
        /// Renames all the form fields in this duplicated PDF so that the form field names don't conflict with each other when
        /// they are all added together.
        /// </summary>
        /// <param name="templateLocation">The string location of the template PDF file.</param>
        /// <param name="pageNo">The page number of the first page of this new PDF.</param>
        /// <returns>The PdfReader object used to read the new PDF.</returns>
        private PdfReader RenamePDFFields(int pageNo)
        {
            MemoryStream memoryStream = new MemoryStream();
            PdfReader reader = new PdfReader(templateLocation);
            PdfStamper stamper = new PdfStamper(reader, memoryStream);
            AcroFields forms = stamper.AcroFields;
            HashSet<string> keys = new HashSet<string>(forms.Fields.Keys);

            foreach (string formKey in keys)
            {
                forms.RenameField(formKey, string.Format("{0}P{1}", formKey, pageNo));
            }

            stamper.Close();
            reader.Close();

            return new PdfReader(memoryStream.ToArray());
        }

        private Image GenerateQRCode(string student1first, string student1last, int student1_homeroom, string student1id, 
            float posx, float posy, int width, int height)
        {
            Image qr = null;

            if (!string.IsNullOrEmpty(formURL))
            {
                // Generate the proper url. TODO make URL easy to change.
                string url = string.Format(formURL, student1first, student1last, student1_homeroom, student1id);

                BarcodeQRCode qRCode = new BarcodeQRCode(url, width, height, null);
                qr = qRCode.GetImage();
                qr.SetAbsolutePosition(posx, posy); 
            }

            return qr;
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
        /// Gets the grade of this license.
        /// </summary>
        /// <param name="license">The license to get the grade of.</param>
        /// <returns>The license's grade.</returns>
        private static int GetGrade(License license)
        {
            // TODO this is kinda not good. Shouldn't be determining grade by homeroom number. Could use student's grade but that breaks it so don't (students doing reach-ahead, etc will mess things up if we did).
            return license.Homeroom1 / 100;
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

            int lastGrade = GetGrade(sortedLicenses[0]);
            foreach (License license in sortedLicenses)
            {
                int licenseGrade = GetGrade(license);
                if (licenseGrade != lastGrade && licenseGrade != 0)
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
        private void SendUpdateMessage(double percent, PrintStatus status, Tuple<int, int> fraction = null, string path = "")
        {
            string textMessage;

            if (status == PrintStatus.Generating_License_Object)
            {
                textMessage = "Making License Objects";
            }
            else if (status == PrintStatus.Setup)
            {
                textMessage = "Getting Ready";
            }
            else if (status == PrintStatus.CreatingPages)
            {
                textMessage = "Creating pages";
            }
            else if (status == PrintStatus.Filling)
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
                path = outputLocation;
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
