using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reindeer_Hunter.Data_Classes;
using System.Windows.Controls;
using Reindeer_Hunter.Subsystems;
using System.ComponentModel;
using Reindeer_Hunter.FFA;

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
        public RelayCommand CancelButtonClick { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };
        public RelayCommand SubmitButtonClick { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };

        public RelayCommand SwitchToHomeScreen { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };

        // Command for the Pin button
        public RelayCommand PinCommand { get; } = new RelayCommand();

        public DataFileIO DataHandler;
        private DataGrid MainDisplay;
        public School _School;
        
        public FreeForAll ParentPage { get; private set; }

        public Dictionary<int, Victor> Victors { get; private set; } = new Dictionary<int, Victor>();

        public VictorHandler()
        {
            SwitchToHomeScreen.FunctionToExecute = SwitchToHome;

            // Subscribe to its own ParentPageSet event, to do stuff when parentpage is set
            ParentPageSet += OnParentPageSet;

            // Give the commands their functions to execute
            SubmitButtonClick.FunctionToExecute = Submit;
            CancelButtonClick.FunctionToExecute = Cancel;
            PinCommand.FunctionToExecute = Pin;
            PinCommand.CanExecuteDeterminer = CanBePinned;
        }

        /// <summary>
        /// Function called when the ParentPageSet event is fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnParentPageSet(object sender, EventArgs e)
        {
            // Set some properties so we can actually do stuff
            MainDisplay = ParentPage.VictorDisplay;

            // Get the victors
            Victors = DataHandler.GetVictors();

            // If there are no victors, create them.
            if (Victors == null)
            {
                Victors = new Dictionary<int, Victor>();
                // Loop around, converting students to victors
                List<Student> studentsList = _School.GetAllParticipatingStudents();
                foreach (Student student in studentsList)
                {
                    Victors.Add(student.Id, new Victor(student));
                }

                // Save newly created victors.
                DataHandler.SaveVictors(Victors);
            }

            // Refresh the display
            PropertyChanged(this, new PropertyChangedEventArgs("Victors"));

            // Subscribe to the double click and single click events from the MainDisplay
            MainDisplay.SelectedCellsChanged += SelectionChanged;
            MainDisplay.MouseDoubleClick += OnDoubleClick;
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
            // Figure out which victor we're pinning
            Victor pinnedVictor = (Victor)MainDisplay.CurrentCell.Item;

            /* Copy the current victors list so we can remove this person from it.
             * The reason for removing them is because, if we send the list with the
             * victor who's getting pinned in it, the user could select the pinned victor
             * as the pinner. */
            List<Victor> victors = new List<Victor>(Victors.Values);
            // Remove the person being pinned from the list
            victors.Remove(pinnedVictor);

            // Make the dialog
            dialog = new AskStudentNameDialog(victors, pinnedVictor, CancelButtonClick, SubmitButtonClick);

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
            // Find the student Id and get the victor from it.
            int selectionindex = (int)parameter;
            int studentId = dialog.GetVictorIdByIndex(selectionindex);
            Victor killer = Victors[studentId];

            // Add to their kills and to the list of people whom they've killed.
            killer.NumKills += 1;
            if (killer.Kills == null) killer.Kills = new List<int>();
            killer.Kills.Add(dialog.KilledStudentId);

            // Toggle the status of the pinned victor
            Victors[dialog.KilledStudentId].In = false;

            dialog.Close();

            // Refresh the GUI
            PropertyChanged(this, new PropertyChangedEventArgs("Victors"));
            MainDisplay.Items.Refresh();
            
            // Save changes
            Save();
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
        /// </summary>
        public void Save()
        {
            DataHandler.SaveVictors(Victors);
        }
    }
}