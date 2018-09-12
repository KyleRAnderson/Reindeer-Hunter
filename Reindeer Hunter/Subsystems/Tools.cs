using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.Subsystems.ToolsCommands.Editor;
using Reindeer_Hunter.ThreadMonitors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Subsystems
{
    public class Tools : Subsystem
    {
        /// <summary>
        /// Fired when the matches in the queue have been closed.
        /// </summary>
        public event EventHandler SelectedMatchesClosed;

        /// <summary>
        /// Fired after matches are edited.
        /// </summary>
        public event EventHandler MatchesEdited;

        public RelayCommand CloseAll { get; } = new RelayCommand();
        public RelayCommand CloseSelected { get; } = new RelayCommand();
        public RelayCommand EditMatchesCommand { get; } = new RelayCommand();
        public RelayCommand PrintSelected { get; } = new RelayCommand();

        protected override void OnHomePageSet(object sender, EventArgs e)
        {
            base.OnHomePageSet(sender, e);

            #region Setting up RelayCommands
            // Close all matches relaycommand
            CloseAll.FunctionToExecute = CloseAllMatches;
            CloseAll.CanExecuteDeterminer = CanCloseAll;

            // Close selected matches relaycommand
            CloseSelected.FunctionToExecute = CloseSelectedMatches;
            CloseSelected.CanExecuteDeterminer = MatchEditQueueHasMatches;

            // Instant Print Selected relaycommand
            PrintSelected.FunctionToExecute = Print;
            PrintSelected.CanExecuteDeterminer = MatchEditQueueHasMatches;

            // EditMatches relaycommand
            EditMatchesCommand.FunctionToExecute = EditMatches;
            EditMatchesCommand.CanExecuteDeterminer = MatchEditQueueHasMatches;
            #endregion

            // Refresh and update the commands' statuses
            Refresh();

            #region Subscribing to the right events
            Manager._Passer.MatchAdded += Refresh;
            Manager._Passer.MatchRemoved += Refresh;
            school.MatchChangeEvent += Refresh;
            #endregion
        }

        private void EditMatches(object parameter)
        {
            List<Match> matchesToEdit = Manager._Passer.EditQueue;

            new MatchEditor(school, matchesToEdit).ShowDialog();

            MatchesEdited?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Determines whether or not the match
        /// queue has matches in it. 
        /// This is necessary for many relaycommands to determine if they can run/
        /// </summary>
        /// <returns></returns>
        private bool MatchEditQueueHasMatches()
        {
            return Manager._Passer.Status == PasserSubsystem.PasserStatus.Handling_Matches;
        }

        /// <summary>
        /// The function called by the relaycommand in order to print selected.
        /// </summary>
        /// <param name="obj"></param>
        private void Print(object parameter)
        {
            // Get the matches that need printing.
            List<Match> matchesToPrint = Manager._Passer.EditQueue;

            // Get the handler to print them.
            new InstantPrintHandler(school, Manager._ProcessButtonSubsystem, matchesToPrint);
        }

        private void Refresh(object sender = null, EventArgs e = null)
        {
            CloseAll.RaiseCanExecuteChanged();
            CloseSelected.RaiseCanExecuteChanged();
            PrintSelected.RaiseCanExecuteChanged();
            EditMatchesCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Closes the matches in the match edit queue
        /// </summary>
        /// <param name="sender"></param>
        private async void CloseSelectedMatches(object sender)
        {
            // Convert MatchButtons to Match objects.
            List<Match> matchesToClose = Manager._Passer.EditQueue;

            // Close them.
            await Task.Run(() => school.CloseMatches(matchesToClose));

            SelectedMatchesClosed?.Invoke(this, new EventArgs());
        }

        private bool CanCloseAll()
        {
            return school.NumOpenMatches > 0;
        }

        private async void CloseAllMatches(object sender)
        {
            await Task.Run(school.CloseAllMatches);
        }
    }
}