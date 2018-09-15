using System;
using System.Collections.Generic;
using System.Linq;
using Reindeer_Hunter.Subsystems.ProcessButtonCommands;
using System.Windows.Controls;
using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.Hunt;

namespace Reindeer_Hunter.Subsystems
{
    public class ProcessButtonSubsystem : Subsystem
    {
        public static ProcessButtonSubsystem CurrentInstance = null;

        public event EventHandler<NewMatches_EventArgs> MatchesMade;
        public event EventHandler MatchesDiscarded;
        public event EventHandler WentToFFA;

        // The commands that this subsystem is in charge of.
        public Process ProcessCommand { get; } = new Process();
        public RelayCommand GoToFFAOverrideCommand { get; } = new RelayCommand();

        // The GUI display objects that this subsystem is responsible for.
        private Button processButton;
        private ProgressBar progressBar;
        private TextBox messageDisplayBox; 

        public long CurrRoundNo { get; set; }

        // Status variable and on status chang event
        public event EventHandler StatusChanged;
        private int status = 0;
        public int Status {
            get
            {
                return status;
            }
            set
            {
                status = value;
                StatusChanged(this, new EventArgs());
            }
        }

        /*
         * Information set by the MatchMakeHandler when matches are generated.
         * This is required for when things are saved and both the matches and the
         * match number have to be saved.
         */
        private Tuple<long, List<Match>, string> generationInfo;
        public Tuple<long, List<Match>, string> GenerationInfo
        {
            get
            {
                return generationInfo;
            }

            set
            {
                generationInfo = value;
                NewMatches = value.Item2;
            }
        }

        private List<Match> newMatches = new List<Match>();
        /// <summary>
        /// Stores matches that were just created by the match maker.
        /// </summary>
        public List<Match> NewMatches {
        get
            {
                return newMatches;
            }
            set
            {
                newMatches = value;
                MatchesMade(this, new NewMatches_EventArgs(value));
            }
        }

        // True when matches were just made and haven't been saved, false otherwise.
        public bool AreMatchesMade { get
            {
                // This is true whenever the NewMatches list is not empty.
                return NewMatches.Count() > 0;
            } }

        // Constants for status
        public readonly int MATCHMAKING = 0;
        public readonly int INSTANTPRINT = 1;
        public readonly int FFA = 2;

        public ProcessButtonSubsystem() : base()
        {
            ProcessCommand.ProcessButtonSubsystem = this;
            StatusChanged += OnStatusChange;

            CurrentInstance = this;
        }


        protected override void OnHomePageSet(object sender, EventArgs e)
        {
            // Call the base function
            base.OnHomePageSet(sender, e);

            // Set the GUI element variables now that we have access
            processButton = Manager.Home.process_button;
            progressBar = Manager.Home.progressBar;
            messageDisplayBox = Manager.Home.progressDisplayBox;

            // Update current round number
            CurrRoundNo = school.GetCurrRoundNo();
            // Update Display Status
            UpdateStatus();

            // Subscribe to match change event and round increased event and save/discard events.
            school.MatchChangeEvent += (a, b) => OnMatchesChanged();
            school.RoundIncreased += (a, b) => OnMatchesChanged();
            school.StudentsImported += (a, b) => OnMatchesChanged();
            Manager._SaveDiscard.Save += OnSave;
            Manager._SaveDiscard.Discard += OnDiscard;

            // Set up the commands
            GoToFFAOverrideCommand.FunctionToExecute = (object parameter) => GoToFFA();
            GoToFFAOverrideCommand.CanExecuteDeterminer = () => school.NumInStudents > 0;
            // Make sure that the GoToFFAOverrideCommand updates when number of students change.
            school.MatchChangeEvent += (object EventSender, Match[] a) => GoToFFAOverrideCommand.RaiseCanExecuteChanged();
        }

        private void UpdateStatus()
        {
            if (school.IsTimeForFFA || school.IsFFARound) Status = FFA;
            else if (school.IsReadyForNextRound) Status = MATCHMAKING;
            else Status = INSTANTPRINT;
        }

        /// <summary>
        /// Function called when something about matches is changed and stuff
        /// will need updating.
        /// </summary>
        private void OnMatchesChanged()
        {
            // Update the round and status.
            CurrRoundNo = school.GetCurrRoundNo();
            UpdateStatus();
        }

        /// <summary>
        /// Handling for when the status changed event is raised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStatusChange(object sender, EventArgs e)
        {
            // All we need to do is update the text.
            if (Status == MATCHMAKING)
            {
                processButton.Content = string.Format("Matchmake R{0}", CurrRoundNo + 1);
            }
            // Status is otherwise instant print.
            else if (Status == INSTANTPRINT)
            {
                processButton.Content = "Instant Print";
            }
            // Status is otherwise FFA
            else
            {
                processButton.Content = "Go to FFA";
            }
        }

        /// <summary>
        /// Function to update the GUI elements on the progress of either the matchmaking
        /// operation or the instant print operation.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="progressPercent"></param>
        public void UpdateOperationStatus(string message, double progressPercent)
        {
            messageDisplayBox.Text = message;
            progressBar.Value = (int) Math.Round(progressPercent * 100);
        }

        /// <summary>
        /// Event that fires when new matches are saved.
        /// </summary>
        public event EventHandler<Match[]> MatchesRegistered;

        /// <summary>
        /// Function called by the Save event. 
        /// All it does is register the new matches and then clear the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSave(object sender, EventArgs e)
        {
            // If matches were indeed just made but not saved, add them.
            if (AreMatchesMade)
            {
                // Add the generated matches
                school.AddMatches(NewMatches);
                
                // Set the new current match number
                school.CurrMatchNo = GenerationInfo.Item1;
                school.RoundEndDate = GenerationInfo.Item3;

                NewMatches.Clear();
                // Fire the event.
                MatchesRegistered(this, NewMatches.ToArray());
            }
        }

        /// <summary>
        /// Called by the Discard event.
        /// Just clear the new matches list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDiscard(object sender, EventArgs e)
        {
            NewMatches.Clear();
            MatchesDiscarded(this, new EventArgs());
        }

        /// <summary>
        /// Function to set the page to the FFA page
        /// </summary>
        public void GoToFFA()
        {
            school.IsFFARound = true;
            ((StartupWindow)Manager.Home.Parent).GoToFFA();
            WentToFFA(this, new EventArgs());
            UpdateStatus();
        }
    }
}
