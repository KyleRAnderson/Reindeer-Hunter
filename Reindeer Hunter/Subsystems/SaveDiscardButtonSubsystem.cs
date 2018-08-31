using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Reindeer_Hunter.Subsystems
{
    public class SaveDiscardButtonSubsystem : Subsystem
    {
        // The commands that this subsystem is in charge of.
        public RelayCommand SaveCommand { get; private set; } = new RelayCommand();
        public RelayCommand DiscardCommand { get; } = new RelayCommand();

        // Called when save is pressed
        public event EventHandler Save;
        // Called when discard is pressed
        public event EventHandler Discard;

        /// <summary>
        /// True when we just made matches and they're pending getting saved
        /// </summary>
        public bool MatchAddPending { get; set; }

        public SaveDiscardButtonSubsystem() : base()
        {
            DiscardCommand.CanExecuteDeterminer = CanSave;
            SaveCommand.CanExecuteDeterminer = CanSave;
            DiscardCommand.FunctionToExecute = DiscardButtonPressed;
            SaveCommand.FunctionToExecute = SaveButtonPressed;
        }

        protected override void OnHomePageSet(object sender, EventArgs e)
        {
            base.OnHomePageSet(sender, e);

            // Subscribe to certain events
            Manager._ProcessButtonSubsystem.MatchesMade += WhenButtonsNeedUpdating;
            school.MatchChangeEvent += WhenButtonsNeedUpdating;
            Manager._Passer.ResultAdded += WhenButtonsNeedUpdating;
            Manager._Passer.ResultRemoved += WhenButtonsNeedUpdating;
        }

        /// <summary>
        /// Called by the save command
        /// </summary>
        public void SaveButtonPressed(object parameter)
        {
            Save(this, new EventArgs());
            WhenButtonsNeedUpdating();
        }

        /// <summary>
        /// Function which determines if the save and discard buttons should be enabled or not.
        /// </summary>
        /// <returns>True if they should be enabled, false otherwise.</returns>
        public bool CanSave()
        {
            return (Manager._Passer.IsPassingStudents ||
            Manager._ProcessButtonSubsystem.AreMatchesMade);
        }

        /// <summary>
        /// Called by the discard command.
        /// </summary>
        public void DiscardButtonPressed(object parameter)
        {
            Discard(this, new EventArgs());
            WhenButtonsNeedUpdating();
        }

        /// <summary>
        /// Called by certain key event handlers whenver we should re-evaluate
        /// the state of the buttons.
        /// Those events are: when matches are saved, when match results are added or removed, when matches are created.
        /// </summary>
        public void WhenButtonsNeedUpdating(object sender = null, EventArgs e = null)
        {
            SaveCommand.RaiseCanExecuteChanged();
            DiscardCommand.RaiseCanExecuteChanged();
        }
    }
}
