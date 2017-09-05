using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : System.Windows.Controls.UserControl
    {
        // Lock object for queues
        protected readonly object Key = new object();


        // The thread used to create the matches.
        protected static System.Threading.Thread printing_thread;

        // The master window upon which this content will be displayed.
        protected static StartupWindow MasterWindow;

        /* Used to communicate between the main thread (this thread)
         * and the match creator thread
         */
        protected static System.Collections.Generic.Queue<Reindeer_Hunter.Message> comms;

        /* Used to communicate between the main thread (this thread)
         * and the instant printer thread
         */
        protected static System.Collections.Generic.Queue<Reindeer_Hunter.PrintMessage> comms_print;

        /* True when matches were just made so that we know when 
         * to add matches vs when to save changes
         */
        public bool MatchesMade { get; set; } = false;

        // List of all MatchGuiResult objects created after user passes someone
        protected static List<MatchGuiResult> MatchResultsList = new List<MatchGuiResult>();
        protected static List<System.Windows.Controls.Button> MatchResultButtonList 
            = new List<System.Windows.Controls.Button>();

        // See "MainDisplay_SelectedCellsChanged" method for explanation of this.
        private int SelectionCounter = 0;

        public HomePage(StartupWindow mainWindow)
        {
            MasterWindow = mainWindow; 
            InitializeComponent();
            ReloadItemsSource();

            MasterWindow.GetSchool().MatchChangeEvent += OnMatchChangeEvent;
            UpdateMatchmakeButton();

            EnableDisableMatcmakeButton(MasterWindow.GetSchool().IsReadyForNextRound());
            PassingStudentsBox.ItemsSource = MatchResultButtonList;
        }

        protected virtual void OnMatchChangeEvent(object source, EventArgs e)
        {
            ReloadItemsSource();
            bool ReadyForNextRound = MasterWindow.GetSchool().IsReadyForNextRound();
            EnableDisableMatcmakeButton(ReadyForNextRound);
        }

        /// <summary>
        /// Updates the matchmake button with the new round number
        /// </summary>
        private void UpdateMatchmakeButton()
        {
            string round = ((long)(MasterWindow.GetSchool().GetCurrRoundNo() + 1)).ToString();
            process_button.Content = "Matchmake R" + round;
        }

        /// <summary>
        /// Enable or disable the matchmake button
        /// </summary>
        /// <param name="enable">Boolean representing what to do</param>
        private void EnableDisableMatcmakeButton(bool enable)
        {
            if (enable) UpdateMatchmakeButton();
            else
            {
                process_button.Content = "Instant Print";
            }
        }

        /// <summary>
        /// Stuff that has to happen when matches are created.
        /// </summary>
        private void OnMatchesCreate()
        {
            // Update state and enable saving and discarding
            SetMatchesMade(true);
        }

        /// <summary>
        /// Sets the matches made boolean and deals with anything else that has to happen
        /// </summary>
        /// <param name="matchesMade"></param>
        private void SetMatchesMade(bool matchesMade)
        {
            MatchesMade = matchesMade;

            // Disable result inputting just after matches have been made.
            Import_Match_ResultsButton.IsEnabled = !matchesMade;
            if (matchesMade)
            {
                EnableSaveDiscardButtons();
            }
        }

        /// <summary>
        /// Function called when it is time to create matches.
        /// </summary>
        private void Matchmake()
        {
            // Create the queue for communications purposes.
            comms = new Queue<Message>();

            // Instantiate school object for simplicity
            School school = MasterWindow.GetSchool();

            // Create the matchmaker and then assign the thread target to it
            // +1 to current round because we want next round's matches.
            Matcher matcher;
            if (!school.IsCombineTime())
            {
                matcher = new Matcher(school.GetCurrRoundNo() + 1, school.GetCurrMatchNo(), Key,
                    comms, studentsDic: school.GetStudentsByGrade());
            }
            else
            {
                matcher = new Matcher(school.GetCurrRoundNo() + 1, school.GetCurrMatchNo(), Key,
                    comms, studentList: school.GetAllParticipatingStudents());
            }

            printing_thread = new System.Threading.Thread(matcher.Generate)
            {
                Name = "Matchmaker"
            };
            printing_thread.Start();

            // Put the execute function into the mainloop to be executed
            CompositionTarget.Rendering += Execute_Matchmaking;
        }

        /// <summary>
        /// Called by event handler when the main process button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((string)process_button.Content != "Instant Print") Matchmake();
            else InstantPrint();
        }

        private void InstantPrint()
        {
            // Create the queue for communications purposes.
            comms_print = new Queue<PrintMessage>();

            // Instantiate school object for simplicity
            School school = MasterWindow.GetSchool();

            // Create the matchmaker and then assign the thread target to it
            // +1 to current round because we want next round's matches.
            InstantPrinter printer;
            try
            {
                printer = new InstantPrinter(school.GetCurrRoundMatches(),
                school.GetCurrRoundNo(), Key, comms_print);
            }
            catch (IOException)
            {
                return;
            }
            

            printing_thread = new System.Threading.Thread(printer.Print)
            {
                Name = "Instant Printer"
            };
            printing_thread.Start();

            // Put the execute function into the mainloop to be executed
            CompositionTarget.Rendering += Execute_Printing;
        }

        private void Execute_Printing(object sender, EventArgs e)
        {
            lock(Key)
            {
                if (comms_print.Count() <= 0) return;

                // Convert queue to list and retrieve last value
                List<PrintMessage> returnList = comms_print.ToList<PrintMessage>();
                PrintMessage returnValue = returnList[returnList.Count() - 1];

                // Clear queue
                comms_print.Clear();

                // Update progress text
                progressDisplayBox.Text = returnValue.Message;

                // Update the progressbar
                progressBar.Value = returnValue.Progress;

                // Progress is 100 % when complete
                if (returnValue.Progress == 1)
                {
                    // Give the second thread the info it needs to move the file
                    string path;
                    SaveFileDialog fileDialog = new SaveFileDialog
                    {
                        // Open the file dialog to the user's directory
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                        // Filter only for comma-seperated value files. 
                        Filter = "pdf files (*.pdf)|*.pdf",
                        FilterIndex = 2,
                        RestoreDirectory = true
                    };

                    fileDialog.ShowDialog();

                    if (fileDialog.FileName == "")
                    {
                        path = System.IO.Path.Combine(Environment.GetFolderPath(
                            Environment.SpecialFolder.Desktop), "FilledLicenses.pdf");

                        System.Windows.Forms.MessageBox.Show("Export location Error. File was outputted to "
                            + path, "Export Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else path = fileDialog.FileName;

                    // Join the thread
                    printing_thread.Join();

                    // Unsubscribe from event.
                    CompositionTarget.Rendering -= Execute_Printing;

                    // In case the user tries to overwrite another file
                    if (File.Exists(path)) File.Delete(path);

                    // Move the temporary file out of the code's folder.
                    File.Move(returnValue.Path, path);

                    System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(path));
                }
            }
        }

        /// <summary>
        /// This is the process that will monitor the matchmaking process
        /// and update the gui.
        /// </summary>
        private void Execute_Matchmaking(object sender, EventArgs e)
        {
            // Lock it so we don't get problems.
            lock(Key)
            {
                // Don't do anything if no new data has been sent. 
                if (comms.Count() <= 0) return;

                // Convert queue to list and retrieve last value
                List<Message> returnList = comms.ToList<Message>();
                Message returnValue = returnList[returnList.Count() - 1];

                // Clear queue
                comms.Clear();

                // Update the message on the text box.
                progressDisplayBox.Text = returnValue.MessageText;

                // Update the progressBar
                progressBar.Value = returnValue.ProgressDecimal;

                if (returnValue.Matches != null)
                {
                    //  Terminate the thread
                    printing_thread.Join();

                    CompositionTarget.Rendering -= Execute_Matchmaking;
                    MainDisplay.ItemsSource = returnValue.Matches;
                    OnMatchesCreate();
                }
            }
        }

        private void EnableSaveDiscardButtons()
        {
            SaveButton.IsEnabled = true;
            DiscardButton.IsEnabled = true;
        }

        private void DisableSaveDiscardButtons()
        {
            SaveButton.IsEnabled = false;
            DiscardButton.IsEnabled = false;
        }

        private void Search_Box_GotFocus(object sender, RoutedEventArgs e)
        {
            search_box.Clear();
        }

        private void MainDisplay_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            // TODO insert the code for looking at what they clicked and then search it and display it in the data popup.
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            School school = MasterWindow.GetSchool();
            if (MatchesMade)
            {
                school.IncreaseCurrRoundNo();
                school.AddMatches((List<Match>)MainDisplay.ItemsSource);
                SetMatchesMade(false);

            }
            else
            {
                MasterWindow.GetSchool().AddMatchResults(MatchResultsList);
                RemoveAllResults();
            }
            DisableSaveDiscardButtons();
            MatchResultsList.Clear();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            if (MatchesMade)
            {
                ReloadItemsSource();
                SetMatchesMade(false);
            }
            DisableSaveDiscardButtons();
            RemoveAllResults();
        }

        /// <summary>
        /// Loads the match list and displays the one on file, replacing whatever's on there now.
        /// </summary>
        private void ReloadItemsSource()
        {
            MainDisplay.ItemsSource = MasterWindow.GetSchool().GetOpenMatchesList();
        }

        private void MainDisplay_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Get the match 
            Match row = (Match)MainDisplay.SelectedItems[0];
            bool validMatch = MasterWindow.GetSchool().MatchIsOpen(row);

            // If the match is closed, error
            if (!validMatch)
            {
                System.Windows.Forms.MessageBox.Show("That match is closed " +
                    "and its status cannot be edited.", "Match Status error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ReloadItemsSource();
                return;
            }
        }

        private void ComboBox_ImportStudentButton (object semder, EventArgs e)
        {
            MasterWindow.ImportStudents();
        }

        private void MainDisplay_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // Make sure it's the "pass" column. Otherwise, we don't care.
            if (MainDisplay.CurrentCell.Column.Header.ToString() != "Pass") return;

            /* This function is called twice because at the end
             * We clear the selectedobjects list of the maindisplay,
             * which re-triggers this function */
            SelectionCounter += 1;
            if (SelectionCounter % 2 == 0) return;

            // Actually time to do stuff.
            Match selectedMatch = (Match)MainDisplay.CurrentCell.Item;

            // If the match is closed, error
            if (selectedMatch.Closed)
            {
                System.Windows.Forms.MessageBox.Show("That match is closed " +
                    "and its status cannot be edited.", "Match Status error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!MatchesMade)
            {
                int studentId = 0;
                string name = "";
                // Actually update the match checkbox display
                if (MainDisplay.CurrentCell.Column.DisplayIndex == 0)
                {
                    /* If it is now false again (say the user pressed the checkbox twice)
                     * remove it from the list
                     */
                    if (selectedMatch.Pass1) RemoveResult(selectedMatch.Id1);
                    else
                    {
                        // In this case, pass was set to true to pass that student
                        selectedMatch.Pass1 = !selectedMatch.Pass1;
                        studentId = selectedMatch.Id1;
                        name = selectedMatch.First1 + " " + selectedMatch.Last1;
                    }
                }

                else
                {
                    /* If it is now false again (say the user pressed the checkbox twice)
                     * remove it from the list
                     */
                    if (selectedMatch.Pass2) RemoveResult(selectedMatch.Id2);
                    else
                    {
                        selectedMatch.Pass2 = !selectedMatch.Pass2;
                        studentId = selectedMatch.Id2;
                        name = selectedMatch.First2 + " " + selectedMatch.Last2;
                    }
                }

                // Make sure we only do this for adding results.
                if (name != "" && studentId != 0)
                {
                    MatchGuiResult matchResult = new MatchGuiResult(name)
                    {
                        MatchID = selectedMatch.MatchId,
                        StuID = studentId,
                        Home = this
                    };

                    // Add match result to the list
                    MatchResultsList.Add(matchResult);

                    // Add the button created in the result object to the list and get its index.
                    MatchResultButtonList.Add(matchResult.ResultButton);

                    // Refresh the display
                    PassingStudentsBox.Items.Refresh();
                }
            }
            //  Refresh the DataGrid
            MainDisplay.Items.Refresh();
            if (MatchResultsList.Count() > 0) EnableSaveDiscardButtons();

            /* Clear the selection. When this happens, the SelectedCellsChanged event is called
            * again and so therefore we need the Selection counter to make 
            * sure we don't do all this twice.
            */
            MainDisplay.SelectedCells.Clear();
        }
        
        /// <summary>
        /// Used to remove a match result from the list.
        /// </summary>
        /// <param name="stuId">The student id of the match result.</param>
        internal void RemoveResult(int stuId)
        {
            foreach (MatchGuiResult matchResult in MatchResultsList)
            {
                // Find the correct match result and remove it.
                if (matchResult.StuID == stuId)
                {
                    // Remove the match result object from the list
                    MatchResultsList.Remove(matchResult);

                    // Remove the button from the list
                    MatchResultButtonList.Remove(matchResult.ResultButton);

                    // Set the new pass variable of the match, if it is in the DataGrid
                    foreach (Match match in MainDisplay.ItemsSource)
                    {
                        if (match.MatchId == matchResult.MatchID)
                        {
                            if (matchResult.StuID == match.Id1) match.Pass1 = !match.Pass1;
                            else match.Pass2 = !match.Pass2;
                            break;
                        }
                    }

                    // Refresh everything
                    PassingStudentsBox.Items.Refresh();
                    MainDisplay.Items.Refresh();
                    break;
                }
            }
            if (MatchResultsList.Count() == 0) DisableSaveDiscardButtons();
        }

        internal void RemoveAllResults()
        {
            // Reset the display to all open matche
            ReloadItemsSource();
            MatchResultsList.Clear();
            MatchResultButtonList.Clear();
            PassingStudentsBox.Items.Refresh();

        }

        private void Import_Match_ResultsButton_Click(object sender, RoutedEventArgs e)
        {
            List<ResultStudent> results = new List<ResultStudent>();
            ResultStudent[] inputtedResults = (ResultStudent[])MasterWindow.GetImporter().Import(1);

            // In case of any import errors.
            if (inputtedResults == null) return;

            foreach (ResultStudent student in inputtedResults)
            {
                results.Add(student);
            }

            MasterWindow.GetSchool().AddMatchResults(results);
        }

        private void Search_Button_Click(object sender, EventArgs e)
        {

        }
    }
}
