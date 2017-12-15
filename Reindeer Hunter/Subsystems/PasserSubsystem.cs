using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.Subsystems.Passer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Reindeer_Hunter.Subsystems
{
    /// <summary>
    /// Subsystem in charge of handling passing students (and closing matches)
    /// </summary>
    public class PasserSubsystem : Subsystem
    {
        //  The statuses
        public static readonly int DOING_NOTHING = 0;
        public static readonly int PASSING_STUDENTS = 1;
        public static readonly int HANDLING_MATCHES = 2;

        /// <summary>
        /// The current state of the subsystem, whether we're about to pass students, or
        /// we're dealing with matches.
        /// </summary>
        public int Status
        {
            get
            {
                if (PassingStudents.Count > 0) return PASSING_STUDENTS;
                else if (MatchEditQueue.Count > 0) return HANDLING_MATCHES;
                else return DOING_NOTHING;
            }
        }

        /// <summary>
        /// Triggered when a result is removed.
        /// Sends a MatchGuiResult object as EventArgs.
        /// </summary>
        public event EventHandler ResultRemoved;

        /// <summary>
        /// Triggered when a result is added, by checking the checkbox.
        /// </summary>
        public event EventHandler ResultAdded;

        /// <summary>
        /// Triggered when a match is removed from the match edit queue.
        /// </summary>
        public event EventHandler MatchRemoved;

        /// <summary>
        /// Triggered when a match is added to the match edit queue
        /// </summary>
        public event EventHandler MatchAdded;

        public RelayCommand ClearQueueCommand { get; } = new RelayCommand();

        // List of MatchGuiResults containing the students that are currently set to pass.
        private Dictionary<int, MatchGuiResult> PassingStudents = new Dictionary<int, MatchGuiResult>();

        // List of Matches To Edit
        private Dictionary<string, MatchButton> MatchEditQueue { get; set; } = new Dictionary<string, MatchButton>();

        /// <summary>
        /// The matches that have been put in the edit queue
        /// by the user for editing.
        /// </summary>
        public List<Match> EditQueue
        {
            get
            {
                return MatchEditQueue
                    .Values
                    .ToList()
                    .Select(matchButton => matchButton.ToMatch())
                    .ToList();
            }
        }

        // The listbox where the passing students buttons are held.
        private ListBox DisplayBox;
        public List<Button> DisplayButtons { get; } =
            new List<Button>();

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
            DisplayBox = Manager.Home.ListDisplayBox;

            #region Setting up RelayCommands
            // Set up the Clear command.
            ClearQueueCommand.CanExecuteDeterminer = CanClearMatchEditQueue;
            ClearQueueCommand.FunctionToExecute = ClearMatchEditQueueRelay;
            #endregion

            #region Subscribing to events
            // Subscribe to Save and Discard events.
            Manager._SaveDiscard.Save += OnSave;
            Manager._SaveDiscard.Discard += OnDiscard;

            // Subscribe to the pass student event
            Manager._FiltersAndSearch.StudentAddedToPassQueue += OnStudentAddedToPassQueue;

            // Subscribe to the adding match to queue event
            Manager._FiltersAndSearch.MatchAddedToQueue += AddMatchToQueue;

            // Subscribe to the closing matches event, to be able to clear queue
            Manager._Tools.SelectedMatchesClosed += ClearMatchEditQueue;
            Manager._Tools.MatchesEdited += ClearMatchEditQueue;
            #endregion
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
                Manager._School.AddMatchResults(PassingStudents.Values.ToList());
                // Fire event.
                PassingStudentsSaved(this, new EventArgs());
            }
            PassingStudents.Clear();
            DisplayButtons.Clear();

            // Refresh
            Refresh();
        }

        /// <summary>
        /// Function called when discard button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDiscard(object sender, EventArgs e)
        {
            PassingStudents.Clear();
            DisplayButtons.Clear();

            // Refresh
            Refresh();
        }


        private void OnStudentAddedToPassQueue(object sender, Tuple<Student, Match> tuple)
        {
            Match selectedMatch = tuple.Item2;
            Student changedStudent = tuple.Item1;

            if (Status != DOING_NOTHING && Status != PASSING_STUDENTS) return;

            // If the match is closed, error
            if (selectedMatch.Closed)
            {
                MessageBox.Show("That match is closed " +
                    "and its status cannot be edited.", "Match Status error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Pass matches or any other match to be ignored will have id2 of 0
            else if (selectedMatch.Id2 == 0)
            {
                MessageBox.Show("Cannot edit status of that match, " +
                    "it is a special match.", "Match Status error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // If matches have just been made and the user attempts to pass someone, warn the user.
            else if (Manager._ProcessButtonSubsystem.AreMatchesMade)
            {
                MessageBox.Show("Cannot edit status of that match, " +
                   "generated matches haven't been saved.", "Matches not Saved",
                   MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // If matches haven't just been made by the matchmaker, proceed.
            else if (!Manager._ProcessButtonSubsystem.AreMatchesMade)
            {
                int studentId = 0;
                string name = "";
                // Actually update the match checkbox display
                if (changedStudent.Id == selectedMatch.Id1)
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
                        name = selectedMatch.FullName1;
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
                        name = selectedMatch.FullName2;
                    }
                }

                Manager._FiltersAndSearch.RefreshDisplay();

                // Make sure we only do this for proper matches.
                if (name != "" && studentId != 0)
                {
                    MatchGuiResult matchResult = new MatchGuiResult(name)
                    {
                        MatchID = selectedMatch.MatchId,
                        StuID = studentId,
                    };
                    matchResult.ResultButtonClick += RemoveResult;

                    // Add match result to the list
                    PassingStudents.Add(matchResult.StuID, matchResult);

                    // Add the button created in the result object to the list and get its index.
                    DisplayButtons.Add(matchResult.ResultButton);

                    // Refresh the display
                    Refresh();

                    ResultAdded(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Function to remove the given result from the Passing Students
        /// lists given the MatchGuiResult object
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void RemoveResult(object sender, EventArgs e)
        {
            DisplayButtons.Remove(((MatchGuiResult)sender).ResultButton);
            PassingStudents.Remove(((MatchGuiResult)sender).StuID);

            // Refresh the items.
            Refresh();

            ResultRemoved(this, (MatchGuiResult)sender);
        }

        /// <summary>
        /// Function to remove the given result from the Passing Students
        /// lists given the student ID associated with the result.
        /// </summary>
        /// <param name="studentId"></param>
        private void RemoveResult(int studentId)
        {
            RemoveResult(PassingStudents[studentId], new EventArgs());
        }

        /// <summary>
        /// Determines whether or not the user should be allowed to edit matches.
        /// </summary>
        /// <returns></returns>
        private bool CanEditMatches()
        {
            /* Can only edit matches if the status is correct, 
             * which only happens when there are matches in the list.
             */ 
            return ( Status == HANDLING_MATCHES);
        }

        private void EditMatches(object parameter)
        {
            // Make sure passing students list is cleared for this.
            PassingStudents.Clear();
        }

        private void AddMatchToQueue(object sender, Match match)
        {
            // If we're not in the right status, return.
            if ((Status != DOING_NOTHING && Status != HANDLING_MATCHES) || match.MatchId == null ||
                MatchEditQueue.ContainsKey(match.MatchId) || _School.GetCurrRoundNo() == 0 || !_School.IsValidMatch(match)) return;

            MatchButton matchButton = new MatchButton(match);

            DisplayButtons.Add(matchButton._Button);
            MatchEditQueue.Add(matchButton.MatchId, matchButton);

            matchButton.ButtonClicked += RemoveMatchFromQueue;

            Refresh();

            MatchAdded?.Invoke(this, new EventArgs());
        }

        private void RemoveMatchFromQueue(object sender, MatchButton e)
        {
            DisplayButtons.Remove(e._Button);
            MatchEditQueue.Remove(e.MatchId);

            Refresh();
            MatchRemoved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Deermines whether or not clearing of the match edit queue
        /// is allowed.
        /// </summary>
        /// <returns></returns>
        private bool CanClearMatchEditQueue()
        {
            return Status == HANDLING_MATCHES;
        }

        /// <summary>
        /// Just a way to call the ClearMatchEditQueue function from a RelayCommand.
        /// </summary>
        /// <param name="parameter"></param>
        private void ClearMatchEditQueueRelay(object parameter)
        {
            ClearMatchEditQueue();
        }
        /// <summary>
        /// Clears the match edit queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearMatchEditQueue(object parameter = null, EventArgs e = null)
        {
            // Just clear and refresh
            MatchEditQueue.Clear();
            DisplayButtons.Clear();

            Refresh();
        }

        /// <summary>
        /// Refresh the DisplayBox
        /// </summary>
        private void Refresh()
        {
            DisplayBox.Items.Refresh();
            ClearQueueCommand.RaiseCanExecuteChanged();
        }
    }
}
