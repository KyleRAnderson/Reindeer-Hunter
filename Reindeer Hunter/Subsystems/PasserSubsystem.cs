using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Reindeer_Hunter.Subsystems
{
    /// <summary>
    /// Subsystem in charge of handling passing students (and closing matches)
    /// </summary>
    public class PasserSubsystem : Subsystem
    {
        /// <summary>
        /// Triggered when a result is removed.
        /// Sends a MatchGuiResult object as EventArgs.
        /// </summary>
        public event EventHandler ResultRemoved;

        /// <summary>
        /// Triggered when a result is added, by checking the checkbox.
        /// </summary>
        public event EventHandler ResultAdded;

        // List of MatchGuiResults containing the students that are currently set to pass.
        private List<MatchGuiResult> PassingStudents = new List<MatchGuiResult>();

        // The listbox where the passing students buttons are held.
        private System.Windows.Controls.ListBox PassingStudentsBox;
        public List<System.Windows.Controls.Button> PassingStudentsButtons { get; } =
            new List<System.Windows.Controls.Button>();

        /// <summary>
        /// If there are currently students selected to pass, this is true.
        /// </summary>
        public bool IsPassingStudents
        {
            get
            {
                return PassingStudents.Count() > 0;
            }
        }

        protected override void OnHomePageSet(object sender, EventArgs e)
        {
            base.OnHomePageSet(sender, e);

            // Declare the passing students box variable
            PassingStudentsBox = Manager.Home.PassingStudentsBox;


            // Subscribe to Save and Discard events.
            Manager._SaveDiscard.Save += OnSave;
            Manager._SaveDiscard.Discard += OnDiscard;

            // Subscribe to the DataGrid's selected cells changed event
            Manager.Home.MainDisplay.SelectedCellsChanged += SelectedCellsChanged;
        }

        /// <summary>
        /// Event fired once the passing students are saved.
        /// </summary>
        public event EventHandler PassingStudentsSaved;

        /// <summary>
        /// Function called whenever save button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSave(object sender, EventArgs e)
        {
            if (!IsPassingStudents) return;
            else
            {
                Manager._School.AddMatchResults(PassingStudents);
                // Fire event.
                PassingStudentsSaved(this, new EventArgs());
            }
            PassingStudents.Clear();
            PassingStudentsButtons.Clear();

            // Refresh the box
            PassingStudentsBox.Items.Refresh();
        }

        /// <summary>
        /// Function called when discard button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDiscard(object sender, EventArgs e)
        {
            PassingStudents.Clear();
            PassingStudentsButtons.Clear();

            // Refresh the box.
            PassingStudentsBox.Items.Refresh();
        }

        /* An integer used because, in the function below the stuff that we do
         * Calls the event again, and this would run everything infinitely, so we use a counter */
        private int SelectionCounter = 0;
        private void SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // The mainDisplay object that we look at.
            System.Windows.Controls.DataGrid mainDisplay = Manager.Home.MainDisplay;

            // Make sure it's the "pass" column. Otherwise, we don't care.
            if (mainDisplay.CurrentCell.Column.Header.ToString() != "Pass") return;

            /* This function is called twice because at the end
             * We clear the selectedobjects list of the maindisplay,
             * which re-triggers this function */
            SelectionCounter += 1;
            if (SelectionCounter % 2 == 0) return;

            // Actually time to do stuff.
            Match selectedMatch = (Match)mainDisplay.CurrentCell.Item;

            // If the match is closed, error
            if (selectedMatch.Closed)
            {
                System.Windows.Forms.MessageBox.Show("That match is closed " +
                    "and its status cannot be edited.", "Match Status error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // Pass matches or any other match to be ignored will have id2 of 0
            else if (selectedMatch.Id2 == 0)
            {
                System.Windows.Forms.MessageBox.Show("Cannot edit status of that match, " +
                    "it is a special match.", "Match Status error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // If matches haven't just been made by the matchmaker, proceed.
            else if (!Manager._ProcessButtonSubsystem.AreMatchesMade)
            {
                int studentId = 0;
                string name = "";
                // Actually update the match checkbox display
                if (mainDisplay.CurrentCell.Column.DisplayIndex == 0)
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
                mainDisplay.Items.Refresh();

                // Make sure we only do this for adding results.
                if (name != "" && studentId != 0)
                {
                    MatchGuiResult matchResult = new MatchGuiResult(name)
                    {
                        MatchID = selectedMatch.MatchId,
                        StuID = studentId,
                    };
                    matchResult.ResultButtonClick += RemoveResult;

                    // Add match result to the list
                    PassingStudents.Add(matchResult);

                    // Add the button created in the result object to the list and get its index.
                    PassingStudentsButtons.Add(matchResult.ResultButton);

                    // Refresh the display
                    PassingStudentsBox.Items.Refresh();

                    ResultAdded(this, new EventArgs());
                }
            }

            /* Clear the selection. When this happens, the SelectedCellsChanged event is called
            * again and so therefore we need the Selection counter to make 
            * sure we don't do all this twice.
            */
            mainDisplay.SelectedCells.Clear();
        }

        /// <summary>
        /// Function to remove the given result from the Passing Students lists.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void RemoveResult(object source, EventArgs e)
        {
            PassingStudentsButtons.Remove(((MatchGuiResult)source).ResultButton);
            PassingStudents.Remove((MatchGuiResult)source);

            // Refresh the items.
            PassingStudentsBox.Items.Refresh();

            ResultRemoved(this, (MatchGuiResult)source);
        }

        private void RemoveResult(int studentId)
        {
            foreach (MatchGuiResult result in PassingStudents)
            {
                if (result.StuID == studentId)
                {
                    PassingStudents.Remove(result);
                    PassingStudentsButtons.Remove(result.ResultButton);
                    ResultRemoved(this, result);
                    break;
                }
            }

            // Refresh the passing students box
            PassingStudentsBox.Items.Refresh();
        }
    }
}
