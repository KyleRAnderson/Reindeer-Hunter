using System;
using System.Collections.Generic;
using System.Linq;
using Reindeer_Hunter.Data_Classes;
using System.Windows.Controls;
using Reindeer_Hunter.Subsystems;
using System.ComponentModel;
using System.Windows;
using System.Collections;
using Reindeer_Hunter.Hunt;

namespace Reindeer_Hunter.FFA
{
    /// <summary>
    /// Class to modify and retrieve data on victor students
    /// </summary>
    public class VictorHandler : INotifyPropertyChanged
    {
        public event EventHandler ParentPageSet;
        public event PropertyChangedEventHandler PropertyChanged;

        private AskStudentNameDialog dialog;

        // These two commands are for handling the dialog button when it is closed.
        public RelayCommand CancelButtonCommand { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };
        public RelayCommand SubmitButtonCommand { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };

        public RelayCommand SwitchToHomeScreen { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };
        public RelayCommand CalculateVictor { get; } = new RelayCommand();
                // Command for the Pin button
        public RelayCommand PinCommand { get; } = new RelayCommand();

        /// <summary>
        /// List of victors who haven't been pinned yet.
        /// </summary>
        private List<Victor> InVictors
        {
            get
            {
                List<Victor> returnable = new List<Victor>();
                foreach (Victor victor in Victors.Values) if (victor.In) returnable.Add(victor);

                return returnable;
            }
        }

        public DataFileIO DataHandler;
        private DataGrid MainDisplay;
        public School _School;

        public FreeForAll ParentPage { get; private set; }

        private Hashtable Data { get; set; }

        public Dictionary<int, Victor> Victors { get; private set; }

        private int NumStudentsLeft
        {
            get
            {
                int count = 0;
                foreach (Victor victor in Victors.Values)
                {
                    if (victor.In) count += 1;
                }
                return count;
            }
        }

        private bool AreWinnersSet
        {
            get
            {
                if (Data.ContainsKey(winnerDataLoc))
                {
                    return ((List<Victor>)Data[winnerDataLoc]).Count > 0;
                }
                else return false;
            }
        }

        // Location of some of the data in the Data Hashtable
        private string winnerDataLoc;
        private string victorDataLoc;

        public VictorHandler()
        {
            SwitchToHomeScreen.FunctionToExecute = SwitchToHome;
            CalculateVictor.FunctionToExecute = Calculate_Victor;
            CalculateVictor.CanExecuteDeterminer = Can_Calculate_Victor;

            // Subscribe to its own ParentPageSet event, to do stuff when parentpage is set
            ParentPageSet += OnParentPageSet;

            // Give the commands their functions to execute
            SubmitButtonCommand.FunctionToExecute = Submit;
            CancelButtonCommand.FunctionToExecute = Cancel;
            PinCommand.FunctionToExecute = Pin;
            PinCommand.CanExecuteDeterminer = CanBePinned;
        }

        /// <summary>
        /// Function that the calculate victor command uses to see if it can calculate the victor
        /// </summary>
        /// <returns>Bool true when the operation should be allowed, false otherwise</returns>
        public bool Can_Calculate_Victor()
        {
            if (DataHandler == null) return false;
            return !DataHandler.IsTerminated;
        }

        /// <summary>
        /// Function called when the ParentPageSet event is fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnParentPageSet(object sender, EventArgs e)
        {
            // Define the locations of the data
            winnerDataLoc = DataFileIO.winnerDataLoc;
            victorDataLoc = DataFileIO.victorDataLoc;

            // Do this to refresh the command
            CalculateVictor.RaiseCanExecuteChanged();

            // Set some properties so we can actually do stuff
            MainDisplay = ParentPage.VictorDisplay;

            // Get the victors
            Data = DataHandler.GetFFAData();

            // If there are no victors, create them.
            if (Data == null)
            {
                Data = new Hashtable
                {
                    { victorDataLoc, new Dictionary<int, Victor>() }
                };
                Victors = (Dictionary<int, Victor>)Data[victorDataLoc];

                // Loop around, converting students to victors
                List<Student> studentsList = _School.GetAllParticipatingStudents();
                foreach (Student student in studentsList)
                {
                    Victors.Add(student.Id, new Victor(student));
                }

                // Save newly created victors.
                Save();
            }
            else if (!Data.ContainsKey(winnerDataLoc))
            {
                // Set the Victors
                Victors = (Dictionary<int, Victor>)Data[victorDataLoc];
            }
            // Figure out if someone has won already
            else
            {
                // Set the Victors
                Victors = (Dictionary<int, Victor>)Data[victorDataLoc];

                /* If there is only one person, they won. If there are
                 * more than one, then it's time to prompt the user 
                 * for coint toss. */
                List<Victor> winner = (List<Victor>)Data[winnerDataLoc];
                if (winner.Count == 1) DisplayWinner(winner[0]);
                else
                {
                    Display_Coin_Toss(winner);
                }
            }

            // Refresh the display
            PropertyChanged(this, new PropertyChangedEventArgs("Victors"));

            // Subscribe to the double click and single click events from the MainDisplay
            MainDisplay.SelectedCellsChanged += SelectionChanged;
            MainDisplay.MouseDoubleClick += OnDoubleClick;
        }

        private void Display_Coin_Toss(List<Victor> possibleWinnners)
        {
            dialog = new AskStudentNameDialog(possibleWinnners, "Tie. Who won the coin toss?", CancelButtonCommand, SubmitButtonCommand);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Function to save the ties in the calculation of the victor to the data file.
        /// </summary>
        /// <param name="winners"></param>
        private void Set_Coin_Toss(List<Victor> winners)
        {
            Data.Add(winnerDataLoc, winners);

            Save();

            Display_Coin_Toss(winners);
        }

        /// <summary>
        /// Function that switches to the home page
        /// </summary>
        public void SwitchToHome(object parameter)
        {
            ((StartupWindow)ParentPage.Parent).GoToHome();
        }

        /// <summary>
        /// Simple function to set the parent page property of this object
        /// </summary>
        /// <param name="parent">The parent page (FreeForAll page)</param>
        public void SetParentPage(FreeForAll parent)
        {
            ParentPage = parent;
            ParentPageSet(this, new EventArgs());
        }

        /// <summary>
        /// Function that the pin command uses to verify if the currently selected victor can be pinned.
        /// </summary>
        /// <returns></returns>
        public bool CanBePinned()
        {
            // We'll get a null reference when nothing is selected, so it shouldn't be enabled.
            try
            {
                // Make sure they can't pin the last person
                if (NumStudentsLeft <= 1 || DataHandler.IsTerminated || AreWinnersSet) return false;
                Victor currentVictor = (Victor)MainDisplay.CurrentCell.Item;
                return currentVictor.In;
            }
            catch (NullReferenceException)
            {
                return false;
            }

        }

        /// <summary>
        /// Function that the pin command actually runs
        /// </summary>
        public void Pin(object parameter)
        {

            // If selection is invalid, return
            if (!MainDisplay.CurrentCell.IsValid) return;

            // Figure out which victor we're pinning
            Victor pinnedVictor = (Victor)MainDisplay.CurrentCell.Item;

            /* Copy the current victors list so we can remove this person from it.
             * The reason for removing them is because, if we send the list with the
             * victor who's getting pinned in it, the user could select the pinned victor
             * as the pinner. */
            List<Victor> victors = new List<Victor>(); 

            foreach (Victor victor in Victors.Values)
            {
                // If the person is out, or if they're the one being pinned, don't add them.
                if (victor.In && victor.Id != pinnedVictor.Id) victors.Add(victor); 
            }

            // Make the dialog
            dialog = new AskStudentNameDialog(victors, pinnedVictor, CancelButtonCommand, SubmitButtonCommand);

            // Show the dialog
            dialog.ShowDialog();
        }

        /// <summary>
        /// Function called whenever the MainDisplay's selection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectionChanged(object sender, EventArgs e)
        {
            // All we have to do is make sure the pin command's CanExecute is up to date.
            PinCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Function called whenever the dialog asking the user for the pinner of
        /// the pinned person and then the submit button is pressed.
        /// </summary>
        /// <param name="parameter"></param>
        public void Submit(object parameter)
        {
            /* Get the Student id of whatever victor did something 
             * (either won coin toss or pinned someone else) */
            int selectionindex = (int)parameter;
            int studentId = dialog.GetVictorIdByIndex(selectionindex);
            Victor actingVictor = Victors[studentId];

            // It's zero if we're asking for who won the coin toss.
            if (dialog.KilledStudentId != 0)
            {
                // Add to their kills and to the list of people whom they've killed.
                actingVictor.NumKills += 1;
                if (actingVictor.Kills == null) actingVictor.Kills = new List<int>();
                actingVictor.Kills.Add(dialog.KilledStudentId);

                // Toggle the status of the pinned victor
                Victors[dialog.KilledStudentId].In = false;

                dialog.Close();

                // Save changes
                Save();
            }
            else
            {
                // Close dialog
                dialog.Close();

                // Set the winner
                SetWinner(actingVictor);
            }

            // Refresh the GUI
            PropertyChanged(this, new PropertyChangedEventArgs("Victors"));
            MainDisplay.Items.Refresh();
        }

        /// <summary>
        /// Function called whenever the dialog asking the user for the pinner of
        /// the pinned person and then the cancel button is pressed.
        /// </summary>
        /// <param name="parameter"></param>
        public void Cancel(object parameter)
        {
            dialog.Close();
        }

        /// <summary>
        /// Function called whenever the user double clicks in the DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnDoubleClick(object sender, EventArgs e)
        {
            Victor selection = (Victor)MainDisplay.CurrentCell.Item;

            // Get the name of the people this person has killed.
            List<string> victorsKills = new List<string>();

            // It is null when it is empty
            if (selection.Kills != null)
            {
                foreach (int stuNo in selection.Kills)
                {
                    victorsKills.Add(Victors[stuNo].FullName);
                }
            }

            DataCard dataDialog = new DataCard(selection, victorsKills);
            dataDialog.ShowDialog();
        }

        /// <summary>
        /// Simple function to save what has been changed.
        /// Note that when the hunt is over, saving is blocked.
        /// </summary>
        public void Save()
        {
            // As a backup, make sure we don't save anything if the hunt is supposed to be over.
            if (DataHandler.IsTerminated) return;

            DataHandler.SaveVictors(Data);
        }

        /// <summary>
        /// Function called by the calculate victor button command. 
        /// </summary>
        public void Calculate_Victor(object parameter)
        {
            // If there are winner set already, make sure we don't overwrite it, or calculate it twice.
            if (AreWinnersSet)
            {
                Display_Coin_Toss((List<Victor>)Data[winnerDataLoc]);
                return;
            }

            // Make sure this is what the user wants, as it can't be undone.
            MessageBoxResult result = MessageBox.Show("Proceed with calculating the victor of this " +
                "Reindeer Hunt? This action CANNOT BE UNDONE.", "Proceed?", 
                MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result != MessageBoxResult.OK) return;

            // We only look at the victors that are still in
            List<Victor> possiblewinner = new List<Victor>(InVictors);

            // This will acutally have the list of victors who  got past the loop 
            List<Victor> moveOn = new List<Victor>();

            // Determine who has the highest number of kills
            int maxKills = 0;
            foreach (Victor victor in possiblewinner)
            {
                if (victor.NumKills > maxKills)
                {
                    moveOn.Clear();
                    moveOn.Add(victor);
                    maxKills = victor.NumKills;
                }

                // In case of ties, don't delete the others.
                else if (victor.NumKills == maxKills)
                {
                    moveOn.Add(victor);
                }
            }

            // That's it, the hunt is over.
            if (moveOn.Count() == 1) SetWinner(moveOn[0]);

            else
            {
                Set_Coin_Toss(moveOn);
            }
        }

        /// <summary>
        /// Function to be called when the winner is known, and it is time to
        /// do stuff to save and handle the info.
        /// </summary>
        private void SetWinner(Victor winner)
        {
            Data.Add(winnerDataLoc, new List<Victor>
            {
                {winner }
            });

            // Save the stuff before terminating hunt
            Save();

            // End the hunt. 
            DataHandler.IsTerminated = true;

            DisplayWinner(winner);
        }

        public void DisplayWinner(Victor winner)
        {
            string messageToShow = String.Format("The Winner of this Reindeer Hunt is {0}!", winner.FullName);
            MessageBox.Show(messageToShow, "AND THE WINNER IS...", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}